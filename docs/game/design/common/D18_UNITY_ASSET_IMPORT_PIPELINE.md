# D18-A02 Unity 资产导入与 Prefab 预检管线

任务：`D18-A02`  
日期：2026-08-14  
结论：**PASS**

本管线是 Unity Editor-only 的导入审计和建议工具。它读取 D18-A01 的 manifest/schema 与 Unity 当前导入结果，生成可重复的机器报告和 Markdown 摘要；默认不改导入设置、不改 Registry/`asset_state`、不创建 Prefab、不修改场景、不下载资源。

## 1. A01 接口和目录边界

工具直接消费 `docs/game/design/common/assets/manifests/*.manifest.json`，遵守 D18-A01 `schema_version=1` 的 `manifest_kind`、`pack_id`、来源/许可证、`candidate_files[].path/role/target_id`、`review_status` 和 `import_status` 字段；没有另建第二套资产清单。

实现和配置位于：

```text
game/Assets/Editor/D18ArtPipeline/
├─ D18ArtPipeline.cs
├─ D18ArtPipelineConfig.json
├─ WhalesIslandReef.D18ArtPipeline.Editor.asmdef
└─ Tests/
   ├─ D18ArtPipelineEditModeTests.cs
   ├─ WhalesIslandReef.D18ArtPipeline.Tests.asmdef
   └─ Fixtures/D18ArtPipelineFixture.json
```

`WhalesIslandReef.D18ArtPipeline.Editor.asmdef` 只包含 `Editor` 平台，运行时程序集和 `game/Assets/Scripts/Systems/` 不依赖 UnityEditor。

## 2. 分类规则与支持格式

固定分类为：`Ships`、`Characters`、`Animals`、`Environment`、`Buildings`、`Props`、`Vegetation`、`UI`、`Audio`。分类由资源路径和 manifest role 的稳定关键词确定，并按路径片段匹配，避免 `Buildings` 中的字母序列误命中短关键词 `UI`；最后回退到 Props。

支持的导入检查扩展名：

- 模型：`.fbx`、`.obj`、`.blend`、`.dae`、`.3ds`、`.dxf`、`.ma`、`.mb`、`.max`、`.gltf`、`.glb`；
- 贴图：`.png`、`.jpg`、`.jpeg`、`.tga`、`.tif`、`.tiff`、`.psd`、`.exr`、`.hdr`、`.dds`；
- 音频：`.wav`、`.mp3`、`.ogg`、`.aif`、`.aiff`；
- Prefab：`.prefab`。

不支持的扩展名会生成具体错误，不会被静默跳过。扫描路径必须在项目/仓库范围内，且会拒绝 `Library`、`Temp`、`Logs`；路径会先规范化解析 `..`，避免夹具字符串前缀逃逸到项目外。

## 3. 导入审计字段

### 模型 ModelImporter

每个模型记录模型自身可观测结果，不把所有模型强行统一 scale：

- 单位说明 `1 Unity unit = 1 metre`、模型 bounds min/max、最大尺寸和分类目标尺度参考；
- `globalScale`、文件尺度/轴向转换字段；Unity Y-up/Z-forward 参考；
- 法线、切线、网格压缩、Read/Write；
- 动画开关、动画类型、Avatar 设置、动画 clip 数量；
- Mesh 数量、材质/贴图子资源数量、骨骼/Transform 数量、Avatar 存在性；
- MeshRenderer/SkinnedMeshRenderer、bounds、碰撞、交互锚点、LODGroup、缺失脚本/引用和命名问题；
- 对异常设置生成 `D18Issue` 和建议，例如 Read/Write 开启、无正常线/切线、High 压缩、角色无 Avatar、非角色开启动画、模型名不符合 snake_case。

报告中的目标尺度是后续 Prefab 的参考，不是自动缩放命令；实际水线、脚底、门高、舱室和交互位置由消费者 Prefab/系统明确修正。

### 贴图 TextureImporter

记录贴图类型、sRGB、最大尺寸、压缩、Read/Write、MipMap、实际像素宽高和 UI 分类提示。默认尺寸预算为 2048px；超限只报告 warning，不自动 resize。

### 音频 AudioImporter

记录压缩格式、Load Type、质量、Force To Mono、后台加载、预加载、声道、采样率和时长；短音频使用 Vorbis、PCM 过大等情况只生成建议。

### 材质提取

模型记录材质子资源/贴图数量以及 `materialImportMode`/`materialLocation`；工具不自动提取、重绑或覆盖材质，候选资源仍由后续 Prefab/美术审核接管。

## 4. Prefab 预检

对明确传入的 Prefab 或测试 GameObject 递归检查：

