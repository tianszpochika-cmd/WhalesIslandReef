# 第 10 章开发任务提示词：D18 免费资源、美术标准与导入管线

> 本章目标：把“网上找免费模型”变成可审计、可复用、可持续扩展的正式资源管线。历史主线 `D18-S01 → A01/A02 → A03—A07 → S02-v2` 与 R01 准入刷新均已留痕；但女性/中性人形仍没有真实 Unity Humanoid/Avatar 证据，玩家发型/时代服装也没有可消费资源。因此当前修复线是 `D18-A04-R02-F-R01 / D18-A04-R03-P（并行）→ D18-S02-R02（唯一准入刷新写者）→ D20-A01-R01`。`D18-S03` 仍只在 D19—D22 正式集成后执行。
>
> D18 允许推进本地开发资源，但不替代 D16 的人工内容审查、目标设备和发布门禁。

## D18-S01：冻结视觉方向、质量档位和资源授权规则

<details>
<summary>展开/复制：D18-S01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 美术总监与同步规划 Agent。请直接完成任务，不要只给建议。

任务编号：D18-S01（同步任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置条件：读取当前 D16/D17 状态；本任务可以在 D16 外部门禁未关闭时执行，但只能冻结本地开发标准。
并发规则：你是本任务唯一写者。执行期间不要让其他 Agent 同时修改 AGENT_DEVELOPMENT_TASKS、D18 标准或资源目录规则。

开始前按顺序完整读取：
1. AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md；
2. docs/game/REQUIREMENTS.md、DESIGN_PILLARS.md、design/DESIGN_BOUNDARIES.md；
3. docs/ARCHITECTURE.md 第 8.1 节、docs/game/design/environment、character、ship、ui、audio 的相关设计；
4. docs/game/D17_POST_D16_DEVELOPMENT_PLAN.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md；
5. D16-A04/A05 当前审计、候选包、placeholder 统计和 InitialHarbor 当前截图/场景结构。

本任务目标：
- 明确项目采用“统一风格化低多边形、可信比例、清晰光照与反馈”的美术方向；
- 定义玩家模式与开发调试模式的视觉边界；
- 冻结 1080p 画质、帧率、内存、同屏实体、LOD、材质和加载预算；
- 定义模型比例、单位、轴向、Pivot、命名、材质、贴图、骨骼、动画、碰撞、LOD、Prefab 和截图标准；
- 定义 Candidate、LicenseEvidenceComplete、ImportVerified、LocalDevelopmentReady、ReleaseApproved、Rejected 六种状态及转换条件；
- 明确 ReleaseApproved 只能由有责任的人类审核者确认，Agent 不得代签；
- 定义大体积二进制资源的 Git LFS、本地下载缓存、仓库增量和单包大小策略；没有确认远程 LFS 支持/配额时，不得把大量压缩包直接加入普通 Git 历史；
- 给圣玛格丽特港建立不少于 12 个固定验收镜头：主菜单、出生点、市场、码头、酒馆/室内入口、海岸、登船、船上、离港、近海、天气、返港；
- 将当前截图中的方块岛、白色地面、占位模型、HUD 堆叠和死按钮列为必须消除的视觉债务。

只允许修改：
- docs/game/design/common/D18_VISUAL_ART_DIRECTION_AND_BUDGET.md；
- docs/game/design/common/handoffs/D18/D18-S01_HANDOFF.md；
- .workbuddy/memory/当天日期.md；
- 如需链接，只可对 docs/game/design/README.md 做最小索引补充。

禁止事项：
- 不下载或导入资源；不修改 Unity 场景、Prefab、代码、Registry、构建设置；
- 不更改玩法公式、存档字段、实体 ID 或系统所有权；
- 不把 D16 未完成项写成已完成；
- 不把“写实”理解为照片级目标，不制定个人项目无法验收的无限规格；
- 不覆盖工作区中其他 Agent 或用户的未提交改动，不提交或推送 Git。

验收要求：
- 文档包含视觉支柱、颜色/材质/灯光示例原则、模型和动画技术规范、三档画质、12+ 固定镜头、HUD/调试层边界、资源状态机和可量化 DoD；
- 每一项预算有单位、测试场景、采集方式和超限处理；
- 明确 D18—D24 是本地开发轨，D24-S02 不自动等于 Steam Ready；
- 对照 AGENTS.md 和 DEVELOPMENT_STANDARDS DoD 自查，文档链接有效，git diff --check 通过。

结果必须保存到：
- 主成果：docs/game/design/common/D18_VISUAL_ART_DIRECTION_AND_BUDGET.md、game/tools/results/D18/D18-S01/（验证结果）；
- 交接报告：docs/game/design/common/handoffs/D18/D18-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须写：实际修改文件、冻结的关键数值、12+ 镜头清单、未决事项、验证结果，以及结论 PASS/PARTIAL/BLOCKED。聊天回复只给摘要和上述路径，不能代替文件。
```
</details>

## D18-A01：免费资源来源目录、许可证清单与自动审计

<details>
<summary>展开/复制：D18-A01 完整提示词</summary>

```text
你是 WhalesIslandReef 的免费资源治理 Agent。请直接完成 D18-A01，建立真实可运行的清单和审计，不要只列网站名称。

任务编号：D18-A01（异步工具/文档任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S01 必须 PASS。
并发边界：只写 tools 中本任务专属工具/测试、docs/game/design/common/assets、必要的 `.gitattributes`/`.gitignore` 最小资源规则和 D18-A01 交接；不碰 Unity 主场景、正式模型、Prefab、Registry 和其他 D18 Agent 的目录。

开始前读取：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/ARCHITECTURE.md 第 8.1 节、D18_VISUAL_ART_DIRECTION_AND_BUDGET.md、D18_D24_VISUAL_PLAYABLE_ROADMAP.md、D16_A05_KENNEY_CANDIDATE_PACKET_01.md、现有 D16 许可证/候选/替换预检工具。

执行内容：
1. 建立统一资源包 manifest Schema 和示例，字段至少含 source_id、pack_id、title、author、official_page、download_url、downloaded_at、archive_name、sha256、license_id、license_url、license_evidence_path、attribution_required、commercial_use、modification_allowed、source_redistribution、build_distribution、category、file_count、unity_object_count、review_status、import_status、runtime_consumers、notes；
2. 建立只读审计工具和自动测试：检查缺字段、重复 ID、非法状态跃迁、哈希格式、许可证证据断链、候选文件断链、路径逃逸、未批准运行时引用、同一文件多来源冲突；工具不得自动批准或自动修改 Registry；
3. 建立来源目录，至少核对 Kenney、Quaternius、Poly Haven、ambientCG 的官方许可页与适用类别，并登记第一批明确包：Kenney Pirate Kit、Blocky Characters、UI Pack、Input Prompts；Quaternius Pirate Kit、Ultimate Stylized Nature、Ultimate Animated Animal Pack、Animated Man Pack；
4. 对许可证混杂、要求登录、仅限个人、NC、来源不明和禁止再分发的站点写明拒绝/人工复核规则；
5. 给后续 A03—A06 提供可复制的 manifest 模板和唯一 ID 规则，不伪造未下载文件的哈希、数量或审核结果。
6. 审计当前 Git LFS 是否安装、远程是否支持以及配额是否可确认；确认可用时只为明确的模型/贴图/音频/归档扩展名增加最小 `.gitattributes` 规则，不迁移历史；不可确认时建立仓库外或被忽略的本地下载缓存规则，并规定每包/每章仓库增量上限，禁止后续 Agent 静默加入数 GB 二进制。

硬约束：
- 技术搜索必须以官方来源和许可证原文为准；页面内容可能变化，记录本次核对日期；
- 基线自动接入只接受证据完整的 CC0；其他许可一律 Pending，除非责任审核者确认；
- “免费下载”不等于可商业发行或可提交源文件；禁止抓取盗版、爬取整站或规避登录/限流；
- 工具只读；任何 Approved/ReleaseApproved 状态都不能由工具推断；
- 不执行 Git 历史迁移，不自动提交/推送，不假定 GitHub LFS 配额；修改 ignore/attributes 前先保留并合并用户现有规则；
- 不修改 Systems、contracts、server、web、Unity 场景/Prefab；不提交或推送 Git。

