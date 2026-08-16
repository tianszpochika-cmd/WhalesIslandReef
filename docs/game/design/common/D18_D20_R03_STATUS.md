# D18—D20 R03 当前状态

更新时间：2026-08-16  
状态依据：磁盘机器结果、Unity/Player 日志、截图和交接报告；不依据聊天结论。

## 基线表

| 任务 | 最新结论 | 证据 | 是否需要继续 |
|---|---|---|---|
| D18-S02-R02 | PASS | `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-S02-R02\D18_S02_R02_MACHINE_RESULT.json` | 否；保留并只读使用 |
| D18-R03-A01 | 未形成 R03 结果 | `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-R03-A01\` | 是 |
| D18-R03-S01 | 未执行 | `C:\Users\Administrator\Desktop\game\tools\results\D18\D18-R03-S01\` | 等 A01 |
| D19-R03-A01 | PARTIAL | `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D19\D19-R03-A01\D19_R03_A01_MACHINE_RESULT.json` | 是 |
| D19-R03-A02 | PARTIAL | `C:\Users\Administrator\Desktop\game\tools\results\D19\D19-R03-A02\D19_R03_A02_MACHINE_RESULT.json` | 是 |
| D19-R03-A03 | PARTIAL | `C:\Users\Administrator\Desktop\game\tools\results\D19\D19-R03-A03\D19_R03_A03_MACHINE_RESULT.json` | 是 |
| D19-R03-S01 | BLOCKED | `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D19\D19-R03-S01\D19_R03_S01_MACHINE_RESULT_RERUN20.json` | 等 A01—A03 |
| D19-R02-S02 | PARTIAL（历史） | `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D19\D19-R02-S02\D19_R02_S02_RERUN8_MACHINE_RESULT.json` | R03 S01 PASS 后重新验收 |
| D20-A01-R01 | 未找到本轮接口文件 | `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D20\D20-A01-R01\` | 按恢复线执行 |
| D20-A02-R01 | 未找到本轮接口文件 | `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D20\D20-A02-R01\` | 按恢复线执行 |
| D20-S02-R01 | 未启动 | `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D20\D20-S02-R01\` | 等 D19 PASS 与 A01/A02 |

## 已确认事实

- D18 gate 的真实路径为 `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-S02-R02\D18_R02_PLAYER_CONSUMPTION_GATE.json`，其 `status=PASS`、`humanoid_valid_avatar_count=1`、`player_humanoid_candidate_available=true`。
- LocalDevelopmentReady=0、ReleaseApproved=0；本恢复线不得擅自晋级。
- D19 R03 A02 RERUN14 已有独立船体表现 Prefab 和真实 Player 截图，但不是完整港口验收。
- D19 R03 A01 RERUN4 已实际运行 301.169769 秒，观察到移动、E/B/C 反馈，但路线与保存重载不完整。
- D19 R03 A03 RERUN16 已有三次 1920×1080 Player 采样，项目实际质量名称是 Very Low/Low/Medium。
- D19 R03 S01 当前 BLOCKED；D19-R02-S02 与 D20-S02-R01 不得提前启动。

## 责任边界

- D18 R03 只写 D18 角色/外观候选与隔离证据，不改全局资源状态或正式场景。
- D19 R03 A01/A02/A03 分别负责路线/交互、港口表现/HUD、真实性能；不得互写他人专属目录。
- D19 R03 S01 只做预检，不在未通过时把正式验收写成 PASS。

## RERUN17 状态校正（追加，不覆盖历史表）

- D18-R03-A01：隔离角色资源审计实际为 PASS，见 `game/tools/results/D18/D18-R03-A01/D18_R03_A01_MACHINE_RESULT_RERUN1.json`；不等于运行时晋级。
- D18-R03-S01：隔离复核实际为 PASS，见 `game/tools/results/D18/D18-R03-S01/D18_R03_S01_MACHINE_RESULT_RERUN1.json`；不等于正式场景集成。
- D19-R03-A01：仍为 PARTIAL；RERUN10 机器结果 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN10_MACHINE_RESULT.json`，系统安全提示仍阻断真实菜单输入。
- D19-R03-A02：仍为 PARTIAL；RERUN14 仍缺 14 固定 Player 镜头、3 天气变体和视觉债务收口。
- D19-R03-A03：RERUN17 性能范围 PASS，见 `game/tools/results/D19/D19-R03-A03/D19_R03_A03_MACHINE_RESULT_RERUN17.json`；三档实际为 Low/Medium/High。
- D19-R03-S01：仍 BLOCKED，最新汇总见 `game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN17.json`；D19-R02-S02、D20-S02-R01 继续不启动。
- D19-R03-S01 最新 RERUN18 已真实记录 A02 固定镜头 14/14，但天气 0/3、视觉债务和 A01 路线/保存重载仍未闭合：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN18.json`。
- D20-A01-R02、D20-A02-R01：隔离范围已有 PASS，但正式集成仍等待 D19 放行。
- D20 A01/A02 只写工作 Prefab/动作表现；只有 D20-S02-R01 才可修改正式角色生成引用与 InitialHarbor 角色表现。

## RERUN19 状态校正（2026-08-16）

- D19-R03-A02 RERUN17：真实 Player 固定镜头 14/14、1920×1080 尺寸 14/14；材质调色、巨大三角树和浅色竖杆债务有所改善，但平坦地表/码头、低细节块状建筑、近岸代理几何、Calm-only 仍使任务保持 PARTIAL。证据：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN17.json`。
- D19-R03-S01 RERUN19：重新汇总 A01 RERUN10、A02 RERUN17、A03 RERUN17，结论仍 BLOCKED。证据：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN19.json`。
- D19-R02-S02 与 D20-S02-R01 继续不启动；D20-A01-R02/A02-R01 的隔离 PASS 不等于正式集成放行。

## RERUN20 状态校正（2026-08-16）

- D19-R03-A01 RERUN10：真实 Player 704.296936 秒，1920x1080，移动/船控/交互输入/保存输入可观察；Continue 重载成功，但市场、仓库、船坞、返港未闭合，仍 PARTIAL。
- D19-R03-S01 RERUN20：继续 BLOCKED；新机器结果为 `game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN20.json`。D19-R02-S02 与 D20 正式集成继续不启动。

## RERUN24—RERUN26 状态校正（2026-08-16）

- D19-R03-A01：RERUN25 真实 Player 连续运行 `675.432556s`，观察到 Player/控制器/路由器、位移、交互输入和保存键边沿；完整市场/仓库/船坞/返港路线未完成，保存成功与本轮重载未记录，仍 **PARTIAL**。证据：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN25_MACHINE_RESULT.json`。
- D19-R03-A02：RERUN26 使用 8 个 intake-eligible NatureKit 候选进入真实 Player；最新 Editor audit 为 315 Renderer、314 enabled、6457 顶点，Unity 审计退出码 0。块状建筑、平坦地表和开发 HUD 仍可见，仍 **PARTIAL**。证据：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json`、`game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_AUDIT_RERUN26_CANDIDATES_REFRESH.log`。
- D19-R03-S01：RERUN24—RERUN26 未闭合完整路线、最终视觉和天气门禁，继续 **BLOCKED**；D19-R02-S02 与 D20 正式集成继续不启动。
- D18-S02-R02 仍为 PASS；本轮未改变 LocalDevelopmentReady=0、ReleaseApproved=0，也未修改 D18 资源状态。

## RERUN29 状态校正（2026-08-16）

- D19-R03-A01：真实 Player `694.13134765625s`、1920×1080；Player/controller/router、位移、交互输入、保存输入均观察到，日志确认保存反馈、登船/下船以及 `D19_TerrainRoot` 接地恢复；16 张 Player 截图已落盘。
- RERUN29 到达出生点、酒馆、码头、泊位，未到达市场、仓库、船坞、返港；本轮未执行同一轮退出后 Continue 重载。因此 D19-R03-A01 仍 **PARTIAL**，D19-R03-S01 仍 **BLOCKED**。
- 本轮只在 D19 R03 Integration 增加/验证受控路线碰撞体，不改变 D2/D6 逻辑、资源状态、Registry、存档 Schema 或 Systems；D20 正式集成继续锁定。

### 基线表事实校正

上方历史基线表中的“未形成 R03 结果/未执行”行是早期状态，保留用于追溯；当前有效 R03 事实为：D18-R03-A01 与 D18-R03-S01 均为隔离范围 PASS（不晋级资源状态），D19-R03-A01 以 RERUN29 机器结果为最新 PARTIAL，D19-R03-A02 以 RERUN28 机器结果为最新 PARTIAL，D19-R03-S01 以 RERUN29 机器结果为最新 BLOCKED。

## RERUN35 状态校正（2026-08-16）

- D19-R03-A02 已在 R03 Integration 表现层收敛屋顶/门廊/市场入口/远景尺度，并在几何更新后重新采集 14/14 个 1920×1080 固定镜头；平坦地表、低细节建筑、代理岩体、开发 HUD 和 Calm-only 仍使 A02 保持 PARTIAL。
- D19-R03-A02 机器证据：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN35_VISUAL_PATCH.json`、`D19_R03_A02_MACHINE_RESULT_RERUN35_FIXED.json`。D19-R03-S01 仍引用真实门禁结果并保持 BLOCKED；D19-R02-S02 与 D20-S02-R01 未放行。

