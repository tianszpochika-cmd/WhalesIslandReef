# D16-A06 发布技术准备报告

## 结论

状态：**IN_PROGRESS，发布构建与本地 staging 已验证，正式发布流程仍阻断**。

D15-S02 已通过，Save.Tests 已可构建，Windows 构建和 workspace staging 安装/更新/卸载烟测已有证据；正式 installer/update 脚本、人工审查收口和 Steam 授权仍未完成。因此本报告不把本地 staging 或构建烟测标记为正式发布通过。

## 已读取的发布基线

| 范围 | 当前基线 | 本任务状态 |
|---|---|---|
| Unity/目标平台 | Unity 6000.0.80f1；EditorBuildSettings 含 MainMenu、Bootstrap、InitialHarbor | 已确认基线，未完成发布构建 |
| 产品标识 | companyName=Whalebone Studio；productName=WhalesIslandReef；bundleVersion=1.0 | 已确认基线，未完成最终版本冻结 |
| D5 Steam 桥接 | D5SteamSystem 支持离线模式、成就本地状态、云存档待上传队列；默认 AppId 为 UNCONFIGURED | 仅本地接口预留 |
| 商店目录 | D5ReleaseCatalog 有中英文短描述、年龄提示和必需素材名称清单 | 仅元数据模板，未完成素材授权/版本闭包 |
| Agent 降级 | AgentCore 已有本地 Ollama、可选云客户端、路由器和 TemplateFallback；模板回复 AffinityChange=0、Action=none | 代码基线存在，D16 发布级验证未运行 |
| 存档位置 | SaveSystem 使用 Application.persistentDataPath/save.db | 已确认路径；D16 文件级备份/恢复未完成 |
| 迁移保护 | D8 SaveMigration 有事务、重复迁移、未知版本和部分 JSON 默认值入口 | 受 Save.Tests 编译阻断，不能宣称可用 |
| 崩溃记录 | Unity 工程的 enableCrashReportAPI=0；未找到 D16 隐私裁剪后的发布崩溃收集器 | 未完成 |
| 安装/更新 | 未找到 D16 安装器、卸载器、补丁或版本清单脚本 | 未完成 |

## D15-S02 门禁阻断

本次复核继续发现：

1. D15-A06 资源登记缺 historical_review、review_evidence_refs、content_warning_ids、skip_allowed 等字段，部分还缺 LOD/审查状态；
2. D15-A02 有两个生境字段冲突；
3. D15-A07 有 18 次引用指向 12 个未定义宠物外观 ID；
4. D15-A08 任务/音频缺少测试、LOD-N/A 或资源状态元数据；
5. A01-A07 有 2536 个 display key 没有可验证的中英 locale 包证明；
6. 24 个抽样中有 9 个没有明确的运行时 save/restore 标记。

这些问题属于 D15 内容 QA、资产、审查、本地化和对应所有权系统，不在 D16-A06 中越权修复。未删除问题内容，也未把 placeholder 或未授权资源放进正式构建。

## 发布能力矩阵

| 能力 | 当前证据 | 结论 | 重新开工后的验收 |
|---|---|---|---|
| Windows 构建配置 | Unity 版本、场景清单、产品版本已存在 | BLOCKED | 在 D15-S02 通过后生成可追溯构建，记录版本、提交、场景、构建日志和产物摘要 |
| 配置校验 | Content Registry 与 D15 QA 可扫描内容；未发现独立 D16 release manifest 校验器 | PARTIAL | 增加或确认发布清单校验：版本、场景、资源状态、授权、敏感内容、locale、存档迁移和依赖 |
| 崩溃日志 | Unity 原生 crash handler 只说明编辑器/平台可生成报告；工程 CrashReport API 当前关闭 | NOT READY | 明确本地报告目录、脱敏字段、用户同意/删除路径、大小上限、崩溃后不覆盖存档的测试 |
| 隐私 | 未找到 D16 发布级隐私说明和日志字段清单 | NOT READY | 在发布文档中冻结不采集内容、脱敏规则、保存期限、删除/导出说明；无外部上传则明确离线行为 |
| 离线模式 | D5SteamSystem 有 OfflineMode 和离线状态文本；Save/D5 状态保存在本地 | PARTIAL | 断网启动、断网游玩、保存、重启、恢复和重新在线的回归测试；不改变领域裁决 |
| Template Agent 降级 | TemplateFallback 只生成中性回复、零亲密度变化、none 行动 | PARTIAL | 验证模型不可用时只替代表达，不写关系/奖励/天气/经济/战斗等强逻辑结果 |
| 安装 | 未发现 D16 安装脚本或安装包 | NOT READY | 运行全新安装、已有存档安装和路径含中文/空格的安装测试 |
| 卸载 | 未发现 D16 卸载脚本或文件保留规则 | NOT READY | 验证卸载不删除用户存档；提供明确的用户主动删除入口和提示 |
| 更新 | 未发现 D16 更新/补丁脚本或版本迁移清单 | NOT READY | 从 D7、D8/v3 和当前版本更新，保留字段、备份原档、失败可恢复 |
| 存档备份 | SaveSystem 使用 SQLite；D8 迁移有事务，但没有 D16 文件级备份/临时提交/恢复演练 | BLOCKED | 写入前备份、临时文件、原子替换、断电/写入中断、备份轮换和人工恢复测试 |
| 商店素材 | D5 仅有素材名称清单和中英文短描述；D16-A04 授权闭包未完成，D16-A05 仍有 placeholder | BLOCKED | 每项素材有来源、权利人、许可、版本、修改权、敏感审查状态和构建纳入证明 |
| Steam 发布 | D5 清单仍要求开发者账号、真实 App ID、SDK、上传和审核 | NOT EXECUTED | 需要用户/发布负责人授权后由人工执行；本 Agent 不上传、不发布、不发送外部消息 |

