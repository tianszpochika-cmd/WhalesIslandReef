# 第 10 章：D18 资源与内容逻辑治理（53 项）

> 本章只建立资源、内容、许可证和 Registry 的**数据治理逻辑**。不下载资源、不导入 Unity、不制作 Prefab、不做截图、不启动 Player。
>
> 前置：`D18-L01` 是全章唯一冻结任务。`D18-L02`—`D18-L38` 可按波次并行；`D18-L39` 后必须严格串行。
>
> 需要直接复制给 Agent 的完整提示词时，使用 [D18 详细提示词（53 项）](AGENT_DEVELOPMENT_CHAPTER_10_D18_PROMPTS.md)。本文件保留任务卡、边界和验收索引。

## 复制给 Agent 的单任务执行提示词

```text
你是 WhalesIslandReef 的纯逻辑开发 Agent。直接执行任务 [TASK_ID]，不要等待确认，不要改做相邻任务。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前必须阅读：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md，以及本文件中 [TASK_ID] 的任务卡。

严格按任务卡的前置条件、唯一写入范围、实施内容和验收执行。当前是纯逻辑阶段：允许编译、单元测试、契约/静态检查和确定性模拟；禁止启动 Unity Player、PlayMode、性能/截图/路线实测、外部资源导入以及任何场景、Prefab、Canvas、Build Settings 修改。

完成时写入 game/tools/results/D18/[TASK_ID]/[TASK_ID下划线]_LOGIC_RESULT.json、docs/game/design/common/handoffs/D18/[TASK_ID]_LOGIC_HANDOFF.md 和当天 .workbuddy/memory/YYYY-MM-DD.md；运行 python tools/sync_logic_task_status.py 与 git diff --check。最终仅报告 LOGIC_PASS、LOGIC_PARTIAL 或 LOGIC_BLOCKED、实际验证命令、延期的 Runtime 风险和结果路径。
```

## D18 的逻辑完成标准

- 候选、准入、运行时消费、发布审批四个状态是不同状态，不能互相伪造；
- 每个资源记录可静态验证来源、许可、哈希、归属、用途和内容 ID；
- 游戏逻辑不能直接依赖候选二进制文件；外部资源的视觉导入/Prefab 消费全部延期；
- 所有 Schema、目录、验证器和测试使用相同字段语义；
- 本章只输出 `LOGIC_PASS`，不输出资源数量、画质、模型可用或 ReleaseApproved 结论。

## 任务卡与执行顺序

