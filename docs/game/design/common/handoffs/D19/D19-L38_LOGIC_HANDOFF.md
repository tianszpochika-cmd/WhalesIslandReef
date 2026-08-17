# D19-L38 逻辑交接文档：港口保存/加载往返测试

## 1. 任务元数据
- 任务编号：`D19-L38`
- 所属章节：`D19 港口领域逻辑（波次 19.7 整合写者与全流程验证）`
- 状态：`LOGIC_PASS`
- 编写范围：`game/Assets/Tests/EditMode/D19/D19PortSaveLoadTests.cs`
- 交付物：
  - `game/Assets/Tests/EditMode/D19/D19PortSaveLoadTests.cs`
  - `game/tools/results/D19/D19-L38/D19_L38_LOGIC_RESULT.json`
  - `docs/game/design/common/handoffs/D19/D19-L38_LOGIC_HANDOFF.md`

## 2. 测试覆盖与往返等价性验证
1. **节点 1（PortVisit）**：到港状态保存，恢复后重放 EnterPort 幂等返回 `IsDuplicate=true`，继续执行 Dock 命令成功转入 `Docked`；
2. **节点 2（Berthed & Lease）**：靠岸并完成泊位分配后保存，恢复后维持 `Berthed` 与 `BerthId`，继续请求市场服务（`StartService`）成功进入 `InService`；
3. **节点 3（CrewLocations）**：船员 `OnShore` / `OnBoard` 逻辑位置字典与投影双向往返映射，数量与枚举值 100% 保持；
4. **节点 4（PendingCommands & CompensationDrafts）**：挂起命令与补偿草案深度克隆/往返序列化验证，ID、领域、原因与参数字典完整无损；
5. **节点 5（Depart & Return）**：离港状态保存恢复后，继续投递返港命令（`Return`）确定性转入 `Returned` 状态。

## 3. 验证结果
- `dotnet run` 运行 5 个节点往返测试全部 **PASS**；
- 状态同步脚本核验通过。
