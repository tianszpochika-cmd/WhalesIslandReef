# D18-L38 交接报告

## 任务信息与结论

- 任务编号：`D18-L38`（波次 2，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L38/D18_L38_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_generated_asset_guard.py`（新建，只读谱系守卫）
- `tools/tests/test_d18_generated_asset_guard.py`（新建，31 项正负夹具）
- `game/tools/results/D18/D18-L38/D18_L38_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L38_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何既有 Schema、catalog、其他 D18 工具、Unity、Prefab、场景、资源、Registry、contracts、server 或 web。

## 实现内容

**谱系四元模型**（对齐 L01 基线 6.1 字段词典与 L02 `canonical_asset_id` 键格式）：

| origin_kind | 定义 | 谱系要求 |
|---|---|---|
| `original` | 第三方原件 | 必须携带 `third_party_claim`（source_id/source_url/license_id）；不得声明 parent 或转换步骤 |
| `converted` | 转换件（同一内容、格式转换） | 必须指向 parent + 转换步骤 + `license_basis` |
| `derived` | 派生件（基于原件的新内容） | 必须指向 parent + 转换步骤 + `license_basis` |
| `generated` | 生成件（程序/工作台生成） | 必须含生成步骤；项目自有（`project_authored=true`）可无外部 parent |

**规则（18 条，P0 阻断 / P1 警告）**：json_parse_error、missing_records、duplicate_record、bad_asset_id、unknown_origin_kind、self_parent、circular_lineage、origin_with_parent、origin_missing_claim、**non_original_missing_lineage（缺谱系阻断批准）**、non_original_with_claim、**generated_masquerading_as_original（生成物伪装第三方原件）**、missing_parent_reference、missing_license_basis、**license_widening（不得放宽 parent 的商用/修改/分发/署名权利）**、evidence_path_escape、missing_evidence_path（P1）、approval_with_invalid_lineage（交叉检查）。

**交叉检查**：`--asset-records` 可选读入 L02 资源记录；批准状态（IntakeEligible/RuntimeRequested/RuntimeApproved/ReleaseApproved）记录若其谱系记录无效 → 整体 BLOCKED。仅对两端同时出现的 id 生效，不误伤无谱系记录的 original。

**只读契约**：工具不写、不转换、不生成、不删除任何贴图/模型/二进制或记录；测试含 `test_tool_is_read_only` 契约断言。

## 验证命令与结果

| 命令 | 结果 |
|---|---|
| `python -m unittest tools.tests.test_d18_generated_asset_guard -v` | 31/31 PASS |
| CLI 正夹具（真实 Kenney License.txt 证据路径） | PASS，exit 0 |
| CLI 负夹具（生成物伪装第三方） | BLOCKED，exit 2，`generated_masquerading_as_original` |
| `python -m unittest discover -s tools/tests -p 'test*.py'` | 738 项，4 项既有失败（遗留 R01 断言，SUPERSEDED 基线，与本任务无关） |
| `python -m py_compile` ×2 | exit 0 |
| `python tools/sync_logic_task_status.py` | D18-L38 = LOGIC_PASS |
| `git diff --check` | exit 0，仅既有 LF→CRLF 警告 |

## 接口与后续使用

- 谱系目录格式冻结于工具 docstring（`lineage_kind: d18_generated_asset_lineage`）；字段语义与 L01 基线 6.1 一致。
- 消费方：L42 目录编译器应把生成/派生记录生成谱系目录并用本守卫校验；L43/L44 审计可对真实候选/派生文件跑本守卫；L45 Schema 兼容审计应纳入本格式。
- 项目自有生成物（如 D18-R03-A01 的 Hair_TwistPonytail、AvatarBuilder 生成的 Avatar）应登记为 `generated` + `project_authored=true`，禁止带 `third_party_claim`。
- Runtime 延期：真实 Unity 生成流程的谱系登记与最终全覆盖复核属 Runtime/消费任务阶段。

## 未宣称

- 不宣称任何真实资产谱系已完备（本任务只提供校验器与夹具，未生成真实谱系数据）；
- 不宣称任何资产 `RuntimeApproved`/`ReleaseApproved`（二者仍为 0）。
