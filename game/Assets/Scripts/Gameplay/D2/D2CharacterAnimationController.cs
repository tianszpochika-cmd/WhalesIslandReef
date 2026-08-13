// ============================================================
// D2CharacterAnimationController —— D2 角色动作体验层
//
// 当前项目的占位船员 FBX 没有随模型提供 AnimationClip，因此由编辑器管线
// 生成项目内可复用的 Unity AnimationClip/AnimatorController；运行时由 Animator
// 提供动作状态，再用确定性的平滑姿态层增强待机、走路、奔跑、跳跃、上下船表现。
// 以后替换为正式 Humanoid clips 时，D2CharacterAction 与输入接口保持不变。
// ============================================================

using System;
using UnityEngine;

namespace Whalebone.Gameplay.D2
{
    public enum D2CharacterAction
    {
        Idle,
        Walk,
        Run,
        Jump,
        Boarding,
        Disembarking,
    }

    /// <summary>
    /// 轻量角色动作控制器。Animator 负责正式状态语义，平滑姿态层负责占位模型的
    /// 步态、呼吸和上下船过渡；找不到骨骼时仍不让缺少动画资源阻塞玩法。
    /// </summary>
    public sealed class D2CharacterAnimationController : MonoBehaviour
    {
        [SerializeField] private float transitionDuration = 0.75f;
        [SerializeField] private float actionBlendDuration = 0.18f;
        [SerializeField] private float idleBreathHeight = 0.018f;
        [SerializeField] private float walkBobHeight = 0.035f;
        [SerializeField] private float runBobHeight = 0.055f;

        private sealed class BonePose
        {
            public readonly Transform Transform;
            public readonly Quaternion BaseRotation;

            public BonePose(Transform transform)
            {
                Transform = transform;
                BaseRotation = transform.localRotation;
            }
        }

        private D2CharacterController _character;
        private Transform _visualRoot;
        private Vector3 _visualRootBasePosition;
        private Quaternion _visualRootBaseRotation;
        private BonePose _leftUpperArm;
        private BonePose _rightUpperArm;
        private BonePose _leftUpperLeg;
        private BonePose _rightUpperLeg;
        private BonePose _leftLowerLeg;
        private BonePose _rightLowerLeg;
        private BonePose _leftFoot;
        private BonePose _rightFoot;
        private Animator _animator;
        private bool _formalAnimatorReady;
        private D2CharacterAction _lastAnimatorAction = (D2CharacterAction)(-1);
        private float _phase;
        private float _transitionRemaining;
        private D2CharacterAction _transitionAction;
        private D2CharacterAction _previousAction = D2CharacterAction.Idle;
        private float _actionBlend = 1f;
        private bool _actionInitialized;
        private int _actionTransitionCount;

        public D2CharacterAction Action { get; private set; } = D2CharacterAction.Idle;
        public int BoundBoneCount { get; private set; }
        public bool IsTransitioning => _transitionRemaining > 0f;
        public bool FormalAnimatorReady => _formalAnimatorReady;
        public bool NeutralPoseSampled { get; private set; }
        public float NeutralPoseError { get; private set; } = float.PositiveInfinity;
        public int ActionTransitionCount => _actionTransitionCount;
        public bool ProceduralPoseLayerReady => BoundBoneCount >= 4;

        private void Awake()
        {
            _character = GetComponent<D2CharacterController>();
            DiscoverVisualHierarchy();
            SetupFormalAnimator();
        }

        /// <summary>播放一次上下船过渡；输入控制仍由 D2ControlRouter 管理。</summary>
        public void PlayTransition(D2CharacterAction action)
        {
            if (action != D2CharacterAction.Boarding && action != D2CharacterAction.Disembarking)
                return;

            _transitionAction = action;
            _transitionRemaining = Mathf.Max(0.1f, transitionDuration);
            _phase = 0f;
        }

        private void LateUpdate()
        {
            float deltaTime = Time.timeScale <= 0f ? 0f : Time.deltaTime;
            if (deltaTime > 0f) _phase += deltaTime * PhaseSpeed();

            if (_transitionRemaining > 0f)
                _transitionRemaining -= deltaTime;

            UpdateAction(ResolveAction(), deltaTime);

            if (_formalAnimatorReady)
            {
                PlayAnimatorAction(Action);
            }

            ApplyPose(BuildBlendedPose());
            SampleNeutralPose();
        }

