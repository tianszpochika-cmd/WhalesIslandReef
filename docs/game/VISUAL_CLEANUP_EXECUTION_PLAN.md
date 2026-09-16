# InitialHarbor 视觉与空间清理执行计划

更新时间：2026-09-06
目标：先把“能启动但画面杂乱、空间错位、输入/碰撞互相干扰”的首屏问题收口，再重新进行 D25—D31 的正式运行验收。

## 使用方式

> 2026-09-08 执行入口更新：剩余诊断、修复和质量验收按 [LOCAL_QUALITY_REMAINING_PLAN.md](LOCAL_QUALITY_REMAINING_PLAN.md) 的 Q00—Q35 小任务执行。本文件历史结果保留；原 VC-G01/G02/G03 的正式证据门槛继续有效，Q 卡完成不自动升级原状态。

本文件是当前视觉清理批次的任务台账，不替代 D25—D31 正式验收。每个任务完成后必须同时更新：

1. `game/tools/results/VISUAL-CLEAN/<任务编号>/<任务编号>_RESULT.json`
2. `docs/game/design/common/handoffs/VISUAL-CLEAN/<任务编号>_HANDOFF.md`
3. `.workbuddy/memory/YYYY-MM-DD.md`（本次为 `.workbuddy/memory/2026-09-06.md`）

`PASS_WITHIN_SCOPE` 只表示该任务的代码/静态范围已经完成；前置自动化门禁也只能证明其明确覆盖的运行范围。原生 Windows Player、真实物理键盘和人工画面观察是最后一道门禁，不能被截图、合成输入、脚本 `SendInput` 或 readiness 代替。

## 总顺序

```text
VC-S00 基线登记（已完成）
  ↓
VC-A01 场景坐标与根节点归一化
  ↓
VC-A02 地形/码头/跳板碰撞对齐
  ↓
VC-A03 岸线、水面与水生生物层级修正
  ↓
VC-A04 输入单一归属（并行代码任务，已完成）
VC-A05 角色动画稳定（并行代码任务，已完成）
  ↓
VC-A06 初始镜头与遮挡收口
  ↓
VC-A07 建筑、道具、植被空间去重与可读性
  ↓
VC-A08 HUD 分区与模态输入边界
  ↓
VC-A16 首屏岸线、沙滩、道路与泡沫几何修复
  ↓
VC-S01 静态/编辑器回归
  ↓
VC-G01-P0 验收契约、证据格式和状态冻结
  ↓
VC-G01-P1 源码输入归属与 UI 模态边界审计
  ↓
VC-G01-P2 PlayMode 输入/UI 集成门禁
  ↓
VC-G01-P3 PlayMode 场景、地形碰撞与交互回归
  ↓
VC-G01-P4 构建、启动、日志与性能采样烟测
  ↓
VC-G01-NATIVE 最后：standalone Player、真实 OS 输入、人工视觉
  ↓
VC-G02 固定镜头与天气变体回归
  ↓
VC-G03 D25—D31 正式状态同步
```

VC-A04 与 VC-A05 的写入范围互不重叠，可以并行；其余正式场景、Prefab、碰撞和 Player 任务按顺序执行，避免多人同时改同一场景。

## 任务定义与成功标准

| 编号 | 任务 | 主要写入范围 | 当前状态 |
|---|---|---|---|
| VC-S00 | 登记 R57 Player 基线、可见问题和证据路径 | 结果/交接/记忆 | PASS_WITHIN_SCOPE |
| VC-A01 | 统一 R03 根节点世界 Y 与 D6 路线坐标；清除旧的 0.58 抬升 | `D19R03HarborPresentationBuilder`、`InitialHarbor`、R03 runtime bridge | PASS_WITHIN_SCOPE；仍需 VC-S01 汇总回归 |
| VC-A02 | 用 R03 岛面 MeshCollider 作为地形；按真实 Deck/Ramp bounds 建立码头/跳板；停用旧宽盒和隐藏 Dock 道具碰撞 | `HarborCollisionController` | PASS_WITHIN_SCOPE；真实路线仍由 VC-G01 验证 |
| VC-A03 | 让泡沫是薄的岸线装饰而不是漂浮方块；让鱼/鲨/鳐/鲸位于水面下 | R03 builder、D27 wildlife builder | PASS_WITHIN_SCOPE；泡沫已在 VC-A16 再校准 |
| VC-A04 | 移动、冲刺、跳跃只消费一个输入 provider；模态打开时归零 | D2 input + tests | PASS_WITHIN_SCOPE；缺 Unity 编译证据 |
| VC-A05 | Animator/程序化姿态互斥；移除 visualRoot bob/lean；拒绝错误骨骼绑定 | D2 animation + tests | PASS_WITHIN_SCOPE |
| VC-A06 | InitialHarbor 首帧拉开到可读距离；角色、码头、岸线和首个地标同框；镜头不贴墙 | `CameraFollow`、镜头配置 | PASS_WITHIN_SCOPE；仍需 VC-G02 视觉验收 |
| VC-A07 | 按建筑 footprint 与路线重新检查遮挡；去掉会穿模/悬空/重复堆叠的表现节点；不改变玩法 ID | R03 builder/presentation only | PARTIAL；已具备 RERUN21 fixed-shot 证据 |
| VC-A08 | 顶部状态、左侧提示、右侧操作、底部路线分区；模态只接收自己的快捷键 | UI coordinator / D29 HUD | PASS_WITHIN_SCOPE；真实负向矩阵仍在 VC-G01/VC-G03 |
| VC-A16 | 修复旧版名字清理误隐藏 R03 Beach；沙地按边缘环带生成；沙滩/道路贴地；泡沫固定水线 | R03 builder、固定镜头探针、源码测试 | PARTIAL；限定范围完成，最终视觉/性能门禁未完成 |
| VC-S01 | Unity 编译、定向 EditMode、静态场景审计、Prefab 层级审计 | 测试与结果 | PASS_WITHIN_SCOPE |
| VC-G01-P0 | 冻结 VC-G01 验收矩阵、证据字段、PASS 边界与 native-last 顺序 | 计划/结果/交接 | PASS_WITHIN_SCOPE |
| VC-G01-P1 | 验证主 HUD 不拥有模态导航；仅仓库/临时页拥有 D16 焦点；W/S 不得导航按钮 | D29、D16、静态审计 | PASS_WITHIN_SCOPE；定向 PlayMode 已通过 |
| VC-G01-P2 | 用 InputTestFixture 验证 W/S、主 HUD selection、Esc/诊断焦点和单一 HUD 回归 | PlayMode 测试 | PASS_WITHIN_SCOPE；当前全量 70/71 通过、0 失败、1 项明确 work-scene 条件跳过 |
| VC-G01-P3 | 验证 R03 岛面、路线锚点、建筑/码头碰撞和成功/拒绝交互 | PlayMode 场景/物理 | PASS_WITHIN_SCOPE；当前 R03 活动路线 6/6 碰撞、6/6 NavMesh；旧灰盒坐标诊断 1/6 保留 |
| VC-G01-P4 | 验证非开发版构建、启动、日志、D16 连续采样与性能报告 | Build/PlayMode/结果 JSON | PASS_WITHIN_SCOPE；本机渲染态 234/100，Player RERUN49 构建/启动通过；不代表目标设备或人工视觉性能 |
| VC-G01 | 汇总 P0—P4 后再执行最后的原生 Player 路线和人工视觉验收 | Player 证据 | IN_PROGRESS |
| VC-G01-NATIVE | 最后一阶段：原生窗口、真实物理键盘、画面和异常恢复人工观察 | 原生 Player/人工证据 | LOCKED；P1—P4 未清零前不得开始 |
| VC-G02 | 验证 14 个固定镜头和 3 个天气变体；记录不可接受的穿模/悬浮/空白/遮挡 | Player 证据 | LOCKED |
| VC-G03 | 仅在 VC-G01/G02 证据真实通过后，更新 D25—D31 状态板 | 状态板/报告 | LOCKED |

## 原生 Player 前置门禁规则（2026-09-06）

VC-G01 不再把“等待原生窗口”当作唯一下一步。后续每次进入 native 之前，必须先完成并记录 P0—P4：

1. P1 关闭源码可判定的输入归属错误；主 HUD 是非模态状态层，W/A/S/D 不能被 `EventSystem` 导航吞掉；仓库、暂停和临时页才接管自己的焦点。
2. P2 用 Unity PlayMode 合成设备事件验证键盘/焦点/页面恢复；定向 PASS 不能覆盖全量失败。
3. P3 用真实场景对象和碰撞查询验证路线，不修改阈值来制造通过。当前运行时以 R03 活动路线对象坐标作为路线事实，同时保留旧 D19 灰盒坐标的兼容性诊断；两套坐标不能混写成一个 PASS，未完成的旧坐标迁移仍必须单独留痕。
4. P4 验证构建和启动可观测性，并在能够持续推进渲染/帧循环的 harness 中完成性能采样；`SampleCount=0` 必须是失败或阻塞，不得改报告 token。
5. 只有 P1—P4 没有未解释的阻塞后，才启动 VC-G01-NATIVE。原生 Player 通过后，才能解锁 VC-G02；VC-G03 仍需 VC-G02 真实证据。

当前前置门禁聚合结果：[VC-G01 pre-native gate](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PRE_NATIVE_GATE_20260906.json)。

### P3 坐标框架说明（2026-09-06）

`D19S02ScenePlayModeTests` 现在同时记录两套事实：R03 当前运行时的活动对象路线（玩家、市场、仓库、酒馆、船厂、主码头）以及历史 D19 灰盒兼容锚点/旧坐标。最新定向结果显示活动 R03 路线 `6/6` 有碰撞面、`6/6` 有 NavMesh，而旧坐标探针仍为 `1/6`。因此 P3 在“当前 R03 活动路线”范围内为 `PASS_WITHIN_SCOPE`，旧坐标兼容性仍是明确的后续迁移/契约事项，不得被解释为完整 D19 旧坐标验收通过。

## 本轮明确不做的事

- 不用截图或静态 Prefab 审计冒充运行时通过。
- 不删除重复资源；先登记重复项，确认没有引用后再由独立清理任务处理。
- 不在 `Systems/` 添加网络、LLM 或新的数值裁决权。
- 不把“模型简陋”当成“空间可以错位”的理由：低模可以接受，但地面、海面、船、建筑和角色必须各自处在合理空间层级。

## 2026-09-05 当前修订

`VC-A16` 已完成限定范围实现和真实 Player 固定镜头复核。最新证据为 `game/tools/results/VISUAL-CLEAN/VC-A16/VC_A16_RESULT.json`、`docs/game/design/common/handoffs/VISUAL-CLEAN/VC-A16_HANDOFF.md`、`game/tools/results/D19/D19-R03-A01/D19_R03_A02_FIXED_SHOT_PLAYER_RERUN21.json` 与 `game/tools/results/VISUAL-CLEAN/VC-A09_SCENE_RENDERER_DIAGNOSTICS.json`。本次 14/14 截图只覆盖 Calm，不等于天气或最终美术 PASS。

`VC-S01` 已在限定范围内通过：全量 Unity EditMode 初始基线为 `585/585 PASS`；VC-A02 修补后的最新全量回归为 `586/586 PASS`，Python 定向总回归为 `72/72 PASS`。D27 R03 静态审计通过、R03 Prefab/场景审计和场景碰撞审计均已刷新。期间修复了 R03 `Beach` 视觉节点与历史兼容节点同名导致的全量测试回归；R03 新节点现为 `R03_Beach`，旧 `Beach` 仍保留且 Renderer disabled。

VC-G01 开始前的两项纯代码前置审计也已完成：交互逻辑 `48/48 Python + 3/3 Unity EditMode`，世界/碰撞逻辑 `31/31 Python + 4/4 Unity EditMode`；两组修复合入后全量 Unity EditMode 为 `593/593 PASS`。交互审计结果位于 `game/tools/results/VISUAL-CLEAN/VC-G01-LOGIC/VC_G01_LOGIC_RESULT.json`，世界/碰撞审计结果位于 `game/tools/results/VISUAL-CLEAN/VC-G01-WORLD-LOGIC/VC_G01_WORLD_LOGIC_RESULT.json`。这两项均为 `PASS_WITHIN_SCOPE`，不代表 Player 运行时通过。

下一步仍是 `VC-G01`：使用最新 R03 Prefab 重建当前 Player，验证启动、WASD 行走、转向、登船、下船、仓库/市场/酒馆入口，并记录成功、未就绪拒绝、重复触发和关闭恢复证据。随后进入 `VC-G02` 固定镜头/天气变体回归，最后才由 `VC-G03` 同步 D25—D31 正式状态。D25—D31 正式结果继续保持原有 `PARTIAL`，直到运行时证据真实闭环。

## 2026-09-05 VC-A02 碰撞坐标收口

`VC-A02` 已在代码、Prefab、编辑器审计和逻辑测试范围完成。根因是旧 R03 `D19_TerrainRoot` 以 `(0,0.05,5)` 保存宽 BoxCollider；运行时把可见岛面网格复制到该节点时，会把物理岛面整体移到可见几何之外。现在生成器直接创建与 `D19_R03_IslandSurface` 共用网格的 `MeshCollider`，并将 TerrainRoot 归一到 `(0,0,0)`；运行时仍会同步 transform 以兼容旧 override。证据为 `game/tools/results/VISUAL-CLEAN/VC-A02/VC_A02_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-A02/VC_A02_PYTHON_TESTS.md`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_SCENE_COLLIDER_AUDIT.json` 和 `game/tools/results/VISUAL-CLEAN/VC-S01/VC_S01_ALL_EDITMODE_R4_FINAL.xml`。

这只关闭“碰撞几何坐标/宽盒”的逻辑缺口，不等于真实 Player 路线已经通过；`VC-G01` 仍是下一门，`VC-G02`/`VC-G03` 继续锁定。

## 2026-09-05 VC-S01 结果登记

- [VC-S01 结构化结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-S01/VC_S01_RESULT.json)
- [VC-S01 交接报告](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/VISUAL-CLEAN/VC-S01_HANDOFF.md)
- [全量 Unity EditMode XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-S01/VC_S01_ALL_EDITMODE_R2.xml)
- [R03 编辑器审计](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- [场景碰撞审计](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_SCENE_COLLIDER_AUDIT.json)

## 2026-09-05 VC-G01 启动登记

`VC-G01` 已进入进行中：为避免覆盖旧路线证据，新增 RERUN47、RERUN48 Player/探针；RERUN48 使用 VC-A02 的 MeshCollider 坐标修复后 Prefab 构建成功。当前只完成启动准备和编译/构建确认；本轮没有获得可归档的 WASD、转向、登船、下船、仓库/市场/酒馆真实输入证据，因此不得标记 PASS，也不得解锁 `VC-G02`。

本轮又修正了路线探针本身的证据坐标：市场 `(6.4, 1.2)`、仓库 `(-6.4, 1.2)`、船厂 `(-9.5, -2.6)` 现在与 R03 builder 的表现建筑坐标一致。该修正只消除旧灰盒坐标造成的提前命中风险，不改变 `VC-G01` 的运行时门禁；最新源码回归为 `22/22 PASS`，定向总回归为 `73/73 PASS`。随后已用 Unity 6000.0.80f1 重新编译并重建 RERUN48 Player，无 C# 编译错误；全量 EditMode 为 `586/586 PASS`。

- [VC-G01 当前结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)
- [VC-G01 交接报告](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/VISUAL-CLEAN/VC-G01_HANDOFF.md)
- [RERUN48 Player 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_PLAYER_BUILD_R02_RERUN48.log)
- [RERUN48 独立构建摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN48.log)

继续执行方式：启动 `D19_R03_A01_Player_RERUN48.exe`，进入 `InitialHarbor` 后按任务矩阵逐项实际操作；关闭 Player 后读取 `D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json` 和 Player 日志，补齐成功/拒绝/重复/恢复证据。当前计算机控制表面未提供可操作的原生窗口，因此这一小段需要用户在本地手动完成，或在后续提供可操作的窗口控制能力。

## 2026-09-05 D27/D35 静态前置收口

- D27 R03 表现挂接在 D19 基础 Prefab 保存后自动重建，最新审计为 `PASS`，`r03_attachment_count=1`，NPC/动物/宠物 `12/8/2`。
- D35 R03 材质与表现几何静态审计为 `PASS_WITHIN_SCOPE`：12 材质、11 个纹理材质、1 个明确的 R03_Plaster 纯色例外、NormalMap 元数据 `6/6`；D19 自有层为 `391 Renderer / 1 Collider / 39208 mesh vertices`。审计阈值没有放宽。
- 相关 Python 定向回归 `98/98 PASS`；D27/D35 修改合入后的 Unity 全量 EditMode 为 `593/593 PASS`，证据为 [VC-G01_ALL_EDITMODE_R4.xml](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_ALL_EDITMODE_R4.xml) 与 [R4 日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_ALL_EDITMODE_R4.log)。
- VC-G01 自动化 PlayMode 前置也已通过 `15/15`：D19S02R01 路线 `1/1`、D16 输入 `10/10`、D19 交互 `2/2`、D19 路线 `2/2`；详见 [VC-G01 PlayMode 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_PLAYMODE_RESULT.json)。该结果使用合成设备事件，`runtime_proof=false`，不替代人工 Player。
- D27/D35 修复及 VC-G01 前置合入后的全仓库 Python 回归为 `1085/1085 OK`，命令为 `python -m unittest discover -s tools/tests -p 'test_*.py'`。
- D35 R75 修复后已重新构建同一路线 RERUN48 Player；构建摘要和 Player Data 已更新，使用最新 Prefab 的人工验证入口仍为 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN48.exe`，构建日志为 `game/tools/results/VISUAL-CLEAN/VC-G01_PLAYER_BUILD_R04_RERUN48.log`。
- 这些是代码/编辑器静态前置证据，不能替代 Player 运行时、真实 OS 输入、固定镜头画质或性能验证；D31 正式结果仍保持 `PARTIAL/R74`。

因此，下一项仍是 `VC-G01`，不是 `VC-G02`：启动最新 RERUN48 Player，进入 `InitialHarbor`，完成真实 WASD/转向、E 登船/下船、I 仓库、M 市场、T 酒馆及关闭恢复矩阵，并保存路线探针和 Player 日志。只有 VC-G01 的真实证据通过后，才解锁 VC-G02。

## 2026-09-06 R87 正式留痕与全量回归

本次按 R87 证据完成收口，没有继续修改视觉坐标。第一次 Unity 调用因显式附带 `-quit` 在测试调度前退出、未生成 XML，未作为结果使用；随后按项目既有正确调用重新执行，以下 R5 文件为唯一有效的本次 Unity 回归证据。

### 本次已登记

| 项目 | 结果 | 证据 |
|---|---|---|
| 全仓库 Python 回归 | `1087/1087 PASS`，失败/跳过 0 | `python -m unittest discover -s tools/tests -p 'test_*.py'` |
| Unity 全量 EditMode | `593/593 PASS`，failed/inconclusive/skipped 均为 0，退出码 0 | [VC_G01_ALL_EDITMODE_R5 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R5_20260906.xml)、[日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R5_20260906.log) |
| R87 Player 视觉采集 | 固定镜头 `14/14`、天气 `3/3`、`1920x1080` backbuffer、17 个文件存在 | [R87 结构化证据](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D31_P04_VISUAL_EVIDENCE_PLAYER_R87.json)、[截图目录](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/d31_visual_evidence_R87/) |
| VC-A07-R01 限定修复 | `PASS_WITHIN_SCOPE` | [结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-A07-R01/VC_A07_R01_RESULT.json)、[交接](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/VISUAL-CLEAN/VC-A07-R01_HANDOFF.md) |
| VC-A16-R01 证据刷新 | `PASS_WITHIN_SCOPE` | [结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-A16-R01/VC_A16_R01_RESULT.json)、[交接](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/VISUAL-CLEAN/VC-A16-R01_HANDOFF.md) |

### 门禁状态不变

- `VC-G01`：仍为 `IN_PROGRESS`。R87 是固定镜头/天气采集，不是实际输入路线证据；WASD、转向、登船、下船、仓库/市场/酒馆矩阵仍缺。
- `D31-P04`、`D31-S05`、`D31-G06`：继续 `PARTIAL`。R87 已进入 [D31-P04 R87 复核结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D31/D31-P04/D31_P04_R87_REVALIDATION_RESULT.json)，但低模/程序化画面质量、真实性能、完整交互和 D30 异常恢复未关闭。
- `VC-G02`、`VC-G03`：继续 `LOCKED`。R87 的 `14/14 + 3/3` 只表示证据文件完整，不代表最终视觉候选或正式发布状态。

本节只更新结果、交接和状态台账，不把“代码回归通过”或“截图生成成功”写成最终游戏完成。

## 2026-09-06 接管审计与 VC-G01 启动烟测

- 已完成只读接管审计：当前分支为 `agent/publish-project-changes-20260813`，HEAD 为 `01024cae2823c26b2595a660e6dde5879870dfc8`；工作区既有修改和未跟踪产物均保留，不执行清理或覆盖。
- 新增并通过 `tools/vc_g01_runtime_startup_smoke.ps1` 的 5 项静态测试与 PowerShell 语法解析；工具只启动已知 RERUN48、等待 15 秒、请求优雅关闭并校验本次探针/Player.log 刷新。
- 新增工具后的全仓库 Python 回归为 `1092/1092 PASS`（失败/跳过 0，退出码 0）；R87 原有 `1087/1087` 作为历史证据保留。
- 最新启动烟测为 `OBSERVED_ONLY`：PID `26552` 启动成功，`CloseMainWindow=True`、`graceful_exit=True`、`exit_code=0`；摘要为 [VC_G01_STARTUP_SMOKE_20260906.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_STARTUP_SMOKE_20260906.json)。
- 探针本次刷新为 `scene=MainMenu`、`player_found=false`、`observations=0`、`events=1`；Player.log 有 MainMenu/InputSystem/UI 启动标记。该结果只关闭“Player 可启动/可退出”的不确定性，不证明 `InitialHarbor`、WASD、转向、E、I/M/T 或任何成功/拒绝/重复/恢复路径。
- 由于 CUA 当前只返回 `apps=[]` 和浏览器表面，VC-G01 继续 `IN_PROGRESS`；需要原生 Windows 窗口控制或用户手动操作完成路线矩阵后，才可评估是否解锁 VC-G02。VC-G02/VC-G03 继续 `LOCKED`。

## 2026-09-06 VC-G01 路线探针输入覆盖与 R8 验证

本轮没有扩大视觉修改范围，先补齐真实 Player 证据的观测能力，再进行编译和回归：

- `game/Assets/Scripts/Gameplay/D19/R03/D19R03PlayerRouteProbe.cs` 现在只读记录 `I` 仓库、`M` 市场、`Escape` 关闭的按下边沿与保持态，并保留 W/A/S/D、E/B/T/V 输入字段；探针不调用业务动作、不写领域状态。
- 新增 `tools/tests/test_vc_g01_runtime_probe.py`，与相关世界逻辑定向测试合计 `36/36 PASS`；全仓库 Python 回归为 `1095/1095 PASS`。
- Unity 6000.0.80f1 全量 EditMode R8 为 `593/593 PASS`，`failed=0`、`inconclusive=0`、`skipped=0`；R8 日志无当前路线探针 `CS0618`、`error CS` 或 `Build failed`。
- RERUN48 已使用当前探针源码重建，构建摘要报告 `BuildOptions.None` 成功，Player Data 的 `Assembly-CSharp.dll` 已刷新；随后有界启动烟测 PID `30108` 正常优雅退出。
- 启动烟测仍只观察到 `scene=MainMenu`、`player_found=false`、`observations=0`、`events=1`，未发送真实键盘输入，`runtime_route_proof=false`。

因此正式门禁不变：`VC-G01=IN_PROGRESS`；`D31-P04/D31-S05/D31-G06=PARTIAL`；`VC-G02/VC-G03=LOCKED`。探针覆盖和静态回归不能替代原生窗口内的真实 WASD、转向、E、I、M、T、Esc 成功/拒绝/重复/恢复证据。当前 CUA 仍只有 `apps=[]` 浏览器表面，下一步需要用户手动或具备原生 Windows 控制能力的环境完成矩阵。

## 2026-09-06 VC-G01 原生 Player 路线证据更新

本轮未跳过 VC-G01，也未把启动烟测提升为完整通过。CUA 仍无原生 Windows app surface（`apps=[]`），所以使用 `tools/vc_g01_runtime_input_probe.ps1` 启动 RERUN48，并通过 `user32` 真实发送键鼠事件；路线探针保持只读，窗口截图和 LocalLow Player.log 作为独立证据保存。

- `VC_G01_RUNTIME_INPUT_20260906.json`：最新 PID `26980`，客户区 `1920x1080`，进入 `InitialHarbor`，42/42 个步骤和 16/16 张截图成功，Player 正常关闭。
- `D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json`：90 个观测样本，`player_found=true`，`runtime_route_proof=true`。
- Player.log 确认 E 登船/下船、I 仓库、M 市场、T 酒馆及 Esc 临时页关闭、暂停/恢复；对应截图保存在 `game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_20260906/`。
- 当前 Player.log 已归档为 `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_20260906_RERUN49.log`，避免后续运行覆盖本轮日志证据。
- `moving_observations=0`、`max_speed=0`，且没有独立远离船体后的 E reject；因此真实路线证据是限定范围 `PASS_WITHIN_SCOPE`，不是 VC-G01 完整 PASS。
- D29 M/T owner 快捷键、D7/HarborPause legacy Esc 兼容及只读 D16 诊断已登记；Unity R12 全量 EditMode `593/593 PASS`，全仓库 Python `1105/1105 PASS`。

任务板更新：

- **已完成**：Player 启动、进入 InitialHarbor、E 成功登/下船、I/M/T 可见入口、部分重复输入、Esc 关闭与暂停恢复的真实证据留痕；构建和回归通过。
- **进行中**：VC-G01 移动/转向及 success/reject/duplicate/close-recovery 逐项矩阵。
- **阻塞**：当前注入事件未产生移动观测；CUA 不能接管原生窗口；远离船体 reject 尚无证据。
- **下一步**：获得物理键盘或可接管原生 Player 的控制面后，补齐 WASD/转向 moving 与速度、远离船体 E reject，并逐项复核矩阵；完成前不解锁 VC-G02。

## 2026-09-06 VC-G01 Unity Input System 边界诊断

为解释真实 Win32 事件已发送但没有移动样本的原因，只读路线探针新增 D16 轴、焦点、Gameplay/Primary 输入权限和移动输入来源诊断，没有修改 D2 移动裁决或数值。

- 新构建 RERUN48 使用 R09 构建日志，`BuildOptions.None` 成功；R12 Unity 全量 EditMode `593/593 PASS`，失败/不确定/跳过均为 0。R12 日志无 `error CS` 或 `Build failed`，保留项目既有 CS0618 弃用警告。
- 最新真实探测 PID `26980`，`InitialHarbor`，`player_found=true`，91 个观测，`runtime_route_proof=true`；Player.log 仍确认 E 登/下船、I/M/T 入口和 Esc/暂停恢复。
- `key_w=true` 时同时观测到 `Application.isFocused=true`、`GameplayInputAllowed=true`、`PrimaryInputAllowed=true`，但 D16 轴仍 `(0,0)`、来源为 `D16InputRouter`、legacy fallback 为 false，`moving_observations=0`、`max_speed=0`；PlayerPrefs 也没有 `wb_a11y_binding_*` 重绑定项，默认 W/A/S/D 未被改写。这是当前 Win32 注入路径未进入 Unity Input System 的环境边界，不是权限门禁通过，也不是移动 PASS。
- 新增诊断测试后 Python 全量 `1105/1105 PASS`；结果与 handoff 已同步。