## 数据边界与降级规则

- 发布技术层只能读取构建、配置、存档、审查和表现状态；不成为 Player、Ship、Crew、Relation、Finance、Quest、WorldEvent 或 Reward 的写者。
- 离线模式只切换外部服务可用性和表现提示；不得改变天气、关系、牌局胜负、奖励、债务、生态或战斗裁决。
- TemplateFallback 只提供表达层回复和 none 行动；任何 Agent 输出仍必须通过既有 AgentReply 校验和所属系统裁决。
- 崩溃日志只能保留必要的版本、构建、平台、错误类别、非敏感状态摘要和用户主动提供的诊断信息；不得默认收集姓名、完整路径、账号密钥、对话原文或存档正文。
- 安装、卸载、更新和备份层不得删除 D7 字段，不得在迁移失败时覆盖原始存档，不得重复结算奖励或世界事件。
- 云存档只保留 D5 的本地待同步接口；本任务没有接入外部服务或新增跨端契约。

## 已知问题与责任任务

| 问题 | 责任范围 | 当前处理 |
|---|---|---|
| D15 内容 QA 1326 errors/10 warnings | D15-S02 及对应 A01-A08 | 历史阻断，已由 D15 顺序复验关闭 |
| A06 资源审查/授权字段缺失 | D15-A06、D16-A04 | D16-A04 已登记，尚未关闭 |
| 宠物外观孤儿引用 | D15-A07 | 待责任任务补齐引用或定义 |
| locale 包证明不足 | D15-A01-A07/A08 | 待本地化内容任务补齐 |
| 9 个样本缺运行时存档恢复标记 | D15-S02，后续 D16-A02 | 待 Save/QA 运行时验证 |
| CrewAgent.cs 缺 D10CrewBehaviorState | D10 关系/Agent 工程依赖 | 当前阻断 Save.Tests；D16-A06 不越权修复 |
| 无文件级备份/恢复实测 | D16-A02 Save/Storage | 未开始 |
| 无发布级 crash/privacy 方案 | D16-A06 | 当前只登记，不宣称完成 |
| 无安装/卸载/更新器 | D16-A06 发布工程 | 当前只登记，不生成未验证脚本 |
| 未授权/placeholder 正式表现 | D16-A04/A05、D15 资产 | 禁止进入正式构建 |

## 本次未执行的动作

- 未修改 game/、Systems、AgentCore、Save/Storage 或 contracts；
- 未创建安装器、卸载器、更新器、崩溃上传器或外部服务接入；
- 未执行具有破坏性的安装/卸载/覆盖更新测试；
- 未生成或上传 Steam 构建、商店素材、云存档或外部消息；
- 未将 D5 的模板或接口预留误报为 D16 发布通过；
- 未删除 D15 问题内容来降低 QA 数量。

## 重新开工门槛

1. D15-S02 统一 QA 错误归零并重新出具 PASS 报告；
2. D16-A04 授权/审查阻塞项关闭，或每条资源明确保持 Pending 且从正式构建排除；
3. 修复 D10CrewBehaviorState 工程依赖并使 Save.Tests 可构建；
4. 先完成 D16-A02 文件级备份、断电恢复、迁移失败和重复结算保护；
5. 冻结 D16 发布 manifest、版本号、构建场景、资源授权、locale、敏感审查和存档兼容矩阵；
6. 再实现并验证隐私安全的崩溃记录、安装/卸载/更新流程和离线/模板降级；
7. Steam 真实发布动作必须由用户或发布负责人单独授权，不能由本任务自动执行。

## 验收结论

D16-A06 **未通过，发布准备不得开始异步执行**。本报告完成的是可审计的发布基线和阻断登记，不是构建、安装、更新或商店发布验收。

## 2026-08-12 本地隐私诊断子门禁

本轮补齐了一个可审计的本地运行时诊断最小实现，但不把它扩大解释为发布级崩溃系统：

