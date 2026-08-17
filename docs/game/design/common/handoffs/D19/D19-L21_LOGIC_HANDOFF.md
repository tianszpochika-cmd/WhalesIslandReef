# D19-L21 交接报告

## 任务信息与结论

- 任务编号：`D19-L21`（波次 19.5，港口旧档迁移）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L21/D19_L21_LOGIC_RESULT.json`

## 前置核验

精确前置检查通过：
- `D19-L20`：港口存档投影纯模型（`LOGIC_PASS`）
- 读取了 `D19-L20` 的公开投影 `D19PortSaveProjection` 结构与其 `CreateDefault` 安全默认值语义；
- 继承了既有 `SaveMigration` 规则与 `D19-L17` 失败码、`D19-L18` 本地化键、`D19-L04` 锚点目录别名规范化。

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19PortSaveMigration.cs`（新建，纯 C# 港口旧档安全迁移器）
- `game/tools/results/D19/D19-L21/D19_L21_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L21_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）
- 状态总览由 `python tools/sync_logic_task_status.py` 刷新（唯一共享写者）

未重定义或复制 L20 投影模型，未修改 Save 根模型（`GameState`/`PlayerState`），未改动 contracts/Unity 内容。

## 核心迁移与兼容实现（`D19PortSaveMigration`）

1. **合法缺失字段安全默认（保守确定性）**：
   - 投影完全缺失（`null` 输入）-> 安全调用 `D19PortSaveProjection.CreateDefault`，生成 `VisitState="None"`, `PortId=""`, `BerthId=""`, `WorldDay=0`, `SaveRevision=0` 的合规初始投影；
   - 缺省成员位置 -> 保守默认回退为 `OnShore`（岸上），**绝不自动置为 `OnBoard` 伪造在船**；
   - 缺省泊位分配/服务状态/未完成命令/补偿草案/幂等投影 -> 初始化为空列表与默认幂等容器（30 天保留期、上限 512）。

2. **锚点键/别名迁移（平滑升级）**：
   - 旧 D3 裸港口键（`santa_margarita` / `green_cay` / `driftwood_isle`）-> 经 `D19PortAnchorCatalog.Instance.ResolvePort` 规范化为 `port_santa_margarita` 等，并保留 `MigrationHint`；
   - 旧 D9 历史泊位别名（`building_saint_marys_dock`）-> 规范化为 `berth_santa_margarita_dock_01`；
   - 服务键别名（如 `service_santa_margarita_market`）-> 规范化为 `service_santa_margarita_market`。

3. **不可能状态与数据损失严格拒绝（返回 L17 错误码）**：
   - 未来/不支持版本（`SchemaVersion > 1` 或 `< 0`）-> 返回 `d19.save.unsupported_version` (`D19PortLocalizationKeys.MigrationSaveUnsupported`)，`UnsupportedVersion` 状态；
   - 负数世界日 / 负数修订号 -> 返回 `d19.save.conflict`；
   - 处于 `Berthed` 或 `InService` 却缺失 `BerthId` / `PortId` -> 判定为不可逆数据损失，返回 `d19.berth.occupied` (`FailureNotBerthed`)，**绝不伪造泊位**；
   - 处于非港口状态（`None` / `Departed`）却携带 `BerthId` -> 返回 `d19.lifecycle.invalid_transition`；
   - 跨港泊位冲突（泊位归属港口与当前停靠港不一致）-> 返回 `d19.port.permission_denied`；
   - 重复成员 ID / 占用的泊位缺少船只 ID / 预留泊位租期非法 -> 返回对应的不变式破坏错误码。

4. **幂等性与版本单调性**：
   - 版本由 0 升级至 1；已是 1 且合法的输入返回 `AlreadyUpToDate`；
   - 相同输入执行多次迁移，结果完全一致（`res(f(x)) == f(x)`）；
   - 原始对象保证不被污染（`OriginalPreserved = true`）。

5. **铁律与红线守则**：
   - 迁移过程不调用任何业务命令（不扣款、不占泊、不发事件）；
   - 不提升任何权限，不自动开启任何服务；
   - 失败原因精确可被 `D19-L39`（保存与迁移回归测试）定位。

## 验证证据

| 检查项 | 命令 / 工具 | 结果 |
|---|---|---|
| 纯 C# 编译与单元测试 | `dotnet run`（在 `temp_test_build` 运行覆盖 19 组完整剧本） | PASS（ALL 19 D19PortSaveMigration COMPREHENSIVE CHECKS PASSED） |
| 覆盖场景 | 缺失 null 默认值、已最新幂等性、v0->v1 升级、未来不支持版本拒绝、负数时钟拒绝、别名/废弃键规范化、泊位分配别名规范化、Berthed 缺泊位数据损失拒绝、非港口持泊位拒绝、跨港泊位冲突拒绝、成员位置默认 OnShore、重复成员拒绝、重复泊位拒绝、占用泊位缺船只拒绝、预留租期非法拒绝、服务别名迁移、迁移幂等性、原始数据保留保证 | 19/19 全部 PASS |
| 边界与静态检查 | 代码审计 | PASS（零 Unity/UI 依赖，零 static 状态） |
| 状态总览同步 | `python tools/sync_logic_task_status.py` | PASS（D19-L21 = LOGIC_PASS） |
| Git 格式检查 | `git diff --check` | PASS（exit 0） |

## 后续任务提示

- 波次 19.6（领域测试，`D19-L25`—`D19-L32`）的前置条件均已就绪（`D19-L21=LOGIC_PASS`）；
- `D19-L39`（保存与迁移测试）可直接对 `D19PortSaveMigration` 的迁移、边界与幂等性进行端到端断言。
