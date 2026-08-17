# D18 玩家/NPC 分类词典（D18-L12）

> 冻结日期：2026-08-16
> 任务：`D18-L12`（波次 1，并行；与 L02、L03、L04—L20 并行执行）
> 唯一写入范围：本文件、`game/tools/results/D18/D18-L12/`、`docs/game/design/common/handoffs/D18/D18-L12_LOGIC_HANDOFF.md`、当日记忆日志。
> 本文件是 [D18-L01 基线](../D18_LOGIC_GOVERNANCE_BASELINE.md) 共享字段词典第 6.1 节 `category` 字段在**角色领域**的权威分类来源（与 L11 船舶港口、L13 生态、L14 UI/音频并列）。
> 读取顺序：D18—D24 路线 → LOGIC_TASK_PROTOCOL → D18 章节任务卡 → L01 基线 → 本文；角色领域历史设计见 [D10 外观与服装组合](../D10_APPEARANCE_OUTFIT.md)、[D10 玩家身份表达](../D10_PLAYER_IDENTITY_EXPRESSION.md)、[D8 内容 ID 注册表](../D8_CONTENT_ID_REGISTRY.md)、[角色表现设计](../../character/06_CHARACTER_PRESENTATION.md)、[D14 宗教社群与历史审查](../D14_RELIGION_HISTORY_REVIEW.md)。

## 1. 范围与定位

### 1.1 本文定义什么

1. 角色视觉与外观资源的**逻辑分类词典**（player_visual / npc_visual / hair / outfit / accessory / role_variant / body / injury_overlay / shared_body）；
2. content_id 与 ResourceKey 的约束（前缀、命名、引用目标、禁止项）；
3. 性别、体型、职业与外观的**非裁决映射**边界；
4. 视觉键的写入禁区（不得写身份、关系、特质、金钱、库存、NPC 行为）；
5. 敏感历史表达只复用既有审查标签的规则；
6. 供 D20（L09 外观选择规则、L10 身份表现规则、L32 content_id 集成测试）消费的纯逻辑接口范围——**不需要 Humanoid、Avatar、Prefab 或模型**。

### 1.2 本文不做什么（红线）

- 不修改任何真实 catalog、Schema、验证器、Unity、Prefab、场景、Registry 或资源文件；
- 不授予任何资源状态（本词典只是分类规范；`RuntimeApproved`/`ReleaseApproved` 持有者仍为 0）；
- 不统计或声称模型已存在：本文所有"现有证据"数字来自磁盘真实扫描（见第 9 节验证记录），不补猜；
- 不定义动画、骨架、材质网格的可用性（全部登记为 Runtime 延期，见第 8 节）。

### 1.3 消费者与只读数据源

| 消费者（只读本词典） | 用途 |
|---|---|
| D20-L09 `D20AppearanceSelectionRules` | 校验发型/服装/体型外观选择合法，且不改变身份/关系/数值 |
| D20-L10 `D20IdentityPresentationRules` | 姓名/称谓/代词/敏感历史表达只读表现映射 |
| D20-L32 `D20ContentIdIntegrationTests` | 角色 content_id、资源 key、弃用映射、无候选路径依赖 |
| D18-L15 content_id 关联验证器 | 角色记录 content_ids 存在且不反向依赖候选 |
| D18-L16 资源键验证器 | ResourceKey 唯一、稳定、可迁移 |
| D18-L21—L24 来源目录 / D18-L42 编译器 | 角色记录分类登记与编译 |
| D18-L45 Schema 兼容性审计 | category 枚举一致性核对 |

只读数据源（真实磁盘事实）：`game/Assets/Resources/ContentRegistry/entries/outfit/`（180 条）、`entries/crew/`（61 条）、`entries/asset/`（229 条）、准入目录 `d18_prefab_candidate_intake_catalog.json`（domain=character 37 条）。

## 2. 逻辑分类词典（category 字段权威取值——角色领域）

