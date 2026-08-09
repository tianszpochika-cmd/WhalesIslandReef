// ============================================================
// VoyageSettlementSystem —— 航程计划、日推进与结算
// ============================================================

using System;

namespace Whalebone.Systems.D3
{
    public sealed class VoyageSettlementSystem
    {
        private readonly VoyageEventSystem _events;

        public VoyageSettlementSystem(VoyageEventSystem events = null)
        {
            _events = events ?? new VoyageEventSystem();
        }

        public bool TryBegin(string originId, string destinationId, float shipSpeed, int seed,
            int water, int food, int hull, int hullMax, out VoyageState state, out string error)
        {
            state = null;
            error = null;
            var origin = PortCatalog.Find(originId);
            var destination = PortCatalog.Find(destinationId);
            if (origin == null || destination == null)
            {
                error = "invalid_route";
                return false;
            }

            int totalDays = PortCatalog.CalculateTravelDays(originId, destinationId, shipSpeed);
            if (totalDays <= 0)
            {
                error = "same_location";
                return false;
            }
            if (water < totalDays || food < totalDays)
            {
                error = "insufficient_supplies";
                return false;
            }
            if (hull <= 0)
            {
                error = "ship_disabled";
                return false;
            }

            var weather = WeatherCatalog.All[NormalizeIndex(seed, WeatherCatalog.All.Count)];
            state = new VoyageState
            {
                OriginId = originId,
                DestinationId = destinationId,
                Status = D3VoyageStatus.Sailing,
                Weather = weather.Type,
                CurrentDay = 0,
                TotalDays = totalDays,
                Seed = seed,
                Water = Math.Max(0, water),
                Food = Math.Max(0, food),
                Hull = Math.Max(0, hull),
                HullMax = Math.Max(0, hullMax),
            };
            return true;
        }

        public VoyageDayResult AdvanceDay(VoyageState state)
        {
            if (state == null)
                return FailDay("invalid_state", "航程状态不存在。", null);
            if (state.Status != D3VoyageStatus.Sailing)
                return FailDay("not_sailing", "当前没有进行中的航程。", state);
            if (state.Water <= 0 || state.Food <= 0)
            {
                state.Status = D3VoyageStatus.Failed;
                return FailDay("insufficient_supplies", "补给耗尽，航程失败。", state);
            }

            state.Water--;
            state.Food--;
            var eventDefinition = _events.Roll(state);
            _events.Apply(state, eventDefinition);
            state.CurrentDay++;

            if (state.Hull <= 0)
            {
                state.Status = D3VoyageStatus.Failed;
                return FailDay("ship_disabled", "船体损毁，航程失败。", state, eventDefinition.Id);
            }

            bool arrived = state.CurrentDay >= state.TotalDays;
            if (arrived) state.Status = D3VoyageStatus.Arrived;
            return new VoyageDayResult
            {
                Succeeded = true,
                Arrived = arrived,
                EventId = eventDefinition.Id,
                Message = eventDefinition.DisplayName + "：" + eventDefinition.Description,
                WaterAfter = state.Water,
                FoodAfter = state.Food,
                HullAfter = state.Hull,
                MoneyDelta = eventDefinition.MoneyDelta,
            };
        }

        public VoyageSettlement Settle(VoyageState state)
        {
            if (state == null)
                return FailureSettlement(null, null, "invalid_state", "航程状态不存在。", 0, 0, 0, 0);
            if (state.Status == D3VoyageStatus.Failed)
                return FailureSettlement(state.OriginId, state.DestinationId, "voyage_failed", "航程失败，无法结算。",
                    state.CurrentDay, state.Water, state.Food, state.Hull);
            if (state.Status != D3VoyageStatus.Arrived)
                return FailureSettlement(state.OriginId, state.DestinationId, "not_arrived", "尚未抵达目的地。",
                    state.CurrentDay, state.Water, state.Food, state.Hull);

            state.Status = D3VoyageStatus.InPort;
            return new VoyageSettlement
            {
                Succeeded = true,
                OriginId = state.OriginId,
                DestinationId = state.DestinationId,
                DaysElapsed = state.CurrentDay,
                Events = new System.Collections.Generic.List<string>(state.EventIds),
                WaterRemaining = state.Water,
                FoodRemaining = state.Food,
                HullRemaining = state.Hull,
                MoneyDelta = state.MoneyDelta,
                Message = "航程抵达，结算完成。",
            };
        }

        private static VoyageDayResult FailDay(string code, string message, VoyageState state, string eventId = null)
        {
            return new VoyageDayResult
            {
                Succeeded = false,
                Arrived = false,
                ErrorCode = code,
                EventId = eventId,
                Message = message,
                WaterAfter = state == null ? 0 : state.Water,
                FoodAfter = state == null ? 0 : state.Food,
                HullAfter = state == null ? 0 : state.Hull,
                MoneyDelta = state == null ? 0 : state.MoneyDelta,
            };
        }

        private static VoyageSettlement FailureSettlement(string origin, string destination, string code,
            string message, int days, int water, int food, int hull)
        {
            return new VoyageSettlement
            {
                Succeeded = false,
                OriginId = origin,
                DestinationId = destination,
                DaysElapsed = Math.Max(0, days),
                WaterRemaining = Math.Max(0, water),
                FoodRemaining = Math.Max(0, food),
                HullRemaining = Math.Max(0, hull),
                Message = code + ": " + message,
            };
        }

        private static int NormalizeIndex(int value, int count)
        {
            if (count <= 0) return 0;
            unchecked
            {
                uint unsigned = (uint)value;
                return (int)(unsigned % (uint)count);
            }
        }
    }
}
