# D2 航行与对话闭环范围冻结

> 版本：D2-M0 v0.1
> 状态：第一道同步门已完成，允许异步任务 03-07 按提示词并行；运行时集成仍需同步任务 08-10 验收。

## 1. 可体验目标

D2 只保证一条可反复测试的单船、单港口、单船员闭环：

```text
MainMenu 新游戏
  → InitialHarbor/SeaTrial
  → 人物能走动、相机能旋转和缩放
  → 玩家获得船只控制权，船能前进、减速、转向
  → 海面和昼夜变化可见
  → 接近老水手并完成一次对话
  → 回复通过 AgentReply 校验，合法好感提议影响老水手状态
  → 保存，返回主菜单，Continue 恢复状态
```

本阶段不追求完整开放世界，优先证明“能玩、能对话、能保存、失败不锁死”。

## 2. 场景与输入口径

| 行为 | 默认输入 | 规则 |
|---|---|---|
| 人物移动 | WASD/方向键 | 只在人物控制权下生效 |
| 第三人称视角 | 鼠标右键或中键拖动 | 视角旋转不改变强逻辑数值 |
| 镜头缩放 | 鼠标滚轮 | 有最小/最大距离，避免进入模型内部 |
| 船前进/减速 | W/S 或上下 | 输入转为航行意图，由航行系统裁决速度 |
| 船转向 | A/D 或左右 | 输入转为舵角意图，由航行系统裁决朝向 |
| 对话 | 接近老水手后按 E/交互键 | 对话期间锁定移动和航行输入 |
| 暂停 | ESC | 暂停时间、航行、昼夜；菜单可继续/返回/退出 |

如果项目现有 Input Actions 的按键名称不同，适配器可以映射，但不能改变上述玩家可理解的行为。

## 3. 最小领域接口（概念契约）

实际 C# 命名由任务 01 后续实现决定，但语义不得改变。

### 3.1 航行

```text
SailingInput
  throttle: -1..1
  rudder: -1..1
  brake: bool

SailingState
  shipId
  position / heading
  speed / maxSpeed
  hull / hullMax
  supplies
  controlMode: Character | Ship | Locked
```

输入只是意图；速度、方向、耐久和补给变化只能由航行强逻辑系统写入。Unity `MonoBehaviour`、Crest、输入设备和相机不进入这个核心接口。

### 3.2 昼夜

```text
DayNightState
  dayIndex
  normalizedTime: 0..1
  paused
```

时间由一个时钟单一写入。表现层订阅时间，暂停系统负责停止推进；SaveSystem 保存并恢复最小时间快照。

### 3.3 对话

请求至少包含 `crew_id`、玩家输入和当前允许展示的人格快照；回复必须符合 `contracts/agent-reply.schema.json`：

```text
AgentReply
  speech
  emotion
  affinity_change (-10..10)
  action (D2 只允许 none)
  action_args
```

本地 Ollama 是首选；不可用、超时、返回非法 JSON 或 Schema 校验失败时最多重试一次，然后返回模板回复。非法原始回复永远不能进入逻辑层。

### 3.4 好感

Agent 只能提出 `affinity_change`。Crew/关系逻辑系统负责范围裁决（当前 `Affinity` 为 -100..100），SaveSystem 负责落库。UI 只显示结果，不直接写数值。

## 4. 数据所有权

| 数据 | 唯一写者 | 只读消费者 |
|---|---|---|
| 航行速度、方向、船体和补给 | SailingSystem | Unity 船表现、HUD、SaveSystem 快照 |
| 昼夜时间 | DayNightClock | 光照/天空/海面、SaveSystem 快照 |
| LLM 原始文本 | DialogueSession/LLMClient 边界 | 校验器（不进入游戏状态） |
| 合法 AgentReply | DialogueSession | Dialogue UI、关系逻辑 |
| 船员 Affinity/Morale/Loyalty 等 | Crew/关系强逻辑 | 对话人格、UI、SaveSystem |
| SQLite 存档 | SaveSystem | MainMenu Continue、运行时恢复 |
| 控制权与暂停状态 | GameManager/暂停控制器 | 输入适配器、UI、航行/昼夜 |

## 5. 故障与降级

- Crest 未安装：使用低成本海面占位适配器，允许继续航行。
- Ollama 未运行：使用 Mock/模板回复，UI 明确显示“本地 AI 不可用，使用模板回复”，不能无限加载。
- AgentReply 非法：重试一次，仍失败则模板回复；不修改好感。
- Prefab/材质缺失：使用稳定的占位模型/材质并一次性报错，不能每帧刷日志。
- 存档写入失败：保留上一次有效存档并向 UI 返回失败，不覆盖旧档。
- 场景切换：恢复 `Time.timeScale=1`，避免 MainMenu/InitialHarbor 互相携带暂停状态或重复 EventSystem。

## 6. D2 DoD

1. 冷启动后新游戏能进入可见的港口/海上预览场景。
2. 人物可移动，第三人称相机可旋转/缩放。
3. 一艘船可前进、减速和转向，输入与逻辑分离。
4. 海面与昼夜变化可见，暂停时停止。
5. 老水手对话能在 Mock 下完成，Ollama 失败不阻塞。
6. 回复经过 Schema/边界校验，好感变化可见且由逻辑层裁决。
7. 保存、返回主菜单、Continue 和编辑器重启后的读档恢复成立。
8. AgentCore 独立构建通过；Unity Console 没有 D2 阻塞错误、每帧错误或重复对象。

贸易、完整天气、战斗、多岛、任务树、联网联机和 Steam 不计入本次验收。
