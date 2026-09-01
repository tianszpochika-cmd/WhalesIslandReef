# D20-L22 逻辑交接说明

- 任务：`D20-L22`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L22/D20_L22_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20AnimalBehaviorStateMachine.cs`，实现不依赖 Unity 的确定性动物行为投影：

- `D20AnimalBehaviorState` 覆盖 `Rest`、`Forage`、`Alert`、`Flee`、`Follow`、`Injured`、`Dead`；`Dead` 是终止状态；
- `D20AnimalBehaviorInput` 只接受稳定 `command_id`、`animal_id`、适用的 `target_id`/`cause_event_id`、显式 `world_day`/`save_revision` 和 owner 提供的恢复确认；
- 状态迁移由显式输入决定：威胁发现/解除、觅食完成、跟随目标丢失、取消、受伤、确认恢复和死亡都拥有稳定迁移/失败码；不从动画、距离、碰撞、位置或环境数值隐式推断；
- 物种能力只读消费 `D20AnimalSpeciesDefinition.Capabilities`：宠物可由 `Follow` 能力进入跟随，缺少能力的野生定义返回 `Unavailable`，不修改物种目录；
- 每个有效命令生成确定性 `event_d20_animal_behavior:<animal_id>:<command_id>`；同一 `command_id` 同指纹返回 `Duplicate` 历史结果，不重复推进投影，换指纹返回冲突结果；
- `D20AnimalBehaviorSnapshot` 保存 D20 行为状态、跟随目标、投影修订和回执，并提供校验/恢复；恢复只写回 D20 自有投影，不重建外部 owner 事实。

## 2. 后续可消费公开 API

- `D20AnimalBehaviorStateMachine.Apply(D20AnimalBehaviorInput)` / `Transition(...)`：唯一行为迁移入口；
- `D20AnimalBehaviorStateMachine.CanTransition(...)`、`IsSupportedState(...)`、`IsSupportedInput(...)`：纯逻辑前置查询；
- `CurrentState`/`State`、`FollowTargetId`、`ProjectionRevision`、`CanRecover`、只读 `Species`：行为读面；
- `CaptureSnapshot()` / `Restore(D20AnimalBehaviorSnapshot)`：D20 行为投影保存边界；
- `D20AnimalBehaviorResult`：`Status`、`Applied`、`Duplicate`、`StateChanged`、`ResultCode`、`ReasonCode`、`EventId`、前后状态及投影修订；
- `D20AnimalBehaviorInputValidationCode`：schema、ID、目标、因果引用、世界日、存档修订和恢复确认失败码。

## 3. 所有权与限制

- 本任务只拥有 D20 行为状态、显式输入结果、幂等回执和行为快照；
- `EcologySystem` 继续拥有野生/家养生态、种群、压力和生命周期事实；`AnimalCareSystem` 继续拥有宠物健康、照料、位置、隔离、丢失和死亡事实；
- D20 不计算或写入健康数值、生态数量、资源/库存、关系、经济、奖励、法律、坐标、寻路或表现状态；
- Presentation 只能读取结果/快照并提出下一条输入，不能以动画、Prefab、模型或 UI 成功覆盖逻辑结果。

## 4. 验证与未完成项

- .NET Framework `csc.exe` 对 L20/L21/L22 进行 `/warnaserror+` library 编译：exit 0，XML 文档生成成功；
- Add-Type 正/负 smoke 覆盖迁移路径、非法输入、幂等重放与冲突、Follow 目标变化、快照恢复、确定性事件和物种能力边界，全部通过；
- 目标源文件静态检查：无 Unity/网络/LLM/系统时钟/随机实现引用，尾随空白为 0；结果 JSON 解析和 `git diff --check` 通过；
- 未启动 Player、PlayMode、场景、输入、物理、资源导入或设备验证；这些内容保持 Runtime Deferred。

模型、Prefab、Animator、场景、Transform、物理、寻路、输入、设备、资源准入以及 D11 owner 的真实运行时接线继续保持 Runtime Deferred。

## 5. Coordinator memory entry

请协调器在本批次汇合后统一写入当天日志：`D20-L22 LOGIC_PASS`；新增 `D20AnimalBehaviorStateMachine`，以显式输入实现动物行为迁移、失败回退、能力前置、确定性事件、命令幂等和快照恢复；L20/L21/L22 独立 csc、正负 smoke、禁用引用扫描、JSON 解析和 diff 校验通过；未启动 Runtime，生态/宠物事实仍归 D11 owner。
