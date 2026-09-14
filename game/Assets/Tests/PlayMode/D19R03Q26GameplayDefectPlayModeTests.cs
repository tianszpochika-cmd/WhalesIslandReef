using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Whalebone.Systems.Save;

namespace Whalebone.Tests.D19R03Q26
{
    /// <summary>
    /// Q26 diagnostics for the defects reported from the native Player.
    /// These tests use the real InitialHarbor runtime and the Input System,
    /// but they do not claim final native-window visual acceptance.
    /// </summary>
    public sealed class D19R03Q26GameplayDefectPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator Q26A01RealKeyboardInputMovesPlayerWhileSwimming()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            var inputRouterType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");
            Assert.IsNotNull(inputRouterType, "D16InputRouter 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            var animation = player.GetComponent(animationType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var isSwimming = characterType.GetProperty("IsSwimming");
            var waterSurfaceY = characterType.GetProperty("WaterSurfaceY");
            var action = animationType.GetProperty("Action");
            Assert.IsNotNull(setInputEnabled, "角色缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "角色缺少 SetWorldPosition。");
            Assert.IsNotNull(isSwimming, "角色缺少 IsSwimming。");
            Assert.IsNotNull(waterSurfaceY, "角色缺少 WaterSurfaceY。");
            Assert.IsNotNull(action, "动作控制器缺少 Action。");

            var setBlocked = inputRouterType.GetMethod("SetGameplayInputBlocked");
            Assert.IsNotNull(setBlocked, "D16InputRouter 缺少 SetGameplayInputBlocked。");

            Keyboard keyboard = null;
            try
            {
                setBlocked.Invoke(null, new object[] { false });
                setInputEnabled.Invoke(character, new object[] { true });
                setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, 30f) });
                Physics.SyncTransforms();

