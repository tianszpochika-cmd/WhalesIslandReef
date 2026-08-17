# D18 逻辑资源索引（D18-L20）

> 任务：`D18-L20`（波次 1，并行）
> 冻结日期：2026-08-16
> 用途：**单页入口**——后续 Agent（D19—D24 及 D18 后续任务）从本页即可找到 D18 全部逻辑事实源与状态。本索引只做**链接与一句话用途**，不复制、不改写任何任务的内容；每个条目的唯一事实源是其自身交付物与 `*_LOGIC_RESULT.json`。
> 权威状态：以 [逻辑任务状态总览](../../../LOGIC_TASK_STATUS.md)（仅 `python tools/sync_logic_task_status.py` 写入）为准；本索引只做路径索引，不自行判定 PASS/BLOCKED。下文状态列为**写盘时点快照**，如与总览不一致以总览为准。
> 依据：[D18 纯逻辑资源治理基线](../D18_LOGIC_GOVERNANCE_BASELINE.md)（D18-L01）、[单任务逻辑开发协议](../../../LOGIC_TASK_PROTOCOL.md)、[D18 章节任务卡](../../../AGENT_DEVELOPMENT_CHAPTER_10_D18.md)。

## 0. 建议阅读顺序（新 Agent 对齐页）

1. [D18_LOGIC_GOVERNANCE_BASELINE.md](../D18_LOGIC_GOVERNANCE_BASELINE.md)（基线：状态、字段、波次、禁区）
2. [逻辑任务状态总览](../../../LOGIC_TASK_STATUS.md)（谁完成、谁 Pending）
3. 本索引（按需跳到对应交付物）
4. 目标任务自己的结果 JSON 与 handoff（`game/tools/results/D18/D18-Lnn/` 与 `handoffs/D18/`，见第 9 节）

## 1. Schema（机器可校验契约）

| 任务 | 交付物 | 用途 | 状态 |
|---|---|---|---|
| L02 | `tools/schemas/d18_logic_asset_record.schema.json`（[链接](../../../../../tools/schemas/d18_logic_asset_record.schema.json)） | 资源逻辑记录 Schema：13 必填字段、7 状态枚举、路径边界；1 合法 + 8 非法夹具在 `tools/tests/d18_logic_asset_record_fixtures/` | LOGIC_PASS |
| （A 系列只读） | `tools/schemas/d18_asset_pack_manifest.schema.json`（[链接](../../../../../tools/schemas/d18_asset_pack_manifest.schema.json)） | 既有 manifest 字段口径（只读兼容层，不改） | 只读 |
| L25 | `tools/schemas/d18_import_descriptor.schema.json`（**预定位置，未创建**） | 导入描述器 Schema；明确不是 Unity Import Settings | **Pending**（波次 2） |
| L37 | `tools/schemas/d18_runtime_promotion_request.schema.json`（**预定位置，未创建**） | 运行时晋级申请 Schema；禁止产生批准 | **Pending**（波次 2） |

## 2. 状态机（7 状态与迁移）

