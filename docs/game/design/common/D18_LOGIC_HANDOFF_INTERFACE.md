# D18 只读逻辑交接接口（D18-L46）

> 任务：`D18-L46`（波次 3，第 8 项；前置 `D18-L45=LOGIC_PASS`）
> 发布日期：2026-08-16
> **本接口供 D19—D24 消费方只读使用**。它不是 Schema、不是验证器、不是 Registry，也不授予任何状态。

## 1. 定位与读取范围

- D19—D24 任务在引用 D18 逻辑数据前必须阅读本文与 [D18 逻辑治理基线](D18_LOGIC_GOVERNANCE_BASELINE.md)（L01）；
- 本文汇总：有效 Schema/版本、ResourceKey/content_id 规则、状态含义与不可消费状态、错误码、来源 catalog、审计命令、唯一写者、Runtime Deferred；
- **红线**：下游不得引用 `Assets/Art/Candidates` 二进制、不得把任何 Pending/rejected 记录当 Ready、不得声称资源已导入或已批准。

## 2. 有效 Schema 与版本（只读）

| Schema | 路径 | schema_version | 用途 |
|---|---|---|---|
| L02 资源逻辑记录 | [d18_logic_asset_record.schema.json](../../../../tools/schemas/d18_logic_asset_record.schema.json) | 无独立版本字段（13 个必需字段冻结） | 每条候选/准入资源的规范记录 |
| L25 导入描述 | [d18_import_descriptor.schema.json](../../../../tools/schemas/d18_import_descriptor.schema.json) | `1` | 未来 Runtime 导入意图声明（非导入设置） |
| L37 晋级申请 | [d18_runtime_promotion_request.schema.json](../../../../tools/schemas/d18_runtime_promotion_request.schema.json) | `1` | 运行时晋级申请数据（本阶段不产生批准） |
| 历史 manifest | [d18_asset_pack_manifest.schema.json](../../../../tools/schemas/d18_asset_pack_manifest.schema.json) | `1` | 包级 manifest（只读数据源） |

版本一致性由 [D18-L45 兼容性审计](../../../../tools/d18_schema_compatibility_audit.py) 保证（5 个语义槽全部一致，机器结果见 §6）。

## 3. 合法 ResourceKey / content_id 规则

### 3.1 ResourceKey（D18-L16 冻结）

- 格式：命名空间三段 `d10:<segment>:<tail>` 或 `d20:<segment>:<tail>`（冻结命名空间仅 `d10`、`d20`）；或非命名空间 `<prefix>_<tail>`；
- 前缀映射（`PREFIX_CATEGORY_MAP`，L16）：`ship_*`/`boat_*`→ship；`crew_*`/`npc_*`/`outfit_*`→character；`animal_*`/`pet_*`/`wildlife_*`→animal；`fish_*`→fish；`plant_*`/`island_*`/`quest_*`→environment；`building_*`/`port_service_*`/`dock_*`/`berth_*`→building；`prop_*`/`item_*`→prop；`ui_*`/`icon_*`/`font_*`→ui；`audio_*`/`music_*`→audio；`material_*`→material；`hdri_*`/`lighting_*`→lighting_reference；`asset_*`→任意（11 类）；
- `candidate_`/`d18a05_` 是历史候选前缀：**只能作为 `deprecated_alias_of` 别名出现，不得作为活跃键**（L16 P0）；
- 键必须独立于二进制与 Unity GUID；禁止磁盘路径/显示名作键（L16 拒绝 `display_name_as_key`）。

### 3.2 content_id（D18-L15 冻结）

- 必须是 lower snake_case 业务键（`^[a-z0-9_]+$`）；
- 禁止候选文件路径、显示名、绝对路径（L15 `path_like_content_id`/`candidate_path_leak` P0）；
- 引用必须存在于内容定义源；前缀类型必须匹配 category；不得重复、不得 deprecated。

### 3.3 业务引用纪律（D18-L44 强制）

- game 代码/数据/Registry 中键位置与引用字段（`sky_reference_asset`、`model_ref`、`resource_path` 等）只能使用稳定键；引用 `candidate_*` 值 = P0（L52 已修复 `D19LightingProfileCatalog.json` 5 处，L44 复扫 runtime_findings_count=0）。

