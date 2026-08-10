// ============================================================
// D7AudioController —— 首航音频通道与安全兜底
//
// 外部授权音频接入前，运行时生成极低音量的环境/脚步/船舶提示音，
// 让动作和控制状态有听觉反馈；正式 AudioClip 可直接替换同名通道。
// ============================================================

using UnityEngine;
using Whalebone.Gameplay.D2;
using Whalebone.Systems.D2;

namespace Whalebone.Gameplay.D7
{
    public sealed class D7AudioController : MonoBehaviour
    {
        private AudioSource _ambient;
        private AudioSource _shipLoop;
        private AudioSource _oneShot;
        private AudioClip _ambientClip;
        private AudioClip _footstepClip;
        private AudioClip _boardingClip;
        private AudioClip _shipClip;
        private D2CharacterController _character;
        private D2ControlRouter _router;
        private D2ShipController _ship;
        private D2ControlMode _lastMode = D2ControlMode.Character;
        private float _nextStep;

        public bool IsReady { get; private set; }
        public bool UsesFallbackClips { get; private set; }
        public int OneShotCount { get; private set; }

        private void Awake()
        {
            _ambient = CreateSource("D7_AmbientSource", true, 0.035f);
            _shipLoop = CreateSource("D7_ShipSource", true, 0f);
            _oneShot = CreateSource("D7_OneShotSource", false, 0.12f);
            _ambientClip = LoadOrCreate("harbor_ambient", 2.4f, 165f, 0.20f);
            _footstepClip = LoadOrCreate("footstep", 0.10f, 92f, 0.70f);
            _boardingClip = LoadOrCreate("boarding", 0.24f, 240f, 0.50f);
            _shipClip = LoadOrCreate("ship_movement", 1.2f, 58f, 0.22f);

            _ambient.clip = _ambientClip;
            _ambient.Play();
            IsReady = _ambient != null && _shipLoop != null && _oneShot != null &&
                      _ambientClip != null && _footstepClip != null && _boardingClip != null && _shipClip != null;
            Debug.Log("[D7/Audio] 音频通道已就绪：港口环境、脚步、上下船、船舶移动，fallback=" + UsesFallbackClips);
        }

        private void Update()
        {
            BindRuntimeObjects();
            if (!IsReady || _router == null) return;

            if (_router.Mode != _lastMode)
            {
                if (_router.Mode == D2ControlMode.Ship || _lastMode == D2ControlMode.Ship)
                    PlayOneShot(_boardingClip);
                _lastMode = _router.Mode;
            }

            if (_router.Mode == D2ControlMode.Character && _character != null &&
                _character.IsGrounded && _character.IsMoving &&
                Time.time >= _nextStep && D7ControlCoordinator.GameplayInputAllowed)
            {
                _nextStep = Time.time + (_character.IsRunning ? 0.24f : 0.38f);
                PlayOneShot(_footstepClip);
            }

            bool shipActive = _router.Mode == D2ControlMode.Ship && _ship != null;
            if (!shipActive)
            {
                _shipLoop.Stop();
                return;
            }

            if (_shipLoop.clip != _shipClip) _shipLoop.clip = _shipClip;
            if (!_shipLoop.isPlaying) _shipLoop.Play();
            float maxSpeed = Mathf.Max(0.1f, _ship.Snapshot.MaxSpeed);
            _shipLoop.volume = Mathf.Lerp(0.015f, 0.08f, Mathf.Clamp01(_ship.Snapshot.Speed / maxSpeed));
        }

        private void BindRuntimeObjects()
        {
            if (_router == null) _router = FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            if (_character == null)
            {
                var player = GameObject.Find("D2_PlayerAvatar");
                if (player != null) _character = player.GetComponent<D2CharacterController>();
            }
            if (_ship == null)
            {
                var ship = GameObject.Find("ph_ship_fishing");
                if (ship != null) _ship = ship.GetComponent<D2ShipController>();
            }

            var camera = Camera.main;
            if (camera == null) camera = FindFirstObjectByType<Camera>();
            if (camera != null && camera.GetComponent<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();
        }

        private AudioSource CreateSource(string name, bool loop, float volume)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            return source;
        }

        private AudioClip LoadOrCreate(string resourceName, float seconds, float frequency, float amplitude)
        {
            var loaded = Resources.Load<AudioClip>("D7/Audio/" + resourceName);
            if (loaded != null) return loaded;

            UsesFallbackClips = true;
            int sampleRate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(seconds * sampleRate));
            var clip = AudioClip.Create("D7_Fallback_" + resourceName, sampleCount, 1, sampleRate, false);
            var samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Clamp01(1f - t / Mathf.Max(0.01f, seconds));
                float tone = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude * envelope;
                float undertone = Mathf.Sin(2f * Mathf.PI * frequency * 0.5f * t) * amplitude * 0.18f * envelope;
                samples[i] = Mathf.Clamp(tone + undertone, -1f, 1f);
            }
            clip.SetData(samples, 0);
            return clip;
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (_oneShot == null || clip == null) return;
            _oneShot.PlayOneShot(clip);
            OneShotCount++;
        }
    }
}
