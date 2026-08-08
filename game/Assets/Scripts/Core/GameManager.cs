// ============================================================
// GameManager —— 全局游戏状态机与场景切换（P1A 控制中台）
//
// 归属：Core（全局流程控制，见 DEVELOPMENT_STANDARDS §1）
// 职责：
//   1. 单例 + DontDestroyOnLoad（跨场景常驻）
//   2. 游戏状态枚举：MainMenu / NewGame / InGame
//   3. 场景切换封装：LoadMainMenu() / LoadInitialHarbor()
//      （P1C 场景暂不存在时只打日志，不抛异常）
//
// 说明：
//   - 不包含任何 LLM/网络代码（Systems 红线之外，本类为纯流程）
//   - 其他系统通过 GameManager.Instance.State 查询当前状态
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Core
{
    /// <summary>游戏全局状态（供其他系统查询）。</summary>
    public enum GameState
    {
        MainMenu,   // 主菜单
        NewGame,    // 创建新游戏流程中
        InGame      // 游戏中（航行/港口）
    }

    /// <summary>
    /// 全局单例状态机。挂载于"GameManager"常驻对象（DontDestroyOnLoad）。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>当前游戏状态，默认主菜单。</summary>
        public GameState State { get; private set; } = GameState.MainMenu;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>切换到主菜单场景（P1A 场景）。</summary>
        public void LoadMainMenu()
        {
            State = GameState.MainMenu;
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// 切换到初始港口场景（P1C 实现）。场景暂不存在时仅记录日志，不抛异常。
        /// </summary>
        public void LoadInitialHarbor()
        {
            State = GameState.InGame;
            Time.timeScale = 1f;

            // 修复 P1A 在 Editor 下 Application.CanStreamedLevelBeLoaded 假阳性 + LoadScene 抛异常导致流程中断（2026-08-08 统筹）
            try
            {
                SceneManager.LoadScene("InitialHarbor");
            }
            catch (UnityException ex)
            {
                Debug.LogWarning("[GameManager] InitialHarbor 加载失败（不在 Build Settings？sceneCountInBuildSettings="
                    + UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings + "）：" + ex.Message);
            }
        }
    }
}
