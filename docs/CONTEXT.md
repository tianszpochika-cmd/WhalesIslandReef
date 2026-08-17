# 项目上下文（CONTEXT）— 每个开发任务开工前必读

> 给所有开发者（人类 + AI agent）的 5 分钟对齐页。**先读这个，再动手。**

## 项目是什么

1675 年加勒比海的开放世界航海沙盒 RPG（纯 Agent 游戏）：
**AI 船员社会模拟 × 航海贸易 × 真实主义**。Steam 目标、个人开发、三端一体。

## 必读文档（按序，缺一不可）

1. `docs/game/REQUIREMENTS.md` — 统一需求与玩法目录
2. `docs/game/DESIGN_PILLARS.md` — 设计支柱和时代边界
3. `docs/game/design/DESIGN_BOUNDARIES.md` — 系统所有权和 Agent 边界
4. `docs/game/design/DATA_MODELS.md` — 核心实体、事件和存档
5. `docs/game/DEV_ROADMAP.md` — D7 后开发工作包
6. `docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md`、`docs/game/AGENT_DEVELOPMENT_TASKS.md` 与对应章节文件 — 当前本地画面/可玩目标、执行顺序和可复制提示词
7. `docs/ARCHITECTURE.md`、`docs/DEVELOPMENT_STANDARDS.md` — 工程约束

## 项目速览

| 端 | 技术 | 位置 |
|---|---|---|
| 游戏客户端 | Unity 6 + C#（AgentCore 纯 C# 核心库） | `game/` |
| AI 服务端 | NestJS + TypeScript（账号/token/LLM 网关） | `server/` |
| 官网 | Next.js（待建） | `web/` |
| 契约 | JSON Schema + OpenAPI | `contracts/` |
| 文档 | 需求/玩法/设计/路线/规范/历史证据 | `docs/` |

## 铁律 5 条（违反即不合格）

1. **契约先行**：改 `contracts/` 必须三端同步，禁止单端私有协议；
2. **强逻辑红线**：`Systems/` 禁止 LLM/网络代码；数值修改只在所属系统类内；
3. **数据所有权唯一**：每份数据只有一个系统能写（裁决权表）；
4. **AgentCore 纯 C#**：零 Unity 依赖、零第三方依赖，可独立编译测试；
5. **产出必须留痕**：代码 + 文档同步 + 写当日记忆日志（.workbuddy/memory/）。

## 当前进度指针

- ✅ 统一需求 v3.0、统一设计 v3.0 和 D7 后路线图已建立；历史版本移入归档
- ✅ 设计边界/数据模型/玩法总目录已统一；服务器骨架可运行；契约基础落地
- ✅ D1 工程底座与 InitialHarbor 已通过用户验收
- ✅ D2 航行与对话闭环已完成：Unity 6 编译通过，EditMode 23/23 通过，AgentCore/server 检查通过
- ✅ D3 贸易与完整航程可玩环已完成：10 商品、3 地点、5 天气、8 事件；Unity 40/40 测试与运行时冒烟通过，D2 接口保持稳定
- ✅ D4 内容扩充已完成：5 船型/改装、炮战、7 势力、15 动物、5 疾病医疗、三章主线和航线日志；Unity 55/55 测试与运行时冒烟通过
- ✅ D5 打磨上架技术阶段已完成：12 岛、50 特质、8 个基础角色预设、中英本地化、1080p/60 目标监控、Steam 离线 Bridge 与发布清单；Unity 63/63 测试与 D5 运行时冒烟通过
- ✅ D6 首次航程体验层、D6.1 物理落地补强、D6.2 角色姿态基线、D6.3 动作/视角增强与 D6.4 相机遮挡保护已完成技术与用户手动验收：角色重力/落地、港口碰撞、上下船碰撞状态、平滑动作、三种视角、相机遮挡收缩、三轴动画姿态、港口品质、交互稳定化、新手引导；Unity D5 运行时冒烟通过
- ✅ D7 首航体验质量版开发与自动化验收完成：控制状态统一、人物动作/落地质量、舵位上下船、首航保存恢复、材质/音频/回归测试；人工复测作为交接证据保留

## 2026-08-12 当前验收快照

