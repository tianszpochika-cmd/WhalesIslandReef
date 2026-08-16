# D19-R01-S02 圣玛格丽特港最终场景复验报告

**任务：** D19-R01-S02  
**日期：** 2026-08-15  
**结论：** **PARTIAL**  
**放行：** 不允许进入 D20-S02

本报告是 D19 当前正式门禁状态的新一轮真实复验，不覆盖或改写历史报告。D19-S02、D19-S02-R01 和 D19-R01-B 的 PARTIAL 证据均保留。本轮没有修改 `InitialHarbor`、D19 Integration Prefab、Systems、Registry、存档、contracts、server、web 或资源状态。

## 1. 验收范围与基线

本轮开始时核对：

- D18-S02-R01 仍为 PASS；准入目录为 `docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json`。
- `LocalDevelopmentReady=0`、`ReleaseApproved=0`；本轮没有把 `intake_eligible` 推升为任何运行时或发布状态。
- 当前正式场景 SHA-256：`4C45779C7B5A020583E9F11F9F8F61642C12F1EC3920E8001F4752EF626D7E9D`，165,574 bytes。
- R01-B Integration Prefab SHA-256：`C7DC7401A38E5C6D7528CAB7D86640ACE8E619CC2B1B5BD0402134DDE1E3BD08`，934,641 bytes。
- 场景起始和结束哈希相同；本轮未对正式场景写入修正。
- 回滚快照保留在 `game/tools/results/D19/D19-R01-B/D19_R01_B_ROLLBACK_INITIALHARBOR.unity`。

机器记录：

- [D19_R01_S02_MACHINE_RESULT.json](../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_MACHINE_RESULT.json)
- [D19_R01_S02_RESOURCE_REFERENCE_AUDIT.json](../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_RESOURCE_REFERENCE_AUDIT.json)

## 2. 资源准入与场景引用

### 2.1 静态门禁结果

R01-B 的 53 条外部资源使用记录去重后得到 32 个 `content_id`。32/32 都能在最新 `d18_prefab_candidate_intake_catalog.json` 的 `eligible_entries` 中找到，缺失 0 条；场景仅挂 Integration 根节点/Prefab 引用。资源类别为 Nature、Building、Pirate/Dock Props；R01-B 项目自有材质/地形表现继续按项目自有表现资产处理。

本轮没有修改候选 manifest、intake catalog、asset_state、Registry 或实体 ID。准入目录的真实状态仍为：

| 项目 | 真实值 |
|---|---:|
| catalog manifest_total | 28 |
| catalog manifest_intake_eligible | 24 |
| candidate_file_total | 1,863 |
| intake_eligible_file_total | 1,833 |
| LocalDevelopmentReady | 0 |
| ReleaseApproved | 0 |
| 正式场景使用的唯一外部 content_id | 32 |
| 正式场景使用的外部 ID 缺失 | 0 |

只读命令及结果：

```text
python tools/d18_prefab_candidate_intake_audit.py --project-root . --catalog docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json --output game/tools/results/D19/D19-R01-S02/D19_R01_S02_INTAKE_AUDIT.json
PASS; errors=[]

python tools/d18_asset_manifest_audit.py --project-root . --manifest-root docs/game/design/common/assets/packs --output game/tools/results/D19/D19-R01-S02/D19_R01_S02_MANIFEST_AUDIT.json
PASS; manifest_count=26; error_count=0; issue_count=0
```

准入工具扫描输入的统计为 manifest 27、candidate 1,862、intake 文件 1,674；catalog 自身快照为上表的 28/1,863/1,833。本报告保留两组原始统计，不静默归一化；32 个正式场景引用通过了独立的 `content_id` 对照。该统计差异是后续资源库工具复核项，不被包装为运行时就绪。

### 2.2 资源门槛结论

资源引用和准入证据静态通过；运行时消费者、碰撞/锚点/LOD/Animator 的最终晋级仍未通过 D18-S03，不能据此标记 `LocalDevelopmentReady` 或 `ReleaseApproved`。

## 3. 真实 Player 路线

### 3.1 Main Menu 路线

在隔离 Player 中尝试了真实菜单操作。菜单截图真实存在，但 `New Game → 角色确认 → InitialHarbor` 的场景切换没有观察到，Player 日志没有对应的 NewGame/InitialHarbor 转换标记。因此不能把菜单尝试算作完整新游戏路线。

证据：

- `game/tools/results/D19/D19-R01-S02/d19_r01_s02_route_driver.ps1`
- `game/tools/results/D19/D19-R01-S02/route_player.log`
- `game/tools/results/D19/D19-R01-S02/screenshots/route_00_main_menu.png`

### 3.2 直接进入 InitialHarbor 的补充路线

