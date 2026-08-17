# D18-L15 交接报告

## 任务信息与结论

- 任务编号：`D18-L15`（波次 1，并行；与 L02—L14、L16—L20 并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L15/D18_L15_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_content_id_asset_link_validator.py`（新建，验证器本体）
- `tools/tests/test_d18_content_id_asset_link_validator.py`（新建，25 项单测）
- `game/tools/results/D18/D18-L15/D18_L15_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L15_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何其他 D18 工具、Schema、真实 catalog（ContentRegistry）、Unity、资源、Registry、contracts、server、web 或旧 A/S/R 历史文件；未创建任何私有 ID；未导入任何资源；未授予任何状态。

## 工具行为（`tools/d18_content_id_asset_link_validator.py`）

对每个 D18 逻辑资源记录的 `content_ids`：

| 检查 | 错误码 | 语义 |
|---|---|---|
| 字段存在/为数组/非空 | missing_content_ids / invalid_content_ids / empty_content_ids | 记录形态 |
| 单个取值合法（非空字符串、lower snake_case） | invalid_content_id / path_like_content_id | 候选路径、展示名、绝对路径（含斜杠/点/空格）一律拦截 |
| 禁止候选派生键 | candidate_key_leak | `candidate_` 前缀是准入目录的候选映射键，不得作业务 content_id |
| 前缀已注册 | unknown_prefix | 默认 D8 13 前缀（registry.json，2026-08-10 冻结），可用 `--registry` 显式替换/扩展 |
| 类型匹配 | type_mismatch | 前缀注册类型 vs 目录定义声明的 `category`（intake 风格目录只声明 domain 时不误报） |
| 已弃用 | deprecated_content_id | 由 `--deprecated` 提供（list 或 {id: replacement}），带 replacement |
| 候选路径/展示名精确泄漏 | candidate_path_leak | 由 `--candidates` 清单精确匹配 |
| 存在性 | missing_content_id | 必须存在于 `--catalog` 提供的定义源（D8 条目 / entries / eligible_entries 三种形态） |
| 记录内重复 | duplicate_content_id | 同一记录数组内重复 |
| 跨记录重复 | duplicate_content_id_across_records | 同一 content_id 被多个记录映射 |
| 无 catalog | catalog_missing（PENDING） | 存在性无法验证 → PENDING，绝不通过 |

- 只读：`approved_count` 恒为 0；不改 catalog、不创建私有 ID、不导入资源；`read_only: true`；
- 结果结构沿用 L06 约定：`status` PASS/PENDING/BLOCKED（退出码 0/1/2）、`issue_counts`、`issues`（record_id/content_id/code/severity/message）、`records` 明细。

## 真实仓库只读冒烟（临时文件，不落仓库）

- 目录源：真实 ContentRegistry 全部条目合并（2,812 条，临时目录生成），前缀表：真实 `game/Assets/Resources/ContentRegistry/registry.json`；
- 合法记录（outfit_hair_short_brown / asset_ph_crew_male / ship_moonwake / crew_isaac_reed / item_dye_indigo）→ `PASS`、2/2 verified、approved=0；
- 非法记录（`candidate_hair_buzzed_r03p`、`Assets/Art/Candidates/Kenney/a.fbx`、`outfit_hair_missing_x`）→ `BLOCKED`（exit 2），`candidate_key_leak`/`path_like_content_id`/`missing_content_id` 各 1 条，逐条含原因。

## 测试（25/25 PASS）

覆盖：缺 ID、空/非数组、非字符串取值、路径/展示名/绝对路径（path_like）、candidate_ 键泄漏、candidates 清单精确泄漏、记录内重复、跨记录重复、目录不存在（missing）、未知前缀、类型不匹配、已弃用（含/不含 replacement）、无 catalog 为 PENDING 且不通过、intake 风格目录无误报、registry 覆盖、非法记录、空记录、只读性（目录字节不变）、加载器正反例、D8 13 前缀冻结。

## 明确未宣称

- 未宣称任何真实逻辑记录已通过全量校验（真实记录由 L21—L24/L42 产出后，在波次 3 由 L39 聚合审计执行）；
- 未修改 ContentRegistry 或任何 catalog；未新建前缀/ID；
- 全量工具套件中 7 项失败均位于本任务写入范围之外：4 项 `test_d18_s02_r01_intake_refresh` 为基线 1.3 声明的 SUPERSEDED 断言（非 L 门禁）；3 项 `test_d18_candidate_runtime_guard` 属 L17 并行写入范围（并行 Agent 正在实时修改套件，多次运行失败集有变化）；本任务未改动任何他人测试。

## 待办 / 后续注意事项

- L16（ResourceKey 验证器）可复用本工具的 `content_ids` 禁路径口径；L39 聚合审计把本工具纳入波次 3 全量运行；
- 如未来需要让 intake 级 `candidate_`/`env_`/`d18a05_`/`material_` 前缀进入业务 content_ids，必须经 `--registry` 前缀表显式声明并由 L45 一致性审计核对，默认保持 D8 13 前缀。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python -m unittest tools.tests.test_d18_content_id_asset_link_validator -v` | PASS：25/25 |
| 真实目录+真实 registry 冒烟（合法记录，`--output` 到临时目录） | PASS：status=PASS，verified=2/2，approved=0，catalog_entry_count=2812 |
| 真实目录+真实 registry 冒烟（非法记录） | PASS：status=BLOCKED（exit 2），issue_counts={candidate_key_leak:1, path_like_content_id:1, missing_content_id:1} |
| `python -m unittest discover -s tools/tests -p 'test_d18_*.py'` | 本模块 25/25；套件其余失败均属范围外（SUPERSEDED 4 项 + L17 并行 3 项），未修改 |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |
| `python tools/sync_logic_task_status.py` | PASS：总览重新生成，D18-L15=LOGIC_PASS |
