using System;
using System.Collections.Generic;
using UnityEngine;
using Whalebone.Systems.D5;
using Whalebone.Systems.D16;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D16
{
    /// <summary>
    /// D5 岛屿目录在 D16 表现层的只读流式索引。
    ///
    /// 这里维护的是稳定 ID、海图坐标和表现预算等级，不创建岛屿实体，
    /// 不写 IslandSystem/SaveSystem，也不把海图 marker 当作 3D 场景资源。
    /// 没有真实视觉根的条目明确保持 data-only，供性能报告和后续资产接入使用。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class D16IslandDataStreamRegistry : MonoBehaviour
    {
        [Serializable]
        public sealed class Cell
        {
            public string IslandId { get; private set; }
            public string NameZh { get; private set; }
            public string NameEn { get; private set; }
            public Vector2 ChartPosition { get; private set; }
            public bool IsMetadataActive { get; internal set; }
            public D16SimulationTier SimulationTier { get; internal set; }
            public Transform VisualRoot { get; internal set; }
            public bool HasVisualPresentation => VisualRoot != null;

            internal void Configure(D5IslandDefinition island)
            {
                IslandId = island == null ? string.Empty : island.Id;
                NameZh = island == null ? string.Empty : island.NameZh;
                NameEn = island == null ? string.Empty : island.NameEn;
                ChartPosition = island == null ? Vector2.zero : new Vector2(island.MapX, island.MapY);
                IsMetadataActive = false;
                SimulationTier = D16SimulationTier.Frozen;
                VisualRoot = null;
            }
        }

        private readonly List<Cell> _cells = new List<Cell>();
        private string _referenceIslandId;
        private bool _configured;

        public IReadOnlyList<Cell> Cells => _cells;
        public string ReferenceIslandId => _referenceIslandId;
        public int CatalogCellCount => _cells.Count;
        public int ActiveCellCount { get; private set; }
        public int DataOnlyCellCount { get; private set; }
        public int VisualPresentationCellCount { get; private set; }

        /// <summary>
        /// 用当前 D5 存档快照作为海图参考点建立目录索引。
        /// 只读取快照；没有快照时使用 D5 的稳定初始岛屿 ID。
        /// </summary>
        public void ConfigureRuntime(string referenceIslandId = null)
        {
            _referenceIslandId = ResolveReferenceIslandId(referenceIslandId);
            _cells.Clear();
            foreach (var island in D5IslandCatalog.All)
            {
                if (island == null || string.IsNullOrWhiteSpace(island.Id)) continue;
                var cell = new Cell();
                cell.Configure(island);
                _cells.Add(cell);
            }
            _configured = true;
            RefreshFromChartReference();
        }

        /// <summary>
        /// 保存/读档或 D5 运行时更新后重新读取当前岛屿，不写回任何状态。
        /// </summary>
        public void RefreshFromCurrentSnapshot()
        {
            string resolved = ResolveReferenceIslandId(null);
            if (!_configured || !string.Equals(resolved, _referenceIslandId, StringComparison.Ordinal))
            {
                ConfigureRuntime(resolved);
                return;
            }
            RefreshFromChartReference();
        }

        /// <summary>
        /// 供真实授权的最终岛屿表现接入。传入 null 不会把条目标记为已集成。
        /// </summary>
        public bool RegisterVisualPresentation(string islandId, Transform visualRoot)
        {
            if (string.IsNullOrWhiteSpace(islandId) || visualRoot == null) return false;
            var cell = FindCell(islandId);
            if (cell == null) return false;
            cell.VisualRoot = visualRoot;
            RecountPresentationState();
            return true;
        }

        public Cell FindCell(string islandId)
        {
            if (string.IsNullOrWhiteSpace(islandId)) return null;
            foreach (var cell in _cells)
                if (cell != null && string.Equals(cell.IslandId, islandId, StringComparison.Ordinal))
                    return cell;
            return null;
        }

        private void RefreshFromChartReference()
        {
            var reference = D5IslandCatalog.Find(_referenceIslandId);
            Vector2 referencePosition = reference == null
                ? Vector2.zero
                : new Vector2(reference.MapX, reference.MapY);

            ActiveCellCount = 0;
            for (int i = 0; i < _cells.Count; i++)
            {
                var cell = _cells[i];
                if (cell == null) continue;
                float distance = Vector2.Distance(referencePosition, cell.ChartPosition);
                cell.SimulationTier = distance <= 0.35f ? D16SimulationTier.Full :
                    distance <= 0.75f ? D16SimulationTier.Reduced :
                    distance <= 1.25f ? D16SimulationTier.Background : D16SimulationTier.Frozen;
                cell.IsMetadataActive = cell.SimulationTier != D16SimulationTier.Frozen;
                if (cell.IsMetadataActive) ActiveCellCount++;
            }
            RecountPresentationState();
        }

        private void RecountPresentationState()
        {
            DataOnlyCellCount = 0;
            VisualPresentationCellCount = 0;
            foreach (var cell in _cells)
            {
                if (cell == null) continue;
                if (cell.HasVisualPresentation) VisualPresentationCellCount++;
                else DataOnlyCellCount++;
            }
        }

        private static string ResolveReferenceIslandId(string requested)
        {
            if (!string.IsNullOrWhiteSpace(requested) && D5IslandCatalog.Find(requested) != null)
                return requested;

            var snapshot = GameState.CurrentD5Runtime;
            string savedIsland = snapshot == null || snapshot.World == null
                ? null
                : snapshot.World.CurrentIslandId;
            if (!string.IsNullOrWhiteSpace(savedIsland) && D5IslandCatalog.Find(savedIsland) != null)
                return savedIsland;
            return "santa_margarita";
        }
    }
}
