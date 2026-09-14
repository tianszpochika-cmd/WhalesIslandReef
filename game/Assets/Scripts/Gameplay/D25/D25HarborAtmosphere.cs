using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Gameplay.D25
{
    /// <summary>
    /// InitialHarbor 的表现层大气桥。只写 Unity RenderSettings 和相机表现，
    /// 不读取或裁决天气、航行、存档和实体状态。
    /// </summary>
    public sealed class D25HarborAtmosphere : MonoBehaviour
    {
        private void OnEnable()
        {
            if (SceneManager.GetActiveScene().name != "InitialHarbor") return;
            Apply();
        }

        private void Start()
        {
            Apply();
        }

        public void Apply()
        {
            if (SceneManager.GetActiveScene().name != "InitialHarbor") return;
            D25VisualTheme.ApplyHarborAtmosphere();
            var camera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            D25VisualTheme.ApplyCameraDefaults(camera);
        }
    }
}
