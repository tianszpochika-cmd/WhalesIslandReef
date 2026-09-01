// ============================================================
// D20AquaticBehaviorAdapter —— D20-L25 水生行为只读适配（纯 C#）
//
// 鱼群的数量/压力由 D4 EcologySystem 写入，鲸豚遭遇路线由
// D11WhaleDolphinSystem 写入；海况和生态输入也只作为公开快照消费。
// 本文件只输出逻辑可见性、遭遇资格和交互资格，不移动实体、不处理
// 表现组件，不裁决捕捞、救助、捕鲸、奖励、法律或生态后果。
// D20-L20/L21/L22 未落地，因此这里直接消费现有 D4/D11 公开读面，不复制
// 那些任务的动物身份、物种目录或行为状态机。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D4;
using Whalebone.Systems.D11;

namespace Whalebone.Systems.D20
{
    /// <summary>水生读模型的实体类别。</summary>
    public enum D20AquaticEntityKind
    {
        Unknown = 0,
        FishSchool = 1,
        Whale = 2,
        Dolphin = 3,
    }

    /// <summary>D20 只表达交互意图，不代表 owner 已完成结算。</summary>
    public enum D20AquaticInteractionKind
    {
        None = 0,
        Observe = 1,
        Fish = 2,
        Escort = 3,
        Rescue = 4,
        Whaling = 5,
    }

    /// <summary>逻辑可见性，不是渲染器或镜头可见性。</summary>
    public enum D20AquaticVisibilityState
    {
        Unknown = 0,
        Visible = 1,
        Hidden = 2,
        Blocked = 3,
        Depleted = 4,
        Terminal = 5,
    }

    /// <summary>交互资格结果。</summary>
    public enum D20AquaticInteractionStatus
    {
        Unknown = 0,
        Eligible = 1,
        Ineligible = 2,
        OwnerRequired = 3,
        Blocked = 4,
    }

    /// <summary>水生读模型的风险解释桶；不写回 Ecology/D11。</summary>
    public enum D20AquaticRiskLevel
    {
        Unknown = 0,
        None = 1,
        Low = 2,
        Moderate = 3,
        High = 4,
        Critical = 5,
    }

    /// <summary>水生交互资格的只读结果。</summary>
    [Serializable]
    public sealed class D20AquaticInteractionEvaluation
    {
        public bool InputAvailable;
        public bool Eligible;
        public D20AquaticInteractionKind Interaction;
        public D20AquaticInteractionStatus Status;
        public string EntityId = string.Empty;
        public string ReasonCode = string.Empty;
        public string FeedbackKey = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D20AquaticInteractionEvaluation Clone()
        {
            return new D20AquaticInteractionEvaluation
            {
                InputAvailable = InputAvailable,
                Eligible = Eligible,
                Interaction = Interaction,
                Status = Status,
                EntityId = EntityId ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                FeedbackKey = FeedbackKey ?? string.Empty,
            };
        }
    }

    /// <summary>
    /// 鱼群/鲸豚共用的水生逻辑读模型。位置只用稳定逻辑引用，不包含
    /// Ecology/D11 的可变集合或 Unity 空间对象。
    /// </summary>
    [Serializable]
    public sealed class D20AquaticBehaviorReadModel
    {
        public int SchemaVersion = D20AquaticBehaviorAdapter.CurrentSchemaVersion;
        public bool InputAvailable;
        public bool LogicalEncounterEligible;
        public bool InteractionEligible;
        public D20AquaticEntityKind EntityKind;
        public D20AquaticVisibilityState Visibility;
        public D20AquaticInteractionKind RequestedInteraction;
        public D20AquaticInteractionStatus InteractionStatus;
        public D20AquaticRiskLevel RiskLevel;
        public string EntityId = string.Empty;
        public string SpeciesId = string.Empty;
        public string HabitatId = string.Empty;
        public string LocationRef = string.Empty;
        public string RouteRef = string.Empty;
        public int Biomass;
        public int Capacity;
        public int FishingPressure;
        public float EcologyStress;
        public bool ProtectedPopulation;
        public bool IsStranded;
        public bool Observed;
        public bool Escorted;
        public bool Rescued;
        public bool TrackingAbandoned;
        public D11MarineSpeciesKind MarineSpeciesKind = D11MarineSpeciesKind.Unknown;
        public D11MarineRouteAction LastRouteAction = D11MarineRouteAction.Unknown;
        public long SourceRevision;
        public string SourceOwnerSystem = string.Empty;
        public string ReasonCode = string.Empty;
        public string FeedbackKey = string.Empty;
        public D20AquaticInteractionEvaluation Interaction =
            new D20AquaticInteractionEvaluation();

