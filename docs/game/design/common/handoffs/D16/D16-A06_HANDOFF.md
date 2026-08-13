# D16-A06 任务交接报告

## 任务信息

- 任务编号：D16-A06
- 日期：2026-08-11
- 状态：IN_PROGRESS
- 前置门禁：D15-S02 已通过；正式 installer/update 与 Steam 授权未完成
- 发布动作：未执行；未上传、未发布、未发送外部消息

## 修改文件

- docs/game/design/common/D16_RELEASE_TECHNICAL_PREP.md
- docs/game/design/common/handoffs/D16/D16-A06_HANDOFF.md
- .workbuddy/memory/2026-08-11.md

本次没有修改 game、contracts、Systems、AgentCore、Save/Storage、构建工程或安装脚本。没有创建未验证的安装器、更新器、崩溃上传器或 Steam 上传配置。

## 数据边界

- 发布技术准备只读取配置、内容 QA、审查/授权、存档和表现状态，不写任何领域实体。
- Player、Ship、Crew、Relation、Finance、Quest、WorldEvent、Reward 等状态仍由各自系统和 SaveSystem 按既有所有权写入。
- 离线模式只影响外部服务状态；Template Agent 只产生表达层回复和 none 行动，不能改变强逻辑裁决。
- 存档安装、卸载、更新和备份必须保留 D7 字段、原始存档和迁移失败恢复能力。
- 云存档仍是 D5 本地接口预留，不连接外部服务。

## 已确认基线

- Unity 6000.0.80f1。
- 已启用场景基线：MainMenu、Bootstrap、InitialHarbor。
- 产品基线：Whalebone Studio / WhalesIslandReef / 1.0。
- D5SteamSystem：离线安全状态、本地成就和待同步云存档状态。
- D5ReleaseCatalog：商店中英文短描述、年龄提示、必需素材名称清单。
- AgentCore：Ollama/路由器/TemplateFallback 现有代码；不在本任务中改写。
- SaveSystem：Application.persistentDataPath/save.db；D16-A02 已记录尚无文件级备份恢复验收。

## 阻塞证据

### D15-S02

复核命令：

python tools/d15_s02_content_qa.py game/Assets/Resources/ContentRegistry --json-out tools/results/d15_s02_content_qa_result.json

历史复核结果：D15_S02_QA_EXIT=1，1326 errors，10 warnings；当前顺序复验为 PASS（0 errors、0 warnings）。

主要阻断：

- A06 资源审查/授权字段缺失；
- A02 生境冲突；
- A07 宠物外观孤儿引用；
- A08 测试、LOD-N/A、资源状态元数据缺失；
- A01-A07 的 2536 个 display key 缺中英 locale 包证明；
- 24 个抽样有 9 个缺运行时 save/restore 标记。

### 构建与存档

复核命令：

dotnet build game/Save.Tests.csproj --no-restore

结果：失败，game/Assets/Scripts/Agent/AgentCore/CrewAgent.cs(18) 缺少 D10CrewBehaviorState（CS0246）。该问题属于既有 D10/Agent 工程依赖，本任务未越权修复。

## D16-A06 验收结论

| 验收项 | 结论 |
|---|---|
| 构建配置冻结 | PARTIAL：已有 Unity/场景/版本基线，未生成发布构建 |
| 崩溃记录与隐私 | BLOCKED：未找到发布级脱敏方案 |
| 离线模式 | PARTIAL：D5 本地桥接存在，未完成发布级回归 |
| Template Agent 降级 | PARTIAL：现有 TemplateFallback，未完成发布级回归 |
| 配置校验 | PARTIAL：已有内容 QA，不等于 D16 发布 manifest 校验 |
| 安装/卸载/更新 | NOT READY：未找到脚本和实测证据 |
| 存档备份 | BLOCKED：D16 文件级备份/恢复未完成 |
| 商店素材 | BLOCKED：授权/审查闭包和最终表现未完成 |
| Steam 发布 | NOT EXECUTED：需要用户授权 |

## 未完成事项

1. 关闭 D15-S02 的全部内容、资产、审查、本地化和存档标记问题；
2. 关闭 D16-A04 的授权/历史敏感内容阻塞；
3. 修复 Save.Tests 的 D10CrewBehaviorState 工程依赖；
4. 完成 D16-A02 文件级备份、恢复、断电、迁移失败和重复结算测试；
5. 冻结发布 manifest、版本、构建产物摘要、安装路径和存档保留规则；
6. 完成隐私安全的崩溃记录、离线/模板降级、配置校验和安装/更新回归；
7. 逐项关闭商店素材来源、授权、审查、版本记录和 placeholder 状态。

## 是否允许开始异步任务

**不允许。**

D16-A06 只有在 D15-S02 通过、D16-A04 阻塞项关闭、Save.Tests 可构建并完成 D16-A02 存档保护验收后，才允许重新进入发布技术实现和测试。真实 Steam 发布仍需用户或发布负责人单独授权。

## 2026-08-11 状态更新

D15-S02 已通过；D10 AgentCore/Systems/Save.Tests standalone build 均为 0 warning / 0 error。D16-A06 当前为 **PENDING**，发布构建、安装/卸载、覆盖更新、隐私/崩溃、文件级恢复、manifest 和商店素材仍未完成；没有执行外部发布动作。

## 2026-08-11 静态审计进度补充

当前状态：**IN_PROGRESS，真实发布流程待授权与实测**。