### 3.4 双层分类契约（D18-L51 冻结）

- **`category`**：永远是 L02 的 **11 值资源大类**（animal/audio/building/character/environment/fish/lighting_reference/material/prop/ship/ui）；细分类不得直接写入 `category`；
- **`taxonomy_category`**（可选）：领域细分类；取值必须存在于机器映射 [d18_taxonomy_category_map.json](../../../../tools/schemas/d18_taxonomy_category_map.json)（8 个已登记：terrestrial_animal/aquatic_animal/bird/pet→animal；npc_visual/player_visual/hair/outfit→character），且映射出的父类必须等于 `category`；
- 未知细分类或父类不匹配 → P0（L39/L42/L45 同语义）；无 `taxonomy_category` 的既有大类记录仍合法；
- 下游**只能读取静态数据**（Schema/来源目录/编译产物/机器结果），不得引用 `Assets/Art/Candidates` 二进制、不得把 Candidate/Pending 当 Ready、不得声称资源已导入。

## 4. 状态含义与不可消费状态（D18-L01/L03 冻结）

| 状态 | 含义 | 下游可消费？ |
|---|---|---|
| `Candidate` | 有来源入口/文件但证据未完备 | **否**（二进制禁止引用） |
| `IntakeEligible` | 证据完备，可被指定任务用于受控工作场景/Prefab | 是（仅受控工作场景） |
| `RuntimeRequested` | 消费任务已提交晋级申请（申请非批准） | 是（只读申请数据） |
| `RuntimeApproved` | 运行时消费证据完备 | 本阶段 0 个；仅未来 Runtime 门禁授予 |
| `ReleaseApproved` | 人类审核的发布审批 | 本阶段 0 个；仅人类授予 |
| `Rejected` / `Revoked` | 拒绝/撤销终态（append-only 审计） | 否（不可消费） |

**不可消费状态**：`Candidate`（二进制）、`Rejected`、`Revoked`；sha256=null（Pending hash）的记录不得当作 Ready（L06 语义：Pending 不批准，不补猜）。迁移矩阵见 [D18_ASSET_STATE_MACHINE.md](assets/D18_ASSET_STATE_MACHINE.md)（L03）。

## 5. 错误码总表（跨工具冻结规则）

| 语义 | 规则码 | 工具 |
|---|---|---|
| 缺来源 / 缺许可 / 许可疑似(NC/未知) / 缺权利 | `missing_source`、`missing_license`、`license_suspect`、`missing_rights` | L39 |
| 缺内容映射 / 缺证据 / 路径越界 | `missing_content_mapping`、`missing_evidence`、`path_escape` | L39 |
| 哈希 Pending（不批准） | `hash_pending` | L39 / L06 |
| 非法状态 / 本阶段越级批准 | `state_invalid`、`approval_state_in_logic_phase` | L39 |
| 分类/键双层契约 | `category_invalid`（非法大类）、`unknown_taxonomy_category`（未知细分类）、`taxonomy_parent_mismatch`（父类不匹配） | L39 / L42 / L45 |
| content_id 分裂 | `missing_content_id`、`unknown_prefix`、`type_mismatch`、`duplicate_content_id`、`deprecated_content_id`、`path_like_content_id`、`candidate_path_leak`、`candidate_key_leak` | L15 |
| ResourceKey 分裂 | `unknown_namespace`、`unknown_prefix`、`category_prefix_mismatch`、`content_id_mismatch`、`display_name_as_key`、`candidate_prefix_active`、`duplicate_key` | L16 |
| 发布声明越界 | `doc_overstep_claim`、`superseded_result_as_pass`、`json_release_approved_without_human`、`json_readiness_claim`、`json_approval_state_hold` | L18 |
| 重复 identity | `duplicate_record`、`path_hash_conflict`、`file_missing`、`hash_mismatch` | L31 |
| 谱系分裂 | `non_original_missing_lineage`、`generated_masquerading_as_original`、`non_original_with_claim`、`license_widening`、`origin_with_parent`、`origin_missing_claim`、`self_parent`、`circular_lineage`、`missing_parent_reference`、`missing_license_basis` | L38 |
| 游戏引用违规 | `candidate_path_reference`、`temp_cache_reference`、`archive_reference`、`external_archive_path`、`candidate_key_as_business_key`、`key_position_path_reference`、`display_name_as_key` | L44 |
| 编译拒绝 | `rejected`（原因如 `category 非法：'<细分类>'`） | L42 |
| 兼容性分裂 | `slot_inconsistent`、`category_split_real_data`、`cross_tool_acceptance_split`、`result_json_*` | L45 |

