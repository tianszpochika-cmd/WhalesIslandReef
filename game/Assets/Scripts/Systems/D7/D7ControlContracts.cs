// ============================================================
// D7ControlContracts —— 首航体验的控制权与模态输入契约
//
// 这是无 Unity 依赖的纯状态机。表现层只把人物/船/对话/贸易/暂停
// 的当前状态映射进来，不能通过它修改任何航行、经济或船员数值。
// ============================================================

using System;

namespace Whalebone.Systems.D7
{
    public enum D7ControlAuthority
    {
        Character,
        Ship,
        Locked
    }

    public enum D7ModalBlock
    {
        None,
        Dialogue,
        Trade,
        Pause,
        FocusLost,
        SceneTransition
    }

    [Serializable]
    public struct D7ControlSnapshot
    {
        public D7ControlAuthority Authority;
        public D7ModalBlock Modal;

        public bool GameplayInputAllowed => Modal == D7ModalBlock.None;
        public bool CharacterInputAllowed => GameplayInputAllowed && Authority == D7ControlAuthority.Character;
        public bool ShipInputAllowed => GameplayInputAllowed && Authority == D7ControlAuthority.Ship;
        public bool CameraInputAllowed => GameplayInputAllowed;
    }

    /// <summary>集中表达 D7 输入权限，避免各个表现层脚本自行猜测控制权。</summary>
    public sealed class D7ControlStateMachine
    {
        private D7ControlAuthority _authority = D7ControlAuthority.Character;
        private D7ModalBlock _modal = D7ModalBlock.None;

        public D7ControlSnapshot Snapshot => new D7ControlSnapshot
        {
            Authority = _authority,
            Modal = _modal
        };

        public void SetAuthority(D7ControlAuthority authority)
        {
            _authority = authority;
        }

        public void SetModal(D7ModalBlock modal)
        {
            _modal = modal;
        }

        public string AuthorityLabel
        {
            get
            {
                switch (_authority)
                {
                    case D7ControlAuthority.Ship: return "船舶";
                    case D7ControlAuthority.Locked: return "锁定";
                    default: return "人物";
                }
            }
        }

        public string ModalLabel
        {
            get
            {
                switch (_modal)
                {
                    case D7ModalBlock.Dialogue: return "对话";
                    case D7ModalBlock.Trade: return "贸易";
                    case D7ModalBlock.Pause: return "暂停";
                    case D7ModalBlock.FocusLost: return "窗口未聚焦";
                    case D7ModalBlock.SceneTransition: return "场景切换";
                    default: return "无";
                }
            }
        }
    }
}
