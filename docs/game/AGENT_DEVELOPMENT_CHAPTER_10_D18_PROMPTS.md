# D18：50 个可直接复制执行的纯逻辑任务提示词

> 本文是 D18 的唯一“复制给 Agent”入口；任务卡和总体顺序仍以 [D18 章节说明](AGENT_DEVELOPMENT_CHAPTER_10_D18.md) 与 [总任务索引](AGENT_DEVELOPMENT_TASKS.md) 为准。
>
> 当前可开始：**仅 `D18-L01`**。不要把多项任务粘给同一个 Agent；同一编号只允许一个 Agent 执行。

## 执行波次

```text
波次 0（串行）
D18-L01
  ↓ LOGIC_PASS
波次 1（可同步并行，19 项）
D18-L02 ～ D18-L20
  ↓ 所有 P0 输出 LOGIC_PASS
波次 2A（可同步并行，15 项）
D18-L21 ～ D18-L25、D18-L29 ～ D18-L38
  ↓ D18-L25 = LOGIC_PASS
波次 2B（可同步并行，3 项）
D18-L26 ～ D18-L28
  ↓ 全部结束
波次 3（严格顺序）
D18-L39 → L40 → L41 → L42 → L43 → L44 → L45 → L46
  ↓
波次 4（严格顺序的汇合验收）
D18-L47 → L48 → L49 → L50
```

`同步并行` 只表示不同 Agent 可以同时执行。禁止两个 Agent 修改同一文件、同一 Schema、同一工具或同一目录。每项提示词均指定唯一写入范围。

## 所有任务都必须完成的记录动作

每个提示词已包含这些要求；这里单独列出是为了你检查交付：

1. 写唯一机器结果：`game/tools/results/D18/D18-Lnn/D18_Lnn_LOGIC_RESULT.json`。
2. 写交接：`docs/game/design/common/handoffs/D18/D18-Lnn_LOGIC_HANDOFF.md`。
3. 追加当天 `.workbuddy/memory/YYYY-MM-DD.md`。
4. 执行 `python tools/sync_logic_task_status.py`，更新 [逻辑任务状态总览](LOGIC_TASK_STATUS.md)。
5. 执行 `git diff --check`。

结果 JSON 至少包含 `task_id`、`status`、`updated_at`、`summary`、`changed_files`、`verification`、`runtime_deferred`、`blockers`、`handoff`。状态只能是 `LOGIC_PASS`、`LOGIC_PARTIAL` 或 `LOGIC_BLOCKED`。没有实际命令和结果证据时不得写 `LOGIC_PASS`。

---

## 当前修复批次：D18-L26、D18-L27、D18-L28（可交给一个 Agent 顺序完成）

> 2026-08-16 复核：L25 已取得 `LOGIC_PASS`；L26—L28 之前只是因为 L25 当时未落盘而 `LOGIC_BLOCKED`。以下提示词是一次受控的三任务修复例外：同一 Agent 必须分别写三个任务的结果与交接，不能把它们合成一个结果。

```text
你是 WhalesIslandReef 的 D18 依赖恢复 Agent。直接完成 D18-L26、D18-L27、D18-L28 的受控修复批次；严格按顺序执行 L26 → L27 → L28。不要执行 L39 及后续任务，不要修改 L01—L25、L29—L38 的实现，也不要重跑旧 D18-A/S/R 或任何 Unity/Player 任务。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前必须阅读：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md、docs/game/AGENT_DEVELOPMENT_CHAPTER_10_D18.md、D18-L25 的结果 JSON/交接，以及 D18-L26/L27/L28 当前 BLOCKED 结果和各自任务卡。

前置确认：先读取 game/tools/results/D18/D18-L25/D18_L25_LOGIC_RESULT.json，确认 task_id=D18-L25、status=LOGIC_PASS、tools/schemas/d18_import_descriptor.schema.json 存在且 Draft 2020-12 Schema 自检通过。若任一条件不成立，三个任务均保持 LOGIC_BLOCKED，写明真实原因并停止；不得复制、重建或修改 L25 Schema 来规避前置。

允许写入范围仅限：
- L26：tools/d18_mesh_descriptor_validator.py、tools/tests/test_d18_mesh_descriptor_validator.py、game/tools/results/D18/D18-L26/、docs/game/design/common/handoffs/D18/D18-L26_LOGIC_HANDOFF.md；
- L27：tools/d18_material_descriptor_validator.py、tools/tests/test_d18_material_descriptor_validator.py、game/tools/results/D18/D18-L27/、docs/game/design/common/handoffs/D18/D18-L27_LOGIC_HANDOFF.md；
- L28：tools/d18_animation_descriptor_validator.py、tools/tests/test_d18_animation_descriptor_validator.py、game/tools/results/D18/D18-L28/、docs/game/design/common/handoffs/D18/D18-L28_LOGIC_HANDOFF.md；
- 当天 .workbuddy/memory/YYYY-MM-DD.md 与自动生成的 docs/game/LOGIC_TASK_STATUS.md。

禁止修改：L25 Schema、来源 catalog、真实资源、Unity/Assets、场景、Prefab、Canvas、Registry、Systems、contracts、server、web、Build Settings；禁止启动 Unity Player、PlayMode、资源导入、网络下载、截图和性能测试。

执行 L26：只依据 L25 的 import descriptor Schema 静态验证单位、轴向、root、bounds、collision_intent、lod_intent、类别条件和 fallback。验证器必须先 Schema 校验输入；实际读取 FBX/网格、碰撞体、LODGroup 均是 Runtime Deferred。为合法几何描述及缺 bounds/非法轴向/负 bounds/错误类型/缺 fallback 等负例写单测。

执行 L27：只依据 L25 的描述器字段和 L14 分类/许可口径验证 material、texture、hdri、font 的描述引用、许可依赖、类别、回退和 runtime_deferred。不得创建 Unity Material/Shader/TextureImporter；若 L25 不包含某个希望验证的字段，使用明确的缺字段/Deferred 结果，不得私自扩展 L25 Schema。为正负描述写单测。

执行 L28：只依据 L25 和 D20 动作逻辑边界验证 skeleton_type、animation_list、root_motion、动作意图、许可证依赖和 fallback。不得导入 Clip、创建 Animator/Avatar 或修改 D20；为玩家/NPC/动物/无动作回退的正负案例写单测。

每一个任务分别验收：相关单测全部通过；输入/源文件在验证前后 SHA 不变；工具只读、无网络、无 Unity 依赖；输出含稳定错误码；不产生 RuntimeApproved/ReleaseApproved。不要把旧 test_d18_s02_r01_intake_refresh.py 的四个历史失败修成通过，它们属于 SUPERSEDED 历史测试，必须在结果中单独说明但不得掩盖本任务测试失败。

对 L26、L27、L28 分别覆盖原 canonical 结果 JSON：game/tools/results/D18/D18-Lnn/D18_Lnn_LOGIC_RESULT.json。新的 JSON 必须保留“此前因 L25 缺失而 BLOCKED、现已解除”的历史说明，并包含 task_id、LOGIC_*、真实 changed_files、命令/结果、runtime_deferred、blockers、handoff。分别更新三份 handoff，追加当天记忆日志。每完成一个任务运行对应单测；三项均完成后运行 python tools/sync_logic_task_status.py 和 git diff --check。

最终报告必须逐项列出 L26/L27/L28 的状态、测试数、未解决 blocker、Runtime Deferred 风险和结果路径。若其中一项失败，只将该项写 LOGIC_PARTIAL/BLOCKED；仍继续执行其余两个独立任务，不能因一个工具失败而伪造整批完成。
```

---

## 波次 0：冻结任务（严格顺序）

### D18-L01 — 纯逻辑资源治理基线