为避免把菜单失败误读成场景路线，本轮另用真实键盘输入直接进入 InitialHarbor 取证。该补充路线不证明角色创建，且没有完成所需 300 秒：从 11:19:42 到最后有效 `route_09` 帧 11:23:56，约 254 秒；驱动预期的 71 次 W/A/E/P/M/T/D/F5 等输入没有形成完整的交互闭环，驱动本身也没有写出最终 JSON。

| 节点 | 真实输入/稳定 ID | 实际观察 | 结论 |
|---|---|---|---|
| 出生/落地 | W；`D19_Anchor_D2_PlayerAvatar` | 有出生帧；日志反复输出 BeachGround 恢复 | PARTIAL |
| 移动 | W/A；`D19_Anchor_santa_margarita` | 输入由驱动尝试，完整位置路线不可证明 | NOT_CONFIRMED |
| 关键 NPC | E；`D19_Anchor_npc_santa_margarita_dockhand` | 未见对话或明确失败反馈 | BLOCKED |
| 市场 | P/M/Escape；`D19_Anchor_building_market_santa_margarita` | 未见可复核的市场结算反馈 | NOT_CONFIRMED |
| 码头/泊位 | P/B；`D19_Anchor_berth_santa_margarita` | 未见状态转换 | BLOCKED |
| 登船/下船 | E；`D19_Anchor_GangplankGround` | 未见 Ship/Character 模式变化 | BLOCKED |
| 出航 | D；`D19_Anchor_departure_channel_santa_margarita` | 未见可复核船舶出航画面 | NOT_CONFIRMED |
| 保存 | F5；`D19_Anchor_save_point_santa_margarita` | Player 日志未给出可复核保存确认 | NOT_CONFIRMED |
| 返港/重载 | N/L/B/F5；`D19_Anchor_return_channel_santa_margarita` | 在 route_09 前结束，未完成 | BLOCKED |

本轮有效全屏路线帧为 10 张，全部为 1920×1080：`route_00_main_menu.png`、`route_01_spawn.png`、`route_02.png`—`route_09.png`。`route_external_debug*.png` 未计入路线证据。路线机器结果见 [D19_R01_S02_ROUTE_LOG.json](../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_ROUTE_LOG.json)。

**路线门槛：FAIL。** 没有完成“出生 → 市场 → 仓库 → 酒馆 → 码头 → 登船 → 出航 → 返港 → 保存 → 重载”的真实 Player 闭环，不能 PASS。

## 4. 真实 1920×1080 性能

Player 使用 1920×1080、16:9、VSync=0，Low/Medium/High 每档 warm-up 30 秒、采样 60 秒；不是 `Camera.Render` 或批处理无窗口采样。详细原始结果：

| 档位 | 平均 FPS | 1% low | p95 帧时间 | GC Gen0/1/2 | GPU | 结论 |
|---|---:|---:|---:|---:|---|---|
| Low | 13.54 | 0.18 | 17.42 ms | 26/26/26 | unavailable | FAIL |
| Medium | 57.85 | 11.62 | 18.18 ms | 65/65/65 | unavailable | FAIL |
| High | 59.18 | 21.49 | 18.04 ms | 71/71/71 | unavailable | FAIL |

D18 目标是平均 60 FPS，Low 1% low≥50，Medium/High 1% low≥45。三档均未满足；Medium/High 的平均值也低于 60。采样器报告 `actual_resolution=1920x1080`，同时保留 `current_resolution=3840x2160` 的不一致信号；工作集/私有内存接口返回 0/0，不能当作真实零内存。GPU timing unavailable，不能用 CPU 时间替代 GPU 时间。

原始档位文件：

- [Low.json](../../../../game/tools/results/D19/D19-R01-S02/performance/Low.json)
- [Medium.json](../../../../game/tools/results/D19/D19-R01-S02/performance/Medium.json)
- [High.json](../../../../game/tools/results/D19/D19-R01-S02/performance/High.json)
- 汇总：[D19_R01_S02_PERFORMANCE_1080P.json](../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_PERFORMANCE_1080P.json)

**性能门槛：FAIL。** 责任方向是 D19 Integration 的渲染/材质/阴影/透明与实例规模，以及采样器的分辨率和 GPU/工作集 telemetry；不得删逻辑或删内容掩盖超限。

## 5. 固定镜头和视觉检查

D19 规定的 14 个固定镜头在 R01-B 中有 14 张 Editor `Camera.Render` 截图，但它们不是本轮真实 Player 镜头。本轮真实 Player 只生成了 10 张路线帧，没有完成 14 个固定镜头和 3 个天气变体，因此历史 Editor 截图不被重新标记为 Player 通过证据。

对当前 Player 的出生、主街/市场邻接和市场/码头邻接帧进行人工检查，发现：

