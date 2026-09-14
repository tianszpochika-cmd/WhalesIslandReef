# VC-A02 地形/码头/跳板碰撞对齐交接

状态：`PASS_WITHIN_SCOPE`

## 已完成

- R03 `D19_TerrainRoot` 不再保存宽大的 BoxCollider；生成器现在把可见 `D19_R03_IslandSurface` 的同一份网格写入 `MeshCollider`。
- `D19_TerrainRoot`、可见岛面和运行时碰撞使用同一局部原点 `(0,0,0)`。
- `HarborCollisionController` 在运行时再次同步 TerrainRoot 与岛面 transform，兼容旧 Prefab override，且不新增移动/导航裁决权。
- 码头和跳板仍由 `DockGround`、`GangplankGround` 根据 R03 的 `Deck`/`Ramp` Renderer bounds 创建，旧场景 Dock 道具碰撞继续停用。

## 证据

- [VC-A02 结构化结果](../../../../game/tools/results/VISUAL-CLEAN/VC-A02/VC_A02_RESULT.json)
- [Python 回归结果](../../../../game/tools/results/VISUAL-CLEAN/VC-A02/VC_A02_PYTHON_TESTS.md)
- [Unity 全量 EditMode XML](../../../../game/tools/results/VISUAL-CLEAN/VC-A02/VC_A02_EDITMODE_R01.xml)
- [R03 编辑器审计](../../../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- [场景碰撞审计](../../../../game/tools/results/D19/D19-R03-A01/D19_R03_A01_SCENE_COLLIDER_AUDIT.json)
- [MeshCollider 重建日志](../../../../game/tools/results/VISUAL-CLEAN/VC-A02_REBUILD_R02_MESH_COLLIDER.log)

Unity 全量 EditMode 最新结果为 `586/586 PASS`；Python 定向总回归为 `72/72 PASS`。

## 边界与下一步

本任务只完成代码、Prefab、编辑器审计和逻辑测试范围；不能替代真实 Player 路线。下一任务仍是 `VC-G01`，需要在最新 Player 中验证 WASD、转向、登船/下船以及仓库、市场、酒馆入口。当前 CUA 没有返回可操作的原生 Windows 窗口，因此这部分不能在本轮伪造为 PASS；`VC-G02` 继续锁定。
