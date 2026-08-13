// ============================================================
// D2RuntimeBootstrap —— InitialHarbor 的 D2 运行时组装入口
//
// 只在场景加载后创建一次。它把 D2 组件接线，但不拥有航行/好感/存档数值。
// 缺少 D1 占位资产时自动生成可见的玩家、船和海面，保证可玩闭环不被资源卡住。
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using Whalebone.Core;
using Whalebone.Agent;
using Whalebone.Gameplay.World;
using Whalebone.Gameplay.Harbor;
using Whalebone.Gameplay.D7;
using Whalebone.Gameplay.UI.Dialogue;
using Whalebone.Systems.D2;
using Whalebone.Systems.D16;
using Whalebone.Systems.Save;
using Whalebone.Gameplay.UI.D16;

namespace Whalebone.Gameplay.D2
{
    public sealed class D2RuntimeBootstrap : MonoBehaviour
    {
        private enum RuntimeBuildPhase
        {
            EnsureShip,
            EnsureCrewAnchor,
            EnsurePlayer,
            ConfigurePhysics,
            ConfigureControls,
            ConfigurePresentation,
            Complete,
        }

        private RuntimeBuildPhase _runtimeBuildPhase = RuntimeBuildPhase.EnsureShip;
        private GameObject _ship;
        private GameObject _npc;
        private GameObject _player;
        private Camera _camera;
        private D2CharacterController _character;
        private D2CharacterAnimationController _animation;
        private D2ShipController _shipController;
        private D2DialogueController _dialogue;
        private D2ControlRouter _router;
        private bool _runtimeReady;
        private float _runtimeBuildStartedAt;
        private float _phaseStartedAt;
        private bool _phaseTimerStarted;
        private RuntimeBuildPhase _timedPhase;

        public bool IsReady => _runtimeReady;
        public float RuntimeBuildElapsedMs { get; private set; }
        public bool RuntimePhaseTimingsComplete { get; private set; }
        public int CompletedRuntimePhaseCount { get; private set; }
        public float EnsureShipMs { get; private set; }
        public float EnsureCrewAnchorMs { get; private set; }
        public float EnsurePlayerMs { get; private set; }
        public float ConfigurePhysicsMs { get; private set; }
        public float ConfigureControlsMs { get; private set; }
        public float ConfigurePresentationMs { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D2RuntimeBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D2RuntimeBootstrap").AddComponent<D2RuntimeBootstrap>();
            Debug.Log("[D2] InitialHarbor 运行时入口已创建");
        }

        private void Start()
        {
            _runtimeBuildStartedAt = Time.realtimeSinceStartup;
            _runtimeBuildPhase = RuntimeBuildPhase.EnsureShip;
        }

        private void Update()
        {
            if (!_runtimeReady) TryBuildRuntime();
        }

