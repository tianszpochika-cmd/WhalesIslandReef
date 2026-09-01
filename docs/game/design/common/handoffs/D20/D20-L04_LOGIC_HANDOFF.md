# D20-L04 逻辑交接说明

- 任务：`D20-L04`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L04/D20_L04_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20ActionIntentContracts.cs`：

- 冻结八个动作意图：`walk`、`run`、`interact`、`talk`、`fish`、`board`、`helm`、`hit`；
- `D20ActionIntentDefinition` 给出动作键、业务 owner、目标要求和逻辑位置元数据；
- `D20ActionIntent` 包含稳定 `CommandId`、`ActorId`、适用的 `TargetId`/`LocationRef`、owner、世界日、存档修订、cause event 和稳定参数；
- `D20ActionIntentContracts` 提供只读定义目录、按枚举/动作键查找、稳定键映射和不自动修正输入的校验入口；
- `D20ActionIntentValidationResult` 与稳定校验码覆盖版本、命令/行为者、未知动作、键漂移、目标、owner、修订、因果引用和重复参数；
- `D20ActionIntentStatus` 明确 accepted/succeeded/rejected/duplicate/cancelled/unavailable 结果语义。

意图只提出动作，不写移动、关系、库存、鱼获、港口、航向、伤害或任何业务结果。

## 2. 后续可消费公开 API

- `D20ActionIntent.Create(...)`、`Clone()`、`Validate()`；
- `D20ActionIntentContracts.Definitions`、`IsSupported(...)`、`TryGetDefinition(...)`；
- `ActionIdFor(...)`、`OwnerSystemFor(...)`、`Validate(...)`；
- `D20ActionIntentKind`、`D20ActionIntentStatus`、`D20ActionIntentValidationCode` 和八个稳定动作常量。

## 3. 验证与未完成项

- L02-L04 用 .NET Framework `csc.exe` 以 `/warnaserror+` 独立 library 编译：通过，exit 0；
- 纯 C# smoke：八个意图全部构造/校验通过，目录防御性副本和 ActionId 漂移拒绝通过；
- 源码静态边界检查：无禁止 engine/network/LLM/time/random 引用，无尾随空白；
- 未启动 Player、PlayMode、场景、输入或设备；未修改 D10/D11/D19。

真实 owner 路由和动作结算保持后续 L28/L31 或 Runtime Deferred，不由本任务越权实现。

## 4. Coordinator memory entry

请协调器在本批次汇合后统一写入当天日志：`D20-L04 LOGIC_PASS`；新增八动作意图冻结契约及严格校验，后续 L05 应只消费 `D20ActionIntentKind`/`D20ActionIntent`/定义目录，不自行创建动作字符串；L02-L04 csc、八意图 smoke 和静态边界检查通过；未启动 Runtime。
