# D18-L35 交接报告

## 任务信息与结论

- 任务编号：`D18-L35`（波次 2，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L35/D18_L35_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_status_transition_audit.py`（新建，表驱动状态迁移审计，CLI 可用）
- `tools/tests/test_d18_status_transition_audit.py`（新建，56 项单测）
- `game/tools/results/D18/D18-L35/D18_L35_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L35_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema、L03 状态机文档、真实 catalog、资源、Unity、场景、Registry、审批状态、contracts、server、web 或旧 D18-A/S/R 历史文件。

## 工具接口（只读，表驱动）

```powershell
python tools/d18_status_transition_audit.py --records <审计记录 JSON> [--catalog <记录目录>...] [--matrix-doc <L03 文档>] [--phase logic|runtime] [--project-root .] [--output <json>]
```

- 内置矩阵 = L03 4.4 JSON 逐字镜像；`--matrix-doc` 解析冻结文档的 JSON 代码块并与嵌入矩阵比对，不一致即 exit 2。
- 退出码：0=PASS，2=FAIL。输出恒含 `read_only=true`、`network_accessed=false`。
- `--phase`：logic（默认）拒绝一切 `logic_phase=false` 的授予并计数 `logic_phase_grants_attempted`；runtime 允许 T3/T4 通过审计（工具仍不授予任何东西）。

## 校验项（错误均定位字段）

| 维度 | 检查 | 错误码 |
|---|---|---|
| 记录形态 | record_id（TR-asset_<...>-<seq>）、canonical_asset_id、前缀一致、result_state==to_state、occurred_at（ISO-8601 带时区）、operator（tool:/human:） | invalid_record_id / record_id_asset_mismatch / missing_result_state / result_state_mismatch / invalid_occurred_at / invalid_operator |
| 矩阵 | from/to/event 在冻结枚举内；(from,event,to) 必须在 allowed_transitions，禁止边显式拒绝 | unknown_state / unknown_event / transition_not_allowed |
| 审计完整性 | REJECT/REVOKE/RE_REGISTER 必填 reason；REVOKE/RELEASE_APPROVE 必填 prior_record_id | missing_reason / missing_prior_record_id |
| 证据 | evidence_paths 非空、仓库内相对路径、文件存在且非空 | missing_evidence / evidence_path_escape / evidence_file_missing / evidence_file_empty |
| 角色门 | RELEASE_APPROVE 与 RE_REGISTER 必须 human 操作者 | release_approve_requires_human / re_register_requires_human |
| 阶段门 | logic 相位下 RUNTIME_APPROVE/RELEASE_APPROVE → 伪造处理 | logic_phase_forbidden（+ 计数） |
| waiver（L19） | expires_at 必填、status∈{Active,Expired}、过期/Expired → 任何批准 FAIL 并引导 REJECT/REVOKE | invalid_waiver / expired_waiver / expired_waiver_note(warning) |
| 链审计 | record_id 唯一、序列连续、from==前序 to、prior 必须存在、REVOKE prior.to==被撤状态、RELEASE_APPROVE prior 必须是 RuntimeApproved 授予 | duplicate_record_id / chain_gap / chain_break / missing_prior_record / prior_record_state_mismatch / wrong_prior_record |
| 目录扫描 | 既有记录 state 枚举合法性；logic 相位持有 RuntimeApproved/ReleaseApproved → 伪造 | unknown_state / logic_phase_state_hold |

## 测试覆盖（56/56 PASS，对应 L03 7.2 清单）

1. T1—T7 每条允许迁移（T3/T4 分矩阵合法性与相位门两层验证）；
2. 4.2 每条禁止迁移（含 Candidate→RuntimeRequested/RuntimeApproved/ReleaseApproved、IntakeEligible→RuntimeApproved/ReleaseApproved、RuntimeRequested→ReleaseApproved、ReleaseApproved→Rejected、Rejected 终态无出边）；
3. Revoked 恢复：仅 RE_REGISTER+human PASS；Revoked→IntakeEligible/RuntimeApproved/直接复活 FAIL；RE_REGISTER 非 human FAIL；
4. 过期 waiver：INTAKE_APPROVE/RUNTIME_REQUEST 阻断；REJECT/REVOKE（reason+prior）放行并带过期提示；缺 expires_at/非法 status FAIL；
5. ReleaseApproved 伪造：tool 操作者、from≠RuntimeApproved、缺 prior、prior 非 RuntimeApproved 授予 → 全部 FAIL；
6. 审计完整性：REJECT/REVOKE/RE_REGISTER 缺 reason、REVOKE/RELEASE_APPROVE 缺 prior、result_state 不一致、证据缺失/越界/文件缺失 → FAIL；
7. 未知事件/未知状态/矩阵外迁移/非法 operator/非法时间/非法 record_id → FAIL；
8. 链断裂、重复 ID、序列间隙、prior 不存在、prior 状态不符 → FAIL；
9. 真实记录只读：真实 kenney/quaternius/intake catalog 扫描前后 SHA-256 完全一致。

## 真实数据扫描（只读）

- 真实来源目录 70 条记录（kenney 9 + quaternius 61，均为 Candidate 登记）：`catalog_state_holds=0`、失败 0、exit 0。
- intake catalog（1,865 条）无 L02 `state` 字段，按设计跳过；三个文件 SHA-256 运行前后一致。
- 当前仓库无任何 `TR-*` 审计记录、无 Rejected/Revoked/waiver 持有者——这是真实事实，不是通过条件。

## 明确未宣称

- 未授予、未修改任何状态；logic 相位下任何 RUNTIME_APPROVE/RELEASE_APPROVE 均被判定为伪造。
- 人类审核结论（ReleaseApproved）只能校验形态（human operator、prior、证据），无法由工具验证真实性。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python -m unittest tools.tests.test_d18_status_transition_audit` | PASS：56/56 |
| `python tools/d18_status_transition_audit.py --matrix-doc <L03 文档>` | PASS：嵌入矩阵与冻结文档一致 |
| 真实 catalog 只读扫描（kenney+quaternius+intake） | PASS：70 条 0 持有 0 失败；SHA-256 不变 |
| `python tools/sync_logic_task_status.py` | PASS：D18-L35=LOGIC_PASS |
| `git diff --check` | PASS：无空白错误，仅既有 LF→CRLF 警告 |

## 待办 / 后续注意事项

- L36（LocalDevelopmentReady 守卫）可直接消费本工具链审计结论作为 C10 条件输入。
- L39 聚合审计可调用 `audit_all()`；L42 编译器产生 INTAKE_APPROVE 审计记录时，链审计接口已就绪。
- 波次 2 其他并行任务进行中；波次 3 汇总（L39/L47）时应重跑全量回归。
