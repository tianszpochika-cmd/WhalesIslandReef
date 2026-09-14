# D30-S05 视觉可玩完整路线交接

## 状态

`PARTIAL`

## 已完成

- D30 已建立从逻辑只读状态到港口、交易、酒馆、船坞、船上、航行和返港的视觉阶段桥。
- R6 Player 已完成主要纵向路线：新游戏、港口、整备/交易入口、登船、航行、返港、保存和 Continue 重载。
- R8/R11 补充临时页面互斥、标题安全区、P/Esc 入口和输入不串层回归。

## 未完成

- 尚未逐阶段记录全部视觉回执、失败/禁用分支、重复点击和路线中断恢复。
- D30-G06 继续为 `PARTIAL`，不把纵向冒烟路线当作完整 Visual Candidate。

## 证据

- [R6 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R6.md)
- [R8 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R8.md)
- [R11 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R11.md)
- [D30 结果](../../../../game/tools/results/D30/D30-S05/D30_S05_RESULT.json)

## 2026-09-03 R13 输入隔离补充

R13 复核了 D9 整备页与 D3/D29 主快捷键的焦点隔离：D9 内 `B` 泊位、`M` 买盐、`T` 酒馆和 `U` 船坞门禁均保持在当前页面并产生反馈；关闭临时页后主 HUD `B` 仍可打开 D3 贸易。R13 仅证明代表性输入路径，不替代 D30 的全阶段视觉回执、失败/禁用分支、重复点击和路线中断恢复验收。

- [R13 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R13.md)
- [R13 runtime 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R13.log)

## 2026-09-03 R16 完整纵向路线补充

R16 重新执行并记录：

`MainMenu → NewGame("D30 Route R16") → InitialHarbor → P(D9) → B(泊位) → Esc → E(登船) → T(规划航线) → N(盘查) → N(平静海面/抵达) → L(靠港/结算) → C(保存)`

随后重启同一 Player：`MainMenu → Continue → 读取保存后的绿湾村状态`。runtime 日志出现了真实的登船、航线规划、盘查扣款、平静海面、抵达、靠港结算、保存和加载标记，未发现异常级 `Exception`、`MissingComponent` 或 `Crash`。

- [R16 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R16_D30.md)
- [R16 路线日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R16_D30_ROUTE.log)
- [R16 Continue 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_CONTINUE_R16.log)

本补充证明 D30 的主要逻辑状态链能够在 Player 中重放，但尚未逐阶段提供视觉截图和状态 fingerprint，也未完成失败、禁用、重复点击、路线中断和加载后完整视觉恢复；D30-S05 与 D30-G06 继续保持 `PARTIAL`。

## 2026-09-03 R19/R20 代码逻辑收口

R19/R20 对 D30 所依赖的 D10/D29 页面完成代码级交互修补：模态页面现在拥有明确的 D16 焦点边界，隐藏或不可交互页面会清理自身选择；D10 未就绪时不再静默返回，D29 也不再打开未就绪酒馆页面。定向回归 `19/19 PASS`，选择性 D25—D31 回归 `43/43 PASS`，Unity Tundra 脚本编译成功且无 `error CS`。

这轮没有把源码审计冒充 Player 验收。D30 仍需真实记录逐阶段视觉回执、状态 fingerprint、失败/禁用分支、重复点击、航行中断恢复和 Continue 后视觉恢复，因此 D30-S05 与 D30-G06 保持 `PARTIAL`。

- [R20 交互逻辑审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R20.json)
- [R20 Unity 编译日志](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R20.log)

## 2026-09-03 R23 表现安全区补充

R23 将 D30 路线条从顶部标题副区域下移到 `anchoredPosition = (0, -174)`，并在独立 Windows Player 中复核港口主标题、副标题和路线信息不再互相压叠。该修复只改变表现层布局，不改变 D30 状态桥、航行 owner 或保存逻辑。

- [R23 视觉证据 JSON](../../../../../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R23.json)
- [R23 Player 构建日志](../../../../../../game/tools/results/D31/D31-P04/D31_P04_STANDALONE_BUILD_R23.log)

R23 只关闭路线条遮挡这一具体缺陷；交易失败、登船失败、航行中断恢复、重复点击、逐阶段状态 fingerprint 和 Continue 后完整视觉恢复仍未完成。D30-S05 与 D30-G06 保持 `PARTIAL`。

## 2026-09-03 R24 失败阶段保护补充

R24 修复 D30 表现层对失败事件的错误描述：`market_failed`、`board_failed` 和未知事件现在保持当前视觉阶段，显示“请求未通过，状态未改变”；同时 D29 市场拒绝时不再提前打开空的 D3 交易 Canvas。该修改不推进航行、不写业务状态、不改变保存 owner。

- [R24 交互逻辑审计](../../../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R24.json)
- [R24 Unity 编译日志](../../../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R24.log)

R24 选定回归 `46/46 PASS`，Unity 编译无 `error CS`；D30 真实 Player 的交易失败、登船失败、航行中断、重复点击和 Continue 后视觉恢复仍需后续运行取证，因此 D30-S05 与 D30-G06 保持 `PARTIAL`。

## 2026-09-03 R25 港内表现阶段保持补充

R25 修复 D30 `RefreshStage` 的投影逻辑：市场、酒馆和船坞阶段由交互事件产生，且在 D3 仍为 `InPort` 时继续保持；只有船上、航行、返港等 owner 状态才覆盖当前阶段。这样路线条不会在 `Notify` 后下一次轮询时错误闪回“港口”。

- [R25 交互逻辑审计](../../../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R25.json)
- [R25 Unity 编译日志](../../../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R25.log)

R25 选定回归 `47/47 PASS`，Unity 编译无 `error CS`；本修复不替代真实 Player 的逐阶段视觉回执、失败/禁用/重复点击和 D30 异常恢复证据，D30-S05 与 D30-G06 继续保持 `PARTIAL`。
