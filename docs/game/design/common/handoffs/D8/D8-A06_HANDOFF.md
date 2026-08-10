# D8-A06 任务交接报告

任务：D8-A06 数值、范围、天气/稀有度/法律/品质枚举统一配置  
日期：2026-08-10  
前置：D8-S01 已完成；D8 数据边界冻结表作为本任务来源。

## 交付摘要

- 新增唯一纯 C# 配置入口：`game/Assets/Scripts/Agent/AgentCore/NumericEnumConfiguration.cs`。
- AgentCore 通过 `game/AgentCore/AgentCore.csproj` linked compile 复用同一份配置源码，避免 Standalone 与 Unity 各自维护参数。
- 登记 20 个数值参数和 4 个 canonical 枚举；每项都有类型、单位/允许值、默认值、边界、拥有者、来源和测试入口。
- 新增 `NumericConfiguration.Validate`/`ThrowIfInvalid`，可发现空范围、反向范围、超出安全边界、默认值越界和未知枚举。
- 新增 `--config-check` 入口和纯 C# 配置测试；没有新增网络或 LLM 代码。
- 新增说明文档：`docs/game/design/common/D8_NUMERIC_ENUM_CONFIG.md`，并登记到设计文档索引。

## 新增配置

### 数值

`money_amount`、`starting_money`、`cash_balance`、`weight`、`volume`、`item_unit_volume`、`distance`、`combat_distance`、`boarding_distance`、`world_day`、`seconds_per_world_day`、`time_of_day`、`relation_value`、`relation_proposal_delta`、`mutiny_affinity_threshold`、`health_percent`、`fatigue_percent`、`morale_percent`、`loyalty_percent`、`wanted_level_percent`。

### 枚举

- `WeatherState`：`Unknown`、`Calm`、`TradeWind`、`Rain`、`Storm`、`Hurricane`。
- `Rarity`：`Unknown`、`Common`、`Uncommon`、`Rare`、`VeryRare`、`Unique`。
- `LegalStatus`：`Unknown`、`Legal`、`Licensed`、`Restricted`、`Illegal`。
- `QualityGrade`：`Unknown`、`Poor`、`Standard`、`Good`、`Excellent`、`Masterwork`。

完整登记（单位、默认值、范围、拥有者、来源和测试 ID）见 `D8_NUMERIC_ENUM_CONFIG.md`。

## 替换的硬编码读取点

- D2 关系值、Agent 关系提议校验、CrewRelationSystem clamp、MutinyMonitor 阈值。
- D2 日夜系统、DayNightController、SaveSystem 的日长、初始世界日和归一化时间。
- D3 现金/货物体积边界，以及旧天气枚举到 canonical `WeatherState` 的兼容投影。
- D4 战斗距离、接舷距离、生态/生存/船员健康与疲劳边界、势力声望和法律系统通缉等级边界。
- D4 `AnimalDefinition.Rare` 改为 canonical `Rarity` 的兼容属性；现有内容初始化行为保持不变。
- `PlayerState.StartingMoney`、D8-A01 V3 typed values 的重量/体积/世界日/时间/关系/百分比边界。

未替换的价格、奖励、战斗公式常量属于本任务明确排除的最终平衡内容；文档中已单独列出边界。

## 配置检查命令

```text
dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --config-check
```

配置测试入口：`Whalebone.Game.Config.NumericConfigurationTests.RunAll()`。覆盖默认配置、元数据完整性、D7 边界行为、非法范围和非法枚举。

Unity 生成解决方案若出现缺少新增源码的情况，需要使用项目要求的 Unity `6000.0.80f1` 重新生成 IDE 工程；不要直接编辑生成的 `Whalebone.Agent.Runtime.csproj`。当前机器检测到的 Unity Editor 为 `2022.3.52f1c1`，与项目版本不一致，因此该生成步骤不作为 D8-A06 的代码修改目标。

## 测试结果

- AgentCore 构建：通过，0 warning / 0 error。
- `--config-check`：通过，5 项 `[OK]`：默认值合法、参数元数据完整、边界读取保持 D7 行为、非法数值范围可发现、非法枚举可发现。
- Unity 解决方案：待项目版本 `6000.0.80f1` 重新生成后复跑；此前失败原因是生成的工程文件尚未包含新增配置源码及其他已有未提交 D8-A04 源码，不是 Standalone 配置测试失败。

## 交接注意

- 配置类只提供统一入口与合法性检查，不拥有经济、战斗、关系、生态或时间状态；领域系统仍是唯一写者。
- D3/D4 旧枚举和布尔字段保留兼容投影，避免本任务引入存档/契约破坏。
- 后续若需要调最终经济或战斗平衡，应由对应领域 owner 单独提出变更，并补充其领域测试，不要在本任务配置表中直接改默认值。
