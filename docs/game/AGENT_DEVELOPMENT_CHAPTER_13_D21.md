# 第 13 章开发任务提示词：D21 船舶、海洋、天气与航海表现

> 本章目标：把简单船模和静态蓝平面升级为可上下、可驾驶、可停靠、有帆装/锚/尾流/天气反馈的代表航海体验。执行顺序：`D21-S01 → D21-A01—A06 → D21-S02`。

## D21-S01：冻结船体比例、模块锚点、物理与表现接口

<details>
<summary>展开/复制：D21-S01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D21 船舶技术总控 Agent。请直接完成 D21-S01。

任务编号：D21-S01（同步冻结任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS；读取 D17 船控本机基线。工作场景/Prefab 可先做，正式场景集成将在 D19-S02 后进行；D18-S03 负责最终资源晋级。
并发规则：本任务完成前不启动 D21-A01—A06；只冻结接口，不修改正式船和场景。

开始前读取：AGENTS/CONTEXT/规范、D18 视觉/资源清单、ship 全套设计、D4/D5 船型/改装、D9 航行/泊位、D17 上下船/船控/镜头报告、D19 海平面/航道规范、当前 ShipSystem、船 Prefab 和控制适配器。

执行内容：
- 清点所有现有 hull_id/ship_id、起始船、代表船型、物理根、浮力/速度接口、船控、舵位、登船点、离船点、泊位、相机和存档引用；
- 冻结船体前向轴、世界尺度、海平面/吃水参考、Pivot、物理根与视觉根、碰撞层、甲板/舷墙/楼梯尺寸；
- 定义 HullRoot、VisualRoot、DeckColliders、HelmAnchor、BoardingAnchors、MooringAnchors、ModuleSockets、CameraAnchors、WakeAnchors、DamageSockets 等稳定接口；
- 定义起始小船和至少 3 类代表船的最低正式表现、LOD、材质、帆装、碰撞和模块要求；
- 定义表现层从 Wind/Weather/Ship 状态读取的接口，严禁视觉脚本裁决速度、伤害、货物或经济；
- 划分 A01—A06 唯一写入目录、测试矩阵和 S02 串行集成顺序；
- 明确 D17 上下船、人物视角/船视角切换、暂停和十循环基线不能回退。

只允许修改 D21 标准/接口文档、交接和记忆日志；禁止修改代码、Prefab、场景、Registry、存档和 Systems；不提交或推送 Git。

验收：所有锚点有名称、坐标约定、所有者和验证方式；代表船型与测试矩阵明确；与 D17/D19 的海平面、船控、登船兼容；git diff --check 通过。

结果必须保存到：
- 主成果：docs/game/design/common/D21_SHIP_VISUAL_PHYSICS_STANDARD.md、game/tools/results/D21/D21-S01/（验证结果）；
- 交接报告：docs/game/design/common/handoffs/D21/D21-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出 hull/anchor/接口冻结和 PASS/PARTIAL/BLOCKED。
```
</details>

## D21-A01：起始船与代表船体正式模型/Prefab

<details>
<summary>展开/复制：D21-A01 完整提示词</summary>

```text
你是 WhalesIslandReef 的船体模型与 Prefab Agent。请直接完成 D21-A01。

任务编号：D21-A01（异步船体 Prefab 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D21-S01 PASS。
单一写者：game/Assets/Art/Models/Ships/D21/Hulls/、game/Assets/Prefabs/Ships/D21/Hulls/、船体材质/测试/文档。禁止改正式船生成器、场景、帆装和模块 Agent 文件。

开始前读取：D21 标准、D18 合法船资源、ship 设计/船型目录、D4/D5 hull_id、当前起始船/远景船 Prefab 和 D17 船控锚点。