新增 `tools/d16_a06_release_audit.py`、`tools/results/d16_a06_release_audit.json` 和 `docs/game/design/common/D16_RELEASE_MANIFEST_TEMPLATE.json`。静态基线检查通过项目版本、产品标识、必需场景、离线桥接、TemplateFallback、CrashReport 默认关闭和 SaveFileSafety；正式 runtime manifest、Unity 构建、安装/卸载/更新仍是阻断项。没有执行 Steam 上传或外部消息。

## 2026-08-11 Unity 6 实测更新

当前状态仍为：**IN_PROGRESS（静态准备和工程构建通过，发布实测未执行）**。

- Unity 6 PlayMode 基础运行时 1/1 PASS，EditMode 全量回归 291/291 PASS；D16 相关运行时程序集、Assembly-CSharp 和 Assembly-CSharp-Editor 串行构建均 0 warning / 0 error。

## 2026-08-11 静态发布审计更新

最新 A06 审计确认 installer/update 脚本检查为 true，但 `release_manifest_present=false`；Unity 目标构建、安装/卸载/覆盖更新、隐私/崩溃记录和 Steam 上传仍未执行。外部发布动作保持未授权、未执行，A06 继续 IN_PROGRESS。

已新增 `game/Assets/Resources/D16ReleaseManifest.json`，其 `release_status` 明确为 `blocked`，Steam 上传保持未授权/未执行，安装/卸载/更新仍为 PENDING。最新静态审计 `missing_static_evidence=[]`，但 Unity 构建、安装/更新实测和外部发布门禁仍未关闭。

`tools/d16_a06_release_audit.py` 已增加“清单必须可解析且保持 blocked”校验，A04/A05/A06 审计单测现为 **4/4 PASS**；这只加强静态门禁，不代表发布通过。
- 这些结果只证明当前工程能编译、InitialHarbor 能启动并完成基础接线；没有替代 Windows 目标构建、安装/卸载、覆盖更新、崩溃恢复、离线更新和 Steam 商店授权流程。
- 继续保持 `D16_RELEASE_MANIFEST_TEMPLATE.json` 为 blocked，直到获得目标构建和安装/更新实测证据；Steam 上传仍未执行。

## 2026-08-11 Windows 构建与启动烟测更新

