using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace WhalesIslandReef.Tests.D19Interaction
{
    public sealed class D19R02A02InteractionTests
    {
        [Test]
        public void RouterPublishesReadableMissingTargetFeedback()
        {
            var type = FindRuntimeType("Whalebone.Gameplay.D2.D2ControlRouter");
            Assert.That(type, Is.Not.Null);
            var go = new GameObject("D19R02A02RouterTest");
            try
            {
                var router = go.AddComponent(type);
                var result = (object)type.GetMethod("TryToggleShip").Invoke(router, null);
                Assert.That(result, Is.EqualTo(false));
                var feedback = type.GetProperty("LastInteractionFeedback").GetValue(router);
                Assert.That(feedback, Is.Not.Null);
                Assert.That(GetString(feedback, "Code"), Is.EqualTo("ship_target_missing"));
                Assert.That(GetString(feedback, "Message"), Is.Not.Empty);
                Assert.That(GetString(feedback, "TargetStableId"), Is.Empty);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RouterHasPublicInteractionEntryAndReadOnlyFeedbackSnapshot()
        {
            var type = FindRuntimeType("Whalebone.Gameplay.D2.D2ControlRouter");
            Assert.That(type, Is.Not.Null);
            Assert.That(type.GetMethod("TryOpenCrewDialogue", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetMethod("ReadLastInteractionFeedback", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetEvent("InteractionFeedbackChanged", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
            Assert.That(type.GetProperty("LastInteractionFeedback", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        }

        [Test]
        public void FeedbackSnapshotCloneIsIndependent()
        {
            var feedbackType = FindRuntimeType("Whalebone.Gameplay.D2.D2InteractionFeedback");
            Assert.That(feedbackType, Is.Not.Null);
            var original = Activator.CreateInstance(feedbackType,
                "test_code", "test message", "target", true, "Character", 7);
            var clone = feedbackType.GetMethod("Clone").Invoke(original, null);
            Assert.That(clone, Is.Not.SameAs(original));
            Assert.That(GetString(clone, "Code"), Is.EqualTo("test_code"));
            Assert.That(GetString(clone, "TargetStableId"), Is.EqualTo("target"));
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
