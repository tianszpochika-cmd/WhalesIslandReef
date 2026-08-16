using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhalesIslandReef.Presentation.D20.NpcPresentation;

namespace WhalesIslandReef.Tests.D20A04
{
    public sealed class D20A04NpcPresentationTests
    {
        private const string DemoPrefabPath = "Assets/Prefabs/Characters/D20/NPCPresentation/D20_NPC_Presentation_Demo.prefab";

        [Test]
        public void StateMapCoversNineRequiredBehaviorStates()
        {
            var states = new[]
            {
                D20NpcBehaviorState.Idle, D20NpcBehaviorState.Work, D20NpcBehaviorState.Walk,
                D20NpcBehaviorState.Talk, D20NpcBehaviorState.Rest, D20NpcBehaviorState.Eat,
                D20NpcBehaviorState.Alarm, D20NpcBehaviorState.Refuse, D20NpcBehaviorState.Injured
            };

            foreach (var state in states)
            {
                var snapshot = D20NpcPresentationSnapshot.At("npc-test", "resource-test", state, D20NpcJobPost.Dock, Vector3.zero);
                Assert.That(D20NpcPresentationStateMap.ToVisualAction(snapshot), Is.Not.EqualTo(D20NpcVisualAction.NeutralFallback), state.ToString());
                Assert.That(D20NpcPresentationStateMap.AnimatorStateName(D20NpcPresentationStateMap.ToVisualAction(snapshot)), Does.StartWith("D20_NPC_"));
            }
        }

        [Test]
        public void DialogueNavigationAndHealthFailuresHaveExplicitFeedback()
        {
            var open = D20NpcPresentationSnapshot.At("npc-test", "resource-test", D20NpcBehaviorState.Talk, D20NpcJobPost.Market, Vector3.zero);
            open.dialogueOpen = true;
            open.dialogueStatus = D20NpcDialogueStatus.Open;
            Assert.That(D20NpcPresentationStateMap.ToFeedback(open, false).feedbackKey, Is.EqualTo("npc_feedback_dialogue_open"));

            var ended = open;
            ended.dialogueOpen = false;
            ended.dialogueStatus = D20NpcDialogueStatus.Completed;
            Assert.That(D20NpcPresentationStateMap.ToFeedback(ended, false).feedbackKey, Is.EqualTo("npc_feedback_dialogue_ended"));

            var busy = open;
            busy.busy = true;
            busy.dialogueStatus = D20NpcDialogueStatus.Busy;
            Assert.That(D20NpcPresentationStateMap.ToFeedback(busy, false).feedbackKey, Is.EqualTo("npc_feedback_busy"));

            var unreachable = open;
            unreachable.dialogueStatus = D20NpcDialogueStatus.Unreachable;
            unreachable.navigationStatus = D20NpcNavigationStatus.NoAnchor;
            Assert.That(D20NpcPresentationStateMap.ToFeedback(unreachable, false).feedbackKey, Is.EqualTo("npc_feedback_unreachable"));

            var refused = open;
            refused.dialogueStatus = D20NpcDialogueStatus.Refused;
            refused.behaviorState = D20NpcBehaviorState.Refuse;
            Assert.That(D20NpcPresentationStateMap.ToFeedback(refused, false).feedbackKey, Is.EqualTo("npc_feedback_refused"));

            var injured = open;
            injured.dialogueOpen = false;
            injured.dialogueStatus = D20NpcDialogueStatus.None;
            injured.injured = true;
            injured.behaviorState = D20NpcBehaviorState.Injured;
            Assert.That(D20NpcPresentationStateMap.ToFeedback(injured, false).feedbackKey, Is.EqualTo("npc_feedback_injured"));
        }

