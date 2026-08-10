// D11-A05 ecology-owned rare animal decisions.
// Collection/codex state is written by D11RareAnimalCollectionSystem; animal
// lifecycle, welfare and transport mortality remain EcologySystem facts.

using System;
using Whalebone.Systems.D9;

namespace Whalebone.Systems.D4
{
    public sealed partial class D4EcologySystem
    {
        public D11AnimalEcologyDecision DiscoverRareAnimal(string animalId, string habitatId,
            string locationRef, string sourceRef, string season, int worldDay)
        {
            var animal = Find(animalId);
            var definition = D4AnimalCatalog.Find(animalId);
            if (animal == null || definition == null || !definition.Rare)
                return RejectRare(animalId, "animal_not_available", "目标稀有动物不在当前生态状态中。", worldDay);
            if (string.IsNullOrWhiteSpace(locationRef) || string.IsNullOrWhiteSpace(sourceRef) ||
                string.IsNullOrWhiteSpace(season))
                return RejectRare(animalId, "discovery_evidence_missing", "发现必须提供地点、季节和正式来源。", worldDay);
            if (!HabitatMatches(habitatId, _state.HabitatId, animal.HabitatId))
                return RejectRare(animalId, "wrong_habitat", "目标动物不在请求的生境中。", worldDay);
            if (animal.Lifecycle == D11AnimalLifecycleState.Dead ||
                animal.Lifecycle == D11AnimalLifecycleState.Collected)
                return RejectRare(animalId, "animal_unavailable", "目标动物已经不再可发现。", worldDay);

            animal.Lifecycle = D11AnimalLifecycleState.Discovered;
            animal.CurrentLocationRef = locationRef;
            animal.SourceRef = sourceRef;
            animal.LastHandledDay = Math.Max(animal.LastHandledDay, Math.Max(0, worldDay));
            return Decision(animal, "animal_discovered", "生态系统确认了现存稀有动物。", worldDay);
        }

        public D11AnimalEcologyDecision IdentifyRareAnimal(string animalId, int worldDay)
        {
            var animal = Find(animalId);
            if (animal == null) return RejectRare(animalId, "animal_missing", "动物状态不存在。", worldDay);
            if (animal.Lifecycle == D11AnimalLifecycleState.Dead ||
                animal.Lifecycle == D11AnimalLifecycleState.Collected)
                return RejectRare(animalId, "animal_unavailable", "死亡或已收藏的动物不能再次识别。", worldDay);
            if (animal.Lifecycle == D11AnimalLifecycleState.Wild)
                return RejectRare(animalId, "discovery_required", "必须先发现动物才能识别。", worldDay);
            if (animal.Lifecycle < D11AnimalLifecycleState.Identified)
                animal.Lifecycle = D11AnimalLifecycleState.Identified;
            animal.LastHandledDay = Math.Max(animal.LastHandledDay, Math.Max(0, worldDay));
            return Decision(animal, "animal_identified", "生态系统确认了物种身份。", worldDay);
        }

        public D11AnimalEcologyDecision ObserveRareAnimal(string animalId, D9WeatherSeaSnapshot weather,
            int worldDay)
        {
            var animal = Find(animalId);
            if (animal == null) return RejectRare(animalId, "animal_missing", "动物状态不存在。", worldDay);
            if (animal.Lifecycle == D11AnimalLifecycleState.Dead ||
                animal.Lifecycle == D11AnimalLifecycleState.Collected)
                return RejectRare(animalId, "animal_unavailable", "该动物当前不可观察。", worldDay);
            if (animal.Lifecycle == D11AnimalLifecycleState.Wild)
                return RejectRare(animalId, "discovery_required", "必须先发现动物才能观察。", worldDay);
            var conditions = ReadConditions(weather, worldDay);
            if (conditions.ObservationBlocked)
                return RejectRare(animalId, "observation_blocked", "天气和浑浊度不允许可靠观察。", worldDay);
            animal.Observed = true;
            if (animal.Lifecycle < D11AnimalLifecycleState.Observed)
                animal.Lifecycle = D11AnimalLifecycleState.Observed;
            animal.LastHandledDay = Math.Max(animal.LastHandledDay, Math.Max(0, worldDay));
            return Decision(animal, "animal_observed", "生态系统提供了可记录的行为观察。", worldDay);
        }

