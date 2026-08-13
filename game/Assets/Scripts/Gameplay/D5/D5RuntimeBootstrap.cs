// ============================================================
// D5RuntimeBootstrap —— D5 打磨上架阶段的 Unity 表现接线
//
// 只收集输入、调用 Systems.D5、显示快照和生成轻量海图标记。
// 世界发现、特质生成、预设、性能、成就和云队列都由 D5 系统裁决。
// ============================================================

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.D7;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.UI.D16;
using Whalebone.Systems.D5;
using Whalebone.Systems.D16;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D5
{
    public sealed class D5RuntimeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D5RuntimeBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D5RuntimeBootstrap").AddComponent<D5RuntimeBootstrap>();
            Debug.Log("[D5] 打磨上架运行时入口已创建");
        }

        private D5RuntimeSnapshot _snapshot;
        private D5WorldSystem _world;
        private D5CharacterSystem _character;
        private D5PerformanceSystem _performance;
        private D5SteamSystem _steam;
        private Transform _player;
        private Vector3 _basePlayerScale = Vector3.one;
        private bool _ready;
        private int _presetIndex;
        private string _message = "D5 正在等待场景对象…";
        private RuntimeBuildPhase _runtimeBuildPhase = RuntimeBuildPhase.WaitingForPlayer;
        private bool _characterVisualBuildStarted;
        private const int CharacterRenderersPerFrame = 4;
        private Renderer[] _pendingCharacterRenderers;
        private int _pendingCharacterRendererIndex;
        private Color _pendingCharacterColor;
        private bool _characterVisualPending;
        private float _runtimeBuildStartedAt;
        private float _phaseStartedAt;
        private bool _phaseTimerStarted;
        private RuntimeBuildPhase _timedPhase;

        private enum RuntimeBuildPhase
        {
            WaitingForPlayer,
            LoadingSnapshot,
            CreatingSystems,
            InitializingState,
            ApplyingCharacterVisual,
            BuildingMap,
            Complete,
        }

        public bool IsReady => _ready;
        public int SaveRevision { get; private set; }
        public float AwakeDurationMs { get; private set; }
        public float RuntimeBuildElapsedMs { get; private set; }
        public bool RuntimePhaseTimingsComplete { get; private set; }
        public int CompletedRuntimePhaseCount { get; private set; }
        public float WaitingForPlayerMs { get; private set; }
        public float LoadingSnapshotMs { get; private set; }
        public float CreatingSystemsMs { get; private set; }
        public float InitializingStateMs { get; private set; }
        public float ApplyingCharacterVisualMs { get; private set; }
        public float BuildingMapMs { get; private set; }

        private Text _statusText;
        private Text _detailText;
        private Text _hintText;
        private Text _mapTitle;
        private readonly Dictionary<string, Image> _markerImages = new Dictionary<string, Image>();
        private readonly Dictionary<string, D5ButtonLabel> _buttonLabels = new Dictionary<string, D5ButtonLabel>();

        private sealed class D5ButtonLabel
        {
            public readonly Text Text;
            public readonly string Chinese;
            public readonly string English;

            public D5ButtonLabel(Text text, string chinese, string english)
            {
                Text = text;
                Chinese = chinese;
                English = english;
            }
        }

        private void Awake()
        {
            float startedAt = Time.realtimeSinceStartup;
            _runtimeBuildStartedAt = startedAt;
            EnsureEventSystem();
            BuildUi();
            RuntimeHudCoordinator.ApplyNow();
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            AwakeDurationMs = (Time.realtimeSinceStartup - startedAt) * 1000f;
        }

        private void Update()
        {
            if (!_ready || _performance == null || _world == null || _character == null || _steam == null)
            {
                _ready = false;
                TryBuildRuntime();
                return;
            }

            ApplyPendingCharacterVisuals();
            _performance.RecordFrame(Time.unscaledDeltaTime);
            if (Time.timeScale > 0f && D7ControlCoordinator.GameplayInputAllowed)
            {
                if (D16InputRouter.WasPressedThisFrame(D16InputAction.D5DiscoverIsland)) DiscoverNextIsland();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D5VisitIsland)) VisitCurrentIsland();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.CharacterPreset)) NextPreset();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.Language)) ToggleLanguage();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.Quality)) CycleQuality();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.Achievement)) UnlockAchievement();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.Save)) SaveProgress();
            }
            RefreshHud();
        }

        public void DiscoverNextIsland()
        {
            if (!_ready) return;
            var result = _world.DiscoverNext();
            _message = result.Message;
            if (_world.Snapshot.DiscoveredIslands.Count >= D5IslandCatalog.All.Count)
                _steam.UnlockAchievement("discover_all_islands");
            RefreshHud();
        }

        public void VisitCurrentIsland()
        {
            if (!_ready) return;
            var result = _world.VisitIsland(_world.Snapshot.CurrentIslandId);
            _message = result.Message;
            if (result.Accepted && GameState.CurrentPlayer != null && GameState.CurrentPlayer.Location != null)
            {
                GameState.CurrentPlayer.Location.Type = "port";
                GameState.CurrentPlayer.Location.PortId = result.IslandId;
            }
            RefreshHud();
        }

        public void NextPreset()
        {
            if (!_ready || D5CharacterCatalog.All.Count == 0) return;
            _presetIndex = (_presetIndex + 1) % D5CharacterCatalog.All.Count;
            var preset = D5CharacterCatalog.All[_presetIndex];
            _character.ApplyPreset(preset.Id);
            ApplyCharacterVisual();
            _steam.UnlockAchievement("customized_captain");
            _message = "角色预设已切换：" + Localized(preset.NameZh, preset.NameEn) + "。";
            RefreshHud();
        }

        public void ToggleLanguage()
        {
            if (!_ready) return;
            _snapshot.Language = _snapshot.Language == "en" ? "zh" : "en";
            Localization.SetLanguage(_snapshot.Language);
            _message = _snapshot.Language == "en" ? "Language switched to English." : "语言已切换为简体中文。";
            RefreshHud();
        }

        public void CycleQuality()
        {
            if (!_ready) return;
            var current = _performance.Snapshot.Quality;
            var next = current == D5QualityLevel.Low ? D5QualityLevel.Medium :
                current == D5QualityLevel.Medium ? D5QualityLevel.High : D5QualityLevel.Low;
            _performance.SetQuality(next);
            ApplyQualityToUnity(next);
            _message = "画质档位：" + next + "。";
            RefreshHud();
        }

        public void UnlockAchievement()
        {
            if (!_ready) return;
            string id = _world.Snapshot.DiscoveredIslands.Count >= D5IslandCatalog.All.Count
                ? "discover_all_islands" : "first_voyage";
            _message = _steam.UnlockAchievement(id) ? "成就已解锁：" + LocalizedAchievement(id) : "成就已经解锁或条件尚未满足。";
            RefreshHud();
        }

        public void SaveProgress()
        {
            if (!_ready || _snapshot == null) return;
            // D7 的 F5 是首航全量保存入口；各模块仍只写自己的快照。
            D7SaveCoordinator.SaveFoundationState();
            SyncSnapshot();
            try
            {
                _steam.QueueCloudUpload();
                SyncSnapshot();
                SaveSystem.SaveD5Runtime(_snapshot);
                SaveRevision++;
                _message = "D5 状态已保存，云存档已进入待同步队列。";
                Debug.Log("[D5/Save] 世界、特质、预设、性能和 Steam 状态已保存");
            }
            catch (Exception ex)
            {
                _message = "D5 保存失败：" + ex.Message;
                Debug.LogWarning("[D5/Save] " + ex.Message);
            }
        }

        public void ToggleShipControl()
        {
            var router = FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            if (router == null)
            {
                _message = "人物控制尚未就绪，请稍候再试。";
                RefreshHud();
                return;
            }

            if (!router.TryToggleShip())
            {
                _message = router.Hint;
                RefreshHud();
                return;
            }

            _message = router.Mode == Whalebone.Systems.D2.D2ControlMode.Ship
                ? "已登船：现在可以用 W/S 航行、A/D 转向。"
                : "已下船：现在可以用 WASD 移动人物。";
            RefreshHud();
        }

        private void TryBuildRuntime()
        {
            BeginPhaseTiming();
            switch (_runtimeBuildPhase)
            {
                case RuntimeBuildPhase.WaitingForPlayer:
                    var player = GameObject.Find("D2_PlayerAvatar");
                    if (player == null) return;
                    _player = player.transform;
                    _basePlayerScale = _player.localScale;
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.LoadingSnapshot;
                    return;

                case RuntimeBuildPhase.LoadingSnapshot:
                    // 读档属于 D5 的持久化入口，单独占一帧，避免与场景表现构建叠加。
                    _snapshot = SaveSystem.LoadD5Runtime() ?? new D5RuntimeSnapshot();
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.CreatingSystems;
                    return;

                case RuntimeBuildPhase.CreatingSystems:
                    _world = new D5WorldSystem(_snapshot.World);
                    _character = new D5CharacterSystem(_snapshot.Character);
                    _performance = new D5PerformanceSystem(_snapshot.Performance);
                    _steam = new D5SteamSystem(_snapshot.Steam);
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.InitializingState;
                    return;

                case RuntimeBuildPhase.InitializingState:
                    InitializeTraits();
                    _presetIndex = IndexOfPreset(_character.Snapshot.PresetId);
                    ApplyQualityToUnity(_performance.Snapshot.Quality);
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.ApplyingCharacterVisual;
                    return;

                case RuntimeBuildPhase.ApplyingCharacterVisual:
                    if (!_characterVisualBuildStarted)
                    {
                        ApplyCharacterVisual();
                        _characterVisualBuildStarted = true;
                    }
                    else
                    {
                        ApplyPendingCharacterVisuals();
                    }

                    if (_characterVisualPending) return;
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.BuildingMap;
                    return;

                case RuntimeBuildPhase.BuildingMap:
                    BuildIslandMap();
                    _ready = true;
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.Complete;
                    RuntimeBuildElapsedMs = (Time.realtimeSinceStartup - _runtimeBuildStartedAt) * 1000f;
                    RuntimePhaseTimingsComplete = true;
                    SyncSnapshot();
                    RefreshHud();
                    Debug.Log("[D5] 运行时已接线：12 岛海图、50 特质、基础预设、本地化、性能和 Steam Bridge");
                    return;

                case RuntimeBuildPhase.Complete:
                    return;
            }
        }

        private void BeginPhaseTiming()
        {
            if (_phaseTimerStarted) return;
            _timedPhase = _runtimeBuildPhase;
            _phaseStartedAt = Time.realtimeSinceStartup;
            _phaseTimerStarted = true;
        }

        private void CompletePhaseTiming()
        {
            if (!_phaseTimerStarted || _timedPhase != _runtimeBuildPhase) return;
            float durationMs = Mathf.Max(0f, (Time.realtimeSinceStartup - _phaseStartedAt) * 1000f);
            switch (_timedPhase)
            {
                case RuntimeBuildPhase.WaitingForPlayer:
                    WaitingForPlayerMs = durationMs;
                    break;
                case RuntimeBuildPhase.LoadingSnapshot:
                    LoadingSnapshotMs = durationMs;
                    break;
                case RuntimeBuildPhase.CreatingSystems:
                    CreatingSystemsMs = durationMs;
                    break;
                case RuntimeBuildPhase.InitializingState:
                    InitializingStateMs = durationMs;
                    break;
                case RuntimeBuildPhase.ApplyingCharacterVisual:
                    ApplyingCharacterVisualMs = durationMs;
                    break;
                case RuntimeBuildPhase.BuildingMap:
                    BuildingMapMs = durationMs;
                    break;
            }
            CompletedRuntimePhaseCount++;
            _phaseTimerStarted = false;
        }

        private void InitializeTraits()
        {
            if (GameState.CurrentPlayer == null) return;
            var playerTraits = GameState.CurrentPlayer.Traits;
            if (playerTraits == null || !D5TraitSystem.IsValidTraitSet(playerTraits, 3, 4))
                GameState.CurrentPlayer.Traits = D5TraitSystem.GenerateTraitIds(GameState.CurrentPlayer.Id, 4);
            _snapshot.PlayerTraitIds = new List<string>(GameState.CurrentPlayer.Traits);

            foreach (var crew in GameState.CrewAgents)
            {
                List<string> traits;
                if (!GameState.CrewAgentTraitIds.TryGetValue(crew.Id, out traits) || !D5TraitSystem.IsValidTraitSet(traits, 3, 4))
                {
                    traits = D5TraitSystem.GenerateTraitIds(crew.Id, 3);
                    GameState.CrewAgentTraitIds[crew.Id] = traits;
                }
                _snapshot.CrewTraitIds[crew.Id] = new List<string>(traits);
            }
        }

        private void SyncSnapshot()
        {
            if (_snapshot == null) return;
            _snapshot.World = _world == null ? _snapshot.World : _world.Snapshot;
            _snapshot.Character = _character == null ? _snapshot.Character : _character.Snapshot;
            _snapshot.Performance = _performance == null ? _snapshot.Performance : _performance.Snapshot;
            _snapshot.Steam = _steam == null ? _snapshot.Steam : _steam.Snapshot;
            if (GameState.CurrentPlayer != null)
                _snapshot.PlayerTraitIds = new List<string>(GameState.CurrentPlayer.Traits ?? new List<string>());
            if (GameState.CurrentPlayer != null) GameState.CurrentPlayer.D5Runtime = _snapshot.Clone();
        }

        private void ApplyCharacterVisual()
        {
            if (_player == null || _character == null) return;
            var preset = _character.CurrentPreset;
            if (preset == null) return;
            // D2CharacterController 统一拥有角色物理根节点和视觉归一化。
            // D5 预设不能再改根节点缩放，否则会把 CharacterController 的
            // 世界尺寸重新压回 FBX 原始比例，导致首帧抬升、漂浮和动作误判。
            // 没有 D2 物理组件的纯表现对象才沿用旧的根节点缩放路径。
            if (_player.GetComponent<D2CharacterController>() == null)
                _player.localScale = _basePlayerScale * preset.BodyScale;
            _pendingCharacterRenderers = _player.GetComponentsInChildren<Renderer>(true);
            _pendingCharacterRendererIndex = 0;
            _pendingCharacterColor = new Color32(
                (byte)((preset.AccentColor >> 16) & 0xff),
                (byte)((preset.AccentColor >> 8) & 0xff),
                (byte)(preset.AccentColor & 0xff),
                255);
            _characterVisualPending = _pendingCharacterRenderers.Length > 0;
            ApplyPendingCharacterVisuals();
        }

        /// <summary>
        /// 角色表现只允许在 D5 表现层分帧处理；玩法状态和输入不会等待材质复制。
        /// 这样首次进入 InitialHarbor 时不会把所有材质实例化集中到同一帧，
        /// 但队列仍会在后续帧完整应用当前角色预设。
        /// </summary>
        private void ApplyPendingCharacterVisuals()
        {
            if (!_characterVisualPending || _pendingCharacterRenderers == null) return;

            int processed = 0;
            while (_pendingCharacterRendererIndex < _pendingCharacterRenderers.Length &&
                   processed++ < CharacterRenderersPerFrame)
            {
                var renderer = _pendingCharacterRenderers[_pendingCharacterRendererIndex++];
                if (renderer == null) continue;
                var sources = renderer.sharedMaterials;
                for (int i = 0; i < sources.Length; i++)
                {
                    var material = sources[i] == null
                        ? CreateCharacterFallbackMaterial(renderer.name)
                        : new Material(sources[i]);
                    material.name = "D7CharacterPreset_" + renderer.name + "_" + i;
                    if (material.HasProperty("_BaseColor"))
                    {
                        var baseColor = material.GetColor("_BaseColor");
                        // 有贴图时只轻微染色，避免角色预设把衣服和皮肤变成纯色块。
                        Color tint = material.mainTexture == null
                            ? _pendingCharacterColor
                            : Color.Lerp(Color.white, _pendingCharacterColor, 0.16f);
                        baseColor = new Color(baseColor.r * tint.r, baseColor.g * tint.g,
                            baseColor.b * tint.b, baseColor.a);
                        material.SetColor("_BaseColor", baseColor);
                    }
                    if (material.HasProperty("_Color")) material.SetColor("_Color", _pendingCharacterColor);
                    sources[i] = material;
                }
                renderer.sharedMaterials = sources;
            }

            if (_pendingCharacterRendererIndex >= _pendingCharacterRenderers.Length)
            {
                _characterVisualPending = false;
                _pendingCharacterRenderers = null;
            }
        }

        private static Material CreateCharacterFallbackMaterial(string rendererName)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { name = "D7CharacterFallback_" + rendererName };
        }

        private void ApplyQualityToUnity(D5QualityLevel quality)
        {
            int index = quality == D5QualityLevel.Low ? 0 : quality == D5QualityLevel.Medium ? 1 : 2;
            if (QualitySettings.names != null && QualitySettings.names.Length > index) QualitySettings.SetQualityLevel(index, true);
            Application.targetFrameRate = 60;
        }

        private void BuildIslandMap()
        {
            var map = GameObject.Find("D5WorldMap");
            if (map != null) return;
            map = new GameObject("D5WorldMap", typeof(RectTransform), typeof(Image));
            map.transform.SetParent(transform.Find("D5Canvas"), false);
            var mapRect = (RectTransform)map.transform;
            mapRect.anchorMin = new Vector2(0f, 0f); mapRect.anchorMax = new Vector2(0f, 0f); mapRect.pivot = new Vector2(0f, 0f);
            mapRect.sizeDelta = new Vector2(720f, 300f); mapRect.anchoredPosition = new Vector2(28f, 88f);
            map.GetComponent<Image>().sprite = UiSprites.White;
            map.GetComponent<Image>().color = new Color(0.02f, 0.08f, 0.12f, 0.88f);
            _mapTitle = CreateText(map, "MapTitle", D5LocalizationSystem.Get("d5_world", _snapshot.Language), 22, new Vector2(20f, -16f), new Vector2(420f, 35f), TextAnchor.UpperLeft);
            foreach (var island in D5IslandCatalog.All)
            {
                var marker = new GameObject("D5IslandMarker_" + island.Id, typeof(RectTransform), typeof(Image), typeof(Button));
                marker.transform.SetParent(map.transform, false);
                var rect = (RectTransform)marker.transform;
                rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f); rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(34f, 34f); rect.anchoredPosition = new Vector2(island.MapX * 285f, island.MapY * 108f - 8f);
                marker.GetComponent<Image>().sprite = UiSprites.White;
                marker.GetComponent<Button>().onClick.AddListener(() => SelectIsland(island.Id));
                var label = CreateText(marker, "Label", island.Id.Substring(0, 1).ToUpperInvariant(), 14, Vector2.zero, new Vector2(34f, 34f), TextAnchor.MiddleCenter);
                label.color = Color.white;
                _markerImages[island.Id] = marker.GetComponent<Image>();
            }
        }

        private void SelectIsland(string islandId)
        {
            if (!_ready || !_world.IsDiscovered(islandId))
            {
                _message = "该岛屿尚未发现。按 I 逐步扩展海图。";
                return;
            }
            var result = _world.VisitIsland(islandId);
            _message = result.Message;
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (!_ready || _statusText == null) return;
            string language = _snapshot.Language;
            var world = _world.Snapshot;
            var character = _character.Snapshot;
            var performance = _performance.Report();
            var preset = D5CharacterCatalog.Find(character.PresetId);
            foreach (var buttonLabel in _buttonLabels.Values)
                buttonLabel.Text.text = Localized(buttonLabel.Chinese, buttonLabel.English);
            _statusText.text = Localized("D5 打磨上架", "D5 Release Polish") + "  |  " +
                Localized("海图 " + world.DiscoveredIslands.Count + "/12", "Chart " + world.DiscoveredIslands.Count + "/12") +
                "  |  " + LocalizedIsland(world.CurrentIslandId);
            var detail = new StringBuilder();
            detail.Append(Localized("特质：", "Traits: "));
            foreach (var traitId in _snapshot.PlayerTraitIds) detail.Append(LocalizedTrait(traitId)).Append(" ");
            detail.Append("\n").Append(Localized("预设：", "Preset: ")).Append(preset == null ? character.PresetId : Localized(preset.NameZh, preset.NameEn));
            detail.Append("\n").Append(Localized("性能：", "Performance: ")).Append(performance.LastFps.ToString("0.0")).Append(" FPS / ").Append(performance.MinFps.ToString("0.0"));
            detail.Append("  ").Append(performance.MeetsTarget ? "✓" : "…").Append("  ").Append(_steam.StatusText());
            _detailText.text = detail.ToString();
            _hintText.text = _message + (language == "en"
                ? "    I discover  V visit  P preset  L language  F1 quality  F2 achievement  F5 save  |  WASD move  right/middle drag camera  F3 view  Shift run  Space jump  E board/leave  B/T/N/G/L trade/voyage  F/K/R/U/A/M/J content"
                : "    I发现  V访问  P预设  L语言  F1画质  F2成就  F5保存  |  WASD移动  右键/中键拖动视角  F3切换视角  Shift奔跑  Space跳跃  E上船/下船  B/T/N/G/L贸易与航行  F/K/R/U/A/M/J内容");
            _mapTitle.text = D5LocalizationSystem.Get("d5_world", language) + "  ·  " + world.DiscoveredIslands.Count + "/12";
            foreach (var island in D5IslandCatalog.All)
            {
                Image image;
                if (!_markerImages.TryGetValue(island.Id, out image)) continue;
                bool discovered = world.DiscoveredIslands.Contains(island.Id);
                bool current = world.CurrentIslandId == island.Id;
                image.color = current ? new Color(0.95f, 0.75f, 0.15f, 1f) : discovered ? new Color(0.25f, 0.75f, 0.95f, 1f) : new Color(0.25f, 0.28f, 0.32f, 1f);
            }
        }

        private string Localized(string zh, string en) { return _snapshot.Language == "en" ? en : zh; }

        private string LocalizedIsland(string id) { return D5LocalizationSystem.Get("island_" + id, _snapshot.Language); }

        private string LocalizedTrait(string id) { return D5LocalizationSystem.Get("trait_" + id, _snapshot.Language); }

        private string LocalizedAchievement(string id)
        {
            var achievement = D5AchievementCatalog.Find(id);
            return achievement == null ? id : Localized(achievement.NameZh, achievement.NameEn);
        }

        private int IndexOfPreset(string id)
        {
            for (int i = 0; i < D5CharacterCatalog.All.Count; i++) if (D5CharacterCatalog.All[i].Id == id) return i;
            return 0;
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("D5Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 160;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            _statusText = CreateText(canvasGo, "D5Status", "D5 正在等待场景对象…", 22, Vector2.zero, new Vector2(860f, 45f), TextAnchor.UpperLeft);
            SetTopCenter(_statusText.rectTransform, new Vector2(430f, -24f));
            _detailText = CreateText(canvasGo, "D5Detail", "", 17, Vector2.zero, new Vector2(690f, 110f), TextAnchor.UpperLeft);
            SetBottomLeft(_detailText.rectTransform, new Vector2(28f, 400f));
            _hintText = CreateText(canvasGo, "D5Hint", "", 16, Vector2.zero, new Vector2(1800f, 48f), TextAnchor.MiddleLeft);
            SetBottomLeft(_hintText.rectTransform, new Vector2(28f, 24f));

            var controls = new GameObject("D5Controls", typeof(RectTransform), typeof(Image));
            controls.transform.SetParent(canvasGo.transform, false);
            var controlsRect = (RectTransform)controls.transform;
            controlsRect.anchorMin = new Vector2(1f, 0f); controlsRect.anchorMax = new Vector2(1f, 0f); controlsRect.pivot = new Vector2(1f, 0f);
            controlsRect.sizeDelta = new Vector2(360f, 350f); controlsRect.anchoredPosition = new Vector2(-28f, 88f);
            controls.GetComponent<Image>().sprite = UiSprites.White;
            controls.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.08f, 0.92f);
            CreateButton(controls, "D5Ship", "上船/下船 [E]", "Board/Leave ship [E]", new Vector2(0f, -28f), ToggleShipControl);
            CreateButton(controls, "D5Discover", "发现下一个岛 [I]", "Discover next island [I]", new Vector2(0f, -68f), DiscoverNextIsland);
            CreateButton(controls, "D5Visit", "驶向当前岛 [V]", "Visit current island [V]", new Vector2(0f, -108f), VisitCurrentIsland);
            CreateButton(controls, "D5Preset", "角色预设 [P]", "Character preset [P]", new Vector2(0f, -148f), NextPreset);
            CreateButton(controls, "D5Language", "中英切换 [L]", "Switch language [L]", new Vector2(0f, -188f), ToggleLanguage);
            CreateButton(controls, "D5Quality", "画质档位 [F1]", "Quality [F1]", new Vector2(0f, -228f), CycleQuality);
            CreateButton(controls, "D5Achievement", "解锁成就 [F2]", "Unlock achievement [F2]", new Vector2(0f, -268f), UnlockAchievement);
            CreateButton(controls, "D5Save", "保存状态 [F5]", "Save state [F5]", new Vector2(0f, -308f), SaveProgress);
        }

        private static Text CreateText(GameObject parent, string name, string content, int size, Vector2 position, Vector2 sizeDelta, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(0f, 1f); rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = sizeDelta; rect.anchoredPosition = position;
            var text = go.GetComponent<Text>();
            text.font = Fonts.Default; text.fontSize = size; text.color = Color.white; text.alignment = anchor; text.text = content;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void SetTopCenter(RectTransform rect, Vector2 position)
        {
            rect.anchorMin = new Vector2(0.5f, 1f); rect.anchorMax = new Vector2(0.5f, 1f); rect.pivot = new Vector2(0.5f, 1f); rect.anchoredPosition = position;
        }

        private static void SetBottomLeft(RectTransform rect, Vector2 position)
        {
            rect.anchorMin = new Vector2(0f, 0f); rect.anchorMax = new Vector2(0f, 0f); rect.pivot = new Vector2(0f, 0f); rect.anchoredPosition = position;
        }

        private void CreateButton(GameObject parent, string name, string chinese, string english, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 1f); rect.anchorMax = new Vector2(0.5f, 1f); rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(300f, 34f); rect.anchoredPosition = position;
            var image = go.GetComponent<Image>(); image.sprite = UiSprites.White; image.color = new Color(0.12f, 0.36f, 0.58f, 0.95f);
            var button = go.GetComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(onClick);
            var text = CreateText(go, "Label", chinese, 17, Vector2.zero, new Vector2(300f, 34f), TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one; text.rectTransform.pivot = new Vector2(0.5f, 0.5f); text.rectTransform.anchoredPosition = Vector2.zero;
            _buttonLabels[name] = new D5ButtonLabel(text, chinese, english);
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = EventSystem.current ?? FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null) eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)).GetComponent<EventSystem>();
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null) eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }
}
