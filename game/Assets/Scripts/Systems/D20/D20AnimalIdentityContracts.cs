// ============================================================
// D20AnimalIdentityContracts —— D20-L20 动物身份契约（纯 C#）
//
// 本文件只冻结动物的稳定逻辑引用和 D20 语义投影。生态种群、宠物照料、
// 健康、数量、位置和稀有动物收集事实仍由 D4/D11 的 owner 裁决；这里不
// 保存可替代这些 owner 的数值状态，也不引用 Unity、网络或系统时钟。
// ============================================================

using System;

namespace Whalebone.Systems.D20
{
    /// <summary>
    /// D18 生态分类的位标记。RareSpecies 是横切标记，不是新的 content_id
    /// 前缀；例如绿海龟属于 Aquatic | RareSpecies。
    /// </summary>
    [Flags]
    public enum D20AnimalClassification
    {
        /// <summary>未分类值，仅允许作为未初始化载体值。</summary>
        None = 0,
        /// <summary>陆生动物，包括家禽/牲畜但不包括宠物。</summary>
        Terrestrial = 1,
        /// <summary>鸟类；D18 通过子类字段与陆生动物区分。</summary>
        Bird = 2,
        /// <summary>水生动物，包括鱼类、海龟和鲸豚。</summary>
        Aquatic = 4,
        /// <summary>受保护/受监管的横切标记，不改变原始前缀。</summary>
        RareSpecies = 8,
        /// <summary>非战斗伴侣动物内容分类。</summary>
        Pet = 16,
    }

    /// <summary>动物的 D20 管理语义；Domestic 保留 D18 家禽/牲畜不是宠物的边界。</summary>
    public enum D20AnimalPopulationKind
    {
        /// <summary>未识别值，仅允许作为未初始化载体值。</summary>
        Unknown = 0,
        /// <summary>野生个体或生态系统中的野生实例。</summary>
        Wild = 1,
        /// <summary>家养/养殖个体，但不是 D11 宠物实例。</summary>
        Domestic = 2,
        /// <summary>D11 AnimalCareSystem 管理的非战斗宠物实例。</summary>
        Pet = 3,
    }

    /// <summary>动物年龄的离散语义；不把年龄换算成生态或健康数值。</summary>
    public enum D20AnimalLifeStage
    {
        /// <summary>未识别值，仅允许作为未初始化载体值。</summary>
        Unknown = 0,
        /// <summary>新生阶段。</summary>
        Neonate = 1,
        /// <summary>幼年阶段。</summary>
        Juvenile = 2,
        /// <summary>亚成年阶段。</summary>
        Subadult = 3,
        /// <summary>成年阶段。</summary>
        Adult = 4,
        /// <summary>老年阶段。</summary>
        Elder = 5,
    }

    /// <summary>
    /// D20 自有的生命周期投影。D11 的发现/救援/运输/收集和宠物死亡事实
    /// 仍由对应 owner 写入；D20 只接收其公开结果并保留最小语义状态。
    /// </summary>
    public enum D20AnimalLifecycleStatus
    {
        /// <summary>未识别值，仅允许作为未初始化载体值。</summary>
        Unknown = 0,
        /// <summary>动物仍可被行为层处理。</summary>
        Alive = 1,
        /// <summary>生命周期已由 owner 确认终止。</summary>
        Dead = 2,
        /// <summary>不再参与当前模拟，但不等价于死亡。</summary>
        Retired = 3,
    }

    /// <summary>动物归属的语义类型；归属事实仍需由外部 owner 认可。</summary>
    public enum D20AnimalOwnershipKind
    {
        /// <summary>无个体归属，通常用于野生动物。</summary>
        None = 0,
        /// <summary>玩家归属引用。</summary>
        Player = 1,
        /// <summary>船员/角色归属引用。</summary>
        Crew = 2,
        /// <summary>组织、收容所或农场归属引用。</summary>
        Organization = 3,
        /// <summary>由外部 owner 定义的其它归属引用。</summary>
        External = 4,
    }