- ✅ D15-S02 内容 QA 已通过：8 批次、2,700 个批次条目、Content Registry 2,812 条；统一 QA `errors=0 warnings=0`。
- ⏳ D16 开发阶段：C01—C08、C12 已过；C09/C10/C11 因真机实测缺证据保持 `GATE_BLOCKED`。**用户裁定真机四门暂不列入开发必做**，见 `docs/game/design/common/D16_EXTERNAL_HARDWARE_DEFERRED.md`。禁止把 BLOCKED 改成 PASS。A04 审查与切片 4 项资源已在 C07/C08 处理。
- ⏳ D16-A01—A06 目标设备、断电、真机 a11y、目标机发布/隐私仍缺真实 envelope；S01 正式长线与章节放行推迟到预发布波次。
- ✅ D17 本机垂直切片基线已完成：D17-S02 本机十循环 **1/1 PASS**，Unity PlayMode **60/60 PASS**，工具测试 **77/77 PASS**；这不是正式 D17-S01/S02，也不绕过 D16 外部门禁。

- ✅ 项目公开产品名已统一为 `WhalesIslandReef`，并完成旧 `Whalebone` 存档目录的保留式迁移保护；这项本地发布技术缺口已用 Unity 全量回归验证。
- ✅ D16 外部门禁已有保守证据入口：`tools/d16_external_gate_audit.py`；A01/A02/A03/A06 真实 envelope 仍缺失，审计保持 BLOCKED 是开发阶段预期状态，不以本机回归替代。

## 当前开发入口

当前有两条不能混淆的轨道：

- **本地开发轨**：用户于 2026-08-13 明确授权继续画面、UI、真实免费资源和完整可玩体验，入口为 `docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md` 与 `AGENT_DEVELOPMENT_TASKS.md` 第 10—16 章；从 `D18-S01` 开始。
- **正式发布门禁轨**：剩余工作按 [第 9B 章](game/AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATES.md) 分派。当前有效编号是 `D16-C01`—`C12`、`D17-C01`—`C04`（2026-08-17 已合并；`C01`—`C05` 保留）。D17-C01 必须等 `D16-C11 GATE_PASS`。旧 A/R/S 与旧 `D16-C13`—`C31` / `D17-C05`—`C16` 禁止再派。

D18—D24 可以推进本机资源、场景和玩法集成，但不能把 D16 未完成的目标设备、跨平台、真实断电、人工审查、隐私或 Steam 证据写成通过。

### 2026-08-12 最新本机快照

- 工具测试已纳入物理设备预检后更新为 **84/84 PASS**；Unity D17 本机连续基线仍为 **60/60 PASS**。
- Windows PnP 预检识别到 Xbox Wireless Controller、Bluetooth LE XINPUT compatible input device、3840x2160 显示配置和 RTX 4070 Ti；预检仍明确为 `IN_PROGRESS/PENDING_MANUAL`，不等于真机验收。
- XInput 只读探针当前为 `NO_ACTIVE_XINPUT_USER`：API 可用但用户 0—3 均返回 `1167`，设备存在不等于活动 XInput 输入；后续真机验收必须实际确认手柄唤醒、按钮/摇杆输入和 Unity 枚举。
- D16-A01—A06 真实外部门禁、D16-S01/S02、正式 D17 和 D18+ 状态不变。

### 2026-08-13 当前最终本机快照

- 工具全量回归为 **88/88 PASS**；A04/A05/A06 聚焦回归为 **28/28 PASS**，A05 资源包预检为 **4/4 PASS**，D15-S02 QA 为 8 batches / 24 samples / 0 errors / 0 warnings。
- A04 当前为 3,139 entries、结构错误 0、授权记录 2,918/2,918、Pending 267；A05 为 2,918 visual、654 integrated、2,264 placeholder、声明路径断链 0；A06 本机/安装器技术检查 19/19 PASS。
- D16 外部门禁仍为 **BLOCKED**：A01—A06 六个真实 evidence envelope 缺失，发布清单保留 7 条外部/政策阻断；D16-S01/S02、正式 D17 和 D18+ 不提前启动。
- 当前工作区仍有未提交变更；本快照不代表已提交或推送到 GitHub。

### 2026-08-13 A04 逐条复核包快照