验证：
- 运行新增工具测试并记录测试数与结果；
- 用真实现有 Kenney 候选包和至少 3 个故意损坏的测试夹具验证 PASS/BLOCKED；
- 审计现有候选目录并输出机器结果到 game/tools/results/D18/D18-A01/；
- 运行 git diff --check。

结果必须保存到：
- 主成果：docs/game/design/common/D18_FREE_ASSET_SOURCE_CATALOG.md、docs/game/design/common/assets/d18_asset_pack_manifest.example.json、tools/schemas/d18_asset_pack_manifest.schema.json、tools/d18_asset_manifest_audit.py、对应 tests、game/tools/results/D18/D18-A01/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须列出来源、真实核对日期、工具命令、测试结果、当前可证明与不可证明的许可范围、未解决风险和 PASS/PARTIAL/BLOCKED。不要把网页摘要当作许可证证据的唯一副本。
```
</details>

## D18-A02：Unity 模型、材质、动画和 Prefab 导入管线

<details>
<summary>展开/复制：D18-A02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 Unity 美术技术管线 Agent。请直接完成 D18-A02。

任务编号：D18-A02（异步编辑器工具任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
Unity 项目：C:\Users\Administrator\Desktop\game\first01\game
前置任务：D18-S01 必须 PASS；读取 D18-A01 的 Schema/模板（若 A01 尚未完成，只做不依赖最终 Schema 的导入工具并在交接中标记接口待对齐，不能自行创建第二套清单）。
并发边界：只写 game/Assets/Editor/D18ArtPipeline、其测试、D18 专属配置和文档；不修改 InitialHarbor、主 Bootstrap、正式 Prefab 和其他 Agent 的候选包。

开始前读取：AGENTS.md、docs/CONTEXT.md、DEVELOPMENT_STANDARDS.md、ARCHITECTURE.md 资源位置、D18_VISUAL_ART_DIRECTION_AND_BUDGET.md、D18-A01 清单（若存在）、现有 D16CandidateAssetAudit.cs、当前 URP/渲染与 Unity 版本配置。

执行内容：
- 建立可重复执行的 ModelImporter/TextureImporter/AudioImporter 检查与建议工具，覆盖单位、轴向、法线、切线、网格压缩、Read/Write、动画类型、骨骼、Avatar、材质提取、贴图类型、sRGB、尺寸、音频压缩；
- 建立分类规则：Ships、Characters、Animals、Environment、Buildings、Props、Vegetation、UI、Audio；
- 建立 Prefab 预检：必须检查 MeshRenderer/SkinnedMeshRenderer、材质、bounds、碰撞、交互锚点、LODGroup、缺失脚本、丢失引用和命名；
- 建立“报告优先”模式：默认只生成问题报告，不自动覆盖导入设置；只有显式选择且文件位于 D18 测试夹具时才允许自动修复；
- 对已有 Kenney 候选包运行导入审计，输出当前真实结果，不把 ImportVerified 写成 ReleaseApproved；
- 建立 EditMode 测试和一个最小测试夹具，证明重复运行幂等、不会修改项目外路径、不会扫描 Library/Temp/Logs。

硬约束：
- 工具必须是 Editor-only，运行时代码和 Systems 不得依赖 UnityEditor；
- 不自动创建人工审核结论，不改 Registry/asset_state，不从网络下载资源；
- 不把所有模型强制成同一个 scale；报告模型自身尺度与目标尺度，由后续 Prefab 明确修正；
- 场景只引用 Prefab；工具不得把散模型自动塞进 InitialHarbor；
- 保留 .meta/GUID，不做全项目 Reimport，不覆盖用户未提交改动，不提交或推送 Git。

验证：
- Unity 6 批处理编译无新增错误；相关 EditMode 测试通过；
- 对现有候选包生成 JSON/Markdown 报告，包含文件数、Mesh、动画、材质、问题数；
- 构建受影响程序集，运行 git diff --check；
- 若 Unity 路径不可用，必须留下可复现命令并标记 PARTIAL，不能宣称通过。

结果必须保存到：
- 主成果：game/Assets/Editor/D18ArtPipeline/、对应 Tests、docs/game/design/common/D18_UNITY_ASSET_IMPORT_PIPELINE.md、game/tools/results/D18/D18-A02/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告写清修改文件、支持格式、只读/自动修复边界、Unity 命令、实际测试结果、现有候选包问题和 PASS/PARTIAL/BLOCKED。
```
</details>

## D18-A03：环境、地形、植被与岩石免费资源包

<details>
<summary>展开/复制：D18-A03 完整提示词</summary>

```text
你是 WhalesIslandReef 的环境资源 Agent。请直接完成 D18-A03，获取并整理能服务样板岛的真实免费资源，不要只写推荐清单。

任务编号：D18-A03（A* 异步资产任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S01、D18-A01、D18-A02 必须完成；若任一缺失，停止下载并在交接写 BLOCKED。
单一写者目录：game/Assets/Art/Candidates/Environment/、docs/game/design/common/assets/packs/environment/、本任务结果/交接。不得写角色、船、UI、主场景或正式 Prefab。

开始前读取：项目通用约束、D18 视觉标准、免费来源目录、manifest Schema、导入管线、islands/environment/vegetation 设计、D16-A05 placeholder 队列中圣玛格丽特岛相关条目。

执行内容：
1. 从官方来源选择证据完整的 CC0 包，优先 Quaternius Ultimate Stylized Nature、Poly Haven 和 ambientCG 中与加勒比海岸相符的单项资源；
2. 建立不少于 150 个可复用“模型或模块”候选清单，其中实际下载并完成 Unity 导入预检的样板岛子集不少于 50 个，至少覆盖棕榈/阔叶植物、灌木、草、岩石、悬崖、沙滩、土路、木材、石材、浅水/海床材质；如果合法来源不足，按真实数量交付并标记 PARTIAL，禁止复制变体凑数；
3. 每个包保留官方页、许可证证据、下载日期、原文件名和 SHA-256；移除不需要的重复示例工程、可执行文件和缓存，但不得删许可证；
4. 按 manifest 登记实际文件数、Unity 对象数、材质/贴图、比例、轴向、Pivot、碰撞和 LOD 状态；
5. 生成缩略图/联系表或 Unity 预览截图，便于 D19 选择；
6. 所有资源保持 Candidate/ImportVerified/LocalDevelopmentReady 中的真实状态，不得标记 ReleaseApproved，不得接入 InitialHarbor。

硬约束：
- 禁止来源不明、NC、仅限个人、盗版、游戏提取、混合许可证未逐项确认的资源；
- 不抓取整个 Poly Haven/ambientCG；只下载样板岛明确需要的条目；
- 不提交重复压缩包和解压副本；若保留归档，必须说明必要性和大小；
- 不修改 Registry、content_id、Systems、场景、Prefab、构建设置；
- 不覆盖其他 Agent 文件，不提交或推送 Git。

验证：运行 manifest 审计、Unity 导入审计、缺失引用检查和 git diff --check；记录下载总量、仓库增量、实际资源数、错误/警告。资源过大或 Git 策略不允许时，停止并写明外部缓存方案建议，不能静默提交数 GB 文件。

结果必须保存到：
- 主成果：game/Assets/Art/Candidates/Environment/、docs/game/design/common/assets/packs/environment/、docs/game/design/common/D18_ENVIRONMENT_ASSET_PACKET.md、game/tools/results/D18/D18-A03/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须列出每个包、官方链接、许可、哈希、真实模型/材质数量、导入结果、磁盘增量、未满足数量及原因、PASS/PARTIAL/BLOCKED。
```
</details>

## D18-A04：玩家、NPC、动物与动作免费资源包

<details>
<summary>展开/复制：D18-A04 完整提示词</summary>

