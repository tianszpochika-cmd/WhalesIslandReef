// ============================================================
// D19RouteHudManager —— D19 路线反馈管理器
//
// 负责显示 D19RouteHud，接收 D2ControlRouter.Hint、D3 状态和反馈。
// 仅在玩家模式下显示，D5 调试面板默认隐藏。
// 不得由 HUD 改变任何交易、航行、存档或任务事实。
// ============================================================

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.UI;

namespace Whalebone.Gameplay.UI.D19RouteHud
{
    public sealed class D19RouteHudManager : MonoBehaviour
    {
        private D19RouteHud _hud;
        private D2ControlRouter _controlRouter;
        private D2ControlRouter _subscribedRouter;
        private D2InteractionFeedback _lastFeedback;

        private void Awake()
        {
            EnsureEventSystem();
            _hud = FindObjectOfType<D19RouteHud>();
            if (_hud == null)
            {
                var go = new GameObject("D19RouteHudManager", typeof(D19RouteHud));
                _hud = go.GetComponent<D19RouteHud>();
            }
            BindControlRouter(FindObjectOfType<D2ControlRouter>());
        }

        private void EnsureEventSystem()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                var go = new GameObject("EventSystem");
                eventSystem = go.AddComponent<EventSystem>();
            }
            // This manager only subscribes to D2 feedback and updates text.
            // The InitialHarbor input owner is HarborInteractionStabilizer / D29;
            // do not add a second legacy module that can consume WASD navigation.
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "InitialHarbor") return;

            // D2RuntimeBootstrap 在本管理器之后创建控制路由；持续做一次轻量重绑，
            // 避免 Awake 时缓存 null 后永远收不到真实交互反馈。
            if (_controlRouter == null)
                BindControlRouter(FindObjectOfType<D2ControlRouter>());

            UpdateHud();
        }

        private void BindControlRouter(D2ControlRouter router)
        {
            if (ReferenceEquals(_subscribedRouter, router))
            {
                _controlRouter = router;
                return;
            }

            if (_subscribedRouter != null)
                _subscribedRouter.InteractionFeedbackChanged -= OnInteractionFeedbackChanged;

            _controlRouter = router;
            _subscribedRouter = router;
            _lastFeedback = router == null ? null : router.ReadLastInteractionFeedback();
            if (_subscribedRouter != null)
                _subscribedRouter.InteractionFeedbackChanged += OnInteractionFeedbackChanged;
        }

        private void OnInteractionFeedbackChanged(D2InteractionFeedback feedback)
        {
            _lastFeedback = feedback == null ? null : feedback.Clone();
        }

        private void OnDestroy()
        {
            if (_subscribedRouter != null)
                _subscribedRouter.InteractionFeedbackChanged -= OnInteractionFeedbackChanged;
        }

        private void UpdateHud()
        {
            if (_hud == null) return;

            string hint = _controlRouter != null ? _controlRouter.Hint : "WASD 移动，E 交互";
            string status = GameObject.Find("D3Canvas") != null ? "港口贸易与航行可用" : "港口航行已就绪";
            string feedback = GetFeedbackFromEvents();

            _hud.UpdateHud(hint, status, feedback);
        }

        private string GetFeedbackFromEvents()
        {
            // 只显示 D2 路由已经产生的输入结果；HUD 不创建、不修改任何事实。
            if (_lastFeedback == null || _lastFeedback.Sequence <= 0 ||
                string.Equals(_lastFeedback.Code, "router_ready", StringComparison.Ordinal))
                return string.Empty;

            string prefix = _lastFeedback.Succeeded ? "✓ " : "⚠ ";
            return prefix + _lastFeedback.Message;
        }
    }
}
