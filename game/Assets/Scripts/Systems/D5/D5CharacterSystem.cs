using System;

namespace Whalebone.Systems.D5
{
    /// <summary>基础角色预设和外观参数的唯一裁决者。</summary>
    public sealed class D5CharacterSystem
    {
        private readonly D5CharacterState _state;

        public D5CharacterSystem(D5CharacterState initial = null)
        {
            _state = initial == null ? new D5CharacterState() : initial.Clone();
            if (D5CharacterCatalog.Find(_state.PresetId) == null) ApplyPreset("dockhand_male");
        }

        public D5CharacterState Snapshot => _state.Clone();

        public bool ApplyPreset(string presetId)
        {
            var preset = D5CharacterCatalog.Find(presetId);
            if (preset == null) return false;
            _state.PresetId = preset.Id;
            _state.SkinTone = preset.SkinTone;
            _state.HairStyle = preset.HairStyle;
            _state.HairColor = preset.HairColor;
            _state.Outfit = preset.Outfit;
            _state.FaceScale = Clamp(preset.FaceScale, 0.85f, 1.15f);
            _state.BodyScale = Clamp(preset.BodyScale, 0.85f, 1.15f);
            return true;
        }

        public D5CharacterPresetDefinition CurrentPreset => D5CharacterCatalog.Find(_state.PresetId);

        private static float Clamp(float value, float min, float max) { return Math.Max(min, Math.Min(max, value)); }
    }
}