        [Test]
        public void AdapterUsesNeutralFallbackWhenActionClipIsMissing()
        {
            var root = new GameObject("D20A04_TestNpc");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.AddComponent<MeshFilter>();
            visual.AddComponent<MeshRenderer>();
            var adapter = root.AddComponent<D20NpcReadOnlyPresentationAdapter>();
            adapter.ConfigureForEditor("npc-test", "resource-test", D20NpcJobPost.Dock, null, null);
            adapter.CachePresentationParts();

            var snapshot = D20NpcPresentationSnapshot.At("npc-test", "resource-test", D20NpcBehaviorState.Work, D20NpcJobPost.Dock, new Vector3(4f, 0f, 6f));
            bool accepted = adapter.ApplyReadOnlySnapshot(snapshot);

            Assert.That(accepted, Is.True);
            Assert.That(adapter.HasSnapshot, Is.True);
            Assert.That(adapter.LastSnapshot.worldPosition, Is.EqualTo(new Vector3(4f, 0f, 6f)));
            Assert.That(adapter.LastFeedback.feedbackKey, Is.EqualTo("npc_feedback_working_dock"));
            Assert.That(adapter.LastFeedback.fallbackUsed, Is.True);
            Assert.That(adapter.CurrentVisualAction, Is.EqualTo(D20NpcVisualAction.NeutralFallback));
            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void FourJobVisualAnchorsAreStableAndReadOnly()
        {
            var root = new GameObject("D20A04_AnchorTest");
            foreach (var jobPost in Enum.GetValues(typeof(D20NpcJobPost)).Cast<D20NpcJobPost>())
            {
                var child = new GameObject(jobPost.ToString());
                child.transform.SetParent(root.transform, false);
                var anchor = child.AddComponent<D20NpcPresentationAnchor>();
                anchor.ConfigureForEditor(D20NpcPresentationAnchorIds.ForJob(jobPost), jobPost, "npc_interaction_" + jobPost.ToString().ToLowerInvariant());
            }

            var adapter = root.AddComponent<D20NpcReadOnlyPresentationAdapter>();
            adapter.ConfigureForEditor("npc-test", "resource-test", D20NpcJobPost.Market, null, null);
            foreach (var jobPost in Enum.GetValues(typeof(D20NpcJobPost)).Cast<D20NpcJobPost>())
            {
                Assert.That(adapter.TryGetVisualAnchor(jobPost, out var anchor), Is.True, jobPost.ToString());
                Assert.That(anchor.AnchorId, Is.EqualTo(D20NpcPresentationAnchorIds.ForJob(jobPost)));
            }

            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void LodPauseAndReloadKeepSnapshotWithoutWritingLogic()
        {
            var root = new GameObject("D20A04_LodTest");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.AddComponent<MeshFilter>();
            visual.AddComponent<MeshRenderer>();
            var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallback.name = "FarProxy";
            fallback.transform.SetParent(root.transform, false);
            UnityEngine.Object.DestroyImmediate(fallback.GetComponent<Collider>());
            var adapter = root.AddComponent<D20NpcReadOnlyPresentationAdapter>();
            adapter.ConfigureForEditor("npc-test", "resource-test", D20NpcJobPost.Tavern, fallback, null);
            adapter.CachePresentationParts();

            var snapshot = D20NpcPresentationSnapshot.At("npc-test", "resource-test", D20NpcBehaviorState.Rest, D20NpcJobPost.Tavern, new Vector3(1f, 0f, 2f));
            adapter.ApplyReadOnlySnapshot(snapshot);
            adapter.SetLodBand(D20NpcLodBand.Far);
            Assert.That(adapter.CurrentLodBand, Is.EqualTo(D20NpcLodBand.Far));
            Assert.That(adapter.IsFallbackVisible, Is.True);

            snapshot.paused = true;
            adapter.ApplyReadOnlySnapshot(snapshot);
            Assert.That(adapter.IsPaused, Is.True);
            Assert.That(adapter.LastSnapshot.worldPosition, Is.EqualTo(new Vector3(1f, 0f, 2f)));

            adapter.NotifyPresentationSuspended();
            Assert.That(adapter.NeedsSnapshotResync, Is.True);
            Assert.That(adapter.NotifyPresentationResumed(), Is.True);
            Assert.That(adapter.HasSnapshot, Is.True);
            Assert.That(adapter.NeedsSnapshotResync, Is.False);
            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void DemoPrefabUsesA03ReadOnlySourceAndFourAnchors()
        {
            var demo = AssetDatabase.LoadAssetAtPath<GameObject>(DemoPrefabPath);
            Assert.That(demo, Is.Not.Null, DemoPrefabPath);
            var root = PrefabUtility.LoadPrefabContents(DemoPrefabPath);
            try
            {
                Assert.That(root.GetComponent<D20NpcReadOnlyPresentationAdapter>(), Is.Not.Null);
                Assert.That(root.GetComponentsInChildren<D20NpcPresentationAnchor>(true).Length, Is.EqualTo(4));
                Assert.That(root.GetComponentInChildren<D20NpcFeedbackBillboard>(true), Is.Not.Null);
                Assert.That(root.transform.Find("FarProxy"), Is.Not.Null);
                Assert.That(root.transform.Find("A03_SourceNpcVisual_ReadOnlyInstance"), Is.Not.Null);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [Test]
        public void PresentationSourceContainsNoStrongLogicOrNetworkWriter()
        {
            string gameRoot = Directory.GetParent(Application.dataPath).FullName;
            string sourceRoot = Path.Combine(gameRoot, "Assets/Scripts/Presentation/D20/NpcPresentation");
            string[] banned =
            {
                "EventLog", "EconomySystem", "InventorySystem", "RelationSystem", "SaveSystem",
                "NPCSystem", "AgentCore", "LLMClient", "UnityWebRequest", "HttpClient"
            };
            foreach (var file in Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
            {
                string text = File.ReadAllText(file);
                foreach (var token in banned)
                    StringAssert.DoesNotContain(token, text, file + " contains forbidden writer/network token " + token);
            }
        }
    }
}