- `D16LocalDiagnostics` 在场景加载前安装为持久对象，默认不启用；只有设置层显式调用 `GrantConsent()` 后才监听 Error、Exception、Assert。
- `D16DiagnosticRecord` 只输出固定的非内容字段与统计值。原始 condition、stack trace、用户路径、存档正文和对话文本不写入 NDJSON；记录目录固定在 `Application.persistentDataPath/D16Diagnostics`，单文件 512 KiB 后轮转。
- `RevokeConsentAndClear()` 提供撤回同意并删除本模块两个固定诊断文件的入口；诊断写入失败会被隔离，不能反向影响游戏或存档。
- Unity 6 PlayMode `D16LocalDiagnosticsTests`：**3/3 PASS**；`Assembly-CSharp` 与 `Assembly-CSharp-Editor`：**0 warning / 0 error**。
- `D16ReleaseManifest.json` 已明确 `external_upload=false`、`local_opt_in=true`、`raw_message_collection=false`、`raw_stack_collection=false` 和删除入口；静态发布审计复跑 `missing_static_evidence=[]`。

未关闭项：设置界面和用户可见隐私说明尚未接入、尚无原生崩溃转储/目标设备验证，且仍需人工隐私审查。A06 仍为 **IN_PROGRESS**，发布清单继续保持 `blocked`，没有 Steam 上传或外部服务动作。

## 2026-08-11 门禁复验更新

D15-S02 已通过；D10 AgentCore/Systems/Save.Tests standalone build 均为 0 warning / 0 error。D16-A06 当前状态更新为 **PENDING**：Unity 构建配置、安装/卸载、覆盖更新、隐私与崩溃记录、文件级存档恢复、发布 manifest 和商店素材闭包仍未完成；未执行 Steam 上传或任何外部发布动作。

## 2026-08-11 静态发布审计更新

当前状态：**IN_PROGRESS，发布构建/安装更新实测未完成**。

## 2026-08-11 静态发布审计更新

重新执行 `tools/d16_a06_release_audit.py`：项目版本、Windows 产品标识、必需场景、默认崩溃 API 关闭、离线桥接、TemplateFallback、文件级存档安全和 installer/update 脚本检查通过；仍缺 `Assets/Resources/D16ReleaseManifest.json`，并且 Unity 目标构建、安装/卸载/覆盖更新和 Steam 授权动作未实测。A06 继续 IN_PROGRESS。

补充：已落地 `game/Assets/Resources/D16ReleaseManifest.json`，并明确 `release_status=blocked`、Steam 未授权/未执行和六类阻断原因。最新 A06 静态审计的 `missing_static_evidence` 已为空；Unity 目标构建、安装/卸载/覆盖更新和 Steam 授权仍未实测。

审计器现额外校验清单内容完整性（版本、场景、阻断原因和 Steam 双未执行），对应测试已更新为 **4/4 PASS**；最新清单状态仍为 `blocked`。

新增 `tools/d16_a06_release_audit.py` 与 `tools/results/d16_a06_release_audit.json`。静态检查确认 Unity 版本、产品标识、MainMenu/InitialHarbor 场景、CrashReport API 默认关闭、D5 离线桥接、Agent TemplateFallback 和 SaveFileSafety 均存在；仍缺正式运行时 release manifest，且 Unity 构建、安装/卸载/覆盖更新未执行。Steam 上传保持未授权、未执行。

发布清单草案见 `D16_RELEASE_MANIFEST_TEMPLATE.json`，状态明确为 blocked，不是可发布构建凭证。脚本单测已通过。

## 2026-08-11 当前 A06 证据复验

以 `game` 作为 Unity 项目根目录重新运行 `tools/d16_a06_release_audit.py`，避免把工作区根目录误当作项目根目录。当前静态检查全部通过：必需场景、离线 Steam 桥接、TemplateFallback、文件级存档安全、blocked release manifest、Windows 构建证据和 workspace staging 安装/更新/卸载证据均为 `true`。

当前静态审计已无缺失项；新增的 `tools/d16_a06_install_update.ps1` 提供工作区限定的 Install/Update/Uninstall 流程，三步实测均 PASS 并保留独立 user-data 目录。它不等价于签名 OS 安装包、注册表/权限集成或 Steam 流程验收；当前唯一发布阻断为政策性 `steam_upload_not_authorized`，Steam 上传和外部发布仍未执行。

## 2026-08-12 用户可见隐私入口子门禁

### 变更

- 新增 `game/Assets/Scripts/Gameplay/UI/D16/D16LocalDiagnosticsPanel.cs`，使用独立的高层模态 Canvas（sorting order 900），明确展示本地诊断的默认关闭、脱敏字段、保存范围、轮转上限和删除边界。
- 主菜单“设置”新增 `BtnDiagnostics`，暂停菜单新增“本地诊断与隐私”入口；面板打开时只锁定玩法输入，不覆盖或重建已有 EventSystem，关闭后恢复输入焦点。
- `ConsentToggle` 调用 `D16LocalDiagnostics.GrantConsent/RevokeConsent`；`DeleteAndDisable` 调用 `RevokeConsentAndClear`，只处理本模块的两个固定本地文件。
- 中英文文案登记在 `game/Assets/Scripts/Gameplay/UI/Localization.cs`，未把原始错误、堆栈、存档正文或对话内容展示给用户，也没有新增网络或外部收集。

### 真实验证

