using Whalebone.Systems.D11;
using Whalebone.Systems.Events;

var log = new EventLog();
var system = new D11WhaleDolphinSystem(log);
var migration = system.AdvanceMigration(
    "smoke_whale", "whale_humpback", D11MarineSpeciesKind.Whale, "route_smoke", 1, 7,
    new D11MarineEcologyReadSnapshot(), new D11MarineEnvironmentReadSnapshot(),
    "smoke", "open_sea", 1);
if (!migration.Succeeded) throw new InvalidOperationException(migration.Message);

var policy = new D11WhalingPolicySnapshot
{
    LicenseStatus = D11WhalingLicenseStatus.Valid,
    LicenseId = "license_smoke_001",
    LawPolicy = D11WhalingLawPolicy.CommercialAllowed,
    CrewCount = 4,
    RequiredCrewCount = 2,
    ShipClassId = "whaling_vessel",
    RequiredShipClassId = "whaling_vessel",
    HasProcessingFacility = true,
    EquipmentReady = true,
    SocialReputation = 20,
    MinimumSocialReputation = 10,
};
var request = new D11WhaleDolphinActionRequest
{
    Action = D11MarineRouteAction.Whaling,
    EncounterId = "smoke_whale",
    WorldDay = 2,
    Seed = 0,
    SaveRevision = 2,
    WhalingPolicy = policy,
};
var whaling = system.Resolve(request);
if (!whaling.Succeeded) throw new InvalidOperationException(whaling.Message);
var duplicate = system.Resolve(request);
if (!duplicate.Duplicate) throw new InvalidOperationException("D11 duplicate gate did not engage.");
system.Snapshot.Validate();
Console.WriteLine("D11-A04 smoke PASS: migration/whaling/alternative-boundary/dedupe/snapshot");
