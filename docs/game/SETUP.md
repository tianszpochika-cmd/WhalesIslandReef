# 开发环境搭建 · 本地测试 · 素材获取清单

> 配套 `REQUIREMENTS.md`、`DEV_ROADMAP.md` 和 `docs/DEVELOPMENT_STANDARDS.md` 使用。目标：在 Windows 上运行 D7 基线并进入 D8。
> 更新：2026-08-07 ｜ 前提：目标先做单机（架构预留多人，本阶段不装多人框架）

---

## 1. 需要手动安装的软件（按顺序）

| 顺序 | 软件 | 版本/要点 | 用途 | 费用 |
|---|---|---|---|---|
| 1 | **Unity Hub** → Unity 6 LTS | 选 6000.x LTS 版本；安装时勾选模块：**Windows Build Support (IL2CPP)**、中文语言包（可选） | 引擎本体 + 打包 | 免费（个人版：年收入 <$200k 免许可费，Unity 6 已取消 Runtime Fee） |
| 2 | **Visual Studio 2022 Community**（或 VS Code + C# Dev Kit 扩展） | 安装时勾选"Unity 游戏开发"工作负载 | C# 代码编辑/调试 | 免费 |
| 3 | **Git** + **Git LFS** | https://git-scm.com ；LFS 用于大资产 | 版本控制（Unity 资产二进制多，必须 LFS） | 免费 |
| 4 | **Ollama** | https://ollama.com ；装好后 `ollama pull qwen2.5:3b` 和 `qwen2.5:7b` | 本地 AI 船员推理（离线、零成本） | 免费 |
| 5 | **Steamworks SDK**（提前下载备用） | partner.steamgames.com 登录后下载 | 发布时集成成就/商店/联机 | 免费（发布阶段才用） |

> 不需要装：Node.js、Python、数据库服务（SQLite 走 Unity 内 C# 库，无需独立安装）。多人框架（Mirror 等）**本阶段不装**，只做架构预留。

## 2. 需要在 Unity 项目内安装的插件/包

| 包 | 来源 | 用途 | 阶段 |
|---|---|---|---|
| **Crest Ocean**（≈$90） | Unity Asset Store | 海洋渲染 + 波浪 + 浮力 | D9 按垂直切片接入 |
| **Unity Test Framework** | Package Manager（引擎自带） | 单元测试 | D8 必须可用 |
| **Input System**（新版） | Package Manager（自带） | 键盘/手柄/未来语音输入统一入口 | D8 必须可用 |
| Cinemachine | Package Manager（自带） | 摄像机（第三人称跟船、舰桥视角） | D9 按需接入 |
| TextMeshPro | Package Manager（自带） | 对话 UI 文本 | D9 按需接入 |
| ParrelSync | GitHub 开源（**多人阶段再装**） | 本地多开编辑器联机测试 | 将来 |
| FMOD / Wwise | 第三方（可选，后期音频量大了再引入） | 音频中间件 | 可后置 |

**明确本阶段不装**：Mirror / Netcode for GameObjects / Relay —— 多人规划已体现在代码架构（状态层与表现层解耦 + `Scripts/Networking` 命名空间预留），不引入运行时依赖。

## 3. 项目初始化三步（避免踩坑）

1. **先用 Unity Hub 新建项目**：路径建议 `C:\Users\Administrator\Desktop\game\first01\Game`（若 Hub 提示目标目录非空，就新建到这个子目录，不要强行用非空根目录）；
2. 项目建好后：`git init` + 把仓库根目录的 `.gitignore` 复制到项目根 + `git lfs install`，做第一次提交；
3. 建立 Assets 目录规范（已列好，直接照建）：

```
Assets/
  Scenes/            # 主场景、港口场景、测试场景
  Scripts/
    Gameplay/        # 航行、船只、天气（表现层）
    Agent/           # LLM 封装、Persona、记忆、对话 UI
    Systems/         # 强逻辑层：经济/状态机/事件/声望（纯 C#，可单测）
    Networking/      # 预留空目录（多人阶段使用）
    Core/            # 事件总线、存档、配置
  Prefabs/           # 船、船员、港口、岛屿
  Art/Models/  Art/Textures/  Art/Materials/
  Audio/Music/  Audio/SFX/  Audio/Voice/
  UI/                # 字体、图标、对话界面
  Settings/          # ScriptableObject 配置
  ThirdParty/        # Crest 等第三方包导入位置
```

## 4. 本地测试方案（分层，从快到慢）

