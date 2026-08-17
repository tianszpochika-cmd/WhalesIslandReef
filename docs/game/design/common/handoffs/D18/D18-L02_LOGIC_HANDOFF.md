# D18-L02 交接报告

## 任务信息与结论

- 任务编号：`D18-L02`（波次 1，可与 L03—L20 并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L02/D18_L02_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `tools/schemas/d18_logic_asset_record.schema.json`（新建，本任务独占 Schema）
- `tools/tests/d18_logic_asset_record_fixtures/README.md`（新建，夹具说明）
- `tools/tests/d18_logic_asset_record_fixtures/valid.json` + 8 个 `invalid_*.json`（新建，合成夹具）
- `game/tools/results/D18/D18-L02/D18_L02_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L02_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何其它 D18 工具、catalog、Unity、资源、场景、Prefab、Registry、旧 manifest、contracts、server 或 web。

## Schema 字段边界（冻结于本任务）

1. **13 个必填字段**（`additionalProperties: false`，严格模式）：
   `canonical_asset_id`（`^asset_[a-z0-9_]+$`，业务键，禁止磁盘路径/显示名）、
   `category`（11 值枚举，见下）、`source_id`（`^source_[a-z0-9_]+$`）、
   `source_url`（仅 https）、`license`（对象：license_id/license_url/license_evidence_path/attribution_required）、
   `rights`（对象：commercial_use/modification_allowed/repository_storage/distribution_with_game 四项必填布尔）、
   `download_date`（ISO 日期或 null=未下载）、`original_filename`（不含路径分隔符）、
   `sha256`（64 位十六进制或 null=Pending，绝不补猜）、`state`（七个冻结状态枚举）、
   `content_ids`（非空数组，`^[a-z0-9_]+$`，口径以 1,865 条真实 intake 条目为据）、
   `evidence_paths`（非空数组，仅仓库相对路径）、`runtime_deferred`（字符串数组，诚实登记）。
2. **状态枚举**：仅 `Candidate / IntakeEligible / RuntimeRequested / RuntimeApproved / ReleaseApproved / Rejected / Revoked`。
   历史/未知批准状态（`Approved`、`LocalDevelopmentReady` 等）一律拒绝。
3. **批准状态条件证据**（`allOf` 条件必填）：`ReleaseApproved` → 必填 `human_reviewer`、
   `human_review_date`、`release_evidence_path`；`RuntimeApproved` → 必填 `runtime_consumers`。
   本逻辑阶段不产生任何批准授予。
4. **路径边界**：证据与许可路径只允许 `[A-Za-z0-9_.][A-Za-z0-9_./-]*` 且无 `..` 路径段；
   绝对路径、盘符（`C:`）、UNC（`\\server`）、反斜杠、前导斜杠全部拒绝。
5. **不对真实候选补造数据**：全部夹具为合成数据（`asset_fixture_*` + `example.invalid`），
   valid.json 的 sha256 是空输入串公开摘要，不代表任何真实文件。

## 夹具（1 合法 + 8 非法，全部按预期字段失败）

| 夹具 | 预期 | 失败字段（实测） |
|---|---|---|
| `valid.json` | VALID | — |
| `invalid_missing_license.json` | INVALID | 缺 `license` |
| `invalid_bad_sha256.json` | INVALID | `sha256` 格式 |
| `invalid_state_jump.json` | INVALID | `ReleaseApproved` 缺三项人工证据（越级） |
| `invalid_empty_source.json` | INVALID | `source_id=''` + 缺 `source_url`（空来源） |
| `invalid_absolute_path.json` | INVALID | `evidence_paths` 盘符绝对路径（越界） |
| `invalid_unknown_state.json` | INVALID | `'Approved'` 不在枚举（未知批准状态） |
| `invalid_empty_content_ids.json` | INVALID | `content_ids` 空数组 |
| `invalid_rights_incomplete.json` | INVALID | 缺 `distribution_with_game` |

## 与后续任务的接口（已知依赖，非 blocker）

- **L03** 状态机文档输出完整迁移规则；本 Schema 已按七个状态冻结枚举与批准证据条件，L35 将做迁移顺序表驱动校验。
- **L04/L06/L07/L15** 分别做许可证语义、真实哈希、权利一致性、content_id 存在性校验；本 Schema 只约束结构。
- **L11—L14** 冻结分类词典。本 Schema 的 `category` 枚举以真实数据为起点
  （intake 目录 domain 9 个 + 本地就绪类别并集 = 11 个：animal/audio/building/character/environment/fish/lighting_reference/material/prop/ship/ui）。
  **若 L11—L14 冻结出新类别，需在后续任务按基线冲突规则跟进本 Schema 枚举**（L45 做一致性审计）。
- **L21—L24** 来源目录写入时按本 Schema 生成记录；未下载 → `download_date: null`，缺原文件 → `sha256: null`。
- **L40/L41** 将建立正式测试文件；本任务的夹具目录即为其固定负向/正向数据源之一。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| 临时校验脚本（Draft202012Validator.check_schema + 逐夹具断言，脚本在仓库外临时目录，不进入仓库） | PASS：Schema 合法；valid.json VALID；8 个非法夹具分别命中预期字段 |
| 同一脚本内路径边界定向断言（8 越界形态必须拒绝 / 4 相对形态必须接受） | PASS（首轮发现 `/etc/passwd` 前导斜杠放行，修复 pattern 后复验通过） |
| 夹具合成性抽查（前缀/域名/摘要） | PASS：未对真实候选补造数据 |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |
| `python tools/sync_logic_task_status.py` | PASS：D18-L02=LOGIC_PASS，总览重新生成 |

## 明确未宣称

- 不宣称任何资产为 `RuntimeApproved` / `ReleaseApproved`（本阶段禁止，无持有者）。
- 不宣称任何夹具指向真实候选、真实哈希或真实许可证据。
- 静态 Schema 校验 ≠ 可玩、画面完成或 Runtime PASS。
