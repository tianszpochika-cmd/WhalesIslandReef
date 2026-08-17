# D18 纯逻辑资源治理基线（D18-L01）

> 冻结日期：2026-08-16
> 任务：`D18-L01`（波次 0，串行冻结任务）
> 本文件是 D18 全章唯一权威基线。**后续 D18-L02—L50 与 D19-L01 只读盘点必须从本文件出发；不阅读任何旧 R01—R03/RERUN 文档也能独立工作。**
> 生效依据：[D18—D24 纯逻辑完成路线](../../D18_D24_VISUAL_PLAYABLE_ROADMAP.md)、[单任务逻辑开发协议](../../LOGIC_TASK_PROTOCOL.md)、[D18 章节任务卡](../../AGENT_DEVELOPMENT_CHAPTER_10_D18.md)。

## 0. 读取顺序与本文用途

1. 先读 [D18—D24 纯逻辑完成路线](../../D18_D24_VISUAL_PLAYABLE_ROADMAP.md)（状态语义与延期边界）→ [单任务逻辑开发协议](../../LOGIC_TASK_PROTOCOL.md)（结果格式与禁止项）→ [D18 章节任务卡](../../AGENT_DEVELOPMENT_CHAPTER_10_D18.md)（任务卡与波次）→ 本文。
2. 本文冻结五项内容：
   - 术语与资源状态机边界（Candidate / IntakeEligible / RuntimeRequested / RuntimeApproved / ReleaseApproved / Rejected / Revoked）；
   - 本逻辑阶段允许与禁止的验证；
   - 旧 D18-A/S/R、D19-R03/RERUN 的 SUPERSEDED 隔离清单；
   - L02—L50 每一波的可开始条件、共享字段词典、结果 JSON 格式与冲突写入边界；
   - 现状盘点（磁盘事实快照，不补猜哈希、许可或数量）。
3. 本文不替代 L03 的状态机规范文档（`assets/D18_ASSET_STATE_MACHINE.md`，由 L03 独占写入、本基线发布时尚不存在）；本文只冻结基线语义，L03 输出完整状态图、表驱动迁移与测试接口。

## 1. 现状盘点（2026-08-16 磁盘事实，不补猜）

以下数字全部来自磁盘实际扫描或已有机器结果；本基线不新增任何哈希、许可或数量假设。

### 1.1 现有 manifest（只读数据源）

| 位置 | 数量 | 说明 |
|---|---:|---|
| `docs/game/design/common/assets/packs/**/*.manifest.json` | 28 | 包级 manifest：characters_animals 10、environment 9、ships_port_props 2、ui_materials_audio 7 |
| `docs/game/design/common/assets/manifests/` | 2 | 垂直切片 manifest（kenney_blocky_characters、kenney_pirate_kit） |
| `docs/game/design/common/assets/d18_asset_pack_manifest.example.json` | 1 | 示例，非真实 manifest |

- 包级 manifest 合计 30，与 [准入目录](assets/d18_prefab_candidate_intake_catalog.json) 的 `manifest_total=30` 一致。
- 辅助 JSON（非 manifest）：`packs/ships_port_props/D18_A05_FILE_HASHES.json`、`packs/ui_materials_audio/font_license_audit.json`、`packs/ui_materials_audio/polyhaven_selected_assets.json`、`packs/characters_animals/gap_fill/*/` 下的角色外观 packet 等。
- manifest 字段口径以 [d18_asset_pack_manifest.schema.json](../../../../tools/schemas/d18_asset_pack_manifest.schema.json) 为准（字段见第 6 节）。

### 1.2 候选/准入目录

- 候选文件根：`game/Assets/Art/Candidates/`，5 个顶层目录（非 `.meta` 文件数）：

| 顶层目录 | 非 .meta 文件数 |
|---|---:|
| `CharactersAnimals` | 128 |
| `Environment` | 338 |
| `Kenney` | 10 |
| `ShipsPortProps` | 305 |
| `UIMaterialsAudio` | 1,343 |
| 合计 | **2,124** |

- 准入目录 [d18_prefab_candidate_intake_catalog.json](assets/d18_prefab_candidate_intake_catalog.json)（D18-S02-R02 生成，保留的历史快照事实）：

```json
{"manifest_total": 30, "manifest_intake_eligible": 26, "candidate_file_total": 1895,
 "intake_eligible_file_total": 1865, "excluded_manifest_total": 4,
 "duplicate_manifest_file_references": 0, "errors": 0}
```

