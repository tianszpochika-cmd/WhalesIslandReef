# D18-L30 交接报告

## 任务信息与结论

- 任务编号：`D18-L30`（波次 2，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L30/D18_L30_LOGIC_RESULT.json`
- 交付物：`tools/d18_asset_dependency_graph.py`、`tools/tests/test_d18_asset_dependency_graph.py`

## 实际修改文件（唯一写入范围）

- `tools/d18_asset_dependency_graph.py`（新建，只读资源依赖图工具）
- `tools/tests/test_d18_asset_dependency_graph.py`（新建，42 项单测）
- `game/tools/results/D18/D18-L30/D18_L30_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L30_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改任何 Schema（含 L25 预定位置）、其他任务工具/测试、catalog、manifest、Unity、Prefab、场景、Registry、contracts、server、web 或既有 D18-A/S/R 历史材料。

## 工具职责与输入契约

从**静态 JSON**（L02 形态 asset record + import-descriptor 式文档 + 可选包不兼容声明）构建稳定有向依赖图，检测：

| 错误码 | 语义 | 判定依据 |
|---|---|---|
| `cycle` | 依赖循环（>1 节点强连通分量 + 自环） | Tarjan SCC |
| `dangling_reference` | 边目标不是已声明节点 | 节点表比对 |
| `cross_package_incompatibility` | 边跨越全局互斥包对，或源节点声明目标包不兼容仍依赖 | `--packages` 文件 `incompatible_pairs`（无序对归一化）/ 节点 `incompatible_packages` |
| `state_skip_edge` | 边违反冻结状态机禁止对 | `D18_ASSET_STATE_MACHINE.md` §4.2/4.4 展开：Candidate→RuntimeRequested/RuntimeApproved/ReleaseApproved、IntakeEligible→RuntimeApproved/ReleaseApproved、RuntimeRequested→ReleaseApproved、ReleaseApproved→Rejected、Rejected→任何、除 T4 外任何状态→ReleaseApproved；T4 `RuntimeApproved→ReleaseApproved` 显式放行 |
| `missing_license` / `missing_license_evidence` | 节点无 license 对象 / license_id 未指定（含 unknown/pending/空）/ 缺 `license_evidence_path` | L02 Schema license 语义 |

输入契约（工具 docstring 固定声明）：
- `--records`：L02 形态记录（`canonical_asset_id`/`state`/`license` + 可选 `package`/`manifest_id`/`pack_id`/`incompatible_packages`/`dependencies` 扩展；L02 Schema 本身 `additionalProperties:false`，图输入可携带边声明作为文档化扩展）；
- `--descriptors`：**L30 图输入契约，不是 L25 Schema**——对象含 `asset_id`（必填）+ 可选 `package`/`state`/`license`/`incompatible_packages`/`dependencies`（字符串或 `{"asset_id":...}` 条目，支持 `references` 别名）。L25（`tools/schemas/d18_import_descriptor.schema.json`）目前 PLANNED 未交付，字段级引用（材质槽/骨架/动画/bounds 等）收割与对齐明确留给波次 3/L45；本任务未复制或发明 L25 字段定义；
- `--packages`：`{"incompatible_pairs": [["pack_a","pack_b"], ...]}`，每对须两个不同非空字符串，无序归一化。

输出：固定 JSON（节点/边/循环/问题全部排序；`fingerprint` = 规范序列化的 SHA-256，稳定可测）；`approved_count` 恒 0、`catalog_modified=false`；exit 0=PASS、2=BLOCKED（输入文件错误与空图同样 BLOCKED）。

## 与既有规范的一致性

- 状态枚举与禁止对逐条取自冻结的 `D18_ASSET_STATE_MACHINE.md`（L03，§3/§4.2/§4.4），T4 例外显式放行并写测试；
- 只读口径与 L04—L18 系列验证器一致：不读/不哈希二进制、不导入、不写 catalog、不授予任何状态；
- 确定性口径与 L15/L16 一致：排序 + fingerprint，重复运行结果逐字节一致。

## 验证（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `python -m py_compile`（两个新文件） | PASS |
| `python -m unittest tools.tests.test_d18_asset_dependency_graph` | PASS：42/42 |
| CLI 冒烟（仓库外临时目录，正负夹具） | PASS：正向 exit 0；负向 exit 2，检出 cycle/dangling_reference/missing_license/state_skip_edge；两次运行输出一致、输入文件哈希不变 |
| 全量套件 `python -m unittest discover -s tools/tests` | 671 项运行，8 项失败且全部写入范围外：`test_d18_local_development_guard` 4 项（D18-L36 并行任务自身在途测试）、`test_d18_s02_r01_intake_refresh` 4 项（基线已声明 SUPERSEDED 断言） |
| `python tools/sync_logic_task_status.py` | PASS：D18-L30=LOGIC_PASS，280 项总览重新生成 |
| `git diff --check` | PASS：exit 0（仅既有 LF→CRLF 警告） |

## 测试覆盖（42 项）

- 正向：无环图 PASS、确定性（fingerprint+逐字节）、重复边去重、record+descriptor 同属性合并、T4 合法边、全合法状态链（Candidate→IntakeEligible→RuntimeRequested→RuntimeApproved→ReleaseApproved）、同包边放行、未知状态节点不出现于输出 state 字段、冻结状态枚举断言、输入只读比对；
- 负向：2 节点/3 节点环、自环、悬挂目标、边目标空、非法边条目、边字段非数组、全局互斥包对（两种顺序）、节点级不兼容声明、Candidate→RuntimeRequested/RuntimeApproved/ReleaseApproved、IntakeEligible→RuntimeApproved/ReleaseApproved、RuntimeRequested→ReleaseApproved、Rejected 出边、ReleaseApproved→Rejected、未知状态、缺 license、未指定 license_id（空/unknown/pending）、缺证据路径、descriptor 缺许可、重复节点、record/descriptor 状态冲突、空图、非法记录、输入错误 BLOCKED、文件形状/缺失/损坏、packages 文件形状与非法对。

## 明确未宣称

- 未把 L30 图输入契约当作 L25 import descriptor Schema；L25 未交付，字段级收割未做，如实登记 Runtime/波次 3 延期；
- 未对真实 catalog 全量建图（L21—L24 来源目录未交付），未修改任何 catalog/manifest 数据；
- 未授予 RuntimeApproved/ReleaseApproved（二者持有者仍为 0）；本工具不产生任何批准。

## 待办 / 后续注意事项

- L25 交付后：建议由 L45（Schema 兼容审计）或波次 3 任务将 descriptor 字段级引用映射为本工具边输入，并对齐字段名；
- L21—L24 来源目录交付后：可用 `--records` 全量跑真实 catalog，跨包不兼容事实对账由 L33/L34 或波次 3 执行；
- L39（聚合审计）如需调用本工具，CLI 接口（`--records/--descriptors/--packages/--output`）与输出 JSON 结构即为契约。

## 附录：CLI 用法

```powershell
python tools/d18_asset_dependency_graph.py `
  --records <records.json> [--descriptors <descriptors.json>] `
  [--packages <packages.json>] [--output <result.json>]
```
