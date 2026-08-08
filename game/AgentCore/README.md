# Whalebone AgentCore —— 纯 Agent 项目骨架

> 游戏暂名《鲸骨航线》。本骨架实现了"强逻辑 × 重交互"的最小闭环，纯 C#、零第三方依赖，
> 已在本机验证编译通过、6 项自测全部 PASS。

## 1. 骨架里有什么（对应架构分层）

| 文件 | 分层 | 职责 |
|---|---|---|
| `AgentModels.cs` | 强逻辑层 | `CrewStats` 船员数值模型（士气/忠诚/健康/疲劳/好感 + 叛变风险公式）；`PersonaDefinition` 人格模板；`AgentReply` 结构化输出；`MiniJson` 极简 JSON 解析 |
| `LLMClient.cs` | 推理层 | `ILLMClient` 统一接口；`OllamaClient`（本地，默认优先）；`DeepSeekClient`（云端兜底）；`LLMRouter` 自动降级；`TemplateFallback` 无模型兜底 |
| `AgentMemory.cs` | Agent 层 | 三层记忆：短期上下文(8 轮) / 会话摘要(10 轮触发) / 长期事件(重要性加权)；`IMemoryStore` 接口（骨架为内存实现，Unity 阶段接 SQLite） |
| `CrewAgent.cs` | Agent 层 | 船员聚合体：人格 + 数值 + 记忆 |
| `DialogueSession.cs` | 交互闭环 | 玩家输入 → 组装 prompt → LLM → 结构化校验(可重试) → 数值裁决 → 写记忆 |
| `MutinyMonitor.cs` | 强逻辑层 | 数值边界示例：叛变预警 + 分支锁（低好感锁"求情"，LLM 无权限绕过） |
| `Bootstrap.cs` | 本地演示 | CLI 交互入口（仅验证用，Unity 内由 UI 替代） |
| `AgentCoreTests.cs` | 测试 | 无网络自测：JSON 解析 / 叛变风险 / 好感夹取 / Prompt 组装 / 分支锁 |

**核心原则：Agent 只做"嘴和脑的提案者"，`CrewStats` 等强逻辑层做"手和法的裁决者"。**
LLM 输出必须过 JSON schema 解析（`AgentReply.FromJson`）才能影响数值——幻觉被锁死在文案层。

## 2. 本地运行（验证闭环）

```bash
cd AgentCore
dotnet run -- --mock                    # Mock 模型演示（无网络，推荐先跑这个）
dotnet run                              # 本地 Ollama（需已启动：ollama serve + ollama pull qwen2.5:3b）
dotnet run -- --deepseek=<api_key>      # 本地优先，DeepSeek 云端兜底
```

交互命令：直接输入说话；`stats` 查看状态快照与叛变风险；`exit` 退出。

## 3. 接入 Unity 项目（等你建好 Unity 工程后）

1. 把 `AgentCore` 下的 `.cs` 文件复制到 `Assets/Scripts/Agent/`（**csproj 和 Bootstrap.cs 不需要**——那是本地 CLI 用的；Unity 用自己的编译系统）；
2. `DialogueSession.SpeakAsync` 是 `async Task`，Unity 里用协程/`async void` 包装，或调用同步版 `Speak()`；
3. 对话 UI（输入框/气泡/打字机）接到 `Speak()` 的返回值 `AgentReply`（含 Speech/Emotion/Action）；
4. `Action` 字段对接游戏侧已注册的动作系统（P2A，需求文档阶段定义动作清单）；
5. 记忆持久化：把 `InMemoryStore` 换成 SQLite 实现（骨架已定义 `IMemoryStore` 接口，直接实现即可）；
6. 原 `MiniJson` 若在正式版想换 Newtonsoft / System.Text.Json，只改 `AgentReply.FromJson` 一处。

## 4. 占位素材工作流（先用免费素材推逻辑，后期替换）

**完全可行，这是行业标准的灰盒开发。** 纪律如下：

1. **只用免费可商用（CC0/CC-BY）占位**，别用版权不明的素材——占位阶段养成坏习惯，后期容易忘：
   - 模型/UI：**Kenney.nl**（游戏占位资产之王，CC0）、Synty 免费包、Unity Asset Store 免费包
   - 贴图：Poly Haven（CC0）、ambientCG
   - 音效：Freesound（筛选 CC0）、Kenney 音频包
   - 字体：思源黑体 / 阿里巴巴普惠体（免费商用）
2. **占位素材集中放一个目录**：`Assets/Art/Placeholder/`，替换时整目录换，不污染正式 `Art/` 目录；
3. **通过 Prefab/引用隔离**：场景只引用 Prefab，换素材时只改 Prefab 内部的模型/材质，**不重写任何逻辑代码**——这正是本骨架"逻辑与表现解耦"的意义；
4. **建替换追踪表**（Excel/Notion 一张表即可）：每个占位资产 → 待替换目标来源 → 状态，P3 前统一替换；
5. **动画/绑定成本最高**：占位角色一开始就用带骨架的（Synty 免费角色 + Mixamo 动画），避免后期重绑；
6. 逻辑正确性与素材无关——`AgentCoreTests` 的 6 项测试证明：**换素材永远不需要改逻辑**。

## 5. 给需求文档的接口约定（你要写的需求文档可以这样组织）

骨架已锁定的契约（需求文档围绕这些扩展即可）：

- **数值模型**：`CrewStats` 字段（Morale/Loyalty/Health/Fatigue/Affinity/Skills）+ 叛变风险公式参数 —— 需求文档定义各数值的初始值、上下限、成长规则；
- **结构化输出 Schema**：`{ speech, emotion, affinity_change(-10~10), action, action_args }` —— 需求文档定义 action 清单（游戏侧注册的动作名）；
- **Persona 约束模板**：`PersonaDefinition.Constraints` —— 需求文档定义船员人格库（角色、性格 Traits、背景、口吻）；
- **记忆**：`IMemoryStore` + 三层策略 —— 需求文档定义哪些事件该进长期记忆、重要性如何评定；
- **分支锁**：`MutinyMonitor.IsBranchLocked` —— 需求文档定义各对话分支的锁定数值阈值；
- **多人预留**：命名空间 `Whalebone.Agent` 全部与表现层解耦，多人阶段直接把 `DialogueSession` 挂到权威服务器即可（现阶段不做）。

## 6. 已知边界（骨架阶段刻意不做）

- 不做 UI/渲染/音频（那是 Unity 工程的事）；
- 不做 SQLite 持久化（已留 `IMemoryStore` 接口）；
- 不做多 Agent 并发调度（P1 阶段做：空间分区 + 只模拟玩家周围船员）；
- 不做语音（TTS/ASR 后置）。
