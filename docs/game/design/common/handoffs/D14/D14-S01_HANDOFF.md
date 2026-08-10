# D14-S01 同步集成交接报告

日期：2026-08-11  
状态：完成，D14 综合验收通过

## 已交付

- `D14S01IntegrationContracts.cs`：D14-S01 阶段请求、法律/奖励/装备意图、owner authority、集成账本、快照和结果契约。
- `D14S01IntegrationSystem.cs`：战斗、战后、装备和奖励集成；跨域来源校验；任务/关系/法律请求转发；Agent 边界；集成事件和幂等门。
- `D14S01IntegrationTests.cs`：战斗→船体/船员/经济/法律/任务/关系/装备、战后→船员/货物/经济/法律/任务/关系、装备生命周期、奖励读档不重复和 Agent 拒绝测试。
- `tools/d14_s01_runner/`：独立测试执行器。
- SaveSystem 接入 `location_json.d14S01Integration`、`PlayerState.D14S01IntegrationRuntime` 和 `GameState.CurrentD14S01IntegrationRuntime`。
- `docs/game/design/common/D14_ACCEPTANCE_REPORT.md`：D14 综合验收结论。

## 接口与所有权

| authority | 只能由其写入的结果 |
|---|---|
| `ID14S01ShipAuthority` | 船体/船舶损伤应用 |
| `ID14S01CrewAuthority` | 船员战斗和战后状态 |
| `ID14S01CargoAuthority` | 货物/库存战后变化 |
| `ID14S01EconomyAuthority` | 金钱与经济后果 |
| `ID14S01LegalAuthority` | 违法、私掠、港口和势力法律结果 |
| `ID14S01QuestAuthority` | 任务事实与目标变化 |
| `ID14S01RelationAuthority` | 船员/NPC 关系变化 |
| `ID14S01EquipmentAuthority` | A03 特殊装备生命周期 |
| `ID14S01RewardAuthority` | A04 奖励请求/发放结果 |

S01 不保存这些领域的数值副本，也不接受 AgentReply 中的最终结果。authority 必须返回正确 owner 和已存在的来源事实；不满足时集成拒绝。

## 使用约定

1. Combat/Aftercare 先由所属系统产出并落盘事实，再调用 `IntegrateCombat`/`IntegrateAftercare`。
2. 特殊装备使用 `IntegrateEquipment` 转发 A03 生命周期请求；强度、耐久、唯一 ID 和合法性仍由 A03/所属 authority 裁决。
3. 奖励使用 `IntegrateReward`；集成层只提交奖励意图并保存 grant 引用，不生成奖励值。
4. 所有请求使用新的 `OperationId` 和合法 `SaveRevision`；重放已保存操作必须接受为 `Duplicate`，不能再次调用 owner。
5. UI 只读取集成快照/事件；Agent 只能产生战术或外交意图。

## 验证

- `dotnet build game/Whalebone.Systems.csproj --no-restore`：0 warning / 0 error。
- `dotnet build game/Save.Tests.csproj --no-restore`：0 warning / 0 error。
- `dotnet build game/AgentCore/AgentCore.csproj --no-restore`：0 warning / 0 error。
- `D14-S01 tests PASS count=2`。
- D14-A01—A06、D13-S01 回归全部通过。
- Content Registry 124 entries / 13 prefixes；D14 Systems 无网络/LLM。

## D15 交接结论

D14 已通过，允许 D15 开始批量内容生产。D15 只扩展内容目录和经过审查的来源数据，继续复用 D14 owner/authority、事件和存档契约，不在内容生产层复制裁决系统。

