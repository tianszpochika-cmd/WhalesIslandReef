// ============================================================
// D9TreasureQuestSystem —— 第一张海图寻宝任务
//
// QuestSystem 只拥有任务状态、线索可靠度、危险标签、恢复路线和
// RewardRequest。它不写金钱、库存、关系、船体或奖励结果；实际奖励由
// RewardSystem/EconomySystem/InventorySystem 读取请求后各自裁决。
//
// 所有成功、失败和恢复事实都进入 D8 EventLog。保存 envelope 只承载
// Quest 快照 + EventLog 快照，SQLite 仍由 SaveSystem 负责，不在此复制。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D9
{
    public enum D9TreasureQuestStatus
    {
        RumorAvailable,
        RumorHeard,
        Accepted,
        Failed,
        Recovered,
        Discovered,
    }

    public enum D9TreasureRouteChoice
    {
        DirectLanding,
        WaitForTide,
        VillageGuide,
    }

    public static class D9TreasureEventIds
    {
        public const string Clue = "event_d9_treasure_clue";
        public const string Resolved = "event_d9_treasure_resolved";
        public const string Recovered = "event_d9_treasure_recovered";
    }

    public static class D9TreasureFailureCodes
    {
        public const string None = "none";
        public const string RumorNotHeard = "rumor_not_heard";
        public const string EquipmentRequired = "equipment_required";
        public const string WrongLandingSite = "wrong_landing_site";
        public const string UnsafeLanding = "unsafe_landing";
        public const string TideWindowClosed = "tide_window_closed";
        public const string GuideUnavailable = "guide_unavailable";
        public const string RecoveryRequired = "recovery_required";
        public const string RecoveryUnavailable = "recovery_unavailable";
        public const string AlreadyDiscovered = "already_discovered";
        public const string ClueRequired = "clue_required";
        public const string ClueNotVerified = "clue_not_cross_validated";
        public const string ClueWindowClosed = "clue_window_closed";
        public const string ClueAlreadyRecorded = "clue_already_recorded";
        public const string ClueVerificationUnavailable = "clue_verification_unavailable";
    }

    /// <summary>D12 线索的结构化来源；来源类型不代表可靠度，可靠度由 QuestSystem 裁决。</summary>
    public enum D9TreasureClueSourceKind
    {
        TavernRumor,
        SalvagedChart,
        CrewTestimony,
        MapDiscovery,
        DirectObservation,
    }

    /// <summary>奖励类别仅用于 RewardSystem 路由，不携带金额、数量或物品结算值。</summary>
    public enum D9TreasureRewardType
    {
        MapKnowledge,
        Currency,
        Item,
        Reputation,
    }

    [Serializable]
    public sealed class D9TreasureClueRecord
    {
        public string ClueId;
        public string QuestId;
        public D9TreasureClueSourceKind SourceKind;
        public string SourceRef;
        public string EvidenceRef;
        public string Text;
        public int ReliabilityPercent;
        public string RegionRef;
        public string SiteRef;
        public int ValidFromWorldDay = -1;
        public int ValidUntilWorldDay = -1;
        public List<string> RequiredEquipmentIds = new List<string>();
        public string AlternativeRouteId;
        public D9TreasureRewardType RewardType = D9TreasureRewardType.MapKnowledge;
        public List<string> CauseEventIds = new List<string>();
        public List<string> CrossValidatedByClueIds = new List<string>();
        public string FactEventRef;
        public bool IsCrossValidated;

        public D9TreasureClueRecord Clone()
        {
            var result = new D9TreasureClueRecord
            {
                ClueId = ClueId,
                QuestId = QuestId,
                SourceKind = SourceKind,
                SourceRef = SourceRef,
                EvidenceRef = EvidenceRef,
                Text = Text,
                ReliabilityPercent = ReliabilityPercent,
                RegionRef = RegionRef,
                SiteRef = SiteRef,
                ValidFromWorldDay = ValidFromWorldDay,
                ValidUntilWorldDay = ValidUntilWorldDay,
                AlternativeRouteId = AlternativeRouteId,
                RewardType = RewardType,
                FactEventRef = FactEventRef,
                IsCrossValidated = IsCrossValidated,
            };
            if (RequiredEquipmentIds != null) result.RequiredEquipmentIds.AddRange(RequiredEquipmentIds);
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            if (CrossValidatedByClueIds != null) result.CrossValidatedByClueIds.AddRange(CrossValidatedByClueIds);
            return result;
        }

        public void Validate()
        {
            Require(ClueId, "ClueId");
            Require(QuestId, "QuestId");
            Require(SourceRef, "SourceRef");
            Require(EvidenceRef, "EvidenceRef");
            Require(RegionRef, "RegionRef");
            Require(SiteRef, "SiteRef");
            if (ReliabilityPercent < 0 || ReliabilityPercent > 100)
                throw new ArgumentOutOfRangeException("ReliabilityPercent", "Treasure clue reliability must be 0..100.");
            if (ValidFromWorldDay < -1 || ValidUntilWorldDay < -1)
                throw new ArgumentOutOfRangeException("ValidWorldDay", "Treasure clue window uses -1 or a non-negative world day.");
            if (ValidFromWorldDay >= 0 && ValidUntilWorldDay >= 0 && ValidFromWorldDay > ValidUntilWorldDay)
                throw new ArgumentException("Treasure clue valid_from cannot exceed valid_until.");
            if (!Enum.IsDefined(typeof(D9TreasureClueSourceKind), SourceKind))
                throw new ArgumentException("Treasure clue source kind is invalid.");
            if (!Enum.IsDefined(typeof(D9TreasureRewardType), RewardType))
                throw new ArgumentException("Treasure clue reward type is invalid.");
            ValidateIds(RequiredEquipmentIds, "RequiredEquipmentIds");
            ValidateIds(CauseEventIds, "CauseEventIds");
            ValidateIds(CrossValidatedByClueIds, "CrossValidatedByClueIds");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(field + " is required.");
        }

        private static void ValidateIds(IEnumerable<string> values, string field)
        {
            if (values == null) return;
            foreach (var value in values)
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(field + " cannot contain empty ids.");
        }
    }

    [Serializable]
    public sealed class D9TreasureClueDefinition
    {
        public string Id;
        public D9TreasureClueSourceKind SourceKind = D9TreasureClueSourceKind.TavernRumor;
        public string SourceRef;
        public string EvidenceRef;
        public string Text;
        public int ReliabilityPercent;
        public string RegionRef;
        public string SiteRef;
        public string TimeWindow;
        public int ValidFromWorldDay = -1;
        public int ValidUntilWorldDay = -1;
        public List<string> RequiredEquipmentIds = new List<string>();
        public string AlternativeRouteId;
        public D9TreasureRewardType RewardType = D9TreasureRewardType.MapKnowledge;
        public List<string> DangerTags = new List<string>();
        public List<string> AlternativeRouteIds = new List<string>();

        public D9TreasureClueDefinition Clone()
        {
            var result = new D9TreasureClueDefinition
            {
                Id = Id,
                SourceKind = SourceKind,
                SourceRef = SourceRef,
                EvidenceRef = EvidenceRef,
                Text = Text,
                ReliabilityPercent = ReliabilityPercent,
                RegionRef = RegionRef,
                SiteRef = SiteRef,
                TimeWindow = TimeWindow,
                ValidFromWorldDay = ValidFromWorldDay,
                ValidUntilWorldDay = ValidUntilWorldDay,
                AlternativeRouteId = AlternativeRouteId,
                RewardType = RewardType,
            };
            if (RequiredEquipmentIds != null) result.RequiredEquipmentIds.AddRange(RequiredEquipmentIds);
            if (DangerTags != null) result.DangerTags.AddRange(DangerTags);
            if (AlternativeRouteIds != null) result.AlternativeRouteIds.AddRange(AlternativeRouteIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D9TreasureRecoveryOption
    {
        public string RouteId;
        public string Action;
        public string Description;
        public string RequiredEquipmentId;

        public D9TreasureRecoveryOption Clone()
        {
            return new D9TreasureRecoveryOption
            {
                RouteId = RouteId,
                Action = Action,
                Description = Description,
                RequiredEquipmentId = RequiredEquipmentId,
            };
        }
    }

    [Serializable]
    public sealed class D9TreasureQuestDefinition
    {
        public string QuestId;
        public string SiteRef;
        public string RewardRequestId;
        public D9TreasureClueDefinition Clue = new D9TreasureClueDefinition();
        public List<D9TreasureClueDefinition> AdditionalClues = new List<D9TreasureClueDefinition>();
        public List<string> RequiredEquipmentIds = new List<string>();
        public List<string> DangerTags = new List<string>();
        public List<D9TreasureRecoveryOption> RecoveryOptions = new List<D9TreasureRecoveryOption>();

        public D9TreasureQuestDefinition Clone()
        {
            var result = new D9TreasureQuestDefinition
            {
                QuestId = QuestId,
                SiteRef = SiteRef,
                RewardRequestId = RewardRequestId,
                Clue = Clue == null ? null : Clue.Clone(),
            };
            if (AdditionalClues != null)
                foreach (var clue in AdditionalClues)
                    if (clue != null) result.AdditionalClues.Add(clue.Clone());
            if (RequiredEquipmentIds != null) result.RequiredEquipmentIds.AddRange(RequiredEquipmentIds);
            if (DangerTags != null) result.DangerTags.AddRange(DangerTags);
            if (RecoveryOptions != null)
                foreach (var option in RecoveryOptions)
                    if (option != null) result.RecoveryOptions.Add(option.Clone());
            return result;
        }

        internal void Validate()
        {
            Require(QuestId, "QuestId");
            Require(SiteRef, "SiteRef");
            Require(RewardRequestId, "RewardRequestId");
            if (Clue == null) throw new ArgumentException("D9 treasure clue is required.");
            Require(Clue.Id, "Clue.Id");
            Require(Clue.SourceRef, "Clue.SourceRef");
            Require(Clue.EvidenceRef, "Clue.EvidenceRef");
            Require(Clue.RegionRef, "Clue.RegionRef");
            Require(Clue.SiteRef, "Clue.SiteRef");
            if (Clue.ReliabilityPercent < 0 || Clue.ReliabilityPercent > 100)
                throw new ArgumentOutOfRangeException("Clue.ReliabilityPercent", "Clue reliability must be 0..100.");
            if (Clue.ValidFromWorldDay < -1 || Clue.ValidUntilWorldDay < -1 ||
                (Clue.ValidFromWorldDay >= 0 && Clue.ValidUntilWorldDay >= 0 &&
                 Clue.ValidFromWorldDay > Clue.ValidUntilWorldDay))
                throw new ArgumentException("Primary clue world-day window is invalid.");
            if (!Enum.IsDefined(typeof(D9TreasureClueSourceKind), Clue.SourceKind) ||
                !Enum.IsDefined(typeof(D9TreasureRewardType), Clue.RewardType))
                throw new ArgumentException("Primary clue source or reward type is invalid.");
            var clueIds = new HashSet<string>(StringComparer.Ordinal) { Clue.Id };
            if (AdditionalClues != null)
                foreach (var clue in AdditionalClues)
                {
                    if (clue == null) throw new ArgumentException("Additional clue cannot be null.");
                    Require(clue.Id, "Additional clue id");
                    if (!clueIds.Add(clue.Id)) throw new ArgumentException("Treasure clue ids must be unique: " + clue.Id);
                    Require(clue.SourceRef, "Additional clue source");
                    Require(clue.EvidenceRef, "Additional clue evidence");
                    Require(clue.RegionRef, "Additional clue region");
                    Require(clue.SiteRef, "Additional clue site");
                    if (clue.ReliabilityPercent < 0 || clue.ReliabilityPercent > 100)
                        throw new ArgumentOutOfRangeException("AdditionalClue.ReliabilityPercent");
                    if (clue.ValidFromWorldDay < -1 || clue.ValidUntilWorldDay < -1 ||
                        (clue.ValidFromWorldDay >= 0 && clue.ValidUntilWorldDay >= 0 &&
                         clue.ValidFromWorldDay > clue.ValidUntilWorldDay))
                        throw new ArgumentException("Additional clue world-day window is invalid: " + clue.Id);
                    if (!Enum.IsDefined(typeof(D9TreasureClueSourceKind), clue.SourceKind) ||
                        !Enum.IsDefined(typeof(D9TreasureRewardType), clue.RewardType))
                        throw new ArgumentException("Additional clue source or reward type is invalid: " + clue.Id);
                }
            if (RequiredEquipmentIds == null || RequiredEquipmentIds.Count == 0)
                throw new ArgumentException("At least one equipment precondition is required.");
            if (DangerTags == null || DangerTags.Count == 0)
                throw new ArgumentException("At least one danger tag is required.");
            if (RecoveryOptions == null || RecoveryOptions.Count < 2)
                throw new ArgumentException("At least two recovery/alternative routes are required.");

            var routeIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var route in RecoveryOptions)
            {
                if (route == null) throw new ArgumentException("Recovery route cannot be null.");
                Require(route.RouteId, "Recovery route id");
                Require(route.Action, "Recovery route action");
                if (!routeIds.Add(route.RouteId))
                    throw new ArgumentException("Recovery route ids must be unique: " + route.RouteId);
            }
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(field + " is required.");
        }
    }

    public static class D9TreasureQuestCatalog
    {
        public const string QuestId = "quest_first_chart";
        public const string RewardRequestId = "reward_first_chart";
        public const string SiteRef = "landmark:saint_marys_chart_cove";
        public const string RequiredEquipmentId = "item_chart_compass";

        public static D9TreasureQuestDefinition CreateFirstChart()
        {
            var definition = new D9TreasureQuestDefinition
            {
                QuestId = QuestId,
                SiteRef = SiteRef,
                RewardRequestId = RewardRequestId,
                Clue = new D9TreasureClueDefinition
                {
                    Id = "clue:first_chart_tide_mark",
                    SourceKind = D9TreasureClueSourceKind.TavernRumor,
                    SourceRef = "rumor:santa_margarita_dockhand",
                    EvidenceRef = "testimony:first_chart_tide_mark",
                    Text = "旧海图的潮线标记指向圣玛丽海图湾；先用罗盘核对岸线，不需要猜解密码。",
                    ReliabilityPercent = 72,
                    RegionRef = "island_saint_marys",
                    SiteRef = SiteRef,
                    TimeWindow = "low_tide",
                    RewardType = D9TreasureRewardType.MapKnowledge,
                    DangerTags = new List<string> { "surf_break", "unstable_shore" },
                    AlternativeRouteIds = new List<string>
                    {
                        "route_wait_tide",
                        "route_village_guide",
                        "route_return_to_harbor",
                    },
                },
                AdditionalClues = new List<D9TreasureClueDefinition>
                {
                    new D9TreasureClueDefinition
                    {
                        Id = "clue:first_chart_salvaged_chart",
                        SourceKind = D9TreasureClueSourceKind.SalvagedChart,
                        SourceRef = "chart:salvaged_coastal_fragment",
                        EvidenceRef = "chart_fragment:first_chart_cove",
                        Text = "打捞的旧海图碎片把同一处湾口标成低潮可接近的浅滩。",
                        ReliabilityPercent = 84,
                        RegionRef = "island_saint_marys",
                        SiteRef = SiteRef,
                        TimeWindow = "low_tide",
                        RewardType = D9TreasureRewardType.Item,
                        RequiredEquipmentIds = new List<string> { RequiredEquipmentId },
                        AlternativeRouteId = "route_buy_chart_kit",
                    },
                    new D9TreasureClueDefinition
                    {
                        Id = "clue:first_chart_reef_keeper",
                        SourceKind = D9TreasureClueSourceKind.CrewTestimony,
                        SourceRef = "npc:green_cay_reef_keeper",
                        EvidenceRef = "testimony:reef_keeper_low_tide",
                        Text = "绿礁村守礁人指出冲浪断口的位置，并确认低潮时可以从内湾绕行。",
                        ReliabilityPercent = 66,
                        RegionRef = "island_saint_marys",
                        SiteRef = SiteRef,
                        TimeWindow = "low_tide",
                        RewardType = D9TreasureRewardType.Reputation,
                        AlternativeRouteId = "route_village_guide",
                    },
                },
                RequiredEquipmentIds = new List<string> { RequiredEquipmentId },
                DangerTags = new List<string> { "surf_break", "unstable_shore" },
                RecoveryOptions = new List<D9TreasureRecoveryOption>
                {
                    new D9TreasureRecoveryOption
                    {
                        RouteId = "route_buy_chart_kit",
                        Action = "obtain_equipment",
                        Description = "在港口取得海图罗盘后，从已知岸线重新尝试。",
                        RequiredEquipmentId = RequiredEquipmentId,
                    },
                    new D9TreasureRecoveryOption
                    {
                        RouteId = "route_wait_tide",
                        Action = "wait_for_tide",
                        Description = "等待低潮窗口，再沿潮线登陆。",
                    },
                    new D9TreasureRecoveryOption
                    {
                        RouteId = "route_village_guide",
                        Action = "ask_village_guide",
                        Description = "从绿礁村寻找熟悉岸线的向导，绕开冲浪断口。",
                    },
                    new D9TreasureRecoveryOption
                    {
                        RouteId = "route_return_to_harbor",
                        Action = "return_to_known_harbor",
                        Description = "返回圣玛格丽特港，保留线索和失败事实后再准备。",
                    },
                    new D9TreasureRecoveryOption
                    {
                        RouteId = "route_cross_validate_clue",
                        Action = "cross_validate_clue",
                        Description = "从另一种独立来源核对同一湾口，解除不可靠线索造成的阻断。",
                    },
                },
            };
            definition.Validate();
            return definition;
        }
    }

    [Serializable]
    public sealed class D9TreasureClueExpressionProposal
    {
        public string QuestId;
        public string ClueId;
        public string SpeakerRef;
        public string SuggestedText;
        public D9TreasureClueSourceKind SourceKind;
        public string SourceRef;
        public string EvidenceRef;
        public int ReliabilityPercent;
    }

    [Serializable]
    public sealed class D9TreasureClueRequest
    {
        public string ActorRef;
        public string ClueId;
        public D9TreasureClueSourceKind SourceKind;
        public string SourceRef;
        public string EvidenceRef;
        public string Text;
        public int ReliabilityPercent;
        public string RegionRef;
        public string SiteRef;
        public int ValidFromWorldDay = -1;
        public int ValidUntilWorldDay = -1;
        public List<string> RequiredEquipmentIds = new List<string>();
        public string AlternativeRouteId;
        public D9TreasureRewardType RewardType = D9TreasureRewardType.MapKnowledge;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D9TreasureClueVerificationRequest
    {
        public string ActorRef;
        public string ClueId;
        public string OtherClueId;
    }

    [Serializable]
    public sealed class D9TreasureLandingRequest
    {
        public string ActorRef;
        public string LocationRef;
        public string ClueId;
        public D9TreasureRouteChoice RouteChoice;
        public bool TideWindowOpen;
        public bool EnforceClueWindow;
        public bool RequireCrossValidation;
        public bool VillageGuideAvailable;
        public List<string> EquipmentIds = new List<string>();
    }

    [Serializable]
    public sealed class D9TreasureRecoveryRequest
    {
        public string ActorRef;
        public string RouteId;
        public string ClueId;
        public bool TideWindowOpen;
        public bool VillageGuideAvailable;
        public List<string> EquipmentIds = new List<string>();
    }

    [Serializable]
    public sealed class D9TreasureRewardRequest
    {
        public string RequestId;
        public string RewardId;
        public string QuestId;
        public string SourceEventRef;
        public D9TreasureRewardType RewardType = D9TreasureRewardType.MapKnowledge;
        public List<string> CauseEventIds = new List<string>();
        public List<string> ClueIds = new List<string>();
        public string Status = "pending";

        public D9TreasureRewardRequest Clone()
        {
            var result = new D9TreasureRewardRequest
            {
                RequestId = RequestId,
                RewardId = RewardId,
                QuestId = QuestId,
                SourceEventRef = SourceEventRef,
                RewardType = RewardType,
                Status = Status,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            if (ClueIds != null) result.ClueIds.AddRange(ClueIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D9TreasureQuestSnapshot
    {
        public int SchemaVersion = 1;
        public string QuestId;
        public D9TreasureQuestStatus Status;
        public bool RumorHeard;
        public bool EquipmentReady;
        public int ClueReliabilityPercent;
        public string LastFailureCode = D9TreasureFailureCodes.None;
        public string RecoveryRouteId;
        public string LastResolutionEventRef;
        public string LastRecoveryEventRef;
        public List<string> DangerTags = new List<string>();
        public List<string> FactEventRefs = new List<string>();
        public List<D9TreasureClueRecord> Clues = new List<D9TreasureClueRecord>();
        public List<string> VerifiedClueIds = new List<string>();
        public int CrossValidationCount;
        public D9TreasureRewardRequest RewardRequest;

        public D9TreasureQuestSnapshot Clone()
        {
            var result = new D9TreasureQuestSnapshot
            {
                SchemaVersion = SchemaVersion,
                QuestId = QuestId,
                Status = Status,
                RumorHeard = RumorHeard,
                EquipmentReady = EquipmentReady,
                ClueReliabilityPercent = ClueReliabilityPercent,
                LastFailureCode = LastFailureCode,
                RecoveryRouteId = RecoveryRouteId,
                LastResolutionEventRef = LastResolutionEventRef,
                LastRecoveryEventRef = LastRecoveryEventRef,
                CrossValidationCount = CrossValidationCount,
                RewardRequest = RewardRequest == null ? null : RewardRequest.Clone(),
            };
            if (DangerTags != null) result.DangerTags.AddRange(DangerTags);
            if (FactEventRefs != null) result.FactEventRefs.AddRange(FactEventRefs);
            if (VerifiedClueIds != null) result.VerifiedClueIds.AddRange(VerifiedClueIds);
            if (Clues != null)
                foreach (var clue in Clues)
                    if (clue != null) result.Clues.Add(clue.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != 1) throw new InvalidOperationException("D9 treasure quest schema_version mismatch.");
            if (string.IsNullOrWhiteSpace(QuestId)) throw new InvalidOperationException("D9 treasure quest id is required.");
            if (!Enum.IsDefined(typeof(D9TreasureQuestStatus), Status)) throw new InvalidOperationException("Unknown D9 treasure quest status.");
            if (ClueReliabilityPercent < 0 || ClueReliabilityPercent > 100) throw new ArgumentOutOfRangeException("ClueReliabilityPercent");
            if (CrossValidationCount < 0) throw new ArgumentOutOfRangeException("CrossValidationCount");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (Clues != null)
                foreach (var clue in Clues)
                {
                    if (clue == null) throw new InvalidOperationException("D9 treasure snapshot cannot contain null clue.");
                    clue.Validate();
                    if (!ids.Add(clue.ClueId)) throw new InvalidOperationException("D9 treasure clue duplicated: " + clue.ClueId);
                }
            if (VerifiedClueIds != null)
                foreach (var clueId in VerifiedClueIds)
                    if (!ids.Contains(clueId)) throw new InvalidOperationException("Verified clue is not in snapshot: " + clueId);
        }
    }

    [Serializable]
    public sealed class D9TreasureSaveEnvelope
    {
        public int SaveVersion = 1;
        public D9TreasureQuestSnapshot Quest;
        public EventLogSnapshot EventLog;

        public D9TreasureSaveEnvelope Clone()
        {
            return new D9TreasureSaveEnvelope
            {
                SaveVersion = SaveVersion,
                Quest = Quest == null ? null : Quest.Clone(),
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
        }
    }

    public sealed class D9TreasureQuestOutcome
    {
        public bool Accepted;
        public string Code;
        public string Message;
        public D9TreasureQuestStatus Status;
        public FactEvent Fact;
        public D9TreasureRewardRequest RewardRequest;
        public D9TreasureRecoveryOption RecoveryRoute;
        public IReadOnlyList<string> MissingEquipment;

        internal static D9TreasureQuestOutcome Rejected(
            D9TreasureQuestStatus status, string code, string message,
            D9TreasureRecoveryOption route = null, IReadOnlyList<string> missing = null)
        {
            return new D9TreasureQuestOutcome
            {
                Accepted = false,
                Code = code,
                Message = message,
                Status = status,
                RecoveryRoute = route,
                MissingEquipment = missing,
            };
        }

        internal static D9TreasureQuestOutcome Recorded(
            D9TreasureQuestStatus status, string code, string message, FactEvent fact,
            D9TreasureRewardRequest rewardRequest = null, D9TreasureRecoveryOption route = null)
        {
            return new D9TreasureQuestOutcome
            {
                Accepted = true,
                Code = code,
                Message = message,
                Status = status,
                Fact = fact,
                RewardRequest = rewardRequest == null ? null : rewardRequest.Clone(),
                RecoveryRoute = route,
            };
        }
    }

    /// <summary>D9 第一张海图的 QuestSystem owner。</summary>
    public sealed class D9TreasureQuestSystem
    {
        private const int SnapshotVersion = 1;
        private readonly EventLog _eventLog;
        private readonly D9TreasureQuestDefinition _definition;
        private readonly D9TreasureQuestSnapshot _state;

        public D9TreasureQuestSystem(EventLog eventLog, D9TreasureQuestDefinition definition = null,
            D9TreasureQuestSnapshot snapshot = null)
        {
            _eventLog = eventLog ?? throw new ArgumentNullException("eventLog");
            _definition = definition == null ? D9TreasureQuestCatalog.CreateFirstChart() : definition.Clone();
            _definition.Validate();
            _state = snapshot == null ? NewSnapshot(_definition) : RestoreSnapshot(snapshot, _definition);
        }

        public D9TreasureQuestDefinition Definition => _definition.Clone();
        public D9TreasureQuestSnapshot Snapshot => _state.Clone();
        public EventLog EventLog => _eventLog;

        /// <summary>只返回给 Agent 的表达提议；不会改变任务状态或事实日志。</summary>
        public D9TreasureClueExpressionProposal BuildClueExpressionProposal(string speakerRef, string suggestedText)
        {
            return new D9TreasureClueExpressionProposal
            {
                QuestId = _definition.QuestId,
                ClueId = _definition.Clue.Id,
                SpeakerRef = speakerRef ?? string.Empty,
                SuggestedText = suggestedText ?? string.Empty,
                SourceKind = _definition.Clue.SourceKind,
                SourceRef = _definition.Clue.SourceRef,
                EvidenceRef = _definition.Clue.EvidenceRef,
                ReliabilityPercent = _state.ClueReliabilityPercent,
            };
        }

        /// <summary>结构化线索的唯一写入口；Agent 只能使用 BuildClueExpressionProposal。</summary>
        public D9TreasureQuestOutcome RecordClue(D9TreasureClueRequest request, int worldDay, long saveRevision)
        {
            if (request == null) return Reject("request_required", "线索记录请求不能为空。");
            if (!HasActor(request.ActorRef)) return Reject("actor_required", "必须有线索记录者。");
            if (_state.Status == D9TreasureQuestStatus.Discovered)
                return Reject(D9TreasureFailureCodes.AlreadyDiscovered, "发现已经完成，不能再修改线索账本。");
            if (FindClue(request.ClueId) != null)
                return Reject(D9TreasureFailureCodes.ClueAlreadyRecorded, "这条线索已经记录。");

            var record = new D9TreasureClueRecord
            {
                ClueId = request.ClueId,
                QuestId = _definition.QuestId,
                SourceKind = request.SourceKind,
                SourceRef = request.SourceRef,
                EvidenceRef = request.EvidenceRef,
                Text = request.Text ?? string.Empty,
                ReliabilityPercent = request.ReliabilityPercent,
                RegionRef = request.RegionRef,
                SiteRef = request.SiteRef,
                ValidFromWorldDay = request.ValidFromWorldDay,
                ValidUntilWorldDay = request.ValidUntilWorldDay,
                AlternativeRouteId = request.AlternativeRouteId,
                RewardType = request.RewardType,
            };
            if (request.RequiredEquipmentIds != null) record.RequiredEquipmentIds.AddRange(request.RequiredEquipmentIds);
            if (request.CauseEventIds != null) record.CauseEventIds.AddRange(request.CauseEventIds);
            try { record.Validate(); }
            catch (Exception ex) { return Reject("invalid_clue", ex.Message); }
            if (!string.Equals(record.RegionRef, _definition.Clue.RegionRef, StringComparison.Ordinal) ||
                !string.Equals(record.SiteRef, _definition.SiteRef, StringComparison.Ordinal))
                return Reject("clue_target_mismatch", "线索必须指向任务定义的区域和登陆点。");

            var fact = AppendFact(
                BuildClueEventId(record.ClueId), worldDay, saveRevision, request.ActorRef,
                BuildClueInputFacts(record), "treasure_clue_recorded",
                "结构化线索已记录；来源、可靠度、时间窗和装备前置由 QuestSystem 保留。",
                BuildClueValues(record), WorldEventStatus.Normal, null, record.CauseEventIds);
            record.FactEventRef = FactRef(fact);
            _state.Clues.Add(record);
            AddFact(fact);
            if (string.Equals(record.ClueId, _definition.Clue.Id, StringComparison.Ordinal))
            {
                _state.ClueReliabilityPercent = record.ReliabilityPercent;
                _state.RumorHeard = true;
                if (_state.Status == D9TreasureQuestStatus.RumorAvailable)
                    _state.Status = D9TreasureQuestStatus.RumorHeard;
            }
            return D9TreasureQuestOutcome.Recorded(_state.Status, "treasure_clue_recorded", "已记录结构化线索。", fact);
        }

        /// <summary>按内容目录记录一条额外来源，避免 UI/Agent 自行伪造可靠度或目标。</summary>
        public D9TreasureQuestOutcome RecordCatalogClue(string clueId, string actorRef, int worldDay, long saveRevision)
        {
            var definition = FindDefinitionClue(clueId);
            if (definition == null) return Reject("unknown_clue", "任务目录中没有这条线索。");
            return RecordClue(new D9TreasureClueRequest
            {
                ActorRef = actorRef,
                ClueId = definition.Id,
                SourceKind = definition.SourceKind,
                SourceRef = definition.SourceRef,
                EvidenceRef = definition.EvidenceRef,
                Text = definition.Text,
                ReliabilityPercent = definition.ReliabilityPercent,
                RegionRef = definition.RegionRef,
                SiteRef = definition.SiteRef,
                ValidFromWorldDay = definition.ValidFromWorldDay,
                ValidUntilWorldDay = definition.ValidUntilWorldDay,
                RequiredEquipmentIds = definition.RequiredEquipmentIds == null
                    ? new List<string>() : new List<string>(definition.RequiredEquipmentIds),
                AlternativeRouteId = definition.AlternativeRouteId,
                RewardType = definition.RewardType,
            }, worldDay, saveRevision);
        }

        /// <summary>
        /// 只有不同来源、同一目标、各自有事实引用的线索才能互相验证；验证本身追加事实，
        /// 不改变库存、经济、关系或 RewardSystem 状态。
        /// </summary>
        public D9TreasureQuestOutcome CrossValidateClues(D9TreasureClueVerificationRequest request,
            int worldDay, long saveRevision)
        {
            if (request == null) return Reject("request_required", "交叉验证请求不能为空。");
            if (!HasActor(request.ActorRef)) return Reject("actor_required", "必须有交叉验证执行者。");
            var first = FindClue(request.ClueId);
            var second = FindClue(request.OtherClueId);
            if (first == null || second == null || first.ClueId == second.ClueId)
                return Reject(D9TreasureFailureCodes.ClueVerificationUnavailable, "必须选择两条不同的已记录线索。");
            if (first.SourceKind == second.SourceKind)
                return Reject(D9TreasureFailureCodes.ClueVerificationUnavailable, "交叉验证必须来自不同来源类型。");
            if (!string.Equals(first.SiteRef, second.SiteRef, StringComparison.Ordinal) ||
                !string.Equals(first.RegionRef, second.RegionRef, StringComparison.Ordinal))
                return Reject(D9TreasureFailureCodes.ClueVerificationUnavailable, "两条线索没有指向同一目标。");
            if (first.CrossValidatedByClueIds.Contains(second.ClueId))
                return Reject(D9TreasureFailureCodes.ClueAlreadyRecorded, "这组线索已经完成交叉验证。");

            first.IsCrossValidated = true;
            second.IsCrossValidated = true;
            first.CrossValidatedByClueIds.Add(second.ClueId);
            second.CrossValidatedByClueIds.Add(first.ClueId);
            if (!_state.VerifiedClueIds.Contains(first.ClueId)) _state.VerifiedClueIds.Add(first.ClueId);
            if (!_state.VerifiedClueIds.Contains(second.ClueId)) _state.VerifiedClueIds.Add(second.ClueId);
            _state.CrossValidationCount++;
            var fact = AppendFact(
                BuildCrossValidationEventId(first.ClueId, second.ClueId), worldDay, saveRevision,
                request.ActorRef,
                new[] { "quest:" + _definition.QuestId, "clue:" + first.ClueId, "clue:" + second.ClueId,
                    "source:" + first.SourceRef, "source:" + second.SourceRef },
                "treasure_clues_cross_validated", "两条独立来源已确认同一寻宝目标。",
                new Dictionary<string, string>
                {
                    ["quest_id"] = _definition.QuestId,
                    ["clue_a"] = first.ClueId,
                    ["clue_b"] = second.ClueId,
                    ["source_a"] = first.SourceKind.ToString(),
                    ["source_b"] = second.SourceKind.ToString(),
                    ["reliability_a"] = first.ReliabilityPercent.ToString(),
                    ["reliability_b"] = second.ReliabilityPercent.ToString(),
                    ["cross_validated"] = "true",
                    ["puzzle_gate"] = "false",
                }, WorldEventStatus.Normal, null,
                new[] { EventIdFromRef(first.FactEventRef), EventIdFromRef(second.FactEventRef) });
            AddFact(fact);
            return D9TreasureQuestOutcome.Recorded(_state.Status, "treasure_clues_cross_validated",
                "线索已交叉验证，可按明确路线继续。", fact);
        }

        public D9TreasureQuestOutcome HearRumor(string actorRef, int worldDay, long saveRevision)
        {
            if (!HasActor(actorRef)) return Reject("actor_required", "必须有传闻见证者。" );
            if (_state.Status != D9TreasureQuestStatus.RumorAvailable)
                return D9TreasureQuestOutcome.Rejected(_state.Status, "rumor_already_heard", "这条传闻已经进入任务日志。" );

            var values = new Dictionary<string, string>
            {
                ["quest_id"] = _definition.QuestId,
                ["clue_id"] = _definition.Clue.Id,
                ["source_kind"] = _definition.Clue.SourceKind.ToString(),
                ["source_ref"] = _definition.Clue.SourceRef,
                ["evidence_ref"] = _definition.Clue.EvidenceRef,
                ["region_ref"] = _definition.Clue.RegionRef,
                ["reliability_percent"] = _definition.Clue.ReliabilityPercent.ToString(),
                ["site_ref"] = _definition.SiteRef,
                ["valid_from_world_day"] = _definition.Clue.ValidFromWorldDay.ToString(),
                ["valid_until_world_day"] = _definition.Clue.ValidUntilWorldDay.ToString(),
                ["reward_type"] = _definition.Clue.RewardType.ToString(),
                ["puzzle_gate"] = "false",
            };
            var fact = AppendFact(
                D9TreasureEventIds.Clue, worldDay, saveRevision, actorRef,
                new[] { "rumor:" + _definition.Clue.SourceRef },
                "rumor_received", "传闻已记录；可靠度和岸线范围进入任务快照。", values,
                WorldEventStatus.Normal, null, null);

            _state.Status = D9TreasureQuestStatus.RumorHeard;
            _state.RumorHeard = true;
            AddFact(fact);
            var primary = BuildClueRecord(_definition.Clue, _definition.QuestId);
            primary.FactEventRef = FactRef(fact);
            if (FindClue(primary.ClueId) == null) _state.Clues.Add(primary);
            return D9TreasureQuestOutcome.Recorded(_state.Status, "rumor_received", "已记录传闻和线索可靠度。", fact);
        }

        public D9TreasureQuestOutcome AcceptRumor()
        {
            if (!_state.RumorHeard)
                return Reject(D9TreasureFailureCodes.RumorNotHeard, "还没有可接受的传闻。" );
            if (_state.Status == D9TreasureQuestStatus.Discovered)
                return Reject(D9TreasureFailureCodes.AlreadyDiscovered, "这条线索已经完成发现。" );
            if (_state.Status == D9TreasureQuestStatus.Failed)
                return Reject(D9TreasureFailureCodes.RecoveryRequired, "请先选择一条失败恢复路线。" );
            _state.Status = D9TreasureQuestStatus.Accepted;
            return D9TreasureQuestOutcome.Recorded(_state.Status, "quest_accepted", "任务已接受；下一步是准备装备和路线。", null);
        }

        /// <summary>只检查装备，不写库存；InventorySystem 负责实际持有量。</summary>
        public IReadOnlyList<string> MissingEquipment(IEnumerable<string> equipmentIds)
        {
            return MissingEquipment(equipmentIds, null);
        }

        private IReadOnlyList<string> MissingEquipment(IEnumerable<string> equipmentIds, D9TreasureClueRecord clue)
        {
            var owned = new HashSet<string>(equipmentIds ?? new string[0], StringComparer.Ordinal);
            var missing = new List<string>();
            var requiredEquipment = clue == null || clue.RequiredEquipmentIds == null || clue.RequiredEquipmentIds.Count == 0
                ? _definition.RequiredEquipmentIds : clue.RequiredEquipmentIds;
            foreach (var required in requiredEquipment)
                if (!owned.Contains(required)) missing.Add(required);
            return missing;
        }

        public D9TreasureQuestOutcome TryLand(D9TreasureLandingRequest request, int worldDay, long saveRevision)
        {
            if (request == null) return Reject("request_required", "登陆请求不能为空。" );
            if (!HasActor(request.ActorRef)) return Reject("actor_required", "必须有登陆执行者。" );
            if (_state.Status == D9TreasureQuestStatus.Discovered)
                return Reject(D9TreasureFailureCodes.AlreadyDiscovered, "发现已经完成。" );
            if (_state.Status == D9TreasureQuestStatus.Failed)
                return Reject(D9TreasureFailureCodes.RecoveryRequired, "请先执行失败恢复路线。", FindRoute(_state.RecoveryRouteId));
            if (!_state.RumorHeard || (_state.Status != D9TreasureQuestStatus.Accepted &&
                _state.Status != D9TreasureQuestStatus.Recovered))
                return Reject(D9TreasureFailureCodes.RumorNotHeard, "必须先听取并接受传闻。" );

            var selectedClue = FindClue(string.IsNullOrWhiteSpace(request.ClueId)
                ? _definition.Clue.Id : request.ClueId);
            if (selectedClue == null)
                return Reject(D9TreasureFailureCodes.ClueRequired, "必须先记录一条有效的目标线索。");
            var missing = MissingEquipment(request.EquipmentIds, selectedClue);
            _state.EquipmentReady = missing.Count == 0;
            var inputFacts = BuildLandingInputFacts(request, missing);
            string failureCode = D9TreasureFailureCodes.None;
            D9TreasureRecoveryOption route = null;
            string resultCode = "treasure_discovered";
            string summary = "已在圣玛丽海图湾确认旧海图藏点；RewardRequest 等待 RewardSystem 裁决。";
            WorldEventStatus outcomeStatus = WorldEventStatus.Normal;

            if (request.RequireCrossValidation && !selectedClue.IsCrossValidated)
            {
                failureCode = D9TreasureFailureCodes.ClueNotVerified;
                resultCode = "treasure_attempt_failed_verification";
                summary = "线索尚未完成独立来源交叉验证；可获取另一来源后再尝试，不需要猜谜。";
                route = FindRoute("route_cross_validate_clue");
            }
            else if (request.EnforceClueWindow && !IsWithinClueWindow(selectedClue, worldDay))
            {
                failureCode = D9TreasureFailureCodes.ClueWindowClosed;
                resultCode = "treasure_attempt_failed_clue_window";
                summary = "当前不在线索有效时间窗内；可等待下一窗口或返回已知港口。";
                route = FindRoute("route_wait_tide") ?? FindRoute("route_return_to_harbor");
            }
            else if (missing.Count > 0)
            {
                failureCode = D9TreasureFailureCodes.EquipmentRequired;
                resultCode = "treasure_attempt_failed_equipment";
                summary = "登陆准备失败：缺少海图装备；可回港取得装备后重试。";
                route = FindRoute("route_buy_chart_kit");
            }
            else if (!string.Equals(request.LocationRef, _definition.SiteRef, StringComparison.Ordinal))
            {
                failureCode = D9TreasureFailureCodes.WrongLandingSite;
                resultCode = "treasure_attempt_failed_location";
                summary = "登陆点不是冻结的圣玛丽海图湾；可返回已知港口保留线索。";
                route = FindRoute("route_return_to_harbor");
            }
            else if (request.RouteChoice == D9TreasureRouteChoice.DirectLanding && HasDanger())
            {
                failureCode = D9TreasureFailureCodes.UnsafeLanding;
                resultCode = "treasure_attempt_failed_danger";
                summary = "直达登陆遇到冲浪断口和不稳定岸线；可等待低潮或寻求村落向导。";
                route = FindRoute("route_wait_tide");
            }
            else if (request.RouteChoice == D9TreasureRouteChoice.WaitForTide && !request.TideWindowOpen)
            {
                failureCode = D9TreasureFailureCodes.TideWindowClosed;
                resultCode = "treasure_attempt_failed_tide";
                summary = "当前不是低潮窗口；可以回港或在下一窗口重试。";
                route = FindRoute("route_return_to_harbor");
            }
            else if (request.RouteChoice == D9TreasureRouteChoice.VillageGuide && !request.VillageGuideAvailable)
            {
                failureCode = D9TreasureFailureCodes.GuideUnavailable;
                resultCode = "treasure_attempt_failed_guide";
                summary = "当前没有可用的村落向导；可以等待低潮或回港。";
                route = FindRoute("route_wait_tide");
            }

            if (failureCode != D9TreasureFailureCodes.None)
            {
                outcomeStatus = WorldEventStatus.Failed;
                _state.Status = D9TreasureQuestStatus.Failed;
                _state.LastFailureCode = failureCode;
                _state.RecoveryRouteId = route == null ? null : route.RouteId;
            }
            else
            {
                _state.Status = D9TreasureQuestStatus.Discovered;
                _state.LastFailureCode = D9TreasureFailureCodes.None;
                _state.RecoveryRouteId = null;
            }

            var values = BuildResolutionValues(request, selectedClue, missing, failureCode, route);
            // 失败事实已经通过 input_facts / result_values 记录本次尝试；恢复链只需要
            // 直接指向失败的 resolved 事实，避免把失败前的线索传闻再次展开成恢复原因。
            var causeEventIds = outcomeStatus == WorldEventStatus.Failed
                ? new string[0]
                : new[] { EventIdFromRef(selectedClue.FactEventRef) };
            var fact = AppendFact(
                D9TreasureEventIds.Resolved, worldDay, saveRevision, request.ActorRef,
                inputFacts, resultCode, summary, values, outcomeStatus, route,
                causeEventIds);
            AddFact(fact);
            _state.LastResolutionEventRef = FactRef(fact);

            D9TreasureRewardRequest rewardRequest = null;
            if (outcomeStatus == WorldEventStatus.Normal)
            {
                rewardRequest = new D9TreasureRewardRequest
                {
                    RequestId = _definition.RewardRequestId + "@" + fact.SaveRevision,
                    RewardId = _definition.RewardRequestId,
                    QuestId = _definition.QuestId,
                    SourceEventRef = FactRef(fact),
                    RewardType = selectedClue.RewardType,
                };
                rewardRequest.ClueIds.Add(selectedClue.ClueId);
                rewardRequest.CauseEventIds.Add(EventIdFromRef(selectedClue.FactEventRef));
                _state.RewardRequest = rewardRequest.Clone();
            }

            var outcome = D9TreasureQuestOutcome.Recorded(_state.Status, resultCode, summary, fact, rewardRequest, route);
            outcome.MissingEquipment = missing;
            // D12 的显式线索请求使用 canonical failure code；保留 D9 旧路线请求的
            // result code，兼容既有 UI/脚本对“尝试结果”的显示契约。
            if (outcomeStatus == WorldEventStatus.Failed && !string.IsNullOrWhiteSpace(request.ClueId))
                outcome.Code = failureCode;
            return outcome;
        }

        public D9TreasureQuestOutcome Recover(D9TreasureRecoveryRequest request, int worldDay, long saveRevision)
        {
            if (request == null) return Reject("request_required", "恢复请求不能为空。" );
            if (!HasActor(request.ActorRef)) return Reject("actor_required", "必须有恢复执行者。" );
            if (_state.Status != D9TreasureQuestStatus.Failed)
                return Reject(D9TreasureFailureCodes.RecoveryUnavailable, "当前没有可恢复的失败事实。" );

            var route = FindRoute(request.RouteId);
            if (route == null) return Reject(D9TreasureFailureCodes.RecoveryUnavailable, "未知的恢复路线。" );
            if (!string.IsNullOrEmpty(route.RequiredEquipmentId) &&
                !new HashSet<string>(request.EquipmentIds ?? new List<string>(), StringComparer.Ordinal).Contains(route.RequiredEquipmentId))
                return Reject(D9TreasureFailureCodes.EquipmentRequired, "恢复路线仍需要指定装备。", route);
            if (route.RouteId == "route_wait_tide" && !request.TideWindowOpen)
                return Reject(D9TreasureFailureCodes.TideWindowClosed, "等待低潮路线尚未进入窗口。", route);
            if (route.RouteId == "route_village_guide" && !request.VillageGuideAvailable)
                return Reject(D9TreasureFailureCodes.GuideUnavailable, "当前没有可用的村落向导。", route);
            if (route.RouteId == "route_cross_validate_clue")
            {
                var clue = FindClue(request.ClueId);
                if (clue == null || !clue.IsCrossValidated)
                    return Reject(D9TreasureFailureCodes.ClueNotVerified, "恢复前必须完成另一独立来源的交叉验证。", route);
            }

            var values = new Dictionary<string, string>
            {
                ["quest_id"] = _definition.QuestId,
                ["recovery_route_id"] = route.RouteId,
                ["resume_from_event_ref"] = _state.LastResolutionEventRef ?? string.Empty,
                ["equipment_ready"] = (MissingEquipment(request.EquipmentIds).Count == 0).ToString().ToLowerInvariant(),
                ["danger_tags"] = string.Join(",", _definition.DangerTags.ToArray()),
                ["clue_id"] = request.ClueId ?? _definition.Clue.Id,
            };
            var fact = AppendFact(
                D9TreasureEventIds.Recovered, worldDay, saveRevision, request.ActorRef,
                new[] { "recovery_route:" + route.RouteId }, "treasure_recovered", route.Description,
                values, WorldEventStatus.Recovered, null, new[] { D9TreasureEventIds.Resolved });

            _state.Status = D9TreasureQuestStatus.Recovered;
            _state.LastFailureCode = D9TreasureFailureCodes.None;
            _state.RecoveryRouteId = null;
            _state.LastRecoveryEventRef = FactRef(fact);
            _state.EquipmentReady = MissingEquipment(request.EquipmentIds).Count == 0;
            AddFact(fact);
            return D9TreasureQuestOutcome.Recorded(_state.Status, "treasure_recovered", route.Description, fact);
        }

        public D9TreasureSaveEnvelope CaptureSaveEnvelope()
        {
            return new D9TreasureSaveEnvelope
            {
                SaveVersion = SnapshotVersion,
                Quest = Snapshot,
                EventLog = _eventLog.CaptureSnapshot(),
            };
        }

        public static D9TreasureQuestSystem RestoreFromSave(D9TreasureSaveEnvelope envelope,
            D9TreasureQuestDefinition definition = null)
        {
            if (envelope == null) throw new ArgumentNullException("envelope");
            if (envelope.SaveVersion != SnapshotVersion)
                throw new InvalidOperationException("D9 treasure save_version mismatch: expected " + SnapshotVersion + ".");
            if (envelope.EventLog == null) throw new InvalidOperationException("D9 treasure save is missing EventLog snapshot.");
            if (envelope.Quest == null) throw new InvalidOperationException("D9 treasure save is missing Quest snapshot.");
            return new D9TreasureQuestSystem(EventLog.FromSnapshot(envelope.EventLog), definition, envelope.Quest);
        }

        private static D9TreasureQuestSnapshot NewSnapshot(D9TreasureQuestDefinition definition)
        {
            var result = new D9TreasureQuestSnapshot
            {
                SchemaVersion = SnapshotVersion,
                QuestId = definition.QuestId,
                Status = D9TreasureQuestStatus.RumorAvailable,
                ClueReliabilityPercent = definition.Clue.ReliabilityPercent,
                LastFailureCode = D9TreasureFailureCodes.None,
            };
            result.DangerTags.AddRange(definition.DangerTags);
            return result;
        }

        private D9TreasureClueRecord FindClue(string clueId)
        {
            if (string.IsNullOrWhiteSpace(clueId) || _state.Clues == null) return null;
            foreach (var clue in _state.Clues)
                if (clue != null && string.Equals(clue.ClueId, clueId, StringComparison.Ordinal)) return clue;
            return null;
        }

        private D9TreasureClueDefinition FindDefinitionClue(string clueId)
        {
            if (string.Equals(_definition.Clue.Id, clueId, StringComparison.Ordinal)) return _definition.Clue.Clone();
            if (_definition.AdditionalClues != null)
                foreach (var clue in _definition.AdditionalClues)
                    if (clue != null && string.Equals(clue.Id, clueId, StringComparison.Ordinal)) return clue.Clone();
            return null;
        }

        private static D9TreasureClueRecord BuildClueRecord(D9TreasureClueDefinition definition, string questId)
        {
            var result = new D9TreasureClueRecord
            {
                ClueId = definition.Id,
                QuestId = questId,
                SourceKind = definition.SourceKind,
                SourceRef = definition.SourceRef,
                EvidenceRef = definition.EvidenceRef,
                Text = definition.Text,
                ReliabilityPercent = definition.ReliabilityPercent,
                RegionRef = definition.RegionRef,
                SiteRef = definition.SiteRef,
                ValidFromWorldDay = definition.ValidFromWorldDay,
                ValidUntilWorldDay = definition.ValidUntilWorldDay,
                AlternativeRouteId = definition.AlternativeRouteId,
                RewardType = definition.RewardType,
            };
            if (definition.RequiredEquipmentIds != null)
                result.RequiredEquipmentIds.AddRange(definition.RequiredEquipmentIds);
            return result;
        }

        private List<string> BuildClueInputFacts(D9TreasureClueRecord clue)
        {
            return new List<string>
            {
                "quest:" + _definition.QuestId,
                "clue:" + clue.ClueId,
                "source:" + clue.SourceRef,
                "evidence:" + clue.EvidenceRef,
                "site:" + clue.SiteRef,
            };
        }

        private static Dictionary<string, string> BuildClueValues(D9TreasureClueRecord clue)
        {
            return new Dictionary<string, string>
            {
                ["quest_id"] = clue.QuestId,
                ["clue_id"] = clue.ClueId,
                ["source_kind"] = clue.SourceKind.ToString(),
                ["source_ref"] = clue.SourceRef,
                ["evidence_ref"] = clue.EvidenceRef,
                ["region_ref"] = clue.RegionRef,
                ["site_ref"] = clue.SiteRef,
                ["reliability_percent"] = clue.ReliabilityPercent.ToString(),
                ["valid_from_world_day"] = clue.ValidFromWorldDay.ToString(),
                ["valid_until_world_day"] = clue.ValidUntilWorldDay.ToString(),
                ["required_equipment_ids"] = string.Join(",", clue.RequiredEquipmentIds.ToArray()),
                ["alternative_route_id"] = clue.AlternativeRouteId ?? string.Empty,
                ["reward_type"] = clue.RewardType.ToString(),
                ["puzzle_gate"] = "false",
            };
        }

        private static string BuildClueEventId(string clueId)
        {
            return "event_d12_treasure_clue_" + SanitizeEventPart(clueId);
        }

        private static string BuildCrossValidationEventId(string first, string second)
        {
            if (string.CompareOrdinal(first, second) > 0)
            {
                var swap = first;
                first = second;
                second = swap;
            }
            return "event_d12_treasure_cross_validate_" + SanitizeEventPart(first) + "_" + SanitizeEventPart(second);
        }

        private static string SanitizeEventPart(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unknown";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-') chars[i] = '_';
            return new string(chars);
        }

        private static string EventIdFromRef(string factRef)
        {
            if (string.IsNullOrWhiteSpace(factRef)) return string.Empty;
            int at = factRef.IndexOf('@');
            return at < 0 ? factRef : factRef.Substring(0, at);
        }

        private static bool IsWithinClueWindow(D9TreasureClueRecord clue, int worldDay)
        {
            if (clue == null) return false;
            if (clue.ValidFromWorldDay >= 0 && worldDay < clue.ValidFromWorldDay) return false;
            if (clue.ValidUntilWorldDay >= 0 && worldDay > clue.ValidUntilWorldDay) return false;
            return true;
        }

        private static D9TreasureQuestSnapshot RestoreSnapshot(D9TreasureQuestSnapshot source,
            D9TreasureQuestDefinition definition)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (source.SchemaVersion != SnapshotVersion)
                throw new InvalidOperationException("D9 treasure quest schema_version mismatch: expected " + SnapshotVersion + ".");
            if (!string.Equals(source.QuestId, definition.QuestId, StringComparison.Ordinal))
                throw new InvalidOperationException("D9 treasure quest id does not match the definition.");
            var result = source.Clone();
            if (result.DangerTags.Count == 0) result.DangerTags.AddRange(definition.DangerTags);
            if (result.Clues == null) result.Clues = new List<D9TreasureClueRecord>();
            if (result.VerifiedClueIds == null) result.VerifiedClueIds = new List<string>();
            if (result.RumorHeard && result.Clues.Count == 0)
            {
                var legacyPrimary = BuildClueRecord(definition.Clue, definition.QuestId);
                legacyPrimary.FactEventRef = D9TreasureEventIds.Clue + "@legacy";
                result.Clues.Add(legacyPrimary);
            }
            result.Validate();
            return result;
        }

        private D9TreasureQuestOutcome Reject(string code, string message,
            D9TreasureRecoveryOption route = null, IReadOnlyList<string> missing = null)
        {
            return D9TreasureQuestOutcome.Rejected(_state.Status, code, message, route, missing);
        }

        private static bool HasActor(string actorRef)
        {
            return !string.IsNullOrWhiteSpace(actorRef);
        }

        private bool HasDanger()
        {
            return _definition.DangerTags.Count > 0;
        }

        private D9TreasureRecoveryOption FindRoute(string routeId)
        {
            if (string.IsNullOrEmpty(routeId)) return null;
            foreach (var route in _definition.RecoveryOptions)
                if (route.RouteId == routeId) return route.Clone();
            return null;
        }

        private List<string> BuildLandingInputFacts(D9TreasureLandingRequest request,
            IReadOnlyList<string> missing)
        {
            string clueId = string.IsNullOrWhiteSpace(request.ClueId) ? _definition.Clue.Id : request.ClueId;
            var facts = new List<string>
            {
                "quest:" + _definition.QuestId,
                "clue:" + clueId,
                "site:" + request.LocationRef,
                "route:" + request.RouteChoice,
            };
            foreach (var equipment in request.EquipmentIds ?? new List<string>())
                if (!string.IsNullOrWhiteSpace(equipment)) facts.Add("equipment:" + equipment);
            foreach (var missingEquipment in missing) facts.Add("missing_equipment:" + missingEquipment);
            foreach (var danger in _definition.DangerTags) facts.Add("danger:" + danger);
            return facts;
        }

        private Dictionary<string, string> BuildResolutionValues(D9TreasureLandingRequest request,
            D9TreasureClueRecord clue, IReadOnlyList<string> missing, string failureCode,
            D9TreasureRecoveryOption route)
        {
            var values = new Dictionary<string, string>
            {
                ["quest_id"] = _definition.QuestId,
                ["clue_id"] = clue == null ? _definition.Clue.Id : clue.ClueId,
                ["site_ref"] = _definition.SiteRef,
                ["route_choice"] = request.RouteChoice.ToString(),
                ["clue_reliability_percent"] = clue == null ? _state.ClueReliabilityPercent.ToString() : clue.ReliabilityPercent.ToString(),
                ["clue_source_kind"] = clue == null ? string.Empty : clue.SourceKind.ToString(),
                ["cross_validated"] = clue != null && clue.IsCrossValidated ? "true" : "false",
                ["clue_window_enforced"] = request.EnforceClueWindow ? "true" : "false",
                ["equipment_ready"] = (missing.Count == 0).ToString().ToLowerInvariant(),
                ["danger_tags"] = string.Join(",", _definition.DangerTags.ToArray()),
                ["danger_outcome"] = failureCode == D9TreasureFailureCodes.UnsafeLanding ? "encountered" : "managed",
                ["failure_code"] = failureCode,
                ["recovery_route_id"] = route == null ? string.Empty : route.RouteId,
                ["puzzle_gate"] = "false",
            };
            if (failureCode == D9TreasureFailureCodes.None)
                values["reward_request_id"] = _definition.RewardRequestId;
            return values;
        }

        private FactEvent AppendFact(string eventId, int worldDay, long saveRevision,
            string actorRef, IEnumerable<string> inputFacts, string resultCode, string summary,
            Dictionary<string, string> values, WorldEventStatus status,
            D9TreasureRecoveryOption route, IEnumerable<string> causeIds)
        {
            var worldEvent = new WorldEvent
            {
                EventId = eventId,
                SchemaVersion = EventSchema.CurrentVersion,
                WorldDay = Math.Max(0, worldDay),
                LocationRef = _definition.SiteRef,
                ActorIds = new List<string> { actorRef },
                InputFacts = new List<string>(inputFacts ?? new string[0]),
                OwnerSystem = "QuestSystem",
                Result = new EventResult
                {
                    ResultCode = resultCode,
                    Summary = summary,
                    Values = values ?? new Dictionary<string, string>(),
                },
                CauseIds = new List<string>(causeIds ?? new string[0]),
                SaveRevision = Math.Max(0L, saveRevision),
                OutcomeStatus = status,
                RecoveryRoute = route == null ? null : new RecoveryRoute
                {
                    RouteId = route.RouteId,
                    Action = route.Action,
                    ResumeFromEventId = D9TreasureEventIds.Resolved,
                    Description = route.Description,
                },
            };
            return _eventLog.Append(worldEvent).Fact;
        }

        private void AddFact(FactEvent fact)
        {
            if (fact == null) throw new InvalidOperationException("D9 treasure event append returned no fact.");
            string reference = FactRef(fact);
            if (!_state.FactEventRefs.Contains(reference)) _state.FactEventRefs.Add(reference);
        }

        private static string FactRef(FactEvent fact)
        {
            return fact.EventId + "@" + fact.SaveRevision;
        }
    }
}