## RERUN39 / RERUN36_CURRENT 状态校正（2026-08-16）

- D19-R03-A01 RERUN39：真实 Player 约 211.79 秒，菜单/新游戏/上船/下船/移动输入/保存反馈均有证据；市场、仓库、船坞、返港、同轮 Continue 未闭合，仍 **PARTIAL**。
- D19-R03-A03 RERUN36_CURRENT：当前场景真实 1920×1080 三档采样完成；Medium/High 达到 1% low 阈值，Low=7.903 FPS 未达阈值，整体 **PARTIAL**。
- D19-R03-S01 继续 **BLOCKED**；不放行 D19-R02-S02，也不启动 D20 正式集成。历史结果、资源状态和 LocalDevelopmentReady/ReleaseApproved 边界均保持不变。

## RERUN44 状态校正（2026-08-16）

- R03 Integration 本轮仅新增重复碰撞收敛：运行时禁用旧 `DockDeck_Collision`/`Gangplank_Collision` 共 3 个，保留 D6 地面所有权。真实 Player 独立运行 `193.38430786132813s`、1920×1080 请求，生成 7 张当前截图；机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN44_MACHINE_RESULT.json`。
- 运行时探针确认船位 `(0,0,0)`、玩家与船体不重叠、最终位移约 `0.045m`；只到达码头/泊位，保存输入为 false，完整路线和 Continue 未执行。此结果仍为 A01 **PARTIAL**。
- 新 S01 门禁：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN44_CURRENT.json`，仍 **BLOCKED**。当前性能、视觉、天气和路线/保存门槛未闭合；D19-R02-S02 与 D20 正式集成不放行。历史报告和结果均保留。

## RERUN45/RERUN46 状态校正（2026-08-16）

- R03 Integration 桥接新增禁用 D19-A01 旧地形 Collider 1 个；不改变 D19-A01 源成果、D6 地面/船体所有权、稳定 ID、Systems、存档或资源状态。RERUN45/RERUN46 Player 日志均确认该修复执行。
- RERUN45 真实 Player `120.1856460571289s`、2 张截图，新增酒馆/码头/泊位到达；RERUN46 `126.42691040039063s`、4 张截图，新增出生/酒馆/码头/泊位到达；两轮保存 false，完整路线和 Continue 未执行。A01 继续 PARTIAL。
- S01 仍 **BLOCKED**：市场/仓库/船坞/返港、保存/重载、当前性能、视觉债务和天气门槛未闭合；D19-R02-S02 与 D20 正式集成不放行。历史 RERUN44/RERUN42 及更早结果保留。
- 当前有效门禁机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN46_CURRENT.json`。
