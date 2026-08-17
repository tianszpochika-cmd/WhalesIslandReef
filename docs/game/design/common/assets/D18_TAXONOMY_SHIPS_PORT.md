# D18 船舶/港口/建筑分类词典（D18-L11）

> 冻结日期：2026-08-16
> 任务：`D18-L11`（波次 1，并行）
> 本词典冻结 ship / hull_component / port_service / dock / berth / building / prop 的逻辑类别、子类、稳定 content_id 命名规则、弃用映射与不可混用关系。
> 依据：[D18 纯逻辑资源治理基线](../D18_LOGIC_GOVERNANCE_BASELINE.md)（状态机、字段词典、SUPERSEDED 隔离）、[D15 内容生产冻结](../D15_CONTENT_PRODUCTION_FREEZE.md)（前缀所有权）、[船舶设计目录](../../ship/README.md)、[建筑港口设计目录](../../buildings/README.md)、[D19 港口业务章节](../../../AGENT_DEVELOPMENT_CHAPTER_11_D19.md)、[D21 船天气海洋章节](../../../AGENT_DEVELOPMENT_CHAPTER_13_D21.md)。
> 历史证据引用（不作为通过依据）：`d18_prefab_candidate_intake_catalog.json`（A 系列准入目录，SUPERSEDED，仅作真实角色/文件事实来源）、`d18_placeholder_replacement_map.json`（A 系列占位映射，SUPERSEDED）。

## 1. 定位与边界（先读）

1. **分类只服务数据/资源映射**：本词典只决定「逻辑类别 → content_id → 资源记录」的映射关系；**不改变**船舶、港口、经济或场景的任何状态，不裁决航速、价格、伤害、工期、泊位占用、服务排队或建筑状态。
2. **不统计、不声称模型已存在**：本词典不统计候选文件数量，不把任何候选文件写成「模型已存在」「已导入」「可驾驶」「运行时可用」或 `RuntimeApproved`/`ReleaseApproved`。候选文件存在性只由 L31/L33/L34 用真实磁盘审计核实；本词典不参与。
3. **两套命名空间不可混淆**：
   - **内容定义 id**（游戏数据层，如 `building_market`、`ship_sloop`）：唯一事实源在 Content Registry / ship_type_defs，写者见第 4 节所有者列。
   - **资源逻辑记录 id**（D18-L02 Schema 的 `canonical_asset_id`，`asset_*`）：只描述外部资源候选的证据与映射，**不得被游戏数据反向依赖**（L15 校验）。
4. 本词典属于 L02 Schema `category` 枚举值（ship / building / prop）下的子层：**每条子类必须能映射回一个 L02 `category` 值**（第 6 节映射表），供 L21—L24 来源目录与 L42 编译器使用。

## 2. content_id / ResourceKey 命名规则（冻结）

稳定规则（口径来自 D15 内容生产冻结第 3 节前缀表与真实 Registry 记录，本词典不新增顶层前缀，只冻结本领域的子前缀）：

| 规则 | 内容 |
|---|---|
| 字符集 | 全小写 `[a-z0-9_]+`，不允许连字符、点、空格、路径分隔符或大写（与既有 1,865 条 intake 条目和 Registry `building_*`/`item_*` 记录一致） |
| 内容定义前缀 | 船体/船舶内容定义用 `ship_`；进入 Inventory 的改装物品继续用 `item_`（D15 既有冻结，不改）；建筑/功能建筑用 `building_`；资源型模型/音频/UI 用 `asset_`（D15 既有规则） |
| 本词典冻结的子前缀 | `ship_hull_*`、`ship_module_*`、`ship_function_*`、`ship_rigging_*`、`ship_weapon_*`、`boat_*`、`port_service_*`、`dock_*`、`berth_*`、`building_*`（沿用 Registry 80 条真实 id）、`prop_*` |
| 唯一性 | 一个 content_id 只能归属一个 L11 类别；重名检测由 L32（name collision audit）负责 |
| 稳定性 | content_id 一旦登记不得因显示名/文件名改名而改变（迁移用弃用映射，第 7 节）；禁用磁盘路径、显示名作 id（L16 校验） |
| 候选级 id | A 系列候选 content_id（如 `d18a05_*`、`candidate_*`）属历史证据，可被 L 记录引用但**不新增**（L21—L24 写 `asset_*` 记录时可引用它们为 content_ids 证据，不复制为新内容定义） |

## 3. 类别总览

