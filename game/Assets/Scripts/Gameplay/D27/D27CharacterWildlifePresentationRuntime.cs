using UnityEngine;
using UnityEngine.SceneManagement;
using Whalebone.Gameplay.D2;

namespace Whalebone.Gameplay.D27
{
    /// <summary>
    /// D27 角色与动物表现层接线器。
    /// 它只恢复已有准入 Prefab 的 Renderer，并确保玩家的动作表现组件存在；
    /// NPC/动物事实仍必须由 D20 只读快照驱动，本类不创建或修改生态/关系状态。
    /// </summary>
    public sealed class D27CharacterWildlifePresentationRuntime : MonoBehaviour
    {
        private const string RootName = "D27CharacterWildlifePresentation";
        private bool _applied;
        private float _nextRetry;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D27CharacterWildlifePresentationRuntime>(FindObjectsInactive.Include) != null)
                return;
            new GameObject(RootName).AddComponent<D27CharacterWildlifePresentationRuntime>();
        }

        private void Start()
        {
            ApplyIfReady();
        }

        private void Update()
        {
            if (_applied || Time.unscaledTime < _nextRetry) return;
            _nextRetry = Time.unscaledTime + 0.5f;
            ApplyIfReady();
        }

        private void ApplyIfReady()
        {
            if (SceneManager.GetActiveScene().name != "InitialHarbor") return;
            var player = GameObject.Find("D2_PlayerAvatar");
            if (player == null)
            {
                var controller = FindFirstObjectByType<D2CharacterController>(FindObjectsInactive.Include);
                player = controller == null ? null : controller.gameObject;
            }
            var crew = GameObject.Find("ph_crew_male");
            var candidate = player == null
                ? null
                : player.GetComponent<D20PlayerCandidatePresentationBridge>();
            var candidateInstalled = candidate != null && candidate.IsInstalled;
            // The opt-in D20 bridge owns the decision to hide the legacy
            // player renderers. D27 may restore the formal fallback path, but
            // it must not resurrect a second player after candidate install.
            var playerRendererCount = player == null
                ? 0
                : player.GetComponentsInChildren<Renderer>(true).Length;
            var restoredPlayerRendererCount = candidateInstalled
                ? 0
                : RestoreRenderers(player);
            var crewRendererCount = RestoreRenderers(crew);

            // D2 creates the visual player after this component's Start(). Do not
            // mark the presentation as applied merely because the scene crew
            // anchor exists; that used to make the first pass consume the retry
            // path and left the real player without its animation controller.
            if (player == null || !D2CharacterVisualProbe.HasCharacterVisual(player))
            {
                Debug.LogWarning("[D27/Presentation] 玩家表现对象尚未就绪，等待 D2 完成实例化");
                return;
            }

            var animationController = player.GetComponent<D2CharacterAnimationController>();
            if (animationController == null && player.GetComponent<D2CharacterController>() != null)
                player.AddComponent<D2CharacterAnimationController>();

            if (playerRendererCount == 0 && player.GetComponentsInChildren<Renderer>(true).Length == 0)
            {
                Debug.LogWarning("[D27/Presentation] 玩家已实例化但没有 Renderer，保留回退并继续重试");
                return;
            }

            _applied = true;
            Debug.Log("[D27/Presentation] 角色表现已恢复：playerRenderers=" + playerRendererCount +
                ", restored=" + restoredPlayerRendererCount +
                ", crewRenderers=" + crewRendererCount +
                ", candidateOwned=" + candidateInstalled + ", D20 facts remain read-only");
        }

        private static int RestoreRenderers(GameObject root)
        {
            if (root == null) return 0;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var restored = 0;
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null) continue;
                if (!renderer.enabled)
                {
                    renderer.enabled = true;
                    restored++;
                }
                renderer.allowOcclusionWhenDynamic = true;
            }
            return restored;
        }
    }
}