- Unity 6 StandaloneWindows64 构建成功：日志 `tools/results/d16_a06_windows_build_unity6.log` 明确记录 `Build Finished, Result: Success`，退出码 0。
- 构建产物位于 `artifacts/d16_a06_windows_build/`：`WhalesIslandReef.exe`、UnityPlayer、数据目录和 3 个 Build Settings 场景；exe SHA-256 为 `D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。
- 使用 `-batchmode -nographics` 进行 15 秒本地启动烟测，主菜单 EventSystem、InputSystemUIInputModule、UI 初始化和 D16-A03 设置应用均出现在 `tools/results/d16_a06_windows_player_smoke.log`；未发现 Error/Exception/Crash/No cameras 日志。测试结束时只停止了该烟测进程。

这关闭了 A06 的“Unity Windows 构建 + 启动烟测”子门禁，但没有把烟测当安装/卸载/覆盖更新验收：安装器、更新器、文件级存档恢复、隐私/崩溃正式验证和 Steam 仍未执行，A06 保持 IN_PROGRESS。

最新静态审计 `tools/results/d16_a06_release_audit.json` 已消费本地构建证据：`unity_windows_build_evidence=true`；当前剩余阻断为 `install_uninstall_update_not_run` 和 `steam_upload_not_authorized`。审计单测现为 **6/6 PASS**。

## 2026-08-11 工作区 staging 安装/更新/卸载烟测

- 新增 `tools/d16_a06_install_update_smoke.py`，只在 `artifacts/d16_a06_install_update_smoke/` 内执行：复制安装、替换更新包、验证独立 save 哨兵保留、删除已安装 payload 并再次验证存档保留。
- `tools/results/d16_a06_install_update_smoke_result.json`：180 个安装文件、180 个更新文件，install/update/uninstall 全部 true，结果 PASS。
- A06 审计已消费该证据：`workspace_install_update_smoke_evidence=true`，但正式 OS installer/update 脚本仍不存在，因此当前剩余阻断为 `installer_update_flow_missing` 和 `steam_upload_not_authorized`。
- 审计单测最新 **8/8 PASS**；工作区 staging 测试不宣称真实 Windows 安装器、注册表、权限、卸载器或 Steam 流程通过。

## 2026-08-11 当前证据复验

本轮以 `game` 作为项目根目录重跑发布静态审计，避免工作区根目录导致的相对路径假缺口。最新 `tools/results/d16_a06_release_audit.json` 为：

```text
required_scenes_enabled=true
offline_steam_bridge_present=true
template_fallback_present=true
file_level_save_safety_present=true
release_manifest_blocked_and_complete=true
unity_windows_build_evidence=true
workspace_install_update_smoke_evidence=true
installer_or_update_script_present=false
```

最新静态审计的 `missing_static_evidence=[]`，阻断只剩 `steam_upload_not_authorized`。新增 `tools/d16_a06_install_update.ps1`，在 `artifacts/d16_a06_installer_script_smoke/` 内实测 Install/Update/Uninstall 三步均 PASS，且独立 user-data 目录保留。该脚本是工作区限定部署入口，不被扩大解释为签名 OS installer、注册表/权限集成或 Steam 流程通过。A06 仍为 **IN_PROGRESS**，没有执行 Steam 上传或外部发布。

## 2026-08-12 本地隐私诊断子门禁

### 变更

- 新增 `game/Assets/Scripts/Gameplay/D16/D16DiagnosticRecord.cs`：生成 `d16_local_diagnostic_v1` 脱敏记录，只保留错误类别、版本、平台、场景和长度/行数统计，固定写入 `raw_message_omitted=true`、`raw_stack_omitted=true`。
- 新增 `game/Assets/Scripts/Gameplay/D16/D16LocalDiagnostics.cs`：默认关闭，显式同意后才记录 Error/Exception/Assert；独立目录、512 KiB 轮转、撤回并清理入口，不读取存档，不联网，不上传。
- 更新 `game/Assets/Resources/D16ReleaseManifest.json` 与 `tools/d16_a06_release_audit.py`，把本地同意、原始字段禁采集和删除入口纳入静态门禁。
- 新增 `game/Assets/Tests/PlayMode/D16LocalDiagnosticsTests.cs`，覆盖原始内容不落盘、分类和边界字段。

### 真实验证

- Unity 6 PlayMode：`D16LocalDiagnosticsTests` **3/3 PASS**，结果 `tools/results/d16_a06_diagnostics_playmode_unity6_final.xml`。
- 新增诊断后的完整 D16 PlayMode 回归：**10/10 PASS**，结果 `tools/results/d16_post_diagnostics_playmode_unity6.xml`；全量工具回归 **48/48 PASS**。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error。
- `dotnet build game/Assembly-CSharp-Editor.csproj --no-restore -v:minimal`：0 warning / 0 error。
- `python -m unittest tools.tests.test_d16_a04_a06_audits -v`：**12/12 PASS**。
- `python tools/d16_a06_release_audit.py game --json-out tools/results/d16_a06_release_audit.json`：所有静态检查为 `true`，只保留政策阻断 `steam_upload_not_authorized`。

### 边界与待办

- 这不是完整的原生崩溃转储系统，也没有外部收集服务；设置 UI、用户可见隐私说明、原生崩溃行为和目标设备验证仍待完成。
- 发行清单继续为 `release_status=blocked`；未上传、未发布、未发送外部消息。
- D16-A06 总体仍为 **IN_PROGRESS**，D16-S01/S02 不能启动；没有因为这项子门禁通过而改变 A04/A05、A01/A02/A03 的未完成状态。

## 2026-08-12 用户可见隐私入口子门禁

- 任务编号：D16-A06（本地诊断隐私 UI 子门禁）。
- 状态：**PASS（子门禁）/ IN_PROGRESS（D16-A06 总体）**。
- 修改文件：`game/Assets/Scripts/Gameplay/UI/D16/D16LocalDiagnosticsPanel.cs` 及 `.meta`、`MainMenuController.cs`、`HarborPauseController.cs`、`Localization.cs`、`D16LocalDiagnosticsPanelTests.cs` 及 `.meta`。
- 新入口：主菜单设置的 `BtnDiagnostics`、暂停菜单的 `Diagnostics`；同意开关使用 `D16LocalDiagnostics` 的公开接口，删除按钮使用 `RevokeConsentAndClear`。
- 数据所有权：UI 只调用 D16LocalDiagnostics 的同意/删除接口；不读取或写入 Player、Ship、Crew、存档正文、事件、奖励或网络状态。
- 输入边界：面板使用独立 sorting order 900 的模态 Canvas；打开时阻断玩法输入并保留单一 EventSystem，关闭时恢复玩法输入；没有修改 `Systems/`、AgentCore 或 contracts。
- 真实测试：Unity PlayMode 单项 **1/1 PASS**（`tools/results/d16_a06_privacy_panel_playmode_unity6.xml`）；D16 PlayMode 全量 **11/11 PASS**（`tools/results/d16_a06_privacy_full_d16_playmode_unity6.xml`）；`Assembly-CSharp` **0 warning / 0 error**；A04/A05/A06 专项审计 **12/12 PASS**；A06 静态审计 `missing_static_evidence=[]`，仅保留 `steam_upload_not_authorized`。
- 验收结论：用户可见的默认关闭、显式同意、撤回、删除和关闭链已真实通过；不扩大为完整隐私、原生崩溃、目标设备或发布验收。
- 未完成事项：原生崩溃行为、目标设备、正式 OS installer/update、最终人工隐私审查和 Steam 授权仍缺证据；A06 总体保持 IN_PROGRESS。
- 下游任务：补 A06 剩余发布证据；关闭 A01—A05 后才进入 D16-S01/S02。Steam 上传继续禁止自动执行。

## 2026-08-12 安装更新烟测复验

- 重新执行 `tools/d16_a06_install_update_smoke.py`，仅操作 `artifacts/d16_a06_install_update_smoke/` 隔离目录。
- 安装 180 个文件、覆盖更新 180 个文件；Install、Update、Uninstall 均 PASS，更新和卸载后独立用户存档哨兵均保留。
- 最新 `tools/results/d16_a06_release_audit.json`：`missing_static_evidence=[]`，所有本地静态、构建和工作区烟测检查为 true，唯一阻断仍为 `steam_upload_not_authorized`。
- 该证据仍不是签名 OS installer、真实权限/注册表卸载器、目标设备崩溃验证或 Steam 发布；D16-A06 总体保持 IN_PROGRESS。

## 2026-08-12 D16-R02 安装更新安全回归

- `tools/d16_a06_install_update.ps1` 已改为可回滚的更新流程：旧版本移动到隔离回滚目录后再切换新 payload；切换失败恢复旧版本；`Install` 不覆盖已有安装目录。
- 新增 `tools/tests/test_d16_a06_install_update_script.py`，真实调用 PowerShell 验证 Install、Update、Uninstall、已有安装拒绝覆盖和用户存档保留，**1/1 PASS**。
- `tools/d16_a06_install_update_smoke.py` 最新结果：180/180 文件安装、180/180 文件更新、卸载后用户存档哨兵保留，**PASS**。
- `python -m unittest tools.tests.test_d16_a04_a06_audits tools.tests.test_d16_a06_install_update_script -v`：**13/13 PASS**。
- `python -m unittest discover -s tools/tests -p 'test_*.py' -v`：**49/49 PASS**。
- 发布审计现在区分工作区部署脚本和正式 Windows 安装器：工作区脚本为 `true`；正式安装器产物和正式安装器烟测为 `false`，新增阻断 `formal_os_installer_missing`。没有创建未验证的 MSI/MSIX/签名 Setup，也没有执行 Steam 或外部发布。

### D16-R02 结论

已完成：工作区限定部署入口的安装、覆盖更新、卸载、存档保留和失败防护回归。

未完成：正式 OS installer/update（安装器工具/产物/权限/卸载注册/签名/目标机器实测）。因此 D16-A06、D16-S01、D16-S02 仍不能标记完成。

## 2026-08-12 A06 发布清单运行契约校验

- `tools/d16_a06_release_audit.py` 现在验证发布清单的产品身份、Unity/目标平台、内容目录 QA 状态、中英文 locale、离线模式、存档兼容/备份策略和 Install/Uninstall/Update 状态字段。
- 测试覆盖完整清单和故意破坏 `offline_mode` 的清单；不完整清单会产生 `release_manifest_contract_incomplete`，不会被当作静态通过。
- A06 专项测试 **14/14 PASS**；工具全量测试 **50/50 PASS**。
- 当前项目清单契约校验为 `true`；正式 OS installer 产物/烟测仍为 `false`，Steam 仍未授权、未执行。

该子门禁不改变 A06 总体状态，也不允许提前进入 D16-S01/S02。

## 2026-08-12 Agent 兜底与 D5 离线代码回归

- `TemplateFallback` 已显式返回 schema 版本、空关系提议、中性谈判/拒绝字段、空动作意图和 `none` 动作；并使用原子索引，保证并发调用不会竞争模板序号。
- 新增 `D8A04AgentReplyMockTests` 两项回归：模板回复必须是 v1.1 no-op；模型失败经 `DialogueSession` 降级时 CrewStats 不发生变化。
- 新增 `D5ContentTests` 两项回归：默认 Steam 桥接为 `UNCONFIGURED`/离线安全模式，云队列只写本地待同步 revision；关闭云功能时不创建新 revision。
- Unity 6 `Save.Tests` 最新全量 **297/297 PASS**，D5 用例 **10/10 PASS**；Agent fallback 中间结果 **295/295 PASS**，证据位于 `game/tools/results/d16_a06_agent_fallback_editmode_unity6_after_crew_sync.xml` 和 `game/tools/results/d16_d5_offline_editmode_unity6.xml`。
- AgentCore、Agent Runtime、Assembly-CSharp、Save.Tests 串行编译均 **0 warning / 0 error**。Unity/纯 C# 镜像硬链接已恢复，D10 `CrewAgent.BehaviorState` 已回到唯一主源。

### 边界

本轮关闭的是代码级离线/模板降级子门禁；没有把本机测试扩大为真实断网重启、跨平台文件系统、目标设备、正式 installer/update 或 Steam 发布证明。A06 继续 **IN_PROGRESS**，A01—A05 的真实门禁和 D16-S01/S02 前置条件不变。

## 2026-08-12 正式便携 Windows 安装器子门禁更新

### 已完成

- 新增 `tools/d16_a06_formal_installer/`，构建出自包含 `artifacts/d16_a06_formal_installer/WhalesIslandReefSetup.exe`。
- 安装器支持显式 `install`、`update`、`uninstall`；payload、安装目录、user-data 目录必须互斥，已有安装不会被 `install` 静默覆盖，复制过程拒绝 reparse point，更新切换失败会恢复旧目录。
- `tools/d16_a06_formal_installer_smoke.py` 已使用真实 Unity Windows 构建执行隔离烟测：Install PASS；非法 payload 的 Update 被拒绝且旧安装、用户存档均保留；合法 Update PASS；Uninstall PASS；卸载后独立用户存档哨兵仍保留。

### 证据与边界

- `tools/results/d16_a06_formal_installer_smoke_result.json` 为 **PASS**，包含 `failed_update_rejected=true`、`failed_update_preserved_install=true`、`failed_update_preserved_user_save=true`。
- `tools/results/d16_a06_release_audit.json`：`formal_installer_artifact_present=true`、`formal_installer_smoke_evidence=true`、`missing_static_evidence=[]`；A06 总体仍 **IN_PROGRESS**，阻断为 `steam_upload_not_authorized`。
- 没有执行 Steam 上传、外部发布或外部消息发送；该子门禁不扩大为签名 MSI/MSIX、注册表/权限集成、目标机器矩阵、原生崩溃行为或最终人工隐私审查。A01—A05 的真实缺口仍未关闭，D16-S01/S02 不能启动。

## 2026-08-12 负向更新证据审计复验

- 发布审计已强制要求正式安装器烟测包含非法更新拒绝、旧安装保留和用户存档保留字段，避免仅凭正向安装成功误判。
- A06 专项 **15/15 PASS**；工具全量 **52/52 PASS**。
- 最新 A06 JSON 仍确认所有本地检查通过，正式安装器产物和烟测为 true；当前只剩 `steam_upload_not_authorized`，Steam 未执行。

## 2026-08-12 发布清单状态对齐

- 更新 `game/Assets/Resources/D16ReleaseManifest.json`：Install、Update、Uninstall 三个字段现在与真实正式便携安装器烟测一致为 `PASS`。
- 清单仍保持 `release_status=blocked`；目标设备、断电/跨平台、可访问性、人工审查、placeholder、原生崩溃/隐私和 Steam 未授权项仍保留为阻断。
- 这只是配置事实同步，不代表 A06 总体、D16-S01/S02 或 Steam 发布通过。

## 2026-08-12 安装器清单一致性门禁

- 审计器现在拒绝“正式烟测 PASS 但 manifest 安装状态仍 PENDING”的不一致组合；新增负例覆盖。
- A06 专项 **16/16 PASS**；工具全量 **53/53 PASS**。
- 当前真实审计的 `release_manifest_installer_status_aligned=true`、`formal_installer_smoke_evidence=true`；Steam 仍未授权、未执行，A06 总体和 D16-S01/S02 不变。

## 2026-08-12 本地诊断真实文件链回归

新增 `DiagnosticsWritesOnlyAfterConsentAndDeleteRemovesLocalFile`，在真实 MainMenu 场景中覆盖：默认关闭不写文件、显式同意后通过真实日志入口写入本地 ndjson、原始错误/堆栈/路径/存档文件名不落盘、撤回并删除后主文件和轮转文件均清除。

- Unity 6 D16 PlayMode 全量：**19/19 PASS**；
- XML/日志：`tools/results/d16_a06_diagnostics_playmode_unity6.xml`、`.log`；
- 未修改存档、Systems、网络或外部收集逻辑。

该结果只关闭 A06 本地脱敏诊断文件链的代码级子门禁；原生崩溃转储、目标设备矩阵、签名安装器和人工隐私审查仍未完成。A06 继续 **IN_PROGRESS**，D16-S01/S02 不能启动。

## 2026-08-12 后台线程诊断可靠性子门禁

- 状态：**PASS（代码级子门禁）/ IN_PROGRESS（D16-A06 总体）**。
- `D16LocalDiagnostics` 现在同时接收 Unity 主线程和后台线程日志；后台线程只计算错误类别、消息长度和堆栈行数，进入最多 64 条的有界队列，不访问 Unity API、不读写文件、不保存原始文本。
- 主线程每帧最多落盘 8 条，撤回同意时清空待写队列；诊断写入失败被隔离，不影响游戏循环。`D16DiagnosticRecord.CreateJsonFromMetadata` 统一限制长度/行数并保留原始字段省略标记。
- 新增 `ThreadedDiagnosticsAreQueuedAndRedactedBeforeMainThreadWrite`，验证真实 MainMenu 场景下后台路径最终落盘、类别可用、原始内容/路径/堆栈不落盘。
- Unity 6 专项：`D16LocalDiagnosticsTests` **5/5 PASS**，证据 `tools/results/d16_a06_diagnostics_threaded_playmode_unity6.xml` / `.log`。
- Unity 6 D16 PlayMode 全量：**25/25 PASS**，证据 `tools/results/d16_a06_full_d16_playmode_unity6.xml` / `.log`。
- `Assembly-CSharp`、`Assembly-CSharp-Editor`、`Save.Tests`、`AgentCore`：均 **0 warning / 0 error**；`git diff --check` 通过（仅有既存换行格式提示）。

该子门禁只证明托管日志的线程安全排队与脱敏落盘链；原生崩溃转储、目标机器矩阵、断电/跨平台文件系统、最终人工隐私审查和 Steam 政策授权仍未完成。不得据此启动 D16-S01/S02 或执行 Steam 上传。

## 2026-08-12 A06 权威审计再次复验

- `python tools/d16_a06_release_audit.py game --json-out tools/results/d16_a06_release_audit.json`：全部 18 项静态/安装器检查为 true，`missing_static_evidence=[]`。
- 正式便携安装器和正/负向更新烟测证据仍有效；`release_manifest_status=blocked`。
- 唯一机器阻断为 `steam_upload_not_authorized`。依据项目政策未执行 Steam 上传，A06 继续 **IN_PROGRESS**，不启动 D16-S01/S02。

## 2026-08-12 发布清单阻断文字事实同步

- `game/Assets/Resources/D16ReleaseManifest.json` 中原先将“原生安装器”列为待完成的旧文字已改为 `D16-A06 target-device/native-crash and final privacy review pending`。
- 该修改与当前真实证据一致：正式便携 Windows 安装器产物、安装/非法更新拒绝/合法更新/卸载烟测均已通过；目标设备、原生崩溃行为和最终人工隐私审查仍未完成。
- `release_status` 继续为 `blocked`，Steam 上传继续未授权、未执行；D16-S01/S02 不启动。本次仅同步配置事实，不扩大验收范围。

## 2026-08-12 A06 审计阻断原因完整性修复

- `tools/d16_a06_release_audit.py` 现在完整透传发布清单的 7 条项目级 `release_manifest_blocking_reasons`，并将 `steam_upload_not_authorized` 单独放入 `policy_blocking_reasons`；不再把 Steam 政策项误显示为唯一阻断。
- 当前 `tools/results/d16_a06_release_audit.json`：19 项本机/安装器检查全为 `true`，`missing_static_evidence=[]`，`technical_preparation_status=PASS`，`local_blocking_reasons=[]`。
- `test_d16_a04_a06_audits.py` 专项审计测试当前 **20/20 PASS**。这只关闭 A06 审计结果完整性与本机技术准备子门禁；目标设备、原生崩溃、最终隐私复核及 A01—A05 清单阻断仍未完成，D16-A06 总体保持 **IN_PROGRESS**。
- 工具测试全集当前 **61/61 PASS**，0 failures、0 errors；原始结果见 `tools/results/tools_full_run_current.err.log`。

## 2026-08-12 A06 检查项计数校准

### 状态

**IN_PROGRESS（本机技术准备 PASS，项目发布门禁仍阻断）**。

### 修改文件与接口

- `tools/d16_a06_release_audit.py`：新增 `checks.steam_upload_policy_safe`，验证发布清单明确保持 Steam 未授权、未执行；审计器仍不连接 Steam、不上传文件。
- `tools/tests/test_d16_a04_a06_audits.py`：在完整清单测试中断言该检查项为真。
- `tools/results/d16_a06_release_audit_current.json`：刷新权威结果。

数据所有权仍归发布配置/发布审计模块；本检查只读 `D16ReleaseManifest`，不改变游戏状态、存档、契约或外部发布状态。

### 原始验证

- `python tools/d16_a06_release_audit.py game --json-out tools/results/d16_a06_release_audit_current.json`：**19/19 checks true**；
- `missing_static_evidence=[]`、技术准备 `PASS`、发布状态 `blocked`；
- `python -m unittest tools.tests.test_d16_a04_a06_audits -v`：**21/21 PASS**；
- 项目级阻断仍为 7 条，包含 A01—A06 未闭合外部证据和 Steam 未授权；没有执行 Steam 上传。

全量工具回归在该修复后重新执行：`python -m unittest discover -s tools/tests -p 'test_*.py' -q`，**69/69 PASS**（95.562s）。

### 验收结论与下游

已修复 A06 审计覆盖与文档计数不一致的问题；没有关闭 A06 总门禁。目标设备、原生崩溃、最终隐私人工复核及 A01—A05 项目级阻断完成前，D16-S01/S02 继续不启动。

## 2026-08-12 当前源码 Windows Player 复验

- 使用当前工作区源码重新构建 StandaloneWindows64：`tools/results/d16_a06_windows_build_unity6_current.log` 记录 `Build Finished, Result: Success`，Unity batchmode 返回码为 0。
- 对应产物为 `artifacts/d16_a06_windows_build/WhalesIslandReef.exe`，SHA-256：`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。
- 以 `-batchmode -nographics -quit` 启动当前 Player，日志 `tools/results/d16_a06_windows_player_smoke_current_0812.log` 检出 MainMenu EventSystem、InputSystemUIInputModule、UI 初始化和 D16 设置应用；Error/Exception/Crash/No cameras 扫描为 0。无图形批处理模式下 Player 不自行退出，验收进程已结束。
- 该复验只关闭当前源码的本机 Windows 构建/启动子门禁；目标设备性能、原生崩溃行为、最终人工隐私审查、A01—A05 的项目级阻断和 Steam 授权仍未闭合，A06 总体保持 **IN_PROGRESS**。

