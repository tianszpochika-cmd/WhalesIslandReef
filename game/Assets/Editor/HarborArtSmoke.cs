// ============================================================
// HarborArtSmoke —— 港口美化场景冒烟验证（P1C-U2）
//
// 归属：Editor 工具（场景美术搭建职责）
// 用途：菜单 Tools > Whalebone > Smoke Test Harbor Art
//       断言场景内存在 船/老水手/椰树/岩石/海面材质/相机跟随/地名标签，
//       任一失败抛异常（Console 红色报错），供 GUI 一键验收。
// ============================================================

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Whalebone.EditorTools
{
    /// <summary>冒烟验证：加载 InitialHarbor 并断言美化资产齐全。</summary>
    public static class HarborArtSmoke
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";

        [MenuItem("Tools/Whalebone/Smoke Test Harbor Art")]
        public static void Run()
        {
            try
            {
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.isLoaded) throw new Exception("场景未能加载: " + ScenePath);

                // 1. 船（Quaternius FBX 实例）
                var ship = GameObject.Find("ph_ship_fishing");
                if (ship == null) throw new Exception("缺少船实例 ph_ship_fishing");
                if (ship.GetComponentInChildren<MeshRenderer>() == null) throw new Exception("船实例无网格渲染器");

                // 2. 老水手（Quaternius FBX 实例）站船头附近
                var crew = GameObject.Find("ph_crew_male");
                if (crew == null) throw new Exception("缺少老水手实例 ph_crew_male");
                if (Mathf.Abs(crew.transform.position.z - ship.transform.position.z) > 3f)
                    throw new Exception("老水手不在船头附近");

                // 3. 椰树 ≥3 棵
                int palms = Count("ph_palm_tree");
                if (palms < 3) throw new Exception("椰树数量不足（应 ≥3）");

                // 4. 岩石 ≥3
                int rocks = Count("ph_rock");
                if (rocks < 3) throw new Exception("岩石数量不足（应 ≥3）");

                // 5. 海面材质 = 半透明蓝占位（非默认/丢失）
                var sea = GameObject.Find("Sea");
                if (sea == null) throw new Exception("缺少海面 Sea");
                var seaMat = sea.GetComponent<Renderer>().sharedMaterial;
                if (seaMat == null || !seaMat.name.Contains("ph_mat_sea"))
                    throw new Exception("海面材质未使用 ph_mat_sea");

                // 6. 相机跟随绑定
                var camGo = GameObject.Find("Main Camera");
                if (camGo == null) throw new Exception("缺少 Main Camera");
                var follow = camGo.GetComponent<Whalebone.Gameplay.CameraFollow>();
                if (follow == null) throw new Exception("缺少 CameraFollow");
                if (follow.Target == null) throw new Exception("CameraFollow.Target 未绑定");

                // 7. 地名标签
                bool foundLabel = false;
                foreach (Text t in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
                    if (t.text == "圣玛格丽特港") { foundLabel = true; break; }
                if (!foundLabel) throw new Exception("缺少地名标签：圣玛格丽特港");

                // 8. Build Settings 收录
                if (!Array.Exists(EditorBuildSettings.scenes, s => s.path == ScenePath && s.enabled))
                    throw new Exception("InitialHarbor 未加入 Build Settings");

                Debug.Log($"[Smoke] PASS HarborArt: 船={ship.name}, 水手={crew.name}, 椰树={palms}, 岩石={rocks}, 海面={seaMat.name}, 相机Target={follow.Target.name}, 标签=圣玛格丽特港");
            }
            catch (Exception e)
            {
                Debug.LogError("[Smoke] FAIL HarborArt: " + e.Message);
                throw;
            }
        }

        private static int Count(string name)
        {
            var all = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int n = 0;
            foreach (var t in all) if (t.name == name) n++;
            return n;
        }
    }
}
