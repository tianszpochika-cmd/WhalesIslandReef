#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Whalebone.Gameplay.D2;

namespace Save.Tests
{
    /// <summary>验证 D2 缺少正式船员资源时仍能生成可驱动人物的编辑器烟测。</summary>
    public static class D2FallbackHumanoidSmokeRunner
    {
        public static void Run()
        {
            GameObject root = null;
            try
            {
                root = D2FallbackHumanoidBuilder.Create(
                    "D2_FallbackHumanoid_Smoke",
                    Vector3.zero,
                    new Color(0.12f, 0.34f, 0.62f));

                int rendererCount = root.GetComponentsInChildren<Renderer>(true).Length;
                bool childCollidersDisabled = true;
                int colliderCount = 0;
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                {
                    colliderCount++;
                    if (collider.enabled) childCollidersDisabled = false;
                }

                string[] requiredBones =
                {
                    "HumanoidVisual/Hips/Spine/LeftUpperArm",
                    "HumanoidVisual/Hips/Spine/RightUpperArm",
                    "HumanoidVisual/Hips/LeftUpperLeg",
                    "HumanoidVisual/Hips/RightUpperLeg",
                    "HumanoidVisual/Hips/LeftUpperLeg/LeftLowerLeg",
                    "HumanoidVisual/Hips/RightUpperLeg/RightLowerLeg",
                    "HumanoidVisual/Hips/LeftUpperLeg/LeftLowerLeg/LeftFoot",
                    "HumanoidVisual/Hips/RightUpperLeg/RightLowerLeg/RightFoot"
                };
                bool bonesNamed = true;
                for (int i = 0; i < requiredBones.Length; i++)
                    if (root.transform.Find(requiredBones[i]) == null) bonesNamed = false;

                bool visual = D2CharacterVisualProbe.HasCharacterVisual(root);
                bool passed = visual && rendererCount >= 10 && colliderCount >= 1 &&
                    childCollidersDisabled && bonesNamed;
                Debug.Log((passed ? "[D2 Fallback Smoke] PASS" : "[D2 Fallback Smoke] FAIL") +
                          ": visual=" + visual +
                          ", renderers=" + rendererCount +
                          ", colliders=" + colliderCount +
                          ", childCollidersDisabled=" + childCollidersDisabled +
                          ", bonesNamed=" + bonesNamed);
                if (!passed) EditorApplication.Exit(1);
                else EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
            finally
            {
                if (root != null) UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
#endif
