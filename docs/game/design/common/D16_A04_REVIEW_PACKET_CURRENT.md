# D16-A04 Pending 人工复核填写包

> 本文件只生成逐条填写模板和证据入口，不批准历史事实、敏感内容、授权或资产状态。

- 包编号：`D16-A04-REVIEW-PACKET-2026-08-13`
- 状态：**HUMAN_DECISION_REQUIRED**
- Pending 条目：**267**
- 审查批次：**12**（每批最多 25 条）
- 引用缺口：**0**
- 已有本地文件型复核入口：**267** 条
- 所有条目初始决定均为 `Pending`；只有责任审核者提供真实证据后，才能调用单条决定工具。

## 填写规则

1. 每条记录单独填写责任审核者、日期、决定依据和新增证据文件。
2. `ApprovedWithWarning` 必须同时填写警告、展示级别和跳过策略；敏感条目不能直接 `Approved`。
3. 目录引用只是上下文入口，不等同于决定证据；写回工具要求新增证据引用最终解析到文件。
4. 不修改 `historical_tag`、`source`、`license`、`authorization`、`asset_state` 或玩法数值。

## 批次清单

| 批次 | 范围 | 条目数 | 首条 ID | 末条 ID |
|---|---|---:|---|---|
| D16-A04-001 | asset_history_and_authorization | 25 | asset_blockade_order | asset_d15_a06_model_function_cargo_davit |
| D16-A04-002 | asset_history_and_authorization | 25 | asset_d15_a06_model_function_cargo_hatch | asset_d15_a06_model_function_medical_berth |
| D16-A04-003 | asset_history_and_authorization | 25 | asset_d15_a06_model_function_medicine_cabinet | asset_d15_a06_model_island_packet_sloop |
| D16-A04-004 | asset_history_and_authorization | 25 | asset_d15_a06_model_longwake_barque | asset_d15_a06_model_ship_module_crew_rest_area |
| D16-A04-005 | asset_history_and_authorization | 25 | asset_d15_a06_model_ship_module_crew_storage_lockers | asset_d15_a06_model_ship_module_light_canvas |
| D16-A04-006 | asset_history_and_authorization | 25 | asset_d15_a06_model_ship_module_light_cargo_dividers | asset_d15_a06_model_ship_module_spare_bucket_line |
| D16-A04-007 | asset_history_and_authorization | 25 | asset_d15_a06_model_ship_module_spare_mast_kit | asset_vegetation_profile_crop |
| D16-A04-008 | asset_history_and_authorization | 11 | asset_vegetation_profile_epiphyte | asset_vegetation_profile_wetland |
| D16-A04-009 | historical_content | 25 | animal_domestic_cattle | item_fruit_guava |
| D16-A04-010 | historical_content | 25 | item_gear_cast_net | item_seed_sugarcane |
| D16-A04-011 | historical_content | 25 | item_seed_tobacco | outfit_injury_old_leg |
| D16-A04-012 | historical_content | 6 | outfit_shoes_deck_boots | reward_d14_worldclass_reinforced_keel |

## 逐条填写表