```text
你是 WhalesIslandReef 的角色与动物资源 Agent。请直接完成 D18-A04。

任务编号：D18-A04（A* 异步资产任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S01、D18-A01、D18-A02 必须完成。
单一写者目录：game/Assets/Art/Candidates/CharactersAnimals/、docs/game/design/common/assets/packs/characters_animals/、本任务结果/交接。不得写主场景、Animator 正式控制器或其他资源分类。

开始前读取：AGENTS/CONTEXT/规范、D18 标准和管线、character/animals 设计、D10 外观与角色行为成果、D11 动物成果、当前 ph_crew 模型和 D16 placeholder 替换队列。

执行内容：
- 核对并整理 Kenney Blocky Characters、Quaternius Animated Man、Ultimate Animated Animal Pack 等官方 CC0 候选；可补充其他官方 CC0 单项，但必须完整留证；
- 候选库至少覆盖：可作为男女/中性玩家基体的角色、港口平民/商人/水手/军人外观模块、猫狗宠物、鸟类、陆生动物和代表性水生动物；
- 实际导入并预检不少于 20 个可独立使用的角色/动物模型和其真实动画；不足时按真实数量交付并标记 PARTIAL；不得把同一网格换色记成多个独立模型；
- 对每个角色检查身高、骨架层级、Avatar、蒙皮、材质槽、眼睛/头发/服装拆分、Idle/Walk/Run/Interact/Jump 等可用片段；
- 对每个动物检查比例、骨架、Idle/移动/进食/受惊或游泳等可用片段、LOD 和碰撞候选；
- 建立动作片段映射建议和预览截图，但不改正式 Animator、不接入运行时场景；
- manifest 中如实记录缺动画、Generic/Humanoid 兼容性、重定向风险和文化/时代不符项。

硬约束：
- 不使用来历不明的 Mixamo 转存、商业角色生成器导出物或他人购买资产；
- 不把现代服饰、现代装备和明显不符合 1675 年的模型直接列为可用；可登记 Rejected/NeedsModification；
- 不修改 CharacterSystem、AnimalSystem、存档、Registry、InitialHarbor 或正式 Prefab；
- 不做人工 ReleaseApproved，不覆盖其他 Agent 文件，不提交或推送 Git。

验证：manifest 审计、Unity 导入/动画片段审计、缺材质/丢骨骼/异常 bounds 检查、预览场景或 Editor 预览截图、git diff --check。记录真实模型数和动画数。

结果必须保存到：
- 主成果：game/Assets/Art/Candidates/CharactersAnimals/、docs/game/design/common/assets/packs/characters_animals/、docs/game/design/common/D18_CHARACTER_ANIMAL_ASSET_PACKET.md、game/tools/results/D18/D18-A04/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告列出包、许可、哈希、模型/动作数量、时代适配、导入问题、仓库增量、未完成项和 PASS/PARTIAL/BLOCKED。
```
</details>

## D18-A05：船舶、港口建筑、码头与道具免费资源包

<details>
<summary>展开/复制：D18-A05 完整提示词</summary>

```text
你是 WhalesIslandReef 的船舶与港口资源 Agent。请直接完成 D18-A05。

任务编号：D18-A05（A* 异步资产任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S01、D18-A01、D18-A02 必须完成。
单一写者目录：game/Assets/Art/Candidates/ShipsPortProps/、docs/game/design/common/assets/packs/ships_port_props/、本任务结果/交接。不得写主场景、正式船 Prefab、角色/UI 资源。

开始前读取：通用规范、D18 标准/清单/导入管线、ship/buildings/items 设计、D9 港口与航海实现、当前 D5/D7 船和泊位占位、D16 A05 替换队列。

执行内容：
1. 核对 Kenney Pirate Kit、Quaternius Pirate Kit 等官方 CC0 包，并寻找证据完整的 CC0 木船/港口模块补充；
2. 建立不少于 150 个可复用模型或模块的候选目录，实际下载并通过导入预检的样板子集不少于 60 个；至少包含小型可驾驶帆船、远景船、船体/桅杆/帆/舵/锚候选、木码头、栈桥、系船柱、木屋/仓库/市场模块、木桶/箱子/渔具/灯具/绳索/招牌；
3. 船舶候选必须记录船体尺寸、甲板高度、吃水参考、Pivot、前向轴、可拆模块、登船点、舵位、碰撞和动画可能性；
4. 建筑模块必须检查网格闭合、门窗比例、地基、室内可能性、碰撞和拼接尺寸；
5. 道具必须检查抓取/摆放 Pivot、碰撞、材质和批处理/实例化可能性；
6. 生成缩略图、尺寸表、D19/D21 推荐清单，不接入运行时。

硬约束：只接受证据完整的可用许可；不下载盗版船模、商业资产重打包或影视/游戏提取物；不以缩放同一模型冒充不同船型；不修改 ShipSystem/BuildingSystem/Registry/场景/正式 Prefab；不自动 ReleaseApproved；不提交或推送 Git。

验证：manifest、Unity 模型/材质/bounds 审计，异常比例与缺材质报告，下载量和仓库增量报告，git diff --check。达不到数量必须真实标记 PARTIAL，不能伪造。

结果必须保存到：
- 主成果：game/Assets/Art/Candidates/ShipsPortProps/、docs/game/design/common/assets/packs/ships_port_props/、docs/game/design/common/D18_SHIP_BUILDING_PROP_ASSET_PACKET.md、game/tools/results/D18/D18-A05/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告列出包、官方链接、许可、哈希、真实模型数、尺寸/导入问题、仓库增量、推荐用途、未完成项和 PASS/PARTIAL/BLOCKED。
```
</details>

## D18-A06：UI、输入图标、材质、HDRI 与音频免费资源包

<details>
<summary>展开/复制：D18-A06 完整提示词</summary>

```text
你是 WhalesIslandReef 的 UI、材质与音频资源 Agent。请直接完成 D18-A06。

任务编号：D18-A06（A* 异步资产任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S01、D18-A01、D18-A02 必须完成。
单一写者目录：game/Assets/Art/Candidates/UIMaterialsAudio/、docs/game/design/common/assets/packs/ui_materials_audio/、本任务结果/交接。不得修改现有正式 Canvas、场景或其他候选目录。

开始前读取：通用规范、D18 标准/清单/导入管线、ui/audio/environment 设计、D16 本地化/可访问性成果和当前字体、图标、音频、材质配置。

执行内容：
- 核对 Kenney UI Pack、Kenney Input Prompts 的官方 CC0 包，整理键鼠、Xbox 风格通用手柄和导航图标；
- 从 Poly Haven/ambientCG 选择样板港真正需要的 CC0 木材、石材、沙地、岩石、布帆、金属和水岸材质；只下载明确单项，不抓整站；
- 选择 3—5 个可用于加勒比晴天、阴天、黄昏/夜间的 CC0 HDRI 或天空参考，并记录 Unity 使用策略；
- 整理可合法使用的 CC0 环境音/交互音候选；每个音频逐项证明许可证，来源混杂站点不能按网站整体推定；
- 实际导入预检至少 100 个 UI/输入切片或图标、20 套 PBR 材质、30 个音频候选（合法来源不足就 PARTIAL），记录分辨率、通道、压缩、循环点、响度和用途；
- 检查字体许可证；只有 OFL/明确允许商业嵌入的字体可进入候选，必须保留许可证；
- 生成视觉联系表和音频试听清单，不接入正式 UI/场景。

硬约束：不使用未经逐项确认的音频；不把 CC-BY/NC 当 CC0；不抓取整站；不覆盖项目现有字体和输入映射；不修改 Canvas、EventSystem、Systems、Registry 或构建设置；不自动 ReleaseApproved；不提交或推送 Git。

验证：manifest 审计、Unity Texture/AudioImporter 审计、重复文件哈希检查、字体许可证检查、联系表/音频清单、git diff --check。记录真实数量和磁盘增量。

结果必须保存到：
- 主成果：game/Assets/Art/Candidates/UIMaterialsAudio/、docs/game/design/common/assets/packs/ui_materials_audio/、docs/game/design/common/D18_UI_AUDIO_MATERIAL_ASSET_PACKET.md、game/tools/results/D18/D18-A06/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须列出包和单项来源、许可、哈希、真实数量、导入/响度问题、仓库增量、未完成项和 PASS/PARTIAL/BLOCKED。
```
</details>

## D18-A07：2,264 个占位资源的替换映射与优先队列

<details>
<summary>展开/复制：D18-A07 完整提示词</summary>

