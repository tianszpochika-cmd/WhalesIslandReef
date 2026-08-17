# 第 12 章：D20 玩家、NPC、动物与动作逻辑（40 项）

> 本章实现角色身份、动作意图、行为状态、互动资格、保存和 Presentation 接口逻辑。模型、Avatar、Animator、Prefab、动作预览和正式港口接入全部延期。
>
> 前置：`D19-L01 LOGIC_PASS` 后可冻结 D20；涉及港口命令的整合任务必须等待 `D19-L48 LOGIC_PASS`。旧 D20-A01—A06 的视觉成果只作参考，不能作为本章通过证据。

## 复制给 Agent 的单任务执行提示词

```text
你是 WhalesIslandReef 的纯逻辑开发 Agent。直接执行任务 [TASK_ID]，不要等待确认，不要改做相邻任务。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前必须阅读：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md，以及本文件中 [TASK_ID] 的任务卡。

本任务只处理角色/NPC/动物的领域数据、命令、状态机、保存、事件和 Presentation 只读接口。禁止启动 Player/PlayMode，禁止修改模型、Animator、Prefab、场景、Canvas、输入和物理控制器。Systems 是强逻辑唯一裁决者；Presentation 只能读取状态和提出意图。

完成时写入 game/tools/results/D20/[TASK_ID]/[TASK_ID下划线]_LOGIC_RESULT.json、docs/game/design/common/handoffs/D20/[TASK_ID]_LOGIC_HANDOFF.md 和当天 .workbuddy/memory/YYYY-MM-DD.md；运行 python tools/sync_logic_task_status.py 与 git diff --check。最终仅报告真实逻辑状态。
```

## D20 的逻辑完成标准

- 角色外观、动作和表现不拥有身份、关系、金钱、库存、伤害、任务或位置裁决权；
- 玩家、NPC、宠物、野生动物都有稳定 ID、状态、意图、失败回退、事件和存档投影；
- 动作请求可按优先级、打断、暂停、死亡/昏迷、读档恢复处理；
- 不依赖 Animator、骨架、模型或场景即可测试；这些引用全部记录为 Runtime 延期。

## 任务卡与执行顺序

