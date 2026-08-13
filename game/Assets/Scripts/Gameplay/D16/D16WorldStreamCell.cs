using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whalebone.Gameplay.D16
{
    /// <summary>
    /// 一个只包含表现对象的世界流式单元。
    /// 不允许把领域状态、事件或存档唯一对象放入 PresentationRoot。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class D16WorldStreamCell : MonoBehaviour
    {
        [SerializeField] private string _cellId = "d16_cell";
        [SerializeField] private Transform _presentationRoot = null;
        [SerializeField] private float _loadRadius = 140f;
        [SerializeField] private float _unloadRadius = 220f;
        [SerializeField] private bool _startLoaded = true;
        [SerializeField, Min(1)] private int _activationBatchSize = 24;
        [SerializeField] private D16AudioResourceScope _audioScope = null;

        private readonly List<GameObject> _stagedChildren = new List<GameObject>();
        private readonly Dictionary<int, bool> _initialActiveStateByInstanceId =
            new Dictionary<int, bool>();
        private HashSet<string> _audioBaselinePaths;
        private string _audioOwnerId;
        private Coroutine _loadingCoroutine;
        private float _loadStartedAt;

        public string CellId => _cellId;
        public Transform PresentationRoot => _presentationRoot;
        public float LoadRadius => _loadRadius;
        public float UnloadRadius => _unloadRadius;
        public bool IsLoaded { get; private set; }
        public bool IsLoading { get; private set; }
        public float LoadingProgress { get; private set; }
        public int LoadedPresentationObjectCount { get; private set; }
        public int PendingPresentationObjectCount =>
            Mathf.Max(0, _stagedChildren.Count - LoadedPresentationObjectCount);
        public float LastLoadDurationMs { get; private set; }
        public int LoadCount { get; private set; }
        public D16AudioResourceScope AudioScope => _audioScope;

        /// <summary>
        /// 将表现音频登记到当前流式单元的 owner。相同 scope/路径被其他单元使用时，
        /// 卸载当前单元不会释放其他 owner 的引用。
        /// </summary>
        public bool TrackAudioResource(string resourcePath, AudioClip clip)
        {
            if (_audioScope == null) return false;
            EnsureAudioOwner();
            return _audioScope.TrackLoadedClipForOwner(_audioOwnerId, resourcePath, clip);
        }

        /// <summary>
        /// 为场景引导层或测试场景配置表现流式单元。
        /// 该入口只保存表现层引用，不创建、删除或修改任何领域实体。
        /// </summary>
        public void ConfigureRuntime(string cellId, Transform presentationRoot, float loadRadius,
            float unloadRadius, bool startLoaded, D16AudioResourceScope audioScope = null,
            int activationBatchSize = 24)
        {
            StopLoading();
            if (_presentationRoot != null && _presentationRoot != presentationRoot)
                _presentationRoot.gameObject.SetActive(false);
            // 流式对象可能被对象池重新配置；在覆盖 root/scope 前必须先释放旧 owner，
            // 否则旧 scope 会失去最后一个可达的清理入口。
            ReleaseAudioResources();
            _cellId = cellId;
            _presentationRoot = presentationRoot;
            _loadRadius = loadRadius;
            _unloadRadius = unloadRadius;
            _startLoaded = startLoaded;
            _activationBatchSize = Mathf.Max(1, activationBatchSize);
            _audioScope = audioScope != null
                ? audioScope
                : (presentationRoot == null ? null : presentationRoot.GetComponentInChildren<D16AudioResourceScope>(true));
            SetLoadedInternal(startLoaded, true);
        }

        private void Awake()
        {
            SetLoadedInternal(_startLoaded, true);
        }

        public bool ShouldBeLoaded(float distance)
        {
            if (float.IsNaN(distance) || float.IsInfinity(distance)) return false;
            distance = Mathf.Max(0f, distance);
            if (IsLoaded) return distance <= _unloadRadius;
            return distance <= _loadRadius;
        }

        public bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(_cellId))
            {
                error = "cell_id_missing";
                return false;
            }
            if (_presentationRoot == null)
            {
                error = "presentation_root_missing";
                return false;
            }
            if (_presentationRoot == transform || transform.IsChildOf(_presentationRoot))
            {
                error = "presentation_root_must_not_contain_cell_controller";
                return false;
            }
            if (_loadRadius < 0f || _unloadRadius <= _loadRadius)
            {
                error = "stream_radius_order_invalid";
                return false;
            }
            error = string.Empty;
            return true;
        }

        public void SetLoaded(bool loaded)
        {
            SetLoadedInternal(loaded, false);
        }

        private void SetLoadedInternal(bool loaded, bool immediate)
        {
            bool wasLoaded = IsLoaded;
            if (wasLoaded == loaded && !immediate)
            {
                // 即使状态没有发生变化，也要清理“尚未进入过加载态但已经被
                // 加载器登记”的 owner 引用。否则一个保持 unloaded 的单元会
                // 把表现音频一直留在 scope 中，直到整个对象被销毁。
                if (!loaded) ReleaseAudioResources();
                return;
            }
            StopLoading();
            IsLoaded = loaded;
            LoadingProgress = 0f;
            LoadedPresentationObjectCount = 0;

            if (!loaded)
            {
                // 只释放本单元进入加载态后新增的引用；共享 scope 中其他单元的
                // 音频必须继续保留，不能因为本单元离开视距而被一起清空。
                ReleaseAudioResources();
                if (_presentationRoot == null) return;
                if (_presentationRoot.gameObject.activeSelf)
                    _presentationRoot.gameObject.SetActive(false);
                return;
            }

            if (_presentationRoot == null) return;

            if (!_presentationRoot.gameObject.activeSelf)
                _presentationRoot.gameObject.SetActive(true);

            if (_audioScope != null)
            {
                EnsureAudioOwner();
                _audioBaselinePaths = _audioScope.CaptureLoadedResourcePaths();
            }

            EnsureActivationPlan();
            if (immediate || _stagedChildren.Count == 0)
            {
                RestoreInitialActivationState();
                LoadCount++;
                return;
            }

            for (int i = 0; i < _stagedChildren.Count; i++)
            {
                var child = _stagedChildren[i];
                if (child != null) child.SetActive(false);
            }
            _loadStartedAt = Time.realtimeSinceStartup;
            LoadCount++;
            _loadingCoroutine = StartCoroutine(ActivatePresentationInBatches());
        }

        private void EnsureActivationPlan()
        {
            if (_presentationRoot == null) return;
            _stagedChildren.Clear();
            var transforms = _presentationRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                var child = transforms[i];
                if (child == null || child == _presentationRoot) continue;
                var childObject = child.gameObject;
                int instanceId = childObject.GetInstanceID();
                if (!_initialActiveStateByInstanceId.ContainsKey(instanceId))
                    _initialActiveStateByInstanceId.Add(instanceId, childObject.activeSelf);
                if (_initialActiveStateByInstanceId[instanceId])
                    _stagedChildren.Add(childObject);
            }
            _stagedChildren.Sort((left, right) => GetDepth(left.transform).CompareTo(GetDepth(right.transform)));
        }

        private static int GetDepth(Transform current)
        {
            int depth = 0;
            while (current != null && current.parent != null)
            {
                depth++;
                current = current.parent;
            }
            return depth;
        }

        private void RestoreInitialActivationState()
        {
            for (int i = 0; i < _stagedChildren.Count; i++)
            {
                var child = _stagedChildren[i];
                if (child != null) child.SetActive(true);
            }
            LoadedPresentationObjectCount = _stagedChildren.Count;
            LoadingProgress = 1f;
            IsLoading = false;
        }

        private IEnumerator ActivatePresentationInBatches()
        {
            IsLoading = true;
            LoadedPresentationObjectCount = 0;
            LoadingProgress = 0f;
            int batchSize = Mathf.Max(1, _activationBatchSize);
            while (LoadedPresentationObjectCount < _stagedChildren.Count)
            {
                int end = Mathf.Min(LoadedPresentationObjectCount + batchSize, _stagedChildren.Count);
                for (int i = LoadedPresentationObjectCount; i < end; i++)
                {
                    var child = _stagedChildren[i];
                    if (child != null) child.SetActive(true);
                }
                LoadedPresentationObjectCount = end;
                LoadingProgress = _stagedChildren.Count == 0
                    ? 1f
                    : (float)LoadedPresentationObjectCount / _stagedChildren.Count;
                if (LoadedPresentationObjectCount < _stagedChildren.Count)
                    yield return null;
            }
            IsLoading = false;
            LoadingProgress = 1f;
            LastLoadDurationMs = Mathf.Max(0f, (Time.realtimeSinceStartup - _loadStartedAt) * 1000f);
            _loadingCoroutine = null;
        }

        private void StopLoading()
        {
            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
                _loadingCoroutine = null;
            }
            IsLoading = false;
        }

        private void EnsureAudioOwner()
        {
            if (_audioScope == null || !string.IsNullOrWhiteSpace(_audioOwnerId)) return;
            _audioOwnerId = "D16StreamCell/" + GetInstanceID();
        }

        private void ReleaseAudioResources()
        {
            if (_audioScope == null)
            {
                _audioOwnerId = null;
                _audioBaselinePaths = null;
                return;
            }

            if (!string.IsNullOrWhiteSpace(_audioOwnerId))
                _audioScope.ReleaseOwner(_audioOwnerId);
            if (_audioBaselinePaths != null)
                _audioScope.ReleaseResourcesAddedSince(_audioBaselinePaths);
            _audioOwnerId = null;
            _audioBaselinePaths = null;
        }

        private void OnDestroy()
        {
            StopLoading();
            ReleaseAudioResources();
            _stagedChildren.Clear();
            _initialActiveStateByInstanceId.Clear();
        }
    }
}
