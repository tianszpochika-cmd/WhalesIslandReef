using System;
using UnityEngine;

namespace WhalesIslandReef.Presentation.D20.NpcPresentation
{
    public enum D20NpcBehaviorState
    {
        Idle,
        Work,
        Walk,
        Talk,
        Rest,
        Eat,
        Alarm,
        Refuse,
        Injured,
        Busy,
        Unreachable,
        Paused
    }

    public enum D20NpcJobPost
    {
        Market,
        Dock,
        Shipyard,
        Tavern
    }

    public enum D20NpcNavigationStatus
    {
        NotRequested,
        Moving,
        Arrived,
        Unreachable,
        NoAnchor,
        SceneUnloaded
    }

    public enum D20NpcDialogueStatus
    {
        None,
        Opening,
        Open,
        Ending,
        Busy,
        Unreachable,
        Refused,
        Completed
    }

    public enum D20NpcLodBand
    {
        Near = 0,
        Mid = 1,
        Far = 2,
        Proxy = 3
    }

    public enum D20NpcVisualAction
    {
        Idle,
        Work,
        Walk,
        Talk,
        Rest,
        Eat,
        Alarm,
        Refuse,
        Injured,
        NeutralFallback
    }

    public enum D20NpcFeedbackSeverity
    {
        None,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Read-only data supplied by NPC/Crew/Dialogue/Schedule owners.
    /// It intentionally has no wage, inventory, relation, quest or time-write fields.
    /// </summary>
    [Serializable]
    public struct D20NpcPresentationSnapshot
    {
        public string npcId;
        public string resourceKey;
        public D20NpcBehaviorState behaviorState;
        public D20NpcJobPost jobPost;
        public Vector3 worldPosition;
        [Range(0f, 1f)] public float speed01;
        public bool grounded;
        public bool dialogueOpen;
        public bool busy;
        public bool injured;
        public bool paused;
        public D20NpcNavigationStatus navigationStatus;
        public D20NpcDialogueStatus dialogueStatus;
        public D20NpcLodBand lodBand;
        public string anchorId;
        public string feedbackKey;

        public static D20NpcPresentationSnapshot At(
            string stableNpcId,
            string stableResourceKey,
            D20NpcBehaviorState state,
            D20NpcJobPost post,
            Vector3 position)
        {
            return new D20NpcPresentationSnapshot
            {
                npcId = stableNpcId,
                resourceKey = stableResourceKey,
                behaviorState = state,
                jobPost = post,
                worldPosition = position,
                speed01 = state == D20NpcBehaviorState.Walk ? 0.55f : 0f,
                grounded = true,
                navigationStatus = D20NpcNavigationStatus.Arrived,
                dialogueStatus = D20NpcDialogueStatus.None,
                lodBand = D20NpcLodBand.Near
            };
        }
    }

    [Serializable]
    public struct D20NpcPresentationFeedback
    {
        public string feedbackKey;
        public string displayKey;
        public D20NpcFeedbackSeverity severity;
        public bool visible;
        public bool fallbackUsed;

        public static D20NpcPresentationFeedback Hidden()
        {
            return new D20NpcPresentationFeedback
            {
                feedbackKey = string.Empty,
                displayKey = string.Empty,
                severity = D20NpcFeedbackSeverity.None,
                visible = false,
                fallbackUsed = false
            };
        }
    }

    public static class D20NpcPresentationAnchorIds
    {
        public const string Market = "npc_visual_anchor_market_service";
        public const string Dock = "npc_visual_anchor_dock_work";
        public const string Shipyard = "npc_visual_anchor_shipyard_work";
        public const string Tavern = "npc_visual_anchor_tavern_service";

        public static string ForJob(D20NpcJobPost jobPost)
        {
            switch (jobPost)
            {
                case D20NpcJobPost.Market: return Market;
                case D20NpcJobPost.Dock: return Dock;
                case D20NpcJobPost.Shipyard: return Shipyard;
                case D20NpcJobPost.Tavern: return Tavern;
                default: return string.Empty;
            }
        }
    }

