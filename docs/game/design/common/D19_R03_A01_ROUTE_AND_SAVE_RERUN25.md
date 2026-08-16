# D19-R03-A01 RERUN25 真实 Player 路线与保存复验

日期：2026-08-16  
结论：**PARTIAL**

## 证据

- Player：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN25.exe`
- Player 日志：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN25_PLAYER.log`
- 只读探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN25.json`
- 独立机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN25_MACHINE_RESULT.json`
- 检查点截图：`game/tools/results/D19/D19-R03-A01/RERUN25_PLAYER_CHECKPOINT_01.jpg`、`RERUN25_PLAYER_CHECKPOINT_02.jpg`、`RERUN25_PLAYER_CHECKPOINT_03.jpg`

## 实际运行

本轮在真实 Windows Player 中以 1920×1080 窗口运行 675.432556 秒，Player、D2 控制器和控制路由器均被探针发现。探针观察到真实位移、交互输入边沿和保存键边沿；起点为 `(0, 1.100000024, -6.5)`，终点为 `(1.693310499, 0.691850007, -4.109002590)`，相对起点最大距离约 11.267m。

| 地标/阶段 | 首次观察秒数 | 实际结论 |
|---|---:|---|
| `anchor.player.spawn` | 85.534 | 观察到 |
| `anchor.dock.primary` | 36.265 | 观察到 |
| `anchor.berth.primary` | 36.265 | 观察到 |
| `building:tavern_santa_margarita` | 73.034 | 观察到 |
| `building:market_santa_margarita` | -1 | 未到达 |
| `building:warehouse_santa_margarita` | -1 | 未到达 |
| `building:shipyard_santa_margarita` | -1 | 未到达 |
| `anchor.return.harbor` | -1 | 未到达 |

## 保存边界

本轮共观察到 4 次 `save_input_observed`，但当前探针只记录 F5/C 按键边沿，不调用 SaveProgress。因此本轮没有可证明的保存成功、退出、重载或恢复事件；不能把按键边沿解释为存档闭环。此前 RERUN10 的 Continue 重载证据继续保留，但不与本轮路线拼接为一条完整路线。

## 门禁结论

连续运行时长通过，真实 Player 输入/位移/交互观察通过；完整市场—仓库—船坞—出航/返港路线、9+路线截图和本轮保存重载未通过。D19-R03-A01 保持 PARTIAL，D19-R03-S01 不放行。

## RERUN27 焦点排查补充

RERUN27 通过真实 Player 进入 InitialHarbor 并验证 E 模式切换；W/A/S/D 只能通过短脉冲注入，未形成连续路线。其归档探针与 RERUN25 SHA-256 完全相同，因此明确排除为独立路线证据。机器记录：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_CUA_FOCUS_RERUN27_MACHINE_RESULT.json`。本补充不改变 RERUN25 的 PARTIAL 结论，也不将按键边沿解释为保存成功。

## RERUN29_LONG 增量

RERUN29 使用补齐 `D19_TerrainRoot` 后重新构建的真实 Player，连续运行 `694.13134765625s`，分辨率 `1920×1080`。本轮真实观察到角色位移、E/B/T/V 交互输入边沿、C/F5 保存输入边沿、登船/下船模式反馈，以及日志中的保存成功反馈；Player 日志还记录 `D19_TerrainRoot` 被 D2 物理识别为可行走接地面。

| 地标/阶段 | 首次观察 | 本轮结论 |
|---|---:|---|
| `anchor.player.spawn` | 63.750s | 到达 |
| `anchor.dock.primary` | 45.989s | 到达 |
| `anchor.berth.primary` | 45.989s | 到达 |
| `building:tavern_santa_margarita` | 57.000s | 到达 |
| `building:market_santa_margarita` | -1 | 未到达 |
| `building:warehouse_santa_margarita` | -1 | 未到达 |
| `building:shipyard_santa_margarita` | -1 | 未到达 |
| `anchor.return.harbor` | -1 | 未到达 |

截图共 16 张，位于 `game/tools/results/D19/D19-R03-A01/screenshots/route_rerun29_00.png` 至 `route_rerun29_15.png`，首张使用 System.Drawing 核验为 1920×1080。保存成功与登船反馈是真实日志证据，但本轮没有执行退出后 Continue 重载，不能与其它轮次拼接为本轮完整闭环。

因此 RERUN29 仍是 **PARTIAL**，只证明新碰撞体已被 Player 读取、保存输入/保存反馈存在、5—10 分钟运行和部分节点可达；不放行 D19-R03-S01。
