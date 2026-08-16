# D18—D20 R03 恢复计划

日期：2026-08-16  
项目根目录：`C:\Users\Administrator\Desktop\game\first01`

## 目标与边界

本恢复线的最终目标是用真实 Unity Windows Player 证据，使 `D19-R02-S02` 与 `D20-S02-R01` 均达到 **PASS**。D18-S03、D21 及之后任务不在本轮范围内，不提前启动。

旧有 R02/R03 的 PARTIAL/BLOCKED 结果全部保留；本计划只允许在各任务的单一写者目录中修复后，以新结果命名重新验证。静态测试、编辑器预览、离屏截图和聊天结论均不能替代真实 Player 证据。

## 任务矩阵

| 工作包 | 当前真实状态 | 前置 | 单一写者/允许范围 | 当前结果或目标证据 |
|---|---|---|---|---|
| D18-R03-A01 | 待继续（资源补齐） | D18-S02-R02 PASS | D18 玩家外观候选、隔离导入、证据包 | `game/tools/results/D18/D18-R03-A01/` |
| D18-R03-S01 | 待执行资源复审 | A01 完成 | D18 R03 审计/消费门禁，不改全局状态机 | `game/tools/results/D18/D18-R03-S01/` |
| D20-A01-R02 | 待执行 | D18 gate 允许；D18 R03 S01 PASS | 玩家视觉工作 Prefab/接口 | `game/tools/results/D20/D20-A01-R02/` |
| D20-A02-R02 | 待执行 | A01-R02 `rig_interface_ready=true` | 玩家动作表现工作层 | `game/tools/results/D20/D20-A02-R02/` |
| D19-R03-A01 | PARTIAL | D19 R03 S01 未通过前可修复 | 真实路线、交互、保存证据/专属结果 | `game/tools/results/D19/D19-R03-A01/` |
| D19-R03-A02 | PARTIAL | 可与 A01 顺序修复 | InitialHarbor 表现/HUD/Integration | `game/tools/results/D19/D19-R03-A02/` |
| D19-R03-A03 | PARTIAL | 读取当前场景 | 真实 1080p 性能取证 | `game/tools/results/D19/D19-R03-A03/` |
| D19-R03-S01 | BLOCKED | A01—A03 全部硬门槛 | 预检，不启动正式重验 | `game/tools/results/D19/D19-R03-S01/` |
| D19-R02-S02 | PARTIAL 历史 | R03 S01 PASS、R02 证据完整 | 正式港口最终验收 | `game/tools/results/D19/D19-R02-S02/` |
| D20-S02-R01 | 未启动 | D19-R02-S02 PASS、A01/A02-R02 PASS | 正式角色/动物集成与验收 | `game/tools/results/D20/D20-S02-R01/` |

## 真实 D18 gate

当前实际 gate 文件为：

`C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-S02-R02\D18_R02_PLAYER_CONSUMPTION_GATE.json`

该 gate 当前为 `PASS`，且记录 `humanoid_valid_avatar_count=1`、`player_humanoid_candidate_available=true`；它只允许受控工作 Prefab，不表示 LocalDevelopmentReady、ReleaseApproved 或正式场景消费者。当前 `local_development_ready_count=0`、`release_approved_count=0`，必须保持不变，除非未来 D18-S03 按其职责处理。

## 执行顺序与门禁

1. 读取并核对 D18 现有角色/外观候选，补齐真实可消费的身体、发型和 1675 服装组合；保留许可证、哈希、Unity 导入与实际装配证据。
2. 只读复审 D18 R03 证据，生成当前消费门禁；不把 Candidate 或 intake 写成运行时就绪。
3. 按 D20 原始职责制作玩家视觉工作 Prefab 和动作层；只有真实 Humanoid + valid Avatar 与工作 Player 证据同时成立才允许下一步。
4. 在 D19 R03 A01/A02/A03 允许范围内修复真实路线、视觉/HUD 和性能，再运行 R03-S01 预检。
5. 只有 R03-S01 PASS 才启动正式 `D19-R02-S02`；正式验收 PASS 后才启动 `D20-S02-R01`。
6. 每个工作包完成后写机器 JSON、Markdown 报告、交接和当日 memory；任何硬门槛缺失均保持 PARTIAL/BLOCKED。