        private D2CharacterAction ResolveAction()
        {
            if (_transitionRemaining > 0f) return _transitionAction;
            // The presentation component can be attached one frame before the
            // D2 physics component. That is a binding gap, not a jump. Treating
            // it as Jump made newly-created characters briefly show an airborne
            // pose and made the same fallback look like it was floating.
            if (_character == null) return D2CharacterAction.Idle;
            if (_character.IsJumping ||
                (!_character.IsGrounded && _character.VerticalVelocity > 0.1f))
                return D2CharacterAction.Jump;
            if (_character.IsMoving) return _character.IsRunning ? D2CharacterAction.Run : D2CharacterAction.Walk;
            return D2CharacterAction.Idle;
        }

        private void UpdateAction(D2CharacterAction nextAction, float deltaTime)
        {
            if (!_actionInitialized)
            {
                Action = nextAction;
                _previousAction = nextAction;
                _actionInitialized = true;
                _actionBlend = 1f;
            }
            else if (nextAction != Action)
            {
                _previousAction = Action;
                Action = nextAction;
                _actionBlend = 0f;
                _actionTransitionCount++;
            }

            float blendStep = actionBlendDuration <= 0.001f ? 1f : deltaTime / actionBlendDuration;
            _actionBlend = Mathf.MoveTowards(_actionBlend, 1f, blendStep);
        }

        private void SampleNeutralPose()
        {
            if (NeutralPoseSampled || Action != D2CharacterAction.Idle || IsTransitioning || _actionBlend < 0.98f)
                return;

            NeutralPoseError = Mathf.Max(
                PoseError(_leftUpperLeg),
                PoseError(_rightUpperLeg),
                PoseError(_leftLowerLeg),
                PoseError(_rightLowerLeg),
                PoseError(_leftFoot),
                PoseError(_rightFoot));
            NeutralPoseSampled = true;
            Debug.Log("[D2/Animation] 中立姿态基线已采样：下肢最大偏差=" + NeutralPoseError.ToString("F1") + "°");
        }

        private static float PoseError(BonePose bone)
        {
            return bone == null || bone.Transform == null
                ? 0f
                : Quaternion.Angle(bone.Transform.localRotation, bone.BaseRotation);
        }

        private void DiscoverVisualHierarchy()
        {
            _visualRoot = transform.childCount == 0 ? transform : transform.GetChild(0);
            _visualRootBasePosition = _visualRoot.localPosition;
            _visualRootBaseRotation = _visualRoot.localRotation;

            Transform[] bones = GetComponentsInChildren<Transform>(true);
            _leftUpperArm = CreatePose(FindBone(bones, "left", "upperarm", "arm", "shoulder"));
            _rightUpperArm = CreatePose(FindBone(bones, "right", "upperarm", "arm", "shoulder"));
            _leftUpperLeg = CreatePose(FindBone(bones, "left", "upleg", "upperleg", "thigh", "femur"));
            _rightUpperLeg = CreatePose(FindBone(bones, "right", "upleg", "upperleg", "thigh", "femur"));
            _leftLowerLeg = CreatePose(FindBone(bones, "left", "lowerleg", "calf", "shin", "leg"));
            _rightLowerLeg = CreatePose(FindBone(bones, "right", "lowerleg", "calf", "shin", "leg"));
            _leftFoot = CreatePose(FindBone(bones, "left", "foot", "ankle"));
            _rightFoot = CreatePose(FindBone(bones, "right", "foot", "ankle"));

            BoundBoneCount = CountBoundBones();
            Debug.Log("[D2/Animation] 角色动作控制器已接线：骨骼=" + BoundBoneCount + "，Animator 状态 + 平滑姿态层");
        }

        private void SetupFormalAnimator()
        {
            var controller = Resources.Load<RuntimeAnimatorController>("D6/D6CrewAnimator");
            if (controller == null)
            {
                Debug.LogWarning("[D2/Animation] 未找到 D6CrewAnimator，继续使用程序化动作降级。");
                return;
            }

            _animator = GetComponent<Animator>();
            if (_animator == null) _animator = gameObject.AddComponent<Animator>();
            _animator.runtimeAnimatorController = controller;
            _animator.applyRootMotion = false;
            _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            _formalAnimatorReady = true;
            Debug.Log("[D2/Animation] Unity Animator 动作资源已加载：D6CrewAnimator");
        }

