using System;

namespace Whalebone.Systems.D5
{
    public sealed class D5WorldActionResult
    {
        public bool Accepted;
        public string IslandId;
        public string Message;
    }

    /// <summary>12 岛世界、发现和访问状态的唯一裁决者。</summary>
    public sealed class D5WorldSystem
    {
        private readonly D5WorldState _state;

        public D5WorldSystem(D5WorldState initial = null)
        {
            _state = initial == null ? new D5WorldState() : initial.Clone();
            EnsureKnownDefaults();
        }

        public D5WorldState Snapshot => _state.Clone();

        public bool IsDiscovered(string islandId)
        {
            return !string.IsNullOrEmpty(islandId) && _state.DiscoveredIslands.Contains(islandId);
        }

        public bool HasVisited(string islandId)
        {
            return !string.IsNullOrEmpty(islandId) && _state.VisitedIslands.Contains(islandId);
        }

        public D5WorldActionResult DiscoverIsland(string islandId)
        {
            var island = D5IslandCatalog.Find(islandId);
            if (island == null) return new D5WorldActionResult { Message = "海图上没有这个岛屿。" };
            if (IsDiscovered(islandId))
                return new D5WorldActionResult { Accepted = true, IslandId = islandId, Message = island.NameZh + " 已在海图上。" };
            _state.DiscoveredIslands.Add(islandId);
            _state.MapRevision++;
            return new D5WorldActionResult { Accepted = true, IslandId = islandId, Message = "发现新岛屿：" + island.NameZh + "。" };
        }

        public D5WorldActionResult DiscoverNext()
        {
            foreach (var island in D5IslandCatalog.All)
            {
                if (!IsDiscovered(island.Id)) return DiscoverIsland(island.Id);
            }
            return new D5WorldActionResult { Accepted = true, Message = "全部 12 个岛屿都已绘入海图。" };
        }

        public D5WorldActionResult VisitIsland(string islandId)
        {
            var island = D5IslandCatalog.Find(islandId);
            if (island == null) return new D5WorldActionResult { Message = "无法驶向未知岛屿。" };
            if (!IsDiscovered(islandId)) return new D5WorldActionResult { Message = "先发现该岛屿，才能规划航线。" };
            _state.CurrentIslandId = islandId;
            if (!HasVisited(islandId)) _state.VisitedIslands.Add(islandId);
            _state.MapRevision++;
            return new D5WorldActionResult { Accepted = true, IslandId = islandId, Message = "已抵达 " + island.NameZh + "。" };
        }

        public void DiscoverAll()
        {
            foreach (var island in D5IslandCatalog.All) DiscoverIsland(island.Id);
        }

        private void EnsureKnownDefaults()
        {
            foreach (var island in D5IslandCatalog.All)
            {
                if (island.Id == "santa_margarita" || island.Id == "green_cay" || island.Id == "driftwood_isle")
                    if (!IsDiscovered(island.Id)) _state.DiscoveredIslands.Add(island.Id);
            }
            if (D5IslandCatalog.Find(_state.CurrentIslandId) == null) _state.CurrentIslandId = "santa_margarita";
        }
    }
}
