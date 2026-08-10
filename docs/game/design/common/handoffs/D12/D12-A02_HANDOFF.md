# D12-A02 交接报告：沉船、洞穴与打捞

任务：`D12-A02`  
状态：已完成，D12-A03/A08 可读取打捞事实与 RewardRequest  
日期：2026-08-11

## 1. 已交付

- `D12WreckCaveSalvageContracts`：沉船/洞穴定义、潮汐、装备、绳索、照明、风险、竞争者、法律裁决、失败损失、恢复路线、奖励请求和快照 DTO。
- `D12WreckCaveSalvageCatalog`：`wreck_saint_marys_chart_cove` 与 `cave_green_cay_tide_cave` 两个纵切片地点。
- `D12WreckCaveSalvageSystem`：D12 沉船/洞穴/打捞唯一写者；成功只产生 RewardRequest，失败必带恢复路线，风险型失败保存损失记录。
- D12-A01 海图目录新增 `ExplorationSites` 层和沉船/洞穴入口；打捞 UI 读模型只显示已经测绘的地点。
- SaveSystem：`D12RuntimeSnapshot.Salvage` 写入既有 `location_json.d12.salvage`，读档恢复地点状态和奖励请求幂等性。
- 测试与运行器：`D12WreckCaveSalvageTests` 四项，`tools/d12_a02_runner` 四项实际通过。

## 2. 关键接口

### 打捞入口

`D12WreckCaveSalvageSystem.TrySalvage(D12SalvageRequest)`：

- 必须先在 `D12MapDiscoverySystem.GetMapReadModel()` 中出现对应地图特征；
- 读取潮汐窗口、装备列表、绳长、照明余量和竞争者观察；
- 调用 `ID12SalvageLegalAuthority.Rule`，不在 D12 内推断产权；
- 写入 `event_d12_salvage_attempt_*`，成功只返回 `D12SalvageRewardRequest`。

### 失败恢复

`Recover(D12SalvageRecoveryRequest)` 只允许从 `Failed` 状态进入 `Recovered`，追加恢复事实并引用失败事件。洞穴默认回头路线为 `route_cave_return_to_entrance`。

### UI/Agent 边界

- UI 只读 `GetReadModel()`，不得生成装备、宝物或奖励；
- Agent 没有产权裁决入口，也不能写 `D12SalvageSnapshot`；
- Inventory、Ship、Crew、Faction/Legal、Reward 仍由各自 canonical owner 写入。

## 3. 验证结果

- `dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal`：PASS，0 warning / 0 error。
- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：PASS，0 warning / 0 error。
- `dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal`：PASS，0 warning / 0 error。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：PASS，0 warning / 0 error。
- `dotnet run --project tools/d12_a02_runner/D12A02Runner.csproj --no-build --no-restore`：PASS，`D12-A02 tests PASS count=4`。
- `python tools/d9_s02_acceptance_check.py --json`：PASS，`blocked_count=0`，`frozen_event_count=14`。
- A02 新增 Systems 文件红线扫描：PASS，无网络/LLM 代码。

## 4. 工作区并行变更说明

- D9 相关 10 项补充 Python 测试中，D9 门禁和其余测试通过；内容注册正式检查有 6 条并行 D12 内容条目的 `owner-prefix-mismatch`，不属于 A02 沉船/洞穴代码，未在本任务越权修改。
- D12 全目录的文本红线扫描会命中并行 D12-A06 既有 Tarot 的 `llm_text_only` 解释提供者字符串/注释；A02 新增文件未出现该引用，且 A02 运行器与工程编译通过。
- Unity Editor Test Runner 本轮未启动；当前检测到 Unity 后台导入进程，因此以独立 .NET 运行器实际执行 A02 四项测试，不伪造编辑器结果。

## 5. 后续接入

1. D12-A03 线索/任务只读取 `D12SalvageSiteView`、事实事件和 `RewardRequest`，不得复制打捞状态。
2. Faction/Legal 适配器实现 `ID12SalvageLegalAuthority`，由法律系统提供许可证、claim、争议和查封裁决。
3. Reward/Inventory/Ship/Crew 系统分别消费成功/失败事实和请求，D12 不直接生成物品、奖励、船损或伤病。