        public D11AnimalEcologyDecision RescueRareAnimal(string animalId, string locationRef, int worldDay)
        {
            var animal = Find(animalId);
            if (animal == null) return RejectRare(animalId, "animal_missing", "动物状态不存在。", worldDay);
            if (animal.Lifecycle == D11AnimalLifecycleState.Dead ||
                animal.Lifecycle == D11AnimalLifecycleState.Collected)
                return RejectRare(animalId, "animal_unavailable", "死亡或已收藏的动物不能救助。", worldDay);
            if (animal.Lifecycle < D11AnimalLifecycleState.Identified && !animal.Observed)
                return RejectRare(animalId, "observation_required", "救助前必须先完成发现和观察。", worldDay);
            int healthWelfare = Math.Max(0, Math.Min(100, animal.Health));
            int stressPenalty = (int)Math.Round(Math.Max(0f, Math.Min(1f, animal.EcologyStress)) * 25f);
            animal.Welfare = Math.Max(20, Math.Min(100, healthWelfare - stressPenalty));
            animal.Lifecycle = D11AnimalLifecycleState.Rescued;
            animal.CurrentLocationRef = string.IsNullOrWhiteSpace(locationRef) ? animal.CurrentLocationRef : locationRef;
            animal.LastHandledDay = Math.Max(animal.LastHandledDay, Math.Max(0, worldDay));
            return Decision(animal, "animal_rescued", "生态系统确认动物进入救助状态。", worldDay);
        }

        public D11AnimalEcologyDecision BeginRareAnimalTransport(string animalId, string destinationRef, int worldDay)
        {
            var animal = Find(animalId);
            if (animal == null) return RejectRare(animalId, "animal_missing", "动物状态不存在。", worldDay);
            if (animal.Lifecycle != D11AnimalLifecycleState.Rescued)
                return RejectRare(animalId, "rescue_required", "只有已救助动物才能进入合法运输。", worldDay);
            if (string.IsNullOrWhiteSpace(destinationRef))
                return RejectRare(animalId, "destination_missing", "合法转移缺少目的地。", worldDay);
            animal.Lifecycle = D11AnimalLifecycleState.InTransit;
            animal.TransportDays = 0;
            animal.Welfare = Math.Max(0, animal.Welfare - 2);
            animal.LastHandledDay = Math.Max(animal.LastHandledDay, Math.Max(0, worldDay));
            return Decision(animal, "animal_transport_started", "生态系统确认动物进入运输状态。", worldDay);
        }

        public D11AnimalEcologyDecision AdvanceRareAnimalTransport(string animalId, int worldDay,
            int daysElapsed, int seed, D9WeatherSeaSnapshot weather)
        {
            var animal = Find(animalId);
            if (animal == null) return RejectRare(animalId, "animal_missing", "动物状态不存在。", worldDay);
            if (animal.Lifecycle != D11AnimalLifecycleState.InTransit)
                return RejectRare(animalId, "transport_not_active", "动物当前没有进行中的合法运输。", worldDay);
            var conditions = ReadConditions(weather, worldDay);
            int days = Math.Max(1, daysElapsed);
            float risk = 0.02f + (100f - animal.Welfare) * 0.004f +
                Math.Max(0f, Math.Min(1f, animal.EcologyStress)) * 0.25f +
                (conditions.WeatherStateId == D9WeatherState.Storm.ToString() ? 0.28f :
                    conditions.WeatherStateId == D9WeatherState.Rain.ToString() ? 0.08f : 0f) +
                (1f - Math.Max(0f, Math.Min(1f, weather == null ? 1f : weather.Visibility))) * 0.18f +
                Math.Max(0f, conditions.CurrentSpeed - 0.35f) * 0.08f + days * 0.015f;
            risk = Math.Max(0.01f, Math.Min(0.95f, risk));
            int roll = D11Stable(animalId + ":transport:" + worldDay + ":" + seed + ":" + days) % 1000;
            animal.TransportDays += days;
            animal.LastHandledDay = Math.Max(animal.LastHandledDay, Math.Max(0, worldDay));
            if (roll < (int)Math.Round(risk * 1000f))
            {
                animal.Lifecycle = D11AnimalLifecycleState.Dead;
                animal.Welfare = 0;
                return Decision(animal, "transport_mortality", "生态系统判定运输中动物死亡。", worldDay, true, risk);
            }
            animal.Welfare = Math.Max(1, animal.Welfare - Math.Max(1, (int)Math.Round(risk * 8f)));
            return Decision(animal, "transport_survived", "生态系统判定动物安全度过本次运输日。", worldDay, false, risk);
        }

