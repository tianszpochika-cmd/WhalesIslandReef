# D18-L06 交接报告

## 任务信息与结论

- 任务编号：`D18-L06`（波次 1，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L06/D18_L06_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_hash_validator.py`（新建，离线 SHA-256 验证器）
- `tools/tests/test_d18_hash_validator.py`（新建，15 项单元测试）
- `game/tools/results/D18/D18-L06/D18_L06_LOGIC_RESULT.json`（新建）
- `game/tools/results/D18/D18-L06/D18_L06_A05_REAL_CHECK.json`（新建，真实 A05 逐文件哈希实证）
- `docs/game/design/common/handoffs/D18/D18-L06_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema、manifest、catalog、Unity、Prefab、场景、资源、Registry、Systems、contracts、server、web 或旧 D18-A/S/R 历史文件。

## 实现内容

`tools/d18_hash_validator.py` 对每条记录做五项校验，每种失败有独立错误码：

| 校验 | 错误码 | 严重级 |
|---|---|---|
| 记录必须是对象 | `invalid_record` | error |
| 路径缺失/为空 | `invalid_path` | error |
| `sha256` 缺失（缺证据，不可验证） | `missing_sha256` | error |
| 哈希不是 64 位十六进制 | `invalid_hash_format` | error |
| 路径绝对化/`..` 越界/前导 `/`（仓库相对路径约束） | `path_escape` | error |
| 源文件不存在（缺证据） | `file_missing` | **pending**（记录状态 PENDING，总状态 PENDING） |
| 真实内容哈希与声明不一致（被篡改） | `hash_mismatch` | error |
| 相同 hash 被多条记录声明（只报告，不删除） | `duplicate_hash` | error |
| 输入为空 | `no_records` | error |

- 记录状态：`OK` / `PENDING`（仅 file_missing）/ `ERROR`；总状态 `PASS` / `PENDING`（只有缺文件）/ `BLOCKED`。
- **绝不合成 hash、绝不批准资产**：结果恒含 `approved_count: 0`；源文件不可用时只返回 PENDING/缺证据。
- **严格只读**：源文件/manifest 只以 `rb` 打开读取；唯一写入文件是 `--output` 结果 JSON；无任何删除/重命名/网络/导入/Unity 操作。
- 输入 JSON 兼容三种形态：记录数组、`{"records": [...]}`、`{"packages": [{"files": [{repository_path, sha256}]}]}`（兼容 A05 FILE_HASHES 形态）；路径字段接受 `path` / `candidate_file_path` / `repository_path`。

## 验证结果

| 命令 | 结果 |
|---|---|
| `python -m unittest tools.tests.test_d18_hash_validator -v` | PASS：15/15 |
| `python -m unittest discover -s tools/tests -p 'test*.py'` | 326 项：322 通过、4 失败（全部为既有 SUPERSEDED `test_d18_s02_r01_intake_refresh.py` 历史断言，基线第 1.3 节已记录，不是 L 门禁） |
| `python tools/d18_hash_validator.py --records .../D18_A05_FILE_HASHES.json --output game/tools/results/D18/D18-L06/D18_L06_A05_REAL_CHECK.json` | PASS：302/302 真实哈希匹配，approved 0 |
| `python tools/sync_logic_task_status.py` | PASS：D18-L06=LOGIC_PASS |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |

## 明确未宣称

- 没有任何资产被批准：`approved_count` 恒为 0；不产生 `RuntimeApproved`/`ReleaseApproved`/`IntakeEligible` 授予。
- A05 真实检查仅作为只读历史证据引用（基线第 4 节），不作为任何新任务的通过条件。
- 重复 hash 只登记报告，迁移裁决留给 L31（基线 2.4 拒绝条件 #3）。

## 待办 / 后续注意事项

- L31（重复身份审计）可复用本工具输出的 `duplicates` 数组与 `duplicate_hash` 报告做裁定入口。
- L40/L41（L 系列测试）可把本工具 15 项测试纳入 L 系列回归门禁。
- 其余目录（characters_animals / environment / ui_materials_audio 等）的逐文件哈希重算由 L31/L33 真实对账，本任务不越界声明。

## 变更文件

- `tools/d18_hash_validator.py`、`tools/tests/test_d18_hash_validator.py`、`game/tools/results/D18/D18-L06/`、本 handoff、当日记忆日志。
