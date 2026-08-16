# D19-R03-S01 RERUN23 真实 Player 输入复验

日期：2026-08-16
结论：**BLOCKED**

本轮只验证 R03-A02 的时序修复是否在真实 Windows Player 中生效，并复验一次玩家输入焦点。RERUN20 及更早报告、机器结果和截图均保留；本轮没有覆盖历史文件，也没有把短输入尝试解释为完整路线。

## 已验证

- Unity 场景/表现重建成功，退出码 0：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_BUILD_RERUN21_COLLISION_WAIT.log`。
- Windows Player 构建成功，退出码 0：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_BUILD_PLAYER_RERUN21_UNITY.log`。
- 真实 Player 日志确认 `ShoreBoundary z=17.0`，并确认 D6 collider owner retained：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN23_PLAYER.log`。
- RERUN23 真实 Player 持续 `160.012863 s`，探针采样 `538` 条、事件 `4` 条；玩家、控制器和路由器均找到。
- 交互输入边沿被观察到，但 `moved_observed=false`、`save_input_observed=false`。探针最终位置约为 `z=-6.49`，没有形成可复核的市场/仓库/酒馆/船坞/返港路线。
- 新的独立探针归档：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN23.json`。
- RERUN21 画面证据：`game/tools/results/D19/D19-R03-S01/RERUN21_HARBOR_AFTER_FIX.png`。它仅说明修复后的 R03 表现层在 Player 中可见，不是 14 镜头或视觉最终通过证据。

## 未通过与责任

| 门禁 | 结果 | 责任入口 |
|---|---|---|
| 真实玩家连续路线 | BLOCKED；本轮无可复核位移 | D19-R03-A01 / 新的 D19 Integration 输入与碰撞修复 |
| 保存与重载 | 本轮未运行；历史仅有有限 Continue 恢复 | D19-R03-A01 |
| 视觉最终门禁 | 仍为 PARTIAL；历史 Calm-only，平坦/块状细节债务仍在 | D19-R03-A02 |
| 天气变体 | 0/3 最新证据 | EnvironmentSystem 受控天气取证责任 |
| 性能 | 仅沿用历史 A03 三档数据，本轮未重新采样 | D19-R03-A03 / 最终 S02 门禁 |

## 决策

`D19-R03-S01` 继续 BLOCKED，`D19-R02-S02`、`D20-A01-R02`、`D20-A02-R02` 和 `D20-S02-R01` 不放行。此次补丁修正了“HUD 先出现、ShoreBoundary 尚未建立”的时序缺陷，但没有证明真实玩家输入形成位移，因此不能把路线或保存闭环写成 PASS。

机器结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN23.json`。A02 复验机器结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN23_INPUT_RECHECK.json`。
