// ============================================================
// HarborArtInstaller —— 港口场景 CC0 资产美化安装器（P1C-U2）
//
// 归属：Editor 工具（场景美术搭建职责，见 DEVELOPMENT_STANDARDS §1）
// 菜单：Tools > Whalebone > Install Harbor Art (占位美化, 幂等)
//
// 职责：
//   1. 用 Quaternius CC0 资产（FBX 船/人形）替换灰盒几何体
//   2. 植被/岩石用内置几何体 + URP 纯色材质（Blender 缺失的降级方案）
//   3. 全代码重建 InitialHarbor 场景（绝不手写场景 YAML——D1 fileID 重复教训）
//   4. 幂等：重复执行先清旧产物再重建；不触碰 Assets/Scripts/ 任何逻辑代码
//
// 素材来源（全部 CC0，详见 Art/Placeholder/PH_ASSET_SOURCES.md）：
//   - ph_ship_fishing.fbx / ph_crew_male.fbx ← Quaternius（CC0 1.0）
//   - 其余 ← Unity 内置几何体 + 自写 URP 材质
//
// 设计决策：
//   - fbx 实例统一覆盖占位材质（船=船体棕、人=亚麻色），避免贴图引用断裂显示粉红
//   - 椰树 = 圆柱树干 + 扁 Cube 叶冠（4 片斜插）；岩石 = 压扁球体；草丛/花 = 小几何体
//   - 天空 = Procedural Skybox（纯代码创建，零下载）；海 = 半透明蓝 URP Lit
//   - CameraFollow 保留运行时自动 Find("ph_ship_fishing") 的绑定方式
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
    /// 初始环境场景（圣玛格丽特港）CC0 资产美化安装器。菜单：Tools > Whalebone > Install Harbor Art (占位美化, 幂等)
    /// </summary>
    [InitializeOnLoad]
    public static class HarborArtInstaller
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string ArtRoot = "Assets/Art/Placeholder";
        private const string MatDir = ArtRoot + "/Materials";
        private const string PrefabDir = "Assets/Prefabs/Placeholder";
        private const string FontPath = ArtRoot + "/Fonts/ph_font_noto_sans_cjk_sc.otf";

        private const string ShipFbx = ArtRoot + "/Models/Ships/ph_ship_fishing.fbx";
        private const string CrewFbx = ArtRoot + "/Models/Crew/ph_crew_male.fbx";

        // 旧几何体 Prefab（被 fbx 替换，清理）
        private const string LegacyShipPrefab = PrefabDir + "/ph_ship_fishing.prefab";
        private const string LegacyCrewPrefab = PrefabDir + "/ph_crew_male.prefab";
        private const string AutoRepairSessionKey = "Whalebone.HarborArtInstaller.AutoRepairAttempted";

        // 世界布局常量（米：海面 y=0；陆地 -Z；码头沿 X；船停码头南侧、船头朝 +Z 面向大海）
        private static readonly Vector3 ShipPos = new Vector3(0f, 0f, -5.2f);
        private const float BeachTopY = 0.28f;      // 沙滩顶面
        private const float DockTopY = 0.19f;       // 码头平台顶面

        static HarborArtInstaller()
        {
            // 资产目录被清理、首次导入中断或旧场景引用失效时，自动修复一次。
            // 延迟到编辑器空闲后执行，避免在 Unity 正在导入脚本/模型时重建场景。
            EditorApplication.delayCall += TryAutoRepair;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        // ---------------- 入口 ----------------

        [MenuItem("Tools/Whalebone/Install Harbor Art (占位美化, 幂等)")]
        public static void Install()
        {
            EnsureFolders();
            DeleteLegacyAssets();

            Dictionary<string, Material> mats = CreateMaterials();
            CreateVegetationPrefabs(mats);
            // 修复 106 删除事件后 Prefabs/Placeholder 目录为空导致船/人不可见（2026-08-09 统筹）：
            // 从 FBX 自动生成 Prefab 到 Prefabs/Placeholder/，避免 LoadAssetAtPath 返回 null
            EnsurePrefabFromFbx(ShipFbx, LegacyShipPrefab);
            EnsurePrefabFromFbx(CrewFbx, LegacyCrewPrefab);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildScene(scene, mats);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[HarborArtInstaller] 安装完成: " + ScenePath);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
                EditorApplication.delayCall += TryAutoRepair;
        }

        private static void TryAutoRepair()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                SessionState.GetBool(AutoRepairSessionKey, false) ||
                HasRequiredHarborAssets())
                return;

            SessionState.SetBool(AutoRepairSessionKey, true);
            Debug.LogWarning("[HarborArtInstaller] 检测到港口船只/水手 Prefab 缺失，自动执行一次港口占位资源安装。");
            try
            {
                Install();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[HarborArtInstaller] 自动安装失败：" + ex);
            }
        }

        private static bool HasRequiredHarborAssets()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(LegacyShipPrefab) != null &&
                   AssetDatabase.LoadAssetAtPath<GameObject>(LegacyCrewPrefab) != null &&
                   AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/ph_mat_sea.mat") != null;
        }

        // ---------------- 目录与清理 ----------------

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Art", "Placeholder");
            EnsureFolder("Assets/Art/Placeholder", "Materials");
            EnsureFolder("Assets/Prefabs", "Placeholder");
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent))
            {
                string grand = parent.Substring(0, parent.LastIndexOf('/'));
                string leaf = parent.Substring(parent.LastIndexOf('/') + 1);
                if (grand.Length > 0 && !AssetDatabase.IsValidFolder(grand))
                    EnsureFolder(grand, leaf);
                AssetDatabase.CreateFolder(grand.Length == 0 ? "Assets" : grand, leaf);
            }
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }

        /// <summary>删除旧产物（幂等重建）：旧场景 + 旧几何体 Prefab。只删本任务已知路径。</summary>
        private static void DeleteLegacyAssets()
        {
            string[] legacy = { ScenePath, LegacyShipPrefab, LegacyCrewPrefab };
            foreach (string path in legacy)
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                    AssetDatabase.DeleteAsset(path);
            }
            string[] guids = AssetDatabase.FindAssets("ph_mat_", new[] { MatDir });
            foreach (string g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (p.StartsWith(MatDir)) AssetDatabase.DeleteAsset(p);
            }
            AssetDatabase.SaveAssets();
        }

        // ---------------- 材质 ----------------

        private static Dictionary<string, Material> CreateMaterials()
        {
            var mats = new Dictionary<string, Material>();
            AddLit(mats, "ph_mat_sea", new Color(0.05f, 0.32f, 0.48f, 0.85f), transparent: true);
            AddLit(mats, "ph_mat_sand", new Color(0.82f, 0.74f, 0.58f));
            AddLit(mats, "ph_mat_dock_wood", new Color(0.45f, 0.32f, 0.21f));
            AddLit(mats, "ph_mat_dark_wood", new Color(0.28f, 0.19f, 0.12f));
            AddLit(mats, "ph_mat_boat_hull", new Color(0.36f, 0.24f, 0.15f));
            AddLit(mats, "ph_mat_sail", new Color(0.95f, 0.93f, 0.88f));
            AddLit(mats, "ph_mat_crew_cloth", new Color(0.58f, 0.50f, 0.42f));   // 亚麻色（1675 水手）
            AddLit(mats, "ph_mat_crew_hat", new Color(0.18f, 0.14f, 0.10f));
            AddLit(mats, "ph_mat_palm_trunk", new Color(0.42f, 0.28f, 0.15f));
            AddLit(mats, "ph_mat_palm_leaf", new Color(0.16f, 0.42f, 0.20f));
            AddLit(mats, "ph_mat_rock", new Color(0.52f, 0.50f, 0.48f));
            AddLit(mats, "ph_mat_grass", new Color(0.34f, 0.55f, 0.24f));
            AddLit(mats, "ph_mat_flower", new Color(0.85f, 0.55f, 0.25f));
            return mats;
        }

        private static void AddLit(Dictionary<string, Material> mats, string name, Color color, bool transparent = false)
        {
            string path = MatDir + "/" + name + ".mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader) { name = name, color = color };
            if (shader != null && shader.name.Contains("Lit"))
            {
                mat.SetFloat("_Smoothness", 0.3f);
                if (transparent)
                {
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

        // ---------------- 植被 Prefab（内置几何体降级方案） ----------------

        private static void CreateVegetationPrefabs(Dictionary<string, Material> mats)
        {
            // 椰树：圆柱树干 + 4 片斜插扁 Cube 叶冠
            var palm = new GameObject("ph_palm_tree");
            AddPart(palm.transform, "ph_trunk", PrimitiveType.Cylinder, mats["ph_mat_palm_trunk"],
                new Vector3(0f, 1.5f, 0f), new Vector3(0.16f, 1.5f, 0.16f));
            for (int i = 0; i < 4; i++)
            {
                float ang = i * 90f;
                AddPart(palm.transform, "ph_leaf_" + i, PrimitiveType.Cube, mats["ph_mat_palm_leaf"],
                    new Vector3(Mathf.Sin(ang * Mathf.Deg2Rad) * 0.35f, 3.1f, Mathf.Cos(ang * Mathf.Deg2Rad) * 0.35f),
                    new Vector3(0.06f, 0.7f, 1.6f),
                    new Vector3(55f, ang, 0f));
            }
            PrefabUtility.SaveAsPrefabAsset(palm, PrefabDir + "/ph_palm_tree.prefab");
            Object.DestroyImmediate(palm);

            // 岩石：压扁球体（灰岩色）
            var rock = new GameObject("ph_rock");
            AddPart(rock.transform, "ph_rock_body", PrimitiveType.Sphere, mats["ph_mat_rock"],
                Vector3.zero, new Vector3(0.7f, 0.45f, 0.7f));
            PrefabUtility.SaveAsPrefabAsset(rock, PrefabDir + "/ph_rock.prefab");
            Object.DestroyImmediate(rock);

            // 草丛：3 片小扁叶交叉
            var grass = new GameObject("ph_grass");
            for (int i = 0; i < 3; i++)
            {
                AddPart(grass.transform, "ph_blade_" + i, PrimitiveType.Cube, mats["ph_mat_grass"],
                    new Vector3((i - 1) * 0.12f, 0.12f, 0f),
                    new Vector3(0.05f, 0.24f, 0.10f),
                    new Vector3(20f, i * 45f, 0f));
            }
            PrefabUtility.SaveAsPrefabAsset(grass, PrefabDir + "/ph_grass.prefab");
            Object.DestroyImmediate(grass);

            // 花：茎 + 彩色花头
            var flower = new GameObject("ph_flower");
            AddPart(flower.transform, "ph_stem", PrimitiveType.Cube, mats["ph_mat_grass"],
                new Vector3(0f, 0.08f, 0f), new Vector3(0.04f, 0.16f, 0.04f));
            AddPart(flower.transform, "ph_bloom", PrimitiveType.Sphere, mats["ph_mat_flower"],
                new Vector3(0f, 0.2f, 0f), new Vector3(0.10f, 0.10f, 0.10f));
            PrefabUtility.SaveAsPrefabAsset(flower, PrefabDir + "/ph_flower.prefab");
            Object.DestroyImmediate(flower);
        }

        private static void AddPart(Transform parent, string name, PrimitiveType type, Material mat,
            Vector3 localPos, Vector3 localScale, Vector3? localRotDeg = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (localRotDeg.HasValue) go.transform.localEulerAngles = localRotDeg.Value;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
        }

        // ---------------- 场景重建 ----------------

        private static void BuildScene(Scene scene, Dictionary<string, Material> mats)
        {
            // ---- 海面：半透明蓝 ----
            var sea = GameObject.CreatePrimitive(PrimitiveType.Plane);
            sea.name = "Sea";
            sea.transform.position = new Vector3(0f, -0.02f, 0f);
            sea.transform.localScale = new Vector3(120f, 1f, 120f);
            sea.GetComponent<Renderer>().sharedMaterial = mats["ph_mat_sea"];
            Object.DestroyImmediate(sea.GetComponent<Collider>());

            // ---- 沙滩（陆地） ----
            var beach = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beach.name = "Beach";
            beach.transform.position = new Vector3(0f, -0.32f, -26f);
            beach.transform.localScale = new Vector3(70f, 1.2f, 46f);
            beach.GetComponent<Renderer>().sharedMaterial = mats["ph_mat_sand"];
            Object.DestroyImmediate(beach.GetComponent<Collider>());

            // ---- 码头 ----
            BuildDock(mats);

            // ---- 船（Quaternius FBX） ----
            GameObject ship = InstantiateModel(ShipFbx, "ph_ship_fishing", ShipPos, mats["ph_mat_boat_hull"],
                targetLength: 4.6f, targetHeight: 2.2f);

            // ---- 老水手（Quaternius FBX）站船头，面向大海（+Z） ----
            Vector3 bowPos = ShipPos + new Vector3(0f, 1.1f, 2.25f);
            GameObject crew = InstantiateModel(CrewFbx, "ph_crew_male", bowPos, mats["ph_mat_crew_cloth"],
                targetLength: 0.6f, targetHeight: 1.8f);
            crew.transform.rotation = Quaternion.identity;

            // ---- 植被/岩石沿沙滩边缘 ----
            PlaceVegetation(mats);

            // ---- 灯光/环境/天空 ----
            ConfigureLighting(scene);

            // ---- 相机（第三人称跟随占位） ----
            BuildCamera(ship);

            // ---- 地名 UI ----
            BuildHarborUi();

            // ---- 放置清单（自检/交接用） ----
            int palms = CountInScene("ph_palm_tree");
            int rocks = CountInScene("ph_rock");
            int grass = CountInScene("ph_grass");
            Debug.Log("[HarborArtInstaller] 放置清单: 船=ph_ship_fishing, 老水手=ph_crew_male, " +
                      $"椰树={palms}, 岩石={rocks}, 草丛/花={grass}, 海面材质={sea.GetComponent<Renderer>().sharedMaterial.name}");
        }

        private static void BuildDock(Dictionary<string, Material> mats)
        {
            Material wood = mats["ph_mat_dock_wood"];
            Material dark = mats["ph_mat_dark_wood"];
            var dock = new GameObject("Dock");

            CreateChild(dock.transform, "DockDeck", PrimitiveType.Cube, wood,
                new Vector3(0f, 0.10f, -6f), new Vector3(26f, 0.18f, 3.4f));
            foreach (float x in new[] { -10f, -6f, -2f, 2f, 6f, 10f })
            {
                CreateChild(dock.transform, "Pile", PrimitiveType.Cube, dark,
                    new Vector3(x, -0.71f, -6f), new Vector3(0.24f, 1.8f, 0.24f));
            }
            CreateChild(dock.transform, "HandRail", PrimitiveType.Cube, wood,
                new Vector3(0f, 0.55f, -7.68f), new Vector3(26f, 0.7f, 0.08f));
            foreach (float x in new[] { -6f, 6f })
            {
                CreateChild(dock.transform, "Bollard", PrimitiveType.Cylinder, dark,
                    new Vector3(x, 0.44f, -4.3f), new Vector3(0.3f, 0.5f, 0.3f));
            }
            // 木桶 ×2、货箱 ×2（木色占位）
            CreateChild(dock.transform, "Barrel", PrimitiveType.Cylinder, wood,
                new Vector3(2.4f, 0.48f, -5.0f), new Vector3(0.55f, 0.6f, 0.55f));
            CreateChild(dock.transform, "Barrel2", PrimitiveType.Cylinder, wood,
                new Vector3(1.6f, 0.42f, -5.4f), new Vector3(0.45f, 0.5f, 0.45f));
            CreateChild(dock.transform, "Crate", PrimitiveType.Cube, wood,
                new Vector3(3.4f, 0.35f, -4.6f), new Vector3(0.7f, 0.7f, 0.7f));
            CreateChild(dock.transform, "Crate2", PrimitiveType.Cube, wood,
                new Vector3(4.2f, 0.3f, -5.2f), new Vector3(0.6f, 0.6f, 0.6f));
            // 跳板：码头南缘 → 船甲板
            CreateChild(dock.transform, "Gangplank", PrimitiveType.Cube, wood,
                new Vector3(0f, 0.5f, -4.75f), new Vector3(1.8f, 0.07f, 1.1f));
        }

        private static GameObject CreateChild(Transform parent, string name, PrimitiveType type, Material mat,
            Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            return go;
        }

        /// <summary>从 FBX 生成 Prefab 到 Prefabs/Placeholder/（修复 106 删除后船/人不可见，2026-08-09 统筹）。
        /// 如果 Prefab 已存在 → 跳过；如果 FBX 不存在 → 打 Error 跳过（避免静默失败）。</summary>
        private static void EnsurePrefabFromFbx(string fbxPath, string prefabPath)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

            // 强制导入 FBX（首次打开工程可能没导入完）
            AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);
            var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxAsset == null)
            {
                Debug.LogError("[HarborArtInstaller] EnsurePrefabFromFbx 失败：FBX 资产加载不到 " + fbxPath + "（请手动 Reimport）");
                return;
            }

            // 在临时场景实例化 → 保存为 Prefab → 清理
            var tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject instance = null;
            try
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            }
            finally
            {
                if (instance != null) Object.DestroyImmediate(instance);
                EditorSceneManager.CloseScene(tempScene, true);
            }
            Debug.Log("[HarborArtInstaller] 生成 Prefab: " + prefabPath);
        }

        /// <summary>实例化 FBX 模型：按包围盒缩放到目标尺寸、覆盖占位材质、长轴对齐 +Z（船头朝海）。</summary>
        private static GameObject InstantiateModel(string fbxPath, string instanceName, Vector3 position,
            Material overrideMat, float targetLength, float targetHeight)
        {
            // 优先 Prefab 路径（更稳）；fallback FBX 直接加载
            string prefabPath = fbxPath
                .Replace("/Models/Ships/ph_ship_fishing.fbx", "/Prefabs/Placeholder/ph_ship_fishing.prefab")
                .Replace("/Models/Crew/ph_crew_male.fbx", "/Prefabs/Placeholder/ph_crew_male.prefab")
                .Replace("/Models/Props/", "/Prefabs/Placeholder/");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (prefab == null)
            {
                Debug.LogError("[HarborArtInstaller] 找不到模型资产: " + fbxPath + "（请确认 Unity 已完成素材导入）");
                return new GameObject(instanceName);
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = instanceName;
            go.transform.position = position;

            // 统一覆盖占位材质（避免 fbx 内贴图引用断裂显示粉红）
            foreach (var r in go.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = overrideMat;

            // 按包围盒归一化：最长轴≈targetLength，高度≈targetHeight
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);
                Vector3 size = b.size;
                float len = Mathf.Max(size.x, size.z);
                float scale = Mathf.Min(targetLength / Mathf.Max(len, 0.001f), targetHeight / Mathf.Max(size.y, 0.001f));
                go.transform.localScale = Vector3.one * scale;
                if (size.x > size.z)
                    go.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }
            return go;
        }

        private static void PlaceVegetation(Dictionary<string, Material> mats)
        {
            // 椰树 ×5：沿沙滩边缘
            float[] palmXs = { -15f, -9f, -3f, 3f, 9f };
            float[] palmZs = { -18f, -14f, -20f, -12f, -17f };
            for (int i = 0; i < palmXs.Length; i++)
            {
                var palm = (GameObject)PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/ph_palm_tree.prefab"));
                palm.name = "ph_palm_tree";
                palm.transform.position = new Vector3(palmXs[i], BeachTopY, palmZs[i]);
                palm.transform.rotation = Quaternion.Euler(0f, i * 47f, 0f);
                float s = Random.Range(0.9f, 1.2f);
                palm.transform.localScale = new Vector3(s, s, s);
            }

            // 岩石 ×6：沙滩与海岸线
            for (int i = 0; i < 6; i++)
            {
                var rock = (GameObject)PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/ph_rock.prefab"));
                rock.name = "ph_rock";
                rock.transform.position = new Vector3(Random.Range(-12f, 12f), BeachTopY, Random.Range(-13f, -9f));
                rock.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                float s = Random.Range(0.7f, 1.6f);
                rock.transform.localScale = new Vector3(s, s, s);
            }

            // 草丛 ×8
            for (int i = 0; i < 8; i++)
            {
                var g = (GameObject)PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/ph_grass.prefab"));
                g.name = "ph_grass";
                g.transform.position = new Vector3(Random.Range(-16f, 16f), BeachTopY, Random.Range(-12f, -7f));
                g.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }

            // 花 ×4
            for (int i = 0; i < 4; i++)
            {
                var f = (GameObject)PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/ph_flower.prefab"));
                f.name = "ph_flower";
                f.transform.position = new Vector3(Random.Range(-14f, 14f), BeachTopY, Random.Range(-11f, -8f));
                f.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }
        }

        /// <summary>灯光/环境光调亮 + Procedural Skybox（加勒比晴空，零下载）。</summary>
        private static void ConfigureLighting(Scene scene)
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.95f, 0.85f);
            light.intensity = 1.3f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            lightGo.transform.position = new Vector3(0f, 12f, -4f);

            // 环境光调亮（加勒比午间）
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.60f, 0.68f, 0.85f);
            RenderSettings.ambientEquatorColor = new Color(0.75f, 0.78f, 0.80f);
            RenderSettings.ambientGroundColor = new Color(0.45f, 0.42f, 0.35f);
            RenderSettings.fog = false;

            // Procedural Skybox（URP 兼容的内置 Shader，纯代码创建）
            Shader skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                var sky = new Material(skyShader) { name = "ph_mat_sky_procedural" };
                sky.SetFloat("_AtmosphereThickness", 1.1f);
                sky.SetFloat("_Exposure", 1.2f);
                sky.SetColor("_SkyTint", new Color(0.45f, 0.62f, 0.90f));
                sky.SetColor("_GroundColor", new Color(0.72f, 0.68f, 0.60f));
                AssetDatabase.CreateAsset(sky, MatDir + "/ph_mat_sky_procedural.mat");
                RenderSettings.skybox = sky;
            }
        }

        private static void BuildCamera(GameObject ship)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;   // 使用 Procedural Skybox
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 500f;
            cam.fieldOfView = 60f;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(2f, 5.5f, -8.5f);
            camGo.transform.LookAt(ship.transform.position + Vector3.up * 1.2f);

            // 保留 CameraFollow（target 为空时运行时自动 Find("ph_ship_fishing")）
            var follow = camGo.AddComponent<Whalebone.Gameplay.CameraFollow>();
            follow.Target = ship.transform;
        }

        private static void BuildHarborUi()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);

            var canvasGo = new GameObject("Harbor UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Text title = MakeLabel(canvasGo.transform, "HarborName", -44f, 48, "圣玛格丽特港", font, FontStyle.Bold, Color.white);
            var shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
            shadow.effectDistance = new Vector2(2f, -2f);

            MakeLabel(canvasGo.transform, "HarborSubtitle", -100f, 22, "Sainte-Marguerite · 1675", font, FontStyle.Normal, new Color(0.95f, 0.90f, 0.80f, 0.95f));
            MakeLabel(canvasGo.transform, "HarborControls", -970f, 24,
                "操作：按住鼠标右键或中键拖动调整视角 · 滚轮缩放距离",
                font, FontStyle.Normal, new Color(1f, 1f, 1f, 0.9f));
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
            text.raycastTarget = false;
            if (font != null) text.font = font;
            return text;
        }

        // ---------------- Build Settings ----------------

        /// <summary>幂等追加场景到 Build Settings；若已存在但 guid 过期（场景被重建/删除后 meta 变化）则更新。</summary>
        private static void AddToBuildSettings()
        {
            string guid = AssetDatabase.AssetPathToGUID(ScenePath);
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int idx = scenes.FindIndex(s => s.path == ScenePath);
            if (idx >= 0 && scenes[idx].guid.ToString() == guid) return;   // 已存在且 guid 一致
            if (idx >= 0) scenes.RemoveAt(idx);                             // guid 过期 → 移除重加
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[HarborArtInstaller] Build Settings 已更新: " + ScenePath + " guid=" + guid);
        }

        // ---------------- 辅助 ----------------

        private static int CountInScene(string name)
        {
            var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int n = 0;
            foreach (var t in all)
                if (t.name == name) n++;
            return n;
        }
    }
}
