# Unity CLI 插件能力与 VC-G01 回归交接

日期：2026-09-10  
状态：`PASS_WITH_SCOPE_LIMITS`

## 结论

Unity CLI 1.0.0-beta.9 已安装并可调用。它能在后台启动 Unity 6000.0.80f1 的 batchmode Editor，使用 `-nographics -runTests` 执行 EditMode/PlayMode 测试并输出 JSON、NUnit、JUnit 结果；这条路径不使用 Computer Use，也不抢占用户前台 Player 窗口。

这已经可以作为后续代码/逻辑回归主通道，但不能替代 standalone Player 的真实画面、物理键盘、窗口焦点和视觉验收。

## Pipeline 包安装结果

已通过 Unity CLI 安装并解析 `com.unity.pipeline@0.6.0-exp.1`。`Packages/manifest.json` 增加了该依赖，Unity 一次性 batchmode 解析后在 `Packages/packages-lock.json` 登记 `depth=0/source=registry`，并生成 `Library/PackageCache/com.unity.pipeline@9bb4172c603c`。

本次只启动了一次 `-batchmode -nographics -quit` 的 Unity Editor 做包解析；进程已退出，未启动 standalone Player，也未留下持久化 Editor。安装证据记录在 [UNITY_CLI_20260910_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/UNITY-CLI/UNITY_CLI_20260910_RESULT.json)。

后续连接能力探测已单独记录在 [UNITY_CLI_PIPELINE_SESSION_20260910_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/UNITY-CLI/UNITY_CLI_PIPELINE_SESSION_20260910_RESULT.json) 及其 [交接说明](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/VISUAL-CLEAN/UNITY_CLI_PIPELINE_SESSION_20260910_HANDOFF.md)。

日志出现包内 `System.Runtime.CompilerServices.Unsafe.dll` 重复版本警告；Unity 选择了 `com.unity.collections` 的版本并继续完成编译与退出，当前未观察到包解析失败。该警告仍应在后续 Editor/PlayMode 使用中关注。

## 已验证能力

- `unity editors --installed`：识别 Unity 6000.0.80f1，编辑器路径为 `C:/Program Files/Unity 6000.0.80f1/Editor/Unity.exe`。
- `unity projects info`：识别项目 `C:/Users/Administrator/Desktop/game/first01/game`，目标平台 `StandaloneWindows64`、URP。
- `unity test`：成功启动 batchmode、完成脚本编译并生成 NUnit/JUnit 报告；不需要 `com.unity.pipeline`。
- `unity pipeline list`：安装前确认没有可连接的 Pipeline Editor；注册表可见最新版本 `0.6.0-exp.1`。
- `unity pipeline install` + Unity batchmode resolve：安装后 manifest、packages-lock 和 PackageCache 三处均有证据。
- `unity command` / `unity status` / `unity list` / `unity mcp`：CLI 提供这些入口；包已接入，但实时 Editor 控制仍需另行启动并保持一个可达 Editor 实例。

## 真实回归证据

### 过滤器错误被正确识别

首次过滤器 `VC_G01WorldLogicTests` 只产生 `0` 个测试。Unity 日志明确写出 `No tests were executed`；虽然 CLI 返回 0，但本结果登记为 `NO_TESTS_EXECUTED_NOT_PASS`，没有伪造 PASS。

### 可匹配的 VC-G01 世界逻辑测试

过滤器改为 `WhalesIslandReef.Tests.D19Interaction.VcG01WorldLogicTests` 后，真实执行 `12` 项：`11 PASS / 1 FAIL`，CLI 错误码为 `TESTS_FAILED`，进程退出码为 `8`。

失败项：

`HelmPointUsesScaleIndependentWorldDeckOffset`：期望 world Y=`1.1 +/- 0.001`，实际 world Y=`0.96`。

这是一条可复现的真实逻辑失败证据，和用户反馈的“上船人物与舵/控制对象关系不对”属于同一风险方向；本交接只记录失败，不在本卡擅自改动锚点或场景坐标。

权威结构化结果：[UNITY_CLI_20260910_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/UNITY-CLI/UNITY_CLI_20260910_RESULT.json)。本次临时 NUnit/JUnit 文件位于 `C:/Users/Administrator/AppData/Local/Temp/`，路径也已写入结果 JSON。

## 当前门禁

- `VC-G01=IN_PROGRESS`：后台测试通道已可用，但当前有 `11/12` 通过、1 项锚点逻辑失败，且用户已经否决当前 Player 视觉质量。
- `VC-G02=LOCKED`、`VC-G03=LOCKED`：依赖 VC-G01，不能提前解锁。
- `D31=PARTIAL`：CLI 回归不能证明动作、游泳、树木接地、穿模、性能或完整流程。

## 下一步

1. 先修复并重新运行 `VcG01WorldLogicTests` 的 helm world-offset 失败；同时补充 W/S-HUD 输入归属、游泳和树接地的可执行 PlayMode 断言。
2. 用 CLI 运行可匹配的 EditMode/PlayMode 测试矩阵，并保存每次结果 JSON/JUnit；`0 tests executed` 永远不能升级状态。
3. 如需实时场景层级、Transform、截图或 Editor 命令，启动一次有界的无窗口 Pipeline Editor 并先验证 `unity status` / `unity pipeline list`；不要把“包已安装”误报成“实时 Editor 已连接”。
4. 继续保留一次短时 standalone Player 人工/原生输入验收，用于视觉和真实窗口焦点；不要求电脑连续运行数天。
