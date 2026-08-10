// ============================================================
// D2RuntimeBootstrap —— InitialHarbor 的 D2 运行时组装入口
//
// 只在场景加载后创建一次。它把 D2 组件接线，但不拥有航行/好感/存档数值。
// 缺少 D1 占位资产时自动生成可见的玩家、船和海面，保证可玩闭环不被资源卡住。
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Whalebone.Core;
using Whalebone.Agent;
using Whalebone.Gameplay.World;
using Whalebone.Gameplay.Harbor;
using Whalebone.Gameplay.D7;
using Whalebone.Gameplay.UI.Dialogue;
using Whalebone.Systems.D2;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D2
{
    public sealed class D2RuntimeBootstrap : MonoBehaviour
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
            if (FindFirstObjectByType<D2RuntimeBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D2RuntimeBootstrap").AddComponent<D2RuntimeBootstrap>();
            Debug.Log("[D2] InitialHarbor 运行时入口已创建");
        }

        private void Start()
        {
            BuildRuntime();
        }

        private void BuildRuntime()
        {
            var ship = EnsureShip();
            var npc = EnsureCrewAnchor(ship);
            var player = EnsurePlayer(ship);
            var harborCollision = gameObject.GetComponent<HarborCollisionController>();
            if (harborCollision == null) harborCollision = gameObject.AddComponent<HarborCollisionController>();
            harborCollision.Apply(ship);
            var camera = EnsureCamera();

            var character = player.GetComponent<D2CharacterController>();
            if (character == null) character = player.AddComponent<D2CharacterController>();
            character.InitializePhysics();
            var animation = player.GetComponent<D2CharacterAnimationController>();
            if (animation == null) animation = player.AddComponent<D2CharacterAnimationController>();

            var shipController = ship.GetComponent<D2ShipController>();
            if (shipController == null) shipController = ship.AddComponent<D2ShipController>();
            shipController.Initialize(ship.transform.position, ship.transform.eulerAngles.y);
            if (Whalebone.Systems.Save.GameState.CurrentPlayer != null &&
                Whalebone.Systems.Save.GameState.CurrentPlayer.Location != null &&
                Whalebone.Systems.Save.GameState.CurrentPlayer.Location.Type == "open_sea")
            {
                player.transform.position = ship.transform.position - ship.transform.forward * 5f + Vector3.up;
            }

            var dialogue = gameObject.GetComponent<D2DialogueController>();
            if (dialogue == null) dialogue = gameObject.AddComponent<D2DialogueController>();

            var router = gameObject.GetComponent<D2ControlRouter>();
            if (router == null) router = gameObject.AddComponent<D2ControlRouter>();
            router.Initialize(player, ship, npc, character, shipController, dialogue, camera);

            var dayNight = gameObject.GetComponent<DayNightController>();
            if (dayNight == null) dayNight = gameObject.AddComponent<DayNightController>();
            ApplySavedClock(dayNight);

            dialogue.ReplyReceived -= OnDialogueReply;
            dialogue.ReplyReceived += OnDialogueReply;

            var ocean = gameObject.GetComponent<OceanAdapter>();
            if (ocean == null) ocean = gameObject.AddComponent<OceanAdapter>();

            var cameraFollow = camera.GetComponent<CameraFollow>();
            if (cameraFollow == null) cameraFollow = camera.gameObject.AddComponent<CameraFollow>();
            cameraFollow.SetTargets(player.transform, ship.transform);
            cameraFollow.SetControlMode(false);

            var harborPresentation = gameObject.GetComponent<HarborPresentationController>();
            if (harborPresentation == null) harborPresentation = gameObject.AddComponent<HarborPresentationController>();
            harborPresentation.Apply();

            var tutorial = gameObject.GetComponent<HarborTutorialController>();
            if (tutorial == null) tutorial = gameObject.AddComponent<HarborTutorialController>();

            var hint = gameObject.GetComponent<D2ControlHint>();
            if (hint == null) hint = gameObject.AddComponent<D2ControlHint>();
            hint.Initialize(router);

            Debug.Log("[D2] 运行时闭环已接线：人物物理、船、相机、海面、昼夜、对话");
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

            crew = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            crew.name = "ph_crew_male";
            crew.transform.position = ship.transform.position + new Vector3(2.5f, 1f, 1f);
            crew.transform.localScale = new Vector3(0.55f, 1f, 0.55f);
            ApplyColor(crew, new Color(0.65f, 0.42f, 0.2f));
            Debug.LogWarning("[D2] 缺少 ph_crew_male，已创建老水手占位人物");
            return crew;
        }

        private static GameObject EnsurePlayer(GameObject ship)
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            if (player != null) return player;

            // D1 场景已经有一个可用的 Quaternius 船员模型。玩家应复用这份表现资产，
            // 而不是再次创建 Capsule；Capsule 只保留给素材确实缺失的降级路径。
            var crew = GameObject.Find("ph_crew_male");
            if (crew != null && HasCharacterVisual(crew))
            {
                player = Object.Instantiate(crew);
                player.name = "D2_PlayerAvatar";
                player.transform.SetParent(null, true);
                player.transform.position = ship.transform.position + new Vector3(0f, 1.1f, -5f);
                player.transform.rotation = Quaternion.identity;
                Debug.Log("[D2] 玩家已绑定场景船员模型：ph_crew_male -> D2_PlayerAvatar");
                return player;
            }

            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "D2_PlayerAvatar";
            player.transform.position = ship.transform.position + new Vector3(0f, 1f, -5f);
            player.transform.localScale = new Vector3(0.55f, 1f, 0.55f);
            ApplyColor(player, new Color(0.12f, 0.55f, 0.85f));
            Debug.LogWarning("[D2] 缺少可用船员模型，玩家暂时使用 Capsule 降级外观");
            return player;
        }

        private static bool HasCharacterVisual(GameObject source)
        {
            if (source.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0)
                return true;

            // 导入的角色通常把 MeshRenderer 放在子节点；单个根节点 MeshRenderer
            // 更可能是运行时生成的 Capsule，因此不把它误认成角色模型。
            foreach (var renderer in source.GetComponentsInChildren<MeshRenderer>(true))
                if (renderer.transform != source.transform)
                    return true;

            return false;
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

    public sealed class D2CharacterController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float runMultiplier = 1.65f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float deceleration = 24f;
        [SerializeField] private float jumpHeight = 1.35f;
        [SerializeField] private float gravity = -18f;
        private bool _inputEnabled;
        private bool _isGrounded = true;
        private bool _isRunning;
        private float _verticalVelocity;
        private Vector3 _moveDirection;
        private Vector3 _planarVelocity;
        private CharacterController _body;
        private bool _collisionEnabled = true;
        private bool _bodyConfigured;
        private float _visualGroundOffset;

        public bool IsGrounded => _isGrounded;
        public bool IsRunning => _isRunning;
        public bool IsMoving => _planarVelocity.sqrMagnitude > 0.0001f;
        public float CurrentSpeed => new Vector3(_planarVelocity.x, 0f, _planarVelocity.z).magnitude;
        public float SpeedNormalized => Mathf.Clamp01(CurrentSpeed / Mathf.Max(0.01f, moveSpeed * runMultiplier));
        public float VerticalVelocity => _verticalVelocity;
        public bool HasPhysicsBody => _body != null;
        public bool CollisionEnabled => _collisionEnabled && _body != null && _body.enabled;

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
            _isGrounded = _body == null || !_body.enabled || _body.isGrounded;
        }

        public void SetWorldPosition(Vector3 position)
        {
            MoveTransformWithoutController(position);
            _verticalVelocity = 0f;
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

            var keyboard = Keyboard.current;
            Vector2 input = Vector2.zero;
            if (_inputEnabled && keyboard != null && D7ControlCoordinator.GameplayInputAllowed)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
                if (keyboard.spaceKey.wasPressedThisFrame) TryJump();
            }

            input = Vector2.ClampMagnitude(input, 1f);
            _moveDirection = ResolveWorldDirection(input);
            _isRunning = _moveDirection.sqrMagnitude > 0.0001f && keyboard != null &&
                (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);

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
            CollisionFlags flags = _body.Move(velocity * Time.deltaTime);
            _isGrounded = _body.isGrounded || (flags & CollisionFlags.Below) != 0;
            if (_isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
        }

        private void ConfigurePhysicsBody()
        {
            if (_body == null || _bodyConfigured) return;

            Bounds bounds;
            if (TryGetVisualBounds(out bounds))
            {
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
                _visualGroundOffset = 0f;
                _body.height = 1.8f;
                _body.radius = 0.32f;
                _body.center = new Vector3(0f, 0.9f, 0f);
            }

            _body.stepOffset = Mathf.Min(0.35f, _body.height * 0.25f);
            _body.slopeLimit = 50f;
            _body.skinWidth = Mathf.Clamp(_body.radius * 0.08f, 0.015f, 0.06f);
            _body.minMoveDistance = 0f;
            _body.detectCollisions = true;
            _bodyConfigured = true;

            Debug.Log("[D2/Physics] 玩家碰撞体已配置：height=" + _body.height.ToString("F2") +
                      ", radius=" + _body.radius.ToString("F2") +
                      ", visualGroundOffset=" + _visualGroundOffset.ToString("F2"));
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
            if (wasEnabled) _body.enabled = true;
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
