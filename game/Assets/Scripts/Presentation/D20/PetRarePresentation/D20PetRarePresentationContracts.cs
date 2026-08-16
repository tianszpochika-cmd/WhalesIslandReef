using System;
using UnityEngine;

namespace WhalesIslandReef.Presentation.D20.PetRare
{
    /// <summary>
    /// Presentation state only. PetSystem, EcologySystem and CollectionSystem own the facts;
    /// this enum is a read-only projection of their already-decided state.
    /// </summary>
    public enum D20PetRareVisualState
    {
        Idle = 0,
        Follow = 1,
        Stay = 2,
        Rest = 3,
        Interact = 4,
        Startled = 5,
        Observe = 6,
        Unavailable = 7,
        DiscoverySuccess = 8,
        DiscoveryFailure = 9,
        Travel = 10,
        Alert = 11,
        Flee = 12,
        Observed = 13,
        Sit = 14,
        Play = 15,
        Eat = 16,
    }

    public enum D20PetRareLocation
    {
        Harbor = 0,
        Cabin = 1,
        ShipDeck = 2,
        Boarding = 3,
        Disembarking = 4,
        ObservationSite = 5,
        Lost = 6,
        Unavailable = 7,
    }

    public enum D20PetRareLodBand
    {
        Near = 0,
        Mid = 1,
        Far = 2,
        Proxy = 3,
    }

    public enum D20PetRareDiscoveryFeedback
    {
        None = 0,
        ObservationPrompt = 1,
        DiscoverySuccess = 2,
        DiscoveryFailure = 3,
    }

    /// <summary>
    /// Read-only input assembled by the owner systems. It deliberately contains no ownership,
    /// affection, health, reward, inventory, ecology-pressure, capture or save-writer fields.
    /// </summary>
    [Serializable]
    public struct D20PetRarePresentationInput
    {
        public string petInstanceId;
        public string speciesId;
        public string resourceKey;
        public D20PetRareVisualState visualState;
        public D20PetRareLocation location;
        public Vector3 authoritativeWorldPosition;
        [Range(0f, 1f)] public float speed01;
        public bool present;
        public bool available;
        public D20PetRareLodBand lodBand;
        public D20PetRareDiscoveryFeedback discoveryFeedback;
        public string feedbackPromptKey;
    }

    /// <summary>
    /// Deterministic mapping from owner facts to Animator state names. Missing source clips are
    /// represented by a controller state that points at the neutral fallback clip.
    /// </summary>
    public static class D20PetRareStateMap
    {
        public static string ToAnimatorState(D20PetRareVisualState state)
        {
            switch (state)
            {
                case D20PetRareVisualState.Follow:
                    return "Follow";
                case D20PetRareVisualState.Travel:
                    return "Travel";
                case D20PetRareVisualState.Stay:
                    return "Stay";
                case D20PetRareVisualState.Rest:
                    return "Rest";
                case D20PetRareVisualState.Interact:
                    return "Interact";
                case D20PetRareVisualState.Startled:
                    return "Startled";
                case D20PetRareVisualState.Alert:
                    return "Alert";
                case D20PetRareVisualState.Flee:
                    return "Flee";
                case D20PetRareVisualState.Observe:
                    return "Observe";
                case D20PetRareVisualState.Unavailable:
                    return "Unavailable";
                case D20PetRareVisualState.DiscoverySuccess:
                    return "DiscoverySuccess";
                case D20PetRareVisualState.DiscoveryFailure:
                    return "DiscoveryFailure";
                case D20PetRareVisualState.Observed:
                    return "Observed";
                case D20PetRareVisualState.Sit:
                    return "Sit";
                case D20PetRareVisualState.Play:
                    return "Play";
                case D20PetRareVisualState.Eat:
                    return "Eat";
                default:
                    return "Idle";
            }
        }
    }
}
