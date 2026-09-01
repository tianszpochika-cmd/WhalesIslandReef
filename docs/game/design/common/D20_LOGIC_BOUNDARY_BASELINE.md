# D20-L01 角色、NPC、动物逻辑边界基线

> 冻结日期：2026-09-01（Asia/Shanghai）
> 任务：`D20-L01`  —  结论：`LOGIC_PASS`（仅表示本基线的逻辑边界与依赖冻结，不表示 D20 章节完成、可玩或可发布）

## 1. 目的、范围与证据口径

本文件是 D20 纯逻辑轨的边界单一事实源，供 `D20-L02`—`D20-L40`、以及后续 D21—D24 的逻辑任务消费。它冻结：

1. 玩家、NPC、动物/宠物与 Presentation 的事实所有权、唯一写者、只读入口和请求入口；
2. D10、D11、D19 已存在公开类型对 D20 的消费边界；
3. 命令、读模型、事实事件、存档投影与失败回退的最小语义；
4. D20-L02—L27 并行、L28—L35 串行、L36—L40 汇合门禁，以及 D19-L48 的当前前置；
5. D20-A01—A06、D20-S01/S02 旧视觉/运行时产物的隔离清单。

本文件不是 Unity 场景、Prefab、Animator、模型、输入、物理、NavMesh、Canvas、网络或 LLM 设计，也不授予任何运行时晋级或发布权限。D20 逻辑代码必须保持可在无 Unity 对象、无场景和无资源二进制的条件下测试。

### 1.1 当前前置事实

- `D19-L01 LOGIC_PASS` 是冻结 D20 的最低前置；当前更强的 `D19-L48` 已真实通过。
- D19-L48 结果确认：D19 结果协议完整性通过，逻辑边界审计 `38/38` 零违规，Unity EditMode 程序集 `WhalesIslandReef.D19A02.Tests` 实测 `117/117` 通过；Player、场景、画面、输入、性能与发布仍为 Runtime Deferred。
- D19 当前公开接口位于 `game/Assets/Scripts/Systems/D19/`，规范见 `docs/game/design/common/D19_LOGIC_PORT_INTERFACE.md`。D20-L28 的港口命令部分因此可以消费 D19 Router；不能回读旧的 D19-R01-S02 阻断结论来否定当前 `D19-L48`，也不能把 D19-L48 误读成 D20 运行时通过。
- D10 的真实所有权与集成证据见 `docs/game/design/common/D10_ACCEPTANCE_REPORT.md`、`D10_PLAYER_IDENTITY_EXPRESSION.md` 和 `D10_APPEARANCE_OUTFIT.md`。
- D11 的真实所有权与集成证据见 `docs/game/design/common/D11_ACCEPTANCE_REPORT.md` 及 `game/Assets/Scripts/Systems/D11/` 下的公开系统/契约。
- D18 的 `content_id`、`resource_key`、角色分类和生态分类只作为只读内容元数据输入；候选路径、模型存在与运行时批准状态不是 D20 逻辑证据。
- 截至本冻结点，D20 纯逻辑 `game/Assets/Scripts/Systems/D20/` 尚未形成可被本文件冒充为已实现的系统集合；本任务只冻结边界，不提前创建相邻任务代码。

### 1.2 术语

| 术语 | 本基线含义 |
|---|---|
| 唯一写者（owner/writer） | 对某一类可变事实拥有唯一裁决和持久化写入权的系统。其它系统只能调用公开命令、提交意图或读取快照。 |
| 只读读模型 | 从所有者状态派生、不可反向写入事实的 DTO/快照；可供 Presentation、UI、D21—D24 或适配器消费。 |
| 意图/请求 | 对所有者提出的可拒绝操作；提出方不预先修改事实，结果以所有者返回的结果/事实为准。 |
| 事实事件 | 已由所有者裁决的可追溯结果，携带稳定事件 ID、原因/因果 ID、所有者和存档修订；Agent 或 Presentation 不能伪造事实事件。 |
| Presentation | 读取逻辑状态/事件并转换为动画、音频、视觉或提示的消费者；也可以提出输入意图，但不能写回事实。 |
| 运行时延期 | 已知需要 Unity/资源/设备/场景验证但不影响纯逻辑边界的事项；延期不是逻辑失败，也不是通过。 |

## 2. 全局不可越过的逻辑规则

### 2.1 单一写者和单向依赖

1. 每一份事实只有一个唯一写者。聚合服务可以编排，但不得复制一份可变真相。
2. 调用链保持单向：`命令/意图 → 权限与前置校验 → 所有者裁决 → 事实/状态 → 只读事件或读模型`。失败不产生假成功，不在调用方偷偷补写状态。
3. D20 只能通过本文件列出的公开 DTO、快照、命令方法和事件结果消费 D10/D11/D19；缺少公开契约时任务为 `LOGIC_BLOCKED`，不得通过反射、内部字段、序列化副本或硬编码路径绕过。
4. SaveSystem 负责组合/保存既有所有者快照。D20 投影只能保存 D20 自己拥有的语义字段，并引用或嵌入所有者公开版本化快照；不能另造 D10/D11/D19 的第二个事实源。

