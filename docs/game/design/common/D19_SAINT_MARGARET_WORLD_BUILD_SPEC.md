# D19 圣玛格丽特样板世界搭建规范

> 任务：D19-S01（重新执行 / Attempt 2）
> 日期：2026-08-14
> 状态：**PASS**
> 结论：Attempt 1 因旧版 D18-S02 循环依赖而 BLOCKED；Attempt 2 基于 D18-S02 v2 资源准入验收重新执行。本文件的 Attempt 2 补充和 `assets/d19_saint_margaret_layout_v1.json` 是当前冻结事实；Attempt 1 的阻断内容只作为历史记录保留。

## Attempt 2 当前冻结事实（权威）

Attempt 2 已在不修改 `InitialHarbor`、Prefab、代码、Systems、Registry、存档或资源状态的前提下，通过 D18-S02 v2 前置门。机器可读布局、坐标、锚点、区域责任、资源准入、工作场景、14 个镜头、7 分钟路线和 D19-S02 回滚策略统一记录于 [`d19_saint_margaret_layout_v1.json`](assets/d19_saint_margaret_layout_v1.json)。

- D18-S02 v2：`PASS`；`LocalDevelopmentReady=0`、`ReleaseApproved=0` 不阻断本次布局冻结；
- 世界原点：`SM_PORT_ORIGIN = (0, 0, 0)`；1 Unity 单位 = 1 米；Y-up；+Z 向内陆；-Z 向海与出航；海平面 `Y=0`；布局网格 1 米、锚点网格 0.25 米；
- 样板范围：`X=-128..128m`、`Z=-128..144m`，玩家相机目标可见范围 128m、港口总览 256m、远景 384m；
- 资源只准来自 `d18_prefab_candidate_intake_catalog.json` 的 `intake_eligible=true` 条目，或在 JSON 中明确登记的项目自有表现资产；任何条目都不在 S01 晋级 `LocalDevelopmentReady`；
- `InitialHarbor` 只读基线：24 个 GameObject、25 个 PrefabInstance、7 个场景 MonoBehaviour、NavMeshData 为 `fileID: 0`；这些数字和对象名是保护基线，不是已修改的正式场景；
- A01—A06 的 Work 场景、Prefab/设置/音频目录、共享锚点和禁止写入范围不重叠；只有 D19-S02 的指定人类同步集成者可写正式 `InitialHarbor`；
- Attempt 2 不宣称 Prefab、碰撞、NavMesh、Animator、运行时消费者、PlayMode、截图或 `LocalDevelopmentReady` 已通过；这些由后续任务和 D18-S03 验证。

## 0. Attempt 1 结论与边界（历史记录）

本节保存 Attempt 1 当时的阻断事实。它不覆盖上方“Attempt 2 当前冻结事实”，也不表示当前 D18-S02 v2 或 D19-S01 仍为 BLOCKED。

D19-S01 的硬前置是 D18-S02=PASS。当前权威资料显示：

- [`D18_FREE_ASSET_LIBRARY_ACCEPTANCE.md`](D18_FREE_ASSET_LIBRARY_ACCEPTANCE.md) 的最终结论为 `BLOCKED`；
- [`d18_local_development_ready_catalog.json`](assets/d18_local_development_ready_catalog.json) 的 `local_development_ready_count` 为 `0`，`eligible_entries` 为空；
- D18-S02 机器结果记录 1,857 个候选文件、9 个 Candidate manifest、16 个 ImportVerified manifest，但没有可运行时消费的 LocalDevelopmentReady 候选；
- P0 需要的 Prefab、碰撞、交互锚点、LOD、动画和运行时消费者链仍未闭合，海水/天气候选也未闭合；
- D18-S02 明确不解锁 D19—D22 资源接入。

因此 Attempt 1 当时采取以下安全动作：

1. 只读清点当前 `InitialHarbor` 与代码入口；
2. 记录待冻结字段、现有 ID 线索和不得丢失的功能入口；
3. 保存 14 个与 D18 对齐的验收镜头契约和后续路线，但不写入虚构坐标；
4. 不修改场景、Prefab、运行时代码、Systems、Registry、存档、经济公式或资源目录；
5. 不启动 D19-A01—A06。

