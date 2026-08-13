using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Whalebone.Systems.D16;

namespace Whalebone.Tests.D16
{
    /// <summary>
    /// D16-A03 的设备输入回归：使用 Input System 的受控设备事件验证
    /// 键盘/手柄会进入同一动作意图，且模态面板阻断时不会泄漏到玩法层。
    /// </summary>
    public sealed class D16InputRouterPlayModeTests : InputTestFixture
    {
        private static readonly System.Type RouterType =
            System.Type.GetType("Whalebone.Gameplay.UI.D16.D16InputRouter, Assembly-CSharp");

        private static readonly System.Type SettingsStoreType =
            System.Type.GetType("Whalebone.Gameplay.UI.D16.D16AccessibilitySettingsStore, Assembly-CSharp");

        [UnityTest]
        public IEnumerator D16A03SyntheticGamepadAndKeyboardEventsReachIntentRouter()
        {
            Assert.IsNotNull(RouterType, "D16InputRouter 未被 Unity 主程序集加载。");
            Assert.IsNotNull(SettingsStoreType, "D16AccessibilitySettingsStore 未被 Unity 主程序集加载。");
            InvokeRouter("SetGameplayInputBlocked", false);
            SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            var gamepad = InputSystem.AddDevice<Gamepad>();
            var keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                gamepad.MakeCurrent();
                Press(gamepad.buttonWest, queueEventOnly: true);
                InputSystem.Update();

                Assert.AreSame(gamepad, Gamepad.current, "合成手柄没有成为 Input System 当前设备。 ");
                Assert.IsTrue(gamepad.buttonWest.isPressed, "合成手柄 West 状态没有进入 Input System。 ");
                Assert.IsTrue(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.Interact),
                    "手柄 West 应映射到 Interact 意图。");
                Assert.IsTrue(
                    InvokeRouter<bool>("IsPressed", D16InputAction.Interact),
                    "手柄 West 按住期间应保持 Interact 意图。 ");

                InvokeRouter("SetGameplayInputBlocked", true);
                Assert.IsFalse(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.Interact),
                    "模态界面打开时不能把手柄输入泄漏给玩法层。 ");
                Assert.IsFalse(
                    InvokeRouter<bool>("IsPressed", D16InputAction.Interact),
                    "模态界面打开时不能保持玩法动作。 ");

                InvokeRouter("SetGameplayInputBlocked", false);
                keyboard.MakeCurrent();
                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();

