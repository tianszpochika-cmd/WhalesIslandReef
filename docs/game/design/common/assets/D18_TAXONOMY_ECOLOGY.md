# D18 生态分类词典（D18-L13）

> 任务：`D18-L13`（波次 1，并行）
> 冻结日期：2026-08-16
> 唯一权威基线：[D18_LOGIC_GOVERNANCE_BASELINE.md](../D18_LOGIC_GOVERNANCE_BASELINE.md)（D18-L01）；本词典是该基线第 6.1 节 `category` 字段在生态域的实例化。
> 下游消费者：D20-L20/L21/L24/L25/L32、D21-L13、D24-L09（见第 6 节接口）。
> 平行词典：L11（船/港口）、L12（角色）、L14（UI/音频）与本词典同波次冻结，互不覆盖。

## 0. 边界声明（三条"不"）

本词典是**纯逻辑分类词典**，只定义分类键、content_id 规则、生态所有者和资源映射边界。它：

1. **不生成物种数量**：任何分类、子类或 count 数字都不是种群数量；运行时数量、生物量、刷新与恢复只由 `EcologySystem` 裁决。
2. **不决定种群**：分类不创建动物/鱼群/植物实例，不写 `asset_state`、Registry 或存档；`LocalDevelopmentReady=0`、`ReleaseApproved=0` 保持（基线第 1.5 节）。
3. **不导入模型**：所有候选资源映射（第 2 节）只是 Candidate 登记；本词典不产生 ImportVerified、RuntimeApproved 或任何运行时接入声明。

本词典不新增 Registry 条目、不改 ContentRegistry、不写任何真实 catalog（基线第 5.3 节全章禁区）。机器可校验摘要见第 8 节 JSON 块；L45 将自动核对一致性。

## 1. 分类总表（8 个逻辑分类键）

> **双层分类契约（D18-L51）**：本表的"分类键"是**生态领域细分类（`taxonomy_category`）**，不是 L02 记录的 `category`。L02 记录中 `category` 永远是 11 值资源大类：陆生/鸟类/水生动物/宠物 → `category=animal`，植被 → `category=environment`（地形/水域为场景数据，不产生资源记录）。细分类不得直接写入 `category`；实际使用某细分类时记录必须写 `category=<大类>` + `taxonomy_category=<细分类>`，且细分类必须登记在机器映射 [d18_taxonomy_category_map.json](../../../../../tools/schemas/d18_taxonomy_category_map.json)。当前已实际使用并登记的细分类：`terrestrial_animal`、`bird`、`aquatic_animal`、`pet`；`rare_species`、`vegetation`、`terrain`、`water_body` 为领域词汇（`d15_category`/标记/场景数据），不登记为资源记录细分类。`fish` 是 L02 独立大类，供真正鱼类资源记录使用，不受本映射影响。

