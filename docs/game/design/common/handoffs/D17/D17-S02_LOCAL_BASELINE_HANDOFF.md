# D17-S02 本机连续流程基线交接

日期：2026-08-12

## 状态

**本机代码级基线通过；正式 D17-S02 未启动。**

本交接只记录当前 Windows/Unity 6 编辑器中的自动化回归结果。它不能替代真实设备、真实手柄、人工视觉/可访问性、最终授权资源、断电/跨平台和 D16 前置门禁。

## 测试范围

`D17FailureRecoveryPlayModeTests.D17S02LocalStoryRepeatsTenTimes` 从 `MainMenu` 的真实新建游戏按钮开始，连续 10 次执行：

1. 进入 `InitialHarbor`，等待 D2/D7 运行时组装完成；
2. 检查 `D2_PlayerAvatar` 的 `CharacterController`、落地和碰撞；
3. 上船、验证船只控制权、切换视角、下船并验证安全行走面；
4. 保存，按 ESC 暂停，点击继续，再次按 ESC；
5. 点击返回主菜单，验证时间缩放恢复、暂停画布清理；
6. 点击继续读取并进入下一轮。

## 交付与证据

- `game/Assets/Tests/PlayMode/D17FailureRecoveryPlayModeTests.cs`：新增十循环本机回归及等待运行时/按钮的辅助方法；
- `tools/tests/test_project_scene_settings.py`：新增实际 Unity 工程场景顺序、启用状态和 GUID 回归保护；
- `tools/results/d17_s02_local_story_ten_cycles_20260812.xml`：聚焦 **1/1 PASS**；日志含 `cycle 1/10` 至 `cycle 10/10 passed`；
- `tools/results/d17_full_after_s02_local_20260812.xml`：全量 PlayMode **60/60 PASS**；
- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**72/72 PASS**；
- `Assembly-CSharp`、`AgentCore`、`Save.Tests`、`D16.PlayMode.Tests`：0 warning / 0 error。

## 失败复盘

第一次聚焦执行在第 1 次 ESC 处失败。日志显示运行时已进入港口、完成上/下船和保存；失败来自测试夹具在排队键盘事件后额外调用 `InputSystem.Update()`，抢先消耗了输入边沿。已按已有成功测试的“按下 → 下一帧 → 松开 → 下一帧”时序修正夹具，重新运行后 10 次全部通过。没有删除暂停断言，也没有修改生产暂停逻辑来制造绿灯。

## 数据所有权与边界

- 本轮只修改 D17 PlayMode 测试、测试工具和交接/计划文档；没有修改 Systems、AgentCore、contracts、存档裁决、AssetRegistry 状态或 Steam 状态；
- 当前场景仍允许程序化低模降级，不能把它当成最终真人动作/美术资源；
- D16-A01—A06 外部门禁仍为 `IN_PROGRESS`，D16-S01/S02 和正式 D17-S01/S02 仍未开始；
- 尚缺真实手柄/目标分辨率人工测试、目标/低端设备性能、物理断电与跨平台存档、A04 267 条 Pending 人工复核、A05 最终授权资源、A06 目标机原生崩溃/隐私证据。

## 下游

1. 先完成 D16 外部真实性门禁和 D16-S01/S02，不把本机 60/60 当成阶段验收；
2. D16-S02 通过后，将本文件的十循环夹具与正式设备矩阵、人工操作记录合并；
3. 只有真实故事证据齐全后，才把 D17-S02 状态改为 PASS。

## 2026-08-12 当前最终复核

- 本机十循环聚焦证据仍为 **1/1 PASS**，Unity 6 全量为 **60/60 PASS**；
- 工具测试当前为 **77/77 PASS**；
- 本交接仍是 `LOCAL_BASELINE_ONLY`，不改写为正式 D17-S02 PASS；D16 外部真实性门禁和真人/目标设备人工验收仍未完成。

## 2026-08-13 本机回归复验

- 聚焦十循环：`game/tools/results/d17_s02_local_story_ten_cycles_20260813_retry.xml`，**1/1 PASS**，日志中十个 cycle 全部通过。
- 全量 PlayMode：`game/tools/results/d17_full_after_s02_local_20260813.xml`，**60/60 PASS**，0 failed / 0 inconclusive / 0 skipped；日志：`tools/results/d17_full_after_s02_local_20260813.log`。
- 这次复验只确认当前 Unity 6 工作区基线未回退，仍不替代 D16-A01—A06 外部真实性证据，也不把 D17-S01/S02 改为正式通过。

## 2026-08-13 角色物理/动作缺陷收口

- 最终聚焦十循环：`game/tools/results/d17_s02_local_story_ten_cycles_20260813_final.xml`，**1/1 PASS**，10/10 cycle 全部通过。
- 最终全量 PlayMode：`game/tools/results/d17_playmode_full_final_20260813.xml`，**60/60 PASS**；没有失败、不确定或跳过项。
- 根因：D3 的旧港口定位把玩家写回 `-5f + up` 的船体接缝，D5 的角色预设又重写 D2 物理根节点缩放；这两处共同造成角色漂浮/抬升、平地被判 `Jump`、碰撞体与模型不一致。
- 修复：D2 世界尺寸归一和单位物理根节点、世界步高换算、合法行走面回吸、禁止攀爬非行走碰撞体；D3 使用安全出生点并调用 D2 落地校准；D5 对 D2 角色不再改写根节点缩放；动作层按真实跳跃状态判定。
- 验证：`Assembly-CSharp`、`Save.Tests`、`D16.PlayMode.Tests`、`AgentCore`、`Whalebone.Systems` 均 0 warning / 0 error；工具测试 **79/79 PASS**；`git diff --check` 通过。

本交接仍为 `LOCAL_BASELINE_ONLY`。D16 A01—A06 的外部证据、授权最终资源、目标设备/人工验收以及 D17-S01/S02 正式门禁仍未完成。
