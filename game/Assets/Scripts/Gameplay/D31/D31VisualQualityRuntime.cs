using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Gameplay.D31
{
    /// <summary>
    /// D31 表现质量档控制器。只修改 Unity 的渲染质量参数和远景裁剪策略；
    /// 不读取/写入存档、船况、天气事实或业务数值。真实 FPS/GC 验收留给后续
    /// 受控构建测试，本组件本身不伪造性能通过。
    /// </summary>
    public sealed class D31VisualQualityRuntime : MonoBehaviour
    {
        public enum VisualQualityTier
        {
            Low = 0,
            Medium = 1,
            High = 2,
        }

        private const string RootName = "D31VisualQualityRuntime";
        private VisualQualityTier _tier = VisualQualityTier.Medium;

        public static VisualQualityTier CurrentTier
        {
            get
            {
                var instance = FindFirstObjectByType<D31VisualQualityRuntime>(FindObjectsInactive.Include);
                return instance == null ? VisualQualityTier.Medium : instance._tier;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D31VisualQualityRuntime>(FindObjectsInactive.Include) != null)
                return;
            new GameObject(RootName).AddComponent<D31VisualQualityRuntime>();
        }

        private void Start()
        {
            ApplyTier(_tier);
        }

        public static bool TryCycleQuality()
        {
            var instance = FindFirstObjectByType<D31VisualQualityRuntime>(FindObjectsInactive.Include);
            if (instance == null) return false;
            instance.ApplyTier((VisualQualityTier)(((int)instance._tier + 1) % 3));
            return true;
        }

        /// <summary>兼容旧快捷键入口；需要结果的调用方使用 TryCycleQuality。</summary>
        public static void CycleQuality()
        {
            TryCycleQuality();
        }

        public static void SetQualityPreview(string value)
        {
            var instance = FindFirstObjectByType<D31VisualQualityRuntime>(FindObjectsInactive.Include);
            if (instance == null) return;
            VisualQualityTier tier;
            if (!Enum.TryParse(value, true, out tier)) tier = VisualQualityTier.Medium;
            instance.ApplyTier(tier);
        }

        public void ApplyTier(VisualQualityTier tier)
        {
            _tier = tier;
            switch (tier)
            {
                case VisualQualityTier.Low:
                    QualitySettings.lodBias = 0.75f;
                    QualitySettings.shadowDistance = 35f;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    QualitySettings.pixelLightCount = 1;
                    break;
                case VisualQualityTier.High:
                    QualitySettings.lodBias = 1.35f;
                    QualitySettings.shadowDistance = 90f;
                    QualitySettings.shadowResolution = ShadowResolution.High;
                    QualitySettings.pixelLightCount = 4;
                    break;
                default:
                    QualitySettings.lodBias = 1f;
                    QualitySettings.shadowDistance = 60f;
                    QualitySettings.shadowResolution = ShadowResolution.Medium;
                    QualitySettings.pixelLightCount = 2;
                    break;
            }
            Debug.Log("[D31/Quality] 表现质量预览=" + tier + "; runtime performance approval remains pending");
        }
    }
}
