using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Whalebone.Gameplay.D2;

namespace Whalebone.Gameplay.D19.R03
{
    /// <summary>
    /// Read-only runtime evidence probe for D19-R03-A01.
    /// It is installed only when the standalone Player is launched with
    /// -d19-r03-route. It records physical transform/input observations and
    /// never invokes gameplay methods or writes game/save state.
    /// </summary>
    public sealed class D19R03PlayerRouteProbe : MonoBehaviour
    {
        [Serializable]
        private sealed class LandmarkRecord
        {
            public string stable_id;
            public string object_name;
            public bool found;
            public float x;
            public float y;
            public float z;
            public float first_reached_seconds = -1f;
            public float reach_radius;
        }

        [Serializable]
        private sealed class Observation
        {
            public float elapsed_seconds;
            public float realtime_seconds;
            public float player_x;
            public float player_y;
            public float player_z;
            public float ship_x;
            public float ship_y;
            public float ship_z;
            public bool ship_found;
            public bool player_overlaps_ship_hull;
            public bool grounded;
            public bool moving;
            public bool running;
            public float speed;
            public float vertical_velocity;
            public string control_mode;
            public string feedback_code;
            public string feedback_target;
            public bool key_w;
            public bool key_a;
            public bool key_s;
            public bool key_d;
            public bool key_e;
            public bool key_b;
            public bool key_t;
            public bool key_v;
            public bool key_f5;
            public bool key_c;
            public bool key_escape;
        }

        [Serializable]
        private sealed class EventRecord
        {
            public float elapsed_seconds;
            public string type;
            public string detail;
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public string schema = "D19_R03_A01_RUNTIME_ROUTE_PROBE_V1";
            public string task = "D19-R03-A01";
            public string evidence_mode = "read-only runtime observation; no Inspector or direct gameplay method invocation";
            public string scene;
            public string player_object;
            public string router_object;
            public float duration_seconds;
            public bool player_found;
            public bool controller_found;
            public bool router_found;
            public bool moved_observed;
            public bool interaction_input_observed;
            public bool save_input_observed;
            public LandmarkRecord[] landmarks;
            public Observation[] observations;
            public EventRecord[] events;
        }

