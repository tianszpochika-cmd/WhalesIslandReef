# D21-L01 逻辑基线交接

## 已完成

- 任务：`D21-L01`
- 状态：`LOGIC_PASS`，范围仅为基线冻结。
- 已冻结 D2、D4、D9、D19、D20 到 D21 的公开入口、唯一写者、禁止越界、命令流和依赖波次。
- 已覆盖船身份/船型、船体组件、船况、货物容量、船员需求、航线、天气输入、海况/天气影响、锚泊/系泊、港口出发/返港、补给、事件和存档责任。
- 已将船体模型、水面 Shader、VFX、输入、物理/碰撞、驾驶/Transform、场景/Prefab、性能、真机和发布门禁列入 Runtime Deferred。

## 真实上游证据

- D19-L48：已有 `LOGIC_PASS`；D21 港口出发/返港必须使用 `D19PortCommandRouter.Route(D19RoutedPortCommand)`、公开结果、事件和保存投影。
- D20-L01：已有 `LOGIC_PASS`；仅能作为边界和波次证据，不能替代 D20-L40 的公开投影验收。
- D2：当前 `SailingSystem`/`DayNightSystem` 是最小兼容入口；其帧式 `float` 推进、位置和速度不构成 D21 权威航线/物理事实。
- D3/Save：`D3InventorySystem` 继续唯一写入货物数量和容量使用量；`ShipState`、`VoyageState` 是需要适配的旧模型，Save 只负责序列化/恢复。
- D9：`EnvironmentSystem` 继续唯一写入天气/海况事实；D21 只能通过公开命令/事件输入并读取快照/派生结果。

## 进行中 / 未宣称

- D21-L02—L23 尚未由本任务实现；本次没有新增 D21 生产 C#、测试、场景或 Prefab。
- D21-L24—L29 尚未聚合，D21-L30—L32 尚未执行收敛门审查。
- D20-L40 仍是依赖 D21 后续非港口任务的开放门；D19-L48 已满足港口上游逻辑依赖。
- 旧 D19/D20 Player、场景、视觉、输入、物理、性能和真机材料不被提升为 D21 逻辑证据。

## 风险与下一步

1. L02 必须处理当前 `ShipState` 缺少 owner/flag/home 等目标身份字段，以及 `CreateNew(ownerId)` 尚未形成所有权事实的问题。
2. L03/L04 必须把船体组件和损伤写权放入 D21 owner；D4 战斗只能提供已裁决结果，不能直接改 D21 船况。
3. L05/L14 必须区分 D3 货物数量写入和 D21 航程补给消耗账本，禁止两个系统直接改同一兼容字典。
4. L09/L10 必须通过 D19 公共路由；当前 D19 路由成功不会自动执行外部船、货物、补给或船员适配器，需显式定义 owner effect。
5. 依赖 D20 角色/身份投影的任务等待 D20-L40 真实通过；不得提前绑定未验收的类型或方法名。

## 交付物

- `docs/game/design/common/D21_LOGIC_BOUNDARY_BASELINE.md`
- `game/tools/results/D21/D21-L01/D21_L01_LOGIC_RESULT.json`
- `docs/game/design/common/handoffs/D21/D21-L01_LOGIC_HANDOFF.md`

## 验证与范围保护

- 结果 JSON 需通过协议键、基线引用和 `git diff --check` 检查。
- 按本次委派要求，不运行 `tools/sync_logic_task_status.py`，不修改 `docs/game/LOGIC_TASK_STATUS.md` 或共享 `.workbuddy/memory/2026-09-01.md`。
- 不运行 Unity Player/PlayMode、截图、物理/碰撞、输入、Shader、场景、性能、真机或发布验证；这些属于 Runtime Deferred。

## Coordinator memory entry

`2026-09-01：D21-L01 已冻结 D2/D4/D9/D19/D20 导航边界和唯一写者；D19-L48/D20-L01 仅作上游逻辑证据，D20-L40 仍是后续依赖；旧船/航程/天气入口必须适配，Runtime Deferred 不计为逻辑失败。`
