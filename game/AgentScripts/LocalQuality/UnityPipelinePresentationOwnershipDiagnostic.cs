using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Whalebone.Gameplay.D2;
using WhalesIslandReef.Presentation.D20.PlayerAnimation;

namespace Whalebone.LocalQuality
{
    /// <summary>
    /// Read-only connected-Editor diagnostic for the active InitialHarbor
    /// player presentation owner. It records the D2 fallback, the D20 bridge,
    /// Animator/Avatar bindings and visible renderer hierarchy without moving
    /// the player or invoking gameplay methods.
    /// </summary>
    public static class UnityPipelinePresentationOwnershipDiagnostic
    {
        private const string ResultPath =
            "C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_EDITOR_PRESENTATION_OWNERSHIP_20260916_R1.json";

        public static string Write()
        {
            var json = Main();
            var directory = Path.GetDirectoryName(ResultPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(ResultPath, json);
            return ResultPath;
        }

        public static string Main()
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            var d2 = player == null ? null : player.GetComponent<D2CharacterAnimationController>();
            var controller = player == null ? null : player.GetComponent<D2CharacterController>();
            var bridge = player == null
                ? null
                : player.GetComponent<D20PlayerCandidatePresentationBridge>();
            var adapter = bridge == null
                ? null
                : bridge.GetComponentInChildren<D20PlayerAnimationPresentationAdapter>(true);
            var humanoid = bridge == null
                ? null
                : bridge.GetComponentInChildren<D20FormalHumanoidPoseAdapter>(true);
            var animators = player == null
                ? Array.Empty<Animator>()
                : player.GetComponentsInChildren<Animator>(true);
            var renderers = player == null
                ? Array.Empty<Renderer>()
                : player.GetComponentsInChildren<Renderer>(true);

            var report = new Report
            {
                task_id = "Q31",
                probe_id = "Q31_CURRENT_EDITOR_PRESENTATION_OWNERSHIP_20260916_R1",
                captured_at_local = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz"),
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                play_mode = Application.isPlaying,
                player_found = player != null,
                player_position = player == null ? Array.Empty<float>() : ToArray(player.transform.position),
                controller_swimming = controller != null && controller.IsSwimming,
                controller_grounded = controller != null && controller.IsGrounded,
                d2_animation_found = d2 != null,
                d2_pose_driver_mode = d2 == null ? string.Empty : d2.PoseDriverModeName,
                d2_procedural_pose_layer_ready = d2 != null && d2.ProceduralPoseLayerReady,
                d2_procedural_pose_layer_active = d2 != null && d2.ProceduralPoseLayerActive,
                d2_animator_pose_layer_active = d2 != null && d2.AnimatorPoseLayerActive,
                d2_imported_skinned_rig_neutral_only = d2 != null && d2.ImportedSkinnedRigNeutralOnly,
                d2_imported_visual_action_motion_active = d2 != null && d2.ImportedVisualActionMotionActive,
                d2_action = d2 == null ? string.Empty : d2.Action.ToString(),
                d2_bound_bone_count = d2 == null ? 0 : d2.BoundBoneCount,
                d2_visual_grounding_offset_local = d2 == null ? Array.Empty<float>() : ToArray(d2.VisualGroundingOffsetLocal),
                d20_bridge_found = bridge != null,
                d20_bridge_installed = bridge != null && bridge.IsInstalled,
                d20_bridge_mode = bridge == null ? string.Empty : bridge.ConsumerModeName,
                d20_bridge_installation_failure = bridge == null ? string.Empty : bridge.InstallationFailure,
                d20_uses_generic_path_adapter = bridge != null && bridge.UsesGenericPathAdapter,
                d20_uses_humanoid_pose_adapter = bridge != null && bridge.UsesHumanoidPoseAdapter,
                d20_swimming_visual_supported = bridge != null && bridge.SwimmingVisualSupported,
                d20_hidden_existing_renderer_count = bridge == null ? 0 : bridge.HiddenExistingRendererCount,
                d20_candidate_renderer_count = bridge == null ? 0 : bridge.CandidateRendererCount,
                d20_last_d2_action = bridge == null ? string.Empty : bridge.LastD2Action,
                d20_candidate_state = bridge == null ? string.Empty : bridge.LastCandidateState.ToString(),
                d20_adapter_found = adapter != null,
                d20_adapter_state = adapter == null ? string.Empty : adapter.CurrentState.ToString(),
                d20_adapter_last_state_name = adapter == null ? string.Empty : adapter.LastStateName,
                d20_adapter_last_apply_used_fallback = adapter != null && adapter.LastApplyUsedFallback,
                d20_animator_bound = adapter != null && adapter.BoundAnimator != null,
                d20_humanoid_adapter_found = humanoid != null,
                d20_humanoid_adapter_ready = humanoid != null && humanoid.IsReady,
                d20_humanoid_last_apply_succeeded = humanoid != null && humanoid.LastApplySucceeded,
                d20_humanoid_current_state = humanoid == null ? string.Empty : humanoid.CurrentState.ToString(),
                d20_humanoid_last_body_pitch_degrees = humanoid == null ? 0f : humanoid.LastBodyPitchDegrees,
                d20_animator_diagnostics = BuildAnimatorDiagnostics(animators),
                renderer_diagnostics = BuildRendererDiagnostics(renderers, player),
                hierarchy_diagnostics = BuildHierarchyDiagnostics(player),
                ownership_conclusion = BuildConclusion(d2, bridge, adapter, humanoid, animators, renderers)
            };
            return JsonUtility.ToJson(report, true);
        }

