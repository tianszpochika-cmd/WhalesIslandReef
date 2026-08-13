# D16-A02 存档与迁移 QA 报告

## 最终结论

状态：**IN_PROGRESS，存档安全与迁移子门禁已执行，完整矩阵未通过**。

D15-S02 已通过，Save.Tests 已可构建；当前已完成本地文件安全、Windows 强制终止和有限长线压力子门禁，但物理断电、跨平台恢复、完整档案/重复结算压力和 D16-S01 仍未通过。本文件不将局部结果标记为 A02 总 PASS。

## 门禁复核

### D15-S02

命令：

python tools/d15_s02_content_qa.py game/Assets/Resources/ContentRegistry --json-out tools/results/d15_s02_content_qa_result.json

历史结果：D15_S02_QA_EXIT=1；当前顺序复验结果：D15-S02 PASS（0 errors、0 warnings）。

已知阻断包括：

- 两条植被生境字段冲突；
- D15-A06 资源缺少历史审查/证据/敏感标签/跳过策略和部分 LOD 字段；
- D15-A07 的 18 次引用指向 12 个未定义宠物外观 ID；
- D15-A08 缺测试、LOD-N/A 和资源状态字段；
- A01—A07 共 2536 个 display key 没有可验证的中英本地化包证明；
- 24 个抽样中 9 个缺少明确 save/restore 标记。

### D8-A05

已读取：

- docs/game/design/common/D8_SAVE_MIGRATION.md；
- game/Assets/Scripts/Systems/Save/SaveMigration.cs；
- game/Assets/Scripts/Systems/Save/SaveSystem.cs；
- game/Assets/Tests/EditMode/SaveSystemTests.cs。

现有代码明确包含 D7 到 save_version=3 的结构迁移、重复迁移保护、事务回滚、未知版本拒绝、部分 JSON 损坏默认值和 D7 字段保留测试入口。但当前命令：

dotnet build game/Save.Tests.csproj --no-restore

结果：失败，CrewAgent.cs(18) 缺少 D10CrewBehaviorState（CS0246）。因此本次不能把 D8-A05 记为“可用”或把 Unity EditMode 测试记为通过。

## 当前 Save/Storage 基线

- SaveMigration 在原 SQLite 文件内使用 BEGIN IMMEDIATE TRANSACTION、提交前异常注入和 ROLLBACK。
- 迁移失败的事务路径会保留 D7 表和字段；D8 规范也要求原始字段不覆盖。
- 当前未找到 D16 所需的独立文件级备份、临时文件提交、断电后备份选择和恢复演练证据；SQLite 事务回滚不能替代本任务要求的文件级备份 QA。
- SaveSystem 已保留 D5 Steam/云存档状态接口，如 QueueCloudUpload、MarkCloudSynced 和 PendingCloudRevision；本次没有连接外部服务，也没有把它当作真实云存档通过。

## D16-A02 测试矩阵状态

| 档案/故障类型 | 本次状态 | 原因 |
|---|---|---|
| D7 基础档 | BLOCKED | Save.Tests 无法构建 |
| D8/v3 迁移档 | BLOCKED | 同上，不能执行迁移断言 |
| D9 航行/事件档 | BLOCKED | 同上 |
| 完整资产档 | BLOCKED | D15-S02 未通过且 Save.Tests 无法构建 |
| 岛屿经营档 | BLOCKED | 同上 |
| 负债档 | BLOCKED | 同上 |
| 战斗中档 | BLOCKED | 同上 |
| 生态变化档 | BLOCKED | 同上 |
| 关系档 | BLOCKED | 同上 |
| 损坏档/部分 JSON 损坏 | BLOCKED | 同上 |
| 重复结算/重复迁移 | BLOCKED | 代码有 D8 测试入口，但未能执行 |
| 断电/写入中断 | BLOCKED | 无文件级备份/恢复实测 |
| 迁移失败不覆盖原档 | BLOCKED | 事务代码存在，但测试程序集未构建 |
| 备份恢复 | BLOCKED | 尚未实现/验证 D16 级备份入口 |
| 云存档接口预留 | STATIC ONLY | 仅确认 D5 本地状态接口，未接外部服务 |

## 本次未做的工作

- 未修改 Save/Storage、SaveMigration、存档模型或测试；
- 未删除旧字段、坏档样本或迁移测试；
- 未加入网络、云服务或 LLM；
- 未执行 D7、D8、D9、资产、岛屿经营、负债、战斗、生态、关系和损坏档测试；
- 未宣称断电恢复、备份恢复、重复结算保护或云接口通过；
- 未创建任何会覆盖原始存档的迁移或恢复路径。

## 重新开工门槛

