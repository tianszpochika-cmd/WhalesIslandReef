using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhalesIslandReef.Editor.D20A01R01
{
    /// <summary>
    /// Builds the D20-A01-R01 candidate-only PlayerVisual prefab. The prefab
    /// owns no gameplay component; D17 owns the physical root and D20-A02 owns
    /// the Animator/action layer.
    /// </summary>
    public static class D20A01PlayerVisualBuilder
    {
        private const string SourcePath =
            "Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/" +
            "QuaterniusUniversalBaseCharacters/Models/Base/Superhero_Female_FullBody.fbx";
        private const string PrefabPath =
            "Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_HumanoidFemale_R01.prefab";
        private const string AvatarAssetPath =
            "Assets/Art/Models/Characters/D20/Player/D20_PlayerHumanoidAvatar_R01.asset";
        private const string MaterialRoot = "Assets/Art/Models/Characters/D20/Player/Materials";
        private const string ResultRoot = "tools/results/D20/D20-A01-R01";

        [MenuItem("Tools/WhalesIslandReef/D20-A01-R01/Build Player Visual Work Prefab")]
        public static void BuildFromMenu()
        {
            Build();
        }

        [MenuItem("Tools/WhalesIslandReef/D20-A01-R01/Audit Player Visual Work Prefab")]
        public static void AuditFromMenu()
        {
            var report = Audit();
            Debug.Log("[D20-A01-R01] Player visual audit: " + report.status);
        }

        [MenuItem("Tools/WhalesIslandReef/D20-A01-R01/Audit Source Import")]
        public static void AuditSourceFromMenu()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var result = new SourceAuditReport
            {
                task = "D20-A01-R01",
                source_path = SourcePath,
                assets = new List<string>(),
                avatars = new List<AvatarAudit>()
            };
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(SourcePath))
            {
                if (asset == null) continue;
                result.assets.Add(asset.GetType().FullName + ":" + asset.name);
                var avatar = asset as Avatar;
                if (avatar != null)
                {
                    result.avatars.Add(new AvatarAudit
                    {
                        name = avatar.name,
                        is_valid = avatar.isValid,
                        is_human = avatar.isHuman
                    });
                }
            }
            var importer = AssetImporter.GetAtPath(SourcePath) as ModelImporter;
            if (importer != null)
            {
                result.animation_type = importer.animationType.ToString();
                result.avatar_setup = importer.avatarSetup.ToString();
                result.import_animation = importer.importAnimation;
            }
            WriteSourceAudit(result);
            Debug.Log("[D20-A01-R01] Source import audit written; avatars=" + result.avatars.Count);
        }

        [MenuItem("Tools/WhalesIslandReef/D20-A01-R01/Capture Player Visual Previews")]
        public static void CapturePreviewsFromMenu()
        {
            CapturePreviews();
        }

        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePath);
            if (source == null)
                throw new InvalidOperationException("D20-A01-R01 source FBX is missing: " + SourcePath);

            EnsureFolder("Assets/Art/Models", "Characters");
            EnsureFolder("Assets/Art/Models/Characters", "D20");
            EnsureFolder("Assets/Art/Models/Characters/D20", "Player");
            EnsureFolder(MaterialRoot, null);
            EnsureFolder("Assets/Prefabs", "Characters");
            EnsureFolder("Assets/Prefabs/Characters", "D20");
            EnsureFolder("Assets/Prefabs/Characters/D20", "PlayerVisuals");

            var neutral = CreateMaterial("D20_PlayerSkin_Neutral", new Color(0.56f, 0.30f, 0.18f, 1f));
            var warm = CreateMaterial("D20_PlayerSkin_Warm", new Color(0.72f, 0.42f, 0.25f, 1f));
            var brownHair = CreateMaterial("D20_PlayerHair_Brown", new Color(0.16f, 0.07f, 0.035f, 1f));
            var blackHair = CreateMaterial("D20_PlayerHair_Black", new Color(0.025f, 0.02f, 0.018f, 1f));
            AssetDatabase.SaveAssets();

            var root = new GameObject("D20_PlayerVisual_HumanoidFemale_R01");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            var visual = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (visual == null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                throw new InvalidOperationException("Could not instantiate source FBX as a Prefab instance.");
            }

            visual.name = "Visual_HumanoidFemale_Source";
            visual.transform.SetParent(root.transform, false);
            var generatedAvatar = CreateOrReplaceAvatarAsset(visual);
            if (generatedAvatar == null || !generatedAvatar.isValid || !generatedAvatar.isHuman)
            {
                UnityEngine.Object.DestroyImmediate(root);
                throw new InvalidOperationException("D20-A01-R01 could not build a valid Human Avatar from the admitted skeleton.");
            }
            NormalizeToD20Height(visual, 1.8f);
            BindCandidateMaterials(visual, neutral);

            CreateAnchor(root.transform, "D20Socket_Head", new Vector3(0f, 1.61f, 0f));
            CreateAnchor(root.transform, "D20Socket_Hand_L", new Vector3(-0.27f, 1.08f, 0.02f));
            CreateAnchor(root.transform, "D20Socket_Hand_R", new Vector3(0.27f, 1.08f, 0.02f));
            CreateAnchor(root.transform, "D20Socket_Foot_L", new Vector3(-0.12f, 0.04f, 0f));
            CreateAnchor(root.transform, "D20Socket_Foot_R", new Vector3(0.12f, 0.04f, 0f));
            CreateAnchor(root.transform, "D20Socket_Carry", new Vector3(0f, 0.95f, 0.28f));
            CreateAnchor(root.transform, "D20Socket_Fish", new Vector3(0f, 0.92f, 0.34f));
            CreateAnchor(root.transform, "D20Socket_Boarding", new Vector3(0f, 0.86f, -0.18f));
            CreateAnchor(root.transform, "D20Socket_Disembark", new Vector3(0f, 0.04f, -0.18f));
            CreateAnchor(root.transform, "D20Socket_Helm", new Vector3(0f, 1.03f, 0.30f));

            var lod = root.AddComponent<LODGroup>();
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            lod.SetLODs(new[]
            {
                new LOD(0.55f, renderers),
                new LOD(0.12f, renderers),
                new LOD(0.025f, Array.Empty<Renderer>())
            });
            lod.RecalculateBounds();

            bool saved;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out saved);
            UnityEngine.Object.DestroyImmediate(root);
            if (!saved || prefab == null)
                throw new InvalidOperationException("Could not save D20-A01-R01 prefab: " + PrefabPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var audit = Audit();
            if (audit.status != "PASS")
                throw new InvalidOperationException("D20-A01-R01 audit failed after build: " + audit.status);

            Debug.Log("[D20-A01-R01] Player visual work prefab built: " + PrefabPath);
        }

        public static AuditReport Audit()
        {
            var report = new AuditReport
            {
                task = "D20-A01-R01",
                source_path = SourcePath,
                prefab_path = PrefabPath,
                status = "BLOCKED",
                errors = new List<string>(),
                warnings = new List<string>(),
                anchors = new List<string>()
            };

            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(AvatarAssetPath);
            report.avatar_present = avatar != null;
            report.avatar_valid = avatar != null && avatar.isValid;
            report.avatar_is_human = avatar != null && avatar.isHuman;
            if (!report.avatar_present || !report.avatar_valid || !report.avatar_is_human)
                report.errors.Add("derived_human_avatar_invalid");

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            if (root == null)
            {
                report.errors.Add("prefab_missing_or_unloadable");
                WriteReport(report);
                return report;
            }

            try
            {
                report.root_name = root.name;
                report.root_scale = ToArray(root.transform.localScale);
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                report.renderer_count = renderers.Length;
                report.material_count = CountMaterials(renderers);
                report.monobehaviour_count = root.GetComponentsInChildren<MonoBehaviour>(true).Length;
                report.animator_count = root.GetComponentsInChildren<Animator>(true).Length;
                report.character_controller_count = root.GetComponentsInChildren<CharacterController>(true).Length;
                report.lod_group_count = root.GetComponentsInChildren<LODGroup>(true).Length;

                var bounds = new Bounds(Vector3.zero, Vector3.zero);
                bool hasBounds = false;
                foreach (var renderer in renderers)
                {
                    if (!hasBounds)
                    {
                        bounds = renderer.bounds;
                        hasBounds = true;
                    }
                    else bounds.Encapsulate(renderer.bounds);
                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material == null) report.errors.Add("null_material");
                    }
                }

                report.world_bounds_center = hasBounds ? ToArray(bounds.center) : new[] { 0f, 0f, 0f };
                report.world_bounds_size = hasBounds ? ToArray(bounds.size) : new[] { 0f, 0f, 0f };
                report.world_height_m = hasBounds ? bounds.size.y : 0f;

                var transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var transform in transforms)
                {
                    if (transform.name.StartsWith("D20Socket_", StringComparison.Ordinal))
                        report.anchors.Add(transform.name);
                }

                if (report.renderer_count == 0) report.errors.Add("renderer_missing");
                if (report.monobehaviour_count != 0) report.errors.Add("gameplay_monobehaviour_in_visual_prefab");
                if (report.animator_count != 0) report.errors.Add("animator_must_be_owned_by_D20_A02");
                if (report.character_controller_count != 0) report.errors.Add("character_controller_must_be_owned_by_D17");
                if (report.root_scale[0] != 1f || report.root_scale[1] != 1f || report.root_scale[2] != 1f)
                    report.errors.Add("root_scale_not_one");
                if (Mathf.Abs(report.world_height_m - 1.8f) > 0.03f)
                    report.errors.Add("visual_height_out_of_D20_tolerance");
                if (report.anchors.Count < 10) report.errors.Add("required_socket_count_below_10");
                if (report.lod_group_count != 1) report.errors.Add("lod_group_missing");

                if (report.avatar_present && report.avatar_valid && report.avatar_is_human && report.errors.Count == 0)
                    report.status = "PASS";
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            report.warnings.Add("only_one_independent_humanoid_body_direction_is_admitted");
            report.warnings.Add("six_independent_hair_meshes_and_eight_era_outfit_combinations_are_not_proven");
            report.warnings.Add("formal_runtime_consumer_and_LocalDevelopmentReady_wait_for_D18-S03_and_D20-S02-R01");
            report.warnings.Add("main_project_source_import_is_Generic_NoAvatar; derived Avatar asset is built from the same admitted skeleton without changing candidate metadata");
            WriteReport(report);
            return report;
        }

        public static void CapturePreviews()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) throw new InvalidOperationException("Preview prefab is missing: " + PrefabPath);
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var previewRoot = Path.GetFullPath(Path.Combine(projectRoot, ResultRoot, "previews"));
            Directory.CreateDirectory(previewRoot);

            var views = new[]
            {
                new PreviewView { id = "front", position = new Vector3(0f, 0.95f, -3.2f) },
                new PreviewView { id = "back", position = new Vector3(0f, 0.95f, 3.2f) },
                new PreviewView { id = "three_quarter_tpose", position = new Vector3(2.6f, 1.25f, -2.6f) }
            };
            var manifest = new PreviewManifest
            {
                task = "D20-A01-R01",
                width = 512,
                height = 768,
                files = new List<string>()
            };

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) throw new InvalidOperationException("Could not instantiate preview prefab.");
            var cameraObject = new GameObject("D20-A01-R01-PreviewCamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.11f, 0.15f, 1f);
            camera.fieldOfView = 35f;
            var target = new Vector3(0f, 0.9f, 0f);
            var renderTexture = new RenderTexture(512, 768, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            camera.targetTexture = renderTexture;

            try
            {
                foreach (var view in views)
                {
                    camera.transform.position = view.position;
                    camera.transform.LookAt(target);
                    camera.Render();
                    RenderTexture.active = renderTexture;
                    var texture = new Texture2D(512, 768, TextureFormat.RGBA32, false);
                    texture.ReadPixels(new Rect(0, 0, 512, 768), 0, 0);
                    texture.Apply();
                    var file = "D20_A01_R01_PREVIEW_" + view.id + ".png";
                    File.WriteAllBytes(Path.Combine(previewRoot, file), texture.EncodeToPNG());
                    manifest.files.Add("game/tools/results/D20/D20-A01-R01/previews/" + file);
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
            finally
            {
                RenderTexture.active = null;
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(instance);
            }

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(previewRoot), "D20_A01_R01_PREVIEWS.json"), JsonUtility.ToJson(manifest, true));
            Debug.Log("[D20-A01-R01] Preview captures written: " + manifest.files.Count);
        }

        private static void NormalizeToD20Height(GameObject visual, float targetHeight)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Source visual has no Renderer.");
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            var sourceHeight = bounds.size.y;
            if (sourceHeight <= 0.001f) throw new InvalidOperationException("Source visual has invalid height.");
            var scale = targetHeight / sourceHeight;
            visual.transform.localScale = Vector3.one * scale;
            visual.transform.localPosition = new Vector3(0f, -bounds.min.y * scale, 0f);
        }

        private static Avatar CreateOrReplaceAvatarAsset(GameObject visual)
        {
            var description = BuildHumanDescription(visual);
            var avatar = AvatarBuilder.BuildHumanAvatar(visual, description);
            if (avatar == null) return null;
            if (AssetDatabase.LoadAssetAtPath<Avatar>(AvatarAssetPath) != null)
                AssetDatabase.DeleteAsset(AvatarAssetPath);
            AssetDatabase.CreateAsset(avatar, AvatarAssetPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<Avatar>(AvatarAssetPath);
        }

        private static HumanDescription BuildHumanDescription(GameObject visual)
        {
            var human = new List<HumanBone>();
            AddHumanBone(human, visual, "Hips", "pelvis");
            AddHumanBone(human, visual, "Spine", "spine_01");
            AddHumanBone(human, visual, "Chest", "spine_02");
            AddHumanBone(human, visual, "UpperChest", "spine_03");
            AddHumanBone(human, visual, "Neck", "neck_01");
            AddHumanBone(human, visual, "Head", "Head");
            AddHumanBone(human, visual, "LeftShoulder", "clavicle_l");
            AddHumanBone(human, visual, "LeftUpperArm", "upperarm_l");
            AddHumanBone(human, visual, "LeftLowerArm", "lowerarm_l");
            AddHumanBone(human, visual, "LeftHand", "hand_l");
            AddHumanBone(human, visual, "RightShoulder", "clavicle_r");
            AddHumanBone(human, visual, "RightUpperArm", "upperarm_r");
            AddHumanBone(human, visual, "RightLowerArm", "lowerarm_r");
            AddHumanBone(human, visual, "RightHand", "hand_r");
            AddHumanBone(human, visual, "LeftUpperLeg", "thigh_l");
            AddHumanBone(human, visual, "LeftLowerLeg", "calf_l");
            AddHumanBone(human, visual, "LeftFoot", "foot_l");
            AddHumanBone(human, visual, "RightUpperLeg", "thigh_r");
            AddHumanBone(human, visual, "RightLowerLeg", "calf_r");
            AddHumanBone(human, visual, "RightFoot", "foot_r");

            var skeleton = new List<SkeletonBone>();
            foreach (var transform in visual.GetComponentsInChildren<Transform>(true))
            {
                skeleton.Add(new SkeletonBone
                {
                    name = transform.name,
                    position = transform.localPosition,
                    rotation = transform.localRotation,
                    scale = transform.localScale
                });
            }

            return new HumanDescription
            {
                human = human.ToArray(),
                skeleton = skeleton.ToArray(),
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0f,
                hasTranslationDoF = false,
            };
        }

        private static void AddHumanBone(List<HumanBone> human, GameObject root, string humanName, string boneName)
        {
            if (FindTransform(root.transform, boneName) == null) return;
            human.Add(new HumanBone
            {
                humanName = humanName,
                boneName = boneName,
                limit = new HumanLimit { useDefaultValues = true }
            });
        }

        private static Transform FindTransform(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindTransform(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static void BindCandidateMaterials(GameObject visual, Material material)
        {
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                var slots = renderer.sharedMaterials;
                for (var i = 0; i < slots.Length; i++) slots[i] = material;
                renderer.sharedMaterials = slots;
            }
        }

        private static Material CreateMaterial(string name, Color color)
        {
            var path = MaterialRoot + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            return material;
        }

        private static Avatar FindHumanAvatar(string path)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                var avatar = asset as Avatar;
                if (avatar != null && avatar.isValid && avatar.isHuman) return avatar;
            }
            return null;
        }

        private static void CreateAnchor(Transform parent, string name, Vector3 localPosition)
        {
            var anchor = new GameObject(name);
            anchor.transform.SetParent(parent, false);
            anchor.transform.localPosition = localPosition;
            anchor.transform.localRotation = Quaternion.identity;
            anchor.transform.localScale = Vector3.one;
        }

        private static int CountMaterials(IEnumerable<Renderer> renderers)
        {
            var materials = new HashSet<int>();
            foreach (var renderer in renderers)
            foreach (var material in renderer.sharedMaterials)
                if (material != null) materials.Add(material.GetInstanceID());
            return materials.Count;
        }

        private static float[] ToArray(Vector3 value)
        {
            return new[] { value.x, value.y, value.z };
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (string.IsNullOrEmpty(child))
            {
                if (!AssetDatabase.IsValidFolder(parent))
                    throw new InvalidOperationException("Expected existing folder: " + parent);
                return;
            }
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static void WriteReport(AuditReport report)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var resultPath = Path.GetFullPath(Path.Combine(projectRoot, ResultRoot, "D20_A01_R01_EDITOR_AUDIT.json"));
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
        }

        private static void WriteSourceAudit(SourceAuditReport report)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var resultPath = Path.GetFullPath(Path.Combine(projectRoot, ResultRoot, "D20_A01_R01_SOURCE_IMPORT_AUDIT.json"));
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
        }

        [Serializable]
        public sealed class AuditReport
        {
            public string task;
            public string source_path;
            public string prefab_path;
            public string status;
            public string root_name;
            public bool avatar_present;
            public bool avatar_valid;
            public bool avatar_is_human;
            public int renderer_count;
            public int material_count;
            public int monobehaviour_count;
            public int animator_count;
            public int character_controller_count;
            public int lod_group_count;
            public float world_height_m;
            public float[] root_scale;
            public float[] world_bounds_center;
            public float[] world_bounds_size;
            public List<string> anchors;
            public List<string> errors;
            public List<string> warnings;
        }

        [Serializable]
        public sealed class SourceAuditReport
        {
            public string task;
            public string source_path;
            public string animation_type;
            public string avatar_setup;
            public bool import_animation;
            public List<string> assets;
            public List<AvatarAudit> avatars;
        }

        [Serializable]
        public sealed class AvatarAudit
        {
            public string name;
            public bool is_valid;
            public bool is_human;
        }

        [Serializable]
        private sealed class PreviewView
        {
            public string id;
            public Vector3 position;
        }

        [Serializable]
        private sealed class PreviewManifest
        {
            public string task;
            public int width;
            public int height;
            public List<string> files;
        }
    }
}
