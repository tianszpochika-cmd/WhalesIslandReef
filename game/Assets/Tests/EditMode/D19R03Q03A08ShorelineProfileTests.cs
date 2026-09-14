using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Save.Tests
{
    /// <summary>
    /// Q03-A08 / D31-P04 的定向海岸剖面回归。
    ///
    /// 该测试锁定真实 R03 Prefab 生成结果：海岸边界必须从地表向水下
    /// 过渡，而不是继续暴露旧的整段垂直切面。它只验证表现层与唯一
    /// D19_TerrainRoot 的 owner 契约，不能替代 standalone Player 视觉门禁。
    /// </summary>
    public sealed class D19R03Q03A08ShorelineProfileTests
    {
        private const string PrefabPath =
            "Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab";
        private const int SurfaceGridVertexCount = 25 * 25 * 4;
        private const int SkirtSegments = 64;
        private const int ProfileVertexCount = 6;
        private const float WaterlineY = -0.04f;
        private const float CoastalShelfY = -0.24f;
        private const float SkirtBottomY = -0.42f;

        private GameObject _instance;

        [TearDown]
        public void TearDown()
        {
            if (_instance != null) Object.DestroyImmediate(_instance);
            _instance = null;
        }

        [Test]
        public void R03IslandEdge_UsesBeveledSubmergedShelf_AndKeepsSingleTerrainOwner()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, "Q03-A08 R03 presentation Prefab must be importable.");

            _instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Assert.IsNotNull(_instance, "Q03-A08 R03 presentation Prefab could not be instantiated.");

            var island = FindByName(_instance.transform, "D19_R03_IslandSurface");
            Assert.IsNotNull(island, "R03 island surface is missing.");
            var filter = island.GetComponent<MeshFilter>();
            var renderer = island.GetComponent<MeshRenderer>();
            Assert.IsNotNull(filter, "R03 island surface MeshFilter is missing.");
            Assert.IsNotNull(filter.sharedMesh, "R03 island surface mesh is missing.");
            Assert.IsNotNull(renderer, "R03 island surface renderer is missing.");

            var mesh = filter.sharedMesh;
            Assert.AreEqual(SurfaceGridVertexCount + SkirtSegments * ProfileVertexCount,
                mesh.vertexCount,
                "R03 island mesh must contain six profile vertices per coastal segment.");
            Assert.GreaterOrEqual(renderer.sharedMaterials.Length, 4,
                "R03 island surface must retain the sand material slot.");
            Assert.IsNotNull(renderer.sharedMaterials[3], "R03 sand material slot is missing.");
            StringAssert.Contains("R03_Sand", renderer.sharedMaterials[3].name,
                "The visible coastal bevel must use the sand owner, not a striped stone wall.");

            var vertices = mesh.vertices;
            float minimumShelfExpansion = float.MaxValue;
            float maximumInnerY = float.MinValue;
            float minimumInnerY = float.MaxValue;
            for (int segment = 0; segment < SkirtSegments; segment++)
            {
                int start = SurfaceGridVertexCount + segment * ProfileVertexCount;
                var inner0 = new Vector2(vertices[start].x, vertices[start].z);
                var inner1 = new Vector2(vertices[start + 1].x, vertices[start + 1].z);
                var shelf1 = new Vector2(vertices[start + 2].x, vertices[start + 2].z);
                var shelf0 = new Vector2(vertices[start + 3].x, vertices[start + 3].z);

                Assert.Greater(Vector2.Distance(inner0, shelf0), 1.20f,
                    "Each coastal segment needs a visible outward shelf width.");
                Assert.Greater(Vector2.Distance(inner1, shelf1), 1.20f,
                    "Each coastal segment needs a visible outward shelf width.");
                minimumShelfExpansion = Mathf.Min(minimumShelfExpansion,
                    Vector2.Distance(inner0, shelf0), Vector2.Distance(inner1, shelf1));

                maximumInnerY = Mathf.Max(maximumInnerY, vertices[start].y, vertices[start + 1].y);
                minimumInnerY = Mathf.Min(minimumInnerY, vertices[start].y, vertices[start + 1].y);
                Assert.That(vertices[start + 2].y, Is.EqualTo(CoastalShelfY).Within(0.0001f));
                Assert.That(vertices[start + 3].y, Is.EqualTo(CoastalShelfY).Within(0.0001f));
                Assert.That(vertices[start + 4].y, Is.EqualTo(SkirtBottomY).Within(0.0001f));
                Assert.That(vertices[start + 5].y, Is.EqualTo(SkirtBottomY).Within(0.0001f));
                Assert.Greater(vertices[start].y, vertices[start + 2].y,
                    "The inner-to-water edge must slope downward instead of staying vertical.");
                Assert.Greater(vertices[start + 1].y, vertices[start + 3].y,
                    "The inner-to-water edge must slope downward instead of staying vertical.");
            }

            Assert.Greater(maximumInnerY, WaterlineY,
                "The R03 island surface must remain above the waterline at the boundary.");
            Assert.Less(minimumInnerY, 0.60f,
                "The coastal profile audit must cover the low shoreline band, not only the ridge.");

            var terrain = FindByName(_instance.transform, "D19_TerrainRoot");
            Assert.IsNotNull(terrain, "R03 terrain collision owner is missing.");
            var terrainCollider = terrain.GetComponent<MeshCollider>();
            Assert.IsNotNull(terrainCollider, "R03 terrain owner must remain a MeshCollider.");
            Assert.IsFalse(terrainCollider.isTrigger);
            Assert.AreSame(mesh, terrainCollider.sharedMesh,
                "The beveled R03 surface and its single terrain owner must stay aligned.");
            var shoreline = FindByName(_instance.transform, "D19_R03_ShorelineAndWater");
            Assert.IsNotNull(shoreline, "R03 shoreline owner is missing.");
            Assert.AreEqual(0, shoreline.GetComponentsInChildren<Collider>(true).Length,
                "Q03-A08 must not introduce a second shoreline collision owner.");

            Debug.Log(string.Format(
                "[Q03-A08] coastal_profile=beveled, segments={0}, vertices={1}, " +
                "min_shelf_width={2:F3}, inner_y=({3:F3},{4:F3}), shelf_y={5:F3}, " +
                "bottom_y={6:F3}, terrain_owner={7}",
                SkirtSegments,
                mesh.vertexCount,
                minimumShelfExpansion,
                minimumInnerY,
                maximumInnerY,
                CoastalShelfY,
                SkirtBottomY,
                terrainCollider.name));
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
    }
}