```text
你是 WhalesIslandReef 的 D18 纯逻辑资源治理 Agent。直接完成 D18-L01；这是串行冻结任务，未完成前不要开始 D18-L02—L50，也不要执行旧 D18-A/S/R 或 D19-R03 任务。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前阅读：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md、docs/game/AGENT_DEVELOPMENT_CHAPTER_10_D18.md，以及现有 D18 manifest、审计工具、资源目录和历史 handoff。

唯一写入范围：docs/game/design/common/D18_LOGIC_GOVERNANCE_BASELINE.md、game/tools/results/D18/D18-L01/、docs/game/design/common/handoffs/D18/D18-L01_LOGIC_HANDOFF.md、当天记忆日志。除为修复本任务文档链接所需的最小改动外，不修改 Schema、Python 工具、Unity、Prefab、场景、资源、Registry、Systems、contracts、server 或 web。

实施：
1. 盘点现有 D18 manifest、候选/准入目录、audit 工具和真实结果，明确哪些只是 Candidate、哪些有静态证据、哪些尚未证实；绝不补猜哈希、许可或数量。
2. 冻结 Candidate、IntakeEligible、RuntimeRequested、RuntimeApproved、ReleaseApproved 的精确定义、允许迁移、拒绝/撤销条件和唯一写者。
3. 冻结本逻辑阶段允许的验证（编译、单元测试、Schema、静态审计）和禁止项（Player、PlayMode、导入、Prefab、场景、截图、性能、网络下载）。
4. 列出旧 D18-A/S/R、D19-R03/RERUN 文档只作 SUPERSEDED 历史证据，禁止作为新 L 任务通过条件。
5. 为 L02—L50 写出每一波可开始条件、共享字段词典、结果 JSON 格式和冲突写入边界。

成功标准：基线文档能让后续 Agent 不阅读旧 RERUN 文档也知道状态机、边界、术语、波次和证据要求；不宣称任何资产已 RuntimeApproved/ReleaseApproved；git diff 仅在允许范围。

验证：检查所有链接存在；运行 git diff --check。把实际命令和结果写入 JSON。Runtime 风险填入 runtime_deferred。

完成记录：写 game/tools/results/D18/D18-L01/D18_L01_LOGIC_RESULT.json（task_id 必须为 D18-L01），写指定 handoff 和记忆日志，运行 python tools/sync_logic_task_status.py。最终只报告 LOGIC_PASS/LOGIC_PARTIAL/LOGIC_BLOCKED、关键冻结结论、阻塞项和结果路径。
```

---

## 波次 1：基础 Schema、验证器和分类（同步并行）

### D18-L02 — 资源逻辑记录 Schema

```text
执行 D18-L02（波次 1，可与 L03—L20 并行）。先阅读 AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D18 纯逻辑路线、LOGIC_TASK_PROTOCOL、D18-L01 结果和本任务说明。

唯一写入范围：tools/schemas/d18_logic_asset_record.schema.json、tools/tests/d18_logic_asset_record_fixtures/、game/tools/results/D18/D18-L02/、docs/game/design/common/handoffs/D18/D18-L02_LOGIC_HANDOFF.md、当天记忆日志。不得修改其它 D18 工具、catalog、Unity、资源、场景、Prefab、Registry 或旧 manifest。

实现一个可机器校验的 JSON Schema，至少要求 canonical asset_id、category、source_id、source_url、license、rights、download_date、original_filename、sha256、state、content_ids、evidence_paths、runtime_deferred；限制状态枚举，禁止未知批准状态和绝对路径。提供一个合法和至少五个非法夹具，覆盖缺许可、坏 hash、越级状态、空来源和路径越界。

成功状态：Schema 能由仓库现有 JSON Schema 校验方式验证；合法夹具通过，所有非法夹具分别因预期字段失败；不对真实候选补造数据。记录具体校验命令和结果。

禁止 Player、PlayMode、资源导入、网络下载、外部二进制、Unity/场景/Prefab 修改。完成时写 D18_L02_LOGIC_RESULT.json、D18-L02 handoff、记忆日志；运行 python tools/sync_logic_task_status.py 和 git diff --check。最终如实报告 LOGIC_*、夹具数量、字段边界和 Runtime 延期项。
```

### D18-L03 — 资源状态机规范

```text
执行 D18-L03（波次 1，可与 L02、L04—L20 并行）。先阅读项目约定、D18-L01 基线、D18 路线与 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/D18_ASSET_STATE_MACHINE.md、game/tools/results/D18/D18-L03/、docs/game/design/common/handoffs/D18/D18-L03_LOGIC_HANDOFF.md、当天记忆日志。不得修改 Schema、验证器、真实 catalog、Unity 或资源文件。

用表格和状态图严格定义 Candidate、IntakeEligible、RuntimeRequested、RuntimeApproved、ReleaseApproved、Rejected、Revoked：每个状态的证据、唯一写者、允许/禁止迁移、撤销规则、下游可读取范围。明确 IntakeEligible 不表示导入/Prefab/运行时可用，RuntimeApproved 不表示 ReleaseApproved。写出与 D18-L02/L35/L36/L37 的字段和测试接口，但不抢改它们。

成功状态：没有任何从 Candidate 直接到 RuntimeApproved/ReleaseApproved 的路径；每次批准、拒绝、撤销都有可审计记录；Runtime 阶段任务与当前逻辑阶段边界明确。

禁止运行 Player、导入资源或伪造审批。写 D18_L03_LOGIC_RESULT.json、handoff、记忆日志，运行状态同步与 git diff --check；最终报告 LOGIC_* 和未决的人工许可风险。
```

### D18-L04 — 许可证标准化工具

```text
执行 D18-L04（波次 1，并行）。阅读项目约定、D18-L01、现有 manifest 审计工具和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_license_normalizer.py、tools/tests/test_d18_license_normalizer.py、game/tools/results/D18/D18-L04/、docs/game/design/common/handoffs/D18/D18-L04_LOGIC_HANDOFF.md、当天记忆日志。不得改真实 manifest、Schema、catalog 或任何 Unity 文件。

实现纯离线许可证标准化：接受原始许可证名称/标识、署名要求、商用/修改/再分发字段；输出 canonical_license、attribution_required、commercial_allowed、redistribution_allowed 和确定性错误。未知、冲突、NC、个人使用限制、缺失字段必须保守返回阻断/Pending，不能猜测为 CC0。为 CC0、CC-BY、未知、冲突、NC、缺署名记录编写测试。

成功状态：测试能证明规范化不放宽限制；错误消息可供上层审计读取；无网络请求、无硬编码真实资源批准。

完成时写 D18_L04_LOGIC_RESULT.json、handoff、记忆日志，运行相关测试、python tools/sync_logic_task_status.py、git diff --check。最终报告 LOGIC_*、测试计数和 Runtime 延期项。
```

### D18-L05 — 来源证据验证器

```text
执行 D18-L05（波次 1，并行）。阅读 AGENTS、D18-L01、现有 D18 来源目录和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_provenance_validator.py、tools/tests/test_d18_provenance_validator.py、game/tools/results/D18/D18-L05/、docs/game/design/common/handoffs/D18/D18-L05_LOGIC_HANDOFF.md、当天记忆日志。

实现只读来源验证：检查 source_id、官方 source_url 格式、作者/组织、下载日期、原始包名、证据文件相对路径和来源/证据的一致性。不得访问网络来“确认”，不得把可访问 URL 当作许可证批准。缺任一 P0 证据时输出明确失败/Pending 原因。提供合法、缺作者、非官方 URL、绝对路径、未来日期、证据包不匹配等单测。

成功状态：工具只处理输入数据，不下载/导入资源；错误可定位字段；测试覆盖正反例。

禁止改真实 catalog、资源、Unity、场景和审批状态。写 D18_L05_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步和 git diff --check，报告 LOGIC_*。
```

### D18-L06 — 哈希与文件完整性验证器

```text
执行 D18-L06（波次 1，并行）。阅读 D18-L01、现有 manifest/audit 以及 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_hash_validator.py、tools/tests/test_d18_hash_validator.py、game/tools/results/D18/D18-L06/、docs/game/design/common/handoffs/D18/D18-L06_LOGIC_HANDOFF.md、当天记忆日志。

实现离线 SHA-256 验证：校验格式、相对文件路径、文件存在性和实际哈希匹配；检测相同 hash 的重复记录并报告，不删除文件。源文件不可用只能返回 Pending/缺证据，绝不能合成 hash 或批准资产。测试有效 hash、格式错误、文件缺失、内容被篡改、重复 hash 和路径越界。

成功状态：每种失败有不同错误码/原因；验证器不修改源文件/manifest；真实文件只读。

禁止资源导入、Unity、Player、网络和删除操作。写 D18_L06_LOGIC_RESULT.json、handoff、记忆日志，运行测试、状态同步、git diff --check；报告 LOGIC_*。
```

### D18-L07 — 分发权利验证器

```text
执行 D18-L07（波次 1，并行）。阅读 D18-L01、许可证现有记录和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_distribution_rights_validator.py、tools/tests/test_d18_distribution_rights_validator.py、game/tools/results/D18/D18-L07/、docs/game/design/common/handoffs/D18/D18-L07_LOGIC_HANDOFF.md、当天记忆日志。

实现权利验证：要求商业使用、修改、仓库存储、随游戏分发四项均有显式结论和证据；将 false/unknown/相互矛盾值区分；不得把“免费”或“可下载”解释为可商用/可再分发。给全允许、仅个人、NC、未知、矛盾和缺证据输入编写测试。

成功状态：任何未知/冲突都会阻断 Runtime/Release 迁移；工具不改审批状态、不访问网络。

写 D18_L07_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，最终报告 LOGIC_* 与人工审核延期。
```

