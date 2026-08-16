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
            new Shot("d19_shot_spawn", new Vector3(-22f, 3.2f, 82f), new Vector3(-22f, 1f, 66f)),
            new Shot("d19_shot_market", new Vector3(42f, 5f, 74f), new Vector3(26f, 3f, 62f)),
            new Shot("d19_shot_dock_overview", new Vector3(14f, 7f, 4f), new Vector3(0f, 1f, -22f)),
            new Shot("d19_shot_tavern_entry", new Vector3(-58f, 4f, 58f), new Vector3(-42f, 2f, 50f)),
            new Shot("d19_shot_coastline", new Vector3(-86f, 3f, -2f), new Vector3(-58f, 1f, -8f)),
            new Shot("d19_shot_boarding", new Vector3(6f, 3.2f, -15f), new Vector3(0f, 1f, -20f)),
            new Shot("d19_shot_on_ship", new Vector3(0f, 5f, -28f), new Vector3(0f, 2f, -18f)),
            new Shot("d19_shot_departure", new Vector3(0f, 8f, -42f), new Vector3(0f, 1f, -78f)),
            new Shot("d19_shot_nearshore", new Vector3(18f, 4f, -66f), new Vector3(0f, -1f, -88f)),
            new Shot("d19_shot_weather", new Vector3(-18f, 6f, 18f), new Vector3(0f, 3f, -20f)),
            new Shot("d19_shot_return_harbor", new Vector3(8f, 5f, -52f), new Vector3(0f, 1f, -12f)),
            new Shot("d19_shot_settings_accessibility", new Vector3(0f, 3f, 60f), new Vector3(0f, 3f, 60f)),
            new Shot("d19_shot_debug_boundary", new Vector3(104f, 72f, 104f), new Vector3(0f, 0f, 0f)),
        };

        private Camera _camera;
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
                attempt = HasFlag("-d19-r03-fixed-shots-rerun17") ? "RERUN17" : HasFlag("-d19-r03-fixed-shots-rerun16") ? "RERUN16" : "RERUN15",
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
            string runDirectory = HasFlag("-d19-r03-fixed-shots-rerun17") ? "fixed_cameras_rerun17" : HasFlag("-d19-r03-fixed-shots-rerun16") ? "fixed_cameras_rerun16" : "fixed_cameras_rerun15";
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
            string resultName = HasFlag("-d19-r03-fixed-shots-rerun17")
                ? "D19_R03_A02_FIXED_SHOT_PLAYER_RERUN17.json"
                : HasFlag("-d19-r03-fixed-shots-rerun16")
                ? "D19_R03_A02_FIXED_SHOT_PLAYER_RERUN16.json"
                : "D19_R03_A02_FIXED_SHOT_PLAYER_RERUN15.json";
            string path = Path.Combine(baseDirectory, resultName);
            File.WriteAllText(path, JsonUtility.ToJson(_result, true));
            Debug.Log("[D19-R03-A02] Fixed shot result written: " + path);
        }

        private void OnDestroy()
        {
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
