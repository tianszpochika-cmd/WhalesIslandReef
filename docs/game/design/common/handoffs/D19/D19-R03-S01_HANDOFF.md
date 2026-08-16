# D19-R03-S01 交接

- 任务：D19-R03-S01
- 日期：2026-08-16
- 结论：**BLOCKED**
- 性质：R03 预检/正式重验门禁；正式 D19 Player 验收尚未启动。

## 读取的真实证据

- `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN2_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R03-A03/D19_R03_A03_MACHINE_RESULT.json`
- 历史 `game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN8_MACHINE_RESULT.json`

## 实际结论

- A01 RERUN2 的真实 Player 持续 584.5516 秒，移动、落地、Character/Ship 和部分交互可观察；但市场/仓库/船坞/返港、保存重载闭环未完成。
- A02 的真实 Player 已移除一部分旧代理渲染，但仍有平面与块状视觉债务；14 固定镜头和天气变体均未采集。
- A03 的真实 Player 1920×1080 采样可复核，但实际项目档名为 Very Low/Low/Medium，不能替代完整 D19 验收。

## 下游状态

`D19-R02-S02` 不放行；`D20-A01-R02`、`D20-A02-R02`、`D20-S02-R01` 不启动。下一次重验必须使用新命名结果并在闭合路线、保存恢复、固定镜头和视觉债务后执行。旧报告不删除、不覆盖。

## RERUN13/RERUN16 增量（2026-08-16）

- A02 RERUN13 已将生成网格持久化为 297 个 `.asset` 文件，真实 Player 画面可见港口几何；这修复了旧版运行时 `MeshFilter` 空引用问题，但没有闭合 14 个固定镜头、天气变体和最终视觉质量门槛。
- A03 RERUN16 已在真实独立 Player 的 `InitialHarbor` 场景采集三档 1920×1080、30 秒 warm-up + 60 秒采样；平均 FPS 59.9967/59.9942/59.9944，1% low 59.7199/59.5348/59.5566。项目实际档名是 Very Low/Low/Medium，不能解释成 High。
- A01 RERUN2 的完整路线和保存/重载仍未闭合，因此本交接结论保持 **BLOCKED**；没有启动正式 D19-R02-S02，也没有放行 D20。

## RERUN4/RERUN14 最新增量（2026-08-16）

本节只追加真实证据，保留此前 RERUN2/RERUN13/RERUN16 和 BLOCKED 结论。

- A01 RERUN4：真实 Windows Player 运行 `301.169769` 秒，1920×1080、D3D11；移动、E 登船/下船、B 交易页和 C 保存检查点提示均实际观察到。探针为 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN4.json`，日志为 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN4_PLAYER_FROM_A02.log`。实际到达码头、泊位、酒馆、仓库、出生点，未到达市场、船坞、返港；离港、返港、保存后重载未验证。重复不可行走碰撞体恢复警告仍在，因此 A01 保持 PARTIAL。
- A02 RERUN14：Unity Builder、Editor audit、Windows Player build 均成功；R03 为 311 Renderer/310 启用/5,996 顶点/0 R03 Collider。新增 `game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_ShipVisual.prefab` 并作为实例放在稳定 `ph_ship_fishing` 下，稳定船根为 19 Renderer/17 启用，保留 D2/D6 所有权。真实截图为 `game/tools/results/D19/D19-R03-A02/screenshots/r03_a02_ship_visual_rerun14_player.jpg`。无粉色或大面积白色代理面，但近景屋顶/路面重叠、固定镜头、天气变体和视觉收口仍未通过，A02 保持 PARTIAL。
- A03 RERUN16：三档真实 1920×1080 Player 性能证据仍可复核，实际档名为 Very Low/Low/Medium，平均 FPS 59.9967/59.9942/59.9944，1% low 59.7199/59.5348/59.5566；不能替代项目 High 档证据。

### 最新交接结论

`D19_R03_S01_MACHINE_RESULT.json` 已追加 RERUN4/RERUN14 的机器事实；`conclusion` 仍为 **BLOCKED**。完整路线、保存/重载、14 个玩家固定镜头、三种天气变体和最终视觉债务尚未闭合，所以 `D19-R02-S02`、D20 R02 任务均不放行。没有修改 Systems、Save Schema、Registry、contracts 或资源状态。

## RERUN7_FULLSCREEN 增量（2026-08-16）

- 真实 Player 主菜单启动和 UI 初始化日志可复核；全屏取景为控制工具返回的 2194×1234 桌面截图，启动参数仍记录了 1920×1080 请求。
- Windows Security 防火墙提示覆盖窗口，点击被识别为 `PickerHost.exe`，Return/Tab/Space 未激活新游戏；没有触碰或关闭系统提示。
- 未进入 InitialHarbor，因此本轮不增加任何路线、交互、保存或重载通过事实。新结果保持 `D19-R03-A01=PARTIAL`，并与历史 RERUN4/RERUN6 分开保存。
- 结论不变：`D19-R03-S01=BLOCKED`，不得启动 `D19-R02-S02` 或 `D20-S02-R01`。