- `eligible_entries` 共 **1,865** 条，字段含 `intake_id / manifest_id / manifest_path / candidate_file_path / content_id / domain / priority / role / intake_eligible / license / hash_evidence / import_verified_evidence_path / allowed_task_scope / forbidden_scope / known_gaps / local_development_ready_evidence_required / responsibility`。
- **口径差异声明**：磁盘候选文件 2,124 与准入目录声明 1,895 的差（229）是什么内容（未登记文件/重复/口径），本基线不做对账假设；交给 L31（重复身份）、L33（目录声称审计）、L34（缺失证据）真实计算。
- 本地就绪目录 [d18_local_development_ready_catalog.json](assets/d18_local_development_ready_catalog.json)：`status=BLOCKED`，`local_development_ready_count=0`，`release_approved_count=0`；10 个 `Pending` 类别：environment 332、character 6、animal 13、fish 7、ship 13、building 104、prop 37、ui 1,224、material 91、audio 30。
- 占位替换映射 [d18_placeholder_replacement_map.json](assets/replacement/d18_placeholder_replacement_map.json)：D18-A07 机器结果 `PARTIAL`，2,264 输入/2,264 输出、1,107 mapped、1,157 unmapped。

### 1.3 现有 audit 工具（全部属 A/S/R 系列，SUPERSEDED）

| 工具 | 用途 |
|---|---|
| `tools/d18_asset_manifest_audit.py` | manifest 静态审计（含 Unity 对象计数核对） |
| `tools/d18_prefab_candidate_intake_audit.py` | 准入目录生成/审计 |
| `tools/d18_placeholder_replacement_map.py` | 占位替换映射 |
| `tools/d18_a04_r01_review_audit.py` | A04 人工复核包 |
| `tools/d18_r03_a01_resource_audit.py` | R03 外观资源只读审计 |
| `tools/d18_r03_s01_review.py`、`d18_r03_s01_review_rerun1.py` | R03 只读消费门禁 |
| `tools/d18_s02_r01_intake_refresh.py`、`d18_s02_r02_refresh.py` | 历史准入刷新（R01/R02） |

- Schema：`tools/schemas/d18_asset_pack_manifest.schema.json`。
- 测试：`tools/tests/test_d18_asset_manifest_audit.py`、`test_d18_placeholder_replacement_map.py`、`test_d18_prefab_candidate_intake.py`、`test_d18_r03_s01_review_rerun1.py`、`test_d18_s02_r01_intake_refresh.py`、`test_d18_s02_r02_refresh.py`。
- 已知事实（2026-08-16 全量回归记录）：`test_d18_s02_r01_intake_refresh.py` 仍断言历史 R01 目录 task/数量，与当前磁盘事实不一致而失败（4 项）；该测试属 SUPERSEDED 系列，**不是 L 任务门禁**，L 系列测试从 L40/L41 起独立建立。

### 1.4 真实结果目录（game/tools/results/D18/）

D18-A01—A07（含 A04-R01/R02-B/R02-C/R02-F/R02-F-R01/R03-P、A05-R01）、D18-S01、D18-S02、D18-S02-R01、D18-S02-R02、D18-R03-A01、D18-R03-S01：全部为历史结果（SUPERSEDED，见第 4 节）。其中 **manifest 审计 JSON、Unity 导入审计 JSON（A02—A06）、A05 逐文件哈希、font 审计** 等作为只读数据源可被 L 任务引用；其“PASS/验收”结论一律不作 L 任务通过条件。

### 1.5 状态归属：仅 Candidate / 有静态证据 / 未证实

| 类别 | 判定口径 | 实例 |
|---|---|---|
| 只是 Candidate | 有来源入口，但本地证据链不完整（缺许可/哈希/文件/映射任一），或未通过任何 L 系列验证 | 历史 source catalog 中“未下载”的包行（2026-08-14 时点声明）；R03 外观包中第 6 个独立发型等未证实条目；磁盘存在但未被 manifest/准入目录覆盖的候选文件（差异 229 的归属） |
| 有静态证据 | manifest/准入目录/机器结果记录了真实许可证、哈希、路径、权利、导入证据 | 26 个 `intake_eligible` manifest 的 `license`、`hash_evidence`、`import_verified_evidence_path`；`D18_A05_FILE_HASHES.json`（ships_port_props 逐文件 SHA-256）；字体 OFL 审计（`font_license_audit.json`） |
| 尚未证实 | 缺真实证据或证据口径有限 | 逐文件 SHA-256（准入目录 `hash_evidence.scope` 明示为“manifest archive or selected-file digest”，非逐文件 digest）；A04 人工审核 267 条 `Pending`；D16 外部证据六项缺失；D19-R03 路线/天气/性能门禁未闭合；`LocalDevelopmentReady=0`、`ReleaseApproved=0`；**L 系列 `RuntimeApproved` 无任何持有者** |

