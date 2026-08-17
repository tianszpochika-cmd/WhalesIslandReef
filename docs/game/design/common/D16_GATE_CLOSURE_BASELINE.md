# D16 门禁闭环基线（D16_GATE_CLOSURE_BASELINE）

> 本文件由 **D16-C01** 权威冻结并写入，是 D16 剩余发布门禁工作的基准输入。
> **2026-08-17 分派修订**：下方第 3 节 31 项对照表是 C01 当时的冻结快照。当前有效分派改为 `D16-C01`—`C12`、`D17-C01`—`C04`，见 [第 9B 章](../../AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATES.md)。旧 `D16-C13`—`C31` 禁止再派。`C01`—`C05` 编号保留。

---

## 1. 轨道边界与状态原则

1. **发布门禁轨 vs 逻辑开发轨**：
   - 本轨（D16/D17 发布门禁轨）仅处理真实目标设备实测、人工历史与敏感审核、真实授权资产接入、物理断电/跨平台恢复、安装器/更新/卸载边界、目标机原生崩溃与隐私、以及正式长线故事验收。
   - 逻辑轨（D18—D24 及 D19 港口逻辑）与本轨并行，但**编号、结果目录、状态词和验收口径完全独立**。
   - **禁止**使用 D18—D24 的 `LOGIC_PASS` 或 D19 逻辑推进结果来关闭本轨任务；**禁止**将本轨任务状态写为 `LOGIC_PASS`。
2. **权威事实源**：
   - 每个任务在其专用目录下的 `Dxx_Cnn_GATE_RESULT.json` 是唯一事实源。
   - 汇总文件 `docs/game/GATE_TASK_STATUS.md` 由 `python tools/sync_gate_task_status.py` 自动生成，禁止手动编辑。
3. **真实性原则**：
   - 任何未获得真实外部证据、真实人工审查决定或真实授权资源的任务，严禁虚标 `GATE_PASS`。
   - 证据缺失或前置未达成时，必须诚实标为 `GATE_BLOCKED` 或 `GATE_PARTIAL`。

---

## 2. D16 历史门禁真实缺口盘点

当前工作区虽然在自动化与本机代码层具备完整基线，但发布门禁的真实缺口如下：

| 旧门禁 / 任务 | 类别 | 当前真实缺口与阻断原因 | 权威审计与证据现状 |
|---|---|---|---|
| **D16-A01** (R03) | 目标设备性能 | 缺乏目标机（如 Steam Deck / 推荐及低端配置机器）在代表性大场景下连续运行 ≥60 秒的真实采样数据；缺峰值内存、GC 分配、同屏实体数、流式加载恢复及原始日志。 | 本机 RTX 4070 Ti 采样仅属本机基线，`tools/results/external/d16_a01_target_device.json` 当前缺失。 |
| **D16-A02** (R06) | 断电与跨平台存档 | 缺乏真实物理断电（或等价硬件断电演练）、跨至少两个平台（Windows + Linux/SteamOS/macOS）文件系统恢复、迁移失败保留原档的真实测试证据与原始存档对比。 | 本机进程强制终止 3/3 仅属代码级安全，`tools/results/external/d16_a02_power_cross_platform.json` 当前缺失。 |
| **D16-A03** (R04) | 真机输入与无障碍 | 缺乏真实物理手柄（XInput/Xbox/PS）、目标分辨率（如 1080p/2K/4K/Steam Deck 800p）显示、多场景窗口焦点切换、字体覆盖、字幕清晰度、色觉及低刺激的人工实测记录。 | 本机设备探测 `tools/results/d16_a03_device_probe_current.json` 保持 `IN_PROGRESS/PENDING_MANUAL`，`tools/results/external/d16_a03_device_accessibility.json` 当前缺失。 |
| **D16-A04** (R01) | 人工内容与权利复核 | 3,139 条登记条目中，仍有 **267 条 Pending**（涵盖 12 个批次），包含敏感历史条目、船员特质、疾病、道具等，必须由责任审核人逐条做出人工决定。 | 机器审计结构 0 错误，但 `tools/results/external/d16_a04_manual_review.json` 缺失，`pending_count` 尚未清零。 |
| **D16-A05** (R05) | 最终授权资产替换 | 2,918 个可视资源中，仍有 **2,264 个 placeholder**。必须替换为具备商业授权、真实来源、明确责任人、LOD、碰撞体、锚点及场景回归验证的最终资源。 | Kenney 候选包仅处于候选待审状态，`tools/results/external/d16_a05_final_assets.json` 缺失，`placeholder_count` 尚未清零。 |
| **D16-A06** (R02) | 目标机发布与隐私 | 缺乏在目标机器上的正式安装、覆盖更新、卸载保留存档、原生崩溃捕获分析、最终隐私政策人工复核；Steam 状态仍处于未授权、未执行。 | 本机工作区安装器烟测通过，但 `tools/results/external/d16_a06_target_release_privacy.json` 缺失，发布清单保留 7 条阻断。 |
| **D16-S01** | 全量长线验收 | 新游戏 → 完整游玩 → 保存 → 恢复 → 退出 → 更新 → 再次恢复的长线全流程尚未在上述 A01—A06 关闭后正式执行。 | `tools/d16_s01_readiness.py` 判定当前状态为 `BLOCKED`。 |
| **D16-S02** | 最终交接与发布决策 | 依赖 D16-S01 的全量通过，目前处于被 S01 严格阻塞状态。 | 尚未生成正式发布放行索引与决策。 |