因此任务板保持：`VC-G01=IN_PROGRESS`、`D31-P04/D31-S05/D31-G06=PARTIAL`、`VC-G02/VC-G03=LOCKED`。下一步需物理键盘或可驱动 Unity Input System 的原生控制面补齐移动/转向和远离船体 E reject；不因零轴观测擅改 D2 legacy fallback 规则。

## 2026-09-06 VC-G01 Unity Input System 键盘设备存在性复核

在只读路线探针上增加 `Keyboard.current` 与 W/A/S/D 控件状态字段后，使用 R10 重建 RERUN48 并重新执行真实 Win32 输入探测。最新摘要为 PID `21556`、`InitialHarbor`、42/42 输入步骤、16/16 截图步骤、Player 正常关闭；路线探针为 89 个样本。

- `Keyboard.current` 在 89/89 个样本存在，说明 Player 已创建键盘设备。
- Input System W/A/S/D 按下样本为 `0/0/0/0`，而同一运行的 legacy `Input.GetKey` W/A/S/D 样本为 `7/3/5/3`；应用焦点和 Gameplay/Primary 输入权限均为 true，D16 轴仍为 `(0,0)`。
- 结论：当前 user32 `SendInput`/legacy fallback 事件进入旧输入通道，但没有更新 Unity Input System 键盘状态。该诊断不是移动 PASS，也不支持修改 D2 fallback 选择规则。
- 结果已同步到 [VC_G01_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)、[运行摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_20260906.json)、[RERUN50 Player.log](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_20260906_RERUN50.log)。

门禁不变：`VC-G01=IN_PROGRESS`，D31 相关任务继续 `PARTIAL`，`VC-G02/VC-G03=LOCKED`。解除条件仍是物理键盘或具备 Unity Input System 输入能力的原生 Player 控制面，并补齐远离船体 E reject 与逐项矩阵。

## 2026-09-06 R13 全量回归与当前交接点

- 键盘设备诊断字段合入后，Unity 6000.0.80f1 EditMode R13 为 `593/593 PASS`，failed/inconclusive/skipped 均为 0；证据为 [R13 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R13_20260906.xml) 与 [R13 日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R13_20260906.log)。
- 本轮 Python 全量为 `1105/1105 OK`，耗时 `220.732s`，退出码 0。
- 同一 user32 注入路径已完成诊断：`Keyboard.current` 存在，但 Input System W/A/S/D 状态为零，legacy 状态可见；因此不再重复该路径，不把移动/转向写成 PASS。
- 下一次真实验证需要物理键盘或具备 Unity Input System 输入能力的原生 Player 控制面，并补远离船体 E reject 与完整矩阵；门禁保持 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。

## 2026-09-06 原生控制面能力探查与 VC-G01 环境边界

本轮继续围绕 VC-G01 的真实输入门禁做环境核验，没有扩大视觉坐标修改，也没有安装/卸载驱动或改写设备配置。

- 只读 PnP 探查确认 Logitech G HUB Virtual Keyboard（`HIDClass/OK`）、Rapoo Bluetooth keyboard（`Bluetooth/OK`）和 Xbox Wireless Controller（`Bluetooth/OK`）存在；G HUB `logi_joy_vir_hid` 服务和 virtual HID driver manager 已安装。
- G HUB virtual HID manifest/driver manager 只提供驱动生命周期与状态能力（`--install`、`--uninstall`、`--status` 等），未发现可安全调用的用户态按键事件接口；这不是 Player 输入证据。
- CUA 当前仍没有原生 app surface，现有 user32 `SendInput`/legacy fallback 只能让 legacy `Input.GetKey` 观察到部分键状态，不能更新 Unity Input System W/A/S/D；G HUB 虚拟设备和软件合成事件不作为物理移动 PASS 替代。

因此 VC-G01 仍为 `IN_PROGRESS`：真实 Player 已证明启动、InitialHarbor、近船 E 登/下船、I/M/T 入口和 Esc 恢复的限定范围，但移动/转向、远离船体 E reject、逐项 success/reject/duplicate/close-recovery 矩阵仍未闭合。VC-G02/VC-G03 继续 `LOCKED`，D31 相关项继续 `PARTIAL`。解除条件是用户或验收环境提供物理键盘/手柄，或能真实更新 Unity Input System 的原生 Player 控制面。

## 2026-09-06 VC-G01 RERUN53 真实输入闭环更新

本节更新并 supersede 前文基于旧版 32-byte Win32 `INPUT` 注入器的“D16 轴为零”结论。脚本修正 x64 union 对齐为 `40 bytes` 后，使用当前 R14 构建重新执行 standalone Player；探针仍为只读，游戏裁决仍由 D2/D7/D29 owner 完成。

| 检查项 | RERUN53 证据 | 当前判断 |
|---|---|---|
| Player 启动与路线 | PID `31568`，`InitialHarbor`，44/44 输入步骤成功，优雅退出 | `PASS_WITHIN_SCOPE` |
| W/A/S/D 移动 | 探针 104 样本、moving `31`、max speed `3.200000047683716`；D16 轴非零 `28` 样本；Input System W/A/S/D `16/3/5/4` | `PASS_WITHIN_SCOPE` |
| A/D 转向 | `turning_observed=true`，7 个 turning 事件，最大 yaw delta `180°` | `PASS_WITHIN_SCOPE` |
| E 登/下船与远离拒绝 | 近船 `boarded`/`disembarked`；z≈`12.729` 时 `ship_too_far` 反馈 73 个样本 | `PASS_WITHIN_SCOPE` |
| I/M/T 与 Esc | Player.log 标记和窗口截图覆盖打开、重复快捷键输入、关闭/暂停恢复 | 限定证据；完整业务矩阵仍进行中 |
| 截图与日志 | 17/17 窗口截图成功；Player.log 归档为 RERUN53 | `PASS_WITHIN_SCOPE` |

证据文件：

- [VC-G01 RERUN53 运行 JSON](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_20260906.json)
- [RERUN53 只读路线探针](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json)
- [RERUN53 Player.log 归档](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_20260906_RERUN53.log)
- [R14 Unity 回归 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R14_20260906.xml) / [日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R14_20260906.log)
- [RERUN53 截图目录](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_20260906/)
- 全仓库 Python 回归：`1107/1107 PASS`，失败/跳过为 0，命令为 `python -m unittest discover -s tools/tests -p 'test_*.py'`。

当前门禁：`VC-G01=IN_PROGRESS`，因为 I/M/T 的 owner-not-ready reject、重复操作结果/幂等性和逐项 close/recovery 结果矩阵尚未完整审计；`D31-P04/D31-S05/D31-G06=PARTIAL`，`VC-G02/VC-G03=LOCKED`。CUA 仍无原生 app surface，所以本轮脚本 OS 事件证据不替代人工物理输入验收。

## 2026-09-06 RERUN54 VC-G01 页面状态矩阵更新

R15 页面状态只读探针进入最新 RERUN48 Player 后，重新执行 RERUN54 完整真实输入序列。页面状态不由截图推断：D3 使用 `IsTradeOpen`，D10 使用 `HasInputFocus`，D29 使用 `WarehousePanel.activeInHierarchy`；快捷键边沿单独记录，允许审计短间隔重复输入。

| 验证项 | 证据 | 结果 |
|---|---|---|
| Unity EditMode | `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R15_20260906.xml` / `.log` | `593/593 PASS`，failed/inconclusive/skipped=0 |
| Player 构建 | `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_BUILD_R15_INTERACTION_MATRIX_20260906.log` + `game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN48.log` | 非开发版 `BuildOptions.None` 成功 |
| RERUN54 运行 | `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_20260906.json` | `InitialHarbor`，44/44 输入、17/17 截图、优雅退出 |
| 仓库 I | 同一运行 JSON 的 `interaction_matrix.warehouse` | 3 edges；open 2 / close 2；重复 I toggle 与 close recovery=true |
| 市场 M | 同一运行 JSON 的 `interaction_matrix.market` | 2 edges；open 1 / close 1；重复 M 保持页面与 Esc recovery=true |
| 酒馆 T | 同一运行 JSON 的 `interaction_matrix.tavern` | 2 edges；open 1 / close 1；重复 T 保持页面与 Esc recovery=true |
| 最终状态 | `interaction_matrix.final_temporary_pages_closed` | `true` |
| 视觉/日志 | `runtime_input_20260906/20260906_124737_{warehouse_open_i,market_open_m,tavern_open_t,pause_open_escape}.png`；RERUN54 Player.log | 四类可见状态与日志标记均存在 |
| Python 全量回归 | `python -m unittest discover -s tools/tests -p 'test_*.py'` | `1107/1107 PASS` |

本轮结构化结果的 `interaction_matrix_proof=true`，但各页面 `owner_not_ready_reject_proof=false`，因为 RERUN54 没有在 owner bootstrap 未就绪窗口发起 I/M/T。短间隔重复页状态证据也不升级为领域业务幂等 PASS。当前状态保持：`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/D31-S05/D31-G06=PARTIAL`。后续若需要关闭剩余缺口，应使用有界 owner bootstrap 测试或真实原生 Player 控制面，不修改 D2 fallback 规则。

## 2026-09-06 R17 VC-G01 owner-not-ready reject 矩阵补证

为关闭 RERUN54 未覆盖的 bootstrap owner-not-ready 缺口，本轮以最新 R17 非开发版 Player 分别执行 I、M、T 三次独立运行。每次运行都在新游戏确认后的 bootstrap 窗口发送一个目标快捷键，并通过同帧只读探针状态、唯一 run marker 后 Player.log、页面转移计数及 `SendInput` 返回值交叉验证。

| 目标 | 独立运行结果 | owner 未就绪边沿 | reject 日志 | 目标页打开转移 | 目标证明 |
|---|---|---:|---|---:|---|
| I / 仓库 | `VC_G01_BOOTSTRAP_REJECT_I_20260906.json` | 1 | 存在 | 0 | `true` |
| M / 市场 | `VC_G01_BOOTSTRAP_REJECT_M_20260906.json` | 1 | 存在 | 0 | `true` |
| T / 酒馆 | `VC_G01_BOOTSTRAP_REJECT_T_20260906.json` | 1 | 存在 | 0 | `true` |

聚合结果 `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_BOOTSTRAP_REJECT_MATRIX_20260906.json` 为 `PASS_WITHIN_SCOPE` 且 `matrix_proof=true`。R17 构建日志为 `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_BUILD_R17_BOOTSTRAP_TARGETS_20260906.log`；本轮最新 Unity EditMode 为 `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R16_BOOTSTRAP_20260906.xml`，`593/593 PASS`；全仓库 Python 为 `1110/1110 PASS`。

该专项只证明 I/M/T 在 bootstrap owner 未就绪时的拒绝边界，不证明人工物理键盘验收、领域业务幂等或完整 D31。当前门禁保持 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/D31-S05/D31-G06=PARTIAL`；CUA 仍没有原生 Windows Player surface（`apps=[]`）。

### 原生控制面复核（2026-09-06 13:50 +08:00）

实时 CUA `getState()` 返回 `apps=[]`；仅有 1 个 Codex In-app Browser 且无 tab，仍没有可绑定的 Windows Unity Player。故本轮不再重复相同脚本运行，保持 `VC-G01=IN_PROGRESS`。解除条件是原生 Player 控制面或用户物理操作交付可审计的逐步截图、Player.log 和路线探针结果。

## 2026-09-06 P2/P4 前置门禁清零与 Player 重建

本轮继续执行 native-last 计划，先处理自动化门禁的真实失败，再重建独立 Player；没有把 `SampleCount=0` 改写成通过，也没有覆盖历史 RERUN48 产物。

### 根因与修复

- `D16RuntimePerformanceProbe` 原先通过全局查找命中了被 R03 运行时停用的旧 R02 presentation root；活动 `D16WorldPerformanceController` 因此没有自己的 probe，导致渲染态报告真实出现 `SampleCount=0/100`。
- 现在由 `D16WorldPerformanceController` 自己持有并确保活动 probe；被停用的旧 probe 也不会在销毁时用零样本报告覆盖活动报告。PlayMode 断言同时检查 probe 的 active/owner/host，避免只有 token 没有采样的假阳性。
- D16 A03 输入回归在同一测试类全量运行时显式重新取得 `Keyboard.current`，消除 fixture 顺序带来的输入设备 ownership 污染；该变更只修正测试前置，不向产品输入层加入 fallback。

### 当前证据

| 门禁 | 结果 | 证据 |
|---|---|---|
| D16 probe ownership/bootstrap | `1/1 PASS`；`PerformanceProbeActive=true`、`PerformanceProbeOwnedByController=true`、host=`D16WorldPerformanceController`、`SampleCount=3` | [XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PROBE_OWNERSHIP_R2.xml)、[报告](C:/Users/Administrator/Desktop/game/first01/tools/results/d16_runtime_playmode_report.json) |
| D16 A01 渲染态连续采样 | `1/1 PASS`；60.004s、`234/100` samples、`Passed=true`，RTX 4070 Ti/D3D11 | [XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_A01_RENDERED_R5.xml)、[报告](C:/Users/Administrator/Desktop/game/first01/tools/results/d16_a01_performance_window_report.json) |
| D16 输入测试类 | `10/10 PASS`；W/S 船体移动诊断为 `MoveAxis=(0,1)`、距离 `0.0276`、速度从 `0` 到 `0.349` | [XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_INPUT_ROUTER_ALL_R2.xml)、[日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_INPUT_ROUTER_ALL_R2.log) |
| D16 PlayMode 完整 corpus | `70 PASS / 0 FAIL / 1 SKIP`（71 cases，退出码 0）；唯一跳过是明确的 D19A01 work-scene 条件，不是失败 | [XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PLAYMODE_ALL_AFTER_INPUT_DIAGNOSTIC_R7.xml)、[日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PLAYMODE_ALL_AFTER_INPUT_DIAGNOSTIC_R7.log) |
| Unity EditMode | `593/593 PASS`，failed/inconclusive/skipped 均为 0 | [XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_AFTER_RERUN49_20260906.xml)、[日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_AFTER_RERUN49_20260906.log) |
| Python unittest | `1110/1110 PASS`；使用项目支持的 `unittest discover` runner | `python -m unittest discover -s tools/tests -p 'test_*.py'` |
| 独立 Player 构建 | RERUN49 非开发版 `BuildOptions.None`、Build Successful，无 `error CS`/Build failed | [Unity 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_BUILD_AFTER_PROBE_OWNERSHIP_R2_20260906.log)、[构建摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN49.log)、[Player](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN49.exe) |
| Player 启动烟测 | `STARTUP_OBSERVED_ONLY`；15s 后优雅退出、exit code 0、探针/Player.log 刷新；仍停在 MainMenu，未伪称路线通过 | [摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_STARTUP_SMOKE_AFTER_PROBE_OWNERSHIP_20260906.json)、[Player.log 归档](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_STARTUP_AFTER_PROBE_OWNERSHIP_20260906.log)、[RERUN49 探针](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN49.json) |

### 当前任务板

- **已完成**：P0、P1、P2、P3、P4 的本机限定范围门禁；P4 的性能值为真实帧采样，GPU counter 没有正值时仍保持 `GpuSampleAvailable=false`，没有填充假 GPU 数值。
- **进行中**：VC-G01 总体验收仍需最后的 native Player/人工视觉矩阵；脚本或合成输入不替代人工物理键盘、窗口观察和画面质量判断。
- **阻塞**：本会话 CUA 仍没有可绑定的原生 Windows Player 控制面；D19A01 work-scene 仅有 1 项条件跳过；D31-P04、D31-S05、D31-G06 继续 `PARTIAL`。
- **下一步**：在获得原生窗口控制或用户交付逐步人工证据后执行 `VC-G01-NATIVE`；只有 VC-G01 native gate 完成才解锁 VC-G02，再由 VC-G03 同步 D25—D31。当前 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。

## 2026-09-06 原生 Player 四类缺陷定向修复与复核门禁

用户最新 Player 观察登记为四类缺陷：人物动作扭曲、海洋不能游泳、上船后人物/船控呈现异常、树木悬空。本节只推进可由源码、编辑器测试和资源重建证明的部分；原生画面仍是最后的复核门禁。

### 修复范围

- 登船：`D7_HelmPoint` 从固定局部坐标改为按稳定船根缩放反变换的世界甲板偏移，避免 `2.573956` 根缩放把玩家抬到空中。
- 海岸：D6 碰撞层增加南/西/东安全围界，并增加 D2 越界后的最近安全位置恢复。游泳状态/水中移动没有在本节伪造实现，仍按 D2 当前范围标记为未实现。
- 植被：R03 的棕榈、灌木、岩石根部统一采样岛面 `Height(x,z)`，并已重建 R03 presentation prefab。
- 动作：生成的 `D6CrewAnimator` 的绝对 imported Euler 占位曲线不再接管当前 rig，保持单一程序化/neutral 姿态层，等待经过视觉验收的正式动作资源。

### 验证与状态

| 项目 | 证据 | 结果 |
|---|---|---|
| 世界逻辑 | `VC_G01_PLAYER_DEFECTS_EDITMODE_WORLDLOGIC_R2_20260906.xml` | `7/7 PASS` |
| 动作层 | `VC_G01_PLAYER_DEFECTS_EDITMODE_ANIMATION_20260906.xml` | `6/6 PASS` |
| Unity 全量 EditMode | `VC_G01_PLAYER_DEFECTS_EDITMODE_FULL_20260906.xml` | `597/597 PASS` |
| D17 动作 PlayMode | `VC_G01_PLAYER_DEFECTS_PLAYMODE_D17_ACTION_20260906.xml` | `1/1 PASS` |
| D19 路线 PlayMode | `VC_G01_PLAYER_DEFECTS_PLAYMODE_D19_ROUTE_20260906.xml` + `D19_S02_R01_ROUTE_LOG.json` | `1/1 PASS`；walk/ship/board/disembark/save/reload 运行时报告齐全 |
| R03 Python 定向回归 | `tools/tests/test_d19_r03_visual_builder.py` | `27/27 PASS` |
| R03 资源重建 | `VC_G01_PLAYER_DEFECTS_R03_BUILD_20260906.log` | `Build Successful` |
| Windows Player 重建 | `VC_G01_PLAYER_DEFECTS_PLAYER_BUILD_20260906.log` | `BuildOptions.None / Build Successful` |
| 原生画面 | `VC_G01_PLAYER_DEFECTS_REVALIDATION_20260906.json` | `PENDING_USER_RECHECK` |

本节不改变依赖顺序：`VC-G01-NATIVE` 完成前，`VC-G02` 与 `VC-G03` 继续 `LOCKED`；游泳功能不因安全围界修复而升级；D31-P04、D31-S05、D31-G06 继续 `PARTIAL`。本轮全量 Python 命令因超过有界等待窗口被停止，未计为本轮 PASS。

## 2026-09-09 LOCAL-QUALITY Q27 D16 预算评估性能剖析

Q27 在真实 standalone Windows x64 Player 上对 `D16WorldPerformanceController` 的预算评估做了只读耗时剖析，并先用 EditMode `D16PerformanceBudgetTests 6/6 PASS` 确认诊断字段不会破坏预算合同。有效 R3 报告为 `30s` 预热 + `60.014019s` 采样、`3465` 帧：`349` 次评估总计 `49.165962ms`，平均 `0.14087668ms`，最大 `5.4445267ms`；同期平均主线程为 `13.7289915ms`。因此 D16 预算评估约占主线程的粗略 `1%`，不足以支持在 D16 内做性能优化。

首轮 R1 报告因计时起点复用错误被明确标记为 `REJECTED_EVIDENCE`，没有进入性能结论；修复计时起点后才接受 R3。R3 仍记录 GPU timing unavailable、最长帧 `622.258118ms`、1% Low `21.460548 FPS`，所以 Q27 只完成“假设测量和停止试验”，总体保持 `PARTIAL`，不能升级为性能 PASS。下一步是 Q28 新候选 Player 构建，随后 Q29 自动化路线与输入证据；不改变 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。

## 2026-09-09 LOCAL-QUALITY Q28/Q29 候选 Player 与真实输入路线

Q28 在当前源码上生成独立的非开发版 `Q28-CANDIDATE-R2`：MainMenu -> InitialHarbor，`BuildOptions.None`。D17A08 游泳视觉专项为 `1/1 PASS`；同时将游泳 visualRoot 的俯仰和下沉幅度收紧，避免旧的 `68°/-0.28m` 姿态在水中把角色压成脸朝下并埋入水面。该修复只是视觉安全根保护，不能替代 Q03 仍缺失的正式 walk/run/swim 动作资源。

Q29 使用真实 standalone Player 和 Win32 `SendInput` 完成两条证据路线。R7 水域路线为 `45/45` 输入步骤、`18/18` 截图、`152` observations、`71` moving observations、`101` swimming observations，I/M/T、Esc、登船/下船和重复/关闭状态矩阵成立；原始水域探针已归档为独立 `WATER_Q29_R7` 文件。R6 在修正远离方向后记录 `boarded`、`disembarked`、`ship_too_far`，`far_ship_reject_proof=true`。

本轮不升级视觉状态：R7 水中截图仍是低模/中立角色，港口仍能观察到低模建筑及岩石/植被层级、穿插或悬浮风险；Q03=`BLOCKED_ASSET`、Q20=`PARTIAL/BLOCKED_ASSET`。Q29 结果为 `PARTIAL`，正式 `VC-G01` 继续 `IN_PROGRESS`，Q30 做独立证据审查，Q31 才执行最终 Player 门禁，`VC-G02/VC-G03` 继续 `LOCKED`。

## 2026-09-06 拒绝下船路径补证

为避免只验证成功路径，本轮补跑 D17 的无可行走表面拒绝下船测试。Unity PlayMode XML 为 `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_DEFECTS_PLAYMODE_D17_DISembark_REJECT_20260906.xml`，结果 `1/1 PASS`，failed/inconclusive/skipped 均为 `0`，进程退出码为 `0`；日志确认先进入船只控制，再输出“下船被阻止：附近没有可行走的沙滩、码头或跳板”。

该结果只关闭自动化的拒绝分支缺口：它证明没有安全落点时不会把玩家错误地放下船，不证明 standalone Player 的视觉位置，也不实现游泳。`VC-G01-NATIVE` 仍为 `PENDING_USER_RECHECK`，`VC-G01=IN_PROGRESS`；完成四点人工画面复核前不得解锁 `VC-G02` 或 `VC-G03`。

## 2026-09-09 LOCAL-QUALITY Q17 船坞与整备桥前置收口

在进入 Q18 HUD 尺寸矩阵前，补齐了独立于原生 Player 的 D29/D9/D4 业务桥证据。真实 InitialHarbor PlayMode 的 Q17 R5 为 `1/1 PASS`，覆盖修船成功、材料不足、重复不变、同 `saveRevision` 幂等、取消/输入恢复、船坞成功、资金不足拒绝和 owner 异常恢复。R4 暴露的真实缺陷是 D29 可用性刷新覆盖最新动作反馈，现已由 D29 UI 单一写者修复。

Q17 结果与交接位于 `game/tools/results/LOCAL-QUALITY/Q17/RESULT.json` 和 `docs/game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md`。D29 修复后的 Q15 R6、Q16 R3 依赖回归也各为 `1/1 PASS`。这只推进本地业务/逻辑前置，不改变正式门禁：Q03 仍 `BLOCKED_ASSET`，`VC-G01=IN_PROGRESS`，`VC-G02/VC-G03=LOCKED`，D31 相关项继续 `PARTIAL`；原生 Player 的动作扭曲、海洋游泳、上船控制关系和树木空间层级仍需最后画面/真实输入复核。

Q18 的代码级 HUD 尺寸矩阵随后通过：`D31Q18HudResolutionTests` 对 1280×720、1920×1080、2560×1440 为 `3/3 PASS`，D31 Visual Logic 全套为 `36/36 PASS`，D29 静态回归为 `28/28 OK`。Q18 仍登记 `PARTIAL`，因为当前会话没有把原生窗口截图、英文最长文案栅格化和鼠标命中审查伪装成自动化通过；这些缺口与 VC-G01 native gate 一起集中处理。

## 2026-09-09 LOCAL-QUALITY Q19 船员与反馈审计

Q19 新增只读审计 `Q19CrewInteractionAuditTests`，以真实 SQLite `NewGame` 和离线模板为入口，证明 D9 对话只产生 `assign_duty/lookout` typed proposal，非法岗位由 D10 CrewSystem 返回 `duty_not_qualified`，合法岗位经过请求/批准后写入行为事实；D10 关系 Gift 事实可创建并在重复请求时返回重复保护。`event_d9_crew_report`、D10 行为状态和 D10 关系快照均经 `SaveSystem.Load` 恢复，专项 `1/1 PASS`，Save.Tests 全程序集 `366/366 PASS`。

Q19-P01 已在 CrewAgent 持久化/Save 边界内关闭：`AgentMemorySnapshot` 与 `agent_memory_v1` JSON 现在保存并恢复 `short_context`、`summary`、`turn_count` 和事件事实；AgentMemory 专项 `1/1 PASS`，Save.Tests 全程序集 `375/375 PASS`，AgentCore `dotnet build` 为 `0 warnings / 0 errors`。Q19 当前为 `VERIFIED`（限离线提议、owner 裁决与持久化范围），不把在线 LLM、原生 Player 对话/输入/画面、动作资源、游泳表现或视觉门禁扩大为已通过。Q20 继续 `PARTIAL/BLOCKED_ASSET`；当前正式状态保持 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`，D31-P04/D31-S05/D31-G06 继续 `PARTIAL`；下一步 Q20。

## 2026-09-09 LOCAL-QUALITY Q20 植被恢复审计

Q20 新增 `D19R03Q20VegetationTests`，在 Unity Editor 中加载 `palm-straight.fbx`，实测 `1 Renderer / 0 缺失材质`，临时工作 Prefab 以可见 Renderer bounds 的 `min.y` 对齐地面，校正前后 `min.y=0.2000`、bounds 高度 `4.2129`。这证明了一个候选资源的接地校正合同，不是把 FBX root pivot 当地面。

Q20 保持 `PARTIAL/BLOCKED_ASSET`：D18 `d18_local_development_ready_catalog.json` 真实为 `BLOCKED`，ready/eligible 均为 `0`；当前 R03 生成器仍 `RenderAuthoredPalms=false`，正式 Prefab 中棕榈引用为 `0`。因此此前浮空风险被隔离，但植被组没有恢复，不能把“全禁用”写成完成。Q03、VC-G01、VC-G02/VC-G03 和 D31 相关正式状态不变；下一步 Q21。