### 2.2 强逻辑红线

`game/Assets/Scripts/Systems/` 内的 D20 逻辑禁止引用 LLM、HTTP、网络客户端、远端服务、系统时钟或不可控随机。Agent 只生成经过 Schema 校验的提议；身份、关系、经济、库存、健康、生态、奖励、法律、港口状态和存档事实必须由所属系统裁决。

Presentation、Animator、Prefab、模型、骨架、Avatar、Transform、NavMesh、Canvas、场景、碰撞器、相机、输入系统和设备权限都不属于 D20 逻辑写路径。逻辑层使用稳定 ID、逻辑位置引用、状态枚举、动作意图和可序列化数据，不使用 Unity 对象引用。

### 2.3 结果、回退与存档

- 每个命令都必须有稳定 `command_id`、行为者 ID、目标 ID（适用时）、世界日/存档修订（适用时）、确定性失败码和重放/幂等语义。
- 事实事件先于表现事件；事件至少能够追溯 `event_id`、`owner_system`、`cause_ids`、`save_revision` 和关联命令。
- 无法表现、无资源、逻辑前置不满足、角色不可用、受伤/昏迷/死亡、取消或读档中断时，逻辑保留可恢复状态并返回稳定回退/失败码；Presentation 缺失不能改变逻辑结果。
- 迁移只补齐文档明确允许的默认值；遇到不可能状态、跨所有者冲突或未来不支持版本必须拒绝并保留原始数据，不能编造角色、动物、关系、港口或资源事实。

## 3. 事实所有权矩阵

以下矩阵冻结“谁能写什么”。“D20 角色层”是本章后续纯逻辑任务将实现的 D20 自有语义层，不等于接管下列既有事实。

