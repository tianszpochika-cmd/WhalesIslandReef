# D22 Runtime Deferred Register

> 归属：`D22-L34`。本登记只冻结纯逻辑交接后的 Runtime/Release 未验证项，不把它们写成逻辑失败或运行时通过。
>
> 登记日期：2026-09-01（Asia/Shanghai）。机器门禁：`game/tools/results/D22/D22-L33/D22_L33_RUNNER_REPORT.json`。

## 1. 边界结论

D22 的逻辑层只提供稳定的 screen/panel/modal/focus 状态、命令信封、owner 结果映射、反馈/localization key、通知/暂停读模型和防御性 ViewModel 投影。Runtime 只能把真实输入归一化为逻辑意图，再消费 D22 的结果；Canvas、UGUI、EventSystem、设备输入、场景和 Player 不属于本阶段证据。

下列项目保持 `Runtime Deferred`。除非有单独的 Runtime/设备证据，它们不能改变 D22 的 `LOGIC_*` 结论，也不能被“纯逻辑命令成功”替代。

## 2. Deferred 清单

| ID | Deferred 项 | 需要的后续证据 | 当前状态 |
|---|---|---|---|
| `D22-RT-01` | Canvas 层级、UGUI/UIToolkit 实例、raycast 顺序、旧 HUD 与重复 EventSystem 隔离 | 真实场景/Prefab 与 PlayMode 检查，确认单一可见层和输入路由 | `Runtime Deferred` |
| `D22-RT-02` | 键盘、鼠标、手柄、触摸、pointer/cursor lock 与输入法行为 | 目标设备上的真实输入矩阵；不能用逻辑 intent 单测替代 | `Runtime Deferred` |
| `D22-RT-03` | UGUI selectable、EventSystem 选择修复、modal 抢焦、返回焦点和焦点丢失 | PlayMode + 键鼠/手柄设备实测，记录抢焦和恢复顺序 | `Runtime Deferred` |
| `D22-RT-04` | 真实按钮点击、禁用态、loading/pending、owner error、retry/cancel 与通知确认交互 | 真实按钮和 owner 接线的成功/失败/重复/离线场景 | `Runtime Deferred` |
| `D22-RT-05` | Canvas 尺寸、分辨率缩放、响应式布局、文本裁切、字体、对比度和本地化渲染 | 目标分辨率与中英文视觉检查，包含长文案和缺 key 处理 | `Runtime Deferred` |
| `D22-RT-06` | 动画、减少动态效果、字幕/通知时间、音频和可访问性视觉表现 | Player/设备实测，核对 D16 presentation preference 的实际应用 | `Runtime Deferred` |
| `D22-RT-07` | 场景、Prefab、Transform、真实 SaveSystem 介质和各 owner 的 Runtime 接线 | 不启动 Player 的逻辑 runner 不覆盖的真实生命周期与恢复证据 | `Runtime Deferred` |
| `D22-RT-08` | 性能、长时间运行、平台输入差异、安装包与发行配置 | 目标设备性能 envelope、构建包和发布验收记录 | `Runtime Deferred` |

## 3. D23 接线约束

- D23 只能调用 `D22UiCommandRequest`、`D22UiCommandResult/D22UiCommandRouteResult`、`D22FeedbackRecord` 和 `D22ViewModel` 等公开逻辑面。
- `Succeeded` 之外的 `Blocked`、`Rejected`、`Pending`、`Duplicate` 和 `Cancelled` 必须保持原语义；视觉上的按钮状态不能反向写入业务 owner。
- `D22ViewModel` 的缺失、版本不匹配、adapter 失败和降级 reason 必须可见地保留到 Runtime view adapter；不得降级为空数据后伪装成完整业务状态。
- 真实输入适配器负责设备读取和 cursor/EventSystem 处理，D22 Systems 不得加入这些依赖；领域事实仍由 D3/D12/D14/D19/D20/D21/Save 等 owner 写入。

## 4. 证据解释

`D22_L33_RUNNER_REPORT.json` 只证明本地离线纯逻辑命令、读模型、测试和静态边界；它不证明 Canvas 可点击、焦点在设备上正确、画面可读、场景可玩、性能达标或可发行。Runtime 与 Release 状态在取得对应外部证据前保持未放行。