- 至少一个 `MeshRenderer` 或 `SkinnedMeshRenderer`；
- 每个 Renderer 的材质槽非空且无缺失材质；
- Renderer bounds 非零；
- 至少一个 Collider；
- `InteractionAnchor`/`InteractionAnchor_*`/`*_interaction` 锚点；
- `LODGroup`；
- 缺失 MonoBehaviour 脚本；
- 序列化丢失对象引用；
- 根节点稳定 snake_case 命名。

原始模型只报告这些表现/交互缺口并建议“创建 Prefab 后复检”，不会把散模型自动塞进 `InitialHarbor`，也不会从模型推导系统碰撞或交互事实。

## 5. 报告优先和自动修复边界

菜单入口：

- `WhalesIslandReef/D18 Art Pipeline/Run Kenney Candidate Audit`：只读报告；
- `WhalesIslandReef/D18 Art Pipeline/Apply Fixture Suggestions To Selected Asset`：需要显式选择、二次确认，并且资源必须位于 `Assets/Editor/D18ArtPipeline/Tests/Fixtures/`。

自动修复目前只允许测试夹具内的安全贴图尺寸示例；候选模型、骨骼、轴向、材质、Prefab、场景、Registry 和 `asset_state` 永不自动覆盖。未明确选择时返回 blocked；候选目录即使显式选中也返回 blocked。

报告固定写入：

```text
game/tools/results/D18/D18-A02/D18_A02_KENNEY_IMPORT_AUDIT.json
game/tools/results/D18/D18-A02/D18_A02_KENNEY_IMPORT_AUDIT.md
```

JSON 包含 `read_only`、`registry_modified`、`asset_state_modified`、`release_approved_asserted`、`downloaded_assets` 等边界字段；`ReleaseApproved` 永远由工具置为 false。

## 6. Kenney 候选包真实审计

审计读取 D18-A01 已落盘的两个 Kenney manifest，共 7 个候选 FBX：

| 指标 | 真实结果 |
|---|---:|
| manifest | 2 |
| candidate files | 7 |
| models | 7 |
| textures | 0 |
| audio | 0 |
| prefabs | 0 |
| Mesh | 15 |
| animation clips | 27 |
| materials | 7 |
| errors | 0 |
| warnings | 7 |

分类为 Buildings 2、Characters 1、Environment 2、Ships 1、Vegetation 1。当前 7 个 warning 均为原始 Kenney 文件名不符合项目稳定 snake_case 命名，模型仍保留原文件和 GUID；报告建议在后续 Prefab 阶段处理。warning 是候选资产的非阻塞建议，不是导入管线失败；只有错误才会使报告 `BLOCKED`。因此本次管线结果为 `PASS`，同时保留 7 个 warning 和“原始模型尚缺 Prefab 碰撞、锚点、LOD 及人工风格/历史审查”的事实；`ImportVerified` 不被改写成 `ReleaseApproved`。

## 7. 验证命令与当前结果

Unity 6 版本：`6000.0.80f1`；URP 包：`17.0.4`。

在项目根目录执行：

```powershell
& "C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\Administrator\Desktop\game\first01\game" `
  -executeMethod WhalesIslandReef.Editor.D18ArtPipeline.D18ArtPipelineBatch.RunKenneyAudit `
  -logFile "C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-A02\unity_audit_current.log"

& "C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\Administrator\Desktop\game\first01\game" `
  -runTests -testPlatform editmode `
  -testFilter "WhalesIslandReef.Editor.D18ArtPipeline.Tests.D18ArtPipelineEditModeTests" `
  -testResults "C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-A02\D18_A02_EDITMODE_TEST_RESULTS.xml" `
  -logFile "C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-A02\editmode_current.log"

git diff --check
```

本次交付复核：

- D18-A01 manifest/schema、D18-S01 PASS、Unity 6/URP 版本已对齐；
- Kenney Unity 审计已在释放项目锁后真实重跑：`PASS / 2 manifests / 7 files / 0 errors / 7 warnings`；Unity 批处理退出码为 `0`；
- EditMode 测试已真实运行并生成 XML：`6/6 passed, 0 failed`，Unity 测试退出码为 `0`；首次失败的分类和路径逃逸测试已修正为稳定的片段匹配和拒绝 `.`/`..` 原始路径；
- Unity Hub 可保留；本次没有运行中的 Unity/UnityHub 进程，也没有项目锁阻塞；
- 先前结果目录中残留的临时 Unity 测试项目含 `Library`，不属于最小夹具和正式结果，工具本身不会读取该目录；正式最小夹具只在 `Assets/Editor/D18ArtPipeline/Tests/Fixtures`。

## 8. 所有权和后续入口

本任务只写 Editor 工具、测试、D18 配置/结果和文档。它不写运行时、Systems、Registry、asset_state、场景、正式 Prefab 或主 Bootstrap。后续 D18-A03—A06 可以读取本报告作为导入事实，分别接管样板岛、Prefab/碰撞/锚点、角色/动物和场景视觉接入；所有候选状态仍需人工审核，不得从本报告直接进入 ReleaseApproved。
