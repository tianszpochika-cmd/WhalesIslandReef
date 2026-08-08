# 开发阶段路线图（DEV_ROADMAP）

> 状态：**设计阶段完成，进入开发阶段**（2026-08-08）
> 阶段划分原则：按"**可体验的可见成果**"切分——每个阶段结束时项目能跑、能玩、能看到东西，而不是"代码写完了"。
> 与 MVP 分级（M0-M3）对齐：D 是执行视角，M 是范围视角。

---

## 审核结论（进入开发的前置条件）

| 项 | 状态 | 说明 |
|---|---|---|
| 设计文档链 | ✅ 完整 | 需求 v1.0 / 边界 / 数据模型 M0 / 商业模型 / 契约 / 规范 / 自动约束（AGENTS.md） |
| 工具链 | ✅ 就绪 | dotnet 9 / node 22 / Git / .gitignore / .editorconfig |
| Unity 环境 | ⬜ 需装 | **D1 前必装**：Unity Hub + Unity 6 LTS（6000.x，勾选 Windows Build Support）+ VS2022 或 VS Code |
| 海洋插件 | ⬜ D2 引入 | Crest Ocean（$90）——主菜单阶段不需要 |
| 本地 AI | ⬜ D2 引入 | Ollama + qwen2.5:3b——对话闭环才需要 |
| Steamworks | ⬜ D5 引入 | 发布阶段 |
| Ollama 包/账号注册/支付 | ⬜ 按需 | 服务端随 D2/D5 联动 |

**结论：D1 只需装 Unity + IDE 即可开工。** 其余依赖按阶段引入，不阻塞起步。

---

## 阶段总览（D1-D5）

| 阶段 | 名称 | 周期参考 | 验收标准（可体验） |
|---|---|---|---|
| **D1** | 工程底座 + 控制中台 | 2-3 周 | 项目启动 → 主菜单（新游戏/继续/设置/关于）→ 新游戏 → 进入初始环境 |
| **D2** | 航行与对话闭环 | 4-6 周 | 船能开（海洋/昼夜）→ 船员能对话（本地 AI）→ 好感影响状态 |
| **D3** | 可玩环 | 2-3 月 | 完整航程（整备→航行→登岛→交易→结算）可玩 |
| **D4** | 内容扩充 | 2-3 月 | 炮战/四国/动物/疾病/主线三章走通 |
| **D5** | 打磨上架 | 2-3 月 | EA 发售：12 岛/特质 50/本地化/性能/商店页 |

---

## D1 详细任务拆解（★ 当前阶段）

### D1.1 工程底座（第 1 周）
- [ ] Unity Hub 创建项目到 `game/`（Unity 6 LTS，URP 模板）
- [ ] git init + LFS + 应用 `.gitignore`（已有），首次提交
- [ ] 落地目录规范：`Assets/Scripts/{Core,Gameplay,Systems,Agent,Networking}` + Art/Audio/Prefabs/UI/Settings（SETUP.md 第 3 节）
- [ ] **AgentCore 接入**：`game/AgentCore/*.cs` 通过 asmdef 被 Unity 引用，`dotnet build` 与 Unity 编译双通过
- [ ] 空场景 + 输入系统（新 Input System）初始化

### D1.2 控制中台（主菜单）（第 1-2 周）
- [ ] 主菜单场景 `MainMenu`：背景 + 标题 + 菜单项
  - **新游戏**：进入创建流程（输入船长名 → 选择初始船型[小渔船] → 生成初始船员[老水手] → 生成初始港口）
  - **继续**：检测本地存档（SQLite）→ 有则加载，无则置灰/提示
  - **设置**：音量（主/音乐/音效）/ 画质档位 / 语言（中英切换）/ 键位说明
  - **关于**：版本号 / 开发信息 / 合规声明 / 素材授权入口
- [ ] 场景管理：`GameManager`（场景切换 + 游戏状态机：MainMenu → NewGame/Continue → InGame）
- [ ] UI 框架：UGUI 基础（面板切换、按钮、输入框、下拉框）

### D1.3 新游戏与存档骨架（第 2-3 周）
- [x] 存档系统接口 `SaveSystem`（SQLite：players/ships/crew_agents 表 + trait_defs/ship_type_defs 种子，见 DATA_MODELS 第 7 节）——✅ D1-P1B 完成（实现说明见 design/SAVE_SYSTEM.md）
- [x] 新游戏流程【逻辑层】：创建 Player（船长名/初始金钱 500/小渔船/老水手船员）→ 落库 → HasSave true ——✅ D1-P1B 完成（UI 创建流程待 P1A 接入）
- [x] 继续流程【逻辑层】：读存档 → 恢复 Player/Ship/CrewAgent → true/false ——✅ D1-P1B 完成（UI 入口待 P1A 接入）
- [ ] 初始环境场景 `InitialHarbor`：初始港口（低模占位） + 玩家的船（小渔船占位） + 老水手 NPC（站立/简单动画）——P1C
- [ ] 设置持久化（PlayerPrefs 或 Settings 表：音量/语言/画质）——P1A

### D1.4 验收清单（DoD）
- [ ] 项目冷启动（exe 或编辑器）→ 主菜单正常显示
- [ ] 新游戏：创建 → 存档写入 → 进入初始港口（能看到船 + 老水手）
- [ ] 继续：退出后重启 → 读档回到港口
- [ ] 设置/关于可用；中英语言切换生效
- [ ] AgentCore 在 Unity 内编译无冲突
- [ ] 全部通过 `docs/DEVELOPMENT_STANDARDS.md` 第 8 节 DoD

### D1 依赖
Unity 6 + IDE + Git（仅此三项）。

---

## D2-D5 概览（细节按阶段展开）

### D2 航行与对话闭环（M0 核心）
船 + 海洋（Crest 导入、浮力、航行控制、第三人称视角、昼夜循环）→ Agent 对话 UI 接入 AgentCore（本地 Ollama 优先）→ 好感变化 → 对话影响状态 → `dotnet run -- --mock` 自测迁移为游戏内对话。
**依赖**：Crest Ocean、Ollama、D1 存档骨架。

### D3 可玩环（M1）
贸易 10 商品 + 价格公式 + 3 岛（1 镇 1 村 1 荒岛）+ 登岛采集 + 特质 20 条 + 事件 8 + 天气 5 → 完整航程闭环（整备→航行→登岛→交易→结算）。
**依赖**：D2 航行。

### D4 内容扩充（M2）
船型 5 + 改装数值化 + 炮战（简化格斗）+ 四国声望（含私掠/禁运）+ 动物 8 + 疾病医疗 + 主线 3 章 + 航线日志。

### D5 打磨上架（M3）
12 岛 + 特质 50 + 捏脸基础 + 中英本地化 + 性能（1080p 60fps）+ Steamworks 集成（成就/云存档/商店页）+ Steam Direct 缴费（$100）+ 愿望单运营 + Next Fest → EA 发售。

---

## 原则

1. 每阶段结束 = 可运行 + 可展示 + 过 DoD；
2. 阶段内任务按 `DEVELOPMENT_STANDARDS` 工作流执行（单一写者、契约先行、留痕）；
3. 范围只增不减：D1-D5 之外的完整愿景在 IDEA_BANK + REQUIREMENTS 第 11 节（EA 后）；
4. 每阶段开工前：更新 `docs/CONTEXT.md` 进度指针，确认依赖就绪。
