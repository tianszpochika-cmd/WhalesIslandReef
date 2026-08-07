// ============================================================
// SaveSystem —— 存档系统接口桩（P0 工程底座）
//
// ★★★ 本文件是 P1A（调用方）与 P1B（实现方）之间的【唯一契约】 ★★★
// 签名一经 P0 定稿，不得修改；如需扩展必须先行回报统筹者（DEV_ROADMAP D1）。
//
// 归属：Core（存档所有权，见 DEVELOPMENT_STANDARDS §1 与 DESIGN_BOUNDARIES §6）
// 红线：Systems/ 目录禁止 LLM / HttpClient / 网络代码 —— 本文件零外部依赖。
// ============================================================

namespace Whalebone.Systems.Save
{
    /// <summary>
    /// 新游戏初始化参数。
    /// </summary>
    public struct NewGameParams
    {
        /// <summary>船长名（由玩家输入/创建角色界面提供）。</summary>
        public string CaptainName;

        /// <summary>初始船型 ID；调用方未指定时由实现层回退为 "small_fishing"（DATA_MODELS §5）。</summary>
        public string ShipTypeId;
    }

    /// <summary>
    /// 存档系统静态入口（P0 桩版本：可编译、返回默认值）。
    /// 正式实现将基于 SQLite（DATA_MODELS §7 表结构：players / ships / crew_agents）。
    /// </summary>
    public static class SaveSystem
    {
        /// <summary>是否存在可读取的存档（桩：恒为 false）。</summary>
        public static bool HasSave()
        {
            // TODO(P1B)：检查本地存档文件/数据库存在性
            return false;
        }

        /// <summary>初始化新游戏（桩：空实现）。</summary>
        public static void NewGame(NewGameParams p)
        {
            // TODO(P1B)：按 NewGameParams 初始化玩家/船只/初始船员并落库
        }

        /// <summary>保存当前进度（桩：空实现）。</summary>
        public static void Save()
        {
            // TODO(P1B)：序列化 Player/Ship/CrewAgent 快照写入 SQLite
        }

        /// <summary>读取存档（桩：恒为 false，表示无存档可读）。</summary>
        public static bool Load()
        {
            // TODO(P1B)：从 SQLite 恢复 Player/Ship/CrewAgent，成功返回 true
            return false;
        }
    }
}
