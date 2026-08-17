# D18-L09 交接报告

## 任务信息与结论

- 任务编号：`D18-L09`（波次 1，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L09/D18_L09_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_attribution_validator.py`（新建，署名元数据验证器）
- `tools/tests/test_d18_attribution_validator.py`（新建，28 项正负单测）
- `game/tools/results/D18/D18-L09/D18_L09_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L09_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema、其他工具、真实 manifest/catalog、Unity、Prefab、场景、资源、Registry、Systems、contracts、server、web 或旧 D18-A/S/R 历史文件。

## 交付内容

### 1. 验证规则（纯元数据、只读）

`validate_attribution(record)` 只判断输入 dict，不读写文件、不访问网络、不修改 credits/UI/资源：

- **署名要求判定**：`license.attribution_required` → `rights.attribution_required` → 顶层 `attribution_required`；三者全缺 → `ATTR_REQUIREMENT_UNKNOWN`（保守阻断，不猜测）；非布尔 → `ATTR_REQUIREMENT_INVALID`。
- **必须署名时（P0 缺失即失败）**：作者（占位值如 unknown/TBD/未知 视为缺失）、许可证文本或链接（`license_url` 或 `license_evidence_path` 或 `license_text_path` 至少一项）、展示/credits 位置（`credits_location`/`display_location`/`credits_placement`）、资源关联（`canonical_asset_id`/`asset_id`/`content_ids`）。
- **格式校验**：`license_url` 必须是 http(s) URL；证据/文本路径必须是仓库内相对路径（拒绝绝对路径、盘符、`..`）。
- **CC0 防伪**：规范化许可证为 CC0 系列（如 `CC0-1.0`）但标记 `attribution_required=true` → `ATTR_CC0_FALSE_ATTRIBUTION_REQUIRED` 失败；CC0 不要求署名字段即可通过。
- **不越权警告**：CC-BY 系列但 `attribution_required=false`、公有领域却要求署名、缺 `canonical_license` 只给 warning，交由 L04 标准化复核，本任务不裁决。
- **商业权利边界**：结果恒含 `commercial_rights_inferred: false` 与说明 note——署名存在不构成商业权利证明（商业权利由 L07 独立验证）。

### 2. 可本地化错误码

全部错误码稳定输出在 `issues[].code`（message_key 同码），消息目录 `MESSAGES` 提供 zh-CN/en，`localize(code, lang)` 缺语言回退 en、缺码回退原码。错误码清单（14 P0 + 3 WARN + 1 NOTE）：

| 码 | 含义 |
|---|---|
| `ATTR_INPUT_TYPE_INVALID` | 输入非对象 |
| `ATTR_REQUIREMENT_UNKNOWN` / `ATTR_REQUIREMENT_INVALID` | 署名要求缺失/非布尔 |
| `ATTR_CC0_FALSE_ATTRIBUTION_REQUIRED` | CC0 伪造必须署名 |
| `ATTR_AUTHOR_MISSING` / `ATTR_AUTHOR_PLACEHOLDER` | 缺作者/作者为占位 |
| `ATTR_LICENSE_REF_MISSING` | 缺许可证文本/链接 |
| `ATTR_LICENSE_URL_INVALID` / `ATTR_LICENSE_EVIDENCE_PATH_INVALID` | 链接/证据路径格式非法 |
| `ATTR_CREDITS_LOCATION_MISSING` / `ATTR_CREDITS_LOCATION_PLACEHOLDER` | 缺展示位置/为占位 |
| `ATTR_ASSET_LINK_MISSING` | 缺资源关联 |
| `WARN_BY_LICENSE_NO_ATTRIBUTION` 等 3 项 | 交 L04 复核的异常提示 |

### 3. 与 D18-L04 的接口衔接

L04 与 L09 并行开发，本任务**未导入** `tools/d18_license_normalizer.py`。按 L04 任务卡接口说明（`canonical_license`/`attribution_required`/`commercial_allowed`/`redistribution_allowed` 与确定性错误）鸭子兼容消费：`license` 嵌套或顶层平铺均可，字段名与基线 6.1/6.2 字段词典一致。若 L04 最终模块接口字段名有差异，由 L39/L40/L41 集成时对齐，本任务无 blocker。

## 明确未宣称

- 没有验证任何真实 manifest/catalog 条目（L21—L24 登记、L40/L41 夹具与 L43 审计才批量消费本验证器）。
- 没有产生任何 `RuntimeApproved`/`ReleaseApproved` 授予，未改 credits、UI、场景或资源。
- 没有把署名元数据存在当作商业/发布权利证明。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python -m py_compile tools/d18_attribution_validator.py tools/tests/test_d18_attribution_validator.py` | PASS：编译通过 |
| `python -m unittest tools.tests.test_d18_attribution_validator` | PASS：28/28 |
| `python -m unittest discover -s tools/tests -p 'test*.py'` | 190/194 PASS；4 项失败均为 SUPERSEDED 历史 `test_d18_s02_r01_intake_refresh.py` 断言（基线 1.3 节已知，非 L 门禁），无回归 |
| `python tools/sync_logic_task_status.py` | PASS：总览重新生成，D18-L09=LOGIC_PASS |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |

## 待办 / 后续注意事项

- 真实 credits/UI 中署名展示落地属 Runtime 阶段；本任务只保证元数据声明齐全。
- 许可证文本内容与署名文本一致性、署名出现在游戏内位置的实际可见性需人工复核。
- 单测覆盖 28 项（正例 11、负例 17），包括输入不被修改、商业权利不推断、聚合函数、本地化回退；供 L40/L41 复用。
