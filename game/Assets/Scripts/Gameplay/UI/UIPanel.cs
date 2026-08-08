// ============================================================
// UIPanel —— 面板切换基类（显示/隐藏 + 可选暂停游戏逻辑）
//
// 归属：Gameplay/UI（表现层，见 DEVELOPMENT_STANDARDS §1）
// 职责：
//   1. 统一面板打开/关闭接口（Show / Hide / Toggle）
//   2. 可选：打开时暂停游戏逻辑（Time.timeScale=0，暂停类面板用）
//   3. 记录当前可见状态供控制器查询
//
// 说明：
//   - 所有 UI 面板继承此类；控制器（MainMenuController 等）只操作
//     Show/Hide，不直接碰 gameObject.SetActive
// ============================================================

using UnityEngine;

namespace Whalebone.Gameplay.UI
{
    /// <summary>
    /// 面板基类。子类挂到面板根节点上。
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        [Tooltip("打开面板时是否暂停游戏逻辑（Time.timeScale=0）")]
        [SerializeField] private bool pauseGameWhenOpen = false;

        /// <summary>当前是否可见。</summary>
        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            // 默认隐藏（避免编辑器里摆放时可见，运行时统一由控制器控制）
            gameObject.SetActive(false);
        }

        /// <summary>打开面板。</summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
            IsVisible = true;
            if (pauseGameWhenOpen) Time.timeScale = 0f;
            OnShow();
        }

        /// <summary>关闭面板。</summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
            IsVisible = false;
            if (pauseGameWhenOpen) Time.timeScale = 1f;
            OnHide();
        }

        /// <summary>切换显示状态。</summary>
        public void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        /// <summary>子类可选：打开后逻辑（如刷新文本）。</summary>
        protected virtual void OnShow() { }

        /// <summary>子类可选：关闭后逻辑。</summary>
        protected virtual void OnHide() { }
    }
}