---

## 3. 旧门禁编号到 D16-C01—C31 映射对照表

| 任务 ID | 波次 | 映射旧门禁 | 唯一写入范围 | 任务职责与完成准则 | 精确前置条件 |
|---|---|---|---|---|---|
| **D16-C01** | 16.0 串行 | 全章基线 | `docs/game/design/common/D16_GATE_CLOSURE_BASELINE.md` | 冻结剩余缺口、旧编号对照、六份外部 envelope 路径、禁止伪通过清单和波次开始条件。 | 无（D16 冻结第一项） |
| **D16-C02** | 16.1 并行 | A04 / R01 (批次 001) | `docs/game/design/common/reviews/D16_A04_BATCH_001.md` | 人工审查批次 001（25 条：asset_blockade_order 到 asset_d15_a06_model_function_cargo_davit）。逐条证据与单条写回。 | D16-C01 = GATE_PASS |
| **D16-C03** | 16.1 并行 | A04 / R01 (批次 002) | `docs/game/design/common/reviews/D16_A04_BATCH_002.md` | 人工审查批次 002（25 条：asset_d15_a06_model_function_cargo_hatch 到 asset_d15_a06_model_function_medical_berth）。 | D16-C01 = GATE_PASS |
| **D16-C04** | 16.1 并行 | A04 / R01 (批次 003) | `docs/game/design/common/reviews/D16_A04_BATCH_003.md` | 人工审查批次 003（25 条：asset_d15_a06_model_function_medicine_cabinet 到 asset_d15_a06_model_island_packet_sloop）。 | D16-C01 = GATE_PASS |
| **D16-C05** | 16.1 并行 | A04 / R01 (批次 004) | `docs/game/design/common/reviews/D16_A04_BATCH_004.md` | 人工审查批次 004（25 条：asset_d15_a06_model_longwake_barque 到 asset_d15_a06_model_ship_module_crew_rest_area）。 | D16-C01 = GATE_PASS |
| **D16-C06** | 16.1 并行 | A04 / R01 (批次 005) | `docs/game/design/common/reviews/D16_A04_BATCH_005.md` | 人工审查批次 005（25 条：asset_d15_a06_model_ship_module_crew_storage_lockers 到 asset_d15_a06_model_ship_module_light_canvas）。 | D16-C01 = GATE_PASS |
| **D16-C07** | 16.1 并行 | A04 / R01 (批次 006) | `docs/game/design/common/reviews/D16_A04_BATCH_006.md` | 人工审查批次 006（25 条：asset_d15_a06_model_ship_module_light_cargo_dividers 到 asset_d15_a06_model_ship_module_spare_bucket_line）。 | D16-C01 = GATE_PASS |
| **D16-C08** | 16.1 并行 | A04 / R01 (批次 007) | `docs/game/design/common/reviews/D16_A04_BATCH_007.md` | 人工审查批次 007（25 条：asset_d15_a06_model_ship_module_spare_mast_kit 到 asset_vegetation_profile_crop）。 | D16-C01 = GATE_PASS |
| **D16-C09** | 16.1 并行 | A04 / R01 (批次 008) | `docs/game/design/common/reviews/D16_A04_BATCH_008.md` | 人工审查批次 008（11 条：asset_vegetation_profile_epiphyte 到 asset_vegetation_profile_wetland）。 | D16-C01 = GATE_PASS |
| **D16-C10** | 16.1 并行 | A04 / R01 (批次 009) | `docs/game/design/common/reviews/D16_A04_BATCH_009.md` | 人工审查批次 009（25 条历史内容：animal_domestic_cattle 到 item_fruit_guava）。 | D16-C01 = GATE_PASS |
| **D16-C11** | 16.1 并行 | A04 / R01 (批次 010) | `docs/game/design/common/reviews/D16_A04_BATCH_010.md` | 人工审查批次 010（25 条历史内容：item_gear_cast_net 到 item_seed_sugarcane）。 | D16-C01 = GATE_PASS |
| **D16-C12** | 16.1 并行 | A04 / R01 (批次 011) | `docs/game/design/common/reviews/D16_A04_BATCH_011.md` | 人工审查批次 011（25 条历史内容：item_seed_tobacco 到 outfit_injury_old_leg）。 | D16-C01 = GATE_PASS |
| **D16-C13** | 16.1 并行 | A04 / R01 (批次 012) | `docs/game/design/common/reviews/D16_A04_BATCH_012.md` | 人工审查批次 012（6 条历史内容：outfit_shoes_deck_boots 到 reward_d14_worldclass_reinforced_keel）。 | D16-C01 = GATE_PASS |
| **D16-C14** | 16.2 串行 | A04 关闭审计 | `tools/results/external/d16_a04_manual_review.json` | 复核 267 条全部有责任人决定，生成 A04 外部 envelope，确认 `pending_count=0`。 | D16-C02—C13 全部 GATE_PASS |
| **D16-C15** | 16.3 并行 | A05 预检 | `tools/results/d16_a05_replacement_preflight_current.json` | 对第一批最终替换包执行只读预检，验证来源/授权/LOD/碰撞/锚点完整性。 | D16-C01 = GATE_PASS |
| **D16-C16** | 16.3 并行 | A05 / R05 资源包 | `docs/game/design/common/D16_A05_SLICE_RESOURCE_PACKET.md` | 登记角色/船/码头/沙滩最终资源的来源、授权、LOD、碰撞体与锚点。 | D16-C01 = GATE_PASS |
| **D16-C17** | 16.3 并行 | A05 接入清单 | `docs/game/design/common/D16_A05_SLICE_INTEGRATION_PLAN.md` | 编写切片最终资源接入清单和消费者列表，不执行状态伪升级。 | D16-C01 = GATE_PASS |
| **D16-C18** | 16.4 串行 | A05 切片关闭 | `tools/results/external/d16_a05_final_assets.json` | 关闭垂直切片所需最终资源，生成 A05 外部 envelope；未替换项必须如实标为 placeholder。 | D16-C15、C16、C17 全部 GATE_PASS |
| **D16-C19** | 16.5 并行 | A01 / R03 目标机性能 | `tools/results/external/d16_a01_target_device.json` | 目标机 + 低端机 ≥60 秒窗口实测，提交包含 FPS、内存、GC、加载的原始日志与 envelope。 | D16-C01 = GATE_PASS |
| **D16-C20** | 16.5 并行 | A02 / R06 断电跨平台 | `tools/results/external/d16_a02_power_cross_platform.json` | 真实硬件断电等价 + 至少两平台恢复 + 迁移失败保留原档的真实 envelope。 | D16-C01 = GATE_PASS |
| **D16-C21** | 16.5 并行 | A03 / R04 真机无障碍 | `tools/results/external/d16_a03_device_accessibility.json` | 真机手柄/键盘、目标分辨率、字体、字幕、色觉/低刺激人工实测记录与 envelope。 | D16-C01 = GATE_PASS |
| **D16-C22** | 16.5 并行 | A06 目标机发布 | `tools/results/external/d16_a06_target_release_privacy.json` | 目标机安装/更新/卸载、原生崩溃行为、最终隐私人工复核记录与 envelope。 | D16-C01 = GATE_PASS |
| **D16-C23** | 16.6 串行 | 六门外部门禁审计 | `tools/results/d16_external_gate_audit_current.json` | 执行 `tools/d16_external_gate_audit.py`，六份 envelope 全部 PASS 方可判定 GATE_PASS。 | D16-C14、C18、C19、C20、C21、C22 全部 GATE_PASS |
| **D16-C24** | 16.7 串行 | S01 就绪检查 | `tools/results/d16_s01_readiness_current.json` | 运行 `tools/d16_s01_readiness.py` 进行只读启动资格检查；存在阻断时禁止开启长线。 | D16-C23 = GATE_PASS |
| **D16-C25** | 16.7 串行 | S01 长线执行 | `docs/game/design/common/D16_FULL_VERSION_ACCEPTANCE_REPORT.md` | 执行新游戏→游玩→保存→恢复→退出→更新→再恢复全流程长线实测。 | D16-C24 = GATE_PASS |
| **D16-C26** | 16.7 串行 | S01 故事复核 | `docs/game/design/common/handoffs/D16/D16-C26_GATE_HANDOFF.md` | 复核 S01 全流程无软锁、无重复结算、无数据破坏。 | D16-C25 = GATE_PASS |
| **D16-C27** | 16.8 串行 | S02 发布决策 | `docs/game/design/common/D16_FINAL_HANDOFF_INDEX.md` | 编写可发布/不可发布边界、剩余风险、回滚路径；Steam 保持未授权未执行。 | D16-C26 = GATE_PASS |
| **D16-C28** | 16.8 串行 | 全章正式验收 | `docs/game/design/common/D16_GATE_ACCEPTANCE.md` | 章节唯一正式验收；做出放行 D17-C01 的最终仲裁。 | D16-C27 = GATE_PASS |
| **D16-C29** | 16.X 旁路 | A06 / R02 安装器回归 | `game/tools/results/D16/D16-C29/` | 复跑本机便携安装器隔离烟测，不作为目标机通过，不阻塞 S01。 | D16-C01 = GATE_PASS |
| **D16-C30** | 16.X 旁路 | S01 夹具准备 | `docs/game/design/common/D16_S01_STORY_FIXTURE.md` | 准备长线故事测试夹具与检查表，不启动 S01 实际流程。 | D16-C01 = GATE_PASS |
| **D16-C31** | 16.X 旁路 | A06 Steam 保持冻结 | `docs/game/design/common/D16_STEAM_HOLD.md` | 明确核对 Steam 上传处于未授权状态，严禁上传。 | D16-C01 = GATE_PASS |

