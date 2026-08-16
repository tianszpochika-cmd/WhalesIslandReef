# D19-R03-A01 RERUN29 真实 Player 路线与保存复验

日期：2026-08-16  
结论：**PARTIAL**

## 证据

- Player：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN29.exe`
- Player 日志：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN29_PLAYER_LONG.log`
- 长运行日志归档：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN29_LONG_PLAYER.log`
- 只读探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN29_LONG.json`
- 机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN29_MACHINE_RESULT.json`
- 截图：`game/tools/results/D19/D19-R03-A01/screenshots/route_rerun29_00.png` 至 `route_rerun29_15.png`

## 真实运行结果

本轮从 Player Main Menu 创建新游戏后进入 InitialHarbor，使用真实窗口和 CUA 输入连续运行 `694.13134765625s`，分辨率 `1920×1080`。探针确认 `player_found/controller_found/router_found/moved_observed/interaction_input_observed/save_input_observed` 均为 `true`。日志真实记录：

- `D19_TerrainRoot` 被 D2 物理识别并恢复为可行走接地面；
- E 触发登船/下船模式切换，探针记录 `boarded`/`disembarked`；
- C/F5 产生保存输入，Player 画面显示“存档检查点已完成”，日志记录 `[D3/Save] 贸易、库存和航程快照已保存`；
- 共生成 16 张 Player 检查点截图，首张用 System.Drawing 核验尺寸 `1920×1080`。

| 路线节点 | 首次观察秒数 | 结论 |
|---|---:|---|
| 出生点 `anchor.player.spawn` | 63.750 | 通过 |
| 码头 `anchor.dock.primary` | 45.989 | 通过 |
| 泊位 `anchor.berth.primary` | 45.989 | 通过 |
| 酒馆 `building:tavern_santa_margarita` | 57.000 | 通过 |
| 市场 `building:market_santa_margarita` | -1 | 未到达 |
| 仓库 `building:warehouse_santa_margarita` | -1 | 未到达 |
| 船坞 `building:shipyard_santa_margarita` | -1 | 未到达 |
| 返港 `anchor.return.harbor` | -1 | 未到达 |
| 同一轮退出后 Continue 重载 | -1 | 未执行 |

## 门禁结论

连续运行、玩家输入、部分移动、登船/下船、保存反馈和截图数量/分辨率通过；完整市场—仓库—酒馆—码头—出航—返港—保存—重载路线没有通过，且本轮没有同一轮重载验证。RERUN29 不放行 D19-R03-S01；D19-R03-A02 仍需闭合路线可达性、天气/性能和视觉质量，历史 RERUN10/RERUN25/RERUN27 证据继续保留且不被本报告覆盖。