| 事实域 | 唯一写者/裁决者 | D20 可做的事 | 公开边界 | 禁止事项 |
|---|---|---|---|---|
| 玩家核心档案、技能、成长与通用身份 | Core/Progression 及其既有 PlayerProfile 所有者 | 读取稳定身份引用，提出角色动作或身份表达请求 | 既有 PlayerProfile/Progression 快照；由所有者定义的命令 | 直接写技能、成长、金币、声望、船、房产、库存或地图 |
| 玩家语义存在/可行动状态 | D20 后续 `D20PlayerPresenceSystem`（L03）只拥有 D20 自己定义的语义快照 | 维护港口/船上/航行/不可行动等逻辑状态，接受所有者事实输入 | L03 只读语义状态、状态迁移结果、失败码 | 写 Transform、坐标、刚体、相机、碰撞或场景出生点 |
| 玩家动作意图与打断 | D20 后续 L04—L06/L28 的动作命令层，实际业务结果仍归所属系统 | 校验意图、优先级、互斥、取消/打断并向业务所有者转发 | D20 action intent/result DTO、D19 公共路由、D10/D11 公共命令 | 以动作动画代替业务裁决，或用客户端状态覆盖服务结果 |
| 玩家身份表达、历史、文件/证件 | `D10PlayerIdentitySystem` | 读取 `Snapshot`，通过公开 port update 提出更新 | `Snapshot`、`InspectAtPort`、`ApplyPortUpdate`、`SaveSystem.SaveD10Identity/LoadD10Identity` | 直接写 `PlayerState.Name` 兼容镜像、`identity` JSON、法律状态或文档集合 |
| 玩家外观槽位、拥有、耐久、染色、伤痕 | `D10AppearanceSystem` | 读取外观快照/资源状态，提出试穿、购买、装备、染色、修复或伤痕意图 | `Snapshot`、`Context`、`Find`、`TryOn`、`Purchase`、`Equip`、`Dye`、`Repair`、`ApplyInjuryAppearance` | 由身份、颜色、动画、Prefab 或资源键直接裁决职业、阵营、关系、收入或法律身份 |
| NPC 稳定身份、Persona/记忆引用与角色档案 | Character/Crew/D9 既有角色所有者；D20 L02/L14 只定义稳定契约/投影 | 读取角色快照，生成经过校验的角色意图 | 角色公开快照、稳定 ID、过滤后的 Agent 输入 | 在表现层生成身份真相，或让 Agent 直接写姓名、关系、健康、经济、任务 |
| NPC 岗位、轮班、疲劳、健康、行为 | `D10CrewBehaviorSystem`（`OwnerSystem="CrewSystem"`） | 读取状态，提交调岗、请假、值班、归队或行为请求；D20-L29 只能聚合 | `EnsureState`、`AssignShift`、`RequestDutyTransfer`、`ApproveDutyTransfer`、`RequestLeave`、`ApproveLeave`、`ReturnFromLeave`、`RequestDeparture`、`ApproveDeparture`、`Tick`、`ApplyHealthChange`、`ApplyAlcoholEffect`、`EvaluateMutinyForewarning`、`CalculateEfficiency` | 直接改 role/shift/fatigue/health/behavior，或用 NPC 动画/远近代理覆盖 D10 状态 |
| NPC 关系事实图 | A01 `RelationSystem`/关系所有者 | 读取边和原因，提交带 `FactEvent`/`RelationChangeIntent` 的事实意图 | `ApplyFactEvent`、`ReadEdge`、`ReadEdges`、`CaptureSnapshot`、`Restore`；`D10RelationPanelReadModel` 只读 | 直接改 Value/Confidence/Cause/Visibility；把浪漫投影当成 canonical graph 写者 |
| D10 浪漫/社交投影 | D10 对应关系/酒馆系统在其公开入口内裁决；canonical graph 仍由关系所有者写入 | 消费结果或提交 D10 公共 action request | `D10RelationSystem.RefreshFromCanonicalGraph`/`Apply`、`D10TavernSocialSystem.BuildSceneSnapshot`/`RequestAction`/`Resolve` 等公开入口 | 新建第二份关系真相，直接改 D10 runtime snapshot 或替 D9/Economy/Ship 决定结果 |
| 港口访问、靠泊、泊位、登离船、返港、服务可用性、幂等 | D19：`D19PortVisitSystem`、`D19BerthSystem`、`D19PortAvailabilitySystem`、D19 Router/幂等组件 | 读取港口状态/可用性，提交统一的港口路由命令 | `D19PortCommandRouter.Route(D19RoutedPortCommand)`、`D19PortReadModel`、`D19PortSaveProjection/Migration` | 直接写 D19 生命周期、泊位、服务、成员位置、幂等表或内部事件 |
| 现金、价格、工资、市场、库存、船舶、债务、法律、奖励、任务 | 各自既有唯一所有者（Economy/Market/Inventory/Ship/Finance/Faction-Legal/Reward/Quest） | 只能读取公开结果或提交意图，经所属系统裁决 | 所属系统公开快照、结果和事实事件 | 将“交互/动画/捕获/救援/靠泊”直接转化为金钱、伤害、奖励、库存或法律事实 |
| 野生鱼、植物、种群、生态压力/恢复 | D4 `EcologySystem`；D11 相关系统只编排或投影 | 读取生态公开快照，提交生态操作请求 | D11 harvest/vegetation/aquatic 公开请求、结果和事件；D4 Ecology 快照 | D20 自己计算/写数量、压力、保护状态、生境或死亡事实 |
| 钓鱼收获、处理与销售的跨域结果 | `D11FishingHarvestSystem` 负责操作编排/结果投影；生态、库存、经济仍由各自所有者写入 | 读取 `D11HarvestResult`/处理结果，提交公开 harvest/process/sell 请求 | `Harvest`、`Process`、`Sell`、`CaptureSnapshot`、`Restore` 与 `EventLog` | 直接扣货、改鱼群压力、改金币或把视觉捕获当成成功事实 |
| 岛屿池塘/近岸笼、设施、输入、水体与养殖结果 | `D11IslandAquacultureSystem`；生态修正仍回到 Ecology | 读取设施/鱼类状态，提交 supply/advance/harvest 请求 | `Snapshot`、`FindFacilitySnapshot`、`Build`、`SupplyInputs`、`Advance`、`Harvest`、`CaptureRuntime` | 直接改建筑、鱼量、输入、生态压力或市场库存 |
| 活鱼舱状态 | `D11LiveFishHoldSystem`；货物/容量的通用库存仍属 D3 | 读取承载/拥挤/重量快照，提出投放、维护、收获请求 | `Snapshot`、`StockFry`、`Maintain`、`Harvest`、`CaptureRuntime` | 直接写 `D11LiveFishHoldState`、D3 cargo 或船舶容量 |
| 宠物生命/照料/位置/隔离/丢失状态 | `D11PetSystem`（`OwnerSystem="AnimalCareSystem"`） | 通过 D11 公开入口提交领养/照料/移动/找回意图；D20 可拥有独立的行为/跟随资格投影，但不能复制宠物事实 | `Snapshot`、`CrewReactions`、`Adopt`、`Feed`、`Clean`、`Comfort`、`MoveTo`、`Quarantine`、`Treat`、`ReleaseFromQuarantine`、`MarkLost`、`Search`、`CatchMouse`、`Warn`、`AdvanceDay` | 直接写 pet state、库存消耗、宠物关系或丢失/死亡事实；L23 不得重建第二个所有权真相 |
| 珍稀动物收集、图鉴、识别、合法转移 | `D11RareAnimalCollectionSystem`；野生生命周期归 Ecology，法律/势力/奖励/任务归各自所有者 | 读取收集结果，提交 discover/identify/observe/rescue/transfer/release/collect/illegal-trade 请求 | 对应 `D11*Request`/`D11RareAnimalResult`、`Snapshot`、`FactionSnapshot` | 由 Presentation 或 D20 直接创建野生动物、改生态、授予奖励、改法律或库存 |
| 鲸豚路线/遭遇投影 | `D11WhaleDolphinSystem` 只写其 route runtime projection/facts/pending requests；生态事实由 Ecology，海况由 D9 Environment | 读取 encounter/route 资格并提交动作请求 | `Snapshot`、`FindEncounter`、`AdvanceMigration`、`Resolve` 及 marine read snapshots | 把路线投影当成 Ecology、奖励、任务、船员或法律事实的唯一写者 |
| Presentation/视觉/音频/输入提示 | Presentation 层是消费者，不拥有上述逻辑事实 | 读取快照/事件并提出意图；资源缺失使用明确降级 | D20 后续 `D20PresentationEventContracts`、只读 action read model | 任何直接写回身份、关系、行为、动物、港口、经济、存档或所有者私有字段 |
| 存档根与迁移组合 | `SaveSystem`/既有各领域 SaveProjection | D20 只提供自有 projection/migration，交给存档组合器 | 版本化 DTO、`CaptureSnapshot`/`Restore`、迁移结果 | 用 D20 projection 替换 D10/D11/D19 的投影，或在迁移中伪造不存在的实体 |

