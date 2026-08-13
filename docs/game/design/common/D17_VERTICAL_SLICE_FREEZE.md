# D17 真人角色—船舶垂直切片冻结候选

> 状态：`PREPARED_NOT_FORMAL`（候选范围已冻结，正式 D17-S01 仍等待 D16-S02）
>
> 本文件只冻结验收范围、输入和证据格式，不改变运行时数值，也不把本机回归写成正式阶段通过。

## 1. 固定范围

| 项目 | 冻结内容 |
|---|---|
| 港口 | `InitialHarbor` / 圣玛格丽特港 |
| 玩家 | `D2_PlayerAvatar`，优先使用 `ph_crew_male`；缺失时仅允许程序化低模降级 |
| 船只 | `ph_ship_fishing` |
| 行走面 | `BeachGround`、`DockGround`、`GangplankGround` |
| 核心流程 | 出生 → 走到沙滩/码头 → 上船 → 离岸操船 → 调整视角 → 暂停/继续 → 保存/读取 → 下船 → 重启恢复 |
| 不在范围 | 第二座岛、更多船型、正式 Steam 上传、未授权最终资产、自动批准历史/敏感内容 |

## 2. 输入冻结

| 动作 | 键鼠 | 手柄 |
|---|---|---|
| 移动 | WASD | 左摇杆 |
| 奔跑 | Shift + 移动 | 由绑定设置决定 |
| 跳跃 | Space | 由绑定设置决定 |
| 上船/下船 | E | 由绑定设置决定 |
| 镜头旋转 | 按住鼠标右键/中键拖动 | 右摇杆 |
| 镜头缩放 | 鼠标滚轮 | 由绑定设置决定 |
| 切换视角 | F3 | 由绑定设置决定 |
| 暂停/继续 | Esc | Start（由绑定设置决定） |
| 保存 | F5 | 由绑定设置决定 |

模态菜单打开时，玩法输入必须锁定；关闭后必须恢复。EventSystem 只能有一个启用实例，所有按钮必须有可命中的 Graphic 和可执行回调。

## 3. 验收证据点

每次正式验收至少记录以下事实：

1. 玩家由真人角色或明确标记的过渡低模组成，不能是单一 Capsule 柱体；
2. 玩家由根 `CharacterController` 负责重力、落地和阻挡，脚底在合法行走面上；
3. 上船使用船舵位/甲板锚点，下船只落到声明的安全行走面；无安全面时必须拒绝并保留船上控制；
4. 船、人、相机和暂停菜单的控制权切换有日志，失败时不能进入不可退出状态；
5. 保存、读取、返回主菜单和重新进入后，玩家、船、控制模式和暂停状态一致；
6. 连续 10 次故事均无阻塞错误、菜单叠层、漂浮/倒置、穿透或按钮无响应。

## 4. 当前本机基线

- D17-A01/A03/A04/A06/A02 已有本机代码级回归；D17-S02 的本机十循环基线也已通过，最新全量 PlayMode 为 60/60；
- 十循环覆盖真实 MainMenu 新建游戏回调、InitialHarbor 运行时组装、人物落地/碰撞、上下船、船控、视角切换、保存、ESC 暂停/继续、返回主菜单、继续读取和下一轮重进；
- `tools/tests` 工具测试和相关 C# 工程构建必须在每次垂直切片改动后重跑；
- 本机证据不替代 D16-A01—A06 的目标设备、人工审查、最终资源、跨平台/断电和正式发布门禁；
- D17-S01 只有在 D16-S02 通过后才能从 `PREPARED_NOT_FORMAL` 改为正式冻结，D17-S02 只有连续 10 次真实故事证据齐全后才能通过。

## 5. 本机连续回归证据（2026-08-12）

