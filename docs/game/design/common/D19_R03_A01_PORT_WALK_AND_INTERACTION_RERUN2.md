# D19-R03-A01 RERUN2：真实 Player 港口路线取证

日期：2026-08-16  
结论：**PARTIAL**

## 取证范围

本轮使用实际 Windows Player `D19_R03_A01_Player_RERUN2.exe`，启动参数包含 `1920x1080`、窗口化和 `-force-d3d11`，通过 Player 窗口执行新游戏、移动、E 交互、上下船和 F5 尝试。没有使用 Inspector 直接调用玩法方法，也没有把截图命名当作成功证据。

Player 从新游戏界面开始运行 **584.5516 秒** 后关闭。探针文件为：

`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN2.json`

本轮保留 12 张真实 Player 截图：

`game/tools/results/D19/D19-R03-A01/screenshots/route_rerun2_*.jpg`

## 机器观察

| 项目 | 结果 |
|---|---|
| Player 场景 | `InitialHarbor` |
| 玩家对象 | `D2_PlayerAvatar` |
| 控制器/路由器 | 探针找到；路由器对象为 `D2RuntimeBootstrap` |
| 落地 | 观察到 `grounded=true` |
| 移动 | `moved_observed=true`，89 次移动观察、516 次位移观察 |
| 交互输入 | `interaction_input_observed=true`，3 次探针事件 |
| 控制模式 | `Character` 与 `Ship` 均观察到 |
| 码头/泊位 | 到达坐标邻域并记录稳定 ID 事件 |
| 酒馆 | 到达坐标邻域并记录事件；不是对象解析通过 |
| 市场/仓库/船坞/返港 | 未到达或未证明 |
| 保存 | `save_input_observed=false` |
| 保存后重载 | 未执行/未证明 |
| 连续按住 WASD | 未证明；本轮是可复核的重复真实按键输入 |

探针虽然以稳定 ID 作为坐标观察标签，但当前 R03 表现层对象名查找仍为 `found=false`。因此码头、泊位和酒馆的结果只能报告为坐标邻近观察，不能升级为完整建筑/交互通过。

## 路线阶段表

| 阶段 | 玩家输入与证据 | 结论 |
|---|---|---|
| 新游戏 | 点击 New Game，输入 `R03RouteRerun2`，确认 | PASS |
| 出生/落地 | Player 真实画面；探针观察 grounded | PASS |
| 移动 | 重复 W/A/S 输入；探针观察位移 | PARTIAL |
| 市场 | 截图保留，但坐标/对象均未通过 | NOT_REACHED |
| 仓库 | 未完成 | NOT_RUN |
| 酒馆/NPC | 到达坐标邻域，E 输入被观察 | OBSERVED_PARTIAL |
| 码头/泊位 | 稳定 ID proximity 事件 | OBSERVED |
| 上船/下船 | Player 观察到 `Ship` 控制模式及后续下船画面 | OBSERVED_PARTIAL |
| 出航/返港 | 未形成完整事实链 | NOT_PROVEN |
| 保存/重载 | F5 尝试，但探针未观察且未重载 | NOT_PROVEN |

## 诚实门禁结论

本轮满足“真实 Player、超过 5 分钟、确有移动/交互/上下船观察和截图”的子门槛，但不满足 D19-R03-A01 的完整路线与保存恢复门槛。不能把 `route_rerun2_save_03.jpg` 解释成保存成功，也不能把坐标 proximity 解释成市场/仓库/船坞交互通过。

随后使用新编译的 `D19_R03_A01_Player_RERUN3.exe` 做了输入边沿诊断，结果记录在 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_INPUT_CONTROL_TEST.json`。菜单点击/新游戏加载可复核，但重复 W 短按、文本化 w 和 Escape 没有形成可靠可见位移/暂停反馈；这被记录为当前 computer-use 输入路径限制，不被解释成游戏控制器已坏，也不被当成路线通过证据。

下一步需要在本任务允许范围内继续修复/取证：更新探针以逐帧捕获 F5/C 保存边沿，使用真实 Player 完成市场→仓库→酒馆→码头→登船→出航→返港→保存→重载；若路线入口本身无法在当前 R03 范围内完成，交回 D19 场景/交互修复任务。D19-R03-S01 在这些门槛闭合前不得放行正式 D19 验收。
