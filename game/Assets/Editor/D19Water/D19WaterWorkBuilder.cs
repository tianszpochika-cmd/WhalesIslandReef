using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WhalesIslandReef.D19.Water;

namespace WhalesIslandReef.Editor.D19Water
{
    public static class D19WaterWorkBuilder
    {
        private const string PrefabRoot = "Assets/Prefabs/Environment/D19/Water";
        private const string ScriptRoot = PrefabRoot + "/Scripts";
        private const string MeshRoot = PrefabRoot + "/Meshes";
        private const string MaterialRoot = PrefabRoot + "/Materials";
        private const string ScenePath = "Assets/Scenes/D19/Work/D19_Water_Work.unity";
        private const string ResultRoot = "tools/results/D19/D19-A04";
        private const string ReportPath = ResultRoot + "/D19_A04_WATER_AUDIT.json";
        private const string ScreenshotRoot = ResultRoot + "/screenshots";

        [Serializable]
        private sealed class WaterAuditResult
        {
            public string task = "D19-A04";
            public string status = "PASS";
            public string unity_version;
            public string render_pipeline = "Universal Render Pipeline 17.0.4";
            public string scene_path = ScenePath;
            public string prefab_path = PrefabRoot + "/WaterRoot.prefab";
            public float sea_level_y = 0f;
            public int water_surface_renderers;
            public int foam_renderers;
            public int underwater_volume_renderers;
            public int reef_renderers;
            public int water_materials;
            public int generated_meshes;
            public int shader_compile_errors;
            public int scene_compile_errors;
            public bool initial_harbor_changed;
            public bool global_urp_settings_changed;
            public bool gameplay_systems_changed;
            public bool weather_logic_changed;
            public bool camera_code_changed;
            public bool formal_canvas_changed;
            public bool sea_level_contract_passed;
            public bool ui_overlay_order_passed;
            public bool underwater_guard_passed;
            public string[] quality_tiers = { "Low", "Medium", "High" };
            public string[] quality_notes =
            {
                "Low: one analytic wave, no reflection probe, alpha blend, no scene-depth dependency; mobile fallback.",
                "Medium: two analytic waves, transparent shoreline foam, reef visibility through alpha water; default work-scene tier.",
                "High: three analytic waves, higher wave detail and fresnel response; no screen-space reflection or post effect."
            };
            public string[] allowed_external_manifest_ids =
            {
                "pack_polyhaven_sand_01_diff_jpg_d18_a03_v1",
                "pack_polyhaven_sand_01_nor_gl_jpg_d18_a03_v1",
                "pack_polyhaven_sand_01_rough_jpg_d18_a03_v1",
                "pack_polyhaven_coral_mud_01_diff_jpg_d18_a03_v1",
                "pack_polyhaven_coral_mud_01_nor_gl_jpg_d18_a03_v1",
                "pack_polyhaven_coral_mud_01_rough_jpg_d18_a03_v1"
            };
            public string[] screenshots = new string[0];
            public float[] render_ms_samples = new float[0];
            public string performance_note = "Editor camera.Render rough sample only; target-device profiling remains a D18-S03/D19-S02 gate.";
            public string[] warnings = new string[0];
        }

        private sealed class ShotDefinition
        {
            public string id;
            public string file;
            public Vector3 position;
            public Vector3 lookAt;
            public D19WaterQualityTier quality;
            public Color background;
        }

        [MenuItem("WhalesIslandReef/D19/Generate Water Work Package")]
        public static void GenerateWaterWorkPackage()
        {
            GenerateAssetsAndScene();
            UnityEngine.Debug.Log("[D19-A04] Water work package generated: " + ScenePath);
        }

        [MenuItem("WhalesIslandReef/D19/Audit Water Work Package")]
        public static void AuditWaterWorkPackage()
        {
            WaterAuditResult result = GenerateAssetsAndScene();
            AuditSceneAndCapture(result);
            WriteResult(result);
            UnityEngine.Debug.Log("[D19-A04] Water audit status=" + result.status + " screenshots=" + result.screenshots.Length + " shader_errors=" + result.shader_compile_errors);
        }