### `asset_blockade_order`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_blockade_order.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md`
- 现有复核入口：`docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_cargo_seizure_writ`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_cargo_seizure_writ.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md`
- 现有复核入口：`docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_anchor_profiles`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_anchor_profiles.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_animation_profiles`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_animation_profiles.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_audio_profiles`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_audio_profiles.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_collision_profiles`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_collision_profiles.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_material_profiles`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_material_profiles.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_blackreef_brig`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_blackreef_brig.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_bluewater_cutter`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_bluewater_cutter.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_coastal_bark`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_coastal_bark.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_cocoa_brigantine`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_cocoa_brigantine.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_deepwater_frigate`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_deepwater_frigate.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_free_cove_catboat`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_free_cove_catboat.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_aft_mast`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_aft_mast.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_anchor_chain_guide`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_anchor_chain_guide.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_anchor_chain_locker`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_anchor_chain_locker.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_anchor_winch`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_anchor_winch.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_bilge_pump_access`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_bilge_pump_access.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_boarding_net`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_boarding_net.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_boarding_rail`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_boarding_rail.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_boat_hook_rack`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_boat_hook_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_bow_bollard`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_bow_bollard.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_breast_line_point`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_breast_line_point.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_captains_cabin`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_captains_cabin.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_cargo_davit`

- 批次：`D16-A04-001`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_cargo_davit.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_cargo_hatch`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_cargo_hatch.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_carpenter_workbench`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_carpenter_workbench.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_chart_table`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_chart_table.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_compass_stand`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_compass_stand.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_cook_workbench`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_cook_workbench.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_dry_goods_shelf`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_dry_goods_shelf.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_fire_bucket_station`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_fire_bucket_station.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_fish_hold`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_fish_hold.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_food_larder`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_food_larder.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_fore_mast`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_fore_mast.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_fore_sail`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_fore_sail.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_freshwater_barrel_rack`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_freshwater_barrel_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_galley_stove`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_galley_stove.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_gangplank`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_gangplank.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_gun_carriage`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_gun_carriage.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_gunner_workbench`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_gunner_workbench.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_gunport`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_gunport.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_hand_winch`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_hand_winch.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_helm_wheel`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_helm_wheel.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_log_line`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_log_line.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_lookout_platform`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_lookout_platform.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_main_cargo_hold`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_main_cargo_hold.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_main_mast`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_main_mast.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_main_sail`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_main_sail.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_medical_berth`

- 批次：`D16-A04-002`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_medical_berth.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_medicine_cabinet`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_medicine_cabinet.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_port_anchor`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_port_anchor.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_powder_magazine`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_powder_magazine.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_rigging_locker`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_rigging_locker.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_rudder`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_rudder.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_sail_trim`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_sail_trim.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_sailor_berth`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_sailor_berth.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_sandglass`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_sandglass.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_ship_bell_alarm`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_ship_bell_alarm.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_shot_locker`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_shot_locker.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_signal_flag_locker`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_signal_flag_locker.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_signal_gun`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_signal_gun.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_sounding_lead`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_sounding_lead.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_starboard_anchor`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_starboard_anchor.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_staysail`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_staysail.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_steering_transmission`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_steering_transmission.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_stern_bollard`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_stern_bollard.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_temporary_deck_cargo`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_temporary_deck_cargo.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_tiller`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_tiller.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_tool_rack`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_tool_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_valuables_locker`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_valuables_locker.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_waste_treatment_point`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_waste_treatment_point.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_function_weapon_rack`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_function_weapon_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_harbor_brig`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_harbor_brig.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_island_packet_sloop`

- 批次：`D16-A04-003`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_island_packet_sloop.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_longwake_barque`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_longwake_barque.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_mangrove_punt`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_mangrove_punt.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_privateer_brigantine`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_privateer_brigantine.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_raincrown_schooner`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_raincrown_schooner.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_reef_runner_sloop`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_reef_runner_sloop.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_sable_fin_skiff`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_sable_fin_skiff.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_saint_marys_ketch`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_saint_marys_ketch.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_saltwind_schooner`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_saltwind_schooner.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_auxiliary_staysail`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_auxiliary_staysail.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_ballast_rack`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_ballast_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_bilge_drain_pump`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_bilge_drain_pump.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_boarding_net`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_boarding_net.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_braced_cargo_shelves`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_braced_cargo_shelves.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_braced_frames`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_braced_frames.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_braced_gun_carriage`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_braced_gun_carriage.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_braced_mast`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_braced_mast.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_captains_cabin_refit`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_captains_cabin_refit.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_cargo_davit`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_cargo_davit.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_cargo_winch`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_cargo_winch.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_carpenter_workbench`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_carpenter_workbench.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_chain_shot_rack`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_chain_shot_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_concealed_cargo_compartment`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_concealed_cargo_compartment.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_confinement_isolation_cabin`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_confinement_isolation_cabin.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_council_table`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_council_table.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_crew_rest_area`

