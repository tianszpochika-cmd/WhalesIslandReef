# D19-R03-A01 RERUN7：真实 Player 主菜单入口复验

日期：2026-08-16  
结论：**PARTIAL**

## 本轮事实

本轮使用已构建的 `D19_R03_A01_Player_RERUN7.exe`，先以窗口化、再以 1920×1080 全屏参数启动真实 Windows Player。Unity 日志证明 `MainMenu`、`EventSystem`、`InputSystemUIInputModule` 和主菜单按钮初始化完成；主菜单画面真实可见。

本轮没有进入 `InitialHarbor`。系统安全中心的防火墙提示覆盖 Player，并把鼠标点击报告为 `PickerHost.exe` 所有；Return、Tab、Space 和 Return 没有触发“新游戏”。没有点击、允许、取消或关闭该系统提示。证据截图为：

`game/tools/results/D19/D19-R03-A01/screenshots/mainmenu_rerun7_system_blocker.jpg`

日志为：

- `game/tools/results/D19/D19-R03-A01/D19_R03_A01_MAINMENU_ROUTE_RERUN7_PLAYER.log`
- `game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN7_FULLSCREEN_PLAYER.log`

机器记录：

- `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN7_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN7_ROUTE_LOG.json`

## 门禁结论

| 门槛 | 结果 | 说明 |
|---|---|---|
| 真实 Player 启动 | PASS | Unity Player 进程启动，主菜单 UI 初始化日志存在 |
| 主菜单显示 | PASS | 截图可复核；截图同时记录了未触碰的系统提示 |
| 真实新游戏输入 | BLOCKED | 系统浮层占用窗口焦点，按钮未被触发 |
| InitialHarbor 路线 | NOT_RUN | 未进入场景 |
| 5—10 分钟路线 | NOT_RUN | 无法从主菜单开始 |
| 交互/登船/出航/返港 | NOT_RUN | 无法从主菜单开始 |
| 保存/退出/重载恢复 | NOT_RUN | 无法从主菜单开始 |

因此本轮只能作为环境级取证阻断，不能升级 `D19-R03-A01` 或 `D19-R03-S01`。历史 RERUN4、RERUN6 的真实 PARTIAL 结果继续保留并仍是当前 D19 事实的一部分。

## 责任与下一步

在不触碰系统安全提示的前提下，需要重新获得可交互的 Player 窗口后，才可继续真实路线取证；重新取证必须产生新的路线 JSON、9+ 全屏截图、保存/重载事实和日志。D19 视觉固定镜头/天气证据与 R03-A02 的平面/块状视觉债务仍未闭合，不能由本轮主菜单截图替代。