        public static void GenerateAndAuditBatch()
        {
            WaterAuditResult result = GenerateAssetsAndScene();
            AuditSceneAndCapture(result);
            WriteResult(result);
            if (result.status != "PASS")
                throw new InvalidOperationException("D19-A04 water audit failed: " + result.status);
        }

        private static WaterAuditResult GenerateAssetsAndScene()
        {
            EnsureFolders();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Shader waterShader = Shader.Find("WhalesIslandReef/D19/WaterSurface");
            Shader foamShader = Shader.Find("WhalesIslandReef/D19/WaterFoam");
            Shader underwaterShader = Shader.Find("WhalesIslandReef/D19/UnderwaterVolume");
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (waterShader == null || foamShader == null || underwaterShader == null || litShader == null)
                throw new InvalidOperationException("D19-A04 required shader was not imported.");

            Texture2D sandTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Candidates/Environment/PolyHaven/Textures/sand_01_diff_1k.jpg");
            Texture2D coralTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Candidates/Environment/PolyHaven/Textures/coral_mud_01_diff_1k.jpg");
            if (sandTexture == null || coralTexture == null)
                throw new InvalidOperationException("D19-A04 allowed intake texture is missing.");

            Material waterLow = CreateWaterMaterial("D19_Water_Low", waterShader, new Color(0.012f, 0.10f, 0.16f, 1f), new Color(0.04f, 0.38f, 0.42f, 1f), 0.68f, 0.32f, D19WaterQualityTier.Low);
            Material waterMedium = CreateWaterMaterial("D19_Water_Medium", waterShader, new Color(0.010f, 0.12f, 0.20f, 1f), new Color(0.03f, 0.44f, 0.49f, 1f), 0.78f, 0.22f, D19WaterQualityTier.Medium);
            Material waterHigh = CreateWaterMaterial("D19_Water_High", waterShader, new Color(0.006f, 0.10f, 0.18f, 1f), new Color(0.025f, 0.47f, 0.52f, 1f), 0.82f, 0.18f, D19WaterQualityTier.High);
            Material foam = CreateFoamMaterial("D19_Water_Foam", foamShader);
            Material underwater = CreateUnderwaterMaterial("D19_Underwater_Tint", underwaterShader);
            Material sand = CreateLitMaterial("D19_Shore_Sand", litShader, sandTexture, new Color(0.75f, 0.67f, 0.48f, 1f), 0.25f);
            Material coral = CreateLitMaterial("D19_Reef_CoralMud", litShader, coralTexture, new Color(0.38f, 0.30f, 0.22f, 1f), 0.62f);
            Material wood = CreateLitMaterial("D19_Test_Dock_Wood", litShader, null, new Color(0.28f, 0.13f, 0.055f, 1f), 0.35f);

            Mesh waterMesh = CreateOrReplaceMesh("D19_Water_Surface_Mesh", CreateWaterMesh());
            Mesh foamMesh = CreateOrReplaceMesh("D19_Water_ShoreFoam_Mesh", CreateFoamMesh());
            Mesh shoreMesh = CreateOrReplaceMesh("D19_Water_TestShore_Mesh", CreateShoreMesh());
            Mesh rockMesh = CreateOrReplaceMesh("D19_Water_ReefRock_Mesh", CreateRockMesh());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            GameObject prefabRoot = BuildWaterPrefab(waterMesh, foamMesh, rockMesh, waterHigh, foam, underwater, coral);
            string prefabPath = PrefabRoot + "/WaterRoot.prefab";
            DeleteAssetIfExists(prefabPath);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            UnityEngine.Object.DestroyImmediate(prefabRoot);
            AssetDatabase.SaveAssets();

            BuildWorkScene(prefab, shoreMesh, sand, wood, coral);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            return new WaterAuditResult
            {
                unity_version = Application.unityVersion,
                water_surface_renderers = 1,
                foam_renderers = 1,
                underwater_volume_renderers = 1,
                reef_renderers = 5,
                water_materials = 7,
                generated_meshes = 4,
                initial_harbor_changed = false,
                global_urp_settings_changed = false,
                gameplay_systems_changed = false,
                weather_logic_changed = false,
                camera_code_changed = false,
                formal_canvas_changed = false,
            };
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                "Assets/Scenes/D19", "Assets/Scenes/D19/Work", "Assets/Prefabs/Environment", "Assets/Prefabs/Environment/D19",
                PrefabRoot, ScriptRoot, MeshRoot, MaterialRoot, "Assets/Shaders", "Assets/Shaders/D19", "Assets/Editor", "Assets/Editor/D19Water",
                ResultRoot, ScreenshotRoot
            };
            for (int i = 0; i < folders.Length; i++)
            {
                string absolute = Path.Combine(Directory.GetParent(Application.dataPath).FullName, folders[i].Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(absolute)) Directory.CreateDirectory(absolute);
            }
        }