---

## 4. 六份外部 Envelope 规范路径与必填字段

所有外部 envelope JSON 统一放置于 `tools/results/external/` 目录下。

### 4.1 通用必填字段（所有 6 个 envelope 必须具备）

- `status`: 必须为 `"PASS"`
- `evidence_kind`: 证据类别描述
- `operator`: 实际操作者 / 责任审核人 ID
- `device_id`: 目标硬件设备标识
- `captured_at`: 证据采集时间（ISO 8601）
- `raw_artifacts`: 非空数组，列出实际存在且不可变的原始材料路径（相对路径必须位于 `tools/results/external/` 内且不得包含 `..`，绝对路径必须存在）
- **禁止字段**：不得标记 `synthetic: true`、`editor_only: true` 或 `simulated: true`

### 4.2 专属必填字段与门禁校验规则

| 门禁 ID | 规范文件路径 | 专属必填字段 | 校验规则 |
|---|---|---|---|
| **D16-A01** | `tools/results/external/d16_a01_target_device.json` | `duration_seconds`<br>`fps_min`<br>`fps_avg`<br>`memory_peak_mb`<br>`gc_alloc_bytes`<br>`same_screen_count`<br>`streaming_load_restore_pass`<br>`save_restore_pass` | `duration_seconds >= 60`<br>`fps_min > 0` 且 `fps_avg > 0`<br>`streaming_load_restore_pass == true` 或 `"PASS"`<br>`save_restore_pass == true` 或 `"PASS"` |
| **D16-A02** | `tools/results/external/d16_a02_power_cross_platform.json` | `power_loss_pass`<br>`cross_platform_pass`<br>`migration_failure_preserves_original`<br>`backup_restore_pass`<br>`platforms` | `power_loss_pass == PASS`<br>`cross_platform_pass == PASS`<br>`migration_failure_preserves_original == PASS`<br>`backup_restore_pass == PASS`<br>`platforms` 为数组且长度 ≥ 2 |
| **D16-A03** | `tools/results/external/d16_a03_device_accessibility.json` | `physical_input_pass`<br>`focus_pass`<br>`accessibility_manual_pass`<br>`resolutions`<br>`input_devices` | `physical_input_pass == PASS`<br>`focus_pass == PASS`<br>`accessibility_manual_pass == PASS`<br>`resolutions` 为非空数组<br>`input_devices` 为非空数组 |
| **D16-A04** | `tools/results/external/d16_a04_manual_review.json` | `reviewer_id`<br>`decision_log_ref`<br>`reviewed_count`<br>`pending_count` | `pending_count == 0`<br>`reviewed_count > 0` |
| **D16-A05** | `tools/results/external/d16_a05_final_assets.json` | `reviewer_id`<br>`asset_manifest_ref`<br>`final_asset_count`<br>`placeholder_count`<br>`scene_regression_pass` | `placeholder_count == 0`<br>`final_asset_count > 0`<br>`scene_regression_pass == PASS` |
| **D16-A06** | `tools/results/external/d16_a06_target_release_privacy.json` | `install_update_uninstall_pass`<br>`native_crash_review_pass`<br>`privacy_manual_review_pass`<br>`target_build_ref` | `install_update_uninstall_pass == PASS`<br>`native_crash_review_pass == PASS`<br>`privacy_manual_review_pass == PASS` |