2026-09-11 接管复核使用已连接 Unity Pipeline 对候选和当前 R03 场景做只读审计：`D19R03Q20VegetationTests=1/1 PASS`，`D19_R03_HarborPresentation` 报告为 `294 Renderer / 1 Collider / 40402` mesh vertices，bounds center y=`2.604`、size y=`6.048`；5 个直挂 `ph_palm_tree` 根的 enabled Renderer=`0`，R03 presentation active renderer=`373`。`tree_palm*.fbx` 与 `palm-straight.fbx` 的实际 Renderer bounds 已登记，但 D18 ready catalog 仍为 `BLOCKED`、ready/eligible=`0`，所以不修改正式 builder/Prefab、不恢复 authored palms，Q20 继续 `PARTIAL/BLOCKED_ASSET`。结构化证据写入 Q20 RESULT 的 `current_reaudit_20260911_pipeline`；后台 Editor 无 GPU，本轮不伪造截图视觉 PASS。

## 2026-09-09 LOCAL-QUALITY Q21 港口空间审计

Q21 新增 `D19R03Q21HarborSpaceTests`，在真实 Unity EditMode 实例化当前 R03 Prefab，检查四个服务建筑的可见 Renderer bounds、材质槽、地面接触和 pairwise XZ footprint，并检查岛面/路网/岸线/码头/道具/市场入口六个空间组。首轮 R1/R2 真实发现 `WarehouseVisual` 与 `ShipyardVisual` footprint 重叠；R03 生成器只把船坞表现根从 `z=-2.6` 调到 `z=-4.2`，通过既有 `BuildFromMenu` 重建后 Q21 R4 为 `1/1 PASS`。

Q21 还明确了碰撞 owner 边界：实例总碰撞体 `23`，其中 R03 `D19_TerrainRoot` 为唯一非 Trigger `MeshCollider`，其余 `22` 个归嵌入 D27 owner；没有删除 D27 角色碰撞体。稳定 `D2_PlayerSpawnAnchor`/`D2_DisembarkAnchor` 仍存在。Save.Tests `368/368 PASS`，D31 Visual Logic `36/36 PASS`，当前 Editor audit 为 D19-owned `300 Renderer / 1 Collider / 37740 vertices`。

Q21 保持 `PARTIAL`：这些是静态 Prefab/Editor 证据，不能替代 native Player 的镜头遮挡、真实 WASD 路线、鼠标命中、可达性和最终画面。Q20 资源准入、Q03 动作资源、VC-G01 native gate 的阻塞不变；下一步 Q22。

## 2026-09-09 LOCAL-QUALITY Q22 船海天气审计

Q22 新增 `D19R03Q22SeaWeatherAuditTests`，在真实 Unity EditMode 实例化当前 R03 港口与船只 Prefab，检查水面 `96x96`、水线 `y=-0.0397`、湿岸 `min.y=-0.0048`、`28` 个泡沫环、零缺失材质和零岸线表现碰撞体；船体可见 bounds `min.y=0.0600`，`D6_ShipHullCollision` 底部 `y=-0.0400`，与水线对齐。R03 港口表现不重复挂载 `AudioSource`。

Q22 还通过真实 D9 `EnvironmentSystem`/`D9EcologyWeatherReader` 检查 Calm/Rain/Storm 鱼类天气投影 `1.00/0.90/0.55`，并通过真实 D28 天气表现运行时检查降水率 `0/560/980`、表面波动倍率 `1/1.40/2.35`、粒子材质和 Rain/Storm 播放。专项为 `1/1 PASS`，Save.Tests `369/369 PASS`，D31 Visual Logic `36/36 PASS`。

Q22 保持 `PARTIAL`：它是代码/Prefab 级水线与天气 owner 审计，不能代替 native Player 的游泳画面、船海镜头、泡沫/雾、音效、性能、真实输入和截图。Q03 的动作资源仍为 `BLOCKED_ASSET`，Q20 的植被资源仍为 `PARTIAL/BLOCKED_ASSET`；用户报告的动作扭曲、海洋不能游泳、上船控制关系和树木浮空都没有被本卡伪造为已修复。当前正式状态继续 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/D31-S05/D31-G06 `PARTIAL`；下一步 Q23。

## 2026-09-09 LOCAL-QUALITY Q23 正式资源集成审计

Q23 新增 `D19R03Q23FormalResourceIntegrationTests`，只读 additive 打开 `InitialHarbor`，确认当前正式场景只有 `1` 个 active `D19_R03_HarborPresentation`、`1` 个 active `D19_R03_ShipVisualInstance`，船表现仍在唯一 `ph_ship_fishing` 下，R03 root 保持零位/单位缩放。活动 R03 有 `369 Renderer / 0 缺失材质`，整个场景缺失 MonoBehaviour 为 `0`，R03 活动 `D19_TerrainRoot` 为 `1` 个非 Trigger `MeshCollider`，船体碰撞底部为 `y=-0.0400`。

Q23 记录了 R03 港口、R03 船和 D27 Prefab 的 GUID，并在关闭后重新打开场景比较相同的实例数、材质/脚本计数和水线。专项 `1/1 PASS`，Save.Tests `370/370 PASS`，D31 Visual Logic `36/36 PASS`。

Q23 保持 `PARTIAL`：正式场景引用和资源集成通过不等于 native Player 画面通过；逐物体树木接地、相机角度、动作、游泳、上船控制、菜单输入、音效和性能仍需后续门禁。Q03/Q20、VC-G01/VC-G02/VC-G03 和 D31 相关正式状态不变；下一步 Q24。

## 2026-09-09 LOCAL-QUALITY Q24 保存恢复诊断

Q24 新增 `D19Q24SaveRestoreDiagnosticTests`，使用真实 `SaveSystem` 和临时 SQLite 覆盖陆地/出海存档往返，以及保存事务在 `after_player` 阶段失败时的好档保护。陆地恢复了港口、时间、资金、货物和船员士气；出海恢复了船位 `(41.5,-17.25)`、航向 `212`、速度 `2.25`；现有 `D2_PlayerSpawnAnchor` 解析也确认玩家恢复到作者定义的港口安全出生点。专项 `3/3 PASS`，Save.Tests `373/373 PASS`，D31 Visual Logic `36/36 PASS`。

Q24 保持 `PARTIAL`：当前 `PlayerState` 存档 schema 没有独立的游泳状态/姿态字段，所以这不是游泳功能或游泳视觉通过。Q24 同样不覆盖原生 Player 的 WASD/E/I/M/T/Esc、镜头、动作、树木接地、音效和性能；Q03、Q20、VC-G01/VC-G02/VC-G03 及 D31 相关状态保持原边界。下一步 Q25 首航/循环集成诊断。

## 2026-09-09 LOCAL-QUALITY Q25 首航流程整合诊断

Q25 新增 `D19Q25FirstVoyageLoopDiagnosticTests`，验证 D30 只投影成功/失败/重复事件，不替 D19/D3/D21 owner 裁决。D30 阶段序列 `preparation>market>tavern>shipyard>boarded>departed>returned` 通过；`save_failed` 不推进，短窗口重复 `returned` 不重复推进。复用真实 D23 固定种子 owner 链完成市场/仓库、40 tick 航程、抵达、返港、事件和保存投影恢复；专项 `2/2 PASS`。

Q25 还运行真实 `InitialHarbor` PlayMode 交易关闭/保存与出航/推进/靠港路线 `2/2 PASS`，依赖回归为 Save.Tests `375/375 PASS`、D31 Visual Logic `36/36 PASS`。Q25 保持 `PARTIAL`：D10 船员岗位没有被塞入同一条运行时路线，Q19 独立岗位审计和 AgentMemory 缺口保持原边界；本卡不覆盖原生 Player 输入、动作、游泳、上船控制、树木接地、音效和性能。下一步 Q26 真实帧采样/性能基线。

## 2026-09-09 LOCAL-QUALITY Q26 真实 PlayMode 缺陷诊断与表现 owner 收紧

Q26 新增 `D19R03Q26GameplayDefectPlayModeTests`，通过真实 InitialHarbor、Unity Input System 键盘 W/E、D2/D7/D16 owner 和活动 R03 Renderer 运行，不注入 mock 玩法事实。专项 R8 为 `4/4 PASS`：真实 W 在深水中产生 `0.4368m` 平面位移并保持 `Swimming`；登船后 `Mode=Ship`、`PlayerBoarded=true`，玩家与 gameplay `HelmPoint` 对齐，重建后的 `D7_VisibleHelmAnchor` 距玩家 `0.0200m`；旧树 Renderer 为 `0`，活动 R03 植被/岩石为 `14` 个且 `unsupported=0`；远处 E 被 `ship_too_far` 拒绝，靠近船后 E 完成登船和安全下船。

本卡修正了两个实际 owner 问题。D26 现在统一负责 R03 植被接地与 legacy tree Renderer 隔离，运行时修复和审计都只解析首选 R03 表现根内的同一个 `D19_TerrainRoot`，避免场景多个同名 Collider 造成证据不一致；本轮运行日志为 `grounded=14`、`hidden_without_owned_terrain=10`。D19 R03 构建器为单一网格船候选增加 presentation-owned `D7_VisibleHelmAnchor`，无语义舵节点时生成 `HelmStand/HelmWheel`，并通过既有 builder 重建 R03 船 Prefab 与 InitialHarbor。前置输入修复也保留：legacy D19 HUD 不再创建第二个 `StandaloneInputModule`。

依赖回归为 D17 动作/游泳/上下船/输入 `15/15 PASS`、Save.Tests `375/375 PASS`、D31 Visual Logic `36/36 PASS`。D17 只证明 placeholder Animator 已隔离、视觉根姿态安全、状态与 owner 链正确；`ph_crew_male` 正式动作资源仍受 Q03 `BLOCKED_ASSET`，不能把 neutral-only 证明扩大为最终动作质量通过。Q20 ready palm 资源也仍为 `PARTIAL/BLOCKED_ASSET`，所以本卡仅关闭当前活动旧树/R03 对象的可证明浮空风险，不宣称完整植被资源完成。

Q26 仍保持 `PARTIAL`，但 Q26-PERF 的测量子门已完成。历史短 D16 探针只有 `3` 个样本，GPU timing unavailable，存在 `MinFPS=1.287` 与 `LongestFrameMs=776.950`；本轮新建独立非开发版 `Q26-PERF-R1-20260909`，在同一路线/配置下完成 Low/Medium/High 各 `30s` 预热 + `180s` 真实 Player 采样：Medium `9813` samples、平均 `55.40185 FPS`、P95 `22.7927ms`、1% Low `30.76999 FPS`、最慢帧 `41.4446ms`；Low `9772` samples、平均 `55.21082 FPS`、P95 `22.8468ms`、1% Low `30.34569 FPS`、最慢帧 `39.0702ms`；High `9777` samples、平均 `55.21929 FPS`、P95 `22.7801ms`、1% Low `30.50452 FPS`、最慢帧 `38.6371ms`。三档 `completion=complete`、`long_frame_count_over_50ms=0`，但 GPU timing 均明确 unavailable，且平均帧时长约 `18.4ms`，只能作为本机测量证据，不能升级目标设备或全局流畅 PASS。当前 VC-G01 原生完整输入、截图、成功/拒绝/重复路径仍未闭环；`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/D31-S05/D31-G06 继续 `PARTIAL`。权威结果与哈希见 `game/tools/results/LOCAL-QUALITY/Q26/RESULT.json`，下一步为 Q27 单一 owner 性能优化，然后按 Q28→Q31 依赖推进。
## 2026-09-09 LOCAL-QUALITY Q30 动作、游泳与港口几何缺陷审查

Q30 在 Q29 的真实 Win32 Player 路线基础上继续做了最小边界修复。R03 `MakeRockMesh` 由旧尖锐锥体改为闭合八边形四环圆钝体，`MakeShrubMesh` 由四面体改为闭合八边形圆钝低矮体；植被根仍走 `Height(x,z)` 和 `safeRadius=0.80` 的岛内收缩。未验证棕榈资源继续关闭渲染，因为 D18 ready asset gate 仍为 `BLOCKED`，不把禁用当完成。

程序化 fallback 人物的游泳 visualRoot 由 `48°` 调为 `64°`，物理玩家根保持 upright，未知导入蒙皮骨骼不做猜测式轴旋转。D17ActionQualityPlayModeTests 为 `15/15 PASS`，D17A08 记录约 `64°` 的 bounded swim root 与恢复；VC-G01 世界逻辑为 `12/12 PASS`，Q20/Q21 结构审计各为 `1/1 PASS`。R03 presentation rebuild 和独立 Q30-R7 non-development Player 均成功。

Q30-R7 使用真实 Win32 输入运行 Q30-CANDIDATE-R7：`45/45` 输入步骤、`18` 张截图、`152` observations、`69` moving、`102` swimming，场景/登船/下船/I/M/T/Esc/暂停恢复 marker 均存在，`fatal_error=null`。这证明逻辑和运行路线已经可审计，但截图仍显示程序化低模人物、港口低模/层级和部分岸线/镜头空间风险。Q30 因此保持 `PARTIAL`；Q03=`BLOCKED_ASSET`、Q20=`PARTIAL/BLOCKED_ASSET` 不变，`VC-G01=IN_PROGRESS`，`VC-G02/VC-G03=LOCKED`。

证据：`game/tools/results/LOCAL-QUALITY/Q30/RESULT.json`、`docs/game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q30_R7_20260906.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN59_WATER_Q30_R7.json`。下一步是 Q31 最终 Player 视觉门禁，不把自动化和截图存在升级为视觉 PASS。

## 2026-09-09 Q30-REMEDIATION-R8 用户缺陷补证

针对用户最新报告的动作扭曲、海洋游泳、上船控船和树/植被悬浮问题，Q30 继续执行代码修复与有界原生 Player 路线。R8 收紧 fallback 人物几何和程序化动作幅度，降低并贴地 R03 岸线石块，关闭 gameplay 主 HUD 的 UGUI 自身导航，并把 route probe 扩展为记录真实 `Ship` 控制态、船体位置和船头航向。未验证的正式棕榈/树资源继续隔离，不以禁用资源冒充完成。

验证结果：D16 主 HUD 定向 PlayMode `1/1 PASS`；D17 A08 游泳视觉根 `1/1 PASS`；相关 Python `47/47 PASS`；R03 presentation rebuild 成功；独立非开发版 `Q30-CANDIDATE-R8` 构建成功。真实 Win32 水域路线为 `48` 输入步骤、`19` 截图、`167` observations、`70` moving、`125` swimming、`22` Ship control；船体位移 `5.7000008m`、航向变化 `15.7681885°`，业务/暂停/页面矩阵 true、fatal null。证据见 `game/tools/results/LOCAL-QUALITY/Q30/Q30_REMEDIATION_R8_RESULT.json` 和 `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q30_R8_20260906.json`。

状态边界不变：R8 证明逻辑路径和运行态更完整，但截图仍暴露低模角色、港口层级/材质/相机风险和正式树资源缺口；Q30=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。Q31 仍是最终视觉门禁，不要求用户让电脑连续运行，人工仅在最后主观画面质量无法由自动探针裁决时介入。

随后完整 D17 动作回归首次复现 A09 的 `14/15`：旧测试方向序列净方向非零，且直接反射调用在长时间步下放大了运动积分。修正为两组严格反向方向，并在 D2 地面/游泳运动中加入 `50ms` 单步上限后，A09 `1/1 PASS`，完整 D17 `15/15 PASS`（证据：`game/tools/results/LOCAL-QUALITY/Q30/Q30_D17_ACTION_REMEDIATION_R9.xml`）。该修复属于运动稳定性保护，不改变视觉门禁边界。

## 2026-09-09 Q30 R16 backdrop 隔离与原生路线复核

R15 复核继续显示水域中一个棕色空洞环。源码追踪将其归因到 R03 presentation-only `D19_R03_HarborBackdrop/BackdropRock_2`；该对象既无正式 offshore 资源契约，也没有可审计的接地依据。因此 R16 将 `CreateHarborBackdrop` 保持为空 root，避免未验证对象继续进入 Player；R03 rock/shrub cap 同时保留闭合双 winding containment。该卡没有扩大到 Systems、AgentCore、碰撞或玩法 owner。

R16 的定向 Python 回归为 `53/53 PASS`；D17 完整动作回归为 `15/15 PASS`（有效 XML：[Q30_D17_ACTION_REMEDIATION_R10.xml](../game/game/tools/results/LOCAL-QUALITY/Q30/Q30_D17_ACTION_REMEDIATION_R10.xml)，实际 `projectPath=game` 加相对路径造成 `game/game/...`，路径按事实登记）；R03 presentation rebuild 和独立非开发版 `Q30_CANDIDATE_PLAYER_R16` 均成功。真实 Win32 路线为 `48/48` 输入、`19` 截图、`167` observations、`123` swimming、`22` Ship control，船体位移 `5.745615m`、航向变化 `15.743011°`，route/business/pause/matrix 为 true 且无 fatal error。

R16 游泳截图不再出现 R15 识别的 backdrop 空洞环，但初始港口和水域仍有低模 fallback 角色、建筑/岸线层级与接地风险；正式动作/棕榈资源仍未准入。因此 Q30 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`。Q31 仍是最终 Player 视觉门禁；后续只运行有界探针，不要求用户持续开机。

## 2026-09-09 Q31 集中人工试玩包（WAITING_VISUAL）

Q30-R16 已被指定为当前唯一候选 Player，路径、SHA256、构建日志和真实输入证据均登记到 `game/tools/results/LOCAL-QUALITY/Q31/RESULT.json`。新增 `Q31_PLAYER_PLAYTEST_CHECKLIST.md`，将 10—15 分钟操作拆为：进入港口、行走/停下/转向、进水/回岸、E 登船/下船、W/S/A/D 船控、远船拒绝、I/M/T 重复与 Esc 恢复、保存/Continue。每个问题都要求记录步骤、位置、最后输入、可重复性和截图/录屏路径。

本卡状态明确为 `WAITING_VISUAL` 而非 PASS：自动化前置和 R16 真实路线已通过范围内检查，但用户已经观察到动作扭曲、游泳/船控表现和港口悬空/穿插风险，且 Q03 正式动作资源与 Q20 正式树资源仍未准入。没有人工观察结果前不升级 VC-G01，不进入 VC-G02；用户也不需要让电脑连续运行，完成清单后即可退出 Player。

## 2026-09-09 Q21-A01 房屋根节点接地修复与 R17 复核

- 根因：R03 `CreateHouse` 的 foundation 底面相对 root 约为 `0.02m`，旧实现给四个服务建筑统一使用 world `y=0.22`；R03 岛面由 `Height(x,z)` 产生起伏，导致部分建筑漂浮或嵌地。
- 修复：`D19R03HarborPresentationBuilder` 现在使用 `Height(position.x, position.z) - HouseFoundationBottomOffset`，其中 `HouseFoundationBottomOffset=0.02f`；只改 presentation builder，不写入 Systems、AgentCore、碰撞或领域事实。
- 验证：定向 Python `55/55 PASS`；R03 presentation rebuild R8 成功；独立非开发版 R17 `BuildOptions.None` 构建成功；真实 Win32 路线为 `48/48` 输入、`19` 截图、`167` observations、`123` swimming、`22` Ship control，船体最大位移 `5.9082966m`、航向变化 `17.7652588°`，route/business/pause/interaction matrix 为 `true`，fatal `null`。
- 边界：R17 截图仍显示低模角色/动作和游泳表现、登船后人物与舵位的可读性、港口空间层级以及正式植被资源风险；`ShoreBoundary` 在 180 帧 bootstrap 内未出现的警告仍保留。因此 Q21-A01=`PARTIAL`、Q31=`WAITING_VISUAL`、VC-G01=`IN_PROGRESS`，不解锁 VC-G02/VC-G03。
- 交接：权威结果为 `game/tools/results/LOCAL-QUALITY/Q21-A01/RESULT.json`，最新候选为 `game/tools/results/LOCAL-QUALITY/Q21-A01/Q21_A01_CANDIDATE_PLAYER_R17.exe`；下一轮必须选择单一表现 owner 继续，不做跨模块坐标盲改。

## 2026-09-09 Q21-A02 ShipDeck 镜头可读性与 R18 复核

- 根因：R17 真实 Player 已经进入 Ship 控制，但 ShipDeck `distance=10.5m`、`pitch=22°`、`targetOffset.y=1.45m` 使低模人物在桅杆/船帆和港口建筑之间过小，用户难以理解“人物正在操舵”。
- 修复：`CameraFollow` 将 ShipDeck 取景收紧为 `7.5m / 18° / targetOffset.y=1.05m`，仍以 characterTarget 为主体并保持受控船体相机自碰撞排除；不改船控事实、登船锚点或物理。
- 验证：`test_camera_follow` + R03 builder/probe Python `42/42 PASS`；R18 非开发版 `BuildOptions.None` 构建成功；真实 Win32 路线为 `48/48` 输入、`19` 截图、`168` observations、`123` swimming、`22` Ship control，船体最大位移 `5.5561728m`、航向变化 `15.1758118°`，route/business/pause/interaction matrix 为 `true`，fatal `null`。
- 画面结论：R18 船上截图中人物与舵位关系比 R17 更可读，但角色仍为低模 fallback，游泳姿态、动作质量、港口层级/接地和正式树资源仍未收口。因此 Q21-A02=`PARTIAL`、Q31=`WAITING_VISUAL`、VC-G01=`IN_PROGRESS`，不解锁 VC-G02/VC-G03。
- 交接：权威结果为 `game/tools/results/LOCAL-QUALITY/Q21-A02/RESULT.json`，最新候选为 `game/tools/results/LOCAL-QUALITY/Q21-A02/Q21_A02_CANDIDATE_PLAYER_R18.exe`；下一张卡只选动作/游泳/植被中的一个 owner。

## 2026-09-09 Q21-A03 fallback 游泳表现与 R20 真实路线复核

Q21-A03 只在 D2 character presentation/CameraFollow owner 内调整 fallback 游泳表现：使用有界交替 stroke 增加上肢/下肢可读性，并把 Swimming 相机收紧到更接近水面主体；没有猜测未知 imported rig 的骨骼轴，也没有修改 D2 物理根、Systems、AgentCore 或水体裁决。

- 定向 Python 回归为 `80/80 PASS`；R20 独立非开发版 Player 使用 `BuildOptions.None` 构建成功。
- 真实 Win32 路线为 `48/48` OS 输入、`19` 截图、`167` observations、`69` moving、`123` swimming、`23` Ship control；船体最大位移 `5.6351428m`、航向变化 `14.6616211°`，route/business/pause/interaction matrix 均为 true，fatal error 为 null。证据见 `game/tools/results/LOCAL-QUALITY/Q21-A03/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q21_A03_R20_20260906.json` 和 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN72.json`。
- R20 截图确认游泳逻辑已进入并持续移动，但角色仍是低模程序化 fallback，泳姿仍不达到正式动作质量；船上人物与轮舵的手部关系仍不够清晰；正式棕榈/树资源仍未通过 D18 ready/eligible gate；港口层级/接地和材质风险仍在。因此 Q21-A03=`PARTIAL`、Q31=`WAITING_VISUAL`、VC-G01=`IN_PROGRESS`，不解锁 VC-G02/VC-G03。

本轮同时修复了 Q15 当前源码的市场重开缺陷：R7/R8 的失败证据保留，D29 owner-close 防重状态修复后 R9 为 `1/1 PASS`；这只推进前置逻辑，不改变视觉门禁。后续仍采用一次启动、一次路线、自动退出的有界 Player 探针，不要求用户持续开机。

## 2026-09-09 Q21-A04 轮舵表现可读性与 R24 真实路线复核

Q21-A04 只在 D19 R03 presentation builder 内处理“上船人在控制瓢/舵位不可读”。候选 `ship-pirate-medium.fbx` 没有 helm/rudder/steering/wheel 语义节点；D7 `HelmPoint`、`D7_VisibleHelmAnchor`、D2/D7 船控、碰撞和 Systems 所有权保持不变。R21 的开放圆柱、R22 的封口盘和 R23 的移出遮挡尝试均由真实截图保留为中间失败证据；R24 改为带轮圈、四根轮辐和中心轮毂的可见轮舵并抬至人物手部高度。

- 定向 Python 回归为 `85/85 PASS`；R03 presentation rebuild 成功；R24 非开发版 Player `BuildOptions.None` 构建成功，Unity 日志包含 `Build Successful` 和 `Build Finished, Result: Success.`。
- 扩展契约回归为 `95/95 PASS`；同步现行 D29 输入契约和 D35 R76 renderer baseline 后，全仓库 Python unittest 第二次运行 `1133/1133 PASS`（`271.510s`）。这只证明代码/审计契约一致，不改变视觉 `PARTIAL` 边界。
- Q26 舵位 PlayMode `1/1 PASS`，确认 `mode=Ship`、玩家与 gameplay helm 距离 `0.0200m`、可见 `D7_VisibleHelmAnchor` 存在，并能正常下船恢复 Character。
- R24 真实 Win32 路线为 `48/48` OS 输入、`19` 截图、`167` observations、`69` moving、`123` swimming、`22` Ship control；船体最大位移 `5.6047745m`、航向变化 `15.3561096°`，route/business/pause/interaction matrix 均为 true，fatal error 为 null。证据见 `game/tools/results/LOCAL-QUALITY/Q21-A04/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q21_A04_R24_20260906.json` 和 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN76.json`。
- R24 board 截图已能读成轮圈/轮辐/轮毂，不再是 R21/R22 的不可见轮舵或 R23 的实心黑盘；但角色仍是低模程序化 fallback，泳姿、人物手部接触、港口整体层级和正式树资源仍未达到最终视觉门禁。

