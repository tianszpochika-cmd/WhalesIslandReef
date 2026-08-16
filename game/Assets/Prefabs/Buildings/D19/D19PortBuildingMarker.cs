using UnityEngine;

namespace WhalesIslandReef.D19
{
    /// <summary>港口建筑表现元数据；不承载价格、库存、船坞或交易公式。</summary>
    [DisallowMultipleComponent]
    public sealed class D19PortBuildingMarker : MonoBehaviour
    {
        [SerializeField] private string _buildingId;
        [SerializeField] private string _category;
        [SerializeField] private string _periodNote = "1675 Caribbean port visual candidate";
        [SerializeField] private string _materialGroup;
        [SerializeField] private string _lodStrategy = "LOD0 structural detail / LOD1 silhouette block";
        [SerializeField] private string _testId;

        public string BuildingId => _buildingId;
        public string Category => _category;
        public string PeriodNote => _periodNote;
        public string MaterialGroup => _materialGroup;
        public string LodStrategy => _lodStrategy;
        public string TestId => _testId;

        public void Configure(string buildingId, string category, string materialGroup, string testId)
        {
            _buildingId = buildingId ?? string.Empty;
            _category = category ?? string.Empty;
            _materialGroup = materialGroup ?? string.Empty;
            _testId = testId ?? string.Empty;
        }
    }
}