        private void PlayAnimatorAction(D2CharacterAction action)
        {
            if (!_formalAnimatorReady || _animator == null || !_animator.isActiveAndEnabled ||
                action == _lastAnimatorAction)
                return;

            if (_lastAnimatorAction == (D2CharacterAction)(-1))
                _animator.Play("D6_" + action, 0, 0f);
            else
                _animator.CrossFadeInFixedTime("D6_" + action, Mathf.Min(actionBlendDuration, 0.2f), 0, 0f);
            _lastAnimatorAction = action;
        }

        private float PhaseSpeed()
        {
            if (_character == null || !_character.IsMoving) return 1.6f;
            return _character.IsRunning ? 14f : 9f;
        }

        private struct PoseFrame
        {
            public float Bob;
            public float Lean;
            public float ArmSwing;
            public float LeftUpperLeg;
            public float RightUpperLeg;
            public float LeftLowerLeg;
            public float RightLowerLeg;
            public float LeftFoot;
            public float RightFoot;
        }

        private PoseFrame BuildBlendedPose()
        {
            PoseFrame from = BuildPose(_previousAction);
            PoseFrame to = BuildPose(Action);
            float blend = Mathf.SmoothStep(0f, 1f, _actionBlend);
            return LerpPose(from, to, blend);
        }

        private PoseFrame BuildPose(D2CharacterAction action)
        {
            var pose = new PoseFrame();

            switch (action)
            {
                case D2CharacterAction.Idle:
                    pose.Bob = Mathf.Sin(_phase) * idleBreathHeight;
                    pose.ArmSwing = Mathf.Sin(_phase * 0.75f) * 1.5f;
                    break;
                case D2CharacterAction.Walk:
                    ApplyLocomotionPose(ref pose, Mathf.Sin(_phase), 22f, 10f, 14f, walkBobHeight, 0f);
                    break;
                case D2CharacterAction.Run:
                    ApplyLocomotionPose(ref pose, Mathf.Sin(_phase), 34f, 18f, 25f, runBobHeight, 5f);
                    break;
                case D2CharacterAction.Jump:
                    pose.ArmSwing = -24f;
                    pose.LeftUpperLeg = -7f;
                    pose.RightUpperLeg = 7f;
                    pose.LeftLowerLeg = 17f;
                    pose.RightLowerLeg = 17f;
                    pose.LeftFoot = 5f;
                    pose.RightFoot = -5f;
                    break;
                case D2CharacterAction.Boarding:
                    ApplyBoardingPose(ref pose, false);
                    break;
                case D2CharacterAction.Disembarking:
                    ApplyBoardingPose(ref pose, true);
                    break;
            }

            return pose;
        }

        private static void ApplyLocomotionPose(ref PoseFrame pose, float gait, float legSwing,
            float kneeBend, float armSwing, float bobHeight, float lean)
        {
            pose.Bob = Mathf.Abs(gait) * bobHeight;
            pose.Lean = lean;
            pose.ArmSwing = -gait * armSwing;
            pose.LeftUpperLeg = -gait * legSwing;
            pose.RightUpperLeg = gait * legSwing;
            pose.LeftLowerLeg = Mathf.Max(0f, -gait) * kneeBend;
            pose.RightLowerLeg = Mathf.Max(0f, gait) * kneeBend;
            pose.LeftFoot = gait * 4f;
            pose.RightFoot = -gait * 4f;
        }

        private void ApplyBoardingPose(ref PoseFrame pose, bool disembarking)
        {
            float progress = Mathf.Clamp01(1f - _transitionRemaining / Mathf.Max(transitionDuration, 0.1f));
            float eased = progress * progress * (3f - 2f * progress);
            float direction = disembarking ? 1f : -1f;
            pose.Bob = Mathf.Sin(eased * Mathf.PI) * (disembarking ? 0.045f : 0.06f);
            pose.Lean = Mathf.Lerp(0f, disembarking ? 5f : -8f, eased);
            pose.ArmSwing = direction * Mathf.Lerp(0f, 28f, eased);
            pose.LeftUpperLeg = direction * Mathf.Lerp(0f, 12f, eased);
            pose.RightUpperLeg = -direction * Mathf.Lerp(0f, 12f, eased);
            pose.LeftLowerLeg = Mathf.Lerp(0f, 20f, eased);
            pose.RightLowerLeg = Mathf.Lerp(0f, 20f, eased);
            pose.LeftFoot = -direction * Mathf.Lerp(0f, 5f, eased);
            pose.RightFoot = direction * Mathf.Lerp(0f, 5f, eased);
        }

