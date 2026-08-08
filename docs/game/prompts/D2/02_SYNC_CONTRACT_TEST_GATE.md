# D2 同步任务 02：契约与测试门

你负责 D2 的本地运行时契约和测试入口。任务 01 的范围冻结已经完成；本任务完成后，03-10 必须只使用这里定义的类型，不再各自发明接口。

## 负责范围

- `game/Assets/Scripts/Systems/D2/`：纯 C# D2 运行时类型。
- `game/Assets/Tests/EditMode/`：D2 EditMode 测试程序集和契约测试。
- `contracts/test-fixtures/agent-reply/`：AgentReply 有效/非法样例。
- `docs/game/design/D2_INTEGRATION_CONTRACT.md`。

## 冻结类型

- `D2ControlMode`：`Character`、`Ship`、`Locked`。
- `SailingInput`：`Throttle`、`Rudder`、`Brake`；Throttle/Rudder 必须归一化到 `-1..1`。
- `SailingSnapshot`：船 ID、X/Z 位置、Heading、Speed、MaxSpeed、ControlMode。
- `DayNightSnapshot`：DayIndex、NormalizedTime、Paused。
- `D2RuntimeSnapshot`：SailingSnapshot + DayNightSnapshot 的组合快照。

这些类型不得引用 Unity、Crest、InputSystem、LLM 或网络库。现有 `contracts/agent-reply.schema.json` 不修改；D2 只新增测试夹具，AgentReply 的严格解析由任务 05 完成。

## 验收

- 新增 EditMode 测试程序集能引用 `Whalebone.Systems` 和 `Whalebone.Agent.Runtime`。
- 测试覆盖输入夹取、快照深拷贝、昼夜时间归一化和运行时快照组合。
- `git diff --check` 通过。
- 不改 MainMenu、SaveSystem、场景或服务端行为。
- 更新当日记忆日志和 D2 任务总览。

## 回报格式

返回新增接口、测试清单、运行方式、未解决依赖和后续任务必须遵守的字段语义。