**本基线不宣称任何资产已 `RuntimeApproved` 或 `ReleaseApproved`；当前两者均为 0 个。**

## 2. 状态机冻结（术语定义、迁移、拒绝/撤销、唯一写者）

四个主状态 + 两个终态 + 一个申请态。**候选、准入、运行时消费、发布审批是四个不同状态，不能互相伪造**；状态迁移必须有可审计记录（谁、何时、依据哪个证据）。

### 2.1 状态定义

| 状态 | 精确定义 | 证据要求 | 本阶段（纯逻辑）可授予？ |
|---|---|---|---|
| `Candidate` | 有来源入口或磁盘文件，但证据链未完整或未通过准入校验；游戏逻辑**不得直接引用** Candidate 二进制 | 来源入口记录即可；无需完整许可/哈希 | 是（登记） |
| `IntakeEligible` | 证据完备、可被指定消费任务用于受控工作场景/Prefab 的“准入”状态；**不是资源状态**，不表示导入成功、Prefab 可用、运行时可用或发布可用 | 许可证规范、来源可验证、哈希/文件存在、四项权利、内容映射、路径边界全部通过（L04—L10/L15—L16 校验） | 是（由准入工具授予） |
| `RuntimeRequested` | 消费任务已建立受控工作场景/Prefab 引用并提交运行时晋级申请 | 申请数据（L37 Schema）+ 消费场景引用 + 用途 + 回退声明 | 是（申请数据可写；**不产生批准**） |
| `RuntimeApproved` | 运行时消费证据完备（真实 Prefab/场景消费者、保存/恢复、回退入口） | 真实运行时证据（未来 Runtime 阶段） | **否（本阶段禁止）** |
| `ReleaseApproved` | 可随游戏分发的发布审批 | 责任人类审核者确认（含 Steam/外部分发结论） | **否（仅人类；工具从不批准）** |
| `Rejected` | 拒绝终态：记录拒绝原因、证据路径、操作者，可审计 | 拒绝原因必填 | 是（人工或验证器） |
| `Revoked` | 撤销状态：原证据被推翻或授权撤回；保留原证据与撤销原因 | 撤销原因必填 | 是（人工或验证器） |

### 2.2 允许的迁移（冻结）

| 迁移 | 条件 |
|---|---|
| `Candidate → IntakeEligible` | 证据完备 + 全部相关验证器通过（唯一路径，不可跳过） |
| `IntakeEligible → RuntimeRequested` | 消费任务提交申请（L37）+ 受控消费引用 |
| `RuntimeRequested → RuntimeApproved` | 未来 Runtime 阶段门禁（D18-S03 等价物）真实运行时证据；**本逻辑阶段禁止** |
| `RuntimeApproved → ReleaseApproved` | 责任人类审核者确认；**本逻辑阶段禁止** |
| 任意非 ReleaseApproved 状态 → `Rejected` | 满足 2.3 拒绝条件，记录原因 |
| 任意状态 → `Revoked` | 满足 2.4 撤销条件，记录原因 |
| `Revoked → Candidate` | 仅人类复核后重新登记 |

### 2.3 禁止的迁移（冻结）

- `Candidate → RuntimeRequested / RuntimeApproved / ReleaseApproved`（无直达路径）；
- `IntakeEligible → RuntimeApproved / ReleaseApproved`；
- `RuntimeRequested → ReleaseApproved`；
- 任何状态 → `ReleaseApproved`（除 `RuntimeApproved` 且人类审核外）；
- 任何跳过中间状态的自选迁移；任何“越级后再补”的迁移。

### 2.4 拒绝条件（→ Rejected）

1. 缺许可证，或许可证据文件不存在/不可读；
2. 来源冲突：同一规范化路径多来源声称、来源页与包内许可矛盾；
3. 重复文件：哈希相同但 canonical_id 不同（先登记，不删除，L31 裁定）；
4. 不可分发：NC、个人使用限制、明确禁止再分发/随游戏分发；
5. 越级申请：无消费消费者或无 `RuntimeRequested` 却申请 `RuntimeApproved`；
6. 证据被推翻：哈希不匹配、文件缺失、来源页内容变更。

### 2.5 撤销条件（→ Revoked）

1. 哈希不再匹配真实文件；
2. 许可证撤回或来源 URL 失效；
3. 发现来源/作者声明冲突或伪造、补猜证据；
4. 审计发现该条目不再满足所在状态的证据要求。