- Unity 6 PlayMode 单项：`D16LocalDiagnosticsPanelTests.D16A06PanelSupportsOptInDeleteAndClose` **1/1 PASS**，结果 `tools/results/d16_a06_privacy_panel_playmode_unity6.xml`。
- Unity 6 D16 PlayMode 全量：**11/11 PASS**（含新增隐私面板用例），结果 `tools/results/d16_a06_privacy_full_d16_playmode_unity6.xml`。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：**0 warning / 0 error**。
- `python -m unittest tools.tests.test_d16_a04_a06_audits -v`：**12/12 PASS**。
- `python tools/d16_a06_release_audit.py game --json-out tools/results/d16_a06_release_audit.json`：静态检查全部为 `true`，`missing_static_evidence=[]`，阻断仍为 `steam_upload_not_authorized`。

### 边界与待办

- 该子门禁只证明用户能看见并控制本地诊断同意/删除入口；不等于原生崩溃转储、目标设备、正式 OS 安装器或人工隐私审查已完成。
- `D16ReleaseManifest.json` 继续保持 `release_status=blocked`；没有执行 Steam 上传、外部服务接入或外部消息发送。
- D16-A06 总体继续 **IN_PROGRESS**，D16-S01/S02 仍不能启动。

## 2026-08-12 安装更新烟测复验

- 重新执行 `tools/d16_a06_install_update_smoke.py`，仅操作 `artifacts/d16_a06_install_update_smoke/` 隔离目录。
- 安装 180 个文件、覆盖更新 180 个文件；Install、Update、Uninstall 均 PASS，更新和卸载后独立用户存档哨兵均保留。
- 最新 `tools/results/d16_a06_release_audit.json`：所有本地静态、Windows 构建和工作区烟测检查为 `true`，`missing_static_evidence=[]`，唯一阻断仍为 `steam_upload_not_authorized`。
- 该证据不等价于签名 OS installer、真实权限/注册表卸载器、目标设备崩溃验证、最终人工隐私审查或 Steam 发布；D16-A06 总体保持 **IN_PROGRESS**。

## 2026-08-12 D16-R02 安装更新安全回归

- 加固 `tools/d16_a06_install_update.ps1`：`Update` 先把旧安装移入隔离回滚目录，只有新 payload 成功切换后才清理旧版本；切换失败会恢复旧版本并清理半成品。`Install` 遇到已存在目标时拒绝覆盖，要求显式使用 `Update`。
- 新增 `tools/tests/test_d16_a06_install_update_script.py`，实际调用 Windows PowerShell 验证 Install → Update → Uninstall、已有安装拒绝覆盖和独立用户存档保留：**1/1 PASS**。
- 重新执行工作区 staging 烟测：安装/更新各 180 个文件，Install、Update、Uninstall 全部 PASS，用户存档哨兵保留。
- D16-A04/A06 专项测试 **13/13 PASS**；全量工具测试 **49/49 PASS**。
- 收紧 `tools/d16_a06_release_audit.py` 的语义：工作区部署脚本与正式 Windows 安装器分开统计。当前 `workspace_deployment_script_present=true`，但 `formal_installer_artifact_present=false`、`formal_installer_smoke_evidence=false`，阻断为 `formal_os_installer_missing` 与 `steam_upload_not_authorized`。
- 本机未发现可直接运行的 Inno Setup/WiX 安装器工具，也没有创建未验证的 MSI/MSIX/签名 Setup；因此 D16-R02 只关闭了工作区部署安全子门禁，未宣称正式 OS 安装包通过。

结论仍为 **IN_PROGRESS**：正式 OS installer/update、原生崩溃行为、目标设备实测、最终人工隐私审查、A01—A05 其他真实门禁和 Steam 授权仍未完成。

## 2026-08-12 A06 发布清单运行契约校验

- `tools/d16_a06_release_audit.py` 新增清单运行契约校验：产品/公司身份、Unity 版本、StandaloneWindows64、内容目录与 D15-S02 状态、中英文 locale、离线模式、存档版本/备份策略和安装状态字段。
- 新增“故意破坏 offline_mode 的清单必须被拒绝”测试，证明校验不是只检查字段存在。
- A06 专项测试 **14/14 PASS**；工具全量测试 **50/50 PASS**。
- 当前真实清单契约为 PASS；A06 审计仍为 **IN_PROGRESS**，`formal_installer_artifact_present=false`、`formal_installer_smoke_evidence=false`，阻断保持 `formal_os_installer_missing` 与 `steam_upload_not_authorized`。

这项工作只强化发布验证，不改变游戏领域状态、存档 schema、Systems 所有权或外部发布策略。

## 2026-08-12 Agent 模板兜底与 D5 离线回归