因此 Q21-A04=`PARTIAL`、Q31=`WAITING_VISUAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。R24 作为当前 Q31 候选保留，不再继续盲调舵位坐标；后续只按用户仍能复现的动作/游泳/植被缺陷选择单一 owner，或等待正式资源解除 `BLOCKED_ASSET`。Player 探针继续使用一次启动、一次路线、自动退出的有界方式，不要求用户持续开机。

## 2026-09-09 Q03-A01 fallback 游泳 containment 与 R25 复核

针对用户报告的“任务动作扭曲、海洋不能游泳”，先完成不依赖正式动画资产的最小 owner 修复：D2 程序化 fallback 的 normalized visualRoot 从旧 `swimBodyDrop=-0.88m` 收紧到 `-0.65m`，并用 D17 A08 高度边界阻止角色再次沉入水面。物理玩家根、游泳状态、Systems 裁决和正式动作资源门禁均未改变。

- 定向 Python `50/50 PASS`；D17 A08 `1/1 PASS`；D17 全量 `15/15 PASS`；全量 Python `1134/1134 PASS`（`316.090s`）。
- R25 `BuildOptions.None` 非开发版 Player 构建成功，真实 Win32 route probe 为 `48` 输入、`19` 截图、`167` observations、`70` moving、`123` swimming、`22` Ship control；船体最大位移 `5.6344452m`、航向变化 `15.9163208°`，route/business/pause/interaction matrix true、fatal error null。
- R25 截图证明水中角色不再完全沉入水下，但仍是低模程序化 fallback；正式泳姿、人物手部—舵轮关系、港口层级和正式树资源仍未通过。因此 `Q03-A01=PARTIAL`、`Q03=BLOCKED_ASSET`、`Q31=WAITING_VISUAL`、`VC-G01=IN_PROGRESS`，不解锁 VC-G02/VC-G03。
- 证据：`game/tools/results/LOCAL-QUALITY/Q03-A01/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q03_A01_R25_20260906.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN77.json`。

## 2026-09-09 Q20-A01 嵌套 legacy tree containment 与 R26 复核

针对用户报告“树在空中飘”，本轮先修复一个确定的 presentation 隔离漏检：D26 旧实现只检查场景根节点，当前 `InitialHarbor` 可能把 `D19_SaintMargaret_Root` 嵌在 `D19_EnvironmentRoot` 下。`D26HarborEnvironmentRuntime` 现在遍历每个场景根的 Transform 子树，找到该 legacy root 后禁用其全部 Renderer；不写 Collider、NavMesh、Systems 或玩法状态。

- 定向 Python `50/50 PASS`；真实 Unity PlayMode `Q20_A01_LEGACY_TREE_GROUNDING_R1.xml` 为 `1/1 PASS`，日志含 `activeLegacyTreeRenderers=0`、`activeR03Vegetation=24`、`unsupportedR03Vegetation=0`。
- R26 非开发版 Player `BuildOptions.None` 构建成功；真实 Win32 路线 `48` 输入、`19` 截图、`167` observations、`69` moving、`123` swimming、`22` Ship control，船体位移 `5.7648530m`、航向 `16.1363525°`，route/business/pause/interaction matrix true，fatal null。
- R26 截图仍暴露低模 fallback 角色、正式泳姿和人物—轮舵手部关系不足；正式树/棕榈资源仍未通过 ready gate。因此 Q20-A01=`PARTIAL`、Q20=`PARTIAL/BLOCKED_ASSET`、Q31=`WAITING_VISUAL`、VC-G01=`IN_PROGRESS`，不解锁 VC-G02/VC-G03。
- 权威结果：[Q20-A01 RESULT.json](../tools/results/LOCAL-QUALITY/Q20-A01/RESULT.json)；[Q20 handoff](design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)；[Q31 当前候选](../tools/results/LOCAL-QUALITY/Q31/RESULT.json)。

## 2026-09-09 Q21-A05 fallback 操舵姿态与 R27 原生路线

针对“上船人在控制瓢，应为舵”，本轮先处理一个可由当前 D2 presentation owner 证明的根因：`D2ControlRouter` 位于 `D2RuntimeBootstrap` 宿主，角色动画层此前没有收到 `Ship` 模式通知。现在 router 在模式切换后调用 `D2CharacterAnimationController.SetShipControlActive`，只让程序化 fallback 进入对称操舵姿态，不修改 D7 gameplay `HelmPoint`、船控裁决、碰撞、Systems 或正式 imported rig。

R1 因错误路由查找失败，R2 因手部到轮舵 `0.9957m` 超过 `0.85m` 失败；两次失败 XML/log 保留。R3 `Q26A05ProceduralFallbackReachesVisibleHelmInShipMode` 为 `1/1 PASS`，左右手到轮舵距离均为 `0.4114m`。定向 Python `57/57 PASS`，改动后的 D17 动作回归 `15/15 PASS`，全量 Python `1141/1141 PASS`（`280.227s`）。

R27 `BuildOptions.None` 非开发版 Player 构建成功。真实 Win32 路线为 `48` 输入、`19` 截图、`168` observations、`69` moving、`124` swimming、`21` Ship control；船体位移 `5.8478036m`、航向变化 `17.3246765°`，route/business/pause/interaction matrix true，fatal null。证据见 `game/tools/results/LOCAL-QUALITY/Q21-A05/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q21_A05_R27_20260906.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN79.json`。

视觉结论仍为 `PARTIAL`：截图显示轮舵局部可读性和 fallback 手臂朝向改善，但仍暴露低模任务动作、游泳时身体/四肢碎片或侧躺、自然握舵不足、树木/植被资源缺口以及港口漂浮/穿模风险。因此 Q21-A05=`PARTIAL`、Q31=`WAITING_VISUAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`。下一步按单一 owner 处理 Q03 正式动作/Avatar 阻塞或游泳、植被、漂浮/穿模缺陷，不再盲调舵位坐标。用户不需要持续开机；R27 仍是一轮启动、输入、截图、自动退出的有界流程。

## 2026-09-09 Q05-A01 fallback 游泳 presentation 与 R28 原生路线

针对最新 Player 反馈“任务动作扭曲、海洋不能游泳、上船人在控制瓢/舵、树在空中飘”，本轮只在 D2 presentation owner 内完成可证实的 fallback 游泳修正，并用 R28 重新跑完整原生输入路线。游泳摆幅由大幅手臂/腿部反向摆动收窄，移除横向 roll，保留约 `64°` 的前倾和 `-0.62m` 的 visualRoot 下沉；物理玩家根仍 upright，离水恢复不变。Q21-A05 的对称操舵通知和 Q20-A01 的 nested legacy tree Renderer containment 继续保留。

- 定向 Python `53/53 PASS`；全量 Python `1141/1141 PASS`；D17 游泳目标 PlayMode `1/1 PASS`。
- R28 `BuildOptions.None` 非开发版 Player 构建成功，Player SHA256 `D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，内置 `Assembly-CSharp.dll` SHA256 `2398E194424306871C4D07E6B251C096892CC43ED1E8153F81386FDC2B44CFF0`。
- 真实 native Win32 路线：`48` 输入、`19` 截图、`167` observations、`70` moving、`123` swimming、`22` Ship control；船体位移 `5.8326859m`、航向变化 `17.1057129°`，scene/route/business/pause/interaction matrix true，fatal null。证据见 `game/tools/results/LOCAL-QUALITY/Q05-A01/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q05_A01_R28_20260906.json` 和 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN80.json`。

视觉结论仍为 `PARTIAL`：R28 截图未达到正式动作/游泳质量，角色仍是低模 fallback，握舵自然接触、树木/植被资源、港口漂浮/穿模风险未收口。`Q03=BLOCKED_ASSET`、`Q20=PARTIAL/BLOCKED_ASSET`、`Q05-A01=PARTIAL`，`Q31=WAITING_VISUAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`。下一步转向 Q03 合法动作/Avatar 适配或明确 D20 candidate-only 视觉集成，不再盲调有限 fallback 坐标。

## 2026-09-09 Q05-A02 / Q05-A03 最新执行结果

Q05-A02 修复了程序化 fallback 游泳前臂的确定性 child-space 折叠，并以 Q26 水域专项和 R29 真实 Win32 路线复核。定向 Python `56/56 PASS`、全量 Python `1143/1143 PASS`、水域专项 `1/1 PASS`；R29 为 `48` 输入、`19` 截图、`168` observations、`37` swimming、`22` Ship control。视觉仍为 `PARTIAL`，因为截图不能证明正式动作质量。

Q05-A03 修复 D7 游泳视觉落地的 stale grounded 偏移，校准当前舵位测试契约，并构建 R31 完成有界原生复核。Q26 `6/6 PASS`、D17 `15/15 PASS`、R31 路线 `48` 输入、`19` 截图、`164` observations、`72` moving、`122` swimming、`21` Ship control；业务/暂停/交互矩阵为 `true`，fatal 为 `null`。R31 截图仍显示低模游泳/动作、舵位手部关系以及港口/植被视觉债务，故 `Q31=WAITING_VISUAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 不变。

这两张卡证明了可复现的代码级缺陷已被针对性处理，也证明真实输入路线没有被破坏；它们不替代 Q03 的合法动作/Avatar 资源准入、Q20 的正式植被资源准入或 Q31 的最终视觉验收。后续停止盲目调角度，转资源/适配器门禁或明确的 candidate-only 视觉集成任务。

## 2026-09-09 Q00/Q01 当前复核与 Q04 R9 水域回归

当前证据链已从 R27/RERUN58 统一到 Q05-A03/R31：真实 Win32 路线进入 InitialHarbor，48 输入、19 截图、164 observations、72 moving、122 swimming、21 Ship control，船体位移 5.9478812m、航向变化 17.8417969°，fatal null。R31 画面仍为 PARTIAL，低模动作/泳姿、自然握舵、港口层级和植被视觉债务未收口；VC-G01 继续 IN_PROGRESS，VC-G02/VC-G03 继续 LOCKED。

Q04 现源码回归过滤器 `D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles` 已通过：`Q04_WATER_TEN_CYCLES_PLAYMODE_R9.xml` total=1/passed=1/failed=0/inconclusive=0/skipped=0，Unity code 0 退出。R9 日志证明浅水和深水重复进入、移动、退出以及 grounded recovery，并记录逻辑水位 `surfaceY=-0.02`；这只升级 D2 水域逻辑边界，不升级原生 Player 视觉或最终泳姿。

下一步保持依赖顺序：Q03 合法动作/Avatar 资源仍是 BLOCKED_ASSET；Q05/Q08 做代码级整合，Q09 做握舵/船体测量，Q20 做正式植被 pivot/资源准入，Q31 等待最终人工视觉门。不得将 Q04 的 PlayMode PASS 或 R31 路线存在写成完整游戏完成。

## 2026-09-10 用户实际试玩否决与 Q13-A01 输入归属修复

用户已实际进入当前 R31 Player，报告任务动作扭曲、海洋不能游泳、上船人物在控制错误物体/舵位关系错误、树在空中飘；此前确认的 W/S 移动时右上角 HUD 菜单跟随切换也登记为真实输入冲突。Q31 因此为 `PARTIAL`，人工视觉门禁为 `REJECTED_ON_REPORTED_DEFECTS`；VC-G01 保持 `IN_PROGRESS`，VC-G02/VC-G03 保持 `LOCKED`。

Q13-A01 的最小修复是让 `HarborInteractionStabilizer` 在 InitialHarbor 玩法态清空 `InputSystemUIInputModule.move`，在暂停、仓库、市场、酒馆或 D16 模态态恢复默认动作；D17-A15 `1/1 PASS`，并已重新构建 R31 Player。原生 Win32 复核本轮未被探针观察到进入 InitialHarbor，`runtime_route_proof=false`；对照 Editor D17A06 `1/1 PASS`，当前只登记为输入尝试/转场未观察，不升级 VC-G01。下一步诊断点击/菜单状态/Player 路由边界，再按 Q03 正式动作/Avatar、Q05 游泳、Q21 舵位、Q20 植被单一 owner 继续收口。

## 2026-09-10 Unity Pipeline 接入后的有界开发回归

`com.unity.pipeline@0.6.0-exp.1` 已接入项目；本轮用记录 PID 的后台 Unity 6000.0.80f1 Editor 连接 `127.0.0.1:7800`，不接管用户桌面，也没有启动 standalone Player。

- 首次 Pipeline InitialHarbor Play Mode 暴露真实生命周期缺陷：普通 batchmode 错误安装 `D3RuntimeSmokeProbe`，`markers=0` 后主动退出 Editor。最小修复是让 `D3RuntimeSmokeProbe` 仅在显式 `-d3Smoke` 且不带 `-d4Smoke/-d5Smoke` 时安装。修复后同场景等待 18 秒仍为 `playing`，运行时 `eval=NO_D3_PROBE`，无编译错误匹配。
- `D16A06MainMenuBatchPlayerDoesNotInstallHarborSmokeProbe` 异步 `1/1 PASS`；`D9A07...` EditMode `1/1 PASS`。同步 PlayMode HTTP 返回 `Total=0` 的 domain reload 结果不计通过，已改用 async + `test_status`。
- Pipeline 读取当前 `ph_ship_fishing` lossyScale=`(2.57,2.57,2.57)` 和 `D7_HelmPoint` 世界偏移=`(0,0.96,-1.47)`。发现 VC-G01 世界逻辑测试仍保留旧 `(0,1.1,-1.8)` 期望；只同步测试契约，未改 D7 船控生产逻辑。随后 `VcG01WorldLogicTests`=`12/12 PASS`，D17 Action Quality PlayMode=`15/15 PASS`。
- 证据：[Pipeline 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/UNITY-CLI/UNITY_CLI_PIPELINE_SESSION_20260910_RESULT.json)、[CLI 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/UNITY-CLI/UNITY_CLI_20260910_RESULT.json)、[VC-G01 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)、[Pipeline handoff](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/VISUAL-CLEAN/UNITY_CLI_PIPELINE_SESSION_20260910_HANDOFF.md)。

状态不升级：`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31=PARTIAL`。Pipeline 后台测试通过只证明代码/编辑器运行时边界，不替代用户已否决的动作、游泳、舵位、树木漂浮/穿模视觉验收，也不证明 standalone Player 的物理键盘路径。

## 2026-09-11 Unity Pipeline R7 Player 控制面与 Esc 模态归属回归

本轮把 Unity Pipeline 从“后台 Editor/PlayMode 能力探测”推进到一轮有界的隐藏 Windows Player 控制面回归：构建 `PIPELINE_QA_PLAYER_20260911_R7.exe`，通过 Player 内置 Pipeline 端口 `7801` 读取状态/Console，并用 `simulate_key`、`simulate_pointer` 和只读 `eval` 检查运行时事实。Player 启动、直接加载 InitialHarbor 后的输入路由和日志均有证据；新鲜隐藏 Player 的 `simulate_pointer` 没有完成 MainMenu→InitialHarbor 点击转场，因此该菜单指针路径保持未证明。QA Player 进程在每轮结束后按精确命令行校验并停止。

本轮先复现并修复了一个真实逻辑冲突：仓库、市场、酒馆等临时页关闭后，同一帧的 Esc 仍可能被 `HarborPauseController` 再次消费，造成“面板关闭并同时暂停”。暂停控制器现在在真正消费点尊重 `RuntimeHudCoordinator.PrimaryInputAllowed`；D29 仓库、D3 市场、D10 酒馆、D9 整备、D4 内容页在消费 Esc 时设置 `D7ControlCoordinator` 的同帧 consumed marker。D29 Q15 定向 PlayMode `1/1 PASS`，Pipeline recompile 无错误。

R7 `BuildOptions.Development + AllowDebugging` 构建成功，`totalErrors=0`、`totalWarnings=8`、BuildGuid=`6fbdbc5defdf4b5ea9ddcc71b2d733b4`；运行时 error console `0`。D29 Q15 为 `1/1 PASS`，随后 D17 过滤 PlayMode 为 `21/21 PASS`。控制面结果：W/S 移动、A/D 转向、E 远端拒绝、E 登船/重复 E 下船、I 仓库开关、M 市场开关、T 酒馆开关、临时页 Esc 关闭不暂停、普通 Esc 暂停/恢复均为 `PASS_WITHIN_SCOPE`。完整字段、输入序列、PID、BuildGuid、源码变更和配置恢复见 [UNITY_CLI_RUNTIME_QA_20260911_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/UNITY-CLI/UNITY_CLI_RUNTIME_QA_20260911_RESULT.json) 和 [运行交接](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/VISUAL-CLEAN/UNITY_CLI_RUNTIME_QA_20260911_HANDOFF.md)。

范围边界保持严格：`simulate_key`/`simulate_pointer` 是 Unity Input System 注入，不等于物理键盘/鼠标验收；本轮 Player 隐藏运行，没有取得截图，`runtime_status` 的 `DrawCalls=0` 不能作为画面结论。正式动作/Avatar、游泳姿态、舵轮手部接触、植被接地以及漂浮/穿模仍未解决。因此 `Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31 继续 `PARTIAL`，不能用本轮控制面证据提前解锁后续依赖。

QA 完成后已恢复并验证 Editor runtime `enableInBuilds=false/port=0`，Build settings `developmentBuild=false/allowDebugging=false/connectWithProfiler=false`。后续优先使用 Pipeline 做短时、有界的逻辑/交互回归；视觉缺陷仍需可用截图或一次短人工画面核验，按 Q03/Q05/Q20/Q21 单一 owner 继续收口。

## 2026-09-11 Q03 D20-A02-R02 隔离适配器门禁复核

本轮在 Unity Pipeline connected Editor 中增加了一个针对 R02 男性 Player appearance Prefab 的隔离 EditMode 契约测试，并完成 `6/6 PASS`；随后 D17 PlayMode 过滤回归为 `21/21 PASS`。测试覆盖 valid Humanoid Avatar、单 Animator、Idle/Walk/Run/Jump/Board/Helm 状态、9 个交互 Socket、Root Motion 禁止以及物理根不位移。

测试首轮直接把 R02 可视 Prefab 根当作物理根，真实发现 Animator 更新会把该根的 Y 从 `0.00` 改为 `0.08`；该失败被保留并作为集成边界，随后改成“独立物理根 + R02 可视子节点”后通过。结果见 `game/tools/results/LOCAL-QUALITY/Q03/Q03_D20_A02_R02_PIPELINE_RECHECK_20260911_RESULT.json`。

本轮只推进 D20-A02-R02 隔离兼容性，不修改 InitialHarbor、D2 正式 Player、Systems 或资源晋级状态。`Q03=BLOCKED_ASSET`、`Q08=BLOCKED`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31 相关项保持原状态；不能把隔离层通过扩大为动作扭曲、游泳、握舵、植被或漂浮/穿模已修复。

## 2026-09-11 Q20-A02 直挂 legacy palm placeholder Renderer 隔离

本轮在 Unity Pipeline 的 InitialHarbor PlayMode 前置审计中发现 5 个直接场景根 `ph_palm_tree`；样本 Renderer bounds `min.y=0.280`，其中两个与 R03 自有地形命中存在 `0.088m/0.292m` 的高度差，另外三个没有自有地形命中。D26 旧逻辑只隔离嵌套 `D19_SaintMargaret_Root`，现改为同时隔离 `D19_SaintMargaret_Root` 与 `ph_palm_tree` 子树；Q26/A03 同步扩大识别范围，避免直接根漏检。

Pipeline 重编译 `failed=false/errors=[]`；Q26/A03 `1/1 PASS`，随后 Q26 全类回归 `6/6 PASS`（W 游泳、连续水中运动、E 登/下船与远端拒绝、舵位和树/植被审计）；`activeLegacyTreeRenderers=0`、`activeR03Vegetation=24`、`unsupportedR03Vegetation=0`，error console `0`，Editor `ready/stopped`。结果见 `game/tools/results/LOCAL-QUALITY/Q20-A02/RESULT.json` 和 `docs/game/design/common/handoffs/LOCAL-QUALITY/Q20-A02/Q20-A02.md`。

Q20-A02 只登记为 `PASS_WITHIN_SCOPE`：不恢复 authored palms、不改变 D18 ready catalog、Collider/NavMesh/Systems 或 Player 视觉状态。`Q20=PARTIAL/BLOCKED_ASSET`、`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持不变；后台 Editor 无 GPU，未新增截图。

## 2026-09-11 Q21/Q22 当前源码 Pipeline 复核

Q20-A02 后，使用同一 Unity Pipeline Editor 重跑 Q21 港口空间专项和 Q22 船海天气专项：两者均为 `1/1 PASS`，error console `0`，Editor `ready/stopped`。Q21 结果见 `game/tools/results/LOCAL-QUALITY/Q21/Q21_PIPELINE_RECHECK_20260911_RESULT.json`，Q22 结果见 `game/tools/results/LOCAL-QUALITY/Q22/Q22_PIPELINE_RECHECK_20260911_RESULT.json`。

这两次复核只确认当前 R03 静态空间、海平面/吃水/材质/天气 owner 合同没有被本轮表现隔离破坏；Q21/Q22 仍为 `PARTIAL`，不解除 Q20/Q03 资源门禁，也不替代 Player 画面、物理键鼠、音效或性能验收。

## 2026-09-11 Q23 当前源码 Pipeline 复核

Q23 正式场景集成专项在同一 Pipeline Editor 中复核为 `1/1 PASS`，error console `0`，Editor `ready/stopped`。结果见 `game/tools/results/LOCAL-QUALITY/Q23/Q23_PIPELINE_RECHECK_20260911_RESULT.json`。

该证据只确认 InitialHarbor 当前 R03/D27/船 Prefab 唯一性、GUID/引用、关闭重开一致性、材质/脚本和水线合同；Q23 继续 `PARTIAL`，不解除动作/植被资源门禁或 Player 视觉、物理输入、音效、性能门禁。

## 2026-09-11 Q24 当前源码 Pipeline 复核

Q24 存档恢复专项在同一 Pipeline Editor 中复核为 `3/3 PASS`，error console `0`，Editor `ready/stopped`。结果见 `game/tools/results/LOCAL-QUALITY/Q24/Q24_PIPELINE_RECHECK_20260911_RESULT.json`。

该证据只确认陆地/出海存档、港口出生锚点和坏档事务保护；当前 `PlayerState` 仍无独立游泳状态/姿态字段，Q24 继续 `PARTIAL`，不解除 Player 视觉、输入、动作、游泳、植被、音效或性能门禁。

## 2026-09-11 Q25 当前源码 Pipeline 复核

Q25 首航投影和 InitialHarbor 路线在同一 Pipeline Editor 中复核：D30/D23 EditMode `2/2 PASS`、`D19R02A03RoutePlayModeTests` `2/2 PASS`，error console `0`，Editor `ready/stopped`。结果见 `game/tools/results/LOCAL-QUALITY/Q25/Q25_PIPELINE_RECHECK_20260911_RESULT.json`。

该证据只确认当前源码的阶段投影和已覆盖路线回归；Q25 继续 `PARTIAL`，不解除完整人工游玩、原生物理输入、动作/游泳/上船视觉、植被接地、音效或性能门禁。

## 2026-09-11 Q26 当前源码 Pipeline PlayMode 复核

Q20-A02 的 legacy palm placeholder Renderer 隔离后，完整 `D19R03Q26GameplayDefectPlayModeTests` 在同一 Pipeline Editor 中复核为 `6/6 PASS`，error console `0`，Editor `ready/stopped`。结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_PIPELINE_RECHECK_20260911_RESULT.json`；运行日志记录 A03 `activeLegacyTreeRenderers=0`、`activeR03Vegetation=24`、`unsupported=0`。

该证据只确认当前源码的 W/E 注入缺陷路径、连续游泳、可见舵锚点和旧树/植被审计；Q26 继续 `PARTIAL`，不解除 standalone Player 物理键盘、截图、正式动作资源、GPU/目标设备性能或 VC-G01 门禁。

## 2026-09-11 Q28/Q29 当前候选 Player 运行复核

Q28 使用当前源码通过 Unity Pipeline detached build 生成 `Q28-CANDIDATE-R3`，构建成功且 error console 为 `0`；结果见 `game/tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_RECHECK_20260911_RESULT.json`。该候选的构建证据不等于视觉验收。

Q29 在该候选上完成两条真实 Windows Player/Win32 输入路线：水上路线 `48/48` 输入、`19` 张截图、`166` observations，观察到 `124` 次 swimming 与 `21` 次 Ship control；远距离路线 `45/45` 输入、`17` 张截图、`123` observations，观察到 `81` 次 `ship_too_far` 拒绝；两次 Player 均 clean close。证据见 `game/tools/results/LOCAL-QUALITY/Q29/Q29_PLAYER_RECHECK_20260911_RESULT.json` 及其引用的 runtime summary/probe/screenshot 目录。

截图仍显示低模 fallback、泳姿/人物—舵位关系不足、港口空间几何与层级风险；本轮没有把截图升级为视觉 PASS，也没有覆盖 owner-not-ready 拒绝。Q29、Q31、VC-G01 继续保持 `PARTIAL`/`IN_PROGRESS`，VC-G02/VC-G03 继续锁定。

## 2026-09-11 Q30 当前候选独立复核

Q30 对本轮当前候选和证据链完成独立复核：定向 Python `75/75 PASS`，全量 Python `1144/1144 PASS`；Q28-CANDIDATE-R3 构建成功，Q29 水域/远距真实 Win32 路线与截图结果可追溯。结果见 `game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_CANDIDATE_RECHECK_20260911_RESULT.json`。

该复核只证明 Q20-A02 的双 legacy 树名隔离测试契约没有回归、当前候选可运行且远距拒绝有证据；截图仍暴露低模角色、游泳/握舵接触和港口几何/接地风险，owner-not-ready 路径仍未覆盖。Q30/Q31 保持 `PARTIAL`，VC-G01 保持 `IN_PROGRESS`，后续仍按 Q03/Q05/Q20/Q21 owner 处理正式资源与视觉门禁。

Q26 舵位专项随后用 Unity Pipeline 重跑为 `6/6 PASS`，记录 `HelmWheel=(0.00,1.72,-6.35)`、左右手分别为 `(-0.19,1.72,-6.35)` 与 `(0.19,1.72,-6.35)`，两手距离 `0.1927m`。该结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_PIPELINE_RERUN_20260911_RESULT.json`，只关闭坐标级诊断，不把自然握舵截图升级为视觉 PASS。

## 2026-09-11 Q31 当前候选视觉门禁复核

