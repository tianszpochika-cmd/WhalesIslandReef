# D29-S05 正式交互 Canvas 集成交接

## 结论

当前状态：`PARTIAL`。

D29 交互层已补齐运行时输入基础设施：复用或创建 `EventSystem`，确保 `InputSystemUIInputModule` 启用；每个按钮显式绑定 `targetGraphic`；反馈栏位于快捷面板内部；每次命令都有可见成功/失败文案并写入低频日志。

## 已验证

- Unity Asset Refresh 后 Tundra build success。
- Play Mode 键盘回归：`B` 市场、`R` 天气、`Q` 画质、`C` 保存均产生对应可见反馈日志；`I` 仓库入口执行无异常。
- D29 静态交互测试通过；D25—D31 相关静态测试保持通过。
- 日志：`C:/Users/Administrator/AppData/Local/Unity/Editor/Editor.log`。

## 尚未关闭

- Computer Use 无法稳定捕获 Unity GameView，鼠标逐按钮、面板互斥和完整 Player 路线仍没有足够截图/输入证据。
- 保存结果受现有 owner 是否 ready 影响；拒绝时会显示明确失败反馈，不伪造成功。

## 下游

`D29-G06` 已有范围受限 Player 证据，改为 `PARTIAL`；仍等待逐按钮运行矩阵和完整路线证据。

## 2026-09-03 独立 Player 复测补充

临时 Windows Player `D31-Standalone-R4-20260903` 已完成真实冒烟路线，构建结果为 `Build Finished, Result: Success.`：

- 主菜单 → 新游戏 → `InitialHarbor` 成功进入；
- 点击 D29 酒馆后，专用 D10 酒馆页显示，D29 快捷 HUD 隐藏；
- 点击“关闭酒馆 [Esc]”后，D29 快捷 HUD 恢复；重新打开后按 `Escape` 也能恢复；
- `B` 打开并关闭市场面板；`R` 切换到 Rain 表现并产生成功回执；
- runtime log 未发现异常级 `Exception`、`Error`、`MissingComponent` 或 `Crash`；其中 D7 的 `error=0.002` 是数值指标，不是错误日志。

原始证据：

- [Standalone build log](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R4.log)
- [Standalone runtime log](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R4.log)
- [D29 machine result](../../../../game/tools/results/D29/D29-S05/D29_S05_RESULT.json)

这次复测只关闭了“没有任何 Player 证据”的旧阻断，不代表完整 D30 路线、逐按钮矩阵、固定镜头或性能门禁已通过，因此 D29-S05 仍为 `PARTIAL`，D29-G06 不自动升级。

## 2026-09-03 R6 完整交互路线补充

最新受控构建为 `D31-Standalone-R6-20260903`，构建日志包含 `Build Finished, Result: Success.`。本次路线为：

`MainMenu → NewGame → InitialHarbor → I 仓库只读页 → P D9 整备/泊位 → D4 船坞内容页 → E 登船 → T 出航 → N×2 推进/抵达 → L 靠港结算 → C 保存 → 关闭重启 → Continue 读取绿湾村`

已观察到：D9 和 D4 临时页打开时 D25 主 HUD 隐藏，关闭按钮后恢复；泊位分配、船坞请求、登船、航程、靠港、结算和保存均有真实运行回执。重新启动同一 R6 Player 选择 Continue 后进入保存后的绿湾村状态。

- [R6 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R6.md)
- [R6 构建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R6.log)
- [R6 新游戏路线日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R6.log)
- [R6 Continue 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_CONTINUE_R6.log)

本补充将 D29-S05 的 Player 证据推进到范围受限的完整纵向切片，但不关闭逐按钮成功/失败/禁用矩阵、14 固定镜头、三天气过程或性能门禁；状态仍为 `PARTIAL`，D29-G06 仍需后续验收。

## 2026-09-03 交互矩阵静态收口

已建立 [D29—D30 交互按钮矩阵](../../D29_D30_INTERACTION_BUTTON_MATRIX.md)，覆盖 D29 主 HUD、D29 仓库关闭、D4 内容页和 D9 整备页共 29 个可见按钮，另列 4 个 D4 键盘专用入口。只读审计逐项确认源文件、回调方法、owner 归属和可见反馈标记，结果为 `29 buttons + 4 keyboard-only controls PASS`。

