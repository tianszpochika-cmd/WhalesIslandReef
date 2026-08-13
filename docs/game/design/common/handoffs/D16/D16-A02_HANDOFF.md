# D16-A02 存档 QA Agent 交接报告

## 1. 任务信息

- 任务编号：D16-A02
- 执行日期：2026-08-11
- 前置任务：D15-S02、D8-A05
- 当前状态：IN_PROGRESS
- 是否完成存档 QA：否
- 是否允许进入后续 D16 存档验收：是，完整矩阵仍未通过

D15-S02 已通过，Save.Tests 已可构建；A02 的文件安全、Windows 强制终止和有限长线子门禁已执行，但物理断电、跨平台和完整 D16-S01 仍未通过。

## 2. 修改文件

- docs/game/design/common/D16_SAVE_MIGRATION_QA.md：阻断 QA 报告。
- docs/game/design/common/handoffs/D16/D16-A02_HANDOFF.md：本交接报告。
- .workbuddy/memory/2026-08-11.md：执行记忆日志。

未修改 Save/Storage、SaveMigration、存档模型、测试、Systems、AgentCore、contracts、server 或 web。

## 3. 前置条件结论

### D15-S02

D15-S02 的早期重新运行曾返回 D15_S02_QA_EXIT=1；该段为历史阻断记录。当前顺序复验为 PASS（0 errors、0 warnings），Save.Tests 已可构建。

### D8-A05

D8_SAVE_MIGRATION.md、SaveMigration.cs 和 SaveSystemTests.cs 中已有 D8 迁移设计、代码和测试入口，覆盖：

- 空档、D7 档、航行中档、损伤船档、负债档、半途任务档；
- 重复迁移、未知版本、部分损坏和提交前中断；
- D7 字段保留、v3 默认值和事务回滚。

但 dotnet build game/Save.Tests.csproj --no-restore 因 CrewAgent.cs(18) 缺少 D10CrewBehaviorState 失败。迁移测试未能执行，因此 D8-A05“可用”条件未满足。

## 4. 当前能力与缺口

- 事务回滚路径存在，但没有本任务要求的文件级备份和断电恢复实测。
- D5 有 QueueCloudUpload、MarkCloudSynced 和待同步 revision 的本地预留；未接外部云服务，符合“只做接口预留”的边界。
- SaveSystem 已保存多阶段 D3—D14 快照入口，但本次没有对完整资产、岛屿经营、战斗、生态和关系组合档执行验收。

## 5. 待执行档案与故障矩阵

待门禁恢复后执行：D7、D8、D9、完整资产、岛屿经营、负债、战斗中、生态变化、关系、损坏档；并覆盖重复结算、重复迁移、断电/写入中断、迁移失败、备份恢复和云接口预留。

每类档案必须证明：原档不被覆盖、旧字段保留、迁移失败有可理解错误、恢复后实体/事件不重复、重复结算不重复发放、备份可恢复。

## 6. 重新开工注意事项

先修复 D10CrewBehaviorState 并使 Save.Tests 构建通过，再运行 D8-A05 迁移测试；随后补齐文件级备份和断电恢复策略，再执行 D16-A02 全部矩阵。不得因为新档字段缺失而丢弃 D7 字段，不得用外部云服务替代本地恢复测试，不得把未执行的测试标记为通过。

## 2026-08-11 状态更新

D15-S02 已通过，AgentCore、Systems、Save.Tests standalone build 均为 0 warning / 0 error。D16-A02 当前为 **PENDING/运行环境待处理**：Unity batch runner 停在 `Licensing::Module`，未产生 EditMode 结果；迁移矩阵、文件级备份/原子替换、断电恢复和重复结算保护仍未验收。

## 2026-08-11 实现进度补充

当前状态：**IN_PROGRESS，完整运行时矩阵待验收**。

已新增 `SaveFileSafety` 并接入 `SaveSystem`：覆盖写入/新游戏/迁移前文件级备份，临时文件 flush + 原子替换，恢复前回滚副本，SQLite `-wal`/`-shm` sidecar 处理；保存失败尝试恢复旧档，迁移结果返回备份路径。该层不写玩法状态、不接网络/云服务。

证据：Systems 与 Save.Tests standalone build 均 0 warning / 0 error；`tools/d16_a02_runner/D16A02Runner.csproj` 的 5 项保存安全测试全部通过。

未完成：真实断电/进程中止、D7—D14 全档案矩阵、Unity EditMode/PlayMode、迁移中断和跨平台恢复演练。Unity batch runner 仍停在 `Licensing::Module`，不能把当前独立测试当作完整 D16-A02 PASS。

## 2026-08-11 Unity 6 实测更新

当前状态：**IN_PROGRESS（Unity 回归已通过，故障注入与档案矩阵仍待执行）**。