1. D15-S02 统一 QA 错误归零并重新出具通过报告；
2. 修复 D10CrewBehaviorState 后，Save.Tests 可构建；
3. 先执行并通过 D8-A05 全部迁移测试；
4. 增加或确认文件级备份/临时写入/原子替换/恢复策略，覆盖断电和写入中断；
5. 执行本报告全部档案矩阵，记录重复结算、失败保护、错误信息和备份恢复证据；
6. 云存档只保留本地接口预留，任何外部服务接入必须另开契约和授权任务。

## 2026-08-11 门禁复验更新

D15-S02 已通过统一 QA；AgentCore、Systems、Save.Tests standalone build 均为 0 warning / 0 error。D16-A02 当前状态更新为 **PENDING，等待运行时测试环境**：已尝试用 Unity EditMode batch runner 执行存档测试，但 Unity 停在 `Licensing::Module` 初始化，未生成 Test Runner 结果文件，日志保留在 `tools/results/d16_a02_unity_test.log`。因此迁移代码“可构建”已确认，运行时测试、文件级备份/原子替换、断电恢复和完整档案矩阵仍未通过。

## 2026-08-11 D16-A02 实现进度更新

当前状态：**IN_PROGRESS，完整运行时矩阵仍未通过**。

已落地 `game/Assets/Scripts/Systems/Save/SaveFileSafety.cs`：

- 覆盖写入/新游戏/迁移前自动创建 `save.db.backups` 文件级备份；
- 临时文件写入、flush 后原子替换；
- 从明确备份恢复时，先保留恢复前文件的回滚副本；
- 同步处理 SQLite `-wal` / `-shm` sidecar；
- 不把云存档或外部服务当作本地恢复的替代；
- `SaveSystem.Save` 和 `NewGame` 失败时尝试恢复最近一次有效主文件，`MigrateToV3` 返回备份路径。

独立证据：

- `dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet run --project tools/d16_a02_runner/D16A02Runner.csproj --no-restore`：5/5 通过，覆盖备份恢复、原子 bytes 写入、新游戏覆盖备份、保存失败回滚、迁移备份与重复迁移。

仍未宣称通过：真实断电/进程杀死时序、所有 D7—D14 档案矩阵、Unity EditMode/PlayMode、迁移失败的真实电源中断、跨平台文件系统语义。Unity batch runner 仍停在 `Licensing::Module`，这些项目必须在可运行 Unity 环境下复验。

## 2026-08-11 D16-A02 故障注入与档案矩阵更新

当前状态：**IN_PROGRESS（进程中止和矩阵证据已补齐，真实断电/跨平台门禁仍未关闭）**。

### 新增实现

- `SaveFileSafety` 增加默认关闭的测试故障注入点：临时文件 flush 后、原子替换后；正常运行时 hook 为 `null`，不改变生产行为。
- `SaveMigration` 增加仅供测试的提交前进程中止点；默认迁移路径不启用。
- `D16SaveSafetyTests` 提供独立子进程入口；`tools/d16_a02_runner` 只在显式传入故障模式时触发退出码 73/74/75。
- 新增 `tools/d16_a02_fault_injection.py`，父进程在子进程退出后重新打开文件和 SQLite，检查旧档、新档、临时文件、备份和事务恢复。
- 新增 `tools/d16_a02_archive_matrix.py`，只从 Unity 原始 XML 中匹配真实 Passed 用例，不凭空声明覆盖。

### 原始验证结果

- `dotnet build game/Save.Tests.csproj --no-restore`：0 warning / 0 error。
- `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore`：0 warning / 0 error。
- `python tools/d16_a02_fault_injection.py`：3/3 PASS：flush 后中止保留旧文件、替换后中止保留完整新文件、迁移提交前中止后 SQLite 完整且 D7 字段保留，备份数=1。
- Unity 6 EditMode：`D2/D3 EditMode tests: passed=291, failed=0, skipped=0`；日志：`tools/results/d16_a02_editmode_unity6.log`；XML：`game/D2_TestResults.xml`。
- `python tools/d16_a02_archive_matrix.py`：10/10 档案类别有真实 Passed 用例，0 failed/inconclusive；报告：`tools/results/d16_a02_archive_matrix_result.json`。
- `dotnet tools/d16_a02_runner/bin/Debug/net9.0/D16A02Runner.dll`：D16-A02 存档安全测试 5/5 PASS。

### 仍未关闭的门禁

- 进程中止是可控退出演练，不等同于物理断电、强制终止或每种 Windows 文件系统/杀毒软件时序。
- 仍需目标 Windows 配置下的断电/强制杀进程演练、跨平台恢复矩阵、发布升级覆盖和长线存档压力测试。
- Unity 原始矩阵证明现有 D7—D14 测试用例通过，不代表每一种生产内容组合都已生成独立存档样本；D16-S01 仍需完整新游戏长线运行。

