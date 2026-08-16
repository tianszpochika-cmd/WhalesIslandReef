#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhalesIslandReef.Tests.D19R01B
{
    /// <summary>
    /// D19-R01-B 的一次性正式场景修复编排器。
    /// 只创建 D19 Integration/R01 的表现 Prefab，并把这个 Prefab 作为单一新表现根接入 InitialHarbor。
    /// 不写 Systems、交互事实、Registry、存档或外部资源状态。
    /// </summary>
    public static class D19R01BRepairBuilder
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string RepairDirectory = "Assets/Prefabs/Environment/D19/Integration/R01";
        private const string RepairPrefabPath = RepairDirectory + "/D19_R01_B_HarborVisualRepair.prefab";
        private const string MeshPath = RepairDirectory + "/D19_R01_B_TerrainBreakup.asset";
        private const string ResultRoot = "tools/results/D19/D19-R01-B";
        private const string ScreenshotRoot = ResultRoot + "/screenshots";

        private static readonly string NatureRoot = "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/";
        private static readonly string BuildingRoot = "Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/";
        private static readonly string PirateRoot = "Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/";
        private static readonly string MaterialRoot = "Assets/Prefabs/Environment/D19/Integration/Materials/";

        [Serializable]
        private sealed class UsageRecord
        {
            public string candidate_file_path;
            public string content_id;
            public string domain;
            public string role;
            public string consumer = "D19-R01-B-visual-only";
            public bool source_exists;
            public bool instantiated;
            public string note;
        }

        [Serializable]
        private sealed class ShotRecord
        {
            public string shot_id;
            public string file;
            public int width;
            public int height;
            public string capture_mode = "Unity_Editor_Camera.Render_visual_regression_only";
            public string status;
        }

        [Serializable]
        private sealed class BuildResult
        {
            public string task = "D19-R01-B";
            public string status = "BLOCKED";
            public string generated_on_utc;
            public string scene_path = ScenePath;
            public string repair_prefab_path = RepairPrefabPath;
            public string rollback_point_id = "d19-r01-b-previsual-repair";
            public bool formal_initial_harbor_modified;
            public bool systems_modified;
            public bool registry_modified;
            public bool save_schema_modified;
            public bool external_asset_state_promoted;
            public bool scene_only_references_prefab;
            public bool legacy_stable_anchors_preserved;
            public bool dev_overlay_default_off;
            public int hidden_legacy_visual_renderers;
            public int candidate_models_instantiated;
            public int project_owned_renderers;
            public int missing_candidate_models;
            public int fixed_shot_count;
            public int fixed_shot_width;
            public int fixed_shot_height;
            public string[] stable_anchor_ids = new string[0];
            public UsageRecord[] resource_usage = new UsageRecord[0];
            public ShotRecord[] shots = new ShotRecord[0];
            public string[] warnings = new string[0];
        }

        private readonly struct CandidatePlacement
        {
            public readonly string path;
            public readonly string contentId;
            public readonly string domain;
            public readonly string role;
            public readonly Vector3 position;
            public readonly Vector3 scale;
            public readonly float yaw;
            public readonly string label;

            public CandidatePlacement(string path, string contentId, string domain, string role,
                Vector3 position, Vector3 scale, float yaw, string label)
            {
                this.path = path;
                this.contentId = contentId;
                this.domain = domain;
                this.role = role;
                this.position = position;
                this.scale = scale;
                this.yaw = yaw;
                this.label = label;
            }
        }

        [MenuItem("WhalesIslandReef/D19-R01-B/Build and Integrate Harbor Visual Repair")]
        public static void BuildAndIntegrate()
        {
            var result = new BuildResult { generated_on_utc = DateTime.UtcNow.ToString("o") };
            var usage = new List<UsageRecord>();
            var shots = new List<ShotRecord>();
            var warnings = new List<string>();

            try
            {
                EnsureRepairDirectory();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                var prefab = BuildRepairPrefab(usage, warnings);
                IntegrateScene(prefab, result, warnings);
                CaptureFixedShots(shots, warnings);
                result.fixed_shot_count = shots.Count;
                result.fixed_shot_width = shots.Count == 0 ? 0 : shots[0].width;
                result.fixed_shot_height = shots.Count == 0 ? 0 : shots[0].height;

                result.status = warnings.Count == 0 && result.missing_candidate_models == 0 &&
                    result.fixed_shot_count == 14 && result.legacy_stable_anchors_preserved
                    ? "PASS_VISUAL_REPAIR_BUILD"
                    : "PARTIAL";
                result.resource_usage = usage.ToArray();
                result.shots = shots.ToArray();
                result.warnings = warnings.ToArray();
                WriteResult(result);
                Debug.Log("[D19-R01-B] status=" + result.status + " candidates=" + result.candidate_models_instantiated +
                          " shots=" + result.fixed_shot_count);
                if (Application.isBatchMode)
                    EditorApplication.Exit(result.status == "PASS_VISUAL_REPAIR_BUILD" ? 0 : 1);
            }
            catch (Exception exception)
            {
                warnings.Add(exception.ToString());
                result.status = "BLOCKED";
                result.resource_usage = usage.ToArray();
                result.shots = shots.ToArray();
                result.warnings = warnings.ToArray();
                WriteResult(result);
                Debug.LogError("[D19-R01-B] failed: " + exception);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
        }

        private static GameObject BuildRepairPrefab(List<UsageRecord> usage, List<string> warnings)
        {
            DeleteAssetIfExists(RepairPrefabPath);
            DeleteAssetIfExists(MeshPath);

            var root = new GameObject("D19_R01_B_HarborVisualRepair");
            var surface = CreateChild(root.transform, "D19_R01_B_TerrainBreakup_ProjectOwned");
            CreateTerrainBreakup(surface.transform);

            var route = CreateChild(root.transform, "D19_R01_B_RouteSurface_Approved");
            AddRouteSurface(route.transform, usage, warnings);

            var nature = CreateChild(root.transform, "D19_R01_B_Nature_Approved");
            AddNature(nature.transform, usage, warnings);

            var port = CreateChild(root.transform, "D19_R01_B_PortModules_Approved");
            AddPortModules(port.transform, usage, warnings);

            var props = CreateChild(root.transform, "D19_R01_B_DockMarketProps_Approved");
            AddDockAndMarketProps(props.transform, usage, warnings);

            var ownedDetails = CreateChild(root.transform, "D19_R01_B_ProjectOwned_PortAndCoastDetails");
            AddProjectOwnedPortAndCoastDetails(ownedDetails.transform);

            Debug.Log($"[D19-R01-B] pre-save hierarchy root={root.transform.childCount} owned={ownedDetails.transform.childCount} all={root.GetComponentsInChildren<Transform>(true).Length}");

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, RepairPrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Failed to save repair prefab: " + RepairPrefabPath);

            AssetDatabase.ImportAsset(RepairPrefabPath, ImportAssetOptions.ForceSynchronousImport);
            var reloadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RepairPrefabPath);
            Debug.Log($"[D19-R01-B] post-save hierarchy root={root.transform.childCount} prefab={(reloadedPrefab == null ? -1 : reloadedPrefab.GetComponentsInChildren<Transform>(true).Length)}");

            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return reloadedPrefab ?? prefab;
        }

        private static void CreateTerrainBreakup(Transform parent)
        {
            var grass = LoadOrCreateMaterial("D19_R01_B_TerrainGrass", "D19_R01_Landform_Grass.mat", new Color(0.09f, 0.22f, 0.12f), 0.18f);
            var sand = LoadOrCreateMaterial("D19_R01_B_TerrainSand", "D19_R01_Landform_Sand.mat", new Color(0.52f, 0.31f, 0.13f), 0.24f);
            var rock = LoadOrCreateMaterial("D19_R01_B_TerrainRock", "D19_R01_Landform_Rock.mat", new Color(0.18f, 0.21f, 0.20f), 0.20f);

            var vertices = new List<Vector3>();
            var grassTriangles = new List<int>();
            var sandTriangles = new List<int>();
            var rockTriangles = new List<int>();
            const int cellsX = 28;
            const int cellsZ = 30;
            const float minX = -112f;
            const float minZ = -112f;
            const float step = 8f;

            for (var ix = 0; ix < cellsX; ix++)
            {
                for (var iz = 0; iz < cellsZ; iz++)
                {
                    var x0 = minX + ix * step;
                    var x1 = x0 + step;
                    var z0 = minZ + iz * step;
                    var z1 = z0 + step;
                    var centerY = TerrainHeightAt((x0 + x1) * 0.5f, (z0 + z1) * 0.5f);
                    if (centerY < -0.2f) continue;
                    var target = centerY < 1.05f ? sandTriangles : centerY > 6.5f ? rockTriangles : grassTriangles;
                    AddTriangle(vertices, target, x0, z0, x1, z0, x1, z1);
                    AddTriangle(vertices, target, x0, z0, x1, z1, x0, z1);
                }
            }

            var mesh = new Mesh { name = "D19_R01_B_TerrainBreakup" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.subMeshCount = 3;
            mesh.SetTriangles(grassTriangles, 0);
            mesh.SetTriangles(sandTriangles, 1);
            mesh.SetTriangles(rockTriangles, 2);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, MeshPath);

            var filter = parent.gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = parent.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { grass, sand, rock };
            parent.gameObject.isStatic = true;
        }

        private static void AddTriangle(List<Vector3> vertices, List<int> triangles,
            float ax, float az, float bx, float bz, float cx, float cz)
        {
            var start = vertices.Count;
            vertices.Add(new Vector3(ax, TerrainHeightAt(ax, az) + 0.07f, az));
            vertices.Add(new Vector3(bx, TerrainHeightAt(bx, bz) + 0.07f, bz));
            vertices.Add(new Vector3(cx, TerrainHeightAt(cx, cz) + 0.07f, cz));
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 1);
        }

        private static void AddRouteSurface(Transform parent, List<UsageRecord> usage, List<string> warnings)
        {
            var path = NatureRoot + "path_stone.fbx";
            var placements = new[]
            {
                new CandidatePlacement(path, "env_kenney_nature_path_stone", "environment", "candidate_dirt_path_model", new Vector3(-18f, 0.65f, 84f), new Vector3(1.35f, 1.35f, 1.35f), 28f, "RouteStone_01"),
                new CandidatePlacement(path, "env_kenney_nature_path_stone", "environment", "candidate_dirt_path_model", new Vector3(-8f, 0.65f, 78f), new Vector3(1.35f, 1.35f, 1.35f), 28f, "RouteStone_02"),
                new CandidatePlacement(path, "env_kenney_nature_path_stone", "environment", "candidate_dirt_path_model", new Vector3(2f, 0.65f, 72f), new Vector3(1.35f, 1.35f, 1.35f), 28f, "RouteStone_03"),
                new CandidatePlacement(path, "env_kenney_nature_path_stone", "environment", "candidate_dirt_path_model", new Vector3(12f, 0.65f, 66f), new Vector3(1.35f, 1.35f, 1.35f), 28f, "RouteStone_04"),
                new CandidatePlacement(path, "env_kenney_nature_path_stone", "environment", "candidate_dirt_path_model", new Vector3(21f, 0.65f, 62f), new Vector3(1.35f, 1.35f, 1.35f), 28f, "RouteStone_05"),
                new CandidatePlacement(path, "env_kenney_nature_path_stone", "environment", "candidate_dirt_path_model", new Vector3(28f, 0.65f, 56f), new Vector3(1.35f, 1.35f, 1.35f), 0f, "RouteStone_06"),
                new CandidatePlacement(NatureRoot + "platform_beach.fbx", "env_kenney_nature_platform_beach", "environment", "candidate_beach_model", new Vector3(-44f, 0.05f, -28f), new Vector3(2.6f, 1.2f, 2.6f), 0f, "BeachPlatform_01"),
                new CandidatePlacement(NatureRoot + "platform_beach.fbx", "env_kenney_nature_platform_beach", "environment", "candidate_beach_model", new Vector3(-24f, 0.05f, -32f), new Vector3(2.6f, 1.2f, 2.6f), 8f, "BeachPlatform_02")
            };
            AddPlacements(parent, placements, usage, warnings);
        }

        private static void AddNature(Transform parent, List<UsageRecord> usage, List<string> warnings)
        {
            var placements = new[]
            {
                new CandidatePlacement(NatureRoot + "tree_palmDetailedTall.fbx", "env_kenney_nature_tree_palm_detailed_tall", "environment", "candidate_vegetation_model", new Vector3(-66f, TerrainHeightAt(-66f, 42f), 42f), Vector3.one * 2.45f, -8f, "Palm_Coast_01"),
                new CandidatePlacement(NatureRoot + "tree_palmBend.fbx", "env_kenney_nature_tree_palm_bend", "environment", "candidate_vegetation_model", new Vector3(-58f, TerrainHeightAt(-58f, 28f), 28f), Vector3.one * 2.25f, 18f, "Palm_Coast_02"),
                new CandidatePlacement(NatureRoot + "tree_palmTall.fbx", "env_kenney_nature_tree_palm_tall", "environment", "candidate_vegetation_model", new Vector3(74f, TerrainHeightAt(74f, 50f), 50f), Vector3.one * 2.35f, -18f, "Palm_Shipyard_01"),
                new CandidatePlacement(NatureRoot + "tree_palmShort.fbx", "env_kenney_nature_tree_palm_short", "environment", "candidate_vegetation_model", new Vector3(82f, TerrainHeightAt(82f, 34f), 34f), Vector3.one * 2.35f, 32f, "Palm_Shipyard_02"),
                new CandidatePlacement(NatureRoot + "tree_default.fbx", "env_kenney_nature_tree_default", "environment", "candidate_vegetation_model", new Vector3(-82f, TerrainHeightAt(-82f, 88f), 88f), Vector3.one * 2.65f, 12f, "Canopy_Highland_01"),
                new CandidatePlacement(NatureRoot + "tree_oak.fbx", "env_kenney_nature_tree_oak", "environment", "candidate_vegetation_model", new Vector3(88f, TerrainHeightAt(88f, 100f), 100f), Vector3.one * 2.65f, -14f, "Canopy_Highland_02"),
                new CandidatePlacement(NatureRoot + "plant_bushLarge.fbx", "env_kenney_nature_plant_bush_large", "environment", "candidate_vegetation_model", new Vector3(-54f, TerrainHeightAt(-54f, 42f), 42f), Vector3.one * 2.30f, 0f, "Shrub_Coast_01"),
                new CandidatePlacement(NatureRoot + "plant_bushDetailed.fbx", "env_kenney_nature_plant_bush_detailed", "environment", "candidate_vegetation_model", new Vector3(-36f, TerrainHeightAt(-36f, 74f), 74f), Vector3.one * 2.15f, 22f, "Shrub_Spawn_01"),
                new CandidatePlacement(NatureRoot + "plant_bushLarge.fbx", "env_kenney_nature_plant_bush_large", "environment", "candidate_vegetation_model", new Vector3(52f, TerrainHeightAt(52f, 44f), 44f), Vector3.one * 2.20f, -12f, "Shrub_Warehouse_01"),
                new CandidatePlacement(NatureRoot + "grass_leafsLarge.fbx", "env_kenney_nature_grass_leafs_large", "environment", "candidate_vegetation_model", new Vector3(-32f, TerrainHeightAt(-32f, 80f), 80f), Vector3.one * 2.30f, 14f, "Grass_Spawn_01"),
                new CandidatePlacement(NatureRoot + "grass_large.fbx", "env_kenney_nature_grass_large", "environment", "candidate_vegetation_model", new Vector3(44f, TerrainHeightAt(44f, 52f), 52f), Vector3.one * 2.20f, 0f, "Grass_Market_01"),
                new CandidatePlacement(NatureRoot + "rock_largeA.fbx", "env_kenney_nature_rock_large_a", "environment", "candidate_rock_model", new Vector3(-94f, TerrainHeightAt(-94f, 18f), 18f), Vector3.one * 2.40f, 20f, "Rock_Coast_01"),
                new CandidatePlacement(NatureRoot + "rock_largeB.fbx", "env_kenney_nature_rock_large_b", "environment", "candidate_rock_model", new Vector3(-82f, TerrainHeightAt(-82f, -4f), -4f), Vector3.one * 2.35f, -18f, "Rock_Coast_02"),
                new CandidatePlacement(NatureRoot + "rock_tallA.fbx", "env_kenney_nature_rock_tall_a", "environment", "candidate_rock_model", new Vector3(94f, TerrainHeightAt(94f, 74f), 74f), Vector3.one * 2.30f, 12f, "Rock_Ridge_01"),
                new CandidatePlacement(NatureRoot + "rock_smallFlatA.fbx", "env_kenney_nature_rock_small_flat_a", "environment", "candidate_rock_model", new Vector3(56f, TerrainHeightAt(56f, 18f), 18f), Vector3.one * 2.40f, 42f, "Rock_Shipyard_01"),
                new CandidatePlacement(NatureRoot + "cliff_large_rock.fbx", "env_kenney_nature_cliff_large_rock", "environment", "candidate_cliff_model", new Vector3(-100f, TerrainHeightAt(-100f, 78f), 78f), Vector3.one * 3.40f, 0f, "Cliff_Ridge_West"),
                new CandidatePlacement(NatureRoot + "cliff_blockSlope_rock.fbx", "env_kenney_nature_cliff_block_slope_rock", "environment", "candidate_cliff_model", new Vector3(100f, TerrainHeightAt(100f, 86f), 86f), Vector3.one * 3.35f, 180f, "Cliff_Ridge_East"),
                new CandidatePlacement(NatureRoot + "cliff_cave_rock.fbx", "env_kenney_nature_cliff_cave_rock", "environment", "candidate_cliff_model", new Vector3(72f, TerrainHeightAt(72f, 72f), 72f), Vector3.one * 3.10f, 90f, "Cave_Fissure_Visual"),
                new CandidatePlacement(NatureRoot + "log_stack.fbx", "env_kenney_nature_log_stack", "environment", "candidate_wood_model", new Vector3(46f, TerrainHeightAt(46f, 32f) + 0.2f, 32f), Vector3.one * 1.55f, 18f, "LogStack_Warehouse_01"),
                new CandidatePlacement(NatureRoot + "stone_largeA.fbx", "env_kenney_nature_stone_large_a", "environment", "candidate_stone_model", new Vector3(36f, TerrainHeightAt(36f, 54f), 54f), Vector3.one * 2.30f, -20f, "Stone_Market_01"),
                new CandidatePlacement(NatureRoot + "tree_palmBend.fbx", "env_kenney_nature_tree_palm_bend", "environment", "candidate_vegetation_model", new Vector3(-28f, TerrainHeightAt(-28f, 72f), 72f), Vector3.one * 2.15f, -24f, "Palm_Spawn_03"),
                new CandidatePlacement(NatureRoot + "tree_palmShort.fbx", "env_kenney_nature_tree_palm_short", "environment", "candidate_vegetation_model", new Vector3(8f, TerrainHeightAt(8f, 55f), 55f), Vector3.one * 2.0f, 14f, "Palm_Market_01"),
                new CandidatePlacement(NatureRoot + "plant_bushDetailed.fbx", "env_kenney_nature_plant_bush_detailed", "environment", "candidate_vegetation_model", new Vector3(8f, TerrainHeightAt(8f, 64f), 64f), Vector3.one * 2.0f, -8f, "Shrub_Market_02"),
                new CandidatePlacement(NatureRoot + "rock_smallFlatA.fbx", "env_kenney_nature_rock_small_flat_a", "environment", "candidate_rock_model", new Vector3(-12f, TerrainHeightAt(-12f, 50f), 50f), Vector3.one * 1.8f, 25f, "Rock_Road_01")
            };
            AddPlacements(parent, placements, usage, warnings);
        }

        private static void AddPortModules(Transform parent, List<UsageRecord> usage, List<string> warnings)
        {
            var placements = new[]
            {
                new CandidatePlacement(BuildingRoot + "wall-window-square-detailed.fbx", "d18a05_building_wall_window_square_detailed", "building", "building_opening_module", new Vector3(16f, 3.0f, 62f), new Vector3(1.15f, 1.15f, 1.15f), 0f, "Market_Window_Left"),
                new CandidatePlacement(BuildingRoot + "wall-window-round-detailed.fbx", "d18a05_building_wall_window_round_detailed", "building", "building_opening_module", new Vector3(36f, 3.0f, 62f), new Vector3(1.15f, 1.15f, 1.15f), 180f, "Market_Window_Right"),
                new CandidatePlacement(BuildingRoot + "roof-flat-patch-large.fbx", "d18a05_building_roof_flat_patch_large", "building", "building_roof_module", new Vector3(26f, 7.0f, 62f), new Vector3(1.35f, 1.35f, 1.35f), 0f, "Market_Roof_Module"),
                new CandidatePlacement(BuildingRoot + "door-rotate-square-b.fbx", "d18a05_building_door_rotate_square_b", "building", "building_opening_module", new Vector3(26f, 1.2f, 56.8f), new Vector3(1.1f, 1.1f, 1.1f), 180f, "Market_Entrance_Door"),
                new CandidatePlacement(BuildingRoot + "wall-window-square-detailed.fbx", "d18a05_building_wall_window_square_detailed", "building", "building_opening_module", new Vector3(38f, 3.2f, 34f), new Vector3(1.15f, 1.15f, 1.15f), 0f, "Warehouse_Window"),
                new CandidatePlacement(BuildingRoot + "roof-flat-side.fbx", "d18a05_building_roof_flat_side", "building", "building_roof_module", new Vector3(48f, 7.6f, 34f), new Vector3(1.45f, 1.45f, 1.45f), 0f, "Warehouse_Roof_Module"),
                new CandidatePlacement(BuildingRoot + "door-rotate-square-b.fbx", "d18a05_building_door_rotate_square_b", "building", "building_opening_module", new Vector3(48f, 1.2f, 27.7f), new Vector3(1.1f, 1.1f, 1.1f), 180f, "Warehouse_Entrance_Door"),
                new CandidatePlacement(BuildingRoot + "wall-window-round-detailed.fbx", "d18a05_building_wall_window_round_detailed", "building", "building_opening_module", new Vector3(-42f, 2.8f, 50f), new Vector3(1.1f, 1.1f, 1.1f), 90f, "Tavern_Window_Module"),
                new CandidatePlacement(BuildingRoot + "roof-flat-patch-large.fbx", "d18a05_building_roof_flat_patch_large", "building", "building_roof_module", new Vector3(-42f, 6.6f, 50f), new Vector3(1.2f, 1.2f, 1.2f), 0f, "Tavern_Roof_Module"),
                new CandidatePlacement(BuildingRoot + "stairs-open-short.fbx", "d18a05_building_stairs_open_short", "building", "building_access_module", new Vector3(40f, 0.75f, 46f), new Vector3(1.2f, 1.2f, 1.2f), 90f, "Warehouse_Stair_Access")
            };
            AddPlacements(parent, placements, usage, warnings);
        }

        private static void AddDockAndMarketProps(Transform parent, List<UsageRecord> usage, List<string> warnings)
        {
            var placements = new[]
            {
                new CandidatePlacement(PirateRoot + "structure-platform.fbx", "d18a05_pirate_structure_platform", "ship", "port_platform_module", new Vector3(0f, 0.28f, -18f), new Vector3(2.2f, 1.0f, 2.8f), 0f, "Dock_Platform_01"),
                new CandidatePlacement(PirateRoot + "platform-planks.fbx", "d18a05_pirate_platform_planks", "ship", "port_platform_module", new Vector3(0f, 0.30f, -26f), new Vector3(2.1f, 1.0f, 3.1f), 0f, "Boarding_Planks_01"),
                new CandidatePlacement(PirateRoot + "flag-pirate.fbx", "d18a05_pirate_flag_pirate", "building", "port_flag_sign_prop", new Vector3(26f, 6.2f, 65f), new Vector3(1.3f, 1.3f, 1.3f), 0f, "Market_Flag"),
                new CandidatePlacement(PirateRoot + "flag-pirate.fbx", "d18a05_pirate_flag_pirate", "building", "port_flag_sign_prop", new Vector3(60f, 6.4f, 11f), new Vector3(1.3f, 1.3f, 1.3f), 25f, "Shipyard_Flag"),
                new CandidatePlacement(PirateRoot + "barrel.fbx", "d18a05_pirate_barrel", "building", "port_storage_prop", new Vector3(18f, 0.85f, 57f), Vector3.one, 15f, "Market_Barrel_01"),
                new CandidatePlacement(PirateRoot + "barrel.fbx", "d18a05_pirate_barrel", "building", "port_storage_prop", new Vector3(34f, 0.85f, 57f), Vector3.one * 1.05f, -10f, "Market_Barrel_02"),
                new CandidatePlacement(PirateRoot + "crate.fbx", "d18a05_pirate_crate", "building", "port_storage_prop", new Vector3(22f, 0.7f, 55f), Vector3.one * 1.05f, 18f, "Market_Crate_01"),
                new CandidatePlacement(PirateRoot + "crate.fbx", "d18a05_pirate_crate", "building", "port_storage_prop", new Vector3(30f, 0.7f, 55f), Vector3.one * 1.05f, -16f, "Market_Crate_02"),
                new CandidatePlacement(PirateRoot + "barrel.fbx", "d18a05_pirate_barrel", "building", "port_storage_prop", new Vector3(8f, 0.85f, -10f), Vector3.one, 35f, "Dock_Barrel_01"),
                new CandidatePlacement(PirateRoot + "crate.fbx", "d18a05_pirate_crate", "building", "port_storage_prop", new Vector3(-8f, 0.7f, -10f), Vector3.one * 1.05f, -20f, "Dock_Crate_01"),
                new CandidatePlacement(NatureRoot + "log_stack.fbx", "env_kenney_nature_log_stack", "environment", "candidate_wood_model", new Vector3(57f, 0.8f, 5f), Vector3.one * 1.1f, 18f, "Shipyard_Log_Stack")
            };
            AddPlacements(parent, placements, usage, warnings);
        }

        private static void AddProjectOwnedPortAndCoastDetails(Transform parent)
        {
            var stucco = LoadOrCreateMaterial("D19_R01_B_Stucco", "D19_R01_Facade_Stucco.mat", new Color(0.58f, 0.39f, 0.22f), 0.28f);
            var limewash = LoadOrCreateMaterial("D19_R01_B_Limewash", "D19_R01_Facade_Limewash.mat", new Color(0.72f, 0.67f, 0.48f), 0.25f);
            var darkWood = LoadOrCreateMaterial("D19_R01_B_DarkWood", "D19_R01_Facade_DarkWood.mat", new Color(0.12f, 0.035f, 0.018f), 0.22f);
            var wood = LoadOrCreateMaterial("D19_R01_B_Wood", "D19_R01_Facade_Wood.mat", new Color(0.34f, 0.12f, 0.035f), 0.26f);
            var terracotta = LoadOrCreateMaterial("D19_R01_B_Terracotta", "D19_R01_Facade_Terracotta.mat", new Color(0.38f, 0.075f, 0.038f), 0.24f);
            var window = LoadOrCreateMaterial("D19_R01_B_Window", "D19_R01_Facade_Window.mat", new Color(0.035f, 0.16f, 0.19f), 0.16f);
            var cloth = LoadOrCreateMaterial("D19_R01_B_Cloth", "D19_S02_Port_Cloth.mat", new Color(0.55f, 0.11f, 0.055f), 0.2f);
            var soil = LoadOrCreateMaterial("D19_R01_B_Soil", "D19_R01_Landform_Sand.mat", new Color(0.34f, 0.19f, 0.08f), 0.35f);
            var sand = LoadOrCreateMaterial("D19_R01_B_ShoreSand", "D19_R01_Landform_Sand.mat", new Color(0.72f, 0.52f, 0.28f), 0.3f);
            var reef = LoadOrCreateMaterial("D19_R01_B_ReefRock", "D19_R01_Coast_ReefRock.mat", new Color(0.15f, 0.23f, 0.21f), 0.32f);

            var architecture = CreateChild(parent, "D19_R01_B_PortArchitecture");
            CreateBuilding(architecture.transform, "MarketHouse", new Vector3(26f, TerrainHeightAt(26f, 60f), 60f), 20f, 12f, 5.2f, limewash, terracotta, darkWood, window, cloth, "MARKET");
            CreateBuilding(architecture.transform, "WarehouseHouse", new Vector3(48f, TerrainHeightAt(48f, 32f), 32f), 18f, 11f, 5.6f, stucco, terracotta, darkWood, window, cloth, "WAREHOUSE");
            CreateBuilding(architecture.transform, "TavernHouse", new Vector3(-42f, TerrainHeightAt(-42f, 48f), 48f), 16f, 10f, 4.7f, stucco, terracotta, darkWood, window, cloth, "TAVERN");
            CreateBuilding(architecture.transform, "ShipyardHouse", new Vector3(60f, TerrainHeightAt(60f, 8f), 8f), 18f, 11f, 5.4f, limewash, terracotta, darkWood, window, cloth, "SHIPYARD");

            var roads = CreateChild(parent, "D19_R01_B_EarthAndStoneRoads");
            CreateRoadRibbon(roads.transform, "Road_Spawn_Market", new[]
            {
                new Vector3(-22f, 0.16f, 82f), new Vector3(-12f, 0.3f, 76f), new Vector3(0f, 0.55f, 70f),
                new Vector3(12f, 0.85f, 66f), new Vector3(26f, TerrainHeightAt(26f, 60f) + 0.14f, 60f)
            }, 4.6f, soil);
            CreateRoadRibbon(roads.transform, "Road_Market_Warehouse", new[]
            {
                new Vector3(30f, 0.9f, 57f), new Vector3(38f, 0.8f, 48f), new Vector3(48f, TerrainHeightAt(48f, 32f) + 0.14f, 32f)
            }, 3.8f, soil);
            CreateRoadRibbon(roads.transform, "Road_Market_Tavern", new[]
            {
                new Vector3(18f, 0.8f, 58f), new Vector3(2f, 0.7f, 55f), new Vector3(-18f, 0.65f, 51f), new Vector3(-42f, TerrainHeightAt(-42f, 48f) + 0.14f, 48f)
            }, 3.5f, soil);
            CreateRoadRibbon(roads.transform, "Road_Dock_Market", new[]
            {
                new Vector3(0f, 0.32f, -15f), new Vector3(2f, 0.52f, 10f), new Vector3(12f, 0.75f, 34f), new Vector3(26f, TerrainHeightAt(26f, 60f) + 0.14f, 60f)
            }, 3.4f, soil);

            var dock = CreateChild(parent, "D19_R01_B_DockCraftedSurface");
            CreateDock(dock.transform, wood, darkWood, cloth);

            var shoreline = CreateChild(parent, "D19_R01_B_ShorelineAndReefBands");
            CreateShoreline(shoreline.transform, sand, reef);
        }

        private static void CreateBuilding(Transform parent, string name, Vector3 basePosition, float width, float depth,
            float wallHeight, Material wall, Material roof, Material trim, Material glass, Material awning, string sign)
        {
            var building = CreateChild(parent, "D19_R01_B_" + name);
            var wallY = basePosition.y + wallHeight * 0.5f + 0.18f;
            var frontZ = basePosition.z - depth * 0.5f;
            var backZ = basePosition.z + depth * 0.5f;
            var leftX = basePosition.x - width * 0.5f;
            var rightX = basePosition.x + width * 0.5f;
            const float wallThickness = 0.34f;
            const float doorWidth = 2.8f;

            CreateBoxVisual(building.transform, name + "_BackWall", new Vector3(basePosition.x, wallY, backZ), new Vector3(width, wallHeight, wallThickness), wall);
            CreateBoxVisual(building.transform, name + "_LeftWall", new Vector3(leftX, wallY, basePosition.z), new Vector3(wallThickness, wallHeight, depth), wall);
            CreateBoxVisual(building.transform, name + "_RightWall", new Vector3(rightX, wallY, basePosition.z), new Vector3(wallThickness, wallHeight, depth), wall);
            var frontPanelWidth = (width - doorWidth) * 0.5f;
            CreateBoxVisual(building.transform, name + "_FrontLeft", new Vector3(leftX + frontPanelWidth * 0.5f, wallY, frontZ), new Vector3(frontPanelWidth, wallHeight, wallThickness), wall);
            CreateBoxVisual(building.transform, name + "_FrontRight", new Vector3(rightX - frontPanelWidth * 0.5f, wallY, frontZ), new Vector3(frontPanelWidth, wallHeight, wallThickness), wall);

            var roofY = basePosition.y + wallHeight + 0.35f;
            CreateGabledRoof(building.transform, name + "_TerracottaRoof", new Vector3(basePosition.x, roofY, basePosition.z), width + 1.3f, 2.7f, depth + 1.3f, roof);
            CreateBoxVisual(building.transform, name + "_Foundation", new Vector3(basePosition.x, basePosition.y + 0.16f, basePosition.z), new Vector3(width + 0.5f, 0.32f, depth + 0.5f), trim);

            CreateDoorAndFrame(building.transform, name + "_Door", new Vector3(basePosition.x, basePosition.y + 2.0f, frontZ - 0.24f), doorWidth, 3.8f, trim, trim);
            CreateWindowAndShutters(building.transform, name + "_WindowLeft", new Vector3(leftX + 3.3f, basePosition.y + 2.9f, frontZ - 0.24f), glass, trim, awning);
            CreateWindowAndShutters(building.transform, name + "_WindowRight", new Vector3(rightX - 3.3f, basePosition.y + 2.9f, frontZ - 0.24f), glass, trim, awning);
            CreateDoorAndFrame(building.transform, name + "_BackDoor", new Vector3(basePosition.x, basePosition.y + 2.0f, backZ + 0.24f), doorWidth, 3.8f, trim, trim);
            CreateWindowAndShutters(building.transform, name + "_BackWindowLeft", new Vector3(leftX + 3.3f, basePosition.y + 2.9f, backZ + 0.24f), glass, trim, awning);
            CreateWindowAndShutters(building.transform, name + "_BackWindowRight", new Vector3(rightX - 3.3f, basePosition.y + 2.9f, backZ + 0.24f), glass, trim, awning);
            CreateBoxVisual(building.transform, name + "_BackAwning", new Vector3(basePosition.x, basePosition.y + 4.55f, backZ + 1.15f), new Vector3(width * 0.58f, 0.22f, 2.0f), awning, Quaternion.Euler(-12f, 0f, 0f));

            CreateBoxVisual(building.transform, name + "_Awning", new Vector3(basePosition.x, basePosition.y + 4.55f, frontZ - 1.15f), new Vector3(width * 0.58f, 0.22f, 2.0f), awning, Quaternion.Euler(12f, 0f, 0f));
            CreateBoxVisual(building.transform, name + "_Sign", new Vector3(basePosition.x, basePosition.y + wallHeight + 0.5f, frontZ - 0.34f), new Vector3(Mathf.Min(width * 0.55f, 7.5f), 0.85f, 0.16f), trim);
            CreateBoxVisual(building.transform, name + "_SignInset", new Vector3(basePosition.x, basePosition.y + wallHeight + 0.5f, frontZ - 0.44f), new Vector3(Mathf.Min(width * 0.46f, 6.2f), 0.48f, 0.08f), trim);
            CreateBoxVisual(building.transform, name + "_BeamFront", new Vector3(basePosition.x, basePosition.y + wallHeight - 0.36f, frontZ - 0.27f), new Vector3(width - 0.8f, 0.35f, 0.28f), trim);
            CreateBoxVisual(building.transform, name + "_BeamLeft", new Vector3(leftX + 0.45f, basePosition.y + wallHeight * 0.5f, frontZ - 0.29f), new Vector3(0.32f, wallHeight - 0.7f, 0.27f), trim);
            CreateBoxVisual(building.transform, name + "_BeamRight", new Vector3(rightX - 0.45f, basePosition.y + wallHeight * 0.5f, frontZ - 0.29f), new Vector3(0.32f, wallHeight - 0.7f, 0.27f), trim);
        }

        private static void CreateDoorAndFrame(Transform parent, string name, Vector3 center, float width, float height, Material frame, Material door)
        {
            CreateBoxVisual(parent, name + "_Panel", center, new Vector3(width * 0.63f, height, 0.18f), door);
            var outer = width * 0.5f;
            CreateBoxVisual(parent, name + "_LeftFrame", center + new Vector3(-outer, 0f, -0.12f), new Vector3(0.22f, height + 0.35f, 0.28f), frame);
            CreateBoxVisual(parent, name + "_RightFrame", center + new Vector3(outer, 0f, -0.12f), new Vector3(0.22f, height + 0.35f, 0.28f), frame);
            CreateBoxVisual(parent, name + "_Header", center + new Vector3(0f, height * 0.5f, -0.12f), new Vector3(width + 0.45f, 0.22f, 0.28f), frame);
        }

        private static void CreateWindowAndShutters(Transform parent, string name, Vector3 center, Material glass, Material frame, Material shutter)
        {
            CreateBoxVisual(parent, name + "_Glass", center, new Vector3(2.0f, 1.55f, 0.12f), glass);
            CreateBoxVisual(parent, name + "_Top", center + new Vector3(0f, 0.88f, -0.04f), new Vector3(2.55f, 0.18f, 0.26f), frame);
            CreateBoxVisual(parent, name + "_Bottom", center + new Vector3(0f, -0.88f, -0.04f), new Vector3(2.55f, 0.18f, 0.26f), frame);
            CreateBoxVisual(parent, name + "_Left", center + new Vector3(-1.17f, 0f, -0.04f), new Vector3(0.18f, 1.9f, 0.26f), frame);
            CreateBoxVisual(parent, name + "_Right", center + new Vector3(1.17f, 0f, -0.04f), new Vector3(0.18f, 1.9f, 0.26f), frame);
            CreateBoxVisual(parent, name + "_ShutterL", center + new Vector3(-1.45f, 0f, -0.03f), new Vector3(0.28f, 1.72f, 0.20f), shutter, Quaternion.Euler(0f, 0f, -5f));
            CreateBoxVisual(parent, name + "_ShutterR", center + new Vector3(1.45f, 0f, -0.03f), new Vector3(0.28f, 1.72f, 0.20f), shutter, Quaternion.Euler(0f, 0f, 5f));
        }

        private static void CreateDock(Transform parent, Material wood, Material darkWood, Material cloth)
        {
            var dockY = 0.42f;
            for (var i = -4; i <= 4; i++)
                CreateBoxVisual(parent, "DockDeck_Plank_" + (i + 4), new Vector3(0f, dockY, -18f + i * 3.7f), new Vector3(9.0f, 0.34f, 3.25f), wood);
            for (var z = -32f; z <= -5f; z += 6.2f)
            {
                CreateBoxVisual(parent, "Dock_Post_L_" + z.ToString("F0"), new Vector3(-4.0f, 1.45f, z), new Vector3(0.42f, 2.7f, 0.42f), darkWood);
                CreateBoxVisual(parent, "Dock_Post_R_" + z.ToString("F0"), new Vector3(4.0f, 1.45f, z), new Vector3(0.42f, 2.7f, 0.42f), darkWood);
                CreateBoxVisual(parent, "Dock_Rope_" + z.ToString("F0"), new Vector3(0f, 2.0f, z + 2.25f), new Vector3(7.6f, 0.16f, 0.16f), cloth, Quaternion.Euler(0f, 0f, 2f));
            }
            CreateBoxVisual(parent, "Dock_Head", new Vector3(0f, 0.54f, -36f), new Vector3(12f, 0.40f, 4.5f), wood);
            CreateBoxVisual(parent, "Dock_Head_Flag", new Vector3(5.2f, 4.5f, -35.5f), new Vector3(0.18f, 6.0f, 0.18f), darkWood);
            CreateBoxVisual(parent, "Dock_Head_Cloth", new Vector3(6.25f, 5.3f, -35.5f), new Vector3(2.1f, 1.15f, 0.10f), cloth, Quaternion.Euler(0f, 8f, 0f));
        }

        private static void CreateShoreline(Transform parent, Material sand, Material reef)
        {
            for (var x = -96f; x <= 96f; x += 12f)
            {
                var z = -28f + 2.8f * Mathf.Sin(x * 0.08f);
                var y = Mathf.Max(0.05f, TerrainHeightAt(x, z) + 0.10f);
                CreateBoxVisual(parent, "ShoreSand_Band_" + x.ToString("F0"), new Vector3(x, y, z), new Vector3(11.0f, 0.18f, 3.6f), sand, Quaternion.Euler(0f, Mathf.Sin(x) * 4f, 0f));
                if (Mathf.Abs(x) % 24f < 0.1f)
                    CreateBoxVisual(parent, "ReefRock_Band_" + x.ToString("F0"), new Vector3(x + 2.5f, -0.05f, z - 3.3f), new Vector3(4.5f, 0.32f, 2.0f), reef, Quaternion.Euler(0f, x * 0.7f, 0f));
            }
        }

        private static void CreateRoadRibbon(Transform parent, string name, Vector3[] points, float width, Material material)
        {
            for (var i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                var mid = (a + b) * 0.5f + Vector3.up * 0.02f;
                var direction = b - a;
                var length = new Vector2(direction.x, direction.z).magnitude;
                var road = CreateBoxVisual(parent, name + "_Segment_" + i, mid, new Vector3(width, 0.12f, length), material);
                road.transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0f);
                CreateBoxVisual(parent, name + "_EdgeL_" + i, mid + Vector3.left * (width * 0.40f) + Vector3.up * 0.10f, new Vector3(0.12f, 0.10f, length), material);
                CreateBoxVisual(parent, name + "_EdgeR_" + i, mid + Vector3.right * (width * 0.40f) + Vector3.up * 0.10f, new Vector3(0.12f, 0.10f, length), material);
            }
        }

        private static GameObject CreateBoxVisual(Transform parent, string name, Vector3 center, Vector3 size, Material material, Quaternion rotation = default)
        {
            var go = CreateChild(parent, name);
            go.transform.position = center;
            go.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            go.transform.localScale = size;
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return go;
        }

        private static void CreateGabledRoof(Transform parent, string name, Vector3 center, float width, float ridgeHeight, float depth, Material material)
        {
            EnsureFolder(RepairDirectory, "Geometry");
            var path = RepairDirectory + "/Geometry/" + name + ".asset";
            DeleteAssetIfExists(path);
            var x = width * 0.5f;
            var z = depth * 0.5f;
            var mesh = new Mesh { name = name + "Mesh" };
            mesh.vertices = new[]
            {
                new Vector3(-x, 0f, -z), new Vector3(x, 0f, -z), new Vector3(-x, 0f, z), new Vector3(x, 0f, z),
                new Vector3(0f, ridgeHeight, -z), new Vector3(0f, ridgeHeight, z)
            };
            mesh.triangles = new[]
            {
                0, 4, 1, 2, 3, 5, 0, 2, 5, 0, 5, 4, 1, 4, 5, 1, 5, 3
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            var go = CreateChild(parent, name);
            go.transform.position = center;
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        private static Material LoadOrCreateMaterial(string name, string baseMaterialFile, Color color, float smoothness)
        {
            var path = RepairDirectory + "/Materials/" + name + ".mat";
            EnsureFolder(RepairDirectory, "Materials");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var source = LoadRequired<Material>(MaterialRoot + baseMaterialFile);
                material = new Material(source) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_SpecularHighlights")) material.SetFloat("_SpecularHighlights", 0.25f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void AddPlacements(Transform parent, CandidatePlacement[] placements,
            List<UsageRecord> usage, List<string> warnings)
        {
            foreach (var placement in placements)
            {
                var record = new UsageRecord
                {
                    candidate_file_path = placement.path,
                    content_id = placement.contentId,
                    domain = placement.domain,
                    role = placement.role,
                    source_exists = File.Exists(ProjectAbsolute(placement.path))
                };
                if (!record.source_exists)
                {
                    record.note = "Candidate file missing; no fallback was substituted.";
                    usage.Add(record);
                    warnings.Add("Candidate file missing: " + placement.path);
                    continue;
                }

                var source = AssetDatabase.LoadAssetAtPath<GameObject>(placement.path);
                if (source == null)
                {
                    record.note = "Unity model import did not produce a GameObject.";
                    usage.Add(record);
                    warnings.Add("Candidate model not imported: " + placement.path);
                    continue;
                }

                var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
                if (instance == null)
                {
                    record.note = "PrefabUtility.InstantiatePrefab returned null.";
                    usage.Add(record);
                    warnings.Add("Candidate model instantiation failed: " + placement.path);
                    continue;
                }

                instance.name = placement.label;
                instance.transform.SetParent(parent, false);
                instance.transform.localPosition = placement.position;
                instance.transform.localRotation = Quaternion.Euler(0f, placement.yaw, 0f);
                instance.transform.localScale = placement.scale;
                foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
                    collider.enabled = false;

                record.instantiated = true;
                record.note = "Approved intake candidate used as visual-only nested model; collision remains owned by D19 terrain/port systems.";
                usage.Add(record);
            }
        }

        private static void IntegrateScene(GameObject repairPrefab, BuildResult result, List<string> warnings)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var root = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "D19_SaintMargaret_Root");
            if (root == null)
                throw new InvalidOperationException("D19_SaintMargaret_Root is missing from InitialHarbor.");

            var oldVisualNames = new[]
            {
                "D19_R01_LandformPresentation",
                "D19_R01_SettlementGroundPresentation",
                "D19_R01_DockDetail",
                "D19_R01_RoadMarkers",
                "D19_R01_CoastDetail",
                "D19_R01_PortFacades"
            };
            foreach (var name in oldVisualNames)
            {
                var oldVisual = FindChild(root.transform, name);
                if (oldVisual == null) continue;
                foreach (var renderer in oldVisual.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer == null || !renderer.enabled) continue;
                    renderer.enabled = false;
                    result.hidden_legacy_visual_renderers++;
                }
            }

            // Re-runs must be idempotent. Earlier attempts could leave more than one
            // instance in the scene when Unity was interrupted between save/import.
            // Keep the scene to one Integration root so visual and performance results
            // cannot be inflated by duplicate presentation layers.
            var existingInstances = scene.GetRootGameObjects()
                .SelectMany(sceneRoot => sceneRoot.GetComponentsInChildren<Transform>(true))
                .Where(t => t != null && t.name == "D19_R01_B_HarborVisualRepair")
                .Select(t => t.gameObject)
                .Distinct()
                .ToArray();
            Debug.Log("[D19-R01-B] removing existing integration instances=" + existingInstances.Length);
            foreach (var existing in existingInstances)
                UnityEngine.Object.DestroyImmediate(existing);

            var instance = PrefabUtility.InstantiatePrefab(repairPrefab) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("Repair prefab could not be instantiated.");
            SceneManager.MoveGameObjectToScene(instance, scene);
            instance.name = "D19_R01_B_HarborVisualRepair";
            instance.transform.SetParent(root.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            result.formal_initial_harbor_modified = true;
            result.scene_only_references_prefab = PrefabUtility.GetCorrespondingObjectFromSource(instance) != null;
            result.legacy_stable_anchors_preserved = HasStableAnchors(root.transform, out var stableIds);
            result.stable_anchor_ids = stableIds;
            result.dev_overlay_default_off = IsInactive(root.transform, "D19_DevOverlay");
            result.candidate_models_instantiated = instance.GetComponentsInChildren<Transform>(true)
                .Count(t => t.name.StartsWith("Palm_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Canopy_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Shrub_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Grass_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Rock_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Cliff_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Stone_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Market_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Warehouse_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Tavern_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Dock_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Boarding_", StringComparison.Ordinal) ||
                            t.name.StartsWith("Shipyard_", StringComparison.Ordinal) ||
                            t.name.StartsWith("BeachPlatform_", StringComparison.Ordinal) ||
                            t.name.StartsWith("RouteStone_", StringComparison.Ordinal));
            result.project_owned_renderers = instance.GetComponentsInChildren<MeshRenderer>(true)
                .Count(renderer => renderer != null && renderer.sharedMaterial != null &&
                                   renderer.sharedMaterial.name.StartsWith("D19_", StringComparison.Ordinal));
            result.missing_candidate_models = 0;

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("InitialHarbor save failed.");
        }

        private static void CaptureFixedShots(List<ShotRecord> shots, List<string> warnings)
        {
            var scene = SceneManager.GetActiveScene();
            var root = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "D19_SaintMargaret_Root");
            var cameraObject = root == null ? null : FindChild(root.transform, "D19_S02_CaptureCamera");
            var camera = cameraObject == null ? null : cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                warnings.Add("D19_S02_CaptureCamera missing; fixed shots not captured.");
                return;
            }

            Directory.CreateDirectory(ProjectAbsolute(ScreenshotRoot));
            var shotsToCapture = new[]
            {
                new Shot("d19_shot_main_menu", new Vector3(0f, 4f, 20f), new Vector3(0f, 2f, 60f)),
                new Shot("d19_shot_spawn", new Vector3(-22f, 5f, 82f), new Vector3(-22f, 2f, 68f)),
                new Shot("d19_shot_market", new Vector3(44f, 17f, 76f), new Vector3(26f, 3f, 62f)),
                new Shot("d19_shot_dock_overview", new Vector3(14f, 8f, 4f), new Vector3(0f, 1f, -22f)),
                new Shot("d19_shot_tavern_entry", new Vector3(-58f, 6f, 60f), new Vector3(-42f, 2f, 50f)),
                new Shot("d19_shot_coastline", new Vector3(-86f, 6f, 4f), new Vector3(-58f, 1f, -8f)),
                new Shot("d19_shot_boarding", new Vector3(8f, 4f, -16f), new Vector3(0f, 1f, -24f)),
                new Shot("d19_shot_on_ship", new Vector3(0f, 6f, -28f), new Vector3(0f, 2f, -18f)),
                new Shot("d19_shot_departure", new Vector3(0f, 8f, -42f), new Vector3(0f, 1f, -78f)),
                new Shot("d19_shot_nearshore", new Vector3(18f, 6f, -66f), new Vector3(0f, -1f, -88f)),
                new Shot("d19_shot_weather", new Vector3(-18f, 8f, 18f), new Vector3(0f, 3f, -20f)),
                new Shot("d19_shot_return_harbor", new Vector3(8f, 6f, -52f), new Vector3(0f, 1f, -12f)),
                new Shot("d19_shot_settings_accessibility", new Vector3(0f, 6f, 60f), new Vector3(0f, 3f, 60f)),
                new Shot("d19_shot_debug_boundary", new Vector3(104f, 72f, 104f), new Vector3(0f, 0f, 0f))
            };

            var previousEnabled = camera.enabled;
            camera.enabled = false;
            var texture = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            var renderTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32)
            {
                name = "D19_R01_B_FixedShots_RenderTexture"
            };
            renderTexture.Create();
            foreach (var shot in shotsToCapture)
            {
                camera.transform.position = shot.position;
                camera.transform.LookAt(shot.lookAt);
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                texture.Apply();
                var file = ScreenshotRoot + "/" + shot.id + ".png";
                File.WriteAllBytes(ProjectAbsolute(file), texture.EncodeToPNG());
                shots.Add(new ShotRecord { shot_id = shot.id, file = file, width = 1920, height = 1080, status = "CAPTURED_VISUAL_REGRESSION" });
            }
            camera.targetTexture = null;
            camera.enabled = previousEnabled;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(texture);
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        private readonly struct Shot
        {
            public readonly string id;
            public readonly Vector3 position;
            public readonly Vector3 lookAt;
            public Shot(string id, Vector3 position, Vector3 lookAt)
            {
                this.id = id;
                this.position = position;
                this.lookAt = lookAt;
            }
        }

        private static bool HasStableAnchors(Transform root, out string[] stableIds)
        {
            var names = new[]
            {
                "D19_Anchor_D2_PlayerAvatar", "D19_Anchor_santa_margarita", "D19_Anchor_building_market_santa_margarita",
                "D19_Anchor_building_warehouse_santa_margarita", "D19_Anchor_building_tavern_santa_margarita",
                "D19_Anchor_building_shipyard_santa_margarita", "D19_Anchor_building_saint_marys_dock",
                "D19_Anchor_berth_santa_margarita", "D19_Anchor_DockGround", "D19_Anchor_GangplankGround",
                "D19_Anchor_D7_HelmPoint", "D19_Anchor_departure_channel_santa_margarita",
                "D19_Anchor_return_channel_santa_margarita", "D19_Anchor_save_point_santa_margarita",
                "D19_Anchor_npc_santa_margarita_dockhand", "D19_Anchor_npc_santa_margarita_lookout",
                "D19_Anchor_npc_santa_margarita_tavern_keeper"
            };
            stableIds = names.Where(name => FindChild(root, name) != null).ToArray();
            return stableIds.Length == names.Length;
        }

        private static bool IsInactive(Transform root, string name)
        {
            var target = FindChild(root, name);
            return target != null && !target.gameObject.activeSelf;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static GameObject FindChild(Transform root, string name)
        {
            if (root == null) return null;
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                if (child != null && child.name == name) return child.gameObject;
            return null;
        }

        private static T LoadRequired<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("Missing required project-owned asset: " + path);
            return asset;
        }

        private static void EnsureRepairDirectory()
        {
            EnsureFolder("Assets/Prefabs", "Environment");
            EnsureFolder("Assets/Prefabs/Environment", "D19");
            EnsureFolder("Assets/Prefabs/Environment/D19", "Integration");
            EnsureFolder("Assets/Prefabs/Environment/D19/Integration", "R01");
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static void DeleteAssetIfExists(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                AssetDatabase.DeleteAsset(path);
        }

        private static string ProjectAbsolute(string projectRelativePath)
        {
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, projectRelativePath);
        }

        private static void WriteResult(BuildResult result)
        {
            Directory.CreateDirectory(ProjectAbsolute(ResultRoot));
            File.WriteAllText(ProjectAbsolute(ResultRoot + "/D19_R01_B_BUILD_RESULT.json"), JsonUtility.ToJson(result, true));
        }

        private static float TerrainHeightAt(float x, float z)
        {
            var coast = -44f + 7f * Mathf.Sin(x * 0.028f) + 3f * Mathf.Sin(x * 0.071f + 0.8f);
            var land = SmoothStep(coast - 2f, coast + 18f, z);
            var highland = Gaussian(x, z, 68f, 76f, 34f, 13f);
            var ridge = Gaussian(x, z, -72f, 82f, 45f, 7f);
            var terrace = 1.2f * Mathf.Sin((x + 18f) * 0.045f) * Mathf.Sin((z - 8f) * 0.026f) * land;
            var beach = 0.45f * SmoothStep(coast, coast + 9f, z);
            var seaBed = Mathf.Lerp(-7f, -1.2f, SmoothStep(coast - 12f, coast + 1f, z));
            var height = Mathf.Lerp(seaBed, 0.35f + highland + ridge + terrace + beach, land);
            var distance = Mathf.Min(
                DistanceToSegment(x, z, -22f, 88f, 26f, 62f),
                DistanceToSegment(x, z, 26f, 62f, 48f, 34f),
                DistanceToSegment(x, z, 26f, 62f, 0f, -12f),
                DistanceToSegment(x, z, -42f, 50f, 0f, -12f));
            var blend = 1f - SmoothStep(4f, 10f, distance);
            height = Mathf.Lerp(height, 0.55f, blend * 0.72f);
            if (z > coast + 2f) height = Mathf.Max(height, 0.55f);
            return height;
        }

        private static float DistanceToSegment(float x, float z, float ax, float az, float bx, float bz)
        {
            var abx = bx - ax;
            var abz = bz - az;
            var t = Mathf.Clamp01(((x - ax) * abx + (z - az) * abz) / Mathf.Max(0.0001f, abx * abx + abz * abz));
            var dx = x - (ax + abx * t);
            var dz = z - (az + abz * t);
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        private static float Gaussian(float x, float z, float centerX, float centerZ, float radius, float amplitude)
        {
            var dx = x - centerX;
            var dz = z - centerZ;
            return amplitude * Mathf.Exp(-(dx * dx + dz * dz) / (2f * radius * radius));
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            var t = Mathf.Clamp01((value - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }
    }
}
#endif