        private static string BuildConclusion(
            D2CharacterAnimationController d2,
            D20PlayerCandidatePresentationBridge bridge,
            D20PlayerAnimationPresentationAdapter adapter,
            D20FormalHumanoidPoseAdapter humanoid,
            Animator[] animators,
            Renderer[] renderers)
        {
            if (bridge == null || !bridge.IsInstalled)
                return "D20 bridge is absent or not installed; inspect the D2 visual owner.";
            if (bridge.UsesHumanoidPoseAdapter && humanoid != null && humanoid.IsReady)
                return "D20 Formal Humanoid pose adapter is the active presentation writer; D2 remains the physical root owner.";
            if (bridge.UsesGenericPathAdapter && adapter != null && adapter.BoundAnimator != null && adapter.BoundAnimator.isActiveAndEnabled)
                return "D20 generic path Animator is the active presentation writer; D2 remains the physical root owner.";
            return "D20 bridge is installed but the active presentation writer is unresolved; inspect diagnostics before changing code.";
        }

        private static string BuildAnimatorDiagnostics(Animator[] animators)
        {
            var lines = new List<string>();
            for (var i = 0; i < animators.Length; i++)
            {
                var animator = animators[i];
                if (animator == null) continue;
                var state = animator.runtimeAnimatorController == null
                    ? string.Empty
                    : animator.GetCurrentAnimatorStateInfo(0).shortNameHash.ToString();
                var controllerName = animator.runtimeAnimatorController == null
                    ? string.Empty
                    : animator.runtimeAnimatorController.name;
                var avatar = animator.avatar;
                lines.Add(RelativePath(animator.transform) +
                    "|enabled=" + animator.enabled +
                    "|active=" + animator.isActiveAndEnabled +
                    "|controller=" + controllerName +
                    "|avatar=" + (avatar == null ? string.Empty : avatar.name) +
                    "|avatarValid=" + (avatar != null && avatar.isValid) +
                    "|avatarHuman=" + (avatar != null && avatar.isHuman) +
                    "|stateHash=" + state +
                    "|applyRootMotion=" + animator.applyRootMotion +
                    "|culling=" + animator.cullingMode);
            }
            return string.Join("||", lines);
        }

        private static string BuildRendererDiagnostics(Renderer[] renderers, GameObject player)
        {
            var lines = new List<string>();
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null) continue;
                var bounds = renderer.bounds;
                lines.Add(RelativePath(renderer.transform) +
                    "|type=" + renderer.GetType().Name +
                    "|enabled=" + renderer.enabled +
                    "|active=" + renderer.gameObject.activeInHierarchy +
                    "|boundsCenter=" + Format(bounds.center) +
                    "|boundsSize=" + Format(bounds.size) +
                    "|root=" + renderer.transform.root.name);
            }
            return string.Join("||", lines);
        }

        private static string BuildHierarchyDiagnostics(GameObject player)
        {
            if (player == null) return string.Empty;
            var lines = new List<string>();
            var transforms = player.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var current = transforms[i];
                if (current == null) continue;
                var name = current.name.ToLowerInvariant();
                if (name.Contains("d20") || name.Contains("formal") || name.Contains("armature") ||
                    name.Contains("visual") || name.Contains("player"))
                    lines.Add(RelativePath(current) + "|active=" + current.gameObject.activeInHierarchy +
                        "|local=" + Format(current.localPosition) +
                        "|euler=" + Format(current.localEulerAngles));
            }
            return string.Join("||", lines);
        }

        private static string RelativePath(Transform current)
        {
            if (current == null) return string.Empty;
            var stack = new Stack<string>();
            var cursor = current;
            while (cursor != null)
            {
                stack.Push(cursor.name);
                cursor = cursor.parent;
            }
            return string.Join("/", stack.ToArray());
        }

        private static string Format(Vector3 value)
        {
            return value.x.ToString("F3") + "," + value.y.ToString("F3") + "," + value.z.ToString("F3");
        }

        private static float[] ToArray(Vector3 value)
        {
            return new[] { value.x, value.y, value.z };
        }

        [Serializable]
        private sealed class Report
        {
            public string task_id;
            public string probe_id;
            public string captured_at_local;
            public string scene;
            public bool play_mode;
            public bool player_found;
            public float[] player_position;
            public bool controller_swimming;
            public bool controller_grounded;
            public bool d2_animation_found;
            public string d2_pose_driver_mode;
            public bool d2_procedural_pose_layer_ready;
            public bool d2_procedural_pose_layer_active;
            public bool d2_animator_pose_layer_active;
            public bool d2_imported_skinned_rig_neutral_only;
            public bool d2_imported_visual_action_motion_active;
            public string d2_action;
            public int d2_bound_bone_count;
            public float[] d2_visual_grounding_offset_local;
            public bool d20_bridge_found;
            public bool d20_bridge_installed;
            public string d20_bridge_mode;
            public string d20_bridge_installation_failure;
            public bool d20_uses_generic_path_adapter;
            public bool d20_uses_humanoid_pose_adapter;
            public bool d20_swimming_visual_supported;
            public int d20_hidden_existing_renderer_count;
            public int d20_candidate_renderer_count;
            public string d20_last_d2_action;
            public string d20_candidate_state;
            public bool d20_adapter_found;
            public string d20_adapter_state;
            public string d20_adapter_last_state_name;
            public bool d20_adapter_last_apply_used_fallback;
            public bool d20_animator_bound;
            public bool d20_humanoid_adapter_found;
            public bool d20_humanoid_adapter_ready;
            public bool d20_humanoid_last_apply_succeeded;
            public string d20_humanoid_current_state;
            public float d20_humanoid_last_body_pitch_degrees;
            public string d20_animator_diagnostics;
            public string renderer_diagnostics;
            public string hierarchy_diagnostics;
            public string ownership_conclusion;
        }
    }
}
