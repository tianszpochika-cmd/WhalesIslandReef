# D19-R03-S01：D19 正式验收预检

日期：2026-08-16  
结论：**BLOCKED**

本文件是 D19-R03-S01 的门禁预检，不覆盖历史 D19-R02-S02、D19-S02-R01 或 D19-S02-R01-R01 的 PARTIAL/BLOCKED 结果。预检只读取 D19-R03-A01、A02、A03 的新证据；未满足硬门槛时不启动正式 `D19-R02-S02` 重验。

## RERUN17 最新汇总（保留 BLOCKED）

机器汇总：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN17.json`。A03 的最新三档性能已改用真实运行时档名 `Low/Medium/High` 并通过性能子任务门槛；这只消除了旧的档名证据缺口，不改变 A01/A02 的未完成事实。A01 RERUN10 仍被未触碰的 Windows 安全提示拦截在 MainMenu，A02 固定镜头/天气仍为 0/14、0/3，所以本预检结论继续为 **BLOCKED**。

## RERUN18 最新汇总（保留 BLOCKED）

机器汇总：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN18.json`。A02 RERUN15 已真实生成 14/14、1920×1080 固定镜头，但人工画面检查仍未通过，天气仍为 0/3；A01 RERUN10 仍未进入 InitialHarbor。性能门槛已闭合为子任务 PASS，正式 D19 仍 **BLOCKED**。

## 预检结果

| 门槛 | 实际证据 | 结果 |
|---|---|---|
| 真实 Player 时长 ≥ 300 秒 | A01 RERUN2：584.5516 秒 | PASS（子门槛） |
| 玩家落地、移动、Character/Ship | A01 探针；真实 Player | PASS（子门槛） |
| 市场→仓库→酒馆/NPC→码头→登船→出航→返港 | A01 RERUN2 | BLOCKED，未形成完整路线 |
| 保存、退出、重载、恢复 | A01 RERUN2：`save_input_observed=false` | BLOCKED |
| 真实 1080p 性能 | A03 三次真实 Player | PASS（取证子门槛）；项目档名为 Very Low/Low/Medium |
| 14 个固定 Player 镜头 | A02 | BLOCKED，0/14 |
| 3 个天气变体 | A02 | BLOCKED，0/3 |
| 港口视觉债务 | A02 RERUN4/RERUN6 截图 | BLOCKED，平地与块状建筑仍可见 |
| 静态 D19/D17/D18 回归 | 旧 R02 机器结果：D19 EditMode 34/0、D19 PlayMode 8/0/1 skip、D17 7/0、D18 11/0 | PASS（静态子门槛；不替代 Player） |

## 决策

R03 子任务已经产生真实进展，尤其是 584 秒 Player 运行和 1920×1080 性能采样，但没有完整路线/保存恢复和视觉镜头证据。因此 S01 必须保持 BLOCKED，正式 D19 重验暂不执行。`D20-A01-R02`、`D20-A02-R02` 和 `D20-S02-R01` 仍不放行；不得把本预检报告写成 D19 PASS。

机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT.json`。

## 2026-08-16 最新增量复核（保留原预检结论）

本节只追加 RERUN13/RERUN16 的真实证据，不覆盖前述 BLOCKED 结论，也没有启动正式 `D19-R02-S02`。

- A02 RERUN13：Unity 构建退出码 0；编辑器审计记录 311 个 Renderer、310 个启用 Renderer、5,996 个网格顶点、297 个持久化生成网格 `.asset`，无 Collider；真实 Player 截图 `game/tools/results/D19/D19-R03-A02/screenshots/r03_a02_spawn_rerun13_player.jpg` 可见屋顶、立面、门窗、市场入口、摊位和码头木板，且没有粉色材质或大面积白平面。但固定镜头仍为 0/14、天气变体仍为 0/3，平坦地表、块状比例和近景遮挡仍未完成视觉门禁。
- A03 RERUN16：真实独立 Windows Player 强制加载 `InitialHarbor`，1920×1080，30 秒 warm-up + 60 秒采样 × 3，平均 FPS 为 59.9967 / 59.9942 / 59.9944，1% low 为 59.7199 / 59.5348 / 59.5566；原始结果位于 `game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_*_rerun16.json`。运行时实际质量名称为 `Very Low`、`Low`、`Medium`，因此没有把请求参数 `high` 伪称为项目 High；该档名差异仍是记录项，不修改 QualitySettings。
- A01 RERUN2：仍只有 584.5516 秒真实 Player、移动/落地/部分交互证据；完整市场→仓库→酒馆/NPC→码头→登船→出航→返港→保存→重载链和连续 WASD 未闭合，保存输入仍未被探针观察。

增量结论仍为 **BLOCKED**。`D19-R02-S02`、`D20-A01-R02`、`D20-A02-R02`、`D20-S02-R01` 不放行；必须先在授权范围内补齐真实路线/保存恢复、14 个固定 Player 镜头与天气变体，并重新执行门禁预检。

## 2026-08-16 RERUN4/RERUN14 最新增量复核（保留 BLOCKED）

本节追加一次真实 Player 路线和一次船体表现复核，不覆盖 RERUN2/RERUN13/RERUN16，也没有启动正式 `D19-R02-S02`。

- A01 RERUN4 使用真实 Windows Player `D19_R03_A02_Player_RERUN14.exe`，启动参数为 1920×1080、窗口化、D3D11，持续运行 `301.169769` 秒。实际观察到移动、E 登船/下船反馈、B 交易页和 C 保存检查点提示；探针捕获 `interaction_input_observed=true`、`save_input_observed=true`。到达码头、泊位、酒馆、仓库和出生点，但未到达市场、船坞和返港锚点；离港、返港、保存后重载未完成。原始证据：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN4.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN4_PLAYER_FROM_A02.log`。
- A01 RERUN4 同时记录了重复的不可行走碰撞体恢复警告，说明当前路线仍有碰撞/路线连续性风险；不能以“观察到保存提示”替代保存—退出—重载恢复证据。
- A02 RERUN14 的 Unity 构建与审计退出码为 0；R03 记录 311 Renderer、310 个启用 Renderer、5,996 个网格顶点、0 个 R03 Collider。新增独立 `D19_R03_ShipVisual.prefab` 并挂载到稳定 `ph_ship_fishing` 下，稳定船根统计为 19 Renderer/17 个启用 Renderer，保留 D2/D6 节点所有权。真实截图：`game/tools/results/D19/D19-R03-A02/screenshots/r03_a02_ship_visual_rerun14_player.jpg`。该截图未见粉色材质或大面积白色代理面，但近景屋顶/路面重叠、粗糙构图以及固定镜头/天气证据仍未闭合。
- A03 RERUN16 的真实 1920×1080 三档性能事实不变：实际质量名为 `Very Low`/`Low`/`Medium`，平均 FPS 为 `59.9967/59.9942/59.9944`，1% low 为 `59.7199/59.5348/59.5566`；不把请求参数 `high` 解释成项目 `High`。

