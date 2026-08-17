# 第 13 章：D21 船舶、海况、天气与航海逻辑（32 项）

> 本章只完成船舶、海况、天气和航程的领域模型、数值规则、事件和保存。船体模型、海水、特效、驾驶输入和实际碰撞均延期。

## 复制给 Agent 的单任务执行提示词

```text
你是 WhalesIslandReef 的纯逻辑开发 Agent。直接执行任务 [TASK_ID]。阅读 AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md 和本文件的任务卡；只改卡中指定范围。

禁止启动 Player/PlayMode、修改船体/海水/天气表现、输入、场景、Prefab 或 Build Settings。所有航海数值只能由所属 Systems 裁决；Presentation 只读。完成后写 D21 唯一结果 JSON、handoff、记忆日志，运行状态同步和 git diff --check，并如实给出 LOGIC_* 结论。
```

## 任务卡与执行顺序

| ID | 波次 | 写入范围（唯一） | 实施内容与逻辑验收 |
|---|---|---|---|
| D21-L01 | S | `docs/game/design/common/D21_LOGIC_BOUNDARY_BASELINE.md` | 冻结 D2/D4/D9/D19/D20 航海边界、船数据所有权和 Runtime 延期清单。 |
| D21-L02 | P | `game/Assets/Scripts/Systems/D21/D21ShipIdentityContracts.cs` | 定义船只 ID、船型、所有权、船籍、状态与弃用映射。 |
| D21-L03 | P | `game/Assets/Scripts/Systems/D21/D21HullComponentSystem.cs` | 定义船体、帆、舵、锚、货舱等组件的逻辑状态与合法组合。 |
| D21-L04 | P | `game/Assets/Scripts/Systems/D21/D21ShipConditionSystem.cs` | 处理耐久、损伤、维修资格和不可航行状态，不接物理伤害。 |
| D21-L05 | P | `game/Assets/Scripts/Systems/D21/D21CargoCapacityRules.cs` | 校验货舱容量、重量、危险品和超载后果接口。 |
| D21-L06 | P | `game/Assets/Scripts/Systems/D21/D21CrewRequirementRules.cs` | 计算最低船员、岗位、士气/疲劳对航行资格的逻辑影响。 |
| D21-L07 | P | `game/Assets/Scripts/Systems/D21/D21AnchorMooringSystem.cs` | 定义下锚、起锚、系泊、漂泊和港口泊位的状态转换。 |
| D21-L08 | P | `game/Assets/Scripts/Systems/D21/D21NavigationRouteContracts.cs` | 定义航线、航点、距离、危险区和发现状态，不存 Transform 路径。 |
| D21-L09 | P | `game/Assets/Scripts/Systems/D21/D21PortDepartureAdapter.cs` | 经 D19 公共命令启动航程，禁止重写港口/泊位事实。 |
| D21-L10 | P | `game/Assets/Scripts/Systems/D21/D21PortReturnAdapter.cs` | 经 D19 公共命令结束航程并请求返港/靠泊。 |
| D21-L11 | P | `game/Assets/Scripts/Systems/D21/D21WeatherInputContracts.cs` | 定义季节、风向、风力、降水、能见度、气压的确定性输入。 |
| D21-L12 | P | `game/Assets/Scripts/Systems/D21/D21SeaStateSystem.cs` | 将天气输入映射为海况等级、风险、可航行性和事件概率。 |
| D21-L13 | P | `game/Assets/Scripts/Systems/D21/D21WeatherImpactRules.cs` | 定义天气对速度、燃料/补给、船员、钓鱼和港口可用性的只读影响。 |
| D21-L14 | P | `game/Assets/Scripts/Systems/D21/D21ProvisionConsumptionSystem.cs` | 处理航程补给、淡水、损耗、饥饿阈值和失败原因。 |
| D21-L15 | P | `game/Assets/Scripts/Systems/D21/D21VoyageTimeSystem.cs` | 以固定 Tick 计算航程时间、昼夜推进和到达估算。 |
| D21-L16 | P | `game/Assets/Scripts/Systems/D21/D21CourseCommandSystem.cs` | 校验设定航向、改航、停船、避险命令和冷却。 |
| D21-L17 | P | `game/Assets/Scripts/Systems/D21/D21VoyageRiskSystem.cs` | 计算暗礁、风暴、敌对、疾病等风险触发资格和确定性种子。 |
| D21-L18 | P | `game/Assets/Scripts/Systems/D21/D21VoyageEventContracts.cs` | 定义航程事件、结果、因果 ID 和可重放序列化格式。 |
| D21-L19 | P | `game/Assets/Scripts/Systems/D21/D21ShipSaveProjection.cs` | 将船、锚泊、航线、天气种子、补给和事件投影到 Save。 |
| D21-L20 | P | `game/Assets/Tests/EditMode/D21/D21ShipConditionTests.cs` | 覆盖船型/组件、损伤、维修、载重和不可航行不变式。 |
| D21-L21 | P | `game/Assets/Tests/EditMode/D21/D21WeatherSeaStateTests.cs` | 覆盖同一种子同一天气/海况结果、边界天气和风险资格。 |
| D21-L22 | P | `game/Assets/Tests/EditMode/D21/D21PortVoyageAdapterTests.cs` | 覆盖离港、返港、泊位占用和失败补偿的 D19 边界。 |
| D21-L23 | P | `game/Assets/Tests/EditMode/D21/D21VoyageSaveLoadTests.cs` | 覆盖航程中每个节点保存→加载→继续的等价性。 |
| D21-L24 | S | `game/Assets/Scripts/Systems/D21/D21VoyageCommandService.cs` | 汇合船况、船员、航线、天气、补给、港口适配和事件。 |
| D21-L25 | S | `game/Assets/Scripts/Systems/D21/D21VoyageResolutionSystem.cs` | 以固定 Tick 结算航速、消耗、风险、到达和失败，不依赖帧率。 |
| D21-L26 | S | `game/Assets/Scripts/Systems/D21/D21VoyageEventRouter.cs` | 保证航程事件顺序、幂等、补偿和存档投影一致。 |
| D21-L27 | S | `game/Assets/Tests/EditMode/D21/D21LongVoyageSimulationTests.cs` | 使用固定种子模拟至少 10,000 Tick，验证无负补给、无重复到达、无死锁状态。 |
| D21-L28 | S | `game/Assets/Tests/EditMode/D21/D21VoyageInvariantTests.cs` | 覆盖锚泊/航行互斥、损坏限制、错误权限和重复命令。 |
| D21-L29 | S | `tools/d21_logic_boundary_audit.py` | 阻止 D21 出现 Unity 物理、Input、Shader、场景/Prefab 和强逻辑越界。 |
| D21-L30 | G | `tools/d21_logic_test_runner.py` | 汇总并运行 D21 纯逻辑测试，写机器结果。 |
| D21-L31 | G | `docs/game/design/common/D21_RUNTIME_DEFERRED_REGISTER.md` | 登记真实驾驶、碰撞、船体、海水、天气表现、特效、性能等延期风险。 |
| D21-L32 | G | `docs/game/design/common/D21_LOGIC_ACCEPTANCE.md` | 章节逻辑验收，确认可向 D23 提供确定性航程接口。 |

## 依赖

L02—L23 在 L01 后可并行；L24—L29 串行整合；L30—L32 为汇合门禁。涉及 D19 港口适配的任务必须等待 D19-L48，其他纯船/天气任务可在 D20-L40 后与 D22 并行。