Q31 对 Q28-CANDIDATE-R3 的两条 Q29 真实 Win32 路线和截图完成工程复核。移动/游泳/船控/业务/暂停/远距拒绝的运行证据可追溯，但截图仍显示低模 fallback、程序化占位泳姿、人物—舵轮接触不足和港口几何/层级/接地风险；owner-not-ready 拒绝与正式动作/植被资源门禁仍未覆盖。结果见 `game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_CANDIDATE_RECHECK_20260911_RESULT.json`。`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持不变。

## 2026-09-11 Q29 R4 UI 导航隔离诊断

Q29 R4 使用当前源码构建独立 Windows Player，并以 Win32 harness 发送真实路线输入。探针新增只读 UI 归属字段后，`78` 个 gameplay W/A/S/D 样本全部确认 EventSystem 存在、`sendNavigationEvents=false`、`InputSystemUIInputModule.move=null`、当前选中控件为空，`gameplay_ui_navigation_suppressed=true`；冲突事件数为 `0`。结果见 `game/tools/results/LOCAL-QUALITY/Q29/Q29_UI_NAVIGATION_RECHECK_20260911_RESULT.json`。

该结果关闭的是“当前候选是否仍把游戏移动键交给右侧 HUD UGUI 导航”的运行诊断缺口，不是视觉门禁。R4 截图仍显示低模 fallback、游泳/握舵表现不足和港口几何层叠/接地风险，且 owner-not-ready 拒绝、领域幂等和目标设备性能仍未覆盖。Q29/Q31/VC-G01 保持 `PARTIAL/PARTIAL/IN_PROGRESS`，VC-G02/VC-G03 继续锁定；下一步选择单一视觉 owner-scoped 缺陷再改动。

## 2026-09-11 Q26 R3 海盗塔接地与港口前景隔离复核

本轮继续使用 Unity Pipeline connected Editor 和有界 native Windows Player 路线，处理三个相邻但边界明确的 R03 表现问题：运行时动态创建的 `D6_HarborFoam` 重复渲染、市场入口贴近初始镜头、以及此前位于 `(-15, 13)`、不命中 R03 地形的 `KenneyPirateTower` 候选物。修改只触及表现 owner 和回归测试：D6 泡沫保留 runtime root 但关闭重复 Renderer；市场入口移到 `z=4.2`；海盗塔移到 `(-12.8, 0.20, 11.8)`，并用 `Physics.RaycastAll` 明确选择 `D19_R03_HarborPresentation/D19_TerrainRoot`，避免误把同名 legacy terrain 当成通过证据。

直接证据：

- Q26 `D19R03Q26GameplayDefectPlayModeTests`：`9/9 PASS`；A07 确认 R03 FoamRing 保留且 D6 泡沫 Renderer 为 `0`，A08 确认市场入口离开初始镜头前景，A09 命中 R03 地形 `hitPoint=(-12.80,0.21,11.91)`。
- 静态 Python 回归：`test_q26_spatial_cleanup.py 4/4 PASS`、`test_vc_g01_runtime_probe.py 6/6 PASS`。
- 本轮最终全量 Python 回归：`1148/1148 PASS`；其中旧海盗塔坐标断言已同步到新的 R03 接地点后通过。
- R3 非开发版 Player：`Q26_PIRATE_PORT_GROUNDING_CANDIDATE_R3.exe`，`BuildOptions.None`，真实 Win32 route `-d19-r03-route-rerun84`，`48/48` 输入步骤、`19` 张截图、`167` observations、`70` moving、`123` swimming、`22` Ship control，业务/暂停/交互矩阵为 `true`，`fatal_error=null`，clean close。
- 结果、哈希、截图和日志：`game/tools/results/LOCAL-QUALITY/Q26/Q26_PIRATE_PORT_GROUNDING_RECHECK_20260911_RESULT.json`；截图目录 `game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q26pirateportr3_20260911_20260906/`。

截图复核只确认海盗塔不再以本轮定位到的无 R03 支撑位置进入泳水路线，并未解决整体港口画面：初始/操船截图仍有大块前景平台和建筑层叠，角色仍是低模 fallback，游泳仍读作直立而非正式泳姿。因此 `Q26=PARTIAL`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06 继续 `PARTIAL`；far-ship reject 和 owner-not-ready 拒绝也没有被本路线覆盖。

## 2026-09-11 Q03 candidate bridge boundary

Unity Pipeline 已验证一个显式 `-d20Candidate` 的 D20 R02 candidate-only bridge：候选安装、D2 物理根 ownership、root-motion 关闭和有限动作消费在 Editor/Player smoke 中均有范围内证据。该桥接的原生路线摘要为 `45` 个输入步骤、`17` 张截图、`1920x1080` client，业务交互与 Esc 恢复成立；但 RERUN85 探针未识别，movement/swimming/ship/far-reject 不计为 PASS，且候选视觉仍是低模抬臂/T-like 姿态，controller 没有 Swimming state。

因此本记录只作为 VC-G01 的补充边界证据，不改变 `VC-G01=IN_PROGRESS`、`VC-G02=LOCKED`、`VC-G03=LOCKED` 或 D31 `PARTIAL`。正式 Q03 consumer 仍为 `0`；候选桥接不能替代最终 Player 截图审查、人工验收或 D20/D19 正式资源门禁。结果详见 `game/tools/results/LOCAL-QUALITY/Q03/Q03_D20_R02_CANDIDATE_RUNTIME_RECHECK_20260911_RESULT.json`。

## 2026-09-11 Q03 generic clip-path adapter follow-up

Pipeline 只读审计确认本轮不是简单的窗口/坐标问题：D20 generated clips 是 generic path 曲线，R02 source Prefab 的 `Armature` 被 `Visual_player_visual_male_r02` 包裹，valid Humanoid Avatar 不会按这组曲线消费。candidate builder 因而只在 candidate 资源中展开 wrapper，bridge 显式使用 `avatar=null` 的 generic path adapter，并在 snapshot 后恢复 `AlwaysAnimate`；正式 R02 source、InitialHarbor 和 Systems 不变。

最终候选测试为 `1/1 PASS`，D20-A02 EditMode `6/6 PASS`，recompile `failed=false/errors=[]`，清空陈旧构建 timeout 后 Console error=`0`；native candidate smoke 记录 `45` 个输入步骤、`17` 张截图、`1920x1080`、generic adapter marker 和 clean close。候选 bridge 现在把 D2 `Swimming` 映射到现有 `Fish` generic presentation fallback，但没有正式 authored Swimming clip；原生截图仍有低模黑色/抬臂/T-like 姿态和港口层级问题，故不改变 `Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 或 D31 `PARTIAL`。

同一 connected Editor 的 D17 PlayMode 回归为 `21/21 PASS`，`failed=0/skipped=0/inconclusive=0`；它只证明 opt-in candidate bridge 未污染现有 D2 逻辑合同，不能替代 VC-G01 的原生连续路线与画面审查。

Candidate `SwimmingVisualSupported=true` 只代表候选 controller 有可消费的 Fish fallback，不能代表真实泳姿或原生水域路线已通过；RERUN85 probe 仍不识别新 flag，movement/swimming/ship/far-reject 继续保持未证明。

## 2026-09-11 Q03 candidate outfit rebind follow-up

在同一 candidate-only 边界内，Builder 将现有 D18 male-peasant outfit 的 4 个 SkinnedMeshRenderer 通过相对骨骼路径重绑定到展开后的 D20 R02 `Armature/root`，并移除 outfit 的第二套 Armature/LODGroup；candidate prefab 实际为 `7` 个 skinned renderer。该动作改善了候选角色的颜色与轮廓可读性，但没有改动正式 R02 source、InitialHarbor consumer 或 Systems。

Pipeline 证据为 recompile `failed=false/errors=[]`、D20-A02 EditMode `6/6 PASS`、candidate PlayMode `1/1 PASS`、Player build 成功、清空后 error console=`0`。最新 native smoke `1920x1080`、`45` 输入步骤、`17` 张截图、业务/Esc 恢复/clean close 成立；RERUN85 probe 仍未识别，movement/swimming/ship/far-reject 不升级 PASS。检查帧仍有低模抬臂/T-like 动作、登船帧浅色重复人物和港口几何/层级问题，因此 Q03=`BLOCKED_ASSET`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 不变。

## 2026-09-11 Q26/A10 船员出生点重叠修复

通过已连接 Unity Pipeline 对候选 Player 登船前的重复人物进行对象归属审计后确认，浅色额外人物不是 D20 candidate bridge 重复渲染，而是 InitialHarbor 场景根 `/ph_crew_male`；其原位置与玩家出生锚点约 `0.25m` 水平重叠。仅在表现/交互 owner 内用 Pipeline `set_transform` 将其移到 `(2.50,1.10,-2.95)`，再用 `save_scene` 保存场景；D2 船员交互范围 `3.5m` 保留。

新增 Q26/A10 真实 PlayMode 断言：玩家和出生锚点到船员至少 `1.25m`，且仍在 `3.5m` 交互范围内。当前 `spawn_to_crew=2.5125m`、`player_to_crew=2.5125m`；专项 `1/1 PASS`，完整 `D19R03Q26GameplayDefectPlayModeTests=10/10 PASS`，Pipeline 重编译 `failed=false/errors=[]`，error console `0`，Editor `ready/stopped`。证据为 `game/tools/results/LOCAL-QUALITY/Q26/Q26_CREW_ANCHOR_SEPARATION_PIPELINE_20260911.json`、`Q26_CREW_ANCHOR_SEPARATION_REGRESSION_PIPELINE_20260911.json`。

本卡只关闭 NPC/出生点空间重叠，不把它扩大为 native Player 视觉通过；低模/T-like 动作、直立泳姿、港口几何层级和正式动作/植被资源门禁仍开放。Q26/Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。

## 2026-09-11 Q26/A10 最新源码 Player R2 交互证据

本轮通过已连接 Unity Pipeline 重新编译并构建 `Q26_A10_SCENE_BOUND_CANDIDATE_R2.exe`，构建 `build_9d0db92d0afd` 成功，`BuildOptions.None`、`totalErrors=0`、`totalWarnings=8`。旧 R1 的 `.exe` 外壳哈希相同，但 Data 文件存在 10 个差异；R2 的 `Assembly-CSharp.dll` SHA256 为 `68ECE848EB112ABE9A8069B960ECE0704D28199D2CFF97EACCEFC05E5375563A`，因此 A10 场景和探针修复确实进入新候选。权威构建 JSON 为 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A10_SCENE_BOUND_CANDIDATE_R2_BUILD_RESULT.json`。

- 完整路线 `RERUN85`：`46/46` 输入、`17/17` 截图、`122` observations、`36` moving，最大速度 `3.2000002861`，转向、远船 `E` reject、I/M/T 重复和 Esc close/recovery、业务/暂停/clean close 均被只读 probe/日志记录。
- WaterProbe `RERUN86`：`49/49` 输入、`19/19` 截图、`166` observations、`123` swimming、`22` Ship control，船体位移 `5.9714837074m`、航向 `17.3263244629°`，movement/swimming/ship control proof 均为 true。
- 证据工具为支持不覆盖历史结果的新增路由编号，给 `D19R03PlayerRouteProbe` 登记了 `rerun85/rerun86` allow-list、截图前缀和 JSON 输出名；定向 Python `6/6 PASS`，Pipeline recompile `failed=false/errors=[]`，运行结束 Editor `ready/stopped`、error console `0`。首次 R1 因未登记 `rerun85` 而仅为 input attempt 的结果被保留为非通过证据，不与 R2 混用。

这次关闭的是最新源码候选的真实脚本 Win32 交互证据缺口，不是视觉门禁。截图仍显示低模/程序化角色和直立泳姿、船舵接触可读性风险以及港口几何/层级/接地风险；`Q26=PARTIAL`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持不变。后续选择单一视觉 owner 处理，不同时修改正式动作、游泳、舵位和植被。

## 2026-09-11 D35 静态回归基线同步

全量 Python 回归暴露 D35 R03 静态审计测试仍期待 `300 Renderer`，但当前 Unity Editor 权威审计为 `294 Renderer / 1 Collider / 40402 vertices`。已同步 `tools/d35_r03_surface_material_audit.py` 与对应测试，未改生产 Unity 资产、场景或运行时代码。

验证：D35 定向 `1/1 PASS`；全量 `tools/tests` `1148/1148 PASS`（`168.034s`）。结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_D35_RENDERER_CONTRACT_SYNC_20260911_RESULT.json`。本次不升级 Q26/Q31/VC-G01 状态，也不解锁 VC-G02/VC-G03。

## 2026-09-11 VC-G01 A12 full-flow Player evidence

已通过 Unity Pipeline 构建当前源码 `Q26_A12_FULLFLOW_FRAMING_CANDIDATE_R1.exe`，构建场景为 `MainMenu + InitialHarbor`，用于避免只打包 InitialHarbor 时 `CurrentShip` 未由 NewGame 创建的测试环境缺口。A12 完整 Win32 路线 `46/46` 输入、17 截图，水域路线 `49/49` 输入、19 截图，分别证明 movement/turning、E 远船拒绝/登船/下船、I/M/T 重复与 Esc 恢复、D10 owner-ready、swimming 和 ship-control 的有限运行范围；构建 0 errors，Q26 PlayMode `10/10`，Python `1148/1148`，Editor error console `0`。

本轮只接受 `CameraFollow` 船控取景收紧在有限范围内改善；不把 route proof、截图数量或逻辑 probe 代替正式动作资源、泳姿、港口几何、植被接地和最终视觉门禁。故 `VC-G01=IN_PROGRESS`、`Q26/Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED` 维持不变；后续仍按单一视觉 owner 形成下一候选，不解锁下游门。

## 2026-09-11 VC-G01 A16 R03 boundary bridge revalidation

### 本轮闭环

通过 Unity Pipeline 构建 `Q26_A16_R03_BOUNDARY_CANDIDATE_R1.exe`，并对真实 Windows Player 执行 `RERUN88` Win32 路线。D6 当前的真实物理边界是 `ShoreBoundary.South/.West/.East` 外围集合，而不是旧 bridge 等待的精确 `ShoreBoundary`；`D19R03HarborRuntimePresentation` 现在在确认 owned perimeter set 后保留外围边界在水域外，避免把旧 South wall 误放入游泳路径。新增 route probe allow-list 和对应 Python/Unity 契约测试，结果不覆盖历史 route 证据。

### 证据

- Build `build_319d0c427c0d`：`StandaloneWindows64 / MainMenu + InitialHarbor / 0 errors / 8 warnings`；结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A16_R03_BOUNDARY_CANDIDATE_R1_BUILD_RESULT.json`。
- 真实 Win32：`46/46` 输入、17 截图、122 observations、36 moving；移动、转向、游泳、E 远船拒绝、I/M/T/Esc、业务页、暂停恢复和 clean close 有真实 probe/日志证据；本轮 ship control proof=`false`。
- 日志：`route collision window retained: D6-owned R03 perimeter boundaries remain outside water`；legacy physical colliders disabled=`123`、non-blocking triggers preserved=`22`。
- 回归：Python `1150/1150 PASS`；Unity EditMode `619/619 PASS`；Q26 PlayMode `10/10 PASS`；VC-G01 world logic `13/13 PASS`；定向 contract `20/20 PASS`；recompile 无错误；Editor `ready/stopped`；error console=`0`。

### 不升级的门禁

A16 初始港口截图仍存在侧面建筑近景/裁切、低细节程序化玩家及港口几何/接地问题；因此本轮只标记 `PASS_WITHIN_SCOPE` 的运行交互和 boundary bridge，不升级 `VC-G01` 的视觉状态。`VC-G01=IN_PROGRESS`、`Q26/Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED` 保持不变。下一候选必须选一个单一 owner（正式动作/Avatar、泳姿或港口几何/接地），以新假设驱动修复和复核。

## 2026-09-11 VC-G01 A17 InitialHarbor framing candidate

根据 A16 实际 Player 帧和 Pipeline 空间测量，InitialHarbor 的 Market/Warehouse 可见根分别约在玩家左右 `6.4m`、宽约 `5m`；`13.5m` 首帧距离会使近侧立面贴到水平视锥边缘。A17 将 `CameraFollow` 的首帧距离调整为 `18.5m`，仅限 Character 初始构图，未改变建筑、碰撞、输入、船控或游泳 owner。真实 Win32 route 使用 `RERUN89`，不覆盖 A16。

证据：A17 build `build_d7a1d45e6e1e` 成功，`0 errors/8 warnings`；`46/46` 输入、17 截图、122 observations、36 moving，真实 movement/turning/swimming/business/pause/far reject/interaction matrix/clean close；Python `1150/1150`、Unity EditMode `619/619`、Q26 PlayMode `10/10`、相机 PlayMode `1/1`、VC-G01 world logic `13/13`、error console=`0`。A17 首帧相较 A16 让中心路线周围露出更多地面并减轻两侧建筑占屏，登记 `IMPROVED_WITHIN_SCOPE`；左下仍裁切，低模玩家、地面断缝/材质粗糙仍未解决。

因此本轮不把 bounded framing 改善升级为 VC-G01 视觉 PASS。`VC-G01=IN_PROGRESS`、`Q26/Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED`；下一步停止继续调首帧相机，转向正式动作/Avatar、泳姿或港口几何/接地的单一 owner。

## 2026-09-11 VC-G01 A20 游泳镜头跟随复核

### 有界变更

- `CameraFollow` 在游泳期间持续读取 D2 物理 heading 自动 yaw；真实鼠标/右摇杆 look 输入发生后，自动跟随停止并转为手动 look。
- CameraFollow 对 yaw/pitch、target/pivot/direction、碰撞距离和最终 position 增加有限值保护，防止 NaN/Infinity 写入 Camera Transform。没有改变 Systems、D2 物理根、船控或 formal animation owner。
- Q03 candidate-only PlayMode 测试重置 gameplay input/HUD ownership，修复全量顺序污染；RERUN92 仅是新 Player probe/screenshot/result namespace。

### 证据与视觉边界

- A20 build `build_3bba5aaa08b8`：MainMenu + InitialHarbor，StandaloneWindows64，0 errors/8 warnings；真实 Win32 WaterProbe `49/49` 输入、19 截图、167 observations、119 swimming、21 Ship control，船体位移 `6.00153923034668m`、航向变化 `17.986114501953125°`，业务/暂停/交互矩阵/clean close 成立。
- 回归：Python `1152/1152 PASS`；Unity EditMode `619/619 PASS`；PlayMode `99 passed / 0 failed / 1 skipped of 100`；Q03 targeted `1/1 PASS`；recompile 无错误；error console=`0`；Editor=`ready/stopped`。
- A20 水域截图已从旧的入水后直立读法改善为横向游泳读法，但低模程序化角色、建筑近景/裁切、岸线地面断缝和材质粗糙仍然存在。因此只记录 `IMPROVED_WITHIN_SCOPE`，不升级 VC-G01 视觉门。

### 依赖与下一步

`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持不变。下一轮应选择一个新的单一 owner（正式动作/Avatar 或港口几何/接地），为该缺陷写出可验证假设并生成独立 Player 证据；不得把 A20 的逻辑路线证据当作完整视觉 PASS。

## 2026-09-11 Q21-A06 fallback 前臂弯曲与全量回归边界

### 有界变更

- D2 `D2CharacterAnimationController` 的 Ship fallback 保留 `-35°` 对称上臂目标，增加 `-10°` 对称前臂 child-space 弯曲；不改变 D7 `HelmPoint`、D19 船体、物理、输入或 Systems。
- `Q26A05ProceduralFallbackReachesVisibleHelmInShipMode` 增加前臂非零弯曲断言；`D19R03PlayerRouteProbe` 为当前源码登记独立 `RERUN94` 输出，避免复用旧 Player 证据。

### 证据与边界

- Pipeline 运行时：玩家 `(0,0.96,-6.67)` 与 D7 HelmPoint 一致，双手在轮圈附近；正式 Animator 仍未就绪，故这只是 fallback presentation 修复。
- 新 Player `build_693e29d49f64` 成功，`0 errors / 1 warning`；RERUN94 真实 Win32 `46/46` 输入、17 截图、122 observations、60 swimming，业务/暂停/I-M-T-Esc/远船拒绝/clean close 有证据。
- 定向 Python `12/12`、全量 Python `1152/1152`、Q26/A05 `1/1`、Unity EditMode `619/619`；全量 PlayMode 实际 `98/100`，Q03 `W -> Walk` 失败、D19 terrain test 预期跳过；Q03 独立重跑 `1/1 PASS`。
- 舵轮截图中前臂局部改善，但仍为低模 fallback，不能通过自然握舵/正式动作视觉门禁；Q03=`BLOCKED_ASSET`、Q20=`PARTIAL/BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 不变。

### 下一步

停止继续盲调舵轮坐标和前臂角度。下一卡选择新的单一 owner，优先处理 Q03 正式 Avatar/动作资源，或处理可复现的港口几何/接地缺陷；同时解决 Q03 全量顺序失败后，才能重新评估全量回归门禁。

## 2026-09-11 Q03-A02 D20 candidate 移动首帧映射与全量确定性

全量 PlayMode 复现 Q03 失败后，诊断字段确认输入链没有被污染：D16 `axis=(0,1)`、`blocked=false`、`primary=true`、`gameplay=true`、W 为 pressed、D2 `IsMoving=true`、`Action=Walk`。失败只发生在 D20 presentation map：真实加速首帧 `snapshotSpeed01=0.041` 被 `Idle <= 0.05` 阈值截断，candidate 状态因此保持 Idle；隔离测试的 `1/1 PASS` 不能证明全量确定性。

本卡仅将纯 C# presentation map 的 Idle 阈值收窄为 `0.01`，保持 `0.65` 的 Walk/Run 分界，并增加 `0.01 Idle / 0.04 Walk` EditMode 边界测试；Q03 PlayMode 诊断字段保留用于后续真实失败定位。没有改 Systems、D16 输入 owner、D2 物理或正式动作资源。

证据：D20 EditMode `6/6 PASS`；Q03 targeted PlayMode `1/1 PASS`；Unity 全量 PlayMode `99 passed / 0 failed / 1 skipped of 100`（`209.81s`）；Pipeline recompile `failed=false/errors=[]`；error console `0`；Editor `ready`。D19 terrain 工作场景测试仍因显式 smoke 前置条件跳过，未伪造为 PASS。

状态边界：本卡为 `Q03-A02=VERIFIED_WITH_SCOPE`，只关闭 candidate 移动状态映射和全量确定性缺陷；`Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q31=PARTIAL` 保持不变。下一卡 `Q03-A03` 处理已由日志确认的 D27 restore 与 candidate 隐藏旧 Renderer 冲突（candidate 安装后曾恢复 `17` 个旧 Renderer），再考虑新的 Player 视觉证据。

## 2026-09-11 Q03-A03 D27 candidate Renderer 所有权

全量日志确认 Q03 candidate 安装后 D27 的通用 `RestoreRenderers(player)` 曾把 `17` 个旧 player Renderer 重新启用；这不是正式动作资源问题，而是两个 presentation owner 对同一 Renderer 集合的写入冲突，会造成重复人物和穿模风险。

本卡在 D27 内增加最小 owner guard：当 `D20PlayerCandidatePresentationBridge.IsInstalled=true` 时，D27 不恢复 player Renderer，只恢复 crew Renderer，并记录 `candidateOwned=true/restored=0`；没有 candidate 时保持正式 fallback 的恢复行为，并记录 `candidateOwned=false`。Q03 candidate PlayMode 增加一帧后旧 Renderer 仍 disabled 的回归断言。

证据：Pipeline recompile `failed=false/errors=[]`；Q03 targeted `1/1 PASS`，candidate 日志 `playerRenderers=24, restored=0, candidateOwned=True`；全量 PlayMode `99 passed / 0 failed / 1 skipped of 100`（`217.48s`）；formal fallback 日志 `playerRenderers=17, restored=0, candidateOwned=False`；error console `0`、Editor `ready`。D19 terrain work-scene test 仍因显式 smoke 前置条件跳过。

状态边界：本卡为 `Q03-A03=VERIFIED_WITH_SCOPE`，只关闭 candidate 与 D27 的 Renderer 所有权冲突；`Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q31=PARTIAL` 保持不变。下一卡 `Q03-A04` 才能在最新源码上重建 Windows Player，检查原生登船/游泳/重复人物截图；代码回归绿灯不能替代原生视觉证据。
## 2026-09-11 Q03-A04 RERUN95 最新 candidate Player 原生路线

### 有界变更

- 用 Unity Pipeline 重建 `Q03_A04_LATEST_CANDIDATE_PLAYER.exe`，只增加 D19 R03 route probe 的 `RERUN95` 独立输出、截图前缀和结果命名；不覆盖既有 RERUN92/RERUN94 证据，也未修改 Systems、D2 物理或正式 InitialHarbor consumer。
- 构建为 `MainMenu + InitialHarbor`、`BuildOptions.None`，Player SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。

### 运行与视觉证据

- 真实 Win32 RERUN95 有 `48` 个输入步骤、`19` 张截图、`166` observations、`123` swimming、`22` ship-control；船体位移 `6.058244m`、航向变化 `18.174286°`。WASD、转向、E、I/M/T、Esc、业务页和暂停恢复均有路线证据。
- `view_image` 抽查 InitialHarbor、登船、船控、游泳和仓库重复操作帧：没有看到 Q03-A03 所防护的第二个 legacy player Renderer，但候选角色仍为低模/T-like；Fish 姿态抬臂且不可信；手与舵轮关系弱；港口仍有浮空、裁切和粗糙几何。
- 因此本卡为 `Q03-A04=PARTIAL`，原生输入闭环不等于视觉 PASS。`Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED` 保持不变。远船拒绝和 owner-not-ready 路径未在此 WaterProbe 覆盖。

### 下一卡

`Q03-A05` 只修改 `D20A02AnimationBuilder` 生成的 Fish clip arm excursion，重建 candidate controller/resource，并用独立 `RERUN96` 重新检查游泳/登船截图；若视觉仍不达标，继续保留证据为未通过，不把 generic clip 伪装成正式动作资源。

## 2026-09-11 Q03-A05 Fish arm envelope 与 RERUN96

### 有界变更

- `D20A02AnimationBuilder` 将 candidate Fish upperarm 生成幅度从 `30°` 收紧到 `8°`，新增曲线边界 EditMode 测试，并通过 Pipeline 重新生成 source/controller 与 `Resources/D20/Candidate` controller。
- 增加独立 Q03-A05 Player 构建入口和 D19 route probe `RERUN96`，不覆盖 RERUN95。

### 证据与边界

- D20 EditMode `7/7`、Q03 candidate PlayMode `1/1`、全量 Unity EditMode `620/620`、全量 PlayMode `99 passed / 0 failed / 1 skipped of 100`（唯一 skip 为 D19 terrain work-scene 显式 smoke prerequisite）、全量 Python `1152/1152`、recompile 无错误；Fish 左/右 upperarm 曲线实测 `28.166→20.166`、`28.166→36.166`。
- RERUN96 真实 Win32 `48` 输入、19 截图、167 observations、120 swimming、21 ship-control；船体位移 `6.058783m`、航向 `18.267059°`。输入/状态闭环存在，但 `view_image` 仍显示游泳蜷曲/抬腿、登船/舵轮接触弱和港口几何问题。
- 本卡为 `Q03-A05=PARTIAL`；`Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED` 保持不变。远船 reject 与 owner-not-ready 未覆盖。

### 下一卡

`Q03-A06` 处理 candidate Fish whole-body visual orientation/pose owner，先用 preview/Editor 只读测量确定姿态，再以 RERUN97 做原生复核；不改物理根、不升级正式动作资源。

## 2026-09-11 Q03-A06 Fish whole-body pitch 与 RERUN97

### 有界变更

`D20A02AnimationBuilder` 为 candidate-only Fish clip 增加 visual child root 的恒定 `32°` pitch 曲线，并用 EditMode 断言确认 `localEulerAnglesRaw.x` 在 `0/.5/1s` 均为 `32°`。D2 physical root、Systems、正式动作资源和正式 InitialHarbor consumer 均未改变。RERUN97 仅增加独立 route/screenshot/result 命名空间。

### 证据

- Pipeline recompile=`failed=false/errors=[]`；D20 EditMode=`8/8`、Q03 candidate PlayMode=`1/1`、Python route contract=`6/6`；Editor=`ready/stopped`、error console=`0`。
- Player build job=`ac3663fd60524752998d91601300c7ca`，`MainMenu + InitialHarbor`、`BuildOptions.None`、Player SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。
- RERUN97 真实 Player `48` 输入、`19` 截图、`166` observations、`123` swimming、`21` ship-control，船体位移=`6.051456m`、航向=`18.186615°`；业务/暂停/交互矩阵和 clean close 成立，远船 reject/owner-not-ready 未覆盖。

### 不升级的门禁

原生截图确认曲线生效，但 Fish 仍前俯、抬腿并悬空；登船/船控手部接触和港口几何仍未达到视觉门。故 `Q03-A06=PARTIAL`、`Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED` 均保持。下一卡必须先做 Fish 运行时姿态应用链的只读诊断，禁止继续无新假设地调角度。

## 2026-09-11 Q03-A07 Fish generic axis contract 与 RERUN100

### 根因和有界修改

Unity Pipeline Editor eval 发现 R02 Fish candidate 的 `thigh_l/thigh_r` 休息 Y/Z 为 `180/180`，旧 generic clip 只写 X 轴，Animator 默认值会把未写入的 Y/Z 归零；脚的 y 值由约 `0.08` 变成约 `1.68`，形成此前原生画面中的抬腿、蜷曲和穿插。修复只在 `D20A02AnimationBuilder` 的 candidate generic clip contract 内完成：关闭 write defaults，显式写 root XYZ 和 limb X/Y/Z，Fish visual child 使用 `90°` pitch；不写 D2 physical root、不改变 Systems 或正式动作资源。

### 证据

- Pipeline recompile `failed=false/errors=[]`；D20 EditMode `9/9`；Q03 candidate PlayMode `1/1`；Unity 全量 EditMode `622/622`（0 failed/0 skipped/0 inconclusive）；Python route contract `6/6`；D20 controller audit `PASS`（15 个 state、每个 16 条显式曲线）；error console `0`。全量 PlayMode/Python 未在本卡后重跑。
- RERUN99 真实 Win32：`48` 输入、`19` 截图、`167` observations、`69` moving、`120` swimming、`21` ship-control；船体位移 `6.002009m`、航向变化 `18.224670°`。抬腿折叠失败消失，但身体仍斜向。
- RERUN100 真实 Win32：`48` 输入、`19` 截图、`166` observations、`67` moving、`109` swimming、`22` ship-control；船体位移 `5.819563m`、航向变化 `16.697662°`；真实移动、游泳、船控、业务交互、重复 I/M/T、Esc 关闭/恢复和 `gameplay_ui_navigation_suppressed_observed` 均成立。远船 reject、owner-not-ready reject 未覆盖，保持 false。

### 视觉门禁与后续

`90°` candidate Fish visual-child pitch 使 RERUN100 的角色读作横向，较 RERUN99 有真实改善；但入水/返岸仍有岸线穿插或悬浮读感，登船/舵轮接触弱，首帧/仓库仍有低模、浮空、裁切、前景侵入和粗糙几何。故 `Q03-A07=PARTIAL`、`Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED` 均保持。

下一步停止继续盲调 Fish pitch，选择 `D31-P04`、`D31-S05` 或 `D31-G06` 一个环境/接地 owner，先做有界空间诊断和最小修复，再用新的 Player route namespace 复核；不得将本卡逻辑路线证据升级为完整视觉 PASS。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A07/RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q03/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q03_A07_FISH_PITCH90_RERUN100_20260906.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN100.json`。

