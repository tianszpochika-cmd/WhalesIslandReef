using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace WhalesIslandReef.Tests.D19A01
{
    public sealed class D19A01TerrainTests
    {
        private const string ScenePath = "Assets/Scenes/D19/Work/D19_Terrain_Work.unity";
        private const string TerrainPrefabPath = "Assets/Prefabs/Environment/D19/Terrain/D19_TerrainRoot.prefab";
        private const string CollisionPrefabPath = "Assets/Prefabs/Environment/D19/Terrain/D19_TerrainCollision.prefab";
        private const string AnchorPrefabPath = "Assets/Prefabs/Environment/D19/Terrain/D19_TerrainAnchors.prefab";
        private const string NavMeshPath = "Assets/Scenes/D19/Work/Data/D19_SaintMargaret_NavMesh.asset";

        [Test]
        public void TerrainPrefabHasFrozenResolutionAndLayers()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TerrainPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            var terrain = prefab.GetComponent<Terrain>();
            Assert.That(terrain, Is.Not.Null);
            Assert.That(terrain.terrainData, Is.Not.Null);
            Assert.That(terrain.terrainData.heightmapResolution, Is.EqualTo(257));
            Assert.That(terrain.terrainData.size.x, Is.EqualTo(256f).Within(0.01f));
            Assert.That(terrain.terrainData.size.z, Is.EqualTo(272f).Within(0.01f));
            Assert.That(terrain.terrainData.terrainLayers.Length, Is.EqualTo(4));
        }

        [Test]
        public void TerrainPrefabHasTexturedVisualMeshWithoutGameplayCollider()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TerrainPrefabPath);
            var visual = prefab.transform.Find("D19_TerrainVisualMesh");
            Assert.That(visual, Is.Not.Null);
            var filter = visual.GetComponent<MeshFilter>();
            var renderer = visual.GetComponent<MeshRenderer>();
            Assert.That(filter, Is.Not.Null);
            Assert.That(filter.sharedMesh, Is.Not.Null);
            Assert.That(filter.sharedMesh.uv.Length, Is.EqualTo(filter.sharedMesh.vertexCount));
            Assert.That(renderer, Is.Not.Null);
            Assert.That(renderer.sharedMaterials.Length, Is.EqualTo(4));
            Assert.That(visual.GetComponent<Collider>(), Is.Null);
        }

        [Test]
        public void WorkSceneContainsOnlyPrefabRoots()
        {
            Assert.That(File.Exists(ProjectPath("game/Assets/Scenes/D19/Work/D19_Terrain_Work.unity")), Is.True);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Assert.That(scene.rootCount, Is.GreaterThanOrEqualTo(7));
            foreach (var root in scene.GetRootGameObjects())
                Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(root), Is.Not.Null, root.name + " is not a prefab instance");
        }

        [Test]
        public void CollisionPrefabHasRepresentativeBoundaries()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CollisionPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            var colliders = prefab.GetComponentsInChildren<Collider>(true);
            Assert.That(colliders.Length, Is.GreaterThanOrEqualTo(7));
            Assert.That(prefab.transform.Find("D19_ReefCollision"), Is.Not.Null);
            Assert.That(prefab.transform.Find("D19_CliffExclusion"), Is.Not.Null);
            Assert.That(prefab.transform.Find("D19_DeepSeaBoundary"), Is.Not.Null);
        }

        [Test]
        public void AnchorPrefabContainsA01SharedAnchors()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AnchorPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            var names = new[] { "anchor_player_recovery", "anchor_beach_landing", "anchor_shallow_waterline", "anchor_cliff_exclusion", "anchor_channel_safe" };
            foreach (var name in names) Assert.That(prefab.transform.Find(name), Is.Not.Null, name);
        }

        [Test]
        public void NavMeshDataAssetExistsAndIsNotLocalReadyPromotion()
        {
            var data = AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshPath);
            Assert.That(data, Is.Not.Null);
            Assert.That(AssetDatabase.AssetPathToGUID(NavMeshPath), Is.Not.Empty);
        }

        [Test]
        public void IntakeEnvironmentModelsRemainSourceReferences()
        {
            var paths = new[]
            {
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_corner_rock.fbx",
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeF.fbx",
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallF.fbx",
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathBend.fbx",
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_wood.fbx",
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_stepsCorner_stone.fbx"
            };
            foreach (var path in paths)
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(path), Is.Not.Null, path);
        }

        private static string ProjectPath(string relative)
        {
            const string projectPrefix = "game/Assets/";
            var assetRelative = relative.StartsWith(projectPrefix, System.StringComparison.Ordinal)
                ? relative.Substring(projectPrefix.Length)
                : relative;
            return Path.Combine(Application.dataPath, assetRelative.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
