using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WhalesIslandReef.Tests.D19S02R01
{
    /// <summary>
    /// R01 only measures the real runtime contracts. It does not teleport the
    /// player or write gameplay state except through the existing save owner.
    /// </summary>
    public sealed class D19S02R01RoutePlayModeTests : InputTestFixture
    {
        private const string ResultRoot = "tools/results/D19/D19-S02-R01";

        [UnityTest]
        public IEnumerator R01RouteMovesBoardsSavesAndReloads()
        {
            var inputRouterType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var saveCoordinatorType = FindLoadedType("Whalebone.Gameplay.D7.D7SaveCoordinator");
            Assert.IsNotNull(inputRouterType, "D16InputRouter 未加载。");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            Assert.IsNotNull(saveCoordinatorType, "D7SaveCoordinator 未加载。");

            var setBlocked = inputRouterType.GetMethod("SetGameplayInputBlocked", BindingFlags.Public | BindingFlags.Static);
            var saveAll = saveCoordinatorType.GetMethod("SaveAllRuntimeState", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(setBlocked, "D16InputRouter 缺少输入锁入口。");
            Assert.IsNotNull(saveAll, "D7SaveCoordinator 缺少全量保存入口。");
            setBlocked.Invoke(null, new object[] { false });

            Keyboard keyboard = null;
            var samples = new List<RouteSample>();
            var routeStart = Vector3.zero;
            var routeEnd = Vector3.zero;
            var shipStart = Vector3.zero;
            var shipEnd = Vector3.zero;
            bool sceneReloaded = false;
            bool boarded = false;
            bool disembarked = false;
            bool saved = false;
            try
            {
                yield return LoadSceneAndWait("InitialHarbor", bootstrapType);
                var player = GameObject.Find("D2_PlayerAvatar");
                var ship = GameObject.Find("ph_ship_fishing");
                Assert.IsNotNull(player, "R01 路线没有玩家对象。");
                Assert.IsNotNull(ship, "R01 路线没有起始船对象。");
                var character = player.GetComponent(characterType);
                var grounded = characterType.GetProperty("IsGrounded");
                Assert.IsNotNull(character, "R01 玩家缺少 D2CharacterController。");
                yield return WaitUntil(() => (bool)grounded.GetValue(character), 4f);

                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                routeStart = player.transform.position;
                samples.Add(new RouteSample("spawn", routeStart, (bool)grounded.GetValue(character)));

                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                for (int i = 0; i < 90; i++)
                {
                    yield return null;
                    if (i % 30 == 29)
                        samples.Add(new RouteSample("walk_" + (i + 1), player.transform.position, (bool)grounded.GetValue(character)));
                }
                Release(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                yield return null;
                routeEnd = player.transform.position;
                samples.Add(new RouteSample("walk_end", routeEnd, (bool)grounded.GetValue(character)));

                var router = UnityEngine.Object.FindFirstObjectByType(routerType, FindObjectsInactive.Include) as Component;
                var toggleShip = routerType.GetMethod("TryToggleShip", BindingFlags.Public | BindingFlags.Instance);
                var mode = routerType.GetProperty("Mode", BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(router, "R01 路线没有 D2ControlRouter。");
                Assert.IsNotNull(toggleShip, "D2ControlRouter 缺少 TryToggleShip。");
                Assert.IsNotNull(mode, "D2ControlRouter 缺少 Mode。");

                boarded = (bool)toggleShip.Invoke(router, null);
                Assert.IsTrue(boarded, "R01 路线不能在起始泊位上船。");
                Assert.AreEqual("Ship", mode.GetValue(router).ToString(), "上船后控制模式不是 Ship。");
                shipStart = ship.transform.position;
                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                for (int i = 0; i < 60; i++) yield return null;
                Release(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                yield return null;
                shipEnd = ship.transform.position;
                samples.Add(new RouteSample("ship_end", player.transform.position, (bool)grounded.GetValue(character)));
                Assert.Greater(Vector3.Distance(shipStart, shipEnd), 0.001f, "上船后 W 没有驱动船只移动。");

                disembarked = (bool)toggleShip.Invoke(router, null);
                Assert.IsTrue(disembarked, "R01 路线不能下船。");
                Assert.AreEqual("Character", mode.GetValue(router).ToString(), "下船后控制模式不是 Character。");
                yield return null;
                Assert.IsTrue((bool)grounded.GetValue(character), "R01 下船后玩家没有保持落地。");

                saveAll.Invoke(null, null);
                saved = true;
                yield return null;

                yield return LoadSceneAndWait("InitialHarbor", bootstrapType);
                sceneReloaded = GameObject.Find("D19_SaintMargaret_Root") != null &&
                                GameObject.Find("D2_PlayerAvatar") != null &&
                                GameObject.Find("ph_ship_fishing") != null;
                Assert.IsTrue(sceneReloaded, "保存后重新加载 InitialHarbor 缺少稳定运行时对象。");
                WriteRouteReport(new RouteReport
                {
                    status = "PASS",
                    route_contract = "spawn -> walk -> board -> sail -> disembark -> save -> reload",
                    actual_runtime_seconds = 90f / 60f + 60f / 60f,
                    route_start = routeStart,
                    route_end = routeEnd,
                    ship_start = shipStart,
                    ship_end = shipEnd,
                    player_walk_distance = Vector3.Distance(routeStart, routeEnd),
                    ship_distance = Vector3.Distance(shipStart, shipEnd),
                    boarded = boarded,
                    disembarked = disembarked,
                    saved = saved,
                    scene_reloaded = sceneReloaded,
                    stable_anchor_count_after_reload = CountStableAnchors(),
                    samples = samples.ToArray(),
                    note = "This is an automated runtime probe; it does not claim the human 5—10 minute route gate."
                });
            }
            finally
            {
                Time.timeScale = 1f;
                setBlocked.Invoke(null, new object[] { false });
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.wKey);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
            }
        }

        [UnityTest]
        public IEnumerator R01PlayMode1080pFrameAndMemorySample()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            yield return LoadSceneAndWait("InitialHarbor", bootstrapType);

            Screen.SetResolution(1920, 1080, false);
            for (int i = 0; i < 10; i++) yield return null;

            var frameTimes = new List<float>();
            var gc0 = GC.CollectionCount(0);
            var gc1 = GC.CollectionCount(1);
            var gc2 = GC.CollectionCount(2);
            var memoryBefore = Profiler.GetTotalAllocatedMemoryLong();
            using (var frameTimeRecorder = new ProfilerRecorder(ProfilerCategory.Internal, "Main Thread", 15))
            {
                frameTimeRecorder.Start();
                for (int i = 0; i < 120; i++)
                {
                    var started = Time.realtimeSinceStartup;
                    yield return null;
                    frameTimes.Add((Time.realtimeSinceStartup - started) * 1000f);
                }
                frameTimeRecorder.Stop();
            }

            frameTimes.Sort();
            var actualWidth = Screen.width;
            var actualHeight = Screen.height;
            WritePerformanceReport(new PlayModePerformanceReport
            {
                status = actualWidth == 1920 && actualHeight == 1080 ? "RECORDED_PLAYMODE_1080P" : "RECORDED_PLAYMODE_REQUESTED_1080P",
                requested_width = 1920,
                requested_height = 1080,
                actual_width = actualWidth,
                actual_height = actualHeight,
                frame_count = frameTimes.Count,
                average_frame_ms = frameTimes.Count == 0 ? 0f : frameTimes.Average(),
                p95_frame_ms = frameTimes.Count == 0 ? 0f : frameTimes[Mathf.Min(frameTimes.Count - 1, Mathf.FloorToInt(frameTimes.Count * 0.95f))],
                allocated_memory_before_bytes = memoryBefore,
                allocated_memory_after_bytes = Profiler.GetTotalAllocatedMemoryLong(),
                gc_gen0 = GC.CollectionCount(0) - gc0,
                gc_gen1 = GC.CollectionCount(1) - gc1,
                gc_gen2 = GC.CollectionCount(2) - gc2,
                gpu_timing_available = false,
                note = "Unity batch PlayMode sample; GPU timing and target-device budget remain unproven."
            });
            Assert.That(frameTimes.Count, Is.EqualTo(120));
        }

        private static IEnumerator LoadSceneAndWait(string sceneName, Type bootstrapType)
        {
            var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, sceneName + " 不在 Build Settings 中。");
            while (!load.isDone) yield return null;
            yield return WaitUntil(() =>
            {
                var bootstrap = UnityEngine.Object.FindFirstObjectByType(bootstrapType, FindObjectsInactive.Include) as Component;
                var ready = bootstrapType.GetProperty("IsReady");
                return bootstrap != null && ready != null && (bool)ready.GetValue(bootstrap);
            }, 8f);
        }

        private static IEnumerator WaitUntil(Func<bool> predicate, float timeoutSeconds)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!predicate())
            {
                if (Time.realtimeSinceStartup >= deadline)
                    Assert.Fail("R01 PlayMode 条件等待超时。");
                yield return null;
            }
        }

        private static int CountStableAnchors()
        {
            var root = GameObject.Find("D19_StableInteractionAnchors");
            return root == null ? 0 : root.GetComponentsInChildren<Transform>(true).Count(t => t.name.StartsWith("D19_Anchor_", StringComparison.Ordinal));
        }

        private static void WriteRouteReport(RouteReport report)
        {
            var path = ProjectPath(ResultRoot + "/D19_S02_R01_ROUTE_LOG.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

        private static void WritePerformanceReport(PlayModePerformanceReport report)
        {
            var path = ProjectPath(ResultRoot + "/D19_S02_R01_PERFORMANCE_PLAYMODE_1080P.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath.Replace('/', Path.DirectorySeparatorChar)));
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

        [Serializable]
        private sealed class RouteReport
        {
            public string status;
            public string route_contract;
            public float actual_runtime_seconds;
            public Vector3 route_start;
            public Vector3 route_end;
            public Vector3 ship_start;
            public Vector3 ship_end;
            public float player_walk_distance;
            public float ship_distance;
            public bool boarded;
            public bool disembarked;
            public bool saved;
            public bool scene_reloaded;
            public int stable_anchor_count_after_reload;
            public RouteSample[] samples;
            public string note;
        }

        [Serializable]
        private sealed class RouteSample
        {
            public string label;
            public Vector3 position;
            public bool grounded;

            public RouteSample(string label, Vector3 position, bool grounded)
            {
                this.label = label;
                this.position = position;
                this.grounded = grounded;
            }
        }

        [Serializable]
        private sealed class PlayModePerformanceReport
        {
            public string status;
            public int requested_width;
            public int requested_height;
            public int actual_width;
            public int actual_height;
            public int frame_count;
            public float average_frame_ms;
            public float p95_frame_ms;
            public long allocated_memory_before_bytes;
            public long allocated_memory_after_bytes;
            public int gc_gen0;
            public int gc_gen1;
            public int gc_gen2;
            public bool gpu_timing_available;
            public string note;
        }
    }
}
