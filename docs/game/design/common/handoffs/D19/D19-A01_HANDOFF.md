# D19-A01 交接报告：地形、海岸与导航

## 任务结论

**PARTIAL**。

Work 场景、TerrainRoot、代表碰撞、共享锚点、洞口样板、NavMesh 和测试均已交付；Unity EditMode 7/7、PlayMode 探针 PASS。由于尚未用 D2/D17 真实玩家控制器完成连续 5—10 分钟步行/登船回归，也尚未完成目标设备 CPU/GPU/GC/内存采集，不能把本任务升级为完整 PASS。

## 实际修改与交付文件

### D19-A01 代码/场景/资源

- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Editor\D19A01\D19A01TerrainBuilder.cs`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Scenes\D19\Work\D19_Terrain_Work.unity`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Scenes\D19\Work\D19A01TerrainNavMeshRuntime.cs`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Scenes\D19\Work\D19A01TerrainRuntimeProbe.cs`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Prefabs\Environment\D19\Terrain\` 下的 8 个 D19-A01 Prefab 及 `.meta`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Scenes\D19\Work\Data\D19_SaintMargaret_TerrainData.asset`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Scenes\D19\Work\Data\D19_SaintMargaret_TerrainVisualMesh.asset`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Scenes\D19\Work\Data\D19_SaintMargaret_NavMeshSource.asset`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Scenes\D19\Work\Data\D19_SaintMargaret_NavMesh.asset`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Art\Materials\D19\A01\` 下的四层 TerrainLayer、五个材质及 `.meta`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Tests\EditMode\D19A01TerrainTests.cs`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Tests\PlayMode\D19A01TerrainPlayModeTests.cs`

为让 Unity 的隔离测试程序集能够编译并读取既有 D19 工作模块，另有最小程序集边界/测试引用调整：

- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Prefabs\Buildings\D19\WhalesIslandReef.D19.Port.Runtime.asmdef`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Settings\D19\Lighting\Whalebone.Game.D19.Lighting.Runtime.asmdef`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Prefabs\Environment\D19\Water\WhalesIslandReef.D19.Water.Runtime.asmdef`
- `C:\Users\Administrator\Desktop\game\first01\game\Assets\Tests\EditMode\Save.Tests.asmdef`

### 文档、机器结果与截图

- `C:\Users\Administrator\Desktop\game\first01\docs\game\design\common\D19_TERRAIN_COAST_NAVIGATION.md`
- `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D19\D19-A01\D19_A01_BUILD_RESULT.json`
- `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D19\D19-A01\D19_A01_PLAYMODE_SMOKE.json`
- `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D19\D19-A01\D19_A01_EDITMODE_TESTS_FINAL2.xml`
- `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D19\D19-A01\D19_A01_PERFORMANCE.json`
- `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D19\D19-A01\D19_A01_SCREENSHOTS.json` 与 5 张 PNG
- 本交接文件和 `.workbuddy/memory/2026-08-14.md`

## 冻结参数与接口

- 原点 `SM_PORT_ORIGIN=(0,0,0)`；1 unit=1 m；Y-up；+Z inland；-Z sea/departure；海平面 `Y=0`。
- TerrainRoot transform `(-128,-8,-128)`；尺寸 `[256,32,272] m`；边界 X `[-128,128]`、Z `[-128,144]`。
- Heightmap `257x257`；Alphamap/BaseMap `256x256`；TerrainLayer 4 层。
- Visual mesh 4,485 vertices / 8,704 triangles；无 Collider。TerrainCollider 继续承担地形落地，避免表现层成为第二写者。
- NavMesh source 4,485 vertices / 8,704 triangles；Humanoid agent radius `0.35`、height `1.8`、slope `38°`、climb `0.35`。
- 共享锚点：`anchor_player_recovery(-22,0.2,82)`、`anchor_beach_landing(-58,0.2,-8)`、`anchor_shallow_waterline(-16,0,-48)`、`anchor_vegetation_boundary(-72,0.5,54)`、`anchor_road_clearance(2,0.55,52)`、`anchor_cliff_exclusion(72,6,72)`、`anchor_reef_boundary(-16,-1.5,-66)`、`anchor_channel_safe(0,-2,-96)`。
- 代表碰撞：`D19_ReefCollision`、`D19_CliffExclusion`、`D19_CaveInteriorBlocker`、三向边界和 `D19_DeepSeaBoundary`。
- 关键采样路线：recovery → beach → market → warehouse → tavern → dock；探针报告 6/6 NavMesh sample。

## 资源与状态

只使用 D18 intake-eligible 的三个 manifest：

1. `pack_kenney_nature_kit_d18_a03_v1`；6 个 Kenney Nature Kit 环境模型；
2. `pack_polyhaven_sand_01_diff_jpg_d18_a03_v1`；`sand_01_diff_1k.jpg`；
3. `pack_polyhaven_coral_mud_01_diff_jpg_d18_a03_v1`；`coral_mud_01_diff_1k.jpg`。

没有下载新资源，没有伪造缺少的海水/天气/建筑/角色/船配件候选。`LocalDevelopmentReady=false`、`ReleaseApproved=false`，不能在 A01 交接中晋级。

## 验证命令与结果

Unity 版本：`6000.0.80f1`。

```text
Unity.exe -batchmode -nographics -projectPath C:\Users\Administrator\Desktop\game\first01\game -executeMethod WhalesIslandReef.Editor.D19A01.D19A01TerrainBuilder.BuildTerrainWorkScene -quit
=> exit 0; Work scene built

Unity.exe -batchmode -nographics -projectPath C:\Users\Administrator\Desktop\game\first01\game -runTests -testPlatform editmode -assemblyNames Save.Tests -testFilter D19A01TerrainTests -testResults game/tools/results/D19/D19-A01/D19_A01_EDITMODE_TESTS_FINAL2.xml
=> 7/7 passed

Unity.exe -batchmode -nographics -projectPath C:\Users\Administrator\Desktop\game\first01\game -executeMethod WhalesIslandReef.Editor.D19A01.D19A01TerrainBuilder.RunPlayModeSmoke
=> report PASS; ground 3/3, cave blocker PASS, reef collision PASS, NavMesh 6/6

Unity.exe -batchmode -projectPath C:\Users\Administrator\Desktop\game\first01\game -executeMethod WhalesIslandReef.Editor.D19A01.D19A01TerrainBuilder.CaptureValidationScreenshots
=> 5/5 PNG, 1280x720

git diff --check
=> PASS; only Git line-ending conversion warnings, no whitespace error; no commit/push
```

## 风险、回滚与后续

1. D19-S02/A06 需要把上述采样路线替换为真实 D2/D17 玩家控制器连续移动与登船回归；若失败，回滚 Work Prefab/场景，不修改 InitialHarbor、Systems 或存档。
2. 性能结果目前是静态预算：1.76 MB 左右的 D19-A01 生成文件、4,485 visual/nav vertices、8,704 triangles、8 个根 Prefab。目标设备 CPU/GPU/GC/峰值内存待 D19-S02 采集。
3. D18-S03 负责真实运行时消费者、交互、保存/恢复和截图门禁；A01 不把任何资源状态提升为 LocalDevelopmentReady。
4. 初始场景 SHA-256 保持 `D3D76848BDA2E00F3313F111D7E0DBD2B59E8D306C1A681765B67FD5A591932B`。