## 2026-08-11 真实强制终止复验更新

当前状态：**IN_PROGRESS（Windows 强制终止边界已通过；物理断电、跨平台和长线门禁仍未关闭）**。

本轮在 Windows 子进程真正到达故障边界后，由父进程调用 `Process.kill()` 强制终止；子进程不是主动调用 `Environment.Exit`。三个边界均写入 ready 标记后才允许父进程终止，确保检查点时序可复核：

- 临时文件 flush 后终止：旧文件保持 `stable-force-v1`，临时文件在终止后可观察到；
- 原子替换后终止：目标文件保持完整 `new-force-replace`，没有临时文件残留；
- SQLite 迁移提交前终止：`PRAGMA integrity_check=ok`、`user_version=1`、无 `save_metadata`/`v3_entities`，D7 玩家“强制终止验收船长”和 500 金钱保留，备份数为 1。

真实强制终止结果：`tools/results/d16_a02_fault_injection_result.json` 中 `real_force_kill_passed=true`、`passed=true`；受控退出对照仍为 3/3。该证据证明 Windows 进程终止时序，不宣称物理断电等价。

同轮验证：

- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet tools/d16_a02_runner/bin/Debug/net9.0/D16A02Runner.dll`：5/5 PASS；
- `python tools/d16_a02_archive_matrix.py tools/results/d16_a02_long_run_editmode_unity6.xml`：296 个 EditMode 用例、0 failed/inconclusive、10/10 档案类别覆盖；
- Unity 6 EditMode：`tools/results/d16_a02_long_run_editmode_unity6.xml` 为 296/296 PASS。

仍未关闭：物理断电/不同文件系统时序、跨平台恢复、重复结算的长线压力、完整新游戏链路和 D16-S01。下一步仍需在不扩大 Save/Storage 写入边界的前提下，补长线存档压力与跨平台证据。

## 2026-08-11 长线存档压力更新

长线压力首次运行发现真实缺陷：`SaveSystem.NewGame` 只清空旧 `v3_entities`/`save_metadata` 的行，保留空表；下一次新档迁移会被 `SaveMigration` 判定为 `metadata_incomplete`。修复为新游戏覆盖旧档时删除旧 v3 投影表，保留 D7 三张兼容表和种子表，允许新档重新建立完整迁移投影。

修复后执行：`python tools/d16_a02_long_run.py 100`，结果 `tools/results/d16_a02_long_run_result.json`：

- 100 次连续新游戏、保存、读取、迁移、重复迁移和恢复；
- 最终 SQLite `integrity_check=ok`、`user_version=1`；
- `save_metadata` 与 `v3_entities` 均存在且完整；
- 最终玩家为“长线存档船长-99”，金钱 599；
- 备份数 399；
- 进程退出码 0，`passed=true`。

这关闭了当前 Windows/本地文件系统上的重复迁移与短程长线压力子门禁，但不等于物理断电、跨平台文件系统或 D16-S01 全流程长线通过；D16-A02 继续保持 **IN_PROGRESS**。

## 2026-08-12 路径与 SQLite sidecar 保护补充

为增加平台无关的文件语义证据，在 Save/Storage 测试模块新增 `UnicodeAndSidecarBackupRestorePreservePortablePathSemantics`：

- 使用包含中文和空格的临时存档路径；
- 创建并备份主数据库、`-wal` 与 `-shm` sidecar；
- 人为破坏主文件、删除 WAL、修改 SHM 后，从备份原子恢复；
- 验证主文件和两个 sidecar 均恢复，且恢复前损坏文件仍保留为 rollback backup。

真实结果：

- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet tools/d16_a02_runner/bin/Debug/net9.0/D16A02Runner.dll`：**6/6 PASS**；
- Unity 6 EditMode `Save.Tests` 过滤 `D16SaveSafetyTests`：**6/6 PASS**，结果 `tools/results/d16_a02_save_safety_editmode_unity6.xml`；
- Unity 6 EditMode `Save.Tests` 全量：**293/293 PASS**，结果 `tools/results/d16_a02_save_tests_editmode_unity6.xml`；
- `python tools/d16_a02_fault_injection.py`：受控边界 3/3、Windows `Process.kill()` 边界 3/3、最终 `passed=true`；
- `python tools/d16_a02_long_run.py 100`：100 次循环、SQLite integrity `ok`、事实与备份数校验通过。

该测试证明当前 Windows/.NET 环境下的 Unicode/空格路径和 SQLite sidecar 复制恢复行为；它不替代真实 Linux/macOS 文件系统、物理断电或目标设备证据。D16-A02 仍为 **IN_PROGRESS**。

## 2026-08-12 当前工作区复验

本轮重新执行了 D16-A02 的独立证据链，没有把旧 XML 当作当前结果：

