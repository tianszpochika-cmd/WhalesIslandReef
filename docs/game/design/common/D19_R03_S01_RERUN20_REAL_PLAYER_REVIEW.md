# D19-R03-S01 RERUN20 真实 Player 门禁复核

日期：2026-08-16  
结论：**BLOCKED**

本轮是对 RERUN19 预检的增量复核，不覆盖历史机器结果、预检或交接。RERUN10 使用不带 `-popupwindow` 的真实 Windows Player，排除了此前弹窗模式的输入焦点问题；仍不修改系统安全提示，不将静态证据解释为完整可玩通过。

## 真实路线

机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN10_MACHINE_RESULT.json`  
路线日志：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN10_ROUTE_LOG.json`  
探针（原始文件后被后续短运行覆盖，非独立证据）：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN10.json`；短运行归档：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN11.json`

真实 Player 持续 `704.296936 s`（约 11.7 分钟），分辨率 `1920x1080`，实际观察到人物移动、Character/Ship 控制切换、E 交互输入、保存输入和 16 张全屏 PNG。到达出生点、主码头、泊位和酒馆地标；市场、仓库、船坞和返港地标未到达。日志仍有不可行走碰撞体恢复提示，说明路线连续性尚未达标。由于原始探针文件被后续短运行覆盖，本轮不把该探针作为独立证明。

## 保存与恢复

保存输入在路线中被探针观察到。关闭路线 Player 后，以新 Player 从主菜单点击 Continue；`D19_R03_A01_RELOAD_AFTER_SAVE_RERUN10_PLAYER.log` 记录存档加载成功、InitialHarbor 重建、`D19_TerrainRoot` 落地和引导状态恢复。因此“保存输入 + Continue 恢复”通过，但“离港→返港→保存→退出→重载”的完整路线仍未通过。

## 与既有门禁的合并

| 门禁 | RERUN20 实际结果 | 结论 |
|---|---|---|
| 连续真实 Player ≥5 分钟 | 704.296936 秒 | PASS 子门槛 |
| 玩家移动与船控 | Character/Ship 样本均存在 | PASS 子门槛 |
| 关键交互输入 | E、保存输入可观察 | PASS 子门槛 |
| 市场→仓库→酒馆→码头→登船→出航→返港 | 仅部分地标到达 | BLOCKED |
| 保存退出重载 | Continue 恢复成功 | PASS（有限范围） |
| 14 固定镜头 | A02 RERUN17 为 14/14 文件 | PASS（数量/尺寸子门槛） |
| 视觉债务 | A02 人工 QA 仍有平坦/块状/近岸代理问题 | BLOCKED |
| Calm/Rain/Storm | A02 仅 Calm，0/3 变体 | BLOCKED |
| 1080p 性能 | A03 三档真实采样 | PASS（子任务范围） |

## 决策与责任

整体保持 **BLOCKED**，不启动 `D19-R02-S02`，不放行 D20 正式集成。路线/碰撞责任回交 D19-R03-A01 或新的 D19 Integration 修复任务；视觉与天气责任回交 D19-R03-A02。A03 的性能数据保留，不因本轮路线失败而删除或重采。

本轮没有修改 Systems、AgentCore、Registry、Save Schema、contracts 或资源状态；没有把任何 Candidate、LocalDevelopmentReady 或 ReleaseApproved 结论升级。
