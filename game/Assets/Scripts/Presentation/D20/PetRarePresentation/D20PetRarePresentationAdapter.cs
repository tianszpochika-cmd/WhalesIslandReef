using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhalesIslandReef.Presentation.D20.PetRare
{
    /// <summary>
    /// Read-only visual adapter for pets and rare-animal observation candidates.
    ///
    /// The adapter writes only its own Transform, Renderer visibility, Animator playback and
    /// presentation anchors. It does not call domain writers, persistence writers, reward or
    /// relationship logic, or any network API.
    /// </summary>
    public sealed class D20PetRarePresentationAdapter : MonoBehaviour
    {
        [SerializeField] private string speciesId;
        [SerializeField] private string resourceKey;
        [SerializeField] private string presentationKind;
        [SerializeField] private bool rareObservationOnly;
        [SerializeField] private D20PetRareLodBand initialLodBand = D20PetRareLodBand.Near;
        [SerializeField] private GameObject remoteFallback;
        [SerializeField] private Transform followAnchor;
        [SerializeField] private Transform interactionAnchor;
        [SerializeField] private Transform boardingAnchor;
        [SerializeField] private Transform restAnchor;
        [SerializeField] private Transform discoveryHintAnchor;

        private Animator _animator;
        private Renderer[] _fullRenderers = Array.Empty<Renderer>();
        private Renderer[] _fallbackRenderers = Array.Empty<Renderer>();
        private D20PetRareLodBand _lodBand;
        private D20PetRareLocation _location;
        private D20PetRareVisualState _visualState;
        private D20PetRareDiscoveryFeedback _discoveryFeedback;
        private string _feedbackPromptKey = string.Empty;
        private string _lastAnimatorState = string.Empty;
        private bool _visible = true;
        private bool _available = true;

        public string SpeciesId => speciesId;
        public string ResourceKey => resourceKey;
        public string PresentationKind => presentationKind;
        public bool RareObservationOnly => rareObservationOnly;
        public D20PetRareLodBand CurrentLodBand => _lodBand;
        public D20PetRareLocation CurrentLocation => _location;
        public D20PetRareVisualState CurrentVisualState => _visualState;
        public D20PetRareDiscoveryFeedback LastDiscoveryFeedback => _discoveryFeedback;
        public string LastFeedbackPromptKey => _feedbackPromptKey;
        public string LastAnimatorState => _lastAnimatorState;
        public bool IsAvailable => _available;
        public bool IsFallbackVisible => remoteFallback != null && remoteFallback.activeSelf;
        public Transform FollowAnchor => followAnchor;
        public Transform InteractionAnchor => interactionAnchor;
        public Transform BoardingAnchor => boardingAnchor;
        public Transform RestAnchor => restAnchor;
        public Transform DiscoveryHintAnchor => discoveryHintAnchor;

        private void Awake()
        {
            CachePresentationParts();
            SetLodBand(initialLodBand);
        }

        private void OnValidate()
        {
            if (remoteFallback == gameObject)
                remoteFallback = null;
        }

        /// <summary>
        /// Editor-only construction hook. Runtime callers must use the read-only snapshot method.
        /// </summary>
        public void ConfigureForEditor(string stableSpeciesId, string stableResourceKey,
            string stablePresentationKind, bool observationOnly, GameObject fallback,
            Transform follow, Transform interact, Transform boarding, Transform rest,
            Transform discovery)
        {
            speciesId = stableSpeciesId ?? string.Empty;
            resourceKey = stableResourceKey ?? string.Empty;
            presentationKind = stablePresentationKind ?? string.Empty;
            rareObservationOnly = observationOnly;
            remoteFallback = fallback;
            followAnchor = follow;
            interactionAnchor = interact;
            boardingAnchor = boarding;
            restAnchor = rest;
            discoveryHintAnchor = discovery;
        }

        /// <summary>
        /// Applies a snapshot already decided by the owning system. The authoritative position
        /// is supplied by that system for follow, boarding, teleport and save-load recovery.
        /// </summary>
        public bool ApplyReadOnlySnapshot(D20PetRarePresentationInput input)
        {
            if (!string.IsNullOrWhiteSpace(input.speciesId) &&
                !string.Equals(input.speciesId, speciesId, StringComparison.Ordinal))
                return false;

            if (!string.IsNullOrWhiteSpace(input.resourceKey) &&
                !string.Equals(input.resourceKey, resourceKey, StringComparison.Ordinal))
                return false;

            transform.position = input.authoritativeWorldPosition;
            _location = input.location;
            _visualState = input.visualState;
            _available = input.available;
            _visible = input.present && input.available;
            _feedbackPromptKey = input.feedbackPromptKey ?? string.Empty;
            SetLodBand(input.lodBand);

            if (!_visible)
            {
                SetVisualState(D20PetRareVisualState.Unavailable, input.speed01);
                SetRenderers(_fullRenderers, false);
                SetRenderers(_fallbackRenderers, false);
                if (remoteFallback != null)
                    remoteFallback.SetActive(false);
                return true;
            }

            SetRenderers(_fullRenderers, _lodBand <= D20PetRareLodBand.Mid);
            SetRenderers(_fallbackRenderers, _lodBand >= D20PetRareLodBand.Far);
            if (remoteFallback != null)
                remoteFallback.SetActive(_lodBand >= D20PetRareLodBand.Far);
            SetVisualState(input.visualState, input.speed01);
            if (input.discoveryFeedback != D20PetRareDiscoveryFeedback.None)
                SetDiscoveryFeedback(input.discoveryFeedback, input.feedbackPromptKey);
            return true;
        }

        /// <summary>
        /// Visual recovery entry used after the owner reports a lost/found, boarding or save-load
        /// transition. It accepts the owner's authoritative position and never finds a target.
        /// </summary>
        public bool RecoverAtAuthoritativePosition(Vector3 position, D20PetRareLocation location,
            bool available, D20PetRareLodBand lodBand)
        {
            return ApplyReadOnlySnapshot(new D20PetRarePresentationInput
            {
                speciesId = speciesId,
                resourceKey = resourceKey,
                visualState = available ? D20PetRareVisualState.Idle : D20PetRareVisualState.Unavailable,
                location = location,
                authoritativeWorldPosition = position,
                speed01 = 0f,
                present = available,
                available = available,
                lodBand = lodBand,
                discoveryFeedback = D20PetRareDiscoveryFeedback.None,
                feedbackPromptKey = string.Empty,
            });
        }

        public void SetLodBand(D20PetRareLodBand band)
        {
            if (_fullRenderers == null || _fullRenderers.Length == 0)
                CachePresentationParts();

            _lodBand = band;
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);

            bool useFull = _visible && _available && band <= D20PetRareLodBand.Mid;
            bool useFallback = _visible && _available && band >= D20PetRareLodBand.Far &&
                remoteFallback != null;
            SetRenderers(_fullRenderers, useFull);
            SetRenderers(_fallbackRenderers, useFallback);
            if (remoteFallback != null)
                remoteFallback.SetActive(useFallback);

            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                _animator.cullingMode = useFull
                    ? AnimatorCullingMode.CullUpdateTransforms
                    : AnimatorCullingMode.CullCompletely;
                _animator.enabled = useFull;
            }
        }

        public void SetDiscoveryFeedback(D20PetRareDiscoveryFeedback feedback, string promptKey)
        {
            _discoveryFeedback = feedback;
            _feedbackPromptKey = promptKey ?? string.Empty;
            switch (feedback)
            {
                case D20PetRareDiscoveryFeedback.DiscoverySuccess:
                    SetVisualState(D20PetRareVisualState.DiscoverySuccess, 0f);
                    break;
                case D20PetRareDiscoveryFeedback.DiscoveryFailure:
                    SetVisualState(D20PetRareVisualState.DiscoveryFailure, 0f);
                    break;
                case D20PetRareDiscoveryFeedback.ObservationPrompt:
                    SetVisualState(D20PetRareVisualState.Observe, 0f);
                    break;
            }
        }

        public void SetVisualState(D20PetRareVisualState state, float speed01)
        {
            _visualState = state;
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);
            if (_animator == null || !_animator.enabled || _animator.runtimeAnimatorController == null)
                return;

            _animator.speed = Mathf.Clamp(speed01 <= 0f ? 1f : speed01, 0.25f, 2f);
            string stateName = D20PetRareStateMap.ToAnimatorState(state);
            int hash = Animator.StringToHash(stateName);
            if (!_animator.HasState(0, hash))
            {
                stateName = "Idle";
                hash = Animator.StringToHash(stateName);
            }

            if (_animator.HasState(0, hash))
            {
                if (!string.Equals(_lastAnimatorState, stateName, StringComparison.Ordinal))
                    _animator.Play(hash, 0, 0f);
                _lastAnimatorState = stateName;
            }
            else
            {
                _lastAnimatorState = "fallback_neutral";
            }
        }

        public void CachePresentationParts()
        {
            _animator = GetComponentInChildren<Animator>(true);
            if (followAnchor == null) followAnchor = FindChild("Sockets/FollowAnchor");
            if (interactionAnchor == null) interactionAnchor = FindChild("Sockets/InteractionAnchor");
            if (boardingAnchor == null) boardingAnchor = FindChild("Sockets/BoardingAnchor");
            if (restAnchor == null) restAnchor = FindChild("Sockets/RestAnchor");
            if (discoveryHintAnchor == null) discoveryHintAnchor = FindChild("Sockets/DiscoveryHintAnchor");

            var all = GetComponentsInChildren<Renderer>(true);
            var full = new List<Renderer>();
            var fallback = new List<Renderer>();
            foreach (var renderer in all)
            {
                if (remoteFallback != null && renderer.transform.IsChildOf(remoteFallback.transform))
                    fallback.Add(renderer);
                else
                    full.Add(renderer);
            }

            _fullRenderers = full.ToArray();
            _fallbackRenderers = fallback.ToArray();
        }

        private Transform FindChild(string path)
        {
            return transform.Find(path);
        }

        private static void SetRenderers(Renderer[] renderers, bool enabled)
        {
            if (renderers == null)
                return;
            foreach (var renderer in renderers)
                if (renderer != null) renderer.enabled = enabled;
        }
    }
}