- Unity 6 EditMode 全量回归：`D2/D3 EditMode tests: passed=291, failed=0, skipped=0`，其中包含 D16 Save Safety 测试。
- D16-A02 独立存档安全测试：5/5 PASS；Save.Tests、Systems 和相关运行时程序集串行构建 0 warning / 0 error。
- 这些证据证明备份/原子替换/失败回滚实现和现有测试没有回归；不等价于真实断电、进程中止、跨平台恢复或 D7—D14 全档案矩阵通过。

证据总览：`docs/game/design/common/D16_RUNTIME_ACCEPTANCE_REPORT.md`；Unity XML：`game/D2_TestResults.xml`。下一步保持在 A02：执行可控进程中止/写入中断、备份恢复和全档案矩阵，并记录恢复后的事实去重结果。

## 2026-08-11 故障注入与档案矩阵交接更新

### 状态

**IN_PROGRESS**。本轮关闭了“没有进程中止证据”和“没有可复核档案矩阵入口”两个子缺口，但没有关闭真实断电、跨平台和长线门禁。

### 修改文件与接口

- `game/Assets/Scripts/Systems/Save/SaveFileSafety.cs`：新增默认关闭的 flush 后/原子替换后测试 hook。
- `game/Assets/Scripts/Systems/Save/SaveMigration.cs`：新增默认关闭的 `ProcessAbortBeforeCommit` 测试故障点。
- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs`：新增三个独立子进程故障入口。
- `tools/d16_a02_runner/Program.cs`：显式故障模式映射到退出码 73/74/75；无参数仍只运行 5 项安全测试。
- `tools/d16_a02_fault_injection.py`：父进程验证文件和 SQLite 恢复事实，输出 `tools/results/d16_a02_fault_injection_result.json`。
- `tools/d16_a02_archive_matrix.py`：验证 Unity XML 的 10 类档案覆盖，输出 `tools/results/d16_a02_archive_matrix_result.json`。

### 验收证据

- Save.Tests 与 fault runner 构建均 0 warning / 0 error。
- 故障注入 3/3 PASS；迁移中止后 SQLite 完整性为 `ok`，D7 表和玩家金钱保留，备份存在。
- Unity EditMode `291/291 PASS`，档案矩阵 `10/10` 类别匹配真实 Passed 用例。
- 独立 D16-A02 安全 runner `5/5 PASS`。

### 数据所有权与风险

故障 hook 只存在于 Save/Storage 的测试边界，不改变 SaveSystem、迁移器或玩法系统的数据所有权；没有网络、云服务或 LLM。可控进程退出不能宣称物理断电等价，后续仍需真实设备强制终止/断电和跨平台文件恢复证据。

## 2026-08-11 Windows 强制终止复验更新

### 状态

**IN_PROGRESS**。本轮补齐了 Windows 子进程在三个存档临界点被父进程真正强制终止的证据，但没有把它扩大解释为物理断电、跨平台或长线通过。

### 修改文件与接口

- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs`：新增三个等待 ready 标记的强制终止入口；父进程终止时不删除标记，避免子进程继续执行。
- `tools/d16_a02_runner/Program.cs`：新增 `--force-kill-after-flush`、`--force-kill-after-replace`、`--force-kill-during-migration` 三个显式测试模式；若父进程未终止，runner 返回 76，不能伪造通过。
- `tools/d16_a02_fault_injection.py`：使用 `subprocess.Popen` 等待 ready 标记后调用 `Process.kill()`，分别检查旧文件、新文件、临时文件、SQLite 回滚和备份。
- `game/Assets/Editor/D9A07UiAudioLogTests.cs`：修复 Unity 6 EditMode 临时 `AddComponent` 不自动派发 `Awake` 的测试初始化问题；通过反射显式调用私有 `Awake`，不改变运行时逻辑。

### 原始验证结果

- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `python tools/d16_a02_fault_injection.py`：受控退出 3/3，真实强制终止 3/3，最终 `passed=true`；
- `dotnet tools/d16_a02_runner/bin/Debug/net9.0/D16A02Runner.dll`：5/5 PASS；
- `python tools/d16_a02_archive_matrix.py tools/results/d16_a02_long_run_editmode_unity6.xml`：10/10 类别，296 用例，0 failed/inconclusive；
- Unity 6 EditMode：`tools/results/d16_a02_long_run_editmode_unity6.xml` 为 296/296 PASS；单独 D9 UI 测试 `d9_ui_audio_single_editmode_retry2.xml` 为 1/1 PASS。

### 数据所有权与风险

新增接口仍只在 Save/Storage 测试边界生效，默认 hook 为 `null`；D9 测试改动只修复 Editor 测试初始化，不写入玩法状态。真实强制终止证据仍局限于当前 Windows 配置；物理断电、跨平台文件系统语义、长线存档压力、重复结算和 D16-S01 完整链路继续保持未完成。

