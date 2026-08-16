using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Whalebone.Agent;
using Whalebone.Gameplay.Harbor;
using Whalebone.Gameplay.UI.Dialogue;
using Whalebone.Gameplay.D7;
using Whalebone.Systems.D2;
using Whalebone.Systems.D16;
using Whalebone.Systems.Save;
using Whalebone.Gameplay.UI.D16;

namespace Whalebone.Gameplay.D2
{
    /// <summary>
    /// D2 交互公开反馈。它只描述输入尝试和当前模式，不拥有 NPC、船、存档或任务事实。
    /// </summary>
    [Serializable]
    public sealed class D2InteractionFeedback
    {
        public string Code { get; private set; }
        public string Message { get; private set; }
        public string TargetStableId { get; private set; }
        public bool Succeeded { get; private set; }
        public string Mode { get; private set; }
        public int Sequence { get; private set; }

        public D2InteractionFeedback(string code, string message, string targetStableId,
            bool succeeded, string mode, int sequence)
        {
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            TargetStableId = targetStableId ?? string.Empty;
            Succeeded = succeeded;
            Mode = mode ?? string.Empty;
            Sequence = sequence;
        }

        public D2InteractionFeedback Clone()
        {
            return new D2InteractionFeedback(Code, Message, TargetStableId, Succeeded, Mode, Sequence);
        }
    }

    public sealed class D2ControlRouter : MonoBehaviour
    {
        [SerializeField] private float shipInteractRange = 8f;
        [SerializeField] private float crewInteractRange = 3.5f;

        private GameObject _player;
        private GameObject _ship;
        private GameObject _crew;
        private D2CharacterController _character;
        private D2ShipController _shipController;
        private D2DialogueController _dialogue;
        private CameraFollow _cameraFollow;
        private D7ShipDeckController _shipDeck;
        private D2ControlMode _mode = D2ControlMode.Character;
        private D2ControlMode _modeBeforeDialogue = D2ControlMode.Character;
        private float _dialogueExitGraceUntil;
        private int _interactionSequence;

        public D2ControlMode Mode => _mode;
        public string Hint { get; private set; } = "WASD 移动 | Shift 奔跑 | Space 跳跃 | 右键/中键调整视角 | F3 切换视角 | 靠近船按 E 上船/下船 | ESC 暂停";
        public D2InteractionFeedback LastInteractionFeedback { get; private set; } =
            new D2InteractionFeedback("router_ready", "D2 交互路由已就绪", string.Empty, true, D2ControlMode.Character.ToString(), 0);
        public event Action<D2InteractionFeedback> InteractionFeedbackChanged;

        public void Initialize(GameObject player, GameObject ship, GameObject crew,
            D2CharacterController character, D2ShipController shipController,
            D2DialogueController dialogue, Camera camera)
        {
            _player = player;
            _ship = ship;
            _crew = crew;
            _character = character;
            _shipController = shipController;
            _dialogue = dialogue;
            _cameraFollow = camera == null ? null : camera.GetComponent<CameraFollow>();
            _dialogue.ControlLockChanged -= OnDialogueLockChanged;
            _dialogue.ControlLockChanged += OnDialogueLockChanged;
            SetMode(D2ControlMode.Character);
        }