- `game/AgentCore/LLMClient.cs` 的 `TemplateFallback` 现在显式生成当前 `AgentReply` schema 版本、空关系提议、中性谈判/拒绝字段、空动作意图和 `Action=none`；回复序号使用原子递增，避免并发对话共享状态竞争。
- `game/Assets/Tests/EditMode/D8A04AgentReplyMockTests.cs` 新增模板字段完整性和模型失败不改变 CrewStats 的测试；Unity 6 `Save.Tests` 全量 **295/295 PASS**，结果为 `game/tools/results/d16_a06_agent_fallback_editmode_unity6_after_crew_sync.xml`。
- AgentCore 主源与 Unity 镜像已重新建立硬链接；同时将 D10 `BehaviorState` 接线恢复到主源，并让 Unity 跳过仅供纯 C# CLI 的 `System.Text.Json` 自测入口，避免镜像漂移和 Unity 编译边界混淆。
- `D5ContentTests` 新增默认离线、云队列本地化和关闭云功能不产生新 revision 的回归；Unity 6 `Save.Tests` 最新全量 **297/297 PASS**，D5 相关用例 **10/10 PASS**。D5 Systems 目录未发现 `HttpClient` 或 `UnityWebRequest`。
- 串行 `AgentCore`、`Whalebone.Agent.Runtime`、`Assembly-CSharp`、`Save.Tests` 编译均为 **0 warning / 0 error**；Unity 刷新生成资产后需先执行受限 restore，再进行 `--no-restore` 编译。

这些结果关闭了 Agent 模板安全字段和本地离线逻辑的代码级回归子门禁，但不替代断网启动/保存/重启/恢复的真实设备演练、跨平台测试、正式 OS installer 或 Steam 授权；D16-A06 总体继续 **IN_PROGRESS**，D16-S01/S02 仍不得提前启动。

## 2026-08-12 D16-A06 正式便携 Windows 安装器子门禁

- 新增 `tools/d16_a06_formal_installer/`，生成自包含 `WhalesIslandReefSetup.exe`；安装、更新、卸载限定在显式提供的 payload、安装目录和独立 user-data 目录内，拒绝复制符号链接/reparse point，不覆盖已有安装，更新切换失败时恢复旧安装。
- 真实 `dotnet publish` 构建成功，产物为 `artifacts/d16_a06_formal_installer/WhalesIslandReefSetup.exe`。
- `tools/d16_a06_formal_installer_smoke.py` 使用当前 Unity Windows payload 完成隔离 Install、非法 Update 拒绝且旧安装/用户存档保留、合法 Update、Uninstall；结果 `tools/results/d16_a06_formal_installer_smoke_result.json` 为 **PASS**。
- 最新 `tools/results/d16_a06_release_audit.json`：`formal_installer_artifact_present=true`、`formal_installer_smoke_evidence=true`、`missing_static_evidence=[]`；A06 总体仍为 **IN_PROGRESS**，当前审计阻断只剩政策性 `steam_upload_not_authorized`。

这关闭的是正式便携式 Windows 安装器的本地代码/烟测子门禁，不等价于签名 MSI/MSIX、注册表/权限集成、目标机器矩阵、原生崩溃验证、最终人工隐私审查或 Steam 发布。没有执行 Steam 上传或任何外部发布；D16-S01/S02 仍需等待 A01—A06 的全部真实门禁。

## 2026-08-12 A06 审计器负向证据复验

- `tools/d16_a06_release_audit.py` 现在要求正式安装器烟测同时提供：非法更新被拒绝、旧安装保留、用户存档保留；只有正向 Install/Update/Uninstall 成功不能单独关闭该子门禁。
- A06 专项测试 **15/15 PASS**；工具全量测试 **52/52 PASS**。
- 重新生成的 `tools/results/d16_a06_release_audit.json` 仍为所有本地检查 `true`、`missing_static_evidence=[]`、正式安装器产物/烟测 `true`，唯一阻断为 `steam_upload_not_authorized`。

## 2026-08-12 安装器清单一致性审计复验

- `tools/d16_a06_release_audit.py` 新增一致性门禁：正式安装器正/负向烟测为 PASS 时，manifest 的 Install/Update/Uninstall 三项必须全部为 PASS；旧的 PENDING 组合会被拒绝。
- A06 专项测试 **16/16 PASS**；工具全量测试 **53/53 PASS**。
- 当前审计：`release_manifest_installer_status_aligned=true`、`formal_installer_smoke_evidence=true`、`missing_static_evidence=[]`；A06 仍因 Steam 未授权政策保持 IN_PROGRESS。

## 2026-08-12 发布清单与真实安装器证据对齐

已将 `game/Assets/Resources/D16ReleaseManifest.json` 的 `installer.install_test`、`uninstall_test`、`update_test` 从历史 `PENDING` 更新为真实烟测结果 `PASS`；`release_status` 仍为 `blocked`，清单继续保留目标设备、人工审查、placeholder、原生隐私和 Steam 未授权等阻断。该配置修正只同步已验证事实，没有制造发布许可。

## 2026-08-12 本地诊断真实文件链回归

新增 `DiagnosticsWritesOnlyAfterConsentAndDeleteRemovesLocalFile`，在真实 MainMenu 场景中验证 A06 本地诊断边界：默认无同意时不写入；显式同意后通过实际 `D16LocalDiagnostics.OnLogMessage` 写入 `Application.persistentDataPath/D16Diagnostics/runtime-diagnostics.ndjson`；记录只保留分类、长度和行数，原始消息、堆栈、路径和存档文件名均不落盘；执行撤回并删除后主文件和轮转文件均不存在。

