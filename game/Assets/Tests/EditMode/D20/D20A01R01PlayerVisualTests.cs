using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WhalesIslandReef.Tests.D20
{
    public sealed class D20A01R01PlayerVisualTests
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_HumanoidFemale_R01.prefab";
        private const string AvatarPath = "Assets/Art/Models/Characters/D20/Player/D20_PlayerHumanoidAvatar_R01.asset";

        [Test]
        public void R01PrefabUsesRealHumanoidAvatarAndD20Height()
        {
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath);
            Assert.That(avatar, Is.Not.Null);
            Assert.That(avatar.isValid, Is.True);
            Assert.That(avatar.isHuman, Is.True);

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            Assert.That(root, Is.Not.Null);
            try
            {
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                Assert.That(renderers.Length, Is.GreaterThan(0));
                var bounds = renderers[0].bounds;
                for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                Assert.That(bounds.size.y, Is.EqualTo(1.8f).Within(0.03f));
                Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [Test]
        public void R01PrefabHasTenPureTransformSocketsAndNoGameplayComponents()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            Assert.That(root, Is.Not.Null);
            try
            {
                var sockets = 0;
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    if (transform.name.StartsWith("D20Socket_")) sockets++;
                Assert.That(sockets, Is.EqualTo(10));
                Assert.That(root.GetComponentsInChildren<MonoBehaviour>(true).Length, Is.EqualTo(0));
                Assert.That(root.GetComponentsInChildren<Animator>(true).Length, Is.EqualTo(0));
                Assert.That(root.GetComponentsInChildren<CharacterController>(true).Length, Is.EqualTo(0));
                Assert.That(root.GetComponentsInChildren<LODGroup>(true).Length, Is.EqualTo(1));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [Test]
        public void R01InterfaceKeepsMissingVariantTruthAndRuntimePromotionBoundary()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var path = Path.Combine(projectRoot, "tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE.json");
            var text = File.ReadAllText(path);
            StringAssert.Contains("\"rig_interface_ready\": true", text);
            StringAssert.Contains("second_body_or_gender_direction", text);
            StringAssert.Contains("local_development_ready\": false", text);
            StringAssert.Contains("release_approved\": false", text);
            StringAssert.Contains("D18-S03", text);
        }
    }
}
