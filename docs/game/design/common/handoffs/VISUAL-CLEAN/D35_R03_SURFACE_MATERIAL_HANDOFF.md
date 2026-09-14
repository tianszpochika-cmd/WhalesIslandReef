# D35 R03 Surface Material and Geometry Handoff

日期：2026-09-05  
批次：R75  
状态：`PASS_WITHIN_SCOPE / EDITOR_STATIC`  
`runtime_proof=false`

## 已完成

- 修复 `D19R03HarborPresentationBuilder.ConfigurePresentationMaterial` 中的陈旧地表清空分支。`R03_Grass`、`R03_GrassLight`、`R03_Earth`、`R03_Sand`、`R03_SandLight` 现在继续走现有 `PresentationBaseMapPath` / `PresentationNormalMapPath` / `LoadNormalMap` 真实资源绑定链；`R03_Plaster` 仍保持唯一 authored-color、无贴图例外。
- 恢复 R64/R66 已审计的 R03 表现几何：28 段泡沫、市场道路与 14 个边缘石、完整港口小道具数量、外围棕榈/灌木/岩石以及 6 个环境候选和 6 个 Pirate Kit 港口候选。几何为表现层，不新增业务或碰撞所有权。
- 保留 D27 自动挂接：D19 Prefab 保存后执行 D27 `BuildAndAttach`，D27 节点先去重再挂接；最新 R03 总根节点含 D27 为 470 Renderer，D27 审计仍为 `r03_attachment_count=1`。D35 的 391/1 基线是 D19 自有表现层审计，明确排除唯一命名的 `D27_R03_CharacterWildlifePresentation` 子树，不删除或覆盖 D27。

## 结果

- 材质：12 总数，11 个纹理材质，1 个 authored-color 灰泥例外；NormalMap 元数据 `6/6` 通过。
- D19 R03 Editor 审计：`391 Renderer / 1 Collider / 39208 mesh vertices`，错误为空；没有放宽原 `391 Renderer / 1 Collider / >=32000 vertices` 审计阈值。
- Unity 重建链：返回码 0，Tundra build success，`D19-R03-A02 Built and placed`，`error CS=0`。
- Unity 定向 EditMode：`WhalesIslandReef.Tests.D19Interaction`，`8/8 PASS`，failed=0，skipped=0。
- Python 定向回归：D19 builder + D35 material audit，`26/26 PASS`。

## 证据

- [R75 材质审计](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R75.json)
- [R75 回归日志](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_REGRESSION_R75.log)
- [R75 Unity 重建](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R75.log)
- [R75 Unity Editor 审计](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R75.log)
- [R75 Unity EditMode 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_TEST_R75.log)
- [R75 Unity EditMode XML](../../../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R75.xml)
- [当前 R03 Editor 结果](../../../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- [D27 挂接审计](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_AUDIT.json)
- [D27 R75 Unity 审计日志](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_D27_AUDIT_R75.log)
- [R03 表现 Prefab](../../../../game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab)

## 未验证与下一步

- 本批次没有执行 standalone Player、物理 OS 输入、固定机位最终画质、运行时光照或性能 1% low；这些不能由 Editor 静态结果替代。
- 下一步执行 `D31-P04/D31-G06` Player visual recheck，重点复查地表贴图可读性、固定机位遮挡/构图、D27 角色动物层和运行时性能。
- `game/tools/results/D31/D31-S05/D31_S05_RESULT.json` 未修改；D31 正式门禁仍为 `PARTIAL`，最新正式修订仍为 `R74`。
