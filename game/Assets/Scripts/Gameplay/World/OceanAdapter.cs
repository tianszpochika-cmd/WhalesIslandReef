// ============================================================
// OceanAdapter —— D2 海面适配器
//
// 当前项目未安装 Crest，因此提供稳定的占位海面。未来接入 Crest 时只替换
// 本适配器，不把插件类型带入 Systems 或 AgentCore。
// ============================================================

using UnityEngine;

namespace Whalebone.Gameplay.World
{
    public sealed class OceanAdapter : MonoBehaviour
    {
        [SerializeField] private bool createFallbackIfMissing = true;
        [SerializeField] private float fallbackSize = 120f;
        [SerializeField] private Color fallbackColor = new Color(0.035f, 0.32f, 0.58f, 1f);

        public static OceanAdapter Active { get; private set; }
        public bool FallbackActive { get; private set; }
        public GameObject Surface { get; private set; }
        public float WaterSurfaceY { get; private set; } = -0.05f;
        public float WaterHalfExtent { get; private set; } = 48f;

        private const float MinimumWaterHalfExtent = 48f;

        private void OnEnable()
        {
            Active = this;
        }

        private void OnDisable()
        {
            if (Active == this) Active = null;
        }

        private void Awake()
        {
            EnsureSurface();
        }

        private void EnsureSurface()
        {
            Surface = GameObject.Find("D2_OceanSurface");
            if (Surface == null) Surface = GameObject.Find("Sea");
            if (Surface != null && Surface.GetComponent<Renderer>() != null)
            {
                FallbackActive = Surface.name == "D2_OceanSurface";
                ConfigureWaterBounds();
                return;
            }

            if (!createFallbackIfMissing) return;
            Surface = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Surface.name = "D2_OceanSurface";
            Surface.transform.SetParent(transform, false);
            Surface.transform.position = new Vector3(0f, -0.05f, 0f);
            Surface.transform.localScale = new Vector3(fallbackSize / 10f, 1f, fallbackSize / 10f);
            var renderer = Surface.GetComponent<Renderer>();
            renderer.sharedMaterial = CreateFallbackMaterial();
            FallbackActive = true;
            ConfigureWaterBounds();
            Debug.Log("[D2/Ocean] Crest 未安装，已启用占位海面");
        }

        private void ConfigureWaterBounds()
        {
            if (Surface != null)
            {
                var renderer = Surface.GetComponent<Renderer>();
                if (renderer != null)
                {
                    WaterSurfaceY = Surface.transform.position.y;
                    WaterHalfExtent = Mathf.Max(MinimumWaterHalfExtent,
                        renderer.bounds.extents.x, renderer.bounds.extents.z);
                }
            }

            // 海面是视觉层，不是 CharacterController 的地面。若占位 Plane
            // 留有 MeshCollider，角色会在水下撞到一块看不见的硬地面。
            if (Surface != null)
            {
                foreach (var collider in Surface.GetComponentsInChildren<Collider>(true))
                    if (collider != null) collider.enabled = false;
            }

            Debug.Log("[D2/Ocean] 水域查询已就绪：surfaceY=" + WaterSurfaceY.ToString("F2") +
                ", halfExtent=" + WaterHalfExtent.ToString("F1") +
                ", surfaceCollider=disabled");
        }

        /// <summary>
        /// 只判断水平位置是否落在本适配器拥有的水域内，并返回海面高度。
        /// 是否真正进入游泳态由 D2CharacterController 结合可行走地面射线裁决。
        /// </summary>
        public bool TryGetWaterSurface(Vector3 position, out float surfaceY)
        {
            surfaceY = WaterSurfaceY;
            return Mathf.Abs(position.x) <= WaterHalfExtent &&
                   Mathf.Abs(position.z) <= WaterHalfExtent;
        }

        private Material CreateFallbackMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader) { name = "D2_FallbackOceanMaterial" };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", fallbackColor);
            if (material.HasProperty("_Color")) material.SetColor("_Color", fallbackColor);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.85f);
            return material;
        }
    }
}