撤销/拒绝记录一律 append-only：保留原证据路径、原状态、原因、操作者与日期；**工具不得静默覆盖或清除历史记录**。

### 2.6 唯一写者（谁可以授予/写入该状态）

| 状态 | 唯一写者 | 只读消费者 |
|---|---|---|
| `Candidate` | 来源目录任务（L21—L24）登记；L42 编译器维护；人类复核写 Rejected/Revoked | 全部验证器与审计 |
| `IntakeEligible` | L42 目录编译器（依据 L04—L10/L15—L16 验证器通过结果） | 消费任务、L17/L18 守卫 |
| `RuntimeRequested` | 消费任务（D19—D22 对应任务） | L35/L36/L37 |
| `RuntimeApproved` | 未来 Runtime 阶段门禁（D18-S03 等价物） | L18/L46 只读 |
| `ReleaseApproved` | 责任人类审核者 | 全员只读 |
| `Rejected` / `Revoked` | 人类复核；验证器可标记但必须留原因 | 审计 |

**冻结：本逻辑阶段（D18-L01—L50）任何工具不得产生 `RuntimeApproved` 或 `ReleaseApproved` 授予；允许的只有“申请数据（RuntimeRequested）”“证据缺口”和“拒绝/撤销记录”。**

### 2.7 与历史术语的映射

| 历史术语（A/S/R 系列） | L 系列对应 | 说明 |
|---|---|---|
| `Candidate` | `Candidate` | 同义保留 |
| `LicenseEvidenceComplete` / `ImportVerified` | 无直接对应 | 历史中间态；L 系列以 `IntakeEligible` 统一表示“证据完备可受控使用” |
| `intake_eligible=true`（catalog 非状态字段） | `IntakeEligible` | 字段升级为正式状态，语义不变 |
| `LocalDevelopmentReady` | 未来 `RuntimeApproved` | 本阶段 0 个；只有 Runtime 阶段真实证据后可授予 |
| `ReleaseApproved` | `ReleaseApproved` | 仅人类 |
| `D18-S03`（历史任务） | 未来 Runtime 阶段门禁 | 等价物，不继承历史计数 |

## 3. 本逻辑阶段验证边界（冻结）

### 3.1 允许的验证

- C# 编译、AgentCore `dotnet build`、服务端 `npm run build`/tsc；
- 契约/JSON Schema 校验（含 L 系列新增 Schema 与既有 manifest Schema）；
- 纯单元测试、确定性状态机模拟、存档往返测试、静态依赖/引用/缺失配置检查；
- 静态文件与目录审计（路径、哈希格式、存在性、数量对账）；
- Unity 测试仅限**不启动 Player 的编译或纯 EditMode 静态测试**；
- `git diff --check` 与链接存在性检查。

### 3.2 禁止的验证（本阶段一律不做）

- Unity Player 构建/启动、PlayMode、场景漫游、碰撞/寻路/路线实测；
- 截图、录屏、视频、固定镜头、视觉/UI 排版与美术质量判定；
- FPS、1% Low、加载时间、显存、质量档、LOD、性能采样；
- 外部资源下载/网络访问/Windows 防火墙操作；
- 外部二进制资源的运行时导入、Prefab/场景/Canvas/Build Settings 修改；
- 把静态编译/静态审计写成“可玩”“画面完成”“RuntimeApproved”“ReleaseApproved”或“Runtime PASS”。

## 4. SUPERSEDED 隔离清单（旧证据，禁止作 L 任务通过条件）

以下历史材料**只作历史证据**：可被 L 任务只读引用（数据源），**禁止重跑、禁止作为新 L 任务的通过条件或前置门禁**，也不得把其结论转换成 L 系列 `LOGIC_PASS`。

