using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace D20.Tests.A02
{
    /// <summary>
    /// Q03 resource-admission contract only. This test does not promote the
    /// D20 work assets into InitialHarbor; it proves the asset-side facts that
    /// a later formal consumer is allowed to rely on.
    /// </summary>
    public sealed class Q03FormalAvatarActionResourceContractTests
    {
        private const string FormalAppearancePrefabPath =
            "Assets/Prefabs/Characters/D20/R02/PlayerAppearance/D20_R02_PlayerVisual_Male.prefab";
        private const string CandidateAppearancePrefabPath =
            "Assets/Resources/D20/Candidate/D20_R02_PlayerVisual_Male.prefab";
        private const string ControllerPath =
            "Assets/Animations/D20/Player/D20_PlayerAnimation.controller";
        private const string MaleAvatarPath =
            "Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/ProjectOwned/R03/Avatars/D18_R03_A01_PlayerHumanoidMale.asset";

        private static readonly string[] RequiredStates =
        {
            "Idle", "Walk", "Run", "Jump", "Fall", "Land", "Interact", "Talk",
            "Carry", "Fish", "Board", "Disembark", "Helm", "Hit", "Recover"
        };

        [Test]
        public void FormalAppearanceExposesVerifiedHumanoidAvatarAndActionInterface()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FormalAppearancePrefabPath);
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(MaleAvatarPath);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

            Assert.That(prefab, Is.Not.Null, "D20 R02 formal appearance prefab is missing.");
            Assert.That(avatar, Is.Not.Null, "D18 R03 project-owned male Avatar is missing.");
            Assert.That(controller, Is.Not.Null, "D20 Player AnimatorController is missing.");
            Assert.That(avatar.isValid && avatar.isHuman, Is.True,
                "The formal appearance contract requires a valid Humanoid Avatar.");

            var animator = prefab.GetComponent<Animator>();
            Assert.That(animator, Is.Not.Null, "Formal appearance prefab must own an Animator.");
            Assert.That(animator.avatar, Is.Not.Null, "Formal appearance Animator has no Avatar.");
            Assert.That(animator.avatar.isValid && animator.avatar.isHuman, Is.True,
                "Formal appearance Animator must expose a valid Humanoid Avatar.");

            var humanBones = new HashSet<string>(StringComparer.Ordinal);
            foreach (var humanBone in animator.avatar.humanDescription.human)
                humanBones.Add(humanBone.humanName);
            foreach (var required in new[]
            {
                "Hips", "Spine", "Chest", "Head", "LeftUpperArm", "RightUpperArm",
                "LeftLowerArm", "RightLowerArm", "LeftUpperLeg", "RightUpperLeg",
                "LeftLowerLeg", "RightLowerLeg", "LeftFoot", "RightFoot"
            })
            {
                Assert.That(humanBones, Does.Contain(required),
                    "Humanoid mapping is missing " + required + ".");
            }

            var stateMachine = controller.layers[0].stateMachine;
            foreach (var stateName in RequiredStates)
            {
                var state = FindState(stateMachine, stateName);
                Assert.That(state, Is.Not.Null, stateName + " state is missing.");
                Assert.That(state.motion, Is.TypeOf<AnimationClip>(),
                    stateName + " state must use a real AnimationClip.");
                Assert.That(AnimationUtility.GetCurveBindings((AnimationClip)state.motion).Length,
                    Is.GreaterThan(0), stateName + " has no authored curve bindings.");
            }

            Assert.That(HasParameter(controller, "Speed01", AnimatorControllerParameterType.Float), Is.True);
            Assert.That(HasParameter(controller, "VerticalSpeed", AnimatorControllerParameterType.Float), Is.True);
            Assert.That(HasParameter(controller, "Grounded", AnimatorControllerParameterType.Bool), Is.True);
            Assert.That(HasParameter(controller, "InteractionActive", AnimatorControllerParameterType.Bool), Is.True);
            Assert.That(HasParameter(controller, "ActionHash", AnimatorControllerParameterType.Int), Is.True);
            Assert.That(HasParameter(controller, "Paused", AnimatorControllerParameterType.Bool), Is.True);
        }

        /// <summary>
        /// Q03-A19 supersedes the previous generic-path candidate contract.
        /// While the D20 action set was generated generic transform-path
        /// motion, the candidate prefab deliberately cleared its Avatar and the
        /// adapter resolved curves as literal transform paths. The action set is
        /// now authored Humanoid motion, so the candidate must keep its valid
        /// Humanoid Avatar and the adapter must select its Humanoid pose path
        /// (D20PlayerCandidatePresentationBridge.HasGenericPathClip() == false).
        /// </summary>
        [Test]
        public void CandidateAdapterUsesHumanoidRetargetingForTheHumanoidActionSet()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CandidateAppearancePrefabPath);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

            Assert.That(prefab, Is.Not.Null, "D20 candidate appearance prefab is missing.");
            Assert.That(controller, Is.Not.Null, "D20 Player AnimatorController is missing.");

            var animator = prefab.GetComponent<Animator>();
            Assert.That(animator, Is.Not.Null, "Candidate prefab must own the candidate Animator.");
            Assert.That(animator.avatar, Is.Not.Null,
                "The candidate adapter must keep the audited Humanoid Avatar; a Humanoid action set cannot retarget without it.");
            Assert.That(animator.avatar.isValid && animator.avatar.isHuman, Is.True,
                "The candidate adapter requires a valid Humanoid Avatar.");
            Assert.That(prefab.transform.Find("Armature"), Is.Not.Null,
                "Candidate contract requires Armature directly under the Animator root.");

            var genericRootClips = 0;
            var humanMotionClips = 0;
            foreach (var clip in controller.animationClips)
            {
                if (clip == null) continue;
                if (clip.hasGenericRootTransform) genericRootClips++;
                if (clip.humanMotion) humanMotionClips++;
                // A Humanoid muscle clip binds to the Avatar, so it must not
                // carry literal Armature transform paths.
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    Assert.That(string.IsNullOrEmpty(binding.path), Is.True,
                        clip.name + " mixes a Humanoid action set with a generic transform path: " + binding.path);
            }

            Assert.That(genericRootClips, Is.EqualTo(0),
                "The candidate adapter must no longer expose generic-root clips.");
            Assert.That(humanMotionClips, Is.GreaterThanOrEqualTo(RequiredStates.Length),
                "The candidate adapter did not expose the Humanoid action surface.");
            Assert.That(humanMotionClips, Is.EqualTo(controller.animationClips.Length),
                "Every candidate action clip must be Humanoid motion.");
        }

        private static AnimatorState FindState(AnimatorStateMachine machine, string name)
        {
            foreach (var child in machine.states)
                if (string.Equals(child.state.name, name, StringComparison.Ordinal)) return child.state;
            foreach (var childMachine in machine.stateMachines)
            {
                var found = FindState(childMachine.stateMachine, name);
                if (found != null) return found;
            }
            return null;
        }

        private static bool HasParameter(AnimatorController controller, string name,
            AnimatorControllerParameterType type)
        {
            foreach (var parameter in controller.parameters)
                if (parameter.name == name && parameter.type == type) return true;
            return false;
        }
    }
}
