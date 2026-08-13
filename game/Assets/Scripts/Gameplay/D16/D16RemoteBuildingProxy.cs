using System.Collections.Generic;
using UnityEngine;
using Whalebone.Systems.D16;

namespace Whalebone.Gameplay.D16
{
    /// <summary>
    /// 远景建筑的表现代理切换器。
    ///
    /// FullPresentationRoot 和 ProxyPresentationRoot 只能放表现对象；
    /// 领域实体、碰撞体、事件和存档对象必须放在本组件之外。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class D16RemoteBuildingProxy : MonoBehaviour
    {
        [SerializeField] private string _stableId = "building.remote_proxy";
        [SerializeField] private Transform _fullPresentationRoot = null;
        [SerializeField] private Transform _proxyPresentationRoot = null;
        [SerializeField] private bool _playerRelevant;
        [SerializeField] private bool _activeInteraction;

        private Renderer[] _fullRenderers;
        private Renderer[] _proxyRenderers;

        public string StableId => string.IsNullOrWhiteSpace(_stableId)
            ? BuildHierarchyId()
            : _stableId.Trim();
        public Transform FullPresentationRoot => _fullPresentationRoot;
        public Transform ProxyPresentationRoot => _proxyPresentationRoot;

        /// <summary>
        /// 为运行时生成的建筑表现绑定稳定 ID 和两个表现变体。
        /// 该方法不接管建筑领域状态或碰撞体。
        /// </summary>
        public void ConfigureRuntime(string stableId, Transform fullPresentationRoot,
            Transform proxyPresentationRoot, bool playerRelevant = false,
            bool activeInteraction = false)
        {
            _stableId = stableId;
            _fullPresentationRoot = fullPresentationRoot;
            _proxyPresentationRoot = proxyPresentationRoot;
            _playerRelevant = playerRelevant;
            _activeInteraction = activeInteraction;
            EnsureCaches();
        }

        public bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(_stableId))
            {
                error = "remote_building_proxy_id_missing";
                return false;
            }
            if (_fullPresentationRoot == null || _proxyPresentationRoot == null)
            {
                error = "remote_building_proxy_root_missing";
                return false;
            }
            if (_fullPresentationRoot == _proxyPresentationRoot ||
                _fullPresentationRoot == transform || _proxyPresentationRoot == transform)
            {
                error = "remote_building_proxy_roots_must_be_distinct_presentation_children";
                return false;
            }
            if (transform.IsChildOf(_fullPresentationRoot) || transform.IsChildOf(_proxyPresentationRoot))
            {
                error = "remote_building_proxy_controller_must_not_be_under_presentation_root";
                return false;
            }
            error = string.Empty;
            return true;
        }

        public D16PerformanceCandidate BuildCandidate(Transform focus)
        {
            EnsureCaches();
            float distance = focus == null ? 0f : Vector3.Distance(transform.position, focus.position);
            return new D16PerformanceCandidate
            {
                Id = StableId,
                Distance = distance,
                IsVisible = IsVisibleFromMainCamera(),
                IsPlayerRelevant = _playerRelevant,
                HasActiveInteraction = _activeInteraction,
                IsAgent = false,
                IsAnimal = false,
                UsesAudio = false,
            };
        }

        /// <summary>
        /// Full/Reduced 使用建筑本体，Background 使用远景代理，Frozen 两者都不显示。
        /// 这里只切换表现根，不切换根外的逻辑碰撞体。
        /// </summary>
        public void Apply(D16PerformanceDecision decision)
        {
            if (decision == null || !Validate(out _)) return;
            bool fullVisible = decision.SimulationTier == D16SimulationTier.Full ||
                decision.SimulationTier == D16SimulationTier.Reduced;
            bool proxyVisible = decision.SimulationTier == D16SimulationTier.Background;
            SetRootActive(_fullPresentationRoot, fullVisible);
            SetRootActive(_proxyPresentationRoot, proxyVisible);
        }

        public void ResetPresentationState()
        {
            SetRootActive(_fullPresentationRoot, true);
            SetRootActive(_proxyPresentationRoot, false);
        }

        private void EnsureCaches()
        {
            if (_fullRenderers != null && _proxyRenderers != null) return;
            _fullRenderers = _fullPresentationRoot == null
                ? new Renderer[0]
                : _fullPresentationRoot.GetComponentsInChildren<Renderer>(true);
            _proxyRenderers = _proxyPresentationRoot == null
                ? new Renderer[0]
                : _proxyPresentationRoot.GetComponentsInChildren<Renderer>(true);
        }

        private bool IsVisibleFromMainCamera()
        {
            var camera = Camera.main;
            if (camera == null) return true;
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            foreach (var renderer in _fullRenderers)
                if (renderer != null && GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) return true;
            foreach (var renderer in _proxyRenderers)
                if (renderer != null && GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) return true;
            return GeometryUtility.TestPlanesAABB(planes, new Bounds(transform.position, Vector3.one));
        }

        private static void SetRootActive(Transform root, bool active)
        {
            if (root != null && root.gameObject.activeSelf != active)
                root.gameObject.SetActive(active);
        }

        private string BuildHierarchyId()
        {
            var parts = new List<string>();
            var current = transform;
            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }
            parts.Reverse();
            return "scene/" + string.Join("/", parts);
        }
    }
}