```text
你是 WhalesIslandReef 的资源替换规划与审计 Agent。请直接完成 D18-A07。

任务编号：D18-A07（异步数据/工具任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S01、D18-A01 完成；应等待 D18-A03—A06 产生候选清单后做最终映射。若候选尚未齐全，只能生成可增量更新的 PARTIAL 队列。
单一写者目录：tools 中 D18-A07 专属工具/测试、docs/game/design/common/assets/replacement/、本任务结果/交接。不得修改 Registry、asset_state、正式资源或场景。

开始前读取：D16-A05 权威审计与 2,264 placeholder 队列、Content Registry、D18 视觉标准、所有 D18 候选 manifest 和现有 d16_a05_replacement_preflight.py。

执行内容：
- 读取权威 Registry/placeholder 队列，生成稳定、可重复的替换映射，不手工复制 2,264 行；
- 每条记录包含 content_id、placeholder_asset、领域、玩法优先级、可见频率、目标候选 pack/file、状态、许可证证据、Prefab/场景消费者、碰撞/锚点/LOD/动画需求、负责阶段和阻断原因；
- 将队列分为 P0 样板闭环、P1 首批可玩内容、P2 规模扩展、P3 长尾；P0 必须覆盖玩家、关键 NPC、圣玛格丽特地形/码头/建筑、起始船、海水/天气、核心 UI；
- 建立只读工具检查一对多/多对一合法性、缺候选、缺证据、重复写者、错误状态、运行时消费者断链；
- 输出每个阶段能替换多少、仍缺多少，以及 D19—D24 的明确责任任务；
- 禁止自动把 candidate 写为 integrated，禁止为了降低 placeholder 数量修改审计口径。

硬约束：Registry 和 asset_state 只读；不伪造映射；同一模型可服务多个内容 ID，但必须明确这是复用而非独立模型；所有强逻辑 ID 和存档兼容保持不变；不修改 Unity 场景/Prefab/Systems；不提交或推送 Git。

验证：工具单元测试、全量当前 Registry 运行、记录输入总数/输出总数/未映射数/重复数/证据缺口，证明重复运行结果稳定；运行 git diff --check。

结果必须保存到：
- 主成果：tools/d18_placeholder_replacement_map.py、对应 tests、docs/game/design/common/assets/replacement/d18_placeholder_replacement_map.json、docs/game/design/common/D18_PLACEHOLDER_REPLACEMENT_MAP.md、game/tools/results/D18/D18-A07/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A07_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须给出真实统计、P0 明细摘要、未映射原因、工具命令、测试结果和 PASS/PARTIAL/BLOCKED。
```
</details>

## D18-S02-v2：资源库准入验收与 D19—D22 受控开发解锁

<details>
<summary>展开/复制：D18-S02-v2 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 资源库准入验收 Agent。请直接审计并完成 D18-S02-v2；不要替未完成的子任务补写虚假结果，也不要把运行时集成证据提前塞进资源库门禁。

任务编号：D18-S02-v2（同步准入验收任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S01、D18-A01—D18-A07 全部有交接报告；允许某个资产包 PARTIAL，但 P0 样板闭环所需资源和管线必须齐备，否则本任务 BLOCKED。
并发规则：验收期间暂停其他 Agent 对 D18 manifest、资源状态、候选目录和替换映射的写入。你可以做最小修复和状态汇总，但不得重做某个资产 Agent 的大范围任务。

开始前读取：AGENTS/CONTEXT/规范、D18 路线和视觉标准、A01—A07 全部主成果/交接/机器结果、D16 A04/A05 当前权威审计、候选目录和 Unity 导入报告。

执行内容：
1. 审计每个资源包的官方来源、许可证证据、哈希、文件、manifest、Unity 导入结果、磁盘增量和状态转换；
2. 运行全量只读 manifest/路径/重复/许可证/导入审计；保留 25 个 manifest、1,857 个候选文件和 A01—A07 原始结果，不删除或覆盖历史；
3. 新建 `d18_prefab_candidate_intake_catalog.json`，只把许可证、哈希、路径、ImportVerified 证据和 A07 映射齐全的候选记为 `intake_eligible=true`；这是受控工作场景/Prefab 制作准入，不是资源状态机晋级；
4. 明确 P0 领域的真实外部候选、缺口、项目自有表现资产责任和后续证据要求；海水、天气、Shader、灯光等没有外部候选的内容不得伪造为资源就绪；
5. 核对 A07 映射与准入目录的可追溯性，禁止 D19—D22 绕过准入目录直接使用未准入文件；
6. 说明旧版 D18-S02 BLOCKED 是运行时就绪验收与后续任务反向依赖造成的历史门禁设计错误；运行时消费者、Prefab、碰撞、锚点、LOD、Animator、保存和截图证据转移到 D18-S03；
7. 不在本任务把资源接入 InitialHarbor，不修改任何 manifest 的真实资源状态，不标记 LocalDevelopmentReady 或 ReleaseApproved。

统一通过门槛：
- Schema/审计工具可运行，测试通过，六种资源状态机没有非法跃迁；
- 25 个 manifest、候选文件、许可证、哈希、路径和 ImportVerified 证据可由本地文件复核；
- 每个可准入 P0 领域都有真实 `intake_eligible` 候选，或明确记录由哪个后续任务创建项目自有表现资产；
- A07 映射和 P0 使用方向可追溯，重复引用、缺失文件、许可证断链和路径逃逸可报告；
- 准入目录不包含 `LocalDevelopmentReady`、`ReleaseApproved` 或正式场景消费者的伪造声明；
- 旧版 BLOCKED 报告和机器结果保留，新版结果使用 v2 命名；
- D19-S01 可在不绕过资源证据的前提下重新执行；D16 人工 ReleaseApproved、目标设备和发布阻断保持原状态。

允许修改：D18-S02-v2 验收报告、准入目录、D18 专属只读工具/测试、任务依赖文档、v2 交接和记忆日志。禁止改玩法 Systems、contracts、主场景、主 UI、Prefab、Registry、存档或构建发布配置；不提交或推送 Git。

验证并记录：所有工具/Unity 测试命令与真实结果、资源数量与磁盘大小、抽检表、警告/阻断、git diff --check。

结果必须保存到：
- 主成果：docs/game/design/common/D18_FREE_ASSET_LIBRARY_ADMISSION_ACCEPTANCE.md、docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json；
- 机器结果：game/tools/results/D18/D18-S02/D18_S02_V2_MACHINE_RESULT.json；
- 交接报告：docs/game/design/common/handoffs/D18/D18-S02_V2_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论只能是 PASS、PARTIAL 或 BLOCKED。`PASS` 只解锁 D19—D22 的受控工作场景/Prefab 开发；不等于 LocalDevelopmentReady、正式 InitialHarbor 集成或 Steam Ready。聊天回复必须列出结论、真实数量、主要风险和结果路径。
```
</details>

## D18 加速修复包：角色、动物、港口资源与准入刷新

> 这是对已完成 D18-S02-v2 的增量修复，不重跑或覆盖历史验收。五个 `A` 任务的唯一共同产物是可复核的证据包和隔离候选目录；它们**不能**修改全局 `d18_prefab_candidate_intake_catalog.json`。只有 `D18-S02-R01` 可以读取这些结果并刷新全局准入目录。这样可以同时交给五个 Agent，而不会互相覆盖。

## D18-A04-R01：既有角色、动物与动作候选的证据复审

<details>
<summary>展开/复制：D18-A04-R01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 角色/动物资源证据复审 Agent。请直接完成 D18-A04-R01，不要顺手改做 D20 Prefab、场景或正式集成。

任务编号：D18-A04-R01（异步、只读审计任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS；D18-A04 的历史结果为 PARTIAL。本任务可与 D18-A04-R02-F/C/B、D18-A05-R01、D19-R01-A 并行。
单一写者：docs/game/design/common/assets/review_packets/D18-A04-R01/、game/tools/results/D18/D18-A04-R01/、docs/game/design/common/handoffs/D18/D18-A04-R01_HANDOFF.md 和当日记忆日志。你只读既有 Candidates、manifest、D18-A04 报告和 Unity 导入结果；不得改全局准入目录、原始 manifest、Prefab、场景、Registry、Systems、存档、contracts/server/web 或 Git 历史。

开始前必须读取：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、D18-S01/D18-S02-v2 规则、D18-A04 主成果和交接、现有 `d18_prefab_candidate_intake_catalog.json`、角色/动物设计、D20-S01 标准，以及已有 D18 审计工具/测试。先记录当前文件哈希、真实导入数、已知缺口和原始结果位置。

执行内容：
1. 对 D18-A04 声称导入的每个角色、动物和动作候选逐项核对本地文件、来源 URL、包内许可证、SHA-256、Unity 导入结果、网格/骨架/动画和实际用途；没有本地或官方证据的项目必须标为缺证据，不能猜测；
2. 按以下消费类别给每个候选做可机读的适配结论：`player_visual_core`、`npc_visual_core`、`wildlife_core`、`pet_core`、`animation_core`，以及 `female_or_neutral_human_gap`、`cat_gap`、`bird_gap` 等明确缺口；
3. 记录尺度、轴向、骨架类型、可用动作、材质槽、LOD/碰撞/锚点缺失、时代风格风险和 D20 可否制作工作 Prefab；这只是建议，不能把 Candidate 伪写成 LocalDevelopmentReady 或 ReleaseApproved；
4. 运行现有 manifest 审计与相关只读测试；必要时添加仅处理本复审证据 JSON 的纯工具/测试，但不得修改导入资源或全局 catalog；
5. 写出“推荐可准入 / 需补证据 / 不适合 / 缺口”清单。推荐并不等于已准入，最终裁决只能由 D18-S02-R01 完成。

验收标准：
- 任何“可用”结论都能定位到真实文件、哈希、许可证证据和 Unity 导入检查；
- 清楚回答当前能否支持玩家、NPC、野生动物、宠物和动作五类 D20 工作；
- 女性/中性人形、猫、鸟的缺口分别有明确真值，不把 Blender 源文件或未导入文件冒充可用资源；
- 审计/测试真实执行并记录命令和结果；git diff --check 通过。

结果必须保存到：
- 主成果：docs/game/design/common/assets/review_packets/D18-A04-R01/D18_A04_R01_CHARACTER_ANIMAL_REVIEW.md 和同目录机器可读 `D18_A04_R01_CHARACTER_ANIMAL_REVIEW.json`；
- 机器结果：game/tools/results/D18/D18-A04-R01/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A04-R01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复必须列出真实复审数量、每类可消费资源、缺口和上述结果路径；不得用聊天回复替代文件。
```
</details>

