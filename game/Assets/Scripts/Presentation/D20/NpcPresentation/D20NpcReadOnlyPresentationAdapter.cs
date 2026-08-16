using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhalesIslandReef.Presentation.D20.NpcPresentation
{
    /// <summary>
    /// NPC behavior-to-visual adapter. All input arrives as a read-only snapshot.
    /// No domain system, event log, save writer, Agent or network dependency is used.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class D20NpcReadOnlyPresentationAdapter : MonoBehaviour
    {
        [SerializeField] private string npcId;
        [SerializeField] private string resourceKey;
        [SerializeField] private D20NpcJobPost defaultJobPost;
        [SerializeField] private D20NpcLodBand initialLodBand = D20NpcLodBand.Near;
        [SerializeField] private GameObject distantFallback;
        [SerializeField] private D20NpcFeedbackBillboard feedbackBillboard;

        private Animator _animator;
        private Renderer[] _fullRenderers = Array.Empty<Renderer>();
        private Renderer[] _fallbackRenderers = Array.Empty<Renderer>();
        private Transform _visualRoot;
        private Vector3 _visualBasePosition;
        private Quaternion _visualBaseRotation;
        private D20NpcLodBand _lodBand;
        private D20NpcPresentationSnapshot _lastSnapshot;
        private bool _hasSnapshot;
        private bool _visible = true;
        private float _posePhase;

        public string NpcId => npcId;
        public string ResourceKey => resourceKey;
        public D20NpcJobPost DefaultJobPost => defaultJobPost;
        public D20NpcLodBand CurrentLodBand => _lodBand;
        public D20NpcVisualAction CurrentVisualAction { get; private set; } = D20NpcVisualAction.NeutralFallback;
        public D20NpcPresentationFeedback LastFeedback { get; private set; }
        public D20NpcPresentationSnapshot LastSnapshot => _lastSnapshot;
        public bool HasSnapshot => _hasSnapshot;
        public bool NeedsSnapshotResync { get; private set; }
        public bool IsPaused => _hasSnapshot && _lastSnapshot.paused;
        public bool IsFallbackVisible => distantFallback != null && distantFallback.activeSelf;
        public event Action<D20NpcPresentationFeedback> FeedbackChanged;

        private void Awake()
        {
            CachePresentationParts();
            LastFeedback = D20NpcPresentationFeedback.Hidden();
            SetLodBand(initialLodBand);
        }

        private void OnEnable()
        {
            if (_hasSnapshot)
            {
                ApplyReadOnlySnapshot(_lastSnapshot);
                NeedsSnapshotResync = false;
            }
        }

        private void OnDisable()
        {
            // Scene unload/disable never clears the owner snapshot. The next instance
            // or re-enable must receive/apply the same read model again.
            NotifyPresentationSuspended();
        }

        private void LateUpdate()
        {
            if (!_hasSnapshot || !_visible || _lodBand >= D20NpcLodBand.Far || _lastSnapshot.paused)
                return;

            _posePhase += Time.deltaTime * Mathf.Lerp(0.8f, 1.4f, Mathf.Clamp01(_lastSnapshot.speed01));
            ApplyNeutralPoseFallback(CurrentVisualAction, _posePhase);
        }

        public void ConfigureForEditor(
            string stableNpcId,
            string stableResourceKey,
            D20NpcJobPost jobPost,
            GameObject fallback,
            D20NpcFeedbackBillboard billboard)
        {
            npcId = stableNpcId;
            resourceKey = stableResourceKey;
            defaultJobPost = jobPost;
            distantFallback = fallback;
            feedbackBillboard = billboard;
            CachePresentationParts();
        }

        /// <summary>
        /// Applies position, action, feedback and LOD from an owner-provided snapshot.
        /// This method has no result-side effects beyond visual hierarchy changes.
        /// </summary>
        public bool ApplyReadOnlySnapshot(D20NpcPresentationSnapshot snapshot)
        {
            if (!string.IsNullOrEmpty(npcId) &&
                !string.IsNullOrEmpty(snapshot.npcId) &&
                !string.Equals(npcId, snapshot.npcId, StringComparison.Ordinal))
                return false;

            _lastSnapshot = snapshot;
            _hasSnapshot = true;
            NeedsSnapshotResync = false;
            transform.position = snapshot.worldPosition;
            SetLodBand(snapshot.lodBand);

            D20NpcVisualAction requestedAction = D20NpcPresentationStateMap.ToVisualAction(snapshot);
            bool fallbackUsed = !TryApplyAnimatorState(requestedAction, snapshot);
            CurrentVisualAction = fallbackUsed ? D20NpcVisualAction.NeutralFallback : requestedAction;
            ApplyNeutralPoseFallback(CurrentVisualAction, _posePhase);

            LastFeedback = D20NpcPresentationStateMap.ToFeedback(snapshot, fallbackUsed);
            if (feedbackBillboard != null)
                feedbackBillboard.Apply(LastFeedback);
            FeedbackChanged?.Invoke(LastFeedback);
            return true;
        }

        /// <summary>
        /// Explicit lifecycle hook for scene unload/pause coordinators. It only marks
        /// the cached read model stale; the owner remains responsible for the next snapshot.
        /// </summary>
        public void NotifyPresentationSuspended()
        {
            NeedsSnapshotResync = _hasSnapshot;
        }

        /// <summary>
        /// Re-applies the last owner snapshot after a scene/presentation reload.
        /// No domain state is reconstructed or written here.
        /// </summary>
        public bool NotifyPresentationResumed()
        {
            if (!_hasSnapshot)
                return false;
            bool applied = ApplyReadOnlySnapshot(_lastSnapshot);
            NeedsSnapshotResync = !applied;
            return applied;
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            SetRenderers(_fullRenderers, visible && _lodBand <= D20NpcLodBand.Mid);
            SetRenderers(_fallbackRenderers, visible && _lodBand >= D20NpcLodBand.Far);
            if (distantFallback != null)
                distantFallback.SetActive(visible && _lodBand >= D20NpcLodBand.Far);
            if (feedbackBillboard != null && _lodBand >= D20NpcLodBand.Far)
                feedbackBillboard.SetVisible(false);
        }

        public void SetLodBand(D20NpcLodBand band)
        {
            if (_fullRenderers == null || _fullRenderers.Length == 0)
                CachePresentationParts();

            _lodBand = band;
            bool useFull = band == D20NpcLodBand.Near || band == D20NpcLodBand.Mid;
            bool useFallback = band == D20NpcLodBand.Far || band == D20NpcLodBand.Proxy;
            SetRenderers(_fullRenderers, _visible && useFull);
            SetRenderers(_fallbackRenderers, _visible && useFallback);
            if (distantFallback != null)
                distantFallback.SetActive(_visible && useFallback);

            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                _animator.cullingMode = useFull
                    ? AnimatorCullingMode.CullUpdateTransforms
                    : AnimatorCullingMode.CullCompletely;
                _animator.enabled = useFull;
            }

            if (feedbackBillboard != null)
                feedbackBillboard.SetVisible(_visible && useFull && LastFeedback.visible);
        }

        public bool TryGetVisualAnchor(D20NpcJobPost jobPost, out D20NpcPresentationAnchor anchor)
        {
            var anchors = GetComponentsInChildren<D20NpcPresentationAnchor>(true);
            string requiredId = D20NpcPresentationAnchorIds.ForJob(jobPost);
            for (int i = 0; i < anchors.Length; i++)
            {
                if (anchors[i] != null &&
                    string.Equals(anchors[i].AnchorId, requiredId, StringComparison.Ordinal))
                {
                    anchor = anchors[i];
                    return true;
                }
            }

            anchor = null;
            return false;
        }

        public void CachePresentationParts()
        {
            _animator = GetComponentInChildren<Animator>(true);
            _visualRoot = FindVisualRoot();
            if (_visualRoot != null)
            {
                _visualBasePosition = _visualRoot.localPosition;
                _visualBaseRotation = _visualRoot.localRotation;
            }

            var all = GetComponentsInChildren<Renderer>(true);
            var full = new List<Renderer>();
            var fallback = new List<Renderer>();
            for (int i = 0; i < all.Length; i++)
            {
                if (distantFallback != null && all[i].transform.IsChildOf(distantFallback.transform))
                    fallback.Add(all[i]);
                else
                    full.Add(all[i]);
            }

            _fullRenderers = full.ToArray();
            _fallbackRenderers = fallback.ToArray();
        }

        private bool TryApplyAnimatorState(
            D20NpcVisualAction requestedAction,
            D20NpcPresentationSnapshot snapshot)
        {
            if (_animator == null)
                return false;

            _animator.applyRootMotion = false;
            SetFloat("D20_Speed01", Mathf.Clamp01(snapshot.speed01));
            SetBool("D20_Grounded", snapshot.grounded);
            SetBool("D20_Injured", snapshot.injured || snapshot.behaviorState == D20NpcBehaviorState.Injured);
            SetBool("D20_Paused", snapshot.paused);
            SetInt("D20_LodBand", (int)snapshot.lodBand);
            SetInt("D20_ActionId", (int)requestedAction);
            SetInt("D20_InteractionKind", snapshot.dialogueOpen ? 2 : 0);

            if (!_animator.enabled || _animator.runtimeAnimatorController == null)
                return false;

            string stateName = D20NpcPresentationStateMap.AnimatorStateName(requestedAction);
            int hash = Animator.StringToHash(stateName);
            if (!_animator.HasState(0, hash))
                return false;

            _animator.Play(hash, 0, 0f);
            return true;
        }

        private void ApplyNeutralPoseFallback(D20NpcVisualAction action, float phase)
        {
            if (_visualRoot == null || _lodBand >= D20NpcLodBand.Far || IsPaused)
                return;

            float bob = 0f;
            float lean = 0f;
            switch (action)
            {
                case D20NpcVisualAction.Work: bob = Mathf.Sin(phase * 1.6f) * 0.012f; lean = 2.5f; break;
                case D20NpcVisualAction.Walk: bob = Mathf.Abs(Mathf.Sin(phase * 3f)) * 0.018f; lean = 2f; break;
                case D20NpcVisualAction.Talk: bob = Mathf.Sin(phase * 1.2f) * 0.008f; lean = -1.5f; break;
                case D20NpcVisualAction.Rest: bob = Mathf.Sin(phase * 0.7f) * 0.004f; lean = -3f; break;
                case D20NpcVisualAction.Eat: bob = Mathf.Sin(phase * 1.5f) * 0.006f; lean = 5f; break;
                case D20NpcVisualAction.Alarm: lean = -4f; break;
                case D20NpcVisualAction.Refuse: lean = -2f; break;
                case D20NpcVisualAction.Injured: lean = 7f; break;
            }

            _visualRoot.localPosition = _visualBasePosition + Vector3.up * bob;
            _visualRoot.localRotation = _visualBaseRotation * Quaternion.AngleAxis(lean, Vector3.right);
        }

        private Transform FindVisualRoot()
        {
            var transforms = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (string.Equals(transforms[i].name, "Visual", StringComparison.Ordinal)) return transforms[i];
            return transform.childCount > 0 ? transform.GetChild(0) : transform;
        }

        private void SetFloat(string parameter, float value)
        {
            if (HasParameter(parameter, AnimatorControllerParameterType.Float)) _animator.SetFloat(parameter, value);
        }

        private void SetBool(string parameter, bool value)
        {
            if (HasParameter(parameter, AnimatorControllerParameterType.Bool)) _animator.SetBool(parameter, value);
        }

        private void SetInt(string parameter, int value)
        {
            if (HasParameter(parameter, AnimatorControllerParameterType.Int)) _animator.SetInteger(parameter, value);
        }

        private bool HasParameter(string parameter, AnimatorControllerParameterType type)
        {
            if (_animator == null) return false;
            var parameters = _animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i].name == parameter && parameters[i].type == type) return true;
            return false;
        }

        private static void SetRenderers(Renderer[] renderers, bool enabled)
        {
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].enabled = enabled;
        }
    }
}
