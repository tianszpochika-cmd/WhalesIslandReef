using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using Whalebone.Systems.D16;

namespace Whalebone.Gameplay.UI.D16
{
    /// <summary>
    /// D16-A03 输入意图桥接：键盘和手柄都映射到同一动作，支持 PlayerPrefs 重绑定。
    /// 该类只读输入，不直接改变玩法状态。
    /// </summary>
    public static class D16InputRouter
    {
        private static bool _gameplayInputBlocked;
        private static int _triggerSampleFrame = -1;
        private static int _triggerDeviceId = -1;
        private static bool _leftTriggerPressed;
        private static bool _rightTriggerPressed;
        private static bool _leftTriggerPressedThisFrame;
        private static bool _rightTriggerPressedThisFrame;

        public static bool GameplayInputBlocked => _gameplayInputBlocked;

        /// <summary>由 D16 模态设置面板切断玩法输入，但不切断 EventSystem 导航。</summary>
        public static void SetGameplayInputBlocked(bool blocked)
        {
            _gameplayInputBlocked = blocked;
        }

        public static bool IsPressed(D16InputAction action)
        {
            if (_gameplayInputBlocked) return false;
            var binding = D16AccessibilitySettingsStore.GetBinding(action);
            return binding != null &&
                   (ReadKeyboard(binding.PrimaryKeyboard, false) ||
                    ReadKeyboard(binding.SecondaryKeyboard, false) ||
                    ReadGamepad(binding.GamepadButton, false));
        }

        public static bool WasPressedThisFrame(D16InputAction action)
        {
            if (_gameplayInputBlocked) return false;
            var binding = D16AccessibilitySettingsStore.GetBinding(action);
            return binding != null &&
                   (ReadKeyboard(binding.PrimaryKeyboard, true) ||
                    ReadKeyboard(binding.SecondaryKeyboard, true) ||
                    ReadGamepad(binding.GamepadButton, true));
        }

        public static UnityEngine.Vector2 MoveAxis()
        {
            var x = (IsPressed(D16InputAction.MoveRight) ? 1f : 0f) -
                    (IsPressed(D16InputAction.MoveLeft) ? 1f : 0f);
            var y = (IsPressed(D16InputAction.MoveForward) ? 1f : 0f) -
                    (IsPressed(D16InputAction.MoveBack) ? 1f : 0f);
            var axis = new UnityEngine.Vector2(x, y);
            if (!_gameplayInputBlocked && Gamepad.current != null)
            {
                var stick = Gamepad.current.leftStick.ReadValue();
                if (stick.sqrMagnitude > axis.sqrMagnitude)
                    axis = stick;
            }
            return axis.sqrMagnitude > 1f ? axis.normalized : axis;
        }