## RERUN10/RERUN17 增量（2026-08-16）

- A01 RERUN10 使用不带 `Development/AllowDebugging` 的真实 Windows Player，构建和启动日志分别为 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN10.log`、`D19_R03_A01_MAINMENU_ROUTE_RERUN10_PLAYER.log`。MainMenu/EventSystem/路线探针初始化成功，但已有 Windows 安全提示仍覆盖窗口；未发送输入、未进入 InitialHarbor、未写路线探针 JSON。机器结果与路线日志：`D19_R03_A01_RERUN10_MACHINE_RESULT.json`、`D19_R03_A01_RERUN10_ROUTE_LOG.json`。未操作系统提示，A01 仍 PARTIAL。
- A03 RERUN17 已修正档位名称解析，真实 Player 三档为 `Low/Medium/High`，各自 1920×1080、30 秒预热 + 60 秒采样；性能子任务范围为 PASS。机器结果：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_MACHINE_RESULT_RERUN17.json`。
- 本次 S01 汇总结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN17.json`，结论仍为 **BLOCKED**。完整路线/保存重载、14 固定镜头、3 天气变体和视觉债务尚未闭合；因此不放行 `D19-R02-S02` 或 D20 正式集成。

## RERUN18 固定镜头汇总（2026-08-16）

- A02 RERUN15 已在真实 Player 1920×1080 生成 14/14 固定镜头；结果 `game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN15.json`。这闭合了“镜头文件数量”子门槛，但人工视觉仍发现平面/代理柱/屋顶重叠/开发 HUD，且天气变体为 0/3。
- 最新 S01 汇总：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN18.json`，结论继续 **BLOCKED**。A01 路线和保存重载仍未完成，A02 视觉与天气仍未通过，故不放行 D19-R02-S02 或 D20 正式集成。

## RERUN19 目标视觉修复后复核（2026-08-16）

- 本轮明确保留 RERUN18 与更早的 BLOCKED/PARTIAL 结果；没有覆盖历史机器结果。
- A01 继续使用 RERUN10：真实非调试 Player 能启动 MainMenu，但 Windows 安全提示仍覆盖窗口；未发送系统提示输入，未进入 InitialHarbor，路线、保存和重载仍为 false。
- A02 使用 RERUN17：真实 Player 1920×1080 固定镜头 14/14、尺寸 14/14；旧三角树/浅色竖杆有所降低，但平坦地表/码头、块状建筑、近岸代理几何和 Calm-only 仍使视觉门禁失败。逐镜头 QA 为 `game/tools/results/D19/D19-R03-A02/D19_R03_A02_VISUAL_QA_RERUN17.json`。
- A03 使用 RERUN17：Low/Medium/High 三档真实 1920×1080 30 秒预热 + 60 秒采样，性能子任务通过。
- 新机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN19.json`，结论仍为 **BLOCKED**。

因此 `D19-R02-S02`、`D20-A01-R02`、`D20-A02-R02` 和 `D20-S02-R01` 继续不放行。当前剩余硬门槛是安全提示下的真实菜单/路线/保存恢复、视觉债务收口以及 Rain/Storm 真实表现证据；不得用静态检查、固定镜头数量或旧路线截图替代。

## RERUN20 非弹窗真实 Player 增量（2026-08-16）

- 新机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN20.json`。
- 新复核报告：`docs/game/design/common/D19_R03_S01_RERUN20_REAL_PLAYER_REVIEW.md`。
- A01 RERUN10 使用不带 `-popupwindow` 的真实 Player，连续 `704.296936` 秒，1920x1080；移动、Character/Ship 控制切换、E 交互、保存输入和 16 张真实截图均有证据。
- 本轮到达出生点、码头、泊位、酒馆；市场、仓库、船坞和返港未到达。探针/日志继续记录不可行走碰撞体恢复，完整路线仍未通过。
- 关闭路线 Player 后，新 Player 从 MainMenu Continue 加载成功，记录 InitialHarbor 重建与 D19_TerrainRoot 落地。保存—退出—Continue 恢复是有限范围 PASS；离港—返港后保存恢复仍未证明。
- A02 RERUN17 的 14/14 截图数量和 A03 RERUN17 三档性能子门槛保留，但视觉债务与 Rain/Storm 0/3 仍未闭合。

### RERUN20 决策

