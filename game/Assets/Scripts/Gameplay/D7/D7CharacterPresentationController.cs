// ============================================================
// D7CharacterPresentationController —— 角色视觉落地与朝向稳定层
//
// CharacterController 负责真实碰撞，Animator 负责动作；本类只把视觉
// 网格底部校准到碰撞体底部，避免模型脚底悬空或动作曲线把角色抬离地面。
// ============================================================

using UnityEngine;
using Whalebone.Gameplay.D2;

namespace Whalebone.Gameplay.D7
{
    [DefaultExecutionOrder(180)]
    public sealed class D7CharacterPresentationController : MonoBehaviour
    {
        private Transform _visualRoot;
        private CharacterController _body;
        private bool _calibrated;
        private float _nextCalibration;
        private bool _loggedReady;

        public bool IsReady { get; private set; }
        public bool HasHumanVisual { get; private set; }
        public float GroundError { get; private set; }
        public int CalibrationCount { get; private set; }

        private void Awake()
        {
            TryBindRuntimeParts();
        }

        private void LateUpdate()
        {
            // D7ControlCoordinator 的高优先级 Update 可能会在 D2RuntimeBootstrap
            // 为玩家添加 CharacterController 的同一帧先创建本组件。不要把 Awake
            // 时的瞬时缺失永久缓存成“未就绪”，否则表现层不会校准脚底和动作。
            if (!IsReady) TryBindRuntimeParts();
            if (!IsReady || _body == null || !_body.enabled || !_body.isGrounded ||
                Time.unscaledTime < _nextCalibration) return;
            _nextCalibration = Time.unscaledTime + 0.18f;
            CalibrateVisualGrounding();
        }

        private void TryBindRuntimeParts()
        {
            if (_body == null) _body = GetComponent<CharacterController>();
            if (_visualRoot == null)
                _visualRoot = transform.childCount == 0 ? transform : transform.GetChild(0);

            HasHumanVisual = D2CharacterVisualProbe.HasCharacterVisual(gameObject);
            bool wasReady = IsReady;
            IsReady = _body != null && HasHumanVisual;
            if (!wasReady && IsReady && !_loggedReady)
            {
                _loggedReady = true;
                Debug.Log("[D7/Character] 角色表现组件已完成运行时绑定：CharacterController=" +
                          (_body != null) + ", humanVisual=" + HasHumanVisual);
            }
        }

        private void CalibrateVisualGrounding()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            Vector3 bodyBottom = transform.TransformPoint(_body.center + Vector3.down * (_body.height * 0.5f));
            GroundError = bounds.min.y - bodyBottom.y;

            // 只有明显悬空/穿地才修正，保留 Animator 的轻微呼吸和步态 bob。
            if (!_calibrated || Mathf.Abs(GroundError) > 0.08f)
            {
                Vector3 correction = Vector3.down * GroundError;
                _visualRoot.localPosition += transform.InverseTransformVector(correction);
                CalibrationCount++;
                _calibrated = true;
                Debug.Log("[D7/Character] 视觉落地已校准：error=" + GroundError.ToString("F3") +
                          "，calibrations=" + CalibrationCount);
            }
        }
    }
}
