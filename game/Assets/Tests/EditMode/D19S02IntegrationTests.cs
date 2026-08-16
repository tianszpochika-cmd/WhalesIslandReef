#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhalesIslandReef.Tests.D19S02
{
    public sealed class D19S02IntegrationTests
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string IntegrationPrefabPath = "Assets/Prefabs/Environment/D19/Integration/D19_SaintMargaret_Root.prefab";

        [Test]
        public void IntegrationPrefabContainsOrderedRootsAndNestedPrefabInstances()
        {
            var root = PrefabUtility.LoadPrefabContents(IntegrationPrefabPath);
            try
            {
                Assert.That(root, Is.Not.Null);
                var required = new[]
                {
                    "D19_EnvironmentRoot", "D19_PortBuildingsRoot", "D19_BiomeRoot", "D19_ShipsSeaRoot",
                    "D19_PresentationRoot", "D19_AmbienceRoot", "D19_CharactersAnimalsRoot", "ExistingGameplayAnchors"
                };
                foreach (var name in required)
                    Assert.That(FindChild(root.transform, name), Is.Not.Null, "missing root " + name);
                Assert.That(root.GetComponentsInChildren<Transform>(true)
                    .Count(transform => PrefabUtility.GetCorrespondingObjectFromSource(transform.gameObject) != null), Is.GreaterThan(20));
                Assert.That(FindChild(root.transform, "D19_DevOverlay").gameObject.activeSelf, Is.False);
                Assert.That(FindChild(root.transform, "D19_S02_CaptureCamera").GetComponent<Camera>().enabled, Is.False);
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [Test]
        public void IntegrationPrefabHasNoMissingOrErrorMaterials()
        {
            var root = PrefabUtility.LoadPrefabContents(IntegrationPrefabPath);
            try
            {
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    Assert.That(renderer.sharedMaterial, Is.Not.Null, renderer.name + " has no material");
                    Assert.That(renderer.sharedMaterial.shader, Is.Not.Null, renderer.name + " has no shader");
                    Assert.That(renderer.sharedMaterial.shader.name, Is.Not.EqualTo("Hidden/InternalErrorShader"), renderer.name + " uses Unity error shader");
                    Assert.That(renderer.sharedMaterial.shader.name, Is.Not.EqualTo("Standard"), renderer.name + " retains the legacy Standard shader in the URP integration");
                }
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [Test]
        public void FormalScenePreservesCompatibilityNodesAndStableAnchors()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            try
            {
                Assert.That(GameObject.Find("D19_SaintMargaret_Root"), Is.Not.Null);
                foreach (var name in new[] { "Dock", "DockDeck", "Gangplank", "Beach", "Sea", "Main Camera", "Harbor UI", "HarborControls" })
                    Assert.That(GameObject.Find(name), Is.Not.Null, "missing compatibility node " + name);
                foreach (var id in new[]
                {
                    "D2_PlayerAvatar", "santa_margarita", "building_market_santa_margarita", "market_queue_santa_margarita",
                    "building_warehouse_santa_margarita", "warehouse_queue_santa_margarita", "building_tavern_santa_margarita",
                    "building_shipyard_santa_margarita", "shipyard_repair_santa_margarita", "building_saint_marys_dock",
                    "DockGround", "berth_santa_margarita", "GangplankGround", "departure_channel_santa_margarita",
                    "return_channel_santa_margarita", "save_point_santa_margarita"
                })
                    Assert.That(GameObject.Find("D19_Anchor_" + id), Is.Not.Null, "missing anchor " + id);
                Assert.That(GameObject.Find("D19_DevOverlay"), Is.Null, "development overlay must be inactive by default");
                foreach (var legacyName in new[] { "Dock", "Beach", "Sea" })
                {
                    var legacy = GameObject.Find(legacyName);
                    foreach (var renderer in legacy.GetComponentsInChildren<Renderer>(true))
                        Assert.That(renderer.enabled, Is.False, legacyName + " placeholder renderer remains visible");
                }
            }
            finally
            {
                if (scene.IsValid()) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        [Test]
        public void FormalSceneKeepsNavigationAndPresentationBoundaries()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            try
            {
                Assert.That(GameObject.Find("D19_TerrainNavMesh"), Is.Not.Null);
                Assert.That(GameObject.Find("WaterRoot"), Is.Not.Null);
                Assert.That(GameObject.Find("D19LightingRoot"), Is.Not.Null);
                Assert.That(GameObject.Find("D19_AmbienceRoot"), Is.Not.Null);
                Assert.That(GameObject.Find("D19_S02_CaptureCamera").GetComponent<Camera>().enabled, Is.False);
            }
            finally
            {
                if (scene.IsValid()) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindChild(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif
