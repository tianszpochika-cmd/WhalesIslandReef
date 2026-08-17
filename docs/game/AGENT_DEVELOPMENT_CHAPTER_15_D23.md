# 第 15 章：D23 跨系统确定性完整循环（38 项）

> 本章用代码剧本证明“新游戏到返港经营并保存继续”的业务闭环。它不是 30—45 分钟真人试玩，也不产生截图、Player 或性能证据。

## 复制给 Agent 的单任务执行提示词

```text
你是 WhalesIslandReef 的纯逻辑开发 Agent。直接执行任务 [TASK_ID]。阅读 AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md 和本文件任务卡。

本章只能使用已通过的 D18—D22 公开接口，在确定性测试/模拟中编排业务流程。不得启动 Player/PlayMode、改场景/UI/Prefab/输入/画面，也不得绕过任一系统直接改库存、金钱、关系、船舶或存档。

完成时写 D23 结果 JSON、handoff、记忆日志，运行状态同步和 git diff --check；只有自动化逻辑证据可产生 LOGIC_PASS。
```

## 任务卡与执行顺序

| ID | 波次 | 写入范围（唯一） | 实施内容与逻辑验收 |
|---|---|---|---|
| D23-L01 | S | `docs/game/design/common/D23_LOGIC_SCENARIO_BASELINE.md` | 冻结跨系统剧本的参与模块、种子、初始数据、禁止捷径和 Runtime 延期范围。 |
| D23-L02 | P | `game/Assets/Tests/EditMode/D23/D23ScenarioFixtureFactory.cs` | 创建固定种子、最小合法港口/船/角色/库存/天气测试夹具。 |
| D23-L03 | P | `game/Assets/Tests/EditMode/D23/D23NewGameScenario.cs` | 验证新游戏建立稳定 ID、初始存档、船、玩家和港口访问状态。 |
| D23-L04 | P | `game/Assets/Tests/EditMode/D23/D23CharacterSetupScenario.cs` | 验证身份/外观选择只影响允许字段并可保存恢复。 |
| D23-L05 | P | `game/Assets/Tests/EditMode/D23/D23PortEntryScenario.cs` | 验证到港、服务可用性、泊位和港口读模型一致。 |
| D23-L06 | P | `game/Assets/Tests/EditMode/D23/D23MarketScenario.cs` | 验证市场买卖、资金、库存、事件和失败补偿。 |
| D23-L07 | P | `game/Assets/Tests/EditMode/D23/D23WarehouseScenario.cs` | 验证货物存取、容量、归属和后续航行载重。 |
| D23-L08 | P | `game/Assets/Tests/EditMode/D23/D23TavernScenario.cs` | 验证酒馆社交/招募、费用、关系与失败反馈。 |
| D23-L09 | P | `game/Assets/Tests/EditMode/D23/D23ShipyardScenario.cs` | 验证维修/改造请求和船况变化的跨系统约束。 |
| D23-L10 | P | `game/Assets/Tests/EditMode/D23/D23PreparationScenario.cs` | 验证补给、船员、货舱、天气和离港前置检查。 |
| D23-L11 | P | `game/Assets/Tests/EditMode/D23/D23BoardingScenario.cs` | 验证玩家/船员登船、下船和位置语义转换。 |
| D23-L12 | P | `game/Assets/Tests/EditMode/D23/D23DepartureScenario.cs` | 验证离泊、航程启动、港口占用释放和失败补偿。 |
| D23-L13 | P | `game/Assets/Tests/EditMode/D23/D23VoyageScenario.cs` | 验证固定种子航行、天气/海况、补给、风险、采集或代表事件。 |
| D23-L14 | P | `game/Assets/Tests/EditMode/D23/D23ReturnScenario.cs` | 验证返港、靠泊、服务恢复、售卖和事件收束。 |
| D23-L15 | P | `game/Assets/Tests/EditMode/D23/D23EventLogScenario.cs` | 验证全链路事件顺序、关联 ID、无重复副作用和重放读模型。 |
| D23-L16 | P | `game/Assets/Tests/EditMode/D23/D23SavePointScenario.cs` | 在新游戏、港口服务、航程中、返港后分别验证保存/加载。 |
| D23-L17 | P | `game/Assets/Tests/EditMode/D23/D23TimeAdvanceScenario.cs` | 验证时间推进同时满足港口、NPC、天气、航程和经济边界。 |
| D23-L18 | P | `game/Assets/Tests/EditMode/D23/D23FailureRecoveryScenario.cs` | 验证缺钱、缺货、满仓、无泊位、坏船、恶劣天气和中断恢复。 |
| D23-L19 | P | `game/Assets/Tests/EditMode/D23/D23CurrencyInventoryInvariantTests.cs` | 验证跨流程无货币凭空增加、无负库存、无重复奖励。 |
| D23-L20 | P | `game/Assets/Tests/EditMode/D23/D23PetAnimalScenario.cs` | 验证宠物/动物状态与航行、港口、生态读模型的安全边界。 |
| D23-L21 | P | `game/Assets/Tests/EditMode/D23/D23FishingCollectionScenario.cs` | 验证钓鱼/采集进入库存、事件和经济但不绕过生态裁决。 |
| D23-L22 | P | `game/Assets/Tests/EditMode/D23/D23FactionConsequenceScenario.cs` | 验证势力/法律后果穿过港口服务与航行资格的接口。 |
| D23-L23 | P | `game/Assets/Tests/EditMode/D23/D23UiCommandScenario.cs` | 验证 D22 命令/读模型可表达剧本结果，不接 Canvas。 |
| D23-L24 | P | `game/Assets/Tests/EditMode/D23/D23ContractCompatibilityTests.cs` | 验证跨系统 DTO/Schema/事件版本兼容。 |
| D23-L25 | P | `docs/game/design/common/D23_LOGIC_SCENARIO_CATALOG.md` | 记录每个剧本步骤、所有者、输入、预期事件与 Runtime 对应实测项。 |
| D23-L26 | S | `game/Assets/Tests/EditMode/D23/D23FullLoopScenarioTests.cs` | 将 L03—L24 汇合为唯一新游戏→返港经营→保存→继续的确定性主剧本。 |
| D23-L27 | S | `game/Assets/Tests/EditMode/D23/D23ReplayDeterminismTests.cs` | 同种子/同命令重复至少 20 次，要求事件、存档摘要和结果完全一致。 |
| D23-L28 | S | `game/Assets/Tests/EditMode/D23/D23InterruptionMatrixTests.cs` | 在每个关键步骤注入失败/加载，验证补偿与恢复无数据损失。 |
| D23-L29 | S | `game/Assets/Tests/EditMode/D23/D23LongRunScenarioTests.cs` | 连续运行至少 50 个逻辑循环，验证状态不会漂移或泄漏。 |
| D23-L30 | S | `tools/d23_cross_system_boundary_audit.py` | 审计剧本不直接修改各系统私有状态、不依赖 Unity 或候选资源路径。 |
| D23-L31 | S | `tools/d23_scenario_runner.py` | 统一执行固定种子剧本、重放、长跑和中断矩阵，输出 JSON。 |
| D23-L32 | S | `docs/game/design/common/D23_LOGIC_FULL_LOOP_INTERFACE.md` | 发布主剧本步骤、断点、存档摘要、事件序列和 Runtime 测试映射。 |
| D23-L33 | S | `docs/game/design/common/D23_RUNTIME_DEFERRED_REGISTER.md` | 登记真人路线、输入、镜头、碰撞、UI、画面、性能和时长实测项。 |
| D23-L34 | S | `docs/game/design/common/D23_LOGIC_DEBT_REGISTER.md` | 区分逻辑 blocker、非 P0 缺口与 Runtime Deferred，禁止把旧 RERUN 抄入 blocker。 |
| D23-L35 | G | `game/tools/results/D23/D23-L35/` | 审核主剧本、重放、长跑、中断、跨系统边界的机器证据。 |
| D23-L36 | G | `docs/game/design/common/D23_LOGIC_SAVE_AUDIT.md` | 审核全剧本任意断点保存/加载、迁移和数据所有权。 |
| D23-L37 | G | `docs/game/design/common/D23_LOGIC_STATUS_REVIEW.md` | 审核本章全部结果、handoff、日志和状态同步。 |
| D23-L38 | G | `docs/game/design/common/D23_LOGIC_ACCEPTANCE.md` | 章节逻辑验收；允许 D24 以此作为完整业务闭环基线。 |

## 依赖

L02—L25 必须等待 D18-L50、D19-L48、D20-L40、D21-L32、D22-L36 全部 `LOGIC_PASS` 后并行。L26—L34 串行，L35—L38 汇合。任何上游 `LOGIC_PARTIAL` 都不能由 D23 伪造补齐。