| 类别 | 范围 |
|---|---|
| D18 A/S/R 结果目录 | `game/tools/results/D18/D18-A01`…`D18-A07`（含 `D18-A04-R01/R02-B/R02-C/R02-F/R02-F-R01/R03-P`、`D18-A05-R01`）、`D18-S01`、`D18-S02`、`D18-S02-R01`、`D18-S02-R02`、`D18-R03-A01`、`D18-R03-S01` |
| D18 历史文档 | `docs/game/design/common/D18_D20_R03_STATUS.md`、`D18_D20_R03_RECOVERY_PLAN.md`、`D18_D20_R03_FINAL_DASHBOARD.md`、`D18_FREE_ASSET_SOURCE_CATALOG.md`、`D18_FREE_ASSET_LIBRARY_ACCEPTANCE.md`、`D18_FREE_ASSET_LIBRARY_ADMISSION_ACCEPTANCE.md`、`D18_FREE_ASSET_INTAKE_REFRESH_R01/R02.md`、`D18_*_ASSET_PACKET.md`（CHARACTER_ANIMAL/ENVIRONMENT/SHIP_BUILDING_PROP/UI_AUDIO_MATERIAL）、`D18_UNITY_ASSET_IMPORT_PIPELINE.md`、`D18_VISUAL_ART_DIRECTION_AND_BUDGET.md`、`D18_PLACEHOLDER_REPLACEMENT_MAP.md` |
| D18 handoff | `docs/game/design/common/handoffs/D18/D18-A*_HANDOFF.md`、`D18-S*_HANDOFF.md`、`D18-R03-*_HANDOFF.md`、`D18-S02_V2_HANDOFF.md` |
| D19/D20 历史 | `game/tools/results/D19/D19-R03-*`、`D19-R02-S02`、`D20-A01-R*`、`D20-A02-R*`、`D20-S02-R*` 及全部 RERUN* 机器结果；`docs/game/design/common/D19_R0*`、`D20_*` 文档 |
| 归档 | `docs/game/archive/runtime-first-plan-20260816/`（归档说明即声明 SUPERSEDED） |
| 记忆日志 | `.workbuddy/memory/*.md` 中的 R01—R03/RERUN 记录（追溯用） |

规则：

- 上述目录/文档/机器结果中记录的“PASS/PARTIAL/BLOCKED、截图、性能样本、Player 日志”只可作历史诊断证据；D19-R03 RERUN 系列证明真实 Player 路线尚未通过，且不再是当前纯逻辑阶段的任务；
- 新 L 编号全部从 `PLANNED` 开始，必须重新以纯逻辑证据取得 `LOGIC_PASS`；
- L 任务引用这些材料时必须写明“历史证据引用，不作为通过依据”。

## 5. L02—L50 波次与可开始条件（冻结）

### 5.1 波次总表

| 波次 | 模式 | 任务 | 可开始条件 |
|---|---|---|---|
| 0 | S | `D18-L01`（本任务） | — |
| 1 | P | `D18-L02`—`D18-L20`（19 项） | `D18-L01 = LOGIC_PASS`；每项唯一写入范围不重叠；只依赖 L01 |
| 2 | P | `D18-L21`—`D18-L38`（18 项） | 波次 1 全部 `LOGIC_PASS`（无 P0 blocker）；必须使用 L02—L20 已冻结字段；若所需 Schema 尚未合并 → 保持 `LOGIC_BLOCKED`，**不得复制字段定义** |
| 3 | S | `D18-L39` → `L40` → `L41` → `L42` → `L43` → `L44` → `L45` → `L46` | `L39` 等待 L02—L38 全部结束（有未证实项则保持 BLOCKED，不得聚合成 PASS）；此后每项前一任务 `LOGIC_PASS` |
| 4 | G/S | `D18-L47` → `L48` → `L49` → `L50` | 每项前一任务 `LOGIC_PASS`；`L50` 为章节唯一逻辑验收：`LOGIC_PASS` 时 D19 港口逻辑集成可将 D18 输出视为稳定输入 |

- 串行冻结：`L01`、`L39`—`L46`、`L47`—`L50` 为串行唯一写者，**无论电脑空闲与否都必须等待前一任务完成**。
- `P` 并行只表示写入范围不同；两个 Agent 不得同时写同一文件/同一目录。
- 跨章：`D19-L01` 只读基线盘点可在 `D18-L01` 通过后并行开始；D19 其余任务等待 `D18-L50`。
- 旧 D18-A/S/R、D19-R03 结果**永远不能作为任何 L 任务的前置**（见第 4 节）。

### 5.2 每任务唯一写入范围（冲突写入边界，与章节任务卡一致）