        /// <summary>返回深拷贝。</summary>
        public D20AquaticBehaviorReadModel Clone()
        {
            return new D20AquaticBehaviorReadModel
            {
                SchemaVersion = SchemaVersion,
                InputAvailable = InputAvailable,
                LogicalEncounterEligible = LogicalEncounterEligible,
                InteractionEligible = InteractionEligible,
                EntityKind = EntityKind,
                Visibility = Visibility,
                RequestedInteraction = RequestedInteraction,
                InteractionStatus = InteractionStatus,
                RiskLevel = RiskLevel,
                EntityId = EntityId ?? string.Empty,
                SpeciesId = SpeciesId ?? string.Empty,
                HabitatId = HabitatId ?? string.Empty,
                LocationRef = LocationRef ?? string.Empty,
                RouteRef = RouteRef ?? string.Empty,
                Biomass = Biomass,
                Capacity = Capacity,
                FishingPressure = FishingPressure,
                EcologyStress = EcologyStress,
                ProtectedPopulation = ProtectedPopulation,
                IsStranded = IsStranded,
                Observed = Observed,
                Escorted = Escorted,
                Rescued = Rescued,
                TrackingAbandoned = TrackingAbandoned,
                MarineSpeciesKind = MarineSpeciesKind,
                LastRouteAction = LastRouteAction,
                SourceRevision = SourceRevision,
                SourceOwnerSystem = SourceOwnerSystem ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                FeedbackKey = FeedbackKey ?? string.Empty,
                Interaction = Interaction == null
                    ? new D20AquaticInteractionEvaluation()
                    : Interaction.Clone(),
            };
        }
    }

    /// <summary>当前水生公开读面的组合快照。</summary>
    [Serializable]
    public sealed class D20AquaticBehaviorSnapshot
    {
        public int SchemaVersion = D20AquaticBehaviorAdapter.CurrentSchemaVersion;
        public bool InputAvailable;
        public long EcologyRevision;
        public long MarineRevision;
        public List<D20AquaticBehaviorReadModel> Records =
            new List<D20AquaticBehaviorReadModel>();

        /// <summary>返回深拷贝。</summary>
        public D20AquaticBehaviorSnapshot Clone()
        {
            var result = new D20AquaticBehaviorSnapshot
            {
                SchemaVersion = SchemaVersion,
                InputAvailable = InputAvailable,
                EcologyRevision = EcologyRevision,
                MarineRevision = MarineRevision,
            };
            if (Records != null)
                foreach (var record in Records)
                    if (record != null) result.Records.Add(record.Clone());
            return result;
        }

        /// <summary>验证只读快照的结构和实体唯一性。</summary>
        public void Validate()
        {
            if (SchemaVersion != D20AquaticBehaviorAdapter.CurrentSchemaVersion)
                throw new InvalidOperationException("D20 aquatic behavior schema_version mismatch.");
            if (EcologyRevision < 0 || MarineRevision < 0)
                throw new InvalidOperationException("D20 aquatic source revisions cannot be negative.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (Records == null) return;
            foreach (var record in Records)
            {
                if (record == null || string.IsNullOrEmpty(record.EntityId))
                    throw new InvalidOperationException("D20 aquatic entity_id is required.");
                if (!ids.Add(record.EntityId))
                    throw new InvalidOperationException("D20 aquatic entity_id is duplicated: " + record.EntityId);
            }
        }
    }

    /// <summary>
    /// D20-L25 适配器。它只消费 D4 EcologyState 和 D11 whale/dolphin
    /// Snapshot/FindEncounter 的 clone；没有任何会改变 owner 状态的方法。
    /// </summary>
    public sealed class D20AquaticBehaviorAdapter
    {
        /// <summary>适配器投影版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>D20 适配器 owner 标识。</summary>
        public const string OwnerSystem = "D20AquaticBehaviorAdapter";

        /// <summary>鱼群事实 owner。</summary>
        public const string EcologyOwnerSystem = "EcologySystem";

        /// <summary>鲸豚路线事实 owner。</summary>
        public const string MarineOwnerSystem = "EcologySystem";

