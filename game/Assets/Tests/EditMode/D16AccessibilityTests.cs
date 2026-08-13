using System.Linq;
using NUnit.Framework;
using Whalebone.Systems.D16;

namespace Save.Tests
{
    public sealed class D16AccessibilityTests
    {
        [Test]
        public void DefaultsCoverCoreKeyboardAndGamepadActions()
        {
            var settings = D16AccessibilityDefaults.Create();

            Assert.IsEmpty(settings.Validate());
            CollectionAssert.Contains(
                settings.InputBindings.Select(binding => binding.Action).ToArray(),
                D16InputAction.Pause);
            CollectionAssert.Contains(
                settings.InputBindings.Select(binding => binding.GamepadButton).ToArray(),
                "South");
            CollectionAssert.Contains(
                settings.InputBindings.Select(binding => binding.Action).ToArray(),
                D16InputAction.D3StartVoyage);
            CollectionAssert.Contains(
                settings.InputBindings.Select(binding => binding.Action).ToArray(),
                D16InputAction.D10OpenTavern);
        }

        [Test]
        public void InvalidScaleAndLocaleAreRejected()
        {
            var settings = D16AccessibilityDefaults.Create();
            settings.Locale = "fr-FR";
            settings.TextScale = 3f;

            CollectionAssert.Contains(settings.Validate(), "unsupported_locale");
            CollectionAssert.Contains(settings.Validate(), "text_scale_out_of_range");
        }

        [Test]
        public void DuplicateBindingsAreRejectedBeforeSave()
        {
            var settings = D16AccessibilityDefaults.Create();
            string error;
            Assert.IsFalse(settings.TrySetBinding(
                D16InputAction.MoveBack,
                D16InputBindingDevice.PrimaryKeyboard,
                settings.InputBindings[0].PrimaryKeyboard,
                out error));

            Assert.IsTrue(error.StartsWith("duplicate_keyboard_binding:"));
        }

        [Test]
        public void BindingCanBeChangedWithoutChangingOtherActions()
        {
            var settings = D16AccessibilityDefaults.Create();
            string error;
            Assert.IsTrue(settings.TrySetBinding(
                D16InputAction.CameraCycle,
                D16InputBindingDevice.PrimaryKeyboard,
                "F4",
                out error));
            Assert.IsEmpty(error);
            Assert.AreEqual("F4", settings.InputBindings
                .Single(binding => binding.Action == D16InputAction.CameraCycle).PrimaryKeyboard);
            Assert.AreEqual("W", settings.InputBindings
                .Single(binding => binding.Action == D16InputAction.MoveForward).PrimaryKeyboard);
        }

        [Test]
        public void ContextBindingsMayReuseKeysButSameScopeCannot()
        {
            var settings = D16AccessibilityDefaults.Create();
            string error;

            Assert.IsTrue(settings.TrySetBinding(
                D16InputAction.D4Board,
                D16InputBindingDevice.PrimaryKeyboard,
                "B",
                out error));
            Assert.IsEmpty(error);

            Assert.IsFalse(settings.TrySetBinding(
                D16InputAction.D4EscapeCombat,
                D16InputBindingDevice.PrimaryKeyboard,
                "B",
                out error));
            Assert.IsTrue(error.StartsWith("duplicate_keyboard_binding:"));
        }

        [Test]
        public void NumberAndPercentFormattingIsStableForBothLocales()
        {
            Assert.AreEqual("1,234.50", D16DisplayFormatter.Number(1234.5m, "en-US", 2));
            Assert.AreEqual("1,234.50", D16DisplayFormatter.Number(1234.5m, "zh-CN", 2));
            Assert.AreEqual("-12", D16DisplayFormatter.SignedNumber(-12m, "en-US"));
            Assert.AreEqual("25%", D16DisplayFormatter.Percent(0.25m, "zh-CN"));
        }

        [Test]
        public void SensitiveContentRequiresWarningAndSupportsSkip()
        {
            Assert.AreEqual(
                D16SensitiveContentDecision.ShowWarning,
                D16SensitiveContentPolicy.Decide(true, true, false, false));
            Assert.AreEqual(
                D16SensitiveContentDecision.ShowContent,
                D16SensitiveContentPolicy.Decide(true, true, true, false));
            Assert.AreEqual(
                D16SensitiveContentDecision.SkipContent,
                D16SensitiveContentPolicy.Decide(true, true, false, true));
            Assert.AreEqual(
                D16SensitiveContentDecision.HiddenByPreference,
                D16SensitiveContentPolicy.Decide(true, false, false, false));
        }
    }
}