## 4. 已存在公开接口：D10、D11、D19

本节是 D20 允许依赖的“公开面”。列出的类型/方法来自当前仓库代码和对应验收文档；D20 不得依赖未列出的内部字段。方法名的具体参数以源代码为准，本节只冻结所有权和方向。

### 4.1 D10：角色身份、外观、关系、船员行为与酒馆编排

| 领域 | 公开读面 | 公开写/请求面 | D20 使用限制 |
|---|---|---|---|
| 身份 | `D10PlayerIdentitySystem.Snapshot`、`InspectAtPort` | `ApplyPortUpdate`；SaveSystem 的 `SaveD10Identity`/`LoadD10Identity` | D20 只提出身份表达/港口更新意图；身份表达、历史合并、文档校验和外观委托由 D10 裁决。 |
| 外观 | `D10AppearanceSystem.Snapshot`、`Context`、`Find`；`D10AppearanceSnapshot`、`D10AppearanceLoadout` | `SetContext`、`TryOn`、`Purchase`、`Equip`、`Dye`、`Repair`、`ApplyInjuryAppearance` | 只能消费结果/资源状态；`AssetId`/`ResourceKey` 缺失只能得到占位/失败状态，不能声称资源存在或已导入。 |
| canonical 关系 | `RelationSystem.KnowsCharacter`、`KnownCharacterIds`、`ReadEdge`、`ReadEdges`、`CaptureSnapshot` | `RegisterCharacter`、`ApplyFactEvent(FactEvent, RelationChangeIntent)`、`Restore` | D20 通过事实意图请求关系所有者裁决；不直接写 `RelationEdge`，不把 Panel/romance projection 当作写入口。 |
| 关系展示 | `D10RelationPanelReadModel.Build` 返回 `RelationPanelSnapshot/Row/Cause` | 无事实写入口 | 仅作为只读解释/原因读模型，不能反向修改 canonical graph。 |
| 船员行为 | `D10CrewBehaviorSystem.EventLog`、状态/效率/前兆结果 | `EnsureState`、shift/duty/leave/departure 请求与批准、`Tick`、健康/酒精结果 | D20-L16/L17/L29 只读或转发；D10 仍是 role/shift/fatigue/health/behavior 唯一写者。 |
| 酒馆社交 | `D10TavernSocialSystem.BuildSceneSnapshot`、`EventLog`、resolution | `EnterTavern`、`RequestRecruitment`、`RequestRest`、`RequestMeal`、`RequestAlcohol`、`HearRumor`、`RequestPokerEntry`、`RequestDance`、`RequestNightTalk`、`OpenSideJobBoard`、`RequestSideJobOffer`、`SkipSensitiveContent`、`Resolve`、`RequestAction` | D20 只能通过酒馆公开服务提出请求；D10TavernOwnerServices 编排 D9/Economy/Ship/D10/Crew，不把酒馆当成经济、关系或行为事实的替代写者。 |

D10 的特殊边界：`D10PlayerIdentitySystem` 的 `PlayerState.Name` 是兼容镜像，不是 D20 可写字段；法律/wanted/permission/penalty 属 Legal，关系属 Crew/Relation，外观属 Appearance。身份更新失败必须在 D10 前置校验阶段返回，不允许 D20 先写半个身份再等待外观或法律结果。

### 4.2 D11：生态、鱼类、动物、宠物与水生逻辑

