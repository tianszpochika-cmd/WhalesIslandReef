# VC-S01 静态与编辑器回归交接

日期：2026-09-05  
状态：`PASS_WITHIN_SCOPE`

## 结论

VC-S01 已在限定范围内通过。VC-A01—VC-A16 本轮涉及的源码、Prefab、场景层级和表现边界没有产生新的编译或逻辑回归；R03 沙滩命名冲突已修复，完整 Unity EditMode 从 584/585 提升到 585/585。

这不是 D25—D31 的最终运行时通过。真实 Player 的 WASD、登船/下船、仓库/市场/酒馆入口、异常恢复、天气变体和性能仍必须由后续任务独立取得证据。

## 本次修复

- `game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs`
  - 将 R03 的沙滩视觉节点由 `Beach` 改为 `R03_Beach`。
  - 保留历史场景级 `Beach` 兼容节点，并由旧版清理逻辑保持其 Renderer disabled。
  - 这样 `GameObject.Find("Beach")` 不会误选 R03 新视觉节点，D19S02 兼容性测试和旧适配器的稳定命名边界重新成立。
- `tools/tests/test_d19_r03_visual_builder.py`
  - 增加 R03 沙滩必须使用作用域名称的源码约束，防止同类回归。

## 验证证据

- 全量 Unity EditMode：`585/585 PASS`，`failed=0`，返回码 `0`。
  - [XML 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-S01/VC_S01_ALL_EDITMODE_R2.xml)
  - [运行日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-S01/VC_S01_ALL_EDITMODE_R2.log)
- Python 定向回归：`65/65 PASS`。
- D27 R03 静态审计：NPC=12、野生动物=8、宠物=2、R03 挂接=1。
  - [D27 审计](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-S01/VC_S01_D27_R03_AUDIT.json)
- R03 Prefab/场景审计：287 个渲染器、1 个 R03 整合碰撞体、30,815 个网格顶点；历史 Sea/Beach/Dock 表现节点 disabled。
  - [编辑器审计](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- 场景碰撞审计：192 个总碰撞器；R03 表现根下只保留 1 个 `D19_TerrainRoot`，稳定玩法碰撞仍属于原 D2/D6/D19 owner。
  - [碰撞审计](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_SCENE_COLLIDER_AUDIT.json)
- [结构化结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-S01/VC_S01_RESULT.json)

## 当前边界

- `D31-S05`、`D31-G06` 和 D29/D30 正式运行门禁继续为 `PARTIAL`。
- 本次没有把静态测试、编辑器审计或旧的 Calm 固定镜头升级成最终视觉 PASS。
- 192 个总碰撞器包含远端历史世界碰撞，不能把总数等同于 R03 单一碰撞层；后续真实路线需要继续验证玩家能否正常经过码头、沙滩和建筑入口。

## 下一任务

进入 `VC-G01`：用最新 R03 Prefab 重建当前 Windows Player，验证启动、WASD 行走、转向、登船、下船、仓库/市场/酒馆入口，并把每个入口的成功、未就绪拒绝、重复触发和关闭恢复写入独立结果 JSON。完成后再执行 `VC-G02` 固定镜头与天气变体回归，最后才由 `VC-G03` 同步正式状态。
