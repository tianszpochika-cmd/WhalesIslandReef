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

        public bool FallbackActive { get; private set; }
        public GameObject Surface { get; private set; }

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
            Debug.Log("[D2/Ocean] Crest 未安装，已启用占位海面");
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