- Unity 6 D16 PlayMode 全量：**19/19 PASS**；
- 证据：`tools/results/d16_a06_diagnostics_playmode_unity6.xml`、`.log`；
- 本轮没有修改存档、Systems、网络或外部收集逻辑。

该证据关闭 A06 本地脱敏诊断文件链的代码级子门禁，不等于原生崩溃转储、目标设备矩阵或人工隐私审查通过。A06 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 后台线程诊断可靠性复验

- `D16LocalDiagnostics` 新增 `Application.logMessageReceivedThreaded` 接线：后台回调只生成脱敏元数据并进入 64 条有界队列，主线程每帧最多写入 8 条；撤回同意会清空队列，诊断异常不会冒泡到玩法循环。
- `D16DiagnosticRecord.CreateJsonFromMetadata` 统一构造脱敏 NDJSON，仍不保存原始错误、堆栈、路径、存档名或对话内容。
- Unity 6 专项 `D16LocalDiagnosticsTests`：**5/5 PASS**，`tools/results/d16_a06_diagnostics_threaded_playmode_unity6.xml`。
- Unity 6 D16 PlayMode 全量：**25/25 PASS**，`tools/results/d16_a06_full_d16_playmode_unity6.xml`。
- `Assembly-CSharp`、`Assembly-CSharp-Editor`、`Save.Tests`、`AgentCore`：0 warning / 0 error；格式检查通过。

该复验关闭的是 A06 托管日志线程安全/脱敏代码子门禁，不等于原生崩溃转储、目标机器、断电/跨平台、人工隐私审查或 Steam 发布授权完成；A06、D16-S01/S02 继续保持未完成/阻断。

## 2026-08-12 权威发布审计再次复验

`d16_a06_release_audit.py` 再次确认 18 项静态/安装器检查全部为 true、缺失静态证据为空；正式安装器及正/负向更新烟测证据有效。发布清单仍为 blocked，唯一机器阻断是 `steam_upload_not_authorized`。项目政策禁止上传 Steam，因此 A06 继续 **IN_PROGRESS**，不启动 D16-S01/S02。

## 2026-08-12 A06 审计阻断原因完整性修复

- 修复 `tools/d16_a06_release_audit.py`：审计结果现在完整透传 `D16ReleaseManifest.json` 的项目级 `blocking_reasons`，不再只显示 Steam 政策项而隐藏目标设备、原生崩溃、隐私和 A01—A05 的未闭合证据。
- 当前 `tools/results/d16_a06_release_audit.json`：19 项本机/安装器检查全部为 `true`，`missing_static_evidence=[]`，`technical_preparation_status=PASS`，`local_blocking_reasons=[]`。
- 当前发布清单仍有 7 条项目级阻断；Steam 未授权单独记录为 `policy_blocking_reasons=["steam_upload_not_authorized"]`。因此 A06 的本机技术准备子门禁通过，但 D16-A06 总体仍 **IN_PROGRESS**，不启动 D16-S01/S02。
- 新增单测覆盖“清单阻断原因”和“政策阻断原因”分离；`test_d16_a04_a06_audits.py` 专项测试 **20/20 PASS**。
- 工具测试全集 `python -m unittest discover -s tools/tests -p "test_*.py" -q`：**61/61 PASS**，0 failures、0 errors；原始结果见 `tools/results/tools_full_run_current.err.log`。

## 2026-08-12 A06 检查项计数校准

复核发现当前审计器实际返回 18 项检查，而历史文档多处记录为 19 项。已在 `tools/d16_a06_release_audit.py` 增加 `steam_upload_policy_safe`：明确验证清单中的 Steam `upload_authorized=false` 且 `upload_executed=false`，不执行任何上传。

- 当前 `tools/results/d16_a06_release_audit_current.json`：19/19 checks 为 `true`；
- `missing_static_evidence=[]`、`technical_preparation_status=PASS`、`release_manifest_status=blocked`；
- 项目级阻断仍完整保留：A01 目标性能、A02 断电/跨平台、A03 真机可访问性、A04 人工审查、A05 placeholder、A06 目标设备/原生崩溃/隐私，以及 Steam 未授权；
- A06 总体继续 **IN_PROGRESS**，本次只修正审计覆盖，不把技术准备 PASS 写成发布 PASS。

专项 `python -m unittest tools.tests.test_d16_a04_a06_audits -v`：**21/21 PASS**。

全量工具回归在该修复后重新执行：`python -m unittest discover -s tools/tests -p 'test_*.py' -q`，**69/69 PASS**（95.562s）。

## 2026-08-12 异常退出会话标记回归

### 修改内容

- `D16LocalDiagnostics` 在用户明确同意后为当前运行会话创建固定内容的本地 marker；正常 `OnApplicationQuit`、撤回同意或对象销毁时清理 marker。
- 下次启动若发现上一次 marker，则只写入 `unexpected_exit` 分类、时间、场景和版本等脱敏元数据；marker 原文、错误消息、堆栈、路径、存档正文和对话内容均不写入诊断文件。
- `D16ReleaseManifest.json` 明确登记该 marker 仅为本地脱敏异常退出提示，不是原生崩溃转储，也不上传外部服务。

