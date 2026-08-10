# D8 数值、范围与枚举统一配置

状态：D8-A06 已完成（2026-08-10）  
前置：D8-S01 数据边界冻结已完成  
唯一代码入口：`game/Assets/Scripts/Agent/AgentCore/NumericEnumConfiguration.cs`

## 1. 目标与边界

本文件登记跨系统重复出现、需要统一单位或安全边界的数值，以及天气、稀有度、法律状态和品质枚举。默认值是当前 D7/D8 实现行为的登记值，不是本任务的最终经济或战斗平衡方案。

`NumericConfiguration.Default` 是只读运行时入口。纯 C# AgentCore 通过 `game/AgentCore/AgentCore.csproj` 的 linked compile 使用同一份源码，Unity 的 Agent Runtime 直接编译同一文件。配置只提供类型、单位、默认值和安全边界；实际经济、战斗、关系、生态和时间状态仍由各自 owner 系统裁决。

UI 只能读取并展示配置，Agent 只能提出关系变更，不能写入配置或裁决数值。Systems 中没有加入网络或 LLM 代码，也没有修改 contracts。

## 2. 数值登记表

整数参数在代码中仍以 `int` 读取，非整数参数以 `float`/`decimal` 读取；登记表中的数值使用 `decimal` 保存，避免配置元数据自身丢失精度。

| ID | 类型 / 单位 | 默认值 | 最小值 | 最大值 | 拥有者 | 来源 | 测试入口 |
|---|---|---:|---:|---:|---|---|---|
| `money_amount` | Integer / 银币 | 0 | -1,000,000,000 | 1,000,000,000 | Economy/Finance | `DATA_MODELS §2.1`、D8 freeze §4 | `D8A06_Config_Money` |
| `starting_money` | Integer / 银币 | 500 | 0 | 1,000,000,000 | Economy | D7 `PlayerState.StartingMoney`、D8 freeze §3 | `D8A06_Config_StartingMoney` |
| `cash_balance` | Integer / 银币 | 0 | 0 | 1,000,000,000 | Economy/Finance | D3 `D3TradeState.Money`、D8 freeze §7 | `D8A06_Config_CashBalance` |
| `weight` | Decimal / 公吨 | 0 | 0 | 100,000 | Inventory/Cargo | `DATA_MODELS §2.6`、D8-A01 `WeightAmount` | `D8A06_Config_Weight` |
| `volume` | Decimal / 立方米 | 0 | 0 | 100,000 | Inventory/Cargo | `DATA_MODELS §2.2/§2.6`、D8-A01 `VolumeAmount` | `D8A06_Config_Volume` |
| `item_unit_volume` | Integer / 立方米 | 1 | 1 | 100,000 | ItemCatalog/Inventory | D7 `CommodityDefinition.cargoVolume` | `D8A06_Config_ItemVolume` |
| `distance` | Decimal / 米 | 0 | 0 | 1,000,000 | Environment/Ship | `DATA_MODELS §2.2`、D8-A01 距离类型 | `D8A06_Config_Distance` |
| `combat_distance` | Decimal / 米 | 35 | 10 | 100 | Combat | D4 `BeginEncounter`/`Fire` | `D8A06_Config_CombatDistance` |
| `boarding_distance` | Decimal / 米 | 28 | 0 | 100 | Combat | D4 `Board` | `D8A06_Config_BoardingDistance` |
| `world_day` | Integer / 世界日 | 0 | 0 | 1,000,000 | Environment/Time | `DATA_MODELS §1`、D8 freeze §2.1 | `D8A06_Config_WorldDay` |
| `seconds_per_world_day` | Decimal / 秒 | 600 | 1 | 86,400 | Environment/Time | D2 `DayNightSystem`/`DayNightController` | `D8A06_Config_DayLength` |
| `time_of_day` | Decimal / 归一化日 | 0.25 | 0 | 1 | Environment/Time | D2 `DayNightSnapshot`、D8 freeze §2.1 | `D8A06_Config_TimeOfDay` |
| `relation_value` | Decimal / 关系点 | 0 | -100 | 100 | Character/Crew | `DATA_MODELS §2.3/§2.4`、D2 AgentModels | `D8A06_Config_Relation` |
| `relation_proposal_delta` | Integer / 关系点 | 0 | -10 | 10 | Character/Crew（提议边界） | D2 `AgentReplyValidator`/`CrewRelationSystem` | `D8A06_Config_RelationProposal` |
| `mutiny_affinity_threshold` | Decimal / 关系点 | -50 | -100 | 100 | Character/Crew | D2 `MutinyMonitor` 分支锁 | `D8A06_Config_MutinyThreshold` |
| `health_percent` | Integer / 百分比 | 100 | 0 | 100 | Character/Crew；Ecology 读取范围 | `DATA_MODELS §2.3/§2.6`、D4 health projection | `D8A06_Config_Health` |
| `fatigue_percent` | Integer / 百分比 | 0 | 0 | 100 | Character/Crew | `DATA_MODELS §2.3`、D7 `CrewStats` | `D8A06_Config_Fatigue` |
| `morale_percent` | Integer / 百分比 | 60 | 0 | 100 | Character/Crew | D7 `CrewStats` | `D8A06_Config_Morale` |
| `loyalty_percent` | Integer / 百分比 | 60 | 0 | 100 | Character/Crew | D7 `CrewStats` | `D8A06_Config_Loyalty` |
| `wanted_level_percent` | Integer / 百分比 | 0 | 0 | 100 | Faction/Legal | D4 `FactionState.WantedLevel` | `D8A06_Config_WantedLevel` |