| ID | 波次 | 写入范围（唯一） | 实施内容与逻辑验收 |
|---|---|---|---|
| D18-L01 | S | `docs/game/design/common/D18_LOGIC_GOVERNANCE_BASELINE.md` | 冻结本章字段词典、状态机、唯一写者和历史 R01—R03 隔离清单；检查现有 manifest/审计工具的真实边界。 |
| D18-L02 | P | `tools/schemas/d18_logic_asset_record.schema.json` | 定义资源逻辑记录 Schema：ID、类别、来源、许可、哈希、状态、内容映射；加入有效/无效夹具验证。 |
| D18-L03 | P | `docs/game/design/common/assets/D18_ASSET_STATE_MACHINE.md` | 定义 Candidate→IntakeEligible→RuntimeRequested→RuntimeApproved 的不可跳跃状态图和拒绝/撤销规则。 |
| D18-L04 | P | `tools/d18_license_normalizer.py` | 规范许可证名称、署名和商用/再分发字段；为未知或冲突许可证返回阻断。 |
| D18-L05 | P | `tools/d18_provenance_validator.py` | 校验官方来源 URL、作者、下载日期、原始包名和证据路径，不访问网络也不伪造网页结果。 |
| D18-L06 | P | `tools/d18_hash_validator.py` | 校验 SHA-256 格式、文件存在性和哈希重复；缺原文件时返回 Pending，而非批准。 |
| D18-L07 | P | `tools/d18_distribution_rights_validator.py` | 校验商业使用、修改、仓库存储、随游戏分发四项权利的显式记录。 |
| D18-L08 | P | `tools/d18_asset_path_validator.py` | 定义 Candidate/正式逻辑元数据/本地缓存的目录边界，静态阻止路径越界和绝对路径泄露。 |
| D18-L09 | P | `tools/d18_attribution_validator.py` | 校验需要署名的条目存在署名文本和展示位置；CC0 不强制伪署名。 |
| D18-L10 | P | `tools/d18_package_boundary_validator.py` | 校验包、文件、manifest、许可证证据之间的包含关系，不读取或导入二进制模型。 |
| D18-L11 | P | `docs/game/design/common/assets/D18_TAXONOMY_SHIPS_PORT.md` | 冻结船、港口、建筑、道具的逻辑分类和 content_id 映射规则。 |
| D18-L12 | P | `docs/game/design/common/assets/D18_TAXONOMY_CHARACTERS.md` | 冻结玩家/NPC 外观资源与身份、性别、职业数据的非裁决映射边界。 |
| D18-L13 | P | `docs/game/design/common/assets/D18_TAXONOMY_ECOLOGY.md` | 冻结动物、植被、水生物、地形类别和生态 content_id 的静态词典。 |
| D18-L14 | P | `docs/game/design/common/assets/D18_TAXONOMY_UI_AUDIO.md` | 冻结 UI、音频、材质、天气表现资源的逻辑类别和许可字段。 |
| D18-L15 | P | `tools/d18_content_id_asset_link_validator.py` | 校验 asset record 引用的 content_id 存在且不反向让游戏数据依赖候选文件。 |
| D18-L16 | P | `tools/d18_resource_key_validator.py` | 校验 ResourceKey 唯一、稳定、可迁移，禁止用磁盘路径或显示名作业务键。 |
| D18-L17 | P | `tools/d18_candidate_runtime_guard.py` | 静态扫描游戏运行时目录，阻止直接引用 `Assets/Art/Candidates` 或声明为已发布资源。 |
| D18-L18 | P | `tools/d18_release_claim_guard.py` | 扫描 manifest/文档，阻止 Candidate 或 IntakeEligible 被写成 ReleaseApproved。 |
| D18-L19 | P | `docs/game/design/common/assets/D18_WAIVER_AND_REJECTION_RULES.md` | 定义缺许可证、来源冲突、重复文件和不可分发条目的拒绝/豁免流程。 |
| D18-L20 | P | `docs/game/design/common/assets/D18_LOGIC_RESOURCE_INDEX.md` | 建立不含二进制的逻辑资源索引入口和责任人表。 |
| D18-L21 | P | `docs/game/design/common/assets/sources/d18_kenney_logic_catalog.json` | 将现有 Kenney 候选转换为 Schema 合法的静态记录；不补猜哈希或数量。 |
| D18-L22 | P | `docs/game/design/common/assets/sources/d18_quaternius_logic_catalog.json` | 建立 Quaternius 候选逻辑目录，并将未证实项保留 Pending。 |
| D18-L23 | P | `docs/game/design/common/assets/sources/d18_polyhaven_logic_catalog.json` | 建立 Poly Haven 单项材质/模型的来源和许可记录。 |
| D18-L24 | P | `docs/game/design/common/assets/sources/d18_ambientcg_logic_catalog.json` | 建立 ambientCG 单项材质记录，校验不能以站点许可替代单项证据。 |
| D18-L25 | P | `tools/schemas/d18_import_descriptor.schema.json` | 定义未来导入所需的比例、轴向、材质槽、骨架、动画元数据；不导入任何资源。 |
| D18-L26 | P | `tools/d18_mesh_descriptor_validator.py` | 验证网格描述信息完整性、尺度单位和碰撞意图；实际网格检查延期。 |
| D18-L27 | P | `tools/d18_material_descriptor_validator.py` | 验证材质/贴图描述、颜色空间和许可依赖；不创建 Unity 材质。 |
| D18-L28 | P | `tools/d18_animation_descriptor_validator.py` | 验证动画用途、骨架类型、root-motion 策略和缺失回退声明。 |
| D18-L29 | P | `tools/d18_ui_audio_descriptor_validator.py` | 验证 UI/音频资源用途、本地化/可访问性标签和授权记录。 |
| D18-L30 | P | `tools/d18_asset_dependency_graph.py` | 生成资源逻辑依赖图，检测循环、悬挂依赖和候选→发布跨级引用。 |
| D18-L31 | P | `tools/d18_duplicate_identity_audit.py` | 按 hash、来源和 canonical_id 检测重复；只登记，不删除文件。 |
| D18-L32 | P | `tools/d18_name_collision_audit.py` | 检测文件名、ResourceKey、content_id 和显示名的冲突，输出可复核映射。 |
| D18-L33 | P | `tools/d18_catalog_claim_audit.py` | 校验文档中的数量/类别宣称可由真实静态目录计算，不接受手填总数。 |
| D18-L34 | P | `tools/d18_missing_evidence_audit.py` | 枚举缺 license、hash、来源、rights、content mapping 的 P0/P1 缺口。 |
| D18-L35 | P | `tools/d18_status_transition_audit.py` | 对批准、撤销、拒绝、Pending 迁移做表驱动测试。 |
| D18-L36 | P | `tools/d18_local_development_guard.py` | 定义 LocalDevelopmentReady 的逻辑条件，禁止以视觉或运行时通过替代证据。 |
| D18-L37 | P | `tools/schemas/d18_runtime_promotion_request.schema.json` | 定义未来 Runtime 晋级申请数据；本章禁止产生批准结果。 |
| D18-L38 | P | `tools/d18_generated_asset_guard.py` | 标出生成/派生资源与外部原件的关系，防止把生成物伪装为第三方原件。 |
| D18-L39 | S | `tools/d18_logic_audit.py` | 聚合 L02—L38 验证器为单一只读审计入口，保留每类失败原因。 |
| D18-L40 | S | `tools/tests/test_d18_logic_valid_records.py` | 建立覆盖每类合法资源记录的固定夹具和单元测试。 |
| D18-L41 | S | `tools/tests/test_d18_logic_invalid_records.py` | 建立缺证据、越级、冲突、重复和路径越界的负向夹具。 |
| D18-L42 | S | `tools/d18_logic_catalog_compiler.py` | 从静态来源目录编译规范 catalog；禁止写入 Unity、Prefab 或运行时 Registry。 |
| D18-L43 | S | `docs/game/design/common/D18_LOGIC_SOURCE_AUDIT.md` | 运行聚合审计，记录真实候选、Pending、Rejected 和缺口，不制造完成数量。 |
| D18-L44 | S | `tools/d18_game_reference_audit.py` | 只读审计 game 代码/配置对资源 key 的使用，确认业务逻辑不依赖候选路径。 |
| D18-L45 | S | `tools/d18_schema_compatibility_audit.py` | 校验 Schema、catalog、验证器和工具输出字段一致且可由 CI 调用。 |
| D18-L46 | S | `docs/game/design/common/D18_LOGIC_HANDOFF_INTERFACE.md` | 发布给 D19—D24 的只读接口：允许使用的类别/键、拒绝规则和 Runtime 延期边界。 |
| D18-L47 | G | `game/tools/results/D18/D18-L47/` | 复核所有 P0 Schema/验证器/夹具；无证据的并行任务必须 BLOCKED，不得聚合成 PASS。 |
| D18-L48 | G | `docs/game/design/common/D18_BINARY_AND_IMPORT_DEFERRAL.md` | 审计并记录全部二进制导入/运行时消费延期项，确认未偷偷修改场景/Prefab。 |
| D18-L49 | G | `docs/game/design/common/D18_LOGIC_STATUS_REVIEW.md` | 审计结果 JSON、handoff、记忆日志和状态总览是否完整、可追溯。 |
| D18-L50 | G | `docs/game/design/common/D18_LOGIC_ACCEPTANCE.md` | 首轮章节逻辑验收：汇总 P0，给出 LOGIC_PASS/PARTIAL/BLOCKED 与对 D19 的明确放行条件；收尾修复后由 L53 负责重跑本验收。 |
| D18-L51 | S | `tools/schemas/d18_taxonomy_category_map.json` + L02/L39/L42 相关文件 | 修复“资源大类 category”与“细分类 taxonomy_category”混用：保留 11 值大类、建立可机器校验的细分类→大类映射，迁移 Quaternius 61 条记录，并使 L39/L42 同一语义验证。 |
| D18-L52 | S | `game/Assets/Settings/D19/Lighting/D19LightingProfileCatalog.json` | 将 5 个运行时 `candidate_hdri_*` 引用替换为已登记的稳定 `asset_hdri_*` content_id；以 L44 静态审计确认没有 P0 候选键引用。 |
| D18-L53 | G | `game/tools/results/D18/` + D18 验收/接口文档 | 严格重跑受 L51/L52 影响的 L39、L42—L50 证据链，修复历史 R01 测试的过期断言但不跳过测试；只有所有 D18-L01—L53 与 L50 均为 LOGIC_PASS 才通过。 |

## 本章的依赖细则

- L02—L20 只依赖 L01，可并行；每项禁止修改其他工具的文件。
- L21—L25、L29—L38 在波次 1 后可并行；它们必须使用 L02—L20 已冻结的字段。`L26`—`L28` 额外硬依赖 `L25` 的 import descriptor Schema，必须等待 `L25=LOGIC_PASS` 后才可并行；若该 Schema 尚未合并，保持 `LOGIC_BLOCKED`，不可复制字段定义。
- L39 等待 L02—L38 全部结束；L40→L46 严格串行；L47→L50 严格串行。L50 的首次 `LOGIC_PARTIAL` 不可覆盖或伪写通过。
- 收尾链严格为 `L51 → L52 → L53`。L53 必须重跑并更新受影响的 L39、L42—L50 事实证据，保留 L50 首次失败原因于交接/记忆日志。
- 只有 L53=`LOGIC_PASS` 且重跑后的 L50=`LOGIC_PASS` 时，D19 的港口逻辑集成任务可以把 D18 输出视为稳定输入。D19-L01 的只读基线盘点可在 L01 通过后并行开始。
