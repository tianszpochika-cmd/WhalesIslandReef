using System;
using System.Reflection;
using Save.Tests;

internal static class Program
{
    private static int Main()
    {
        var type = typeof(D16AccessibilityTests);
        var names = new[]
        {
            "DefaultsCoverCoreKeyboardAndGamepadActions",
            "InvalidScaleAndLocaleAreRejected",
            "DuplicateBindingsAreRejectedBeforeSave",
            "BindingCanBeChangedWithoutChangingOtherActions",
            "ContextBindingsMayReuseKeysButSameScopeCannot",
            "NumberAndPercentFormattingIsStableForBothLocales",
            "SensitiveContentRequiresWarningAndSupportsSkip",
        };

        int passed = 0;
        foreach (var name in names)
        {
            var test = Activator.CreateInstance(type);
            try
            {
                type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance).Invoke(test, null);
                Console.WriteLine("[OK] " + name);
                passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FAIL] " + name + ": " + (ex.InnerException ?? ex));
                return 1;
            }
        }

        Console.WriteLine("D16-A03 accessibility tests PASS count=" + passed);
        return 0;
    }
}
