# D18-L10 交接报告

## 任务信息与结论

- 任务编号：`D18-L10`（波次 1，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L10/D18_L10_LOGIC_RESULT.json`
- 真实扫描证据：`game/tools/results/D18/D18-L10/D18_L10_REAL_SCAN_EVIDENCE.json`

## 实际修改文件（唯一写入范围）

- `tools/d18_package_boundary_validator.py`（新建，只读包边界验证器）
- `tools/tests/test_d18_package_boundary_validator.py`（新建，33 项正负单测，临时目录构造 fixture，不新增 fixture 文件）
- `game/tools/results/D18/D18-L10/D18_L10_LOGIC_RESULT.json`（新建）
- `game/tools/results/D18/D18-L10/D18_L10_REAL_SCAN_EVIDENCE.json`（新建，真实仓库扫描输出）
- `docs/game/design/common/handoffs/D18/D18-L10_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema、既有 manifest/catalog、Unity、Prefab、场景、Systems、contracts、server、web、AgentCore 或旧 D18-A/S/R 历史文件。

## 实现内容（包边界检查语义，冻结在本工具文档串中）

1. **可追溯性**：`candidate_files[]` 是 pack 的声明文件清单（无清单 → `manifest_without_file_list`）；`--records` 传入的资源记录（如准入目录 `eligible_entries`）必须：`manifest_id` 存在于已加载 manifest（否则 `record_manifest_missing`）、`manifest_path` 与 manifest 实际路径一致（`record_manifest_path_mismatch`）、`candidate_file_path` 被该 manifest 声明（`record_file_not_declared_by_pack`）、`license.license_evidence_path` 与该 pack 声明的许可证证据一致（`record_evidence_mismatch` / `record_evidence_missing`）。
2. **相对路径**：所有声明路径必须是仓库内安全相对路径，拒绝绝对路径、盘符/UNC、`..`、URL、环境变量与忽略缓存段（复用 D18-L08 冻结的路径边界 `validate_path`，跨端一致）；解析必须落在项目内。
3. **包内许可证证据**：`license_evidence_path` 必须位于冻结候选根 `game/Assets/Art/Candidates/` 内（否则 `license_evidence_outside_pack`）、位于 pack 自身分类子树（首个 Candidates 子段一致，否则 `evidence_category_mismatch`）、是真实非空文件（否则 `license_evidence_missing`）、不得等于本包候选文件（`evidence_collides_with_candidate`）。
4. **禁止重复归属**：同一规范化路径（大小写折叠、分隔符归一）不得被两个 pack 声明——不同来源 → `file_declared_by_incompatible_sources`；同源跨包 → `file_declared_by_multiple_packs`；清单内重复 → `duplicate_file_in_manifest`；证据被两包共享 → `evidence_shared_across_packs`；证据指向另一 pack 的候选文件 → `evidence_collides_with_other_pack_file`（ReleaseApproved 的 `release_evidence_path` 同规则）。
5. **只读**：仅解析 manifest/记录 JSON 与 `stat` 文件存在性/大小；不读取、不哈希、不导入候选二进制，不审批资产，不写 registry，不访问网络，不移动/删除文件（`read_only: true`、`registry_modified: false`，单测含前后快照比对）。

## 真实仓库扫描结果（证据 JSON 与本文一致）

- 30 个 manifest（`assets/packs/**/*.manifest.json` 28 + `assets/manifests/` 2）：**20 通过 / 10 失败**。
- 失败原因单一：`license_evidence_outside_pack` × 10——全部 10 个 polyhaven 系包（environment 8 个 sand/coral 材质 + `pack_polyhaven_hdri_sky_references_v1` + `pack_polyhaven_ui_materials_selected_v1`）的许可证证据指向 `docs/game/design/common/assets/packs/...` 而非包内 `game/Assets/Art/Candidates/`。工具按任务卡要求正确"许可证不在包内 → 失败"。
- 1,865 条 `eligible_entries` 记录全部可追溯到其 pack（record failed=0）。
- 无重复归属、无缺文件、无路径逃逸、无缺清单。
- 该 10 个包的修复属修改 A 系列 manifest（基线第 5.3 节全章禁区），本任务不越权修复；已记录为 runtime_deferred 与后续任务输入（L33/L34/L42）。

## 接口（供 L40/L41/L42 等消费）

- `validate_pack_boundaries(project_root: Path, manifest_paths: Iterable[Path], records: list | None = None) -> dict`：确定性 JSON 结果（`status` PASS/BLOCKED、`manifest_count`、`manifest_files`、`pack_summary{total,passed,failed,failed_pack_ids,passing_pack_ids}`、`error_count`、`issue_count`、`issue_counts`、`issues[{code,message,severity,manifest,pack_id,related_pack_ids?}]`、可选 `record_count`/`record_summary`）。
- CLI：`python tools/d18_package_boundary_validator.py --manifest-root <dir> --manifest <file>... --records <catalog.json> [--output <json>]`；exit 0 = PASS、exit 2 = BLOCKED；`--output` 写报告文件（对资源零写入）。
- 依赖：`tools/d18_asset_path_validator.py`（D18-L08，路径边界唯一事实源）。

## 验证命令与结果

| 命令 | 结果 |
|---|---|
| `python -m unittest discover -s tools/tests -p "test_d18_package_boundary_validator.py"` | PASS：33/33 |
| 真实仓库扫描 CLI（30 manifest + 1,865 records） | BLOCKED=10（polyhaven 证据包外，工具正确检出），记录 0 失败 |
| `python -m unittest discover -s tools/tests -p "test*.py"`（全量） | 384 tests / 10 failures：4 项为基线已记录的 SUPERSEDED `test_d18_s02_r01_intake_refresh`；6 项为并行任务 L15/L16 自身在途测试（`test_d18_content_id_asset_link_validator` 1、`test_d18_resource_key_validator` 5），不在本任务写入范围，未越权修复 |
| `python tools/sync_logic_task_status.py` | PASS：D18-L10=LOGIC_PASS |
| `git diff --check` | PASS：exit 0，无空白错误 |

## 待办 / 后续注意事项

- 10 个 polyhaven 包证据外置是真实边界违规，L33/L34 应静态对账，L42 编译器门禁不得将其编译为 `IntakeEligible` 前需证据入包；当前这些包的 intake 记录仍可追溯（记录级 0 失败）但包级 BLOCKED。
- L15/L16 的 6 项在途测试失败属并行任务自身状态，非 L10 门禁；L40/L41 建立 L 系列统一测试时需重新核对。
- 本工具不校验逐文件哈希（L06/L31）、内容映射（L15）、署名（L09）与权利（L07）——边界职责分离，避免重复归属。

## 明确未宣称

- 未宣称任何资产 `RuntimeApproved`/`ReleaseApproved`（仍为 0）；未宣称 polyhaven 证据问题已修复；未把静态扫描写成可玩或运行时通过。
