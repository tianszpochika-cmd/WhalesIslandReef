using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhalesIslandReef.Presentation.D20.Wildlife
{
    /// <summary>
    /// Read-only presentation input owned by AnimalSystem/EcologySystem.
    /// This DTO deliberately contains no population mutation, catch, rarity or reward fields.
    /// </summary>
    [Serializable]
    public struct D20WildlifePresentationInput
    {
        public string speciesId;
        public string behaviorState;
        public Vector3 worldPosition;
        [Range(0f, 1f)] public float speed01;
        public float waterDepthM;
        public D20WildlifeEcologyView ecology;
    }

    [Serializable]
    public struct D20WildlifeEcologyView
    {
        public bool present;
        [Range(0f, 1f)] public float visibility01;
        public int populationBand;
        public string observationStatus;
    }

    public enum D20WildlifeLodBand
    {
        Near = 0,
        Mid = 1,
        Far = 2,
        Proxy = 3
    }

    /// <summary>
    /// Stable display budget from D20-S01. It is a presentation guard, not an ecology cap.
    /// </summary>
    public static class D20WildlifePerformanceBudget
    {
        public const int LowAnimals1080p = 12;
        public const int MediumAnimals1080p = 24;
        public const int HighAnimals1080p = 32;

        public static int GetAnimalBudget(string quality)
        {
            switch ((quality ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "low": return LowAnimals1080p;
                case "high": return HighAnimals1080p;
                default: return MediumAnimals1080p;
            }
        }
    }

    /// <summary>
    /// Converts deterministic behavior facts to visual state names only.
    /// Missing clips intentionally fall back to neutral or Swim; no gameplay state is changed.
    /// </summary>
    public static class D20WildlifeStateMap
    {
        public static string ToAnimatorState(string behaviorState, bool aquatic)
        {
            string normalized = (behaviorState ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "forage":
                case "feed":
                case "eating":
                    return aquatic ? "Swim" : "Feed";
                case "travel":
                case "move":
                case "walk":
                    return aquatic ? "Swim" : "Move";
                case "alert":
                case "startle":
                case "observed":
                    return aquatic ? "Swim" : "Alert";
                case "flee":
                case "escape":
                    return aquatic ? "Swim" : "Flee";
                case "dive":
                    return aquatic ? "Dive" : "Idle";
                case "surface":
                    return aquatic ? "Surface" : "Idle";
                case "rest":
                case "injured":
                case "idle":
                default:
                    return aquatic ? "Swim" : "Idle";
            }
        }
    }

    /// <summary>
    /// Presentation-only adapter for a wildlife Prefab.
    /// It consumes a snapshot supplied by the owning system and never writes ecology facts.
    /// </summary>
    public sealed class D20WildlifePresentationAdapter : MonoBehaviour
    {
        [SerializeField] private string speciesId;
        [SerializeField] private string resourceKey;
        [SerializeField] private bool aquatic;
        [SerializeField] private float targetLengthM = 1f;
        [SerializeField] private float groundOffsetM;
        [SerializeField] private float waterDepthM;
        [SerializeField] private D20WildlifeLodBand initialLodBand = D20WildlifeLodBand.Near;
        [SerializeField] private GameObject distantFallback;

        private Animator _animator;
        private Renderer[] _fullRenderers = Array.Empty<Renderer>();
        private Renderer[] _fallbackRenderers = Array.Empty<Renderer>();
        private D20WildlifeLodBand _lodBand;
        private string _lastAnimatorState;
        private bool _visible = true;

        public string SpeciesId => speciesId;
        public string ResourceKey => resourceKey;
        public bool Aquatic => aquatic;
        public float TargetLengthM => targetLengthM;
        public float GroundOffsetM => groundOffsetM;
        public float WaterDepthM => waterDepthM;
        public D20WildlifeLodBand CurrentLodBand => _lodBand;
        public string LastAnimatorState => _lastAnimatorState;
        public bool IsFallbackVisible => distantFallback != null && distantFallback.activeSelf;

        private void Awake()
        {
            CachePresentationParts();
            SetLodBand(initialLodBand);
        }

        private void OnValidate()
        {
            targetLengthM = Mathf.Max(0.01f, targetLengthM);
            groundOffsetM = Mathf.Clamp(groundOffsetM, -10f, 10f);
            waterDepthM = Mathf.Clamp(waterDepthM, -100f, 100f);
        }

        public void ConfigureForEditor(string stableSpeciesId, string stableResourceKey, bool isAquatic,
            float lengthM, float groundOffset, float depthM, GameObject fallback)
        {
            speciesId = stableSpeciesId;
            resourceKey = stableResourceKey;
            aquatic = isAquatic;
            targetLengthM = Mathf.Max(0.01f, lengthM);
            groundOffsetM = groundOffset;
            waterDepthM = depthM;
            distantFallback = fallback;
        }

        /// <summary>
        /// Applies only visual state, placement and culling from an owner-provided snapshot.
        /// </summary>
        public void ApplyReadOnlySnapshot(D20WildlifePresentationInput input)
        {
            if (!string.IsNullOrWhiteSpace(input.speciesId) &&
                !string.Equals(input.speciesId, speciesId, StringComparison.Ordinal))
                return;

            transform.position = input.worldPosition;
            bool shouldShow = input.ecology.present && input.ecology.visibility01 > 0f;
            SetVisible(shouldShow);
            if (!shouldShow)
                return;

            TryApplyAnimatorState(input.behaviorState, input.speed01);
        }

        public void SetLodBand(D20WildlifeLodBand band)
        {
            if (_fullRenderers == null || _fullRenderers.Length == 0)
                CachePresentationParts();
            _lodBand = band;
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);

            bool useFull = band == D20WildlifeLodBand.Near || band == D20WildlifeLodBand.Mid;
            bool useFallback = !useFull && distantFallback != null;
            SetRenderers(_fullRenderers, useFull && _visible);
            SetRenderers(_fallbackRenderers, useFallback && _visible);
            if (distantFallback != null)
                distantFallback.SetActive(useFallback && _visible);

            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                _animator.cullingMode = useFull
                    ? AnimatorCullingMode.CullUpdateTransforms
                    : AnimatorCullingMode.CullCompletely;
                _animator.enabled = useFull;
            }
        }

        public void SetVisible(bool visible)
        {
            if (_fullRenderers == null || _fullRenderers.Length == 0)
                CachePresentationParts();
            _visible = visible;
            SetRenderers(_fullRenderers, visible && (_lodBand == D20WildlifeLodBand.Near || _lodBand == D20WildlifeLodBand.Mid));
            SetRenderers(_fallbackRenderers, visible && _lodBand >= D20WildlifeLodBand.Far);
            if (distantFallback != null)
                distantFallback.SetActive(visible && _lodBand >= D20WildlifeLodBand.Far);
        }

        public void CachePresentationParts()
        {
            _animator = GetComponentInChildren<Animator>(true);
            var all = GetComponentsInChildren<Renderer>(true);
            var full = new List<Renderer>();
            var fallback = new List<Renderer>();
            foreach (var renderer in all)
            {
                if (distantFallback != null && renderer.transform.IsChildOf(distantFallback.transform))
                    fallback.Add(renderer);
                else
                    full.Add(renderer);
            }

            _fullRenderers = full.ToArray();
            _fallbackRenderers = fallback.ToArray();
        }

        private void TryApplyAnimatorState(string behaviorState, float speed01)
        {
            if (_animator == null || !_animator.enabled || _animator.runtimeAnimatorController == null)
                return;

            string state = D20WildlifeStateMap.ToAnimatorState(behaviorState, aquatic);
            _animator.speed = Mathf.Clamp(speed01 <= 0f ? 1f : speed01, 0.25f, 2f);
            int stateHash = Animator.StringToHash(state);
            if (!_animator.HasState(0, stateHash))
            {
                state = aquatic ? "Swim" : "Idle";
                stateHash = Animator.StringToHash(state);
            }

            if (_animator.HasState(0, stateHash))
            {
                if (!string.Equals(_lastAnimatorState, state, StringComparison.Ordinal))
                    _animator.Play(stateHash, 0, 0f);
                _lastAnimatorState = state;
            }
            else
            {
                _lastAnimatorState = "fallback_neutral";
            }
        }

        private static void SetRenderers(Renderer[] renderers, bool enabled)
        {
            if (renderers == null)
                return;
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = enabled;
            }
        }
    }
}
