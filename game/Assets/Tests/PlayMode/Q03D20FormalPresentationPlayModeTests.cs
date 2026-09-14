using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.LocalQuality.Q03
{
    /// <summary>
    /// Q03-A23 formal-consumer gate. The default route is the audited Humanoid
    /// consumer; the test still verifies that D2 remains the physical-root
    /// owner and that the old fallback is hidden rather than stacked.
    /// </summary>
    public sealed class Q03D20FormalPresentationPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator FormalR02ConsumerFollowsContinuousD2ActionsWithoutMovingPhysicalRoot()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var bridgeType = FindLoadedType("Whalebone.Gameplay.D2.D20PlayerCandidatePresentationBridge");
            Assert.IsNotNull(bootstrapType);
            Assert.IsNotNull(characterType);
            Assert.IsNotNull(routerType);
            Assert.IsNotNull(bridgeType);

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var bootstrapDeadline = Time.realtimeSinceStartup + 8f;
            Component bootstrap = null;
            var isReady = bootstrapType.GetProperty("IsReady");
            while ((bootstrap == null || !(bool)isReady.GetValue(bootstrap)) && Time.realtimeSinceStartup < bootstrapDeadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType, FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2RuntimeBootstrap 没有创建。");
            Assert.IsTrue((bool)isReady.GetValue(bootstrap), "D2 运行时闭环没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");

            const string formalPrefabResourcePath = "D20/Formal/D20_R02_PlayerVisual_Male";
            var sourcePrefab = Resources.Load<GameObject>(formalPrefabResourcePath);
            Assert.IsNotNull(sourcePrefab, "Formal runtime Resources prefab 缺失。");
            var readableSkin = Resources.Load<Material>("D20/Materials/D20_PlayerSkin_Warm");
            Assert.IsNotNull(readableSkin, "Formal runtime Resources 缺少可读 skin material。");
            Assert.AreEqual("Universal Render Pipeline/Lit", readableSkin.shader.name,
                "Formal runtime readable skin 必须使用 URP Lit shader。");
            var sourceAnimator = sourcePrefab.GetComponent<Animator>();
            Assert.IsNotNull(sourceAnimator, "Formal runtime prefab 必须拥有根 Animator。");
            Assert.IsNotNull(sourceAnimator.avatar, "Formal runtime prefab 丢失 Avatar。");
            Assert.IsTrue(sourceAnimator.avatar.isValid && sourceAnimator.avatar.isHuman,
                "Formal runtime prefab 必须保留有效 Humanoid Avatar。");
            var bodyTransform = FindByName(sourcePrefab.transform, "SuperHero_Male");
            var bodyRenderer = bodyTransform == null ? null : bodyTransform.GetComponent<Renderer>();
            Assert.IsNotNull(bodyRenderer, "Formal runtime prefab 必须保留可渲染的 SuperHero_Male body。");
            Assert.IsNotNull(bodyRenderer.sharedMaterial, "Formal runtime body 缺少 skin material。");
            Assert.AreEqual("Universal Render Pipeline/Lit", bodyRenderer.sharedMaterial.shader.name,
                "Formal runtime body 必须使用可运行的 URP Lit skin shader。");
            var skinColor = bodyRenderer.sharedMaterial.HasProperty("_BaseColor")
                ? bodyRenderer.sharedMaterial.GetColor("_BaseColor")
                : bodyRenderer.sharedMaterial.color;
            Assert.Greater(skinColor.r, 0.35f, "Formal runtime skin material 仍然过暗，近景会成为黑色剪影。");
            Assert.Greater(skinColor.g, 0.15f, "Formal runtime skin material 仍然过暗，近景会成为黑色剪影。");

            var install = bridgeType.GetMethod("InstallForFormalHumanoid", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(install, "Formal D2 presentation consumer 缺少显式安装入口。");
            var bridge = install.Invoke(null, new object[] { player }) as Component;
            Assert.IsNotNull(bridge, "Formal D2 presentation consumer 没有创建。");
            var installed = bridgeType.GetProperty("IsInstalled");
            var failure = bridgeType.GetProperty("InstallationFailure");
            Assert.IsTrue((bool)installed.GetValue(bridge), failure.GetValue(bridge) as string);
            Assert.IsTrue((bool)bridgeType.GetProperty("IsFormalConsumer").GetValue(bridge),
                "该实例必须登记为 Formal consumer。");
            Assert.AreEqual("Formal", bridgeType.GetProperty("ConsumerModeName").GetValue(bridge));
            Assert.IsFalse((bool)bridgeType.GetProperty("UsesGenericPathAdapter").GetValue(bridge),
                "当前正式 Humanoid 路径不能退回 generic transform-path adapter。");
            Assert.IsTrue((bool)bridgeType.GetProperty("UsesHumanoidPoseAdapter").GetValue(bridge),
                "当前正式 Player 必须由 Humanoid pose adapter 消费已准入动作。");
            var candidateAnimator = bridgeType.GetProperty("CandidateAnimator").GetValue(bridge) as Animator;
            var appearance = bridgeType.GetProperty("AppearanceInstance").GetValue(bridge) as GameObject;
            var adapter = bridgeType.GetProperty("Adapter").GetValue(bridge) as Component;
            Assert.IsNotNull(candidateAnimator);
            Assert.IsNotNull(appearance);
            Assert.IsNotNull(adapter);
            Assert.IsNotNull(candidateAnimator.avatar,
                "运行态 Humanoid 路径必须保留有效 Avatar；不能清空 Avatar 后伪装成正式动作。");
            Assert.IsTrue(candidateAnimator.avatar.isValid && candidateAnimator.avatar.isHuman,
                "运行态 Humanoid 路径必须继续使用有效人形 Avatar。");
            yield return null;
            Assert.IsNotNull(appearance.transform.Find("Visual_player_visual_male_r02/Armature/root/pelvis"),
                "Formal Humanoid runtime instance 必须保留 Avatar-authored wrapper skeleton。");
            Assert.Greater((int)bridgeType.GetProperty("CandidateRendererCount").GetValue(bridge), 0);
            Assert.Greater((int)bridgeType.GetProperty("HiddenExistingRendererCount").GetValue(bridge), 0,
                "Formal consumer 必须隐藏旧 fallback，不能叠加第二个人物。");
            var formalBody = FindByName(appearance.transform, "SuperHero_Male");
            var formalBodyRenderer = formalBody == null ? null : formalBody.GetComponent<Renderer>();
            Assert.IsNotNull(formalBodyRenderer, "Formal consumer 缺少 SuperHero_Male renderer。");
            Assert.AreEqual("D20_PlayerSkin_Warm", formalBodyRenderer.sharedMaterial.name,
                "D20 presentation owner 的 readable skin 不得被 D7 material polish 覆盖。");

            var idleDeadline = Time.realtimeSinceStartup + 1f;
            while (bridgeType.GetProperty("LastCandidateState").GetValue(bridge).ToString() != "Idle" &&
                   Time.realtimeSinceStartup < idleDeadline)
            {
                yield return null;
            }
            Assert.AreEqual("Idle", bridgeType.GetProperty("LastCandidateState").GetValue(bridge).ToString(),
                "Formal presentation must settle from the initial landing transition before the Idle assertions run.");
            Assert.IsFalse((bool)adapter.GetType().GetProperty("LastApplyUsedFallback").GetValue(adapter));
            var idleLeftHand = FindByName(appearance.transform, "hand_l");
            var idleRightHand = FindByName(appearance.transform, "hand_r");
            var idleLeftUpperArm = FindByName(appearance.transform, "upperarm_l");
            var idleRightUpperArm = FindByName(appearance.transform, "upperarm_r");
            Assert.IsNotNull(idleLeftHand);
            Assert.IsNotNull(idleRightHand);
            Assert.IsNotNull(idleLeftUpperArm);
            Assert.IsNotNull(idleRightUpperArm);
            var poseProbe = appearance.AddComponent<LateUpdatePoseProbe>();
            poseProbe.Configure(idleLeftHand, idleRightHand, idleLeftUpperArm, idleRightUpperArm);
            yield return null;
            Assert.IsTrue(poseProbe.Captured, "Formal Idle pose was not sampled after the bridge LateUpdate.");
            Assert.Less(poseProbe.LeftHandY, poseProbe.LeftUpperArmY,
                "Generic formal Idle must not expose the imported horizontal T-pose arm.");
            Assert.Less(poseProbe.RightHandY, poseProbe.RightUpperArmY,
                "Generic formal Idle must not expose the imported horizontal T-pose arm.");

            var physicalPosition = player.transform.position;
            var physicalRotation = player.transform.rotation;
            candidateAnimator.Update(0.1f);
            Assert.AreEqual(physicalPosition, player.transform.position,
                "D20 formal Animator 不得移动 D2 physical root。");
            Assert.AreEqual(physicalRotation, player.transform.rotation,
                "D20 formal Animator 不得旋转 D2 physical root。");

            var character = player.GetComponent(characterType);
            var router = Object.FindFirstObjectByType(routerType, FindObjectsInactive.Include) as Component;
            Assert.IsNotNull(character);
            Assert.IsNotNull(router);
            characterType.GetMethod("SetInputEnabled").Invoke(character, new object[] { true });

            Keyboard keyboard = null;
            try
            {
                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                var deadline = Time.realtimeSinceStartup + 1f;
                while (bridgeType.GetProperty("LastCandidateState").GetValue(bridge).ToString() != "Walk" &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.AreEqual("Walk", bridgeType.GetProperty("LastCandidateState").GetValue(bridge).ToString(),
                    "正式 consumer 未消费 D2 Walk。");

                Press(keyboard.leftShiftKey, queueEventOnly: true);
                InputSystem.Update();
                deadline = Time.realtimeSinceStartup + 1f;
                while (bridgeType.GetProperty("LastCandidateState").GetValue(bridge).ToString() != "Run" &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.AreEqual("Run", bridgeType.GetProperty("LastCandidateState").GetValue(bridge).ToString(),
                    "正式 consumer 未消费 D2 Run。");

                Release(keyboard.wKey, queueEventOnly: true);
                Release(keyboard.leftShiftKey, queueEventOnly: true);
                InputSystem.Update();
                characterType.GetMethod("SetInputEnabled").Invoke(character, new object[] { false });
                yield return null;
                var preBoardPosition = player.transform.position;
                var preBoardRotation = player.transform.rotation;
                candidateAnimator.Update(0.1f);
                Assert.AreEqual(preBoardPosition, player.transform.position,
                    "正式 consumer 的 Animator.Update 不得在上船前移动 D2 physical root。");
                Assert.AreEqual(preBoardRotation, player.transform.rotation,
                    "正式 consumer 的 Animator.Update 不得在上船前旋转 D2 physical root。");
                Assert.IsTrue((bool)routerType.GetMethod("TryToggleShip").Invoke(router, null),
                    "正式 consumer 路径无法触发上船。");
                yield return null;
                var shipState = bridgeType.GetProperty("LastCandidateState").GetValue(bridge).ToString();
                Assert.That(shipState, Is.EqualTo("Board").Or.EqualTo("Helm"),
                    "正式 consumer 未消费 Board/Helm。");
                var helmDeadline = Time.realtimeSinceStartup + 1f;
                while (bridgeType.GetProperty("LastCandidateState").GetValue(bridge).ToString() != "Helm" &&
                       Time.realtimeSinceStartup < helmDeadline)
                    yield return null;
                Assert.AreEqual("Helm", bridgeType.GetProperty("LastCandidateState").GetValue(bridge).ToString(),
                    "正式 consumer 进入 Ship 后没有稳定消费 Helm。");
                var helmLeftHand = FindByName(appearance.transform, "hand_l");
                var helmRightHand = FindByName(appearance.transform, "hand_r");
                var helmLeftUpperArm = FindByName(appearance.transform, "upperarm_l");
                var helmRightUpperArm = FindByName(appearance.transform, "upperarm_r");
                poseProbe.Configure(helmLeftHand, helmRightHand, helmLeftUpperArm, helmRightUpperArm);
                yield return null;
                Assert.IsTrue(poseProbe.Captured, "Formal Helm pose was not sampled after the bridge LateUpdate.");
                Assert.Less(poseProbe.LeftHandY, poseProbe.LeftUpperArmY,
                    "Formal Helm generic pose 仍将左手保持在横向 T-pose 高度。");
                Assert.Less(poseProbe.RightHandY, poseProbe.RightUpperArmY,
                    "Formal Helm generic pose 仍将右手保持在横向 T-pose 高度。");
                Assert.IsTrue((bool)routerType.GetMethod("TryToggleShip").Invoke(router, null),
                    "正式 consumer 路径无法触发下船。");
                yield return null;
                Assert.AreEqual("Disembark", bridgeType.GetProperty("LastCandidateState").GetValue(bridge).ToString(),
                    "正式 consumer 未消费 Disembark。");

                var postDisembarkPosition = player.transform.position;
                var postDisembarkRotation = player.transform.rotation;
                candidateAnimator.Update(0.1f);
                Assert.AreEqual(postDisembarkPosition, player.transform.position,
                    "正式 consumer 的 Animator.Update 不得在下船后移动 D2 physical root。");
                Assert.AreEqual(postDisembarkRotation, player.transform.rotation,
                    "正式 consumer 的 Animator.Update 不得在下船后旋转 D2 physical root。");
            }
            finally
            {
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.wKey);
                    Release(keyboard.leftShiftKey);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
            }

            Assert.Greater((int)bridgeType.GetProperty("SnapshotCount").GetValue(bridge), 3,
                "正式 consumer 没有持续消费 D2 snapshots。");
            Assert.AreEqual(0, (int)bridgeType.GetProperty("UnsupportedActionCount").GetValue(bridge));
        }

        private static System.Type FindLoadedType(string fullName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }

        private static Transform FindByName(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindByName(child, name);
                if (found != null) return found;
            }

            return null;
        }

        [DefaultExecutionOrder(10000)]
        private sealed class LateUpdatePoseProbe : MonoBehaviour
        {
            private Transform _leftHand;
            private Transform _rightHand;
            private Transform _leftUpperArm;
            private Transform _rightUpperArm;

            public bool Captured { get; private set; }
            public float LeftHandY { get; private set; }
            public float RightHandY { get; private set; }
            public float LeftUpperArmY { get; private set; }
            public float RightUpperArmY { get; private set; }

            public void Configure(
                Transform leftHand,
                Transform rightHand,
                Transform leftUpperArm,
                Transform rightUpperArm)
            {
                _leftHand = leftHand;
                _rightHand = rightHand;
                _leftUpperArm = leftUpperArm;
                _rightUpperArm = rightUpperArm;
                Captured = false;
            }

            private void LateUpdate()
            {
                if (Captured || _leftHand == null || _rightHand == null ||
                    _leftUpperArm == null || _rightUpperArm == null)
                    return;

                LeftHandY = _leftHand.position.y;
                RightHandY = _rightHand.position.y;
                LeftUpperArmY = _leftUpperArm.position.y;
                RightUpperArmY = _rightUpperArm.position.y;
                Captured = true;
            }
        }
    }
}