- 新增 `tools/d16_a04_review_packet.py` 与 `docs/game/design/common/D16_A04_REVIEW_PACKET_CURRENT.md`，覆盖当前 267 条 Pending、12 批；状态为 `HUMAN_DECISION_REQUIRED`，引用缺口 0，267/267 条已有本地文件型复核入口。
- 专项测试 **4/4 PASS**；填写包默认全部保持 `Pending`，不会自动批准或修改 Registry。
- A04 人工决定仍未完成，D16-S01/S02、正式 D17 和 D18+ 继续等待真实责任审核与其余外部门禁。

### 2026-08-13 A04 复核包最终回归快照

- 工具全量回归更新为 **96/96 PASS**（新增 A04 复核包专项 4/4、D16-S01 启动预检专项 4/4）；`git diff --check` 通过。
- 权威状态未改变：A04 3,139 entries / 授权 2,918 / Pending 267 / 结构错误 0；A05 2,918 visual / integrated 654 / placeholder 2,264 / 声明路径断链 0；A06 本机技术准备 19/19 PASS。
- 外部门禁仍 `BLOCKED`，A01—A06 六个真实 evidence envelope 缺失且 Steam 未授权；D16-S01/S02、正式 D17 和 D18+ 继续未完成。

## 开工自检

- [ ] 我读了需求、支柱、边界、数据、路线、Agent 任务索引/对应章节提示词和工程规范？
- [ ] 我知道我的任务只动哪个模块（单一写者）？
- [ ] 我知道产出要过哪个验收清单（DoD）？

### 2026-08-13 D16-A05 第一批候选资源快照

- 新增 Kenney CC0 候选包：`docs/game/design/common/D16_A05_KENNEY_CANDIDATE_PACKET_01.md`；机器清单为 `tools/results/d16_a05_candidate_packet_01_20260813.json`。
- 候选资源放在 `game/Assets/Art/Candidates/Kenney/`，当前状态为 `CANDIDATE_PENDING_REVIEW`，人工决定仍为 `Pending`，没有 Registry 或 `asset_state` 写入。
- A05 权威统计未被候选文件改变：2,918 visual、data_only 125、integrated 654、placeholder 2,264；声明路径断链 0。候选包专项 **2/2 PASS**，工具全量回归更新为 **98/98 PASS**。
- 这只补齐第一批真实来源候选和许可证留痕，不等于最终资源接入；A04 Pending 267、六项外部门禁缺失、D16-S01/S02 及正式 D17/D18+ 状态均不变。
- Unity 导入级复核 `tools/results/d16_a05_candidate_import_current_20260813.json` 为 7/7 PASS；候选包与导入证据专项合计 4/4 PASS。另修正 D10 行为状态源码 `.meta`，Unity 批处理脚本编译已通过；这不改变 A05 最终资源门禁。

### 2026-08-13 最终当前回归

- 工具全量回归更新为 **100/100 PASS**；候选包/Unity 导入证据专项 **4/4 PASS**；`git diff --check` 通过。
- A04 仍为 3,139 entries、Pending 267、结构错误 0；A05 仍为 2,918 visual、654 integrated、2,264 placeholder、声明路径断链 0；A06 本机技术准备仍为 19/19 PASS。
- 外部门禁复核结果仍 `BLOCKED`：A01—A06 六个真实 evidence envelope 缺失，Steam 未授权；D16-S01/S02、正式 D17 和 D18+ 未完成。

### 2026-08-13 下一本地开发目标（覆盖性当前指针）

- 用户已明确授权继续本地画面、UI、免费真实资源和完整可玩体验；这条授权只改变“是否可以继续本地开发”，不改变上述 D16 外部门禁事实。
- 新入口为 `docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md`，任务从 `D18-S01` 开始；共 D18—D24 七章、63 个完整可复制任务提示词。
- 正式本地开发顺序为 `D18-S02-v2 准入 → D19/D20/D21/D22 工作场景/Prefab → D19-S02 → D20-S02 → D21-S02 → D22-S02 → D18-S03 运行时资源晋级 → D23 → D24`；D19—D22 工作场景只能使用 `d18_prefab_candidate_intake_catalog.json` 的 `intake_eligible` 外部资源。D24-S02 最多给出“本地可玩候选版”，不能代替目标设备、跨平台、人工审核、隐私或 Steam 发布验收。
