using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    internal sealed class D11AquacultureProjection
    {
        public D11AquacultureFacilityState State;
        public int DeadQuantity;
        public int FeedConsumed;
        public int LaborConsumed;
        public int DaysApplied;
    }

    internal static class D11AquacultureMath
    {
        public static D11AquacultureProjection Advance(D11AquacultureFacilityState source,
            D11AquacultureFacilityDefinition definition, D11FishDefinition fish, int worldDay,
            int waterExchangeUnits, int externalLaborUnits, int seed, bool islandFacility)
        {
            var result = new D11AquacultureProjection { State = source.Clone() };
            int day = Math.Max(0, worldDay);
            int days = Math.Max(1, day - Math.Max(0, source.LastWorldDay));
            int exchangePerDay = Math.Max(1, definition.WaterExchangePerDay);
            int laborPerDay = Math.Max(1, definition.MaintenanceLaborPerDay);

            for (int offset = 0; offset < days; offset++)
            {
                int currentDay = Math.Max(0, source.LastWorldDay + offset + 1);
                float crowdingBefore = Crowding(result.State);
                bool exchanged = Math.Max(0, waterExchangeUnits) >= exchangePerDay;
                bool maintained;
                if (islandFacility)
                {
                    maintained = result.State.LaborStock >= laborPerDay;
                    if (maintained) result.State.LaborStock -= laborPerDay;
                }
                else
                {
                    maintained = Math.Max(0, externalLaborUnits) >= laborPerDay;
                    if (maintained) result.LaborConsumed += laborPerDay;
                }

                float feedFactor = 1f;
                if (islandFacility)
                {
                    int requiredFeed = result.State.FishQuantity * Math.Max(1, fish.FeedPerFishPerDay);
                    int consumedFeed = Math.Min(Math.Max(0, result.State.FeedStock), Math.Max(0, requiredFeed));
                    result.State.FeedStock -= consumedFeed;
                    result.FeedConsumed += consumedFeed;
                    feedFactor = requiredFeed <= 0 ? 1f : consumedFeed / (float)requiredFeed;
                }

                result.State.Oxygen = Clamp01(result.State.Oxygen + (exchanged ? 0.20f : -0.10f) -
                    crowdingBefore * 0.12f - (islandFacility && feedFactor < 0.5f ? 0.04f : 0f));
                result.State.WaterQuality = Clamp01(result.State.WaterQuality + (exchanged ? 0.14f : -0.10f) +
                    (islandFacility ? feedFactor * 0.03f : 0f) - crowdingBefore * 0.16f);
                result.State.Maintenance = Clamp01(result.State.Maintenance +
                    (maintained ? 0.08f : -0.12f));
                if (maintained) result.State.MaintenanceDebt = Math.Max(0f, result.State.MaintenanceDebt - 0.5f);
                else result.State.MaintenanceDebt = Clamp01(result.State.MaintenanceDebt + 0.25f);

                if (islandFacility && feedFactor >= 0.8f && maintained)
                {
                    int growth = Math.Min(result.State.FreeVolume,
                        Math.Max(0, result.State.FishQuantity / 8));
                    if (growth > 0)
                        AddFish(result.State, fish, growth);
                }

                int deaths = Mortality(result.State, definition, fish, currentDay, seed,
                    islandFacility && feedFactor < 0.5f);
                if (deaths > 0)
                {
                    RemoveFish(result.State, fish.SpeciesId, deaths);
                    result.DeadQuantity += deaths;
                }
                result.State.Crowding = Crowding(result.State);
            }

            result.DaysApplied = days;
            result.State.LastWorldDay = day;
            result.State.Crowding = Crowding(result.State);
            return result;
        }

        public static int OccupiedBySpecies(D11AquacultureFacilityState state, string speciesId)
        {
            if (state == null || state.Fish == null) return 0;
            int total = 0;
            foreach (var stock in state.Fish)
                if (stock != null && string.Equals(stock.SpeciesId, speciesId, StringComparison.Ordinal))
                    total += Math.Max(0, stock.Quantity);
            return total;
        }

        public static void AddFish(D11AquacultureFacilityState state, D11FishDefinition fish, int quantity)
        {
            if (state == null || fish == null || quantity <= 0) return;
            var stock = FindStock(state, fish.SpeciesId);
            if (stock == null)
            {
                stock = new D11FishStockState
                {
                    SpeciesId = fish.SpeciesId,
                    CargoItemId = fish.CargoItemId,
                    UnitWeight = Math.Max(1, fish.UnitWeight),
                    UnitVolume = Math.Max(1, fish.UnitVolume),
                };
                state.Fish.Add(stock);
            }
            stock.Quantity = Math.Min(int.MaxValue - stock.Quantity, stock.Quantity + quantity);
        }

        public static int RemoveFish(D11AquacultureFacilityState state, string speciesId, int quantity)
        {
            if (state == null || quantity <= 0) return 0;
            var stock = FindStock(state, speciesId);
            if (stock == null) return 0;
            int removed = Math.Min(Math.Max(0, stock.Quantity), quantity);
            stock.Quantity -= removed;
            if (stock.Quantity <= 0) state.Fish.Remove(stock);
            return removed;
        }

        public static D11FishStockState FindStock(D11AquacultureFacilityState state, string speciesId)
        {
            if (state == null || state.Fish == null) return null;
            foreach (var stock in state.Fish)
                if (stock != null && string.Equals(stock.SpeciesId, speciesId, StringComparison.Ordinal))
                    return stock;
            return null;
        }

        public static int Mortality(D11AquacultureFacilityState state,
            D11AquacultureFacilityDefinition definition, D11FishDefinition fish,
            int day, int seed, bool feedShortage)
        {
            int quantity = OccupiedBySpecies(state, fish.SpeciesId);
            if (quantity <= 0) return 0;
            float rate = fish.BaseMortalityRate + Crowding(state) * 0.10f +
                (1f - state.Oxygen) * 0.22f + (1f - state.WaterQuality) * 0.18f +
                (1f - state.Maintenance) * 0.10f + (feedShortage ? 0.08f : 0f);
            rate = Clamp(rate, 0.0f, 0.95f);
            int dead = 0;
            int threshold = (int)Math.Round(rate * 10000f);
            for (int index = 0; index < quantity; index++)
            {
                int roll = Stable(fish.SpeciesId + ":" + (definition == null ? "facility" : definition.Id) +
                    ":" + day.ToString(CultureInfo.InvariantCulture) + ":" + seed + ":" + index) % 10000;
                if (roll < threshold) dead++;
            }
            return Math.Min(quantity, dead);
        }

        public static float Crowding(D11AquacultureFacilityState state)
        {
            if (state == null || state.CapacityVolume <= 0) return 1f;
            return Clamp01(state.UsedVolume / (float)state.CapacityVolume);
        }

        public static string Key(string eventId, long saveRevision)
        {
            return (eventId ?? string.Empty) + "\u001f" + Math.Max(0L, saveRevision).ToString(CultureInfo.InvariantCulture);
        }

        public static bool IsDuplicate(D11AquacultureFacilityState state, EventLog log,
            string eventId, long saveRevision)
        {
            string key = Key(eventId, saveRevision);
            return (state != null && state.AppliedEventKeys != null && state.AppliedEventKeys.Contains(key)) ||
                (log != null && log.Find(eventId, Math.Max(0L, saveRevision)) != null);
        }

        public static void Remember(D11AquacultureFacilityState state, string eventId, long saveRevision)
        {
            if (state == null) return;
            if (state.AppliedEventKeys == null) state.AppliedEventKeys = new List<string>();
            string key = Key(eventId, saveRevision);
            if (!state.AppliedEventKeys.Contains(key)) state.AppliedEventKeys.Add(key);
        }

        public static EventAppendResult Append(EventLog log, string eventId, int day, string location,
            string owner, IEnumerable<string> actors, IEnumerable<string> inputs,
            IEnumerable<string> causes, string resultCode, string summary,
            Dictionary<string, string> values, IEnumerable<string> factIds, long saveRevision,
            WorldEventStatus status = WorldEventStatus.Normal, RecoveryRoute recovery = null)
        {
            return log.Append(new WorldEvent(eventId, Math.Max(0, day),
                string.IsNullOrEmpty(location) ? "nearshore_saint_marys" : location,
                owner,
                new EventResult
                {
                    ResultCode = resultCode,
                    Summary = summary,
                    FactIds = new List<string>(factIds ?? new string[0]),
                    Values = values ?? new Dictionary<string, string>(),
                },
                actors ?? new string[0], inputs ?? new string[0], causes ?? new string[0],
                Math.Max(0L, saveRevision), status, recovery));
        }

        public static string Format(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        public static int Stable(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash & 0x7fffffff);
            }
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static float Clamp(float value, float min, float max)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return min;
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
