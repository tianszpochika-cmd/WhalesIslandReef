// D24-L19：代表性逻辑 DTO JSON 往返与未知字段兼容入口（纯 C#）。

using System;
using System.Text.Json;
using Whalebone.Systems.D19;
using Whalebone.Systems.D21;
using Whalebone.Systems.D22;

namespace Whalebone.Tests.D24
{
    public static class D24SchemaRoundTripTests
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
        };

        public static int Run() { return RunAll(); }

        public static int RunAll()
        {
            int failed = 0;
            failed += Check("D24-L19 D19 save projection round-trips", D19SaveRoundTrip);
            failed += Check("D24-L19 D21 time DTOs round-trip", D21TimeRoundTrip);
            failed += Check("D24-L19 D22 command/context DTOs round-trip", D22CommandRoundTrip);
            failed += Check("D24-L19 unknown fields do not change validated state", UnknownFieldsAreIgnored);
            return failed;
        }

        private static int Check(string name, Func<bool> test)
        {
            try
            {
                bool ok = test();
                Console.WriteLine((ok ? "[OK] " : "[FAIL] ") + name);
                return ok ? 0 : 1;
            }
            catch (Exception error)
            {
                Console.WriteLine("[EX] " + name + " : " + error.Message);
                return 1;
            }
        }

        private static T RoundTrip<T>(T value)
        {
            string json = JsonSerializer.Serialize(value, Options);
            T restored = JsonSerializer.Deserialize<T>(json, Options);
            if (restored == null) throw new InvalidOperationException("JSON round-trip returned null for " + typeof(T).Name);
            return restored;
        }

        private static bool D19SaveRoundTrip()
        {
            D19PortSaveProjection source = D19PortSaveProjection.CreateDefault("ship_d24");
            source.SaveRevision = 2401;
            D19PortSaveProjection restored = RoundTrip(source);
            restored.Validate();
            return restored.ShipId == source.ShipId && restored.SaveRevision == source.SaveRevision;
        }

        private static bool D21TimeRoundTrip()
        {
            D21TickContext context = D21VoyageTimeSystem.CreateTickContext(24, 600, 0, 2401, 2419, "voyage_d24", "ship_d24");
            D21VoyageTimeState state = D21VoyageTimeState.CreateInitial("voyage_d24", "ship_d24", 120000, 24, 600, 2401);
            D21VoyageTimeInput input = new D21VoyageTimeInput
            {
                RouteDistanceMilliNm = 120000,
                BaseSpeedMilliKnots = 12000,
                SpeedPermille = 1000,
                CurrentPermille = 1000,
            };
            D21TickContext contextCopy = RoundTrip(context);
            D21VoyageTimeState stateCopy = RoundTrip(state);
            D21VoyageTimeInput inputCopy = RoundTrip(input);
            contextCopy.Validate();
            stateCopy.Validate();
            inputCopy.Validate();
            return contextCopy.VoyageSeed == context.VoyageSeed
                && stateCopy.Fingerprint == state.Fingerprint
                && inputCopy.EffectiveSpeedMilliKnots == input.EffectiveSpeedMilliKnots;
        }

        private static bool D22CommandRoundTrip()
        {
            D22UiCommandRequest request = D22UiCommandRequest.Create(
                D22UiCommandKind.OpenPanel, "d24.l19.open", "d24.l19.correlation", "player_d24",
                D22ScreenIds.MainMenu, D22PanelIds.MainMenu, D22PanelIds.MainMenuSettings, 24, 2401);
            request.Parameters.Add(new D22UiParameter { Key = "source", Value = "d24" });
            request.CauseEventIds.Add("event_d24_schema");
            D22UiCommandContext context = new D22UiCommandContext
            {
                ActiveScreenId = D22ScreenIds.MainMenu,
                ActivePanelId = D22PanelIds.MainMenu,
                ActorId = "player_d24",
                WorldDay = 24,
                SaveRevision = 2401,
            };
            D22UiCommandRequest requestCopy = RoundTrip(request);
            D22UiCommandContext contextCopy = RoundTrip(context);
            return requestCopy.ValidateShape() == D22UiCommandValidationCode.None
                && requestCopy.Fingerprint() == request.Fingerprint()
                && contextCopy.ActiveScreenId == context.ActiveScreenId
                && contextCopy.SaveRevision == context.SaveRevision;
        }

        private static bool UnknownFieldsAreIgnored()
        {
            D21VoyageTimeState source = D21VoyageTimeState.CreateInitial("voyage_d24_unknown", "ship_d24", 1000, 24, 600, 2401);
            string json = JsonSerializer.Serialize(source, Options);
            int end = json.LastIndexOf('}');
            if (end < 0) return false;
            json = json.Insert(end, ",\"d24_unknown_future_field\":true");
            D21VoyageTimeState restored = JsonSerializer.Deserialize<D21VoyageTimeState>(json, Options);
            restored.Validate();
            return restored.Fingerprint == source.Fingerprint;
        }
    }
}