| L11 类别 | 定义 | 子类（示例） | 命名例子（真实或规则示例） | L02 category 映射 |
|---|---|---|---|---|
| `ship` | 船型/船体/船上部件与功能定义 | 船型定义、可驾驶/远景船体、船型变体、损坏变体、远景剪影、桅杆索具、船上武器、交通艇/小艇 | `ship_sloop`、`ship_hull_small`、`ship_rigging_mast`、`ship_weapon_cannon`、`boat_tender_row` | `ship` |
| `hull_component` | 船体可拆模块、舱室、功能点与改装部件（改装物品入 Inventory 用 `item_`） | 船体模块、舱室、功能点、改装槽物品 | `ship_module_bowsprit`、`ship_function_galley`、`item_module_cannon_upgrade` | `ship` |
| `port_service` | 港口服务功能（装卸、修理、补给、上下船、排队、检疫、检查） | 服务类型定义 | `port_service_repair`、`port_service_provision` | `building` |
| `dock` | 码头/栈桥/平台/舷梯等靠泊设施 | 货运码头、栈桥、平台模块、小艇码头 | `dock_cargo_wharf`、`dock_pier_module` | `building` |
| `berth` | 泊位（大船/小船泊位、锚泊点） | 泊位规格、位置 | `berth_large_01`、`berth_small` | `building` |
| `building` | 功能建筑与建筑构件（港口/船坞/贸易/生活/社会/防御/生产/探索/管理九类） | 建筑定义、建筑构件（墙/门洞/窗洞/地板/屋顶/排水/楼梯/塔结构） | `building_market`（真实 Registry id）、`building_customs_house`、构件以 `asset_`/`candidate_*` 记录 | `building` |
| `prop` | 港口/船上道具（桶、箱、宝箱、炮、炮弹、旗帜、桨、铲等） | 仓储道具、标识旗帜、工具、武器道具 | `prop_barrel`、`prop_flag_pennant`、`prop_cannon` | `prop` |

## 4. 类别明细

### 4.1 ship（船型与船体）

| 子类 | 定义 | 命名例子 | 允许资源类型 | 所有者 | D19/D21 使用边界 |
|---|---|---|---|---|---|
| `ship_type` | 船型定义（不可变 ShipTypeDef） | `ship_sloop`、`ship_small_fishing` | 纯数据（无模型要求）；视觉候选仅作映射证据 | ShipSystem（D4 船型定义） | D21 只读引用船型键；不得用候选模型改写船型数据 |
| `ship_hull_playable` | 可驾驶候选船体 | `ship_hull_small`、`ship_hull_medium` | 原始模型/网格候选（FBX 等） | ShipSystem（消费裁决）/ L 系列只登记 | D21 Hull Prefab 预检（Pivot、甲板基准、吃水、登船点）属 Runtime 延期 |
| `ship_hull_variant` | 同尺寸船型的视觉变体（海盗涂装等） | `ship_hull_pirate_small` | 视觉模型候选 | ShipSystem | 变体只改变外观键，不改变船型数值；写外观不写强逻辑（DESIGN_BOUNDARIES） |
| `ship_damage` | 损坏/残骸变体 | `ship_hull_wreck` | 视觉模型候选 | ShipSystem | 残骸候选不可当作可驾驶船体（A05 包结论，历史证据） |
| `ship_distant` | 远景剪影 | `ship_hull_ghost` | 低面视觉候选 | 表现层 | 只做远景表现键，不参与碰撞/路线 |
| `ship_rigging` | 桅杆、索具、帆（帆/舵/锚目前无独立证据包） | `ship_rigging_mast` | 视觉模型候选 | ShipSystem | 独立帆/舵/锚缺证据 → 缺口留给后续资源包，不缩放补数 |
| `ship_weapon` | 船上武器与弹药道具 | `ship_weapon_cannon`、`ship_weapon_cannonball` | 视觉模型候选 | ShipSystem/Combat（裁决） | 视觉键不写伤害、射程、装填数值 |
| `boat_tender` | 划艇/交通艇 | `boat_tender_row_large` | 视觉模型候选 | ShipSystem | 交通艇不做可驾驶主船体 |
| `ship_module`/`ship_function` | 船体模块与功能点（见 4.2） | `ship_module_*`、`ship_function_*` | 纯数据定义；改装物品 `item_*` | ShipSystem | D21 只读；改装效果由 Ship/Economy 裁决 |

### 4.2 hull_component（船体组件）

