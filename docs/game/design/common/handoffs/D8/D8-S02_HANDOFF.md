# D8-S02 任务交接报告

任务：D8-S02 同步集成与 D8 总体验收
日期：2026-08-10
结论：D8 通过，允许进入 D9

## 交付摘要

- 合并并审计 D8-A01～D8-A06 的实体、事件、AgentReply、P2A、存档迁移、内容 ID 和统一配置变更。
- 完成 game/server/web 的 Schema 漂移检查；server、纯 C# AgentCore、Unity 镜像和 contracts 使用同一 AgentReply v1.1 与 60 Action 集合。
- 修正 `SaveSystem` 对 D8-A06 健康/疲劳配置返回值到旧整数存档字段的显式投影，避免 Unity 生成工程的 float→int 编译错误。
- D7 SQLite 读取与 D7→v3 迁移测试通过；迁移仍保留 D7 三张兼容表和 `user_version=1`。
- Unity 同版本隔离 Mock 测试 21/21 通过；主项目已有 Unity 实例锁定，因此未强制关闭用户进程。

## 契约版本

| 契约 | 版本/约束 |
|---|---|
| `contracts/agent-reply.schema.json` | `agent_reply.v1.1` |
| `contracts/p2a-action-registry.json` | `registry_version=p2a.v1`，来源 `version=d8-a04` |
| `contracts/world-event.schema.json` | `schema_version=1` |
| D8 entity DTO | `schema_version=1`，16 个实体 |
| D8 save migration | `save_version=3`，来源 SQLite `user_version=1` |

## 60 个 P2A Action 清单

完整参数 Schema、权限、前置、失败码、执行者及 actor/target 类型以 `docs/game/design/common/D8_P2A_ACTION_REGISTRY.md` 和 `contracts/p2a-action-registry.json` 为准。注册名清单如下：

- navigation：`trim_sail`、`steer`、`set_course`、`drop_anchor`、`weigh_anchor`、`stop_ship`、`set_watch`、`report`、`heave_to`、`change_speed`
- shipboard：`fire_cannon`、`repair`、`bilge_pump`、`ration`、`hold_below`、`reward_crew`、`launch_boat`、`board`、`extinguish_fire`、`secure_cargo`
- crew：`assemble_crew`、`gather`、`scout`、`treat`、`trade`、`cook_meal`、`distribute_rations`、`assign_duty`、`call_meeting`、`request_leave`
- negotiation：`greet_contact`、`ask_information`、`propose_trade`、`propose_alliance`、`request_passage`、`request_mediation`、`offer_apology`、`negotiate_terms`、`accept_offer`、`end_talk`
- exploration：`survey_coast`、`inspect_wreck`、`map_landmark`、`observe_animal`、`rescue_animal`、`release_animal`、`fish`、`collect_water`、`collect_firewood`、`enter_cave`
- economy：`buy_supplies`、`sell_cargo`、`visit_shipyard`、`request_upgrade`、`hire_crew`、`dismiss_crew`、`store_cargo`、`retrieve_cargo`、`submit_quest`、`tend_garden`

每个 Action 的注册项均已验证包含：

1. 注册名与分类；
2. `parameter_schema`（必填字段、类型、枚举、正则或数值范围）；
3. `permissions`；
4. `prerequisites`；
5. `failure_codes`；
6. `executor`；
7. `allowed_actor_kinds` 与 `allowed_target_kinds`。

## 拒绝逻辑

- Schema 层拒绝未知字段、缺失字段、错误版本、错误枚举和错误字段类型。
- AgentCore/server gateway 拒绝未知 Action：`unknown_action`。
- Action 参数按注册表执行精确字段、必填项、枚举、正则和最小/最大值校验；失败码包含 `invalid_action_parameters` 或 `missing_action_parameter`。
- 运行时授权集合校验 actor/target；未授权 actor 为 `unauthorized_actor`，未授权 target 为 `unauthorized_target`，类型不符为 `invalid_target_kind`。
- 权限和前置分别校验为 `permission_denied`、`missing_prerequisite`。
- AgentReply/Action 参数一旦携带最终 money、price、damage、reward、weather 或 relation 数值结果，拒绝为 `forbidden_result_field`。
- `refusal_reason` 非 `none` 时携带 Action，拒绝为 `refusal_with_action`。
- 关系提议目标也必须在当前授权集合内；越权目标不会由 Agent 或 server gateway 自动扩权。

