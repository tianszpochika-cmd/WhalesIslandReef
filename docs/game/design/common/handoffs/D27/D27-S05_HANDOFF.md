# D27-S05 角色和动物运行时集成交接

## 结论

当前状态：`PARTIAL`。

已完成 D27 运行时初始化时序修复：D27 不再因为场景中的 `ph_crew_male` 锚点先出现就提前结束；它会等待 D2 创建 `D2_PlayerAvatar`，确认玩家有真实 Renderer 后，再补齐 `D2CharacterAnimationController` 并恢复 Renderer 可见性。

## 证据

- Unity fresh compile：Tundra build success。
- Play Mode 日志先记录“玩家表现对象尚未就绪，等待 D2 完成实例化”，随后记录 `角色表现已恢复：playerRenderers=1, restored=0`。
- 静态回归：D25—D31 相关测试 `30/30 PASS`。
- 原始日志：`C:/Users/Administrator/AppData/Local/Unity/Editor/Editor.log`。
- 机器结果：`game/tools/results/D27/D27-S05/D27_S05_RESULT.json`。

## 尚未关闭

- R6/R8 已形成玩家模型和初始化时序的真实 Player 证据，但不足以宣称完整角色/动物视觉路线通过。
- 出生、完整 WASD 路线、对话、交易、登船、宠物/动物观察仍需后续受控运行证据。
- 本任务没有修改 D20 事实、关系、生态、宠物或库存 owner。

## 2026-09-03 独立 Player 复测补充

在 `D31-Standalone-R4-20260903` 中，日志先出现“玩家表现对象尚未就绪，等待 D2 完成实例化”，随后出现 `角色表现已恢复：playerRenderers=1`。这证明 D27 不会再把场景里的 `ph_crew_male` 预置锚点误判成玩家实例；本次证据仅覆盖初始化时序和表现绑定，不代表完整角色/动物路线已经通过。

- [Standalone runtime log](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R4.log)
- [D27 machine result](../../../../game/tools/results/D27/D27-S05/D27_S05_RESULT.json)

## 下游

`D27-G06` 已从“无 Player 证据”推进为 `PARTIAL`，直到正式场景/角色/动物观察证据齐全前不能通过；本交接不改变 D28—D31 的代码层候选实现。

R6 更新：同一 Player 的新游戏路线再次记录等待 D2 后恢复 `playerRenderers=1`，详见 [R6 runtime log](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R6.log) 和 [R6 route report](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R6.md)。本次仍只证明初始化与登船/航行切片，不扩张为完整角色、动物或宠物验收。

R8 更新：在 `InitialHarbor` fresh Player 中再次记录 D2 实例化后 `playerRenderers=1`，并完成仓库打开/关闭后的玩家 HUD 恢复；详见 [R8 runtime log](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R8.log) 和 [R8 route report](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R8.md)。岗位 NPC、动物、宠物、水生生物和完整动画状态仍待逐项运行证据。

R11 更新：最新 Player 仍记录 D2 实例化后 `playerRenderers=1`；D9/D4 页面关闭后玩家主层恢复，未见角色表现相关异常。该证据只补强玩家初始化和页面恢复，不扩张为岗位 NPC、动物、宠物、水生生物或完整动画验收。

## 2026-09-03 R72 编辑器表现集成补充

本轮完成了一个独立的 R03 角色/动物表现 Prefab，并将其挂入 `D19_R03_HarborPresentation.prefab`：

- 12 个岗位 NPC：码头、渔业、船员、补网、交易、酒馆、船坞、守卫和居民表现；
- 8 个动物/鸟类/水生生物：狐狸、牛、鹰、鱼群、鳐鱼、鲨鱼和远景鲸鱼；
- 2 个宠物：猫和狗；
- NPC 使用 `D20NpcReadOnlyPresentationAdapter` 与岗位 `D20NpcPresentationAnchor`，并绑定演示 Animator；
- NPC 的非触发器碰撞体全部禁用，装饰性表现不会阻挡 D2/D6 的移动；
- 没有修改 `InitialHarbor.unity`、Systems、AgentCore、D20/D21 事实 owner、交互裁决或存档。

### R72 证据

- [R03 角色/动物表现构建器](../../../../game/Assets/Editor/D27/D27R03CharacterWildlifePresentationBuilder.cs)
- [角色/动物表现 Prefab](../../../../game/Assets/Prefabs/Characters/D27/R03/D27_R03_CharacterWildlifePresentation.prefab)
- [R03 港口表现 Prefab](../../../../game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab)
- [Editor 构建结果](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_BUILD.json)
- [Editor 静态审计结果](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_AUDIT.json)
- [静态审计脚本](../../../../tools/d27_r03_character_wildlife_audit.py)
- [审计单元测试](../../../../tools/tests/test_d27_r03_character_wildlife_audit.py)

### R72 验证和边界