## 2026-08-12 当前权威审计复验

再次执行 `python tools/d16_a06_release_audit.py game --json-out tools/results/d16_a06_release_audit.json`：19 项本机/安装器检查全为 true，`missing_static_evidence=[]`，`technical_preparation_status=PASS`，`local_blocking_reasons=[]`。

发布状态仍为 `blocked`：目标设备、原生崩溃、最终人工隐私审查、A01—A05 的未闭合清单以及 Steam 未授权/未执行仍保留在发布阻断原因中。本轮未执行 Steam 上传；A06 总体继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前状态复核

以 `game` 为项目根重新执行 A06 审计：19 项本机/安装器检查全为 true，`missing_static_evidence=[]`，`technical_preparation_status=PASS`，`local_blocking_reasons=[]`。发布清单仍保留目标设备、原生崩溃、隐私、A01—A05 和 Steam 未授权阻断；A06 总体不标记 PASS，且未执行 Steam 上传。

## 2026-08-12 当前发布审计输出

- 当前证据：`tools/results/d16_a06_release_audit_current.json`，19/19 本机与安装器检查为 true，`missing_static_evidence=[]`，技术准备为 `PASS`，发布状态为 `blocked`。
- 保留的 7 条项目级阻断为：A01 目标性能、A02 物理断电/跨平台、A03 真机可访问性、A04 人工审核、A05 placeholder、A06 目标设备/原生崩溃/隐私，以及 Steam 未授权；本轮未执行 Steam 上传。
- A06 总体继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 异常退出会话标记回归

