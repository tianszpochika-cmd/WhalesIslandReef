# D19-A02 圣玛格丽特港口建筑样板

状态：`PASS（D19-A02 工作场景/Prefab 范围）`

本成果建立了一个不接入正式 `InitialHarbor` 的 D19 工作场景：
`game/Assets/Scenes/D19/Work/D19_PortArchitecture_Work.unity`。
它遵循 D19-S01 的 1 Unity 单位 = 1 米、Y-up、+Z 向内陆、-Z 向海面的坐标约定。

## 资源边界

D18 准入清单的 `resource_contracts.allowed_external_manifest_ids_by_task["D19-A02"]` 为空，
因此本任务没有使用任何外部候选包，`external_manifest_ids_used=[]`。所有建筑、道路、码头和配件均为本项目在编辑器中生成的低多边形模块；没有修改 Registry、经济/库存/船坞强逻辑、存档、Bootstrap、地形、水体或 `InitialHarbor`。

## Prefab 目录

| Prefab | 目录 | 用途 | building/category | test_id |
|---|---|---|---|---|
| `D19_PortEntry_Admin` | `Prefabs/Buildings/D19` | 港口入口/管理建筑 | `building_port_entry_santa_margarita` / `management` | `test_d19_a02_port_entry` |
| `D19_Market` | `Prefabs/Buildings/D19` | 市场主体 | `building_market_santa_margarita` / `trade` | `test_d19_a02_market` |
| `D19_Warehouse` | `Prefabs/Buildings/D19` | 仓库主体 | `building_warehouse_santa_margarita` / `trade` | `test_d19_a02_warehouse` |
| `D19_Tavern_Exterior` | `Prefabs/Buildings/D19` | 酒馆外观和入口 | `building_tavern_santa_margarita` / `life/social` | `test_d19_a02_tavern_entry` |
| `D19_Shipyard` | `Prefabs/Buildings/D19` | 船坞/修理区外观 | `building_shipyard_santa_margarita` / `shipyard` | `test_d19_a02_shipyard` |
| `D19_Dock_Primary` | `Prefabs/Buildings/D19` | 主码头、泊位接口 | `building_saint_marys_dock` / `port/dock` | `test_d19_a02_primary_dock` |
| `D19_Dock_Secondary` | `Prefabs/Buildings/D19` | 次级栈桥 | `building_secondary_pier_santa_margarita` / `port/dock` | `test_d19_a02_secondary_pier` |
| `D19_Common_Residence` | `Prefabs/Buildings/D19` | 普通民居 | `building_common_residence` / `life/residence` | `test_d19_a02_residence` |
| `D19_Market_Stall` | `Prefabs/Buildings/D19` | 可重复市场摊位 | `market_stall_santa_margarita` / `trade/market_stall` | `test_d19_a02_market_stall` |
| `D19_Bollard` | `Prefabs/Environment/D19/Port` | 系船柱 | `port_bollard_module` / `port/fitting` | `test_d19_a02_bollard` |
| `D19_Fence_Module` | `Prefabs/Environment/D19/Port` | 围栏/边界 | `port_fence_module` / `port/boundary` | `test_d19_a02_fence` |
| `D19_Stair_Module` | `Prefabs/Environment/D19/Port` | 码头台阶/高差过渡 | `port_stair_module` / `port/access` | `test_d19_a02_stairs` |
| `D19_Port_Walk_Surface` | `Prefabs/Environment/D19/Port` | 行走面/路线模块 | `port_walk_surface_module` / `port/road` | `test_d19_a02_walk_surface` |

每个 Prefab 都有 `D19PortBuildingMarker`、`LODGroup`（LOD0 结构细节 + LOD1 轮廓块），并以 Collider 组成实际入口、门、台阶、栏杆、码头边缘和甲板高度碰撞。LOD0 承担交互和碰撞候选，LOD1 不增加玩法碰撞。材质使用 10 个项目自有共享材质；没有全局 StaticBatching 改写。

## 锚点和写者边界

