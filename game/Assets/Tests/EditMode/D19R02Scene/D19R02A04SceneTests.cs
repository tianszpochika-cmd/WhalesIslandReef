#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WhalesIslandReef.Tests.D19R02A04
{
    public sealed class D19R02A04SceneTests
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string PresentationPrefabPath = "Assets/Prefabs/Environment/D19/Integration/R02/D19_R02_HarborPresentation.prefab";
        private const string AnchorPrefabPath = "Assets/Prefabs/Environment/D19/Integration/D19_StableInteractionAnchors.prefab";

        [Test]
        public void R02PresentationPrefabHasTerrainMeshColliderAndLodData()
        {
            var prefab = PrefabUtility.LoadPrefabContents(PresentationPrefabPath);
            try
            {
                Assert.That(prefab, Is.Not.Null);
                var terrain = prefab.transform.Find("D19_R02_TerrainSurface");
                Assert.That(terrain, Is.Not.Null);
                var filter = terrain.GetComponent<MeshFilter>();
                var meshCollider = terrain.GetComponent<MeshCollider>();
                Assert.That(filter, Is.Not.Null);
                Assert.That(filter.sharedMesh, Is.Not.Null);
                Assert.That(filter.sharedMesh.vertexCount, Is.GreaterThanOrEqualTo(1900));
                Assert.That(meshCollider, Is.Not.Null);
                Assert.That(meshCollider.sharedMesh, Is.SameAs(filter.sharedMesh));
                Assert.That(prefab.GetComponentsInChildren<LODGroup>(true).Length, Is.GreaterThanOrEqualTo(1));
                Assert.That(prefab.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThanOrEqualTo(70));
            }
            finally
            {
                if (prefab != null) PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        [Test]
        public void R02PresentationHasNoMissingOrInternalErrorMaterials()
        {
            var prefab = PrefabUtility.LoadPrefabContents(PresentationPrefabPath);
            try
            {
                foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                {
                    Assert.That(renderer.sharedMaterials, Is.Not.Null, renderer.name + " has no material array");
                    Assert.That(renderer.sharedMaterials.All(material => material != null), Is.True, renderer.name + " has a missing material");
                    Assert.That(renderer.sharedMaterials.All(material => material.shader != null), Is.True, renderer.name + " has a missing shader");
                    Assert.That(renderer.sharedMaterials.All(material => material.shader.name != "Hidden/InternalErrorShader"), Is.True, renderer.name + " uses the Unity error shader");
                }
            }
            finally
            {
                if (prefab != null) PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        [Test]
        public void InitialHarborReferencesR02PrefabAndStableAnchorPrefab()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            try
            {
                var presentation = GameObject.Find("D19_R02_HarborPresentation");
                Assert.That(presentation, Is.Not.Null);
                Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(presentation), Is.Not.Null);
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(AnchorPrefabPath), Is.Not.Null);

                foreach (var id in new[]
                {
                    "D19_Anchor_D2_PlayerAvatar", "D19_Anchor_building_market_santa_margarita",
                    "D19_Anchor_building_warehouse_santa_margarita", "D19_Anchor_building_tavern_santa_margarita",
                    "D19_Anchor_building_shipyard_santa_margarita", "D19_Anchor_building_saint_marys_dock",
                    "D19_Anchor_berth_santa_margarita", "D19_Anchor_GangplankGround", "D19_Anchor_DockGround",
                    "D19_Anchor_D7_HelmPoint", "D19_Anchor_departure_channel_santa_margarita",
                    "D19_Anchor_return_channel_santa_margarita", "D19_Anchor_save_point_santa_margarita",
                    "D19_Anchor_npc_santa_margarita_dockhand", "D19_Anchor_npc_santa_margarita_lookout",
                    "D19_Anchor_npc_santa_margarita_tavern_keeper", "D19_Anchor_santa_margarita"
                })
                {
                    Assert.That(GameObject.Find(id), Is.Not.Null, "missing stable anchor " + id);
                }
            }
            finally
            {
                if (scene.IsValid()) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }
    }
}
#endif
