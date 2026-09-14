using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.D17
{
    /// <summary>
    /// D17-A17 回归：水面与岛面发生垂直重叠时，角色不能继续游进可行走
    /// 地形下方；若已经在游泳态，也必须回到可行走面。该测试只验证
    /// D2 物理裁决，不把视觉占位建筑或最终 Player 画面误报为通过。
    /// </summary>
    public sealed class D17SwimmingSpacePlayModeTests
    {
        [UnityTest]
        public IEnumerator D17A17WaterOverlapRejectsSwimmingUnderWalkableTerrain()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var ready = bootstrapType.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)ready.GetValue(bootstrap)) &&
                   Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType(bootstrapType,
                    FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2 运行时入口没有创建。");
            Assert.IsTrue((bool)ready.GetValue(bootstrap), "D2 运行时组装没有完成。");
            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");

            var character = player.GetComponent(characterType);
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            var setWorldPosition = characterType.GetMethod("SetWorldPosition");
            var applyPhysics = characterType.GetMethod("ApplyPhysicsMotion",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var isSwimming = characterType.GetProperty("IsSwimming");
            var isGrounded = characterType.GetProperty("IsGrounded");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            Assert.IsNotNull(setWorldPosition, "D2CharacterController 缺少 SetWorldPosition。");
            Assert.IsNotNull(applyPhysics, "D2CharacterController 缺少 ApplyPhysicsMotion。");
            Assert.IsNotNull(isSwimming, "D2CharacterController 缺少 IsSwimming。");
            Assert.IsNotNull(isGrounded, "D2CharacterController 缺少 IsGrounded。");

            GameObject overheadTerrain = null;
            try
            {
                setInputEnabled.Invoke(character, new object[] { false });
                // R03 岛外的深水安全点：没有额外地形时，旧路径必须真实进入游泳态。
                var waterPosition = new Vector3(0f, 0.42f, 30f);
                setWorldPosition.Invoke(character, new object[] { waterPosition });
                deadline = Time.realtimeSinceStartup + 2f;
                while (!(bool)isSwimming.GetValue(character) &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue((bool)isSwimming.GetValue(character),
                    "无上方地形的深水点没有进入游泳态，测试前置条件不成立。");

                // 用 D2 认可的地形命名契约构造一个可行走顶面，模拟海面与
                // 岛面裙边重叠；对象只存在于本次 PlayMode 测试，不改场景资产。
                overheadTerrain = new GameObject("D19_TerrainRoot");
                overheadTerrain.transform.position = new Vector3(0f, 1.65f, 30f);
                var collider = overheadTerrain.AddComponent<BoxCollider>();
                collider.name = "D19_TerrainRoot";
                collider.size = new Vector3(5f, 0.2f, 5f);
                Physics.SyncTransforms();

                // 直接执行一次真实 D2 物理步，把阻断事件与恢复结果锁定在
                // 同一调用内；否则下一帧已经站到地面上时，当前态诊断字段
                // 合理地会回到 false，测试就会丢失本次阻断事件。
                LogAssert.Expect(LogType.Log,
                    "[D2/Physics] 玩家退出游泳态：reason=游泳空间上方存在可行走地面");
                LogAssert.Expect(LogType.Log,
                    new System.Text.RegularExpressions.Regex(
                        @"^\[D2/Physics\] 已从水下地形重定位到可行走面：surface=D19_TerrainRoot, surfaceY=1\.75$"));
                applyPhysics.Invoke(character, null);

                Assert.IsFalse((bool)isSwimming.GetValue(character),
                    "水面与可行走地形重叠时，角色仍停留在地形下方游泳态。");
                Assert.IsTrue((bool)isGrounded.GetValue(character),
                    "检测到上方可行走地形后，角色没有恢复 grounded。");
                Assert.Greater(player.transform.position.y, 1.5f,
                    "从水下地形恢复后玩家仍停留在水面高度附近。");
            }
            finally
            {
                setInputEnabled.Invoke(character, new object[] { true });
                if (overheadTerrain != null)
                    Object.DestroyImmediate(overheadTerrain);
            }
        }

        private static System.Type FindLoadedType(string fullName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }
    }
}