| 领域 | 公开读面 | 公开写/请求面 | D20 使用限制 |
|---|---|---|---|
| 钓鱼/采集 | `D11FishingHarvestSystem.LastHarvest`、`EcologySnapshot`、`Gears`、`Locations`、`EventLog` | `Harvest(D11HarvestRequest, D9WeatherSeaSnapshot)`、`Process`、`Sell`、`CaptureSnapshot`、`Restore` | D20 只编排行为意图；压力、货物、价格/金币由 Ecology/D3/Economy 裁决。天气/海况仅读 D9 快照。 |
| 植被 | `D11VegetationHarvestSystem.LastHarvest`、catalog/definition 结果 | `Identify`、`Harvest` | D11 结果是适配器输出；资源数量由 Ecology，货物由 D3Inventory，不能由 D20 改写。 |
| 宠物 | `D11PetSystem.Snapshot`、`CrewReactions`、`EventLog` | `Adopt`、照料、移动、隔离、治疗、丢失/搜索/找回和 `AdvanceDay` 公开方法 | L23 可以定义 D20 follow/interaction eligibility projection；所有宠物生命、照料、地点、丢失状态必须回到 D11PetSystem。宠物的 Crew reaction 通过 RelationSystem 公开事实意图进入关系图。 |
| 稀有动物 | `D11RareAnimalCollectionSystem.Snapshot`、`FactionSnapshot`、`Catalog` | `Discover`、`Identify`、`Observe`、`Rescue`、`StartLegalTransfer`、`AdvanceTransport`、`Release`、`Collect`、`IllegalTrade` | 收集/图鉴与合法转移不等于野生种群事实；D20 不得创建动物或直接写 Ecology/Reward/Quest/Legal。 |
| 养殖与活鱼舱 | `D11IslandAquacultureSystem.Snapshot`/facility snapshot；`D11LiveFishHoldSystem.Snapshot`、`CapacityVolume`、`UsedVolume`、`TotalLiveWeight`、`Crowding` | 对应 `Build`/`SupplyInputs`/`Advance`/`Harvest` 与 `StockFry`/`Maintain`/`Harvest`/`CaptureRuntime` | D20 只消费状态和结果；不直接写设施、输入、水体、活鱼舱或 D3 cargo。 |
| 鲸豚/水生遭遇 | `D11WhaleDolphinSystem.Snapshot`、`FindEncounter`、marine environment/ecology read snapshots | `AdvanceMigration`、`Resolve(D11WhaleDolphinActionRequest)` | 路线/遭遇投影与生态事实分离；奖励、任务、船员/船舶、法律和环境事实由其所有者写入。 |
| 救援/宠物关系桥接 | `D11EcologyVoyageIntegrationSystem.EventLog` 与结果 | `RescueRareAnimal(D11RescueRequest)`、`ApplyPetCrewRelations(D11PetActionResult)` | 先由 Collection/Pet/生态所有者产生事实，再写 EventLog/提交 Relation intent；D20 不能跳过中间所有者。 |

### 4.3 D19：港口只读消费和唯一命令路由

D20 对 D19 的依赖只允许经过以下公开面：

1. `D19PortCommandRouter.Route(D19RoutedPortCommand)` 是到港、靠泊、泊位、登船、离港、返港和服务命令的唯一入口。请求至少携带 `CommandId`、`Kind`、`ActorId`、`ShipId`、`PortId`、适用的 `BerthId/ServiceId`、`WorldDay`、`SaveRevision`、`Claims/Parameters/ActorIds`。
2. 路由结果 `D19RoutedCommandResult` 公开 `Success`、`IsDuplicate`、`CommandId`、`Kind`、`ReasonCode`、`LocKey`、`RetryPolicy`、`StateAfter`、`EventDraft`、`CompensationDraft` 和 `ResultData`。D20 只能读取结果，不得写入 Router、VisitSystem、BerthSystem、AvailabilitySystem 或 Idempotency 的字段。
3. `D19PortReadModel` 提供只读访问/港口/泊位/指纹数据；`D19PortVisitReadModel` 的 `State`、`PortId`、`BerthId`、`ShipId`、`StateLocKey`、`IsInPort`、`IsBerthed`、`IsBoarded`、`CanStartService`、`CanDepart` 等属性只能被读取。
4. `D19PortVisitState` 的生命周期（`None`、`PortVisit`、`Docked`、`Berthed`、`InService`、`Boarded`、`Departed`、`Returned`）由 D19 状态机保护；D20 不能制造非法迁移。
5. `D19PortSaveProjection`（当前 SchemaVersion 1）和 `D19PortSaveMigration.Migrate` 是 D19 访问状态、泊位分配、服务、成员位置、幂等记录、挂起命令、补偿草案和事件游标的公开保存边界。D20 只能引用/组合，不可另存同名事实。
6. D19 的库存、市场、船、船员、经济等适配器仍把裁决委托给 D3/D4/D9/Economy/Ship/Crew；D20 不得把港口结果解释为这些外部域已变更，除非对应所有者返回成功事实。

## 5. D20 自有逻辑边界与下游任务依赖

### 5.1 D20 自有字段的最小范围

D20 可以拥有并持久化的事实限于本章定义的角色/NPC/动物语义层：稳定逻辑 ID、身份/生命阶段/可用性契约、玩家语义 presence、动作意图/优先级/打断状态、行为状态机的 D20-owned 状态、交互资格、Presentation 只读事件游标和 D20 版本化投影。任何字段只要已经由 D10、D11、D19 或其它既有系统拥有，就不能在 D20 再复制成可变事实。

特别约束：