### D18-L08 — 资源路径边界验证器

```text
执行 D18-L08（波次 1，并行）。阅读 D18-L01、架构文档、现有 Assets/Art 路径和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_asset_path_validator.py、tools/tests/test_d18_asset_path_validator.py、game/tools/results/D18/D18-L08/、docs/game/design/common/handoffs/D18/D18-L08_LOGIC_HANDOFF.md、当天记忆日志。

定义并验证 Candidate 源文件、逻辑元数据、被忽略的本地缓存、未来正式资源的路径边界。拒绝绝对路径、`..`、驱动器路径、临时目录、候选二进制直接出现在运行时引用字段。测试 Windows/Unix 分隔符、大小写、越界与合法相对路径。

成功状态：工具不会移动/删除/导入文件；错误输出说明允许根和违规路径；不修改 .gitignore 或真实资源目录。

写 D18_L08_LOGIC_RESULT.json、handoff、记忆日志，运行测试、状态同步、git diff --check；报告 LOGIC_*。
```

### D18-L09 — 署名信息验证器

```text
执行 D18-L09（波次 1，并行）。阅读 D18-L01、D18-L04 的接口说明和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_attribution_validator.py、tools/tests/test_d18_attribution_validator.py、game/tools/results/D18/D18-L09/、docs/game/design/common/handoffs/D18/D18-L09_LOGIC_HANDOFF.md、当天记忆日志。

实现署名验证：当 license/rights 标记必须署名时，要求作者、许可证文本/链接、展示/credits 位置和资源关联；CC0 不应伪造“必须署名”。缺展示位置、作者或许可引用必须失败。编写正负单测并输出可本地化的错误码。

成功状态：验证器只判断元数据，不改 credits、UI 或资源；不会将署名存在当作商业权利证明。

写 D18_L09_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步和 git diff --check，报告 LOGIC_*。
```

### D18-L10 — 资源包边界验证器

```text
执行 D18-L10（波次 1，并行）。阅读 D18-L01、现有 pack manifest 与 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_package_boundary_validator.py、tools/tests/test_d18_package_boundary_validator.py、game/tools/results/D18/D18-L10/、docs/game/design/common/handoffs/D18/D18-L10_LOGIC_HANDOFF.md、当天记忆日志。

实现包边界检查：资源记录必须可追溯到一个 pack、包内许可证证据、声明文件清单和相对路径；禁止一个文件被不兼容来源重复声明，禁止证据引用包外文件。仅处理元数据和文件清单，不读取/导入模型。

成功状态：合法包通过；包外证据、重复归属、无清单、许可证不在包内等失败；工具只读。

写 D18_L10_LOGIC_RESULT.json、handoff、记忆日志，运行测试、状态同步、git diff --check；报告 LOGIC_*。
```

### D18-L11 — 船舶/港口/建筑分类词典

```text
执行 D18-L11（波次 1，并行）。阅读 D18-L01、船/港口设计文档、现有 content catalog 与 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/D18_TAXONOMY_SHIPS_PORT.md、game/tools/results/D18/D18-L11/、docs/game/design/common/handoffs/D18/D18-L11_LOGIC_HANDOFF.md、当天记忆日志。

冻结 ship、hull_component、port_service、dock、berth、building、prop 等逻辑类别、子类、稳定 content_id 命名规则、弃用映射和不可混用关系。说明分类只服务数据/资源映射，不能改变船舶、港口、经济或场景状态。

成功状态：每类有命名例子、允许资源类型、所有者和 D19/D21 使用边界；不统计或声称模型已存在。

不改 catalog、资源、Unity 或 Systems。写 D18_L11_LOGIC_RESULT.json、handoff、记忆日志，运行状态同步与 git diff --check，报告 LOGIC_*。
```

### D18-L12 — 玩家/NPC 分类词典

```text
执行 D18-L12（波次 1，并行）。阅读 D18-L01、D10/D20 设计与 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/D18_TAXONOMY_CHARACTERS.md、game/tools/results/D18/D18-L12/、docs/game/design/common/handoffs/D18/D18-L12_LOGIC_HANDOFF.md、当天记忆日志。

定义 player_visual、npc_visual、hair、outfit、accessory、role_variant 等逻辑分类、content_id/ResourceKey 约束、性别/体型/职业的非裁决映射。明确视觉键不能写身份、关系、特质、金钱、库存或 NPC 行为。

成功状态：D20 可据此建立纯逻辑接口但不需要 Humanoid、Avatar、Prefab 或模型；敏感历史表达只复用既有审查标签，不虚构事实。

写 D18_L12_LOGIC_RESULT.json、handoff、记忆日志；运行状态同步和 git diff --check，报告 LOGIC_* 与模型/动画延期项。
```

### D18-L13 — 生态分类词典

```text
执行 D18-L13（波次 1，并行）。阅读 D18-L01、D11/D15 生态设计与 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/D18_TAXONOMY_ECOLOGY.md、game/tools/results/D18/D18-L13/、docs/game/design/common/handoffs/D18/D18-L13_LOGIC_HANDOFF.md、当天记忆日志。

定义陆生动物、鸟类、水生动物、珍稀物种、宠物、植被、地形、水域的逻辑分类、content_id、生态所有者和资源映射边界。强调分类不生成物种数量、不决定种群、不导入模型。

成功状态：D20/D21/D24 能验证类别/ID；每类有缺失/弃用规则，且没有将候选资源写为已运行时接入。

写 D18_L13_LOGIC_RESULT.json、handoff、记忆日志；运行状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L14 — UI/音频/材质分类词典

```text
执行 D18-L14（波次 1，并行）。阅读 D18-L01、D21/D22 设计和 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/D18_TAXONOMY_UI_AUDIO.md、game/tools/results/D18/D18-L14/、docs/game/design/common/handoffs/D18/D18-L14_LOGIC_HANDOFF.md、当天记忆日志。

定义 UI icon、input prompt、audio cue、music、material、texture、HDRI、weather presentation 等分类、许可字段、ResourceKey 规则与本地化/可访问性标签。明确 UI/材质分类不允许接入 Canvas、Shader、AudioSource 或视觉效果。

成功状态：分类可支持静态验证，且把实际导入、音量、颜色空间、视觉效果登记为 Runtime Deferred。

写 D18_L14_LOGIC_RESULT.json、handoff、记忆日志；运行状态同步与 git diff --check，报告 LOGIC_*。
```

### D18-L15 — content_id 与资源记录关联验证

```text
执行 D18-L15（波次 1，并行）。阅读 D18-L01、现有 catalog、D8/D15 content ID 约定和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_content_id_asset_link_validator.py、tools/tests/test_d18_content_id_asset_link_validator.py、game/tools/results/D18/D18-L15/、docs/game/design/common/handoffs/D18/D18-L15_LOGIC_HANDOFF.md、当天记忆日志。

实现校验：asset record 的 content_ids 必须存在、类型匹配且不重复；游戏业务 content_id 不能反向依赖 Candidate 文件路径或展示名。对缺 ID、错误类型、重复、已弃用 ID、候选路径泄漏编写测试。

成功状态：工具只读 catalog/定义；不改 content catalog、不创建私有 ID、不导入资源。

写 D18_L15_LOGIC_RESULT.json、handoff、记忆日志，运行测试、状态同步、git diff --check；报告 LOGIC_*。
```

### D18-L16 — ResourceKey 验证器

```text
执行 D18-L16（波次 1，并行）。阅读 D18-L01、现有 ResourceKey 使用、D8 数据模型和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_resource_key_validator.py、tools/tests/test_d18_resource_key_validator.py、game/tools/results/D18/D18-L16/、docs/game/design/common/handoffs/D18/D18-L16_LOGIC_HANDOFF.md、当天记忆日志。

实现 ResourceKey 唯一性、稳定命名、弃用别名、类别匹配和迁移规则验证。拒绝磁盘路径、文件扩展名、显示名称、空 key、重复 key 和跨类别碰撞。提供正负夹具。

成功状态：key 能独立于二进制资源/Unity GUID 存在；工具不修改 Registry 或运行时引用。

写 D18_L16_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L17 — 候选资源运行时引用防线

