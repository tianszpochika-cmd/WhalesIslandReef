#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WhalesIslandReef.Tests.D19R01B
{
    public sealed class D19R01BVisualRepairTests
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string RepairPrefabPath = "Assets/Prefabs/Environment/D19/Integration/R01/D19_R01_B_HarborVisualRepair.prefab";

        [Test]
        public void RepairPrefabContainsProjectOwnedDetailAndApprovedNestedReferences()
        {
            var prefab = PrefabUtility.LoadPrefabContents(RepairPrefabPath);
            try
            {
                Assert.That(prefab, Is.Not.Null);
                Assert.That(prefab.transform.Find("D19_R01_B_ProjectOwned_PortAndCoastDetails"), Is.Not.Null);
                Assert.That(prefab.GetComponentsInChildren<Transform>(true).Length, Is.GreaterThanOrEqualTo(250));
                Assert.That(prefab.GetComponentsInChildren<MeshRenderer>(true).Length, Is.GreaterThanOrEqualTo(180));
                Assert.That(prefab.GetComponentsInChildren<Transform>(true)
                    .Count(t => PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject) != null), Is.GreaterThanOrEqualTo(40));
            }
            finally
            {
                if (prefab != null) PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        [Test]
        public void RepairPrefabHasNoMissingOrErrorMaterials()
        {
            var prefab = PrefabUtility.LoadPrefabContents(RepairPrefabPath);
            try
            {
                foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                {
                    Assert.That(renderer.sharedMaterial, Is.Not.Null, renderer.name + " has no material");
                    Assert.That(renderer.sharedMaterial.shader, Is.Not.Null, renderer.name + " has no shader");
                    Assert.That(renderer.sharedMaterial.shader.name, Is.Not.EqualTo("Hidden/InternalErrorShader"), renderer.name + " uses error shader");
                }
            }
            finally
            {
                if (prefab != null) PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        [Test]
        public void FormalSceneKeepsRepairPrefabAndStableInteractionAnchors()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            try
            {
                var repair = GameObject.Find("D19_R01_B_HarborVisualRepair");
                Assert.That(repair, Is.Not.Null);
                Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(repair), Is.Not.Null);
                foreach (var id in new[]
                {
                    "D19_Anchor_D2_PlayerAvatar", "D19_Anchor_building_market_santa_margarita",
                    "D19_Anchor_building_warehouse_santa_margarita", "D19_Anchor_building_tavern_santa_margarita",
                    "D19_Anchor_building_shipyard_santa_margarita", "D19_Anchor_berth_santa_margarita",
                    "D19_Anchor_GangplankGround", "D19_Anchor_D7_HelmPoint",
                    "D19_Anchor_departure_channel_santa_margarita", "D19_Anchor_return_channel_santa_margarita",
                    "D19_Anchor_save_point_santa_margarita"
                })
                    Assert.That(GameObject.Find(id), Is.Not.Null, "missing stable anchor " + id);
            }
            finally
            {
                if (scene.IsValid()) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }
    }
}
#endif
