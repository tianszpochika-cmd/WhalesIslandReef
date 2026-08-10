# D13-A05 岛屿治理与人口

## 目标

本切片以圣玛丽岛为代表，建立人口、家庭、住房、职业、供水、食物、教育、宗教、税费、工资、治安、政策、共同体协议和代理人管理之间的可保存因果链。治理系统只写自己的治理状态，外部系统通过带来源的 authority 提供产权、法律、经济、生态、社群和代理人裁决。

## 写者边界

| 状态 | 唯一写者 | A05 的使用方式 |
|---|---|---|
| 居民、家庭、迁入迁出、职业投影 | `IslandGovernanceSystem` | 写居民生命周期、家庭成员和岗位投影 |
| 健康、满意度、治安与治安事件 | `IslandGovernanceSystem` | 按日推进的确定性规则写入 |
| 产权、住房资格、法律争议 | `PropertySystem` / 法律 authority | `ID13GovernanceResidenceAuthority` 返回来源事实，A05 不改所有权 |
| 食物、供水、生态压力 | `EcologySystem` 或资源 authority | `ID13GovernanceProvisionAuthority` 返回数量和 Ecology 来源 |
| 税率、工资、税款和工资账本 | `EconomySystem` | `ID13GovernanceEconomyAuthority` 裁决金额、账本和来源 |
| 政策 | 法律/产权 authority | `ID13GovernancePolicyAuthority` 批准后才进入治理状态 |
| 共同体协议、社群关系和库存 | `CommunityTradeSystem` | A05 只保存协议的治理投影 |
| 代理人任命/管理结果 | `AgentManagementSystem` | A05 只保存分配投影 |

UI 只能提交 request 或读取 `D13GovernanceReadModel`；Agent 只能提交 `D13GovernancePolicyProposal` 等建议，不能直接改变居民、税率、工资、政策、奖励、金钱或库存。

## 人口与家庭

- 迁入必须同时通过 PropertySystem 资格、法律来源和宗教内容审查。
- 新家庭占用一个住房单位；同一 `HouseholdId` 的新成员共享家庭并写入 `MemberIds`。
- `HousingCapacity` 是迁入硬门槛；容量不足返回 `housing_full`，并提供产权/法律复核路线。
- 迁出将居民标记为 `Departed`，释放岗位和工资待付状态；家庭无成员时移除家庭投影。
- 居民状态包含角色（成人/儿童/老人）、教育等级、宗教内容引用、生命周期、最后事实和 revision。

## 工作、教育与供给因果

- 岗位具有槽位、工资、教育要求和治安贡献。岗位满或教育不足时拒绝分配。
- 岗位工资由 Economy authority 返回；治理系统只记录 `WageDue` 和支付凭证，不写金钱。
- 教育受政策开关和教育容量控制；每次入学提升教育等级和满意度，最高等级为 3，避免无界增长。
- 每日推进根据活跃居民数量计算食物/水需求，需求、实际供给及 Ecology 来源写入 `D13GovernanceProvisionRecord`。
- 缺粮时健康 -10、满意度 -6；缺水时健康 -12、满意度 -7；失业满意度 -4；已有教育且教育开放时满意度 +2。健康降至 0 时居民进入 `Deceased`。
- 食物和水的数量不是 UI 或 Agent 生成的，缺给结果必须可由外部资源 authority 复核。

## 税、工资与政策

- 政策包含税率、食物/水配给等级、教育开关、宗教安排和治安姿态；税率范围为 0—5000 bps。
- `ApplyPolicy` 只接受法律或产权 authority 的批准，并记录批准事件；`ProposePolicy` 只返回 Agent 提案，不写入政策。
- 税费评估把税基、税率、应缴和实收写入事实；实际收缴仍由 EconomySystem 处理。
- 工资结算必须匹配居民当前 `WageDue`，由 EconomySystem 提供金额、币种、账本和来源；重复支付返回 `duplicate`。

## 宗教、社群与历史审查

岛屿、宗教和共同体协议均带 `ContentId`、`HistoricalTag`、`SensitiveContentLabel`、`ContentWarning`、`ReviewStatus`、`SourceNote` 和标签。`ApprovedWithWarning` 必须由请求显式确认；`PendingReview` 或 `Blocked` 不得登记/实践。宗教实践只保存经过审查的语境事实，不把信仰简化成阵营或善恶。共同体协议只保存来自 `CommunityTradeSystem` 的投影，不复制社群库存或声望。

## 治安与恢复

治安评分由岛屿基础值、守卫岗位贡献、供给短缺、治安姿态和未解决事件组成。每日事件由岛屿、世界日和请求 seed 的稳定 FNV 规则决定；短缺提高事件概率。事件有严重度、来源世界日和恢复路线；`ResolveSecurityIncident` 要求显式 `ResolutionId`，解决后才恢复评分。没有来源或恢复路线的治安裁决不会被接受。

## 存档与幂等

`D13GovernanceRuntimeSnapshot` 保存家庭、居民、政策、共同体协议、供给记录、税费、工资、治安事件、代理人分配、最后世界日、最后事件和 `AppliedEventKeys`。`SaveSystem.SaveD13GovernanceRuntime` 只校验并复制快照；编解码位于既有 `location_json.d13Governance`，不拥有任何领域裁决。操作使用 `event_id + save_revision`，读档后重复迁入、重复工资、重复税费或重复日推进不会再次改变状态。

## 验证

- `D13IslandGovernanceSystemTests`：完整人口/治理链与存档幂等、缺粮缺水/失业导致健康满意度下降和治安恢复、住房/岗位/authority 边界。
- `tools/d13_a05_runner`：3/3 PASS。
- Systems、Save.Tests 编译 0 warning/0 error；D13 治理代码不含网络或 LLM。
