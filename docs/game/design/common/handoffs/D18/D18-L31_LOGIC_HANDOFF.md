# D18-L31 交接报告

## 任务信息与结论

- 任务编号：`D18-L31`（波次 2，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L31/D18_L31_LOGIC_RESULT.json`
- 真实数据扫描证据：`game/tools/results/D18/D18-L31/d18_l31_real_scan.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_duplicate_identity_audit.py`（新建，只登记式重复身份审计）
- `tools/tests/test_d18_duplicate_identity_audit.py`（新建，27 项测试）
- `game/tools/results/D18/D18-L31/D18_L31_LOGIC_RESULT.json`（新建）
- `game/tools/results/D18/D18-L31/d18_l31_real_scan.json`（新建，真实 2,196 条记录扫描证据）
- `docs/game/design/common/handoffs/D18/D18-L31_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema、manifest、catalog、资源、Unity、Prefab、场景、Registry、Systems、contracts、server、web 或旧 D18-A/S/R 历史文件。未删除、重命名、移动任何资源。

## 实现内容

`tools/d18_duplicate_identity_audit.py` 按四个身份维度（`sha256`、`source_id`、`pack_id/canonical_id`、`original_filename`）检测并输出三个可复核分类，每个发现携带触发它的完整字段值（sha256、source、pack、文件名、canonical_ids、成员路径）：

| 分类 | kind | 判定规则 | 处置 |
|---|---|---|---|
| **精确重复** | `byte_identical_copy` | 同 hash + 同 pack + 同 original_filename，不同路径/canonical_id（状态机 4.5 拒绝条件 #3 语义：哈希相同 canonical_id 不同） | `PENDING_ADJUDICATION`，只登记 |
| | `duplicate_record` | 同一输入内同路径同 hash 重复声明 | 同上 |
| **疑似重复** | `renamed_copy` | 同 pack 内同 hash 但 original_filename 不同 | 同上 |
| | `path_hash_conflict` | 同一路径被声明为不同 sha256（跨输入也算） | 同上 |
| | `canonical_identity_collision` | 同一 canonical_id 被不同内容哈希的文件声明 | 同上 |
| **合法变体** | `legal_mirror` | 同 hash 但跨 pack/来源登记（含垂直切片镜像与跨来源共享） | `OBSERVATION_ONLY`，绝不写"已修复" |
| | `named_variant` | 同 source+pack+文件名但 hash 不同（版本/分辨率/换色变体） | 同上 |

红线落实：

- **只登记，绝不处理资源**：结果恒含 `fixed_count: 0`、`files_deleted: 0`、`files_renamed: 0`、`files_moved: 0`、`registrations_only: true`、`read_only: true`；无删除/重命名/移动/网络/导入/Unity 操作。
- **成功状态保证**（单测固化）：同名不同 hash → 落入 `named_variant` 合法变体（状态 CLEARED，不进重复列表）；同 hash 合法镜像 → 落入 `legal_mirror` 合法变体（不进精确/疑似列表）。两类均 `not_fixed: true`，任何情况下都不把"同名不同 hash"或"同 hash 镜像"写成"已修复"。
- 跨输入同路径同 hash → 合并为一条记录（`co_declared_paths` 登记来源），不误报为重复；跨输入同路径异 hash → 登记 `path_hash_conflict`。
- 输入形态兼容 L06 三种 JSON（数组/`{"records":[...]}`/A05 `{"packages":[...]}`）+ 新 `--manifests`（目录/文件/glob），manifest 候选文件逐文件真实 SHA-256 计算；缺文件 → PENDING 记录进入 `missing_files`（hash 未知，排除出分组，不猜不补）。
- 比较口径：sha256 大小写归一；source/pack/文件名 casefold 比较；路径反斜杠归一。输出确定性（重跑 SHA-256 一致）。
- 状态语义：`CLEARED`（无精确/疑似重复）/ `DUPLICATES_REGISTERED`（≥1 精确或疑似，只登记）/ `BLOCKED`（输入错误）；CLI 退出码 0/1/2。

## 真实数据扫描结果（2026-08-16）

输入：A05 `D18_A05_FILE_HASHES.json`（302 条历史逐文件哈希，L06 已实证，只读引用）+ `docs/game/design/common/assets/packs/**/*.manifest.json` + `manifests/*.manifest.json`（30 个 pack manifest，1,894 条候选文件逐文件真实哈希）。

