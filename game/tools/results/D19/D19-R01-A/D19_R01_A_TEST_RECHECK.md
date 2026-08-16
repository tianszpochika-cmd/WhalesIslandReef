# D19-R01-A 测试与完整性复核

## 本任务实际执行

- `git diff --check`：退出码 0；输出仅为已有文件的 LF→CRLF 换行提示，没有 whitespace error。
- 机器 JSON：`D19_R01_A_EVIDENCE.json` 与 `D19_R01_A_SCREENSHOT_HASHES.json` 均通过 PowerShell JSON 解析。
- 截图哈希：14/14 唯一，重复哈希 0。

## 既有 D19/D17 结果复核

以下是开始前读取并在本任务报告中保留边界的既有结果；它们不是本次真实 Player 路线的替代品：

- `game/tools/results/D19/D19-S02-R01/D19_S02_R01_ROUTE_LOG.json`：自动化路线 2/2，报告明确为 automated probe only。
- `game/tools/results/D19/D19-S02-R01/D19_S02_R01_PERFORMANCE_1080P_RERUN_V2.json`：真实 Player 目标分辨率/性能 NOT_RUN；旧批处理 640×480 不作为本次 1080p 证据。
- `game/tools/results/D19/D19-S02-R01/D19_S02_R01_VISUAL_QA.json`：16 张 Editor 截图，未有 Player capture。
- `game/tools/results/d17_s02_local_story_ten_cycles_20260812.xml`：D17 本机连续基线 1/1。
- `game/tools/results/d17_playmode_full_after_save_transaction_20260813.xml`：保存事务后 D17 本机 PlayMode 60/60。

本任务没有在同一工作区重新启动 Unity Editor 测试 runner：该 runner 会写入项目 Library/测试日志，无法在本只读证任务的单一写者范围内把这些副作用误称为纯只读。现有 XML/报告仅作回归背景，真实 Player 结果以 `D19_R01_A_EVIDENCE.json` 为准。