## D18-A04-R02-F：女性或中性人形候选资源缺口补齐

<details>
<summary>展开/复制：D18-A04-R02-F 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 女性/中性人形资源补齐 Agent。请直接完成 D18-A04-R02-F，只补候选资源和证据，不做 D20 玩家 Prefab 或正式角色替换。

任务编号：D18-A04-R02-F（异步、隔离候选资源任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS。可与 D18-A04-R01、D18-A04-R02-C、D18-A04-R02-B、D18-A05-R01、D19-R01-A 并行；不等待这些任务。
单一写者：game/Assets/Art/Candidates/CharactersAnimals/GapFill/FemaleNeutral/、docs/game/design/common/assets/packs/characters_animals/gap_fill/female_neutral/、game/tools/results/D18/D18-A04-R02-F/、对应交接和记忆日志。不得修改其它 Candidate 子目录、全局准入目录、D20 文件、Prefab、场景、Systems、Registry、存档、contracts/server/web 或 Git 历史。

开始前必须读取：AGENTS.md、CONTEXT、D18-S01/A01/A02/S02-v2、D18-A04 与 D18-A04-R01（若已存在则只读）、D20-S01 标准、现有 manifest schema 与审计工具。先确认现有男性/中性人形是否已满足骨架和许可证要求，再只补真实缺口。

执行内容：
1. 仅从官方来源寻找一套允许项目使用、可重新分发条件明确的女性或中性 Humanoid/Generic 人形候选；必须同时保存官方资源页、直接下载页、包内许可证或许可证快照、下载日期、原始文件名和 SHA-256；不使用 NC、来源不明、仅个人使用、商业游戏提取或无法证明的模型；
2. 下载包先留在已忽略的本地缓存；只把必要候选源文件、许可证和可审计 manifest 放进本任务唯一目录。远程 Git LFS 配额未明确时，不把压缩原包或大批二进制写入普通 Git 历史；
3. 导入 Unity 候选并验证：无编译错误、真实模型存在、尺度/轴向/脚底/骨架/材质/动作可读取；只做候选导入检查，不创建 D20 正式 Prefab、Animator 或场景引用；
4. 生成符合现有 schema 的隔离 manifest 和一份缺口填补说明，明确它仅是 Candidate。记录是否能满足 D20-A01 的第二外观方向、哪些发型/服装仍缺失；
5. 运行 manifest 审计、Unity 导入检查及 git diff --check。若找不到合法资源，仍要交付搜索范围、拒绝原因和可复现证据，结论如实写 PARTIAL/BLOCKED。

验收标准：
- 不伪造哈希、许可证、来源、文件数或导入数；
- 候选拥有可复核的人形网格和 D20 可评估的骨架/材质信息，或真实地报告为何无法获得；
- 隔离 manifest 能通过 schema/审计；没有把资源写成 `intake_eligible`、LocalDevelopmentReady 或 ReleaseApproved；
- Unity Console 无新增错误，结果和风险均可定位。

结果必须保存到：
- 主成果：上述唯一候选目录、docs/game/design/common/assets/packs/characters_animals/gap_fill/female_neutral/D18_A04_R02_F_PACKET.md；
- 机器结果：game/tools/results/D18/D18-A04-R02-F/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A04-R02-F_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。报告给出来源、许可证、哈希、导入数、适配边界和所有未补齐项；D18-S02-R01 才能决定是否准入。
```
</details>

## D18-A04-R02-C：猫候选资源缺口补齐

<details>
<summary>展开/复制：D18-A04-R02-C 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 猫资源补齐 Agent。请直接完成 D18-A04-R02-C，只交付隔离候选和证据，不能做宠物逻辑或正式 Prefab 集成。

任务编号：D18-A04-R02-C（异步、隔离候选资源任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS。可与 D18-A04-R01、D18-A04-R02-F、D18-A04-R02-B、D18-A05-R01、D19-R01-A 并行。
单一写者：game/Assets/Art/Candidates/CharactersAnimals/GapFill/Cat/、docs/game/design/common/assets/packs/characters_animals/gap_fill/cat/、game/tools/results/D18/D18-A04-R02-C/、对应交接和记忆日志。不得改全局准入目录、其它任务目录、D20/D11 代码或 Prefab、正式场景、Systems、存档、contracts/server/web 或 Git 历史。

开始前必须读取：AGENTS.md、CONTEXT、D18-S01/A01/A02/S02-v2、D18-A04、D20-S01、D11 宠物设计、现有 manifest schema/审计工具。检查项目中已有狗或其他宠物候选，确保只补“猫”这一真实缺口。

执行内容：
1. 仅从官方、许可可证实且可用于项目的来源寻找一套猫模型；优先含 Idle/Walk/Run/Rest/Interact 等可评估动作，但不能把缺动作伪报为已具备；
2. 保存官方页、直接下载页、包内许可证或快照、下载日期、原始文件名、SHA-256 与归档内文件清单；不接入 NC、个人限定、来源不明或授权链不可证明资源；
3. 把候选源、许可证和 manifest 仅写入本任务唯一目录，遵守大二进制/LFS 缓存规则；导入 Unity 后核对真实网格、动画、材质、比例、脚底、轴向、bounds 和可用于 D20-A06 的碰撞/跟随评估信息；
4. 生成隔离 manifest、导入审计结果和 `D18_A04_R02_C_PACKET.md`，明确 Candidate 状态、可用动作、缺失 fallback 与 D20-A06 适配建议；不得创建正式宠物 Prefab、修改奖励/好感/生态或写入全局 catalog；
5. 运行 schema/manifest 审计、Unity 导入检查和 git diff --check。若没有合法猫资源，保留真实搜索证据与拒绝原因，不得用狗或其他动物改名冒充猫。

验收标准：来源、许可证、哈希和导入检查一一对应；候选确实是猫或明确无合法候选；所有结论可复核；不出现 `intake_eligible`、LocalDevelopmentReady、ReleaseApproved 的伪状态。

结果必须保存到：
- 主成果：上述唯一候选目录、docs/game/design/common/assets/packs/characters_animals/gap_fill/cat/D18_A04_R02_C_PACKET.md；
- 机器结果：game/tools/results/D18/D18-A04-R02-C/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A04-R02-C_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。报告必须写明真实动作、导入数、宠物接口适配边界和结果路径；D18-S02-R01 是唯一准入裁决者。
```
</details>

## D18-A04-R02-B：鸟类候选资源缺口补齐

<details>
<summary>展开/复制：D18-A04-R02-B 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 鸟类资源补齐 Agent。请直接完成 D18-A04-R02-B，只补可审计的隔离候选；不要把 Blender 源文件、截图或未导入文件冒充 D20 可用模型。