        private void TryBuildRuntime()
        {
            BeginPhaseTiming();
            switch (_runtimeBuildPhase)
            {
                case RuntimeBuildPhase.EnsureShip:
                    _ship = EnsureShip();
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.EnsureCrewAnchor;
                    return;

                case RuntimeBuildPhase.EnsureCrewAnchor:
                    _npc = EnsureCrewAnchor(_ship);
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.EnsurePlayer;
                    return;

                case RuntimeBuildPhase.EnsurePlayer:
                    _player = EnsurePlayer(_ship);
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.ConfigurePhysics;
                    return;

                case RuntimeBuildPhase.ConfigurePhysics:
                    var harborCollision = gameObject.GetComponent<HarborCollisionController>();
                    if (harborCollision == null) harborCollision = gameObject.AddComponent<HarborCollisionController>();
                    harborCollision.Apply(_ship);
                    _camera = EnsureCamera();

                    _character = _player.GetComponent<D2CharacterController>();
                    if (_character == null) _character = _player.AddComponent<D2CharacterController>();
                    _character.InitializePhysics();
                    _animation = _player.GetComponent<D2CharacterAnimationController>();
                    if (_animation == null) _animation = _player.AddComponent<D2CharacterAnimationController>();

                    _shipController = _ship.GetComponent<D2ShipController>();
                    if (_shipController == null) _shipController = _ship.AddComponent<D2ShipController>();
                    _shipController.Initialize(_ship.transform.position, _ship.transform.eulerAngles.y);
                    if (Whalebone.Systems.Save.GameState.CurrentPlayer != null &&
                        Whalebone.Systems.Save.GameState.CurrentPlayer.Location != null &&
                        Whalebone.Systems.Save.GameState.CurrentPlayer.Location.Type == "open_sea")
                    {
                        // 即使存档来自 open_sea，回到当前港口时也要落在码头安全区，
                        // 不把角色放到船体碰撞盒边缘再等待首帧纠正。
                        _player.transform.position = _ship.transform.position - _ship.transform.forward * 6.5f +
                            Vector3.up * 1.1f;
                    }

                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.ConfigureControls;
                    return;

                case RuntimeBuildPhase.ConfigureControls:
                    _dialogue = gameObject.GetComponent<D2DialogueController>();
                    if (_dialogue == null) _dialogue = gameObject.AddComponent<D2DialogueController>();

                    _router = gameObject.GetComponent<D2ControlRouter>();
                    if (_router == null) _router = gameObject.AddComponent<D2ControlRouter>();
                    _router.Initialize(_player, _ship, _npc, _character, _shipController, _dialogue, _camera);

                    var dayNight = gameObject.GetComponent<DayNightController>();
                    if (dayNight == null) dayNight = gameObject.AddComponent<DayNightController>();
                    ApplySavedClock(dayNight);

                    _dialogue.ReplyReceived -= OnDialogueReply;
                    _dialogue.ReplyReceived += OnDialogueReply;
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.ConfigurePresentation;
                    return;

                case RuntimeBuildPhase.ConfigurePresentation:
                    var ocean = gameObject.GetComponent<OceanAdapter>();
                    if (ocean == null) ocean = gameObject.AddComponent<OceanAdapter>();

                    var cameraFollow = _camera.GetComponent<CameraFollow>();
                    if (cameraFollow == null) cameraFollow = _camera.gameObject.AddComponent<CameraFollow>();
                    cameraFollow.SetTargets(_player.transform, _ship.transform);
                    cameraFollow.SetControlMode(false);

                    var harborPresentation = gameObject.GetComponent<HarborPresentationController>();
                    if (harborPresentation == null) harborPresentation = gameObject.AddComponent<HarborPresentationController>();
                    harborPresentation.Apply();

                    var tutorial = gameObject.GetComponent<HarborTutorialController>();
                    if (tutorial == null) tutorial = gameObject.AddComponent<HarborTutorialController>();

                    var hint = gameObject.GetComponent<D2ControlHint>();
                    if (hint == null) hint = gameObject.AddComponent<D2ControlHint>();
                    hint.Initialize(_router);

                    _runtimeReady = true;
                    CompletePhaseTiming();
                    _runtimeBuildPhase = RuntimeBuildPhase.Complete;
                    RuntimeBuildElapsedMs = (Time.realtimeSinceStartup - _runtimeBuildStartedAt) * 1000f;
                    RuntimePhaseTimingsComplete = true;
                    Debug.Log("[D2] 运行时闭环已接线：人物物理、船、相机、海面、昼夜、对话");
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
                case RuntimeBuildPhase.EnsureShip:
                    EnsureShipMs = durationMs;
                    break;
                case RuntimeBuildPhase.EnsureCrewAnchor:
                    EnsureCrewAnchorMs = durationMs;
                    break;
                case RuntimeBuildPhase.EnsurePlayer:
                    EnsurePlayerMs = durationMs;
                    break;
                case RuntimeBuildPhase.ConfigurePhysics:
                    ConfigurePhysicsMs = durationMs;
                    break;
                case RuntimeBuildPhase.ConfigureControls:
                    ConfigureControlsMs = durationMs;
                    break;
                case RuntimeBuildPhase.ConfigurePresentation:
                    ConfigurePresentationMs = durationMs;
                    break;
            }
            CompletedRuntimePhaseCount++;
            _phaseTimerStarted = false;
        }

