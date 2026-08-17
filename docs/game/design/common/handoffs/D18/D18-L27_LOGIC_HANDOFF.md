# D18-L27 交接报告

## 任务信息与结论

- 任务编号：`D18-L27`（波次 2B；前置 D18-L25=LOGIC_PASS 已满足）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**（此前因 L25 未落盘为 LOGIC_BLOCKED；现 L25 已合并，本报告按原 handoff 待办覆盖该 BLOCKED 结果，task_id 不变）
- 机器结果：`game/tools/results/D18/D18-L27/D18_L27_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_material_descriptor_validator.py`（新建，只读验证器）
- `tools/tests/test_d18_material_descriptor_validator.py`（新建，27 个单测）
- `game/tools/results/D18/D18-L27/D18_L27_LOGIC_RESULT.json`（覆盖本任务 BLOCKED 结果，task_id 不变）
- `docs/game/design/common/handoffs/D18/D18-L27_LOGIC_HANDOFF.md`（本文件，覆盖 BLOCKED 版本）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema、Unity、资源、场景、Prefab、Registry、contracts、server、web 或其它 L 任务文件；未创建 Material/Shader/TextureImporter，未做像素级图像检查。

## 字段词汇（与 L25 对齐）

验证器输入为 `d18_material_descriptor` 记录，字段词汇采用已合并的 `d18_import_descriptor.schema.json` 语义：`source_asset_id`（引用 L02 记录）、`category`、`maps[]`（map_name/map_role/channels/color_space_intent/source_path）、`pbr_set_complete`、`license_dependency`（license_id/license_url/license_evidence_path/attribution_required）、`rights`（四项）、`fallback`（reference/description）、`runtime_deferred`、`represents_imported_asset`（恒 false）。

## 拒绝规则（每条含稳定 code + field + message）

| 规则 | 校验 | 拒绝条件 |
|---|---|---|
| R1 关联 asset_id | `^asset_[a-z0-9_]+$`；提供 records 目录时核对存在 | 非 asset_ 前缀、磁盘路径/显示名、指向不存在记录 |
| R2 类别 | 命中 L14 材质域词典 material/texture/hdri/font | 词典外类别 |
| R3 类别↔map | PBR 角色仅限 material；texture 不得带 PBR 角色；hdri 必须恰好 1 张且仅 hdri；font 不得有 map | 类别与 map 类型矛盾 |
| R4 map 角色 | 冻结角色词汇；每角色至多一次 | 未知角色、重复角色 |
| R5 通道语义 | albedo→rgb、normal→xyz/xy、roughness/ao/metalness/height→r、opacity→a、hdri→rgb 等 | 角色与通道不匹配、缺通道 |
| R6 颜色空间 | 每 map 必须声明 color_space_intent ∈ {srgb, linear}（仅意图） | 缺意图、非法取值 |
| R7 PBR 完整性 | pbr_set_complete=true 必须含 albedo+normal+roughness+ao | 缺任一（AO 缺失不得伪装完整） |
| R8 许可依赖 | L04 词汇、NC/ND/SA 阻断、证据文件存在且仓库内、四项权利显式 | 缺失/未知/NC/ND/SA、证据缺失或越界、权利不全 |
| R9 回退 | 非空 reference（asset_*/placeholder_*）+ description | 空回退、缺回退、回退指向候选二进制 |
| R10 runtime_deferred | 非空字符串数组 | 缺失、空、含空串 |
| R11 未导入 | represents_imported_asset 恒 false | 任何 true 值 |

## 验证结果

| 验证 | 命令 | 结果 |
|---|---|---|
| 单测 | `python -m unittest tools.tests.test_d18_material_descriptor_validator` | 27/27 PASS |
| 语法 | `python -m py_compile tools/d18_material_descriptor_validator.py` | PASS |
| 全量回归 | `python -m unittest discover -s tools/tests -p "test*.py"` | 844 测试，仅 4 个既有 SUPERSEDED 失败 |
| diff | `git diff --check` | PASS（仅既有 LF→CRLF 警告） |
| 状态同步 | `python tools/sync_logic_task_status.py` | PASS：D18-L27=LOGIC_PASS |

开发中修复记录：首轮发现 hdri 规则缺口（仅校验 hdri 角色数量，未拒绝 hdri 描述器携带非 hdri map），在唯一写入范围内修复为「hdri 必须恰好 1 张且仅 hdri」并复验通过。

## 明确未宣称

- 不宣称任何材质/贴图可运行时使用；未创建任何 Material/Shader/TextureImporter 或导入设置。
- 实际图像检查（像素/通道/尺寸）与颜色空间/压缩接线全部延期 Runtime（L14 第 8 节）。
- `RuntimeApproved/ReleaseApproved` 持有者仍为 0。

## 待办 / 后续注意事项

- L42 编译器生成的 material/texture/hdri/font 描述器以本验证器为准；L45 一致性审计核对本工具与 L25 Schema 词汇。
- L26（mesh）/L28（animation）描述器验证器与 L25 对齐后可解除阻塞（当前 BLOCKED）；L29（ui_audio）已 LOGIC_PASS。
- 真实 intake 材料（polyhaven_selected_assets.json、ui_materials_audio manifests）只作只读数据源；单项许可/哈希由 L21—L24/L06 验证。