- 批次：`D16-A04-004`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_crew_rest_area.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_crew_storage_lockers`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_crew_storage_lockers.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_deck_catch_area`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_deck_catch_area.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_dinghy_davit`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_dinghy_davit.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_disguise_flag_mount`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_disguise_flag_mount.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_dry_cargo_hold`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_dry_cargo_hold.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_dry_medicine_cabinet`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_dry_medicine_cabinet.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_dry_powder_locker`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_dry_powder_locker.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_enlarged_main_sail`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_enlarged_main_sail.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_expanded_cargo_hold`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_expanded_cargo_hold.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_extra_crew_berths`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_extra_crew_berths.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_fire_bucket_rack`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_fire_bucket_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_fire_bulkhead`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_fire_bulkhead.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_fire_sand_box`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_fire_sand_box.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_flag_signal_set`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_flag_signal_set.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_food_preservation_cabinet`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_food_preservation_cabinet.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_freshwater_tank`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_freshwater_tank.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_grape_shot_rack`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_grape_shot_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_heavy_gun_mount`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_heavy_gun_mount.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_high_lookout_platform`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_high_lookout_platform.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_hull_paint_layer`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_hull_paint_layer.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_improved_fore_sail`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_improved_fore_sail.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_improved_galley_stove`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_improved_galley_stove.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_improved_helm_wheel`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_improved_helm_wheel.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_isolation_partition`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_isolation_partition.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_light_canvas`

- 批次：`D16-A04-005`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_light_canvas.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_light_cargo_dividers`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_light_cargo_dividers.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_light_gun_mount`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_light_gun_mount.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_locked_valuables_case`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_locked_valuables_case.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_long_voyage_granary`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_long_voyage_granary.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_name_and_crest_board`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_name_and_crest_board.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_navigation_compass`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_navigation_compass.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_night_signal_lamp`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_night_signal_lamp.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_professional_sailor_platform`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_professional_sailor_platform.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_quick_release_rigging`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_quick_release_rigging.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_quiet_rigging`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_quiet_rigging.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_raised_freeboard`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_raised_freeboard.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_rapid_loading_rack`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_rapid_loading_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_rapid_patch_layer`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_rapid_patch_layer.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_reinforced_dinghy`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_reinforced_dinghy.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_reinforced_keel`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_reinforced_keel.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_reinforced_planking`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_reinforced_planking.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_reinforced_rudder`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_reinforced_rudder.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_rescue_rope_and_float`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_rescue_rope_and_float.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_shallow_draft_conversion`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_shallow_draft_conversion.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_ship_sickbay`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_ship_sickbay.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_shot_sorting_rack`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_shot_sorting_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_simple_medical_berth`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_simple_medical_berth.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_sounding_gear_rack`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_sounding_gear_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_spare_anchor_chain_set`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_spare_anchor_chain_set.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_spare_bucket_line`

