// ============================================================
// WorldCatalog —— D3 的 3 个地点、5 种天气和 8 个事件
// ============================================================

using System.Collections.Generic;

namespace Whalebone.Systems.D3
{
    public static class PortCatalog
    {
        public const string StartingPortId = "santa_margarita";
        public const string GreenCayId = "green_cay";
        public const string DriftwoodIsleId = "driftwood_isle";

        public static readonly IReadOnlyList<PortDefinition> All = BuildAll();
        private static readonly Dictionary<string, PortDefinition> ById = BuildIndex();

        public static PortDefinition Find(string id)
        {
            if (id == null) return null;
            ById.TryGetValue(id, out PortDefinition port);
            return port;
        }

        public static int CalculateTravelDays(string originId, string destinationId, float shipSpeed)
        {
            var origin = Find(originId);
            var destination = Find(destinationId);
            if (origin == null || destination == null || origin.Id == destination.Id) return 0;

            float dx = destination.WorldX - origin.WorldX;
            float dz = destination.WorldZ - origin.WorldZ;
            float distance = (float)System.Math.Sqrt(dx * dx + dz * dz);
            float speed = float.IsNaN(shipSpeed) || float.IsInfinity(shipSpeed) || shipSpeed <= 0f
                ? 1f : shipSpeed;
            return System.Math.Max(1, (int)System.Math.Ceiling(distance / (speed * 2.5f)));
        }

        private static List<PortDefinition> BuildAll()
        {
            var town = new PortDefinition(StartingPortId, "圣玛格丽特港", D3LocationType.Town, 0f, 0f, 0.08f);
            town.SpecialtyCoefficients["tobacco"] = 0.86f;
            town.SpecialtyCoefficients["rum"] = 0.9f;

            var village = new PortDefinition(GreenCayId, "绿湾村", D3LocationType.Village, 14f, 8f, 0.04f);
            village.SpecialtyCoefficients["sugar"] = 0.82f;
            village.SpecialtyCoefficients["cocoa"] = 0.86f;
            village.GatheringLimits["sugar"] = 8;
            village.GatheringLimits["cocoa"] = 5;
            village.GatheringLimits["medicine"] = 3;

            var island = new PortDefinition(DriftwoodIsleId, "漂木荒岛", D3LocationType.DesertedIsland, -16f, 13f, 0f);
            island.SpecialtyCoefficients["timber"] = 0.78f;
            island.SpecialtyCoefficients["dyes"] = 0.88f;
            island.SpecialtyCoefficients["spices"] = 0.9f;
            island.GatheringLimits["timber"] = 12;
            island.GatheringLimits["dyes"] = 5;
            island.GatheringLimits["spices"] = 4;
            island.GatheringLimits["medicine"] = 2;

            return new List<PortDefinition> { town, village, island };
        }

        private static Dictionary<string, PortDefinition> BuildIndex()
        {
            var result = new Dictionary<string, PortDefinition>();
            foreach (var port in All) result[port.Id] = port;
            return result;
        }
    }

    public static class WeatherCatalog
    {
        public static readonly IReadOnlyList<WeatherDefinition> All = new List<WeatherDefinition>
        {
            new WeatherDefinition(D3WeatherType.Calm, "晴朗", 0.96f),
            new WeatherDefinition(D3WeatherType.TradeWind, "信风", 0.92f),
            new WeatherDefinition(D3WeatherType.Rain, "降雨", 1.02f),
            new WeatherDefinition(D3WeatherType.Storm, "风暴", 1.18f),
            new WeatherDefinition(D3WeatherType.Hurricane, "飓风", 1.35f),
        };

        public static WeatherDefinition Find(D3WeatherType type)
        {
            foreach (var weather in All)
                if (weather.Type == type) return weather;
            return All[0];
        }
    }

    public static class VoyageEventCatalog
    {
        public static readonly IReadOnlyList<VoyageEventDefinition> All = new List<VoyageEventDefinition>
        {
            new VoyageEventDefinition("calm_sea", "平静海面", "海面平缓，船员得以休整。"),
            new VoyageEventDefinition("favorable_wind", "顺风", "信风推着船帆前进，航程没有额外损耗。"),
            new VoyageEventDefinition("rain_catch", "雨水收集", "船员收集雨水，补给得到小幅补充。", 2, 0),
            new VoyageEventDefinition("sail_tear", "帆布撕裂", "一阵强风撕裂了帆布，船体受到轻微牵连。", 0, 0, -3),
            new VoyageEventDefinition("reef_strike", "暗礁擦碰", "瞭望发现暗礁太迟，船底受损。", 0, 0, -8),
            new VoyageEventDefinition("crew_sickness", "船员染病", "潮湿的船舱让一名船员病倒，食物消耗增加。", 0, -2),
            new VoyageEventDefinition("floating_cargo", "漂流货箱", "海面发现一只尚可使用的货箱，卖掉后得到少量金币。", 0, 0, 0, 8),
            new VoyageEventDefinition("naval_inspection", "海上盘查", "巡逻船要求缴纳临时通行费。", 0, 0, 0, -12),
        };

        public static VoyageEventDefinition Find(string id)
        {
            foreach (var item in All)
                if (item.Id == id) return item;
            return null;
        }
    }
}