只有 D18-S02 重新验收为 PASS，且 P0 资源/运行时消费者/碰撞锚点证据齐全后，才能将本文件的“待冻结”表格转为正式冻结表，并启动 D19-A01—A06。

## 1. Attempt 1 前置门禁历史证据（保留）

| 门禁 | 当前事实 | D19-S01 判定 | 解锁证据 |
|---|---|---|---|
| D18-S02 总结 | `BLOCKED` | 不通过 | D18-S02 主报告、交接报告和机器结果均为 PASS |
| LocalDevelopmentReady | `0` | 不通过 | 至少覆盖 D19 港口 P0 类别，并逐项有导入、Prefab、碰撞/锚点、消费者证据 |
| P0 运行时消费者 | `0` | 不通过 | 候选 manifest 与正式消费者的可追溯引用 |
| 海水/天气候选 | 未闭合 | 不通过 | 海水、浅水、天气表现和保存恢复链的 ImportVerified + LocalDevelopmentReady |
| D16 外部门禁 | 未关闭 | 继续保留 | D16 人工审查、placeholder、性能/发布等原始报告 |

禁止用 `Candidate` 或 `ImportVerified` 代替 `LocalDevelopmentReady`，禁止用当前灰盒对象名代替正式 `content_id`、Prefab 或交互锚点。

## 2. InitialHarbor 只读结构清点

### 2.1 场景与构建登记

| 项目 | 只读观察值 | 解释 |
|---|---|---|
| 场景文件 | `game/Assets/Scenes/InitialHarbor.unity` | 文件存在；本任务没有改动 |
| Bootstrap 场景文件 | `game/Assets/Scenes/Bootstrap.unity` | 文件存在；静态扫描未发现可用于 D19 布局冻结的命名对象清单 |
| Build Settings | `ProjectSettings/EditorBuildSettings.asset` 的 `m_Scenes: []` | 当前工作区没有登记场景条目；是否由外部工具/测试注入需后续确认，不能在本任务修复 |
| InitialHarbor 文件大小 | 只读扫描记录约 130,530 bytes | 仅作基线，不是性能或发布证据 |
| 场景 GameObject | 24 | 静态 YAML 对象计数 |
| PrefabInstance | 25 | 静态 YAML 实例计数；不等同于有效运行时 Prefab 消费者 |
| 场景 MonoBehaviour | 7 | 运行时 Bootstrap 多数通过 `SceneManager.sceneLoaded` 动态创建，不全在场景 YAML 中 |
| NavMeshData | `fileID: 0` | 当前场景没有已绑定的 NavMesh 数据；后续必须由负责任务提供可复核数据 |
| InitialHarbor 截图 | 未找到可直接引用的正式截图 | D18 镜头只能作为待执行验收契约，不能伪造当前画面证据 |

### 2.2 场景根节点和现有层级线索

以下是 YAML/静态代码观察值，不是 D19 的正式空间冻结。`m_Layer` 当前均为 `0`，`m_TagString` 除主相机外均为 `Untagged`，说明正式 Layer/Tag 规范尚未在本场景完成冻结。

```text
InitialHarbor (scene file)
├─ Dock                         local (0, 0, 0)
│  ├─ DockDeck                  local (0, 0.1, -6)
│  ├─ Gangplank                 local (0, 0.5, -4.75)
│  ├─ HandRail                  local (0, 0.55, -7.68)
│  ├─ Bollard                   local (-6, 0.44, -4.3)
│  ├─ Bollard                   local (6, 0.44, -4.3)
│  ├─ Crate / Crate2 / Barrel / Barrel2
│  └─ Pile x 6
├─ Beach                       local (0, -0.32, -26)
├─ Sea                         local (0, -0.02, 0)
├─ Main Camera                 local (2, 5.5, -8.5), tag MainCamera
├─ Directional Light           local (0, 12, -4)
├─ Harbor UI
│  ├─ HarborName
│  ├─ HarborSubtitle
│  └─ HarborControls
└─ other root objects from scene YAML
```

