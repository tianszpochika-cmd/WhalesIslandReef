# D20-A04 交接报告：NPC 行为表现

任务：D20-A04
日期：2026-08-15
结论：`PARTIAL`

## 前置门禁

- D20-S01：`PASS`。
- D18-S02-R01：`PASS`。
- A03 interface：`game/tools/results/D20/D20-A03/D20_A03_NPC_VISUAL_INTERFACE.json` 存在，`npc_visual_interface_ready=true`，`review_status=Candidate`。
- A03 Prefab/interface 只读消费；A03 的 12 个 NPC Prefab、资源 key 和回退未被修改。

## 交付路径

| 交付物 | 路径 |
| --- | --- |
| Runtime Presentation | `game/Assets/Scripts/Presentation/D20/NpcPresentation/` |
| 动作映射 | `game/Assets/Animations/D20/NPC/D20_NPC_ACTION_MAP.json` |
| 演示 Animator | `game/Assets/Animations/D20/NPC/D20_NPC_Presentation_Demo.controller` |
| 专属演示 Prefab | `game/Assets/Prefabs/Characters/D20/NPCPresentation/D20_NPC_Presentation_Demo.prefab` |
| A04 测试 | `game/Assets/Tests/EditMode/D20A04/` |
| 设计文档 | `docs/game/design/common/D20_NPC_BEHAVIOR_PRESENTATION.md` |
| 构建报告 | `game/tools/results/D20/D20-A04/D20_A04_PRESENTATION_BUILD_REPORT.json` |
| Unity 测试 XML | `game/tools/results/D20/D20-A04/D20_A04_UNITY_TEST_RESULTS.xml` |

专属演示 Prefab 嵌套 A03 `npc_d20_dock_clerk_01` 的只读源实例，附加 A04 适配器、四个岗位视觉锚点、反馈 Billboard 和无碰撞 FarProxy；没有写入 InitialHarbor 或正式 NPC 生成器。

## 状态映射与反馈

| 行为状态 | 表现动作 | 缺动作回退 |
| --- | --- | --- |
| Idle | `D20_NPC_Idle` | neutral breathing |
| Work | `D20_NPC_Work` | work bob + `npc_feedback_working_{job}` |
| Walk | `D20_NPC_Walk` | walk bob |
| Talk | `D20_NPC_Talk` | talk lean + dialogue-open key |
| Rest | `D20_NPC_Rest` | rest breathing |
| Eat | `D20_NPC_Eat` | eat lean |
| Alarm | `D20_NPC_Alarm` | alarm lean + alarm key |
| Refuse | `D20_NPC_Refuse` | refuse lean + refusal key |
| Injured | `D20_NPC_Injured` | injured lean + injury key |

失败/交互反馈：

- 对话开始/结束：`npc_feedback_dialogue_open` / `npc_feedback_dialogue_ended`；
- 忙碌：`npc_feedback_busy`；
- 不可达、缺锚点、场景卸载：`npc_feedback_unreachable`；
- 拒绝：`npc_feedback_refused`；
- 受伤：`npc_feedback_injured`；
- 暂停：`npc_feedback_paused`；
- 动作/Clip 缺失：保留主反馈 key，并设置 `fallbackUsed=true`，进入 neutral pose，不 T-pose、不传送。

## 锚点接口

| 岗位 | Anchor ID | 责任边界 |
| --- | --- | --- |
| Market | `npc_visual_anchor_market_service` | 只读视觉服务位置 |
| Dock | `npc_visual_anchor_dock_work` | 只读视觉工作位置 |
| Shipyard | `npc_visual_anchor_shipyard_work` | 只读视觉工作位置 |
| Tavern | `npc_visual_anchor_tavern_service` | 只读视觉服务位置 |

`D20NpcPresentationAnchor` 只保存 ID、岗位和提示 key；`D20NpcReadOnlyPresentationAdapter.TryGetVisualAnchor` 只读查询，不调用导航、交易、船坞、酒馆或日程系统。

## 只读与越权证据

`D20NpcPresentationSnapshot` 没有工资、金钱、库存、价格、关系、任务、奖励、时间推进或存档字段。A04 源扫描测试拒绝以下引用：`EventLog`、`EconomySystem`、`InventorySystem`、`RelationSystem`、`SaveSystem`、`NPCSystem`、`AgentCore`、`LLMClient`、`UnityWebRequest`、`HttpClient`；结果通过。

表现层只写：

- Animator 参数和状态；
- Visual 姿态、Root Motion 开关；
- Renderer/LOD/FarProxy 可见性；
- 反馈 Billboard 文本；
- `NeedsSnapshotResync` 生命周期标记。

## 测试命令与结果

Unity 6000.0.80f1 隔离导入工程命令：

```text
Unity -batchmode -nographics -disable-assembly-updater -projectPath game/tools/results/D20/D20-A04/UnityNpcPresentationBuilderProject -runTests -testPlatform editmode -assemblyNames WhalesIslandReef.D20A04.Tests -testFilter D20A04 -testResults game/tools/results/D20/D20-A04/D20_A04_UNITY_TEST_RESULTS.xml
```

结果：`Passed`，7/7：

1. 9 个行为状态映射；
2. 对话/导航/健康失败反馈；
3. 缺 Clip/Controller neutral fallback；
4. 四岗位锚点；
5. LOD、暂停和 `NotifyPresentationSuspended/Resumed` 恢复；
6. A03 只读源嵌套演示 Prefab 完整性；
7. 强逻辑/网络/LLM 越权扫描。

专项构建报告：7 个 Animator 参数、4 个视觉锚点、0 个逻辑写组件、4 类回退策略。

`git diff --check`：0 whitespace error；仅保留既有 CRLF 转换提示。

## 实际边界与后续

- 主工程在测试窗口期间被并行 D19-R01-B Unity 编辑器占用，因此本轮没有在 InitialHarbor 上运行 A04 专属测试或 PlayMode 日程路线；没有为了测试终止其他 Agent 的 Unity 进程，也没有删除 Unity 锁文件。
- A03 的 Generic/NoAvatar 限制仍在，A04 不宣称 Humanoid 重定向、正式动作完整性或历史服装定稿。
- D20-S02/后续同步集成负责把已通过的表现 Prefab 接到正式场景，并运行 NPC 导航、日程、对话和读档回归。
- 资源和表现状态继续保持 `Candidate`，不提前标记 `LocalDevelopmentReady` 或 `ReleaseApproved`。

最终结论：`PARTIAL`。核心只读表现适配与专项测试已完成；主场景 PlayMode 集成证据留待锁释放后的同步验收任务。