- 大面积低细节、分面明显的地形仍在；
- 建筑、码头和道具轮廓仍偏方块/灰盒，未达到最终港口样板的视觉门槛；
- D19/D5 教程、状态、事件和控制面板大量叠加，关键画面被调试/开发层遮挡；
- Player 帧未观察到粉色缺材质，但这只是已捕获帧范围内的结论；
- 日间当前帧未观察到旧版纯白/薄荷绿主平面，但这不能抵消灰盒、叠字、镜头贴近和路线反馈问题；
- 物理日志反复输出 `玩家已恢复可行走接地: surface=BeachGround`，需要检查出生/地面接触的恢复频率和真正路线可达性；
- 近海、返港和天气镜头本轮未完成。

机器视觉清单：[D19_R01_S02_VISUAL_QA.json](../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_VISUAL_QA.json)。

**视觉门槛：FAIL/PARTIAL。** 不是通过改镜头、裁剪、浓雾或关闭对象得到的结论；应由新的 D19 Integration 修复任务处理。

## 6. 回归测试

### 6.1 当前本轮实际运行

隔离副本删除了已知 D20 缺失命名空间测试文件，仅用于不越权地运行 D19 测试；没有将副本变化写回主仓库。

```text
Unity.exe -batchmode -nographics -quit -projectPath <isolated-copy> -executeMethod D19R01S02TestRunner.RunEditD19
23 passed, 0 failed, 0 skipped
```

结果：[D19_R01_S02_EDITMODE_RESULTS.xml](../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_EDITMODE_RESULTS.xml)。

PlayMode 曾分别尝试全量 D19 类别和单独 `D19S02ScenePlayModeTests`；两个运行都停在 `Unity.PerformanceTesting.Editor.TestRunBuilder` 预构建阶段，没有产生 XML，已停止并保存日志，不能记为通过：[D19_R01_S02_PLAYMODE_TIMEOUT.json](../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_PLAYMODE_TIMEOUT.json)。

主项目已有编译阻断仍在：`Assets/Tests/EditMode/D20/D20WildlifePresentationTests.cs` 引用缺失的 `WhalesIslandReef.Presentation`。本轮没有修改 D20 文件绕过它。D17 的历史回归 XML 保留，但因本轮 PlayMode 超时没有重新分类为当前门禁通过。

### 6.2 工具测试与差异检查

```text
python -m pytest -q tools/tests/test_d18_prefab_candidate_intake.py tools/tests/test_d18_asset_manifest_audit.py
FAIL: No module named pytest

python -m unittest tools.tests.test_d18_prefab_candidate_intake tools.tests.test_d18_asset_manifest_audit
Ran 11 tests in 0.138s; OK

git diff --check
exit code 0; only existing LF/CRLF conversion warnings
```

pytest 缺失不被包装为测试通过；Python 标准库 unittest 的同一组 11 个用例真实通过。

## 7. 门禁结论和责任顺序

| 门禁 | 结论 | 责任/下一步 |
|---|---|---|
| 资源 ID/准入静态对照 | PASS_STATIC_ONLY | 保持 D18-S03 边界；不晋级资源状态 |
| 新游戏到重载真实路线 | FAIL | 新 D19 路线/交互修复任务；完成 NPC、登船、出航、返港、保存恢复反馈 |
| 1080p 三档性能 | FAIL | D19 Integration 性能修复；补真实 GPU/工作集与分辨率一致性 |
| 14 个 Player 固定镜头/天气 | NOT_RUN + visual fail | D19 重新采集并消除叠字/灰盒/方块债务 |
| D19 EditMode | 23/23 PASS | 仅代码/编辑器级当前证据 |
| D19 PlayMode | TIMEOUT | 先修测试预构建/运行阻塞，再重跑 |
| D17 回归 | 未重新分类 | PlayMode 可执行后重跑，不复用历史作为本轮 PASS |
| 主项目编译 | 已知 D20 阻断 | 责任移交 D20，不能在本任务越权修复 |

**最终结论：PARTIAL。** 本轮通过了资源引用的静态准入核对和隔离 D19 EditMode，但未通过真实玩家闭环、性能、固定 Player 镜头、PlayMode/D17 当前回归和视觉质量门槛。

**D20-S02：不允许开始。** 需要先完成新的 D19 修复与本任务的重新验收；不得把历史 D19-S02 或 D19-S02-R01 的 PARTIAL/PASS 文字替代新的真实证据。

## 8. 回滚与保留项

本轮没有场景修正，因此当前回滚点仍是 R01-B 快照。若后续修复需要回滚：关闭 Unity，不保存；用 `D19_R01_B_ROLLBACK_INITIALHARBOR.unity` 及匹配 `.meta` 恢复，随后重新核验场景 SHA、Integration 根节点、17 个 R01-B 稳定锚点和资源引用。保留本轮所有性能、路线、PlayMode timeout、截图和失败记录，禁止删除失败证据。
