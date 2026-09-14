using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Whalebone.Gameplay.D28;

namespace Whalebone.Gameplay.D31
{
    /// <summary>
    /// D31-P04 的真实 Player 视觉证据探针。
    ///
    /// 探针只移动运行时主相机并调用 Unity 的 backbuffer 截图 API；它不写
    /// InitialHarbor 场景资产、天气事实、存档、航行状态或任何业务 owner。
    /// 每个截图在结果中只有在文件实际存在后才算 captured，避免把“请求截图”
    /// 错报成“截图已经落盘”。
    /// </summary>
    public sealed class D31VisualEvidenceProbe : MonoBehaviour
    {
        [Serializable]
        private sealed class CaptureRecord
        {
            public int index;
            public string shot_id;
            public string kind;
            public string scene;
            public string weather_requested;
            public string weather_observed;
            public string path;
            public int screen_width;
            public int screen_height;
            public bool capture_requested;
            public bool file_exists;
            public long file_bytes;
            public Vector3 camera_position;
            public Vector3 look_at;
            public string error;
        }

        [Serializable]
        private sealed class Result
        {
            public string schema = "D31_P04_VISUAL_EVIDENCE_PLAYER_V1";
            public string task = "D31-P04";
            public string attempt;
            public string evidence_mode = "real standalone Windows x64 Player; ScreenCapture backbuffer";
            public string unity_version;
            public string graphics_device;
            public string graphics_api;
            public int screen_width;
            public int screen_height;
            public int fixed_requested_count;
            public int fixed_captured_count;
            public int weather_requested_count;
            public int weather_captured_count;
            public bool all_requested_files_exist;
            public bool player_backbuffer_capture = true;
            public bool formal_scene_asset_modified;
            public string final_scene;
            public string final_weather;
            public CaptureRecord[] captures;
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

        private static readonly Shot[] FixedShots =
        {
            new Shot("d31_fixed_main_menu", Vector3.zero, Vector3.zero),
            // These coordinates belong to the current D19-R03 harbor frame
            // (roughly x=-16..16, z=-15..15). The old evidence set used a
            // retired world frame (z=60..104), which captured sky/empty land
            // once CameraFollow was correctly disabled for fixed shots.
            new Shot("d31_fixed_spawn", new Vector3(-10f, 4.8f, -15f), new Vector3(-1f, 1.15f, -8.5f)),
            new Shot("d31_fixed_market", new Vector3(15f, 5.5f, -7f), new Vector3(6.4f, 1.4f, -0.8f)),
            new Shot("d31_fixed_dock_overview", new Vector3(12f, 6f, -14f), new Vector3(-1f, 1f, -8.8f)),
            new Shot("d31_fixed_tavern_entry", new Vector3(16.5f, 5.5f, 14.5f), new Vector3(10.2f, 1.5f, 8.2f)),
            new Shot("d31_fixed_coastline", new Vector3(14f, 3f, -19f), new Vector3(0f, 1f, -11f)),
            new Shot("d31_fixed_boarding", new Vector3(7f, 4f, -15f), new Vector3(0f, 1.1f, -8.8f)),
            new Shot("d31_fixed_on_ship", new Vector3(5f, 5f, -10f), new Vector3(0f, 1.5f, 0f)),
            new Shot("d31_fixed_departure", new Vector3(0f, 9f, -22f), new Vector3(0f, 1f, -10f)),
            new Shot("d31_fixed_nearshore", new Vector3(15f, 5f, -18f), new Vector3(5f, 0f, -13f)),
            new Shot("d31_fixed_weather", new Vector3(-14f, 7f, 15f), new Vector3(0f, 2f, -8f)),
            new Shot("d31_fixed_return_harbor", new Vector3(14f, 5f, -12f), new Vector3(0f, 1f, -3f)),
            new Shot("d31_fixed_settings_accessibility", new Vector3(0f, 4f, -18f), new Vector3(0f, 2f, 0f)),
            new Shot("d31_fixed_debug_boundary", new Vector3(30f, 22f, 30f), new Vector3(0f, 0f, 0f)),
        };