## 最终 PASS 条件

- D18 资源来源、许可证、哈希、导入、装配和消费映射可复核；角色缺口没有被错误模型冒充。
- D19 真实 Player 从 Main Menu 开始完成至少 300 秒的连续 WASD 路线、市场/仓库/酒馆/NPC/码头/登船/出航/返港/保存/退出/重载；14 个 Player 镜头与 3 种天气变体真实存在。
- D19 1920×1080 Player 的实际质量档、VSync、FPS、1% low、GC/内存及可得 CPU/GPU 数据齐全；Unity 测试和场景/碰撞/交互回归无新增失败。
- D20 玩家 Avatar/动作、NPC/动物/宠物正式集成不改变强逻辑、稳定 ID 或存档事实；真实 Player 能验证外观、动作、交互、保存恢复和性能。
- 所有旧报告保留，D18-S03、D21+ 未提前启动；没有把本地开发证据写成发布批准。

## 当前硬阻塞（基线）

- D19 R03 真实路线未闭合：最新 RERUN4 只到达码头、泊位、酒馆、仓库和出生点，市场、船坞、返港、离港及保存重载未完成。
- D19 视觉固定镜头/天气证据为 0/14、0/3；局部平地、近景遮挡与块状比例仍未关闭。
- D19 当前真实性能档名称为 `Very Low`/`Low`/`Medium`，尚未证明项目命名为 `High` 的第三档。
- D20 正式集成未启动；必须先取得 D19 正式 PASS 和 D20 R02 工作接口证据。

## RERUN17 最新增量（2026-08-16）

- A03 性能证据已闭合为 Low/Medium/High 三档真实 Player 采样，但仅属于性能子任务范围，不能解锁正式验收。
- A01 非调试 Player 仍被系统安全提示阻断；A02 固定镜头、天气矩阵和视觉债务仍是独立责任。
- 下一步仍是先取得真实路线/保存重载和 A02 视觉矩阵，再生成新的 D19-R03-S01 预检；不得绕过门禁启动 D19-R02-S02。

## RERUN20 路线复核（2026-08-16）

- A01 已通过不带 `-popupwindow` 的真实 Player 连续运行 704.296936 秒，观察到移动、船控、E 交互、保存输入和退出后 Continue 恢复。
- 仍未形成市场、仓库、船坞、返港完整路线；日志有不可行走碰撞体恢复提示，因此 S01 仍 BLOCKED。
- 新证据：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN10_MACHINE_RESULT.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN10_ROUTE_LOG.json`、`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN20.json`。
- 下一步责任仍为 D19 R03 路线/视觉任务；不启动 D19-R02-S02 或 D20 正式集成。

## RERUN18 更新（2026-08-16）

- A02 固定 Player 镜头数量已达到 14/14，但真实截图仍显示视觉债务，且仅有 Calm 天气；不能把数量门槛当作视觉通过。
- S01 仍 BLOCKED，必须先闭合 A01 路线/保存恢复和 A02 画面/天气，再考虑 D19-R02-S02。

## RERUN19 更新（2026-08-16）

- A02 RERUN17 已在真实 Player 补做目标调色/代理表现修复，并完成 14/14 个 1920×1080 固定镜头；视觉质量仍未达到门禁，天气仍 0/3。
- A01 RERUN10 的 Windows 安全提示仍未安全可操作，真实菜单输入、完整路线和保存—退出—重载仍未证明。
- 新 S01 复核为 BLOCKED，见 `game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN19.json`；不得绕过门禁启动 D19-R02-S02 或 D20 正式集成。