执行内容：
- 为起始可驾驶船制作正式 Hull Prefab，并为至少 3 类代表船型制作可用船体 Prefab；合法模型不足时至少完成起始船并按真实情况 PARTIAL；
- 对齐 hull_id、尺度、前向轴、海平面/吃水、物理根、视觉根、甲板高度和 Bounds；
- 船体必须有可行走甲板、舷墙/主要障碍碰撞、船舱入口占位或明确封闭、Helm/Boarding/Camera/Wake 等接口空节点；
- 修复材质、法线、阴影、重复网格和明显穿模；建立近/中/远 LOD 或明确性能方案；
- 视觉 Prefab 不挂 ShipSystem 逻辑，不自行设置速度/货舱/火力；通过现有 hull_id 解析；
- 建立尺度、轴向、锚点、碰撞、材质、LOD 和缺失引用测试，输出多角度预览。

硬约束：工作场景/Prefab 制作阶段只能使用准入目录中的 `intake_eligible` 外部资源；不改变 hull_id/数值/存档；不以缩放同一船体伪造多个船型；不改场景、ShipSystem、控制器、模块/帆装、Registry；不得提前标记 LocalDevelopmentReady，正式晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译/测试；起始船和代表船预览，无粉色材质/缺网格；角色尺度、甲板、舵位和登船锚点符合标准；记录三角面/材质/LOD；git diff --check。

结果必须保存到：
- 主成果：上述船体模型/Prefab/材质/测试、docs/game/design/common/D21_SHIP_HULL_PREFABS.md、game/tools/results/D21/D21-A01/；
- 交接报告：docs/game/design/common/handoffs/D21/D21-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出 hull_id、来源、实际完成船型、碰撞/LOD、缺口和 PASS/PARTIAL/BLOCKED。
```
</details>

## D21-A02：桅杆、帆、索具、舵、锚与改装模块表现

<details>
<summary>展开/复制：D21-A02 完整提示词</summary>

```text
你是 WhalesIslandReef 的帆装与船舶模块表现 Agent。请直接完成 D21-A02。

任务编号：D21-A02（异步模块 Prefab 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D21-S01 PASS；读取 A01 接口但不得修改 A01 文件。
单一写者：game/Assets/Prefabs/Ships/D21/Modules/、game/Assets/Art/Models/Ships/D21/Modules/、模块动画/材质/Presentation/测试/文档。禁止改 ShipSystem、正式船组合和场景。

开始前读取：D21 标准、D18 合法船/道具资源、ship 桅杆/帆/改装/锚泊设计、D4/D5 模块 ID 和当前船帆/锚/改装状态接口。

执行内容：
- 制作起始船需要的桅杆、帆、索具、舵、锚和系泊绳表现 Prefab；
- 至少为货运、战斗、探索/渔业三类代表改装提供可见模块或挂点表现，例如货物、炮位、渔具、灯具、救生/修理设备；
- 通过 ModuleSocket/module_id 挂接，不直接改速度、耐久、容量、伤害或价格；
- 帆根据现有航行/风状态做张帆、收帆、转向或受控近似动画；锚有升降/收纳表现，但 AnchoringSystem 才能裁决锚泊；
- 建立模块缺失 fallback，旧档中的 module_id 即使无正式视觉也不能丢失；
- 设置材质、LOD、碰撞/触发边界和动画参数，添加挂点/状态映射/缺失模块测试和预览。

硬约束：表现层只读；不改 ShipSystem/Module/Anchor 数值、Registry/存档 Schema、A01 船体、场景；不让绳索/帆物理造成不可控性能；外部资源只能使用准入目录中的 `intake_eligible` 条目；不得提前标记 LocalDevelopmentReady，正式晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译/测试；起始船完整帆装、锚/舵可见、3 类改装可辨识；缺模块安全回退；记录动画/材质/性能；git diff --check。

结果必须保存到：
- 主成果：上述模块/动画/Presentation/测试、docs/game/design/common/D21_RIGGING_ANCHOR_MODULE_PRESENTATION.md、game/tools/results/D21/D21-A02/；
- 交接报告：docs/game/design/common/handoffs/D21/D21-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出 module_id、状态映射、缺口、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D21-A03：停靠、上下船、甲板碰撞与交互锚点

