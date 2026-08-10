# D2 本地运行时集成契约

> 版本：D2-M0 v0.1
> 归属：`Whalebone.Systems.D2`
> 说明：这是 Unity 客户端内部契约，不是跨端网络契约；跨端对话仍以 `contracts/` 为唯一事实源。

## 控制权

`D2ControlMode` 只有三种状态：

- `Character`：WASD/方向键驱动玩家人物。
- `Ship`：W/S 控制前进/减速，A/D 控制转向。
- `Locked`：对话、暂停或场景切换期间禁止人物和船输入。

控制权只能由运行时控制路由器切换，表现层不能直接修改船只数值。

## 航行输入

`SailingInput` 是每帧输入意图：

```text
Throttle: -1..1
Rudder:   -1..1
Brake:    bool
```

航行系统负责速度、转向和位置计算；Unity 只负责采集输入和应用表现。

## 航行快照

`SailingSnapshot` 保存船只的最小运行时状态：

```text
ShipId
PositionX / PositionZ
Heading
Speed / MaxSpeed
ControlMode
```

`PositionZ` 在 SQLite 的旧 `LocationRef.Y` 字段中兼容保存；旧存档缺少该值时使用 0。

## 昼夜快照

`DayNightSnapshot`：

```text
DayIndex: 非负整数
NormalizedTime: 0..1
Paused: bool
```

超出范围的时间在进入表现层前必须归一化。暂停由暂停控制器负责，时钟本身只接受明确的暂停状态。

## AgentReply 测试夹具

任务 02 不修改现有 `agent-reply.schema.json`。合法回复、缺字段回复、越界回复和非法情绪回复放在 `contracts/test-fixtures/agent-reply/`，任务 05 必须用它们验证重试和模板兜底。

## 兼容策略

- D1 旧存档可继续加载。
- 旧存档没有 D2 位置/朝向/时间字段时，回退到初始港口和默认白天时间。
- Crest、Ollama 和服务端不可用不改变上述本地类型。