| 任务 | 唯一写入范围 |
|---|---|
| L02 | `tools/schemas/d18_logic_asset_record.schema.json`、`tools/tests/d18_logic_asset_record_fixtures/` |
| L03 | `docs/game/design/common/assets/D18_ASSET_STATE_MACHINE.md` |
| L04 | `tools/d18_license_normalizer.py`、`tools/tests/test_d18_license_normalizer.py` |
| L05 | `tools/d18_provenance_validator.py` |
| L06 | `tools/d18_hash_validator.py` |
| L07 | `tools/d18_distribution_rights_validator.py` |
| L08 | `tools/d18_asset_path_validator.py` |
| L09 | `tools/d18_attribution_validator.py` |
| L10 | `tools/d18_package_boundary_validator.py` |
| L11 | `docs/game/design/common/assets/D18_TAXONOMY_SHIPS_PORT.md` |
| L12 | `docs/game/design/common/assets/D18_TAXONOMY_CHARACTERS.md` |
| L13 | `docs/game/design/common/assets/D18_TAXONOMY_ECOLOGY.md` |
| L14 | `docs/game/design/common/assets/D18_TAXONOMY_UI_AUDIO.md` |
| L15 | `tools/d18_content_id_asset_link_validator.py` |
| L16 | `tools/d18_resource_key_validator.py` |
| L17 | `tools/d18_candidate_runtime_guard.py` |
| L18 | `tools/d18_release_claim_guard.py` |
| L19 | `docs/game/design/common/assets/D18_WAIVER_AND_REJECTION_RULES.md` |
| L20 | `docs/game/design/common/assets/D18_LOGIC_RESOURCE_INDEX.md` |
| L21 | `docs/game/design/common/assets/sources/d18_kenney_logic_catalog.json` |
| L22 | `docs/game/design/common/assets/sources/d18_quaternius_logic_catalog.json` |
| L23 | `docs/game/design/common/assets/sources/d18_polyhaven_logic_catalog.json` |
| L24 | `docs/game/design/common/assets/sources/d18_ambientcg_logic_catalog.json` |
| L25 | `tools/schemas/d18_import_descriptor.schema.json` |
| L26 | `tools/d18_mesh_descriptor_validator.py` |
| L27 | `tools/d18_material_descriptor_validator.py` |
| L28 | `tools/d18_animation_descriptor_validator.py` |
| L29 | `tools/d18_ui_audio_descriptor_validator.py` |
| L30 | `tools/d18_asset_dependency_graph.py` |
| L31 | `tools/d18_duplicate_identity_audit.py` |
| L32 | `tools/d18_name_collision_audit.py` |
| L33 | `tools/d18_catalog_claim_audit.py` |
| L34 | `tools/d18_missing_evidence_audit.py` |
| L35 | `tools/d18_status_transition_audit.py` |
| L36 | `tools/d18_local_development_guard.py` |
| L37 | `tools/schemas/d18_runtime_promotion_request.schema.json` |
| L38 | `tools/d18_generated_asset_guard.py` |
| L39 | `tools/d18_logic_audit.py` |
| L40 | `tools/tests/test_d18_logic_valid_records.py` |
| L41 | `tools/tests/test_d18_logic_invalid_records.py` |
| L42 | `tools/d18_logic_catalog_compiler.py` |
| L43 | `docs/game/design/common/D18_LOGIC_SOURCE_AUDIT.md` |
| L44 | `tools/d18_game_reference_audit.py` |
| L45 | `tools/d18_schema_compatibility_audit.py` |
| L46 | `docs/game/design/common/D18_LOGIC_HANDOFF_INTERFACE.md` |
| L47 | `game/tools/results/D18/D18-L47/` |
| L48 | `docs/game/design/common/D18_BINARY_AND_IMPORT_DEFERRAL.md` |
| L49 | `docs/game/design/common/D18_LOGIC_STATUS_REVIEW.md` |
| L50 | `docs/game/design/common/D18_LOGIC_ACCEPTANCE.md` |

每项任务另含自己的结果目录 `game/tools/results/D18/D18-Lnn/`、handoff `docs/game/design/common/handoffs/D18/D18-Lnn_LOGIC_HANDOFF.md` 与当日记忆日志。

### 5.3 全章禁区（任何 L 任务不得写入）

`game/Assets/Scenes/InitialHarbor.unity`、正式 Prefab、Canvas、Player/Harbor Builder、Unity Build Settings、`game/Assets/Scripts/Systems/`、`game/AgentCore/`、`contracts/`、`server/`、`web/`、既有 A 系列 manifest/catalog/机器结果、`asset_state`、Registry、存档 Schema。发现相邻模块缺陷 → 记录 blocker，不越权修复。

## 6. 共享字段词典（L02—L45 全章统一语义，冻结）

> 所有 Schema、目录、验证器、工具输出与测试必须使用同一字段语义；L45 自动核对一致性。语义冲突时以本文为准，本文缺失时以任务卡为准并记录待定。

### 6.1 资源逻辑记录字段（L02 Schema 必须覆盖）

