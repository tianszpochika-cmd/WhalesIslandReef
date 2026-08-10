# D14-A06 交接报告

日期：2026-08-11  
状态：完成

## 已交付

- 新增 `D14ReligionCommunityContracts.cs`、`D14ReligionCommunityCatalog.cs`、`D14ReligionCommunitySystem.cs`。
- 实现节期周期与参与、饮食标签/合规、建筑开放请求、葬礼善后请求、誓言状态机、D10 船员信任和 D12 社群信任 authority 接入、社群礼仪前置条件。
- 所有样例内容带历史标签、警告、来源说明、`ApprovedWithWarning` 审查状态和跳过选项。
- Agent 仅能生成表达草案；直接以 `agent` 或 `AgentReply` 写宗教、社群、信任结果会被拒绝。
- 接入 SaveSystem，运行时保存到 `location_json.d14ReligionCommunity`，含审查记录、所有操作记录、事件日志和去重键。
- 新增 `D14ReligionCommunitySystemTests.cs` 与 `tools/d14_a06_runner/`。

## 接口/所有权说明

`D14ReligionCommunitySystem` 是宗教实践、审查与自身记录的唯一写者。它不写 BuildingSystem、D10 RelationSystem、D12 CommunityTradeSystem 或 Community/Aftercare 的最终状态；这些系统通过 `ID14BuildingScheduleAuthority`、`ID14FuneralRiteAuthority`、`ID14D10TrustAuthority`、`ID14D12CommunityAuthority` 返回带来源事件的裁决。

所有成功事实携带操作来源、历史审查证据、事件输入和因果来源。重复 `OperationId` 或已保存事件去重键不会再次累加状态。

## 验证结果

- `dotnet build game/Whalebone.Systems.csproj --no-restore`：通过，0 警告、0 错误。
- `dotnet build game/Save.Tests.csproj --no-restore`：通过，0 警告、0 错误。
- A06 独立执行器：`D14-A06 tests PASS count=2`。

## 后续注意

- 新增真实历史或宗教内容前，必须先提交来源、历史标签和审查事实；禁止把项目抽象写成无来源史实。
- UI 只读快照/事件并提交请求；Agent 只生成表达、解释或意图。
- D10/D12/Building/Aftercare 的 authority 适配器需要在对应系统集成层提供，且必须返回可在统一 EventLog 中找到的来源事件。