| 层级 | 方法 | 覆盖内容 |
|---|---|---|
| 日常迭代 | **Play Mode**（编辑器内直接运行） | 手感、UI、场景流转，最快反馈 |
| 逻辑单元测试 | **Test Runner**（Edit Mode 测试，纯 C# 跑，不渲染） | 经济供需公式、船员状态机、事件触发条件、声望结算 —— 强逻辑系统的正确性全靠这层 |
| 集成场景测试 | 手写场景测试清单（每个里程碑过一遍） | 航行→靠港→贸易→事件→存档读档→昼夜循环 全链路 |
| AI 链路测试 | 日志 + 开关 | ① Ollama/DeepSeek 切换开关 ② 断网降级（回落本地→模板）③ 延迟记录（目标本地<1.5s/云端<3s）④ JSON schema 校验器（LLM 输出必须通过 schema 才进逻辑层） |
| 性能测试 | Unity **Profiler**（CPU/GPU/Memory）+ Memory Profiler + Frame Debugger | 目标：中端显卡 1080p 60fps；重点盯 Crest 波浪开销与多 Agent 轮询 |
| 打包验收 | Build Settings → Windows Standalone → 本机跑 exe | 启动时间、全屏/窗口、多分辨率、存档路径、无编辑器运行 |
| 黑盒测试 | 把 exe 发给 3–5 个朋友 | 真实玩家视角的崩溃/卡关/理解成本（首轮评价生死线） |
| （将来多人） | ParrelSync 双开编辑器 + 双客户端 | 联机同步正确性 |

**测试基建建议**：逻辑层（Systems/ 下的类）从一开始就写成"可脱离 Unity 运行"的纯 C# 类，单元测试才写得动——这是"强逻辑"质量的生命线。

## 5. 素材怎么做（建模 / 贴图 / 音乐音效）

### 5.1 总体策略：买为主、AI 辅助、原创只做必要部分

单人开发低多边形航海风格，**90% 用资产商店现成包 + 10% 原创/AI 补缺**，性价比最高。

### 5.2 3D 模型

| 方式 | 渠道/工具 | 说明 |
|---|---|---|
| 购买（首选） | **Synty 系列**（Asset Store / Synty Store，海盗、航海、港口等主题包，每个 $30–60） | 低多边形风格统一、自带动画，最适合个人项目；首发 10–15 个港口完全够用 |
| 免费资源 | Unity Asset Store 免费包、Sketchfab / TurboSquid（看授权，商业授权才可用） | 补零散物品 |
| 原创（Blender，免费） | 船只小部件、旗子、绳索等少量物件 | 按需学基础，别在建模上花大时间 |
| AI 建模（原型加速） | Meshy、Tripo、Rodin 等文生/图生 3D | 2026 年已可产出可用的低模原型，再进 Blender 清理 |

### 5.3 贴图与材质

- 免费高质贴图：**Poly Haven**（CC0）、ambientCG；
- AI 生图做概念图/贴图：即梦、通义万相、Midjourney 等（生成后需在 Blender/PS 处理成可平铺贴图）；
- 自定义材质用 Unity **Shader Graph**（URP 内置，不用写代码）。

### 5.4 角色动画

- Synty 包自带基础动画（走/跑/划船/搬运等）；
- 缺的动画去 **Adobe Mixamo**（免费，绑定+动画重定向到 Synty 角色）；
- 对话时的人物表现：先做"文本气泡 + 表情/朝向"即可，口型动画后置。

### 5.5 音乐与音效

| 类型 | 首选方案 | 备选 |
|---|---|---|
| 背景音乐 | **AI 生成**（Suno / Udio / Stable Audio），航海氛围曲风，生成后拿到商用授权 | 免费站 Incompetech（Kevin MacLeod，署名即可）；商业库 Artlist / AudioJungle（$15–50/首） |
| 音效（海浪/帆/雷暴/碰撞） | **Freesound**（CC0 优先筛选）+ Soundly 采样库 | Boom Library 商业包 |
| 船员语音（TTS） | **后置**：先文本对话，后期用 Edge TTS（免费）或 Azure TTS / ElevenLabs | 中文语音选 Azure 晓晓类音色 |

### 5.6 字体与其他

- 中文：**思源黑体 / 思源宋体**（SIL OFL 免费商用）、阿里巴巴普惠体；
- 所有素材（含 AI 生成产物）**留存授权记录清单**（来源、授权类型、链接）——Steam 上架合规必查项，从第一天就建表格。

### 5.7 素材预算参考

| 项 | 预算 |
|---|---|
| Synty 海盗 + 航海 + 港口包 | $150–300 |
| Crest Ocean | $90 |
| 音频（AI 生成 + 少量商业库） | $30–80 |
| 字体/杂项 | $0–30 |
| **合计** | **$300–500**（比原预估低，因为 AI 生图/音频替代了部分采购） |

---

## 6. 开工顺序（装好后第一周）

1. 装完第 1–4 项软件，确认 Unity 6 能建项目、Ollama 能跑通 qwen2.5:3b；
2. Unity Hub 建项目 → git init + LFS → 建 Assets 目录规范；
3. 以 D7 基线为起点，先完成 D8 数据/契约/存档检查，再在 D9 垂直切片接入海洋和玩法；
4. 写一个最小 Agent 对话脚本（本地 Ollama → 对话 → 好感度变化 → 写 SQLite），跑通"强逻辑 × 重交互"的最小闭环；
5. 打开 Test Runner，给经济/状态机类写第一批单元测试，形成"边写边测"习惯。
