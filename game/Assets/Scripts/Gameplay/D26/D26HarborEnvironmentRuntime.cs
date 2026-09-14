using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Gameplay.D26
{
    /// <summary>
    /// D26 港口环境表现接线器。
    ///
    /// R03 的港口 Prefab 已经包含经过准入审计的建筑、道路、植被、岩石、
    /// 码头和水面网格；上一轮为了避免重复碰撞而把其中大量 Renderer 关闭，
    /// 造成玩家看到白色灰盒。本类只恢复唯一的 R03 表现层，不写入港口、
    /// 导航、航行、存档或任何领域事实，也不启用重复 Collider。
    /// </summary>
    public sealed class D26HarborEnvironmentRuntime : MonoBehaviour
    {
        private const string EnvironmentRootName = "D26HarborEnvironment";
        private const string PreferredPresentationName = "D19_R03_HarborPresentation";
        private const string LegacyPresentationName = "D19_R02_HarborPresentation";
        private const string LegacyTreeRootName = "D19_SaintMargaret_Root";
        private const string LegacyPlaceholderTreeRootName = "ph_palm_tree";

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
            if (FindFirstObjectByType<D26HarborEnvironmentRuntime>(FindObjectsInactive.Include) != null)
                return;

            var root = new GameObject(EnvironmentRootName);
            root.AddComponent<D26HarborEnvironmentRuntime>();
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

            var preferred = FindSceneObject(PreferredPresentationName);
            if (preferred == null)
            {
                Debug.LogWarning("[D26/Environment] R03 港口表现层尚未出现，等待下一帧重试");
                return;
            }

            // R02 与 R03 在场景中同时登记会产生重叠网格；R03 是本阶段唯一
            // 正式表现源。这里仅处理表现根，不碰任何稳定碰撞/逻辑根。
            var legacy = FindSceneObject(LegacyPresentationName);
            if (legacy != null && legacy != preferred) legacy.SetActive(false);
            preferred.SetActive(true);

            var restoredRenderers = 0;
            var renderers = preferred.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null) continue;
                // Renderer 是表现所有权；Collider、NavMesh 和 MonoBehaviour
                // 的启用状态由既有 D6/D19/D21 owner 保持不变。
                if (!renderer.enabled)
                {
                    renderer.enabled = true;
                    restoredRenderers++;
                }
                renderer.allowOcclusionWhenDynamic = true;
            }

            var grounding = GroundR03VegetationAgainstOwnedTerrain(preferred);
            var legacyTrees = DisableLegacyTreeRenderers();

            _applied = true;
            Debug.Log("[D26/Environment] R03 港口表现层已恢复：renderers=" + renderers.Length +
                ", restored=" + restoredRenderers + ", vegetation_grounded=" + grounding.grounded +
                ", vegetation_hidden_without_terrain=" + grounding.hidden +
                ", legacy_tree_renderers_disabled=" + legacyTrees +
                ", duplicate colliders untouched");
        }

        private struct VegetationGroundingResult
        {
            public int grounded;
            public int hidden;
        }

        private static VegetationGroundingResult GroundR03VegetationAgainstOwnedTerrain(GameObject preferred)
        {
            var result = new VegetationGroundingResult();
            if (preferred == null) return result;

            var vegetation = FindChild(preferred.transform, "D19_R03_VegetationAndRocks");
            // InitialHarbor 可能同时存在 legacy/稳定逻辑地形和 R03 表现地形，
            // 不能用全场景同名查找，否则接地修复与审计可能命中不同 Collider。
            // R03 植被只允许以同一表现 Prefab 下的 D19_TerrainRoot 为依据。
            var terrainTransform = FindChild(preferred.transform, "D19_TerrainRoot");
            var terrainCollider = terrainTransform == null ? null : terrainTransform.GetComponent<Collider>();
            if (vegetation == null || terrainCollider == null || !terrainCollider.enabled)
            {
                Debug.LogWarning("[D26/Environment] R03 vegetation grounding skipped: owned D19_TerrainRoot collider unavailable");
                return result;
            }

            var children = vegetation.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child == null || (child.name.IndexOf("Shrub", System.StringComparison.OrdinalIgnoreCase) < 0 &&
                                      child.name.IndexOf("Rock", System.StringComparison.OrdinalIgnoreCase) < 0))
                    continue;

                var renderer = child.GetComponent<Renderer>();
                if (renderer == null || !renderer.enabled || !child.gameObject.activeInHierarchy) continue;

                var bounds = renderer.bounds;
                var origin = bounds.center + Vector3.up * Mathf.Max(0.5f, bounds.size.y + 0.5f);
                RaycastHit hit;
                if (!terrainCollider.Raycast(new Ray(origin, Vector3.down), out hit, 32f))
                {
                    renderer.enabled = false;
                    result.hidden++;
                    continue;
                }

                float correction = hit.point.y - bounds.min.y;
                if (Mathf.Abs(correction) > 0.001f)
                    child.position += Vector3.up * correction;
                result.grounded++;
            }

            // Candidate environment roots are presentation-only children of
            // the harbor prefab, not members of VegetationAndRocks. Apply the
            // same owned-terrain contract to the explicitly admitted roots so
            // an imported pivot cannot leave a rock/path slab floating at the
            // water edge. A missing ray hit hides only that renderer hierarchy
            // instead of guessing a world-space height.
            var candidateNames = new[]
            {
                "KenneyCliffBackdrop",
                "KenneyRockShore",
                "KenneyPathBendMarket",
                "KenneyCliffStepsNorth"
            };
            for (var i = 0; i < candidateNames.Length; i++)
            {
                var candidate = FindChild(preferred.transform, candidateNames[i]);
                GroundRendererHierarchy(candidate, terrainCollider, ref result);
            }

            Physics.SyncTransforms();
            return result;
        }

        private static void GroundRendererHierarchy(Transform root, Collider terrainCollider,
            ref VegetationGroundingResult result)
        {
            if (root == null || terrainCollider == null || !root.gameObject.activeInHierarchy) return;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                if (renderers[i] != null) bounds.Encapsulate(renderers[i].bounds);

            var origin = bounds.center + Vector3.up * Mathf.Max(0.5f, bounds.size.y + 0.5f);
            RaycastHit hit;
            if (!terrainCollider.Raycast(new Ray(origin, Vector3.down), out hit, 32f))
            {
                for (var i = 0; i < renderers.Length; i++)
                    if (renderers[i] != null) renderers[i].enabled = false;
                result.hidden++;
                return;
            }

            float correction = hit.point.y - bounds.min.y;
            if (Mathf.Abs(correction) > 0.001f)
                root.position += Vector3.up * correction;
            result.grounded++;
        }

        private static int DisableLegacyTreeRenderers()
        {
            var disabled = 0;
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var sceneRoot = roots[i];
                if (sceneRoot == null) continue;

                // D19_SaintMargaret_Root is a child of the scene's
                // D19_EnvironmentRoot prefab instance, not necessarily a
                // scene-level root. Direct ph_palm_tree placeholders can
                // also be scene roots created by the legacy installer. The
                // previous name check only covered the nested root and left
                // those direct palm renderers active, which made them appear
                // to float above the R03 terrain. Find both admitted legacy
                // tree roots, then quarantine every Renderer under each
                // subtree. This is presentation containment only; it does
                // not alter terrain, navigation, collision, or assets.
                var transforms = sceneRoot.GetComponentsInChildren<Transform>(true);
                for (var j = 0; j < transforms.Length; j++)
                {
                    var legacyRoot = transforms[j];
                    if (legacyRoot == null ||
                        (legacyRoot.name != LegacyTreeRootName &&
                         legacyRoot.name != LegacyPlaceholderTreeRootName))
                        continue;

                    var renderers = legacyRoot.GetComponentsInChildren<Renderer>(true);
                    for (var k = 0; k < renderers.Length; k++)
                    {
                        if (renderers[k] == null || !renderers[k].enabled) continue;
                        renderers[k].enabled = false;
                        disabled++;
                    }
                }
            }
            return disabled;
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (var i = 0; i < root.childCount; i++)
            {
                var result = FindChild(root.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }

        private static GameObject FindSceneObject(string name)
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null) continue;
                if (root.name == name) return root;

                var transforms = root.GetComponentsInChildren<Transform>(true);
                for (var j = 0; j < transforms.Length; j++)
                {
                    var candidate = transforms[j];
                    if (candidate != null && candidate.name == name)
                        return candidate.gameObject;
                }
            }
            return null;
        }
    }
}