## 2026-08-11 长线存档压力与新游戏迁移修复

### 修改与原因

- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`：新游戏覆盖旧档时由清空 v3 表行改为删除 `save_metadata` 和 `v3_entities` 表；否则空的 `save_metadata` 会让下一次新档迁移被判定为 `metadata_incomplete`。
- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs`：新增 `RunLongRunArchiveStress`，通过真实 SaveSystem 入口反复执行新游戏、保存、读取、迁移、重复迁移和恢复。
- `tools/d16_a02_runner/Program.cs`：新增显式 `--long-run <path> <iterations>` 模式。
- `tools/d16_a02_long_run.py`：执行临时 SQLite 压力循环并检查最终事实、v3 投影、备份数和完整性；报告为 `tools/results/d16_a02_long_run_result.json`。

### 原始验证结果

- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `python tools/d16_a02_long_run.py 100`：100/100 循环通过，退出码 0，SQLite integrity `ok`，最终玩家“长线存档船长-99”/599 金钱，备份数 399；
- 修复后重新执行 `python tools/d16_a02_fault_injection.py`：受控退出 3/3、真实强制终止 3/3、最终 `passed=true`；
- `dotnet tools/d16_a02_runner/bin/Debug/net9.0/D16A02Runner.dll`：5/5 PASS。

### 仍未完成

本轮只关闭当前 Windows/本地文件系统上的重复迁移和有限长线压力子门禁；物理断电、跨平台恢复、重复结算的完整长线压力、完整新游戏玩法链和 D16-S01 仍未完成。不得将 A02 标为 PASS。

修复后的 Unity 运行时复验：`tools/results/d16_a02_long_run_playmode_unity6.xml`，D16.PlayMode.Tests **5/5 PASS**；`dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal` 为 0 warning / 0 error。

## 2026-08-12 路径与 SQLite sidecar 保护补充

### 修改

- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs` 新增 `UnicodeAndSidecarBackupRestorePreservePortablePathSemantics`，覆盖中文/空格路径、主数据库、`-wal`、`-shm` 备份和恢复前 rollback。
- `tools/d16_a02_runner/Program.cs` 将该用例纳入默认独立 Save/Storage runner；生产 SaveSystem 和数据所有权未改变。

### 验证

- Save.Tests 构建：0 warning / 0 error；
- D16A02Runner 构建：0 warning / 0 error；
- 独立 runner：**6/6 PASS**；
- Unity 6 EditMode `Save.Tests.D16SaveSafetyTests`：**6/6 PASS**，结果 `tools/results/d16_a02_save_safety_editmode_unity6.xml`；
- Unity 6 EditMode `Save.Tests` 全量：**293/293 PASS**，结果 `tools/results/d16_a02_save_tests_editmode_unity6.xml`；
- 故障注入：受控 3/3、真实 Windows 强制终止 3/3、`passed=true`；
- 100 次新游戏→保存→读取→迁移→重复迁移→恢复：`passed=true`，SQLite `integrity_check=ok`。

### 边界

路径测试只是当前 Windows/.NET 环境的可移植路径语义证据，不等于 Linux/macOS 实机恢复。物理断电、跨平台文件系统、D16-S01 完整长线和最终 A02 PASS 仍未完成。

## 2026-08-12 当前工作区复验补充

本轮按当前文件和当前 Unity 结果重新执行：

- Unity 6 `Save.Tests` 全量 **293/293 PASS**：`tools/results/d16_a02_save_tests_current_editmode_unity6.xml`；
- `d16_a02_fault_injection.py`：受控边界 3/3、Windows `Process.kill()` 边界 3/3、`passed=true`；
- `d16_a02_archive_matrix.py`：292 个当前 EditMode 用例，10/10 档案类别匹配，0 failed/inconclusive；
- `d16_a02_long_run.py 100`：100 次新游戏→保存→读取→迁移→重复迁移→恢复通过，SQLite `integrity_check=ok`，最终玩家/金钱和 399 个备份符合预期；
- Save.Tests 正常恢复依赖后构建 **0 warning / 0 error**。

本轮没有修改 Save/Storage 逻辑；证据只关闭 Windows/.NET 本地边界和有限长线子门禁。物理断电、跨平台恢复、重复结算完整压力、目标设备和 D16-S01 仍未完成，A02 保持 **IN_PROGRESS**。

## 2026-08-12 结算幂等跨存档回归

### 修改

- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs` 新增 `SettlementClaimSurvivesSaveRoundTripAndCannotRepeat`。
- 仅扩展 Save 测试边界；未修改 EventLog、SaveSystem 的生产裁决逻辑，也未改变任何领域数值所有权。

### 验证