        private void Update()
        {
            if (_player == null || Time.timeScale <= 0f || !D7ControlCoordinator.GameplayInputAllowed) return;
            if (_dialogue != null && _dialogue.IsOpen) return;
            if (D16InputRouter.WasPressedThisFrame(D16InputAction.CameraCycle) && _cameraFollow != null)
            {
                _cameraFollow.CycleViewMode();
                Hint = "当前视角：" + _cameraFollow.ViewModeLabel + " | F3 切换视角 | 右键/中键拖动调整方向";
                return;
            }
            bool interactPressed = D16InputRouter.WasPressedThisFrame(D16InputAction.Interact);
            // 某些独立 Player 输入焦点组合下，D16 的可重绑定读取可能错过
            // 键盘边沿；仅对默认 E 入口读取同一 Input System 边沿作为兼容兜底，
            // 仍然交给下方的 NPC/船逻辑裁决，不创建第二个交互入口。
            if (!interactPressed &&
                ((Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
                 Input.GetKeyDown(KeyCode.E)))
            {
                interactPressed = true;
                Debug.Log("[D2/Input] 通过兼容 E 边沿恢复交互入口");
            }
            if (!interactPressed) return;

            // 老水手与船在初始港口相距很近；键盘 E 先处理 NPC 对话，
            // HUD 的上船按钮仍只调用 TryToggleShip，避免两个入口互相抢事件。
            // 刚关闭对话时允许一次 E 直接上船，避免玩家必须离开交互区才能继续引导。
            if (Time.unscaledTime < _dialogueExitGraceUntil)
            {
                TryToggleShip();
                return;
            }
            // 当玩家已经处在船的登船范围内时，船入口优先于相邻 NPC。
            // 这样港口出生/泊位区域的 E 键不会被附近对话目标永久抢占；
            // 离开船的范围后仍保留 NPC 对话入口。
            if (IsShipWithinBoardingRange())
            {
                TryToggleShip();
                return;
            }
            if (TryOpenCrewDialogue()) return;
            TryToggleShip();
        }

        private bool IsShipWithinBoardingRange()
        {
            if (_player == null || _ship == null) return false;
            if (_mode == D2ControlMode.Ship) return true;

            var deck = _ship.GetComponent<D7ShipDeckController>();
            return deck != null && deck.IsReady
                ? deck.IsWithinBoardingRange(_player.transform, shipInteractRange)
                : HorizontalDistance(_player.transform.position, _ship.transform.position) <= shipInteractRange;
        }

        /// <summary>
        /// 允许 D19 HUD/测试读取最近一次交互结果；不执行任何领域裁决或存档写入。
        /// </summary>
        public D2InteractionFeedback ReadLastInteractionFeedback()
        {
            return LastInteractionFeedback == null ? null : LastInteractionFeedback.Clone();
        }

        /// <summary>
        /// 统一处理键盘和 HUD 按钮的上船入口，返回值表示本次是否完成了切换。
        /// </summary>
        public bool TryToggleShip()
        {
            if (_player == null || _ship == null)
            {
                SetInteractionFeedback("ship_target_missing", "当前没有可用船只目标", StableId(_ship), false);
                return false;
            }
            if (_mode == D2ControlMode.Locked)
            {
                SetInteractionFeedback("interaction_mode_locked", "当前模式不允许登船或下船", StableId(_ship), false);
                return false;
            }
            if (!D7ControlCoordinator.GameplayInputAllowed)
            {
                SetInteractionFeedback("gameplay_input_blocked", "当前输入被暂停或模态界面占用", StableId(_ship), false);
                return false;
            }
            if (_dialogue != null && _dialogue.IsOpen)
            {
                SetInteractionFeedback("dialogue_open", "请先关闭对话再进行登船操作", StableId(_ship), false);
                return false;
            }

            if (_mode == D2ControlMode.Ship)
                return ExitShip();

            _shipDeck = _ship.GetComponent<D7ShipDeckController>();
            bool inRange = _shipDeck != null && _shipDeck.IsReady
                ? _shipDeck.IsWithinBoardingRange(_player.transform, shipInteractRange)
                : HorizontalDistance(_player.transform.position, _ship.transform.position) <= shipInteractRange;
            if (inRange)
            {
                EnterShip();
                SetInteractionFeedback("boarded", "已上船：可以操控船只；按 E 下船", StableId(_ship), true);
                return true;
            }

            // 玩家经常只看到船但不知道交互距离；把失败原因直接显示在底部提示。
            SetInteractionFeedback("ship_too_far", "请先靠近码头上的船，再按 E 上船", StableId(_ship), false);
            return false;
        }

        public bool TryOpenCrewDialogue()
        {
            if (_player == null || _crew == null)
            {
                SetInteractionFeedback("npc_target_missing", "当前没有可用 NPC 目标", StableId(_crew), false);
                return false;
            }
            if (_dialogue == null)
            {
                SetInteractionFeedback("npc_dialogue_unavailable", "对话入口暂不可用", StableId(_crew), false);
                return false;
            }
            if (HorizontalDistance(_player.transform.position, _crew.transform.position) > crewInteractRange)
            {
                SetInteractionFeedback("npc_too_far", "请先靠近老水手，再按 E 对话", StableId(_crew), false);
                return false;
            }

            var agent = GameState.CrewAgents.Count == 0 ? null : GameState.CrewAgents[0];
            if (agent == null)
            {
                SetInteractionFeedback("npc_state_missing", "NPC 当前没有可用船员事实", StableId(_crew), false);
                return false;
            }
            _dialogue.Open(agent);
            SetInteractionFeedback("npc_dialogue_open", "已开始对话：输入内容后发送，关闭对话恢复控制", StableId(_crew), true);
            return true;
        }

        public void OnDialogueLockChanged(bool locked)
        {
            if (locked)
            {
                _modeBeforeDialogue = _mode == D2ControlMode.Locked ? D2ControlMode.Character : _mode;
                SetMode(D2ControlMode.Locked);
            }
            else
            {
                _dialogueExitGraceUntil = Time.unscaledTime + 1f;
                SetMode(_modeBeforeDialogue);
                SetInteractionFeedback("dialogue_closed", "对话已关闭，控制已恢复", StableId(_crew), true);
            }
        }

        private void EnterShip()
        {
            // 让玩家模型真正站到船上，并作为船的子物体跟随航行变换。
            if (_character != null) _character.SetCollisionEnabled(false);
            _player.transform.SetParent(_ship.transform, true);
            Vector3 deckPosition = _shipDeck != null && _shipDeck.HelmPoint != null
                ? _shipDeck.HelmPoint.position
                : _ship.transform.position + _ship.transform.up * 1.15f +
                  _ship.transform.forward * 0.25f;
            if (_character != null) _character.SetWorldPosition(deckPosition);
            else _player.transform.position = deckPosition;
            _player.transform.rotation = _ship.transform.rotation;
            SetMode(D2ControlMode.Ship);
            if (_character != null) _character.PlayTransition(D2CharacterAction.Boarding);
            Hint = "已上船：W/S 前进或减速，A/D 转向；当前视角：" + CurrentViewLabel() + "；F3 切换视角；按 E 下船";
            Debug.Log("[D2/Control] 已进入船只控制");
        }

        private bool ExitShip()
        {
            if (_ship == null || _player == null)
                return false;

            // 下船前先解除父子关系并关闭 CharacterController；这样候选地面检测
            // 不会把玩家自己的碰撞体或船体碰撞体当成可行走地面。
            if (_character != null) _character.SetCollisionEnabled(false);
            _player.transform.SetParent(null, true);

            Vector3 exitPosition;
            if (!TryFindSafeDisembarkPosition(out exitPosition))
            {
                _player.transform.SetParent(_ship.transform, true);
                SetInteractionFeedback("disembark_unavailable", "船已离开可下船区域，请靠岸后再按 E 下船", StableId(_ship), false);
                Debug.LogWarning("[D2/Control] 下船被阻止：附近没有可行走的沙滩、码头或跳板");
                return false;
            }

            if (_character != null)
            {
                _character.SetWorldPosition(exitPosition);
                _character.SetCollisionEnabled(true);
            }
            else
            {
                _player.transform.position = exitPosition;
            }

            SetMode(D2ControlMode.Character);
            if (_character != null) _character.PlayTransition(D2CharacterAction.Disembarking);
            SetInteractionFeedback("disembarked", "已下船：WASD 移动，靠近老水手按 E 对话", StableId(_ship), true);
            Debug.Log("[D2/Control] 已退出船只控制");
            return true;
        }

        private bool TryFindSafeDisembarkPosition(out Vector3 position)
        {
            position = Vector3.zero;
            if (_ship == null) return false;

            Vector3 forward = _ship.transform.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude < 0.001f ? Vector3.forward : forward.normalized;
            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            Vector3 origin = _ship.transform.position;
            Vector3[] candidates =
            {
                origin - forward * 4.5f,
                origin + forward * 4.5f,
                origin - right * 4.5f,
                origin + right * 4.5f,
                origin - forward * 7f,
                origin + forward * 7f,
                origin,
            };

            float nearest = float.PositiveInfinity;
            bool found = false;
            for (int i = 0; i < candidates.Length; i++)
            {
                RaycastHit[] hits = Physics.RaycastAll(
                    candidates[i] + Vector3.up * 8f,
                    Vector3.down,
                    16f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);

                for (int j = 0; j < hits.Length; j++)
                {
                    var hit = hits[j];
                    if (!IsSafeDisembarkSurface(hit.collider))
                        continue;

                    float distance = Vector3.SqrMagnitude(hit.point - origin);
                    if (distance >= nearest) continue;

                    nearest = distance;
                    position = hit.point + Vector3.up * 1.1f;
                    found = true;
                }
            }

            return found;
        }

        private bool IsSafeDisembarkSurface(Collider collider)
        {
            if (collider == null || _ship == null) return false;
            if (collider.transform == _ship.transform ||
                collider.transform.IsChildOf(_ship.transform))
                return false;
            if (HarborCollisionController.IsNavigationBoundary(collider))
                return false;

            // 装饰道具可以参与阻挡，但不能成为下船目标；只接受运行时
            // 明确声明的行走面，避免把人物放到桶、木桩或船舷顶上。
            return collider.name == "BeachGround" ||
                   collider.name == "DockGround" ||
                   collider.name == "GangplankGround" ||
                   collider.name == "D19_TerrainRoot" ||
                   collider.name == "D19_R02_TerrainSurface" ||
                   collider.name == "D19_R02_WalkableTerrain";
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private void SetMode(D2ControlMode mode)
        {
            _mode = mode;
            if (_ship != null)
            {
                var shipDeck = _ship.GetComponent<D7ShipDeckController>();
                if (shipDeck != null) shipDeck.RecordControlTransition(mode);
            }
            if (_character != null) _character.SetInputEnabled(mode == D2ControlMode.Character);
            if (_shipController != null) _shipController.SetControlMode(mode);
            if (_cameraFollow != null)
            {
                _cameraFollow.SetTargets(
                    _player == null ? null : _player.transform,
                    _ship == null ? null : _ship.transform);
                _cameraFollow.SetControlMode(mode == D2ControlMode.Ship);
            }
        }

        private string CurrentViewLabel()
        {
            return _cameraFollow == null ? "人物视角" : _cameraFollow.ViewModeLabel;
        }

        private void SetInteractionFeedback(string code, string message, string targetStableId, bool succeeded)
        {
            Hint = message ?? string.Empty;
            _interactionSequence++;
            LastInteractionFeedback = new D2InteractionFeedback(
                code,
                Hint,
                targetStableId,
                succeeded,
                _mode.ToString(),
                _interactionSequence);
            var changed = InteractionFeedbackChanged;
            if (changed != null) changed(LastInteractionFeedback.Clone());
        }

        private static string StableId(GameObject target)
        {
            return target == null ? string.Empty : target.name;
        }
    }
}
