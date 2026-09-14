#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Save.Tests
{
    /// <summary>
    /// Q23 的正式场景/Prefab 集成审计。
    ///
    /// 该测试只读打开 InitialHarbor，验证 R03 生成 Prefab 和船只表现是否以
    /// 唯一、活动、可重开的一份挂接到正式场景；同时检查活动表现的脚本/材质
    /// 完整性和船体碰撞水线。它不替代 native Player 的输入和人工画面门禁。
    /// </summary>
    public sealed class D19R03Q23FormalResourceIntegrationTests
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string HarborPrefabPath =
            "Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab";
        private const string ShipPrefabPath =
            "Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_ShipVisual.prefab";
        private const string D27PrefabPath =
            "Assets/Prefabs/Characters/D27/R03/D27_R03_CharacterWildlifePresentation.prefab";
        private const float ExpectedSeaLevelY = -0.04f;

        [Test]
        public void InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently()
        {
            var first = CaptureSceneSnapshot();
            AssertSnapshot(first, "first open");

            SceneManager.SetActiveScene(first.Scene);
            Assert.That(first.Scene.GetRootGameObjects().Length, Is.GreaterThan(0));
            Assert.That(EditorSceneManager.CloseScene(first.Scene, true), Is.True);

            var second = CaptureSceneSnapshot();
            try
            {
                AssertSnapshot(second, "reopen");
                Assert.That(second.ActiveRootCount, Is.EqualTo(first.ActiveRootCount));
                Assert.That(second.R03RendererCount, Is.EqualTo(first.R03RendererCount));
                Assert.That(second.R03MissingMaterialSlots, Is.EqualTo(first.R03MissingMaterialSlots));
                Assert.That(second.MissingScriptCount, Is.EqualTo(first.MissingScriptCount));
                Assert.That(second.HullBottomY, Is.EqualTo(first.HullBottomY).Within(0.0001f));

                Debug.Log(string.Format(
                    "[Q23] active_roots={0}, r03_instances={1}, ship_instances={2}, " +
                    "r03_renderers={3}, r03_missing_materials={4}, missing_scripts={5}, " +
                    "terrain_colliders={6}, ship_hull_bottom_y={7:F4}, " +
                    "harbor_guid={8}, ship_guid={9}, d27_guid={10}",
                    second.ActiveRootCount,
                    second.R03InstanceCount,
                    second.ShipInstanceCount,
                    second.R03RendererCount,
                    second.R03MissingMaterialSlots,
                    second.MissingScriptCount,
                    second.TerrainColliderCount,
                    second.HullBottomY,
                    second.HarborGuid,
                    second.ShipGuid,
                    second.D27Guid));
            }
            finally
            {
                if (second.Scene.IsValid()) EditorSceneManager.CloseScene(second.Scene, true);
            }
        }

        private static SceneSnapshot CaptureSceneSnapshot()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            Assert.That(scene.IsValid(), Is.True, "InitialHarbor must open as an additive Editor scene.");

            var roots = scene.GetRootGameObjects();
            var r03Roots = roots.Where(root => root.name == "D19_R03_HarborPresentation").ToArray();
            var shipInstances = FindObjects(scene, "D19_R03_ShipVisualInstance");
            var stableShipParents = FindObjects(scene, "ph_ship_fishing");
            var r03 = r03Roots.SingleOrDefault();
            var ship = shipInstances.SingleOrDefault();

            int activeRootCount = roots.Count(root => root.activeSelf);
            int r03RendererCount = r03 == null
                ? 0
                : r03.GetComponentsInChildren<Renderer>(true).Count(renderer => renderer.enabled && renderer.gameObject.activeInHierarchy);
            int r03MissingMaterials = r03 == null ? 0 : CountMissingMaterials(r03, true);
            int missingScripts = roots.Sum(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount);
            int terrainColliders = r03 == null
                ? 0
                : r03.GetComponentsInChildren<MeshCollider>(true)
                    .Count(collider => collider.gameObject.name == "D19_TerrainRoot" && collider.enabled && !collider.isTrigger);
            float hullBottomY = ReadHullBottomY(ship);

            return new SceneSnapshot(
                scene,
                roots,
                r03Roots.Length,
                shipInstances.Length,
                activeRootCount,
                r03RendererCount,
                r03MissingMaterials,
                missingScripts,
                terrainColliders,
                hullBottomY,
                FindPrefabGuid(r03, HarborPrefabPath),
                FindPrefabGuid(ship, ShipPrefabPath),
                AssetDatabase.AssetPathToGUID(D27PrefabPath),
                stableShipParents.Length,
                r03,
                ship);
        }

        private static void AssertSnapshot(SceneSnapshot snapshot, string label)
        {
            Assert.That(snapshot.R03InstanceCount, Is.EqualTo(1), label + ": exactly one R03 harbor instance is required.");
            Assert.That(snapshot.R03.activeInHierarchy, Is.True, label + ": R03 harbor instance must be active.");
            Assert.That(snapshot.R03.transform.localPosition, Is.EqualTo(Vector3.zero), label + ": R03 root local position.");
            Assert.That(snapshot.R03.transform.localScale, Is.EqualTo(Vector3.one), label + ": R03 root local scale.");
            Assert.That(snapshot.ShipInstanceCount, Is.EqualTo(1), label + ": exactly one R03 ship visual instance is required.");
            Assert.That(snapshot.Ship.activeInHierarchy, Is.True, label + ": R03 ship visual must be active.");
            Assert.That(snapshot.StableShipParentCount, Is.EqualTo(1), label + ": stable ph_ship_fishing owner must be unique.");
            Assert.That(snapshot.Ship.transform.IsChildOf(snapshot.StableShipParent), Is.True,
                label + ": ship visual must remain under stable ph_ship_fishing owner.");
            Assert.That(snapshot.HarborGuid, Is.Not.Empty, label + ": R03 harbor must remain a Prefab instance.");
            Assert.That(snapshot.ShipGuid, Is.Not.Empty, label + ": R03 ship must remain a Prefab instance.");
            Assert.That(snapshot.D27Guid, Is.EqualTo(AssetDatabase.AssetPathToGUID(D27PrefabPath)),
                label + ": D27 source GUID must resolve.");
            Assert.That(snapshot.R03MissingMaterialSlots, Is.EqualTo(0), label + ": active R03 renderers have missing material slots.");
            Assert.That(snapshot.MissingScriptCount, Is.EqualTo(0), label + ": InitialHarbor has missing MonoBehaviour scripts.");
            Assert.That(snapshot.TerrainColliderCount, Is.EqualTo(1), label + ": R03 must own one active terrain MeshCollider.");
            Assert.That(snapshot.HullBottomY, Is.EqualTo(ExpectedSeaLevelY).Within(0.01f),
                label + ": active ship hull collision must meet the R03 waterline.");
        }

        private static float ReadHullBottomY(GameObject ship)
        {
            if (ship == null) return float.NaN;
            var hull = FindByName(ship.transform, "D6_ShipHullCollision");
            if (hull == null) return float.NaN;
            var collider = hull.GetComponent<BoxCollider>();
            return collider == null || !collider.enabled ? float.NaN : collider.bounds.min.y;
        }

        private static string FindPrefabGuid(GameObject instance, string expectedPath)
        {
            if (instance == null) return string.Empty;
            var source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            string path = source == null ? string.Empty : AssetDatabase.GetAssetPath(source);
            Assert.That(path, Is.EqualTo(expectedPath), "Prefab source path mismatch for " + instance.name);
            return AssetDatabase.AssetPathToGUID(path);
        }

        private static int CountMissingMaterials(GameObject root, bool activeOnly)
        {
            int missing = 0;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (activeOnly && (!renderer.enabled || !renderer.gameObject.activeInHierarchy)) continue;
                foreach (var material in renderer.sharedMaterials)
                    if (material == null) missing++;
            }
            return missing;
        }

        private static GameObject[] FindObjects(Scene scene, string name)
        {
            var found = new List<GameObject>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    if (transform.name == name) found.Add(transform.gameObject);
            }
            return found.ToArray();
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

        private sealed class SceneSnapshot
        {
            public readonly Scene Scene;
            public readonly GameObject[] Roots;
            public readonly int R03InstanceCount;
            public readonly int ShipInstanceCount;
            public readonly int ActiveRootCount;
            public readonly int R03RendererCount;
            public readonly int R03MissingMaterialSlots;
            public readonly int MissingScriptCount;
            public readonly int TerrainColliderCount;
            public readonly float HullBottomY;
            public readonly string HarborGuid;
            public readonly string ShipGuid;
            public readonly string D27Guid;
            public readonly int StableShipParentCount;
            public readonly GameObject R03;
            public readonly GameObject Ship;
            public readonly Transform StableShipParent;

            public SceneSnapshot(
                Scene scene,
                GameObject[] roots,
                int r03InstanceCount,
                int shipInstanceCount,
                int activeRootCount,
                int r03RendererCount,
                int r03MissingMaterialSlots,
                int missingScriptCount,
                int terrainColliderCount,
                float hullBottomY,
                string harborGuid,
                string shipGuid,
                string d27Guid,
                int stableShipParentCount,
                GameObject r03,
                GameObject ship)
            {
                Scene = scene;
                Roots = roots;
                R03InstanceCount = r03InstanceCount;
                ShipInstanceCount = shipInstanceCount;
                ActiveRootCount = activeRootCount;
                R03RendererCount = r03RendererCount;
                R03MissingMaterialSlots = r03MissingMaterialSlots;
                MissingScriptCount = missingScriptCount;
                TerrainColliderCount = terrainColliderCount;
                HullBottomY = hullBottomY;
                HarborGuid = harborGuid;
                ShipGuid = shipGuid;
                D27Guid = d27Guid;
                StableShipParentCount = stableShipParentCount;
                R03 = r03;
                Ship = ship;
                StableShipParent = stableShipParentCount == 1
                    ? FindObjects(scene, "ph_ship_fishing")[0].transform
                    : null;
            }
        }
    }
}
#endif
