// ============================================================
// D2CharacterAnimationController —— D2 角色动作体验层
//
// 当前项目的占位船员 FBX 没有随模型提供 AnimationClip，因此由编辑器管线
// 生成项目内可复用的 Unity AnimationClip/AnimatorController。Animator 与程序化
// 姿态层是互斥的表现驱动源；两者不能在同一帧无条件写同一批 Transform。
// 以后替换为正式 Humanoid clips 时，D2CharacterAction 与输入接口保持不变。
// ============================================================

using System;
using System.Collections.Generic;
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
        Swimming,
    }

    /// <summary>
    /// 轻量角色动作控制器。移动控制器拥有实体根的位置、朝向、接地和跳跃；本类
    /// 只消费这些状态并驱动视觉子树。Animator、程序化姿态和 neutral 回退严格
    /// 互斥，找不到可信骨骼时仍不让缺少动画资源阻塞玩法。
    /// </summary>
    public sealed class D2CharacterAnimationController : MonoBehaviour
    {
        // The fallback visual root is pitched around X before the bone layer is
        // submitted.  Keep the swimmer reach as an explicit local-axis
        // contract: a hanging -Y arm needs a 180-degree combined local-X
        // rotation to follow the head/torso direction after the root pitch.
        // These constants apply only to the generated fallback, never to an
        // imported skinned rig without a verified Avatar/axis contract.
        private const float ProceduralSwimmingPitchDegrees = 64f;
        private const float ProceduralSwimmingUpperArmReachDegrees = 150f;
        private const float ProceduralSwimmingForearmReachDegrees = 60f;
        private const float ProceduralSwimmingFootStrokeDegrees = 4f;

        [SerializeField] private float transitionDuration = 0.75f;
        [SerializeField] private float actionBlendDuration = 0.18f;

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

        private enum PoseDriverMode
        {
            Neutral,
            Animator,
            Procedural,
        }

        private D2CharacterController _character;
        private bool _shipControlActive;
        private Transform _visualRoot;
        private Vector3 _visualRootBasePosition;
        private Quaternion _visualRootBaseRotation;
        private BonePose _leftUpperArm;
        private BonePose _rightUpperArm;
        private BonePose _leftLowerArm;
        private BonePose _rightLowerArm;
        private BonePose _leftUpperLeg;
        private BonePose _rightUpperLeg;
        private BonePose _leftLowerLeg;
        private BonePose _rightLowerLeg;
        private BonePose _leftFoot;
        private BonePose _rightFoot;
        private Animator _animator;
        private bool _formalAnimatorReady;
        private bool _proceduralRigReady;
        private bool _importedSkinnedRigNeutralOnly;
        private bool _boneBindingConflict;
        private bool _boundBonesAreUnique;
        private PoseDriverMode _poseDriverMode = PoseDriverMode.Neutral;
        private D2CharacterAction _lastAnimatorAction = (D2CharacterAction)(-1);
        private float _phase;
        private float _transitionRemaining;
        private D2CharacterAction _transitionAction;
        private D2CharacterAction _previousAction = D2CharacterAction.Idle;
        private float _actionBlend = 1f;
        private bool _actionInitialized;
        private int _actionTransitionCount;
        private Vector3 _visualGroundingOffsetLocal;

        public D2CharacterAction Action { get; private set; } = D2CharacterAction.Idle;
        public int BoundBoneCount { get; private set; }
        public bool IsTransitioning => _transitionRemaining > 0f;
        public bool FormalAnimatorReady => _formalAnimatorReady;
        public bool NeutralPoseSampled { get; private set; }
        public float NeutralPoseError { get; private set; } = float.PositiveInfinity;
        public int ActionTransitionCount => _actionTransitionCount;
        /// <summary>
        /// 表示是否具备完整的程序化骨骼能力；Animator 接管时该能力不会被使用。
        /// </summary>
        public bool ProceduralPoseLayerReady => _proceduralRigReady;
        public bool ProceduralPoseLayerActive => _poseDriverMode == PoseDriverMode.Procedural;
        /// <summary>
        /// Presentation-only fallback state for the real ship-control camera.
        /// Formal imported rigs remain neutral until their Avatar/axis contract
        /// is verified; only the complete procedural fallback may place both
        /// arms toward the visible helm.
        /// </summary>
        public bool ShipSteeringPoseActive => _shipControlActive &&
            _poseDriverMode == PoseDriverMode.Procedural;
        /// <summary>
        /// Optional forearm layer used only by the explicit procedural fallback.
        /// It lets the upper arm stay inside the safe local-axis envelope while
        /// the forearm reaches forward during swimming.
        /// </summary>
        public bool SwimLowerArmPoseLayerReady => _leftLowerArm != null && _rightLowerArm != null;
        /// <summary>
        /// Runtime diagnostics for the bounded procedural swimming contract.
        /// These values are written from the same pose submission that drives
        /// the visible fallback, so tests do not infer posture from a screenshot.
        /// </summary>
        public float SwimmingVisualPitchDegrees { get; private set; }
        public float SwimmingVisualRollDegrees { get; private set; }
        public float SwimmingVisualBodyDrop { get; private set; }
        public bool AnimatorPoseLayerActive => _poseDriverMode == PoseDriverMode.Animator &&
            _animator != null && _animator.isActiveAndEnabled;
        public bool BoneBindingConflict => _boneBindingConflict;
        public bool BoundBonesAreUnique => _boundBonesAreUnique;
        /// <summary>
        /// 当前角色使用导入的蒙皮骨骼，但还没有经过正式动作轴/Avatar 契约
        /// 验证。此类角色只允许保持导入的 neutral 姿态，避免用局部 X 轴
        /// 猜测驱动导致四肢扭曲；已验证的程序化回退人物不走此分支。
        /// </summary>
        public bool ImportedSkinnedRigNeutralOnly => _importedSkinnedRigNeutralOnly;
        /// <summary>
        /// 导入蒙皮仍禁止猜测式骨骼旋转，但允许动作层只对完整视觉根做
        /// 校准过的整体倾斜/浮动。这样游泳、跳跃和上下船不会再次触发
        /// FBX 局部轴扭曲，同时玩家不会在水里保持完全直立的行走外观。
        /// </summary>
        public bool ImportedVisualActionMotionActive =>
            _importedSkinnedRigNeutralOnly && _visualRoot != null && _visualRoot != transform;
        public string PoseDriverModeName => _poseDriverMode.ToString();
        public Vector3 VisualGroundingOffsetLocal => _visualGroundingOffsetLocal;

        private void Awake()
        {
            _character = GetComponent<D2CharacterController>();
            DiscoverVisualHierarchy();
            SetupFormalAnimator();
        }

        private void OnEnable()
        {
            // A pooled/re-enabled presentation component must start from the
            // imported baseline.  Otherwise the previous transition phase can
            // survive a UI/modal or scene handoff and look like a twisted pose.
            _phase = 0f;
            _transitionRemaining = 0f;
            _actionInitialized = false;
            _actionBlend = 1f;
            _previousAction = D2CharacterAction.Idle;
            Action = D2CharacterAction.Idle;
            _lastAnimatorAction = (D2CharacterAction)(-1);
            _visualGroundingOffsetLocal = Vector3.zero;
            _shipControlActive = false;
            SwimmingVisualPitchDegrees = 0f;
            SwimmingVisualRollDegrees = 0f;
            SwimmingVisualBodyDrop = 0f;
            NeutralPoseSampled = false;
            NeutralPoseError = float.PositiveInfinity;
            ResetVisualPoseToBaseline();
        }

        private void OnDisable()
        {
            _transitionRemaining = 0f;
            _visualGroundingOffsetLocal = Vector3.zero;
            _shipControlActive = false;
            SwimmingVisualPitchDegrees = 0f;
            SwimmingVisualRollDegrees = 0f;
            SwimmingVisualBodyDrop = 0f;
            ResetVisualPoseToBaseline();
        }

        /// <summary>
        /// D2ControlRouter owns the authoritative mode transition. It notifies
        /// this component once so the fallback presentation can pose both arms
        /// at the visible helm without searching for or writing control state.
        /// </summary>
        public void SetShipControlActive(bool active)
        {
            _shipControlActive = active;
        }

        /// <summary>
        /// D7 owns the ground measurement, but D2 owns the visual-root write.
        /// Keeping this handoff as an offset prevents a second component from
        /// competing with the action pose on the same Transform.
        /// </summary>
        public void SetVisualGroundingOffsetWorld(Vector3 worldOffset)
        {
            _visualGroundingOffsetLocal = transform.InverseTransformVector(worldOffset);
        }

        public void ClearVisualGroundingOffset()
        {
            _visualGroundingOffsetLocal = Vector3.zero;
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

            if (_poseDriverMode == PoseDriverMode.Animator)
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
            if (_character.IsSwimming) return D2CharacterAction.Swimming;
            if (_character.IsJumping ||
                (!_character.IsGrounded && _character.VerticalVelocity > 0.1f))
                return D2CharacterAction.Jump;
            if (_character.IsMoving) return _character.IsRunning ? D2CharacterAction.Run : D2CharacterAction.Walk;
            return D2CharacterAction.Idle;
        }

        private void UpdateAction(D2CharacterAction nextAction, float deltaTime)
        {
            if (nextAction == D2CharacterAction.Swimming)
            {
                // D7's grounding offset is valid only for a grounded visual.
                // Clear it at the D2 action boundary as soon as water owns the
                // presentation; otherwise the first swimming frame can reuse
                // the previous shore correction and lift the whole character
                // above the surface before D7's LateUpdate observes swimming.
                _visualGroundingOffsetLocal = Vector3.zero;
            }

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
            _visualRoot = FindVisualRoot();
            _visualRootBasePosition = _visualRoot.localPosition;
            _visualRootBaseRotation = _visualRoot.localRotation;
            _importedSkinnedRigNeutralOnly =
                GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0;

            Transform[] bones = GetComponentsInChildren<Transform>(true);
            var claimedBones = new HashSet<Transform>();
            _boneBindingConflict = false;
            _leftUpperArm = CreatePose(FindBone(bones, claimedBones, "left", ref _boneBindingConflict,
                "upperarm", "uparm", "arm"));
            _rightUpperArm = CreatePose(FindBone(bones, claimedBones, "right", ref _boneBindingConflict,
                "upperarm", "uparm", "arm"));
            _leftLowerArm = CreatePose(FindBone(bones, claimedBones, "left", ref _boneBindingConflict,
                "lowerarm", "forearm"));
            _rightLowerArm = CreatePose(FindBone(bones, claimedBones, "right", ref _boneBindingConflict,
                "lowerarm", "forearm"));
            _leftUpperLeg = CreatePose(FindBone(bones, claimedBones, "left", ref _boneBindingConflict,
                "upperleg", "upleg", "thigh", "femur"));
            _rightUpperLeg = CreatePose(FindBone(bones, claimedBones, "right", ref _boneBindingConflict,
                "upperleg", "upleg", "thigh", "femur"));
            _leftLowerLeg = CreatePose(FindBone(bones, claimedBones, "left", ref _boneBindingConflict,
                "lowerleg", "calf", "shin", "leg"));
            _rightLowerLeg = CreatePose(FindBone(bones, claimedBones, "right", ref _boneBindingConflict,
                "lowerleg", "calf", "shin", "leg"));
            _leftFoot = CreatePose(FindBone(bones, claimedBones, "left", ref _boneBindingConflict,
                "foot", "ankle"));
            _rightFoot = CreatePose(FindBone(bones, claimedBones, "right", ref _boneBindingConflict,
                "foot", "ankle"));

            BoundBoneCount = CountBoundBones();
            _boundBonesAreUnique = CheckBoundBonesAreUnique();
            _proceduralRigReady = !_importedSkinnedRigNeutralOnly &&
                !_boneBindingConflict && _boundBonesAreUnique &&
                HasCompleteProceduralRig();
            Debug.Log("[D2/Animation] 角色动作控制器已接线：骨骼=" + BoundBoneCount +
                "，唯一绑定=" + _boundBonesAreUnique + "，冲突=" + _boneBindingConflict +
                "，程序化能力=" + _proceduralRigReady +
                "，导入蒙皮仅中立=" + _importedSkinnedRigNeutralOnly);
            if (_importedSkinnedRigNeutralOnly)
                Debug.LogWarning("[D2/Animation] 当前导入蒙皮 rig 尚未通过动作轴/Avatar 契约，保持 neutral，禁止猜测骨骼轴驱动。");
        }

        private void SetupFormalAnimator()
        {
            _formalAnimatorReady = false;
            _poseDriverMode = _proceduralRigReady ? PoseDriverMode.Procedural : PoseDriverMode.Neutral;
            var controller = Resources.Load<RuntimeAnimatorController>("D6/D6CrewAnimator");
            if (controller == null)
            {
                DisableAllAnimators();
                Debug.LogWarning("[D2/Animation] 未找到 D6CrewAnimator，使用单一程序化/neutral 降级。");
                return;
            }

            _animator = GetComponent<Animator>();
            if (_animator == null) _animator = gameObject.AddComponent<Animator>();
            _animator.runtimeAnimatorController = controller;
            _animator.applyRootMotion = false;
            _animator.updateMode = AnimatorUpdateMode.Normal;
            _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            DisableOtherAnimators(_animator);

            // D6CrewAnimator is the generated placeholder asset.  Its clips
            // write imported Humanoid localEulerAnglesRaw curves, which are
            // absolute FBX-space rotations rather than safe deltas for this
            // runtime-bound rig.  Letting it drive the model produces the
            // distorted action reported in the native Player.  Keep one
            // deterministic procedural pose layer until a verified authored
            // controller replaces this placeholder.
            if (string.Equals(controller.name, "D6CrewAnimator", StringComparison.Ordinal))
            {
                _animator.enabled = false;
                _formalAnimatorReady = false;
                _poseDriverMode = _proceduralRigReady ? PoseDriverMode.Procedural : PoseDriverMode.Neutral;
                Debug.LogWarning("[D2/Animation] generated D6CrewAnimator uses absolute imported Euler curves; procedural/neutral pose layer retained.");
                return;
            }

            if (!_proceduralRigReady)
            {
                // 未知/不完整模型不能让 Animator 盲目写入未知层级；静止视觉比
                // 左右骨骼误配或根节点漂移更可解释，等正式 Rig 到位再接管。
                _animator.enabled = false;
                _poseDriverMode = PoseDriverMode.Neutral;
                Debug.LogWarning("[D2/Animation] 骨骼集合不完整或存在左右冲突，禁用 Animator 并保持 neutral 回退。");
                return;
            }

            if (!HasFormalActionStates(_animator))
            {
                _animator.enabled = false;
                _poseDriverMode = PoseDriverMode.Procedural;
                Debug.LogWarning("[D2/Animation] Animator 缺少 D6 动作状态，改用单一程序化姿态层。");
                return;
            }

            if (!_animator.isActiveAndEnabled)
            {
                _poseDriverMode = PoseDriverMode.Procedural;
                Debug.LogWarning("[D2/Animation] Animator 当前未启用，改用单一程序化姿态层。");
                return;
            }

            _formalAnimatorReady = true;
            _poseDriverMode = PoseDriverMode.Animator;
            Debug.Log("[D2/Animation] Unity Animator 动作资源已加载并独占姿态层：D6CrewAnimator");
        }

        private void PlayAnimatorAction(D2CharacterAction action)
        {
            if (_poseDriverMode != PoseDriverMode.Animator || !_formalAnimatorReady ||
                _animator == null || !_animator.isActiveAndEnabled ||
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
                    pose.ArmSwing = Mathf.Sin(_phase * 0.75f) * 1.5f;
                    break;
                case D2CharacterAction.Walk:
                    ApplyLocomotionPose(ref pose, Mathf.Sin(_phase), 14f, 7f, 10f);
                    break;
                case D2CharacterAction.Run:
                    ApplyLocomotionPose(ref pose, Mathf.Sin(_phase), 22f, 11f, 16f);
                    break;
                case D2CharacterAction.Jump:
                    pose.ArmSwing = -16f;
                    pose.LeftUpperLeg = -5f;
                    pose.RightUpperLeg = 5f;
                    pose.LeftLowerLeg = 11f;
                    pose.RightLowerLeg = 11f;
                    pose.LeftFoot = 3f;
                    pose.RightFoot = -3f;
                    break;
                case D2CharacterAction.Boarding:
                    ApplyBoardingPose(ref pose, false);
                    break;
                case D2CharacterAction.Disembarking:
                    ApplyBoardingPose(ref pose, true);
                    break;
                case D2CharacterAction.Swimming:
                    ApplySwimmingPose(ref pose);
                    break;
            }

            return pose;
        }

        private static void ApplyLocomotionPose(ref PoseFrame pose, float gait, float legSwing,
            float kneeBend, float armSwing)
        {
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
            pose.ArmSwing = direction * Mathf.Lerp(0f, 12f, eased);
            pose.LeftUpperLeg = direction * Mathf.Lerp(0f, 6f, eased);
            pose.RightUpperLeg = -direction * Mathf.Lerp(0f, 6f, eased);
            pose.LeftLowerLeg = Mathf.Lerp(0f, 9f, eased);
            pose.RightLowerLeg = Mathf.Lerp(0f, 9f, eased);
            pose.LeftFoot = -direction * Mathf.Lerp(0f, 2.5f, eased);
            pose.RightFoot = direction * Mathf.Lerp(0f, 2.5f, eased);
        }

        private void ApplySwimmingPose(ref PoseFrame pose)
        {
            // 仅对已经通过完整骨骼契约的程序化回退生效；导入蒙皮仍保持
            // neutral，避免再次用猜测的轴向造成“动作扭曲”。
            float stroke = Mathf.Sin(_phase * 1.6f);
            // The fallback arm bones are authored in a hanging-down neutral
            // pose.  A small angle around that neutral pose therefore keeps
            // both arms behind the torso after the whole visual root is
            // pitched into the water; the Player then reads as a broken
            // upright figure instead of a swimmer.  Use a forward-reaching
            // breaststroke silhouette with a bounded alternating offset.
            // This only drives the explicit procedural fallback contract; it
            // does not guess axes for imported skinned rigs.
            pose.ArmSwing = ProceduralSwimmingUpperArmReachDegrees + stroke * 10f;
            pose.LeftUpperLeg = 4f - stroke * 3f;
            pose.RightUpperLeg = -4f + stroke * 3f;
            pose.LeftLowerLeg = 3f + Mathf.Abs(stroke) * 2.5f;
            pose.RightLowerLeg = 3f + Mathf.Abs(stroke) * 2.5f;
            pose.LeftFoot = stroke * ProceduralSwimmingFootStrokeDegrees;
            pose.RightFoot = -stroke * ProceduralSwimmingFootStrokeDegrees;
        }

        private static PoseFrame LerpPose(PoseFrame from, PoseFrame to, float t)
        {
            return new PoseFrame
            {
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
            // Imported FBX bones do not yet have a verified local-axis contract.
            // Animate only the already-calibrated visual root; never write
            // guessed arm/leg rotations into the skinned hierarchy. This is a
            // safe presentation layer, not a substitute for authored clips.
            if (_importedSkinnedRigNeutralOnly)
            {
                ApplyImportedVisualPose();
                return;
            }

            // There is one visual-root submission per frame.  Animator and
            // procedural bone layers may differ, but neither gets a second
            // root write through OnAnimatorMove.
            ApplyVisualRootBaseline();

            // Animator 已接管时这里不再写任何骨骼；程序化层只在完整、唯一、
            // 无冲突的骨骼集合上运行，避免同一帧被两套驱动源反复覆盖。
            if (_poseDriverMode != PoseDriverMode.Procedural) return;

            ApplyProceduralVisualPose();
            float leftArmAngle = pose.ArmSwing;
            float rightArmAngle = -pose.ArmSwing;
            float leftLowerArmAngle = 0f;
            float rightLowerArmAngle = 0f;
            if (Action == D2CharacterAction.Swimming)
            {
                // Keep both upper arms inside the verified local-axis envelope
                // and use the forearms to complete the forward reach. The
                // sign-flipped idle/locomotion convention would otherwise send
                // one arm backwards and collapse the silhouette in side-biased
                // swimming shots.
                float stroke = Mathf.Sin(_phase * 1.6f);
                leftArmAngle = ProceduralSwimmingUpperArmReachDegrees + stroke * 10f;
                rightArmAngle = ProceduralSwimmingUpperArmReachDegrees - stroke * 10f;
                // LowerArm is a child of UpperArm in the procedural fallback.
                // Applying the old -80 degree value twice folded the forearm
                // back over the shoulder (the native Player read as a twisted
                // or dangling action).  Keep the upper-arm stroke bounded and
                // use a small compensating child rotation so the combined arm
                // points toward the water instead of back toward the torso.
                leftLowerArmAngle = ProceduralSwimmingForearmReachDegrees - stroke * 6f;
                rightLowerArmAngle = ProceduralSwimmingForearmReachDegrees + stroke * 6f;
            }
            if (_shipControlActive)
            {
                // D7 places the procedural fallback at the visible helm. The
                // old Idle pose left both arms hanging by the torso, so the
                // Player read as standing beside/behind an unrelated prop.
                // Keep the fix presentation-only and symmetric: both arms
                // reach toward the stern-side wheel while D2 owns the mode.
                // A small forearm bend keeps the elbow silhouette readable;
                // the bounded value still leaves each hand inside the wheel
                // ring and does not claim a formal Avatar/clip contract.
                const float shipSteeringArmAngle = -35f;
                const float shipSteeringForearmAngle = -10f;
                leftArmAngle = shipSteeringArmAngle;
                rightArmAngle = shipSteeringArmAngle;
                leftLowerArmAngle = shipSteeringForearmAngle;
                rightLowerArmAngle = shipSteeringForearmAngle;
            }
            SetBoneRotation(_leftUpperArm, leftArmAngle);
            SetBoneRotation(_rightUpperArm, rightArmAngle);
            SetBoneRotation(_leftLowerArm, leftLowerArmAngle);
            SetBoneRotation(_rightLowerArm, rightLowerArmAngle);
            SetBoneRotation(_leftUpperLeg, pose.LeftUpperLeg);
            SetBoneRotation(_rightUpperLeg, pose.RightUpperLeg);
            SetBoneRotation(_leftLowerLeg, pose.LeftLowerLeg);
            SetBoneRotation(_rightLowerLeg, pose.RightLowerLeg);
            SetBoneRotation(_leftFoot, pose.LeftFoot);
            SetBoneRotation(_rightFoot, pose.RightFoot);
        }


        private void ApplyProceduralVisualPose()
        {
            if (_visualRoot == null || _visualRoot == transform) return;

            float pitch = 0f;
            float roll = 0f;
            float bob = 0f;
            Vector3 visualPosition = _visualRootBasePosition + _visualGroundingOffsetLocal;
            Quaternion visualRotation = _visualRootBaseRotation;
            SwimmingVisualPitchDegrees = 0f;
            SwimmingVisualRollDegrees = 0f;
            SwimmingVisualBodyDrop = 0f;
            if (Action == D2CharacterAction.Swimming)
            {
                // The procedural fallback has an explicit bone contract, so
                // it can use a whole-visual swim posture in addition to the
                // arm/leg stroke.  The physics root remains upright; only the
                // presentation child leans toward the water surface.  This
                // prevents the fallback from reading as a standing character
                // even when a screenshot catches a neutral stroke phase.
                // Rotate around the hip line instead of the physics-root
                // origin.  Rotating around the feet was the source of the
                // native Player's twisted/donut-like swim silhouette: the
                // torso and head moved above the water while the legs stayed
                // pinned to the capsule base.  The explicit pivot keeps the
                // whole fallback rig coherent and leaves the CharacterController
                // upright for collision and water entry decisions.
                const float swimPivotHeight = 1.12f;
                // The visual child is normalized to a 1.8m target height before
                // this pose runs.  The previous -0.88m drop was authored against
                // the unscaled fallback bounds, leaving the torso/legs under the
                // water surface in the real Player and making the action read as
                // detached fragments.  Keep the hip pivot and physics root
                // contracts unchanged, but raise the normalized visual body.
                const float swimBodyDrop = -0.62f;
                pitch = ProceduralSwimmingPitchDegrees;
                // Do not add a second lateral axis to the already pitched
                // low-poly fallback.  R27 proved movement but the visible
                // body read as twisted whenever this roll crossed a screenshot
                // sample.  The arm/leg stroke remains the only animation
                // source during this bounded remediation.
                roll = 0f;
                bob = swimBodyDrop + Mathf.Abs(Mathf.Sin(_phase * 1.6f)) * 0.025f;
                SwimmingVisualPitchDegrees = pitch;
                SwimmingVisualRollDegrees = roll;
                SwimmingVisualBodyDrop = bob;
                visualRotation = _visualRootBaseRotation * Quaternion.Euler(pitch, 0f, roll);
                Vector3 pivot = Vector3.up * swimPivotHeight;
                visualPosition += (_visualRootBaseRotation * pivot) - (visualRotation * pivot);
            }

            _visualRoot.localPosition = visualPosition + Vector3.up * bob;
            _visualRoot.localRotation = visualRotation;
        }

        private void ApplyImportedVisualPose()
        {
            if (!ImportedVisualActionMotionActive) return;

            float pitch = 0f;
            float roll = 0f;
            float bob = 0f;
            switch (Action)
            {
                case D2CharacterAction.Walk:
                    bob = Mathf.Abs(Mathf.Sin(_phase)) * 0.025f;
                    pitch = 2.5f;
                    break;
                case D2CharacterAction.Run:
                    bob = Mathf.Abs(Mathf.Sin(_phase)) * 0.045f;
                    pitch = 5f;
                    break;
                case D2CharacterAction.Jump:
                    bob = 0.08f;
                    pitch = -8f;
                    break;
                case D2CharacterAction.Boarding:
                    pitch = -10f * ActionTransitionProgress();
                    break;
                case D2CharacterAction.Disembarking:
                    pitch = 8f * ActionTransitionProgress();
                    break;
                case D2CharacterAction.Swimming:
                    // A whole-rig pitch is axis-safe and makes the imported
                    // humanoid read as a surface swimmer without touching
                    // unknown FBX arm/leg axes. Keep the tilt below the
                    // collapse-to-horizontal range seen in the native Player;
                    // the small roll and bob still make the swimming state
                    // visible without burying the whole mesh under the water.
                    pitch = 32f;
                    roll = Mathf.Sin(_phase * 1.25f) * 2f;
                    bob = -0.06f + Mathf.Abs(Mathf.Sin(_phase * 1.25f)) * 0.02f;
                    break;
            }

            _visualRoot.localPosition = _visualRootBasePosition + _visualGroundingOffsetLocal + Vector3.up * bob;
            _visualRoot.localRotation = _visualRootBaseRotation * Quaternion.Euler(pitch, 0f, roll);
        }

        private float ActionTransitionProgress()
        {
            return Mathf.Clamp01(1f - _transitionRemaining / Mathf.Max(transitionDuration, 0.1f));
        }

        private void OnAnimatorMove()
        {
            // Root Motion 已关闭。visualRoot 的提交统一发生在 ApplyPose，
            // 避免 Animator 回调和动作层在同一帧重复写入同一批 Transform。
        }

        private void ApplyVisualRootBaseline()
        {
            if (_visualRoot == null || _visualRoot == transform) return;
            _visualRoot.localPosition = _visualRootBasePosition + _visualGroundingOffsetLocal;
            _visualRoot.localRotation = _visualRootBaseRotation;
        }

        private void ResetVisualPoseToBaseline()
        {
            if (_visualRoot != null && _visualRoot != transform)
            {
                _visualRoot.localPosition = _visualRootBasePosition;
                _visualRoot.localRotation = _visualRootBaseRotation;
            }

            ResetBonePose(_leftUpperArm);
            ResetBonePose(_rightUpperArm);
            ResetBonePose(_leftLowerArm);
            ResetBonePose(_rightLowerArm);
            ResetBonePose(_leftUpperLeg);
            ResetBonePose(_rightUpperLeg);
            ResetBonePose(_leftLowerLeg);
            ResetBonePose(_rightLowerLeg);
            ResetBonePose(_leftFoot);
            ResetBonePose(_rightFoot);
        }

        private static void ResetBonePose(BonePose bone)
        {
            if (bone != null && bone.Transform != null)
                bone.Transform.localRotation = bone.BaseRotation;
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

        private Transform FindVisualRoot()
        {
            if (transform.childCount == 0) return transform;

            Transform rendererRoot = null;
            int rendererCount = -1;
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                string name = NormalizeName(child.name);
                if (name == "humanarmature" || name == "humanoidvisual" || name == "visual")
                    return child;

                int count = child.GetComponentsInChildren<Renderer>(true).Length;
                if (count > rendererCount)
                {
                    rendererRoot = child;
                    rendererCount = count;
                }
            }

            return rendererRoot == null ? transform.GetChild(0) : rendererRoot;
        }

        private static bool HasFormalActionStates(Animator animator)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;
            foreach (D2CharacterAction action in Enum.GetValues(typeof(D2CharacterAction)))
            {
                int stateHash = Animator.StringToHash("D6_" + action);
                if (!animator.HasState(0, stateHash)) return false;
            }
            return true;
        }

        private void DisableAllAnimators()
        {
            foreach (var animator in GetComponentsInChildren<Animator>(true))
                if (animator != null) animator.enabled = false;
        }

        private static void DisableOtherAnimators(Animator keeper)
        {
            if (keeper == null) return;
            foreach (var animator in keeper.GetComponentsInChildren<Animator>(true))
                if (animator != null && animator != keeper) animator.enabled = false;
        }

        private bool HasCompleteProceduralRig()
        {
            return _leftUpperArm != null && _rightUpperArm != null &&
                _leftUpperLeg != null && _rightUpperLeg != null &&
                _leftLowerLeg != null && _rightLowerLeg != null &&
                _leftFoot != null && _rightFoot != null && BoundBoneCount == 8;
        }

        private bool CheckBoundBonesAreUnique()
        {
            var bound = new HashSet<Transform>();
            return AddUnique(bound, _leftUpperArm) &&
                AddUnique(bound, _rightUpperArm) &&
                AddUnique(bound, _leftUpperLeg) &&
                AddUnique(bound, _rightUpperLeg) &&
                AddUnique(bound, _leftLowerLeg) &&
                AddUnique(bound, _rightLowerLeg) &&
                AddUnique(bound, _leftFoot) &&
                AddUnique(bound, _rightFoot);
        }

        private static bool AddUnique(HashSet<Transform> bound, BonePose bone)
        {
            return bone == null || bone.Transform == null || bound.Add(bone.Transform);
        }

        private static Transform FindBone(Transform[] bones, HashSet<Transform> claimed,
            string side, ref bool bindingConflict, params string[] tokens)
        {
            Transform best = null;
            int bestScore = -1;
            bool tiedForBest = false;
            int candidateCount = 0;
            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];
                if (bone == null) continue;
                // 低模回退会在骨骼旁挂同名的 *Mesh 节点；渲染节点不是可驱动
                // 骨骼，若把它与骨骼一起计入候选会把正常模型误判成左右冲突。
                if (IsInvalidBoneCandidate(bone)) continue;
                int score = ScoreBoneName(bone.name, side, tokens);
                if (score < 0) continue;
                candidateCount++;
                if (claimed.Contains(bone)) continue;

                if (score > bestScore)
                {
                    best = bone;
                    bestScore = score;
                    tiedForBest = false;
                }
                else if (score == bestScore)
                {
                    tiedForBest = true;
                }
            }

            if (best == null)
            {
                if (candidateCount > 0) bindingConflict = true;
                return null;
            }
            if (tiedForBest)
            {
                bindingConflict = true;
                return null;
            }

            claimed.Add(best);
            return best;
        }

        private static bool IsInvalidBoneCandidate(Transform candidate)
        {
            if (candidate == null || candidate.GetComponent<Renderer>() != null) return true;
            string name = NormalizeName(candidate.name);
            return name.Contains("mesh") || name.Contains("geometry") ||
                name.Contains("renderer") || name.Contains("skin");
        }

        private static int ScoreBoneName(string value, string side, string[] tokens)
        {
            string name = NormalizeName(value);
            string opposite = side == "left" ? "right" : "left";
            if (name.Contains(opposite) || HasOppositeShortSide(name, side) ||
                !HasSide(name, side)) return -1;

            int bestTokenScore = -1;
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = NormalizeName(tokens[i]);
                if (string.IsNullOrEmpty(token) || !name.Contains(token)) continue;
                if (IsIncompatibleBoneName(name, token)) continue;
                bestTokenScore = Mathf.Max(bestTokenScore, TokenScore(token));
            }

            if (bestTokenScore < 0) return -1;
            int sideScore = name.Contains(side) ? 1000 : 500;
            return sideScore + bestTokenScore;
        }

        private static int TokenScore(string token)
        {
            switch (token)
            {
                case "upperarm": return 500;
                case "uparm": return 490;
                case "arm": return 300;
                case "upperleg": return 500;
                case "upleg": return 490;
                case "thigh": return 480;
                case "femur": return 470;
                case "lowerleg": return 500;
                case "calf": return 490;
                case "shin": return 480;
                case "leg": return 300;
                case "foot": return 500;
                case "ankle": return 400;
                default: return token.Length;
            }
        }

        private static bool IsIncompatibleBoneName(string name, string token)
        {
            if (token == "arm" || token == "upperarm" || token == "uparm")
                return name.Contains("forearm") || name.Contains("lowerarm") || name.Contains("hand");
            if (token == "leg")
                return name.Contains("upper") || name.Contains("lower") || name.Contains("upleg") ||
                    name.Contains("thigh") || name.Contains("femur") || name.Contains("calf") || name.Contains("shin");
            return false;
        }

        private static bool HasSide(string name, string side)
        {
            if (name.Contains(side)) return true;
            string prefix = side == "left" ? "l" : "r";
            return name.StartsWith(prefix, StringComparison.Ordinal) ||
                name.EndsWith(prefix, StringComparison.Ordinal);
        }

        private static bool HasOppositeShortSide(string name, string side)
        {
            string opposite = side == "left" ? "r" : "l";
            return name.StartsWith(opposite, StringComparison.Ordinal) ||
                name.EndsWith(opposite, StringComparison.Ordinal);
        }

        private static string NormalizeName(string value)
        {
            return (value ?? string.Empty).ToLowerInvariant()
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty)
                .Replace(":", string.Empty);
        }
    }
}