| 任务 | 交付物 | 用途 | 状态 |
|---|---|---|---|
| L03 | `assets/D18_ASSET_STATE_MACHINE.md`（[链接](D18_ASSET_STATE_MACHINE.md)） | 七状态（Candidate/IntakeEligible/RuntimeRequested/RuntimeApproved/ReleaseApproved/Rejected/Revoked）证据/唯一写者/迁移矩阵/撤销规则/审计记录 | LOGIC_PASS |
| L01 | 基线第 2 节（[链接](../D18_LOGIC_GOVERNANCE_BASELINE.md#2-状态机冻结术语定义迁移拒绝撤销唯一写者)） | 状态定义与迁移冻结原文 | LOGIC_PASS |

## 3. 分类词典（L02 category 的实例化子层）

| 任务 | 交付物 | 用途 | 状态 |
|---|---|---|---|
| L11 | `assets/D18_TAXONOMY_SHIPS_PORT.md`（[链接](D18_TAXONOMY_SHIPS_PORT.md)） | ship/hull_component/port_service/dock/berth/building/prop 七类与子类、命名规则、弃用映射、不可混用关系 | LOGIC_PASS |
| L12 | `assets/D18_TAXONOMY_CHARACTERS.md`（[链接](D18_TAXONOMY_CHARACTERS.md)） | player_visual/npc_visual/hair/outfit/accessory/role_variant/body/injury_overlay/shared_body 九类、ResourceKey 约束、视觉键禁区 | LOGIC_PASS |
| L13 | `assets/D18_TAXONOMY_ECOLOGY.md`（[链接](D18_TAXONOMY_ECOLOGY.md)） | 陆生/鸟类/水生/珍稀/宠物/植被/地形/水域八类、生态所有者、17 个 intake role 映射 | LOGIC_PASS |
| L14 | `assets/D18_TAXONOMY_UI_AUDIO.md`（[链接](D18_TAXONOMY_UI_AUDIO.md)） | ui_icon/input_prompt/audio_cue/music/material/texture/hdri/weather_presentation/font 九类、本地化/可访问性标签 | LOGIC_PASS |
| L02 | `d18_logic_asset_record.schema.json` 的 `category` 枚举（11 类，见第 1 节链接） | 资源/领域层顶层枚举 | LOGIC_PASS |

## 4. 验证器（只读，全部不改 Registry/不访问网络）

| 任务 | 工具（链接） | 校验内容 | 状态 |
|---|---|---|---|
| L04 | [d18_license_normalizer.py](../../../../../tools/d18_license_normalizer.py) | 许可证标准化；未知/冲突/NC/缺字段保守阻断 | LOGIC_PASS |
| L05 | [d18_provenance_validator.py](../../../../../tools/d18_provenance_validator.py) | 来源证据：source_id/URL/作者/下载日期/证据一致性 | LOGIC_PASS |
| L06 | [d18_hash_validator.py](../../../../../tools/d18_hash_validator.py) | SHA-256 格式/路径/真实文件哈希；302/302 真实记录实证 | LOGIC_PASS |
| L07 | [d18_distribution_rights_validator.py](../../../../../tools/d18_distribution_rights_validator.py) | 四项分发权利（商业/修改/仓库存储/随游戏分发）显式结论+证据；unknown/conflict 阻断迁移 | LOGIC_PASS |
| L08 | [d18_asset_path_validator.py](../../../../../tools/d18_asset_path_validator.py) | 资源路径边界：禁绝对路径/`..`/盘符/临时目录 | LOGIC_PASS |
| L09 | [d18_attribution_validator.py](../../../../../tools/d18_attribution_validator.py) | 署名元数据；CC0 禁伪造必须署名 | LOGIC_PASS |
| L10 | [d18_package_boundary_validator.py](../../../../../tools/d18_package_boundary_validator.py) | 包边界：包内证据、文件清单、重复归属 | LOGIC_PASS |
| L15 | [d18_content_id_asset_link_validator.py](../../../../../tools/d18_content_id_asset_link_validator.py) | content_id↔资源链接；禁游戏数据反向依赖候选文件 | LOGIC_PASS |
| L16 | [d18_resource_key_validator.py](../../../../../tools/d18_resource_key_validator.py) | ResourceKey 唯一/稳定/别名/迁移/类别；禁路径/扩展名/显示名/空键 | LOGIC_PASS |
| L17 | [d18_candidate_runtime_guard.py](../../../../../tools/d18_candidate_runtime_guard.py) | 候选运行时引用防线：静态扫描 Candidates 直引 | 工具已落盘，结果 **Pending** |
| L18 | [d18_release_claim_guard.py](../../../../../tools/d18_release_claim_guard.py) | 发布声明防线：禁止伪造 Release 声明 | 工具已落盘，结果 **Pending** |

- 测试镜像位于 `tools/tests/test_d18_*.py`（[目录](../../../../../tools/tests/)）；夹具按工具约定放置。
- 使用口径：验证器只产出证据缺口与拒绝/撤销记录，**从不产生 RuntimeApproved/ReleaseApproved 授予**（基线 §2.6 冻结）。

## 5. 来源 catalog 与规则文档（预定位置）

| 任务 | 预定位置（当前不存在，标记 Pending） | 用途 | 状态 |
|---|---|---|---|
| L19 | `assets/D18_WAIVER_AND_REJECTION_RULES.md`（[链接](D18_WAIVER_AND_REJECTION_RULES.md)） | 缺许可/来源冲突/重复文件/不可分发条目的拒绝与豁免流程 | LOGIC_PASS |
| L21 | `assets/sources/d18_kenney_logic_catalog.json` | Kenney 候选转静态逻辑记录（L02 Schema 合法） | **Pending**（波次 2） |
| L22 | `assets/sources/d18_quaternius_logic_catalog.json` | Quaternius 候选静态记录 | **Pending**（波次 2） |
| L23 | `assets/sources/d18_polyhaven_logic_catalog.json` | Poly Haven 单项材质/HDRI/模型静态记录 | **Pending**（波次 2） |
| L24 | `assets/sources/d18_ambientcg_logic_catalog.json` | ambientCG 单项材质记录；站点许可不能替代单项证据 | **Pending**（波次 2） |

## 6. 唯一写者（谁授予/写入；不复制全文，指向事实源）

- 完整唯一写者表：基线 [第 2.6 节](../D18_LOGIC_GOVERNANCE_BASELINE.md#26-唯一写者谁可以授予写入该状态)（L01 冻结）。
- 摘要：`Candidate`＝来源目录任务（L21—L24）登记；`IntakeEligible`＝L42 目录编译器（依据 L04—L10/L15—L16 通过结果）；`RuntimeRequested`＝消费任务（D19—D22）申请数据；`RuntimeApproved`＝未来 Runtime 阶段门禁；`ReleaseApproved`＝责任人类审核者；`Rejected/Revoked`＝人类复核（验证器可标记但必须留原因）。
- 状态机迁移细节与审计记录格式：L03 状态机文档（第 2 节链接）。

## 7. 下游 D19—D24 读取边界（只读消费，接口由各章独占实现）

| 下游 | 消费入口 | 读取边界 |
|---|---|---|
| D19（港口） | `D19-L01` 只读基线盘点可在 `D18-L01` 后并行；其余等待 `D18-L50` | 港口的 `port_service_*`/`dock_*`/`berth_*`/`ship_*` 键口径来自 L11；不得把 D18 候选资源写为运行时接入 |
| D20（玩家/NPC/动物/动作） | `D20-L21` 物种目录、`D20-L32` content_id 集成测试、`D20-L09/L10` 外观/身份规则 | 分类键/前缀/ResourceKey 约束来自 L12/L13/L16；视觉键禁区与审查标签见 L12 §5/§6；无候选路径依赖 |
| D21（船/天气/海洋） | `D21-L13` 天气影响规则 | 只读影响规则；不写生态数量/分类（L13 生态所有者）；船型/泊位键来自 L11 |
| D22/D23 | 按各章任务卡消费对应分类键 | 只读引用；不修改 D18 交付物 |
| D24（本地可玩） | `D24-L09` 库存不变式审计等 | 宠物/采集物前缀（`pet_*`/`plant_*`/`fish_*`/`item_*`）与归属不变式来自 L13/L16 |
| 章节级接口 | `L46` `assets/D18_LOGIC_HANDOFF_INTERFACE.md`（**预定位置，未创建**） | 发布 D19—D24 可读取的完整接口说明 | **Pending**（波次 3，串行） |

## 8. Runtime 延期清单（不在此复制，指向事实源）

- 当前冻结清单：基线 [第 9 节](../D18_LOGIC_GOVERNANCE_BASELINE.md#9-runtime-延期清单当前已知冻结)（L01）。
- 展开文档：`L48` `assets/D18_BINARY_AND_IMPORT_DEFERRAL.md`（**预定位置，未创建**）——二进制与导入延期 | **Pending**（波次 4）。
- 一句话口径：本逻辑阶段禁止任何工具授予 `RuntimeApproved`/`ReleaseApproved`；逐文件哈希（L06/L31）、Unity 导入（L26—L28）、Prefab/场景集成、保存恢复均属 Runtime。

## 9. 结果、交接与状态约定

| 内容 | 位置 | 写者 |
|---|---|---|
| 每任务机器结果 | `game/tools/results/D18/D18-Lnn/D18_Lnn_LOGIC_RESULT.json`（示例：[D18-L01 结果](../../../../../game/tools/results/D18/D18-L01/D18_L01_LOGIC_RESULT.json)） | 各任务自己 |
| 交接报告 | `docs/game/design/common/handoffs/D18/D18-Lnn_LOGIC_HANDOFF.md`（示例：[D18-L01 handoff](../handoffs/D18/D18-L01_LOGIC_HANDOFF.md)） | 各任务自己 |
| 状态总览 | `docs/game/LOGIC_TASK_STATUS.md`（[链接](../../../LOGIC_TASK_STATUS.md)） | **仅** `python tools/sync_logic_task_status.py`，禁止手改 |
| 结果 JSON 格式 | [LOGIC_TASK_PROTOCOL.md](../../../LOGIC_TASK_PROTOCOL.md) 固定格式 | 全部 L 任务 |

## 10. Pending 声明（2026-08-16 写盘时点快照，如实标注，不写 PASS）

以下项**未完成或结果未提交**，任何下游不得将其当作通过条件：

1. **L17 / L18**：工具与测试文件已在 `tools/` 落盘（波次 1 并行 Agent 进行中），`LOGIC_RESULT.json` 尚未提交 → 状态总览为 `PLANNED`；下游引用这些工具接口时应以对应结果 JSON 为准，缺结果 = 不可作证据。
2. **L21—L24**（来源 catalog）：预定路径未创建（见第 5 节）。
3. **L25—L50**：全部未开始（波次 2/3/4；L39 后严格串行）；其中 L39 必须等 L02—L38 全部结束（有未证实项保持 BLOCKED，不得聚合成 PASS）。
4. 跨章：D19 其余任务、D20—D24 正式消费等待 `D18-L50 LOGIC_PASS`。
5. 本索引自身状态：`D18-L20` 波次 1 交付；后续字段/路径修订由 L20 独占写，或由 L50 验收时核对，不允许其他任务覆盖。
6. 状态快照与权威总览不一致时，以 [LOGIC_TASK_STATUS.md](../../../LOGIC_TASK_STATUS.md) 为准（写盘后新提交的结果不会被本索引自动感知）。

## 11. 机器可读摘要（L45/L50 可解析本 JSON 块）

```json
{
  "document": "D18_LOGIC_RESOURCE_INDEX",
  "task_id": "D18-L20",
  "frozen": "2026-08-16",
  "base_dir": "docs/game/design/common/assets",
  "artifacts": {
    "schema": {
      "logic_asset_record": "tools/schemas/d18_logic_asset_record.schema.json",
      "import_descriptor": "tools/schemas/d18_import_descriptor.schema.json",
      "runtime_promotion_request": "tools/schemas/d18_runtime_promotion_request.schema.json"
    },
    "state_machine": "docs/game/design/common/assets/D18_ASSET_STATE_MACHINE.md",
    "taxonomies": {
      "ships_port": "D18_TAXONOMY_SHIPS_PORT.md",
      "characters": "D18_TAXONOMY_CHARACTERS.md",
      "ecology": "D18_TAXONOMY_ECOLOGY.md",
      "ui_audio": "D18_TAXONOMY_UI_AUDIO.md"
    },
    "validators": {
      "L04_license_normalizer": "tools/d18_license_normalizer.py",
      "L05_provenance": "tools/d18_provenance_validator.py",
      "L06_hash": "tools/d18_hash_validator.py",
      "L07_distribution_rights": "tools/d18_distribution_rights_validator.py",
      "L08_asset_path": "tools/d18_asset_path_validator.py",
      "L09_attribution": "tools/d18_attribution_validator.py",
      "L10_package_boundary": "tools/d18_package_boundary_validator.py",
      "L15_content_id_asset_link": "tools/d18_content_id_asset_link_validator.py",
      "L16_resource_key": "tools/d18_resource_key_validator.py",
      "L17_candidate_runtime_guard": "tools/d18_candidate_runtime_guard.py",
      "L18_release_claim_guard": "tools/d18_release_claim_guard.py"
    },
    "source_catalogs_reserved": {
      "L21_kenney": "docs/game/design/common/assets/sources/d18_kenney_logic_catalog.json",
      "L22_quaternius": "docs/game/design/common/assets/sources/d18_quaternius_logic_catalog.json",
      "L23_polyhaven": "docs/game/design/common/assets/sources/d18_polyhaven_logic_catalog.json",
      "L24_ambientcg": "docs/game/design/common/assets/sources/d18_ambientcg_logic_catalog.json"
    },
    "waiver_rules": "docs/game/design/common/assets/D18_WAIVER_AND_REJECTION_RULES.md",
    "interface_doc": "docs/game/design/common/assets/D18_LOGIC_HANDOFF_INTERFACE.md",
    "deferral_doc": "docs/game/design/common/assets/D18_BINARY_AND_IMPORT_DEFERRAL.md"
  },
  "pending_at_freeze": [
    "D18-L17", "D18-L18",
    "D18-L21", "D18-L22", "D18-L23", "D18-L24",
    "D18-L25", "D18-L26", "D18-L27", "D18-L28", "D18-L29",
    "D18-L30", "D18-L31", "D18-L32", "D18-L33", "D18-L34",
    "D18-L35", "D18-L36", "D18-L37", "D18-L38", "D18-L39",
    "D18-L40", "D18-L41", "D18-L42", "D18-L43", "D18-L44",
    "D18-L45", "D18-L46", "D18-L47", "D18-L48", "D18-L49", "D18-L50"
  ],
  "note": "status authority is docs/game/LOGIC_TASK_STATUS.md written by tools/sync_logic_task_status.py; pending items must never be treated as PASS"
}
```
