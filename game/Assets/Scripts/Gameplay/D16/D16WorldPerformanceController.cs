using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Whalebone.Systems.D16;

namespace Whalebone.Gameplay.D16
{
    /// <summary>
    /// InitialHarbor 的 D16 表现预算入口。
    /// 流式单元只切换 PresentationRoot；登记对象只降级表现，不停用碰撞/逻辑对象。
    /// </summary>
    public sealed class D16WorldPerformanceController : MonoBehaviour
    {
        [SerializeField] private Transform _focus;
        [SerializeField] private float _evaluationInterval = 0.25f;
        [SerializeField] private bool _highPressure = false;
        [SerializeField] private bool _autoHighPressure = true;
        [SerializeField] private float _registrationRefreshInterval = 2f;
        [SerializeField] private D16PerformanceBudgetConfiguration _budgetConfiguration = new D16PerformanceBudgetConfiguration();

        private readonly List<D16WorldStreamCell> _cells = new List<D16WorldStreamCell>();
        private readonly List<D16PresentationRegistration> _registrations = new List<D16PresentationRegistration>();
        private readonly List<D16RemoteBuildingProxy> _remoteBuildingProxies =
            new List<D16RemoteBuildingProxy>();
        private readonly List<D16PerformanceCandidate> _candidates = new List<D16PerformanceCandidate>();
        private readonly Dictionary<string, D16PerformanceDecision> _decisionsById =
            new Dictionary<string, D16PerformanceDecision>(System.StringComparer.Ordinal);
        private readonly HashSet<int> _invalidCellInstanceIds = new HashSet<int>();
        private readonly HashSet<int> _invalidRemoteBuildingProxyInstanceIds = new HashSet<int>();
        private D16PerformanceBudgetSystem _budget;
        private D16IslandDataStreamRegistry _islandDataStream;
        private float _nextEvaluation;
        private float _nextRegistrationRefresh;
        private D16PerformanceBudgetSnapshot _lastSnapshot;
        private float _lastCellTransitionMs;
        private float _totalCellTransitionMs;
        private int _cellTransitionCount;
        private bool _firstUpdateMeasured;
        private bool _firstRegistrationRefreshMeasured;
        private bool _firstBudgetEvaluationMeasured;

        public D16PerformanceBudgetSnapshot LastSnapshot => _lastSnapshot;
        public int RegistrationCount => _registrations.Count;
        public int LoadedCellCount { get; private set; }
        public float LastCellTransitionMs => _lastCellTransitionMs;
        public float TotalCellTransitionMs => _totalCellTransitionMs;
        public int CellTransitionCount => _cellTransitionCount;
        public int LoadingCellCount { get; private set; }
        public int LoadedPresentationObjectCount { get; private set; }
        public int PendingPresentationObjectCount { get; private set; }
        public float MaxPresentationLoadDurationMs { get; private set; }
        public int PresentationLoadCount { get; private set; }
        public int CatalogIslandCellCount => _islandDataStream == null ? 0 : _islandDataStream.CatalogCellCount;
        public int ActiveIslandDataCellCount => _islandDataStream == null ? 0 : _islandDataStream.ActiveCellCount;
        public int DataOnlyIslandCellCount => _islandDataStream == null ? 0 : _islandDataStream.DataOnlyCellCount;
        public int VisualPresentationIslandCellCount =>
            _islandDataStream == null ? 0 : _islandDataStream.VisualPresentationCellCount;
        public string ReferenceIslandId => _islandDataStream == null ? string.Empty : _islandDataStream.ReferenceIslandId;
        public float StartupAwakeMs { get; private set; }
        public float FirstRegistrationRefreshMs { get; private set; }
        public float FirstUpdateMs { get; private set; }
        public float FirstBudgetEvaluationMs { get; private set; }

        /// <summary>
        /// 为运行时场景引导或验收夹具指定观察焦点。
        /// 性能控制器仍只裁决表现预算，不接管焦点对象的移动或领域状态。
        /// </summary>
        public void ConfigureRuntimeFocus(Transform focus, float evaluationInterval = 0.1f)
        {
            _focus = focus;
            _evaluationInterval = Mathf.Max(0.05f, evaluationInterval);
            _nextEvaluation = 0f;
            RefreshRegistrations();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D16WorldPerformanceController>(FindObjectsInactive.Include) != null) return;
            new GameObject("D16WorldPerformanceController").AddComponent<D16WorldPerformanceController>();
        }

        private void Awake()
        {
            float startedAt = Time.realtimeSinceStartup;
            _budget = new D16PerformanceBudgetSystem(_budgetConfiguration);
            EnsureIslandDataStream();
            EnsureDefaultWorldCells();
            RefreshRegistrations();
            if (FindFirstObjectByType<D16RuntimePerformanceProbe>(FindObjectsInactive.Include) == null)
                gameObject.AddComponent<D16RuntimePerformanceProbe>();
            StartupAwakeMs = (Time.realtimeSinceStartup - startedAt) * 1000f;
        }

