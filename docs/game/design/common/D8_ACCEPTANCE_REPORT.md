# D8 集成验收报告

任务：D8-S02
日期：2026-08-10
项目：鲸骨航线
前置：D8-S01、D8-A01～D8-A06 交接内容已读取并完成同步审计。

## 最终结论

**D8 通过，允许进入 D9。**

本次同步验收覆盖契约漂移、AgentCore、server、存档迁移、WorldEvent 去重、内容 ID、统一配置、P2A 拒绝边界、Unity Mock 和项目铁律检查。代码编译与规定测试均通过；没有删除 D7 功能，也没有新增单端私有协议或领域裁决逻辑。

主 Unity 项目已有 Unity 实例占用，主项目 batchmode 未能取得 EditMode XML。为完成本任务要求的 Unity Mock 执行验证，使用同版本 Unity `6000.0.80f1` 的隔离最小项目运行了相同 D8 相关程序集与测试，结果 **21/21 通过、0 失败、0 跳过**；主项目生成的 `Assembly-CSharp` 与 `Whalebone.Systems` 也已独立编译通过。主项目锁定是后续完整客户端回归的操作提醒，不构成 D8 验收阻塞。

## 1. 契约与实体合并结果

| 契约/模型 | 当前版本 | 结果 |
|---|---:|---|
| `AgentReply` | `agent_reply.v1.1` | 对话、情绪、关系提议、谈判语气、拒绝原因、P2A 意图字段统一；禁止最终 money/price/damage/reward/weather/relation 数值结果 |
| P2A Action Registry | `p2a.v1`，来源版本 `d8-a04` | 60 个动作；每个动作均具备注册名、内联参数 Schema、权限、前置、失败码、执行者、actor/target 类型边界 |
| `WorldEvent` | `schema_version=1` | 事实事件、`event_id + save_revision` 去重键、owner_system 裁决边界统一 |
| D8 实体 DTO | `schema_version=1` | 16 个实体、共同字段、所有权和 D7 兼容字段保持一致 |
| D8 存档 | `save_version=3`；SQLite `user_version=1` 保持兼容 | D7 表不删除、不改名、不重写；v3 投影写入新增迁移表 |
| 数值/枚举配置 | D8-A06 统一入口 | AgentCore 与 Unity 复用同一配置源码，领域系统仍拥有最终写权限 |

已完成的跨端漂移审计结果：

- `contracts/` 中全部 JSON 契约可解析。
- `agent-reply.schema.json` 顶层 9 个字段与 server gateway、纯 C# AgentCore、Unity AgentCore 一致。
- 合同 Action、纯 C# Action Registry、Unity Action Registry 均为同一组 60 个注册名。
- web 未发现独立 AgentReply DTO 或私有 AgentReply 协议，因此没有额外 Schema 漂移。
- 未发现为同步任务复制领域逻辑；server 只做契约与权限边界校验，最终结果仍交给 executor 所属强逻辑系统。

本轮同步修正：`game/Assets/Scripts/Systems/Save/SaveSystem.cs` 将 D8-A06 健康值配置返回的浮点边界显式投影为已有整数存档字段，并保留 D7 读取兼容；该修正不改变领域所有权。

## 2. P2A Action 清单

完整注册信息写入 `D8_P2A_ACTION_REGISTRY.md`，以下为 60 个注册名的完整清单：

| 分类 | Action 注册名 |
|---|---|
| navigation | `trim_sail`, `steer`, `set_course`, `drop_anchor`, `weigh_anchor`, `stop_ship`, `set_watch`, `report`, `heave_to`, `change_speed` |
| shipboard | `fire_cannon`, `repair`, `bilge_pump`, `ration`, `hold_below`, `reward_crew`, `launch_boat`, `board`, `extinguish_fire`, `secure_cargo` |
| crew | `assemble_crew`, `gather`, `scout`, `treat`, `trade`, `cook_meal`, `distribute_rations`, `assign_duty`, `call_meeting`, `request_leave` |
| negotiation | `greet_contact`, `ask_information`, `propose_trade`, `propose_alliance`, `request_passage`, `request_mediation`, `offer_apology`, `negotiate_terms`, `accept_offer`, `end_talk` |
| exploration | `survey_coast`, `inspect_wreck`, `map_landmark`, `observe_animal`, `rescue_animal`, `release_animal`, `fish`, `collect_water`, `collect_firewood`, `enter_cave` |
| economy | `buy_supplies`, `sell_cargo`, `visit_shipyard`, `request_upgrade`, `hire_crew`, `dismiss_crew`, `store_cargo`, `retrieve_cargo`, `submit_quest`, `tend_garden` |

## 3. 拒绝逻辑验收

AgentReply 先通过 Schema/精确字段检查，再通过 AgentCore 与 server gateway 的运行时边界检查：

