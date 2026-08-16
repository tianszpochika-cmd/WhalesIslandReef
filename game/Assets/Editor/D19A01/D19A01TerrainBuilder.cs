#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace WhalesIslandReef.Editor.D19A01
{
    /// <summary>
    /// D19-A01 受控工作场景构建器。
    /// 只写 D19-A01 Work/Prefab/材质/测试结果，不触碰 InitialHarbor、Registry 或 Systems。
    /// </summary>
    public static class D19A01TerrainBuilder
    {
        private const string ScenePath = "Assets/Scenes/D19/Work/D19_Terrain_Work.unity";
        private const string PrefabRoot = "Assets/Prefabs/Environment/D19/Terrain";
        private const string MaterialRoot = "Assets/Art/Materials/D19/A01";
        private const string DataRoot = "Assets/Scenes/D19/Work/Data";
        private const string NatureRoot = "Assets/Art/Candidates/Environment/KenneyNatureKit/Models";
        private const string ResultRoot = "../tools/results/D19/D19-A01";

        private const string TerrainPrefabPath = PrefabRoot + "/D19_TerrainRoot.prefab";
        private const string CollisionPrefabPath = PrefabRoot + "/D19_TerrainCollision.prefab";
        private const string AnchorPrefabPath = PrefabRoot + "/D19_TerrainAnchors.prefab";
        private const string SurfacePrefabPath = PrefabRoot + "/D19_TerrainSurfaceSet.prefab";
        private const string CavePrefabPath = PrefabRoot + "/D19_CaveFissure.prefab";
        private const string PresentationPrefabPath = PrefabRoot + "/D19_TerrainPresentation.prefab";
        private const string NavMeshPrefabPath = PrefabRoot + "/D19_TerrainNavMesh.prefab";
        private const string ProbePrefabPath = PrefabRoot + "/D19_TerrainRuntimeProbe.prefab";
        private const string TerrainDataPath = DataRoot + "/D19_SaintMargaret_TerrainData.asset";
        private const string NavMeshDataPath = DataRoot + "/D19_SaintMargaret_NavMesh.asset";
        private const string NavMeshSourceMeshPath = DataRoot + "/D19_SaintMargaret_NavMeshSource.asset";
        private const string TerrainVisualMeshPath = DataRoot + "/D19_SaintMargaret_TerrainVisualMesh.asset";

        private static readonly Vector3 TerrainPosition = new Vector3(-128f, -8f, -128f);
        private static readonly Vector3 TerrainSize = new Vector3(256f, 32f, 272f);

        private static readonly string[] ExternalModelPaths =
        {
            NatureRoot + "/cliff_corner_rock.fbx",
            NatureRoot + "/rock_largeF.fbx",
            NatureRoot + "/rock_tallF.fbx",
            NatureRoot + "/ground_pathBend.fbx",
            NatureRoot + "/bridge_wood.fbx",
            NatureRoot + "/cliff_stepsCorner_stone.fbx"
        };

        [MenuItem("Tools/WhalesIslandReef/D19-A01/Build Terrain Work Scene")]
        public static void BuildTerrainWorkScene()
        {
            try
            {
                EnsureFolders();
                DeleteOwnArtifacts();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                var materials = CreateMaterials();
                var terrainData = CreateTerrainData(materials);
                var terrainPrefab = CreateTerrainPrefab(terrainData, materials);
                var collisionPrefab = CreateCollisionPrefab();
                var anchorPrefab = CreateAnchorPrefab();
                var cavePrefab = CreateCavePrefab(materials["rock"]);
                var surfacePrefab = CreateSurfacePrefab(materials["rock"]);
                var presentationPrefab = CreatePresentationPrefab();
                var probePrefab = CreateProbePrefab();

                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var terrainInstance = InstantiatePrefab(terrainPrefab, scene, TerrainPosition);
                InstantiatePrefab(collisionPrefab, scene, Vector3.zero);
                InstantiatePrefab(anchorPrefab, scene, Vector3.zero);
                InstantiatePrefab(cavePrefab, scene, new Vector3(72f, TerrainHeight(72f, 72f) - 0.2f, 72f));
                InstantiatePrefab(surfacePrefab, scene, Vector3.zero);
                InstantiatePrefab(presentationPrefab, scene, Vector3.zero);
                InstantiatePrefab(probePrefab, scene, Vector3.zero);

                var navMeshData = BuildNavMeshData(scene);
                var navMeshPrefab = CreateNavMeshPrefab(navMeshData);
                InstantiatePrefab(navMeshPrefab, scene, Vector3.zero);

                // Terrain instance remains a prefab instance; no scene-local mesh or material is created.
                terrainInstance.name = "D19_TerrainRoot";
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                WriteBuildResult(terrainData, navMeshData, scene);
                Debug.Log("[D19-A01] Terrain Work Scene built: " + ScenePath);
            }
            catch (Exception exception)
            {
                Debug.LogError("[D19-A01] Terrain build failed: " + exception);
                throw;
            }
        }

        [MenuItem("Tools/WhalesIslandReef/D19-A01/Run PlayMode Smoke")]
        public static void RunPlayModeSmoke()
        {
            if (!File.Exists(ProjectFile(ScenePath)))
                BuildTerrainWorkScene();

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Tools/WhalesIslandReef/D19-A01/Capture Validation Screenshots")]
        public static void CaptureValidationScreenshots()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var cameraObject = GameObject.Find("D19_TerrainWorkCamera");
            if (cameraObject == null) throw new InvalidOperationException("D19 terrain validation camera is missing.");
            var camera = cameraObject.GetComponent<Camera>();
            if (camera == null) throw new InvalidOperationException("D19 terrain validation camera component is missing.");

            var outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, ResultRoot.Replace('/', Path.DirectorySeparatorChar)));
            Directory.CreateDirectory(outputPath);
            var shots = new[]
            {
                new ScreenshotSpec("01_terrain_panorama", new Vector3(42f, 44f, -102f), new Vector3(0f, 4f, 22f)),
                new ScreenshotSpec("02_coast_beach", new Vector3(0f, 18f, -58f), new Vector3(0f, 1f, -12f)),
                new ScreenshotSpec("03_road_approach", new Vector3(-45f, 16f, 36f), new Vector3(0f, 1f, 50f)),
                new ScreenshotSpec("04_cave_fissure", new Vector3(95f, 30f, 103f), new Vector3(72f, TerrainHeight(72f, 72f) + 2f, 72f)),
                new ScreenshotSpec("05_reef_boundary", new Vector3(35f, 16f, -85f), new Vector3(0f, -1f, -66f))
            };

            var renderTexture = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var texture = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            camera.targetTexture = renderTexture;
            for (var i = 0; i < shots.Length; i++)
            {
                camera.transform.position = shots[i].position;
                camera.transform.LookAt(shots[i].lookAt);
                camera.Render();
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
                texture.Apply(false, false);
                File.WriteAllBytes(Path.Combine(outputPath, "D19_A01_" + shots[i].id + ".png"), texture.EncodeToPNG());
            }
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(texture);
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
            EditorSceneManager.CloseScene(scene, true);

            var summary = "{\n  \"task\": \"D19-A01\",\n  \"screenshot_count\": " + shots.Length + ",\n  \"resolution\": [1280, 720],\n  \"files\": [\n";
            for (var i = 0; i < shots.Length; i++)
                summary += "    \"tools/results/D19/D19-A01/D19_A01_" + shots[i].id + ".png\"" + (i + 1 == shots.Length ? "\n" : ",\n");
            summary += "  ],\n  \"formal_initial_harbor_modified\": false\n}\n";
            File.WriteAllText(Path.Combine(outputPath, "D19_A01_SCREENSHOTS.json"), summary);
            Debug.Log("[D19-A01] Validation screenshots captured: " + shots.Length);
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Scenes", "D19");
            EnsureFolder("Assets/Scenes/D19", "Work");
            EnsureFolder("Assets/Prefabs", "Environment");
            EnsureFolder("Assets/Prefabs/Environment", "D19");
            EnsureFolder(PrefabRoot.Substring(0, PrefabRoot.LastIndexOf('/')), "Terrain");
            EnsureFolder("Assets/Art", "Materials");
            EnsureFolder("Assets/Art/Materials", "D19");
            EnsureFolder("Assets/Art/Materials/D19", "A01");
            EnsureFolder("Assets/Scenes/D19/Work", "Data");
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

        private static void DeleteOwnArtifacts()
        {
            var paths = new[]
            {
                ScenePath, TerrainPrefabPath, CollisionPrefabPath, AnchorPrefabPath, SurfacePrefabPath,
                CavePrefabPath, PresentationPrefabPath, NavMeshPrefabPath, ProbePrefabPath, TerrainDataPath, NavMeshDataPath,
                NavMeshSourceMeshPath,
                TerrainVisualMeshPath,
                MaterialRoot + "/D19_A01_Sand.mat", MaterialRoot + "/D19_A01_Rock.mat",
                MaterialRoot + "/D19_A01_Grass.mat", MaterialRoot + "/D19_A01_Mud.mat",
                MaterialRoot + "/D19_A01_CaveInterior.mat", MaterialRoot + "/D19_A01_SandLayer.terrainlayer",
                MaterialRoot + "/D19_A01_RockLayer.terrainlayer", MaterialRoot + "/D19_A01_GrassLayer.terrainlayer",
                MaterialRoot + "/D19_A01_MudLayer.terrainlayer"
            };
            foreach (var path in paths)
            {
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                    AssetDatabase.DeleteAsset(path);
            }
            AssetDatabase.SaveAssets();
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            var materials = new Dictionary<string, Material>();
            materials["sand"] = CreateMaterial("D19_A01_Sand", "Assets/Art/Candidates/Environment/PolyHaven/Textures/sand_01_diff_1k.jpg", new Color(0.78f, 0.67f, 0.48f));
            materials["rock"] = CreateMaterial("D19_A01_Rock", "Assets/Art/Candidates/Environment/PolyHaven/Textures/coral_mud_01_diff_1k.jpg", new Color(0.32f, 0.31f, 0.27f));
            materials["grass"] = CreateMaterial("D19_A01_Grass", null, new Color(0.23f, 0.39f, 0.20f));
            materials["mud"] = CreateMaterial("D19_A01_Mud", "Assets/Art/Candidates/Environment/PolyHaven/Textures/coral_mud_01_diff_1k.jpg", new Color(0.39f, 0.28f, 0.20f));
            materials["cave"] = CreateMaterial("D19_A01_CaveInterior", null, new Color(0.025f, 0.018f, 0.012f));
            return materials;
        }

        private static Material CreateMaterial(string name, string texturePath, Color fallback)
        {
            var path = MaterialRoot + "/" + name + ".mat";
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture") ??
                         Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { name = name };
            material.color = fallback;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", fallback);
            if (material.HasProperty("_Color")) material.SetColor("_Color", fallback);
            if (!string.IsNullOrWhiteSpace(texturePath))
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture != null)
                {
                    material.color = Color.white;
                    if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
                    if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
                    material.mainTexture = texture;
                }
                else
                {
                    Debug.LogWarning("[D19-A01] Intake texture not loaded, using documented color fallback: " + texturePath);
                }
            }
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static TerrainData CreateTerrainData(Dictionary<string, Material> materials)
        {
            var data = new TerrainData { name = "D19_SaintMargaret_TerrainData" };
            data.heightmapResolution = 257;
            data.alphamapResolution = 256;
            data.baseMapResolution = 256;
            data.size = TerrainSize;

            var heights = new float[data.heightmapResolution, data.heightmapResolution];
            for (var z = 0; z < data.heightmapResolution; z++)
            {
                var worldZ = TerrainPosition.z + TerrainSize.z * z / (data.heightmapResolution - 1f);
                for (var x = 0; x < data.heightmapResolution; x++)
                {
                    var worldX = TerrainPosition.x + TerrainSize.x * x / (data.heightmapResolution - 1f);
                    var height = TerrainHeight(worldX, worldZ);
                    heights[z, x] = Mathf.Clamp01((height - TerrainPosition.y) / TerrainSize.y);
                }
            }
            data.SetHeights(0, 0, heights);

            var layers = new[]
            {
                CreateTerrainLayer("D19_A01_SandLayer", materials["sand"], new Vector2(12f, 12f)),
                CreateTerrainLayer("D19_A01_RockLayer", materials["rock"], new Vector2(10f, 10f)),
                CreateTerrainLayer("D19_A01_GrassLayer", materials["grass"], new Vector2(16f, 16f)),
                CreateTerrainLayer("D19_A01_MudLayer", materials["mud"], new Vector2(8f, 8f))
            };
            data.terrainLayers = layers;

            var alpha = new float[data.alphamapResolution, data.alphamapResolution, layers.Length];
            for (var z = 0; z < data.alphamapResolution; z++)
            {
                var worldZ = TerrainPosition.z + TerrainSize.z * (z + 0.5f) / data.alphamapResolution;
                for (var x = 0; x < data.alphamapResolution; x++)
                {
                    var worldX = TerrainPosition.x + TerrainSize.x * (x + 0.5f) / data.alphamapResolution;
                    var coast = Coastline(worldX);
                    var land = SmoothStep(coast - 2f, coast + 16f, worldZ);
                    var rock = Mathf.Clamp01((TerrainHeight(worldX, worldZ) - 7f) / 12f) + Mathf.Clamp01(Mathf.Abs(worldX - 72f) / 100f) * 0.05f;
                    var mud = Mathf.Clamp01(1f - Mathf.Abs(worldZ - coast - 8f) / 14f) * 0.45f;
                    var sand = Mathf.Clamp01(1f - land) + (1f - rock) * Mathf.Clamp01(1f - land) * 0.35f;
                    var grass = Mathf.Clamp01(land * (1f - rock) * (1f - mud));
                    var total = sand + rock + grass + mud;
                    alpha[z, x, 0] = sand / total;
                    alpha[z, x, 1] = rock / total;
                    alpha[z, x, 2] = grass / total;
                    alpha[z, x, 3] = mud / total;
                }
            }
            data.SetAlphamaps(0, 0, alpha);
            AssetDatabase.CreateAsset(data, TerrainDataPath);
            foreach (var layer in layers) AssetDatabase.CreateAsset(layer, MaterialRoot + "/" + layer.name + ".terrainlayer");
            return data;
        }

        private static TerrainLayer CreateTerrainLayer(string name, Material material, Vector2 tileSize)
        {
            var layer = new TerrainLayer { name = name, tileSize = tileSize };
            if (material != null)
            {
                var baseMap = material.GetTexture("_BaseMap") ?? material.GetTexture("_MainTex");
                layer.diffuseTexture = baseMap as Texture2D;
            }
            return layer;
        }

        private static float TerrainHeight(float x, float z)
        {
            var coast = Coastline(x);
            var land = SmoothStep(coast - 2f, coast + 18f, z);
            var highland = Gaussian(x, z, 68f, 76f, 34f, 13f);
            var ridge = Gaussian(x, z, -72f, 82f, 45f, 7f);
            var terrace = 1.2f * Mathf.Sin((x + 18f) * 0.045f) * Mathf.Sin((z - 8f) * 0.026f) * land;
            var beach = 0.45f * SmoothStep(coast, coast + 9f, z);
            var seaBed = Mathf.Lerp(-7f, -1.2f, SmoothStep(coast - 12f, coast + 1f, z));
            var height = Mathf.Lerp(seaBed, 0.35f + highland + ridge + terrace + beach, land);
            height = FlattenNearRoads(x, z, height);
            return height;
        }

        private static float Coastline(float x)
        {
            return -44f + 7f * Mathf.Sin(x * 0.028f) + 3f * Mathf.Sin(x * 0.071f + 0.8f);
        }

        private static float FlattenNearRoads(float x, float z, float height)
        {
            var distance = Mathf.Min(
                DistanceToSegment(x, z, -22f, 88f, 26f, 62f),
                DistanceToSegment(x, z, 26f, 62f, 48f, 34f),
                DistanceToSegment(x, z, 26f, 62f, 0f, -12f),
                DistanceToSegment(x, z, -42f, 50f, 0f, -12f));
            var blend = 1f - SmoothStep(4f, 10f, distance);
            return Mathf.Lerp(height, 0.55f, blend * 0.72f);
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

        private static GameObject CreateTerrainPrefab(TerrainData data, Dictionary<string, Material> materials)
        {
            var root = Terrain.CreateTerrainGameObject(data);
            root.name = "D19_TerrainRoot";
            root.transform.position = TerrainPosition;
            root.isStatic = true;
            var terrain = root.GetComponent<Terrain>();
            terrain.drawInstanced = false;
            terrain.basemapDistance = 1000f;
            terrain.drawHeightmap = false;
            CreateTerrainVisualMesh(root.transform, materials);
            GameObjectUtility.SetStaticEditorFlags(root, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, TerrainPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreateTerrainVisualMesh(Transform parent, Dictionary<string, Material> materials)
        {
            const int xResolution = 65;
            const int zResolution = 69;
            var vertices = new Vector3[xResolution * zResolution];
            var uvs = new Vector2[xResolution * zResolution];
            var submeshTriangles = new[]
            {
                new List<int>(), new List<int>(), new List<int>(), new List<int>()
            };
            for (var z = 0; z < zResolution; z++)
            {
                var worldZ = TerrainPosition.z + TerrainSize.z * z / (zResolution - 1f);
                for (var x = 0; x < xResolution; x++)
                {
                    var worldX = TerrainPosition.x + TerrainSize.x * x / (xResolution - 1f);
                    vertices[z * xResolution + x] = new Vector3(
                        worldX - TerrainPosition.x,
                        TerrainHeight(worldX, worldZ) - TerrainPosition.y + 0.025f,
                        worldZ - TerrainPosition.z);
                    uvs[z * xResolution + x] = new Vector2(
                        (worldX - TerrainPosition.x) / 12f,
                        (worldZ - TerrainPosition.z) / 12f);
                }
            }

            for (var z = 0; z < zResolution - 1; z++)
            {
                for (var x = 0; x < xResolution - 1; x++)
                {
                    var i0 = z * xResolution + x;
                    var i1 = i0 + 1;
                    var i2 = i0 + xResolution;
                    var i3 = i2 + 1;
                    var worldX = TerrainPosition.x + TerrainSize.x * (x + 0.5f) / (xResolution - 1f);
                    var worldZ = TerrainPosition.z + TerrainSize.z * (z + 0.5f) / (zResolution - 1f);
                    var materialIndex = DominantTerrainMaterial(worldX, worldZ);
                    submeshTriangles[materialIndex].Add(i0);
                    submeshTriangles[materialIndex].Add(i2);
                    submeshTriangles[materialIndex].Add(i1);
                    submeshTriangles[materialIndex].Add(i1);
                    submeshTriangles[materialIndex].Add(i2);
                    submeshTriangles[materialIndex].Add(i3);
                }
            }

            var mesh = new Mesh { name = "D19_SaintMargaret_TerrainVisualMesh" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.subMeshCount = submeshTriangles.Length;
            for (var i = 0; i < submeshTriangles.Length; i++)
                mesh.SetTriangles(submeshTriangles[i], i, true);
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, TerrainVisualMeshPath);

            var visual = new GameObject("D19_TerrainVisualMesh") { isStatic = true };
            visual.transform.SetParent(parent, false);
            var filter = visual.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = visual.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { materials["sand"], materials["rock"], materials["grass"], materials["mud"] };
            GameObjectUtility.SetStaticEditorFlags(visual, StaticEditorFlags.BatchingStatic);
        }

        private static int DominantTerrainMaterial(float x, float z)
        {
            var coast = Coastline(x);
            var land = SmoothStep(coast - 2f, coast + 16f, z);
            var rock = Mathf.Clamp01((TerrainHeight(x, z) - 7f) / 12f) + Mathf.Clamp01(Mathf.Abs(x - 72f) / 100f) * 0.05f;
            var mud = Mathf.Clamp01(1f - Mathf.Abs(z - coast - 8f) / 14f) * 0.45f;
            var sand = Mathf.Clamp01(1f - land) + (1f - rock) * Mathf.Clamp01(1f - land) * 0.35f;
            var grass = Mathf.Clamp01(land * (1f - rock) * (1f - mud));
            if (rock >= sand && rock >= grass && rock >= mud) return 1;
            if (grass >= sand && grass >= mud) return 2;
            if (mud >= sand) return 3;
            return 0;
        }

        private static GameObject CreateCollisionPrefab()
        {
            var root = new GameObject("D19_TerrainCollision") { isStatic = true };
            AddBox(root.transform, "D19_ReefCollision", new Vector3(-16f, -0.8f, -66f), new Vector3(108f, 2f, 5f), false);
            AddBox(root.transform, "D19_CliffExclusion", new Vector3(72f, TerrainHeight(72f, 72f) + 5f, 72f), new Vector3(68f, 10f, 5f), false);
            AddBox(root.transform, "D19_CaveInteriorBlocker", new Vector3(72f, TerrainHeight(72f, 72f) + 1.2f, 68f), new Vector3(5f, 2.4f, 2f), false);
            AddBoundary(root.transform, "D19_BoundaryNorth", new Vector3(0f, 12f, 144f), new Vector3(256f, 24f, 1f));
            AddBoundary(root.transform, "D19_BoundaryWest", new Vector3(-128f, 12f, 8f), new Vector3(1f, 24f, 272f));
            AddBoundary(root.transform, "D19_BoundaryEast", new Vector3(128f, 12f, 8f), new Vector3(1f, 24f, 272f));
            AddBoundary(root.transform, "D19_DeepSeaBoundary", new Vector3(0f, -1f, -128f), new Vector3(256f, 10f, 1f));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, CollisionPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void AddBox(Transform parent, string name, Vector3 position, Vector3 size, bool trigger)
        {
            var go = new GameObject(name) { isStatic = true };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var collider = go.AddComponent<BoxCollider>();
            collider.size = size;
            collider.isTrigger = trigger;
        }

        private static void AddBoundary(Transform parent, string name, Vector3 position, Vector3 size)
        {
            AddBox(parent, name, position, size, true);
        }

        private static GameObject CreateAnchorPrefab()
        {
            var root = new GameObject("D19_TerrainAnchors");
            var anchors = new Dictionary<string, Vector3>
            {
                ["anchor_player_recovery"] = new Vector3(-22f, 0.2f, 82f),
                ["anchor_beach_landing"] = new Vector3(-58f, 0.2f, -8f),
                ["anchor_shallow_waterline"] = new Vector3(-16f, 0f, -48f),
                ["anchor_vegetation_boundary"] = new Vector3(-72f, 0.5f, 54f),
                ["anchor_road_clearance"] = new Vector3(2f, 0.55f, 52f),
                ["anchor_cliff_exclusion"] = new Vector3(72f, 6f, 72f),
                ["anchor_reef_boundary"] = new Vector3(-16f, -1.5f, -66f),
                ["anchor_channel_safe"] = new Vector3(0f, -2f, -96f)
            };
            foreach (var pair in anchors)
            {
                var anchor = new GameObject(pair.Key);
                anchor.transform.SetParent(root.transform, false);
                anchor.transform.localPosition = pair.Value;
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, AnchorPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateCavePrefab(Material caveMaterial)
        {
            var root = new GameObject("D19_CaveFissure") { isStatic = true };
            var left = CreatePrimitive(root.transform, "CaveLeftPillar", PrimitiveType.Cube, new Vector3(-3.2f, 2.2f, 0f), new Vector3(2.2f, 4.4f, 5f), caveMaterial);
            var right = CreatePrimitive(root.transform, "CaveRightPillar", PrimitiveType.Cube, new Vector3(3.2f, 2.2f, 0f), new Vector3(2.2f, 4.4f, 5f), caveMaterial);
            var lintel = CreatePrimitive(root.transform, "CaveLintel", PrimitiveType.Cube, new Vector3(0f, 4.6f, 0f), new Vector3(8.6f, 2f, 5f), caveMaterial);
            var interior = CreatePrimitive(root.transform, "CaveInterior", PrimitiveType.Cube, new Vector3(0f, 1.6f, -2.6f), new Vector3(4f, 2.2f, 0.2f), caveMaterial);
            left.isStatic = true; right.isStatic = true; lintel.isStatic = true; interior.isStatic = true;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, CavePrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static GameObject CreateSurfacePrefab(Material rockMaterial)
        {
            var root = new GameObject("D19_TerrainSurfaceSet") { isStatic = true };
            var placements = new[]
            {
                new ModelPlacement("cliff_corner_rock.fbx", SurfacePosition(72f, 76f, 0.1f), 1.8f),
                new ModelPlacement("rock_largeF.fbx", SurfacePosition(88f, 58f, 0.1f), 1.5f),
                new ModelPlacement("rock_tallF.fbx", SurfacePosition(56f, 92f, 0.1f), 1.3f),
                new ModelPlacement("ground_pathBend.fbx", SurfacePosition(-6f, 44f, 0.1f), 1.0f),
                new ModelPlacement("bridge_wood.fbx", new Vector3(-58f, 0.65f, -8f), 1.0f),
                new ModelPlacement("cliff_stepsCorner_stone.fbx", new Vector3(58f, 0.45f, 62f), 1.0f)
            };
            var imported = 0;
            foreach (var placement in placements)
            {
                var source = NatureRoot + "/" + placement.fileName;
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(source);
                if (asset == null)
                {
                    Debug.LogError("[D19-A01] Required intake-eligible environment FBX missing: " + source);
                    continue;
                }
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
                instance.name = "Intake_" + Path.GetFileNameWithoutExtension(placement.fileName);
                instance.transform.SetParent(root.transform, false);
                instance.transform.localPosition = placement.position;
                instance.transform.localScale = Vector3.one * placement.scale;
                instance.isStatic = true;
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer.sharedMaterial == null && rockMaterial != null) renderer.sharedMaterial = rockMaterial;
                    renderer.gameObject.isStatic = true;
                }
                foreach (var filter in instance.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null) continue;
                    var collider = filter.gameObject.GetComponent<MeshCollider>();
                    if (collider == null) collider = filter.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = filter.sharedMesh;
                    collider.convex = false;
                }
                imported++;
            }
            if (imported != placements.Length)
                throw new InvalidOperationException("D19-A01 surface set did not import all six intake-eligible environment models.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, SurfacePrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static Vector3 SurfacePosition(float x, float z, float offset)
        {
            return new Vector3(x, TerrainHeight(x, z) + offset, z);
        }

        private static GameObject CreatePresentationPrefab()
        {
            var root = new GameObject("D19_TerrainPresentation");
            var camera = new GameObject("D19_TerrainWorkCamera");
            camera.transform.SetParent(root.transform, false);
            camera.transform.position = new Vector3(42f, 44f, -102f);
            camera.transform.LookAt(new Vector3(0f, 4f, 22f));
            var cameraComponent = camera.AddComponent<Camera>();
            cameraComponent.fieldOfView = 55f;
            cameraComponent.farClipPlane = 500f;
            cameraComponent.clearFlags = CameraClearFlags.Skybox;
            var light = new GameObject("D19_TerrainWorkKeyLight");
            light.transform.SetParent(root.transform, false);
            light.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            var lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.15f;
            lightComponent.color = new Color(1f, 0.86f, 0.7f);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PresentationPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateProbePrefab()
        {
            var root = new GameObject("D19_TerrainRuntimeProbe");
            root.AddComponent<WhalesIslandReef.D19A01.D19A01TerrainRuntimeProbe>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, ProbePrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static NavMeshData BuildNavMeshData(Scene scene)
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();
            var sourceMesh = CreateNavMeshSourceMesh();
            sources.Add(new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = sourceMesh,
                transform = Matrix4x4.identity,
                area = 0
            });
            var settings = NavMesh.GetSettingsByID(0);
            settings.agentRadius = 0.35f;
            settings.agentHeight = 1.8f;
            settings.agentSlope = 38f;
            settings.agentClimb = 0.35f;
            var bounds = new Bounds(new Vector3(0f, 8f, 8f), new Vector3(256f, 48f, 272f));
            var data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
            if (data == null) throw new InvalidOperationException("NavMeshBuilder returned null for D19 terrain work scene.");
            data.name = "D19_SaintMargaret_NavMesh";
            AssetDatabase.CreateAsset(data, NavMeshDataPath);
            return data;
        }

        private static Mesh CreateNavMeshSourceMesh()
        {
            if (AssetDatabase.LoadAssetAtPath<Mesh>(NavMeshSourceMeshPath) != null)
                AssetDatabase.DeleteAsset(NavMeshSourceMeshPath);

            const int xResolution = 65;
            const int zResolution = 69;
            var vertices = new Vector3[xResolution * zResolution];
            var triangles = new int[(xResolution - 1) * (zResolution - 1) * 6];
            for (var z = 0; z < zResolution; z++)
            {
                var worldZ = TerrainPosition.z + TerrainSize.z * z / (zResolution - 1f);
                for (var x = 0; x < xResolution; x++)
                {
                    var worldX = TerrainPosition.x + TerrainSize.x * x / (xResolution - 1f);
                    vertices[z * xResolution + x] = new Vector3(worldX, TerrainHeight(worldX, worldZ), worldZ);
                }
            }

            var triangleIndex = 0;
            for (var z = 0; z < zResolution - 1; z++)
            {
                for (var x = 0; x < xResolution - 1; x++)
                {
                    var i0 = z * xResolution + x;
                    var i1 = i0 + 1;
                    var i2 = i0 + xResolution;
                    var i3 = i2 + 1;
                    triangles[triangleIndex++] = i0;
                    triangles[triangleIndex++] = i2;
                    triangles[triangleIndex++] = i1;
                    triangles[triangleIndex++] = i1;
                    triangles[triangleIndex++] = i2;
                    triangles[triangleIndex++] = i3;
                }
            }

            var mesh = new Mesh { name = "D19_SaintMargaret_NavMeshSource" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, NavMeshSourceMeshPath);
            return mesh;
        }

        private static GameObject CreateNavMeshPrefab(NavMeshData data)
        {
            var root = new GameObject("D19_TerrainNavMesh");
            var runtime = root.AddComponent<WhalesIslandReef.D19A01.D19A01TerrainNavMeshRuntime>();
            runtime.NavMeshData = data;
            runtime.AgentType = "HumanoidDefault";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, NavMeshPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Scene scene, Vector3 position)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            SceneManager.MoveGameObjectToScene(instance, scene);
            instance.transform.position = position;
            return instance;
        }

        private static string ProjectFile(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar)));
        }

        private static void WriteBuildResult(TerrainData terrainData, NavMeshData navMeshData, Scene scene)
        {
            var result = new D19A01BuildResult
            {
                task = "D19-A01",
                status = "PASS",
                scene_path = ScenePath,
                terrain_prefab = TerrainPrefabPath,
                runtime_probe_prefab = ProbePrefabPath,
                terrain_data = TerrainDataPath,
                navmesh_data = NavMeshDataPath,
                terrain_resolution = terrainData.heightmapResolution,
                terrain_size_m = new[] { terrainData.size.x, terrainData.size.y, terrainData.size.z },
                terrain_layer_count = terrainData.terrainLayers == null ? 0 : terrainData.terrainLayers.Length,
                scene_root_count = scene.rootCount,
                external_model_count = 6,
                collision_contract_count = 7,
                navmesh_source = "D19_SaintMargaret_NavMeshSource mesh generated from frozen TerrainHeight plus UnityEngine.AI.NavMeshBuilder",
                intake_manifest_ids = new[]
                {
                    "pack_kenney_nature_kit_d18_a03_v1",
                    "pack_polyhaven_sand_01_diff_jpg_d18_a03_v1",
                    "pack_polyhaven_coral_mud_01_diff_jpg_d18_a03_v1"
                },
                local_development_ready = false,
                release_approved = false,
                formal_initial_harbor_modified = false
            };
            var resultPath = Path.GetFullPath(Path.Combine(Application.dataPath, ResultRoot.Replace('/', Path.DirectorySeparatorChar)));
            Directory.CreateDirectory(resultPath);
            File.WriteAllText(Path.Combine(resultPath, "D19_A01_BUILD_RESULT.json"), JsonUtility.ToJson(result, true));
        }

        private struct ModelPlacement
        {
            public readonly string fileName;
            public readonly Vector3 position;
            public readonly float scale;

            public ModelPlacement(string fileName, Vector3 position, float scale)
            {
                this.fileName = fileName;
                this.position = position;
                this.scale = scale;
            }
        }

        private struct ScreenshotSpec
        {
            public readonly string id;
            public readonly Vector3 position;
            public readonly Vector3 lookAt;

            public ScreenshotSpec(string id, Vector3 position, Vector3 lookAt)
            {
                this.id = id;
                this.position = position;
                this.lookAt = lookAt;
            }
        }

        [Serializable]
        private sealed class D19A01BuildResult
        {
            public string task;
            public string status;
            public string scene_path;
            public string terrain_prefab;
            public string runtime_probe_prefab;
            public string terrain_data;
            public string navmesh_data;
            public int terrain_resolution;
            public float[] terrain_size_m;
            public int terrain_layer_count;
            public int scene_root_count;
            public int external_model_count;
            public int collision_contract_count;
            public string navmesh_source;
            public string[] intake_manifest_ids;
            public bool local_development_ready;
            public bool release_approved;
            public bool formal_initial_harbor_modified;
        }
    }
}
#endif