- 定义：船体可拆模块、舱室、功能点与改装部件。
- 命名：内容定义 `ship_module_<模块>`、`ship_function_<功能>`；**进入 Inventory 的改装物品沿用 `item_<模块>`**（D15 冻结，不改）。
- 允许资源类型：模块/部件的视觉候选模型（原始网格）、功能点数据定义。**不包含**航速/重量/伤害/价格/效果公式（D15-A06 口径）。
- 所有者：ShipSystem（船体/舱室/改装）；Inventory（物品）写入归各自系统。
- D19/D21 边界：D21 用 `ship_module_*`/`ship_function_*` 建装配/状态逻辑；本词典不裁决槽位上限、强度、耐久（D14-S03 二次验证不越目录上限）。

### 4.3 port_service（港口服务）

- 定义：港口向船与玩家提供的服务功能定义：装卸（cargo）、修理（repair）、补给（provision）、上下船（embark）、排队（queue）、检疫（quarantine）、检查（inspection）。
- 命名：`port_service_<服务键>`，如 `port_service_repair`、`port_service_cargo`。
- 允许资源类型：服务数据定义；服务建筑/设施视觉归 `building` 或 `prop` 子类，**服务本身不占用模型类别**。
- 所有者：D19 港口域（到港/靠泊/在修/返航状态机）；ShipyardSystem（工期/材料/服务）；Economy（价格）。
- D19/D21 边界：D19 靠 `port_service_*` 与 PortVisit 状态（Docked/Berthed/InService/Boarded/Departed/Returned，D19-L02 契约）建业务闭环；服务排队、费用与工期不在此词典内定义数值。

### 4.4 dock（码头/栈桥）

- 定义：靠泊与装卸的设施结构：货运码头、栈桥、平台模块、舷梯。
- 命名：`dock_<位置或模块键>`，如 `dock_cargo_wharf`、`dock_pier_module`。
- 允许资源类型：结构视觉候选（平台/栈桥/舷梯网格）；碰撞、缆桩、跳板、护栏、潮位/断缆行为全部属 Runtime 阶段。
- 所有者：D19 港口域（靠泊设施状态）；场景布局归 Runtime。
- D19/D21 边界：D19 只读 `dock_*` 键做靠泊匹配；码头结构碰撞/通行/排队行为延期。**历史注意**：A 系列准入目录把 Kenney Pirate Kit 的 `port_dock_pier_module`/`port_platform_module` 归到 `domain=ship`，属 A 系列口径（SUPERSEDED，见第 7 节弃用映射），本词典冻结 `dock → building`。

### 4.5 berth（泊位）

- 定义：泊位与锚泊点：大船泊位、小船泊位、外锚地。
- 命名：`berth_<规格或位置>`，如 `berth_large_01`、`berth_small`。
- 允许资源类型：泊位数据定义（规格、位置、吃水限制）；泊位视觉/碰撞归 Runtime。
- 所有者：D19 港口域（泊位占用/释放状态机）。
- D19/D21 边界：D19 用 `berth_*` 做靠泊裁决与泊位占用；吃水/潮位判定公式归 ShipSystem/D21，本词典不写数值。

### 4.6 building（建筑）

- 定义：功能建筑定义与建筑构件。
- 功能建筑九类（01_BUILDING_OVERVIEW 冻结）：港口（码头/泊位/灯塔/海关）、船坞（修船棚/帆具坊/炮匠坊）、贸易（市场/仓库/交易所/商会）、生活（住宅/酒馆/旅店/诊所）、社会（教堂/祈祷所/会所/学校）、防御（炮台/城墙/军营/检查站）、生产（农场/磨坊/木材场/渔场）、探索（瞭望塔/洞穴入口/遗迹）、管理（市政厅/监狱/档案室）。
- 构件子类（真实 intake 角色，A 系列证据）：`building_shell_module`（墙/角件/柱/地板）、`building_opening_module`（门洞/窗洞/围挡）、`building_access_module`（楼梯）、`building_roof_module`（屋顶）、`building_drainage_module`（排水）、`port_building_module`（塔/城堡结构）。
- 命名：内容定义 `building_<功能>`（Registry 已有 80 个真实 id，如 `building_market`、`building_customs_house`、`building_cargo_wharf`、`building_drydock`）；构件视觉以 `asset_`/`candidate_*` 资源记录关联。
- 允许资源类型：建筑数据定义 + 构件视觉候选（模块化拼装）。
- 所有者：Building/Shipyard（建筑、船坞、工期、材料、服务）；防御类归 Faction/Combat 读取；经济类价格归 Economy。
- D19/D21 边界：D19 用 `building_*` 做服务/交互引用；建筑实例状态（缺货/停工/关闭/火灾/战争损伤/占领/封锁/修复/废弃）由所属系统裁决，本词典不改变任何建筑状态。