| 逻辑分类 | 领域细分类（taxonomy_category） | content_id 前缀 | 内容目录 `owner_system` | 运行时裁决唯一写者 | 真实计数（2026-08-16） | 代表 ID |
|---|---|---|---|---|---|---|
| 陆生动物 | `terrestrial_animal` | `animal_*` | `EcologySystem` | `EcologySystem` | 85（D15-A03 terrestrial 40 + reptiles 25 + insects 20） | `animal_agouti_forest`、`animal_domestic_chicken` |
| 鸟类 | `bird` | `animal_*`（子类 `coastal_birds`/`birds`） | `EcologySystem` | `EcologySystem` | D15-A03 coastal_birds 30 + birds 25 = 55 | `animal_anhinga`、`animal_caribbean_parrot` |
| 水生动物 | `aquatic_animal` | `fish_*` 与 `animal_*`（子类 `sea_turtles`/`freshwater_estuary`） | `EcologySystem` | `EcologySystem` | `fish_` 134 + `animal_*` 40（sea_turtles 15 + freshwater_estuary 25，两栖条目按生境归属本类） | `fish_mackerel_school`、`animal_green_turtle`、`fish_bottlenose_dolphin` |
| 珍稀物种 | `rare_species` | **横切标记**：保持原前缀（`animal_*`/`fish_*`/`plant_*`），不创建 `rare_` 前缀 | `EcologySystem`（野外状态）/ `CollectionSystem`（图鉴投影） | `EcologySystem` 写野外；`CollectionSystem` 只写图鉴记录 | 无独立计数；以 `legal_profile ∈ {protected, regulated}` 或 D12 图鉴条目标记（当前代表 `animal_green_turtle`） | `animal_green_turtle`、`plant_red_mangrove`（保护区样例） |
| 宠物 | `pet` | `pet_*` | `AnimalCareSystem` | `AnimalCareSystem` | 6（`pet_cabin_cat`/`pet_cat_galley`/`pet_cat_mooring`/`pet_harbor_dog`/`pet_dog_watch`/`pet_dog_village`） | `pet_cabin_cat`、`pet_harbor_dog` |
| 植被 | `vegetation` | `plant_*` | `EcologySystem` | `EcologySystem` | 1,211 `plant_` | `plant_red_mangrove`、`plant_orchard_guava_v1` |
| 地形 | `terrain` | 无内容前缀（`asset_*`/`island_*` 或场景资源） | 无生态所有者（场景/岛屿数据） | 无生态裁决写者 | 资源映射 224 条（见 2.7） | `island_saint_marys`（世界数据） |
| 水域 | `water_body` | 无内容前缀（`island_*`/场景资源） | 无生态所有者（场景/岛屿数据） | 无生态裁决写者 | 资源映射 13 条（见 2.8） | `nearshore_saint_marys`（捕捞点） |

判定优先级（一条记录只能属于一个分类键）：`pet` > `rare_species`（标记叠加，记原类别+横切标记）> `terrestrial_animal`/`bird`/`aquatic_animal`（按子类字段）> `vegetation` > `terrain`/`water_body`（资源映射无内容 ID）。

## 2. 每类详细定义与资源映射边界

### 2.1 陆生动物 `terrestrial_animal`

- 定义：陆地及陆缘生活的哺乳、爬行、两栖、昆虫等非水生、非鸟类动物；家禽/牲畜（`animal_domestic_*`，D15-A07）也属此类（不是宠物）。
- content_id：`^animal_[a-z0-9_]+$`；D15-A03 子类 `terrestrial`/`reptiles`/`insects`；`freshwater_estuary` 全量按生境归属 `aquatic_animal`（2.3），不按前缀重分类。
- 资源映射（intake domain=`animal`）：`candidate_land_animal`（9 条）→ 本类；`candidate_land_animal_variant_not_counted_separately`（1 条）→ 本类**变体引用**，不产生新 content_id（见 5.2 弃用/变体规则）。
- 所有者：目录 `EcologySystem`；行为 L0—L4 与数量、刷新、健康、生态压力、法律采集结果全部由 `EcologySystem` 裁决（D15-A03 第 5 节；D11-A01 状态机）。
- 缺失规则：缺 `d15_category` 或行为等级、无 `EcologySystem` owner、候选文件缺许可/哈希/证据 → Pending，不得猜补（见 5.1）。

### 2.2 鸟类 `bird`

- 定义：以飞行/岸栖为主的鸟类；D15-A03 子类 `coastal_birds`（30）与 `birds`（25）。
- content_id：`^animal_[a-z0-9_]+$`，与陆生动物共用前缀；**子类字段区分**（`d15_category`），不靠前缀。
- 资源映射：intake domain=`animal` 的 `candidate_bird_nearshore_eagle`（1 条）→ 本类。
- 所有者：`EcologySystem`（同 2.1，行为/数量/压力）。
- 缺失/弃用：同 2.1；鸟类外形候选的模型/材质映射必须先过 L04/L06/L07/L08，未过不得写为已接入。

