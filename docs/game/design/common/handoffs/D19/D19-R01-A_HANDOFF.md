# D19-R01-A 交接报告

状态：**PARTIAL**

任务：D19-R01-A（真实玩家路线与性能取证，只读）  
日期：2026-08-15  
单一写者范围：`game/tools/results/D19/D19-R01-A/`、本报告、`D19_R01_PLAYER_ROUTE_AND_PERFORMANCE_EVIDENCE.md`、当日记忆日志。

## 已完成

- 使用 `WhalesIslandReef_D19_S02_R01_Player/WhalesIslandReef.exe` 在真实可交互 Player 中执行了 5 分 38 秒活跃路线：出生/落地、移动、NPC 尝试、市场交易、离港事件、登船尝试、F5 保存、返港/泊位重试。
- 真实输入为键盘、鼠标；取得 9 张带本地时间的路线截图和 5 张性能截图，均为 2194×1234 的实际屏幕帧，保持 16:9。
- 市场交易实际反馈：500→496，`event_d9_trade_settled`，owner=`EconomySystem`。
- 离港实际反馈：`event_d9_departure_prepared`，owner=`ShipSystem`，状态显示“航行中”；但画面未从步行海滩切换。
- F5 后看到存档检查点/事件反馈；没有观察到重复奖励或新库存赠送。
- Low/Medium/High 三档都进行了真实窗口切换和驻留。Low/Medium 约 60/67 秒；High 约 25 秒。
- 对截图进行逐项可见性审查，并生成重复哈希检查；14 个截图哈希均唯一。
- `git diff --check` 退出码 0，仅有既有换行符提示。

## 未完成/阻断

- NPC 对话：`E` 无对话、toast 或明确失败反馈。
- 登船/下船：`E` 无可见转场，仍停留海滩步行视角。
- 返港/泊位：`B` 重试无可见转场，状态仍为“航行中”。
- 未确认原生 1920×1080；真实 Player 返回的屏幕证据为 2194×1234，不作原生 1080p 结论。
- 未取得有效平均 FPS、帧时间、1% Low、分位数、GPU 时间、Unity GC 或 VSync 证据。D5 叠加层的 `MinFPS 2.7` 未被当作 1% Low。
- High 采样短于 60 秒目标。
- 本任务没有在独立只读副本重新执行 Unity 测试；既有 D19/D17 XML 与报告仅作回归背景，不冒充真实 Player。

## 最严重可见问题

真实画面中存在大面积白色/薄荷绿色平面、块状稀疏地貌和重叠状态/调试文字；更严重的是离港事件和“航行中”状态出现后仍保留海滩步行视角，且 NPC/登船/泊位操作没有可见反馈。这直接阻断玩家路线判断。

## 交付物

- 人工报告：`docs/game/design/common/D19_R01_PLAYER_ROUTE_AND_PERFORMANCE_EVIDENCE.md`
- 机器证据：`game/tools/results/D19/D19-R01-A/D19_R01_A_EVIDENCE.json`
- 截图哈希：`game/tools/results/D19/D19-R01-A/D19_R01_A_SCREENSHOT_HASHES.json`
- 截图目录：`game/tools/results/D19/D19-R01-A/screenshots/`
- 交接报告：本文件
- 记忆日志：`.workbuddy/memory/2026-08-15.md`

## 结论

**PARTIAL**。本结果可用于复现路线阻断和当前可见问题；不能作为 D19 真实玩家路线 PASS 或 1920×1080 完整性能 PASS。重试时需先提供原生分辨率确认和可见的 NPC/泊位/登船/返港反馈，再补齐帧时间、1% Low、分位数、GPU、GC 与 VSync 数据。