        private readonly D4EcologySystem _ecologySystem;
        private readonly D11WhaleDolphinSystem _whaleDolphinSystem;

        /// <summary>创建只读水生适配器。</summary>
        public D20AquaticBehaviorAdapter(D4EcologySystem ecologySystem = null,
            D11WhaleDolphinSystem whaleDolphinSystem = null)
        {
            _ecologySystem = ecologySystem;
            _whaleDolphinSystem = whaleDolphinSystem;
        }

        /// <summary>读取鱼群公开状态并评估默认观察资格。</summary>
        public D20AquaticBehaviorReadModel ReadFishSchool(string fishSchoolId)
        {
            return ReadFishSchool(fishSchoolId, D20AquaticInteractionKind.Observe);
        }

        /// <summary>读取鱼群公开状态并评估指定交互资格。</summary>
        public D20AquaticBehaviorReadModel ReadFishSchool(string fishSchoolId,
            D20AquaticInteractionKind interaction)
        {
            return ReadFishSchool(_ecologySystem == null ? null : _ecologySystem.Snapshot,
                fishSchoolId, interaction);
        }

        /// <summary>读取鲸豚遭遇并评估默认观察资格。</summary>
        public D20AquaticBehaviorReadModel ReadEncounter(string encounterId)
        {
            return ReadEncounter(encounterId, D20AquaticInteractionKind.Observe, null, null);
        }

        /// <summary>读取鲸豚遭遇并消费显式生态/海况快照。</summary>
        public D20AquaticBehaviorReadModel ReadEncounter(string encounterId,
            D20AquaticInteractionKind interaction,
            D11MarineEcologyReadSnapshot ecology = null,
            D11MarineEnvironmentReadSnapshot environment = null)
        {
            return ReadEncounter(_whaleDolphinSystem == null ? null :
                _whaleDolphinSystem.FindEncounter(encounterId), interaction, ecology, environment,
                _whaleDolphinSystem == null ? 0L : _whaleDolphinSystem.Snapshot.Revision);
        }

        /// <summary>FindEncounter 的语义别名，保持下游调用命名清晰。</summary>
        public D20AquaticBehaviorReadModel ReadMarineEncounter(string encounterId,
            D20AquaticInteractionKind interaction = D20AquaticInteractionKind.Observe,
            D11MarineEcologyReadSnapshot ecology = null,
            D11MarineEnvironmentReadSnapshot environment = null)
        {
            return ReadEncounter(encounterId, interaction, ecology, environment);
        }

        /// <summary>按 ID 读取鱼群或鲸豚；优先读取生态鱼群。</summary>
        public D20AquaticBehaviorReadModel Read(string entityId,
            D20AquaticInteractionKind interaction = D20AquaticInteractionKind.Observe,
            D11MarineEcologyReadSnapshot ecology = null,
            D11MarineEnvironmentReadSnapshot environment = null)
        {
            var fish = ReadFishSchool(entityId, interaction);
            if (fish.InputAvailable) return fish;
            return ReadEncounter(entityId, interaction, ecology, environment);
        }

        /// <summary>读取指定实体的交互资格，不执行交互。</summary>
        public D20AquaticInteractionEvaluation EvaluateInteraction(string entityId,
            D20AquaticInteractionKind interaction,
            D11MarineEcologyReadSnapshot ecology = null,
            D11MarineEnvironmentReadSnapshot environment = null)
        {
            var read = Read(entityId, interaction, ecology, environment);
            return read.Interaction == null
                ? new D20AquaticInteractionEvaluation
                {
                    InputAvailable = read.InputAvailable,
                    Eligible = false,
                    Interaction = interaction,
                    Status = D20AquaticInteractionStatus.Unknown,
                    EntityId = entityId ?? string.Empty,
                    ReasonCode = "aquatic_interaction_unavailable",
                    FeedbackKey = "aquatic.feedback.unavailable",
                }
                : read.Interaction.Clone();
        }

        /// <summary>构建当前两个公开 owner 的组合只读快照。</summary>
        public D20AquaticBehaviorSnapshot Snapshot
        {
            get { return BuildSnapshot(); }
        }

        /// <summary>捕获当前组合快照。</summary>
        public D20AquaticBehaviorSnapshot CaptureSnapshot()
        {
            return Snapshot;
        }

