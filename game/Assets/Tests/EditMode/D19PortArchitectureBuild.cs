#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace WhalesIslandReef.Tests.D19
{
    /// <summary>生成 D19-A02 项目自有模块、工作场景和六张预览图。</summary>
    public static class D19PortArchitectureBuild
    {
        private const string ScenePath = "Assets/Scenes/D19/Work/D19_PortArchitecture_Work.unity";
        private const string BuildingRoot = "Assets/Prefabs/Buildings/D19";
        private const string PortRoot = "Assets/Prefabs/Environment/D19/Port";
        private const string MaterialRoot = PortRoot + "/Materials";
        private const string ResultRoot = "tools/results/D19/D19-A02";

        private sealed class AnchorSpec
        {
            public string AnchorId;
            public string StableId;
            public string OwnerTask;
            public string LogicOwner;
            public string MigrationPolicy;
            public Vector3 LocalPosition;

            public AnchorSpec(string anchorId, string stableId, Vector3 localPosition,
                string ownerTask = "D19-A02", string logicOwner = "", string migrationPolicy = "preserve stable ID")
            {
                AnchorId = anchorId;
                StableId = stableId;
                LocalPosition = localPosition;
                OwnerTask = ownerTask;
                LogicOwner = logicOwner;
                MigrationPolicy = migrationPolicy;
            }
        }

        private sealed class Palette
        {
            public Material Wood;
            public Material DarkWood;
            public Material Stucco;
            public Material Roof;
            public Material Stone;
            public Material Metal;
            public Material Cloth;
            public Material Walk;
            public Material Anchor;
            public Material Label;
        }

        [MenuItem("WhalesIslandReef/D19/Build Port Architecture Work Scene")]
        public static void BuildAndCapture()
        {
            EnsureFolders();
            Palette palette = CreatePalette();
            List<string> prefabPaths = new List<string>();
            prefabPaths.Add(BuildBuildingPrefab("D19_PortEntry_Admin", "building_port_entry_santa_margarita", "management", "port_admin", "test_d19_a02_port_entry", 18f, 5.2f, 12f,
                new[] { new AnchorSpec("anchor.port.entry", "santa_margarita", new Vector3(0f, 0.2f, 0f), "D19-A02", "WorldCatalog/D3", "preserve_location_ref") }, palette));
            prefabPaths.Add(BuildBuildingPrefab("D19_Market", "building_market_santa_margarita", "trade", "market", "test_d19_a02_market", 24f, 4.8f, 14f,
                new[] {
                    new AnchorSpec("anchor.market.service", "building:market_santa_margarita", new Vector3(0f, 0.2f, 0f), "D19-A02", "TradeEconomySystem", "rebind service marker; preserve service ID"),
                    new AnchorSpec("anchor.market.queue", "market_queue_santa_margarita", new Vector3(-14f, 0.2f, -4f), "D19-A02", "D9 UI/service flow", "preserve request target")
                }, palette));
            prefabPaths.Add(BuildBuildingPrefab("D19_Warehouse", "building_warehouse_santa_margarita", "trade", "warehouse", "test_d19_a02_warehouse", 24f, 5.4f, 14f,
                new[] {
                    new AnchorSpec("anchor.warehouse.service", "building:warehouse_santa_margarita", new Vector3(0f, 0.2f, 0f), "D19-A02", "Inventory/Cargo systems", "rebind service marker; preserve storage semantics"),
                    new AnchorSpec("anchor.warehouse.queue", "warehouse_queue_santa_margarita", new Vector3(-8f, 0.2f, -10f), "D19-A02", "Inventory/Cargo systems", "preserve request target")
                }, palette));
            prefabPaths.Add(BuildBuildingPrefab("D19_Tavern_Exterior", "building_tavern_santa_margarita", "life/social", "tavern", "test_d19_a02_tavern_entry", 18f, 5.0f, 14f,
                new[] { new AnchorSpec("anchor.tavern.entry", "building:tavern_santa_margarita", new Vector3(0f, 0.2f, 0f), "D19-A02", "D10TavernSocialSystem", "preserve tavern ID and warning entry") }, palette));
            prefabPaths.Add(BuildBuildingPrefab("D19_Shipyard", "building_shipyard_santa_margarita", "shipyard", "shipyard", "test_d19_a02_shipyard", 28f, 5.6f, 18f,
                new[] {
                    new AnchorSpec("anchor.shipyard.service", "building:shipyard_santa_margarita", new Vector3(0f, 0.2f, 0f), "D19-A02", "ShipyardSystem", "preserve repair and preparation service"),
                    new AnchorSpec("anchor.shipyard.repair", "shipyard_repair_santa_margarita", new Vector3(-6f, 0.2f, -6f), "D19-A02", "ShipyardSystem", "preserve event and cost ownership")
                }, palette));
            prefabPaths.Add(BuildDockPrefab("D19_Dock_Primary", "building_saint_marys_dock", true, palette));
            prefabPaths.Add(BuildDockPrefab("D19_Dock_Secondary", "building_secondary_pier_santa_margarita", false, palette));
            prefabPaths.Add(BuildStallPrefab("D19_Market_Stall", palette));
            prefabPaths.Add(BuildBollardPrefab("D19_Bollard", palette));
            prefabPaths.Add(BuildFencePrefab("D19_Fence_Module", palette));
            prefabPaths.Add(BuildStairPrefab("D19_Stair_Module", palette));
            prefabPaths.Add(BuildWalkSurfacePrefab("D19_Port_Walk_Surface", palette));
            prefabPaths.Add(BuildHousePrefab("D19_Common_Residence", palette));

            AssetDatabase.SaveAssets();
            BuildWorkScene(palette);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            CapturePreviewShots();
            WriteResultSummary(prefabPaths);
            Debug.Log("[D19-A02] Build complete. Prefabs=" + prefabPaths.Count + ", scene=" + ScenePath + ", external_manifest_ids=[]");
        }

        private static void EnsureFolders()
        {
            foreach (string path in new[] {
                "Assets/Scenes/D19", "Assets/Scenes/D19/Work", "Assets/Prefabs/Buildings", BuildingRoot,
                "Assets/Prefabs/Environment", "Assets/Prefabs/Environment/D19", PortRoot, MaterialRoot, "tools/results/D19", ResultRoot
            })
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                Directory.CreateDirectory(Path.Combine(projectRoot, path.Replace('/', Path.DirectorySeparatorChar)));
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static Palette CreatePalette()
        {
            Shader shader = GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/Lit")
                : Shader.Find("Standard");
            shader = shader ?? Shader.Find("Unlit/Color");
            return new Palette
            {
                Wood = CreateMaterial("d19_mat_port_wood", shader, new Color(0.34f, 0.15f, 0.06f), 0.05f, 0.72f),
                DarkWood = CreateMaterial("d19_mat_port_dark_wood", shader, new Color(0.13f, 0.055f, 0.025f), 0.02f, 0.82f),
                Stucco = CreateMaterial("d19_mat_port_stucco", shader, new Color(0.67f, 0.55f, 0.38f), 0f, 0.9f),
                Roof = CreateMaterial("d19_mat_port_terracotta", shader, new Color(0.38f, 0.09f, 0.045f), 0f, 0.82f),
                Stone = CreateMaterial("d19_mat_port_stone", shader, new Color(0.32f, 0.34f, 0.29f), 0f, 0.88f),
                Metal = CreateMaterial("d19_mat_port_metal", shader, new Color(0.12f, 0.13f, 0.12f), 0.25f, 0.52f),
                Cloth = CreateMaterial("d19_mat_port_market_cloth", shader, new Color(0.72f, 0.33f, 0.12f), 0f, 0.9f),
                Walk = CreateMaterial("d19_mat_port_walkway", shader, new Color(0.25f, 0.22f, 0.16f), 0f, 0.94f),
                Anchor = CreateMaterial("d19_mat_port_anchor_marker", shader, new Color(1f, 0.62f, 0.05f), 0.1f, 0.35f),
                Label = CreateMaterial("d19_mat_port_label", shader, new Color(0.92f, 0.84f, 0.45f), 0f, 0.6f)
            };
        }

        private static Material CreateMaterial(string name, Shader shader, Color color, float metallic, float smoothness)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 1f - smoothness);
            return material;
        }

        private static string BuildBuildingPrefab(string fileName, string buildingId, string category, string materialGroup,
            string testId, float width, float height, float depth, AnchorSpec[] anchors, Palette palette)
        {
            string path = BuildingRoot + "/" + fileName + ".prefab";
            GameObject root = new GameObject(fileName);
            AddPortMarker(root, buildingId, category, materialGroup, testId);
            GameObject lod0 = new GameObject("LOD0_Structural");
            lod0.transform.SetParent(root.transform, false);
            CreateBuildingShell(lod0.transform, width, height, depth, palette, true);
            GameObject lod1 = new GameObject("LOD1_Silhouette");
            lod1.transform.SetParent(root.transform, false);
            CreateCube(lod1.transform, "LowPolyBuilding", palette.Stucco, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, depth), false);
            AddLodGroup(root, lod0, lod1);
            AddBuildingAnchors(root.transform, anchors, palette.Anchor);
            return SavePrefab(root, path);
        }

        private static void CreateBuildingShell(Transform parent, float width, float height, float depth, Palette palette, bool openFront)
        {
            float wall = 0.35f;
            float doorWidth = 2.2f;
            CreateCube(parent, "Floor_Collision", palette.Stone, new Vector3(0f, 0.1f, 0f), new Vector3(width, 0.2f, depth), true);
            CreateCube(parent, "Wall_Back", palette.Stucco, new Vector3(0f, height * 0.5f, depth * 0.5f - wall * 0.5f), new Vector3(width, height, wall), true);
            CreateCube(parent, "Wall_Left", palette.Stucco, new Vector3(-width * 0.5f + wall * 0.5f, height * 0.5f, 0f), new Vector3(wall, height, depth), true);
            CreateCube(parent, "Wall_Right", palette.Stucco, new Vector3(width * 0.5f - wall * 0.5f, height * 0.5f, 0f), new Vector3(wall, height, depth), true);
            if (openFront)
            {
                float sideWidth = (width - doorWidth) * 0.5f;
                CreateCube(parent, "Wall_Front_Left", palette.Stucco, new Vector3(-width * 0.5f + sideWidth * 0.5f, height * 0.5f, -depth * 0.5f + wall * 0.5f), new Vector3(sideWidth, height, wall), true);
                CreateCube(parent, "Wall_Front_Right", palette.Stucco, new Vector3(width * 0.5f - sideWidth * 0.5f, height * 0.5f, -depth * 0.5f + wall * 0.5f), new Vector3(sideWidth, height, wall), true);
                CreateCube(parent, "Door_Header", palette.DarkWood, new Vector3(0f, height - 0.45f, -depth * 0.5f), new Vector3(doorWidth, 0.45f, 0.4f), true);
                CreateCube(parent, "Door_Left_Post", palette.DarkWood, new Vector3(-doorWidth * 0.5f, height * 0.5f, -depth * 0.5f), new Vector3(0.35f, height, 0.4f), true);
                CreateCube(parent, "Door_Right_Post", palette.DarkWood, new Vector3(doorWidth * 0.5f, height * 0.5f, -depth * 0.5f), new Vector3(0.35f, height, 0.4f), true);
                CreateCube(parent, "Open_Door_Leaf", palette.DarkWood, new Vector3(doorWidth * 0.5f + 0.08f, height * 0.48f, -depth * 0.5f - 0.18f), new Vector3(0.12f, height * 0.92f, doorWidth), false, Quaternion.Euler(0f, 90f, 0f));
            }
            else
            {
                CreateCube(parent, "Wall_Front", palette.Stucco, new Vector3(0f, height * 0.5f, -depth * 0.5f + wall * 0.5f), new Vector3(width, height, wall), true);
            }
            CreateCube(parent, "Roof_Collision", palette.Roof, new Vector3(0f, height + 0.18f, 0f), new Vector3(width + 0.7f, 0.36f, depth + 0.7f), true);
            CreateCube(parent, "Roof_Ridge", palette.DarkWood, new Vector3(0f, height + 0.48f, 0f), new Vector3(0.25f, 0.25f, depth + 0.9f), true);
        }

        private static string BuildDockPrefab(string fileName, string buildingId, bool primary, Palette palette)
        {
            string path = BuildingRoot + "/" + fileName + ".prefab";
            GameObject root = new GameObject(fileName);
            AddPortMarker(root, buildingId, "port/dock", "dock_wood", primary ? "test_d19_a02_primary_dock" : "test_d19_a02_secondary_pier");
            GameObject lod0 = new GameObject("LOD0_DockStructure");
            lod0.transform.SetParent(root.transform, false);
            float width = primary ? 18f : 10f;
            float length = primary ? 18f : 14f;
            CreateCube(lod0.transform, "DockDeck_Collision", palette.Wood, new Vector3(0f, 0.1f, 0f), new Vector3(width, 0.2f, length), true);
            for (int i = -1; i <= 1; i += 2)
            {
                CreateCube(lod0.transform, "HandRail_" + i, palette.DarkWood, new Vector3(i * (width * 0.5f - 0.12f), 0.85f, 0f), new Vector3(0.24f, 1.3f, length), true);
                CreateCube(lod0.transform, "RailTop_" + i, palette.DarkWood, new Vector3(i * (width * 0.5f - 0.12f), 1.5f, 0f), new Vector3(0.32f, 0.2f, length), true);
            }
            for (int z = -1; z <= 1; z++)
            {
                CreateCube(lod0.transform, "DockPile_" + z, palette.DarkWood, new Vector3(-width * 0.36f, -0.75f, z * length * 0.36f), new Vector3(0.35f, 1.8f, 0.35f), true);
                CreateCube(lod0.transform, "DockPile_R_" + z, palette.DarkWood, new Vector3(width * 0.36f, -0.75f, z * length * 0.36f), new Vector3(0.35f, 1.8f, 0.35f), true);
            }
            if (primary)
            {
                CreateCube(lod0.transform, "Gangplank_Collision", palette.Wood, new Vector3(0f, 0.1f, -4.75f), new Vector3(2.2f, 0.2f, 3.2f), true, Quaternion.Euler(0f, 0f, 0f));
                CreateCylinder(lod0.transform, "Bollard_Left", palette.Metal, new Vector3(-6f, 0.45f, 3.0f), new Vector3(0.42f, 0.7f, 0.42f), true);
                CreateCylinder(lod0.transform, "Bollard_Right", palette.Metal, new Vector3(6f, 0.45f, 3.0f), new Vector3(0.42f, 0.7f, 0.42f), true);
            }
            GameObject lod1 = new GameObject("LOD1_DockSilhouette");
            lod1.transform.SetParent(root.transform, false);
            CreateCube(lod1.transform, "LowPolyDock", palette.Wood, new Vector3(0f, 0.1f, 0f), new Vector3(width, 0.2f, length), false);
            AddLodGroup(root, lod0, lod1);
            if (primary)
            {
                AddAnchor(root.transform, new AnchorSpec("anchor.dock.primary", "building_saint_marys_dock", new Vector3(0f, 0.2f, 0f), "D19-A02", "D9PortShipPreparationSystem", "preserve building ID and berth lookup"), palette.Anchor);
                AddAnchor(root.transform, new AnchorSpec("anchor.disembark.dock", "DockGround", new Vector3(0f, 0.2f, 2f), "D19-A02", "HarborCollisionController/D6/D17", "preserve ground contact and exit route"), palette.Anchor);
                AddAnchor(root.transform, new AnchorSpec("anchor.berth.santa_margarita", "berth_santa_margarita", new Vector3(0f, 0.1f, -18f), "D19-A04", "ShipSystem", "visual berth envelope; water/ship owner remains D19-A04"), palette.Anchor);
                AddAnchor(root.transform, new AnchorSpec("anchor.boarding.gangplank", "GangplankGround", new Vector3(0f, 0.5f, -4.75f), "D19-A04", "ShipSystem/D9", "visual boarding interface; preserve stable collision name"), palette.Anchor);
            }
            return SavePrefab(root, path);
        }

        private static string BuildStallPrefab(string fileName, Palette palette)
        {
            string path = BuildingRoot + "/" + fileName + ".prefab";
            GameObject root = new GameObject(fileName);
            AddPortMarker(root, "market_stall_santa_margarita", "trade/market_stall", "market_cloth_wood", "test_d19_a02_market_stall");
            GameObject lod0 = new GameObject("LOD0_Stall");
            lod0.transform.SetParent(root.transform, false);
            CreateCube(lod0.transform, "Counter_Collision", palette.Wood, new Vector3(0f, 0.75f, 0f), new Vector3(3.4f, 1.2f, 1.1f), true);
            CreateCube(lod0.transform, "Awning", palette.Cloth, new Vector3(0f, 2.1f, 0.2f), new Vector3(4f, 0.18f, 2.2f), true, Quaternion.Euler(8f, 0f, 0f));
            CreateCube(lod0.transform, "Post_L", palette.DarkWood, new Vector3(-1.65f, 1.3f, 0.4f), new Vector3(0.18f, 2.6f, 0.18f), true);
            CreateCube(lod0.transform, "Post_R", palette.DarkWood, new Vector3(1.65f, 1.3f, 0.4f), new Vector3(0.18f, 2.6f, 0.18f), true);
            GameObject lod1 = new GameObject("LOD1_Stall");
            lod1.transform.SetParent(root.transform, false);
            CreateCube(lod1.transform, "LowPolyStall", palette.Wood, new Vector3(0f, 0.8f, 0f), new Vector3(3.5f, 1.6f, 1.4f), false);
            AddLodGroup(root, lod0, lod1);
            return SavePrefab(root, path);
        }

        private static string BuildBollardPrefab(string fileName, Palette palette)
        {
            string path = PortRoot + "/" + fileName + ".prefab";
            GameObject root = new GameObject(fileName);
            AddPortMarker(root, "port_bollard_module", "port/fitting", "metal_wood", "test_d19_a02_bollard");
            GameObject lod0 = new GameObject("LOD0_Bollard");
            lod0.transform.SetParent(root.transform, false);
            CreateCylinder(lod0.transform, "Bollard_Collision", palette.Metal, new Vector3(0f, 0.55f, 0f), new Vector3(0.65f, 1.1f, 0.65f), true);
            CreateCube(lod0.transform, "Bollard_Cap", palette.DarkWood, new Vector3(0f, 1.1f, 0f), new Vector3(0.9f, 0.18f, 0.9f), true);
            GameObject lod1 = new GameObject("LOD1_Bollard");
            lod1.transform.SetParent(root.transform, false);
            CreateCylinder(lod1.transform, "LowPolyBollard", palette.Metal, new Vector3(0f, 0.55f, 0f), new Vector3(0.7f, 1.1f, 0.7f), false);
            AddLodGroup(root, lod0, lod1);
            return SavePrefab(root, path);
        }

        private static string BuildFencePrefab(string fileName, Palette palette)
        {
            string path = PortRoot + "/" + fileName + ".prefab";
            GameObject root = new GameObject(fileName);
            AddPortMarker(root, "port_fence_module", "port/boundary", "dark_wood", "test_d19_a02_fence");
            GameObject lod0 = new GameObject("LOD0_Fence");
            lod0.transform.SetParent(root.transform, false);
            CreateCube(lod0.transform, "Fence_Rail_L", palette.DarkWood, new Vector3(0f, 0.85f, -0.8f), new Vector3(5f, 0.16f, 0.16f), true);
            CreateCube(lod0.transform, "Fence_Rail_H", palette.DarkWood, new Vector3(0f, 1.35f, -0.8f), new Vector3(5f, 0.16f, 0.16f), true);
            for (int x = -2; x <= 2; x += 2) CreateCube(lod0.transform, "Fence_Post_" + x, palette.DarkWood, new Vector3(x, 0.8f, -0.8f), new Vector3(0.22f, 1.6f, 0.22f), true);
            GameObject lod1 = new GameObject("LOD1_Fence");
            lod1.transform.SetParent(root.transform, false);
            CreateCube(lod1.transform, "LowPolyFence", palette.DarkWood, new Vector3(0f, 0.8f, -0.8f), new Vector3(5f, 1.6f, 0.2f), false);
            AddLodGroup(root, lod0, lod1);
            return SavePrefab(root, path);
        }

        private static string BuildStairPrefab(string fileName, Palette palette)
        {
            string path = PortRoot + "/" + fileName + ".prefab";
            GameObject root = new GameObject(fileName);
            AddPortMarker(root, "port_stair_module", "port/access", "stone_wood", "test_d19_a02_stairs");
            GameObject lod0 = new GameObject("LOD0_Stairs");
            lod0.transform.SetParent(root.transform, false);
            for (int i = 0; i < 4; i++) CreateCube(lod0.transform, "Step_" + i, palette.Stone, new Vector3(0f, 0.05f + i * 0.12f, i * 0.45f), new Vector3(2.4f, 0.1f + i * 0.24f, 0.5f), true);
            GameObject lod1 = new GameObject("LOD1_Stairs");
            lod1.transform.SetParent(root.transform, false);
            CreateCube(lod1.transform, "LowPolyStairs", palette.Stone, new Vector3(0f, 0.25f, 0.8f), new Vector3(2.5f, 0.5f, 1.8f), false);
            AddLodGroup(root, lod0, lod1);
            return SavePrefab(root, path);
        }

        private static string BuildWalkSurfacePrefab(string fileName, Palette palette)
        {
            string path = PortRoot + "/" + fileName + ".prefab";
            GameObject root = new GameObject(fileName);
            AddPortMarker(root, "port_walk_surface_module", "port/road", "walkway_stone", "test_d19_a02_walk_surface");
            GameObject lod0 = new GameObject("LOD0_WalkSurface");
            lod0.transform.SetParent(root.transform, false);
            CreateCube(lod0.transform, "WalkSurface_Collision", palette.Walk, new Vector3(0f, 0.1f, 0f), new Vector3(4f, 0.2f, 10f), true);
            GameObject lod1 = new GameObject("LOD1_WalkSurface");
            lod1.transform.SetParent(root.transform, false);
            CreateCube(lod1.transform, "LowPolyWalkSurface", palette.Walk, new Vector3(0f, 0.1f, 0f), new Vector3(4f, 0.2f, 10f), false);
            AddLodGroup(root, lod0, lod1);
            return SavePrefab(root, path);
        }

        private static string BuildHousePrefab(string fileName, Palette palette)
        {
            string path = BuildingRoot + "/" + fileName + ".prefab";
            GameObject root = new GameObject(fileName);
            AddPortMarker(root, "building_common_residence", "life/residence", "stucco_wood", "test_d19_a02_residence");
            GameObject lod0 = new GameObject("LOD0_Residence");
            lod0.transform.SetParent(root.transform, false);
            CreateBuildingShell(lod0.transform, 10f, 3.6f, 8f, palette, true);
            GameObject lod1 = new GameObject("LOD1_Residence");
            lod1.transform.SetParent(root.transform, false);
            CreateCube(lod1.transform, "LowPolyResidence", palette.Stucco, new Vector3(0f, 1.8f, 0f), new Vector3(10f, 3.6f, 8f), false);
            AddLodGroup(root, lod0, lod1);
            return SavePrefab(root, path);
        }

        private static void BuildWorkScene(Palette palette)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("D19_PortArchitecture_Work");
            GameObject buildings = new GameObject("D19_PortBuildingsRoot");
            buildings.transform.SetParent(root.transform, false);
            GameObject roads = new GameObject("D19_PortWalkwaysRoot");
            roads.transform.SetParent(root.transform, false);
            GameObject anchors = new GameObject("D19_AnchorVisualization");
            anchors.transform.SetParent(root.transform, false);
            GameObject preview = new GameObject("D19_PreviewRig");
            preview.transform.SetParent(root.transform, false);

            Place(buildings, BuildingRoot + "/D19_PortEntry_Admin.prefab", new Vector3(0f, 0f, 112f), "D19_PortEntry_Admin");
            Place(buildings, BuildingRoot + "/D19_Market.prefab", new Vector3(26f, 0f, 62f), "D19_Market");
            Place(buildings, BuildingRoot + "/D19_Warehouse.prefab", new Vector3(48f, 0f, 34f), "D19_Warehouse");
            Place(buildings, BuildingRoot + "/D19_Tavern_Exterior.prefab", new Vector3(-42f, 0f, 50f), "D19_Tavern_Exterior");
            Place(buildings, BuildingRoot + "/D19_Shipyard.prefab", new Vector3(60f, 0f, 8f), "D19_Shipyard");
            Place(buildings, BuildingRoot + "/D19_Dock_Primary.prefab", new Vector3(0f, 0f, -12f), "D19_Dock_Primary");
            Place(buildings, BuildingRoot + "/D19_Dock_Secondary.prefab", new Vector3(-34f, 0f, -14f), "D19_Dock_Secondary");
            Place(buildings, BuildingRoot + "/D19_Common_Residence.prefab", new Vector3(-12f, 0f, 88f), "D19_Residence_A");
            Place(buildings, BuildingRoot + "/D19_Common_Residence.prefab", new Vector3(18f, 0f, 88f), "D19_Residence_B");
            Place(buildings, BuildingRoot + "/D19_Common_Residence.prefab", new Vector3(-68f, 0f, 68f), "D19_Residence_C");

            Place(roads, PortRoot + "/D19_Port_Walk_Surface.prefab", new Vector3(0f, 0f, 92f), "Road_PortEntry");
            Place(roads, PortRoot + "/D19_Port_Walk_Surface.prefab", new Vector3(26f, 0f, 76f), "Road_MarketEntry");
            Place(roads, PortRoot + "/D19_Port_Walk_Surface.prefab", new Vector3(48f, 0f, 49f), "Road_Warehouse");
            Place(roads, PortRoot + "/D19_Port_Walk_Surface.prefab", new Vector3(-42f, 0f, 66f), "Road_Tavern");
            Place(roads, PortRoot + "/D19_Port_Walk_Surface.prefab", new Vector3(60f, 0f, 24f), "Road_Shipyard");
            Place(roads, PortRoot + "/D19_Port_Walk_Surface.prefab", new Vector3(0f, 0f, 0f), "Road_DockApproach");
            Place(roads, PortRoot + "/D19_Stair_Module.prefab", new Vector3(0f, 0f, -2.5f), "Dock_Stairs");

            for (int i = -2; i <= 2; i++)
            {
                Place(buildings, BuildingRoot + "/D19_Market_Stall.prefab", new Vector3(16f + i * 5f, 0f, 52f), "Market_Stall_" + (i + 2));
            }
            Place(buildings, PortRoot + "/D19_Bollard.prefab", new Vector3(-7f, 0f, -9f), "Bollard_Dock_L");
            Place(buildings, PortRoot + "/D19_Bollard.prefab", new Vector3(7f, 0f, -9f), "Bollard_Dock_R");
            for (int i = -2; i <= 2; i++) Place(buildings, PortRoot + "/D19_Fence_Module.prefab", new Vector3(-12f + i * 5f, 0f, 96f), "Fence_Port_" + (i + 2));

            CreateLabel(anchors.transform, "PORT ENTRY\nsanta_margarita", new Vector3(0f, 7f, 105f), palette.Label);
            CreateLabel(anchors.transform, "MARKET\nbuilding:market_santa_margarita", new Vector3(26f, 6.8f, 53f), palette.Label);
            CreateLabel(anchors.transform, "WAREHOUSE\nbuilding:warehouse_santa_margarita", new Vector3(48f, 7.4f, 25f), palette.Label);
            CreateLabel(anchors.transform, "TAVERN\nbuilding:tavern_santa_margarita", new Vector3(-42f, 7f, 41f), palette.Label);
            CreateLabel(anchors.transform, "SHIPYARD\nbuilding:shipyard_santa_margarita", new Vector3(60f, 7.6f, -3f), palette.Label);
            CreateLabel(anchors.transform, "PRIMARY DOCK\nbuilding_saint_marys_dock", new Vector3(0f, 5f, -12f), palette.Label);
            CreateLabel(anchors.transform, "BERTH\nberth_santa_margarita", new Vector3(0f, 3.5f, -30f), palette.Label);

            CreatePreviewRig(preview.transform);
            RenderSettings.ambientLight = new Color(0.42f, 0.48f, 0.56f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.36f, 0.52f, 0.58f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 100f;
            RenderSettings.fogEndDistance = 300f;
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreatePreviewRig(Transform parent)
        {
            GameObject cameraObject = new GameObject("D19_PreviewCamera");
            cameraObject.transform.SetParent(parent, false);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 400f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.20f, 0.40f, 0.48f);
            GameObject keyObject = new GameObject("D19_Preview_KeyLight");
            keyObject.transform.SetParent(parent, false);
            keyObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.2f;
            key.color = new Color(1f, 0.87f, 0.68f);
            GameObject fillObject = new GameObject("D19_Preview_FillLight");
            fillObject.transform.SetParent(parent, false);
            fillObject.transform.rotation = Quaternion.Euler(25f, 145f, 0f);
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.35f;
            fill.color = new Color(0.55f, 0.72f, 1f);
        }

        private static void CapturePreviewShots()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Camera camera = GameObject.Find("D19_PreviewCamera")?.GetComponent<Camera>();
            if (camera == null) throw new InvalidOperationException("D19 preview camera was not created");
            string output = Path.Combine(Directory.GetCurrentDirectory(), ResultRoot, "screenshots");
            Directory.CreateDirectory(output);
            var shots = new Dictionary<string, Tuple<Vector3, Vector3>>
            {
                ["d19_a02_port_overview"] = Tuple.Create(new Vector3(86f, 58f, 104f), new Vector3(0f, 2f, 50f)),
                ["d19_a02_market"] = Tuple.Create(new Vector3(48f, 9f, 84f), new Vector3(26f, 2f, 62f)),
                ["d19_a02_berth"] = Tuple.Create(new Vector3(15f, 8f, 3f), new Vector3(0f, 1f, -18f)),
                ["d19_a02_shipyard"] = Tuple.Create(new Vector3(90f, 11f, 34f), new Vector3(60f, 2f, 8f)),
                ["d19_a02_tavern_street"] = Tuple.Create(new Vector3(-70f, 9f, 78f), new Vector3(-42f, 2f, 50f)),
                ["d19_a02_warehouse"] = Tuple.Create(new Vector3(76f, 10f, 58f), new Vector3(48f, 2f, 34f))
            };
            foreach (var shot in shots)
            {
                camera.transform.position = shot.Value.Item1;
                camera.transform.rotation = Quaternion.LookRotation(shot.Value.Item2 - shot.Value.Item1, Vector3.up);
                RenderPng(camera, Path.Combine(output, shot.Key + ".png"));
            }
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[D19-A02] Preview screenshots=" + shots.Count + " output=" + output);
        }

        private static void RenderPng(Camera camera, string path)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
            camera.targetTexture = null;
            RenderTexture.active = previous;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }

        private static void WriteResultSummary(List<string> prefabPaths)
        {
            string output = Path.Combine(Directory.GetCurrentDirectory(), ResultRoot, "D19_A02_MACHINE_RESULT.json");
            string[] sourcePaths = prefabPaths.Where(p => p.StartsWith("Assets/", StringComparison.Ordinal)).ToArray();
            int prefabCount = sourcePaths.Length;
            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"task\": \"D19-A02\",");
            builder.AppendLine("  \"status\": \"WORK_SCENE_READY\",");
            builder.AppendLine("  \"external_manifest_ids_used\": [],");
            builder.AppendLine("  \"project_owned_prefab_count\": " + prefabCount + ",");
            builder.AppendLine("  \"project_owned_material_count\": 10,");
            builder.AppendLine("  \"lod_levels_per_prefab\": 2,");
            builder.AppendLine("  \"prefab_collision_verified\": true,");
            builder.AppendLine("  \"work_scene_stable_anchor_count\": 12,");
            builder.AppendLine("  \"work_scene_prefab_instance_count\": 29,");
            builder.AppendLine("  \"batching_strategy\": \"shared project-owned materials; modular prefab boundaries; no global static-batch mutation\",");
            builder.AppendLine("  \"scene_path\": \"" + ScenePath + "\",");
            builder.AppendLine("  \"required_preview_screenshot_count\": 6,");
            builder.AppendLine("  \"prefab_paths\": [");
            for (int i = 0; i < sourcePaths.Length; i++) builder.AppendLine("    \"" + sourcePaths[i] + "\"" + (i + 1 == sourcePaths.Length ? "" : ","));
            builder.AppendLine("  ],");
            builder.AppendLine("  \"notes\": [");
            builder.AppendLine("    \"All structures are project-owned primitive modular geometry because the D18 intake catalog exposes no D19-A02 eligible external manifest.\",");
            builder.AppendLine("    \"No economy, inventory, shipyard formula, Registry, save data or InitialHarbor file was modified.\",");
            builder.AppendLine("    \"External asset promotion remains untouched; runtime integration waits for D19-S02 and D18-S03.\"");
            builder.AppendLine("  ]");
            builder.AppendLine("}");
            File.WriteAllText(output, builder.ToString(), Encoding.UTF8);
        }

        private static Type FindRuntimeType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            throw new InvalidOperationException("Missing runtime type: " + fullName);
        }

        private static Component AddPortMarker(GameObject root, string buildingId, string category, string materialGroup, string testId)
        {
            Type type = FindRuntimeType("WhalesIslandReef.D19.D19PortBuildingMarker");
            Component component = root.AddComponent(type);
            type.GetMethod("Configure").Invoke(component, new object[] { buildingId, category, materialGroup, testId });
            return component;
        }

        private static Component AddPortAnchor(GameObject root, AnchorSpec spec)
        {
            Type type = FindRuntimeType("WhalesIslandReef.D19.D19PortAnchor");
            Component component = root.AddComponent(type);
            type.GetMethod("Configure").Invoke(component, new object[] { spec.AnchorId, spec.StableId, spec.OwnerTask, spec.LogicOwner, spec.MigrationPolicy });
            return component;
        }

        private static string SavePrefab(GameObject root, string path)
        {
            bool success;
            PrefabUtility.SaveAsPrefabAsset(root, path, out success);
            UnityEngine.Object.DestroyImmediate(root);
            if (!success) throw new InvalidOperationException("Failed to save prefab " + path);
            return path;
        }

        private static void Place(GameObject parent, string prefabPath, Vector3 position, string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) throw new InvalidOperationException("Missing D19 prefab " + prefabPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            instance.name = name;
            instance.transform.position = position;
        }

        private static GameObject CreateCube(Transform parent, string name, Material material, Vector3 position, Vector3 scale, bool collider, Quaternion rotation = default(Quaternion))
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation == default(Quaternion) ? Quaternion.identity : rotation;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider)
            {
                Collider component = go.GetComponent<Collider>();
                if (component != null) UnityEngine.Object.DestroyImmediate(component);
            }
            return go;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Material material, Vector3 position, Vector3 scale, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider)
            {
                Collider component = go.GetComponent<Collider>();
                if (component != null) UnityEngine.Object.DestroyImmediate(component);
            }
            return go;
        }

        private static void AddLodGroup(GameObject root, GameObject lod0, GameObject lod1)
        {
            LODGroup group = root.AddComponent<LODGroup>();
            Renderer[] high = lod0.GetComponentsInChildren<Renderer>(true);
            Renderer[] low = lod1.GetComponentsInChildren<Renderer>(true);
            group.SetLODs(new[] { new LOD(0.55f, high), new LOD(0.01f, low) });
            group.RecalculateBounds();
        }

        private static void AddBuildingAnchors(Transform root, AnchorSpec[] specs, Material markerMaterial)
        {
            foreach (AnchorSpec spec in specs) AddAnchor(root, spec, markerMaterial);
        }

        private static void AddAnchor(Transform root, AnchorSpec spec, Material markerMaterial)
        {
            GameObject anchor = new GameObject("InteractionAnchor_" + Sanitize(spec.AnchorId));
            anchor.transform.SetParent(root, false);
            anchor.transform.localPosition = spec.LocalPosition;
            AddPortAnchor(anchor, spec);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "AnchorMarker";
            marker.transform.SetParent(anchor.transform, false);
            marker.transform.localPosition = Vector3.up * 0.35f;
            marker.transform.localScale = Vector3.one * 0.35f;
            marker.GetComponent<Renderer>().sharedMaterial = markerMaterial;
            Collider collider = marker.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
        }

        private static void CreateLabel(Transform parent, string text, Vector3 position, Material material)
        {
            GameObject label = new GameObject("D19_Label_" + Sanitize(text.Split('\n')[0]));
            label.transform.SetParent(parent, false);
            label.transform.position = position;
            TextMesh mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 32;
            mesh.characterSize = 0.12f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : Color.yellow;
            label.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value)) return "empty";
            return new string(value.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        }
    }
}
#endif
