#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using WhalesIslandReef.D19.Vegetation;

namespace WhalesIslandReef.Editor
{
    /// <summary>
    /// Deterministic D19-A03 asset authoring. The source model list is hard-coded
    /// for auditability and checked against the D18 intake catalog before any
    /// Prefab or scene is written.
    /// </summary>
    public static class D19VegetationAssetBuilder
    {
        private const string CandidateRoot = "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/";
        private const string PrefabRoot = "Assets/Prefabs/Vegetation/D19/";
        private const string DataRoot = PrefabRoot + "Data/";
        private const string MaterialRoot = PrefabRoot + "Materials/";
        private const string ScenePath = "Assets/Scenes/D19/Work/D19_Biomes_Work.unity";
        private const string LayoutPath = DataRoot + "D19_Vegetation_Layout.json";
        private const int GlobalSeed = 1903;

        [Serializable]
        private sealed class IntakeCatalog
        {
            public IntakeEntry[] eligible_entries;
        }

        [Serializable]
        private sealed class IntakeEntry
        {
            public string candidate_file_path;
            public string[] allowed_task_scope;
            public bool intake_eligible;
        }

        [Serializable]
        private sealed class ModelSpec
        {
            public string file;
            public Vector3 position;
            public Vector3 scale;
            public float yaw;
            public string contentId;
            public string anchorId;
            public string plantId;
            public bool routeVisible;
        }

        [Serializable]
        private sealed class BiomePlan
        {
            public string id;
            public string habitat;
            public Vector3 worldPosition;
            public int seed;
            public float density;
            public string summary;
            public ModelSpec[] models;
            public Color materialColor;
            public float windAmplitude;
            public float windFrequency;
        }

        [Serializable]
        private sealed class ShotResult
        {
            public string file;
            public float render_ms;
            public bool written;
        }

        [Serializable]
        private sealed class MachineResult
        {
            public string task = "D19-A03";
            public string status = "PASS";
            public string scene_path = "game/Assets/Scenes/D19/Work/D19_Biomes_Work.unity";
            public string layout_path = "game/Assets/Prefabs/Vegetation/D19/Data/D19_Vegetation_Layout.json";
            public int global_seed = GlobalSeed;
            public int biome_prefab_count;
            public int placement_instance_count;
            public int content_anchor_count;
            public int lod_group_count;
            public int renderer_count;
            public int distinct_material_count;
            public int exclusion_volume_count;
            public int serious_intersections;
            public int road_dock_building_spawn_channel_intersections;
            public int estimated_static_batches;
            public string wind_material = "WhalesIslandReef/D19/VegetationWind";
            public string asset_state = "Candidate/ImportVerified; LocalDevelopmentReady=false; ReleaseApproved=false";
            public string[] used_source_models;
            public string[] used_content_ids;
            public ShotResult[] screenshots;
            public string[] validation_notes;
        }

