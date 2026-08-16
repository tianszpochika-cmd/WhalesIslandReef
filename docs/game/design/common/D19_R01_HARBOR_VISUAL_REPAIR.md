# D19-R01-B 圣玛格丽特港正式场景视觉修复

状态：**PARTIAL**（2026-08-15）

本轮把 InitialHarbor 从旧的白/薄荷绿大平面和临时视觉层，推进到可回滚的 R01-B Integration 视觉层。它不是 D19 最终验收，也不替代 D19-R01-S02 的真实玩家路线与 1080p 性能门禁。

## 1. 证据边界与历史保留

- D18-S02-v2 / D18-S02-R01 准入结论保持 PASS；外部资源只从最新 `d18_prefab_candidate_intake_catalog.json` 的 `intake_eligible=true` 条目读取。
- `LocalDevelopmentReady=0`、`ReleaseApproved=0`，本轮没有修改任何资源状态。
- D19-R01-A 的真实玩家证据保持原状：338 秒（5 分 38 秒）真实键鼠路线完成出生、移动、市场和保存检查点，但 NPC 对话、登船、离港视觉转场、返港泊位未完成；真实 Player 性能没有完整的 1920×1080/GPU/GC/1% low 证据。
- 历史 D19-S02-R01 的 PARTIAL 报告和快照未删除。

## 2. 回滚基线

| 项目 | 修复前 | 修复后 |
|---|---:|---:|
| InitialHarbor 字节 | 133,143 | 165,574 |
| InitialHarbor SHA-256 | `3E081A48B4D7B53E9084308B86587170527E006C637EC0B280E074449C8B02AA` | `4C45779C7B5A020583E9F11F9F8F61642C12F1EC3920E8001F4752EF626D7E9D` |
| 正式新增视觉根 | 无 R01-B 根 | `D19_R01_B_HarborVisualRepair` Prefab 实例 |
| 稳定锚点 | 17 | 17，保持 |

回滚文件：[D19_R01_B_ROLLBACK_INITIALHARBOR.unity](/C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R01-B/D19_R01_B_ROLLBACK_INITIALHARBOR.unity)。回滚时先关闭 Unity 且不保存，再替换场景文件和配套 `.meta`，不得删除原始 D19 兼容节点。

## 3. 本轮实际修改

允许范围内的实际修改：

- `game/Assets/Scenes/InitialHarbor.unity`：只加入 R01-B Integration Prefab 实例，并隐藏旧视觉渲染器；出生、市场、仓库、酒馆、船坞、码头、泊位、登船、出航、返港、保存点稳定 ID 保持。
- `game/Assets/Prefabs/Environment/D19/Integration/R01/D19_R01_B_HarborVisualRepair.prefab`：367 个层级节点、290 个 MeshRenderer；视觉层不带玩法碰撞。重跑时清理了旧重复实例，正式场景最终只保留 1 个 Integration Prefab 实例。
- 项目自有表现：28×30 起伏地表网格、草/沙/岩色块、土路、码头木板/立柱/绳索/旗帜、岸线沙带与暗礁带、市场/仓库/酒馆/船坞四个带屋顶、门窗、遮阳和招牌构件的低多边形建筑。
- 集成层只读引用准入候选：69 个候选模型实例；隐藏旧视觉层渲染器 121 个。候选模型碰撞关闭，碰撞/导航仍由既有 D19 地形和港口系统负责。
- 新增 R01-B EditMode 测试，覆盖 Prefab 序列化、材质/Shader、Prefab 实例和稳定交互锚点。

未修改：Systems、AgentCore、D20/D22、Registry、存档、经济公式、contracts、server、web、主 Canvas、D19-A01—A06 工作场景/源 Prefab、资源状态和构建设置。

## 4. 真实资源类别

外部准入候选类别与代表 content ID：

- Environment：`env_kenney_nature_tree_palm_detailed_tall`、`env_kenney_nature_tree_palm_bend`、`env_kenney_nature_plant_bush_detailed`、`env_kenney_nature_grass_large`、`env_kenney_nature_rock_large_a`、`env_kenney_nature_cliff_large_rock`、`env_kenney_nature_cliff_cave_rock`、`env_kenney_nature_path_stone`、`env_kenney_nature_platform_beach`。
- Building：`d18a05_building_wall_window_square_detailed`、`d18a05_building_wall_window_round_detailed`、`d18a05_building_roof_flat_patch_large`、`d18a05_building_door_rotate_square_b`、`d18a05_building_stairs_open_short`。
- Dock/port props：`d18a05_pirate_structure_platform`、`d18a05_pirate_platform_planks`、`d18a05_pirate_flag_pirate`、`d18a05_pirate_barrel`、`d18a05_pirate_crate`。

完整路径、资源记录和构建统计见：[D19_R01_B_BUILD_RESULT_V11.json](/C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R01-B/D19_R01_B_BUILD_RESULT_V11.json) 与 [D19_R01_B_MACHINE_RESULT.json](/C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R01-B/D19_R01_B_MACHINE_RESULT.json)。

## 5. 14 个固定镜头

14 个截图均真实生成，尺寸均为 1920×1080，模式为 Unity Editor Camera.Render 视觉回归；它们不是 Player 路线证据。人工检查结果如下：

