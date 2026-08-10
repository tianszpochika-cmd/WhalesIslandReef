# D12-A03 交接报告：线索与寻宝任务

任务：`D12-A03`  
前置：`D9-S02 = PASS`、`D12-A01` 海图发现成果  
状态：已完成，允许后续任务读取寻宝线索事实和奖励请求

## 1. 新增配置与接口

- `D9TreasureClueSourceKind`：港口传闻、打捞海图、船员证词、地图发现、直接观察五类结构化来源。
- `D9TreasureRewardType`：地图知识、货币、物品、关系/声望四类奖励路由标记；不包含金额、数量或最终关系值。
- `D9TreasureClueRecord`：保存 `ClueId`、来源、证据引用、可靠度、目标、世界日窗口、装备引用、替代路线、事实/因果引用和交叉验证引用。
- `D9TreasureClueRequest` / `D9TreasureClueVerificationRequest`：强逻辑记录和交叉验证请求。
- `D9TreasureQuestSnapshot.Clues`、`VerifiedClueIds`、`CrossValidationCount`：与原 D9 Quest 快照一起保存。
- `D9TreasureRewardRequest.RewardType`、`CauseEventIds`、`ClueIds`：提供给 RewardSystem 的可追溯请求，不发放奖励。
- Quest 入口：`RecordClue`、`RecordCatalogClue`、`CrossValidateClues`；D9 Unity runtime bootstrap 增加对应转发入口。

第一张海图内容目录新增三来源路径：

- `clue:first_chart_tide_mark` / `rumor:santa_margarita_dockhand`
- `clue:first_chart_salvaged_chart` / `chart:salvaged_coastal_fragment`
- `clue:first_chart_reef_keeper` / `npc:green_cay_reef_keeper`

## 2. 替换的散落硬编码

- 原先只在 `D9TreasureClueDefinition` 和 `HearRumor` 中保存单个来源/可靠度；现在统一生成 `D9TreasureClueRecord`，所有新线索都必须通过来源、证据、可靠度、目标和枚举合法性检查。
- 原先寻宝结算固定以 `event_d9_treasure_clue` 作为唯一因果；现在按选用线索的 `FactEventRef` 写入 `cause_ids`，交叉验证事实同时引用两条来源事实。
- 原先成功奖励请求只有固定 `rewardId`；现在增加 `RewardType`、`ClueIds`、`CauseEventIds`，仍没有价格、数量、库存增量或最终奖励结果。
- 原先登陆请求没有可选线索和验证门；现在以 `ClueId`、`RequireCrossValidation`、`EnforceClueWindow` 让交叉验证、时间窗和装备前置真正影响结果。
- D9 SaveSystem JSON 增加线索、验证、窗口、奖励类别和因果列表读写；未另建数据库或第二份 EventLog。

本任务没有修改价格、经济公式、奖励数量、战斗公式或关系数值。

## 3. 事实事件和所有权

| 事实 | owner | 作用 |
|---|---|---|
| `event_d9_treasure_clue` | `QuestSystem` | 冻结的主传闻事实 |
| `event_d12_treasure_clue_<clue_id>` | `QuestSystem` | 结构化来源线索事实 |
| `event_d12_treasure_cross_validate_<a>_<b>` | `QuestSystem` | 不同来源、同一目标的交叉验证事实 |
| `event_d9_treasure_resolved` | `QuestSystem` | 登陆成功或失败事实，失败带恢复路由 |
| `event_d9_treasure_recovered` | `QuestSystem` | 恢复事实，沿 `cause_ids` 指回失败 |

QuestSystem 只拥有任务/线索投影。Inventory/Cargo、Economy、Relation 和 RewardSystem 继续各自拥有自己的状态，UI/Agent 只有快照和事实读取权限。Agent 只收到 `BuildClueExpressionProposal` 的表达提议，不拥有线索裁决权。

## 4. 配置检查命令

```powershell
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162
dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162
dotnet run --project tools/d12_a03_smoke/D12A03Smoke.csproj
dotnet run --project tools/d9_a06_smoke/D9A06Smoke.csproj
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

## 5. 测试结果

- `Whalebone.Systems`：PASS，0 warning / 0 error。
- `Save.Tests`：PASS，0 warning / 0 error；`D12TreasureClueTests` 已纳入测试程序集。
- `D12-A03` 纯 C# 烟测：PASS，覆盖多来源、交叉验证、奖励因果、读档和重复发现保护。
- `D9-A06` 既有烟测：PASS，覆盖旧有成功/失败/恢复/读档/重复保护。
- Unity EditMode 未在本轮强行启动：当前仓库已有 Unity 进程占用项目；源文件已编译进入 `Save.Tests`，释放项目后运行 `D12TreasureClueTests` 即可补齐编辑器执行记录。
- `content_registry_check.py`：当前工作区已有 6 个与本任务无关的内容注册表错误（`asset_historical_saint_marys_log`、`item_special_weapon_moonwake_cutlass` 的 owner/test_id/orphan-reference）；D12-A03 未修改这些内容条目。

## 6. 后续接入

1. D12-A02 或后续地图任务从 `D12MapDiscoverySystem.RecordQuestKnowledge` 形成地图事实，再调用 `RecordCatalogClue`，不要复制 `D12ChartEvidence` 或任务状态。
2. UI 只显示 `D9TreasureQuestSnapshot` 和 EventLog 只读投影；交互通过 bootstrap 的结构化请求入口提交。
3. RewardSystem 领取成功事件前继续调用 D8 `EventLog.ClaimSettlement`；寻宝系统不直接发放任何奖励。
4. 如果需要跨端同步 `D9TreasureClueRecord`，先把字段加入 contracts 并同步 game/server/web，不在本任务增加私有协议。