- 测试通过真实 `SaveSystem.SaveD3Runtime` 写盘、`GameState.Reset`、`SaveSystem.Load` 和 `RestoreD3TradeEventLog`，验证结算门与 DedupeKey 跨存档保留；
- Unity 6 focused EditMode：**1/1 PASS**，`tools/results/d16_a02_settlement_roundtrip_editmode_unity6.xml`；
- Unity 6 `Save.Tests` 全量：**298/298 PASS**，`tools/results/d16_a02_save_tests_after_settlement_editmode_unity6.xml`；
- Save.Tests 构建：0 warning / 0 error。

### 验收边界

该子门禁证明 D3 EventLog 的结算声明在本机 Save 往返后不会重复通过或重复追加；不宣称所有 D7—D14 结算组合、真实断电、Linux/macOS、目标设备或 D16-S01 已通过。A02 继续 **IN_PROGRESS**，D16-S01 不启动。

## 2026-08-12 多条结算事实多轮存档幂等压力回归

### 修改文件与边界

- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs`：新增 24 条结算事实、5 轮 Save/Load 往返及重复声明/重复追加断言；
- `tools/d16_a02_runner/Program.cs`：将该回归纳入独立 D16-A02 安全 runner；
- 只修改 Save 测试与测试 runner，不修改 EventLog、SaveSystem 生产裁决、领域数值、contracts 或网络代码。

### 验证结果

- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet tools/d16_a02_runner/bin/Debug/net9.0/D16A02Runner.dll`：**7/7 PASS**，新增测试覆盖 24 条事实 × 5 轮存档往返。

### 验收结论与未完成事项

当前 Windows/.NET 下多条结算事实的跨轮次幂等保护已取得本机证据；A02 总体仍 **IN_PROGRESS**。物理断电、跨平台恢复、目标设备、全 D7—D14 组合和 D16-S01 长线链路仍未完成，D16-S01/S02 不启动。

## 2026-08-12 A02 本机安全 runner 复验

- `D16A02Runner` 重新构建并执行，7/7 PASS：备份恢复、原子写入、失败不覆盖、迁移幂等、重复结算往返和 Unicode/sidecar 路径均通过。
- `AgentCore` 与 `Save.Tests` 当前构建均 0 warning / 0 error。

这只确认当前 Windows/.NET 本机子门禁仍稳定；物理断电、跨平台恢复、目标设备和完整 D7—D14 业务组合仍缺真实证据，A02 保持 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 SQLite 损坏档真实恢复回归

### 修改

- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs` 新增 `CorruptedLiveDatabaseRestoresLatestValidArchiveAndLoads`：先创建真实 SQLite 新档并触发覆盖备份，再破坏主数据库字节，确认损坏主档不能读入；随后从最新有效备份恢复，验证恢复前的损坏文件保留为 rollback，并重新成功加载船长事实。
- `tools/d16_a02_runner/Program.cs` 将该用例和既有 `SettlementClaimSurvivesSaveRoundTripAndCannotRepeat` 纳入默认独立 runner；之前 runner 漏掉的现有测试不再被遗漏。
- 未修改 SaveSystem/SaveFileSafety 生产实现、领域数值、契约或网络逻辑。

### 验证

- `dotnet build game/Save.Tests.csproj --no-restore`：0 warning / 0 error；
- `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore`：0 warning / 0 error；
- `dotnet tools/d16_a02_runner/bin/Debug/net9.0/D16A02Runner.dll`：**9/9 PASS**；
- Unity 6 EditMode `Save.Tests` 全量：**302/302 PASS**，`tools/results/d16_a02_save_tests_after_sqlite_recovery_editmode_unity6.xml`；
- `python tools/d16_a02_archive_matrix.py tools/results/d16_a02_save_tests_after_sqlite_recovery_editmode_unity6.xml`：10/10 档案类别真实匹配，0 failed/inconclusive，`passed=true`；
- `tools/results/d16_a02_archive_matrix_result.json` 已更新为本轮 302 个真实用例。

### 验收边界

本轮关闭了“真实 SQLite 损坏主档恢复”和 runner 测试遗漏的本机子门禁；仍不能替代物理断电、Linux/macOS/跨平台恢复、目标设备以及完整 D7—D14 生产组合。D16-A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前故障边界与长线复验

### 验证结果

- `python tools/d16_a02_fault_injection.py`：受控写入中止 **3/3 PASS**，Windows `Process.kill()` **3/3 PASS**，总结果 `passed=true`。
- `python tools/d16_a02_long_run.py 100`：**PASS**；100 次真实存档压力后 SQLite 完整性为 `ok`，最终船长事实为 `长线存档船长-99 / 599`，备份数为 399。
- A04 人工队列已按当前 Registry 重新生成：`tools/results/d16_a04_review_queue.json`、`.md`；267 条 Pending、12 批，工具明确不自动批准。

### 结论与边界

本轮只补强当前 Windows/.NET 的写入中断和有限长线压力证据，并刷新 A04 人工复核入口；不宣称物理断电、Linux/macOS/跨平台恢复、目标设备或完整 D7—D14 生产组合通过。D16-A02、D16-A04 仍 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前 Save.Tests 权威复验

- Unity 6 EditMode `Save.Tests` 当前权威结果：`tools/results/d16_current_save_tests_editmode.xml`，**302/302 PASS**，failed/inconclusive/skipped 均为 0。
- `tools/results/d16_a02_archive_matrix_result.json`（来源为 `tools/results/d16_current_save_tests_editmode.xml`）：10/10 档案类别匹配，`passed=true`。
- 本次只复验现有保存/迁移测试和档案矩阵，没有改动生产存档逻辑、Schema 或领域数据；物理断电、Linux/macOS/跨平台、目标设备和完整 S01 仍缺证据，D16-A02 保持 **IN_PROGRESS**。

## 2026-08-12 损坏主档自动恢复增量

### 状态

**IN_PROGRESS**。本轮修复了读档入口不会自动使用有效备份的真实缺口，并完成当前 Windows/.NET 与 Unity 6 回归；物理断电、跨平台和完整 D16-A02 仍未通过。

### 修改文件与接口边界

- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`：`HasSave()` 增加 SQLite 完整性、玩家/船关联校验；主档不可读或缺失时倒序尝试有效备份，恢复前保留 rollback；日志走 stderr，兼容 Unity 编辑器和独立 runner。
- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs`：真实 SQLite 损坏用例改为验证 `SaveSystem.Load()` 自动恢复并保留 rollback。
- `docs/game/design/common/D16_SAVE_MIGRATION_QA.md`、本交接报告、`.workbuddy/memory/2026-08-12.md`：记录实现、证据和边界。

未修改存档 Schema、SaveMigration 版本、玩法数值、领域裁决、contracts、网络、云服务或 AgentCore；Save/Storage 仍是唯一存档写者。

### 原始验证

- Systems、Save.Tests、Assembly-CSharp 和独立 runner 构建均 0 warning / 0 error；
- 独立 `D16A02Runner`：**9/9 PASS**；
- Unity 6 focused `D16SaveSafetyTests`：**9/9 PASS**，`tools/results/d16_a02_save_recovery_editmode_current.xml`；
- Unity 6 `Save.Tests` 全量：**302/302 PASS**，`tools/results/d16_a02_save_tests_after_auto_recovery_editmode_unity6.xml`；
- 当前 XML 档案矩阵：**10/10 类别**、0 failed/inconclusive；
- 故障注入：受控 **3/3**、Windows `Process.kill()` **3/3**；
- 100 次 Save/Load/迁移/恢复压力：SQLite integrity `ok`，最终事实与 399 个备份符合预期。

### 下游与未完成事项

该增量只关闭本机“损坏主档无自动恢复入口”子缺口。仍需真实物理断电、Linux/macOS/跨平台恢复、目标设备和完整 D7—D14 生产档案组合；A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 跨平台测试器初始化修复

### 修改

- `tools/d16_a02_runner/Program.cs`：将 `kernel32.SetDllDirectory` 包装在 `OperatingSystem.IsWindows()` 分支中；Windows 继续显式加载项目内 sqlite 目录，Linux/macOS 不再调用 Windows 专用 API，交由平台原生加载器解析 sqlite。
- `tools/d16_a02_runner/D16A02Runner.csproj`：将仓库内依赖改为相对路径，并支持 `-p:UnityManagedDir=<平台对应的 UnityEngine 托管目录>`；Windows 保留自动发现本机 Unity 6 路径，缺失时明确报错。

### 原始验证

- `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore --nologo`：0 warning / 0 error；
- `dotnet tools/d16_a02_runner/bin/Debug/net9.0/D16A02Runner.dll`：**9/9 PASS**；
- `python tools/d16_a02_fault_injection.py`：受控中止 3/3、Windows 强制终止 3/3、SQLite 迁移完整性与备份检查全部通过，总结果 `passed=true`。

### 边界

这次修复消除了测试器的 Windows API 硬编码，但当前工作区仍只有 Windows 原生 sqlite 与 Windows 实测证据；尚未取得 Linux/macOS 实机、物理断电或跨平台恢复结果。因此 A02 继续 **IN_PROGRESS**，不能据此标记跨平台 PASS，D16-S01/S02 不启动。

## 2026-08-12 当前工作区存档全量复验

- Unity 6 `Save.Tests` EditMode 当前 XML `tools/results/d16_current_save_tests_editmode_unity6.xml`：**302/302 PASS**，0 failed、0 inconclusive、0 skipped。
- 受控恢复临时 `project.assets.json` 后，AgentCore 与 Save.Tests 按 `--no-restore` 构建均 0 warning / 0 error；没有缩小测试范围或复制 D10 类型。
- 本机存档子门禁没有新增回归；物理断电、Linux/macOS/跨平台恢复和完整 D7—D14 生产档案组合仍缺，A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

随后按当前源码重新生成 standalone 依赖并复跑 `D16A02Runner`：构建 0 warning / 0 error、runner 存档安全 **9/9 PASS**；档案矩阵 **10/10**，故障注入受控中止 **3/3** 与 Windows 强制终止 **3/3**，100 次长线循环通过。Unity 刷新后临时输出会被清理，复验使用先恢复工程输出、再构建 runner 的顺序，未使用旧 DLL 作为当前源码证据。

## 2026-08-12 严格覆盖矩阵复验

新增 `tools/d16_a02_archive_matrix_manifest.json`，把 10 个要求类别固定到 31 个精确 Unity 测试名；`tools/d16_a02_archive_matrix.py` 改为 exact-name matching，不再使用宽泛类名或片段匹配。当前 `tools/results/d16_a02_archive_matrix_result.json` 显示 302 个测试用例中 31/31 精确证据匹配，10/10 类别通过，失败/不确定为 0；新增矩阵工具单测 3/3 PASS。

该项只收紧 QA 证据，不改变生产存档代码，也不扩大 A02 结论。物理断电、Linux/macOS/跨平台恢复和完整目标环境仍缺真实证据，D16-A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D2 修复后的当前 A02 独立复验

- 当前 XML `tools/results/d16_a02_current_after_d2_save_tests_editmode_unity6.xml`：Unity 6 `Save.Tests` **302/302 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 当前依赖顺序构建 `Save.Tests` 与 `D16A02Runner` 均 0 warning / 0 error；runner **9/9 PASS**；严格矩阵 **10/10 类别、31/31 精确用例匹配**。
- 故障注入：受控 **3/3 PASS**、Windows `Process.kill()` **3/3 PASS**；100 次长线 Save/Load/迁移/恢复通过，SQLite integrity `ok`，最终事实和 399 个备份符合预期。

数据所有权仍由 Save/Storage 持有；本轮没有修改存档 Schema、领域数值或跨端契约。A02 总体继续 **IN_PROGRESS**，因为物理断电、Linux/macOS/跨平台文件系统、目标设备和全生产组合仍缺真实证据。

## 2026-08-12 当前故障注入与长线复验

- `python tools/d16_a02_fault_injection.py` 当前结果 `tools/results/d16_a02_fault_injection_result.json`：受控中止 **3/3**、Windows 强制终止 **3/3**，SQLite integrity、旧档保留、临时文件和迁移备份检查均通过，`passed=true`。
- `python tools/d16_a02_long_run.py 100` 当前结果：100 次 Save/Load/迁移/恢复通过，SQLite integrity `ok`，`user_version=1`，v3 tables present，最终船长 `长线存档船长-99`、money `599`、backup_count `399`。
- 这些是当前 Windows/.NET 的可重复中断和长线证据，不是物理断电、Linux/macOS 实机或完整 D7—D14 生产组合证据；A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前 Unity Save.Tests v2 复验

### 状态

**IN_PROGRESS**。当前 Unity 存档回归和工程构建已刷新通过；外部环境门禁仍未满足。

### 当前证据

- Unity 6 EditMode `Save.Tests`：`tools/results/d16_current_save_tests_editmode_unity6_v2.xml`，**302/302 PASS**，failed/inconclusive/skipped 均为 0；
- 依赖恢复后按顺序构建 AgentCore、Save.Tests、Assembly-CSharp：均 **0 warning / 0 error**；
- 未修改 Save 生产逻辑、Schema、迁移版本、领域数值、contracts 或网络边界。

### 未完成与下游

物理断电、Linux/macOS/跨平台文件系统、目标设备和完整 D7—D14 生产组合仍无真实证据；D16-A02 继续 **IN_PROGRESS**，D16-S01/S02 继续不启动。

## 2026-08-12 当前工作树 Save.Tests 独立复验

- 当前 Unity 6 EditMode：`tools/results/d16_current_save_tests_continuation_unity6.xml`，**302/302 PASS**，failed/inconclusive/skipped 均为 0，耗时 11.885s；
- Unity 刷新后先恢复工程依赖，再按顺序构建 AgentCore、Save.Tests、Assembly-CSharp，均 **0 warning / 0 error**；
- 本轮未修改 Save 生产逻辑、Schema、迁移版本、领域数值、contracts 或网络边界；A02 仍因物理断电、Linux/macOS/跨平台文件系统、目标设备和完整生产组合缺真实证据而保持 **IN_PROGRESS**。

环境边界：当前 Windows 主机没有可用的 WSL/Linux 发行版；本轮不把 Windows/.NET 结果冒充 Linux/macOS/跨平台实测，也没有执行虚假的跨平台验收。

## 2026-08-12 sidecar 恢复失败成组回滚

### 状态

本机代码级增量 **PASS**；D16-A02 总体仍 **IN_PROGRESS**。

### 修改文件与接口

- `game/Assets/Scripts/Systems/Save/SaveFileSafety.cs`：`RestoreBackup(...)` 增加主文件/WAL/SHM 成组失败回滚；新增内部测试故障注入入口 `TestBeforeRestoreSidecarHook`。
- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs`：新增 `RestoreFailureRollsBackMainFileAndSqliteSidecarsTogether`，验证 sidecar 阶段失败后主文件和两个 sidecar 都恢复到操作前内容。
- `tools/d16_a02_runner/Program.cs`：默认安全清单加入该测试。