        public void SaveProgress()
        {
            var ship = FindFirstObjectByType<D2ShipController>(FindObjectsInactive.Include);
            var clock = GetComponent<DayNightController>();
            if (ship == null || clock == null) return;
            try
            {
                SaveSystem.SaveD2Runtime(ship.Snapshot, clock.Snapshot);
                Debug.Log("[D2/Save] 运行时快照已保存");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[D2/Save] 快照保存失败，保留上一次有效存档：" + ex.Message);
            }
        }

        private void OnDialogueReply(CrewAgent agent, AgentReply reply)
        {
            bool changed = CrewRelationSystem.ApplyAgentProposal(agent, reply);
            Debug.Log("[D2/Relation] " + agent.DisplayName + " 好感提议已裁决：" +
                      reply.AffinityChange + "，实际变化=" + changed);
            SaveProgress();
        }

        private static void ApplySavedClock(DayNightController dayNight)
        {
            var location = Whalebone.Systems.Save.GameState.CurrentPlayer == null
                ? null : Whalebone.Systems.Save.GameState.CurrentPlayer.Location;
            if (location == null) return;
            dayNight.ApplySnapshot(new DayNightSnapshot(location.DayIndex, location.NormalizedTime, false));
        }

        private static GameObject EnsureShip()
        {
            var ship = GameObject.Find("ph_ship_fishing");
            if (ship != null) return ship;

            ship = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ship.name = "ph_ship_fishing";
            ship.transform.position = new Vector3(0f, 0.6f, 0f);
            ship.transform.localScale = new Vector3(4.5f, 0.8f, 8f);
            ApplyColor(ship, new Color(0.42f, 0.22f, 0.09f));
            Debug.LogWarning("[D2] 缺少 ph_ship_fishing，已创建运行时占位船");
            return ship;
        }

        private static GameObject EnsureCrewAnchor(GameObject ship)
        {
            var crew = GameObject.Find("ph_crew_male");
            if (crew != null) return crew;

            crew = D2FallbackHumanoidBuilder.Create(
                "ph_crew_male",
                ship.transform.position + new Vector3(2.5f, 0.2f, 1f),
                new Color(0.48f, 0.26f, 0.12f));
            Debug.LogWarning("[D2] 缺少 ph_crew_male，已创建可驱动的程序化低模船员；这不是最终美术资产");
            return crew;
        }

        private static GameObject EnsurePlayer(GameObject ship)
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            var crew = GameObject.Find("ph_crew_male");

            // 旧版运行时可能已经留下蓝色 Capsule。不能因为对象同名就直接复用，
            // 否则重启后仍会把“降级外观”误当成玩家，场景里的人就只剩 NPC。
            if (player != null)
            {
                if (D2CharacterVisualProbe.HasCharacterVisual(player))
                {
                    Debug.Log("[D2] 已复用真人角色玩家：D2_PlayerAvatar");
                    return player;
                }

                if (crew != null && crew != player && D2CharacterVisualProbe.HasCharacterVisual(crew))
                {
                    var replacement = InstantiateCharacterPlayer(crew, ship);
                    // 先停用旧对象，避免同一帧内 D7 协调器通过 GameObject.Find
                    // 重新绑定到即将销毁的 Capsule。
                    player.SetActive(false);
                    Object.Destroy(player);
                    Debug.LogWarning("[D2] 检测到旧版 Capsule 玩家，已自动替换为场景船员模型：D2_PlayerAvatar");
                    return replacement;
                }

                var fallback = CreateFallbackPlayer(ship);
                player.SetActive(false);
                Object.Destroy(player);
                Debug.LogWarning("[D2] 未找到可替换的船员模型，已将旧版 Capsule 玩家替换为程序化低模人物");
                return fallback;
            }