- Unity 6 EditMode `Save.Tests` 全量：**293/293 PASS**，`tools/results/d16_a02_save_tests_current_editmode_unity6.xml`；
- `python tools/d16_a02_fault_injection.py`：受控 flush/replace/migration **3/3 PASS**，Windows `Process.kill()` **3/3 PASS**，总 `passed=true`；
- `python tools/d16_a02_archive_matrix.py`：基于当前 `game/D2_TestResults.xml` 的 292 个用例，10/10 档案类别均有真实 Passed 覆盖，0 failed/inconclusive；
- `python tools/d16_a02_long_run.py 100`：100 次循环通过，SQLite integrity `ok`，最终事实与备份数校验通过；
- `dotnet build game/Save.Tests.csproj`（在 Unity 刷新后恢复临时 NuGet 资产后）：0 warning / 0 error。

这些结果关闭了当前 Windows/.NET 的存档安全、进程中止、档案矩阵和有限长线压力子门禁；仍不能等同物理断电、Linux/macOS 文件系统、目标设备或 D16-S01 全流程，D16-A02 继续 **IN_PROGRESS**。

## 2026-08-12 多条结算事实多轮存档幂等压力回归

为补强“重复结算保护”的本机证据，在 `D16SaveSafetyTests` 新增 `SettlementClaimsRemainIdempotentAcrossRepeatedSaveRoundTrips`：

- 构造 24 条不同 `event_id + save_revision` 的交易结算事实并首次声明；
- 每轮通过真实 `SaveSystem.SaveD3Runtime` 写入，清空 `GameState`，重新 `Load` 和 `RestoreD3TradeEventLog`；
- 连续 5 轮检查所有事实数量不变，重复结算声明全部被拦截，重复事实追加全部去重；
- 独立 `D16A02Runner` 默认将该测试纳入安全回归，结果由原来的 6 项扩展为 **7/7 PASS**。

验证：`dotnet build game/Save.Tests.csproj --no-restore -v:minimal` 和 `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore -v:minimal` 均 0 warning / 0 error；`dotnet tools/d16_a02_runner/bin/Debug/net9.0/D16A02Runner.dll` 为 7/7 PASS。

该结果只关闭当前 Windows/.NET 下多条结算事实跨多轮 Save 往返的代码级回归风险；不等同于所有 D7—D14 结算组合、物理断电、Linux/macOS 文件系统、目标设备或 D16-S01 全流程通过。D16-A02 继续 **IN_PROGRESS**。

## 2026-08-12 A02 本机安全 runner 复验

`D16A02Runner` 已重新构建并执行，7/7 PASS，覆盖备份/回滚、原子写入、失败不覆盖、迁移幂等、重复结算跨存档往返和 Unicode/sidecar 备份路径。`AgentCore`、`Save.Tests` 构建均 0 warning / 0 error。

本次复验不扩大验收范围；物理断电、跨平台文件系统、目标设备和完整 D7—D14 组合仍需真实证据，D16-A02 继续 **IN_PROGRESS**。

## 2026-08-12 结算幂等跨存档回归

新增 `D16SaveSafetyTests.SettlementClaimSurvivesSaveRoundTripAndCannotRepeat`，通过真实 `SaveSystem.SaveD3Runtime` 写入 D3 EventLog 快照，清空内存后重新 `Load`，再调用 `RestoreD3TradeEventLog`：

- 首次结算声明成功；同一进程第二次声明返回 `AlreadyClaimed`；
- 读档恢复后仍返回 `AlreadyClaimed`；
- 读档后重复追加同一 `event_id + save_revision` 返回 Duplicate，日志数量保持 1；
- 不发放奖励、不修改领域数值，Save 只持有并恢复 EventLog 快照。

验证结果：

- Unity 6 focused EditMode：**1/1 PASS**，`tools/results/d16_a02_settlement_roundtrip_editmode_unity6.xml`；
- Unity 6 `Save.Tests` 全量：**298/298 PASS**，`tools/results/d16_a02_save_tests_after_settlement_editmode_unity6.xml`；
- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error。

该回归关闭了 D3 EventLog settlement claim 的本地存档往返证据缺口；不等于所有 D7—D14 结算、真实物理断电、Linux/macOS 文件系统或 D16-S01 长线全流程已通过。D16-A02 继续 **IN_PROGRESS**。

## 2026-08-12 真实 SQLite 损坏档恢复与完整 Save.Tests 复验

本轮补充了真实 SQLite 主档恢复场景：创建第一份有效新档，覆盖生成第二份新档后取得备份，直接破坏当前主数据库；确认 `SaveSystem.Load()` 返回失败，再用最新备份原子恢复，保留恢复前损坏文件为 rollback，并重新加载第一份船长事实。该用例不是文本文件替身，覆盖了实际 SQLite 文件链路。

