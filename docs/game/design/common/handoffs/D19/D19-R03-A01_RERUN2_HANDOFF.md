# D19-R03-A01 RERUN2 交接

- 任务：D19-R03-A01
- Attempt：RERUN2
- 日期：2026-08-16
- 结论：**PARTIAL**

## 实际修改

- 保留上一轮 `D19_R03_A01_MACHINE_RESULT.json`、`D19_R03_A01_ROUTE_LOG.json` 和原始探针结果；本轮新建 `*_RERUN2_*` 结果，不覆盖旧证据。
- 为本轮真实 Player 运行保留 12 张截图和新的运行时探针结果。
- 未修改 InitialHarbor、Systems、Registry、存档 Schema、contracts 或 D19-A01—A06 工作成果。

## 真实验证

- Unity 编译：`D19_R03_A01_UNITY_COMPILE_RERUN1.log`，退出码 0。
- Player 构建：`D19_R03_A01_PLAYER_BUILD_RERUN2.log`，退出码 0。
- 真实 Player 时长：584.5516 秒。
- 移动：通过；落地观察：通过；Character/Ship 模式：均观察到。
- 交互：有真实 E 输入观察；码头/泊位/酒馆坐标邻域事件存在。
- 截图：12 张真实 Player 截图。

## 未通过项

1. 市场、仓库、船坞、返港没有在同一完整路线中被证明。
2. 探针没有观察到保存输入；没有保存后退出、重载和状态恢复证据。
3. 反复按键产生了位移，但没有证明连续按住 WASD 的路线行为。
4. 当前探针对新版表现对象名查找仍为 false，坐标 proximity 不能替代对象/交互解析。

## 下游注意事项

- D19-R03-S01 只能据此保持 BLOCKED/PARTIAL，不能放行正式 D19-R02-S02。
- D19-R03-A02 的视觉债务仍需单独处理；本交接不把路线证据升级为视觉通过。
- D20-A01-R02、D20-A02-R02、D20-S02-R01 仍受 D19 最终 Player 门禁约束。
