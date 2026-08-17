# D18-L32 交接报告

## 任务信息与结论

- 任务编号：`D18-L32`（波次 2，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L32/D18_L32_LOGIC_RESULT.json`
- 真实数据扫描证据：`game/tools/results/D18/D18-L32/d18_l32_real_records.json` + `d18_l32_real_scan.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_name_collision_audit.py`（新建，命名冲突审计）
- `tools/tests/test_d18_name_collision_audit.py`（新建，28 项测试）
- `game/tools/results/D18/D18-L32/D18_L32_LOGIC_RESULT.json`（新建）
- `game/tools/results/D18/D18-L32/d18_l32_real_records.json` / `d18_l32_real_scan.json`（新建，真实数据只读扫描证据）
- `docs/game/design/common/handoffs/D18/D18-L32_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema、其它 Python 工具、Unity、Prefab、场景、资源、Registry、catalog、Systems、contracts、server、web 或旧 D18-A/S/R 历史文件。

## 交付内容

### 1. 检查维度（输入字段：canonical_id / resource_keys / content_ids / display_names / original_filenames / original_paths / category / deprecated_alias_of）

| 严重度 | 冲突 kind | 说明 |
|---|---|---|
| **P0 阻断** | `duplicate_canonical_id` | 跨记录重复 canonical_id |
| P0 | `cross_field_collision` | canonical_id 与其它记录的 resource_key/content_id 相同；content_id 与其它记录 canonical_id/content_id 相同 |
| P0 | `case_collision` | id-like 值仅有大小写差异（Registry 为小写 snake_case 口径） |
| P0 | `same_path_duplicate` | 同一完整路径（含 `\` 归一化为 `/`）被多条记录声明 |
| P0 | `alias_cycle` / `alias_self_reference` | 弃用别名环（二节点/三节点链均测）或指向自己 |
| P1 卫生 | `id_equals_display_name` / `key_equals_display_name` | 逻辑 id/Key 与其自身显示名相同 |
| P1 | `id_equals_filename` | canonical_id 与带扩展名的原始文件名相同 |
| P1 | `id_looks_like_display_name` | 某记录逻辑 id 被另一记录复用为显示名 |
| P1 | `alias_target_not_in_set` | 别名目标不在记录集合中（不阻断，提示补登记） |
| P2 提示 | `same_display_name_same_category` | 同类别多条记录共用显示名（不阻断，人工确认） |

### 2. 合法情形（有测试防误报）

- 记录内 `canonical_id == 自身 resource_key`：合法（key 由 id 派生），不报冲突。
- **跨类别同显示名：合法**（如两个不同分类的 "Harbor Bell"），不产生任何条目。
- 同 basename（`rock.fbx`）在不同路径：合法。
- 合法弃用别名链（旧→新，无环）：通过。
- 仅 P1/P2 时 verdict=CLEARED（审计不阻断）；任何 P0 → BLOCKED。

### 3. 推荐动作（每条冲突携带）

静态动作文案（中文，机器可读）：保留单登记+别名指向（append-only）；合并登记或按 L31 重复身份处理；规范小写唯一值；人工消除别名环；display_name 使用本地化 key（`content.*`）等。**工具从不自动重命名、不修改 Registry**（报告恒有 `no_auto_rename=true`、`registry_unchanged=true`、`read_only=true`）。

### 4. 确定性

报告内所有列表排序（pair/fields/value/kind/severity 多键排序）；`deterministic=true`；单测用乱序输入断言两次输出深度相等。

## 真实数据扫描结果（2026-08-16）

- 记录构造：30 个真实 manifest（pack_id/title/category）+ intake 目录 1,865 条按 `content_id` 分组为 1,097 个内容记录（distinct 路径、domain 作类别）＝ **1,127 条**。
- 结果：**CLEARED，conflict_count=0，P0/P1/P2 全 0**——真实准入数据无跨内容 id 碰撞、无跨分组路径重复、无大小写变体、manifest 标题无重复。
- 数据形状教训（记录在案，非工具缺陷）：初版把 manifest 的 candidate_files 与 content 分组路径同时作为记录输入，同一物理文件被两个"视图"重复声明，产生 1,865 个假 `same_path_duplicate`；改为**每条记录 = 一个逻辑资源**（单粒度）后归零。审计输入约定：pack 与其内容映射行不是两个资源，使用者须先归并再审计。

## 明确未宣称

- 未重命名、未修改 Registry、未改变任何状态；`RuntimeApproved`/`ReleaseApproved` 持有者仍为 0。
- 冲突裁定与登记变更属人工或 L42 目录编译器；本工具只输出建议动作。
- 未引用旧 D18-A/S/R 结论作为通过依据（真实扫描仅使用准入目录与 manifest 的只读字段）。

## 待办 / 后续注意事项

- L31（重复身份审计，哈希级）与 L44（游戏引用审计）是相邻防线；L42 编译器可在登记前调用本工具（只读）过滤 P0 冲突。
- 本工具与 L16（ResourceKey 验证器）边界：L16 验证单键格式/类别/别名语义；L32 审计多字段跨记录碰撞与别名环，二者互补不重叠。
- 真实扫描为静态快照；L21—L24 来源目录产出后应重跑本工具（记录形状已按单资源粒度约定）。
- 新文档登记到 `docs/README.md` 文档地图：按 D18-L01 handoff 建议由 L20（逻辑资源索引）或文档维护任务统一补齐。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python tools/tests/test_d18_name_collision_audit.py` | PASS：28/28 OK |
| `python tools/d18_name_collision_audit.py --records <1127 真实记录> --output <real_scan.json>` | PASS：CLEARED、0 冲突、退出码 0 |
| `python -m py_compile tools/d18_name_collision_audit.py tools/tests/test_d18_name_collision_audit.py` | PASS |
| `python tools/sync_logic_task_status.py` | PASS：D18-L32=LOGIC_PASS，280 项总览重新生成 |
| `git diff --check` | PASS：exit 0（仅既有 LF→CRLF 警告） |

## 测试覆盖清单（28 项）

干净集合、id==自身 key 合法、大小写变体 P0（canonical/resource_key 两例）、显示名大小写 P2、合法别名链、二/三节点别名环、别名自引用、别名目标缺失 P1、重复 canonical P0、canonical↔key 跨字段 P0、content↔canonical 跨字段 P0、id 复用为显示名 P1、跨类别同显示名合法、同类别同显示名 P2、三记录共享显示名 P2、同 basename 不同路径合法、同完整路径 P0、反斜杠归一化 P0、id==文件名 P1、id==显示名 P1、key==显示名 P1、记录内 id/key 相同不误报、确定性（乱序输入）、只读/不重命名声明、冲突条目四要素、无网络导入。