同时将该用例和已存在的结算跨存档用例纳入 `D16A02Runner` 默认清单，避免独立 runner 与 Unity 测试夹具不一致。

当前证据：

- `D16A02Runner`：**9/9 PASS**；
- Unity 6 `Save.Tests` 全量：**302/302 PASS**，`tools/results/d16_a02_save_tests_after_sqlite_recovery_editmode_unity6.xml`；
- D16-A02 档案矩阵：基于该 XML 的 10/10 类别均有真实 Passed 用例，0 failed/inconclusive，`tools/results/d16_a02_archive_matrix_result.json`；
- Save.Tests 与独立 runner 构建均 0 warning / 0 error。

这只关闭当前 Windows/.NET 本机的 SQLite 损坏恢复与测试清单覆盖子门禁；物理断电、跨平台恢复、目标设备、全生产 D7—D14 组合和 D16-S01 仍未完成，D16-A02 保持 **IN_PROGRESS**。

## 2026-08-12 当前 A02 故障边界与长线复验

- `python tools/d16_a02_fault_injection.py`：受控写入中止 **3/3 PASS**，Windows `Process.kill()` 边界 **3/3 PASS**，总结果 `passed=true`；flush 前保留旧文件、replace 后保留完整新文件、迁移提交前 SQLite 完整且旧版字段保留。
- `python tools/d16_a02_long_run.py 100`：**PASS**；100 次真实 SaveSystem 压力后 SQLite `integrity_check=ok`、`user_version=1`、迁移后的 v3 表完整、最终船长事实正确、备份数 399。
- `python tools/d16_a04_review_queue.py game --json-out tools/results/d16_a04_review_queue.json --markdown-out tools/results/d16_a04_review_queue.md`：当前 A04 Pending 仍为 **267**，拆为 12 个不混合范围的人工批次；工具不写回 Registry、不自动批准。

这些结果补强 Windows/.NET 本机的写入中断与长线存档证据，但不替代物理断电、Linux/macOS/跨平台恢复、目标设备和完整 D7—D14 生产组合；D16-A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前 Save.Tests 权威复验

- Unity 6 EditMode `Save.Tests` 当前结果：`tools/results/d16_current_save_tests_editmode.xml`，**302/302 PASS**，failed/inconclusive/skipped 均为 0。
- `tools/results/d16_a02_archive_matrix_result.json`（来源为 `tools/results/d16_current_save_tests_editmode.xml`）：D7 基础档、D8 迁移、D9 航行/贸易、完整资产、岛屿经营、负债、战斗、生态、船员关系和损坏档 **10/10 类别真实匹配**，`passed=true`。
- 本轮没有修改 Save 生产逻辑、存档 schema 或领域数值，只更新当前 Unity 证据；物理断电、Linux/macOS/跨平台恢复、目标设备和 D16-S01 完整新游戏长线仍未取得证据，D16-A02 继续 **IN_PROGRESS**。

## 2026-08-12 损坏主档自动恢复路径

### 修复内容

此前 `SaveSystem.HasSave()` 对不可读 SQLite 只返回 `false`，不会尝试已经存在的文件级备份；因此写入中断后用户只能看到“没有存档”，不能从主菜单/读档入口恢复。本轮在 Save/Storage 模块补齐安全恢复路径：

- `HasSave()` 先检查 SQLite `integrity_check`、玩家行和关联船行；
- 主档不可读或缺失时，按备份时间倒序尝试 `.backups/save_*.db`；
- 每次候选恢复仍通过 `SaveFileSafety.RestoreBackup`，恢复前主文件保留为 rollback backup；
- 恢复后的文件再次验证 SQLite 完整性、玩家和船实体，失败候选继续尝试更早备份；
- 诊断输出使用 stderr，保证 Unity 运行日志和无 Unity runtime 的独立 runner 都不会因 `UnityEngine.Debug` 原生调用而崩溃；
- 没有修改存档 Schema、领域数值、迁移版本、网络/云服务或数据所有权。

### 当前验证

- `dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet tools/d16_a02_runner/bin/Debug/net9.0/D16A02Runner.dll`：9/9 PASS；
- Unity 6 focused `D16SaveSafetyTests`：9/9 PASS，证据 `tools/results/d16_a02_save_recovery_editmode_current.xml`；
- Unity 6 `Save.Tests` 全量：302/302 PASS，证据 `tools/results/d16_a02_save_tests_after_auto_recovery_editmode_unity6.xml`；
- `python tools/d16_a02_archive_matrix.py tools/results/d16_a02_save_tests_after_auto_recovery_editmode_unity6.xml`：10/10 类别、0 failed/inconclusive；
- `python tools/d16_a02_fault_injection.py`：受控中止 3/3、Windows `Process.kill()` 3/3、总结果 `passed=true`；
- `python tools/d16_a02_long_run.py 100`：100 次压力循环通过，SQLite `integrity_check=ok`，最终事实和备份数正确。