        private static PoseFrame LerpPose(PoseFrame from, PoseFrame to, float t)
        {
            return new PoseFrame
            {
                Bob = Mathf.Lerp(from.Bob, to.Bob, t),
                Lean = Mathf.Lerp(from.Lean, to.Lean, t),
                ArmSwing = Mathf.Lerp(from.ArmSwing, to.ArmSwing, t),
                LeftUpperLeg = Mathf.Lerp(from.LeftUpperLeg, to.LeftUpperLeg, t),
                RightUpperLeg = Mathf.Lerp(from.RightUpperLeg, to.RightUpperLeg, t),
                LeftLowerLeg = Mathf.Lerp(from.LeftLowerLeg, to.LeftLowerLeg, t),
                RightLowerLeg = Mathf.Lerp(from.RightLowerLeg, to.RightLowerLeg, t),
                LeftFoot = Mathf.Lerp(from.LeftFoot, to.LeftFoot, t),
                RightFoot = Mathf.Lerp(from.RightFoot, to.RightFoot, t)
            };
        }

        private void ApplyPose(PoseFrame pose)
        {

            if (_visualRoot != transform)
            {
                _visualRoot.localPosition = _visualRootBasePosition + Vector3.up * pose.Bob;
                _visualRoot.localRotation = _visualRootBaseRotation * Quaternion.AngleAxis(pose.Lean, Vector3.right);
            }

            SetBoneRotation(_leftUpperArm, pose.ArmSwing);
            SetBoneRotation(_rightUpperArm, -pose.ArmSwing);
            SetBoneRotation(_leftUpperLeg, pose.LeftUpperLeg);
            SetBoneRotation(_rightUpperLeg, pose.RightUpperLeg);
            SetBoneRotation(_leftLowerLeg, pose.LeftLowerLeg);
            SetBoneRotation(_rightLowerLeg, pose.RightLowerLeg);
            SetBoneRotation(_leftFoot, pose.LeftFoot);
            SetBoneRotation(_rightFoot, pose.RightFoot);
        }

        private static BonePose CreatePose(Transform transform)
        {
            return transform == null ? null : new BonePose(transform);
        }

        private int CountBoundBones()
        {
            int count = 0;
            if (_leftUpperArm != null) count++;
            if (_rightUpperArm != null) count++;
            if (_leftUpperLeg != null) count++;
            if (_rightUpperLeg != null) count++;
            if (_leftLowerLeg != null) count++;
            if (_rightLowerLeg != null) count++;
            if (_leftFoot != null) count++;
            if (_rightFoot != null) count++;
            return count;
        }

        private static void SetBoneRotation(BonePose bone, float angle)
        {
            if (bone == null || bone.Transform == null) return;
            bone.Transform.localRotation = bone.BaseRotation * Quaternion.AngleAxis(angle, Vector3.right);
        }

        private static Transform FindBone(Transform[] bones, string side, params string[] tokens)
        {
            Transform best = null;
            int bestScore = -1;
            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];
                if (bone == null) continue;
                string name = NormalizeName(bone.name);
                if (!HasSide(name, side)) continue;
                if (side == "left" && name.Contains("right")) continue;
                if (side == "right" && name.Contains("left")) continue;

                int score = 0;
                for (int tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
                {
                    string token = NormalizeName(tokens[tokenIndex]);
                    if (name.Contains(token)) score = Mathf.Max(score, token.Length);
                }

                // “leg” 不能把 upperleg 误认成 lowerleg。
                if (tokens.Length > 0 && tokens[tokens.Length - 1] == "leg" &&
                    (name.Contains("upper") || name.Contains("up")))
                    score = 0;

                if (score > bestScore)
                {
                    best = bone;
                    bestScore = score;
                }
            }
            return bestScore < 0 ? null : best;
        }

        private static bool HasSide(string name, string side)
        {
            if (name.Contains(side)) return true;
            string suffix = side == "left" ? "l" : "r";
            return name.EndsWith(suffix, StringComparison.Ordinal);
        }

        private static string NormalizeName(string value)
        {
            return (value ?? string.Empty).ToLowerInvariant()
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .Replace(":", string.Empty);
        }
    }
}