- `D20CharacterIdentityContracts` 负责稳定 ID、身份、生命阶段、可用性与不可变标识的契约，不等于拥有 D10 身份表达、法律证件、外观或 D11 宠物事实。
- `D20PlayerPresenceSystem` 只表达语义存在/可行动状态，不表达 Transform/世界坐标/碰撞。
- `D20ActionIntentContracts`、优先级与打断层只管理意图和 D20 action state；走、跑、交互、交谈、钓鱼、登船、掌舵、受击等业务结果仍由所属系统裁决。
- `D20NpcBehaviorStateMachine`/`D20NpcBehaviorService` 可以拥有 D20 行为投影和调度决策，但 D10 的岗位、轮班、疲劳、健康、行为事实与 D19 港口状态必须通过公开入口更新。
- `D20AnimalBehaviorStateMachine`/`D20AnimalBehaviorService` 可以拥有 D20 行为、交互资格或跟随投影；生态种群、宠物生命/照料/地点、稀有动物收集和水生事实必须委托 D11/生态所有者。
- `D20PetOwnershipSystem`（L23）不得成为 `D11PetState` 的第二写者。若 D11 当前公开面不足以完成某项领养/找回/所有权校验，该部分必须记录为缺失公开契约并阻断，而不是直接访问 D11 私有状态。
- `D20PresentationEventContracts`、`D20ActionReadModel` 是逻辑→表现的只读面；表现层不拥有逻辑数据。

### 5.2 波次和串行依赖

| 阶段 | 任务 | 允许依赖/必须保持的边界 | 验收门 |
|---|---|---|---|
| 冻结 | `D20-L01` | 本文件冻结后才允许其它 D20-L 任务开始；D19 至少 L01，当前已达 L48 | 本文件、结果 JSON、handoff、当日日志、状态同步和 diff 校验齐全 |
| 角色/NPC 基础并行 | `D20-L02`—`D20-L15` | 只消费本基线、D10 公开类型与 D18 只读内容元数据；不得等待/修改表现资源 | 稳定 ID、身份、意图、优先级、打断、Presentation 事件、读模型、外观/身份/特质/条件/关系适配器、NPC 身份/角色契约均纯逻辑可测 |
| NPC/动物/存档基础并行 | `D20-L16`—`D20-L27` | 只消费本基线与公开 D10/D11/D19 快照/命令；不得重建 D10/D11 事实 | 日程、行为、动物身份/物种/宠物/生态/水生适配器、保存投影和迁移均明确唯一写者 |
| 串行聚合 | `D20-L28`—`D20-L35` | L28 是 D20 角色命令唯一入口；L29/L30 聚合但不夺取 D10/D11/D19 所有权；L31 统一排序/重放；L32—L35 只能验证已冻结的公开契约 | L28→L35 按序完成；任一缺少公开契约、非法迁移或跨域直写必须失败/阻断 |
| 汇合门禁 | `D20-L36`—`D20-L40` | L36 静态审计、L37 纯逻辑运行器、L38 发布接口、L39 Runtime Deferred、L40 章节验收 | 不验模型、Animator、Prefab、场景、真实画面、设备性能；只验逻辑、保存、边界和可重放证据 |

### 5.3 D20-L28—L35 的强制顺序

1. **L28**：`D20CharacterCommandService` 是 D20 角色命令的唯一 D20 入口。身份、presence、action、condition 和 port permission 统一经它校验/转发；涉及港口时必须调用 D19 Router，不得另造 `EnterPort`、`Board` 或 `Depart` 私有路径。`D19-L48 LOGIC_PASS` 已满足港口部分的前置。
2. **L29**：`D20NpcBehaviorService` 聚合岗位、日程、行为、可用性和事件，但 D10CrewBehaviorSystem、RelationSystem、D19 的港口所有权不变。
3. **L30**：`D20AnimalBehaviorService` 聚合动物、宠物、生态读模型、交互资格和事件，但 D11/ Ecology 的状态只通过公开请求和结果流动。
4. **L31**：`D20ActionEventRouter` 固定命令接收、校验/拒绝、所有者结果、D20 状态变化、事实事件和 Presentation 事件的顺序；重复命令重放已有结果，不产生第二次副作用。
5. **L32**：content ID/resource key 只能使用 D18 已冻结的逻辑元数据；禁止 `Assets/Art/Candidates`、外部归档、绝对路径或二进制存在性作为逻辑通过条件。
6. **L33**：保存→加载必须保留稳定 ID、状态、意图/取消、事件游标、所有者引用和失败恢复信息；各域快照通过所有者公开投影组合。
7. **L34**：固定种子至少 1,000 Tick；验证状态可达、可恢复、无永久动作锁，不以帧率或画面作为证据。
8. **L35**：覆盖唯一写者、幂等、死亡/取消、非法迁移、关系/经济/库存/生态/港口不可越权等不变式。

### 5.4 D20 对后续章节的依赖

| 下游 | 依赖方式 | 不得做什么 |
|---|---|---|
| D21-L01 及 D21 逻辑轨 | 消费本基线冻结的角色/动物/港口只读接口，结合船舶、天气、海洋所有者公开状态 | 不直接改 D20/D10/D11/D19 私有事实，不用 Transform/物理坐标替代逻辑状态 |
| D22-L01 及 D22 UI 逻辑轨 | 只消费 D20 action/identity/behavior read model 和 Presentation event contract，UI 命令必须回到 D20/D10/D11/D19 唯一入口 | UI/Canvas/输入层不写事实，不因按钮显示成功而伪造业务结果 |
| D23 跨系统确定性完整循环 | 通过公开 D20 command/read/event、D19 Router 和 D10/D11 owners 组装剧本 | 不把历史 D20 视觉/运行时结果作为角色逻辑证据，不绕过任何所有者 |
| D24 回归、迁移与逻辑候选版 | 审计 D20 公共接口、保存/迁移、内容 ID、边界和 Runtime Deferred | 不把 `Runtime Deferred`、Candidate 资源或旧 PlayMode/截图写成 ReleaseApproved |

