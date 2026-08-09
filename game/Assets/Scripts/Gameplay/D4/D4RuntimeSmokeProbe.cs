#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Whalebone.Gameplay.D4
{
    /// <summary>Editor-only D4 runtime gate，验证 InitialHarbor 内容层和可操作面板创建完成。</summary>
    internal sealed class D4RuntimeSmokeProbe : MonoBehaviour
    {
        private float _deadline;
        private bool _finished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!Application.isBatchMode || !HasArgument("-d4Smoke") || HasArgument("-d5Smoke")) return;
            new GameObject("D4RuntimeSmokeProbe").AddComponent<D4RuntimeSmokeProbe>();
        }

        private void Start() { _deadline = Time.realtimeSinceStartup + 15f; }

        private void Update()
        {
            if (_finished) return;
            bool bootstrap = FindFirstObjectByType<D4RuntimeBootstrap>(FindObjectsInactive.Include) != null;
            bool canvas = GameObject.Find("D4Canvas") != null;
            bool controls = GameObject.Find("D4Controls") != null;
            bool player = GameObject.Find("D2_PlayerAvatar") != null;
            if (bootstrap && canvas && controls && player)
            {
                Debug.Log("[D4 Smoke] InitialHarbor runtime objects verified: bootstrap, canvas, controls, player");
                Finish(0);
                return;
            }
            if (Time.realtimeSinceStartup >= _deadline)
            {
                Debug.LogError("[D4 Smoke] runtime objects not ready: bootstrap=" + bootstrap + ", canvas=" + canvas + ", controls=" + controls + ", player=" + player);
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
