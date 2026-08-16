using System;
using System.Reflection;
using NUnit.Framework;

namespace WhalesIslandReef.Tests.D19Route
{
    public sealed class D19R02A03RouteTests
    {
        [Test]
        public void D3RouteExposesReadOnlyRouteAndSaveEntryPoints()
        {
            var type = FindRuntimeType("Whalebone.Gameplay.D3.D3RuntimeBootstrap");
            Assert.That(type, Is.Not.Null);
            Assert.That(type.GetMethod("RequestTrade", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetMethod("RequestDeparture", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetMethod("RequestAdvanceVoyage", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetMethod("RequestLandAndSettle", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetMethod("RequestSave", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetMethod("ReadRouteFeedback", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetProperty("LastRouteFeedback", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetProperty("LastSaveSucceeded", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetEvent("RouteFeedbackChanged", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        }

        [Test]
        public void RouteFeedbackClonePreservesEvidenceWithoutSharingInstance()
        {
            var type = FindRuntimeType("Whalebone.Gameplay.D3.D3RouteFeedback");
            Assert.That(type, Is.Not.Null);
            var original = Activator.CreateInstance(type,
                "save_result", "保存已完成", "InPort", "saint_margaret_port", 14L, 3);
            var clone = type.GetMethod("Clone", BindingFlags.Public | BindingFlags.Instance).Invoke(original, null);
            Assert.That(clone, Is.Not.SameAs(original));
            Assert.That(GetString(clone, "Code"), Is.EqualTo("save_result"));
            Assert.That(GetString(clone, "Message"), Is.Not.Empty);
            Assert.That(GetInt64(clone, "SaveRevision"), Is.EqualTo(14L));
            Assert.That(GetInt32(clone, "Sequence"), Is.EqualTo(3));
        }

        [Test]
        public void MainMenuRetainsContinueAndNewGameRouteHooks()
        {
            var type = FindRuntimeType("Whalebone.Gameplay.UI.MainMenuController");
            Assert.That(type, Is.Not.Null);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(Array.Exists(methods, m => m.Name.IndexOf("NewGame", StringComparison.OrdinalIgnoreCase) >= 0), Is.True);
            Assert.That(Array.Exists(methods, m => m.Name.IndexOf("Continue", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.Name.IndexOf("Load", StringComparison.OrdinalIgnoreCase) >= 0), Is.True);
        }

        private static string GetString(object value, string property)
        {
            return (string)value.GetType().GetProperty(property).GetValue(value);
        }

        private static long GetInt64(object value, string property)
        {
            return (long)value.GetType().GetProperty(property).GetValue(value);
        }

        private static int GetInt32(object value, string property)
        {
            return (int)value.GetType().GetProperty(property).GetValue(value);
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
