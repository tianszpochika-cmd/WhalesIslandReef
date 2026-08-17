# D19-L32 交接报告

## 任务信息与结论

- 任务编号：`D19-L32`（波次 19.6，领域回归测试）
- 精确前置：`D19-L05`、`D19-L13`、`D19-L17`、`D19-L18`、`D19-L19`、`D19-L21`（全部 `LOGIC_PASS`，已验证）
- 日期：2026-08-18
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L32/D19_L32_LOGIC_RESULT.json`

## 唯一写入范围

1. `game/Assets/Tests/EditMode/D19/D19AvailabilityAndNpcTests.cs`（新建，纯 C# EditMode NUnit 测试套件）
2. `game/tools/results/D19/D19-L32/D19_L32_LOGIC_RESULT.json`（新建）
3. `docs/game/design/common/handoffs/D19/D19-L32_LOGIC_HANDOFF.md`（本文件）
4. `.workbuddy/memory/2026-08-18.md`（追加当日记忆）
5. `docs/game/LOGIC_TASK_STATUS.md`（由 `python tools/sync_logic_task_status.py` 刷新同步）

未修改任何生产代码（`Systems/D19/` 等）、未修改 D3/D4/D9/D10/Save 生产文件、未引入任何场景、Prefab、行为树、动画或 NavMesh。

## 测试覆盖详情（23 项纯逻辑断言全部通过）

### 1. 基础确定性与幂等性
- `Adjudicate_SameInputs_ProducesIdenticalResultsRepeatedly`：相同输入循环执行 1,000 次，逐字段断言结果（Verdict、ReasonCode、LocKey、RetryPolicy、Detail、RetryHint、ServiceId、PortId）绝对一致；
- `Adjudicate_AllStandardServices_AvailableUnderNominalConditions`：验证全部 5 项已知标准服务（market, warehouse, tavern, shipyard, supply）在标称条件下均判定为 `Available`。

### 2. 设施状态（FacilityState）
- `FacilityState_Closed_MakesServiceUnavailable_FacilityClosed`：`Closed` 状态稳定映射至 `d19.service.unavailable`、`FacilityClosed` 细目与 `RetryLaterSameDay`；
- `FacilityState_UnderRepair_MakesServiceUnavailable_UnderRepair`：`UnderRepair` 稳定映射至 `UnderRepair` 细目与 `AfterRepair`；
- `FacilityState_Blockaded_MakesServiceUnavailable_Blockaded`：`Blockaded` 稳定映射至 `Blockaded` 细目与 `AfterBlockadeLifted`；
- `FacilityState_OutOfStock_OnlyValidForMarketAndSupply`：Market 与 Supply 缺货判定为 `Unavailable`；Tavern / Warehouse / Shipyard 声明缺货视为冲突输入保守 `Rejected (ConflictingInput)`。

### 3. 势力许可（FactionPermission）
- `FactionPermission_Denied_OverridesWeatherAndTime_DetailFactionDenied`：`FactionPermission.Denied` 稳定阻断全部服务，输出 `FactionDenied` 细目与 `ResolveFactionPermission`。

### 4. 天气逻辑标签（WeatherLabel）
- `WeatherLabel_Storm_ClosesMarketAndSupply_TavernWarehouseShipyardRemainOpen`：`Storm` 风暴关闭露天服务（Market, Supply），而室内/码头重型服务（Tavern, Warehouse, Shipyard）保持开放；
- `WeatherLabel_Rain_ClosesMarketOnly`：`Rain` 仅关闭 Market，Supply 与其余服务保持开放。

### 5. 时间段（ServiceTimePeriod）
- `ServiceTimePeriod_Night_ClosesMarketWarehouseShipyard_TavernAndSupplyBehavior`：`Night` 夜间关闭 Market, Warehouse, Shipyard；Tavern 完全夜间开放（符合酒馆核心设计），Supply 亦保持开放。

### 6. NPC 逻辑服务标识（staffed 布尔）
- `Staffed_False_BlocksAllOtherwiseAvailableServices_DetailNotStaffed`：NPC 严格作为逻辑服务标识 `staffed`（零 Unity GameObject / 行为树 / NavMesh 依赖），`staffed=false` 判定为 `Unavailable (NotStaffed)` 与 `WhenStaffed` 重试提示。

### 7. 冻结确定性优先级链
- `PriorityChain_FacilityClosed_Overrides_FactionWeatherTimeAndStaff`：设施关闭优先于势力、天气、时段和配人；
- `PriorityChain_FactionDenied_Overrides_WeatherTimeAndStaff`：势力拒绝优先于天气、时段和配人；
- `PriorityChain_WeatherClosed_Overrides_TimeAndStaff`：天气关闭优先于时段和配人；
- `PriorityChain_TimeClosed_Overrides_Staff`：时段关闭优先于配人。

### 8. 保守拒绝与边界
- `Adjudicate_UnknownPort_RejectedWithUnknownPortCode`：未知港口拒绝并返回 `d19.port.unknown_port`；
- `Adjudicate_NullOrEmptyOrUnknownService_RejectedWithUnknownInput`：空/未知服务拒绝并返回 `UnknownInput`；
- `Adjudicate_InvalidEnumValues_RejectedWithUnknownInput`：越界枚举值强制拒绝并返回 `UnknownInput`；
- `Adjudicate_UnregisteredServiceInPort_UnavailableNotRegistered`：港口未登记服务判定为 `Unavailable (NotRegistered)`；
- `Adjudicate_PortAliasResolution_NormalizesAndSucceeds`：裸键别名规范化解析（如 `santa_margarita` -> `port_santa_margarita`）。

### 9. 零副作用验证
- `UnavailableService_DoesNotMutateMoneyOrInventoryOrGenerateSideEffects`：服务不可用（如设施关闭）通过 `AvailabilityGate` 注入 `D19MarketPortAdapter.Buy`，断言交易阻断且金钱不扣减、市场库存不改变、货舱不改变、EventLog 零追加。

### 10. 读模型（D19PortReadModel）投影聚合与稳定排序
- `ReadModel_ServicesSummary_CoversAllServicesAndOrdersDeterministically`：`D19ServicesSummaryReadModel` 聚合全部服务，断言严格按 `ServiceId` 字符串升序稳定排序；
- `ReadModel_RootSnapshot_FingerprintIsDeterministic`：`D19PortReadModel` 快照指纹（SnapshotFingerprint）纯确定性，相同输入生成完全相同的指纹字符串。

## 验证证据

| 验证项 | 方式 | 结果 |
|---|---|---|
| 纯 C# 编译与单元测试 | `dotnet test`（NUnit 23 tests） | PASS（23/23 全部通过，0 error 0 warning） |
| 覆盖场景 | 确定性循环、5大标准服务、4种设施状态、势力拒绝、风暴/下雨限制、夜间限制、NPC 未配人阻断、优先级链、未知/冲突保守拒绝、零副作用、读模型稳定排序、快照指纹 | 23/23 全部通过 |
| 代码规范与依赖隔离 | 静态代码审计 | PASS（零 Unity/行为树/模型/NavMesh/场景引用） |
| 状态总览同步 | `python tools/sync_logic_task_status.py` | PASS（D19-L32 = LOGIC_PASS） |
| Git 格式检查 | `git diff --check` | PASS（exit 0） |

## 明确未宣称

- 未声称任何 NPC 3D 模型、Avatar 或动画已接入 Unity 场景；
- 未声称任何行为树、寻路网格或物理触发器已运行；
- 未授予任何 RuntimeApproved 或 ReleaseApproved 状态。
