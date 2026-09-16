using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.D17
{
    /// <summary>
    /// D17-A02 本机动作状态回归：人物实际移动时必须进入走路/奔跑，
    /// 跳跃和上下船必须进入各自过渡；不替代真人动画资源和目标设备验收。
    /// </summary>
    public sealed class D17ActionQualityPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator D17A02CharacterActionStateFollowsMovementJumpAndBoarding()
        {
            var inputRouterType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var settingsType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16AccessibilitySettingsStore");
            Assert.IsNotNull(inputRouterType, "D16InputRouter 未加载。");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            var setBlocked = inputRouterType.GetMethod("SetGameplayInputBlocked");
            setBlocked.Invoke(null, new object[] { false });
            if (settingsType != null)
            {
                var reset = settingsType.GetMethod("Reset");
                if (reset != null) reset.Invoke(null, null);
            }

            Keyboard keyboard = null;
            try
            {
                var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
                Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
                while (!load.isDone) yield return null;

                Component bootstrap = null;
                var bootstrapReady = bootstrapType.GetProperty("IsReady");
                float deadline = Time.realtimeSinceStartup + 8f;
                while (bootstrap == null || !(bool)bootstrapReady.GetValue(bootstrap))
                {
                    bootstrap = Object.FindFirstObjectByType(bootstrapType,
                        FindObjectsInactive.Include) as Component;
                    if (Time.realtimeSinceStartup >= deadline) break;
                    yield return null;
                }

                Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
                Assert.IsTrue((bool)bootstrapReady.GetValue(bootstrap), "D2 运行时组装没有完成。");

                var player = GameObject.Find("D2_PlayerAvatar");
                Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
                var shipVisual = GameObject.Find("D19_R03_ShipVisualInstance");
                Assert.IsNotNull(shipVisual, "R03 场景缺少可见船实例，不能验证出生点避让船体。");
                Assert.Greater(Vector3.Distance(player.transform.position, shipVisual.transform.position), 2f,
                    "玩家初始出生点不能落在 R03 可见船体中心附近。");
                var character = player.GetComponent(characterType);
                var animation = player.GetComponent(animationType);
                Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
                Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");

                var importedSkinnedRigNeutralOnly = animationType.GetProperty("ImportedSkinnedRigNeutralOnly");
                Assert.IsNotNull(importedSkinnedRigNeutralOnly, "动作层缺少导入蒙皮中立姿态诊断。");
                var presentationType = FindLoadedType("Whalebone.Gameplay.D2.D20PlayerCandidatePresentationBridge");
                var presentation = presentationType == null ? null : player.GetComponent(presentationType);
                deadline = Time.realtimeSinceStartup + 3f;
                while (presentationType != null && presentation == null &&
                       Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                    presentation = player.GetComponent(presentationType);
                }

                var presentationInstalled = presentation != null &&
                    (bool)presentationType.GetProperty("IsInstalled").GetValue(presentation);
                if (presentation != null && !presentationInstalled)
                {
                    deadline = Time.realtimeSinceStartup + 3f;
                    while (!(bool)presentationType.GetProperty("IsInstalled").GetValue(presentation) &&
                           Time.realtimeSinceStartup < deadline)
                        yield return null;
                    presentationInstalled = (bool)presentationType.GetProperty("IsInstalled").GetValue(presentation);
                }

                bool hasSkinnedVisual = player.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0;
                if (hasSkinnedVisual && !presentationInstalled)
                {
                    Assert.IsTrue((bool)importedSkinnedRigNeutralOnly.GetValue(animation),
                        "当前导入蒙皮人物必须明确进入 neutral-only 安全路径。");
                    Assert.IsFalse((bool)animationType.GetProperty("ProceduralPoseLayerActive").GetValue(animation),
                        "未验证动作轴的导入蒙皮人物不能被猜测式程序化骨骼旋转驱动。");
                }
                else if (hasSkinnedVisual)
                {
                    var usesGeneric = (bool)presentationType.GetProperty("UsesGenericPathAdapter").GetValue(presentation);
                    var usesHumanoid = (bool)presentationType.GetProperty("UsesHumanoidPoseAdapter").GetValue(presentation);
                    Assert.IsTrue(usesGeneric || usesHumanoid,
                        "当前 D20 蒙皮表现必须由已登记的 generic 或 Humanoid consumer 接管。");
                    Assert.Greater((int)presentationType.GetProperty("CandidateRendererCount").GetValue(presentation), 0,
                        "D20 蒙皮表现没有可见 renderer。");
                    Assert.Greater((int)presentationType.GetProperty("HiddenExistingRendererCount").GetValue(presentation), 0,
                        "D20 蒙皮表现必须隐藏旧 fallback，不能与另一套人物叠加。");
                }

                var grounded = characterType.GetProperty("IsGrounded");
                var action = animationType.GetProperty("Action");
                var transitioning = animationType.GetProperty("IsTransitioning");
                var moveAxis = inputRouterType.GetMethod("MoveAxis");
                var tryJump = characterType.GetMethod("TryJump");
                var setInputEnabled = characterType.GetMethod("SetInputEnabled");
                Assert.IsNotNull(tryJump, "D2CharacterController 缺少 TryJump。");
                Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
                deadline = Time.realtimeSinceStartup + 3f;
                while ((!(bool)grounded.GetValue(character) ||
                        action.GetValue(animation).ToString() != "Idle") &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)grounded.GetValue(character), "动作回归开始前玩家没有落地。");
                Assert.AreEqual("Idle", action.GetValue(animation).ToString(),
                    "静止且落地的玩家必须处于待机动作。");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();

                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                for (int i = 0; i < 3; i++) yield return null;
                Debug.Log("[D17/A02] W action=" + action.GetValue(animation) +
                          ", moveAxis=" + moveAxis.Invoke(null, null));
                Assert.AreEqual("Walk", action.GetValue(animation).ToString(),
                    "按住 W 移动时没有进入走路动作。");

                Press(keyboard.leftShiftKey, queueEventOnly: true);
                InputSystem.Update();
                for (int i = 0; i < 2; i++) yield return null;
                Assert.AreEqual("Run", action.GetValue(animation).ToString(),
                    "按住 Shift+W 移动时没有进入奔跑动作。");

                Release(keyboard.wKey, queueEventOnly: true);
                Release(keyboard.leftShiftKey, queueEventOnly: true);
                InputSystem.Update();
                // 清空移动残余速度，确保接下来检查的是跳跃动作本身，
                // 而不是把减速阶段误判成跳跃失败。
                setInputEnabled.Invoke(character, new object[] { false });
                yield return null;
                setInputEnabled.Invoke(character, new object[] { true });
                yield return null;
                Assert.IsTrue((bool)grounded.GetValue(character), "跳跃输入前玩家已经失去地面接触。");
                Assert.AreEqual("Idle", action.GetValue(animation).ToString(),
                    "停止输入后动作层没有回到待机，不能安全开始跳跃。");

                // Unity Input System 的一帧按键边沿已由 D16 输入专项覆盖；
                // 这里直接调用角色 owner 的 TryJump，避免测试 runner 的
                // InputSystem.Update 时序把“动作状态”误判为输入失败。
                Assert.IsTrue((bool)tryJump.Invoke(character, null), "TryJump 没有接受有效跳跃请求。");
                yield return null;
                Assert.AreEqual("Jump", action.GetValue(animation).ToString(),
                    "跳跃请求后没有进入跳跃动作。");

                var router = Object.FindFirstObjectByType(routerType,
                    FindObjectsInactive.Include) as Component;
                Assert.IsNotNull(router, "D2ControlRouter 没有创建。");
                var tryToggleShip = routerType.GetMethod("TryToggleShip");
                Assert.IsNotNull(tryToggleShip, "D2ControlRouter 缺少 TryToggleShip。");
                deadline = Time.realtimeSinceStartup + 3f;
                while (!(bool)grounded.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)grounded.GetValue(character), "跳跃动作回归后玩家没有回到地面。");

                Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "动作回归无法完成上船。");
                for (int i = 0; i < 2; i++) yield return null;
                Assert.AreEqual("Boarding", action.GetValue(animation).ToString(),
                    "上船过渡没有进入 Boarding 动作。");
                Assert.IsTrue((bool)transitioning.GetValue(animation), "上船动作没有进入过渡状态。");

                if (hasSkinnedVisual && presentationInstalled)
                {
                    Assert.AreEqual("Boarding", presentationType.GetProperty("LastD2Action").GetValue(presentation),
                        "D20 表现消费者没有消费 D2 Boarding 动作状态。");

                    if ((bool)presentationType.GetProperty("UsesHumanoidPoseAdapter").GetValue(presentation))
                    {
                        var formalAdapter = presentationType.GetProperty("HumanoidPoseAdapter").GetValue(presentation);
                        Assert.IsNotNull(formalAdapter, "Formal Humanoid consumer 已声明启用，但 adapter 实例不存在。");
                        Assert.IsTrue((bool)formalAdapter.GetType().GetProperty("IsReady").GetValue(formalAdapter),
                            "登船过渡期间 Formal Humanoid adapter 必须保持 ready。");
                        Assert.IsTrue((bool)formalAdapter.GetType().GetProperty("LastApplySucceeded").GetValue(formalAdapter),
                            "登船过渡期间 Formal Humanoid adapter 必须成功提交 pose。");
                        Assert.AreEqual("Board", formalAdapter.GetType().GetProperty("CurrentState").GetValue(formalAdapter).ToString(),
                            "D2 Boarding 动作必须映射为 Formal Humanoid Board presentation state。");
                        Assert.AreEqual("Board", presentationType.GetProperty("LastCandidateState").GetValue(presentation).ToString(),
                            "D20 candidate 在上船过渡期间必须处于 Board presentation state。");
                    }
                }
                else if (hasSkinnedVisual)
                {
                    var cameraType = FindLoadedType("Whalebone.Gameplay.CameraFollow");
                    Assert.IsNotNull(cameraType, "InitialHarbor 缺少 CameraFollow 类型，无法确认船上人物取景主体。");
                    var camera = Object.FindFirstObjectByType(cameraType,
                        FindObjectsInactive.Include) as Component;
                    Assert.IsNotNull(camera, "InitialHarbor 缺少 CameraFollow，无法确认船上人物取景主体。");
                    Assert.AreEqual("ShipDeck", cameraType.GetProperty("ViewMode").GetValue(camera).ToString(),
                        "进入船控后相机没有切换到船上视角。");
                    Assert.AreEqual(player.transform, cameraType.GetProperty("Target").GetValue(camera),
                        "船上视角必须以已登船玩家为取景主体，不能只盯船根导致玩家被船帆遮挡。");
                }

                deadline = Time.realtimeSinceStartup + 2f;
                while ((bool)transitioning.GetValue(animation) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                if (hasSkinnedVisual && presentationInstalled &&
                    (bool)presentationType.GetProperty("UsesHumanoidPoseAdapter").GetValue(presentation))
                {
                    var formalAdapter = presentationType.GetProperty("HumanoidPoseAdapter").GetValue(presentation);
                    Assert.AreEqual("Helm", formalAdapter.GetType().GetProperty("CurrentState").GetValue(formalAdapter).ToString(),
                        "登船过渡结束后 Formal Humanoid consumer 必须进入 Helm presentation state。");
                    Assert.AreEqual("Helm", presentationType.GetProperty("LastCandidateState").GetValue(presentation).ToString(),
                        "船控模式下 D20 candidate 必须进入 Helm presentation state。");
                }

                Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "动作回归无法完成下船。");
                for (int i = 0; i < 2; i++) yield return null;
                Assert.AreEqual("Disembarking", action.GetValue(animation).ToString(),
                    "下船过渡没有进入 Disembarking 动作。");
                Assert.IsTrue((bool)transitioning.GetValue(animation), "下船动作没有进入过渡状态。");
                var ship = GameObject.Find("ph_ship_fishing");
                Assert.IsNotNull(ship, "动作回归下船后缺少稳定船对象。");
                Assert.Greater(Vector3.Distance(player.transform.position, ship.transform.position), 1f,
                    "下船不能把玩家留在船根原点或可见船体内部。");
            }
            finally
            {
                Time.timeScale = 1f;
                setBlocked.Invoke(null, new object[] { false });
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.wKey);
                    Release(keyboard.leftShiftKey);
                    Release(keyboard.spaceKey);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
            }
        }

        [UnityTest]
        public IEnumerator D17A03CharacterEntersWaterAndReportsSwimmingAction()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while (bootstrap == null || !(bool)ready.GetValue(bootstrap))
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                if (Time.realtimeSinceStartup >= deadline) break;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");

            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            var animation = player.GetComponent(animationType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var isSwimming = characterType.GetProperty("IsSwimming");
            var grounded = characterType.GetProperty("IsGrounded");
            var action = animationType.GetProperty("Action");
            var importedVisualMotion = animationType.GetProperty("ImportedVisualActionMotionActive");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(importedVisualMotion, "动作层缺少导入蒙皮整体动作安全路径诊断。");

            var presentationType = FindLoadedType("Whalebone.Gameplay.D2.D20PlayerCandidatePresentationBridge");
            var presentation = presentationType == null ? null : player.GetComponent(presentationType);
            deadline = Time.realtimeSinceStartup + 3f;
            while (presentationType != null && presentation == null &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                presentation = player.GetComponent(presentationType);
            }

            var presentationInstalled = presentation != null &&
                (bool)presentationType.GetProperty("IsInstalled").GetValue(presentation);
            if (presentation != null && !presentationInstalled)
            {
                deadline = Time.realtimeSinceStartup + 3f;
                while (!(bool)presentationType.GetProperty("IsInstalled").GetValue(presentation) &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;
                presentationInstalled = (bool)presentationType.GetProperty("IsInstalled").GetValue(presentation);
            }

            bool hasSkinnedVisual = player.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0;
            Transform visualRoot = null;
            Quaternion visualRootBaseRotation = Quaternion.identity;
            if (hasSkinnedVisual && !presentationInstalled)
            {
                var visualRootField = animationType.GetField("_visualRoot",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var visualBaseRotationField = animationType.GetField("_visualRootBaseRotation",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                visualRoot = visualRootField == null ? null : visualRootField.GetValue(animation) as Transform;
                if (visualBaseRotationField != null)
                    visualRootBaseRotation = (Quaternion)visualBaseRotationField.GetValue(animation);
                Assert.IsNotNull(visualRoot, "导入蒙皮人物缺少视觉根，无法验证安全整体动作层。");
                Assert.IsTrue((bool)importedVisualMotion.GetValue(animation),
                    "导入蒙皮人物必须启用校准后的整体视觉动作层。");
            }
            else if (hasSkinnedVisual)
            {
                var usesGeneric = (bool)presentationType.GetProperty("UsesGenericPathAdapter").GetValue(presentation);
                var usesHumanoid = (bool)presentationType.GetProperty("UsesHumanoidPoseAdapter").GetValue(presentation);
                Assert.IsTrue(usesGeneric || usesHumanoid,
                    "当前 D20 蒙皮游泳表现必须由已登记的 presentation consumer 接管。");
                Assert.IsTrue((bool)presentationType.GetProperty("SwimmingVisualSupported").GetValue(presentation),
                    "当前 D20 presentation consumer 没有声明 Swimming 表现能力。");
            }

            setInputEnabled.Invoke(character, new object[] { false });
            // z=30 在 R03 岛外、海面安全范围内；只把角色移动到测试水域，
            // 不调用任何游泳/状态注入方法，让 Update 自己完成进入判定。
            setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, 30f) });
            deadline = Time.realtimeSinceStartup + 2f;
            while (!(bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                yield return null;

            Assert.IsTrue((bool)isSwimming.GetValue(character), "角色进入水域后没有进入游泳态。");
            Assert.IsFalse((bool)grounded.GetValue(character), "游泳态不能同时报告 grounded。");
            // 物理状态在 Update 切换，动作表现层在 LateUpdate 消费该状态。
            yield return null;
            Assert.AreEqual("Swimming", action.GetValue(animation).ToString(),
                "游泳态没有映射到 Swimming 动作状态。");
            if (hasSkinnedVisual && presentationInstalled)
            {
                Assert.AreEqual("Swimming", presentationType.GetProperty("LastD2Action").GetValue(presentation),
                    "D20 presentation consumer 没有消费 D2 Swimming 动作状态。");
                Assert.AreEqual("Fish", presentationType.GetProperty("LastCandidateState").GetValue(presentation).ToString(),
                    "D20 presentation consumer 没有把 D2 Swimming 映射到可执行表现状态。");
                if ((bool)presentationType.GetProperty("UsesHumanoidPoseAdapter").GetValue(presentation))
                {
                    var humanoidAdapter = presentationType.GetProperty("HumanoidPoseAdapter").GetValue(presentation);
                    Assert.IsNotNull(humanoidAdapter, "Humanoid presentation consumer 缺少 pose adapter。");
                    Assert.IsTrue((bool)humanoidAdapter.GetType().GetProperty("LastApplySucceeded").GetValue(humanoidAdapter),
                        "Humanoid presentation consumer 没有成功提交 Swimming pose。");
                    // The formal Avatar uses Stretch muscles where values near
                    // zero fold the imported elbows/knees. Keep this contract
                    // explicit so a logical Fish state cannot regress to the
                    // raised-arms/tucked-legs silhouette seen in the native
                    // Player evidence.
                    var poseField = humanoidAdapter.GetType().GetField("_pose",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    Assert.IsNotNull(poseField, "Humanoid presentation consumer 缺少当前 HumanPose 诊断。");
                    var pose = poseField.GetValue(humanoidAdapter);
                    var musclesField = pose.GetType().GetField("muscles");
                    Assert.IsNotNull(musclesField, "Humanoid presentation consumer 缺少 muscles 数据。");
                    var muscles = (float[])musclesField.GetValue(pose);
                    var bodyPositionField = pose.GetType().GetField("bodyPosition");
                    var restPoseField = humanoidAdapter.GetType().GetField("_restPose",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    Assert.IsNotNull(bodyPositionField, "Humanoid presentation consumer 缺少 bodyPosition 数据。");
                    Assert.IsNotNull(restPoseField, "Humanoid presentation consumer 缺少 neutral HumanPose 基线。");
                    var restPose = restPoseField.GetValue(humanoidAdapter);
                    var swimBodyPosition = (Vector3)bodyPositionField.GetValue(pose);
                    var restBodyPosition = (Vector3)bodyPositionField.GetValue(restPose);
                    var bodyDrop = swimBodyPosition.y - restBodyPosition.y;
                    Assert.That(bodyDrop, Is.InRange(-1.05f, -0.60f),
                        "正式游泳姿态没有把视觉骨架下沉到海面附近。");
                    Assert.Greater(muscles[42], 0.70f, "正式游泳姿态的左前臂 Stretch 过低，人物会折肘。");
                    Assert.Greater(muscles[51], 0.70f, "正式游泳姿态的右前臂 Stretch 过低，人物会折肘。");
                    Assert.Greater(muscles[24], 0.75f, "正式游泳姿态的左小腿 Stretch 过低，人物会收膝。");
                    Assert.Greater(muscles[32], 0.75f, "正式游泳姿态的右小腿 Stretch 过低，人物会收膝。");
                    Assert.Less(Mathf.Abs(muscles[37]), 0.25f, "正式游泳姿态的左肩抬举超出安全包络。");
                    Assert.Less(Mathf.Abs(muscles[46]), 0.25f, "正式游泳姿态的右肩抬举超出安全包络。");
                    var bodyPitch = (float)humanoidAdapter.GetType().GetProperty("LastBodyPitchDegrees").GetValue(humanoidAdapter);
                    Assert.That(bodyPitch, Is.InRange(55f, 75f), "正式游泳姿态的整体 pitch 超出安全包络。");
                }
            }
            else if (hasSkinnedVisual)
            {
                Assert.Greater(Quaternion.Angle(visualRootBaseRotation, visualRoot.localRotation), 25f,
                    "导入蒙皮人物进入游泳态后仍保持直立，缺少可见的安全游泳姿态。");
            }

            // 直接改变空间位置只用于构造“游回岸边”的下一帧环境，退出仍由
            // D2CharacterController 的水域/可行走地面裁决完成。
            player.transform.position = new Vector3(0f, 0.42f, 0f);
            Physics.SyncTransforms();
            deadline = Time.realtimeSinceStartup + 2f;
            while ((bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsFalse((bool)isSwimming.GetValue(character), "角色重新接触岛面后仍停留在游泳态。");
            if (hasSkinnedVisual && !presentationInstalled)
            {
                // Physics exits swimming in Update; give the presentation
                // layer one LateUpdate to consume the new action state.
                yield return null;
                Assert.Less(Quaternion.Angle(visualRootBaseRotation, visualRoot.localRotation), 1f,
                    "离开水域后导入蒙皮视觉根没有恢复 neutral 基线。");
            }
        }

        [UnityTest]
        public IEnumerator D17A04CharacterCrossesShorelineWithoutGettingStuck()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while (bootstrap == null || !(bool)ready.GetValue(bootstrap))
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                if (Time.realtimeSinceStartup >= deadline) break;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");

            var character = player.GetComponent(characterType);
            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var isSwimming = characterType.GetProperty("IsSwimming");
            var movementField = characterType.GetField("_moveDirection",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var applyPhysics = characterType.GetMethod("ApplyPhysicsMotion",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(movementField, "D2CharacterController 缺少移动意图字段。");
            Assert.IsNotNull(applyPhysics, "D2CharacterController 缺少物理步进入口。");

            setInputEnabled.Invoke(character, new object[] { true });
            bool entered = false;
            // z≈15.43 is the observed visual island edge in R03. Sweep a small
            // band around it to prove the controller predicts the shoreline
            // crossing instead of relying on an exact mesh vertex.
            for (float z = 14.5f; z <= 16.5f; z += 0.25f)
            {
                setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, z) });
                Physics.SyncTransforms();
                movementField.SetValue(character, Vector3.forward);
                applyPhysics.Invoke(character, null);
                if ((bool)isSwimming.GetValue(character))
                {
                    entered = true;
                    break;
                }
            }

            Assert.IsTrue(entered, "角色在岛面岸缘没有切换到游泳态，可能被 MeshCollider 卡住。");
            setInputEnabled.Invoke(character, new object[] { false });
        }

        [UnityTest]
        public IEnumerator D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var applyPhysics = characterType.GetMethod("ApplyPhysicsMotion",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var isSwimming = characterType.GetProperty("IsSwimming");
            var isGrounded = characterType.GetProperty("IsGrounded");
            var waterSurfaceY = characterType.GetProperty("WaterSurfaceY");
            var speed = characterType.GetProperty("CurrentSpeed");
            var moveDirection = characterType.GetField("_moveDirection",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var planarVelocity = characterType.GetField("_planarVelocity",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(applyPhysics, "D2CharacterController 缺少 ApplyPhysicsMotion。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(isGrounded, "D2CharacterController 缺少 IsGrounded。");
            Assert.IsNotNull(waterSurfaceY, "D2CharacterController 缺少 WaterSurfaceY。");
            Assert.IsNotNull(speed, "D2CharacterController 缺少 CurrentSpeed。");
            Assert.IsNotNull(moveDirection, "D2CharacterController 缺少移动方向字段。");
            Assert.IsNotNull(planarVelocity, "D2CharacterController 缺少水平速度字段。");

            Vector3 shorePosition = player.transform.position;
            setInputEnabled.Invoke(character, new object[] { true });

            for (int cycle = 0; cycle < 10; cycle++)
            {
                bool enteredShallow = false;
                for (float z = 14.5f; z <= 16.5f; z += 0.25f)
                {
                    setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, z) });
                    moveDirection.SetValue(character, Vector3.forward);
                    applyPhysics.Invoke(character, null);
                    if ((bool)isSwimming.GetValue(character))
                    {
                        enteredShallow = true;
                        break;
                    }
                }

                Assert.IsTrue(enteredShallow, "第 " + (cycle + 1) + " 次浅水岸缘没有进入游泳态。");
                Assert.IsFalse((bool)isGrounded.GetValue(character), "浅水游泳态不能同时 grounded。");
                Assert.That(Mathf.Abs(player.transform.position.y -
                    ((float)waterSurfaceY.GetValue(character) + 0.42f)), Is.LessThan(0.1f),
                    "浅水进入后角色没有稳定在水面身体偏移处。");

                moveDirection.SetValue(character, Vector3.zero);
                planarVelocity.SetValue(character, Vector3.zero);
                player.transform.position = shorePosition;
                Physics.SyncTransforms();
                deadline = Time.realtimeSinceStartup + 2f;
                while ((bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsFalse((bool)isSwimming.GetValue(character),
                    "第 " + (cycle + 1) + " 次浅水离岸没有退出游泳态。");

                setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, 30f) });
                moveDirection.SetValue(character, Vector3.zero);
                deadline = Time.realtimeSinceStartup + 2f;
                while (!(bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)isSwimming.GetValue(character),
                    "第 " + (cycle + 1) + " 次深水没有进入游泳态。");
                float startZ = player.transform.position.z;
                moveDirection.SetValue(character, Vector3.back);
                applyPhysics.Invoke(character, null);
                float displacement = startZ - player.transform.position.z;
                Assert.Greater(displacement, 0.001f,
                    "第 " + (cycle + 1) + " 次深水游泳没有产生水平位移。");
                Assert.Greater((float)speed.GetValue(character), 0f,
                    "第 " + (cycle + 1) + " 次深水游泳速度保持为零。");
                Assert.That(Mathf.Abs(player.transform.position.y -
                    ((float)waterSurfaceY.GetValue(character) + 0.42f)), Is.LessThan(0.1f),
                    "深水游泳后角色没有稳定在水面身体偏移处。");

                moveDirection.SetValue(character, Vector3.zero);
                planarVelocity.SetValue(character, Vector3.zero);
                player.transform.position = shorePosition;
                Physics.SyncTransforms();
                deadline = Time.realtimeSinceStartup + 2f;
                while ((bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsFalse((bool)isSwimming.GetValue(character),
                    "第 " + (cycle + 1) + " 次深水离岸没有退出游泳态。");
                Assert.IsTrue((bool)isGrounded.GetValue(character),
                    "第 " + (cycle + 1) + " 次深水上岸后没有恢复 grounded。");
            }

            setInputEnabled.Invoke(character, new object[] { false });
        }

        [UnityTest]
        public IEnumerator D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            var animation = player.GetComponent(animationType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var isSwimming = characterType.GetProperty("IsSwimming");
            var action = animationType.GetProperty("Action");
            var moveDirection = characterType.GetField("_moveDirection",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var planarVelocity = characterType.GetField("_planarVelocity",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var visualRootField = animationType.GetField("_visualRoot",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var visualBasePositionField = animationType.GetField("_visualRootBasePosition",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var visualBaseRotationField = animationType.GetField("_visualRootBaseRotation",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var importedNeutral = animationType.GetProperty("ImportedSkinnedRigNeutralOnly");
            var swimmingPitch = animationType.GetProperty("SwimmingVisualPitchDegrees");
            var swimmingRoll = animationType.GetProperty("SwimmingVisualRollDegrees");
            var swimmingBodyDrop = animationType.GetProperty("SwimmingVisualBodyDrop");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(action, "动作控制器缺少 Action。");
            Assert.IsNotNull(moveDirection, "D2CharacterController 缺少移动方向字段。");
            Assert.IsNotNull(planarVelocity, "D2CharacterController 缺少水平速度字段。");
            Assert.IsNotNull(visualRootField, "动作控制器缺少 visualRoot 诊断字段。");
            Assert.IsNotNull(visualBasePositionField, "动作控制器缺少视觉根基线位置诊断字段。");
            Assert.IsNotNull(visualBaseRotationField, "动作控制器缺少视觉根基线诊断字段。");
            Assert.IsNotNull(importedNeutral, "动作控制器缺少 imported neutral 诊断。");
            Assert.IsNotNull(swimmingPitch, "动作控制器缺少程序化游泳 pitch 诊断。");
            Assert.IsNotNull(swimmingRoll, "动作控制器缺少程序化游泳 roll 诊断。");
            Assert.IsNotNull(swimmingBodyDrop, "动作控制器缺少程序化游泳下沉诊断。");

            var visualRoot = visualRootField.GetValue(animation) as Transform;
            var basePosition = (Vector3)visualBasePositionField.GetValue(animation);
            var baseRotation = (Quaternion)visualBaseRotationField.GetValue(animation);
            Assert.IsNotNull(visualRoot, "当前玩家没有可验证的 visualRoot。");
            bool importedRig = (bool)importedNeutral.GetValue(animation);
            Transform proceduralArm = player.transform.Find("HumanoidVisual/Hips/Spine/LeftUpperArm");
            Transform proceduralForearm = player.transform.Find("HumanoidVisual/Hips/Spine/LeftUpperArm/LeftLowerArm");
            Transform proceduralLeg = player.transform.Find("HumanoidVisual/Hips/LeftUpperLeg");
            Quaternion proceduralArmBase = proceduralArm == null ? Quaternion.identity : proceduralArm.localRotation;
            Quaternion proceduralForearmBase = proceduralForearm == null ? Quaternion.identity : proceduralForearm.localRotation;
            Quaternion proceduralLegBase = proceduralLeg == null ? Quaternion.identity : proceduralLeg.localRotation;
            Vector3 shorePosition = player.transform.position;
            bool proceduralLegMotionObserved = false;
            setInputEnabled.Invoke(character, new object[] { true });

            for (int cycle = 0; cycle < 10; cycle++)
            {
                setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, 30f) });
                moveDirection.SetValue(character, Vector3.zero);
                deadline = Time.realtimeSinceStartup + 2f;
                while (!(bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)isSwimming.GetValue(character),
                    "第 " + (cycle + 1) + " 次游泳视觉姿态构造没有进入游泳态。");
                yield return null;

                Assert.AreEqual("Swimming", action.GetValue(animation).ToString(),
                    "第 " + (cycle + 1) + " 次游泳态没有消费 Swimming 动作。");
                if (importedRig)
                {
                    Assert.Greater(Quaternion.Angle(baseRotation, visualRoot.localRotation), 25f,
                        "第 " + (cycle + 1) + " 次游泳可见根仍接近直立，缺少安全整体验证姿态。");
                    Assert.Less(Quaternion.Angle(baseRotation, visualRoot.localRotation), 42f,
                        "第 " + (cycle + 1) + " 次游泳视觉根倾斜过大，人物会横躺或扭曲。");
                    Assert.Greater(visualRoot.localPosition.y, basePosition.y - 0.16f,
                        "第 " + (cycle + 1) + " 次游泳视觉根下沉过大，人物会沉入水面。");
                }
                else
                {
                    float pitch = (float)swimmingPitch.GetValue(animation);
                    float roll = (float)swimmingRoll.GetValue(animation);
                    float bodyDrop = (float)swimmingBodyDrop.GetValue(animation);
                    Assert.Greater(Quaternion.Angle(baseRotation, visualRoot.localRotation), 58f,
                        "程序化游泳视觉根仍接近直立，游泳姿态不可读。");
                    Assert.Less(Quaternion.Angle(baseRotation, visualRoot.localRotation), 70f,
                        "程序化游泳视觉根倾斜过大，人物可能翻倒或扭曲。");
                    Assert.Greater(visualRoot.localPosition.y, basePosition.y - 0.15f,
                        "程序化游泳视觉根下沉过大，归一化 fallback 的身体会沉入水下。");
                    Assert.Less(visualRoot.localPosition.y, basePosition.y + 0.20f,
                        "程序化游泳视觉根抬得过高，人物会漂在水面上方。");
                    Assert.That(pitch, Is.InRange(58f, 70f),
                        "程序化游泳 pitch 超出已验证的可读姿态范围。");
                    Assert.Less(Mathf.Abs(roll), 0.01f,
                        "程序化 fallback 不应叠加横向 roll，避免动作轮廓扭曲。");
                    Assert.That(bodyDrop, Is.InRange(-0.70f, -0.50f),
                        "程序化游泳原始身体下沉参数偏离已验证的水面姿态范围。");
                    Assert.IsNotNull(proceduralArm, "程序化玩家缺少左上臂动作节点。");
                    Assert.IsNotNull(proceduralForearm, "程序化玩家缺少左前臂动作节点。");
                    Assert.IsNotNull(proceduralLeg, "程序化玩家缺少左大腿动作节点。");
                    float maxArmMotion = 0f;
                    float maxForearmMotion = 0f;
                    float maxLegMotion = 0f;
                    for (int sample = 0; sample < 8; sample++)
                    {
                        yield return null;
                        maxArmMotion = Mathf.Max(maxArmMotion,
                            Quaternion.Angle(proceduralArmBase, proceduralArm.localRotation));
                        maxForearmMotion = Mathf.Max(maxForearmMotion,
                            Quaternion.Angle(proceduralForearmBase, proceduralForearm.localRotation));
                        maxLegMotion = Mathf.Max(maxLegMotion,
                            Quaternion.Angle(proceduralLegBase, proceduralLeg.localRotation));
                    }
                    Debug.Log("[D17/A08] cycle=" + (cycle + 1) +
                        ", rootAngle=" + Quaternion.Angle(baseRotation, visualRoot.localRotation).ToString("F2") +
                        ", rootOffsetY=" + (visualRoot.localPosition.y - basePosition.y).ToString("F3") +
                        ", maxArmMotion=" + maxArmMotion.ToString("F2") +
                        ", maxForearmMotion=" + maxForearmMotion.ToString("F2") +
                        ", maxLegMotion=" + maxLegMotion.ToString("F2"));
                    proceduralLegMotionObserved |= maxLegMotion > 0.2f;
                    Assert.Greater(maxArmMotion, 0.5f,
                        "程序化游泳左上臂在采样窗口内没有动作，可能退化为静态姿势。");
                    Assert.Less(maxArmMotion, 175f,
                        "程序化游泳左上臂超出明确的前伸轴包络，人物会翻折。");
                    Assert.Greater(maxForearmMotion, 0.5f,
                        "程序化游泳左前臂没有参与前伸，泳姿会退化为站立手臂。");
                    Assert.Less(maxForearmMotion, 90f,
                        "程序化游泳左前臂动作幅度过大，人物会扭曲。");
                    Assert.Less(maxLegMotion, 32f,
                        "程序化游泳左腿动作幅度过大，人物会扭曲。");

                    // The generated fallback arm bones start by hanging down
                    // from the spine.  The swim visual-root pitch is applied
                    // before the bone layer, so a sign-only arm swing can
                    // still leave both hands below the torso.  Assert the
                    // actual world-space hand direction against the fallback
                    // head/torso direction to protect the corrected axis
                    // composition without making a claim about imported rigs.
                    Transform proceduralHand = player.transform.Find(
                        "HumanoidVisual/Hips/Spine/LeftUpperArm/LeftLowerArm/LeftHandMesh");
                    Transform proceduralHead = player.transform.Find(
                        "HumanoidVisual/Hips/Spine/Neck/Head");
                    Transform proceduralHips = player.transform.Find("HumanoidVisual/Hips");
                    Assert.IsNotNull(proceduralHand, "程序化玩家缺少左手动作节点。");
                    Assert.IsNotNull(proceduralHead, "程序化玩家缺少头部动作节点。");
                    Assert.IsNotNull(proceduralHips, "程序化玩家缺少髋部动作节点。");
                    Vector3 handDirection = (proceduralHand.position - proceduralArm.position).normalized;
                    Vector3 headDirection = (proceduralHead.position - proceduralHips.position).normalized;
                    float handHeadAlignment = Vector3.Dot(handDirection, headDirection);
                    Debug.Log("[D17/A08] swim_hand_head_alignment=" +
                        handHeadAlignment.ToString("F3"));
                    Assert.Greater(handHeadAlignment, 0.55f,
                        "程序化游泳手臂仍指向躯干/脚部方向，未形成向头部方向的前伸轮廓。");
                }
                Assert.Greater(Vector3.Dot(player.transform.up, Vector3.up), 0.999f,
                    "第 " + (cycle + 1) + " 次游泳不应旋转物理玩家根或碰撞胶囊。");

                moveDirection.SetValue(character, Vector3.zero);
                planarVelocity.SetValue(character, Vector3.zero);
                player.transform.position = shorePosition;
                Physics.SyncTransforms();
                deadline = Time.realtimeSinceStartup + 2f;
                while ((bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsFalse((bool)isSwimming.GetValue(character),
                    "第 " + (cycle + 1) + " 次离水没有退出游泳态。");
                yield return null;
                if (importedRig)
                {
                    Assert.Less(Quaternion.Angle(baseRotation, visualRoot.localRotation), 1f,
                        "第 " + (cycle + 1) + " 次离水后 visualRoot 没有恢复 neutral 基线。");
                }
                else
                {
                    Assert.Less(Quaternion.Angle(baseRotation, visualRoot.localRotation), 1f,
                        "第 " + (cycle + 1) + " 次离水后程序化 visualRoot 没有恢复 neutral 基线。");
                    deadline = Time.realtimeSinceStartup + 1.0f;
                    while ((Quaternion.Angle(proceduralArmBase, proceduralArm.localRotation) >= 1f ||
                            Quaternion.Angle(proceduralLegBase, proceduralLeg.localRotation) >= 1f) &&
                           Time.realtimeSinceStartup < deadline)
                    {
                        yield return null;
                    }
                    Assert.Less(Quaternion.Angle(proceduralArmBase, proceduralArm.localRotation), 2f,
                        "第 " + (cycle + 1) + " 次离水后程序化左上臂没有恢复 idle 基线。");
                    Assert.Less(Quaternion.Angle(proceduralLegBase, proceduralLeg.localRotation), 1f,
                        "第 " + (cycle + 1) + " 次离水后程序化左腿没有恢复 neutral 基线。");
                }
            }

            if (!importedRig)
            {
                Assert.IsTrue(proceduralLegMotionObserved,
                    "程序化游泳左腿在十次采样循环中始终没有动作，可能退化为静态姿势。");
            }

            setInputEnabled.Invoke(character, new object[] { false });
        }

        [UnityTest]
        public IEnumerator D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var tryJump = characterType.GetMethod("TryJump");
            var isGrounded = characterType.GetProperty("IsGrounded");
            var isSwimming = characterType.GetProperty("IsSwimming");
            var moveDirection = characterType.GetField("_moveDirection",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var applyPhysics = characterType.GetMethod("ApplyPhysicsMotion",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(tryJump, "D2CharacterController 缺少 TryJump。");
            Assert.IsNotNull(isGrounded, "D2CharacterController 缺少 IsGrounded。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(moveDirection, "D2CharacterController 缺少移动方向字段。");
            Assert.IsNotNull(applyPhysics, "D2CharacterController 缺少 ApplyPhysicsMotion。");

            setInputEnabled.Invoke(character, new object[] { true });
            deadline = Time.realtimeSinceStartup + 3f;
            while (!(bool)isGrounded.GetValue(character) && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsTrue((bool)isGrounded.GetValue(character), "跳跃回归开始前玩家没有落地。");

            for (int cycle = 0; cycle < 10; cycle++)
            {
                Assert.IsTrue((bool)tryJump.Invoke(character, null),
                    "第 " + (cycle + 1) + " 次跳跃没有被地面状态接受。");
                deadline = Time.realtimeSinceStartup + 3f;
                while (!(bool)isGrounded.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)isGrounded.GetValue(character),
                    "第 " + (cycle + 1) + " 次跳跃没有恢复 grounded。");
                Assert.IsFalse((bool)isSwimming.GetValue(character),
                    "第 " + (cycle + 1) + " 次跳跃错误切换成游泳态。");
                Assert.IsFalse(float.IsNaN(player.transform.position.y) ||
                               float.IsInfinity(player.transform.position.y) ||
                               player.transform.position.y <= -8f,
                    "第 " + (cycle + 1) + " 次跳跃后玩家位置越界或非有限。");
            }

            // 保持在出生点附近的真实岛面上，做小步正/斜/反向移动；不把角色
            // 传送到墙内，也不关闭碰撞，确保 CharacterController.Move 的接地
            // 和方向合成路径在连续小步中保持稳定。
            Vector3 origin = player.transform.position;
            // Use two exact reverse pairs. The previous sequence contained
            // forward/right/diagonal steps whose net direction was non-zero,
            // so a correct controller could be rejected as "drift".
            Vector3[] directions =
            {
                Vector3.forward,
                (Vector3.forward + Vector3.right).normalized,
                Vector3.back,
                (Vector3.back + Vector3.left).normalized
            };
            for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
            {
                for (int step = 0; step < 8; step++)
                {
                    moveDirection.SetValue(character, directions[directionIndex]);
                    applyPhysics.Invoke(character, null);
                    Assert.IsFalse((bool)isSwimming.GetValue(character),
                        "出生点附近的第 " + directionIndex + "/" + step + " 步错误进入游泳态。");
                    Assert.IsTrue((bool)isGrounded.GetValue(character),
                        "出生点附近的第 " + directionIndex + "/" + step + " 步失去接地。");
                    Assert.IsFalse(float.IsNaN(player.transform.position.x) ||
                                   float.IsInfinity(player.transform.position.x) ||
                                   float.IsNaN(player.transform.position.y) ||
                                   float.IsInfinity(player.transform.position.y) ||
                                   float.IsNaN(player.transform.position.z) ||
                                   float.IsInfinity(player.transform.position.z),
                        "连续移动产生非有限 Transform。");
                }
            }

            Assert.Less(Vector3.Distance(player.transform.position, origin), 2.5f,
                "小步往返移动后玩家偏离出生点过远，可能存在速度/方向累积。");
            setInputEnabled.Invoke(character, new object[] { false });
        }

        [UnityTest]
        public IEnumerator D17A10TerrainDockAndGangplankSupportBidirectionalWalkSteps()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            var body = player.GetComponent<CharacterController>();
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(body, "玩家缺少 CharacterController。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var isGrounded = characterType.GetProperty("IsGrounded");
            var isSwimming = characterType.GetProperty("IsSwimming");
            var moveDirection = characterType.GetField("_moveDirection",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var applyPhysics = characterType.GetMethod("ApplyPhysicsMotion",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(isGrounded, "D2CharacterController 缺少 IsGrounded。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(moveDirection, "D2CharacterController 缺少移动方向字段。");
            Assert.IsNotNull(applyPhysics, "D2CharacterController 缺少 ApplyPhysicsMotion。");

            Collider terrain = FindActiveCollider("D19_TerrainRoot");
            Collider dock = FindActiveCollider("DockGround");
            Collider gangplank = FindActiveCollider("GangplankGround");
            Assert.IsNotNull(terrain, "InitialHarbor 缺少 D19_TerrainRoot MeshCollider。");
            Assert.IsNotNull(dock, "InitialHarbor 缺少 DockGround 可行走碰撞体。");
            Assert.IsNotNull(gangplank, "InitialHarbor 缺少 GangplankGround 可行走碰撞体。");
            Assert.IsTrue(terrain.enabled && !terrain.isTrigger, "D19_TerrainRoot 必须是启用的实体碰撞。");
            Assert.IsTrue(dock.enabled && !dock.isTrigger, "DockGround 必须是启用的实体碰撞。");
            Assert.IsTrue(gangplank.enabled && !gangplank.isTrigger, "GangplankGround 必须是启用的实体碰撞。");

            setInputEnabled.Invoke(character, new object[] { true });
            Collider[] routes = { terrain, dock, gangplank };
            for (int routeIndex = 0; routeIndex < routes.Length; routeIndex++)
            {
                var route = routes[routeIndex];
                Vector3 rayOrigin = route.bounds.center + Vector3.up * 8f;
                Ray ray = new Ray(rayOrigin, Vector3.down);
                RaycastHit hit;
                Assert.IsTrue(route.Raycast(ray, out hit, 16f),
                    "路线 " + route.name + " 没有返回自身可行走表面射线命中。");

                setWorldPosition.Invoke(character, new object[] { hit.point + Vector3.up * 2f });
                Physics.SyncTransforms();
                // SetWorldPosition intentionally resets D2's logical state,
                // but Unity's CharacterController can retain its previous
                // contact until the next simulation frame. Wait for the
                // actual body contact instead of accepting the stale flag.
                for (int settleFrame = 0; settleFrame < 2; settleFrame++)
                    yield return null;
                deadline = Time.realtimeSinceStartup + 3f;
                while ((!body.isGrounded || !(bool)isGrounded.GetValue(character)) &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue(body.isGrounded && (bool)isGrounded.GetValue(character),
                    "路线 " + route.name + " 的表面无法让玩家恢复接地。");
                Assert.IsFalse((bool)isSwimming.GetValue(character),
                    "路线 " + route.name + " 的可行走表面错误进入游泳态。");

                Vector3[] directions = { Vector3.forward, Vector3.back };
                for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    for (int step = 0; step < 8; step++)
                    {
                        moveDirection.SetValue(character, directions[directionIndex]);
                        applyPhysics.Invoke(character, null);
                        Assert.IsTrue(body.isGrounded && (bool)isGrounded.GetValue(character),
                            "路线 " + route.name + " 方向 " + directionIndex + " 第 " + step + " 步失去接地。");
                        Assert.IsFalse((bool)isSwimming.GetValue(character),
                            "路线 " + route.name + " 方向 " + directionIndex + " 第 " + step + " 步进入游泳态。");
                        Assert.IsFalse(float.IsNaN(player.transform.position.x) ||
                                       float.IsInfinity(player.transform.position.x) ||
                                       float.IsNaN(player.transform.position.y) ||
                                       float.IsInfinity(player.transform.position.y) ||
                                       float.IsNaN(player.transform.position.z) ||
                                       float.IsInfinity(player.transform.position.z),
                            "路线 " + route.name + " 产生非有限玩家位置。");
                    }
                }
            }

            setInputEnabled.Invoke(character, new object[] { false });
        }

        [UnityTest]
        public IEnumerator D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var deckType = FindLoadedType("Whalebone.Gameplay.D7.D7ShipDeckController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            Assert.IsNotNull(deckType, "D7ShipDeckController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");

            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            var shipVisual = GameObject.Find("D19_R03_ShipVisualInstance");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(ship, "InitialHarbor 稳定船对象没有创建。");
            Assert.IsNotNull(shipVisual, "InitialHarbor 可见船实例没有创建。");

            var character = player.GetComponent(characterType);
            var body = player.GetComponent<CharacterController>();
            var router = Object.FindFirstObjectByType(routerType,
                FindObjectsInactive.Include) as Component;
            var deck = ship.GetComponent(deckType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(body, "玩家缺少 CharacterController。");
            Assert.IsNotNull(router, "D2ControlRouter 没有创建。");
            Assert.IsNotNull(deck, "稳定船对象缺少 D7ShipDeckController。");

            var helmProperty = deckType.GetProperty("HelmPoint");
            var deckReadyProperty = deckType.GetProperty("IsReady");
            var boardingProperty = deckType.GetProperty("PlayerBoarded");
            var modeProperty = routerType.GetProperty("Mode");
            var tryToggleShip = routerType.GetMethod("TryToggleShip");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var snapToGround = characterType.GetMethod("SnapToGround");
            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            Assert.IsNotNull(helmProperty, "甲板控制器缺少 HelmPoint。");
            Assert.IsNotNull(deckReadyProperty, "甲板控制器缺少 IsReady。");
            Assert.IsNotNull(boardingProperty, "甲板控制器缺少 PlayerBoarded。");
            Assert.IsNotNull(modeProperty, "控制路由缺少 Mode。");
            Assert.IsNotNull(tryToggleShip, "D2ControlRouter 缺少 TryToggleShip。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(snapToGround, "D2CharacterController 缺少 SnapToGround。");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");

            for (int settle = 0; settle < 2; settle++) yield return null;
            var helm = helmProperty.GetValue(deck) as Transform;
            Assert.IsTrue((bool)deckReadyProperty.GetValue(deck), "D7 甲板锚点没有完成初始化。");
            Assert.IsNotNull(helm, "D7 甲板控制器没有生成 HelmPoint。");
            Assert.AreEqual(ship.transform, helm.parent,
                "HelmPoint 必须挂在稳定 gameplay ship root，不能挂在可见船体子节点。");
            Assert.IsFalse(helm.IsChildOf(shipVisual.transform),
                "HelmPoint 不能挂在 R03 可见船体子节点下。");

            var authoredVisibleHelmAnchor = shipVisual.transform.Find("D7_VisibleHelmAnchor");
            Assert.IsNotNull(authoredVisibleHelmAnchor,
                "R03 可见船体必须发布 D7_VisibleHelmAnchor，供稳定船根消费。");
            Assert.Less(Vector3.Distance(helm.position, authoredVisibleHelmAnchor.position), 0.02f,
                "HelmPoint 没有消费 R03 作者可见舵位锚点，玩家会落在船外。");

            Vector3 shipScale = ship.transform.lossyScale;
            Assert.Greater(shipScale.x, 1f, "Q09 必须覆盖当前场景的非单位船根缩放。");
            Assert.Greater(shipScale.y, 1f, "Q09 必须覆盖当前场景的非单位船根缩放。");
            Vector3 approachPosition = player.transform.position;
            Assert.IsNull(player.transform.parent, "登船前玩家不能已经挂在船上。");
            Assert.AreEqual("Character", modeProperty.GetValue(router).ToString(),
                "登船前控制模式必须是 Character。");

            var deckController = deck as Component;
            var isWithinRange = deckType.GetMethod("IsWithinBoardingRange");
            Assert.IsNotNull(isWithinRange, "D7 甲板控制器缺少登船范围判断。");
            Assert.IsTrue((bool)isWithinRange.Invoke(deckController,
                    new object[] { player.transform, 8f }),
                "InitialHarbor 玩家出生点必须处于合法静止登船范围内。");

            // 先验证远离船体的拒绝路径；失败不得改变 parent 或控制模式。
            Vector3 remotePosition = ship.transform.position + Vector3.right * 30f + Vector3.up * 1.1f;
            setWorldPosition.Invoke(character, new object[] { remotePosition });
            bool remoteResult = (bool)tryToggleShip.Invoke(router, null);
            Assert.IsFalse(remoteResult, "远离船体时 TryToggleShip 不能成功。");
            Assert.IsNull(player.transform.parent, "远离拒绝后玩家 parent 不能被改成船。");
            Assert.AreEqual("Character", modeProperty.GetValue(router).ToString(),
                "远离拒绝后控制模式不能离开 Character。");

            setWorldPosition.Invoke(character, new object[] { approachPosition });
            Assert.IsTrue((bool)snapToGround.Invoke(character, new object[] { 8f }),
                "恢复到合法登船点后玩家没有回到可行走地面。");
            setInputEnabled.Invoke(character, new object[] { true });
            for (int settle = 0; settle < 2; settle++) yield return null;

            float maxDeckHeightDelta = 0f;
            float maxDeckAnchorDistance = 0f;
            for (int cycle = 0; cycle < 10; cycle++)
            {
                Assert.IsTrue((bool)isWithinRange.Invoke(deckController,
                        new object[] { player.transform, 8f }),
                    "第 " + cycle + " 次登船前玩家不在合法范围。");
                Assert.IsTrue((bool)tryToggleShip.Invoke(router, null),
                    "第 " + cycle + " 次登船没有成功。");
                for (int settle = 0; settle < 2; settle++) yield return null;

                Assert.AreEqual("Ship", modeProperty.GetValue(router).ToString(),
                    "第 " + cycle + " 次登船后控制模式不是 Ship。");
                Assert.AreEqual(ship.transform, player.transform.parent,
                    "第 " + cycle + " 次登船后玩家没有挂到稳定船根。");
                Assert.IsFalse(body.enabled,
                    "第 " + cycle + " 次登船后 CharacterController 必须暂时关闭，不能和船体碰撞竞争。");
                Assert.IsTrue((bool)boardingProperty.GetValue(deck),
                    "第 " + cycle + " 次登船后甲板控制器没有记录 PlayerBoarded。");

                float heightDelta = Mathf.Abs(player.transform.position.y - helm.position.y);
                float anchorDistance = Vector3.Distance(player.transform.position, helm.position);
                maxDeckHeightDelta = Mathf.Max(maxDeckHeightDelta, heightDelta);
                maxDeckAnchorDistance = Mathf.Max(maxDeckAnchorDistance, anchorDistance);
                Assert.Less(heightDelta, 0.05f,
                    "第 " + cycle + " 次登船产生了船根缩放导致的抬升。");
                Assert.Less(anchorDistance, 0.05f,
                    "第 " + cycle + " 次登船玩家没有落在 HelmPoint。");
                Assert.AreEqual(shipScale, ship.transform.lossyScale,
                    "重复登船不能修改稳定船根缩放。");

                Assert.IsTrue((bool)tryToggleShip.Invoke(router, null),
                    "第 " + cycle + " 次下船没有成功。");
                for (int settle = 0; settle < 2; settle++) yield return null;
                Assert.AreEqual("Character", modeProperty.GetValue(router).ToString(),
                    "第 " + cycle + " 次下船后控制模式没有恢复 Character。");
                Assert.IsNull(player.transform.parent,
                    "第 " + cycle + " 次下船后玩家仍挂在船根。");
                Assert.IsTrue(body.enabled,
                    "第 " + cycle + " 次下船后 CharacterController 没有恢复。");
                Assert.IsFalse((bool)boardingProperty.GetValue(deck),
                    "第 " + cycle + " 次下船后甲板控制器仍记录 PlayerBoarded。");

                if (cycle < 9)
                {
                    setWorldPosition.Invoke(character, new object[] { approachPosition });
                    Assert.IsTrue((bool)snapToGround.Invoke(character, new object[] { 8f }),
                        "第 " + cycle + " 次下船后回到登船点失败。");
                    setInputEnabled.Invoke(character, new object[] { true });
                    for (int settle = 0; settle < 2; settle++) yield return null;
                }
            }

            Debug.Log("[D17/A11] boarding_cycles=10, maxDeckHeightDelta=" +
                maxDeckHeightDelta.ToString("F4") + ", maxDeckAnchorDistance=" +
                maxDeckAnchorDistance.ToString("F4") + ", shipScale=" + shipScale);
        }

        [UnityTest]
        public IEnumerator D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var shipControllerType = FindLoadedType("Whalebone.Gameplay.D2.D2ShipController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            Assert.IsNotNull(shipControllerType, "D2ShipController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(ship, "InitialHarbor 稳定船对象没有创建。");

            var router = Object.FindFirstObjectByType(routerType,
                FindObjectsInactive.Include) as Component;
            var shipController = ship.GetComponent(shipControllerType);
            Assert.IsNotNull(router, "D2ControlRouter 没有创建。");
            Assert.IsNotNull(shipController, "稳定船对象缺少 D2ShipController。");

            var tryToggleShip = routerType.GetMethod("TryToggleShip");
            var modeProperty = routerType.GetProperty("Mode");
            var snapshotProperty = shipControllerType.GetProperty("Snapshot");
            var applySnapshot = shipControllerType.GetMethod("ApplySnapshot");
            Assert.IsNotNull(tryToggleShip, "D2ControlRouter 缺少 TryToggleShip。");
            Assert.IsNotNull(modeProperty, "D2ControlRouter 缺少 Mode。");
            Assert.IsNotNull(snapshotProperty, "D2ShipController 缺少 Snapshot。");
            Assert.IsNotNull(applySnapshot, "D2ShipController 缺少 ApplySnapshot。");

            for (int settle = 0; settle < 2; settle++) yield return null;
            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "Q10 无法进入 Ship 控制模式。");
            for (int settle = 0; settle < 2; settle++) yield return null;
            Assert.AreEqual("Ship", modeProperty.GetValue(router).ToString(),
                "航行关系检查开始前控制模式不是 Ship。");
            Assert.AreEqual(ship.transform, player.transform.parent,
                "航行关系检查开始前玩家没有挂在稳定船根。");

            var body = player.GetComponent<CharacterController>();
            Assert.IsNotNull(body, "玩家缺少 CharacterController。");
            Assert.IsFalse(body.enabled, "船控期间 CharacterController 必须保持关闭。");

            var baselineSnapshot = snapshotProperty.GetValue(shipController);
            Assert.IsNotNull(baselineSnapshot, "D2ShipController 没有可读取的航行快照。");
            var snapshotType = baselineSnapshot.GetType();
            var clone = snapshotType.GetMethod("Clone");
            var positionX = snapshotType.GetField("PositionX");
            var positionZ = snapshotType.GetField("PositionZ");
            var heading = snapshotType.GetField("Heading");
            var speed = snapshotType.GetField("Speed");
            var controlMode = snapshotType.GetField("ControlMode");
            Assert.IsNotNull(clone, "SailingSnapshot 缺少 Clone。");
            Assert.IsNotNull(positionX, "SailingSnapshot 缺少 PositionX。");
            Assert.IsNotNull(positionZ, "SailingSnapshot 缺少 PositionZ。");
            Assert.IsNotNull(heading, "SailingSnapshot 缺少 Heading。");
            Assert.IsNotNull(speed, "SailingSnapshot 缺少 Speed。");
            Assert.IsNotNull(controlMode, "SailingSnapshot 缺少 ControlMode。");

            Vector3 boardedLocalPosition = player.transform.localPosition;
            Quaternion boardedLocalRotation = player.transform.localRotation;
            Vector3 startingShipPosition = ship.transform.position;
            float startingHeading = ship.transform.eulerAngles.y;
            object shipMode = System.Enum.Parse(controlMode.FieldType, "Ship");
            float maximumRelativePositionError = 0f;
            float maximumRelativeRotationError = 0f;
            float maximumShipTranslation = 0f;
            float maximumShipHeadingDelta = 0f;

            for (int sample = 0; sample < 10; sample++)
            {
                var snapshot = clone.Invoke(baselineSnapshot, null);
                float translationX = (sample % 4) * 0.45f;
                float translationZ = ((sample % 3) - 1) * 0.65f;
                float headingDelta = sample % 2 == 0 ? sample * 4f : -sample * 3f;
                positionX.SetValue(snapshot, startingShipPosition.x + translationX);
                positionZ.SetValue(snapshot, startingShipPosition.z + translationZ);
                heading.SetValue(snapshot, startingHeading + headingDelta);
                speed.SetValue(snapshot, 0.5f + sample * 0.2f);
                controlMode.SetValue(snapshot, shipMode);
                applySnapshot.Invoke(shipController, new[] { snapshot });
                Physics.SyncTransforms();
                yield return null;

                Vector3 expectedWorldPosition = ship.transform.TransformPoint(boardedLocalPosition);
                Quaternion expectedWorldRotation = ship.transform.rotation * boardedLocalRotation;
                float positionError = Vector3.Distance(player.transform.position, expectedWorldPosition);
                float rotationError = Quaternion.Angle(player.transform.rotation, expectedWorldRotation);
                float shipTranslation = Vector3.Distance(ship.transform.position, startingShipPosition);
                float shipHeadingDelta = Mathf.Abs(Mathf.DeltaAngle(ship.transform.eulerAngles.y, startingHeading));
                maximumRelativePositionError = Mathf.Max(maximumRelativePositionError, positionError);
                maximumRelativeRotationError = Mathf.Max(maximumRelativeRotationError, rotationError);
                maximumShipTranslation = Mathf.Max(maximumShipTranslation, shipTranslation);
                maximumShipHeadingDelta = Mathf.Max(maximumShipHeadingDelta, shipHeadingDelta);

                Assert.AreEqual("Ship", modeProperty.GetValue(router).ToString(),
                    "第 " + sample + " 个航行样本改变了控制模式。");
                Assert.AreEqual(ship.transform, player.transform.parent,
                    "第 " + sample + " 个航行样本改变了玩家 parent。");
                Assert.IsFalse(body.enabled,
                    "第 " + sample + " 个航行样本重新启用了 CharacterController。");
                Assert.Less(positionError, 0.001f,
                    "第 " + sample + " 个航行样本中玩家没有跟随船根的相对甲板位置。");
                Assert.Less(rotationError, 0.001f,
                    "第 " + sample + " 个航行样本中玩家没有跟随船根的相对甲板旋转。");
            }

            applySnapshot.Invoke(shipController, new[] { baselineSnapshot });
            Physics.SyncTransforms();
            for (int settle = 0; settle < 2; settle++) yield return null;
            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "恢复原航行快照后无法安全下船。");
            for (int settle = 0; settle < 2; settle++) yield return null;
            Assert.AreEqual("Character", modeProperty.GetValue(router).ToString(),
                "航行关系检查结束后控制模式没有恢复 Character。");
            Assert.IsNull(player.transform.parent, "航行关系检查结束后玩家仍挂在船根。");
            Assert.IsTrue(body.enabled, "航行关系检查结束后 CharacterController 没有恢复。");

            Debug.Log("[D17/A12] motion_samples=10, maxRelativePositionError=" +
                maximumRelativePositionError.ToString("F6") + ", maxRelativeRotationError=" +
                maximumRelativeRotationError.ToString("F6") + ", maxShipTranslation=" +
                maximumShipTranslation.ToString("F3") + ", maxShipHeadingDelta=" +
                maximumShipHeadingDelta.ToString("F3"));
        }

        [UnityTest]
        public IEnumerator D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var shipControllerType = FindLoadedType("Whalebone.Gameplay.D2.D2ShipController");
            var deckType = FindLoadedType("Whalebone.Gameplay.D7.D7ShipDeckController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            Assert.IsNotNull(shipControllerType, "D2ShipController 未加载。");
            Assert.IsNotNull(deckType, "D7ShipDeckController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            var shipVisual = GameObject.Find("D19_R03_ShipVisualInstance");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(ship, "InitialHarbor 稳定船对象没有创建。");
            Assert.IsNotNull(shipVisual, "InitialHarbor 可见船实例没有创建。");

            var character = player.GetComponent(characterType);
            var animation = player.GetComponent(animationType);
            var body = player.GetComponent<CharacterController>();
            var router = Object.FindFirstObjectByType(routerType,
                FindObjectsInactive.Include) as Component;
            var shipController = ship.GetComponent(shipControllerType);
            var deck = ship.GetComponent(deckType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");
            Assert.IsNotNull(body, "玩家缺少 CharacterController。");
            Assert.IsNotNull(router, "D2ControlRouter 没有创建。");
            Assert.IsNotNull(shipController, "稳定船对象缺少 D2ShipController。");
            Assert.IsNotNull(deck, "稳定船对象缺少 D7ShipDeckController。");

            var tryToggleShip = routerType.GetMethod("TryToggleShip");
            var modeProperty = routerType.GetProperty("Mode");
            var feedbackProperty = routerType.GetProperty("LastInteractionFeedback");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var snapToGround = characterType.GetMethod("SnapToGround");
            var isGrounded = characterType.GetProperty("IsGrounded");
            var isSwimming = characterType.GetProperty("IsSwimming");
            var action = animationType.GetProperty("Action");
            var transitioning = animationType.GetProperty("IsTransitioning");
            var playerBoarded = deckType.GetProperty("PlayerBoarded");
            var snapshotProperty = shipControllerType.GetProperty("Snapshot");
            var applySnapshot = shipControllerType.GetMethod("ApplySnapshot");
            Assert.IsNotNull(tryToggleShip, "D2ControlRouter 缺少 TryToggleShip。");
            Assert.IsNotNull(modeProperty, "D2ControlRouter 缺少 Mode。");
            Assert.IsNotNull(feedbackProperty, "D2ControlRouter 缺少 LastInteractionFeedback。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(snapToGround, "D2CharacterController 缺少 SnapToGround。");
            Assert.IsNotNull(isGrounded, "D2CharacterController 缺少 IsGrounded。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(action, "D2CharacterAnimationController 缺少 Action。");
            Assert.IsNotNull(transitioning, "D2CharacterAnimationController 缺少 IsTransitioning。");
            Assert.IsNotNull(playerBoarded, "D7ShipDeckController 缺少 PlayerBoarded。");
            Assert.IsNotNull(snapshotProperty, "D2ShipController 缺少 Snapshot。");
            Assert.IsNotNull(applySnapshot, "D2ShipController 缺少 ApplySnapshot。");

            for (int settle = 0; settle < 2; settle++) yield return null;
            Vector3 approachPosition = player.transform.position;
            Assert.IsNull(player.transform.parent, "合法下船回归开始前玩家不能已经挂在船上。");
            Assert.AreEqual("Character", modeProperty.GetValue(router).ToString(),
                "合法下船回归开始前控制模式必须是 Character。");

            for (int cycle = 0; cycle < 10; cycle++)
            {
                Assert.IsTrue((bool)tryToggleShip.Invoke(router, null),
                    "第 " + cycle + " 次合法回归无法登船。");
                for (int settle = 0; settle < 2; settle++) yield return null;
                Assert.AreEqual("Ship", modeProperty.GetValue(router).ToString(),
                    "第 " + cycle + " 次合法回归登船后模式不是 Ship。");
                Assert.AreEqual(ship.transform, player.transform.parent,
                    "第 " + cycle + " 次合法回归登船后 parent 不在稳定船根。");

                Assert.IsTrue((bool)tryToggleShip.Invoke(router, null),
                    "第 " + cycle + " 次合法回归无法下船。");
                for (int settle = 0; settle < 2; settle++) yield return null;
                deadline = Time.realtimeSinceStartup + 3f;
                while ((!body.isGrounded || !(bool)isGrounded.GetValue(character)) &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;

                Assert.AreEqual("Character", modeProperty.GetValue(router).ToString(),
                    "第 " + cycle + " 次合法下船后模式没有恢复 Character。");
                Assert.IsNull(player.transform.parent,
                    "第 " + cycle + " 次合法下船后玩家仍挂在船根。");
                Assert.IsTrue(body.enabled,
                    "第 " + cycle + " 次合法下船后 CharacterController 没有恢复。");
                Assert.IsTrue(body.isGrounded && (bool)isGrounded.GetValue(character),
                    "第 " + cycle + " 次合法下船后玩家没有恢复接地。");
                Assert.IsFalse((bool)isSwimming.GetValue(character),
                    "第 " + cycle + " 次合法下船后玩家进入游泳态。");
                Assert.AreEqual("Disembarking", action.GetValue(animation).ToString(),
                    "第 " + cycle + " 次合法下船没有进入 Disembarking 过渡。");
                Assert.IsTrue((bool)transitioning.GetValue(animation),
                    "第 " + cycle + " 次合法下船没有保持过渡状态。");
                Assert.IsFalse((bool)playerBoarded.GetValue(deck),
                    "第 " + cycle + " 次合法下船后 D7 仍记录 PlayerBoarded。");
                Assert.Greater(Vector3.Distance(player.transform.position, shipVisual.transform.position), 1f,
                    "第 " + cycle + " 次合法下船的玩家落在可见船体中心附近。");

                if (cycle < 9)
                {
                    setWorldPosition.Invoke(character, new object[] { approachPosition });
                    Assert.IsTrue((bool)snapToGround.Invoke(character, new object[] { 8f }),
                        "第 " + cycle + " 次合法下船后无法回到下一次登船点。");
                    for (int settle = 0; settle < 2; settle++) yield return null;
                }
            }

            // 再次登船，把船通过真实 D2 航行 owner 移到没有可行走表面的区域。
            setWorldPosition.Invoke(character, new object[] { approachPosition });
            Assert.IsTrue((bool)snapToGround.Invoke(character, new object[] { 8f }),
                "无落点拒绝回归前无法回到合法登船点。");
            for (int settle = 0; settle < 2; settle++) yield return null;
            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "无落点拒绝回归无法先登船。");
            for (int settle = 0; settle < 2; settle++) yield return null;

            var baselineSnapshot = snapshotProperty.GetValue(shipController);
            var snapshotType = baselineSnapshot.GetType();
            var clone = snapshotType.GetMethod("Clone");
            var positionX = snapshotType.GetField("PositionX");
            var positionZ = snapshotType.GetField("PositionZ");
            var speed = snapshotType.GetField("Speed");
            var controlMode = snapshotType.GetField("ControlMode");
            Assert.IsNotNull(clone, "SailingSnapshot 缺少 Clone。");
            Assert.IsNotNull(positionX, "SailingSnapshot 缺少 PositionX。");
            Assert.IsNotNull(positionZ, "SailingSnapshot 缺少 PositionZ。");
            Assert.IsNotNull(speed, "SailingSnapshot 缺少 Speed。");
            Assert.IsNotNull(controlMode, "SailingSnapshot 缺少 ControlMode。");

            Collider terrain = FindActiveCollider("D19_TerrainRoot");
            Assert.IsNotNull(terrain, "无落点拒绝回归缺少 D19_TerrainRoot。");
            // The current runtime terrain mesh is the authority; do not guess
            // a world coordinate that may still be inside a regenerated mesh.
            // Choose a point beyond its actual bounds so the refusal path is
            // testing a genuinely missing walkable surface.
            Vector3 noGroundPosition = new Vector3(
                terrain.bounds.max.x + 50f,
                ship.transform.position.y,
                terrain.bounds.max.z + 50f);

            var noGroundSnapshot = clone.Invoke(baselineSnapshot, null);
            positionX.SetValue(noGroundSnapshot, noGroundPosition.x);
            positionZ.SetValue(noGroundSnapshot, noGroundPosition.z);
            speed.SetValue(noGroundSnapshot, 0f);
            controlMode.SetValue(noGroundSnapshot,
                System.Enum.Parse(controlMode.FieldType, "Ship"));
            applySnapshot.Invoke(shipController, new[] { noGroundSnapshot });
            Physics.SyncTransforms();
            for (int settle = 0; settle < 2; settle++) yield return null;

            bool noGroundResult = (bool)tryToggleShip.Invoke(router, null);
            Assert.IsFalse(noGroundResult, "移动到无可行走表面时下船不能伪造成功。");
            Assert.AreEqual("Ship", modeProperty.GetValue(router).ToString(),
                "无落点拒绝后必须保留 Ship 控制模式。");
            Assert.AreEqual(ship.transform, player.transform.parent,
                "无落点拒绝后玩家必须保留船根 parent。");
            Assert.IsFalse(body.enabled, "无落点拒绝后船控 CharacterController 必须仍关闭。");
            Assert.IsTrue((bool)playerBoarded.GetValue(deck),
                "无落点拒绝后 D7 必须仍记录 PlayerBoarded。");
            var feedback = feedbackProperty.GetValue(router);
            var feedbackCode = feedback.GetType().GetProperty("Code");
            Assert.IsNotNull(feedbackCode, "D2InteractionFeedback 缺少 Code。");
            Assert.AreEqual("disembark_unavailable", feedbackCode.GetValue(feedback),
                "无落点拒绝必须留下明确的 disembark_unavailable 反馈。");

            applySnapshot.Invoke(shipController, new[] { baselineSnapshot });
            Physics.SyncTransforms();
            for (int settle = 0; settle < 2; settle++) yield return null;
            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null),
                "恢复有落点的航行快照后无法再次安全下船。");
            for (int settle = 0; settle < 2; settle++) yield return null;
            Assert.AreEqual("Character", modeProperty.GetValue(router).ToString(),
                "恢复有落点快照后模式没有回到 Character。");
            Assert.IsNull(player.transform.parent, "恢复有落点快照后玩家仍挂在船根。");
            Assert.IsTrue(body.enabled, "恢复有落点快照后 CharacterController 没有恢复。");
            Assert.IsTrue(body.isGrounded && (bool)isGrounded.GetValue(character),
                "恢复有落点快照后玩家没有恢复接地。");

            Debug.Log("[D17/A13] legal_disembark_cycles=10, no_ground_rejection=true, final_grounded=" +
                body.isGrounded + ", final_mode=" + modeProperty.GetValue(router));
        }

        [UnityTest]
        public IEnumerator D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var cameraType = FindLoadedType("Whalebone.Gameplay.CameraFollow");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            Assert.IsNotNull(cameraType, "CameraFollow 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(ship, "InitialHarbor 稳定船对象没有创建。");

            var camera = Object.FindFirstObjectByType(cameraType,
                FindObjectsInactive.Include) as Component;
            var router = Object.FindFirstObjectByType(routerType,
                FindObjectsInactive.Include) as Component;
            Assert.IsNotNull(camera, "InitialHarbor 缺少 CameraFollow。");
            Assert.IsNotNull(router, "InitialHarbor 缺少 D2ControlRouter。");

            var setTargets = cameraType.GetMethod("SetTargets");
            var setControlMode = cameraType.GetMethod("SetControlMode");
            var setViewMode = cameraType.GetMethod("SetViewMode");
            var targetProperty = cameraType.GetProperty("Target");
            var viewModeProperty = cameraType.GetProperty("ViewMode");
            var resolvedDistanceProperty = cameraType.GetProperty("ResolvedDistance");
            var obstructedProperty = cameraType.GetProperty("IsObstructed");
            var collisionChecksProperty = cameraType.GetProperty("CollisionCheckCount");
            var tryToggleShip = routerType.GetMethod("TryToggleShip");
            Assert.IsNotNull(setTargets, "CameraFollow 缺少 SetTargets。");
            Assert.IsNotNull(setControlMode, "CameraFollow 缺少 SetControlMode。");
            Assert.IsNotNull(setViewMode, "CameraFollow 缺少 SetViewMode。");
            Assert.IsNotNull(targetProperty, "CameraFollow 缺少 Target。");
            Assert.IsNotNull(viewModeProperty, "CameraFollow 缺少 ViewMode。");
            Assert.IsNotNull(resolvedDistanceProperty, "CameraFollow 缺少 ResolvedDistance。");
            Assert.IsNotNull(obstructedProperty, "CameraFollow 缺少 IsObstructed。");
            Assert.IsNotNull(collisionChecksProperty, "CameraFollow 缺少 CollisionCheckCount。");
            Assert.IsNotNull(tryToggleShip, "D2ControlRouter 缺少 TryToggleShip。");

            var modeType = setViewMode.GetParameters()[0].ParameterType;
            object characterMode = System.Enum.Parse(modeType, "Character");
            object shipDeckMode = System.Enum.Parse(modeType, "ShipDeck");
            object overviewMode = System.Enum.Parse(modeType, "Overview");
            setTargets.Invoke(camera, new object[] { player.transform, ship.transform });
            setControlMode.Invoke(camera, new object[] { false });
            setViewMode.Invoke(camera, new[] { characterMode });
            for (int settle = 0; settle < 5; settle++) yield return null;

            Assert.AreEqual("Character", viewModeProperty.GetValue(camera).ToString(),
                "初始相机模式没有进入 Character。");
            Assert.AreEqual(player.transform, targetProperty.GetValue(camera),
                "人物模式相机目标必须是玩家。");
            Assert.Greater((int)collisionChecksProperty.GetValue(camera), 0,
                "相机没有执行任何实际遮挡检测。");
            Assert.Greater((float)resolvedDistanceProperty.GetValue(camera), 1.04f,
                "人物视角解析距离低于相机安全下限。");
            Assert.IsFalse(float.IsNaN(camera.transform.position.x) ||
                           float.IsInfinity(camera.transform.position.x) ||
                           float.IsNaN(camera.transform.position.y) ||
                           float.IsInfinity(camera.transform.position.y) ||
                           float.IsNaN(camera.transform.position.z) ||
                           float.IsInfinity(camera.transform.position.z),
                "人物视角相机位置必须有限。");

            for (int cycle = 0; cycle < 10; cycle++)
            {
                setViewMode.Invoke(camera, new[] { overviewMode });
                for (int settle = 0; settle < 2; settle++) yield return null;
                Assert.AreEqual("Overview", viewModeProperty.GetValue(camera).ToString(),
                    "第 " + cycle + " 次相机切换没有进入 Overview。");
                Assert.AreEqual(player.transform, targetProperty.GetValue(camera),
                    "未登船时 Overview 仍应以玩家为观察目标。");

                setViewMode.Invoke(camera, new[] { shipDeckMode });
                for (int settle = 0; settle < 2; settle++) yield return null;
                Assert.AreEqual("ShipDeck", viewModeProperty.GetValue(camera).ToString(),
                    "第 " + cycle + " 次相机切换没有进入 ShipDeck。");
                Assert.AreEqual(player.transform, targetProperty.GetValue(camera),
                    "ShipDeck 视角必须以玩家为主体。");
                Assert.Greater((float)resolvedDistanceProperty.GetValue(camera), 1.04f,
                    "第 " + cycle + " 次模式切换后解析距离低于安全下限。");
            }

            // 通过真实 D2 控制路由登船，验证模式自动切换仍由相机 owner 完成。
            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "相机模式回归无法先登船。");
            for (int settle = 0; settle < 4; settle++) yield return null;
            Assert.AreEqual("ShipDeck", viewModeProperty.GetValue(camera).ToString(),
                "进入 Ship 控制后相机没有自动切到 ShipDeck。");
            Assert.AreEqual(player.transform, targetProperty.GetValue(camera),
                "船控相机目标不能退回船根而遮挡玩家。");

            float distanceBeforeObstruction = (float)resolvedDistanceProperty.GetValue(camera);
            Vector3 targetPosition = player.transform.position;
            Vector3 cameraPosition = camera.transform.position;
            Vector3 cameraDirection = cameraPosition - targetPosition;
            Assert.Greater(cameraDirection.sqrMagnitude, 1f,
                "相机与玩家距离过近，无法建立有效外部遮挡回归。");
            cameraDirection.Normalize();
            var wall = new GameObject("Q12_CameraTest_ExternalWall");
            wall.transform.position = targetPosition + cameraDirection *
                Mathf.Clamp(Vector3.Distance(targetPosition, cameraPosition) * 0.55f, 1.5f, 5f);
            wall.transform.rotation = Quaternion.LookRotation(cameraDirection, Vector3.up);
            var wallCollider = wall.AddComponent<BoxCollider>();
            wallCollider.size = new Vector3(4f, 4f, 0.5f);

            for (int settle = 0; settle < 10; settle++) yield return null;
            float distanceWithObstruction = (float)resolvedDistanceProperty.GetValue(camera);
            Assert.IsTrue((bool)obstructedProperty.GetValue(camera),
                "外部障碍进入相机球形检测路径后没有标记 IsObstructed。");
            Assert.Less(distanceWithObstruction, distanceBeforeObstruction - 0.01f,
                "外部障碍存在时相机没有收缩到安全距离。");
            Assert.Greater((int)collisionChecksProperty.GetValue(camera), 10,
                "外部障碍回归没有产生连续遮挡采样。");

            Object.Destroy(wall);
            for (int settle = 0; settle < 20; settle++) yield return null;
            float distanceAfterRecovery = (float)resolvedDistanceProperty.GetValue(camera);
            Assert.Greater(distanceAfterRecovery, distanceWithObstruction + 0.02f,
                "外部障碍移除后相机距离没有通过阻尼恢复。");
            Assert.IsFalse(float.IsNaN(camera.transform.position.x) ||
                           float.IsInfinity(camera.transform.position.x) ||
                           float.IsNaN(camera.transform.position.y) ||
                           float.IsInfinity(camera.transform.position.y) ||
                           float.IsNaN(camera.transform.position.z) ||
                           float.IsInfinity(camera.transform.position.z),
                "遮挡恢复后相机位置必须有限。");

            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "相机回归结束后无法安全下船。");
            for (int settle = 0; settle < 2; settle++) yield return null;
            Assert.AreEqual("Character", viewModeProperty.GetValue(camera).ToString(),
                "离开 Ship 控制后相机没有恢复 Character。");
            Assert.AreEqual(player.transform, targetProperty.GetValue(camera),
                "离开 Ship 控制后相机目标没有恢复玩家。");

            Debug.Log("[D17/A14] mode_cycles=10, obstructed=true, distanceBefore=" +
                distanceBeforeObstruction.ToString("F3") + ", distanceBlocked=" +
                distanceWithObstruction.ToString("F3") + ", distanceRecovered=" +
                distanceAfterRecovery.ToString("F3") + ", collisionChecks=" +
                collisionChecksProperty.GetValue(camera));
        }

        [UnityTest]
        public IEnumerator D17A16SwimmingCameraTracksHeadingUntilManualLookHandoff()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var cameraType = FindLoadedType("Whalebone.Gameplay.CameraFollow");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(cameraType, "CameraFollow 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            var camera = Object.FindFirstObjectByType(cameraType,
                FindObjectsInactive.Include) as Component;
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(camera, "InitialHarbor 缺少 CameraFollow。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var isSwimming = characterType.GetProperty("IsSwimming");
            var swimmingActive = cameraType.GetProperty("SwimmingPresentationActive");
            var autoYaw = cameraType.GetProperty("SwimmingPresentationAutoYaw");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(swimmingActive, "CameraFollow 缺少游泳镜头状态诊断。");
            Assert.IsNotNull(autoYaw, "CameraFollow 缺少游泳镜头自动航向诊断。");

            setInputEnabled.Invoke(character, new object[] { true });
            setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, 30f) });
            deadline = Time.realtimeSinceStartup + 2f;
            while (!(bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsTrue((bool)isSwimming.GetValue(character), "深水起点没有进入游泳态。");
            yield return null;
            Assert.IsTrue((bool)swimmingActive.GetValue(camera), "游泳态没有启用专用镜头表现。");
            Assert.IsTrue((bool)autoYaw.GetValue(camera), "游泳态初始镜头没有启用自动侧向航向。");

            player.transform.rotation = Quaternion.Euler(
                0f, player.transform.eulerAngles.y + 90f, 0f);
            Physics.SyncTransforms();
            for (int settle = 0; settle < 5; settle++) yield return null;

            Vector3 relative = camera.transform.position - player.transform.position;
            relative.y = 0f;
            Assert.Greater(relative.sqrMagnitude, 1f,
                "游泳镜头与玩家距离不足，无法判断侧向构图。");
            relative.Normalize();
            Vector3 playerForward = player.transform.forward;
            playerForward.y = 0f;
            playerForward.Normalize();
            float forwardAlignment = Mathf.Abs(Vector3.Dot(relative, playerForward));
            Assert.Less(forwardAlignment, 0.35f,
                "玩家转向后游泳镜头仍停留在前后方，角色会读成直立/背向轮廓。");

            var mouse = InputSystem.AddDevice<Mouse>();
            mouse.MakeCurrent();
            Press(mouse.rightButton, queueEventOnly: true);
            InputSystem.QueueDeltaStateEvent(mouse.delta, new Vector2(90f, 0f));
            InputSystem.Update();
            yield return null;
            Release(mouse.rightButton, queueEventOnly: true);
            InputSystem.Update();
            yield return null;
            Assert.IsFalse((bool)autoYaw.GetValue(camera),
                "真实鼠标右键拖动后，游泳镜头仍被自动航向接管。");

            setInputEnabled.Invoke(character, new object[] { false });
            if (mouse.added)
                InputSystem.RemoveDevice(mouse);
            Debug.Log("[D17/A16] swimming_camera_side_alignment=" +
                forwardAlignment.ToString("F3") + ", auto_yaw=" +
                autoYaw.GetValue(camera));
        }

        [UnityTest]
        public IEnumerator D17A15GameplayKeyboardDoesNotNavigateHudAndPauseRestoresModalOwnership()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var inputRouterType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            var settingsType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16AccessibilitySettingsStore");
            var pauseType = FindLoadedType("Whalebone.Gameplay.UI.HarborPauseController");
            var hudCoordinatorType = FindLoadedType("Whalebone.Gameplay.UI.RuntimeHudCoordinator");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(inputRouterType, "D16InputRouter 未加载。");
            Assert.IsNotNull(settingsType, "D16AccessibilitySettingsStore 未加载。");
            Assert.IsNotNull(pauseType, "HarborPauseController 未加载。");
            Assert.IsNotNull(hudCoordinatorType, "RuntimeHudCoordinator 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");

            var resetSettings = settingsType.GetMethod("Reset");
            var setGameplayInputBlocked = inputRouterType.GetMethod("SetGameplayInputBlocked");
            var gameplayInputBlocked = inputRouterType.GetProperty("GameplayInputBlocked");
            var primaryInputAllowed = hudCoordinatorType.GetProperty("PrimaryInputAllowed");
            var paused = pauseType.GetProperty("IsPaused");
            Assert.IsNotNull(resetSettings, "D16 设置仓缺少 Reset。");
            Assert.IsNotNull(setGameplayInputBlocked, "D16InputRouter 缺少 SetGameplayInputBlocked。");
            Assert.IsNotNull(gameplayInputBlocked, "D16InputRouter 缺少 GameplayInputBlocked。");
            Assert.IsNotNull(primaryInputAllowed, "RuntimeHudCoordinator 缺少 PrimaryInputAllowed。");
            Assert.IsNotNull(paused, "HarborPauseController 缺少 IsPaused。");
            resetSettings.Invoke(null, null);
            setGameplayInputBlocked.Invoke(null, new object[] { false });
            Time.timeScale = 1f;

            var eventSystem = EventSystem.current;
            deadline = Time.realtimeSinceStartup + 5f;
            while (eventSystem == null && Time.realtimeSinceStartup < deadline)
            {
                eventSystem = EventSystem.current;
                yield return null;
            }
            Assert.IsNotNull(eventSystem, "InitialHarbor 没有统一 EventSystem。");
            for (int settle = 0; settle < 4; settle++) yield return null;

            Assert.IsFalse((bool)paused.GetValue(
                Object.FindFirstObjectByType(pauseType, FindObjectsInactive.Include)),
                "输入回归开始时暂停状态必须关闭。");
            Assert.IsTrue((bool)primaryInputAllowed.GetValue(null),
                "正常港口玩法开始时主玩法输入必须可用。");
            Assert.IsFalse((bool)gameplayInputBlocked.GetValue(null),
                "正常港口玩法开始时 D16 玩法输入不能被阻塞。");
            Assert.IsFalse(eventSystem.sendNavigationEvents,
                "正常港口玩法不应把 W/S 当作 UGUI 导航。");
            Assert.IsNull(eventSystem.currentSelectedGameObject,
                "正常港口玩法不能保留会吞掉 W/S 的选中菜单项。");

            var modules = eventSystem.GetComponents<BaseInputModule>();
            int enabledModuleCount = 0;
            string enabledModuleName = string.Empty;
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] == null || !modules[i].isActiveAndEnabled) continue;
                enabledModuleCount++;
                enabledModuleName = modules[i].GetType().Name;
            }
            Assert.AreEqual(1, enabledModuleCount,
                "InitialHarbor 必须只保留一个启用的 EventSystem 输入模块。");
            Assert.AreEqual("InputSystemUIInputModule", enabledModuleName,
                "InitialHarbor 的启用 UI 输入模块必须是 InputSystemUIInputModule。");
            var uiInputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            Assert.IsNotNull(uiInputModule, "InitialHarbor 缺少统一 InputSystemUIInputModule。");
            Assert.IsNull(uiInputModule.move,
                "正常港口玩法必须断开 UI Navigate action，不能只依赖 sendNavigationEvents=false。");

            var keyboard = InputSystem.AddDevice<Keyboard>();
            keyboard.MakeCurrent();
            try
            {
                // 正常玩法下重复采样 W/S：玩家应移动，但 EventSystem 不能产生选中项。
                for (int cycle = 0; cycle < 10; cycle++)
                {
                    Vector3 beforeForward = player.transform.position;
                    Press(keyboard.wKey, queueEventOnly: true);
                    InputSystem.Update();
                    for (int frame = 0; frame < 2; frame++) yield return null;
                    Release(keyboard.wKey, queueEventOnly: true);
                    InputSystem.Update();
                    yield return null;
                    Assert.Greater(Vector3.Distance(beforeForward, player.transform.position), 0.0001f,
                        "正常玩法第 " + cycle + " 次 W 输入没有驱动玩家。");
                    Assert.IsNull(eventSystem.currentSelectedGameObject,
                        "正常玩法第 " + cycle + " 次 W 输入改变了 UI 选中项，存在键盘竞争。");

                    Vector3 beforeBack = player.transform.position;
                    Press(keyboard.sKey, queueEventOnly: true);
                    InputSystem.Update();
                    for (int frame = 0; frame < 2; frame++) yield return null;
                    Release(keyboard.sKey, queueEventOnly: true);
                    InputSystem.Update();
                    yield return null;
                    Assert.Greater(Vector3.Distance(beforeBack, player.transform.position), 0.0001f,
                        "正常玩法第 " + cycle + " 次 S 输入没有驱动玩家。");
                    Assert.IsNull(eventSystem.currentSelectedGameObject,
                        "正常玩法第 " + cycle + " 次 S 输入改变了 UI 选中项，存在键盘竞争。");
                    Assert.IsFalse(eventSystem.sendNavigationEvents,
                        "正常玩法第 " + cycle + " 次 W/S 后 UI 导航意外开启。");
                    Assert.IsNull(uiInputModule.move,
                        "正常玩法第 " + cycle + " 次 W/S 后 UI Navigate action 被重新接回。");
                }

                // Esc 打开模态暂停：UI 才能取得导航权，玩法位置必须冻结。
                Press(keyboard.escapeKey, queueEventOnly: true);
                // Escape 是边沿输入；交给 Unity 下一帧的正常 Input System 更新，
                // 否则在协程里提前手动 Update 会在 HarborPauseController.Update
                // 之前清掉 wasPressedThisFrame。
                yield return null;
                Release(keyboard.escapeKey, queueEventOnly: true);
                for (int frame = 0; frame < 2; frame++) yield return null;
                var pause = Object.FindFirstObjectByType(pauseType, FindObjectsInactive.Include);
                Assert.IsTrue((bool)paused.GetValue(pause), "Esc 没有打开港口暂停模态页。");
                Assert.IsTrue(eventSystem.sendNavigationEvents,
                    "暂停模态页打开后 UI 导航没有取得输入权。");
                Assert.IsNotNull(eventSystem.currentSelectedGameObject,
                    "暂停模态页打开后没有建立初始 UI 焦点。");
                Assert.IsNotNull(uiInputModule.move,
                    "暂停模态页打开后没有恢复 UI Navigate action。");
                Vector3 positionWhilePaused = player.transform.position;

                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                for (int frame = 0; frame < 3; frame++) yield return null;
                Release(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                yield return null;
                Assert.Less(Vector3.Distance(positionWhilePaused, player.transform.position), 0.0001f,
                    "暂停模态页打开时 W 仍然驱动了玩家。");

                Press(keyboard.escapeKey, queueEventOnly: true);
                yield return null;
                Release(keyboard.escapeKey, queueEventOnly: true);
                for (int frame = 0; frame < 2; frame++) yield return null;
                Assert.IsFalse((bool)paused.GetValue(pause), "第二次 Esc 没有恢复港口玩法。");
                Assert.AreEqual(1f, Time.timeScale, 0.0001f,
                    "恢复港口玩法后 Time.timeScale 没有恢复。");
                Assert.IsFalse(eventSystem.sendNavigationEvents,
                    "恢复港口玩法后 UI 导航仍然占用 W/S。");
                Assert.IsNull(eventSystem.currentSelectedGameObject,
                    "恢复港口玩法后暂停按钮仍保留 UI 焦点。");
                Assert.IsNull(uiInputModule.move,
                    "恢复港口玩法后 UI Navigate action 仍占用 W/S。");

                // D16 模态输入阻塞也必须拥有同一套 UI 导航策略，不能只切断 D2 而
                // 把 W/S 留给背景 HUD；解除阻塞后再恢复纯玩法输入。
                setGameplayInputBlocked.Invoke(null, new object[] { true });
                for (int frame = 0; frame < 3; frame++) yield return null;
                Assert.IsTrue(eventSystem.sendNavigationEvents,
                    "D16 玩法输入阻塞后 UI 导航没有取得模态权。");
                Assert.IsFalse((bool)primaryInputAllowed.GetValue(null),
                    "D16 玩法输入阻塞后主 HUD 仍声称拥有输入权。");
                Assert.IsNotNull(uiInputModule.move,
                    "D16 玩法输入阻塞后没有恢复 UI Navigate action。");
                Vector3 positionWhileBlocked = player.transform.position;
                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                for (int frame = 0; frame < 3; frame++) yield return null;
                Release(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                yield return null;
                Assert.Less(Vector3.Distance(positionWhileBlocked, player.transform.position), 0.0001f,
                    "D16 玩法输入阻塞后 W 仍然驱动了玩家。");

                setGameplayInputBlocked.Invoke(null, new object[] { false });
                for (int frame = 0; frame < 3; frame++) yield return null;
                Assert.IsFalse(eventSystem.sendNavigationEvents,
                    "解除 D16 玩法输入阻塞后 UI 导航仍占用 W/S。");
                Assert.IsNull(eventSystem.currentSelectedGameObject,
                    "解除 D16 玩法输入阻塞后没有清除模态 UI 焦点。");
                Assert.IsNull(uiInputModule.move,
                    "解除 D16 玩法输入阻塞后 UI Navigate action 仍占用 W/S。");

                Debug.Log("[D17/A15] gameplay_w_s_cycles=10, normal_navigation=false, " +
                    "pause_navigation=true, blocked_navigation=true, restored_navigation=false, " +
                    "enabled_ui_module=" + enabledModuleName);
            }
            finally
            {
                setGameplayInputBlocked.Invoke(null, new object[] { false });
                Time.timeScale = 1f;
                if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            }
        }

        [UnityTest]
        public IEnumerator D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var shipControllerType = FindLoadedType("Whalebone.Gameplay.D2.D2ShipController");
            var deckType = FindLoadedType("Whalebone.Gameplay.D7.D7ShipDeckController");
            var cameraType = FindLoadedType("Whalebone.Gameplay.CameraFollow");
            var inputRouterType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            var hudCoordinatorType = FindLoadedType("Whalebone.Gameplay.UI.RuntimeHudCoordinator");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            Assert.IsNotNull(shipControllerType, "D2ShipController 未加载。");
            Assert.IsNotNull(deckType, "D7ShipDeckController 未加载。");
            Assert.IsNotNull(cameraType, "CameraFollow 未加载。");
            Assert.IsNotNull(inputRouterType, "D16InputRouter 未加载。");
            Assert.IsNotNull(hudCoordinatorType, "RuntimeHudCoordinator 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(ship, "InitialHarbor 稳定船对象没有创建。");

            var character = player.GetComponent(characterType);
            var animation = player.GetComponent(animationType);
            var body = player.GetComponent<CharacterController>();
            var router = Object.FindFirstObjectByType(routerType,
                FindObjectsInactive.Include) as Component;
            var shipController = ship.GetComponent(shipControllerType);
            var deck = ship.GetComponent(deckType);
            var camera = Object.FindFirstObjectByType(cameraType,
                FindObjectsInactive.Include) as Component;
            var eventSystem = EventSystem.current;
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");
            Assert.IsNotNull(body, "玩家缺少 CharacterController。");
            Assert.IsNotNull(router, "D2ControlRouter 没有创建。");
            Assert.IsNotNull(shipController, "稳定船对象缺少 D2ShipController。");
            Assert.IsNotNull(deck, "稳定船对象缺少 D7ShipDeckController。");
            Assert.IsNotNull(camera, "InitialHarbor 缺少 CameraFollow。");
            Assert.IsNotNull(eventSystem, "InitialHarbor 缺少 EventSystem。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var snapToGround = characterType.GetMethod("SnapToGround");
            var isGrounded = characterType.GetProperty("IsGrounded");
            var isSwimming = characterType.GetProperty("IsSwimming");
            var action = animationType.GetProperty("Action");
            var importedNeutral = animationType.GetProperty("ImportedSkinnedRigNeutralOnly");
            var tryToggleShip = routerType.GetMethod("TryToggleShip");
            var modeProperty = routerType.GetProperty("Mode");
            var feedbackProperty = routerType.GetProperty("LastInteractionFeedback");
            var playerBoarded = deckType.GetProperty("PlayerBoarded");
            var snapshotProperty = shipControllerType.GetProperty("Snapshot");
            var applySnapshot = shipControllerType.GetMethod("ApplySnapshot");
            var cameraMode = cameraType.GetProperty("ViewMode");
            var cameraTarget = cameraType.GetProperty("Target");
            var gameplayInputBlocked = inputRouterType.GetProperty("GameplayInputBlocked");
            var primaryInputAllowed = hudCoordinatorType.GetProperty("PrimaryInputAllowed");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(snapToGround, "D2CharacterController 缺少 SnapToGround。");
            Assert.IsNotNull(isGrounded, "D2CharacterController 缺少 IsGrounded。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(action, "D2CharacterAnimationController 缺少 Action。");
            Assert.IsNotNull(importedNeutral, "动画层缺少 imported neutral 诊断。");
            Assert.IsNotNull(tryToggleShip, "D2ControlRouter 缺少 TryToggleShip。");
            Assert.IsNotNull(modeProperty, "D2ControlRouter 缺少 Mode。");
            Assert.IsNotNull(feedbackProperty, "D2ControlRouter 缺少 LastInteractionFeedback。");
            Assert.IsNotNull(playerBoarded, "D7ShipDeckController 缺少 PlayerBoarded。");
            Assert.IsNotNull(snapshotProperty, "D2ShipController 缺少 Snapshot。");
            Assert.IsNotNull(applySnapshot, "D2ShipController 缺少 ApplySnapshot。");
            Assert.IsNotNull(cameraMode, "CameraFollow 缺少 ViewMode。");
            Assert.IsNotNull(cameraTarget, "CameraFollow 缺少 Target。");
            Assert.IsNotNull(gameplayInputBlocked, "D16InputRouter 缺少 GameplayInputBlocked。");
            Assert.IsNotNull(primaryInputAllowed, "RuntimeHudCoordinator 缺少 PrimaryInputAllowed。");

            Keyboard keyboard = null;
            try
            {
                setInputEnabled.Invoke(character, new object[] { false });
                setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, 30f) });
                Physics.SyncTransforms();
                deadline = Time.realtimeSinceStartup + 2f;
                while (!(bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)isSwimming.GetValue(character), "整合路线进入水域后没有进入游泳态。");
                yield return null;
                Assert.AreEqual("Swimming", action.GetValue(animation).ToString(),
                    "整合路线水域状态没有进入 Swimming 动作状态。");

                // 回到真实岛面，退出判定仍由 D2 水域/地面 owner 裁决。
                player.transform.position = new Vector3(0f, 0.42f, 0f);
                Physics.SyncTransforms();
                deadline = Time.realtimeSinceStartup + 2f;
                while ((bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsFalse((bool)isSwimming.GetValue(character), "整合路线回岸后仍停留在游泳态。");
                deadline = Time.realtimeSinceStartup + 2f;
                while ((!body.isGrounded || !(bool)isGrounded.GetValue(character)) &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue(body.isGrounded && (bool)isGrounded.GetValue(character),
                    "整合路线回岸后没有恢复接地。");

                setInputEnabled.Invoke(character, new object[] { true });
                Assert.IsTrue((bool)primaryInputAllowed.GetValue(null),
                    "整合路线开始登船前主玩法输入必须可用。");
                Assert.IsFalse((bool)gameplayInputBlocked.GetValue(null),
                    "整合路线开始登船前 D16 不能阻塞玩法输入。");
                Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "整合路线无法从岸边登船。");
                for (int settle = 0; settle < 3; settle++) yield return null;
                Assert.AreEqual("Ship", modeProperty.GetValue(router).ToString(),
                    "登船后 D2 控制 owner 没有进入 Ship。");
                Assert.AreEqual(ship.transform, player.transform.parent,
                    "登船后玩家没有挂到稳定船根。");
                Assert.IsTrue((bool)playerBoarded.GetValue(deck),
                    "登船后 D7 甲板 owner 没有记录 PlayerBoarded。");
                Assert.AreEqual("ShipDeck", cameraMode.GetValue(camera).ToString(),
                    "登船后 CameraFollow 没有进入 ShipDeck。");
                Assert.AreEqual(player.transform, cameraTarget.GetValue(camera),
                    "登船后相机目标必须保持玩家而不是船根。");

                // 通过实际键盘持续输入驱动船的 throttle + rudder；不直接调用
                // SailingSystem，确保这一段仍经过 D16 -> D2ShipController。
                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Vector3 shipPositionBefore = ship.transform.position;
                float shipHeadingBefore = ship.transform.eulerAngles.y;
                Press(keyboard.wKey, queueEventOnly: true);
                Press(keyboard.dKey, queueEventOnly: true);
                InputSystem.Update();
                for (int frame = 0; frame < 8; frame++) yield return null;
                Release(keyboard.wKey, queueEventOnly: true);
                Release(keyboard.dKey, queueEventOnly: true);
                InputSystem.Update();
                for (int settle = 0; settle < 2; settle++) yield return null;
                Assert.AreEqual("Ship", modeProperty.GetValue(router).ToString(),
                    "船转向输入期间控制 owner 不能退回 Character。");
                Assert.Greater(Vector3.Distance(shipPositionBefore, ship.transform.position), 0.001f,
                    "W 船行驶输入没有改变真实船根位置。");
                Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(shipHeadingBefore,
                    ship.transform.eulerAngles.y)), 0.01f,
                    "D 船转向输入没有改变真实船根航向。");

                var baselineSnapshot = snapshotProperty.GetValue(shipController);
                var snapshotType = baselineSnapshot.GetType();
                var clone = snapshotType.GetMethod("Clone");
                var positionX = snapshotType.GetField("PositionX");
                var positionZ = snapshotType.GetField("PositionZ");
                var speed = snapshotType.GetField("Speed");
                var controlMode = snapshotType.GetField("ControlMode");
                Assert.IsNotNull(clone, "SailingSnapshot 缺少 Clone。");
                Collider terrain = FindActiveCollider("D19_TerrainRoot");
                Assert.IsNotNull(terrain, "整合路线缺少 D19_TerrainRoot。");
                var noGroundSnapshot = clone.Invoke(baselineSnapshot, null);
                positionX.SetValue(noGroundSnapshot, terrain.bounds.max.x + 50f);
                positionZ.SetValue(noGroundSnapshot, terrain.bounds.max.z + 50f);
                speed.SetValue(noGroundSnapshot, 0f);
                controlMode.SetValue(noGroundSnapshot,
                    System.Enum.Parse(controlMode.FieldType, "Ship"));
                applySnapshot.Invoke(shipController, new[] { noGroundSnapshot });
                Physics.SyncTransforms();
                for (int settle = 0; settle < 2; settle++) yield return null;
                Assert.IsFalse((bool)tryToggleShip.Invoke(router, null),
                    "无落点整合路径不能伪造成功下船。");
                Assert.AreEqual("Ship", modeProperty.GetValue(router).ToString(),
                    "无落点拒绝后必须保留 Ship 控制 owner。");
                Assert.AreEqual(ship.transform, player.transform.parent,
                    "无落点拒绝后玩家 parent 不能丢失。");
                var feedback = feedbackProperty.GetValue(router);
                Assert.AreEqual("disembark_unavailable",
                    feedback.GetType().GetProperty("Code").GetValue(feedback),
                    "无落点拒绝必须留下 disembark_unavailable 反馈。");

                applySnapshot.Invoke(shipController, new[] { baselineSnapshot });
                Physics.SyncTransforms();
                for (int settle = 0; settle < 2; settle++) yield return null;
                Assert.IsTrue((bool)tryToggleShip.Invoke(router, null),
                    "恢复合法甲板位置后整合路线无法下船。");
                for (int settle = 0; settle < 3; settle++) yield return null;
                Assert.AreEqual("Character", modeProperty.GetValue(router).ToString(),
                    "合法下船后 D2 控制 owner 没有恢复 Character。");
                Assert.IsNull(player.transform.parent, "合法下船后玩家仍挂在船根。");
                Assert.IsTrue(body.enabled && body.isGrounded && (bool)isGrounded.GetValue(character),
                    "合法下船后玩家 CharacterController/grounded 没有恢复。");
                Assert.IsFalse((bool)isSwimming.GetValue(character),
                    "合法下船后玩家不能进入游泳态。");
                Assert.AreEqual("Disembarking", action.GetValue(animation).ToString(),
                    "合法下船后动作状态没有进入 Disembarking。");
                Assert.AreEqual("Character", cameraMode.GetValue(camera).ToString(),
                    "合法下船后相机没有恢复 Character。");
                Assert.AreEqual(player.transform, cameraTarget.GetValue(camera),
                    "合法下船后相机目标没有恢复玩家。");
                Assert.IsFalse(eventSystem.sendNavigationEvents,
                    "合法下船后 UI 导航不应占用玩法键盘。");

                Debug.Log("[D17/A16] shore_water_shore=true, board_owner=Ship, " +
                    "ship_input_turn=true, no_ground_rejection=true, legal_disembark=true, " +
                    "imported_neutral=" + importedNeutral.GetValue(animation));
            }
            finally
            {
                Time.timeScale = 1f;
                setInputEnabled.Invoke(character, new object[] { true });
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.wKey, queueEventOnly: true);
                    Release(keyboard.dKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
            }
        }

        [UnityTest]
        public IEnumerator D17A05GroundingUsesAnimationOwnerWithoutDirectVisualRootWrites()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var presentationType = FindLoadedType("Whalebone.Gameplay.D7.D7CharacterPresentationController");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(presentationType, "D7CharacterPresentationController 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");

            var animation = player.GetComponent(animationType);
            var presentation = player.GetComponent(presentationType);
            Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");
            Assert.IsNotNull(presentation, "玩家缺少 D7CharacterPresentationController。");

            yield return null;
            yield return null;

            var usesContract = presentationType.GetProperty("UsesAnimationGroundingContract");
            var directWrites = presentationType.GetProperty("DirectVisualRootWriteCount");
            Assert.IsNotNull(usesContract, "D7 表现层缺少 D2 visualRoot 提交契约诊断。");
            Assert.IsNotNull(directWrites, "D7 表现层缺少 direct visualRoot 写入诊断。");
            Assert.IsTrue((bool)usesContract.GetValue(presentation),
                "真实玩家路径必须使用 D2 animation owner 的 grounding offset 契约。");
            Assert.AreEqual(0, directWrites.GetValue(presentation),
                "D7 不得在已有 D2 animation owner 时直接写 visualRoot.localPosition。");
        }

        [UnityTest]
        public IEnumerator D17A06UnverifiedImportedRigKeepsPlaceholderAnimatorQuarantined()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var animation = player.GetComponent(animationType);
            Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");

            var presentationType = FindLoadedType("Whalebone.Gameplay.D2.D20PlayerCandidatePresentationBridge");
            var presentation = presentationType == null ? null : player.GetComponent(presentationType);
            deadline = Time.realtimeSinceStartup + 3f;
            while (presentationType != null && presentation == null &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                presentation = player.GetComponent(presentationType);
            }

            var presentationInstalled = presentation != null &&
                (bool)presentationType.GetProperty("IsInstalled").GetValue(presentation);
            if (presentation != null && !presentationInstalled)
            {
                deadline = Time.realtimeSinceStartup + 3f;
                while (!(bool)presentationType.GetProperty("IsInstalled").GetValue(presentation) &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;
                presentationInstalled = (bool)presentationType.GetProperty("IsInstalled").GetValue(presentation);
            }

            var importedNeutral = animationType.GetProperty("ImportedSkinnedRigNeutralOnly");
            var formalReady = animationType.GetProperty("FormalAnimatorReady");
            var animatorActive = animationType.GetProperty("AnimatorPoseLayerActive");
            var proceduralActive = animationType.GetProperty("ProceduralPoseLayerActive");
            var boundBoneCount = animationType.GetProperty("BoundBoneCount");
            Assert.IsNotNull(importedNeutral, "动画控制器缺少 imported neutral 诊断。");
            Assert.IsNotNull(formalReady, "动画控制器缺少正式 Animator 契约诊断。");
            Assert.IsNotNull(animatorActive, "动画控制器缺少 Animator 姿态层诊断。");
            Assert.IsNotNull(proceduralActive, "动画控制器缺少程序化姿态层诊断。");
            Assert.IsNotNull(boundBoneCount, "动画控制器缺少骨骼绑定数量诊断。");

            bool hasSkinnedVisual = player.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0;
            if (hasSkinnedVisual)
            {
                if (presentationInstalled)
                {
                    var usesGeneric = (bool)presentationType.GetProperty("UsesGenericPathAdapter").GetValue(presentation);
                    var usesHumanoid = (bool)presentationType.GetProperty("UsesHumanoidPoseAdapter").GetValue(presentation);
                    Assert.IsTrue(usesGeneric || usesHumanoid,
                        "当前 InitialHarbor 蒙皮人物没有登记 presentation consumer。");
                    Assert.Greater((int)presentationType.GetProperty("CandidateRendererCount").GetValue(presentation), 0,
                        "当前 InitialHarbor presentation consumer 没有渲染器。");
                }
                else
                {
                    Assert.IsTrue((bool)importedNeutral.GetValue(animation),
                        "当前 InitialHarbor 导入蒙皮没有被识别为待契约验证 rig。");
                }
                Assert.IsFalse((bool)formalReady.GetValue(animation),
                    "D2 物理根上的 Animator 未获得独立动作准入时不得宣称 D2 Animator 已就绪。");
                Assert.IsFalse((bool)animatorActive.GetValue(animation),
                    "D2 物理根上的占位 Animator 不得接管当前导入蒙皮。");
            }
            else
            {
                Assert.IsFalse((bool)importedNeutral.GetValue(animation),
                    "程序化 Player 不应被误报为导入蒙皮 neutral-only。");
                Assert.IsFalse((bool)formalReady.GetValue(animation),
                    "程序化回退没有正式 Animator 资源时不得宣称 Animator 已就绪。");
                Assert.IsFalse((bool)animatorActive.GetValue(animation),
                    "程序化 Player 不应被 D6 占位 Animator 接管。");
                Assert.IsTrue((bool)proceduralActive.GetValue(animation),
                    "未使用可验证导入动作资源时，Player 必须启用程序化姿态层。");
                Assert.GreaterOrEqual((int)boundBoneCount.GetValue(animation), 8,
                    "程序化 Player 必须绑定完整的八个四肢骨骼。");
            }
        }

        private static Collider FindActiveCollider(string name)
        {
            var colliders = Object.FindObjectsByType<Collider>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].name == name)
                    return colliders[i];
            }

            return null;
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
    }
}
