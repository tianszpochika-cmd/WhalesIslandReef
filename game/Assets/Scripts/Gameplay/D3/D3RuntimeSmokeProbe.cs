#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Whalebone.Gameplay.D3
{
    /// <summary>
    /// Editor-only runtime probe used by the batch smoke gate. It is created after
    /// Play Mode domain reload, so the check is not lost when Unity enters Play Mode.
    /// </summary>
    internal sealed class D3RuntimeSmokeProbe : MonoBehaviour
    {
        private float _deadline;
        private bool _finished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Application.isBatchMode)
                new GameObject("D3RuntimeSmokeProbe").AddComponent<D3RuntimeSmokeProbe>();
        }

        private void Start()
        {
            _deadline = Time.realtimeSinceStartup + 15f;
        }

        private void Update()
        {
            if (_finished) return;

            bool foundBootstrap = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include) != null;
            bool foundCanvas = GameObject.Find("D3Canvas") != null;
            int markers = 0;
            foreach (var portId in new[] { "santa_margarita", "green_cay", "driftwood_isle" })
                if (GameObject.Find("D3_Port_" + portId) != null) markers++;

            if (foundBootstrap && foundCanvas && markers == 3 && GameObject.Find("D2_PlayerAvatar") != null)
            {
                Debug.Log("[D3 Smoke] InitialHarbor runtime objects verified: bootstrap, canvas, player, 3 port markers");
                Finish(0);
                return;
            }

            if (Time.realtimeSinceStartup >= _deadline)
            {
                Debug.LogError("[D3 Smoke] runtime objects not ready: bootstrap=" + foundBootstrap +
                    ", canvas=" + foundCanvas + ", markers=" + markers);
                Finish(1);
            }
        }

        private void Finish(int exitCode)
        {
            _finished = true;
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
#else
            Application.Quit(exitCode);
#endif
        }
    }
}