    /// <summary>D20-L20 身份契约的稳定校验码。</summary>
    public enum D20AnimalIdentityValidationCode
    {
        /// <summary>校验通过。</summary>
        None = 0,
        /// <summary>身份载体为空。</summary>
        MissingIdentity = 1,
        /// <summary>契约版本不受支持。</summary>
        InvalidSchemaVersion = 2,
        /// <summary>animal_id 缺失或包含不稳定字符。</summary>
        InvalidAnimalId = 3,
        /// <summary>content_id 缺失、前缀非法或格式不符合 D18。</summary>
        InvalidContentId = 4,
        /// <summary>物种引用缺失或格式非法。</summary>
        InvalidSpeciesId = 5,
        /// <summary>分类位组合非法。</summary>
        InvalidClassification = 6,
        /// <summary>野生/家养/宠物语义与分类冲突。</summary>
        PopulationClassificationMismatch = 7,
        /// <summary>归属类型与归属引用不匹配。</summary>
        InvalidOwnership = 8,
        /// <summary>owner_system 不属于 D18 冻结的唯一写者。</summary>
        InvalidOwnerSystem = 9,
        /// <summary>生命阶段不受支持。</summary>
        InvalidLifeStage = 10,
        /// <summary>生命周期枚举不受支持。</summary>
        InvalidLifecycle = 11,
        /// <summary>年龄或世界日字段越界。</summary>
        InvalidLifecycleDates = 12,
        /// <summary>原因/来源引用格式非法。</summary>
        InvalidReference = 13,
        /// <summary>存档修订号非法。</summary>
        InvalidSaveRevision = 14,
    }

    /// <summary>不改变原始请求的身份契约校验结果。</summary>
    [Serializable]
    public sealed class D20AnimalIdentityValidationResult
    {
        /// <summary>是否通过校验。</summary>
        public bool Valid;

        /// <summary>稳定机器校验码。</summary>
        public D20AnimalIdentityValidationCode Code;

        /// <summary>机器可读原因键，不承载 UI 文案。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>创建成功结果。</summary>
        public static D20AnimalIdentityValidationResult Success()
        {
            return new D20AnimalIdentityValidationResult
            {
                Valid = true,
                Code = D20AnimalIdentityValidationCode.None,
                ReasonCode = string.Empty,
            };
        }