- 批次：`D16-A04-006`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_spare_bucket_line.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_spare_mast_kit`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_spare_mast_kit.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_spare_rope_rack`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_spare_rope_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_spare_tiller_assembly`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_spare_tiller_assembly.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_spare_timber_rack`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_spare_timber_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_stability_sponson`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_stability_sponson.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_storm_canvas`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_storm_canvas.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_twin_rudder_linkage`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_twin_rudder_linkage.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_ventilation_hatch`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_ventilation_hatch.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_washing_and_hygiene_point`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_washing_and_hygiene_point.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_water_saving_kitchen`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_water_saving_kitchen.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_watertight_bulkhead`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_watertight_bulkhead.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_weapon_and_firearm_rack`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_weapon_and_firearm_rack.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_ship_module_weatherproof_chart_case`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_ship_module_weatherproof_chart_case.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_sugar_route_schooner`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_sugar_route_schooner.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_tideglass_cutter`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_tideglass_cutter.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_model_windward_pinnace`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_model_windward_pinnace.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_a06_ui_profiles`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_d15_a06_ui_profiles.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 现有复核入口：`game/Assets/Content/D15/A06; docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_item_prop_placeholder`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_d15_item_prop_placeholder.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Prefabs/D15/d15_item_prop_placeholder.prefab; game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md`
- 现有复核入口：`game/Assets/Prefabs/D15/d15_item_prop_placeholder.prefab; game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_d15_outfit_placeholder`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_d15_outfit_placeholder.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Prefabs/Placeholder/ph_crew_male.prefab; game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md`
- 现有复核入口：`game/Assets/Prefabs/Placeholder/ph_crew_male.prefab; game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_ph_crew_male`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_ph_crew_male.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_ph_palm_leaves`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_ph_palm_leaves.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_ph_ship_fishing`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Content/D15/A06/entries/assets/asset_ph_ship_fishing.json`
- 当前标签：`inferred`；当前资产状态：`integrated`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_privateer_letter_of_marque`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_privateer_letter_of_marque.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md`
- 现有复核入口：`docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_coastal`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_coastal.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_crop`

- 批次：`D16-A04-007`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_crop.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_epiphyte`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_epiphyte.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_fern`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_fern.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_fruit_tree`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_fruit_tree.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_herb`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_herb.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_medicinal`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_medicinal.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_moss`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_moss.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_shrub`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_shrub.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_spice`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_spice.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_tree`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_tree.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_variant`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_variant.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `asset_vegetation_profile_wetland`

- 批次：`D16-A04-008`；范围：`asset_history_and_authorization`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/asset/asset_vegetation_profile_wetland.json`
- 当前标签：`inferred`；当前资产状态：`placeholder`
- 现有来源入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 现有复核入口：`game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md; game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence; rights_holder_and_license; commercial_use_and_modification`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| rights_holder_and_license | `` |
| commercial_use_and_modification | `` |
| notes | `` |

---

