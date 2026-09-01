// ============================================================
// D20AnimalSpeciesCatalog —— D20-L21 动物物种目录（纯 C#）
//
// 目录只登记 D18 已冻结的逻辑 content_id、生态分类、owner 和行为能力
// 标签。它不生成实例、不保存数量/健康/压力，不检查候选路径，也不把任何
// 资源或视觉状态写成运行时事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D20
{
    /// <summary>动物可消费的行为能力标签；标签不包含生态数值或视觉资源。</summary>
    [Flags]
    public enum D20AnimalBehaviorAbility
    {
        /// <summary>没有声明行为能力。</summary>
        None = 0,
        /// <summary>可以进入休息状态。</summary>
        Rest = 1,
        /// <summary>可以提出觅食意图。</summary>
        Forage = 2,
        /// <summary>可以感知并进入警觉状态。</summary>
        Alert = 4,
        /// <summary>可以提出逃离意图。</summary>
        Flee = 8,
        /// <summary>可以接受跟随目标意图。</summary>
        Follow = 16,
        /// <summary>可以从受伤语义恢复到活动状态。</summary>
        Recover = 32,
        /// <summary>可以参与非战斗社会互动投影。</summary>
        Social = 64,
        /// <summary>内容元数据标记为飞行/空中活动。</summary>
        Fly = 128,
        /// <summary>内容元数据标记为水生活动。</summary>
        Aquatic = 256,
        /// <summary>内容元数据标记为家养照料对象，但不等于宠物。</summary>
        DomesticCare = 512,
        /// <summary>可被观察/图鉴 owner 作为稀有物种投影消费。</summary>
        Observe = 1024,
    }

    /// <summary>便于调用方按单一分类读取定义的语义枚举。</summary>
    public enum D20AnimalSpeciesCategory
    {
        /// <summary>未分类。</summary>
        Unknown = 0,
        /// <summary>D18 terrestrial_animal。</summary>
        Terrestrial = 1,
        /// <summary>D18 bird。</summary>
        Bird = 2,
        /// <summary>D18 aquatic_animal。</summary>
        Aquatic = 3,
        /// <summary>D18 rare_species 横切标记。</summary>
        RareSpecies = 4,
        /// <summary>D18 pet。</summary>
        Pet = 5,
    }

    /// <summary>物种定义的目录校验码。</summary>
    public enum D20AnimalSpeciesValidationCode
    {
        /// <summary>校验通过。</summary>
        None = 0,
        /// <summary>定义为空。</summary>
        MissingDefinition = 1,
        /// <summary>schema_version 不支持。</summary>
        InvalidSchemaVersion = 2,
        /// <summary>content_id 不符合 D18 前缀/格式。</summary>
        InvalidContentId = 3,
        /// <summary>物种引用不稳定。</summary>
        InvalidSpeciesId = 4,
        /// <summary>分类位或 taxonomy_category 不一致。</summary>
        InvalidClassification = 5,
        /// <summary>管理类型与分类不一致。</summary>
        InvalidPopulationKind = 6,
        /// <summary>owner_system 不符合 D18 唯一写者。</summary>
        InvalidOwnerSystem = 7,
        /// <summary>能力标签包含未冻结的位。</summary>
        InvalidAbility = 8,
        /// <summary>行为配置引用不稳定。</summary>
        InvalidBehaviorProfile = 9,
        /// <summary>来源引用缺失或不安全。</summary>
        InvalidSourceReference = 10,
    }

    /// <summary>物种定义的纯逻辑校验结果。</summary>
    [Serializable]
    public sealed class D20AnimalSpeciesValidationResult
    {
        /// <summary>是否通过校验。</summary>
        public bool Valid;

        /// <summary>稳定校验码。</summary>
        public D20AnimalSpeciesValidationCode Code;

        /// <summary>机器可读原因键。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>创建成功结果。</summary>
        public static D20AnimalSpeciesValidationResult Success()
        {
            return new D20AnimalSpeciesValidationResult
            {
                Valid = true,
                Code = D20AnimalSpeciesValidationCode.None,
                ReasonCode = string.Empty,
            };
        }

        /// <summary>创建失败结果。</summary>
        public static D20AnimalSpeciesValidationResult Failure(
            D20AnimalSpeciesValidationCode code,
            string reasonCode)
        {
            return new D20AnimalSpeciesValidationResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>
    /// D18 逻辑物种定义。所有成员都是内容元数据；当前实例状态必须由
    /// EcologySystem 或 AnimalCareSystem 的公开接口提供。
    /// </summary>
    [Serializable]
    public sealed class D20AnimalSpeciesDefinition
    {
        /// <summary>目录条目的 schema 版本。</summary>
        public int SchemaVersion = D20AnimalIdentityContracts.CurrentSchemaVersion;

        /// <summary>D18 冻结的稳定内容定义 ID。</summary>
        public string ContentId = string.Empty;

        /// <summary>外部生态/宠物 owner 使用的稳定物种引用。</summary>
        public string SpeciesId = string.Empty;

        /// <summary>基础分类与可选 RareSpecies 横切标记。</summary>
        public D20AnimalClassification Classification = D20AnimalClassification.None;

        /// <summary>D18 生态分类键；rare_species 作为横切标记保留在 Classification。</summary>
        public string TaxonomyCategory = string.Empty;

        /// <summary>野生、家养或宠物管理语义。</summary>
        public D20AnimalPopulationKind PopulationKind = D20AnimalPopulationKind.Unknown;

        /// <summary>实际裁决内容事实的唯一 owner_system。</summary>
        public string OwnerSystem = string.Empty;

        /// <summary>只读行为能力位集合。</summary>
        public D20AnimalBehaviorAbility Capabilities;

        /// <summary>D18 行为 profile 的稳定逻辑引用；不是资源路径。</summary>
        public string BehaviorProfileId = string.Empty;

        /// <summary>内容定义来源的稳定逻辑引用。</summary>
        public string SourceRef = string.Empty;

        /// <summary>是否具有 D18 rare_species 横切标记。</summary>
        public bool IsRareSpecies
        {
            get { return D20AnimalIdentityContracts.HasClassification(Classification, D20AnimalClassification.RareSpecies); }
        }

        /// <summary>是否是宠物内容。</summary>
        public bool IsPet
        {
            get { return PopulationKind == D20AnimalPopulationKind.Pet; }
        }

        /// <summary>是否是野生内容。</summary>
        public bool IsWild
        {
            get { return PopulationKind == D20AnimalPopulationKind.Wild; }
        }

        /// <summary>返回主要基础分类；稀有标记不覆盖基础分类。</summary>
        public D20AnimalSpeciesCategory PrimaryCategory
        {
            get { return D20AnimalSpeciesCatalog.CategoryFor(Classification); }
        }

        /// <summary>
        /// 供需要单一显示分类的调用方使用；稀有定义返回 RareSpecies，
        /// 同时仍可通过 PrimaryCategory 取得原始基础分类。
        /// </summary>
        public D20AnimalSpeciesCategory Category
        {
            get { return IsRareSpecies ? D20AnimalSpeciesCategory.RareSpecies : PrimaryCategory; }
        }

        /// <summary>以只读字符串列表投影能力标签，不引入资源引用。</summary>
        public IReadOnlyList<string> AbilityTags
        {
            get { return D20AnimalSpeciesCatalog.AbilityTagIdsFor(Capabilities); }
        }

        /// <summary>返回防御性副本。</summary>
        public D20AnimalSpeciesDefinition Clone()
        {
            return new D20AnimalSpeciesDefinition
            {
                SchemaVersion = SchemaVersion,
                ContentId = ContentId ?? string.Empty,
                SpeciesId = SpeciesId ?? string.Empty,
                Classification = Classification,
                TaxonomyCategory = TaxonomyCategory ?? string.Empty,
                PopulationKind = PopulationKind,
                OwnerSystem = OwnerSystem ?? string.Empty,
                Capabilities = Capabilities,
                BehaviorProfileId = BehaviorProfileId ?? string.Empty,
                SourceRef = SourceRef ?? string.Empty,
            };
        }

        /// <summary>返回校验结果，不修改定义。</summary>
        public D20AnimalSpeciesValidationResult TryValidate()
        {
            return D20AnimalSpeciesCatalog.ValidateDefinition(this);
        }

        /// <summary>验证定义并在失败时抛出确定性错误。</summary>
        public void Validate()
        {
            D20AnimalSpeciesValidationResult result = TryValidate();
            if (!result.Valid) throw new InvalidOperationException(result.ReasonCode);
        }
    }

    /// <summary>
    /// D20-L21 稳定物种目录。条目只来自 D18/D11 已冻结的逻辑 ID；目录
    /// 不读取磁盘 Registry，不检查模型是否存在，也不声明 RuntimeApproved。
    /// </summary>
    public static class D20AnimalSpeciesCatalog
    {
        /// <summary>目录 owner 标识。</summary>
        public const string OwnerSystem = "D20AnimalSpeciesCatalog";

        /// <summary>D18 terrestrial_animal 分类键。</summary>
        public const string TerrestrialCategoryId = "terrestrial_animal";

        /// <summary>D18 bird 分类键。</summary>
        public const string BirdCategoryId = "bird";

        /// <summary>D18 aquatic_animal 分类键。</summary>
        public const string AquaticCategoryId = "aquatic_animal";

        /// <summary>D18 rare_species 横切分类键。</summary>
        public const string RareSpeciesCategoryId = "rare_species";

        /// <summary>D18 pet 分类键。</summary>
        public const string PetCategoryId = "pet";

        /// <summary>陆生代表：森林刺豚鼠。</summary>
        public const string AgoutiForestContentId = "animal_agouti_forest";

        /// <summary>家养代表：加勒比家鸡。</summary>
        public const string DomesticChickenContentId = "animal_domestic_chicken";

        /// <summary>鸟类代表：蛇鹈。</summary>
        public const string AnhingaContentId = "animal_anhinga";

        /// <summary>鸟类代表：加勒比鹦鹉。</summary>
        public const string CaribbeanParrotContentId = "animal_caribbean_parrot";

        /// <summary>水生代表：鲭鱼鱼群。</summary>
        public const string MackerelSchoolContentId = "fish_mackerel_school";

        /// <summary>珍稀水生代表：绿海龟。</summary>
        public const string GreenTurtleContentId = "animal_green_turtle";

        /// <summary>珍稀水生代表：宽吻海豚。</summary>
        public const string BottlenoseDolphinContentId = "fish_bottlenose_dolphin";

        /// <summary>D11 宠物代表：船舱猫。</summary>
        public const string CabinCatContentId = "pet_cabin_cat";

        /// <summary>D11 宠物代表：港口犬。</summary>
        public const string HarborDogContentId = "pet_harbor_dog";

        private const D20AnimalBehaviorAbility AllCapabilities =
            D20AnimalBehaviorAbility.Rest |
            D20AnimalBehaviorAbility.Forage |
            D20AnimalBehaviorAbility.Alert |
            D20AnimalBehaviorAbility.Flee |
            D20AnimalBehaviorAbility.Follow |
            D20AnimalBehaviorAbility.Recover |
            D20AnimalBehaviorAbility.Social |
            D20AnimalBehaviorAbility.Fly |
            D20AnimalBehaviorAbility.Aquatic |
            D20AnimalBehaviorAbility.DomesticCare |
            D20AnimalBehaviorAbility.Observe;

        private static readonly IReadOnlyList<D20AnimalSpeciesDefinition> Definitions =
            new List<D20AnimalSpeciesDefinition>
            {
                Definition(AgoutiForestContentId, AgoutiForestContentId,
                    D20AnimalClassification.Terrestrial, TerrestrialCategoryId,
                    D20AnimalPopulationKind.Wild, D20AnimalBehaviorAbility.Rest |
                    D20AnimalBehaviorAbility.Forage | D20AnimalBehaviorAbility.Alert |
                    D20AnimalBehaviorAbility.Flee,
                    "ecology.d15.terrestrial.l2", "d18:taxonomy:terrestrial_animal"),
                Definition(DomesticChickenContentId, DomesticChickenContentId,
                    D20AnimalClassification.Terrestrial, TerrestrialCategoryId,
                    D20AnimalPopulationKind.Domestic, D20AnimalBehaviorAbility.Rest |
                    D20AnimalBehaviorAbility.Forage | D20AnimalBehaviorAbility.Alert |
                    D20AnimalBehaviorAbility.Flee | D20AnimalBehaviorAbility.DomesticCare,
                    "ecology.d15.terrestrial.l1", "d18:taxonomy:terrestrial_animal"),
                Definition(AnhingaContentId, AnhingaContentId,
                    D20AnimalClassification.Bird, BirdCategoryId,
                    D20AnimalPopulationKind.Wild, D20AnimalBehaviorAbility.Rest |
                    D20AnimalBehaviorAbility.Forage | D20AnimalBehaviorAbility.Alert |
                    D20AnimalBehaviorAbility.Flee | D20AnimalBehaviorAbility.Fly,
                    "ecology.d15.coastal_birds.l3", "d18:taxonomy:bird"),
                Definition(CaribbeanParrotContentId, CaribbeanParrotContentId,
                    D20AnimalClassification.Bird, BirdCategoryId,
                    D20AnimalPopulationKind.Wild, D20AnimalBehaviorAbility.Rest |
                    D20AnimalBehaviorAbility.Forage | D20AnimalBehaviorAbility.Alert |
                    D20AnimalBehaviorAbility.Flee | D20AnimalBehaviorAbility.Fly |
                    D20AnimalBehaviorAbility.Social,
                    "ecology.d15.birds.l0", "d18:taxonomy:bird"),
                Definition(MackerelSchoolContentId, "mackerel",
                    D20AnimalClassification.Aquatic, AquaticCategoryId,
                    D20AnimalPopulationKind.Wild, D20AnimalBehaviorAbility.Rest |
                    D20AnimalBehaviorAbility.Forage | D20AnimalBehaviorAbility.Alert |
                    D20AnimalBehaviorAbility.Flee | D20AnimalBehaviorAbility.Aquatic,
                    "ecology.d15.nearshore_fish.l2", "d18:taxonomy:aquatic_animal"),
                Definition(GreenTurtleContentId, GreenTurtleContentId,
                    D20AnimalClassification.Aquatic | D20AnimalClassification.RareSpecies,
                    AquaticCategoryId, D20AnimalPopulationKind.Wild,
                    D20AnimalBehaviorAbility.Rest | D20AnimalBehaviorAbility.Forage |
                    D20AnimalBehaviorAbility.Alert | D20AnimalBehaviorAbility.Flee |
                    D20AnimalBehaviorAbility.Aquatic | D20AnimalBehaviorAbility.Observe,
                    "ecology.d15.sea_turtles.l3", "d18:taxonomy:aquatic_animal"),
                Definition(BottlenoseDolphinContentId, BottlenoseDolphinContentId,
                    D20AnimalClassification.Aquatic | D20AnimalClassification.RareSpecies,
                    AquaticCategoryId, D20AnimalPopulationKind.Wild,
                    D20AnimalBehaviorAbility.Rest | D20AnimalBehaviorAbility.Forage |
                    D20AnimalBehaviorAbility.Alert | D20AnimalBehaviorAbility.Flee |
                    D20AnimalBehaviorAbility.Aquatic | D20AnimalBehaviorAbility.Observe |
                    D20AnimalBehaviorAbility.Social,
                    "ecology.d15.whales_dolphins.l4", "d18:taxonomy:aquatic_animal"),
                Definition(CabinCatContentId, "cat",
                    D20AnimalClassification.Pet, PetCategoryId,
                    D20AnimalPopulationKind.Pet, D20AnimalBehaviorAbility.Rest |
                    D20AnimalBehaviorAbility.Alert | D20AnimalBehaviorAbility.Flee |
                    D20AnimalBehaviorAbility.Follow | D20AnimalBehaviorAbility.Recover |
                    D20AnimalBehaviorAbility.Social,
                    "animalcare.pet.cat", "d18:taxonomy:pet"),
                Definition(HarborDogContentId, "dog",
                    D20AnimalClassification.Pet, PetCategoryId,
                    D20AnimalPopulationKind.Pet, D20AnimalBehaviorAbility.Rest |
                    D20AnimalBehaviorAbility.Alert | D20AnimalBehaviorAbility.Flee |
                    D20AnimalBehaviorAbility.Follow | D20AnimalBehaviorAbility.Recover |
                    D20AnimalBehaviorAbility.Social,
                    "animalcare.pet.dog", "d18:taxonomy:pet"),
            }.AsReadOnly();

        /// <summary>返回定义的防御性副本集合。</summary>
        public static IReadOnlyList<D20AnimalSpeciesDefinition> All
        {
            get
            {
                var result = new List<D20AnimalSpeciesDefinition>();
                foreach (var definition in Definitions) result.Add(definition.Clone());
                return new ReadOnlyCollection<D20AnimalSpeciesDefinition>(result);
            }
        }

        /// <summary>按稳定 content_id 读取定义副本。</summary>
        public static D20AnimalSpeciesDefinition Find(string contentId)
        {
            if (string.IsNullOrEmpty(contentId)) return null;
            foreach (var definition in Definitions)
                if (string.Equals(definition.ContentId, contentId, StringComparison.Ordinal))
                    return definition.Clone();
            return null;
        }

        /// <summary>按稳定 content_id 读取定义。</summary>
        public static bool TryGetDefinition(string contentId,
            out D20AnimalSpeciesDefinition definition)
        {
            definition = Find(contentId);
            return definition != null;
        }

        /// <summary>判断 content_id 是否是本目录的已知稳定定义。</summary>
        public static bool IsKnownContentId(string contentId)
        {
            return Find(contentId) != null;
        }

        /// <summary>按分类读取定义副本；RareSpecies 按横切位筛选。</summary>
        public static IReadOnlyList<D20AnimalSpeciesDefinition> FindByCategory(
            D20AnimalClassification category)
        {
            var result = new List<D20AnimalSpeciesDefinition>();
            foreach (var definition in Definitions)
            {
                if (D20AnimalIdentityContracts.HasClassification(definition.Classification, category))
                    result.Add(definition.Clone());
            }
            return new ReadOnlyCollection<D20AnimalSpeciesDefinition>(result);
        }

        /// <summary>把基础分类位映射为 D18 语义分类。</summary>
        public static D20AnimalSpeciesCategory CategoryFor(
            D20AnimalClassification classification)
        {
            D20AnimalClassification primary = D20AnimalIdentityContracts.PrimaryClassification(classification);
            if (primary == D20AnimalClassification.Terrestrial) return D20AnimalSpeciesCategory.Terrestrial;
            if (primary == D20AnimalClassification.Bird) return D20AnimalSpeciesCategory.Bird;
            if (primary == D20AnimalClassification.Aquatic) return D20AnimalSpeciesCategory.Aquatic;
            if (primary == D20AnimalClassification.Pet) return D20AnimalSpeciesCategory.Pet;
            return D20AnimalSpeciesCategory.Unknown;
        }

        /// <summary>把分类位映射为 D18 taxonomy_category 字符串。</summary>
        public static string TaxonomyCategoryFor(D20AnimalClassification classification)
        {
            switch (CategoryFor(classification))
            {
                case D20AnimalSpeciesCategory.Terrestrial: return TerrestrialCategoryId;
                case D20AnimalSpeciesCategory.Bird: return BirdCategoryId;
                case D20AnimalSpeciesCategory.Aquatic: return AquaticCategoryId;
                case D20AnimalSpeciesCategory.Pet: return PetCategoryId;
                default: return string.Empty;
            }
        }

        /// <summary>返回能力位的稳定标签键，顺序固定以便日志/快照可重放。</summary>
        public static IReadOnlyList<string> AbilityTagIdsFor(
            D20AnimalBehaviorAbility capabilities)
        {
            var result = new List<string>();
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.Rest, "rest");
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.Forage, "forage");
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.Alert, "alert");
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.Flee, "flee");
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.Follow, "follow");
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.Recover, "recover");
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.Social, "social");
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.Fly, "fly");
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.Aquatic, "aquatic");
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.DomesticCare, "domestic_care");
            AddAbilityTag(result, capabilities, D20AnimalBehaviorAbility.Observe, "observe");
            return new ReadOnlyCollection<string>(result);
        }

        /// <summary>校验单个定义；不会访问 Registry 或资源文件。</summary>
        public static D20AnimalSpeciesValidationResult ValidateDefinition(
            D20AnimalSpeciesDefinition definition)
        {
            if (definition == null)
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.MissingDefinition, "animal_species_definition_missing");
            if (definition.SchemaVersion != D20AnimalIdentityContracts.CurrentSchemaVersion)
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidSchemaVersion, "animal_species_schema_version_mismatch");
            if (!D20AnimalIdentityContracts.IsStableAnimalContentId(definition.ContentId))
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidContentId, "animal_species_content_id_invalid");
            if (!D20AnimalIdentityContracts.IsStableSpeciesId(definition.SpeciesId))
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidSpeciesId, "animal_species_id_invalid");
            if (!D20AnimalIdentityContracts.IsSupportedClassification(definition.Classification) ||
                !string.Equals(TaxonomyCategoryFor(definition.Classification), definition.TaxonomyCategory,
                    StringComparison.Ordinal))
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidClassification, "animal_species_category_invalid");
            if (!D20AnimalIdentityContracts.IsSupportedPopulationKind(definition.PopulationKind))
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidPopulationKind, "animal_species_population_invalid");

            bool petPrefix = definition.ContentId.StartsWith("pet_", StringComparison.Ordinal);
            bool petCategory = D20AnimalIdentityContracts.HasClassification(
                definition.Classification, D20AnimalClassification.Pet);
            if (petPrefix != petCategory ||
                (definition.PopulationKind == D20AnimalPopulationKind.Pet) != petCategory)
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidPopulationKind, "animal_species_pet_boundary_invalid");
            if (!string.Equals(definition.OwnerSystem,
                    D20AnimalIdentityContracts.OwnerSystemFor(definition.PopulationKind),
                    StringComparison.Ordinal))
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidOwnerSystem, "animal_species_owner_system_invalid");
            if ((definition.Capabilities & ~AllCapabilities) != 0 ||
                !D20AnimalIdentityContracts.HasClassification(definition.Classification,
                    D20AnimalClassification.Pet) &&
                (definition.Capabilities & D20AnimalBehaviorAbility.Follow) != 0)
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidAbility, "animal_species_ability_invalid");
            if (!D20AnimalIdentityContracts.IsStableSpeciesId(definition.BehaviorProfileId))
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidBehaviorProfile, "animal_species_behavior_profile_invalid");
            if (!D20AnimalIdentityContracts.IsStableSpeciesId(definition.SourceRef))
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidSourceReference, "animal_species_source_ref_invalid");
            if (definition.IsRareSpecies && definition.PopulationKind == D20AnimalPopulationKind.Pet)
                return D20AnimalSpeciesValidationResult.Failure(
                    D20AnimalSpeciesValidationCode.InvalidClassification, "pet_cannot_use_rare_species_bit");

            return D20AnimalSpeciesValidationResult.Success();
        }

        /// <summary>检查整个静态目录内部一致性，返回空列表即通过。</summary>
        public static List<string> ValidateCatalog()
        {
            var errors = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in Definitions)
            {
                D20AnimalSpeciesValidationResult validation = ValidateDefinition(definition);
                if (!validation.Valid)
                    errors.Add(validation.ReasonCode + ":" + (definition == null ? string.Empty : definition.ContentId));
                if (definition == null) continue;
                if (!ids.Add(definition.ContentId))
                    errors.Add("animal_species_content_id_duplicate:" + definition.ContentId);
                if ((definition.Capabilities & D20AnimalBehaviorAbility.Rest) == 0)
                    errors.Add("animal_species_rest_ability_missing:" + definition.ContentId);
                if ((definition.Capabilities & D20AnimalBehaviorAbility.Alert) == 0)
                    errors.Add("animal_species_alert_ability_missing:" + definition.ContentId);
            }
            return errors;
        }

        private static D20AnimalSpeciesDefinition Definition(
            string contentId,
            string speciesId,
            D20AnimalClassification classification,
            string taxonomyCategory,
            D20AnimalPopulationKind populationKind,
            D20AnimalBehaviorAbility capabilities,
            string behaviorProfileId,
            string sourceRef)
        {
            return new D20AnimalSpeciesDefinition
            {
                ContentId = contentId,
                SpeciesId = speciesId,
                Classification = classification,
                TaxonomyCategory = taxonomyCategory,
                PopulationKind = populationKind,
                OwnerSystem = D20AnimalIdentityContracts.OwnerSystemFor(populationKind),
                Capabilities = capabilities,
                BehaviorProfileId = behaviorProfileId,
                SourceRef = sourceRef,
            };
        }

        private static void AddAbilityTag(
            List<string> target,
            D20AnimalBehaviorAbility capabilities,
            D20AnimalBehaviorAbility ability,
            string tag)
        {
            if ((capabilities & ability) != 0) target.Add(tag);
        }
    }
}