- 非法字段：拒绝未知顶层字段、未知嵌套字段、缺失必填字段、错误 Schema 版本和错误枚举。
- 超范围数值：拒绝关系变化越界、对白长度越界、Action 整数参数越界（例如 `hold_below.duration_hours` 不在 1～72）；D8-A06 配置检查拒绝非法范围和未知枚举。
- 未知动作：不在 60 个注册表中的 Action 返回 `unknown_action`。
- 未授权对象：actor 不在运行时授权集合返回 `unauthorized_actor`；target 不在授权集合或类型不匹配返回 `unauthorized_target`/`invalid_target_kind`。
- 权限与前置：缺少注册表权限返回 `permission_denied`；未满足前置返回 `missing_prerequisite`。
- 越权目标：关系提议和动作目标均必须属于当前授权范围；不接受 LLM 自行扩展目标范围。
- 最终结果越权：Action 参数或 AgentReply 中出现 money、price、damage、reward、weather 或 relation 数值结果返回 `forbidden_result_field`；AgentReply 只保留意图，最终结果由所属 executor 裁决。
- 拒绝原因一致性：`refusal_reason != none` 时不得同时携带可执行 Action，返回 `refusal_with_action`。

## 4. 数据所有权与铁律检查

| 数据 | 唯一领域写者/裁决者 | 本次检查 |
|---|---|---|
| 船体、航行、锚、风帆 | ShipSystem / 其控制器 | Agent 只提议；Save 只读写存档投影 |
| 现金、市场库存、交易 | TradeEconomySystem | gateway 不写 money/price |
| 货物与容量 | D3InventorySystem / InventoryCargoSystem | Action 只携带 cargo/hold 意图 |
| 关系 Affinity | CrewRelationSystem | Action/AgentReply 不直接改关系数值 |
| 船员健康/疲劳 | D4SurvivalSystem | SaveSystem 仅迁移/持久化 |
| 动物健康 | D4EcologySystem | SaveSystem 仅迁移/持久化 |
| 通缉/法律状态 | D4FactionSystem / 法律系统 | Agent 仅提出谈判/关系姿态 |
| WorldEvent/EventLog 事实 | EventLog 追加与去重门 | 事件 owner_system 负责结果，重复事件不重复结算 |
| save_metadata/v3_entities | SaveMigration | 仅迁移层拥有迁移表写权 |
| Content Registry / NumericConfiguration | 内容读取器 / 配置读取器 | 只读定义与校验，不取得领域状态写权 |

红线检查通过：`game/Assets/Scripts/Systems/` 可执行代码未发现网络或 LLM 调用；`game/AgentCore/` 无 Unity 或第三方 PackageReference/ProjectReference，也没有活动的 Unity/第三方 using。AgentCore 中的 `System.Net.Http` 仅属于已有标准库 LLM 客户端，不是第三方依赖，且不进入 Systems。

## 5. 测试与结果

| 顺序 | 命令/检查 | 结果 |
|---:|---|---|
| 1 | D8 文档、交接报告、contracts 与现有实体变更审计 | 通过 |
| 2 | game/server/web Schema 漂移检查 | 通过：9 个 AgentReply 顶层字段、60 个 Action、纯 C#/Unity/server 一致；web 无独立 AgentReply DTO |
| 3 | `dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal` | 通过，0 warning / 0 error |
| 4 | `npm run build`（`server/`） | 通过 |
| 5 | `python contracts/tests/agent_reply_schema_test.py` | 通过，3/3 |
| 6 | `dotnet restore game/Save.Tests.csproj`；`dotnet build game/Save.Tests.csproj --no-restore -v:minimal` | 通过，迁移/Systems 程序集 0 warning / 0 error |
| 6 | 同版本 Unity EditMode Mock，`-runTests -testPlatform editmode -testFilter D8` | 通过，21/21，0 failed，0 skipped；D8-A05 10、WorldEvent 7、Content 2、AgentReply Mock 2 |
| 7 | `dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --self-test` | 通过：v3 16 实体序列化/回环、D8-A04、WorldEvent、存档元数据、D8-A06 配置测试均通过 |
| 7 | `dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --config-check` | 通过，5 项 |
| 7 | `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry` | 通过，15 entries / 13 prefixes |
| 7 | `python -m unittest discover -s tools/tests -p "test_content_registry_check.py" -v` | 通过，7/7 |
| 7 | `npm test`（`server/`） | 通过，5/5：v1.1、非法参数/结果、未知动作、权限/前置、actor/target 边界 |
| 7 | server gateway registry runtime count | 通过，60 |
| 8 | `git diff --check` | 通过，无 whitespace error |
| 9 | Systems 网络/LLM 可执行代码扫描 | 通过 |
| 10 | AgentCore Unity/第三方依赖扫描 | 通过 |
| 11 | canonical writer ownership audit | 通过；迁移、读取、旧版兼容 projection 未误判为领域写者 |

补充说明：对生成的 Unity 工程执行了 `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal` 和 `dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal`，均为 0 warning / 0 error。生成工程的 `dotnet test` 没有 NUnit adapter，不作为测试通过证据；Unity Test Runner 的 21/21 XML 才是 Unity 测试证据。

## 6. D9 放行条件

D8-S02 的门禁已满足，允许进入 D9。后续完整客户端回归仍应在主 Unity 实例释放项目锁后补跑一次主项目 EditMode 测试；该操作不改变本次 D8 结论，也不得用它绕过契约、权限或领域裁决边界。

