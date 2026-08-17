# D18-L08 交接报告

## 任务信息与结论

- 任务编号：`D18-L08`（波次 1，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L08/D18_L08_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_asset_path_validator.py`（新建，资源路径边界验证器，只读）
- `tools/tests/test_d18_asset_path_validator.py`（新建，38 项单测）
- `game/tools/results/D18/D18-L08/D18_L08_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L08_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改 `.gitignore`、真实资源目录、任何 Schema、Unity、Prefab、场景、Registry、contracts、server、web、既有 D18-A/S/R 历史文件或其他 L 任务文件。

## 冻结的路径边界（本任务唯一权威定义，供 L02/L09/L17/L21—L24/L33/L42 引用）

| 类型 | 允许根 | 说明 |
|---|---|---|
| Candidate 源文件 | `game/Assets/Art/Candidates/` | 候选文件只允许登记在此；游戏逻辑不得直接引用 |
| 逻辑元数据/证据 | `docs/game/design/common/assets`、`docs/game/design/common/handoffs`、`game/tools/results`、`game/Assets/Art/Candidates` | 允许根含候选根，因为许可证证据文本与候选文件同目录（真实事实：1,774 条 `license_evidence_path` 指向 `.../Candidates/*/License.txt`） |
| 忽略的本地缓存 | 冻结段清单（依据 2026-08-16 已提交 `.gitignore`）：Library/Temp/Obj/Build/Builds/Logs/UserSettings/MemoryCaptures/Recordings/TerrainDataCache/ScreenShots/ServerData/Caches/PackageCache/MonoBleedingEdge/node_modules/dist/coverage/__pycache__/bin/.vs/.idea/.vscode，及首段 `artifacts` | 段匹配大小写不敏感（对应 `[Tt]emp` 式规则）；工具不读不写 `.gitignore`，未来变更靠人工复核 |
| 未来正式资源 | `game/Assets/Art/{Models,Textures,Materials,Placeholder,Vegetation}`、`game/Assets/Audio`、`game/Assets/Prefabs`、`game/Assets/UI`、`game/Assets/Resources`、`game/Assets/Settings`、`game/Assets/ThirdParty` | 运行时引用字段只允许指向这些根；不含 `Candidates`、`Scripts`、`Tests` |

## 拒绝规则（对所有类型生效）

- `empty_path_rejected`：空/None；
- `absolute_path_rejected`：POSIX `/...` 与 UNC `\\server\share`（反斜杠统一转 `/` 后判定）；
- `drive_path_rejected`：盘符 `C:\...`、`C:/...`、`C:foo`；
- `env_var_path_rejected`：`%TEMP%` 等环境变量引用与 `$` 开头；
- `parent_traversal_rejected`：任何 `..` 段（Windows/Unix/混合分隔符与任意大小写均拒绝）；
- `ignored_cache_path_rejected`：命中冻结缓存段；
- `root_case_mismatch`：根前缀大小写与规范不一致（跨平台确定性）；
- `outside_allowed_root`：不在该类型的允许根内（错误输出逐条列出允许根与违规路径）；
- 运行时引用字段（`runtime_reference` 类型）额外拒绝 `candidate_binary_in_runtime_reference`（Candidate 二进制直接出现）与 `runtime_reference_outside_formal_roots`。

## 真实盘点事实（本任务验证，不补猜）

- 真实准入目录 `d18_prefab_candidate_intake_catalog.json`：1,865 条目全部通过，共检查 9,325 个路径字段（`candidate_file_path` / `manifest_path` / `license.license_evidence_path` / `hash_evidence.path` / `import_verified_evidence_path`），违规 0。
- 关键口径记录：许可证据 1,774 条指向候选根内的 `License.txt`、91 条指向 docs；导入证据 1,865 条全部指向 `game/tools/results/D18/D18-A0x/*UNITY_IMPORT_AUDIT.json`（SUPERSEDED 历史只读数据源，本工具仅做路径校验，不重跑）；目录内路径字段全部为前向分隔符、根前缀大小写与规范一致。
- 工具只读保证：不移动/删除/导入文件，不写 `.gitignore` 与真实资源目录；`--check-catalog` 输出 `read_only=true`。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python -m unittest tools.tests.test_d18_asset_path_validator` | PASS：38/38 |
| `python tools/d18_asset_path_validator.py --check-catalog` | PASS：1,865 条目 / 9,325 字段 / 0 违规 |
| CLI 单路径 PASS 样例（合法候选路径） | PASS |
| CLI 单路径 REJECT 样例（运行时引用指向候选二进制） | PASS：exit 1，输出允许根与违规路径 |
| `python -m py_compile tools/d18_asset_path_validator.py` | PASS |
| 全量工具回归（`python -m unittest discover -s tools/tests -p 'test*.py'`） | 384 项；L08 38/38 通过；6 项失败均非本任务（4 项基线 §1.3 已知 SUPERSEDED intake-refresh 断言，2 项为并行 D18-L16 进行中文件），不构成 L08 门禁 |
| `python tools/sync_logic_task_status.py` | PASS：总览刷新，D18-L08=LOGIC_PASS |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |

## 明确未宣称

- 未宣称任何资源状态迁移、导入成功、运行时可用或 `RuntimeApproved/ReleaseApproved` 授予（基线第 2 节禁止）。
- 路径词法校验不等于磁盘存在性审计（后者属 L33/L34）。
- 并行波次中 D18-L02—L07/L09—L20 各自唯一写入范围未被触碰。

## 待办 / 后续注意事项

- L17（`candidate_runtime_guard`）应复用 `validate_path(kind="runtime_reference")` 作为运行时引用的路径级前置；L42 目录编译器应为 `candidate_file_path`/`evidence_paths` 调用 `kind="candidate"/"evidence"`。
- 若未来 `.gitignore` 新增忽略段，需人工同步本工具 `IGNORED_CACHE_SEGMENTS`（工具永不写 `.gitignore`）。
- 建议 L33 目录声称审计补充「根前缀外子段」与磁盘实际文件名的大小写比对。

## 附录：错误输出格式（实际执行样例）

```text
[REJECT] candidate_binary_in_runtime_reference  field='runtime_reference_paths'
  value: game/Assets/Art/Candidates/Kenney/Models/tree.fbx
  allowed_roots: game/Assets/Art/Models, game/Assets/Art/Textures, ... , game/Assets/ThirdParty
  reason: 运行时引用字段直接指向 Candidate 二进制，游戏逻辑不得直接引用 Candidate。
```
