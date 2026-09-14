using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.LocalQuality.Q03
{
    /// <summary>
    /// Q03-A16 covers the opt-in formal Avatar muscle adapter. It proves the
    /// valid Humanoid path can submit bounded presentation poses without
    /// moving the D2 physical root. It does not open the authored-action gate.
    /// </summary>
    public sealed class Q03D20FormalHumanoidPosePlayModeTests
    {
        [UnityTest]
        public IEnumerator FormalHumanoidAdapterUsesAvatarMusclesAndPreservesPhysicalRoot()
        {
            var bridgeType = FindLoadedType("Whalebone.Gameplay.D2.D20PlayerCandidatePresentationBridge");
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bridgeType);
            Assert.IsNotNull(bootstrapType);

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var readyProperty = bootstrapType.GetProperty("IsReady");
            Component bootstrap = null;
            var deadline = Time.realtimeSinceStartup + 10f;
            while ((bootstrap == null || !(bool)readyProperty.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = FindFirstComponent(bootstrapType);
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2RuntimeBootstrap 没有创建。");
            Assert.IsTrue((bool)readyProperty.GetValue(bootstrap), "D2 运行时闭环没有完成。");

            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            // Let the freshly-created CharacterController finish its first
            // grounding step before taking the physical-root baseline.  The
            // pose adapter must be compared against a settled D2 root; using
            // the pre-grounding y=0.15 sample made this ownership regression
            // race the normal D2 spawn correction.
            yield return null;
            yield return null;
            var physicalPosition = player.transform.position;
            var physicalRotation = player.transform.rotation;

            var install = bridgeType.GetMethod("InstallForFormalHumanoid", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(install, "Formal Humanoid consumer 缺少显式 opt-in 安装入口。");
            var bridge = install.Invoke(null, new object[] { player }) as Component;
            Assert.IsNotNull(bridge, "Formal Humanoid consumer 没有创建。");
            Assert.IsTrue((bool)bridgeType.GetProperty("IsInstalled").GetValue(bridge));
            Assert.IsTrue((bool)bridgeType.GetProperty("IsFormalConsumer").GetValue(bridge));
            Assert.IsFalse((bool)bridgeType.GetProperty("UsesGenericPathAdapter").GetValue(bridge),
                "Humanoid route must not consume the generic-path adapter.");
            Assert.IsTrue((bool)bridgeType.GetProperty("UsesHumanoidPoseAdapter").GetValue(bridge));

            var appearance = bridgeType.GetProperty("AppearanceInstance").GetValue(bridge) as GameObject;
            var animator = bridgeType.GetProperty("CandidateAnimator").GetValue(bridge) as Animator;
            Assert.IsNotNull(appearance);
            Assert.IsNotNull(animator);
            Assert.IsNotNull(animator.avatar);
            Assert.IsTrue(animator.avatar.isValid && animator.avatar.isHuman);
            Assert.IsFalse(animator.enabled, "Humanoid pose route must disable generic Animator curve writes.");
            Assert.IsNotNull(appearance.transform.Find("Visual_player_visual_male_r02/Armature/root/pelvis"),
                "Humanoid route must retain the Avatar-authored wrapper root.");

            var humanoidAdapter = bridgeType.GetProperty("HumanoidPoseAdapter").GetValue(bridge) as Component;
            Assert.IsNotNull(humanoidAdapter);
            Assert.IsTrue((bool)humanoidAdapter.GetType().GetProperty("IsReady").GetValue(humanoidAdapter));

            yield return null;
            yield return null;

            Assert.Greater((int)humanoidAdapter.GetType().GetProperty("PoseApplicationCount").GetValue(humanoidAdapter), 0,
                "Humanoid adapter 没有提交任何 pose。");
            Assert.IsTrue((bool)humanoidAdapter.GetType().GetProperty("LastApplySucceeded").GetValue(humanoidAdapter));
            var leftUpperArm = appearance.transform.Find(
                "Visual_player_visual_male_r02/Armature/root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l");
            var leftHand = appearance.transform.Find(
                "Visual_player_visual_male_r02/Armature/root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l");
            Assert.IsNotNull(leftUpperArm);
            Assert.IsNotNull(leftHand);
            Assert.Less(leftHand.position.y, leftUpperArm.position.y,
                "Humanoid Idle pose must leave the imported T-pose arm envelope.");

            var adapter = bridgeType.GetProperty("Adapter").GetValue(bridge) as Component;
            Assert.IsNotNull(adapter);
            var snapshotType = adapter.GetType().GetMethod("ApplyReadOnlySnapshot").GetParameters()[0].ParameterType;
            var snapshot = Activator.CreateInstance(snapshotType);
            snapshotType.GetField("grounded").SetValue(snapshot, true);
            snapshotType.GetField("speed01").SetValue(snapshot, 0f);
            snapshotType.GetField("verticalSpeed").SetValue(snapshot, 0f);
            snapshotType.GetField("interactionKind").SetValue(snapshot, "fish");
            snapshotType.GetField("controlMode").SetValue(snapshot, "Character");
            snapshotType.GetField("paused").SetValue(snapshot, false);
            snapshotType.GetField("lodBand").SetValue(snapshot, 0);
            adapter.GetType().GetMethod("ApplyReadOnlySnapshot").Invoke(adapter, new[] { snapshot });

            var stateType = humanoidAdapter.GetType().Assembly.GetType(
                "WhalesIslandReef.Presentation.D20.PlayerAnimation.D20PlayerAnimationState");
            var fishState = Enum.Parse(stateType, "Fish");
            humanoidAdapter.GetType().GetMethod("ApplySnapshot").Invoke(humanoidAdapter, new[] { snapshot, fishState });

            Assert.AreEqual("Fish", humanoidAdapter.GetType().GetProperty("CurrentState").GetValue(humanoidAdapter).ToString());
            Assert.AreEqual(70f, (float)humanoidAdapter.GetType().GetProperty("LastBodyPitchDegrees").GetValue(humanoidAdapter), 0.01f);
            Assert.AreEqual(physicalPosition, player.transform.position,
                "Humanoid pose submission must not move the D2 physical root.");
            Assert.AreEqual(physicalRotation, player.transform.rotation,
                "Humanoid pose submission must not rotate the D2 physical root.");
        }

        private static Type FindLoadedType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }

            return null;
        }

        private static Component FindFirstComponent(Type type)
        {
            foreach (var component in UnityEngine.Object.FindObjectsByType<Component>(FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (component != null && type.IsInstanceOfType(component)) return component;
            }

            return null;
        }
    }
}