最新门禁结论仍为 **BLOCKED**：完整路线、保存恢复、14 个固定 Player 镜头、三种天气变体和视觉债务关闭均没有同时满足；因此不放行 `D19-R02-S02` 或任何 D20 正式集成任务。

## RERUN7_FULLSCREEN 环境阻断增量（2026-08-16）

本轮只复验了真实 Player 的主菜单入口，没有启动正式 D19 重验。主菜单和 EventSystem 初始化成功，但 Windows Security 防火墙提示覆盖 Player，鼠标控制被路由到 `PickerHost.exe`，键盘也没有触发 `New Game`。未触碰该系统提示，未把主菜单截图解释为港口路线证据。

机器记录：

- `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN7_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN7_ROUTE_LOG.json`
- `game/tools/results/D19/D19-R03-A01/screenshots/mainmenu_rerun7_system_blocker.jpg`

该增量不能减少既有阻断：完整路线、保存/重载、14 固定镜头、3 天气变体和最终视觉收口仍未闭合；`D19-R02-S02` 与 D20 正式集成继续不放行。

## RERUN19 目标视觉修复后复核（2026-08-16）

本节只追加 RERUN17/RERUN19 事实，保留所有历史 BLOCKED/PARTIAL 结果。

- A01 RERUN10：非调试真实 Player 仍停在 MainMenu，Windows 安全提示覆盖窗口；未安全操作该提示，未进入 InitialHarbor，路线与保存重载均无新通过事实。
- A02 RERUN17：真实 Player 固定镜头 14/14、1920×1080 尺寸 14/14；材质/三角树/浅色竖杆问题有所改善，但平坦地表/码头、块状建筑、近岸代理几何和 Calm-only（0/3）仍失败。逐镜头记录：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_VISUAL_QA_RERUN17.json`。
- A03 RERUN17：Low/Medium/High 三档真实 1920×1080、30 秒预热 + 60 秒采样，性能子任务 PASS。

新的门禁机器结果为 `game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN19.json`，结论继续 **BLOCKED**。当前不放行 `D19-R02-S02`、`D20-A01-R02`、`D20-A02-R02` 或 `D20-S02-R01`。

## RERUN20 非弹窗真实 Player 增量（2026-08-16）

本节追加 RERUN10 的真实证据，不覆盖 RERUN19 或更早结论。机器汇总：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN20.json`。

- A01 使用不带 `-popupwindow` 的真实 Windows Player，`1920x1080`，连续运行 `704.296936` 秒；真实输入观察到人物移动、Character/Ship 切换、E 交互和保存输入，生成 16 张 1920x1080 路线截图。
- 真实到达 `anchor.player.spawn`、`anchor.dock.primary`、`anchor.berth.primary`、`building:tavern_santa_margarita`；市场、仓库、船坞、返港未到达。日志仍有不可行走碰撞体恢复提示。
- 关闭路线 Player 后，新 Player 从 MainMenu Continue 加载成功；日志记录 InitialHarbor 重建和 `D19_TerrainRoot` 落地。该证据使“保存输入+退出后 Continue 恢复”成为有限范围 PASS，但不等于完整离港—返港存档闭环。
- A02 RERUN17 的人工视觉问题与 Calm-only/0/3 天气变体不变；A03 RERUN17 三档性能子门槛不变。

