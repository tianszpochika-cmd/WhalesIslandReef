# D13-A05 交接报告：岛屿治理与人口

## 交付

- `game/Assets/Scripts/Systems/D13/D13IslandGovernanceContracts.cs`
- `game/Assets/Scripts/Systems/D13/D13IslandGovernanceConfiguration.cs`
- `game/Assets/Scripts/Systems/D13/D13IslandGovernanceCatalog.cs`
- `game/Assets/Scripts/Systems/D13/D13IslandGovernanceSystem.cs`
- `game/Assets/Tests/EditMode/D13IslandGovernanceSystemTests.cs`
- `tools/d13_a05_runner`
- `docs/game/design/common/D13_ISLAND_GOVERNANCE_POPULATION.md`

## 新接口

- authority：`ID13GovernanceResidenceAuthority`、`ID13GovernanceProvisionAuthority`、`ID13GovernanceEconomyAuthority`、`ID13GovernancePolicyAuthority`、`ID13GovernanceCommunityAuthority`、`ID13GovernanceAgentAuthority`。
- 操作：`MigrateIn`、`MigrateOut`、`AssignJob`、`AdvanceDay`、`EnrollEducation`、`PracticeReligion`、`AssessTax`、`PayWage`、`ApplyPolicy`、`ApplyCommunityAgreement`、`ManageAgent`、`ResolveSecurityIncident`。
- 只读：`GetReadModel`、`FindResident`、`FindHousehold`、`FindJob`、`CaptureSnapshot`。
- 存档：`SaveSystem.SaveD13GovernanceRuntime`、`LoadD13GovernanceRuntime`；JSON 节点为 `location_json.d13Governance`。

## 边界与事实

`IslandGovernanceSystem` 是居民、家庭、就业投影、健康、满意度、治安和治理记录的唯一写者。产权/法律、生态供给、经济、政策、社群和代理人结果必须由各自 authority 以来源事件返回。税、工资、金钱、库存、声望、宗教系统状态和产权不会在 A05 内复制或直接修改。

所有成功和失败操作写入统一 EventLog；事件包含 owner、原因引用、source event id、世界日、保存 revision 和失败恢复路线。宗教、历史和共同体内容都要求结构化审查标签；带警告内容需要显式确认。

## 验证结果

- `dotnet build game/Whalebone.Systems.csproj --no-restore --nologo`：PASS，0 warning/0 error。
- `dotnet build game/Save.Tests.csproj --no-restore --nologo`：PASS，0 warning/0 error。
- `dotnet build tools/d13_a05_runner/D13A05Runner.csproj --nologo`：PASS。
- `dotnet run --no-build --project tools/d13_a05_runner/D13A05Runner.csproj`：D13-A05 tests PASS count=3。

## 后续接入

D13 后续系统可读取治理 read model、保存快照和带来源事件；不得成为人口/健康/满意度/治安、税率、工资或政策的第二写者。D13-S01 应把治理快照纳入整体验收，但仍通过 owner authority 连接 D10/D11/D12/D13-A01。