- [机器审计结果](../../../../game/tools/results/D29/D29-S05/D29_D30_INTERACTION_MATRIX_AUDIT.json)
- [审计脚本](../../../../tools/d29_d30_interaction_matrix_audit.py)

这只证明代码接线和反馈路径完整，不替代当前 Player 的逐按钮成功/失败/禁用运行证据；D29-S05 继续为 `PARTIAL`，D29-G06 继续等待运行时矩阵。

## 2026-09-03 R8 仓库模态互斥回归

R8 是在 D29 仓库临时页互斥修复和 D30 顶部路线条安全区修复后的新构建。受控路线为：

`InitialHarbor → I 打开仓库只读面板 → Escape 关闭 → 恢复主 HUD`

真实 Player 画面确认：仓库面板打开时 D25 主 HUD 与 D30 路线条隐藏；Escape 关闭后 D25、D30 和 D29 主层恢复。runtime log 记录仓库打开/关闭成功回执，未发现异常级 `Exception`、`Error`、`MissingComponent` 或 `Crash`。

- [R8 构建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R8.log)
- [R8 runtime 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R8.log)
- [R8 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R8.md)

本次关闭仓库叠层问题，但不关闭 29 个可见按钮逐项运行矩阵、失败/禁用分支、D30 全路线、固定镜头和性能门禁；D29-S05 与 D29-G06 均保持 `PARTIAL`。

## 2026-09-03 R11 输入归属与安全区复核

R11 在主快捷键让路、D9/D4 长摘要安全区和整备页键盘关闭兜底修复后构建成功。实际路线：

`MainMenu → NewGame → InitialHarbor → P 打开 D9 → Esc 关闭 → 点击 D29 船坞 → D4 内容页 → R 不触发 D29 天气 → Esc 关闭`

已确认：`P` 能打开 D9；D9/D4 摘要不再覆盖中央港口标题；临时页打开时 D29 的天气等主快捷键不抢输入；D9/D4 均能用 Esc 关闭并恢复主 HUD。

- [R11 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R11.md)
- [R11 构建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R11.log)
- [R11 runtime 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R11.log)

本次修复了按键串层和整备页 Esc 兜底，但没有关闭逐按钮成功/失败/禁用矩阵、D30 全路线、固定镜头、三天气和性能门禁；D29-S05 与 D29-G06 继续为 `PARTIAL`。

## 2026-09-03 R13 D9 输入所有权回归

R13 在 D3/D4/D5/D2 主输入入口增加临时页面让路，并为 D9 增加同一意图的 Player 键盘边沿兜底。实际路线：

`MainMenu → NewGame → InitialHarbor → P D9 → B 泊位 → M 买盐 → T D10 酒馆 → Esc → B 主 HUD 市场 → Esc → P D9 → U 船坞门禁反馈 → Esc`

真实 Player 观察：D9 打开时按 `B` 保持在 D9，日志出现“已分配泊位”，没有再误打开 D3 贸易；按 `M` 完成买盐并保持在 D9；按 `T` 进入 D10，Esc 恢复主 HUD；主 HUD 关闭临时页后按 `B` 仍可打开 D3 贸易；`U` 显示声望不足的明确拒绝反馈。R13 构建成功，定向模态/输入回归 `15/15 PASS`。

- [R13 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R13.md)
- [R13 构建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R13.log)
- [R13 runtime 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R13.log)

R13 只补充代表性输入所有权和反馈证据，不关闭 29 个可见按钮逐项运行矩阵、D30 全路线、固定镜头、三天气或性能门禁；D29-S05 与 D29-G06 仍为 `PARTIAL`。

## 2026-09-03 R16 全入口运行切片与 R18 相机回归

R16 在独立 Windows Player 中按 `D29_D30_INTERACTION_BUTTON_MATRIX.json` 执行了 D29 主 HUD、仓库、D4 内容页和 D9 整备页的可见入口点击，并补充执行 D4 的 `1/2/3/B/E/C` 键盘专用入口。已执行入口均产生可见反馈或明确拒绝文案；D9/D4/仓库的临时页关闭后能恢复主 HUD。完整的逐条禁用态、焦点转移、重复点击和异常恢复记录仍未完成。