因此整体仍为 **BLOCKED**：完整关键路线、视觉债务和天气变体未闭合；不启动 `D19-R02-S02`，不放行 D20 正式集成。详见 `docs/game/design/common/D19_R03_S01_RERUN20_REAL_PLAYER_REVIEW.md`。

## RERUN39 / RERUN36_CURRENT 当前门禁复核（2026-08-16）

本节追加当前 R03 场景版本的真实证据，不覆盖历史 RERUN20、RERUN29 或 RERUN35。机器汇总：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN39_CURRENT.json`。

- A01 RERUN39：真实 Player `211.7932586669922s`、1920×1080；菜单、新游戏、上船、下船、移动输入和保存反馈可见；只到达出生、酒馆、码头、泊位，市场、仓库、船坞、返港和同轮 Continue 未完成。5 张截图位于 `game/tools/results/D19/D19-R03-A01/screenshots/rerun39/`，不构成完整路线证据。
- A03 RERUN36_CURRENT：当前场景 Low/Medium/High 三档均真实 30 秒预热 + 60 秒采样、1920×1080；Medium/High 的 1% low 约 59.65/59.64，Low 的真实 1% low=7.903 FPS，性能整体不能放行。
- 场景碰撞审计确认旧 R02 地形碰撞已禁用、R03 `D19_TerrainRoot` 启用，但这不能替代关键路线可达性。
- A02 仍为 Calm-only，固定镜头虽有 14/14 文件但低细节/平坦区域与开发信息仍未清除，Rain/Storm 为 0/3。

最新结论保持 **BLOCKED**；`D19-R02-S02` 与 D20 正式集成继续不放行。下一步必须解决真实连续输入路线、同轮保存/退出/Continue、Low 性能异常、视觉债务和天气矩阵。

## RERUN41 参数化探针复验（2026-08-16）

本节追加一次带专属 `-d19-r03-route-rerun41` 参数的真实 Windows Player 记录，不覆盖 RERUN39、RERUN35 或更早证据。结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN41_CURRENT.json`。

- Player 请求分辨率为 1920×1080，实际运行 `59.76485061645508s`；Player/controller/router、移动输入、E 交互、B 上下船和 C 保存输入均被探针观察到。
- 本轮到达 `anchor.player.spawn`、`building:market_santa_margarita`、`building:tavern_santa_margarita`、`anchor.dock.primary`、`anchor.berth.primary`；未到达仓库、船坞和返港点，未完成出航—返港或同轮退出—Continue。
- 运行时探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN41.json`；Player 日志：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN41_PLAYER_RERUN2.log`。
- 运行没有产生当前轮截图；历史固定镜头未被改名为当前运行证据。门禁继续 **BLOCKED**，不放行 `D19-R02-S02` 或 D20 正式集成。

## RERUN42 定向 Player 复验（2026-08-16）

本节追加一次新的非开发 Player 运行，不覆盖 RERUN41 及更早结果。机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN42_MACHINE_RESULT.json`。

- 真实 Windows Player 请求 1920×1080，运行 `200.1754150390625s`；Player/controller/router、移动输入和交互输入被探针观察到，当前轮生成 6 张 `route_rerun42_*.png`。
- 本轮仅到达 `anchor.dock.primary` 与 `anchor.berth.primary`；市场、仓库、酒馆、船坞、返港和出生锚点未形成新的到达记录，保存输入与同轮 Continue 未证明。
- 运行时探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN42.json`；Player 日志：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN42_PLAYER.log`。
- 该轮仍为 **PARTIAL**，没有减少 S01 的硬阻断；`D19-R02-S02` 与 D20 正式集成继续不放行。

## RERUN45/RERUN46 旧地形 Collider 修复后当前门禁（2026-08-16）

本节追加 R03 Integration 的最小碰撞收敛及两轮真实 Player 复验，不覆盖历史门禁。最新机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN46_CURRENT.json`。

- R03 桥接层禁用 D19-A01 旧地形 Collider 1 个，并继续禁用旧码头/跳板重复 Collider 3 个；稳定 ID、D6 地面/船体所有权和资源状态均保留。
- RERUN45：真实 Player `120.1856460571289s`、1920×1080 请求、2 张截图；到达酒馆/码头/泊位，Player/controller/router、移动/交互为 true，保存为 false。结果：`D19_R03_A01_REAL_PLAYER_RERUN45_MACHINE_RESULT.json`。
- RERUN46：真实 Player `126.42691040039063s`、1920×1080 请求、4 张截图；到达出生/酒馆/码头/泊位，移动/交互为 true，保存为 false；市场、仓库、船坞、返港未到达。结果：`D19_R03_A01_REAL_PLAYER_RERUN46_MACHINE_RESULT.json`。
- RERUN46 日志仍有不可行走碰撞恢复提示，未完成 300 秒路线、出航—返港、保存—退出—Continue。门禁继续 **BLOCKED**，不放行 `D19-R02-S02` 或 D20 正式集成。