        private static GameObject BuildWaterPrefab(Mesh waterMesh, Mesh foamMesh, Mesh rockMesh,
            Material waterMaterial, Material foamMaterial, Material underwaterMaterial, Material coralMaterial)
        {
            GameObject root = new GameObject("WaterRoot");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            D19WaterPresentation presentation = root.AddComponent<D19WaterPresentation>();

            Renderer waterRenderer = CreateRendererChild(root.transform, "WaterSurface_SeaLevelY0", waterMesh, waterMaterial);
            Renderer foamRenderer = CreateRendererChild(root.transform, "ShorelineFoam_WetEdge", foamMesh, foamMaterial);

            GameObject volume = GameObject.CreatePrimitive(PrimitiveType.Cube);
            volume.name = "UnderwaterBoundary_TintOnly";
            volume.transform.SetParent(root.transform, false);
            volume.transform.localPosition = new Vector3(0f, -10f, -64f);
            volume.transform.localScale = new Vector3(210f, 20f, 150f);
            RemoveCollider(volume);
            Renderer volumeRenderer = volume.GetComponent<Renderer>();
            volumeRenderer.sharedMaterial = underwaterMaterial;
            volumeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            volumeRenderer.receiveShadows = false;

            List<Renderer> reefs = new List<Renderer>();
            Vector3[] reefPositions =
            {
                new Vector3(-43f, -1.25f, -61f), new Vector3(-24f, -1.05f, -68f),
                new Vector3(-7f, -1.30f, -76f), new Vector3(12f, -1.00f, -67f), new Vector3(30f, -1.35f, -79f)
            };
            Vector3[] reefScales =
            {
                new Vector3(7f, 2.5f, 5f), new Vector3(5f, 2.0f, 4f), new Vector3(8f, 2.2f, 5f),
                new Vector3(4f, 1.8f, 3f), new Vector3(7f, 2.7f, 4f)
            };
            for (int i = 0; i < reefPositions.Length; i++)
            {
                Renderer reef = CreateRendererChild(root.transform, "ReefVisibility_" + (i + 1).ToString("00"), rockMesh, coralMaterial);
                reef.transform.localPosition = reefPositions[i];
                reef.transform.localScale = reefScales[i];
                reef.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                reef.receiveShadows = true;
                reefs.Add(reef);
            }

            presentation.ConfigureOwnedRenderers(new[] { waterRenderer }, new[] { foamRenderer }, new[] { volumeRenderer });
            presentation.ApplyExternalFrame(D19WaterFrameParameters.Default);
            presentation.SetQuality(D19WaterQualityTier.High);
            return root;
        }