---

## 5. 禁止伪通过项（严厉红线）

后续所有 Agent 严禁将以下任何情况作为门禁通过依据：

1. **本机 FPS 替代目标设备实测**：严禁以开发机（如 RTX 4070 Ti）或无图形设备（Null Device）的批处理/PlayMode FPS 冒充目标设备/低端设备表现。
2. **编辑器 PlayMode / 合成输入替代真机验收**：严禁将 Unity PlayMode Test Runner、自动化代码注入或合成 InputSystem 事件当作真实物理手柄、目标分辨率及人工无障碍验收。
3. **候选资源冒充最终资源**：严禁将 `game/Assets/Art/Candidates/` 中的候选包（如 Kenney 候选资源）或未挂载 Prefab 的素材冒充已验收的最终授权资源。
4. **通过修改 placeholder 状态伪造接入**：严禁将 ContentRegistry 中条目的 `asset_state` 从 `placeholder` 直接改为 `integrated` 或 `verified` 来人为清零 A05 缺口。
5. **脚本批量批准 A04 Pending**：严禁编写自动化脚本一键批准 A04 的 267 条待审记录；每条必须由具备权责的人员逐条审核并留下依据。
6. **未授权的 Steam 操作**：在用户明确授权前，严禁调用任何 Steamworks 上传接口、发布脚本或外部网络发布行为；`D16-C31` 的通过准则是“确认未授权且未执行”。
7. **跨轨伪通过**：严禁将 D15-S02 内容通过、D18 53/53 逻辑通过或 D19 港口逻辑推进写为 D16 门禁已通过。

