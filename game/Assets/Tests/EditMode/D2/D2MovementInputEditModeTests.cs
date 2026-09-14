using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace D2.Tests
{
    /// <summary>
    /// D2 移动入口的 EditMode 规则回归。通过反射读取运行时程序集，
    /// 避免测试程序集反向引用 Assembly-CSharp，同时覆盖权限归零和 provider 单一消费。
    /// </summary>
    public sealed class D2MovementInputEditModeTests
    {
        [Test]
        public void ModalOrPrimaryHudBlock_ResolvesMovementToZero()
        {
            var modalBlocked = ResolveAxis(new Vector2(0.8f, 0.6f), true, false, true);
            var primaryHudBlocked = ResolveAxis(new Vector2(0.8f, 0.6f), true, true, false);

            Assert.That(modalBlocked, Is.EqualTo(Vector2.zero));
            Assert.That(primaryHudBlocked, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void FocusRecovery_AllowsMovementAgainAfterTheGateReopens()
        {
            var axis = new Vector2(0f, 1f);
            var blocked = ResolveAxis(axis, true, false, true);
            var recovered = ResolveAxis(axis, true, true, true);

            Assert.That(blocked, Is.EqualTo(Vector2.zero));
            Assert.That(recovered, Is.EqualTo(axis));
        }

        [Test]
        public void MovementSource_ConsumesOnlyTheSelectedProviderPerFrame()
        {
            int d16Reads = 0;
            int legacyReads = 0;
            var selector = CreateSelector(
                () => { d16Reads++; return Vector2.zero; },
                () => { legacyReads++; return Vector2.right; });

            var first = ReadAxis(selector);
            Assert.That(first, Is.EqualTo(Vector2.zero));
            Assert.That(d16Reads, Is.EqualTo(1));
            Assert.That(legacyReads, Is.EqualTo(0));
            Assert.That(GetProperty<int>(selector, "LegacyFallbackActivationCount"), Is.EqualTo(0));

            Invoke(selector, "MarkD16RouteUnavailable");
            var second = ReadAxis(selector);
            Assert.That(second, Is.EqualTo(Vector2.right));
            Assert.That(d16Reads, Is.EqualTo(1));
            Assert.That(legacyReads, Is.EqualTo(1));
            Assert.That(GetProperty<object>(selector, "Source").ToString(),
                Is.EqualTo("LegacyKeyboardFallback"));
            Assert.That(GetProperty<int>(selector, "LegacyFallbackActivationCount"), Is.EqualTo(1));

            Invoke(selector, "MarkD16RouteUnavailable");
            ReadAxis(selector);
            Assert.That(d16Reads, Is.EqualTo(1));
            Assert.That(legacyReads, Is.EqualTo(2));
            Assert.That(GetProperty<int>(selector, "LegacyFallbackActivationCount"), Is.EqualTo(1));
        }

        [Test]
        public void D16RouteFailure_DefersLegacyFallbackUntilTheNextRead()
        {
            int d16Reads = 0;
            int legacyReads = 0;
            var selector = CreateSelector(
                () =>
                {
                    d16Reads++;
                    throw new InvalidOperationException("synthetic D16 route failure");
                },
                () => { legacyReads++; return Vector2.up; });

            var failureFrame = ReadAxis(selector);
            Assert.That(failureFrame, Is.EqualTo(Vector2.zero));
            Assert.That(d16Reads, Is.EqualTo(1));
            Assert.That(legacyReads, Is.EqualTo(0));
            Assert.That(GetProperty<string>(selector, "LastD16Failure"), Is.EqualTo("InvalidOperationException"));

            var fallbackFrame = ReadAxis(selector);
            Assert.That(fallbackFrame, Is.EqualTo(Vector2.up));
            Assert.That(d16Reads, Is.EqualTo(1));
            Assert.That(legacyReads, Is.EqualTo(1));
            Assert.That(GetProperty<int>(selector, "LegacyFallbackActivationCount"), Is.EqualTo(1));
        }

        [Test]
        public void ExplicitlyUnavailableD16Route_UsesLegacyWithoutReadingD16()
        {
            int d16Reads = 0;
            int legacyReads = 0;
            var selector = CreateSelector(
                () => { d16Reads++; return Vector2.up; },
                () => { legacyReads++; return Vector2.left; },
                false);

            var axis = ReadAxis(selector);

            Assert.That(axis, Is.EqualTo(Vector2.left));
            Assert.That(d16Reads, Is.EqualTo(0));
            Assert.That(legacyReads, Is.EqualTo(1));
            Assert.That(GetProperty<int>(selector, "LegacyFallbackActivationCount"), Is.EqualTo(1));
        }

        private static Vector2 ResolveAxis(Vector2 axis, bool inputEnabled,
            bool gameplayInputAllowed, bool primaryInputAllowed)
        {
            var type = FindRuntimeType("Whalebone.Gameplay.D2.D2MovementInputPolicy");
            var method = type.GetMethod("ResolveAxis", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "D2MovementInputPolicy.ResolveAxis 未加载。");
            return (Vector2)method.Invoke(null, new object[]
            {
                axis, inputEnabled, gameplayInputAllowed, primaryInputAllowed,
            });
        }

        private static object CreateSelector(Func<Vector2> d16Reader, Func<Vector2> legacyReader,
            bool d16RouteAvailable = true)
        {
            var type = FindRuntimeType("Whalebone.Gameplay.D2.D2MovementInputSourceSelector");
            var constructor = type.GetConstructor(new[]
            {
                typeof(Func<Vector2>), typeof(Func<Vector2>), typeof(bool),
            });
            Assert.IsNotNull(constructor, "D2MovementInputSourceSelector 构造函数未加载。");
            return constructor.Invoke(new object[] { d16Reader, legacyReader, d16RouteAvailable });
        }

        private static Vector2 ReadAxis(object selector)
        {
            return (Vector2)Invoke(selector, "ReadAxis");
        }

        private static object Invoke(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName,
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(method, "缺少运行时方法：" + methodName);
            return method.Invoke(target, null);
        }

        private static T GetProperty<T>(object target, string propertyName)
        {
            var property = target.GetType().GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, "缺少运行时属性：" + propertyName);
            return (T)property.GetValue(target, null);
        }

        private static Type FindRuntimeType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }

            Assert.Fail("运行时类型未加载：" + fullName);
            return null;
        }
    }
}
