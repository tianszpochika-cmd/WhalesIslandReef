using UnityEngine;

namespace WhalesIslandReef.D19
{
    /// <summary>港口工作场景使用的可视化交互锚点；不写入任何玩法状态。</summary>
    [DisallowMultipleComponent]
    public sealed class D19PortAnchor : MonoBehaviour
    {
        [SerializeField] private string _anchorId;
        [SerializeField] private string _stableId;
        [SerializeField] private string _ownerTask = "D19-A02";
        [SerializeField] private string _logicOwner;
        [SerializeField] private string _migrationPolicy;

        public string AnchorId => _anchorId;
        public string StableId => _stableId;
        public string OwnerTask => _ownerTask;
        public string LogicOwner => _logicOwner;
        public string MigrationPolicy => _migrationPolicy;

        public void Configure(string anchorId, string stableId, string ownerTask, string logicOwner, string migrationPolicy)
        {
            _anchorId = anchorId ?? string.Empty;
            _stableId = stableId ?? string.Empty;
            _ownerTask = string.IsNullOrWhiteSpace(ownerTask) ? "D19-A02" : ownerTask;
            _logicOwner = logicOwner ?? string.Empty;
            _migrationPolicy = migrationPolicy ?? string.Empty;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = _ownerTask == "D19-A02" ? new Color(1f, 0.7f, 0.1f, 0.9f) : new Color(0.2f, 0.8f, 1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.35f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.8f);
        }
#endif
    }
}
