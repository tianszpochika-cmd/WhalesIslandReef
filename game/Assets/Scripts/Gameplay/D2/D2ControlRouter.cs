using UnityEngine;
using UnityEngine.InputSystem;
using Whalebone.Agent;
using Whalebone.Gameplay.UI.Dialogue;
using Whalebone.Systems.D2;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D2
{
    public sealed class D2ControlRouter : MonoBehaviour
    {
        [SerializeField] private float shipInteractRange = 5.5f;
        [SerializeField] private float crewInteractRange = 3.5f;

        private GameObject _player;
        private GameObject _ship;
        private GameObject _crew;
        private D2CharacterController _character;
        private D2ShipController _shipController;
        private D2DialogueController _dialogue;
        private CameraFollow _cameraFollow;
        private D2ControlMode _mode = D2ControlMode.Character;
        private D2ControlMode _modeBeforeDialogue = D2ControlMode.Character;

        public D2ControlMode Mode => _mode;
        public string Hint { get; private set; } = "WASD 移动 | 右键/中键调整视角 | 靠近目标按 E | ESC 暂停";

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
            if (_player == null || Time.timeScale <= 0f || Keyboard.current == null) return;
            if (_dialogue != null && _dialogue.IsOpen) return;
            if (!Keyboard.current.eKey.wasPressedThisFrame) return;

            if (_mode == D2ControlMode.Ship)
            {
                ExitShip();
                return;
            }

            if (_crew != null && Vector3.Distance(_player.transform.position, _crew.transform.position) <= crewInteractRange)
            {
                var agent = GameState.CrewAgents.Count == 0 ? null : GameState.CrewAgents[0];
                if (agent != null && _dialogue != null)
                {
                    _dialogue.Open(agent);
                    Hint = "对话中：输入内容后点击发送；关闭对话恢复控制";
                    return;
                }
            }

            if (_ship != null && Vector3.Distance(_player.transform.position, _ship.transform.position) <= shipInteractRange)
                EnterShip();
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
                SetMode(_modeBeforeDialogue);
            }
        }

        private void EnterShip()
        {
            SetMode(D2ControlMode.Ship);
            Hint = "操舵中：W/S 前进或减速，A/D 转向；按 E 离开船舵";
            Debug.Log("[D2/Control] 已进入船只控制");
        }

        private void ExitShip()
        {
            if (_ship != null && _player != null)
            {
                Vector3 away = _ship.transform.forward.sqrMagnitude < 0.01f
                    ? Vector3.back
                    : -_ship.transform.forward;
                _player.transform.position = _ship.transform.position + away * 5f + Vector3.up;
            }
            SetMode(D2ControlMode.Character);
            Hint = "WASD 移动 | 靠近船按 E 操舵 | 靠近老水手按 E 对话 | ESC 暂停";
            Debug.Log("[D2/Control] 已退出船只控制");
        }

        private void SetMode(D2ControlMode mode)
        {
            _mode = mode;
            if (_character != null) _character.SetInputEnabled(mode == D2ControlMode.Character);
            if (_shipController != null) _shipController.SetControlMode(mode);
            if (_cameraFollow != null)
                _cameraFollow.Target = mode == D2ControlMode.Ship && _ship != null
                    ? _ship.transform
                    : _player == null ? null : _player.transform;
        }
    }
}
