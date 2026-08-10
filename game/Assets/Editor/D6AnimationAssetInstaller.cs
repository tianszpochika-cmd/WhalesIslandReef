// ============================================================
// D6AnimationAssetInstaller —— 角色动作资源管线
//
// ph_crew_male 当前没有随 FBX 提供 AnimationClip。这里把动作状态
// 预先生成成 Unity AnimationClip + AnimatorController 资源，运行时
// 只负责选择状态；以后替换为正式动捕资源时不需要改输入和控制代码。
// ============================================================

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Whalebone.EditorTools
{
    [InitializeOnLoad]
    public static class D6AnimationAssetInstaller
    {
        private const string CrewPrefabPath = "Assets/Prefabs/Placeholder/ph_crew_male.prefab";
        private const string CrewFbxPath = "Assets/Art/Placeholder/Models/Crew/ph_crew_male.fbx";
        private const string GeneratedFolder = "Assets/Resources/D6";
        private const string ControllerPath = GeneratedFolder + "/D6CrewAnimator.controller";
        private static int _ensureAttempts;

        private static readonly string[] ActionNames =
        {
            "Idle", "Walk", "Run", "Jump", "Boarding", "Disembarking"
        };

        private sealed class BoneBinding
        {
            public readonly Transform Transform;
            public readonly string Path;
            public readonly Vector3 BaseEuler;

            public BoneBinding(Transform transform, Transform root)
            {
                Transform = transform;
                Path = AnimationUtility.CalculateTransformPath(transform, root);
                BaseEuler = transform.localEulerAngles;
            }
        }

        static D6AnimationAssetInstaller()
        {
            EditorApplication.delayCall += TryEnsure;
        }

        [MenuItem("Tools/Whalebone/Rebuild D6 Character Animations")]
        public static void Rebuild()
        {
            Build(true);
        }

        /// <summary>供港口安装器在生成角色 Prefab 后立即补齐动作资源。</summary>
        public static void EnsureAfterHarborInstall()
        {
            Build(false);
        }

        private static void TryEnsure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath) != null)
                return;

            Build(false);
            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath) == null &&
                _ensureAttempts++ < 20)
                EditorApplication.delayCall += TryEnsure;
        }

        private static void Build(bool force)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            AssetDatabase.ImportAsset(CrewFbxPath, ImportAssetOptions.ForceUpdate);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CrewPrefabPath);
            if (prefab == null) prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CrewFbxPath);
            if (prefab == null)
            {
                Debug.LogWarning("[D6/Animation] 尚未找到 ph_crew_male，动作资源将在港口资产安装后自动生成。");
                return;
            }

            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets/Resources", "D6");
            if (!force && AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath) != null)
                return;

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ControllerPath) != null)
                AssetDatabase.DeleteAsset(ControllerPath);
            foreach (var action in ActionNames)
            {
                string path = GeneratedFolder + "/D6Crew_" + action + ".anim";
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                    AssetDatabase.DeleteAsset(path);
            }

            Transform root = prefab.transform;
            Transform visualRoot = root.childCount == 0 ? root : root.GetChild(0);
            var bindings = DiscoverBindings(root);
            var clips = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
            foreach (var action in ActionNames)
            {
                var clip = CreateClip(action, root, visualRoot, bindings);
                string path = GeneratedFolder + "/D6Crew_" + action + ".anim";
                AssetDatabase.CreateAsset(clip, path);
                clips[action] = clip;
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var stateMachine = controller.layers[0].stateMachine;
            bool first = true;
            int stateIndex = 0;
            foreach (var action in ActionNames)
            {
                var state = stateMachine.AddState("D6_" + action, new Vector3(240f, stateIndex * 55f, 0f));
                state.motion = clips[action];
                // 这些是“局部姿态”片段，不应该把没有打关键帧的骨骼重置到
                // Animator 默认姿态；否则导入模型的原始骨骼方向会被覆盖。
                state.writeDefaultValues = false;
                if (first)
                {
                    stateMachine.defaultState = state;
                    first = false;
                }
                stateIndex++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[D6/Animation] 已生成 Unity 动作资源：AnimatorController + " + bindings.Count + " 个骨骼绑定");
        }

        private static List<BoneBinding> DiscoverBindings(Transform root)
        {
            var bones = root.GetComponentsInChildren<Transform>(true);
            var result = new List<BoneBinding>();
            AddBinding(result, FindBone(bones, "left", "upperarm", "arm", "shoulder"), root);
            AddBinding(result, FindBone(bones, "right", "upperarm", "arm", "shoulder"), root);
            AddBinding(result, FindBone(bones, "left", "upleg", "upperleg", "thigh", "femur"), root);
            AddBinding(result, FindBone(bones, "right", "upleg", "upperleg", "thigh", "femur"), root);
            AddBinding(result, FindBone(bones, "left", "lowerleg", "calf", "shin", "leg"), root);
            AddBinding(result, FindBone(bones, "right", "lowerleg", "calf", "shin", "leg"), root);
            AddBinding(result, FindBone(bones, "left", "foot", "ankle"), root);
            AddBinding(result, FindBone(bones, "right", "foot", "ankle"), root);
            return result;
        }

        private static void AddBinding(List<BoneBinding> list, Transform transform, Transform root)
        {
            if (transform == null) return;
            foreach (var binding in list)
                if (binding.Transform == transform) return;
            list.Add(new BoneBinding(transform, root));
        }

        private static AnimationClip CreateClip(string action, Transform root, Transform visualRoot,
            List<BoneBinding> bindings)
        {
            bool loop = action == "Idle" || action == "Walk" || action == "Run";
            float duration = action == "Run" ? 0.55f : action == "Walk" ? 0.78f : 0.9f;
            var clip = new AnimationClip
            {
                name = "D6Crew_" + action,
                frameRate = 30f,
                legacy = false,
                wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever
            };

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.stopTime = duration;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            float bob = action == "Run" ? 0.045f : action == "Walk" ? 0.028f : action == "Idle" ? 0.012f : 0f;
            AddFloatCurve(clip, visualRoot, root, "m_LocalPosition.y",
                new[] { 0f, duration * 0.5f, duration },
                new[] { visualRoot.localPosition.y, visualRoot.localPosition.y + bob, visualRoot.localPosition.y });

            float arm = 0f;
            float leg = 0f;
            float knee = 0f;
            float lean = 0f;
            switch (action)
            {
                case "Walk": arm = 24f; leg = 30f; break;
                case "Run": arm = 38f; leg = 48f; lean = 7f; break;
                case "Jump": arm = -28f; leg = 14f; knee = 24f; break;
                case "Boarding": arm = -34f; leg = 16f; knee = 26f; lean = -10f; break;
                case "Disembarking": arm = 26f; leg = -16f; knee = 18f; lean = 8f; break;
                default: arm = 2f; break;
            }

            var armBindings = new[]
            {
                FindBinding(bindings, "left", "arm"), FindBinding(bindings, "right", "arm")
            };
            var legBindings = new[]
            {
                FindBinding(bindings, "left", "leg"), FindBinding(bindings, "right", "leg")
            };
            var footBindings = new[]
            {
                FindBinding(bindings, "left", "foot"), FindBinding(bindings, "right", "foot")
            };

            for (int i = 0; i < armBindings.Length; i++)
            {
                var binding = armBindings[i];
                if (binding == null) continue;
                float sign = i == 0 ? 1f : -1f;
                AddRotationCurve(clip, binding, duration, sign * arm, action == "Idle" ? 1f : -1f);
            }
            for (int i = 0; i < legBindings.Length; i++)
            {
                var binding = legBindings[i];
                if (binding == null) continue;
                float sign = i == 0 ? -1f : 1f;
                AddRotationCurve(clip, binding, duration, sign * leg, 1f);
            }
            for (int i = 0; i < footBindings.Length; i++)
            {
                var binding = footBindings[i];
                if (binding == null) continue;
                float sign = i == 0 ? -1f : 1f;
                AddRotationCurve(clip, binding, duration, sign * knee * 0.2f, 1f);
            }

            if (visualRoot != root)
                AddTransformRotationCurves(clip, visualRoot, root, duration, lean);

            return clip;
        }

        private static void AddRotationCurve(AnimationClip clip, BoneBinding binding, float duration,
            float peak, float phaseSign)
        {
            float[] times = { 0f, duration * 0.5f, duration };
            AddFloatCurve(clip, binding.Transform, null, "localEulerAnglesRaw.x",
                times,
                new[] { binding.BaseEuler.x, binding.BaseEuler.x + peak * phaseSign, binding.BaseEuler.x });
            AddConstantRotationCurve(clip, binding.Transform, null, "localEulerAnglesRaw.y", times, binding.BaseEuler.y);
            AddConstantRotationCurve(clip, binding.Transform, null, "localEulerAnglesRaw.z", times, binding.BaseEuler.z);
        }

        private static void AddTransformRotationCurves(AnimationClip clip, Transform target, Transform root,
            float duration, float xOffset)
        {
            Vector3 baseEuler = target.localEulerAngles;
            float[] times = { 0f, duration };
            AddFloatCurve(clip, target, root, "localEulerAnglesRaw.x", times,
                new[] { baseEuler.x + xOffset, baseEuler.x + xOffset });
            AddConstantRotationCurve(clip, target, root, "localEulerAnglesRaw.y", times, baseEuler.y);
            AddConstantRotationCurve(clip, target, root, "localEulerAnglesRaw.z", times, baseEuler.z);
        }

        private static void AddConstantRotationCurve(AnimationClip clip, Transform target, Transform root,
            string property, float[] times, float value)
        {
            var values = new float[times.Length];
            for (int i = 0; i < values.Length; i++) values[i] = value;
            AddFloatCurve(clip, target, root, property, times, values);
        }

        private static void AddFloatCurve(AnimationClip clip, Transform target, Transform root,
            string property, float[] times, float[] values)
        {
            if (target == null) return;
            if (root == null) root = target.root;
            var binding = EditorCurveBinding.FloatCurve(
                AnimationUtility.CalculateTransformPath(target, root), typeof(Transform), property);
            var keys = new Keyframe[times.Length];
            for (int i = 0; i < times.Length; i++)
                keys[i] = new Keyframe(times[i], values[i]);
            AnimationUtility.SetEditorCurve(clip, binding, new AnimationCurve(keys));
        }

        private static BoneBinding FindBinding(List<BoneBinding> bindings, string side, string part)
        {
            foreach (var binding in bindings)
            {
                string name = NormalizeName(binding.Transform.name);
                if (HasSide(name, side) && name.Contains(part)) return binding;
            }
            return null;
        }

        private static Transform FindBone(Transform[] bones, string side, params string[] tokens)
        {
            Transform best = null;
            int bestScore = -1;
            foreach (var bone in bones)
            {
                string name = NormalizeName(bone.name);
                if (!HasSide(name, side)) continue;
                if (side == "left" && name.Contains("right")) continue;
                if (side == "right" && name.Contains("left")) continue;
                int score = 0;
                foreach (var token in tokens)
                {
                    string normalizedToken = NormalizeName(token);
                    if (name.Contains(normalizedToken)) score = Mathf.Max(score, normalizedToken.Length);
                }
                if (tokens.Length > 0 && tokens[tokens.Length - 1] == "leg" &&
                    (name.Contains("upper") || name.Contains("up"))) score = 0;
                if (score > bestScore)
                {
                    best = bone;
                    bestScore = score;
                }
            }
            return bestScore < 0 ? null : best;
        }

        private static bool HasSide(string name, string side)
        {
            if (name.Contains(side)) return true;
            return name.EndsWith(side == "left" ? "l" : "r", StringComparison.Ordinal);
        }

        private static string NormalizeName(string value)
        {
            return (value ?? string.Empty).ToLowerInvariant()
                .Replace("_", string.Empty).Replace("-", string.Empty)
                .Replace(" ", string.Empty).Replace(":", string.Empty);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
