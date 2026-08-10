# D8-A04 任务交接报告：AgentReply v1.1 与 P2A Action 注册表

- 任务编号：`D8-A04`
- 前置任务：`D8-S01`（已完成）
- 完成日期：`2026-08-10`
- 交接状态：契约、AgentCore、server gateway、Unity Mock 测试与文档已同步；Unity 主工程 EditMode 测试被既有 D7 编译错误阻断，详见第 7 节。

## 1. 契约版本与边界

- AgentReply 契约版本：`agent_reply.v1.1`
- P2A Action 注册版本：`p2a.v1`
- 契约单一事实源：`contracts/agent-reply.schema.json`
- Action 注册单一事实源：`contracts/p2a-action-registry.json`
- AgentReply 新增并强制要求：对话意图、情绪、关系提议、谈判语气、拒绝原因、动作意图。
- `affinity_change` 仅是 `-10..10` 的关系变化提议，不是最终关系结果。
- AgentReply 不允许直接携带最终 `money`、`price`、`damage`、`reward`、`weather` 或 relation 数值结果；最终结果仍由所属强逻辑系统裁决。

## 2. 60 个 P2A Action 清单

`trim_sail`、`steer`、`set_course`、`drop_anchor`、`weigh_anchor`、`stop_ship`、`set_watch`、`report`、`heave_to`、`change_speed`、`fire_cannon`、`repair`、`bilge_pump`、`ration`、`hold_below`、`reward_crew`、`launch_boat`、`board`、`extinguish_fire`、`secure_cargo`、`assemble_crew`、`gather`、`scout`、`treat`、`trade`、`cook_meal`、`distribute_rations`、`assign_duty`、`call_meeting`、`request_leave`、`greet_contact`、`ask_information`、`propose_trade`、`propose_alliance`、`request_passage`、`request_mediation`、`offer_apology`、`negotiate_terms`、`accept_offer`、`end_talk`、`survey_coast`、`inspect_wreck`、`map_landmark`、`observe_animal`、`rescue_animal`、`release_animal`、`fish`、`collect_water`、`collect_firewood`、`enter_cave`、`buy_supplies`、`sell_cargo`、`visit_shipyard`、`request_upgrade`、`hire_crew`、`dismiss_crew`、`store_cargo`、`retrieve_cargo`、`submit_quest`、`tend_garden`。

每个 Action 均已登记：稳定注册名、参数 Schema、权限、前置条件、失败码、执行者、允许的 actor/target 类型。完整机器可读字段和可读摘要见 `contracts/p2a-action-registry.json` 与 `docs/game/design/common/D8_P2A_ACTION_REGISTRY.md`。

## 3. 拒绝逻辑

1. 顶层未知字段、缺少必填字段、错误版本、非法字符串/枚举、对白超长、`affinity_change` 越界或非整数直接拒绝。
2. `relation_proposal` 只接受固定四字段；关系目标必须是授权快照中的对象；禁止把数值结果塞进关系提议。
3. 未知 Action、错误 actor/target 引用格式、未允许的对象类型、`none` 携带执行对象或参数均拒绝。
4. Action 参数按注册表逐项校验：未知参数、缺少必填参数、类型错误、枚举错误、正则不匹配、超范围整数均拒绝。
5. 运行时 `TryValidate` / gateway 校验继续检查 actor、target、权限和前置条件；不满足分别返回 `unauthorized_actor`、`unauthorized_target`、`permission_denied`、`missing_prerequisite`。
6. 拒绝原因非 `none` 时不得同时提交可执行 Action；递归参数对象/数组中的最终结果字段也一并拒绝。
7. 校验器只做边界拒绝，不执行船、经济、战斗、天气、奖励或关系结算。

## 4. 跨端同步文件

### contracts

- `contracts/agent-reply.schema.json`
- `contracts/p2a-action-registry.json`
- `contracts/test-fixtures/agent-reply/*.json`
- `contracts/tests/agent_reply_schema_test.py`

### game / AgentCore / Unity

- `game/AgentCore/AgentModels.cs`
- `game/AgentCore/AgentReplyValidator.cs`
- `game/AgentCore/P2AActionRegistry.cs`
- `game/AgentCore/AgentReplyTestFixtures.cs`
- `game/AgentCore/AgentCoreTests.cs`
- `game/AgentCore/D2DialogueTests.cs`
- `game/AgentCore/README.md`
- Unity 镜像：`game/Assets/Scripts/Agent/AgentCore/AgentModels.cs`、`AgentReplyValidator.cs`、`P2AActionRegistry.cs`、`AgentReplyTestFixtures.cs`、`AgentCoreTests.cs`、`D2DialogueTests.cs`
- Unity Mock：`game/Assets/Tests/EditMode/D8A04AgentReplyMockTests.cs`

### server / web

- `server/src/modules/gateway/gateway.service.ts`
- `server/test/gateway.service.test.ts`
- `server/package.json`
- `web/` 未发现独立 AgentReply DTO 或私有协议实现，因此无需新增 web 私有同步；web 继续以 `contracts/` 为跨端入口。

### docs

- `docs/game/design/common/D8_P2A_ACTION_REGISTRY.md`
- `docs/game/ACTIONS.md`
- 本报告与 `.workbuddy/memory/2026-08-10.md`

## 5. 测试命令与结果

- `python contracts/tests/agent_reply_schema_test.py`：通过，3 项契约测试。
- `dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal`：通过，0 警告、0 错误。
- `dotnet run --project game/AgentCore/AgentCore.csproj -- --self-test`：通过，全部 AgentCore、D2、D8-A04、既有配置和 v3 自测通过。
- `npm run build`（`server/`）：通过。
- `npm test`（`server/`）：通过，5/5 gateway 校验测试。
- `git diff --check`：通过；仅有工作区既有的换行格式提示，无 diff 错误。

## 6. Unity Mock 测试覆盖

`D8A04AgentReplyMockTests` 覆盖：Mock LLM 回复必须先通过 AgentReply v1.1 边界，以及运行时授权快照之外的目标必须被拒绝。测试代码已加入 Unity EditMode 测试目录。

## 7. Unity 环境说明

已尝试使用项目 Unity `6000.0.80f1` 执行 EditMode 测试。主工程当前存在任务外既有编译错误：`game/Assets/Scripts/Gameplay/D7/D7RuntimeStatusHud.cs(73,26)` 的 `Fonts` 符号无法解析，Unity 未生成测试结果 XML，因此本报告不虚报 Unity 通过。该错误属于 D7 基线，不在 D8-A04 允许修改的模块范围内；恢复 D7 编译基线后应复跑：

```text
Unity.exe -batchmode -nographics -projectPath C:\Users\Administrator\Desktop\game\first01 -runTests -testPlatform editmode -testFilter D8.Tests.D8A04AgentReplyMockTests -testResults Temp/D8A04-editmode-results.xml -quit
```

## 8. 后续边界

D8-A04 只完成 AgentReply 提议边界、注册信息和校验链，不在 AgentCore、gateway 或 `Systems/` 中执行世界结果。后续具体执行者必须从注册表读取意图，并由对应强逻辑系统独立裁决和记录最终结果。
