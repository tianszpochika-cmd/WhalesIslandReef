# D18-L04 交接报告

## 任务信息与结论

- 任务编号：`D18-L04`（波次 1，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L04/D18_L04_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_license_normalizer.py`（新建，许可证标准化工具）
- `tools/tests/test_d18_license_normalizer.py`（新建，33 项单元测试）
- `game/tools/results/D18/D18-L04/D18_L04_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L04_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何真实 manifest、Schema、catalog、Unity、Prefab、场景、资源、Registry、Systems、contracts、server、web 或旧 D18-A/S/R 历史文件；未访问网络。

## 工具接口（供 L07/L09/L42/L35/L40/L41 消费）

CLI：

```powershell
python tools/d18_license_normalizer.py --input record.json [--output out.json]
python tools/d18_license_normalizer.py < record.json
python tools/d18_license_normalizer.py --input records.json --strict
```

输入记录（单对象或对象数组）字段（别名）：

- `license_id` / `license` / `license_url`（原始许可证名称/标识/URL）
- `attribution_required`
- `commercial_use` / `commercial_allowed`
- `modification_allowed`
- `source_redistribution` / `build_distribution` / `redistribution_allowed`
- `record_id`（可选，审计追溯）

输出（确定性、JSON）：

- `canonical_license`：稳定规范标识（`CC0-1.0`、`CC-BY-3.0/4.0`、`CC-BY-SA-3.0/4.0`、`CC-BY-NC-3.0/4.0`、`CC-BY-NC-SA-3.0/4.0`、`MIT`、`OFL-1.1`），未知则为 `null`
- `attribution_required` / `commercial_allowed` / `modification_allowed` / `redistribution_allowed`：保守有效真值
- `status`：仅 `NORMALIZED` / `BLOCKED` / `PENDING`（无任何“批准”语义）
- `errors`：`{code, field, severity, message}`，代码：`LICENSE_UNKNOWN`、`LICENSE_UNSUPPORTED_VERSION`、`LICENSE_NC`、`LICENSE_PERSONAL_ONLY`、`LICENSE_NO_REDISTRIBUTION`、`LICENSE_NO_DERIVATIVES`、`LICENSE_SHARE_ALIKE`、`FIELD_MISSING`、`FIELD_UNCONFIRMED`、`FIELD_INVALID`、`CONFLICT_ATTRIBUTION`、`CONFLICT_COMMERCIAL`、`CONFLICT_MODIFICATION`、`CONFLICT_REDISTRIBUTION`、`RECORD_NOT_OBJECT`
- `warnings`：OFL 保留字体名提示、MIT 许可文本保留提示、多别名字段提示等
- `input`：原始输入快照

Python API：`canonicalize_license_id(raw) -> (canonical|None, code)`、`parse_claim(value, allow_restriction=...)`、`normalize_license(record) -> dict`、`normalize_license_batch(records) -> dict`、常量 `NORMALIZED/BLOCKED/PENDING`、`SUPPORTED_LICENSES`。

## 保守规则（冻结，测试证明不放宽限制）

1. 纯离线：无网络访问、无下载、不校验 URL 可达性（URL 仅作静态标识匹配）。
2. 未知许可证文本一律不猜测为 CC0 或任何许可 → `PENDING LICENSE_UNKNOWN`；“Public Domain”“Free asset pack”“CC-BY”（无版本）均不猜测。
3. 缺失字段（`FIELD_MISSING`）、值为 pending/not_applicable（`FIELD_UNCONFIRMED`）、不可解析（`FIELD_INVALID`）→ `PENDING`，即使许可证已知。
4. NC、个人使用、禁止再分发、禁止衍生、Share-Alike → `BLOCKED`（`LICENSE_NC`/`LICENSE_PERSONAL_ONLY`/`LICENSE_NO_REDISTRIBUTION`/`LICENSE_NO_DERIVATIVES`/`LICENSE_SHARE_ALIKE`）。
5. 输入声明与规范许可证矛盾（如 CC-BY-4.0 + `attribution_required=false`，或 CC0 + `commercial_use=false`）→ `BLOCKED CONFLICT_*`，输出取更严格一侧，绝不放松。
6. `NORMALIZED` 仅表示“规范化且未放宽任何限制”，是规范化判定，不是资产批准；状态机批准仍由人类门禁负责（基线 2.6）。