### 4.7 prop（港口/船上道具）

- 定义：近景可交互或装饰道具：仓储道具（桶/箱/宝箱）、标识旗帜、工具（桨/铲）、武器道具（炮/炮弹）。
- 命名：`prop_<道具键>`，如 `prop_barrel`、`prop_flag_pennant`、`prop_cannon`。
- 允许资源类型：视觉模型候选；抓取/摆放 Pivot、碰撞、批处理归 Runtime。
- 所有者：表现层/场景装配（Runtime）；交互裁决归对应系统（武器 → Combat）。
- D19/D21 边界：D19 可引用 `prop_*` 做近景装配键；道具不携带经济/战斗数值。

## 5. 所有者与只读消费者汇总

| 数据层 | 唯一写者 | 只读消费者 |
|---|---|---|
| 船型定义（`ship_*`、ShipTypeDef） | ShipSystem | D19/D21/D23 逻辑任务、验证器 |
| 船体/模块/功能点（`ship_module_*`/`ship_function_*`/`item_*`） | ShipSystem / Inventory（物品） | D14 战斗、D21、D22 UI |
| 港口访问状态（PortVisit/Docked/Berthed/InService/Boarded/Departed/Returned） | D19 港口域（D19-L02 契约） | D21/D23 只读 |
| 建筑/船坞状态与工期 | Building/Shipyard | Economy、Faction、D19 |
| 泊位/码头占用 | D19 港口域 | D21 只读 |
| 资源候选记录（`asset_*`，L02 Schema） | L 系列登记任务（L21—L24）；L42 编译 | 全部验证器只读 |

## 6. 子类 → L02 `category` 映射（L42 编译器必须遵守）

| L11 类别 | L02 `category` 枚举值 |
|---|---|
| ship（全部子类） | `ship` |
| hull_component（全部子类） | `ship` |
| port_service | `building` |
| dock | `building` |
| berth | `building` |
| building（功能定义与构件） | `building` |
| prop | `prop` |

规则：**每条资源记录的 `category` 必须取自 L02 Schema 的 11 值枚举，并经本表落到唯一值**；与 L02 枚举不一致 → 记录非法（L45 一致性审计复核）。

## 7. 弃用映射（冻结）

| 历史名称 | 历史归属（A/S/R 系列，SUPERSEDED） | L 系列对应 | 说明 |
|---|---|---|---|
| intake `domain=ship`（含 `port_platform_module`、`port_dock_pier_module` 角色） | 准入目录把码头/平台归 ship | 本词典冻结：`dock` → `building` | A 系列口径只作历史事实，不阻塞新分类 |
| intake 角色 `ship_hull_playable_or_distant` | 准入目录 | `ship_hull_playable` / `ship_hull_distant` 两子类 | 语义拆分：可驾驶与远景是两个键 |
| intake 角色 `ship_damage_variant`、`ship_distant_silhouette` | 准入目录 | `ship_damage` / `ship_distant` | 同义迁移 |
| intake 角色 `ship_mast_rigging`、`ship_weapon_prop`、`boat_tender` | 准入目录 | `ship_rigging` / `ship_weapon` / `boat_tender` | 同义迁移 |
| intake 角色 `port_building_module`、`port_flag_sign_prop`、`port_storage_prop` | 准入目录 | `building` 构件 / `prop` 子类 | 按第 4.6/4.7 节归类 |
| D4 运行时船型裸键 `sloop`、`small_fishing` | ShipTypeDef.Id（运行时主键） | 内容定义 `ship_sloop`、`ship_small_fishing` | 两层 id 并存：运行时主键不动；资源/内容层用 `ship_` 前缀。**注意设计文档 `fishing_boat` 与代码 `small_fishing` 不一致**，以代码/注册表真实键为准，不在本词典裁决船型目录开放顺序 |
| A 系列候选 content_id（`d18a05_*`、`candidate_*`） | 准入目录候选 | 只读证据引用 | L 记录引用它们作 content_ids 证据，不复制为新内容定义 |
| 占位符 `placeholder_*`（replacement map） | 占位映射 | 保持 `placeholder` 状态直到真实映射证据（L33/L34 审计） | 当前 Registry 船舶/港口相关占位仅 `building_*` 类 80 条；`ship_*`/`prop_*`/`dock_*`/`berth_*`/`port_service_*` 无 Registry 记录，**不得补造** |

