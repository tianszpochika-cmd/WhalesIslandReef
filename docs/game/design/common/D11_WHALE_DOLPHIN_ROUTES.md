# D11-A04 鲸豚路线与捕鲸分支

状态：完成（2026-08-11）  
任务：D11-A04  
前置：D9-S02 已通过；D11-A01 生态状态机已接入当前工作区。

## 1. 历史标签与敏感内容审查

捕鲸路线的固定审查标签为：

- `historical_review_tag`: `history_1675_caribbean_whaling`
- `sensitive_content_review`: `sensitive_whaling_non_glorifying_review_required`

捕鲸不是普通捕鱼，也不是一个点击即发钱的按钮。路线必须先有迁徙或观测事实，再经许可证、季节、法律/势力政策、船员数量、船型、处理设施、社会声望和生态压力复核。失败没有收益，并留下船体/船员风险和恢复路线；保护种群、政策禁止、许可证缺失或生态压力过高时不能进入捕鲸结算。

本任务没有批量增加鲸豚物种、地区或历史事件内容，只提供稳定 DTO、确定性状态机和一个可审查的代表性路线。

## 2. 唯一写者和读取边界

| 数据 | 本任务唯一写者 | 读取者/后续结算者 | 本任务是否直接修改 |
|---|---|---|---|
| 鲸群迁徙/海豚路线投影 | `D11WhaleDolphinSystem`（D11 路线投影） | EcologySystem、QuestSystem、UI、Agent read model | 只写 D11 投影 |
| 野生动物、生境压力、保护种群 | `EcologySystem`（D11-A01） | D11 路线系统只读 | 否 |
| 天气、能见度、海流 | `EnvironmentSystem`（D9） | D11 路线系统只读 | 否 |
| 许可证、法律政策、势力声望 | Faction/Legal owner | D11 路线系统只读 | 否 |
| 船型、船体风险、船员风险 | Ship/Crew owner | D11 路线系统只读条件，失败输出请求 | 否 |
| 金钱、鱼货、鲸体处理收益 | Economy/Inventory/Reward owner | RewardSystem 消费待结算请求 | 否 |
| 任务完成状态 | QuestSystem | QuestSystem 消费 `QuestFacts` | 否 |
| D11 路线事实和待结算请求 | `D11WhaleDolphinSystem` | Quest、Reward、Ecology、Faction、Ship、UI | 是 |

UI 和 Agent 只能提交 `D11WhaleDolphinActionRequest`，不能直接发放奖励、改许可证、改声望、改生态或标记捕鲸成功。

## 3. 稳定实体和存档字段

代码位置：

- `D11WhaleDolphinContracts.cs`：稳定 ID、枚举、输入/输出和存档 DTO；schema version=`1`。
- `D11WhaleDolphinSystem.cs`：唯一的 D11 鲸豚路线投影写入口。
- `SaveSystem.cs`：在现有 `location_json` 下读写 `d11`，旧档缺少该字段时使用空快照默认值。

`D11WhaleDolphinRuntimeSnapshot` 保存：

```text
schemaVersion, worldDay, revision, lastEventId,
appliedEventKeys[],
encounters[], routeRecords[], rewardRequests[],
consequenceRequests[], questFacts[]
```

`encounters[]` 保存稳定 `encounterId`、`speciesId/speciesKind`、`routeRef`、生境引用、确定性位置、迁徙方向/日期、生态压力镜像、保护/搁浅/观察/伴航/救助/捕鲸/放弃追踪标记和 cause event IDs。

`routeRecords[]` 保存动作、状态、代码、事件 ID、风险 roll/阈值、生态压力变化、社会声望变化、法律结果代码、替代/恢复路线、历史标签和敏感内容审查状态。

所有请求以 `event_id + save_revision` 幂等；重复执行只返回 duplicate，不再次添加路线记录、奖励请求或世界后果。

## 4. 四条路线

### 4.1 迁徙

`AdvanceMigration` 读取 A01 生态快照和 D9 海况快照，使用 encounter、路线、世界日和 seed 的稳定哈希计算位置与方向。天气/海流只进入记录值，不被改写。迁徙事件示例：`event_d11_<encounter>_migration_<day>`。

### 4.2 海豚伴航与观察

伴航只接受海豚 encounter，并要求能见度足够；结果产生 `dolphin_route_chart` 请求。观察产生 `observation_log` 请求和 Quest fact，能见度过低时拒绝，不伪造观测。

### 4.3 搁浅救助

救助必须有 EcologySystem 提供的 `IsStranded` 事实、救助工具和至少一名船员。D11 记录救助成功，并提出：

- `EcologySystem`：生态压力 `-0.20` 的恢复请求；
- `FactionSystem/RewardSystem`：社会声望 `+5` 的记录/表彰请求；
- QuestSystem：`stranding_rescue_completed` 事实。

实际数值写入仍由所属系统负责。

### 4.4 捕鲸、失败和放弃追踪

捕鲸必须同时满足：

1. 有 `Valid` 许可证和非空 `licenseId`；
2. 法律政策为 `CommercialAllowed` 或 `SubsistenceOnly`；
3. 季节满足要求；
4. 船员数量达到要求；
5. 船型等于要求船型；
6. 有处理设施和已准备的设备；
7. 社会声望达到门槛；
8. 目标不是保护种群，生态压力不超过上限；
9. 同一 encounter 没有既有捕鲸尝试。

通过条件后，仍按稳定哈希进行风险判定。成功只生成 `licensed_whale_processing` 待结算请求，并记录生态压力 `+0.18`、社会声望影响和合法记录；RewardSystem、Inventory/Cargo、EcologySystem、FactionSystem 后续各自裁决。失败不生成收益，请求中记录船体风险 `+0.15` 和船员风险 `+1`，并给出 `abandon_tracking_or_rescue` 恢复路线。

放弃追踪始终可用，生成 `abandon_tracking_alternative` 替代奖励请求（观察记录或路线图），捕鲸收益明确为 `0`，不会重复发放捕鲸收益。

## 5. 事件追溯

所有已裁决路线事件写入共享 D8 `EventLog`，owner 标记为 `EcologySystem`，并包含：路线动作、历史审查标签、稳定 encounter 引用、结果代码、法律/生态/社会/风险值和恢复路线。QuestSystem 可通过 `QuestFacts` 和 EventLog 的 cause chain 追溯；UI/Agent 只读副本。

## 6. 测试入口

Unity EditMode：`D11WhaleDolphinRoutesTests.cs`，覆盖迁徙确定性、观察、海豚伴航、搁浅救助、放弃替代奖励、捕鲸全条件、捕鲸成功、重复结算和缺字段/未知枚举默认值。

独立烟测：`tools/d11_a04_smoke/D11A04Smoke.csproj`，覆盖迁徙、合法捕鲸、重复门和快照校验。

禁止项静态检查：D11-A04 文件不引用 Unity、网络、LLM、`GameObject`、`Renderer` 或 `MonoBehaviour`。