### `animal_domestic_cattle`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/animal/animal_domestic_cattle.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/animals/01_ANIMAL_OVERVIEW.md; docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 现有复核入口：`docs/game/design/animals/01_ANIMAL_OVERVIEW.md; docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `animal_domestic_goat`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/animal/animal_domestic_goat.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/animals/01_ANIMAL_OVERVIEW.md; docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 现有复核入口：`docs/game/design/animals/01_ANIMAL_OVERVIEW.md; docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `animal_domestic_pig`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/animal/animal_domestic_pig.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/animals/01_ANIMAL_OVERVIEW.md; docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 现有复核入口：`docs/game/design/animals/01_ANIMAL_OVERVIEW.md; docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_cassava_flour`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_cassava_flour.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_cattle_leather`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_cattle_leather.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 现有复核入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_chart_compass`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_chart_compass.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D9_TREASURE_VERTICAL_SLICE.md`
- 现有复核入口：`docs/game/design/common/D9_TREASURE_VERTICAL_SLICE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_chicken_egg`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_chicken_egg.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 现有复核入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_cocoa_cured`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_cocoa_cured.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_coffee_roasted`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_coffee_roasted.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_conch`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_conch.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_cooked_mackerel`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_cooked_mackerel.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/gameplay/03_FISHING_WHALING_AQUACULTURE.md; docs/game/design/items/01_ITEM_OVERVIEW.md`
- 现有复核入口：`docs/game/design/gameplay/03_FISHING_WHALING_AQUACULTURE.md; docs/game/design/items/01_ITEM_OVERVIEW.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_cured_shellfish`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_cured_shellfish.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_dried_mackerel`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_dried_mackerel.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_dried_sardine`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_dried_sardine.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_dried_seaweed`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_dried_seaweed.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_dried_snapper`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_dried_snapper.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_dye_indigo`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_dye_indigo.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 现有复核入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_farm_pest_treatment`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_farm_pest_treatment.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_fiber_agave`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_fiber_agave.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 现有复核入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_firearm_matchlock_carbine`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_firearm_matchlock_carbine.json`
- 当前标签：`historical`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`
- 现有复核入口：`docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_fish_oil`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_fish_oil.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_fresh_mackerel`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_fresh_mackerel.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/gameplay/03_FISHING_WHALING_AQUACULTURE.md; docs/game/design/items/01_ITEM_OVERVIEW.md`
- 现有复核入口：`docs/game/design/gameplay/03_FISHING_WHALING_AQUACULTURE.md; docs/game/design/items/01_ITEM_OVERVIEW.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_fresh_sardine`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_fresh_sardine.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_fresh_snapper`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_fresh_snapper.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_fruit_guava`

- 批次：`D16-A04-009`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_fruit_guava.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 现有复核入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_gear_cast_net`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_gear_cast_net.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_gear_fish_trap`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_gear_fish_trap.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_gear_handline_basic`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_gear_handline_basic.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/gameplay/03_FISHING_WHALING_AQUACULTURE.md; docs/game/design/common/D9_VERTICAL_SLICE_FREEZE.md`
- 现有复核入口：`docs/game/design/gameplay/03_FISHING_WHALING_AQUACULTURE.md; docs/game/design/common/D9_VERTICAL_SLICE_FREEZE.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_gear_seaweed_hook`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_gear_seaweed_hook.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_gear_shellfish_rake`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_gear_shellfish_rake.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_gear_shore_rod`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_gear_shore_rod.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_goat_milk`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_goat_milk.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 现有复核入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_guava_preserve`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_guava_preserve.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_herb_prepared`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_herb_prepared.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_livestock_feed`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_livestock_feed.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 现有复核入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_livestock_medicine`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_livestock_medicine.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 现有复核入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_pig_meat`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_pig_meat.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 现有复核入口：`docs/game/design/common/D13_RANCH_LIVESTOCK.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_resin_pine`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_resin_pine.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 现有复核入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_salted_cod`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_salted_cod.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/items/01_ITEM_OVERVIEW.md; docs/game/design/economy/01_ECONOMY_OVERVIEW.md`
- 现有复核入口：`docs/game/design/items/01_ITEM_OVERVIEW.md; docs/game/design/economy/01_ECONOMY_OVERVIEW.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_salted_mackerel`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_salted_mackerel.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/gameplay/03_FISHING_WHALING_AQUACULTURE.md; docs/game/design/economy/01_ECONOMY_OVERVIEW.md`
- 现有复核入口：`docs/game/design/gameplay/03_FISHING_WHALING_AQUACULTURE.md; docs/game/design/economy/01_ECONOMY_OVERVIEW.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_salted_sardine`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_salted_sardine.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_salted_snapper`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_salted_snapper.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_seaweed`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_seaweed.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 现有复核入口：`docs/game/design/common/D11_FISHING_HARVESTING.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_seed_cassava`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_seed_cassava.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_seed_cocoa`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_seed_cocoa.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_seed_coffee`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_seed_coffee.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_seed_guava`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_seed_guava.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_seed_herb`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_seed_herb.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_seed_spice`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_seed_spice.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_seed_sugarcane`

