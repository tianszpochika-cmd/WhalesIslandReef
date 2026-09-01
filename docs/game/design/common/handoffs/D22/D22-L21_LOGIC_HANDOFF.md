# D22-L21 NPC读模型适配 交接

- 状态：`LOGIC_PASS`。
- 本任务只实现/验证 D22 纯逻辑 NPC读模型适配；业务事实仍由对应 owner 裁决，适配层不写入 owner 状态。
- 真实验证：D22 隔离编译 0 warning / 0 error；D22 定向测试 10/10，通过 fail-closed、稳定排序、Clone 隔离、命令/焦点/面板/反馈边界断言。
- D21-L32 已为 `LOGIC_PASS`；本任务不启动 Unity Player/PlayMode，不涉及 Canvas、输入、场景、Prefab、性能或资源二进制。
- 结果：`game/tools/results/D22/D22-L21/D22_L21_LOGIC_RESULT.json`。
