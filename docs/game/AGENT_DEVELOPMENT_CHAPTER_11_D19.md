# 第 11 章：D19 港口业务闭环逻辑（48 项）

> 本章将“到港—靠泊—港口服务—登船—出航—返港—保存”写成可测试的领域逻辑。它不修改港口场景、HUD、输入、碰撞、NPC 外观或 Player 路线。
>
> 前置：`D18-L53 LOGIC_PASS`（且重核后的 `D18-L50 LOGIC_PASS`）后可开始 `D19-L01`。D19 只读取 D18 的静态逻辑资源接口与稳定键规则；模型导入、Prefab、场景、画面、UI、性能和 Player 路线仍是 Runtime Deferred。D19 的任何任务不等待截图、FPS 或旧 R03 报告。

> 可直接复制的完整指令在 [D19 详细提示词（48 项）](AGENT_DEVELOPMENT_CHAPTER_11_D19_PROMPTS.md)。不要再使用本文件旧的 `[TASK_ID]` 通用模板代替详细提示词。

## 通用协议（详细提示词已包含）

```text
你是 WhalesIslandReef 的纯逻辑开发 Agent。直接执行任务 [TASK_ID]，不要等待确认，不要改做相邻任务。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前必须阅读：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md，以及本文件中 [TASK_ID] 的任务卡。

只在任务卡允许的 D19 领域类、契约、单元测试、确定性模拟和文档中工作。禁止启动 Unity Player/PlayMode，禁止改 InitialHarbor、HUD/Canvas、输入、碰撞、Prefab、Builder 或性能配置。不得让 Presentation、UI 或场景对象裁决市场、船只、库存、保存或航程事实。

完成时写入 game/tools/results/D19/[TASK_ID]/[TASK_ID下划线]_LOGIC_RESULT.json、docs/game/design/common/handoffs/D19/[TASK_ID]_LOGIC_HANDOFF.md 和当天 .workbuddy/memory/YYYY-MM-DD.md；运行 python tools/sync_logic_task_status.py 与 git diff --check。最终如实报告 LOGIC_PASS、LOGIC_PARTIAL 或 LOGIC_BLOCKED。
```

## D19 的逻辑完成标准

- 任何港口命令有前置条件、成功结果、失败原因、事件和存档投影；
- 重复命令、非法状态、缺货、泊位冲突、保存中断和读档恢复都有确定性行为；
- D2/D3/D4/D9 已有系统保持数据所有权，D19 只通过公开契约编排；
- 测试可在无 Unity 场景、无键鼠输入的进程中重放完整港口剧本；
- 所有位置、碰撞、路径、UI 可读性和真实交互手感列为 `RUNTIME_DEFERRED`。

## 任务卡与执行顺序

