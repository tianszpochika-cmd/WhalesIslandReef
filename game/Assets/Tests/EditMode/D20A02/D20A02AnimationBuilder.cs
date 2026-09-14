using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace WhalesIslandReef.Editor.D20A02
{
    /// <summary>
    /// Creates only the D20-A02 work-layer clips/controller. It reads the
    /// A01 prefab and never edits that prefab, a scene or a gameplay component.
    /// </summary>
    public static class D20A02AnimationBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_HumanoidFemale_R01.prefab";
        private const string RootPath = "Assets/Animations/D20/Player";
        private const string ClipPath = RootPath + "/Clips";
        private const string ControllerPath = RootPath + "/D20_PlayerAnimation.controller";
        private const string ResultRoot = "tools/results/D20/D20-A02-R01";

        private static readonly string[] States =
        {
            "Idle", "Walk", "Run", "Jump", "Fall", "Land", "Interact", "Talk",
            "Carry", "Fish", "Board", "Disembark", "Helm", "Hit", "Recover"
        };

        [MenuItem("Tools/WhalesIslandReef/D20-A02-R01/Build Player Animation Work Layer")]
        public static void BuildFromMenu() => Build();

        public static void BuildFromCommandLine() => Build();

        [MenuItem("Tools/WhalesIslandReef/D20-A02-R01/Capture Animation Work Previews")]
        public static void CapturePreviewsFromMenu() => CapturePreviews();

        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (sourcePrefab == null) throw new InvalidOperationException("A01 work prefab missing: " + PrefabPath);

            EnsureFolder("Assets/Animations", "D20");
            EnsureFolder("Assets/Animations/D20", "Player");
            EnsureFolder(RootPath, "Clips");

            var sourceInstance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            if (sourceInstance == null) throw new InvalidOperationException("Could not instantiate A01 work prefab.");

            try
            {
                var animationRoot = FindByName(sourceInstance.transform, "Visual_HumanoidFemale_Source") ?? sourceInstance.transform;
                var clips = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
                foreach (var state in States)
                {
                    var clipPath = ClipPath + "/D20_Player_" + state + ".anim";
                    DeleteIfExists(clipPath);
                    var clip = CreateClip(state, animationRoot);
                    AssetDatabase.CreateAsset(clip, clipPath);
                    clips.Add(state, clip);
                }

                DeleteIfExists(ControllerPath);
                var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
                controller.name = "D20_PlayerAnimation_R01";
                controller.AddParameter("Speed01", AnimatorControllerParameterType.Float);
                controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
                controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
                controller.AddParameter("InteractionActive", AnimatorControllerParameterType.Bool);
                controller.AddParameter("ActionHash", AnimatorControllerParameterType.Int);
                controller.AddParameter("Paused", AnimatorControllerParameterType.Bool);

                var stateMachine = controller.layers[0].stateMachine;
                foreach (var stateName in States)
                {
                    var state = stateMachine.AddState(stateName);
                    state.motion = clips[stateName];
                    // These clips are generic transform-path clips. Every
                    // authored limb axis is written explicitly below, so the
                    // controller must not reset omitted model axes to Unity's
                    // default pose when a state changes.
                    state.writeDefaultValues = false;
                    if (stateName == "Idle") stateMachine.defaultState = state;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                WriteControllerAudit(controller, clips);
                Debug.Log("[D20-A02-R01] Controller and clips built: " + ControllerPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sourceInstance);
            }
        }

        private static AnimationClip CreateClip(string stateName, Transform root)
        {
            var clip = new AnimationClip { name = "D20_Player_" + stateName, frameRate = 30f, legacy = false };
            var loop = stateName != "Jump" && stateName != "Fall" && stateName != "Land" &&
                       stateName != "Board" && stateName != "Disembark" && stateName != "Hit" && stateName != "Recover";
            var rootPosition = root.localPosition;
            var amplitude = stateName == "Run" ? 0.045f : stateName == "Walk" ? 0.025f : 0.012f;
            if (stateName == "Jump" || stateName == "Fall") amplitude = 0.03f;
            if (stateName == "Land") amplitude = 0.018f;
            AddFloatCurve(clip, "", typeof(Transform), "localPosition.y", new[]
            {
                new Keyframe(0f, rootPosition.y),
                new Keyframe(0.5f, rootPosition.y + amplitude),
                new Keyframe(1f, rootPosition.y)
            });

            // The D2 procedural swim pitch belongs to its legacy fallback
            // visual root. The candidate is an independent presentation child,
            // so give only Fish a stable surface-swim pitch while every other
            // state explicitly restores the source root orientation. This
            // keeps a generic clip transition from leaving stale root rotation.
            // The candidate mesh is authored standing upright. A full
            // quarter-turn lays the visual child on the water plane; the D2
            // physical root remains authoritative and is never animated.
            const float candidateSwimPitch = 90f;
            var rootEuler = root.localEulerAngles;
            var rootPitch = rootEuler.x + (stateName == "Fish" ? candidateSwimPitch : 0f);
            AddConstantCurve(clip, "", "localEulerAnglesRaw.x", rootPitch);
            AddConstantCurve(clip, "", "localEulerAnglesRaw.y", rootEuler.y);
            AddConstantCurve(clip, "", "localEulerAnglesRaw.z", rootEuler.z);

            AddLimbCurve(clip, root, "thigh_l", stateName, true);
            AddLimbCurve(clip, root, "thigh_r", stateName, false);
            AddLimbCurve(clip, root, "upperarm_l", stateName, true);
            AddLimbCurve(clip, root, "upperarm_r", stateName, false);
            SetLoop(clip, loop);
            return clip;
        }

        private static void AddLimbCurve(AnimationClip clip, Transform root, string boneName,
            string stateName, bool left)
        {
            var bone = FindByName(root, boneName);
            if (bone == null) return;
            var path = AnimationUtility.CalculateTransformPath(bone, root);
            var baseEuler = bone.localEulerAngles;
            var magnitude = 0f;
            switch (stateName)
            {
                case "Walk": magnitude = 24f; break;
                case "Run": magnitude = 40f; break;
                case "Jump": magnitude = boneName.Contains("thigh") ? 12f : 18f; break;
                case "Fall": magnitude = boneName.Contains("thigh") ? 8f : 12f; break;
                case "Land": magnitude = boneName.Contains("thigh") ? 10f : 8f; break;
                case "Interact": magnitude = boneName.Contains("arm") ? 16f : 4f; break;
                case "Talk": magnitude = boneName.Contains("arm") ? 8f : 2f; break;
                case "Carry": magnitude = boneName.Contains("arm") ? 22f : 10f; break;
                // Candidate-only generic Fish presentation: keep the arms in a
                // restrained forward stroke envelope until a current-rig authored
                // swimming set or a verified Avatar/axis adapter is admitted.
                case "Fish": magnitude = boneName.Contains("arm") ? 8f : 5f; break;
                case "Board": magnitude = boneName.Contains("thigh") ? 14f : 20f; break;
                case "Disembark": magnitude = boneName.Contains("thigh") ? 12f : 16f; break;
                case "Helm": magnitude = boneName.Contains("arm") ? 18f : 3f; break;
                case "Hit": magnitude = boneName.Contains("arm") ? 26f : 8f; break;
                case "Recover": magnitude = boneName.Contains("arm") ? 12f : 6f; break;
            }

            var sign = left ? -1f : 1f;
            AddFloatCurve(clip, path, typeof(Transform), "localEulerAnglesRaw.x", new[]
            {
                new Keyframe(0f, baseEuler.x),
                new Keyframe(0.5f, baseEuler.x + sign * magnitude),
                new Keyframe(1f, baseEuler.x)
            });
            AddConstantCurve(clip, path, "localEulerAnglesRaw.y", baseEuler.y);
            AddConstantCurve(clip, path, "localEulerAnglesRaw.z", baseEuler.z);
        }

        private static void AddConstantCurve(AnimationClip clip, string path, string property, float value)
        {
            AddFloatCurve(clip, path, typeof(Transform), property, new[]
            {
                new Keyframe(0f, value),
                new Keyframe(0.5f, value),
                new Keyframe(1f, value)
            });
        }

        private static void AddFloatCurve(AnimationClip clip, string path, Type type,
            string property, Keyframe[] keys)
        {
            var curve = new AnimationCurve(keys);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, type, property), curve);
        }

        private static void SetLoop(AnimationClip clip, bool loop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        private static void WriteControllerAudit(AnimatorController controller,
            Dictionary<string, AnimationClip> clips)
        {
            var report = new ControllerAudit
            {
                task = "D20-A02-R01",
                controller_path = "game/" + ControllerPath,
                root_motion = false,
                states = new List<string>(),
                parameters = new List<string>(),
                clips = new List<ClipAudit>()
            };
            foreach (var state in controller.layers[0].stateMachine.states)
                report.states.Add(state.state.name);
            foreach (var parameter in controller.parameters)
                report.parameters.Add(parameter.name + ":" + parameter.type);
            foreach (var pair in clips)
            {
                var bindings = AnimationUtility.GetCurveBindings(pair.Value);
                report.clips.Add(new ClipAudit { state = pair.Key, curve_count = bindings.Length, loop = pair.Key == "Idle" || pair.Key == "Walk" || pair.Key == "Run" || pair.Key == "Interact" || pair.Key == "Talk" || pair.Key == "Carry" || pair.Key == "Fish" || pair.Key == "Helm" });
            }
            report.status = report.states.Count == States.Length && report.clips.Count == States.Length ? "PASS" : "PARTIAL";
            WriteResult("D20_A02_R01_CONTROLLER_AUDIT.json", JsonUtility.ToJson(report, true));
        }

        public static void CapturePreviews()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/Art/Models/Characters/D20/Player/D20_PlayerHumanoidAvatar_R01.asset");
            if (prefab == null || controller == null || avatar == null)
                throw new InvalidOperationException("D20-A02 preview inputs are incomplete.");

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var previewRoot = Path.Combine(projectRoot, ResultRoot, "previews");
            Directory.CreateDirectory(previewRoot);
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            var host = new GameObject("D20-A02-R01-AnimationHost");
            instance.transform.SetParent(host.transform, false);
            var animator = host.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            var cameraObject = new GameObject("D20-A02-R01-PreviewCamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.11f, 0.15f, 1f);
            camera.fieldOfView = 40f;
            var renderTexture = new RenderTexture(512, 768, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            camera.targetTexture = renderTexture;
            var views = new[] { "Idle", "Walk", "Board", "Fish" };
            var manifest = new PreviewAudit { task = "D20-A02-R01", width = 512, height = 768, files = new List<string>() };
            try
            {
                foreach (var state in views)
                {
                    animator.Play(state, 0, 0.25f);
                    animator.Update(0.05f);
                    camera.transform.position = new Vector3(0f, 0.95f, -4.8f);
                    camera.transform.LookAt(new Vector3(0f, 0.9f, 0f));
                    camera.Render();
                    RenderTexture.active = renderTexture;
                    var texture = new Texture2D(512, 768, TextureFormat.RGBA32, false);
                    texture.ReadPixels(new Rect(0, 0, 512, 768), 0, 0);
                    texture.Apply();
                    var file = "D20_A02_R01_PREVIEW_" + state.ToLowerInvariant() + ".png";
                    File.WriteAllBytes(Path.Combine(previewRoot, file), texture.EncodeToPNG());
                    manifest.files.Add("game/" + ResultRoot + "/previews/" + file);
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
            finally
            {
                RenderTexture.active = null;
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(host);
            }
            WriteResult("D20_A02_R01_PREVIEWS.json", JsonUtility.ToJson(manifest, true));
            Debug.Log("[D20-A02-R01] Preview captures written: " + manifest.files.Count);
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (string.IsNullOrEmpty(child)) return;
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static void DeleteIfExists(string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) AssetDatabase.DeleteAsset(path);
        }

        private static Transform FindByName(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindByName(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static void WriteResult(string fileName, string json)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var resultRoot = Path.Combine(projectRoot, ResultRoot);
            Directory.CreateDirectory(resultRoot);
            File.WriteAllText(Path.Combine(resultRoot, fileName), json + "\n");
        }

        [Serializable] private sealed class ControllerAudit
        {
            public string task;
            public string controller_path;
            public string status;
            public bool root_motion;
            public List<string> states;
            public List<string> parameters;
            public List<ClipAudit> clips;
        }

        [Serializable] private sealed class ClipAudit
        {
            public string state;
            public int curve_count;
            public bool loop;
        }

        [Serializable] private sealed class PreviewAudit
        {
            public string task;
            public int width;
            public int height;
            public List<string> files;
        }
    }
}