任务编号：D18-A04-R02-B（异步、隔离候选资源任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS。可与 D18-A04-R01、D18-A04-R02-F、D18-A04-R02-C、D18-A05-R01、D19-R01-A 并行。
单一写者：game/Assets/Art/Candidates/CharactersAnimals/GapFill/Bird/、docs/game/design/common/assets/packs/characters_animals/gap_fill/bird/、game/tools/results/D18/D18-A04-R02-B/、对应交接和记忆日志。禁止改全局准入目录、其他候选目录、D20/D11 逻辑或 Prefab、场景、Systems、存档、contracts/server/web 或 Git 历史。

开始前必须读取：AGENTS.md、CONTEXT、D18-S01/A01/A02/S02-v2、D18-A04、D20-S01、D11/D15 鸟类与生态设计、现有鸟类源文件及 manifest schema/审计工具。先写清现有鸟类为何不能用（如仅 Blender 源、未导入、无许可证或缺动作）。

执行内容：
1. 仅从官方且许可可证实的来源寻找一套港口/近海环境可用的鸟类候选；优先有站立、行走、飞行或可降级动作，至少能判断能否用于 D20-A05 的近景或远景表现；
2. 保存官方来源/直接下载、许可证证据、下载日期、原始文件名、SHA-256、归档内文件清单；许可或来源不确定即拒绝，不能猜测；
3. 仅向本任务唯一候选目录导入必要文件和许可证。导入 Unity 并验证网格、材质、动画、比例、轴向、bounds 与 Renderer/Animator 真实存在；不得只保存 `.blend` 而不验证 Unity 侧导入；
4. 写隔离 manifest、导入审计结果和 `D18_A04_R02_B_PACKET.md`，说明每个候选可作为近景鸟、远景鸟或两者皆不可，列出 LOD/飞行 fallback 需求；不得建立正式动物 Prefab 或变更生态刷新；
5. 运行 schema/manifest 审计、Unity 导入检查和 git diff --check。合法来源不足时交付真实未通过原因和检索证据，结果如实 PARTIAL/BLOCKED。

验收标准：候选、许可证、哈希和导入证据可逐项对应；没有把非鸟资源或未导入源文件冒充可用鸟类；全程不改变全局 catalog、正式资源状态或运行时逻辑。

结果必须保存到：
- 主成果：上述唯一候选目录、docs/game/design/common/assets/packs/characters_animals/gap_fill/bird/D18_A04_R02_B_PACKET.md；
- 机器结果：game/tools/results/D18/D18-A04-R02-B/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A04-R02-B_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。报告列出真实类型、动作、导入数、D20-A05 适配边界和结果路径；准入只能由 D18-S02-R01 处理。
```
</details>

## D18-A05-R01：既有船舶、港口建筑与道具候选的证据复审

<details>
<summary>展开/复制：D18-A05-R01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 船舶/港口资源证据复审 Agent。请直接完成 D18-A05-R01；本任务只产生复审证据，不能修改 InitialHarbor、船舶系统、正式 Prefab 或全局资源准入目录。

任务编号：D18-A05-R01（异步、只读审计任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS；D18-A05 的历史结果为 PARTIAL。本任务可与 D18-A04-R01、D18-A04-R02-F/C/B、D19-R01-A 并行。
单一写者：docs/game/design/common/assets/review_packets/D18-A05-R01/、game/tools/results/D18/D18-A05-R01/、docs/game/design/common/handoffs/D18/D18-A05-R01_HANDOFF.md 和当日记忆日志。只读 ShipsPortProps candidates、manifest、D18-A05 结果、D19-S01 规格和现有导入结果；不得改全局 catalog、候选源文件、D19/D21 Prefab/场景、Systems、Registry、存档、contracts/server/web 或 Git 历史。

开始前必须读取：AGENTS.md、CONTEXT、D18-S01/A01/A02/S02-v2、D18-A05 主成果/交接、D19-S01/D19-S02-R01 历史报告、D21 船舶设计、现有 d18 审计工具和全局 intake catalog。先列出当前白色方块港口、船坞、码头、市场和船视觉分别缺什么。

执行内容：
1. 对已下载的港口建筑、码头、道具、船体/船帆候选逐项核对真实文件、来源 URL、包内许可证、SHA-256、Unity 导入、网格/材质和风格适配；
2. 分类输出 `port_structure_core`、`dock_and_shipyard_core`、`market_prop_core`、`ship_visual_candidate`、`sail_rig_gap`、`rudder_anchor_gap` 等建议消费类别；注明哪些能帮助 D19-R01-B、哪些只能留给 D21；
3. 对每项给出缩放/轴向/碰撞/LOD/材质/锚点/船舶动画或时代风格风险，明确哪些资源只能做工作 Prefab，哪些证据缺失不能准入；
4. 运行现有 manifest 审计和只读导入检查，按真实结果生成机器可读 review；不得把 Candidate 改为 intake_eligible，也不得为了港口画面去修改 InitialHarbor；
5. 为 D18-S02-R01 写出可验证的推荐清单、拒绝清单和港口仍缺资源清单；不得伪造船锚、船帆或船坞功能已经实现。

验收标准：每个推荐或拒绝都有可定位的文件/哈希/许可/导入证据；D19 与 D21 的消费边界清晰；不会用许可证不明或未导入资源提高画面；审计结果、git diff --check 和风险清单完整。

结果必须保存到：
- 主成果：docs/game/design/common/assets/review_packets/D18-A05-R01/D18_A05_R01_SHIP_PORT_REVIEW.md 和同目录机器可读 `D18_A05_R01_SHIP_PORT_REVIEW.json`；
- 机器结果：game/tools/results/D18/D18-A05-R01/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A05-R01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复列出真实复审数、可供 D19/D21 消费的类别、未解决缺口和结果路径。
```
</details>

## D18-S02-R01：增量资源准入刷新与消费类别解锁

<details>
<summary>展开/复制：D18-S02-R01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 增量资源准入刷新 Agent。请直接完成 D18-S02-R01。你的工作是把五份隔离复审/补齐结果诚实合并到唯一的 PrefabCandidateIntake 目录；不要重跑、篡改或覆盖历史 D18-S02-v2 PASS，也不要进行运行时晋级。

任务编号：D18-S02-R01（同步、唯一全局准入目录写者）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS，且以下五份交接均存在：D18-A04-R01、D18-A04-R02-F、D18-A04-R02-C、D18-A04-R02-B、D18-A05-R01。各上游可以是真实 PASS/PARTIAL/BLOCKED，但必须留下可读结果，不能用聊天结论代替文件。
并发规则：开始前暂停其它 Agent 对 `docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json`、D18 全局准入报告和全局 intake 工具的写入。你是这些文件的唯一写者；其它 Agent 只能读。

开始前必须读取：AGENTS.md、CONTEXT、D18-S01/A01/A02/S02-v2、当前 intake catalog、五份上游主成果/机器结果/交接、所有相关 manifest、schema、审计工具/测试、D19-S01、D20-S01 和历史 D19-S02-R01 报告。先备份式记录当前 catalog 统计和历史 v2 结论，不删除历史记录。

执行内容：
1. 对五份上游结果逐项运行 schema、哈希、许可证、本地文件和 Unity 导入证据审计；证据不足、许可不明、文件缺失或导入失败时保持 Candidate/排除，不得猜测；
2. 只在现有准入状态机允许且证据真实完整时，把条目写入或更新全局 `d18_prefab_candidate_intake_catalog.json` 的 `intake_eligible` 清单；为每项登记来源 manifest、审核包、消费类别、文件位置、导入限制和责任任务；
3. 单独汇总以下类别的真实可用性：`player_visual_core`、`npc_visual_core`、`wildlife_core`、`pet_core`、`animation_core`、`port_structure_core`、`dock_and_shipyard_core`、`market_prop_core`。不存在的类别必须显式为空并写依赖阻断，不能用近似模型冒充；
4. 不把 Candidate、`intake_eligible` 或可做工作 Prefab 的事实写成 LocalDevelopmentReady、ReleaseApproved 或“正式场景已接入”；这些必须继续等待 D18-S03 与各集成任务；
5. 生成消费门禁表：D19-R01-B 需要哪些港口/环境类别，D20-A01/A03/A05/A06 分别需要哪些角色/动物类别；指出哪些任务可立即启动、哪些仅能 PARTIAL/BLOCKED；
6. 运行全量 D18 manifest/intake 审计、相关 Unity 导入检查、现有单元测试和 git diff --check；记录命令、真实数量、错误和排除原因。

