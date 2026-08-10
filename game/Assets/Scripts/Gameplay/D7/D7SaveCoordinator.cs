// ============================================================
// D7SaveCoordinator —— 首航的一键全量保存入口
//
// 表现层只调用各系统公开 SaveProgress；每个系统仍负责自己的快照
// 和数据所有权。此类只负责调用顺序，不复制或裁决数值。
// ============================================================

using UnityEngine;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.D4;
using Whalebone.Gameplay.D5;

namespace Whalebone.Gameplay.D7
{
    public static class D7SaveCoordinator
    {
        public static int SaveFoundationState()
        {
            int count = 0;
            var d2 = Object.FindFirstObjectByType<D2RuntimeBootstrap>(FindObjectsInactive.Include);
            if (d2 != null) { d2.SaveProgress(); count++; }
            var d3 = Object.FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            if (d3 != null) { d3.SaveProgress(); count++; }
            var d4 = Object.FindFirstObjectByType<D4RuntimeBootstrap>(FindObjectsInactive.Include);
            if (d4 != null) { d4.SaveProgress(); count++; }
            return count;
        }

        public static int SaveAllRuntimeState()
        {
            var d5 = Object.FindFirstObjectByType<D5RuntimeBootstrap>(FindObjectsInactive.Include);
            if (d5 != null && d5.IsReady)
            {
                // D5 的保存入口已经先调用 Foundation，避免返回主菜单时重复写入
                // D2/D3/D4 快照；各模块仍只写自己的数据。
                d5.SaveProgress();
                Debug.Log("[D7/Save] 全量运行时状态已请求保存：D5 入口已覆盖 D2-D5");
                return 4;
            }

            int count = SaveFoundationState();
            Debug.Log("[D7/Save] 全量运行时状态已请求保存：模块=" + count);
            return count;
        }
    }
}
