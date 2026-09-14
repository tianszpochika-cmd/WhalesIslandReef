using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace D20.Tests.A02
{
    /// <summary>
    /// Q03 formal action admission gate.
    ///
    /// History: Q03-A15 wrote this gate as an intentionally-passing guard while
    /// the D20 controller only exposed generated generic transform-path clips
    /// (clips=15; humanMotion=0; genericRoot=15). That guard asserted
    /// humanMotionCount == 0 precisely so a generic-only set could never be
    /// mistaken for an admitted action surface.
    ///
    /// Q03-A19 replaced that generic set with curve-identical derivatives of an
    /// admitted CC0 Humanoid source library, so the same gate is now inverted
    /// and strengthened: it requires a fully Humanoid action surface and fails
    /// if any generic-root clip or missing state is reintroduced. The gate is
    /// only satisfied by real Humanoid motion, never by relabelling.
    /// </summary>
    public sealed class Q03FormalActionAdmissionGateTests
    {
        private const string ControllerPath =
            "Assets/Animations/D20/Player/D20_PlayerAnimation.controller";

        private const string SourceLibraryPath =
            "Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/" +
            "QuaterniusUniversalBaseCharacters/IncomingAnimations/UAL1_Standard.fbx";

        private static readonly string[] RequiredStates =
        {
            "Idle", "Walk", "Run", "Jump", "Fall", "Land", "Interact", "Talk",
            "Carry", "Fish", "Board", "Disembark", "Helm", "Hit", "Recover"
        };

        [Test]
        public void FormalActionAdmissionRequiresAHumanoidActionSurface()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            Assert.That(controller, Is.Not.Null, "D20 Player AnimatorController is missing.");

            var clipCount = 0;
            var humanMotionCount = 0;
            var genericRootCount = 0;
            var genericRootNames = string.Empty;
            var missingHumanMotion = string.Empty;
            foreach (var clip in controller.animationClips)
            {
                if (clip == null) continue;
                clipCount++;
                if (clip.humanMotion) humanMotionCount++;
                else
                {
                    if (missingHumanMotion.Length > 0) missingHumanMotion += ",";
                    missingHumanMotion += clip.name;
                }

                if (clip.hasGenericRootTransform)
                {
                    genericRootCount++;
                    if (genericRootNames.Length > 0) genericRootNames += ",";
                    genericRootNames += clip.name;
                }
            }

            Debug.Log("[Q03/A19] formalActionAdmission=HUMANOID; clips=" + clipCount +
                      "; humanMotion=" + humanMotionCount +
                      "; genericRoot=" + genericRootCount +
                      "; nonHumanoidClipNames=" + missingHumanMotion +
                      "; genericRootClipNames=" + genericRootNames);

            Assert.That(clipCount, Is.GreaterThanOrEqualTo(RequiredStates.Length),
                "The formal controller no longer exposes the audited action surface.");
            Assert.That(humanMotionCount, Is.EqualTo(clipCount),
                "Every admitted action clip must be real Humanoid motion. Non-Humanoid clips: " + missingHumanMotion);
            Assert.That(genericRootCount, Is.EqualTo(0),
                "A generic transform-path clip was reintroduced into the formal action surface: " + genericRootNames);

            var stateMachine = controller.layers[0].stateMachine;
            foreach (var stateName in RequiredStates)
            {
                var state = FindState(stateMachine, stateName);
                Assert.That(state, Is.Not.Null, "Formal action surface lost state: " + stateName);
                Assert.That(state.motion, Is.TypeOf<AnimationClip>(),
                    stateName + " state must use a real AnimationClip.");
                var clip = (AnimationClip)state.motion;
                Assert.That(clip.humanMotion, Is.True,
                    stateName + " state clip is not Humanoid motion.");
                Assert.That(AnimationUtility.GetCurveBindings(clip).Length, Is.GreaterThan(0),
                    stateName + " has no authored curve bindings.");
            }
        }

        [Test]
        public void FormalActionSurfaceIsDerivedFromTheAdmittedSourceLibrary()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            Assert.That(controller, Is.Not.Null, "D20 Player AnimatorController is missing.");

            var assets = AssetDatabase.LoadAllAssetsAtPath(SourceLibraryPath);
            Assert.That(assets, Is.Not.Null.And.Length.GreaterThan(0),
                "The admitted CC0 Humanoid action library is missing: " + SourceLibraryPath);

            var sourceClips = new HashSet<string>(StringComparer.Ordinal);
            foreach (var asset in assets)
            {
                var clip = asset as AnimationClip;
                if (clip == null) continue;
                if (clip.name.StartsWith("__preview__", StringComparison.Ordinal)) continue;
                sourceClips.Add(clip.name);
            }

            Assert.That(sourceClips.Count, Is.GreaterThanOrEqualTo(40),
                "The admitted library no longer exposes the audited clip inventory.");

            // The swimming owner is what the reported "ocean cannot swim" defect
            // depends on, so its source clip is asserted explicitly.
            Assert.That(sourceClips, Does.Contain("Armature|Swim_Fwd_Loop"),
                "The admitted library lost the authored swimming clip.");
        }

        [Test]
        public void FormalActionSurfaceExposesAuthoredTransitions()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            Assert.That(controller, Is.Not.Null, "D20 Player AnimatorController is missing.");

            var stateMachine = controller.layers[0].stateMachine;
            var transitionCount = 0;
            foreach (var child in stateMachine.states)
                transitionCount += child.state.transitions.Length;

            Debug.Log("[Q03/A19] formalActionTransitions=" + transitionCount);

            Assert.That(transitionCount, Is.GreaterThan(0),
                "The formal action surface must expose an authored transition graph, " +
                "not the previous zero-transition direct-drive-only controller.");

            var hasSwimmingParameter = false;
            foreach (var parameter in controller.parameters)
                if (string.Equals(parameter.name, "Swimming", StringComparison.Ordinal) &&
                    parameter.type == AnimatorControllerParameterType.Bool)
                    hasSwimmingParameter = true;
            Assert.That(hasSwimmingParameter, Is.True,
                "The controller must expose an explicit Swimming bool parameter.");
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
    }
}
