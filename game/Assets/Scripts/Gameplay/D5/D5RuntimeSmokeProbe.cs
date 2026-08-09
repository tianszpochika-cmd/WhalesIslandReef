#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Whalebone.Gameplay.D5
{
    /// <summary>Editor-only D5 runtime gate，验证海图、12 个岛标记、UI 和角色对象。</summary>
    internal sealed class D5RuntimeSmokeProbe : MonoBehaviour
    {
        private float _deadline;
        private bool _finished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!Application.isBatchMode || !HasArgument("-d5Smoke")) return;
            new GameObject("D5RuntimeSmokeProbe").AddComponent<D5RuntimeSmokeProbe>();
        }

        private void Start() { _deadline = Time.realtimeSinceStartup + 20f; }

        private void Update()
        {
            if (_finished) return;
            bool bootstrap = FindFirstObjectByType<D5RuntimeBootstrap>(FindObjectsInactive.Include) != null;
            bool canvas = GameObject.Find("D5Canvas") != null;
            bool map = GameObject.Find("D5WorldMap") != null;
            int markerCount = 0;
            foreach (var island in Whalebone.Systems.D5.D5IslandCatalog.All)
                if (GameObject.Find("D5IslandMarker_" + island.Id) != null) markerCount++;
            bool player = GameObject.Find("D2_PlayerAvatar") != null;
            if (bootstrap && canvas && map && markerCount == 12 && player)
            {
                Debug.Log("[D5 Smoke] InitialHarbor verified: bootstrap, canvas, world map, 12 island markers, player");
                Finish(0);
                return;
            }
            if (Time.realtimeSinceStartup >= _deadline)
            {
                Debug.LogError("[D5 Smoke] runtime objects not ready: bootstrap=" + bootstrap + ", canvas=" + canvas + ", map=" + map + ", markers=" + markerCount + ", player=" + player);
                Finish(1);
            }
        }

        private void Finish(int code)
        {
            _finished = true;
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => EditorApplication.Exit(code);
#else
            Application.Quit(code);
#endif
        }

        private static bool HasArgument(string argument)
        {
            foreach (var value in System.Environment.GetCommandLineArgs()) if (value == argument) return true;
            return false;
        }
    }
}
