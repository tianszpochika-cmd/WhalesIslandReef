using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Whalebone.Tests.D17
{
    /// <summary>
    /// D17-A06 的本机失败恢复回归：暂停、返回主菜单、重新新建游戏必须形成完整可重复链。
    /// 该测试验证场景生命周期和输入基础设施，不替代目标设备安装/崩溃/长线验收。
    /// </summary>
    public sealed class D17FailureRecoveryPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator D17A06PauseReturnMainMenuAndStartNewGameRestoresRuntime()
        {
            var inputRouterType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            var pauseType = FindLoadedType("Whalebone.Gameplay.UI.HarborPauseController");
            var gameManagerType = FindLoadedType("Whalebone.Core.GameManager");
            Assert.IsNotNull(inputRouterType, "D16InputRouter 未加载。");
            Assert.IsNotNull(pauseType, "HarborPauseController 未加载。");
            Assert.IsNotNull(gameManagerType, "GameManager 未加载。");

            var setBlocked = inputRouterType.GetMethod("SetGameplayInputBlocked",
                BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(setBlocked, "D16InputRouter 缺少输入锁入口。");
            setBlocked.Invoke(null, new object[] { false });

            Keyboard keyboard = null;
            try
            {
                // 从玩家真实入口开始，避免直接加载港口时触发“没有进行中的游戏”保存告警，
                // 同时覆盖“主菜单新建游戏”这条实际运行链。
                yield return LoadSceneAndWait("MainMenu");

                var firstNewGameButton = FindButton("MainMenuCanvas/MainMenu/BtnNewGame");
                Assert.IsNotNull(firstNewGameButton, "首次进入主菜单时新游戏按钮未创建。");
                Assert.IsTrue(firstNewGameButton.interactable, "首次进入主菜单时新游戏按钮不可交互。");
                firstNewGameButton.onClick.Invoke();
                yield return null;

                var firstConfirmButton = FindButton("MainMenuCanvas/NewGamePanel/BtnConfirm");
                Assert.IsNotNull(firstConfirmButton, "首次新游戏确认按钮未创建。");
                Assert.IsTrue(firstConfirmButton.interactable, "首次新游戏确认按钮不可交互。");
                firstConfirmButton.onClick.Invoke();
                yield return WaitForScene("InitialHarbor", 8f);

                var firstRuntimeDeadline = Time.realtimeSinceStartup + 8f;
                while (GameObject.Find("D2_PlayerAvatar") == null &&
                       Time.realtimeSinceStartup < firstRuntimeDeadline)
                {
                    yield return null;
                }
                Assert.IsNotNull(GameObject.Find("D2_PlayerAvatar"),
                    "首次新建游戏后没有创建玩家对象。");

                Component pauseController = null;
                var pauseDeadline = Time.realtimeSinceStartup + 8f;
                while (pauseController == null && Time.realtimeSinceStartup < pauseDeadline)
                {
                    pauseController = UnityEngine.Object.FindFirstObjectByType(pauseType,
                        FindObjectsInactive.Include) as Component;
                    if (pauseController == null) yield return null;
                }
                Assert.IsNotNull(pauseController, "InitialHarbor 暂停控制器未创建。");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Press(keyboard.escapeKey, queueEventOnly: true);
                yield return null;
                Release(keyboard.escapeKey, queueEventOnly: true);
                yield return null;

                var isPaused = pauseType.GetProperty("IsPaused",
                    BindingFlags.Public | BindingFlags.Instance);
                Assert.IsTrue((bool)isPaused.GetValue(pauseController),
                    "ESC 后暂停菜单没有进入暂停状态。");
                Assert.AreEqual(0f, Time.timeScale, 0.001f,
                    "暂停菜单打开后时间缩放没有停止。");

                var mainMenuButton = FindButton("HarborPauseCanvas/PausePanel/MainMenu");
                Assert.IsNotNull(mainMenuButton, "暂停菜单返回主菜单按钮未创建。");
                Assert.IsTrue(mainMenuButton.interactable, "暂停菜单返回主菜单按钮不可交互。");
                Assert.IsTrue(mainMenuButton.GetComponent<Graphic>().raycastTarget,
                    "暂停菜单返回主菜单按钮没有有效命中图形。");

                mainMenuButton.onClick.Invoke();
                yield return WaitForScene("MainMenu", 8f);
                Assert.AreEqual(1f, Time.timeScale, 0.001f,
                    "返回主菜单后时间缩放没有恢复。");
                Assert.IsNotNull(GameObject.Find("MainMenuController"),
                    "返回主菜单后 MainMenuController 没有重新创建。");
                Assert.AreEqual(1, CountActive<EventSystem>(),
                    "返回主菜单后启用中的 EventSystem 不应重复。");
                Assert.IsNull(GameObject.Find("HarborPauseCanvas"),
                    "返回主菜单后旧暂停画布仍然残留并可能覆盖主菜单。");

                var newGameButton = FindButton("MainMenuCanvas/MainMenu/BtnNewGame");
                Assert.IsNotNull(newGameButton, "主菜单新游戏按钮未创建。");
                Assert.IsTrue(newGameButton.interactable, "主菜单新游戏按钮不可交互。");
                newGameButton.onClick.Invoke();
                yield return null;

                var confirmButton = FindButton("MainMenuCanvas/NewGamePanel/BtnConfirm");
                Assert.IsNotNull(confirmButton, "新游戏确认按钮未创建。");
                Assert.IsTrue(confirmButton.interactable, "新游戏确认按钮不可交互。");
                confirmButton.onClick.Invoke();
                yield return WaitForScene("InitialHarbor", 8f);

                var secondRuntimeDeadline = Time.realtimeSinceStartup + 8f;
                while (GameObject.Find("D2_PlayerAvatar") == null &&
                       Time.realtimeSinceStartup < secondRuntimeDeadline)
                {
                    yield return null;
                }
                Assert.AreEqual(1f, Time.timeScale, 0.001f,
                    "重新新建游戏后时间缩放没有恢复。");
                Assert.IsNotNull(GameObject.Find("D2_PlayerAvatar"),
                    "返回主菜单后重新新建游戏没有创建玩家对象。");
                Assert.IsNotNull(GameObject.Find("HarborPauseController"),
                    "返回主菜单后重新新建游戏没有恢复暂停控制器。");
                Assert.AreEqual(1, CountActive<EventSystem>(),
                    "重新进入 InitialHarbor 后启用中的 EventSystem 不应重复。");
            }
            finally
            {
                Time.timeScale = 1f;
                setBlocked.Invoke(null, new object[] { false });
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.escapeKey);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
            }
        }

        [UnityTest]
        public IEnumerator D17S02LocalStoryRepeatsTenTimes()
        {
            var inputRouterType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var pauseType = FindLoadedType("Whalebone.Gameplay.UI.HarborPauseController");
            var saveCoordinatorType = FindLoadedType("Whalebone.Gameplay.D7.D7SaveCoordinator");
            var cameraType = FindLoadedType("Whalebone.Gameplay.CameraFollow");
            Assert.IsNotNull(inputRouterType, "D16InputRouter 未加载。");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            Assert.IsNotNull(pauseType, "HarborPauseController 未加载。");
            Assert.IsNotNull(saveCoordinatorType, "D7SaveCoordinator 未加载。");
            Assert.IsNotNull(cameraType, "CameraFollow 未加载。");

            var setBlocked = inputRouterType.GetMethod("SetGameplayInputBlocked",
                BindingFlags.Public | BindingFlags.Static);
            var saveAll = saveCoordinatorType.GetMethod("SaveAllRuntimeState",
                BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(setBlocked, "D16InputRouter 缺少输入锁入口。");
            Assert.IsNotNull(saveAll, "D7SaveCoordinator 缺少全量保存入口。");
            setBlocked.Invoke(null, new object[] { false });

            Keyboard keyboard = null;
            try
            {
                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();

                for (int cycle = 1; cycle <= 10; cycle++)
                {
                    Time.timeScale = 1f;
                    yield return LoadSceneAndWait("MainMenu");

                    Button newGameButton = null;
                    yield return WaitForButton("MainMenuCanvas/MainMenu/BtnNewGame", 8f,
                        value => newGameButton = value);
                    Assert.IsNotNull(newGameButton, "第 " + cycle + " 次新游戏按钮未创建。");
                    Assert.IsTrue(newGameButton.interactable, "第 " + cycle + " 次新游戏按钮不可交互。");
                    newGameButton.onClick.Invoke();
                    yield return null;

                    Button confirmButton = null;
                    yield return WaitForButton("MainMenuCanvas/NewGamePanel/BtnConfirm", 3f,
                        value => confirmButton = value);
                    Assert.IsNotNull(confirmButton, "第 " + cycle + " 次新游戏确认按钮未创建。");
                    Assert.IsTrue(confirmButton.interactable, "第 " + cycle + " 次确认按钮不可交互。");
                    confirmButton.onClick.Invoke();
                    yield return WaitForScene("InitialHarbor", 8f);
                    yield return WaitForRuntime(bootstrapType, 8f);

                    var player = GameObject.Find("D2_PlayerAvatar");
                    var ship = GameObject.Find("ph_ship_fishing");
                    Assert.IsNotNull(player, "第 " + cycle + " 次进入港口后没有玩家。");
                    Assert.IsNotNull(ship, "第 " + cycle + " 次进入港口后没有船。");

                    var character = player.GetComponent(characterType);
                    var grounded = characterType.GetProperty("IsGrounded");
                    var collision = characterType.GetProperty("CollisionEnabled");
                    var visualWorldHeight = characterType.GetProperty("VisualWorldHeight");
                    Assert.IsNotNull(character, "第 " + cycle + " 次玩家没有物理控制器。");
                    Assert.IsTrue((bool)grounded.GetValue(character), "第 " + cycle + " 次玩家没有落地。");
                    Assert.IsTrue((bool)collision.GetValue(character), "第 " + cycle + " 次玩家碰撞没有启用。");
                    Assert.That((float)visualWorldHeight.GetValue(character), Is.InRange(1.65f, 1.95f),
                        "第 " + cycle + " 次玩家视觉尺寸没有归一到人物尺度。");

                    var router = UnityEngine.Object.FindFirstObjectByType(routerType,
                        FindObjectsInactive.Include) as Component;
                    var toggleShip = routerType.GetMethod("TryToggleShip",
                        BindingFlags.Public | BindingFlags.Instance);
                    var mode = routerType.GetProperty("Mode",
                        BindingFlags.Public | BindingFlags.Instance);
                    Assert.IsNotNull(router, "第 " + cycle + " 次控制路由器没有创建。");
                    Assert.IsTrue((bool)toggleShip.Invoke(router, null), "第 " + cycle + " 次无法上船。");
                    Assert.AreEqual("Ship", mode.GetValue(router).ToString(),
                        "第 " + cycle + " 次上船后控制权没有切到船。");

                    var camera = Camera.main == null
                        ? UnityEngine.Object.FindFirstObjectByType<Camera>()
                        : Camera.main;
                    var cameraFollow = camera == null ? null : camera.GetComponent(cameraType);
                    var viewMode = cameraType.GetProperty("ViewMode");
                    var cycleView = cameraType.GetMethod("CycleViewMode",
                        BindingFlags.Public | BindingFlags.Instance);
                    Assert.IsNotNull(cameraFollow, "第 " + cycle + " 次主相机没有 CameraFollow。");
                    string beforeView = viewMode.GetValue(cameraFollow).ToString();
                    cycleView.Invoke(cameraFollow, null);
                    Assert.AreNotEqual(beforeView, viewMode.GetValue(cameraFollow).ToString(),
                        "第 " + cycle + " 次视角切换没有改变视角状态。");

                    Assert.IsTrue((bool)toggleShip.Invoke(router, null), "第 " + cycle + " 次无法下船。");
                    Assert.AreEqual("Character", mode.GetValue(router).ToString(),
                        "第 " + cycle + " 次下船后控制权没有恢复给人物。");
                    yield return null;
                    Assert.IsTrue((bool)grounded.GetValue(character), "第 " + cycle + " 次下船后玩家没有落地。");

                    saveAll.Invoke(null, null);
                    yield return null;

                    var pauseController = UnityEngine.Object.FindFirstObjectByType(pauseType,
                        FindObjectsInactive.Include) as Component;
                    Assert.IsNotNull(pauseController, "第 " + cycle + " 次暂停控制器没有创建。");
                    Press(keyboard.escapeKey, queueEventOnly: true);
                    yield return null;
                    Release(keyboard.escapeKey, queueEventOnly: true);
                    yield return null;

                    var isPaused = pauseType.GetProperty("IsPaused",
                        BindingFlags.Public | BindingFlags.Instance);
                    Assert.IsTrue((bool)isPaused.GetValue(pauseController),
                        "第 " + cycle + " 次 ESC 没有打开暂停菜单。");
                    Assert.AreEqual(0f, Time.timeScale, 0.001f,
                        "第 " + cycle + " 次暂停菜单没有停止时间。");

                    Button resumeButton = null;
                    yield return WaitForButton("HarborPauseCanvas/PausePanel/Resume", 3f,
                        value => resumeButton = value);
                    Assert.IsNotNull(resumeButton, "第 " + cycle + " 次继续按钮没有创建。");
                    Assert.IsTrue(resumeButton.interactable, "第 " + cycle + " 次继续按钮不可交互。");
                    resumeButton.onClick.Invoke();
                    yield return null;
                    Assert.AreEqual(1f, Time.timeScale, 0.001f,
                        "第 " + cycle + " 次继续后时间没有恢复。");

                    Press(keyboard.escapeKey, queueEventOnly: true);
                    yield return null;
                    Release(keyboard.escapeKey, queueEventOnly: true);
                    yield return null;

                    Button mainMenuButton = null;
                    yield return WaitForButton("HarborPauseCanvas/PausePanel/MainMenu", 3f,
                        value => mainMenuButton = value);
                    Assert.IsNotNull(mainMenuButton, "第 " + cycle + " 次返回主菜单按钮没有创建。");
                    Assert.IsTrue(mainMenuButton.interactable, "第 " + cycle + " 次返回主菜单按钮不可交互。");
                    mainMenuButton.onClick.Invoke();
                    yield return WaitForScene("MainMenu", 8f);
                    Assert.AreEqual(1f, Time.timeScale, 0.001f,
                        "第 " + cycle + " 次返回主菜单后时间没有恢复。");
                    Assert.IsNull(GameObject.Find("HarborPauseCanvas"),
                        "第 " + cycle + " 次返回主菜单后暂停画布残留。");

                    Button continueButton = null;
                    yield return WaitForButton("MainMenuCanvas/MainMenu/BtnContinue", 3f,
                        value => continueButton = value);
                    Assert.IsNotNull(continueButton, "第 " + cycle + " 次保存后继续按钮没有创建。");
                    Assert.IsTrue(continueButton.interactable,
                        "第 " + cycle + " 次保存后继续按钮仍不可交互，读取链断开。");
                    continueButton.onClick.Invoke();
                    yield return WaitForScene("InitialHarbor", 8f);
                    yield return WaitForRuntime(bootstrapType, 8f);
                    Assert.IsNotNull(GameObject.Find("D2_PlayerAvatar"),
                        "第 " + cycle + " 次继续读取后没有恢复玩家。");

                    Debug.Log("[D17/S02 Local] cycle " + cycle + "/10 passed");
                }
            }
            finally
            {
                Time.timeScale = 1f;
                setBlocked.Invoke(null, new object[] { false });
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.escapeKey);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
            }
        }

        private static IEnumerator LoadSceneAndWait(string sceneName)
        {
            var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, sceneName + " 不在 Build Settings 中。");
            while (!load.isDone) yield return null;
            yield return WaitForScene(sceneName, 8f);
        }

        private static IEnumerator WaitForScene(string sceneName, float timeoutSeconds)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (SceneManager.GetActiveScene().name != sceneName &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.AreEqual(sceneName, SceneManager.GetActiveScene().name,
                "场景没有在限定时间内切换到 " + sceneName + "。");
        }

        private static IEnumerator WaitForRuntime(Type bootstrapType, float timeoutSeconds)
        {
            var readyProperty = bootstrapType.GetProperty("IsReady",
                BindingFlags.Public | BindingFlags.Instance);
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (Time.realtimeSinceStartup < deadline)
            {
                var bootstrap = UnityEngine.Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                if (bootstrap != null && readyProperty != null &&
                    (bool)readyProperty.GetValue(bootstrap))
                    yield break;
                yield return null;
            }

            Assert.Fail("D2 运行时入口没有在限定时间内完成接线。");
        }

        private static IEnumerator WaitForButton(string path, float timeoutSeconds,
            Action<Button> assign)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (Time.realtimeSinceStartup < deadline)
            {
                var go = GameObject.Find(path);
                var button = go == null ? null : go.GetComponent<Button>();
                if (button != null)
                {
                    assign(button);
                    yield break;
                }
                yield return null;
            }

            assign(null);
        }

        private static Button FindButton(string path)
        {
            var target = GameObject.Find(path);
            return target == null ? null : target.GetComponent<Button>();
        }

        private static int CountActive<T>() where T : Component
        {
            int count = 0;
            foreach (var component in UnityEngine.Object.FindObjectsByType<T>(
                         FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (component.gameObject.activeInHierarchy) count++;
            }

            return count;
        }

        private static Type FindLoadedType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }
    }
}
