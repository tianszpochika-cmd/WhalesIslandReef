# D20 NPC 行为表现适配

任务：D20-A04
状态：`PARTIAL` / `Candidate`
日期：2026-08-15

## 结论与范围

已交付独立的 NPC Presentation Runtime、NPC 动作映射、四岗位视觉锚点、头顶反馈提示、低距降级、快照恢复和专属演示 Prefab。A03 的 Prefab、interface JSON 和资源 key 只读消费，未修改 A03 目录、NPCSystem、AgentCore、生成器、Registry、存档、契约或 InitialHarbor。

本批结论为 `PARTIAL`：A04 专项 Unity 隔离导入测试 7/7 通过，但主工程当时被并行 D19 编辑器占用，未在本轮覆盖 InitialHarbor 的完整 PlayMode 日程路线；正式场景接入仍由后续同步集成任务负责。该限制不影响本批 Presentation Runtime 的编译和专项行为映射证据。

## 只读输入契约

`D20NpcPresentationSnapshot` 是表现层唯一输入，包含：`npcId`、`resourceKey`、行为状态、岗位、世界位置、速度、接地、对话状态、忙碌、受伤、暂停、导航状态、LOD、视觉锚点 ID 和由上游提供的反馈 key。

快照不含工资、金钱、库存、价格、关系、任务、奖励、时间推进或存档写入字段。`D20NpcReadOnlyPresentationAdapter` 只写 Animator 参数、Visual 姿态、Renderer/LOD、FarProxy 和反馈文本；不会调用领域系统、EventLog、SaveSystem、NPCSystem、AgentCore、网络或 LLM。

## 状态映射

| 领域状态 | Animator state | 无动作/无 Controller 回退 | 视觉表现 |
| --- | --- | --- | --- |
| `Idle` | `D20_NPC_Idle` | neutral pose / breathing | 中立站姿 |
| `Work` | `D20_NPC_Work` | work bob + 岗位工作提示 | 工作姿态和岗位轮廓 |
| `Walk` | `D20_NPC_Walk` | walk bob | 位置由快照提供，表现不改导航 |
| `Talk` | `D20_NPC_Talk` | talk lean | 面向/对话姿态 |
| `Rest` | `D20_NPC_Rest` | rest breathing | 休息姿态 |
| `Eat` | `D20_NPC_Eat` | eat lean | 进食姿态 |
| `Alarm` | `D20_NPC_Alarm` | alarm lean | 警戒轮廓 |
| `Refuse` | `D20_NPC_Refuse` | refuse lean | 拒绝状态提示 |
| `Injured` | `D20_NPC_Injured` | injured lean | 受伤姿态/警告 |

状态名和 Animator 参数登记在 `game/Assets/Animations/D20/NPC/D20_NPC_ACTION_MAP.json`。标准参数包括 `D20_ActionId`、`D20_Speed01`、`D20_Grounded`、`D20_Injured`、`D20_Paused`、`D20_LodBand` 和 `D20_InteractionKind`。Root Motion 始终关闭。

## 玩家反馈与失败回退

| 输入事实 | 稳定反馈 key | 表现层处理 |
| --- | --- | --- |
| 对话开始/打开 | `npc_feedback_dialogue_open` | Talk 状态、头顶提示 |
| 对话结束/完成 | `npc_feedback_dialogue_ended` | 结束提示并回到快照状态 |
| NPC 忙碌 | `npc_feedback_busy` | Info 提示，不重复发起动作 |
| 导航不可达/缺锚点/场景卸载 | `npc_feedback_unreachable` | Warning 提示，不传送 NPC、不修改路线 |
| NPC 拒绝 | `npc_feedback_refused` | Refuse 姿态和 Warning 提示 |
| NPC 受伤 | `npc_feedback_injured` | Injured 姿态和 Warning 提示 |
| 暂停 | `npc_feedback_paused` | 停止表现时间，保留最后快照 |
| Animator/Clip 缺失 | 原反馈 key + `fallbackUsed=true` | neutral pose，不 T-pose、不阻塞交互 |

反馈只显示稳定 key，最终中文/英文文案由 D16/UI 本地化层消费。按钮是否可用、对话是否接受、路径是否成功以及拒绝原因仍由对应系统裁决；A04 不把提示当作裁决结果。

## 岗位锚点

`D20NpcPresentationAnchor` 仅提供稳定 ID、岗位枚举和交互提示 key；`TryGetVisualAnchor` 只读查询，不调用市场、码头、船坞或酒馆系统。