严重度约定：P0=阻断（BLOCKED）；P1=疑似/提示（不阻断，需人工复核）；INFO=test/editor/注释上下文。

## 6. 来源 catalog 与审计命令

### 6.1 数据位置

- 来源目录：`docs/game/design/common/assets/sources/d18_*_logic_catalog.json`（L21—L24；kenney 9、quaternius 61、polyhaven 91、ambientcg 0）；
- 规范编译产物（L42）：**100 条合法记录（全 `Candidate`）+ 61 条 rejected（细分类）+ 4 条 Pending hash**；编译命令：`python tools/d18_logic_catalog_compiler.py --output <path>`；
- 真实审计快照：`game/tools/results/D18/D18-L43/D18_L43_L39_AUDIT.json`、`D18_L43_L42_COMPILE.json`；报告见 [D18_LOGIC_SOURCE_AUDIT.md](D18_LOGIC_SOURCE_AUDIT.md)（L43）。

### 6.2 审计命令（D19—D24 消费方复核用）

```text
python tools/d18_logic_audit.py --output <path>                     # L39 聚合审计
python tools/d18_logic_catalog_compiler.py --output <path>          # L42 目录编译
python tools/d18_game_reference_audit.py --output <path>            # L44 游戏引用审计
python tools/d18_schema_compatibility_audit.py --output <path>      # L45 兼容性审计
python tools/sync_logic_task_status.py                              # 逻辑任务状态总览
```

## 7. 唯一写者（谁可以写入什么）

| 数据 | 唯一写者 |
|---|---|
| 来源目录 `sources/*.json` | L21—L24（只读给全部下游） |
| 规范 catalog（L42 编译产物） | L42 编译器（确定性输出） |
| 记录状态迁移（Candidate→IntakeEligible→…） | L01 基线 §2.6：IntakeEligible=L42 门；RuntimeRequested=消费任务；RuntimeApproved=未来 Runtime 门禁；ReleaseApproved=责任人类 |
| 谱系记录（generated/derived 登记） | L38 守卫校验 + 消费任务登记 |
| 结果 JSON / handoff | 对应任务唯一写者 |
| `LOGIC_TASK_STATUS.md` | 仅 `sync_logic_task_status.py` |

## 8. Runtime Deferred 清单（本阶段明确延期）

1. Unity 导入、Prefab/场景/Canvas 装配、碰撞、LOD、Animator、材质/音频消费者；
2. 真实顶点/碰撞/LOD 测量与逐文件 SHA-256（L06/L31 对真实文件补证）；
3. 保存/恢复、性能、截图、输入与路线验证；
4. `RuntimeApproved`/`ReleaseApproved` 授予（两者当前均为 0）；
5. 已知阻塞项（L45 记录）：category 权威映射（8 个细分类 vs 11 值枚举）待 L50 验收前定案；
6. D19LightingProfileCatalog.json 的 `candidate_hdri_*` 引用修复（L44 检出，归 D19/D21）。

## 9. 下游使用契约（成功标准）

- 只读使用：下游任务可读取 §2 Schema、§6 catalog、各机器结果与本文，但**不得修改** D18 数据/工具；
- **不得引用 Candidate 二进制**（§3.3/§4）；
- **不得声称资源已导入**（`represents_imported_asset` 恒为 false；无 RuntimeApproved/ReleaseApproved 持有者）；
- 违反任一红线 = D18 接口使用违规，对应任务应 BLOCKED。