`D19PortAnchor` 只保存可视化和迁移元数据：`anchorId`、`stableId`、`ownerTask`、`logicOwner`、`migrationPolicy`。它不写库存、价格、交易、修船、船位或存档状态。

工作场景保留并可视化以下稳定接口；服务锚点的 X/Z 与 D19-S01 一致：

| stableId | 场景位置（米） | 逻辑写者/责任任务 |
|---|---:|---|
| `santa_margarita` | `[0, .2, 112]` | `WorldCatalog/D3` / D19-A02 |
| `building:market_santa_margarita` | `[26, .2, 62]` | `TradeEconomySystem` / D19-A02 |
| `market_queue_santa_margarita` | `[12, .2, 58]` | D9 服务流程 / D19-A02 |
| `building:warehouse_santa_margarita` | `[48, .2, 34]` | `Inventory/Cargo systems` / D19-A02 |
| `warehouse_queue_santa_margarita` | `[40, .2, 24]` | D9 仓储流程 / D19-A02 样板扩展 |
| `building:tavern_santa_margarita` | `[-42, .2, 50]` | `D10TavernSocialSystem` / D19-A02 |
| `building:shipyard_santa_margarita` | `[60, .2, 8]` | `ShipyardSystem` / D19-A02 |
| `shipyard_repair_santa_margarita` | `[54, .2, 2]` | `ShipyardSystem` / D19-A02 |
| `building_saint_marys_dock` | `[0, .2, -12]` | `D9PortShipPreparationSystem` / D19-A02 |
| `DockGround` | `[0, .2, -10]` | `HarborCollisionController/D6/D17` / D19-A02 |
| `berth_santa_margarita` | `[0, .1, -30]` | `ShipSystem/D9` / D19-A04（仅视觉接口） |
| `GangplankGround` | `[0, .5, -16.75]` | `HarborCollisionController/D6/D17` / D19-A04（仅视觉接口） |

泊位和登船锚点由 D19-A04 负责事实与运行时装配；本任务只提供可见边界和碰撞候选。市场、仓库、酒馆、船坞和出航请求仍由既有系统裁决，UI 不拥有目录或价格。

## 工作场景检查点

场景包含独立的 `D19_PortBuildingsRoot`、`D19_PortWalkwaysRoot`、`D19_AnchorVisualization` 和 `D19_PreviewRig`，没有散模型自动注入正式场景。道路、摊位、围栏、系船柱、主码头、次级栈桥、船坞、仓库、酒馆街道和居住模块均可单独替换。

自动输出的截图：

- `game/tools/results/D19/D19-A02/screenshots/d19_a02_port_overview.png`
- `game/tools/results/D19/D19-A02/screenshots/d19_a02_market.png`
- `game/tools/results/D19/D19-A02/screenshots/d19_a02_berth.png`
- `game/tools/results/D19/D19-A02/screenshots/d19_a02_shipyard.png`
- `game/tools/results/D19/D19-A02/screenshots/d19_a02_tavern_street.png`
- `game/tools/results/D19/D19-A02/screenshots/d19_a02_warehouse.png`

## 验证入口

在 Unity 6 中执行菜单：

`WhalesIslandReef/D19/Build Port Architecture Work Scene`

生成器位于 `game/Assets/Tests/EditMode/D19PortArchitectureBuild.cs`。验证类为
`game/Assets/Tests/EditMode/D19PortArchitectureTests.cs`，可执行：

`WhalesIslandReef/D19/Run Port Architecture Assertions`

该入口通过 4 组断言：13 个 Prefab 的 Marker/Collider/LOD、材质与锚点元数据、12 个稳定锚点及坐标、工作场景根节点和 InitialHarbor 隔离。机器结果见
`game/tools/results/D19/D19-A02/D19_A02_MACHINE_RESULT.json`，最终断言结果见
`game/tools/results/D19/D19-A02/D19_A02_EDITMODE_ASSERTIONS.json`。

正式运行时晋级仍等待 D19-S02 和 D18-S03；本成果不标记 `LocalDevelopmentReady`。