<details>
<summary>展开/复制：D21-A03 完整提示词</summary>

```text
你是 WhalesIslandReef 的船舶交互与物理适配 Agent。请直接完成 D21-A03。

任务编号：D21-A03（异步交互适配任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D21-S01 PASS；读取 D17 上下船实现和 D21-A01/A02 接口，不修改其资源。
单一写者：game/Assets/Scripts/Gameplay/D21/ShipInteractionAdapters/、game/Assets/Prefabs/Ships/D21/Interaction/、相关测试/文档。禁止改 Systems 中航行/锚泊裁决和 InitialHarbor。

开始前读取：D21 标准、D17 boarding/ship control/camera 回归、D9 泊位流程、D19 港口泊位/航道、当前输入意图、角色碰撞层和船体移动父子关系。

执行内容：
- 建立稳定的 BoardingAnchor、DisembarkAnchor、HelmAnchor、MooringAnchor 和甲板安全区适配；
- 上下船必须继续调用现有意图/裁决，适配层只选择安全锚点、切换视觉/物理附着和给出失败原因；
- 设置甲板、舷墙、桅杆、楼梯/坡道和舵位碰撞，角色在静止/移动船上不穿透、不被弹飞、不永久父子缩放；
- 停靠时显示系泊/跳板表现和可交互范围；距离、速度或泊位状态不允许时明确反馈；
- 处理无锚点、锚点被挡、船移动、角色落水、读档在船上、暂停/离港/返港恢复；
- 添加至少 10 轮上下船、驾驶/停靠、失败分支、读档和旧 Prefab fallback PlayMode 测试。

硬约束：不改速度、泊位合法性、锚泊、伤害和经济强逻辑；Systems 不得出现 Unity 表现或网络；不改 A01/A02、正式场景、存档 Schema/Registry；不提交或推送 Git。

验证：Unity 编译/测试；D17 上下船/落地/船控/相机测试全部保持；10 轮零永久卡死，失败有明确 code/message；git diff --check。

结果必须保存到：
- 主成果：上述适配器/交互 Prefab/测试、docs/game/design/common/D21_BOARDING_DOCKING_COLLISION.md、game/tools/results/D21/D21-A03/；
- 交接报告：docs/game/design/common/handoffs/D21/D21-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出锚点、碰撞层、10 轮结果、失败恢复和 PASS/PARTIAL/BLOCKED。
```
</details>

## D21-A04：船体浮沉表现、尾流、浪花与海况适配

<details>
<summary>展开/复制：D21-A04 完整提示词</summary>

```text
你是 WhalesIslandReef 的船海交互技术美术 Agent。请直接完成 D21-A04。

任务编号：D21-A04（异步表现任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D21-S01 PASS；读取 D19 水体和现有 ShipMovement 状态。
单一写者：game/Assets/Scripts/Presentation/D21/ShipOcean/、game/Assets/Prefabs/VFX/D21/ShipOcean/、相关材质/测试/文档。禁止改水体核心文件、ShipSystem 和正式场景。

开始前读取：D21 标准、D19_WATER_SHORE_UNDERWATER、ship 航行/损伤设计、Weather/Wind/Ship 只读接口、当前船体运动和性能目标。

执行内容：
- 建立视觉浮沉、横摇/纵摇、船艏浪、尾流、航迹泡沫、转向侧浪和停船衰减表现；
- 所有强度从现有船速、转向、风浪/天气只读状态映射，不反向设置速度、浮力、位置或伤害；
- 船停泊/锚泊/受损/搁浅时有不同但受控反馈；
- 提供低/中/高三档粒子、贴花或网格方案，并支持对象池、距离裁剪和多船预算；
- 处理海平面切换、船 Prefab 缺 WakeAnchor、读档/场景重载和时间暂停；
- 添加状态映射、无副作用、对象池、三档和性能测试，输出不同速度/天气截图。

硬约束：不实现第二套浮力物理，不写 ShipSystem/WeatherSystem；不改 D19 水 Shader、正式船/场景、Registry/存档；不引入高成本第三方海洋插件；不提交或推送 Git。

验证：Unity/Shader 编译和测试；静止/低速/高速/转向/风暴状态可辨识，暂停不继续累积；1080p 性能样本和三档对比；git diff --check。

结果必须保存到：
- 主成果：上述 Presentation/VFX/测试、docs/game/design/common/D21_SHIP_OCEAN_PRESENTATION.md、game/tools/results/D21/D21-A04/；
- 交接报告：docs/game/design/common/handoffs/D21/D21-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出状态映射、VFX 预算、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D21-A05：风、雨、雾、风暴与昼夜航行反馈

<details>
<summary>展开/复制：D21-A05 完整提示词</summary>

```text
你是 WhalesIslandReef 的海上天气表现 Agent。请直接完成 D21-A05。

