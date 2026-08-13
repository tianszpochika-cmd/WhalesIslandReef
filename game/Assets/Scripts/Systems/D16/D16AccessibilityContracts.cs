using System;
using System.Collections.Generic;
using System.Globalization;

namespace Whalebone.Systems.D16
{
    /// <summary>
    /// D16-A03 的表现层设置契约。它只描述用户偏好，不拥有玩法数值或领域状态。
    /// </summary>
    public enum D16AccessibilityColorVisionMode
    {
        None,
        Deuteranopia,
        Protanopia,
        Tritanopia,
    }

    public enum D16InputAction
    {
        MoveForward,
        MoveBack,
        MoveLeft,
        MoveRight,
        Sprint,
        Jump,
        Interact,
        Pause,
        CameraCycle,
        Map,
        Trade,
        CharacterPreset,
        Language,
        Quality,
        Achievement,
        Save,
        D3CloseTrade,
        D3StartVoyage,
        D3AdvanceVoyage,
        D3Gather,
        D3LandAndSettle,
        D4Faction,
        D4StartCombat,
        D4Reload,
        D4SelectRoundShot,
        D4SelectChainShot,
        D4SelectGrapeShot,
        D4Fire,
        D4Board,
        D4EscapeCombat,
        D4Upgrade,
        D4ObserveAnimal,
        D4TreatCrew,
        D4AdvanceMainline,
        D5DiscoverIsland,
        D5VisitIsland,
        D9OpenPortPreparation,
        D9ClosePortPreparation,
        D9EnterBerth,
        D9BuySalt,
        D9BuyTimber,
        D9VisitTavern,
        D9BuyWater,
        D9RepairHull,
        D9RecruitCrew,
        D9SettleWages,
        D9Upgrade,
        D9Depart,
        D10OpenTavern,
        D10CloseTavern,
        D10Board,
        D10SecurityJob,
        D10Rest,
        D10Eat,
        D10Drink,
        D10Rumor,
        D10Poker,
        D10Dance,
        D10NightTalk,
        D10Recruit,
        D10SkipSensitive,
    }

    public enum D16InputBindingDevice
    {
        PrimaryKeyboard,
        SecondaryKeyboard,
        GamepadButton,
    }

    public enum D16SensitiveContentDecision
    {
        ShowWarning,
        ShowContent,
        SkipContent,
        HiddenByPreference,
    }

    [Serializable]
    public sealed class D16InputBinding
    {
        public D16InputAction Action;
        /// <summary>
        /// 输入作用域只描述表现层上下文。不同页面可以复用同一个物理键，
        /// 同一页面内仍禁止重复绑定，避免一个按键同时触发两个同层意图。
        /// </summary>
        public string Scope = "core";
        public string PrimaryKeyboard;
        public string SecondaryKeyboard;
        public string GamepadButton;

        public D16InputBinding Clone()
        {
            return new D16InputBinding
            {
                Action = Action,
                Scope = Scope,
                PrimaryKeyboard = PrimaryKeyboard,
                SecondaryKeyboard = SecondaryKeyboard,
                GamepadButton = GamepadButton,
            };
        }
    }

    [Serializable]
    public sealed class D16AccessibilitySettings
    {
        public string Locale = "zh-CN";
        public float TextScale = 1f;
        public bool HighContrast;
        public D16AccessibilityColorVisionMode ColorVisionMode;
        public bool ReducedMotion;
        public bool LowStimulus;
        public bool Subtitles = true;
        public bool SubtitleSpeakerLabels = true;
        public bool SensitiveWarnings = true;
        public List<D16InputBinding> InputBindings = D16AccessibilityDefaults.CreateBindings();