当前根节点/对象的坐标只用于保护现有运行基线，不得在 D19-A01—A06 中直接作为最终布局坐标。D19 正式坐标表必须在门禁通过后重新取样，并同时给出世界单位、轴向、海平面、容差、碰撞边界和回滚版本。

### 2.3 摄像机、Canvas、碰撞与导航

- `Main Camera` 是 `MainCamera`，静态相机组件为透视投影、FOV 60、近裁剪 0.3、远裁剪 500；场景 YAML 中绑定了一个 target fileID，运行时相机控制还由 D2/D5 逻辑接管。摄像机位置和镜头参数在 D19 未冻结。
- `Harbor UI` 为 Screen Space Canvas，`CanvasScaler` 的参考分辨率为 1920×1080；`HarborName`、`HarborSubtitle`、`HarborControls` 是现有静态 UI 入口。运行时还有 D5/D9/D10/D16 动态面板，因此不能由美术任务重建 Canvas。
- 场景 YAML 未提供可视为 D19 固定接口的 NavMeshData；`HarborCollisionController` 在运行时创建 `D6_HarborCollision`、`BeachGround`、`DockGround`、`GangplankGround`、`ShoreBoundary` 和船体边界等对象。它们的所有权属于碰撞/Gameplay 任务，不属于美术替换任务。
- 静态场景对象目前主要是网格/渲染器和装饰物；运行时碰撞、玩家、登船检测、港口服务和 HUD 不能由替换模型单独提供。

### 2.4 运行时 Bootstrap 入口清单

只读扫描确认以下入口以 `InitialHarbor` 为目标，通过 `SceneManager.sceneLoaded` 或运行时查找创建/连接对象：

| 责任域 | 已观察入口 | D19 保护规则 |
|---|---|---|
| 玩家/基础运行 | `D2RuntimeBootstrap`、`D3RuntimeBootstrap`、`D4RuntimeBootstrap`、`D5RuntimeBootstrap` | 保留玩家生成、港口标记、贸易、船坞、生态和主 HUD 接口 |
| D7 控制/存档 | `D7ControlCoordinator`、`D7SaveCoordinator`、D7 角色/甲板/表现入口 | 不更改存档字段、输入动作或船员/船舶所有权 |
| 港口表现/碰撞 | `HarborPresentationController`、`HarborCollisionController`、`HarborTutorialController` | 表现可以替换，碰撞和教学事实不能被视觉假象覆盖 |
| D9 航行/港口 | `D9VerticalSliceRuntimeBootstrap`、`D9FishingRuntimeBootstrap`、`D9PortShipPreparationBootstrap`、`D9VoyageHudController`、`D9AudioPresentationController` | 市场、酒馆、船坞、离港和天气快照保持由所属系统裁决 |
| D10 社交/外观 | `D10TavernSocialBootstrap`、`D10AppearanceRuntimeBootstrap` | UI 只提交请求，不能成为关系/外观数据写者 |
| UI/输入/暂停 | `HarborInteractionStabilizer`、`RuntimeHudCoordinator`、`HarborPauseController`、D16 无障碍/诊断面板 | 不重建 EventSystem，不改变输入意图和关键按钮语义 |
| 性能/观察 | D16 世界性能/运行探针 | 只记录指标，不以降级删除事实、事件或存档 |

## 3. 现有功能入口和迁移保护表

下表是从现有代码和场景静态扫描出的“保护清单”。其中 `stable_id` 是已有领域 ID 时才标出；纯 GameObject 名称明确标为运行时节点名，不能误当成跨版本 ID。D19-S01 阻塞期间不做位置迁移。