    /// <summary>
    /// Stable mapping from domain facts to presentation-only action and feedback.
    /// </summary>
    public static class D20NpcPresentationStateMap
    {
        public static D20NpcVisualAction ToVisualAction(D20NpcPresentationSnapshot snapshot)
        {
            if (snapshot.paused)
                return D20NpcVisualAction.Idle;

            switch (snapshot.behaviorState)
            {
                case D20NpcBehaviorState.Work: return D20NpcVisualAction.Work;
                case D20NpcBehaviorState.Walk: return D20NpcVisualAction.Walk;
                case D20NpcBehaviorState.Talk: return D20NpcVisualAction.Talk;
                case D20NpcBehaviorState.Rest: return D20NpcVisualAction.Rest;
                case D20NpcBehaviorState.Eat: return D20NpcVisualAction.Eat;
                case D20NpcBehaviorState.Alarm: return D20NpcVisualAction.Alarm;
                case D20NpcBehaviorState.Refuse: return D20NpcVisualAction.Refuse;
                case D20NpcBehaviorState.Injured: return D20NpcVisualAction.Injured;
                case D20NpcBehaviorState.Idle:
                case D20NpcBehaviorState.Busy:
                case D20NpcBehaviorState.Unreachable:
                case D20NpcBehaviorState.Paused:
                default: return D20NpcVisualAction.Idle;
            }
        }

        public static D20NpcPresentationFeedback ToFeedback(
            D20NpcPresentationSnapshot snapshot,
            bool actionFallbackUsed)
        {
            string key = FirstMeaningful(
                snapshot.feedbackKey,
                NavigationFeedback(snapshot.navigationStatus),
                DialogueFeedback(snapshot.dialogueStatus),
                snapshot.busy ? "npc_feedback_busy" : string.Empty,
                snapshot.injured || snapshot.behaviorState == D20NpcBehaviorState.Injured
                    ? "npc_feedback_injured" : string.Empty,
                snapshot.paused ? "npc_feedback_paused" : string.Empty,
                snapshot.behaviorState == D20NpcBehaviorState.Refuse
                    ? "npc_feedback_refused" : string.Empty,
                snapshot.dialogueOpen ? "npc_feedback_dialogue_open" : string.Empty,
                snapshot.dialogueStatus == D20NpcDialogueStatus.Completed
                    ? "npc_feedback_dialogue_ended" : string.Empty);

            if (string.IsNullOrEmpty(key) && snapshot.behaviorState == D20NpcBehaviorState.Work)
                key = "npc_feedback_working_" + snapshot.jobPost.ToString().ToLowerInvariant();

            return new D20NpcPresentationFeedback
            {
                feedbackKey = key,
                displayKey = key,
                severity = SeverityFor(key),
                visible = !string.IsNullOrEmpty(key),
                fallbackUsed = actionFallbackUsed
            };
        }

        public static string AnimatorStateName(D20NpcVisualAction action)
        {
            return "D20_NPC_" + action.ToString();
        }

        private static string NavigationFeedback(D20NpcNavigationStatus status)
        {
            switch (status)
            {
                case D20NpcNavigationStatus.Unreachable:
                case D20NpcNavigationStatus.NoAnchor:
                case D20NpcNavigationStatus.SceneUnloaded:
                    return "npc_feedback_unreachable";
                default: return string.Empty;
            }
        }

        private static string DialogueFeedback(D20NpcDialogueStatus status)
        {
            switch (status)
            {
                case D20NpcDialogueStatus.Busy: return "npc_feedback_busy";
                case D20NpcDialogueStatus.Unreachable: return "npc_feedback_unreachable";
                case D20NpcDialogueStatus.Refused: return "npc_feedback_refused";
                case D20NpcDialogueStatus.Open:
                case D20NpcDialogueStatus.Opening: return "npc_feedback_dialogue_open";
                case D20NpcDialogueStatus.Ending:
                case D20NpcDialogueStatus.Completed: return "npc_feedback_dialogue_ended";
                default: return string.Empty;
            }
        }

        private static string FirstMeaningful(params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
                if (!string.IsNullOrEmpty(values[i])) return values[i];
            return string.Empty;
        }

        private static D20NpcFeedbackSeverity SeverityFor(string key)
        {
            if (string.IsNullOrEmpty(key)) return D20NpcFeedbackSeverity.None;
            if (key.Contains("unreachable") || key.Contains("refused") || key.Contains("injured"))
                return D20NpcFeedbackSeverity.Warning;
            if (key.Contains("busy") || key.Contains("paused"))
                return D20NpcFeedbackSeverity.Info;
            return D20NpcFeedbackSeverity.Info;
        }
    }

}
