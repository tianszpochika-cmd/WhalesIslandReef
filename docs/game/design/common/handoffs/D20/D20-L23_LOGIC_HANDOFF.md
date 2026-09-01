# D20-L23 Logic Handoff

## 状态

- task_id：`D20-L23`
- status：`LOGIC_PASS`
- 结果文件：[D20_L23_LOGIC_RESULT.json](../../../../../../game/tools/results/D20/D20-L23/D20_L23_LOGIC_RESULT.json)

## 已完成

- 新增 `D20PetOwnershipSystem`，只保存 `pet_id -> owner_id` 的 D20 所有权投影和 revision。
- 通过 `D11PetSystem` 公开 API 实现领养、Feed、Clean、Comfort、MarkLost、Search 的协调结果。
- 通过公开 `D11PetState` 快照计算跟随资格：owner 匹配、宠物已领养、健康有效且位于可跟随位置时才允许；隔离、走失、收容、死亡、owner 不匹配和数据缺失均返回稳定失败码。
- D11 `AnimalCareSystem` 仍是宠物生命、照料、位置和走失事实的唯一写者；D20 不写这些字段，也不接库存、关系或生态系统。

## 公开接口与接线

- 构造：`D20PetOwnershipSystem(D11PetSystem petSystem, D20PetOwnershipSnapshot initialSnapshot = null)`。
- 写协调：`Adopt`、`Feed`、`Clean`、`Comfort`、`MarkLost`、`Search`。
- 读模型：`PetSnapshot`、`Snapshot`、`EvaluateFollowEligibility`、`CanFollow`。
- 所有返回的 D11 state、ownership record 与 follow model 均为副本，避免调用方取得 owner 内部可变引用。

## 依赖核对

- 执行时未找到 D20-L20、D20-L21、D20-L22 的生产源文件、结果文件和 handoff 文件。
- 本任务未复制或臆造这些类型，使用当前存在的 D11 `D11PetSystem` 与 `D11PetState` 公开接口完成逻辑；后续若这些任务补充公开契约，可在适配层消费，不应改变 D11 唯一写者边界。

## 验证证据

- `dotnet build game/Whalebone.Systems.csproj --no-restore --nologo`：0 warnings，0 errors。
- `dotnet build .codex-temp/D20LogicCompile.csproj --no-restore --nologo`：D20-L23/L24/L25 纯 C# 目标编译通过，0 warnings，0 errors。
- `dotnet run --project .codex-temp/D20LogicSmoke.csproj --no-restore --nologo`：`D20-L23/L24/L25 LOGIC SMOKE PASS`，含成功和失败路径及读取无副作用断言。
- 禁止依赖扫描 0 hits；L23 跨领域 owner 类型扫描 0 hits；目标文件尾随空白 0；`git diff --check` exit code 0。

## 待办与运行时延期

- Unity MonoBehaviour、场景/Prefab、UI、输入、动画、音频、物理、设备、PlayMode、可见跟随表现、存档接线和性能验证均未在本逻辑任务中执行。
- 本 handoff 不声明 `RuntimeApproved`、发布就绪或真实设备通过。

## Coordinator memory entry

请协调者将本任务结果同步到共享状态与 2026-09-01 记忆；本子任务未修改全局状态文件或 `.workbuddy/memory`。