`D19-R03-S01` 继续 **BLOCKED**；不启动 `D19-R02-S02`，不放行 `D20-A01-R02`、`D20-A02-R02` 或 `D20-S02-R01`。本轮未修改 Systems、Save Schema、Registry、contracts 或资源状态；历史 RERUN19 及更早结果均保留。

## RERUN29_GATE_REVIEW（2026-08-16）

- 新机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN29.json`。A01 证据与探针分别为 `D19_R03_A01_REAL_PLAYER_RERUN29_MACHINE_RESULT.json`、`D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN29_LONG.json`。
- RERUN29 真实 Player 连续运行 `694.13134765625s`、`1920×1080`，Player/controller/router、位移、交互输入、保存输入、保存反馈和登船/下船均有证据；16 张截图已落盘。但只到达出生点、酒馆、码头、泊位，市场/仓库/船坞/返港未到达，同一轮退出后 Continue 重载未执行。
- A02 仍为 PARTIAL：8 个 intake-eligible 环境候选和 `D19_TerrainRoot` 已在 Integration 审计，但固定镜头视觉债务和天气变体未闭合；A03 仅为三档性能子任务 PASS。
- 因此本门禁继续 **BLOCKED**，不放行 `D19-R02-S02` 或任何 D20 正式集成；旧版 BLOCKED/PARTIAL 结果全部保留。

### RERUN11 证据完整性补充

随后一次目标路线短运行没有获得有效输入，且误复用了 `-d19-r03-route-rerun10` 参数，覆盖了原始 `D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN10.json`。已将该短运行归档为 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN11.json` 与 `D19_R03_A01_TARGETED_RERUN11_PLAYER.log`，并在 `D19_R03_A01_REAL_PLAYER_RERUN10_MACHINE_RESULT.json` 与 S01 RERUN20 结果中明确排除被覆盖探针作为独立证据。704 秒路线事实仍由已保存路线日志、Player 日志、16 张截图、机器结果和 Continue 重载日志支撑；没有将此次短运行或覆盖后的探针解释为通过。

## RERUN23 输入复验增量（2026-08-16）

- 本轮独立结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN23.json`；详细复验：`docs/game/design/common/D19_R03_S01_RERUN23_INPUT_RECHECK.md`。
- 新的真实 Player 日志确认桥接修复执行：`ShoreBoundary z=17.0`，D6 collider owner retained；但 `moved_observed=false`、`save_input_observed=false`，持续 `160.012863 s`，只在初始附近观察到 dock/berth 半径内地标。
- 结论继续 **BLOCKED**。不能把 HUD/碰撞时序修复当作路线通过；市场、仓库、酒馆、船坞、返港、离港和保存闭环仍未由本轮真实输入证明。
- `D19-R02-S02`、D20 R02 角色任务和 D20 正式集成仍不放行；历史机器结果和证据完整性说明均保留。

## RERUN35_GATE_REVIEW（2026-08-16）

- 本轮明确：R03 Integration 表现层在 RERUN29 门禁之后发生了几何变更，因此旧 RERUN29 的路线和性能不能直接作为当前场景的最终证据；旧结果保留不覆盖。
- 当前机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN35.json`。
- RERUN35 固定镜头在新几何后真实重新采集 14/14 张 1920×1080 backbuffer 截图；粉色材质/大面积纯白代理面未观察到，但平地、低细节建筑、代理岩体、开发 HUD 仍在，天气仍 Calm-only（0/3）。
- RERUN35 短 Player 运行约 254.29 秒，但本轮未形成可复核移动或保存边沿；因此没有路线、保存恢复或性能新通过证据。
- 结论继续 **BLOCKED**。D19-R02-S02、D20 正式集成不放行；下一轮必须在当前场景版本完成真实 5—10 分钟路线、同轮保存/退出/Continue、三档 1920×1080 性能和 Calm/Rain/Storm 复核。

## RERUN39 / RERUN36_CURRENT_GATE_REVIEW（2026-08-16）