- `tools/results/d17_s02_local_story_ten_cycles_20260812.xml`：聚焦 **1/1 PASS**，日志记录 `cycle 1/10` 至 `cycle 10/10 passed`；
- `tools/results/d17_full_after_s02_local_20260812.xml`：Unity 6 PlayMode **60/60 PASS**，0 failed / 0 inconclusive / 0 skipped；
- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**72/72 PASS**；
- `Assembly-CSharp`、`AgentCore`、`Save.Tests`、`D16.PlayMode.Tests`：均为 0 warning / 0 error；
- 首次聚焦运行只暴露了测试夹具在 ESC 注入后额外调用 `InputSystem.Update()`、抢先消费输入边沿的问题；已按已有成功回归的帧时序修正测试夹具，并重新取得上述 PASS。该失败不计作生产运行时失败，也没有通过删除断言来掩盖问题。

## 6. 交接待办

- [ ] D16-R01：267 条 Pending 由人工逐条决定并留下依据；
- [ ] D16-R05：第一批真人、船、码头、沙滩/碰撞最终资源具备真实来源和授权；
- [ ] D16-R03/R04/R06：目标/低端设备性能、真机可访问性、断电等价/跨平台存档；
- [ ] D16-R02：目标环境安装、更新、卸载、崩溃和隐私边界；
- [ ] D16-S01 → D16-S02；
- [ ] D17-S01 正式冻结 → D17-S02 连续 10 次验收。

## 2026-08-13 本机回归复验

- Unity 6 聚焦 `D17FailureRecoveryPlayModeTests.D17S02LocalStoryRepeatsTenTimes`：**1/1 PASS**，日志记录 `cycle 1/10` 至 `cycle 10/10 passed`；结果为 `game/tools/results/d17_s02_local_story_ten_cycles_20260813_retry.xml`。
- Unity 6 全量 PlayMode：**60/60 PASS**，`failed=0`、`inconclusive=0`、`skipped=0`；结果为 `game/tools/results/d17_full_after_s02_local_20260813.xml`，日志为 `tools/results/d17_full_after_s02_local_20260813.log`。
- 本轮再次覆盖本机的新游戏、角色落地/碰撞、上下船、船控、镜头、保存、ESC 暂停/恢复、返回主菜单、继续读取和重进链路；结论仍只属于 `LOCAL_BASELINE`。
- 本轮没有把 D16 外部 evidence envelope、真实最终资源、目标设备/真机、人工审查或 D17-S01/S02 正式状态改为通过。

## 7. 2026-08-13 角色物理与动作根因修复

- 新增最终聚焦证据 `game/tools/results/d17_s02_local_story_ten_cycles_20260813_final.xml`：**1/1 PASS**，日志明确记录 `cycle 1/10` 至 `cycle 10/10 passed`。
- 新增最终全量证据 `game/tools/results/d17_playmode_full_final_20260813.xml`：**60/60 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 修复 `ph_crew_male` 原始视觉高度只有 **0.84 世界单位**的问题：D2 运行时归一到约 **1.80 世界单位**，物理根节点保持单位缩放，视觉缩放转移到子树；碰撞体和脚底落地使用同一世界尺寸。
- 修复 D3 港口定位把玩家放回船体/跳板接缝的问题；玩家现在从安全码头/沙滩位置生成，并通过 D2 的合法行走面校准。
- 修复 D5 角色预设覆盖 D2 根节点缩放的问题；D5 不再破坏 `CharacterController` 的世界尺寸。
- 修复跳板接缝的步高换算、可行走面回吸和不可行走碰撞体攀爬；动作层只有真实跳跃才进入 `Jump`，平地移动稳定进入 `Walk/Run`。
- 受影响的 Unity/.NET 工程均为 **0 warning / 0 error**；工具测试为 **79/79 PASS**。

以上仍是 Windows/Unity 编辑器的 `LOCAL_BASELINE`，不替代 D16 外部门禁、真实授权最终资源、目标设备和人工验收；D17-S01/S02 正式状态保持未通过。