数据所有权仍归 Save/Storage；没有修改 contracts、Schema、迁移版本、领域数值、网络或 AgentCore。

### 原始验证

- focused Unity 6 EditMode：`tools/results/d16_a02_sidecar_rollback_current_20260812.xml`，**1/1 PASS**；
- Unity 6 `Save.Tests` 全量：`tools/results/d16_a02_save_tests_after_sidecar_rollback_20260812.xml`，**304/304 PASS**；
- 严格档案矩阵：`tools/results/d16_a02_archive_matrix_sidecar_rollback_20260812.json`，10/10 类别、31/31 精确用例；
- Save.Tests、D16A02Runner 真实构建：0 warning / 0 error；独立 runner **11/11 PASS**。

### 验收结论与未完成事项

本轮证明 sidecar 恢复失败不会把存档留在主文件/sidecar 不一致的中间状态；仍缺物理断电、Linux/macOS/真实跨平台文件系统、目标设备和完整生产组合证据。下游 D16-S01/S02 继续等待 A01—A06 全部 PASS。

### 跨模块回归

- `tools/results/d16_full_after_sidecar_rollback_20260812.xml`：D16 PlayMode 全量 **53/53 PASS**，0 failed / 0 inconclusive / 0 skipped，约 73.47s。
- 本次回归未发现 UI、输入、镜头、上下船、性能控制或运行时存档链回归；A02 的外部证据缺口仍保持原状。

