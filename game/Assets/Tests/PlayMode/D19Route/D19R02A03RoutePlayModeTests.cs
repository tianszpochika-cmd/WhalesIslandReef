using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Whalebone.Systems.Save;

namespace WhalesIslandReef.Tests.D19Route
{
    public sealed class D19R02A03RoutePlayModeTests
    {
        [UnityTest]
        public IEnumerator TradeCloseAndSavePublishActionableRouteEvidence()
        {
            yield return LoadInitialHarbor();
            var type = FindRuntimeType("Whalebone.Gameplay.D3.D3RuntimeBootstrap");
            var bootstrap = FindFirstComponent(type);
            Assert.That(bootstrap, Is.Not.Null);

            var requestTrade = type.GetMethod("RequestTrade", BindingFlags.Public | BindingFlags.Instance);
            var isTradeOpen = type.GetProperty("IsTradeOpen", BindingFlags.Public | BindingFlags.Instance);
            var closeTrade = type.GetMethod("CloseTradeFromCoordinator", BindingFlags.Public | BindingFlags.Instance);
            var requestSave = type.GetMethod("RequestSave", BindingFlags.Public | BindingFlags.Instance);
            var feedback = type.GetProperty("LastRouteFeedback", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(requestTrade, Is.Not.Null);
            Assert.That(closeTrade, Is.Not.Null);
            Assert.That(requestSave, Is.Not.Null);
            Assert.That(feedback, Is.Not.Null);

            Assert.That((bool)requestTrade.Invoke(bootstrap, null), Is.True,
                "港内可交易路线入口没有打开交易表现层");
            Assert.That((bool)isTradeOpen.GetValue(bootstrap), Is.True);
            Assert.That(GetString(feedback.GetValue(bootstrap), "Code"), Is.EqualTo("trade_open"));

            closeTrade.Invoke(bootstrap, null);
            Assert.That((bool)isTradeOpen.GetValue(bootstrap), Is.False);
            Assert.That(GetString(feedback.GetValue(bootstrap), "Code"), Is.EqualTo("trade_closed"));

            var saved = (bool)requestSave.Invoke(bootstrap, null);
            Assert.That(saved, Is.True, "D3 路线保存入口未能保存当前快照");
            var saveFeedback = feedback.GetValue(bootstrap);
            Assert.That(GetString(saveFeedback, "Code"), Is.EqualTo("save_result"));
            Assert.That(GetString(saveFeedback, "Message"), Is.Not.Empty);
            Assert.That((bool)type.GetProperty("LastSaveSucceeded").GetValue(bootstrap), Is.True);
        }

        [UnityTest]
        public IEnumerator DepartureAdvanceAndReturnUseSystemOwnedState()
        {
            yield return LoadInitialHarbor();
            var type = FindRuntimeType("Whalebone.Gameplay.D3.D3RuntimeBootstrap");
            var bootstrap = FindFirstComponent(type);
            Assert.That(bootstrap, Is.Not.Null);
            var requestDeparture = type.GetMethod("RequestDeparture", BindingFlags.Public | BindingFlags.Instance);
            var requestAdvance = type.GetMethod("RequestAdvanceVoyage", BindingFlags.Public | BindingFlags.Instance);
            var requestLand = type.GetMethod("RequestLandAndSettle", BindingFlags.Public | BindingFlags.Instance);
            var snapshot = type.GetProperty("Snapshot", BindingFlags.Public | BindingFlags.Instance);
            var feedback = type.GetProperty("LastRouteFeedback", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(requestDeparture, Is.Not.Null);
            Assert.That(requestAdvance, Is.Not.Null);
            Assert.That(requestLand, Is.Not.Null);
            Assert.That(snapshot, Is.Not.Null);

            var departed = (bool)requestDeparture.Invoke(bootstrap, null);
            if (!departed)
            {
                var failedFeedback = feedback.GetValue(bootstrap);
                Assert.That(GetString(failedFeedback, "Message"), Is.Not.Empty);
                yield break;
            }

            var arrived = false;
            for (var i = 0; i < 12; i++)
            {
                requestAdvance.Invoke(bootstrap, null);
                var state = snapshot.GetValue(bootstrap);
                var status = GetString(state, "VoyageStatus");
                if (status == "Arrived" || status == "Failed")
                {
                    arrived = status == "Arrived";
                    break;
                }
                yield return null;
            }

            Assert.That(arrived, Is.True, "出航后未在固定测试窗口内抵达可返港状态");
            Assert.That((bool)requestLand.Invoke(bootstrap, null), Is.True);
            Assert.That(GetString(snapshot.GetValue(bootstrap), "VoyageStatus"), Is.EqualTo("InPort"));
        }

        private static IEnumerator LoadInitialHarbor()
        {
            var type = FindRuntimeType("Whalebone.Gameplay.D3.D3RuntimeBootstrap");
            Assert.That(type, Is.Not.Null);
            SaveSystem.NewGame(new NewGameParams
            {
                CaptainName = "D19-A03-Test-Captain",
                ShipTypeId = "small_fishing"
            });
            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone) yield return null;

            var deadline = Time.realtimeSinceStartup + 15f;
            Component bootstrap = null;
            while (bootstrap == null && Time.realtimeSinceStartup < deadline)
            {
                bootstrap = FindFirstComponent(type);
                yield return null;
            }
            Assert.That(bootstrap, Is.Not.Null);
            var ready = type.GetProperty("IsReady", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(ready, Is.Not.Null);
            deadline = Time.realtimeSinceStartup + 15f;
            while (!(bool)ready.GetValue(bootstrap) && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That((bool)ready.GetValue(bootstrap), Is.True);
        }

        private static Component FindFirstComponent(Type type)
        {
            return UnityEngine.Object.FindFirstObjectByType(type, FindObjectsInactive.Include) as Component;
        }

        private static string GetString(object value, string property)
        {
            Assert.That(value, Is.Not.Null, "读取路线状态时快照为空");
            var valueProperty = value.GetType().GetProperty(property,
                BindingFlags.Public | BindingFlags.Instance);
            if (valueProperty != null) return valueProperty.GetValue(value).ToString();
            var valueField = value.GetType().GetField(property,
                BindingFlags.Public | BindingFlags.Instance);
            Assert.That(valueField, Is.Not.Null, "路线状态成员不存在：" + property);
            return valueField.GetValue(value).ToString();
        }

        private static Type FindRuntimeType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }
    }
}
