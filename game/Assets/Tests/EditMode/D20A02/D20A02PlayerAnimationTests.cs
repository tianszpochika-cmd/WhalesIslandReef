using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using WhalesIslandReef.Presentation.D20.PlayerAnimation;
using Object = UnityEngine.Object;

namespace D20.Tests.A02
{
    public sealed class D20A02PlayerAnimationTests
    {
        private const string ControllerPath = "Assets/Animations/D20/Player/D20_PlayerAnimation.controller";
        private const string PrefabPath = "Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_HumanoidFemale_R01.prefab";
        private const string AvatarPath = "Assets/Art/Models/Characters/D20/Player/D20_PlayerHumanoidAvatar_R01.asset";
        private const string R02MalePrefabPath = "Assets/Prefabs/Characters/D20/R02/PlayerAppearance/D20_R02_PlayerVisual_Male.prefab";

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
            Assert.AreEqual(D20PlayerAnimationState.Idle, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0.01f }, true));
            Assert.AreEqual(D20PlayerAnimationState.Walk, D20PlayerAnimationStateMap.Resolve(new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0.04f }, true));
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

        /// <summary>
        /// Q03-A19 supersedes the previous candidate Fish contract.
        ///
        /// History: while the D20 action set was generated generic
        /// transform-path motion, the Fish clip had to fake swimming by
        /// pitching the whole visual root 90 degrees and swinging two upper-arm
        /// transform curves inside a hand-tuned 8-degree envelope. That was a
        /// documented placeholder, not an authored stroke.
        ///
        /// The Fish state now carries an admitted authored Humanoid swimming
        /// clip, so the placeholder envelope is replaced by a stronger
        /// contract: real Humanoid muscle curves that drive both arms, both
        /// legs, the spine and the head. The 90-degree root pitch assertion is
        /// deliberately dropped because a real stroke must not be composed by
        /// rotating the whole body onto the water plane.
        /// </summary>
        [Test]
        public void FishStateCarriesAuthoredHumanoidStrokeCoveringBothSidesOfTheBody()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/Animations/D20/Player/Clips/D20_Player_Fish.anim");
            Assert.That(clip, Is.Not.Null, "D20 Fish clip has not been generated.");

            Assert.That(clip.humanMotion, Is.True,
                "The Fish/游泳 state must be authored Humanoid motion, not a generic transform placeholder.");
            Assert.That(clip.hasGenericRootTransform, Is.False,
                "The Fish state must not fall back to a generic root transform clip.");
            Assert.That(clip.length, Is.GreaterThan(1f),
                "The Fish state must carry a real stroke cycle rather than a single static pose.");

            var muscles = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            var armed = 0;
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                Assert.That(string.IsNullOrEmpty(binding.path), Is.True,
                    "A Humanoid swimming clip must bind through the Avatar, not through a transform path: " + binding.path);
                Assert.That(binding.type, Is.EqualTo(typeof(Animator)),
                    "A Humanoid swimming clip must bind to Animator muscle properties.");
                muscles.Add(binding.propertyName);
                if (binding.propertyName.StartsWith("Left Arm", StringComparison.Ordinal) ||
                    binding.propertyName.StartsWith("Right Arm", StringComparison.Ordinal))
                    armed++;
            }

            foreach (var required in new[]
            {
                "Left Arm Down-Up", "Right Arm Down-Up",
                "Left Forearm Stretch", "Right Forearm Stretch",
                "Left Upper Leg Front-Back", "Right Upper Leg Front-Back",
                "Spine Front-Back", "Head Nod Down-Up"
            })
            {
                Assert.That(muscles, Does.Contain(required),
                    "The authored stroke is missing humanoid muscle channel: " + required);
            }

            Assert.That(armed, Is.GreaterThanOrEqualTo(6),
                "Both arms must be driven by the authored stroke; found " + armed + " arm channels.");
        }

        /// <summary>
        /// Q03-A19: the candidate now shares the same authored Humanoid action
        /// set as the formal path, so it keeps the audited Humanoid Avatar
        /// (see D20A02R02CandidateRuntimeBuilder). The previous version of this
        /// test cleared the Avatar to consume generic transform-path clips;
        /// what still matters is that the state machine drives whole-body,
        /// symmetric motion without the Animator touching the D2 physical root.
        /// </summary>
        [Test]
        public void CandidateHumanoidSurfaceDrivesWholeBodyWithoutRootMotion()
        {
            const string candidatePrefabPath = "Assets/Resources/D20/Candidate/D20_R02_PlayerVisual_Male.prefab";
            const string candidateControllerPath = "Assets/Resources/D20/Candidate/D20_PlayerAnimation.controller";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(candidatePrefabPath);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(candidateControllerPath);
            Assert.That(prefab, Is.Not.Null, "Candidate prefab has not been built.");
            Assert.That(controller, Is.Not.Null, "Candidate controller has not been built.");

            var physicalRoot = new GameObject("D20-A19-Candidate-Humanoid-PhysicalRoot");
            var instance = Object.Instantiate(prefab);
            try
            {
                instance.transform.SetParent(physicalRoot.transform, false);
                var animator = instance.GetComponentInChildren<Animator>(true);
                Assert.That(animator, Is.Not.Null);
                Assert.That(animator.avatar, Is.Not.Null,
                    "The candidate must keep its Humanoid Avatar now that the action set is Humanoid motion.");
                Assert.That(animator.avatar.isValid && animator.avatar.isHuman, Is.True);

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                // The runtime bridge sets AlwaysAnimate before evaluating the
                // Humanoid pose (see D20PlayerCandidatePresentationBridge). The
                // default CullUpdateTransforms mode skips skeletal updates for
                // an Animator the renderer tree does not consider visible, which
                // makes bone-level assertions read a frozen pose in batch mode.
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var thighL = FindByName(instance.transform, "thigh_l");
                var upperArmL = FindByName(instance.transform, "upperarm_l");
                var upperArmR = FindByName(instance.transform, "upperarm_r");
                Assert.That(thighL, Is.Not.Null);
                Assert.That(upperArmL, Is.Not.Null);
                Assert.That(upperArmR, Is.Not.Null);

                var rootPosition = physicalRoot.transform.position;
                var restThigh = thighL.localRotation;

                animator.Rebind();
                animator.Play("Idle", 0, 0.5f);
                animator.Update(0f);
                var idleThigh = thighL.localRotation;

                animator.Play("Walk", 0, 0.5f);
                animator.Update(0f);
                var walkThigh = thighL.localRotation;
                var walkArmL = upperArmL.localRotation;
                var walkArmR = upperArmR.localRotation;
                Assert.That(Quaternion.Angle(restThigh, idleThigh), Is.GreaterThan(0.01f),
                    "The Humanoid Idle state must drive the skeleton away from the neutral rest pose.");
                Assert.That(Quaternion.Angle(idleThigh, walkThigh), Is.GreaterThan(0.01f),
                    "Distinct states must produce distinct skeletal poses.");
                Assert.That(Quaternion.Angle(walkArmL, walkArmR), Is.GreaterThan(0.01f),
                    "Both arms must be driven by the authored Humanoid motion.");

                Assert.IsFalse(animator.applyRootMotion);
                Assert.AreEqual(rootPosition, physicalRoot.transform.position,
                    "The candidate Animator must never move the D2 physical root.");
            }
            finally
            {
                Object.DestroyImmediate(physicalRoot);
            }
        }

        /// <summary>
        /// Q03-A19: the authored Humanoid clips now carry the real limb motion,
        /// so the candidate is bound with its Humanoid Avatar instead of being
        /// forced onto the legacy generic-path workaround. This test asserts the
        /// ownership boundary that still matters: the managed adapter drives the
        /// Animator (states, pause, fallback) and never writes bone or root
        /// transforms itself.
        /// </summary>
        [Test]
        public void ManagedAdapterOwnsStateAndPauseWithoutWritingBoneTransforms()
        {
            const string candidatePrefabPath = "Assets/Resources/D20/Candidate/D20_R02_PlayerVisual_Male.prefab";
            const string candidateControllerPath = "Assets/Resources/D20/Candidate/D20_PlayerAnimation.controller";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(candidatePrefabPath);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(candidateControllerPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(controller, Is.Not.Null);

            var physicalRoot = new GameObject("D20-A19-Managed-Adapter-PhysicalRoot");
            var instance = Object.Instantiate(prefab);
            try
            {
                instance.transform.SetParent(physicalRoot.transform, false);
                var animator = instance.GetComponentInChildren<Animator>(true);
                Assert.That(animator, Is.Not.Null);
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.Rebind();
                animator.Play("Idle", 0, 0f);
                animator.Update(0f);

                var adapter = instance.AddComponent<D20PlayerAnimationPresentationAdapter>();
                adapter.ConfigureForWork(animator);
                adapter.ResetSnapshotHistory(true);
                adapter.ApplyReadOnlySnapshot(new D20PlayerAnimationSnapshot
                {
                    grounded = true,
                    speed01 = 0f,
                });
                animator.Update(0f);

                var leftUpperArm = FindByName(instance.transform, "upperarm_l");
                var rightUpperArm = FindByName(instance.transform, "upperarm_r");
                var leftHand = FindByName(instance.transform, "hand_l");
                var rightHand = FindByName(instance.transform, "hand_r");
                Assert.That(leftUpperArm, Is.Not.Null);
                Assert.That(rightUpperArm, Is.Not.Null);
                Assert.That(leftHand, Is.Not.Null);
                Assert.That(rightHand, Is.Not.Null);

                var rootPosition = physicalRoot.transform.position;
                var leftArmRotation = leftUpperArm.localRotation;
                var rightArmRotation = rightUpperArm.localRotation;

                // The state surface is owned by the adapter.
                Assert.AreEqual(D20PlayerAnimationState.Idle, adapter.CurrentState);
                Assert.IsFalse(adapter.LastApplyUsedFallback);

                // Pause must be idempotent and must not disturb the pose.
                adapter.SetPaused(true);
                Assert.IsTrue(adapter.IsPaused);
                Assert.That(leftUpperArm.localRotation, Is.EqualTo(leftArmRotation));
                Assert.That(rightUpperArm.localRotation, Is.EqualTo(rightArmRotation));

                // A resolve with no bound Animator must report a fallback rather
                // than silently writing bones.
                var probe = new GameObject("D20-A19-Adapter-Fallback-Probe");
                try
                {
                    var probeAdapter = probe.AddComponent<D20PlayerAnimationPresentationAdapter>();
                    probeAdapter.ApplyReadOnlySnapshot(new D20PlayerAnimationSnapshot { grounded = true, actionIntent = "helm" });
                    Assert.IsTrue(probeAdapter.LastApplyUsedFallback);
                }
                finally
                {
                    Object.DestroyImmediate(probe);
                }

                Assert.That(leftUpperArm.localRotation, Is.EqualTo(leftArmRotation));
                Assert.That(rightUpperArm.localRotation, Is.EqualTo(rightArmRotation));
                Assert.That(physicalRoot.transform.position, Is.EqualTo(rootPosition));
            }
            finally
            {
                Object.DestroyImmediate(physicalRoot);
            }
        }

        /// <summary>
        /// Q03-A19 replacement for CandidateFishClipAppliesWholeBodyPitchWithoutPhysicalRootMotion.
        ///
        /// The removed assertion required a constant 90-degree visual-root pitch
        /// curve, which was the placeholder mechanism for faking a swim pose.
        /// With an authored Humanoid stroke the whole-body pitch is gone, so
        /// what still must hold is the ownership boundary: the clip may carry
        /// Humanoid root translation/rotation channels, but gameplay must keep
        /// root motion disabled so the D2 physical root stays authoritative.
        /// </summary>
        [Test]
        public void HumanoidClipsKeepRootMotionOwnedByD2NotByTheClip()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/Animations/D20/Player/Clips/D20_Player_Fish.anim");
            Assert.That(clip, Is.Not.Null, "D20 Fish clip has not been generated.");
            Assert.That(clip.humanMotion, Is.True, "Fish clip must be authored Humanoid motion.");

            // A Humanoid clip legitimately carries RootT/RootQ channels. The
            // contract is that consuming them is opt-in through applyRootMotion,
            // which every D20 consumer explicitly disables.
            var hasRootChannels = false;
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                if (binding.propertyName.StartsWith("RootT", StringComparison.Ordinal) ||
                    binding.propertyName.StartsWith("RootQ", StringComparison.Ordinal))
                    hasRootChannels = true;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(avatar, Is.Not.Null);
            Assert.That(controller, Is.Not.Null);

            var physicalRoot = new GameObject("D20-A19-HumanoidRootMotion-Test");
            var host = new GameObject("D20-A19-HumanoidRootMotion-Host");
            var instance = Object.Instantiate(prefab);
            try
            {
                host.transform.SetParent(physicalRoot.transform, false);
                instance.transform.SetParent(host.transform, false);
                var animator = host.AddComponent<Animator>();
                animator.avatar = avatar;
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                var rootPosition = physicalRoot.transform.position;
                var rootRotation = physicalRoot.transform.rotation;
                foreach (var state in new[] { "Idle", "Walk", "Run", "Fish", "Helm" })
                {
                    animator.Rebind();
                    animator.Play(state, 0, 0.5f);
                    animator.Update(0.1f);
                    Assert.IsFalse(animator.applyRootMotion, state);
                    Assert.AreEqual(rootPosition, physicalRoot.transform.position, state + " moved the D2 physical root.");
                    Assert.AreEqual(rootRotation, physicalRoot.transform.rotation, state + " rotated the D2 physical root.");
                }

                Assert.That(hasRootChannels, Is.True,
                    "The admitted Humanoid source is expected to expose RootT/RootQ channels; " +
                    "if this changes, re-audit the root-motion ownership contract.");
            }
            finally
            {
                Object.DestroyImmediate(physicalRoot);
            }
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
        public void R02MaleAppearancePrefabAcceptsD20ControllerWithoutRootMotionOrSocketLoss()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(R02MalePrefabPath);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            Assert.That(prefab, Is.Not.Null, "D20-A01-R02 male appearance prefab is missing.");
            Assert.That(controller, Is.Not.Null, "D20-A02 controller is missing.");

            var physicalRoot = new GameObject("D20-A02-R02-PhysicalRoot-Test");
            var instance = Object.Instantiate(prefab);
            try
            {
                instance.transform.SetParent(physicalRoot.transform, false);
                var animator = instance.GetComponent<Animator>();
                Assert.That(animator, Is.Not.Null, "R02 appearance prefab must own exactly one Animator.");
                Assert.That(instance.GetComponentsInChildren<Animator>(true), Has.Length.EqualTo(1));
                Assert.That(animator.avatar, Is.Not.Null);
                Assert.That(animator.avatar.isValid && animator.avatar.isHuman, Is.True,
                    "R02 appearance prefab must expose a valid Humanoid Avatar.");

                animator.runtimeAnimatorController = controller;
                var adapter = instance.AddComponent<D20PlayerAnimationPresentationAdapter>();
                adapter.ConfigureForWork(animator);

                var rootPosition = physicalRoot.transform.position;
                var requiredSockets = new[]
                {
                    "D20Socket_Head", "D20Socket_Hand_L", "D20Socket_Hand_R",
                    "D20Socket_Helm", "D20Socket_Boarding", "D20Socket_Carry",
                    "D20Socket_Fish", "D20Socket_Foot_L", "D20Socket_Foot_R"
                };
                foreach (var socket in requiredSockets)
                    Assert.That(FindByName(instance.transform, socket), Is.Not.Null, socket);

                var snapshots = new[]
                {
                    new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0f },
                    new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0.4f },
                    new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0.9f },
                    new D20PlayerAnimationSnapshot { grounded = false, verticalSpeed = 2f },
                    new D20PlayerAnimationSnapshot { grounded = true, interactionKind = "boarding" },
                    new D20PlayerAnimationSnapshot { grounded = true, actionIntent = "helm" }
                };
                var expectedStates = new[]
                {
                    D20PlayerAnimationState.Idle,
                    D20PlayerAnimationState.Walk,
                    D20PlayerAnimationState.Run,
                    D20PlayerAnimationState.Jump,
                    D20PlayerAnimationState.Board,
                    D20PlayerAnimationState.Helm
                };

                for (var i = 0; i < snapshots.Length; i++)
                {
                    Assert.AreEqual(expectedStates[i], adapter.ApplyReadOnlySnapshot(snapshots[i]));
                    Assert.IsFalse(adapter.LastApplyUsedFallback, expectedStates[i].ToString());
                    animator.Update(0.1f);
                    Assert.IsFalse(animator.applyRootMotion);
                    Assert.AreEqual(rootPosition, physicalRoot.transform.position);
                }
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
