# D18-L23 交接报告

## 任务信息与结论

- 任务编号：`D18-L23`（波次 2，并行；波次 1 L01—L20 全部 LOGIC_PASS 后启动）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L23/D18_L23_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `docs/game/design/common/assets/sources/d18_polyhaven_logic_catalog.json`（新建，91 条 Poly Haven 单项静态记录）
- `game/tools/results/D18/D18-L23/D18_L23_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L23_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何真实 manifest、Schema、catalog、Unity、Prefab、场景、资源、Registry、Systems、contracts、server、web 或旧 D18-A/S/R 历史文件；无网络访问；不创建材质、不接 Shader、不导入。

## 目录结构与记录口径

`d18_polyhaven_logic_catalog.json` = `{catalog_kind, catalog_task, schema_version, source_id, source_name, official_license_page, site_license_statement, site_license_is_not_item_evidence, item_evidence_rule, models_policy, created_at, record_count, summary, records[]}`；`records` 为 L02 Schema 合法记录（`{"records": [...]}` 形状可直接被 L05/L06/L15/L16 工具读取）。

| 组 | 记录数 | canonical_asset_id 模式 | sha256 | 单项来源页 |
|---|---|---|---|---|
| 选中材质 map（D18-A06 选集） | 79 | `asset_material_<slug>_<map>`（albedo/ao/normal/roughness；fabric_pattern_05 无 albedo，实为 3 map） | 真实逐文件（polyhaven_selected_assets.json） | `https://polyhaven.com/a/<slug>` |
| 环境材质 map（sand_01 / coral_mud_01） | 8 | `asset_material_sand_01_{diff,nor_gl,rough,ao}` 等 | 真实逐文件（各自 manifest） | `https://polyhaven.com/a/sand_01`、`/a/coral_mud_01` |
| HDRI | 4 | `asset_hdri_<slug>` | **null（Pending）** | `https://polyhaven.com/a/<slug>` |

汇总：91 条记录；87 条带真实 sha256；4 条 pending sha256；state 全部 `Candidate`；license 全部 `CC0-1.0`（evidence：`polyhaven_cc0_evidence_2026-08-14.txt` / `packs/environment/licenses/polyhaven_license_evidence_2026-08-14.md`）；`attribution_required=false` 91/91；**models=0**（未下载任何 Poly Haven 模型，不为不存在或无单项证据的模型建记录）。

## 关键口径（冻结于本任务）

1. **站点许可 ≠ 单项证据**：`site_license_is_not_item_evidence=true`；每条记录由 official_page + 许可证据文件 + manifest + 逐文件哈希（或明确 null）解释。
2. **缺项 Pending**：4 条 HDRI 的逐文件 SHA-256 在 `polyhaven_selected_assets.json` 中无 files 条目 → `sha256=null` + runtime_deferred 说明，绝不补猜、绝不把站点 CC0 声明当作哈希证据。
3. **未来用途标注，不创建资源**：每条材质记录 runtime_deferred 标注 D18-A06 选集的用途组（木材/甲板、沙地/泥地/水岸、岩石/碎石、布/皮革、金属）与环境 manifest 角色（beach_material*/seabed_material*）；不创建 Unity Material、不接 Shader、不导入（L14 第 7 节红线）。
4. **状态**：全部 `Candidate`（来源目录任务登记）；IntakeEligible 只能由 L42 编译器在 L04—L10/L15/L16 全部通过后授予；本任务不授予任何状态。

## 验证结果汇总（与结果 JSON 一致）

| 验证器 | 输入 | 结果 |
|---|---|---|
| L02 Schema（jsonschema Draft202012Validator） | 91 条记录 | 91/91 合法，0 错误 |
| L04 许可规范化（d18_license_normalizer.py 派生视图） | 91 条 | 91/91 NORMALIZED，0 错误 |
| L05 来源验证（validate_source_record 逐条，as-of 2026-08-16） | 91 条 | 91/91 PASS |
| L06 哈希验证（派生逐文件记录） | 87 条带哈希 | 87/87 真实匹配 PASS；全量 91 → 4 条 missing_sha256（HDRI 缺证据，approved_count=0） |
| L07 权利验证（派生视图） | 91 条 | 91/91 四项权利显式 allowed，migration_allowed=true |
| L09 署名验证（validate_attribution 直输） | 91 条 | 91/91 valid，无伪造必须署名 |
| L08 路径边界 CLI（--kind evidence） | 93 条唯一路径 | exit 0，全部 PASS |
| L15 content_id 关联 | catalog + intake + registry | formal ids 未登记 → missing_content_id 91（BLOCKED；真实缺口，登记 Pending） |
| L16 ResourceKey | asset_ 形式 91 条 | CLEARED（0 issues） |
| L16 ResourceKey | L14 d18: 命名空间形式 91 条 | BLOCKED（unknown_namespace 91；L16 冻结命名空间 d10/d20） |
| L18 发布声称守卫 | 全仓库扫描 | 本次零新增（仅既有 D18-L18 自身 handoff 1 处自引用误报） |
| `python tools/sync_logic_task_status.py` | — | PASS，D18-L23=LOGIC_PASS |
| `git diff --check` | — | PASS，exit 0（仅既有 LF→CRLF 警告） |

## 明确未宣称

- 没有任何 Poly Haven 资源被写成“可运行时使用”“环境美术完成”“RuntimeApproved”或“ReleaseApproved”；
- 没有创建材质实例、没有接 Shader/管线、没有接线天空盒/光照组件（L14 红线）；
- 没有把站点许可页当作单项哈希/来源记录；没有为未下载的模型建记录；
- 没有把“免费/可下载/CC0 站点声明”扩大为超出 manifest 记录的许可结论。

## 待办 / 后续注意事项

- **HDRI 逐文件哈希**（4 条）：由 L06/Runtime 对真实 .hdr 文件计算后补录，补齐前 sha256 保持 null。
- **formal content_ids 登记**：`asset_material_*/asset_hdri_*` 需在 ContentRegistry 登记（D8 前缀 asset_ 已存在，L42/D8 内容任务负责），登记后 L15 复跑转 CLEARED。
- **L14 vs L16 命名空间差异**：L14 冻结 `d18:<分类键>:<content_id>` 资源键格式，L16 冻结命名空间 d10/d20 → 交由 L45 一致性核对裁定，本任务不越权修改任一文件。
- 目录可直接作为 L42 编译器输入（`records` 数组）；L33（目录声称审计）可用本目录与准入目录 91 条 polyhaven 条目 1:1 对账。
- 并行注意：L21/L22/L24 各自来源目录与本目录并存于 `assets/sources/`，互不覆盖。

## 附录：真实数据来源（只读引用，SUPERSEDED 历史材料不作通过依据）

- `docs/game/design/common/assets/packs/ui_materials_audio/polyhaven_selected_assets.json`（20 materials/79 maps + 4 HDRIs 的逐文件 URL/哈希/尺寸/通道）
- `docs/game/design/common/assets/packs/ui_materials_audio/polyhaven_ui_materials_selected_v1.manifest.json`、`polyhaven_hdri_sky_references_v1.manifest.json`
- `docs/game/design/common/assets/packs/environment/polyhaven_*.manifest.json`（8 个，含逐文件 sha256）
- 许可证据：`polyhaven_cc0_evidence_2026-08-14.txt`、`packs/environment/licenses/polyhaven_license_evidence_2026-08-14.md`
- 分类词典（只读）：`D18_TAXONOMY_UI_AUDIO.md`（L14，material/hdri 分类、content_id/ResourceKey 规则）