> **双层分类契约（D18-L51）**：L02 记录中 `category` 永远是 11 值资源大类；下表是**角色领域细分类（`taxonomy_category`）**的权威词典。细分类不得直接写入 `category`；当来源目录实际使用某细分类时，记录必须写 `category=character` + `taxonomy_category=<细分类>`，且细分类必须登记在机器映射 [d18_taxonomy_category_map.json](../../../../../tools/schemas/d18_taxonomy_category_map.json)。当前已实际使用并登记的细分类：`player_visual`、`npc_visual`、`hair`、`outfit`；`accessory`、`role_variant`、`body`、`injury_overlay` 等词典条目为领域词汇，仅在 `gameplay_role`/`tags` 等字段使用（或未来登记映射后作 `taxonomy_category`）。`player_visual_core`、`appearance_*_layer`、`crew_preset` 等既有 gameplay_role/tag 不是 category，只能出现在 `gameplay_role`/`tags` 字段。

| 分类 | 精确定义 | 现有真实证据 | 允许资源类型 | 唯一写者/owner | D20 使用边界 | 缺失/弃用规则 |
|---|---|---|---|---|---|---|
| `player_visual` | 玩家视觉核心（身体/肤色/占位基体与玩家专属表现层） | `asset_ph_crew_male`（占位）；准入目录角色 domain 的 `player_visual_core`、`player_appearance_module.male_clothing_reference`、`player_appearance_module.female_clothing`、`player_appearance_module.facial_hair_support` | 网格/材质/贴图描述器（L26/L27）；不直接引用二进制 | AppearanceSystem 读取；来源目录任务登记 | D20-L09 校验玩家外观组合合法；组合不写身份/关系/数值 | 缺玩家视觉基体 → 保持占位引用（`asset_ph_*`），不得伪造模型存在 |
| `npc_visual` | 普通 NPC 的视觉变体（水手/平民/商人/军官等） | 准入目录角色 domain：`candidate_sailor_visual`、`candidate_port_civilian_visual`、`candidate_port_merchant_visual`、`candidate_military_or_officer_visual`、`candidate_neutral_player_base` | 同上；普通 NPC 使用共享身体+组合（06_CHARACTER_PRESENTATION 第 1 节） | 同上 | D20-L14/L15 NPC 身份/岗位目录可引用视觉变体 ID，但**视觉不裁决岗位行为** | 未登记视觉变体 → NPC 使用默认占位，不补猜数量 |
| `hair` | 发型/发色表现层 | registry 15 条 `functional_hair` + 30 条 `decorative_hair`（如 `outfit_hair_short_brown`、`outfit_braided_deckhair`、`outfit_braid_beads_hair`、`outfit_coiled_hair`、`outfit_headwrap_*_hair`） | 发型网格/贴图描述器 | AppearanceSystem（Hair 槽位）；来源目录登记 | D20-L09 校验发型选择；发型 key 不与性别表达绑定（见第 4 节） | 发型槽位空 → 使用默认/占位；弃用条目走 L19 拒绝或 Revoked |
| `outfit` | 服装表现层（D10 11 槽位中的 Body/Hat/Top/Bottom/Coat/Shoes/Gloves/CareerEquipment/Injury 之外的常规服装层） | registry 180 条 outfit（`outfit_top_linen_deckhand`、`outfit_bottom_canvas_trousers`、`outfit_coat_rain_oilskin`、`outfit_hat_straw_deck`、`outfit_shoes_deck_boots`、`outfit_gloves_ropework`、`outfit_linen_shift`、`outfit_work_skirt` 等） | 服装网格/材质描述器 | AppearanceSystem（各槽位 owner）；价格/耐久由 AppearanceSystem 裁决，金币由 EconomySystem | D20-L09 校验槽位合法性与拥有状态；不裁决价格/耐久/文化资格之外的事实 | 缺资源 → `MissingResourcePlaceholder` 结构状态（D10-A05 第 5 节），不静默跳过 |
| `accessory` | 饰品/徽章/小标识（职业、文化、纪念、宗教语境标签所在层） | registry：`outfit_accessory_compass_badge`、`outfit_decor_*`（约 120 条 plain/festival 对）、`outfit_crew_union_token`、`outfit_fisher_knot_badge`、`outfit_family_locket_plain`、`outfit_mourning_ribbon`、`outfit_plain_prayer_beads_*`、`outfit_small_cross_token_*` 等 | 小件网格/贴图描述器 | AppearanceSystem（Accessory 槽位） | D20-L10 对含宗教/纪念语境标签的 accessory 复用既有审查标签（第 6 节），不新增虚构历史 | 语境敏感 accessory 未过审查 → 不得进入任何表现组合 |
| `role_variant` | 职业/岗位外观组合变体（一组外观槽位的"角色版本"引用，非独立资源） | `crew_d15_a07_preset_001`—`060`（`appearance_combo_id=appearance_combo_d15_a07_*`）；真实 career_id 24 个（deckhand/rigger/fisher/cook/navigator/medic/carpenter/boatbuilder/cooper/sailmaker/lookout/net_mender/signal_keeper/pilot/shore_trader/farmer/poultry_keeper/dock_clerk/interpreter/animal_keeper/coastal_guide/ship_companion/village_companion/watch_companion） | 无独立二进制；只引用 outfit_/asset_ 组合（`references`） | CrewSystem（预设 owner）；AppearanceSystem 读取组合 | D20-L15 岗位目录可引用 role_variant；**职业资格/工资/岗位裁决在 CrewSystem/NPC 系统**（第 4 节） | 职业无对应外观组合 → 允许使用通用组合；不因缺组合裁决职业不可用 |
| `body` | 体型/肤色/年龄感基础表现（非身份） | `outfit_body_sun_bronzed`（`appearance_body_layer`） | 基础网格/肤色贴图描述器 | AppearanceSystem（Body 槽位） | 体型只描述尺度预期；不绑定身份/年龄/健康（第 4 节） | 缺基础体 → 占位；不得补猜骨骼/尺寸数值 |
| `injury_overlay` | 伤病叠加表现层（不可被普通换装清除） | `outfit_injury_bandaged_arm`、`outfit_injury_old_leg`（`appearance_injury_overlay`） | 叠加网格/贴图描述器 | AppearanceSystem（Injury 槽位；`ActiveInjuryIds`/`InjuryHistoryIds` 由系统写） | 伤害数值由医疗系统裁决；表现层只接收 presentation ID（D10-A05 第 3/5 节） | 伤病历史永不自动清除；表现层不得写健康数值 |
| `shared_body` | 普通 NPC 共享身体/服装/动作组合（降低变体数量的组合层） | 06_CHARACTER_PRESENTATION 第 1 节设计证据；无独立资源持有 | 共享网格/材质描述器 + 组合引用 | 来源目录登记；D20-L14 NPC 身份可引用 | 共享组合不得被写成"该 NPC 的身份" | 缺共享组合 → 使用默认占位 |