```text
执行 D18-L17（波次 1，并行）。阅读 D18-L01、架构边界、现有 game 资源引用和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_candidate_runtime_guard.py、tools/tests/test_d18_candidate_runtime_guard.py、game/tools/results/D18/D18-L17/、docs/game/design/common/handoffs/D18/D18-L17_LOGIC_HANDOFF.md、当天记忆日志。

实现静态扫描：检测运行时代码、配置和 Registry 对 `Assets/Art/Candidates`、临时下载包、Candidate 状态的直接引用；区分注释/文档和实际运行时引用；输出文件、行号、规则和修复方向。不可自动替换或删除引用。

成功状态：对人工正负夹具可可靠识别；不改 game 代码、不把现有候选升级为正式。

写 D18_L17_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_* 与后续 Runtime 消费风险。
```

### D18-L18 — 发布审批声明防线

```text
执行 D18-L18（波次 1，并行）。阅读 D18-L01、资源状态机和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_release_claim_guard.py、tools/tests/test_d18_release_claim_guard.py、game/tools/results/D18/D18-L18/、docs/game/design/common/handoffs/D18/D18-L18_LOGIC_HANDOFF.md、当天记忆日志。

扫描 manifest、逻辑 catalog 和活跃 D18 文档，检测 Candidate/IntakeEligible/RuntimeRequested 被写为 RuntimeApproved、ReleaseApproved、可发布或 Steam Ready 的越级声明。允许历史文档以 SUPERSEDED 描述旧结果，但不能当作通过。

成功状态：越级样例失败；历史引用不产生误报；工具不修改任何审批字段。

写 D18_L18_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L19 — 豁免与拒绝规则

```text
执行 D18-L19（波次 1，并行）。阅读 D18-L01、许可证/来源规则和 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/D18_WAIVER_AND_REJECTION_RULES.md、game/tools/results/D18/D18-L19/、docs/game/design/common/handoffs/D18/D18-L19_LOGIC_HANDOFF.md、当天记忆日志。

制定可审计的 Rejected、Pending、WaiverRequested、Revoked 规则：触发条件、证据、审批角色、期限、可读范围、恢复方式和记录字段。严格声明 waiver 不能绕过商业授权、不能变成 Runtime/Release 批准。

成功状态：缺许可、冲突来源、重复哈希、不可再分发、证据缺失都有明确处理；不修改真实条目状态。

写 D18_L19_LOGIC_RESULT.json、handoff、记忆日志；运行状态同步与 git diff --check，报告 LOGIC_*。
```

### D18-L20 — 逻辑资源索引入口

```text
执行 D18-L20（波次 1，并行）。阅读 D18-L01、L03/L11—L14 的已完成输出（若未完成，只链接基线并如实标注）和 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/D18_LOGIC_RESOURCE_INDEX.md、game/tools/results/D18/D18-L20/、docs/game/design/common/handoffs/D18/D18-L20_LOGIC_HANDOFF.md、当天记忆日志。

建立无二进制、无审批伪造的逻辑资源入口：链接 Schema、状态机、分类、验证器、来源 catalog 预定位置、唯一写者、下游 D19—D24 读取边界和 Runtime 延期清单。不得复制或改写其他任务的内容。

成功状态：后续 Agent 可从一页找到 D18 的事实源；未完成输出明确写 Pending/依赖，不写 PASS。

写 D18_L20_LOGIC_RESULT.json、handoff、记忆日志；运行链接检查（可用 rg/脚本）、状态同步、git diff --check，报告 LOGIC_*。
```

---

## 波次 2A：来源目录、导入描述与静态审计（同步并行；等待波次 1）

### D18-L21 — Kenney 静态来源目录

```text
执行 D18-L21（波次 2A；仅在 D18-L02—L20 的 P0 输出 LOGIC_PASS 后开始，可与 L22—L25、L29—L38 并行）。阅读 Schema、状态机、来源验证器、现有 Kenney manifest 和 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/sources/d18_kenney_logic_catalog.json、game/tools/results/D18/D18-L21/、docs/game/design/common/handoffs/D18/D18-L21_LOGIC_HANDOFF.md、当天记忆日志。

将现有 Kenney 候选转为 D18-L02 Schema 合法的静态记录：保留真实包名、来源、许可证据、哈希和文件清单；无法验证的字段必须 Pending/null，不得补猜。按 L11—L14 分类关联 content_id/ResourceKey，只登记、不可导入。

成功状态：Schema/来源/权利/哈希验证均真实通过或明确列出缺口；不声称模型、UI 或音频已可运行时使用。

写 D18_L21_LOGIC_RESULT.json、handoff、记忆日志；运行相关验证器、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L22 — Quaternius 静态来源目录

```text
执行 D18-L22（波次 2A，并行）。阅读 D18-L02—L10、现有 Quaternius 候选与 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/sources/d18_quaternius_logic_catalog.json、game/tools/results/D18/D18-L22/、docs/game/design/common/handoffs/D18/D18-L22_LOGIC_HANDOFF.md、当天记忆日志。

将实际存在的 Quaternius 候选写为 Schema 记录；逐条保留来源/包内许可证/哈希/文件证据。不能验证的许可证、哈希、文件或分发权利必须 `Pending`，不得因为来源知名而批准。分类只用 L11—L14 已定义 key。

成功状态：所有数量能由记录计算；无伪造 Avatar、动画、导入或可玩结论。

写 D18_L22_LOGIC_RESULT.json、handoff、记忆日志；运行 Schema 和验证器、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L23 — Poly Haven 静态来源目录

```text
执行 D18-L23（波次 2A，并行）。阅读 D18 状态机、来源/许可规则、现有 Poly Haven 资料和 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/sources/d18_polyhaven_logic_catalog.json、game/tools/results/D18/D18-L23/、docs/game/design/common/handoffs/D18/D18-L23_LOGIC_HANDOFF.md、当天记忆日志。

只为真实存在且有单项证据的 Poly Haven 材质/HDRI/模型建立记录；站点许可不能替代单项文件/来源/哈希记录。标明未来材质用途但不创建材质、不接 Shader、不导入。

成功状态：每条记录可由静态证据解释；缺项 Pending；不夸大为环境美术完成。

写 D18_L23_LOGIC_RESULT.json、handoff、记忆日志；运行验证器、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L24 — ambientCG 静态来源目录

```text
执行 D18-L24（波次 2A，并行）。阅读 D18-L02—L10、现有 ambientCG 资料和 LOGIC_TASK_PROTOCOL。

唯一写入范围：docs/game/design/common/assets/sources/d18_ambientcg_logic_catalog.json、game/tools/results/D18/D18-L24/、docs/game/design/common/handoffs/D18/D18-L24_LOGIC_HANDOFF.md、当天记忆日志。

按单项资源建立材质/贴图静态记录，校验来源、许可、哈希、文件、分发权利与分类。没有单项证据不可批准；不得下载、导入、压缩或创建 Unity 材质。

成功状态：Schema 和 D18 验证器的结论可复现；真实缺口可见。

写 D18_L24_LOGIC_RESULT.json、handoff、记忆日志；运行验证器、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L25 — 未来导入描述 Schema

```text
执行 D18-L25（波次 2A，并行）。阅读 D18-L02、L03、角色/船/生态分类与 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/schemas/d18_import_descriptor.schema.json、tools/tests/d18_import_descriptor_fixtures/、game/tools/results/D18/D18-L25/、docs/game/design/common/handoffs/D18/D18-L25_LOGIC_HANDOFF.md、当天记忆日志。

定义未来 Runtime 导入描述：source asset_id、类型、单位/轴向、bounds、材质槽、骨架类型、动画列表、root-motion、碰撞意图、LOD 意图、许可证依赖和回退。Schema 必须显式标明这不是 Unity Import Settings，也不能表示资源已导入。

成功状态：合法/非法夹具覆盖角色、船、环境、UI/音频的描述差异；无 Unity API/Prefab 改动。

写 D18_L25_LOGIC_RESULT.json、handoff、记忆日志；运行 Schema 校验、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L26 — 网格描述验证器

```text
执行 D18-L26（波次 2B；必须等待 D18-L25=LOGIC_PASS，可与 L27/L28 并行）。阅读 D18-L25 Schema、D20/D21 逻辑边界和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_mesh_descriptor_validator.py、tools/tests/test_d18_mesh_descriptor_validator.py、game/tools/results/D18/D18-L26/、docs/game/design/common/handoffs/D18/D18-L26_LOGIC_HANDOFF.md、当天记忆日志。

验证网格描述的单位、轴向、root、bounds、碰撞意图、LOD 意图、类别匹配和缺失回退字段；实际顶点、碰撞、LOD 和导入检查必须延期。非法值输出字段级错误。

成功状态：不读取 FBX/OBJ、不调用 Unity；正负单测覆盖每个 P0 字段。

写 D18_L26_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L27 — 材质描述验证器

```text
执行 D18-L27（波次 2B；必须等待 D18-L25=LOGIC_PASS，可与 L26/L28 并行）。阅读 D18-L25、L14、来源许可规则和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_material_descriptor_validator.py、tools/tests/test_d18_material_descriptor_validator.py、game/tools/results/D18/D18-L27/、docs/game/design/common/handoffs/D18/D18-L27_LOGIC_HANDOFF.md、当天记忆日志。

