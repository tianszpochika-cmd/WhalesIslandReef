# D19-A03 交接报告：植被生境与表现布置

任务：D19-A03  
前置：D19-S01 Attempt 2 = PASS  
结论：**PASS（D19 植被工作场景/Prefab 范围）**

## 交付物

- [D19 植被工作场景](C:/Users/Administrator/Desktop/game/first01/game/Assets/Scenes/D19/Work/D19_Biomes_Work.unity)
- [D19 BiomeRoot 与六个生境 Prefab](C:/Users/Administrator/Desktop/game/first01/game/Assets/Prefabs/Vegetation/D19/)
- [确定性布局 JSON](C:/Users/Administrator/Desktop/game/first01/game/Assets/Prefabs/Vegetation/D19/Data/D19_Vegetation_Layout.json)
- [风动 Shader](C:/Users/Administrator/Desktop/game/first01/game/Assets/Shaders/D19/D19VegetationWind.shader)
- [D19-A03 植被测试](C:/Users/Administrator/Desktop/game/first01/game/Assets/Tests/EditMode/D19VegetationBiomeTests.cs)
- [机器结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-A03/D19_A03_MACHINE_RESULT.json)
- [导入审计](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-A03/D19_A03_IMPORT_AUDIT.json)
- [穿插审计](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-A03/D19_A03_INTERSECTION_AUDIT.json)
- [性能样本](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-A03/D19_A03_PERFORMANCE_SAMPLE.json)
- [六张截图](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-A03/screenshots/)

## 生境、种子与数量

| 生境 | biome seed | 实例 | 代表内容 |
|---|---:|---:|---|
| beach | 190301 | 10 | 棕榈、灌木、草、扁岩、漂木、树桩 |
| port_disturbed | 190302 | 8 | 小树、灌木、短草、木堆、树桩 |
| road_edge | 190303 | 8 | 成对树冠、灌木岛、草、边缘岩石、枯木 |
| secondary_forest | 190304 | 10 | 三层树冠、灌木下层、草、悬挂藤本、倒木 |
| rocky_slope | 190305 | 8 | 崖岩、立岩、岩隙树、灌木、枯木/树桩 |
| wet_lowland | 190306 | 8 | 湿地树、棕榈、灌木、湿草、藤本、漂木、浅岩 |

全局种子 `1903`；总计 6 个 Prefab、52 个固定实例、52 个 LODGroup、160 个植被 Renderer、7 个材质、8 个内容锚点和 13 个排除体。固定布置没有运行时随机散点。

## 使用资源

来源均为 D18-S02-v2 `intake_eligible=true` 且允许 `D19-A03` 的 Kenney Nature Kit 候选；构建器在写 Prefab 前逐项检查目录准入和文件存在。实际使用的 30 个不同模型为：

`tree_palmTall.fbx`、`tree_palmBend.fbx`、`tree_palmDetailedTall.fbx`、`plant_bushLarge.fbx`、`plant_bushSmall.fbx`、`grass_large.fbx`、`grass_leafsLarge.fbx`、`rock_smallFlatB.fbx`、`log_large.fbx`、`stump_old.fbx`、`tree_small.fbx`、`tree_default.fbx`、`plant_bushDetailed.fbx`、`grass.fbx`、`log_stack.fbx`、`stump_round.fbx`、`tree_default_dark.fbx`、`tree_oak.fbx`、`rock_smallA.fbx`、`tree_tall.fbx`、`tree_small_dark.fbx`、`hanging_moss.fbx`、`log_stackLarge.fbx`、`stump_oldTall.fbx`、`cliff_large_rock.fbx`、`rock_tallF.fbx`、`rock_largeC.fbx`、`tree_pineTallA.fbx`、`stump_square.fbx`、`rock_smallFlatA.fbx`。

稳定内容锚点为：`plant_red_mangrove`、`plant_coast_agave_fiber`、`plant_orchard_guava_v1`、`plant_dry_aloe_medicinal`、`plant_rainforest_mahogany_v1`、`plant_rainforest_pine_resin_v1`、`plant_farm_cassava_mature`。锚点的 owner 记录为 `EcologySystem`；本任务没有修改采集数值、刷新或 Registry。

