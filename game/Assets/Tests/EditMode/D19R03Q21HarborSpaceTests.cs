using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Save.Tests
{
    /// <summary>
    /// Q21 对当前 R03 表现 Prefab 做静态空间审计。
    ///
    /// 本测试只读检查表现层：四个服务建筑的可见 footprint、地面接触、
    /// 路网/岸线/码头根节点、碰撞体归属，以及 InitialHarbor 的稳定路线锚点。
    /// 它不能替代 Player 中的真实路线、镜头和鼠标可达性验收。
    /// </summary>
    public sealed class D19R03Q21HarborSpaceTests
    {
        private const string PrefabPath =
            "Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab";

        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";

        private GameObject _instance;

        private sealed class Footprint
        {
            public string Name;
            public Bounds Bounds;
            public int RendererCount;
            public int MissingMaterialCount;
        }

        [TearDown]
        public void TearDown()
        {
            if (_instance != null) Object.DestroyImmediate(_instance);
            _instance = null;
        }

        [Test]
        public void R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, "Q21 R03 presentation Prefab must be importable.");

            _instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Assert.IsNotNull(_instance, "Q21 R03 presentation Prefab could not be instantiated.");

            var names = new[] { "MarketVisual", "WarehouseVisual", "TavernVisual", "ShipyardVisual" };
            var footprints = new List<Footprint>(names.Length);
            foreach (var name in names)
            {
                var root = FindByName(_instance.transform, name);
                Assert.IsNotNull(root, "Missing service presentation root: " + name);

                var renderers = root.GetComponentsInChildren<Renderer>(true);
                Assert.IsNotEmpty(renderers, name + " has no visible Renderer footprint.");
                var bounds = renderers[0].bounds;
                int missingMaterials = 0;
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (i > 0) bounds.Encapsulate(renderers[i].bounds);
                    foreach (var material in renderers[i].sharedMaterials)
                        if (material == null) missingMaterials++;
                }

                Assert.AreEqual(0, missingMaterials, name + " has missing material slots.");
                Assert.Greater(bounds.size.x, 0.05f, name + " has no measurable X footprint.");
                Assert.Greater(bounds.size.z, 0.05f, name + " has no measurable Z footprint.");
                Assert.Greater(bounds.size.y, 0.05f, name + " has no measurable vertical footprint.");
                Assert.That(bounds.min.y, Is.GreaterThanOrEqualTo(-0.05f),
                    name + " visible bounds are below the R03 ground contract.");

                footprints.Add(new Footprint
                {
                    Name = name,
                    Bounds = bounds,
                    RendererCount = renderers.Length,
                    MissingMaterialCount = missingMaterials
                });
                Debug.Log("[Q21] footprint " + Describe(footprints[footprints.Count - 1]));
            }

            for (int i = 0; i < footprints.Count; i++)
            {
                for (int j = i + 1; j < footprints.Count; j++)
                {
                    var first = footprints[i];
                    var second = footprints[j];
                    bool overlapX = first.Bounds.min.x < second.Bounds.max.x &&
                        second.Bounds.min.x < first.Bounds.max.x;
                    bool overlapZ = first.Bounds.min.z < second.Bounds.max.z &&
                        second.Bounds.min.z < first.Bounds.max.z;
                    Assert.IsFalse(overlapX && overlapZ,
                        first.Name + " footprint overlaps " + second.Name + ".");
                }
            }

            var requiredGroups = new[]
            {
                "D19_R03_IslandSurface",
                "D19_R03_RoadNetwork",
                "D19_R03_ShorelineAndWater",
                "D19_R03_DockAndBerth",
                "D19_R03_PortProps",
                "D19_R03_MarketEntryAndStreet"
            };
            foreach (var groupName in requiredGroups)
            {
                var group = FindByName(_instance.transform, groupName);
                Assert.IsNotNull(group, "Missing R03 spatial group: " + groupName);
                Assert.IsNotEmpty(group.GetComponentsInChildren<Renderer>(true),
                    groupName + " has no presentation renderer.");
            }

            var terrain = FindByName(_instance.transform, "D19_TerrainRoot");
            Assert.IsNotNull(terrain, "R03 terrain collision owner is missing.");
            var terrainColliders = terrain.GetComponentsInChildren<Collider>(true);
            Assert.AreEqual(1, terrainColliders.Length,
                "R03 presentation must keep one terrain collision owner; route collider changes belong to Q07.");
            Assert.AreEqual("D19_TerrainRoot", terrainColliders[0].name);
            Assert.IsInstanceOf<MeshCollider>(terrainColliders[0]);
            Assert.IsFalse(terrainColliders[0].isTrigger);

            var colliders = _instance.GetComponentsInChildren<Collider>(true);
            var externalColliderOwners = new HashSet<string>();
            foreach (var collider in colliders)
            {
                if (IsDescendantOrSelf(collider.transform, terrain.transform)) continue;
                var owner = TopLevelName(collider.transform, _instance.transform);
                externalColliderOwners.Add(owner);
                Assert.AreEqual("D27_R03_CharacterWildlifePresentation", owner,
                    "Only the nested D27 owner may contribute non-terrain colliders to the R03 Prefab; " +
                    "R03 route collider changes belong to Q07.");
            }

            var sceneText = File.ReadAllText(Path.Combine(Application.dataPath, "Scenes/InitialHarbor.unity"));
            StringAssert.Contains("m_Name: D2_PlayerSpawnAnchor", sceneText,
                "R03 presentation changes must not remove the stable player spawn anchor.");
            StringAssert.Contains("m_Name: D2_DisembarkAnchor", sceneText,
                "R03 presentation changes must not remove the stable disembark anchor.");

            Debug.Log(string.Format(
                "[Q21] buildings={0}, footprints={1}, required_groups={2}, colliders={3}, " +
                "terrain_colliders={4}, external_collider_owners={5}, market={6}, warehouse={7}, " +
                "tavern={8}, shipyard={9}",
                footprints.Count,
                footprints.Count,
                requiredGroups.Length,
                colliders.Length,
                terrainColliders.Length,
                string.Join(",", new List<string>(externalColliderOwners).ToArray()),
                Describe(footprints[0]),
                Describe(footprints[1]),
                Describe(footprints[2]),
                Describe(footprints[3])));
        }

        private static string Describe(Footprint footprint)
        {
            var bounds = footprint.Bounds;
            return string.Format(
                "{0}:center=({1:F2},{2:F2},{3:F2}),size=({4:F2},{5:F2},{6:F2}),renderers={7},missing={8}",
                footprint.Name,
                bounds.center.x, bounds.center.y, bounds.center.z,
                bounds.size.x, bounds.size.y, bounds.size.z,
                footprint.RendererCount,
                footprint.MissingMaterialCount);
        }

        private static Transform FindByName(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindByName(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static bool IsDescendantOrSelf(Transform candidate, Transform ancestor)
        {
            var current = candidate;
            while (current != null)
            {
                if (current == ancestor) return true;
                current = current.parent;
            }
            return false;
        }

        private static string TopLevelName(Transform candidate, Transform root)
        {
            var current = candidate;
            while (current.parent != null && current.parent != root)
                current = current.parent;
            return current.name;
        }
    }
}
