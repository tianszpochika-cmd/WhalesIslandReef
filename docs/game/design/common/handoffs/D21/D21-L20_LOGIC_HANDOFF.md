# D21-L20 逻辑交接

## 已完成

- `D21ShipConditionTests.cs` 覆盖 canonical ship type、starter hull 组件组合和必需结构拒绝。
- 覆盖事实驱动结构/进水损伤、船厂维修、适航等级，以及损伤边界的 flooding clamp 与 durability 不得为负。
- 覆盖 D21 cargo capacity 的正常加载、超载出航拒绝、危险货物隔离和交给 D3 的只读 mutation request。
- 新增 `D21.Tests.asmdef`，仅声明 Editor 测试程序集和现有系统/NUnit 依赖。

## 验证证据

- `dotnet build .codex-temp/d21-logic-runner/D21LogicRunner.csproj --no-restore`：通过。
- 直接调用 NUnit 测试方法的离线 runner：14/14 通过，其中 L20 五项全部通过。
- 禁用依赖可执行行扫描：0 命中。
- 结果 JSON、asmdef 和 `.meta` 资产元数据解析通过；`git diff --check` 通过。

## 边界与未宣称

- 测试只调用 D21 公开 owner API，不写 D3 数量、不复制 D4/港口/船员事实，也不使用反射、系统时钟、随机、Unity 场景或物理。
- Unity Editor 的实际测试发现/窗口执行、运行时接线、场景/Prefab、物理、输入、UI、性能、真机和发行门禁均 Runtime Deferred。
- 按委派要求未运行 `tools/sync_logic_task_status.py`，不修改 `docs/game/LOGIC_TASK_STATUS.md` 或 `.workbuddy/memory/2026-09-01.md`。

## Coordinator memory entry

`2026-09-01：D21-L20 已增加船舶身份/组件、事实损伤与维修、适航边界、货舱容量和危险货物纯逻辑 EditMode 测试；离线 runner 14/14 通过，未修改共享状态表与当天记忆。`
