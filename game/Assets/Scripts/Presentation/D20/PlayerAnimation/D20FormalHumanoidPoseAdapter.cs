using System;
using UnityEngine;

namespace WhalesIslandReef.Presentation.D20.PlayerAnimation
{
    /// <summary>
    /// Experimental formal-R02 presentation adapter. It consumes the same
    /// read-only D20 snapshot as the generic adapter, but submits pose changes
    /// through Unity's validated Humanoid Avatar muscle contract instead of
    /// writing guessed FBX local axes.
    ///
    /// This is deliberately opt-in. It is not an authored animation resource
    /// and therefore must not open the Q03 formal-action admission gate.
    /// </summary>
    public sealed class D20FormalHumanoidPoseAdapter : MonoBehaviour
    {
        private const float SwimBodyPitchDegrees = 70f;
        private const float WalkPhaseSpeed = 7.5f;
        private const float RunPhaseSpeed = 11f;
        private const float SwimPhaseSpeed = 2.8f;

        // The formal R02 wrapper is authored at a higher local body origin
        // than the visible D19 helm wheel. These offsets were measured against
        // the current R02 prefab + InitialHarbor HelmWheel in the connected
        // Windows Player. They are a bounded presentation pose, not gameplay
        // or IK ownership; the physical D2 root remains untouched.
        private static readonly Vector3 SwimBodyPositionOffset =
            new Vector3(0f, -0.85f, 0f);
        private static readonly Vector3 HelmBodyPositionOffset =
            new Vector3(0f, -0.73f, 0.34f);
        private const float HelmShoulderFrontBack = -1f;
        private const float HelmArmFrontBack = -1f;
        private const float HelmForearmStretch = -1f;

        // Unity Humanoid muscle indices are stable through HumanTrait and are
        // resolved once here so the pose remains in the Avatar contract.
        private const int SpineFrontBack = 0;
        private const int ChestFrontBack = 3;
        private const int UpperChestFrontBack = 6;
        private const int LeftUpperLegFrontBack = 21;
        private const int LeftUpperLegTwist = 23;
        private const int LeftLowerLegStretch = 24;
        private const int LeftFootUpDown = 26;
        private const int RightUpperLegFrontBack = 29;
        private const int RightUpperLegTwist = 31;
        private const int RightLowerLegStretch = 32;
        private const int RightFootUpDown = 34;
        private const int LeftShoulderDownUp = 37;
        private const int LeftShoulderFrontBack = 38;
        private const int LeftArmDownUp = 39;
        private const int LeftArmFrontBack = 40;
        private const int LeftForearmStretch = 42;
        private const int RightShoulderDownUp = 46;
        private const int RightShoulderFrontBack = 47;
        private const int RightArmDownUp = 48;
        private const int RightArmFrontBack = 49;
        private const int RightForearmStretch = 51;

        private Animator _animator;
        private Avatar _avatar;
        private Transform _humanoidRoot;
        private HumanPoseHandler _handler;
        private HumanPose _restPose;
        private HumanPose _pose;
        private float _phase;
        private bool _configured;

        public bool IsReady { get; private set; }
        public bool LastApplySucceeded { get; private set; }
        public int PoseApplicationCount { get; private set; }
        public float LastBodyPitchDegrees { get; private set; }
        public D20PlayerAnimationState CurrentState { get; private set; } = D20PlayerAnimationState.Idle;
        public Transform HumanoidRoot => _humanoidRoot;

        /// <summary>
        /// Binds only to a formal Animator that already exposes a valid
        /// Humanoid Avatar. The source wrapper is retained because its Avatar
        /// skeleton paths are authored relative to that wrapper.
        /// </summary>
        public bool ConfigureForWork(Animator workAnimator)
        {
            DisposeHandler();
            _animator = workAnimator;
            _avatar = _animator == null ? null : _animator.avatar;
            _humanoidRoot = FindHumanoidRoot(_animator);
            _configured = true;
            IsReady = _avatar != null && _avatar.isValid && _avatar.isHuman && _humanoidRoot != null;
            LastApplySucceeded = false;
            PoseApplicationCount = 0;
            LastBodyPitchDegrees = 0f;

            if (!IsReady) return false;

            try
            {
                _handler = new HumanPoseHandler(_avatar, _humanoidRoot);
                _restPose = new HumanPose { muscles = new float[HumanTrait.MuscleCount] };
                _pose = new HumanPose { muscles = new float[HumanTrait.MuscleCount] };
                _handler.GetHumanPose(ref _restPose);
                if (_restPose.muscles == null || _restPose.muscles.Length != HumanTrait.MuscleCount)
                {
                    DisposeHandler();
                    IsReady = false;
                    return false;
                }

                Array.Copy(_restPose.muscles, _pose.muscles, HumanTrait.MuscleCount);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[D20/FormalHumanoid] Avatar pose handler unavailable: " + exception.Message);
                DisposeHandler();
                IsReady = false;
                return false;
            }
        }

