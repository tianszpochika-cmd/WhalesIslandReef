using UnityEngine;
using UnityEngine.AI;

namespace WhalesIslandReef.D19A01
{
    /// <summary>
    /// D19-A01 工作场景专属 NavMeshData 挂载器。
    /// 不修改 Navigation/Ship 强逻辑；只把本任务生成的 NavMeshData 加入当前工作场景。
    /// </summary>
    public sealed class D19A01TerrainNavMeshRuntime : MonoBehaviour
    {
        [SerializeField] private NavMeshData navMeshData;
        [SerializeField] private string agentType = "HumanoidDefault";
        private NavMeshDataInstance _instance;

        public NavMeshData NavMeshData
        {
            get { return navMeshData; }
            set { navMeshData = value; }
        }

        public string AgentType
        {
            get { return agentType; }
            set { agentType = value; }
        }

        public bool HasData { get { return navMeshData != null; } }

        private void OnEnable()
        {
            if (navMeshData != null)
            {
                _instance = NavMesh.AddNavMeshData(navMeshData, transform.position, transform.rotation);
                Debug.Log("[D19-A01] NavMesh data registered: valid=" + _instance.valid + ", agentType=" + agentType);
            }
            else
            {
                Debug.LogError("[D19-A01] NavMesh data reference is missing from D19_TerrainNavMesh prefab.");
            }
        }

        private void OnDisable()
        {
            if (_instance.valid)
                _instance.Remove();
        }
    }
}
