# D21-L08 逻辑交接

## 任务状态

- 状态：`LOGIC_PASS`。
- 范围：路线、航点、危险区、发现记录和路线使用评估；不包含路径搜索或场景导航。

## 已完成

- `D21NavigationRouteContracts` 冻结路线 ID、起终点逻辑引用、连续航点、距离、最小水深、危险区和发现状态。
- 校验路线/航点/危险区的稳定引用、序列连续性、距离单调性、危险区引用存在性和数值范围。
- 根据当前发现记录、船只吃水和危险容忍度输出路线允许、未发现危险区、受限航点和拒绝原因。
- 不保存坐标、Transform、路径网格或场景对象；发现记录仍是只读事实输入。

## Public API / owner 边界

- 主要入口：`ValidateRoute`、`EvaluateRoute`。
- 主要 DTO：`D21NavigationRouteDefinition`、`D21RouteWaypoint`、`D21HazardZoneDefinition`、`D21RouteDiscoveryRecord`、`D21NavigationRouteRequest`、`D21NavigationRouteDecision`。
- 路线定义/发现事实的持久化和上游适配不由本静态契约类写入。

## 验证

- `/warnaserror+` 独立 C# library 编译通过，XML docs 生成成功。
- 冒烟通过 4 项：路线校验、未发现边界、发现后允许、吃水超深拒绝。
- D21-L02—L08 最终合编通过；可执行行禁用依赖扫描命中 0。
- 结果文件：`game/tools/results/D21/D21-L08/D21_L08_LOGIC_RESULT.json`。

## 未完成 / Runtime Deferred

- 坐标/pathfinding/NavMesh、场景航点、Transform、相机、输入、真实驾驶、海图 UI、危险区视觉、天气/水面/VFX、音频、性能、真机和发行门禁均未执行。

## Blockers / 下一步

- 本项无纯逻辑 blocker。后续由路线 owner 提供定义和发现事实，由运行时适配层消费 `D21NavigationRouteDecision`。

## Coordinator memory entry

`2026-09-01：D21-L08 已冻结稳定 ID 路线/航点/危险区/发现契约，并完成吃水与危险容忍度评估；不引入坐标、路径搜索或场景导航 owner。`