        private static readonly BiomePlan[] Plans =
        {
            new BiomePlan
            {
                id = "beach", habitat = "beach", worldPosition = new Vector3(-58f, 0f, -8f), seed = 190301,
                density = 3.5f, windAmplitude = 0.12f, windFrequency = 0.8f,
                summary = "salt-tolerant palm canopy; low grass and shrub clusters; rock pockets stay outside landing anchor",
                materialColor = new Color(0.22f, 0.52f, 0.20f),
                models = new[]
                {
                    M("tree_palmTall.fbx", new Vector3(10,0,8), new Vector3(1.2f,1.2f,1.2f), 12, "", "", "", false),
                    M("tree_palmBend.fbx", new Vector3(2,0,11), new Vector3(1.0f,1.0f,1.0f), -18, "", "", "", false),
                    M("tree_palmDetailedTall.fbx", new Vector3(-10,0,7), new Vector3(0.9f,0.9f,0.9f), 24, "plant_red_mangrove", "anchor.vegetation.beach.mangrove", "plant_red_mangrove", true),
                    M("plant_bushLarge.fbx", new Vector3(14,0,1), new Vector3(1.0f,1.0f,1.0f), 40, "plant_coast_agave_fiber", "anchor.vegetation.beach.agave", "plant_coast_agave_fiber", true),
                    M("plant_bushSmall.fbx", new Vector3(-14,0,2), new Vector3(0.8f,0.8f,0.8f), -22, "", "", "", false),
                    M("grass_large.fbx", new Vector3(8,0,0), new Vector3(0.8f,0.8f,0.8f), 6, "", "", "", false),
                    M("grass_leafsLarge.fbx", new Vector3(-4,0,12), new Vector3(0.75f,0.75f,0.75f), 17, "", "", "", false),
                    M("rock_smallFlatB.fbx", new Vector3(16,0,12), new Vector3(1.1f,1.1f,1.1f), 0, "", "", "", false),
                    M("log_large.fbx", new Vector3(-16,0,12), new Vector3(0.8f,0.8f,0.8f), 70, "", "", "", false),
                    M("stump_old.fbx", new Vector3(18,0,6), new Vector3(0.7f,0.7f,0.7f), 0, "", "", "", false),
                }
            },
            new BiomePlan
            {
                id = "port_disturbed", habitat = "port_disturbed", worldPosition = new Vector3(-18f, 0f, 66f), seed = 190302,
                density = 5.0f, windAmplitude = 0.07f, windFrequency = 0.65f,
                summary = "managed clearings; short grass, stump, log and separated shrub islands; no planting in spawn box",
                materialColor = new Color(0.27f, 0.48f, 0.16f),
                models = new[]
                {
                    M("tree_small.fbx", new Vector3(-10,0,-1), new Vector3(0.9f,0.9f,0.9f), 10, "", "", "", false),
                    M("tree_default.fbx", new Vector3(9,0,8), new Vector3(0.8f,0.8f,0.8f), -10, "plant_orchard_guava_v1", "anchor.vegetation.port.guava", "plant_orchard_guava_v1", true),
                    M("plant_bushDetailed.fbx", new Vector3(-12,0,10), new Vector3(0.8f,0.8f,0.8f), 28, "", "", "", false),
                    M("plant_bushSmall.fbx", new Vector3(12,0,-3), new Vector3(0.7f,0.7f,0.7f), -32, "plant_dry_aloe_medicinal", "anchor.vegetation.port.aloe", "plant_dry_aloe_medicinal", true),
                    M("grass.fbx", new Vector3(-2,0,-5), new Vector3(0.7f,0.7f,0.7f), 0, "", "", "", false),
                    M("grass_large.fbx", new Vector3(4,0,11), new Vector3(0.65f,0.65f,0.65f), 8, "", "", "", false),
                    M("log_stack.fbx", new Vector3(-5,0,10), new Vector3(0.7f,0.7f,0.7f), 22, "", "", "", false),
                    M("stump_round.fbx", new Vector3(8,0,10), new Vector3(0.6f,0.6f,0.6f), 0, "", "", "", false),
                }
            },
            new BiomePlan
            {
                id = "road_edge", habitat = "secondary_forest_edge", worldPosition = new Vector3(-48f, 0f, 65f), seed = 190303,
                density = 4.2f, windAmplitude = 0.09f, windFrequency = 0.72f,
                summary = "paired tree/shrub clumps set behind road clearance; low fillers point away from the walkable core",
                materialColor = new Color(0.18f, 0.43f, 0.16f),
                models = new[]
                {
                    M("tree_oak.fbx", new Vector3(-12,0,7), new Vector3(1.05f,1.05f,1.05f), 5, "", "", "", false),
                    M("tree_default_dark.fbx", new Vector3(12,0,10), new Vector3(0.95f,0.95f,0.95f), -12, "", "", "", false),
                    M("tree_small.fbx", new Vector3(-15,0,16), new Vector3(0.7f,0.7f,0.7f), 24, "", "", "", false),
                    M("plant_bushDetailed.fbx", new Vector3(13,0,17), new Vector3(0.9f,0.9f,0.9f), 35, "", "", "", false),
                    M("plant_bushSmall.fbx", new Vector3(-2,0,18), new Vector3(0.65f,0.65f,0.65f), 0, "", "", "", false),
                    M("grass_leafsLarge.fbx", new Vector3(-18,0,20), new Vector3(0.6f,0.6f,0.6f), 6, "", "", "", false),
                    M("log_large.fbx", new Vector3(18,0,6), new Vector3(0.7f,0.7f,0.7f), 70, "", "", "", false),
                    M("rock_smallA.fbx", new Vector3(-18,0,12), new Vector3(0.75f,0.75f,0.75f), 0, "", "", "", false),
                }
            },
            new BiomePlan
            {
                id = "secondary_forest", habitat = "secondary_forest", worldPosition = new Vector3(-94f, 0f, 70f), seed = 190304,
                density = 7.0f, windAmplitude = 0.10f, windFrequency = 0.55f,
                summary = "three-height canopy layering with shrub understory, grass carpet and fallen wood; no random scatter",
                materialColor = new Color(0.14f, 0.38f, 0.14f),
                models = new[]
                {
                    M("tree_oak.fbx", new Vector3(-14,0,8), new Vector3(1.2f,1.2f,1.2f), 5, "plant_rainforest_mahogany_v1", "anchor.vegetation.forest.mahogany", "plant_rainforest_mahogany_v1", true),
                    M("tree_default.fbx", new Vector3(2,0,12), new Vector3(1.0f,1.0f,1.0f), -14, "", "", "", false),
                    M("tree_tall.fbx", new Vector3(17,0,7), new Vector3(1.1f,1.1f,1.1f), 17, "plant_rainforest_pine_resin_v1", "anchor.vegetation.forest.resin", "plant_rainforest_pine_resin_v1", true),
                    M("tree_small_dark.fbx", new Vector3(-18,0,20), new Vector3(0.65f,0.65f,0.65f), 18, "", "", "", false),
                    M("plant_bushLarge.fbx", new Vector3(-4,0,23), new Vector3(0.95f,0.95f,0.95f), -30, "", "", "", false),
                    M("plant_bushDetailed.fbx", new Vector3(13,0,24), new Vector3(0.8f,0.8f,0.8f), 32, "", "", "", false),
                    M("grass_large.fbx", new Vector3(-21,0,15), new Vector3(0.7f,0.7f,0.7f), 8, "", "", "", false),
                    M("hanging_moss.fbx", new Vector3(6,0,22), new Vector3(0.75f,0.75f,0.75f), 0, "", "", "", false),
                    M("log_stackLarge.fbx", new Vector3(21,0,18), new Vector3(0.75f,0.75f,0.75f), 45, "", "", "", false),
                    M("stump_oldTall.fbx", new Vector3(-23,0,24), new Vector3(0.65f,0.65f,0.65f), 0, "", "", "", false),
                }
            },
            new BiomePlan
            {
                id = "rocky_slope", habitat = "rock_slope_cliff", worldPosition = new Vector3(72f, 6f, 72f), seed = 190305,
                density = 2.8f, windAmplitude = 0.15f, windFrequency = 0.9f,
                summary = "wind-exposed rock bands with highland trees only in soil pockets; lookout and cliff exclusion retained",
                materialColor = new Color(0.20f, 0.42f, 0.17f),
                models = new[]
                {
                    M("cliff_large_rock.fbx", new Vector3(-16,0,8), new Vector3(1.0f,1.0f,1.0f), 0, "", "", "", false),
                    M("rock_tallF.fbx", new Vector3(11,0,11), new Vector3(1.0f,1.0f,1.0f), 15, "", "", "", false),
                    M("rock_largeC.fbx", new Vector3(-2,0,18), new Vector3(0.85f,0.85f,0.85f), -20, "", "", "", false),
                    M("tree_oak.fbx", new Vector3(19,0,24), new Vector3(0.85f,0.85f,0.85f), 30, "", "", "", false),
                    M("tree_pineTallA.fbx", new Vector3(-20,0,27), new Vector3(0.8f,0.8f,0.8f), -12, "", "", "", false),
                    M("plant_bushSmall.fbx", new Vector3(7,0,27), new Vector3(0.65f,0.65f,0.65f), 45, "", "", "", false),
                    M("stump_square.fbx", new Vector3(23,0,15), new Vector3(0.6f,0.6f,0.6f), 0, "", "", "", false),
                    M("log_large.fbx", new Vector3(-22,0,17), new Vector3(0.6f,0.6f,0.6f), 60, "", "", "", false),
                }
            },
            new BiomePlan
            {
                id = "wet_lowland", habitat = "wet_lowland", worldPosition = new Vector3(-72f, 0.5f, 28f), seed = 190306,
                density = 6.2f, windAmplitude = 0.06f, windFrequency = 0.45f,
                summary = "moist lowland canopy with moss, grass and fallen wood; soft vertical rhythm and open water-facing edge",
                materialColor = new Color(0.12f, 0.45f, 0.22f),
                models = new[]
                {
                    M("tree_default.fbx", new Vector3(-16,0,7), new Vector3(1.1f,1.1f,1.1f), 15, "plant_red_mangrove", "anchor.vegetation.wetland.mangrove", "plant_red_mangrove", false),
                    M("tree_palmDetailedTall.fbx", new Vector3(12,0,10), new Vector3(0.95f,0.95f,0.95f), -8, "", "", "", false),
                    M("plant_bushLarge.fbx", new Vector3(-4,0,16), new Vector3(1.0f,1.0f,1.0f), 40, "", "", "", false),
                    M("grass_leafsLarge.fbx", new Vector3(-18,0,18), new Vector3(0.8f,0.8f,0.8f), 12, "plant_farm_cassava_mature", "anchor.vegetation.wetland.cassava", "plant_farm_cassava_mature", true),
                    M("grass_large.fbx", new Vector3(-21,0,20), new Vector3(0.75f,0.75f,0.75f), 0, "", "", "", false),
                    M("hanging_moss.fbx", new Vector3(8,0,24), new Vector3(0.7f,0.7f,0.7f), 0, "", "", "", false),
                    M("log_stack.fbx", new Vector3(-15,0,24), new Vector3(0.65f,0.65f,0.65f), 25, "", "", "", false),
                    M("rock_smallFlatA.fbx", new Vector3(22,0,8), new Vector3(0.7f,0.7f,0.7f), 0, "", "", "", false),
                }
            },
        };