### 2.3 水生动物 `aquatic_animal`

- 定义：海洋/河口/淡水的鱼类、鲨鳐、鲸豚、海龟、甲壳软体、珊瑚等；D15-A03 子类 `sea_turtles`、`crustaceans_softbody`、`nearshore_fish`、`whales_dolphins`、`sharks_rays` 与 `freshwater_estuary` 水生条目。
- content_id：`^fish_[a-z0-9_]+$` 为主；`animal_*`（如 `animal_green_turtle`、`animal_manatee_estuary`）允许但须带水生子类字段。
- 资源映射：intake domain=`animal` 的 `candidate_aquatic_mammal`（2）、`candidate_aquatic_species`（2）、`candidate_fish_species`（3）→ 本类。
- 所有者：`EcologySystem`（鱼群生物量、位置、深度、迁徙、捕捞压力唯一写者；D11-A01 第 3.2 节）。
- 缺失/弃用：`fish_*` 不得登记为陆生/鸟类；缺 `fishSchoolId` 语义的物种记录不得声明为鱼群实例（D11-A01 第 2 节口径）。

### 2.4 珍稀物种 `rare_species`

- 定义：横切分类——受保护/受监管/需要许可的物种与保护区资源。**不是独立前缀**：content_id 保持 `animal_*`/`fish_*`/`plant_*` 原 ID。
- 标记字段：`legal_profile ∈ {protected, regulated, licensed}`（D15-A03）、`tags` 含 `protected_candidate`、或 D12 图鉴条目（`D12CollectionCatalog`）；代表 `animal_green_turtle`（protected，D11-A05）。
- 所有者：野外状态 `EcologySystem`（福利、运输死亡、保护区后果）；`CollectionSystem` 只写图鉴记录/证据/合法转移（D11-A05 第 3 节）；`D12CollectionAtlasSystem` 只写图鉴投影（D12-A07）。
- 资源映射：继承基类映射；保护区/收藏家/运输定义属逻辑数据而非候选模型，不消费候选文件。
- 缺失/弃用：缺失法律档位不得自动推断为普通物种；受保护标记的撤销须人工复核并 append-only 留痕（基线 2.5 节）。

### 2.5 宠物 `pet`

- 定义：非战斗伴侣动物实例；唯一写者 `AnimalCareSystem`（`D11PetSystem`）。
- content_id：`^pet_[a-z0-9_]+$`，**仅此前缀**；家禽/牲畜（`animal_domestic_*`）不是宠物，禁止用 `pet_`。
- 资源映射：intake domain=`animal` 的 `candidate_dog_pet`（2）、`isolated_cat_pet_model_candidate`（1）→ 本类；映射只登记候选模型，不创建宠物实例。
- 所有者：`AnimalCareSystem`（领养、健康、饥饿、清洁、恐惧、依恋、位置、检疫、走失；D11-A06 第 1 节）；内容目录 `owner_system=AnimalCareSystem`。
- 缺失/弃用：宠物外观候选缺来源/授权（D15-A07 `auth_d15_a07_internal_authored_20260811`）不得接入；宠物不是战斗单位，禁止把战斗字段写入宠物记录。

### 2.6 植被 `vegetation`

- 定义：可生长/可采集/可识别的植物资源（果实、蔬果、药材、纤维、木材、树脂、染料、毒物、保护区植物）。
- content_id：`^plant_[a-z0-9_]+$`（1,211 条 Registry 记录）；`plant_red_mangrove`、`plant_orchard_guava_v1` 等代表（D11-A07 第 2 节）。
- 资源映射：intake domain=`environment` 的 `candidate_vegetation_model`（92 条）→ 本类；模型/材质候选只登记，不决定成熟度、产量或数量。
- 所有者：`EcologySystem`（数量、采集压力、恢复、成熟度结果唯一写者；D11-A07 第 1 节）；采集系统只提交请求。
- 缺失/弃用：缺成熟度/工具/保护区规则的植物不得接入采集链；被保护区拒绝的条目保持生态拒绝路径，不删除历史。