            // D1 场景已经有一个可用的 Quaternius 船员模型。玩家应复用这份表现资产，
            // 而不是再次创建 Capsule；Capsule 只保留给素材确实缺失的降级路径。
            if (crew != null && D2CharacterVisualProbe.HasCharacterVisual(crew))
                return InstantiateCharacterPlayer(crew, ship);

            return CreateFallbackPlayer(ship);
        }

        private static GameObject CreateFallbackPlayer(GameObject ship)
        {
            var player = D2FallbackHumanoidBuilder.Create(
                "D2_PlayerAvatar",
                // 出生在 DockGround 的中央安全区，避免从船体/跳板接缝出生后
                // 被 CharacterController 当成台阶抬起；距离船仍在上船交互范围内。
                ship.transform.position + new Vector3(0f, 1.1f, -6.5f),
                new Color(0.12f, 0.34f, 0.62f));
            Debug.LogWarning("[D2] 缺少可用船员模型，已创建可驱动的程序化低模人物；正式角色资源接入后将自动替换");
            return player;
        }

        private static GameObject InstantiateCharacterPlayer(GameObject crew, GameObject ship)
        {
            var player = Object.Instantiate(crew);
            player.name = "D2_PlayerAvatar";
            player.transform.SetParent(null, true);
            // 出生在 DockGround 的中央安全区，避免与船体碰撞盒重叠；
            // InitializePhysics 会继续把人物吸附到明确的可行走面。
            player.transform.position = ship.transform.position + new Vector3(0f, 1.1f, -6.5f);
            player.transform.rotation = Quaternion.identity;
            Debug.Log("[D2] 玩家已绑定场景船员模型：ph_crew_male -> D2_PlayerAvatar");
            return player;
        }

        private static Camera EnsureCamera()
        {
            var camera = Camera.main;
            if (camera != null) return camera;
            camera = FindFirstObjectByType<Camera>();
            if (camera != null) return camera;

            var go = new GameObject("D2_MainCamera");
            camera = go.AddComponent<Camera>();
            camera.tag = "MainCamera";
            return camera;
        }

