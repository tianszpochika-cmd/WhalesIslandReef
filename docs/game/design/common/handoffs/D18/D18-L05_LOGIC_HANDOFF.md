# D18-L05 交接报告

## 任务信息与结论

- 任务编号：`D18-L05`（波次 1，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L05/D18_L05_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_provenance_validator.py`（新建，只读来源证据验证器，CLI 可用）
- `tools/tests/test_d18_provenance_validator.py`（新建，27 项单测，临时目录夹具，不落库）
- `game/tools/results/D18/D18-L05/D18_L05_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L05_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema、真实 manifest/catalog、资源、Unity、场景、Registry、审批状态、contracts、server、web 或旧 D18-A/S/R 历史文件。

## 工具接口（只读，无网络）

输入为来源记录列表或 `{"records": [...]}`：

```json
{
  "source_id": "source_kenney",
  "source_url": "https://kenney.nl/assets/pirate-kit",
  "author": "Kenney",
  "download_date": "2026-08-14",
  "original_filename": "kenney_pirate-kit.zip",
  "evidence_paths": ["docs/game/design/common/assets/packs/ships_port_props/D18_A05_SOURCE_EVIDENCE.md"]
}
```

CLI：`python tools/d18_provenance_validator.py --input <json> [--project-root <根>] [--today YYYY-MM-DD] [--output <json>]`
退出码：0=PASS，1=PENDING，2=FAIL。输出恒含 `read_only=true`、`network_accessed=false`、`license_approval=false`。

## 校验项（每项失败均定位字段）

| P0 证据 | 检查 | 错误码 |
|---|---|---|
| source_id | 必填、`source_<lower_snake_case>` | `missing_source_id` / `invalid_source_id` |
| source_url | 必填、https、无空白/凭据、host 存在 | `missing_source_url` / `invalid_source_url` |
| 作者/组织 | author 或 organization 至少一个非空 | `missing_author` |
| download_date | 必填、ISO 日历日期、不晚于 as-of | `missing_download_date` / `invalid_download_date` / `future_download_date` |
| original_filename | 必填、裸文件名（禁路径分隔符/绝对路径/URL/`.`/`..`） | `missing_original_filename` / `invalid_original_filename` |
| evidence_paths | 非空列表、仓库内相对路径、文件存在且非空 | `missing_evidence_paths` / `absolute_evidence_path` / `evidence_path_escape` / `evidence_file_missing` / `evidence_file_empty` |
| 来源/证据一致性 | URL host 与静态官方域名表一致；证据路径不得含异源 token（目录或文件名）；跨记录 source_id/证据路径不得重复 | `source_url_domain_mismatch` / `unknown_source_domain`(PENDING) / `evidence_source_mismatch` / `duplicate_source_id` / `duplicate_evidence_path` |

口径说明：

- `OFFICIAL_SOURCE_DOMAINS`（kenney.nl / quaternius.com / polyhaven.com / ambientcg.com / opengameart.org）与 `KNOWN_SOURCE_TOKENS` 是**格式/一致性静态参考表，不是准入或许可证批准**；未知来源一律 `PENDING`（保守，不通过也不判失败）。
- as-of 日期可经 `--today` 注入保证测试确定性；默认取真实当天。
- 不做任何网络请求；URL 仅作格式与静态域名一致性检查，可访问性不作证据，也绝不推导许可证。

## 测试覆盖（27/27 PASS）

正例：合法记录、organization 替代 author、as_of 同日不判未来、官方域子域名格式通过、无 token 证据路径。
反例：6 个 P0 字段逐一缺失、空 author、非法 source_id、非 https、URL 带凭据、非官方域名、未来日期、非法日历日期、包名含分隔符、Windows/POSIX 绝对路径、`../` 越界、证据文件缺失/空、异源证据目录、异源证据文件名（下划线分隔也被识别）、重复 source_id、record 非对象、CLI 往返与失败退出码（2）。

## 明确未宣称

- 任何资源未被本工具批准、导入、下载或授予状态；`license_approval=false`。
- 未知来源域名、URL 可达性、来源页内容变更均为未证实事项，保持 PENDING/Runtime 延期。
- 本工具不读取真实 catalog 内容（仅存在性/非空检查），不写任何 Registry/审批。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python -m unittest tools.tests.test_d18_provenance_validator` | PASS：27/27 |
| CLI 真实仓库冒烟（4 条记录：2 合法 + 域名不匹配 + 绝对路径 + 异源证据 + 重复 source_id） | PASS：按字段定位，exit 2 |
| `python tools/sync_logic_task_status.py` | PASS：D18-L05=LOGIC_PASS |
| `git diff --check` | PASS：无空白错误，仅既有 LF→CRLF 警告 |

## 待办 / 后续注意事项

- 全量回归在并行波次期间被其他进行中模块（如 L15 `test_d18_content_id_asset_link_validator` 的 `KeyError: 'reason'`）干扰，结果不稳定；本模块 27/27 稳定。波次结束后应在 L39/L47 汇总时重跑全量。
- L21—L24 来源目录登记时，未知来源（非静态表）将得到 PENDING，需人类确认官方域名后补充静态参考表或改为显式已知来源。
- L39 聚合审计可复用本工具 `validate_records()` 作为来源维度只读检查器；字段语义与 L01 基线第 6.1 节一致。
- 许可证/分发权利裁决属于 L04/L07 与人类审核，本工具不越权。