任务编号：D21-A05（异步天气 Presentation 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D21-S01 PASS；读取 D19 Lighting/Water Profile 和现有天气系统。
单一写者：game/Assets/Scripts/Presentation/D21/MarineWeather/、game/Assets/Prefabs/VFX/D21/Weather/、game/Assets/Audio/D21/Weather/、专属 Profile/测试/文档。禁止改 WeatherSystem/TimeSystem 和正式场景。

开始前读取：D19 光照/水体验收、D21 标准、environment 天气/海流/音频设计、D9 现有 5 天气状态、可访问性低刺激设置和 AudioMixer。

执行内容：
- 将现有晴、阴/雨、雾、强风/风暴及昼夜状态映射为风向可视化、植被/帆响应参数、雨粒子、雾、云/天空、闪电可选效果和天气音景；
- 保持天气概率、持续时间、航行影响和事件裁决由原系统负责；Presentation 只订阅只读状态；
- 为导航可读性提供能见度、方向、危险和状态反馈，低刺激模式降低闪电/屏幕闪烁和强震动；
- 处理室内/遮挡区域、相机切换、暂停、加载和天气快速切换；
- 提供低/中/高三档粒子、阴影、雾和音频密度；
- 添加 5 状态切换、暂停、读档、低刺激、性能和无强逻辑写入测试。

硬约束：不改 Weather/Time/Navigation 强逻辑，不在 Systems 加 VFX/Audio；不改 D19 核心 Profile/水 Shader、UI、场景、Registry/存档；不引入未审核资源；不提交或推送 Git。

验证：Unity/Shader 编译与测试；5 状态和昼夜截图/音频证据；切换无泄漏/叠加失控，低刺激有效；三档性能；git diff --check。

结果必须保存到：
- 主成果：上述 Presentation/VFX/Audio/Profile/测试、docs/game/design/common/D21_MARINE_WEATHER_PRESENTATION.md、game/tools/results/D21/D21-A05/；
- 交接报告：docs/game/design/common/handoffs/D21/D21-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出天气映射、可访问性、性能、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D21-A06：船舶相机、音频、碰撞和损伤反馈

<details>
<summary>展开/复制：D21-A06 完整提示词</summary>