通过门槛：
- 五份上游结果均被处理，并且全局 catalog JSON/schema/审计全部通过；
- 每个新增 `intake_eligible` 条目都能回溯到本地文件、哈希、许可证、导入和复审来源；
- 许可或导入不完整的条目保持不准入；没有把“数量很多”当作“角色/猫/鸟/港口问题已经解决”；
- 输出明确、可执行的 D19/D20 消费门禁表；历史 D18-S02-v2 和 D18-S03 的边界不被破坏。

结果必须保存到：
- 主成果：更新后的 docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json、docs/game/design/common/D18_FREE_ASSET_INTAKE_REFRESH_R01.md；
- 机器结果：game/tools/results/D18/D18-S02-R01/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-S02-R01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。`PASS` 仅表示“本轮 catalog 刷新真实、完整、可复核”，不表示所有缺口都有资源，更不表示 LocalDevelopmentReady；聊天回复必须列出类别计数、不可用类别、放行/阻断的 D19/D20 任务和结果路径。
```
</details>

## D18 R02 修复包：真实人形 Avatar、玩家外观资源与准入刷新

> 历史 D18-S02-R01 的 PASS 仍有效，但它只证明 catalog 刷新正确，不证明玩家模型已经是 Humanoid，也不证明发型/服装已经可用。本组只补这两个事实并刷新准入目录。两个 A 任务的写入目录完全隔离；只有 D18-S02-R02 可以更新全局 `d18_prefab_candidate_intake_catalog.json`。

## D18-A04-R02-F-R01：女性/中性人形的真实 Unity Humanoid 与 Avatar 修复

<details>
<summary>展开/复制：D18-A04-R02-F-R01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 玩家人形资源修复 Agent。请直接完成 D18-A04-R02-F-R01：把历史 D18-A04-R02-F 的“候选但 NotImported”问题修成可被 D20 检查的真实 Unity 导入证据。不要开始 D20 玩家 Prefab、Animator、场景或正式集成。

任务编号：D18-A04-R02-F-R01（异步、隔离候选资源修复任务）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D18-S02-v2 PASS、D18-S02-R01 PASS，且历史 D18-A04-R02-F 的主成果和交接存在。可与 D18-A04-R03-P、D19-R02-A01/A02/A04/A05 并行。
单一写者：game/Assets/Art/Candidates/CharactersAnimals/GapFill/FemaleNeutral/、docs/game/design/common/assets/packs/characters_animals/gap_fill/female_neutral/、game/tools/results/D18/D18-A04-R02-F-R01/、docs/game/design/common/handoffs/D18/D18-A04-R02-F-R01_HANDOFF.md 和当日记忆日志。不得修改其它 Candidate 子目录、全局 intake catalog、D20 Prefab/Animator、InitialHarbor、Systems、Registry、存档、contracts/server/web 或 Git 历史。

电脑独占规则：本任务必须实际启动 Unity 导入/Avatar 验证。开始前先明确告知用户：“本任务需要独占电脑进行 Unity 导入与 Avatar 实测，请保存其他工作并停止使用键盘鼠标；收到允许接管后再开始。”未获允许时只做只读预检并如实写 PARTIAL；获允许后不得与任何其它 Unity/Player Agent 同时操作本项目。

开始前必须读取：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、D18-S01/A01/A02/S02-v2/S02-R01、历史 D18-A04-R02-F 的 manifest/机器结果/交接、D20-S01 和 D20 角色标准第 3 节、现有 manifest schema、资产审计工具及最新 intake catalog。先记录旧候选的 URL、许可证证据、SHA-256、文件路径、Unity 导入状态与旧结果；旧包页面若不是官方来源，必须重新核验，不得沿用猜测的来源链。

执行内容：
1. 对历史女性/中性候选逐项用官方资源页、直接下载页、包内许可证、原始归档 SHA-256 和本地文件核验来源。来源/许可/哈希任一不成立即保持排除，不得把非官方镜像或无法证明的文件写成合格；
2. 在隔离目录内把必要模型实际导入 Unity。逐项记录真实 FBX/模型文件、Importer 的 Animation Type、Avatar 配置、Avatar 是否存在且 valid、Humanoid 映射/T 或 A Pose、骨骼映射、网格/材质/clip 数、bounds、脚底、轴向和尺度；必须从 Unity 实际状态读取，不能由文件名推断；
3. D20 的 P0 条件是至少一个可合法使用的人形候选在 Unity 中同时满足：Animation Type=Humanoid、Avatar 已生成且 valid、主要骨骼映射可读、无新增编译/导入错误。若历史候选都是 Generic/NoAvatar，可在相同隔离范围内从官方且许可可证明的来源补一个新候选，但不得把 Generic 误报成 Humanoid；
4. 只为候选制作导入检查场景或 Editor 检查，不创建 D20 PlayerVisual、Animator、正式材质、场景引用或 ResourceKey 解析。女性/中性表达是外观方向，不得修改身份、关系、特质或任何存档字段；
5. 生成机器可读的 Humanoid/Avatar 证据表，区分“已导入但 Generic”“Humanoid 但 Avatar 无效”“Humanoid+Avatar 通过”“未导入/许可证失败”。运行 manifest/schema 审计、Unity 导入检查和 git diff --check。

通过门槛：
- 每个声称可用的候选都有真实本地文件、SHA-256、官方许可链和 Unity 导入记录；
- 至少一个候选实际通过 Humanoid+valid Avatar 检查，或在报告中精确说明为什么无法合法达到该条件；
- 没有把 Candidate、intake_eligible、LocalDevelopmentReady 或 ReleaseApproved 混为一谈；
- 不产生 D20 Prefab、正式场景、逻辑/存档/Registry 改动。

结果必须保存到：
- 主成果：docs/game/design/common/assets/packs/characters_animals/gap_fill/female_neutral/D18_A04_R02_F_R01_HUMANOID_AVATAR_PACKET.md、同目录 D18_A04_R02_F_R01_HUMANOID_AVATAR_PACKET.json，以及必要的隔离 candidate/import metadata；
- 机器结果：game/tools/results/D18/D18-A04-R02-F-R01/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A04-R02-F-R01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复必须写明是否实际获得电脑独占、Humanoid+valid Avatar 的真实数量、每项失败原因和上述结果路径；不得用聊天结论替代文件。
```
</details>

## D18-A04-R03-P：玩家发型与 1675 服装模块候选补齐

<details>
<summary>展开/复制：D18-A04-R03-P 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 玩家发型与时代服装资源补齐 Agent。请直接完成 D18-A04-R03-P，只交付隔离候选、许可证、导入证据和可消费清单；不要把资源提前组装为 D20 玩家 Prefab，也不要改任何角色数据或正式场景。

任务编号：D18-A04-R03-P（异步、隔离玩家外观资源任务）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D18-S02-v2 PASS、D18-S02-R01 PASS。可与 D18-A04-R02-F-R01 和 D19-R02-A01/A02/A04/A05 并行；不等待 D20。
单一写者：game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/、docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/、game/tools/results/D18/D18-A04-R03-P/、docs/game/design/common/handoffs/D18/D18-A04-R03-P_HANDOFF.md 和当日记忆日志。不得修改 FemaleNeutral 目录、全局 intake catalog、D20 PlayerVisual、AppearanceSystem、InitialHarbor、Systems、Registry、存档、contracts/server/web 或 Git 历史。

电脑独占规则：本任务需要 Unity 导入和网格/材质检查。开始前先明确通知用户需要独占电脑；用户确认后才启动 Unity。若未获独占，只能做来源/文件只读工作并报告 PARTIAL，不能把未导入资产写成已验证。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D18-S01/A01/A02/S02-v2/S02-R01、D20-S01 的外观组合边界、D10 AppearanceLoadout/资源 key 规则、历史 D20-A01 的缺口表、manifest schema 与审计工具。先记录现有逻辑槽位和已存在资源，避免重复下载或用颜色变化冒充独立几何。

