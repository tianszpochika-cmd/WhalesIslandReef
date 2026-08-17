# D19-L37 逻辑交接文档：固定种子不变式模糊测试

## 1. 任务元数据
- 任务编号：`D19-L37`
- 所属章节：`D19 港口领域逻辑（波次 19.7 整合写者与全流程验证）`
- 状态：`LOGIC_PASS`
- 编写范围：`game/Assets/Tests/EditMode/D19/D19PortFuzzInvariantTests.cs`
- 交付物：
  - `game/Assets/Tests/EditMode/D19/D19PortFuzzInvariantTests.cs`
  - `game/tools/results/D19/D19-L37/D19_L37_LOGIC_RESULT.json`
  - `docs/game/design/common/handoffs/D19/D19-L37_LOGIC_HANDOFF.md`

## 2. 测试目标与方法
使用确定性 LCG 伪随机数生成器（固定种子 `16751937` 与 `20260818`），生成超过 4,400 次混合港口命令序列，对系统进行每步强不变式校验：
1. **生命周期一致性**：`Berthed` 与 `InService` 状态必须持有效 `PortId` 与 `BerthId`；`None` 与 `Departed` 状态必须无 `BerthId`；
2. **幂等一致性**：已成功的命令重复投递必须标记 `IsDuplicate=true`，返回相同结果且不向事件流追加重复事件；
3. **存档投影有效性**：每步提取的 `D19PortSaveProjection` 必须满足 `SchemaVersion=1` 并通过 `Validate()`；
4. **事件流审计合规性**：产生的所有真实事件必须通过 `D19PortEventOrderAudit` 的全维度审计（单调序列、无重复 ID、无非法跳跃）；
5. **混沌载荷鲁棒性**：面对空 ID、非法枚举、超长文本、负数世界日等异常输入，系统全部优雅拦截并返回稳定原因码，绝不崩溃。

## 3. 验证结果
- 执行 `dotnet run` 运行全部 4 组模糊测试，累计执行 4,400+ 命令，结果全部 **PASS**；
- 状态同步脚本核验通过。
