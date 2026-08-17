# D18-L25 未来导入描述夹具（fixtures）

本目录是 `tools/schemas/d18_import_descriptor.schema.json` 的固定校验夹具（D18-L25 唯一写入范围）。

## 声明

- 全部夹具为**合成数据**：`source_asset_id` 一律以 `asset_fixture_` 开头、来源为 `example.invalid` 保留域；**不对任何真实候选补造数据**，不宣称任何真实资产状态，不表示任何资源已导入。
- 本 Schema 不是 Unity Import Settings：不含任何 importer 选项字段；`represents_imported_asset` 恒为 `false`（任何 true 都非法）。
- 校验方式与仓库既有用法一致：Python `jsonschema.Draft202012Validator`（同 D18-L02 夹具）。

## 合法夹具（VALID，覆盖分类差异）

| 文件 | 分类 | 描述差异要点 |
|---|---|---|
| `valid_character.json` | character | humanoid 骨架意图、动画列表（idle/walk/run/work）、root_motion=derived、capsule_agent 碰撞意图、body/outfit 材质槽、LOD 2 级 |
| `valid_ship.json` | ship | static 骨架意图、非空 bounds、convex_mesh 碰撞意图、hull/sail 材质槽、LOD 3 级、空动画列表 |
| `valid_environment.json` | environment | none 骨架、terrain 碰撞意图、ground/vegetation 材质槽、LOD 3 级非自动生成 |
| `valid_ui.json` | ui | 无几何（bounds=null、LOD=null）、ui 材质槽、collision/root_motion=none |
| `valid_audio.json` | audio | 全几何字段 null/空、material_slots 空、skeleton=none |
| `valid_animal.json` | animal | animal 骨架意图、walk/run 动画（root_motion=true）、capsule_agent |
| `valid_fish.json` | fish | fish 骨架意图、swim 动画、collision=none |
| `valid_prop.json` | prop | none 骨架、simple_box、wood/metal 材质槽 |
| `valid_material.json` | material | 非几何组（bounds=null、skeleton=none、LOD=null） |

## 非法夹具（INVALID，每项只偏离一个维度）

| 文件 | 失败原因 |
|---|---|
| `invalid_claims_imported.json` | `represents_imported_asset=true`（本 Schema 禁止表示已导入） |
| `invalid_unity_import_settings_field.json` | 混入 Unity importer 字段 `import_settings`（additionalProperties=false） |
| `invalid_missing_source_asset.json` | 缺 `source_asset_id`（required） |
| `invalid_source_asset_not_asset_id.json` | `source_asset_id` 是磁盘路径而非 `asset_*` 格式 |
| `invalid_unknown_type.json` | `type="terrain"` 不在 11 值枚举 |
| `invalid_negative_bounds.json` | `bounds.height_m=-1`（必须为正数） |
| `invalid_audio_with_skeleton.json` | `type=audio` 但 `skeleton_type=humanoid`（非几何组条件约束） |
| `invalid_audio_with_bounds.json` | `type=audio` 但 `bounds` 非 null（非几何组条件约束） |
| `invalid_root_motion_bad_value.json` | `root_motion="always"` 不在枚举 |
| `invalid_material_slot_bad_role.json` | `slot_role="skin"` 不在材质槽角色枚举 |
| `invalid_collision_bad_value.json` | `collision_intent="mesh_collider"` 不在枚举 |
| `invalid_lod_levels_out_of_range.json` | `lod_intent.levels=0`（1—5 范围外） |
| `invalid_fallback_missing.json` | 缺 `fallback`（每次导入尝试必须带回退声明） |
| `invalid_license_evidence_escape.json` | `license_evidence_path` 为 Windows 绝对路径（路径越界） |
| `invalid_animation_bad_key.json` | `animation_key="Run Cycle"` 含空格大写（必须 `[a-z0-9_]+`） |
| `invalid_character_static_skeleton.json` | `type=character` 但 `skeleton_type=static`（角色必须有角色骨架意图） |
| `invalid_ship_null_bounds.json` | `type=ship` 但 `bounds=null`（几何类必须声明预期 bounds） |

## 校验命令（可复跑）

```powershell
python -c "import json;from jsonschema import Draft202012Validator;s=json.load(open('tools/schemas/d18_import_descriptor.schema.json',encoding='utf-8'));Draft202012Validator.check_schema(s);print('schema ok')"
python -c "import json,glob;from jsonschema import Draft202012Validator;s=json.load(open('tools/schemas/d18_import_descriptor.schema.json',encoding='utf-8'));v=Draft202012Validator(s);[print(p, 'VALID' if v.is_valid(json.load(open(p,encoding='utf-8'))) else 'INVALID') for p in sorted(glob.glob('tools/tests/d18_import_descriptor_fixtures/*.json'))]"
```
