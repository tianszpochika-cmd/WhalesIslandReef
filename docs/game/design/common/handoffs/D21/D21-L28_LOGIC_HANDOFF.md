# D21-L28 交接

- 状态：`LOGIC_PASS`。
- 已覆盖锚泊/航行互斥、严重损坏拒绝、缺少 crew projection 的依赖阻断、重复命令/命令 ID 冲突和逆序事件批次原子拒绝。
- 所有负向断言保持 fail-closed，不把缺少上游事实或失败路径写成成功；测试只消费公开纯逻辑 API。
- 未执行 Unity Player/PlayMode、场景、Prefab、物理、输入、设备、性能或发行验证。
- 结果：`game/tools/results/D21/D21-L28/D21_L28_LOGIC_RESULT.json`。