        /// <summary>直接消费 EcologySystem clone 的鱼群公开状态。</summary>
        public static D20AquaticBehaviorReadModel ReadFishSchool(EcologyState snapshot,
            string fishSchoolId,
            D20AquaticInteractionKind interaction = D20AquaticInteractionKind.Observe)
        {
            if (snapshot == null) return Missing(fishSchoolId, interaction, "ecology_snapshot_missing");
            if (string.IsNullOrWhiteSpace(fishSchoolId))
                return Missing(fishSchoolId, interaction, "fish_school_id_missing");
            if (snapshot.FishSchools == null)
                return Missing(fishSchoolId, interaction, "fish_school_records_missing");
            foreach (var school in snapshot.FishSchools)
            {
                if (school == null || !string.Equals(school.FishSchoolId, fishSchoolId,
                    StringComparison.Ordinal)) continue;
                return FromFishSchool(snapshot, school, interaction);
            }
            return Missing(fishSchoolId, interaction, "fish_school_not_found");
        }

        /// <summary>直接消费 D11WhaleDolphinSystem 返回的 encounter clone。</summary>
        public static D20AquaticBehaviorReadModel ReadEncounter(D11MarineEncounter encounter,
            D20AquaticInteractionKind interaction = D20AquaticInteractionKind.Observe,
            D11MarineEcologyReadSnapshot ecology = null,
            D11MarineEnvironmentReadSnapshot environment = null,
            long sourceRevision = 0)
        {
            if (encounter == null)
                return Missing(string.Empty, interaction, "marine_encounter_missing");
            if (encounter.SpeciesKind != D11MarineSpeciesKind.Whale &&
                encounter.SpeciesKind != D11MarineSpeciesKind.Dolphin)
                return Missing(encounter.EncounterId, interaction, "marine_species_unknown");

            var safeEcology = ecology == null ? new D11MarineEcologyReadSnapshot() : ecology.Clone();
            var safeEnvironment = environment == null
                ? new D11MarineEnvironmentReadSnapshot() : environment.Clone();
            var result = new D20AquaticBehaviorReadModel
            {
                InputAvailable = true,
                EntityKind = encounter.SpeciesKind == D11MarineSpeciesKind.Whale
                    ? D20AquaticEntityKind.Whale : D20AquaticEntityKind.Dolphin,
                EntityId = encounter.EncounterId ?? string.Empty,
                SpeciesId = encounter.SpeciesId ?? string.Empty,
                HabitatId = encounter.HabitatId ?? safeEcology.HabitatId ?? string.Empty,
                LocationRef = "marine:" + (encounter.EncounterId ?? string.Empty),
                RouteRef = encounter.RouteRef ?? string.Empty,
                EcologyStress = Math.Max(encounter.EcologyStress, safeEcology.EcologyStress),
                ProtectedPopulation = encounter.ProtectedPopulation || safeEcology.ProtectedPopulation,
                IsStranded = encounter.IsStranded || safeEcology.IsStranded,
                Observed = encounter.Observed,
                Escorted = encounter.Escorted,
                Rescued = encounter.Rescued,
                TrackingAbandoned = encounter.TrackingAbandoned,
                MarineSpeciesKind = encounter.SpeciesKind,
                SourceRevision = Math.Max(0L, sourceRevision),
                SourceOwnerSystem = MarineOwnerSystem,
                RequestedInteraction = interaction,
            };

            if (float.IsNaN(encounter.EcologyStress) || float.IsInfinity(encounter.EcologyStress) ||
                float.IsNaN(safeEcology.EcologyStress) || float.IsInfinity(safeEcology.EcologyStress) ||
                safeEcology.EcologyStress < 0f || safeEcology.EcologyStress > 1f ||
                float.IsNaN(safeEnvironment.Visibility) || float.IsInfinity(safeEnvironment.Visibility) ||
                safeEnvironment.Visibility < 0f || safeEnvironment.Visibility > 1f)
                return InvalidMarine(result, "marine_read_input_invalid");

            if (encounter.Rescued || encounter.TrackingAbandoned)
            {
                result.Visibility = D20AquaticVisibilityState.Terminal;
                result.RiskLevel = D20AquaticRiskLevel.None;
                result.ReasonCode = encounter.Rescued
                    ? "marine_encounter_rescued" : "marine_tracking_abandoned";
                result.FeedbackKey = "aquatic.feedback.terminal";
                result.Interaction = EvaluateMarineInteraction(result, interaction,
                    safeEcology, safeEnvironment);
                CopyInteraction(result);
                return result;
            }

            if (safeEcology.ObservationBlocked || safeEnvironment.Visibility <= 0f)
            {
                result.Visibility = safeEcology.ObservationBlocked
                    ? D20AquaticVisibilityState.Blocked : D20AquaticVisibilityState.Hidden;
                result.RiskLevel = RiskForMarine(result);
                result.ReasonCode = safeEcology.ObservationBlocked
                    ? "marine_observation_blocked" : "marine_visibility_unavailable";
                result.FeedbackKey = "aquatic.feedback.unavailable";
                result.Interaction = EvaluateMarineInteraction(result, interaction,
                    safeEcology, safeEnvironment);
                CopyInteraction(result);
                return result;
            }

            result.Visibility = D20AquaticVisibilityState.Visible;
            result.LogicalEncounterEligible = true;
            result.RiskLevel = RiskForMarine(result);
            result.ReasonCode = "marine_encounter_available";
            result.FeedbackKey = "aquatic.feedback.encounter";
            result.Interaction = EvaluateMarineInteraction(result, interaction,
                safeEcology, safeEnvironment);
            CopyInteraction(result);
            return result;
        }

