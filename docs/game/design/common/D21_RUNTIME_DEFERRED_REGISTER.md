# D21 Runtime Deferred 登记表

> 归属：`D21-L31`。本表只登记 D21 纯逻辑完成后必须在 Unity/Player/目标设备阶段验证的内容，不把延期事项写成逻辑通过。
>
> 当前结论：`D21-L31 = LOGIC_PASS`（延期清单文档完成）；D21-L30/L32 的纯逻辑章节门禁已在完整证据核验后通过，Runtime Deferred 仍保持待测。

## 1. 逻辑结果与运行时边界

D21 的逻辑层已经提供确定性船舶、天气、海况、航线、补给、事件和保存投影接口。以下事项不能由离线 C# 编译、静态扫描或确定性模拟替代：

| ID | Runtime Deferred 事项 | 当前逻辑接口/替代 | 后续验收方式 | 当前阶段不得宣称的内容 |
|---|---|---|---|---|
| RT-01 | 20 种船型的真实模型、材质、舱段和功能点 | `D21ShipIdentityContracts`、`D21HullComponentSystem` 的稳定 ID/组件状态 | Unity 资源准入、Prefab/模型映射、功能点逐项检查 | 逻辑船型 ID 不等于模型、碰撞体或船舱可用 |
| RT-02 | 船体、帆、桅、舵、锚和货舱的 3D 布局 | `D21HullComponentSystem` 的合法组合与状态 | 场景中 Prefab 层级、挂点、缩放和运行时读模型检查 | 组件 DTO 不证明视觉挂点或实际可操作 |
| RT-03 | 海面、水体 Shader、波浪、反射和风暴表现 | `D21SeaStateSystem`、`D21WeatherImpactRules` 的数值结果 | URP/水体材质、不同海况截图与 Player 回归 | 海况等级不等于波浪、浮力或水面画面完成 |
| RT-04 | 天气 VFX、音频、警报和航海 HUD | 天气/航程事件、状态和原因码 | 雨雾、闪电、风暴音效、HUD 文案与错误反馈实测 | 事件日志不证明 VFX、音效或 UI 已接线 |
| RT-05 | 舵、帆、锚、停船和改航的真实输入与相机反馈 | `D21CourseCommandSystem`、`D21AnchorMooringSystem` 的已校验命令 | 键鼠/手柄/触屏逐项输入、焦点、相机和重复输入测试 | 命令 API 不证明 Input System 或相机可用 |
| RT-06 | Rigidbody、浮力、碰撞、靠岸、锚链和真实漂移 | 确定性锚泊/船况状态机与安全阈值 | Unity 物理、靠泊、走锚、卡死、沉没和恢复测试 | 逻辑漂移风险不等于实际物理漂移 |
| RT-07 | 港口、泊位、航线坐标、登船触发器和加载定位 | D19 公开命令/结果、稳定 port/berth/route ID | D19 场景路线、泊位占用、触发器和返港定位回归 | D19 公开路由通过不证明真实场景生命周期 |
| RT-08 | 船员在船上的位置、岗位表现和动画反馈 | D20 public crew projection、D21 crew requirement decision | 船员登船、轮班、岗位动画、阻挡和异常反馈测试 | 只读 crew projection 不证明 D10/Crew 运行时写入 |
| RT-09 | 真实 Save 介质、崩溃恢复、迁移和云同步 | `D21ShipSaveProjection` 的 Validate/JSON round-trip | Unity SaveSystem、断电/重载、旧版本迁移和平台存档验证 | 内存 projection 不等于磁盘、云端或断电安全 |
| RT-10 | 帧率、GC、内存、异步加载和大世界规模 | 固定 Tick、事件去重和长历史离线证据 | 目标分辨率 Player、Profiler、长航程和加载压力测试 | 纯逻辑长跑耗时不构成产品性能承诺 |
| RT-11 | 真机、平台构建、无障碍输入和发行包 | 逻辑命令/结果与原因码 | Windows/Steam 目标机、输入设备、安装器和发布清单回归 | `LOGIC_PASS` 不等于 `RuntimeApproved` 或 `ReleaseApproved` |

## 2. 允许下游消费的逻辑边界

下游只能消费公开 DTO、只读投影和稳定结果，不读取 D21 owner 私有字段：

- 船身份/船体/船况：`D21ShipIdentityContracts`、`D21HullComponentSystem`、`D21ShipConditionSystem`；
- 货舱/船员/路线：`D21CargoCapacityRules`、`D21CrewRequirementRules`、`D21NavigationRouteContracts`；
- 天气/海况/时间/风险：`D21WeatherInputContracts`、`D21SeaStateSystem`、`D21WeatherImpactRules`、`D21VoyageTimeSystem`、`D21VoyageRiskSystem`；
- 航程汇合/事件/保存：`D21VoyageCommandService`、`D21VoyageResolutionSystem`、`D21VoyageEventRouter`、`D21ShipSaveProjection`；
- 港口：只能通过 D19 `D19PortCommandRouter.Route` 及其公开结果；D21 不写 D19 港口或泊位事实。

## 3. 登记结论

本表覆盖船体、海水、天气表现、输入、驾驶、碰撞、港口坐标、船员运行时、Save 介质、性能、设备和发布边界。它是后续 Runtime 验收清单，不是当前运行时通过证明。

当前明确保持 `RuntimeApproved=0`、`ReleaseApproved=0`；任何一项 Runtime Deferred 都不能由本表或离线逻辑证据自动提升。

`D21-L31 = LOGIC_PASS`（文档登记完成；Runtime Deferred 保持待测）。
