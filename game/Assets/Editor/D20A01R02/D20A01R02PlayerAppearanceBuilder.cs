#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhalesIslandReef.Editor.D20A01R02
{
    /// <summary>
    /// Builds the D20-A01-R02 isolated Player appearance workbench. The
    /// builder consumes the D18-R03 evidence and creates only work Prefabs and
    /// a demonstration scene. It does not modify the formal Player generator,
    /// ResourceKey registry, InitialHarbor, or any gameplay ownership.
    /// </summary>
    public static class D20A01R02PlayerAppearanceBuilder
    {
        private const string CandidateRoot =
            "Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance";
        private const string UniversalRoot = CandidateRoot + "/QuaterniusUniversalBaseCharacters";
        private const string D18Root = "Assets/Prefabs/Characters/D18/R03/PlayerAppearance";
        private const string D18AvatarRoot = CandidateRoot + "/ProjectOwned/R03/Avatars";
        private const string OutputRoot = "Assets/Prefabs/Characters/D20/R02/PlayerAppearance";
        private const string GeneratedMeshRoot = OutputRoot + "/GeneratedMeshes";
        private const string GeneratedMaterialRoot = OutputRoot + "/GeneratedMaterials";
        private const string GeneratedHairMaterialPath = GeneratedMaterialRoot + "/D20_R02_Hair_Brown.mat";
        private const string GeneratedHairCapMeshPath = GeneratedMeshRoot + "/D20_R02_Hair_Cap.asset";
        private const string SceneRoot = "Assets/Scenes/D20/R02/Work";
        private const string DemoScenePath = SceneRoot + "/D20_A01_R02_PlayerAppearance_Work.unity";
        private const string WorkbenchPath = OutputRoot + "/D20_R02_PlayerAppearance_Workbench.prefab";
        private const string ResultRoot = "tools/results/D20/D20-A01-R02";
        private const string InterfaceResultPath = ResultRoot + "/D20_A01_R02_PLAYER_APPEARANCE_INTERFACE.json";

        private static readonly BodyDefinition[] Bodies =
        {
            new BodyDefinition(
                "player_visual_female_r02",
                UniversalRoot + "/Models/Base/Superhero_Female_FullBody.fbx",
                D18AvatarRoot + "/D18_R03_A01_PlayerHumanoidFemale.asset",
                OutputRoot + "/D20_R02_PlayerVisual_Female.prefab",
                1.8f),
            new BodyDefinition(
                "player_visual_male_r02",
                UniversalRoot + "/Models/Base/Superhero_Male_FullBody.fbx",
                D18AvatarRoot + "/D18_R03_A01_PlayerHumanoidMale.asset",
                OutputRoot + "/D20_R02_PlayerVisual_Male.prefab",
                1.8f),
        };

        private static readonly string[] HairSources =
        {
            UniversalRoot + "/Models/Hairstyles/Hair_Buns.fbx",
            UniversalRoot + "/Models/Hairstyles/Hair_Buzzed.fbx",
            UniversalRoot + "/Models/Hairstyles/Hair_BuzzedFemale.fbx",
            UniversalRoot + "/Models/Hairstyles/Hair_Long.fbx",
            UniversalRoot + "/Models/Hairstyles/Hair_SimpleParted.fbx",
            D18Root + "/D18_R03_A01_Hair_TwistPonytail.prefab",
        };

        private static readonly string[] OutfitPrefabs =
        {
            D18Root + "/D18_R03_A01_outfit_female_peasant_1675_local_r03.prefab",
            D18Root + "/D18_R03_A01_outfit_female_peasant_hood_1675_local_r03.prefab",
            D18Root + "/D18_R03_A01_outfit_female_ranger_1675_local_r03.prefab",
            D18Root + "/D18_R03_A01_outfit_female_ranger_pauldron_1675_local_r03.prefab",
            D18Root + "/D18_R03_A01_outfit_male_peasant_1675_local_r03.prefab",
            D18Root + "/D18_R03_A01_outfit_male_peasant_hood_1675_local_r03.prefab",
            D18Root + "/D18_R03_A01_outfit_male_ranger_1675_local_r03.prefab",
            D18Root + "/D18_R03_A01_outfit_male_ranger_pauldron_1675_local_r03.prefab",
        };

        [MenuItem("Tools/WhalesIslandReef/D20-A01-R02/Build Player Appearance Workbench")]
        public static void BuildFromMenu()
        {
            Build();
        }

        [MenuItem("Tools/WhalesIslandReef/D20-A01-R02/Audit Player Appearance Workbench")]
        public static void AuditFromMenu()
        {
            var report = Audit();
            Debug.Log("[D20-A01-R02] appearance interface status=" + report.status);
        }

        [MenuItem("Tools/WhalesIslandReef/D20-A01-R02/Build Isolated Player Demo")]
        public static void BuildDemoFromMenu()
        {
            BuildDemoScene();
        }

        [MenuItem("Tools/WhalesIslandReef/D20-A01-R02/Build Windows Demo Player")]
        public static void BuildPlayerFromMenu()
        {
            BuildDemoScene();
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var output = Path.Combine(projectRoot, ResultRoot, "D20_A01_R02_PlayerDemo.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { DemoScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging,
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("D20-A01-R02 demo Player build failed: " + report.summary.result);
            WriteBuildResult(output, report.summary.totalSize, report.summary.result.ToString());
        }

        [MenuItem("Tools/WhalesIslandReef/D20-A01-R02/Diagnose Demo Bounds")]
        public static void DiagnoseDemoBoundsFromMenu()
        {
            var scene = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
            var root = GameObject.Find("PlayerDemo_Female_Humanoid");
            var camera = GameObject.Find("D20_R02_DemoCamera");
            var renderers = root == null ? Array.Empty<Renderer>() : root.GetComponentsInChildren<Renderer>(true);
            var bounds = renderers.Length == 0 ? new Bounds(Vector3.zero, Vector3.zero) : renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            var lines = new List<string>
            {
                "{",
                "  \"scene\": \"" + DemoScenePath + "\",",
                "  \"root_position\": " + Vector3Json(root == null ? Vector3.zero : root.transform.position) + ",",
                "  \"root_scale\": " + Vector3Json(root == null ? Vector3.one : root.transform.lossyScale) + ",",
                "  \"renderer_count\": " + renderers.Length + ",",
                "  \"combined_bounds_center\": " + Vector3Json(bounds.center) + ",",
                "  \"combined_bounds_size\": " + Vector3Json(bounds.size) + ",",
                "  \"camera_position\": " + Vector3Json(camera == null ? Vector3.zero : camera.transform.position) + ",",
                "  \"camera_forward\": " + Vector3Json(camera == null ? Vector3.forward : camera.transform.forward),
                "  ,\"renderer_bounds\": [",
            };
            for (var i = 0; i < renderers.Length; i++)
            {
                var suffix = i == renderers.Length - 1 ? "" : ",";
                lines.Add("    {\"name\":\"" + renderers[i].name.Replace("\"", "'") + "\",\"center\":" + Vector3Json(renderers[i].bounds.center) + ",\"size\":" + Vector3Json(renderers[i].bounds.size) + "}" + suffix);
            }
            lines.Add("  ]");
            lines.Add("}");
            foreach (var hairSource in HairSources)
            {
                var source = AssetDatabase.LoadAssetAtPath<GameObject>(hairSource);
                var sourceRenderer = source == null ? null : source.GetComponentInChildren<SkinnedMeshRenderer>(true);
                if (sourceRenderer != null)
                    Debug.Log("[D20-A01-R02] source_hair=" + hairSource +
                              " transform=" + sourceRenderer.transform.localPosition +
                              " mesh_bounds=" + sourceRenderer.sharedMesh.bounds +
                              " mesh_vertices=" + sourceRenderer.sharedMesh.vertexCount);
            }
            var absolute = Path.Combine(ToAbsoluteProjectPath(ResultRoot), "D20_A01_R02_DEMO_BOUNDS.json");
            Directory.CreateDirectory(Path.GetDirectoryName(absolute));
            File.WriteAllLines(absolute, lines);
            Debug.Log("[D20-A01-R02] demo bounds written: " + absolute);
            EditorSceneManager.CloseScene(scene, true);
        }

        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureFolderPath(OutputRoot);
            EnsureFolderPath(GeneratedMeshRoot);
            EnsureFolderPath(GeneratedMaterialRoot);
            EnsureFolderPath(SceneRoot);
            BuildGeneratedHairCapMesh();

            var bodyPrefabs = Bodies.Select(BuildBodyPrefab).ToArray();
            var hairPrefabs = HairSources.Select(BuildHairPrefab).ToArray();
            var workbench = BuildWorkbench(bodyPrefabs, hairPrefabs);
            BuildDemoScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var report = Audit();
            WriteInterface(report, bodyPrefabs, hairPrefabs, workbench);
            if (report.status != "PASS")
                throw new InvalidOperationException("D20-A01-R02 appearance audit failed: " + report.status);
            Debug.Log("[D20-A01-R02] Player appearance workbench built: " + WorkbenchPath);
        }

        public static AuditReport Audit()
        {
            var report = new AuditReport
            {
                task = "D20-A01-R02",
                status = "BLOCKED",
                body_prefabs = new List<BodyAudit>(),
                hair_prefabs = new List<ModuleAudit>(),
                outfit_prefabs = new List<ModuleAudit>(),
                errors = new List<string>(),
                warnings = new List<string>
                {
                    "This is an isolated workbench; no formal Player generator or scene consumer is changed.",
                    "1675 clothing suitability remains provisional and requires the historical review boundary.",
                    "LocalDevelopmentReady and ReleaseApproved are never written by this builder."
                }
            };

            foreach (var body in Bodies) AuditBody(report, body);
            for (var index = 0; index < HairSources.Length; index++)
                AuditModule(report.hair_prefabs, HairPrefabPath(index), HairSources[index], "hair");
            foreach (var outfit in OutfitPrefabs)
                AuditModule(report.outfit_prefabs, outfit, outfit, "outfit");

            report.workbench_exists = AssetDatabase.LoadAssetAtPath<GameObject>(WorkbenchPath) != null;
            report.demo_scene_exists = File.Exists(ToAbsoluteProjectPath(DemoScenePath));
            if (!report.workbench_exists) report.errors.Add("workbench_prefab_missing");
            if (!report.demo_scene_exists) report.errors.Add("demo_scene_missing");
            report.status = report.body_prefabs.Count == 2 &&
                            report.body_prefabs.All(item => item.avatar_valid && item.avatar_is_human && item.socket_count >= 8 && item.lod_present) &&
                            report.hair_prefabs.Count == 6 && report.hair_prefabs.All(item => item.valid) &&
                            report.outfit_prefabs.Count == 8 && report.outfit_prefabs.All(item => item.valid) &&
                            report.workbench_exists && report.demo_scene_exists && report.errors.Count == 0
                ? "PASS"
                : "PARTIAL";
            return report;
        }

        private static string BuildBodyPrefab(BodyDefinition definition)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(definition.source);
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(definition.avatar);
            if (source == null || avatar == null || !avatar.isValid || !avatar.isHuman)
                throw new InvalidOperationException("D20-A01-R02 body source/avatar missing: " + definition.id);

            var root = new GameObject("D20_R02_" + definition.id);
            var visual = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (visual == null) throw new InvalidOperationException("Could not instantiate body source: " + definition.source);
            visual.name = "Visual_" + definition.id;
            visual.transform.SetParent(root.transform, false);
            NormalizeToHeight(visual, definition.target_height);

            var animator = root.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            CreateSocket(root.transform, "D20Socket_Head", new Vector3(0f, 1.62f, 0f));
            CreateSocket(root.transform, "D20Socket_Hand_L", new Vector3(-0.28f, 1.08f, 0.02f));
            CreateSocket(root.transform, "D20Socket_Hand_R", new Vector3(0.28f, 1.08f, 0.02f));
            CreateSocket(root.transform, "D20Socket_Foot_L", new Vector3(-0.12f, 0.03f, 0f));
            CreateSocket(root.transform, "D20Socket_Foot_R", new Vector3(0.12f, 0.03f, 0f));
            CreateSocket(root.transform, "D20Socket_Carry", new Vector3(0f, 0.95f, 0.28f));
            CreateSocket(root.transform, "D20Socket_Fish", new Vector3(0f, 0.92f, 0.34f));
            CreateSocket(root.transform, "D20Socket_Boarding", new Vector3(0f, 0.86f, -0.18f));
            CreateSocket(root.transform, "D20Socket_Helm", new Vector3(0f, 1.03f, 0.30f));

            var lod = root.AddComponent<LODGroup>();
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            lod.SetLODs(new[]
            {
                new LOD(0.55f, renderers),
                new LOD(0.01f, renderers),
                new LOD(0.001f, Array.Empty<Renderer>())
            });
            lod.RecalculateBounds();
            SavePrefab(root, definition.prefab);
            return definition.prefab;
        }

        private static string BuildHairPrefab(string sourcePath, int index)
        {
            var path = HairPrefabPath(index);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null) throw new InvalidOperationException("D20-A01-R02 hair source missing: " + sourcePath);
            var root = new GameObject("D20_R02_Hair_" + index.ToString("00"));
            var visual = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (visual == null) throw new InvalidOperationException("Could not instantiate hair source: " + sourcePath);
            visual.name = "HairGeometry_" + index.ToString("00");
            visual.transform.SetParent(root.transform, false);
            ConvertSkinnedHairToStatic(visual, index);
            // The sixth module is itself a D18 work Prefab with an LODGroup.
            // Flatten the nested LOD component so the D20 module owns exactly
            // one LOD decision and Unity does not register the same Renderer
            // with two competing groups.
            foreach (var nestedLod in visual.GetComponentsInChildren<LODGroup>(true))
                UnityEngine.Object.DestroyImmediate(nestedLod);
            if (sourcePath.EndsWith("Hair_TwistPonytail.prefab", StringComparison.OrdinalIgnoreCase))
                visual.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            var lod = root.AddComponent<LODGroup>();
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            lod.SetLODs(new[]
            {
                // Hair modules are small; the previous 0.45 threshold
                // culled the preview hair at a normal 4.8 m showcase
                // distance. Keep a visible close/mid LOD and cull only at
                // genuinely distant view ranges.
                new LOD(0.12f, renderers),
                new LOD(0.025f, Array.Empty<Renderer>())
            });
            lod.RecalculateBounds();
            SavePrefab(root, path);
            return path;
        }

        private static void ConvertSkinnedHairToStatic(GameObject visual, int index)
        {
            var skinnedRenderers = visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (var rendererIndex = 0; rendererIndex < skinnedRenderers.Length; rendererIndex++)
            {
                var skinned = skinnedRenderers[rendererIndex];
                if (skinned.sharedMesh == null) continue;

                // These imported hair files already contain their neutral
                // head-space vertex positions. Copying the source mesh keeps
                // the authored silhouette intact; the imported skin
                // component itself is not a valid runtime dependency for a
                // standalone hair module.
                var baked = UnityEngine.Object.Instantiate(skinned.sharedMesh);
                baked.name = "D20_R02_Hair_" + (index + 1).ToString("00") + "_BakedMesh";
                var meshPath = GeneratedMeshRoot + "/D20_R02_Hair_" + (index + 1).ToString("00") + "_BakedMesh.asset";
                if (AssetDatabase.LoadAssetAtPath<Mesh>(meshPath) != null)
                    AssetDatabase.DeleteAsset(meshPath);
                AssetDatabase.CreateAsset(baked, meshPath);
                AssetDatabase.ImportAsset(meshPath, ImportAssetOptions.ForceSynchronousImport);

                var staticObject = new GameObject(skinned.name + "_StaticMesh");
                staticObject.transform.SetParent(skinned.transform.parent, false);
                staticObject.transform.localPosition = skinned.transform.localPosition;
                staticObject.transform.localRotation = skinned.transform.localRotation;
                staticObject.transform.localScale = skinned.transform.localScale;
                var filter = staticObject.AddComponent<MeshFilter>();
                filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                var meshRenderer = staticObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = GetGeneratedHairMaterial();
                meshRenderer.shadowCastingMode = skinned.shadowCastingMode;
                meshRenderer.receiveShadows = skinned.receiveShadows;
                UnityEngine.Object.DestroyImmediate(skinned);
            }
        }

        private static Material GetGeneratedHairMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(GeneratedHairMaterialPath);
            if (material != null)
            {
                var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (unlitShader != null) material.shader = unlitShader;
                if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
                material.color = new Color(0.09f, 0.025f, 0.01f, 1f);
                EditorUtility.SetDirty(material);
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                         Shader.Find("Universal Render Pipeline/Lit") ??
                         Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("No supported shader found for D20 R02 hair material.");
            material = new Material(shader)
            {
                name = "D20_R02_Hair_Brown",
                color = new Color(0.09f, 0.025f, 0.01f, 1f)
            };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.22f);
            if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
            AssetDatabase.CreateAsset(material, GeneratedHairMaterialPath);
            AssetDatabase.ImportAsset(GeneratedHairMaterialPath, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<Material>(GeneratedHairMaterialPath);
        }

        private static Mesh BuildGeneratedHairCapMesh()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(GeneratedHairCapMeshPath);
            if (existing != null) AssetDatabase.DeleteAsset(GeneratedHairCapMeshPath);

            // Project-owned low-poly cap used only to make the isolated R02
            // proof frame readable. It is an authored mesh, not a gameplay
            // body substitute and is not attached to any formal generator.
            var mesh = new Mesh { name = "D20_R02_Hair_Cap" };
            const int sides = 16;
            var ringHeights = new[] { 0f, 0.12f, 0.25f, 0.32f };
            var ringRadiiX = new[] { 0.18f, 0.205f, 0.16f, 0.06f };
            var ringRadiiZ = new[] { 0.15f, 0.175f, 0.14f, 0.05f };
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var ring = 0; ring < ringHeights.Length; ring++)
            {
                for (var side = 0; side < sides; side++)
                {
                    var angle = (Mathf.PI * 2f * side) / sides;
                    vertices.Add(new Vector3(
                        Mathf.Cos(angle) * ringRadiiX[ring],
                        ringHeights[ring],
                        Mathf.Sin(angle) * ringRadiiZ[ring]));
                }
            }
            for (var ring = 0; ring < ringHeights.Length - 1; ring++)
            for (var side = 0; side < sides; side++)
            {
                var next = (side + 1) % sides;
                var a = ring * sides + side;
                var b = ring * sides + next;
                var c = (ring + 1) * sides + next;
                var d = (ring + 1) * sides + side;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(a); triangles.Add(d); triangles.Add(c);
            }
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, GeneratedHairCapMeshPath);
            AssetDatabase.ImportAsset(GeneratedHairCapMeshPath, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<Mesh>(GeneratedHairCapMeshPath);
        }

        private static string BuildWorkbench(string[] bodyPrefabs, string[] hairPrefabs)
        {
            var root = new GameObject("D20_R02_PlayerAppearance_Workbench");
            var female = InstantiateAsset(bodyPrefabs[0], root.transform, new Vector3(-1.1f, 0f, 0f));
            female.name = "PlayerVisual_Female_1m80";
            var male = InstantiateAsset(bodyPrefabs[1], root.transform, new Vector3(1.1f, 0f, 0f));
            male.name = "PlayerVisual_Male_1m80";
            var hairRoot = new GameObject("HairModuleLibrary");
            hairRoot.transform.SetParent(root.transform, false);
            for (var i = 0; i < hairPrefabs.Length; i++)
            {
                var instance = InstantiateAsset(hairPrefabs[i], hairRoot.transform, new Vector3(-4.2f + (i % 3) * 1.8f, 0f, 2.8f + (i / 3) * 1.6f));
                instance.name = "HairVariant_" + (i + 1).ToString("00");
            }
            var outfitRoot = new GameObject("OutfitCombinationLibrary");
            outfitRoot.transform.SetParent(root.transform, false);
            for (var i = 0; i < OutfitPrefabs.Length; i++)
            {
                var instance = InstantiateAsset(OutfitPrefabs[i], outfitRoot.transform, new Vector3(-5.3f + (i % 4) * 3.4f, 0f, -3.4f - (i / 4) * 2.4f));
                instance.name = "OutfitCombination_" + (i + 1).ToString("00");
            }
            SavePrefab(root, WorkbenchPath);
            return WorkbenchPath;
        }

        private static void BuildDemoScene()
        {
            var workbench = AssetDatabase.LoadAssetAtPath<GameObject>(WorkbenchPath);
            if (workbench == null) throw new InvalidOperationException("Workbench prefab must be built before the demo scene.");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var demoRoot = new GameObject("D20_A01_R02_PlayerAppearance_Demo");
            var preview = InstantiateAsset(Bodies[0].prefab, demoRoot.transform, Vector3.zero);
            preview.name = "PlayerDemo_Female_Humanoid";
            // Use the project-owned static ponytail direction in the Player
            // proof frame; the other five independent directions remain in
            // the workbench library.
            var hair = InstantiateAsset(HairPrefabPath(5), demoRoot.transform, Vector3.zero);
            hair.name = "PlayerDemo_Hair_TwistPonytail";
            // The source mesh is authored as a rear/side ponytail module. Keep
            // the proof frame readable from the front camera without changing
            // the reusable prefab's source transform.
            hair.transform.localPosition = new Vector3(0.10f, 0.90f, -0.16f);
            hair.transform.localScale = Vector3.one * 0.55f;
            foreach (var hairLod in hair.GetComponentsInChildren<LODGroup>(true))
                hairLod.enabled = false;
            if (AssetDatabase.LoadAssetAtPath<Mesh>(GeneratedHairCapMeshPath) == null)
            {
                EnsureFolderPath(GeneratedMeshRoot);
                BuildGeneratedHairCapMesh();
                AssetDatabase.SaveAssets();
            }
            var cap = new GameObject("PlayerDemo_Hair_Cap");
            cap.transform.SetParent(demoRoot.transform, false);
            cap.transform.localPosition = new Vector3(0f, 1.62f, 0.05f);
            cap.transform.localScale = new Vector3(0.70f, 0.70f, 0.60f);
            var capFilter = cap.AddComponent<MeshFilter>();
            capFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GeneratedHairCapMeshPath);
            var capRenderer = cap.AddComponent<MeshRenderer>();
            capRenderer.sharedMaterial = GetGeneratedHairMaterial();
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "D20_R02_DemoGround";
            ground.transform.position = new Vector3(0f, 0f, 0f);
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
            var lightObject = new GameObject("D20_R02_DemoKeyLight");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            var cameraObject = new GameObject("D20_R02_DemoCamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 1.15f, -4.8f);
            camera.transform.rotation = Quaternion.Euler(5f, 0f, 0f);
            camera.fieldOfView = 42f;
            camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.usePhysicalProperties = false;
            cameraObject.transform.LookAt(new Vector3(0f, 0.9f, 0f));
            cameraObject.AddComponent<WhalesIslandReef.D20R02.D20R02PlayerRuntimeProbe>();
            SceneManager.SetActiveScene(scene);
            EditorSceneManager.SaveScene(scene, DemoScenePath);
        }

        private static void AuditBody(AuditReport report, BodyDefinition definition)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(definition.prefab);
            var item = new BodyAudit { id = definition.id, prefab = definition.prefab, avatar_valid = false, avatar_is_human = false };
            if (root == null)
            {
                report.errors.Add("body_prefab_missing:" + definition.id);
                report.body_prefabs.Add(item);
                return;
            }
            var animator = root.GetComponentInChildren<Animator>(true);
            item.avatar_valid = animator != null && animator.avatar != null && animator.avatar.isValid;
            item.avatar_is_human = item.avatar_valid && animator.avatar.isHuman;
            item.socket_count = new[] { "D20Socket_Head", "D20Socket_Hand_L", "D20Socket_Hand_R", "D20Socket_Foot_L", "D20Socket_Foot_R", "D20Socket_Carry", "D20Socket_Fish", "D20Socket_Boarding", "D20Socket_Helm" }
                .Count(name => root.transform.Find(name) != null);
            item.lod_present = root.GetComponent<LODGroup>() != null;
            item.renderer_count = root.GetComponentsInChildren<Renderer>(true).Length;
            item.valid = item.avatar_valid && item.avatar_is_human && item.socket_count >= 8 && item.lod_present && item.renderer_count > 0;
            if (!item.valid) report.errors.Add("body_interface_invalid:" + definition.id);
            report.body_prefabs.Add(item);
        }

        private static void AuditModule(List<ModuleAudit> list, string prefabPath, string source, string kind)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var item = new ModuleAudit { prefab = prefabPath, source = source, kind = kind, valid = false };
            if (root == null)
            {
                list.Add(item);
                return;
            }
            item.renderer_count = root.GetComponentsInChildren<Renderer>(true).Length;
            item.mesh_count = root.GetComponentsInChildren<MeshFilter>(true).Count(filter => filter.sharedMesh != null) +
                              root.GetComponentsInChildren<SkinnedMeshRenderer>(true).Count(renderer => renderer.sharedMesh != null);
            item.lod_present = root.GetComponent<LODGroup>() != null;
            item.valid = item.renderer_count > 0 && item.mesh_count > 0 && item.lod_present && root.GetComponentsInChildren<MonoBehaviour>(true).Length == 0;
            list.Add(item);
        }

        private static void WriteInterface(AuditReport report, string[] bodyPrefabs, string[] hairPrefabs, string workbench)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var path = Path.Combine(projectRoot, InterfaceResultPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var result = new InterfaceResult
            {
                schema = "D20_A01_R02_PLAYER_APPEARANCE_INTERFACE_V1",
                task = "D20-A01-R02",
                status = report.status,
                generated_at = DateTime.UtcNow.ToString("o"),
                rig_interface_ready = report.status == "PASS",
                body_directions = 2,
                hair_variants = 6,
                outfit_combinations = 8,
                body_prefabs = bodyPrefabs,
                hair_prefabs = hairPrefabs,
                workbench_prefab = workbench,
                demo_scene = DemoScenePath,
                formal_runtime_consumers = 0,
                local_development_ready = false,
                release_approved = false,
                d17_compatibility = "1.80m visual height, foot sockets, no root motion on visual Animator; formal movement/landing/boarding remains D17 owner.",
                allowed_consumer = "D20-A02-R02 isolated action compatibility and later D20-S02-R01 formal integration after D19 gate.",
                forbidden_consumer = "InitialHarbor, formal generator, Registry, Save, Systems and ReleaseApproved are not modified."
            };
            File.WriteAllText(path, JsonUtility.ToJson(result, true) + "\n");
            var reportPath = Path.Combine(projectRoot, ResultRoot, "D20_A01_R02_EDITOR_AUDIT.json");
            File.WriteAllText(reportPath, JsonUtility.ToJson(report, true) + "\n");
        }

        private static void WriteBuildResult(string output, ulong size, string result)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var path = Path.Combine(projectRoot, ResultRoot, "D20_A01_R02_PLAYER_BUILD.json");
            File.WriteAllText(path, JsonUtility.ToJson(new PlayerBuildResult
            {
                task = "D20-A01-R02",
                result = result,
                output = output,
                total_size_bytes = size,
                scene = DemoScenePath,
                resolution_target = "1920x1080 demo evidence is captured separately; build is an isolated visual workbench, not formal gameplay integration."
            }, true) + "\n");
        }

        private static string HairPrefabPath(int index)
        {
            return OutputRoot + "/D20_R02_Hair_" + (index + 1).ToString("00") + ".prefab";
        }

        private static GameObject InstantiateAsset(string path, Transform parent, Vector3 localPosition)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) throw new InvalidOperationException("Missing workbench asset: " + path);
            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null) throw new InvalidOperationException("Could not instantiate workbench asset: " + path);
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            return instance;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            bool saved;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path, out saved);
            UnityEngine.Object.DestroyImmediate(root);
            if (!saved || prefab == null) throw new InvalidOperationException("Could not save prefab: " + path);
        }

        private static void NormalizeToHeight(GameObject visual, float targetHeight)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Body visual has no renderers.");
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            if (bounds.size.y <= 0.001f) throw new InvalidOperationException("Body visual has invalid height.");
            var scale = targetHeight / bounds.size.y;
            visual.transform.localScale = Vector3.one * scale;
            // Candidate FBX pivots are not guaranteed to be at the visual
            // center. Center X/Z as well as putting the feet on y=0 so the
            // isolated Player demo matches the D17 ground/pivot contract.
            visual.transform.localPosition = new Vector3(
                -bounds.center.x * scale,
                -bounds.min.y * scale,
                -bounds.center.z * scale);
        }

        private static void CreateSocket(Transform parent, string name, Vector3 localPosition)
        {
            var socket = new GameObject(name);
            socket.transform.SetParent(parent, false);
            socket.transform.localPosition = localPosition;
            socket.transform.localRotation = Quaternion.identity;
            socket.transform.localScale = Vector3.one;
        }

        private static void EnsureFolderPath(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }

        private static string ToAbsoluteProjectPath(string assetPath)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.Combine(projectRoot, assetPath);
        }

        private static string Vector3Json(Vector3 value)
        {
            return "{\"x\":" + value.x.ToString("R", System.Globalization.CultureInfo.InvariantCulture) +
                   ",\"y\":" + value.y.ToString("R", System.Globalization.CultureInfo.InvariantCulture) +
                   ",\"z\":" + value.z.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "}";
        }

        [Serializable]
        private sealed class BodyDefinition
        {
            public readonly string id;
            public readonly string source;
            public readonly string avatar;
            public readonly string prefab;
            public readonly float target_height;

            public BodyDefinition(string id, string source, string avatar, string prefab, float targetHeight)
            {
                this.id = id;
                this.source = source;
                this.avatar = avatar;
                this.prefab = prefab;
                target_height = targetHeight;
            }
        }

        [Serializable]
        public sealed class AuditReport
        {
            public string task;
            public string status;
            public bool workbench_exists;
            public bool demo_scene_exists;
            public List<BodyAudit> body_prefabs;
            public List<ModuleAudit> hair_prefabs;
            public List<ModuleAudit> outfit_prefabs;
            public List<string> errors;
            public List<string> warnings;
        }

        [Serializable]
        public sealed class BodyAudit
        {
            public string id;
            public string prefab;
            public bool valid;
            public bool avatar_valid;
            public bool avatar_is_human;
            public bool lod_present;
            public int socket_count;
            public int renderer_count;
        }

        [Serializable]
        public sealed class ModuleAudit
        {
            public string prefab;
            public string source;
            public string kind;
            public bool valid;
            public bool lod_present;
            public int renderer_count;
            public int mesh_count;
        }

        [Serializable]
        private sealed class InterfaceResult
        {
            public string schema;
            public string task;
            public string status;
            public string generated_at;
            public bool rig_interface_ready;
            public int body_directions;
            public int hair_variants;
            public int outfit_combinations;
            public string[] body_prefabs;
            public string[] hair_prefabs;
            public string workbench_prefab;
            public string demo_scene;
            public int formal_runtime_consumers;
            public bool local_development_ready;
            public bool release_approved;
            public string d17_compatibility;
            public string allowed_consumer;
            public string forbidden_consumer;
        }

        [Serializable]
        private sealed class PlayerBuildResult
        {
            public string task;
            public string result;
            public string output;
            public ulong total_size_bytes;
            public string scene;
            public string resolution_target;
        }
    }
}
#endif