        private const float SampleInterval = 0.25f;
        private const float LandmarkRadius = 3.5f;
        private readonly List<Observation> _observations = new List<Observation>();
        private readonly List<EventRecord> _events = new List<EventRecord>();
        private LandmarkRecord[] _landmarks;
        private D2CharacterController _controller;
        private D2ControlRouter _router;
        private Transform _player;
        private Transform _ship;
        private Collider _shipHull;
        private float _startedAt;
        private float _nextSampleAt;
        private float _nextScreenshotAt = 5f;
        private int _screenshotIndex;
        private Vector3 _firstPosition;
        private bool _wrote;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!HasAnyRouteFlag()) return;
            if (FindObjectOfType<D19R03PlayerRouteProbe>() != null) return;
            var host = new GameObject("D19R03PlayerRouteProbe");
            DontDestroyOnLoad(host);
            host.AddComponent<D19R03PlayerRouteProbe>();
        }

        private static bool HasFlag(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private void Start()
        {
            Debug.Log("[D19-R03-A01] Route probe component started; args=" + string.Join(" ", Environment.GetCommandLineArgs()));
            if (!HasAnyRouteFlag())
            {
                Debug.Log("[D19-R03-A01] Route probe disabled because no route flag was supplied");
                enabled = false;
                return;
            }
            _startedAt = Time.realtimeSinceStartup;
            _nextSampleAt = 0f;
            _landmarks = CreateLandmarks();
            AddEvent("probe_started", "D19-R03-A01 read-only route probe installed");
        }

        private static bool HasAnyRouteFlag()
        {
            return HasFlag("-d19-r03-route") || HasFlag("-d19-r03-route-rerun2") ||
                   HasFlag("-d19-r03-route-rerun3") || HasFlag("-d19-r03-route-rerun4") ||
                   HasFlag("-d19-r03-route-rerun5") || HasFlag("-d19-r03-route-rerun6") ||
                   HasFlag("-d19-r03-route-rerun7") || HasFlag("-d19-r03-route-rerun8") ||
                   HasFlag("-d19-r03-route-rerun9") || HasFlag("-d19-r03-route-rerun10") ||
                   HasFlag("-d19-r03-route-rerun29") ||
                   HasFlag("-d19-r03-route-rerun41") ||
                   HasFlag("-d19-r03-route-rerun42") ||
                   HasFlag("-d19-r03-route-rerun43") ||
                   HasFlag("-d19-r03-route-rerun46") ||
                   HasFlag("-d19-r03-route-rerun45") ||
                   HasFlag("-d19-r03-route-rerun44") ||
                   HasFlag("-d19-r03-route-control-test");
        }

        private void Update()
        {
            if (_wrote) return;
            ResolveRuntimeObjects();
            var elapsed = Time.realtimeSinceStartup - _startedAt;
            PollInputEdges();
            if (elapsed + 0.001f < _nextSampleAt) return;
            _nextSampleAt = elapsed + SampleInterval;
            Capture(elapsed);
        }

        private void PollInputEdges()
        {
            // Poll every frame so a short real Player key edge cannot be missed by
            // the evidence sample interval. This remains observation-only.
            if (Input.GetKeyDown(KeyCode.F5) || Input.GetKeyDown(KeyCode.C))
                AddEvent("save_input_observed", "F5/C save binding edge observed in Player");
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.B) ||
                Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.V))
                AddEvent("interaction_input_observed", "E/B/T/V interaction binding edge observed in Player");
        }

        private void ResolveRuntimeObjects()
        {
            if (_player == null)
            {
                var playerObject = GameObject.Find("D2_PlayerAvatar");
                if (playerObject == null) playerObject = GameObject.Find("Player");
                if (playerObject != null) _player = playerObject.transform;
            }
            if (_controller == null && _player != null)
                _controller = _player.GetComponent<D2CharacterController>();
            if (_ship == null)
            {
                var shipObject = GameObject.Find("ph_ship_fishing");
                if (shipObject != null)
                {
                    _ship = shipObject.transform;
                    var colliders = _ship.GetComponentsInChildren<Collider>(true);
                    for (var i = 0; i < colliders.Length; i++)
                    {
                        if (colliders[i] != null && colliders[i].name == "D6_ShipHullCollision")
                        {
                            _shipHull = colliders[i];
                            break;
                        }
                    }
                }
            }
            if (_router == null) _router = FindObjectOfType<D2ControlRouter>();
        }

        private void Capture(float elapsed)
        {
            if (_player == null) return;
            RefreshLandmarkObjectResolution();
            if (_observations.Count == 0) _firstPosition = _player.position;
            var feedback = _router == null ? null : _router.ReadLastInteractionFeedback();
            var observation = new Observation
            {
                elapsed_seconds = elapsed,
                realtime_seconds = Time.realtimeSinceStartup,
                player_x = _player.position.x,
                player_y = _player.position.y,
                player_z = _player.position.z,
                ship_x = _ship == null ? 0f : _ship.position.x,
                ship_y = _ship == null ? 0f : _ship.position.y,
                ship_z = _ship == null ? 0f : _ship.position.z,
                ship_found = _ship != null,
                player_overlaps_ship_hull = _shipHull != null && _controller != null &&
                    Physics.ComputePenetration(
                        _controller.GetComponent<Collider>(), _player.position, _player.rotation,
                        _shipHull, _shipHull.transform.position, _shipHull.transform.rotation,
                        out _, out _),
                grounded = _controller != null && _controller.IsGrounded,
                moving = _controller != null && _controller.IsMoving,
                running = _controller != null && _controller.IsRunning,
                speed = _controller == null ? 0f : _controller.CurrentSpeed,
                vertical_velocity = _controller == null ? 0f : _controller.VerticalVelocity,
                control_mode = _router == null ? string.Empty : _router.Mode.ToString(),
                feedback_code = feedback == null ? string.Empty : feedback.Code,
                feedback_target = feedback == null ? string.Empty : feedback.TargetStableId,
                key_w = Input.GetKey(KeyCode.W),
                key_a = Input.GetKey(KeyCode.A),
                key_s = Input.GetKey(KeyCode.S),
                key_d = Input.GetKey(KeyCode.D),
                key_e = Input.GetKey(KeyCode.E),
                key_b = Input.GetKey(KeyCode.B),
                key_t = Input.GetKey(KeyCode.T),
                key_v = Input.GetKey(KeyCode.V),
                key_f5 = Input.GetKey(KeyCode.F5),
                key_c = Input.GetKey(KeyCode.C),
                key_escape = Input.GetKey(KeyCode.Escape)
            };
            _observations.Add(observation);

            if (observation.moving) AddEvent("movement_observed", "D2CharacterController reported movement");
            if (observation.key_e || observation.key_b || observation.key_t || observation.key_v)
                AddEvent("interaction_input_observed", "E/B/T/V held or pressed in Player");
            if (observation.key_f5 || observation.key_c) AddEvent("save_input_observed", "F5/C held or pressed in Player");
            if (Vector3.Distance(_firstPosition, _player.position) >= 1.2f)
                AddEvent("displacement_observed", "player displacement reached 1.2m");

            for (int i = 0; i < _landmarks.Length; i++)
            {
                var landmark = _landmarks[i];
                if (landmark.first_reached_seconds >= 0f) continue;
                if (Vector3.Distance(_player.position, new Vector3(landmark.x, landmark.y, landmark.z)) <= landmark.reach_radius)
                {
                    landmark.first_reached_seconds = elapsed;
                    AddEvent("landmark_reached", landmark.stable_id);
                }
            }

            CapturePlayerFrame(elapsed);
        }

        private void CapturePlayerFrame(float elapsed)
        {
            if (!HasFlag("-d19-r03-route-rerun5") && !HasFlag("-d19-r03-route-rerun6") &&
                !HasFlag("-d19-r03-route-rerun7") && !HasFlag("-d19-r03-route-rerun8") &&
                !HasFlag("-d19-r03-route-rerun9") && !HasFlag("-d19-r03-route-rerun10") &&
                !HasFlag("-d19-r03-route-rerun29") &&
                !HasFlag("-d19-r03-route-rerun41") &&
                !HasFlag("-d19-r03-route-rerun42") &&
                !HasFlag("-d19-r03-route-rerun43") &&
                !HasFlag("-d19-r03-route-rerun46") &&
                !HasFlag("-d19-r03-route-rerun45") &&
                !HasFlag("-d19-r03-route-rerun44")) return;
            if (_screenshotIndex >= 16 || elapsed + 0.001f < _nextScreenshotAt) return;

            var prefix = HasFlag("-d19-r03-route-rerun41") ? "route_rerun41_" :
                HasFlag("-d19-r03-route-rerun42") ? "route_rerun42_" :
                HasFlag("-d19-r03-route-rerun43") ? "route_rerun43_" :
                HasFlag("-d19-r03-route-rerun46") ? "route_rerun46_" :
                HasFlag("-d19-r03-route-rerun45") ? "route_rerun45_" :
                HasFlag("-d19-r03-route-rerun44") ? "route_rerun44_" :
                         HasFlag("-d19-r03-route-rerun29") ? "route_rerun29_" :
                         HasFlag("-d19-r03-route-rerun10") ? "route_rerun10_" :
                         HasFlag("-d19-r03-route-rerun9") ? "route_rerun9_" :
                         HasFlag("-d19-r03-route-rerun8") ? "route_rerun8_" :
                         HasFlag("-d19-r03-route-rerun7") ? "route_rerun7_" :
                         HasFlag("-d19-r03-route-rerun6") ? "route_rerun6_" : "route_rerun5_";
            var path = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/screenshots/" + prefix +
                       _screenshotIndex.ToString("00") + ".png";
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                ScreenCapture.CaptureScreenshot(path);
                AddEvent("player_screenshot_requested", path);
                _screenshotIndex++;
                _nextScreenshotAt = elapsed + 30f;
            }
            catch (Exception ex)
            {
                AddEvent("player_screenshot_failed", ex.GetType().Name);
            }
        }

        private void RefreshLandmarkObjectResolution()
        {
            if (_landmarks == null) return;
            for (var i = 0; i < _landmarks.Length; i++)
            {
                if (_landmarks[i].found) continue;
                _landmarks[i].found = FindSceneObject(_landmarks[i].object_name) != null;
            }
        }

        private static GameObject FindSceneObject(string objectName)
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (var j = 0; j < transforms.Length; j++)
                    if (transforms[j].name == objectName) return transforms[j].gameObject;
            }
            return null;
        }

        private void AddEvent(string type, string detail)
        {
            if (_events.Count > 0)
            {
                var last = _events[_events.Count - 1];
                if (last.type == type && last.detail == detail &&
                    Time.realtimeSinceStartup - _startedAt - last.elapsed_seconds < 1.0f) return;
            }
            _events.Add(new EventRecord
            {
                elapsed_seconds = Time.realtimeSinceStartup - _startedAt,
                type = type,
                detail = detail
            });
        }

        private LandmarkRecord[] CreateLandmarks()
        {
            return new[]
            {
                Landmark("anchor.player.spawn", "D2_PlayerAvatar", Vector3.zero, 4f),
                Landmark("building:market_santa_margarita", "MarketVisual", new Vector3(5.0f, 0f, 0.35f), LandmarkRadius),
                Landmark("building:warehouse_santa_margarita", "WarehouseVisual", new Vector3(-5.0f, 0f, 0.35f), LandmarkRadius),
                Landmark("building:tavern_santa_margarita", "TavernVisual", new Vector3(0f, 0f, 4.15f), LandmarkRadius),
                Landmark("building:shipyard_santa_margarita", "ShipyardVisual", new Vector3(-7f, 0f, -3.6f), LandmarkRadius),
                Landmark("anchor.dock.primary", "DockDeck", new Vector3(-1f, 0f, -8.8f), 5f),
                Landmark("anchor.berth.primary", "BerthStoneEdge", new Vector3(-1f, 0f, -11.1f), 5f),
                Landmark("anchor.return.harbor", "D19_R03_ShorelineAndWater", new Vector3(0f, 0f, 13f), 7f)
            };
        }

        private static LandmarkRecord Landmark(string stableId, string objectName, Vector3 position, float radius)
        {
            return new LandmarkRecord
            {
                stable_id = stableId,
                object_name = objectName,
                found = FindSceneObject(objectName) != null,
                x = position.x,
                y = position.y,
                z = position.z,
                reach_radius = radius
            };
        }

        private void OnApplicationQuit()
        {
            WriteResult();
        }

        private void OnDestroy()
        {
            if (Application.isPlaying) WriteResult();
        }

        private void WriteResult()
        {
            if (_wrote) return;
            _wrote = true;
            ResolveRuntimeObjects();
            var result = new ProbeResult
            {
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                player_object = _player == null ? string.Empty : _player.name,
                router_object = _router == null ? string.Empty : _router.name,
                duration_seconds = Time.realtimeSinceStartup - _startedAt,
                player_found = _player != null,
                controller_found = _controller != null,
                router_found = _router != null,
                moved_observed = _events.Exists(e => e.type == "movement_observed" || e.type == "displacement_observed"),
                interaction_input_observed = _events.Exists(e => e.type == "interaction_input_observed"),
                save_input_observed = _events.Exists(e => e.type == "save_input_observed"),
                landmarks = _landmarks ?? Array.Empty<LandmarkRecord>(),
                observations = _observations.ToArray(),
                events = _events.ToArray()
            };
            var fileName = HasFlag("-d19-r03-route-rerun41")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN41.json"
                : HasFlag("-d19-r03-route-rerun42")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN42.json"
                : HasFlag("-d19-r03-route-rerun43")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN43.json"
                : HasFlag("-d19-r03-route-rerun46")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN46.json"
                : HasFlag("-d19-r03-route-rerun45")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN45.json"
                : HasFlag("-d19-r03-route-rerun44")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN44.json"
                : HasFlag("-d19-r03-route-rerun29")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN29.json"
                : HasFlag("-d19-r03-route-rerun10")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN10.json"
                : HasFlag("-d19-r03-route-rerun9")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN9.json"
                : HasFlag("-d19-r03-route-rerun8")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN8.json"
                : HasFlag("-d19-r03-route-rerun7")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN7.json"
                : HasFlag("-d19-r03-route-rerun6")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN6.json"
                : HasFlag("-d19-r03-route-rerun5")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN5.json"
                : HasFlag("-d19-r03-route-rerun4")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN4.json"
                : HasFlag("-d19-r03-route-rerun3")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN3.json"
                : HasFlag("-d19-r03-route-control-test")
                    ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_CONTROL_TEST.json"
                    : HasFlag("-d19-r03-route-rerun2")
                        ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN2.json"
                        : "D19_R03_A01_RUNTIME_ROUTE_PROBE.json";
            var path = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/" + fileName;
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(path, JsonUtility.ToJson(result, true));
                Debug.Log("[D19-R03-A01] Runtime route probe written: " + path);
            }
            catch (Exception ex)
            {
                Debug.LogError("[D19-R03-A01] Runtime route probe write failed: " + ex);
            }
        }
    }
}