| 岗位 | 稳定 anchor ID | 演示用途 |
| --- | --- | --- |
| Market | `npc_visual_anchor_market_service` | 市场服务/交易姿态 |
| Dock | `npc_visual_anchor_dock_work` | 码头装卸/值勤姿态 |
| Shipyard | `npc_visual_anchor_shipyard_work` | 船坞修造姿态 |
| Tavern | `npc_visual_anchor_tavern_service` | 酒馆服务/休息姿态 |

专属演示 Prefab：`game/Assets/Prefabs/Characters/D20/NPCPresentation/D20_NPC_Presentation_Demo.prefab`。它只嵌套 A03 `npc_d20_dock_clerk_01` 的只读视觉实例，并附加 A04 适配器、FarProxy、反馈 Billboard 和四个锚点；没有写入正式场景。

## 远距、暂停和恢复

- 0–12m：完整源渲染器、Animator、岗位模块和反馈提示。
- 12–32m：保留源渲染器，Animator 使用 `CullUpdateTransforms`。
- 32–80m：关闭详细 Animator，显示无碰撞 FarProxy；保留上游快照和世界位置。
- 80m+：Proxy/裁剪；不删除逻辑实体、不改变岗位或行为状态。
- 暂停：表现时间停止，快照值保持不变；不写领域时钟。
- 场景卸载/重载：`NotifyPresentationSuspended()` 标记 `NeedsSnapshotResync`；`NotifyPresentationResumed()` 或 `OnEnable()` 重新应用最后的只读快照。新实例仍须由 owner 重新提供快照，表现层不自行重建事实。
- 读档恢复：适配器消费恢复后的快照，重新设置 Animator/LOD/提示，不修改存档或事件游标。

## 实际文件

| 类别 | 路径 |
| --- | --- |
| Runtime asmdef | `game/Assets/Scripts/Presentation/D20/NpcPresentation/WhalesIslandReef.D20.NpcPresentation.Runtime.asmdef` |
| 状态/反馈/锚点契约 | `game/Assets/Scripts/Presentation/D20/NpcPresentation/D20NpcPresentationContracts.cs`、`D20NpcPresentationAnchor.cs` |
| 只读适配器 | `game/Assets/Scripts/Presentation/D20/NpcPresentation/D20NpcReadOnlyPresentationAdapter.cs` |
| 反馈提示 | `game/Assets/Scripts/Presentation/D20/NpcPresentation/D20NpcFeedbackBillboard.cs` |
| 动作映射 | `game/Assets/Animations/D20/NPC/D20_NPC_ACTION_MAP.json` |
| 演示 Animator | `game/Assets/Animations/D20/NPC/D20_NPC_Presentation_Demo.controller` |
| 演示 Prefab | `game/Assets/Prefabs/Characters/D20/NPCPresentation/D20_NPC_Presentation_Demo.prefab` |
| A04 测试 | `game/Assets/Tests/EditMode/D20A04/` |

## 验收证据

Unity 6000.0.80f1 隔离导入工程测试：`D20_A04_UNITY_TEST_RESULTS.xml`，`7/7 PASS`。

覆盖：

1. 9 个行为状态映射；
2. 对话开始/结束、忙碌、不可达、拒绝、受伤反馈；
3. 缺动作 neutral fallback；
4. 市场、码头、船坞、酒馆 4 个视觉锚点；
5. LOD/FarProxy、暂停和恢复；
6. A03 只读源嵌套的演示 Prefab；
7. Presentation 源代码无强逻辑写者、网络或 LLM token。

机器结果：`game/tools/results/D20/D20-A04/D20_A04_PRESENTATION_BUILD_REPORT.json`、`D20_A04_UNITY_TEST_RESULTS.xml`、`unity_d20_a04_tests.log`。专项构建报告确认 7 个 Animator 参数、4 个视觉锚点、0 个逻辑写组件和 4 种回退策略。

## 已知边界

- 当前 A03 源人物是 Generic/NoAvatar；A04 不伪造 Humanoid 重定向或正式动作完整性。
- 反馈显示的是稳定 key，不直接改 D16 本地化表或 UI；正式 UI/音频反馈由对应任务接入。
- 没有修改正式 NPC 生成、导航、对话、日程或 InitialHarbor；D20-S02/后续任务负责集成回归。
- 资源和表现仍为 Candidate；不授予 `LocalDevelopmentReady` 或 `ReleaseApproved`。
