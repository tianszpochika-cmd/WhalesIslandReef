# D14-A03 交接报告

## 任务

- 任务编号：D14-A03
- 目标：特殊炮架、火枪、信号器具、捕缆/救援工具、历史武器和独特船体模块的 ID、来源、授权、获取、装配、维护、强度边界、损坏、出售、收缴和恢复。
- 状态：完成。

## 修改文件

- `game/Assets/Scripts/Systems/D14/D14SpecialEquipmentContracts.cs`
- `game/Assets/Scripts/Systems/D14/D14SpecialEquipmentCatalog.cs`
- `game/Assets/Scripts/Systems/D14/D14SpecialEquipmentSystem.cs`
- `game/Assets/Scripts/Systems/Save/GameState.cs`
- `game/Assets/Scripts/Systems/Save/Models/PlayerState.cs`
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- `game/Whalebone.Systems.csproj`
- `game/Save.Tests.csproj`
- `game/Assets/Tests/EditMode/D14SpecialEquipmentTests.cs`
- `tools/d14_a03_runner/D14A03Runner.csproj`
- `tools/d14_a03_runner/Program.cs`
- `game/Assets/Resources/ContentRegistry/entries/item/` 下 6 个 D14 条目及 `registry.json` roots。
- `docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md`

## 新接口和数据边界

- `D14SpecialEquipmentDefinition`：不可变目录定义、统一 `item_` ID、槽位、强度/耐久上限、来源、授权和历史审查。
- `D14EquipmentInstance`：唯一实例和状态投影；实例 ID 由定义 ID 与来源事件稳定生成。
- `D14SpecialEquipmentSystem`：只编排请求、来源事实、authority 决定、统一 EventLog 和快照。
- `ID14EquipmentRewardAuthority`：获取和遗失恢复。
- `ID14EquipmentShipAuthority`：装配和维护。
- `ID14EquipmentCombatAuthority`：损坏/遗失。
- `ID14EquipmentTradeAuthority`：出售、价格、Inventory 出库来源。
- `ID14EquipmentLegalAuthority`：收缴和证据状态。
- `D14SpecialEquipmentRuntimeSnapshot`：装备、操作、应用键和共享 EventLog。

最终值不由 Agent、UI 或 A03 直接产生：

- 强度、耐久、船型兼容由 Ship/Combat authority 返回并受目录上限校验；
- Inventory/Cargo 只由 InventorySystem 写；
- 获取/奖励由 RewardSystem 或 CollectionAtlasSystem 写；
- 出售价格和账本由 EconomySystem 写；
- 收缴和合法性由 FactionLegalSystem 写。

## 历史与授权

- 历史武器和 `historical_tag=historical` 条目必须有历史审查事实；
- `ApprovedWithWarning` 必须有显式警告确认；
- 所有正式条目有 `source_refs`、`source_note`、license、owner_system、reviewer 和 test_id；
- 收缴保存 `Seized` 与 `Evidence`，恢复不允许绕过法律释放。

## 验收结果

- `D14-A03 tests PASS count=2`；
- Systems 编译：0 warning / 0 error；
- Save.Tests 编译：0 warning / 0 error；
- 内容注册表：116 entries / 13 prefixes PASS；
- A03 Systems 红线：无网络、LLM 或 Agent 裁决逻辑。

## 交接注意事项

1. D14-A04 接入奖励时必须返回 `InstanceId`、奖励来源、Inventory 来源和授权事实；不能在 RewardSystem 外复制实例。
2. D14-A05 接入法律时必须继续保留收缴证据和许可证来源；`Seized` 实例不能直接恢复或出售。
3. D14-S01 只读取装备快照和事实事件；海战损坏、船体最终状态、价格、库存和法律状态继续由各 owner 写入。
4. D14-A03 目前提供代表性目录，不承诺需求中的完整 80 个改装模块规模。