        /// <summary>直接消费两个 owner 的 clone 构建水生组合投影。</summary>
        public static D20AquaticBehaviorSnapshot BuildSnapshot(EcologyState ecology,
            D11WhaleDolphinRuntimeSnapshot marine)
        {
            var result = new D20AquaticBehaviorSnapshot
            {
                InputAvailable = ecology != null || marine != null,
                EcologyRevision = ecology == null ? 0L : Math.Max(0L, ecology.EcologyRevision),
                MarineRevision = marine == null ? 0L : Math.Max(0L, marine.Revision),
            };
            if (ecology != null && ecology.FishSchools != null)
                foreach (var school in ecology.FishSchools)
                    if (school != null)
                        result.Records.Add(ReadFishSchool(ecology, school.FishSchoolId));
            if (marine != null && marine.Encounters != null)
                foreach (var encounter in marine.Encounters)
                    if (encounter != null)
                        result.Records.Add(ReadEncounter(encounter));
            return result;
        }

        private D20AquaticBehaviorSnapshot BuildSnapshot()
        {
            return BuildSnapshot(_ecologySystem == null ? null : _ecologySystem.Snapshot,
                _whaleDolphinSystem == null ? null : _whaleDolphinSystem.Snapshot);
        }

        private static D20AquaticBehaviorReadModel FromFishSchool(EcologyState snapshot,
            FishSchoolState school, D20AquaticInteractionKind interaction)
        {
            var result = new D20AquaticBehaviorReadModel
            {
                InputAvailable = true,
                EntityKind = D20AquaticEntityKind.FishSchool,
                EntityId = school.FishSchoolId ?? string.Empty,
                SpeciesId = school.FishSpeciesId ?? string.Empty,
                HabitatId = snapshot.Habitat == null ? snapshot.HabitatId ?? string.Empty
                    : snapshot.Habitat.HabitatId ?? snapshot.HabitatId ?? string.Empty,
                LocationRef = school.FishingSpotId ?? string.Empty,
                RouteRef = school.Migration ?? string.Empty,
                Biomass = school.Biomass,
                Capacity = school.Capacity,
                FishingPressure = school.FishingPressure,
                RequestedInteraction = interaction,
                SourceRevision = Math.Max(0L, snapshot.EcologyRevision),
                SourceOwnerSystem = EcologyOwnerSystem,
            };

            if (school.Biomass < 0 || school.Capacity <= 0 || school.FishingPressure < 0)
                return InvalidFish(result, "fish_school_read_input_invalid");
            if (school.Biomass == 0)
            {
                result.Visibility = D20AquaticVisibilityState.Depleted;
                result.RiskLevel = D20AquaticRiskLevel.None;
                result.ReasonCode = "fish_school_depleted";
                result.FeedbackKey = "aquatic.feedback.depleted";
                result.Interaction = EvaluateFishInteraction(result, interaction);
                CopyInteraction(result);
                return result;
            }

            result.Visibility = D20AquaticVisibilityState.Visible;
            result.LogicalEncounterEligible = true;
            result.RiskLevel = RiskForFish(result);
            result.ReasonCode = "fish_school_available";
            result.FeedbackKey = "aquatic.feedback.encounter";
            result.Interaction = EvaluateFishInteraction(result, interaction);
            CopyInteraction(result);
            return result;
        }