        private static readonly string[] WeatherVariants = { "Calm", "Rain", "Storm" };
        private static readonly Shot WeatherShot = new Shot(
            "d31_weather_camera", new Vector3(-14f, 7f, 15f), new Vector3(0f, 2f, -8f));

        private readonly List<CaptureRecord> _captures = new List<CaptureRecord>();
        private Camera _camera;
        private CameraFollow _cameraFollow;
        private Result _result;
        private bool _started;
        private bool _wrote;
        private bool _cameraFollowWasEnabled;
        private bool _cameraFollowStateCaptured;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!HasFlag("-d31-visual-evidence")) return;
            if (FindFirstObjectByType<D31VisualEvidenceProbe>() != null) return;
            var host = new GameObject("D31VisualEvidenceProbe");
            DontDestroyOnLoad(host);
            host.AddComponent<D31VisualEvidenceProbe>();
        }

        private void Awake()
        {
            _result = new Result
            {
                attempt = ReadRunLabel(),
                unity_version = Application.unityVersion,
                graphics_device = SystemInfo.graphicsDeviceName,
                graphics_api = SystemInfo.graphicsDeviceType.ToString(),
                screen_width = Screen.width,
                screen_height = Screen.height,
                fixed_requested_count = FixedShots.Length,
                weather_requested_count = WeatherVariants.Length,
                formal_scene_asset_modified = false,
                final_scene = SceneManager.GetActiveScene().name,
                final_weather = "unknown",
            };
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            StartCoroutine(BeginAfterSceneReady());
        }

        private IEnumerator BeginAfterSceneReady()
        {
            yield return new WaitForSecondsRealtime(1f);
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
                StartCoroutine(CaptureHarborEvidence());
        }

        private IEnumerator CaptureMainMenuThenLoadHarbor()
        {
            _started = true;
            yield return new WaitForEndOfFrame();
            _camera = Camera.main;
            yield return CaptureShot(FixedShots[0], 1, "fixed", "MainMenu", "none");
            SceneManager.LoadScene("InitialHarbor");
            _started = false;
        }

        private IEnumerator CaptureHarborEvidence()
        {
            _started = true;
            yield return new WaitForSecondsRealtime(1f);
            _camera = Camera.main;
            if (_camera == null) _camera = FindFirstObjectByType<Camera>();
            CaptureAndDisableCameraFollow();

            D28WaterWeatherPresentationRuntime.SetWeatherPreview("Calm");
            yield return new WaitForSecondsRealtime(0.8f);
            for (var i = 1; i < FixedShots.Length; i++)
            {
                yield return MoveCamera(FixedShots[i]);
                yield return CaptureShot(FixedShots[i], i + 1, "fixed", "InitialHarbor", "Calm");
            }

            for (var i = 0; i < WeatherVariants.Length; i++)
            {
                var weather = WeatherVariants[i];
                D28WaterWeatherPresentationRuntime.SetWeatherPreview(weather);
                yield return new WaitForSecondsRealtime(1f);
                yield return MoveCamera(WeatherShot);
                yield return CaptureShot(WeatherShot, FixedShots.Length + i + 1,
                    "weather", "InitialHarbor", weather);
            }

            D28WaterWeatherPresentationRuntime.SetWeatherPreview("Calm");
            yield return new WaitForSecondsRealtime(0.5f);
            _result.final_scene = SceneManager.GetActiveScene().name;
            _result.final_weather = D28WaterWeatherPresentationRuntime.CurrentWeather.ToString();
            _result.captures = _captures.ToArray();
            _result.fixed_captured_count = CountCaptured("fixed");
            _result.weather_captured_count = CountCaptured("weather");
            _result.all_requested_files_exist = _captures.Count == FixedShots.Length + WeatherVariants.Length &&
                _captures.TrueForAll(record => record.file_exists);
            WriteResult();
            RestoreCameraFollow();
            Application.Quit();
        }

        private void CaptureAndDisableCameraFollow()
        {
            if (_camera == null) return;
            _cameraFollow = _camera.GetComponent<CameraFollow>();
            if (_cameraFollow == null) return;

            // Fixed-shot evidence owns the camera for the duration of this
            // opt-in probe. Otherwise CameraFollow.LateUpdate overwrites the
            // requested shot before the backbuffer is captured.
            _cameraFollowWasEnabled = _cameraFollow.enabled;
            _cameraFollowStateCaptured = true;
            _cameraFollow.enabled = false;
        }