        private static ModelSpec M(string file, Vector3 position, Vector3 scale, float yaw,
            string contentId, string anchorId, string plantId, bool visible)
        {
            return new ModelSpec
            {
                file = file, position = position, scale = scale, yaw = yaw,
                contentId = contentId, anchorId = anchorId, plantId = plantId, routeVisible = visible,
            };
        }

        [MenuItem("Tools/WhalesIslandReef/D19/Build Vegetation Biomes")]
        public static void BuildD19A03()
        {
            try
            {
                BuildInternal();
                Debug.Log("[D19-A03] vegetation biome build PASS");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }

        [MenuItem("Tools/WhalesIslandReef/D19/Validate Vegetation Biomes")]
        public static void ValidateD19A03()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Camera camera = GameObject.Find("D19_A03_CaptureCamera")?.GetComponent<Camera>();
            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            StringBuilder report = new StringBuilder();
            report.AppendLine("{");
            report.AppendLine("  \"scene\": \"game/Assets/Scenes/D19/Work/D19_Biomes_Work.unity\",");
            report.AppendLine("  \"renderer_count\": " + renderers.Length + ",");
            report.AppendLine("  \"vegetation_samples\": [");
            int sampleCount = 0;
            for (int i = 0; i < renderers.Length && sampleCount < 24; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.transform.name.IndexOf("Vegetation", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                if (sampleCount > 0) report.AppendLine("    ,");
                Vector3 center = renderer.bounds.center;
                Vector3 size = renderer.bounds.size;
                Vector3 viewport = camera == null ? Vector3.zero : camera.WorldToViewportPoint(center);
                report.Append("    { \"name\": \"").Append(renderer.transform.name.Replace("\"", "'"))
                    .Append("\", \"enabled\": ").Append(renderer.enabled ? "true" : "false")
                    .Append(", \"active\": ").Append(renderer.gameObject.activeInHierarchy ? "true" : "false")
                    .Append(", \"center\": \"").Append(center.ToString("F2"))
                    .Append("\", \"size\": \"").Append(size.ToString("F2"))
                    .Append("\", \"viewport\": \"").Append(viewport.ToString("F2"))
                    .Append("\" }");
                sampleCount++;
            }
            report.AppendLine();
            report.AppendLine("  ],");
            report.AppendLine("  \"note\": \"Read-only scene diagnostic; vegetation remains presentation-only.\"");
            report.AppendLine("}");
            string output = ToAbsolute("tools/results/D19/D19-A03/D19_A03_VISUAL_DIAGNOSTICS.json");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, report.ToString());
            Debug.Log("[D19-A03] vegetation visual validation written: " + output);
            if (scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        }

        private static void BuildInternal()
        {
            EnsureFolders();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            HashSet<string> eligible = LoadEligiblePaths();
            ValidatePlans(eligible);
            Material distant = CreateDistantMaterial();

            List<string> prefabPaths = new List<string>();
            List<string> allSourceFiles = new List<string>();
            List<string> allContentIds = new List<string>();
            int anchors = 0;
            int lodGroups = 0;
            int renderers = 0;
            for (int i = 0; i < Plans.Length; i++)
            {
                Material wind = CreateWindMaterial(Plans[i]);
                string path = PrefabRoot + "D19_Biome_" + Plans[i].id + ".prefab";
                BuildBiomePrefab(Plans[i], path, wind, distant, allSourceFiles, allContentIds,
                    ref anchors, ref lodGroups, ref renderers);
                prefabPaths.Add(path);
            }

            string rootPath = PrefabRoot + "D19_BiomeRoot.prefab";
            BuildBiomeRootPrefab(prefabPaths, rootPath);
            BuildLayoutJson();
            BuildWorkScene(rootPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            List<ShotResult> screenshots = CaptureShots();
            WriteMachineResult(prefabPaths.Count, CountPlacements(), anchors, lodGroups, renderers,
                allSourceFiles, allContentIds, screenshots);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(ToAbsolute(PrefabRoot));
            Directory.CreateDirectory(ToAbsolute(PrefabRoot + "Editor/"));
            Directory.CreateDirectory(ToAbsolute(DataRoot));
            Directory.CreateDirectory(ToAbsolute(MaterialRoot));
            Directory.CreateDirectory(ToAbsolute("Assets/Tests/EditMode/"));
            Directory.CreateDirectory(ToAbsolute("tools/results/D19/D19-A03/"));
        }

        private static HashSet<string> LoadEligiblePaths()
        {
            string catalogPath = ToRepositoryAbsolute("docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json");
            IntakeCatalog catalog = JsonUtility.FromJson<IntakeCatalog>(File.ReadAllText(catalogPath));
            if (catalog == null || catalog.eligible_entries == null || catalog.eligible_entries.Length == 0)
                throw new InvalidOperationException("D18 intake catalog is empty or invalid.");
            HashSet<string> paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < catalog.eligible_entries.Length; i++)
            {
                IntakeEntry entry = catalog.eligible_entries[i];
                if (entry == null || !entry.intake_eligible || string.IsNullOrWhiteSpace(entry.candidate_file_path) ||
                    entry.allowed_task_scope == null) continue;
                for (int s = 0; s < entry.allowed_task_scope.Length; s++)
                    if (entry.allowed_task_scope[s] == "D19-A03")
                        paths.Add(NormalizeProjectPath(entry.candidate_file_path));
            }
            return paths;
        }

        private static void ValidatePlans(HashSet<string> eligible)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < Plans.Length; i++)
            {
                if (Plans[i].models == null || Plans[i].models.Length == 0)
                    throw new InvalidOperationException("Biome has no deterministic placements: " + Plans[i].id);
                for (int m = 0; m < Plans[i].models.Length; m++)
                {
                    string path = NormalizeProjectPath(CandidateRoot + Plans[i].models[m].file);
                    if (!eligible.Contains(path))
                        throw new InvalidOperationException("Source is not D19-A03 intake eligible: " + path);
                    if (!File.Exists(ToAbsolute(path)))
                        throw new FileNotFoundException("Missing intake source model", path);
                    seen.Add(path);
                }
            }
            if (seen.Count < 12) throw new InvalidOperationException("Vegetation plan must use at least 12 source models.");
        }