```text
你是 WhalesIslandReef 的船舶镜头、音频与损伤表现 Agent。请直接完成 D21-A06。

任务编号：D21-A06（异步表现任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D21-S01 PASS；读取 D17 相机/船控和 D21 锚点规范。
单一写者：game/Assets/Scripts/Presentation/D21/ShipFeedback/、game/Assets/Audio/D21/Ships/、game/Assets/Prefabs/VFX/D21/Damage/、专属相机 Profile/测试/文档。禁止改输入裁决、正式船/场景和 DamageSystem。

开始前读取：D17 第三人称/船相机报告、D21 标准、ship damage/audio 设计、现有碰撞/损伤事件、D16 可访问性/低刺激和 AudioMixer。

执行内容：
- 为登船、掌舵、甲板步行、追尾视角建立可配置相机 Profile，保留 D17 输入焦点和遮挡收缩；
- 船体木结构、帆索、风、浪、舵、锚、碰撞和受损事件映射到分层音频；
- 受损使用裂痕/碎屑/烟火/进水提示等表现，但损伤值和沉没由 DamageSystem 裁决；
- 相机震动按事件强度且受设置/低刺激控制，不能持续抖动或抢 UI 焦点；
- 处理视角切换、暂停、菜单、读档、船被销毁/切换和缺失锚点回退；
- 添加输入焦点、遮挡、震动关闭、音量、损伤事件映射和无副作用测试。

硬约束：不改伤害、碰撞裁决、输入意图、船速或存档；不在 Systems 放音频/相机；不改正式船/场景/UI 根；只用合法资源；不提交或推送 Git。

验证：Unity 编译/测试；D17 角色/船视角、暂停/菜单焦点回归通过；音频走 Mixer，低刺激关闭震动；代表损伤截图/事件日志；git diff --check。

结果必须保存到：
- 主成果：上述 Presentation/Audio/VFX/Profile/测试、docs/game/design/common/D21_SHIP_CAMERA_AUDIO_DAMAGE_FEEDBACK.md、game/tools/results/D21/D21-A06/；
- 交接报告：docs/game/design/common/handoffs/D21/D21-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出镜头模式、事件映射、可访问性、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D21-S02：正式船海集成与航海体验验收

<details>
<summary>展开/复制：D21-S02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D21 船海同步集成与验收 Agent。请直接完成 D21-S02。

任务编号：D21-S02（同步集成任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D21-S01、D21-A01—A06 全部有可用成果；D19-S02 PASS；建议 D20-S02 已通过以验证正式角色上下船。
并发规则：你是正式船 Prefab、船生成映射、InitialHarbor 船/水面集成和共享船相机连接的唯一写者。

开始前读取：全部 D21 成果/交接/测试、D19/D20 验收、D17 船控十循环、D18 资源清单、现有 hull/module/ship_id 与存档映射。

执行内容：
1. 组合起始船 Hull、帆装/模块、交互碰撞、船海 VFX、天气参数、相机/音频/损伤反馈并通过现有 hull_id 接入；
2. 在正式港口替换简单船视觉，保持泊位、登船、舵位、离船、离港、返港和旧档兼容；
3. 接入代表船型到受控测试/展示入口，不一次性替换无合法资源的所有 5+ 船型；缺失时明确 fallback；
4. 验证角色在静止/移动船上、上下船、掌舵、停靠、锚泊反馈、不同天气、日夜、碰撞/损伤和读档；
5. 运行至少 10 轮“港口登船 → 离港 → 航行/天气 → 返港 → 下船”聚焦流程，以及 D17/D19/D20/核心 PlayMode 回归；
6. 输出船体多角度、晴/雨/雾/风暴、尾流、甲板、停靠、损伤和夜航截图/性能证据。

硬约束：不改 Ship/Weather/Damage/Economy 强逻辑和存档 Schema/contracts；表现适配只读；正式集成只使用准入目录和项目自有表现资产，不标 LocalDevelopmentReady/ReleaseApproved；不删除旧 hull_id/fallback；为 D18-S03 保留真实消费者、保存和回滚证据；不提交或推送 Git。

通过门槛：起始船不再是简单占位；帆/舵/锚/甲板/碰撞/登船点可见且可用；海水、尾流和 5 类天气反馈稳定；10 轮零关键卡死/焦点丢失/读档失败；D17 回归不下降；1080p 三档性能有记录；Unity 无新增错误。

结果必须保存到：
- 主成果：正式船/船海集成 Prefab、适配器/测试、docs/game/design/common/D21_SHIP_OCEAN_ACCEPTANCE.md、game/tools/results/D21/D21-S02/；
- 交接报告：docs/game/design/common/handoffs/D21/D21-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED；报告列出 10 轮、全量测试、截图、性能、兼容 fallback 和阻断项。
```
</details>