        public static bool TryGetPressedKeyboardKey(out string keyName)
        {
            keyName = string.Empty;
            if (Keyboard.current == null) return false;

            foreach (Key key in System.Enum.GetValues(typeof(Key)))
            {
                if (key == Key.None) continue;
                var control = Keyboard.current[key];
                if (control != null && control.wasPressedThisFrame)
                {
                    keyName = key.ToString();
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetPressedGamepadButton(out string buttonName)
        {
            buttonName = string.Empty;
            var pad = Gamepad.current;
            if (pad == null) return false;

            if (pad.buttonSouth.wasPressedThisFrame) { buttonName = "South"; return true; }
            if (pad.buttonWest.wasPressedThisFrame) { buttonName = "West"; return true; }
            if (pad.buttonNorth.wasPressedThisFrame) { buttonName = "North"; return true; }
            if (pad.buttonEast.wasPressedThisFrame) { buttonName = "East"; return true; }
            if (pad.startButton.wasPressedThisFrame) { buttonName = "Start"; return true; }
            if (pad.selectButton.wasPressedThisFrame) { buttonName = "Select"; return true; }
            if (pad.leftShoulder.wasPressedThisFrame) { buttonName = "LeftShoulder"; return true; }
            if (pad.rightShoulder.wasPressedThisFrame) { buttonName = "RightShoulder"; return true; }
            if (pad.dpad.up.wasPressedThisFrame) { buttonName = "DpadUp"; return true; }
            if (pad.dpad.down.wasPressedThisFrame) { buttonName = "DpadDown"; return true; }
            if (pad.dpad.left.wasPressedThisFrame) { buttonName = "DpadLeft"; return true; }
            if (pad.dpad.right.wasPressedThisFrame) { buttonName = "DpadRight"; return true; }
            if (pad.leftStickButton.wasPressedThisFrame) { buttonName = "LeftStickPress"; return true; }
            if (pad.rightStickButton.wasPressedThisFrame) { buttonName = "RightStickPress"; return true; }
            RefreshTriggerState(pad);
            if (_leftTriggerPressedThisFrame) { buttonName = "LeftTrigger"; return true; }
            if (_rightTriggerPressedThisFrame) { buttonName = "RightTrigger"; return true; }
            return false;
        }

        private static bool ReadKeyboard(string name, bool pressedThisFrame)
        {
            if (string.IsNullOrWhiteSpace(name) || Keyboard.current == null) return false;
            Key key;
            if (!System.Enum.TryParse(name.Trim(), true, out key)) return false;
            var control = Keyboard.current[key];
            return control != null && (pressedThisFrame ? control.wasPressedThisFrame : control.isPressed);
        }

        private static bool ReadGamepad(string name, bool pressedThisFrame)
        {
            if (string.IsNullOrWhiteSpace(name) || Gamepad.current == null) return false;
            var control = FindGamepadControl(Gamepad.current, name.Trim());
            if (control == null) return false;
            if (control is ButtonControl button)
                return pressedThisFrame ? button.wasPressedThisFrame : button.isPressed;

            // Triggers are AxisControl rather than ButtonControl. Sample their edge
            // once per Unity frame so a one-shot action does not repeat while held.
            RefreshTriggerState(Gamepad.current);
            if (!pressedThisFrame) return control.IsPressed();
            return name.Trim() == "LeftTrigger"
                ? _leftTriggerPressedThisFrame
                : _rightTriggerPressedThisFrame;
        }

        private static void RefreshTriggerState(Gamepad pad)
        {
            if (pad == null) return;
            int frame = Time.frameCount;
            if (_triggerSampleFrame == frame && _triggerDeviceId == pad.deviceId) return;

            bool previousLeft = _triggerDeviceId == pad.deviceId && _triggerSampleFrame >= 0
                ? _leftTriggerPressed : false;
            bool previousRight = _triggerDeviceId == pad.deviceId && _triggerSampleFrame >= 0
                ? _rightTriggerPressed : false;
            _leftTriggerPressed = pad.leftTrigger.IsPressed();
            _rightTriggerPressed = pad.rightTrigger.IsPressed();
            _leftTriggerPressedThisFrame = _leftTriggerPressed && !previousLeft;
            _rightTriggerPressedThisFrame = _rightTriggerPressed && !previousRight;
            _triggerSampleFrame = frame;
            _triggerDeviceId = pad.deviceId;
        }

        private static UnityEngine.InputSystem.InputControl FindGamepadControl(Gamepad pad, string name)
        {
            switch (name)
            {
                case "South": return pad.buttonSouth;
                case "West": return pad.buttonWest;
                case "North": return pad.buttonNorth;
                case "East": return pad.buttonEast;
                case "Start": return pad.startButton;
                case "Select": return pad.selectButton;
                case "LeftShoulder": return pad.leftShoulder;
                case "RightShoulder": return pad.rightShoulder;
                case "DpadUp": return pad.dpad.up;
                case "DpadDown": return pad.dpad.down;
                case "DpadLeft": return pad.dpad.left;
                case "DpadRight": return pad.dpad.right;
                case "LeftStickPress": return pad.leftStickButton;
                case "RightStickPress": return pad.rightStickButton;
                case "LeftTrigger": return pad.leftTrigger;
                case "RightTrigger": return pad.rightTrigger;
                default: return null;
            }
        }
    }

    /// <summary>
    /// 将键盘/手柄导航限制在当前可见模态内，并在焦点进入滚动区域时把控件带入视口。
    /// 该组件只管理 UGUI 选择状态，不写玩法、存档或领域数据。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class D16ModalFocusScope : MonoBehaviour
    {
        private GameObject _modalRoot;
        private GameObject _initialSelection;
        private readonly List<Selectable> _selectables = new List<Selectable>();
        private GameObject _lastSelection;

        public static D16ModalFocusScope Attach(GameObject modalRoot, GameObject initialSelection)
        {
            if (modalRoot == null) return null;
            var scope = modalRoot.GetComponent<D16ModalFocusScope>();
            if (scope == null) scope = modalRoot.AddComponent<D16ModalFocusScope>();
            scope.Configure(modalRoot, initialSelection);
            return scope;
        }

        private void Configure(GameObject modalRoot, GameObject initialSelection)
        {
            _modalRoot = modalRoot;
            _initialSelection = initialSelection;
            RebuildNavigation();
        }

        private void OnEnable()
        {
            _lastSelection = null;
            RebuildNavigation();
        }

        private void LateUpdate()
        {
            if (_modalRoot == null || !_modalRoot.activeInHierarchy || EventSystem.current == null)
                return;

            var selected = EventSystem.current.currentSelectedGameObject;
            if (!IsUsableSelection(selected))
            {
                selected = ResolveInitialSelection();
                if (selected != null)
                    EventSystem.current.SetSelectedGameObject(selected);
            }

            if (selected != null && selected != _lastSelection)
            {
                _lastSelection = selected;
                EnsureSelectedVisible(selected.GetComponent<Selectable>());
            }
        }

        private bool IsUsableSelection(GameObject selected)
        {
            if (selected == null || _modalRoot == null ||
                !selected.transform.IsChildOf(_modalRoot.transform))
                return false;

            var selectable = selected.GetComponent<Selectable>();
            return selectable != null && selectable.isActiveAndEnabled && selectable.interactable;
        }

        private GameObject ResolveInitialSelection()
        {
            if (_initialSelection != null && IsUsableSelection(_initialSelection))
                return _initialSelection;

            for (int i = 0; i < _selectables.Count; i++)
            {
                var selectable = _selectables[i];
                if (selectable != null && selectable.isActiveAndEnabled && selectable.interactable)
                    return selectable.gameObject;
            }

            return null;
        }

        private void RebuildNavigation()
        {
            _selectables.Clear();
            if (_modalRoot == null) return;

            var candidates = _modalRoot.GetComponentsInChildren<Selectable>(true);
            for (int i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || candidate.gameObject == _modalRoot ||
                    !candidate.isActiveAndEnabled || !candidate.interactable)
                    continue;
                _selectables.Add(candidate);
            }

            // 以层级创建顺序建立显式上下导航，避免 EventSystem 自动导航穿透到下层 Canvas。
            // 同一行的键盘/手柄按钮也会按创建顺序成为左右邻居，保证手柄可完成重绑定。
            for (int i = 0; i < _selectables.Count; i++)
            {
                var navigation = _selectables[i].navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnUp = _selectables[(i + _selectables.Count - 1) % _selectables.Count];
                navigation.selectOnDown = _selectables[(i + 1) % _selectables.Count];
                navigation.selectOnLeft = navigation.selectOnUp;
                navigation.selectOnRight = navigation.selectOnDown;
                _selectables[i].navigation = navigation;
            }
        }

        private static void EnsureSelectedVisible(Selectable selectable)
        {
            if (selectable == null) return;
            var scroll = selectable.GetComponentInParent<ScrollRect>();
            if (scroll == null || scroll.viewport == null || scroll.content == null)
                return;

            Canvas.ForceUpdateCanvases();
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                scroll.viewport, selectable.transform);
            float offset = 0f;
            if (bounds.max.y > scroll.viewport.rect.height)
                offset = scroll.viewport.rect.height - bounds.max.y;
            else if (bounds.min.y < 0f)
                offset = -bounds.min.y;

            if (Mathf.Abs(offset) < 0.5f) return;
            var position = scroll.content.anchoredPosition;
            position.y -= offset;
            float maxY = Mathf.Max(0f, scroll.content.rect.height - scroll.viewport.rect.height);
            position.y = Mathf.Clamp(position.y, 0f, maxY);
            scroll.content.anchoredPosition = position;
            scroll.StopMovement();
        }
    }
}