### 故障注入与长线结果

- `tools/results/d16_a02_fault_injection_result.json`：受控中止 3/3、Windows 强杀 3/3，`passed=true`；
- `tools/results/d16_a02_long_run_result.json`：100 次 Save/Load/迁移/恢复通过，SQLite `integrity_check=ok`，最终事实为船长-99/599，备份 399 个。
- 证据边界仍是 Windows/.NET；物理断电、Linux/macOS/跨平台文件系统和目标设备未实测。

## 2026-08-12 原子写入孤儿临时文件清理增量

### 修改文件与接口

- `game/Assets/Scripts/Systems/Save/SaveFileSafety.cs`：新增 `CleanupTemporaryArtifacts(string sourcePath)`；严格按当前存档主文件及 SQLite `-wal`/`-shm` sidecar 的 `.tmp_` + 32 位十六进制 GUID 规则清理孤儿临时文件。
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`：`HasSave()` 在读取/恢复前调用清理入口。
- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs`：新增正式存档字节保持、生成命名临时文件删除、未知命名临时文件保留的回归用例。
- `tools/d16_a02_runner/Program.cs`：将新增测试加入独立 runner 的显式测试列表。

数据所有权仍由 Save/Storage 持有；本增量不修改 contracts、Schema、迁移版本、领域数值、网络或 AgentCore。

### 当前证据

- Unity 6 focused `D16SaveSafetyTests`：**10/10 PASS**，`tools/results/d16_a02_save_safety_current_unity6.xml`；
- Unity 6 `Save.Tests` 全量：**303/303 PASS**，`tools/results/d16_a02_save_tests_full_current_unity6.xml`；
- 严格档案矩阵：**10/10 类别、31/31 精确测试匹配**，`tools/results/d16_a02_archive_matrix_result.json`；
- 独立 `D16A02Runner`：**10/10 PASS**；
- Save.Tests、Assembly-CSharp、D16.PlayMode.Tests 和 runner 构建均 0 warning / 0 error。

### 边界与后续

该增量只关闭当前 Windows/.NET 的启动清理代码缺口，不替代物理断电、Linux/macOS/跨平台文件系统、目标设备和完整生产组合证据；D16-A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前证据链复验

### 原始结果

- `python tools/d16_a02_fault_injection.py`：受控写入中止 **3/3 PASS**、Windows `Process.kill()` **3/3 PASS**，总结果 `passed=true`；
- `python tools/d16_a02_long_run.py 100`：**PASS**，100 次 Save/Load/迁移/恢复循环，SQLite `integrity_check=ok`，最终事实正确，备份数 399；
- `tools/results/d16_a02_save_tests_full_current_unity6.xml`：Unity 6 `Save.Tests` **303/303 PASS**；
- `python tools/d16_a02_archive_matrix.py tools/results/d16_a02_save_tests_full_current_unity6.xml`：**10/10 类别、31/31 精确用例匹配**，0 failed/inconclusive。

