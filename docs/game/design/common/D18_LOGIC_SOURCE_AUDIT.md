# D18 逻辑来源审计（D18-L43，L53 收尾重跑版）

> 任务：`D18-L43`（波次 3，第 5 项）
> 原始发布：2026-08-16；**L53 收尾重跑：2026-08-17**（L51 双层分类契约 + L52 配置键修复后，全部数字来自本次机器输出）
> 性质：**只读审计**。未补下载、未修改任何源目录、未把 Pending 说成 Ready。

## 1. 方法（命令、输入路径、机器结果）

| 工具 | 命令 | 输入 | 机器结果（本次重跑） |
|---|---|---|---|
| L39 聚合审计 | `python tools/d18_logic_audit.py --output game/tools/results/D18/D18-L53/D18_L53_L39_AUDIT.json` | `docs/game/design/common/assets/sources/d18_*_logic_catalog.json`（4 个） | [D18_L53_L39_AUDIT.json](../../../game/tools/results/D18/D18-L53/D18_L53_L39_AUDIT.json)（同目录 `D18-L43/D18_L43_L39_AUDIT.json` 已同步） |
| L42 目录编译 | `python tools/d18_logic_catalog_compiler.py --output game/tools/results/D18/D18-L53/D18_L53_L42_COMPILE.json` | 同上 | [D18_L53_L42_COMPILE.json](../../../game/tools/results/D18/D18-L53/D18_L53_L42_COMPILE.json)（同目录 `D18-L43/D18_L43_L42_COMPILE.json` 已同步） |

- 两个工具均 exit 0；本重跑基于 **D18-L51 双层分类契约**（61 条 Quaternius 记录迁移为 `category=animal/character` + `taxonomy_category` 逐条保留）后的真实目录。

## 2. L39 聚合结果（记录级字段覆盖，本次机器输出）

来源：`D18_L53_L39_AUDIT.json` 的 `aggregate` 与 `subtools`：

- 交付物完整性：37/37 存在、JSON 全合法（`deliverables_missing=0`）；子规则冒烟 37/37（`smoke.failed_count=0`）。
- 聚合：4 个来源目录、**161 条记录**、`catalog_json_invalid_count=0`。
- 状态分布：161 条全部 `Candidate`（`state_issues=0`）；**无 RuntimeApproved/ReleaseApproved 持有者**。
- 缺口：**P0=0、P1=4**，全部为 `hash_pending`（4 条 Poly Haven HDRI 记录 `sha256=null`）。
- `path_issues=0`、`license_issues=0`、`gap_counts={"hash_pending": 4}`。
- 分类（双层契约后）：`category` 全部落在 L02 11 值大类；8 个细分类经 `taxonomy_category` 表达（L51 迁移结果）。

## 3. L42 编译结果（Schema 门 + 合并，本次机器输出）

来源：`D18_L53_L42_COMPILE.json` 的 `audit_summary`：

| 指标 | 值 |
|---|---|
| 输入记录 | **161** |
| 合法记录（compiled） | **161**（state 全 `Candidate`） |
| 拒绝记录（rejected） | **0** |
| 合并去重 / 冲突 / 来源错误 | 0 / 0 / 0 |
| Pending hash（sha256=null，保留） | **4** |

## 4. 按来源/类别汇总（数字全部来自本次工具输出）

### 4.1 按来源

| 来源目录 | 输入记录 | L42 合法（compiled） | L42 拒绝（rejected） | Pending hash |
|---|---:|---:|---:|---:|
| `d18_ambientcg_logic_catalog.json`（L24） | 0 | 0 | 0 | 0 |
| `d18_kenney_logic_catalog.json`（L21） | 9 | 9 | 0 | 0 |
| `d18_polyhaven_logic_catalog.json`（L23） | 91 | 91 | 0 | 4 |
| `d18_quaternius_logic_catalog.json`（L22，L51 迁移后） | 61 | 61 | 0 | 0 |
| 合计 | 161 | 161 | 0 | 4 |

### 4.2 按类别（L42 `audit_summary` 与 compiled 记录）

`category` 大类分布（161 条）：material 87、animal 21（含 Quaternius 迁移）、character 40（含 Quaternius 迁移）、lighting_reference 4、audio 3、ui 2、ship/building/environment/character 各 1（Kenney）。

`taxonomy_category` 细分类（61 条，L51 迁移逐条保留）：terrestrial_animal 10、aquatic_animal 7、pet 3、bird 1（→animal）；npc_visual 8、player_visual 1、hair 7、outfit 24（→character）。

## 5. P0/P1 缺口与不可验证项目

- **P0：0**。无缺许可、无越级状态、无路径越界、无许可冲突、无状态迁移问题、无分类分裂（L51 已修复原 61 条 rejected）。
- **P1：4**（`hash_pending`）：`asset_hdri_green_point_park`、`asset_hdri_joburg_central_sunset`、`asset_hdri_kloppenheim_06`、`asset_hdri_venice_sunset`——`sha256=null`（L06 语义：Pending，不批准，不补猜）。
- **不可验证项目（明示，不补猜）**：
  1. 逐文件 SHA-256 完整性（当前为归档级/声明级哈希）与真实文件存在性，属 L06/L31 对真实文件的验证范围；
  2. ambientcg（L24）：来源级许可声明已记录（CC0 source-level），但 `records` 为空——无单项记录，无可验证资产；
  3. Unity 导入、Prefab、碰撞、LOD、动画、运行时消费与保存/恢复——全部 `RUNTIME_DEFERRED`。

## 6. Runtime / 人工许可延期（明确声明）

- **Runtime 延期**：真实网格/材质/音频导入、碰撞/LOD/动画装配、Canvas/Mixer 消费、保存恢复、性能与截图验证——全部由未来 Runtime 阶段门禁执行；本审计不产生任何运行时通过结论。
- **人工许可延期**：任何 `ReleaseApproved`（含 Steam/外部分发）只能由责任人类审核者确认；本审计与工具均不批准。`RuntimeApproved`/`ReleaseApproved` 持有者当前均为 0。
- **Pending 语义**：4 条 hash_pending 为**未证实/未准入**状态；本文不把任何 Pending 说成 Ready。

## 7. 结论（L53 收尾重跑）

- 静态来源目录现状（2026-08-17）：161 条记录**全部 Schema 合法（全 Candidate）**、rejected=0、冲突=0；4 条 hash Pending；无越级状态、无许可/路径/状态/分类问题。
- 审计门禁：L39 status=PASS、L42 exit 0；机器结果与命令如上，可复核。
- 本报告替代 2026-08-16 初版（当时 61 条细分类 rejected 属 L51 修复前的历史事实；历史已由 L45/L50 记录，不覆盖、不抹除）。
