using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WhalesIslandReef.Tests.D20
{
    public sealed class D20A01PlayerVisualTests
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_Neutral.prefab";

        [Test]
        public void CandidatePrefabLoadsWithoutGameplayComponents()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            Assert.That(root, Is.Not.Null, "D20-A01 candidate prefab must be loadable by Unity.");
            if (root == null)
                return;

            try
            {
                Assert.That(root.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0), "candidate renderer");
                Assert.That(root.GetComponentsInChildren<MonoBehaviour>(true).Length, Is.EqualTo(0), "candidate gameplay scripts");
                Assert.That(root.GetComponent<Animator>(), Is.Null, "formal Animator is D20 consumer-owned");
                Assert.That(root.GetComponent<CharacterController>(), Is.Null, "D17 physics root is not embedded");
                Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one), "candidate root scale");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [Test]
        public void InterfaceContractAndStableFallbackArePresent()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var interfacePath = Path.Combine(projectRoot, "tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE.json");
            Assert.That(File.Exists(interfacePath), Is.True);
            var interfaceText = File.ReadAllText(interfacePath);
            StringAssert.Contains("\"rig_interface_ready\": true", interfaceText);
            StringAssert.Contains("candidate_superhero_female_fullbody_r03p", interfaceText);
            StringAssert.Contains("D17_programmatic_fallback", interfaceText);
            StringAssert.Contains("Candidate_only_D18-S03_required", interfaceText);
        }

        [Test]
        public void CandidatePrefabTextHasNoRuntimeScriptOrAnimatorBinding()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var prefabPath = Path.Combine(projectRoot, "Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_Neutral.prefab");
            var prefabText = File.ReadAllText(prefabPath);
            StringAssert.DoesNotContain("m_Script:", prefabText);
            StringAssert.DoesNotContain("Animator:", prefabText);
            StringAssert.DoesNotContain("CharacterController:", prefabText);
            StringAssert.Contains("2d800d0a6b7873b42a6c68d197a0d187", prefabText);
        }
    }
}
