using System;
using UnityEngine;
using Whalebone.Gameplay.D2;

namespace Whalebone.LocalQuality
{
    /// <summary>
    /// Ephemeral Pipeline probe for the current default D20 presentation route.
    /// It is compiled in memory and is not a production component.
    /// </summary>
    public static class UnityPipelineD20RouteProbe
    {
        public static string Main()
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            var bridge = player == null
                ? null
                : player.GetComponent<D20PlayerCandidatePresentationBridge>();
            var animation = player == null
                ? null
                : player.GetComponent<D2CharacterAnimationController>();
            var report = new Report
            {
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                player_found = player != null,
                player_skinned_renderer_count = player == null
                    ? 0
                    : player.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length,
                bridge_found = bridge != null,
                bridge_installed = bridge != null && bridge.IsInstalled,
                consumer_mode = bridge == null ? string.Empty : bridge.ConsumerModeName,
                formal_consumer = bridge != null && bridge.IsFormalConsumer,
                generic_path_adapter = bridge != null && bridge.UsesGenericPathAdapter,
                humanoid_pose_adapter = bridge != null && bridge.UsesHumanoidPoseAdapter,
                candidate_renderer_count = bridge == null ? 0 : bridge.CandidateRendererCount,
                hidden_existing_renderer_count = bridge == null ? 0 : bridge.HiddenExistingRendererCount,
                swimming_visual_supported = bridge != null && bridge.SwimmingVisualSupported,
                d2_action = animation == null ? string.Empty : animation.Action.ToString(),
                d2_pose_driver = animation == null ? string.Empty : animation.PoseDriverModeName,
                d2_formal_animator_ready = animation != null && animation.FormalAnimatorReady,
                d2_animator_pose_active = animation != null && animation.AnimatorPoseLayerActive,
                d2_procedural_pose_active = animation != null && animation.ProceduralPoseLayerActive,
                bridge_last_d2_action = bridge == null ? string.Empty : bridge.LastD2Action,
                bridge_last_candidate_state = bridge == null ? string.Empty : bridge.LastCandidateState.ToString(),
                bridge_installation_failure = bridge == null ? string.Empty : bridge.InstallationFailure,
            };
            return JsonUtility.ToJson(report, true);
        }

        [Serializable]
        private sealed class Report
        {
            public string scene;
            public bool player_found;
            public int player_skinned_renderer_count;
            public bool bridge_found;
            public bool bridge_installed;
            public string consumer_mode;
            public bool formal_consumer;
            public bool generic_path_adapter;
            public bool humanoid_pose_adapter;
            public int candidate_renderer_count;
            public int hidden_existing_renderer_count;
            public bool swimming_visual_supported;
            public string d2_action;
            public string d2_pose_driver;
            public bool d2_formal_animator_ready;
            public bool d2_animator_pose_active;
            public bool d2_procedural_pose_active;
            public string bridge_last_d2_action;
            public string bridge_last_candidate_state;
            public string bridge_installation_failure;
        }
    }
}