        private static void ApplyColor(GameObject go, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) return;
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;
        }
    }

    /// <summary>
    /// 统一识别真正的角色表现，避免根节点 MeshRenderer 既被误判为 Capsule，
    /// 又被另一层判定为真人。骨骼层可能没有 SkinnedMeshRenderer，因此同时检查
    /// Animator/骨骼层级；单个根节点的 Primitive 仍会被拒绝。
    /// </summary>
    public static class D2CharacterVisualProbe
    {
        public static bool HasCharacterVisual(GameObject source)
        {
            if (source == null) return false;
            if (source.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0)
                return true;

            foreach (var renderer in source.GetComponentsInChildren<MeshRenderer>(true))
                if (renderer.transform != source.transform)
                    return true;

            var transforms = source.GetComponentsInChildren<Transform>(true);
            bool hasCharacterHierarchy = transforms.Length >= 8 ||
                source.GetComponentInChildren<Animator>(true) != null;
            return hasCharacterHierarchy && source.GetComponent<MeshRenderer>() != null;
        }
    }

    public sealed class D2CharacterController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float runMultiplier = 1.65f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float deceleration = 24f;
        [SerializeField] private float jumpHeight = 1.35f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float visualTargetHeight = 1.8f;
        private bool _inputEnabled;
        private bool _isGrounded = true;
        private bool _isRunning;
        private float _verticalVelocity;
        private Vector3 _moveDirection;
        private Vector3 _planarVelocity;
        private CharacterController _body;
        private bool _collisionEnabled = true;
        private bool _jumpActive;
        private bool _nonWalkableCollision;
        private Vector3 _positionBeforeMove;
        private bool _wasGroundedBeforeMove;
        private bool _bodyConfigured;
        private float _visualGroundOffset;

        public bool IsGrounded => _isGrounded;
        public bool IsRunning => _isRunning;
        public bool IsMoving => _planarVelocity.sqrMagnitude > 0.0001f;
        public float CurrentSpeed => new Vector3(_planarVelocity.x, 0f, _planarVelocity.z).magnitude;
        public float SpeedNormalized => Mathf.Clamp01(CurrentSpeed / Mathf.Max(0.01f, moveSpeed * runMultiplier));
        public float VerticalVelocity => _verticalVelocity;
        public bool IsJumping => _jumpActive;
        public bool HasPhysicsBody => _body != null;
        public bool CollisionEnabled => _collisionEnabled && _body != null && _body.enabled;
        public float VisualWorldHeight { get; private set; }
        public float VisualWorldRadius { get; private set; }
        public float VisualScaleMultiplier { get; private set; } = 1f;

        private void Awake()
        {
            _body = GetComponent<CharacterController>();
            if (_body == null)
            {
                // Unity 会按当前 Transform 缩放校验 CharacterController 的默认 stepOffset。
                // 美术模型为了归一化尺寸通常不是 1 倍缩放，先用单位缩放创建组件，
                // 避免初始化阶段产生误导性的 Step Offset 控制台警告。
                var originalScale = transform.localScale;
                transform.localScale = Vector3.one;
                _body = gameObject.AddComponent<CharacterController>();
                _body.stepOffset = 0.05f;
                transform.localScale = originalScale;
            }
            DisableChildColliders();
            NormalizeVisualScale();
            ConfigurePhysicsBody();
            _isGrounded = _body.isGrounded;
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            _isRunning = false;
            _moveDirection = Vector3.zero;
            _planarVelocity = Vector3.zero;
            _verticalVelocity = 0f;
            _jumpActive = false;
            _isGrounded = _body == null || !_body.enabled || _body.isGrounded;
        }

        public void SetWorldPosition(Vector3 position)
        {
            MoveTransformWithoutController(position);
            _verticalVelocity = 0f;
            _jumpActive = false;
            _isGrounded = _body == null || !_body.enabled || _body.isGrounded;
        }

        public void InitializePhysics()
        {
            ConfigurePhysicsBody();
            if (_collisionEnabled && _body != null && _body.enabled)
                SnapToGround();
        }

        public void SetCollisionEnabled(bool enabled)
        {
            _collisionEnabled = enabled;
            if (_body == null) return;

            if (!enabled)
            {
                _body.enabled = false;
                _verticalVelocity = 0f;
                _planarVelocity = Vector3.zero;
                _jumpActive = false;
                _isGrounded = true;
                return;
            }

            ConfigurePhysicsBody();
            _body.enabled = true;
            _isGrounded = _body.isGrounded;
            SnapToGround();
        }

        public bool SnapToGround(float maxDistance = 8f)
        {
            if (!_collisionEnabled || _body == null || !_body.enabled) return false;

            float worldHeight = Mathf.Max(1f, _body.height * Mathf.Abs(transform.lossyScale.y));
            Vector3 origin = transform.position + Vector3.up * (worldHeight + 1.5f);
            var hits = Physics.RaycastAll(origin, Vector3.down, maxDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            RaycastHit bestHit = default(RaycastHit);
            bool found = false;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.collider == null || hit.collider == _body ||
                    hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform) ||
                    HarborCollisionController.IsNavigationBoundary(hit.collider))
                    continue;
                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestHit = hit;
                    found = true;
                }
            }

            if (!found) return false;

            float bottomOffset = (_body.center.y - _body.height * 0.5f) *
                Mathf.Abs(transform.lossyScale.y);
            var target = transform.position;
            target.y = bestHit.point.y - bottomOffset + 0.01f;
            MoveTransformWithoutController(target);
            _verticalVelocity = -2f;
            _isGrounded = true;
            Debug.Log("[D2/Physics] 玩家已落地：surface=" + bestHit.collider.name +
                      ", surfaceY=" + bestHit.point.y.ToString("F2") +
                      ", playerY=" + target.y.ToString("F2"));
            return true;
        }

        public bool TryJump()
        {
            if (!_inputEnabled || !_isGrounded || !_collisionEnabled || _body == null || !_body.enabled)
                return false;
            _verticalVelocity = Mathf.Sqrt(Mathf.Max(0.01f, jumpHeight * -2f * gravity));
            _jumpActive = true;
            _isGrounded = false;
            return true;
        }

        public void PlayTransition(D2CharacterAction action)
        {
            var animation = GetComponent<D2CharacterAnimationController>();
            if (animation != null) animation.PlayTransition(action);
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                _isRunning = false;
                _moveDirection = Vector3.zero;
                return;
            }

            if (!_collisionEnabled || _body == null || !_body.enabled)
            {
                _isRunning = false;
                _moveDirection = Vector3.zero;
                _planarVelocity = Vector3.zero;
                _isGrounded = true;
                return;
            }

            Vector2 input = Vector2.zero;
            if (_inputEnabled && D7ControlCoordinator.GameplayInputAllowed)
            {
                input = D16InputRouter.MoveAxis();
                if (D16InputRouter.WasPressedThisFrame(D16InputAction.Jump)) TryJump();
            }

            input = Vector2.ClampMagnitude(input, 1f);
            _moveDirection = ResolveWorldDirection(input);
            _isRunning = _moveDirection.sqrMagnitude > 0.0001f &&
                D16InputRouter.IsPressed(D16InputAction.Sprint);

            if (_moveDirection.sqrMagnitude > 0.0001f)
            {
                var targetRotation = Quaternion.LookRotation(_moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 12f * Time.deltaTime);
            }

            ApplyPhysicsMotion();
        }

        private Vector3 ResolveWorldDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.001f) return Vector3.zero;
            var camera = Camera.main;
            Vector3 forward = camera == null ? Vector3.forward : camera.transform.forward;
            Vector3 right = camera == null ? Vector3.right : camera.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward = forward.sqrMagnitude < 0.001f ? Vector3.forward : forward.normalized;
            right = right.sqrMagnitude < 0.001f ? Vector3.right : right.normalized;
            return (right * input.x + forward * input.y).normalized;
        }

        private void ApplyPhysicsMotion()
        {
            if (_body.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += gravity * Time.deltaTime;

            float speed = moveSpeed * (_isRunning ? runMultiplier : 1f);
            Vector3 targetPlanarVelocity = _moveDirection * speed;
            float response = targetPlanarVelocity.sqrMagnitude > 0.0001f ? acceleration : deceleration;
            _planarVelocity = Vector3.MoveTowards(_planarVelocity, targetPlanarVelocity,
                 Mathf.Max(0.1f, response) * Time.deltaTime);
            Vector3 velocity = _planarVelocity + Vector3.up * _verticalVelocity;
            _positionBeforeMove = transform.position;
            _wasGroundedBeforeMove = _isGrounded || _body.isGrounded;
            _nonWalkableCollision = false;
            CollisionFlags flags = _body.Move(velocity * Time.deltaTime);
            _isGrounded = _body.isGrounded || (flags & CollisionFlags.Below) != 0;
            if (_nonWalkableCollision && transform.position.y > _positionBeforeMove.y + 0.02f)
            {
                // 不可行走的船体、桶和木桩可以阻挡水平移动，但不能被 CharacterController
                // 当作台阶爬上去。恢复到碰撞前高度，再由可行走地面校准接触。
                var correctedPosition = transform.position;
                correctedPosition.y = _positionBeforeMove.y;
                MoveTransformWithoutController(correctedPosition);
                _isGrounded = _wasGroundedBeforeMove || TryRecoverWalkableGround();
                Debug.Log("[D2/Physics] 已阻止角色攀爬不可行走碰撞体：恢复碰撞前高度");
            }
            if (!_jumpActive && _wasGroundedBeforeMove && _verticalVelocity <= 0f)
            {
                // Unity 的自动 step 在不同根缩放/接缝碰撞下可能把胶囊体抬得
                // 高于真实可行走面；以可行走面射线作为最终裁决，保持脚底稳定。
                if (TryRecoverWalkableGround(1.2f)) _isGrounded = true;
            }
            if (!_isGrounded && !_jumpActive && _verticalVelocity <= 0f)
                _isGrounded = TryRecoverWalkableGround();
            if (_isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            if (_isGrounded) _jumpActive = false;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit == null || hit.collider == null || IsWalkableSurface(hit.collider))
                return;
            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                return;
            _nonWalkableCollision = true;
        }

        private bool TryRecoverWalkableGround(float maxDrop = 0.45f)
        {
            float scaleY = Mathf.Max(0.001f, Mathf.Abs(transform.lossyScale.y));
            float worldBodyHeight = _body.height * scaleY;
            float worldStepOffset = Mathf.Min(0.35f, worldBodyHeight * 0.25f);
            float bottomOffset = (_body.center.y - _body.height * 0.5f) * scaleY;
            float currentBottom = transform.position.y + bottomOffset;
            Vector3 origin = transform.position + Vector3.up * (worldBodyHeight + 0.25f);
            float maxDistance = worldBodyHeight + worldStepOffset +
                Mathf.Max(0.45f, maxDrop) + 0.35f;
            var hits = Physics.RaycastAll(origin, Vector3.down, maxDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            RaycastHit bestHit = default(RaycastHit);
            float bestDistance = float.MaxValue;
            bool found = false;
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (!IsWalkableSurface(hit.collider)) continue;
                float surfaceDelta = hit.point.y - currentBottom;
                if (surfaceDelta < -Mathf.Max(0.05f, maxDrop) ||
                    surfaceDelta > worldStepOffset + 0.05f)
                    continue;
                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestHit = hit;
                    found = true;
                }
            }

            if (!found) return false;

            float beforeY = transform.position.y;
            Vector3 target = transform.position;
            target.y = bestHit.point.y - bottomOffset + 0.01f;
            MoveTransformWithoutController(target);
            _verticalVelocity = -2f;
            if (Mathf.Abs(target.y - beforeY) > 0.08f)
                Debug.Log("[D2/Physics] 玩家已恢复可行走接地：surface=" + bestHit.collider.name +
                          ", surfaceY=" + bestHit.point.y.ToString("F2"));
            return true;
        }

        private static bool IsWalkableSurface(Collider collider)
        {
            if (collider == null) return false;
            return collider.name == "BeachGround" ||
                   collider.name == "DockGround" ||
                   collider.name == "GangplankGround";
        }

        private void ConfigurePhysicsBody()
        {
            if (_body == null || _bodyConfigured) return;

            // CharacterController 的原生胶囊体在根节点缩放和尺寸属性同时变化时，
            // 可能要到下一次 Move 才重建。先禁用、配置、再启用，避免首帧把玩家
            // 从正确地面高度向上弹出，造成移动动作被误判为 Jump。
            bool restoreEnabled = _body.enabled;
            _body.enabled = false;

            Bounds bounds;
            if (TryGetVisualBounds(out bounds))
            {
                VisualWorldHeight = bounds.size.y;
                VisualWorldRadius = Mathf.Max(bounds.extents.x, bounds.extents.z);
                float scaleX = Mathf.Max(0.001f, Mathf.Abs(transform.lossyScale.x));
                float scaleY = Mathf.Max(0.001f, Mathf.Abs(transform.lossyScale.y));
                float scaleZ = Mathf.Max(0.001f, Mathf.Abs(transform.lossyScale.z));
                float height = bounds.size.y / scaleY;
                float radius = Mathf.Max(bounds.extents.x / scaleX, bounds.extents.z / scaleZ);
                float centerY = (bounds.center.y - transform.position.y) / scaleY;
                _visualGroundOffset = (bounds.min.y - transform.position.y) / scaleY;

                _body.height = Mathf.Max(0.8f, height);
                _body.radius = Mathf.Clamp(radius, 0.18f, _body.height * 0.45f);
                _body.center = new Vector3(0f, centerY, 0f);
            }
            else
            {
                VisualWorldHeight = 0f;
                VisualWorldRadius = 0f;
                _visualGroundOffset = 0f;
                _body.height = 1.8f;
                _body.radius = 0.32f;
                _body.center = new Vector3(0f, 0.9f, 0f);
            }

            // CharacterController 的 height/stepOffset 都是相对根节点的局部单位，
            // 但港口台阶高度是世界单位。角色模型的根节点可能带有 0.14～0.30
            // 的缩放；直接写 0.35 会把实际世界步高压缩到约 0.05～0.11，
            // 玩家从抬高的跳板走向码头时就会丢失接地并被误判为 Jump。
            float worldScaleY = Mathf.Max(0.001f, Mathf.Abs(transform.lossyScale.y));
            float worldBodyHeight = _body.height * worldScaleY;
            float worldStepOffset = Mathf.Min(0.35f, worldBodyHeight * 0.25f);
            _body.stepOffset = Mathf.Clamp(worldStepOffset / worldScaleY,
                0.05f, _body.height * 0.25f);
            _body.slopeLimit = 50f;
            _body.skinWidth = Mathf.Clamp(_body.radius * 0.08f, 0.015f, 0.06f);
            _body.minMoveDistance = 0f;
            _body.detectCollisions = true;
            _bodyConfigured = true;
            _body.enabled = restoreEnabled;
            Physics.SyncTransforms();

            Debug.Log("[D2/Physics] 玩家碰撞体已配置：worldHeight=" + VisualWorldHeight.ToString("F2") +
                       ", worldRadius=" + VisualWorldRadius.ToString("F2") +
                       ", localHeight=" + _body.height.ToString("F2") +
                       ", radius=" + _body.radius.ToString("F2") +
                       ", worldStepOffset=" + ( _body.stepOffset * worldScaleY).ToString("F2") +
                       ", visualGroundOffset=" + _visualGroundOffset.ToString("F2"));
        }

        private void NormalizeVisualScale()
        {
            Bounds bounds;
            if (!TryGetVisualBounds(out bounds) || bounds.size.y < 0.01f)
            {
                VisualScaleMultiplier = 1f;
                return;
            }

            float beforeHeight = bounds.size.y;
            float targetHeight = Mathf.Clamp(visualTargetHeight, 1.2f, 2.4f);
            float multiplier = targetHeight / beforeHeight;
            if (Mathf.Abs(multiplier - 1f) > 0.01f)
            {
                // 物理根节点保持单位缩放，避免 CharacterController 的本地
                // height/center 在 FBX 根缩放下产生首帧纠正；缩放只转移到
                // 视觉子树，骨骼层级与动作控制接口不变。
                Vector3 visualScale = transform.localScale * multiplier;
                if (transform.childCount > 0)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        child.localScale = Vector3.Scale(child.localScale, visualScale);
                    }
                    transform.localScale = Vector3.one;
                }
                else
                {
                    // 极端降级对象没有视觉子树时保留根缩放，仍保证可见性。
                    transform.localScale = visualScale;
                }
            }

            VisualScaleMultiplier = multiplier;
            Debug.Log("[D2/Physics] 角色视觉尺寸已归一化：beforeWorldHeight=" +
                      beforeHeight.ToString("F2") + ", targetWorldHeight=" +
                      targetHeight.ToString("F2") + ", multiplier=" + multiplier.ToString("F3"));
        }

        private void DisableChildColliders()
        {
            foreach (var collider in GetComponentsInChildren<Collider>(true))
            {
                if (collider == _body) continue;
                collider.enabled = false;
            }
        }

        private void MoveTransformWithoutController(Vector3 position)
        {
            bool wasEnabled = _body != null && _body.enabled;
            if (wasEnabled) _body.enabled = false;
            transform.position = position;
            if (wasEnabled)
            {
                _body.enabled = true;
                Physics.SyncTransforms();
            }
        }

        private bool TryGetVisualBounds(out Bounds bounds)
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                bounds = default(Bounds);
                return false;
            }

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds.size.sqrMagnitude > 0.0001f;
        }
    }
}
