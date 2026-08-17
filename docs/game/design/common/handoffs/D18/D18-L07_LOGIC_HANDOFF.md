# D18-L07 交接报告

## 任务信息与结论

- 任务编号：`D18-L07`（波次 1，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L07/D18_L07_LOGIC_RESULT.json`
- 真实数据扫描证据：`game/tools/results/D18/D18-L07/d18_l07_real_scan.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_distribution_rights_validator.py`（新建，分发权利验证器）
- `tools/tests/test_d18_distribution_rights_validator.py`（新建，27 项测试）
- `game/tools/results/D18/D18-L07/D18_L07_LOGIC_RESULT.json`（新建）
- `game/tools/results/D18/D18-L07/d18_l07_real_scan.json`（新建，真实数据只读扫描证据）
- `docs/game/design/common/handoffs/D18/D18-L07_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema、其它 Python 工具、Unity、Prefab、场景、资源、Registry、Systems、contracts、server、web、既有 manifest/catalog 或旧 D18-A/S/R 历史文件。

## 交付内容

### 1. 四项权利的验证口径（与 D18-L01 基线第 6.2 节字段一致）

| 权利 | 字段 | 显式结论 |
|---|---|---|
| 商业使用 | `commercial_use` | 仅 JSON 布尔：`true`=allowed、`false`=denied、缺失/字符串=unknown |
| 修改 | `modification_allowed` | 同上 |
| 仓库存储 | `source_redistribution` | 仅 `allowed`/`not_allowed`；`pending`/`not_applicable`/无效=unknown |
| 随游戏分发 | `build_distribution` | 同上 |

- 四项各自输出 `value`（allowed/denied/unknown）与 `explicit`，不合并、不推断。
- 证据：`evidence_path` 必须为非空、非绝对、无 `..` 越界的仓库相对路径且文件真实存在；违反一律 `missing_evidence` 阻断。

### 2. 关键规则（全部有测试）

- **免费/可下载不是权利**：`free_claims`、`downloadable`、notes 中的 free/download 字样只被标记为 `claims_guarded`，永不把 unknown 升级为 allowed、永不救回 denied；缺显式字段时以“free/downloadable claims are not permission evidence”阻断。
- **NC/个人使用标记**：license_id（如 `CC-BY-NC-4.0`，隔离 token 匹配，不误伤 CC0/OFL）、notes、claims 中的非商业/个人使用标记 → 商业权利：显式 allowed+标记=conflict；显式 denied=denied；缺失=denied（由文本证据限制，`explicit=false`）。
- **矛盾检测**：同记录内 NC 标记与 allowed 矛盾；跨来源（manifest ↔ intake catalog license 对象）逐权利比对，verdict 不一致即 conflict。
- **迁移阻断**：任何 denied/unknown/conflict/missing_evidence → `migration_allowed=false`，即阻断 Runtime/Release 迁移。
- **只读与离线**：`read_only=true`、`network_access=false`、`approval_status_unchanged=true`；无网络导入，无审批状态写入。

### 3. CLI

- 全量扫描：`python tools/d18_distribution_rights_validator.py --root <repo> [--output <json>] [--no-intake] [--max-records N]`
- 单条记录校验：`--record <rights.json>`（退出码 0=CLEARED，1=BLOCKED）
- 记录输入键：`record_id/source/license_id/license_url/evidence_path/commercial_use/modification_allowed/source_redistribution/build_distribution/free_claims/downloadable/notes`

## 真实数据扫描结果（2026-08-16）

- 1,895 条记录 = manifest 30（packs 28 + manifests 2，含非标准后缀 `D18_A04_R02_C_manifest.json`）+ 准入目录 1,865 条。
- **CLEARED 1,894 / BLOCKED 1 / migration_allowed 1,894**；blocker 类型分布 `unknown: 1`。
- 唯一 BLOCKED：`pack_opengameart_bird_d18_a04_v1`（manifest）——`build_distribution: "pending"` 无显式结论。这是验证器发现的一个**真实数据缺口**：该包（Blender-only source）的随游戏分发权利未证实，其任何 Runtime/Release 迁移都被阻断；需来源目录任务（L21—L24）补证据或按 L19 拒绝/豁免规则处理，属人工/数据审核延期，不是工具缺陷。
- 真实数据中未发现 manifest↔catalog 矛盾；已核对该包不在 intake `eligible_entries`（0 条 opengameart 条目），因此无冲突叠加。

## 明确未宣称

- 任何 CLEARED 记录都没有因此获得 `RuntimeApproved`/`ReleaseApproved`；本逻辑阶段禁止工具授予（基线第 2.6 节）。
- 权利验证不替代逐文件 SHA-256（L06/L31）、Unity 导入验证（L26—L28）、真实运行时消费证据（Runtime 阶段）。
- 旧 D18-A/S/R 结论未被引用为通过依据。

## 待办 / 后续注意事项

- `pack_opengameart_bird_d18_a04_v1` 的 `build_distribution` 缺口：由 L21—L24 来源目录任务或人工审核补证据；L19 拒绝/豁免规则就绪前不自动放行。
- L42 目录编译器可调用本工具（只读）作为 `IntakeEligible` 证据链一环；L39 聚合审计应把本工具 P0 缺口原样透传，不吞错。
- 本工具发现的 manifest 发现口径（30 个，含 `_manifest.json` 后缀）与基线 1.1 节数量一致，可作为 L33 目录声称审计的输入之一。
- 新文档登记到 `docs/README.md` 文档地图：按 D18-L01 handoff 建议由 L20（逻辑资源索引）或文档维护任务统一补齐登记。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python tools/tests/test_d18_distribution_rights_validator.py` | PASS：27/27 OK |
| `python tools/d18_distribution_rights_validator.py --output game/tools/results/D18/D18-L07/d18_l07_real_scan.json` | PASS：1,895 条，CLEARED 1,894 / BLOCKED 1；read_only、无网络、不改审批状态 |
| `python -m py_compile tools/d18_distribution_rights_validator.py tools/tests/test_d18_distribution_rights_validator.py` | PASS |
| `python tools/sync_logic_task_status.py` | PASS：D18-L07=LOGIC_PASS，280 项总览重新生成 |
| `git diff --check` | PASS：exit 0（仅既有 LF→CRLF 警告） |

## 测试覆盖清单（27 项）

全允许（CLEARED/无状态变更）、仅个人（4 项 denied）、个人使用文本标记、NC 三态（denied/conflict/unknown→denied）、CC0 不误判 NC、缺字段 unknown、字符串布尔 unknown、pending/not_applicable 等 unknown、normalizer 三值区分、free 声明不升级/不救回、downloadable 不升级、notes free 标记、多源矛盾、矛盾阻断迁移、缺证据四态（null/文件缺失/绝对路径/.. 越界）、manifest/intake 字段映射、真实 manifest 30 与 intake 1,865 计数、无网络导入检查。