## 所有权与实现边界

- D19-A03 只写植被工作场景、植被 Prefab、D19 植被材质/Shader、植被布局数据、对应测试和结果。
- EcologySystem/D11 继续唯一裁决植物数量、刷新、生态压力、采集结果和内容事实。
- A01 Terrain、D19 Water、D19 Lighting 在工作场景中只读实例；D19-A03 不写它们的源文件。
- 排除体是预检标记，Collider 全部禁用，不提供玩法碰撞裁决。
- Prefab 序列化只保留模型、原始材质、LOD、稳定锚点命名和排除标记；元数据权威在布局 JSON，避免自定义 MonoBehaviour 在 Unity 同轮构建中形成缺失脚本引用。
- 未写 InitialHarbor、Terrain、Buildings、Water、Systems、Registry、存档或运行时网络/LLM 代码。

## LOD、风动与性能

- LOD 屏幕相对高度：`0.60 / 0.24 / 0.06`；LOD1 是双面交叉代理，LOD2 是可实例化远距无光材质。
- LOD0 保留候选原材质；六个生境拥有独立 `D19_Vegetation_Wind_<biome>.mat`，参数包含风向、速度、振幅、频率、阵风和确定性相位；远距使用 `D19_Vegetation_Distant.mat`。
- 编辑器 RenderTexture 960×540 样本：首帧海滩约 248.23ms（Shader/纹理预热），其余 2.29/1.54/3.54/1.05/1.38ms，稳态均值约 1.96ms；估算静态批次 30。该样本不等同于最终平台预算。

## 穿插与导入检查

- 排除体：13；道路、码头、建筑、互动点、出生点和离港航道严重穿插：0。
- 固定坐标对 AABB 排除体复核：PASS；植被密度/LOD/碰撞测试：PASS。
- 30 个源模型可加载；缺失源模型、缺失材质、缺失 Prefab 引用：0。
- 六张截图均成功写入 `game/tools/results/D19/D19-A03/screenshots/`。
- `D19_A03_MACHINE_RESULT.json`：`status=PASS`、`serious_intersections=0`、`road_dock_building_spawn_channel_intersections=0`。

## 测试与命令结果

1. Unity 生成命令：

   `Unity.exe -batchmode -quit -projectPath game -executeMethod WhalesIslandReef.Editor.D19VegetationAssetBuilder.BuildD19A03 ...`

   结果：退出码 0；日志出现 `[D19-A03] vegetation biome build PASS`。

2. Unity EditMode 定向测试：

   `Unity.exe -batchmode -quit -projectPath game -executeMethod WhalesIslandReef.Tests.EditMode.D19VegetationBatchTestRunner.RunD19A03 ...`

   结果：`5 passed / 0 failed / 0 skipped`；XML：`game/tools/results/D19/D19-A03/D19_A03_EDITMODE_TESTS.xml`。

3. Unity 只读可见性/材质诊断：

   `Unity.exe -batchmode -quit -projectPath game -executeMethod WhalesIslandReef.Editor.D19VegetationAssetBuilder.ValidateD19A03 ...`

   结果：生成 `D19_A03_VISUAL_DIAGNOSTICS.json`，未发现 D19 生成 Prefab 的缺失脚本、缺失材质或缺失源模型。

4. `git diff --check`：PASS；仅有仓库既有换行格式提示，无 whitespace error。没有提交或推送 Git。

构建日志仍会显示仓库其他任务已有的 VFX `.meta` GUID 和 Cinemachine `.asmref` 警告；本任务没有修改这些文件，D19-A03 生成的模型、材质、Prefab 和场景没有对应错误。

## 已知边界与后续

- 当前是低多边形 Candidate/ImportVerified 工作场景，不是正式场景接入；`LocalDevelopmentReady=false`、`ReleaseApproved=false`。
- D19-S02 贴合 A01 正式地形时应再次采样生境根的 Y，并由集成人员复核道路、建筑、码头、玩家出生和航道清晰度。
- D18-S03 负责正式运行时消费者、真实平台性能、风动运行时接管、碰撞/交互接入和资源状态晋级；D19-A03 不提前代行。
