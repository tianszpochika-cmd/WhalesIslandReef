using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Whalebone.Gameplay.D16;

namespace Whalebone.Tests.D16
{
    public sealed class D16LocalDiagnosticsPanelTests
    {
        [UnityTest]
        public IEnumerator D16A06PanelSupportsOptInDeleteAndClose()
        {
            D16LocalDiagnostics.RevokeConsentAndClear();

            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Button settingsButton = null;
            float deadline = Time.realtimeSinceStartup + 5f;
            while (settingsButton == null && Time.realtimeSinceStartup < deadline)
            {
                var settingsObject = GameObject.Find("BtnSettings");
                settingsButton = settingsObject == null ? null : settingsObject.GetComponent<Button>();
                if (settingsButton == null) yield return null;
            }
            Assert.IsNotNull(settingsButton, "主菜单设置按钮未创建。");
            settingsButton.onClick.Invoke();
            yield return null;

            var diagnosticsButtonObject = GameObject.Find("BtnDiagnostics");
            var diagnosticsButton = diagnosticsButtonObject == null
                ? null
                : diagnosticsButtonObject.GetComponent<Button>();
            Assert.IsNotNull(diagnosticsButton, "设置面板没有本地诊断入口。");
            diagnosticsButton.onClick.Invoke();
            yield return null;

            var modal = GameObject.Find("DiagnosticsModal");
            Assert.IsNotNull(modal, "本地诊断模态面板未创建。");
            Assert.IsTrue(modal.activeSelf, "本地诊断面板打开后仍是隐藏状态。");

            var consentObject = GameObject.Find("ConsentToggle");
            var consent = consentObject == null ? null : consentObject.GetComponent<Toggle>();
            Assert.IsNotNull(consent, "本地诊断同意开关未创建。");
            Assert.IsFalse(consent.isOn, "本地诊断必须默认关闭。");

            consent.isOn = true;
            yield return null;
            Assert.IsTrue(D16LocalDiagnostics.HasConsent, "同意开关没有启用本地诊断。");

            var deleteObject = GameObject.Find("DeleteAndDisable");
            var delete = deleteObject == null ? null : deleteObject.GetComponent<Button>();
            Assert.IsNotNull(delete, "删除本地诊断按钮未创建。");
            delete.onClick.Invoke();
            yield return null;
            Assert.IsFalse(D16LocalDiagnostics.HasConsent, "删除入口没有撤回诊断同意。");

            var closeObject = GameObject.Find("Close");
            var close = closeObject == null ? null : closeObject.GetComponent<Button>();
            Assert.IsNotNull(close, "本地诊断面板关闭按钮未创建。");
            close.onClick.Invoke();
            yield return null;
            Assert.IsFalse(modal.activeSelf, "本地诊断面板关闭后仍拦截界面。");

            D16LocalDiagnostics.RevokeConsentAndClear();
        }
    }
}
