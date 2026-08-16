using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WhalesIslandReef.Tests.D19Interaction
{
    public sealed class D19R02A02InteractionPlayModeTests
    {
        [UnityTest]
        public IEnumerator TenPublicBoardingCyclesPreserveModeAndFeedback()
        {
            var routerType = FindRuntimeType("Whalebone.Gameplay.D2.D2ControlRouter");
            var bootstrapType = FindRuntimeType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.That(routerType, Is.Not.Null);
            Assert.That(bootstrapType, Is.Not.Null);

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone) yield return null;

            Component router = null;
            var deadline = Time.realtimeSinceStartup + 8f;
            while (router == null && Time.realtimeSinceStartup < deadline)
            {
                router = UnityEngine.Object.FindFirstObjectByType(routerType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }
            Assert.That(router, Is.Not.Null);

            var toggle = routerType.GetMethod("TryToggleShip", BindingFlags.Public | BindingFlags.Instance);
            var mode = routerType.GetProperty("Mode", BindingFlags.Public | BindingFlags.Instance);
            var feedbackProperty = routerType.GetProperty("LastInteractionFeedback", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(toggle, Is.Not.Null);
            Assert.That(mode, Is.Not.Null);
            Assert.That(feedbackProperty, Is.Not.Null);

            for (var i = 0; i < 10; i++)
            {
                var boarded = (bool)toggle.Invoke(router, null);
                Assert.That(boarded, Is.True, "第 {0} 轮上船失败", i + 1);
                Assert.That(mode.GetValue(router).ToString(), Is.EqualTo("Ship"));
                Assert.That(GetString(feedbackProperty.GetValue(router), "Code"), Is.EqualTo("boarded"));

                var disembarked = (bool)toggle.Invoke(router, null);
                Assert.That(disembarked, Is.True, "第 {0} 轮下船失败", i + 1);
                Assert.That(mode.GetValue(router).ToString(), Is.EqualTo("Character"));
                Assert.That(GetString(feedbackProperty.GetValue(router), "Code"), Is.EqualTo("disembarked"));
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator MissingWalkableSurfaceReturnsActionableFailure()
        {
            var routerType = FindRuntimeType("Whalebone.Gameplay.D2.D2ControlRouter");
            Assert.That(routerType, Is.Not.Null);
            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone) yield return null;
            Component router = null;
            GameObject ship = null;
            var deadline = Time.realtimeSinceStartup + 8f;
            while ((router == null || ship == null) && Time.realtimeSinceStartup < deadline)
            {
                router = UnityEngine.Object.FindFirstObjectByType(routerType, FindObjectsInactive.Include) as Component;
                ship = GameObject.Find("ph_ship_fishing");
                yield return null;
            }
            Assert.That(router, Is.Not.Null);
            Assert.That(ship, Is.Not.Null);
            var toggle = routerType.GetMethod("TryToggleShip", BindingFlags.Public | BindingFlags.Instance);
            var mode = routerType.GetProperty("Mode", BindingFlags.Public | BindingFlags.Instance);
            var feedbackProperty = routerType.GetProperty("LastInteractionFeedback", BindingFlags.Public | BindingFlags.Instance);

            Assert.That((bool)toggle.Invoke(router, null), Is.True);
            var oldPosition = ship.transform.position;
            ship.transform.position += new Vector3(1000f, 0f, 1000f);
            try
            {
                Assert.That((bool)toggle.Invoke(router, null), Is.False);
                Assert.That(mode.GetValue(router).ToString(), Is.EqualTo("Ship"));
                Assert.That(GetString(feedbackProperty.GetValue(router), "Code"), Is.EqualTo("disembark_unavailable"));
                Assert.That(GetString(feedbackProperty.GetValue(router), "Message"), Is.Not.Empty);
            }
            finally
            {
                ship.transform.position = oldPosition;
            }
        }

        private static string GetString(object value, string property)
        {
            return (string)value.GetType().GetProperty(property).GetValue(value);
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