- 批次：`D16-A04-010`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_seed_sugarcane.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_seed_tobacco`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_seed_tobacco.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_signal_lantern_amber`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_signal_lantern_amber.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`
- 现有复核入口：`docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_special_weapon_royal_gun_carriage`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_special_weapon_royal_gun_carriage.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`
- 现有复核入口：`docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_spices_dried`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_spices_dried.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_tobacco_cured`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_tobacco_cured.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 现有复核入口：`docs/game/design/common/D13_FARMING_ORCHARDS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_tool_rescue_grapnel`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_tool_rescue_grapnel.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`
- 现有复核入口：`docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_tool_vegetation_axe`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_tool_vegetation_axe.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 现有复核入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_tool_vegetation_basket`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_tool_vegetation_basket.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 现有复核入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_tool_vegetation_harvesting_knife`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_tool_vegetation_harvesting_knife.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 现有复核入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_tool_vegetation_herb_knife`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_tool_vegetation_herb_knife.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 现有复核入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_tool_vegetation_machete`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_tool_vegetation_machete.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 现有复核入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_tool_vegetation_resin_scraper`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_tool_vegetation_resin_scraper.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 现有复核入口：`docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_toxin_datura`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_toxin_datura.json`
- 当前标签：`historical`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/02_VEGETATION_CATALOG.md; docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 现有复核入口：`docs/game/design/vegetation/02_VEGETATION_CATALOG.md; docs/game/design/vegetation/05_VEGETATION_INTERACTION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `item_vegetable_cassava`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/item/item_vegetable_cassava.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/vegetation/02_VEGETATION_CATALOG.md`
- 现有复核入口：`docs/game/design/vegetation/02_VEGETATION_CATALOG.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_accessory_compass_badge`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_accessory_compass_badge.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 现有复核入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_body_sun_bronzed`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_body_sun_bronzed.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 现有复核入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_bottom_canvas_trousers`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_bottom_canvas_trousers.json`
- 当前标签：`historical`；当前资产状态：``
- 现有来源入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 现有复核入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_career_fisher_handline`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_career_fisher_handline.json`
- 当前标签：`historical`；当前资产状态：``
- 现有来源入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md; docs/game/design/common/D8_CONTENT_ID_REGISTRY.md`
- 现有复核入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md; docs/game/design/common/D8_CONTENT_ID_REGISTRY.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_ceremonial_silk_coat`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_ceremonial_silk_coat.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 现有复核入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_coat_rain_oilskin`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_coat_rain_oilskin.json`
- 当前标签：`historical`；当前资产状态：``
- 现有来源入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 现有复核入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_gloves_ropework`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_gloves_ropework.json`
- 当前标签：`historical`；当前资产状态：``
- 现有来源入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 现有复核入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_hair_short_brown`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_hair_short_brown.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 现有复核入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_hat_straw_deck`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_hat_straw_deck.json`
- 当前标签：`historical`；当前资产状态：``
- 现有来源入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 现有复核入口：`docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_injury_bandaged_arm`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_injury_bandaged_arm.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 现有复核入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_injury_old_leg`

- 批次：`D16-A04-011`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_injury_old_leg.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 现有复核入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_shoes_deck_boots`

- 批次：`D16-A04-012`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_shoes_deck_boots.json`
- 当前标签：`historical`；当前资产状态：``
- 现有来源入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 现有复核入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_top_linen_deckhand`

- 批次：`D16-A04-012`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_top_linen_deckhand.json`
- 当前标签：`historical`；当前资产状态：``
- 现有来源入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 现有复核入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `outfit_weathered_deckhand`

- 批次：`D16-A04-012`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/outfit/outfit_weathered_deckhand.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md; docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 现有复核入口：`docs/game/design/character/06_CHARACTER_PRESENTATION.md; docs/game/design/gameplay/09_CHARACTER_CUSTOMIZATION.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `reward_d14_professional_rescue`

- 批次：`D16-A04-012`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/reward/reward_d14_professional_rescue.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D14_REWARD_SYSTEM.md`
- 现有复核入口：`docs/game/design/common/D14_REWARD_SYSTEM.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `reward_d14_rare_indigo_dye`

- 批次：`D16-A04-012`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/reward/reward_d14_rare_indigo_dye.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D14_REWARD_SYSTEM.md; docs/game/design/common/D12_COLLECTION_ATLAS.md`
- 现有复核入口：`docs/game/design/common/D14_REWARD_SYSTEM.md; docs/game/design/common/D12_COLLECTION_ATLAS.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---

### `reward_d14_worldclass_reinforced_keel`

- 批次：`D16-A04-012`；范围：`historical_content`
- Registry 文件：`game/Assets/Resources/ContentRegistry/entries/reward/reward_d14_worldclass_reinforced_keel.json`
- 当前标签：`inferred`；当前资产状态：``
- 现有来源入口：`docs/game/design/common/D14_REWARD_SYSTEM.md; docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`
- 现有复核入口：`docs/game/design/common/D14_REWARD_SYSTEM.md; docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`
- 决定证据状态：`has_existing_local_file_reference`
- 必须决定：`historical_tag_and_evidence`

| 字段 | 待填写值 |
|---|---|
| decision | `Pending` |
| reviewer | `` |
| review_date | `` |
| decision_basis | `` |
| additional_evidence_refs | `` |
| notes | `` |

---