### 验收边界

本轮关闭了“损坏主档只能被当成无存档、没有自动恢复入口”的本机 Save/Storage 子缺口。A02 总体仍为 **IN_PROGRESS**：上述证据仍局限于当前 Windows/.NET 环境，不能替代物理断电、Linux/macOS/跨平台文件系统、目标设备和完整 D7—D14 生产组合。D16-S01/S02 继续不启动。

## 2026-08-12 跨平台 runner 路径与初始化修复

- `tools/d16_a02_runner/Program.cs` 仅在 Windows 调用 `kernel32.SetDllDirectory`；非 Windows 交由系统原生加载器解析 sqlite。
- `tools/d16_a02_runner/D16A02Runner.csproj` 改用仓库相对依赖路径，并支持 `-p:UnityManagedDir=<平台 UnityEngine 托管目录>`；未提供有效目录时显式失败，不再隐式依赖 Windows 绝对路径。
- Windows 回归：runner 构建 0 warning / 0 error，存档安全 9/9，故障注入受控中止 3/3、强制终止 3/3，总结果 `passed=true`。

该修复只使跨平台验收入口具备正确的平台分支和可配置依赖路径，尚未产生 Linux/macOS 实机或物理断电结果；D16-A02 继续 **IN_PROGRESS**。

## 2026-08-12 当前工作区 Save.Tests 复验

- Unity 6 `Save.Tests` EditMode 全量当前 XML `tools/results/d16_current_save_tests_editmode_unity6.xml`：**302/302 PASS**，0 failed、0 inconclusive、0 skipped。
- 本轮先处理 Unity 刷新造成的临时 `project.assets.json` 缺失，再执行 `dotnet build ... --no-restore`；AgentCore 与 Save.Tests 均 0 warning / 0 error。
- 该结果确认当前存档测试和 D10 工程依赖没有回归；仍不能替代物理断电、Linux/macOS 实机、跨平台文件系统和完整 D7—D14 生产组合，A02 保持 **IN_PROGRESS**。

本轮随后按依赖顺序重新生成 standalone 输出并构建/运行 `D16A02Runner`：构建 0 warning / 0 error，存档安全 **9/9 PASS**；档案矩阵 **10/10**，故障注入受控中止 **3/3**、强制终止 **3/3**，100 次长线循环通过。runner 的 build 顺序是验收约定的一部分，Unity EditMode 刷新后必须先恢复临时工程输出，不能复用旧 DLL。

## 2026-08-12 当前 Unity Save.Tests v2 复验

- Unity 6 EditMode `Save.Tests` 当前 XML `tools/results/d16_current_save_tests_editmode_unity6_v2.xml`：**302/302 PASS**，failed/inconclusive/skipped 均为 0。
- 本轮 Unity 刷新后先执行工程依赖 restore，再按顺序构建 AgentCore、Save.Tests、Assembly-CSharp，三项均 **0 warning / 0 error**；没有并行复用或旧 DLL 作为构建证据。
- 本轮未修改 Save 生产逻辑、Schema、迁移版本或领域数值；该结果只刷新当前工作区的本机 Unity 存档证据。物理断电、Linux/macOS/跨平台文件系统、目标设备和完整 D7—D14 生产组合仍缺，D16-A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前门禁边界复核

- 当前 Windows 本机 `Save.Tests` 仍为 **302/302 PASS**；D16-A02 runner **9/9 PASS**；档案矩阵 **10/10**；受控中止与 Windows 强杀故障注入各 **3/3**；100 次长线 SQLite 压力循环通过；
- 这些结果只覆盖当前 Windows/.NET 环境。物理断电、Linux/macOS/跨平台文件系统和目标设备证据仍未取得，因此 A02 继续 **IN_PROGRESS**，不能启动 D16-S01/S02。

## 2026-08-12 严格存档覆盖矩阵

为避免“测试名包含某个片段”被误判为完整业务覆盖，新增 `tools/d16_a02_archive_matrix_manifest.json`，将 D7 基础档、D8 迁移、D9 航行/任务/贸易、完整资产/D5、岛屿经营、负债/金融、战斗/奖励、生态/动物、船员关系和损坏档/迁移失败绑定到 **31 个精确测试名**。`tools/d16_a02_archive_matrix.py` 现在只接受 Unity XML 中 `result=Passed` 且名称完全相等的用例；重命名近似用例的反例已加入 Python 单元测试。

当前复验：

