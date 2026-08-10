// D14-A06 代表性宗教/节期/饮食/建筑/礼仪目录。
// 目录只登记有来源和审查状态的上下文，不把现实宗教或社群简化为善恶数值。

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D14
{
    public sealed class D14ReligionCommunityCatalog
    {
        private readonly Dictionary<string, D14HistoricalContentDefinition> _contents = new Dictionary<string, D14HistoricalContentDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D14ReligionDefinition> _religions = new Dictionary<string, D14ReligionDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D14FestivalDefinition> _festivals = new Dictionary<string, D14FestivalDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D14DietaryRuleDefinition> _dietary = new Dictionary<string, D14DietaryRuleDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D14EtiquetteDefinition> _etiquette = new Dictionary<string, D14EtiquetteDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D14BuildingRiteDefinition> _buildings = new Dictionary<string, D14BuildingRiteDefinition>(StringComparer.Ordinal);

        public D14ReligionCommunityCatalog()
        {
            AddContent(new D14HistoricalContentDefinition { ContentId = "content_d14_festival_harbor_lantern", ContentType = D14ReligionContentType.Festival, DisplayNameKey = "content.d14.festival.harbor_lantern", HistoricalTag = "inferred", SensitiveLabel = "religious_community_practice", ContentWarning = "包含 1675 年加勒比港口宗教与社群节期语境；表现不代表现实价值判断。", SourceNote = "项目自有上下文样例，具体史实需以审查事实为准。" });
            AddContent(new D14HistoricalContentDefinition { ContentId = "content_d14_dietary_fast_day", ContentType = D14ReligionContentType.DietaryRule, DisplayNameKey = "content.d14.dietary.fast_day", HistoricalTag = "inferred", SensitiveLabel = "religious_dietary_practice", ContentWarning = "包含历史饮食规约语境；玩家可以选择低强度或跳过表现。", SourceNote = "项目自有饮食语境样例，不创建无来源历史细节。" });
            AddContent(new D14HistoricalContentDefinition { ContentId = "content_d14_building_chapel_opening", ContentType = D14ReligionContentType.BuildingRite, DisplayNameKey = "content.d14.building.chapel_opening", HistoricalTag = "inferred", SensitiveLabel = "religious_building_practice", ContentWarning = "包含宗教建筑开放与社群礼仪语境；建筑状态仍由 BuildingSystem 裁决。", SourceNote = "项目自有建筑仪式样例，开放结果必须引用建筑事实。" });
            AddContent(new D14HistoricalContentDefinition { ContentId = "content_d14_funeral_remembrance", ContentType = D14ReligionContentType.FuneralRite, DisplayNameKey = "content.d14.funeral.remembrance", HistoricalTag = "inferred", SensitiveLabel = "death_and_funeral", ContentWarning = "包含死亡、葬礼和历史战争后果语境；支持低强度与跳过。", SourceNote = "项目自有葬礼语境样例，死亡事实来自 Aftercare/Community authority。" });
            AddContent(new D14HistoricalContentDefinition { ContentId = "content_d14_oath_mutual_aid", ContentType = D14ReligionContentType.Oath, DisplayNameKey = "content.d14.oath.mutual_aid", HistoricalTag = "inferred", SensitiveLabel = "religious_social_oath", ContentWarning = "包含誓言、义务和违约后果语境；信任变化由关系/社群系统裁决。", SourceNote = "项目自有誓言表达样例，不以誓言文本直接写入声望或关系。" });
            AddContent(new D14HistoricalContentDefinition { ContentId = "content_d14_community_greeting", ContentType = D14ReligionContentType.Etiquette, DisplayNameKey = "content.d14.community.greeting", HistoricalTag = "fictional", SensitiveLabel = "community_contact_context", ContentWarning = "包含虚构社群接触礼仪；不代表现实社群的统一行为。", SourceNote = "项目自有社群礼仪样例，长期信任由 D12 CommunityTradeSystem 裁决。" });

            AddReligion(new D14ReligionDefinition { ReligionId = "religion_harbor_parish", DisplayNameKey = "religion.harbor.parish", CulturalContext = "加勒比港口中的天主教堂区语境（项目抽象）", FestivalIds = new List<string> { "festival_harbor_lantern" }, DietaryRuleIds = new List<string> { "dietary_fast_day" }, ContentIds = new List<string> { "content_d14_festival_harbor_lantern", "content_d14_dietary_fast_day", "content_d14_building_chapel_opening", "content_d14_funeral_remembrance", "content_d14_oath_mutual_aid" } });
            AddFestival(new D14FestivalDefinition { FestivalId = "festival_harbor_lantern", ReligionId = "religion_harbor_parish", ContentId = "content_d14_festival_harbor_lantern", StartDay = 4, DurationDays = 3, BuildingRiteId = "building_rite_chapel_opening", EtiquetteId = "etiquette_formal_greeting", DietaryRuleId = "dietary_fast_day" });
            AddDietary(new D14DietaryRuleDefinition { RuleId = "dietary_fast_day", ReligionId = "religion_harbor_parish", ContentId = "content_d14_dietary_fast_day", FoodTag = "plant_staple", ComplianceDelta = 10 });
            AddEtiquette(new D14EtiquetteDefinition { EtiquetteId = "etiquette_formal_greeting", CommunityId = "community_green_cay", ContentId = "content_d14_community_greeting", RequiredFestivalId = "festival_harbor_lantern", RequiredDietaryRuleId = "dietary_fast_day", EtiquetteTag = "formal_greeting_before_exchange" });
            AddEtiquette(new D14EtiquetteDefinition { EtiquetteId = "etiquette_mutual_aid", CommunityId = "community_free_cove", ContentId = "content_d14_community_greeting", EtiquetteTag = "mutual_aid_acknowledgement" });
            AddBuilding(new D14BuildingRiteDefinition { RiteId = "building_rite_chapel_opening", BuildingId = "building_green_cay_chapel", ReligionId = "religion_harbor_parish", ContentId = "content_d14_building_chapel_opening", OpenHour = 6, CloseHour = 18 });
            ValidateLinks();
        }

        public IReadOnlyList<D14HistoricalContentDefinition> Contents() { var result = new List<D14HistoricalContentDefinition>(); foreach (var value in _contents.Values) result.Add(value.Clone()); return result.AsReadOnly(); }
        public IReadOnlyList<D14ReligionDefinition> Religions() { var result = new List<D14ReligionDefinition>(); foreach (var value in _religions.Values) result.Add(value.Clone()); return result.AsReadOnly(); }
        public IReadOnlyList<D14FestivalDefinition> Festivals() { var result = new List<D14FestivalDefinition>(); foreach (var value in _festivals.Values) result.Add(value.Clone()); return result.AsReadOnly(); }
        public IReadOnlyList<D14DietaryRuleDefinition> DietaryRules() { var result = new List<D14DietaryRuleDefinition>(); foreach (var value in _dietary.Values) result.Add(value.Clone()); return result.AsReadOnly(); }
        public IReadOnlyList<D14EtiquetteDefinition> Etiquettes() { var result = new List<D14EtiquetteDefinition>(); foreach (var value in _etiquette.Values) result.Add(value.Clone()); return result.AsReadOnly(); }
        public IReadOnlyList<D14BuildingRiteDefinition> BuildingRites() { var result = new List<D14BuildingRiteDefinition>(); foreach (var value in _buildings.Values) result.Add(value.Clone()); return result.AsReadOnly(); }
        public D14HistoricalContentDefinition FindContent(string id) { D14HistoricalContentDefinition value; return string.IsNullOrWhiteSpace(id) || !_contents.TryGetValue(id, out value) ? null : value.Clone(); }
        public D14ReligionDefinition FindReligion(string id) { D14ReligionDefinition value; return string.IsNullOrWhiteSpace(id) || !_religions.TryGetValue(id, out value) ? null : value.Clone(); }
        public D14FestivalDefinition FindFestival(string id) { D14FestivalDefinition value; return string.IsNullOrWhiteSpace(id) || !_festivals.TryGetValue(id, out value) ? null : value.Clone(); }
        public D14DietaryRuleDefinition FindDietaryRule(string id) { D14DietaryRuleDefinition value; return string.IsNullOrWhiteSpace(id) || !_dietary.TryGetValue(id, out value) ? null : value.Clone(); }
        public D14EtiquetteDefinition FindEtiquette(string id) { D14EtiquetteDefinition value; return string.IsNullOrWhiteSpace(id) || !_etiquette.TryGetValue(id, out value) ? null : value.Clone(); }
        public D14BuildingRiteDefinition FindBuildingRite(string id) { D14BuildingRiteDefinition value; return string.IsNullOrWhiteSpace(id) || !_buildings.TryGetValue(id, out value) ? null : value.Clone(); }

        private void AddContent(D14HistoricalContentDefinition value) { value.Validate("d14.content"); Add(_contents, value.ContentId, value); }
        private void AddReligion(D14ReligionDefinition value) { value.Validate("d14.religion"); Add(_religions, value.ReligionId, value); }
        private void AddFestival(D14FestivalDefinition value) { value.Validate("d14.festival"); Add(_festivals, value.FestivalId, value); }
        private void AddDietary(D14DietaryRuleDefinition value) { value.Validate("d14.dietary"); Add(_dietary, value.RuleId, value); }
        private void AddEtiquette(D14EtiquetteDefinition value) { value.Validate("d14.etiquette"); Add(_etiquette, value.EtiquetteId, value); }
        private void AddBuilding(D14BuildingRiteDefinition value) { value.Validate("d14.building"); Add(_buildings, value.RiteId, value); }

        private void ValidateLinks()
        {
            foreach (var religion in _religions.Values)
            {
                foreach (var id in religion.FestivalIds) if (!_festivals.ContainsKey(id)) throw new InvalidOperationException("宗教引用未知节期：" + id);
                foreach (var id in religion.DietaryRuleIds) if (!_dietary.ContainsKey(id)) throw new InvalidOperationException("宗教引用未知饮食规则：" + id);
                foreach (var id in religion.ContentIds) if (!_contents.ContainsKey(id)) throw new InvalidOperationException("宗教引用未知内容：" + id);
            }
            foreach (var festival in _festivals.Values)
            {
                if (!_religions.ContainsKey(festival.ReligionId) || !_contents.ContainsKey(festival.ContentId) || !_buildings.ContainsKey(festival.BuildingRiteId) || !_etiquette.ContainsKey(festival.EtiquetteId) || !_dietary.ContainsKey(festival.DietaryRuleId)) throw new InvalidOperationException("节期引用不完整：" + festival.FestivalId);
            }
            foreach (var rule in _dietary.Values) if (!_religions.ContainsKey(rule.ReligionId) || !_contents.ContainsKey(rule.ContentId)) throw new InvalidOperationException("饮食规则引用不完整：" + rule.RuleId);
            foreach (var etiquette in _etiquette.Values) if (!_contents.ContainsKey(etiquette.ContentId)) throw new InvalidOperationException("礼仪引用未知内容：" + etiquette.EtiquetteId);
            foreach (var building in _buildings.Values) if (!_religions.ContainsKey(building.ReligionId) || !_contents.ContainsKey(building.ContentId)) throw new InvalidOperationException("建筑仪式引用不完整：" + building.RiteId);
        }

        private static void Add<T>(Dictionary<string, T> map, string id, T value)
        {
            if (map.ContainsKey(id)) throw new InvalidOperationException("D14-A06 目录重复：" + id);
            map.Add(id, value);
        }

    }
}
