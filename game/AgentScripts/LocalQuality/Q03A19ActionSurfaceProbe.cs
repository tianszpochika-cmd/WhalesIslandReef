using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Q03-A19 evidence probe: sample the authored Humanoid action surface on the
/// real D20 candidate prefab and report bone-level pose deltas per state, so
/// the gate result is backed by observed skeletal motion rather than only by
/// clip metadata. Read-only.
/// </summary>
public static class Q03A19ActionSurfaceProbe
{
    public static string Run()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Resources/D20/Candidate/D20_R02_PlayerVisual_Male.prefab");
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Resources/D20/Candidate/D20_PlayerAnimation.controller");
        if (prefab == null || controller == null) return "inputs missing";

        var sb = new StringBuilder();
        var states = new[] { "Idle", "Walk", "Run", "Fish", "Helm", "Board", "Hit" };

        var instance = Object.Instantiate(prefab);
        try
        {
            var animator = instance.GetComponentInChildren<Animator>(true);
            if (animator == null) return "no animator";
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            // The runtime bridge sets AlwaysAnimate; without it a batch-mode
            // Animator culls skeletal updates and reports a frozen pose.
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.Rebind();

            var root = instance.transform;
            var bones = root.GetComponentsInChildren<Transform>(true)
                .Where(t => !t.name.StartsWith("D20Socket_", System.StringComparison.Ordinal))
                .ToArray();
            var rest = bones.Select(b => b.localRotation).ToArray();

            sb.Append("bones=").Append(bones.Length)
              .Append(" avatar=").Append(animator.avatar == null ? "null" : animator.avatar.name)
              .Append(" humanScale=").Append(animator.humanScale.ToString("F3"))
              .Append("  ||  ");

            foreach (var state in states)
            {
                animator.Play(state, 0, 0.5f);
                animator.Update(0.1f);
                var moved = 0;
                var max = 0f;
                var maxBone = "-";
                for (var i = 0; i < bones.Length; i++)
                {
                    var angle = Quaternion.Angle(rest[i], bones[i].localRotation);
                    if (angle > 1f) moved++;
                    if (angle > max) { max = angle; maxBone = bones[i].name; }
                }
                sb.Append(state).Append(":moved=").Append(moved)
                  .Append(",max=").Append(max.ToString("F1")).Append("°@").Append(maxBone).Append("  ");
            }
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }

        return sb.ToString();
    }
}