验证材质/贴图描述：关联 asset_id、类别、许可依赖、颜色空间意图、通道语义、回退和运行时延期字段。不得创建 Material、Shader 或 TextureImporter 设置；实际图像检查延期。

成功状态：错误的许可依赖、通道、类别、空回退被拒绝；测试可重复。

写 D18_L27_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L28 — 动画描述验证器

```text
执行 D18-L28（波次 2B；必须等待 D18-L25=LOGIC_PASS，可与 L26/L27 并行）。阅读 D18-L25、D20 动作设计和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_animation_descriptor_validator.py、tools/tests/test_d18_animation_descriptor_validator.py、game/tools/results/D18/D18-L28/、docs/game/design/common/handoffs/D18/D18-L28_LOGIC_HANDOFF.md、当天记忆日志。

验证动画用途、骨架类型、循环、root-motion 策略、动作意图、所需 fallback、许可证依赖和状态机归属；不得导入 clip、创建 Animator 或 Avatar。冲突的骨架/动作意图/缺 fallback 必须失败。

成功状态：工具不涉及 Unity；测试覆盖玩家/NPC/动物/缺动作情形。

写 D18_L28_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L29 — UI/音频描述验证器

```text
执行 D18-L29（波次 2A，并行）。阅读 D18-L14、L25、D22 逻辑边界和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_ui_audio_descriptor_validator.py、tools/tests/test_d18_ui_audio_descriptor_validator.py、game/tools/results/D18/D18-L29/、docs/game/design/common/handoffs/D18/D18-L29_LOGIC_HANDOFF.md、当天记忆日志。

验证 UI/音频描述的用途、分类、license、localization/accessibility 标签、替代文本/静音回退和运行时意图。不得创建 Canvas、AudioSource、按钮、混音器或导入音频。

成功状态：未知用途、缺许可、缺本地化标签、无回退均可识别；测试只处理 JSON/数据。

写 D18_L29_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L30 — 资源依赖图工具

```text
执行 D18-L30（波次 2A，并行）。阅读 D18 Schema、状态机、catalog 输出和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_asset_dependency_graph.py、tools/tests/test_d18_asset_dependency_graph.py、game/tools/results/D18/D18-L30/、docs/game/design/common/handoffs/D18/D18-L30_LOGIC_HANDOFF.md、当天记忆日志。

从静态 asset record/import descriptor 生成稳定依赖图；检测循环、悬挂引用、跨包不兼容、Candidate 到 RuntimeApproved/ReleaseApproved 越级边和缺许可证节点。输出 JSON/DOT/文本之一即可，但格式固定、可测试、只读。

成功状态：小型正负图夹具验证所有错误类型；不扫描/导入二进制，不改 catalog。

写 D18_L30_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L31 — 重复身份审计

```text
执行 D18-L31（波次 2A，并行）。阅读 D18-L06、L16、现有资源证据和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_duplicate_identity_audit.py、tools/tests/test_d18_duplicate_identity_audit.py、game/tools/results/D18/D18-L31/、docs/game/design/common/handoffs/D18/D18-L31_LOGIC_HANDOFF.md、当天记忆日志。

按 sha256、source_id、canonical_id、original_filename 检测精确重复、疑似重复和合法变体；每类输出可复核原因。只登记重复，绝不删除、重命名或移动资源。

成功状态：不会把同名不同 hash 或同 hash 合法镜像误写为“已修复”；测试覆盖所有分类。

写 D18_L31_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L32 — 命名冲突审计

```text
执行 D18-L32（波次 2A，并行）。阅读 D18-L11—L16、Schema 和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_name_collision_audit.py、tools/tests/test_d18_name_collision_audit.py、game/tools/results/D18/D18-L32/、docs/game/design/common/handoffs/D18/D18-L32_LOGIC_HANDOFF.md、当天记忆日志。

检查 canonical_id、ResourceKey、content_id、显示名、原始文件名之间的冲突和弃用别名循环；输出冲突对、字段、严重度和推荐动作。不得自动重命名或改 Registry。

成功状态：大小写、别名、多类别、同显示名合法情形均有测试；报告确定性。

写 D18_L32_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L33 — 目录数量声明审计

```text
执行 D18-L33（波次 2A，并行）。阅读 D18-L20、L21—L24 目录格式、现有需求数字与 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_catalog_claim_audit.py、tools/tests/test_d18_catalog_claim_audit.py、game/tools/results/D18/D18-L33/、docs/game/design/common/handoffs/D18/D18-L33_LOGIC_HANDOFF.md、当天记忆日志。

实现从实际静态记录按分类/状态计算数量，并核验文档/报告的数量声明；若没有来源记录，返回未知/不支持，不能用需求目标或文件夹数补齐。检测重复记录导致的虚增。

成功状态：工具输出真实计算口径和差异；不修改现有设计的目标数量，不把目标当完成数。

写 D18_L33_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L34 — 缺失证据审计

```text
执行 D18-L34（波次 2A，并行）。阅读 D18-L02—L10、L21—L24 目录格式和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_missing_evidence_audit.py、tools/tests/test_d18_missing_evidence_audit.py、game/tools/results/D18/D18-L34/、docs/game/design/common/handoffs/D18/D18-L34_LOGIC_HANDOFF.md、当天记忆日志。

生成每个记录缺少的 source/license/hash/rights/content_id/descriptor 证据，按 P0/P1/P2 分类并给出不越权的下一步。不得自动填充、批准或下载。

成功状态：空记录、部分记录、完整记录和冲突记录覆盖；输出机器可读且稳定。

写 D18_L34_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L35 — 状态迁移审计

```text
执行 D18-L35（波次 2A，并行）。阅读 D18-L03 状态机、D18-L02 Schema 和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_status_transition_audit.py、tools/tests/test_d18_status_transition_audit.py、game/tools/results/D18/D18-L35/、docs/game/design/common/handoffs/D18/D18-L35_LOGIC_HANDOFF.md、当天记忆日志。

实现表驱动状态迁移审计：批准、拒绝、撤销、证据失效、Runtime 请求和发布请求必须符合状态机、权利和证据。特别覆盖 Candidate 越级、Revoked 恢复、过期 waiver 和 ReleaseApproved 伪造。

成功状态：每条允许/禁止迁移都有测试；工具不改变真实记录状态。

写 D18_L35_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L36 — LocalDevelopmentReady 防线

```text
执行 D18-L36（波次 2A，并行）。阅读 D18-L01、L03、L17、L18、旧 D18 状态和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_local_development_guard.py、tools/tests/test_d18_local_development_guard.py、game/tools/results/D18/D18-L36/、docs/game/design/common/handoffs/D18/D18-L36_LOGIC_HANDOFF.md、当天记忆日志。

定义并校验 LocalDevelopmentReady 的最小逻辑条件：来源/许可/权利/哈希/分类/描述完整及未越级；明确它不等于 Unity 导入、Prefab、运行时消费者、性能或 ReleaseApproved。工具必须输出缺失条件。

成功状态：旧候选/旧 Player 截图不能导致 Ready；所有正负夹具结论确定。

写 D18_L36_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L37 — Runtime 晋级请求 Schema

```text
执行 D18-L37（波次 2A，并行）。阅读 D18-L03、L25、L36 和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/schemas/d18_runtime_promotion_request.schema.json、tools/tests/d18_runtime_promotion_request_fixtures/、game/tools/results/D18/D18-L37/、docs/game/design/common/handoffs/D18/D18-L37_LOGIC_HANDOFF.md、当天记忆日志。

定义未来 Runtime 阶段的“请求”Schema：asset_id、当前状态、目标使用场景、描述器、碰撞/LOD/动画需求、审批人、风险和回退。Schema 只能表达 request，禁止通过 Schema 本身产生 RuntimeApproved/ReleaseApproved。

成功状态：合法请求和越级/证据缺失/非法目标状态夹具均可验证；不改任何审批目录。

写 D18_L37_LOGIC_RESULT.json、handoff、记忆日志；运行 Schema 校验、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L38 — 派生/生成资源防线