        /// <summary>
        /// Applies a presentation pose without changing the D2 physical root,
        /// gameplay facts, input state, or any authoritative system value.
        /// </summary>
        public bool ApplySnapshot(D20PlayerAnimationSnapshot snapshot, D20PlayerAnimationState state)
        {
            if (!IsReady || _handler == null || snapshot.paused)
            {
                LastApplySucceeded = false;
                return false;
            }

            CurrentState = state;
            var phaseSpeed = state == D20PlayerAnimationState.Fish
                ? SwimPhaseSpeed
                : state == D20PlayerAnimationState.Run ? RunPhaseSpeed : WalkPhaseSpeed;
            _phase += Mathf.Max(0f, Time.deltaTime) * phaseSpeed;
            if (_phase > Mathf.PI * 2f) _phase -= Mathf.PI * 2f;

            Array.Copy(_restPose.muscles, _pose.muscles, HumanTrait.MuscleCount);
            _pose.bodyPosition = _restPose.bodyPosition;
            _pose.bodyRotation = _restPose.bodyRotation;
            LastBodyPitchDegrees = 0f;

            switch (state)
            {
                case D20PlayerAnimationState.Fish:
                    ApplySwimmingPose();
                    break;
                case D20PlayerAnimationState.Helm:
                    ApplyHelmPose();
                    break;
                case D20PlayerAnimationState.Board:
                case D20PlayerAnimationState.Disembark:
                    ApplyBoardingPose();
                    break;
                case D20PlayerAnimationState.Jump:
                case D20PlayerAnimationState.Fall:
                    ApplyAirbornePose(snapshot.verticalSpeed);
                    break;
                default:
                    ApplyStandingPose(snapshot.speed01, state == D20PlayerAnimationState.Run);
                    break;
            }

            try
            {
                _handler.SetHumanPose(ref _pose);
                PoseApplicationCount++;
                LastApplySucceeded = true;
                return true;
            }
            catch (Exception exception)
            {
                LastApplySucceeded = false;
                Debug.LogWarning("[D20/FormalHumanoid] Avatar pose submission failed: " + exception.Message);
                return false;
            }
        }

        public void ResetPose()
        {
            if (!IsReady || _handler == null) return;
            Array.Copy(_restPose.muscles, _pose.muscles, HumanTrait.MuscleCount);
            _pose.bodyPosition = _restPose.bodyPosition;
            _pose.bodyRotation = _restPose.bodyRotation;
            _handler.SetHumanPose(ref _pose);
            CurrentState = D20PlayerAnimationState.Idle;
            LastBodyPitchDegrees = 0f;
        }

        private void ApplyStandingPose(float speed01, bool running)
        {
            var stride = Mathf.Sin(_phase) * (running ? 0.30f : Mathf.Lerp(0.08f, 0.20f, speed01));
            Set(LeftShoulderDownUp, 0.65f);
            Set(RightShoulderDownUp, 0.65f);
            Set(LeftShoulderFrontBack, -0.05f);
            Set(RightShoulderFrontBack, -0.05f);
            Set(LeftArmDownUp, 0.45f);
            Set(RightArmDownUp, 0.45f);
            Set(LeftArmFrontBack, -0.08f + stride * 0.35f);
            Set(RightArmFrontBack, -0.08f - stride * 0.35f);
            Set(LeftForearmStretch, 0.10f);
            Set(RightForearmStretch, 0.10f);
            Set(LeftUpperLegFrontBack, stride);
            Set(RightUpperLegFrontBack, -stride);
            Set(LeftLowerLegStretch, 0.08f);
            Set(RightLowerLegStretch, 0.08f);
            Set(LeftFootUpDown, -stride * 0.35f);
            Set(RightFootUpDown, stride * 0.35f);
        }

        private void ApplyBoardingPose()
        {
            Set(LeftShoulderDownUp, 0.55f);
            Set(RightShoulderDownUp, 0.55f);
            Set(LeftArmDownUp, 0.30f);
            Set(RightArmDownUp, 0.30f);
            Set(LeftArmFrontBack, -0.35f);
            Set(RightArmFrontBack, -0.35f);
            Set(LeftUpperLegFrontBack, 0.22f);
            Set(RightUpperLegFrontBack, -0.22f);
            Set(LeftLowerLegStretch, -0.10f);
            Set(RightLowerLegStretch, -0.10f);
            Set(SpineFrontBack, 0.12f);
            Set(ChestFrontBack, 0.10f);
        }

