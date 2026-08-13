using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
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
                var character = player.GetComponent(characterType);
                var animation = player.GetComponent(animationType);
                Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
                Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");

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

                Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "动作回归无法完成下船。");
                for (int i = 0; i < 2; i++) yield return null;
                Assert.AreEqual("Disembarking", action.GetValue(animation).ToString(),
                    "下船过渡没有进入 Disembarking 动作。");
                Assert.IsTrue((bool)transitioning.GetValue(animation), "下船动作没有进入过渡状态。");
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
