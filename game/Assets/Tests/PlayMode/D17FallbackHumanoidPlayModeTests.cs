using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.D17
{
    /// <summary>
    /// D17-A02 的代码级运行回归：真实 PlayMode 创建降级人物，验证物理、骨骼和动作过渡。
    /// 不代表最终美术资源或 D17-S02 正式验收通过。
    /// </summary>
    public sealed class D17FallbackHumanoidPlayModeTests
    {
        [UnityTest]
        public IEnumerator D17A02FallbackHumanoidBindsRuntimeActionAndPhysics()
        {
            var builderType = FindLoadedType("Whalebone.Gameplay.D2.D2FallbackHumanoidBuilder");
            var visualProbeType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterVisualProbe");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            var actionType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAction");
            Assert.IsNotNull(builderType, "D2FallbackHumanoidBuilder 未加载。");
            Assert.IsNotNull(visualProbeType, "D2CharacterVisualProbe 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");
            Assert.IsNotNull(actionType, "D2CharacterAction 未加载。");

            GameObject root = null;
            try
            {
                var create = builderType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                root = (GameObject)create.Invoke(null, new object[]
                {
                    "D17_FallbackHumanoid_PlayMode",
                    Vector3.zero,
                    new Color(0.12f, 0.34f, 0.62f),
                });
                Assert.IsNotNull(root, "低模人物根节点没有创建。");

                var hasVisual = (bool)visualProbeType.GetMethod("HasCharacterVisual")
                    .Invoke(null, new object[] { root });
                Assert.IsTrue(hasVisual, "低模人物没有被识别为角色表现。");
                Assert.GreaterOrEqual(root.GetComponentsInChildren<Renderer>(true).Length, 10,
                    "低模人物渲染层数量过少，不能作为人物表现。");

                var character = root.AddComponent(characterType);
                yield return null;
                Assert.IsTrue((bool)characterType.GetProperty("HasPhysicsBody").GetValue(character),
                    "低模人物没有 CharacterController。");
                Assert.IsTrue((bool)characterType.GetProperty("CollisionEnabled").GetValue(character),
                    "低模人物碰撞没有启用。");

                var animation = root.AddComponent(animationType);
                yield return null;
                int boundBones = (int)animationType.GetProperty("BoundBoneCount").GetValue(animation);
                bool proceduralReady = (bool)animationType.GetProperty("ProceduralPoseLayerReady")
                    .GetValue(animation);
                Assert.GreaterOrEqual(boundBones, 8, "低模人物动作骨骼没有全部绑定。");
                Assert.IsTrue(proceduralReady, "低模人物程序姿态层没有就绪。");

                var boarding = Enum.Parse(actionType, "Boarding");
                animationType.GetMethod("PlayTransition").Invoke(animation, new[] { boarding });
                yield return null;
                Assert.IsTrue((bool)animationType.GetProperty("IsTransitioning").GetValue(animation),
                    "上船动作过渡没有进入运行状态。");

                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                {
                    if (collider is CharacterController) continue;
                    Assert.IsFalse(collider.enabled,
                        "低模子物体碰撞体必须关闭，避免与根 CharacterController 重复碰撞。");
                }
            }
            finally
            {
                if (root != null) UnityEngine.Object.Destroy(root);
            }
        }

        [UnityTest]
        public IEnumerator D17A01PresentationBindsAfterCharacterControllerIsAdded()
        {
            var builderType = FindLoadedType("Whalebone.Gameplay.D2.D2FallbackHumanoidBuilder");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var presentationType = FindLoadedType("Whalebone.Gameplay.D7.D7CharacterPresentationController");
            Assert.IsNotNull(builderType, "D2FallbackHumanoidBuilder 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(presentationType, "D7CharacterPresentationController 未加载。");

            GameObject root = null;
            try
            {
                var create = builderType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                root = (GameObject)create.Invoke(null, new object[]
                {
                    "D17_PresentationLifecycle_PlayMode",
                    Vector3.zero,
                    new Color(0.12f, 0.34f, 0.62f),
                });

                // 复现 D7 高优先级协调器先挂表现组件、D2 随后添加物理组件的真实顺序。
                var presentation = root.AddComponent(presentationType);
                Assert.IsFalse((bool)presentationType.GetProperty("IsReady").GetValue(presentation),
                    "没有 CharacterController 时表现组件不应提前宣称 Ready。");

                root.AddComponent(characterType);
                yield return null;

                Assert.IsTrue((bool)presentationType.GetProperty("IsReady").GetValue(presentation),
                    "CharacterController 后挂载时，表现组件没有重新绑定。");
                Assert.IsTrue((bool)presentationType.GetProperty("HasHumanVisual").GetValue(presentation),
                    "重新绑定后没有保留人物视觉判定。");
            }
            finally
            {
                if (root != null) UnityEngine.Object.Destroy(root);
            }
        }

        [UnityTest]
        public IEnumerator D17A03DisembarkRefusesWhenNoWalkableSurfaceExists()
        {
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component router = null;
            float deadline = Time.realtimeSinceStartup + 8f;
            while (router == null && Time.realtimeSinceStartup < deadline)
            {
                router = UnityEngine.Object.FindFirstObjectByType(routerType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(router, "InitialHarbor 控制路由器未创建。");
            var tryToggle = routerType.GetMethod("TryToggleShip",
                BindingFlags.Public | BindingFlags.Instance);
            var mode = routerType.GetProperty("Mode",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(tryToggle, "D2ControlRouter 缺少 TryToggleShip。");
            Assert.IsNotNull(mode, "D2ControlRouter 缺少 Mode。");

            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象未创建。");
            Assert.IsNotNull(ship, "InitialHarbor 船对象未创建。");

            bool boarded = (bool)tryToggle.Invoke(router, null);
            Assert.IsTrue(boarded, "测试前无法完成上船。");

            // 把船移到没有港口行走面的区域；下船必须拒绝，而不是把人物
            // 瞬移到水面/空中继续伪装成“已下船”。
            ship.transform.position += new Vector3(1000f, 0f, 1000f);
            bool disembarked = (bool)tryToggle.Invoke(router, null);

            Assert.IsFalse(disembarked, "没有行走面时不应允许下船。");
            Assert.AreEqual("Ship", mode.GetValue(router).ToString(),
                "安全下船失败后控制权必须仍保持在船上。");
            Assert.AreSame(ship.transform, player.transform.parent,
                "安全下船失败后玩家必须继续挂在船上，不能漂浮在无碰撞区域。");
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
