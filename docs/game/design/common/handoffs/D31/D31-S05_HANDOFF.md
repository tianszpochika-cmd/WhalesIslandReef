# D31-S05 Visual Candidate 集成交接

## 结论

当前状态：`PARTIAL`。

D25—D31 的代码路径已统一，D27 玩家表现时序和 D29 交互输入基础已修复；Unity fresh compile/domain reload 无新增编译错误或运行异常。D31 仍不宣称 Visual Candidate 或 Runtime 通过。

## 2026-09-03 R50 表现层重复标牌清理

已完成一项范围受限的港口可读性修复：R03 生成器不再为四栋程序化建筑生成后立面 `BackSignBoard`/`BackSign`，避免码头斜视角下同名建筑标签从侧面露出形成重复文字。后立面门、窗、棚架和建筑几何保留；交互锚点、碰撞、业务 owner、存档和天气逻辑未修改。

- 生成器：[D19R03HarborPresentationBuilder.cs](../../../../game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs)
- Unity 重建日志：[D31_S05_UNITY_VISUAL_REBUILD_R50.log](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R50.log)
- Unity 审计日志：[D31_S05_UNITY_VISUAL_AUDIT_R50.log](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R50.log)
- 回归日志：[D31_S05_VISUAL_LOGIC_REGRESSION_R50.log](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R50.log)
- Editor 审计：[D19_R03_A02_EDITOR_AUDIT.json](../../../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- 结果登记：[D31_S05_RESULT.json](../../../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json)

结果：`307 Renderer`、`22004 mesh vertices`、`1 Collider`，Prefab 内后立面标牌标记为 `0`，D28—D31 相关回归 `47/47 PASS`（其中 D19 R03 builder 测试 5/5），Unity batchmode 重建/审计退出码 0。随后 R50 使用真实独立 Windows x64 Player 重新采集 `14/14` 固定镜头和 `3/3` Calm/Rain/Storm 天气镜头，`1920×1080` backbuffer 证据完整；截图确认重复标牌已清理，但低模/程序化建筑、平坦地表、杂乱竖向几何、粗糙环境道具和高密度 HUD 仍存在。因此 D31-S05/G06 继续 `PARTIAL`，不得把采集数量当作最终画质通过。

R50 Player 证据：

- [Player 视觉证据 JSON](../../../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R50.json)
- [R50 复验结论](../../../../game/tools/results/D31/D31-P04/D31_P04_R50_REVALIDATION_RESULT.json)
- [R50 截图目录](../../../../game/tools/results/D31/D31-P04/visual_evidence_R50/)
- [Player 构建日志](../../../../game/tools/results/D31/D31-P04/D31_P04_BUILD_PLAYER_R50.log)
- [Player 运行日志](../../../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_PLAYER_R50.log)

## 已验证

- 42/42 D25—D31 静态/单元测试通过。
- Unity Windows Player R6/R8/R11/R13/R16/R18 build success。
- R16 真实交互回归产生 D29/D4/D9 入口反馈和 D30 出航、返港、保存、Continue 日志；R18 复核船控相机自碰撞修复。
- R14 已实际捕获 14 个固定镜头和 Calm/Rain/Storm 三天气变体，共 17 张非空 PNG。

## 阻断

- R6/R8/R11/R13/R16/R18 独立 Player 已形成可靠的范围受限路线、模态和输入归属证据；固定镜头/天气的数量捕获已经完成，但天气可读性、性能和完整交互矩阵仍未收口。
- 因此 `D31-G06` 改为 `PARTIAL`，仍不可改成 `VISUAL_CANDIDATE`。

## 2026-09-03 独立 Player 复测补充

已完成一次受控 Windows Player 构建与真实交互冒烟，`D31-Standalone-R4-20260903` 的 Unity 构建结果为 `Build Finished, Result: Success.`。路线为：

`MainMenu → NewGame → InitialHarbor → D29 酒馆 → 关闭按钮 → Escape 关闭 → B 市场 → 关闭 → R Rain 表现`

复测观察：D29 临时面板与 D10 酒馆页互斥；关闭按钮和 Escape 都能恢复主 HUD；市场面板可打开/关闭；Rain 表现产生 D28/D29 成功日志；D27 先等待 D2 玩家实例化、随后恢复 `playerRenderers=1`。runtime log 未发现异常级 `Exception`、`Error`、`MissingComponent` 或 `Crash`；其中 D7 的 `error=0.002` 是数值指标，不是错误日志。

原始证据：

- [Standalone build log](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R4.log)
- [Standalone runtime log](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R4.log)
- [D31 machine result](../../../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json)

本证据把 D31-S05 从“尚无 Player 构建”推进到“有范围受限的 Player 冒烟证据”，但不关闭完整 Visual Candidate 门禁。D30 全阶段视觉 fingerprint、Low/Medium/High 实测性能、天气细节可读性、负向交互矩阵和最终画面质量仍未满足，所以 D31-S05 保持 `PARTIAL`，D31-G06 保持 `PARTIAL`。

## 2026-09-03 R6 路线补充

R6 是当前最新的独立 Windows Player 构建。已执行并记录：

`新游戏 → InitialHarbor → 仓库只读 → D9 整备/泊位 → D4 船坞内容 → 登船 → 出航 → N×2 → 靠港结算 → 保存 → 关闭重启 → Continue`

R6 日志显示 D27 在 D2 玩家实例化后恢复 `playerRenderers=1`；D9/D4 临时面板关闭后恢复主 HUD；真实航程、靠港、结算、保存和 Continue 读取均有日志。runtime 日志未发现异常级 `Exception`、`Error`、`MissingComponent` 或 `Crash`。FMOD 无音频设备时走既有 fallback，未被当成音频硬件验收通过。

- [R6 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R6.md)
- [R6 构建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R6.log)
- [R6 新游戏路线日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R6.log)
- [R6 Continue 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_CONTINUE_R6.log)

本补充只证明 D31-S05 已有范围受限的真实交互纵向切片，不代表 Visual Candidate 已完成。D30 负向/异常恢复、Low/Medium/High 性能、天气细节可读性和最终画面质量仍需后续任务闭环，D31-S05 与 D31-G06 保持 `PARTIAL`。

## 2026-09-03 R8 仓库模态互斥复核

R8 在 D30 路线条安全区与 D29 仓库临时页互斥修复后构建成功。Player 路线：

`InitialHarbor → I 仓库只读面板 → Escape 关闭 → 恢复主 HUD`

画面确认仓库面板居中显示时 D25/D30 主层隐藏，关闭后 D25/D30/D29 恢复；日志记录打开/关闭成功回执，未发现异常级 `Exception`、`Error`、`MissingComponent` 或 `Crash`。该结果补强 D29/D31 的模态交互证据，但不证明完整 Visual Candidate。

- [R8 构建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R8.log)
- [R8 runtime 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R8.log)
- [R8 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R8.md)

剩余门禁：R14 固定镜头/三天气已完成文件捕获，但 Rain/Storm 可读性仍弱；29+4 入口的负向/禁用/焦点/重复点击、D30 全阶段异常恢复、Low/Medium/High 性能和最终画面质量仍未闭环。

## 2026-09-03 R11 输入归属与页面安全区复核

R11 是输入所有权、D9/D4 标题安全区和整备页键盘关闭兜底修复后的独立 Player。路线：

`MainMenu → NewGame → InitialHarbor → P 打开 D9 整备页 → Esc 关闭 → 点击 D29 船坞 → D4 内容页 → R 不触发 D29 天气 → Esc 关闭`

真实画面确认 D9/D4 长摘要位于中央港口标题下方；`P` 打开 D9，`Esc` 关闭 D9/D4，临时页打开时 D29 主快捷键让路。runtime log 未发现异常级 `Exception`、`MissingComponent` 或 `Crash`；`D7 error=0.002` 仍是数值校准字段。

- [R11 构建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R11.log)
- [R11 runtime 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R11.log)
- [R11 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R11.md)

R11 将 D31 交互切片推进到输入归属和关闭恢复可复核，但不关闭完整 Visual Candidate 门禁：固定镜头、三天气、逐按钮运行矩阵、D30 全路线和性能仍为 `PARTIAL`。

## 2026-09-03 R13 D9/D3 输入所有权复核

R13 是一次针对真实 Player 按键串层问题的修复后构建。修复内容：D3/D4/D5/D2 的主输入在临时页面打开时统一让路；D9 的 B/M/X/T/S/R/Q/W/U/D 使用同一意图的 Player 兼容边沿。实际路线：

`MainMenu → NewGame(CodexCaptainR13) → InitialHarbor → P(D9) → B(泊位) → M(买盐) → T(D10) → Esc → B(主 HUD 交易) → Esc → P(D9) → U(声望门禁) → Esc`

已确认：D9 的 `B` 不再误开 D3 贸易，D9 `M` 修改库存并显示购买反馈，D9 `T` 进入酒馆、Esc 恢复主 HUD，主 HUD 关闭临时页后 `B` 仍能打开 D3 贸易，D9 `U` 显示明确的声望不足反馈；R13 构建成功；定向回归 `15/15 PASS`；runtime log 未发现异常级 `Exception`、`Error`、`MissingComponent` 或 `Crash`。

- [R13 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R13.md)
- [R13 构建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R13.log)
- [R13 runtime 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R13.log)

R13 只关闭了一个已复现的输入串层缺陷，不关闭最终门禁：14 固定镜头、Calm/Rain/Storm 完整过程、29+4 入口逐项运行、D30 异常恢复、Low/Medium/High 性能和最终画面质量仍为 `PARTIAL`。

## 2026-09-03 R16/R18 收口补充

R16 补充了 D29/D4/D9 可见入口的 Player 点击切片、D4 键盘专用入口和 D30 出航/返港/保存/Continue 路线；R16 的两份路线报告与 runtime 日志已归档到 `game/tools/results/D31/D31-S05/`。R18 在 `CameraFollow` 中加入受控船体自碰撞过滤，并在进入船控时自动切换 ShipDeck 预设；R18 Unity Player 构建成功，受控观察确认登船镜头不再被自身船体碰撞体压成贴脸视角。

- [R16 全入口矩阵报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R16_FULL_MATRIX.md)
- [R16 D30 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R16_D30.md)
- [R18 构建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R18.log)

当前仍不能升级为 `VISUAL_CANDIDATE`：R14 已提供 14 固定镜头和 Calm/Rain/Storm 三天气变体的数量证据，但 Rain/Storm 雨丝与天气差异可读性不足；逐入口负向/禁用/焦点/重复点击、D30 异常恢复、最终低模画面改造和 Low/Medium/High 的 1% low 达标门禁尚未闭环。D31-S05 与 D31-G06 保持 `PARTIAL`。

## 2026-09-03 R19 代码级焦点收口

R19 没有重复构建 Player，只收口 D31 交互逻辑中已经明确的焦点边界：D29 主 HUD、仓库、D4 内容页、D9 整备页和 D10 酒馆页均绑定现有 `D16ModalFocusScope`；焦点范围按页面层级建立显式导航；当页面的 Canvas、CanvasGroup 或根对象不可见/不可交互时，焦点组件会清理自己持有的 EventSystem 选择，避免隐藏页面继续抢键盘/手柄确认键。

代码级审计结果：29 个可见入口、4 个 D4 键盘入口，`16/16 PASS`。结果文件：

- [R19 interaction logic audit](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R19.json)
- [R19 audit tool](../../../../tools/d31_interaction_logic_audit.py)
- [R19 tests](../../../../tools/tests/test_d31_interaction_logic_audit.py)

R19 的 Unity batchmode 脚本编译也已通过，日志为 `Tundra build success (4.21 seconds)`，没有 `error CS`；仍保留项目原有弃用警告和 licensing warning。R19 仍是代码级证据，不等同 Player 运行通过；D29-G06、D30-G06、D31-G06 仍需补逐入口禁用/失败/重复点击、异常恢复、天气可读性、性能和最终画面证据。

- [R19 Unity compile log](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R19.log)

## 2026-09-03 R20 不可用 owner 反馈收口

R20 修复了 D4、D9、D10 在 owner 尚未就绪或必要读模型缺失时的静默 `return`：现在会写入页面反馈区并输出带模块前缀的 warning；D29 打开酒馆前也检查 D10 `IsReady`，不会把未绑定页面显示成已打开。业务数据仍只由原有 owner 写入，UI 只负责提示。

R20 的代码级审计和 Unity 编译均通过：完整回归 `42/42 PASS`，定向交互回归 `19/19 PASS`，Unity `Tundra build success (3.28 seconds)`，无 `error CS`。结果文件：

- [R20 interaction logic audit](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R20.json)
- [R20 Unity compile log](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R20.log)

这只关闭源码层的静默失败风险；没有重复 Player，因此 D29-G06、D30-G06、D31-G06 的运行时禁用/失败/重复点击、异常恢复、天气可读性、性能和最终画面门禁仍保持 `PARTIAL`。

## 2026-09-03 R24 交互失败反馈保护

R24 修复了三个表现层状态错误：D29 市场请求在 owner 返回前不再打开空的交易页；仓库面板/内容区域缺失时显示可见失败反馈；D30 对市场失败、登船失败和未知事件保持当前阶段并显示“请求未通过，状态未改变”。不修改 D3/D21 业务 owner、存档字段或航行事实。

- [R24 交互逻辑审计](../../../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R24.json)
- [R24 Unity 编译日志](../../../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R24.log)

R24 结果为 `STATIC_LOGIC_ONLY / PASS`，29 个可见入口和 4 个键盘入口仍覆盖完整矩阵；选定回归 `46/46 PASS`，Unity `Tundra build success (2.95 seconds)`，无 `error CS`。本轮仍没有把源码证据冒充 Player 证据：真实 Player 的逐入口失败/禁用/焦点/重复点击、D30 异常恢复、最终低模画面质量和性能 1% low 仍未闭环，D31-S05/G06 保持 `PARTIAL`。

## 2026-09-03 R25 港内表现阶段保持

R25 修复 D30 港内表现阶段被下一帧刷新错误重置的问题：`Market`、`Tavern`、`Shipyard` 在 owner 仍处于港内时保持当前投影；船上、航行和返港状态仍由 D2/D3 owner 覆盖。R24 的失败请求保护和失败阶段反馈继续保留。

- [R25 交互逻辑审计](../../../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R25.json)
- [R25 Unity 编译日志](../../../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R25.log)

R25 为 `STATIC_LOGIC_ONLY / PASS`；完整选定回归 `47/47 PASS`；Unity `Tundra build success (4.07 seconds)`，无 `error CS`。这仍不等同 Player 运行通过，D29-G06、D30-G06、D31-G06 的真实负向/禁用/重复点击、异常恢复、最终画面质量和性能门禁继续为 `PARTIAL`。

## 2026-09-03 R26 额度友好复核

R26 只进行轻量、离线的交付一致性复核，没有修改代码、场景、Prefab、资源或业务数据，也没有重复启动 Player 或长时间性能任务。

- 选定 D25—D31 测试模块：`47/47 PASS`；
- `D31` 交互静态审计：`PASS`，29 个可见入口 + 4 个键盘入口；
- `D29/D30` 交互矩阵审计：`PASS`，29 个可见按钮 + 4 个键盘入口；
- 结果 JSON：`57/57` 可解析；`git diff --check` 通过；
- 回归日志：[D31_S05_LOGIC_REGRESSION_R26.log](../../../../../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R26.log)。

本次只确认 D31 交互代码层稳定，不把静态审计升级成 Player 运行通过。D29-G06、D30-G06、D31-G06 仍因逐入口运行时负向/禁用/重复点击、D30 异常恢复、最终画面质量和性能 1% low 证据不足而保持 `PARTIAL`。

## 2026-09-03 R21-R23 表现复核补充

R21-R22 修复并验证了 D28 Rain/Storm 的表现层：降水对象跟随主相机，使用透明拉伸粒子，并按天气设置雨量、风向、颜色和水面波动倍率。R23 在同一独立 Player 构建中复核 D30 路线条安全区，将其下移到 `anchoredPosition = (0, -174)`；港口主标题、副标题和路线信息不再互相压叠。

R23 保留 14 个固定镜头与 Calm/Rain/Storm 三天气，共 17 张 PNG 的机器证据，且构建成功：

- [R23 视觉证据 JSON](../../../../../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R23.json)
- [R23 截图目录](../../../../../../game/tools/results/D31/D31-P04/visual_evidence_R23/)
- [R23 Player 构建日志](../../../../../../game/tools/results/D31/D31-P04/D31_P04_STANDALONE_BUILD_R23.log)

本轮只关闭“天气完全不可读”和“路线条压住副标题”两个具体缺陷；低模建筑/材质、完整负向/禁用/重复点击矩阵、D30 异常恢复和性能 1% low 门禁仍未通过。D31-S05、D31-G06 以及相关 D28/D29/D30 Gate 继续保持 `PARTIAL`，不得据此宣称 `VISUAL_CANDIDATE`。

## 2026-09-03 R27 港口倒角网格修补

R27 找到并修复了一个直接导致港口建筑、门窗、箱子、码头边缘和船体呈硬直角方块的实现缺陷：`D19R03HarborPresentationBuilder.MakeBevelBoxMesh` 旧实现虽然被大量视觉部件调用，却直接返回 `MakeBoxMesh`。现在该方法生成封闭倒角网格，并对面、边和角的三角形绕序做校正；只修改编辑器表现生成器和其生成的 R03 Prefab，不改变 D2/D3/D6/D21 owner、碰撞所有权、输入、存档或资源状态。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_VISUAL_REBUILD_R27.log`
- `game/tools/results/D31/D31-S05/D31_S05_VISUAL_AUDIT_R27.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R27.log`
- `game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json`
- `tools/tests/test_d19_r03_visual_builder.py`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R27.log`

R27 结果：生成器静态测试 `3/3 PASS`，选定 D25—D31 回归 `50/50 PASS`，Unity Tundra 短编译成功；Editor 审计为 `315 Renderer / 15,609 mesh vertices / 1 R03 collider`。本轮为额度友好的源码/Editor 修补，没有重新运行 Player、固定镜头或性能采样，所以 D31-S05/G06 仍为 `PARTIAL`，最终视觉质量待后置 Player 复核。

## 2026-09-03 R28—R29 固定镜头证据归属与当前布局复核

R28 发现 D31 视觉探针的固定镜头请求会被 `CameraFollow.LateUpdate` 覆盖，且旧镜头坐标属于退役布局。已修复 `D31VisualEvidenceProbe`：在 InitialHarbor 视觉捕获期间获取并暂时禁用 `CameraFollow`，退出前恢复原状态；同时增加静态回归，防止探针再次失去相机所有权。

R29 将固定镜头和天气镜头调整到当前 D19-R03 港口坐标系，并使用独立 Windows x64 Player 实际捕获。结果为 `14/14` 固定镜头、`3/3` Calm/Rain/Storm 天气镜头，全部为 `1920×1080` backbuffer PNG；JSON 记录的相机位置与请求位置一致，R27 倒角表现网格已在 Player 中出现。截图仍能看出低模/程序化材质和少数近景遮挡，因此这轮只关闭证据归属与 R27 Player 复核缺口，不升级最终画质门禁。

验证结果：选定 D25—D31 回归 `52/52 PASS`；`d31_interaction_logic_audit.py` 为 `STATIC_LOGIC_ONLY / PASS`（29 个可见入口 + 4 个键盘入口）；D29/D30 矩阵审计为 `PASS`；结果 JSON `58/58` 可解析；R29 Player 构建成功。D31-S05/G06 仍为 `PARTIAL`，因为逐入口负向/禁用/重复点击、D30 异常恢复、最终画面质量和性能 1% low 尚未闭环。

- [R29 视觉证据 JSON](../../../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R29.json)
- [R29 截图目录](../../../../game/tools/results/D31/D31-P04/visual_evidence_R29/)
- [R29 Player 构建日志](../../../../game/tools/results/D31/D31-P04/D31_P04_STANDALONE_BUILD_R29.log)
- [D31 视觉探针源码](../../../../game/Assets/Scripts/Gameplay/D31/D31VisualEvidenceProbe.cs)
- [D31 视觉探针测试](../../../../tools/tests/test_d31_visual_evidence.py)

## 2026-09-03 R30 owner 结果传播与失败阶段保护

R30 修复了 D29 交互层把 owner 拒绝误报为成功的代码风险。D10 现在向表现层提供只读的进入结果、焦点状态和最后反馈；D4 保留原有 `UpgradeHull()` 兼容入口，同时增加返回 owner 接受结果的 `TryUpgradeHull()`、只读反馈和页面可见性；D9 入口读取已有焦点状态。D29 只有在对应 owner 真正接受且页面状态满足条件时才通知 `tavern_opened`、`shipyard_opened` 或 `preparation_opened`，否则显示 owner 原因并通知失败事件。D30 对三类新失败事件保持当前阶段，不覆盖成错误的路线阶段。

验证：选定 D25—D31 回归 `54/54 PASS`；D31 交互静态审计 `STATIC_LOGIC_ONLY / PASS`；D29/D30 矩阵审计 `PASS`；Unity `Tundra build success (3.37 seconds)`，无新增 `error CS`；D25—D31 结果 JSON `59/59` 可解析。R30 没有把静态结果升级为 Player 运行通过，D29-G06、D30-G06、D31-G06 仍保持 `PARTIAL`，真实负向/禁用/重复点击矩阵、D30 异常恢复、最终画面质量和性能 1% low 仍待后置。

- [R30 交互逻辑审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R30.json)
- [R30 逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R30.log)
- [R30 Unity 编译](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R30.log)

## 2026-09-03 R31 天气与画质入口 owner 缺失反馈

R31 将 D29 天气和画质切换从无条件成功提示改为结果驱动：D28 提供 `TryCycleWeather()`，D31 提供 `TryCycleQuality()`，旧的 `CycleWeather()`/`CycleQuality()` 保留为兼容包装；当表现 owner 不存在时，D29 显示明确失败反馈。当 owner 存在并完成切换时才显示成功反馈。未改变业务状态、存档、网络或 D21 天气事实。

验证：选定 D25—D31 回归 `55/55 PASS`；D31 交互静态审计 `STATIC_LOGIC_ONLY / PASS`；D29/D30 矩阵审计 `PASS`；Unity `Tundra build success (2.89 seconds)`，无新增 `error CS`；新增 R31 记录前结果 JSON `59/59` 可解析，新增记录后当前总数 `60/60`。本轮不把静态结果升级为 Player 运行通过，D29-G06、D30-G06、D31-G06 仍为 `PARTIAL`。

- [R31 交互逻辑审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R31.json)
- [R31 逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R31.log)
- [R31 Unity 编译](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R31.log)

## 2026-09-03 R32 D29 可用性/防重与 D30 重复事件保护

R32 继续按代码层交互目标收口：D29 在 owner 解析后更新各主按钮的 `Button.interactable`，并在状态栏列出市场、酒馆、船坞、登船、天气、整备等不可用原因；市场、酒馆、船坞、登船、整备和保存等一次性请求使用 0.35 秒表现层防重，重复点击显示明确反馈，天气/画质预览保持可重复。D30 新增 `preparation_opened` 成功投影，并对短窗口重复事件保持当前阶段、显示“重复事件已忽略，状态未改变”。同时删除了可用性 setter 中的无意义名称自赋值。

验证：选定 D25—D31 回归 `57/57 PASS`；D31 交互静态审计 `STATIC_LOGIC_ONLY / PASS`；D29/D30 矩阵审计 `PASS`；Unity `Tundra build success (2.89 seconds)`，无新增 `error CS`；R32 新增前 D25—D31 直接结果 JSON `60/60` 可解析，新增 R32 结果后为 `61/61`（Unity Library 夹具 JSON 不计入）。

- [R32 交互逻辑审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R32.json)
- [R32 逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R32.log)
- [R32 Unity 编译](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R32.log)

R32 仍是源码层证据，不替代 Player 逐入口负向/禁用/焦点/重复点击、D30 全阶段异常恢复、最终画面质量和性能 1% low 门禁；D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`。

## R33 追加：首帧按钮状态与酒馆拒绝回退

- D29 `Awake` 在解析 owner 后立即刷新 `Button.interactable`，避免首帧可用状态滞后；
- D29 酒馆入口在 D10 拒绝进入时调用 `CloseSocialPage()`，关闭失败临时页并恢复主 HUD，然后发送 `tavern_failed` 与 owner 反馈；
- 选定 D25—D31 回归 `58/58 PASS`；D31 交互静态审计 PASS；D29/D30 矩阵审计 PASS；Unity `Tundra build success (2.54 seconds)`，无新增 `error CS`；
- R33 证据：`game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R33.json`、`D31_S05_LOGIC_REGRESSION_R33.log`、`D31_S05_UNITY_COMPILE_R33.log`；D25—D31 直接结果 JSON `62/62` 可解析（Unity Library 夹具 JSON 不计入）。

R33 只证明代码层回退，不关闭真实 Player 逐入口负向/禁用/重复点击矩阵、D30 全阶段异常恢复、最终画面质量或性能 1% low 门禁；D31-S05/G06 继续保持 `PARTIAL`。

## R34 追加：D29 多入口异常恢复

- D29 对登船、市场、酒馆、船坞和港口整备入口增加统一异常保护；`ReportActionException` 会尽力关闭临时页或恢复主 HUD，向 D30 投影对应失败事件，并显示状态未改变；
- 恢复动作自身异常只保留带模块前缀的 warning，不再次穿透 UI 事件链；
- 选定 D25—D31 回归 `59/59 PASS`；D31 交互静态审计 PASS；D29/D30 矩阵审计 PASS；Unity `Tundra build success (2.55 seconds)`，无新增 `error CS`；D25—D31 直接结果 JSON `63/63` 可解析（Unity Library 夹具 JSON 不计入）。
- R34 证据：`game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R34.json`、`D31_S05_LOGIC_REGRESSION_R34.log`、`D31_S05_UNITY_COMPILE_R34.log`。

R34 仍是代码层证据，不关闭真实 Player 逐入口异常恢复、最终画面质量或性能门禁；D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`。

## 2026-09-03 R35 R03 表现材质与 UV 编辑器侧接入

R35 针对 R03 港口仍呈现纯色/低细节的问题完成一次额度友好的表现层修补：

- `game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs` 将 12/12 个 R03 材质映射到已批准的本地 Poly Haven PBR 底图，并把 6/6 个法线贴图元数据设置为线性 `NormalMap`；
- 岛面与程序生成网格增加投影 UV，按三角面主法线方向选择平面投影；
- 重新生成 R03 表现 Prefab，Editor 审计为 `315 Renderer / 1 Collider / 22532 mesh vertices`；
- 未改变 gameplay owner、碰撞所有权、输入、存档、业务数值或资源事实写者。

验证结果：R35 材质审计 `PASS / EDITOR_STATIC`，12/12 材质、6/6 法线元数据通过；R35 目标测试 `5/5 PASS`；D31 交互静态审计、D29/D30 矩阵和选定逻辑回归通过；Unity `Tundra build success`，无新增 C# 编译错误。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R35.json`
- `game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_REGRESSION_R35.log`
- `game/tools/results/D31/D31-S05/D31_S05_VISUAL_REBUILD_R35.log`
- `game/tools/results/D31/D31-S05/D31_S05_VISUAL_AUDIT_R35.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R35.log`

状态边界：R35 只证明源码/Editor 表现改进，没有 Player 最终画质或性能证据；D31-S05/G06、D26-S05/G06 继续 `PARTIAL`。后续若额度允许，先做短 Player 视觉复核，再决定是否进入更重的材质/灯光/性能工作。

## 2026-09-03 R36 保存与整备拒绝路径

R36 完成 D31 交互代码层的另一轮边界收口：

- D29 保存 owner 拒绝投影 `save_failed`；D3 保存和基础存档异常统一使用 `ReportActionException`，恢复主 HUD并显示状态未改变；
- D29 保存成功只保留本地成功反馈，不发送未定义的 D30 成功事件，避免路线条把成功保存误报为失败；
- D29 整备入口若调用后没有建立焦点，立即调用 `ClosePreparationPage()`，避免留下遮挡主 HUD 的临时页，再投影 `preparation_failed`；
- D30 对 `save_failed` 保持当前视觉阶段，不把失败保存误投影为路线阶段切换；
- 未改变业务 owner、存档格式、库存、航行、金额或场景资产。

验证：D31 静态交互审计 PASS，D29/D30 矩阵 PASS，R36 定向测试 `25/25 PASS`，Unity `Tundra build success`，无新增 C# 编译错误。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R36.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R36.log`
- `game/tools/results/D31/D31-S05/D31_S05_FULL_LOGIC_REGRESSION_R36.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R36.log`

状态：R36 为 `STATIC_LOGIC_ONLY`；Player 负向/禁用/重复点击、D30 实机异常恢复、最终画质和性能门禁仍保持 `PARTIAL`。

## R37：D30 阶段投影纯逻辑化

### 已完成

- 新增 `game/Assets/Scripts/Gameplay/D30/Logic/D30VisualStagePolicy.cs`，将事件归一化、成功阶段映射、失败/未知保持当前阶段、重复事件防重提取为零 Unity 依赖的纯 C# 策略；
- 新增 `WhalesIslandReef.D30.VisualLogic.Runtime` 程序集，D30 MonoBehaviour 通过 `D30VisualStagePolicy.Evaluate` 消费结果；
- 新增 `game/Assets/Tests/EditMode/D31/D31VisualStagePolicyTests.cs` 及独立测试程序集，覆盖 16 个 EditMode case；
- 静态审计和 D28/D30/D31 Python 回归已改为检查纯策略的实际所有权，避免以协调器内部文本重复作为唯一证据。

### 验证

- Unity EditMode：`16/16 PASS`，结果见 `game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R37.xml`；
- D25—D31 选定 Python 回归：`53/53 PASS`；
- Unity 6000.0.80f1 短编译：`Tundra build success`，无新增 `error CS`；
- D31 交互静态审计：`PASS / STATIC_LOGIC_ONLY`，结果见 `D31_S05_INTERACTION_LOGIC_AUDIT_R37.json`。

### 未关闭项

R37 不提供 Player 运行时负向/禁用/重复点击矩阵、D30 全阶段异常恢复、最终画面质量或 Low/Medium/High 性能 1% low 证据。因此 D29-G06、D30-G06、D31-S05、D31-G06 仍保持 `PARTIAL`，不得把本次代码测试写成 Visual Candidate 或 Release Approved。

## R38：D29 一次性请求防重纯逻辑化

### 已完成

- 新增 `game/Assets/Scripts/Gameplay/D29/Logic/D29OneShotPolicy.cs`，将短窗口防重判断抽为零 Unity 依赖的纯 C# 策略；它不替代业务 owner 的幂等键、权限或拒绝裁决；
- 新增 `WhalesIslandReef.D29.InteractionLogic.Runtime` 程序集，D29 HUD 通过 `D29OneShotPolicy.Evaluate` 消费结果；
- 扩展 `D31VisualStagePolicyTests.cs`，覆盖首次请求、同动作重复、不同动作、窗口外重试和空动作，并保留 D30 阶段事件的全部 case；
- 更新 D31 静态审计，检查 D29 纯策略文件和调用关系。

### 验证

- Unity EditMode：`21/21 PASS`，结果见 `game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R38.xml`；
- D25—D31 选定 Python 回归：`53/53 PASS`；
- Unity 6000.0.80f1 短编译：`Tundra build success`，无新增 `error CS`；
- D31 交互静态审计：`PASS / STATIC_LOGIC_ONLY`，结果见 `D31_S05_INTERACTION_LOGIC_AUDIT_R38.json`。

### 未关闭项

R38 不提供 Player 运行时负向/禁用/重复点击矩阵、D30 全阶段异常恢复、最终画面质量或 Low/Medium/High 性能 1% low 证据。因此 D29-G06、D30-G06、D31-S05、D31-G06 仍保持 `PARTIAL`，不得把本次代码测试写成 Visual Candidate 或 Release Approved。

## 2026-09-03 R40 D31 交互反馈修补与短时 Player UGUI 探针

R40 处理了“owner 已执行但界面仍显示旧结果”的表现层缺口：

- D3 商品上一种/下一种写入明确的商品切换结果；
- D4 势力、炮战、装填、弹种、开炮、接舷、脱离、改装、动物、医疗、主线和保存动作在状态变化后主动刷新 HUD；
- D9 酒馆切页和整备结果在 owner 返回后主动刷新提示；
- D31 交互探针收集并排序全部相关 Text，避免全局状态栏截断造成假阴性；增加显式输出目录参数，保存按钮仅做存在性检查，不写正式存档。

验证结果：

- 静态交互审计：`PASS / STATIC_LOGIC_ONLY`；
- D25—D31 选定 Python 回归：`54/54 PASS`；
- Unity EditMode `Whalebone.Tests.D31.D31VisualStagePolicyTests`：`21/21 PASS`；
- Unity 编译：`Tundra build success`，`error CS=0`；
- 短时独立 Windows x64 Player UGUI 探针：预期 `47`、找到 `47`、可观察变化 `43`、保存存在性 `1`、缺失 `0`、禁用 `0`、调用异常 `0`；`1920×1080`，`Direct3D11`，`NVIDIA GeForce RTX 4070 Ti`；
- Player 运行日志异常扫描：0 个异常级 Exception/Error/MissingComponent/Crash 命中。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_RUNTIME_R40.json`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_EVIDENCE_PLAYER_R40.json`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R40.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R40.log`
- `game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R40.xml`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R40.log`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_BUILD_R40.log`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_RUNTIME_R40.log`

边界：R40 的 `pointerClick` 是 UGUI 回调调用，不是物理 OS 鼠标/键盘输入；`D29-G06`、`D30-G06`、`D31-S05`、`D31-G06` 继续保持 `PARTIAL`。仍需后置补齐 D29 市场重复点击和 D9 已泊位幂等反馈的专门验证、逐按钮禁用/失败/焦点/重复点击矩阵、D30 全阶段异常恢复、最终画质及性能 1% low。

## 2026-09-03 R43 D31 交互短探针最终复核

### 改动

- D31 探针在第二次 Market 调用前关闭 D3 临时页并恢复主 HUD，使 D29 一次性防重回调可以被真实观察；
- D31 探针的主 HUD 恢复诊断改为检查所有同名活动 Canvas，避免兼容对象顺序造成错误的恢复失败；
- Player 构建器和本轮证据升级为 R43；保存入口仍为存在性检查，不触发正式存档写入。

### 结果

- 独立 Player UGUI 探针：预期 `47`、找到 `47`、观察到 `46`（含重复保护 `1`）、保存存在性 `1`、无可见反馈 `0`、缺失 `0`、禁用 `0`、调用异常 `0`；仓库关闭后主 HUD 恢复为 `true`；
- 五个交互表面 D29/D3/D4/D9/D10 均被覆盖；运行日志异常级扫描为 `0`；环境为 `1920×1080`、Direct3D11、RTX 4070 Ti；
- D31 静态审计 `PASS`；D25—D31 选定 Python 回归 `54/54 PASS`；Unity EditMode `21/21 PASS`；Unity 编译 `Tundra build success`，`error CS=0`。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_RUNTIME_R43.json`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_EVIDENCE_PLAYER_R43.json`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R43.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R43.log`
- `game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R43.xml`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R43.log`

### 状态边界

R43 关闭了 R40 的两个探针假阴性，但 `pointerClick` 仍不是物理 OS 输入，所以不升级 D29-G06、D30-G06、D31-S05 或 D31-G06。真实负向/禁用/焦点/重复点击矩阵、D30 全阶段异常恢复、最终画质和性能 1% low 仍以后置门禁处理。

## 2026-09-03 R45 D29 临时面板互斥修复与 D9 离港覆盖

### 已完成

- 修复 D29 运行时控制器宿主与子 Canvas 同名的问题：宿主改为 `D29InteractionHudController`，实际 Canvas 保留 `D29InteractionHudCanvas`；
- `RuntimeHudCoordinator` 现在同步处理直接命中的对象和全部同名 Canvas，避免 `GameObject.Find` 命中错误对象后留下可见/可点击的主 HUD；
- D31 探针补齐交互矩阵中的 `D9-DEPART`，并放在全部港内整备用例之后执行。

### 证据

- Player UGUI 探针：预期 `48`、找到 `48`、观察到 `47`（重复保护 `1`）、保存存在性 `1`、missing/disabled/error/no-visible-feedback 均为 `0`；
- D3/D4/D9/D10 临时页打开时，活动 Canvas 只包含对应临时页，不再包含 `D29InteractionHudCanvas`；关闭后主 HUD 恢复；
- D31 静态审计 `PASS`；D25—D31 选定回归 `55/55 PASS`；Unity EditMode `21/21 PASS`；Unity 编译 `Tundra build success`、`error CS=0`；运行日志异常级扫描 `0`。

证据文件：

- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_RUNTIME_R45.json`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_EVIDENCE_PLAYER_R45.json`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R45.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R45.log`
- `game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R45.xml`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R45.log`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_BUILD_R45.log`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_RUNTIME_R45.log`

### 仍未关闭

R45 只关闭了代码/UGUI 层叠缺陷，不升级 `D31-S05` 或 `D31-G06`。`pointerClick` 不是物理 OS 输入；逐入口物理成功/失败/禁用/焦点/重复点击矩阵、D30 全阶段异常恢复、最终低模画面质量以及 Low/Medium/High 性能 1% low 仍保持后置门禁。保存入口仍未在探针中执行正式写盘。

## 2026-09-03 R46 D30 失败事件全阶段逻辑收口

### 已完成

- `D30VisualStagePolicy` 新增显式 `IsFailureEvent(string eventId)`，将已登记失败事件与成功/未知/空事件分开处理；
- 用 6 类失败事件 × 7 个视觉阶段构成 42 个纯逻辑组合矩阵，验证失败后保持当前视觉阶段且结果为 `Failed`；
- Unity EditMode `23/23 PASS`，D25—D31 选定 Python 回归 `55/55 PASS`，D31 静态审计 `PASS`，Unity 短编译成功且无 `error CS`；
- 没有重新运行 Player，未把代码层证据冒充物理输入、最终画质或性能验收。

### 证据

- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R46.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R46.log`
- `game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R46.xml`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R46.log`

### 状态边界

R46 是 D30 异常恢复的 `STATIC_LOGIC_ONLY` 收口，不升级 `D30-G06`、`D31-S05` 或 `D31-G06`。Player 异常注入、物理 OS 输入矩阵、最终画质和性能 1% low 仍保持后置门禁。

## 2026-09-03 R47 D29 动态可用性与键盘/手柄焦点导航修复

### 已完成

- 发现 D29 owner 延迟就绪后虽然会恢复 `Button.interactable`，但 D16 原焦点列表不会自动包含新启用的控件；
- 新增 `D16ModalFocusScope.RefreshNavigation()`，重建当前模态页活动且可交互 `Selectable` 的显式导航；
- D29 在 owner 可用性签名变化后调用该刷新入口，保证键盘/手柄导航与按钮可用性一致；
- 没有修改领域事实、业务数值、存档、正式场景或网络代码。

### 证据

- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R47.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R47.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R47.log`

针对性 Python 测试 `24/24 PASS`；D25—D31 选定测试 `51/51 PASS`；D31 静态审计 `PASS`；Unity 编译 `Tundra build success (2.46 seconds)`、`error CS=0`。

### 状态边界

R47 为 `STATIC_LOGIC_ONLY`，不替代真实 Player 物理输入、异常注入、最终画质或性能证据；`D29-G06`、`D30-G06`、`D31-S05`、`D31-G06` 继续保持 `PARTIAL`。

## 2026-09-03 R48 临时 Canvas 初始化可见性与 HUD 隔离修复

### 已完成

- D4、D9、D10 临时 Canvas 改为创建时默认禁用，避免 `Awake` 到协调器首帧策略之间把临时页文字绘到主 HUD 上；
- 页面打开仍由 `RuntimeHudCoordinator.ShowTemporaryCanvas(...)` 显式启用，关闭仍由统一恢复入口处理；
- 不修改领域事实、业务数值、存档、正式场景或网络代码。

### 证据

- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R48.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R48.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R48.log`

针对性 Python 测试 `25/25 PASS`；D25—D31 选定测试 `52/52 PASS`；D31 静态审计 `PASS`；Unity 编译 `Tundra build success (2.88 seconds)`、`error CS=0`。

### 状态边界

R48 为 `STATIC_LOGIC_ONLY`，不替代真实 Player 物理输入、异常注入、最终画质或性能证据；`D29-G06`、`D30-G06`、`D31-S05`、`D31-G06` 继续保持 `PARTIAL`。

## 2026-09-03 R49 跨阶段注册表契约回归修复

### 已完成

- 修复 D16-C08 `island_saint_marys` 切片资源与 D15-A01 旧 LOD/资源引用校验之间的不一致；例外仅由明确的 D16-C08 审查证据激活；
- 修复 D16-C08 码头/泊位单锚点与 D15-A04 三锚点规则之间的不一致；普通建筑规则未放宽；
- 为 `asset_ph_crew_male`、`building_saint_marys_dock`、`building_small_boat_pier`、`island_saint_marys` 补齐许可证布尔字段；
- 更新回归测试，防止切片例外扩张为任意集成条目放行。

### 证据

- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R49.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R49.log`

跨阶段定向测试 `40/40 PASS`；D25—D31 选定回归 `93/93 PASS`；全量 Python 回归 `1026/1026 PASS`；D31 静态审计 `PASS`；AgentCore `0 warnings / 0 errors`；`git diff --check` `PASS`。

### 状态边界

R49 是 `STATIC_LOGIC_ONLY` 契约修复，不升级 `D29-G06`、`D30-G06`、`D31-S05` 或 `D31-G06`。没有执行长时间 Player、物理 OS 输入矩阵、最终画质或性能采样。

## 2026-09-03 R51 固定机位棕榈遮挡收敛

R50 Player 复核发现固定市场机位被东侧棕榈树干近距离遮挡。R51 在 `D19R03HarborPresentationBuilder` 中将靠近固定机位的棕榈移到画面边缘，并将表现树干半径从 `0.22` 调整为 `0.14`；没有修改玩法、交互锚点、碰撞、业务 owner、存档或天气逻辑。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R51.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R51.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R51.log`
- `game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R51.json`
- `game/tools/results/D31/D31-P04/D31_P04_R51_REVALIDATION_RESULT.json`
- `game/tools/results/D31/D31-P04/visual_evidence_R51/`

R51 相关回归 `48/48 PASS`，Unity 重建/审计完成，Player 真实 backbuffer 证据为 `14/14` 固定镜头、`3/3` 天气镜头、`1920×1080`。近景树干遮挡减少，但低模/程序化建筑、平坦地表、粗糙环境道具和高密度 HUD 仍存在；D31-S05/G06 继续 `PARTIAL`，不得把本轮范围内通过升级为最终 Visual Candidate。

## 2026-09-03 R52 建筑灰泥材质误用修复

R51 Player 复核发现 `R03_Plaster` 仍使用旧泥地贴图。R52 在 `D19R03HarborPresentationBuilder` 的材质配置阶段清除 `R03_Plaster` 的旧 BaseMap、MainTex 和 BumpMap，保留暖色灰泥 BaseColor；未修改玩法、交互锚点、碰撞、业务 owner、存档或天气逻辑。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R52.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R52.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R52.log`
- `game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R52.json`
- `game/tools/results/D31/D31-P04/D31_P04_R52_REVALIDATION_RESULT.json`
- `game/tools/results/D31/D31-P04/visual_evidence_R52/`

R52 相关回归 `49/49 PASS`，Unity 重建/审计完成，Player 真实 backbuffer 证据为 `14/14` 固定镜头、`3/3` 天气镜头、`1920×1080`。墙面材质可读性改善，但低模/程序化建筑、平坦地表、粗糙环境道具和高密度 HUD 仍存在；D31-S05/G06 继续 `PARTIAL`。

## 2026-09-03 R53 岸线岩石轮廓修复

R53 在 `game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs` 中将 `MakeRockMesh` 从六顶点三角锥替换为 8 段、三环不规则轮廓，带顶部和封闭底部。改动仅限 R03 表现生成器，不修改玩法、交互锚点、碰撞、业务 owner、存档或天气逻辑。

已验证：

- 相关回归 `50/50 PASS`。
- Unity R03 表现重建与审计完成，无新增 `error CS`；审计为 `307 Renderer / 24884 mesh vertices / 1 Collider`。
- 真实独立 Windows x64 Player 复验完成，`14/14` 固定镜头、`3/3` Calm/Rain/Storm 天气镜头、真实 `1920×1080` backbuffer PNG 全部落盘。
- 岩石轮廓比旧三角锥更连续，但建筑/屋顶仍是低模/程序化盒体，地表与远景偏平，环境道具和 HUD 分层仍有视觉债务。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R53.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R53.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R53.log`
- `game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R53.json`
- `game/tools/results/D31/D31-P04/D31_P04_R53_REVALIDATION_RESULT.json`
- `game/tools/results/D31/D31-P04/visual_evidence_R53/`

状态边界：R53 为 `PASS_WITHIN_SCOPE`，不是最终视觉门禁通过。D31-P04、D31-S05 和 D31-G06 继续 `PARTIAL`；物理输入负向/禁用/重复矩阵、D30 实机异常恢复、最终画质和性能 1% low 仍未闭环。

## 2026-09-03 R54—R57 港口视觉局部修复与复验

本轮以额度友好的短回归方式继续收敛 R03 港口表现：R54 调校现有天空盒的地平线/天空色调/曝光；R55 将建筑主屋顶改为山墙轮廓并增加地基、立面木构压条和侧窗板；R56 发现 TextMesh 180° 旋转造成出生镜头招牌镜像，R57 撤回错误旋转并加入 `D19R03LandmarkLabelVisibility`，使招牌只在所属 -Z 立面朝向摄像机时显示，背面隐藏以避免透明文字穿墙。

未修改玩法、交互锚点、碰撞 owner、存档、天气事实、业务 owner 或网络代码。

证据：

- [R57 视觉/交互静态回归](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R57.log)
- [R57 Unity 表现重建](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R57.log)
- [R57 Unity Editor 审计](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R57.log)
- [R57 Player 构建](../../../../game/tools/results/D31/D31-P04/D31_P04_BUILD_PLAYER_R57.log)
- [R57 Player 运行日志](../../../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_PLAYER_R57.log)
- [R57 Player 视觉证据](../../../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R57.json)
- [R57 复验结论](../../../../game/tools/results/D31/D31-P04/D31_P04_R57_REVALIDATION_RESULT.json)
- [R57 截图目录](../../../../game/tools/results/D31/D31-P04/visual_evidence_R57/)
- [D31-S05 结果](../../../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json)

验证结果：D25—D31 选定视觉/交互静态回归 `55/55 PASS`；Unity 重建与审计退出码为 0，无新增 `error CS`；Editor 审计为 `367 Renderer / 32828 mesh vertices / 1 Collider`，Prefab 空序列化字段为 `0`；真实独立 Windows x64 Player 在 `1920×1080` backbuffer 下采集 `14/14` 固定镜头和 `3/3` Calm/Rain/Storm 天气镜头，全部存在。R57 截图中出生镜头招牌可读，市场背面镜头不再显示镜像穿墙文字。

状态边界：R54—R57 均为 `PASS_WITHIN_SCOPE`。`D25-S06`、`D31-P04`、`D31-S05`、`D31-G06` 仍为 `PARTIAL`；整体场景仍是低模/程序化风格，地形层次、环境道具和 HUD 尚未达到最终美术门槛，物理 OS 输入负向/禁用/重复矩阵、D30 实机异常恢复和性能 1% low 也未关闭。本轮没有重复长路线或性能采样。

## 2026-09-03 R58 D29 仓库 owner readiness 边界修复

代码审计发现 D29 仓库按钮没有像其他主要入口一样绑定 D3 owner readiness，D3 尚未就绪时鼠标/键盘 `I` 可能打开空仓库模态。R58 已在 `D29InteractionHud` 中保存仓库按钮引用，让 `Button.interactable` 与 D3 readiness 同步，并在 `ToggleInfo` 的共用入口增加 owner 前置判断；关闭已打开仓库的路径保留。

证据：

- [D29InteractionHud.cs](../../../../game/Assets/Scripts/Gameplay/D29/D29InteractionHud.cs)
- [D29 HUD 测试](../../../../tools/tests/test_d29_interaction_hud.py)
- [R58 交互逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_REGRESSION_R58.log)
- [R58 Unity 编译](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R58.log)
- [D29-S05 结果](../../../../game/tools/results/D29/D29-S05/D29_S05_RESULT.json)
- [D29-G06 结果](../../../../game/tools/results/D29/D29-G06/D29_G06_RESULT.json)

验证结果：D29 定向回归 `30/30 PASS`；D25—D31 选定回归 `56/56 PASS`；D31 静态交互审计 `PASS / STATIC_LOGIC_ONLY`；Unity Tundra 短编译成功、`error CS=0`、退出码 0。未修改领域事实、存档、网络、交互锚点或场景资产。

状态边界：R58 为 `PASS_WITHIN_SCOPE`，不升级 `D29-S05`、`D29-G06`、`D31-S05` 或 `D31-G06`。真实 Player 物理输入矩阵、D30 实机异常恢复、最终画质和性能 1% low 仍未完成。

## 2026-09-03 R59 D30 未知事件防重分类修复

代码复核发现 `D30VisualStagePolicy` 对未知事件也执行短时重复判断，可能把第二次未知事件错误显示成“重复事件已忽略”。R59 新增已知成功/失败事件分类，只有路线白名单事件进入防重窗口；未知和空事件始终保持当前阶段并返回 `Unknown`。改动只在 D30 纯逻辑策略和 D31 EditMode 测试内，不修改领域 owner、存档、场景、交互锚点或网络代码。

证据：

- [R59 交互逻辑审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R59.json)
- [R59 逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_REGRESSION_R59.log)
- [R59 Unity 编译/测试摘要](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R59.log)
- [R59 Unity EditMode XML](../../../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R59.xml)

验证结果：Python 定向回归 `38/38 PASS`；D25—D31 选定回归 `56/56 PASS`；D31 静态审计 `PASS / STATIC_LOGIC_ONLY`；Unity EditMode `24/24 PASS`；Tundra 编译成功、`error CS=0`、退出码 0；`git diff --check` 通过。

状态边界：R59 只关闭未知事件的错误重复反馈，不升级 `D29-G06`、`D30-G06`、`D31-S05` 或 `D31-G06`。真实 Player 物理输入矩阵、D30 实机异常恢复、最终画质和性能 1% low 仍后置。

## 2026-09-03 R60 D28 天气 owner 启动就绪边界修复

复核发现 D29 可能在 D28 `Start` 之前找到天气组件并报告切换成功，随后 D28 初始化又将表现预览重置为 Calm。R60 为 D28 增加 `IsReady`，仅在降雨层创建、现有水面接线尝试和 Calm 初始应用完成后置为 true；`TryCycleWeather` 与 D29 天气按钮/可用性状态统一遵守该边界。改动不触碰天气事实、航行风险、存档或业务 owner。

证据：

- [R60 交互逻辑审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R60.json)
- [R60 逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_REGRESSION_R60.log)
- [R60 Unity 编译/测试摘要](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R60.log)
- [R60 Unity EditMode XML](../../../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R60.xml)

验证结果：D29/D28 定向回归 `40/40 PASS`；D25—D31 选定回归 `58/58 PASS`；D31 静态审计 `PASS / STATIC_LOGIC_ONLY`；Unity EditMode `24/24 PASS`；Tundra 编译成功、`error CS=0`、退出码 0；`git diff --check` 通过。

状态边界：R60 只关闭天气 owner 启动竞态，不升级 `D28-S05`、`D28-G06`、`D29-G06`、`D31-S05` 或 `D31-G06`。真实 Player 物理输入、D30 实机异常恢复、最终画质和性能 1% low 仍未完成。

## 2026-09-03 R61 D35 材质审计基线与 R52 灰泥例外对齐

全量 Python 回归发现 D35 材质审计仍按 R35 旧基线要求 12/12 材质有纹理、315 Renderer；这与 R52 已确认的 `R03_Plaster` authored-color 例外以及 R55—R57 当前 Prefab 基线冲突。R61 更新审计契约：11 个材质要求本地批准底图和纹理中性 BaseColor，`R03_Plaster` 是唯一允许无纹理且必须保留明确灰泥色的材质，当前 Editor 基线为 367 Renderer / 1 Collider / 32828 mesh vertices。审计输出增加 `color_only_material_count`，不把合法例外当成缺失资源。

证据：

- [R61 材质审计](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R61.json)
- [R61 材质回归](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_REGRESSION_R61.log)
- [D35 审计工具](../../../../tools/d35_r03_surface_material_audit.py)
- [D35 审计测试](../../../../tools/tests/test_d35_r03_surface_material_audit.py)

验证结果：D35/D19 R03 定向测试 `11/11 PASS`；材质审计 `PASS / EDITOR_STATIC`；11/11 纹理材质、1/1 authored-color 灰泥例外、6/6 线性法线元数据和 367 Renderer / 1 Collider / 32828 mesh vertices 均通过；全量 Python 回归 `1035/1035 PASS`。

状态边界：R61 只修复过期静态审计规则，不升级 `D31-P04`、`D31-S05` 或 `D31-G06`。真实 Player 物理输入、D30 实机异常恢复、最终画质和性能 1% low 仍未完成。

## 2026-09-03 R62 R03 有机岛面轮廓与环岛水面

复核发现 R03 岛面仍由方形 26×26 网格直接铺设，`Water` 只覆盖南侧窄条，造成岛屿方块化、海面背景板化。R62 在 `D19R03HarborPresentationBuilder` 中保留原有网格密度、唯一 Renderer/Collider 结构和玩法坐标，使用确定性的圆盘到椭圆映射与轻微 3/5 次谐波边界扰动生成有机轮廓；同一岛面 Mesh 增加浅岸线裙边；现有 `Water` 网格扩展为 52×52 环岛水面。没有修改 D2/D6 行走地面、航行、交互锚点、存档、天气事实或业务 owner。

证据：

- [R62 Unity 重建](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R62.log)
- [R62 视觉回归](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R62.log)
- [R03 Editor 审计](../../../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- [D19 R03 生成器测试](../../../../tools/tests/test_d19_r03_visual_builder.py)

验证结果：定向 Python 测试 `16/16 PASS`；Unity Editor 构建退出码为 `0`；R03 审计为 `367 Renderer / 1 Collider / 33084 mesh vertices`；D35 材质审计继续 `PASS / EDITOR_STATIC`。本轮没有重复 Player 长路线、物理输入矩阵或性能采样。

状态边界：R62 只关闭方形岛面和单侧水面两个表现层缺陷，不升级 `D31-P04`、`D31-S05` 或 `D31-G06`。真实 Player 画质、物理输入、D30 实机异常恢复和性能 1% low 仍未完成。

## 2026-09-03 R62 最终低成本验收补记

R61/R62 后的低成本复核已完成：D35 材质审计 `PASS / EDITOR_STATIC`；D31 交互逻辑审计 `PASS / STATIC_LOGIC_ONLY`；D19 R03 生成器、D35 材质和 D31 交互定向测试 `16/16 PASS`；3 份当前结果/审计 JSON `3/3 PASS`；`git diff --check` 无尾随空白错误。

本补记只确认代码层、资源准入、Prefab 结构、Editor 构建和静态交互回归仍然一致，不声明新的 Player 运行、物理 OS 输入、最终画质、D30 实机异常恢复或性能 1% low 通过。D31 正式状态继续为 `PARTIAL`，后续按用户额度安排统一运行时验收。

证据：

- [D31-S05 结果](../../../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json)
- [D31-G06 结果](../../../../game/tools/results/D31/D31-G06/D31_G06_RESULT.json)
- [R62 视觉逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R62.log)

## 2026-09-03 R63 D29/D30 防重策略边界硬化

R63 修复两个纯 C# 表现策略的边界：D29/D30 现在同时规范化当前 ID 和上一条 ID；NaN、Infinity、非正 guard 和时钟回拨不会形成错误的重复阻塞。新增测试覆盖大小写/空格上一条 ID和时钟回拨场景。

验证结果：Python D29/D31 测试 `27/27 PASS`；Unity EditMode `28/28 PASS`；Unity 编译 `error CS=0`；D31 静态交互审计 `PASS / STATIC_LOGIC_ONLY`。

证据：

- [R63 交互审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R63.json)
- [R63 逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_REGRESSION_R63.log)
- [R63 Unity 编译/测试](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R63B.log)
- [R63 EditMode XML](../../../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R63B.xml)

状态边界：R63 只属于 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`，不关闭真实 Player 物理输入、D30 实机异常恢复、最终画质或性能 1% low；D31 正式门禁继续为 `PARTIAL`。

## 2026-09-03 R64 R03 水面与岸线泡沫表现补强

- 修改 `game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs`：水面改为 `56×56`、`16×16` 确定性波面网格；沿 `IslandPoint` 补齐 28 段环岛泡沫；`R03_Teal` 使用不透明轻微高光 URP Lit 参数。
- 不新增水面或泡沫 Collider；R03 Editor 最新审计为 `382 Renderer / 1 Collider / 36564 mesh vertices`。
- 定向 Python 测试 `21/21 PASS`；Unity R03 重建/审计退出码 0，日志无 `error CS`，Prefab 已重新生成。
- 证据：[R64 水体审计](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_WATER_SURFACE_AUDIT_R64.json)、[重建日志](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R64.log)、[Editor 审计日志](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R64.log)、[R03 Editor 结果](../../../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)。
- D35 材质审计基线已同步到 R64，结果为 `PASS`；11 个纹理材质、1 个 authored-color 灰泥例外、6/6 线性法线元数据通过，证据为 [R64 材质审计](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R64.json)。

状态边界：R64 仅为 `PASS_WITHIN_SCOPE / EDITOR_STATIC`。没有运行长时间 Player、物理 OS 输入矩阵或性能采样；D31 正式门禁仍为 `PARTIAL`。

## 2026-09-03 R65 Kenney 海盗船候选接入

R65 将 D18-A05 已登记且 `ImportVerified` 的 `ship-pirate-medium.fbx` 接入 R03 船体表现 Prefab。候选模型按渲染包围盒归一到现有船体表现尺度，源材质优先保留，缺失材质才使用 R03 船材质回退；候选不可加载时仍使用原生成式船体。稳定 `ph_ship_fishing` 节点和 `D6_ShipHullCollision` 保留，船体碰撞、航行、交互、存档和业务 owner 没有改变。

证据：

- [R65 船候选审计](../../../../game/tools/results/D31/D31-S05/D31_S05_SHIP_CANDIDATE_AUDIT_R65.json)
- [R65 视觉逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R65.log)
- [R65 Unity 重建](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R65.log)
- [R65 Unity 场景审计](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R65B.log)
- [最新 R03 Editor 审计](../../../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)

验证结果：源候选 `6 Renderer / 6 MeshFilter / 6 MaterialSlot / 0 MissingMaterial / 0 AnimationClip`；生成船 Prefab `6 Renderer / 1 Collider / 2204 mesh vertices`，候选嵌套实例存在；R03 重建/场景审计退出码 0，`error CS=0`；D19 R03 生成器定向测试 `11/11 PASS`。

状态边界：R65 是 `PASS_WITHIN_SCOPE / EDITOR_STATIC`。候选仍为 `Candidate / ImportVerified`，没有修改 `LocalDevelopmentReady` 或 `ReleaseApproved`。D31 正式门禁仍保持 `PARTIAL`，因为 Player 最终画质、物理 OS 输入、D30 实机异常恢复和性能 1% low 尚未重新验收。

## 2026-09-03 R66 Kenney 港口建筑与道具候选补强

### 修改

- 在 `D19R03HarborPresentationBuilder` 的表现 Prefab 中新增 `D19_R03_KenneyPiratePortDetails` 组；接入 `tower-complete-small.fbx`、`structure-platform-dock-small.fbx`、`boat-row-small.fbx`、`cannon.fbx`、`crate-bottles.fbx` 和 `flag-pirate.fbx`。
- 这些资源均来自 D18-A05 manifest，保留源 `colormap` 材质；仅在材质槽缺失时使用 R03 回退材质。
- 候选实例删除任何导入 Collider，R03 仍保持单一 `D19_TerrainRoot` 路线 Collider；没有改变 D2/D6、交互、天气、存档或业务 owner。
- 新增候选源审计菜单方法，记录加载、Renderer、MeshFilter、材质、动画和包围盒证据。

### 验证

- 候选审计：`6/6 loaded`，合计 `9 Renderer / 9 MeshFilter / 9 MaterialSlot / 0 MissingMaterial / 0 AnimationClip / 2554 mesh vertices`。
- Unity R03 重建与场景审计：退出码 `0`，`error CS=0`，最新 Editor 基线 `391 Renderer / 1 Collider / 39118 mesh vertices`。
- Python 定向回归：`24/24 PASS`；D35 材质审计：`PASS / EDITOR_STATIC`；D31 交互逻辑审计：`PASS / STATIC_LOGIC_ONLY`。

证据：

- [R66 候选源审计](../../../../game/tools/results/D31/D31-S05/D31_S05_PIRATE_PORT_CANDIDATE_AUDIT_R66.json)
- [R66 候选审计日志](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_PIRATE_PORT_CANDIDATE_AUDIT_R66.log)
- [R66 Unity 重建](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R66.log)
- [R66 Unity 场景审计](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R66.log)
- [R66 材质审计](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R66.json)
- [R66 视觉/逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R66.log)
- [最新 R03 Editor 审计](../../../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)

### 状态边界

R66 是 `PASS_WITHIN_SCOPE / EDITOR_STATIC`，不是 D31 正式整体 PASS。资源仍为 `Candidate / ImportVerified`，`LocalDevelopmentReady` 与 `ReleaseApproved` 未改变。D31-S05/D31-G06 继续为 `PARTIAL`，因为 Player 最终画质、物理 OS 输入矩阵、D30 实机异常恢复和性能 1% low 仍未重验。

## 2026-09-03 R67 D31 交互布局纯逻辑收口

### 修改

- 新增 `game/Assets/Scripts/Gameplay/D29/Logic/D29InteractionLayoutPolicy.cs`，用零 Unity 依赖的整数矩形统一 1920×1080 HUD 参考布局；
- `D29InteractionHud` 的 Canvas 分辨率、快捷面板、仓库模态和按钮栈改为消费该策略常量；
- 新增 `docs/game/design/common/D31_HUD_LAYOUT_CONTRACT.json`、Python 审计和 D31 EditMode 布局测试；
- 仓库本地对象名 `WarehousePanel` 与协调器临时页别名 `D29WarehousePanel` 明确区分，避免命名冲突再次造成隐藏/显示误判；
- 未修改业务 owner、存档、输入事实、正式场景和 Player 证据。

### 验证

- 布局审计 `PASS`：1920×1080、5 个界面矩形、8 个快捷按钮、0 越界、0 重叠；
- Python 定向回归 `29/29 PASS`；
- Unity EditMode `Whalebone.Tests.D31` `31/31 PASS`；
- Unity 脚本编译 `Tundra build success`，`error CS=0`；
- `git diff --check` `PASS`，只有已有的换行格式提示。

证据：[HUD 布局契约](../../D31_HUD_LAYOUT_CONTRACT.json)、[R67 布局审计](../../../../game/tools/results/D31/D31-S05/D31_S05_HUD_LAYOUT_AUDIT_R67.json)、[R67 回归日志](../../../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R67.log)、[R67 EditMode](../../../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R67B.xml)、[D31-S05 结果](../../../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json)。

### 状态边界

R67 为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`。不宣称物理 OS 输入、完整 Player 负向/重复/禁用矩阵、D30 实机异常恢复、最终视觉质量或性能 1% low 通过；D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`。

## 2026-09-03 R68 D31 交互 owner 激活与船坞拒绝回退

### 修改

- `game/Assets/Scripts/Gameplay/D29/D29InteractionHud.cs` 的 `EnsureEventSystem` 现在显式激活 EventSystem、设置 `EventSystem.current`、打开导航事件、调用 `AssignDefaultActions()` 和 `UpdateModules()`，并关闭竞争输入模块；
- `UpgradeShip` 在 D4 owner 拒绝升级或未建立可见内容页时调用 `_d4.CloseContentPage()`，避免失败请求留下遮挡主 HUD 的空临时页；
- `tools/tests/test_d29_interaction_hud.py` 新增 EventSystem owner 和船坞拒绝回退断言。

### 验证

- Python 定向回归 `31/31 PASS`；
- `d31_hud_layout_audit.py` 与 `d31_interaction_logic_audit.py` 均为 `PASS / STATIC_LOGIC_ONLY`；
- Unity EditMode `Whalebone.Tests.D31` `31/31 PASS`，失败 `0`、跳过 `0`；
- Unity Tundra 编译成功，`error CS=0`；`git diff --check` 通过。

证据：

- [R68 交互审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R68.json)
- [R68 逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R68.log)
- [R68 Unity 编译](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R68.log)
- [R68 Unity 测试](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_TEST_R68B.log)
- [R68 EditMode XML](../../../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R68B.xml)

状态边界：R68 是 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`，不升级 D29-G06、D30-G06、D31-G06。物理 OS 输入、完整 Player 负向矩阵、D30 实机异常恢复、最终画质和性能 1% low 仍为后置门禁；没有重复长时间 Player/性能运行。

## R69 owner 关闭异常兜底（2026-09-03）

### 修改

- `D29InteractionHud.ReportActionException` 在 owner 关闭回调再次抛异常时，继续调用 `RuntimeHudCoordinator.RestorePrimaryHud()`；
- 兜底恢复再次失败时记录 `recovery fallback failed`，保持 UI 事件链不崩溃；
- `tools/tests/test_d29_interaction_hud.py` 新增 owner close failure 的源码级回归断言；
- 未修改 D4/D9/D10 业务 owner、领域数据、存档、正式场景、输入事实或网络代码。

### 验证

- D29 Python 定向回归：`26/26 PASS`；
- D31 交互静态审计：`PASS / STATIC_LOGIC_ONLY`；
- D31 HUD 布局静态审计：`PASS / STATIC_LOGIC_ONLY`；
- Unity EditMode `Whalebone.Tests.D31`：`31/31 PASS`；
- Unity Tundra：`build success (0.20 seconds)`，`error CS=0`；
- `git diff --check`：`PASS`。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R69.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R69.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R69.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_TEST_R69.log`
- `game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R69.xml`

### 结论

R69 仅为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`。它增强了异常恢复代码路径，但不证明物理 OS 输入、完整 Player 负向矩阵、D30 实机异常恢复、最终画质或性能。因此 D31-S05/D31-G06 仍保持 `PARTIAL`。

## R70 天气与画质异常失败投影（2026-09-03）

### 修改

- `D29InteractionHud.CycleWeather`、`CycleQuality` 增加统一异常出口；
- 天气/画质 owner 异常分别投影为 `weather_failed` / `quality_failed`，显示失败反馈且不改变业务事实；
- `D30VisualStagePolicy` 将两类事件纳入失败白名单，保持当前视觉阶段；
- 新增 D29 Python 与 D31 Unity EditMode 回归；未修改天气事实、存档或业务 owner。

### 验证

- D29 Python 定向回归：`27/27 PASS`；
- 交互相关 Python 综合回归：`42/42 PASS`；
- D31 交互静态审计与 HUD 布局审计：`PASS / STATIC_LOGIC_ONLY`；
- Unity EditMode `Whalebone.Tests.D31`：`33/33 PASS`；
- Unity Tundra：`build success (2.54 seconds)`，`error CS=0`；
- `git diff --check`：`PASS`。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R70.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R70.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R70.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_TEST_R70.log`
- `game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R70.xml`

### 结论

R70 为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`。天气/画质异常已进入代码级失败保持契约，但物理 OS 输入、完整 Player 矩阵、D30 实机异常、最终画质和性能门禁仍保持 `PARTIAL`。

## R71 D29 画质快捷入口矩阵补全（2026-09-03）

### 修改

- 将 D29 已存在的 `Q -> CycleQuality` 入口登记到 `D29_D30_INTERACTION_BUTTON_MATRIX.json` 的 `keyboard_only_controls`；
- 主 HUD 副标题明确显示 `B/R/C/I/Q`，让玩家知道画质预览快捷键；
- D31 静态交互审计、矩阵审计和单元测试显式覆盖 D29 `Q/CycleQuality`；
- 未新增可见按钮，未修改画质 owner、业务事实、存档、正式场景或输入裁决。

### 验证

- Python 定向回归：`60/60 PASS`；
- D29 矩阵审计：`29` 个可见入口、`5` 个键盘入口，`PASS`；
- D31 交互静态审计：`PASS / STATIC_LOGIC_ONLY`；
- HUD 布局审计：`5 surfaces / 8 visible buttons / 0 overlap / 0 out of bounds`；
- Unity EditMode `Whalebone.Tests.D31`：`33/33 PASS`，失败 `0`、跳过 `0`；
- Unity Tundra 编译成功，`error CS=0`，退出码 `0`；
- `git diff --check`：`PASS`。

证据：

- `game/tools/results/D31/D31-S05/D31_S05_MATRIX_AUDIT_R71.json`
- `game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R71.json`
- `game/tools/results/D31/D31-S05/D31_S05_HUD_LAYOUT_AUDIT_R71.json`
- `game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R71.log`
- `game/tools/results/D31/D31-S05/D31_S05_UNITY_TEST_R71B.log`
- `game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R71.xml`

### 结论

R71 是 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`。D31 交互代码和文档覆盖已补齐，但不升级 D29-G06、D30-G06 或 D31-G06；物理 OS 输入、完整 Player 负向矩阵、D30 实机异常恢复、最终画质和性能 1% low 仍属于后置门禁。

## R74 仓库模态关闭快捷键边界（2026-09-04）

### 修改

继续 D31 纯逻辑收口时发现：仓库打开后 `RuntimeHudCoordinator.PrimaryInputAllowed` 必须为 `false`，否则主 HUD 快捷键会穿透；但原来的 D29 `I` 关闭入口位于该门禁内部，导致 `I` 也被一并拦截。R74 将仓库自身的 `I/Escape` 关闭处理前移到主 HUD 门禁之前，并在关闭后结束本帧，其他 D29 快捷键仍受主 HUD 门禁保护。

### 验证

- D31 交互静态审计：`PASS / STATIC_LOGIC_ONLY`，29 个可见入口、5 个键盘入口、仓库关闭优先级 `true`；
- Python 定向回归：`39/39 PASS`；
- Unity Tundra 编译：退出码 `0`，`Tundra build success`，`error CS=0`；
- `runtime_proof=false`；没有把静态结果写成 Player 或性能通过。

证据：

- [R74 交互静态审计](../../../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R74.json)
- [R74 逻辑回归](../../../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R74.log)
- [R74 Unity 编译](../../../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R74B.log)
- [D31-S05 结果](../../../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json)

### 状态边界

R74 为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`，只关闭仓库模态输入分发的代码层缺口。D29-G06、D30-G06、D31-G06 仍保持 `PARTIAL`；物理 OS 输入、完整 Player 负向/禁用/焦点/重复点击、D30 实机异常恢复、最终画质和性能 1% low 仍需后置真实证据。

## R76 当前 R03 编辑器材质/Prefab 基线同步（2026-09-09）

Q21-A04 重建 R03 Prefab 后重新读取了 D19-owned 静态审计。当前 builder 明确保持 `RenderAuthoredPalms=false`，因此未经 pivot/ground-contact gate 的 6 组棕榈（12 个 Renderer）不进入可见 Prefab；旧 R75 的 `391` Renderer 基线已过时，当前真实 D19-owned count 为 `300`，D27 嵌套层另有 `79` 个 Renderer。没有为满足旧数字重新启用可能悬浮的树资源。

- R76 材料/Prefab 审计：`game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R76.json`，`12` materials、`11` textured、`1` color-only、NormalMap `6/6`、`300 Renderer / 1 Collider / 37740 mesh vertices`，`PASS_WITHIN_SCOPE / EDITOR_STATIC`。
- 相关静态回归：`95/95 PASS`；其中 D29 旧快捷键字面断言已同步为当前 `WasD29Input` 门禁契约，未改变 D29 生产逻辑。

边界不变：R76 不证明树资源完成、不证明 Player 最终画质或性能，也不升级 D31-S05 的正式 `PARTIAL`、D31-P04/S05/G06、VC-G01/VC-G02/VC-G03 状态。

## R75 当前源码视觉/HUD/交互回归审计（2026-09-14）

本轮使用连接的 Unity Pipeline 完成 D31-S05 当前源码独立回归，没有修改生产代码、测试代码、场景、Prefab 或 Runtime Pipeline 配置。Python 先尝试 `python -m pytest`，因环境缺少 `pytest` 模块而停止该命令；随后使用项目现有 unittest 入口完成同一组静态回归，结果 `60/60 PASS`。

Unity Pipeline 结果：`D31VisualStagePolicyTests=30/30`、`D31HudLayoutPolicyTests=3/3`、`D31Q18HudResolutionTests=3/3`、`D29TradeBridgePlayModeTests=1/1`、`D29ShipyardPreparationPlayModeTests=1/1`，均无失败/跳过。Editor 最终为 `ready/stopped`，Runtime Pipeline 回读 `enableInBuilds=false / port=0`，没有遗留 Player 进程或 7905 监听。

本轮记录了两个工具层命令误用：历史上的 `status` 与本轮的 `get_runtime_settings` 均不是 Pipeline 支持命令，正确读取命令为 `get_runtime_pipeline_settings`。它们不是产品运行时异常或测试失败，但由于历史 Console 保留，R75 不声称 Editor Error Console 为零。D31-S05 与 D31-G06 继续 `PARTIAL`；本轮不关闭前台物理键盘/compositor、完整 Player 成功/拒绝/禁用/焦点/重复矩阵、D30 Player 异常恢复、正式 Avatar/动作/自然泳姿/IK、最终低模质量或目标设备性能门禁。

证据：

- [R75 result](../../../../../../game/tools/results/D31/D31-S05/D31_S05_CURRENT_PIPELINE_REAUDIT_20260914_R75_RESULT.json)
- [R75 log](../../../../../../game/tools/results/D31/D31-S05/D31_S05_CURRENT_PIPELINE_REAUDIT_20260914_R75.log)
- [D31-S05 result index](../../../../../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json)
- [D31-G06 result index](../../../../../../game/tools/results/D31/D31-G06/D31_G06_RESULT.json)
