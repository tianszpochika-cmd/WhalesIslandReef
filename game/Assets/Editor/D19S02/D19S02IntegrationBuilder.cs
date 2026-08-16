#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhalesIslandReef.Editor.D19S02
{
    /// <summary>
    /// D19-S02 唯一的正式场景集成入口。
    /// 先把 A01—A06 成果编排成一个可回滚的 Prefab，再把该 Prefab 作为唯一新表现根接入 InitialHarbor。
    /// </summary>
    public static class D19S02IntegrationBuilder
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string IntegrationDirectory = "Assets/Prefabs/Environment/D19/Integration";
        private const string IntegrationPrefabPath = IntegrationDirectory + "/D19_SaintMargaret_Root.prefab";
        private const string AnchorPrefabPath = IntegrationDirectory + "/D19_StableInteractionAnchors.prefab";
        private const string AmbiencePrefabPath = IntegrationDirectory + "/D19_AmbienceRoot.prefab";
        // R01 is intentionally separate from the historical D19-S02 evidence. The old
        // report remains PARTIAL and is never rewritten by this repair run.
        private const string ResultRoot = "tools/results/D19/D19-S02-R01";
        private const string ScreenshotRoot = ResultRoot + "/screenshots";
        private const string ArchitecturalDetailPrefabPath = IntegrationDirectory + "/D19_R01_PortAndCoastDetail.prefab";

        private static readonly string TerrainRoot = "Assets/Prefabs/Environment/D19/Terrain/";
        private static readonly string PortRoot = "Assets/Prefabs/Buildings/D19/";
        private static readonly string PortModuleRoot = "Assets/Prefabs/Environment/D19/Port/";
        private static readonly string VegetationRoot = "Assets/Prefabs/Vegetation/D19/";
        private static readonly string WaterRoot = "Assets/Prefabs/Environment/D19/Water/";
        private static readonly string LightingRoot = "Assets/Prefabs/Environment/D19/Lighting/";
        private static readonly string PropsRoot = "Assets/Prefabs/Props/D19/";
        private static readonly string VfxRoot = "Assets/Prefabs/VFX/D19/";
        private static readonly string IntegrationMaterialDirectory = IntegrationDirectory + "/Materials";

        private sealed class AnchorSpec
        {
            public string id;
            public string kind;
            public Vector3 position;

            public AnchorSpec(string id, string kind, Vector3 position)
            {
                this.id = id;
                this.kind = kind;
                this.position = position;
            }
        }

        private sealed class ShotSpec
        {
            public string id;
            public Vector3 position;
            public Vector3 lookAt;

            public ShotSpec(string id, Vector3 position, Vector3 lookAt)
            {
                this.id = id;
                this.position = position;
                this.lookAt = lookAt;
            }
        }

        [Serializable]
        private sealed class StageResult
        {
            public string stage;
            public string status;
            public string source;
            public int prefab_instances;
            public int renderers;
            public int colliders;
            public string note;
        }

        [Serializable]
        private sealed class IntegrationResult
        {
            public string task = "D19-S02-R01";
            public string status = "PASS";
            public string generated_on = "2026-08-14";
            public string scene_path = ScenePath;
            public string integration_prefab_path = IntegrationPrefabPath;
            public string anchor_prefab_path = AnchorPrefabPath;
            public string ambience_prefab_path = AmbiencePrefabPath;
            public string rollback_point_id = "d19-s02-r01-preintegration-baseline";
            public bool formal_initial_harbor_modified;
            public bool systems_modified;
            public bool contracts_modified;
            public bool registry_modified;
            public bool save_schema_modified;
            public bool external_asset_state_promoted;
            public bool dev_overlay_default_off;
            public bool legacy_compatibility_nodes_preserved;
            public int legacy_renderers_hidden;
            public int legacy_lights_hidden;
            public int integrated_prefab_instances;
            public int integrated_renderers;
            public int integrated_colliders;
            public int integrated_lod_groups;
            public int stable_anchor_count;
            public int screenshot_count;
            public int fixed_shot_contract_count;
            public int weather_profiles_checked;
            public float capture_1080p_average_ms;
            public float capture_1080p_p95_ms;
            public string performance_scope = "Editor Camera.Render at 1920x1080; not a target-device or PlayMode budget claim.";
            public string[] screenshots = new string[0];
            public string[] weather_profile_ids = new string[0];
            public string[] warnings = new string[0];
            public StageResult[] stages = new StageResult[0];
        }

        private static readonly AnchorSpec[] AnchorSpecs =
        {
            new AnchorSpec("D2_PlayerAvatar", "player_spawn", new Vector3(-22f, 0.2f, 88f)),
            new AnchorSpec("santa_margarita", "port_entry", new Vector3(0f, 0.2f, 112f)),
            new AnchorSpec("building:market_santa_margarita", "market_service", new Vector3(26f, 0.2f, 62f)),
            new AnchorSpec("market_queue_santa_margarita", "market_queue", new Vector3(12f, 0.2f, 58f)),
            new AnchorSpec("building:warehouse_santa_margarita", "warehouse_service", new Vector3(48f, 0.2f, 34f)),
            new AnchorSpec("warehouse_queue_santa_margarita", "warehouse_queue", new Vector3(40f, 0.2f, 24f)),
            new AnchorSpec("building:tavern_santa_margarita", "tavern_entry", new Vector3(-42f, 0.2f, 50f)),
            new AnchorSpec("building:shipyard_santa_margarita", "shipyard_service", new Vector3(60f, 0.2f, 8f)),
            new AnchorSpec("shipyard_repair_santa_margarita", "shipyard_repair", new Vector3(54f, 0.2f, 2f)),
            new AnchorSpec("building_saint_marys_dock", "primary_dock", new Vector3(0f, 0.2f, -12f)),
            new AnchorSpec("DockGround", "dock_collision", new Vector3(0f, 0.1f, -6f)),
            new AnchorSpec("berth_santa_margarita", "berth", new Vector3(0f, 0f, -30f)),
            new AnchorSpec("GangplankGround", "boarding", new Vector3(0f, 0.5f, -4.75f)),
            new AnchorSpec("D7_HelmPoint", "helm_runtime", new Vector3(0f, 1.1f, -28f)),
            new AnchorSpec("departure_channel_santa_margarita", "departure_channel", new Vector3(0f, 0f, -78f)),
            new AnchorSpec("return_channel_santa_margarita", "return_channel", new Vector3(0f, 0f, -54f)),
            new AnchorSpec("save_point_santa_margarita", "save_point", new Vector3(0f, 0.2f, -12f)),
            new AnchorSpec("npc:santa_margarita_dockhand", "npc_dockhand", new Vector3(12f, 0.2f, -8f)),
            new AnchorSpec("npc:santa_margarita_lookout", "npc_lookout", new Vector3(-68f, 6f, 84f)),
            new AnchorSpec("npc:santa_margarita_tavern_keeper", "npc_tavern_keeper", new Vector3(-34f, 0.2f, 52f))
        };

        private static readonly ShotSpec[] FixedShots =
        {
            new ShotSpec("d19_shot_main_menu", new Vector3(0f, 4f, 20f), new Vector3(0f, 2f, 60f)),
            new ShotSpec("d19_shot_spawn", new Vector3(-22f, 3.2f, 82f), new Vector3(-22f, 1f, 66f)),
            // The frozen landform rises to roughly y=12 at the market; keep the
            // verification camera above that surface instead of sampling its underside.
            new ShotSpec("d19_shot_market", new Vector3(42f, 17f, 74f), new Vector3(26f, 12f, 62f)),
            new ShotSpec("d19_shot_dock_overview", new Vector3(14f, 7f, 4f), new Vector3(0f, 1f, -22f)),
            new ShotSpec("d19_shot_tavern_entry", new Vector3(-58f, 4f, 58f), new Vector3(-42f, 2f, 50f)),
            new ShotSpec("d19_shot_coastline", new Vector3(-86f, 3f, -2f), new Vector3(-58f, 1f, -8f)),
            new ShotSpec("d19_shot_boarding", new Vector3(6f, 3.2f, -15f), new Vector3(0f, 1f, -20f)),
            new ShotSpec("d19_shot_on_ship", new Vector3(0f, 5f, -28f), new Vector3(0f, 2f, -18f)),
            new ShotSpec("d19_shot_departure", new Vector3(0f, 8f, -42f), new Vector3(0f, 1f, -78f)),
            new ShotSpec("d19_shot_nearshore", new Vector3(18f, 4f, -66f), new Vector3(0f, -1f, -88f)),
            new ShotSpec("d19_shot_weather", new Vector3(-18f, 6f, 18f), new Vector3(0f, 3f, -20f)),
            new ShotSpec("d19_shot_return_harbor", new Vector3(8f, 5f, -52f), new Vector3(0f, 1f, -12f)),
            new ShotSpec("d19_shot_settings_accessibility", new Vector3(0f, 3f, 60f), new Vector3(0f, 3f, 60f)),
            new ShotSpec("d19_shot_debug_boundary", new Vector3(104f, 72f, 104f), new Vector3(0f, 0f, 0f))
        };

        [MenuItem("WhalesIslandReef/D19-S02/Build and Integrate InitialHarbor")]
        public static void BuildAndIntegrateInitialHarbor()
        {
            var result = new IntegrationResult();
            var stages = new List<StageResult>();
            try
            {
                EnsureFolder("Assets/Prefabs", "Environment");
                EnsureFolder("Assets/Prefabs/Environment", "D19");
                EnsureFolder(IntegrationDirectory.Substring(0, IntegrationDirectory.LastIndexOf('/')), "Integration");
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                // Remove the old outer root before replacing its nested anchor and ambience
                // assets. This keeps Unity from importing a transient prefab with missing
                // nested sources during a deterministic rebuild.
                DeleteAssetIfExists(IntegrationPrefabPath);
                BuildStableAnchorPrefab();
                stages.Add(AuditPrefabStage("lock_evidence", "D19-S01 Attempt 2 + D18-S02-v2", AnchorPrefabPath));

                BuildAmbiencePrefab();
                stages.Add(AuditPrefabStage("integrate_ambience", "D19-A06", AmbiencePrefabPath));

                BuildArchitecturalDetailPrefab();
                stages.Add(AuditPrefabStage("integrate_r01_visual_detail", "D19-S02-R01 project-owned integration detail", ArchitecturalDetailPrefabPath));

                BuildIntegrationPrefab();
                stages.Add(AuditPrefabStage("integrate_terrain_port_biomes_water_lighting", "D19-A01—A05", IntegrationPrefabPath));

                IntegrateFormalScene(result);
                stages.Add(AuditFormalScene(result));

                CaptureFixedShots(result);
                stages.Add(new StageResult
                {
                    stage = "regression_and_evidence",
                    status = result.screenshot_count >= 14 ? "RECORDED" : "PARTIAL",
                    source = "D19-S02 fixed shot contract",
                    note = "Screenshots are formal-scene captures for R01; route and target-device evidence are recorded separately and never inferred from these images."
                });

                result.stages = stages.ToArray();
                result.status = stages.Any(s => s.status == "BLOCKED") ? "BLOCKED" :
                    stages.Any(s => s.status == "PARTIAL") ? "PARTIAL" : "PASS";
                WriteResult(result);
                UnityEngine.Debug.Log("[D19-S02] InitialHarbor integration status=" + result.status + ", screenshots=" + result.screenshot_count);
                if (Application.isBatchMode)
                    EditorApplication.Exit(result.status == "PASS" ? 0 : 1);
            }
            catch (Exception exception)
            {
                result.status = "BLOCKED";
                result.warnings = new[] { exception.ToString() };
                result.stages = stages.ToArray();
                WriteResult(result);
                UnityEngine.Debug.LogError("[D19-S02] Integration failed: " + exception);
                if (Application.isBatchMode) EditorApplication.Exit(1);
                throw;
            }
        }

        [MenuItem("WhalesIslandReef/D19-S02/Audit Integrated Materials")]
        public static void AuditIntegratedMaterials()
        {
            var entries = new List<MaterialAuditEntry>();
            var root = PrefabUtility.LoadPrefabContents(IntegrationPrefabPath);
            try
            {
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer == null) continue;
                    var material = renderer.sharedMaterial;
                    entries.Add(new MaterialAuditEntry
                    {
                        object_path = GetHierarchyPath(renderer.transform),
                        renderer_type = renderer.GetType().Name,
                        material_name = material == null ? string.Empty : material.name,
                        shader_name = material == null || material.shader == null ? string.Empty : material.shader.name,
                        material_missing = material == null,
                        shader_missing = material != null && material.shader == null,
                        error_shader = material != null && material.shader != null && material.shader.name == "Hidden/InternalErrorShader"
                    });
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            var output = new MaterialAuditResult
            {
                generated_on = DateTime.UtcNow.ToString("o"),
                prefab_path = IntegrationPrefabPath,
                renderer_count = entries.Count,
                missing_material_count = entries.Count(e => e.material_missing),
                missing_shader_count = entries.Count(e => e.shader_missing),
                error_shader_count = entries.Count(e => e.error_shader),
                entries = entries.ToArray()
            };
            Directory.CreateDirectory(Path.Combine(Directory.GetParent(Application.dataPath).FullName, ResultRoot, "Preflight"));
            File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).FullName, ResultRoot, "Preflight", "D19_S02_MATERIAL_AUDIT.json"), JsonUtility.ToJson(output, true));
            UnityEngine.Debug.Log($"[D19-S02] Material audit renderers={output.renderer_count}, missingMaterials={output.missing_material_count}, missingShaders={output.missing_shader_count}, errorShaders={output.error_shader_count}");
            if (Application.isBatchMode) EditorApplication.Exit(output.missing_material_count == 0 && output.missing_shader_count == 0 && output.error_shader_count == 0 ? 0 : 1);
        }

        [MenuItem("WhalesIslandReef/D19-S02/Audit R01 Geometry")]
        public static void AuditR01Geometry()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var root = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "D19_SaintMargaret_Root");
            var entries = new List<GeometryAuditEntry>();
            var sceneEntries = new List<GeometryAuditEntry>();
            if (root != null)
            {
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer == null) continue;
                    var bounds = renderer.bounds;
                    entries.Add(new GeometryAuditEntry
                    {
                        object_path = GetHierarchyPath(renderer.transform),
                        material_name = renderer.sharedMaterial == null ? string.Empty : renderer.sharedMaterial.name,
                        enabled = renderer.enabled,
                        center = bounds.center,
                        size = bounds.size,
                        volume = bounds.size.x * bounds.size.y * bounds.size.z
                    });
                }
            }
            foreach (var sceneRoot in scene.GetRootGameObjects())
            {
                foreach (var renderer in sceneRoot.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer == null) continue;
                    var bounds = renderer.bounds;
                    sceneEntries.Add(new GeometryAuditEntry
                    {
                        object_path = GetHierarchyPath(renderer.transform),
                        material_name = renderer.sharedMaterial == null ? string.Empty : renderer.sharedMaterial.name,
                        enabled = renderer.enabled,
                        center = bounds.center,
                        size = bounds.size,
                        volume = bounds.size.x * bounds.size.y * bounds.size.z
                    });
                }
            }
            entries = entries.OrderByDescending(entry => entry.volume).ToList();
            sceneEntries = sceneEntries.OrderByDescending(entry => entry.volume).ToList();
            var output = new GeometryAuditResult
            {
                generated_on = DateTime.UtcNow.ToString("o"),
                scene_path = ScenePath,
                renderer_count = entries.Count,
                scene_renderer_count = sceneEntries.Count,
                scene_enabled_renderer_count = sceneEntries.Count(entry => entry.enabled),
                entries = entries.ToArray(),
                scene_entries = sceneEntries.Take(200).ToArray()
            };
            Directory.CreateDirectory(ProjectPath(ResultRoot));
            File.WriteAllText(ProjectPath(ResultRoot + "/D19_S02_R01_GEOMETRY_AUDIT.json"), JsonUtility.ToJson(output, true));
            UnityEngine.Debug.Log("[D19-S02-R01] Geometry audit renderers=" + output.renderer_count);
            if (Application.isBatchMode) EditorApplication.Exit(root != null ? 0 : 1);
        }

        [Serializable]
        private sealed class MaterialAuditResult
        {
            public string generated_on;
            public string prefab_path;
            public int renderer_count;
            public int missing_material_count;
            public int missing_shader_count;
            public int error_shader_count;
            public MaterialAuditEntry[] entries = new MaterialAuditEntry[0];
        }

        [Serializable]
        private sealed class MaterialAuditEntry
        {
            public string object_path;
            public string renderer_type;
            public string material_name;
            public string shader_name;
            public bool material_missing;
            public bool shader_missing;
            public bool error_shader;
        }

        [Serializable]
        private sealed class GeometryAuditResult
        {
            public string generated_on;
            public string scene_path;
            public int renderer_count;
            public int scene_renderer_count;
            public int scene_enabled_renderer_count;
            public GeometryAuditEntry[] entries = new GeometryAuditEntry[0];
            public GeometryAuditEntry[] scene_entries = new GeometryAuditEntry[0];
        }

        [Serializable]
        private sealed class GeometryAuditEntry
        {
            public string object_path;
            public string material_name;
            public bool enabled;
            public Vector3 center;
            public Vector3 size;
            public float volume;
        }

        [MenuItem("WhalesIslandReef/D19-S02/Capture Formal Fixed Shots")]
        public static void CaptureFormalFixedShotsOnly()
        {
            var result = new IntegrationResult();
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                CaptureFixedShots(result);
                WriteResult(result);
                EditorSceneManager.SaveScene(scene);
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError("[D19-S02] Fixed shot capture failed: " + exception);
                if (Application.isBatchMode) EditorApplication.Exit(1);
                throw;
            }
        }

        private static void BuildStableAnchorPrefab()
        {
            DeleteAssetIfExists(AnchorPrefabPath);
            var root = new GameObject("D19_StableInteractionAnchors");
            foreach (var spec in AnchorSpecs)
            {
                var marker = new GameObject("D19_Anchor_" + Sanitize(spec.id));
                marker.transform.SetParent(root.transform, false);
                marker.transform.localPosition = spec.position;
            }
            SavePrefab(root, AnchorPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void BuildAmbiencePrefab()
        {
            DeleteAssetIfExists(AmbiencePrefabPath);
            var root = new GameObject("D19_AmbienceRoot");

            AddPrefab(root.transform, PropsRoot + "D19_Port_PropSet.prefab", new Vector3(0f, 0.2f, -10f), "D19_PropSet_PrimaryDock");
            AddPrefab(root.transform, PropsRoot + "D19_Market_Display.prefab", new Vector3(26f, 0.2f, 58f), "D19_PropSet_Market");
            AddPrefab(root.transform, PropsRoot + "D19_Shipyard_RepairSet.prefab", new Vector3(54f, 0.2f, 2f), "D19_PropSet_Shipyard");
            AddPrefab(root.transform, PropsRoot + "D19_Night_LampSet.prefab", new Vector3(-42f, 0.2f, 50f), "D19_PropSet_Tavern_Lamps");

            AddPrefab(root.transform, VfxRoot + "D19_Dust_VFX_Proxy.prefab", new Vector3(-4f, 0.2f, 52f), "D19_VFX_Dust_Road");
            AddPrefab(root.transform, VfxRoot + "D19_Firelight_VFX_Proxy.prefab", new Vector3(-42f, 0.2f, 50f), "D19_VFX_Firelight_Tavern");
            AddPrefab(root.transform, VfxRoot + "D19_InsectBird_Distant_VFX_Proxy.prefab", new Vector3(-68f, 4f, 84f), "D19_VFX_Distant_Birds");
            AddPrefab(root.transform, VfxRoot + "D19_SeaSpray_VFX_Proxy.prefab", new Vector3(0f, 0.15f, -54f), "D19_VFX_SeaSpray_Return");
            AddPrefab(root.transform, VfxRoot + "D19_Smoke_VFX_Proxy.prefab", new Vector3(60f, 0.2f, 8f), "D19_VFX_Shipyard_Smoke");
            // A06 prop candidates use the legacy Standard shader. Apply the integration-owned
            // URP palette here so the formal scene never renders them through the pink error path.
            ApplyPortPalette(root);
            ApplyVfxPalette(root);

            var zoneRoot = new GameObject("D19_AudioZoneContracts");
            zoneRoot.transform.SetParent(root.transform, false);
            AddTriggerZone(zoneRoot.transform, "Zone_Port_Entrance", new Vector3(0f, 1f, 112f), new Vector3(48f, 4f, 24f));
            AddTriggerZone(zoneRoot.transform, "Zone_Market", new Vector3(26f, 1f, 62f), new Vector3(36f, 4f, 28f));
            AddTriggerZone(zoneRoot.transform, "Zone_Coast", new Vector3(-58f, 1f, -8f), new Vector3(64f, 4f, 44f));
            AddTriggerZone(zoneRoot.transform, "Zone_Shipyard", new Vector3(60f, 1f, 8f), new Vector3(40f, 4f, 28f));
            AddTriggerZone(zoneRoot.transform, "Zone_Night_Overlay", new Vector3(0f, 1f, 50f), new Vector3(120f, 4f, 116f));
            var audioLibrary = new GameObject("D19_AudioCandidateLibrary_CC0_ReadOnly");
            audioLibrary.transform.SetParent(root.transform, false);

            SavePrefab(root, AmbiencePrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void BuildIntegrationPrefab()
        {
            DeleteAssetIfExists(IntegrationPrefabPath);
            var root = new GameObject("D19_SaintMargaret_Root");

            var environment = CreateChild(root.transform, "D19_EnvironmentRoot");
            // The source terrain mesh is authored in a 256 x 272 local tile and its
            // transform is the frozen world placement. Preserve that placement here;
            // placing it at zero shifts the whole island to the positive quadrant.
            var terrainRoot = AddPrefab(environment.transform, TerrainRoot + "D19_TerrainRoot.prefab", new Vector3(-128f, -8f, -128f), "D19_TerrainRoot");
            DisableOriginalTerrainPresentation(terrainRoot);
            AddPrefab(environment.transform, TerrainRoot + "D19_TerrainCollision.prefab", Vector3.zero, "D19_TerrainCollision");
            AddPrefab(environment.transform, TerrainRoot + "D19_TerrainAnchors.prefab", Vector3.zero, "D19_TerrainAnchors");
            var surfaceSet = AddPrefab(environment.transform, TerrainRoot + "D19_TerrainSurfaceSet.prefab", Vector3.zero, "D19_TerrainSurfaceSet");
            // The A01 surface set is retained for collision/ownership evidence, but its
            // preview strips compete with the R01 landform presentation in the formal
            // scene. Keep the objects and colliders; hide only the duplicate renderers.
            DisableOriginalSurfaceSetPresentation(surfaceSet);
            AddPrefab(environment.transform, TerrainRoot + "D19_CaveFissure.prefab", new Vector3(72f, TerrainHeightAt(72f, 72f) - 0.2f, 72f), "D19_CaveFissure");
            var terrainPresentation = AddPrefab(environment.transform, TerrainRoot + "D19_TerrainPresentation.prefab", Vector3.zero, "D19_TerrainPresentation");
            AddPrefab(environment.transform, TerrainRoot + "D19_TerrainNavMesh.prefab", Vector3.zero, "D19_TerrainNavMesh");
            DisableWorkPreviewCamerasAndLights(terrainPresentation);

            var port = CreateChild(root.transform, "D19_PortBuildingsRoot");
            AddPrefab(port.transform, PortRoot + "D19_PortEntry_Admin.prefab", new Vector3(0f, 0.2f, 112f), "D19_PortEntry_Admin");
            AddPrefab(port.transform, PortRoot + "D19_Market.prefab", new Vector3(26f, 0.2f, 62f), "D19_Market");
            AddPrefab(port.transform, PortRoot + "D19_Market_Stall.prefab", new Vector3(26f, 0.2f, 52f), "D19_Market_Stall");
            AddPrefab(port.transform, PortRoot + "D19_Warehouse.prefab", new Vector3(48f, 0.2f, 34f), "D19_Warehouse");
            AddPrefab(port.transform, PortRoot + "D19_Tavern_Exterior.prefab", new Vector3(-42f, 0.2f, 50f), "D19_Tavern_Exterior");
            AddPrefab(port.transform, PortRoot + "D19_Shipyard.prefab", new Vector3(60f, 0.2f, 8f), "D19_Shipyard");
            AddPrefab(port.transform, PortRoot + "D19_Dock_Primary.prefab", new Vector3(0f, 0.2f, -12f), "D19_Dock_Primary");
            AddPrefab(port.transform, PortRoot + "D19_Dock_Secondary.prefab", new Vector3(34f, 0.2f, -20f), "D19_Dock_Secondary");
            AddPrefab(port.transform, PortRoot + "D19_Common_Residence.prefab", new Vector3(-10f, 0.2f, 82f), "D19_Common_Residence");
            AddPrefab(port.transform, PortModuleRoot + "D19_Bollard.prefab", new Vector3(-6f, 0.2f, -12f), "D19_Bollard_Left");
            AddPrefab(port.transform, PortModuleRoot + "D19_Bollard.prefab", new Vector3(6f, 0.2f, -12f), "D19_Bollard_Right");
            AddPrefab(port.transform, PortModuleRoot + "D19_Fence_Module.prefab", new Vector3(14f, 0.2f, 40f), "D19_Fence_Market_East");
            AddPrefab(port.transform, PortModuleRoot + "D19_Stair_Module.prefab", new Vector3(40f, 0.2f, 46f), "D19_Stairs_Warehouse");
            AddPrefab(port.transform, PortModuleRoot + "D19_Port_Walk_Surface.prefab", new Vector3(0f, 0.2f, 52f), "D19_Port_Walk_Surface");
            ApplyPortPalette(port);
            DisableOriginalPortBuildingVisuals(port);
            AddPrefab(port.transform, ArchitecturalDetailPrefabPath, Vector3.zero, "D19_R01_PortAndCoastDetail");

            var biomes = CreateChild(root.transform, "D19_BiomeRoot");
            AddPrefab(biomes.transform, VegetationRoot + "D19_BiomeRoot.prefab", Vector3.zero, "D19_BiomeRoot");

            var shipsSea = CreateChild(root.transform, "D19_ShipsSeaRoot");
            AddPrefab(shipsSea.transform, WaterRoot + "WaterRoot.prefab", Vector3.zero, "WaterRoot");

            var presentation = CreateChild(root.transform, "D19_PresentationRoot");
            AddPrefab(presentation.transform, LightingRoot + "D19LightingRoot.prefab", Vector3.zero, "D19LightingRoot");
            ApplyIntegrationLightPalette(presentation);
            var captureCamera = CreateChild(presentation.transform, "D19_S02_CaptureCamera");
            var capture = captureCamera.AddComponent<Camera>();
            capture.enabled = false;
            capture.fieldOfView = 58f;
            capture.nearClipPlane = 0.25f;
            capture.farClipPlane = 450f;
            capture.clearFlags = CameraClearFlags.Skybox;

            var ambience = CreateChild(root.transform, "D19_AmbienceRoot");
            AddPrefab(ambience.transform, AmbiencePrefabPath, Vector3.zero, "D19_AmbienceRoot");

            var characters = CreateChild(root.transform, "D19_CharactersAnimalsRoot");
            CreateChild(characters.transform, "D19_RuntimeConsumers_Reserved");

            var gameplayAnchors = CreateChild(root.transform, "ExistingGameplayAnchors");
            AddPrefab(gameplayAnchors.transform, AnchorPrefabPath, Vector3.zero, "D19_StableInteractionAnchors");

            var devOverlay = CreateChild(root.transform, "D19_DevOverlay");
            devOverlay.SetActive(false);

            NormalizeIntegrationPresentation(root);
            SavePrefab(root, IntegrationPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void IntegrateFormalScene(IntegrationResult result)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var existing = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "D19_SaintMargaret_Root");
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(IntegrationPrefabPath);
            if (prefab == null) throw new InvalidOperationException("D19 integration prefab could not be loaded: " + IntegrationPrefabPath);
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) throw new InvalidOperationException("D19 integration prefab could not be instantiated.");
            SceneManager.MoveGameObjectToScene(instance, scene);
            instance.name = "D19_SaintMargaret_Root";
            instance.transform.position = Vector3.zero;

            ApplyIntegrationRenderSettings();

            int hiddenRenderers = 0;
            int hiddenLights = 0;
            foreach (var legacyName in new[] { "Dock", "Beach", "Sea" })
            {
                var legacy = scene.GetRootGameObjects().FirstOrDefault(go => go.name == legacyName);
                if (legacy == null) continue;
                foreach (var renderer in legacy.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer != null && renderer.enabled)
                    {
                        renderer.enabled = false;
                        hiddenRenderers++;
                    }
                }
            }

            var legacyLight = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "Directional Light");
            if (legacyLight != null)
            {
                var light = legacyLight.GetComponent<Light>();
                if (light != null && light.enabled)
                {
                    light.enabled = false;
                    hiddenLights++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("InitialHarbor scene save failed.");

            result.formal_initial_harbor_modified = true;
            result.legacy_renderers_hidden = hiddenRenderers;
            result.legacy_lights_hidden = hiddenLights;
            result.legacy_compatibility_nodes_preserved = new[] { "Dock", "DockDeck", "Gangplank", "Beach", "Sea", "Main Camera", "Harbor UI", "HarborControls" }
                .All(name => GameObject.Find(name) != null);
            result.dev_overlay_default_off = FindInactiveByName(scene, "D19_DevOverlay");
        }

        private static void BuildArchitecturalDetailPrefab()
        {
            DeleteAssetIfExists(ArchitecturalDetailPrefabPath);
            var root = new GameObject("D19_R01_PortAndCoastDetail");
            var facadeRoot = CreateChild(root.transform, "D19_R01_PortFacades");
            var wood = CreateIntegrationMaterial("D19_R01_Facade_Wood", new Color(0.30f, 0.105f, 0.035f), 0.02f, 0.72f);
            var darkWood = CreateIntegrationMaterial("D19_R01_Facade_DarkWood", new Color(0.095f, 0.028f, 0.012f), 0.02f, 0.78f);
            var stucco = CreateIntegrationMaterial("D19_R01_Facade_Stucco", new Color(0.72f, 0.49f, 0.27f), 0f, 0.88f);
            var lime = CreateIntegrationMaterial("D19_R01_Facade_Limewash", new Color(0.68f, 0.68f, 0.52f), 0f, 0.9f);
            var roof = CreateIntegrationMaterial("D19_R01_Facade_Terracotta", new Color(0.36f, 0.075f, 0.035f), 0f, 0.82f);
            var trim = CreateIntegrationMaterial("D19_R01_Facade_Trim", new Color(0.82f, 0.63f, 0.32f), 0.02f, 0.6f);
            var window = CreateIntegrationMaterial("D19_R01_Facade_Window", new Color(0.035f, 0.17f, 0.21f), 0.1f, 0.45f);
            SetEmission(window, new Color(0.04f, 0.15f, 0.18f), 0.35f);
            var stone = CreateIntegrationMaterial("D19_R01_Facade_Stone", new Color(0.25f, 0.28f, 0.24f), 0f, 0.9f);
            var cloth = CreateIntegrationMaterial("D19_R01_Facade_Canopy", new Color(0.68f, 0.19f, 0.07f), 0f, 0.88f);
            var coral = CreateIntegrationMaterial("D19_R01_Coast_Coral", new Color(0.52f, 0.20f, 0.15f), 0f, 0.82f);
            var reefRock = CreateIntegrationMaterial("D19_R01_Coast_ReefRock", new Color(0.16f, 0.24f, 0.22f), 0f, 0.92f);
            var landGrass = CreateIntegrationMaterial("D19_R01_Landform_Grass", new Color(0.24f, 0.39f, 0.19f), 0f, 0.94f);
            var landSand = CreateIntegrationMaterial("D19_R01_Landform_Sand", new Color(0.66f, 0.52f, 0.30f), 0f, 0.92f);
            var landRock = CreateIntegrationMaterial("D19_R01_Landform_Rock", new Color(0.29f, 0.31f, 0.25f), 0f, 0.96f);

            CreateLandformPresentation(root.transform, landGrass, landSand, landRock);
            CreateSettlementGround(root.transform, landGrass, landSand);

            CreateFacade(facadeRoot.transform, "Market", new Vector3(26f, TerrainHeightAt(26f, 62f), 62f), new Vector3(15f, 4.4f, 9f), stucco, roof, trim, window, darkWood, cloth, wood);
            CreateFacade(facadeRoot.transform, "Warehouse", new Vector3(48f, TerrainHeightAt(48f, 34f), 34f), new Vector3(17f, 4.8f, 10f), lime, roof, trim, window, darkWood, cloth, wood);
            CreateFacade(facadeRoot.transform, "Tavern", new Vector3(-42f, TerrainHeightAt(-42f, 50f), 50f), new Vector3(13f, 4.1f, 9f), stucco, roof, trim, window, darkWood, cloth, wood);
            CreateFacade(facadeRoot.transform, "Shipyard", new Vector3(60f, TerrainHeightAt(60f, 8f), 8f), new Vector3(19f, 4.5f, 12f), wood, roof, trim, window, darkWood, cloth, wood);
            CreateFacade(facadeRoot.transform, "PortEntry", new Vector3(0f, TerrainHeightAt(0f, 112f), 112f), new Vector3(14f, 3.8f, 7f), lime, roof, trim, window, darkWood, cloth, wood);
            CreateResidence(facadeRoot.transform, new Vector3(-10f, TerrainHeightAt(-10f, 82f), 82f), lime, roof, trim, window, darkWood);

            var dockDetail = CreateChild(root.transform, "D19_R01_DockDetail");
            CreateDockProps(dockDetail.transform, wood, darkWood, stone, trim);

            var roadDetail = CreateChild(root.transform, "D19_R01_RoadMarkers");
            CreateRoadMarkers(roadDetail.transform, stone, wood);

            var coastDetail = CreateChild(root.transform, "D19_R01_CoastDetail");
            CreateReefDetail(coastDetail.transform, reefRock, coral);

            SavePrefab(root, ArchitecturalDetailPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void CreateFacade(Transform parent, string id, Vector3 basePosition, Vector3 size,
            Material wall, Material roof, Material trim, Material window, Material door, Material canopy, Material propWood)
        {
            var root = CreateChild(parent, "D19_R01_Facade_" + id);
            root.transform.localPosition = basePosition;
            CreateVisualBox(root.transform, "Wall_Main", new Vector3(0f, size.y * 0.5f, 0f), new Vector3(size.x, size.y, size.z), wall);
            CreateVisualBox(root.transform, "Foundation_Stone", new Vector3(0f, 0.22f, 0f), new Vector3(size.x + 0.25f, 0.44f, size.z + 0.25f), trim);
            CreateVisualBox(root.transform, "Door", new Vector3(0f, 1.15f, -size.z * 0.51f), new Vector3(1.6f, 2.3f, 0.16f), door);
            CreateVisualBox(root.transform, "Door_Header", new Vector3(0f, 2.38f, -size.z * 0.52f), new Vector3(2.1f, 0.24f, 0.24f), trim);
            CreateVisualBox(root.transform, "Window_L", new Vector3(-size.x * 0.28f, 2.05f, -size.z * 0.52f), new Vector3(2.2f, 1.25f, 0.14f), window);
            CreateVisualBox(root.transform, "Window_R", new Vector3(size.x * 0.28f, 2.05f, -size.z * 0.52f), new Vector3(2.2f, 1.25f, 0.14f), window);
            CreateVisualBox(root.transform, "WindowBar_L", new Vector3(-size.x * 0.28f, 2.05f, -size.z * 0.535f), new Vector3(0.12f, 1.45f, 0.2f), trim);
            CreateVisualBox(root.transform, "WindowBar_R", new Vector3(size.x * 0.28f, 2.05f, -size.z * 0.535f), new Vector3(0.12f, 1.45f, 0.2f), trim);
            CreateGableRoof(root.transform, size, roof, trim);
            CreateVisualBox(root.transform, "Front_Awning", new Vector3(0f, 3.3f, -size.z * 0.6f), new Vector3(size.x * 0.75f, 0.18f, 1.2f), canopy);
            CreateVisualBox(root.transform, "Awning_Beam_L", new Vector3(-size.x * 0.36f, 2.72f, -size.z * 0.6f), new Vector3(0.18f, 1.4f, 0.18f), trim);
            CreateVisualBox(root.transform, "Awning_Beam_R", new Vector3(size.x * 0.36f, 2.72f, -size.z * 0.6f), new Vector3(0.18f, 1.4f, 0.18f), trim);
            CreateCrate(root.transform, new Vector3(-size.x * 0.42f, 0.55f, size.z * 0.58f), propWood, trim, "Crate_A");
            CreateCrate(root.transform, new Vector3(size.x * 0.42f, 0.42f, size.z * 0.58f), propWood, trim, "Crate_B");
            CreateBarrel(root.transform, new Vector3(size.x * 0.34f, 0.55f, -size.z * 0.64f), door, trim, "Barrel_Front");
        }

        private static void CreateResidence(Transform parent, Vector3 position, Material wall, Material roof, Material trim, Material window, Material door)
        {
            var root = CreateChild(parent, "D19_R01_Facade_Residence");
            root.transform.localPosition = position;
            CreateVisualBox(root.transform, "Wall_Main", new Vector3(0f, 1.8f, 0f), new Vector3(9f, 3.6f, 7f), wall);
            CreateVisualBox(root.transform, "Door", new Vector3(0f, 1.1f, -3.6f), new Vector3(1.35f, 2.2f, 0.14f), door);
            CreateVisualBox(root.transform, "Window", new Vector3(2.7f, 1.95f, -3.6f), new Vector3(1.8f, 1.2f, 0.14f), window);
            CreateGableRoof(root.transform, new Vector3(9f, 3.6f, 7f), roof, trim);
        }

        private static void CreateGableRoof(Transform parent, Vector3 size, Material roof, Material trim)
        {
            var z = size.z * 0.28f;
            var y = size.y + 0.22f;
            var panelSize = new Vector3(size.x + 0.7f, 0.3f, size.z * 0.7f);
            var left = CreateVisualBox(parent, "Roof_Left", new Vector3(0f, y + 0.44f, -z), panelSize, roof);
            left.transform.localRotation = Quaternion.Euler(-28f, 0f, 0f);
            var right = CreateVisualBox(parent, "Roof_Right", new Vector3(0f, y + 0.44f, z), panelSize, roof);
            right.transform.localRotation = Quaternion.Euler(28f, 0f, 0f);
            CreateVisualBox(parent, "Roof_Ridge", new Vector3(0f, y + 0.92f, 0f), new Vector3(size.x + 0.8f, 0.28f, 0.32f), trim);
        }

        private static void CreateDockProps(Transform parent, Material wood, Material darkWood, Material stone, Material trim)
        {
            for (int i = -2; i <= 2; i++)
            {
                CreateVisualBox(parent, "Dock_Post_" + i, new Vector3(i * 3.4f, 1.0f, -27f), new Vector3(0.55f, 2.0f, 0.55f), darkWood);
                CreateVisualBox(parent, "Dock_PostCap_" + i, new Vector3(i * 3.4f, 2.08f, -27f), new Vector3(0.85f, 0.18f, 0.85f), trim);
            }
            CreateBarrel(parent, new Vector3(-8f, 0.55f, -10f), wood, trim, "Dock_Barrel_L");
            CreateBarrel(parent, new Vector3(8f, 0.55f, -10f), wood, trim, "Dock_Barrel_R");
            CreateCrate(parent, new Vector3(-11f, 0.45f, -9f), wood, stone, "Dock_Crate");
        }

        private static void CreateRoadMarkers(Transform parent, Material stone, Material wood)
        {
            var route = new[] { new Vector3(-22f, 88f), new Vector3(-8f, 80f), new Vector3(10f, 71f), new Vector3(26f, 62f), new Vector3(38f, 48f), new Vector3(48f, 34f) };
            for (int i = 0; i < route.Length; i++)
            {
                var x = route[i].x;
                var z = route[i].y;
                var y = TerrainHeightAt(x, z);
                CreateVisualBox(parent, "Road_Stone_" + i, new Vector3(x, y + 0.13f, z), new Vector3(1.3f, 0.26f, 0.7f), stone).transform.localRotation = Quaternion.Euler(0f, i % 2 == 0 ? 18f : -18f, 0f);
                if (i < route.Length - 1)
                    CreateVisualBox(parent, "Road_Post_" + i, new Vector3(x + 1.2f, y + 0.8f, z + 1f), new Vector3(0.14f, 1.6f, 0.14f), wood);
            }
        }

        private static void CreateReefDetail(Transform parent, Material rock, Material coral)
        {
            var points = new[]
            {
                new Vector3(-30f, -22f, 2.8f), new Vector3(18f, -46f, 3.6f), new Vector3(34f, -64f, 3.2f),
                new Vector3(-18f, -80f, 4.2f), new Vector3(48f, -46f, 2.5f), new Vector3(-60f, -18f, 3.3f),
                new Vector3(28f, -88f, 2.2f)
            };
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var rockObject = CreateVisualBox(parent, "Reef_Rock_" + i, new Vector3(p.x, -0.65f, p.y), new Vector3(p.z, 1.3f, p.z * 0.7f), rock);
                rockObject.transform.localRotation = Quaternion.Euler(0f, i * 23f, i % 2 == 0 ? 9f : -12f);
                CreateVisualBox(parent, "Reef_Coral_" + i, new Vector3(p.x + 0.5f, 0.02f, p.y + 0.4f), new Vector3(0.7f, 0.2f, 0.6f), coral).transform.localRotation = Quaternion.Euler(0f, i * 31f, 0f);
            }
        }

        private static void CreateLandformPresentation(Transform parent, Material grass, Material sand, Material rock)
        {
            EnsureFolder(IntegrationDirectory, "Meshes");
            var meshPath = IntegrationDirectory + "/Meshes/D19_R01_LandformMesh.asset";
            DeleteAssetIfExists(meshPath);
            var vertices = new List<Vector3>();
            var grassTriangles = new List<int>();
            var sandTriangles = new List<int>();
            var rockTriangles = new List<int>();
            const int grid = 32;
            const float min = -128f;
            const float step = 8f;
            for (int ix = 0; ix < grid; ix++)
            {
                for (int iz = 0; iz < grid; iz++)
                {
                    float x0 = min + ix * step;
                    float x1 = x0 + step;
                    float z0 = min + iz * step;
                    float z1 = z0 + step;
                    AddLandformTriangle(x0, z0, x1, z0, x1, z1, vertices, grassTriangles, sandTriangles, rockTriangles, grass, sand, rock);
                    AddLandformTriangle(x0, z0, x1, z1, x0, z1, vertices, grassTriangles, sandTriangles, rockTriangles, grass, sand, rock);
                }
            }
            var mesh = new Mesh { name = "D19_R01_LandformMesh" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.subMeshCount = 3;
            mesh.SetTriangles(grassTriangles, 0);
            mesh.SetTriangles(sandTriangles, 1);
            mesh.SetTriangles(rockTriangles, 2);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, meshPath);

            var landform = CreateChild(parent, "D19_R01_LandformPresentation");
            var filter = landform.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = landform.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { grass, sand, rock };
            landform.isStatic = true;
        }

        private static void CreateSettlementGround(Transform parent, Material grass, Material sand)
        {
            var groundRoot = CreateChild(parent, "D19_R01_SettlementGroundPresentation");
            // These are presentation-only pads that close low coastal holes in the
            // sampled landform without replacing A01 collision or NavMesh ownership.
            CreateVisualBox(groundRoot.transform, "Tavern_Court_Visual", new Vector3(-42f, 0.59f, 50f), new Vector3(32f, 0.08f, 28f), grass);
            CreateVisualBox(groundRoot.transform, "Market_Court_Visual", new Vector3(26f, TerrainHeightAt(26f, 62f) + 0.06f, 62f), new Vector3(34f, 0.08f, 28f), grass);
            CreateVisualBox(groundRoot.transform, "PortEntry_Court_Visual", new Vector3(0f, TerrainHeightAt(0f, 112f) + 0.06f, 112f), new Vector3(26f, 0.08f, 20f), sand);
            CreateVisualBox(groundRoot.transform, "DockApproach_Visual", new Vector3(0f, 0.59f, -6f), new Vector3(32f, 0.08f, 34f), sand);
        }

        private static void AddLandformTriangle(float ax, float az, float bx, float bz, float cx, float cz,
            List<Vector3> vertices, List<int> grassTriangles, List<int> sandTriangles, List<int> rockTriangles,
            Material grass, Material sand, Material rock)
        {
            float ay = TerrainHeightAt(ax, az);
            float by = TerrainHeightAt(bx, bz);
            float cy = TerrainHeightAt(cx, cz);
            float centerY = (ay + by + cy) / 3f;
            if (centerY < 0.02f) return;
            int start = vertices.Count;
            vertices.Add(new Vector3(ax, ay + 0.035f, az));
            vertices.Add(new Vector3(bx, by + 0.035f, bz));
            vertices.Add(new Vector3(cx, cy + 0.035f, cz));
            var target = centerY < 0.8f ? sandTriangles : centerY > 7f ? rockTriangles : grassTriangles;
            // Reverse winding so the landform normals face the directional light.
            target.Add(start);
            target.Add(start + 2);
            target.Add(start + 1);
        }

        private static void DisableOriginalTerrainPresentation(GameObject terrainRoot)
        {
            if (terrainRoot == null) return;
            var terrain = terrainRoot.GetComponent<Terrain>();
            if (terrain != null) terrain.drawHeightmap = false;
            foreach (var renderer in terrainRoot.GetComponentsInChildren<Renderer>(true))
                if (renderer != null) renderer.enabled = false;
        }

        private static void DisableOriginalSurfaceSetPresentation(GameObject surfaceSet)
        {
            if (surfaceSet == null) return;
            foreach (var renderer in surfaceSet.GetComponentsInChildren<Renderer>(true))
                if (renderer != null) renderer.enabled = false;
        }

        private static void DisableOriginalPortBuildingVisuals(GameObject portRoot)
        {
            var visualRoots = new[]
            {
                "D19_PortEntry_Admin", "D19_Market", "D19_Market_Stall", "D19_Warehouse",
                "D19_Tavern_Exterior", "D19_Shipyard", "D19_Common_Residence"
            };
            foreach (var name in visualRoots)
            {
                var root = FindChild(portRoot.transform, name);
                if (root == null) continue;
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    if (renderer != null) renderer.enabled = false;
            }
        }

        private static GameObject CreateCrate(Transform parent, Vector3 position, Material wood, Material trim, string name)
        {
            var crate = CreateVisualBox(parent, name, position, new Vector3(0.9f, 0.9f, 0.9f), wood);
            CreateVisualBox(crate.transform, name + "_Band_X", Vector3.zero, new Vector3(0.98f, 0.08f, 0.08f), trim);
            CreateVisualBox(crate.transform, name + "_Band_Z", Vector3.zero, new Vector3(0.08f, 0.08f, 0.98f), trim);
            return crate;
        }

        private static GameObject CreateBarrel(Transform parent, Vector3 position, Material wood, Material trim, string name)
        {
            var barrel = CreateVisualCylinder(parent, name, position, new Vector3(0.65f, 0.8f, 0.65f), wood);
            CreateVisualBox(barrel.transform, name + "_Hoop", new Vector3(0f, 0.32f, 0f), new Vector3(0.72f, 0.09f, 0.72f), trim);
            CreateVisualBox(barrel.transform, name + "_Hoop_Low", new Vector3(0f, -0.32f, 0f), new Vector3(0.72f, 0.09f, 0.72f), trim);
            return barrel;
        }

        private static GameObject CreateVisualBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            var collider = go.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            return go;
        }

        private static GameObject CreateVisualCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            var collider = go.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            return go;
        }

        private static void ApplyIntegrationLightPalette(GameObject presentation)
        {
            var light = presentation.GetComponentsInChildren<Light>(true).FirstOrDefault(candidate => candidate.name == "D19_Main_Directional_Light");
            if (light == null) return;
            light.color = new Color(1f, 0.82f, 0.62f);
            light.intensity = 1.15f;
            light.shadowStrength = 0.78f;
        }

        private static void ApplyIntegrationRenderSettings()
        {
            RenderSettings.skybox = CreateIntegrationSkyboxMaterial();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.24f, 0.42f, 0.62f);
            RenderSettings.ambientEquatorColor = new Color(0.58f, 0.64f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.19f, 0.16f, 0.12f);
            RenderSettings.ambientIntensity = 0.82f;
            RenderSettings.fog = false;
            DynamicGI.UpdateEnvironment();
        }

        private static Material CreateIntegrationSkyboxMaterial()
        {
            EnsureFolder(IntegrationDirectory, "Materials");
            var path = IntegrationMaterialDirectory + "/D19_R01_Skybox_Procedural.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Skybox/Procedural");
            if (material == null && shader != null)
            {
                material = new Material(shader) { name = "D19_R01_Skybox_Procedural" };
                AssetDatabase.CreateAsset(material, path);
            }
            if (material == null) return RenderSettings.skybox;
            if (material.HasProperty("_SkyTint")) material.SetColor("_SkyTint", new Color(0.33f, 0.55f, 0.76f));
            if (material.HasProperty("_GroundColor")) material.SetColor("_GroundColor", new Color(0.25f, 0.21f, 0.17f));
            if (material.HasProperty("_AtmosphereThickness")) material.SetFloat("_AtmosphereThickness", 0.62f);
            if (material.HasProperty("_SunDisk")) material.SetFloat("_SunDisk", 1f);
            if (material.HasProperty("_SunSize")) material.SetFloat("_SunSize", 0.035f);
            if (material.HasProperty("_SunSizeConvergence")) material.SetFloat("_SunSizeConvergence", 5f);
            if (material.HasProperty("_Exposure")) material.SetFloat("_Exposure", 1.12f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static StageResult AuditPrefabStage(string stage, string source, string prefabPath)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null) return new StageResult { stage = stage, status = "BLOCKED", source = source, note = "Prefab could not be loaded: " + prefabPath };
            try
            {
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                var colliders = root.GetComponentsInChildren<Collider>(true);
                var nested = root.GetComponentsInChildren<Transform>(true).Count(t => PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject) != null);
                return new StageResult
                {
                    stage = stage,
                    status = renderers.Any(r => r == null || r.sharedMaterial == null) ? "PARTIAL" : "PASS",
                    source = source,
                    prefab_instances = nested,
                    renderers = renderers.Length,
                    colliders = colliders.Length,
                    note = "Prefab-only integration root; external candidate state is unchanged."
                };
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static StageResult AuditFormalScene(IntegrationResult result)
        {
            var scene = SceneManager.GetActiveScene();
            var root = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "D19_SaintMargaret_Root");
            if (root == null) return new StageResult { stage = "scene_audit", status = "BLOCKED", source = ScenePath, note = "Integration root missing." };

            var requiredRoots = new[]
            {
                "D19_EnvironmentRoot", "D19_PortBuildingsRoot", "D19_BiomeRoot", "D19_ShipsSeaRoot",
                "D19_PresentationRoot", "D19_AmbienceRoot", "D19_CharactersAnimalsRoot", "ExistingGameplayAnchors"
            };
            var missingRoots = requiredRoots.Where(name => FindChild(root.transform, name) == null).ToArray();
            var missingAnchors = AnchorSpecs.Where(spec => FindChild(root.transform, "D19_Anchor_" + Sanitize(spec.id)) == null).Select(spec => spec.id).ToArray();
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var colliders = root.GetComponentsInChildren<Collider>(true);
            var lods = root.GetComponentsInChildren<LODGroup>(true);
            var nested = root.GetComponentsInChildren<Transform>(true).Count(t => PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject) != null);
            result.integrated_prefab_instances = nested;
            result.integrated_renderers = renderers.Length;
            result.integrated_colliders = colliders.Length;
            result.integrated_lod_groups = lods.Length;
            result.stable_anchor_count = AnchorSpecs.Length - missingAnchors.Length;

            var warnings = new List<string>();
            if (missingRoots.Length > 0) warnings.Add("missing_roots:" + string.Join(",", missingRoots));
            if (missingAnchors.Length > 0) warnings.Add("missing_anchors:" + string.Join(",", missingAnchors));
            if (root.GetComponentsInChildren<Camera>(true).Count(c => c.enabled) > 0)
                warnings.Add("integration_camera_enabled_at_default");
            if (!result.legacy_compatibility_nodes_preserved) warnings.Add("compatibility_node_missing");
            result.warnings = warnings.ToArray();
            return new StageResult
            {
                stage = "scene_audit",
                status = warnings.Count == 0 ? "PASS" : "PARTIAL",
                source = ScenePath,
                prefab_instances = nested,
                renderers = renderers.Length,
                colliders = colliders.Length,
                note = warnings.Count == 0 ? "Required D19 roots, stable anchors and compatibility nodes present." : string.Join(";", warnings)
            };
        }

        private static void CaptureFixedShots(IntegrationResult result)
        {
            var scene = SceneManager.GetActiveScene();
            var root = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "D19_SaintMargaret_Root");
            if (root == null) throw new InvalidOperationException("Cannot capture shots before D19 integration root exists.");
            var captureObject = FindChild(root.transform, "D19_S02_CaptureCamera");
            var camera = captureObject == null ? null : captureObject.GetComponent<Camera>();
            if (camera == null) throw new InvalidOperationException("D19_S02_CaptureCamera is missing.");

            Directory.CreateDirectory(ProjectPath(ScreenshotRoot));
            var allCameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var enabledState = allCameras.ToDictionary(c => c, c => c.enabled);
            foreach (var other in allCameras) other.enabled = false;
            camera.enabled = false;

            var texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
            var renderTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            if (!renderTexture.IsCreated())
                throw new InvalidOperationException("D19-S02 fixed-shot capture requires a graphics-capable Unity session; RenderTexture 1920x1080 was not created.");
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            camera.targetTexture = renderTexture;
            var screenshotPaths = new List<string>();
            var samples = new List<float>();
            var weatherProfiles = new List<string>();
            try
            {
                for (var i = 0; i < FixedShots.Length; i++)
                {
                    var shot = FixedShots[i];
                    camera.transform.position = shot.position;
                    camera.transform.LookAt(shot.lookAt);
                    if (shot.id == "d19_shot_weather")
                    {
                        foreach (var weather in new[] { "Calm", "Rain", "Storm" })
                        {
                            var profileId = ApplyWeatherProfile(root, weather, weather == "Storm" ? 0.72f : 0.35f, weather == "Storm" ? 0.55f : 0.95f);
                            if (!string.IsNullOrWhiteSpace(profileId)) weatherProfiles.Add(profileId);
                            screenshotPaths.Add(RenderShot(camera, texture, renderTexture, shot.id + "_" + weather.ToLowerInvariant(), samples));
                        }
                    }
                    else
                    {
                        ApplyWeatherProfile(root, "Calm", 0.35f, 0.95f);
                        screenshotPaths.Add(RenderShot(camera, texture, renderTexture, shot.id, samples));
                    }
                }
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                foreach (var pair in enabledState) if (pair.Key != null) pair.Key.enabled = pair.Value;
                UnityEngine.Object.DestroyImmediate(texture);
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }

            result.screenshots = screenshotPaths.ToArray();
            result.screenshot_count = screenshotPaths.Count;
            result.fixed_shot_contract_count = FixedShots.Length;
            result.weather_profiles_checked = weatherProfiles.Distinct().Count();
            result.weather_profile_ids = weatherProfiles.Distinct().ToArray();
            if (samples.Count > 0)
            {
                samples.Sort();
                result.capture_1080p_average_ms = samples.Average();
                result.capture_1080p_p95_ms = samples[Mathf.Clamp(Mathf.CeilToInt(samples.Count * 0.95f) - 1, 0, samples.Count - 1)];
            }
        }

        private static string RenderShot(Camera camera, Texture2D texture, RenderTexture renderTexture, string id, List<float> samples)
        {
            var stopwatch = Stopwatch.StartNew();
            camera.Render();
            stopwatch.Stop();
            samples.Add((float)stopwatch.Elapsed.TotalMilliseconds);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
            texture.Apply(false, false);
            var file = ScreenshotRoot + "/" + id + ".png";
            File.WriteAllBytes(ProjectPath(file), texture.EncodeToPNG());
            return file;
        }

        private static string ApplyWeatherProfile(GameObject root, string weather, float normalizedTime, float visibility)
        {
            var adapter = root.GetComponentsInChildren<MonoBehaviour>(true)
                .FirstOrDefault(component => component != null && component.GetType().FullName == "Whalebone.Game.D19Presentation.D19LightingWeatherAdapter");
            if (adapter == null) return string.Empty;
            var type = adapter.GetType();
            type.GetMethod("LoadCatalog", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(adapter, null);
            type.GetMethod("EnsureTargets", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(adapter, null);
            var stateType = FindLoadedType("Whalebone.Game.D19.D19LightingReadOnlyState");
            if (stateType == null) return string.Empty;
            var state = Activator.CreateInstance(stateType, weather, normalizedTime, visibility);
            type.GetMethod("ApplyReadOnlyState", BindingFlags.Instance | BindingFlags.Public)?.Invoke(adapter, new[] { state });
            var property = type.GetProperty("CurrentProfileId", BindingFlags.Instance | BindingFlags.Public);
            return property == null ? string.Empty : property.GetValue(adapter, null) as string;
        }

        private static Type FindLoadedType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }

        private static void NormalizeIntegrationPresentation(GameObject root)
        {
            foreach (var camera in root.GetComponentsInChildren<Camera>(true))
                camera.enabled = camera.name == "D19_S02_CaptureCamera" && false;
            foreach (var light in root.GetComponentsInChildren<Light>(true))
            {
                if (light.name == "D19_TerrainWorkKeyLight") light.enabled = false;
            }
        }

        private static void ApplyPortPalette(GameObject portRoot)
        {
            EnsureFolder(IntegrationDirectory, "Materials");
            var palette = new Dictionary<string, Material>
            {
                ["wood"] = CreateIntegrationMaterial("D19_S02_Port_Wood", new Color(0.34f, 0.15f, 0.06f), 0.05f, 0.72f),
                ["dark_wood"] = CreateIntegrationMaterial("D19_S02_Port_DarkWood", new Color(0.13f, 0.055f, 0.025f), 0.02f, 0.82f),
                ["stucco"] = CreateIntegrationMaterial("D19_S02_Port_Stucco", new Color(0.67f, 0.55f, 0.38f), 0f, 0.9f),
                ["terracotta"] = CreateIntegrationMaterial("D19_S02_Port_Terracotta", new Color(0.38f, 0.09f, 0.045f), 0f, 0.82f),
                ["stone"] = CreateIntegrationMaterial("D19_S02_Port_Stone", new Color(0.32f, 0.34f, 0.29f), 0f, 0.88f),
                ["metal"] = CreateIntegrationMaterial("D19_S02_Port_Metal", new Color(0.12f, 0.13f, 0.12f), 0.25f, 0.52f),
                ["cloth"] = CreateIntegrationMaterial("D19_S02_Port_Cloth", new Color(0.72f, 0.33f, 0.12f), 0f, 0.9f),
                ["walkway"] = CreateIntegrationMaterial("D19_S02_Port_Walkway", new Color(0.25f, 0.22f, 0.16f), 0f, 0.94f),
                ["anchor"] = CreateIntegrationMaterial("D19_S02_Port_Anchor", new Color(1f, 0.62f, 0.05f), 0.1f, 0.35f),
                ["label"] = CreateIntegrationMaterial("D19_S02_Port_Label", new Color(0.92f, 0.84f, 0.45f), 0f, 0.6f)
            };
            foreach (var renderer in portRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                var originalName = renderer.sharedMaterial == null ? string.Empty : renderer.sharedMaterial.name.ToLowerInvariant();
                var key = originalName.Contains("market_cloth") || originalName.Contains("cloth") ? "cloth" :
                    originalName.Contains("terracotta") || originalName.Contains("roof") ? "terracotta" :
                    originalName.Contains("stucco") ? "stucco" :
                    originalName.Contains("metal") ? "metal" :
                    originalName.Contains("stone") ? "stone" :
                    originalName.Contains("anchor") ? "anchor" :
                    originalName.Contains("label") ? "label" :
                    originalName.Contains("walk") ? "walkway" :
                    originalName.Contains("dark") ? "dark_wood" : "wood";
                renderer.sharedMaterial = palette[key];
            }
            AssetDatabase.SaveAssets();
        }

        private static void ApplyVfxPalette(GameObject ambienceRoot)
        {
            EnsureFolder(IntegrationDirectory, "Materials");
            var palette = new Dictionary<string, Material>
            {
                ["dust"] = CreateIntegrationMaterial("D19_S02_VFX_Dust", new Color(0.42f, 0.28f, 0.16f), 0f, 0.15f),
                ["bird"] = CreateIntegrationMaterial("D19_S02_VFX_DistantBird", new Color(0.035f, 0.045f, 0.04f), 0f, 0.1f),
                ["sea_spray"] = CreateIntegrationMaterial("D19_S02_VFX_SeaSpray", new Color(0.35f, 0.78f, 0.86f), 0f, 0.35f),
                ["smoke"] = CreateIntegrationMaterial("D19_S02_VFX_Smoke", new Color(0.22f, 0.20f, 0.18f), 0f, 0.15f),
                ["firelight"] = CreateIntegrationMaterial("D19_S02_VFX_Firelight", new Color(1f, 0.28f, 0.04f), 0f, 0.2f)
            };
            SetEmission(palette["firelight"], new Color(1f, 0.08f, 0.01f), 2.4f);
            foreach (var renderer in ambienceRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                var current = renderer.transform;
                var key = string.Empty;
                while (current != null && current != ambienceRoot.transform)
                {
                    if (current.name.IndexOf("D19_VFX_Dust", StringComparison.OrdinalIgnoreCase) >= 0) { key = "dust"; break; }
                    if (current.name.IndexOf("D19_VFX_Distant_Birds", StringComparison.OrdinalIgnoreCase) >= 0) { key = "bird"; break; }
                    if (current.name.IndexOf("D19_VFX_SeaSpray", StringComparison.OrdinalIgnoreCase) >= 0) { key = "sea_spray"; break; }
                    if (current.name.IndexOf("D19_VFX_Shipyard_Smoke", StringComparison.OrdinalIgnoreCase) >= 0) { key = "smoke"; break; }
                    if (current.name.IndexOf("D19_VFX_Firelight", StringComparison.OrdinalIgnoreCase) >= 0) { key = "firelight"; break; }
                    current = current.parent;
                }
                if (!string.IsNullOrEmpty(key)) renderer.sharedMaterial = palette[key];
            }
            AssetDatabase.SaveAssets();
        }

        private static void SetEmission(Material material, Color color, float intensity)
        {
            if (material == null || !material.HasProperty("_EmissionColor")) return;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            EditorUtility.SetDirty(material);
        }

        private static string GetHierarchyPath(Transform transform)
        {
            var names = new Stack<string>();
            var current = transform;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }
            return string.Join("/", names.ToArray());
        }

        private static Material CreateIntegrationMaterial(string name, Color color, float metallic, float smoothness)
        {
            var path = IntegrationMaterialDirectory + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (shader != null)
            {
                material.shader = shader;
            }
            material.name = name;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void DisableWorkPreviewCamerasAndLights(GameObject root)
        {
            if (root == null) return;
            foreach (var camera in root.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
            foreach (var light in root.GetComponentsInChildren<Light>(true)) light.enabled = false;
        }

        private static GameObject AddPrefab(Transform parent, string path, Vector3 localPosition, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new InvalidOperationException("Prefab missing or not imported: " + path);
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) throw new InvalidOperationException("Prefab instantiation failed: " + path);
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.name = name;
            return instance;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void AddTriggerZone(Transform parent, string name, Vector3 position, Vector3 size)
        {
            var zone = CreateChild(parent, name);
            zone.transform.localPosition = position;
            var collider = zone.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            if (prefab == null) throw new InvalidOperationException("Prefab save failed: " + path);
            return prefab;
        }

        private static void WriteResult(IntegrationResult result)
        {
            Directory.CreateDirectory(ProjectPath(ResultRoot));
            File.WriteAllText(ProjectPath(ResultRoot + "/D19_S02_R01_INTEGRATION_RESULT.json"), JsonUtility.ToJson(result, true));
            var perf = "{\n" +
                       "  \"status\": \"RECORDED_EDITOR_CAPTURE\",\n" +
                       "  \"resolution\": [1920, 1080],\n" +
                       "  \"average_render_ms\": " + result.capture_1080p_average_ms.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ",\n" +
                       "  \"p95_render_ms\": " + result.capture_1080p_p95_ms.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ",\n" +
                       "  \"note\": \"Editor Camera.Render sample; target-device/PlayMode performance remains a gate.\"\n}\n";
            File.WriteAllText(ProjectPath(ResultRoot + "/D19_S02_R01_PERFORMANCE_1080P.json"), perf);
        }

        private static bool FindInactiveByName(Scene scene, string name)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Any(transform => transform.name == name && !transform.gameObject.activeSelf);
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

        private static string Sanitize(string value)
        {
            var chars = value.Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray();
            return new string(chars);
        }

        private static string ProjectPath(string relativePath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent))
            {
                var slash = parent.LastIndexOf('/');
                var grand = slash < 0 ? "Assets" : parent.Substring(0, slash);
                var leaf = slash < 0 ? parent : parent.Substring(slash + 1);
                EnsureFolder(grand, leaf);
            }
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static void DeleteAssetIfExists(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                AssetDatabase.DeleteAsset(path);
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
            // Port-side anchors and collision surfaces are authored around y=0.2—0.55.
            // Keep the visual land above the sea plane in the walkable settlement band;
            // this prevents the skybox ground colour from showing through low coastal
            // triangles while leaving the underlying A01 collision/physics untouched.
            if (z > coast + 2f)
                height = Mathf.Max(height, 0.55f);
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

        private static float Gaussian(float x, float z, float cx, float cz, float radius, float amplitude)
        {
            var dx = x - cx;
            var dz = z - cz;
            return amplitude * Mathf.Exp(-(dx * dx + dz * dz) / (radius * radius));
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            var t = Mathf.Clamp01((value - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }
    }
}
#endif
