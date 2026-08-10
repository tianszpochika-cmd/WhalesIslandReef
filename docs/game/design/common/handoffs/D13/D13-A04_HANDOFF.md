# D13-A04 交接报告：牧场、家禽与牲畜

任务：`D13-A04`  
日期：2026-08-11  
状态：完成

## 已交付

- `D13RanchSystem`：来源登记、检疫释放、饲料、饮水、健康、疾病、空间、劳力、单周期产物、死亡、销售和繁殖延后。
- `D4EcologySystem` 牧场水源桥：水源剩余量由 EcologySystem 唯一写入并可保存恢复。
- `D13RanchPropertyAuthority`：只读复用 D13-A01 产权经营资格，不复制产权状态。
- `D13RanchRuntimeSnapshot` 与 SaveSystem `location_json.d13Ranch` 接线。
- D8 内容样例：四类家禽/牲畜、饲料、药剂和四类养殖产物；registry roots 已登记。
- `D13RanchSystemTests`、`D13A04Runner` 和本设计文档。

## 新接口与所有权

- Ranch → `ID13RanchSourceAuthority.ValidateSource`：请求 AnimalCare/检疫 authority 返回来源、物种、检疫和 owner。
- Ranch → `ID13RanchLandAuthority.ValidateFacility`：请求 Property/法律 authority 返回经营资格和来源事件。
- Ranch → `ID13RanchWaterAuthority.ConsumeWater`：请求 EcologySystem 扣水；Ranch 不写水源量。
- Ranch → `ID13RanchEconomyAuthority.QuoteAndApproveSale`：请求 EconomySystem 裁决价格、市场和账本；Ranch 不写金钱。
- Ranch → `D3InventorySystem.TryRemoveCargo` / `TryAddCargo`：饲料、药剂和产品库存唯一写者。

## 验证结果

```text
dotnet build game/Whalebone.Systems.csproj --no-restore   PASS
dotnet build game/Save.Tests.csproj --no-restore          PASS
dotnet build tools/d13_a04_runner/D13A04Runner.csproj    PASS
D13-A04 runner                                             3/3 PASS
```

测试确认：来源和检疫事实缺失会拒绝；不同动物使用不同产品/标签和固定门槛；缺料/缺水/缺劳力会扣健康并最终记录死亡；疾病可通过药剂恢复；空间、产权和市场拒绝不改变错误 owner 的状态；繁殖不会创建隐式子代；保存/读档不会重复产出、扣货或销售。

## 后续接入注意

1. D13-S01 只应消费 Ranch read model、EventLog 和存档快照；不得在集成层直接改动物、库存、水源或价格。
2. 真实游戏接入时，AnimalCareSystem/来源 authority 必须提供带 `AnimalCareSystem` owner 的来源与检疫事实；PropertySystem/EconomySystem 必须提供可追踪来源事件。
3. 后续繁殖、屠宰、加工和粪肥生态回流应另立 owner 设计，不要扩展成隐式自动生成。
4. UI 只显示健康、疾病、输入缺口、最坏结果和恢复路线，并提交结构化操作请求。

## 红线复核

- `game/Assets/Scripts/Systems/` 新增代码无网络、HttpClient 或 LLM。
- Inventory/Cargo、Ecology、Property、Economy 未被 Ranch 复制或越权写入。
- Agent 没有繁殖、疾病、产物或交易裁决路径。