        public D16AccessibilitySettings Clone()
        {
            var clone = new D16AccessibilitySettings
            {
                Locale = Locale,
                TextScale = TextScale,
                HighContrast = HighContrast,
                ColorVisionMode = ColorVisionMode,
                ReducedMotion = ReducedMotion,
                LowStimulus = LowStimulus,
                Subtitles = Subtitles,
                SubtitleSpeakerLabels = SubtitleSpeakerLabels,
                SensitiveWarnings = SensitiveWarnings,
                InputBindings = new List<D16InputBinding>(),
            };

            if (InputBindings != null)
            {
                foreach (var binding in InputBindings)
                    if (binding != null) clone.InputBindings.Add(binding.Clone());
            }

            return clone;
        }

        public List<string> Validate()
        {
            var errors = new List<string>();
            if (Locale != "zh-CN" && Locale != "en-US")
                errors.Add("unsupported_locale");
            if (TextScale < 0.8f || TextScale > 2f)
                errors.Add("text_scale_out_of_range");

            var actions = new HashSet<D16InputAction>();
            var keyboardKeys = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            var gamepadButtons = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            if (InputBindings == null || InputBindings.Count == 0)
            {
                errors.Add("input_bindings_missing");
                return errors;
            }

            foreach (var binding in InputBindings)
            {
                if (binding == null)
                {
                    errors.Add("input_binding_null");
                    continue;
                }

                if (!actions.Add(binding.Action))
                    errors.Add("duplicate_input_action:" + binding.Action);

                var scope = string.IsNullOrWhiteSpace(binding.Scope) ? "core" : binding.Scope.Trim();
                ValidateUniqueKey(binding.PrimaryKeyboard, keyboardKeys, scope,
                    "duplicate_keyboard_binding", errors);
                ValidateUniqueKey(binding.SecondaryKeyboard, keyboardKeys, scope,
                    "duplicate_keyboard_binding", errors);
                ValidateUniqueKey(binding.GamepadButton, gamepadButtons, scope,
                    "duplicate_gamepad_binding", errors);
            }

            return errors;
        }

        public void Normalize()
        {
            Locale = Locale == "en-US" ? "en-US" : "zh-CN";
            if (float.IsNaN(TextScale) || float.IsInfinity(TextScale)) TextScale = 1f;
            TextScale = Math.Max(0.8f, Math.Min(2f, TextScale));
            if (InputBindings == null || InputBindings.Count == 0)
                InputBindings = D16AccessibilityDefaults.CreateBindings();
            else
                foreach (var binding in InputBindings)
                    if (binding != null && string.IsNullOrWhiteSpace(binding.Scope)) binding.Scope = "core";
        }

        /// <summary>
        /// 以设置契约为边界修改一条绑定。UI 只能提交候选值，重复键仍由契约拒绝。
        /// </summary>
        public bool TrySetBinding(D16InputAction action, D16InputBindingDevice device, string value,
            out string error)
        {
            error = string.Empty;
            var candidate = Clone();
            D16InputBinding target = null;
            foreach (var binding in candidate.InputBindings)
            {
                if (binding != null && binding.Action == action)
                {
                    target = binding;
                    break;
                }
            }

            if (target == null)
            {
                error = "input_binding_action_missing:" + action;
                return false;
            }

            switch (device)
            {
                case D16InputBindingDevice.PrimaryKeyboard:
                    target.PrimaryKeyboard = value ?? string.Empty;
                    break;
                case D16InputBindingDevice.SecondaryKeyboard:
                    target.SecondaryKeyboard = value ?? string.Empty;
                    break;
                case D16InputBindingDevice.GamepadButton:
                    target.GamepadButton = value ?? string.Empty;
                    break;
                default:
                    error = "input_binding_device_unknown";
                    return false;
            }

            var errors = candidate.Validate();
            foreach (var validationError in errors)
            {
                if (validationError.StartsWith("duplicate_keyboard_binding:") ||
                    validationError.StartsWith("duplicate_gamepad_binding:"))
                {
                    error = validationError;
                    return false;
                }
            }

            InputBindings = candidate.InputBindings;
            return true;
        }