### 结论边界

本轮只刷新当前 Windows/.NET 本机证据，没有修改存档 Schema、领域数据或跨端契约。物理断电、Linux/macOS/跨平台文件系统、目标设备和完整 D7—D14 生产组合仍缺真实证据；D16-A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。
## 2026-08-12 总纲顺序下的 A02 当前复验

### 状态

**本机存档安全子门禁 PASS；D16-A02 总体 IN_PROGRESS。**

### 当前证据

- `dotnet build tools/d16_a02_runner/D16A02Runner.csproj --no-restore -v:minimal`：0 warning / 0 error；独立 runner **11/11 PASS**；
- `tools/results/d16_a02_fault_injection_result.json`：受控中止 3/3、Windows 强制终止 3/3，`passed=true`；
- `tools/results/d16_a02_long_run_result.json`：100 次 Save/Load/迁移/恢复通过，SQLite `integrity_check=ok`，最终船长 `长线存档船长-99` / money `599` / backups `399`；
- `tools/results/d16_a02_archive_matrix_result.json`：304 个真实用例，10/10 类别、31/31 精确测试匹配，0 failed/inconclusive；
- `Save.Tests` 构建：0 warning / 0 error。

### 边界

上述结果覆盖 Windows/.NET 和受控进程中断，不等价于物理断电、真实跨平台文件系统或目标设备恢复。A02 继续 IN_PROGRESS；D16-S01/S02 不启动。

## 2026-08-12 工具入口纠正与 A02 当前复验

- `python tools/d16_a02_archive_matrix.py tools/results/d16_a02_save_tests_after_sidecar_rollback_20260812.xml`：**304** 个真实用例，10/10 类别、31/31 精确匹配，0 failed/inconclusive，`passed=true`。
- `python tools/d16_a02_long_run.py 100`：**PASS**；100 次真实独立 runner Save/Load/迁移/恢复，SQLite `integrity_check=ok`，`user_version=1`，v3 表存在，最终船长 `长线存档船长-99` / money `599`，备份 399。
- 本轮修正了两个工具入口误用：档案矩阵需要 XML 文件路径，长线脚本的第一个参数是迭代次数；没有把 `game` 目录参数错误当成产品失败。

结论：A02 当前 Windows/.NET 代码与受控中断子门禁继续 PASS；总体仍 **IN_PROGRESS**，物理断电、Linux/macOS/真实跨平台文件系统、目标设备和完整生产组合尚无真实证据。

## 2026-08-12 当前最终复核

- Save.Tests 当前覆盖结果：**304/304 PASS**；档案矩阵 **10/10 类别、31/31 精确匹配**；独立 A02 runner **11/11 PASS**；
- 受控写入中止与 Windows 强制终止均已留证，100 次长线 SQLite 压力通过；
- A02 总体仍 **IN_PROGRESS**：本机强制终止不等价于物理断电，当前没有第二平台/跨平台文件系统和目标设备证据。

## 2026-08-13 Save 多表快照事务补强

### 修改文件与边界

- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`：`NewGame` 覆盖旧档和 `Save` 的 Player/Ship/Crew 写入统一走一个 SQLite 事务；写入函数复用同一 `SqliteDb` 连接。事务异常显式回滚，外层备份恢复策略不变。
- `game/Assets/Tests/EditMode/D16SaveSafetyTests.cs`：新增事务异常回滚断言和独立子进程强杀入口；验证玩家表已经写入但提交前中断时，旧玩家、船和船员仍完整。
- `tools/d16_a02_runner/Program.cs`、`tools/d16_a02_fault_injection.py`：新增 `--force-kill-during-save-after-player` 模式及 SQLite 完整性/实体数量检查。

### 当前证据

- Unity 6 `Save.Tests.D16SaveSafetyTests`：`tools/results/d16_a02_save_transaction_editmode_20260813.xml`，**12/12 PASS**；
- 独立 runner：**12/12 PASS**；
- 故障注入：受控中止 3/3、Windows 强制终止 **4/4**，总结果 `passed=true`；新增保存中断后 `integrity=ok`、玩家 money=500、ship_count=1、crew_count=1；
- `Assembly-CSharp`、`Save.Tests`、`D16A02Runner` 构建：0 warning / 0 error。

### 验收结论

本轮只关闭 Windows/.NET 代码级的多表保存事务缺口；不把本机强制终止扩大为物理断电或跨平台通过。A02 仍 **IN_PROGRESS**，D16-S01/S02 继续等待 A01—A06 的真实外部证据。