- 当前 Unity 6 `Save.Tests` XML：**302/302 PASS**，失败/不确定为 0；
- 严格矩阵：10/10 类别、31/31 精确用例匹配，`passed=true`；
- 工具测试新增矩阵自测 3/3 PASS；
- 本轮当前工作区复验另见 `tools/results/d16_a02_current_after_d2_save_tests_editmode_unity6.xml`：Save.Tests **302/302 PASS**；runner **9/9 PASS**；故障注入受控/强杀各 **3/3 PASS**；100 次长线 SQLite 压力通过。
- 未修改 SaveSystem、存档 Schema 或领域裁决。该改动只提高证据门槛，不能替代物理断电、Linux/macOS 和跨平台实测，因此 D16-A02 仍保持 **IN_PROGRESS**。

## 2026-08-12 当前工作树 Save.Tests 独立复验

- 当前 Unity 6 EditMode XML `tools/results/d16_current_save_tests_continuation_unity6.xml`：**302/302 PASS**，failed/inconclusive/skipped 均为 0，耗时 11.885s。
- 随后按依赖顺序恢复工程输出并构建 AgentCore、Save.Tests、Assembly-CSharp，均为 **0 warning / 0 error**。
- 该结果只确认当前工作树 Windows/.NET 存档测试没有回归；物理断电、Linux/macOS/跨平台文件系统、目标设备和完整 D7—D14 生产组合仍缺，A02 保持 **IN_PROGRESS**。

本机环境复核：Windows 主机未安装可用的 WSL/Linux 发行版；因此本轮没有把 Windows/.NET 结果扩大解释为 Linux/macOS 或跨平台证据，也没有执行虚假的跨平台测试。

## 2026-08-12 原子写入孤儿临时文件清理

### 实现

- `SaveFileSafety.CleanupTemporaryArtifacts(sourcePath)` 只匹配当前存档主文件及 `-wal`、`-shm` sidecar 的 `.tmp_` + 32 位十六进制 GUID 命名；不扫描或删除正式存档、备份和未知命名文件。
- `SaveSystem.HasSave()` 在可读性检查前调用该清理入口，使进程在 Flush 后被强制终止留下的临时文件不会长期堆积，也不会被误当成正式存档。
- 单个临时文件删除失败只跳过该文件，不阻断主菜单存档检查；清理不修改 Schema、领域数值、备份策略或存档所有权。

### 当前证据

- Unity 6 focused `D16SaveSafetyTests`：`tools/results/d16_a02_save_safety_current_unity6.xml`，**10/10 PASS**，0 failed / 0 inconclusive / 0 skipped；
- Unity 6 `Save.Tests` 全量：`tools/results/d16_a02_save_tests_full_current_unity6.xml`，**303/303 PASS**，0 failed / 0 inconclusive / 0 skipped；
- 严格档案矩阵：`tools/results/d16_a02_archive_matrix_result.json`，**10/10 类别、31/31 精确测试匹配**；
- 独立 `D16A02Runner`：**10/10 PASS**；
- `dotnet build game/Save.Tests.csproj`、`game/Assembly-CSharp.csproj`、`game/D16.PlayMode.Tests.csproj` 与 runner：均 **0 warning / 0 error**。

### 验收边界

本轮关闭了“可重复中止后孤儿原子写入临时文件没有启动清理入口”的本机代码缺口。A02 总体仍为 **IN_PROGRESS**：物理断电、Linux/macOS/跨平台文件系统、目标设备和完整 D7—D14 生产组合仍无真实证据，不能把本机回归扩大解释为跨平台或发行版通过。

## 2026-08-12 当前 A02 证据链复验

- `python tools/d16_a02_fault_injection.py`：受控中止 3/3、Windows `Process.kill()` 3/3，`passed=true`；旧文件、完整替换文件、SQLite 完整性、迁移前旧表和备份均符合预期。
- `python tools/d16_a02_long_run.py 100`：100 次真实 Save/Load/迁移/恢复通过；`integrity_check=ok`、`user_version=1`、v3 表存在、最终船长事实正确、备份 399 个。
- `tools/results/d16_a02_save_tests_full_current_unity6.xml`：Save.Tests **303/303 PASS**；严格矩阵 `d16_a02_archive_matrix_result.json`：10/10 类别、31/31 精确用例匹配，0 failed/inconclusive。

该复验只刷新 Windows/.NET 本机证据，不扩大结论。物理断电、Linux/macOS/跨平台文件系统、目标设备和完整 D7—D14 生产组合仍无真实证据，D16-A02 保持 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 sidecar 恢复失败的成组回滚

### 修复