        private static void ValidateUniqueKey(string value, Dictionary<string, HashSet<string>> usedByScope,
            string scope, string errorCode, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            HashSet<string> used;
            if (!usedByScope.TryGetValue(scope, out used))
            {
                used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                usedByScope.Add(scope, used);
            }
            if (!used.Add(value.Trim())) errors.Add(errorCode + ":" + value.Trim());
        }
    }

    public static class D16AccessibilityDefaults
    {
        public static D16AccessibilitySettings Create()
        {
            return new D16AccessibilitySettings();
        }

        public static List<D16InputBinding> CreateBindings()
        {
            return new List<D16InputBinding>
            {
                Binding(D16InputAction.MoveForward, "W", "UpArrow", "DpadUp"),
                Binding(D16InputAction.MoveBack, "S", "DownArrow", "DpadDown"),
                Binding(D16InputAction.MoveLeft, "A", "LeftArrow", "DpadLeft"),
                Binding(D16InputAction.MoveRight, "D", "RightArrow", "DpadRight"),
                Binding(D16InputAction.Sprint, "LeftShift", "RightShift", "LeftStickPress"),
                Binding(D16InputAction.Jump, "Space", "", "South"),
                Binding(D16InputAction.Interact, "E", "", "West"),
                Binding(D16InputAction.Pause, "Escape", "", "Start"),
                Binding(D16InputAction.CameraCycle, "F3", "", "RightShoulder"),
                Binding(D16InputAction.Map, "I", "", "Select"),
                Binding(D16InputAction.Trade, "B", "", "North"),
                Binding(D16InputAction.CharacterPreset, "P", "", "East"),
                Binding(D16InputAction.Language, "L", "", "LeftShoulder"),
                Binding(D16InputAction.Quality, "F1", "", ""),
                Binding(D16InputAction.Achievement, "F2", "", ""),
                Binding(D16InputAction.Save, "F5", "C", "RightStickPress"),

                Binding(D16InputAction.D3CloseTrade, "Escape", "", "Start", "d3"),
                Binding(D16InputAction.D3StartVoyage, "T", "", "South", "d3"),
                Binding(D16InputAction.D3AdvanceVoyage, "N", "", "North", "d3"),
                Binding(D16InputAction.D3Gather, "G", "", "West", "d3"),
                Binding(D16InputAction.D3LandAndSettle, "L", "", "East", "d3"),

                Binding(D16InputAction.D4Faction, "F", "", "LeftShoulder", "d4"),
                Binding(D16InputAction.D4StartCombat, "K", "", "RightShoulder", "d4"),
                Binding(D16InputAction.D4Reload, "R", "", "LeftTrigger", "d4"),
                Binding(D16InputAction.D4SelectRoundShot, "Digit1", "", "DpadLeft", "d4"),
                Binding(D16InputAction.D4SelectChainShot, "Digit2", "", "DpadUp", "d4"),
                Binding(D16InputAction.D4SelectGrapeShot, "Digit3", "", "DpadRight", "d4"),
                Binding(D16InputAction.D4Fire, "Space", "", "RightTrigger", "d4"),
                Binding(D16InputAction.D4Board, "B", "", "South", "d4"),
                Binding(D16InputAction.D4EscapeCombat, "E", "", "West", "d4"),
                Binding(D16InputAction.D4Upgrade, "U", "", "East", "d4"),
                Binding(D16InputAction.D4ObserveAnimal, "A", "", "DpadDown", "d4"),
                Binding(D16InputAction.D4TreatCrew, "M", "", "LeftStickPress", "d4"),
                Binding(D16InputAction.D4AdvanceMainline, "J", "", "RightStickPress", "d4"),

                Binding(D16InputAction.D5DiscoverIsland, "I", "", "Select", "d5"),
                Binding(D16InputAction.D5VisitIsland, "V", "", "South", "d5"),

                Binding(D16InputAction.D9OpenPortPreparation, "P", "", "East", "d9"),
                Binding(D16InputAction.D9ClosePortPreparation, "Escape", "", "Start", "d9"),
                Binding(D16InputAction.D9EnterBerth, "B", "", "South", "d9"),
                Binding(D16InputAction.D9BuySalt, "M", "", "DpadLeft", "d9"),
                Binding(D16InputAction.D9BuyTimber, "X", "", "DpadUp", "d9"),
                Binding(D16InputAction.D9VisitTavern, "T", "", "North", "d9"),
                Binding(D16InputAction.D9BuyWater, "S", "", "DpadDown", "d9"),
                Binding(D16InputAction.D9RepairHull, "R", "", "LeftShoulder", "d9"),
                Binding(D16InputAction.D9RecruitCrew, "Q", "", "West", "d9"),
                Binding(D16InputAction.D9SettleWages, "W", "", "RightShoulder", "d9"),
                Binding(D16InputAction.D9Upgrade, "U", "", "RightStickPress", "d9"),
                Binding(D16InputAction.D9Depart, "D", "", "RightTrigger", "d9"),

                Binding(D16InputAction.D10OpenTavern, "V", "", "South", "d10"),
                Binding(D16InputAction.D10CloseTavern, "Escape", "", "Start", "d10"),
                Binding(D16InputAction.D10Board, "J", "", "North", "d10"),
                Binding(D16InputAction.D10SecurityJob, "K", "", "RightShoulder", "d10"),
                Binding(D16InputAction.D10Rest, "R", "", "West", "d10"),
                Binding(D16InputAction.D10Eat, "E", "", "East", "d10"),
                Binding(D16InputAction.D10Drink, "A", "", "DpadUp", "d10"),
                Binding(D16InputAction.D10Rumor, "H", "", "DpadDown", "d10"),
                Binding(D16InputAction.D10Poker, "P", "", "DpadLeft", "d10"),
                Binding(D16InputAction.D10Dance, "D", "", "DpadRight", "d10"),
                Binding(D16InputAction.D10NightTalk, "N", "", "LeftShoulder", "d10"),
                Binding(D16InputAction.D10Recruit, "Q", "", "LeftStickPress", "d10"),
                Binding(D16InputAction.D10SkipSensitive, "X", "", "RightTrigger", "d10"),
            };
        }