| 字段 | 语义 | 唯一写者 | 禁止/口径 |
|---|---|---|---|
| `canonical_asset_id` | 规范资源 ID（业务键） | 来源目录任务 | 不得用磁盘路径或显示名 |
| `category` | 逻辑分类（L02 的 11 值资源大类，永远不变） | L11—L14 分类冻结 | 必须命中 11 值大类枚举；细分类不得直接写入本字段 |
| `taxonomy_category` | 可选领域细分类（双层分类契约，D18-L51） | L11—L14 词典 + 机器映射 | 取值必须存在于 [d18_taxonomy_category_map.json](../../../../tools/schemas/d18_taxonomy_category_map.json) 且映射出的父类必须等于 `category`；未知细分类或父类不匹配 → P0；无本字段的既有大类记录仍合法 |
| `source_id` | `source_<来源名>` | L21—L24 | 同一来源只登记一次 |
| `source_url` | 官方来源 URL | L21—L24 | 不访问网络验证；仅静态记录 |
| `license` | 规范化许可对象 | L04 标准化 | 未知/冲突/NC → 阻断或 Pending，不得猜测 CC0 |
| `rights` | 四项权利（商业使用/修改/仓库存储/随游戏分发） | L07 | 每项必须显式记录 |
| `download_date` | 实际下载日期 | L21—L24 | 未下载 → null，不写猜测日期 |
| `original_filename` | 原始包/文件名 | L21—L24 | 不因改名造新包 ID |
| `sha256` | 真实文件/归档哈希 | L06 校验 | 64 位十六进制；缺原文件 → Pending，绝不补猜 |
| `state` | 第 2 节状态枚举 | 见 2.6 | 禁止未知状态与越级状态 |
| `content_ids` | 内容映射 | L15 | 引用必须存在；不允许游戏数据反向依赖候选文件 |
| `evidence_paths` | 证据文件相对路径 | 各任务 | 仓库内相对路径；禁止绝对路径泄露 |
| `runtime_deferred` | 逐记录 Runtime 风险 | 各任务 | 必须诚实登记，不作为通过替代 |

### 6.2 既有 manifest / 准入目录字段（只读兼容层）

manifest：`manifest_kind`、`schema_version`、`source_id`、`pack_id`、`title`、`author`、`official_page`、`download_url`、`downloaded_at`、`archive_name`、`sha256`、`license_id`、`license_url`、`license_evidence_path`、`attribution_required`、`commercial_use`、`modification_allowed`、`source_redistribution`、`build_distribution`、`category`、`file_count`、`unity_object_count`、`review_status`、`previous_review_status`、`import_status`、`candidate_files[{path, role, target_id}]`。

准入目录：`intake_id`、`manifest_id`、`manifest_path`、`candidate_file_path`、`content_id`、`domain`、`priority`、`role`、`intake_eligible`、`license{license_id, license_evidence_path, commercial_use, modification_allowed, source_redistribution, build_distribution}`、`hash_evidence{path, field, sha256, scope}`、`import_verified_evidence_path`、`allowed_task_scope`、`forbidden_scope`、`known_gaps`、`local_development_ready_evidence_required`、`responsibility{intake_owner, consumer_owner, promotion_owner}`。

关键口径：

- `hash_evidence.scope` 必须保留来源口径（当前为 “manifest archive or selected-file digest”），L 系列不得把它当作逐文件 digest 宣称；逐文件哈希由 L06/L31 真实计算。
- `file_count`/`unity_object_count` 只有真实统计才写；数量宣称由 L33 用真实静态目录复核，不接受手填总数。
- `intake_eligible` 语义冻结：许可、哈希、路径、导入证据与映射齐全，可被指定任务用于受控工作场景/Prefab；不表示 `LocalDevelopmentReady`、正式场景接入、运行时消费者、`ReleaseApproved` 或 Steam Ready。

### 6.3 描述器与审计字段（L25—L38 方向性冻结）

- 导入描述器（L25）：比例、轴向、材质槽、骨架、动画元数据；**不导入任何资源**。
- 网格/材质/动画/UI 音频描述器（L26—L29）：用途、尺度单位、碰撞意图、颜色空间、骨架类型、root-motion 策略、缺失回退声明；实际网格检查延期。
- 依赖图（L30）：检测循环、悬挂依赖、候选→发布跨级引用。
- 重复身份（L31）：按 hash、来源、canonical_id 检测；只登记不删除。
- 晋级申请（L37）：申请数据字段由 L37 定义；**本章禁止产生 RuntimeApproved 批准结果**。

## 7. 结果 JSON 格式（统一，冻结）

每个任务在唯一目录写 `D18_Lnn_LOGIC_RESULT.json`：