### 2.1 机器可读词典（与上表一致，L15/L16/L45 可据此校验）

```json
{
  "schema_version": "1.0",
  "document": "docs/game/design/common/assets/D18_TAXONOMY_CHARACTERS.md",
  "scope": "character",
  "categories": [
    {"category": "player_visual", "definition": "玩家视觉核心（身体/肤色/占位基体与玩家专属表现层）", "id_prefixes": ["asset_", "outfit_"], "owner": "AppearanceSystem", "runtime_deferred": ["mesh", "material", "humanoid", "avatar"]},
    {"category": "npc_visual", "definition": "普通 NPC 的视觉变体（水手/平民/商人/军官等）", "id_prefixes": ["asset_"], "owner": "AppearanceSystem", "runtime_deferred": ["mesh", "material"]},
    {"category": "hair", "definition": "发型/发色表现层", "id_prefixes": ["outfit_"], "owner": "AppearanceSystem", "runtime_deferred": ["mesh", "material"]},
    {"category": "outfit", "definition": "服装表现层（D10 常规服装槽位）", "id_prefixes": ["outfit_"], "owner": "AppearanceSystem", "runtime_deferred": ["mesh", "material"]},
    {"category": "accessory", "definition": "饰品/徽章/小标识（职业、文化、纪念、宗教语境标签所在层）", "id_prefixes": ["outfit_"], "owner": "AppearanceSystem", "runtime_deferred": ["mesh", "material"]},
    {"category": "role_variant", "definition": "职业/岗位外观组合变体（引用组合，非独立二进制）", "id_prefixes": ["crew_"], "owner": "CrewSystem", "runtime_deferred": ["none_binary"]},
    {"category": "body", "definition": "体型/肤色/年龄感基础表现（非身份）", "id_prefixes": ["outfit_"], "owner": "AppearanceSystem", "runtime_deferred": ["mesh", "material"]},
    {"category": "injury_overlay", "definition": "伤病叠加表现层（不可被普通换装清除）", "id_prefixes": ["outfit_"], "owner": "AppearanceSystem", "runtime_deferred": ["mesh", "material"]},
    {"category": "shared_body", "definition": "普通 NPC 共享身体/服装/动作组合层", "id_prefixes": ["asset_"], "owner": "AppearanceSystem", "runtime_deferred": ["mesh", "material", "animation"]}
  ],
  "forbidden_categories_as_role": ["player_visual_core", "appearance_hair_layer", "appearance_top_layer", "appearance_bottom_layer", "appearance_coat_layer", "appearance_hat_layer", "appearance_shoes_layer", "appearance_gloves_layer", "appearance_accessory_layer", "appearance_career_equipment_layer", "appearance_body_layer", "appearance_injury_overlay", "appearance_missing_resource_sample", "crew_preset", "functional_hair", "functional_clothing", "functional_accessory", "decorative_hair", "decorative_clothing", "decorative_accessory"],
  "content_id_rules": {
    "allowed_prefixes": ["outfit_", "asset_", "crew_"],
    "forbidden_prefixes": ["body_", "hair_", "gear_", "npc_", "player_", "skin_"],
    "asset_refs_must_be": "asset_",
    "id_pattern": "小写 ASCII snake_case，文件名与 ID 完全一致",
    "reference_targets_must_exist": true
  },
  "resource_key_rules": {
    "logical_format": "d10:appearance:<content_id>",
    "forbidden": ["磁盘路径", "显示名", "Unity 对象引用", "绝对路径", "候选文件路径"]
  },
  "non_adjudicating_mapping": {
    "gender_expression": "外观层不推导性别表达；hair/outfit 分类与 genderExpressionKey 正交",
    "body": "体型只描述尺度预期，不绑定身份/年龄/健康",
    "career": "role_variant 只做外观组合引用；职业资格/工资/岗位由 CrewSystem/NPC 系统裁决"
  },
  "visual_key_write_forbidden": ["identity", "relation", "trait", "money", "inventory", "npc_behavior", "health", "injury_values", "task", "quest"],
  "historical_review_tags": ["historical", "inferred", "fictional"],
  "historical_review_statuses": ["Pending", "Approved", "ApprovedWithWarning"],
  "content_warning_ids": ["content_warning_wildlife_protection", "content_warning_historical_institution", "content_warning_historical_weapon_context", "content_warning_plant_toxicity", "d14_history_context"]
}
```

