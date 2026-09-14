using System;
using UnityEngine;

namespace WhalesIslandReef.Presentation.D20.PlayerAnimation
{
    public enum D20PlayerAnimationState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall,
        Land,
        Interact,
        Talk,
        Carry,
        Fish,
        Board,
        Disembark,
        Helm,
        Hit,
        Recover
    }

    /// <summary>
    /// Read-only facts supplied by the owning movement, interaction, ship or
    /// health system. This type intentionally contains no position, inventory,
    /// reward, save or input-writer fields.
    /// </summary>
    [Serializable]
    public struct D20PlayerAnimationSnapshot
    {
        public bool grounded;
        [Range(0f, 1f)] public float speed01;
        public float verticalSpeed;
        public string controlMode;
        public string interactionKind;
        public string actionIntent;
        public bool hit;
        public bool recovering;
        public bool paused;
        public int lodBand;
    }

    /// <summary>
    /// Deterministic presentation mapping. It never changes the supplied
    /// facts and never owns movement or gameplay state.
    /// </summary>
    public static class D20PlayerAnimationStateMap
    {
        private const float IdleSpeedThreshold = 0.01f;
        private const float RunSpeedThreshold = 0.65f;

        public static D20PlayerAnimationState Resolve(D20PlayerAnimationSnapshot snapshot,
            bool previousGrounded)
        {
            var intent = Normalize(snapshot.actionIntent);
            var interaction = Normalize(snapshot.interactionKind);

            if (snapshot.hit) return D20PlayerAnimationState.Hit;
            if (snapshot.recovering) return D20PlayerAnimationState.Recover;

            var explicitState = ResolveIntent(intent, interaction);
            if (explicitState.HasValue) return explicitState.Value;

            if (!snapshot.grounded)
                return snapshot.verticalSpeed >= 0.1f
                    ? D20PlayerAnimationState.Jump
                    : D20PlayerAnimationState.Fall;

            if (!previousGrounded) return D20PlayerAnimationState.Land;
            // D2 accelerates toward its target velocity. A real movement
            // intent can therefore legitimately arrive with speed01 around
            // 0.04 on the first frame; treating that as Idle makes the
            // candidate visual visibly lag behind the authoritative action.
            if (snapshot.speed01 <= IdleSpeedThreshold) return D20PlayerAnimationState.Idle;
            return snapshot.speed01 < RunSpeedThreshold
                ? D20PlayerAnimationState.Walk
                : D20PlayerAnimationState.Run;
        }

        public static string ToStateName(D20PlayerAnimationState state)
        {
            return state.ToString();
        }

        public static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static D20PlayerAnimationState? ResolveIntent(string intent, string interaction)
        {
            var value = string.IsNullOrEmpty(intent) ? interaction : intent;
            switch (value)
            {
                case "interact":
                    return D20PlayerAnimationState.Interact;
                case "talk":
                case "dialogue":
                    return D20PlayerAnimationState.Talk;
                case "carry":
                case "haul":
                    return D20PlayerAnimationState.Carry;
                case "fish":
                case "fishing":
                    return D20PlayerAnimationState.Fish;
                case "board":
                case "boarding":
                case "embark":
                    return D20PlayerAnimationState.Board;
                case "disembark":
                case "disembarking":
                case "unboard":
                    return D20PlayerAnimationState.Disembark;
                case "helm":
                case "steer":
                case "ship_control":
                    return D20PlayerAnimationState.Helm;
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Presentation-only Animator adapter. It consumes snapshots, parameters
    /// and animation states, but never writes Transform position or gameplay
    /// facts. Root Motion is always disabled so D17 CharacterController stays
    /// the sole movement writer.
    /// </summary>
    public sealed class D20PlayerAnimationPresentationAdapter : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private int initialLodBand;

        private bool _hasPreviousGrounded;
        private bool _previousGrounded = true;
        private bool _initialized;
        private int _lodBand;
        private int _lastNeutralArmCorrectionFrame = -1;
        private int _lastHelmArmCorrectionFrame = -1;
        private int _lastFishArmCorrectionFrame = -1;

        public Animator BoundAnimator => animator;
        public D20PlayerAnimationState CurrentState { get; private set; } = D20PlayerAnimationState.Idle;
        public string LastStateName { get; private set; } = "Idle";
        public bool LastApplyUsedFallback { get; private set; }
        public bool IsPaused { get; private set; }
        public int CurrentLodBand => _lodBand;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
            _lodBand = Mathf.Max(0, initialLodBand);
            ConfigureAnimator();
        }

        private void OnEnable()
        {
            ConfigureAnimator();
        }

        public void ConfigureForWork(Animator workAnimator)
        {
            animator = workAnimator;
            ConfigureAnimator();
        }

        public void ResetSnapshotHistory(bool grounded)
        {
            _hasPreviousGrounded = true;
            _previousGrounded = grounded;
            _initialized = false;
        }

        /// <summary>Applies only visual parameters and the Animator state.</summary>
        public D20PlayerAnimationState ApplyReadOnlySnapshot(D20PlayerAnimationSnapshot snapshot)
        {
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
            ConfigureAnimator();

            if (snapshot.paused)
            {
                IsPaused = true;
                if (animator != null) animator.speed = 0f;
                return CurrentState;
            }

            IsPaused = false;
            if (animator != null) animator.speed = 1f;

            var previousGrounded = _hasPreviousGrounded ? _previousGrounded : snapshot.grounded;
            var next = D20PlayerAnimationStateMap.Resolve(snapshot, previousGrounded);
            _previousGrounded = snapshot.grounded;
            _hasPreviousGrounded = true;
            _initialized = true;

            if (animator != null)
            {
                SetFloat("Speed01", Mathf.Clamp01(snapshot.speed01));
                SetFloat("VerticalSpeed", snapshot.verticalSpeed);
                SetBool("Grounded", snapshot.grounded);
                SetBool("InteractionActive", !string.IsNullOrEmpty(snapshot.interactionKind));
                SetBool("Paused", false);
                SetInteger("ActionHash", Animator.StringToHash(next.ToString()));
            }

            PlayState(next);
            return CurrentState;
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            if (animator == null) return;
            animator.speed = paused ? 0f : 1f;
            SetBool("Paused", paused);
        }

        public void SetLodBand(int band)
        {
            _lodBand = Mathf.Max(0, band);
            if (animator == null) return;
            animator.applyRootMotion = false;
            animator.cullingMode = _lodBand >= 2
                ? AnimatorCullingMode.CullCompletely
                : AnimatorCullingMode.CullUpdateTransforms;
            animator.enabled = _lodBand < 3;
        }

        public bool HasAnimatorState(string stateName)
        {
            return animator != null && animator.runtimeAnimatorController != null &&
                   animator.HasState(0, Animator.StringToHash(stateName));
        }

        /// <summary>
        /// Applies the bounded safety pose for the current generic-path
        /// placeholder clips. The imported R02 rest pose keeps both upper
        /// arms in a horizontal T-pose; until authored generic/humanoid
        /// action clips are admitted, the locomotion and common interaction
        /// states need a presentation-only shoulder drop to avoid showing
        /// that source bind pose in-game. Fish and Helm use dedicated
        /// presentation corrections because their whole-body/action contracts
        /// are different from ordinary locomotion.
        /// </summary>
        public bool ApplyGenericPathNeutralArmCorrection()
        {
            if (animator == null || !_initialized || IsPaused || !UsesNeutralArmDrop(CurrentState)) return false;
            if (_lastNeutralArmCorrectionFrame == Time.frameCount) return false;

            var leftUpperArm = FindBone("upperarm_l");
            var rightUpperArm = FindBone("upperarm_r");
            if (leftUpperArm == null || rightUpperArm == null) return false;

            // Animator evaluation runs before LateUpdate. Preserve the
            // current clip-driven X/Y axes and overwrite only the imported
            // bind-pose Z axis, making this safe when the Animator is paused
            // and this method is reached again in a later frame.
            var leftEuler = leftUpperArm.localEulerAngles;
            var rightEuler = rightUpperArm.localEulerAngles;
            leftUpperArm.localRotation = Quaternion.Euler(leftEuler.x, leftEuler.y, 90f);
            rightUpperArm.localRotation = Quaternion.Euler(rightEuler.x, rightEuler.y, -90f);
            _lastNeutralArmCorrectionFrame = Time.frameCount;
            return true;
        }

        /// <summary>
        /// Applies the bounded generic-path Helm safety pose. The imported
        /// Helm clip keeps both arms horizontal, so the bridge owns a
        /// presentation-only shoulder drop while the D7/D2 systems retain
        /// the actual ship-control and physical-root decisions. Fish remains
        /// separate because its whole-body pitch has a different contract.
        /// </summary>
        public bool ApplyGenericPathHelmArmCorrection()
        {
            if (animator == null || !_initialized || IsPaused ||
                CurrentState != D20PlayerAnimationState.Helm)
                return false;
            if (_lastHelmArmCorrectionFrame == Time.frameCount) return false;

            var leftUpperArm = FindBone("upperarm_l");
            var rightUpperArm = FindBone("upperarm_r");
            if (leftUpperArm == null || rightUpperArm == null) return false;

            var leftEuler = leftUpperArm.localEulerAngles;
            var rightEuler = rightUpperArm.localEulerAngles;
            leftUpperArm.localRotation = Quaternion.Euler(leftEuler.x, leftEuler.y, 90f);
            rightUpperArm.localRotation = Quaternion.Euler(rightEuler.x, rightEuler.y, -90f);
            _lastHelmArmCorrectionFrame = Time.frameCount;
            return true;
        }

        /// <summary>
        /// Applies a bounded silhouette safety pose for the generic-path Fish
        /// clip. The generic clip pitches the visual root into the water but
        /// leaves the imported upper-arm bind pose too wide. This correction
        /// only changes the visual upper-arm Z axes, is idempotent per frame,
        /// and does not claim authored swimming or natural stroke timing.
        /// </summary>
        public bool ApplyGenericPathFishArmCorrection()
        {
            if (animator == null || !_initialized || IsPaused ||
                CurrentState != D20PlayerAnimationState.Fish)
                return false;
            if (_lastFishArmCorrectionFrame == Time.frameCount) return false;

            var leftUpperArm = FindBone("upperarm_l");
            var rightUpperArm = FindBone("upperarm_r");
            if (leftUpperArm == null || rightUpperArm == null) return false;

            var leftEuler = leftUpperArm.localEulerAngles;
            var rightEuler = rightUpperArm.localEulerAngles;
            leftUpperArm.localRotation = Quaternion.Euler(leftEuler.x, leftEuler.y, 90f);
            rightUpperArm.localRotation = Quaternion.Euler(rightEuler.x, rightEuler.y, -90f);
            _lastFishArmCorrectionFrame = Time.frameCount;
            return true;
        }

        public static bool UsesNeutralArmDrop(D20PlayerAnimationState state)
        {
            switch (state)
            {
                case D20PlayerAnimationState.Idle:
                case D20PlayerAnimationState.Walk:
                case D20PlayerAnimationState.Run:
                case D20PlayerAnimationState.Jump:
                case D20PlayerAnimationState.Fall:
                case D20PlayerAnimationState.Land:
                case D20PlayerAnimationState.Interact:
                case D20PlayerAnimationState.Talk:
                case D20PlayerAnimationState.Carry:
                case D20PlayerAnimationState.Board:
                case D20PlayerAnimationState.Disembark:
                case D20PlayerAnimationState.Hit:
                case D20PlayerAnimationState.Recover:
                    return true;
                default:
                    return false;
            }
        }

        private void ConfigureAnimator()
        {
            if (animator == null) return;
            animator.applyRootMotion = false;
            animator.cullingMode = _lodBand >= 2
                ? AnimatorCullingMode.CullCompletely
                : AnimatorCullingMode.CullUpdateTransforms;
        }

        private Transform FindBone(string name)
        {
            if (animator == null || string.IsNullOrEmpty(name)) return null;
            foreach (var candidate in animator.GetComponentsInChildren<Transform>(true))
                if (candidate != null && candidate.name == name) return candidate;
            return null;
        }

        private void PlayState(D20PlayerAnimationState state)
        {
            var desired = D20PlayerAnimationStateMap.ToStateName(state);
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                CurrentState = state;
                LastStateName = desired;
                LastApplyUsedFallback = true;
                return;
            }

            var fallback = false;
            if (!HasAnimatorState(desired))
            {
                desired = D20PlayerAnimationStateMap.ToStateName(D20PlayerAnimationState.Idle);
                fallback = true;
            }

            CurrentState = fallback ? D20PlayerAnimationState.Idle : state;
            LastStateName = desired;
            LastApplyUsedFallback = fallback;
            if (animator != null && animator.enabled && HasAnimatorState(desired))
                animator.CrossFadeInFixedTime(desired, 0.12f, 0, 0f);
        }

        private void SetFloat(string parameter, float value)
        {
            if (HasParameter(parameter, AnimatorControllerParameterType.Float)) animator.SetFloat(parameter, value);
        }

        private void SetBool(string parameter, bool value)
        {
            if (HasParameter(parameter, AnimatorControllerParameterType.Bool)) animator.SetBool(parameter, value);
        }

        private void SetInteger(string parameter, int value)
        {
            if (HasParameter(parameter, AnimatorControllerParameterType.Int)) animator.SetInteger(parameter, value);
        }

        private bool HasParameter(string parameter, AnimatorControllerParameterType type)
        {
            if (animator == null) return false;
            foreach (var item in animator.parameters)
                if (item.name == parameter && item.type == type) return true;
            return false;
        }
    }
}
