// ============================================================
// D10TavernContracts —— 酒馆社会场景请求与只读目录
//
// TavernSystem 只验证/发起请求和提供来源标注的目录，不拥有金钱、健康、
// 疲劳或关系数值。结果必须交给 Economy/Crew/Relation/Quest owner 裁决。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Whalebone.Systems.D10
{
    public enum D10TavernAction
    {
        Enter,
        Recruit,
        Rest,
        Meal,
        DrinkAlcohol,
        HearRumor,
        PokerEntry,
        Dance,
        NightTalk,
        SideJobBoard,
    }

    public enum D10TavernSensitivity
    {
        None,
        Alcohol,
        Gambling,
        Intimacy,
        ExploitativeWork,
    }

    public enum D10TavernRequestStatus
    {
        Accepted,
        Duplicate,
        WarningRequired,
        Skipped,
        Rejected,
    }

    public enum D10AlcoholStrength
    {
        Light,
        Strong,
        Binge,
    }

    public enum D10RumorReliability
    {
        Low,
        Mixed,
        High,
    }

    [Serializable]
    public sealed class D10TavernAlcoholEffect
    {
        public string ProfileId = string.Empty;
        public D10AlcoholStrength Strength;
        public int PriceQuote;
        public float HealthDelta;
        public float FatigueDelta;
        public float ConflictRisk;
        public string ConsequenceSummary = string.Empty;

        public D10TavernAlcoholEffect Clone()
        {
            return new D10TavernAlcoholEffect
            {
                ProfileId = ProfileId,
                Strength = Strength,
                PriceQuote = PriceQuote,
                HealthDelta = HealthDelta,
                FatigueDelta = FatigueDelta,
                ConflictRisk = ConflictRisk,
                ConsequenceSummary = ConsequenceSummary,
            };
        }
    }

    [Serializable]
    public sealed class D10TavernRumor
    {
        public string RumorId = string.Empty;
        public string Text = string.Empty;
        public string SourceRef = string.Empty;
        public string SourceKind = string.Empty;
        public float Reliability;
        public D10RumorReliability ReliabilityBand;
        public string ReliabilityLabel = string.Empty;
        public List<string> Tags = new List<string>();

        public D10TavernRumor Clone()
        {
            var result = new D10TavernRumor
            {
                RumorId = RumorId,
                Text = Text,
                SourceRef = SourceRef,
                SourceKind = SourceKind,
                Reliability = Reliability,
                ReliabilityBand = ReliabilityBand,
                ReliabilityLabel = ReliabilityLabel,
            };
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }
    }

    [Serializable]
    public sealed class D10SideJobOffer
    {
        public string OfferId = string.Empty;
        public string IssuerRef = string.Empty;
        public string JobType = string.Empty;
        public string Summary = string.Empty;
        public List<string> PrerequisiteTags = new List<string>();
        public List<string> RiskTags = new List<string>();
        public List<string> RewardTags = new List<string>();
        public int ExpiresWorldDay;
        public bool RequiresWarning;
        public string Warning = string.Empty;

        public D10SideJobOffer Clone()
        {
            var result = new D10SideJobOffer
            {
                OfferId = OfferId,
                IssuerRef = IssuerRef,
                JobType = JobType,
                Summary = Summary,
                ExpiresWorldDay = ExpiresWorldDay,
                RequiresWarning = RequiresWarning,
                Warning = Warning,
            };
            if (PrerequisiteTags != null) result.PrerequisiteTags.AddRange(PrerequisiteTags);
            if (RiskTags != null) result.RiskTags.AddRange(RiskTags);
            if (RewardTags != null) result.RewardTags.AddRange(RewardTags);
            return result;
        }
    }

    [Serializable]
    public sealed class D10PokerEntryIntent
    {
        public string TableId = string.Empty;
        public string RulesetId = string.Empty;
        public string StakeBand = string.Empty;
        public string SettlementOwner = "MiniGameSystem";
        public string Message = string.Empty;

        public D10PokerEntryIntent Clone()
        {
            return new D10PokerEntryIntent
            {
                TableId = TableId,
                RulesetId = RulesetId,
                StakeBand = StakeBand,
                SettlementOwner = SettlementOwner,
                Message = Message,
            };
        }
    }

    [Serializable]
    public sealed class D10TavernRequest
    {
        public int SchemaVersion = D10TavernConfiguration.CurrentSchemaVersion;
        public string RequestId = string.Empty;
        public string FactEventId = string.Empty;
        public D10TavernAction Action;
        public D10TavernSensitivity Sensitivity;
        public bool WarningAcknowledged;
        public string TavernId = string.Empty;
        public string PortId = string.Empty;
        public string ActorId = string.Empty;
        public string TargetId = string.Empty;
        public string RelationTargetId = string.Empty;
        public string RumorId = string.Empty;
        public string SideJobId = string.Empty;
        public string RecruitCandidateId = string.Empty;
        public D10AlcoholStrength AlcoholStrength;
        public int RestHours;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();

        public D10TavernRequest Clone()
        {
            var result = new D10TavernRequest
            {
                SchemaVersion = SchemaVersion,
                RequestId = RequestId,
                FactEventId = FactEventId,
                Action = Action,
                Sensitivity = Sensitivity,
                WarningAcknowledged = WarningAcknowledged,
                TavernId = TavernId,
                PortId = PortId,
                ActorId = ActorId,
                TargetId = TargetId,
                RelationTargetId = RelationTargetId,
                RumorId = RumorId,
                SideJobId = SideJobId,
                RecruitCandidateId = RecruitCandidateId,
                AlcoholStrength = AlcoholStrength,
                RestHours = RestHours,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D10TavernSceneSnapshot
    {
        public int SchemaVersion = D10TavernConfiguration.CurrentSchemaVersion;
        public string TavernId = string.Empty;
        public string PortId = string.Empty;
        public string DisplayName = string.Empty;
        public List<D10TavernRumor> Rumors = new List<D10TavernRumor>();
        public List<D10SideJobOffer> SideJobs = new List<D10SideJobOffer>();
        public List<D10PokerEntryIntent> PokerTables = new List<D10PokerEntryIntent>();

        public D10TavernSceneSnapshot Clone()
        {
            var result = new D10TavernSceneSnapshot
            {
                SchemaVersion = SchemaVersion,
                TavernId = TavernId,
                PortId = PortId,
                DisplayName = DisplayName,
            };
            if (Rumors != null)
                foreach (var rumor in Rumors) if (rumor != null) result.Rumors.Add(rumor.Clone());
            if (SideJobs != null)
                foreach (var job in SideJobs) if (job != null) result.SideJobs.Add(job.Clone());
            if (PokerTables != null)
                foreach (var table in PokerTables) if (table != null) result.PokerTables.Add(table.Clone());
            return result;
        }
    }

    [Serializable]
    public sealed class D10TavernRequestResult
    {
        public bool Accepted;
        public bool Duplicate;
        public D10TavernRequestStatus Status;
        public string Code = string.Empty;
        public string Message = string.Empty;
        public string Warning = string.Empty;
        public D10TavernRequest Request;
        public FactEvent Fact;
        public D10TavernAlcoholEffect AlcoholEffect;
        public D10TavernRumor Rumor;
        public D10SideJobOffer SideJob;
        public D10PokerEntryIntent PokerEntry;

        public static D10TavernRequestResult Rejected(string code, string message)
        {
            return new D10TavernRequestResult
            {
                Accepted = false,
                Status = D10TavernRequestStatus.Rejected,
                Code = code ?? string.Empty,
                Message = message ?? string.Empty,
            };
        }

        public static D10TavernRequestResult WarningRequired(string warning)
        {
            return new D10TavernRequestResult
            {
                Accepted = false,
                Status = D10TavernRequestStatus.WarningRequired,
                Code = "warning_required",
                Message = warning ?? "该内容需要确认后才能继续。",
                Warning = warning ?? string.Empty,
            };
        }
    }

    [Serializable]
    public sealed class D10TavernResolution
    {
        public bool Accepted;
        public bool Duplicate;
        public string Code = string.Empty;
        public string Message = string.Empty;
        public D10TavernRequest Request;
        public FactEvent Fact;
        public D9PreparationResult Recruitment;
        public D10CrewBehaviorResult CrewBehavior;
        public D10RelationChangeResult Relation;
        public D10PokerEntryIntent PokerEntry;
    }

    /// <summary>由港口/运行时编排层注入的 owner 服务；它不是 UI 写入接口。</summary>
    public sealed class D10TavernOwnerServices
    {
        public TradeEconomySystem Economy;
        public D3TradeState TradeState;
        public ShipState Ship;
        public D9PortShipPreparationSystem Preparation;
        public D10CrewBehaviorSystem CrewBehavior;
        public D10RelationSystem Relations;
        public IList<CrewAgent> Crew;
        public Func<string, CrewAgent> RecruitCandidateResolver;
        public string RelationTargetId = "player:captain";
    }

    /// <summary>D10-A06 统一入口参数；后续平衡调整仍需由 owner/设计任务变更。</summary>
    public static class D10TavernConfiguration
    {
        // static readonly 保持“配置入口”可被合法性检查覆盖，避免 const 散落为编译期硬编码。
        public static readonly int CurrentSchemaVersion = 1;
        public static readonly int RestMinHours = 1;
        public static readonly int RestMaxHours = 12;
        public static readonly int DefaultRestHours = 4;
        public static readonly D10AlcoholStrength DefaultAlcoholStrength = D10AlcoholStrength.Strong;
        public static readonly int MealPriceQuote = 3;
        public static readonly float MealHealthDelta = 2f;
        public static readonly int LightDrinkPriceQuote = 2;
        public static readonly int StrongDrinkPriceQuote = 5;
        public static readonly int BingeDrinkPriceQuote = 8;
        public static readonly float LightDrinkHealthDelta = -1f;
        public static readonly float StrongDrinkHealthDelta = -3f;
        public static readonly float BingeDrinkHealthDelta = -8f;
        public static readonly float LightDrinkFatigueDelta = 4f;
        public static readonly float StrongDrinkFatigueDelta = 8f;
        public static readonly float BingeDrinkFatigueDelta = 16f;
        public static readonly float LightDrinkConflictRisk = 0.10f;
        public static readonly float StrongDrinkConflictRisk = 0.25f;
        public static readonly float BingeDrinkConflictRisk = 0.50f;

        public static D10TavernAlcoholEffect AlcoholEffect(D10AlcoholStrength strength)
        {
            switch (strength)
            {
                case D10AlcoholStrength.Light:
                    return Effect("alcohol_light", strength, LightDrinkPriceQuote,
                        LightDrinkHealthDelta, LightDrinkFatigueDelta, LightDrinkConflictRisk,
                        "轻饮会增加疲劳；连续饮酒可能引发争执。");
                case D10AlcoholStrength.Strong:
                    return Effect("alcohol_strong", strength, StrongDrinkPriceQuote,
                        StrongDrinkHealthDelta, StrongDrinkFatigueDelta, StrongDrinkConflictRisk,
                        "烈酒会损害健康并增加疲劳，低士气时可能引发冲突。");
                default:
                    return Effect("alcohol_binge", strength, BingeDrinkPriceQuote,
                        BingeDrinkHealthDelta, BingeDrinkFatigueDelta, BingeDrinkConflictRisk,
                        "过量饮酒会明显损害健康和疲劳状态，并提高冲突风险。");
            }
        }

        public static List<string> Validate()
        {
            var errors = new List<string>();
            if (CurrentSchemaVersion <= 0) errors.Add("schema_version_invalid");
            if (RestMinHours <= 0 || RestMinHours > RestMaxHours || RestMaxHours > 24)
                errors.Add("rest_hours_invalid");
            if (DefaultRestHours < RestMinHours || DefaultRestHours > RestMaxHours)
                errors.Add("default_rest_hours_invalid");
            if (!Enum.IsDefined(typeof(D10AlcoholStrength), DefaultAlcoholStrength))
                errors.Add("default_alcohol_strength_invalid");
            if (MealPriceQuote < 0 || LightDrinkPriceQuote < 0 || StrongDrinkPriceQuote < 0
                || BingeDrinkPriceQuote < 0) errors.Add("price_quote_invalid");
            if (MealHealthDelta < -100f || MealHealthDelta > 100f)
                errors.Add("meal_health_delta_invalid");
            foreach (var effect in new[] { AlcoholEffect(D10AlcoholStrength.Light),
                AlcoholEffect(D10AlcoholStrength.Strong), AlcoholEffect(D10AlcoholStrength.Binge) })
            {
                if (effect.HealthDelta < -100f || effect.HealthDelta > 100f
                    || effect.FatigueDelta < 0f || effect.FatigueDelta > 100f
                    || effect.ConflictRisk < 0f || effect.ConflictRisk > 1f)
                    errors.Add("alcohol_effect_invalid");
            }
            return errors;
        }

        private static D10TavernAlcoholEffect Effect(string profileId, D10AlcoholStrength strength,
            int price, float health, float fatigue, float risk, string summary)
        {
            return new D10TavernAlcoholEffect
            {
                ProfileId = profileId,
                Strength = strength,
                PriceQuote = price,
                HealthDelta = health,
                FatigueDelta = fatigue,
                ConflictRisk = risk,
                ConsequenceSummary = summary,
            };
        }
    }
}
