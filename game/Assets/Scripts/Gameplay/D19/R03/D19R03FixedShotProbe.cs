using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Gameplay.D19.R03
{
    /// <summary>
    /// Read-only real-Player fixed-camera evidence for D19-R03-A02. The probe
    /// moves only the presentation camera, captures the Player backbuffer and
    /// records the weather snapshot it observes. It never writes WeatherSystem,
    /// saves, stable anchors or scene assets.
    /// </summary>
    public sealed class D19R03FixedShotProbe : MonoBehaviour
    {
        [Serializable]
        private sealed class ShotRecord
        {
            public int index;
            public string shot_id;
            public string scene;
            public string path;
            public int screen_width;
            public int screen_height;
            public string observed_weather;
            public Vector3 position;
            public Vector3 look_at;
            public bool capture_requested;
        }

        [Serializable]
        private sealed class Result
        {
            public string schema = "D19_R03_A02_FIXED_SHOT_PLAYER_V1";
            public string task = "D19-R03-A02";
            public string attempt = "RERUN15";
            public string evidence_mode = "real standalone Windows x64 Player; ScreenCapture backbuffer";
            public string unity_version;
            public string graphics_device;
            public string graphics_api;
            public int screen_width;
            public int screen_height;
            public string active_weather_snapshot;
            public int requested_shot_count;
            public int captured_shot_count;
            public int weather_variant_count;
            public bool player_backbuffer_capture;
            public bool formal_scene_modified;
            public ShotRecord[] shots;
        }

        private struct Shot
        {
            public string Id;
            public Vector3 Position;
            public Vector3 LookAt;

            public Shot(string id, Vector3 position, Vector3 lookAt)
            {
                Id = id;
                Position = position;
                LookAt = lookAt;
            }
        }

        private readonly List<ShotRecord> _records = new List<ShotRecord>();
        private readonly List<Shot> _harborShots = new List<Shot>
        {
            // These coordinates are local to the current R03 island.  The
            // earlier probe still used the retired R01 world offsets and was
            // therefore either outside the scene or looking through the
            // harbor from an unintended angle.
            new Shot("d19_shot_spawn", new Vector3(0f, 7.8f, -18.5f), new Vector3(0f, 1.2f, 0f)),
            new Shot("d19_shot_market", new Vector3(16f, 7.5f, 4.5f), new Vector3(6.4f, 1.4f, 1.2f)),
            new Shot("d19_shot_dock_overview", new Vector3(13f, 8f, -8f), new Vector3(0f, 1.0f, -5f)),
            new Shot("d19_shot_tavern_entry", new Vector3(18.5f, 7.5f, 15.5f), new Vector3(10.2f, 1.5f, 8.5f)),
            new Shot("d19_shot_coastline", new Vector3(-18f, 5f, -14f), new Vector3(-4f, 0.8f, -10f)),
            new Shot("d19_shot_boarding", new Vector3(6f, 4f, -14f), new Vector3(0f, 1f, -7f)),
            new Shot("d19_shot_on_ship", new Vector3(0f, 5.5f, -18.5f), new Vector3(0f, 2f, -5.2f)),
            new Shot("d19_shot_departure", new Vector3(0f, 9f, -22f), new Vector3(0f, 1f, 0f)),
            new Shot("d19_shot_nearshore", new Vector3(18f, 6f, -14f), new Vector3(4f, 0f, -10f)),
            new Shot("d19_shot_weather", new Vector3(-16f, 8f, 12f), new Vector3(0f, 1f, 2f)),
            new Shot("d19_shot_return_harbor", new Vector3(14f, 7f, -14f), new Vector3(0f, 1f, -6f)),
            new Shot("d19_shot_settings_accessibility", new Vector3(0f, 3f, 18f), new Vector3(0f, 3f, 18f)),
            new Shot("d19_shot_debug_boundary", new Vector3(104f, 72f, 104f), new Vector3(0f, 0f, 0f)),
        };

        private Camera _camera;
        private Whalebone.Gameplay.CameraFollow _cameraFollow;
        private bool _cameraFollowWasEnabled;
        private bool _cameraFollowStateCaptured;
        private Result _result;
        private bool _started;
        private bool _finished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!HasFlag("-d19-r03-fixed-shots")) return;
            if (FindFirstObjectByType<D19R03FixedShotProbe>() != null) return;
            var host = new GameObject("D19R03FixedShotProbe");
            DontDestroyOnLoad(host);
            host.AddComponent<D19R03FixedShotProbe>();
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            _result = new Result
            {
                attempt = CurrentAttempt(),
                unity_version = Application.unityVersion,
                graphics_device = SystemInfo.graphicsDeviceName,
                graphics_api = SystemInfo.graphicsDeviceType.ToString(),
                screen_width = Screen.width,
                screen_height = Screen.height,
                requested_shot_count = 14,
                player_backbuffer_capture = true,
                formal_scene_modified = false,
            };
        }

        private void Start()
        {
            StartCoroutine(BeginAfterSceneReady());
        }

        private IEnumerator BeginAfterSceneReady()
        {
            yield return new WaitForSecondsRealtime(1.0f);
            BeginScene(SceneManager.GetActiveScene().name);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_started) StartCoroutine(BeginAfterSceneReady());
        }

        private void BeginScene(string sceneName)
        {
            if (_started) return;
            if (sceneName == "MainMenu")
            {
                StartCoroutine(CaptureMainMenuThenLoadHarbor());
                return;
            }
            if (sceneName == "InitialHarbor")
                StartCoroutine(CaptureHarborShots());
        }

        private IEnumerator CaptureMainMenuThenLoadHarbor()
        {
            _started = true;
            yield return new WaitForEndOfFrame();
            _camera = Camera.main;
            Capture("d19_shot_main_menu", 1, "MainMenu", "main_menu", null);
            SceneManager.LoadScene("InitialHarbor");
            _started = false;
        }

        private IEnumerator CaptureHarborShots()
        {
            _started = true;
            yield return new WaitForSecondsRealtime(1.0f);
            _camera = Camera.main;
            if (_camera == null) _camera = FindFirstObjectByType<Camera>();
            // The probe owns the camera only for this validation run.  The
            // normal follow component would overwrite each requested shot in
            // LateUpdate, producing screenshots of the spawn view while the
            // JSON claimed they were fixed compositions.
            _cameraFollow = _camera == null ? null : _camera.GetComponent<Whalebone.Gameplay.CameraFollow>();
            if (_cameraFollow == null)
                _cameraFollow = FindFirstObjectByType<Whalebone.Gameplay.CameraFollow>();
            if (_cameraFollow != null)
            {
                _cameraFollowWasEnabled = _cameraFollow.enabled;
                _cameraFollowStateCaptured = true;
                _cameraFollow.enabled = false;
            }

            for (int i = 0; i < _harborShots.Count; i++)
            {
                var shot = _harborShots[i];
                if (_camera != null)
                {
                    _camera.transform.position = shot.Position;
                    var direction = shot.LookAt - shot.Position;
                    if (direction.sqrMagnitude > 0.001f)
                        _camera.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                }
                yield return new WaitForSecondsRealtime(0.25f);
                yield return new WaitForEndOfFrame();
                Capture(shot.Id, i + 2, "InitialHarbor", shot.Id, shot);
            }

            // ScreenCapture writes on the following frame; keep the Player
            // alive long enough for the final backbuffer request to land.
            yield return new WaitForSecondsRealtime(0.5f);
            _result.captured_shot_count = _records.Count;
            _result.weather_variant_count = 0;
            _result.shots = _records.ToArray();
            _finished = true;
            WriteResult();
            Application.Quit();
        }

        private void Capture(string shotId, int index, string scene, string fileName, Shot? shot)
        {
            string baseDirectory = Directory.GetParent(Application.dataPath).FullName;
            string runDirectory = CurrentRunDirectory();
            string directory = Path.Combine(baseDirectory, runDirectory);
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "fixed_" + index.ToString("00") + "_" + fileName + ".png");
            string weather = "unknown";
            var bootstrap = FindFirstObjectByType<Whalebone.Gameplay.D9.D9VerticalSliceRuntimeBootstrap>();
            if (bootstrap != null && bootstrap.WeatherSnapshot != null)
                weather = bootstrap.WeatherSnapshot.Weather.ToString();
            try
            {
                ScreenCapture.CaptureScreenshot(path, 1);
                _records.Add(new ShotRecord
                {
                    index = index,
                    shot_id = shotId,
                    scene = scene,
                    path = path.Replace('\\', '/'),
                    screen_width = Screen.width,
                    screen_height = Screen.height,
                    observed_weather = weather,
                    position = shot.HasValue ? shot.Value.Position : (_camera == null ? Vector3.zero : _camera.transform.position),
                    look_at = shot.HasValue ? shot.Value.LookAt : Vector3.zero,
                    capture_requested = true,
                });
            }
            catch (Exception ex)
            {
                Debug.LogError("[D19-R03-A02] Fixed shot capture failed: " + ex.Message);
            }
        }

        private void WriteResult()
        {
            if (!_finished) return;
            string baseDirectory = Directory.GetParent(Application.dataPath).FullName;
            string resultName = CurrentResultName();
            string path = Path.Combine(baseDirectory, resultName);
            File.WriteAllText(path, JsonUtility.ToJson(_result, true));
            Debug.Log("[D19-R03-A02] Fixed shot result written: " + path);
        }

        private static string CurrentAttempt()
        {
            const string prefix = "-d19-r03-fixed-shots-rerun";
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg == null || !arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                var suffix = arg.Substring(prefix.Length);
                if (int.TryParse(suffix, out var attempt) && attempt > 0)
                    return "RERUN" + attempt.ToString();
            }
            return "RERUN15";
        }

        private static string CurrentRunDirectory()
        {
            return "fixed_cameras_rerun" + CurrentAttempt().Replace("RERUN", string.Empty);
        }

        private static string CurrentResultName()
        {
            return "D19_R03_A02_FIXED_SHOT_PLAYER_" + CurrentAttempt() + ".json";
        }

        private void OnDestroy()
        {
            if (_cameraFollowStateCaptured && _cameraFollow != null)
                _cameraFollow.enabled = _cameraFollowWasEnabled;
            _cameraFollowStateCaptured = false;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static bool HasFlag(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
