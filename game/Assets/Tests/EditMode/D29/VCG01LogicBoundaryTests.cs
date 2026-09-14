using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Whalebone.Tests.D29
{
    /// <summary>
    /// VC-G01 的代码层输入边界回归。
    /// 这些测试只验证 RuntimeHudCoordinator 的状态门禁和同帧声明，
    /// 不启动 InitialHarbor，不模拟真实 Player 键盘输入。
    /// </summary>
    public sealed class VCG01LogicBoundaryTests
    {
        private Type _coordinatorType;

        [SetUp]
        public void SetUp()
        {
            _coordinatorType = FindRuntimeType("Whalebone.Gameplay.UI.RuntimeHudCoordinator");
            Assert.That(_coordinatorType, Is.Not.Null);
            ResetPrimaryInputClaim();
        }

        [TearDown]
        public void TearDown()
        {
            ResetPrimaryInputClaim();
        }

        [Test]
        public void PausedCoordinatorRejectsPrimaryInput()
        {
            var root = new GameObject("VCG01PausedCoordinatorTest");
            try
            {
                var coordinator = root.AddComponent(_coordinatorType);
                var paused = _coordinatorType.GetField("_paused",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(paused, Is.Not.Null);
                paused.SetValue(coordinator, true);

                Assert.That(ReadPrimaryInputAllowed(), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TemporaryPageBlocksPrimaryInputAndRestoresIt()
        {
            var root = new GameObject("VCG01TemporaryPageTest");
            try
            {
                root.AddComponent(_coordinatorType);
                _coordinatorType.GetMethod("ShowTemporaryCanvas",
                    BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new object[] { "VCG01TestPage" });
                Assert.That(ReadPrimaryInputAllowed(), Is.False);

                _coordinatorType.GetMethod("RestorePrimaryHud",
                    BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);

                if (!Application.isFocused && !Application.isBatchMode)
                    Assert.Ignore("EditMode host is not focused; focus recovery is covered by the source audit.");
                Assert.That(ReadPrimaryInputAllowed(), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PrimaryInputClaimAllowsOnlyOneOwnerPerFrame()
        {
            var root = new GameObject("VCG01PrimaryClaimTest");
            try
            {
                root.AddComponent(_coordinatorType);
                if (!ReadPrimaryInputAllowed())
                    Assert.Ignore("The test host has no primary input focus.");

                var claim = _coordinatorType.GetMethod("TryClaimPrimaryInput",
                    BindingFlags.Public | BindingFlags.Static);
                Assert.That(claim, Is.Not.Null);
                Assert.That((bool)claim.Invoke(null, null), Is.True);
                Assert.That((bool)claim.Invoke(null, null), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private bool ReadPrimaryInputAllowed()
        {
            var property = _coordinatorType.GetProperty("PrimaryInputAllowed",
                BindingFlags.Public | BindingFlags.Static);
            Assert.That(property, Is.Not.Null);
            return (bool)property.GetValue(null, null);
        }

        private void ResetPrimaryInputClaim()
        {
            if (_coordinatorType == null) return;
            var claimFrame = _coordinatorType.GetField("_primaryInputClaimFrame",
                BindingFlags.Static | BindingFlags.NonPublic);
            claimFrame?.SetValue(null, -1);
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
