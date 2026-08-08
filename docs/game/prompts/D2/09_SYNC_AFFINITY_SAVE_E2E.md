# D2 同步任务 09：好感、状态反馈与存档闭环

你负责 D2 的数据闭环。前置条件：任务 08 已通过基础运行时手测。

## 目标

- 对话返回的合法 `affinity_change` 经过明确的 Crew/关系逻辑裁决后，更新老水手的好感。
- 好感变化产生一个 D2 可见但简单的状态反馈（例如对话情绪/称呼/提示），不能扩展成任务树。
- 新游戏后保存；对话后保存；退出或返回主菜单后点击 Continue，能恢复船、玩家位置、昼夜时间和船员好感。
- 失败时保持上一次有效存档，不写入半成品或非法 AgentReply。

## 负责范围与红线

- 只修改 D2_SCOPE 指定的关系/存档适配路径，避免重写 D1 SQLite。
- SaveSystem 仍是存档唯一写者；UI、LLM 和航行系统不得直接写 SQLite。
- 不把 affinity 变化交给 LLM 裁决，不让 Agent 修改金钱、货物、船体或奖励。
- 不引入网络代码到 `game/Assets/Scripts/Systems/`。

## 验证

- Mock 对话前后好感数值、UI反馈和存档内容一致。
- 重启编辑器后 Continue 能恢复；非法回复、Ollama 超时和写盘失败不会破坏旧档。
- 运行相关测试、`dotnet build game/AgentCore/AgentCore.csproj --nologo`，并完成 Unity 冷启动手测。
- 更新数据模型/设计文档、当日记忆日志和交接说明。
