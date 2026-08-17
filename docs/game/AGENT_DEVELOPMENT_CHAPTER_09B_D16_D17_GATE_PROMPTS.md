# D16 / D17：合并后可复制门禁提示词（16 项）

> 复制下面整段给 Agent。`D16-C01`—`C05` 已下发，提示词保持原样。从 `D16-C06` 和全部 `D17-Cxx` 起按合并粒度分派。
>
> 旧 `D16-C13`—`C31`、旧 `D17-C05`—`C16` 作废，禁止再派。
>
> 协议：[GATE_TASK_PROTOCOL.md](GATE_TASK_PROTOCOL.md)。任务卡：[GATES](AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATES.md)。单 Agent 从 C06 串行到 D17-C04：[SERIAL_RUNNER](AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_SERIAL_RUNNER.md)。

## 执行顺序

```text
已下发：C01（过）→ C02 / C03 / C04 / C05（C05 进行中，勿改）
C06  剩余资产审查 005—008          可与 C05 并行，勿碰 004
C07  历史审查 009—012 + A04 关闭     等 C02—C06 全过
C08  A05 切片资源一次做完
C09  四门外证一次收齐
C10  六门审计 + S01 长线
C11  S02 + D16 放行
C12  旁路：安装器 / 夹具 / Steam 冻结

D17-C01  等 D16-C11：冻结 + 资源登记
D17-C02  运行时切片一条做完
D17-C03  真机 + 性能 + 资源回归
D17-C04  正式 10 次 + 章节验收
```

## 共同硬约束

1. 写结果 JSON、handoff、当天记忆；跑 `python tools/sync_gate_task_status.py` 与 `git diff --check`。
2. 状态只用 `GATE_PASS` / `GATE_PARTIAL` / `GATE_BLOCKED`。
3. 本机 PlayMode、D18/D19 `LOGIC_PASS`、Kenney 候选、低模人物都不是正式通过。
4. 禁止 Steam 上传；禁止脚本批批 A04；禁止把 placeholder 改状态冒充最终资源。
5. 前置不是 `GATE_PASS` 时只写 `GATE_BLOCKED`。

---

## 已下发保留（不要改、不要重派 C01）

### D16-C01 — 剩余门禁基线（已 GATE_PASS）

只读历史，不要重跑。

### D16-C02 — A04 批次 001

```text
直接完成 D16-C02。前置：D16-C01=GATE_PASS。阅读 GATE 协议、D16-C01 基线、D16_A04_REVIEW_PACKET_CURRENT.md 批次 D16-A04-001（25 条，asset_blockade_order 到 asset_d15_a06_model_function_cargo_davit）。

唯一写入范围：docs/game/design/common/reviews/D16_A04_BATCH_001.md、该批每条新增证据文件、经 apply_review_decision 单条写回的对应 Registry 审查字段、game/tools/results/D16/D16-C02/、docs/game/design/common/handoffs/D16/D16-C02_GATE_HANDOFF.md、当天记忆和 GATE_TASK_STATUS.md。不得处理其它批次，不得批批，不得改 asset_state。

逐条审查本批 25 条。敏感条只能 ApprovedWithWarning / Rejected / 继续 Pending。写回只允许 python tools/d16_a04_apply_review_decision.py 单条，先 dry-run。本批 Pending 未清零不得写 GATE_PASS。完成后同步状态并 diff 检查。
```

### D16-C03 — A04 批次 002

```text
直接完成 D16-C03。前置：D16-C01=GATE_PASS。批次 D16-A04-002（25 条，asset_d15_a06_model_function_cargo_hatch 到 asset_d15_a06_model_function_medical_berth）。

唯一写入范围：docs/game/design/common/reviews/D16_A04_BATCH_002.md、本批证据、本批单条写回、game/tools/results/D16/D16-C03/、handoffs/D16/D16-C03_GATE_HANDOFF.md、当天记忆和状态总览。规则同 C02。不得碰其它批次。
```

### D16-C04 — A04 批次 003

```text
直接完成 D16-C04。前置：D16-C01=GATE_PASS。批次 D16-A04-003（25 条，asset_d15_a06_model_function_medicine_cabinet 到 asset_d15_a06_model_island_packet_sloop）。

唯一写入范围：docs/game/design/common/reviews/D16_A04_BATCH_003.md、本批证据与单条写回、game/tools/results/D16/D16-C04/、handoffs/D16/D16-C04_GATE_HANDOFF.md、当天记忆和状态总览。规则同 C02。
```