### 2.7 地形 `terrain`

- 定义：地表、岩石、悬崖、土路、沙滩、木制地面与场景环境模块——**非生态实体，无内容前缀**。
- content_id：无；如需要内容引用，使用既有 `island_*`（50 条）或 `asset_*`（229 条）登记，禁止新造地形前缀。
- 资源映射：intake domain=`environment` 的 `candidate_rock_model`（32）、`candidate_stone_model`（40）、`candidate_cliff_model`（56）、`candidate_dirt_path_model`（20）、`candidate_beach_model`（1）、`candidate_wood_model`（20）、`candidate_environment_module`（55），合计 224 条 → 本类。
- 所有者：无生态所有者；场景/岛屿数据持有者（世界数据），生态系统不裁决地形。
- 缺失/弃用：地形候选不映射任何物种 content_id；环境模块无许可/哈希/路径证据不得 IntakeEligible（L04—L10 链）。

### 2.8 水域 `water_body`

- 定义：水面、水缘、浅水、捕捞点等水域表现——**非生态实体，无内容前缀**。
- content_id：无；捕捞点/生境引用使用既有 D11/D12 冻结 ID（如 `nearshore_saint_marys`、`mangrove`）。
- 资源映射：intake domain=`environment` 的 `candidate_water_edge_model`（13 条）→ 本类。
- 所有者：无生态所有者；水域表现属场景数据；`EcologySystem` 只写鱼群位置与生态条件，不写水面表现。
- 缺失/弃用：水域候选不得被写成海况/天气表现证据；天气输入只由 `EnvironmentSystem` 提供（D11-A01 第 3 节）。

## 3. content_id 机器规则

- 全小写 ASCII snake_case：`^[a-z][a-z0-9]*(_[a-z0-9]+)*$`；文件名与 `id` 完全一致（D15-A03 第 2 节）。
- 前缀→分类键映射（唯一判定表）：

| 前缀 | 允许分类键 |
|---|---|
| `animal_*` | `terrestrial_animal` / `bird` / `aquatic_animal` / `rare_species`（按 `d15_category` 与 legal 标记） |
| `fish_*` | `aquatic_animal` / `rare_species` |
| `plant_*` | `vegetation` / `rare_species` |
| `pet_*` | `pet`（唯一） |
| `island_*` / `asset_*` | `terrain` / `water_body`（场景引用） |

- 分类键必须在第 8 节枚举内；未命中 → 拒绝登记并写 Pending（L45 一致性审计兜底）。
- 候选资源角色（intake `role`）→ 分类键映射见第 2 节各条；未映射角色归 `unknown` 域，由 L33/L34 登记缺口，**不猜测归属**。

## 4. 生态所有者与裁决边界（冻结）

| 内容 | 唯一写者 | 本词典分类 |
|---|---|---|
| 植物/动物/鱼群数量、生物量、刷新、压力、恢复、法律采集结果 | `EcologySystem`（`D4EcologySystem`） | 2.1—2.4、2.6 |
| 宠物实例状态 | `AnimalCareSystem` | 2.5 |
| 珍稀物种图鉴/合法转移记录 | `CollectionSystem`、`D12CollectionAtlasSystem`（只写投影） | 2.4 |
| 天气/海况输入 | `EnvironmentSystem`（只读快照给生态） | — |
| 地形/水域表现 | 场景/岛屿数据（无生态裁决） | 2.7、2.8 |

- 内容目录只提供定义、行为等级、刷新配置引用、法律规则引用、表现预算；**目录/UI/Agent 不裁决数量、奖励、价格、库存、保护区结果**（D15-A03 第 5 节；D11-A07 第 1 节）。
- UI/Agent 只读快照与事件；`Systems/` 不得出现 LLM/网络裁决入口（铁律 2）。