- 当前路线复验：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN39_MACHINE_RESULT.json`。真实 Player 证明菜单、新游戏、上船、下船、移动输入和保存反馈存在，但只运行约 211.79 秒，市场/仓库/船坞/返港和同轮 Continue 未闭合；因此路线门禁仍 BLOCKED。
- 当前性能复验：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_MACHINE_RESULT_RERUN36_CURRENT.json`。Medium/High 通过采样阈值，Low 的 1% low=7.903 FPS 未通过；因此不能把当前性能门禁写成 PASS。
- 场景碰撞审计：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_SCENE_COLLIDER_AUDIT.json`；旧 `D19_R02_TerrainSurface` 已禁用，R03 `D19_TerrainRoot` 保持启用。该事实只证明碰撞所有权收敛，不证明完整路线可达。
- 视觉与天气门禁仍未闭合：当前固定镜头仍存在低细节/平坦区域与开发信息，Rain/Storm 仍为 0/3；没有将 RERUN39 的 5 张路线截图扩写成 14 镜头通过。
- 结论保持 **BLOCKED**；不放行 `D19-R02-S02`，不启动 D20 正式集成。历史 RERUN29/RERUN35 结果不覆盖。
- Low 性能复采 RERUN36_CURRENT2 的 1% low=59.624 FPS，但首轮当前场景结果的真实 7.903 FPS 仍保留；连续三轮取最差尚未完成，因此性能门禁仍不能写 PASS。

## RERUN41 参数化探针复验（2026-08-16）

- 本轮使用专属 `-d19-r03-route-rerun41` 参数重新构建并启动真实 Windows Player；未覆盖历史 RERUN29/RERUN35/RERUN39 结果。
- 真实运行 `59.76485061645508s`、请求 1920×1080。探针确认 Player/controller/router、移动、E 交互、B 上下船和 C 保存输入存在；到达出生、市场、酒馆、码头和泊位。
- 未到达仓库、船坞、返港；未完成出航—返港、同轮保存—退出—Continue；本轮当前截图数为 0，因此没有把历史截图扩写为新镜头证据。
- 机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN41_CURRENT.json`；路线结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN41_MACHINE_RESULT.json`；运行时探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN41.json`；日志：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN41_PLAYER_RERUN2.log`。
- 结论继续 **BLOCKED**。RERUN41 只补强了真实玩家输入和已到达节点的可复核性，没有闭合 D19-R03-S01 的完整路线、性能、天气和视觉门槛；`D19-R02-S02` 与 `D20-S02-R01` 继续锁定。

## RERUN42 定向 Player 复验（2026-08-16）

- 新的独立非 Development Player 真实运行 `200.1754150390625s`、请求 1920×1080；Player/controller/router、移动输入和交互输入真实可见，当前轮生成 6 张 `route_rerun42_*.png`。
- 本轮只到达 `anchor.dock.primary` 与 `anchor.berth.primary`；市场、仓库、酒馆、船坞、返港和出生锚点未形成新的到达记录；保存输入为 false，未执行同轮 Continue。
- 机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN42_CURRENT.json`；A01 结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN42_MACHINE_RESULT.json`；探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN42.json`。
- RERUN42 不改变结论：D19-R03-S01 继续 **BLOCKED**，D19-R02-S02 与 D20-S02-R01 继续锁定；历史结果均保留。

## RERUN44 当前门禁复验（2026-08-16）

- R03 Integration 桥接层本轮禁用 3 个旧重复码头/跳板碰撞体，保留 D6 运行时地面所有权；这是可回滚的集成修复，不改变稳定 ID、Systems、存档或资源状态。
- 独立真实 Player 运行 `193.38430786132813s`、请求 1920×1080，Player/controller/router、移动输入和交互输入为 true，保存输入为 false，生成 7 张当前轮截图；运行时船位 `(0,0,0)`，玩家与船体不重叠，最终位移约 `0.045m`。
- 本轮仍只到达 `anchor.dock.primary`、`anchor.berth.primary`。完整菜单→市场→仓库→酒馆→码头→登船→出航→返港→保存/退出/Continue 未完成，性能当前 Low 首轮异常、14 镜头视觉收口及 Rain/Storm 证据仍未闭合。
- 当前门禁结果为 **BLOCKED**，不放行 `D19-R02-S02` 或 D20 正式集成。机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN44_CURRENT.json`；A01 结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN44_MACHINE_RESULT.json`；探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN44.json`。

## RERUN45/RERUN46 当前门禁增量（2026-08-16）

- R03 Integration 本轮只增加一个可回滚的集成桥接：禁用 D19-A01 旧地形 Collider 1 个，叠加保留此前禁用的旧码头/跳板 Collider 3 个；D6 地面/船体所有权和稳定 ID 保持不变。
- RERUN45：真实 Player `120.1856460571289s`、1920×1080 请求、2 张截图；到达酒馆/码头/泊位，保存 false。RERUN46：真实 Player `126.42691040039063s`、1920×1080 请求、4 张截图；到达出生/酒馆/码头/泊位，保存 false。两个结果均为 A01 **PARTIAL**，路径见对应机器结果。
- 新增地形桥接后路线确实能离开初始区域并产生新的地标，但完整 300 秒路线、市场/仓库/船坞/返港、保存/退出/Continue 仍未闭合；RERUN46 日志仍有不可行走碰撞体恢复提示。当前 S01 继续 **BLOCKED**，不放行 D19-R02-S02 或 D20 正式集成。
- 最新机器门禁为 `game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN46_CURRENT.json`；它取代当前门禁引用，不删除或改写 RERUN44/RERUN42 等历史结果。
