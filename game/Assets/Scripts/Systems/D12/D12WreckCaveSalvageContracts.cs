// ============================================================
// D12WreckCaveSalvageContracts —— 沉船、洞穴与打捞契约
//
// D12WreckCaveSalvageSystem 是沉船、洞穴入口和打捞运行时状态的唯一写者。
// 法律裁决通过只读 authority 接口注入；Inventory/Ship/Crew/Reward/Faction
// 仍由各自 owner 写入，D12 只保存输入证据、事实事件、损失和请求。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D12
{
    public static class D12SalvageIds
    {
        public const string EventPrefix = "event_d12_salvage_";
        public const string WreckMapFeatureId = "wreck_saint_marys_chart_cove";
        public const string CaveMapFeatureId = "cave_green_cay_tide_cave";
        public const string GrapnelToolId = "item_salvage_grapnel";
        public const string RopeToolId = "item_salvage_rope";
        public const string LanternToolId = "item_salvage_lantern";
        public const string OilToolId = "item_salvage_oil";
        public const string PumpToolId = "item_salvage_pump";
        public const string CaveMarkerToolId = "item_cave_markers";
    }

    public enum D12ExplorationSiteType
    {
        Wreck,
        Cave,
    }

    public enum D12TideStage
    {
        Unknown,
        Low,
        Falling,
        Rising,
        High,
    }

    public enum D12SalvageStatus
    {
        Available,
        Failed,
        Recovered,
        Salvaged,
    }

    public enum D12SalvageFailureCode
    {
        None,
        SiteNotCharted,
        TideWindowClosed,
        EquipmentRequired,
        RopeTooShort,
        LightInsufficient,
        LegalRulingRequired,
        LegalDenied,
        RiskEncountered,
        CompetitorInterference,
        CaveCollapse,
        FallOrWaterInrush,
        AlreadySalvaged,
        RecoveryRequired,
        RecoveryUnavailable,
    }

    public enum D12SalvageLossKind
    {
        None,
        LineSlipped,
        WaterInrush,
        CaveCollapse,
        FallRisk,
        CompetitorDispute,
        AccessDelayed,
    }

    [Serializable]
    public sealed class D12SalvageRecoveryOption
    {
        public string RouteId;
        public string Action;
        public string Description;
        public string RequiredEquipmentId;
        public D12TideStage RequiredTideStage = D12TideStage.Unknown;

        public D12SalvageRecoveryOption Clone()
        {
            return new D12SalvageRecoveryOption
            {
                RouteId = RouteId,
                Action = Action,
                Description = Description,
                RequiredEquipmentId = RequiredEquipmentId,
                RequiredTideStage = RequiredTideStage,
            };
        }

        public void Validate(string siteId)
        {
            Require(RouteId, "RouteId", siteId);
            Require(Action, "Action", siteId);
            Require(Description, "Description", siteId);
            if (!Enum.IsDefined(typeof(D12TideStage), RequiredTideStage))
                throw new InvalidOperationException("恢复路线潮汐阶段非法：" + RouteId);
        }

        private static void Require(string value, string field, string siteId)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("沉船/洞穴 " + siteId + " 缺少恢复路线 " + field + "。");
        }
    }

    [Serializable]
    public sealed class D12ExplorationSiteDefinition
    {
        public string SiteId;
        public D12ExplorationSiteType Type;
        public string MapFeatureId;
        public string LocationRef;
        public string RegionRef;
        public string DisplayName;
        public string ClaimRef;
        public string RewardRequestId;
        public string CompetitorRef;
        public D12TideStage PreferredTideStage;
        public int MinimumRopeMeters;
        public int RequiredLightHours;
        public int BaseRiskPercent;
        public int CollapseRiskPercent;
        public int WaterRiskPercent;
        public int CompetitorRiskPercent;
        public List<string> RequiredEquipmentIds = new List<string>();
        public List<D12SalvageRecoveryOption> RecoveryOptions = new List<D12SalvageRecoveryOption>();
        public string SourceNote = "D12-A02 沉船/洞穴纵切片";
        public string HistoricalTag = "inferred";
        public string OwnerSystem = "ExplorationSystem";
        public string TestId = "D12-A02-SALVAGE";

        public D12ExplorationSiteDefinition Clone()
        {
            var result = new D12ExplorationSiteDefinition
            {
                SiteId = SiteId,
                Type = Type,
                MapFeatureId = MapFeatureId,
                LocationRef = LocationRef,
                RegionRef = RegionRef,
                DisplayName = DisplayName,
                ClaimRef = ClaimRef,
                RewardRequestId = RewardRequestId,
                CompetitorRef = CompetitorRef,
                PreferredTideStage = PreferredTideStage,
                MinimumRopeMeters = MinimumRopeMeters,
                RequiredLightHours = RequiredLightHours,
                BaseRiskPercent = BaseRiskPercent,
                CollapseRiskPercent = CollapseRiskPercent,
                WaterRiskPercent = WaterRiskPercent,
                CompetitorRiskPercent = CompetitorRiskPercent,
                SourceNote = SourceNote,
                HistoricalTag = HistoricalTag,
                OwnerSystem = OwnerSystem,
                TestId = TestId,
            };
            if (RequiredEquipmentIds != null) result.RequiredEquipmentIds.AddRange(RequiredEquipmentIds);
            if (RecoveryOptions != null)
                foreach (var option in RecoveryOptions)
                    if (option != null) result.RecoveryOptions.Add(option.Clone());
            return result;
        }

        public void Validate()
        {
            Require(SiteId, "SiteId");
            Require(MapFeatureId, "MapFeatureId");
            Require(LocationRef, "LocationRef");
            Require(RegionRef, "RegionRef");
            Require(ClaimRef, "ClaimRef");
            Require(RewardRequestId, "RewardRequestId");
            Require(OwnerSystem, "OwnerSystem");
            Require(TestId, "TestId");
            if (!Enum.IsDefined(typeof(D12ExplorationSiteType), Type))
                throw new InvalidOperationException("探索地点类型非法：" + SiteId);
            if (!Enum.IsDefined(typeof(D12TideStage), PreferredTideStage) || PreferredTideStage == D12TideStage.Unknown)
                throw new InvalidOperationException("探索地点必须登记有效潮汐窗口：" + SiteId);
            if (MinimumRopeMeters < 0 || RequiredLightHours < 0)
                throw new InvalidOperationException("探索地点装备边界不能为负数：" + SiteId);
            ValidatePercent(BaseRiskPercent, "BaseRiskPercent");
            ValidatePercent(CollapseRiskPercent, "CollapseRiskPercent");
            ValidatePercent(WaterRiskPercent, "WaterRiskPercent");
            ValidatePercent(CompetitorRiskPercent, "CompetitorRiskPercent");
            if (RequiredEquipmentIds == null || RequiredEquipmentIds.Count == 0)
                throw new InvalidOperationException("探索地点必须登记装备前置：" + SiteId);
            if (RecoveryOptions == null || RecoveryOptions.Count < 2)
                throw new InvalidOperationException("探索地点至少需要两条恢复路线：" + SiteId);
            var equipment = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in RequiredEquipmentIds)
                if (string.IsNullOrWhiteSpace(id) || !equipment.Add(id))
                    throw new InvalidOperationException("探索地点装备前置重复或为空：" + SiteId);
            var routes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var route in RecoveryOptions)
            {
                if (route == null) throw new InvalidOperationException("恢复路线不能为 null：" + SiteId);
                route.Validate(SiteId);
                if (!routes.Add(route.RouteId))
                    throw new InvalidOperationException("恢复路线重复：" + route.RouteId);
            }
        }

        private static void ValidatePercent(int value, string field)
        {
            if (value < 0 || value > 100) throw new InvalidOperationException(field + " 必须在 0..100。");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " 不能为空。");
        }
    }

    [Serializable]
    public sealed class D12SalvageCompetitorObservation
    {
        public bool Present;
        public string CompetitorRef;
        public string ClaimRef;
        public string EvidenceRef;
        public int PressurePercent;

        public D12SalvageCompetitorObservation Clone()
        {
            return new D12SalvageCompetitorObservation
            {
                Present = Present,
                CompetitorRef = CompetitorRef,
                ClaimRef = ClaimRef,
                EvidenceRef = EvidenceRef,
                PressurePercent = PressurePercent,
            };
        }
    }

    [Serializable]
    public sealed class D12SalvageLegalRequest
    {
        public string SiteId;
        public string ActorRef;
        public string ClaimRef;
        public string CompetitorClaimRef;
        public string EvidenceRef;
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D12SalvageLegalDecision
    {
        public bool RulingAvailable;
        public bool SalvageAllowed;
        public string ClaimStatus;
        public string RulingRef;
        public string ClaimantRef;
        public string ControllingFactionId;
        public string PermitRef;
        public string Reason;

        public D12SalvageLegalDecision Clone()
        {
            return new D12SalvageLegalDecision
            {
                RulingAvailable = RulingAvailable,
                SalvageAllowed = SalvageAllowed,
                ClaimStatus = ClaimStatus,
                RulingRef = RulingRef,
                ClaimantRef = ClaimantRef,
                ControllingFactionId = ControllingFactionId,
                PermitRef = PermitRef,
                Reason = Reason,
            };
        }

        public void Validate(string siteId)
        {
            if (!RulingAvailable) throw new InvalidOperationException("法律裁决尚未提供：" + siteId);
            if (string.IsNullOrWhiteSpace(RulingRef)) throw new InvalidOperationException("法律裁决缺少 ruling_ref：" + siteId);
            if (string.IsNullOrWhiteSpace(ClaimStatus)) throw new InvalidOperationException("法律裁决缺少 claim_status：" + siteId);
            if (string.IsNullOrWhiteSpace(Reason)) throw new InvalidOperationException("法律裁决缺少理由：" + siteId);
        }
    }

    /// <summary>由 Faction/Legal owner 注入；D12 不能实现或推断产权裁决。</summary>
    public interface ID12SalvageLegalAuthority
    {
        D12SalvageLegalDecision Rule(D12SalvageLegalRequest request);
    }

    [Serializable]
    public sealed class D12SalvageRequest
    {
        public string AttemptId;
        public string SiteId;
        public string ActorRef = "player";
        public string LocationRef;
        public int WorldDay;
        public long SaveRevision;
        public bool TideWindowOpen;
        public D12TideStage TideStage = D12TideStage.Unknown;
        public List<string> EquipmentIds = new List<string>();
        public int RopeMeters;
        public int LightHoursAvailable;
        public int RiskSeed;
        public D12SalvageCompetitorObservation Competitor = new D12SalvageCompetitorObservation();
    }

    [Serializable]
    public sealed class D12SalvageRecoveryRequest
    {
        public string RecoveryAttemptId;
        public string SiteId;
        public string ActorRef = "player";
        public string RouteId;
        public string LocationRef;
        public int WorldDay;
        public long SaveRevision;
        public bool TideWindowOpen;
        public D12TideStage TideStage = D12TideStage.Unknown;
        public List<string> EquipmentIds = new List<string>();
    }

    [Serializable]
    public sealed class D12SalvageLossRecord
    {
        public string LossId;
        public D12SalvageLossKind Kind;
        public string Summary;
        public int Severity;
        public string RecoveryRouteId;
        public string EventId;
        public int WorldDay;

        public D12SalvageLossRecord Clone()
        {
            return new D12SalvageLossRecord
            {
                LossId = LossId,
                Kind = Kind,
                Summary = Summary,
                Severity = Severity,
                RecoveryRouteId = RecoveryRouteId,
                EventId = EventId,
                WorldDay = WorldDay,
            };
        }

        public void Validate(string siteId)
        {
            if (string.IsNullOrWhiteSpace(LossId)) throw new InvalidOperationException("损失记录缺少 loss_id：" + siteId);
            if (!Enum.IsDefined(typeof(D12SalvageLossKind), Kind)) throw new InvalidOperationException("损失类型非法：" + LossId);
            if (string.IsNullOrWhiteSpace(Summary)) throw new InvalidOperationException("损失记录缺少摘要：" + LossId);
            if (Severity < 0 || Severity > 100) throw new InvalidOperationException("损失严重度非法：" + LossId);
            if (string.IsNullOrWhiteSpace(EventId)) throw new InvalidOperationException("损失记录缺少事实事件：" + LossId);
            if (WorldDay < 0) throw new InvalidOperationException("损失记录日期非法：" + LossId);
        }
    }

    [Serializable]
    public sealed class D12SalvageRewardRequest
    {
        public string RequestId;
        public string RewardId;
        public string SiteId;
        public string SourceEventRef;
        public string Status = "pending";

        public D12SalvageRewardRequest Clone()
        {
            return new D12SalvageRewardRequest
            {
                RequestId = RequestId,
                RewardId = RewardId,
                SiteId = SiteId,
                SourceEventRef = SourceEventRef,
                Status = Status,
            };
        }

        public void Validate(string siteId)
        {
            if (string.IsNullOrWhiteSpace(RequestId) || string.IsNullOrWhiteSpace(RewardId) ||
                string.IsNullOrWhiteSpace(SourceEventRef))
                throw new InvalidOperationException("奖励请求字段不完整：" + siteId);
        }
    }

    [Serializable]
    public sealed class D12SalvageSiteState
    {
        public string SiteId;
        public D12SalvageStatus Status = D12SalvageStatus.Available;
        public int AttemptCount;
        public string LastFailureCode = D12SalvageFailureCode.None.ToString();
        public string LastRecoveryRouteId;
        public string LastResolutionEventRef;
        public string LastFailureEventRef;
        public string LastRecoveryEventRef;
        public string LastCompetitorRef;
        public int LastCompetitorPressurePercent;
        public D12SalvageRewardRequest RewardRequest;
        public List<D12SalvageLossRecord> LossRecords = new List<D12SalvageLossRecord>();
        public List<string> FactEventRefs = new List<string>();

        public D12SalvageSiteState Clone()
        {
            var result = new D12SalvageSiteState
            {
                SiteId = SiteId,
                Status = Status,
                AttemptCount = AttemptCount,
                LastFailureCode = LastFailureCode,
                LastRecoveryRouteId = LastRecoveryRouteId,
                LastResolutionEventRef = LastResolutionEventRef,
                LastFailureEventRef = LastFailureEventRef,
                LastRecoveryEventRef = LastRecoveryEventRef,
                LastCompetitorRef = LastCompetitorRef,
                LastCompetitorPressurePercent = LastCompetitorPressurePercent,
                RewardRequest = RewardRequest == null ? null : RewardRequest.Clone(),
            };
            if (LossRecords != null)
                foreach (var record in LossRecords)
                    if (record != null) result.LossRecords.Add(record.Clone());
            if (FactEventRefs != null) result.FactEventRefs.AddRange(FactEventRefs);
            return result;
        }
    }

    [Serializable]
    public sealed class D12SalvageSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public int LastWorldDay;
        public long Revision;
        public string LastEventId;
        public List<D12SalvageSiteState> Sites = new List<D12SalvageSiteState>();

        public D12SalvageSnapshot Clone()
        {
            var result = new D12SalvageSnapshot
            {
                SchemaVersion = SchemaVersion,
                LastWorldDay = LastWorldDay,
                Revision = Revision,
                LastEventId = LastEventId,
            };
            if (Sites != null)
                foreach (var site in Sites)
                    if (site != null) result.Sites.Add(site.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D12 打捞快照 schema_version 不匹配：收到 " + SchemaVersion + "。");
            if (LastWorldDay < 0 || Revision < 0)
                throw new InvalidOperationException("D12 打捞快照时间或 revision 非法。");
            var siteIds = new HashSet<string>(StringComparer.Ordinal);
            if (Sites == null) return;
            foreach (var site in Sites)
            {
                if (site == null || string.IsNullOrWhiteSpace(site.SiteId))
                    throw new InvalidOperationException("D12 打捞快照站点不能为空。");
                if (!siteIds.Add(site.SiteId)) throw new InvalidOperationException("D12 打捞站点重复：" + site.SiteId);
                if (!Enum.IsDefined(typeof(D12SalvageStatus), site.Status)) throw new InvalidOperationException("D12 打捞状态非法：" + site.SiteId);
                if (site.AttemptCount < 0 || site.LastCompetitorPressurePercent < 0 || site.LastCompetitorPressurePercent > 100)
                    throw new InvalidOperationException("D12 打捞运行时数值非法：" + site.SiteId);
                var refs = new HashSet<string>(StringComparer.Ordinal);
                if (site.FactEventRefs != null)
                    foreach (var reference in site.FactEventRefs)
                        if (string.IsNullOrWhiteSpace(reference) || !refs.Add(reference)) throw new InvalidOperationException("D12 打捞事实引用重复：" + site.SiteId);
                var losses = new HashSet<string>(StringComparer.Ordinal);
                if (site.LossRecords != null)
                    foreach (var loss in site.LossRecords)
                    {
                        if (loss == null) throw new InvalidOperationException("D12 打捞损失记录不能为空：" + site.SiteId);
                        loss.Validate(site.SiteId);
                        if (!losses.Add(loss.LossId)) throw new InvalidOperationException("D12 打捞损失重复：" + loss.LossId);
                    }
                if (site.RewardRequest != null) site.RewardRequest.Validate(site.SiteId);
                if (site.Status == D12SalvageStatus.Salvaged && site.RewardRequest == null)
                    throw new InvalidOperationException("已完成打捞必须保留 RewardRequest：" + site.SiteId);
            }
        }
    }

    [Serializable]
    public sealed class D12SalvageResult
    {
        public bool Accepted;
        public bool Duplicate;
        public bool Succeeded;
        public bool Recovered;
        public string Code;
        public string Message;
        public string SiteId;
        public D12SalvageStatus Status;
        public D12SalvageFailureCode FailureCode;
        public int RiskRoll;
        public int RiskThreshold;
        public D12SalvageRecoveryOption RecoveryRoute;
        public D12SalvageLossRecord Loss;
        public D12SalvageRewardRequest RewardRequest;
        public D12SalvageLegalDecision LegalDecision;
        public Whalebone.Systems.Events.FactEvent Fact;

        public D12SalvageResult Clone()
        {
            return new D12SalvageResult
            {
                Accepted = Accepted,
                Duplicate = Duplicate,
                Succeeded = Succeeded,
                Recovered = Recovered,
                Code = Code,
                Message = Message,
                SiteId = SiteId,
                Status = Status,
                FailureCode = FailureCode,
                RiskRoll = RiskRoll,
                RiskThreshold = RiskThreshold,
                RecoveryRoute = RecoveryRoute == null ? null : RecoveryRoute.Clone(),
                Loss = Loss == null ? null : Loss.Clone(),
                RewardRequest = RewardRequest == null ? null : RewardRequest.Clone(),
                LegalDecision = LegalDecision == null ? null : LegalDecision.Clone(),
                Fact = Fact == null ? null : Fact.CloneFact(),
            };
        }
    }

    [Serializable]
    public sealed class D12SalvageSiteView
    {
        public string SiteId;
        public D12ExplorationSiteType Type;
        public string DisplayName;
        public D12SalvageStatus Status;
        public int AttemptCount;
        public string LastFailureCode;
        public string LastRecoveryRouteId;
        public int LossCount;
        public bool RewardRequested;

        public D12SalvageSiteView Clone()
        {
            return new D12SalvageSiteView
            {
                SiteId = SiteId,
                Type = Type,
                DisplayName = DisplayName,
                Status = Status,
                AttemptCount = AttemptCount,
                LastFailureCode = LastFailureCode,
                LastRecoveryRouteId = LastRecoveryRouteId,
                LossCount = LossCount,
                RewardRequested = RewardRequested,
            };
        }
    }

    [Serializable]
    public sealed class D12SalvageReadModel
    {
        public int SchemaVersion = D12SalvageSnapshot.CurrentSchemaVersion;
        public List<D12SalvageSiteView> VisibleSites = new List<D12SalvageSiteView>();

        public D12SalvageReadModel Clone()
        {
            var result = new D12SalvageReadModel { SchemaVersion = SchemaVersion };
            if (VisibleSites != null)
                foreach (var site in VisibleSites)
                    if (site != null) result.VisibleSites.Add(site.Clone());
            return result;
        }
    }
}
