using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23ShipyardScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            D23ScenarioAssertions.Check(ref assertions,
                D19ShipyardProtocol.D4ApiFacts.Count > 0,
                "D23 shipyard scenario must retain the D4 public API evidence.");
            D23ScenarioAssertions.Check(ref assertions,
                D19ShipyardProtocol.ReasonRepairUnimplemented == "port.shipyard.repair_unimplemented",
                "D23 shipyard repair must remain an honest blocked path.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
