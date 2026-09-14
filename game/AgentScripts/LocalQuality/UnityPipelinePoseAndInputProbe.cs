using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.D7;
using Whalebone.Gameplay.Harbor;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.World;

namespace Whalebone.LocalQuality
{
    /// <summary>
    /// Ephemeral Unity Pipeline probe for the current InitialHarbor runtime.
    /// This file is compiled in memory by the Pipeline command and is not a
    /// production component or a substitute for a foreground Player capture.
    /// </summary>
    public static class UnityPipelinePoseAndInputProbe
    {
        public static string Main()
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            var animation = player == null ? null : player.GetComponent<D2CharacterAnimationController>();
            var controller = player == null ? null : player.GetComponent<D2CharacterController>();
            var eventSystem = EventSystem.current;
            var input = eventSystem == null ? null : eventSystem.GetComponent<InputSystemUIInputModule>();
            var router = UnityEngine.Object.FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            var ship = GameObject.Find("ph_ship_fishing");
            var deck = ship == null ? null : ship.GetComponent<D7ShipDeckController>();
            var characterBody = player == null ? null : player.GetComponent<CharacterController>();
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<D2RuntimeBootstrap>(FindObjectsInactive.Include);
            var ocean = UnityEngine.Object.FindFirstObjectByType<OceanAdapter>(FindObjectsInactive.Include);
            var collision = UnityEngine.Object.FindFirstObjectByType<HarborCollisionController>(FindObjectsInactive.Include);
            var leftHand = player == null ? null : player.transform.Find("HumanoidVisual/Hips/Spine/LeftUpperArm/LeftLowerArm/LeftHandMesh");
            var rightHand = player == null ? null : player.transform.Find("HumanoidVisual/Hips/Spine/RightUpperArm/RightLowerArm/RightHandMesh");
            var helmWheel = ship == null ? null : ship.transform.Find("D19_R03_ShipVisualInstance/HelmWheel");
            var visibleHelm = ship == null ? null : ship.transform.Find("D19_R03_ShipVisualInstance/D7_VisibleHelmAnchor");
            var characterData = ReadCharacterController(characterBody);
            var harborData = ReadHarborCollision(collision);
            var report = new Report
            {
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                play_mode = Application.isPlaying,
                time_scale = Time.timeScale,
                player_found = player != null,
                player_parent = player == null || player.transform.parent == null
                    ? string.Empty
                    : player.transform.parent.name,
                action = animation == null ? string.Empty : animation.Action.ToString(),
                pose_driver_mode = animation == null ? string.Empty : animation.PoseDriverModeName,
                bound_bone_count = animation == null ? 0 : animation.BoundBoneCount,
                procedural_ready = animation != null && animation.ProceduralPoseLayerReady,
                procedural_active = animation != null && animation.ProceduralPoseLayerActive,
                ship_steering_pose_active = animation != null && animation.ShipSteeringPoseActive,
                swimming_pitch_degrees = animation == null ? 0f : animation.SwimmingVisualPitchDegrees,
                swimming_roll_degrees = animation == null ? 0f : animation.SwimmingVisualRollDegrees,
                swimming_body_drop = animation == null ? 0f : animation.SwimmingVisualBodyDrop,
                controller_grounded = controller != null && controller.IsGrounded,
                controller_swimming = controller != null && controller.IsSwimming,
                controller_moving = controller != null && controller.IsMoving,
                control_mode = router == null ? string.Empty : router.Mode.ToString(),
                player_position = player == null ? Array.Empty<float>() : ToArray(player.transform.position),
                player_lossy_scale = player == null ? Array.Empty<float>() : ToArray(player.transform.lossyScale),
                ship_position = ship == null ? Array.Empty<float>() : ToArray(ship.transform.position),
                ship_lossy_scale = ship == null ? Array.Empty<float>() : ToArray(ship.transform.lossyScale),
                ship_distance = player == null || ship == null
                    ? -1f
                    : Vector3.Distance(player.transform.position, ship.transform.position),
                helm_position = deck == null || deck.HelmPoint == null
                    ? Array.Empty<float>()
                    : ToArray(deck.HelmPoint.position),
                left_hand_to_helm = leftHand == null || deck == null || deck.HelmPoint == null
                    ? -1f
                    : Vector3.Distance(leftHand.position, deck.HelmPoint.position),
                right_hand_to_helm = rightHand == null || deck == null || deck.HelmPoint == null
                    ? -1f
                    : Vector3.Distance(rightHand.position, deck.HelmPoint.position),
                helm_wheel_position = helmWheel == null ? Array.Empty<float>() : ToArray(helmWheel.position),
                visible_helm_anchor_position = visibleHelm == null ? Array.Empty<float>() : ToArray(visibleHelm.position),
                left_hand_to_wheel = leftHand == null || helmWheel == null
                    ? -1f
                    : Vector3.Distance(leftHand.position, helmWheel.position),
                right_hand_to_wheel = rightHand == null || helmWheel == null
                    ? -1f
                    : Vector3.Distance(rightHand.position, helmWheel.position),
                event_system_found = eventSystem != null,
                ui_navigation_enabled = eventSystem != null && eventSystem.sendNavigationEvents,
                ui_move_action_bound = input != null && input.move != null,
                selected_ui_object = eventSystem == null || eventSystem.currentSelectedGameObject == null
                    ? string.Empty
                    : eventSystem.currentSelectedGameObject.name,
                renderer_count = player == null ? 0 : player.GetComponentsInChildren<Renderer>(true).Length,
                skinned_renderer_count = player == null ? 0 : player.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length,
                renderer_bounds = ReadRendererBounds(player),
                visual_children = ReadVisualChildren(player),
                left_hand_position = ReadWorldPosition(player, "HumanoidVisual/Hips/Spine/LeftUpperArm/LeftLowerArm/LeftHandMesh"),
                right_hand_position = ReadWorldPosition(player, "HumanoidVisual/Hips/Spine/RightUpperArm/RightLowerArm/RightHandMesh"),
                left_foot_position = ReadWorldPosition(player, "HumanoidVisual/Hips/LeftUpperLeg/LeftLowerLeg/LeftFoot/LeftFootMesh"),
                right_foot_position = ReadWorldPosition(player, "HumanoidVisual/Hips/RightUpperLeg/RightLowerLeg/RightFoot/RightFootMesh"),
                visual_root_position = ReadWorldPosition(player, "HumanoidVisual"),
                visual_root_local_scale = ReadLocalScale(player, "HumanoidVisual"),
                visual_root_euler = ReadWorldEuler(player, "HumanoidVisual"),
                left_upper_arm_euler = ReadLocalEuler(player, "HumanoidVisual/Hips/Spine/LeftUpperArm"),
                right_upper_arm_euler = ReadLocalEuler(player, "HumanoidVisual/Hips/Spine/RightUpperArm"),
                left_lower_arm_euler = ReadLocalEuler(player, "HumanoidVisual/Hips/Spine/LeftUpperArm/LeftLowerArm"),
                right_lower_arm_euler = ReadLocalEuler(player, "HumanoidVisual/Hips/Spine/RightUpperArm/RightLowerArm"),
                character_controller = characterData,
                controller_enabled = characterData.enabled,
                controller_radius = characterData.radius,
                controller_height = characterData.height,
                controller_skin_width = characterData.skin_width,
                controller_center = characterData.center,
                bootstrap_ready = bootstrap != null && bootstrap.IsReady,
                completed_runtime_phase_count = bootstrap == null ? 0 : bootstrap.CompletedRuntimePhaseCount,
                ocean_water_surface_y = ocean == null ? float.NaN : ocean.WaterSurfaceY,
                ocean_water_half_extent = ocean == null ? 0f : ocean.WaterHalfExtent,
                ocean_fallback_active = ocean != null && ocean.FallbackActive,
                deck_ready = deck != null && deck.IsReady,
                deck_collision_ready = deck != null && deck.DeckCollisionReady,
                harbor_collision = harborData,
                harbor_collision_initialized = harborData.initialized,
                harbor_r03_surface_installed = harborData.r03_surface_installed,
                harbor_collider_count = harborData.collider_count,
                harbor_walkable_surface_count = harborData.walkable_surface_count,
                harbor_r03_building_collision_count = harborData.r03_building_collision_count,
            };
            return JsonUtility.ToJson(report, true);
        }