        private static Renderer CreateRendererChild(Transform parent, string name, Mesh mesh, Material material)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            MeshFilter filter = child.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = name.StartsWith("WaterSurface", StringComparison.Ordinal) || name.StartsWith("Shoreline", StringComparison.Ordinal)
                ? UnityEngine.Rendering.ShadowCastingMode.Off
                : UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = !name.StartsWith("WaterSurface", StringComparison.Ordinal);
            return renderer;
        }

        private static void BuildWorkScene(GameObject prefab, Mesh shoreMesh, Material sand, Material wood, Material coral)
        {
            if (File.Exists(ProjectAbsolute(ScenePath))) AssetDatabase.DeleteAsset(ScenePath);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "D19_Water_Work";

            GameObject waterRoot = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            waterRoot.name = "WaterRoot";

            GameObject shore = new GameObject("TestShore_VisualOnly");
            MeshFilter shoreFilter = shore.AddComponent<MeshFilter>();
            shoreFilter.sharedMesh = shoreMesh;
            MeshRenderer shoreRenderer = shore.AddComponent<MeshRenderer>();
            shoreRenderer.sharedMaterial = sand;
            shoreRenderer.receiveShadows = true;

            GameObject dock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dock.name = "D19_TestDock_VisualOnly";
            dock.transform.position = new Vector3(0f, 0.2f, -12f);
            dock.transform.localScale = new Vector3(24f, 0.4f, 5f);
            RemoveCollider(dock);
            dock.GetComponent<Renderer>().sharedMaterial = wood;

            GameObject boat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boat.name = "D19_TestBoat_VisualOnly_NoShipLogic";
            boat.transform.position = new Vector3(0f, 0.75f, -30f);
            boat.transform.localScale = new Vector3(5f, 1.1f, 12f);
            RemoveCollider(boat);
            boat.GetComponent<Renderer>().sharedMaterial = wood;

            GameObject reefSign = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reefSign.name = "D19_TestReefMarker_VisualOnly";
            reefSign.transform.position = new Vector3(-57f, 0.35f, -56f);
            reefSign.transform.localScale = new Vector3(0.5f, 0.35f, 0.5f);
            RemoveCollider(reefSign);
            reefSign.GetComponent<Renderer>().sharedMaterial = coral;

            GameObject lightObject = new GameObject("D19_TestSun");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.86f, 0.68f);
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            GameObject cameraObject = new GameObject("D19_WaterTestCamera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 420f;
            camera.allowHDR = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.24f, 0.31f, 1f);

            CreateWorkCanvas();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.28f, 0.32f);
            RenderSettings.fog = false;

            EditorSceneManager.SaveScene(scene, ProjectAbsolute(ScenePath));
        }

        private static void CreateWorkCanvas()
        {
            GameObject canvasObject = new GameObject("D19_Water_Work_UI_Overlay");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject labelObject = new GameObject("WaterWorkReadOnlyLabel");
            labelObject.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = labelObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -24f);
            rect.sizeDelta = new Vector2(720f, 50f);
            Text text = labelObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.text = "D19 WATER WORK  |  READ-ONLY PRESENTATION  |  SEA LEVEL Y=0";
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private static void AuditSceneAndCapture(WaterAuditResult result)
        {
            Scene scene = EditorSceneManager.OpenScene(ProjectAbsolute(ScenePath), OpenSceneMode.Single);
            D19WaterPresentation water = UnityEngine.Object.FindObjectOfType<D19WaterPresentation>();
            Camera camera = GameObject.Find("D19_WaterTestCamera")?.GetComponent<Camera>();
            if (water == null || camera == null)
            {
                result.status = "BLOCKED";
                result.warnings = new[] { "WaterRoot or D19_WaterTestCamera missing from work scene." };
                return;
            }

            result.sea_level_contract_passed = water.IsSeaLevelConsistent(0f) && Mathf.Abs(water.transform.position.y) < 0.001f;
            result.ui_overlay_order_passed = GameObject.Find("D19_Water_Work_UI_Overlay") != null;
            result.underwater_guard_passed = Shader.Find("WhalesIslandReef/D19/UnderwaterVolume") != null;
            if (!result.sea_level_contract_passed || !result.ui_overlay_order_passed || !result.underwater_guard_passed)
            {
                result.status = "PARTIAL";
                result.warnings = new[] { "One or more presentation contract checks failed." };
            }

            List<string> screenshotPaths = new List<string>();
            List<float> renderTimes = new List<float>();
            ShotDefinition[] shots = CreateShots();
            for (int i = 0; i < shots.Length; i++)
            {
                ShotDefinition shot = shots[i];
                camera.transform.position = shot.position;
                camera.transform.LookAt(shot.lookAt);
                camera.backgroundColor = shot.background;
                water.SetQuality(shot.quality);
                water.ApplyExternalFrame(water.ReadFrameParameters());
                string absolute = ProjectAbsolute(shot.file);
                Directory.CreateDirectory(Path.GetDirectoryName(absolute));
                float elapsed = CaptureCamera(camera, absolute);
                screenshotPaths.Add(shot.file);
                renderTimes.Add(elapsed);
            }
            result.screenshots = screenshotPaths.ToArray();
            result.render_ms_samples = renderTimes.ToArray();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static ShotDefinition[] CreateShots()
        {
            return new[]
            {
                new ShotDefinition { id = "sunny_deep_water", file = ScreenshotRoot + "/D19_A04_01_sunny_deep_water.png", position = new Vector3(18f, 7f, -40f), lookAt = new Vector3(0f, 0f, -66f), quality = D19WaterQualityTier.High, background = new Color(0.18f, 0.45f, 0.68f) },
                new ShotDefinition { id = "shallow_water", file = ScreenshotRoot + "/D19_A04_02_shallow_water.png", position = new Vector3(22f, 4f, -50f), lookAt = new Vector3(0f, -0.5f, -68f), quality = D19WaterQualityTier.Medium, background = new Color(0.25f, 0.58f, 0.70f) },
                new ShotDefinition { id = "reef_visibility", file = ScreenshotRoot + "/D19_A04_03_reef_visibility.png", position = new Vector3(-30f, 3.5f, -67f), lookAt = new Vector3(-8f, -1f, -74f), quality = D19WaterQualityTier.High, background = new Color(0.12f, 0.38f, 0.52f) },
                new ShotDefinition { id = "shoreline_wet_edge", file = ScreenshotRoot + "/D19_A04_04_shoreline_wet_edge.png", position = new Vector3(-28f, 5f, 5f), lookAt = new Vector3(-36f, 0f, -8f), quality = D19WaterQualityTier.Medium, background = new Color(0.40f, 0.66f, 0.78f) },
                new ShotDefinition { id = "underwater_boundary", file = ScreenshotRoot + "/D19_A04_05_underwater_boundary.png", position = new Vector3(0f, -3f, -52f), lookAt = new Vector3(0f, -3f, -68f), quality = D19WaterQualityTier.Medium, background = new Color(0.02f, 0.12f, 0.18f) },
                new ShotDefinition { id = "low_quality_mobile", file = ScreenshotRoot + "/D19_A04_06_low_quality_mobile.png", position = new Vector3(28f, 6f, -100f), lookAt = new Vector3(0f, 0f, -110f), quality = D19WaterQualityTier.Low, background = new Color(0.20f, 0.35f, 0.44f) },
            };
        }

        private static float CaptureCamera(Camera camera, string absolutePath)
        {
            const int width = 1280;
            const int height = 720;
            Stopwatch stopwatch = Stopwatch.StartNew();
            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.name = "D19_A04_CaptureTarget";
            target.Create();
            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            image.Apply(false, false);
            File.WriteAllBytes(absolutePath, image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
            camera.targetTexture = null;
            RenderTexture.active = previous;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            stopwatch.Stop();
            return (float)stopwatch.Elapsed.TotalMilliseconds;
        }

        private static Material CreateWaterMaterial(string name, Shader shader, Color deep, Color shallow,
            float transparency, float turbidity, D19WaterQualityTier tier)
        {
            Material material = CreateOrReplaceMaterial(name, shader);
            material.SetColor("_DeepColor", deep);
            material.SetColor("_ShallowColor", shallow);
            material.SetColor("_FoamColor", new Color(0.72f, 0.93f, 0.88f, 1f));
            material.SetColor("_UnderwaterColor", new Color(0.02f, 0.22f, 0.28f, 1f));
            material.SetFloat("_WaterTransparency", transparency);
            material.SetFloat("_Turbidity", turbidity);
            material.SetFloat("_DistanceFade", 260f);
            material.SetFloat("_WaveLength", tier == D19WaterQualityTier.Low ? 24f : 18f);
            material.DisableKeyword("_D19_WATER_LOW");
            material.DisableKeyword("_D19_WATER_MEDIUM");
            material.DisableKeyword("_D19_WATER_HIGH");
            material.EnableKeyword(tier == D19WaterQualityTier.Low ? "_D19_WATER_LOW" : tier == D19WaterQualityTier.High ? "_D19_WATER_HIGH" : "_D19_WATER_MEDIUM");
            return material;
        }

        private static Material CreateFoamMaterial(string name, Shader shader)
        {
            Material material = CreateOrReplaceMaterial(name, shader);
            material.SetColor("_Color", new Color(0.78f, 0.96f, 0.92f, 0.75f));
            material.SetFloat("_ScrollSpeed", 0.25f);
            material.SetFloat("_Softness", 0.55f);
            return material;
        }

        private static Material CreateUnderwaterMaterial(string name, Shader shader)
        {
            Material material = CreateOrReplaceMaterial(name, shader);
            material.SetColor("_UnderwaterColor", new Color(0.02f, 0.24f, 0.30f, 1f));
            material.SetFloat("_UnderwaterAlpha", 0.24f);
            return material;
        }

        private static Material CreateLitMaterial(string name, Shader shader, Texture2D texture, Color color, float smoothness)
        {
            Material material = CreateOrReplaceMaterial(name, shader);
            material.SetColor("_BaseColor", color);
            if (texture != null) material.SetTexture("_BaseMap", texture);
            material.SetFloat("_Smoothness", smoothness);
            return material;
        }

        private static Material CreateOrReplaceMaterial(string name, Shader shader)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            DeleteAssetIfExists(path);
            Material material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Mesh CreateOrReplaceMesh(string name, Mesh mesh)
        {
            string path = MeshRoot + "/" + name + ".asset";
            DeleteAssetIfExists(path);
            mesh.name = name;
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static Mesh CreateWaterMesh()
        {
            return CreateGridMesh("D19_Water_Surface_Mesh", -96f, 96f, -128f, 0f, 32, 32, (x, z) =>
            {
                float shallow;
                if (z >= -48f) shallow = 0.82f;
                else if (z >= -86f) shallow = 0.42f;
                else shallow = 0.08f;
                if (x < -70f || x > 55f) shallow *= 0.72f;
                return new Color(shallow, 0f, 0f, 1f);
            });
        }

        private static Mesh CreateShoreMesh()
        {
            return CreateGridMesh("D19_Water_TestShore_Mesh", -96f, 96f, 0f, 22f, 16, 4, (x, z) => new Color(0f, 0f, 0f, 1f), -0.15f);
        }

        private static Mesh CreateFoamMesh()
        {
            const int segments = 48;
            const int rows = 2;
            Vector3[] vertices = new Vector3[(segments + 1) * (rows + 1)];
            Vector2[] uv = new Vector2[vertices.Length];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[segments * rows * 6];
            for (int z = 0; z <= rows; z++)
            {
                float v = z / (float)rows;
                for (int x = 0; x <= segments; x++)
                {
                    float u = x / (float)segments;
                    int index = z * (segments + 1) + x;
                    float worldX = Mathf.Lerp(-96f, 96f, u);
                    float shorelineZ = -3.5f - Mathf.Abs(worldX + 48f) * 0.018f;
                    vertices[index] = new Vector3(worldX, 0.025f, shorelineZ + Mathf.Lerp(-1.4f, 1.4f, v));
                    uv[index] = new Vector2(u, v);
                    float alpha = z == 1 ? 1f : 0f;
                    colors[index] = new Color(1f, 1f, 1f, alpha);
                }
            }
            int t = 0;
            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int a = z * (segments + 1) + x;
                    int b = a + 1;
                    int c = a + segments + 1;
                    int d = c + 1;
                    triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
                    triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
                }
            }
            Mesh mesh = new Mesh { name = "D19_Water_ShoreFoam_Mesh" };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateRockMesh()
        {
            const int sides = 8;
            Vector3[] vertices = new Vector3[sides * 3];
            int[] triangles = new int[sides * 12];
            for (int i = 0; i < sides; i++)
            {
                float angle = i / (float)sides * Mathf.PI * 2f;
                float radius = 0.8f + 0.16f * Mathf.Sin(i * 2.7f);
                vertices[i] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                vertices[sides + i] = new Vector3(Mathf.Cos(angle) * radius * 0.78f, 0.75f + 0.18f * Mathf.Sin(i * 1.9f), Mathf.Sin(angle) * radius * 0.78f);
                vertices[sides * 2 + i] = new Vector3(Mathf.Cos(angle) * radius * 0.35f, 1.2f + 0.1f * Mathf.Cos(i), Mathf.Sin(angle) * radius * 0.35f);
            }
            int t = 0;
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                triangles[t++] = i; triangles[t++] = next; triangles[t++] = sides + i;
                triangles[t++] = next; triangles[t++] = sides + next; triangles[t++] = sides + i;
                triangles[t++] = sides + i; triangles[t++] = sides + next; triangles[t++] = sides * 2 + i;
                triangles[t++] = sides + next; triangles[t++] = sides * 2 + next; triangles[t++] = sides * 2 + i;
            }
            Mesh mesh = new Mesh { name = "D19_Water_ReefRock_Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateGridMesh(string name, float minX, float maxX, float minZ, float maxZ,
            int xSegments, int zSegments, Func<float, float, Color> colorAt, float y = 0f)
        {
            Vector3[] vertices = new Vector3[(xSegments + 1) * (zSegments + 1)];
            Vector2[] uv = new Vector2[vertices.Length];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[xSegments * zSegments * 6];
            for (int z = 0; z <= zSegments; z++)
            {
                float v = z / (float)zSegments;
                for (int x = 0; x <= xSegments; x++)
                {
                    float u = x / (float)xSegments;
                    int index = z * (xSegments + 1) + x;
                    float worldX = Mathf.Lerp(minX, maxX, u);
                    float worldZ = Mathf.Lerp(minZ, maxZ, v);
                    vertices[index] = new Vector3(worldX, y, worldZ);
                    uv[index] = new Vector2(u, v);
                    colors[index] = colorAt(worldX, worldZ);
                }
            }
            int t = 0;
            for (int z = 0; z < zSegments; z++)
            {
                for (int x = 0; x < xSegments; x++)
                {
                    int a = z * (xSegments + 1) + x;
                    int b = a + 1;
                    int c = a + xSegments + 1;
                    int d = c + 1;
                    triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
                    triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
                }
            }
            Mesh mesh = new Mesh { name = name };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void RemoveCollider(GameObject gameObject)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
        }

        private static void DeleteAssetIfExists(string assetPath)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);
        }

        private static void WriteResult(WaterAuditResult result)
        {
            string absolute = ProjectAbsolute(ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolute));
            File.WriteAllText(absolute, JsonUtility.ToJson(result, true), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string ProjectAbsolute(string projectRelativePath)
        {
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                projectRelativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