| ID | 波次 | 写入范围（唯一） | 实施内容与逻辑验收 |
|---|---|---|---|
| D19-L01 | S | `docs/game/design/common/D19_LOGIC_BOUNDARY_BASELINE.md` | 盘点现有 D2/D3/D4/D9/Save 接口，冻结港口领域边界、禁止写者和旧 R03 隔离清单。 |
| D19-L02 | P | `game/Assets/Scripts/Systems/D19/D19PortLifecycleContracts.cs` | 定义 PortVisit、Docked、Berthed、InService、Boarded、Departed、Returned 的契约和迁移原因。 |
| D19-L03 | P | `game/Assets/Scripts/Systems/D19/D19PortVisitSystem.cs` | 实现纯业务的到港/离港访问状态机，不触碰 Transform、NavMesh 或场景。 |
| D19-L04 | P | `game/Assets/Scripts/Systems/D19/D19PortAnchorCatalog.cs` | 定义稳定 port/berth/service 锚点 ID、别名和废弃映射；不读取 Unity 坐标。 |
| D19-L05 | P | `game/Assets/Scripts/Systems/D19/D19PortAvailabilitySystem.cs` | 依据时间、势力、天气逻辑输入和设施状态计算服务可用性与失败原因。 |
| D19-L06 | P | `game/Assets/Scripts/Systems/D19/D19BerthSystem.cs` | 实现泊位预留、占用、释放、冲突和租期的原子逻辑。 |
| D19-L07 | P | `game/Assets/Scripts/Systems/D19/D19DockingRules.cs` | 校验船只状态、港口访问、泊位能力和靠泊前置；不接入物理碰撞。 |
| D19-L08 | P | `game/Assets/Scripts/Systems/D19/D19BoardingWorkflow.cs` | 定义登船/下船请求、成员位置语义、失败回退和幂等结果。 |
| D19-L09 | P | `game/Assets/Scripts/Systems/D19/D19DepartureWorkflow.cs` | 定义离港检查、离泊、航程启动和失败补偿，不写 SailingSystem 内部数值。 |
| D19-L10 | P | `game/Assets/Scripts/Systems/D19/D19ArrivalWorkflow.cs` | 定义返港、泊位分配、服务恢复和航程结束的确定性流程。 |
| D19-L11 | P | `game/Assets/Scripts/Systems/D19/D19MarketPortAdapter.cs` | 通过 D3 公共契约封装港口市场买卖，保留货币/库存裁决者。 |
| D19-L12 | P | `game/Assets/Scripts/Systems/D19/D19WarehousePortAdapter.cs` | 编排港口仓库取放、容量和物品所有权，不复制库存系统。 |
| D19-L13 | P | `game/Assets/Scripts/Systems/D19/D19TavernPortAdapter.cs` | 连接酒馆社交/招募的公开命令，定义可用性、费用和失败反馈。 |
| D19-L14 | P | `game/Assets/Scripts/Systems/D19/D19ShipyardPortAdapter.cs` | 连接船坞检修/改造公开命令，禁止越权修改船舶数值。 |
| D19-L15 | P | `game/Assets/Scripts/Systems/D19/D19PreparationPortAdapter.cs` | 连接补给、船员、货舱和出航准备检查的只读/命令接口。 |
| D19-L16 | P | `game/Assets/Scripts/Systems/D19/D19PortEventContracts.cs` | 定义可重放的港口事件 ID、顺序、关联命令 ID 和序列化格式。 |
| D19-L17 | P | `game/Assets/Scripts/Systems/D19/D19PortFailureCatalog.cs` | 定义统一失败码、可本地化原因和重试建议，禁止直接写 UI 字符串。 |
| D19-L18 | P | `game/Assets/Scripts/Systems/D19/D19PortLocalizationKeys.cs` | 建立 D19 行为结果使用的本地化 key 清单和缺 key 静态检查。 |
| D19-L19 | P | `game/Assets/Scripts/Systems/D19/D19PortReadModel.cs` | 输出 UI/Presentation 可只读消费的港口状态、服务和反馈模型。 |
| D19-L20 | P | `game/Assets/Scripts/Systems/D19/D19PortSaveProjection.cs` | 将访问、泊位、服务、登船和未完成命令投影到既有 Save 模型。 |
| D19-L21 | P | `game/Assets/Scripts/Systems/D19/D19PortSaveMigration.cs` | 定义旧存档缺 D19 字段时的安全默认/迁移策略和兼容测试。 |
| D19-L22 | P | `game/Assets/Scripts/Systems/D19/D19CommandIdempotency.cs` | 对市场、登船、靠泊、离港和保存命令实现重复投递的稳定结果。 |
| D19-L23 | P | `game/Assets/Scripts/Systems/D19/D19PortAuthorityValidator.cs` | 校验玩家、船、港口、货物和服务权限；拒绝跨实体写入。 |
| D19-L24 | P | `game/Assets/Scripts/Systems/D19/D19InvalidStateGuard.cs` | 给所有生命周期迁移建立非法状态拒绝和不变式检查。 |
| D19-L25 | P | `game/Assets/Tests/EditMode/D19/D19MarketPortAdapterTests.cs` | 测试买卖成功、缺钱、缺货、重复命令和事件结果。 |
| D19-L26 | P | `game/Assets/Tests/EditMode/D19/D19WarehousePortAdapterTests.cs` | 测试入库、取库、容量、归属和保存投影。 |
| D19-L27 | P | `game/Assets/Tests/EditMode/D19/D19TavernPortAdapterTests.cs` | 测试服务可用/不可用、费用、关系前置和失败原因。 |
| D19-L28 | P | `game/Assets/Tests/EditMode/D19/D19ShipyardPortAdapterTests.cs` | 测试修理/改造请求的前置、拒绝和原系统裁决边界。 |
| D19-L29 | P | `game/Assets/Tests/EditMode/D19/D19BerthDockingTests.cs` | 测试预留竞争、占用、超期、释放和船型不兼容。 |
| D19-L30 | P | `game/Assets/Tests/EditMode/D19/D19BoardingDepartureTests.cs` | 测试登船、下船、离泊、失败补偿和命令幂等。 |
| D19-L31 | P | `game/Assets/Tests/EditMode/D19/D19ArrivalReturnTests.cs` | 测试返港、泊位再分配、航程终止和港口服务恢复。 |
| D19-L32 | P | `game/Assets/Tests/EditMode/D19/D19AvailabilityAndNpcTests.cs` | 测试天气/时间/势力逻辑输入对服务和 NPC 可用性的确定性影响。 |
| D19-L33 | S | `game/Assets/Scripts/Systems/D19/D19PortCommandRouter.cs` | 将 L02—L24 汇合为唯一命令入口，保证授权、事件、失败和存档顺序。 |
| D19-L34 | S | `game/Assets/Scripts/Systems/D19/D19PortCompensation.cs` | 为中途失败的市场、泊位、登船、离港流程建立可审计补偿策略。 |
| D19-L35 | S | `game/Assets/Scripts/Systems/D19/D19PortEventOrderAudit.cs` | 校验事件序列单调、无重复副作用、可从事件重建只读状态。 |
| D19-L36 | S | `game/Assets/Tests/EditMode/D19/D19DeterministicPortScenarioTests.cs` | 写无 Unity 场景的港口剧本：到港→服务→登船→离港→返港。 |
| D19-L37 | S | `game/Assets/Tests/EditMode/D19/D19PortFuzzInvariantTests.cs` | 以固定随机种子模拟至少 1,000 次命令，验证无负库存、孤儿泊位或非法状态。 |
| D19-L38 | S | `game/Assets/Tests/EditMode/D19/D19PortSaveLoadTests.cs` | 覆盖每个生命周期节点的保存→加载→继续命令等价性。 |
| D19-L39 | S | `game/Assets/Tests/EditMode/D19/D19LegacySaveTests.cs` | 用历史存档夹具验证迁移、缺字段回退和不可逆错误提示。 |
| D19-L40 | S | `tools/d19_logic_boundary_audit.py` | 扫描 D19 新代码，阻止 Unity Transform、Input、Canvas、PlayerConnection、网络/LLM 和跨系统私写。 |
| D19-L41 | S | `tools/d19_logic_test_runner.py` | 统一运行 D19 可执行的纯逻辑测试，输出机器可读汇总而非 Player 日志。 |
| D19-L42 | S | `docs/game/design/common/D19_LOGIC_PORT_INTERFACE.md` | 发布 D20—D23 可用的命令、事件、读模型、错误码和 Runtime 延期项。 |
| D19-L43 | G | `game/tools/results/D19/D19-L43/` | 复核 contracts、保存投影、失败码和各领域单元测试的 P0 证据。 |
| D19-L44 | G | `docs/game/design/common/D19_LOGIC_PERSISTENCE_AUDIT.md` | 复核所有 D19 状态是否能保存、迁移、加载并保持数据所有权。 |
| D19-L45 | G | `docs/game/design/common/D19_LOGIC_WORKFLOW_AUDIT.md` | 复核完整确定性剧本、补偿、事件重放和不变式结果。 |
| D19-L46 | G | `docs/game/design/common/D19_RUNTIME_DEFERRED_REGISTER.md` | 列出必须之后实测的锚点、碰撞、输入、HUD、路线、画面和性能。 |
| D19-L47 | G | `docs/game/design/common/D19_LOGIC_STATUS_REVIEW.md` | 审计全部结果 JSON、handoff、记忆日志和状态总览。 |
| D19-L48 | G | `docs/game/design/common/D19_LOGIC_ACCEPTANCE.md` | 章节唯一逻辑验收；仅以代码/测试证据决定对 D20、D23 的放行。 |