执行内容：
1. 只从官方且可证明项目使用/修改/分发条件的来源筛选发型、帽子、上衣、下装、外套、鞋靴、手套和职业小件候选；保留资源页、直接下载页、包内许可证/快照、下载日期、原始文件名、版本与 SHA-256。拒绝 NC、个人限定、来源不明、商业游戏提取和无法再分发的资源；
2. 目标是为 D20-A01 提供“至少六个可独立辨识发型槽位”和“至少八个时代合理服装组合所需模块”的候选接口。颜色/材质变化可作为变体，但不得被计作独立发型或独立服装几何；实际不足时按真实数量报告；
3. 在唯一 Candidate 目录导入必要文件并实际检查网格、材质、bounds、轴向、尺度、可挂接部位和现代物件风险。它们不需要自行成为 Humanoid，但必须明确能否与已验证的人形骨架挂接；没有兼容性证据时标为候选而非可消费；
4. 为每个模块登记建议 AppearanceLoadout 槽位、资源类别、时代说明、依赖的骨骼/锚点、遮挡冲突（帽子/发型、外套/上衣等）、材质数和缺失回退。不得新增第二套外观数据或改动 D10 的所有权；
5. 生成 D18_A04_R03_P_PLAYER_APPEARANCE_PACKET.md 和 JSON，运行 manifest/schema 审计、Unity 导入检查及 git diff --check。原始大压缩包继续放被忽略的本地缓存；远程 LFS 配额不明时不得写入普通 Git 历史。

通过门槛：
- 每个被计数的发型/服装模块有来源、许可证、SHA-256 和实际 Unity 导入/检查记录；
- 清楚区分独立网格、材质变体、未兼容候选和真实缺口；
- 不把 Candidate 写成 intake_eligible/LocalDevelopmentReady/ReleaseApproved，不创建正式 D20 Prefab；
- 所有结果、测试和风险可复核。

结果必须保存到：
- 主成果：docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/D18_A04_R03_P_PLAYER_APPEARANCE_PACKET.md、同目录 D18_A04_R03_P_PLAYER_APPEARANCE_PACKET.json，以及必要的隔离 Candidate/manifest 文件；
- 机器结果：game/tools/results/D18/D18-A04-R03-P/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-A04-R03-P_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复列出实际独立发型数、服装模块/组合数、Humanoid 挂接兼容性、电脑独占是否完成、未解决缺口和结果路径。
```
</details>

## D18-S02-R02：R02 人形/外观增量准入刷新与 D20 消费门禁

<details>
<summary>展开/复制：D18-S02-R02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 R02 资源准入刷新 Agent。请直接完成 D18-S02-R02：只把两份新的隔离人形/外观证据诚实合并到唯一 PrefabCandidateIntake catalog，并给 D20-A01-R01 发布明确消费门禁。不要重跑或改写历史 D18-S02-v2、D18-S02-R01，也不要进行运行时晋级。

任务编号：D18-S02-R02（同步、唯一全局准入目录写者）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D18-S02-v2 PASS、D18-S02-R01 PASS，且 D18-A04-R02-F-R01 与 D18-A04-R03-P 的主成果/机器结果/交接均存在。两项可以是 PARTIAL，但文件必须可读；不得用聊天回复替代。
并发规则：开始前暂停所有 Agent 对 docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json、D18 全局 intake 报告和全局 intake 工具的写入。你是这些文件的唯一写者；其它任务只能读。

电脑独占规则：本任务需要复跑 Unity 导入核验时，先明确通知用户并取得独占电脑许可；同一时间不得有其它 Agent 启动 Unity。若可复核的上游 Unity 结果已经存在，可不重复打开 Unity，但必须说明采用了哪些既有真实证据。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D18-S01/A01/A02/S02-v2/S02-R01、当前 intake catalog、两份 R02 上游 packet/JSON/交接、相关 manifest/schema/审计工具、D20-S01 和 D20-A01 的历史 interface。先记录 R01 catalog 的数量和历史结论，不删除失败或 PARTIAL 证据。

执行内容：
1. 对人形与外观候选逐项复核本地文件、SHA-256、官方许可证、Unity 导入和上游审计。缺任一核心证据时维持 Candidate/排除，不得因为 D20 需要而放宽条件；
2. 对 D20 玩家核心人形额外核对 Animation Type=Humanoid、Avatar 实际存在且 valid、骨骼/姿态证据可读。仅在这些条件真实满足时才可把该条目作为 D20-A01-R01 的玩家核心候选；Generic/NoAvatar 只能保留为非核心候选或回退来源；
3. 只按现有 catalog 状态机更新 docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json。若既有 schema 没有记录 Humanoid/Avatar 的字段，不得静默改坏 schema；改用配套 D18_R02_PLAYER_CONSUMPTION_GATE.json 明确列出 content_id、manifest、文件、Avatar 证据、可消费/不可消费理由；
4. 更新 D18_FREE_ASSET_INTAKE_REFRESH_R02.md，分别汇总 player_visual_core、player_appearance_module、npc_visual_core、wildlife_core、pet_core 的真实数量。明确 D20-A01-R01 是否放行、哪些发型/服装仅为可选候选、哪些仍为缺口；
5. 运行全量 manifest/intake 审计、相关单元测试、必要的 Unity import check 和 git diff --check。不得把 intake_eligible 或可做工作 Prefab 写成 LocalDevelopmentReady/ReleaseApproved。

通过门槛：
- 两个上游包都被处理，catalog JSON/schema/审计通过；
- 每个新增 intake_eligible 条目可回溯到本地文件、哈希、许可证、Unity 证据和上游 packet；
- D20-A01-R01 是否可开工有一个机器可读、无歧义的结论；如果没有 valid Avatar，必须明确 BLOCKED；
- 历史 R01 的 catalog/结果被保留，D18-S03 边界未被提前突破。

结果必须保存到：
- 主成果：更新后的 docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json、docs/game/design/common/D18_FREE_ASSET_INTAKE_REFRESH_R02.md、docs/game/design/common/assets/D18_R02_PLAYER_CONSUMPTION_GATE.json；
- 机器结果：game/tools/results/D18/D18-S02-R02/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-S02-R02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。PASS 仅代表 catalog/消费门禁刷新真实正确；聊天回复必须写 D20-A01-R01 是否放行、Humanoid+valid Avatar 数、外观模块数、电脑独占是否实际使用和结果路径。
```
</details>

## D18-S03：运行时消费者与 LocalDevelopmentReady 晋级验收

<details>
<summary>展开/复制：D18-S03 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D18 运行时资源晋级验收 Agent。请直接完成 D18-S03。

任务编号：D18-S03（同步运行时资源验收任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS；D19-S02、D20-S02、D21-S02、D22-S02 已有工作场景、Prefab、正式集成和 PlayMode 回归证据。

执行内容：
- 读取 `d18_prefab_candidate_intake_catalog.json`，只检查其中 `intake_eligible=true` 的候选；不扫描并批准目录外文件；
- 逐项验证真实 Prefab/工作场景引用、碰撞、交互锚点、LOD、Animator/材质/音频消费者、保存/恢复、截图、性能和回滚入口；
- 验证 D19—D22 的运行时消费者不是 UI、Agent 或复制出来的第二套逻辑；不修改 Systems、Registry、存档或 contracts；
- 对全部证据齐全的条目更新 `d18_local_development_ready_catalog.json` 的真实晋级结果，保留原始 manifest 状态和历史报告；
- 任何证据缺失保持 Pending/不进入 LocalDevelopmentReady；禁止标记 ReleaseApproved；
- 对项目自有海水、天气、Shader、灯光和其他无外部候选表现资产，使用项目资产/责任任务证据单独登记，不把它们伪造为外部资源条目；
- 生成逐项失败原因、责任任务、回退入口和 D23/D24 可用边界。

验收门槛：
- 每个晋级条目有可定位 Prefab/工作场景/运行时消费者；
- 碰撞、锚点、LOD、Animator/材质/音频和交互行为按类别有证据；
- 保存/读档、断开/恢复、截图和性能采样可复核；
- LocalDevelopmentReady 只由真实证据决定；ReleaseApproved 仍只能由责任人类审核者确认；
- D23 及后续最终集成门禁等待 D18-S03 PASS。

结果必须保存到：
- 主成果：更新后的 docs/game/design/common/assets/d18_local_development_ready_catalog.json、docs/game/design/common/D18_RUNTIME_ASSET_PROMOTION_ACCEPTANCE.md；
- 机器结果：game/tools/results/D18/D18-S03/；
- 交接报告：docs/game/design/common/handoffs/D18/D18-S03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能是 PASS、PARTIAL 或 BLOCKED；不得把 D18-S02-v2 的准入 PASS 解释为运行时资源晋级 PASS。
```
</details>