## 测试覆盖（33/33 PASS）

正例：CC0（精确/别名/URL）全字段 NORMALIZED、CC-BY-4.0 NORMALIZED、CC-BY-3.0、MIT、OFL-1.1（含 warning）、批处理汇总、确定性输出。
反例：未知许可证（含 “Public Domain”“Free asset pack”“CC-BY” 无版本、CC-BY-2.5 不支持版本）一律 PENDING 不猜 CC0；CC-BY+署名 false 冲突 BLOCKED 且输出仍 true；CC0+商用 false 冲突 BLOCKED；CC-BY-NC-4.0 与 “non-commercial” 声明 BLOCKED；personal use BLOCKED；Share-Alike BLOCKED；缺署名记录 PENDING；缺再分发字段 PENDING；pending 值 FIELD_UNCONFIRMED；source/build 再分发互相矛盾 BLOCKED；非法字段值 PENDING；非对象记录 PENDING；无网络导入、无硬编码真实资源名。
退出码：处理成功 0；`--strict` 下存在非 NORMALIZED 记录 1；输入不可读/非 JSON 2。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python -m py_compile tools/d18_license_normalizer.py tools/tests/test_d18_license_normalizer.py` | PASS |
| `python -m unittest discover -s tools/tests -p "test_d18_license_normalizer.py"` | PASS：33/33 |
| `python -m unittest discover -s tools/tests -p "test*.py"` | 326 个测试，322 通过、4 失败；4 项均为既有 SUPERSEDED `test_d18_s02_r01_intake_refresh.py`（基线 1.3 节声明，非 L 门禁），本次无新增失败 |
| CLI 冒烟（stdin 单记录 / `--input`+`--strict` 批处理 / 非法 JSON） | PASS：NORMALIZED×1、BLOCKED×1、PENDING×1、exit 1；非法输入 exit 2 |
| `python tools/sync_logic_task_status.py` | PASS：D18-L04=LOGIC_PASS |
| `git diff --check` | PASS：exit 0；仅既有 LF→CRLF 警告，无尾随空白错误 |

## 明确未宣称

- 任何资产未被本工具“批准”：工具只输出 `NORMALIZED/BLOCKED/PENDING` 规范化判定；`RuntimeApproved`/`ReleaseApproved` 保持 0 个且无持有者。
- 未把“免费/可下载/URL 可达”解读为可商用、可再分发。
- 未对真实 manifest/catalog 做任何写入或隐含批准。

## 待办 / 后续注意事项

- L07（`d18_distribution_rights_validator.py`）负责四项权利（商业使用/修改/仓库存储/随游戏分发）的完整校验；本工具输出的 `commercial_allowed`/`redistribution_allowed` 是 L07 的输入语义之一，L07 仍需独立判断“免费≠可商用”。
- L09（`d18_attribution_validator.py`）在 `attribution_required=true` 时要求作者/许可引用/展示位置；CC0 的 `attribution_required=false` 不应被伪造为“必须署名”，反向同理（本工具已规范 CC0=false、BY=true）。
- L42 目录编译器可把 `NORMALIZED` 作为 `license` 字段的准入输入；`BLOCKED`/`PENDING` 一律不得进入 IntakeEligible。
- `CC-BY-SA` 与 OFL 保留字体名需要责任人类裁定（本工具保守 BLOCKED/附 warning），属 Runtime/发布阶段人工审核延期项。
- 真实 manifest 中 `build_distribution=pending`（如 opengameart_bird 等历史条目）经本工具必然 PENDING，符合保守口径，由人工补齐字段后重跑。

## 附录：本任务未触碰的既有工具

按基线 1.3 节，`d18_asset_manifest_audit.py`、`d18_prefab_candidate_intake_audit.py`、`d18_placeholder_replacement_map.py` 等 A/S/R 系列工具保持原样（SUPERSEDED 隔离），本任务不重跑、不修改。
