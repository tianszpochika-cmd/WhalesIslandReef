# D10-A04 交接报告：玩家身份表达

- 任务编号：`D10-A04`
- 日期：2026-08-10
- 前置任务：`D9-S02` 已通过；复核结果 `blocked=0`、14 个冻结事件和 13 个流程阶段全部 PASS。
- 状态：身份功能代码、存档接线、测试与文档完成；生成工程和独立定向测试均已通过。

## 1. 主成果

### 代码

- `game/Assets/Scripts/Systems/D10/D10PlayerIdentitySystem.cs`
  - `D10PlayerIdentityCreation`：创建时姓名、性别表达、代词、称谓、年龄段、文化出身和本地化长文本；
  - `D10PlayerIdentityState`：可保存身份快照、证件记录、身份历史和四类域外历史引用；
  - `D10PlayerIdentitySystem`：港口检查、港口称谓/证件修改和历史追加；
  - `D10AppearanceOwnerWriter`：把外观装备请求委托给已有 `AppearanceSystem`，不复制外观所有权和数值裁决。
- `game/Assets/Scripts/Systems/Save/Models/PlayerState.cs`
  - 新增 `PlayerState.Identity`；保留 `PlayerState.Name` 作为 D7/D8 兼容镜像。
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
  - `NewGameParams.Identity` 创建入口；
  - `SaveD10Identity` / `LoadD10Identity`；
  - 身份写入现有 `players.location_json.identity`，不新增数据库；
  - 当前玩家 ID 不匹配时拒绝身份写入；
  - D7/D8 旧档没有身份块时按旧姓名生成默认身份。
- `game/Assets/Scripts/Systems/Save/SaveMigration.cs`
  - D7→v3 迁移保留已有身份 JSON；无身份的旧档添加 `identity_expression_defaulted` 迁移标记；
  - 顶层 `identity_history` 只投影稳定历史 ID，完整身份仍保存在 additive `identity` 对象中。

### 测试与验证工具

- `game/Assets/Tests/EditMode/D10PlayerIdentityTests.cs`
  - 创建、长文本、本地化、港口检查、称谓/外观/证件修改、拒绝无部分写入、保存读档和历史保留。
- `tools/tests/test_d10_player_identity.py`
  - 静态检查字段、存档入口、文档和 Systems 红线。
- `tools/d10_a04_smoke/`
  - 独立 smoke 编译和行为检查，不依赖 Unity 编辑器锁。

## 2. 数据与所有权

身份数据使用 `schema_version=1`，关键字段如下：

`playerId`、`name`、`genderExpressionKey`、`pronounKeys[]`、`title{key,zh,en}`、`ageBandKey`、`cultureOriginKey`、`identityNote{key,zh,en}`、`documents[]`、`identityHistory[]`、`preservedHistory{legalHistoryRefs,debtHistoryRefs,reputationHistoryRefs,relationHistoryRefs}`。

唯一写者为 `Character/Identity`。法律、债务、声望、关系和外观实体仍由各自 owner 写入；身份系统只保存不可变引用，港口修改采用 merge-only，不清空或重算历史。

外观变更通过 `D10AppearanceOwnerWriter` 调用 `D10AppearanceSystem.Equip`：

- 外观目录、文化/职业限制、拥有权、价格、耐久和伤病层仍归 `AppearanceSystem`；
- 身份历史只保存前后 loadout 引用；
- D5 的完整外观快照继续位于 `location_json.d5.appearance`，身份块不复制它。

## 3. 港口检查与拒绝逻辑

`InspectAtPort` 的港口上下文必须由港口/位置 owner 提供；空位置、`open_sea` 和 `sea` 不被视为港口。

拒绝代码：

- `port_access_required`：不在有效港口上下文；
- `identity_update_empty`：没有称谓、外观或证件意图；
- `invalid_title`：称谓 key/text 缺失或超长；
- `invalid_document`：证件类型缺失、文本超长或有效期倒置；
- `appearance_owner_unavailable`：没有绑定 AppearanceSystem；
- `unknown_appearance_loadout`：外观 owner 拒绝未知/未拥有/不满足上下文的外观。

所有输入先完成校验，外观 owner 也先成功后才追加称谓、证件和身份历史，拒绝不会留下部分写入。

## 4. 跨端同步文件

本任务没有修改 `contracts/`、`server/` 或 `web/`：依据 `D8_DATA_BOUNDARY_FREEZE.md §6.2`，完整 Player 身份存档是 game-only，本地游戏是权威；服务端/官网不持有完整身份实例，因此不存在 Schema 漂移或单端私有协议。

D10-S01 接入时应从 `GameState.CurrentPlayer.Identity` 和 `SaveSystem.LoadD10Identity()` 读取身份；UI 只能读取检查快照并提交意图，不能直接写 SQLite 或修改法律/债务/声望/关系。

## 5. 测试命令与结果

已通过：

```text
python -m unittest tools.tests.test_d10_player_identity -v
# 3/3 passed

dotnet build tools/d10_a04_smoke/D10A04Smoke.csproj --no-restore -v:minimal
dotnet run --project tools/d10_a04_smoke/D10A04Smoke.csproj --no-build
# 0 warning / 0 error；D10-A04 smoke PASS

dotnet build tools/d10_a04_runner/D10A04Runner.csproj --no-restore -v:minimal
dotnet run --project tools/d10_a04_runner/D10A04Runner.csproj --no-build
# 0 warning / 0 error；D10-A04 tests PASS count=4

python -m unittest discover -s tools/tests -v
# 15/15 passed

python tools/d9_s02_acceptance_check.py
# D9-S02 PASS: blocked=0, frozen_events=14

python tools/d9_vertical_slice_check.py
# PASS: scenes=3, prefabs=5, commodities=10, events=14, ui_pages=6, agents=7

python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
# Content registry OK: 34 entries, 13 prefixes

dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal
dotnet run --project game/AgentCore/AgentCore.csproj --no-build
# build 0 warning / 0 error；AgentCore 全部自测通过

npm run build                         # server 通过
python contracts/tests/agent_reply_schema_test.py
# 3 contract schema tests passed

git diff --check
# 通过；仅有既有 LF/CRLF 转换提示，无 whitespace error

dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal
dotnet build game/Save.Tests.csproj --no-restore -v:minimal
dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal
# 三项均 0 warning / 0 error
```

主 Unity Editor XML Runner 本轮未执行，原因是当前工程仍有并行编辑器实例占用；这不影响已通过的生成程序集编译和独立 NUnit 定向入口。`D10.Tests.D10PlayerIdentityTests` 的四个测试方法已通过 `tools/d10_a04_runner` 实际执行，SQLite native DLL 由 runner 显式绑定到 `game/Assets/Plugins/x86_64/sqlite3.dll`。

## 6. 后续接线与责任

1. 释放 Unity 工程锁后，可在 Editor Test Runner 再归档同一组 `D10.Tests.D10PlayerIdentityTests` XML；
2. D10-S01 集成时验证身份改称谓/外观不会删除 D10-A03 关系、D10-A05 外观历史或后续法律/债务投影。