```text
执行 D18-L38（波次 2A，并行）。阅读 D18-L02、L03、来源/许可规则和 LOGIC_TASK_PROTOCOL。

唯一写入范围：tools/d18_generated_asset_guard.py、tools/tests/test_d18_generated_asset_guard.py、game/tools/results/D18/D18-L38/、docs/game/design/common/handoffs/D18/D18-L38_LOGIC_HANDOFF.md、当天记忆日志。

实现原件、转换件、派生件、生成件的谱系检查：派生记录必须指向原始来源和转换步骤；禁止将生成物伪装为第三方原件或绕过许可/权利；缺谱系必须阻断批准。

成功状态：正负谱系夹具通过；不生成任何贴图/模型/二进制文件，不修改真实资源。

写 D18_L38_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

---

## 波次 3：严格顺序的整合与测试

### D18-L39 — 聚合逻辑审计入口

```text
执行 D18-L39（波次 3，第 1 项；必须确认 L02—L38 已结束并读取其结果）。

唯一写入范围：tools/d18_logic_audit.py、tools/tests/test_d18_logic_audit.py、game/tools/results/D18/D18-L39/、docs/game/design/common/handoffs/D18/D18-L39_LOGIC_HANDOFF.md、当天记忆日志。不要修改 L02—L38 的工具、真实 catalog、Unity 或资源。

把已交付的 Schema/验证器以只读方式汇总为一个可重复入口：输入静态目录/manifest，输出每条记录、每条规则、P0/P1 缺口、状态迁移/路径/许可问题和总计。不得吞掉子工具错误，不能因子工具缺失伪造 PASS；缺接口写 BLOCKED 并列出任务 ID。

成功状态：使用固定夹具运行时可定位每个子规则；无网络、无 Unity、无目录写入；输出机器可读 JSON。

写 D18_L39_LOGIC_RESULT.json、handoff、记忆日志；运行聚合测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L40 — 合法记录测试夹具

```text
执行 D18-L40（波次 3，第 2 项；等待 L39 LOGIC_PASS）。

唯一写入范围：tools/tests/test_d18_logic_valid_records.py、tools/tests/fixtures/d18_logic_valid/、game/tools/results/D18/D18-L40/、docs/game/design/common/handoffs/D18/D18-L40_LOGIC_HANDOFF.md、当天记忆日志。

为船港、角色、生态、UI/音频、材质等至少五类完全合法的静态记录建立夹具；每类覆盖来源、许可、权利、hash、分类、content_id、ResourceKey、状态和描述器。调用 L39 聚合入口，不复制验证逻辑。

成功状态：全部合法夹具通过，且测试断言不会产生 RuntimeApproved/ReleaseApproved；夹具不含真实二进制或伪造下载证据。

写 D18_L40_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L41 — 非法记录测试夹具

```text
执行 D18-L41（波次 3，第 3 项；等待 L40 LOGIC_PASS）。

唯一写入范围：tools/tests/test_d18_logic_invalid_records.py、tools/tests/fixtures/d18_logic_invalid/、game/tools/results/D18/D18-L41/、docs/game/design/common/handoffs/D18/D18-L41_LOGIC_HANDOFF.md、当天记忆日志。

建立每类 P0 违规的最小夹具：缺来源、许可冲突、缺 hash、路径越界、重复 identity、状态越级、无分发权利、坏 content_id、生成物无谱系、伪发布声明。每项必须断言 L39 输出正确规则与严重度。

成功状态：负例均失败且原因稳定；不修改生产 catalog/工具逻辑。

写 D18_L41_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L42 — 逻辑 catalog 编译器

```text
执行 D18-L42（波次 3，第 4 项；等待 L41 LOGIC_PASS）。

唯一写入范围：tools/d18_logic_catalog_compiler.py、tools/tests/test_d18_logic_catalog_compiler.py、game/tools/results/D18/D18-L42/、docs/game/design/common/handoffs/D18/D18-L42_LOGIC_HANDOFF.md、当天记忆日志。

实现从 D18-L21—L24 的静态来源目录编译规范逻辑 catalog：排序稳定、只包含 Schema 合法记录、保留 Pending/Rejected、输出审计摘要。编译器不得写 Unity Assets、Prefab、运行时 Registry、审批状态或下载缓存。

成功状态：相同输入两次得到同一输出；非法输入被拒绝；测试覆盖多来源合并和冲突。

写 D18_L42_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L43 — 真实静态来源审计报告

```text
执行 D18-L43（波次 3，第 5 项；等待 L42 LOGIC_PASS）。

唯一写入范围：docs/game/design/common/D18_LOGIC_SOURCE_AUDIT.md、game/tools/results/D18/D18-L43/、docs/game/design/common/handoffs/D18/D18-L43_LOGIC_HANDOFF.md、当天记忆日志。

运行 L39/L42 对现有真实静态目录进行只读审计。报告每个来源/类别的真实记录数、合法数、Pending、Rejected、P0/P1 缺口和不可验证项目；所有数字必须来自工具输出。不得补下载、不得修改源目录、不得把 Pending 说成 Ready。

成功状态：报告引用机器结果、命令、输入路径和时间；明确 Runtime/人工许可延期。

写 D18_L43_LOGIC_RESULT.json、handoff、记忆日志；运行状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L44 — 游戏资源引用只读审计

```text
执行 D18-L44（波次 3，第 6 项；等待 L43 LOGIC_PASS）。

唯一写入范围：tools/d18_game_reference_audit.py、tools/tests/test_d18_game_reference_audit.py、game/tools/results/D18/D18-L44/、docs/game/design/common/handoffs/D18/D18-L44_LOGIC_HANDOFF.md、当天记忆日志。

只读扫描 game 代码、数据和 Registry：确认业务逻辑使用稳定 content_id/ResourceKey，而不是 Candidate 路径、临时目录、下载 zip 或显示名；定位违规文件/行号并给出所有者。不得自动改 game 代码或 Registry。

成功状态：人工正负夹具有效，报告可区分测试/文档引用与实际运行时引用；不启动 Unity。

写 D18_L44_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L45 — Schema/工具兼容性审计

```text
执行 D18-L45（波次 3，第 7 项；等待 L44 LOGIC_PASS）。

唯一写入范围：tools/d18_schema_compatibility_audit.py、tools/tests/test_d18_schema_compatibility_audit.py、game/tools/results/D18/D18-L45/、docs/game/design/common/handoffs/D18/D18-L45_LOGIC_HANDOFF.md、当天记忆日志。

校验 D18 的 Schema、catalog compiler、验证器、聚合审计和结果 JSON 的字段名称、状态枚举、版本和错误码一致；检测某工具接受另一个工具拒绝的语义分裂。不得回头批量重写子工具，发现问题记录精确 blocker/责任任务。

成功状态：固定兼容夹具和不兼容夹具均有预期结果；输出可给 D24 使用。

写 D18_L45_LOGIC_RESULT.json、handoff、记忆日志；运行测试、状态同步、git diff --check，报告 LOGIC_*。
```

### D18-L46 — D18 对下游的逻辑交接接口

```text
执行 D18-L46（波次 3，第 8 项；等待 L45 LOGIC_PASS）。

唯一写入范围：docs/game/design/common/D18_LOGIC_HANDOFF_INTERFACE.md、game/tools/results/D18/D18-L46/、docs/game/design/common/handoffs/D18/D18-L46_LOGIC_HANDOFF.md、当天记忆日志。

发布 D19—D24 可读取的 D18 接口说明：有效 Schema/版本、合法 ResourceKey/content_id 规则、状态含义、不可消费状态、错误码、来源 catalog、审计命令、唯一写者和 Runtime Deferred 清单。不要创建任何运行时资源/Registry 或批准条目。

成功状态：下游能据此只读使用逻辑数据，并明确知道不得引用 Candidate 二进制或声称资源已导入。

写 D18_L46_LOGIC_RESULT.json、handoff、记忆日志；运行链接/格式检查、状态同步、git diff --check，报告 LOGIC_*。
```

---

## 波次 4：最终汇合验收（严格顺序）

### D18-L47 — P0 证据复核

```text
执行 D18-L47（波次 4，第 1 项；等待 L39—L46 全部完成）。

唯一写入范围：game/tools/results/D18/D18-L47/、docs/game/design/common/handoffs/D18/D18-L47_LOGIC_HANDOFF.md、当天记忆日志。不得改任何生产工具、Schema、catalog、资源或 Unity 文件。

只读复核 L02—L46：验证每个 P0 Schema、验证器、夹具、聚合审计、真实来源报告、引用审计、兼容性审计均存在且实际运行。缺结果、失败、无测试或范围越界必须保留为 LOGIC_BLOCKED/PARTIAL，不能因为其它任务通过而覆盖。

成功状态：输出任务级 P0 证据矩阵、命令、结果、缺口和下一步；不作 Release/Runtime 结论。

写 D18_L47_LOGIC_RESULT.json、handoff、记忆日志；运行状态同步和 git diff --check，报告 LOGIC_*。
```

### D18-L48 — 二进制与导入延期审计

```text
执行 D18-L48（波次 4，第 2 项；等待 L47 LOGIC_PASS）。