        public D11AnimalEcologyDecision ReleaseRareAnimal(string animalId, string locationRef, int worldDay)
        {
            var animal = Find(animalId);
            if (animal == null) return RejectRare(animalId, "animal_missing", "动物状态不存在。", worldDay);
            if (animal.Lifecycle != D11AnimalLifecycleState.Rescued &&
                animal.Lifecycle != D11AnimalLifecycleState.InTransit)
                return RejectRare(animalId, "release_not_ready", "只有救助中的动物才能释放。", worldDay);
            animal.Lifecycle = D11AnimalLifecycleState.Released;
            animal.CurrentLocationRef = locationRef;
            animal.Welfare = Math.Min(100, animal.Welfare + 5);
            animal.LastHandledDay = Math.Max(animal.LastHandledDay, Math.Max(0, worldDay));
            return Decision(animal, "animal_released", "生态系统确认动物已释放回合适生境。", worldDay);
        }

        public D11AnimalEcologyDecision CollectRareAnimal(string animalId, string locationRef, int worldDay)
        {
            var animal = Find(animalId);
            if (animal == null) return RejectRare(animalId, "animal_missing", "动物状态不存在。", worldDay);
            if (animal.Lifecycle != D11AnimalLifecycleState.InTransit &&
                animal.Lifecycle != D11AnimalLifecycleState.Rescued)
                return RejectRare(animalId, "collection_not_ready", "只有合法转移中的救助动物才能入藏。", worldDay);
            animal.Lifecycle = D11AnimalLifecycleState.Collected;
            animal.CurrentLocationRef = locationRef;
            animal.LastHandledDay = Math.Max(animal.LastHandledDay, Math.Max(0, worldDay));
            return Decision(animal, "animal_collected", "生态系统确认动物进入合法收藏状态。", worldDay);
        }

        public D11AnimalEcologyDecision RecordIllegalCaptureAttempt(string animalId, int worldDay)
        {
            var animal = Find(animalId);
            if (animal == null) return RejectRare(animalId, "animal_missing", "非法交易目标不存在。", worldDay);
            return Decision(animal, "illegal_capture_rejected", "生态系统拒绝非法捕捉，不生成动物或奖励。", worldDay);
        }

        private static bool HabitatMatches(string requested, string stateHabitat, string animalHabitat)
        {
            if (string.IsNullOrWhiteSpace(requested)) return true;
            string r = requested.Replace("/", "_");
            string s = (stateHabitat ?? "").Replace("/", "_");
            string a = (animalHabitat ?? "").Replace("/", "_");
            return string.Equals(r, s, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, a, StringComparison.OrdinalIgnoreCase) ||
                (r == "nearshore" && (s == "nearshore_reef_edge" || a == "beach")) ||
                (r == "coast" && (s == "beach" || a == "beach"));
        }

        private static D11AnimalEcologyDecision RejectRare(string animalId, string code, string message, int day)
        {
            return new D11AnimalEcologyDecision
            {
                Accepted = false,
                Applied = false,
                Code = code,
                Message = message,
                AnimalId = animalId,
                WorldDay = Math.Max(0, day),
            };
        }

        private static D11AnimalEcologyDecision Decision(AnimalState animal, string code, string message,
            int day, bool died = false, float mortalityRisk = 0f)
        {
            return new D11AnimalEcologyDecision
            {
                Accepted = true,
                Applied = true,
                Died = died,
                Code = code,
                Message = message,
                AnimalId = animal.AnimalId,
                LocationRef = animal.CurrentLocationRef,
                Behavior = animal.Behavior.ToString(),
                Welfare = animal.Welfare,
                MortalityRisk = mortalityRisk,
                Lifecycle = animal.Lifecycle,
                WorldDay = Math.Max(0, day),
            };
        }
    }
}