### 修改文件与接口

- `game/Assets/Scripts/Gameplay/D16/D16LocalDiagnostics.cs`：新增 `SessionMarkerPath`、`SessionMarkerFileName` 和 `SessionMarkerToken`；同意后开始会话，正常退出/撤回同意时清理；下次启动检测到遗留 marker 时写入 `unexpected_exit` 脱敏分类。
- `game/Assets/Tests/PlayMode/D16LocalDiagnosticsTests.cs`：新增异常退出 marker、脱敏记录和正常清理回归。
- `game/Assets/Resources/D16ReleaseManifest.json`：登记 marker 的本地脱敏用途，不改变外部上传策略。

### 数据所有权与边界

该功能属于 D16 本地诊断表现/隐私模块，只读会话生命周期和 Unity 运行环境元数据；不读取存档、领域事实、对话正文，不写 Systems、contracts 或网络，不提供原生转储能力。

### 原始验证

- 聚焦 Unity 6 PlayMode：`tools/results/d16_a06_session_marker_playmode_current_unity6.xml`，**1/1 PASS**。
- 当前 D16 PlayMode 全量：`tools/results/d16_a06_session_marker_full_current_unity6.xml`，**44/44 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error。

### 验收结论

本轮关闭托管诊断会话 marker 的代码级缺口，不关闭原生崩溃、目标设备和人工隐私门禁；D16-A06 继续 **IN_PROGRESS**，D16-S01/S02 不启动，Steam 未上传。

