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

        private float yaw;                 // 水平角（围绕目标，0 = 目标正后方）
        private Vector3 velocity;          // SmoothDamp 内部状态
        private float collisionDistance;
        private float collisionDistanceVelocity;
        private readonly RaycastHit[] cameraHits = new RaycastHit[32];
        private Transform fallbackTarget;  // 场景资源损坏时的临时观察锚点
        private Transform characterTarget;
        private Transform shipTarget;
        private bool shipControlled;

        public Transform Target { get => target; set => target = value; }
        public CameraViewMode ViewMode => viewMode;
        public int ViewModeCount => 3;
        public float ResolvedDistance => collisionDistance > 0f ? collisionDistance : distance;
        public bool IsObstructed { get; private set; }
        public int CollisionCheckCount { get; private set; }
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

        public void SetTargets(Transform character, Transform ship)
        {
            characterTarget = character;
            shipTarget = ship;
            RefreshTarget();
        }

        public void SetControlMode(bool isShipControlled)
        {
            shipControlled = isShipControlled;
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
                    target = shipTarget != null ? shipTarget : target;
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
                    distance = 10.5f;
                    pitch = 22f;
                    targetOffset = new Vector3(0f, 1.45f, 0f);
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
            HandleInput();
            ApplyPosition();
            ApplyLookRotation();
        }

        private void ApplyLookRotation()
        {
            // forward 永远 normalized；接近竖直时切换 worldUp，避免 LookRotation 退化。
            Vector3 forward = target.position + targetOffset - transform.position;
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
        private void HandleInput()
        {
            if (!D7ControlCoordinator.CameraInputAllowed) return;
            var mouse = Mouse.current;
            if (mouse != null && (mouse.rightButton.isPressed || mouse.middleButton.isPressed))
            {
                // Mouse delta 是本帧事件像素，不是每秒速度；按帧率再乘 deltaTime
                // 会让低帧率和 Game 视图焦点切换时镜头手感明显失真。
                Vector2 delta = Vector2.ClampMagnitude(mouse.delta.ReadValue(), 120f);
                yaw += delta.x * rotateSpeed * 0.02f;
                pitch -= delta.y * rotateSpeed * 0.02f;
            }

            // 手柄不依赖鼠标按键，右摇杆直接提供相对视角输入；保留死区，
            // 避免摇杆回中抖动，并继续复用 D7 的模态输入锁。
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 look = gamepad.rightStick.ReadValue();
                if (look.sqrMagnitude > 0.01f)
                {
                    yaw += look.x * rotateSpeed * Time.deltaTime;
                    pitch -= look.y * rotateSpeed * Time.deltaTime;
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
        }

        /// <summary>球坐标计算目标位置并平滑移动。</summary>
        private void ApplyPosition()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + targetOffset;
            Vector3 direction = rotation * Vector3.back;
            float safeDistance = ResolveSafeDistance(pivot, direction, distance);
            if (collisionDistance <= 0f) collisionDistance = safeDistance;
            collisionDistance = Mathf.SmoothDamp(collisionDistance, safeDistance,
                ref collisionDistanceVelocity, cameraCollisionSmoothTime);
            Vector3 desired = pivot + direction * collisionDistance;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
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
            return collider.transform == target || collider.transform.IsChildOf(target);
        }
    }
}
