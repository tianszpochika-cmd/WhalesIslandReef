# D21-L05 逻辑交接

## 任务状态

- 状态：`LOGIC_PASS`。
- 范围：货舱容量、危险货物隔离和超载后果评估；D3 数量 owner 未被改动。

## 已完成

- `D21CargoCapacityRules` 读取货物行、重量/体积容量和船侧储运条件，输出确定性评估。
- 支持增加/移除候选、容量剩余、重量/体积超载比例和 ReducedSpeed / RestrictedManeuver / UnsafeDraft / DepartureRefused 分级后果。
- 校验防火隔离、火药舱、通风、活体舱，以及同一隔离舱中的食品/毒物、爆炸物/可燃物等冲突。
- `TryBuildOwnerMutationRequest` 只生成交给 D3 的请求，不写货物数量。

## Public API / owner 边界

- 主要入口：`ValidateLine`、`ValidateSnapshot`、`Evaluate`、`IsDangerousGoodsCompatible`、`TryBuildOwnerMutationRequest`。
- 主要 DTO：`D21CargoLineSnapshot`、`D21CargoCapacitySnapshot`、`D21CargoCapacityEvaluationRequest`、`D21CargoCapacityDecision`、`D21CargoMutationRequest`。
- D3InventorySystem 仍是货物数量和库存字典的唯一写者；本文件不复制 D3 库存状态。

## 验证

- `/warnaserror+` 独立 C# library 编译通过，XML docs 生成成功。
- 冒烟通过 5 项：正常容量、D3 owner request、危险货物拒绝、超载后果、移除评估。
- D21-L02—L08 最终合编通过；可执行行禁用依赖扫描命中 0。
- 结果文件：`game/tools/results/D21/D21-L05/D21_L05_LOGIC_RESULT.json`。

## 未完成 / Runtime Deferred

- D3 实际库存写入、市场/货舱 UI、装载表现、吃水/航速/操纵物理、场景、输入、VFX、音频、性能、真机和发行门禁均未执行。

## Blockers / 下一步

- 本项无纯逻辑 blocker。接入时由 D3 验证 request 的版本和数量余额，再由 D21 消费新的只读容量快照。

## Coordinator memory entry

`2026-09-01：D21-L05 已完成货物容量、危险隔离和分级超载评估；通过只读 D21CargoMutationRequest 对接 D3，D21 不写库存数量。`
