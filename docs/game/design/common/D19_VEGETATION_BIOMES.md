# D19-A03 植被生境与布置包

状态：PASS（表现层工作场景；`LocalDevelopmentReady=false`，`ReleaseApproved=false`）  
工作场景：`game/Assets/Scenes/D19/Work/D19_Biomes_Work.unity`  
布置数据：`game/Assets/Prefabs/Vegetation/D19/Data/D19_Vegetation_Layout.json`

## 范围与边界

D19-A03 只负责 Saint Margaret 样板岛的植被表现、Prefab、材质、固定布置和预检结果。它不写采集数量、刷新、生态压力、库存、奖励或任何运行时裁决；这些继续由 EcologySystem/D11 及其既有边界负责。`D19_BiomeRoot.prefab` 是可由 D19-S02 贴合 A01 地形的表现根，最终运行时接入等待 D18-S03。

本任务未修改 InitialHarbor、A01 地形工作场景、建筑、水体、Systems、Registry 或存档。A01 地形、水体和灯光只在 D19 工作场景中作为只读 Prefab 实例使用。

## 固定规则

- 全局种子：`1903`；生境种子：`190301`—`190306`。
- 坐标：1 Unity 单位=1m，Y-up，`+Z` 向岛内，`-Z` 向水面/出发方向。
- 布置为显式数组，不使用运行时随机散点；模型位置、比例、旋转和内容锚点都记录在 JSON。
- 每个模型实例拥有一个 `LODGroup`：屏幕相对高度 `0.60 / 0.24 / 0.06`，LOD1 为双面交叉代理，LOD2 为远距无光代理；工作场景截图时临时强制 LOD0，截图完成后恢复自动 LOD。
- LOD0 保留准入模型原材质；LOD1 使用带 `_WindDirection`、`_WindSpeed`、`_WindAmplitude`、`_WindFrequency`、`_WindGust`、`_WindPhase` 参数的 D19 风动材质；LOD2 使用可实例化的远距材质。
- 颜色和风幅按生境使用独立材质，实例保持固定比例和明确的高度层级，不通过缩放同一模型伪造新资产。
- 13 个排除体覆盖出生/恢复点、建筑/服务区、码头/泊位、航道、道路核心、滩头登陆点和两个互动点。排除标记只用于布置预检，BoxCollider 保持禁用，不向玩法提供碰撞裁决。

## 六类代表生境

| biome_id | habitat | seed | 实例 | 视觉组合 | 稳定采集锚点 |
|---|---|---:|---:|---|---|
| `beach` | 海滩 | 190301 | 10 | 棕榈、低灌木、草、扁岩、漂木、树桩 | `plant_red_mangrove`、`plant_coast_agave_fiber` |
| `port_disturbed` | 港口扰动区 | 190302 | 8 | 小树、矮灌木、短草、木堆、树桩 | `plant_orchard_guava_v1`、`plant_dry_aloe_medicinal` |
| `road_edge` | 道路边缘/次生林边 | 190303 | 8 | 成对树冠、灌木岛、低草、边缘岩石、枯木 | 无数值写入锚点 |
| `secondary_forest` | 次生林 | 190304 | 10 | 三层树冠、灌木下层、草毯、悬挂藤本、倒木 | `plant_rainforest_mahogany_v1`、`plant_rainforest_pine_resin_v1` |
| `rocky_slope` | 岩坡/崖地 | 190305 | 8 | 崖岩、立岩、岩隙小树、灌木、枯木/树桩 | 无数值写入锚点 |
| `wet_lowland` | 湿润低地 | 190306 | 8 | 湿地树、棕榈、灌木、湿草、藤本、漂木、浅岩 | `plant_red_mangrove`、`plant_farm_cassava_mature` |

总计：6 个生境 Prefab、52 个固定实例、52 个 LODGroup、160 个 Renderer、8 个稳定内容锚点、13 个排除体；工作场景估算静态批次 30。

## 内容锚点

锚点采用稳定 GameObject 名称 `D19_CollectAnchor_<content_id>`，并在布局 JSON 的 `content_anchors` 中记录 `anchor_id`、`content_id`、`plant_id`、`owner_system=EcologySystem` 和路线可见性。D19-A03 不改变 D11 的采集数值或内容注册；EcologySystem 仍是数值和结果唯一写者。

## 来源与准入

模型全部来自 D18-S02-v2 通过的 `game/Assets/Art/Candidates/Environment/KenneyNatureKit/Models/`，构建器在写入前读取 `docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json`，逐文件检查 `intake_eligible=true`、`allowed_task_scope` 包含 `D19-A03` 和文件存在。实际使用 30 个不同模型，覆盖棕榈、阔叶树、灌木、草、藤本/悬挂苔、枯木、树桩、岩石和崖岩；未复制或缩放同一模型冒充新资产。

## 预检与已知边界

- 布置预检：13 个排除体内实例数为 0；道路/码头/建筑/出生点/航道严重穿插为 0。
- 导入预检：30 个源模型可加载，160 个 Renderer 有材质，7 个 D19 材质可加载，无 D19 生成 Prefab 缺失脚本引用。
- 性能样本：首张海滩截图包含一次性 Shader/纹理预热，约 252.53ms；其余五张约 1.17—3.46ms，平均约 1.82ms。该数据是编辑器 RenderTexture 样本，不是最终平台预算结论。
- 视觉包仍是低多边形 Candidate/ImportVerified 表现块；正式运行时材质、风动消费者、批处理和平台性能需在 D18-S03/D19 后续验收中复测。
- A01 地形贴合在工作场景使用 Terrain 采样调整生境根的 Y；Prefab 本身保留设计坐标，S02 接入时应再次以正式地形采样校正。

## 交付索引

- Prefab：`game/Assets/Prefabs/Vegetation/D19/`
- Shader：`game/Assets/Shaders/D19/D19VegetationWind.shader`
- 测试：`game/Assets/Tests/EditMode/D19VegetationBiomeTests.cs`
- 机器结果：`game/tools/results/D19/D19-A03/D19_A03_MACHINE_RESULT.json`
- 截图：`game/tools/results/D19/D19-A03/screenshots/`
- 交接：`docs/game/design/common/handoffs/D19/D19-A03_HANDOFF.md`
