using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using WhalesIslandReef.Presentation.D20.PetRare;

namespace D20.Tests.PetsRare
{
    public sealed class D20PetRarePresentationTests
    {
        private static readonly string[] PrefabPaths =
        {
            "Assets/Prefabs/Animals/D20/PetsRare/D20_PetsRare_cat_core.prefab",
            "Assets/Prefabs/Animals/D20/PetsRare/D20_PetsRare_dog_shiba.prefab",
            "Assets/Prefabs/Animals/D20/PetsRare/D20_PetsRare_eagle_observation.prefab",
            "Assets/Prefabs/Animals/D20/PetsRare/D20_PetsRare_fox_observation.prefab",
        };

        [Test]
        public void FourIsolatedPrefabsLoadWithPetAndRarePresentationStructure()
        {
            Assert.That(PrefabPaths, Has.Length.EqualTo(4));
            foreach (var path in PrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one), path);
                Assert.That(prefab.GetComponent<D20PetRarePresentationAdapter>(), Is.Not.Null, path);
                Assert.That(prefab.GetComponent<LODGroup>(), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Sockets/FollowAnchor"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Sockets/InteractionAnchor"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Sockets/BoardingAnchor"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Sockets/RestAnchor"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Sockets/DiscoveryHintAnchor"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("CollisionProxy"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Visual/LOD0"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Visual/LOD1"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Visual/LOD2"), Is.Not.Null, path);
                Assert.That(prefab.GetComponentsInChildren<Renderer>(true), Is.Not.Empty, path);
            }
        }

        [Test]
        public void CatDogAndRarePrefabsUseRealAnimatorControllersAndNonBlockingCollision()
        {
            foreach (var path in PrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var animator = prefab.GetComponentInChildren<Animator>(true);
                var controller = animator == null ? null : animator.runtimeAnimatorController as AnimatorController;
                Assert.That(animator, Is.Not.Null, path);
                Assert.That(animator.applyRootMotion, Is.False, path);
                Assert.That(controller, Is.Not.Null, path);
                Assert.That(controller.layers, Has.Length.GreaterThan(0), path);
                Assert.That(controller.layers[0].stateMachine.states, Has.Length.GreaterThanOrEqualTo(19), path);
                foreach (var state in controller.layers[0].stateMachine.states)
                    Assert.That(state.state.motion, Is.Not.Null, path + ":" + state.state.name);

                var colliders = prefab.GetComponentsInChildren<Collider>(true);
                Assert.That(colliders, Is.Not.Empty, path);
                foreach (var collider in colliders)
                    Assert.That(collider.isTrigger, Is.True, path + ": collider must not block doors/ladder/helm");
                Assert.That(prefab.GetComponentsInChildren<Rigidbody>(true), Is.Empty, path);
                Assert.That(prefab.GetComponentsInChildren<CharacterController>(true), Is.Empty, path);
            }
        }

        [Test]
        public void PetAndRareStableIdsAreSeparatedAndRareVisualsAreObservationOnly()
        {
            var cat = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPaths[0]).GetComponent<D20PetRarePresentationAdapter>();
            var dog = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPaths[1]).GetComponent<D20PetRarePresentationAdapter>();
            var eagle = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPaths[2]).GetComponent<D20PetRarePresentationAdapter>();
            var fox = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPaths[3]).GetComponent<D20PetRarePresentationAdapter>();
            Assert.That(cat.PresentationKind, Is.EqualTo("pet_cat"));
            Assert.That(dog.PresentationKind, Is.EqualTo("pet_dog"));
            Assert.That(cat.RareObservationOnly, Is.False);
            Assert.That(dog.RareObservationOnly, Is.False);
            Assert.That(eagle.RareObservationOnly, Is.True);
            Assert.That(fox.RareObservationOnly, Is.True);
            Assert.That(eagle.SpeciesId, Is.EqualTo("animal_quaternius_eagle_v01"));
            Assert.That(fox.SpeciesId, Is.EqualTo("animal_quaternius_fox_v01"));
        }

        [Test]
        public void ReadOnlySnapshotCoversFollowStayInteractionBoardingAndFarFallback()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPaths[1]);
            var instance = Object.Instantiate(prefab);
            try
            {
                var adapter = instance.GetComponent<D20PetRarePresentationAdapter>();
                var input = new D20PetRarePresentationInput
                {
                    petInstanceId = "pet_instance_test",
                    speciesId = adapter.SpeciesId,
                    resourceKey = adapter.ResourceKey,
                    visualState = D20PetRareVisualState.Follow,
                    location = D20PetRareLocation.Boarding,
                    authoritativeWorldPosition = new Vector3(3f, 0.2f, 7f),
                    speed01 = 0.6f,
                    present = true,
                    available = true,
                    lodBand = D20PetRareLodBand.Near,
                    discoveryFeedback = D20PetRareDiscoveryFeedback.None,
                    feedbackPromptKey = string.Empty,
                };
                Assert.That(adapter.ApplyReadOnlySnapshot(input), Is.True);
                Assert.That(instance.transform.position, Is.EqualTo(input.authoritativeWorldPosition));
                Assert.That(adapter.CurrentLocation, Is.EqualTo(D20PetRareLocation.Boarding));
                Assert.That(adapter.CurrentVisualState, Is.EqualTo(D20PetRareVisualState.Follow));

                input.visualState = D20PetRareVisualState.Interact;
                input.location = D20PetRareLocation.ShipDeck;
                Assert.That(adapter.ApplyReadOnlySnapshot(input), Is.True);
                Assert.That(adapter.CurrentVisualState, Is.EqualTo(D20PetRareVisualState.Interact));

                input.visualState = D20PetRareVisualState.Stay;
                input.location = D20PetRareLocation.Cabin;
                input.lodBand = D20PetRareLodBand.Far;
                Assert.That(adapter.ApplyReadOnlySnapshot(input), Is.True);
                Assert.That(adapter.IsFallbackVisible, Is.True);

                input.visualState = D20PetRareVisualState.Unavailable;
                input.location = D20PetRareLocation.Lost;
                input.available = false;
                input.present = false;
                Assert.That(adapter.ApplyReadOnlySnapshot(input), Is.True);
                Assert.That(adapter.IsAvailable, Is.False);
                Assert.That(adapter.IsFallbackVisible, Is.False);

                Assert.That(adapter.RecoverAtAuthoritativePosition(new Vector3(9f, 0f, 2f),
                    D20PetRareLocation.Disembarking, true, D20PetRareLodBand.Near), Is.True);
                Assert.That(instance.transform.position, Is.EqualTo(new Vector3(9f, 0f, 2f)));
                Assert.That(adapter.IsAvailable, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void RareDiscoveryFeedbackNeverStoresOrDisplaysCoordinates()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPaths[2]);
            var instance = Object.Instantiate(prefab);
            try
            {
                var adapter = instance.GetComponent<D20PetRarePresentationAdapter>();
                adapter.SetDiscoveryFeedback(D20PetRareDiscoveryFeedback.ObservationPrompt,
                    "wildlife.observation.prompt");
                Assert.That(adapter.LastDiscoveryFeedback, Is.EqualTo(D20PetRareDiscoveryFeedback.ObservationPrompt));
                Assert.That(adapter.LastFeedbackPromptKey, Is.EqualTo("wildlife.observation.prompt"));
                adapter.SetDiscoveryFeedback(D20PetRareDiscoveryFeedback.DiscoverySuccess,
                    "wildlife.discovery.success");
                Assert.That(adapter.LastDiscoveryFeedback, Is.EqualTo(D20PetRareDiscoveryFeedback.DiscoverySuccess));
                Assert.That(adapter.LastFeedbackPromptKey, Does.Not.Contain("x="));
                Assert.That(adapter.LastFeedbackPromptKey, Does.Not.Contain("y="));
                Assert.That(adapter.LastFeedbackPromptKey, Does.Not.Contain("z="));
                adapter.SetDiscoveryFeedback(D20PetRareDiscoveryFeedback.DiscoveryFailure,
                    "wildlife.discovery.failure");
                Assert.That(adapter.LastDiscoveryFeedback, Is.EqualTo(D20PetRareDiscoveryFeedback.DiscoveryFailure));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void PresentationSourceHasNoNetworkLlmOrStrongLogicIntegration()
        {
            string sourcePath = Path.Combine(Application.dataPath,
                "Scripts/Presentation/D20/PetRarePresentation/D20PetRarePresentationAdapter.cs");
            string source = File.ReadAllText(sourcePath);
            Assert.That(source, Does.Not.Contain("UnityWebRequest"));
            Assert.That(source, Does.Not.Contain("HttpClient"));
            Assert.That(source, Does.Not.Contain("System.Net"));
            Assert.That(source, Does.Not.Contain("LLM"));
            Assert.That(source, Does.Not.Contain("AgentCore"));
        }
    }
}
