# D18-A02 Unity 资产导入管线交接

- 任务：`D18-A02`
- 日期：2026-08-14
- 结论：**PASS**
- 前置：D18-S01 `PASS`；D18-A01 `PASS`，已提供 `schema_version=1` manifest/schema 和两个 Kenney 真实候选 manifest。

## 1. 修改文件

### 主成果与代码

- `game/Assets/Editor/D18ArtPipeline/D18ArtPipeline.cs`
- `game/Assets/Editor/D18ArtPipeline/D18ArtPipelineConfig.json`
- `game/Assets/Editor/D18ArtPipeline/WhalesIslandReef.D18ArtPipeline.Editor.asmdef`
- `game/Assets/Editor/D18ArtPipeline/Tests/D18ArtPipelineEditModeTests.cs`
- `game/Assets/Editor/D18ArtPipeline/Tests/WhalesIslandReef.D18ArtPipeline.Tests.asmdef`
- `game/Assets/Editor/D18ArtPipeline/Tests/Fixtures/D18ArtPipelineFixture.json`
- `docs/game/design/common/D18_UNITY_ASSET_IMPORT_PIPELINE.md`
- `docs/game/design/common/handoffs/D18/D18-A02_HANDOFF.md`
- `.workbuddy/memory/2026-08-14.md`

### 报告

- `game/tools/results/D18/D18-A02/D18_A02_KENNEY_IMPORT_AUDIT.json`
- `game/tools/results/D18/D18-A02/D18_A02_KENNEY_IMPORT_AUDIT.md`
- `game/tools/results/D18/D18-A02/D18_A02_RESULT.json`
- `game/tools/results/D18/D18-A02/D18_A02_TEST_RESULTS.md`

本轮补充修正：分类关键词改为路径片段匹配，避免 `Buildings` 被短关键词 `ui` 误判；项目路径审计在规范化前拒绝 `.`/`..` 路径段；警告保持可见但不阻断只读导入审计。

未修改 InitialHarbor、主 Bootstrap、正式 Prefab、Registry、`asset_state`、Systems、contracts、server、web 或 Git 历史。

## 2. 支持格式和检查内容

- 模型：FBX/OBJ/Blend/DAE/3DS/DXF/MA/MB/MAX/GLTF/GLB；
- 贴图：PNG/JPG/JPEG/TGA/TIF/TIFF/PSD/EXR/HDR/DDS；
- 音频：WAV/MP3/OGG/AIF/AIFF；
- Prefab：`.prefab`。

导入审计记录单位、模型自身 bounds/尺度、轴向转换、法线、切线、网格压缩、Read/Write、动画类型、骨骼、Avatar、材质提取、贴图类型、sRGB、尺寸、MipMap、音频压缩/Load Type/质量/声道/采样率/时长。模型目标尺度只是报告参考，未强制统一 scale。

九类分类：Ships、Characters、Animals、Environment、Buildings、Props、Vegetation、UI、Audio。

Prefab 预检覆盖 MeshRenderer/SkinnedMeshRenderer、材质、bounds、Collider、InteractionAnchor、LODGroup、缺失脚本、丢失引用和 snake_case 命名。

## 3. 只读/自动修复边界

默认菜单和批处理方法均为 `report_only`：不 `SaveAndReimport`，不改导入器，不改 Registry/`asset_state`，不下载，不把散模型加入 InitialHarbor。自动修复必须满足：

1. 用户显式选中资源；
2. 用户二次确认；
3. 资源路径位于 `Assets/Editor/D18ArtPipeline/Tests/Fixtures/`；
4. 当前安全示例仅允许夹具贴图尺寸建议。

候选包、模型 scale、骨骼、轴向、材质、Prefab、场景和正式资源均不可自动修复。路径解析拒绝项目外路径以及 `Library/Temp/Logs`，并解析 `..` 防止路径逃逸。

## 4. Kenney 真实结果

输入是 D18-A01 的两个 manifest，共 7 个候选 FBX：2 Buildings、1 Character、2 Environment、1 Ship、1 Vegetation；0 textures、0 audio、0 prefabs；15 Mesh、27 animation clips、7 materials；0 errors、7 warnings。warning 是原始 Kenney 文件名不符合本项目 snake_case 命名规则，未重命名、未破坏 `.meta/GUID`。

结果 status 为 `PASS`，保留 7 个非阻塞 warning。报告没有写 `ReleaseApproved`，并保留 `review_status=Candidate`、`import_status=ImportVerified` 和后续 Prefab/人工审查建议。

## 5. Unity 命令和实际测试

Unity：`6000.0.80f1`；URP：`17.0.4`。

```powershell
& "C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Users\Administrator\Desktop\game\first01\game" -executeMethod WhalesIslandReef.Editor.D18ArtPipeline.D18ArtPipelineBatch.RunKenneyAudit -logFile "C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-A02\unity_audit_current.log"

& "C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Users\Administrator\Desktop\game\first01\game" -runTests -testPlatform editmode -testFilter "WhalesIslandReef.Editor.D18ArtPipeline.Tests.D18ArtPipelineEditModeTests" -testResults "C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-A02\D18_A02_EDITMODE_TEST_RESULTS.xml" -logFile "C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-A02\editmode_current.log"

git diff --check
```

实际结果：Unity 导入审计在项目锁释放后退出码 `0`，生成 `PASS / 2 manifests / 7 files / 0 errors / 7 warnings`；EditMode XML 为 `6/6 passed, 0 failed`，退出码 `0`。Unity Hub 保留不影响本地项目批处理；本轮没有运行中的 Unity 或 UnityHub 进程。

此前结果目录曾残留一个约 1.6GB 的临时 `_UnityTestFixtureProject`，其中含 Unity `Library`；它不是最小夹具，也不属于正式报告。正式工具不会扫描它，最小夹具保留在 Editor 目录下。由于当前文件操作策略拒绝递归删除，该临时目录未在本轮强行删除，后续可在确认无 Unity 进程使用时由仓库维护者清理。

## 6. 交接入口

- D18-A03：读取本报告的模型 bounds/分类/导入问题；不能把 ImportVerified 当作 ReleaseApproved；
- D18-A04：使用 Prefab 预检接口补碰撞、锚点、LOD、材质和命名；
- D18-A05/A06：只通过 Prefab/场景消费者接入，不将散模型自动塞入 InitialHarbor；
- 人工审核者：继续决定风格、历史表现、授权和 ReleaseApproved，工具不代签。
