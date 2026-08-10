# D11-A07 交接报告

任务：`D11-A07` 植被采集与水果资源  
日期：2026-08-11  
前置：`D9-S02` PASS；`D11-A01` 已完成

## 1. 交付结论

D11-A07 已完成代表性闭环：果实、蔬果、药材、纤维、木材、树脂、染料和毒物均有正式 ID、识别门槛、成熟度、工具、采集结果、误采/毒物后果、保护区规则和生态恢复路径。

库存与生态所有权保持冻结边界：

- `D3InventorySystem` 是货舱/库存数量唯一写者；
- `D4EcologySystem` 是植物数量、采集压力和恢复唯一写者；
- `D11VegetationHarvestSystem` 只做确定性规则和两者之间的安全编排；
- UI 只读 `D11VegetationHarvestResult` / `EventLog`；Agent 不能生成资源或改资源量；
- Systems 中没有新增网络或 LLM 依赖。

## 2. 主要文件

### 代码

- `game/Assets/Scripts/Systems/D11/D11VegetationHarvestContracts.cs`
- `game/Assets/Scripts/Systems/D11/D11VegetationHarvestCatalog.cs`
- `game/Assets/Scripts/Systems/D11/D11VegetationHarvestSystem.cs`
- `game/Assets/Scripts/Systems/D4/D4EcologyD11StateMachine.cs`
- `game/Assets/Scripts/Systems/D4/D4Contracts.cs`

### 内容和测试

- `game/Assets/Resources/ContentRegistry/entries/plant/`：8 个 D11-A07 新植物样例，保留 D8 `plant_red_mangrove`；
- `game/Assets/Resources/ContentRegistry/entries/item/`：采集产物和工具条目；
- `game/Assets/Tests/EditMode/D11VegetationHarvestTests.cs`：6 项 EditMode 测试；
- `tools/d11_a07_smoke/`：纯 C# 独立烟测；
- `tools/d11_a07_runner/`：调用编译后测试程序集的 6 项定向测试 runner。

### 文档

- 主成果：`docs/game/design/common/D11_VEGETATION_HARVEST.md`
- 本交接：`docs/game/design/common/handoffs/D11/D11-A07_HANDOFF.md`
- 今日记忆：`.workbuddy/memory/2026-08-11.md`

## 3. 新接口和规则摘要

```text
D11VegetationHarvestSystem.Identify(request)
D11VegetationHarvestSystem.Harvest(request)
D11VegetationHarvestCatalog.Resources / Tools / ProtectionAreas
D4EcologySystem.IsEcologyEventApplied(eventId, saveRevision, eventLog)
D4EcologySystem.HarvestPlant(..., D11PlantHarvestContext context)
```

识别使用技能、图鉴、船员建议和稳定哈希；成熟度使用 `plant_id + day + seed` 稳定周期；工具必须精确匹配；保护区拒绝采集；误采必须显式允许。采集结果通过 Ecology 的 `plant_harvested` 事实记录工具、成熟度、品质、识别 ID、误采、毒性、后果和库存输出数量。

## 4. 保存/读档交接

没有新增第二个 D11-A07 存档。产物沿用 D3 `Cargo`，植被数量/压力沿用 D4 `Ecology.Plants`，去重沿用 D4 `AppliedEcologyEventKeys` 和共享 `EventLogSnapshot`。读档后用保存的 Ecology/EventLog 快照重建采集系统，同一 `event_id + save_revision` 返回 `duplicate_settlement`，不重复增加货舱。

恢复继续由 D4 `AdvanceEcology` / `RestoreHabitat` 裁决，采集系统不复制恢复公式。

## 5. 验证证据

- `Whalebone.Systems`：0 warning / 0 error；
- `Save.Tests`：0 warning / 0 error；
- `D11A07Smoke`：PASS，覆盖识别、成熟采集、毒物误采、保护区和读档重复保护；
- `D11-A07 tests PASS count=6`；
- 内容目录：`Content registry OK: 77 entries, 13 prefixes`；
- 采集代码通过 Systems 红线检查：无 HttpClient、UnityWebRequest、LLM 或网络代码。

本轮未启动 Unity Editor 场景级 EditMode runner；定向测试已在 `Save.Tests.dll` 上执行，Unity EditMode 入口已加入测试程序集。Unity 场景接线由后续 D11-S01 统一集成，不在本任务新增 UI 写入口。

## 6. 交给 D11-S01 / D13 的事项

1. D11-S01 可读取 `D11VegetationHarvestResult` 和 `plant_harvested` 事实，把植被采集接入完整航程 EventLog/UI 读模型；
2. D11-S01 需要验证天气推进后植物恢复和跨域事件链，不得复制 `D4EcologySystem` 恢复公式；
3. 医疗/船员系统可消费 `consequence_id=plant_poisoning`，由其 owner 裁决实际健康后果；
4. D13 种植/农地系统可复用资源 ID、成熟阶段和物品 ID，但种植地块、种子、灌溉和收获状态仍由 D13 对应 owner 写入；
5. 批量植被生产（D15）应沿用本目录字段、唯一 ID、来源、授权和测试 ID 规则，不把代表样例数量当成批量目标完成。