| stable_id / 运行时节点 | 类型 | 当前来源 | 美术重建时必须保持 | 当前位置迁移状态 |
|---|---|---|---|---|
| `santa_margarita` | 港口/起始地点 ID | `WorldCatalog`、Save、D3/D9 | 起始港、玩家初始位置和读档地点语义 | **NOT FROZEN**；必须先由 S01 正式布局表确认 |
| `building_saint_marys_dock` | D9 圣玛格丽特泊位 ID | `D9PortShipPreparationSystem` | 船舶泊位检查、靠泊服务和离港前准备 | **NOT FROZEN**；保留 ID，位置待门禁后冻结 |
| `berth_santa_margarita` | 船状态键/泊位事实 | D9 船舶准备系统 | 不丢失泊位事实，不改写经济/船舶裁决 | **NOT FROZEN**；由 D9/D21 负责绑定 |
| `tavern_santa_margarita` / `building:tavern_santa_margarita` | 酒馆 ID/位置引用 | D10 Tavern 系统 | 酒馆进入、夜话、关系和敏感内容跳过入口 | **NOT FROZEN**；没有正式建筑 Prefab 证据 |
| `npc:santa_margarita_dockhand` | NPC 引用 | D10 传闻目录 | 码头工传闻和关系/任务事实 | **NOT FROZEN**；不得由美术改名或删除 |
| `npc:santa_margarita_lookout` | NPC 引用 | D10 传闻目录 | 瞭望/天气传闻入口 | **NOT FROZEN**；不得由美术改名或删除 |
| `npc:santa_margarita_tavern_keeper` | NPC 引用 | D10 任务/传闻目录 | 酒馆服务、传闻、副业任务入口 | **NOT FROZEN**；不得由美术改名或删除 |
| `Dock` | 场景根对象名 | InitialHarbor YAML | 码头下属物和运行时碰撞查找不丢失 | **NOT FROZEN**；只读观察 |
| `DockDeck` | 场景对象名 | InitialHarbor YAML | 甲板可走面、上船接近方向和码头视觉连续性 | **NOT FROZEN**；碰撞由 D6/D17 负责 |
| `Gangplank` / `GangplankGround` | 登船表现/碰撞节点 | 场景 YAML / HarborCollisionController | 玩家上下船过渡与船岸连接 | **NOT FROZEN**；不得用模型替换移除碰撞锚点 |
| `Beach` / `BeachGround` | 沙滩表现/碰撞节点 | 场景 YAML / HarborCollisionController | 出生、登陆、步行和返回港口路线 | **NOT FROZEN**；待 D19 坐标和 D17 验收确认 |
| `Sea` | 海面表现节点 | InitialHarbor YAML | 海面、近岸和离港镜头的表现承载 | **NOT FROZEN**；水体逻辑归环境/航行系统 |
| `D3_Port_santa_margarita` | 运行时海图标记名 | D3 runtime smoke probe/bootstrap | 海图/港口标记不能因场景美术替换消失 | **NOT FROZEN**；动态节点不能作为 Prefab 目标 |
| `D2_PlayerAvatar` | 运行时玩家节点名 | D2/D3 smoke probe | 玩家生成、镜头目标、保存位置和控制权 | **NOT FROZEN**；由 D2/D17 负责 |
| `D9PortServicePage` / `D10TavernSocialPage` | 运行时 UI 页面名 | D9/D10 Bootstrap | 市场、酒馆、船坞、离港和关系 UI 入口 | **NOT FROZEN**；UI 不由美术任务重建 |
| `HarborControls` / `Harbor UI` | 静态 Canvas 节点名 | InitialHarbor YAML | 旧提示层的兼容查找与交互稳定器 | **NOT FROZEN**；禁止删除或静默改名 |

迁移原则：领域 ID 永远保留；运行时节点若必须改名，只能由对应责任系统提供兼容别名和测试，且必须在 D19-S02 集成记录中回滚可验证。任何“为了换模型而改 ID/改存档/改经济公式”的变更均超出本任务范围。

## 4. Attempt 1 待冻结记录（已由 Attempt 2 JSON supersede）

以下字段是 D19-S01 应该冻结但本次因门禁未达成而保持 `TODO` 的合同，不得填入猜测值。