---

## 6. 各波次准入与开始条件

```text
波次 16.0 (门禁冻结)
  └─ D16-C01 (当前任务) -> 产出本基线文档，达成 GATE_PASS

波次 16.1 (A04 十二批人工审查)
  └─ 准入条件：D16-C01 = GATE_PASS
  └─ 可同步并行：D16-C02 ～ D16-C13 (各批次必须独立审查，逐条单项写回，无重叠写入)

波次 16.2 (A04 闭环审计)
  └─ 准入条件：D16-C02 ～ D16-C13 全部 12 项均为 GATE_PASS 且本批 pending_count 清零
  └─ 串行：D16-C14 产出 d16_a04_manual_review.json

波次 16.3 (A05 资源准备与接入清单)
  └─ 准入条件：D16-C01 = GATE_PASS
  └─ 可同步并行：D16-C15 (只读预检)、D16-C16 (切片资源包)、D16-C17 (接入清单)

波次 16.4 (A05 垂直切片资源关闭)
  └─ 准入条件：D16-C15、D16-C16、D16-C17 全部均为 GATE_PASS
  └─ 串行：D16-C18 产出 d16_a05_final_assets.json

波次 16.5 (目标设备 / 断电 / 无障碍 / 发布隐私 实测)
  └─ 准入条件：D16-C01 = GATE_PASS
  └─ 可同步并行：D16-C19 (A01)、D16-C20 (A02)、D16-C21 (A03)、D16-C22 (A06)

波次 16.6 (六门外部门禁审计)
  └─ 准入条件：D16-C14、D16-C18、D16-C19、D16-C20、D16-C21、D16-C22 全部均为 GATE_PASS
  └─ 串行：D16-C23 运行 tools/d16_external_gate_audit.py，六项 envelope 必须全部 PASS

波次 16.7 (S01 长线全量验收)
  └─ 准入条件：D16-C23 = GATE_PASS
  └─ 严格串行：D16-C24 (S01就绪检查) → D16-C25 (S01长线实测) → D16-C26 (S01故事复核)

波次 16.8 (S02 最终交接与章节验收)
  └─ 准入条件：D16-C26 = GATE_PASS
  └─ 严格串行：D16-C27 (S02交接决策) → D16-C28 (全章正式验收)
  └─ 只有 D16-C28 = GATE_PASS 后，才允许启动 D17-C01

波次 16.X (旁路任务，不阻塞 S01)
  └─ 准入条件：D16-C01 = GATE_PASS
  └─ 可并行执行：D16-C29 (安装器回归)、D16-C30 (S01夹具准备)、D16-C31 (Steam保持未授权)
```

---

## 7. 总结与下阶段指引

本基线文档确立后，D16 剩余发布门禁进入有序可控的执行状态。后续执行 Agent 必须对照本基线执行自身任务卡，严格遵守单一写者、只在允许范围内写入、诚实留存原始证据，绝不抢跑或伪造门禁。
