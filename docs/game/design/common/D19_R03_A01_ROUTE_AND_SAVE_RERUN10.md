# D19-R03-A01 RERUN10 真实 Player 路线与存档复验

## 结论

本轮为 `PARTIAL`。真实 Player 连续运行 704.296936 秒，达到 5—10 分钟持续运行要求，并真实观察到人物移动、船控模式、登船/下船输入边沿、保存输入边沿和 16 张 1920×1080 截图。完整“出生 → 市场 → 仓库 → 酒馆/NPC → 码头 → 登船 → 出航 → 返港 → 保存 → 退出 → 重载”仍未通过：市场、仓库、船坞和返港地标没有到达；打开 NPC 对话和离港/返港事实没有形成可复核闭环。

## 本轮运行

- Player：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN10.exe`
- 启动日志：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_MAINMENU_ROUTE_RERUN10_PLAYER_NOPopup.log`
- Unity：`6000.0.80f1`，D3D11，NVIDIA GeForce RTX 4070 Ti
- 目标分辨率：`1920×1080`；窗口化非 popup，避免历史系统提示遮挡
- 连续时长：`704.296936s`
- 只读探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN10.json`
- 路线机器记录：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN10_ROUTE_LOG.json`
- 机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN10_MACHINE_RESULT.json`
- 截图：`game/tools/results/D19/D19-R03-A01/screenshots/route_rerun10_00.png` 至 `route_rerun10_15.png`，共 16 张，均为 `1920×1080`

> 证据完整性说明：后续一次未产生有效输入的目标路线短运行误复用了 `-d19-r03-route-rerun10` 参数，覆盖了原始 `D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN10.json`。当前短运行已另存为 `D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN11.json` 和 `D19_R03_A01_TARGETED_RERUN11_PLAYER.log`。本报告的 704.296936 秒结论仅依赖本机器结果、路线日志、原 Player 日志、16 张截图和 Continue 重载日志；不把被覆盖探针当作独立证据。

## 真实观察

| 项目 | 结果 | 证据 |
|---|---|---|
| Player / Controller / Router | PASS | 探针三个 found 字段均为 true |
| 人物移动 | PASS | `moved_observed=true`，movement 事件 15 条 |
| 码头/泊位 | PASS | `anchor.dock.primary`、`anchor.berth.primary` 在 41.898s 到达 |
| 登船/下船输入 | PASS（输入与模式） | 交互输入事件；`Character`/`Ship` 两种模式样本 |
| 酒馆地标 | PASS（地标） | `building:tavern_santa_margarita` 在 391.415s 到达 |
| 市场/仓库/船坞 | NOT_REACHED | 探针首达时间为 -1 |
| 离港/返港 | NOT_CONFIRMED / NOT_REACHED | 没有 open-sea 或 return landmark 证据 |
| 保存 | INPUT_OBSERVED | 416.233s 与 693.760s 观察到 F5/C 边沿 |
| 退出与重载 | PASS | 新 Player 点击 Continue 后日志明确记录存档加载成功、InitialHarbor 重建、D19_TerrainRoot 接地 |

## 重载证据

使用同一 Player 可执行文件启动第二个真实 Player，不带路线探针，点击主菜单 `继续`。`D19_R03_A01_RELOAD_AFTER_SAVE_RERUN10_PLAYER.log` 记录：

- `[MainMenu] 存档加载成功，进入游戏`
- `D19-A01 NavMesh data registered: valid=True`
- `D2/Physics 玩家已落地：surface=D19_TerrainRoot`
- `D7/Tutorial 已根据存档恢复引导步骤`

因此“保存输入 → 退出 Player → Continue 重载 → 场景创建和落地恢复”有真实证据；这不等于完整航行返港存档闭环已通过。

## 阻断与责任

1. 关键路线仍被不可行走碰撞体恢复日志阻断，下一责任优先为 `D19-R03-A02` 的集成碰撞/路线修复；不能通过探针传送或直接调用玩法入口绕过。
2. 离港/返港需要 `D19-R03-A01` 在碰撞修复后重新取证；目前只证明 Ship 控制模式出现，不证明航行状态已改变。
3. NPC 对话需要在市场/酒馆可达并靠近稳定 NPC 后用 E 重新取证。

## 权限边界

本轮只使用真实 Player 输入和只读探针，未修改 Systems、Save Schema、Registry、InitialHarbor、D19-A01—A06 工作成果或资源状态；旧 RERUN1/RERUN4/RERUN7/RERUN10 报告和结果保留。
