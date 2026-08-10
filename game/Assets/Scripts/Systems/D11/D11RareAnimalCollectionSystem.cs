using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    /// <summary>
    /// D11-A05 collection/codex owner. It never creates wild animals and never writes
    /// EcologySystem animal state directly; every lifecycle/death decision is delegated.
    /// </summary>
    public sealed class D11RareAnimalCollectionSystem
    {
        private const string Owner = "CollectionSystem";
        private readonly D4EcologySystem _ecology;
        private readonly EventLog _eventLog;
        private readonly D4FactionSystem _factions;
        private readonly D11RareAnimalCollectionState _state;

        public D11RareAnimalCollectionSystem(D4EcologySystem ecology, EventLog eventLog = null,
            D4FactionSystem factions = null, D11RareAnimalCollectionState initial = null)
        {
            if (ecology == null) throw new ArgumentNullException("ecology");
            _ecology = ecology;
            _eventLog = eventLog;
            _factions = factions ?? new D4FactionSystem();
            _state = initial == null ? new D11RareAnimalCollectionState() : initial.Clone();
            InitializeCatalogState();
        }

        public D11RareAnimalCollectionState Snapshot { get { return _state.Clone(); } }
        public FactionState FactionSnapshot { get { return _factions.Snapshot; } }
        public IReadOnlyList<D11RareAnimalDefinition> Catalog { get { return D11RareAnimalCatalog.All; } }

        public D11RareAnimalResult Discover(D11DiscoveryRequest request)
        {
            var input = ValidateInput(request, D11RareAnimalAction.Discover);
            if (input != null) return input;
            var definition = D11RareAnimalCatalog.Find(request.SpeciesId);
            if (definition == null) return Reject("species_missing", "正式稀有动物定义不存在。", request);
            string eventId = EventId("discover", request, definition);
            var duplicate = Duplicate(eventId, request.SaveRevision, request, definition);
            if (duplicate != null) return duplicate;
            var decision = _ecology.DiscoverRareAnimal(definition.RuntimeAnimalId, definition.HabitatId,
                request.LocationRef, definition.SourceRef, request.Season, request.WorldDay);
            if (!decision.Accepted) return FromDecision(decision, definition, request, eventId);
            return ApplyEvidenceAction(eventId, request.SaveRevision, request, definition,
                D11EvidenceKind.Observation, "discovered", "animal_discovered", decision,
                "已发现现存稀有动物，尚未给予收藏或奖励。");
        }

        public D11RareAnimalResult Identify(D11IdentificationRequest request)
        {
            var input = ValidateInput(request, D11RareAnimalAction.Identify);
            if (input != null) return input;
            var definition = D11RareAnimalCatalog.Find(request.SpeciesId);
            if (definition == null) return Reject("species_missing", "正式稀有动物定义不存在。", request);
            string eventId = EventId("identify", request, definition);
            var duplicate = Duplicate(eventId, request.SaveRevision, request, definition);
            if (duplicate != null) return duplicate;
            var decision = _ecology.IdentifyRareAnimal(definition.RuntimeAnimalId, request.WorldDay);
            if (!decision.Accepted) return FromDecision(decision, definition, request, eventId);
            return ApplyEvidenceAction(eventId, request.SaveRevision, request, definition,
                D11EvidenceKind.Identification, "identified", "animal_identified", decision,
                "已识别物种；图鉴记录仍保留首次发现地点。");
        }

        public D11RareAnimalResult Observe(D11ObservationRequest request)
        {
            var input = ValidateInput(request, D11RareAnimalAction.Observe);
            if (input != null) return input;
            var definition = D11RareAnimalCatalog.Find(request.SpeciesId);
            if (definition == null) return Reject("species_missing", "正式稀有动物定义不存在。", request);
            string eventId = EventId("observe", request, definition);
            var duplicate = Duplicate(eventId, request.SaveRevision, request, definition);
            if (duplicate != null) return duplicate;
            var decision = _ecology.ObserveRareAnimal(definition.RuntimeAnimalId, request.Weather,
                request.WorldDay);
            if (!decision.Accepted) return FromDecision(decision, definition, request, eventId);
            return ApplyEvidenceAction(eventId, request.SaveRevision, request, definition,
                D11EvidenceKind.Observation, "observed", "animal_observed", decision,
                "已记录动物首次地点、季节、行为和观察证据。");
        }

        public D11RareAnimalResult Rescue(D11RescueRequest request)
        {
            var input = ValidateInput(request, D11RareAnimalAction.Rescue);
            if (input != null) return input;
            var definition = D11RareAnimalCatalog.Find(request.SpeciesId);
            if (definition == null) return Reject("species_missing", "正式稀有动物定义不存在。", request);
            string eventId = EventId("rescue", request, definition);
            var duplicate = Duplicate(eventId, request.SaveRevision, request, definition);
            if (duplicate != null) return duplicate;
            var area = FindAreaFor(request.LocationRef);
            if (area == null || !area.Active || !area.RescueAllowed)
                return Reject("rescue_area_forbidden", "当前地点不是允许救助的保护区。", request);
            var decision = _ecology.RescueRareAnimal(definition.RuntimeAnimalId, request.LocationRef,
                request.WorldDay);
            if (!decision.Accepted) return FromDecision(decision, definition, request, eventId);
            return ApplyEvidenceAction(eventId, request.SaveRevision, request, definition,
                D11EvidenceKind.Rescue, "rescued", "animal_rescued", decision,
                "已救助动物；后续转移或释放仍需合法证据。");
        }

        public D11RareAnimalResult StartLegalTransfer(D11LegalTransferRequest request)
        {
            var input = ValidateInput(request, D11RareAnimalAction.LegalTransfer);
            if (input != null) return input;
            var definition = D11RareAnimalCatalog.Find(request.SpeciesId);
            if (definition == null) return Reject("species_missing", "正式稀有动物定义不存在。", request);
            var collector = FindCollector(request.CollectorId);
            if (collector == null || !Contains(collector.AllowedSpeciesIds, definition.SpeciesId))
                return Reject("collector_forbidden", "该收藏家没有接收此物种的合法资格。", request);
            if (string.IsNullOrWhiteSpace(request.DestinationRef))
                return Reject("destination_missing", "合法转移缺少目的地。", request);
            if (_factions.GetReputation("england") < collector.ReputationGate)
                return Reject("reputation_gate", "当前势力声望不足以执行合法转移。", request);
            var book = FindBook(definition.SpeciesId);
            if (book == null || !book.Identified || !book.Rescued)
                return Reject("evidence_chain_incomplete", "合法转移必须已有识别、救助和完整证据链。", request);
            string eventId = EventId("transfer", request, definition);
            var duplicate = Duplicate(eventId, request.SaveRevision, request, definition);
            if (duplicate != null) return duplicate;
            var decision = _ecology.BeginRareAnimalTransport(definition.RuntimeAnimalId,
                request.DestinationRef, request.WorldDay);
            if (!decision.Accepted) return FromDecision(decision, definition, request, eventId);
            int days = Math.Max(1, Math.Min(7, request.DaysInTransit));
            string transportId = "transport_" + definition.SpeciesId + "_" + Math.Max(0, request.WorldDay) + "_" + _state.Revision;
            var transport = new D11AnimalTransportState
            {
                TransportId = transportId,
                AnimalId = definition.RuntimeAnimalId,
                SpeciesId = definition.SpeciesId,
                FromLocationRef = request.LocationRef,
                ToLocationRef = request.DestinationRef,
                StartWorldDay = Math.Max(0, request.WorldDay),
                DaysInTransit = days,
                Seed = request.Seed,
                Status = D11TransportStatus.InTransit,
                LastUpdatedDay = Math.Max(0, request.WorldDay),
            };
            _state.Transports.Add(transport);
            var result = ApplyEvidenceAction(eventId, request.SaveRevision, request, definition,
                D11EvidenceKind.Transfer, "legal_transfer", "animal_transfer_started", decision,
                "已建立合法转移记录，运输死亡率由 EcologySystem 逐日判定。");
            result.TransportId = transportId;
            return result;
        }

        public D11RareAnimalResult AdvanceTransport(D11TransportAdvanceRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TransportId))
                return new D11RareAnimalResult { Code = "transport_missing", Message = "运输记录不能为空。" };
            var transport = FindTransport(request.TransportId);
            if (transport == null) return new D11RareAnimalResult { Code = "transport_missing", Message = "运输记录不存在。" };
            var definition = D11RareAnimalCatalog.Find(transport.SpeciesId);
            string eventId = string.IsNullOrWhiteSpace(request.ActionId)
                ? "event_d11_transport_" + transport.TransportId + "_day_" + Math.Max(0, request.WorldDay)
                : request.ActionId;
            var duplicate = Duplicate(eventId, request.SaveRevision, null, definition);
            if (duplicate != null) return duplicate;
            if (transport.Status != D11TransportStatus.InTransit)
                return RejectTransport("transport_not_active", "运输已结束，不能重复推进。", transport, eventId, request.WorldDay);
            var decision = _ecology.AdvanceRareAnimalTransport(transport.AnimalId, request.WorldDay,
                1, transport.Seed, request.Weather);
            if (!decision.Accepted)
                return new D11RareAnimalResult { Code = decision.Code, Message = decision.Message,
                    AnimalId = transport.AnimalId, SpeciesId = transport.SpeciesId, TransportId = transport.TransportId,
                    EventId = eventId, WorldDay = request.WorldDay };
            transport.DaysElapsed++;
            transport.LastUpdatedDay = Math.Max(0, request.WorldDay);
            transport.MortalityRisk = decision.MortalityRisk;
            if (decision.Died) transport.Status = D11TransportStatus.Died;
            else if (transport.DaysElapsed >= transport.DaysInTransit) transport.Status = D11TransportStatus.Arrived;
            string code = decision.Died ? "transport_mortality" :
                transport.Status == D11TransportStatus.Arrived ? "transport_arrived" : "transport_survived";
            var result = CommitEvent(eventId, request.SaveRevision, request.WorldDay, transport.SpeciesId,
                transport.AnimalId, code, decision.Message, new[] { "transport:" + transport.TransportId },
                new Dictionary<string, string>
                {
                    ["transport_id"] = transport.TransportId,
                    ["days_elapsed"] = transport.DaysElapsed.ToString(CultureInfo.InvariantCulture),
                    ["mortality_risk"] = decision.MortalityRisk.ToString("0.###", CultureInfo.InvariantCulture),
                    ["animal_lifecycle"] = decision.Lifecycle.ToString(),
                }, request.SaveRevision);
            result.Mortality = decision.Died;
            result.MortalityRisk = decision.MortalityRisk;
            result.TransportId = transport.TransportId;
            result.Lifecycle = decision.Lifecycle;
            result.Succeeded = true;
            result.Applied = true;
            var book = FindBook(transport.SpeciesId);
            if (book != null)
            {
                book.CurrentStatus = decision.Died ? "dead_in_transport" :
                    transport.Status == D11TransportStatus.Arrived ? "arrived" : "in_transit";
                book.Dead = decision.Died;
                book.Handling = decision.Died ? "transport_mortality" : "legal_transfer";
                book.Revision = ++_state.Revision;
                result.BookEntry = book.Clone();
            }
            _state.LastEventId = eventId;
            Remember(eventId, request.SaveRevision);
            return result;
        }

        public D11RareAnimalResult Release(D11ReleaseRequest request)
        {
            var input = ValidateInput(request, D11RareAnimalAction.Release);
            if (input != null) return input;
            var definition = D11RareAnimalCatalog.Find(request.SpeciesId);
            if (definition == null) return Reject("species_missing", "正式稀有动物定义不存在。", request);
            var area = FindArea(request.ProtectedAreaId);
            if (area == null || !area.Active || !area.ReleaseAllowed ||
                area.LocationRef != request.LocationRef)
                return Reject("release_area_forbidden", "释放地点不在有效保护区内。", request);
            var transport = string.IsNullOrWhiteSpace(request.TransportId) ? null : FindTransport(request.TransportId);
            if (transport != null && transport.Status != D11TransportStatus.Arrived)
                return Reject("transport_not_arrived", "运输尚未安全抵达，不能释放。", request);
            string eventId = EventId("release", request, definition);
            var duplicate = Duplicate(eventId, request.SaveRevision, request, definition);
            if (duplicate != null) return duplicate;
            var decision = _ecology.ReleaseRareAnimal(definition.RuntimeAnimalId, request.LocationRef, request.WorldDay);
            if (!decision.Accepted) return FromDecision(decision, definition, request, eventId);
            if (transport != null) transport.Status = D11TransportStatus.Released;
            var result = ApplyEvidenceAction(eventId, request.SaveRevision, request, definition,
                D11EvidenceKind.Release, "released", "animal_released", decision,
                "动物已释放；图鉴保留救助和释放证据，不抹除历史表现。");
            if (transport != null) result.TransportId = transport.TransportId;
            return result;
        }

        public D11RareAnimalResult Collect(D11CollectionRequest request)
        {
            var input = ValidateInput(request, D11RareAnimalAction.Collect);
            if (input != null) return input;
            var definition = D11RareAnimalCatalog.Find(request.SpeciesId);
            if (definition == null) return Reject("species_missing", "正式稀有动物定义不存在。", request);
            var collector = FindCollector(request.CollectorId);
            var transport = FindTransport(request.TransportId);
            var book = FindBook(definition.SpeciesId);
            if (collector == null || transport == null || transport.Status != D11TransportStatus.Arrived ||
                book == null || !book.Identified || !book.Rescued ||
                !Contains(collector.AllowedSpeciesIds, definition.SpeciesId))
                return Reject("collection_chain_incomplete", "合法收藏必须来自已抵达的合法转移和完整图鉴证据。", request);
            string eventId = EventId("collect", request, definition);
            var duplicate = Duplicate(eventId, request.SaveRevision, request, definition);
            if (duplicate != null) return duplicate;
            var decision = _ecology.CollectRareAnimal(definition.RuntimeAnimalId, request.LocationRef, request.WorldDay);
            if (!decision.Accepted) return FromDecision(decision, definition, request, eventId);
            transport.Status = D11TransportStatus.Collected;
            var result = ApplyEvidenceAction(eventId, request.SaveRevision, request, definition,
                D11EvidenceKind.Collection, "collected", "animal_collected", decision,
                "合法收藏已入册；没有由 Agent 生成额外奖励或价格。");
            result.TransportId = transport.TransportId;
            return result;
        }

        public D11RareAnimalResult IllegalTrade(D11IllegalTradeRequest request)
        {
            var input = ValidateInput(request, D11RareAnimalAction.IllegalTrade);
            if (input != null) return input;
            var definition = D11RareAnimalCatalog.Find(request.SpeciesId);
            if (definition == null) return Reject("species_missing", "正式稀有动物定义不存在。", request);
            string eventId = EventId("illegal_trade", request, definition);
            var duplicate = Duplicate(eventId, request.SaveRevision, request, definition);
            if (duplicate != null) return duplicate;
            var decision = _ecology.RecordIllegalCaptureAttempt(definition.RuntimeAnimalId, request.WorldDay);
            if (!decision.Accepted) return FromDecision(decision, definition, request, eventId);
            var legal = _factions.RecordWildlifeOffense(request.ControllingFactionId,
                definition.SpeciesId);
            var result = ApplyEvidenceAction(eventId, request.SaveRevision, request, definition,
                D11EvidenceKind.IllegalTrade, "illegal_trade_attempt", "illegal_wildlife_trade",
                decision, "非法交易被拒绝并进入法律与声望后果；没有生成动物或奖励。");
            result.LegalConsequence = legal;
            return result;
        }

        private D11RareAnimalResult ApplyEvidenceAction(string eventId, long saveRevision,
            D11RareAnimalEvidenceInput request, D11RareAnimalDefinition definition, D11EvidenceKind kind,
            string handling, string resultCode, D11AnimalEcologyDecision decision, string message)
        {
            var evidence = BuildEvidence(request, definition, kind, handling, decision, eventId);
            var entry = UpsertBook(definition, request, handling, decision);
            if (!Contains(entry.EvidenceIds, evidence.EvidenceId)) entry.EvidenceIds.Add(evidence.EvidenceId);
            _state.Evidence.Add(evidence);
            var result = CommitEvent(eventId, saveRevision, request.WorldDay, definition.SpeciesId,
                definition.RuntimeAnimalId, resultCode, message,
                new[] { evidence.EvidenceId, "codex:" + entry.EntryId },
                new Dictionary<string, string>
                {
                    ["species_id"] = definition.SpeciesId,
                    ["animal_id"] = definition.RuntimeAnimalId,
                    ["evidence_id"] = evidence.EvidenceId,
                    ["animal_lifecycle"] = decision.Lifecycle.ToString(),
                    ["handling"] = handling,
                }, saveRevision);
            result.Succeeded = true;
            result.Applied = true;
            result.EvidenceId = evidence.EvidenceId;
            result.Evidence = evidence.Clone();
            result.BookEntry = entry.Clone();
            result.SpeciesId = definition.SpeciesId;
            result.AnimalId = definition.RuntimeAnimalId;
            result.Lifecycle = decision.Lifecycle;
            result.EventId = eventId;
            _state.LastEventId = eventId;
            Remember(eventId, saveRevision);
            return result;
        }

        private D11RareAnimalResult CommitEvent(string eventId, long saveRevision, int worldDay,
            string speciesId, string animalId, string resultCode, string message, IEnumerable<string> factIds,
            Dictionary<string, string> values, long revision)
        {
            var result = new D11RareAnimalResult
            {
                Code = resultCode,
                Message = message,
                SpeciesId = speciesId,
                AnimalId = animalId,
                EventId = eventId,
                WorldDay = Math.Max(0, worldDay),
            };
            if (_eventLog != null)
            {
                var append = _eventLog.Append(new WorldEvent(eventId, Math.Max(0, worldDay),
                    "island_saint_marys", Owner,
                    new EventResult
                    {
                        ResultCode = resultCode,
                        Summary = message,
                        FactIds = new List<string>(factIds ?? new string[0]),
                        Values = values ?? new Dictionary<string, string>(),
                    }, new[] { Owner }, new[] { "ecology:" + animalId }, new string[0],
                    Math.Max(0, revision)));
                result.Duplicate = append.IsDuplicate;
                if (append.Fact != null) result.EventId = append.Fact.EventId;
            }
            return result;
        }

        private D11AnimalEvidenceRecord BuildEvidence(D11RareAnimalEvidenceInput request,
            D11RareAnimalDefinition definition, D11EvidenceKind kind, string handling,
            D11AnimalEcologyDecision decision, string eventId)
        {
            string id = "evidence_" + definition.SpeciesId + "_" + kind.ToString().ToLowerInvariant() +
                "_" + Math.Max(0, request.WorldDay) + "_" + _state.Evidence.Count;
            return new D11AnimalEvidenceRecord
            {
                EvidenceId = id,
                Kind = kind,
                AnimalId = definition.RuntimeAnimalId,
                SpeciesId = definition.SpeciesId,
                LocationRef = request.LocationRef,
                WorldDay = Math.Max(0, request.WorldDay),
                Season = request.Season,
                Behavior = string.IsNullOrEmpty(decision.Behavior) ? definition.BehaviorNote : decision.Behavior,
                Handling = handling,
                SourceRef = definition.SourceRef,
                EvidenceRef = request.EvidenceRef,
                FactEventId = eventId,
                Verified = true,
            };
        }

        private D11CollectionBookEntry UpsertBook(D11RareAnimalDefinition definition,
            D11RareAnimalEvidenceInput request, string handling, D11AnimalEcologyDecision decision)
        {
            var entry = FindBook(definition.SpeciesId);
            if (entry == null)
            {
                entry = new D11CollectionBookEntry
                {
                    EntryId = "codex_" + definition.SpeciesId,
                    SpeciesId = definition.SpeciesId,
                    AnimalId = definition.RuntimeAnimalId,
                    FirstLocationRef = request.LocationRef,
                    FirstWorldDay = Math.Max(0, request.WorldDay),
                    FirstSeason = request.Season,
                    FirstBehavior = string.IsNullOrEmpty(decision.Behavior) ? definition.BehaviorNote : decision.Behavior,
                };
                _state.Book.Add(entry);
            }
            if (entry.FirstWorldDay < 0) entry.FirstWorldDay = Math.Max(0, request.WorldDay);
            if (string.IsNullOrEmpty(entry.FirstLocationRef)) entry.FirstLocationRef = request.LocationRef;
            if (string.IsNullOrEmpty(entry.FirstSeason)) entry.FirstSeason = request.Season;
            if (string.IsNullOrEmpty(entry.FirstBehavior)) entry.FirstBehavior = decision.Behavior;
            entry.Handling = handling;
            entry.CurrentStatus = decision.Lifecycle.ToString().ToLowerInvariant();
            entry.Observed |= decision.Code == "animal_observed";
            entry.Identified |= decision.Code == "animal_identified";
            entry.Rescued |= decision.Code == "animal_rescued";
            entry.Released |= decision.Code == "animal_released";
            entry.Collected |= decision.Code == "animal_collected";
            entry.Dead |= decision.Died;
            entry.Revision = ++_state.Revision;
            return entry;
        }

        private D11RareAnimalResult Duplicate(string eventId, long saveRevision,
            D11RareAnimalEvidenceInput request, D11RareAnimalDefinition definition)
        {
            string key = EventKey(eventId, saveRevision);
            if (!_state.AppliedEventKeys.Contains(key) &&
                (_eventLog == null || _eventLog.Find(eventId, Math.Max(0, saveRevision)) == null)) return null;
            var entry = definition == null ? null : FindBook(definition.SpeciesId);
            return new D11RareAnimalResult
            {
                Succeeded = true,
                Applied = false,
                Duplicate = true,
                Code = "duplicate_rare_animal_event",
                Message = "稀有动物事实已经结算，未重复写入图鉴或收藏。",
                SpeciesId = definition == null ? null : definition.SpeciesId,
                AnimalId = definition == null ? null : definition.RuntimeAnimalId,
                EventId = eventId,
                WorldDay = request == null ? 0 : Math.Max(0, request.WorldDay),
                BookEntry = entry == null ? null : entry.Clone(),
            };
        }

        private D11RareAnimalResult FromDecision(D11AnimalEcologyDecision decision,
            D11RareAnimalDefinition definition, D11RareAnimalEvidenceInput request, string eventId)
        {
            return new D11RareAnimalResult
            {
                Succeeded = false,
                Applied = false,
                Code = decision.Code,
                Message = decision.Message,
                SpeciesId = definition == null ? null : definition.SpeciesId,
                AnimalId = decision.AnimalId,
                EventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                Lifecycle = decision.Lifecycle,
            };
        }

        private D11RareAnimalResult Reject(string code, string message, D11RareAnimalEvidenceInput request)
        {
            return new D11RareAnimalResult
            {
                Succeeded = false,
                Applied = false,
                Code = code,
                Message = message,
                SpeciesId = request == null ? null : request.SpeciesId,
                AnimalId = request == null ? null : request.AnimalId,
                WorldDay = request == null ? 0 : Math.Max(0, request.WorldDay),
            };
        }

        private D11RareAnimalResult RejectTransport(string code, string message,
            D11AnimalTransportState transport, string eventId, int day)
        {
            return new D11RareAnimalResult
            {
                Succeeded = false,
                Code = code,
                Message = message,
                SpeciesId = transport.SpeciesId,
                AnimalId = transport.AnimalId,
                TransportId = transport.TransportId,
                EventId = eventId,
                WorldDay = Math.Max(0, day),
            };
        }

        private static D11RareAnimalResult ValidateInput(D11RareAnimalEvidenceInput request, D11RareAnimalAction action)
        {
            if (request == null) return new D11RareAnimalResult { Code = "request_missing", Message = "动物操作请求不能为空。" };
            if (string.IsNullOrWhiteSpace(request.SpeciesId) || string.IsNullOrWhiteSpace(request.LocationRef))
                return new D11RareAnimalResult { Code = "location_or_species_missing", Message = "动物操作必须定位物种和地点。" };
            if (request.WorldDay < 0) return new D11RareAnimalResult { Code = "world_day_invalid", Message = "世界日不能为负数。" };
            if (string.IsNullOrWhiteSpace(request.EvidenceRef))
                return new D11RareAnimalResult { Code = "evidence_missing", Message = action.ToString() + " 必须提供证据引用。" };
            if (string.IsNullOrWhiteSpace(request.Season))
                return new D11RareAnimalResult { Code = "season_missing", Message = "图鉴记录必须提供季节。" };
            return null;
        }

        private string EventId(string action, D11RareAnimalEvidenceInput request, D11RareAnimalDefinition definition)
        {
            string suffix = string.IsNullOrWhiteSpace(request.ActionId)
                ? Math.Max(0, request.WorldDay).ToString(CultureInfo.InvariantCulture)
                : request.ActionId;
            return "event_d11_animal_" + action + "_" + definition.SpeciesId + "_" + suffix;
        }

        private void InitializeCatalogState()
        {
            if (_state.Book == null) _state.Book = new List<D11CollectionBookEntry>();
            if (_state.Evidence == null) _state.Evidence = new List<D11AnimalEvidenceRecord>();
            if (_state.Transports == null) _state.Transports = new List<D11AnimalTransportState>();
            if (_state.Collectors == null) _state.Collectors = new List<D11CollectorState>();
            if (_state.ProtectedAreas == null) _state.ProtectedAreas = new List<D11ProtectedAreaState>();
            if (_state.AppliedEventKeys == null) _state.AppliedEventKeys = new List<string>();
            if (FindCollector(D11RareAnimalCatalog.CollectorId) == null)
                _state.Collectors.Add(new D11CollectorState
                {
                    CollectorId = D11RareAnimalCatalog.CollectorId,
                    CollectorType = "protected_research_collector",
                    BuyerType = "research",
                    AllowedSpeciesIds = new[] { "animal_green_turtle" },
                    ProtectedAreaRef = D11RareAnimalCatalog.ProtectedAreaId,
                    ReputationGate = 0,
                    EvidenceRequired = true,
                });
            if (FindArea(D11RareAnimalCatalog.ProtectedAreaId) == null)
                _state.ProtectedAreas.Add(new D11ProtectedAreaState
                {
                    AreaId = D11RareAnimalCatalog.ProtectedAreaId,
                    LocationRef = "island_saint_marys",
                    ObservationAllowed = true,
                    RescueAllowed = true,
                    ReleaseAllowed = true,
                    TransferAllowed = false,
                    Active = true,
                });
        }

        private D11CollectorState FindCollector(string id)
        {
            if (string.IsNullOrEmpty(id)) id = D11RareAnimalCatalog.CollectorId;
            foreach (var collector in _state.Collectors)
                if (collector != null && collector.CollectorId == id) return collector;
            return null;
        }

        private D11ProtectedAreaState FindArea(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var area in _state.ProtectedAreas)
                if (area != null && area.AreaId == id) return area;
            return null;
        }

        private D11ProtectedAreaState FindAreaFor(string locationRef)
        {
            foreach (var area in _state.ProtectedAreas)
                if (area != null && area.LocationRef == locationRef) return area;
            return null;
        }

        private D11CollectionBookEntry FindBook(string speciesId)
        {
            foreach (var entry in _state.Book)
                if (entry != null && entry.SpeciesId == speciesId) return entry;
            return null;
        }

        private D11AnimalTransportState FindTransport(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var transport in _state.Transports)
                if (transport != null && transport.TransportId == id) return transport;
            return null;
        }

        private void Remember(string eventId, long saveRevision)
        {
            string key = EventKey(eventId, saveRevision);
            if (!_state.AppliedEventKeys.Contains(key)) _state.AppliedEventKeys.Add(key);
        }

        private static string EventKey(string eventId, long saveRevision)
        {
            return (eventId ?? "") + "\u001f" + Math.Max(0, saveRevision).ToString(CultureInfo.InvariantCulture);
        }

        private static bool Contains(IEnumerable<string> values, string value)
        {
            if (values == null) return false;
            foreach (var item in values) if (item == value) return true;
            return false;
        }
    }
}