唯一写入范围：docs/game/design/common/D18_BINARY_AND_IMPORT_DEFERRAL.md、game/tools/results/D18/D18-L48/、docs/game/design/common/handoffs/D18/D18-L48_LOGIC_HANDOFF.md、当天记忆日志。

只读审计本轮改动和 D18 结果：确认没有下载/导入外部二进制、创建/编辑 Prefab、场景、Canvas、Build Settings、Player 或运行时 Registry。列出未来 Runtime 阶段必须验证的模型、贴图、骨架、动画、碰撞、LOD、材质、UI、音频和性能项目。

成功状态：没有把“静态记录完整”表述为“资源可用”；任何发现的越界写入列为 blocker 和文件路径。

写 D18_L48_LOGIC_RESULT.json、handoff、记忆日志；运行状态同步与 git diff --check，报告 LOGIC_*。
```

### D18-L49 — 状态与留痕完整性审计

```text
执行 D18-L49（波次 4，第 3 项；等待 L48 LOGIC_PASS）。

唯一写入范围：docs/game/design/common/D18_LOGIC_STATUS_REVIEW.md、game/tools/results/D18/D18-L49/、docs/game/design/common/handoffs/D18/D18-L49_LOGIC_HANDOFF.md、当天记忆日志。

复核 50 个任务的结果 JSON 格式、task_id、状态、验证命令、handoff、记忆日志和状态总览。运行 python tools/sync_logic_task_status.py 后核对总数、每项链接和结果摘要。不得手改 LOGIC_TASK_STATUS.md，不得把 PLANNED 伪装为 PASS。

成功状态：每项有唯一事实源；缺失项列表明确；状态总览显示的总数仍为 50 个 D18 任务。

写 D18_L49_LOGIC_RESULT.json、handoff、记忆日志；运行状态同步与 git diff --check，报告 LOGIC_*。
```

### D18-L50 — D18 纯逻辑验收

```text
执行 D18-L50（波次 4，第 4 项；等待 L49 LOGIC_PASS）。这是 D18 唯一章节验收任务。

唯一写入范围：docs/game/design/common/D18_LOGIC_ACCEPTANCE.md、game/tools/results/D18/D18-L50/、docs/game/design/common/handoffs/D18/D18-L50_LOGIC_HANDOFF.md、当天记忆日志。只读所有前序 D18 输出；禁止修复子任务、改资源、导入 Unity、启动 Player 或修改场景/Prefab。

基于 L47—L49 的真实证据给出唯一结论：LOGIC_PASS、LOGIC_PARTIAL 或 LOGIC_BLOCKED。逐项列出 P0 条件：Schema/状态机、来源/许可/权利/哈希、分类/键、静态目录、验证器、正负夹具、聚合审计、运行时引用防线、兼容性、状态留痕、二进制延期。只有 P0 全部有 PASS 证据时才可写 LOGIC_PASS。

若通过，明确放行给 D19 的仅是“逻辑资源接口和静态审计输入”，不是模型导入、Prefab、画面、运行时资源消费或发布。若不通过，列出最小回退任务编号，不要新建 RERUN。

写 D18_L50_LOGIC_RESULT.json、handoff、记忆日志；运行 python tools/sync_logic_task_status.py 和 git diff --check。最终报告真实 LOGIC_*、放行/阻断条件、Runtime Deferred 清单和结果路径。
```

---

## 收尾修复波次：严格串行，禁止并行

> `D18-L50` 的首次结论为 `LOGIC_PARTIAL`，原因是实际目录把细分类写进了 11 值大类字段。以下三项不是重跑或伪造通过，而是可追溯的正式修复链。必须严格按 `L51 → L52 → L53` 运行。

### D18-L51 — 大类与细分类契约一致性修复

```text
执行 D18-L51（收尾修复第 1 项；前置：D18-L01—L49=LOGIC_PASS、D18-L50=LOGIC_PARTIAL）。直接完成，不等待用户确认。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前必须阅读：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md、docs/game/AGENT_DEVELOPMENT_CHAPTER_10_D18.md、D18-L50/L45/L42/L39 的结果与交接报告，以及 L02、L11—L14 的 Schema/词典。

问题事实：L02 冻结 `category` 为 11 个资源大类；Quaternius 来源目录 61 条记录却把 `terrestrial_animal`、`aquatic_animal`、`bird`、`pet`、`npc_visual`、`player_visual`、`hair`、`outfit` 8 个细分类直接写入该字段，导致 L39 接受、L42 拒绝。不得扩大 L02 的 11 个大类枚举来掩盖问题，也不得删除、拒绝、伪造或改写这 61 条的来源/许可/哈希/状态事实。

唯一工作目标：建立明确的双层分类契约。
1. `category` 永远是现有 L02 的 11 值资源大类；
2. 新的可选 `taxonomy_category` 表示领域细分类；其取值必须来自一个机器可读的、版本化的“细分类 → 大类”映射；
3. 当 `taxonomy_category` 存在时，映射出的父类必须等于 `category`；未知细分类或父类不匹配必须产生 P0；没有细分类的既有大类记录仍然合法；
4. 本次 61 条 Quaternius 的精确迁移规则固定为：`terrestrial_animal`、`bird`、`pet` → `category=animal`；`aquatic_animal` → `category=animal`；`npc_visual`、`player_visual`、`hair`、`outfit` → `category=character`。原细分类必须逐条保存在 `taxonomy_category`，不得丢失；
5. `fish` 仍是 L02 的独立大类，供真正鱼类资源记录使用；不得因为本批水生动物记录迁移为 animal 而删除该大类。

允许且必须修改的范围（仅限本任务）：
- `tools/schemas/d18_logic_asset_record.schema.json`：增加受约束的可选 `taxonomy_category` 字段，不改变既有 13 个必填字段、状态机或 Runtime/Release 边界；
- 新建 `tools/schemas/d18_taxonomy_category_map.json`：只登记本章已有词典实际使用、且能映射到 11 值大类的细分类；每项必须有 parent_category、owner 文档和版本字段；
- `docs/game/design/common/D18_LOGIC_GOVERNANCE_BASELINE.md`、`docs/game/design/common/assets/D18_TAXONOMY_CHARACTERS.md`、`docs/game/design/common/assets/D18_TAXONOMY_ECOLOGY.md`、`docs/game/design/common/assets/D18_TAXONOMY_UI_AUDIO.md`：将“细分类直接写入 category”的旧描述改为双层定义，并链接机器映射；不得改变角色/生态系统的数值、所有权或 Runtime 延期声明；
- `docs/game/design/common/assets/sources/d18_quaternius_logic_catalog.json`：仅按上面的八条精确规则迁移 61 条记录；其他字段的语义值、顺序稳定性、总数和证据路径不得改变；
- `tools/d18_logic_audit.py`、`tools/d18_logic_catalog_compiler.py` 和各自现有测试：两者必须使用同一份 L02 Schema 与细分类映射语义；L39 必须像 L42 一样把非法大类、未知细分类、父类不匹配报告为 P0，不能再只做字段覆盖；L42 必须接受迁移后合法的 161 条来源记录，拒绝故意构造的错误映射；
- 对应 Schema/工具测试、`game/tools/results/D18/D18-L51/`、`docs/game/design/common/handoffs/D18/D18-L51_LOGIC_HANDOFF.md`、当天记忆日志。

禁止：启动 Unity/Player/PlayMode；下载或导入资源；创建或修改 Prefab、Scene、Canvas、Build Settings、Registry；改 D19 灯光文件（留给 L52）；修改 L45/L50 的正式结果；把 Candidate 改为 IntakeEligible/RuntimeApproved/ReleaseApproved；用跳过测试、宽松通配符或未校验的自由文本绕过映射。

必须新增或调整的测试至少覆盖：无 taxonomy_category 的旧记录合法；8 个真实细分类的父类映射合法；未知 taxonomy_category 阻断；taxonomy_category 与 category 父类不匹配阻断；L39 与 L42 对同一正负记录结论一致；相同输入两次编译字节一致。

必须执行并如实记录：
1. 对四个真实来源 catalog 的 L02 Schema 校验：161/161 合法；
2. `python -m unittest tools.tests.test_d18_logic_audit -v`；
3. `python -m unittest tools.tests.test_d18_logic_catalog_compiler -v`；
4. 相关 L02 Schema 测试；
5. `python tools/d18_logic_audit.py --output game/tools/results/D18/D18-L51/D18_L51_LOGIC_AUDIT.json`：必须 records=161、P0=0；4 个 HDRI 的 hash_pending 仍可作为 P1，如实保留；
6. `python tools/d18_logic_catalog_compiler.py --output game/tools/results/D18/D18-L51/D18_L51_COMPILED_CATALOG.json`：必须 input=161、compiled=161、rejected=0、conflicts=0；
7. `python tools/sync_logic_task_status.py`、`git diff --check`。