        private void ApplyAirbornePose(float verticalSpeed)
        {
            var lift = Mathf.Clamp(verticalSpeed * 0.08f, -0.25f, 0.25f);
            Set(LeftShoulderDownUp, 0.45f);
            Set(RightShoulderDownUp, 0.45f);
            Set(LeftArmDownUp, 0.20f + lift);
            Set(RightArmDownUp, 0.20f + lift);
            Set(LeftUpperLegFrontBack, -0.18f);
            Set(RightUpperLegFrontBack, -0.18f);
            Set(LeftLowerLegStretch, -0.20f);
            Set(RightLowerLegStretch, -0.20f);
            Set(SpineFrontBack, -0.08f);
        }

        private void ApplyHelmPose()
        {
            // A bounded two-arm hold calibrated to the visible D19 wheel. The
            // wheel remains a separate geometry owner and this is still not a
            // general IK solver, but the body/arm envelope now meets the
            // current formal R02 helm anchor instead of hovering above it.
            _pose.bodyPosition = _restPose.bodyPosition + HelmBodyPositionOffset;
            Set(LeftShoulderDownUp, 0.45f);
            Set(RightShoulderDownUp, 0.45f);
            Set(LeftShoulderFrontBack, HelmShoulderFrontBack);
            Set(RightShoulderFrontBack, HelmShoulderFrontBack);
            Set(LeftArmDownUp, 0.70f);
            Set(RightArmDownUp, 0.70f);
            Set(LeftArmFrontBack, HelmArmFrontBack);
            Set(RightArmFrontBack, HelmArmFrontBack);
            Set(LeftForearmStretch, HelmForearmStretch);
            Set(RightForearmStretch, HelmForearmStretch);
            Set(SpineFrontBack, 0.05f);
            Set(ChestFrontBack, 0.04f);
        }

        private void ApplySwimmingPose()
        {
            var stroke = Mathf.Sin(_phase);
            var counterStroke = Mathf.Sin(_phase + Mathf.PI);
            _pose.bodyRotation = _restPose.bodyRotation * Quaternion.Euler(SwimBodyPitchDegrees, 0f, 0f);
            _pose.bodyPosition = _restPose.bodyPosition + SwimBodyPositionOffset;
            LastBodyPitchDegrees = SwimBodyPitchDegrees;
            // Keep the verified whole-body pitch, but preserve the Avatar's
            // extended-limb rest envelope.  The former values drove the
            // Stretch muscles toward zero, which is a folded elbow/knee for
            // this imported Avatar rather than a relaxed surface stroke. The
            // result was the visible raised-arms and tucked-legs silhouette
            // recorded in the R7 native Player frame. This remains
            // presentation-only: D2 owns the physical root and no imported
            // action clip is admitted here.
            Set(SpineFrontBack, 0.01f);
            Set(ChestFrontBack, 0.01f);
            Set(UpperChestFrontBack, 0.01f);
            Set(LeftShoulderDownUp, -0.08f);
            Set(RightShoulderDownUp, -0.08f);
            Set(LeftShoulderFrontBack, -0.06f);
            Set(RightShoulderFrontBack, -0.06f);
            Set(LeftArmDownUp, -0.04f);
            Set(RightArmDownUp, -0.04f);
            Set(LeftArmFrontBack, -0.18f + stroke * 0.06f);
            Set(RightArmFrontBack, -0.18f + counterStroke * 0.06f);
            Set(LeftForearmStretch, 0.82f + stroke * 0.04f);
            Set(RightForearmStretch, 0.82f + counterStroke * 0.04f);
            Set(LeftUpperLegFrontBack, 0.04f * counterStroke);
            Set(RightUpperLegFrontBack, 0.04f * stroke);
            Set(LeftUpperLegTwist, 0.01f * stroke);
            Set(RightUpperLegTwist, 0.01f * counterStroke);
            Set(LeftLowerLegStretch, 0.86f);
            Set(RightLowerLegStretch, 0.86f);
            Set(LeftFootUpDown, -0.02f * counterStroke);
            Set(RightFootUpDown, -0.02f * stroke);
        }

        private void Set(int index, float value)
        {
            if (_pose.muscles == null || index < 0 || index >= _pose.muscles.Length) return;
            _pose.muscles[index] = Mathf.Clamp(value, -1f, 1f);
        }

        private static Transform FindHumanoidRoot(Animator workAnimator)
        {
            if (workAnimator == null) return null;
            foreach (Transform child in workAnimator.transform)
            {
                if (child == null) continue;
                if (child.Find("Armature/root/pelvis") != null) return child;
            }

            return workAnimator.transform.Find("Armature/root") ?? workAnimator.transform;
        }

        private void DisposeHandler()
        {
            if (_handler != null) _handler.Dispose();
            _handler = null;
            _avatar = null;
            _humanoidRoot = null;
            _configured = false;
            IsReady = false;
        }

        private void OnDestroy()
        {
            DisposeHandler();
        }
    }
}
