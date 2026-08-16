using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WhalesIslandReef.Presentation.D20.Wildlife;

namespace D20.Tests
{
    public sealed class D20WildlifePresentationTests
    {
        private static readonly string[] PrefabPaths =
        {
            "Assets/Prefabs/Animals/D20/Wildlife/D20_Wildlife_eagle_coastal_bird.prefab",
            "Assets/Prefabs/Animals/D20/Wildlife/D20_Wildlife_fox_small_land.prefab",
            "Assets/Prefabs/Animals/D20/Wildlife/D20_Wildlife_cow_livestock.prefab",
            "Assets/Prefabs/Animals/D20/Wildlife/D20_Wildlife_fish_shallow_school.prefab",
            "Assets/Prefabs/Animals/D20/Wildlife/D20_Wildlife_fish_nearshore.prefab",
            "Assets/Prefabs/Animals/D20/Wildlife/D20_Wildlife_shark_nearshore_predator.prefab",
            "Assets/Prefabs/Animals/D20/Wildlife/D20_Wildlife_manta_large_aquatic_equivalent.prefab",
            "Assets/Prefabs/Animals/D20/Wildlife/D20_Wildlife_whale_distant_aquatic.prefab"
        };

        [Test]
        public void EightRealRepresentativePrefabsHaveStablePresentationStructure()
        {
            Assert.That(PrefabPaths, Has.Length.EqualTo(8));
            foreach (var path in PrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one), path);
                Assert.That(prefab.transform.Find("Visual/LOD0"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Visual/LOD1"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Visual/LOD2"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Rig/Generic"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("Sockets/Observation"), Is.Not.Null, path);
                Assert.That(prefab.transform.Find("CollisionProxy"), Is.Not.Null, path);
                Assert.That(prefab.GetComponent<D20WildlifePresentationAdapter>(), Is.Not.Null, path);
                Assert.That(prefab.GetComponent<LODGroup>(), Is.Not.Null, path);
                Assert.That(prefab.GetComponentsInChildren<Renderer>(true), Is.Not.Empty, path);
            }
        }

        [Test]
        public void PrefabsHaveGenericAnimatorCullingControllersAndRemoteFallback()
        {
            foreach (var path in PrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var animator = prefab.GetComponentInChildren<Animator>(true);
                var lodGroup = prefab.GetComponent<LODGroup>();
                var adapter = prefab.GetComponent<D20WildlifePresentationAdapter>();
                var collision = prefab.transform.Find("CollisionProxy").GetComponent<Collider>();
                var fallback = prefab.transform.Find("Visual/LOD2").GetComponentInChildren<Renderer>(true);

                Assert.That(animator, Is.Not.Null, path);
                Assert.That(animator.runtimeAnimatorController, Is.Not.Null, path);
                Assert.That(animator.applyRootMotion, Is.False, path);
                Assert.That(animator.cullingMode, Is.EqualTo(AnimatorCullingMode.CullUpdateTransforms), path);
                Assert.That(lodGroup.GetLODs(), Has.Length.EqualTo(3), path);
                Assert.That(lodGroup.GetLODs()[2].renderers, Does.Contain(fallback), path);
                Assert.That(collision, Is.Not.Null, path);
                Assert.That(collision.isTrigger, Is.True, path);
                Assert.That(adapter.SpeciesId, Does.StartWith("animal_").Or.StartWith("fish_"), path);
            }
        }

        [Test]
        public void StateMappingUsesNeutralOrSwimFallbackWithoutWritingFacts()
        {
            Assert.That(D20WildlifeStateMap.ToAnimatorState("Idle", false), Is.EqualTo("Idle"));
            Assert.That(D20WildlifeStateMap.ToAnimatorState("Forage", false), Is.EqualTo("Feed"));
            Assert.That(D20WildlifeStateMap.ToAnimatorState("Flee", false), Is.EqualTo("Flee"));
            Assert.That(D20WildlifeStateMap.ToAnimatorState("Dive", true), Is.EqualTo("Dive"));
            Assert.That(D20WildlifeStateMap.ToAnimatorState("UnknownState", true), Is.EqualTo("Swim"));
            Assert.That(D20WildlifeStateMap.ToAnimatorState("UnknownState", false), Is.EqualTo("Idle"));
        }

        [Test]
        public void SnapshotPlacementAndLodOnlyChangePresentation()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPaths[3]);
            var instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                var adapter = instance.GetComponent<D20WildlifePresentationAdapter>();
                var input = new D20WildlifePresentationInput
                {
                    speciesId = adapter.SpeciesId,
                    behaviorState = "Swim",
                    worldPosition = new Vector3(4f, -1.5f, 8f),
                    speed01 = 0.5f,
                    waterDepthM = 1.5f,
                    ecology = new D20WildlifeEcologyView
                    {
                        present = true,
                        visibility01 = 1f,
                        populationBand = 2,
                        observationStatus = "observed"
                    }
                };

                adapter.ApplyReadOnlySnapshot(input);
                Assert.That(instance.transform.position, Is.EqualTo(input.worldPosition));
                adapter.SetLodBand(D20WildlifeLodBand.Far);
                Assert.That(adapter.CurrentLodBand, Is.EqualTo(D20WildlifeLodBand.Far));
                Assert.That(adapter.IsFallbackVisible, Is.True);
                Assert.That(adapter.GetComponentInChildren<Animator>(true).enabled, Is.False);

                input.ecology.present = false;
                adapter.ApplyReadOnlySnapshot(input);
                Assert.That(instance.transform.position, Is.EqualTo(new Vector3(4f, -1.5f, 8f)));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void ScreenBudgetIsD20FrozenAnimalBudgetAndNotAnEcologyCap()
        {
            Assert.That(D20WildlifePerformanceBudget.GetAnimalBudget("low"), Is.EqualTo(12));
            Assert.That(D20WildlifePerformanceBudget.GetAnimalBudget("medium"), Is.EqualTo(24));
            Assert.That(D20WildlifePerformanceBudget.GetAnimalBudget("high"), Is.EqualTo(32));
            Assert.That(D20WildlifePerformanceBudget.HighAnimals1080p, Is.LessThanOrEqualTo(32));
        }

        [Test]
        public void PresentationAdapterContainsNoNetworkOrRewardIntegration()
        {
            var sourcePath = Path.Combine(Application.dataPath, "Scripts/Presentation/D20/Wildlife/D20WildlifePresentationAdapter.cs");
            var source = File.ReadAllText(sourcePath);
            Assert.That(source, Does.Not.Contain("UnityWebRequest"));
            Assert.That(source, Does.Not.Contain("HttpClient"));
            Assert.That(source, Does.Not.Contain("RewardSystem"));
            Assert.That(source, Does.Not.Contain("LLM"));
        }
    }
}
