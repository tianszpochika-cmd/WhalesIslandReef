# D24-L36 逻辑交接

- 状态：`LOGIC_PASS`，仅表示后续 Runtime 阶段的范围、入口条件、停止规则和证据规则已完成交接。
- 交接文档：`docs/game/design/common/D24_NEXT_RUNTIME_PHASE_HANDOFF.md`。
- 前置：D24-L35 纯逻辑候选版为 `LOGIC_PASS`；D24 任务状态必须由 `tools/sync_logic_task_status.py` 生成，不能手改。
- 首轮 P0：Player 启动/`InitialHarbor`、港口到船与出航、航行到达返港、真实 Save/Load/Continue；每项都要使用当前构建建立可重放的真实运行证据。
- 停止规则：崩溃/卡死/缺资源、输入不可达、视觉状态不可观察、owner/authority 绕过、非法状态或重复副作用、Save 丢失/重复写入、设备/性能达到阈值回归，或需要未授权外部依赖。
- 本任务未执行 Unity、Player、PlayMode、设备、网络、LLM、性能或真实 SaveSystem；`RUNTIME_DEFERRED` 保持不变。
- 不得从 `LOGIC_PASS` 或本 handoff 推导 `RuntimeApproved`、`ReleaseApproved`、playable 或发行通过。
