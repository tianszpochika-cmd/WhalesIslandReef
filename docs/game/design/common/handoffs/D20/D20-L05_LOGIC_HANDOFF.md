# D20-L05 逻辑交接说明

- 任务：`D20-L05`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L05/D20_L05_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20ActionPriorityRules.cs`：

- 只消费 L04 的 `D20ActionIntentKind`、`D20ActionIntent` 和定义目录，不重新创建动作字符串；
- 冻结八个动作的确定性优先级：`walk=10`、`run=20`、`interact=30`、`talk=40`、`fish=50`、`helm=60`、`board=70`、`hit=100`；
- 定义 locomotion、social、fishing、helm、boarding、impact 六类互斥集；受击动作可打断低优先级互斥动作，受击本身不可被低优先级动作打断；
- 定义角色可用、港口/船上/航行 presence、目标存在和 owner 权限前置；presence 位在同一规则中按“任一允许状态”解释；
- `Evaluate(...)` 只返回允许、缺失前置、互斥和是否可打断的确定性结果，不执行移动、交互、钓鱼、登船、掌舵、受击或任何业务结算；
- 规则目录、优先级、互斥集和前置集合均通过防御性副本公开，`ValidateCatalog()` 校验 L04 八动作覆盖、动作唯一、优先级唯一递增和互斥集完整。

## 2. 后续可消费公开 API

- `D20ActionPriorityRules.Rules`、`TryGetRule(...)`、`GetPriority(...)`、`GetExclusiveGroup(...)`、`GetPrerequisites(...)`；
- `AreMutuallyExclusive(...)`、`CanInterrupt(...)`、`Evaluate(...)`、`ValidateCatalog()`；
- `D20ActionPriorityRule`、`D20ActionPriorityContext`、`D20ActionPriorityDecision`；
- `D20ActionPriority`、`D20ActionExclusiveGroup`、`D20ActionPrerequisite`。

## 3. 验证与未完成项

- L02-L05 用 .NET Framework `csc.exe` 以 `/warnaserror+` 独立 library 编译：通过，exit 0；
- 纯 C# smoke：14/14 通过，覆盖规则表、前置门禁、同组优先级打断、受击打断、owner 权限拒绝、未知 presence 拒绝和规则副本隔离；
- 源码静态边界检查：无禁止 engine/network/LLM/time/random 引用，无尾随空白；
- 未启动 Player、PlayMode、场景、输入或设备；未修改 D10/D11/D19，也未实现真实 owner 路由和动作结算。

后续 Runtime 或集成任务只能把该规则结果交给对应 owner；本任务不拥有位置、动画、输入、网络或业务事实。

## 4. Coordinator memory entry

请协调器在本批次汇合后统一写入当天日志：`D20-L05 LOGIC_PASS`；新增八动作确定性优先级/互斥/可打断规则及角色可用、presence、目标、owner 权限前置；后续实现应消费 `D20ActionPriorityRules.Evaluate(...)`，不得在表现层复制优先级或动作字符串；L02-L05 csc、14 项 smoke 和静态边界检查通过；未启动 Runtime。