                Assert.IsTrue(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.MoveForward),
                    "键盘 W 应映射到 MoveForward 意图。 ");
                Assert.Greater(
                    InvokeRouter<Vector2>("MoveAxis").y,
                    0f,
                    "键盘 W 应产生正向移动轴。 ");
            }
            finally
            {
                InvokeRouter("SetGameplayInputBlocked", false);
                if (gamepad != null && gamepad.added) InputSystem.RemoveDevice(gamepad);
                if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
                SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator D16A03KeyboardRebindPersistsAndRoutesTheChangedIntent()
        {
            Assert.IsNotNull(RouterType, "D16InputRouter 未被 Unity 主程序集加载。 ");
            Assert.IsNotNull(SettingsStoreType, "D16AccessibilitySettingsStore 未被 Unity 主程序集加载。 ");
            InvokeRouter("SetGameplayInputBlocked", false);
            SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            Keyboard keyboard = null;
            try
            {
                var settings = SettingsStoreType.GetProperty("Current").GetValue(null, null);
                var candidate = settings.GetType().GetMethod("Clone").Invoke(settings, null);
                var trySetBinding = candidate.GetType().GetMethod("TrySetBinding");
                var arguments = new object[]
                {
                    D16InputAction.Interact,
                    D16InputBindingDevice.PrimaryKeyboard,
                    "Q",
                    null,
                };

                Assert.IsTrue((bool)trySetBinding.Invoke(candidate, arguments),
                    "把 Interact 重绑定到 Q 应成功。 ");
                Assert.IsEmpty((string)arguments[3], "有效重绑定不应返回错误。 ");
                SettingsStoreType.GetMethod("Save", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new[] { candidate });

                var persistedBinding = SettingsStoreType.GetMethod("GetBinding", BindingFlags.Public | BindingFlags.Static);
                var binding = persistedBinding.Invoke(null, new object[] { D16InputAction.Interact });
                Assert.AreEqual("Q", binding.GetType().GetField("PrimaryKeyboard").GetValue(binding),
                    "重绑定应在设置保存后仍为 Q。 ");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Press(keyboard.qKey, queueEventOnly: true);
                InputSystem.Update();

                Assert.IsTrue(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.Interact),
                    "保存后的 Q 重绑定应通过同一 Interact 意图路由。 ");
                Assert.IsTrue(
                    InvokeRouter<bool>("IsPressed", D16InputAction.Interact),
                    "Q 按住期间应保持 Interact 意图。 ");
            }
            finally
            {
                InvokeRouter("SetGameplayInputBlocked", false);
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.qKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
                SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator D16A03D3ContextBindingPersistsAndRoutesTheChangedIntent()
        {
            Assert.IsNotNull(RouterType, "D16InputRouter 未被 Unity 主程序集加载。 ");
            Assert.IsNotNull(SettingsStoreType, "D16AccessibilitySettingsStore 未被 Unity 主程序集加载。 ");
            InvokeRouter("SetGameplayInputBlocked", false);
            SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            Keyboard keyboard = null;
            try
            {
                var settings = SettingsStoreType.GetProperty("Current").GetValue(null, null);
                var candidate = settings.GetType().GetMethod("Clone").Invoke(settings, null);
                var trySetBinding = candidate.GetType().GetMethod("TrySetBinding");
                var arguments = new object[]
                {
                    D16InputAction.D3StartVoyage,
                    D16InputBindingDevice.PrimaryKeyboard,
                    "Q",
                    null,
                };

                Assert.IsTrue((bool)trySetBinding.Invoke(candidate, arguments),
                    "D3 航程开始动作重绑定到 Q 应成功。 ");
                Assert.IsEmpty((string)arguments[3], "有效的 D3 上下文重绑定不应返回错误。 ");
                SettingsStoreType.GetMethod("Save", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new[] { candidate });

                var persistedBinding = SettingsStoreType.GetMethod("GetBinding",
                    BindingFlags.Public | BindingFlags.Static);
                var binding = persistedBinding.Invoke(null, new object[] { D16InputAction.D3StartVoyage });
                Assert.AreEqual("Q", binding.GetType().GetField("PrimaryKeyboard").GetValue(binding),
                    "D3 上下文重绑定保存后应仍为 Q。 ");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Press(keyboard.qKey, queueEventOnly: true);
                InputSystem.Update();

                Assert.IsTrue(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.D3StartVoyage),
                    "保存后的 D3 Q 重绑定应进入 D3StartVoyage 意图。 ");
            }
            finally
            {
                InvokeRouter("SetGameplayInputBlocked", false);
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.qKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
                SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator D16A03D4ContextBindingPersistsAndRoutesTheChangedIntent()
        {
            Assert.IsNotNull(RouterType, "D16InputRouter 未被 Unity 主程序集加载。 ");
            Assert.IsNotNull(SettingsStoreType, "D16AccessibilitySettingsStore 未被 Unity 主程序集加载。 ");
            InvokeRouter("SetGameplayInputBlocked", false);
            SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            Keyboard keyboard = null;
            try
            {
                var settings = SettingsStoreType.GetProperty("Current").GetValue(null, null);
                var candidate = settings.GetType().GetMethod("Clone").Invoke(settings, null);
                var trySetBinding = candidate.GetType().GetMethod("TrySetBinding");
                var arguments = new object[]
                {
                    D16InputAction.D4StartCombat,
                    D16InputBindingDevice.PrimaryKeyboard,
                    "Q",
                    null,
                };

                Assert.IsTrue((bool)trySetBinding.Invoke(candidate, arguments),
                    "D4 开始炮战动作重绑定到 Q 应成功。 ");
                Assert.IsEmpty((string)arguments[3], "有效的 D4 上下文重绑定不应返回错误。 ");
                SettingsStoreType.GetMethod("Save", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new[] { candidate });

                var persistedBinding = SettingsStoreType.GetMethod("GetBinding",
                    BindingFlags.Public | BindingFlags.Static);
                var binding = persistedBinding.Invoke(null, new object[] { D16InputAction.D4StartCombat });
                Assert.AreEqual("Q", binding.GetType().GetField("PrimaryKeyboard").GetValue(binding),
                    "D4 上下文重绑定保存后应仍为 Q。 ");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Press(keyboard.qKey, queueEventOnly: true);
                InputSystem.Update();

                Assert.IsTrue(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.D4StartCombat),
                    "保存后的 D4 Q 重绑定应进入 D4StartCombat 意图。 ");
            }
            finally
            {
                InvokeRouter("SetGameplayInputBlocked", false);
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.qKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
                SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator D16A03D5ContextBindingPersistsAndRoutesTheChangedIntent()
        {
            Assert.IsNotNull(RouterType, "D16InputRouter 未被 Unity 主程序集加载。 ");
            Assert.IsNotNull(SettingsStoreType, "D16AccessibilitySettingsStore 未被 Unity 主程序集加载。 ");
            InvokeRouter("SetGameplayInputBlocked", false);
            SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            Keyboard keyboard = null;
            try
            {
                var settings = SettingsStoreType.GetProperty("Current").GetValue(null, null);
                var candidate = settings.GetType().GetMethod("Clone").Invoke(settings, null);
                var trySetBinding = candidate.GetType().GetMethod("TrySetBinding");
                var arguments = new object[]
                {
                    D16InputAction.D5DiscoverIsland,
                    D16InputBindingDevice.PrimaryKeyboard,
                    "Q",
                    null,
                };

                Assert.IsTrue((bool)trySetBinding.Invoke(candidate, arguments),
                    "D5 发现岛屿动作重绑定到 Q 应成功。 ");
                Assert.IsEmpty((string)arguments[3], "有效的 D5 上下文重绑定不应返回错误。 ");
                SettingsStoreType.GetMethod("Save", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new[] { candidate });

                var persistedBinding = SettingsStoreType.GetMethod("GetBinding",
                    BindingFlags.Public | BindingFlags.Static);
                var binding = persistedBinding.Invoke(null, new object[] { D16InputAction.D5DiscoverIsland });
                Assert.AreEqual("Q", binding.GetType().GetField("PrimaryKeyboard").GetValue(binding),
                    "D5 上下文重绑定保存后应仍为 Q。 ");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Press(keyboard.qKey, queueEventOnly: true);
                InputSystem.Update();

                Assert.IsTrue(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.D5DiscoverIsland),
                    "保存后的 D5 Q 重绑定应进入 D5DiscoverIsland 意图。 ");
            }
            finally
            {
                InvokeRouter("SetGameplayInputBlocked", false);
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.qKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
                SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator D16A03D9ContextBindingPersistsAndRoutesTheChangedIntent()
        {
            Assert.IsNotNull(RouterType, "D16InputRouter 未被 Unity 主程序集加载。 ");
            Assert.IsNotNull(SettingsStoreType, "D16AccessibilitySettingsStore 未被 Unity 主程序集加载。 ");
            InvokeRouter("SetGameplayInputBlocked", false);
            SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            Keyboard keyboard = null;
            try
            {
                var settings = SettingsStoreType.GetProperty("Current").GetValue(null, null);
                var candidate = settings.GetType().GetMethod("Clone").Invoke(settings, null);
                var trySetBinding = candidate.GetType().GetMethod("TrySetBinding");
                var arguments = new object[]
                {
                    D16InputAction.D9EnterBerth,
                    D16InputBindingDevice.PrimaryKeyboard,
                    "Z",
                    null,
                };

                Assert.IsTrue((bool)trySetBinding.Invoke(candidate, arguments),
                    "D9 入港靠泊动作重绑定到 Z 应成功。 ");
                Assert.IsEmpty((string)arguments[3], "有效的 D9 上下文重绑定不应返回错误。 ");
                SettingsStoreType.GetMethod("Save", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new[] { candidate });

                var persistedBinding = SettingsStoreType.GetMethod("GetBinding",
                    BindingFlags.Public | BindingFlags.Static);
                var binding = persistedBinding.Invoke(null, new object[] { D16InputAction.D9EnterBerth });
                Assert.AreEqual("Z", binding.GetType().GetField("PrimaryKeyboard").GetValue(binding),
                    "D9 上下文重绑定保存后应仍为 Z。 ");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Press(keyboard.zKey, queueEventOnly: true);
                InputSystem.Update();

                Assert.IsTrue(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.D9EnterBerth),
                    "保存后的 D9 Z 重绑定应进入 D9EnterBerth 意图。 ");
            }
            finally
            {
                InvokeRouter("SetGameplayInputBlocked", false);
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.zKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
                SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator D16A03D10ContextBindingPersistsAndRoutesTheChangedIntent()
        {
            Assert.IsNotNull(RouterType, "D16InputRouter 未被 Unity 主程序集加载。 ");
            Assert.IsNotNull(SettingsStoreType, "D16AccessibilitySettingsStore 未被 Unity 主程序集加载。 ");
            InvokeRouter("SetGameplayInputBlocked", false);
            SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            Keyboard keyboard = null;
            try
            {
                var settings = SettingsStoreType.GetProperty("Current").GetValue(null, null);
                var candidate = settings.GetType().GetMethod("Clone").Invoke(settings, null);
                var trySetBinding = candidate.GetType().GetMethod("TrySetBinding");
                var arguments = new object[]
                {
                    D16InputAction.D10OpenTavern,
                    D16InputBindingDevice.PrimaryKeyboard,
                    "Z",
                    null,
                };

                Assert.IsTrue((bool)trySetBinding.Invoke(candidate, arguments),
                    "D10 打开酒馆动作重绑定到 Z 应成功。 ");
                Assert.IsEmpty((string)arguments[3], "有效的 D10 上下文重绑定不应返回错误。 ");
                SettingsStoreType.GetMethod("Save", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new[] { candidate });

                var persistedBinding = SettingsStoreType.GetMethod("GetBinding",
                    BindingFlags.Public | BindingFlags.Static);
                var binding = persistedBinding.Invoke(null, new object[] { D16InputAction.D10OpenTavern });
                Assert.AreEqual("Z", binding.GetType().GetField("PrimaryKeyboard").GetValue(binding),
                    "D10 上下文重绑定保存后应仍为 Z。 ");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Press(keyboard.zKey, queueEventOnly: true);
                InputSystem.Update();

                Assert.IsTrue(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.D10OpenTavern),
                    "保存后的 D10 Z 重绑定应进入 D10OpenTavern 意图。 ");
            }
            finally
            {
                InvokeRouter("SetGameplayInputBlocked", false);
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.zKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
                SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator D16A03GamepadRebindPersistsAndRoutesTheChangedIntent()
        {
            Assert.IsNotNull(RouterType, "D16InputRouter 未被 Unity 主程序集加载。 ");
            Assert.IsNotNull(SettingsStoreType, "D16AccessibilitySettingsStore 未被 Unity 主程序集加载。 ");
            InvokeRouter("SetGameplayInputBlocked", false);
            SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            Gamepad gamepad = null;
            try
            {
                var settings = SettingsStoreType.GetProperty("Current").GetValue(null, null);
                var candidate = settings.GetType().GetMethod("Clone").Invoke(settings, null);
                var trySetBinding = candidate.GetType().GetMethod("TrySetBinding");
                var arguments = new object[]
                {
                    D16InputAction.Quality,
                    D16InputBindingDevice.GamepadButton,
                    "LeftTrigger",
                    null,
                };

                Assert.IsTrue((bool)trySetBinding.Invoke(candidate, arguments),
                    "把 Quality 重绑定到手柄 LeftTrigger 应成功。 ");
                Assert.IsEmpty((string)arguments[3], "有效手柄重绑定不应返回错误。 ");
                SettingsStoreType.GetMethod("Save", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new[] { candidate });

                var persistedBinding = SettingsStoreType.GetMethod("GetBinding",
                    BindingFlags.Public | BindingFlags.Static);
                var binding = persistedBinding.Invoke(null, new object[] { D16InputAction.Quality });
                Assert.AreEqual("LeftTrigger", binding.GetType().GetField("GamepadButton").GetValue(binding),
                    "手柄重绑定应在设置保存后仍为 LeftTrigger。 ");

                gamepad = InputSystem.AddDevice<Gamepad>();
                gamepad.MakeCurrent();
                InputSystem.QueueDeltaStateEvent(gamepad.leftTrigger, 1f);
                InputSystem.Update();

                Assert.IsTrue(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.Quality),
                    "保存后的手柄 LeftTrigger 重绑定应进入统一 Quality 意图。 ");
                Assert.IsTrue(
                    InvokeRouter<bool>("IsPressed", D16InputAction.Quality),
                    "手柄 LeftTrigger 按住期间应保持 Quality 意图。 ");

                yield return null;
                InputSystem.Update();
                Assert.IsFalse(
                    InvokeRouter<bool>("WasPressedThisFrame", D16InputAction.Quality),
                    "手柄 LeftTrigger 持续按住时，下一帧不应重复触发一次性意图。 ");
                Assert.IsTrue(
                    InvokeRouter<bool>("IsPressed", D16InputAction.Quality),
                    "手柄 LeftTrigger 下一帧仍按住时应保持 Quality 意图。 ");
            }
            finally
            {
                InvokeRouter("SetGameplayInputBlocked", false);
                if (gamepad != null && gamepad.added)
                {
                    InputSystem.QueueDeltaStateEvent(gamepad.leftTrigger, 0f);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(gamepad);
                }
                SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator D16A03SyntheticEscapeTogglesHarborPauseThroughController()
        {
            var pauseType = System.Type.GetType("Whalebone.Gameplay.UI.HarborPauseController, Assembly-CSharp");
            Assert.IsNotNull(pauseType, "HarborPauseController 未被 Unity 主程序集加载。");

            object pauseController = null;
            Keyboard keyboard = null;
            try
            {
                var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
                Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
                while (!load.isDone) yield return null;

                float deadline = Time.realtimeSinceStartup + 5f;
                while (pauseController == null && Time.realtimeSinceStartup < deadline)
                {
                    var controllerObject = GameObject.Find("HarborPauseController");
                    if (controllerObject != null)
                        pauseController = controllerObject.GetComponent(pauseType);
                    if (pauseController == null) yield return null;
                }

                Assert.IsNotNull(pauseController, "InitialHarbor 暂停控制器未创建。");
                var isPaused = pauseType.GetProperty("IsPaused", BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(isPaused, "暂停控制器缺少 IsPaused 状态。");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();

                Press(keyboard.escapeKey, queueEventOnly: true);
                yield return null;

                Assert.IsTrue((bool)isPaused.GetValue(pauseController),
                    "合成键盘 Escape 没有通过 HarborPauseController 打开暂停菜单。");
                Assert.IsTrue(Time.timeScale <= 0.001f,
                    "暂停菜单打开后时间流速没有停止。");

                Release(keyboard.escapeKey, queueEventOnly: true);
                yield return null;
                Press(keyboard.escapeKey, queueEventOnly: true);
                yield return null;

                Assert.IsFalse((bool)isPaused.GetValue(pauseController),
                    "合成键盘 Escape 没有通过 HarborPauseController 恢复游戏。");
                Assert.AreEqual(1f, Time.timeScale, 0.001f,
                    "暂停恢复后时间流速没有恢复。");
            }
            finally
            {
                Time.timeScale = 1f;
                InvokeRouter("SetGameplayInputBlocked", false);
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.escapeKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
            }
        }

        [UnityTest]
        public IEnumerator D16A03InitialHarborKeyboardCameraAndBoardingLoopWorks()
        {
            Assert.IsNotNull(RouterType, "D16InputRouter 未被 Unity 主程序集加载。");
            InvokeRouter("SetGameplayInputBlocked", false);
            SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            Keyboard keyboard = null;
            Mouse mouse = null;
            try
            {
                var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
                Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
                while (!load.isDone) yield return null;

                var bootstrapType = System.Type.GetType("Whalebone.Gameplay.D2.D2RuntimeBootstrap, Assembly-CSharp");
                var routerType = System.Type.GetType("Whalebone.Gameplay.D2.D2ControlRouter, Assembly-CSharp");
                var cameraType = System.Type.GetType("Whalebone.Gameplay.CameraFollow, Assembly-CSharp");
                Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未被 Unity 主程序集加载。");
                Assert.IsNotNull(routerType, "D2ControlRouter 未被 Unity 主程序集加载。");
                Assert.IsNotNull(cameraType, "CameraFollow 未被 Unity 主程序集加载。");

                Component bootstrap = null;
                Component router = null;
                float deadline = Time.realtimeSinceStartup + 8f;
                while ((bootstrap == null || router == null ||
                        !(bool)bootstrapType.GetProperty("IsReady").GetValue(bootstrap)) &&
                       Time.realtimeSinceStartup < deadline)
                {
                    bootstrap = UnityEngine.Object.FindFirstObjectByType(bootstrapType, FindObjectsInactive.Include) as Component;
                    router = UnityEngine.Object.FindFirstObjectByType(routerType, FindObjectsInactive.Include) as Component;
                    // 即使两个对象已经创建，也必须让出主线程，等待 D2 分阶段组装完成。
                    // 否则测试会在 IsReady=false 时忙等，反过来把被测运行时卡死。
                    yield return null;
                }

                Assert.IsNotNull(bootstrap, "InitialHarbor D2 运行时入口未创建。");
                Assert.IsTrue((bool)bootstrapType.GetProperty("IsReady").GetValue(bootstrap),
                    "InitialHarbor D2 运行时接线未完成。");
                Assert.IsNotNull(router, "InitialHarbor D2 控制路由器未创建。");

                var player = GameObject.Find("D2_PlayerAvatar");
                var ship = GameObject.Find("ph_ship_fishing");
                Assert.IsNotNull(player, "InitialHarbor 玩家对象未创建。");
                Assert.IsNotNull(ship, "InitialHarbor 船对象未创建。");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Vector3 characterStart = player.transform.position;
                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                yield return null;
                Release(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                Assert.Greater(Vector3.Distance(characterStart, player.transform.position), 0.001f,
                    "InitialHarbor 键盘 W 没有驱动玩家人物移动。");

                var camera = Camera.main;
                Assert.IsNotNull(camera, "InitialHarbor 主相机未创建。");
                var cameraFollow = camera.GetComponent(cameraType);
                Assert.IsNotNull(cameraFollow, "主相机缺少 CameraFollow。");
                Quaternion cameraStartRotation = camera.transform.rotation;
                mouse = InputSystem.AddDevice<Mouse>();
                mouse.MakeCurrent();
                Press(mouse.rightButton, queueEventOnly: true);
                InputSystem.QueueDeltaStateEvent(mouse.delta, new Vector2(90f, 0f));
                InputSystem.Update();
                yield return null;
                Release(mouse.rightButton, queueEventOnly: true);
                InputSystem.Update();
                Assert.Greater(Quaternion.Angle(cameraStartRotation, camera.transform.rotation), 0.5f,
                    "InitialHarbor 鼠标右键拖动没有改变相机方向。");

                var viewModeProperty = cameraType.GetProperty("ViewMode");
                var beforeViewMode = viewModeProperty.GetValue(cameraFollow);
                cameraType.GetMethod("CycleViewMode", BindingFlags.Public | BindingFlags.Instance)
                    .Invoke(cameraFollow, null);
                yield return null;
                var afterViewMode = viewModeProperty.GetValue(cameraFollow);
                Assert.AreNotEqual(beforeViewMode, afterViewMode,
                    "InitialHarbor F3/视角切换入口没有改变视角模式。");

                bool boarded = (bool)routerType.GetMethod("TryToggleShip",
                    BindingFlags.Public | BindingFlags.Instance).Invoke(router, null);
                Assert.IsTrue(boarded, "玩家在初始船旁不能完成上船。");
                Assert.AreEqual("Ship", routerType.GetProperty("Mode").GetValue(router).ToString(),
                    "上船后控制模式没有切换为 Ship。");
                Assert.AreSame(ship.transform, player.transform.parent,
                    "上船后玩家没有挂到船对象，无法随船航行。");

                Vector3 shipStart = ship.transform.position;
                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                for (int i = 0; i < 8; i++) yield return null;
                Release(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                Assert.Greater(Vector3.Distance(shipStart, ship.transform.position), 0.001f,
                    "上船后键盘 W 没有驱动船只航行。");

                bool disembarked = (bool)routerType.GetMethod("TryToggleShip",
                    BindingFlags.Public | BindingFlags.Instance).Invoke(router, null);
                Assert.IsTrue(disembarked, "玩家无法完成下船。");
                Assert.AreEqual("Character", routerType.GetProperty("Mode").GetValue(router).ToString(),
                    "下船后控制模式没有恢复为 Character。");
                Assert.IsNull(player.transform.parent, "下船后玩家仍挂在船对象上。");
            }
            finally
            {
                Time.timeScale = 1f;
                InvokeRouter("SetGameplayInputBlocked", false);
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.wKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
                if (mouse != null && mouse.added)
                {
                    Release(mouse.rightButton, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(mouse);
                }
                SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            InvokeRouter("SetGameplayInputBlocked", false);
            SettingsStoreType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);
            base.TearDown();
        }

        private static void InvokeRouter(string methodName, params object[] args)
        {
            RouterType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, args);
        }

        private static T InvokeRouter<T>(string methodName, params object[] args)
        {
            return (T)RouterType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, args);
        }
    }
}