- Unity 批处理构建退出码 `0`，Tundra 编译成功，`error CS=0`；仅有既有弃用 API 警告；
- D27/D28/D31 表现与交互定向 Python 单元测试 `12/12 PASS`；
- 静态审计：NPC `12/12`、动物/水生生物 `8/8`、宠物 `2/2`、R03 挂接 `1/1`，结果 `PASS`；
- R72 的 `PASS` 级别是 `EDITOR_STATIC_ONLY`，`runtime_proof=false`。它不能替代岗位 NPC、动物、动画和交互目标的 Windows Player 观察证据，也不升级 D27-G06 或 D31 正式视觉门禁。

## 2026-09-03 R73 表现预算登记补充

本轮将 R03 表现 Prefab 中的 22 个角色/动物/宠物包装器登记到 D16 表现预算层：

- 12 个 NPC 使用唯一 `d27.presentation.npc.*` stableId；
- 8 个动物/鸟类/水生生物使用唯一 `d27.presentation.wildlife.*` stableId；
- 2 个宠物使用唯一 `d27.presentation.pet.*` stableId；
- 每个包装器挂载 `D16PresentationRegistration`，登记仅用于 Renderer/Animator/Audio/LOD 表现预算；`activeInteraction=false`，不伪造交互消费者；
- 没有修改 D20 事实、关系、生态、宠物、库存 owner，也没有新增碰撞体或改变正式场景写入边界。

### R73 证据

- [R03 角色/动物表现构建器](../../../../game/Assets/Editor/D27/D27R03CharacterWildlifePresentationBuilder.cs)
- [角色/动物表现 Prefab](../../../../game/Assets/Prefabs/Characters/D27/R03/D27_R03_CharacterWildlifePresentation.prefab)
- [R03 港口表现 Prefab](../../../../game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab)
- [R73 Unity 构建日志](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_BUILD_R73.log)
- [R73 构建结果](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_BUILD.json)
- [R73 静态审计结果](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_AUDIT.json)

### R73 验证和状态边界

- Unity 批处理构建退出码 `0`，Tundra 编译成功，`error CS=0`；
- 编辑器构建统计：NPC `12`、动物/水生生物 `8`、宠物 `2`、D16 表现预算登记 `22`、Renderer `79`、Collider `22`；
- 静态审计：包装器 `12/8/2`、R03 挂接 `1/1`、登记 `22/22`，结果 `PASS`；
- 定向 Python 单元测试 `12/12 PASS`；
- R73 仍是 `PASS_WITHIN_SCOPE / EDITOR_STATIC_ONLY`，`runtime_proof=false`。岗位 NPC、动物、宠物、水生生物、动画、完整输入矩阵、异常恢复和性能 1% low 仍未形成正式 Windows Player 验收，因此 D27-S05 与 D31 正式门禁继续保持 `PARTIAL`。

## 2026-09-05 R75 D19-R03 构建顺序修复补充

### 根因

`D19R03HarborPresentationBuilder.Build` 每次都会重新保存基础 `D19_R03_HarborPresentation.prefab`。此前 D27 只在单独的 D27 Builder 运行时挂接，因此后续 D19 重建会用新 Prefab 覆盖旧资产，造成正式 R03 Prefab 中的 `D27_R03_CharacterWildlifePresentation` 附件数回到 `0`。

### 修复

- D19 基础 Prefab 保存并销毁生成根后，显式调用 `D27R03CharacterWildlifePresentationBuilder.BuildAndAttach()`；随后重新加载 R03 Prefab，再创建 `InitialHarbor` 场景实例。
- D27 的挂接逻辑继续先删除同名旧节点，再实例化 D27 输出 Prefab，因而每次重建都保持恰好一个嵌套 Prefab 实例。
- D27 仍为 presentation-only：NPC/动物/宠物的非 Trigger Collider 禁用，不改变 Systems/AgentCore、D20/D21 事实 owner、交互裁决或存档。

### R75 证据

- [D19→D27 Unity batch rebuild log](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_BUILD_R75.log)
- [D27 build result](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_BUILD.json)
- [D27 static audit](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_AUDIT.json)
- [R03 presentation prefab](../../../../game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab)
- [D27 output prefab](../../../../game/Assets/Prefabs/Characters/D27/R03/D27_R03_CharacterWildlifePresentation.prefab)
- [D27/R03 EditMode XML](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_EDITMODE_R75.xml)
- [D27/R03 EditMode log](../../../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_EDITMODE_R75.log)

### R75 结果与边界

- D27 build report：`status=PASS`、NPC `12`、wildlife `8`、pet `2`、registration `22`、`formal_scene_modified=false`、`systems_modified=false`、`save_data_modified=false`。
- 静态审计：`status=PASS`、wrapper `12/8/2`、`r03_attachment_count=1`、`errors=[]`。
- Unity EditMode 定向回归：`4/4 PASS`、`failed=0`、`skipped=0`；Python D27/R03 回归：`27/27 PASS`。
- 这是 Editor/Prefab 静态范围证据，`runtime_proof=false`；未升级 D27-S05、D27-G06 或 D31 正式运行门禁，真实 Player 角色/动物/动画路线仍待后续验证。