## 6. 命令、读模型、事件和保存基线

### 6.1 命令/意图

D20 后续命令必须是可序列化的纯数据 DTO，并至少满足：

- 具备稳定 `command_id`、`actor_id`，以及适用的 `target_id`、`owner_system`、`world_day`、`save_revision`、`cause_ids`；
- 先校验生命阶段、可用性、权限、前置、互斥和打断，再调用事实所有者；
- 对 D10/D11/D19 的写操作只能表达“请求/意图”，不携带让 D20 直接覆盖外部状态的字段；
- 明确 `accepted/succeeded/rejected/duplicate/cancelled/unavailable` 等结果语义和稳定 `reason_code/loc_key/retry_policy`；
- 同一 `command_id` 重放必须得到等价结果，不能重复扣钱、改关系、改生态、改宠物、改港口或产生第二份副作用。

### 6.2 读模型

读模型必须只包含 D20 或外部所有者公开的只读值。逻辑位置用稳定 `location_ref`/语义枚举，资源用 `content_id`/`resource_key`，不带 `GameObject`、`Transform`、`Animator`、Prefab 路径或场景引用。读模型可缺少表现资源，但必须能表达 `available=false`、`placeholder`、`unavailable` 或明确失败原因。

### 6.3 事件顺序

D20 采用以下可重放顺序：

```text
CommandReceived
  -> Validation/AuthorityResult
  -> OwnerCommandResult (success/reject/duplicate)
  -> DomainFactEvent (仅所有者裁决成功后)
  -> D20StateProjectionUpdated
  -> PresentationReadOnlyEvent (可选；失败/缺资源也只能是只读提示)
```

失败或重复命令不得伪造 `DomainFactEvent`；若需要表现失败，失败事件必须携带原命令、稳定失败码和当前读模型，且 Presentation 不得回写。跨系统事实事件按 `cause_ids` 串联，D20 只发布自己拥有的投影事件或转发所有者公开事件。

### 6.4 保存和迁移

- D20 `D20CharacterSaveProjection` 只保存 D20-owned identity/action/behavior/animal projection、版本、命令幂等键和事件游标；D10/D11/D19 使用各自的公开投影。
- D20 `D20CharacterSaveMigration` 只迁移 D20 projection；缺失字段可使用文档化安全默认值，未知/未来版本、无法解析 ID、跨所有者冲突必须保留原文并失败。
- 读档后必须先恢复所有者快照，再恢复 D20 引用/投影；恢复顺序不能通过 Presentation 或旧 Prefab 推断实体存在。

## 7. Presentation 与历史视觉/运行时成果隔离

### 7.1 当前逻辑边界

Presentation 只订阅 D20 逻辑的稳定事件/读模型，把 `action_id`、`state`、`reason_code`、`content_id` 等转换为视觉/音频/提示。它不能：

- 通过动画完成、颜色、姿态、模型、远近代理、粒子、碰撞或音效直接改变身份、关系、岗位、健康、生态、宠物所有权、港口状态、经济、奖励、任务或存档；
- 以资源已存在、Prefab 可实例化、截图、Editor audit、Preview、固定镜头或单元隔离测试替代逻辑事实；
- 直接引用候选资源路径或把 `Candidate`/`IntakeEligible` 变成 `RuntimeApproved`/`ReleaseApproved`；
- 让缺失 Animator/Avatar/Prefab/场景阻断本可恢复的逻辑结果，除非逻辑契约本身要求返回明确的不可用/回退状态。

### 7.2 历史 D20-A/R/S 结果隔离清单

下表中的结果全部保留为历史诊断材料。它们不属于 D20-L01 的逻辑证据，不解锁 D20-L02—L40 的实现权限，也不改变 D18/D19 的当前状态。旧结果中出现的 `PASS` 仅限对应的资源审计、隔离测试或标准文档，不可外推为 D20 逻辑/可玩/发布通过。