范围只代表安全边界。比如 `combat_distance` 的默认值仍为 35 米，`boarding_distance` 的默认值仍为 28 米；D8-A06 没有重新计算或平衡战斗结果。

## 3. 枚举登记表

枚举使用 C# canonical 类型，D3/D4 的旧字段只保留兼容投影。`Unknown` 是数据缺失/未知状态，不等同于合法、普通或标准。

| ID / canonical 类型 | 默认值 | 允许值 | 拥有者 | 来源 | 测试入口 |
|---|---|---|---|---|---|
| `weather_state` / `WeatherState` | `Calm` | `Unknown`, `Calm`, `TradeWind`, `Rain`, `Storm`, `Hurricane` | Environment | D3 `D3WeatherType` compatibility、`REQUIREMENTS REQ-WORLD` | `D8A06_Config_Weather` |
| `rarity` / `Rarity` | `Common` | `Unknown`, `Common`, `Uncommon`, `Rare`, `VeryRare`, `Unique` | Item/Ecology/Collection | `DATA_MODELS §2.6/§2.7`、D4 `Rare` compatibility | `D8A06_Config_Rarity` |
| `legal_status` / `LegalStatus` | `Legal` | `Unknown`, `Legal`, `Licensed`, `Restricted`, `Illegal` | Faction/Legal | `DESIGN_BOUNDARIES §4`、D8-A01 compatibility | `D8A06_Config_Legal` |
| `quality_grade` / `QualityGrade` | `Standard` | `Unknown`, `Poor`, `Standard`, `Good`, `Excellent`, `Masterwork` | Inventory/Ecology/Collection | `DATA_MODELS §2.6 CatchRecord.quality` | `D8A06_Config_Quality` |

## 4. 读取、校验和测试入口

- 读取默认值：`NumericConfiguration.Default.<Parameter>`；读取定义元数据：`NumericConfiguration.NumericParameters` 和 `EnumParameters`。
- 读取并限制状态：使用 `ClampRelation`、`ClampHealth`、`ClampFatigue`、`ClampMorale`、`ClampLoyalty`、`ClampCash`、`ClampItemUnitVolume`、`ClampTimeOfDay`、`ClampSecondsPerWorldDay`、`ClampCombatDistance` 和 `ClampBoardingDistance` 等统一入口。
- 校验配置快照：`NumericConfiguration.Validate(profile)` 返回带 parameter ID、错误码和说明的列表；启动期可用 `ThrowIfInvalid(profile)` 直接拒绝非法配置。范围顺序、默认值位置、D8 安全边界和未知枚举都会被发现。
- 纯 C# 配置检查：

  ```text
  dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal
  dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --config-check
  ```

  `NumericConfigurationTests.RunAll()` 覆盖默认值、元数据完整性、边界读取、非法范围和非法枚举。配置检查命令返回 0 才允许交接。

## 5. 已替换的重复硬编码

| 原读取点 | 原散落值 | 统一入口 |
|---|---|---|
| D2 CrewRelation / AgentReplyValidator / AgentModels | 关系 `-100/100`、提议 `-10/10` | `Relation`、`RelationProposalDelta` 和对应 clamp/validator |
| D2 MutinyMonitor | `affinity < -50` | `MutinyAffinityThreshold.DefaultValue` |
| D2 DayNightSystem、Controller、SaveSystem | 600 秒/日、初始日和 0.25 时间 | `SecondsPerWorldDay`、`WorldDay`、`TimeOfDay` |
| D3 贸易状态和货物定义 | 现金非负、货物体积至少 1 | `CashBalance`、`ItemUnitVolume` |
| D3 天气默认值 | 旧 `D3WeatherType.Calm` 初始化 | canonical `WeatherState.Calm` + `LegacyD3WeatherIndex` 投影 |
| D4 战斗 | 35 米、90 米安全区间、28 米接舷 | `CombatDistance`、`BoardingDistance` |
| D4 生态/生存/角色/法律 | 生命、疲劳、士气、忠诚、声望和通缉等级范围 | `Health`、`Fatigue`、`Morale`、`Loyalty`、`Relation`、`WantedLevel` |
| D4 动物稀有度 | `bool Rare` | `Rarity`；`Rare` 作为兼容属性映射，不改变目录结果 |
| Save PlayerState / D8-A01 V3 typed values | 500 起始银币、重量/体积/时间/关系/百分比边界 | 对应 `NumericConfiguration` 参数 |

## 6. 明确未在 D8-A06 中改变的内容

- 没有调整商品价格、奖励、工资、维护费、战利品、掉落或任何经济公式。
- 没有调整伤害、命中、射程效果、接舷结果、疾病/受伤数值或战斗胜负公式；35 米和 28 米只是把现有默认值登记为配置。
- 没有把 D5 UI `SettingsStore` 的图形/音频质量变成玩法 `QualityGrade`；两者所有权不同。
- 没有把 UI 或 Agent 变成参数写者，也没有把领域状态写权迁移到配置类。
- `LegalStatus` 和 `QualityGrade` 先完成 canonical 类型与登记；现有未拥有它们的业务系统不被强行改写。

## 7. 文件与所有权

配置源码放在 Unity Agent Runtime 的纯 C# 子目录，未引用 Unity、网络或第三方库；AgentCore 项目通过 linked compile 复用它。D8-A06 只修改 Settings/纯 C# 配置、配置读取点、配置测试和必要文档，跨端 contracts 未变更。