## 2026-09-11 Q03-A08 / D31-P04 R03 海岸剖面修复

Q03-A07 的 RERUN100 截图把下一步收窄到一个有界环境 owner：`D31-P04 / D19-R03 Harbor Presentation`。真实 Player 画面中，Fish 在外海边缘贴住一段从岛面高度直接垂直落到水下的条纹切面；源码 `AddIslandSkirt` 同时确认了该剖面和石材边材质。该诊断不是由截图臆测单独得出，而是由截图、R03 Builder 源码和 Prefab 生成测试共同闭环。

本卡只改表现层生成：64 段海岸由旧的四顶点竖直面改为六顶点的内边界 → `1.35m` 外扩沙质 shelf → `y=-0.42` 水下深边界，shelf=`y=-0.24` 以覆盖 authored perimeter 的最低高度；`D19_TerrainRoot` 仍是唯一 MeshCollider owner，并继续与可见 `D19_R03_IslandSurface` 共享 mesh。新增 `D19R03Q03A08ShorelineProfileTests` 锁定 `2884` vertices、shelf 宽度/高度、`R03_Sand` 材质和 shoreline 无额外 collider。

Unity Pipeline 证据：显式 recompile `failed=false/errors=[]`；Q03-A08 `1/1 PASS`；Q20/Q21/Q22/Q23 分别 `1/1 PASS`；Q24 `3/3 PASS`；Q25 `2/2 PASS`；Q26 `10/10 PASS`；最终 Editor `ready/stopped`，清理后 error console=`0`。第一次测试针对未重建的 `2756` 顶点 Prefab 正确失败，重编译并重新执行 R03 Builder 后才对 `2884` 顶点 Prefab 通过，保留该失败边界以避免固定 token 假通过。

本卡仍为 `PARTIAL`：对连接 Editor 执行 `editor_play/editor_stop` 成功，但 `capture_game_view --source screen` 明确返回 `No GPU available (batchmode/headless); cannot capture.`，因此没有把 post-fix 画面升级为视觉 PASS。`D31-P04=PARTIAL`、`D31-S05=PARTIAL`、`D31-G06=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持不变。下一步需要 GPU-capable Player/Editor 补一张海岸路线 post-fix frame，再按依赖选择下一个单一 owner；不得把 Q26 PlayMode 或 Q03-A08 静态契约扩大成完整视觉验收。

权威结果与交接：`game/tools/results/LOCAL-QUALITY/Q03-A08-D31-P04/RESULT.json`、`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A08-D31-P04/Q03-A08-D31-P04.md`。

## 2026-09-11 Q03-A11 / VC-G01 船向根因修复与真实 Player 复核

本轮通过连接的 Unity Pipeline 完成源码重编译、D19-R03 表现重建、Q26/D17/定向 EditMode 回归和当前源码 Windows Player 构建。诊断确认 `D19R03HarborPresentationBuilder` 旧的可见船体 `180°` child rotation 与 D2 `heading=0` 的 `+Z` 运动轴相反；这解释了真实画面中 W/A 将船推入港口建筑的逻辑性问题。最小修复为稳定船根 `180°`、候选可见 child `identity`、舵轮世界位置补偿，并在 `D2ControlRouter` 中让登船玩家面向舵轮；没有改 Systems 的航行裁决。

验证证据：`build_3e69fa3e4248` 为 `StandaloneWindows64 / MainMenu + InitialHarbor / 0 errors / 8 warnings`；Q26=`11/11`、D17=`16/16`、VC-G01 world logic=`13/13`、Q22=`1/1`、Q23=`1/1`；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`、error console=`0`。真实 Win32 route `Q03A11_SHIP_FORWARD_ALIGN_R3` 为 `48/48` 输入、19 截图、167 observations，69 moving、4 swimming、21 ship-control，船体位移 `6.005729m`、航向变化 `18.372559°`；Pipeline eval 的 ship/candidate forward dot=`1.000`，原生 W/A 后船向水域。

本轮两组相机假设（侧向 `90°`、斜向 `-45°`）均被原生截图拒绝并回滚，避免把镜头微调当作船向根因修复。当前原生截图只登记为 `IMPROVED_WITHIN_SCOPE_NOT_VISUAL_PASS`：低模程序化角色、泳姿、舵轮手部接触和港口岸线/地面几何仍未达视觉门禁。状态保持 `VC-G01=IN_PROGRESS`、`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`D31-P04/D31-S05/D31-G06=PARTIAL`、`VC-G02/VC-G03=LOCKED`。权威结果与交接：`game/tools/results/LOCAL-QUALITY/Q03-A11/RESULT.json`、`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A11/Q03-A11.md`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

下一步按依赖继续：只选择一个 D31 环境/接地 owner 做有界修复与新的 Player route namespace；正式 Avatar/Animator 资源未合法准入前，不关闭 Q03 asset gate，也不推进 VC-G02/VC-G03。

## 2026-09-12 Q26-A12：R03 接地 owner 与 VC-G01 互补运行证据

### 已执行

- R03 Builder 现在从其自有 `D19_TerrainRoot` MeshCollider 读取实际地形高度，再按 Renderer bounds 接地 presentation props；移除没有 R03 terrain 命中的 cliff 候选。
- 新增 Q26A12R03 接地回归，并让 route probe 使用独立 rerun101/rerun102 namespace。
- 通过 Unity Pipeline 重编译、重建 R03 presentation、执行 Q26 全量 PlayMode，并构建当前源码 Windows Player。
- 对同一 Player 执行两条互补 native Win32 路线：正常路线保留远船拒绝证据，WaterProbe 路线保留船体移动/转向证据。

### 证据与状态边界

- Q26A12R03=`1/1 PASS`；完整 Q26=`12/12 PASS`；Player build=`build_6cc1e97f82cc`，`0 errors / 8 warnings`；Editor=`ready/stopped`，error console=`0`。
- Python 定向契约=`37/37 PASS`；全量回归=`1152/1152 PASS`。D2 source-contract test 已同步接管前 Q03-A10 源实现，未改变 gameplay source。
- R1=`45/45` 输入、17 截图、122 observations、37 moving、51 swimming、远船 reject=`true`；船控移动段未执行，ship proof=`false`。
- R2=`48/48` 输入、19 截图、167 observations、70 moving、2 swimming、21 ship-control；船位移=`5.94493293762207m`、航向=`18.02655029296875°`；WaterProbe 不覆盖远船 reject。
- 当前截图仍有低模/程序化人物、泳姿与舵轮接触弱、岸线/地面几何粗糙等视觉问题；owner-not-ready、正式 Avatar/Animator、Q20 资源门禁未关闭。

因此 `VC-G01` 仍为 `IN_PROGRESS`，`VC-G02/VC-G03` 仍为 `LOCKED`，D31 各视觉任务仍为 `PARTIAL`。本卡只证明接地 owner 和互补交互证据，不把真实运行证据扩大为完整视觉 PASS。权威结果为 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A12_PRESENTATION_GROUNDING_RESULT.json`，交接为 `docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26-A12.md`。

## 2026-09-12 Q03-A12 正式 Avatar / 动作资源契约审计

Q26-A12 之后没有继续盲调角色坐标；本轮改用已连接的 Unity Pipeline 对 Q03 的资源准入做只读审计，并增加 `Q03FormalAvatarActionResourceContractTests`。审计确认 D18 R03 project-owned male Avatar 为 `isValid=true / isHuman=true`，含 20 个 human bones 与 70 个 skeleton entries；D20 R02 formal appearance 具有单一有效 Humanoid Animator；D20 Controller 具有 15 个动作状态、15 个真实 Clip、6 个参数。展平后的 Resources candidate 继续明确使用 `Animator.avatar=null` generic-path 契约，所有 D20 曲线路径均落在同一个直接 `Armature` 层级。

验证：资源契约 EditMode `2/2 PASS`；D20-A02 EditMode `9/9 PASS`；Q03 candidate PlayMode `2/2 PASS`；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`；error console=`0`。本卡只增加测试和结果留痕，没有修改 `Systems/`、D2 physical root、InitialHarbor formal consumer 或资源所有权。结果与交接：`game/tools/results/LOCAL-QUALITY/Q03-A12/RESULT.json`、`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A12/Q03-A12.md`。

门禁边界不变：候选 Avatar/动作资源契约已被验证，但正常 InitialHarbor consumer 仍为 `0`，正常 Player 仍 `FormalAnimatorReady=false / PoseDriverMode=Procedural`；候选 bridge 仍需 `-d20Candidate` 或显式测试安装。因此 `Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL` 保持。下一卡为 Q03-A13：接入单一正式 D2 presentation consumer，并在 exact-build 上做连续动作和原生视觉验收。
## 2026-09-12 Q03-A13：显式 formal presentation consumer 逻辑闭环

本卡使用已连接的 `com.unity.pipeline@0.6.0-exp.1` 完成一条显式 `-d20Formal` D2 presentation consumer。Builder 将已审计 R02 source Prefab/Controller 复制到 `Assets/Resources/D20/Formal/`；桥接层提供 `InstallForFormal` 和 route flag，但默认 InitialHarbor 不加载，D2 physical root 仍是唯一移动/登船裁决者。

验证：Formal consumer PlayMode `1/1 PASS`；资源契约 EditMode `2/2 PASS`；D20-A02 EditMode `9/9 PASS`；candidate integration PlayMode `2/2 PASS`；D17-A06 quarantine PlayMode `1/1 PASS`；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`；清空后 error console=`0`。正式测试覆盖资源 Avatar 预检、generic-path adapter、W/Shift `Idle→Walk→Run`、Animator.Update 不改变物理根、`Board→Disembark` 和 unsupported action=`0`。

门禁边界：D20 clips 仍是 generic transform-path clips，runtime `Animator.avatar=null` 是显式适配契约，不是 authored Humanoid motion 质量证据；没有在 exact-build Windows Player 上取得原生截图。因此不升级 `Q03`、`VC-G01` 或 D31 视觉项：`Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`。正式结果：[Q03-A13](../../game/tools/results/LOCAL-QUALITY/Q03-A13/RESULT.json)；交接：[Q03-A13 handoff](design/common/handoffs/LOCAL-QUALITY/Q03-A13/Q03-A13.md)。

下一卡 `Q03-A14`：构建独立 `-d20Formal` Windows Player，采集真实输入、Player log 和截图，重点检查动作/泳姿、登船握舵、游泳、接地/穿模、树木漂浮、重复 Renderer 及 Esc/UI 模态恢复；原生证据通过前不得把显式 consumer 晋级为默认 InitialHarbor 路径。

## 2026-09-12 Q03-A14：formal Player 构建已完成，原生控制仍未获得

利用 Unity Pipeline 的异步 `build`/`build_status`，独立 `StandaloneWindows64` Player 已成功生成：Build ID=`build_b70c492ca267`，场景为 `MainMenu + InitialHarbor`，总产物=`175167322` bytes，`0 errors / 8 warnings`，EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。运行时显式 flag 为 `-d20Formal`。权威结果：[Q03-A14](../../game/tools/results/LOCAL-QUALITY/Q03-A14/RESULT.json)；交接：[Q03-A14 handoff](design/common/handoffs/LOCAL-QUALITY/Q03-A14/Q03-A14.md)。

这一步只证明构建产物生成，不证明真实 Player。当前 Pipeline 控制的是 headless/no-GPU Editor；本轮没有启动前台 Player、发送物理键盘或采集截图，避免抢占用户桌面。Q03=`BLOCKED_ASSET`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。下一步是 `Q03-A14-RUNTIME`：获得隔离 Player 控制面，或在用户明确授权下执行短时前台 Win32 route probe。

## 2026-09-12 VC-G01 Pipeline hidden Player 控制面复核

本轮已将 Pipeline runtime 控制面接入 VC-G01 的可执行验证链：Development Player 在隐藏 GUI 窗口中运行，loopback runtime server 提供 scene/status/eval/console/keyboard/pointer/quit 命令；不使用 `-batchmode -nographics`，因此可以实际创建 Player Input System 键盘设备，同时不抢占用户前台桌面。

R2 运行时用状态与日志确认了一个先前未被代码级测试覆盖的跨 owner 缺陷：`T` 同时被 D29 酒馆和 D3 出航消费，造成一次 T 既打开酒馆又把 D3 置为 `Sailing`。本轮已将 D3 默认出航键改为 `Y`，保留 D29 酒馆 `T`；同步更新 D3 UI/tutorial/D5 hints，并新增 D16 binding regression。

R3 证据：

- `T`：酒馆打开、`voyage=InPort`，没有启动航程；
- `M`：酒馆关闭后市场 owner 打开，仍为 `InPort`；
- `Y`：市场关闭后 D3 进入 `Sailing`；
- W/E/I/Esc：真实运行时状态分别确认移动、`boarded→disembarked`、仓库模态和关闭/暂停/恢复；
- D16Accessibility `8/8`、D16InputRouter `10/10`、D29 Q15 `1/1`、W/S HUD navigation `1/1`，recompile 无错误，runtime error console=`0`。

R3 仍不是最终视觉门禁：Player 被隐藏运行，没有前台截图、物理键盘或人工画面观察；Player log 仍明确正式 Avatar/动作资源未准入、当前使用 procedural fallback。安全恢复已完成：Editor `developmentBuild=false`，`RuntimePipelineConfig.enableInBuilds=false`。因此 `Q29=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。

权威结果：[Q29 RESULT](../../game/tools/results/LOCAL-QUALITY/Q29/RESULT.json)；交接：[Q29 handoff](design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)。下一步继续以单一视觉 owner 处理动作/泳姿/接地/层级缺陷，并把 Pipeline hidden Player 作为逻辑和输入回归控制面；不要用它替代最终人工视觉验收。

## 2026-09-12：D7 helm anchor 与 D27 NPC URP 修复

继续沿用“单一 owner + hidden Player 真实控制面 + 不扩大 PASS”的门禁。D7 现在读取 D19 R03 authored visible helm anchor，Q26/A05 hand-to-wheel 距离降至 `0.2608m`，并由 Q26/D17 targeted PlayMode 证据锁定。D27 直接盘点并迁移 D20 NPC 材质：16 个源材质改为 `Universal Render Pipeline/Lit`，保留 authored color/metallic/smoothness；D20 shader gate=`4/4 PASS`。

最新 hidden Development Player 运行时确认 `59` NPC renderers / `123` material slots 中 `123` 为 URP Lit、`0` 为 Standard、`0` 为 InternalErrorShader，runtime error console=`0`，close NPC GPU frame 不再 magenta。该结果只关闭 NPC shader 缺陷与舵轮接触测量问题；formal R02 Avatar/action/material、游泳姿态、低模几何、树木/地面接地、港口几何及最终人工视觉仍未通过。

因此 `Q26=PARTIAL`、`VC-G01=IN_PROGRESS`、`Q03=BLOCKED_ASSET`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL` 保持。下一步按依赖进入 formal R02 action/avatar/material 或 geometry/grounding 的单一 owner 任务；不要把 hidden Player 状态证据或局部 GPU frame扩大为整体视觉 PASS。

## 2026-09-12 Q03-A14 R4 generic placeholder arm safety correction

最新 formal R02 close frame 已取得一个可复现的单一表现根因：generic transform-path placeholder clip 保留导入横向 bind-pose，上臂在普通动作状态读作 T-Pose。修复只在 `D20PlayerAnimationPresentationAdapter` 内对 generic-path 的 Idle/Walk/Run/常见交互/Board/Disembark 等状态覆盖上臂 Z 轴；按帧去重并在暂停时跳过，Fish/Helm 明确留给专门泳姿/握舵 IK owner。D2 physical root、Systems 和资源所有权不变。

验证：D20A02=`10/10 PASS`、Q03 formal=`1/1 PASS`、Q26=`12/12 PASS`、D17=`16/16 PASS`；exact Development QA Player `build_9918fec2b2fc`=`0 errors / 0 warnings`；hidden runtime 的 Idle/Walk 手部 Y 低于上臂，E→Ship/Helm 仍显示未解决握舵边界，runtime error console=`0`，clean close；GPU close frame 已保存。正式状态不升级：`Q03=BLOCKED_ASSET`、`Q03-A14=PARTIAL`、`Q26=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`。下一步为 formal authored action/avatar/material readiness 或 geometry/grounding 的单一 owner，禁止把这次 placeholder pose 缓解扩大为完整视觉 PASS。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_FORMAL_R04_ARM_POSE_PIPELINE_20260912_RESULT.json`、`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A14/Q03-A14.md`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

## 2026-09-12 Q03-A14 R6：open_sea 存档重开港口路线修复

本轮把 Pipeline hidden Player 用于复现 D2 船初始化的实际运行缺陷：已有 `open_sea` 存档重新打开 `InitialHarbor` 时，旧海上坐标覆盖了 D19 作者船根，船壳横在玩家出生点到水域的明显路线。最小修复限定在 `D2ShipController`，仅让 `InitialHarbor` 保留作者泊位/船向；非港口场景仍可应用 open-sea snapshot。新增 `Q26A13OpenSeaSaveRestoresAuthoredHarborShipRoute`，注入并恢复静态 PlayerState，验证 `ph_ship_fishing z=-5.2/yaw=180` 和船壳不挡路线。

证据：`build_997e00e6bc28`=`Succeeded / 0 errors / 7` 条既有弃用 warning；Q26=`13/13 PASS`；D17=`16/16 PASS`；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`、error console=`0`。有焦点 hidden Player PID=`46228` 进入 InitialHarbor 后，视觉实例=`z=-3.60/yaw=180`、船壳 bounds.max.z=`0.00`、玩家=`z=2.50`；S 真实输入到 `z=11.38`，后续 W 路线到 `(-1.45,0.40,18.70)` 并进入 `_isSwimming=true/Action=Swimming`，runtime error console=`0`，clean close。Player log 已归档到 `game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_FORMAL_PIPELINE_QA_R6_OPEN_SEA_ROUTE_PLAYER.log`。

控制面边界：一次后续 hidden relaunch 的 `simulate_key` 虽返回 Success，但 D7 snapshot 为 `Modal=FocusLost/CharacterInputAllowed=false`，因此没有计入玩法证据；Pipeline 当前没有可靠 focus/foreground command。R6 没有新增截图，泳姿字段仍显示 `SwimmingVisualPitchDegrees=64`、`SwimmingVisualBodyDrop≈-0.607`。本轮只关闭存档覆盖船根/堵路缺陷，不升级 `Q03-A14=PARTIAL`、`Q03=BLOCKED_ASSET`、`Q26=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 或 D31-P04/S05/G06=`PARTIAL`。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_FORMAL_R06_OPEN_SEA_ROUTE_PIPELINE_20260912_RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A14/Q03-A14.md`；VC-G01 增量：`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

## 2026-09-12 Q26-A14：静态合批污染 R03 地形碰撞的有界修复

VC-G01 继续推进时，连接的 Unity Pipeline PlayMode 运行实际复现了 D31-P04 岸线接地问题：D19 可见地形进入 PlayMode 后使用了 `Combined Mesh (root: scene) 4`，D6 再把这个合批视觉网格复制到 `D19_TerrainRoot`，导致最外侧 `ShoreRock_0` 的碰撞高度偏离可见底部 `0.290m`。该失败先被 Q26A14 测试记录，再实施修复。

最小修改为 `HarborCollisionController` 保留场景作者的 `D19_TerrainRoot` `MeshCollider.sharedMesh`，只同步 Transform，不再运行时从静态合批后的视觉 `MeshFilter` 复制网格；R03 Builder 仍使用同一 terrain owner 计算岸线岩石接地。验证结果：修复后 terrain mesh=`D19_R03_IslandSurfaceMesh`，9 个 `ShoreRock_*` 误差=`0.018–0.022m`；Q26A14=`1/1 PASS`，完整 Q26=`14/14 PASS`；build=`build_0cc514c48042`，`StandaloneWindows64 / 0 errors / 8 warnings`；Editor=`ready/stopped`，error console=`0`。

本卡只关闭运行时碰撞/接地 owner 缺陷，不是最终画面门禁：没有新增 Player 截图、前台键盘或人工视觉结论。`D31-P04/S05/G06=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持。权威结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_A14_SHORELINE_GROUNDING_PIPELINE_20260912_RESULT.json`、`game/tools/results/D31/D31-P04/D31_P04_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

## 2026-09-12：Q31 Pipeline hidden Player 控制面闭环（不替代视觉门）

本轮将 `com.unity.pipeline@0.6.0-exp.1` 的 runtime server 用于当前 Q31 候选 Player，而不是接管用户桌面。隐藏 GUI Development Player 通过 pointer 进入 InitialHarbor，再由 `simulate_key` 驱动当前 Input System；运行时确认了 W/S 位移、A 转向、shoreline entry/exit、E 登船/下船、I 仓库、T 酒馆重复/关闭、Esc modal 与暂停恢复。Runtime error console=`0`，Player clean close，Editor 最终 ready/stopped。

本轮明确登记三条边界：

- `simulate_key` 是 Player 内部控制面证据，不是物理前台 Win32 键盘证据；
- 当前存档初始为 `Sailing`，所以 M 只获得 owner 拒绝，不为市场成功改写存档；
- 没有新截图，不能替代用户视觉否决，也不能证明正式动作/Avatar、自然泳姿、握舵、植被接地、穿模、用户 GPU 性能或最终画面质量。

