# D13-A03 交接报告：农业、果园与蔬菜

任务：`D13-A03`  
日期：2026-08-11  
状态：完成

## 已完成

- 新增农业契约、统一数值配置和样例目录，覆盖果实、蔬菜、甘蔗、烟草、咖啡、可可、香料、药草八种标签。
- 新增 `D13FarmingSystem`，唯一写作物播种、成长、成熟、健康、病虫害、风暴、收获、轮作、加工记录和销售记录。
- 新增 `D4EcologySystem` 农业桥接，唯一写土壤肥力、湿度、水源可用量、污染、生态压力、病虫害压力和风暴环境后果。
- 新增 `D13FarmingPropertyAuthority` 只读适配器，保护区、产权争议、所有权/租赁/共同治理资格由 D13-A01 PropertySystem 提供。
- 种子、收成、加工品和病虫害处理剂均由 `D3InventorySystem` 注册并写入；农业系统不直接写 Cargo 字典。
- SaveSystem/GameState/PlayerState 接入 `D13FarmingRuntimeSnapshot`，保存作物、环境、加工、销售、轮作及幂等键。
- 新增 `D13FarmingSystemTests` 与 `tools/d13_a03_runner`。
- 新增内容注册条目：8 种种子、7 种加工品、1 种病虫害处理剂；内容注册检查通过。

## 对外接口

```text
D13FarmingSystem.Plant / Water / AssignLabor / TreatPests
D13FarmingSystem.AdvanceDay / Harvest / Rotate / Process / Sell
D13FarmingSystem.GetReadModel()
D13FarmingSystem.CaptureSnapshot() / Restore(snapshot)
D13FarmingSystem.SeasonFor(worldDay)
```

依赖接口：

```text
ID13FarmingEcologyAuthority
ID13FarmingLandAuthority
ID13FarmingEconomyAuthority
```

`D13FarmingPropertyAuthority` 可包装 D13-A01 `D13PropertyAssetSystem`；经济 authority 应包装 Economy/Finance 的报价和账本结果。农业销售不会直接增加现金。

## 运行验证

```text
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162
dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162
dotnet run --project tools/d13_a03_runner/D13A03Runner.csproj --no-restore
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

结果：Systems 0 错误；Save.Tests 0 错误；D13-A03 测试 `3/3 PASS`；内容注册表 `95 entries / 13 prefixes PASS`。

测试覆盖：

1. 湿季木薯播种 → 浇水 → 劳力 → 风暴 → 病虫害处理 → 成熟收获 → 轮作 → 加工 → Economy 报价销售 → SaveSystem 读档 → 重复销售/收获拦截；
2. 八类作物类型、标签、Dry/Wet/Hurricane 季节规则；
3. 产权资格拒绝和经济拒绝不扣种、不扣货、不改农业状态。

## 交接边界与待办

1. D13-S01 集成时应同时接入 D11 生态采集快照、D13-A01 产权资格、D13-A02 Economy/Finance 账本，并由各自 owner 保存完整共享 `EventLogSnapshot`。
2. 真实 UI 只消费 `GetReadModel()`、`D13FarmingResult` 和共享事件，不得持有 `D3InventoryState` 的写引用或自行改变价格。
3. 后续 D13 农业扩展可增加更多作物和加工链，但必须继续使用 `D13FarmingCropDefinition` 的独立标签、批次、季节和风险字段；不得把作物状态复制到 Property、Economy、Agent 或 UI。
4. `crop_*` 是农业系统内部目录 ID；跨端若要展示新物品，需另行按 D8 契约注册，不能把未注册的内部作物 ID当作通用 ContentRegistry 条目。

## 红线复核

- `game/Assets/Scripts/Systems/D13/` 与农业 Ecology bridge 无 LLM、HTTP、网络或 UnityWebRequest。
- FarmingSystem 没有金钱、价格、产权、法律、土壤/水源或库存的第二写入口。
- 失败事件带确定性恢复路线；Agent/UI 没有农业裁决接口。
- 存档快照含应用键，读档后重复请求不会再次扣种、生成收成、加工或销售。