### 后续回归

- A06 权威审计：19/19 checks true，`missing_static_evidence=[]`，技术准备 PASS，发布状态 blocked。
- AgentCore、Save.Tests、Assembly-CSharp、D16.PlayMode.Tests：均 0 warning / 0 error；全量工具测试 **69/69 PASS**（97.455s）。
- Systems 边界扫描与 `git diff --check` PASS；A06 总体仍受原生崩溃、目标设备和人工隐私证据阻断。

## 2026-08-12 MainMenu Player 启动探针隔离复验

### 修改

- `D3RuntimeSmokeProbe.Install()` 现在以活动场景为门槛，仅在 `InitialHarbor` 安装；普通正式 Player 从 `MainMenu` 启动时不会等待 D3 港口对象，也不会把正常 MainMenu 启动误报为 smoke failure。
- `D16RuntimeAcceptanceTests` 新增 `D16A06MainMenuBatchPlayerDoesNotInstallHarborSmokeProbe`，覆盖 MainMenu 场景边界。

### 原始验证

- 聚焦 Unity 6 PlayMode：`tools/results/d16_a06_mainmenu_smoke_probe_playmode_current_unity6.xml` **1/1 PASS**；
- 当前 D16 PlayMode 全量：`tools/results/d16_a06_smoke_probe_full_current_unity6.xml` **46/46 PASS**，failed/inconclusive/skipped 均为 0；
- `Assembly-CSharp`、`D16.PlayMode.Tests`：0 warning / 0 error；
- 当前源码 Windows 构建：`artifacts/d16_a06_windows_build_smoke_probe/`，构建日志 `tools/results/d16_a06_windows_build_smoke_probe_current.log` 为 Success；
- Player 日志 `tools/results/d16_a06_windows_player_smoke_probe_graphics_current.log` 已完成 D3D11 初始化并进入 MainMenu，未出现 D3 smoke timeout、No cameras、Exception 或 Crash。Player 不响应 `-quit`，验收进程在启动日志确认后被结束，未将其写成自然退出码通过。