                var deadline = Time.realtimeSinceStartup + 2f;
                while (!(bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)isSwimming.GetValue(character), "深水起点没有进入游泳态。");

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Vector3 start = player.transform.position;
                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                for (var frame = 0; frame < 18; frame++)
                    yield return null;
                Release(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                yield return null;

                Vector3 end = player.transform.position;
                var planarDistance = Vector2.Distance(new Vector2(start.x, start.z),
                    new Vector2(end.x, end.z));
                Debug.Log("[Q26/A01] keyboard=W, swimming=" + isSwimming.GetValue(character) +
                          ", action=" + action.GetValue(animation) +
                          ", planarDistance=" + planarDistance.ToString("F4") +
                          ", start=" + start + ", end=" + end +
                          ", surfaceY=" + ((float)waterSurfaceY.GetValue(character)).ToString("F3"));

                Assert.IsTrue((bool)isSwimming.GetValue(character),
                    "真实 W 输入后玩家不再处于游泳态。");
                Assert.AreEqual("Swimming", action.GetValue(animation).ToString(),
                    "真实 W 输入后动作层没有保持 Swimming。");
                Assert.Greater(planarDistance, 0.01f,
                    "真实 W 输入没有在水中产生水平位移；不能把深水游泳宣布为可用。");
            }
            finally
            {
                setBlocked.Invoke(null, new object[] { false });
                setInputEnabled.Invoke(character, new object[] { false });
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.wKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
            }
        }

        [UnityTest]
        public IEnumerator Q26A06SwimmingRemainsContinuousWithWaterMotion()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var applyPhysics = characterType.GetMethod("ApplyPhysicsMotion",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var isSwimming = characterType.GetProperty("IsSwimming");
            var moveDirection = characterType.GetField("_moveDirection",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(applyPhysics, "D2CharacterController 缺少 ApplyPhysicsMotion。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(moveDirection, "D2CharacterController 缺少移动方向字段。");

            try
            {
                setInputEnabled.Invoke(character, new object[] { true });
                // Start in the same bounded water volume used by the existing
                // Q26 keyboard test, then hold a real movement direction long
                // enough to expose a collision-induced stop. The native R03
                // route supplies the separate authored-shoreline evidence.
                setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, 30f) });
                var swimDirection = Vector3.forward;
                moveDirection.SetValue(character, swimDirection);
                Physics.SyncTransforms();

                var deadline = Time.realtimeSinceStartup + 2f;
                while (!(bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)isSwimming.GetValue(character),
                    "深水窗口没有进入游泳态。");

                Vector3 start = player.transform.position;
                for (var frame = 0; frame < 60; frame++)
                {
                    moveDirection.SetValue(character, swimDirection);
                    applyPhysics.Invoke(character, null);
                    yield return null;
                }

                Vector3 end = player.transform.position;
                float displacement = Vector2.Distance(
                    new Vector2(start.x, start.z), new Vector2(end.x, end.z));
                Debug.Log("[Q26/A06] water_swim_continuous=" + isSwimming.GetValue(character) +
                          ", displacement=" + displacement.ToString("F4") +
                          ", start=" + start + ", end=" + end);
                Assert.IsTrue((bool)isSwimming.GetValue(character),
                    "深水游泳在持续输入期间被错误切回陆地，表现为海水无法游出。");
                Assert.Greater(displacement, 0.25f,
                    "深水游泳持续输入没有产生足够水平位移，角色可能仍被碰撞体卡住。");
            }
            finally
            {
                moveDirection.SetValue(character, Vector3.zero);
                setInputEnabled.Invoke(character, new object[] { false });
            }
        }

        [UnityTest]
        public IEnumerator Q26A22SwimmingExitBlocksImmediateReentryUntilBoundarySettles()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var applyPhysics = characterType.GetMethod("ApplyPhysicsMotion",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var endSwimming = characterType.GetMethod("EndSwimming",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var isSwimming = characterType.GetProperty("IsSwimming");
            var moveDirection = characterType.GetField("_moveDirection",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(applyPhysics, "D2CharacterController 缺少 ApplyPhysicsMotion。");
            Assert.IsNotNull(endSwimming, "D2CharacterController 缺少 EndSwimming。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(moveDirection, "D2CharacterController 缺少移动方向字段。");

            float previousTimeScale = Time.timeScale;
            try
            {
                // The D2 re-entry hysteresis is intentionally measured with
                // Time.time. Keep this diagnostic independent from a modal
                // state left by a preceding PlayMode case, then wait on the
                // same clock as the production state machine.
                Time.timeScale = 1f;
                setInputEnabled.Invoke(character, new object[] { true });
                setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, 30f) });
                Physics.SyncTransforms();

                var deadline = Time.realtimeSinceStartup + 2f;
                while (!(bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)isSwimming.GetValue(character), "深水起点没有进入游泳态。");

                Debug.Log("[Q26/A22] before_exit time=" + Time.time.ToString("F3") +
                          ", realtime=" + Time.realtimeSinceStartup.ToString("F3") +
                          ", timeScale=" + Time.timeScale.ToString("F3"));
                endSwimming.Invoke(character, new object[] { "Q26/A22 boundary regression" });
                moveDirection.SetValue(character, Vector3.forward);
                applyPhysics.Invoke(character, null);
                bool immediateReentry = (bool)isSwimming.GetValue(character);
                Assert.IsFalse(immediateReentry,
                    "游泳退出后的同一边界判定立即重新进入游泳态，存在岸线状态抖动。");

                float cooldownObservationDeadline = Time.time + 0.45f;
                while (Time.time < cooldownObservationDeadline)
                    yield return null;
                Debug.Log("[Q26/A22] after_wait time=" + Time.time.ToString("F3") +
                          ", realtime=" + Time.realtimeSinceStartup.ToString("F3") +
                          ", timeScale=" + Time.timeScale.ToString("F3") +
                          ", deadline=" + cooldownObservationDeadline.ToString("F3"));
                moveDirection.SetValue(character, Vector3.forward);
                applyPhysics.Invoke(character, null);
                bool reenteredAfterCooldown = (bool)isSwimming.GetValue(character);
                Debug.Log("[Q26/A22] immediate_reentry=" + immediateReentry +
                          ", reentered_after_cooldown=" + reenteredAfterCooldown +
                          ", position=" + player.transform.position);
                Assert.IsTrue(reenteredAfterCooldown,
                    "短暂边界迟滞结束后，深水位置没有恢复正常游泳进入。");
            }
            finally
            {
                moveDirection.SetValue(character, Vector3.zero);
                setInputEnabled.Invoke(character, new object[] { false });
                Time.timeScale = previousTimeScale;
            }
        }

        [UnityTest]
        public IEnumerator Q26A02BoardingPlayerStaysNearVisibleHelmAndOwnsShipControl()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var deckType = FindLoadedType("Whalebone.Gameplay.D7.D7ShipDeckController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            Assert.IsNotNull(deckType, "D7ShipDeckController 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            var shipVisual = GameObject.Find("D19_R03_ShipVisualInstance");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(ship, "InitialHarbor gameplay ship 没有创建。");
            Assert.IsNotNull(shipVisual, "InitialHarbor 可见船实例没有创建。");

            var router = UnityEngine.Object.FindFirstObjectByType(routerType,
                FindObjectsInactive.Include) as Component;
            var deck = ship.GetComponent(deckType);
            Assert.IsNotNull(router, "D2ControlRouter 没有创建。");
            Assert.IsNotNull(deck, "稳定船对象缺少 D7ShipDeckController。");

            var tryToggleShip = routerType.GetMethod("TryToggleShip");
            var mode = routerType.GetProperty("Mode");
            var helm = deckType.GetProperty("HelmPoint");
            var boarded = deckType.GetProperty("PlayerBoarded");
            Assert.IsNotNull(tryToggleShip, "D2ControlRouter 缺少 TryToggleShip。");
            Assert.IsNotNull(mode, "D2ControlRouter 缺少 Mode。");
            Assert.IsNotNull(helm, "D7ShipDeckController 缺少 HelmPoint。");
            Assert.IsNotNull(boarded, "D7ShipDeckController 缺少 PlayerBoarded。");

            for (var frame = 0; frame < 2; frame++) yield return null;
            var helmTransform = helm.GetValue(deck) as Transform;
            Assert.IsNotNull(helmTransform, "HelmPoint 没有生成。");
            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "初始船旁无法登船。");
            yield return null;

            var visibleHelm = FindNamedTransform(shipVisual.transform,
                "D7_VisibleHelmAnchor", "HelmWheel", "HelmStand");
            var anchorDistance = Vector3.Distance(player.transform.position, helmTransform.position);
            var visibleDistance = visibleHelm == null
                ? -1f
                : Vector3.Distance(player.transform.position, visibleHelm.position);
            Debug.Log("[Q26/A02] mode=" + mode.GetValue(router) +
                      ", boarded=" + boarded.GetValue(deck) +
                      ", gameplayHelm=" + helmTransform.position +
                      ", player=" + player.transform.position +
                      ", visibleHelm=" + (visibleHelm == null ? "missing" : visibleHelm.name) +
                      ", visibleDistance=" + visibleDistance.ToString("F4"));

            Assert.AreEqual("Ship", mode.GetValue(router).ToString(),
                "登船后控制 owner 没有切换到 Ship。");
            Assert.IsTrue((bool)boarded.GetValue(deck),
                "登船后 D7 没有记录 PlayerBoarded。");
            Assert.Less(anchorDistance, 0.05f,
                "玩家没有落在稳定 gameplay HelmPoint。");
            Assert.IsNotNull(visibleHelm,
                "可见船实例没有稳定的 D7_VisibleHelmAnchor/HelmWheel/HelmStand，玩家会像在控制空物体。");
            Assert.Less(visibleDistance, 0.75f,
                "玩家虽然挂到了 gameplay HelmPoint，但离可见船舵超过 0.75m，原生画面会像站错位置。");

            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "登船后无法执行下船路径。");
        }

        [UnityTest]
        public IEnumerator Q26A11ShipPresentationForwardMatchesGameplayControlAxis()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var ship = GameObject.Find("ph_ship_fishing");
            var shipVisual = GameObject.Find("D19_R03_ShipVisualInstance");
            Assert.IsNotNull(ship, "InitialHarbor gameplay ship 没有创建。");
            Assert.IsNotNull(shipVisual, "InitialHarbor 可见船实例没有创建。");

            var candidate = FindNamedTransform(shipVisual.transform, "KenneyPirateShipPresentation");
            Assert.IsNotNull(candidate, "可见船实例缺少 KenneyPirateShipPresentation 方向根。");

            var alignment = Vector3.Dot(ship.transform.forward, candidate.forward);
            Debug.Log("[Q26/A11] ship_forward=" + ship.transform.forward +
                      ", visual_forward=" + candidate.forward +
                      ", forward_alignment=" + alignment.ToString("F4") +
                      ", ship_heading=" + ship.transform.eulerAngles.y.ToString("F2"));

            Assert.Greater(alignment, 0.98f,
                "可见船体与 D2 航行控制轴相反；W 会把船驶向港口建筑，产生穿模/吞没画面。");
        }

        [UnityTest]
        public IEnumerator Q26A05ProceduralFallbackReachesVisibleHelmInShipMode()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            var shipVisual = GameObject.Find("D19_R03_ShipVisualInstance");
            var router = UnityEngine.Object.FindFirstObjectByType(routerType,
                FindObjectsInactive.Include) as Component;
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(shipVisual, "InitialHarbor 可见船实例没有创建。");
            Assert.IsNotNull(router, "InitialHarbor D2ControlRouter 没有创建。");

            var tryToggleShip = routerType.GetMethod("TryToggleShip");
            var mode = routerType.GetProperty("Mode");
            var steering = animationType.GetProperty("ShipSteeringPoseActive");
            Assert.IsNotNull(tryToggleShip, "D2ControlRouter 缺少 TryToggleShip。");
            Assert.IsNotNull(mode, "D2ControlRouter 缺少 Mode。");
            Assert.IsNotNull(steering, "D2CharacterAnimationController 缺少 ShipSteeringPoseActive。");

            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "初始船旁无法登船。");
            for (var frame = 0; frame < 4; frame++) yield return null;

            var wheel = FindNamedTransform(shipVisual.transform, "HelmWheel");
            var leftHand = FindNamedTransform(player.transform, "LeftHandMesh");
            var rightHand = FindNamedTransform(player.transform, "RightHandMesh");
            var leftLowerArm = FindNamedTransform(player.transform, "LeftLowerArm");
            var rightLowerArm = FindNamedTransform(player.transform, "RightLowerArm");
            Assert.AreEqual("Ship", mode.GetValue(router).ToString(), "登船后控制权没有进入 Ship。");
            Assert.IsTrue((bool)steering.GetValue(player.GetComponent(animationType)),
                "程序化 fallback 登船后没有进入对称舵轮姿态。");
            Assert.IsNotNull(wheel, "可见船实例缺少 HelmWheel。");
            Assert.IsNotNull(leftHand, "程序化 fallback 缺少 LeftHandMesh。");
            Assert.IsNotNull(rightHand, "程序化 fallback 缺少 RightHandMesh。");
            Assert.IsNotNull(leftLowerArm, "程序化 fallback 缺少 LeftLowerArm。");
            Assert.IsNotNull(rightLowerArm, "程序化 fallback 缺少 RightLowerArm。");

            float leftForearmBend = Mathf.Abs(Mathf.DeltaAngle(0f, leftLowerArm.localEulerAngles.x));
            float rightForearmBend = Mathf.Abs(Mathf.DeltaAngle(0f, rightLowerArm.localEulerAngles.x));
            Assert.Greater(leftForearmBend, 5f, "左前臂没有进入舵轮控制的可读弯曲姿态。");
            Assert.Greater(rightForearmBend, 5f, "右前臂没有进入舵轮控制的可读弯曲姿态。");

            float leftDistance = Vector3.Distance(leftHand.position, wheel.position);
            float rightDistance = Vector3.Distance(rightHand.position, wheel.position);
            Debug.Log("[Q26/A05] shipSteering=true, leftHandWheelDistance=" +
                      leftDistance.ToString("F4") + ", rightHandWheelDistance=" +
                      rightDistance.ToString("F4") + ", wheel=" + wheel.position +
                      ", leftHand=" + leftHand.position + ", rightHand=" + rightHand.position);
            Assert.Less(leftDistance, 0.30f, "左手离可见舵轮过远，画面仍像控制无关物体。");
            Assert.Less(rightDistance, 0.30f, "右手离可见舵轮过远，画面仍像控制无关物体。");

            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "舵轮姿态测试结束时无法执行下船路径。");
        }

        [UnityTest]
        public IEnumerator Q26A17GeneratedHelmWheelSharesFallbackHandHeight()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            var shipVisual = GameObject.Find("D19_R03_ShipVisualInstance");
            var router = UnityEngine.Object.FindFirstObjectByType(routerType,
                FindObjectsInactive.Include) as Component;
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(shipVisual, "InitialHarbor 可见船实例没有创建。");
            Assert.IsNotNull(router, "InitialHarbor D2ControlRouter 没有创建。");

            var tryToggleShip = routerType.GetMethod("TryToggleShip");
            Assert.IsNotNull(tryToggleShip, "D2ControlRouter 缺少 TryToggleShip。");
            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "初始船旁无法登船。");
            for (var frame = 0; frame < 4; frame++) yield return null;

            var wheel = FindNamedTransform(shipVisual.transform, "HelmWheel");
            var leftHand = FindNamedTransform(player.transform, "LeftHandMesh");
            var rightHand = FindNamedTransform(player.transform, "RightHandMesh");
            Assert.IsNotNull(wheel, "可见船实例缺少 HelmWheel。");
            Assert.IsNotNull(leftHand, "程序化 fallback 缺少 LeftHandMesh。");
            Assert.IsNotNull(rightHand, "程序化 fallback 缺少 RightHandMesh。");

            float handMidpointY = (leftHand.position.y + rightHand.position.y) * 0.5f;
            float verticalError = Mathf.Abs(wheel.position.y - handMidpointY);
            Debug.Log("[Q26/A17] generatedHelmWheelY=" + wheel.position.y.ToString("F4") +
                      ", handMidpointY=" + handMidpointY.ToString("F4") +
                      ", verticalError=" + verticalError.ToString("F4"));
            Assert.Less(verticalError, 0.06f,
                "生成舵轮中心仍明显低于 fallback 双手，画面会读作人物站在控制物体上方。");

            Assert.IsTrue((bool)tryToggleShip.Invoke(router, null), "舵轮高度测试结束时无法执行下船路径。");
        }

        [UnityTest]
        public IEnumerator Q26A18GeneratedHelmWheelUsesWarmWoodContrast()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var shipVisual = GameObject.Find("D19_R03_ShipVisualInstance");
            Assert.IsNotNull(shipVisual, "InitialHarbor 可见船实例没有创建。");
            var wheel = FindNamedTransform(shipVisual.transform, "HelmWheel");
            Assert.IsNotNull(wheel, "可见船实例缺少 HelmWheel。");
            var renderer = wheel.GetComponent<Renderer>();
            Assert.IsNotNull(renderer, "HelmWheel 缺少 Renderer。");
            Assert.IsNotNull(renderer.sharedMaterial, "HelmWheel 缺少运行时材质。");

            Color color = renderer.sharedMaterial.HasProperty("_BaseColor")
                ? renderer.sharedMaterial.GetColor("_BaseColor")
                : renderer.sharedMaterial.GetColor("_Color");
            Debug.Log("[Q26/A18] helmMaterial=" + renderer.sharedMaterial.name +
                      ", color=" + color);
            Assert.Greater(color.r, color.b * 2f,
                "生成舵轮必须保持暖木色对比，不能退化为近黑色遮挡环。");
            Assert.Greater(color.g, color.b * 1.5f,
                "生成舵轮必须保留可读的木色绿色通道，避免读作黑色容器。");
        }

        [UnityTest]
        public IEnumerator Q26A19GeneratedHelmSupportUsesShipWoodMaterial()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var shipVisual = GameObject.Find("D19_R03_ShipVisualInstance");
            Assert.IsNotNull(shipVisual, "InitialHarbor 可见船实例没有创建。");
            var stand = FindNamedTransform(shipVisual.transform, "HelmStand");
            Assert.IsNotNull(stand, "可见船实例缺少 HelmStand。");
            var renderer = stand.GetComponent<Renderer>();
            Assert.IsNotNull(renderer, "HelmStand 缺少 Renderer。");
            Assert.IsNotNull(renderer.sharedMaterial, "HelmStand 缺少运行时材质。");

            // D7 material polish runs asynchronously after scene readiness. Wait
            // through that owner as well, otherwise the test could only prove
            // the prefab source while the Player later restores a stone-like
            // material on the visible support.
            for (var frame = 0; frame < 90; frame++) yield return null;

            renderer = stand.GetComponent<Renderer>();
            var materialName = renderer.sharedMaterial == null ? string.Empty : renderer.sharedMaterial.name;
            var textureName = renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null
                ? string.Empty
                : renderer.sharedMaterial.mainTexture.name;
            bool readsAsWood = materialName.IndexOf("R03_Wood", StringComparison.OrdinalIgnoreCase) >= 0 ||
                textureName.IndexOf("wood", StringComparison.OrdinalIgnoreCase) >= 0 ||
                textureName.IndexOf("plank", StringComparison.OrdinalIgnoreCase) >= 0;
            Debug.Log("[Q26/A19] helmSupportMaterial=" + materialName +
                      ", texture=" + textureName + ", readsAsWood=" + readsAsWood);
            Assert.IsTrue(readsAsWood,
                "舵轮支撑必须在 prefab 与 D7 polish 后都读作船体木材；石柱会让玩家把舵轮读成孤立的罐/瓢状物。");
        }

        [UnityTest]
        public IEnumerator Q26A21ProceduralSwimPoseHasMeasuredContinuousStroke()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            var animation = player.GetComponent(animationType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var isSwimming = characterType.GetProperty("IsSwimming");
            var action = animationType.GetProperty("Action");
            var proceduralActive = animationType.GetProperty("ProceduralPoseLayerActive");
            var poseDriverMode = animationType.GetProperty("PoseDriverModeName");
            var pitch = animationType.GetProperty("SwimmingVisualPitchDegrees");
            var bodyDrop = animationType.GetProperty("SwimmingVisualBodyDrop");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(action, "动画控制器缺少 Action。");
            Assert.IsNotNull(proceduralActive, "动画控制器缺少程序化姿态层诊断。");
            Assert.IsNotNull(poseDriverMode, "动画控制器缺少姿态驱动源诊断。");
            Assert.IsNotNull(pitch, "动画控制器缺少游泳 pitch 诊断。");
            Assert.IsNotNull(bodyDrop, "动画控制器缺少游泳 body-drop 诊断。");

            try
            {
                setInputEnabled.Invoke(character, new object[] { false });
                setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, 30f) });
                Physics.SyncTransforms();

                var deadline = Time.realtimeSinceStartup + 2f;
                while (!(bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;

                Assert.IsTrue((bool)isSwimming.GetValue(character), "开放水域测试点没有进入游泳态。");
                yield return null;
                Assert.AreEqual("Swimming", action.GetValue(animation).ToString(),
                    "水中状态没有被表现层消费为 Swimming。");
                Assert.IsTrue((bool)proceduralActive.GetValue(animation),
                    "当前默认低模 Player 必须由程序化姿态层独占，不能退回未知 Animator。");
                Assert.AreEqual("Procedural", poseDriverMode.GetValue(animation).ToString(),
                    "当前默认低模 Player 的姿态驱动源不应漂移到未验证 Animator。");

                var visualRoot = FindNamedTransform(player.transform, "HumanoidVisual");
                var leftUpperArm = FindNamedTransform(player.transform, "LeftUpperArm");
                var rightUpperArm = FindNamedTransform(player.transform, "RightUpperArm");
                var leftFoot = FindNamedTransform(player.transform, "LeftFoot");
                var rightFoot = FindNamedTransform(player.transform, "RightFoot");
                Assert.IsNotNull(visualRoot, "程序化 Player 缺少 HumanoidVisual。");
                Assert.IsNotNull(leftUpperArm, "程序化 Player 缺少 LeftUpperArm。");
                Assert.IsNotNull(rightUpperArm, "程序化 Player 缺少 RightUpperArm。");
                Assert.IsNotNull(leftFoot, "程序化 Player 缺少 LeftFoot。");
                Assert.IsNotNull(rightFoot, "程序化 Player 缺少 RightFoot。");

                float pitchDegrees = (float)pitch.GetValue(animation);
                float bodyDropMeters = (float)bodyDrop.GetValue(animation);
                Assert.That(pitchDegrees, Is.InRange(45f, 75f),
                    "程序化泳姿必须保持有界的水平倾角，不能回到直立或塌成翻转姿态。");
                Assert.That(bodyDropMeters, Is.InRange(-0.80f, -0.45f),
                    "程序化泳姿的视觉根高度必须留在水面附近，不能把角色抬到空中或埋入海面。");

                Quaternion previousLeftArm = leftUpperArm.localRotation;
                Quaternion previousRightArm = rightUpperArm.localRotation;
                Quaternion previousLeftFoot = leftFoot.localRotation;
                Quaternion previousRightFoot = rightFoot.localRotation;
                float maxArmDelta = 0f;
                float maxFootDelta = 0f;
                for (var frame = 0; frame < 72; frame++)
                {
                    yield return null;
                    maxArmDelta = Mathf.Max(maxArmDelta,
                        Quaternion.Angle(previousLeftArm, leftUpperArm.localRotation),
                        Quaternion.Angle(previousRightArm, rightUpperArm.localRotation));
                    maxFootDelta = Mathf.Max(maxFootDelta,
                        Quaternion.Angle(previousLeftFoot, leftFoot.localRotation),
                        Quaternion.Angle(previousRightFoot, rightFoot.localRotation));
                    previousLeftArm = leftUpperArm.localRotation;
                    previousRightArm = rightUpperArm.localRotation;
                    previousLeftFoot = leftFoot.localRotation;
                    previousRightFoot = rightFoot.localRotation;
                }

                Debug.Log("[Q26/A21] mode=" + poseDriverMode.GetValue(animation) +
                          ", pitch=" + pitchDegrees.ToString("F1") +
                          ", bodyDrop=" + bodyDropMeters.ToString("F3") +
                          ", maxArmDelta=" + maxArmDelta.ToString("F2") +
                          ", maxFootDelta=" + maxFootDelta.ToString("F2") +
                          ", formalAssetBoundary=unverified");
                Assert.Greater(maxArmDelta, 2f,
                    "游泳表现层连续 72 帧没有可测量的上肢相位变化。");
                Assert.Greater(maxFootDelta, 1f,
                    "游泳表现层连续 72 帧没有可测量的脚部相位变化。");
            }
            finally
            {
                setInputEnabled.Invoke(character, new object[] { false });
            }
        }

        [UnityTest]
        public IEnumerator Q26A23ProceduralSwimHandsStayBelowHeadSilhouette()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var character = player.GetComponent(characterType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var isSwimming = characterType.GetProperty("IsSwimming");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");

            try
            {
                setInputEnabled.Invoke(character, new object[] { false });
                setWorldPosition.Invoke(character, new object[] { new Vector3(0f, 0.42f, 30f) });
                Physics.SyncTransforms();

                var deadline = Time.realtimeSinceStartup + 2f;
                while (!(bool)isSwimming.GetValue(character) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)isSwimming.GetValue(character), "开放水域测试点没有进入游泳态。");

                var head = FindNamedTransform(player.transform, "Head");
                var leftHand = FindNamedTransform(player.transform, "LeftHandMesh");
                var rightHand = FindNamedTransform(player.transform, "RightHandMesh");
                Assert.IsNotNull(head, "程序化 fallback 缺少 Head。");
                Assert.IsNotNull(leftHand, "程序化 fallback 缺少 LeftHandMesh。");
                Assert.IsNotNull(rightHand, "程序化 fallback 缺少 RightHandMesh。");

                float maxHandAboveHead = float.MinValue;
                for (var frame = 0; frame < 36; frame++)
                {
                    yield return null;
                    maxHandAboveHead = Mathf.Max(maxHandAboveHead,
                        leftHand.position.y - head.position.y,
                        rightHand.position.y - head.position.y);
                }

                Debug.Log("[Q26/A23] maxHandAboveHead=" + maxHandAboveHead.ToString("F3") +
                          ", formalAssetBoundary=unverified");
                Assert.LessOrEqual(maxHandAboveHead, 0.22f,
                    "程序化游泳手部越过头部轮廓，画面会读作动作扭曲；正式动画资产仍需单独验收。");
            }
            finally
            {
                setInputEnabled.Invoke(character, new object[] { false });
            }
        }

        [UnityTest]
        public IEnumerator Q26A04RealKeyboardERejectsRemoteBoardsAndCompletesBoardDisembark()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(ship, "InitialHarbor gameplay ship 没有创建。");

            var character = player.GetComponent(characterType);
            var router = UnityEngine.Object.FindFirstObjectByType(routerType,
                FindObjectsInactive.Include) as Component;
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(router, "D2ControlRouter 没有创建。");

            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var mode = routerType.GetProperty("Mode");
            var feedback = routerType.GetProperty("LastInteractionFeedback");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(mode, "D2ControlRouter 缺少 Mode。");
            Assert.IsNotNull(feedback, "D2ControlRouter 缺少 LastInteractionFeedback。");

            Keyboard keyboard = null;
            try
            {
                setInputEnabled.Invoke(character, new object[] { false });
                setWorldPosition.Invoke(character, new object[]
                {
                    ship.transform.position + new Vector3(40f, 0.42f, 40f)
                });
                Physics.SyncTransforms();
                for (var frame = 0; frame < 3; frame++) yield return null;

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                setInputEnabled.Invoke(character, new object[] { true });

                Press(keyboard.eKey, queueEventOnly: true);
                yield return null;
                Release(keyboard.eKey, queueEventOnly: true);
                InputSystem.Update();
                for (var frame = 0; frame < 2; frame++) yield return null;

                var remoteFeedback = feedback.GetValue(router);
                Assert.AreEqual("Character", mode.GetValue(router).ToString(),
                    "远离船只按 E 不应切换到 Ship。");
                Assert.AreEqual("ship_too_far",
                    remoteFeedback.GetType().GetProperty("Code").GetValue(remoteFeedback),
                    "远离船只按 E 必须留下 ship_too_far 拒绝反馈。");

                setInputEnabled.Invoke(character, new object[] { false });
                setWorldPosition.Invoke(character, new object[]
                {
                    ship.transform.position + ship.transform.forward * 2f + Vector3.up * 0.42f
                });
                Physics.SyncTransforms();
                for (var frame = 0; frame < 3; frame++) yield return null;
                setInputEnabled.Invoke(character, new object[] { true });

                Press(keyboard.eKey, queueEventOnly: true);
                yield return null;
                Release(keyboard.eKey, queueEventOnly: true);
                InputSystem.Update();
                for (var frame = 0; frame < 3; frame++) yield return null;

                var boardedFeedback = feedback.GetValue(router);
                Assert.AreEqual("Ship", mode.GetValue(router).ToString(),
                    "靠近船只按真实 E 没有切换到 Ship。");
                Assert.AreEqual("boarded",
                    boardedFeedback.GetType().GetProperty("Code").GetValue(boardedFeedback),
                    "真实 E 登船没有留下 boarded 反馈。");
                Assert.AreEqual(ship.transform, player.transform.parent,
                    "真实 E 登船后玩家没有挂到稳定船根。");

                Press(keyboard.eKey, queueEventOnly: true);
                yield return null;
                Release(keyboard.eKey, queueEventOnly: true);
                InputSystem.Update();
                for (var frame = 0; frame < 5; frame++) yield return null;

                var disembarkedFeedback = feedback.GetValue(router);
                Assert.AreEqual("Character", mode.GetValue(router).ToString(),
                    "真实 E 下船没有恢复 Character。");
                Assert.IsNull(player.transform.parent, "真实 E 下船后玩家仍挂在船根。");
                Assert.AreEqual("disembarked",
                    disembarkedFeedback.GetType().GetProperty("Code").GetValue(disembarkedFeedback),
                    "真实 E 下船没有留下 disembarked 反馈。");

                Debug.Log("[Q26/A04] remote_e_rejected=true, real_e_boarded=true, " +
                          "real_e_disembarked=true, final_mode=" + mode.GetValue(router));
            }
            finally
            {
                setInputEnabled.Invoke(character, new object[] { true });
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.eKey, queueEventOnly: true);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
            }
        }

        [UnityTest]
        public IEnumerator Q26A03ActiveLegacyTreeRenderersAndR03VegetationHaveGroundEvidence()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var legacyTreePaths = new List<string>();
            var activeVegetation = new List<string>();
            var unsupportedVegetation = new List<string>();
            var preferred = GameObject.Find("D19_R03_HarborPresentation");
            Assert.IsNotNull(preferred, "InitialHarbor 缺少 R03 港口表现根，无法审计植被接地。");
            // 场景中存在多个历史地形根时，审计必须与 D26 表现 owner 使用同一
            // Prefab 内的 Collider，避免把不同地形的 raycast 结果混在一起。
            var terrainTransform = FindNamedTransform(preferred.transform, "D19_TerrainRoot");
            var terrainCollider = terrainTransform == null ? null : terrainTransform.GetComponent<Collider>();
            Assert.IsNotNull(terrainCollider, "InitialHarbor 缺少 R03 D19_TerrainRoot collider，无法审计植被接地。");
            Assert.IsTrue(terrainCollider.enabled, "R03 D19_TerrainRoot collider 必须启用才能作为接地证据。");
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var renderers = roots[i].GetComponentsInChildren<Renderer>(true);
                for (var j = 0; j < renderers.Length; j++)
                {
                    var renderer = renderers[j];
                    if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                        continue;

                    string path = GetTransformPath(renderer.transform);
                    string normalized = path.ToLowerInvariant();
                    if (normalized.Contains("ph_palm_tree") || normalized.Contains("ph_trunk") ||
                        normalized.Contains("ph_leaf"))
                        legacyTreePaths.Add(path);

                    if (!normalized.Contains("d19_r03_vegetationandrocks")) continue;
                    if (!normalized.Contains("shrub") && !normalized.Contains("rock")) continue;
                    activeVegetation.Add(path);

                    Vector3 origin = renderer.bounds.center + Vector3.up *
                        Mathf.Max(0.5f, renderer.bounds.size.y + 0.5f);
                    RaycastHit hit;
                    bool hasGround = terrainCollider.Raycast(new Ray(origin, Vector3.down), out hit, 32f);
                    if (!hasGround || Mathf.Abs(renderer.bounds.min.y - hit.point.y) > 0.65f)
                        unsupportedVegetation.Add(path +
                            " boundsMinY=" + renderer.bounds.min.y.ToString("F3") +
                            " ground=" + (hasGround ? hit.point.y.ToString("F3") : "missing"));
                }
            }

            Debug.Log("[Q26/A03] activeLegacyTreeRenderers=" + legacyTreePaths.Count +
                      ", activeR03Vegetation=" + activeVegetation.Count +
                      ", unsupportedR03Vegetation=" + unsupportedVegetation.Count +
                      ", legacyPaths=" + string.Join(" | ", legacyTreePaths.ToArray()) +
                      ", unsupported=" + string.Join(" | ", unsupportedVegetation.ToArray()));

            Assert.IsEmpty(legacyTreePaths,
                "legacy D19 palm/tree Renderer 仍在活动；它们可能产生空中树或旧视觉叠加。");
            Assert.IsEmpty(unsupportedVegetation,
                "R03 植被/岩石 Renderer 没有可证明的地面接触。");
        }

        [UnityTest]
        public IEnumerator Q26A16HarborCollisionUsesScopedR03TerrainOwner()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var collisionType = FindLoadedType("Whalebone.Gameplay.Harbor.HarborCollisionController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(collisionType, "HarborCollisionController 未加载。");

            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var r03 = GameObject.Find("D19_R03_HarborPresentation");
            Assert.IsNotNull(r03, "InitialHarbor 缺少 R03 表现根。");

            var terrainTransform = FindNamedTransform(r03.transform, "D19_TerrainRoot");
            var terrainCollider = terrainTransform == null
                ? null
                : terrainTransform.GetComponent<MeshCollider>();
            var visualTransform = FindNamedTransform(r03.transform, "D19_R03_IslandSurface");
            var visualFilter = visualTransform == null
                ? null
                : visualTransform.GetComponent<MeshFilter>();
            var controller = UnityEngine.Object.FindFirstObjectByType(collisionType,
                FindObjectsInactive.Include) as Component;
            var installed = collisionType.GetProperty("R03SurfaceInstalled");
            Assert.IsNotNull(controller, "InitialHarbor 没有创建 HarborCollisionController owner。");
            Assert.IsNotNull(installed, "HarborCollisionController 缺少 R03SurfaceInstalled 状态。");
            Assert.IsNotNull(terrainTransform, "R03 表现根缺少 scoped D19_TerrainRoot。");
            Assert.IsNotNull(terrainCollider, "R03 D19_TerrainRoot 必须由 MeshCollider 持有 authored terrain。");
            Assert.IsTrue(terrainCollider.enabled && !terrainCollider.isTrigger,
                "R03 D19_TerrainRoot MeshCollider 必须启用且不可为 Trigger。");
            Assert.IsNotNull(terrainCollider.sharedMesh, "R03 D19_TerrainRoot MeshCollider 缺少 authored sharedMesh。");
            Assert.IsTrue((bool)installed.GetValue(controller),
                "HarborCollisionController 没有安装 scoped R03 terrain owner。");
            Assert.AreEqual("D19_R03_IslandSurfaceMesh", terrainCollider.sharedMesh.name,
                "运行时碰撞 owner 不应被静态合批后的视觉 Combined Mesh 替换。");

            var globalNamedTerrain = GameObject.Find("D19_TerrainRoot");
            var globalCollider = globalNamedTerrain == null
                ? null
                : globalNamedTerrain.GetComponent<Collider>();
            string globalPath = globalNamedTerrain == null ? "<none>" : GetTransformPath(globalNamedTerrain.transform);
            string visualMesh = visualFilter == null || visualFilter.sharedMesh == null
                ? "<none>"
                : visualFilter.sharedMesh.name;
            Debug.Log("[Q26/A16] scopedTerrain=" + GetTransformPath(terrainTransform) +
                      ", authoredMesh=" + terrainCollider.sharedMesh.name +
                      ", visualMesh=" + visualMesh +
                      ", controllerInstalled=" + installed.GetValue(controller) +
                      ", ambiguousGlobalTerrain=" + globalPath +
                      ", globalCollider=" + (globalCollider == null ? "<none>" : globalCollider.GetType().Name) +
                      ", globalEnabled=" + (globalCollider != null && globalCollider.enabled));

            Assert.IsTrue(terrainTransform == r03.transform || terrainTransform.IsChildOf(r03.transform),
                "R03 terrain owner must remain scoped to the R03 presentation hierarchy。");
        }

        [UnityTest]
        public IEnumerator Q26A10CrewAnchorStaysInteractableWithoutOverlappingPlayerSpawn()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            var crew = GameObject.Find("ph_crew_male");
            var spawnAnchor = GameObject.Find("D2_PlayerSpawnAnchor");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(crew, "InitialHarbor 缺少 ph_crew_male 船员目标。");
            Assert.IsNotNull(spawnAnchor, "InitialHarbor 缺少 D2_PlayerSpawnAnchor。");

            var playerToCrew = Vector2.Distance(
                new Vector2(player.transform.position.x, player.transform.position.z),
                new Vector2(crew.transform.position.x, crew.transform.position.z));
            var spawnToCrew = Vector2.Distance(
                new Vector2(spawnAnchor.transform.position.x, spawnAnchor.transform.position.z),
                new Vector2(crew.transform.position.x, crew.transform.position.z));

            Debug.Log("[Q26/A10] crew_anchor_separation=" + playerToCrew.ToString("F4") +
                      ", spawn_to_crew=" + spawnToCrew.ToString("F4") +
                      ", player=" + player.transform.position +
                      ", spawn=" + spawnAnchor.transform.position +
                      ", crew=" + crew.transform.position);

            // Keep the NPC outside the player's visual/collision silhouette,
            // while preserving the existing 3.5m D2 dialogue interaction range.
            Assert.GreaterOrEqual(playerToCrew, 1.25f,
                "船员不能与玩家出生点重叠，否则登船镜头会出现第二个人物叠在玩家身上。");
            Assert.LessOrEqual(playerToCrew, 3.5f,
                "船员移开后必须仍在 D2 的 3.5m 对话范围内。");
            Assert.GreaterOrEqual(spawnToCrew, 1.25f,
                "船员不能占用 D2_PlayerSpawnAnchor 的同一视觉位置。");
        }

        [UnityTest]
        public IEnumerator Q26A07R03OwnsCoastalFoamAndLegacyD6FoamIsHidden()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var r03 = GameObject.Find("D19_R03_HarborPresentation");
            Assert.IsNotNull(r03, "InitialHarbor 缺少 R03 港口表现根。");

            var r03Foam = new List<string>();
            foreach (var renderer in r03.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer != null && renderer.enabled &&
                    renderer.transform.name.StartsWith("FoamRing", StringComparison.Ordinal))
                    r03Foam.Add(GetTransformPath(renderer.transform));
            }

            var legacyRoot = GameObject.Find("D6_HarborFoam");
            var legacyEnabled = new List<string>();
            if (legacyRoot != null)
            {
                foreach (var renderer in legacyRoot.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                        legacyEnabled.Add(GetTransformPath(renderer.transform));
                }
            }

            Debug.Log("[Q26/A07] r03FoamRenderers=" + r03Foam.Count +
                      ", legacyD6FoamRenderers=" + legacyEnabled.Count +
                      ", legacyPaths=" + string.Join(" | ", legacyEnabled.ToArray()));
            Assert.IsNotEmpty(r03Foam,
                "R03 必须保留自己的岸线泡沫 Renderer，不能用关闭旧层代替视觉表现。");
            Assert.IsEmpty(legacyEnabled,
                "D6_HarborFoam 与 R03 岸线泡沫重复可见，可能造成前景大块几何穿插。");
        }

        [UnityTest]
        public IEnumerator Q26A08MarketEntryDoesNotOverlapInitialPlayerCamera()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var player = GameObject.Find("D2_PlayerAvatar");
            var entry = GameObject.Find("D19_R03_MarketEntryAndStreet");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            Assert.IsNotNull(entry, "R03 缺少市场入口表现根。");

            var renderers = entry.GetComponentsInChildren<Renderer>(true);
            Assert.IsNotEmpty(renderers, "市场入口表现根没有 Renderer。");
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            var playerPosition = player.transform.position;
            var planarDistance = Vector2.Distance(
                new Vector2(playerPosition.x, playerPosition.z),
                new Vector2(bounds.center.x, bounds.center.z));
            Debug.Log("[Q26/A08] marketEntryCenter=" + bounds.center +
                      ", player=" + playerPosition +
                      ", planarDistance=" + planarDistance.ToString("F3"));
            Assert.Greater(planarDistance, 2.0f,
                "市场入口横梁/屋顶仍贴在初始玩家镜头前，表现层会遮挡人物并形成前景穿插。");
        }

        [UnityTest]
        public IEnumerator Q26A09PirateTowerHasAuthoredTerrainSupport()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var tower = GameObject.Find("KenneyPirateTower");
            Assert.IsNotNull(tower, "R03 海盗塔候选缺失，无法完成接地审计。");
            var renderers = tower.GetComponentsInChildren<Renderer>(true);
            Assert.IsNotEmpty(renderers, "海盗塔候选没有可见 Renderer。");

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            var origin = new Vector3(bounds.center.x, bounds.max.y + 1.0f, bounds.center.z);
            var hits = Physics.RaycastAll(origin, Vector3.down, 20f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            var grounded = false;
            RaycastHit hit = default(RaycastHit);
            var colliderPath = "";
            for (var i = 0; i < hits.Length; i++)
            {
                var candidatePath = GetTransformPath(hits[i].collider.transform);
                if (!candidatePath.Contains("D19_R03_HarborPresentation") ||
                    !candidatePath.Contains("D19_TerrainRoot")) continue;
                grounded = true;
                hit = hits[i];
                colliderPath = candidatePath;
                break;
            }
            Debug.Log("[Q26/A09] towerCenter=" + bounds.center +
                      ", towerMinY=" + bounds.min.y.ToString("F3") +
                      ", groundHit=" + grounded +
                      ", hitPoint=" + (grounded ? hit.point.ToString() : "<none>") +
                      ", collider=" + colliderPath);
            Assert.IsTrue(grounded &&
                          colliderPath.Contains("D19_R03_HarborPresentation") &&
                          colliderPath.Contains("D19_TerrainRoot"),
                "KenneyPirateTower 必须落在 R03 自有 D19_TerrainRoot 上，不能以无支撑候选物进入 Player。");
        }

        [UnityTest]
        public IEnumerator Q26A12R03PresentationPropsUseOwnedTerrainContact()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var r03 = GameObject.Find("D19_R03_HarborPresentation");
            Assert.IsNotNull(r03, "InitialHarbor 缺少 R03 港口表现根。");
            var terrainTransform = FindNamedTransform(r03.transform, "D19_TerrainRoot");
            var terrain = terrainTransform == null ? null : terrainTransform.GetComponent<Collider>();
            Assert.IsNotNull(terrain, "R03 缺少自有 D19_TerrainRoot，无法验证表现接地。");
            Assert.IsTrue(terrain.enabled, "R03 D19_TerrainRoot 必须启用才能作为接地证据。");

            var targets = new List<string>();
            for (var i = 0; i < 7; i++) targets.Add("Barrel_" + i);
            targets.Add("NoticeBoard");
            targets.Add("NoticePost");
            for (var i = 0; i < 4; i++) targets.Add("StoneStep_" + i);
            for (var i = 0; i < 3; i++) targets.Add("MarketStallWall_" + i);
            for (var i = 0; i < 14; i++) targets.Add("RoadEdge_" + i);
            for (var i = 0; i < 4; i++) targets.Add("StreetStone_" + i);
            targets.Add("KenneyPathBendMarket");
            targets.Add("KenneyRockShore");
            targets.Add("KenneyPirateTower");
            targets.Add("KenneyPirateDock");
            targets.Add("KenneyPirateBoat");
            targets.Add("KenneyPirateCannon");
            targets.Add("KenneyPirateCrates");
            targets.Add("KenneyPirateFlag");

            var failures = new List<string>();
            for (var i = 0; i < targets.Count; i++)
            {
                var target = GameObject.Find(targets[i]);
                if (target == null)
                {
                    failures.Add(targets[i] + " missing");
                    continue;
                }

                var renderers = target.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                {
                    failures.Add(targets[i] + " has no Renderer");
                    continue;
                }

                var bounds = renderers[0].bounds;
                for (var rendererIndex = 1; rendererIndex < renderers.Length; rendererIndex++)
                    bounds.Encapsulate(renderers[rendererIndex].bounds);

                var origin = new Vector3(bounds.center.x, bounds.max.y + 1f, bounds.center.z);
                RaycastHit hit;
                var hasGround = terrain.Raycast(new Ray(origin, Vector3.down), out hit, 32f);
                var error = hasGround ? Mathf.Abs(bounds.min.y - hit.point.y) : float.PositiveInfinity;
                if (!hasGround || error > 0.18f)
                    failures.Add(targets[i] +
                        " ground=" + (hasGround ? hit.point.y.ToString("F3") : "missing") +
                        " minY=" + bounds.min.y.ToString("F3") +
                        " error=" + error.ToString("F3"));
            }

            var unsupportedCliff = GameObject.Find("KenneyCliffBackdrop");
            var unsupportedSteps = GameObject.Find("KenneyCliffStepsNorth");
            Assert.IsNull(unsupportedCliff,
                "没有 R03 地形支撑的 KenneyCliffBackdrop 不应进入正式表现 Prefab。");
            Assert.IsNull(unsupportedSteps,
                "没有 R03 地形支撑的 KenneyCliffStepsNorth 不应进入正式表现 Prefab。");

            Debug.Log("[Q26/A12] grounded_targets=" + targets.Count +
                      ", unsupported_removed=" + (unsupportedCliff == null && unsupportedSteps == null) +
                      ", failures=" + string.Join(" | ", failures.ToArray()));
            Assert.IsEmpty(failures,
                "R03 表现道具必须落在自有 D19_TerrainRoot 附近，不能继续以固定 y 产生悬空/穿地几何。");
        }

        [UnityTest]
        public IEnumerator Q26A13OpenSeaSaveRestoresAuthoredHarborShipRoute()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");

            var originalPlayer = GameState.CurrentPlayer;
            if (GameState.CurrentPlayer == null)
            {
                GameState.CurrentPlayer = PlayerState.CreateNew("q26_a13_route", "Q26/A13");
            }

            var originalLocation = GameState.CurrentPlayer.Location;
            GameState.CurrentPlayer.Location = new LocationRef
            {
                Type = "open_sea",
                X = 31f,
                Y = 30f,
                Heading = 37f,
                Speed = 2f,
            };

            try
            {
                yield return LoadInitialHarborAndWaitReady(bootstrapType);

                var ship = GameObject.Find("ph_ship_fishing");
                var player = GameObject.Find("D2_PlayerAvatar");
                var hull = GameObject.Find("D6_ShipHullCollision");
                Assert.IsNotNull(ship, "InitialHarbor 稳定船对象没有创建。");
                Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
                Assert.IsNotNull(hull, "InitialHarbor 缺少 D6_ShipHullCollision。");

                var hullBounds = hull.GetComponent<BoxCollider>().bounds;
                Debug.Log("[Q26/A13] saved_open_sea_reset=true" +
                          ", ship=" + ship.transform.position +
                          ", heading=" + ship.transform.eulerAngles.y.ToString("F2") +
                          ", player=" + player.transform.position +
                          ", hullMaxZ=" + hullBounds.max.z.ToString("F2"));

                Assert.AreEqual(0f, ship.transform.position.x, 0.05f,
                    "港口恢复不能沿用 open_sea 的 X 坐标。");
                Assert.AreEqual(-5.2f, ship.transform.position.z, 0.05f,
                    "港口恢复必须回到 D19 作者船根泊位，而不是 open_sea 坐标。");
                Assert.Less(Mathf.Abs(Mathf.DeltaAngle(ship.transform.eulerAngles.y, 180f)), 0.5f,
                    "港口恢复必须使用 D19 作者船向，不能沿用 open_sea 航向。");
                Assert.Less(hullBounds.max.z, player.transform.position.z - 0.05f,
                    "恢复后的船体碰撞不能横在出生点到水域的正向路线前。");
            }
            finally
            {
                if (originalPlayer == null)
                    GameState.CurrentPlayer = null;
                else
                {
                    GameState.CurrentPlayer = originalPlayer;
                    GameState.CurrentPlayer.Location = originalLocation;
                }
            }
        }

        [UnityTest]
        public IEnumerator Q26A14ShorelineRocksHaveOwnedTerrainContact()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            yield return LoadInitialHarborAndWaitReady(bootstrapType);

            var r03 = GameObject.Find("D19_R03_HarborPresentation");
            Assert.IsNotNull(r03, "InitialHarbor 缺少 R03 港口表现根。");
            var terrainTransform = FindNamedTransform(r03.transform, "D19_TerrainRoot");
            var terrain = terrainTransform == null ? null : terrainTransform.GetComponent<Collider>();
            Assert.IsNotNull(terrain, "R03 缺少自有 D19_TerrainRoot，无法验证岸线岩石接地。");
            Assert.IsTrue(terrain.enabled, "R03 D19_TerrainRoot 必须启用才能作为岸线接地证据。");

            var failures = new List<string>();
            for (var i = 0; i < 9; i++)
            {
                var targetTransform = FindNamedTransform(r03.transform, "ShoreRock_" + i);
                if (targetTransform == null)
                {
                    failures.Add("ShoreRock_" + i + " missing");
                    continue;
                }
                var target = targetTransform.gameObject;

                var renderers = target.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                {
                    failures.Add(target.name + " has no Renderer");
                    continue;
                }

                var bounds = renderers[0].bounds;
                for (var rendererIndex = 1; rendererIndex < renderers.Length; rendererIndex++)
                    bounds.Encapsulate(renderers[rendererIndex].bounds);

                var origin = new Vector3(bounds.center.x, bounds.max.y + 1f, bounds.center.z);
                RaycastHit hit;
                var hasGround = terrain.Raycast(new Ray(origin, Vector3.down), out hit, 32f);
                var error = hasGround ? Mathf.Abs(bounds.min.y - hit.point.y) : float.PositiveInfinity;
                if (!hasGround || error > 0.12f)
                    failures.Add(target.name +
                        " ground=" + (hasGround ? hit.point.y.ToString("F3") : "missing") +
                        " minY=" + bounds.min.y.ToString("F3") +
                        " error=" + error.ToString("F3"));
            }

            Debug.Log("[Q26/A14] shoreline_rocks=9, failures=" +
                      string.Join(" | ", failures.ToArray()));
            Assert.IsEmpty(failures,
                "岸线岩石必须落在自有 D19_TerrainRoot 附近，不能以脱离岛体的固定高度进入 Player。");
        }

        private static IEnumerator LoadInitialHarborAndWaitReady(Type bootstrapType)
        {
            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            var deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = UnityEngine.Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "InitialHarbor D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "InitialHarbor D2 运行时接线未完成。");
        }

        private static Transform FindNamedTransform(Transform root, params string[] names)
        {
            if (root == null) return null;
            string current = root.name.ToLowerInvariant();
            for (var i = 0; i < names.Length; i++)
                if (current == names[i].ToLowerInvariant()) return root;
            for (var i = 0; i < root.childCount; i++)
            {
                var result = FindNamedTransform(root.GetChild(i), names);
                if (result != null) return result;
            }
            return null;
        }

        private static string GetTransformPath(Transform transform)
        {
            var parts = new List<string>();
            var current = transform;
            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private static Type FindLoadedType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }
    }
}
