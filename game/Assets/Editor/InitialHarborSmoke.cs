// ============================================================
// InitialHarborSmoke —— InitialHarbor 场景冒烟验证（P1C）
//
// 归属：Editor 工具（场景搭建职责）
// 用途：batch mode 可执行的自检（-executeMethod），
//       验证场景可加载且关键对象齐全；供 CI/命令行与人工验收使用。
// ============================================================

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Whalebone.EditorTools
{
    /// <summary>
    /// 冒烟验证：加载 InitialHarbor 并断言 相机/灯光/船/水手/地名 UI/相机跟随 全部就绪。
    /// 任一断言失败抛异常 → batch 非 0 退出。
    /// </summary>
    public static class InitialHarborSmoke
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";

        [MenuItem("Tools/Whalebone/Smoke Test InitialHarbor")]
        public static void Run()
        {
            try
            {
                // 1. 场景可加载
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.isLoaded) throw new Exception("场景未能加载: " + ScenePath);

                // 2. 相机 + 跟随组件 + 目标绑定
                var camGo = GameObject.Find("Main Camera");
                if (camGo == null) throw new Exception("缺少 Main Camera");
                var cam = camGo.GetComponent<Camera>();
                if (cam == null) throw new Exception("Main Camera 缺少 Camera 组件");
                var follow = camGo.GetComponent<Whalebone.Gameplay.CameraFollow>();
                if (follow == null) throw new Exception("Main Camera 缺少 CameraFollow");
                if (follow.Target == null) throw new Exception("CameraFollow.Target 未绑定（应指向玩家船）");

                // 3. 主光
                var sun = Object.FindFirstObjectByType<Light>();
                if (sun == null || sun.type != LightType.Directional) throw new Exception("缺少 Directional 主光");

                // 4. 船 / 老水手（Prefab 实例）
                var ship = GameObject.Find("ph_ship_fishing");
                if (ship == null) throw new Exception("缺少 ph_ship_fishing 实例");
                var crew = GameObject.Find("ph_crew_male");
                if (crew == null) throw new Exception("缺少 ph_crew_male 实例");
                // 水手应站在船头附近（甲板高度）
                if (Mathf.Abs(crew.transform.position.z - ship.transform.position.z) > 3f)
                    throw new Exception("老水手不在船头附近，位置异常");

                // 5. 地名 UI 标签
                var texts = Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
                bool foundLabel = false;
                foreach (Text t in texts)
                {
                    if (t.text == "圣玛格丽特港") { foundLabel = true; break; }
                }
                if (!foundLabel) throw new Exception("缺少地名标签 UI：圣玛格丽特港");

                // 6. Build Settings 已收录
                bool inBuild = Array.Exists(EditorBuildSettings.scenes, s => s.path == ScenePath && s.enabled);
                if (!inBuild) throw new Exception("InitialHarbor 未加入 Build Settings");

                Debug.Log($"[Smoke] PASS InitialHarbor: 相机跟随={follow.Target.name}, 光={sun.name}, 船={ship.name}, 水手={crew.name}, 标签=圣玛格丽特港, inBuild={inBuild}");
            }
            catch (Exception e)
            {
                Debug.LogError("[Smoke] FAIL InitialHarbor: " + e.Message);
                throw;
            }
        }
    }
}