- `SaveFileSafety.RestoreBackup(...)` 现在把主 SQLite 文件、`-wal` 和 `-shm` sidecar 视为一次恢复操作；主文件替换后任一 sidecar 失败，会从恢复前生成的 rollback backup 一起回滚三件文件。
- 没有恢复前主文件时，恢复失败会清理本次新生成的主文件和 sidecar，避免留下半恢复状态。
- 新增 `TestBeforeRestoreSidecarHook` 仅供测试注入故障；生产默认值为 null，不改变存档 Schema、迁移版本或领域数据所有权。
- `tools/d16_a02_runner/Program.cs` 将该测试加入独立 runner 的显式清单，避免 runner 漏掉本次安全边界。

### 验证

- Unity 6 focused EditMode：`tools/results/d16_a02_sidecar_rollback_current_20260812.xml`，**1/1 PASS**；
- Unity 6 `Save.Tests` 全量：`tools/results/d16_a02_save_tests_after_sidecar_rollback_20260812.xml`，**304/304 PASS**，0 failed/inconclusive/skipped；
- 严格存档矩阵：`tools/results/d16_a02_archive_matrix_sidecar_rollback_20260812.json`，**10/10 类别、31/31 精确用例匹配**；
- 独立 runner 依赖恢复后真实重建：`dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore` **0 warning / 0 error**；执行结果 **11/11 PASS**。

### 边界

本轮关闭的是 Windows/.NET 代码级 sidecar 半恢复风险，不把它扩大解释为物理断电、Linux/macOS/跨平台文件系统或目标设备证据。D16-A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

### 跨模块回归

- 存档安全改动后，当前 D16 PlayMode 全量 `tools/results/d16_full_after_sidecar_rollback_20260812.xml`：**53/53 PASS**，0 failed / 0 inconclusive / 0 skipped，耗时约 73.47s。
- 该回归确认 D16 的 UI、输入、镜头、上下船、性能控制和运行时存档链没有新增回归；不改变 A02 的跨平台/物理断电边界。

### A02 故障注入与长线复验

- `python tools/d16_a02_fault_injection.py`：受控中止 3/3、Windows `Process.kill()` 3/3，`passed=true`；旧文件保留、替换后完整新文件、迁移前 SQLite 完整性和 D7 旧表均通过。
- `python tools/d16_a02_long_run.py 100`：100 次 Save/Load/迁移/恢复通过；最终船长 `长线存档船长-99`、金钱 599、SQLite integrity `ok`、v3 表存在、备份 399 个。
- 这些是当前 Windows/.NET 的代码级证据，不是物理断电或 Linux/macOS 实机证据。
## 2026-08-12 总纲顺序下的 A02 当前证据

- D16A02Runner：**11/11 PASS**，构建 0 warning / 0 error；
- 受控中止与 Windows 强制终止：**6/6 PASS**；
- 100 次存档、读取、迁移、恢复循环：PASS，SQLite integrity `ok`，最终事实与 399 个备份符合预期；
- 当前 Save.Tests XML：304/304 PASS；严格档案矩阵：10/10 类别、31/31 精确匹配。

这些结果不替代物理断电、跨平台文件系统和目标设备恢复；A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-13 Save 多表快照事务补强

### 修复

- `game/Assets/Scripts/Systems/Save/SaveSystem.cs` 新增统一 SQLite 事务入口；`NewGame` 的覆盖、清理和初始 Player/Ship/Crew 写入，以及 `Save` 的三表快照写入，都在同一个 `BEGIN IMMEDIATE` / `COMMIT` 事务内完成。
- Player、Ship、Crew 写入函数改为复用同一连接，不再各自开启独立连接和独立提交，避免进程中断后出现“新玩家 + 旧船/旧船员”的混合快照。
- 事务异常会先执行 `ROLLBACK`，外层原有的文件级备份恢复继续保留；测试边界钩子仅在测试程序集内使用，生产默认为空。

### 证据

- Unity 6 focused `Save.Tests.D16SaveSafetyTests`：`tools/results/d16_a02_save_transaction_editmode_20260813.xml`，**12/12 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 独立 `D16A02Runner`：**12/12 PASS**；其中 `SaveTransactionRollsBackWhenInterruptedAfterPlayerWrite` 验证事务异常回滚。
- `python tools/d16_a02_fault_injection.py`：`passed=true`；新增 Windows `Process.kill()` 场景 `force_kill_during_save_after_player` 通过，SQLite integrity 为 `ok`，旧玩家 money `500`、船数量 `1`、船员数量 `1` 均保持不变。
- `dotnet build game/Save.Tests.csproj --no-restore`、`game/Assembly-CSharp.csproj --no-restore`、`tools/d16_a02_runner/D16A02Runner.csproj --no-restore`：均 **0 warning / 0 error**。

### 边界

本轮关闭了 Windows/.NET 代码级“保存多表写入不是同一事务”的缺口，并补齐了可重复的进程强制终止证据；这仍不等价于物理断电、Linux/macOS/跨平台文件系统、目标设备或完整生产组合证据。D16-A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。