## 跨端同步文件

契约事实源：

- `contracts/agent-reply.schema.json`
- `contracts/p2a-action-registry.json`
- `contracts/world-event.schema.json`
- `contracts/test-fixtures/agent-reply/*.json`

纯 C# / Unity：

- `game/AgentCore/AgentModels.cs`
- `game/AgentCore/AgentReplyValidator.cs`
- `game/AgentCore/P2AActionRegistry.cs`
- `game/AgentCore/V3EntityModels.cs`
- `game/Assets/Scripts/Agent/AgentCore/AgentModels.cs`
- `game/Assets/Scripts/Agent/AgentCore/AgentReplyValidator.cs`
- `game/Assets/Scripts/Agent/AgentCore/P2AActionRegistry.cs`
- `game/Assets/Scripts/Systems/Save/SaveMigration.cs`
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- `game/Assets/Scripts/Systems/Event/EventContracts.cs`
- `game/Assets/Scripts/Systems/Event/EventLog.cs`

server /测试：

- `server/src/modules/gateway/gateway.service.ts`
- `server/test/gateway.service.test.ts`
- `contracts/tests/agent_reply_schema_test.py`
- `tools/content_registry_check.py`
- `tools/tests/test_content_registry_check.py`

必要设计文档：

- `docs/game/design/common/D8_ENTITY_DTO_CATALOG.md`
- `docs/game/design/common/D8_EVENT_PACKAGE.md`
- `docs/game/design/common/D8_P2A_ACTION_REGISTRY.md`
- `docs/game/design/common/D8_SAVE_MIGRATION.md`
- `docs/game/design/common/D8_NUMERIC_ENUM_CONFIG.md`
- `docs/game/design/common/D8_ACCEPTANCE_REPORT.md`

## 测试命令和结果

```text
dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal       PASS (0 warning / 0 error)
npm run build  [server/]                                                  PASS
python contracts/tests/agent_reply_schema_test.py                          PASS (3/3)
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --self-test PASS
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --config-check PASS (5)
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry PASS (15 entries / 13 prefixes)
python -m unittest discover -s tools/tests -p "test_content_registry_check.py" -v PASS (7/7)
npm test  [server/]                                                          PASS (5/5)
server gateway runtime registry count                                          PASS (60)
dotnet restore game/Save.Tests.csproj                                        PASS
dotnet build game/Save.Tests.csproj --no-restore -v:minimal                 PASS
Unity 6000.0.80f1 EditMode Mock: -runTests -testPlatform editmode -testFilter D8 PASS (21/21, 0 failed, 0 skipped)
dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal             PASS
dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal           PASS
git diff --check                                                             PASS
Systems executable network/LLM scan                                             PASS
AgentCore Unity/third-party dependency scan                                    PASS
canonical writer ownership audit                                               PASS
```

Unity Mock 21 项明细：D8-A05 存档迁移 10 项、D8 EventPackage 7 项、D8 ContentRegistry 2 项、D8-A04 AgentReply Mock 2 项。

注意：生成的 Unity `.csproj` 没有 NUnit adapter，`dotnet test` 不作为 Unity 测试证据；Unity Test Runner 产生的 `test-run` XML 为本次 Unity 验收证据。

## 后续交接

- D8 状态：通过。
- D9 状态：允许开始。
- 主 Unity 项目回归：已有用户 Unity 实例占用项目，后续可在释放锁后补跑完整主项目 EditMode；不要结束或强制关闭用户现有进程作为同步任务步骤。
- D9 不得修改本报告确认的 AgentReply 结果禁止项、P2A 权限/前置/执行者边界或 WorldEvent 去重键；若需改变契约，必须重新走契约先行与跨端同步。

