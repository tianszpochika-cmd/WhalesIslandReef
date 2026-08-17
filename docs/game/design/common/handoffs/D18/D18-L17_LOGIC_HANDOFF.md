# D18-L17 交接报告

## 任务信息与结论

- 任务编号：`D18-L17`（波次 1，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L17/D18_L17_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_candidate_runtime_guard.py`（新建，只读静态扫描工具）
- `tools/tests/test_d18_candidate_runtime_guard.py`（新建，17 个正负单测）
- `game/tools/results/D18/D18-L17/D18_L17_REAL_SCAN.json`（真实仓库扫描结果）
- `game/tools/results/D18/D18-L17/D18_L17_LOGIC_RESULT.json`（本任务结果）
- `docs/game/design/common/handoffs/D18/D18-L17_LOGIC_HANDOFF.md`（本文件）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 game 代码、catalog、资源、Unity、场景、Prefab、Registry、Systems、contracts、server、web；未把任何现有候选升级为正式。

## 工具行为（冻结）

**默认扫描根**：`game/Assets/Scripts`、`game/Assets/Content`、`game/Assets/Settings`、`game/Assets/Resources`（.cs/.json；默认排除 Candidates/obj/.git/Library 目录）。

**规则**（每条发现含 file、line、rule、severity、classification、snippet、fix_direction）：

| 规则 | 语义 | 运行时严重度 |
|---|---|---|
| `candidate_path_reference` | 引用 `Art/Candidates` 路径（含反斜杠、大小写变体） | P0 |
| `temp_cache_reference` | 引用临时下载缓存（`D18/cache`、`WhalesIslandReef/D18`） | P0 |
| `external_archive_reference` | 引用 `external_sources` 或含路径分隔符的压缩包路径 | P0 |
| `candidate_state_runtime_reference` | 行内 Candidate 关键字 + 运行时消费关键字；或 JSON 结构检查：Candidate 状态声明任何运行时路径 / placeholder 状态声明非占位路径 | P1 |

**分类**（`classification`）：`comment_or_documentation`（注释/文档，INFO）、`editor_only`（Editor 目录或 `#if UNITY_EDITOR` 块，INFO）、`runtime`（P0/P1）。仅 runtime 发现使状态 BLOCKED；工具只读，绝不自动替换或删除引用。

**JSON 结构检查**：解析 Registry/配置 JSON，逐对象检查 `asset_state`/`state`；占位引用白名单 = `ph_`/`asset_ph_`/`placeholder_` 前缀、`unity-primitive://` URI、路径名含 `placeholder`（如 `game/Assets/Prefabs/Placeholder/...`、`d15_item_prop_placeholder.prefab`）。

## 真实仓库扫描结论（2026-08-16）

- 扫描 3,417 个文件，**status=PASS（exit 0）**：运行时违规 **0 条**。
- 发现 4 条 Candidates 引用，全部在 `game/Assets/Scripts/Presentation/D20/PetRarePresentation/Editor/D20A06PrefabBuilder.cs`（行 87/98/109/120），**正确分类为 `editor_only`（INFO）**：Editor 受控工作场景装配允许引用 intake-eligible 候选，产物仍需未来 Runtime 晋级门禁。
- Registry（`game/Assets/Resources/ContentRegistry`）2,264 条 `placeholder` 状态条目引用 `unity-primitive://` 与 `ph_`/`placeholder` 命名占位——合法占位，无误报；无真实 Candidate 状态条目。
- `GpuCounterCandidates` 等仅含 "candidates" 的标识符无误报（词边界规则）。

## 验证命令与结果

| 命令 | 结果 |
|---|---|
| `python -m unittest tools.tests.test_d18_candidate_runtime_guard` | PASS：17/17 |
| `python tools/d18_candidate_runtime_guard.py --output .../D18_L17_REAL_SCAN.json` | PASS：exit 0，0 运行时发现，4 editor_only |
| `python -m py_compile tools/d18_candidate_runtime_guard.py` | PASS |
| 全量 `python -m unittest discover -s tools/tests -p "test*.py"` | 417 测试，仅既有 4 个 SUPERSEDED 失败（test_d18_s02_r01_intake_refresh R01 断言，基线 1.3 节），无新增失败 |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |
| `python tools/sync_logic_task_status.py` | PASS：D18-L17=LOGIC_PASS |

## Runtime 消费风险（如实登记）

- 静态扫描不能证明运行时零引用（动态拼装路径、Unity GUID/.meta 引用需 L30/L44 与 Runtime 阶段）。
- Editor 构建器（D20A06PrefabBuilder.cs）引用的候选模型当前为受控装配，正式消费必须走未来 Runtime 晋级门禁。
- Registry 占位（2,264）与项目自有资产引用（324 条，`game/Assets/Content/D15/A06/...` 项目自建几何）为当前合法状态；升级需 L35 迁移审计。

## 待办 / 后续注意事项

- L39 聚合审计可调用本工具（read-only，输出机器可读 JSON）。
- L44 游戏引用审计与本工具互补（本工具查 Candidates 路径/状态，L44 查稳定 key 使用）。
- 若未来新增资源目录形态（如正式 Art 目录变体），需同步更新本工具规则（L45 兼容性审计核对）。