        private static D20AquaticBehaviorReadModel Missing(string entityId,
            D20AquaticInteractionKind interaction, string reasonCode)
        {
            return new D20AquaticBehaviorReadModel
            {
                InputAvailable = false,
                EntityKind = D20AquaticEntityKind.Unknown,
                Visibility = D20AquaticVisibilityState.Unknown,
                InteractionStatus = D20AquaticInteractionStatus.Unknown,
                RequestedInteraction = interaction,
                EntityId = entityId ?? string.Empty,
                ReasonCode = reasonCode ?? string.Empty,
                FeedbackKey = "aquatic.feedback.unavailable",
                SourceOwnerSystem = EcologyOwnerSystem,
                Interaction = new D20AquaticInteractionEvaluation
                {
                    InputAvailable = false,
                    Eligible = false,
                    Interaction = interaction,
                    Status = D20AquaticInteractionStatus.Unknown,
                    EntityId = entityId ?? string.Empty,
                    ReasonCode = reasonCode ?? string.Empty,
                    FeedbackKey = "aquatic.feedback.unavailable",
                },
            };
        }

        private static D20AquaticBehaviorReadModel InvalidFish(
            D20AquaticBehaviorReadModel result, string reasonCode)
        {
            result.Visibility = D20AquaticVisibilityState.Blocked;
            result.RiskLevel = D20AquaticRiskLevel.Unknown;
            result.ReasonCode = reasonCode;
            result.FeedbackKey = "aquatic.feedback.unavailable";
            result.Interaction = Ineligible(result.EntityId, result.RequestedInteraction,
                D20AquaticInteractionStatus.Blocked, reasonCode);
            CopyInteraction(result);
            return result;
        }

        private static D20AquaticBehaviorReadModel InvalidMarine(
            D20AquaticBehaviorReadModel result, string reasonCode)
        {
            result.Visibility = D20AquaticVisibilityState.Blocked;
            result.RiskLevel = D20AquaticRiskLevel.Unknown;
            result.ReasonCode = reasonCode;
            result.FeedbackKey = "aquatic.feedback.unavailable";
            result.Interaction = Ineligible(result.EntityId, result.RequestedInteraction,
                D20AquaticInteractionStatus.Blocked, reasonCode);
            CopyInteraction(result);
            return result;
        }

        private static D20AquaticInteractionEvaluation EvaluateFishInteraction(
            D20AquaticBehaviorReadModel result, D20AquaticInteractionKind interaction)
        {
            if (result.Biomass <= 0)
                return Ineligible(result.EntityId, interaction,
                    D20AquaticInteractionStatus.Ineligible, "fish_school_depleted");
            if (interaction == D20AquaticInteractionKind.Observe)
                return Eligible(result.EntityId, interaction, "fish_observation_eligible");
            if (interaction == D20AquaticInteractionKind.Fish)
            {
                if (string.IsNullOrEmpty(result.LocationRef))
                    return Ineligible(result.EntityId, interaction,
                        D20AquaticInteractionStatus.Ineligible, "fish_spot_missing");
                return Eligible(result.EntityId, interaction, "fish_intent_eligible");
            }
            return Ineligible(result.EntityId, interaction,
                D20AquaticInteractionStatus.Ineligible, "fish_interaction_not_supported");
        }

