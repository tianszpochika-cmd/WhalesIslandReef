# D20-L13 逻辑交接说明

- 任务：`D20-L13`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L13/D20_L13_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20RelationActionAdapter.cs`：

- 支持 `Dialogue` 与 `Cooperation` 两种关系动作，也提供 `Talk`/`Interact` 到关系动作的只读映射；
- 通过 `RelationSystem.KnowsCharacter(...)` 验证双方角色，通过 `ReadEdge(...)` 读取 Affinity、Trust、Respect、Fear 的公开副本；
- 无关系边时对话采用安全可继续语义，合作明确返回 `NoRelationEvidence`；
- 对高 Fear、低 Affinity、低 Trust、未知角色、缺失系统和非法关系边返回稳定失败码与反馈键；
- 合作资格使用 D20 动作策略阈值，关系数值本身仍由 D10 `RelationSystem` canonical owner 负责；
- 只读取副本，不写入关系图，不修改关系边或注册角色。

## 2. 后续可消费公开 API

- `D20RelationActionAdapter.Evaluate(RelationSystem, string, string, D20RelationActionKind)`；
- `D20RelationActionAdapter.Evaluate(RelationSystem, string, string, D20ActionIntentKind)`；
- `D20RelationActionEvaluation`；
- `D20RelationActionKind`、`D20RelationActionFailureCode`。

合作/对话结果只是前置资格；真实事实事件和关系变化必须继续回到 D10 owner。

## 3. 验证与未完成项

- D20-L11—L15 与 D10 Relation/Event 公开依赖的纯 C# 编译：Roslyn `/warnaserror+`，exit 0、0 警告；
- smoke 覆盖已知角色对话 fallback、缺少合作证据、关系 owner 形成 trust 后合作资格；
- 目标源文件静态边界检查：无 Unity/network/LLM/time/random/owner mutation 引用，尾随空白为 0；
- 未启动 Player、PlayMode、场景、输入、动画或设备；真实运行时社交路由和关系结算延期。

## 4. Coordinator memory entry

协调器在批次汇合时统一更新共享状态和当日日志：`D20-L13 LOGIC_PASS`；记录 RelationSystem 只读边投影、对话/合作资格、无证据拒绝和不越权写关系图。
