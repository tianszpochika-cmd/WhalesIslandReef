using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Whalebone.Gameplay.UI;

namespace Whalebone.Gameplay.D25
{
    /// <summary>
    /// D25 视觉表现层的共享主题。它只提供颜色与安全的相机/大气默认值，
    /// 不保存或修改任何 D18-D24 领域事实。
    /// </summary>
    public static class D25VisualTheme
    {
        public static readonly Color DeepSea = new Color32(30, 86, 97, 255);
        public static readonly Color ShallowSea = new Color32(47, 139, 141, 255);
        public static readonly Color Foam = new Color32(215, 228, 210, 255);
        public static readonly Color Sand = new Color32(200, 176, 122, 255);
        public static readonly Color WetSand = new Color32(127, 113, 95, 255);
        public static readonly Color Wood = new Color32(121, 90, 60, 255);
        public static readonly Color Foliage = new Color32(62, 107, 72, 255);
        public static readonly Color Terracotta = new Color32(139, 78, 52, 255);
        public static readonly Color Accent = new Color32(226, 166, 84, 255);
        public static readonly Color Warning = new Color32(217, 154, 43, 255);
        public static readonly Color Ink = new Color32(18, 29, 37, 242);
        public static readonly Color Paper = new Color32(244, 240, 218, 255);

        /// <summary>
        /// 只补齐当前港口相机缺少的安全默认值。若场景已有天空盒，保留天空盒，
        /// 不强行覆盖美术师已经设置的投影和视角。
        /// </summary>
        public static void ApplyCameraDefaults(Camera camera)
        {
            if (camera == null) return;
            if (camera.farClipPlane < 300f) camera.farClipPlane = 420f;
            if (RenderSettings.skybox == null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.42f, 0.67f, 0.70f, 1f);
            }
        }

        /// <summary>建立可读的海岛雾和环境光，表现层不依赖运行时逻辑状态。</summary>
        public static void ApplyHarborAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.43f, 0.64f, 0.62f, 1f);
            RenderSettings.fogStartDistance = 58f;
            RenderSettings.fogEndDistance = 260f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.50f, 0.58f, 0.51f, 1f);
            RenderSettings.reflectionIntensity = 0.65f;

            // The legacy procedural skybox used a brown ground horizon. In
            // the harbor Player that read as an unfinished void behind the
            // island. Tune only the existing skybox presentation properties;
            // do not replace the asset or touch any gameplay/weather facts.
            var skybox = RenderSettings.skybox;
            if (skybox != null)
            {
                if (skybox.HasProperty("_GroundColor"))
                    skybox.SetColor("_GroundColor", new Color(0.16f, 0.30f, 0.34f, 1f));
                if (skybox.HasProperty("_SkyTint"))
                    skybox.SetColor("_SkyTint", new Color(0.30f, 0.58f, 0.80f, 1f));
                if (skybox.HasProperty("_Exposure"))
                    skybox.SetFloat("_Exposure", 1.0f);
            }
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        /// <summary>
        /// 统一初始化运行时 UGUI 文本。D25-D31 的面板都是运行时创建的，
        /// 因此不能依赖编辑器序列化默认值；这里显式刷新字体、可见性和
        /// CanvasRenderer alpha，避免“面板有了但文字空白”的半初始化状态。
        /// </summary>
        public static Text ConfigureText(Text text, string content, int size,
            TextAnchor alignment, Color color)
        {
            if (text == null) return null;

            text.font = Fonts.ResolveForText(content, Fonts.Default);
            text.fontSize = Mathf.Max(1, size);
            text.alignment = alignment;
            text.color = color;
            text.text = content ?? string.Empty;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.maskable = true;
            text.enabled = true;
            text.gameObject.SetActive(true);

            // Graphic.color 与 CanvasRenderer 的 alpha 在运行时重建时可能暂时
            // 不同步；显式设置一次，确保首帧就能生成文字网格。
            text.canvasRenderer.SetAlpha(Mathf.Clamp01(color.a));
            return text;
        }

        /// <summary>输出一次性 UI 结构诊断，供 D31 代码层/运行层证据复用。</summary>
        public static void LogTextDiagnostics(string scope, Transform root)
        {
            if (root == null) return;
            var texts = root.GetComponentsInChildren<Text>(true);
            var enabled = 0;
            var fonts = 0;
            var nonEmpty = 0;
            for (var i = 0; i < texts.Length; i++)
            {
                var text = texts[i];
                if (text == null) continue;
                if (text.isActiveAndEnabled) enabled++;
                if (text.font != null) fonts++;
                if (!string.IsNullOrEmpty(text.text)) nonEmpty++;
            }
            Debug.Log("[D25/UI] " + scope + " text diagnostics: total=" + texts.Length +
                ", enabled=" + enabled + ", fonts=" + fonts + ", nonEmpty=" + nonEmpty);
        }
    }
}
