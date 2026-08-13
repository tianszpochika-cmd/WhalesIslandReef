using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.D17
{
    /// <summary>
    /// D17-A04 的本机输入回归：验证没有鼠标时，手柄右摇杆仍能驱动第三人称镜头。
    /// 该测试只证明 Unity Input System 到表现层的代码链路，不替代真实手柄和目标分辨率验收。
    /// </summary>
    public sealed class D17CameraInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator D17A04InitialHarborGamepadRightStickRotatesCamera()
        {
            var routerType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            var cameraType = FindLoadedType("Whalebone.Gameplay.CameraFollow");
            Assert.IsNotNull(routerType, "D16InputRouter 未加载。");
            Assert.IsNotNull(cameraType, "CameraFollow 未加载。");

            var setGameplayInputBlocked = routerType.GetMethod("SetGameplayInputBlocked",
                BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(setGameplayInputBlocked, "D16InputRouter 缺少输入锁入口。");
            setGameplayInputBlocked.Invoke(null, new object[] { false });

            Gamepad gamepad = null;
            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            try
            {
                Camera camera = null;
                float deadline = Time.realtimeSinceStartup + 8f;
                while (camera == null && Time.realtimeSinceStartup < deadline)
                {
                    camera = Camera.main;
                    if (camera == null) camera = Object.FindFirstObjectByType<Camera>();
                    yield return null;
                }

                Assert.IsNotNull(camera, "InitialHarbor 主相机未创建。");
                var cameraFollow = camera.GetComponent(cameraType);
                Assert.IsNotNull(cameraFollow, "主相机缺少 CameraFollow。");
                var yawField = cameraType.GetField("yaw",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(yawField, "CameraFollow 缺少可验证的水平视角状态。");
                // 等待场景相机从初始位置平滑收敛，再比较输入前后的旋转，
                // 避免把启动摆位动画误判为手柄输入穿透。
                for (int i = 0; i < 5; i++) yield return null;

                gamepad = InputSystem.AddDevice<Gamepad>();
                gamepad.MakeCurrent();
                setGameplayInputBlocked.Invoke(null, new object[] { true });
                float blockedStartYaw = (float)yawField.GetValue(cameraFollow);
                InputSystem.QueueStateEvent(gamepad, new GamepadState
                {
                    rightStick = new Vector2(1f, 0f),
                });
                InputSystem.Update();
                for (int i = 0; i < 3; i++) yield return null;
                InputSystem.QueueStateEvent(gamepad, new GamepadState
                {
                    rightStick = Vector2.zero,
                });
                InputSystem.Update();
                Assert.AreEqual(blockedStartYaw, (float)yawField.GetValue(cameraFollow), 0.001f,
                    "暂停或菜单输入锁打开时，手柄镜头输入不能穿透到玩法层。");

                setGameplayInputBlocked.Invoke(null, new object[] { false });
                Quaternion start = camera.transform.rotation;
                float startYaw = (float)yawField.GetValue(cameraFollow);
                InputSystem.QueueStateEvent(gamepad, new GamepadState
                {
                    rightStick = new Vector2(1f, 0f),
                });
                InputSystem.Update();
                for (int i = 0; i < 3; i++) yield return null;
                InputSystem.QueueStateEvent(gamepad, new GamepadState
                {
                    rightStick = Vector2.zero,
                });
                InputSystem.Update();

                Assert.Greater(Quaternion.Angle(start, camera.transform.rotation), 0.1f,
                    "手柄右摇杆没有驱动第三人称镜头旋转。");
                Assert.Greater(Mathf.Abs((float)yawField.GetValue(cameraFollow) - startYaw), 0.1f,
                    "手柄右摇杆没有更新 CameraFollow 的水平视角状态。");
            }
            finally
            {
                setGameplayInputBlocked.Invoke(null, new object[] { false });
                if (gamepad != null && gamepad.added)
                {
                    InputSystem.QueueStateEvent(gamepad, new GamepadState
                    {
                        rightStick = Vector2.zero,
                    });
                    InputSystem.Update();
                    InputSystem.RemoveDevice(gamepad);
                }
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