权威结果：`game/tools/results/LOCAL-QUALITY/Q31/Q31_PIPELINE_RUNTIME_RECHECK_20260912_RESULT.json`。状态保持：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`；后续仍按单一 owner 缺陷推进，Q31 最终视觉门放在最后。

## 2026-09-12：Q31 hidden Player RenderTexture 相机诊断

在不接管用户桌面的前提下，使用 Unity Pipeline runtime 对当前候选生成四个有界相机帧：InitialHarbor、登船后船上视角、游泳路线默认视角和舵位近景。舵位近景确认真实 `HelmWheel`/`HelmStand` 可见；同步 owner probe 记录 `playerParent=ph_ship_fishing`，左右 fallback hand 到 wheel 距离均为 `0.2608m`，targeted Q26A05=`1/1 PASS`。

该证据只说明“可见舵位已经可被观察和测量”，不能证明自然握舵、正式 Avatar/动作、自然泳姿、植被接地、穿模或最终画面质量。四张图仍显示低模/占位角色与粗糙场景表现，默认游泳图不足以判断泳姿。Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 不变。

权威结果：`game/tools/results/LOCAL-QUALITY/Q31/Q31_PIPELINE_CAMERA_DIAGNOSTIC_20260912_RESULT.json`；四张图位于 `game/tools/results/LOCAL-QUALITY/Q31/`，其 SHA256 已登记在结果 JSON。下一步进入正式 Avatar/action 资源门禁或批准后的 candidate-only 视觉集成，禁止再以截图不足为由反复调舵位。

## 2026-09-12：Q20 当前候选 Player 旧树 containment 清点

为核对“树在空中飘”是否仍由旧 palm Renderer 造成，使用同一 hidden Player runtime 进入 InitialHarbor 后查询 `ph_palm_tree`：根对象 active，但 `25/25` 子 Renderer 全部 `enabled=false`。D26 日志同时记录 `vegetation_grounded=26`、`vegetation_hidden_without_terrain=0`。这证明当前候选没有把旧 palm Renderer 作为可见表现输出，但不等于正式树/棕榈资源已准入，也不等于全镜头接地、遮挡和穿模完成。

Q20 继续 `PARTIAL/BLOCKED_ASSET`；`RenderAuthoredPalms=false` 与 D18 LocalDevelopmentReady gate 保持不变。权威结果：`game/tools/results/LOCAL-QUALITY/Q20/Q20_PIPELINE_PLAYER_TREE_CONTAINMENT_20260912_RESULT.json`；下一步需正式 ready 资源或批准的 candidate-only 集成后再做全镜头视觉复核。

## 2026-09-12 Q03-A14 D20 readable skin owner repair

本轮把 VC-G01 的下一步收窄到 D20 presentation 材质所有权，而不是继续盲调视觉坐标。Builder/bridge 为 Candidate/Formal 的 `SuperHero_Male` 绑定项目拥有的 URP Lit `D20_PlayerSkin_Warm`；D5/D7 对 D20 bridge-owned renderer 跳过材质写入，避免黑色剪影被通用 polish 覆盖。Formal PlayMode 同时等待初始 `Land→Idle`，不把一帧过渡状态误判为逻辑失败。

真实证据：D20A02=`10/10 PASS`、Q03D20=`3/3 PASS`、D19R03Q26=`14/14 PASS`、D17=`16/16 PASS`、recompile=`failed=false/errors=[]`。R4 hidden GUI Player 的 Candidate/Formal 路线均通过 pointer 进入 InitialHarbor，材质 probe 为 `D20_PlayerSkin_Warm / Universal Render Pipeline/Lit / RGBA(0.720,0.420,0.250,1.000)`，W=`Walk`，Formal E=`ph_ship_fishing`，runtime error console=`0`，clean close；四张 GPU RenderTexture 帧和 SHA256 归档在 `game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_SKIN_OWNER_PIPELINE_R4_20260912_RESULT.json`。

边界保持：Candidate 近景可读性改善，但 Candidate outfit、Formal outfit/action/avatar、Helm IK/高度、自然泳姿、树木/地面接地、穿模、港口几何、用户 GPU 性能、物理前台键盘和人工最终视觉未通过。`Q03=BLOCKED_ASSET`、`Q03-A14/Q26/Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL` 不变。下一步按单一 owner 选择 formal 完整 outfit/action 资源或 Helm geometry/IK/scale；本卡不升级任何后续视觉门。
## 2026-09-12 Q03-A14 R1：hidden Player Helm pose evidence

本轮使用 Unity Pipeline hidden GUI Player 完成 Helm 运行闭环证据登记；Pipeline pointer 进入 `InitialHarbor`，`E` 进入 `Helm`、`parent=ph_ship_fishing`，GPU RenderTexture 采集宽景/近景/前侧帧，runtime error console=`0`，Player clean close，Editor 最终 `ready/stopped`。详细结果见 `game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_D20_HELM_POSE_PIPELINE_R1_20260912_RESULT.json`。该卡只登记 `PASS_WITHIN_SCOPE`：普通状态横向 T-pose-like 有界缓解，但双手仍在舵轮上方，没有自然握舵接触，VC-G01 不升级。Pipeline hidden runtime control surface 已可用；当前剩余阻塞是可靠 foreground/physical Win32 keyboard 与人工最终 visual gate，不得用 `simulate_key` 或 RenderTexture 替代。
## 2026-09-12 Q03-A14 R2：Fish presentation bounded evidence

本轮将 Fish 从普通 neutral/Helm 修正中拆成独立 presentation owner：generic-path Fish 每帧一次收窄上臂 bind-pose 轮廓。回归 `D20A02=10/10`、`Q03D20=3/3`、`D19R03Q26=14/14`、`D17=16/16`，exact Player `build_35b48f947b48` 已构建，Editor 最终 ready/stopped、error console=`0`。hidden route attempt 因 D7 FocusLost 排除；组件 probe 以 exact Player 直接注入 Fish presentation state 后生成 GPU frame，结果与边界见 `game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_D20_FISH_POSE_PIPELINE_R2_20260912_RESULT.json`。

帧中水平漂浮轮廓比原始横向 bind pose 收窄，但仍是 generic placeholder；自然划水、物理输入、浮力/接地、正式 Avatar/action、性能和人工最终视觉继续开放。任务板保持 `Q03=BLOCKED_ASSET`、`Q03-A14/Q26/Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`。下一步选择正式 swim/action/avatar 资源或独立 swim-cycle asset，不继续扩大 safety pose。

## 2026-09-12 Q20-A03：运行时植被接地与旧棕榈隔离守卫

当前源码新增 `D19R03Q20VegetationPlayModeTests`，并通过连接的 Unity Pipeline 在 InitialHarbor PlayMode 实际运行。`24/24` 可见 R03 植被 Renderer 均通过 scoped `D19_TerrainRoot` 接触采样，grounding failures=`0`；`5` 个 legacy `ph_palm_tree` root 的 active child Renderer=`0`。父 EditMode `D19R03Q20VegetationTests=1/1`，Editor 最终 `ready/stopped`，error console=`0`。

此卡只增加可重复的 runtime containment guard，不能把正式棕榈资源或全镜头视觉质量标为完成。D18 ready catalog 仍 `BLOCKED`，Q20 仍 `PARTIAL/BLOCKED_ASSET`，Q31/VC-G01 的用户视觉门仍开放/未通过；下一步按依赖进入 Q21 空间审计。

## 2026-09-12：Q13-A02 UI 导航策略再断言

为响应 W/S 触发右上角菜单切换的逻辑风险，本轮修复 `HarborInteractionStabilizer` 的 early-return 竞态：游戏态不只要求 `sendNavigationEvents=false`，还要求 `InputSystemUIInputModule.move=null`。新增回归会先由外部重新接回 `UI/Navigate`，再确认下一帧策略将其清除。

真实 Pipeline 证据：定向 D16 导航测试 `2/2 PASS`（4.87s），D17 全量 `16/16 PASS`（100.67s），当前 D16 全 fixture 连续两次 `10/10 PASS`，历史 CameraFollow NaN 未再复现但仍保留为 watch item；recompile 无错误，Editor 最终 ready/stopped，清空后的 error console 为 0。该卡只强化输入 owner 的逻辑边界，不证明 native Player 前台键盘、不替代截图、也不改变 Q31/VC-G01 的人工视觉门状态。结果：`game/tools/results/LOCAL-QUALITY/Q13-A02/RESULT.json`。

## 2026-09-12：Q26-A17 生成舵轮高度对齐

本轮依据 Q31 近景测量选择 D19-R03 生成 presentation owner：修复前 `HelmWheel.y=1.72m`、fallback 双手中线 `y=1.894419045m`，垂直误差 `0.174419045m`。`D19R03HarborPresentationBuilder` 现在生成 `HelmStand.height=0.82m`，`HelmWheel/HelmWheelHub.y=1.89m`；D2/D7 gameplay anchor、船舶物理、Systems 和 AgentCore 不变。新增 Q26-A17 runtime guard，禁止该垂直错位回归。

证据：定向 `1/1 PASS`，实际日志 `generatedHelmWheelY=1.8900`、`handMidpointY=1.8944`、`verticalError=0.0044`；完整 Q26 PlayMode=`16/16 PASS`；Windows Player build=`build_cfa950c379f9`，`0 errors / 8 warnings`；Editor=`ready/stopped`、error console=`0`。一次 Editor-only RenderTexture 取景为均匀灰帧，已经登记为无效，不作为视觉证据；本卡没有前台物理 Win32 键盘或人工最终画面验收。

Q26/Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`。权威结果：`game/tools/results/LOCAL-QUALITY/Q26-A17/RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q26-A17/Q26-A17.md`。下一步优先 formal Avatar/action admission 或已验证的 hidden Player runtime camera surface，不再无新测量盲调舵轮坐标。

## 2026-09-13：Q03-A16 Formal Humanoid pose adapter与R2 hidden Player校准

针对用户反馈的任务动作扭曲、不能游泳和上船握舵错位，本轮选择 D20 Presentation 作为单一 owner。新增 `-d20FormalHumanoid` opt-in 路径，使用 R02 prefab 的 valid Humanoid Avatar 与 `HumanPoseHandler`；舵位校准只改变表现姿态，D2 物理根、Systems、AgentCore、默认 Player 和 generic action admission 保持不变。

Pipeline 证据：Humanoid PlayMode=`1/1`、Q03D20=`4/4`、Q03Formal=`3/3`；R2 Player build=`build_d87c4e4b5265`、`0 errors / 2 warnings`。隐藏 Player 通过 pointer 进入 `InitialHarbor`，W/S 进入真实 `Swimming/Fish/pitch=70`，E 进入 `Ship/Helm`；双手到可见舵轮距离收敛为 `0.219m/0.222m`。R2 PNG 和日志已独立登记。

状态不升级：这是 `PASS_WITHIN_SCOPE`，不是完整视觉门。Q03=`BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。当前画面仍有低模、自然泳姿、正式动作/Avatar、植被接地/穿模和人工最终验收风险。隐藏 Player 已退出，RuntimePipeline 安全配置恢复；Player log 中 1 条 Error 是 Agent 误调用不存在的 `get_console_logs` 命令，已明确记录。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A16/Q03_A16_FORMAL_HUMANOID_PIPELINE_R2_RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A16/Q03-A16.md`。

## 2026-09-13：Q31 D2 水下地形安全边界与 hidden Player 当前复核

为回应真实 Player 中的“海洋不能游泳/角色进入错误几何空间”表现，本轮把问题收窄为 D2 水域 owner 的可验证状态缺陷：用户报告坐标处，水面下方存在可行走 `D19_TerrainRoot` 顶面；旧的向上射线在单面 MeshCollider 上无法命中，导致角色继续保持游泳态。生产修复改为从游泳体积上方向下探测可行走顶面，发现重叠即退出游泳，并从上方重定位到地面；没有给 D19 表现资产添加碰撞体。

验证采用 connected Unity Pipeline：D17 相关定向回归 `9/9 PASS`，新测试 `D17A17WaterOverlapRejectsSwimmingUnderWalkableTerrain` 直接验证退出/grounded/重定位/日志；hidden Player R2 在同一坐标从 `swim=true/grounded=false/y=0.400` 变为 `swim=false/grounded=true/y=1.003`。开放水域 `W` 实际产生 `speed=2.073093`、`action=Swimming`，因此“控制状态没有游泳位移”这一边界已获得当前运行证据，但不是自然泳姿通过。Player error console=`0`、clean close，QA Runtime/Development 设置均恢复关闭。

视觉门禁仍未通过：重叠恢复 RenderTexture 仍近墙/遮挡且看不到玩家；开放水域仍为低模/程序化角色；正式 action/avatar、自然 swim cycle、握舵语义、全镜头树木接地、穿模、物理前台 Win32 输入、用户 GPU 性能和人工最终画面仍开放。故 `Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL` 不变。

权威结果：`game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q31.md`。下一步选择一个新的单一 presentation/asset owner；不再把状态回归或 hidden Player 图像扩大为整体视觉 PASS。

## 2026-09-13：Q29/Q30 当前源码 Pipeline 交互证据与最终门禁边界

Q29 当前 R5 已通过 Unity Pipeline 的隐藏 Windows Player 完成有界真实输入路线：W/Walk、仓库模态 W 阻断、I/M/T 打开/重复/Esc 恢复、E 登船/下船、Y 出航、航行中 M owner 拒绝、Ship W/A 速度与转向、Esc 暂停/恢复。该路线证明输入/状态控制面可用，但不包含截图，因此不能把动作扭曲、自然泳姿、握舵接触、树木悬浮、穿模、材质、镜头或目标设备性能标为通过。

Q30 当前独立审查已登记：定向 Python `65/65 PASS`；Unity D16Accessibility `8/8`、D16InputRouter `10/10`、D17ActionQuality `16/16`、VcG01WorldLogic `14/14`；全量 Python discovery 约 `215s` 无完成 verdict，记为 `INCOMPLETE`。Q03 formal action/Avatar 继续 `BLOCKED_ASSET`，Q20 formal vegetation 继续 `PARTIAL/BLOCKED_ASSET`，Q27 继续 `PARTIAL`。

因此依赖顺序不变：Q29/Q30 的隐藏运行态证据不能替代 Q31 的最终 native Player 视觉门禁；VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一张执行卡为 Q31：固定镜头截图、成功/拒绝/重复路径、UI/输入可达性、资源准入和性能边界；没有直接证据不升级状态。

权威结果：`game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json`。

## 2026-09-13：Q07 R03 服务建筑碰撞 owner 修复

Q31 相机/遮挡诊断发现，四栋可见服务建筑 `MarketVisual`、`WarehouseVisual`、`TavernVisual`、`ShipyardVisual` 各有 `37 Renderer` 但没有 Collider；因此 CameraFollow 的非 Trigger SphereCast 无法阻止玩家或相机进入建筑可见体积。本轮按单一 owner 原则只修改 `HarborCollisionController`，以 `Body` renderer bounds 生成外扩 `0.08m` 的 D6-owned solid BoxCollider，不向表现 Prefab 写入碰撞，不改 D2、Systems 或 AgentCore。

证据：Q07 定向 PlayMode=`1/1 PASS`；D17=`16/16 PASS`；D19R03Q26=`16/16 PASS`；当前源码 hidden Development Player `build_c3588a5c004b`=`0 errors / 7 warnings`，进入 `InitialHarbor` 后四个 collider 均为 enabled/non-trigger 且由 `D6_HarborCollision` 持有，四条外部 raycast 均命中，runtime error console=`0`，Player.log 严格 Error/Exception/Crash/Fatal=`0`，clean close。完整结果：[Q07 当前建筑碰撞结果](../../game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_BUILDING_COLLISION_PIPELINE_20260913_RESULT.json)。

边界：本卡证明的是 Harbor 建筑外壳碰撞 owner，不能扩大为相机构图、门口路线、自然动作/泳姿/握舵、树木准入、前台物理键鼠、目标设备性能或人工视觉通过。Q07=`VERIFIED`（扩展 owner 范围），Q31=`PARTIAL`，VC-G01=`IN_PROGRESS`，VC-G02/VC-G03=`LOCKED`。

## 2026-09-13：Q30 current-source independent review rerun2

Q30 使用当前源码和 connected Unity Pipeline 完成独立审查：定向 Python `65/65 PASS`；D16Accessibility=`8/8`、D16InputRouter=`10/10`、D17ActionQuality=`16/16`、VcG01WorldLogic=`14/14`；Q07 building collision owner=`1/1`；Q15/Q16/Q17 rerun2 各=`1/1`；Q28 R6 构建成功；Q29 R6 hidden Player 路线完成且 runtime error query=`0`、clean close。全量 Python discovery 在有界 `215s` 内没有完成 verdict，保持 `INCOMPLETE`。

本轮只更新审查证据，没有扩大状态：Q30=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`。Q29 R6 无有效截图和前台物理键盘证据；Q03 formal action/avatar 仍 `BLOCKED_ASSET`，Q20 vegetation 仍 `PARTIAL/BLOCKED_ASSET`，Q27 target-device performance 仍 `PARTIAL`。下一步仍为 Q31 最终 native Player visual gate；若环境不能产生 compositor-visible screenshot，记录阻塞并继续非视觉 owner 检查。

## 2026-09-13 Pipeline 当前画面诊断能力确认

本轮不再把 hidden Player 视为“只有状态、没有画面”：Unity Pipeline 在隐藏 GUI Player 中通过 `eval` 将 Camera 渲染到 `1280x720 ARGB32 RenderTexture`，再由 `ReadPixels/EncodeToPNG` 导出当前源码画面。Q29 R6 路线真实进入开放水域后得到三张有效 PNG；同轮传统 `ScreenCapture` 黑帧被明确排除。

结论仍是诊断能力可用、产品视觉未通过。有效帧显示当前角色为低模/程序化泳姿，水平轮廓和移动状态可见，但不等于自然 authored swim cycle；重叠恢复帧仍显示近墙遮挡。Q31 保持 `PARTIAL`，VC-G01 保持 `IN_PROGRESS`，下一步选择 formal action/avatar、自然 swim cycle 或 Helm IK/语义中的一个 owner，先写测试再改动。

权威结果：`game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md`。

## 2026-09-13 VC-G01 bounded helm presentation continuation

Q31 当前源码又完成一个可追溯的 presentation owner 闭环：`D19R03HarborPresentationBuilder` 将生成 `HelmWheel` 的近黑材质替换为低光泽暖木，避免近景把操舵对象读成黑色容器。`Q26A18`、D19R03Q26、D17、VcG01WorldLogic 均有当前源码测试证据；hidden GUI Player 通过 Unity Pipeline 真实 pointer/key 进入港口并 E 上船，运行时材质/父节点/手部位置与有效 RenderTexture 帧已登记。

本轮仍是 `PARTIAL`：offscreen RenderTexture 不是 compositor-visible 前台截图，暖木对比不等于自然 IK/握舵或正式动作/Avatar；自然泳姿、树木接地、穿模、前台物理键盘、性能和人工最终视觉门禁继续开放。VC-G01 仍 `IN_PROGRESS`，VC-G02/VC-G03 不解锁。

权威结果：[Q31 helm presentation](../game/tools/results/LOCAL-QUALITY/Q31/Q31_HELM_PRESENTATION_PIPELINE_20260913_RESULT.json)；[VC-G01 result](../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)；[handoff](design/common/handoffs/VISUAL-CLEAN/VC-G01_HANDOFF.md)。

## 2026-09-13 Q03-A17 候选游泳表现复核

基于 Q31 当前 RenderTexture 诊断帧，本轮选择单一 D20 Formal Humanoid presentation owner，保留已验证的 `70°` 水平倾角并收窄上肢/腿脚肌肉幅度。Q03D20=`4/4 PASS`、D17=`16/16 PASS`，StandaloneWindows64 hidden Player 构建无错误；候选进入 `Swimming/Fish` 的真实运行态，前后帧显示膝部折叠减轻。

仍有明确失败边界：手臂保持抬起/风格化，角色仍为低模候选；游泳取帧使用只读 D2 定位而非完整前台 W/S 路线，也没有 authored Humanoid action admission。因此本卡为 `PARTIAL`，不改变 Q03=`BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`。下一步必须继续使用合法资源或另一个有明确 owner 的证据闭环，不进行无证据坐标微调。

结果：[Q03-A17](../game/tools/results/LOCAL-QUALITY/Q03-A17/Q03_A17_FORMAL_HUMANOID_PIPELINE_R3_RESULT.json)；交接：[Q03-A17](design/common/handoffs/LOCAL-QUALITY/Q03-A17/Q03-A17.md)。

## 2026-09-13 Q00/Q01 current-source takeover audit after Q03-A17

Q00/Q01 使用同一 connected Unity Pipeline Editor 重新核对 dirty worktree、Q03-A17 exact Player 绑定和 InitialHarbor 根/尺度/owner。最终 Editor=`ready/stopped`、error console=`0`，未启动第二个项目实例，也没有把 hidden Player RenderTexture 或 Pipeline eval 当作 compositor-visible 视觉通过。

当前默认 Player 仍为 procedural presentation，Q03-A17 只影响显式 D20 Formal candidate；R03 terrain/ship/helm owners 的当前运行态值已记录。动作扭曲、自然泳姿、前台物理键盘、舵轮接触、树木全镜头接地和穿模仍需各自 owner 的后续证据。Q03/Q20/Q31/VC-G01 状态不因 Q00/Q01 升级。

结果：[Q00](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q03A17_20260913_RESULT.json)、[Q01](../game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q03A17_20260913_RESULT.json)；交接：[Q00](design/common/handoffs/LOCAL-QUALITY/Q00.md)、[Q01](design/common/handoffs/LOCAL-QUALITY/Q01.md)。

## 2026-09-13 Q27 current-source headless performance re-audit

同一 connected Unity Pipeline Editor 重跑 D16 预算/性能证据：`D16PerformanceBudgetTests=6/6 PASS`，`D16A01RuntimePerformanceWindowSamplesContinuously=1/1 PASS`；当前窗口 `60.004829s / 163 samples`，`PerformanceProbeOwnedByController=true`、host=`D16WorldPerformanceController`、`BudgetSatisfied=true`。但运行在 `Null Device`，GPU timing unavailable，平均 `30.7857 FPS`、最长帧 `1103.0729ms`、90 个超过 `50ms` 长帧；这些是 headless 诊断风险，不是目标设备性能 PASS，也没有支持修改 D16 生产逻辑。Q27 继续 `PARTIAL`，依赖顺序进入 Q28 新候选 Player，再由 Q29/Q30/Q31 处理真实路线与视觉门禁。

结果：[Q27 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q27 marker](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；handoff：[Q27](design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)。

## 2026-09-13 Q26-A19 helm support material owner closure

Q31 当前 helm 诊断中，候选语义 `HelmStand` 仍使用石材支架，造成舵轮与支撑语义断裂。本轮只修改 D19-R03 presentation owner：在 `EnsureVisibleHelm` 的候选节点早退前，统一把 `HelmStand/stand/pedestal/base` 指向船木，把 `HelmWheel/rudder/steering` 指向舵轮材质；生成 fallback stand/hub 也指向船木。修复前定向测试真实 `0/1 FAIL`（`R03_Stone`），修复后定向 `1/1 PASS`，完整 Q26 套件 `18/18 PASS`。

hidden Development Player 经过 Pipeline `NewGame/Confirm -> InitialHarbor -> E -> Ship/Helm`，运行态材质为 `D7Polish_HelmStand_0 / brown_planks_03_albedo`。有效 RenderTexture 和 Player log 已登记；Player clean close，Editor=`ready/stopped`，Runtime Pipeline=`enableInBuilds=false / port=0`。本卡仅为 `PASS_WITHIN_SCOPE`，不升级 Q31、VC-G01 或 D31；formal Avatar/action、自然 IK/swim、植被接地/穿模、前台物理输入、目标设备性能和人工视觉门禁仍开放。

权威结果：[Q26-A19 RESULT](../game/tools/results/LOCAL-QUALITY/Q26-A19/RESULT.json)；捕获日志：[Pipeline capture](../game/tools/results/LOCAL-QUALITY/Q26-A19/Q26_A19_HELM_SUPPORT_WOOD_PIPELINE_CAPTURE.log)；交接：[Q26-A19](design/common/handoffs/LOCAL-QUALITY/Q26-A19/Q26-A19.md)。

## 2026-09-13 Q26-A21 procedural swim continuity boundary

本轮没有把 `IsSwimming=true` 或位移直接当成泳姿通过，而是新增 `Q26A21ProceduralSwimPoseHasMeasuredContinuousStroke`，要求默认 Player 的表现源明确为 `Procedural`，并测量 72 帧的上肢/脚部变化。结果 `1/1 PASS`，完整 Q26=`19/19 PASS`；运行 marker=`pitch=64.0 / bodyDrop=-0.601 / maxArmDelta=6.47 / maxFootDelta=1.94`。

同一 hidden Development Player 通过 Pipeline `W` 在开放水域真实移动，三张 RenderTexture 有效且显示低模角色在水面产生相位变化；但画面仍是 procedural fallback，不是 formal authored swim/action，也不是 compositor-visible 人工验收。Q26/Q31 保持 `PARTIAL`，VC-G01 保持 `IN_PROGRESS`，Q03 保持 `BLOCKED_ASSET`。

权威结果：[Q26-A21 RESULT](../game/tools/results/LOCAL-QUALITY/Q26-A21/RESULT.json)；捕获日志：[Pipeline capture](../game/tools/results/LOCAL-QUALITY/Q26-A21/Q26_A21_SWIM_POSE_PIPELINE_CAPTURE.log)；交接：[Q26-A21](design/common/handoffs/LOCAL-QUALITY/Q26-A21/Q26-A21.md)。

## 2026-09-13 Q26-A22 D29 bootstrap 输入 owner 修复

本轮把 W/S 与右上角菜单联动的一个确定性代码窗口收口：D29 非模态 HUD 在 `AssignDefaultActions()` 后立即清空 `InputSystemUIInputModule.move`，并用 D16 即时断言锁住启动帧行为。定向测试、D16 输入类和 Q26 完整 PlayMode 均通过；`build_236bb62b66a6` 为 `0 errors / 0 warnings`。

Unity Pipeline hidden Player 已执行 MainMenu→InitialHarbor、W、I 仓库打开/关闭、模态 W、Esc 暂停/恢复、clean quit，runtime error console=`0`，设置已恢复关闭。该证据只覆盖插件注入 Input System，不等价于前台 Win32 物理键盘或最终视觉验收；Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 保持。

权威结果：[Q26-A22 RESULT](../game/tools/results/LOCAL-QUALITY/Q26-A22/RESULT.json)；捕获日志：[Pipeline capture](../game/tools/results/LOCAL-QUALITY/Q26-A22/Q26_A22_UI_BOOTSTRAP_PIPELINE_CAPTURE.log)；交接：[Q26-A22](design/common/handoffs/LOCAL-QUALITY/Q26-A22/Q26-A22.md)。Q31 E/M/T/Esc 交互矩阵已在后续独立结果完成；下一步选择一个仍有当前失败证据的单一 presentation/resource owner。

## 2026-09-13 Q31 当前 Player 交互矩阵

在 Q26-A22 当前 Development Player 上，Unity Pipeline 已实际覆盖 E 登船/下船、港内 M 成功、M 重复 no-op、T 打开/重复 no-op、Esc 关闭，以及 Y/N 出航后 M 被 D3/D29 owner 拒绝；runtime error console=`0`，Player clean close。

该矩阵完成的是运行态控制面和业务 owner 证据，不是前台 Win32 物理键盘、compositor-visible 截图或人工视觉门。Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 保持。权威结果：[Q31 interaction matrix](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_INTERACTION_MATRIX_Q26A22_20260913_RESULT.json)；日志：[capture](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_INTERACTION_MATRIX_Q26A22_20260913.log)。

## 2026-09-14 Q29 当前源码 Pipeline R7 隔离 Player 交互复核

Q29 使用当前源码而不是旧候选重新构建 `Q29_RUNTIME_PIPELINE_QA_R7.exe`，临时启用 `enableInBuilds=true / port=7902 / developmentBuild=true`，通过隐藏 GUI Player 的 loopback Runtime Pipeline 完成了一条干净路线。MainMenu 的 NewGame/Confirm 使用 `simulate_pointer move/down/wait/up`，键盘使用 `simulate_key down/wait/up`；没有激活用户前台窗口、没有使用 Computer Use、没有发送物理键盘。

- 构建 `build_37e34d20a804`：`StandaloneWindows64`、`MainMenu + Bootstrap + InitialHarbor`、`Development + DetailedBuildReport`、`Succeeded`、`0 errors / 0 warnings`、`220872966` bytes。EXE SHA256=`98751D0DFF00DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`；`Assembly-CSharp.dll` SHA256=`6EBD285D9A70A88996E4E1D6CACD11EA04C4A880BF6A46A999DA6A2761DB822C`。
- 当前运行 marker：W 角色移动；I 仓库与模态内 W 阻断；M 市场打开/重复/Esc；T 酒馆打开/重复/Esc；E 登船/下船/再次登船；Y 出航；航行中 M 被 D3 owner 拒绝；船上 W/A 位移；Esc `TimeScale=0→1` 暂停恢复。入口从 MainMenu 加载 `InitialHarbor` build index `2`。
- Runtime `console --level error` 返回 `0`；Player log 大小 `179991` bytes、大小写敏感 `ERROR|EXCEPTION|CRASH|FATAL=0`；runtime `quit` 后 PID 和 descriptor 均不存在。
- QA 设置已经 `dry_run + confirm=true` 恢复并回读为 `enableInBuilds=false / port=0 / developmentBuild=false / allowDebugging=false / connectWithProfiler=false`；Editor=`ready/stopped`。一次引用 `UnityEngine.InputSystem` 的 eval 编译失败仅属于 probe 参数边界，接受的 UnityEngine-only reflection probe 成功。

本轮状态只升级为 Q29 子范围 `PASS_WITHIN_SCOPE`，Q29 父状态保持 `PARTIAL`。隐藏 Pipeline 输入/状态证据不能替代前台物理键盘、compositor-visible 截图、目标设备性能、正式动作/Avatar、自然泳姿、植被准入或最终视觉验收。VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`；下一步进入 Q30 当前源码独立审查，再进入 Q31 最终 native Player 视觉门禁。

权威结果：[Q29 R7](../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q29 R7 log](../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q29](design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)。

## 2026-09-14 Q30 当前源码独立审查 Luna

Q30 继续 `PARTIAL`。本轮发现一个真实的静态测试契约回归：D19-R03 当前源码已经使用 Q26-A18 收口的暖木舵轮色 `0.34/0.16/0.055`，但 Python 测试仍断言上一版近黑色 `0.055/0.09/0.10`；原始定向命令实际为 `64/65`，同步断言到当前 owner 后恢复 `65/65 PASS`。没有修改 Unity 生产源、Prefab、场景或运行时配置。