| 镜头 | 结果 | 主要观察 |
|---|---|---|
| `d19_shot_main_menu` | PARTIAL | 地表和建筑可读，但开阔区域稀疏 |
| `d19_shot_spawn` | PARTIAL | 深色地表、棕榈和建筑存在，前景仍宽且低细节 |
| `d19_shot_market` | PARTIAL | 屋顶、墙、遮阳和开口存在，近景比例仍厚重 |
| `d19_shot_dock_overview` | PARTIAL | 码头、栏杆、立柱、棕榈、货物和水体可读；准入木板偏亮 |
| `d19_shot_tavern_entry` | PARTIAL | 门、窗、百叶、遮阳和山墙屋顶可见，墙体仍偏块状 |
| `d19_shot_coastline` | PARTIAL | 深色岸体、暖色岸线和水边清晰，远景层次不足 |
| `d19_shot_boarding` | PARTIAL | 登船面和码头结构可见，地面仍平 |
| `d19_shot_on_ship` | PARTIAL | 港口背景、建筑和道具可见，不证明完整船体表现 |
| `d19_shot_departure` | PARTIAL | 海水/天空/礁石无粉色错误，但近海较空 |
| `d19_shot_nearshore` | PARTIAL | 水体与礁石带存在，近海细节不足 |
| `d19_shot_weather` | PARTIAL | 港口、水、天空和光照存在，天气表现仍有限 |
| `d19_shot_return_harbor` | PARTIAL | 港口、建筑、道路、码头和水体可见，低多边形地表仍明显 |
| `d19_shot_settings_accessibility` | 回归专用 | R01-B 不重构 D22 UI，不能作为 UI 通过证据 |
| `d19_shot_debug_boundary` | 调试专用 | 只证明边界覆盖，不作为玩家视图通过证据 |

截图目录：[game/tools/results/D19/D19-R01-B/screenshots](/C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R01-B/screenshots/)。旧截图与新截图均保留；旧证据见 `game/tools/results/D19/D19-R01-A/screenshots/` 和历史 D19-S02-R01 结果目录。

视觉 QA 机器结果：[D19_R01_B_VISUAL_QA.json](/C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R01-B/D19_R01_B_VISUAL_QA.json)。没有观察到粉色/错误 Shader 或大面积纯白/薄荷绿占位面；仍有宽阔多边形地表、稀疏近海和低多边形建筑比例债务，因此视觉门禁为 PARTIAL。

## 6. 路线、性能和回归

### 真实玩家路线

本轮没有重新执行真实 Player 路线，不能把截图回归误称为路线通过。权威上一轮记录为 338 秒/5 分 38 秒，9 个路线截图：出生、移动、市场交易、离港事件、F5 保存可见；NPC 对话、登船、离港视觉转场、返港/泊位仍失败或无反馈。证据：[D19_R01_A_EVIDENCE.json](/C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R01-A/D19_R01_A_EVIDENCE.json)。

### 1080p 性能

本轮没有采集新的真实 Player Low/Medium/High 1920×1080、1% low、GPU、GC 和帧时间数据。编辑器 Camera.Render 只用于固定镜头，不能作为性能门禁。D19-R01-S02 必须在真实 Player 中重新采集。

### 已运行回归

隔离 Unity 工程中运行：`Save.Tests` 的 `D19R01BVisualRepairTests`，3/3 通过（Prefab 367 节点/290 渲染器、材质/Shader、正式场景单一 Prefab 实例和 17 个稳定锚点）。结果：[D19_R01_B_EDITMODE_RESULTS_V5.xml](/C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R01-B/D19_R01_B_EDITMODE_RESULTS_V5.xml)。

主工程编译仍被既有 D20 测试错误阻断：`D20WildlifePresentationTests.cs` 引用不存在的 `WhalesIslandReef.Presentation`；本轮未修改 D20 文件绕过该问题。原 D19/D17 回归证据保持历史分类，不重新冒充本轮运行结果。

## 7. 未解决问题与后续责任

1. D19-R01-S02：真实玩家连续路线必须完成出生 → 市场 → 仓库 → 酒馆 → 码头 → 登船 → 出航 → 返港 → 保存 → 重载；所有步骤需要输入、反馈和截图。
2. D19-R01-S02：真实 Player 1920×1080 三档 warm-up/采样，补齐 FPS、1% low、CPU/GPU、GC、内存和设备信息。
3. D19 后续视觉任务：继续处理宽阔多边形地表、建筑比例/细节和近海稀疏问题；不得使用未准入资源或提升状态机状态。
4. D20/D22：角色、船体和 HUD/可访问性继续由其拥有任务负责；R01-B 没有改主 UI 或角色/船系统。
5. 主工程 D20 编译阻断需要由 D20 所属任务修复或拆分测试程序集；R01-B 不越权修复。
6. D18-S03：正式运行时消费者、碰撞/锚点/LOD/Animator/保存恢复证据仍未完成，不能晋级任何资源到 LocalDevelopmentReady。

## 8. 结论

R01-B 的正式场景视觉层和可回滚 Integration 产物已真实生成，资源来源和稳定锚点边界可复核；但视觉品质仍有明显低多边形/灰盒债务，真实玩家路线和 1080p 性能门禁未在本轮完成。因此结论为 **PARTIAL**，不得标记 D19 最终完成、D18-S03 通过或 Steam Ready。
