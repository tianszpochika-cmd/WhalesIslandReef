using System;
using System.Globalization;
using Whalebone.Systems.D4;

namespace Whalebone.Systems.D4
{
    /// <summary>
    /// D11-A03 的生态压力接收入口。养殖系统只能提交事实输入；压力增量、
    /// 污染、浑浊度和鱼群压力由现有 EcologySystem 计算并写入。
    /// </summary>
    public sealed partial class D4EcologySystem
    {
        public D11EcologyOperationResult PreviewAquaculturePressure(string facilityId,
            string habitatId, int fishQuantity, int feedConsumed, int outputQuantity,
            int mortality, int worldDay)
        {
            InitializeD11State();
            int fish = Math.Max(0, fishQuantity);
            int feed = Math.Max(0, feedConsumed);
            int output = Math.Max(0, outputQuantity);
            int dead = Math.Max(0, mortality);
            float pressure = 0.01f + fish * 0.0025f + feed * 0.004f +
                output * 0.005f + dead * 0.003f;
            pressure = Math.Max(0.01f, Math.Min(0.50f, pressure));
            return new D11EcologyOperationResult
            {
                Succeeded = true,
                Applied = false,
                Code = "aquaculture_pressure_preview",
                Message = "EcologySystem 已计算养殖压力输入。",
                WorldDay = Math.Max(0, worldDay),
                Quantity = Math.Max(1, (int)Math.Round(pressure * 100f)),
                PollutionAfter = _state.Habitat == null ? pressure :
                    Math.Max(0f, Math.Min(1f, _state.Habitat.Pollution + pressure)),
            };
        }

        public D11EcologyOperationResult ApplyAquaculturePressure(string facilityId,
            string habitatId, int fishQuantity, int feedConsumed, int outputQuantity,
            int mortality, int worldDay, string eventId, long saveRevision)
        {
            InitializeD11State();
            string id = string.IsNullOrWhiteSpace(eventId)
                ? "event_d11_aquaculture_pressure_" + Math.Max(0, worldDay).ToString(CultureInfo.InvariantCulture)
                : eventId;
            if (IsDuplicate(id, saveRevision, null))
                return DuplicateResult(id, worldDay, "养殖生态压力事实已经接收，未重复增加压力。");

            var preview = PreviewAquaculturePressure(facilityId, habitatId, fishQuantity,
                feedConsumed, outputQuantity, mortality, worldDay);
            float pressure = Math.Max(0.01f, preview.Quantity / 100f);
            _state.Habitat.Pollution = Clamp01(_state.Habitat.Pollution + pressure);
            _state.Habitat.Turbidity = Clamp01(_state.Habitat.Turbidity + pressure * 0.35f);
            foreach (var school in _state.FishSchools)
            {
                if (school == null) continue;
                school.FishingPressure = Math.Max(0, school.FishingPressure + preview.Quantity);
                school.Recovery = Math.Max(0f, school.Recovery - pressure);
            }
            _state.Habitat.LastUpdatedDay = Math.Max(_state.Habitat.LastUpdatedDay, Math.Max(0, worldDay));
            _state.Habitat.Revision = ++_state.EcologyRevision;
            _state.LastEcologyDay = Math.Max(_state.LastEcologyDay, Math.Max(0, worldDay));
            _state.LastEcologyEventId = id;
            RememberEvent(id, saveRevision);
            return new D11EcologyOperationResult
            {
                Succeeded = true,
                Applied = true,
                Code = "aquaculture_pressure_applied",
                Message = "养殖生态压力已由 EcologySystem 接收并裁决。",
                EventId = id,
                WorldDay = Math.Max(0, worldDay),
                Quantity = preview.Quantity,
                PollutionAfter = _state.Habitat.Pollution,
            };
        }
    }
}
