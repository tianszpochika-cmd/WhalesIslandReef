using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D4
{
    public static class D4FactionCatalog
    {
        public static readonly IReadOnlyList<FactionDefinition> All = new List<FactionDefinition>
        {
            new FactionDefinition { Id = "england", Name = "英格兰", Culture = "新教/海军贸易", BaseTariff = 8, RivalIds = new[] { "spain" }, HomePorts = new[] { "green_cay" } },
            new FactionDefinition { Id = "spain", Name = "西班牙", Culture = "天主教/殖民秩序", BaseTariff = 12, RivalIds = new[] { "england", "pirates" }, HomePorts = new[] { "santa_margarita" } },
            new FactionDefinition { Id = "france", Name = "法兰西", Culture = "天主教/商贸扩张", BaseTariff = 10, RivalIds = new[] { "england" }, HomePorts = new[] { "driftwood_isle" } },
            new FactionDefinition { Id = "portugal", Name = "葡萄牙", Culture = "天主教/航海传统", BaseTariff = 9, RivalIds = new[] { "spain" }, HomePorts = new[] { "green_cay" } },
            new FactionDefinition { Id = "pirates", Name = "海盗兄弟会", Culture = "自由港/分赃契约", BaseTariff = 0, RivalIds = new[] { "spain" }, HomePorts = new[] { "driftwood_isle" } },
            new FactionDefinition { Id = "tribes", Name = "加勒比原住民", Culture = "部落信仰/互惠", BaseTariff = 4, RivalIds = new string[0], HomePorts = new[] { "green_cay" } },
            new FactionDefinition { Id = "smugglers", Name = "走私网络", Culture = "黑市/风险交易", BaseTariff = 2, RivalIds = new string[0], HomePorts = new[] { "driftwood_isle" } },
        };

        public static FactionDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var definition in All) if (definition.Id == id) return definition;
            return null;
        }

        public static FactionState CreateDefault()
        {
            var result = new FactionState();
            foreach (var definition in All) result.Reputation[definition.Id] = definition.Id == "tribes" ? 5 : 0;
            return result;
        }
    }

    public static class D4ShipCatalog
    {
        public static readonly IReadOnlyList<ShipDefinition> All = new List<ShipDefinition>
        {
            new ShipDefinition { Id = "small_fishing", Name = "小渔船", Speed = 4.0f, Turn = 7.0f, CargoMax = 40, CrewMax = 4, CannonMax = 2, HullMax = 60, Price = 0, UnlockReputation = -100 },
            new ShipDefinition { Id = "sloop", Name = "单桅纵帆船", Speed = 5.5f, Turn = 6.5f, CargoMax = 55, CrewMax = 6, CannonMax = 6, HullMax = 85, Price = 850, UnlockReputation = 10 },
            new ShipDefinition { Id = "brigantine", Name = "双桅纵帆船", Speed = 6.0f, Turn = 5.5f, CargoMax = 75, CrewMax = 10, CannonMax = 10, HullMax = 110, Price = 1600, UnlockReputation = 25 },
            new ShipDefinition { Id = "merchant_brig", Name = "商用双桅船", Speed = 4.8f, Turn = 4.5f, CargoMax = 120, CrewMax = 14, CannonMax = 8, HullMax = 135, Price = 2400, UnlockReputation = 35 },
            new ShipDefinition { Id = "light_galleon", Name = "轻型盖伦船", Speed = 4.4f, Turn = 3.8f, CargoMax = 105, CrewMax = 18, CannonMax = 18, HullMax = 180, Price = 4200, UnlockReputation = 50 },
        };

        public static ShipDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var definition in All) if (definition.Id == id) return definition;
            return null;
        }
    }

    public static class D4AnimalCatalog
    {
        public static readonly IReadOnlyList<AnimalDefinition> All = new List<AnimalDefinition>
        {
            new AnimalDefinition { Id = "whale", Name = "鲸群", Habitat = "open_sea", Danger = 0, CollectionValue = 0, Marine = true },
            new AnimalDefinition { Id = "dolphin", Name = "海豚", Habitat = "open_sea", Danger = 0, CollectionValue = 5, Marine = true },
            new AnimalDefinition { Id = "shark", Name = "鲨鱼", Habitat = "open_sea", Danger = 55, CollectionValue = 15, Marine = true },
            new AnimalDefinition { Id = "sea_turtle", Name = "海龟", Habitat = "beach", Danger = 0, CollectionValue = 30, Marine = true, Rare = true },
            new AnimalDefinition { Id = "stingray", Name = "蝠鲼", Habitat = "reef", Danger = 5, CollectionValue = 10, Marine = true },
            new AnimalDefinition { Id = "fish_school", Name = "鱼群", Habitat = "reef", Danger = 0, CollectionValue = 12, Marine = true },
            new AnimalDefinition { Id = "iguana", Name = "鬣蜥", Habitat = "jungle", Danger = 0, CollectionValue = 18 },
            new AnimalDefinition { Id = "parrot", Name = "鹦鹉", Habitat = "jungle", Danger = 0, CollectionValue = 22 },
            new AnimalDefinition { Id = "hermit_crab", Name = "寄居蟹", Habitat = "beach", Danger = 0, CollectionValue = 8 },
            new AnimalDefinition { Id = "crab", Name = "蟹", Habitat = "mangrove", Danger = 0, CollectionValue = 7 },
            new AnimalDefinition { Id = "venomous_snake", Name = "毒蛇", Habitat = "jungle", Danger = 70, CollectionValue = 20 },
            new AnimalDefinition { Id = "mosquito_swarm", Name = "蚊虫群", Habitat = "mangrove", Danger = 45, CollectionValue = 0 },
            new AnimalDefinition { Id = "crocodile", Name = "鳄鱼", Habitat = "mangrove", Danger = 80, CollectionValue = 35 },
            new AnimalDefinition { Id = "jaguar", Name = "美洲豹", Habitat = "jungle", Danger = 85, CollectionValue = 50 },
            new AnimalDefinition { Id = "golden_parrot", Name = "黄金鹦鹉", Habitat = "jungle", Danger = 0, CollectionValue = 120, Rare = true },
        };

        public static AnimalDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var definition in All) if (definition.Id == id) return definition;
            return null;
        }
    }

    public static class D4DiseaseCatalog
    {
        public static readonly IReadOnlyList<DiseaseDefinition> All = new List<DiseaseDefinition>
        {
            new DiseaseDefinition { Id = "scurvy", Name = "坏血病", DailyRisk = 10, DailyDamage = 4, TreatmentCost = 1, TreatmentPower = 28, Source = "缺乏新鲜蔬果" },
            new DiseaseDefinition { Id = "malaria", Name = "疟疾", DailyRisk = 8, DailyDamage = 7, TreatmentCost = 2, TreatmentPower = 24, Source = "蚊虫区滞留" },
            new DiseaseDefinition { Id = "yellow_fever", Name = "黄热病", DailyRisk = 4, DailyDamage = 12, TreatmentCost = 3, TreatmentPower = 20, Source = "蚊虫/湿热港湾" },
            new DiseaseDefinition { Id = "dysentery", Name = "痢疾", DailyRisk = 7, DailyDamage = 6, TreatmentCost = 1, TreatmentPower = 25, Source = "污染饮水" },
            new DiseaseDefinition { Id = "consumption", Name = "肺痨", DailyRisk = 2, DailyDamage = 3, TreatmentCost = 3, TreatmentPower = 12, Source = "长期虚弱" },
        };

        public static DiseaseDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var definition in All) if (definition.Id == id) return definition;
            return null;
        }
    }
}
