# D18-L37 交接报告

## 任务信息与结论

- 任务编号：`D18-L37`（波次 2，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L37/D18_L37_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/schemas/d18_runtime_promotion_request.schema.json`（新建）
- `tools/tests/d18_runtime_promotion_request_fixtures/`（新建：README + 5 个合法夹具 + 15 个非法夹具）
- `game/tools/results/D18/D18-L37/D18_L37_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L37_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何其他 Schema、manifest、catalog、Unity、Prefab、场景、资源、Registry、Systems、contracts、server、web、审批目录或旧 D18-A/S/R 历史文件。

## Schema 设计（按 L03 第 7.4 节冻结接口实现）

`d18_runtime_promotion_request.schema.json`（Draft 2020-12）：

| 字段 | 语义与约束 |
|---|---|
| `request_kind` / `schema_version` | const 判别器 `d18_runtime_promotion_request` / 1 |
| `asset_id` | 引用 L02 `canonical_asset_id`（`asset_*`，禁止磁盘路径） |
| `current_state` | **枚举仅 IntakeEligible / RuntimeRequested**；Candidate 直达申请、任何批准状态枚举值都非法 |
| `target_usage_scene` | 受控工作场景/Prefab 标识（`[a-z0-9_]+`）；**Schema 无目标状态机制**——申请目标是使用场景而非批准 |
| `descriptors` | L25—L29 描述器引用（kind ∈ import/mesh/material/animation/ui_audio + 仓库相对路径），**至少 1 条**（缺证据引用非法） |
| `requirements` | 碰撞/LOD/动画需求声明对象（声明不产生任何实体） |
| `approver` | `^human:[a-z0-9_]+$`——审批人必须是人类角色，`tool:*` 被模式直接拒绝 |
| `risks` | 风险清单，至少 1 项 |
| `rollback` | 必填：`reference`（`asset_*`/`placeholder_*`）+ `description` 回退说明 |
| `submitted_at` | ISO-8601 带时区（正则强制：`^\d{4}-\d{2}-\d{2}T...Z|[+-]\d{2}:\d{2}$`） |
| `declares_approval` | **const false**——只能表达 request，任何 true 非法 |

**批准禁入保证**（结构性，非仅文档）：无批准结果/批准状态/目标状态/审批意见字段；`grant`/`approval_result`/`target_state`/`decision` 等混入被 `additionalProperties=false` 拒绝；`current_state` 枚举不含任何批准状态；`approver` 模式排除工具。合法申请在逻辑阶段只产生 `RUNTIME_REQUEST` 审计记录（L35），绝不产生 RuntimeApproved/ReleaseApproved。

## 夹具覆盖（5 合法 + 15 非法）

合法：角色晋级（import+animation 描述器）、船体（import+mesh、convex/LOD3）、环境（terrain）、UI/音频（ui_audio 描述器）、RuntimeRequested 续申（覆盖两个合法当前状态）。

非法每项只偏离一个维度：`current_state=Candidate`（越级请求）、缺 rollback、`target_state=ReleaseApproved`（非法目标状态）、缺 descriptors（缺证据）、`approver=tool:...`（工具审批人）、`grant`+`approval_result` 混入（Schema 禁止表达批准）、`current_state=ReleaseApproved`、asset_id 为磁盘路径、risks 空、`decision` 混入、坏 submitted_at、空 target_usage_scene、requirements 类型错、描述器未知 kind、描述器路径越界（`C:/Windows/evil.txt`）。

## 验证结果

| 命令 | 结果 |
|---|---|
| Schema `check_schema`（Draft202012Validator） | PASS：schema ok |
| 双向夹具校验（valid 必须 VALID、invalid 必须 INVALID） | PASS：5/5 + 15/15 零偏差；错误原因抽查正确 |
| `python -m unittest discover -s tools/tests -p 'test*.py'` | 702 项：697 通过、5 失败（4 项为既有 SUPERSEDED `test_d18_s02_r01_intake_refresh.py` 历史断言；1 项 `test_d18_generated_asset_guard` 为并行 L38 Agent 写入中的瞬时状态，单独复跑 31/31 OK；均非 L 门禁，与本任务产物无关） |
| `python tools/sync_logic_task_status.py` | PASS：D18-L37=LOGIC_PASS |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |

## 明确未宣称

- 未产生、也无法通过本 Schema 产生任何 RuntimeApproved/ReleaseApproved；未修改任何审批目录或真实记录状态。
- 夹具全部为合成数据（`asset_fixture_*`、`docs/fixtures/...` 合成路径），不补造真实数据。

## 待办 / 后续注意事项

- L35 表驱动迁移审计将把合法申请的 `RUNTIME_REQUEST` 事件写入审计链；L42 编译器/消费任务使用本 Schema 提交申请。
- `descriptors` 路径引用须与 L25—L29 交付物路径一致；L45 一致性审计核对 `asset_id` 与 L02 记录。
- 审批执行（human 决策、证据补充、驳回/撤销）属未来 Runtime 阶段，不在 Schema 内表达。