        public static string Board()
        {
            var router = UnityEngine.Object.FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            var succeeded = router != null && router.TryToggleShip();
            return "{\"operation\":\"board\",\"succeeded\":" + succeeded.ToString().ToLowerInvariant() + "}";
        }

        public static string MoveToOpenWater()
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<D2CharacterController>(FindObjectsInactive.Include);
            if (controller == null)
                return "{\"operation\":\"move_to_open_water\",\"succeeded\":false}";
            controller.SetWorldPosition(new Vector3(24f, 0.15f, -24f));
            controller.SetInputEnabled(true);
            return "{\"operation\":\"move_to_open_water\",\"succeeded\":true}";
        }

        private static BoundsData ReadRendererBounds(GameObject player)
        {
            var result = new BoundsData();
            if (player == null) return result;
            var renderers = player.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return result;
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                if (renderers[i] != null) bounds.Encapsulate(renderers[i].bounds);
            result.min = ToArray(bounds.min);
            result.max = ToArray(bounds.max);
            result.center = ToArray(bounds.center);
            result.size = ToArray(bounds.size);
            return result;
        }

        private static List<VisualChild> ReadVisualChildren(GameObject player)
        {
            var result = new List<VisualChild>();
            if (player == null) return result;
            for (var i = 0; i < player.transform.childCount; i++)
            {
                var child = player.transform.GetChild(i);
                result.Add(new VisualChild
                {
                    name = child.name,
                    local_position = ToArray(child.localPosition),
                    local_euler = ToArray(child.localEulerAngles),
                    renderer_count = child.GetComponentsInChildren<Renderer>(true).Length,
                });
            }
            return result;
        }

        private static float[] ReadWorldPosition(GameObject player, string relativePath)
        {
            if (player == null) return Array.Empty<float>();
            var target = player.transform.Find(relativePath);
            return target == null ? Array.Empty<float>() : ToArray(target.position);
        }

        private static float[] ReadWorldEuler(GameObject player, string relativePath)
        {
            if (player == null) return Array.Empty<float>();
            var target = player.transform.Find(relativePath);
            return target == null ? Array.Empty<float>() : ToArray(target.eulerAngles);
        }

        private static float[] ReadLocalScale(GameObject player, string relativePath)
        {
            if (player == null) return Array.Empty<float>();
            var target = player.transform.Find(relativePath);
            return target == null ? Array.Empty<float>() : ToArray(target.localScale);
        }

        private static float[] ReadLocalEuler(GameObject player, string relativePath)
        {
            if (player == null) return Array.Empty<float>();
            var target = player.transform.Find(relativePath);
            return target == null ? Array.Empty<float>() : ToArray(target.localEulerAngles);
        }

        private static float[] ToArray(Vector3 value) => new[] { value.x, value.y, value.z };

        private static CharacterControllerData ReadCharacterController(CharacterController body)
        {
            if (body == null) return new CharacterControllerData();
            return new CharacterControllerData
            {
                enabled = body.enabled,
                radius = body.radius,
                height = body.height,
                skin_width = body.skinWidth,
                center = ToArray(body.center),
            };
        }

        private static HarborCollisionData ReadHarborCollision(HarborCollisionController collision)
        {
            if (collision == null) return new HarborCollisionData();
            return new HarborCollisionData
            {
                initialized = collision.IsInitialized,
                r03_surface_installed = collision.R03SurfaceInstalled,
                collider_count = collision.ColliderCount,
                walkable_surface_count = collision.WalkableSurfaceCount,
                r03_building_collision_count = collision.R03BuildingCollisionCount,
            };
        }

        [Serializable]
        private sealed class Report
        {
            public string scene;
            public bool play_mode;
            public float time_scale;
            public bool player_found;
            public string player_parent;
            public string action;
            public string pose_driver_mode;
            public int bound_bone_count;
            public bool procedural_ready;
            public bool procedural_active;
            public bool ship_steering_pose_active;
            public float swimming_pitch_degrees;
            public float swimming_roll_degrees;
            public float swimming_body_drop;
            public bool controller_grounded;
            public bool controller_swimming;
            public bool controller_moving;
            public string control_mode;
            public float[] player_position;
            public float[] player_lossy_scale;
            public float[] ship_position;
            public float[] ship_lossy_scale;
            public float ship_distance;
            public float[] helm_position;
            public float left_hand_to_helm;
            public float right_hand_to_helm;
            public float[] helm_wheel_position;
            public float[] visible_helm_anchor_position;
            public float left_hand_to_wheel;
            public float right_hand_to_wheel;
            public bool event_system_found;
            public bool ui_navigation_enabled;
            public bool ui_move_action_bound;
            public string selected_ui_object;
            public int renderer_count;
            public int skinned_renderer_count;
            public BoundsData renderer_bounds;
            public List<VisualChild> visual_children;
            public float[] left_hand_position;
            public float[] right_hand_position;
            public float[] left_foot_position;
            public float[] right_foot_position;
            public float[] visual_root_position;
            public float[] visual_root_local_scale;
            public float[] visual_root_euler;
            public float[] left_upper_arm_euler;
            public float[] right_upper_arm_euler;
            public float[] left_lower_arm_euler;
            public float[] right_lower_arm_euler;
            public CharacterControllerData character_controller;
            public bool controller_enabled;
            public float controller_radius;
            public float controller_height;
            public float controller_skin_width;
            public float[] controller_center;
            public bool bootstrap_ready;
            public int completed_runtime_phase_count;
            public float ocean_water_surface_y;
            public float ocean_water_half_extent;
            public bool ocean_fallback_active;
            public bool deck_ready;
            public bool deck_collision_ready;
            public HarborCollisionData harbor_collision;
            public bool harbor_collision_initialized;
            public bool harbor_r03_surface_installed;
            public int harbor_collider_count;
            public int harbor_walkable_surface_count;
            public int harbor_r03_building_collision_count;
        }

        [Serializable]
        private sealed class CharacterControllerData
        {
            public bool enabled;
            public float radius;
            public float height;
            public float skin_width;
            public float[] center = Array.Empty<float>();
        }

        [Serializable]
        private sealed class HarborCollisionData
        {
            public bool initialized;
            public bool r03_surface_installed;
            public int collider_count;
            public int walkable_surface_count;
            public int r03_building_collision_count;
        }

        [Serializable]
        private sealed class BoundsData
        {
            public float[] min = Array.Empty<float>();
            public float[] max = Array.Empty<float>();
            public float[] center = Array.Empty<float>();
            public float[] size = Array.Empty<float>();
        }

        [Serializable]
        private sealed class VisualChild
        {
            public string name;
            public float[] local_position;
            public float[] local_euler;
            public int renderer_count;
        }
    }
}