### D16-C05 — A04 批次 004（进行中，提示词锁定）

```text
直接完成 D16-C05。前置：D16-C01=GATE_PASS。批次 D16-A04-004（25 条，asset_d15_a06_model_longwake_barque 到 asset_d15_a06_model_ship_module_crew_rest_area）。

唯一写入范围：docs/game/design/common/reviews/D16_A04_BATCH_004.md、本批证据与单条写回、game/tools/results/D16/D16-C05/、handoffs/D16/D16-C05_GATE_HANDOFF.md、当天记忆和状态总览。规则同 C02。不要改做 005 及以后批次。
```

---

## 合并后的 D16

### D16-C06 — A04 剩余资产审查（批次 005—008）

```text
你是 WhalesIslandReef 的发布门禁 Agent。直接完成 D16-C06。这是一次做完 A04 剩余资产授权批次，不要拆回 4 个任务。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前阅读：AGENTS.md、docs/CONTEXT.md、docs/game/GATE_TASK_PROTOCOL.md、docs/game/AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATES.md、docs/game/design/common/D16_A04_REVIEW_PACKET_CURRENT.md。

前置：D16-C01=GATE_PASS。C05 可以仍在进行；你只处理批次 005—008，绝对不要打开或写回批次 004 的条目。

唯一写入范围：
- docs/game/design/common/reviews/D16_A04_BATCH_005.md
- docs/game/design/common/reviews/D16_A04_BATCH_006.md
- docs/game/design/common/reviews/D16_A04_BATCH_007.md
- docs/game/design/common/reviews/D16_A04_BATCH_008.md
- 这四批每条新增本地证据文件
- 仅这四批经 tools/d16_a04_apply_review_decision.py 的单条写回
- game/tools/results/D16/D16-C06/
- docs/game/design/common/handoffs/D16/D16-C06_GATE_HANDOFF.md
- 当天记忆和 GATE_TASK_STATUS.md

范围：
- 005：25 条，crew_storage_lockers → light_canvas
- 006：25 条，light_cargo_dividers → spare_bucket_line
- 007：25 条，spare_mast_kit → vegetation_profile_crop
- 008：11 条，vegetation_profile_epiphyte → wetland
合计 86 条。

规则：每条必须有责任人、日期、依据、新增本地证据。敏感条禁止直接 Approved。写回先 dry-run 再单条提交。不得改 historical_tag 原文、license、authorization、asset_state 或玩法数值。不得脚本批批。四批 Pending 都必须为 0 才能 GATE_PASS；否则 GATE_PARTIAL 或 GATE_BLOCKED。

完成后按协议留痕，运行 python tools/sync_gate_task_status.py 与 git diff --check。
```

### D16-C07 — A04 历史内容 + 关闭审计

```text
直接完成 D16-C07。一次做完批次 009—012 并输出 A04 关闭 envelope。不要再拆。

精确前置：D16-C02、C03、C04、C05、C06 全部 GATE_PASS。任一未过只写 GATE_BLOCKED，不得代做未完成批次。

唯一写入范围：reviews/D16_A04_BATCH_009.md—012.md、这四批证据与单条写回、tools/results/external/d16_a04_manual_review.json、game/tools/results/D16/D16-C07/、handoffs/D16/D16-C07_GATE_HANDOFF.md、当天记忆和状态总览。

批次：
- 009：25 条 historical_content，animal_domestic_cattle → item_fruit_guava
- 010：25 条，item_gear_cast_net → item_seed_sugarcane
- 011：25 条，item_seed_tobacco → outfit_injury_old_leg
- 012：6 条，outfit_shoes_deck_boots → reward_d14_worldclass_reinforced_keel

历史/敏感条目禁止直接 Approved，只能 ApprovedWithWarning / Rejected / Pending。四批清零后生成符合 D16_EXTERNAL_ACCEPTANCE_RUNBOOK.md 的 d16_a04_manual_review.json，pending_count 必须为 0。机器结构 0 错误不能代替人工决定。完整留痕。
```

### D16-C08 — A05 切片资源一次收口（方案 B 修订）

