using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WhalesIslandReef.Tests.D20
{
    public sealed class D20A03NpcVisualTests
    {
        private static readonly string[] PrefabNames =
        {
            "npc_d20_deckhand_01",
            "npc_d20_rigger_01",
            "npc_d20_shore_trader_01",
            "npc_d20_shore_trader_02",
            "npc_d20_dock_clerk_01",
            "npc_d20_net_mender_01",
            "npc_d20_fisher_01",
            "npc_d20_tavern_cook_01",
            "npc_d20_boatbuilder_01",
            "npc_d20_sailmaker_01",
            "npc_d20_harbor_watch_proxy",
            "npc_d20_common_resident_01",
        };

        private static string PrefabPath(string name) => "Assets/Prefabs/Characters/D20/NPC/" + name + ".prefab";

        [Test]
        public void RosterHasTwelveStablePrefabKeys()
        {
            Assert.That(PrefabNames.Distinct().Count(), Is.EqualTo(12));
            foreach (var name in PrefabNames)
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(name)), Is.Not.Null, name);
        }

        [Test]
        public void EveryRosterPrefabHasVisualCollisionLodAnimatorAndAnchors()
        {
            foreach (var name in PrefabNames)
            {
                var path = PrefabPath(name);
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Assert.That(root.transform.Find("Visual"), Is.Not.Null, name + " visual");
                    Assert.That(root.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0), name + " renderer");
                    Assert.That(root.GetComponent<CapsuleCollider>(), Is.Not.Null, name + " root collision");
                    Assert.That(root.GetComponent<LODGroup>(), Is.Not.Null, name + " lod");
                    Assert.That(root.GetComponent<Animator>(), Is.Not.Null, name + " animator");
                    Assert.That(root.transform.Find("Anchors/Head"), Is.Not.Null, name + " head anchor");
                    Assert.That(root.transform.Find("Anchors/Hand_R"), Is.Not.Null, name + " hand anchor");
                    Assert.That(root.transform.Find("Anchors/Carry"), Is.Not.Null, name + " carry anchor");
                    Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one), name + " root scale");
                    Assert.That(root.GetComponentsInChildren<Renderer>(true).All(r => r.sharedMaterials != null && r.sharedMaterials.All(m => m != null && m.shader != null)), Is.True, name + " materials/shaders");
                    var bounds = root.GetComponentsInChildren<Renderer>(true).AggregateBounds();
                    Assert.That(bounds.size.sqrMagnitude, Is.GreaterThan(0.000001f), name + " bounds");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        [Test]
        public void InterfaceResultAndSourceModelBoundaryArePresent()
        {
            var gameRoot = Directory.GetParent(Application.dataPath).FullName;
            var interfacePath = Path.Combine(gameRoot, "tools/results/D20/D20-A03/D20_A03_NPC_VISUAL_INTERFACE.json");
            Assert.That(File.Exists(interfacePath), Is.True);
            var interfaceText = File.ReadAllText(interfacePath);
            StringAssert.Contains("npc_visual_interface_ready", interfaceText);
            StringAssert.Contains("crew_quaternius_male_shirt_v01", interfaceText);
            StringAssert.DoesNotContain("Male_Suit.fbx", interfaceText);
            StringAssert.Contains("intake_eligible", interfaceText);
        }
    }

    internal static class D20A03RendererBoundsExtensions
    {
        public static Bounds AggregateBounds(this Renderer[] renderers)
        {
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            var initialized = false;
            foreach (var renderer in renderers)
            {
                if (!initialized) { bounds = renderer.bounds; initialized = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }
    }
}