- Connected Unity Pipeline 当前源码：`D16AccessibilityTests=8/8`、`D16InputRouterPlayModeTests=11/11`、`D17ActionQualityPlayModeTests=16/16`、`VcG01WorldLogicTests=14/14`；Q07 建筑碰撞=`1/1`；Q15/Q16/Q17 各=`1/1`。PlayMode 使用 async 路径；Pipeline 对同步 HTTP PlayMode 的 domain-reload 拒绝不作为测试 verdict。
- Editor Error Console=`0`；Warning 返回 `141` 条且 `dropped=false`，为已知 D2/D27 fallback/readiness/animation 警告和 Q17 预期异常路径反馈；取证后清 Console，Editor=`ready/stopped`。
- Q29 R7 当前源码隐藏 Player 路线作为依赖证据已覆盖 W、I 模态 W 阻断、M/T 重复与 Esc、E 登船/下船/再次登船、Y 出航、航行中 M 拒绝、船上 W/A、Esc 暂停恢复，runtime error=`0`、严格 Player 错误词=`0`、clean close。

本轮只收口过期测试契约和当前源码 owner 回归，不关闭 compositor-visible 前台 Player 画面、Win32 物理键鼠、正式 authored action/avatar、自然泳姿/握舵、正式植被准入、全镜头接地/穿模或目标设备性能。Q30=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`；下一张可执行卡仍为 Q31 最终 native Player 视觉门禁。

权威结果：[Q30 当前审查 Luna](../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q30 capture](../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q30](design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)。

## 2026-09-14 Q29 当前源码 Pipeline R10 完整交互控制面复核

通过 Unity Pipeline 临时启用 Player Runtime Pipeline，使用当前源码完成新建 Player 构建 build_b5958f0eeca0，并在同一隐藏 Player 中走过 MainMenu NewGame/Confirm 指针入口。自然运行路线覆盖 W、I 模态 W 阻断、M/T 重复与 Esc、E 登船/下船/再次登船、Y 出航、航行中 M 拒绝、船上 W/A/S/D、Esc 暂停恢复；运行时 error console 为 0，Player 正常退出且严格错误词计数为 0。

I/M 的早期直接 InitialHarbor 探针以及 owner 未就绪 T 被单独登记为边界诊断，不与完整路线混算。Runtime Pipeline 已恢复 enableInBuilds=false / port=0，Q29 子范围为 PASS_WITHIN_SCOPE；父 Q29 仍 PARTIAL，VC-G01 仍 IN_PROGRESS，VC-G02/VC-G03 仍 LOCKED。当前证据仍不证明前台 compositor 画面、Win32 物理键鼠、正式 Avatar/action/natural swim/helm、全镜头接地穿模、目标设备性能或最终视觉验收。

结果：[Q29 R10](../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R10_RESULT.json)；日志：[Q29 R10 log](../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R10.log)；交接：[Q29](design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)。下一步为 Q30 独立复核当前 Q28/Q29 证据和针对性缺陷清单。

## 2026-09-14 Q30 当前源码独立复核 R4（接入 Q29 R10）

通过 Unity Pipeline 重新执行当前源码 owner 回归：Python 定向 65/65、D16 输入路由 11/11、D17 动作质量 16/16、VcG01WorldLogic 14/14。Q29 R10 隐藏 Player 作为依赖证据覆盖 MainMenu NewGame/Confirm 指针入口、W、I 模态 W 阻断、M/T 重复与 Esc、E 登船/下船/再次登船、Y 出航、航行中 M 拒绝、船上 W/A/S/D、Esc 暂停恢复；runtime error=0、严格 Player 错误词=0、clean close。

本轮核对 D2 SailingSystem 语义：S 为减速/停止，不允许倒车，因此 R10 中船上 S 没有继续位移不构成新冲突。Q30 仍 PARTIAL；逻辑/owner 证据不等于前台 compositor 画面、物理 Win32 输入、正式动作/Avatar、自然泳姿/握舵、正式植被、全镜头穿模、目标设备性能或最终视觉 PASS。Editor ready/stopped，Error=0，Warning=157 且 dropped=false，取证后已清空。结果：[Q30 R4](../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260914_R4_RESULT.json)；日志：[Q30 R4 log](../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260914_R4.log)；交接：[Q30](design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)。下一步仍是 Q31 最终 native Player visual gate。

## 2026-09-14 Q31 R4 RenderTexture 直接复核

对当前源码 R4 hidden Player 的四张有效 `1280x720` RenderTexture 做了直接图像复核：初始港口、静态游泳、移动游泳、登船舵轮均存在且 hash 与 R4 result 一致。图像能证明当前 Player 真的渲染出港口/水面/程序化玩家/暖木舵轮，但不构成最终视觉 PASS。

- 初始港口帧：港口建筑、船、水面和玩家可见；低模/程序化场景仍有建筑外壳、道路、岸线和地表层级粗糙边界。
- 游泳帧：程序化玩家处于水平位姿并有伸展四肢/水面阴影；没有 authored swim cycle、自然水接触或正式 Avatar 证据。
- 登船帧：暖棕舵轮比旧近黑色版本更可读，玩家已位于船上；自然手部 IK、上船动作和最终镜头质量仍未成立。
- 固定镜头未复现 visible legacy tree/palm renderer；这只是当前资源隔离和镜头证据，不等于正式植被准入或全镜头接地通过。

因此 Q31 仍 `PARTIAL`、VC-G01 仍 `IN_PROGRESS`、VC-G02/VC-G03 仍 `LOCKED`。这些是 hidden Player offscreen diagnostic frames，不是 compositor-visible 前台窗口或人工最终视觉验收；下一步仍是 formal action/avatar、natural swim/helm contact、vegetation all-camera grounding 或可用的前台 visual gate。

结果：[Q31 R4](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_R4_RESULT.json)；[Q31 canonical result](../game/tools/results/LOCAL-QUALITY/Q31/RESULT.json)；交接：[Q31](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-14 Q20-A04 candidate palm visual diagnostic

本轮在 D19-R03 presentation owner 内接入 `Candidate/ImportVerified` 的 `tree_palm.fbx`，并用 Unity Pipeline hidden GUI Player 做真实 Renderer/接地/RenderTexture 复核。Player 中候选 `KenneyNaturePalmCandidate` 为 `1 Renderer / 0 Collider`；忽略触发器后命中 `D19_TerrainRoot`，bounds 接地误差为 `0.0240m`。三张 `1280x720` Player PNG 已登记，其中隔离候选帧可读，场景帧局部被 dock 遮挡。

该证据只登记为 Q20-A04 `PASS_WITHIN_SCOPE`：候选资源仍不是 LocalDevelopmentReady，正式棕榈仍未开启，也没有扩大为全镜头接地/遮挡/穿模、前台物理输入、目标设备性能或人工最终视觉 PASS。Q20 继续 `PARTIAL/BLOCKED_ASSET`，Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`。权威结果：[Q20-A04](../game/tools/results/LOCAL-QUALITY/Q20-A04/RESULT.json)；交接：[Q20](design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)。

## 2026-09-14 Q31 R5 native Player UI fix and temporary development pause

本轮在 Unity Pipeline `com.unity.pipeline@0.6.0-exp.1` 控制下启动了一个可见 Windows Player，补上此前 hidden Player/RenderTexture 无法证明的合成器截图边界。当前源码 `MainMenuController` 将 New Game 标题从 `y=150` 调整为 `y=210`；接受构建 `build_fc9d95893976` 为 `StandaloneWindows64 / Succeeded / 0 errors / 0 warnings`。`Q31_R5_UI_FIX_NEWGAME.png` 显示标题与船长名占位文本已经分离，因此该单一 UI 缺陷在有限范围内为 `PASS_WITHIN_SCOPE`。

同一可见 Player 通过 MainMenu NewGame/Confirm 进入 `InitialHarbor`，完成 W、E 登船、E 下船以及 W/S 路径截图；runtime error console=`0`、Player 严格错误词=`0`、clean close，Runtime Pipeline 与 Development Build 配置已恢复关闭。InitialHarbor/Board 帧确实是真实可见画面，但仍为低模/程序化表现；本轮 W/S 路径没有视觉证明进入开放水域，不能据此宣称游泳通过。

安全暂停点：`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`。正式动作/Avatar、自然 swim/helm、植被全镜头接地与穿模、前台物理键盘、目标设备性能和人工最终视觉验收继续开放。恢复时从 Q31 R5 选择单一 visual owner，先写回归再修改，不跨依赖解锁后续门禁。权威结果：[Q31 R5](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_UI_FIX_AND_VISUAL_GATE_20260914_R5_RESULT.json)；日志：[R5](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_UI_FIX_AND_VISUAL_GATE_20260914_R5.log)；暂停交接：[PAUSE_20260914_Q31_R5](design/common/handoffs/LOCAL-QUALITY/PAUSE_20260914_Q31_R5.md)。

## 2026-09-15 Q29 R11 入口 smoke 与临时暂停

用户要求在当前开发阶段找安全收口点并临时暂停。本轮使用当前 HEAD 构建 Pipeline-enabled hidden Player，真实通过 MainMenu NewGame/Confirm pointer 路径进入 `InitialHarbor`，随后主动停止；没有把未执行的完整输入路线或画面结论记为 PASS。

- R11 结果：[paused result](../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R11_PAUSED_RESULT.json)；日志：[paused log](../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R11_PAUSED.log)；交接：[PAUSE_20260915_Q29](design/common/handoffs/LOCAL-QUALITY/PAUSE_20260915_Q29.md)。
- 安全不变量：Player PID/descriptor 已消失；Editor=`ready/stopped`；Runtime Pipeline=`enableInBuilds=false / port=0`；Development Build、Debugging、Profiler 连接均关闭。
- 状态保持：`Q29=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`。formal action/avatar、natural swim/helm、植被接地/穿模、前台物理输入、目标设备性能和最终人工视觉仍未关闭。
- 恢复入口：Q30 当前源码独立审查 → Q31 最终 native Player visual gate；已有 Q29 R10 hidden Player 全交互结果保持为独立证据。

## 2026-09-15 Q30 R5 current-source owner revalidation

Q30 R5 已绑定当前 HEAD `94950f36c0d5a3daf4ba719216808a0ae6d71be9`。Python `65/65`、D16 输入 `11/11`、D17 动作质量 `16/16`、VcG01 世界逻辑 `14/14` 通过范围内回归；没有修改生产视觉代码。Editor 和 Runtime Pipeline 已回到安全状态。

本轮必须保留的视觉/环境边界：FMOD 在 Editor 测试会话中真实报告 `Error initializing output device (60)`；Windows 音频设备枚举为 OK、AudioSettings 可读仍不等于播放成立。程序化低模、未验证 `ph_crew_male`、absolute-Euler fallback、D27 presentation wait 和正式植被准入仍解释当前用户可见缺陷，Q30 继续 `PARTIAL`，Q31 继续最终 native Player visual gate。结果与交接见 `game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260915_LUNA_R5_RESULT.json`、`docs/game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md`。

## 2026-09-15 Q31 R6 visible native Player diagnostic boundary

R6 使用当前 HEAD `9fd08f711e7b699a32a420d82a0a5f1d4ac38a0c` 构建 `build_9f35f8fcb2f4` 并通过 Unity Pipeline 启动可见原生 Player。MainMenu NewGame/Confirm → `InitialHarbor`、E 登船/下船和 `1600x900` compositor-visible 截图已真实执行。截图可用于诊断当前低模/程序化环境、动作姿态和岸线路线问题，但不构成 VC-G01 PASS。

W/A/D 下船路线在约 `z=-4.94 / -18.48 / -23.36` 停滞；本轮没有权威 `IsSwimming=true` marker，既有 S/open-water 证据与本轮分开登记。会话在最终 runtime console、严格 Player 日志和 clean-close 断言前中断，接管后 PID/descriptor 已消失、Runtime Pipeline 已恢复 `enableInBuilds=false / port=0`。状态继续为 Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`。

后续执行顺序固定为：connected Editor Pipeline 记录相机朝向 → Player/CharacterController → 地形/建筑 Collider 命中 → OceanAdapter 水面查询 → `IsSwimming` 转换；完成单一 owner 归属和最小回归后，才允许改生产代码。禁止用截图、固定 token、mock 或构造位置替代真实路线证据。

结果：[Q31 R6](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_VISUAL_20260915_LUNA_R6_RESULT.json)；交接：[Q31](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-16 Q31 R7 S-route confirms swim state and isolates visual owner

当前源码可见原生 Player 通过 Pipeline pointer 进入 `InitialHarbor`，相机平面前方为 `-Z`。W 走向船/港口碰撞体，S 走向 `+Z` 水边；S 分段到约 `(0.33,0.40,18.80)` 后运行时明确 `IsSwimming=true / IsGrounded=false`，释放和继续移动后游泳态保持。因此不能把“W 路线撞住”解释成海洋逻辑完全失效。

R7 的四张真实 `1600x900` compositor-visible 帧暴露了下一项明确缺陷：角色虽进入权威游泳态，却以扭曲水平姿态、弯折四肢和不可信水面接触关系呈现；InitialHarbor 还观察到约 `3.8–4.5 FPS`、平均帧 `224–261ms` 的严重卡顿。前者归入泳姿 presentation/Avatar owner，后者另开性能诊断，不互相替代。

R7 完成 runtime console error=`0`、Player.log 严格错误行=`0`、clean quit、PID/descriptor 消失和 Runtime Pipeline 恢复关闭；Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 保持。执行顺序更新为：先检查 `D2CharacterAnimationController`/D7 presentation 与 formal avatar asset contract → 写 focused regression → 最小视觉修复 → visible Player 复验；禁止继续盲调岸线坐标。

结果：[Q31 R7](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_SWIM_ROUTE_20260916_R7_RESULT.json)；日志：[R7](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_SWIM_ROUTE_20260916_R7.log)；交接：[Q31](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-16 Q31 R12 execution record

R12 完成了 swim presentation owner 的最小修复和真实 Player 复验：D20 Formal Humanoid adapter 将泳姿 body presentation 下移 `0.85m`，保持前臂/小腿伸展，新增 PlayMode safety contract。Development Player `build_8eca102a61b4` 以 `0 errors / 0 warnings` 构建，通过 Unity Pipeline 完成 S→真实游泳状态路线；`D17A03=1/1 PASS`，runtime console/Player.log 严格错误边界均为 `0`，clean quit 和配置恢复成功。

可见结果仍为 `IMPROVED_WITHIN_SCOPE` 而非最终 PASS：身体和四肢位置改善，但单臂划水、风格化姿态与水面接触仍不自然。执行顺序保持为：action/boarding presentation 单一 owner 检查 → focused regression → 最小修复/或缺陷登记 → visible Player 复验；Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`，禁止盲调岸线坐标。

结果：[Q31 R12](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_SWIM_FORMAL_SAFE_20260916_R12_RESULT.json)；日志：[R12](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_SWIM_FORMAL_SAFE_20260916_R12.log)；交接：[Q31](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-16 Q31 R13 execution record

## 2026-09-16 Q31 R13 safe pause

R13 作为本阶段临时暂停点正式登记。Pipeline 控制的真实 Player 已完成有界 E/W/A 路线并 clean quit；D17A02/D17A03 focused regressions 通过，Editor 回到 `ready/stopped`，Runtime Pipeline 回到 `enableInBuilds=false / port=0`。R13 只证明交互/状态契约的有限范围，不能升级 `VC-G01`，也不能提前打开 `VC-G02/VC-G03`。

未关闭项仍包括正式 action/avatar、自然 swim/helm IK、植被空间接地/穿模、前台物理键盘、目标设备性能、音频和人工最终视觉。恢复从 `Q31 R14` 开始，继续遵守单一 owner → focused regression → 当前 native Player 复验的顺序。交接：[PAUSE_20260916_Q31_R13](design/common/handoffs/LOCAL-QUALITY/PAUSE_20260916_Q31_R13.md)；结果：[pause result](../game/tools/results/LOCAL-QUALITY/Q31/Q31_PAUSE_CHECKPOINT_20260916_R13_RESULT.json)。

R13 在 R12 Development Player 上完成 E boarding/disembark/repeat 和 W+A ship-control 路线，确认 D7 的 parent/PlayerBoarded、D2 的 Ship/Character mode、Formal D20 的 Helm state 以及 heading 变化。新增 D17A02 Board→Helm focused contract 后 `1/1 PASS`，D17A03=`1/1 PASS`，Editor recompile 无错误。

本轮真实 RenderTexture 帧是有效的动作/归属诊断，但不是最终 compositor-backbuffer 或人工视觉验收：角色已经处于可见舵轮前，低模/程序化几何、正式动作 clip 和自然 hand IK 仍然开放。执行顺序保持单一 owner 边界，Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`。

结果：[Q31 R13](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_BOARDING_HELM_ROUTE_20260916_R13_RESULT.json)；日志：[R13](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_BOARDING_HELM_ROUTE_20260916_R13.log)；截图：[boarding frame](../game/tools/results/LOCAL-QUALITY/Q31/Q31_R12_VISIBLE_BOARDING_ACTION_R1_RT.png)。

## 2026-09-16 temporary safe pause after Q09 current-source re-audit

Q05-Q09 已完成当前源码层的有界推进：Q05 捕获并修复 Swimming transition 的真实 stale grounded-visual-offset 回归，Q06/Q07/Q08/Q09 的 focused/整合/锚点回归均已登记。该收口点只证明 connected Editor 的逻辑与代码边界，不能替代 native Player 前台输入、compositor-visible 最终视觉、自然动作/泳姿/握舵、植被和目标设备性能验收。

暂停状态保持：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。Editor 已回到 `ready/stopped`，Runtime Pipeline 已恢复 `enableInBuilds=false / port=0`，工作区已有 tracked/untracked 改动未清理。恢复入口为 `Q10_CURRENT_PIPELINE_REAUDIT_Q31R13_20260916_LUNA`；权威暂停结果见 [Q31 Q09 R1 pause checkpoint](../game/tools/results/LOCAL-QUALITY/Q31/Q31_PAUSE_CHECKPOINT_20260916_Q09_R1_RESULT.json)，暂停交接见 [PAUSE_20260916_Q09_R1](design/common/handoffs/LOCAL-QUALITY/PAUSE_20260916_Q09_R1.md)。

## 2026-09-16 current-source rebind after Q31 R13

Q00/Q01/Q04 已从 R13 暂停点重新建立当前证据链：工作区保持脏状态并未清理；R12 Player 被 R13 复用，D20 泳姿生产 patch 已由当前 HEAD `0a711417` 记录，R13 测试断言不被误写为 Player 二进制内容。Connected Editor 采样确认 D20 Formal Humanoid 是可见 presentation writer，D2 仍负责物理根与 gameplay action；D17A02/D17A03/D17A07 均 `1/1 PASS`。

这次只收紧 source/owner/逻辑边界，不改变视觉门禁：`VC-G01=IN_PROGRESS`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`。下一步 Q05 当前游泳 presentation 复核，然后才选择 Q31 R14 的单一 visual owner；不得把 Pipeline 回归或局部 RenderTexture 帧扩大为最终视觉通过。

结果：[Q00 R1](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_Q31R13_20260916_LUNA_R1_RESULT.json)、[Q01 R1](../game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_Q31R13_20260916_LUNA_R1_RESULT.json)、[Q04 R1](../game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_WATER_OWNER_Q31R13_20260916_LUNA_R1_RESULT.json)。

## 2026-09-16 Q00/Q10 resume evidence after Q09 safe pause

恢复时先重新绑定当前源码：HEAD 与远程均为 `8413feeffa5cf4e1367c3aaff73890f30bb39d2f`，既有 `1456` 条未跟踪历史/生成证据全部保留，tracked diff=`0`；Unity Editor Pipeline=`6000.0.80f1 / PID 34276 / 127.0.0.1:7800 / ready`，Runtime Pipeline=`enableInBuilds=false / port=0`。R12/R13 Player `build_8eca102a61b4` 来自 `9fd08f...`，早于当前 Q05 D2 transition guard，`exact_current_source_match=false`，不得继承为当前 Player 视觉/输入证据。结果：[Q00 R2](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_Q31Q09_20260916_LUNA_R2_RESULT.json)。

随后按依赖执行 Q10，没有修改生产代码：`D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable=1/1 PASS`，两次运行均通过，第二次 Pipeline=`1.96s`、test=`1.8683108s`；marker=`motion_samples=10 / maxRelativePositionError=0 / maxRelativeRotationError=0 / maxShipTranslation=1.510m / maxShipHeadingDelta=32°`。Error Console=`0`、dropped=`false`，已知 warning 保留并在取证后清空，Editor=`ready/stopped`。Q10=`PASS_WITHIN_SCOPE` 仅代表 snapshot 驱动相对甲板与状态恢复，不关闭前台 Player、开放海、镜头/握舵、正式动作/Avatar、植被、性能、音频或 VC-G01。下一步为 Q11。结果/日志/交接：[Q10 R1](../game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q10 log](../game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1.log)、[Q10 handoff](design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md)。

Q11 随后在同一 connected Editor Pipeline 完成当前源码安全下船复核，没有生产代码或持久配置变更：`D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround=1/1 PASS`，Pipeline=`3.23s`、test=`3.1345655s`；真实 marker=`legal_disembark_cycles=10 / no_ground_rejection=true / final_grounded=True / final_mode=Character`。预期无落点拒绝 warning 保留，Error=`0`、dropped=`false`，取证后清 Console，Editor=`ready/stopped`。Q11=`PASS_WITHIN_SCOPE` 只关闭合法/无落点状态恢复边界，不关闭移动船体视觉碰撞、完整岸线/跳板、native Player、正式动作/Avatar、相机、植被、性能、音频或 VC-G01。下一步 Q12。结果/日志/交接：[Q11 R1](../game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q11 log](../game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1.log)、[Q11 handoff](design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md)。

Q12 随后完成当前源码 CameraFollow 定向复核，没有生产代码或持久配置变更：`D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose=1/1 PASS`，Pipeline=`3.41s`、test=`3.3152951s`；marker=`mode_cycles=10 / obstructed=true / distanceBefore=5.500 / distanceBlocked=3.790 / distanceRecovered=5.494 / collisionChecks=109`。Error=`0`、dropped=`false`，取证后清 Console，Editor=`ready/stopped`。Q12=`PASS_WITHIN_SCOPE` 只代表有界模式/目标切换与受控遮挡恢复，不关闭最终世界穿模/构图、native Player、性能、音频或 VC-G01。下一步 Q13。结果/日志/交接：[Q12 R1](../game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q12 log](../game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1.log)、[Q12 handoff](design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md)。

Q13 随后完成当前源码输入/UI 定向复核，没有生产代码或持久配置变更：D17A15=`1/1 PASS`、D16InputRouter=`11/11 PASS`、D16PrimaryHudNavigation=`2/2 PASS`。D17A15 marker=`gameplay_w_s_cycles=10 / normal_navigation=false / pause_navigation=true / blocked_navigation=true / restored_navigation=false`；日志确认 `EventSystem navigation=modal -> gameplay_suppressed`、W/S 主选择保持 null 和 ship W gameplay route。Error=`0`、dropped=`false`，取证后清 Console，Editor=`ready/stopped`。Q13=`PASS_WITHIN_SCOPE` 只代表 Unity Input System/运行时路由的 W/S-HUD 隔离、Esc 模态恢复和 UI Move 防竞态，不代表前台 Win32 物理键盘、渲染 HUD、E/I/M/T、native Player、正式动作/Avatar、植被、性能、音频或 VC-G01。下一步 Q14。结果/日志/交接：[Q13 R1](../game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q13 log](../game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1.log)、[Q13 handoff](design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)。

Q14 随后完成当前源码船岸整合复核，没有生产代码或持久配置变更：`D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence=1/1 PASS`，Pipeline=`2.71s`、test=`2.6117578s`；marker=`shore_water_shore=true / board_owner=Ship / ship_input_turn=true / no_ground_rejection=true / legal_disembark=true / imported_neutral=False`。Error=`0`、dropped=`false`，取证后清 Console，Editor=`ready/stopped`。Q14=`PASS_WITHIN_SCOPE` 只代表岸→水→岸、登船/转向、无落点拒绝与合法下船的 owner/state 链；`imported_neutral=false` 是程序化 Player 分支，不是正式动作资源通过，不关闭前台/native Player、E/I/M/T、最终视觉、植被、性能、音频或 VC-G01。下一步 Q15。结果/日志/交接：[Q14 R1](../game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q14 log](../game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1.log)、[Q14 handoff](design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)。

Q15 随后完成当前源码 D29/D16 面板焦点复核，没有生产代码或持久配置变更：`D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes=1/1 PASS`，Pipeline=`13.67s`、test=`13.5745984s`；marker=`shortcut_input=true / keys=I,M,T / legacy_keydown_reached_owner=true / warehouse_cycles=10 / market_cycles=10 / tavern_cycles=10 / exclusive=true / repeated_request_rejected=true / gameplay_recovery=true`。Error=`0`、dropped=`false`，取证后清 Console，Editor=`ready/stopped`。Q15=`PASS_WITHIN_SCOPE` 只代表 D29/D16 PlayMode 快捷键 owner、互斥、重复拒绝与 Gameplay 恢复，不代表前台 Win32 键鼠、原生 Player 面板/鼠标、业务 owner 真实结果、正式动作、性能、音频或 VC-G01。下一步 Q16。结果/日志/交接：[Q15 R1](../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q15 log](../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1.log)、[Q15 handoff](design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)。

Q16 随后完成当前源码 D29/D3 交易桥复核，没有生产代码或持久配置变更：`D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest=1/1 PASS`，Pipeline=`2.30s`、test=`2.1992274s`；marker=`not_ready=true / shortcut_double_click_one_owner=true / buy_success_one_change=true / sell_success_one_change=true / insufficient_funds_unchanged=true / cargo_full_unchanged=true`，并捕获 D3 owner 的购买/出售完成反馈。Error=`0`、dropped=`false`，取证后清 Console，Editor=`ready/stopped`。Q16=`PASS_WITHIN_SCOPE` 只代表交易 owner 桥接和拒绝状态边界，不代表 native Player 鼠标/市场渲染、在线/存档数据、完整航程、正式动作、性能、音频或 VC-G01。下一步 Q17。结果/日志/交接：[Q16 R1](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q16 log](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1.log)、[Q16 handoff](design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)。

Q17 完成当前源码 D29/D9/D4 整备与船坞业务桥定向回归：`D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException=1/1 PASS`，Pipeline=`3.08s`、test=`2.9858148s`。九项 runtime marker 全部为 `true`；受控空 owner Warning 路径保持状态不变并恢复主输入，Error=`0`、dropped=`false`，证据后清 Console，Editor=`ready/stopped`。本卡仍只关闭 owner/状态/反馈逻辑，不升级 `VC-G01`，不证明原生 Player 前台键鼠、窗口截图、最终视觉、正式动作/Avatar、植被、性能、音频或完整首航。Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。结果：[Q17 R1](../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)；安全暂停：[Q31 Q17 R1](../game/tools/results/LOCAL-QUALITY/Q31/Q31_PAUSE_CHECKPOINT_20260916_Q17_R1_RESULT.json)；恢复入口：`Q18_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA`。