```text
直接重做 D16-C08。用户已选方案 B：自己检索并下载可商用免费资源，替换垂直切片 4 项，先保证能运行。不要沿用上一轮只登记 Kenney 候选后立刻 BLOCKED 的做法。上一轮 GATE_BLOCKED 是历史记录，本轮用新结果覆盖，不要改 C01—C07。

精确前置：D16-C07=GATE_PASS。

本轮目标不是 2264 个 placeholder 全换完，只换这 4 个：
- asset_ph_crew_male（可识别的 1675 向男性水手/船员，禁止再当柱体）
- building_saint_marys_dock（可走、可碰的主码头）
- building_small_boat_pier（可上下船的小艇泊位）
- island_saint_marys（可落地的沙滩/地面）
必要时可额外登记 1 个小船展示资源，但不改船控数值所有者。

唯一写入范围：
- artifacts/external_sources/d16_c08_slice/（原始下载包、License、来源页摘录、SHA-256）
- game/Assets/Art/SliceFinal/D16C08/（解压后的最终网格/贴图，路径不得含 placeholder 或 ph_）
- game/Assets/Prefabs/SliceFinal/D16C08/（4 个运行时 Prefab：碰撞、锚点、LOD）
- tools/results/d16_a05_replacement_manifest.json
- tools/results/d16_a05_replacement_preflight_current.json
- docs/game/design/common/D16_A05_SLICE_RESOURCE_PACKET.md
- docs/game/design/common/D16_A05_SLICE_INTEGRATION_PLAN.md
- docs/game/design/common/reviews/D16_C08_SLICE_REVIEW.md
- tools/results/external/d16_a05_final_assets.json
- 仅这 4 个 Content Registry 条目中与替换包对应的授权/审查/路径字段
- game/tools/results/D16/D16-C08/
- docs/game/design/common/handoffs/D16/D16-C08_GATE_HANDOFF.md
- 当天记忆和 GATE_TASK_STATUS.md

检索与许可红线：
1. 只使用明确可商业使用且允许修改的免费授权：CC0、CC-BY、CC-BY-SA（保留署名）、或官方书面 Public Domain。禁止 CC-NC、CC-ND、All Rights Reserved、未写许可、盗转、付费库破解。
2. 优先 1675 加勒比写实：17 世纪水手/码头木结构/沙滩。不要奇幻、科幻、现代武器、卡通方块人作为最终切片外观。Kenney Blocky Characters 不能当最终角色。Kenney Pirate Kit 仅当找不到更贴时代的码头/沙滩时才可降级使用，且必须在 handoff 写明“品质降级原因”。
3. 推荐检索来源（打开官方页核许可，不要猜）：Poly Haven、Quaternius、Kenney.nl 非方块包、OpenGameArt 标明 CC0/CC-BY 的条目、NASA/Smithsonian 公共域（若适用）。必须保存来源 URL、License 原文、下载日期。
4. 原始 zip/glb/fbx 先落到 artifacts/external_sources/d16_c08_slice/，再复制进 game/Assets/Art/SliceFinal/D16C08/。禁止只写远程链接。

接入顺序：
1. 下载并校验 4 项本地文件存在。
2. 为每项做运行时 Prefab：关闭子物体乱碰撞，根上可用碰撞；至少 1 个稳定锚点（角色脚底/码头入口/泊位上下船/沙滩地面）。
3. 写 reviews/D16_C08_SLICE_REVIEW.md：每项来源、许可、1675 适配说明、Approved 或 ApprovedWithWarning。
4. 写 schema_version=1、task=D16-A05-REPLACEMENT 的 manifest，字段必须能过 tools/d16_a05_replacement_preflight.py（final_resource_path、runtime_prefab_path、source、license、authorization、review、lod_profile、collision_ref、anchor_refs、test_cases、raw_artifacts）。
5. 运行：python tools/d16_a05_replacement_preflight.py . tools/results/d16_a05_replacement_manifest.json --json-out tools/results/d16_a05_replacement_preflight_current.json
   必须 can_apply=true。否则修包重跑，不要写 GATE_PASS。
6. 预检通过后，只把这 4 条从 placeholder 接到最终路径。其余条目 asset_state 一律不动。
7. 用仓库已有、不启动 Player 的方式验证 Prefab/路径可解析（Unity 批处理导入或现有静态检查）。能运行=角色不再是柱体、码头/泊位/沙滩有网格和碰撞。不要开正式 PlayMode 验收故事。
8. 写 d16_a05_final_assets.json：status=PASS，placeholder_count 仍应反映全库剩余数，final_replaced_count=4，raw_artifacts 指向真实本地文件。

禁止：清零 2264 placeholder；把 Candidates/Kenney 原路径直接标 final；伪造许可；改 Systems 数值或 D2/D3/D4/D9 裁决权。找不到 4 项合规资源时保持 GATE_BLOCKED，列出已找到/未找到项。完整留痕。
```

### D16-C09 — 四门外部真实证据