| 类别 | 必须冻结的字段 | 当前状态 |
|---|---|---|
| 世界基准 | 单位定义、Y-up/前向轴、坐标原点、允许误差、版本号 | TODO；不得从灰盒局部坐标推导最终规范 |
| 海岸/水体 | 海平面、潮线、岸线轮廓、浅水边界、进港航道和安全边界 | TODO；当前 `Sea` 位置不是正式海平面证据 |
| 港口 | 码头方向、外锚地、泊位、登船点、市场、仓库、酒馆、船坞、道路 | TODO；无可用 LocalDevelopmentReady 建筑/港口 Prefab |
| 岛屿 | 圣玛格丽特样板范围、地形层、植被区、远景、登陆点 | TODO；不得用未就绪候选资源冻结视觉边界 |
| 系统接口 | Prefab 锚点、Layer/Tag、碰撞层、导航边界、加载分区、摄像机边界 | TODO；当前静态场景 Layer/Tag 不足以作为 D19 标准 |
| 资产 | 只允许 D18 `LocalDevelopmentReady` 资源 ID | TODO；当前清单为空，因此本次实际使用 0 个资源 ID |

## 5. D18 对齐的固定验收镜头契约（镜头 ID 保留，坐标以 Attempt 2 JSON 为准）

镜头名称和验收意图在 Attempt 1 先行登记；Attempt 2 已冻结位置、朝向、天气 preset 和验收目标，正式截图仍由 D19-S02/后续集成任务产生。镜头合同不等于截图或运行时通过。

| # | shot_id | 目标 |
|---:|---|---|
| 1 | `d19_shot_main_menu` | 主菜单进入港口前的清晰度与无障碍入口 |
| 2 | `d19_shot_spawn` | 出生点、角色脚底、海岸方向和 HUD |
| 3 | `d19_shot_market` | 市场建筑、交易入口、道路可读性 |
| 4 | `d19_shot_dock_overview` | 码头整体、泊位方向、岸线与港池 |
| 5 | `d19_shot_tavern_entry` | 酒馆/室内入口、交互锚点和警告入口 |
| 6 | `d19_shot_coastline` | 海岸、浅水、沙滩/岩地过渡 |
| 7 | `d19_shot_boarding` | 登船点、跳板、玩家与船体比例 |
| 8 | `d19_shot_on_ship` | 船上控制、碰撞、甲板锚点 |
| 9 | `d19_shot_departure` | 离港方向、泊位释放、航道安全边界 |
| 10 | `d19_shot_nearshore` | 近海、海水、浅滩和海图反馈 |
| 11 | `d19_shot_weather` | 晴天/雨天/风暴表现与 HUD 可读性 |
| 12 | `d19_shot_return_harbor` | 返港、靠泊、下船和保存前状态 |
| 13 | `d19_shot_settings_accessibility` | 字体、字幕、色盲/低刺激和输入重绑定 |
| 14 | `d19_shot_debug_boundary` | 开发调试层与玩家模式边界对照；不进入玩家默认画面 |

## 6. 5—10 分钟步行/登船路线契约（路线已冻结，尚未运行）

Attempt 1 只定义顺序；Attempt 2 已将 10 个步骤、锚点和 300—600 秒时长冻结到布局 JSON：

```text
出生点
  → 沙滩可走面与海岸观察
  → 市场外部与交易入口
  → 仓库/道路节点
  → 酒馆入口（仅验证入口、警告和跳过，不强制进入路线）
  → 码头总览
  → 泊位/缆桩/跳板
  → 登船与船上控制
  → 短距离离港
  → 近海镜头与返港
  → 靠泊、下船、保存/恢复检查
```

路线 DoD：5—10 分钟内可连续完成；市场、酒馆、泊位、登船和离港入口均有可理解反馈；不出现丢失锚点、穿模、不可退出、重复结算或丢档。Attempt 2 仍未启动 Unity，不能把路线合同写成运行时通过。

## 7. A01—A06 责任边界（Attempt 2 已启用受控开发）

本表为后续拆分入口，当前不表示这些 Agent 已被启动，也不授予越权。