R18 在 `CameraFollow` 中修复了受控船体自碰撞过滤，并让进入船控时从 Character 预设切到 ShipDeck 预设。R18 构建成功，受控 Player 观察到登船后镜头不再被自身船体碰撞体压到贴脸距离；低模船体、港口几何和最终构图仍需 D31-P04/D31-G06 验收。

- [R16 矩阵路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R16_FULL_MATRIX.md)
- [R16 矩阵 runtime 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R16_FULL_MATRIX.log)
- [R18 构建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R18.log)

本补充将 D29 的 Player 证据推进为“可见入口运行切片”，但不把静态矩阵或代表性点击误报为完整 `RUNTIME_PASS`；D29-S05 和 D29-G06 继续保持 `PARTIAL`。

## 2026-09-03 R19/R20 代码逻辑收口

R19/R20 未重复运行 Player，而是针对已确认的交互缺陷完成代码级收口：

- `D16ModalFocusScope` 现在检查 EventSystem、父 Canvas/CanvasGroup 可见性与可交互性；隐藏页面会清理自身选择焦点。
- D29 主面板、仓库、D4、D9、D10 页面都接入模态焦点边界，避免隐藏页面继续持有键盘焦点。
- D4、D9、D10 的 owner 未就绪路径改为显示明确的“尚未就绪/当前不可用”反馈；D29 不再强行打开未就绪的 D10。
- 定向逻辑回归 `19/19 PASS`，纳入 D25—D31 的选择性回归 `42/42 PASS`，Unity Tundra 脚本编译成功且无 `error CS`。

本轮只证明源码逻辑和编译正确，不能替代真实 Player 的逐按钮禁用态、焦点、重复点击和异常恢复取证；D29-S05 与 D29-G06 仍保持 `PARTIAL`。

- [R20 交互逻辑审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R20.json)
- [R20 Unity 编译日志](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R20.log)

## 2026-09-03 R24 失败反馈与状态保护

R24 在不改变 D3/D21 业务 owner 的前提下修复了表现层误反馈：D29 市场入口先等待 `RequestTrade()` 结果，拒绝时不打开空的 `D3Canvas`；仓库面板或内容区域缺失时给出可见反馈，关闭仓库时显示恢复回执；D30 对市场/登船失败和未知事件保持当前阶段，不再显示“已切换表现上下文”。

代码级结果：`STATIC_LOGIC_ONLY / PASS`；29 个可见入口、4 个键盘入口；选定回归 `46/46 PASS`；Unity batchmode `Tundra build success (2.95 seconds)`，无 `error CS`。

- [R24 交互逻辑审计](../../../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R24.json)
- [R24 Unity 编译日志](../../../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R24.log)

R24 只关闭源码层误反馈风险；真实 Player 的逐按钮失败/禁用/焦点/重复点击/异常恢复矩阵仍未完成，D29-S05 与 D29-G06 继续保持 `PARTIAL`。

## 2026-09-04 R74 仓库 I/Esc 关闭优先级修复

### 修改

- `D29InteractionHud.Update` 现在先判断仓库模态是否可见；仓库打开时，`I` 或 `Escape` 立即调用 `CloseWarehouse()` 并结束本帧处理；
- 只有仓库关闭请求处理完后，才进入 `RuntimeHudCoordinator.PrimaryInputAllowed` 主 HUD 快捷键门禁；因此 B/R/C/Q/P 等快捷键仍不会穿透仓库；
- 该修复只调整表现层输入分发顺序，不改变 D3 库存事实、存档、业务 owner、交互裁决或正式场景。

### 验证

- D31 静态交互审计：`PASS / STATIC_LOGIC_ONLY`，仓库关闭优先级 `true`；
- Python 定向回归：`39/39 PASS`；
- Unity Tundra 编译：退出码 `0`，`Tundra build success`，`error CS=0`；
- 本轮没有启动 Player 或性能长测，`runtime_proof=false`。

证据：

- [R74 交互静态审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R74.json)
- [R74 逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R74.log)
- [R74 Unity 编译](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R74B.log)
- [D29 结果](../../../../game/tools/results/D29/D29-S05/D29_S05_RESULT.json)

### 状态边界

R74 为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`，只关闭仓库 `I` 快捷键被主输入门禁误拦截的代码层缺口。完整 Player 物理输入、负向/禁用/焦点/重复点击矩阵、D30 实机异常恢复、最终画质和性能 1% low 仍未验收；D29-S05 与 D29-G06 继续保持 `PARTIAL`。
