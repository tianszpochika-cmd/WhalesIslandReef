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
//   - 未指定 Target 时自动停用并告警（场景搭建期容错）
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;

namespace Whalebone.Gameplay
{
    /// <summary>
    /// 第三人称跟随相机占位：围绕目标旋转 + 滚轮缩放 + 平滑跟随。
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

        private float yaw;                 // 水平角（围绕目标，0 = 目标正后方）
        private Vector3 velocity;          // SmoothDamp 内部状态

        public Transform Target { get => target; set => target = value; }

        private void Start()
        {
            if (target == null)
            {
                // 占位场景约定：自动绑定名为 ph_ship_fishing 的对象（场景内组件无需序列化引用实例）
                var ship = GameObject.Find("ph_ship_fishing");
                if (ship != null) target = ship.transform;
            }
            if (target == null)
            {
                Debug.LogWarning("[CameraFollow] 未指定跟随目标，相机停用。请在场景中将 Target 指向玩家船（或存在名为 ph_ship_fishing 的对象）。");
                enabled = false;
                return;
            }
            // 初始摆位：目标后上方，避免启动瞬间跳变
            ApplyPosition();
            transform.LookAt(target.position + targetOffset);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            HandleInput();
            ApplyPosition();
            // 修复 P1C 累积旋转导致 QuaternionToEuler 警告刷屏（2026-08-08 统筹）：
            // （1）forward 永远 normalized（2）当 forward 与 up 接近平行时切换 worldUp 避免 LookRotation 退化
            // （3）结果 quaternion 强制 normalize（Unity 6 internal 严格检查）
            Vector3 forward = target.position + targetOffset - transform.position;
            if (forward.sqrMagnitude < 0.0001f) return;
            Vector3 fwd = forward.normalized;
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(fwd, up)) > 0.999f)
            {
                // 接近退化：切换一个不与 fwd 平行的 worldUp（保证 LookRotation 稳定）
                Vector3 alt = Vector3.Cross(fwd, Vector3.forward);
                if (alt.sqrMagnitude < 0.01f) alt = Vector3.Cross(fwd, Vector3.right);
                up = alt.normalized;
            }
            transform.rotation = Quaternion.LookRotation(fwd, up).normalized;
        }

        /// <summary>读取输入：右键/中键拖动旋转，滚轮缩放。</summary>
        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return; // 无鼠标设备（纯触屏/手柄环境）时保持静止

            if (mouse.rightButton.isPressed || mouse.middleButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * rotateSpeed * Time.deltaTime;
                pitch -= delta.y * rotateSpeed * Time.deltaTime;
            }
            pitch = Mathf.Clamp(pitch, 5f, 80f);

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                distance = Mathf.Clamp(distance - scroll * zoomSpeed * Time.deltaTime, minDistance, maxDistance);
            }
        }

        /// <summary>球坐标计算目标位置并平滑移动。</summary>
        private void ApplyPosition()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desired = target.position + targetOffset + rotation * new Vector3(0f, 0f, -distance);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }
    }
}
