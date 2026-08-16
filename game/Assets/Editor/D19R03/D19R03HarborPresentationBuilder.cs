#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Whalebone.Editor.D19R03
{
    /// <summary>
    /// Builds the R03 visual layer only. Gameplay anchors, colliders, save
    /// facts and D19-A01—A06 source scenes remain outside this prefab.
    /// </summary>
    public static class D19R03HarborPresentationBuilder
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string RootName = "D19_R03_HarborPresentation";
        private const string PrefabDirectory = "Assets/Prefabs/Environment/D19/Integration/R03";
        private const string PrefabPath = PrefabDirectory + "/D19_R03_HarborPresentation.prefab";
        private const string ShipPrefabPath = PrefabDirectory + "/D19_R03_ShipVisual.prefab";
        private const string MaterialDirectory = "Assets/Art/Materials/D19/R03";
        private const string MeshDirectory = MaterialDirectory + "/Meshes";
        private static int _meshSequence;

        // Keep the stylized palette readable under the project's URP lighting.
        // The previous values were clipped into large orange/white blocks in a
        // real Player, especially on the sand and roof planes.
        private static readonly Color Grass = new Color(0.13f, 0.23f, 0.13f, 1f);
        private static readonly Color GrassLight = new Color(0.25f, 0.35f, 0.18f, 1f);
        private static readonly Color Earth = new Color(0.28f, 0.17f, 0.09f, 1f);
        private static readonly Color Sand = new Color(0.57f, 0.43f, 0.25f, 1f);
        private static readonly Color SandLight = new Color(0.72f, 0.57f, 0.34f, 1f);
        private static readonly Color Stone = new Color(0.24f, 0.28f, 0.28f, 1f);
        private static readonly Color StoneLight = new Color(0.42f, 0.43f, 0.38f, 1f);
        private static readonly Color Wood = new Color(0.18f, 0.075f, 0.03f, 1f);
        private static readonly Color WoodLight = new Color(0.30f, 0.17f, 0.095f, 1f);
        private static readonly Color Plaster = new Color(0.52f, 0.43f, 0.32f, 1f);
        private static readonly Color Roof = new Color(0.20f, 0.12f, 0.075f, 1f);
        private static readonly Color Teal = new Color(0.035f, 0.17f, 0.22f, 1f);
        private static readonly Color Foam = new Color(0.48f, 0.62f, 0.64f, 1f);
        private static readonly Color Leaf = new Color(0.10f, 0.28f, 0.12f, 1f);

        [MenuItem("Whalebone/D19 R03/Build Harbor Presentation")]
        public static void BuildFromMenu() { Build(ScenePath); }

        [MenuItem("Whalebone/D19 R03/Audit Harbor Presentation")]
        public static void AuditFromMenu() { Audit(ScenePath); }

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
            EnsureFolder(MeshDirectory);
            _meshSequence = 0;

            var grass = Material("R03_Grass", Grass, 0.0f, 0.08f);
            var grassLight = Material("R03_GrassLight", GrassLight, 0.0f, 0.08f);
            var earth = Material("R03_Earth", Earth, 0.0f, 0.12f);
            var sand = Material("R03_Sand", Sand, 0.0f, 0.15f);
            var sandLight = Material("R03_SandLight", SandLight, 0.0f, 0.15f);
            var stone = Material("R03_Stone", Stone, 0.0f, 0.18f);
            var stoneLight = Material("R03_StoneLight", StoneLight, 0.0f, 0.18f);
            var wood = Material("R03_Wood", Wood, 0.0f, 0.18f);
            var woodLight = Material("R03_WoodLight", WoodLight, 0.0f, 0.18f);
            var plaster = Material("R03_Plaster", Plaster, 0.0f, 0.12f);
            var roof = Material("R03_Roof", Roof, 0.0f, 0.10f);
            var teal = Material("R03_Teal", Teal, 0.0f, 0.45f);
            var foam = Material("R03_Foam", Foam, 0.0f, 0.25f);
            var leaf = Material("R03_Leaf", Leaf, 0.0f, 0.08f);
            var sail = Material("R03_Sail", new Color(0.78f, 0.72f, 0.56f, 1f), 0.0f, 0.10f);

            var prefabRoot = new GameObject(RootName);
            prefabRoot.transform.position = Vector3.zero;
            prefabRoot.transform.rotation = Quaternion.identity;
            prefabRoot.transform.localScale = Vector3.one;
            prefabRoot.AddComponent<Whalebone.Gameplay.D19.R03.D19R03HarborRuntimePresentation>();

            // The R03 visual island is intentionally mesh-only, but the
            // approved Player route still needs a real walkable surface between
            // DockGround and the frozen ShoreBoundary.  Keep this collider in
            // the R03 integration prefab, name it with the existing D2/D6
            // walkable-surface contract, and do not add a second movement or
            // navigation authority.  It has no Renderer and cannot promote
            // any resource state.
            AddRouteGroundCollider(prefabRoot.transform);

            var island = CreateIslandSurface(grass, grassLight, earth, sand, sandLight, stone, stoneLight);
            island.transform.SetParent(prefabRoot.transform, false);
            var shore = CreateShoreline(sandLight, stone, foam, teal);
            shore.transform.SetParent(prefabRoot.transform, false);
            var roads = CreateRoadNetwork(earth, sandLight, stoneLight);
            roads.transform.SetParent(prefabRoot.transform, false);
            var market = CreateHouse("MarketVisual", new Vector3(5.0f, 0.22f, 0.35f), new Vector3(5.8f, 3.8f, 4.5f), plaster, roof, wood, woodLight, sandLight, "MARKET");
            market.transform.SetParent(prefabRoot.transform, false);
            var warehouse = CreateHouse("WarehouseVisual", new Vector3(-5.0f, 0.22f, 0.35f), new Vector3(5.4f, 3.9f, 4.2f), woodLight, roof, wood, stoneLight, sandLight, "WAREHOUSE");
            warehouse.transform.SetParent(prefabRoot.transform, false);
            var tavern = CreateHouse("TavernVisual", new Vector3(0f, 0.22f, 4.15f), new Vector3(5.0f, 3.7f, 4.2f), plaster, roof, wood, woodLight, sandLight, "TAVERN");
            tavern.transform.SetParent(prefabRoot.transform, false);
            var shipyard = CreateHouse("ShipyardVisual", new Vector3(-7.0f, 0.22f, -3.6f), new Vector3(4.6f, 3.4f, 3.6f), woodLight, roof, wood, stoneLight, sandLight, "SHIPYARD");
            shipyard.transform.SetParent(prefabRoot.transform, false);
            var rope = Material("R03_Rope", new Color(0.32f, 0.18f, 0.075f, 1f), 0.0f, 0.12f);
            var dock = CreateDock(wood, woodLight, stone, rope);
            dock.transform.SetParent(prefabRoot.transform, false);
            var plants = CreateVegetation(wood, leaf, grassLight, stoneLight);
            plants.transform.SetParent(prefabRoot.transform, false);
            var props = CreatePortProps(wood, woodLight, stone, sandLight);
            props.transform.SetParent(prefabRoot.transform, false);
            var entry = CreateMarketEntry(wood, woodLight, stone, sandLight);
            entry.transform.SetParent(prefabRoot.transform, false);
            var backdrop = CreateHarborBackdrop(earth, stone, grassLight, leaf);
            backdrop.transform.SetParent(prefabRoot.transform, false);
            AddCandidateDetails(prefabRoot.transform, wood, leaf, stone, grassLight);

            foreach (var transform in prefabRoot.GetComponentsInChildren<Transform>(true))
                transform.gameObject.isStatic = true;

            var prefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath, out bool saved);
            if (!saved || prefab == null)
                throw new InvalidOperationException("Could not save R03 presentation prefab: " + PrefabPath);
            UnityEngine.Object.DestroyImmediate(prefabRoot);

            var shipPrefab = BuildShipPrefab(wood, woodLight, stone, foam, sail);

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            RemoveSceneInstance(scene, RootName);
            var instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null) throw new InvalidOperationException("Could not instantiate " + PrefabPath);
            instance.name = RootName;
            instance.transform.position = new Vector3(0f, 0.58f, 0f);
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            HidePresentationRenderers(scene, "D19_R02_HarborPresentation");
            HidePresentationRenderers(scene, "D19_R01_B_HarborVisualRepair");
            HideLegacyVisuals(scene);
            InstallShipPresentation(scene, shipPrefab);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[D19-R03-A02] Built and placed " + PrefabPath);
        }

        private static void AddRouteGroundCollider(Transform parent)
        {
            var ground = new GameObject("D19_TerrainRoot");
            ground.transform.SetParent(parent, false);
            ground.transform.localPosition = new Vector3(0f, 0.05f, 5.0f);
            ground.transform.localRotation = Quaternion.identity;
            ground.transform.localScale = Vector3.one;
            ground.isStatic = true;
            var collider = ground.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = new Vector3(32f, 0.20f, 24f);
            collider.isTrigger = false;
        }

        public static void Audit(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var root = GameObject.Find(RootName);
            if (root == null) throw new InvalidOperationException("Missing " + RootName);
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var colliders = root.GetComponentsInChildren<Collider>(true);
            var bounds = new Bounds(root.transform.position, Vector3.zero);
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            int vertices = 0;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh != null) vertices += filter.sharedMesh.vertexCount;
            var report = new Dictionary<string, object>
            {
                ["schema"] = "D19_R03_A02_EDITOR_AUDIT_V1",
                ["scene"] = scene.path,
                ["root"] = RootName,
                ["renderer_count"] = renderers.Length,
                ["collider_count"] = colliders.Length,
                ["mesh_vertex_sum"] = vertices,
                ["bounds_center"] = new [] { bounds.center.x, bounds.center.y, bounds.center.z },
                ["bounds_size"] = new [] { bounds.size.x, bounds.size.y, bounds.size.z },
                ["scene_root_names"] = SceneRootSummary(scene),
                ["external_candidate_models"] = new [] {
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_default.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_small_dark.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeF.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_large_rock.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/grass_large.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathBend.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_stepsCorner_stone.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallF.fbx"
                }
            };
            string output = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(output));
            System.IO.File.WriteAllText(output, MiniJson(report));
            Debug.Log("[D19-R03-A02] Audit written: " + output);
        }

        public static void AuditSceneColliders()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var rows = new List<SceneColliderAuditRow>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                {
                    if (collider == null) continue;
                    rows.Add(new SceneColliderAuditRow
                    {
                        name = collider.name,
                        hierarchy_path = HierarchyPath(collider.transform),
                        enabled = collider.enabled,
                        is_trigger = collider.isTrigger,
                        position = collider.transform.position,
                        bounds_center = collider.bounds.center,
                        bounds_size = collider.bounds.size
                    });
                }
            }
            var report = new SceneColliderAuditReport
            {
                schema = "D19_R03_A01_SCENE_COLLIDER_AUDIT_V1",
                scene = ScenePath,
                collider_count = rows.Count,
                rows = rows.ToArray()
            };
            var output = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_SCENE_COLLIDER_AUDIT.json";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            Debug.Log("[D19-R03-A01] Scene collider audit written: " + output);
        }

        private static string HierarchyPath(Transform current)
        {
            var path = current == null ? string.Empty : current.name;
            while (current != null && current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }
            return path;
        }

        [Serializable]
        private sealed class SceneColliderAuditReport
        {
            public string schema;
            public string scene;
            public int collider_count;
            public SceneColliderAuditRow[] rows;
        }

        [Serializable]
        private sealed class SceneColliderAuditRow
        {
            public string name;
            public string hierarchy_path;
            public bool enabled;
            public bool is_trigger;
            public Vector3 position;
            public Vector3 bounds_center;
            public Vector3 bounds_size;
        }

        private static GameObject CreateIslandSurface(params Material[] materials)
        {
            var go = new GameObject("D19_R03_IslandSurface");
            var filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            const int width = 26;
            const int depth = 26;
            const float step = 1.55f;
            var vertices = new List<Vector3>();
            var sub = new List<int>[materials.Length];
            for (int i = 0; i < sub.Length; i++) sub[i] = new List<int>();
            for (int z = 0; z < depth - 1; z++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    float px = (x - (width - 1) * 0.5f) * step;
                    float pz = (z - (depth - 1) * 0.5f) * step;
                    float h00 = Height(px, pz);
                    float h10 = Height(px + step, pz);
                    float h11 = Height(px + step, pz + step);
                    float h01 = Height(px, pz + step);
                    int baseIndex = vertices.Count;
                    vertices.Add(new Vector3(px, h00, pz));
                    vertices.Add(new Vector3(px + step, h10, pz));
                    vertices.Add(new Vector3(px + step, h11, pz + step));
                    vertices.Add(new Vector3(px, h01, pz + step));
                    int mat = SurfaceMaterial(px + step * 0.5f, pz + step * 0.5f, materials.Length);
                    sub[mat].Add(baseIndex); sub[mat].Add(baseIndex + 2); sub[mat].Add(baseIndex + 1);
                    sub[mat].Add(baseIndex); sub[mat].Add(baseIndex + 3); sub[mat].Add(baseIndex + 2);
                }
            }
            var mesh = new Mesh { name = "D19_R03_IslandSurfaceMesh" };
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.subMeshCount = sub.Length;
            for (int i = 0; i < sub.Length; i++) mesh.SetTriangles(sub[i], i, true);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            string path = MaterialDirectory + "/D19_R03_IslandSurfaceMesh.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return go;
        }

        private static float Height(float x, float z)
        {
            float edge = Mathf.InverseLerp(19.5f, 11.5f, Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)));
            float hill = (Mathf.Sin(x * 0.26f + z * 0.13f) * 0.26f + Mathf.Cos(z * 0.31f - x * 0.11f) * 0.18f) * edge;
            float ridge = Mathf.Exp(-Mathf.Pow((x + 7.2f) / 4.6f, 2f) - Mathf.Pow((z - 6.0f) / 7.5f, 2f)) * 1.55f;
            if (z < -7.2f && Mathf.Abs(x) < 7.5f) return 0.035f + hill * 0.18f;
            if (Mathf.Abs(x) < 2.4f && z < 5.0f) return 0.08f + hill * 0.20f;
            return 0.16f + hill + ridge;
        }

        private static int SurfaceMaterial(float x, float z, int count)
        {
            if (z < -8.7f) return Mathf.Min(4, count - 1);
            if (Mathf.Abs(z + 7.0f) < 1.4f || Mathf.Abs(x + 2f) < 1.1f && z < 4f) return Mathf.Min(3, count - 1);
            if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) > 14.0f) return Mathf.Min(5, count - 1);
            if (Mathf.Sin(x * 0.22f + z * 0.13f) > 0.78f) return Mathf.Min(1, count - 1);
            return 0;
        }

        private static GameObject CreateShoreline(Material sand, Material stone, Material foam, Material water)
        {
            var root = new GameObject("D19_R03_ShorelineAndWater");
            AddMesh(root.transform, "Water", MakeBoxMesh(34f, 0.08f, 8f), water, new Vector3(0f, -0.04f, -14.5f));
            AddMesh(root.transform, "Beach", MakeRibbonMesh(34f, 0.12f, 4.6f), sand, new Vector3(0f, 0.08f, -10.0f));
            for (int i = 0; i < 13; i++)
            {
                float x = -15.5f + i * 2.6f;
                AddMesh(root.transform, "Foam_" + i, MakeBevelBoxMesh(1.4f, 0.08f, 0.25f, 0.08f), foam, new Vector3(x, 0.13f, -11.9f + Mathf.Sin(i * 1.2f) * 0.18f));
            }
            for (int i = 0; i < 9; i++)
            {
                float x = -15f + i * 3.8f;
                AddMesh(root.transform, "ShoreRock_" + i, MakeRockMesh(0.65f + (i % 3) * 0.18f), stone, new Vector3(x, 0.12f, -10.7f - (i % 2) * 0.7f));
            }
            return root;
        }

        private static GameObject CreateRoadNetwork(Material earth, Material sand, Material stone)
        {
            var root = new GameObject("D19_R03_RoadNetwork");
            AddMesh(root.transform, "MainRoad", MakeRibbonMesh(3.3f, 0.09f, 20f), earth, new Vector3(0f, 0.16f, -0.5f));
            AddMesh(root.transform, "MarketRoad", MakeRibbonMesh(12f, 0.08f, 2.8f), sand, new Vector3(1.2f, 0.18f, 1.2f));
            AddMesh(root.transform, "WarehouseRoad", MakeRibbonMesh(2.5f, 0.08f, 10f), sand, new Vector3(-4.0f, 0.19f, -0.1f));
            for (int i = 0; i < 14; i++)
                AddMesh(root.transform, "RoadEdge_" + i, MakeBevelBoxMesh(0.15f, 0.10f, 1.2f, 0.03f), stone, new Vector3(-1.4f + (i % 3) * 1.4f, 0.25f, -10.0f + i * 1.35f));
            return root;
        }

        private static GameObject CreateHouse(string name, Vector3 position, Vector3 size, Material wall, Material roof, Material trim, Material accent, Material ground, string label)
        {
            var root = new GameObject(name);
            root.transform.position = position;
            float w = size.x, h = size.y, d = size.z;
            AddMesh(root.transform, "Body", MakeBevelBoxMesh(w, h, d, 0.18f), wall, new Vector3(0f, h * 0.5f, 0f));
            // Keep the roof readable from the player camera without letting a
            // near facade fill the whole frame.  The previous 1.1m ridge and
            // full-depth canopies read as a second ceiling when the camera is
            // close to a landmark.
            AddMesh(root.transform, "Roof", MakeRoofMesh(w + 0.45f, 0.62f, d + 0.45f), roof, new Vector3(0f, h + 0.08f, 0f));
            AddMesh(root.transform, "RoofRidge", MakeBevelBoxMesh(0.22f, 0.14f, d + 0.55f, 0.05f), roof, new Vector3(0f, h + 0.48f, 0f));
            AddMesh(root.transform, "RoofFrontTrim", MakeBevelBoxMesh(w + 0.5f, 0.12f, 0.18f, 0.04f), roof, new Vector3(0f, h + 0.10f, -d * 0.5f - 0.24f));
            AddMesh(root.transform, "Door", MakeBevelBoxMesh(0.9f, 1.75f, 0.12f, 0.05f), trim, new Vector3(0f, 0.88f, -d * 0.5f - 0.08f));
            AddMesh(root.transform, "DoorStep", MakeBevelBoxMesh(1.3f, 0.14f, 0.65f, 0.04f), ground, new Vector3(0f, 0.08f, -d * 0.5f - 0.36f));
            AddMesh(root.transform, "WindowL", MakeBevelBoxMesh(1.1f, 0.72f, 0.1f, 0.04f), accent, new Vector3(-w * 0.28f, h * 0.57f, -d * 0.5f - 0.07f));
            AddMesh(root.transform, "WindowR", MakeBevelBoxMesh(1.1f, 0.72f, 0.1f, 0.04f), accent, new Vector3(w * 0.28f, h * 0.57f, -d * 0.5f - 0.07f));
            AddMesh(root.transform, "Eave", MakeBoxMesh(w + 0.9f, 0.16f, 0.28f), trim, new Vector3(0f, h + 0.05f, -d * 0.5f - 0.20f));
            AddMesh(root.transform, "FrontCanopy", MakeRoofMesh(w * 0.58f, 0.22f, 0.62f), roof, new Vector3(0f, h * 0.72f, -d * 0.5f - 0.30f));
            AddMesh(root.transform, "CanopyPostL", MakeCylinderMesh(0.055f, h * 0.52f, 8), trim, new Vector3(-w * 0.26f, h * 0.26f, -d * 0.5f - 0.34f));
            AddMesh(root.transform, "CanopyPostR", MakeCylinderMesh(0.055f, h * 0.52f, 8), trim, new Vector3(w * 0.26f, h * 0.26f, -d * 0.5f - 0.34f));
            AddMesh(root.transform, "SignBoard", MakeBevelBoxMesh(1.9f, 0.48f, 0.10f, 0.05f), accent, new Vector3(0f, h * 0.64f, -d * 0.5f - 0.31f));
            AddText(root.transform, "Sign", label, new Vector3(0f, h * 0.64f, -d * 0.5f - 0.38f), 0.11f);
            // The initial Player camera can approach from either side of a
            // building. Keep both elevations legible without changing the
            // gameplay anchor or adding collision to this presentation-only
            // prefab.
            AddMesh(root.transform, "BackDoor", MakeBevelBoxMesh(0.9f, 1.75f, 0.12f, 0.05f), trim, new Vector3(0f, 0.88f, d * 0.5f + 0.08f));
            AddMesh(root.transform, "BackWindowL", MakeBevelBoxMesh(1.1f, 0.72f, 0.1f, 0.04f), accent, new Vector3(-w * 0.28f, h * 0.57f, d * 0.5f + 0.07f));
            AddMesh(root.transform, "BackWindowR", MakeBevelBoxMesh(1.1f, 0.72f, 0.1f, 0.04f), accent, new Vector3(w * 0.28f, h * 0.57f, d * 0.5f + 0.07f));
            AddMesh(root.transform, "BackCanopy", MakeRoofMesh(w * 0.58f, 0.22f, 0.62f), roof, new Vector3(0f, h * 0.72f, d * 0.5f + 0.30f));
            AddMesh(root.transform, "BackCanopyPostL", MakeCylinderMesh(0.055f, h * 0.52f, 8), trim, new Vector3(-w * 0.26f, h * 0.26f, d * 0.5f + 0.34f));
            AddMesh(root.transform, "BackCanopyPostR", MakeCylinderMesh(0.055f, h * 0.52f, 8), trim, new Vector3(w * 0.26f, h * 0.26f, d * 0.5f + 0.34f));
            AddMesh(root.transform, "BackSignBoard", MakeBevelBoxMesh(1.9f, 0.48f, 0.10f, 0.05f), accent, new Vector3(0f, h * 0.64f, d * 0.5f + 0.31f));
            AddText(root.transform, "BackSign", label, new Vector3(0f, h * 0.64f, d * 0.5f + 0.38f), 0.11f);
            AddMesh(root.transform, "CrateL", MakeBevelBoxMesh(0.65f, 0.6f, 0.65f, 0.08f), trim, new Vector3(-w * 0.38f, 0.32f, -d * 0.5f - 0.55f));
            AddMesh(root.transform, "CrateR", MakeBevelBoxMesh(0.65f, 0.46f, 0.65f, 0.08f), ground, new Vector3(w * 0.38f, 0.24f, -d * 0.5f - 0.55f));
            return root;
        }

        private static GameObject CreateDock(Material wood, Material woodLight, Material stone, Material rope)
        {
            var root = new GameObject("D19_R03_DockAndBerth");
            root.transform.position = new Vector3(-1f, 0.20f, -8.8f);
            AddMesh(root.transform, "Deck", MakeBevelBoxMesh(7.8f, 0.24f, 3.8f, 0.10f), woodLight, new Vector3(0f, 0.18f, 0f));
            for (int i = 0; i < 7; i++)
                AddMesh(root.transform, "Plank_" + i, MakeBoxMesh(0.12f, 0.10f, 3.6f), wood, new Vector3(-3.2f + i * 1.05f, 0.36f, 0f));
            for (int i = 0; i < 5; i++)
            {
                float x = -3.0f + i * 1.5f;
                AddMesh(root.transform, "Post_" + i, MakeCylinderMesh(0.16f, 1.35f, 8), wood, new Vector3(x, -0.35f, 1.45f));
                var ropeMesh = AddMesh(root.transform, "Rope_" + i, MakeCylinderMesh(0.04f, 1.8f, 6), rope, new Vector3(x + 0.35f, 0.55f, 1.35f));
                // A mooring line should read as a short, angled line between
                // the post and the berth, not as a pale vertical pole.
                ropeMesh.transform.localRotation = Quaternion.Euler(0f, 0f, 62f + (i % 2) * 8f);
            }
            AddMesh(root.transform, "BerthStone", MakeBevelBoxMesh(8.6f, 0.4f, 0.55f, 0.16f), stone, new Vector3(0f, -0.02f, 2.0f));
            AddMesh(root.transform, "Ramp", MakeWedgeMesh(2.4f, 0.5f, 4.0f), wood, new Vector3(0f, 0.18f, 3.25f));
            return root;
        }

        private static GameObject CreateVegetation(Material wood, Material leaf, Material grass, Material stone)
        {
            var root = new GameObject("D19_R03_VegetationAndRocks");
            var positions = new[] { new Vector3(-13f, 0.2f, 6f), new Vector3(12f, 0.2f, 8f), new Vector3(10f, 0.2f, -2f), new Vector3(-12f, 0.2f, -2f), new Vector3(8f, 0.2f, 9f), new Vector3(-9f, 0.2f, 10f) };
            for (int i = 0; i < positions.Length; i++)
            {
                var palm = CreatePalm("Palm_" + i, wood, leaf, positions[i], 0.9f + (i % 3) * 0.14f);
                palm.transform.SetParent(root.transform, false);
            }
            for (int i = 0; i < 14; i++)
            {
                float x = -14f + (i * 2.65f) % 28f;
                float z = 4.0f + (i % 4) * 3.1f;
                AddMesh(root.transform, "Shrub_" + i, MakeShrubMesh(0.38f + (i % 3) * 0.08f), grass, new Vector3(x, 0.2f, z));
            }
            for (int i = 0; i < 10; i++)
                AddMesh(root.transform, "Rock_" + i, MakeRockMesh(0.55f + (i % 3) * 0.16f), stone, new Vector3(-14f + i * 3.1f, 0.25f, 10f + (i % 2) * 1.4f));
            return root;
        }

        private static GameObject CreatePortProps(Material wood, Material woodLight, Material stone, Material sand)
        {
            var root = new GameObject("D19_R03_PortProps");
            for (int i = 0; i < 7; i++)
            {
                float x = -7.0f + i * 2.2f;
                AddMesh(root.transform, "Barrel_" + i, MakeCylinderMesh(0.34f, 0.68f, 10), wood, new Vector3(x, 0.52f, -2.0f + (i % 2) * 0.42f));
            }
            AddMesh(root.transform, "NoticeBoard", MakeBevelBoxMesh(1.5f, 1.8f, 0.18f, 0.06f), woodLight, new Vector3(3.2f, 1.0f, -1.4f));
            AddMesh(root.transform, "NoticePost", MakeCylinderMesh(0.08f, 1.4f, 8), stone, new Vector3(3.2f, 0.45f, -1.4f));
            for (int i = 0; i < 4; i++) AddMesh(root.transform, "StoneStep_" + i, MakeBevelBoxMesh(2.3f, 0.18f, 0.8f, 0.05f), sand, new Vector3(2.0f + i * 0.05f, 0.15f + i * 0.16f, 2.4f + i * 0.55f));
            for (int i = 0; i < 3; i++)
            {
                float x = 3.0f + i * 2.1f;
                AddMesh(root.transform, "MarketStallWall_" + i, MakeBevelBoxMesh(1.55f, 1.2f, 0.16f, 0.06f), woodLight, new Vector3(x, 0.75f, -0.75f));
                AddMesh(root.transform, "MarketStallRoof_" + i, MakeRoofMesh(2.0f, 0.42f, 1.25f), sand, new Vector3(x, 1.55f, -0.75f));
                AddMesh(root.transform, "MarketStallCounter_" + i, MakeBevelBoxMesh(1.75f, 0.18f, 0.65f, 0.04f), wood, new Vector3(x, 0.95f, -1.18f));
            }
            return root;
        }

        private static GameObject CreateMarketEntry(Material wood, Material woodLight, Material stone, Material sand)
        {
            var root = new GameObject("D19_R03_MarketEntryAndStreet");
            // A clear, low-profile arrival marker gives the player a readable
            // first landmark while leaving the frozen road and interaction
            // anchors untouched.
            // A low sign frame keeps the street legible while leaving the
            // frozen market approach and the player camera unobstructed.
            AddMesh(root.transform, "EntryPillarL", MakeBevelBoxMesh(0.30f, 1.55f, 0.30f, 0.04f), stone, new Vector3(-3.35f, 0.80f, -3.0f));
            AddMesh(root.transform, "EntryPillarR", MakeBevelBoxMesh(0.30f, 1.55f, 0.30f, 0.04f), stone, new Vector3(3.35f, 0.80f, -3.0f));
            AddMesh(root.transform, "EntryBeam", MakeBevelBoxMesh(7.15f, 0.18f, 0.26f, 0.04f), woodLight, new Vector3(0f, 1.62f, -3.0f));
            AddMesh(root.transform, "EntryRoof", MakeRoofMesh(7.35f, 0.24f, 0.80f), wood, new Vector3(0f, 1.82f, -3.0f));
            AddMesh(root.transform, "EntrySignBoard", MakeBevelBoxMesh(2.45f, 0.40f, 0.10f, 0.04f), sand, new Vector3(0f, 1.28f, -3.18f));
            AddText(root.transform, "EntrySign", "MARKET STREET", new Vector3(0f, 1.28f, -3.27f), 0.10f);
            for (int i = 0; i < 4; i++)
            {
                float x = -5.7f + i * 3.8f;
                AddMesh(root.transform, "StreetStone_" + i, MakeBevelBoxMesh(2.4f, 0.10f, 0.65f, 0.05f), stone, new Vector3(x, 0.24f, -4.8f));
            }
            return root;
        }

        private static GameObject CreateHarborBackdrop(Material earth, Material stone, Material grass, Material leaf)
        {
            var root = new GameObject("D19_R03_HarborBackdrop");
            var positions = new[]
            {
                new Vector3(-12.5f, 0.40f, 13.2f), new Vector3(-6.0f, 0.55f, 14.0f),
                new Vector3(0.0f, 0.35f, 14.8f), new Vector3(7.0f, 0.50f, 14.0f),
                new Vector3(13.0f, 0.45f, 13.0f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                var rock = AddMesh(root.transform, "BackdropRock_" + i, MakeRockMesh(1.65f + (i % 2) * 0.30f), i % 2 == 0 ? stone : earth, positions[i]);
                rock.transform.localScale = new Vector3(1.25f, 1.25f + (i % 3) * 0.18f, 1.05f);
            }
            for (int i = 0; i < 5; i++)
            {
                var tree = CreatePalm("BackdropPalm_" + i, earth, leaf, new Vector3(-11.5f + i * 5.6f, 0.2f, 11.8f + (i % 2) * 0.8f), 0.62f + (i % 2) * 0.10f);
                tree.transform.SetParent(root.transform, false);
            }
            return root;
        }

        private static void AddCandidateDetails(Transform parent, Material wood, Material leaf, Material stone, Material grass)
        {
            // These are controlled, presentation-only uses of candidates that
            // are intake_eligible for D19-A01/D19-A03. The source files remain
            // Candidate/ImportVerified; this builder does not promote resource
            // state or create runtime ownership. Keep the authored palms as a
            // fallback and use the imported modules to break up the repeated
            // generated silhouettes in the Player camera.
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_large_rock.fbx", "KenneyCliffBackdrop", new Vector3(12.8f, 0.25f, 11.0f), new Vector3(2.2f, 2.2f, 2.2f), 22f, wood, leaf, stone, grass);
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeF.fbx", "KenneyRockShore", new Vector3(-12.0f, 0.25f, -10.7f), new Vector3(1.4f, 1.4f, 1.4f), 10f, wood, leaf, stone, grass);
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_default.fbx", "KenneyTreeHarborWest", new Vector3(-10.5f, 0.25f, 8.0f), new Vector3(1.55f, 1.55f, 1.55f), -12f, wood, leaf, stone, grass);
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_small_dark.fbx", "KenneyTreeHarborNorth", new Vector3(-8.3f, 0.25f, 10.0f), new Vector3(1.25f, 1.25f, 1.25f), 18f, wood, leaf, stone, grass);
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/grass_large.fbx", "KenneyGrassClusterEast", new Vector3(11.1f, 0.22f, 5.0f), new Vector3(1.65f, 1.65f, 1.65f), 34f, wood, leaf, stone, grass);
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathBend.fbx", "KenneyPathBendMarket", new Vector3(0.0f, 0.28f, -4.8f), new Vector3(1.6f, 1.6f, 1.6f), 0f, wood, leaf, stone, grass);
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_stepsCorner_stone.fbx", "KenneyCliffStepsNorth", new Vector3(12.0f, 0.25f, 11.7f), new Vector3(1.35f, 1.35f, 1.35f), -28f, wood, leaf, stone, grass);
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallF.fbx", "KenneyRockTallEast", new Vector3(13.0f, 0.25f, -8.4f), new Vector3(1.05f, 1.05f, 1.05f), 42f, wood, leaf, stone, grass);
        }

        private static GameObject BuildShipPrefab(Material wood, Material woodLight, Material stone, Material foam, Material sail)
        {
            var root = new GameObject("D19_R03_ShipVisual");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            AddMesh(root.transform, "Hull", MakeBevelBoxMesh(4.5f, 0.72f, 7.2f, 0.24f), wood, new Vector3(0f, 0.38f, 0f));
            AddMesh(root.transform, "Deck", MakeBevelBoxMesh(3.75f, 0.16f, 5.85f, 0.08f), woodLight, new Vector3(0f, 0.82f, -0.05f));
            AddMesh(root.transform, "Bow", MakeWedgeMesh(3.75f, 0.68f, 2.20f), woodLight, new Vector3(0f, 0.50f, 3.60f));
            AddMesh(root.transform, "SternRail", MakeBevelBoxMesh(3.55f, 0.12f, 0.18f, 0.04f), stone, new Vector3(0f, 1.08f, -2.80f));
            AddMesh(root.transform, "Mast", MakeCylinderMesh(0.13f, 3.35f, 10), wood, new Vector3(0f, 2.15f, -0.35f));
            AddMesh(root.transform, "Boom", MakeCylinderMesh(0.06f, 2.60f, 8), wood, new Vector3(0f, 2.20f, 0.80f)).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            AddMesh(root.transform, "Sail", MakeRibbonMesh(2.20f, 2.25f, 0.06f), sail, new Vector3(0f, 2.15f, 0.30f));
            AddMesh(root.transform, "HelmStand", MakeBevelBoxMesh(0.34f, 0.70f, 0.34f, 0.06f), stone, new Vector3(0f, 1.18f, 1.78f));
            var wheel = AddMesh(root.transform, "HelmWheel", MakeCylinderMesh(0.34f, 0.10f, 12), woodLight, new Vector3(0f, 1.56f, 1.78f));
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int i = 0; i < 4; i++)
            {
                float x = -1.65f + i * 1.10f;
                AddMesh(root.transform, "PortRail_" + i, MakeCylinderMesh(0.045f, 0.55f, 8), stone, new Vector3(x, 1.08f, -1.95f));
                AddMesh(root.transform, "StarboardRail_" + i, MakeCylinderMesh(0.045f, 0.55f, 8), stone, new Vector3(x, 1.08f, 1.95f));
            }

            var collision = new GameObject("D6_ShipHullCollision");
            collision.transform.SetParent(root.transform, false);
            collision.transform.localPosition = new Vector3(0f, 0.38f, 0f);
            var box = collision.AddComponent<BoxCollider>();
            box.size = new Vector3(4.5f, 0.84f, 7.2f);
            box.isTrigger = false;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, ShipPrefabPath, out bool saved);
            if (!saved || prefab == null)
                throw new InvalidOperationException("Could not save R03 ship visual prefab: " + ShipPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void InstallShipPresentation(Scene scene, GameObject shipPrefab)
        {
            if (shipPrefab == null) return;
            var ship = FindSceneObject(scene, "ph_ship_fishing");
            if (ship == null)
            {
                Debug.LogWarning("[D19-R03-A02] Stable ph_ship_fishing node was not found; ship visual was not installed");
                return;
            }

            foreach (var child in ship.transform)
            {
                var childTransform = child as Transform;
                if (childTransform != null && childTransform.name == "D19_R03_ShipVisualInstance")
                    UnityEngine.Object.DestroyImmediate(childTransform.gameObject);
            }

            foreach (var renderer in ship.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.transform == ship.transform || renderer.transform.IsChildOf(ship.transform))
                    renderer.enabled = false;
            }

            var instance = PrefabUtility.InstantiatePrefab(shipPrefab, scene) as GameObject;
            if (instance == null) throw new InvalidOperationException("Could not instantiate " + ShipPrefabPath);
            instance.name = "D19_R03_ShipVisualInstance";
            instance.transform.SetParent(ship.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            var worldScale = ship.transform.lossyScale;
            instance.transform.localScale = new Vector3(
                1f / Mathf.Max(0.001f, Mathf.Abs(worldScale.x)),
                1f / Mathf.Max(0.001f, Mathf.Abs(worldScale.y)),
                1f / Mathf.Max(0.001f, Mathf.Abs(worldScale.z)));
            Debug.Log("[D19-R03-A02] Installed presentation-only ship visual under stable ph_ship_fishing; D2/D6 ownership preserved");
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    if (transform.name == name) return transform.gameObject;
            }
            return null;
        }

        private static GameObject AddCandidate(Transform parent, string path, string name, Vector3 position, Vector3 scale, float yaw, Material wood, Material leaf, Material stone, Material grass)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return null;
            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null) return null;
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = scale;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                var source = renderer.sharedMaterials;
                var mapped = new Material[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    string materialName = source[i] == null ? string.Empty : source[i].name.ToLowerInvariant();
                    mapped[i] = materialName.Contains("leaf") ? leaf : materialName.Contains("wood") ? wood : materialName.Contains("grass") ? grass : stone;
                }
                renderer.sharedMaterials = mapped;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            return instance;
        }

        private static GameObject CreatePalm(string name, Material wood, Material leaf, Vector3 position, float scale)
        {
            var root = new GameObject(name);
            root.transform.localPosition = position;
            root.transform.localScale = Vector3.one * scale;
            AddMesh(root.transform, "Trunk", MakeCylinderMesh(0.22f, 4.1f, 8), wood, new Vector3(0f, 2.05f, 0f));
            for (int i = 0; i < 7; i++)
            {
                float angle = i * Mathf.PI * 2f / 7f;
                var blade = AddMesh(root.transform, "Leaf_" + i, MakeBladeMesh(1.45f, 0.18f), leaf, new Vector3(Mathf.Cos(angle) * 0.9f, 3.95f, Mathf.Sin(angle) * 0.9f));
                blade.transform.rotation = Quaternion.Euler(8f, -i * 360f / 7f, 12f * Mathf.Sin(angle));
            }
            return root;
        }

        private static void AddText(Transform parent, string name, string text, Vector3 position, float size)
        {
            var go = new GameObject(name, typeof(TextMesh));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            // TextMesh faces the camera-facing side in this presentation prefab.  A 180°
            // turn mirrors the sign in the Player, so keep the label orientation neutral.
            go.transform.localRotation = Quaternion.identity;
            var mesh = go.GetComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = size * 0.55f;
            mesh.fontSize = 22;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(0.12f, 0.06f, 0.025f, 1f);
        }

        private static GameObject AddMesh(Transform parent, string name, Mesh mesh, Material material, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var filter = go.AddComponent<MeshFilter>();
            if (mesh != null)
            {
                string safeName = name.Replace("/", "_").Replace("\\", "_").Replace(" ", "_");
                string meshPath = MeshDirectory + "/D19_R03_" + _meshSequence.ToString("D4") + "_" + safeName + ".asset";
                _meshSequence++;
                AssetDatabase.DeleteAsset(meshPath);
                AssetDatabase.CreateAsset(mesh, meshPath);
                filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            }
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return go;
        }

        private static Material Material(string name, Color color, float metallic, float smoothness)
        {
            string path = MaterialDirectory + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            // R03's small procedural presentation meshes intentionally share
            // low-poly modules whose winding is not guaranteed on every
            // elevation. Double-sided display keeps the Player camera from
            // turning a complete facade into an apparently empty grey block.
            if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 2f);
            return material;
        }

        private static void HidePresentationRenderers(Scene scene, string rootName)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null || root.name != rootName) continue;
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                // Older presentation prefabs can carry a broad visual-surface
                // collider (the R02 surface covered the whole harbor).  Once
                // their renderers are hidden, those colliders must not remain
                // as a second movement authority.  The R03 prefab owns the
                // walkable surface; stable D2/D6 colliders remain elsewhere.
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                    collider.enabled = false;
            }
        }

        private static void HideLegacyVisuals(Scene scene)
        {
            var names = new HashSet<string>(StringComparer.Ordinal)
            {
                "Beach", "Sea", "Dock", "DockDeck", "Gangplank", "HandRail",
                "Barrel", "Barrel2", "Crate", "Crate2", "Bollard", "Pile",
                "D19_R01_B_DockMarketProps_Approved", "D19_R01_B_PortModules_Approved"
            };
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null) continue;
                // D19_SaintMargaret_Root still owns the historical grey-box renderers;
                // keep its gameplay colliders/anchors, but remove only its visuals so
                // the R03 prefab is the sole presentation layer in the Player.
                if (root.name == "D19_SaintMargaret_Root")
                {
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                }
                if (root.name == "ph_palm_tree" || root.name == "ph_rock" ||
                    root.name == "ph_grass" || root.name == "ph_flower")
                {
                    // Preserve the stable placeholder nodes for gameplay and
                    // old saves, but remove their grey-box renderers from the
                    // R03 presentation pass.
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                }
                if (root.name.IndexOf("D19_R01", StringComparison.Ordinal) >= 0 ||
                    root.name.IndexOf("D19_R02", StringComparison.Ordinal) >= 0 ||
                    root.name.IndexOf("D19_S02", StringComparison.Ordinal) >= 0)
                {
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                }
                foreach (var child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!names.Contains(child.name)) continue;
                    foreach (var renderer in child.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                }
            }
        }

        private static string[] SceneRootSummary(Scene scene)
        {
            var rows = new List<string>();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null) continue;
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                int enabled = 0;
                foreach (var renderer in renderers) if (renderer.enabled) enabled++;
                rows.Add(root.name + "|active=" + root.activeSelf + "|renderers=" + renderers.Length + "|enabled=" + enabled);
            }
            return rows.ToArray();
        }

        private static void RemoveSceneInstance(Scene scene, string rootName)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root != null && root.name == rootName) UnityEngine.Object.DestroyImmediate(root);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static string MiniJson(Dictionary<string, object> values)
        {
            var lines = new List<string> { "{" };
            int index = 0;
            foreach (var pair in values)
            {
                string value;
                var array = pair.Value as Array;
                if (array != null)
                {
                    var parts = new List<string>();
                    foreach (var item in array) parts.Add("\"" + item.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
                    value = "[" + string.Join(", ", parts.ToArray()) + "]";
                }
                else if (pair.Value is int) value = pair.Value.ToString();
                else value = "\"" + pair.Value.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
                lines.Add("  \"" + pair.Key + "\": " + value + (index++ == values.Count - 1 ? "" : ","));
            }
            lines.Add("}");
            return string.Join("\n", lines.ToArray());
        }

        private static Mesh MakeBoxMesh(float width, float height, float depth)
        {
            float x = width * 0.5f, y = height * 0.5f, z = depth * 0.5f;
            var v = new[] { new Vector3(-x,-y,-z), new Vector3(x,-y,-z), new Vector3(x,-y,z), new Vector3(-x,-y,z), new Vector3(-x,y,-z), new Vector3(x,y,-z), new Vector3(x,y,z), new Vector3(-x,y,z) };
            return MakeMesh(v, new[] {
                0,1,2, 0,2,3,       // bottom
                4,6,5, 4,7,6,       // top
                0,4,5, 0,5,1,       // front (-Z)
                2,6,7, 2,7,3,       // back (+Z)
                1,5,6, 1,6,2,       // right (+X)
                0,3,7, 0,7,4        // left (-X)
            });
        }

        private static Mesh MakeBevelBoxMesh(float width, float height, float depth, float bevel)
        {
            // The previous beveled ring used mixed winding and produced
            // black back-facing slivers in Player. The R03 presentation keeps
            // the bevel impression through trim/roof modules; a correctly
            // wound box is safer and remains fully readable at this scale.
            return MakeBoxMesh(width, height, depth);
        }

        private static Mesh MakeRoofMesh(float width, float height, float depth)
        {
            float x = width * 0.5f, z = depth * 0.5f;
            return MakeMesh(new[] { new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(x,0,z),new Vector3(-x,0,z),new Vector3(0,height,0) }, new[] {
                0,4,1,       // front slope (-Z)
                1,4,2,       // right slope (+X)
                2,4,3,       // back slope (+Z)
                3,4,0,       // left slope (-X)
                0,1,2, 0,2,3 // underside
            });
        }

        private static Mesh MakeWedgeMesh(float width, float height, float depth)
        {
            float x = width * 0.5f, z = depth * 0.5f;
            return MakeMesh(new[] { new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(x,0,z),new Vector3(-x,0,z),new Vector3(-x,height,z),new Vector3(x,height,z) }, new[] { 0,1,2,0,2,3,3,2,5,3,5,4,0,3,4,0,4,1,1,4,5,1,5,2 });
        }

        private static Mesh MakeRibbonMesh(float width, float height, float depth)
        {
            float x = width * 0.5f, z = depth * 0.5f;
            return MakeMesh(new[] { new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(x,height,z),new Vector3(-x,height,z) }, new[] { 0,1,2,0,2,3 });
        }

        private static Mesh MakeRockMesh(float radius)
        {
            var v = new[] { new Vector3(-radius*0.8f,0,-radius*0.6f),new Vector3(radius*0.8f,0,-radius*0.5f),new Vector3(radius,0,radius*0.4f),new Vector3(-radius*0.7f,0,radius*0.6f),new Vector3(-radius*0.25f,radius*1.25f,-radius*0.15f),new Vector3(radius*0.35f,radius*0.95f,radius*0.25f) };
            return MakeMesh(v, new[] { 0,1,4,1,2,5,1,5,4,2,3,5,3,0,4,3,4,5,0,3,2,0,2,1 });
        }

        private static Mesh MakeShrubMesh(float radius)
        {
            var v = new[] { new Vector3(-radius,0,0),new Vector3(radius,0,0),new Vector3(0,0,-radius),new Vector3(0,0,radius),new Vector3(0,radius*1.2f,0) };
            return MakeMesh(v, new[] { 0,1,4,1,3,4,3,2,4,2,0,4,0,2,3,0,3,1 });
        }

        private static Mesh MakeCylinderMesh(float radius, float height, int segments)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int i = 0; i < segments; i++) { float a = i * Mathf.PI * 2f / segments; v.Add(new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius)); v.Add(new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius)); }
            for (int i = 0; i < segments; i++) { int n = (i + 1) % segments; t.Add(i * 2); t.Add(n * 2); t.Add(n * 2 + 1); t.Add(i * 2); t.Add(n * 2 + 1); t.Add(i * 2 + 1); }
            return MakeMesh(v.ToArray(), t.ToArray());
        }

        private static Mesh MakeBladeMesh(float length, float width)
        {
            return MakeMesh(new[] { new Vector3(0,0,0),new Vector3(length*0.65f,width,0),new Vector3(length,0.02f,0),new Vector3(length*0.65f,-width,0),new Vector3(length*0.55f,0,0.12f) }, new[] { 0,1,4,1,2,4,2,3,4,3,0,4 });
        }

        private static Mesh MakeMesh(Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh { vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
#endif