| 责任任务 | 拥有的 Prefab/工作场景 | Layer/Tag/材质责任 | 必须提供的共享锚点 | 本次状态 |
|---|---|---|---|---|
| D19-A01 环境/地形 | `Environment` 工作场景及地形/植被 Prefab | 地形、植被、海岸材质；不得改 gameplay 数值 | 岸线、道路、植被区、远景、浅水边界 | NOT STARTED |
| D19-A02 建筑/港口 | `PortBuildings` 工作场景及市场/仓库/酒馆/船坞/码头 Prefab | 建筑材质、碰撞外壳、入口标识 | building、door、service、tavern、market、shipyard、berth | NOT STARTED |
| D19-A03 角色/NPC/动物 | `CharactersAndAnimals` 工作场景及角色/动物 Prefab | 骨骼/动画材质、角色表现层 | spawn、conversation、work、observation | NOT STARTED |
| D19-A04 船/海上表现 | `ShipsAndSea` 工作场景及船/海面表现 Prefab | 船体/帆/海面材质；航行逻辑不归美术 | ship、helm、gangway、berth、shoreline、channel | NOT STARTED |
| D19-A05 UI/镜头/调试层 | `Presentation` 工作场景；不直接拥有玩法 UI 数据 | Canvas、材质、镜头预设、调试可视化 | shot、hud、warning、accessibility、debug | NOT STARTED |
| D19-A06 集成/验证 | 只拥有集成验证场景/临时副本，不抢主场景写者 | 只检查，不擅自改他人资源 | manifest、anchor audit、route、rollback | NOT STARTED |

在 S02 前，`InitialHarbor` 主场景唯一写者必须是明确指定的人类负责者/同步集成任务；A01—A06 不得并行写入主场景，不得直接改 Registry、存档或 Systems。正式的主场景唯一写者姓名、分支和回滚点必须在 D18-S02 PASS 后由 S02 补入。

## 8. S02 集成顺序与回滚点（Attempt 2 已解锁，正式场景仍待 S02）

解锁后建议严格执行：

1. 锁定 D18-S02 v2 intake 目录与 D19 坐标版本；`LocalDevelopmentReady` 仍可为 0；
2. 建立 D19 集成副本，导入 A01/A02/A03/A04/A05 的 Prefab，不写正式 InitialHarbor；
3. A06 执行静态引用、锚点、碰撞、Layer/Tag、相机和路线检查；
4. 由唯一主场景写者把通过审计的 Prefab 接入 InitialHarbor；
5. 运行 14 镜头、5—10 分钟路线、保存/恢复和 D18 资源审计；
6. 失败时回滚到 `InitialHarbor` 集成前快照，不修改旧存档、不删除失败证据；
7. D19-S02 仅在所有证据齐全后决定 PASS/PARTIAL/BLOCKED。

当前回滚基线：**本次没有产生场景或资源改动，因此工作区现有 InitialHarbor 即为回滚基线**。不得把未验证的候选目录直接复制进正式场景。

## 9. Attempt 2 验收结论

### Attempt 2 已满足

- D18-S02 v2 主报告和交接报告为真实 `PASS`；
- `PrefabCandidateIntake` 目录结构、资源准入和项目自有表现资产缺口明确；不要求 `LocalDevelopmentReady` 非零；
- 正式坐标表、接口锚点、14 个镜头、5—10 分钟路线、A01—A06 写入边界和 S02 回滚策略已冻结；
- InitialHarbor 仍为只读基线；正式构建登记、碰撞/NavMesh/运行时消费者和截图留给 D19-S02/D18-S03。

### 本次验收

| 项目 | 结果 |
|---|---|
| 读取规范与 D18 门禁 | PASS（只读） |
| InitialHarbor 结构清点 | PASS（静态只读；未运行 Unity） |
| 正式布局/坐标冻结 | PASS；以 `d19_saint_margaret_layout_v1.json` 为准 |
| 资源准入 | PASS；仅记录 intake 允许范围，实际接入 0 个资源 |
| A01—A06 受控开发 | UNLOCKED；不得写 InitialHarbor |
| InitialHarbor 修改 | 0 |
| D19-S01 Attempt 2 总结 | **PASS** |
