// ============================================================
// D2RuntimeBootstrap —— InitialHarbor 的 D2 运行时组装入口
//
// 只在场景加载后创建一次。它把 D2 组件接线，但不拥有航行/好感/存档数值。
// 缺少 D1 占位资产时自动生成可见的玩家、船和海面，保证可玩闭环不被资源卡住。
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Whalebone.Core;
using Whalebone.Agent;
using Whalebone.Gameplay.World;
using Whalebone.Gameplay.UI.Dialogue;
using Whalebone.Systems.D2;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D2
{
    public sealed class D2RuntimeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D2RuntimeBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D2RuntimeBootstrap").AddComponent<D2RuntimeBootstrap>();
            Debug.Log("[D2] InitialHarbor 运行时入口已创建");
        }

        private void Start()
        {
            BuildRuntime();
        }

        private void BuildRuntime()
        {
            var ship = EnsureShip();
            var npc = EnsureCrewAnchor(ship);
            var player = EnsurePlayer(ship);
            var camera = EnsureCamera();

            var character = player.GetComponent<D2CharacterController>();
            if (character == null) character = player.AddComponent<D2CharacterController>();

            var shipController = ship.GetComponent<D2ShipController>();
            if (shipController == null) shipController = ship.AddComponent<D2ShipController>();
            shipController.Initialize(ship.transform.position, ship.transform.eulerAngles.y);
            if (Whalebone.Systems.Save.GameState.CurrentPlayer != null &&
                Whalebone.Systems.Save.GameState.CurrentPlayer.Location != null &&
                Whalebone.Systems.Save.GameState.CurrentPlayer.Location.Type == "open_sea")
            {
                player.transform.position = ship.transform.position - ship.transform.forward * 5f + Vector3.up;
            }

            var dialogue = gameObject.GetComponent<D2DialogueController>();
            if (dialogue == null) dialogue = gameObject.AddComponent<D2DialogueController>();

            var router = gameObject.GetComponent<D2ControlRouter>();
            if (router == null) router = gameObject.AddComponent<D2ControlRouter>();
            router.Initialize(player, ship, npc, character, shipController, dialogue, camera);

            var dayNight = gameObject.GetComponent<DayNightController>();
            if (dayNight == null) dayNight = gameObject.AddComponent<DayNightController>();
            ApplySavedClock(dayNight);

            dialogue.ReplyReceived -= OnDialogueReply;
            dialogue.ReplyReceived += OnDialogueReply;

            var ocean = gameObject.GetComponent<OceanAdapter>();
            if (ocean == null) ocean = gameObject.AddComponent<OceanAdapter>();

            var cameraFollow = camera.GetComponent<CameraFollow>();
            if (cameraFollow == null) cameraFollow = camera.gameObject.AddComponent<CameraFollow>();
            cameraFollow.Target = player.transform;

            var hint = gameObject.GetComponent<D2ControlHint>();
            if (hint == null) hint = gameObject.AddComponent<D2ControlHint>();
            hint.Initialize(router);

            Debug.Log("[D2] 运行时闭环已接线：人物、船、相机、海面、昼夜、对话");
        }

        public void SaveProgress()
        {
            var ship = FindFirstObjectByType<D2ShipController>(FindObjectsInactive.Include);
            var clock = GetComponent<DayNightController>();
            if (ship == null || clock == null) return;
            try
            {
                SaveSystem.SaveD2Runtime(ship.Snapshot, clock.Snapshot);
                Debug.Log("[D2/Save] 运行时快照已保存");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[D2/Save] 快照保存失败，保留上一次有效存档：" + ex.Message);
            }
        }

        private void OnDialogueReply(CrewAgent agent, AgentReply reply)
        {
            bool changed = CrewRelationSystem.ApplyAgentProposal(agent, reply);
            Debug.Log("[D2/Relation] " + agent.DisplayName + " 好感提议已裁决：" +
                      reply.AffinityChange + "，实际变化=" + changed);
            SaveProgress();
        }

        private static void ApplySavedClock(DayNightController dayNight)
        {
            var location = Whalebone.Systems.Save.GameState.CurrentPlayer == null
                ? null : Whalebone.Systems.Save.GameState.CurrentPlayer.Location;
            if (location == null) return;
            dayNight.ApplySnapshot(new DayNightSnapshot(location.DayIndex, location.NormalizedTime, false));
        }

        private static GameObject EnsureShip()
        {
            var ship = GameObject.Find("ph_ship_fishing");
            if (ship != null) return ship;

            ship = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ship.name = "ph_ship_fishing";
            ship.transform.position = new Vector3(0f, 0.6f, 0f);
            ship.transform.localScale = new Vector3(4.5f, 0.8f, 8f);
            ApplyColor(ship, new Color(0.42f, 0.22f, 0.09f));
            Debug.LogWarning("[D2] 缺少 ph_ship_fishing，已创建运行时占位船");
            return ship;
        }

        private static GameObject EnsureCrewAnchor(GameObject ship)
        {
            var crew = GameObject.Find("ph_crew_male");
            if (crew != null) return crew;

            crew = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            crew.name = "ph_crew_male";
            crew.transform.position = ship.transform.position + new Vector3(2.5f, 1f, 1f);
            crew.transform.localScale = new Vector3(0.55f, 1f, 0.55f);
            ApplyColor(crew, new Color(0.65f, 0.42f, 0.2f));
            Debug.LogWarning("[D2] 缺少 ph_crew_male，已创建老水手占位人物");
            return crew;
        }

        private static GameObject EnsurePlayer(GameObject ship)
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            if (player != null) return player;

            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "D2_PlayerAvatar";
            player.transform.position = ship.transform.position + new Vector3(0f, 1f, -5f);
            player.transform.localScale = new Vector3(0.55f, 1f, 0.55f);
            ApplyColor(player, new Color(0.12f, 0.55f, 0.85f));
            return player;
        }

        private static Camera EnsureCamera()
        {
            var camera = Camera.main;
            if (camera != null) return camera;
            camera = FindFirstObjectByType<Camera>();
            if (camera != null) return camera;

            var go = new GameObject("D2_MainCamera");
            camera = go.AddComponent<Camera>();
            camera.tag = "MainCamera";
            return camera;
        }

        private static void ApplyColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) return;
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            renderer.sharedMaterial = material;
        }
    }

    public sealed class D2CharacterController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.2f;
        private bool _inputEnabled;

        public void SetInputEnabled(bool enabled) => _inputEnabled = enabled;

        private void Update()
        {
            if (!_inputEnabled || Time.timeScale <= 0f || Keyboard.current == null) return;
            Vector2 input = Vector2.zero;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (input.sqrMagnitude < 0.001f) return;

            input = Vector2.ClampMagnitude(input, 1f);
            var camera = Camera.main;
            Vector3 forward = camera == null ? Vector3.forward : camera.transform.forward;
            Vector3 right = camera == null ? Vector3.right : camera.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward = forward.sqrMagnitude < 0.001f ? Vector3.forward : forward.normalized;
            right = right.sqrMagnitude < 0.001f ? Vector3.right : right.normalized;
            transform.position += (right * input.x + forward * input.y) * moveSpeed * Time.deltaTime;
        }
    }
}
