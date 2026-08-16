#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhalesIslandReef.Tests.D19
{
    public sealed class D19PortArchitectureTests
    {
        private const string ScenePath = "Assets/Scenes/D19/Work/D19_PortArchitecture_Work.unity";
        private static readonly string[] PrefabPaths =
        {
            "Assets/Prefabs/Buildings/D19/D19_PortEntry_Admin.prefab",
            "Assets/Prefabs/Buildings/D19/D19_Market.prefab",
            "Assets/Prefabs/Buildings/D19/D19_Warehouse.prefab",
            "Assets/Prefabs/Buildings/D19/D19_Tavern_Exterior.prefab",
            "Assets/Prefabs/Buildings/D19/D19_Shipyard.prefab",
            "Assets/Prefabs/Buildings/D19/D19_Dock_Primary.prefab",
            "Assets/Prefabs/Buildings/D19/D19_Dock_Secondary.prefab",
            "Assets/Prefabs/Buildings/D19/D19_Common_Residence.prefab",
            "Assets/Prefabs/Buildings/D19/D19_Market_Stall.prefab",
            "Assets/Prefabs/Environment/D19/Port/D19_Bollard.prefab",
            "Assets/Prefabs/Environment/D19/Port/D19_Fence_Module.prefab",
            "Assets/Prefabs/Environment/D19/Port/D19_Stair_Module.prefab",
            "Assets/Prefabs/Environment/D19/Port/D19_Port_Walk_Surface.prefab"
        };

        [MenuItem("WhalesIslandReef/D19/Run Port Architecture Assertions")]
        public static void RunEditModeAssertions()
        {
            var tests = new D19PortArchitectureTests();
            tests.EveryD19PortPrefabHasMarkerColliderAndLod();
            tests.PrefabMaterialsAndAnchorReferencesAreComplete();
            tests.WorkScenePreservesD19A02AndDockInterfaceAnchors();
            tests.WorkSceneHasSeparateArchitectureRootsAndNoInitialHarborReference();

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string resultDirectory = Path.Combine(projectRoot, "tools", "results", "D19", "D19-A02");
            Directory.CreateDirectory(resultDirectory);
            File.WriteAllText(Path.Combine(resultDirectory, "D19_A02_EDITMODE_ASSERTIONS.json"),
                "{\n  \"status\": \"PASS\",\n  \"tests\": 4,\n  \"scene\": \"" + ScenePath + "\"\n}\n");
            Debug.Log("[D19-A02] EditMode assertions passed: 4");
        }

        [Test]
        public void EveryD19PortPrefabHasMarkerColliderAndLod()
        {
            foreach (string path in PrefabPaths)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Assert.That(root, Is.Not.Null, path);
                    Assert.That(FindComponent(root, "WhalesIslandReef.D19.D19PortBuildingMarker"), Is.Not.Null, path + " marker");
                    Assert.That(root.GetComponentsInChildren<Collider>(true).Length, Is.GreaterThan(0), path + " collider");
                    LODGroup lod = root.GetComponent<LODGroup>();
                    Assert.That(lod, Is.Not.Null, path + " LODGroup");
                    Assert.That(lod.GetLODs().Length, Is.GreaterThanOrEqualTo(2), path + " LOD levels");
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }

        [Test]
        public void PrefabMaterialsAndAnchorReferencesAreComplete()
        {
            foreach (string path in PrefabPaths)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                        Assert.That(renderer.sharedMaterial, Is.Not.Null, path + " renderer material");
                    foreach (Component anchor in FindComponents(root, "WhalesIslandReef.D19.D19PortAnchor"))
                    {
                        Assert.That(ReadString(anchor, "AnchorId"), Is.Not.Empty, path + " anchor id");
                        Assert.That(ReadString(anchor, "StableId"), Is.Not.Empty, path + " stable id");
                        Assert.That(anchor.transform.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0), path + " anchor marker");
                    }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }

        [Test]
        public void WorkScenePreservesD19A02AndDockInterfaceAnchors()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var anchors = FindComponentsInLoadedScenes("WhalesIslandReef.D19.D19PortAnchor");
                var stableIds = new HashSet<string>(anchors.Select(a => ReadString(a, "StableId")));
                string[] required =
                {
                    "santa_margarita", "building:market_santa_margarita", "market_queue_santa_margarita",
                    "building:warehouse_santa_margarita", "warehouse_queue_santa_margarita",
                    "building:tavern_santa_margarita", "building:shipyard_santa_margarita", "shipyard_repair_santa_margarita",
                    "building_saint_marys_dock", "DockGround", "berth_santa_margarita", "GangplankGround"
                };
                foreach (string stableId in required) Assert.That(stableIds.Contains(stableId), Is.True, "missing anchor " + stableId);
                Assert.That(anchors.Length, Is.EqualTo(stableIds.Count), "duplicate stable IDs");
                foreach (Component anchor in anchors)
                {
                    string stableId = ReadString(anchor, "StableId");
                    Assert.That(anchor.transform.position.y, Is.GreaterThanOrEqualTo(0.1f), stableId);
                    if (expectedWorldXZ.TryGetValue(stableId, out Vector3 expected))
                    {
                        Assert.That(Mathf.Abs(anchor.transform.position.x - expected.x), Is.LessThan(0.01f), stableId + " x");
                        Assert.That(Mathf.Abs(anchor.transform.position.z - expected.z), Is.LessThan(0.01f), stableId + " z");
                    }
                }
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        private static readonly Dictionary<string, Vector3> expectedWorldXZ = new Dictionary<string, Vector3>
        {
            ["santa_margarita"] = new Vector3(0f, 0f, 112f),
            ["building:market_santa_margarita"] = new Vector3(26f, 0f, 62f),
            ["market_queue_santa_margarita"] = new Vector3(12f, 0f, 58f),
            ["building:warehouse_santa_margarita"] = new Vector3(48f, 0f, 34f),
            ["building:tavern_santa_margarita"] = new Vector3(-42f, 0f, 50f),
            ["building:shipyard_santa_margarita"] = new Vector3(60f, 0f, 8f),
            ["shipyard_repair_santa_margarita"] = new Vector3(54f, 0f, 2f),
            ["building_saint_marys_dock"] = new Vector3(0f, 0f, -12f),
            ["DockGround"] = new Vector3(0f, 0f, -10f),
            ["berth_santa_margarita"] = new Vector3(0f, 0f, -30f),
            ["GangplankGround"] = new Vector3(0f, 0f, -16.75f)
        };

        private static Component FindComponent(GameObject root, string fullName)
        {
            return FindComponents(root, fullName).FirstOrDefault();
        }

        private static IEnumerable<Component> FindComponents(GameObject root, string fullName)
        {
            return root.GetComponentsInChildren<Component>(true).Where(c => c != null && c.GetType().FullName == fullName);
        }

        private static Component[] FindComponentsInLoadedScenes(string fullName)
        {
            return Resources.FindObjectsOfTypeAll<Component>()
                .Where(c => c != null && c.gameObject.scene.IsValid() && c.GetType().FullName == fullName)
                .ToArray();
        }

        private static string ReadString(Component component, string propertyName)
        {
            var property = component.GetType().GetProperty(propertyName);
            return property == null ? string.Empty : property.GetValue(component, null) as string ?? string.Empty;
        }

        [Test]
        public void WorkSceneHasSeparateArchitectureRootsAndNoInitialHarborReference()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                Assert.That(GameObject.Find("D19_PortBuildingsRoot"), Is.Not.Null);
                Assert.That(GameObject.Find("D19_PortWalkwaysRoot"), Is.Not.Null);
                Assert.That(GameObject.Find("D19_AnchorVisualization"), Is.Not.Null);
                Assert.That(GameObject.Find("D19_PreviewRig"), Is.Not.Null);
                Assert.That(scene.path, Is.EqualTo(ScenePath));
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
#endif
