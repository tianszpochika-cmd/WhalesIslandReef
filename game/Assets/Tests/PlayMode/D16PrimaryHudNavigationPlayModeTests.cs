using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Whalebone.Tests.D16
{
    /// <summary>
    /// VC-G01 原生 Player 前的输入/UI 集成门禁：正常游戏移动时，
    /// 主 HUD 不是模态菜单，W/S 不得改变 UGUI 选中项。
    /// </summary>
    public sealed class D16PrimaryHudNavigationPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator GameplayMovementKeysDoNotNavigatePrimaryHud()
        {
            var inputRouterType = FindRuntimeType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            var modalScopeType = FindRuntimeType("Whalebone.Gameplay.UI.D16.D16ModalFocusScope");
            Assert.That(inputRouterType, Is.Not.Null, "D16InputRouter 未被 Unity 主程序集加载。");
            Assert.That(modalScopeType, Is.Not.Null, "D16ModalFocusScope 未被 Unity 主程序集加载。");
            var setGameplayInputBlocked = inputRouterType.GetMethod("SetGameplayInputBlocked");
            var moveAxis = inputRouterType.GetMethod("MoveAxis");
            Assert.That(setGameplayInputBlocked, Is.Not.Null);
            Assert.That(moveAxis, Is.Not.Null);
            setGameplayInputBlocked.Invoke(null, new object[] { false });

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone) yield return null;

            EventSystem eventSystem = null;
            GameObject quickActions = null;
            Button market = null;
            var deadline = Time.realtimeSinceStartup + 12f;
            while ((eventSystem == null || quickActions == null || market == null) &&
                   Time.realtimeSinceStartup < deadline)
            {
                eventSystem = EventSystem.current;
                var canvas = GameObject.Find("D29InteractionHudCanvas");
                quickActions = canvas == null ? null : canvas.transform.Find("QuickActions")?.gameObject;
                market = quickActions == null ? null : quickActions.transform.Find("Market")?.GetComponent<Button>();
                yield return null;
            }

            Assert.That(eventSystem, Is.Not.Null, "InitialHarbor 必须有唯一 EventSystem。");
            Assert.That(quickActions, Is.Not.Null, "D29 主 HUD QuickActions 未创建。");
            Assert.That(market, Is.Not.Null, "D29 主 HUD Market 按钮未创建。");
            Assert.That(quickActions.GetComponent(modalScopeType), Is.Null,
                "正常游戏主 HUD 不能拥有模态焦点范围。");

            var bootstrapInput = eventSystem.GetComponent<InputSystemUIInputModule>();
            Assert.That(bootstrapInput, Is.Not.Null,
                "InitialHarbor 主 EventSystem 必须使用 InputSystemUIInputModule。");
            Assert.That(bootstrapInput.move, Is.Null,
                "D29 非模态 HUD 创建后必须立即移除 UI/Navigate，不能等待稳定器下一帧。");

            // 等待交互稳定器执行一次导航策略，把正常游戏状态切到 gameplay_suppressed。
            yield return null;
            yield return null;
            Assert.That(eventSystem.sendNavigationEvents, Is.False,
                "正常游戏状态必须关闭 UGUI 键盘导航，避免 W/S 与主 HUD 冲突。");
            Assert.That(market.navigation.mode, Is.EqualTo(Navigation.Mode.None),
                "正常游戏主 HUD 按钮必须关闭自身导航，避免 Player 残留 selection 让 W/S 切换菜单。");

            eventSystem.SetSelectedGameObject(market.gameObject);
            var initialSelection = eventSystem.currentSelectedGameObject;
            Assert.That(initialSelection, Is.SameAs(market.gameObject));

            Keyboard keyboard = null;
            try
            {
                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();

                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                var forwardAxis = (Vector2)moveAxis.Invoke(null, null);
                Assert.Greater(forwardAxis.y, 0f,
                    "W 仍必须进入 Gameplay 正向移动意图。");
                yield return null;
                Assert.That(eventSystem.currentSelectedGameObject, Is.SameAs(initialSelection),
                    "W 移动时不应切换主 HUD 选中项。");

                Release(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                Press(keyboard.sKey, queueEventOnly: true);
                InputSystem.Update();
                var backwardAxis = (Vector2)moveAxis.Invoke(null, null);
                Assert.Less(backwardAxis.y, 0f,
                    "S 仍必须进入 Gameplay 反向移动意图。");
                yield return null;
                Assert.That(eventSystem.currentSelectedGameObject, Is.SameAs(initialSelection),
                    "S 移动时不应切换主 HUD 选中项。");
            }
            finally
            {
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.wKey, queueEventOnly: true);
                    Release(keyboard.sKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
                setGameplayInputBlocked.Invoke(null, new object[] { false });
            }
        }

        [UnityTest]
        public IEnumerator GameplaySuppressionReassertsWhenUiMoveActionIsRestored()
        {
            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone) yield return null;

            EventSystem eventSystem = null;
            InputSystemUIInputModule input = null;
            var deadline = Time.realtimeSinceStartup + 12f;
            while ((eventSystem == null || input == null) && Time.realtimeSinceStartup < deadline)
            {
                eventSystem = EventSystem.current;
                input = eventSystem == null ? null : eventSystem.GetComponent<InputSystemUIInputModule>();
                yield return null;
            }

            Assert.That(eventSystem, Is.Not.Null, "InitialHarbor 必须有 EventSystem。");
            Assert.That(input, Is.Not.Null, "InitialHarbor 必须有 InputSystemUIInputModule。");
            Assert.That(eventSystem.sendNavigationEvents, Is.False,
                "正常游戏状态必须保持 UGUI 导航关闭。");
            Assert.That(input.move, Is.Null,
                "正常游戏状态必须移除 InputSystemUIInputModule.move。");

            var navigate = input.actionsAsset == null
                ? null
                : input.actionsAsset.FindAction("UI/Navigate", false);
            Assert.That(navigate, Is.Not.Null, "InputSystem UI actions 必须包含 UI/Navigate。");

            var restoredMove = InputActionReference.Create(navigate);
            input.move = restoredMove;
            yield return null;

            Assert.That(eventSystem.sendNavigationEvents, Is.False,
                "恢复错误的 UI move action 后仍必须保持 gameplay 导航策略。");
            Assert.That(input.move, Is.Null,
                "HarborInteractionStabilizer 必须重新移除被其他 owner 恢复的 UI move action。");

            if (restoredMove != null)
                Object.Destroy(restoredMove);
        }

        private static System.Type FindRuntimeType(string fullName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }

            return null;
        }
    }
}