## 5. 缺失与弃用规则（每类适用）

### 5.1 缺失规则（缺证据 → Pending/拒绝，不猜补）

| 缺失情形 | 处置 | 涉及分类 |
|---|---|---|
| 缺 `content_id` 或文件名≠id | 拒绝登记 | 全部 |
| 缺 `owner_system` 或 owner 不在第 1 节允许表 | Pending | 全部 |
| 分类键不在第 8 节枚举 | 拒绝，写 unknown 域缺口 | 全部 |
| 缺许可证/哈希/证据文件/路径越界 | 阻断 IntakeEligible（L04/L06/L07/L08 链） | 全部（含资源映射） |
| 缺 `d15_category`/行为等级/`habitat_profile` | Pending，不得按前缀猜分类 | 2.1—2.4 |
| 缺 `legal_profile` 却带保护标记 | Pending 待人工 | 2.4 |
| 宠物缺来源/授权记录 | 不得接入（D15-A07 授权口径） | 2.5 |
| 植物缺成熟度/工具/保护区规则 | 不得接入采集链 | 2.6 |
| 地形/水域候选缺许可/哈希 | 不得 IntakeEligible | 2.7、2.8 |

### 5.2 弃用与变体规则（append-only，可审计）

| 规则 | 说明 |
|---|---|
| 弃用标记 | 弃用须写记录：原 ID、原因、日期、替代 `content_id`（如有）、操作者；工具不得静默覆盖或删除历史（基线 2.5 节） |
| 弃用后引用 | 新引用禁止；存量引用只读保留；D20-L32 校验弃用映射与无候选路径依赖 |
| 变体不单独计数 | `candidate_land_animal_variant_not_counted_separately` 等变体角色只生成对主物种的引用，不产生新 content_id（D15-A03 口径） |
| 重命名 | 不因改名造新包/新 ID；原 ID 保留 deprecated + 重定向 |
| 被保护区拒绝的条目 | 保留生态拒绝路径与历史证据，不删除、不降级为普通物种 |

## 6. 与下游任务接口（D20/D21/D24 验证口径）

| 下游任务 | 消费本词典的什么 |
|---|---|
| D20-L21 `D20AnimalSpeciesCatalog` | 用第 1 节分类键与第 3 节前缀表校验陆生/鸟类/水生/珍稀/宠物的 content_id/行为能力目录 |
| D20-L32 `D20ContentIdIntegrationTests` | 用第 5.2 节弃用映射规则验证弃用与无候选路径依赖；分类键枚举来自第 8 节 JSON |
| D20-L24/L25 | 种群/生境输入只读适配 `EcologySystem` 读模型，禁止裁决生态数值；水生适配只读 `fish_*` 可见性 |
| D21-L13 `D21WeatherImpactRules` | 天气影响规则只读；不改生态数量/分类 |
| D24-L09 `d24_inventory_invariant_audit` | 宠物/采集物前缀（`pet_*`/`plant_*`/`fish_*`/`item_*`）与归属不变式 |
| L45 Schema 一致性审计 | 第 8 节 JSON 与 L02 Schema `category` 枚举、L42 编译结果自动核对 |

## 7. 明确未宣称

- 本词典没有生成任何物种数量、没有决定任何种群、没有导入任何模型。
- 第 2 节所有资源映射都是 Candidate 登记；`LocalDevelopmentReady=0`、`ReleaseApproved=0`、L 系列 `RuntimeApproved` 无持有者（基线 1.5/2.6 节），无任何候选资源被写为已运行时接入。
- 本词典不修改 ContentRegistry、`asset_state`、存档 Schema、`contracts/`、`server/`、`web/`；不引用旧 D18-A/S/R 结论作为通过依据（基线第 4 节）。

## 8. 机器可校验摘要（L45 直接解析本 JSON 块）