### 原始验证

- Unity 6 聚焦 PlayMode `UnexpectedSessionMarkerIsRedactedAndCleanExitRemovesMarker`：**1/1 PASS**，证据 `tools/results/d16_a06_session_marker_playmode_current_unity6.xml`。
- 当前 D16 PlayMode 全量：`tools/results/d16_a06_session_marker_full_current_unity6.xml`，**44/44 PASS**，0 failed / 0 inconclusive / 0 skipped，约 72.55s。
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error。

### 边界

该增量关闭了托管运行时“异常退出无可辨识本地会话痕迹”的代码子缺口，并不等于原生崩溃转储、目标设备行为或人工隐私审查完成；A06 继续 **IN_PROGRESS**，发布 manifest 继续 `blocked`，没有执行 Steam 上传。

### 后续回归

- `python tools/d16_a06_release_audit.py game --json-out tools/results/d16_a06_release_audit_current.json`：19/19 本机与安装器检查为 true，`missing_static_evidence=[]`，技术准备 PASS，发布状态仍 blocked。
- AgentCore、Save.Tests、Assembly-CSharp、D16.PlayMode.Tests 均 0 warning / 0 error；全量工具测试 **69/69 PASS**（97.455s）。
- Systems 边界扫描与 `git diff --check` 均 PASS；这些结果不关闭原生崩溃、目标设备、人工隐私和项目级其他阻断。

## 2026-08-12 当前 A06 发布门禁复验

`python tools/d16_a06_release_audit.py game --json-out tools/results/d16_a06_release_audit_current.json` 当前结果：

- 19/19 本机与安装器检查为 true；`missing_static_evidence=[]`；`technical_preparation_status=PASS`；
- `release_manifest_status=blocked`，保留 7 条真实阻断：A01 目标设备性能、A02 物理断电/跨平台、A03 真机可访问性、A04 人工审查、A05 placeholder、A06 目标设备/原生崩溃/隐私、Steam 未授权；
- `local_blocking_reasons=[]`，说明本机可自动收口的静态检查没有新增缺口；未执行 Steam 上传。

A06 仍 **IN_PROGRESS**；本机技术准备 PASS 不等同于最终发布通过，D16-S01/S02 不启动。

## 2026-08-12 D3 MainMenu 批处理探针隔离与当前 Player 启动复验

- 修复 `game/Assets/Scripts/Gameplay/D3/D3RuntimeSmokeProbe.cs`：只在活动场景为 `InitialHarbor` 时安装 D3 港口探针；正式 Player 首场景 `MainMenu` 不再因等待港口对象超时而被误判为 D3 失败。
- 新增回归 `D16A06MainMenuBatchPlayerDoesNotInstallHarborSmokeProbe`：聚焦 Unity 6 **1/1 PASS**，当前 D16 PlayMode 全量 **46/46 PASS**，证据分别为 `tools/results/d16_a06_mainmenu_smoke_probe_playmode_current_unity6.xml` 与 `tools/results/d16_a06_smoke_probe_full_current_unity6.xml`。
- `Assembly-CSharp`、`D16.PlayMode.Tests` 均 **0 warning / 0 error**；当前源码 Windows 构建 `artifacts/d16_a06_windows_build_smoke_probe/` 成功，日志为 `tools/results/d16_a06_windows_build_smoke_probe_current.log`。
- 新 Player 启动日志 `tools/results/d16_a06_windows_player_smoke_probe_graphics_current.log` 已出现 D3D11、MainMenu EventSystem、InputSystemUIInputModule、主菜单 UI 和 D16 设置初始化，未出现 `D3 Smoke`、`No cameras`、Exception 或 Crash。Standalone Player 未自行响应 `-quit`，验收进程在确认启动日志后结束，因此不把该次结束记为自然退出码 PASS。

本轮关闭的是本机 Player 启动时的 D3 探针误触发和 MainMenu 启动日志回归；目标设备、原生崩溃、最终人工隐私审查、A01—A05 项目级阻断和 Steam 授权仍未完成，D16-A06 总体保持 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D5 MainMenu 批处理探针隔离复验

### 根因与修复

正式 Player 首场景是 MainMenu，但 D5RuntimeSmokeProbe 只判断 -d5Smoke 参数，导致从主菜单启动时也创建只属于 InitialHarbor 的港口探针；探针等待港口对象直到超时并以错误码退出，形成“主菜单启动失败”的假故障。

已在 game/Assets/Scripts/Gameplay/D5/D5RuntimeSmokeProbe.cs 增加活动场景门槛：只有 InitialHarbor 才安装 D5 港口探针。MainMenu 不再等待港口对象，也不再把正常主菜单启动误报为 D5 smoke failure。

### 验证

