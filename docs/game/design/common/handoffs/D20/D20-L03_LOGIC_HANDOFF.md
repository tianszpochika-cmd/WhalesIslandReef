# D20-L03 逻辑交接说明

- 任务：`D20-L03`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L03/D20_L03_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20PlayerPresenceSystem.cs`，建立玩家语义 presence 的单一 D20 owner：

- `D20PlayerPresenceState` 使用互斥的 `Port`、`Ship`、`Voyage`、`Unavailable` 状态，`Unknown` 仅作为未初始化载体；
- `D20PlayerPresenceTransitionReason` 冻结进入港口、登船、下船、开始航行、返港、失能、恢复和刷新原因；
- `D20PlayerPresenceRequest`、`D20PlayerPresenceResult`、`D20PlayerPresenceEvent` 和 `D20PlayerPresenceSnapshot` 均为纯数据 DTO；
- `D20PlayerPresenceSystem.Apply/Transition` 按前态+原因裁决迁移，拒绝非法状态组合，失能时保存恢复前态；
- 命令 ID 与请求指纹用于重复命令回放/冲突拒绝，快照包含回执以支持读档后继续幂等保护；
- `CaptureSnapshot/Restore` 只处理 D20 自有语义投影，不推断外部实体。

港口、船舶、健康、输入、空间位置、碰撞、场景和表现事实没有在本系统内复制或写入。

## 2. 后续可消费公开 API

- `D20PlayerPresenceSystem.Apply(...)` / `Transition(...)`；
- `D20PlayerPresenceSystem.CanTransition(...)`、`IsSupportedState(...)`、`IsActiveState(...)`、`ValidateLocation(...)`；
- `D20PlayerPresenceSystem.Snapshot`、`CaptureSnapshot()`、`Restore(...)`；
- `D20PlayerPresenceRequest`、`D20PlayerPresenceResult`、`D20PlayerPresenceEvent`、`D20PlayerPresenceSnapshot` 的字段和 `Clone/Validate`。

## 3. 验证与未完成项

- L02+L03 用 .NET Framework `csc.exe` 以 `/warnaserror+` 独立 library 编译：exit 0；
- 运行时无引擎 smoke：覆盖 Port→duplicate→Ship→Voyage→Unavailable→Recover→Port，8/8 通过；
- 源码静态边界检查：无禁止 engine/network/LLM/time/random 引用，无尾随空白；
- 未启动 Player、PlayMode、场景、输入或设备；未修改 D10/D11/D19。

港口 Router、船舶/健康 owner 的真实接线和空间/表现验证保持 Runtime Deferred，不构成 L03 逻辑阻断。

## 4. Coordinator memory entry

请协调器在本批次汇合后统一写入当天日志：`D20-L03 LOGIC_PASS`；新增 `D20PlayerPresenceSystem`，冻结 Port/Ship/Voyage/Unavailable 迁移、恢复前态、稳定失败码、请求指纹和快照回执；L02+L03 独立 csc、presence smoke 和静态边界检查通过；未启动 Runtime。