- 共 2,196 条有效记录，缺文件 0；`co_declared_paths=151`（A05 FILE_HASHES 与 ships_port_props manifest 同路径同 hash 合并，补全 source_id/pack_id/canonical_id 后单条登记，无假重复）。
- `status=DUPLICATES_REGISTERED`，`counts={exact_duplicate:20, suspected_duplicate:104, legitimate_variant:107}`，`issue_counts={}`。
- **精确重复 20**（byte_identical_copy）：Kenney UI 包将同一组灰色 UI 资产字节级复制到 Blue/Green/Grey/Red/Yellow 五个颜色目录（如 `slide_vertical_grey_section_wide.png` 等），同 hash 同文件名同包，仅路径不同 → 全部登记待裁定（疑似为供应商包内有意结构，最终去留属人工/管线裁定，本工具不裁决）。
- **疑似重复 104**：`canonical_identity_collision` 101（同一 UI content_id 被 10 个不同颜色/尺寸文件声明不同哈希，如 `arrow_basic_e.png`）、`renamed_copy` 3。
- **合法变体 107**：`legal_mirror` 7（垂直切片 Kenney 文件与完整包逐字节一致的真实镜像，如 `Candidates/Kenney/PirateKit/Models/rocks-sand-a.fbx` == `Candidates/ShipsPortProps/KenneyPirateKit/Models/rocks-sand-a.fbx`）、`named_variant` 100（同名字不同颜色/尺寸内容）。
- 全程 `fixed_count=0`、`files_deleted/renamed/moved=0`；两次重跑输出 SHA-256 完全一致（确定性）。

## 明确未宣称

- 未对任何发现做最终裁定（Pending→Rejected/保留），未产生任何状态迁移或 `RuntimeApproved/ReleaseApproved` 授予；裁定按 L19 第 5 条流程归人类/后续门禁。
- 未把 manifest 逐文件哈希声明为"完整性通过"（完整性属 L06/L33/Runtime）；本工具仅用真实磁盘哈希做身份对账。
- 未修改 A05 FILE_HASHES、任何 manifest、intake catalog、资源文件或历史 D18-A/S/R 证据。

## 待办 / 后续注意事项

- L32（名称碰撞）可复用本工具 `named_variant`/`canonical_identity_collision` 分组证据；L39/L45 可校验本工具字段与共享词典一致性。
- L42 编译器与 L19 流程消费 `PENDING_ADJUDICATION` 列表做重复裁定入口（基线 2.4 拒绝条件 #3、L19 第 4 节 #5）。
- Kenney UI 颜色目录的 20 组精确重复若人工裁定为"供应商包内有意结构"，应在目录/资源记录中显式标记，避免后续审计重复登记。
- 其余目录（D19—D22 新增候选）的重复身份审计可复用本工具 `--manifests` 入口增量扫描。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python tools/tests/test_d18_duplicate_identity_audit.py` | PASS：27/27 OK |
| `python -m py_compile tools/d18_duplicate_identity_audit.py tools/tests/test_d18_duplicate_identity_audit.py` | PASS |
| `python -m unittest discover -s tools/tests -p 'test*.py'` | 738 项：734 通过、4 失败（全部为既有 SUPERSEDED `test_d18_s02_r01_intake_refresh.py` 历史断言，基线 1.3 节已知，非 L 门禁） |
| `python tools/d18_duplicate_identity_audit.py --records .../D18_A05_FILE_HASHES.json --manifests docs/game/design/common/assets/packs --manifests docs/game/design/common/assets/manifests --output game/tools/results/D18/D18-L31/d18_l31_real_scan.json` | PASS：2,196 条、DUPLICATES_REGISTERED（20/104/107）、退出码 1；确定性重跑 SHA-256 一致 |
| `python tools/sync_logic_task_status.py` | PASS：D18-L31=LOGIC_PASS |
| `git diff --check` | PASS：exit 0（仅既有 LF→CRLF 警告） |

## 测试覆盖清单（27 项）

byte_identical_copy 精确重复（含 canonical_id 不同与处置标记）、duplicate_record、renamed_copy、canonical_identity_collision、path_hash_conflict（含跨输入）、同名不同 hash→named_variant 合法变体且 CLEARED 不写已修复、同 hash 跨来源→legal_mirror 不写已修复、同源跨包→legal_mirror、大小写不敏感、跨输入同路径同 hash 合并（co_declared 与字段补全）、跨输入同路径异 hash 冲突、manifest 真实哈希计算/record_id/缺文件 PENDING 排除、非法 manifest、缺 sha256/非法格式/空路径/路径越界/非对象/空记录、load_records 三形态、只读不触碰源文件与恒 0 标志、确定性输出、CLI 退出码 0/1/2 与输出文件、manifests 未找到 BLOCKED、无网络导入、唯一记录 CLEARED 零发现。