> 开发阶段（2026-08-18）：真机四门**暂缓采集**，已有结果保持 `GATE_BLOCKED`。见 [D16_EXTERNAL_HARDWARE_DEFERRED.md](design/common/D16_EXTERNAL_HARDWARE_DEFERRED.md)。不要为过审计写假 envelope。C10/C11 同理。


```text
直接完成 D16-C09。一次收集 A01/A02/A03/A06 四份外部 envelope。同一 agent 做，但四份证据彼此独立，缺哪门写哪门 BLOCKED，不要用本机对照顶替。

精确前置：D16-C08=GATE_PASS（切片最终资源未接入则性能包不能正式通过）。

唯一写入范围：
- tools/results/external/d16_a01_target_device.json 及其 raw_artifacts
- tools/results/external/d16_a02_power_cross_platform.json 及其 raw_artifacts
- tools/results/external/d16_a03_device_accessibility.json 及其 raw_artifacts
- tools/results/external/d16_a06_target_release_privacy.json 及其 raw_artifacts
- game/tools/results/D16/D16-C09/
- handoffs/D16/D16-C09_GATE_HANDOFF.md
- 当天记忆和状态总览

必须覆盖：
1. 目标机 + 低端机各 ≥60 秒：FPS/内存/GC/同屏/流式/保存恢复，带原始日志。本机 4070 Ti 编辑器窗口只当对照。
2. 真断电等价 + 至少两平台恢复 + 迁移失败保留原档。Windows 强制终止不是本项 PASS。
3. 真实键盘/已唤醒手柄、目标分辨率、重绑定、字体、字幕、色觉/低刺激人工记录。XInput 1167 不是手柄通过。
4. 目标机安装/更新/卸载/存档保留、原生崩溃、最终隐私人工复核。禁止 Steam 上传。

禁止 synthetic / editor_only / simulated。四门都有真实材料才能 GATE_PASS。完整留痕。
```

### D16-C10 — 外部审计 + S01 长线

```text
直接完成 D16-C10。一次做完六门审计、S01 就绪检查、长线故事、故事复核。中途任何一环失败就停，写 GATE_BLOCKED，不要假装后半段也过了。

精确前置：D16-C07、C08、C09 全部 GATE_PASS。

唯一写入范围：
- tools/results/d16_external_gate_audit_current.json
- tools/results/d16_s01_readiness_current.json
- docs/game/design/common/D16_FULL_VERSION_ACCEPTANCE_REPORT.md 的本轮增量节
- 长线原始日志
- game/tools/results/D16/D16-C10/
- handoffs/D16/D16-C10_GATE_HANDOFF.md
- 当天记忆和状态总览

顺序：
1. python tools/d16_external_gate_audit.py game --evidence-root tools/results/external --json-out tools/results/d16_external_gate_audit_current.json
   六门不全 PASS 则停止。
2. 跑 tools/d16_s01_readiness.py（只读）。未就绪则停止。
3. 执行新游戏→游玩→保存→恢复→退出→更新→再恢复。软锁、重复结算、数据破坏即失败。
4. 复核上述证据，不删失败日志，不把旧 PlayMode 当成本轮故事。

Steam 未授权不是本任务失败。完整留痕。
```

### D16-C11 — S02 交接与 D16 放行

```text
直接完成 D16-C11。一次写最终交接并做 D16 章节验收。这是 D17-C01 的硬前置。

精确前置：D16-C10=GATE_PASS。

唯一写入范围：docs/game/design/common/D16_FINAL_HANDOFF_INDEX.md 本轮增量节、docs/game/design/common/D16_GATE_ACCEPTANCE.md、game/tools/results/D16/D16-C11/、handoffs/D16/D16-C11_GATE_HANDOFF.md、当天记忆和状态总览。禁止改代码、禁止 Steam 上传。

写清可发布/不可发布、剩余风险、回滚路径。只读审计 C01—C10 与 C12（若已做）。当前有效编号是 C01—C12，不要再要求旧 C13—C31。通过后才允许 D17-C01。完整留痕。
```

### D16-C12 — 旁路：安装器、夹具、Steam 冻结

```text
直接完成 D16-C12。前置：D16-C01=GATE_PASS。可与 C06 之后并行，不是 C10 前置。禁止上传 Steam，禁止启动 C10 长线。

唯一写入范围：game/tools/results/D16/D16-C12/、docs/game/design/common/D16_S01_STORY_FIXTURE.md、docs/game/design/common/D16_STEAM_HOLD.md、handoffs/D16/D16-C12_GATE_HANDOFF.md、当天记忆和状态总览。可追加本机安装器烟测日志副本。

一次做三件旁路：复跑本机便携安装器正/负向烟测；写 S01 故事检查表但不执行；核对 Steam 仍未授权未执行。本机安装器绿不等于目标机 C09 通过。完整留痕。
```

