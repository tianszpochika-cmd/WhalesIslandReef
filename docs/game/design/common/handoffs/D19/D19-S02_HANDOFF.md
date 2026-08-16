# D19-S02 交接报告

结论：`PARTIAL`

## 交付位置

- 正式场景：`game/Assets/Scenes/InitialHarbor.unity`
- 集成根：`game/Assets/Prefabs/Environment/D19/Integration/D19_SaintMargaret_Root.prefab`
- 稳定 ID：`game/Assets/Prefabs/Environment/D19/Integration/D19_StableInteractionAnchors.prefab`
- 音景/道具/VFX 集成根：`game/Assets/Prefabs/Environment/D19/Integration/D19_AmbienceRoot.prefab`
- 集成材质：`game/Assets/Prefabs/Environment/D19/Integration/Materials/`
- 机器结果：`game/tools/results/D19/D19-S02/`
- 主验收文档：`docs/game/design/common/D19_SAINT_MARGARET_ACCEPTANCE.md`

## 本次修改文件

- `game/Assets/Scenes/InitialHarbor.unity`
- `game/Assets/Editor/D19S02/D19S02IntegrationBuilder.cs`
- `game/Assets/Tests/EditMode/D19S02IntegrationTests.cs`
- `game/Assets/Tests/PlayMode/D19S02ScenePlayModeTests.cs` 及 `.meta`
- `game/Assets/Shaders/D19/D19VegetationWind.shader`
- `game/Assets/Shaders/D19/D19WaterSurface.shader`
- `game/Assets/Shaders/D19/D19WaterFoam.shader`
- `game/Assets/Shaders/D19/D19UnderwaterVolume.shader`
- D19-A06 五个 VFX `.prefab.meta` 的 32 位 GUID 修正，以及 D19 集成 Prefab/材质/结果文件。

没有修改 Systems 强逻辑、contracts、server/web、Registry、经济公式、存档 Schema、InitialHarbor 之外的主场景或正式运行时 Prefab。

## 集成/所有权

集成顺序为 Terrain → Port → Biomes → Water → Lighting/Weather → Ambience。表现层根节点只负责 Prefab 引用、材质兼容、截图相机和稳定锚点；位置、建筑所有权、库存、损伤、开放规则、玩家实体、船状态和数值裁决仍由对应系统写入。开发叠字根 `D19_DevOverlay` 默认 inactive，截图相机默认 disabled。

正式集成规模：594 PrefabInstances、436 Renderers、145 Colliders、66 LODGroups、20 稳定锚点。`LocalDevelopmentReady`/`ReleaseApproved` 均未晋级。

稳定 ID 完整列在主文档，包含出生、港口入口、市场/队列、仓库/队列、酒馆、船坞/修理、主码头、泊位、登船、舵位、出航、返港、保存点和三个 NPC 锚点。兼容节点 `Dock`、`DockDeck`、`Gangplank`、`Beach`、`Sea`、`Main Camera`、`Harbor UI`、`HarborControls` 保留。

## 测试命令与结果

Unity：`C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe`。

1. 集成和固定镜头：`-batchmode -projectPath game -executeMethod WhalesIslandReef.Editor.D19S02.D19S02IntegrationBuilder.BuildAndIntegrateInitialHarbor` → 退出码 0；16 张 1920×1080 截图。
2. 材质审计：`-batchmode -nographics -projectPath game -executeMethod WhalesIslandReef.Editor.D19S02.D19S02IntegrationBuilder.AuditIntegratedMaterials` → 退出码 0；436 Renderer，0 missing material、0 missing shader、0 error shader、0 Standard shader。
3. D19 EditMode：`-runTests -testPlatform editmode -assemblyNames Save.Tests -testFilter D19S02IntegrationTests` → `4 passed / 0 failed`。
4. D19 全部相关 EditMode：`-runTests -testPlatform editmode -assemblyNames Save.Tests -testFilter D19` → `25 passed / 0 failed`。
5. D19 PlayMode：`-runTests -testPlatform playmode -assemblyNames D16.PlayMode.Tests -testFilter D19S02ScenePlayModeTests` → `1 passed / 0 failed`；6/6 碰撞、6/6 NavMesh、Y=0。
6. D16/D17 完整 PlayMode：`-runTests -testPlatform playmode -assemblyNames D16.PlayMode.Tests` → `61 passed / 0 failed / 1 skipped`。跳过项是仅工作场景适用的 D19-A01 probe。
7. 规范检查：`git diff --check` → exit 0；仅有既有 CRLF 转换提示，无 whitespace error。

## 截图与性能

截图目录：`game/tools/results/D19/D19-S02/screenshots/`，16 张：14 个固定镜头 + calm/rain/storm 天气变体。结果 JSON：`D19_S02_INTEGRATION_RESULT.json`。粉色像素复核为 0；没有发现缺材质粉块。

编辑器 1920×1080 `Camera.Render`：平均 20.182 ms、P95 295.704 ms。该采样不是目标设备或 PlayMode 性能结论，不能用来替代目标平台预算。

## 阻断清单 / 已知问题

- 无已知的稳定锚点缺失、NavMesh 锚点失败、固定镜头缺材质或 D19 PlayMode 必现卡死。
- `人工 5—10 分钟路线`：本次未运行，必须由人类控制器补测出生→市场→仓库→酒馆→码头→登船→出航→返港→保存恢复。
- `目标设备 1080p 性能`：本次未运行；需补 CPU/GPU/GC/内存和低/中/高画质记录。
- A02 建筑为低多边形项目自有表现代理，市场固定镜头偏近，后续需美术精度和构图责任任务。
- D18-S03 尚未执行；保留集成 Prefab 和证据供后续资源消费/状态晋级审计。

## 回滚

使用 `game/tools/results/D19/D19-S02/PreintegrationBaseline/InitialHarbor.unity`、同目录 `.meta` 和 `D19_S02_PREINTEGRATION_BASELINE.json` 恢复。优先只移除/禁用 `D19_SaintMargaret_Root` 实例，不删除 A01—A06 工作成果或结果。