成功状态：只有双层契约、61 条迁移、L39/L42 同语义及以上验证全部通过，且没有 P0 blocker 时才写 LOGIC_PASS。完成时写 `game/tools/results/D18/D18-L51/D18_L51_LOGIC_RESULT.json`、handoff 和记忆日志；结果必须列出 61 条迁移计数、161/161 Schema 结果、4 条仍 Pending 的 HDRI 哈希以及所有 Runtime 延期项。
```

### D18-L52 — 候选键运行时引用修复

```text
执行 D18-L52（收尾修复第 2 项；硬前置：D18-L51=LOGIC_PASS）。直接完成，不等待用户确认。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前必须阅读：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md、D18-L44/L50 结果、D18-L51 结果，以及 `docs/game/design/common/assets/sources/d18_polyhaven_logic_catalog.json`。

问题事实：`game/Assets/Settings/D19/Lighting/D19LightingProfileCatalog.json` 的五个 `sky_reference_asset` 仍把候选键当业务键。L44 因此检出 5 个 runtime P0。已登记的稳定 content_id 是：
- candidate_hdri_green_point_park → asset_hdri_green_point_park
- candidate_hdri_kloppenheim_06 → asset_hdri_kloppenheim_06（出现两处，必须都改）
- candidate_hdri_joburg_central_sunset → asset_hdri_joburg_central_sunset
- candidate_hdri_venice_sunset → asset_hdri_venice_sunset

唯一写入范围：
- `game/Assets/Settings/D19/Lighting/D19LightingProfileCatalog.json`；
- 仅当现有 D19 设置测试明确断言旧候选键时，修改该测试为稳定键断言；
- `game/tools/results/D18/D18-L52/`、`docs/game/design/common/handoffs/D18/D18-L52_LOGIC_HANDOFF.md`、当天记忆日志。

只修改上述五个业务引用及必要测试；不得改变天气映射、颜色、数值、profile_id、许可证、资源状态、D18 catalog、C#、Scene、Prefab、Canvas 或任何候选二进制。`asset_hdri_*` 仍是 Candidate 静态记录，不得在此任务声明导入、RuntimeApproved 或 ReleaseApproved。

必须执行并如实记录：
1. 用 Python 标准库解析该 JSON，断言五个 profile 的 `sky_reference_asset` 只属于上述四个 `asset_hdri_*`；
2. 从 Poly Haven 静态目录读取并断言每个引用都存在于其 `content_ids`；
3. `rg -n --glob '*.json' 'candidate_hdri_' game/Assets/Settings` 必须没有命中；
4. `python -m unittest tools.tests.test_d18_game_reference_audit -v`；
5. `python tools/d18_game_reference_audit.py --output game/tools/results/D18/D18-L52/D18_L52_GAME_REFERENCE_SCAN.json`：`status=PASS`、`runtime_findings_count=0`。既有 Editor-only INFO 可保留，但不得被写成已修复的 Runtime 引用；
6. `python tools/sync_logic_task_status.py`、`git diff --check`。

成功状态：五个正式配置引用均为稳定键、目录可解析、L44 扫描没有 runtime P0 且测试通过时才写 LOGIC_PASS。完成时写 `game/tools/results/D18/D18-L52/D18_L52_LOGIC_RESULT.json`、handoff、记忆日志；清楚声明这是配置键修复，不是资源导入或表现验收。
```

### D18-L53 — D18 收尾复验与正式放行

```text
执行 D18-L53（收尾修复第 3 项；硬前置：D18-L51=LOGIC_PASS 且 D18-L52=LOGIC_PASS）。这是 D18 的最终收尾门禁。直接完成，不等待用户确认。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前必须阅读：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md、D18-L45/L50 旧结果、L51/L52 新结果，以及 D18-L39—L49 的现有交接报告。

任务目标：不掩盖首次 L50=LOGIC_PARTIAL 的历史，而是在 L51/L52 已修复根因后，重跑所有受影响的事实证据链，使 D18-L50 的当前结果可诚实升级为 LOGIC_PASS，并以 L53 证明 53 项 D18 任务都完整可追溯。

允许写入范围：
- L39、L42—L45 的既有机器输出、结果 JSON 和 handoff；L45 工具/测试仅为将“真实目录应 BLOCKED”的旧回归断言改为“迁移后真实目录 PASS，并以注入的错误分类/错误父类记录继续验证 BLOCKED”；
- `docs/game/design/common/D18_LOGIC_SOURCE_AUDIT.md`（L43）、`docs/game/design/common/D18_LOGIC_HANDOFF_INTERFACE.md`（L46）、`docs/game/design/common/D18_BINARY_AND_IMPORT_DEFERRAL.md`（L48）、`docs/game/design/common/D18_LOGIC_STATUS_REVIEW.md`（L49）、`docs/game/design/common/D18_LOGIC_ACCEPTANCE.md`（L50），及其对应结果 JSON/handoff；
- `tools/tests/test_d18_s02_r01_intake_refresh.py`：只修复其中与已归档 D18-S02-R01 现实不一致的断言，使它测试当前目录的真实事实和“不会推断 Runtime/Release 审批”的历史兼容行为。禁止删除文件、`skip`、`expectedFailure`、降低断言、伪造旧计数或把测试从默认发现中排除；
- `game/tools/results/D18/D18-L53/`、`docs/game/design/common/handoffs/D18/D18-L53_LOGIC_HANDOFF.md`、当天记忆日志。

禁止：修改 L51/L52 的业务实现或输入 catalog；修改来源/许可/哈希事实；下载/导入资源；启动 Unity/Player/PlayMode；修改 Scene、Prefab、Canvas、Build Settings、Runtime Registry；授予任何 RuntimeApproved/ReleaseApproved；把 4 条 HDRI 的缺失逐文件哈希伪造为已补齐。

严格执行下列顺序，并以当前真实输出更新各任务结果。不得只改状态 JSON：
1. 重跑 L39 聚合审计；必须 records=161、P0=0，4 条 HDRI hash_pending 保持 P1/Pending；
2. 重跑 L42 编译器；必须 input=161、compiled=161、rejected=0、conflicts=0；
3. 重写 L43 静态来源审计报告，所有数字必须来自本次机器输出；
4. 重跑 L44；必须 runtime_findings_count=0。Editor-only INFO 若存在，只能列为 Runtime Deferred；
5. 更新 L45 并重跑兼容性审计；五个公共语义槽一致，`category_split_real_data=0`、`cross_tool_acceptance_split=0`。必须新增负向测试，证明未知 taxonomy_category 或错误 parent category 仍会 BLOCKED；
6. 更新 L46 交接接口，明确 `category`=11 值大类、`taxonomy_category`=可选细分类，以及下游只能读取静态数据；
7. 重跑 L47、L48、L49：L49 必须按 53 个 D18 任务核对，L48 必须确认本收尾没有二进制/导入/Scene/Prefab 越界；
8. 重跑 L50，更新验收文档、结果 JSON、handoff。新的 L50 必须保留“首次验收为 PARTIAL、由 L51/L52 修复”的历史说明；只有 11 项 P0 均有 PASS 证据时才写 LOGIC_PASS；
9. 运行 `python -m unittest discover -s tools/tests -p 'test*.py'`。默认发现的全量工具测试必须 exit 0；若失败，修复根因或如实写 LOGIC_PARTIAL/BLOCKED，不得以跳过、删测或错误基线绕过；
10. 运行 `python tools/sync_logic_task_status.py`、`git diff --check`，确认状态面板 D18 为 53 个任务、LOGIC_PASS=53、PARTIAL=0、BLOCKED=0、PLANNED=0。

成功状态：仅当 L51/L52、重跑后的 L39/L42—L50、全量工具测试、状态面板和 diff 检查全部通过时，才写 `D18-L53=LOGIC_PASS`。若任何条件不满足，必须写真实 LOGIC_PARTIAL 或 LOGIC_BLOCKED，列出精确文件、命令和下一步；不得报告 D18 已完成。

完成时写 `game/tools/results/D18/D18-L53/D18_L53_LOGIC_RESULT.json`、handoff、当天记忆日志。最终交接须说明：D18 只放行给 D19 的是静态逻辑资源接口和稳定键规则；模型导入、Prefab、画面、UI、性能、Player 路线、RuntimeApproved/ReleaseApproved 全部仍是 Runtime Deferred。
```