### 边界

该修复关闭本机 MainMenu 启动被 D3 港口探针误判的代码缺口，不等于目标设备、原生崩溃转储、人工隐私审查或完整发布门禁通过。A06 继续 **IN_PROGRESS**，发布清单继续 `blocked`，D16-S01/S02 不启动，Steam 未上传。

## 2026-08-12 D5 MainMenu 启动探针隔离

### 修改

- D5RuntimeSmokeProbe.Install() 增加 InitialHarbor 活动场景校验；正式 Player 从 MainMenu 启动时不再创建港口探针。
- 根因是 -d5Smoke 参数在 MainMenu 也会触发探针，探针随后等待不存在的港口对象并超时，造成错误码和假失败。
- D16RuntimeAcceptanceTests 的 MainMenu 探针回归同时检查 D3、D5 两个探针均未安装。

### 验证

- 聚焦 Unity 6：tools/results/d16_a06_d5_probe_guard_current_unity6.xml，**1/1 PASS**；
- 当前 D16 PlayMode 全量：tools/results/d16_a06_d5_guard_full_current_unity6.xml，**46/46 PASS**；
- Assembly-CSharp、D16.PlayMode.Tests：0 warning / 0 error；
- 当前源码 Windows Player 构建：artifacts/d16_a06_windows_build_d5_guard/，日志 tools/results/d16_a06_windows_build_d5_guard_current.log 为 Success；
- 权威 A06 审计：19/19 checks true，技术准备 PASS，发布状态 blocked，7 条项目级阻断完整保留。

### 交接边界

该修复只关闭 MainMenu 的 D5 smoke 误触发；未关闭目标设备、原生崩溃、人工隐私、A01—A05 外部证据或 Steam 政策门禁。D16-S01/S02 继续不启动。

## 2026-08-12 当前发布门禁复验

- `python tools/d16_a06_release_audit.py game --json-out tools/results/d16_a06_release_audit_current.json`：**19/19 checks true**，`missing_static_evidence=[]`，技术准备 **PASS**；
- 发布清单仍 **blocked**，保留 7 条项目级阻断；`local_blocking_reasons=[]`，Steam 上传策略仍为未授权且未执行；
- 该复验只确认本机静态与安装器准备没有新增缺口，不能代替目标设备、原生崩溃、最终隐私人工复核或 A01—A05 外部证据。

## 2026-08-12 本地诊断隐私专项复验

### 验证

