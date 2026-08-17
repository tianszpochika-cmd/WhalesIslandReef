# D18-L25 交接报告

## 任务信息与结论

- 任务编号：`D18-L25`（波次 2，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L25/D18_L25_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/schemas/d18_import_descriptor.schema.json`（新建）
- `tools/tests/d18_import_descriptor_fixtures/`（新建：README + 9 个合法夹具 + 17 个非法夹具）
- `game/tools/results/D18/D18-L25/D18_L25_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L25_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何其他 Schema、manifest、catalog、Unity、Prefab、场景、资源、Registry、Systems、contracts、server、web、旧 D18-A/S/R 历史文件或并行任务产物。**无 Unity API 引用、无 Prefab/场景改动。**

## Schema 设计

`d18_import_descriptor.schema.json`（Draft 2020-12，与 L02 同风格）：

| 字段 | 语义 |
|---|---|
| `descriptor_kind` / `schema_version` | const 判别器 `d18_import_descriptor` / 1 |
| `source_asset_id` | 引用 L02 `canonical_asset_id`（`asset_*`，禁止磁盘路径） |
| `type` | L02 11 值枚举（character/ship/environment/ui/audio/animal/fish/prop/building/material/lighting_reference） |
| `units` / `axes` | 预期单位（meters 为目标约定）与 Y-up/Z-forward 轴向声明，未知须在 Runtime 导入前解决 |
| `bounds` | 预期 AABB（width/depth/height 米，正数）或 null；真实 bounds 由 L26 测量，不在此发明 |
| `material_slots` | 槽名 + 槽角色枚举（body/outfit/fabric/wood/metal/glass/emissive/water/stone/organic/ui/misc）+ 可选材质记录引用 |
| `skeleton_type` | none/static/generic/humanoid/animal/fish/mech 意图声明 |
| `animation_list` | 设计键列表（key + role：idle/walk/run/work/combat/sail/swim/fly/emote/interact/other + 可选 root-motion），不导入任何 clip |
| `root_motion` | none/required/derived/unknown 处理意图 |
| `collision_intent` | none/simple_box/convex_mesh/capsule_agent/terrain/interaction_anchor_only/unknown 意图声明，不产生碰撞体 |
| `lod_intent` | levels(1-5) + generate_automatically，或 null（非几何） |
| `license_dependency` | license_id + license_evidence_path（仓库相对路径）+ 可选 url/attribution，须镜像 L02 记录 |
| `fallback` | 必填：reference（`asset_*`/`placeholder_*`）+ description；每次导入尝试必须带回退声明 |
| `represents_imported_asset` | **const false**——本 Schema 永远不能表示资源已导入 |

**显式边界（Schema description 与结构双重声明）**：这不是 Unity Import Settings——无任何 importer 选项字段，`import_settings`/`mesh_compression` 等字段被 `additionalProperties=false` 直接拒绝；且任何"已导入"声称（`represents_imported_asset=true`）非法。

**条件约束（allOf，覆盖分类差异）**：

- 非几何组（audio/ui/material/lighting_reference）：bounds=null、skeleton_type=none、collision=none、root_motion=none、lod=null、animation 空数组；
- 几何组（character/animal/fish/ship/environment/prop/building）：bounds 必填非 null；
- character → skeleton ∈ {humanoid, generic}；animal → {animal, generic}；fish → {fish, generic}；ship/environment/prop/building → {none, static}。

## 夹具覆盖（9 合法 + 17 非法）

合法夹具逐类体现描述差异：character（humanoid+动画+derived+胶囊+body/outfit）、ship（static+convex_mesh+3 级 LOD+空动画）、environment（none+terrain）、ui（无几何+ui 材质槽）、audio（全 null/空）、animal（animal 骨架+run 动画）、fish（fish+swim）、prop（simple_box）、material（非几何组）。

非法夹具每项只偏离一个维度：声称已导入、混入 Unity import_settings、缺/坏 source_asset_id、未知 type、负 bounds、audio 带骨架、audio 带 bounds、坏 root_motion、坏槽角色、坏碰撞值、LOD levels=0、缺 fallback、证据路径越界（`C:/Windows/evil.txt`）、动画键含空格大写、character+static 骨架、ship+null bounds。

## 验证结果

| 命令 | 结果 |
|---|---|
| Schema `check_schema`（Draft202012Validator） | PASS：schema ok |
| 双向夹具校验（valid 必须 VALID、invalid 必须 INVALID） | PASS：9/9 + 17/17 零偏差；错误原因抽查正确 |
| `python -m unittest discover -s tools/tests -p 'test*.py'` | 417 项：413 通过、4 失败（全部为既有 SUPERSEDED `test_d18_s02_r01_intake_refresh.py` 历史断言，非 L 门禁） |
| `python tools/sync_logic_task_status.py` | PASS：D18-L25=LOGIC_PASS |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |

## 明确未宣称

- 不表示任何资源已导入、可运行、RuntimeApproved 或 ReleaseApproved（`represents_imported_asset` 恒 false）。
- 夹具全部为合成数据（`asset_fixture_*`/`example.invalid`），不补造真实数据。

## 待办 / 后续注意事项

- L26/L27/L28/L29 描述器验证器应以本 Schema 字段（bounds/skeleton/animation/collision/lod）为静态校验依据；L37 晋级申请 `descriptors` 引用本描述器。
- `axes`/`units`/`root_motion` 的 `unknown` 值必须由未来 Runtime 阶段在导入前解决，不得静默假设。
- L45 一致性审计将核对 `type` 与 L02 `category` 一致性。