        /// <summary>创建失败结果。</summary>
        public static D20AnimalIdentityValidationResult Failure(
            D20AnimalIdentityValidationCode code,
            string reasonCode)
        {
            return new D20AnimalIdentityValidationResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>
    /// D20-L20 身份契约与边界验证入口。验证只读字符串引用和语义组合，
    /// 不查询 ContentRegistry 文件、候选资源、场景或任何运行时对象。
    /// </summary>
    public static class D20AnimalIdentityContracts
    {
        /// <summary>当前动物身份契约版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>D20 动物语义层 owner 标识。</summary>
        public const string OwnerSystem = "D20AnimalIdentity";

        /// <summary>D18 野生/家养动物内容的唯一事实 owner。</summary>
        public const string EcologyOwnerSystem = "EcologySystem";

        /// <summary>D18 宠物内容的唯一事实 owner。</summary>
        public const string PetOwnerSystem = "AnimalCareSystem";

        /// <summary>实例 animal_id 的最大长度。</summary>
        public const int AnimalIdMaxLength = 128;

        /// <summary>D18 content_id 的最大长度。</summary>
        public const int ContentIdMaxLength = 160;

        /// <summary>物种或 owner 的稳定引用最大长度。</summary>
        public const int ReferenceMaxLength = 160;

        /// <summary>尚未由外部 owner 提供显示身份时的安全键。</summary>
        public const string UnspecifiedIdentityKey = "animal.identity.unspecified";

        private const int BaseClassificationMask =
            (int)(D20AnimalClassification.Terrestrial |
                  D20AnimalClassification.Bird |
                  D20AnimalClassification.Aquatic |
                  D20AnimalClassification.Pet);

        /// <summary>
        /// 判断 animal_id 是否是可跨存档使用的稳定逻辑键。允许冒号用于
        /// owner 命名空间，但拒绝空白、路径和非 ASCII 字符。
        /// </summary>
        public static bool IsStableAnimalId(string animalId)
        {
            return IsStableReference(animalId, AnimalIdMaxLength);
        }

        /// <summary>判断 species_id 是否是稳定的内容/owner 引用。</summary>
        public static bool IsStableSpeciesId(string speciesId)
        {
            return IsStableReference(speciesId, ReferenceMaxLength);
        }

        /// <summary>
        /// 判断 content_id 是否符合 D18 生态词典。D20 动物目录只接受
        /// animal_、fish_ 和 pet_；珍稀物种必须继续使用原始前缀。
        /// </summary>
        public static bool IsStableAnimalContentId(string contentId)
        {
            if (!IsLowerSnakeCase(contentId, ContentIdMaxLength)) return false;
            return contentId.StartsWith("animal_", StringComparison.Ordinal) ||
                   contentId.StartsWith("fish_", StringComparison.Ordinal) ||
                   contentId.StartsWith("pet_", StringComparison.Ordinal);
        }

        /// <summary>兼容下游调用方的 content_id 校验别名。</summary>
        public static bool IsStableContentId(string contentId)
        {
            return IsStableAnimalContentId(contentId);
        }

        /// <summary>判断分类是否为一个合法的基础类加可选珍稀横切标记。</summary>
        public static bool IsSupportedClassification(D20AnimalClassification classification)
        {
            int raw = (int)classification;
            if ((raw & ~((int)D20AnimalClassification.Terrestrial |
                         (int)D20AnimalClassification.Bird |
                         (int)D20AnimalClassification.Aquatic |
                         (int)D20AnimalClassification.RareSpecies |
                         (int)D20AnimalClassification.Pet)) != 0)
                return false;

            int baseBits = raw & BaseClassificationMask;
            if (baseBits == 0 || !HasExactlyOneBit(baseBits)) return false;
            if ((raw & (int)D20AnimalClassification.Pet) != 0 &&
                (raw & (int)D20AnimalClassification.RareSpecies) != 0)
                return false;
            return true;
        }

        /// <summary>判断枚举值是否为支持的动物管理类型。</summary>
        public static bool IsSupportedPopulationKind(D20AnimalPopulationKind kind)
        {
            return kind == D20AnimalPopulationKind.Wild ||
                   kind == D20AnimalPopulationKind.Domestic ||
                   kind == D20AnimalPopulationKind.Pet;
        }

        /// <summary>判断枚举值是否为支持的年龄阶段。</summary>
        public static bool IsSupportedLifeStage(D20AnimalLifeStage lifeStage)
        {
            return lifeStage >= D20AnimalLifeStage.Neonate &&
                   lifeStage <= D20AnimalLifeStage.Elder;
        }

        /// <summary>判断枚举值是否为支持的 D20 生命周期投影。</summary>
        public static bool IsSupportedLifecycle(D20AnimalLifecycleStatus lifecycle)
        {
            return lifecycle >= D20AnimalLifecycleStatus.Alive &&
                   lifecycle <= D20AnimalLifecycleStatus.Retired;
        }

        /// <summary>判断分类是否含有指定分类位。</summary>
        public static bool HasClassification(
            D20AnimalClassification classification,
            D20AnimalClassification expected)
        {
            return expected != D20AnimalClassification.None &&
                   (classification & expected) == expected;
        }

        /// <summary>返回单一基础分类；无效组合返回 None。</summary>
        public static D20AnimalClassification PrimaryClassification(
            D20AnimalClassification classification)
        {
            if (!IsSupportedClassification(classification))
                return D20AnimalClassification.None;
            return classification & (D20AnimalClassification.Terrestrial |
                                     D20AnimalClassification.Bird |
                                     D20AnimalClassification.Aquatic |
                                     D20AnimalClassification.Pet);
        }

        /// <summary>按动物管理类型返回 D18 冻结的 owner_system。</summary>
        public static string OwnerSystemFor(D20AnimalPopulationKind populationKind)
        {
            return populationKind == D20AnimalPopulationKind.Pet
                ? PetOwnerSystem : EcologyOwnerSystem;
        }

        /// <summary>构造一个可供新实体使用的最小安全身份投影。</summary>
        public static D20AnimalIdentity CreateDefault(
            string animalId,
            string contentId,
            string speciesId,
            D20AnimalClassification classification,
            D20AnimalPopulationKind populationKind = D20AnimalPopulationKind.Wild,
            D20AnimalOwnershipKind ownershipKind = D20AnimalOwnershipKind.None,
            string ownerId = "")
        {
            return new D20AnimalIdentity
            {
                SchemaVersion = CurrentSchemaVersion,
                AnimalId = animalId ?? string.Empty,
                ContentId = contentId ?? string.Empty,
                SpeciesId = string.IsNullOrEmpty(speciesId) ? contentId ?? string.Empty : speciesId,
                Classification = classification,
                PopulationKind = populationKind,
                OwnershipKind = ownershipKind,
                OwnerId = ownerId ?? string.Empty,
                OwnerSystem = OwnerSystemFor(populationKind),
                LifeStage = D20AnimalLifeStage.Adult,
                AgeDays = 0,
                LifecycleState = D20AnimalLifecycleStatus.Alive,
                BirthWorldDay = 0,
                DeathWorldDay = -1,
                LastUpdatedWorldDay = 0,
                LastCauseEventId = string.Empty,
                OriginRef = "content:" + (contentId ?? string.Empty),
                SaveRevision = 0,
            };
        }

        /// <summary>以结果形式验证身份，不会修正或写入对象。</summary>
        public static D20AnimalIdentityValidationResult Validate(
            D20AnimalIdentity identity)
        {
            if (identity == null)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.MissingIdentity, "animal_identity_missing");
            if (identity.SchemaVersion != CurrentSchemaVersion)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidSchemaVersion, "animal_identity_schema_version_mismatch");
            if (!IsStableAnimalId(identity.AnimalId))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidAnimalId, "animal_id_invalid");
            if (!IsStableAnimalContentId(identity.ContentId))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidContentId, "animal_content_id_invalid");
            if (!IsStableSpeciesId(identity.SpeciesId))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidSpeciesId, "animal_species_id_invalid");
            if (!IsSupportedClassification(identity.Classification))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidClassification, "animal_classification_invalid");
            if (!IsSupportedPopulationKind(identity.PopulationKind))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.PopulationClassificationMismatch, "animal_population_kind_invalid");

            bool contentIsPet = identity.ContentId.StartsWith("pet_", StringComparison.Ordinal);
            bool classificationIsPet = HasClassification(identity.Classification, D20AnimalClassification.Pet);
            if (contentIsPet != classificationIsPet ||
                (identity.PopulationKind == D20AnimalPopulationKind.Pet) != classificationIsPet)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.PopulationClassificationMismatch, "animal_pet_classification_mismatch");
            if (identity.PopulationKind != D20AnimalPopulationKind.Pet && classificationIsPet)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.PopulationClassificationMismatch, "animal_non_pet_cannot_use_pet_category");

            string expectedOwner = OwnerSystemFor(identity.PopulationKind);
            if (!string.Equals(identity.OwnerSystem, expectedOwner, StringComparison.Ordinal))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidOwnerSystem, "animal_owner_system_mismatch");

            bool hasOwner = identity.OwnershipKind != D20AnimalOwnershipKind.None;
            if (identity.OwnershipKind < D20AnimalOwnershipKind.None ||
                identity.OwnershipKind > D20AnimalOwnershipKind.External)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidOwnership, "animal_ownership_kind_invalid");
            if (identity.PopulationKind == D20AnimalPopulationKind.Wild && hasOwner)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidOwnership, "wild_animal_cannot_have_individual_owner");
            if (!hasOwner && !string.IsNullOrEmpty(identity.OwnerId))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidOwnership, "owner_id_without_owner_kind");
            if (hasOwner && !IsStableReference(identity.OwnerId, ReferenceMaxLength))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidOwnership, "owner_id_invalid");
            if (identity.PopulationKind == D20AnimalPopulationKind.Pet && !hasOwner)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidOwnership, "pet_owner_required");

            if (!IsSupportedLifeStage(identity.LifeStage))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidLifeStage, "animal_life_stage_invalid");
            if (!IsSupportedLifecycle(identity.LifecycleState))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidLifecycle, "animal_lifecycle_invalid");
            if (identity.AgeDays < -1 || identity.AgeDays > 200000)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidLifecycleDates, "animal_age_days_invalid");
            if (identity.BirthWorldDay < 0 || identity.LastUpdatedWorldDay < identity.BirthWorldDay)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidLifecycleDates, "animal_world_day_invalid");
            if (identity.LifecycleState == D20AnimalLifecycleStatus.Dead)
            {
                if (identity.DeathWorldDay < identity.BirthWorldDay)
                    return D20AnimalIdentityValidationResult.Failure(
                        D20AnimalIdentityValidationCode.InvalidLifecycleDates, "animal_death_day_invalid");
            }
            else if (identity.DeathWorldDay != -1)
            {
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidLifecycleDates, "living_animal_cannot_have_death_day");
            }
            if (identity.OriginRef != null && identity.OriginRef.Length > ReferenceMaxLength)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidReference, "animal_origin_ref_too_long");
            if (!string.IsNullOrEmpty(identity.OriginRef) &&
                !IsStableReference(identity.OriginRef, ReferenceMaxLength))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidReference, "animal_origin_ref_invalid");
            if (!string.IsNullOrEmpty(identity.LastCauseEventId) &&
                !IsStableReference(identity.LastCauseEventId, ReferenceMaxLength))
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidReference, "animal_cause_event_id_invalid");
            if (identity.SaveRevision < 0)
                return D20AnimalIdentityValidationResult.Failure(
                    D20AnimalIdentityValidationCode.InvalidSaveRevision, "animal_save_revision_invalid");

            return D20AnimalIdentityValidationResult.Success();
        }

        /// <summary>
        /// 验证身份并在失败时抛出确定性错误。调用方不得在失败后继续写入
        /// D11、Ecology 或其它领域 owner。
        /// </summary>
        public static void ValidateOrThrow(D20AnimalIdentity identity)
        {
            D20AnimalIdentityValidationResult result = Validate(identity);
            if (!result.Valid)
                throw new InvalidOperationException(result.ReasonCode);
        }

        private static bool IsStableReference(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maxLength || value.Trim() != value)
                return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool letter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == ':' || c == '_' || c == '-' || c == '.';
                if (!letter && !digit && !separator) return false;
            }
            return true;
        }

        private static bool IsLowerSnakeCase(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maxLength) return false;
            if (value[0] < 'a' || value[0] > 'z') return false;
            bool previousSeparator = false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool lower = c >= 'a' && c <= 'z';
                bool digit = c >= '0' && c <= '9';
                if (c == '_')
                {
                    if (i == 0 || previousSeparator || i == value.Length - 1) return false;
                    previousSeparator = true;
                    continue;
                }
                if (!lower && !digit) return false;
                previousSeparator = false;
            }
            return !previousSeparator;
        }

        private static bool HasExactlyOneBit(int value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }
    }

    /// <summary>
    /// D20 动物身份的可序列化语义投影。它不拥有 D11 宠物生命/照料状态，
    /// 也不复制 EcologySystem 的数量、压力、健康或空间坐标。
    /// </summary>
    [Serializable]
    public sealed class D20AnimalIdentity
    {
        /// <summary>契约版本。</summary>
        public int SchemaVersion = D20AnimalIdentityContracts.CurrentSchemaVersion;

        /// <summary>稳定动物实例 ID。</summary>
        public string AnimalId = string.Empty;

        /// <summary>D18 冻结的稳定内容定义 ID。</summary>
        public string ContentId = string.Empty;

        /// <summary>外部生态/宠物 owner 使用的物种稳定引用。</summary>
        public string SpeciesId = string.Empty;

        /// <summary>D18 基础分类及可选珍稀横切标记。</summary>
        public D20AnimalClassification Classification = D20AnimalClassification.None;

        /// <summary>野生、家养或宠物语义。</summary>
        public D20AnimalPopulationKind PopulationKind = D20AnimalPopulationKind.Unknown;

        /// <summary>归属类型；只表达引用，不代表外部 owner 已完成转移。</summary>
        public D20AnimalOwnershipKind OwnershipKind = D20AnimalOwnershipKind.None;

        /// <summary>归属实体稳定引用，野生动物必须为空。</summary>
        public string OwnerId = string.Empty;

        /// <summary>裁决归属事实的外部 owner 标识。</summary>
        public string OwnerSystem = D20AnimalIdentityContracts.EcologyOwnerSystem;

        /// <summary>离散年龄阶段。</summary>
        public D20AnimalLifeStage LifeStage = D20AnimalLifeStage.Unknown;

        /// <summary>已知时的年龄天数；年龄未知使用 -1。</summary>
        public int AgeDays = -1;

        /// <summary>D20 自有生命周期投影。</summary>
        public D20AnimalLifecycleStatus LifecycleState = D20AnimalLifecycleStatus.Unknown;

        /// <summary>出生/建立的世界日；契约默认使用非负显式日值。</summary>
        public int BirthWorldDay;

        /// <summary>死亡世界日；存活或退役时为 -1。</summary>
        public int DeathWorldDay = -1;

        /// <summary>最近一次 D20 投影更新的世界日。</summary>
        public int LastUpdatedWorldDay;

        /// <summary>创建来源的稳定语义引用，不能是资源路径。</summary>
        public string OriginRef = string.Empty;

        /// <summary>最近一次生命周期事实的因果事件引用。</summary>
        public string LastCauseEventId = string.Empty;

        /// <summary>对应逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>稳定动物 ID 的只读别名。</summary>
        public string Id { get { return AnimalId ?? string.Empty; } }

        /// <summary>物种引用的只读别名。</summary>
        public string Species { get { return SpeciesId ?? string.Empty; } }

        /// <summary>生命周期字段的兼容别名。</summary>
        public D20AnimalLifecycleStatus Lifecycle
        {
            get { return LifecycleState; }
            set { LifecycleState = value; }
        }

        /// <summary>年龄字段的兼容别名。</summary>
        public int AgeInDays
        {
            get { return AgeDays; }
            set { AgeDays = value; }
        }

        /// <summary>分类字段的兼容别名。</summary>
        public D20AnimalClassification Category
        {
            get { return Classification; }
            set { Classification = value; }
        }

        /// <summary>是否是野生个体语义。</summary>
        public bool IsWild { get { return PopulationKind == D20AnimalPopulationKind.Wild; } }

        /// <summary>是否是宠物个体语义。</summary>
        public bool IsPet { get { return PopulationKind == D20AnimalPopulationKind.Pet; } }

        /// <summary>返回深拷贝，防止读模型调用方通过别名修改原投影。</summary>
        public D20AnimalIdentity Clone()
        {
            return new D20AnimalIdentity
            {
                SchemaVersion = SchemaVersion,
                AnimalId = AnimalId ?? string.Empty,
                ContentId = ContentId ?? string.Empty,
                SpeciesId = SpeciesId ?? string.Empty,
                Classification = Classification,
                PopulationKind = PopulationKind,
                OwnershipKind = OwnershipKind,
                OwnerId = OwnerId ?? string.Empty,
                OwnerSystem = OwnerSystem ?? string.Empty,
                LifeStage = LifeStage,
                AgeDays = AgeDays,
                LifecycleState = LifecycleState,
                BirthWorldDay = BirthWorldDay,
                DeathWorldDay = DeathWorldDay,
                LastUpdatedWorldDay = LastUpdatedWorldDay,
                OriginRef = OriginRef ?? string.Empty,
                LastCauseEventId = LastCauseEventId ?? string.Empty,
                SaveRevision = SaveRevision,
            };
        }

        /// <summary>返回校验结果，不修改本对象。</summary>
        public D20AnimalIdentityValidationResult TryValidate()
        {
            return D20AnimalIdentityContracts.Validate(this);
        }

        /// <summary>验证本投影；失败时抛出确定性错误。</summary>
        public void Validate()
        {
            D20AnimalIdentityContracts.ValidateOrThrow(this);
        }
    }
}
