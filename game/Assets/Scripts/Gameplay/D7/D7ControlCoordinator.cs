// ============================================================
// D7ControlCoordinator —— 首航体验的唯一输入状态协调器
//
// 只协调表现层输入权限，不修改任何航行、经济、船员或存档数值。
// D2/D3/D5 仍各自拥有业务入口，但必须先尊重这里计算出的模态锁。
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Whalebone.Gameplay;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.D5;
using Whalebone.Gameplay.Harbor;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.UI.Dialogue;
using Whalebone.Systems.D2;
using Whalebone.Systems.D7;

namespace Whalebone.Gameplay.D7
{
    /// <summary>
    /// InitialHarbor 的输入状态协调器。用高优先级脚本先处理模态关闭，
    /// 再让旧版本 Gameplay 入口读取统一的权限结果。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class D7ControlCoordinator : MonoBehaviour
    {
        public static D7ControlCoordinator Instance { get; private set; }
        public static bool EscapeHandledThisFrame { get; private set; }

        /// <summary>没有协调器时保持旧逻辑可运行；协调器存在后统一阻止模态穿透。</summary>
        public static bool GameplayInputAllowed => Instance == null || Instance.Snapshot.GameplayInputAllowed;
        public static bool CameraInputAllowed => Instance == null || Instance.Snapshot.CameraInputAllowed;

        private readonly D7ControlStateMachine _state = new D7ControlStateMachine();
        private D2ControlRouter _router;
        private D2DialogueController _dialogue;
        private D3RuntimeBootstrap _d3;
        private HarborPauseController _pause;
        private CameraFollow _cameraFollow;
        private D7ShipDeckController _shipDeck;
        private D7CharacterPresentationController _characterPresentation;
        private D7RuntimeStatusHud _statusHud;
        private D7AudioController _audio;
        private D7MaterialPolishController _materials;
        private bool _bound;
        private D7ControlSnapshot _lastSnapshot;
        private float _nextReconcile;

        public bool IsReady => _bound;
        public D7ControlSnapshot Snapshot => _state.Snapshot;
        public string AuthorityLabel => _state.AuthorityLabel;
        public string ModalLabel => _state.ModalLabel;
        public int ReconcileCount { get; private set; }
        public int ModalTransitionCount { get; private set; }
        public D7ShipDeckController ShipDeck => _shipDeck;
        public D7CharacterPresentationController CharacterPresentation => _characterPresentation;
        public D7AudioController Audio => _audio;
        public D7MaterialPolishController Materials => _materials;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D7ControlCoordinator>(FindObjectsInactive.Include) != null) return;
            new GameObject("D7ControlCoordinator").AddComponent<D7ControlCoordinator>();
            Debug.Log("[D7/Input] 首航体验控制协调器已创建");
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _lastSnapshot = _state.Snapshot;
        }

        private void Update()
        {
            EscapeHandledThisFrame = false;
            if (SceneManager.GetActiveScene().name != "InitialHarbor") return;

            BindRuntimeObjects();
            HandleModalEscape();
            if (Time.unscaledTime >= _nextReconcile)
            {
                _nextReconcile = Time.unscaledTime + 0.1f;
                ReconcileState();
            }
        }

        private void BindRuntimeObjects()
        {
            if (_router == null) _router = FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            if (_dialogue == null) _dialogue = FindFirstObjectByType<D2DialogueController>(FindObjectsInactive.Include);
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_pause == null) _pause = FindFirstObjectByType<HarborPauseController>(FindObjectsInactive.Include);

            var camera = Camera.main;
            if (camera == null) camera = FindFirstObjectByType<Camera>();
            if (_cameraFollow == null && camera != null)
                _cameraFollow = camera.GetComponent<CameraFollow>();

            var ship = GameObject.Find("ph_ship_fishing");
            var player = GameObject.Find("D2_PlayerAvatar");
            if (ship != null && player != null)
            {
                if (_characterPresentation == null)
                {
                    _characterPresentation = player.GetComponent<D7CharacterPresentationController>();
                    if (_characterPresentation == null)
                        _characterPresentation = player.AddComponent<D7CharacterPresentationController>();
                }
                if (_shipDeck == null)
                {
                    _shipDeck = ship.GetComponent<D7ShipDeckController>();
                    if (_shipDeck == null) _shipDeck = ship.AddComponent<D7ShipDeckController>();
                }
                _shipDeck.Initialize(ship.transform, player.transform, _router);
            }

            if (_statusHud == null)
            {
                _statusHud = GetComponent<D7RuntimeStatusHud>();
                if (_statusHud == null) _statusHud = gameObject.AddComponent<D7RuntimeStatusHud>();
            }
            if (_audio == null)
            {
                _audio = GetComponent<D7AudioController>();
                if (_audio == null) _audio = gameObject.AddComponent<D7AudioController>();
            }
            if (_materials == null)
            {
                _materials = GetComponent<D7MaterialPolishController>();
                if (_materials == null) _materials = gameObject.AddComponent<D7MaterialPolishController>();
            }

            _bound = _router != null && player != null && ship != null;
        }

        private void HandleModalEscape()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

            // 模态窗口优先被关闭；本帧不再让暂停菜单重复响应同一个 ESC。
            if (_dialogue != null && _dialogue.IsOpen)
            {
                _dialogue.Close();
                EscapeHandledThisFrame = true;
                Debug.Log("[D7/Input] ESC 优先关闭对话，不穿透到人物控制");
                return;
            }

            if (_d3 != null && _d3.IsTradeOpen)
            {
                _d3.CloseTradeFromCoordinator();
                EscapeHandledThisFrame = true;
                Debug.Log("[D7/Input] ESC 优先关闭贸易，不穿透到暂停菜单");
            }
        }

        private void ReconcileState()
        {
            D7ModalBlock modal = D7ModalBlock.None;
            if (!Application.isFocused && !Application.isBatchMode)
                modal = D7ModalBlock.FocusLost;
            else if (_pause != null && _pause.IsPaused)
                modal = D7ModalBlock.Pause;
            else if (_dialogue != null && _dialogue.IsOpen)
                modal = D7ModalBlock.Dialogue;
            else if (_d3 != null && _d3.IsTradeOpen)
                modal = D7ModalBlock.Trade;

            D7ControlAuthority authority = D7ControlAuthority.Character;
            if (_router != null)
            {
                authority = _router.Mode == D2ControlMode.Ship
                    ? D7ControlAuthority.Ship
                    : _router.Mode == D2ControlMode.Locked
                        ? D7ControlAuthority.Locked
                        : D7ControlAuthority.Character;
            }

            _state.SetAuthority(authority);
            _state.SetModal(modal);
            var current = _state.Snapshot;
            if (current.Authority != _lastSnapshot.Authority || current.Modal != _lastSnapshot.Modal)
            {
                if (current.Modal != _lastSnapshot.Modal) ModalTransitionCount++;
                Debug.Log("[D7/Input] 状态=" + _state.AuthorityLabel + "，模态=" + _state.ModalLabel);
                _lastSnapshot = current;
            }
            ReconcileCount++;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (hasFocus) _nextReconcile = 0f;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            EscapeHandledThisFrame = false;
        }
    }
}