## 本章依赖细则

- `L01` 是唯一的边界冻结任务；通过前不得创建 D19 系统、测试或“临时接口”。
- `L02`、`L04`、`L16`、`L17`、`L18` 在 L01 后可并行；它们分别拥有独立文件，禁止互改。
- `L03`、`L05`、`L06`、`L23`、`L24` 只能在 19.1 的直接前置任务都为 `LOGIC_PASS` 后并行。它们只能消费公共契约，找不到契约就如实 `LOGIC_BLOCKED`，不得复制类型或抢改前置文件。
- `L07`、`L08`、`L11`—`L15`、`L22` 只能在其详细提示词列出的领域实现为 `LOGIC_PASS` 后并行；`L09`、`L10`、`L19`、`L20` 在 19.3 通过后并行；`L21` 必须等待 L20。
- `L25`—`L32` 是分文件的领域测试，只在相应生产代码/契约及 L21 已通过后并行。测试失败应如实失败，不得在测试任务中越权改生产文件。
- `L33`—`L42` 是严格串行的唯一整合线：命令路由 → 补偿 → 事件审计 → 剧本 → 模糊测试 → 存档往返 → 旧档迁移 → 边界审计 → 统一 runner → 对外接口。
- `L43`—`L48` 是严格串行的唯一汇合门禁。它们从不启动 Player，也不因旧运行时 `PARTIAL` 重跑 R03；任何 P0 未通过时必须停止并写出真实缺口。