- Unity 6 PlayMode `-force-d3d11`，筛选 `D16LocalDiagnostics`：`tools/results/d16_a06_privacy_current_20260812.xml`，**7/7 PASS**，0 failed / 0 skipped / 0 inconclusive；原始日志为 `tools/results/d16_a06_privacy_current_20260812.log`。
- 覆盖 consent 后写入、撤回与删除、线程诊断排队和脱敏、异常会话标记清理，以及隐私面板 opt-in/delete/close。
- 静态发布审计重新生成 `tools/results/d16_a06_release_audit_current.json`：**19/19 checks true**、`missing_static_evidence=[]`、技术准备 PASS、发布清单 blocked。

### 交接边界

本轮只确认当前 Windows/Unity 6 自动化隐私实现没有回归；不能替代目标设备实测、native crash/转储审计、最终人工隐私复核，不能清除 A01—A05 的项目级阻断，也不能授权 Steam 上传。A06 继续 **IN_PROGRESS**，D16-S01/S02 继续不启动。

## 2026-08-12 诊断文件原子替换

### 修改

- `game/Assets/Scripts/Gameplay/D16/D16LocalDiagnostics.cs` 的会话标记和日志轮转统一使用同目录原子替换；目标存在时使用替换提交，目标不存在时移动临时文件，避免先删除旧日志造成崩溃窗口丢失记录。
- `game/Assets/Tests/PlayMode/D16LocalDiagnosticsTests.cs` 新增 `DiagnosticFileReplacementKeepsOldTargetUntilAtomicCommit`，通过私有替换入口验证新目标内容和临时文件清理。

### 验证

- `tools/results/d16_a06_diagnostics_atomic_current_20260812.xml`：聚焦 **8/8 PASS**；
- `tools/results/d16_full_after_diagnostics_atomic_20260812.xml`：D16 PlayMode **54/54 PASS**，0 failed / 0 skipped / 0 inconclusive。

### 交接边界

本轮关闭的是本机诊断标记/日志轮转的代码级原子提交缺口；目标设备、native crash、跨平台安装、最终人工隐私审查和 A01—A05 外部证据仍未完成。A06 保持 **IN_PROGRESS**，发布清单保持 `blocked`，D16-S01/S02 不启动，Steam 未上传。
## 2026-08-12 总纲顺序下的 A06 当前复验

### 状态

**本机技术准备 PASS；发布门禁 IN_PROGRESS。**

### 当前证据

- `tools/results/d16_a06_release_audit_current_goal.json`：19/19 本机与安装器检查 true，`missing_static_evidence=[]`，技术准备 PASS；
- 发布清单仍为 `blocked`，7 条阻断完整保留：A01 目标性能、A02 断电/跨平台、A03 真机可访问性、A04 人工审查、A05 placeholder、A06 目标设备/native crash/隐私、Steam 未授权；
- 当前 D16 PlayMode 全量 `tools/results/d16_full_current_goal_20260812.xml`：54/54 PASS；
- Steam 上传未授权、未执行。

### 边界

本机静态/安装器/托管诊断通过不等价于目标设备、原生崩溃和最终人工隐私审查通过。A06 继续 IN_PROGRESS；D16-S01/S02 不启动。

## 2026-08-12 A06 当前交接快照

- 技术准备审计：19/19 checks true，`missing_static_evidence=[]`；便携安装器与安装/更新烟测证据已存在。
- 发布审计仍为 `blocked`，7 条项目级阻断不变：A01 目标性能、A02 断电/跨平台、A03 真机可访问性、A04 人工审查、A05 placeholder、A06 目标设备/native crash/隐私、Steam 未授权。
- Steam 上传未授权、未执行；没有把本机 Player/Unity/托管诊断结果写成 native crash 或目标设备证据。

结论：A06 本机技术准备子门禁 PASS，总体 **IN_PROGRESS**；发布仍被真实外部证据和授权门禁阻断。

## 2026-08-12 当前最终复核

- `tools/results/d16_a06_release_audit_current_goal_20260812.json`：本机技术准备 **19/19 true**、`missing_static_evidence=[]`；
- 便携安装器及正向/负向更新、卸载烟测已留证；发布清单仍为 `blocked`，保留 7 条项目级阻断；
- A06 总体仍 **IN_PROGRESS**：目标机安装/更新/卸载、原生崩溃复核、最终人工隐私复核和 Steam 授权仍缺。

## 2026-08-13 当前只读审计刷新

- `tools/results/d16_a06_release_audit_current_20260813.json`：本机技术准备 **19/19 PASS**，`missing_static_evidence=[]`；正式便携安装器及正向/负向更新、卸载和存档保留烟测保持通过。
- 发布清单仍为 `blocked`，7 条阻断保持不变：A01 目标性能、A02 断电/跨平台、A03 真机可访问性、A04 人工审查、A05 placeholder、A06 目标机/native crash/隐私、Steam 未授权。

结论：A06 本机技术准备子门禁保持 PASS，总体 **IN_PROGRESS**；本轮没有执行 Steam、目标机原生崩溃或人工隐私审查。

## 2026-08-13 最终本机审计快照

- `tools/results/d16_a06_release_audit_current_20260813.json`：19/19 本机与安装器技术检查为 `true`，`missing_static_evidence=[]`，`technical_preparation_status=PASS`。
- 发布清单继续为 `blocked`，7 条项目级阻断保持：A01 目标性能、A02 断电/跨平台、A03 真机可访问性、A04 人工审核、A05 placeholder、A06 目标机/native crash/隐私、Steam 未授权。
- 本轮完成的是状态核对和文档留痕；没有执行 Steam 上传、目标设备原生崩溃测试或最终人工隐私审查，也没有把本机技术准备改写成 A06 总体 PASS。
