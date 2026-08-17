# D18-L16 交接报告

## 任务信息与结论

- 任务编号：`D18-L16`（波次 1，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L16/D18_L16_LOGIC_RESULT.json`
- 真实数据扫描证据：`game/tools/results/D18/D18-L16/d18_l16_real_key_scan.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_resource_key_validator.py`（新建，ResourceKey 验证器）
- `tools/tests/test_d18_resource_key_validator.py`（新建，33 项测试）
- `game/tools/results/D18/D18-L16/D18_L16_LOGIC_RESULT.json`（新建）
- `game/tools/results/D18/D18-L16/d18_l16_real_key_scan.json`（新建，真实历史 key 只读扫描证据）
- `docs/game/design/common/handoffs/D18/D18-L16_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema、其它 Python 工具、Unity、Prefab、场景、资源、Registry、catalog、Systems、contracts、server、web 或旧 D18-A/S/R 历史文件。

## 交付内容

### 1. 验证维度（六项，全部有正负夹具）

| 维度 | 规则 | 拒绝 kind |
|---|---|---|
| 唯一性 | 同一 key 只能由一条记录声明 | `duplicate_key`、`cross_category_collision` |
| 稳定命名 | 裸键 `^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$`；命名空间键 `domain:category:content_id`（冻结 namespace：`d10`/`d20`）；禁双下划线/前导尾随下划线/连字符 | `invalid_format`、`unity_guid`（32 位 hex） |
| 弃用别名 | `deprecated_alias_of` 目标必须存在、非自引用、无环、类别一致；别名目标自身无效 → P1 | `deprecated_alias_of_target_missing/self_reference/cycle/category_mismatch`、`alias_cycle`、`deprecated_alias_of_target_invalid` |
| 类别匹配 | 类别必须命中 L02 冻结枚举（11 类）；前缀→类别映射 26 条（含 `asset_*` 通配、D20 `wildlife_*`/`npc_*` 展示键）；命名空间键尾段须匹配 `content_id` | `unknown_category`、`unknown_prefix`、`category_prefix_mismatch`、`content_id_mismatch` |
| 迁移规则 | `migrated_from` 源必须存在、非自引用、无环、类别一致 | `migrated_from_target_missing/self_reference/cycle/category_mismatch`、`migration_cycle` |
| 禁用输入 | 磁盘路径（正/反斜杠、盘符、`..`、`Assets/`）、文件扩展名（点）、显示名（空格/大写/非 ASCII/`content.` localization key/display_name==key）、空 key | `disk_path`、`file_extension_or_path`、`display_name`、`display_name_key`、`display_name_as_key`、`empty_key` |

### 2. 关键口径（与 D18-L01/L11/L12/L13 一致）

- **key 独立于二进制资源/Unity GUID**：验证器纯数据输入输出，从不 stat 资源文件、从不要求资产存在；报告固定输出 `key_independent_of_binary=true`、`registry_unchanged=true`、`runtime_references_unchanged=true`、`read_only=true`。
- 逻辑 ResourceKey 格式 `d10:appearance:<content_id>`（L12 3.2）被支持为命名空间键；运行时 Unity Resources 路径（`D10/Appearance/...`）是 Runtime 阶段实现细节，本工具不冻结不声称。
- 历史候选前缀 `candidate_`/`d18a05_` 只允许出现在 `deprecated_alias_of`（历史证据），禁止作活动键（L11 第 2 节"可被引用但不新增"）。
- 类别枚举与前缀映射沿用 L02 Schema 冻结值与 L11—L14 分类词典；L45 一致性审计可解析本工具常量或通过 `--categories`/`--prefix-map` 注入覆盖。

### 3. CLI

- `python tools/d18_resource_key_validator.py --records <records.json> [--output <json>] [--categories <json>] [--prefix-map <json>]`
- 退出码 0=CLEARED，1=BLOCKED；记录字段：`key`（必填）、`category`、`content_id`、`display_name`、`deprecated_alias_of`、`migrated_from`、`record_id`、`source`。

## 真实数据扫描结果（2026-08-16）

- 从 D20-A01/A03/A05/A06 历史机器结果（只读数据源，SUPERSEDED 证据，不作通过条件）提取 29 个 `resource_key`。
- 全部 29 个 key 以正确类别（npc_*→character、wildlife_*/pet_*→animal、asset_*→character）验证：**CLEARED，issue_kind_counts={}**。
- 唯一历史候选键 `candidate_superhero_female_fullbody_r03p`：单独验证时被 `candidate_prefix_active` 阻断；声明 `deprecated_alias_of=asset_d20_player_humanoid_avatar_female_r01` 后通过——与"候选前缀只允许作弃用别名"口径一致。
- 若把 29 键全部作为活动键验证（不加别名标记），结果确定性地为 `candidate_prefix_active: 1`（单测固化该行为）。

## 明确未宣称

- 未修改 ContentRegistry、`asset_state`、存档 Schema、任何运行时引用或历史 D20 结果。
- 未把任何历史/真实 key 升级为正式运行键；CLEARED 只表示键记录自身合法。
- 未引用旧 D18-A/S/R 结论作为通过依据（D20 历史结果仅作只读数据来源，handoff 已标注）。

## 待办 / 后续注意事项

- L31（重复身份）/L32（名称碰撞）可复用本工具的唯一性与类别规则；L42 编译器与 L44 游戏引用审计消费本工具输出。
- 若 L11—L14 后续修订前缀/类别映射，本工具常量需同步（或通过 `--prefix-map` 注入），L45 负责一致性核对。
- 真实 key 的二进制存在性与 Runtime 路径验证属 Runtime 阶段，不在本工具。
- 新文档登记到 `docs/README.md` 文档地图：按 D18-L01 handoff 建议由 L20（逻辑资源索引）或文档维护任务统一补齐。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python tools/tests/test_d18_resource_key_validator.py` | PASS：33/33 OK |
| `python tools/d18_resource_key_validator.py --records <29 真实 key> --output game/tools/results/D18/D18-L16/d18_l16_real_key_scan.json` | PASS：CLEARED，29/29，退出码 0 |
| `python -m py_compile tools/d18_resource_key_validator.py tools/tests/test_d18_resource_key_validator.py` | PASS |
| `python tools/sync_logic_task_status.py` | PASS：D18-L16=LOGIC_PASS，280 项总览重新生成 |
| `git diff --check` | PASS：exit 0（仅既有 LF→CRLF 警告） |

## 测试覆盖清单（33 项）

合法裸键/命名空间键、路径四态（正斜杠/反斜杠/盘符/../Assets/）、扩展名三态、显示名四态（空格/非 ASCII/localization key/大写）、空键三态（空串/空白/null）、GUID、双下划线/尾随下划线/连字符/空格字符集、候选前缀活动键、全允许记录集 CLEARED、独立性与只读声明、重复 key、跨类别碰撞、未知类别、前缀类别不匹配、未知前缀、路径 key、display_name==key、content_id 不匹配、合法别名、别名目标缺失/自引用/环/跨类别、候选前缀作别名合法、合法迁移、迁移源缺失/环/跨类别、真实 29 键确定性分类（活动态与别名态两断言）、冻结枚举一致性、无网络导入。