        private void RestoreCameraFollow()
        {
            if (!_cameraFollowStateCaptured || _cameraFollow == null) return;
            _cameraFollow.enabled = _cameraFollowWasEnabled;
            _cameraFollowStateCaptured = false;
        }

        private IEnumerator MoveCamera(Shot shot)
        {
            if (_camera != null)
            {
                _camera.transform.position = shot.Position;
                var direction = shot.LookAt - shot.Position;
                if (direction.sqrMagnitude > 0.001f)
                    _camera.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
            yield return new WaitForSecondsRealtime(0.35f);
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator CaptureShot(Shot shot, int index, string kind, string scene, string requestedWeather)
        {
            var path = Path.Combine(GetOutputDirectory(), "capture_" + index.ToString("00") + "_" + shot.Id + ".png");
            var record = new CaptureRecord
            {
                index = index,
                shot_id = shot.Id,
                kind = kind,
                scene = scene,
                weather_requested = requestedWeather,
                weather_observed = ReadObservedWeather(),
                path = path.Replace('\\', '/'),
                screen_width = Screen.width,
                screen_height = Screen.height,
                camera_position = _camera == null ? Vector3.zero : _camera.transform.position,
                look_at = shot.LookAt,
                capture_requested = false,
                file_exists = false,
                file_bytes = 0,
                error = string.Empty,
            };

            Directory.CreateDirectory(GetOutputDirectory());
            try
            {
                ScreenCapture.CaptureScreenshot(path, 1);
                record.capture_requested = true;
            }
            catch (Exception exception)
            {
                record.error = exception.GetType().Name + ": " + exception.Message;
            }

            // CaptureScreenshot is asynchronous on Windows. Check the actual
            // file for several frames before recording the result.
            for (var attempt = 0; attempt < 12 && !record.file_exists; attempt++)
            {
                yield return new WaitForSecondsRealtime(0.15f);
                try
                {
                    if (File.Exists(path))
                    {
                        var info = new FileInfo(path);
                        record.file_exists = info.Length > 0;
                        record.file_bytes = info.Length;
                    }
                }
                catch (Exception exception)
                {
                    record.error = exception.GetType().Name + ": " + exception.Message;
                }
            }
            _captures.Add(record);
        }

        private int CountCaptured(string kind)
        {
            var count = 0;
            for (var i = 0; i < _captures.Count; i++)
                if (_captures[i].kind == kind && _captures[i].file_exists) count++;
            return count;
        }

        private string ReadObservedWeather()
        {
            try
            {
                return D28WaterWeatherPresentationRuntime.CurrentWeather.ToString();
            }
            catch (Exception exception)
            {
                return "unavailable:" + exception.GetType().Name;
            }
        }

        private static string GetOutputDirectory()
        {
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "d31_visual_evidence_" + ReadRunLabel());
        }

        private void WriteResult()
        {
            if (_wrote || _result == null) return;
            _wrote = true;
            var path = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "D31_P04_VISUAL_EVIDENCE_PLAYER_" + ReadRunLabel() + ".json");
            try
            {
                File.WriteAllText(path, JsonUtility.ToJson(_result, true));
                Debug.Log("[D31-P04] Visual evidence result written: " + path);
            }
            catch (Exception exception)
            {
                Debug.LogError("[D31-P04] Visual evidence result write failed: " + exception);
            }
        }

        private void OnApplicationQuit()
        {
            WriteResult();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            RestoreCameraFollow();
            WriteResult();
        }

        private static string ReadRunLabel()
        {
            const string prefix = "-d31-visual-evidence-run=";
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                var raw = args[i].Substring(prefix.Length).Trim();
                if (string.IsNullOrEmpty(raw)) break;
                var chars = raw.ToCharArray();
                for (var c = 0; c < chars.Length; c++)
                    if (!(char.IsLetterOrDigit(chars[c]) || chars[c] == '-' || chars[c] == '_')) chars[c] = '_';
                return new string(chars);
            }
            return "R14";
        }

        private static bool HasFlag(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