## 8. 不可混用关系（冻结）

1. **船型数据 ≠ 船体视觉候选**：`ship_type`（数值、载重、吃水、价格）只能由 ShipSystem/D4 船型定义裁决；候选模型（`asset_*`）只映射外观键，**不得反向写入 ShipInstance / ship_type_defs**。
2. **内容定义 id ≠ 资源记录 id**：`building_market`（内容层）与 `asset_*`（资源记录）是两个命名空间；禁止用资源记录 id 充当内容定义 id，也禁止游戏数据反向依赖候选文件（L15）。
3. **建筑构件 ≠ 功能建筑**：构件模块（墙/屋顶等拼装件）只能映射到建筑视觉装配；一个构件不能伪装成 `building_market` 之类的功能建筑定义。
4. **dock/berth 状态 ≠ 建筑状态**：泊位/码头占用归 D19 港口域；建筑实例状态（停工/火灾/占领等）归 Building/Shipyard——两个状态机不互写。
5. **port_service ≠ 经济/工期数值**：`port_service_*` 只定义服务存在性；排队、价格、工期由 Economy/Shipyard 裁决。
6. **损坏/远景变体 ≠ 可驾驶船体**：`ship_damage`、`ship_distant` 不得被当作 `ship_hull_playable` 消费（A05 包结论，历史证据）。
7. **数量与名称不可伪造**：禁止用缩放、改名或复制同一模型补船型/模块数量（D18-A05-R01 复核结论，历史证据）；本词典不统计任何数量。
8. **历史状态不改写**：任何 `placeholder`/`Candidate` 条目在本词典层面不得被写成已接入、已导入或已批准；晋级只能走 L35 迁移审计与未来 Runtime 门禁。

## 9. D19 / D21 使用边界（冻结）

| 消费方 | 允许读取 | 禁止 |
|---|---|---|
| D19 港口业务（L02—L33） | `building_*`、`port_service_*`、`dock_*`、`berth_*`、`ship_*`（只读键）；PortVisit 状态机契约 | 写 ShipSystem 数值；触碰场景/Prefab；以分类代替业务裁决 |
| D21 船/天气/海洋（L01—L32） | `ship_*`、`ship_module_*`、`ship_function_*`、`hull_component` 键；吃水/锚泊判定只读 | 直接引用 `Assets/Art/Candidates` 候选文件；改船型定义；把视觉候选写为可驾驶 |
| 两类共同 | 经 L02 Schema 记录（`asset_*`）只读引用候选证据 | 反向依赖候选路径；把本词典内容当作模型存在证明 |

## 10. 链接与证据

- 基线：[D18_LOGIC_GOVERNANCE_BASELINE.md](../D18_LOGIC_GOVERNANCE_BASELINE.md)
- 内容生产冻结：[D15_CONTENT_PRODUCTION_FREEZE.md](../D15_CONTENT_PRODUCTION_FREEZE.md)
- 船舶设计：[ship/README.md](../../ship/README.md)、[ship/02_SHIP_TYPES.md](../../ship/02_SHIP_TYPES.md)、[ship/10_SHIP_DATA_MODEL.md](../../ship/10_SHIP_DATA_MODEL.md)
- 建筑/港口设计：[buildings/01_BUILDING_OVERVIEW.md](../../buildings/01_BUILDING_OVERVIEW.md)、[buildings/02_PORT_SHIPYARD.md](../../buildings/02_PORT_SHIPYARD.md)
- 数据模型：[DATA_MODELS.md](../../DATA_MODELS.md)
- 边界：[DESIGN_BOUNDARIES.md](../../DESIGN_BOUNDARIES.md)
- 章节：[D19 章节](../../../AGENT_DEVELOPMENT_CHAPTER_11_D19.md)、[D21 章节](../../../AGENT_DEVELOPMENT_CHAPTER_13_D21.md)
- 历史证据（只读，不作通过依据）：`assets/d18_prefab_candidate_intake_catalog.json`、`assets/replacement/d18_placeholder_replacement_map.json`、`../D18_SHIP_BUILDING_PROP_ASSET_PACKET.md`
