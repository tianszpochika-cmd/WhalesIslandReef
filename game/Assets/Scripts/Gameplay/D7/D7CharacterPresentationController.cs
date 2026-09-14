// ============================================================
// D7CharacterPresentationController —— 角色视觉落地与朝向稳定层
//
// CharacterController 负责真实碰撞，D2 动作控制器负责视觉根提交；本类只
// 测量视觉网格与碰撞体的落地误差，并把校准偏移交给 D2，避免第二个组件
// 在 LateUpdate 直接改写同一个 visualRoot。
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
        private D2CharacterController _character;
        private D2CharacterAnimationController _animation;
        private bool _calibrated;
        private float _nextCalibration;
        private bool _loggedReady;

        public bool IsReady { get; private set; }
        public bool HasHumanVisual { get; private set; }
        public float GroundError { get; private set; }
        public int CalibrationCount { get; private set; }
        public bool UsesAnimationGroundingContract => _animation != null;
        public int DirectVisualRootWriteCount { get; private set; }

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
            if (_animation == null) _animation = GetComponent<D2CharacterAnimationController>();
            if (_character == null) _character = GetComponent<D2CharacterController>();
            // A CharacterController can still report a stale physical contact
            // for one frame after D2 has entered water. Ground calibration in
            // that frame adds a land offset to the swimming presentation and
            // makes the Player appear to float or split at the shoreline.
            if (!IsReady || _body == null || !_body.enabled || !_body.isGrounded ||
                (_character != null && _character.IsSwimming))
            {
                _animation?.ClearVisualGroundingOffset();
                return;
            }
            if (Time.unscaledTime < _nextCalibration) return;
            _nextCalibration = Time.unscaledTime + 0.18f;
            CalibrateVisualGrounding();
        }

        private void TryBindRuntimeParts()
        {
            if (_body == null) _body = GetComponent<CharacterController>();
            if (_character == null) _character = GetComponent<D2CharacterController>();
            if (_animation == null) _animation = GetComponent<D2CharacterAnimationController>();
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
            // D7 只负责测量；D2 未就绪时等待下一帧绑定，绝不直接写 visualRoot。
            if (_animation == null || (_calibrated && Mathf.Abs(GroundError) <= 0.08f))
                return;

            Vector3 correction = Vector3.down * GroundError;
            // D7 owns the measurement, while D2 owns the visual-root
            // Transform write. This keeps grounding and action pose in
            // one deterministic submission instead of accumulating a
            // second localPosition writer in LateUpdate.
            _animation.SetVisualGroundingOffsetWorld(correction);
            CalibrationCount++;
            _calibrated = true;
            Debug.Log("[D7/Character] 视觉落地已校准：error=" + GroundError.ToString("F3") +
                      "，calibrations=" + CalibrationCount);
        }

        private void OnDisable()
        {
            _animation?.ClearVisualGroundingOffset();
        }
    }
}
