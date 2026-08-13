using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Whalebone.Systems.D16;

namespace Whalebone.Tests.D16
{
    public sealed class D16SensitiveContentOverlayTests
    {
        [UnityTest]
        public IEnumerator SensitiveContentWarningSupportsContinueSkipAndHiddenPreference()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var settingsType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16AccessibilitySettingsStore, Assembly-CSharp");
            var overlayType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16SensitiveContentOverlay, Assembly-CSharp");
            var routerType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16InputRouter, Assembly-CSharp");
            Assert.IsNotNull(settingsType, "无障碍设置存储未被 Unity 主程序集加载。");
            Assert.IsNotNull(overlayType, "敏感内容提示层未被 Unity 主程序集加载。");
            Assert.IsNotNull(routerType, "D16InputRouter 未被 Unity 主程序集加载。");

            var currentProperty = settingsType.GetProperty("Current", BindingFlags.Static | BindingFlags.Public);
            var save = settingsType.GetMethod("Save", BindingFlags.Static | BindingFlags.Public);
            var hide = overlayType.GetMethod("Hide", BindingFlags.Static | BindingFlags.Public);
            var present = overlayType.GetMethod("PresentWarning", BindingFlags.Static | BindingFlags.Public);
            var isOpen = overlayType.GetProperty("IsOpen", BindingFlags.Static | BindingFlags.Public);
            var inputBlocked = routerType.GetProperty("GameplayInputBlocked",
                BindingFlags.Static | BindingFlags.Public);
            Assert.IsNotNull(currentProperty, "设置存储缺少 Current。");
            Assert.IsNotNull(save, "设置存储缺少 Save。");
            Assert.IsNotNull(hide, "提示层缺少 Hide。");
            Assert.IsNotNull(present, "提示层缺少 PresentWarning。");
            Assert.IsNotNull(isOpen, "提示层缺少 IsOpen。");
            Assert.IsNotNull(inputBlocked, "输入路由缺少 GameplayInputBlocked。");

            var originalSettings = currentProperty.GetValue(null);
            var originalClone = originalSettings.GetType().GetMethod("Clone")?.Invoke(originalSettings, null);
            var continueCount = 0;
            var skipCount = 0;
            try
            {
                var warningSettings = Clone(originalClone);
                WriteMember(warningSettings, "SensitiveWarnings", true);
                save.Invoke(null, new[] { warningSettings });

                var showDecision = (D16SensitiveContentDecision)present.Invoke(null, new object[]
                {
                    "测试警告：包含敏感历史主题。",
                    true,
                    (Action)(() => continueCount++),
                    (Action)(() => skipCount++),
                });
                Assert.AreEqual(D16SensitiveContentDecision.ShowWarning, showDecision);
                Assert.IsTrue((bool)isOpen.GetValue(null), "敏感内容警告没有显示。");
                Assert.IsTrue((bool)inputBlocked.GetValue(null), "敏感内容警告没有阻断玩法输入。");

                var host = FindComponent(overlayType);
                Assert.IsNotNull(host, "敏感内容提示层实例未创建。");
                var continueButton = FindChild(host.transform, "Continue")?.GetComponent<Button>();
                var skipButton = FindChild(host.transform, "Skip")?.GetComponent<Button>();
                Assert.IsNotNull(continueButton, "继续按钮未创建。");
                Assert.IsNotNull(skipButton, "跳过按钮未创建。");

                continueButton.onClick.Invoke();
                yield return null;
                Assert.AreEqual(1, continueCount, "点击继续没有调用内容继续回调。");
                Assert.AreEqual(0, skipCount, "点击继续错误调用了跳过回调。");
                Assert.IsFalse((bool)isOpen.GetValue(null), "点击继续后警告仍然显示。");
                Assert.IsFalse((bool)inputBlocked.GetValue(null), "点击继续后玩法输入仍被阻断。");

                var secondShow = (D16SensitiveContentDecision)present.Invoke(null, new object[]
                {
                    "第二条测试警告。",
                    true,
                    (Action)(() => continueCount++),
                    (Action)(() => skipCount++),
                });
                Assert.AreEqual(D16SensitiveContentDecision.ShowWarning, secondShow);
                host = FindComponent(overlayType);
                FindChild(host.transform, "Skip").GetComponent<Button>().onClick.Invoke();
                yield return null;
                Assert.AreEqual(1, continueCount, "点击跳过后错误调用了继续回调。");
                Assert.AreEqual(1, skipCount, "点击跳过没有调用跳过回调。");
                Assert.IsFalse((bool)isOpen.GetValue(null), "点击跳过后警告仍然显示。");

                var hiddenSettings = Clone(originalClone);
                WriteMember(hiddenSettings, "SensitiveWarnings", false);
                save.Invoke(null, new[] { hiddenSettings });
                var hiddenDecision = (D16SensitiveContentDecision)present.Invoke(null, new object[]
                {
                    "已关闭警告的测试内容。",
                    true,
                    (Action)(() => continueCount++),
                    (Action)(() => skipCount++),
                });
                Assert.AreEqual(D16SensitiveContentDecision.HiddenByPreference, hiddenDecision);
                Assert.AreEqual(2, skipCount, "关闭敏感警告后没有按偏好跳过内容。");
                Assert.IsFalse((bool)isOpen.GetValue(null), "关闭敏感警告后不应打开提示层。");
            }
            finally
            {
                save.Invoke(null, new[] { originalClone });
                hide.Invoke(null, null);
            }
        }

        private static object Clone(object value)
        {
            return value.GetType().GetMethod("Clone")?.Invoke(value, null);
        }

        private static MonoBehaviour FindComponent(Type componentType)
        {
            if (componentType == null) return null;
            foreach (var component in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (component != null && componentType.IsInstanceOfType(component)) return component;
            return null;
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindChild(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static void WriteMember(object target, string memberName, object value)
        {
            var field = target.GetType().GetField(memberName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            var property = target.GetType().GetProperty(memberName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.CanWrite) property.SetValue(target, value, null);
        }
    }
}
