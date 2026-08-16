using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using WhalesIslandReef.Presentation.D20.PlayerAnimation;

namespace D20.Tests.A02
{
    public sealed class D20A02PlayerAnimationTests
    {
        private const string ControllerPath = "Assets/Animations/D20/Player/D20_PlayerAnimation.controller";
        private const string PrefabPath = "Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_HumanoidFemale_R01.prefab";
        private const string AvatarPath = "Assets/Art/Models/Characters/D20/Player/D20_PlayerHumanoidAvatar_R01.asset";

        [Test]
        public void ControllerContainsAllFrozenPresentationStatesAndParameters()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            Assert.That(controller, Is.Not.Null, "D20-A02 controller has not been built.");
            var machine = controller.layers[0].stateMachine;
            var required = new[] { "Idle", "Walk", "Run", "Jump", "Fall", "Land", "Interact", "Talk", "Carry", "Fish", "Board", "Disembark", "Helm", "Hit", "Recover" };
            foreach (var state in required)
                Assert.That(FindState(machine, state), Is.Not.Null, state);
            foreach (var state in required)
            {
                var found = FindState(machine, state);
                Assert.That(found.motion, Is.Not.Null, state + " has no motion clip.");
                var clip = found.motion as AnimationClip;
                Assert.That(clip, Is.Not.Null, state + " motion is not a generated AnimationClip.");
                Assert.That(AnimationUtility.GetCurveBindings(clip).Length, Is.GreaterThan(0), state + " has no animation curves.");
            }
            Assert.That(HasParameter(controller, "Speed01", AnimatorControllerParameterType.Float), Is.True);
            Assert.That(HasParameter(controller, "VerticalSpeed", AnimatorControllerParameterType.Float), Is.True);
            Assert.That(HasParameter(controller, "Grounded", AnimatorControllerParameterType.Bool), Is.True);
            Assert.That(HasParameter(controller, "InteractionActive", AnimatorControllerParameterType.Bool), Is.True);
            Assert.That(HasParameter(controller, "ActionHash", AnimatorControllerParameterType.Int), Is.True);
            Assert.That(HasParameter(controller, "Paused", AnimatorControllerParameterType.Bool), Is.True);
        }

        [Test]
        public void MappingCoversLocomotionAirborneInteractionAndRecovery()
        {
            Assert.AreEqual(D20PlayerAnimationState.Idle, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0f }, true));
            Assert.AreEqual(D20PlayerAnimationState.Walk, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0.4f }, true));
            Assert.AreEqual(D20PlayerAnimationState.Run, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0.9f }, true));
            Assert.AreEqual(D20PlayerAnimationState.Jump, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = false, verticalSpeed = 2f }, true));
            Assert.AreEqual(D20PlayerAnimationState.Fall, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = false, verticalSpeed = -2f }, true));
            Assert.AreEqual(D20PlayerAnimationState.Land, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = true }, false));
            Assert.AreEqual(D20PlayerAnimationState.Fish, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = true, actionIntent = "fishing" }, true));
            Assert.AreEqual(D20PlayerAnimationState.Board, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = true, interactionKind = "boarding" }, true));
            Assert.AreEqual(D20PlayerAnimationState.Hit, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = true, hit = true }, true));
            Assert.AreEqual(D20PlayerAnimationState.Recover, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = true, recovering = true }, true));
        }

        [Test]
        public void WorkAvatarDrivesAnimatorWithoutRootMotionOrPhysicalRootChanges()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(avatar, Is.Not.Null);
            Assert.That(avatar.isValid && avatar.isHuman, Is.True);
            Assert.That(controller, Is.Not.Null);

            var instance = Object.Instantiate(prefab);
            var physicalRoot = new GameObject("D20-A02-PhysicalRoot-Test");
            var host = new GameObject("D20-A02-AnimationHost");
            try
            {
                host.transform.SetParent(physicalRoot.transform, false);
                instance.transform.SetParent(host.transform, false);
                var animator = host.AddComponent<Animator>();
                animator.avatar = avatar;
                animator.runtimeAnimatorController = controller;
                var adapter = host.AddComponent<D20PlayerAnimationPresentationAdapter>();
                adapter.ConfigureForWork(animator);
                var rootPosition = physicalRoot.transform.position;
                adapter.ApplyReadOnlySnapshot(new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0.4f, actionIntent = "talk" });
                animator.Update(0.1f);
                Assert.AreEqual(D20PlayerAnimationState.Talk, adapter.CurrentState);
                Assert.IsFalse(animator.applyRootMotion);
                Assert.AreEqual(rootPosition, physicalRoot.transform.position);
            }
            finally
            {
                Object.DestroyImmediate(physicalRoot);
            }
        }

        [Test]
        public void MissingStateFallsBackAndPauseDoesNotMoveRoot()
        {
            var go = new GameObject("D20-A02-Adapter-Test");
            go.transform.position = new Vector3(2f, 3f, 4f);
            try
            {
                var adapter = go.AddComponent<D20PlayerAnimationPresentationAdapter>();
                var position = go.transform.position;
                adapter.ApplyReadOnlySnapshot(new D20PlayerAnimationSnapshot { grounded = true, actionIntent = "helm" });
                Assert.AreEqual(D20PlayerAnimationState.Helm, adapter.CurrentState);
                Assert.IsTrue(adapter.LastApplyUsedFallback);
                adapter.SetPaused(true);
                Assert.IsTrue(adapter.IsPaused);
                Assert.AreEqual(position, go.transform.position);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void PresentationSourceContainsNoNetworkOrStrongLogicWrites()
        {
            var sourcePath = Path.Combine(Application.dataPath, "Scripts/Presentation/D20/PlayerAnimation/D20PlayerAnimationPresentationAdapter.cs");
            var source = File.ReadAllText(sourcePath);
            Assert.That(source, Does.Not.Contain("HttpClient"));
            Assert.That(source, Does.Not.Contain("UnityWebRequest"));
            Assert.That(source, Does.Not.Contain("SaveSystem"));
            Assert.That(source, Does.Not.Contain("EconomySystem"));
            Assert.That(source, Does.Not.Contain("transform.position"));
            Assert.That(source, Does.Not.Contain("transform.localPosition"));
        }

        private static AnimatorState FindState(AnimatorStateMachine machine, string name)
        {
            foreach (var child in machine.states)
                if (child.state.name == name) return child.state;
            return null;
        }

        private static bool HasParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            foreach (var parameter in controller.parameters)
                if (parameter.name == name && parameter.type == type) return true;
            return false;
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
    }
}
