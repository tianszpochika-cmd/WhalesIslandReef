# D18-L51 交接报告

## 任务信息与结论

- 任务编号：`D18-L51`（L50 验收回退任务：分类/键数据一致性定案）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**（无 P0 blocker）
- 机器结果：`game/tools/results/D18/D18-L51/D18_L51_LOGIC_RESULT.json`；审计 `D18_L51_LOGIC_AUDIT.json`、编译 `D18_L51_COMPILED_CATALOG.json`

## 双层分类契约（唯一工作目标）

1. `category` 永远是 L02 11 值资源大类（未扩大枚举、未删除 `fish` 大类）；
2. 新增可选 `taxonomy_category`：领域细分类，取值必须来自机器可读版本化映射 [d18_taxonomy_category_map.json](../../../tools/schemas/d18_taxonomy_category_map.json)（8 个实际使用细分类，各含 `parent_category`/`owner_document`/`version`）；
3. `taxonomy_category` 存在时映射父类必须等于 `category`；未知细分类/父类不匹配 → P0（L39 与 L42 同语义）；无细分类的旧记录仍合法；
4. 61 条 Quaternius 精确迁移（见下表），原细分类逐条保留，来源/许可/哈希/状态/证据/顺序/总数不变；
5. `fish` 仍为独立大类（真实鱼类记录使用）。

## 61 条迁移计数（精确）

| 原 category | 迁移后 category | taxonomy_category | 条数 |
|---|---:|---|---:|
| terrestrial_animal | animal | terrestrial_animal | 10 |
| aquatic_animal | animal | aquatic_animal | 7 |
| pet | animal | pet | 3 |
| bird | animal | bird | 1 |
| npc_visual | character | npc_visual | 8 |
| player_visual | character | player_visual | 1 |
| hair | character | hair | 7 |
| outfit | character | outfit | 24 |
| **合计** | animal 21 + character 40 | 细分类保留 61 | **61** |

## 验证结果（全部执行并如实记录）

| # | 验证 | 结果 |
|---|---|---|
| 1 | L02 Schema 校验（4 个真实来源 catalog） | **161/161 合法** |
| 2 | `python -m unittest tools.tests.test_d18_logic_audit -v` | 22/22 PASS |
| 3 | `python -m unittest tools.tests.test_d18_logic_catalog_compiler -v` | 23/23 PASS |
| 4 | L02 Schema 测试（fixtures + jsonschema） | VALID/INVALID 均如预期 |
| 5 | `python tools/d18_logic_audit.py --output …/D18_L51_LOGIC_AUDIT.json` | **records=161、P0=0**；P1=4（hash_pending 保留） |
| 6 | `python tools/d18_logic_catalog_compiler.py --output …/D18_L51_COMPILED_CATALOG.json` | **input=161、compiled=161、rejected=0、conflicts=0** |
| 7 | 确定性（两次编译字节一致） | PASS |
| 8 | 全量回归 933 项 | 4 项既有遗留（R01 基线）+ 1 项预期（L45 测试断言旧分裂，见下） |
| 9 | `python tools/sync_logic_task_status.py` | PASS（L51 为 D18 外新编号，不在 sync 50 项注册表；本 JSON 与 handoff 为事实源） |
| 10 | `git diff --check` | exit 0，仅既有 LF→CRLF 警告 |

## 修改文件

- Schema：`d18_logic_asset_record.schema.json`（+可选 `taxonomy_category`，13 必填字段/状态机/Runtime 边界不变）；新建 `d18_taxonomy_category_map.json`；
- 文档：基线 §6.1 字段词典、CHARACTERS/ECOLOGY/UI_AUDIO 双层定义（链接机器映射；数值/所有权/Runtime 延期声明未变）；
- 数据：`d18_quaternius_logic_catalog.json`（仅 category+taxonomy_category 字段，其余语义/顺序/总数/证据不变）；
- 工具：`d18_logic_audit.py`、`d18_logic_catalog_compiler.py`（同语义双层校验）+ 各自测试；
- 夹具：L02 fixtures +2（valid/invalid taxonomy）、README 更新。

## 后续项（记录，不静默跳过）

- **L45 测试更新需求（预期）**：`test_d18_schema_compatibility_audit.py::test_real_audit_blocks_on_known_category_split` 断言旧 category 分裂（8 个 extra categories），L51 迁移后该分裂已消除，断言过时（全量回归如实失败）；L45 测试文件不在本任务允许范围，建议由后续任务更新为「分裂已清零」断言。L45 工具本身对新数据输出 PASS（不再有 category_split）。
- L50 验收文档（`D18_LOGIC_ACCEPTANCE.md`）保持 PARTIAL 为历史记录；基于 L51 结果，章节分类/键 P0 已消除，L50 升级判定属后续验收复核（本任务未修改 L45/L50 正式结果）。
- `D19LightingProfileCatalog.json` 的 `candidate_hdri_*` 引用修复归 L52（本任务未触碰）。

## 未宣称

- 不宣称 4 条 HDRI 已哈希（Pending，不补猜）；
- 不宣称任何资产已导入/`RuntimeApproved`/`ReleaseApproved`（均为 0）；
- 未扩大 L02 枚举、未删除 `fish` 大类、未改写 61 条来源/许可/哈希/状态事实。
