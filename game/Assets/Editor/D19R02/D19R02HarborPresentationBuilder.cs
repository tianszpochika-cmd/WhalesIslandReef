#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Whalebone.Editor.D19R02
{
    /// <summary>
    /// Builds the R02 presentation layer as one prefab and places one prefab instance in InitialHarbor.
    /// This editor-only builder owns no gameplay facts and is intentionally idempotent.
    /// </summary>
    public static class D19R02HarborPresentationBuilder
    {
        [Serializable]
        private sealed class AuditReport
        {
            public string scene;
            public string root;
            public Vector3 root_position;
            public Vector3 root_scale;
            public int renderer_count;
            public int collider_count;
            public Vector3 world_bounds_center;
            public Vector3 world_bounds_size;
            public Vector3 camera_position;
            public Vector3 camera_forward;
            public int terrain_mesh_vertices;
        }

        [Serializable]
        private sealed class CandidateModelAudit
        {
            public string asset_path;
            public bool loaded;
            public string root_name;
            public int renderer_count;
            public int mesh_filter_count;
            public int material_slot_count;
            public Vector3 local_bounds_center;
            public Vector3 local_bounds_size;
            public string[] renderer_materials;
            public string error;
        }

        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string RootName = "D19_R02_HarborPresentation";
        private const string PrefabDirectory = "Assets/Prefabs/Environment/D19/Integration/R02";
        private const string PrefabPath = PrefabDirectory + "/D19_R02_HarborPresentation.prefab";
        private const string MaterialDirectory = "Assets/Art/Materials/D19/R02";

        private static readonly Color Grass = new Color(0.18f, 0.34f, 0.20f, 1f);
        private static readonly Color GrassLight = new Color(0.30f, 0.48f, 0.25f, 1f);
        private static readonly Color Sand = new Color(0.72f, 0.55f, 0.30f, 1f);
        private static readonly Color SandLight = new Color(0.86f, 0.70f, 0.42f, 1f);
        private static readonly Color Stone = new Color(0.42f, 0.45f, 0.44f, 1f);
        private static readonly Color StoneLight = new Color(0.58f, 0.57f, 0.48f, 1f);
        private static readonly Color Wood = new Color(0.28f, 0.13f, 0.07f, 1f);
        private static readonly Color WoodLight = new Color(0.62f, 0.36f, 0.14f, 1f);
        private static readonly Color Roof = new Color(0.52f, 0.13f, 0.07f, 1f);
        private static readonly Color Plaster = new Color(0.82f, 0.75f, 0.58f, 1f);
        private static readonly Color Water = new Color(0.06f, 0.28f, 0.38f, 1f);
        private static readonly Color Foam = new Color(0.60f, 0.80f, 0.72f, 1f);
        private static readonly Color Leaf = new Color(0.12f, 0.30f, 0.16f, 1f);

        [MenuItem("Whalebone/D19 R02/Build Harbor Presentation")]
        public static void BuildFromMenu()
        {
            Build(ScenePath);
        }

        [MenuItem("Whalebone/D19 R02/Audit Harbor Presentation")]
        public static void AuditFromMenu()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var root = GameObject.Find(RootName);
            if (root == null) throw new InvalidOperationException("Missing scene instance: " + RootName);
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var colliders = root.GetComponentsInChildren<Collider>(true);
            var bounds = new Bounds(root.transform.position, Vector3.zero);
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            var camera = Camera.main;
            var payload = new AuditReport
            {
                scene = ScenePath,
                root = RootName,
                root_position = root.transform.position,
                root_scale = root.transform.lossyScale,
                renderer_count = renderers.Length,
                collider_count = colliders.Length,
                world_bounds_center = bounds.center,
                world_bounds_size = bounds.size,
                camera_position = camera == null ? Vector3.zero : camera.transform.position,
                camera_forward = camera == null ? Vector3.forward : camera.transform.forward,
                terrain_mesh_vertices = GetTerrainVertexCount(root)
            };
            string output = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R02-A04/D19_R02_A04_EDITOR_AUDIT.json";
            System.IO.File.WriteAllText(output, JsonUtility.ToJson(payload, true));
            Debug.Log("[D19-R02-A04] Audit written: " + output);
        }

        [MenuItem("Whalebone/D19 R02/Dump Scene Renderers")]
        public static void DumpSceneRenderers()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var lines = new List<string>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer == null) continue;
                    lines.Add(renderer.transform.name + " | " + GetHierarchyPath(renderer.transform) +
                              " | enabled=" + renderer.enabled + " | type=" + renderer.GetType().Name +
                              " | center=" + renderer.bounds.center.ToString("F2") +
                              " | material=" + GetFirstMaterialName(renderer) +
                              " | color=" + GetFirstMaterialColor(renderer));
                }
            }
            string output = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R02-A04/D19_R02_A04_SCENE_RENDERER_DUMP_RERUN4.txt";
            System.IO.File.WriteAllLines(output, lines.ToArray());
            Debug.Log("[D19-R02-A04] Scene renderer dump written: " + output + " count=" + lines.Count);
        }

        [MenuItem("Whalebone/D19 R02/Audit Intake Environment Models")]
        public static void AuditIntakeEnvironmentModels()
        {
            var paths = new[]
            {
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_default.fbx",
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_detailed_dark.fbx",
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeF.fbx",
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_large_rock.fbx",
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_wood.fbx"
            };
            var audits = new List<CandidateModelAudit>();
            foreach (var path in paths)
            {
                var audit = new CandidateModelAudit { asset_path = path, renderer_materials = Array.Empty<string>() };
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                {
                    audit.error = "AssetDatabase.LoadAssetAtPath<GameObject> returned null";
                    audits.Add(audit);
                    continue;
                }
                audit.loaded = true;
                audit.root_name = asset.name;
                var renderers = asset.GetComponentsInChildren<Renderer>(true);
                var filters = asset.GetComponentsInChildren<MeshFilter>(true);
                var bounds = new Bounds(Vector3.zero, Vector3.zero);
                var materials = new List<string>();
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                    foreach (var material in renderer.sharedMaterials)
                        materials.Add(material == null ? "<missing>" : material.name);
                }
                audit.renderer_count = renderers.Length;
                audit.mesh_filter_count = filters.Length;
                audit.material_slot_count = materials.Count;
                audit.local_bounds_center = bounds.center;
                audit.local_bounds_size = bounds.size;
                audit.renderer_materials = materials.ToArray();
                audits.Add(audit);
            }
            string output = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R02-A04/D19_R02_A04_INTAKE_MODEL_AUDIT_RERUN5.json";
            System.IO.File.WriteAllText(output, JsonUtility.ToJson(new CandidateModelAuditCollection { models = audits.ToArray() }, true));
            Debug.Log("[D19-R02-A04] Intake model audit written: " + output);
        }

        [Serializable]
        private sealed class CandidateModelAuditCollection
        {
            public CandidateModelAudit[] models;
        }

        private static string GetHierarchyPath(Transform transform)
        {
            var stack = new Stack<string>();
            var current = transform;
            while (current != null)
            {
                stack.Push(current.name);
                current = current.parent;
            }
            return string.Join("/", stack.ToArray());
        }

        private static string GetFirstMaterialName(Renderer renderer)
        {
            if (renderer == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                return string.Empty;
            return renderer.sharedMaterials[0] == null ? "<missing>" : renderer.sharedMaterials[0].name;
        }

        private static string GetFirstMaterialColor(Renderer renderer)
        {
            if (renderer == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                return string.Empty;
            var material = renderer.sharedMaterials[0];
            if (material == null) return "<missing>";
            if (material.HasProperty("_BaseColor"))
                return ColorUtility.ToHtmlStringRGBA(material.GetColor("_BaseColor"));
            if (material.HasProperty("_Color"))
                return ColorUtility.ToHtmlStringRGBA(material.GetColor("_Color"));
            return "<no-color-property>";
        }

        private static int GetTerrainVertexCount(GameObject root)
        {
            var terrain = root.transform.Find("D19_R02_TerrainSurface");
            if (terrain == null) return 0;
            var filter = terrain.GetComponent<MeshFilter>();
            return filter == null || filter.sharedMesh == null ? 0 : filter.sharedMesh.vertexCount;
        }

        public static void Build(string scenePath)
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/Environment");
            EnsureFolder("Assets/Prefabs/Environment/D19");
            EnsureFolder("Assets/Prefabs/Environment/D19/Integration");
            EnsureFolder(PrefabDirectory);
            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/Materials");
            EnsureFolder("Assets/Art/Materials/D19");
            EnsureFolder(MaterialDirectory);

            var grass = CreateMaterial("D19_R02_Grass", Grass, 0.0f, 0.05f);
            var grassLight = CreateMaterial("D19_R02_GrassLight", GrassLight, 0.0f, 0.05f);
            var sand = CreateMaterial("D19_R02_Sand", Sand, 0.0f, 0.05f);
            var sandLight = CreateMaterial("D19_R02_SandLight", SandLight, 0.0f, 0.05f);
            var stone = CreateMaterial("D19_R02_Stone", Stone, 0.0f, 0.15f);
            var stoneLight = CreateMaterial("D19_R02_StoneLight", StoneLight, 0.0f, 0.15f);
            var wood = CreateMaterial("D19_R02_Wood", Wood, 0.0f, 0.12f);
            var woodLight = CreateMaterial("D19_R02_WoodLight", WoodLight, 0.0f, 0.12f);
            var roof = CreateMaterial("D19_R02_Roof", Roof, 0.0f, 0.05f);
            var plaster = CreateMaterial("D19_R02_Plaster", Plaster, 0.0f, 0.05f);
            var water = CreateMaterial("D19_R02_Water", Water, 0.0f, 0.55f);
            var foam = CreateMaterial("D19_R02_Foam", Foam, 0.0f, 0.35f);
            var leaf = CreateMaterial("D19_R02_Leaf", Leaf, 0.0f, 0.08f);

            var prefabRoot = new GameObject(RootName);
            prefabRoot.transform.position = Vector3.zero;
            prefabRoot.transform.rotation = Quaternion.identity;
            prefabRoot.transform.localScale = Vector3.one;
            ConfigureRuntimePerformanceProbe(prefabRoot);

            var terrain = CreateTerrainSurface(grass, grassLight, sand, sandLight, stone, stoneLight);
            terrain.transform.SetParent(prefabRoot.transform, false);
            terrain.isStatic = true;

            var coast = CreateWaterAndFoam(water, foam);
            coast.transform.SetParent(prefabRoot.transform, false);

            var path = CreatePath(stoneLight, sandLight);
            path.transform.SetParent(prefabRoot.transform, false);

            var market = CreateMarketCluster(plaster, roof, wood, woodLight, sandLight, water);
            market.transform.SetParent(prefabRoot.transform, false);

            var dock = CreateDockCluster(wood, woodLight, stone, foam);
            dock.transform.SetParent(prefabRoot.transform, false);

            var shore = CreateShoreCluster(stone, stoneLight, sand, sandLight);
            shore.transform.SetParent(prefabRoot.transform, false);

            var vegetation = CreateVegetationCluster(wood, leaf, grassLight);
            vegetation.transform.SetParent(prefabRoot.transform, false);

            var props = CreatePortProps(wood, woodLight, stone, sandLight);
            props.transform.SetParent(prefabRoot.transform, false);

            // Use only audited, intake-eligible Kenney Nature Kit models as a
            // small authored detail layer. The procedural meshes remain the
            // fallback, while these nested model instances break up the broad
            // silhouette without changing any gameplay collider or anchor.
            var candidateDetail = CreateCandidateDetailLayer(wood, leaf, stone, grassLight);
            candidateDetail.transform.SetParent(prefabRoot.transform, false);

            // RERUN6: the earlier procedural market silhouette still read as a
            // single block from the player camera. Add a small authored facade
            // layer made only from project-owned meshes/materials. It is visual
            // presentation inside the R02 integration prefab; it owns no
            // interaction, save, navigation, or economy facts.
            var facadeDetail = CreateFacadeDetailLayer(plaster, roof, wood, woodLight, stone, water);
            facadeDetail.transform.SetParent(prefabRoot.transform, false);

            // RERUN6-FACADE2: reuse the already audited project-owned D19
            // building prefabs as visual-only nested instances. Their source
            // prefabs remain untouched; this integration copy disables every
            // gameplay component and collider so stable anchors and route
            // ownership stay in the existing scene systems.
            var authoredBuildings = CreateAuthoredBuildingVisualLayer();
            authoredBuildings.transform.SetParent(prefabRoot.transform, false);

            MarkStatic(prefabRoot);
            var prefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath, out bool saved);
            if (!saved || prefab == null)
            {
                UnityEngine.Object.DestroyImmediate(prefabRoot);
                throw new InvalidOperationException("D19 R02 prefab could not be saved: " + PrefabPath);
            }

            UnityEngine.Object.DestroyImmediate(prefabRoot);

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            RemoveExistingSceneInstance(scene, RootName);
            var instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException("D19 R02 prefab could not be instantiated into " + scenePath);
            }
            instance.name = RootName;
            instance.transform.position = new Vector3(0f, 0.58f, 0f);
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            HideLegacyVisualRenderers(scene, RootName);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[D19-R02-A04] Built and placed one R02 presentation prefab: " + PrefabPath);
        }

        private static GameObject CreateTerrainSurface(Material grass, Material grassLight, Material sand, Material sandLight, Material stone, Material stoneLight)
        {
            var go = new GameObject("D19_R02_TerrainSurface");
            var filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            var collider = go.AddComponent<MeshCollider>();
            var materials = new[] { grass, grassLight, sand, sandLight, stone, stoneLight };
            var mesh = new Mesh { name = "D19_R02_TerrainSurfaceMesh" };
            mesh.indexFormat = IndexFormat.UInt32;
            var vertices = new List<Vector3>();
            var sub = new List<int>[materials.Length];
            for (int i = 0; i < sub.Length; i++) sub[i] = new List<int>();
            const int width = 22;
            const int depth = 22;
            const float step = 1.8f;
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    float px = (x - (width - 1) * 0.5f) * step;
                    float pz = (z - (depth - 1) * 0.5f) * step;
                    float edge = Mathf.InverseLerp(18.5f, 12.5f, Mathf.Max(Mathf.Abs(px), Mathf.Abs(pz)));
                    float height = 0.025f + 0.08f * Mathf.Sin(px * 0.39f + pz * 0.17f) * Mathf.Sin(pz * 0.24f + 0.4f);
                    if (Mathf.Abs(pz + 8.0f) < 3.1f) height = 0.018f;
                    if (Mathf.Abs(px + 3.5f) < 2.0f && pz < 7.0f) height = 0.012f;
                    int baseIndex = vertices.Count;
                    vertices.Add(new Vector3(px, height, pz));
                    vertices.Add(new Vector3(px + step, height, pz));
                    vertices.Add(new Vector3(px + step, height + 0.01f, pz + step));
                    vertices.Add(new Vector3(px, height + 0.01f, pz + step));
                    int mat = 0;
                    if (Mathf.Abs(pz + 8.0f) < 4.0f) mat = (x + z) % 2 == 0 ? 3 : 2;
                    else if (edge > 0.55f) mat = (x + z) % 3 == 0 ? 5 : 4;
                    else if ((x + z) % 5 == 0) mat = 1;
                    // Counter-clockwise from above: the surface normal must point up.
                    sub[mat].Add(baseIndex);
                    sub[mat].Add(baseIndex + 1);
                    sub[mat].Add(baseIndex + 2);
                    sub[mat].Add(baseIndex);
                    sub[mat].Add(baseIndex + 2);
                    sub[mat].Add(baseIndex + 3);
                }
            }
            mesh.SetVertices(vertices);
            mesh.subMeshCount = materials.Length;
            for (int i = 0; i < sub.Length; i++) mesh.SetTriangles(sub[i], i, true);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            string meshPath = MaterialDirectory + "/D19_R02_TerrainSurfaceMesh.asset";
            AssetDatabase.DeleteAsset(meshPath);
            AssetDatabase.CreateAsset(mesh, meshPath);
            filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            renderer.sharedMaterials = materials;
            collider.sharedMesh = filter.sharedMesh;
            collider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation;
            return go;
        }

        private static GameObject CreateWaterAndFoam(Material water, Material foam)
        {
            var root = new GameObject("D19_R02_WaterAndShoreline");
            var waterGo = AddMesh(root.transform, "ShallowWaterShelf", MakeBoxMesh(28f, 0.03f, 12f), water, new Vector3(0f, -0.06f, 17.0f));
            waterGo.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            waterGo.GetComponent<MeshRenderer>().receiveShadows = false;
            AddMesh(root.transform, "ShorelineFoamA", MakeRibbonMesh(30f, 0.08f, 5.3f), foam, new Vector3(0f, 0.02f, 11.0f));
            AddMesh(root.transform, "ShorelineFoamB", MakeRibbonMesh(14f, 0.06f, 2.1f), foam, new Vector3(-8.0f, 0.04f, 13.8f));
            return root;
        }

        private static GameObject CreatePath(Material stone, Material sand)
        {
            var root = new GameObject("D19_R02_PortRoad");
            var road = AddMesh(root.transform, "MainStreetStone", MakeBoxMesh(7.0f, 0.08f, 24f), stone, new Vector3(1.0f, 0.06f, -1.0f));
            road.GetComponent<MeshRenderer>().sharedMaterial = stone;
            AddMesh(root.transform, "BeachRamp", MakeWedgeMesh(8.0f, 0.12f, 8.0f), sand, new Vector3(-5.4f, 0.06f, -9.0f));
            return root;
        }

        private static GameObject CreateMarketCluster(Material plaster, Material roof, Material wood, Material woodLight, Material sand, Material windowGlass)
        {
            var root = new GameObject("D19_R02_MarketAndStreet");
            AddMesh(root.transform, "MarketFacade", MakeBoxMesh(9.0f, 3.0f, 3.2f), plaster, new Vector3(7.2f, 1.58f, 5.5f));
            AddMesh(root.transform, "MarketRoof", MakeRoofMesh(10.0f, 3.8f, 4.4f), roof, new Vector3(7.2f, 3.25f, 5.5f));
            AddMesh(root.transform, "MarketAwning", MakeBoxMesh(10.2f, 0.18f, 1.2f), woodLight, new Vector3(7.2f, 2.55f, 3.75f));
            AddMesh(root.transform, "MarketCounter", MakeBoxMesh(5.8f, 0.85f, 1.0f), wood, new Vector3(7.2f, 1.0f, 3.0f));
            AddMesh(root.transform, "MarketSign", MakeBoxMesh(3.0f, 0.75f, 0.18f), woodLight, new Vector3(7.2f, 3.3f, 3.2f));
            AddMesh(root.transform, "MarketDoor", MakeBoxMesh(1.35f, 2.15f, 0.16f), wood, new Vector3(7.2f, 1.35f, 3.85f));
            AddMesh(root.transform, "MarketDoorTrim", MakeBevelBoxMesh(1.7f, 2.45f, 0.12f, 0.08f), woodLight, new Vector3(7.2f, 1.45f, 3.72f));
            AddMesh(root.transform, "MarketWindowL", MakeBevelBoxMesh(1.45f, 0.95f, 0.12f, 0.08f), windowGlass, new Vector3(3.95f, 1.65f, 3.88f));
            AddMesh(root.transform, "MarketWindowR", MakeBevelBoxMesh(1.45f, 0.95f, 0.12f, 0.08f), windowGlass, new Vector3(10.45f, 1.65f, 3.88f));
            AddMesh(root.transform, "MarketWindowSillL", MakeBoxMesh(1.8f, 0.12f, 0.28f), woodLight, new Vector3(3.95f, 1.1f, 3.72f));
            AddMesh(root.transform, "MarketWindowSillR", MakeBoxMesh(1.8f, 0.12f, 0.28f), woodLight, new Vector3(10.45f, 1.1f, 3.72f));
            AddMesh(root.transform, "MarketPostL", MakeCylinderMesh(0.12f, 2.0f, 8), wood, new Vector3(2.4f, 1.0f, 3.7f));
            AddMesh(root.transform, "MarketPostR", MakeCylinderMesh(0.12f, 2.0f, 8), wood, new Vector3(12.0f, 1.0f, 3.7f));
            for (int i = -2; i <= 2; i++)
            {
                AddMesh(root.transform, "MarketCrate_" + i, MakeBevelBoxMesh(0.85f, 0.7f, 0.85f, 0.08f), sand, new Vector3(4.2f + i * 1.5f, 0.55f, 2.35f));
            }
            return root;
        }

        private static GameObject CreateDockCluster(Material wood, Material woodLight, Material stone, Material foam)
        {
            var root = new GameObject("D19_R02_DockAndShipyard");
            AddMesh(root.transform, "DockDeck", MakeBoxMesh(9.5f, 0.28f, 4.0f), wood, new Vector3(-1.0f, 0.55f, -8.8f));
            for (int i = -4; i <= 4; i++)
            {
                AddMesh(root.transform, "DockPlank_" + i, MakeBoxMesh(0.16f, 0.12f, 4.4f), woodLight, new Vector3(i * 1.05f - 1.0f, 0.78f, -8.8f));
            }
            AddMesh(root.transform, "ShipyardShed", MakeBoxMesh(5.0f, 2.7f, 4.2f), woodLight, new Vector3(-7.2f, 1.8f, -6.2f));
            AddMesh(root.transform, "ShipyardRoof", MakeRoofMesh(6.0f, 3.2f, 5.3f), wood, new Vector3(-7.2f, 3.42f, -6.2f));
            AddMesh(root.transform, "ShipyardDoor", MakeBoxMesh(1.6f, 2.2f, 0.18f), stone, new Vector3(-7.2f, 1.35f, -4.05f));
            AddMesh(root.transform, "ShipyardBeamL", MakeCylinderMesh(0.13f, 2.7f, 8), wood, new Vector3(-9.1f, 1.55f, -4.08f));
            AddMesh(root.transform, "ShipyardBeamR", MakeCylinderMesh(0.13f, 2.7f, 8), wood, new Vector3(-5.3f, 1.55f, -4.08f));
            AddMesh(root.transform, "BerthStoneEdge", MakeBoxMesh(12.0f, 0.32f, 0.55f), stone, new Vector3(-1.0f, 0.62f, -11.1f));
            AddMesh(root.transform, "BerthFoam", MakeRibbonMesh(11.0f, 0.06f, 0.45f), foam, new Vector3(-1.0f, 0.78f, -11.42f));
            AddMesh(root.transform, "DockPostL", MakeCylinderMesh(0.18f, 1.8f, 8), wood, new Vector3(-5.6f, 0.95f, -10.3f));
            AddMesh(root.transform, "DockPostR", MakeCylinderMesh(0.18f, 1.8f, 8), wood, new Vector3(3.6f, 0.95f, -10.3f));
            AddMesh(root.transform, "DockPostBackL", MakeCylinderMesh(0.15f, 1.45f, 8), woodLight, new Vector3(-5.6f, 0.8f, -7.3f));
            AddMesh(root.transform, "DockPostBackR", MakeCylinderMesh(0.15f, 1.45f, 8), woodLight, new Vector3(3.6f, 0.8f, -7.3f));
            // The old straight cylinder read as a bright debug beam across the
            // player camera. Keep the named presentation child for prefab
            // stability, but leave it disabled until a real rope asset is
            // available; it has no gameplay or collision responsibility.
            var ropeLine = AddMesh(root.transform, "DockRopeLine", MakeCylinderMesh(0.045f, 9.2f, 6), wood, new Vector3(-1.0f, 1.02f, -10.15f));
            ropeLine.transform.Rotate(0f, 0f, 90f);
            ropeLine.SetActive(false);
            AddMesh(root.transform, "DockCargoCrateA", MakeBevelBoxMesh(0.9f, 0.8f, 0.9f, 0.1f), woodLight, new Vector3(-3.9f, 1.0f, -7.6f));
            AddMesh(root.transform, "DockCargoCrateB", MakeBevelBoxMesh(0.9f, 0.8f, 0.9f, 0.1f), stone, new Vector3(2.0f, 1.0f, -7.6f));
            return root;
        }

        private static GameObject CreateShoreCluster(Material stone, Material stoneLight, Material sand, Material sandLight)
        {
            var root = new GameObject("D19_R02_ShoreRocks");
            var positions = new[]
            {
                new Vector3(-13f, 0.65f, 12f), new Vector3(-10f, 0.72f, 14f), new Vector3(-4f, 0.62f, 16f),
                new Vector3(3f, 0.66f, 15f), new Vector3(11f, 0.70f, 13f), new Vector3(15f, 0.73f, 9f),
                new Vector3(-15f, 0.65f, -1f), new Vector3(15f, 0.66f, -2f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                var rock = AddMesh(root.transform, "ShoreRock_" + i, MakeRockMesh(0.9f + (i % 3) * 0.25f), i % 2 == 0 ? stone : stoneLight, positions[i]);
                rock.transform.rotation = Quaternion.Euler(0f, i * 19f, 0f);
            }
            AddMesh(root.transform, "BeachColorBreak", MakeRibbonMesh(22f, 0.08f, 3.2f), sandLight, new Vector3(0f, 0.05f, -6.0f));
            AddMesh(root.transform, "BeachColorBreak2", MakeRibbonMesh(16f, 0.06f, 2.2f), sand, new Vector3(-10f, 0.07f, 0f));
            return root;
        }

        private static GameObject CreateVegetationCluster(Material wood, Material leaf, Material grassLight)
        {
            var root = new GameObject("D19_R02_VegetationBands");
            var palmPositions = new[] { new Vector3(-14f, 0.6f, 7f), new Vector3(-11f, 0.6f, 10f), new Vector3(13f, 0.6f, 8f), new Vector3(15f, 0.6f, 4f) };
            for (int i = 0; i < palmPositions.Length; i++)
            {
                var palm = CreatePalm("Palm_" + i, wood, leaf, palmPositions[i]);
                palm.transform.SetParent(root.transform, true);
            }
            var shrubPositions = new[] { new Vector3(-8f, 0.62f, 5f), new Vector3(-5f, 0.62f, 8f), new Vector3(11f, 0.62f, 9f), new Vector3(4f, 0.62f, 11f), new Vector3(-14f, 0.62f, -4f) };
            for (int i = 0; i < shrubPositions.Length; i++)
            {
                AddMesh(root.transform, "Shrub_" + i, MakeShrubMesh(0.8f + (i % 2) * 0.2f), leaf, shrubPositions[i]);
            }
            AddMesh(root.transform, "Broadleaf_0", MakeBroadleafMesh(1.15f), leaf, new Vector3(-9.5f, 1.0f, 12f));
            AddMesh(root.transform, "Broadleaf_1", MakeBroadleafMesh(1.35f), leaf, new Vector3(8.5f, 1.1f, 12.5f));
            AddMesh(root.transform, "Broadleaf_2", MakeBroadleafMesh(1.0f), leaf, new Vector3(14f, 1.0f, 11.5f));
            AddMesh(root.transform, "GrassBand", MakeRibbonMesh(24f, 0.05f, 1.6f), grassLight, new Vector3(0f, 0.05f, 5.5f));
            return root;
        }

        private static GameObject CreatePortProps(Material wood, Material woodLight, Material stone, Material sand)
        {
            var root = new GameObject("D19_R02_PortProps");
            var propPositions = new[] { new Vector3(1.8f, 0.75f, -5.8f), new Vector3(4.1f, 0.75f, -5.8f), new Vector3(-4.1f, 0.78f, -7.0f) };
            for (int i = 0; i < propPositions.Length; i++)
            {
                AddMesh(root.transform, "CargoCrate_" + i, MakeBevelBoxMesh(1.0f, 1.0f, 1.0f, 0.1f), i % 2 == 0 ? woodLight : sand, propPositions[i]);
            }
            AddMesh(root.transform, "StoneMarker", MakeRockMesh(0.8f), stone, new Vector3(10.5f, 0.65f, -3.5f));
            AddMesh(root.transform, "NoticeBoard", MakeBoxMesh(1.6f, 1.7f, 0.16f), wood, new Vector3(4.0f, 1.55f, -2.0f));
            AddMesh(root.transform, "NoticeRoof", MakeRoofMesh(2.1f, 0.6f, 0.8f), woodLight, new Vector3(4.0f, 2.5f, -2.0f));
            return root;
        }

        private static GameObject CreateCandidateDetailLayer(Material wood, Material leaf, Material stone, Material grassLight)
        {
            var root = new GameObject("D19_R02_A04_IntakeDetail");
            AddCandidateModel(root.transform,
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_default.fbx",
                "IntakeTreeDefault_West", new Vector3(-13.8f, 0.64f, 10.8f),
                new Vector3(2.45f, 2.45f, 2.45f), 18f, wood, leaf, stone, grassLight);
            AddCandidateModel(root.transform,
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_detailed_dark.fbx",
                "IntakeTreeDetailed_East", new Vector3(13.0f, 0.64f, 11.0f),
                new Vector3(2.85f, 2.85f, 2.85f), -22f, wood, leaf, stone, grassLight);
            AddCandidateModel(root.transform,
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_large_rock.fbx",
                "IntakeCliffRock_West", new Vector3(-15.2f, 0.60f, 13.2f),
                new Vector3(2.9f, 2.1f, 2.9f), 12f, wood, leaf, stone, grassLight);
            AddCandidateModel(root.transform,
                "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeF.fbx",
                "IntakeRock_East", new Vector3(15.0f, 0.62f, 12.8f),
                new Vector3(2.2f, 2.2f, 2.2f), -15f, wood, leaf, stone, grassLight);
            return root;
        }

        private static GameObject CreateFacadeDetailLayer(Material plaster, Material roof, Material wood,
            Material woodLight, Material stone, Material glass)
        {
            var root = new GameObject("D19_R02_A04_FacadeDetail");

            CreateFacadeUnit(root.transform, "MarketHouseDetail", new Vector3(5.2f, 0f, 1.8f),
                new Vector3(6.8f, 4.2f, 4.2f), plaster, roof, wood, woodLight, stone, glass, true);
            CreateFacadeUnit(root.transform, "WarehouseHouseDetail", new Vector3(-5.2f, 0f, 2.0f),
                new Vector3(7.6f, 3.7f, 4.8f), stone, wood, wood, woodLight, plaster, glass, false);
            CreateFacadeUnit(root.transform, "TavernHouseDetail", new Vector3(0.0f, 0f, 5.6f),
                new Vector3(7.4f, 4.6f, 4.8f), plaster, roof, wood, woodLight, stone, glass, true);
            return root;
        }

        private static GameObject CreateAuthoredBuildingVisualLayer()
        {
            var root = new GameObject("D19_R02_A04_AuthoredBuildingVisuals");
            AddAuthoredBuilding(root.transform, "MarketVisualPrefab",
                "Assets/Prefabs/Buildings/D19/D19_Market.prefab",
                new Vector3(5.4f, 0.58f, 1.8f), Vector3.one, 0f);
            AddAuthoredBuilding(root.transform, "TavernVisualPrefab",
                "Assets/Prefabs/Buildings/D19/D19_Tavern_Exterior.prefab",
                new Vector3(-5.4f, 0.58f, 2.0f), Vector3.one, 0f);
            AddAuthoredBuilding(root.transform, "WarehouseVisualPrefab",
                "Assets/Prefabs/Buildings/D19/D19_Warehouse.prefab",
                new Vector3(0.0f, 0.58f, 5.6f), Vector3.one, 0f);
            AddAuthoredBuilding(root.transform, "ShipyardVisualPrefab",
                "Assets/Prefabs/Buildings/D19/D19_Shipyard.prefab",
                new Vector3(-7.0f, 0.58f, -4.8f), Vector3.one, 0f);
            return root;
        }

        private static GameObject AddAuthoredBuilding(Transform parent, string name, string assetPath,
            Vector3 position, Vector3 scale, float yaw)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null)
            {
                Debug.LogWarning("[D19-R02-A04] Authored project building unavailable: " + assetPath);
                return null;
            }

            // Clone the audited project prefab as a local visual copy. Using
            // Object.Instantiate here avoids a nested-prefab LOD instance
            // becoming an editor-only override when the integration prefab is
            // serialized for a standalone Player build.
            var instance = UnityEngine.Object.Instantiate(asset);
            if (instance == null)
            {
                Debug.LogWarning("[D19-R02-A04] Authored project building could not be instantiated: " + assetPath);
                return null;
            }

            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = scale;

            foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
            foreach (var behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
                behaviour.enabled = false;

            return instance;
        }

        private static void CreateFacadeUnit(Transform parent, string name, Vector3 position, Vector3 size,
            Material wall, Material roof, Material trim, Material accent, Material sill, Material glass,
            bool awning)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;

            float width = size.x;
            float height = size.y;
            float depth = size.z;
            float wallThickness = 0.22f;
            float frontZ = -depth * 0.5f;
            float backZ = depth * 0.5f;
            float sideX = width * 0.5f;

            // Split walls into panels so the silhouette has doors, windows and
            // readable structural rhythm instead of one unbroken cube.
            AddMesh(root.transform, "WallBack", MakeBevelBoxMesh(width, height, wallThickness, 0.12f),
                wall, new Vector3(0f, height * 0.5f, backZ));
            AddMesh(root.transform, "WallLeft", MakeBevelBoxMesh(wallThickness, height, depth, 0.08f),
                wall, new Vector3(-sideX, height * 0.5f, 0f));
            AddMesh(root.transform, "WallRight", MakeBevelBoxMesh(wallThickness, height, depth, 0.08f),
                wall, new Vector3(sideX, height * 0.5f, 0f));

            float panelWidth = (width - 2.0f) * 0.5f;
            AddMesh(root.transform, "FrontPanelL", MakeBevelBoxMesh(panelWidth, height, wallThickness, 0.12f),
                wall, new Vector3(-(panelWidth * 0.5f + 0.5f), height * 0.5f, frontZ));
            AddMesh(root.transform, "FrontPanelR", MakeBevelBoxMesh(panelWidth, height, wallThickness, 0.12f),
                wall, new Vector3(panelWidth * 0.5f + 0.5f, height * 0.5f, frontZ));

            // A doorway and two contrasting windows make the entrance legible
            // at the fixed camera distance without adding an interaction.
            AddMesh(root.transform, "Door", MakeBevelBoxMesh(1.15f, 2.15f, 0.16f, 0.06f),
                trim, new Vector3(0f, 1.075f, frontZ - 0.16f));
            AddMesh(root.transform, "DoorHeader", MakeBevelBoxMesh(1.55f, 0.18f, 0.22f, 0.05f),
                accent, new Vector3(0f, 2.25f, frontZ - 0.17f));
            AddMesh(root.transform, "WindowL", MakeBevelBoxMesh(1.15f, 0.85f, 0.12f, 0.06f),
                glass, new Vector3(-width * 0.30f, height * 0.58f, frontZ - 0.16f));
            AddMesh(root.transform, "WindowR", MakeBevelBoxMesh(1.15f, 0.85f, 0.12f, 0.06f),
                glass, new Vector3(width * 0.30f, height * 0.58f, frontZ - 0.16f));
            AddMesh(root.transform, "WindowSillL", MakeBoxMesh(1.45f, 0.12f, 0.22f),
                accent, new Vector3(-width * 0.30f, height * 0.39f, frontZ - 0.18f));
            AddMesh(root.transform, "WindowSillR", MakeBoxMesh(1.45f, 0.12f, 0.22f),
                accent, new Vector3(width * 0.30f, height * 0.39f, frontZ - 0.18f));

            // Two roof planes, ridge beam and eaves read as a pitched Caribbean
            // roof instead of a flat cap. The geometry remains low-poly and
            // deterministic for the D18/D19 budget.
            AddMesh(root.transform, "Roof", MakeRoofMesh(width + 0.7f, height + 0.15f, depth + 0.8f),
                roof, new Vector3(0f, height + 0.25f, 0f));
            AddMesh(root.transform, "RoofRidge", MakeBoxMesh(width * 0.72f, 0.18f, 0.22f),
                accent, new Vector3(0f, height + 1.08f, 0f));
            AddMesh(root.transform, "EaveFront", MakeBoxMesh(width + 0.9f, 0.16f, 0.34f),
                trim, new Vector3(0f, height + 0.18f, frontZ - 0.18f));

            if (awning)
            {
                AddMesh(root.transform, "Awning", MakeRoofMesh(width * 0.64f, 0.65f, 1.55f),
                    accent, new Vector3(0f, height * 0.62f, frontZ - 0.78f));
                AddMesh(root.transform, "AwningPostL", MakeCylinderMesh(0.07f, height * 0.62f, 8),
                    trim, new Vector3(-width * 0.31f, height * 0.31f, frontZ - 1.48f));
                AddMesh(root.transform, "AwningPostR", MakeCylinderMesh(0.07f, height * 0.62f, 8),
                    trim, new Vector3(width * 0.31f, height * 0.31f, frontZ - 1.48f));
            }

            AddMesh(root.transform, "Sign", MakeBevelBoxMesh(2.15f, 0.58f, 0.12f, 0.05f),
                accent, new Vector3(0f, height + 0.1f, frontZ - 0.28f));
            AddMesh(root.transform, "Step", MakeBevelBoxMesh(1.6f, 0.18f, 0.7f, 0.05f),
                sill, new Vector3(0f, 0.1f, frontZ - 0.45f));

            // Small cargo/readability props give the market and warehouse a
            // human scale without becoming gameplay objects.
            AddMesh(root.transform, "CrateL", MakeBevelBoxMesh(0.7f, 0.65f, 0.7f, 0.08f),
                trim, new Vector3(-width * 0.36f, 0.36f, frontZ - 0.7f));
            AddMesh(root.transform, "CrateR", MakeBevelBoxMesh(0.7f, 0.65f, 0.7f, 0.08f),
                sill, new Vector3(width * 0.36f, 0.36f, frontZ - 0.7f));
        }

        private static GameObject AddCandidateModel(Transform parent, string assetPath, string name,
            Vector3 position, Vector3 scale, float yaw, Material wood, Material leaf,
            Material stone, Material grassLight)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null)
            {
                Debug.LogWarning("[D19-R02-A04] Intake model unavailable; keeping procedural fallback: " + assetPath);
                return null;
            }
            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null)
            {
                Debug.LogWarning("[D19-R02-A04] Intake model could not be instantiated; keeping procedural fallback: " + assetPath);
                return null;
            }
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = scale;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                var sourceMaterials = renderer.sharedMaterials;
                var mappedMaterials = new Material[sourceMaterials.Length];
                for (int i = 0; i < sourceMaterials.Length; i++)
                {
                    var materialName = sourceMaterials[i] == null
                        ? string.Empty
                        : sourceMaterials[i].name.ToLowerInvariant();
                    if (materialName.Contains("leaf")) mappedMaterials[i] = leaf;
                    else if (materialName.Contains("wood")) mappedMaterials[i] = wood;
                    else if (materialName.Contains("grass")) mappedMaterials[i] = grassLight;
                    else mappedMaterials[i] = stone;
                }
                renderer.sharedMaterials = mappedMaterials;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            return instance;
        }

        private static GameObject CreatePalm(string name, Material wood, Material leaf, Vector3 position)
        {
            var root = new GameObject(name);
            root.transform.position = position;
            var trunk = AddMesh(root.transform, "Trunk", MakeCylinderMesh(0.20f, 4.0f, 7), wood, new Vector3(0f, 2f, 0f));
            trunk.transform.rotation = Quaternion.Euler(-5f, 0f, 5f);
            for (int i = 0; i < 6; i++)
            {
                var angle = i * 60f * Mathf.Deg2Rad;
                var leafPos = new Vector3(Mathf.Cos(angle) * 0.92f, 3.75f, Mathf.Sin(angle) * 0.92f);
                var blade = AddMesh(root.transform, "Leaf_" + i, MakeBladeMesh(1.15f, 0.14f), leaf, leafPos);
                blade.transform.rotation = Quaternion.Euler(10f, -i * 60f, 18f * Mathf.Sin(angle));
            }
            return root;
        }

        private static GameObject AddMesh(Transform parent, string name, Mesh mesh, Material material, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            if (mesh.bounds.size.magnitude > 4f)
            {
                var lod = go.AddComponent<LODGroup>();
                // Keep a low-cost far representation visible in the harbor establishing shot.
                // An empty final LOD made the previous pass disappear from the real Player camera.
                lod.SetLODs(new[] { new LOD(0.55f, new[] { renderer }), new LOD(0.08f, new[] { renderer }), new LOD(0.008f, new[] { renderer }) });
                lod.RecalculateBounds();
            }
            return go;
        }

        private static void RemoveExistingSceneInstance(SceneAsset sceneAsset, string rootName)
        {
            // Kept for signature clarity; the scene is inspected by the overload below.
        }

        private static void RemoveExistingSceneInstance(UnityEngine.SceneManagement.Scene scene, string rootName)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == rootName)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static Material CreateMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = MaterialDirectory + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            return material;
        }

        private static Mesh MakeBoxMesh(float width, float height, float depth)
        {
            float x = width * 0.5f, y = height * 0.5f, z = depth * 0.5f;
            var v = new[] { new Vector3(-x,-y,-z), new Vector3(x,-y,-z), new Vector3(x,-y,z), new Vector3(-x,-y,z), new Vector3(-x,y,-z), new Vector3(x,y,-z), new Vector3(x,y,z), new Vector3(-x,y,z) };
            return MakeMesh(v, new[] { 0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,1,2,6,1,6,5,2,3,7,2,7,6,3,0,4,3,4,7 });
        }

        private static Mesh MakeBevelBoxMesh(float width, float height, float depth, float bevel)
        {
            // A compact chamfered box keeps props from reading as debug cubes.
            float x = width * 0.5f, y = height * 0.5f, z = depth * 0.5f;
            var v = new[] {
                new Vector3(-x+bevel,-y,-z), new Vector3(x-bevel,-y,-z), new Vector3(x,-y,-z+bevel), new Vector3(x,-y,z-bevel), new Vector3(x-bevel,-y,z), new Vector3(-x+bevel,-y,z), new Vector3(-x,-y,z-bevel), new Vector3(-x,-y,-z+bevel),
                new Vector3(-x+bevel,y,-z), new Vector3(x-bevel,y,-z), new Vector3(x,y,-z+bevel), new Vector3(x,y,z-bevel), new Vector3(x-bevel,y,z), new Vector3(-x+bevel,y,z), new Vector3(-x,y,z-bevel), new Vector3(-x,y,-z+bevel)
            };
            var t = new List<int>();
            for (int i=0;i<8;i++){int n=(i+1)%8;t.Add(i);t.Add(n);t.Add(8+n);t.Add(i);t.Add(8+n);t.Add(8+i);}
            t.AddRange(new[]{8,9,10,8,10,11,8,11,12,8,12,13,8,13,14,8,14,15});
            t.AddRange(new[]{0,7,6,0,6,5,0,5,4,0,4,3,0,3,2,0,2,1});
            return MakeMesh(v, t.ToArray());
        }

        private static Mesh MakeRoofMesh(float width, float height, float depth)
        {
            float x=width*0.5f,z=depth*0.5f;
            var v=new[]{new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(x,0,z),new Vector3(-x,0,z),new Vector3(0,height,0)};
            return MakeMesh(v,new[]{0,1,4,1,2,4,2,3,4,3,0,4,0,3,2,0,2,1});
        }

        private static Mesh MakeWedgeMesh(float width, float height, float depth)
        {
            float x=width*0.5f,z=depth*0.5f;
            var v=new[]{new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(x,0,z),new Vector3(-x,0,z),new Vector3(-x,height,z),new Vector3(x,height,z)};
            return MakeMesh(v,new[]{0,1,2,0,2,3,3,2,5,3,5,4,0,3,4,0,4,1,1,4,5,1,5,2});
        }

        private static Mesh MakeRibbonMesh(float width, float height, float depth)
        {
            float x=width*0.5f,z=depth*0.5f;
            var v=new[]{new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(x,height,z),new Vector3(-x,height,z)};
            return MakeMesh(v,new[]{0,1,2,0,2,3});
        }

        private static Mesh MakeRockMesh(float radius)
        {
            var v=new[]{new Vector3(-radius*0.8f,0,-radius*0.6f),new Vector3(radius*0.8f,0,-radius*0.5f),new Vector3(radius,0,radius*0.4f),new Vector3(-radius*0.7f,0,radius*0.6f),new Vector3(-radius*0.25f,radius*1.25f,-radius*0.15f),new Vector3(radius*0.35f,radius*0.95f,radius*0.25f)};
            return MakeMesh(v,new[]{0,1,4,1,2,5,1,5,4,2,3,5,3,0,4,3,4,5,0,3,2,0,2,1});
        }

        private static Mesh MakeShrubMesh(float radius)
        {
            var v=new[]{new Vector3(-radius,0,0),new Vector3(radius,0,0),new Vector3(0,0,-radius),new Vector3(0,0,radius),new Vector3(0,radius*1.2f,0)};
            return MakeMesh(v,new[]{0,1,4,1,3,4,3,2,4,2,0,4,0,2,3,0,3,1});
        }

        private static Mesh MakeBroadleafMesh(float radius)
        {
            var v = new[]
            {
                new Vector3(-radius, 0f, 0f), new Vector3(radius, 0.1f, 0f),
                new Vector3(0f, 0.15f, -radius), new Vector3(0f, 0.12f, radius),
                new Vector3(0f, radius * 1.45f, 0f), new Vector3(-radius * 0.45f, radius * 0.72f, -radius * 0.35f),
                new Vector3(radius * 0.5f, radius * 0.8f, radius * 0.35f)
            };
            return MakeMesh(v, new[]
            {
                0, 1, 5, 1, 2, 5, 2, 0, 5, 1, 3, 6, 3, 0, 6, 0, 1, 6,
                5, 2, 4, 2, 3, 4, 3, 6, 4, 6, 1, 4, 4, 0, 5, 4, 5, 2
            });
        }

        private static Mesh MakeCylinderMesh(float radius, float height, int segments)
        {
            var v=new List<Vector3>(); var t=new List<int>();
            for(int i=0;i<segments;i++){float a=i*Mathf.PI*2f/segments;v.Add(new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));v.Add(new Vector3(Mathf.Cos(a)*radius,height,Mathf.Sin(a)*radius));}
            for(int i=0;i<segments;i++){int n=(i+1)%segments; t.Add(i*2);t.Add(n*2);t.Add(n*2+1);t.Add(i*2);t.Add(n*2+1);t.Add(i*2+1);}
            return MakeMesh(v.ToArray(),t.ToArray());
        }

        private static Mesh MakeBladeMesh(float length, float width)
        {
            var v=new[]{new Vector3(0,0,0),new Vector3(length*0.65f,width,0),new Vector3(length,0.02f,0),new Vector3(length*0.65f,-width,0),new Vector3(length*0.55f,0,0.12f)};
            return MakeMesh(v,new[]{0,1,4,1,2,4,2,3,4,3,0,4});
        }

        private static Mesh MakeMesh(Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void MarkStatic(GameObject root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.isStatic = true;
            }
        }

        private static void ConfigureRuntimePerformanceProbe(GameObject prefabRoot)
        {
            var probe = prefabRoot.AddComponent<Whalebone.Gameplay.D16.D16RuntimePerformanceProbe>();
            var serialized = new SerializedObject(probe);
            var interval = serialized.FindProperty("_sampleInterval");
            if (interval != null) interval.floatValue = 0.25f;
            var writeOnDestroy = serialized.FindProperty("_writeReportOnDestroy");
            if (writeOnDestroy != null) writeOnDestroy.boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void HideLegacyVisualRenderers(UnityEngine.SceneManagement.Scene scene, string presentationRootName)
        {
            var legacyNames = new HashSet<string>(StringComparer.Ordinal)
            {
                "Beach", "Sea", "Dock", "DockDeck", "Gangplank", "HandRail",
                "Barrel", "Barrel2", "Crate", "Crate2", "Bollard", "Pile"
            };
            var legacySubtrees = new HashSet<string>(StringComparer.Ordinal)
            {
                // These R01-B subtrees contain the old colormap/red proxy meshes.
                // Keep stable anchors and the audited project-owned architecture; replace
                // only the visually misleading proxy layers with the R02 presentation.
                "D19_R01_B_DockMarketProps_Approved",
                "D19_R01_B_PortModules_Approved"
            };
            int hidden = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null || root.name == presentationRootName) continue;
                if (root.name == "D19_R01_B_HarborVisualRepair")
                {
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    {
                        if (renderer == null || !renderer.enabled) continue;
                        renderer.enabled = false;
                        hidden++;
                    }
                    continue;
                }
                foreach (var child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "D19_R01_B_HarborVisualRepair")
                    {
                        foreach (var renderer in child.GetComponentsInChildren<Renderer>(true))
                        {
                            if (renderer == null || !renderer.enabled) continue;
                            renderer.enabled = false;
                            hidden++;
                        }
                        continue;
                    }
                    if (legacySubtrees.Contains(child.name))
                    {
                        foreach (var renderer in child.GetComponentsInChildren<Renderer>(true))
                        {
                            if (renderer == null || !renderer.enabled) continue;
                            renderer.enabled = false;
                            hidden++;
                        }
                        continue;
                    }
                    if (!legacyNames.Contains(child.name)) continue;
                    foreach (var renderer in child.GetComponentsInChildren<Renderer>(true))
                    {
                        if (renderer == null || !renderer.enabled) continue;
                        renderer.enabled = false;
                        hidden++;
                    }
                }
            }
            Debug.Log("[D19-R02-A04] Legacy visual renderers hidden while compatibility objects remain: " + hidden);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
