// ============================================================
// VoyageEventSystem —— 确定性航程事件裁决
//
// 事件的触发与数值后果全部在这里完成；Agent 只能在上层生成文案，不能修改结果。
// ============================================================

using System;

namespace Whalebone.Systems.D3
{
    public sealed class VoyageEventSystem
    {
        public VoyageEventDefinition Roll(VoyageState state)
        {
            if (state == null) return null;
            int index = StableIndex(state.Seed, state.CurrentDay, state.OriginId, state.DestinationId,
                VoyageEventCatalog.All.Count);
            return VoyageEventCatalog.All[index];
        }

        public VoyageEventDefinition Apply(VoyageState state, VoyageEventDefinition definition)
        {
            if (state == null || definition == null) return definition;
            state.Water = Math.Max(0, state.Water + definition.WaterDelta);
            state.Food = Math.Max(0, state.Food + definition.FoodDelta);
            int maxHull = state.HullMax <= 0 ? int.MaxValue : state.HullMax;
            state.Hull = Math.Max(0, Math.Min(maxHull, state.Hull + definition.HullDelta));
            state.MoneyDelta += definition.MoneyDelta;
            state.EventIds.Add(definition.Id);
            return definition;
        }

        private static int StableIndex(int seed, int day, string origin, string destination, int count)
        {
            unchecked
            {
                uint hash = (uint)seed;
                hash = hash * 16777619u ^ (uint)(day + 1);
                hash = hash * 16777619u ^ StableStringHash(origin);
                hash = hash * 16777619u ^ StableStringHash(destination);
                return (int)(hash % (uint)Math.Max(1, count));
            }
        }

        private static uint StableStringHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                if (value == null) return hash;
                foreach (char c in value) hash = (hash ^ c) * 16777619u;
                return hash;
            }
        }
    }
}
