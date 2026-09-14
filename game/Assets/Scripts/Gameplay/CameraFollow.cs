// ============================================================
// CameraFollow —— 第三人称跟随相机占位（P1C 初始环境场景）
//
// 归属：Gameplay（表现层，见 DEVELOPMENT_STANDARDS §1）
// 职责：
//   1. 跟随目标（玩家的船），右键/中键拖动围绕目标旋转
//   2. 滚轮缩放距离；SmoothDamp 平滑跟随
//   3. 纯表现层占位：无任何逻辑系统依赖；D2 引入 Cinemachine 后替换
//
// 说明：
//   - 输入走新版 Input System（工程 activeInputHandler=Both，向下兼容）
//   - 未指定 Target 时使用临时观察锚点并告警（场景搭建期容错）
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.D7;

namespace Whalebone.Gameplay
{
    public enum CameraViewMode
    {
        Character,
        ShipDeck,
        Overview
    }

    /// <summary>
    /// 第三人称跟随相机：围绕目标旋转 + 滚轮缩放 + 三种可切换视角。
    /// 场景搭建阶段使用，后续由 Cinemachine 相机接管。
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("目标")]
        [SerializeField] private Transform target;                 // 跟随目标（玩家船）
        [SerializeField] private Vector3 targetOffset = Vector3.up * 1.2f; // 注视点抬高（看向船中部）

        [Header("视角参数")]
        [SerializeField] private float distance = 9f;              // 初始观察距离
        [SerializeField] private float minDistance = 3f;           // 最近
        [SerializeField] private float maxDistance = 25f;          // 最远
        [SerializeField] private float pitch = 18f;                // 初始俯仰角（度）
        [SerializeField] private float rotateSpeed = 90f;          // 拖动旋转速度（度/秒）
        [SerializeField] private float zoomSpeed = 4f;             // 滚轮缩放系数
        [SerializeField] private float smoothTime = 0.12f;         // 位置平滑时间
        [SerializeField] private CameraViewMode viewMode = CameraViewMode.Character;
        [SerializeField] private float cameraCollisionRadius = 0.22f;
        [SerializeField] private float cameraCollisionPadding = 0.28f;
        [SerializeField] private float cameraCollisionSmoothTime = 0.08f;
        [SerializeField] private LayerMask cameraCollisionMask = Physics.DefaultRaycastLayers;

        // ShipDeck is a readable gameplay presentation, not a distant harbor
        // establishing shot. The previous 10.5m preset made the boarded
        // fallback character collapse into the mast/sail silhouette, so the
        // player could not tell who was operating the helm. Keep the player
        // as the target and bring the controlled-ship framing in without
        // changing ship physics, ownership, or boarding anchors.
        // The R2 Player screenshot kept the helm operator and wheel readable
        // only as a small central silhouette while the surrounding port filled
        // the frame.  The ship-control route is presentation-only, so tighten
        // this preset around the already-authoritative player target without
        // changing ship physics, the D7 helm anchor, or camera collision rules.
        private const float ShipDeckPresentationDistance = 5.5f;
        private const float ShipDeckPresentationPitch = 20f;
        private static readonly Vector3 ShipDeckPresentationTargetOffset = new Vector3(0f, 1.25f, 0f);
        private const float SwimmingPresentationDistance = 4.6f;
        private const float SwimmingPresentationPitch = 20f;
        private static readonly Vector3 SwimmingPresentationTargetOffset = new Vector3(0f, 0.42f, 0f);
        private const float InitialHarborCompositionDistance = 18.5f;
        private float yaw;                 // 水平角（围绕目标，0 = 目标正后方）
        private Vector3 velocity;          // SmoothDamp 内部状态
        private float collisionDistance;
        private float collisionDistanceVelocity;
        private readonly RaycastHit[] cameraHits = new RaycastHit[32];
        private Transform fallbackTarget;  // 场景资源损坏时的临时观察锚点
        private Transform characterTarget;
        private Transform shipTarget;
        private bool shipControlled;
        private bool swimmingPresentationActive;
        private bool swimmingPresentationAutoYaw;
        private bool snapPositionOnNextUpdate;

        public Transform Target { get => target; set => target = value; }
        public CameraViewMode ViewMode => viewMode;
        public int ViewModeCount => 3;
        public float ResolvedDistance => collisionDistance > 0f ? collisionDistance : distance;
        public bool IsObstructed { get; private set; }
        public int CollisionCheckCount { get; private set; }
        public bool SwimmingPresentationActive => swimmingPresentationActive;
        public bool SwimmingPresentationAutoYaw => swimmingPresentationAutoYaw;
        public string ViewModeLabel
        {
            get
            {
                switch (viewMode)
                {
                    case CameraViewMode.ShipDeck: return "船上视角";
                    case CameraViewMode.Overview: return "港口总览";
                    default: return "人物视角";
                }
            }
        }

        private void Awake()
        {
            // 场景 YAML 或失效 Prefab 可能留下近似但未归一化的旋转；URP 会在相机裁剪阶段直接报错。
            transform.rotation = NormalizeRotation(transform.rotation);
        }

        private void Start()
        {
            ApplyViewPreset();
            ApplyInitialHarborComposition();
            collisionDistance = distance;
            if (target == null)
            {
                // 占位场景约定：自动绑定名为 ph_ship_fishing 的对象（场景内组件无需序列化引用实例）
                var ship = GameObject.Find("ph_ship_fishing");
                if (ship != null) target = ship.transform;
            }
            if (target == null)
            {
                // 允许玩家继续检查相机输入，同时明确报告场景资产问题；资源修复后会自动使用真实船只。
                var fallback = new GameObject("CameraTarget_Fallback");
                fallback.hideFlags = HideFlags.HideAndDontSave;
                fallback.transform.position = Vector3.zero;
                fallbackTarget = fallback.transform;
                target = fallbackTarget;
                Debug.LogError("[CameraFollow] 找不到玩家船 ph_ship_fishing，当前使用临时观察锚点。请重新运行 Tools > Whalebone > Install Harbor Art。");
            }
            // 初始摆位：目标后上方，避免启动瞬间跳变
            ApplyPosition();
            ApplyLookRotation();
        }

        private void ApplyInitialHarborComposition()
        {
            // InitialHarbor opens on the character route, but the character
            // preset was authored as a close-up debug view.  A slightly wider
            // first frame keeps the player, berth, shoreline and first
            // landmark readable without changing the normal F3 view contract.
            if (SceneManager.GetActiveScene().name != "InitialHarbor" ||
                viewMode != CameraViewMode.Character || shipControlled)
                return;

            // The four R03 service buildings sit roughly 6.4m to either side
            // of the authored player spawn.  The A16 Player frame showed that
            // the 13.5m composition still places their 5m-wide facades on the
            // near horizontal frustum edges, where they read as cropped and
            // visually intersecting the route.  Use the measured wider
            // opening distance only for this first presentation frame; the
            // legacy roots are already renderer-quarantined, and normal
            // zoom and F3 camera control remain unchanged afterward, so the
            // player and berth remain inside the camera frustum.
            distance = Mathf.Clamp(InitialHarborCompositionDistance, minDistance, maxDistance);
            pitch = 22f;
            // The authored player spawn is inland of the berth.  The former
            // yaw=0 composition placed the camera on the sea side, with the
            // visible ship directly between the camera and the player; the
            // first Player frame therefore looked like a ship-interior clip.
            // Start from the inland side while preserving normal F3/mouse
            // camera control after the first frame.
            yaw = 180f;
            targetOffset = new Vector3(0f, 1.35f, 0f);
        }

        public void SetTargets(Transform character, Transform ship)
        {
            characterTarget = character;
            shipTarget = ship;
            RefreshTarget();
        }

        public void SetControlMode(bool isShipControlled)
        {
            bool modeChanged = shipControlled != isShipControlled;
            shipControlled = isShipControlled;

            // 角色上船后仍沿用 Character 预设会把注视点放在舵位人物身上，
            // 相机很容易被桅杆、船舷或附近码头挡住。进入船控时自动切到
            // 船上预设，退出船控时恢复人物预设；玩家仍可用 F3 手动循环
            // 三种视角。只改变表现，不改变 D2 的控制权或航行事实。
            if (modeChanged)
            {
                if (isShipControlled && viewMode == CameraViewMode.Character)
                    SetViewMode(CameraViewMode.ShipDeck);
                else if (!isShipControlled && viewMode == CameraViewMode.ShipDeck)
                    SetViewMode(CameraViewMode.Character);
            }
            RefreshTarget();
        }

        public void CycleViewMode()
        {
            SetViewMode((CameraViewMode)(((int)viewMode + 1) % ViewModeCount));
        }

        public void SetViewMode(CameraViewMode mode)
        {
            viewMode = mode;
            ApplyViewPreset();
            collisionDistance = distance;
            collisionDistanceVelocity = 0f;
            RefreshTarget();
            velocity = Vector3.zero;
            if (target != null)
            {
                ApplyPosition();
                ApplyLookRotation();
            }
            Debug.Log("[Camera] 视角切换：" + ViewModeLabel);
        }

        private void OnDestroy()
        {
            if (fallbackTarget != null)
                Destroy(fallbackTarget.gameObject);
        }

        private void RefreshTarget()
        {
            switch (viewMode)
            {
                case CameraViewMode.ShipDeck:
                    // Keep the player as the visual subject while the ship is
                    // controlled.  The old ship-root target put the camera's
                    // pivot behind the helm, so the hull/sail occluded the
                    // boarded player even though the state and parent were
                    // correct.  The ship remains an excluded self-collision
                    // target in IsTargetCollider; only the framing subject
                    // changes, preserving the D7 control contract.
                    target = characterTarget != null ? characterTarget :
                        shipTarget != null ? shipTarget : target;
                    break;
                case CameraViewMode.Overview:
                    target = shipControlled && shipTarget != null
                        ? shipTarget
                        : characterTarget != null ? characterTarget : target;
                    break;
                default:
                    target = characterTarget != null ? characterTarget : target;
                    break;
            }
        }

        private void ApplyViewPreset()
        {
            switch (viewMode)
            {
                case CameraViewMode.ShipDeck:
                    distance = ShipDeckPresentationDistance;
                    pitch = ShipDeckPresentationPitch;
                    targetOffset = ShipDeckPresentationTargetOffset;
                    break;
                case CameraViewMode.Overview:
                    distance = 16f;
                    pitch = 38f;
                    targetOffset = new Vector3(0f, 0.6f, 0f);
                    break;
                default:
                    distance = 6.5f;
                    pitch = 16f;
                    targetOffset = new Vector3(0f, 1.1f, 0f);
                    break;
            }
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            RefreshSwimmingPresentation();
            bool manualLookInput = HandleInput();
            if (swimmingPresentationActive && swimmingPresentationAutoYaw)
            {
                if (manualLookInput)
                {
                    // A real mouse/right-stick look is the explicit handoff
                    // from the automatic swimmer framing to player-owned
                    // camera control.  Keep that handoff stable until the
                    // swimmer leaves the water instead of snapping back every
                    // frame after one look sample.
                    swimmingPresentationAutoYaw = false;
                }
                else if (characterTarget != null)
                {
                    // D2 rotates the physics root toward the current swimming
                    // direction.  The one-shot entry yaw used to become a
                    // rear view as soon as W/S changed that heading, so the
                    // prone fallback was projected as an upright silhouette.
                    // Follow the current heading only while the automatic
                    // presentation handoff is still active; this is camera
                    // presentation only and does not write D2 state.
                    float desiredYaw = characterTarget.eulerAngles.y + 90f;
                    if (Mathf.Abs(Mathf.DeltaAngle(yaw, desiredYaw)) > 1f)
                        snapPositionOnNextUpdate = true;
                    yaw = desiredYaw;
                }
            }
            if (!IsFinite(yaw)) yaw = 0f;
            if (!IsFinite(pitch)) pitch = 16f;
            ApplyPosition();
            ApplyLookRotation();
        }

        private void RefreshSwimmingPresentation()
        {
            var characterController = characterTarget == null
                ? null
                : characterTarget.GetComponent<D2CharacterController>();
            bool shouldUseSwimmingPresentation = viewMode == CameraViewMode.Character &&
                characterController != null && characterController.IsSwimming;
            if (shouldUseSwimmingPresentation == swimmingPresentationActive) return;

            swimmingPresentationActive = shouldUseSwimmingPresentation;
            if (swimmingPresentationActive)
            {
                // The normal character preset looks above a swimmer: the
                // 1.1m target offset leaves the body at the bottom edge and
                // makes the fallback read as a detached ring.  Keep this in
                // the camera presentation layer so water physics and D2
                // ownership remain unchanged.
                distance = SwimmingPresentationDistance;
                pitch = SwimmingPresentationPitch;
                targetOffset = SwimmingPresentationTargetOffset;
                // A prone fallback viewed directly from behind collapses its
                // forward axis into the head/torso silhouette.  A bounded
                // side-biased entry angle exposes the body length and arms;
                // normal mouse/gamepad camera input can still take over in
                // the following frames.
                if (characterTarget != null)
                    yaw = characterTarget.eulerAngles.y + 90f;
                swimmingPresentationAutoYaw = true;
                // The swimmer framing is a presentation contract, not a
                // gradual camera orbit.  A smoothed move can leave the camera
                // behind the old rear-facing composition for several frames,
                // which makes a rotated swimmer read as upright/back-facing.
                // Snap the first swim frame and any meaningful automatic heading
                // change; manual camera motion and normal follow smoothing remain
                // unchanged afterwards.
                snapPositionOnNextUpdate = true;
            }
            else
            {
                swimmingPresentationAutoYaw = false;
                ApplyViewPreset();
            }

            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            collisionDistance = distance;
            collisionDistanceVelocity = 0f;
            velocity = Vector3.zero;
        }

        private void ApplyLookRotation()
        {
            // forward 永远 normalized；接近竖直时切换 worldUp，避免 LookRotation 退化。
            Vector3 forward = target.position + targetOffset - transform.position;
            if (!IsFinite(forward)) return;
            if (forward.sqrMagnitude < 0.0001f) return;
            Vector3 fwd = forward.normalized;
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(fwd, up)) > 0.999f)
            {
                Vector3 alt = Vector3.Cross(fwd, Vector3.forward);
                if (alt.sqrMagnitude < 0.01f) alt = Vector3.Cross(fwd, Vector3.right);
                up = alt.normalized;
            }
            transform.rotation = NormalizeRotation(Quaternion.LookRotation(fwd, up));
        }

        private static Quaternion NormalizeRotation(Quaternion value)
        {
            float lengthSquared = value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w;
            if (float.IsNaN(lengthSquared) || float.IsInfinity(lengthSquared) || lengthSquared < 0.000001f)
                return Quaternion.identity;

            float inverseLength = 1f / Mathf.Sqrt(lengthSquared);
            return new Quaternion(
                value.x * inverseLength,
                value.y * inverseLength,
                value.z * inverseLength,
                value.w * inverseLength);
        }

        /// <summary>读取输入：鼠标右键/中键拖动、手柄右摇杆旋转，滚轮缩放。</summary>
        private bool HandleInput()
        {
            if (!D7ControlCoordinator.CameraInputAllowed) return false;
            bool lookInputObserved = false;
            var mouse = Mouse.current;
            if (mouse != null && (mouse.rightButton.isPressed || mouse.middleButton.isPressed))
            {
                // Mouse delta 是本帧事件像素，不是每秒速度；按帧率再乘 deltaTime
                // 会让低帧率和 Game 视图焦点切换时镜头手感明显失真。
                Vector2 delta = Vector2.ClampMagnitude(mouse.delta.ReadValue(), 120f);
                if (IsFinite(delta))
                {
                    yaw += delta.x * rotateSpeed * 0.02f;
                    pitch -= delta.y * rotateSpeed * 0.02f;
                    lookInputObserved = delta.sqrMagnitude > 0.001f;
                }
            }

            // 手柄不依赖鼠标按键，右摇杆直接提供相对视角输入；保留死区，
            // 避免摇杆回中抖动，并继续复用 D7 的模态输入锁。
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 look = gamepad.rightStick.ReadValue();
                if (IsFinite(look) && look.sqrMagnitude > 0.01f)
                {
                    yaw += look.x * rotateSpeed * Time.deltaTime;
                    pitch -= look.y * rotateSpeed * Time.deltaTime;
                    lookInputObserved = true;
                }
            }

            pitch = Mathf.Clamp(pitch, 5f, 80f);

            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.0001f)
                {
                    // 鼠标滚轮是事件增量，不是每秒速度；不乘 Time.deltaTime，避免缩放几乎无感。
                    // 不同鼠标的滚轮单位不同，0.01f 将常见的 120/格与触控板小增量统一到可用范围。
                    distance = Mathf.Clamp(distance - scroll * zoomSpeed * 0.01f, minDistance, maxDistance);
                }
            }

            return lookInputObserved;
        }

        /// <summary>球坐标计算目标位置并平滑移动。</summary>
        private void ApplyPosition()
        {
            if (!IsFinite(target.position) || !IsFinite(targetOffset) ||
                !IsFinite(yaw) || !IsFinite(pitch) || !IsFinite(distance))
                return;

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + targetOffset;
            Vector3 direction = rotation * Vector3.back;
            if (!IsFinite(pivot) || !IsFinite(direction)) return;
            float safeDistance = ResolveSafeDistance(pivot, direction, distance);
            if (!IsFinite(safeDistance))
                safeDistance = Mathf.Clamp(distance, minDistance, maxDistance);
            if (collisionDistance <= 0f) collisionDistance = safeDistance;
            if (!IsFinite(collisionDistance))
            {
                collisionDistance = safeDistance;
                collisionDistanceVelocity = 0f;
            }
            collisionDistance = Mathf.SmoothDamp(collisionDistance, safeDistance,
                ref collisionDistanceVelocity, cameraCollisionSmoothTime);
            Vector3 desired = pivot + direction * collisionDistance;
            if (!IsFinite(desired) || !IsFinite(transform.position)) return;
            if (snapPositionOnNextUpdate)
            {
                transform.position = desired;
                velocity = Vector3.zero;
                snapPositionOnNextUpdate = false;
                return;
            }
            Vector3 nextPosition = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            if (IsFinite(nextPosition)) transform.position = nextPosition;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private float ResolveSafeDistance(Vector3 pivot, Vector3 direction, float requestedDistance)
        {
            CollisionCheckCount++;
            IsObstructed = false;
            int hitCount = Physics.SphereCastNonAlloc(pivot, Mathf.Max(0.05f, cameraCollisionRadius), direction,
                cameraHits, requestedDistance, cameraCollisionMask, QueryTriggerInteraction.Ignore);
            float safeDistance = requestedDistance;
            float nearestHit = requestedDistance;
            for (int i = 0; i < hitCount; i++)
            {
                var hit = cameraHits[i];
                if (hit.collider == null || IsTargetCollider(hit.collider)) continue;
                if (hit.distance < nearestHit) nearestHit = hit.distance;
            }

            if (nearestHit < requestedDistance - 0.01f)
            {
                IsObstructed = true;
                // 允许镜头靠近目标避开遮挡，但保留近裁剪面的安全余量。
                safeDistance = Mathf.Clamp(nearestHit - cameraCollisionPadding, 1.05f, requestedDistance);
            }
            return safeDistance;
        }

        private bool IsTargetCollider(Collider collider)
        {
            if (target == null || collider == null) return false;
            if (collider.transform == target || collider.transform.IsChildOf(target))
                return true;

            // 上船后 Character 视角仍然以玩家骨骼为注视目标，但玩家已经
            // 成为船的子物体。船体碰撞体此时不再是 character 的子节点；
            // 如果把它当成外部遮挡物，SphereCast 会把相机压进船舷/桅杆，
            // 表现为登船后镜头贴脸、穿模或只看到一块木板。船本身属于
            // 当前受控目标，必须和 target 子树一样从相机自碰撞中排除。
            if (shipControlled && shipTarget != null &&
                (collider.transform == shipTarget || collider.transform.IsChildOf(shipTarget)))
                return true;

            return false;
        }
    }
}
