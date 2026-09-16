using System;
using System.Reflection;
using UnityEngine;
using Whalebone.Gameplay.D2;

namespace Whalebone.LocalQuality
{
    /// <summary>
    /// Read-only Pipeline probe for the current procedural swim visual-root
    /// submission. This records the private baseline/offset inputs so a failed
    /// visual-height contract can be diagnosed without changing runtime state.
    /// </summary>
    public static class UnityPipelineSwimmingRootProbe
    {
        [Serializable]
        private sealed class Report
        {
            public bool player_found;
            public bool swimming;
            public string action;
            public float player_y;
            public Vector3 visual_root_local_position;
            public Vector3 visual_root_base_position;
            public Vector3 visual_grounding_offset_local;
            public Vector3 visual_root_local_euler;
            public Vector3 visual_root_base_euler;
            public float swimming_body_drop;
            public float swimming_pitch_degrees;
            public float swimming_roll_degrees;
        }

        public static string Main()
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            var animation = player == null ? null : player.GetComponent<D2CharacterAnimationController>();
            if (player == null || animation == null)
                return JsonUtility.ToJson(new Report { player_found = false });

            var type = animation.GetType();
            var visualRoot = (Transform)ReadPrivate(type, animation, "_visualRoot");
            var basePosition = (Vector3)ReadPrivate(type, animation, "_visualRootBasePosition");
            var baseRotation = (Quaternion)ReadPrivate(type, animation, "_visualRootBaseRotation");
            var groundingOffset = (Vector3)ReadPrivate(type, animation, "_visualGroundingOffsetLocal");
            var controller = player.GetComponent<D2CharacterController>();
            return JsonUtility.ToJson(new Report
            {
                player_found = true,
                swimming = controller != null && controller.IsSwimming,
                action = animation.Action.ToString(),
                player_y = player.transform.position.y,
                visual_root_local_position = visualRoot == null ? Vector3.zero : visualRoot.localPosition,
                visual_root_base_position = basePosition,
                visual_grounding_offset_local = groundingOffset,
                visual_root_local_euler = visualRoot == null ? Vector3.zero : visualRoot.localEulerAngles,
                visual_root_base_euler = baseRotation.eulerAngles,
                swimming_body_drop = animation.SwimmingVisualBodyDrop,
                swimming_pitch_degrees = animation.SwimmingVisualPitchDegrees,
                swimming_roll_degrees = animation.SwimmingVisualRollDegrees,
            });
        }

        private static object ReadPrivate(Type type, object target, string fieldName)
        {
            return type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }
    }
}