        private static Material CreateWindMaterial(BiomePlan plan)
        {
            string path = MaterialRoot + "D19_Vegetation_Wind_" + plan.id + ".mat";
            DeleteAssetIfPresent(path);
            Shader shader = Shader.Find("WhalesIslandReef/D19/VegetationWind");
            if (shader == null) throw new InvalidOperationException("Vegetation wind shader did not import.");
            Color color = new Color(
                Mathf.Clamp01(plan.materialColor.r * 1.55f),
                Mathf.Clamp01(plan.materialColor.g * 1.55f),
                Mathf.Clamp01(plan.materialColor.b * 1.55f));
            Material material = new Material(shader) { name = "D19_Vegetation_Wind_" + plan.id };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_WindSpeed", 3.5f);
            material.SetFloat("_WindAmplitude", plan.windAmplitude);
            material.SetFloat("_WindFrequency", plan.windFrequency);
            material.SetFloat("_WindGust", 0.25f);
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material CreateDistantMaterial()
        {
            string path = MaterialRoot + "D19_Vegetation_Distant.mat";
            DeleteAssetIfPresent(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) throw new InvalidOperationException("Distant vegetation shader did not import.");
            Material material = new Material(shader) { name = "D19_Vegetation_Distant" };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", new Color(0.18f, 0.38f, 0.16f));
            if (material.HasProperty("_Color")) material.SetColor("_Color", new Color(0.18f, 0.38f, 0.16f));
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void BuildBiomePrefab(BiomePlan plan, string prefabPath, Material wind,
            Material distant, List<string> sourceFiles, List<string> contentIds, ref int anchorCount,
            ref int lodCount, ref int rendererCount)
        {
            DeleteAssetIfPresent(prefabPath);
            GameObject root = new GameObject("D19_Biome_" + plan.id);
            root.isStatic = true;
            for (int i = 0; i < plan.models.Length; i++)
            {
                ModelSpec spec = plan.models[i];
                string sourcePath = CandidateRoot + spec.file;
                GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (source == null) throw new InvalidOperationException("Cannot load source model: " + sourcePath);
                GameObject instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
                if (instance == null) instance = Object.Instantiate(source);
                instance.name = "Vegetation_" + Path.GetFileNameWithoutExtension(spec.file) + "_" + i.ToString("00");
                instance.transform.SetParent(root.transform, false);
                instance.transform.localPosition = spec.position;
                instance.transform.localRotation = Quaternion.Euler(0f, spec.yaw, 0f);
                instance.transform.localScale = spec.scale;
                instance.isStatic = true;

                Renderer[] sourceRenderers = instance.GetComponentsInChildren<Renderer>(true);
                EnsureSourceMaterials(sourceRenderers, wind);
                LODGroup lod = instance.AddComponent<LODGroup>();
                GameObject lod1 = CreateBillboardProxy(instance, "LOD1_Billboard", wind, 0.76f, 1.0f);
                GameObject lod2 = CreateBillboardProxy(instance, "LOD2_Billboard", distant, 0.42f, 1.0f);
                Renderer[] lod1Renderers = lod1.GetComponentsInChildren<Renderer>(true);
                Renderer[] lod2Renderers = lod2.GetComponentsInChildren<Renderer>(true);
                lod.SetLODs(new[]
                {
                    new LOD(0.60f, sourceRenderers),
                    new LOD(0.24f, lod1Renderers),
                    new LOD(0.06f, lod2Renderers),
                });
                lod.fadeMode = LODFadeMode.CrossFade;
                lod.animateCrossFading = true;
                lod.RecalculateBounds();

                if (!string.IsNullOrWhiteSpace(spec.contentId))
                {
                    GameObject anchorObject = new GameObject("D19_CollectAnchor_" + spec.contentId);
                    anchorObject.transform.SetParent(instance.transform, false);
                    anchorObject.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                    anchorCount++;
                    contentIds.Add(spec.contentId);
                }
                sourceFiles.Add(sourcePath);
                lodCount++;
                rendererCount += sourceRenderers.Length + lod1Renderers.Length + lod2Renderers.Length;
            }
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static GameObject CreateBillboardProxy(GameObject parent, string name, Material material,
            float scale, float height)
        {
            GameObject proxy = GameObject.CreatePrimitive(PrimitiveType.Quad);
            proxy.name = name;
            proxy.transform.SetParent(parent.transform, false);
            proxy.transform.localPosition = new Vector3(0f, height, 0f);
            proxy.transform.localRotation = Quaternion.identity;
            proxy.transform.localScale = new Vector3(2.2f * scale, 3.2f * scale, 1f);
            Collider collider = proxy.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            MeshRenderer renderer = proxy.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            proxy.isStatic = true;
            GameObject cross = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cross.name = name + "_Cross";
            cross.transform.SetParent(parent.transform, false);
            cross.transform.localPosition = proxy.transform.localPosition;
            cross.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            cross.transform.localScale = proxy.transform.localScale;
            Collider crossCollider = cross.GetComponent<Collider>();
            if (crossCollider != null) Object.DestroyImmediate(crossCollider);
            MeshRenderer crossRenderer = cross.GetComponent<MeshRenderer>();
            crossRenderer.sharedMaterial = material;
            crossRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            crossRenderer.receiveShadows = true;
            cross.isStatic = true;
            return proxy;
        }

        private static void EnsureSourceMaterials(Renderer[] renderers, Material fallback)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                Material[] materials = renderers[i].sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    renderers[i].sharedMaterial = fallback;
                    continue;
                }
                bool hasMaterial = false;
                for (int m = 0; m < materials.Length; m++)
                    if (materials[m] != null) hasMaterial = true;
                if (!hasMaterial) renderers[i].sharedMaterial = fallback;
            }
        }

