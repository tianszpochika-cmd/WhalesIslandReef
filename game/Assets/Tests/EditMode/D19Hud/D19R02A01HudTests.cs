using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Whalebone.Tests.D19Hud
{
    public sealed class D19R02A01HudTests
    {
        [Test]
        public void PrimaryHudPolicyHidesLegacyCanvasesAndKeepsRouteHudVisible()
        {
            var root = new GameObject("D19HudPolicyTestRoot");
            var coordinatorType = FindRuntimeType("Whalebone.Gameplay.UI.RuntimeHudCoordinator");
            Assert.That(coordinatorType, Is.Not.Null);
            root.AddComponent(coordinatorType);
            var legacy = CreateCanvas("D9PortServicePage", root.transform);
            var d10 = CreateCanvas("D10TavernSocialPage", root.transform);
            var primary = CreateCanvas("D19RouteHud", root.transform);

            coordinatorType.GetMethod("ApplyNow").Invoke(null, null);

            Assert.That(legacy.GetComponent<CanvasGroup>().alpha, Is.EqualTo(0f));
            Assert.That(d10.GetComponent<CanvasGroup>().alpha, Is.EqualTo(0f));
            Assert.That(primary.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f));
            Assert.That(primary.GetComponent<Canvas>().enabled, Is.True);

            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void TemporaryTradeCanvasIsMutuallyExclusiveAndRestoresPrimaryHud()
        {
            var root = new GameObject("D19HudTemporaryTestRoot");
            var coordinatorType = FindRuntimeType("Whalebone.Gameplay.UI.RuntimeHudCoordinator");
            Assert.That(coordinatorType, Is.Not.Null);
            root.AddComponent(coordinatorType);
            var trade = CreateCanvas("D3Canvas", root.transform);
            var primary = CreateCanvas("D19RouteHud", root.transform);

            coordinatorType.GetMethod("ShowTemporaryCanvas").Invoke(null, new object[] { "D3Canvas" });
            Assert.That(trade.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f));
            Assert.That(primary.GetComponent<CanvasGroup>().alpha, Is.EqualTo(0f));

            coordinatorType.GetMethod("RestorePrimaryHud").Invoke(null, null);
            Assert.That(trade.GetComponent<CanvasGroup>().alpha, Is.EqualTo(0f));
            Assert.That(primary.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f));

            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void RouteHudDoesNotInventFeedbackText()
        {
            var root = new GameObject("D19RouteHudTestRoot");
            var hudType = FindRuntimeType("Whalebone.Gameplay.UI.D19RouteHud.D19RouteHud");
            Assert.That(hudType, Is.Not.Null);
            var hud = root.AddComponent(hudType);
            hudType.GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(hud, null);
            hudType.GetMethod("UpdateHud").Invoke(hud,
                new object[] { "WASD 移动", "港口状态可用", string.Empty });

            var feedback = root.transform.Find("D19RouteHud/RoutePanel/Feedback");
            Assert.That(feedback, Is.Not.Null);
            Assert.That(feedback.GetComponent<Text>().text, Is.Empty);

            UnityEngine.Object.DestroyImmediate(root);
        }

        private static GameObject CreateCanvas(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            return go;
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
