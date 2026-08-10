// ============================================================
// D11PetSystem —— D11-A06 AnimalCareSystem owner
//
// Pet state is written here only. Inventory/Cargo remains the sole writer of
// feed item counts; Crew is read for structured reactions; EventLog records
// facts after this system has made the deterministic decision. No LLM, Unity
// or network dependency is allowed in this file.
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Agent;
using Whalebone.Game.Config;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    public sealed class D11PetSystem
    {
        public const string OwnerSystem = "AnimalCareSystem";
        public const string AdoptionEventId = "event_d11_pet_adopted";
        public const string CareEventId = "event_d11_pet_care";
        public const string DailyTickEventId = "event_d11_pet_daily_tick";
        public const string LocationEventId = "event_d11_pet_location";
        public const string LostEventId = "event_d11_pet_lost";
        public const string SearchEventId = "event_d11_pet_search";
        public const string QuarantineEventId = "event_d11_pet_quarantine";
        public const string MouseEventId = "event_d11_pet_mouse_caught";
        public const string WarningEventId = "event_d11_pet_warning";
        public const string CrewReactionEventId = "event_d11_pet_crew_reaction";

        private readonly D3TradeState _trade;
        private readonly List<CrewAgent> _crew = new List<CrewAgent>();
        private readonly EventLog _eventLog;
        private readonly D11PetState _state;
        private readonly List<D11PetCrewReaction> _crewReactions =
            new List<D11PetCrewReaction>();
        private long _nextRevision;

        public D11PetSystem(D3TradeState trade = null, IEnumerable<CrewAgent> crew = null,
            EventLog eventLog = null, D11PetRuntimeSnapshot initial = null)
        {
            _trade = trade;
            if (crew != null) _crew.AddRange(crew);
            _eventLog = eventLog ?? new EventLog();
            _state = initial == null || initial.Pet == null
                ? new D11PetState() : initial.Pet.Clone();
            if (initial != null)
            {
                _nextRevision = Math.Max(0L, initial.NextRevision);
                if (initial.CrewReactions != null)
                    foreach (var reaction in initial.CrewReactions)
                        if (reaction != null) _crewReactions.Add(reaction.Clone());
                if (initial.EventLog != null) _eventLog.Restore(initial.EventLog);
            }
            _nextRevision = Math.Max(_nextRevision, _eventLog.NextSequence);
            D3InventorySystem.RegisterCargoDefinition(D11PetIds.FeedItemId, 1, 1);
        }

        public D11PetState Snapshot { get { return _state.Clone(); } }
        public IReadOnlyList<D11PetCrewReaction> CrewReactions
        {
            get
            {
                var result = new List<D11PetCrewReaction>();
                foreach (var reaction in _crewReactions)
                    if (reaction != null) result.Add(reaction.Clone());
                return result;
            }
        }
        public EventLog EventLog { get { return _eventLog; } }

        public D11PetRuntimeSnapshot CaptureSnapshot()
        {
            var result = new D11PetRuntimeSnapshot
            {
                NextRevision = _nextRevision,
                Pet = _state.Clone(),
                EventLog = _eventLog.CaptureSnapshot(),
            };
            foreach (var reaction in _crewReactions)
                if (reaction != null) result.CrewReactions.Add(reaction.Clone());
            return result;
        }

        public void Restore(D11PetRuntimeSnapshot snapshot)
        {
            if (snapshot == null) return;
            snapshot.Validate();
            CopyState(snapshot.Pet);
            _crewReactions.Clear();
            if (snapshot.CrewReactions != null)
                foreach (var reaction in snapshot.CrewReactions)
                    if (reaction != null) _crewReactions.Add(reaction.Clone());
            if (snapshot.EventLog != null) _eventLog.Restore(snapshot.EventLog);
            _nextRevision = Math.Max(Math.Max(0L, snapshot.NextRevision), _eventLog.NextSequence);
        }

        public D11PetActionResult Adopt(string definitionId, string sourceRef, int worldDay,
            long saveRevision = 0)
        {
            var definition = D11PetCatalog.Find(definitionId);
            if (definition == null) return Reject("pet_unknown", "宠物定义不存在。");
            if (string.IsNullOrWhiteSpace(sourceRef)) return Reject("source_missing", "宠物来源记录不能为空。");
            if (_state.HasInstance && _state.LifeState != D11PetLifeState.Unadopted)
                return Reject(_state.LifeState == D11PetLifeState.Dead ? "pet_dead_no_respawn" : "pet_already_owned",
                    "已有宠物实例，不能无条件重新生成。");

            long revision = ResolveRevision(saveRevision);
            if (IsDuplicate(AdoptionEventId, revision)) return Duplicate(AdoptionEventId, revision);

            _state.PetId = BuildPetId(definition.Id, sourceRef);
            _state.DefinitionId = definition.Id;
            _state.Kind = definition.Kind;
            _state.Breed = definition.Breed;
            _state.SourceRef = sourceRef.Trim();
            _state.SourceTag = definition.SourceTag;
            _state.AdoptionDay = Math.Max(0, worldDay);
            _state.LastUpdatedDay = Math.Max(0, worldDay);
            _state.LostSinceDay = -1;
            _state.LostSaveRevision = 0;
            _state.QuarantineStartedDay = -1;
            _state.QuarantineDaysRemaining = 0;
            _state.Health = 100;
            _state.Hunger = 10;
            _state.Cleanliness = 90;
            _state.Fear = 20;
            _state.Attachment = 25;
            _state.LifeState = D11PetLifeState.Adopted;
            _state.Location = D11PetLocation.Harbor;
            _state.CareState = D11PetCareState.Content;
            _state.LastCauseEventId = AdoptionEventId;
            _state.LastSaveRevision = revision;
            var result = Append(AdoptionEventId, revision, worldDay, "pet_adopted",
                "宠物领养来源已记录，宠物状态由 AnimalCareSystem 建立。",
                new Dictionary<string, string>
                {
                    ["pet_id"] = _state.PetId,
                    ["definition_id"] = _state.DefinitionId,
                    ["source_ref"] = _state.SourceRef,
                    ["location"] = _state.Location.ToString(),
                });
            AddCrewReactions(result, "adoption", worldDay, revision);
            return result;
        }

        public D11PetActionResult Feed(int quantity, int worldDay, long saveRevision = 0)
        {
            if (!CanCare()) return Reject("pet_not_careable", "当前没有可照料的存活宠物。");
            if (quantity <= 0) return Reject("invalid_quantity", "喂食数量必须大于零。");
            var definition = D11PetCatalog.Find(_state.DefinitionId);
            if (definition == null) return Reject("pet_definition_missing", "宠物定义缺失。");
            string inventoryError = null;
            if (_trade == null || !D3InventorySystem.TryRemoveCargo(_trade.Inventory,
                    definition.FeedItemId, quantity, out inventoryError))
                return Reject(inventoryError ?? "feed_unavailable", "货舱中没有足够的宠物饲料。");

            long revision = ResolveRevision(saveRevision);
            if (IsDuplicate(CareEventId, revision))
            {
                D3InventorySystem.TryAddCargo(_trade.Inventory, definition.FeedItemId, quantity, out _);
                return Duplicate(CareEventId, revision);
            }
            _state.Hunger = ClampPercent(_state.Hunger - 18 * quantity);
            _state.Health = ClampHealth(_state.Health + Math.Min(8, 2 * quantity));
            _state.Attachment = ClampPercent(_state.Attachment + 2);
            _state.LastUpdatedDay = Math.Max(_state.LastUpdatedDay, Math.Max(0, worldDay));
            RecomputeCareState();
            _state.LastCauseEventId = CareEventId;
            _state.LastSaveRevision = revision;
            var result = Append(CareEventId, revision, worldDay, "pet_fed",
                "宠物饲料已由 Inventory/Cargo 扣除，照料状态由 AnimalCareSystem 更新。",
                new Dictionary<string, string>
                {
                    ["pet_id"] = _state.PetId,
                    ["care_action"] = "feed",
                    ["feed_item_id"] = definition.FeedItemId,
                    ["quantity"] = quantity.ToString(CultureInfo.InvariantCulture),
                });
            AddCrewReactions(result, "care", worldDay, revision);
            return result;
        }

        public D11PetActionResult Clean(int worldDay, long saveRevision = 0)
        {
            if (!CanCare()) return Reject("pet_not_careable", "当前没有可清洁的存活宠物。");
            return ApplyCare("clean", worldDay, saveRevision, delegate
            {
                _state.Cleanliness = 100;
                _state.Fear = ClampPercent(_state.Fear - 3);
            });
        }

        public D11PetActionResult Comfort(int worldDay, long saveRevision = 0)
        {
            if (!CanCare()) return Reject("pet_not_careable", "当前没有可安抚的存活宠物。");
            return ApplyCare("comfort", worldDay, saveRevision, delegate
            {
                _state.Fear = ClampPercent(_state.Fear - 18);
                _state.Attachment = ClampPercent(_state.Attachment + 8);
            });
        }

        /// <summary>宠物位置由 AnimalCareSystem 写入；UI 只能提交位置意图。</summary>
        public D11PetActionResult MoveTo(D11PetLocation location, int worldDay, long saveRevision = 0)
        {
            if (_state.LifeState != D11PetLifeState.Adopted)
                return Reject("pet_not_movable", "只有已领养且未检疫、未走失的宠物可以移动。");
            if (location != D11PetLocation.Harbor && location != D11PetLocation.ShipCabin &&
                location != D11PetLocation.ShipDeck)
                return Reject("pet_location_invalid", "宠物位置必须是港口、船舱或甲板。");
            long revision = ResolveRevision(saveRevision);
            if (IsDuplicate(LocationEventId, revision)) return Duplicate(LocationEventId, revision);
            _state.Location = location;
            _state.LastUpdatedDay = Math.Max(_state.LastUpdatedDay, Math.Max(0, worldDay));
            _state.LastCauseEventId = LocationEventId;
            _state.LastSaveRevision = revision;
            var result = Append(LocationEventId, revision, worldDay, "pet_location_changed",
                "宠物位置已由 AnimalCareSystem 更新；移动不改变经济、航行或战斗结果。",
                new Dictionary<string, string>
                {
                    ["pet_id"] = _state.PetId,
                    ["location"] = location.ToString(),
                });
            AddCrewReactions(result, "care", worldDay, revision);
            return result;
        }

        public D11PetActionResult Quarantine(int worldDay, long saveRevision = 0)
        {
            if (_state.LifeState != D11PetLifeState.Adopted)
                return Reject("pet_not_quarantineable", "只有在船上或港口的宠物可以检疫。");
            long revision = ResolveRevision(saveRevision);
            if (IsDuplicate(QuarantineEventId, revision)) return Duplicate(QuarantineEventId, revision);
            _state.LifeState = D11PetLifeState.Quarantined;
            _state.Location = D11PetLocation.Quarantine;
            _state.QuarantineStartedDay = Math.Max(0, worldDay);
            _state.QuarantineDaysRemaining = 1;
            _state.CareState = D11PetCareState.Sick;
            _state.Fear = ClampPercent(_state.Fear + 8);
            _state.LastUpdatedDay = Math.Max(_state.LastUpdatedDay, Math.Max(0, worldDay));
            _state.LastCauseEventId = QuarantineEventId;
            _state.LastSaveRevision = revision;
            var result = Append(QuarantineEventId, revision, worldDay, "pet_quarantined",
                "宠物已进入港口检疫，未改变 Crew 或经济结果。",
                new Dictionary<string, string>
                {
                    ["pet_id"] = _state.PetId,
                    ["location"] = _state.Location.ToString(),
                    ["quarantine_days"] = _state.QuarantineDaysRemaining.ToString(CultureInfo.InvariantCulture),
                });
            AddCrewReactions(result, "quarantine", worldDay, revision);
            return result;
        }

        public D11PetActionResult Treat(int worldDay, long saveRevision = 0)
        {
            if (_state.LifeState != D11PetLifeState.Quarantined)
                return Reject("pet_not_quarantined", "宠物当前不在检疫状态。");
            return ApplyCare("treat", worldDay, saveRevision, delegate
            {
                _state.Health = ClampHealth(_state.Health + 30);
                _state.Fear = ClampPercent(_state.Fear - 5);
                _state.CareState = D11PetCareState.Recovering;
            });
        }

        public D11PetActionResult ReleaseFromQuarantine(int worldDay, long saveRevision = 0)
        {
            if (_state.LifeState != D11PetLifeState.Quarantined)
                return Reject("pet_not_quarantined", "宠物当前不在检疫状态。");
            if (_state.QuarantineDaysRemaining > 0 || _state.Health < 70)
                return Reject("quarantine_incomplete", "检疫期或健康恢复尚未完成。");
            long revision = ResolveRevision(saveRevision);
            if (IsDuplicate(QuarantineEventId, revision)) return Duplicate(QuarantineEventId, revision);
            _state.LifeState = D11PetLifeState.Adopted;
            _state.Location = D11PetLocation.Harbor;
            _state.CareState = D11PetCareState.Recovering;
            _state.LastUpdatedDay = Math.Max(_state.LastUpdatedDay, Math.Max(0, worldDay));
            _state.LastCauseEventId = QuarantineEventId;
            _state.LastSaveRevision = revision;
            var result = Append(QuarantineEventId, revision, worldDay, "pet_quarantine_released",
                "宠物完成检疫并恢复到港口照料状态。",
                new Dictionary<string, string>
                {
                    ["pet_id"] = _state.PetId,
                    ["location"] = _state.Location.ToString(),
                });
            AddCrewReactions(result, "recovery", worldDay, revision);
            return result;
        }

        /// <summary>由 AnimalCareSystem 记录走失；该事实是 Interrupted，恢复必须走 Search。</summary>
        public D11PetActionResult MarkLost(string reason, int worldDay, long saveRevision = 0)
        {
            if (!CanCare()) return Reject("pet_not_lost", "当前没有可标记走失的宠物。");
            if (string.IsNullOrWhiteSpace(reason)) return Reject("loss_reason_missing", "走失原因不能为空。");
            long revision = ResolveRevision(saveRevision);
            if (IsDuplicate(LostEventId, revision)) return Duplicate(LostEventId, revision);
            _state.LifeState = D11PetLifeState.Lost;
            _state.Location = D11PetLocation.Lost;
            _state.LostSinceDay = Math.Max(0, worldDay);
            _state.LostSaveRevision = revision;
            _state.LastUpdatedDay = Math.Max(_state.LastUpdatedDay, Math.Max(0, worldDay));
            _state.LastCauseEventId = LostEventId;
            _state.LastSaveRevision = revision;
            _state.Fear = ClampPercent(_state.Fear + 20);
            var result = Append(LostEventId, revision, worldDay, "pet_lost",
                "宠物走失事实已记录，恢复路线为搜索或收容所联络。",
                new Dictionary<string, string>
                {
                    ["pet_id"] = _state.PetId,
                    ["reason"] = reason.Trim(),
                    ["recovery_route_id"] = "pet_search_or_shelter",
                }, WorldEventStatus.Interrupted, new RecoveryRoute
                {
                    RouteId = "pet_search_or_shelter",
                    Action = "search_pet",
                    ResumeFromEventId = LostEventId,
                    Description = "通过确定性搜索恢复；未找到时保留原宠物实例和走失状态。",
                });
            AddCrewReactions(result, "lost", worldDay, revision);
            return result;
        }

        public D11PetActionResult Search(string searchLocationRef, bool hasSearchKit, int worldDay,
            int seed, long saveRevision = 0)
        {
            if (_state.LifeState != D11PetLifeState.Lost)
                return Reject("pet_not_lost", "当前没有待寻找的走失宠物。");
            if (!hasSearchKit) return Reject("search_kit_missing", "寻找宠物需要登记的寻找工具或收容所协助。");
            if (string.IsNullOrWhiteSpace(searchLocationRef))
                return Reject("search_location_missing", "搜索地点不能为空。");
            long revision = ResolveRevision(saveRevision);
            long lostRevision = _state.LostSaveRevision;
            if (IsDuplicate(SearchEventId, revision)) return Duplicate(SearchEventId, revision);
            bool found = Stable(_state.PetId + "|" + searchLocationRef.Trim() + "|" +
                Math.Max(0, worldDay) + "|" + seed) % 100 < 55;
            if (!found)
            {
                var failed = Append(SearchEventId, revision, worldDay, "pet_search_not_found",
                    "本次搜索没有找到宠物；原宠物实例仍保留为走失。",
                    new Dictionary<string, string>
                    {
                        ["pet_id"] = _state.PetId,
                        ["search_location_ref"] = searchLocationRef.Trim(),
                        ["search_result"] = "not_found",
                    });
                failed.SearchFound = false;
                AddCrewReactions(failed, "search_failed", worldDay, revision);
                return failed;
            }

            _state.LifeState = D11PetLifeState.Adopted;
            _state.Location = D11PetLocation.Harbor;
            _state.Fear = ClampPercent(_state.Fear - 12);
            _state.Health = Math.Max(1, _state.Health);
            _state.LostSinceDay = -1;
            _state.LostSaveRevision = 0;
            _state.LastUpdatedDay = Math.Max(_state.LastUpdatedDay, Math.Max(0, worldDay));
            _state.LastCauseEventId = SearchEventId;
            _state.LastSaveRevision = revision;
            var recoveredEvent = new WorldEvent(
                SearchEventId, Math.Max(0, worldDay), "harbor:search", OwnerSystem,
                new EventResult
                {
                    ResultCode = "pet_recovered",
                    Summary = "走失宠物已通过确定性搜索恢复；没有生成新的宠物实例。",
                    FactIds = new List<string> { _state.PetId },
                    Values = new Dictionary<string, string>
                    {
                        ["pet_id"] = _state.PetId,
                        ["search_location_ref"] = searchLocationRef.Trim(),
                        ["search_result"] = "found",
                    },
                }, new[] { _state.PetId }, new[] { LostEventId }, new[] { LostEventId },
                revision, WorldEventStatus.Recovered);
            var append = _eventLog.AppendRecovery(LostEventId, lostRevision, recoveredEvent);
            var result = ToResult(append, SearchEventId, revision, "pet_recovered", true);
            result.SearchFound = true;
            AddCrewReactions(result, "recovered", worldDay, revision);
            return result;
        }

        public D11PetActionResult CatchMouse(int worldDay, int seed, long saveRevision = 0)
        {
            var definition = D11PetCatalog.Find(_state.DefinitionId);
            if (!CanCare()) return Reject("pet_not_careable", "当前没有可执行捕鼠的存活宠物。");
            if (definition == null || !definition.CanCatchMice)
                return Reject("pet_not_mouse_catcher", "该宠物不能执行船上捕鼠行为。");
            long revision = ResolveRevision(saveRevision);
            if (IsDuplicate(MouseEventId, revision)) return Duplicate(MouseEventId, revision);
            bool caught = Stable(_state.PetId + "|mouse|" + Math.Max(0, worldDay) + "|" + seed) % 100 < 70;
            if (caught) _state.MouseCaughtCount++;
            _state.LastUpdatedDay = Math.Max(_state.LastUpdatedDay, Math.Max(0, worldDay));
            _state.LastCauseEventId = MouseEventId;
            _state.LastSaveRevision = revision;
            var result = Append(MouseEventId, revision, worldDay,
                caught ? "pet_mouse_caught" : "pet_mouse_missed",
                caught ? "宠物完成非战斗捕鼠，船舱鼠患记录已更新。" : "宠物巡查了船舱，但本次没有捕到老鼠。",
                new Dictionary<string, string>
                {
                    ["pet_id"] = _state.PetId,
                    ["action"] = "mouse_control",
                    ["caught"] = caught.ToString().ToLowerInvariant(),
                });
            result.MouseCaught = caught;
            AddCrewReactions(result, caught ? "mouse_caught" : "mouse_missed", worldDay, revision);
            return result;
        }

        /// <summary>预警只产生观察事实，不修改战斗、天气或航行结果。</summary>
        public D11PetActionResult Warn(int worldDay, int seed, long saveRevision = 0)
        {
            var definition = D11PetCatalog.Find(_state.DefinitionId);
            if (!CanCare()) return Reject("pet_not_careable", "当前没有可预警的存活宠物。");
            if (definition == null || !definition.CanWarn)
                return Reject("pet_cannot_warn", "该宠物没有可用的预警行为。");
            long revision = ResolveRevision(saveRevision);
            if (IsDuplicate(WarningEventId, revision)) return Duplicate(WarningEventId, revision);
            bool warning = Stable(_state.PetId + "|warning|" + Math.Max(0, worldDay) + "|" + seed) % 100 < 45;
            if (warning) _state.WarningCount++;
            _state.LastUpdatedDay = Math.Max(_state.LastUpdatedDay, Math.Max(0, worldDay));
            _state.LastCauseEventId = WarningEventId;
            _state.LastSaveRevision = revision;
            var result = Append(WarningEventId, revision, worldDay,
                warning ? "pet_warning_raised" : "pet_warning_none",
                warning ? "宠物发出预警；危险结果仍由天气/航行/战斗系统裁决。" : "宠物没有观察到需要预警的异常。",
                new Dictionary<string, string>
                {
                    ["pet_id"] = _state.PetId,
                    ["warning"] = warning.ToString().ToLowerInvariant(),
                    ["observation_only"] = "true",
                });
            result.WarningRaised = warning;
            AddCrewReactions(result, warning ? "warning" : "warning_none", worldDay, revision);
            return result;
        }

        /// <summary>每日状态推进；不生成宠物，不把死亡恢复成活体。</summary>
        public D11PetActionResult AdvanceDay(int worldDay, long saveRevision = 0)
        {
            if (!_state.HasInstance) return Reject("pet_missing", "当前没有宠物实例。");
            int day = Math.Max(0, worldDay);
            if (day < _state.LastUpdatedDay) return Reject("pet_day_rewind", "宠物状态日期不能倒退。");
            long revision = ResolveRevision(saveRevision);
            if (IsDuplicate(DailyTickEventId, revision)) return Duplicate(DailyTickEventId, revision);
            int days = day - _state.LastUpdatedDay;
            for (int i = 0; i < days; i++) AdvanceOneDay();
            _state.LastUpdatedDay = day;
            if (_state.LifeState == D11PetLifeState.Quarantined && _state.QuarantineDaysRemaining > 0)
                _state.QuarantineDaysRemaining = Math.Max(0, _state.QuarantineDaysRemaining - days);
            _state.LastCauseEventId = DailyTickEventId;
            _state.LastSaveRevision = revision;
            var result = Append(DailyTickEventId, revision, day,
                _state.LifeState == D11PetLifeState.Dead ? "pet_died" : "pet_daily_state_updated",
                _state.LifeState == D11PetLifeState.Dead
                    ? "宠物健康降至零，死亡事实已保存，不能无条件重生。"
                    : "宠物每日饥饿、清洁、恐惧、依恋和健康已按确定性规则更新。",
                new Dictionary<string, string>
                {
                    ["pet_id"] = _state.PetId,
                    ["days_applied"] = days.ToString(CultureInfo.InvariantCulture),
                    ["life_state"] = _state.LifeState.ToString(),
                    ["care_state"] = _state.CareState.ToString(),
                });
            AddCrewReactions(result, _state.LifeState == D11PetLifeState.Dead ? "dead" : "daily", day, revision);
            return result;
        }

        private D11PetActionResult ApplyCare(string action, int worldDay, long saveRevision,
            Action mutation)
        {
            long revision = ResolveRevision(saveRevision);
            if (IsDuplicate(CareEventId, revision)) return Duplicate(CareEventId, revision);
            mutation();
            _state.LastUpdatedDay = Math.Max(_state.LastUpdatedDay, Math.Max(0, worldDay));
            RecomputeCareState();
            _state.LastCauseEventId = CareEventId;
            _state.LastSaveRevision = revision;
            var result = Append(CareEventId, revision, worldDay, "pet_care_applied",
                "宠物照料已由 AnimalCareSystem 结算。",
                new Dictionary<string, string>
                {
                    ["pet_id"] = _state.PetId,
                    ["care_action"] = action,
                });
            AddCrewReactions(result, "care", worldDay, revision);
            return result;
        }

        private void AdvanceOneDay()
        {
            if (_state.LifeState == D11PetLifeState.Dead || _state.LifeState == D11PetLifeState.Sheltered)
                return;
            _state.Hunger = ClampPercent(_state.Hunger + (_state.LifeState == D11PetLifeState.Lost ? 12 : 8));
            _state.Cleanliness = ClampPercent(_state.Cleanliness - 7);
            if (_state.LifeState == D11PetLifeState.Lost)
            {
                _state.Fear = ClampPercent(_state.Fear + 5);
                _state.Health = ClampHealth(_state.Health - 3);
            }
            else
            {
                _state.Fear = ClampPercent(_state.Fear - 3);
                _state.Attachment = ClampPercent(_state.Attachment + 1);
            }
            if (_state.Hunger >= 70) _state.Health = ClampHealth(_state.Health - 4);
            if (_state.Cleanliness <= 30) _state.Health = ClampHealth(_state.Health - 3);
            if (_state.Fear >= 75) _state.Health = ClampHealth(_state.Health - 2);
            if (_state.Health <= 0)
            {
                _state.Health = 0;
                _state.LifeState = D11PetLifeState.Dead;
                _state.Location = D11PetLocation.Shelter;
                _state.CareState = D11PetCareState.Sick;
            }
            else RecomputeCareState();
        }

        private void RecomputeCareState()
        {
            if (_state.LifeState == D11PetLifeState.Quarantined)
            {
                _state.CareState = _state.Health < 70 ? D11PetCareState.Sick : D11PetCareState.Recovering;
                return;
            }
            if (_state.Health < 50) _state.CareState = D11PetCareState.Sick;
            else if (_state.Hunger >= 70) _state.CareState = D11PetCareState.Hungry;
            else if (_state.Cleanliness <= 30) _state.CareState = D11PetCareState.Dirty;
            else if (_state.Fear >= 70) _state.CareState = D11PetCareState.Afraid;
            else _state.CareState = D11PetCareState.Content;
        }

        private D11PetActionResult Append(string eventId, long revision, int worldDay,
            string resultCode, string summary, Dictionary<string, string> values,
            WorldEventStatus status = WorldEventStatus.Normal, RecoveryRoute recovery = null)
        {
            var append = _eventLog.Append(new WorldEvent(
                eventId, Math.Max(0, worldDay), LocationRef(), OwnerSystem,
                new EventResult
                {
                    ResultCode = resultCode,
                    Summary = summary,
                    FactIds = new List<string> { _state.PetId ?? "pet:none" },
                    Values = values ?? new Dictionary<string, string>(),
                }, new[] { _state.PetId ?? "pet:none" },
                new[] { "pet:" + (_state.PetId ?? "none") }, new string[0], revision,
                status, recovery));
            _nextRevision = Math.Max(_nextRevision, revision);
            return ToResult(append, eventId, revision, resultCode, true);
        }

        private D11PetActionResult ToResult(EventAppendResult append, string eventId,
            long revision, string code, bool applied)
        {
            var result = new D11PetActionResult
            {
                Succeeded = append != null && append.Succeeded,
                Applied = applied && append != null && !append.IsDuplicate,
                Duplicate = append != null && append.IsDuplicate,
                Code = append != null && append.IsDuplicate ? "duplicate_settlement" : code,
                Message = append != null && append.IsDuplicate ? "宠物事实已经结算，未重复修改状态。" : code,
                EventId = eventId,
                SaveRevision = revision,
                State = _state.Clone(),
                Fact = append == null || append.Fact == null ? null : append.Fact.CloneFact(),
            };
            return result;
        }

        private void AddCrewReactions(D11PetActionResult result, string action, int worldDay, long causeRevision)
        {
            if (result == null || result.Fact == null || _crew.Count == 0) return;
            int index = 0;
            foreach (var crew in _crew)
            {
                if (crew == null || string.IsNullOrWhiteSpace(crew.Id)) continue;
                string role = crew.Persona == null ? string.Empty : crew.Persona.Role ?? string.Empty;
                string reaction = ReactionFor(action, role);
                long reactionRevision = causeRevision > 0 ? causeRevision * 1000L + (++index) : ResolveRevision(0);
                if (IsDuplicate(CrewReactionEventId, reactionRevision)) continue;
                var model = new D11PetCrewReaction
                {
                    CrewId = crew.Id,
                    CrewRole = role,
                    PetId = _state.PetId,
                    ReactionId = reaction,
                    ExpressionKey = "pet.crew_reaction." + reaction,
                    CauseEventId = result.EventId,
                    WorldDay = Math.Max(0, worldDay),
                    SaveRevision = reactionRevision,
                };
                _crewReactions.Add(model);
                var fact = _eventLog.Append(new WorldEvent(
                    CrewReactionEventId, Math.Max(0, worldDay), LocationRef(), OwnerSystem,
                    new EventResult
                    {
                        ResultCode = "pet_crew_reaction_recorded",
                        Summary = "船员对宠物事实的结构化反应已记录。",
                        FactIds = new List<string> { _state.PetId, crew.Id },
                        Values = new Dictionary<string, string>
                        {
                            ["crew_id"] = crew.Id,
                            ["crew_role"] = role,
                            ["reaction_id"] = reaction,
                            ["expression_key"] = model.ExpressionKey,
                        },
                    }, new[] { crew.Id, _state.PetId }, new[] { result.EventId },
                    new[] { result.EventId }, reactionRevision));
                if (fact != null && fact.Fact != null)
                {
                    model.SaveRevision = reactionRevision;
                    result.CrewReactions.Add(model.Clone());
                }
            }
        }

        private bool CanCare()
        {
            return _state.HasInstance && (_state.LifeState == D11PetLifeState.Adopted ||
                _state.LifeState == D11PetLifeState.Quarantined);
        }

        private bool IsDuplicate(string eventId, long revision)
        {
            return _eventLog.Find(eventId, revision) != null;
        }

        private long ResolveRevision(long requested)
        {
            if (requested > 0)
            {
                _nextRevision = Math.Max(_nextRevision, requested);
                return requested;
            }
            return ++_nextRevision;
        }

        private D11PetActionResult Duplicate(string eventId, long revision)
        {
            return new D11PetActionResult
            {
                Succeeded = true,
                Applied = false,
                Duplicate = true,
                Code = "duplicate_settlement",
                Message = "宠物事实已经结算，未重复修改状态。",
                EventId = eventId,
                SaveRevision = revision,
                State = _state.Clone(),
                Fact = _eventLog.Find(eventId, revision),
            };
        }

        private D11PetActionResult Reject(string code, string message)
        {
            return D11PetActionResult.Reject(code, message, _state);
        }

        private string LocationRef()
        {
            return "pet:" + (_state.PetId ?? "none") + ":" + _state.Location.ToString().ToLowerInvariant();
        }

        private void CopyState(D11PetState source)
        {
            var value = source == null ? new D11PetState() : source.Clone();
            _state.PetId = value.PetId;
            _state.DefinitionId = value.DefinitionId;
            _state.Kind = value.Kind;
            _state.Breed = value.Breed;
            _state.SourceRef = value.SourceRef;
            _state.SourceTag = value.SourceTag;
            _state.AdoptionDay = value.AdoptionDay;
            _state.LastUpdatedDay = value.LastUpdatedDay;
            _state.LostSinceDay = value.LostSinceDay;
            _state.LostSaveRevision = Math.Max(0L, value.LostSaveRevision);
            _state.QuarantineStartedDay = value.QuarantineStartedDay;
            _state.QuarantineDaysRemaining = value.QuarantineDaysRemaining;
            _state.Health = ClampHealth(value.Health);
            _state.Hunger = ClampPercent(value.Hunger);
            _state.Cleanliness = ClampPercent(value.Cleanliness);
            _state.Fear = ClampPercent(value.Fear);
            _state.Attachment = ClampPercent(value.Attachment);
            _state.LifeState = value.LifeState;
            _state.Location = value.Location;
            _state.CareState = value.CareState;
            _state.MouseCaughtCount = Math.Max(0, value.MouseCaughtCount);
            _state.WarningCount = Math.Max(0, value.WarningCount);
            _state.LastCauseEventId = value.LastCauseEventId ?? string.Empty;
            _state.LastSaveRevision = Math.Max(0L, value.LastSaveRevision);
        }

        private static string BuildPetId(string definitionId, string sourceRef)
        {
            return "pet_instance_" + definitionId + "_" + Stable(sourceRef).ToString(CultureInfo.InvariantCulture);
        }

        private static string ReactionFor(string action, string role)
        {
            if (action == "lost") return "loss_concern";
            if (action == "warning") return "warning_acknowledged";
            if (action == "dead") return "mourning";
            if (action == "recovered") return "relief";
            if (action == "mouse_caught") return "mouse_control_approved";
            if (action == "quarantine") return "quarantine_support";
            if (action == "adoption") return "adoption_welcome";
            if (role.IndexOf("厨", StringComparison.Ordinal) >= 0) return "care_practical_support";
            return "care_acknowledged";
        }

        private static int ClampHealth(int value)
        {
            return (int)NumericConfiguration.ClampHealth(value);
        }

        private static int ClampPercent(int value)
        {
            return Math.Max(0, Math.Min(100, value));
        }

        private static int Stable(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash & 0x7fffffff);
            }
        }
    }

}