| 历史产物 | 原始范围/已知状态 | 本基线的解释与隔离 |
|---|---|---|
| `D20-S01`：`docs/game/design/common/D20_CHARACTER_RIG_ANIMATION_STANDARD.md`、`game/tools/results/D20/D20-S01/` | `PASS`，只冻结角色/动物表现标准、动作 ID、骨架/LOD/回退约定；没有正式运行时消费者 | 仅是 Presentation 约束。不能证明模型、Avatar、Animator、Prefab、场景或逻辑存在；`LocalDevelopmentReady`/`ReleaseApproved` 不因此改变。 |
| `D20-A01`、`D20-A01-R01`、`D20-A01-R02`：`game/tools/results/D20/D20-A01*/`、`docs/game/design/common/handoffs/D20/D20-A01*` | 玩家视觉/rig/构建/Editor audit；原始 A01 为 `PARTIAL`，候选 `asset_kenney_blocky_character_a_v01` 为 Generic/NoAvatar；R01/R02 是重跑/审计类产物 | 只能说明候选玩家表现接口或构建审计的历史观察；不拥有玩家身份、presence、动作事实，不是 D20 逻辑实现证据。 |
| `D20-A02`、`D20-A02-R01`：`game/tools/results/D20/D20-A02*/`、对应 handoff | A02 原始 `BLOCKED`，因 A01 rig/Avatar 门禁未满足；R01 仅有控制器审计类结果 | 不创建或证明逻辑动作系统；Animator/Override/控制器结果不得被 L04—L06/L28 依赖。 |
| `D20-A03`：`game/tools/results/D20/D20-A03/`、`D20-A03_HANDOFF.md` | NPC 视觉接口/Prefab，12 个候选视觉、3 个独立源模型，Candidate/PARTIAL 口径；有隔离 EditMode 证据，但无正式场景/行为/身份/关系消费者 | 只属于 NPC Presentation 资源观察；不得由外观、职业颜色或 Prefab 推断身份、岗位、阵营、经济、对话或关系。 |
| `D20-A04`：`game/tools/results/D20/D20-A04/`、`D20-A04_HANDOFF.md` | NPC 表现适配/构建/隔离审计，历史为候选/部分完成口径；隔离测试不等于主场景运行 | 只能作为 Presentation 适配历史参考；不解锁 D20 NPC 行为、日程或港口逻辑。 |
| `D20-A05`：`game/tools/results/D20/D20-A05/`、`D20-A05_HANDOFF.md` | 野生/水生视觉样本，8 个 Prefab，历史 `PARTIAL`；隔离测试通过；样本缺口仍存在，Manta 不等于海龟 | 只代表视觉样本/接口审计；不创建物种、数量、种群、捕获、生态、法律、奖励或死亡事实。 |
| `D20-A06`：`game/tools/results/D20/D20-A06/`、`D20-A06_HANDOFF.md` | 宠物/珍稀动物隔离表现，历史 `PASS` 仅限隔离 Presentation 测试；未进入正式场景闭环 | 不解锁 D11PetSystem、RareAnimalCollection、AnimalCare 或 Ecology 的写权限，不是宠物所有权或稀有动物逻辑证据。 |
| `D20-S02`：`game/tools/results/D20/D20-S02/`、`D20-S02_HANDOFF.md` | 历史正式角色/NPC/动物场景集成为 `BLOCKED`；旧阻断包含旧 D19-R01-S02、资源/rig/运行时门禁 | 当前 D19-L48 已通过只说明 D19 纯逻辑前置更新，不能回填 D20-S02 的画面、场景、截图、设备性能或发布证据。S02 仍是历史运行时结果，未来运行时任务需重新执行真实门禁。 |

历史回退（D2 fallback、NPC neutral/FarProxy、动物 Idle/Swim/FarProxy、宠物 unavailable/remote fallback）只属于 Presentation 降级，不能作为 D20 逻辑状态的写者。任何后续任务引用上述目录，必须同时声明“历史、视觉/运行时、非逻辑证据”。

## 8. Runtime Deferred register（本任务不执行）

以下事项明确延期，不计入 D20-L01 blocker，也不能在 D20-L01 或后续纯逻辑结果中写成 PASS：

1. Player/ NPC/动物/宠物/珍稀动物正式模型、骨架、Avatar、Animator、动作覆盖和动画质量；
2. Prefab、Scene、生成器、稳定 3D 锚点、Transform、碰撞、刚体、CharacterController、NavMesh、相机和输入映射；
3. Canvas/UI/音频/粒子/VFX/资源异步加载、GPU/CPU/内存、1080p 多档性能与真实设备；
4. D18 Candidate/IntakeEligible 资源的运行时准入、真实消费者、`LocalDevelopmentReady`、`RuntimeApproved` 和 `ReleaseApproved`；
5. D20-S02 正式场景闭环、固定镜头、PlayMode 截图、存档在场景中的恢复和发布回滚。

本清单的状态是 `runtime_deferred`，不是“逻辑失败”，也不是“运行时通过”。未来运行时任务必须重新取得真实 Unity/设备/场景证据，并沿用本文件的单一写者规则。

## 9. D20-L01 完成判定与交接

本任务的完成判定只检查：

- 所有权矩阵覆盖玩家、NPC、动物/宠物、Presentation 及 D10/D11/D19；
- 公开接口、命令/事件/读模型/保存边界和禁止直写清单可被后续任务直接引用；
- D20-L02—L27 并行、L28—L35 串行、L36—L40 门禁和 D19-L48 前置已明确；
- 历史 D20-A/R/S 产物逐项标记为视觉/运行时隔离证据；
- 没有引入 Unity/网络/LLM/模型/场景/Prefab/Player 实现。

后续任务如发现某项公开接口不存在、字段所有权冲突、旧结果与当前规范冲突，必须先回到本基线登记冲突并将受影响任务标为 `LOGIC_BLOCKED`/`LOGIC_PARTIAL`，不可默默扩权或修改既有所有者。