## 3. content_id 与 ResourceKey 约束

### 3.1 content_id 约束（与 D8 注册表一致，不新增前缀）

1. 角色外观内容 ID 只允许使用既有前缀：`outfit_`（外观/装扮内容定义）、`asset_`（已授权美术/音频/字体资源引用）、`crew_`（船员内容定义/预设）；
2. **禁止新增** `body_`、`hair_`、`gear_`、`npc_`、`player_`、`skin_` 等第二套前缀（D10-A05 第 2 节明确"不新增第二套前缀"）；
3. ID 必须：小写 ASCII snake_case、全局唯一、文件名与 ID 完全一致、改名不改变 ID；
4. `asset_refs` 的目标必须为 `asset_` 前缀；`references`/`dependencies` 目标必须已登记（D8 第 5 节）；
5. 角色记录的 `category` 必须命中第 2 节词典；`gameplay_role`/`tags` 中的 `appearance_*_layer`、`crew_preset`、`functional_*`、`decorative_*` 等**不是** category（见 2.1 `forbidden_categories_as_role`），防止分类混淆；
6. 角色记录不得引用候选路径（`Assets/Art/Candidates/...`）作为 content_id 或 evidence（L17 守卫）。

### 3.2 ResourceKey 约束

1. 逻辑 ResourceKey 格式：`d10:appearance:<content_id>`（如 `d10:appearance:outfit_hair_short_brown`），稳定、唯一、可迁移；
2. **禁止**用磁盘路径、显示名、Unity 对象引用、绝对路径或候选文件路径作 ResourceKey（L16 验证器执行）；
3. 运行时 Unity Resources 路径 `D10/Appearance/<assetId>`（D10-A05 第 5 节）是 **Runtime 阶段实现细节**：本词典只冻结逻辑键规则，不冻结或声称该路径下已有资源；
4. 展示名称、本地化文本和资源路径都不能作为主键（D8 第 2 节）。