```json
{
  "task_id": "D18-Lnn",
  "status": "LOGIC_PASS",
  "updated_at": "2026-08-16T12:00:00+08:00",
  "summary": "一句话说明真实完成内容",
  "changed_files": ["相对路径"],
  "verification": [
    {"command": "实际执行的命令", "result": "PASS 或失败摘要"}
  ],
  "runtime_deferred": ["必须留给 Runtime 阶段的风险"],
  "blockers": [],
  "handoff": "docs/game/design/common/handoffs/D18/D18-Lnn_LOGIC_HANDOFF.md"
}
```

- `status` 仅允许 `PLANNED`、`IN_PROGRESS`、`LOGIC_PASS`、`LOGIC_PARTIAL`、`LOGIC_BLOCKED`、`RUNTIME_DEFERRED`；
- `LOGIC_PASS` 必须无 P0 blocker 且 `verification` 含实际命令与结果；没有证据时必须 `LOGIC_PARTIAL` 或 `LOGIC_BLOCKED`；
- `task_id` 必须与目录/编号一致（sync 脚本核对，不一致自动标 BLOCKED）；
- `verification` 里的命令必须真实执行（含本文件第 3.1 节允许项），不得引用旧 A/S/R 结论；
- `runtime_deferred` 如实登记（见第 9 节），不得以“延期”替代未完成；
- 完成后运行 `python tools/sync_logic_task_status.py` 与 `git diff --check`；状态总览 `docs/game/LOGIC_TASK_STATUS.md` 只由该脚本写入，禁止手改。

## 8. 冲突写入边界（汇总）

1. 每任务唯一写入范围见 5.2；同一文件双写 → 后写者先读再合并，禁止覆盖他人改动；无法合并 → `LOGIC_BLOCKED` 并记录。
2. 共享文件写者：`docs/game/LOGIC_TASK_STATUS.md`（仅 sync 脚本）；`docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json` 与 `d18_local_development_ready_catalog.json`（历史 A 系列产物，L 任务只读，不在本章修改；L42 编译器输出新的 `sources/` 目录与编译结果）；各 Schema 仅其任务卡所有者写。
3. 旧 D18-A/S/R、D19/D20 历史目录与文档只读（第 4 节），任何 L 任务不得修改、重跑或覆盖。
4. 全章禁区见 5.3；发现越界写入 → 还原并记录 blocker。

## 9. Runtime 延期清单（当前已知，冻结）

| 风险 | 说明 |
|---|---|
| RuntimeApproved/ReleaseApproved 授予 | 本逻辑阶段禁止；真实运行时消费证据缺失 |
| 逐文件 SHA-256 完整性 | 准入目录 hash_evidence 为归档级/所选文件级 digest；逐文件哈希待 L06/L31 真实计算，最终文件完整性校验属 Runtime |
| Unity 导入后的实际网格/材质/动画 | L26—L28 只做描述器静态验证；真实导入验证延期 |
| Prefab/场景/Canvas 集成、碰撞、LOD、Animator 装配 | 全部延期 |
| 保存/恢复与回退在真实运行时 | 延期 |
| A04 人工审核 267 条 Pending 与 D16 外部证据 | 非 D18 章节责任，保持未完成 |
| 候选磁盘文件与准入目录数量差异（229） | 由 L31/L33/L34 静态对账，最终由 Runtime 阶段复核 |

## 10. 链接索引（本文件引用的既有文件）

- 路线与协议：`../../D18_D24_VISUAL_PLAYABLE_ROADMAP.md`、`../../LOGIC_TASK_PROTOCOL.md`、`../../AGENT_DEVELOPMENT_CHAPTER_10_D18.md`、`../../AGENT_DEVELOPMENT_CHAPTER_10_D18_PROMPTS.md`、`../../AGENT_DEVELOPMENT_TASKS.md`、`../../LOGIC_TASK_STATUS.md`
- 数据：`assets/d18_prefab_candidate_intake_catalog.json`、`assets/d18_local_development_ready_catalog.json`、`assets/replacement/d18_placeholder_replacement_map.json`、`assets/d18_asset_pack_manifest.example.json`
- 工具：`../../../tools/schemas/d18_asset_pack_manifest.schema.json`、`../../../tools/d18_asset_manifest_audit.py`、`../../../tools/d18_prefab_candidate_intake_audit.py`、`../../../tools/sync_logic_task_status.py`
- 历史：`../D18_D20_R03_STATUS.md`、`../D18_FREE_ASSET_SOURCE_CATALOG.md`、`../../archive/runtime-first-plan-20260816/README.md`
