# D24 后续 Runtime Validation Backlog

> 对应任务：`D24-L30`。本文是延期清单和停止规则，不是执行报告；创建本文不代表任何 Runtime 任务已开始或通过。

## 1. 入口条件

后续 Runtime 阶段只能在 D18—D24 P0 逻辑证据保持 `LOGIC_PASS`、D23 确定性完整循环和 D24 逻辑回归矩阵可重跑后另立计划。每次 Runtime 任务必须使用独立任务编号、独立结果 JSON 和独立 handoff；不能复活旧 RERUN 计数，也不能把旧截图/旧 Player 结果升级为新通过。

## 2. 测试清单

| 优先级 | 入口 | 检查内容 | 通过证据 | 停止/回滚条件 |
|---|---|---|---|---|
| P0 | Player 启动与 InitialHarbor | 正式场景加载、无 missing script/material、主相机/光照/关键交互锚点存在 | 固定构建、Player log、可复核录像/截图 | 启动崩溃、缺资源、场景状态漂移：停止，不进入业务路线 |
| P0 | 港口→船→出航 | 真实输入、交互目标、泊位/登船/出航按钮与 D19/D21 public result 对齐 | 固定设备路线 + visible state + owner event/save evidence | 任一点击无效、错误状态显示为成功、重复点击二次写入：停止 |
| P0 | 航行→返港→继续 | 真实 Tick 驱动、相机/物理反馈、抵达/返港、实际 Save/Load/Continue | Player route、Save artifact、重启后状态/事件/fingerprint 对比 | 丢档、重复事件、状态回滚或 runtime 绕过 owner：停止并回滚 |
| P0 | 资产与内容准入 | 通过 D18 资源状态、稳定键、许可证和实际导入结果逐项核对 | 导入日志、资源清单、授权证据、运行时引用扫描 | Candidate/Pending 被当作已批准或出现外部绝对路径：停止 |
| P1 | Canvas/UI/可访问性 | 布局、缩放、字体、对比度、字幕、焦点、modal、真实分辨率和输入设备 | 多分辨率截图/视频、键鼠手柄触摸矩阵、console clean | 遮挡、不可操作、焦点丢失、错误文案泄漏：停止该 UI 波次 |
| P1 | 画面/音频/物理 | 海水/天气/VFX、动画、音频路由、碰撞/浮力/船体表现 | 目标平台实测、可重复场景记录 | 物理穿透、音频爆音、视觉阻塞交互：停止，不改逻辑 owner |
| P1 | 设备/性能 | Windows/目标移动设备、分辨率、加载、内存、CPU/GPU、长航程稳定性 | 固定硬件矩阵、profiling trace、性能预算报告 | 超预算、发热/崩溃、长跑退化：停止发行评估 |
| P1 | 网络/Agent runtime | 仅在专门契约和权限任务中验证服务、超时、重试、AgentReply schema gate | 脱机/联网对照、HTTP 日志、schema validation、无 token 泄漏 | 未授权网络、私有协议、越权数值结果或凭据泄漏：立即停止 |
| P0/P1 | 发布/Steam | 构建产物、安装/卸载、存档升级、崩溃收集、商店包和平台合规 | 发布候选包与独立 Release gate | 任一逻辑/Runtime/资源/性能/合规门禁未闭合：不得发布 |

## 3. 观察要求

Runtime 阶段必须区分 visible UI state、HTTP/owner evidence、Save artifact 和截图/视频；单张截图、编译成功、服务健康或固定 token 都不构成完整业务事实。麦克风等系统权限不得绕过；若环境权限缺失，记录 blocker 并继续可执行的非麦克风检查。

## 4. 当前状态

截至 D24-L30 创建时，上表所有条目均为 `RUNTIME_DEFERRED`，没有执行命令、设备结果或批准状态。`RuntimeApproved=0`、`ReleaseApproved=0`；后续入口仅由 D24-L36 handoff 传递有限范围，不自动授权启动。
