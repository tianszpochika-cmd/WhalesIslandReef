using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WhalesIslandReef.D19.Vegetation;

namespace WhalesIslandReef.Tests.EditMode
{
    public sealed class D19VegetationBiomeTests
    {
        private const string Root = "Assets/Prefabs/Vegetation/D19/";

        [Test]
        public void D19A03_LayoutHasFixedSeedSixBiomesAndExclusions()
        {
            string path = ProjectFile("Assets/Prefabs/Vegetation/D19/Data/D19_Vegetation_Layout.json");
            Assert.That(File.Exists(path), Is.True, "D19-A03 layout data is missing.");
            D19VegetationLayoutData data = JsonUtility.FromJson<D19VegetationLayoutData>(File.ReadAllText(path));
            Assert.That(data, Is.Not.Null);
            Assert.That(data.global_seed, Is.EqualTo(1903));
            Assert.That(data.biomes, Has.Length.EqualTo(6));
            Assert.That(data.exclusions, Has.Length.GreaterThanOrEqualTo(10));
            Assert.That(data.content_anchors, Has.Length.GreaterThanOrEqualTo(6));
            for (int i = 0; i < data.biomes.Length; i++)
            {
                Assert.That(data.biomes[i].seed, Is.GreaterThan(0));
                Assert.That(data.biomes[i].placements, Is.Not.Empty);
                Assert.That(data.biomes[i].rule_summary, Is.Not.Empty);
            }
        }

