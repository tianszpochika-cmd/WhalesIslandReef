# D19-L42 逻辑交接文档：对后续章节的港口逻辑接口

## 1. 任务元数据
- 任务编号：`D19-L42`
- 所属章节：`D19 港口领域逻辑（波次 19.7 整合写者与全流程验证）`
- 状态：`LOGIC_PASS`
- 编写范围：`docs/game/design/common/D19_LOGIC_PORT_INTERFACE.md`
- 交付物：
  - `docs/game/design/common/D19_LOGIC_PORT_INTERFACE.md`
  - `game/tools/results/D19/D19-L42/D19_L42_LOGIC_RESULT.json`
  - `docs/game/design/common/handoffs/D19/D19-L42_LOGIC_HANDOFF.md`

## 2. 接口规范要点
1. **统一命令入口**：`D19PortCommandRouter.Route(cmd)` 接收 `D19RoutedPortCommand` 并输出 `D19RoutedCommandResult`；
2. **状态与数据边界**：冻结 8 态生命周期 `D19PortVisitState`，公开 `D19PortReadModel` 只读读模型，外部系统只读消费；
3. **持久化契约**：定义 `D19PortSaveProjection`（SchemaVersion=1），说明 `D19PortSaveMigration` 迁移规则与数据损失拒绝边界；
4. **Runtime Deferred 清单**：明确物理坐标、碰撞、输入映射、UI 视觉、NPC 骨骼动画与场景资源留待 D20—D24 实现；
5. **证据链接**：完整链接 L25—L41 全部测试与静态审计工具。

## 3. 验证结果
- 统一运行器 `python tools/d19_logic_test_runner.py` 全 PASS；
- 状态同步脚本核验通过。