## 4. 非裁决映射（性别/体型/职业）

> 映射只做"外观组合 ↔ 稳定 key"的只读关联；**任何映射不得裁决身份、关系、特质、数值或行为**。

| 映射 | 只读关联（允许） | 禁止（不得裁决） | 裁决权属 |
|---|---|---|---|
| 性别表达 | hair/outfit 分类与 `genderExpressionKey` 正交：任何外观槽位不绑定性别表达 key | 不得"男性=某发型/某服装"硬绑定；外观层不得推导性别表达（`gender_expression_androgynous` 等 key 只是稳定表达 key，D10-A04） | `D10PlayerIdentitySystem`（身份表达）；AppearanceSystem（槽位） |
| 体型 | body 分类只描述网格/骨架尺度预期（供 L26 描述器静态验证） | 体型不绑定身份、年龄、健康、特质 | 医疗/身份系统（各自数值）；外观层无裁决权 |
| 职业 | `career_id` → role_variant 的**建议组合**引用（如 `crew_d15_a07_preset_*` 的 `appearance_combo_id`） | 外观组合不得授予职业资格、工资、岗位、技能；缺组合不得判职业不可用 | `CrewSystem`（岗位/工资/关系）、D20-NPC 系统（行为）、`TradeEconomySystem`（金钱） |
| 文化 | 文化标签（`culture_island_saint_marys` 等）只作表现语境标签 | 标签不得裁决声望、法律、社群结果 | 各势力/法律/社群系统 |
| 伤病 | injury_overlay 由系统传入 presentation ID（`ActiveInjuryIds`） | 表现层不得写健康数值、不得伪造伤病事实 | 医疗系统（数值）、AppearanceSystem（表现历史） |

## 5. 视觉键写入禁区（红线）

视觉键（visual key，即外观组合/分类/槽位标识）**只能**表达"用哪个外观组合渲染"，**禁止写入或暗示**：