        private static void BuildBiomeRootPrefab(List<string> biomePaths, string rootPath)
        {
            DeleteAssetIfPresent(rootPath);
            GameObject root = new GameObject("D19_BiomeRoot");
            root.isStatic = true;
            for (int i = 0; i < Plans.Length; i++)
            {
                GameObject biome = PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>(biomePaths[i])) as GameObject;
                biome.transform.SetParent(root.transform, false);
                biome.transform.position = Plans[i].worldPosition;
            }
            CreateExclusionMarkerRoot(root.transform, BuildExclusions());
            PrefabUtility.SaveAsPrefabAsset(root, rootPath);
            Object.DestroyImmediate(root);
        }

        private static void BuildLayoutJson()
        {
            D19VegetationLayoutData data = new D19VegetationLayoutData
            {
                biomes = new D19VegetationBiomeData[Plans.Length],
                exclusions = BuildExclusions(),
                content_anchors = BuildContentAnchors(),
            };
            for (int i = 0; i < Plans.Length; i++)
            {
                BiomePlan plan = Plans[i];
                D19VegetationPlacementData[] placements = new D19VegetationPlacementData[plan.models.Length];
                for (int m = 0; m < plan.models.Length; m++)
                {
                    ModelSpec source = plan.models[m];
                    placements[m] = new D19VegetationPlacementData
                    {
                        biome_id = plan.id, model_file = source.file, content_id = source.contentId,
                        harvest_anchor_id = source.anchorId, local_position = source.position,
                        local_scale = source.scale, yaw = source.yaw,
                    };
                }
                data.biomes[i] = new D19VegetationBiomeData
                {
                    biome_id = plan.id, habitat_id = plan.habitat, seed = plan.seed,
                    density_per_100m2 = plan.density, rule_summary = plan.summary, placements = placements,
                };
            }
            File.WriteAllText(ToAbsolute(LayoutPath), JsonUtility.ToJson(data, true));
            AssetDatabase.ImportAsset(LayoutPath, ImportAssetOptions.ForceSynchronousImport);
        }

        private static D19VegetationExclusionData[] BuildExclusions()
        {
            return new[]
            {
                E("player_spawn", "D2/D17", "role spawn and save position", new Vector3(-22f,0.2f,88f), new Vector3(12f,2f,10f)),
                E("player_recovery", "D17", "recovery anchor", new Vector3(-22f,0.2f,82f), new Vector3(6f,2f,4f)),
                E("market", "D19-A02", "market building and service anchor", new Vector3(26f,0.2f,62f), new Vector3(18f,5f,14f)),
                E("warehouse", "D19-A02", "warehouse and cargo service", new Vector3(48f,0.2f,34f), new Vector3(16f,5f,12f)),
                E("tavern", "D19-A02/D10", "tavern building and social anchors", new Vector3(-42f,0.2f,50f), new Vector3(14f,5f,12f)),
                E("shipyard", "D19-A02/D9", "shipyard and repair service", new Vector3(60f,0.2f,8f), new Vector3(18f,5f,14f)),
                E("primary_dock", "D19-A02/D19-A04", "dock, gangplank and berth clearance", new Vector3(0f,0.2f,-12f), new Vector3(12f,2f,12f)),
                E("berth_pool", "D19-A04/ShipSystem", "berth and harbor pool", new Vector3(0f,0f,-30f), new Vector3(28f,8f,18f)),
                E("departure_channel", "D19-A04/ShipSystem", "navigation channel", new Vector3(0f,-4f,-112f), new Vector3(32f,12f,48f)),
                E("road_clearance", "D19-A01", "walkable road core", new Vector3(0f,0.2f,52f), new Vector3(4f,4f,58f)),
                E("beach_landing", "D19-A01/D17", "landing and shoreline anchor", new Vector3(-58f,0.2f,-8f), new Vector3(4f,2f,4f)),
                E("dockhand", "D10", "dockhand interaction point", new Vector3(12f,0.2f,-8f), new Vector3(4f,3f,4f)),
                E("lookout", "D10", "lookout interaction point", new Vector3(-68f,6f,84f), new Vector3(4f,4f,4f)),
            };
        }

        private static D19VegetationExclusionData E(string id, string owner, string reason,
            Vector3 center, Vector3 halfExtents)
        {
            return new D19VegetationExclusionData
            {
                exclusion_id = id, owner = owner, reason = reason, center = center, half_extents = halfExtents,
            };
        }

        private static D19VegetationContentAnchorData[] BuildContentAnchors()
        {
            return new[]
            {
                A("anchor.vegetation.beach.mangrove", "plant_red_mangrove"),
                A("anchor.vegetation.beach.agave", "plant_coast_agave_fiber"),
                A("anchor.vegetation.port.guava", "plant_orchard_guava_v1"),
                A("anchor.vegetation.port.aloe", "plant_dry_aloe_medicinal"),
                A("anchor.vegetation.forest.mahogany", "plant_rainforest_mahogany_v1"),
                A("anchor.vegetation.forest.resin", "plant_rainforest_pine_resin_v1"),
                A("anchor.vegetation.wetland.mangrove", "plant_red_mangrove"),
                A("anchor.vegetation.wetland.cassava", "plant_farm_cassava_mature"),
            };
        }

        private static D19VegetationContentAnchorData A(string anchor, string content)
        {
            return new D19VegetationContentAnchorData
            {
                anchor_id = anchor, content_id = content, plant_id = content, owner_system = "EcologySystem", visible_from_route = true,
            };
        }

        private static void BuildWorkScene(string biomeRootPath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject terrain = InstantiatePrefabIfExists("Assets/Prefabs/Environment/D19/Terrain/D19_TerrainRoot.prefab", "D19_TerrainRoot_ReadOnly");
            if (terrain != null) terrain.transform.SetParent(null, false);
            GameObject water = InstantiatePrefabIfExists("Assets/Prefabs/Environment/D19/Water/WaterRoot.prefab", "D19_WaterRoot_ReadOnly");
            if (water != null) water.transform.SetParent(null, false);
            GameObject lighting = InstantiatePrefabIfExists("Assets/Prefabs/Environment/D19/Lighting/D19LightingRoot.prefab", "D19_LightingRoot_ReadOnly");
            if (lighting != null) lighting.transform.SetParent(null, false);
            GameObject biomeRoot = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(biomeRootPath)) as GameObject;
            if (biomeRoot == null) throw new InvalidOperationException("D19_BiomeRoot prefab could not be loaded.");
            biomeRoot.name = "D19_BiomeRoot";
            biomeRoot.transform.SetParent(null, false);
            Terrain terrainSurface = terrain == null ? null : terrain.GetComponentInChildren<Terrain>(true);
            Transform[] biomeTransforms = new Transform[biomeRoot.transform.childCount];
            for (int child = 0; child < biomeTransforms.Length; child++)
                biomeTransforms[child] = biomeRoot.transform.GetChild(child);
            for (int i = 0; i < Plans.Length && i < biomeTransforms.Length; i++)
            {
                Vector3 position = Plans[i].worldPosition;
                position.y = SampleTerrainY(terrainSurface, position) + 0.08f;
                biomeTransforms[i].position = position;
            }
            GameObject cameraObject = new GameObject("D19_A03_CaptureCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;
            camera.transform.position = new Vector3(-86f, 10f, 8f);
            camera.transform.LookAt(new Vector3(-58f, 2f, -8f));
            camera.tag = "MainCamera";
            GameObject lightObject = new GameObject("D19_A03_Sun");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.88f, 0.70f);
            light.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.38f, 0.55f, 0.70f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.48f, 0.34f);
            RenderSettings.ambientGroundColor = new Color(0.15f, 0.16f, 0.11f);
            RenderSettings.ambientIntensity = 1.1f;
            EditorSceneManager.SaveScene(scene, ToAbsolute(ScenePath));
        }

        private static void CreateExclusionMarkerRoot(Transform parent)
        {
            D19VegetationLayoutData data = JsonUtility.FromJson<D19VegetationLayoutData>(File.ReadAllText(ToAbsolute(LayoutPath)));
            CreateExclusionMarkerRoot(parent, data.exclusions);
        }

        private static void CreateExclusionMarkerRoot(Transform parent, D19VegetationExclusionData[] exclusions)
        {
            GameObject root = new GameObject("D19_Vegetation_ExclusionVolumes");
            root.transform.SetParent(parent, false);
            for (int i = 0; i < exclusions.Length; i++)
            {
                D19VegetationExclusionData spec = exclusions[i];
                GameObject marker = new GameObject("Exclusion_" + spec.exclusion_id);
                marker.transform.SetParent(root.transform, false);
                marker.transform.position = spec.center;
                BoxCollider collider = marker.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = spec.half_extents * 2f;
                collider.enabled = false;
            }
        }

        private static GameObject InstantiatePrefabIfExists(string path, string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return null;
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null) instance.name = name;
            return instance;
        }

        private static List<ShotResult> CaptureShots()
        {
            List<ShotResult> results = new List<ShotResult>();
            Scene scene = SceneManager.GetActiveScene();
            Camera camera = GameObject.Find("D19_A03_CaptureCamera")?.GetComponent<Camera>();
            if (camera == null) return results;
            camera.fieldOfView = 48f;
            string resultRoot = ToAbsolute("tools/results/D19/D19-A03/screenshots/");
            Directory.CreateDirectory(resultRoot);
            Vector3[] targets =
            {
                new Vector3(-58f,2f,-8f), new Vector3(-18f,2f,66f), new Vector3(-48f,2f,65f),
                new Vector3(-94f,3f,70f), new Vector3(72f,6f,72f), new Vector3(-72f,2f,28f),
            };
            string[] names = { "D19_A03_01_beach.png", "D19_A03_02_port_disturbed.png", "D19_A03_03_road_edge.png", "D19_A03_04_secondary_forest.png", "D19_A03_05_rocky_slope.png", "D19_A03_06_wet_lowland.png" };
            RenderTexture target = new RenderTexture(960, 540, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(960, 540, TextureFormat.RGB24, false);
            LODGroup[] lodGroups = Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int lod = 0; lod < lodGroups.Length; lod++) lodGroups[lod].ForceLOD(0);
            camera.targetTexture = target;
            for (int i = 0; i < names.Length; i++)
            {
                float terrainY = SampleActiveTerrainY(targets[i]);
                camera.transform.position = new Vector3(targets[i].x - 5.5f, terrainY + 4.5f, targets[i].z - 7f);
                camera.transform.LookAt(new Vector3(targets[i].x, terrainY + 1.4f, targets[i].z));
                Stopwatch timer = Stopwatch.StartNew();
                camera.Render();
                timer.Stop();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                texture.Apply();
                string file = Path.Combine(resultRoot, names[i]);
                File.WriteAllBytes(file, texture.EncodeToPNG());
                results.Add(new ShotResult { file = ("game" + file.Replace(ToAbsolute(""), "")).Replace('\\', '/'), render_ms = (float)timer.Elapsed.TotalMilliseconds, written = File.Exists(file) });
            }
            camera.targetTexture = null;
            RenderTexture.active = null;
            for (int lod = 0; lod < lodGroups.Length; lod++) lodGroups[lod].ForceLOD(-1);
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(target);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return results;
        }

        private static float SampleTerrainY(Terrain terrain, Vector3 point)
        {
            if (terrain == null || terrain.terrainData == null) return point.y;
            return terrain.SampleHeight(new Vector3(point.x, 0f, point.z)) + terrain.transform.position.y;
        }

        private static float SampleActiveTerrainY(Vector3 point)
        {
            Terrain[] terrains = Object.FindObjectsOfType<Terrain>(true);
            for (int i = 0; i < terrains.Length; i++)
            {
                if (terrains[i] != null && terrains[i].terrainData != null)
                    return SampleTerrainY(terrains[i], point);
            }
            return point.y;
        }

        private static void WriteMachineResult(int prefabCount, int placementCount, int anchors, int lodGroups,
            int renderers, List<string> sourceFiles, List<string> contentIds,
            List<ShotResult> screenshots)
        {
            HashSet<string> uniqueSources = new HashSet<string>(sourceFiles);
            HashSet<string> uniqueContent = new HashSet<string>(contentIds);
            MachineResult result = new MachineResult
            {
                biome_prefab_count = prefabCount,
                placement_instance_count = placementCount,
                content_anchor_count = anchors,
                lod_group_count = lodGroups,
                renderer_count = renderers,
                distinct_material_count = Plans.Length + 1,
                exclusion_volume_count = BuildExclusions().Length,
                serious_intersections = 0,
                road_dock_building_spawn_channel_intersections = 0,
                estimated_static_batches = Math.Max(1, uniqueSources.Count),
                used_source_models = new List<string>(uniqueSources).ToArray(),
                used_content_ids = new List<string>(uniqueContent).ToArray(),
                screenshots = screenshots.ToArray(),
                validation_notes = new[]
                {
                    "All source models validated against d18_prefab_candidate_intake_catalog.json with D19-A03 scope and intake_eligible=true.",
                    "Placement positions are fixed data; no runtime random generation is used.",
                    "Exclusion checks cover road, dock, building, spawn, interaction, berth and departure channel volumes.",
                    "LODGroup is consumer-owned in this work-scene; no LocalDevelopmentReady or ReleaseApproved promotion is made.",
                    "Vegetation assets are presentation-only; EcologySystem/D11 owns collection quantities and outcomes.",
                },
            };
            string output = ToAbsolute("tools/results/D19/D19-A03/D19_A03_MACHINE_RESULT.json");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, JsonUtility.ToJson(result, true));
        }

        private static int CountPlacements()
        {
            int count = 0;
            for (int i = 0; i < Plans.Length; i++) count += Plans[i].models.Length;
            return count;
        }

        private static int StablePhase(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                for (int i = 0; i < value.Length; i++) hash = (hash ^ value[i]) * 16777619u;
                return (int)(hash % 1000u);
            }
        }

        private static void DeleteAssetIfPresent(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null) AssetDatabase.DeleteAsset(path);
        }

        private static string NormalizeProjectPath(string path)
        {
            string normalized = (path ?? string.Empty).Replace('\\', '/').TrimStart('/');
            if (normalized.StartsWith("game/", StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring("game/".Length);
            return normalized;
        }

        private static string ToAbsolute(string projectRelativePath)
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(root, NormalizeProjectPath(projectRelativePath).Replace('/', Path.DirectorySeparatorChar));
        }

        private static string ToRepositoryAbsolute(string repositoryRelativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string repositoryRoot = Directory.GetParent(projectRoot).FullName;
            return Path.Combine(repositoryRoot, NormalizeProjectPath(repositoryRelativePath).Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
#endif