        [Test]
        public void D19A03_AllBiomePrefabsHaveLodWindMaterialAndStableAnchors()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { Root });
            int biomeCount = 0;
            int placementCount = 0;
            int anchorCount = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.Contains("D19_Biome_")) continue;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null || prefab.name == "D19_BiomeRoot") continue;
                biomeCount++;
                LODGroup[] lodGroups = prefab.GetComponentsInChildren<LODGroup>(true);
                Assert.That(lodGroups.Length, Is.GreaterThan(0), path);
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                Assert.That(renderers.Length, Is.GreaterThan(0), path);
                for (int r = 0; r < renderers.Length; r++)
                {
                    Assert.That(renderers[r].sharedMaterial, Is.Not.Null, path);
                    Assert.That(renderers[r].sharedMaterial.shader, Is.Not.Null, path);
                }
                Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
                int localPlacements = 0;
                for (int t = 0; t < transforms.Length; t++)
                {
                    if (transforms[t].name.StartsWith("Vegetation_")) localPlacements++;
                    if (transforms[t].name.StartsWith("D19_CollectAnchor_")) anchorCount++;
                }
                Assert.That(localPlacements, Is.GreaterThan(0), path);
                placementCount += localPlacements;
            }
            Assert.That(biomeCount, Is.EqualTo(6));
            Assert.That(placementCount, Is.GreaterThanOrEqualTo(40));
            Assert.That(anchorCount, Is.GreaterThanOrEqualTo(6));
            string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { Root + "Materials" });
            bool hasWindMaterial = false;
            bool hasDistantMaterial = false;
            for (int i = 0; i < materialGuids.Length; i++)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialGuids[i]));
                if (material == null || material.shader == null) continue;
                if (material.shader.name == "WhalesIslandReef/D19/VegetationWind") hasWindMaterial = true;
                if (material.shader.name == "Universal Render Pipeline/Unlit" || material.shader.name == "Unlit/Color")
                    hasDistantMaterial = true;
            }
            Assert.That(hasWindMaterial, Is.True);
            Assert.That(hasDistantMaterial, Is.True);
        }

        [Test]
        public void D19A03_LodDistancesAndPlacementExclusionsAreDeterministic()
        {
            string layoutPath = ProjectFile("Assets/Prefabs/Vegetation/D19/Data/D19_Vegetation_Layout.json");
            D19VegetationLayoutData data = JsonUtility.FromJson<D19VegetationLayoutData>(File.ReadAllText(layoutPath));
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { Root });
            int lodCount = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.Contains("D19_Biome_")) continue;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null || prefab.name == "D19_BiomeRoot") continue;
                LODGroup[] groups = prefab.GetComponentsInChildren<LODGroup>(true);
                for (int g = 0; g < groups.Length; g++)
                {
                    LOD[] lods = groups[g].GetLODs();
                    Assert.That(lods, Has.Length.EqualTo(3), path);
                    Assert.That(lods[0].screenRelativeTransitionHeight, Is.EqualTo(0.60f).Within(0.001f), path);
                    Assert.That(lods[1].screenRelativeTransitionHeight, Is.EqualTo(0.24f).Within(0.001f), path);
                    Assert.That(lods[2].screenRelativeTransitionHeight, Is.EqualTo(0.06f).Within(0.001f), path);
                    lodCount++;
                }
            }
            Assert.That(lodCount, Is.EqualTo(52));

            Vector3[] biomeCenters =
            {
                new Vector3(-58f, 0f, -8f), new Vector3(-18f, 0f, 66f), new Vector3(-48f, 0f, 65f),
                new Vector3(-94f, 0f, 70f), new Vector3(72f, 6f, 72f), new Vector3(-72f, 0.5f, 28f),
            };
            for (int b = 0; b < data.biomes.Length; b++)
            {
                for (int p = 0; p < data.biomes[b].placements.Length; p++)
                {
                    Vector3 world = biomeCenters[b] + data.biomes[b].placements[p].local_position;
                    for (int e = 0; e < data.exclusions.Length; e++)
                    {
                        D19VegetationExclusionData exclusion = data.exclusions[e];
                        bool inside = Mathf.Abs(world.x - exclusion.center.x) <= exclusion.half_extents.x &&
                                      Mathf.Abs(world.y - exclusion.center.y) <= exclusion.half_extents.y &&
                                      Mathf.Abs(world.z - exclusion.center.z) <= exclusion.half_extents.z;
                        Assert.That(inside, Is.False,
                            data.biomes[b].biome_id + "/" + data.biomes[b].placements[p].model_file +
                            " intersects " + exclusion.exclusion_id);
                    }
                }
            }
        }

        [Test]
        public void D19A03_ResultKeepsCandidateStateAndReportsZeroSeriousIntersections()
        {
            string path = ProjectFile("tools/results/D19/D19-A03/D19_A03_MACHINE_RESULT.json");
            Assert.That(File.Exists(path), Is.True);
            string result = File.ReadAllText(path);
            Assert.That(result, Does.Contain("\"serious_intersections\": 0"));
            Assert.That(result, Does.Contain("\"road_dock_building_spawn_channel_intersections\": 0"));
            Assert.That(result, Does.Contain("LocalDevelopmentReady=false"));
            Assert.That(result, Does.Contain("ReleaseApproved=false"));
        }

        [Test]
        public void D19A03_WorkSceneAndBiomeRootExistWithNamedExclusions()
        {
            Assert.That(File.Exists(ProjectFile("Assets/Scenes/D19/Work/D19_Biomes_Work.unity")), Is.True);
            GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "D19_BiomeRoot.prefab");
            Assert.That(root, Is.Not.Null);
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            int exclusions = 0;
            int exclusionColliders = 0;
            for (int i = 0; i < transforms.Length; i++)
            {
                if (!transforms[i].name.StartsWith("Exclusion_")) continue;
                exclusions++;
                if (transforms[i].GetComponent<BoxCollider>() != null) exclusionColliders++;
            }
            Assert.That(exclusions, Is.GreaterThanOrEqualTo(10));
            Assert.That(exclusionColliders, Is.EqualTo(exclusions));
            Assert.That(File.Exists(ProjectFile("Assets/Scenes/InitialHarbor.unity")), Is.True);
        }

        private static string ProjectFile(string projectRelativePath)
        {
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                projectRelativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