| 禁止写入 | 说明 | 真实 owner |
|---|---|---|
| 身份（identity） | 姓名、性别表达、称谓、证件、身份历史 | `D10PlayerIdentitySystem` |
| 关系（relation） | 好感、信任、忠诚、恋爱状态、关系边 | D10 `RelationSystem` / `CrewSystem` |
| 特质（trait） | 技能、性格、特质值 | `TraitSystem` |
| 金钱（money） | 价格、余额、债务 | `TradeEconomySystem` |
| 库存（inventory） | 拥有物品、数量、装备物品 | `InventorySystem`/`ItemCatalog` |
| NPC 行为（npc behavior） | 岗位日程、行为状态、可用性、拒绝原因 | D20 NPC 系统 |
| 健康/伤病数值 | 健康、疲劳、伤势数值 | 医疗系统 |
| 任务/因果 | 任务状态、事件因果 | `QuestSystem`/事件系统 |

实现约束：D20-L09 外观选择规则只读外观组合并校验合法性；校验结果不得持久化为身份/关系/数值字段；D20-L19（NPC 行为权威守卫）与 L44（游戏引用审计）持续扫描此类越界。历史既有事实：`crew_d15_a07_preset_001` 的描述已声明"不写运行时关系、工资、健康、奖励或战斗结果"——本词典将该声明冻结为全角色分类强制规则。

## 6. 敏感历史表达

### 6.1 只复用既有审查标签（禁止虚构事实）

角色外观涉及宗教、社群、历史制度的表达（如 `outfit_plain_prayer_beads_*`、`outfit_small_cross_token_*`、`outfit_mourning_pin_*`、`outfit_decor_ceremony_sash_*`、`outfit_harbor_uniform_*`）必须使用 D8/D14 既有的审查字段，**不新增标签体系、不虚构历史事实**：

| 字段 | 合法取值（真实枚举，2026-08-16 统计） | 说明 |
|---|---|---|
| `historical_tag` | `historical`（125 条）/ `inferred`（2,442 条）/ `fictional`（245 条） | 只取这三个值 |
| `historical_review` | `Pending`（101 条）/ `Approved`（2,620 条）/ `ApprovedWithWarning`（91 条） | `Pending`/`Rejected` 内容拒绝进入逻辑（D14-A06 第 3 节） |
| `content_warning_ids` | 既有值：`content_warning_wildlife_protection`、`content_warning_historical_institution`、`content_warning_historical_weapon_context`、`content_warning_plant_toxicity`、`d14_history_context` | 逐条引用，不发明新警告 ID |
| `skip_allowed` | true（275 条）/ false（2,537 条） | 跳过只写"已跳过"审查记录，不产生参与结果 |
| `review_evidence_refs` | 仓库内相对路径 | 必须存在（D8 检查器执行） |

### 6.2 表现等级与语境（D14-A06 冻结语义）

- `ApprovedWithWarning` 允许 `Full`/`Contextual`/`LowIntensity`/`Omitted` 四档表现等级；未确认警告时拒绝执行（`warning_required`）；
- 低强度保留语境和事实但降低表现；跳过只记录"已跳过"；
- Agent 只能 `ProposeExpression` 生成表达草案，不能写审查事实（`agent_cannot_write`）；
- 本词典不新增任何宗教/历史内容条目；新表达必须先有来源/审查事实（D14-A06 第 2 节"任何扩展必须先加入历史来源/审查事实"）。

## 7. 与 D20 的接口（只写接口，不实现）

> D20 可完全依据本词典建立纯逻辑接口；**不需要 Humanoid、Avatar、Prefab 或模型**。本节只声明接口边界，D20-L09/L10/L32 独占实现。

| D20 任务 | 本词典提供的输入 | 接口边界 |
|---|---|---|
| D20-L09 外观选择规则 | 第 2 节 9 分类 + D10 11 槽位映射 + 第 5 节禁区 | 校验发型/服装/体型选择合法（槽位、拥有、文化/职业标签）；结果不写身份/关系/数值 |
| D20-L10 身份表现规则 | 第 4 节非裁决映射 + 第 6 节审查标签 | 姓名/称谓/代词/性别表达 key 只读映射；敏感表达必须携带既有 `historical_tag`/`historical_review`/`content_warning_ids` |
| D20-L32 content_id 集成测试 | 第 3 节 content_id/ResourceKey 约束 + 2.1 JSON 词典 | 验证角色记录命中分类词典、前缀合法、引用存在、无候选路径依赖、弃用映射完整 |