        private static D20AquaticInteractionEvaluation EvaluateMarineInteraction(
            D20AquaticBehaviorReadModel result, D20AquaticInteractionKind interaction,
            D11MarineEcologyReadSnapshot ecology, D11MarineEnvironmentReadSnapshot environment)
        {
            if (result.Visibility == D20AquaticVisibilityState.Blocked ||
                result.Visibility == D20AquaticVisibilityState.Hidden)
                return Ineligible(result.EntityId, interaction,
                    D20AquaticInteractionStatus.Blocked, result.ReasonCode);
            if (result.Rescued || result.TrackingAbandoned)
                return Ineligible(result.EntityId, interaction,
                    D20AquaticInteractionStatus.Ineligible, result.ReasonCode);
            if (interaction == D20AquaticInteractionKind.Observe)
                return Eligible(result.EntityId, interaction, "marine_observation_eligible");
            if (interaction == D20AquaticInteractionKind.Rescue)
                return result.IsStranded
                    ? Eligible(result.EntityId, interaction, "marine_rescue_intent_eligible")
                    : Ineligible(result.EntityId, interaction, "marine_not_stranded");
            if (interaction == D20AquaticInteractionKind.Escort)
            {
                if (result.EntityKind != D20AquaticEntityKind.Dolphin)
                    return Ineligible(result.EntityId, interaction, "escort_requires_dolphin");
                if (result.Escorted)
                    return Ineligible(result.EntityId, interaction, "dolphin_already_escorted");
                if (ecology != null && !ecology.DolphinEscortAvailable)
                    return Ineligible(result.EntityId, interaction, "dolphin_escort_unavailable");
                return Eligible(result.EntityId, interaction, "dolphin_escort_intent_eligible");
            }
            if (interaction == D20AquaticInteractionKind.Whaling)
                return new D20AquaticInteractionEvaluation
                {
                    InputAvailable = true,
                    Eligible = false,
                    Interaction = interaction,
                    Status = D20AquaticInteractionStatus.OwnerRequired,
                    EntityId = result.EntityId,
                    ReasonCode = "whaling_owner_policy_required",
                    FeedbackKey = "aquatic.feedback.owner_required",
                };
            return Ineligible(result.EntityId, interaction,
                D20AquaticInteractionStatus.Ineligible, "marine_interaction_not_supported");
        }

        private static D20AquaticInteractionEvaluation Eligible(string entityId,
            D20AquaticInteractionKind interaction, string reasonCode)
        {
            return new D20AquaticInteractionEvaluation
            {
                InputAvailable = true,
                Eligible = true,
                Interaction = interaction,
                Status = D20AquaticInteractionStatus.Eligible,
                EntityId = entityId ?? string.Empty,
                ReasonCode = reasonCode ?? string.Empty,
                FeedbackKey = "aquatic.feedback.interact",
            };
        }

        private static D20AquaticInteractionEvaluation Ineligible(string entityId,
            D20AquaticInteractionKind interaction, string reasonCode)
        {
            return Ineligible(entityId, interaction, D20AquaticInteractionStatus.Ineligible,
                reasonCode);
        }

        private static D20AquaticInteractionEvaluation Ineligible(string entityId,
            D20AquaticInteractionKind interaction, D20AquaticInteractionStatus status,
            string reasonCode)
        {
            return new D20AquaticInteractionEvaluation
            {
                InputAvailable = true,
                Eligible = false,
                Interaction = interaction,
                Status = status,
                EntityId = entityId ?? string.Empty,
                ReasonCode = reasonCode ?? string.Empty,
                FeedbackKey = status == D20AquaticInteractionStatus.OwnerRequired
                    ? "aquatic.feedback.owner_required" : "aquatic.feedback.unavailable",
            };
        }

        private static void CopyInteraction(D20AquaticBehaviorReadModel result)
        {
            if (result.Interaction == null) return;
            result.InteractionEligible = result.Interaction.Eligible;
            result.InteractionStatus = result.Interaction.Status;
        }

        private static D20AquaticRiskLevel RiskForFish(D20AquaticBehaviorReadModel result)
        {
            if (result.Biomass <= 0) return D20AquaticRiskLevel.None;
            float pressure = result.FishingPressure / (float)Math.Max(1, result.Capacity);
            if (pressure >= 1f || result.Biomass <= Math.Max(1, result.Capacity / 4))
                return D20AquaticRiskLevel.Critical;
            if (pressure >= 0.66f || result.Biomass <= Math.Max(1, result.Capacity / 2))
                return D20AquaticRiskLevel.High;
            if (pressure >= 0.33f) return D20AquaticRiskLevel.Moderate;
            return D20AquaticRiskLevel.Low;
        }

        private static D20AquaticRiskLevel RiskForMarine(D20AquaticBehaviorReadModel result)
        {
            if (result.IsStranded) return D20AquaticRiskLevel.Critical;
            if (result.EcologyStress >= 0.75f) return D20AquaticRiskLevel.Critical;
            if (result.EcologyStress >= 0.50f || result.ProtectedPopulation)
                return D20AquaticRiskLevel.High;
            if (result.EcologyStress >= 0.25f) return D20AquaticRiskLevel.Moderate;
            return D20AquaticRiskLevel.Low;
        }
    }
}