```json
{
  "document": "D18_TAXONOMY_ECOLOGY",
  "task_id": "D18-L13",
  "frozen": "2026-08-16",
  "category_keys": [
    "terrestrial_animal", "bird", "aquatic_animal", "rare_species",
    "pet", "vegetation", "terrain", "water_body"
  ],
  "category_priority": ["pet", "rare_species", "bird", "aquatic_animal",
                        "terrestrial_animal", "vegetation", "terrain", "water_body"],
  "prefix_category_map": {
    "animal_*": ["terrestrial_animal", "bird", "aquatic_animal", "rare_species"],
    "fish_*": ["aquatic_animal", "rare_species"],
    "plant_*": ["vegetation", "rare_species"],
    "pet_*": ["pet"],
    "island_*": ["terrain", "water_body"],
    "asset_*": ["terrain", "water_body"]
  },
  "owner_by_category": {
    "terrestrial_animal": "EcologySystem", "bird": "EcologySystem",
    "aquatic_animal": "EcologySystem", "rare_species": ["EcologySystem", "CollectionSystem"],
    "pet": "AnimalCareSystem", "vegetation": "EcologySystem",
    "terrain": "none", "water_body": "none"
  },
  "intake_role_category_map": {
    "candidate_land_animal": "terrestrial_animal",
    "candidate_land_animal_variant_not_counted_separately": "terrestrial_animal:variant",
    "candidate_bird_nearshore_eagle": "bird",
    "candidate_aquatic_mammal": "aquatic_animal",
    "candidate_aquatic_species": "aquatic_animal",
    "candidate_fish_species": "aquatic_animal",
    "candidate_dog_pet": "pet",
    "isolated_cat_pet_model_candidate": "pet",
    "candidate_vegetation_model": "vegetation",
    "candidate_water_edge_model": "water_body",
    "candidate_rock_model": "terrain", "candidate_stone_model": "terrain",
    "candidate_cliff_model": "terrain", "candidate_dirt_path_model": "terrain",
    "candidate_beach_model": "terrain", "candidate_wood_model": "terrain",
    "candidate_environment_module": "terrain"
  },
  "missing_rules": ["id_absent_or_mismatch", "owner_not_in_allowlist",
                    "category_not_in_enum", "license_or_hash_or_evidence_gap",
                    "subclass_absent", "legal_profile_absent_with_protection_marker",
                    "pet_source_authorization_absent", "plant_harvest_rules_absent"],
  "deprecation_rules": ["append_only_record", "no_new_references_after_deprecation",
                        "variant_not_counted_separately", "rename_preserves_old_id",
                        "protected_rejection_kept_as_history"]
}
```

## 9. 数据事实附录（2026-08-16 磁盘盘点，不补猜）

- Content Registry：2,812 条；前缀分布 `plant` 1,211 / `item` 478 / `asset` 229 / `outfit` 180 / `animal` 171 / `fish` 134 / `quest` 126 / `building` 80 / `ship` 80 / `crew` 61 / `island` 50 / `pet` 6 / `reward` 6（registry.json 快照）。
- D15-A03 子类→本词典：`terrestrial/reptiles/insects`→陆生动物（85）；`coastal_birds/birds`→鸟类（55）；`sea_turtles/crustaceans_softbody/nearshore_fish/whales_dolphins/sharks_rays`→水生动物；`freshwater_estuary`→水生动物（25，按生境，两栖条目同属）。
- D15-A07 宠物/家禽：`pet_*` 6 条（AnimalCareSystem）；`animal_domestic_*` 家禽/牲畜 6 条（EcologySystem，非宠物）。
- 准入目录（D18-S02-R02 快照）生态相关：domain=`animal` 21 条、`environment` 329 条（含 vegetation 92、water_edge 13、rock/stone/cliff/dirt/beach/wood/environment_module 224）；均为 Candidate 登记。
