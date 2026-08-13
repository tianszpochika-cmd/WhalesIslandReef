using System;
using System.Collections.Generic;
using UnityEngine;
using Whalebone.Systems.D16;

namespace Whalebone.Gameplay.D16
{
    /// <summary>
    /// 把一个表现对象登记到 D16 预算层。登记只影响 Renderer、Animator 和 AudioSource，
    /// 不禁用 Collider，也不写入领域状态。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class D16PresentationRegistration : MonoBehaviour
    {
        [SerializeField] private string _stableId;
        [SerializeField] private bool _isAgent;
        [SerializeField] private bool _isAnimal;
        [SerializeField] private bool _usesAudio;
        [SerializeField] private bool _playerRelevant;
        [SerializeField] private bool _activeInteraction;
        [SerializeField] private bool _presentationOnly = true;

        private Renderer[] _renderers;
        private Animator[] _animators;
        private AudioSource[] _audioSources;
        private LODGroup[] _lodGroups;
        private bool[] _baseRendererEnabled;
        private bool[] _baseAnimatorEnabled;
        private bool[] _baseAudioEnabled;
        private float[] _baseAnimatorSpeeds;

        public string StableId => string.IsNullOrWhiteSpace(_stableId) ? BuildHierarchyId() : _stableId.Trim();

        private void Awake()
        {
            EnsureCaches();
        }

        /// <summary>
        /// 仅供场景引导层为运行时生成的表现对象登记稳定语义，不改变领域状态。
        /// </summary>
        public void ConfigureRuntime(string stableId, bool isAgent, bool isAnimal, bool usesAudio,
            bool playerRelevant, bool activeInteraction)
        {
            _stableId = stableId;
            _isAgent = isAgent;
            _isAnimal = isAnimal;
            _usesAudio = usesAudio;
            _playerRelevant = playerRelevant;
            _activeInteraction = activeInteraction;
            _presentationOnly = true;
            EnsureCaches();
        }

        /// <summary>
        /// 判断运行时默认登记是否完整匹配，而不是只比较稳定 ID。
        /// 预制体可能已经带有正确 ID，却保留了错误的保护/类别标志；
        /// 这种情况必须重新接线，否则性能预算会把玩家或交互对象错误降级。
        /// </summary>
        public bool MatchesRuntimeConfiguration(string stableId, bool isAgent, bool isAnimal,
            bool usesAudio, bool playerRelevant, bool activeInteraction)
        {
            return string.Equals(StableId, stableId, StringComparison.Ordinal) &&
                   _isAgent == isAgent &&
                   _isAnimal == isAnimal &&
                   _usesAudio == usesAudio &&
                   _playerRelevant == playerRelevant &&
                   _activeInteraction == activeInteraction &&
                   _presentationOnly;
        }

        private void EnsureCaches()
        {
            if (_renderers != null) return;
            _renderers = GetComponentsInChildren<Renderer>(true);
            _animators = GetComponentsInChildren<Animator>(true);
            _audioSources = GetComponentsInChildren<AudioSource>(true);
            _lodGroups = GetComponentsInChildren<LODGroup>(true);
            _baseRendererEnabled = new bool[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _baseRendererEnabled[i] = _renderers[i] != null && _renderers[i].enabled;
            _baseAnimatorEnabled = new bool[_animators.Length];
            for (int i = 0; i < _animators.Length; i++)
            {
                _baseAnimatorEnabled[i] = _animators[i] != null && _animators[i].enabled;
            }
            _baseAudioEnabled = new bool[_audioSources.Length];
            for (int i = 0; i < _audioSources.Length; i++)
                _baseAudioEnabled[i] = _audioSources[i] != null && _audioSources[i].enabled;
            _baseAnimatorSpeeds = new float[_animators.Length];
            for (int i = 0; i < _animators.Length; i++) _baseAnimatorSpeeds[i] = _animators[i].speed;
        }

        public D16PerformanceCandidate BuildCandidate(Transform focus)
        {
            EnsureCaches();
            float distance = focus == null ? 0f : Vector3.Distance(transform.position, focus.position);
            return new D16PerformanceCandidate
            {
                Id = StableId,
                Distance = distance,
                IsVisible = IsVisibleFromMainCamera(),
                IsPlayerRelevant = _playerRelevant,
                HasActiveInteraction = _activeInteraction,
                IsAgent = _isAgent,
                IsAnimal = _isAnimal,
                UsesAudio = _usesAudio || _audioSources.Length > 0,
            };
        }

        public void Apply(D16PerformanceDecision decision)
        {
            EnsureCaches();
            if (decision == null || !_presentationOnly) return;
            float animatorScale = decision.SimulationTier == D16SimulationTier.Full ? 1f :
                decision.SimulationTier == D16SimulationTier.Reduced ? 0.5f :
                decision.SimulationTier == D16SimulationTier.Background ? 0.2f : 0f;
            for (int i = 0; i < _animators.Length; i++)
            {
                if (_animators[i] == null) continue;
                _animators[i].speed = _baseAnimatorSpeeds[i] * animatorScale;
                _animators[i].enabled = _baseAnimatorEnabled[i] && animatorScale > 0f;
            }

            ApplyLod(decision.SimulationTier);
            bool render = decision.SimulationTier != D16SimulationTier.Frozen;
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null)
                    _renderers[i].enabled = _baseRendererEnabled[i] && render;
            for (int i = 0; i < _audioSources.Length; i++)
                if (_audioSources[i] != null)
                    _audioSources[i].enabled = _baseAudioEnabled[i] && decision.AudioEnabled;
        }

        /// <summary>
        /// 将对象池取出的表现实例恢复为其原始表现状态。
        /// 对象池只能复用表现对象，不能把上一个租借者的 Frozen/LOD/静音状态带给下一个对象。
        /// </summary>
        public void ResetPresentationState()
        {
            EnsureCaches();
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null)
                    _renderers[i].enabled = _baseRendererEnabled[i];
            for (int i = 0; i < _animators.Length; i++)
            {
                if (_animators[i] == null) continue;
                _animators[i].speed = _baseAnimatorSpeeds[i];
                _animators[i].enabled = _baseAnimatorEnabled[i];
            }
            for (int i = 0; i < _audioSources.Length; i++)
                if (_audioSources[i] != null)
                    _audioSources[i].enabled = _baseAudioEnabled[i];
            foreach (var lodGroup in _lodGroups)
                if (lodGroup != null)
                    lodGroup.ForceLOD(-1);
        }

        private void ApplyLod(D16SimulationTier tier)
        {
            int requestedLod;
            switch (tier)
            {
                case D16SimulationTier.Reduced: requestedLod = 1; break;
                case D16SimulationTier.Background: requestedLod = 2; break;
                case D16SimulationTier.Frozen: requestedLod = 0; break;
                default: requestedLod = -1; break;
            }

            foreach (var lodGroup in _lodGroups)
            {
                if (lodGroup == null) continue;
                if (requestedLod < 0)
                {
                    lodGroup.ForceLOD(-1);
                    continue;
                }
                var lods = lodGroup.GetLODs();
                if (lods == null || lods.Length == 0) continue;
                lodGroup.ForceLOD(Mathf.Min(requestedLod, lods.Length - 1));
            }
        }

        private string BuildHierarchyId()
        {
            var parts = new List<string>();
            var current = transform;
            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }
            parts.Reverse();
            return "scene/" + string.Join("/", parts);
        }

        private bool IsVisibleFromMainCamera()
        {
            var camera = Camera.main;
            if (camera == null) return true;
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            var bounds = new Bounds(transform.position, Vector3.one);
            foreach (var renderer in _renderers)
            {
                if (renderer != null && GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) return true;
            }
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }
    }
}