| ID | 波次 | 写入范围（唯一） | 实施内容与逻辑验收 |
|---|---|---|---|
| D20-L01 | S | `docs/game/design/common/D20_LOGIC_BOUNDARY_BASELINE.md` | 冻结玩家、NPC、动物、Presentation、D10/D11/D19 数据所有权和历史视觉成果隔离清单。 |
| D20-L02 | P | `game/Assets/Scripts/Systems/D20/D20CharacterIdentityContracts.cs` | 定义稳定角色 ID、身份、生命阶段、可用性与不可变字段。 |
| D20-L03 | P | `game/Assets/Scripts/Systems/D20/D20PlayerPresenceSystem.cs` | 定义玩家在港口/船上/航行/不可行动的语义状态，不读 Transform。 |
| D20-L04 | P | `game/Assets/Scripts/Systems/D20/D20ActionIntentContracts.cs` | 定义走、跑、交互、交谈、钓鱼、登船、掌舵、受击等动作意图契约。 |
| D20-L05 | P | `game/Assets/Scripts/Systems/D20/D20ActionPriorityRules.cs` | 定义动作优先级、互斥集、前置和可打断规则。 |
| D20-L06 | P | `game/Assets/Scripts/Systems/D20/D20ActionInterruptSystem.cs` | 实现暂停、受击、失能、场景语义切换和命令取消时的动作回退。 |
| D20-L07 | P | `game/Assets/Scripts/Systems/D20/D20PresentationEventContracts.cs` | 定义逻辑→表现的只读事件，禁止表现层写回角色事实。 |
| D20-L08 | P | `game/Assets/Scripts/Systems/D20/D20ActionReadModel.cs` | 生成 UI/Animator 将来可消费的意图、优先级、结果和 fallback 读模型。 |
| D20-L09 | P | `game/Assets/Scripts/Systems/D20/D20AppearanceSelectionRules.cs` | 验证发型、服装、体型等外观选择合法且不改变身份/关系/数值。 |
| D20-L10 | P | `game/Assets/Scripts/Systems/D20/D20IdentityPresentationRules.cs` | 规范姓名、称谓、代词、本地化和敏感历史表达的只读表现映射。 |
| D20-L11 | P | `game/Assets/Scripts/Systems/D20/D20TraitActionAdapter.cs` | 将既有特质以只读方式映射到动作资格/反馈，不重复裁决 TraitSystem。 |
| D20-L12 | P | `game/Assets/Scripts/Systems/D20/D20CharacterConditionAdapter.cs` | 将健康、疲劳、伤势等既有状态转化为动作限制和失败码。 |
| D20-L13 | P | `game/Assets/Scripts/Systems/D20/D20RelationActionAdapter.cs` | 将 D10 关系只读映射到对话/协作资格，禁止在此修改好感度。 |
| D20-L14 | P | `game/Assets/Scripts/Systems/D20/D20NpcIdentityContracts.cs` | 定义 NPC ID、岗位、阵营公开字段、可用性和弃用映射。 |
| D20-L15 | P | `game/Assets/Scripts/Systems/D20/D20NpcRoleCatalog.cs` | 建立商人、码头工、酒馆、船坞、守卫等岗位的逻辑目录。 |
| D20-L16 | P | `game/Assets/Scripts/Systems/D20/D20NpcScheduleInput.cs` | 定义时间、工作、危险、天气等输入，不实现寻路或动画。 |
| D20-L17 | P | `game/Assets/Scripts/Systems/D20/D20NpcBehaviorStateMachine.cs` | 实现 Idle/Work/Trade/Talk/Rest/Alarm/Refuse/Injured 等业务状态。 |
| D20-L18 | P | `game/Assets/Scripts/Systems/D20/D20NpcInteractionAvailability.cs` | 统一 NPC 交互的可用、忙碌、不可达、拒绝和冷却原因。 |
| D20-L19 | P | `game/Assets/Scripts/Systems/D20/D20NpcBehaviorAuthorityGuard.cs` | 扫描/测试 NPC 行为不得写经济、关系、库存、任务或 AgentCore。 |
| D20-L20 | P | `game/Assets/Scripts/Systems/D20/D20AnimalIdentityContracts.cs` | 定义动物 ID、物种、年龄、归属、野生/宠物与生命周期语义。 |
| D20-L21 | P | `game/Assets/Scripts/Systems/D20/D20AnimalSpeciesCatalog.cs` | 建立陆生、鸟类、水生、珍稀、宠物的 content_id/行为能力目录。 |
| D20-L22 | P | `game/Assets/Scripts/Systems/D20/D20AnimalBehaviorStateMachine.cs` | 实现休息、觅食、警觉、逃离、跟随、受伤、死亡等纯逻辑状态。 |
| D20-L23 | P | `game/Assets/Scripts/Systems/D20/D20PetOwnershipSystem.cs` | 实现领养、照料、跟随资格、丢失/找回和所有权约束。 |
| D20-L24 | P | `game/Assets/Scripts/Systems/D20/D20WildlifePopulationAdapter.cs` | 通过生态系统公开读模型处理种群/生境输入，禁止自己裁决生态数值。 |
| D20-L25 | P | `game/Assets/Scripts/Systems/D20/D20AquaticBehaviorAdapter.cs` | 定义鱼群/鲸豚等水生物的逻辑可见性和交互资格接口。 |
| D20-L26 | P | `game/Assets/Scripts/Systems/D20/D20CharacterSaveProjection.cs` | 将角色动作、NPC 行为、动物/宠物状态投影到既有存档。 |
| D20-L27 | P | `game/Assets/Scripts/Systems/D20/D20CharacterSaveMigration.cs` | 为缺失动作/NPC/动物字段提供版本迁移和安全回退。 |
| D20-L28 | S | `game/Assets/Scripts/Systems/D20/D20CharacterCommandService.cs` | 聚合玩家身份、状态、动作意图、条件和港口权限为唯一角色命令入口。 |
| D20-L29 | S | `game/Assets/Scripts/Systems/D20/D20NpcBehaviorService.cs` | 聚合岗位、日程、行为、可用性和事件，保持 D10/D19 所有权。 |
| D20-L30 | S | `game/Assets/Scripts/Systems/D20/D20AnimalBehaviorService.cs` | 聚合动物、宠物、生态读模型、交互资格和事件。 |
| D20-L31 | S | `game/Assets/Scripts/Systems/D20/D20ActionEventRouter.cs` | 保证命令、状态变化、表现事件和失败码顺序一致、可重放。 |
| D20-L32 | S | `game/Assets/Tests/EditMode/D20/D20ContentIdIntegrationTests.cs` | 验证角色/NPC/动物 content_id、资源 key、弃用映射和无候选路径依赖。 |
| D20-L33 | S | `game/Assets/Tests/EditMode/D20/D20CharacterSaveLoadTests.cs` | 覆盖动作、NPC 日程、宠物、野生状态的保存→加载等价性。 |
| D20-L34 | S | `game/Assets/Tests/EditMode/D20/D20DeterministicBehaviorTests.cs` | 使用固定种子模拟至少 1,000 Tick，验证状态可达、可恢复且无永久动作锁。 |
| D20-L35 | S | `game/Assets/Tests/EditMode/D20/D20BehaviorInvariantTests.cs` | 覆盖所有权、重复命令、死亡/取消、非法迁移、关系/经济不可越权等不变式。 |
| D20-L36 | G | `tools/d20_logic_boundary_audit.py` | 静态审计无 Animator、Transform、NavMesh、Prefab、场景、网络/LLM 和强逻辑越界。 |
| D20-L37 | G | `tools/d20_logic_test_runner.py` | 汇总并运行 D20 的纯逻辑测试，生成机器结果。 |
| D20-L38 | G | `docs/game/design/common/D20_LOGIC_INTERFACE.md` | 发布 D21—D23 可消费的角色/NPC/动物命令、读模型、事件和失败码。 |
| D20-L39 | G | `docs/game/design/common/D20_RUNTIME_DEFERRED_REGISTER.md` | 逐项登记模型、Avatar、Animator、Prefab、物理、寻路和视觉表现的后续风险。 |
| D20-L40 | G | `docs/game/design/common/D20_LOGIC_ACCEPTANCE.md` | 章节唯一逻辑验收；核对保存、行为模拟、边界审计和 D19 集成前置。 |

## 本章依赖细则

- L02—L15 为角色/NPC 基础契约，可在 L01 后并行；L16—L27 可并行，但只读其公开类型。
- L28→L35 是串行整合链。若 D19-L48 未通过，L28 对港口命令的部分应如实 `LOGIC_BLOCKED`，不绕过 D19。
- L36—L40 为汇合门禁；它们不验模型、动画、Prefab 或真实行为画面。
