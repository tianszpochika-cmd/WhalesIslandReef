# D17-A06 失败恢复交接

## 本轮目标

验证垂直切片在“新建游戏 → 进入港口 → 暂停 → 返回主菜单 → 再次新建游戏 → 再次进入港口”过程中，不出现时间缩放残留、暂停画布叠层、EventSystem 竞争、输入失焦或运行时玩家缺失。

## 实现与测试

- 新增 `game/Assets/Tests/PlayMode/D17FailureRecoveryPlayModeTests.cs`；
- 测试从 `MainMenu` 的真实按钮回调开始，不直接伪造 `InitialHarbor` 初始状态；
- 等待 D2 分阶段组装完成后再断言玩家对象，避免把“场景已切换但运行时尚未接线”误报为生产失败；
- 覆盖 ESC 暂停、时间缩放为 0、返回主菜单、时间缩放恢复为 1、旧 `HarborPauseCanvas` 清理、`MainMenuController` 重建、单一启用 `EventSystem`、第二次新建游戏和第二次 `D2_PlayerAvatar`/`HarborPauseController` 创建。

## 证据

- 聚焦测试：Unity LocalLow `TestResults.xml`，`D17A06PauseReturnMainMenuAndStartNewGameRestoresRuntime` **1/1 PASS**；
- 全量 PlayMode：`tools/results/d17_full_after_failure_recovery_20260812.xml` **58/58 PASS**；
- 全量日志：`tools/results/d17_full_after_failure_recovery_20260812.log`，包含两次 `InitialHarbor` 运行时入口、第二次真人角色绑定、物理体、动画、D7 表现和暂停控制器日志；
- 本轮没有修改 Systems、AgentCore、contracts、存档裁决、AssetRegistry 状态或 Steam 状态。

## 状态与边界

D17-A06 的本机代码级失败恢复链通过。它不等同于 D16-A06 发布门禁，也不替代目标机器安装/更新、原生崩溃、真实手柄和人工焦点、最终授权资源、跨平台/断电存档以及 D17-S02 连续 10 次故事验收。D16-A01—A06 仍有外部真实性缺口，因此 D16-S01/S02、D17-S01/S02 仍未启动。

## 交接下一步

1. 保留本机全量回归证据，不重复把它计入目标设备或人工验收；
2. D16 外部门禁满足后，先冻结 D17-S01 的设备矩阵和固定故事；
3. 将本回归作为 D17-S02 的单次失败恢复夹具，接入连续 10 次正式故事验收。
