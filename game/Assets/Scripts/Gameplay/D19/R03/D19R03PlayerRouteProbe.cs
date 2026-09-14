using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.D10;
using Whalebone.Gameplay.D7;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.UI.D16;
using Whalebone.Systems.D2;

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
            public float player_yaw;
            public float yaw_delta_from_start;
            public float ship_x;
            public float ship_y;
            public float ship_z;
            public float ship_yaw;
            public bool ship_found;
            public bool player_overlaps_ship_hull;
            public bool grounded;
            public bool swimming;
            public float water_surface_y;
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
            public bool key_i;
            public bool key_m;
            public bool key_f5;
            public bool key_c;
            public bool key_escape;
            public float d16_move_x;
            public float d16_move_y;
            public bool input_system_keyboard_present;
            public bool input_system_w_pressed;
            public bool input_system_a_pressed;
            public bool input_system_s_pressed;
            public bool input_system_d_pressed;
            public bool application_focused;
            public bool gameplay_input_allowed;
            public bool primary_input_allowed;
            public bool event_system_found;
            public bool ui_navigation_events_enabled;
            public bool ui_move_action_bound;
            public string selected_ui_object;
            public bool gameplay_ui_navigation_suppressed;
            public string movement_input_source;
            public bool legacy_movement_fallback_active;
            public float time_scale;
            public bool d3_owner_ready;
            public bool d10_owner_ready;
            public bool d29_warehouse_panel_found;
            public bool warehouse_visible;
            public bool market_visible;
            public bool tavern_visible;
        }

        [Serializable]
        private sealed class EventRecord
        {
            public float elapsed_seconds;
            public string type;
            public string detail;
            // Input-edge events can carry the same-frame owner/page snapshot.
            // Generic de-duplicated events leave these fields at their default
            // values; runtime bootstrap evidence filters on this capture flag.
            public bool runtime_state_captured;
            public bool d3_owner_ready;
            public bool d10_owner_ready;
            public bool d29_warehouse_panel_found;
            public bool warehouse_visible;
            public bool market_visible;
            public bool tavern_visible;
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
            public bool turning_observed;
            public bool interaction_input_observed;
            public bool save_input_observed;
            public bool swimming_observed;
            public bool ship_control_observed;
            public bool ship_moved_observed;
            public bool ship_turning_observed;
            public float ship_max_displacement;
            public float ship_max_heading_delta;
            public LandmarkRecord[] landmarks;
            public Observation[] observations;
            public EventRecord[] events;
        }

        private const float SampleInterval = 0.25f;
        private const float LandmarkRadius = 3.5f;
        private const string R03PresentationRootName = "D19_R03_HarborPresentation";
        private readonly List<Observation> _observations = new List<Observation>();
        private readonly List<EventRecord> _events = new List<EventRecord>();
        private LandmarkRecord[] _landmarks;
        private D2CharacterController _controller;
        private D2ControlRouter _router;
        private D3RuntimeBootstrap _d3;
        private D10TavernSocialBootstrap _d10;
        private Transform _player;
        private Transform _ship;
        private Collider _shipHull;
        private GameObject _warehousePanel;
        private float _startedAt;
        private float _nextSampleAt;
        private float _nextScreenshotAt = 5f;
        private int _screenshotIndex;
        private Vector3 _firstPosition;
        private Quaternion _firstRotation;
        private bool _pageStateInitialized;
        private bool _lastWarehouseVisible;
        private bool _lastMarketVisible;
        private bool _lastTavernVisible;
        private bool _wrote;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!HasAnyRouteFlag()) return;
            if (FindFirstObjectByType<D19R03PlayerRouteProbe>() != null) return;
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

        private static string BootstrapRejectFileSuffix()
        {
            if (HasFlag("-d19-r03-bootstrap-key-i")) return "_I_20260906";
            if (HasFlag("-d19-r03-bootstrap-key-m")) return "_M_20260906";
            if (HasFlag("-d19-r03-bootstrap-key-t")) return "_T_20260906";
            return "_20260906";
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
            if (HasFlag("-d19-r03-bootstrap-reject-rerun1"))
                Debug.Log("[D19-R03-A01] Bootstrap owner-not-ready rejection window active");
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
                   HasFlag("-d19-r03-bootstrap-reject-rerun1") ||
                   HasFlag("-d19-r03-route-rerun47") ||
                   HasFlag("-d19-r03-route-rerun49") ||
                   HasFlag("-d19-r03-route-rerun52") ||
                   HasFlag("-d19-r03-route-rerun53") ||
                   HasFlag("-d19-r03-route-rerun54") ||
                   HasFlag("-d19-r03-route-rerun55") ||
                   HasFlag("-d19-r03-route-rerun56") ||
                   HasFlag("-d19-r03-route-rerun58") ||
                   HasFlag("-d19-r03-route-rerun57") ||
                   HasFlag("-d19-r03-route-rerun59") ||
                   HasFlag("-d19-r03-route-rerun60") ||
                   HasFlag("-d19-r03-route-rerun61") ||
                   HasFlag("-d19-r03-route-rerun62") ||
                   HasFlag("-d19-r03-route-rerun63") ||
                   HasFlag("-d19-r03-route-rerun64") ||
                   HasFlag("-d19-r03-route-rerun65") ||
                   HasFlag("-d19-r03-route-rerun66") ||
                   HasFlag("-d19-r03-route-rerun67") ||
                   HasFlag("-d19-r03-route-rerun73") ||
                   HasFlag("-d19-r03-route-rerun72") ||
                   HasFlag("-d19-r03-route-rerun74") ||
                   HasFlag("-d19-r03-route-rerun75") ||
                    HasFlag("-d19-r03-route-rerun76") ||
                    HasFlag("-d19-r03-route-rerun77") ||
                    HasFlag("-d19-r03-route-rerun78") ||
                   HasFlag("-d19-r03-route-rerun82") ||
                   HasFlag("-d19-r03-route-rerun83") ||
                   HasFlag("-d19-r03-route-rerun84") ||
                   HasFlag("-d19-r03-route-rerun85") ||
                   HasFlag("-d19-r03-route-rerun86") ||
                     HasFlag("-d19-r03-route-rerun102") ||
                     HasFlag("-d19-r03-route-rerun101") ||
                     HasFlag("-d19-r03-route-rerun100") ||
                     HasFlag("-d19-r03-route-rerun99") ||
                     HasFlag("-d19-r03-route-rerun98") ||
                     HasFlag("-d19-r03-route-rerun97") ||
                     HasFlag("-d19-r03-route-rerun96") ||
                     HasFlag("-d19-r03-route-rerun95") ||
                     HasFlag("-d19-r03-route-rerun94") ||
                     HasFlag("-d19-r03-route-rerun93") ||
                     HasFlag("-d19-r03-route-rerun92") ||
                     HasFlag("-d19-r03-route-rerun91") ||
                    HasFlag("-d19-r03-route-rerun90") ||
                   HasFlag("-d19-r03-route-rerun89") ||
                   HasFlag("-d19-r03-route-rerun88") ||
                   HasFlag("-d19-r03-route-rerun87") ||
                   HasFlag("-d19-r03-route-rerun81") ||
                    HasFlag("-d19-r03-route-rerun80") ||
                    HasFlag("-d19-r03-route-rerun79") ||
                    HasFlag("-d19-r03-route-rerun73") ||
                   HasFlag("-d19-r03-route-rerun71") ||
                   HasFlag("-d19-r03-route-rerun70") ||
                   HasFlag("-d19-r03-route-rerun69") ||
                   HasFlag("-d19-r03-route-rerun68") ||
                   HasFlag("-d19-r03-route-rerun51") ||
                   HasFlag("-d19-r03-route-rerun50") ||
                   HasFlag("-d19-r03-route-rerun48") ||
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
                Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.V) ||
                Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.M))
                AddInputEdgeEvent("interaction_input_edge_observed", "E/B/T/V/I/M interaction binding edge observed in Player");
            if (Input.GetKeyDown(KeyCode.I))
                AddInputEdgeEvent("warehouse_input_edge_observed", "I warehouse binding edge observed in Player");
            if (Input.GetKeyDown(KeyCode.M))
                AddInputEdgeEvent("market_input_edge_observed", "M market binding edge observed in Player");
            if (Input.GetKeyDown(KeyCode.T))
                AddInputEdgeEvent("tavern_input_edge_observed", "T tavern binding edge observed in Player");
            if (Input.GetKeyDown(KeyCode.Escape))
                AddInputEdgeEvent("close_input_edge_observed", "Escape close binding edge observed in Player");
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
            if (_router == null) _router = FindFirstObjectByType<D2ControlRouter>();
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_d10 == null) _d10 = FindFirstObjectByType<D10TavernSocialBootstrap>(FindObjectsInactive.Include);
            if (_warehousePanel == null)
            {
                var hudCanvas = GameObject.Find("D29InteractionHudCanvas");
                if (hudCanvas != null)
                {
                    var transforms = hudCanvas.GetComponentsInChildren<Transform>(true);
                    for (var i = 0; i < transforms.Length; i++)
                    {
                        if (transforms[i] != null && transforms[i].name == "WarehousePanel")
                        {
                            _warehousePanel = transforms[i].gameObject;
                            break;
                        }
                    }
                }
            }
        }

        private void Capture(float elapsed)
        {
            if (_player == null) return;
            RefreshLandmarkObjectResolution();
            if (_observations.Count == 0)
            {
                _firstPosition = _player.position;
                _firstRotation = _player.rotation;
            }
            var feedback = _router == null ? null : _router.ReadLastInteractionFeedback();
            var playerCollider = _controller == null ? null : _controller.GetComponent<Collider>();
            var d16MoveAxis = Vector2.zero;
            var keyboard = Keyboard.current;
            var gameplayInputAllowed = false;
            var primaryInputAllowed = false;
            try
            {
                d16MoveAxis = D16InputRouter.MoveAxis();
                gameplayInputAllowed = D7ControlCoordinator.GameplayInputAllowed;
                primaryInputAllowed = RuntimeHudCoordinator.PrimaryInputAllowed;
            }
            catch
            {
                // The probe must remain observational if a transient runtime
                // owner is not ready during scene bootstrap.
            }
            var eventSystem = EventSystem.current;
            var uiInputModule = eventSystem == null
                ? null
                : eventSystem.GetComponent<InputSystemUIInputModule>();
            var selectedUiObject = eventSystem == null ? null : eventSystem.currentSelectedGameObject;
            var uiNavigationEventsEnabled = eventSystem != null && eventSystem.sendNavigationEvents;
            var uiMoveActionBound = uiInputModule != null && uiInputModule.move != null;
            var gameplayUiNavigationSuppressed = primaryInputAllowed && eventSystem != null &&
                !uiNavigationEventsEnabled && !uiMoveActionBound && selectedUiObject == null;
            var warehouseVisible = _warehousePanel != null && _warehousePanel.activeInHierarchy;
            var marketVisible = _d3 != null && _d3.IsTradeOpen;
            var tavernVisible = _d10 != null && _d10.HasInputFocus;
            var observation = new Observation
            {
                elapsed_seconds = elapsed,
                realtime_seconds = Time.realtimeSinceStartup,
                player_x = _player.position.x,
                player_y = _player.position.y,
                player_z = _player.position.z,
                player_yaw = _player.eulerAngles.y,
                yaw_delta_from_start = Quaternion.Angle(_firstRotation, _player.rotation),
                ship_x = _ship == null ? 0f : _ship.position.x,
                ship_y = _ship == null ? 0f : _ship.position.y,
                ship_z = _ship == null ? 0f : _ship.position.z,
                ship_yaw = _ship == null ? 0f : _ship.eulerAngles.y,
                ship_found = _ship != null,
                player_overlaps_ship_hull = _shipHull != null && playerCollider != null &&
                    Physics.ComputePenetration(
                        playerCollider, _player.position, _player.rotation,
                        _shipHull, _shipHull.transform.position, _shipHull.transform.rotation,
                        out _, out _),
                grounded = _controller != null && _controller.IsGrounded,
                swimming = _controller != null && _controller.IsSwimming,
                water_surface_y = _controller == null ? 0f : _controller.WaterSurfaceY,
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
                 key_i = Input.GetKey(KeyCode.I),
                 key_m = Input.GetKey(KeyCode.M),
                 key_f5 = Input.GetKey(KeyCode.F5),
                 key_c = Input.GetKey(KeyCode.C),
                  key_escape = Input.GetKey(KeyCode.Escape),
                  d16_move_x = d16MoveAxis.x,
                  d16_move_y = d16MoveAxis.y,
                  input_system_keyboard_present = keyboard != null,
                  input_system_w_pressed = keyboard != null && keyboard.wKey.isPressed,
                  input_system_a_pressed = keyboard != null && keyboard.aKey.isPressed,
                  input_system_s_pressed = keyboard != null && keyboard.sKey.isPressed,
                  input_system_d_pressed = keyboard != null && keyboard.dKey.isPressed,
                  application_focused = Application.isFocused,
                  gameplay_input_allowed = gameplayInputAllowed,
                  primary_input_allowed = primaryInputAllowed,
                  event_system_found = eventSystem != null,
                  ui_navigation_events_enabled = uiNavigationEventsEnabled,
                  ui_move_action_bound = uiMoveActionBound,
                  selected_ui_object = selectedUiObject == null ? string.Empty : selectedUiObject.name,
                  gameplay_ui_navigation_suppressed = gameplayUiNavigationSuppressed,
                  movement_input_source = _controller == null
                     ? string.Empty : _controller.MovementInputSource.ToString(),
                  legacy_movement_fallback_active = _controller != null &&
                     _controller.LegacyMovementFallbackActive,
                  time_scale = Time.timeScale,
                  d3_owner_ready = _d3 != null && _d3.IsReady,
                  d10_owner_ready = _d10 != null && _d10.IsReady,
                  d29_warehouse_panel_found = _warehousePanel != null,
                  warehouse_visible = warehouseVisible,
                  market_visible = marketVisible,
                  tavern_visible = tavernVisible
            };
            _observations.Add(observation);

            if (!_pageStateInitialized)
            {
                _pageStateInitialized = true;
                _lastWarehouseVisible = warehouseVisible;
                _lastMarketVisible = marketVisible;
                _lastTavernVisible = tavernVisible;
            }
            else
            {
                if (warehouseVisible != _lastWarehouseVisible)
                    AddEvent(warehouseVisible ? "warehouse_page_open_observed" : "warehouse_page_close_observed",
                        warehouseVisible ? "D29 WarehousePanel became visible" : "D29 WarehousePanel became hidden");
                if (marketVisible != _lastMarketVisible)
                    AddEvent(marketVisible ? "market_page_open_observed" : "market_page_close_observed",
                        marketVisible ? "D3 trade page became visible" : "D3 trade page became hidden");
                if (tavernVisible != _lastTavernVisible)
                    AddEvent(tavernVisible ? "tavern_page_open_observed" : "tavern_page_close_observed",
                        tavernVisible ? "D10 tavern page received input focus" : "D10 tavern page lost input focus");
                _lastWarehouseVisible = warehouseVisible;
                _lastMarketVisible = marketVisible;
                _lastTavernVisible = tavernVisible;
            }

            if (observation.moving) AddEvent("movement_observed", "D2CharacterController reported movement");
            if (observation.swimming) AddEvent("swimming_observed", "D2CharacterController reported swimming");
            if (observation.control_mode == D2ControlMode.Ship.ToString() && observation.ship_found)
                AddEvent("ship_control_observed", "D2ShipController owned the Player control mode");
            if ((observation.key_a || observation.key_d) && observation.yaw_delta_from_start >= 5f)
                AddEvent("turning_observed", "D2CharacterController rotated while A/D was held");
            if (observation.key_w || observation.key_s || observation.key_a || observation.key_d)
            {
                if (observation.gameplay_ui_navigation_suppressed)
                    AddEvent("gameplay_ui_navigation_suppressed_observed",
                        "Gameplay movement keys had no active UGUI navigation action or selected object");
                else if (observation.primary_input_allowed &&
                         (observation.ui_navigation_events_enabled || observation.ui_move_action_bound ||
                          !string.IsNullOrEmpty(observation.selected_ui_object)))
                    AddEvent("gameplay_ui_navigation_conflict_observed",
                        "Gameplay movement key overlapped active UGUI navigation state; selected=" +
                        observation.selected_ui_object);
            }
            if (observation.key_e || observation.key_b || observation.key_t || observation.key_v ||
                observation.key_i || observation.key_m)
                AddEvent("interaction_input_observed", "E/B/T/V/I/M held or pressed in Player");
            if (observation.key_i)
                AddEvent("warehouse_input_observed", "I warehouse binding held or pressed in Player");
            if (observation.key_m)
                AddEvent("market_input_observed", "M market binding held or pressed in Player");
            if (observation.key_t)
                AddEvent("tavern_input_observed", "T tavern binding held or pressed in Player");
            if (observation.key_f5 || observation.key_c) AddEvent("save_input_observed", "F5/C held or pressed in Player");
            if (observation.key_escape) AddEvent("close_input_observed", "Escape close binding held or pressed in Player");
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
            if (!HasFlag("-d19-r03-route-rerun102") &&
                !HasFlag("-d19-r03-route-rerun101") &&
                !HasFlag("-d19-r03-route-rerun100") &&
                !HasFlag("-d19-r03-route-rerun99") &&
                !HasFlag("-d19-r03-route-rerun98") &&
                !HasFlag("-d19-r03-route-rerun97") &&
                !HasFlag("-d19-r03-route-rerun96") &&
                !HasFlag("-d19-r03-route-rerun95") &&
                !HasFlag("-d19-r03-route-rerun94") &&
                !HasFlag("-d19-r03-route-rerun93") &&
                !HasFlag("-d19-r03-route-rerun92") &&
                !HasFlag("-d19-r03-route-rerun91") &&
                !HasFlag("-d19-r03-route-rerun90") &&
                !HasFlag("-d19-r03-route-rerun89") &&
                !HasFlag("-d19-r03-route-rerun88") &&
                !HasFlag("-d19-r03-route-rerun87") &&
                !HasFlag("-d19-r03-route-rerun5") && !HasFlag("-d19-r03-route-rerun6") &&
                !HasFlag("-d19-r03-route-rerun7") && !HasFlag("-d19-r03-route-rerun8") &&
                !HasFlag("-d19-r03-route-rerun9") && !HasFlag("-d19-r03-route-rerun10") &&
                !HasFlag("-d19-r03-route-rerun29") &&
                !HasFlag("-d19-r03-route-rerun41") &&
                !HasFlag("-d19-r03-route-rerun42") &&
                !HasFlag("-d19-r03-route-rerun43") &&
                !HasFlag("-d19-r03-route-rerun47") &&
                !HasFlag("-d19-r03-route-rerun49") &&
                !HasFlag("-d19-r03-route-rerun52") &&
                !HasFlag("-d19-r03-route-rerun53") &&
                !HasFlag("-d19-r03-route-rerun54") &&
                !HasFlag("-d19-r03-route-rerun55") &&
                !HasFlag("-d19-r03-route-rerun56") &&
                !HasFlag("-d19-r03-route-rerun58") &&
                !HasFlag("-d19-r03-route-rerun57") &&
                !HasFlag("-d19-r03-route-rerun60") &&
                !HasFlag("-d19-r03-route-rerun59") &&
                !HasFlag("-d19-r03-route-rerun61") &&
                !HasFlag("-d19-r03-route-rerun62") &&
                !HasFlag("-d19-r03-route-rerun63") &&
                !HasFlag("-d19-r03-route-rerun64") &&
                !HasFlag("-d19-r03-route-rerun65") &&
                !HasFlag("-d19-r03-route-rerun66") &&
                !HasFlag("-d19-r03-route-rerun67") &&
                !HasFlag("-d19-r03-route-rerun73") &&
                !HasFlag("-d19-r03-route-rerun72") &&
                !HasFlag("-d19-r03-route-rerun74") &&
                !HasFlag("-d19-r03-route-rerun75") &&
                 !HasFlag("-d19-r03-route-rerun76") &&
                 !HasFlag("-d19-r03-route-rerun77") &&
                 !HasFlag("-d19-r03-route-rerun78") &&
                 !HasFlag("-d19-r03-route-rerun82") &&
                 !HasFlag("-d19-r03-route-rerun83") &&
                !HasFlag("-d19-r03-route-rerun84") &&
                !HasFlag("-d19-r03-route-rerun81") &&
                 !HasFlag("-d19-r03-route-rerun80") &&
                 !HasFlag("-d19-r03-route-rerun79") &&
                 !HasFlag("-d19-r03-route-rerun73") &&
                !HasFlag("-d19-r03-route-rerun71") &&
                !HasFlag("-d19-r03-route-rerun70") &&
                !HasFlag("-d19-r03-route-rerun69") &&
                !HasFlag("-d19-r03-route-rerun68") &&
                !HasFlag("-d19-r03-route-rerun51") &&
                !HasFlag("-d19-r03-route-rerun50") &&
                !HasFlag("-d19-r03-route-rerun48") &&
                !HasFlag("-d19-r03-route-rerun46") &&
                !HasFlag("-d19-r03-route-rerun45") &&
                !HasFlag("-d19-r03-route-rerun44")) return;
            if (_screenshotIndex >= 16 || elapsed + 0.001f < _nextScreenshotAt) return;

            var prefix = HasFlag("-d19-r03-route-rerun102") ? "route_rerun102_" :
                HasFlag("-d19-r03-route-rerun101") ? "route_rerun101_" :
                HasFlag("-d19-r03-route-rerun100") ? "route_rerun100_" :
                HasFlag("-d19-r03-route-rerun99") ? "route_rerun99_" :
                HasFlag("-d19-r03-route-rerun98") ? "route_rerun98_" :
                HasFlag("-d19-r03-route-rerun97") ? "route_rerun97_" :
                HasFlag("-d19-r03-route-rerun96") ? "route_rerun96_" :
                HasFlag("-d19-r03-route-rerun95") ? "route_rerun95_" :
                HasFlag("-d19-r03-route-rerun94") ? "route_rerun94_" :
                HasFlag("-d19-r03-route-rerun93") ? "route_rerun93_" :
                HasFlag("-d19-r03-route-rerun92") ? "route_rerun92_" :
                HasFlag("-d19-r03-route-rerun91") ? "route_rerun91_" :
                HasFlag("-d19-r03-route-rerun90") ? "route_rerun90_" :
                HasFlag("-d19-r03-route-rerun89") ? "route_rerun89_" :
                HasFlag("-d19-r03-route-rerun88") ? "route_rerun88_" :
                HasFlag("-d19-r03-route-rerun87") ? "route_rerun87_" :
                HasFlag("-d19-r03-route-rerun86") ? "route_rerun86_" :
                HasFlag("-d19-r03-route-rerun85") ? "route_rerun85_" :
                HasFlag("-d19-r03-route-rerun84") ? "route_rerun84_" :
                HasFlag("-d19-r03-route-rerun83") ? "route_rerun83_" :
                HasFlag("-d19-r03-route-rerun82") ? "route_rerun82_" :
                HasFlag("-d19-r03-route-rerun81") ? "route_rerun81_" :
                HasFlag("-d19-r03-route-rerun80") ? "route_rerun80_" :
                HasFlag("-d19-r03-route-rerun79") ? "route_rerun79_" :
                HasFlag("-d19-r03-route-rerun78") ? "route_rerun78_" :
                HasFlag("-d19-r03-route-rerun77") ? "route_rerun77_" :
                HasFlag("-d19-r03-route-rerun76") ? "route_rerun76_" :
                HasFlag("-d19-r03-route-rerun75") ? "route_rerun75_" :
                HasFlag("-d19-r03-route-rerun74") ? "route_rerun74_" :
                HasFlag("-d19-r03-route-rerun73") ? "route_rerun73_" :
                HasFlag("-d19-r03-route-rerun72") ? "route_rerun72_" :
                HasFlag("-d19-r03-route-rerun71") ? "route_rerun71_" :
                HasFlag("-d19-r03-route-rerun70") ? "route_rerun70_" :
                HasFlag("-d19-r03-route-rerun69") ? "route_rerun69_" :
                HasFlag("-d19-r03-route-rerun68") ? "route_rerun68_" :
                HasFlag("-d19-r03-route-rerun67") ? "route_rerun67_" :
                HasFlag("-d19-r03-route-rerun66") ? "route_rerun66_" :
                HasFlag("-d19-r03-route-rerun65") ? "route_rerun65_" :
                HasFlag("-d19-r03-route-rerun64") ? "route_rerun64_" :
                HasFlag("-d19-r03-route-rerun63") ? "route_rerun63_" :
                HasFlag("-d19-r03-route-rerun62") ? "route_rerun62_" :
                HasFlag("-d19-r03-route-rerun61") ? "route_rerun61_" :
                HasFlag("-d19-r03-route-rerun60") ? "route_rerun60_" :
                HasFlag("-d19-r03-route-rerun59") ? "route_rerun59_" :
                HasFlag("-d19-r03-route-rerun58") ? "route_rerun58_" :
                HasFlag("-d19-r03-route-rerun57") ? "route_rerun57_" :
                HasFlag("-d19-r03-route-rerun56") ? "route_rerun56_" :
                HasFlag("-d19-r03-route-rerun55") ? "route_rerun55_" :
                HasFlag("-d19-r03-route-rerun54") ? "route_rerun54_" :
                HasFlag("-d19-r03-route-rerun53") ? "route_rerun53_" :
                HasFlag("-d19-r03-route-rerun52") ? "route_rerun52_" :
                HasFlag("-d19-r03-route-rerun51") ? "route_rerun51_" :
                HasFlag("-d19-r03-route-rerun50") ? "route_rerun50_" :
                HasFlag("-d19-r03-route-rerun49") ? "route_rerun49_" :
                HasFlag("-d19-r03-route-rerun41") ? "route_rerun41_" :
                HasFlag("-d19-r03-route-rerun42") ? "route_rerun42_" :
                HasFlag("-d19-r03-route-rerun43") ? "route_rerun43_" :
                HasFlag("-d19-r03-route-rerun47") ? "route_rerun47_" :
                HasFlag("-d19-r03-route-rerun48") ? "route_rerun48_" :
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
            if (IsR03LandmarkName(objectName))
            {
                // Landmark evidence must resolve against the current R03
                // presentation. A global name search can otherwise report a
                // hidden legacy DockDeck or building as present even when the
                // visible R03 object is missing.
                var presentation = GameObject.Find(R03PresentationRootName);
                if (presentation == null || !presentation.activeInHierarchy) return null;
                var presentationTransforms = presentation.GetComponentsInChildren<Transform>(false);
                for (var i = 0; i < presentationTransforms.Length; i++)
                {
                    var candidate = presentationTransforms[i];
                    if (candidate != null && candidate.name == objectName && candidate.gameObject.activeInHierarchy)
                        return candidate.gameObject;
                }
                return null;
            }

            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var transforms = roots[i].GetComponentsInChildren<Transform>(false);
                for (var j = 0; j < transforms.Length; j++)
                {
                    var candidate = transforms[j];
                    if (candidate != null && candidate.name == objectName && candidate.gameObject.activeInHierarchy)
                        return candidate.gameObject;
                }
            }
            return null;
        }

        private static bool IsR03LandmarkName(string objectName)
        {
            return objectName == "MarketVisual" ||
                   objectName == "WarehouseVisual" ||
                   objectName == "TavernVisual" ||
                   objectName == "ShipyardVisual" ||
                   objectName == "DockDeck" ||
                   objectName == "BerthStoneEdge" ||
                   objectName == "D19_R03_ShorelineAndWater";
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

        private void AddInputEdgeEvent(string type, string detail)
        {
            // GetKeyDown already supplies the edge boundary. Do not collapse two
            // real presses that happen inside the one-second observation de-dupe
            // window used for held-state events; the runtime matrix needs to
            // count the duplicate M/T/I input edges explicitly.
            var warehouseVisible = _warehousePanel != null && _warehousePanel.activeInHierarchy;
            var marketVisible = _d3 != null && _d3.IsTradeOpen;
            var tavernVisible = _d10 != null && _d10.HasInputFocus;
            _events.Add(new EventRecord
            {
                elapsed_seconds = Time.realtimeSinceStartup - _startedAt,
                type = type,
                detail = detail,
                runtime_state_captured = true,
                d3_owner_ready = _d3 != null && _d3.IsReady,
                d10_owner_ready = _d10 != null && _d10.IsReady,
                d29_warehouse_panel_found = _warehousePanel != null,
                warehouse_visible = warehouseVisible,
                market_visible = marketVisible,
                tavern_visible = tavernVisible
            });
        }

        private LandmarkRecord[] CreateLandmarks()
        {
            return new[]
            {
                Landmark("anchor.player.spawn", "D2_PlayerAvatar", Vector3.zero, 4f),
                // Keep evidence landmarks on the authored R03 presentation
                // coordinates.  The old values were inherited from the
                // retired grey-box layout; the 3.5m radius hid the drift in
                // a casual run but could report a landmark reached before
                // the player actually arrived at its facade.
                Landmark("building:market_santa_margarita", "MarketVisual", new Vector3(6.4f, 0f, 1.2f), LandmarkRadius),
                Landmark("building:warehouse_santa_margarita", "WarehouseVisual", new Vector3(-6.4f, 0f, 1.2f), LandmarkRadius),
                Landmark("building:tavern_santa_margarita", "TavernVisual", new Vector3(10.2f, 0f, 8.5f), LandmarkRadius),
                Landmark("building:shipyard_santa_margarita", "ShipyardVisual", new Vector3(-9.5f, 0f, -2.6f), LandmarkRadius),
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
                turning_observed = _observations.Exists(o =>
                    (o.key_a || o.key_d) && o.yaw_delta_from_start >= 5f),
                interaction_input_observed = _events.Exists(e => e.type == "interaction_input_observed"),
                save_input_observed = _events.Exists(e => e.type == "save_input_observed"),
                swimming_observed = _events.Exists(e => e.type == "swimming_observed"),
                ship_control_observed = _events.Exists(e => e.type == "ship_control_observed"),
                ship_max_displacement = MaxShipDisplacement(),
                ship_max_heading_delta = MaxShipHeadingDelta(),
                landmarks = _landmarks ?? Array.Empty<LandmarkRecord>(),
                observations = _observations.ToArray(),
                events = _events.ToArray()
            };
            result.ship_moved_observed = result.ship_control_observed && result.ship_max_displacement >= 0.05f;
            result.ship_turning_observed = result.ship_control_observed && result.ship_max_heading_delta >= 1f;
            var fileName = HasFlag("-d19-r03-route-rerun102")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN102.json"
                : HasFlag("-d19-r03-route-rerun101")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN101.json"
                : HasFlag("-d19-r03-route-rerun100")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN100.json"
                : HasFlag("-d19-r03-route-rerun99")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN99.json"
                : HasFlag("-d19-r03-route-rerun98")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN98.json"
                : HasFlag("-d19-r03-route-rerun97")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN97.json"
                : HasFlag("-d19-r03-route-rerun96")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN96.json"
                : HasFlag("-d19-r03-route-rerun95")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN95.json"
                : HasFlag("-d19-r03-route-rerun94")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN94.json"
                : HasFlag("-d19-r03-route-rerun93")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN93.json"
                : HasFlag("-d19-r03-route-rerun92")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN92.json"
                : HasFlag("-d19-r03-route-rerun91")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN91.json"
                : HasFlag("-d19-r03-route-rerun90")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN90.json"
                : HasFlag("-d19-r03-route-rerun89")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN89.json"
                : HasFlag("-d19-r03-route-rerun88")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN88.json"
                : HasFlag("-d19-r03-route-rerun87")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN87.json"
                : HasFlag("-d19-r03-route-rerun86")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN86.json"
                : HasFlag("-d19-r03-route-rerun85")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN85.json"
                : HasFlag("-d19-r03-route-rerun84")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN84.json"
                : HasFlag("-d19-r03-route-rerun83")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN83.json"
                : HasFlag("-d19-r03-route-rerun82")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN82.json"
                : HasFlag("-d19-r03-route-rerun81")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN81.json"
                : HasFlag("-d19-r03-route-rerun80")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN80.json"
                : HasFlag("-d19-r03-route-rerun79")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN79.json"
                : HasFlag("-d19-r03-route-rerun78")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN78.json"
                : HasFlag("-d19-r03-route-rerun77")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN77.json"
                : HasFlag("-d19-r03-route-rerun76")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN76.json"
                : HasFlag("-d19-r03-route-rerun75")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN75.json"
                : HasFlag("-d19-r03-route-rerun74")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN74.json"
                : HasFlag("-d19-r03-route-rerun73")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN73.json"
                : HasFlag("-d19-r03-route-rerun72")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN72.json"
                : HasFlag("-d19-r03-route-rerun71")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN71.json"
                : HasFlag("-d19-r03-route-rerun70")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN70.json"
                : HasFlag("-d19-r03-route-rerun69")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN69.json"
                : HasFlag("-d19-r03-route-rerun68")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN68.json"
                : HasFlag("-d19-r03-route-rerun67")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN67.json"
                : HasFlag("-d19-r03-route-rerun66")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN66.json"
                : HasFlag("-d19-r03-route-rerun65")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN65.json"
                : HasFlag("-d19-r03-route-rerun64")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN64.json"
                : HasFlag("-d19-r03-route-rerun63")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN63.json"
                : HasFlag("-d19-r03-route-rerun62")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN62.json"
                : HasFlag("-d19-r03-route-rerun61")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN61.json"
                : HasFlag("-d19-r03-route-rerun60")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN60.json"
                : HasFlag("-d19-r03-route-rerun59")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN59.json"
                : HasFlag("-d19-r03-route-rerun41")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN41.json"
                : HasFlag("-d19-r03-route-rerun42")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN42.json"
                : HasFlag("-d19-r03-route-rerun43")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN43.json"
                : HasFlag("-d19-r03-bootstrap-reject-rerun1")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_BOOTSTRAP_REJECT" + BootstrapRejectFileSuffix() + ".json"
                : HasFlag("-d19-r03-route-rerun47")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN47.json"
                : HasFlag("-d19-r03-route-rerun49")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN49.json"
                : HasFlag("-d19-r03-route-rerun52")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN52.json"
                : HasFlag("-d19-r03-route-rerun53")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN53.json"
                : HasFlag("-d19-r03-route-rerun58")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN58.json"
                : HasFlag("-d19-r03-route-rerun57")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN57.json"
                : HasFlag("-d19-r03-route-rerun56")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN56.json"
                : HasFlag("-d19-r03-route-rerun55")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN55.json"
                : HasFlag("-d19-r03-route-rerun54")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN54.json"
                : HasFlag("-d19-r03-route-rerun51")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN51.json"
                : HasFlag("-d19-r03-route-rerun50")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN50.json"
                : HasFlag("-d19-r03-route-rerun48")
                ? "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json"
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

        private float MaxShipDisplacement()
        {
            var hasStart = false;
            var start = Vector3.zero;
            var maximum = 0f;
            for (var i = 0; i < _observations.Count; i++)
            {
                var observation = _observations[i];
                if (!observation.ship_found || observation.control_mode != D2ControlMode.Ship.ToString()) continue;
                var position = new Vector3(observation.ship_x, observation.ship_y, observation.ship_z);
                if (!hasStart)
                {
                    hasStart = true;
                    start = position;
                    continue;
                }
                maximum = Mathf.Max(maximum, Vector3.Distance(start, position));
            }
            return maximum;
        }

        private float MaxShipHeadingDelta()
        {
            var hasStart = false;
            var start = 0f;
            var maximum = 0f;
            for (var i = 0; i < _observations.Count; i++)
            {
                var observation = _observations[i];
                if (!observation.ship_found || observation.control_mode != D2ControlMode.Ship.ToString()) continue;
                if (!hasStart)
                {
                    hasStart = true;
                    start = observation.ship_yaw;
                    continue;
                }
                maximum = Mathf.Max(maximum, Mathf.Abs(Mathf.DeltaAngle(start, observation.ship_yaw)));
            }
            return maximum;
        }
    }
}
