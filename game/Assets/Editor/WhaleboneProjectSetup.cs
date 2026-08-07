// ============================================================
// WhaleboneProjectSetup —— 项目首开自动配置（幂等，可重复执行）
//
// 职责（P0 工程底座 · D1.1）：
//   1. 启用新版 Input System（activeInputHandler = 1）
//   2. 设置产品名/公司名（ProjectSettings.asset 由 Unity 生成，
//      所有定制统一走本脚本，避免手写 YAML 漂移）
//   3. 首次检测到 URP 包已就绪时，自动创建 URP 管线资产并挂到
//      GraphicsSettings（URP 模板等价物）
//   4. 构建目标锁定 Windows Standalone（默认即此，防御性设置）
//
// 说明：
//   - 仅编辑器代码，位于 Assets/Editor/，不进入运行时程序集
//   - 不写 ProjectSettings.asset 的原因：手写 YAML 在版本间易漂移，
//     编辑器脚本可审计、幂等、可手动重跑（Tools > Whalebone > Setup Project）
// ============================================================

using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Whalebone.EditorTools
{
    [InitializeOnLoad]
    public static class WhaleboneProjectSetup
    {
        // 每次编辑器启动（编译完成后）执行一次；失败/未就绪则跳过，下次启动重试
        static WhaleboneProjectSetup()
        {
            EditorApplication.delayCall += RunOnce;
        }

        [MenuItem("Tools/Whalebone/Setup Project (幂等)")]
        public static void RunOnce()
        {
            // ---- 1. 启用新版 Input System（0=旧版 1=新版 2=两者）----
            if (PlayerSettings.activeInputHandler != ActiveInputHandler.InputSystemPlayer)
            {
                PlayerSettings.activeInputHandler = ActiveInputHandler.InputSystemPlayer;
                Debug.Log("[WhaleboneSetup] Input System 已启用（activeInputHandler=1）");
            }

            // ---- 2. 产品信息 ----
            PlayerSettings.companyName = "Whalebone Studio";
            PlayerSettings.productName = "Whalebone";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;

            // ---- 3. 构建目标：Windows Standalone（防御性）----
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                // 不做强制切换（首次导入可能不稳定），仅记录期望值
                Debug.Log("[WhaleboneSetup] 期望构建目标 StandaloneWindows64，当前: " +
                          EditorUserBuildSettings.activeBuildTarget);
            }

            // ---- 4. URP 管线自动创建（URP 包就绪后生效）----
            TrySetupUrp();

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 若 GraphicsSettings 尚未挂 URP 资产，且 URP 包已可解析，则自动创建。
        /// 幂等：已配置时直接返回。
        /// </summary>
        private static void TrySetupUrp()
        {
            if (GraphicsSettings.currentRenderPipeline != null)
                return; // 已配置完成

            // 检查 URP 包是否已导入（通过类型存在性判断，避免硬编码包版本）
            System.Type assetType = System.Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
            System.Type rendererType = System.Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalRendererData, Unity.RenderPipelines.Universal.Runtime");
            if (assetType == null || rendererType == null)
            {
                Debug.Log("[WhaleboneSetup] URP 包尚未就绪，跳过管线配置（包导入后重启编辑器自动完成）");
                return;
            }

            // 查找/创建 URP 资产目录
            const string urpDir = "Assets/Settings/URP";
            if (!AssetDatabase.IsValidFolder(urpDir))
            {
                AssetDatabase.CreateFolder("Assets/Settings", "URP");
            }

            // 创建 Renderer Data（URP 2D/3D 渲染器）
            string rendererPath = urpDir + "/WhaleboneRenderer.asset";
            var rendererData = AssetDatabase.LoadAssetAtPath(rendererPath, rendererType);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance(rendererType);
                AssetDatabase.CreateAsset(rendererData, rendererPath);
            }

            // 创建 Render Pipeline Asset
            string pipelinePath = urpDir + "/WhaleboneURP.asset";
            var pipelineAsset = AssetDatabase.LoadAssetAtPath(pipelinePath, assetType);
            if (pipelineAsset == null)
            {
                pipelineAsset = ScriptableObject.CreateInstance(assetType);
                AssetDatabase.CreateAsset(pipelineAsset, pipelinePath);
            }

            // 把 RendererData 挂到 RenderPipelineAsset（反射调用 SetRenderer）
            var setRenderer = assetType.GetMethod(
                "SetRenderer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (setRenderer != null && rendererData != null)
            {
                setRenderer.Invoke(pipelineAsset, new object[] { 0, rendererData });
            }

            // 挂到 GraphicsSettings（URP 管线）
            GraphicsSettings.defaultRenderPipeline = (RenderPipelineAsset)pipelineAsset;
            GraphicsSettings.renderPipelineAsset = (RenderPipelineAsset)pipelineAsset;

            Debug.Log("[WhaleboneSetup] URP 管线已创建并启用: " + pipelinePath);
        }
    }
}