        private void Update()
        {
            bool measureFirstUpdate = !_firstUpdateMeasured;
            float startedAt = measureFirstUpdate ? Time.realtimeSinceStartup : 0f;
            if (Time.unscaledTime < _nextEvaluation)
            {
                RecordFirstUpdate(startedAt, measureFirstUpdate);
                return;
            }
            _nextEvaluation = Time.unscaledTime + Mathf.Max(0.05f, _evaluationInterval);
            if (_focus == null) _focus = FindFocus();
            if (Time.unscaledTime >= _nextRegistrationRefresh)
            {
                EnsureIslandDataStream();
                _islandDataStream.RefreshFromCurrentSnapshot();
                EnsureDefaultRegistrations();
                EnsureDefaultWorldCells();
                RefreshRegistrations();
                _nextRegistrationRefresh = Time.unscaledTime + Mathf.Max(0.5f, _registrationRefreshInterval);
            }
            ApplyStreamCells();
            EvaluatePresentationBudget();
            RecordFirstUpdate(startedAt, measureFirstUpdate);
        }

        public void RefreshRegistrations()
        {
            float startedAt = Time.realtimeSinceStartup;
            EnsureIslandDataStream();
            _islandDataStream.RefreshFromCurrentSnapshot();
            // 公开刷新入口也必须补齐运行时默认对象。D2 采用分阶段组装后，
            // 手动刷新可能发生在玩家/船员刚创建的边界；只扫描会永久漏掉该对象，
            // 直到下一次定时刷新才进入预算快照。
            EnsureDefaultRegistrations();
            _cells.Clear();
            _registrations.Clear();
            _remoteBuildingProxies.Clear();
            _cells.AddRange(FindObjectsByType<D16WorldStreamCell>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            _registrations.AddRange(FindObjectsByType<D16PresentationRegistration>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            _remoteBuildingProxies.AddRange(FindObjectsByType<D16RemoteBuildingProxy>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            if (!_firstRegistrationRefreshMeasured)
            {
                FirstRegistrationRefreshMs = (Time.realtimeSinceStartup - startedAt) * 1000f;
                _firstRegistrationRefreshMeasured = true;
            }
        }

        private void ApplyStreamCells()
        {
            _lastCellTransitionMs = 0f;
            if (_focus == null)
            {
                LoadedCellCount = 0;
                LoadingCellCount = 0;
                LoadedPresentationObjectCount = 0;
                PendingPresentationObjectCount = 0;
                MaxPresentationLoadDurationMs = 0f;
                PresentationLoadCount = 0;
                return;
            }
            LoadedCellCount = 0;
            LoadingCellCount = 0;
            LoadedPresentationObjectCount = 0;
            PendingPresentationObjectCount = 0;
            MaxPresentationLoadDurationMs = 0f;
            PresentationLoadCount = 0;
            foreach (var cell in _cells)
            {
                if (cell == null || cell.PresentationRoot == null) continue;
                string validationError;
                if (!cell.Validate(out validationError))
                {
                    int instanceId = cell.GetInstanceID();
                    if (_invalidCellInstanceIds.Add(instanceId))
                        Debug.LogWarning("[D16/Streaming] 忽略无效流式单元 " + cell.name + ": " + validationError);
                    continue;
                }
                float distance = Vector3.Distance(_focus.position, cell.transform.position);
                bool shouldLoad = cell.ShouldBeLoaded(distance);
                if (shouldLoad != cell.IsLoaded)
                {
                    float started = Time.realtimeSinceStartup;
                    cell.SetLoaded(shouldLoad);
                    float elapsedMs = (Time.realtimeSinceStartup - started) * 1000f;
                    _lastCellTransitionMs += elapsedMs;
                    _totalCellTransitionMs += elapsedMs;
                    _cellTransitionCount++;
                }
                if (cell.IsLoaded) LoadedCellCount++;
                if (cell.IsLoading) LoadingCellCount++;
                LoadedPresentationObjectCount += cell.LoadedPresentationObjectCount;
                PendingPresentationObjectCount += cell.PendingPresentationObjectCount;
                MaxPresentationLoadDurationMs = Mathf.Max(
                    MaxPresentationLoadDurationMs, cell.LastLoadDurationMs);
                PresentationLoadCount += cell.LoadCount;
            }
        }

        private void EvaluatePresentationBudget()
        {
            bool measureFirstEvaluation = !_firstBudgetEvaluationMeasured;
            float startedAt = measureFirstEvaluation ? Time.realtimeSinceStartup : 0f;
            _candidates.Clear();
            foreach (var registration in _registrations)
                if (registration != null) _candidates.Add(registration.BuildCandidate(_focus));
            foreach (var proxy in _remoteBuildingProxies)
            {
                if (proxy == null) continue;
                string validationError;
                if (!proxy.Validate(out validationError))
                {
                    if (_invalidRemoteBuildingProxyInstanceIds.Add(proxy.GetInstanceID()))
                        Debug.LogWarning("[D16/RemoteBuilding] 忽略无效远景建筑代理 " + proxy.name + ": " + validationError);
                    continue;
                }
                _candidates.Add(proxy.BuildCandidate(_focus));
            }
            bool highPressure = _highPressure;
            if (_autoHighPressure && _budget != null && _budget.Configuration.TargetFps > 0)
            {
                float frameBudgetMs = 1000f / _budget.Configuration.TargetFps;
                highPressure |= Time.unscaledDeltaTime * 1000f > frameBudgetMs * 1.35f;
            }
            _lastSnapshot = _budget.Evaluate(_candidates, highPressure);
            _decisionsById.Clear();
            foreach (var decision in _lastSnapshot.Decisions)
                if (decision != null && !_decisionsById.ContainsKey(decision.Id))
                    _decisionsById.Add(decision.Id, decision);
            foreach (var registration in _registrations)
            {
                if (registration == null) continue;
                D16PerformanceDecision decision;
                if (_decisionsById.TryGetValue(registration.StableId, out decision))
                    registration.Apply(decision);
            }
            foreach (var proxy in _remoteBuildingProxies)
            {
                if (proxy == null) continue;
                D16PerformanceDecision decision;
                if (_decisionsById.TryGetValue(proxy.StableId, out decision))
                    proxy.Apply(decision);
            }
            if (measureFirstEvaluation)
            {
                FirstBudgetEvaluationMs = (Time.realtimeSinceStartup - startedAt) * 1000f;
                _firstBudgetEvaluationMeasured = true;
            }
        }

        private void RecordFirstUpdate(float startedAt, bool measureFirstUpdate)
        {
            if (!measureFirstUpdate) return;
            FirstUpdateMs = (Time.realtimeSinceStartup - startedAt) * 1000f;
            _firstUpdateMeasured = true;
        }

        private void EnsureDefaultRegistrations()
        {
            EnsureRegistration(GameObject.Find("D2_PlayerAvatar"), "player.avatar", false, false, false, true, true);
            EnsureRegistration(GameObject.Find("ph_ship_fishing"), "player.ship", false, false, true, true, true);
            EnsureRegistration(GameObject.Find("ph_crew_male"), "crew.harbor_anchor", true, false, false, false, false);
        }

        private void EnsureIslandDataStream()
        {
            if (_islandDataStream == null)
                _islandDataStream = GetComponent<D16IslandDataStreamRegistry>();
            if (_islandDataStream == null)
                _islandDataStream = gameObject.AddComponent<D16IslandDataStreamRegistry>();
            if (_islandDataStream.CatalogCellCount == 0)
                _islandDataStream.ConfigureRuntime();
        }

        private static void EnsureDefaultWorldCells()
        {
            EnsureWorldCell(GameObject.Find("Sea"), "harbor.sea", 140f, 220f);
            EnsureWorldCell(GameObject.Find("Beach"), "harbor.beach", 100f, 180f);
        }

        private static void EnsureWorldCell(GameObject presentation, string cellId,
            float loadRadius, float unloadRadius)
        {
            if (presentation == null) return;
            string objectName = "D16_StreamCell_" + cellId.Replace('.', '_');
            if (GameObject.Find(objectName) != null) return;

            var cellObject = new GameObject(objectName);
            cellObject.transform.position = presentation.transform.position;
            var cell = cellObject.AddComponent<D16WorldStreamCell>();
            cell.ConfigureRuntime(cellId, presentation.transform, loadRadius, unloadRadius, true);
        }

        private static void EnsureRegistration(GameObject target, string stableId, bool isAgent, bool isAnimal,
            bool usesAudio, bool playerRelevant, bool activeInteraction)
        {
            if (target == null) return;
            var registration = target.GetComponent<D16PresentationRegistration>();
            if (registration == null)
                registration = target.AddComponent<D16PresentationRegistration>();
            // 玩家由港口船员对象复制出来时，会继承船员的表现登记组件。
            // 运行时默认目标必须重新绑定自己的稳定 ID，否则两个对象会被预算系统
            // 当成一个候选项，造成候选数量丢失和错误的表现裁决。
            if (!registration.MatchesRuntimeConfiguration(stableId, isAgent, isAnimal, usesAudio,
                    playerRelevant, activeInteraction))
            {
                registration.ConfigureRuntime(stableId, isAgent, isAnimal, usesAudio, playerRelevant, activeInteraction);
            }
        }

        private static Transform FindFocus()
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            if (player != null) return player.transform;
            var camera = Camera.main;
            return camera == null ? null : camera.transform;
        }
    }
}
