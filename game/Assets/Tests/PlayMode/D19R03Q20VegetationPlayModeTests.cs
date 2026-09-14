using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.D19R03Q20
{
    /// <summary>
    /// Q20-A03 runtime containment guard for the vegetation owner.  This is a
    /// measurable presentation check, not a formal asset-admission decision:
    /// visible R03 vegetation must sit on the owned terrain collider, while
    /// legacy palm renderers must stay quarantined until a ready asset exists.
    /// </summary>
    public sealed class D19R03Q20VegetationPlayModeTests
    {
        [UnityTest]
        public IEnumerator VisibleR03VegetationUsesOwnedTerrainContactAndLegacyPalmsStayQuarantined()
        {
            yield return LoadInitialHarborAndWaitReady();

            var presentation = GameObject.Find("D19_R03_HarborPresentation");
            Assert.IsNotNull(presentation, "InitialHarbor 缺少 R03 港口表现根。");

            var terrain = FindNamedTransform(presentation.transform, "D19_TerrainRoot")?.GetComponent<Collider>();
            Assert.IsNotNull(terrain, "R03 缺少自有 D19_TerrainRoot Collider。");
            Assert.IsTrue(terrain.enabled && !terrain.isTrigger,
                "D19_TerrainRoot 必须是启用的实体碰撞，才能作为植被接地 owner。");

            var vegetation = FindNamedTransform(presentation.transform, "D19_R03_VegetationAndRocks");
            Assert.IsNotNull(vegetation, "R03 缺少 D19_R03_VegetationAndRocks 表现根。");

            var visible = new List<Renderer>();
            foreach (var renderer in vegetation.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                    visible.Add(renderer);
            }

            var failures = new List<string>();
            for (var i = 0; i < visible.Count; i++)
            {
                var renderer = visible[i];
                var bounds = renderer.bounds;
                var origin = new Vector3(bounds.center.x, bounds.max.y + 1f, bounds.center.z);
                RaycastHit hit;
                var hasGround = terrain.Raycast(new Ray(origin, Vector3.down), out hit, 32f);
                var error = hasGround ? Mathf.Abs(bounds.min.y - hit.point.y) : float.PositiveInfinity;
                if (!hasGround || error > 0.08f)
                {
                    failures.Add(renderer.name +
                        " ground=" + (hasGround ? hit.point.y.ToString("F3") : "missing") +
                        " minY=" + bounds.min.y.ToString("F3") +
                        " error=" + error.ToString("F3"));
                }
            }

            var legacyRendererCount = 0;
            var legacyRoots = 0;
            foreach (var root in UnityEngine.Object.FindObjectsByType<GameObject>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (root == null || !string.Equals(root.name, "ph_palm_tree",
                        StringComparison.OrdinalIgnoreCase)) continue;

                legacyRoots++;
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                        legacyRendererCount++;
                }
            }

            Debug.Log("[Q20/A03] visibleR03Vegetation=" + visible.Count +
                      ", grounded=" + (visible.Count - failures.Count) +
                      ", failures=" + failures.Count +
                      ", legacyPalmRoots=" + legacyRoots +
                      ", activeLegacyPalmRenderers=" + legacyRendererCount +
                      ", terrain=" + terrain.name);

            Assert.GreaterOrEqual(visible.Count, 24,
                "R03 当前可见 vegetation owner 应保留已审计的 24 个 shrubs/rocks Renderer。");
            Assert.IsEmpty(failures,
                "R03 可见 shrubs/rocks 必须落在自有 D19_TerrainRoot 附近，不能再次悬空或穿入地面。");
            Assert.GreaterOrEqual(legacyRoots, 1,
                "场景应保留 legacy ph_palm_tree 引用以供 D26 quarantine 检查。");
            Assert.AreEqual(0, legacyRendererCount,
                "ready palm 资源进入前，legacy ph_palm_tree Renderer 必须保持隔离。");
        }

        [UnityTest]
        public IEnumerator NatureKitPalmCandidateUsesOwnedTerrainContactAndRemainsPresentationOnly()
        {
            yield return LoadInitialHarborAndWaitReady();

            var presentation = GameObject.Find("D19_R03_HarborPresentation");
            Assert.IsNotNull(presentation, "InitialHarbor 缺少 R03 港口表现根。");
            var terrain = FindNamedTransform(presentation.transform, "D19_TerrainRoot")?.GetComponent<Collider>();
            Assert.IsNotNull(terrain, "R03 缺少自有 D19_TerrainRoot Collider。");

            var candidate = FindNamedTransform(presentation.transform, "KenneyNaturePalmCandidate");
            Assert.IsNotNull(candidate,
                "Q20-A04 候选棕榈没有进入 R03 展示 prefab；不得用资源清单代替运行时证据。");

            var renderers = candidate.GetComponentsInChildren<Renderer>(true);
            Assert.AreEqual(1, renderers.Length,
                "Q20-A04 当前只准接入单 Renderer 的候选棕榈，避免重新引入悬空叶片层级。");
            Assert.IsTrue(renderers[0].enabled && renderers[0].gameObject.activeInHierarchy,
                "Q20-A04 候选棕榈必须是可见的 presentation-only Renderer。");

            var bounds = renderers[0].bounds;
            var origin = new Vector3(bounds.center.x, bounds.max.y + 1f, bounds.center.z);
            var hasGround = terrain.Raycast(new Ray(origin, Vector3.down), out var hit, 32f);
            var groundError = hasGround ? Mathf.Abs(bounds.min.y - hit.point.y) : float.PositiveInfinity;
            var colliders = candidate.GetComponentsInChildren<Collider>(true);

            Debug.Log("[Q20/A04] candidateRenderers=" + renderers.Length +
                      ", candidateColliders=" + colliders.Length +
                      ", boundsMinY=" + bounds.min.y.ToString("F3") +
                      ", ground=" + (hasGround ? hit.point.y.ToString("F3") : "missing") +
                      ", groundError=" + groundError.ToString("F3") +
                      ", resourceState=Candidate/ImportVerified");

            Assert.IsTrue(hasGround, "Q20-A04 候选棕榈下方必须能命中 D19_TerrainRoot。");
            Assert.LessOrEqual(groundError, 0.08f,
                "Q20-A04 候选棕榈必须以实际 Renderer bounds 接地，不能再次悬空。");
            Assert.AreEqual(0, colliders.Length,
                "Q20-A04 只接入展示候选；碰撞权仍属于 D6/D19_TerrainRoot。");
        }

        private static IEnumerator LoadInitialHarborAndWaitReady()
        {
            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            var ready = bootstrapType.GetProperty("IsReady");
            Assert.IsNotNull(ready, "D2RuntimeBootstrap 缺少 IsReady 诊断。");

            Component bootstrap = null;
            var deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = UnityEngine.Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "InitialHarbor D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "InitialHarbor D2 运行时接线未完成。");
        }

        private static Transform FindNamedTransform(Transform root, string name)
        {
            if (root == null) return null;
            if (string.Equals(root.name, name, StringComparison.Ordinal)) return root;
            for (var i = 0; i < root.childCount; i++)
            {
                var result = FindNamedTransform(root.GetChild(i), name);
                if (result != null) return result;
            }

            return null;
        }

        private static Type FindLoadedType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }
    }
}