---

## 合并后的 D17

### D17-C01 — 切片冻结 + 最终资源登记

```text
直接完成 D17-C01。精确前置：D16-C11=GATE_PASS。本机十循环 1/1 或 PlayMode 60/60 不是前置。

唯一写入范围：docs/game/design/common/D17_SLICE_FREEZE.md、D17_SLICE_CHARACTER_ASSETS.md、D17_SLICE_ENV_ASSETS.md、相关授权/模型证据、game/tools/results/D17/D17-C01/、handoffs/D17/D17-C01_GATE_HANDOFF.md、当天记忆和状态总览。不写运行时，不改 Systems。

一次完成：冻结一座港/一名玩家/一艘船/一次上下船/一次保存恢复，以及输入、镜头、碰撞、截图点、设备矩阵；登记最终角色网格/碰撞/骨骼/授权，以及船/码头/沙滩/海水最终文件。低模人物只能当对照。缺最终文件 GATE_BLOCKED。完整留痕。
```

### D17-C02 — 运行时垂直切片

```text
直接完成 D17-C02。一次接入物理、上下船、镜头焦点、正式动作、失败恢复。不要拆成五个任务。共享 D2/D6 写者，必须串行在本任务内完成。

精确前置：D17-C01=GATE_PASS。C01 资源仍 BLOCKED 时，本任务不得把程序姿态/低模标最终。

唯一写入范围：开工时在 handoff 冻结的 D2/D6 物理、上下船、镜头、动作、失败恢复文件（先列清单再改，不得顺手改存档裁决或 Systems 网络/LLM）、对应测试、game/tools/results/D17/D17-C02/、handoffs/D17/D17-C02_GATE_HANDOFF.md、当天记忆和状态总览。

验收：重力/地面/斜坡/走跑跳落地；上船下船过渡与无安全地面拒绝，禁止瞬移；键鼠/手柄镜头、暂停锁输入、菜单不叠层；待机走跑跳上下船有正式状态，脚不漂不倒；加载失败/失焦/保存失败/回主菜单再进港口可恢复。禁止柱体伪装。本机旧 smoke 只当对照。完整留痕。
```

### D17-C03 — 真机、性能、资源回归

```text
直接完成 D17-C03。一次做完真机输入点、目标窗口采样、最终资源场景回归。

精确前置：D17-C02=GATE_PASS。

唯一写入范围：tools/results/external/d17_device_input.json、d17_target_perf.json 及其 raw_artifacts、docs/game/design/common/D17_SLICE_ASSET_REGRESSION.md、game/tools/results/D17/D17-C03/、handoffs/D17/D17-C03_GATE_HANDOFF.md、当天记忆和状态总览。

真实键鼠/已唤醒手柄走完冻结故事输入点；记录目标窗口采样，不删内容伪装性能；核验最终角色/船/码头/沙滩的碰撞、锚点、授权。编辑器合成输入不能当唯一证据。完整留痕。
```

### D17-C04 — 正式 10 次验收

```text
直接完成 D17-C04。一次冻结正式故事脚本、跑连续 10 次、复核重启、写状态审查和章节验收。禁止复用 2026-08-12 本机十循环 XML。

精确前置：D17-C03=GATE_PASS。

唯一写入范围：docs/game/design/common/D17_S02_STORY_SCRIPT.md、tools/results/external/d17_s02_ten_cycles.json 及其 raw_artifacts、D17_S02_RESTART_AUDIT.md、D17_GATE_STATUS_REVIEW.md、D17_GATE_ACCEPTANCE.md、game/tools/results/D17/D17-C04/、handoffs/D17/D17-C04_GATE_HANDOFF.md、当天记忆和状态总览。不新增功能。

故事：出生 → 沙滩/码头 → 真实碰撞 → E 上下船 → 离岸操船 → 调视角 → Esc 暂停/继续 → 保存读取 → 下船 → 重启恢复，连续 10 次。阻塞错误、叠层、漂浮/倒置、不可退出即失败。不要把逻辑通过或本机 60/60 写成切片完成。完整留痕。
```

---

## 作废编号（禁止分派）

`D16-C13`—`D16-C31`、`D17-C05`—`D17-C16` 已合并进上面各项。看到这些号不要执行，不要写结果 JSON。
