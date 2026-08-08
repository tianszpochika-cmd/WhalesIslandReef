// ============================================================
// InitialHarborBuilder —— 初始环境场景生成器（P1C）
//
// 归属：Editor 工具（场景搭建职责，见 DEVELOPMENT_STANDARDS §1）
// 职责：
//   1. 创建占位材质（URP Lit 纯色，ph_mat_*）→ Art/Placeholder/Materials
//   2. 创建 ph_ship_fishing / ph_crew_male 占位 Prefab（纯几何体组合）→ Prefabs/Placeholder
//   3. 搭建 InitialHarbor 场景：海/沙滩/码头/船/老水手/暖色主光/跟随相机/地名 UI
//   4. 把场景追加进 Build Settings（不改变 Bootstrap/MainMenu 现有顺序）
//
// 设计决策：
//   - 全部用占位几何体（Cube/Capsule/Sphere/Plane）+ 纯色 URP 材质，
//     零第三方版权素材（符合 SETUP.md §5 占位纪律，替换时整目录换）
//   - 场景只引用 Prefab 实例，不直接挂资源（ARCHITECTURE §8.1 替换隔离）
//   - 幂等：重复执行会先删除旧产物再重建
//   - UI 用 UGUI 动态构建（与 MainMenuController 的"纯代码可复现"方法论一致）；
//     中文字体用 Noto Sans CJK SC（SIL OFL 可商用），见 Art/Placeholder/PH_ASSET_SOURCES.md
// ============================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Whalebone.EditorTools
{
    /// <summary>
    /// 初始环境场景（圣玛格丽特港）生成工具。菜单：Tools > Whalebone > Build InitialHarbor (占位场景, 幂等)
    /// </summary>
    public static class InitialHarborBuilder
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string MatDir = "Assets/Art/Placeholder/Materials";
        private const string PrefabDir = "Assets/Prefabs/Placeholder";
        private const string FontPath = "Assets/Art/Placeholder/Fonts/ph_font_noto_sans_cjk_sc.otf";

        private const string ShipPrefabPath = PrefabDir + "/ph_ship_fishing.prefab";
        private const string CrewPrefabPath = PrefabDir + "/ph_crew_male.prefab";

        // ---------------- 入口 ----------------

        [MenuItem("Tools/Whalebone/Build InitialHarbor (占位场景, 幂等)")]
        public static void Build()
        {
            BuildInternal();
        }

        /// <summary>构建 + 冒烟验证组合入口（batch mode 一条命令跑完，CI 用）。</summary>
        public static void BuildAndSmoke()
        {
            BuildInternal();
            InitialHarborSmoke.Run();
        }

        private static void BuildInternal()
        {
            EnsureFolders();
            DeleteLegacyAssets();

            Dictionary<string, Material> mats = CreateMaterials();

            // Prefab 先在新场景中搭好再保存（避免污染当前打开场景）
            Scene scratch = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject shipPrefab = CreateShipPrefab(mats);
            GameObject crewPrefab = CreateCrewPrefab(mats);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildEnvironment(scene, mats, shipPrefab, crewPrefab);
            ConfigureLighting(scene);
            AddHarborUi(scene);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[InitialHarborBuilder] 生成完成: " + ScenePath);
        }

        // ---------------- 目录与清理 ----------------

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets", "Scenes");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Art", "Placeholder");
            EnsureFolder("Assets/Art/Placeholder", "Materials");
            EnsureFolder("Assets/Art/Placeholder", "Fonts");
            EnsureFolder("Assets/Prefabs", "Placeholder");
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent))
            {
                // 父目录缺失时逐级创建（幂等，容忍干净 checkout / 最小副本工程）
                string grand = parent.Substring(0, parent.LastIndexOf('/'));
                string leaf = parent.Substring(parent.LastIndexOf('/') + 1);
                if (grand.Length > 0 && !AssetDatabase.IsValidFolder(grand))
                {
                    EnsureFolder(grand, leaf);
                }
                AssetDatabase.CreateFolder(grand.Length == 0 ? "Assets" : grand, leaf);
            }
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        /// <summary>删除本工具之前生成的产物（幂等重建）。只删已知的 ph_ 占位文件。</summary>
        private static void DeleteLegacyAssets()
        {
            string[] legacy = { ScenePath, ShipPrefabPath, CrewPrefabPath };
            foreach (string path in legacy)
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
            // 清理旧占位材质（ph_mat_*），只删本工具目录下的
            string[] matGuids = AssetDatabase.FindAssets("ph_mat_", new[] { MatDir });
            foreach (string guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith(MatDir))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
            AssetDatabase.SaveAssets();
        }

        // ---------------- 占位材质 ----------------

        private static Dictionary<string, Material> CreateMaterials()
        {
            var mats = new Dictionary<string, Material>();
            AddLit(mats, "ph_mat_sea", new Color(0.05f, 0.30f, 0.42f, 0.88f), true);
            AddLit(mats, "ph_mat_sand", new Color(0.85f, 0.78f, 0.62f));
            AddLit(mats, "ph_mat_dock_wood", new Color(0.42f, 0.30f, 0.20f));
            AddLit(mats, "ph_mat_boat_hull", new Color(0.35f, 0.22f, 0.14f));
            AddLit(mats, "ph_mat_boat_deck", new Color(0.55f, 0.42f, 0.28f));
            AddLit(mats, "ph_mat_mast", new Color(0.25f, 0.18f, 0.12f));
            AddLit(mats, "ph_mat_sail", new Color(0.95f, 0.93f, 0.88f));
            AddLit(mats, "ph_mat_crew_cloth", new Color(0.35f, 0.45f, 0.55f));
            AddLit(mats, "ph_mat_crew_skin", new Color(0.82f, 0.62f, 0.48f));
            AddLit(mats, "ph_mat_crew_hat", new Color(0.20f, 0.16f, 0.12f));
            AddLit(mats, "ph_mat_prop_wood", new Color(0.60f, 0.50f, 0.35f));
            return mats;
        }

        private static void AddLit(Dictionary<string, Material> mats, string name, Color color, bool transparent = false)
        {
            string path = MatDir + "/" + name + ".mat";
            // 优先 URP Lit，缺失时退回 Simple Lit / Standard（不同 URP 版本兼容）
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader)
            {
                name = name,
                color = color
            };
            if (shader != null && shader.name.Contains("Lit"))
            {
                mat.SetFloat("_Smoothness", 0.3f);
                if (transparent)
                {
                    // 切换 URP Lit 到 Transparent/Alpha 混合（海面半透明）
                    mat.SetFloat("_Surface", 1f);
                    mat.SetFloat("_Blend", 0f);
                    mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    mat.SetFloat("_ZWrite", 0f);
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.renderQueue = (int)RenderQueue.Transparent;
                }
            }
            AssetDatabase.CreateAsset(mat, path);
            mats[name] = mat;
        }

        // ---------------- 占位 Prefab ----------------

        /// <summary>创建小渔船占位 Prefab（几何体组合：船体/船首/船尾/甲板/船舷/桅杆/横杆/帆）。船头朝 +Z。</summary>
        private static GameObject CreateShipPrefab(Dictionary<string, Material> mats)
        {
            var root = new GameObject("ph_ship_fishing");
            Material hull = mats["ph_mat_boat_hull"];
            Material deck = mats["ph_mat_boat_deck"];
            Material mast = mats["ph_mat_mast"];
            Material sail = mats["ph_mat_sail"];

            AddPart(root.transform, "ph_hull", PrimitiveType.Cube, hull, Vector3.zero, new Vector3(4.6f, 0.7f, 1.6f));
            AddPart(root.transform, "ph_bow", PrimitiveType.Cube, hull, new Vector3(0f, 0.48f, 2.25f), new Vector3(1.2f, 0.42f, 1.2f));
            AddPart(root.transform, "ph_stern", PrimitiveType.Cube, hull, new Vector3(0f, 0.32f, -2.2f), new Vector3(1.3f, 0.5f, 1.3f));
            AddPart(root.transform, "ph_deck", PrimitiveType.Cube, deck, new Vector3(0f, 0.72f, 0f), new Vector3(4.6f, 0.08f, 1.5f));
            AddPart(root.transform, "ph_rail_l", PrimitiveType.Cube, deck, new Vector3(-0.78f, 0.95f, 0f), new Vector3(4.6f, 0.14f, 0.08f));
            AddPart(root.transform, "ph_rail_r", PrimitiveType.Cube, deck, new Vector3(0.78f, 0.95f, 0f), new Vector3(4.6f, 0.14f, 0.08f));
            AddPart(root.transform, "ph_mast", PrimitiveType.Cube, mast, new Vector3(0f, 2.0f, 0.2f), new Vector3(0.14f, 2.6f, 0.14f));
            AddPart(root.transform, "ph_yard", PrimitiveType.Cube, mast, new Vector3(0f, 2.5f, 0.2f), new Vector3(1.8f, 0.07f, 0.07f));
            AddPart(root.transform, "ph_sail", PrimitiveType.Cube, sail, new Vector3(0f, 2.55f, 0.78f), new Vector3(0.05f, 1.5f, 1.2f));

            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = new Vector3(4.8f, 1.2f, 2.8f);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, ShipPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <summary>创建老水手占位 Prefab（胶囊身体 + 球头 + 帽）。脚底在 y=0。</summary>
        private static GameObject CreateCrewPrefab(Dictionary<string, Material> mats)
        {
            var root = new GameObject("ph_crew_male");
            Material cloth = mats["ph_mat_crew_cloth"];
            Material skin = mats["ph_mat_crew_skin"];
            Material hat = mats["ph_mat_crew_hat"];

            AddPart(root.transform, "ph_body", PrimitiveType.Capsule, cloth, new Vector3(0f, 0.95f, 0f), new Vector3(0.42f, 0.95f, 0.42f));
            AddPart(root.transform, "ph_head", PrimitiveType.Sphere, skin, new Vector3(0f, 1.95f, 0f), new Vector3(0.36f, 0.36f, 0.36f));
            AddPart(root.transform, "ph_hat", PrimitiveType.Cylinder, hat, new Vector3(0f, 2.16f, 0f), new Vector3(0.44f, 0.09f, 0.44f));

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.95f, 0f);
            collider.radius = 0.3f;
            collider.height = 1.9f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, CrewPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <summary>创建几何体子件（去 collider，材质赋值）。</summary>
        private static void AddPart(Transform parent, string name, PrimitiveType type, Material mat,
            Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
        }

        // ---------------- 场景布置 ----------------

        /// <summary>世界布局（米）：海面 y=0；陆地 -Z；码头沿 X；船停码头南侧、船头朝 +Z（面向大海）。</summary>
        private static void BuildEnvironment(Scene scene, Dictionary<string, Material> mats, GameObject shipPrefab, GameObject crewPrefab)
        {
            // ---- 海面：大平面，半透明海蓝（URP 水占位） ----
            var sea = GameObject.CreatePrimitive(PrimitiveType.Plane);
            sea.name = "Sea";
            sea.transform.position = new Vector3(0f, -0.02f, 0f);
            sea.transform.localScale = new Vector3(120f, 1f, 120f);
            sea.GetComponent<Renderer>().sharedMaterial = mats["ph_mat_sea"];
            Object.DestroyImmediate(sea.GetComponent<Collider>());

            // ---- 陆地（沙滩）：码头后方的矮台 ----
            var beach = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beach.name = "Beach";
            beach.transform.position = new Vector3(0f, -0.32f, -26f);
            beach.transform.localScale = new Vector3(70f, 1.2f, 46f);
            beach.GetComponent<Renderer>().sharedMaterial = mats["ph_mat_sand"];
            Object.DestroyImmediate(beach.GetComponent<Collider>());

            // ---- 码头组 ----
            var dock = new GameObject("Dock");
            Material wood = mats["ph_mat_dock_wood"];

            // 平台（顶面 y=0.19）
            var deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.name = "DockDeck";
            deck.transform.SetParent(dock.transform, false);
            deck.transform.localPosition = new Vector3(0f, 0.10f, -6f);
            deck.transform.localScale = new Vector3(26f, 0.18f, 3.4f);
            deck.GetComponent<Renderer>().sharedMaterial = wood;
            Object.DestroyImmediate(deck.GetComponent<Collider>());

            // 桩柱 ×6（插入水中）
            float[] pileXs = { -10f, -6f, -2f, 2f, 6f, 10f };
            foreach (float x in pileXs)
            {
                var pile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pile.name = "Pile";
                pile.transform.SetParent(dock.transform, false);
                pile.transform.localPosition = new Vector3(x, -0.71f, -6f);
                pile.transform.localScale = new Vector3(0.24f, 1.8f, 0.24f);
                pile.GetComponent<Renderer>().sharedMaterial = wood;
                Object.DestroyImmediate(pile.GetComponent<Collider>());
            }

            // 北侧护栏（陆地侧）；南侧开放登船
            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "HandRail";
            rail.transform.SetParent(dock.transform, false);
            rail.transform.localPosition = new Vector3(0f, 0.55f, -7.68f);
            rail.transform.localScale = new Vector3(26f, 0.7f, 0.08f);
            rail.GetComponent<Renderer>().sharedMaterial = wood;
            Object.DestroyImmediate(rail.GetComponent<Collider>());

            // 系缆桩 ×2（南缘两端）
            foreach (float x in new[] { -6f, 6f })
            {
                var bollard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bollard.name = "Bollard";
                bollard.transform.SetParent(dock.transform, false);
                bollard.transform.localPosition = new Vector3(x, 0.44f, -4.3f);
                bollard.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
                bollard.GetComponent<Renderer>().sharedMaterial = wood;
                Object.DestroyImmediate(bollard.GetComponent<Collider>());
            }

            // 码头杂物点缀：木桶 + 货箱（纯占位几何体）
            Material prop = mats["ph_mat_prop_wood"];
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.SetParent(dock.transform, false);
            barrel.transform.localPosition = new Vector3(2.4f, 0.42f, -5.0f);
            barrel.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
            barrel.GetComponent<Renderer>().sharedMaterial = prop;
            Object.DestroyImmediate(barrel.GetComponent<Collider>());

            var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "Crate";
            crate.transform.SetParent(dock.transform, false);
            crate.transform.localPosition = new Vector3(3.4f, 0.3f, -4.6f);
            crate.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            crate.GetComponent<Renderer>().sharedMaterial = prop;
            Object.DestroyImmediate(crate.GetComponent<Collider>());

            // ---- 船（Prefab 实例）：停靠码头南侧，船头朝 +Z（面向大海） ----
            Vector3 shipPos = new Vector3(0f, 0f, -5.2f);
            GameObject ship = (GameObject)PrefabUtility.InstantiatePrefab(shipPrefab);
            ship.name = "ph_ship_fishing";
            ship.transform.position = shipPos;
            ship.transform.rotation = Quaternion.identity; // 船头 +Z = 面向海

            // 跳板：码头南缘 → 船甲板
            var gangplank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gangplank.name = "Gangplank";
            gangplank.transform.SetParent(dock.transform, false);
            gangplank.transform.localPosition = new Vector3(0f, 0.5f, -4.75f);
            gangplank.transform.localScale = new Vector3(1.8f, 0.07f, 1.1f);
            gangplank.GetComponent<Renderer>().sharedMaterial = wood;
            Object.DestroyImmediate(gangplank.GetComponent<Collider>());

            // ---- 老水手（Prefab 实例）：站在船头甲板，面向大海（+Z） ----
            GameObject crew = (GameObject)PrefabUtility.InstantiatePrefab(crewPrefab);
            crew.name = "ph_crew_male";
            crew.transform.position = new Vector3(0f, 0.72f, -3.2f); // 甲板高度 0.72
            crew.transform.rotation = Quaternion.identity;

            // ---- 相机（第三人称跟随占位）：船尾后上方 ----
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.68f, 0.80f, 1f); // 加勒比晴空（不依赖 URP Volume 天空）
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 500f;
            cam.fieldOfView = 60f;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(2f, 5.5f, -8.5f);
            camGo.transform.LookAt(ship.transform.position + Vector3.up * 1.2f);

            var follow = camGo.AddComponent<Whalebone.Gameplay.CameraFollow>();
            follow.Target = ship.transform;

            // 记录关键对象（冒烟验证与调试用）
            Debug.Log($"[InitialHarborBuilder] 相机 Target 已绑定: {follow.Target.name} ({shipPos})");
        }

        /// <summary>光照：加勒比午间暖色主光 + 三色环境光（Trilight）。</summary>
        private static void ConfigureLighting(Scene scene)
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.94f, 0.82f); // 暖白
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            lightGo.transform.position = new Vector3(0f, 12f, -4f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.62f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.72f, 0.75f, 0.78f);
            RenderSettings.ambientGroundColor = new Color(0.42f, 0.38f, 0.30f);
            RenderSettings.fog = false;
        }

        /// <summary>地名标签 UI：Canvas Overlay + UGUI Text（Noto Sans CJK SC）。</summary>
        private static void AddHarborUi(Scene scene)
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                Debug.LogWarning("[InitialHarborBuilder] 未找到中文字体: " + FontPath + "，UI 标签将使用默认字体（可能无法显示中文）");
            }

            var canvasGo = new GameObject("Harbor UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // 主标签：港口中文名
            Text title = MakeLabel(canvasGo.transform, "HarborName", -44f, 48, "圣玛格丽特港", font, FontStyle.Bold, Color.white);
            var shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
            shadow.effectDistance = new Vector2(2f, -2f);

            // 副标签：罗马名 + 年份（英文，同字体）
            MakeLabel(canvasGo.transform, "HarborSubtitle", -100f, 22, "Sainte-Marguerite · 1675", font, FontStyle.Normal, new Color(0.95f, 0.90f, 0.80f, 0.95f));
        }

        private static Text MakeLabel(Transform parent, string name, float yPos, int fontSize, string content,
            Font font, FontStyle style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, yPos);
            rect.sizeDelta = new Vector2(900f, 70f);

            var text = go.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false; // 纯展示，不参与交互
            if (font != null) text.font = font;
            return text;
        }

        // ---------------- Build Settings ----------------

        /// <summary>把 InitialHarbor 追加到 Build Settings；保持 Bootstrap/MainMenu 现有顺序与起始场景不变。</summary>
        private static void AddToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == ScenePath)) return;
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[InitialHarborBuilder] Build Settings 已追加: " + ScenePath);
        }
    }
}