        private static D16InputBinding Binding(D16InputAction action, string primary, string secondary,
            string gamepad, string scope = "core")
        {
            return new D16InputBinding
            {
                Action = action,
                Scope = scope,
                PrimaryKeyboard = primary,
                SecondaryKeyboard = secondary,
                GamepadButton = gamepad,
            };
        }
    }

    public static class D16DisplayFormatter
    {
        public static string Number(decimal value, string locale, int decimals = 0)
        {
            var culture = GetCulture(locale);
            var places = Math.Max(0, Math.Min(6, decimals));
            return value.ToString("N" + places, culture);
        }

        public static string Percent(decimal value, string locale, int decimals = 0)
        {
            var culture = GetCulture(locale);
            var places = Math.Max(0, Math.Min(6, decimals));
            return value.ToString("P" + places, culture);
        }

        public static string SignedNumber(decimal value, string locale, int decimals = 0)
        {
            var formatted = Number(Math.Abs(value), locale, decimals);
            if (value > 0) return "+" + formatted;
            if (value < 0) return "-" + formatted;
            return formatted;
        }

        private static CultureInfo GetCulture(string locale)
        {
            return locale == "en-US"
                ? CultureInfo.GetCultureInfo("en-US")
                : CultureInfo.GetCultureInfo("zh-CN");
        }
    }

    public static class D16SensitiveContentPolicy
    {
        public static D16SensitiveContentDecision Decide(
            bool warningRequired,
            bool warningEnabled,
            bool acknowledged,
            bool skipRequested)
        {
            if (skipRequested) return D16SensitiveContentDecision.SkipContent;
            if (warningRequired && !warningEnabled) return D16SensitiveContentDecision.HiddenByPreference;
            if (warningRequired && !acknowledged) return D16SensitiveContentDecision.ShowWarning;
            return D16SensitiveContentDecision.ShowContent;
        }
    }
}