## 8. Runtime 延期项（模型/动画，全部登记不执行）

| 延期项 | 说明 |
|---|---|
| 角色网格/材质/贴图实际导入与验证 | L26/L27 只做描述器静态验证；真实导入属 Runtime 阶段 |
| Humanoid Avatar / 骨骼 / Animator | 本文不定义任何骨架/动画资源；D20-L10 只读表现映射不依赖 |
| Prefab / 场景接入 / 碰撞 / LOD | 全部延期（基线第 9 节） |
| 动画分类（待机/走路/奔跑/工作/战斗等，06_CHARACTER_PRESENTATION 第 2 节） | 仅登记设计类别，不建立资源分类；动作类别清单只作为未来 Runtime 输入 |
| `D10/Appearance/` 运行时资源路径可用性 | 逻辑键规则已冻结；该路径下资源是否存在不验证、不声称 |
| 服装/发型/体型在真实视觉中的表现验证 | 截图/视觉 QA 属 Runtime 阶段 |

## 9. 本任务实际执行的验证（与结果 JSON 一致）

1. **分类与约束机器检查**（Python 一次性脚本，临时目录不落仓库）：解析 2.1 JSON 词典与第 2 节表格，断言 9 分类一一对应、禁止分类列表无重复、前缀规则与 D8 13 前缀一致、ResourceKey 格式约束存在、视觉键禁区字段与第 5 节表一致、审查标签取值与第 6 节一致；
2. **真实数据交叉核对**（Python 只读扫描，不修改任何文件）：registry 分类/role/tag/审查字段计数与本文引用一致（outfit 180、crew 61、asset 229、historical_tag 三值 125/2,442/245、historical_review 三值 101/2,620/91、content_warning_ids 五个既有值、career_id 24 个、culture 13 个、准入目录 character domain 37 条）；
3. **相对链接存在性检查**（Python，同 L03/L01 方法）；
4. `git diff --check`；
5. `python tools/sync_logic_task_status.py`。

## 10. 链接索引

- 路线与协议：[D18—D24 纯逻辑完成路线](../../../D18_D24_VISUAL_PLAYABLE_ROADMAP.md)、[单任务逻辑开发协议](../../../LOGIC_TASK_PROTOCOL.md)
- 基线与本任务：[D18-L01 逻辑治理基线](../D18_LOGIC_GOVERNANCE_BASELINE.md)、[D18 章节任务卡](../../../AGENT_DEVELOPMENT_CHAPTER_10_D18.md)、[D18-L03 状态机规范](D18_ASSET_STATE_MACHINE.md)、[D18-L01 交接报告](../handoffs/D18/D18-L01_LOGIC_HANDOFF.md)
- 角色历史设计（只读）：[D10 外观与服装组合](../D10_APPEARANCE_OUTFIT.md)、[D10 玩家身份表达](../D10_PLAYER_IDENTITY_EXPRESSION.md)、[D8 内容 ID 注册表](../D8_CONTENT_ID_REGISTRY.md)、[D14 宗教社群与历史审查](../D14_RELIGION_HISTORY_REVIEW.md)、[角色表现设计](../../character/06_CHARACTER_PRESENTATION.md)
- D20 章节：[D20 玩家/NPC/动物与动作逻辑](../../../AGENT_DEVELOPMENT_CHAPTER_12_D20.md)
- 状态总览：[逻辑任务状态总览](../../../LOGIC_TASK_STATUS.md)
- 只读数据（本任务不修改）：`game/Assets/Resources/ContentRegistry/entries/`、`docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json`
