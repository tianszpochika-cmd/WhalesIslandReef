# D21 船舶、海况、天气与航海逻辑验收

> 归属：`D21-L32`，D21 章节逻辑汇合门禁唯一验收文档。
>
> 验收时间：2026-09-01（Asia/Shanghai）。
>
> 当前结论：`LOGIC_PASS`。D21-L01—L32 的结果 JSON 与 handoff 已完整，离线纯逻辑 runner 的 9/9 suites、确定性 integration/crew 断言、编译和边界审计均有真实证据。

## 1. 证据核对

| 检查项 | 真实结果 | 结论 |
|---|---|---|
| D21-L01—L29 结果 JSON | 29/29 存在、可解析且状态均为 `LOGIC_PASS` | 通过 |
| D21-L01—L29 handoff | 29/29 存在 | 通过 |
| D21-L30—L32 结果与 handoff | 3/3 结果 JSON、3/3 handoff 存在并可解析 | 通过 |
| D21-L30 runner | 9/9 suites 通过；边界扫描 29 个 C# 文件、0 违规；D21 编译 0 warning/0 error；最终证据检查 32/32 | 通过 |
| D21 11-case integration | `D21_INTEGRATION_BASE_CASES=8`、`D21_INTEGRATION_CASES=11`、`D21_INTEGRATION_FAILURES=0` | 通过 |
| D20→D21 crew projection | `D20_CREW_PROJECTION_ASSERTIONS=14`、`D21_CREW_PROJECTION_ASSERTIONS=16`、`CREW_PROJECTION_FAILURES=0` | 通过 |
| D21-L31 | Runtime Deferred 登记已完成 | 通过 |
| D21-L32 | 章节逻辑验收文档、接口边界和 Runtime 分离结论完成 | 通过 |

机器汇总报告：`game/tools/results/D21/D21-L30/D21_L30_RUNNER_REPORT.json`。

## 2. 逻辑已提供的确定性接口

| 领域 | owner / 公共入口 | 下游使用约束 |
|---|---|---|
| 船身份、船型和状态 | `D21ShipIdentityContracts` | 只使用稳定 ship/type ID、迁移结果和只读 identity clone |
| 船体组件与船况 | `D21HullComponentSystem`、`D21ShipConditionSystem` | 安装、损伤、维修和适航性只能由对应 owner 裁决 |
| 货舱与船员 | `D21CargoCapacityRules`、`D21CrewRequirementRules` | D21 不写 D3 货物数量；D20 crew 只读投影先映射再评估岗位/人数/状态 |
| 航线与天气 | `D21NavigationRouteContracts`、`D21WeatherInputContracts` | 使用稳定 route/waypoint/hazard ID 和显式天气输入，不保存 Transform |
| 海况、影响与时间 | `D21SeaStateSystem`、`D21WeatherImpactRules`、`D21VoyageTimeSystem` | 使用显式固定 Tick、世界日、保存修订和稳定 seed，不使用帧时间或系统时钟 |
| 航向、锚泊与风险 | `D21CourseCommandSystem`、`D21AnchorMooringSystem`、`D21VoyageRiskSystem` | 命令必须经过前置、权限、状态和幂等校验；港内泊位事实仍由 D19 写入 |
| 出航与返港 | `D21PortDepartureAdapter`、`D21PortReturnAdapter` | 只能构造公开 D19 routed command 并消费 D19 result/event，不私写港口事实 |
| 航程汇合与推进 | `D21VoyageCommandService.Route`、`D21VoyageResolutionSystem.ResolveTick` | 固定顺序读取快照、计算影响、消耗补给、裁决事件、更新 projection 并提交修订 |
| 事件与保存 | `D21VoyageEventRouter`、`D21ShipSaveProjection` | event_id/save_revision 去重，批次原子追加；保存 owner projection、seed、tick、游标并可验证 round-trip |

## 3. 固定 Tick 与不变量

- 航程只接受显式 `D21TickContext`；同一输入、seed、world day 和 revision 得到相同海况、风险、事件与时间结果。
- 船舶锚泊与航行互斥；严重损坏、缺少合格船员、无效航线、补给短缺和不合法命令 fail-closed，拒绝时不留下半提交状态。
- 补给扣减原子、资源不为负；同一 ApplicationId/tick 的重放不重复扣减或追加事件，冲突 payload 被拒绝。
- 事件批次按 tick/sequence 固定顺序原子提交；重复 `event_id + save_revision` 不重复消费，保存投影可从完整事件历史重建。
- D19 港口/泊位、D20/Crew、D3 货物、D9 环境和 D11 生态事实仍由各自 owner 写入；D21 只消费公开结果或提交公开命令。

## 4. D23 下游接口放行条件

D23 现在可以消费上述 DTO、公开命令结果和 `D21ShipSaveProjection`，但仍须遵守各 owner 和 Runtime Deferred 边界。D21 章节放行以完整结果/交接证据和最终 runner 报告为依据，不以局部 11-case 结果单独替代。

下游接线仍只能调用 `D21VoyageCommandService.Route`、`D21VoyageResolutionSystem.ResolveTick` 等公开入口，不能直接写 D3/D9/D19/D20/D11 或 D21 私有状态。

最终机器证据：`game/tools/results/D21/D21-L30/D21_L30_RUNNER_REPORT.json`，其中 `expected_results=32`、`present_results=32`、`present_handoffs=32`。

## 5. Runtime / Release 明确未放行

详见 [D21_RUNTIME_DEFERRED_REGISTER.md](D21_RUNTIME_DEFERRED_REGISTER.md)。未执行 Unity Player/PlayMode、场景/Prefab、真实驾驶、输入、相机、物理、海水 Shader、天气 VFX/UI、目标设备、性能、真实 Save 或发行验证；`RuntimeApproved=0`、`ReleaseApproved=0`。

## 6. 最终结论

当前 D21 逻辑实现、离线命令证据和结果交接链均已闭合：

**D21-L30 = `LOGIC_PASS`（9/9 suite、32/32 证据完整）。**
**D21-L31 = `LOGIC_PASS`（延期登记文档完成）。**
**D21-L32 = `LOGIC_PASS`（确定性航程接口可供 D23 只读消费）。**