- Unity 6 聚焦回归 D16A06MainMenuBatchPlayerDoesNotInstallHarborSmokeProbe：**1/1 PASS**，证据 tools/results/d16_a06_d5_probe_guard_current_unity6.xml；同时确认 D3、D5 两个探针都未在 MainMenu 安装；
- 当前 D16 PlayMode 全量：tools/results/d16_a06_d5_guard_full_current_unity6.xml，**46/46 PASS**，failed/inconclusive/skipped 均为 0；
- Assembly-CSharp、D16.PlayMode.Tests：**0 warning / 0 error**；
- 包含修复的 Windows Player 构建：artifacts/d16_a06_windows_build_d5_guard/，日志 tools/results/d16_a06_windows_build_d5_guard_current.log，Build Finished, Result: Success；
- A06 权威审计仍为 **19/19 checks true**、technical_preparation_status=PASS、release_manifest_status=blocked，项目级阻断仍为 7 条；本轮没有执行 Steam 上传。

### 边界

这次关闭的是 D5 冒烟探针在 MainMenu 的误安装代码缺口，不等于正式 Player 的目标设备、原生崩溃、最终隐私人工审查或完整发布门禁通过。D16-A06 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 本地诊断隐私专项复验

- Unity 6 PlayMode 使用 `-force-d3d11`、`D16.PlayMode.Tests`、`D16LocalDiagnostics` 筛选重新执行；结果 `tools/results/d16_a06_privacy_current_20260812.xml`：**7/7 PASS**，failed/inconclusive/skipped 均为 0；完整日志为 `tools/results/d16_a06_privacy_current_20260812.log`。
- 本轮复验覆盖本地诊断 consent、删除、线程队列、脱敏、异常会话标记清理，以及 A06 隐私面板的 opt-in/delete/close 行为；未发现当前 Windows/Unity 6 自动化回归。
- 重新执行 `python tools/d16_a06_release_audit.py game --json-out tools/results/d16_a06_release_audit_current.json`：**19/19** 静态与安装器检查为 true，`missing_static_evidence=[]`，`technical_preparation_status=PASS`。
- A06 总体仍 **IN_PROGRESS**：目标设备/native crash、最终人工隐私审查以及 A01—A05 项目级证据仍未完成；发布清单继续 `blocked`，Steam 未授权且未上传。D16-S01/S02 不启动。

## 2026-08-12 A06 诊断文件原子替换复验

### 修改

- `D16LocalDiagnostics` 的会话标记写入与日志轮转改为同目录原子替换；不再先删除旧目标文件，提交失败时保留旧记录。
- 新增 `DiagnosticFileReplacementKeepsOldTargetUntilAtomicCommit` 回归，覆盖替换提交后的新内容与临时文件清理。

### 验证

- 聚焦 Unity 6 `D16LocalDiagnostics`：`tools/results/d16_a06_diagnostics_atomic_current_20260812.xml`，**8/8 PASS**，0 failed / 0 skipped / 0 inconclusive；
- 当前 D16 PlayMode 全量：`tools/results/d16_full_after_diagnostics_atomic_20260812.xml`，**54/54 PASS**，0 failed / 0 skipped / 0 inconclusive；
- 本轮未新增目标设备、原生崩溃、跨平台、最终隐私人工审查或最终资源证据；A06 继续 **IN_PROGRESS**，发布清单继续 `blocked`，Steam 未授权且未上传。

## 2026-08-12 产品名与存档路径兼容性收口

- 实际 Unity 工程、编辑器幂等设置、运行时发布清单和英文主菜单标题已统一为公开项目名 `WhalesIslandReef`；公司名仍为 `Whalebone Studio`，内部 C# 命名空间不改。
- 由于 Unity 会把 `productName` 纳入 `Application.persistentDataPath`，`SaveSystem` 新增一次性旧目录迁移：仅在新目录没有存档且旧 `Whalebone/save.db` 可读时迁移；迁移前建立文件级备份，旧库和备份不删除，失败不会阻断主菜单启动。
- 发布审计器的 Unity YAML 产品字段匹配改为兼容缩进，避免静态审计“存在字段但没有真正比较值”。新增 `tools/tests/test_project_identity.py` 覆盖产品名、发布清单、编辑器设置、主菜单标题和存档迁移保护。
- 验证：Unity 6 PlayMode `tools/results/d17_full_after_project_identity_20260812.xml` **60/60 PASS**；工具测试 **74/74 PASS**；Assembly-CSharp、AgentCore、Save.Tests、D16.PlayMode.Tests 均 0 warning / 0 error；日志确认旧 `Whalebone` 存档已保留式迁移到新目录。

本轮只关闭产品身份与旧存档兼容性的本地技术缺口；A06 仍 **IN_PROGRESS**，目标设备/native crash、最终人工隐私审查及 A01—A05 外部证据仍未完成，Steam 未授权、未上传，D16-S01/S02 不启动。
## 2026-08-12 总纲顺序下的 A06 当前证据

当前发布审计 19/19 本机与安装器检查通过，`missing_static_evidence=[]`，技术准备 PASS；发布清单仍 blocked，7 条项目级阻断完整保留，Steam 未授权且未执行。A06 继续 **IN_PROGRESS**，D16-S01/S02 不启动。
