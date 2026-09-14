using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace WhalesIslandReef.Editor.Q03A19
{
    /// <summary>
    /// Q03-A19 rebuilds the D20 Player action set as a real authored Humanoid
    /// action surface. It replaces the previous generated generic
    /// transform-path clips with curve-identical derivatives of the admitted
    /// CC0 Universal Animation Library (Quaternius) Humanoid clips.
    ///
    /// Ownership: this builder owns only the D20 animation assets
    /// (Assets/Animations/D20/Player). It never edits the R02 appearance
    /// prefab, InitialHarbor, a gameplay component or a Systems class.
    ///
    /// Why curve copying rather than an AnimatorOverrideController: the D20
    /// resource contract is expressed against AnimatorController state motions
    /// that must expose authored curve bindings. Copying the source Humanoid
    /// bindings preserves that contract while also setting humanMotion, which
    /// is a read-only property that cannot be authored by hand.
    /// </summary>
    public static class Q03A19FormalActionBuilder
    {
        public const string SourceLibraryPath =
            "Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/" +
            "QuaterniusUniversalBaseCharacters/IncomingAnimations/UAL1_Standard.fbx";

        private const string RootPath = "Assets/Animations/D20/Player";
        private const string ClipPath = RootPath + "/Clips";
        private const string ControllerPath = RootPath + "/D20_PlayerAnimation.controller";
        private const string ResultRoot = "tools/results/LOCAL-QUALITY/Q03-A19";

        /// <summary>
        /// D20 state -> admitted Humanoid source clip. Every source clip is a
        /// real authored Humanoid motion clip; no generated filler remains.
        /// </summary>
        [Serializable]
        public sealed class StateMapping
        {
            public string state;
            public string source;
        }

        private static readonly StateMapping[] StateMap =
        {
            new StateMapping { state = "Idle",      source = "Idle_Loop" },
            new StateMapping { state = "Walk",      source = "Walk_Loop" },
            new StateMapping { state = "Run",       source = "Jog_Fwd_Loop" },
            new StateMapping { state = "Jump",      source = "Jump_Start" },
            new StateMapping { state = "Fall",      source = "Jump_Loop" },
            new StateMapping { state = "Land",      source = "Jump_Land" },
            new StateMapping { state = "Interact",  source = "Interact" },
            new StateMapping { state = "Talk",      source = "Idle_Talking_Loop" },
            new StateMapping { state = "Carry",     source = "Push_Loop" },
            // Fish is the D20 swimming state: D20PlayerCandidatePresentationBridge
            // reports SwimmingVisualSupported from HasAnimatorState("Fish").
            new StateMapping { state = "Fish",      source = "Swim_Fwd_Loop" },
            new StateMapping { state = "Board",     source = "Sitting_Enter" },
            new StateMapping { state = "Disembark", source = "Sitting_Exit" },
            new StateMapping { state = "Helm",      source = "Driving_Loop" },
            new StateMapping { state = "Hit",       source = "Hit_Chest" },
            new StateMapping { state = "Recover",   source = "Roll" },
        };

        private static readonly string[] RequiredStates =
        {
            "Idle", "Walk", "Run", "Jump", "Fall", "Land", "Interact", "Talk",
            "Carry", "Fish", "Board", "Disembark", "Helm", "Hit", "Recover"
        };

        private static readonly string[] LoopStates =
        {
            "Idle", "Walk", "Run", "Fall", "Interact", "Talk", "Carry", "Fish", "Helm"
        };

        [MenuItem("Tools/WhalesIslandReef/Q03-A19/Build Formal Humanoid Action Set")]
        public static void BuildFromMenu() => Build();

        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var source = LoadSourceLibrary();
            var report = new BuildReport
            {
                task = "Q03-A19",
                source_library = SourceLibraryPath,
                status = "BLOCKED",
                states = new List<StateReport>(),
                errors = new List<string>(),
                warnings = new List<string>(),
            };

            EnsureFolder("Assets/Animations", "D20");
            EnsureFolder("Assets/Animations/D20", "Player");
            EnsureFolder(RootPath, "Clips");

            var clips = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
            foreach (var mapping in StateMap)
            {
                var stateName = mapping.state;
                var sourceName = "Armature|" + mapping.source;
                var sourceClip = FindClip(source, sourceName);
                if (sourceClip == null)
                {
                    var message = "Admitted source clip is missing: " + sourceName;
                    report.errors.Add(message);
                    throw new InvalidOperationException(message);
                }

                if (!sourceClip.humanMotion)
                {
                    var message = "Admitted source clip is not Humanoid motion: " + sourceName;
                    report.errors.Add(message);
                    throw new InvalidOperationException(message);
                }

                var clipPath = ClipPath + "/D20_Player_" + stateName + ".anim";
                DeleteIfExists(clipPath);

                var clip = BuildDerivedClip(stateName, sourceClip);
                AssetDatabase.CreateAsset(clip, clipPath);
                clips.Add(stateName, clip);

                report.states.Add(new StateReport
                {
                    state = stateName,
                    source_clip = mapping.source,
                    source_length_s = Round(sourceClip.length),
                    derived_length_s = Round(clip.length),
                    curve_count = AnimationUtility.GetCurveBindings(clip).Length,
                    human_motion = clip.humanMotion,
                    generic_root = clip.hasGenericRootTransform,
                    loop = Array.IndexOf(LoopStates, stateName) >= 0,
                });
            }

            AssetDatabase.SaveAssets();

            var controller = RebuildController(clips);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            report.controller_path = "game/" + ControllerPath;
            report.controller_state_count = controller.layers[0].stateMachine.states.Length;
            report.controller_transition_count = controller.layers[0].stateMachine.anyStateTransitions.Length;
            report.clip_count = controller.animationClips.Length;

            var humanMotionCount = 0;
            var genericRootCount = 0;
            foreach (var clip in controller.animationClips)
            {
                if (clip == null) continue;
                if (clip.humanMotion) humanMotionCount++;
                if (clip.hasGenericRootTransform) genericRootCount++;
            }

            report.human_motion_clip_count = humanMotionCount;
            report.generic_root_clip_count = genericRootCount;

            if (humanMotionCount != report.clip_count)
                report.errors.Add("Not every controller clip is Humanoid motion: humanMotion=" +
                                  humanMotionCount + "/" + report.clip_count);
            if (genericRootCount != 0)
                report.errors.Add("A generic-root clip survived the rebuild: " + genericRootCount);

            report.status = report.errors.Count == 0 ? "PASS" : "FAIL";
            report.warnings.Add("Pistol_* and Spell_* clips from the source library are deliberately not admitted (1675 Caribbean realism).");
            report.warnings.Add("The D20 Avatar maps 20 human bones while the source library rig exposes 52;末端 finger/toe muscles are retargeted as far as the D20 Avatar allows.");
            WriteResult("Q03_A19_FORMAL_ACTION_BUILD.json", JsonUtility.ToJson(report, true));

            if (report.errors.Count > 0)
                throw new InvalidOperationException("Q03-A19 build failed: " + string.Join("; ", report.errors));

            Debug.Log("[Q03-A19] Formal Humanoid action set built: clips=" + report.clip_count +
                      "; humanMotion=" + humanMotionCount + "; genericRoot=" + genericRootCount);
        }

        private static AnimationClip[] LoadSourceLibrary()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(SourceLibraryPath);
            if (assets == null || assets.Length == 0)
                throw new InvalidOperationException("Admitted action library is missing: " + SourceLibraryPath);

            var clips = new List<AnimationClip>();
            foreach (var asset in assets)
            {
                var clip = asset as AnimationClip;
                if (clip == null) continue;
                if (clip.name.StartsWith("__preview__", StringComparison.Ordinal)) continue;
                clips.Add(clip);
            }

            if (clips.Count == 0)
                throw new InvalidOperationException("Admitted action library exposes no clips: " + SourceLibraryPath);
            return clips.ToArray();
        }

        private static AnimationClip FindClip(AnimationClip[] clips, string name)
        {
            foreach (var clip in clips)
                if (string.Equals(clip.name, name, StringComparison.Ordinal)) return clip;
            return null;
        }

        /// <summary>
        /// Produces a curve-identical derivative of an admitted Humanoid clip.
        /// Copying the source bindings is what makes the result carry
        /// humanMotion; a clip authored by hand cannot set that flag because it
        /// is a read-only Unity property.
        /// </summary>
        private static AnimationClip BuildDerivedClip(string stateName, AnimationClip source)
        {
            var clip = new AnimationClip
            {
                name = "D20_Player_" + stateName,
                frameRate = source.frameRate > 0f ? source.frameRate : 30f,
                legacy = false,
            };

            var copied = 0;
            foreach (var binding in AnimationUtility.GetCurveBindings(source))
            {
                var curve = AnimationUtility.GetEditorCurve(source, binding);
                if (curve == null) continue;
                AnimationUtility.SetEditorCurve(clip, binding, curve);
                copied++;
            }

            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(source))
            {
                var keys = AnimationUtility.GetObjectReferenceCurve(source, binding);
                if (keys == null || keys.Length == 0) continue;
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
                copied++;
            }

            if (copied == 0)
                throw new InvalidOperationException("Source clip exposed no curves: " + source.name);

            var settings = AnimationUtility.GetAnimationClipSettings(source);
            settings.loopTime = Array.IndexOf(LoopStates, stateName) >= 0;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        private static AnimatorController RebuildController(Dictionary<string, AnimationClip> clips)
        {
            DeleteIfExists(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.name = "D20_PlayerAnimation_R02";

            controller.AddParameter("Speed01", AnimatorControllerParameterType.Float);
            controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("InteractionActive", AnimatorControllerParameterType.Bool);
            controller.AddParameter("ActionHash", AnimatorControllerParameterType.Int);
            controller.AddParameter("Paused", AnimatorControllerParameterType.Bool);
            // Q03-A19: the swimming owner now has a real authored clip, so the
            // controller exposes an explicit swimming parameter instead of
            // overloading the fishing/interaction hash.
            controller.AddParameter("Swimming", AnimatorControllerParameterType.Bool);

            var machine = controller.layers[0].stateMachine;
            AnimatorState idle = null;
            foreach (var mapping in StateMap)
            {
                var stateName = mapping.state;
                if (!clips.ContainsKey(stateName))
                    throw new InvalidOperationException("Derived clip is missing for state: " + stateName);
                var state = machine.AddState(stateName);
                state.motion = clips[stateName];
                // Q03-A19: these are complete authored Humanoid clips, so the
                // controller restores default values on state change instead of
                // carrying over stale axes from the removed generic set.
                state.writeDefaultValues = true;
                if (string.Equals(stateName, "Idle", StringComparison.Ordinal)) idle = state;
            }

            foreach (var required in RequiredStates)
            {
                if (FindState(machine, required) == null)
                    throw new InvalidOperationException("Controller lost required state: " + required);
            }

            if (idle != null) machine.defaultState = idle;

            AddTransitions(machine);
            return controller;
        }

        private static AnimatorState FindState(AnimatorStateMachine machine, string name)
        {
            foreach (var child in machine.states)
                if (string.Equals(child.state.name, name, StringComparison.Ordinal)) return child.state;
            return null;
        }

        private static void Link(AnimatorState from, AnimatorState to, string parameter,
            AnimatorConditionMode mode, float threshold, float duration, bool hasExitTime)
        {
            if (from == null || to == null) return;
            var transition = from.AddTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.exitTime = hasExitTime ? 0.9f : 0f;
            transition.duration = duration;
            transition.hasFixedDuration = true;
            if (!string.IsNullOrEmpty(parameter))
                transition.AddCondition(mode, threshold, parameter);
        }

        /// <summary>
        /// Adds the authored transition graph. The previous controller had zero
        /// transitions, which forced every consumer to drive states directly.
        /// The graph below keeps that direct-drive option while making the
        /// locomotion, airborne and interaction paths explicit.
        /// </summary>
        private static void AddTransitions(AnimatorStateMachine machine)
        {
            var idle = FindState(machine, "Idle");
            var walk = FindState(machine, "Walk");
            var run = FindState(machine, "Run");
            var jump = FindState(machine, "Jump");
            var fall = FindState(machine, "Fall");
            var land = FindState(machine, "Land");
            var fish = FindState(machine, "Fish");
            var interact = FindState(machine, "Interact");

            // Locomotion blend driven by the existing speed contract.
            Link(idle, walk, "Speed01", AnimatorConditionMode.Greater, 0.1f, 0.15f, false);
            Link(walk, run, "Speed01", AnimatorConditionMode.Greater, 0.65f, 0.12f, false);
            Link(run, walk, "Speed01", AnimatorConditionMode.Less, 0.60f, 0.12f, false);
            Link(walk, idle, "Speed01", AnimatorConditionMode.Less, 0.1f, 0.15f, false);

            // Airborne chain driven by the existing VerticalSpeed/Grounded pair.
            Link(idle, jump, "Grounded", AnimatorConditionMode.IfNot, 0f, 0.08f, false);
            Link(walk, jump, "Grounded", AnimatorConditionMode.IfNot, 0f, 0.08f, false);
            Link(run, jump, "Grounded", AnimatorConditionMode.IfNot, 0f, 0.08f, false);
            Link(jump, fall, "VerticalSpeed", AnimatorConditionMode.Less, 0f, 0.1f, false);
            Link(fall, land, "Grounded", AnimatorConditionMode.If, 0f, 0.08f, false);
            Link(land, idle, null, AnimatorConditionMode.If, 0f, 0.12f, true);

            // Swimming is owned by the D2 water state, not by the fishing hash.
            Link(idle, fish, "Swimming", AnimatorConditionMode.If, 0f, 0.15f, false);
            Link(walk, fish, "Swimming", AnimatorConditionMode.If, 0f, 0.15f, false);
            Link(run, fish, "Swimming", AnimatorConditionMode.If, 0f, 0.15f, false);
            Link(jump, fish, "Swimming", AnimatorConditionMode.If, 0f, 0.15f, false);
            Link(fall, fish, "Swimming", AnimatorConditionMode.If, 0f, 0.15f, false);
            Link(fish, idle, "Swimming", AnimatorConditionMode.IfNot, 0f, 0.15f, false);

            // Interaction entry/exit keeps the existing InteractionActive owner.
            Link(idle, interact, "InteractionActive", AnimatorConditionMode.If, 0f, 0.12f, false);
            Link(walk, interact, "InteractionActive", AnimatorConditionMode.If, 0f, 0.12f, false);
            Link(interact, idle, "InteractionActive", AnimatorConditionMode.IfNot, 0f, 0.12f, false);
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (string.IsNullOrEmpty(child)) return;
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static void DeleteIfExists(string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) AssetDatabase.DeleteAsset(path);
        }

        private static float Round(float value) => Mathf.Round(value * 1000f) / 1000f;

        private static void WriteResult(string fileName, string json)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var resultRoot = Path.Combine(projectRoot, ResultRoot);
            Directory.CreateDirectory(resultRoot);
            File.WriteAllText(Path.Combine(resultRoot, fileName), json + "\n");
        }

        [Serializable]
        private sealed class StateReport
        {
            public string state;
            public string source_clip;
            public float source_length_s;
            public float derived_length_s;
            public int curve_count;
            public bool human_motion;
            public bool generic_root;
            public bool loop;
        }

        [Serializable]
        private sealed class BuildReport
        {
            public string task;
            public string status;
            public string source_library;
            public string controller_path;
            public int clip_count;
            public int controller_state_count;
            public int controller_transition_count;
            public int human_motion_clip_count;
            public int generic_root_clip_count;
            public List<StateReport> states;
            public List<string> errors;
            public List<string> warnings;
        }
    }
}
