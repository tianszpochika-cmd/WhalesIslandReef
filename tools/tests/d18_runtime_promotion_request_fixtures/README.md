# D18-L37 Runtime 晋级请求夹具（fixtures）

本目录是 `tools/schemas/d18_runtime_promotion_request.schema.json` 的固定校验夹具（D18-L37 唯一写入范围）。

## 声明

- 全部夹具为**合成数据**：`asset_id` 一律以 `asset_fixture_` 开头、描述器路径为 `docs/fixtures/...` 合成相对路径；**不对任何真实候选补造数据**，不宣称任何真实资产状态。
- 本 Schema **只能表达 request**：没有任何批准结果/批准状态/目标状态字段，`declares_approval` 恒为 `false`；任何 true、任何批准字段混入、任何 `tool:` 审批人都非法。合法申请在逻辑阶段只产生 `RUNTIME_REQUEST` 审计记录（L35），绝不产生 RuntimeApproved/ReleaseApproved。
- 校验方式与仓库既有用法一致：Python `jsonschema.Draft202012Validator`（同 D18-L02/L25 夹具）。

## 合法夹具（VALID）

| 文件 | 当前状态 | 覆盖点 |
|---|---|---|
| `valid_character_promotion.json` | IntakeEligible | 角色晋级受控工作 Prefab：import+animation 描述器、胶囊碰撞/LOD2/动画需求、human 审批人 |
| `valid_ship_promotion.json` | IntakeEligible | 船体：import+mesh 描述器、convex 碰撞/LOD3/无动画、回退指向自身 asset |
| `valid_environment_promotion.json` | IntakeEligible | 环境：terrain 碰撞、手写 LOD |
| `valid_ui_audio_promotion.json` | IntakeEligible | UI/音频：ui_audio 描述器、全部 none 需求 |
| `valid_rerequest_from_runtime_requested.json` | RuntimeRequested | 已在申请态续申（第二个合法当前状态） |

## 非法夹具（INVALID，每项只偏离一个维度）

| 文件 | 失败原因 |
|---|---|
| `invalid_candidate_direct_request.json` | `current_state=Candidate`（越级请求：无准入证据直接申请） |
| `invalid_rollback_missing.json` | 缺 `rollback`（回退声明必填） |
| `invalid_release_approved_target.json` | 混入 `target_state=ReleaseApproved`（Schema 没有目标状态机制，additionalProperties=false 拒绝） |
| `invalid_missing_descriptors.json` | 缺 `descriptors`（缺证据引用） |
| `invalid_approver_is_tool.json` | `approver=tool:d18_catalog_compiler`（审批人不得是工具，必须 `human:*`） |
| `invalid_approval_grant_fields.json` | 混入 `grant=RuntimeApproved`、`approval_result=approved`（Schema 禁止表达批准） |
| `invalid_current_state_release_approved.json` | `current_state=ReleaseApproved`（不在 IntakeEligible/RuntimeRequested 枚举） |
| `invalid_asset_id_format.json` | `asset_id` 是磁盘路径而非 `asset_*` |
| `invalid_empty_risks.json` | `risks=[]`（风险清单至少 1 项） |
| `invalid_unknown_field.json` | 混入 `decision=approve`（未知字段） |
| `invalid_bad_submitted_at.json` | `submitted_at` 非 ISO-8601 date-time |
| `invalid_empty_target_scene.json` | `target_usage_scene=""`（空目标场景） |
| `invalid_requirements_wrong_type.json` | `requirements` 为字符串而非对象 |
| `invalid_descriptor_unknown_kind.json` | 描述器 `kind=prefab`（未知描述器类型） |
| `invalid_descriptor_path_escape.json` | 描述器 `path` 为 Windows 绝对路径（路径越界） |

## 校验命令（可复跑）

```powershell
python -c "import json;from jsonschema import Draft202012Validator;s=json.load(open('tools/schemas/d18_runtime_promotion_request.schema.json',encoding='utf-8'));Draft202012Validator.check_schema(s);print('schema ok')"
python -c "import json,glob;from jsonschema import Draft202012Validator;s=json.load(open('tools/schemas/d18_runtime_promotion_request.schema.json',encoding='utf-8'));v=Draft202012Validator(s);vb=[p for p in glob.glob('tools/tests/d18_runtime_promotion_request_fixtures/valid_*.json') if not v.is_valid(json.load(open(p,encoding='utf-8')))];ib=[p for p in glob.glob('tools/tests/d18_runtime_promotion_request_fixtures/invalid_*.json') if v.is_valid(json.load(open(p,encoding='utf-8')))];print('valid unexpected INVALID:',vb);print('invalid unexpectedly VALID:',ib)"
```
