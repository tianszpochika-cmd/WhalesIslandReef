# D19-R02-A01 玩家 HUD 焦点修复

## 结论

本任务结论为 **PARTIAL**。`RuntimeHudCoordinator` 与 `D19RouteHud` 已将玩家模式收拢为一个主 HUD，并在真实 Windows Player 中确认旧 D9/D10/D7 面板不再叠加；专用 EditMode 3/3、专用 PlayMode 1/1 通过。任务提示词要求的真实移动、港口操作、暂停/恢复、输入切换和失败反馈尚未全部通过真实 Player 输入取证，因此不能写 PASS。

历史 D19-S02、D19-S02-R01 以及本轮之前的 D19 R02 结果保持不变。场景中的灰盒视觉债务不属于本任务写入范围，交给 D19-R02-A04。

## 实际改动

- `RuntimeHudCoordinator` 现在默认只显示 `D19RouteHud`，D2/D3 仅在临时焦点请求时显示；D5/D6/D7/D9/D10 旧面板默认隐藏。
- 保留旧 Canvas 名称和稳定玩法入口，不删除兼容节点，不写入交易、航程、存档或任务事实。
- `D19RouteHud` 从屏幕中央移到底部中心，避免盖住港口路线；反馈行没有事件时保持空，不制造成功/失败文本。
- `D19RouteHudManager` 不再从事件日志推断或伪造成功/失败结果；真正反馈仍由 D2/D3 所属系统提供。
- 新增 3 个 EditMode 和 1 个 PlayMode 专用测试。

## 焦点策略

机器可读接口见 `game/tools/results/D19/D19-R02-A01/D19_R02_A01_HUD_ROUTE_INTERFACE.json`。默认策略如下：

| 层 | 默认状态 | 责任 |
|---|---|---|
| `D19RouteHud` | 显示、只读 | A01 |
| `D2HintCanvas` | 隐藏，临时 D2 焦点才显示 | D2 |
| `D3Canvas` | 隐藏，临时 D3 焦点才显示 | D3 |
| D5/D6/D7/D9/D10 | 隐藏 | 各原系统保留入口 |

HUD 只读取状态；UI、Agent 和 HUD 协调器都不是交易、航程、关系或存档事实的写者。

## 真实 Player 证据

真实 Player 已完成：主菜单 → New Game → 创建航程 → InitialHarbor 加载 → HUD 清理观察。修复前截图可见 D9 左上、D7 右上、D10 左上、D3 侧栏和 D19 中央面板重叠；修复后截图只保留底部 D19 主 HUD：

- 修复前：`game/tools/results/D19/D19-R02-A01/screenshots/00_before_hud_fix_player.png`
- 修复后：`game/tools/results/D19/D19-R02-A01/screenshots/05_harbor_hud_clean_player.png`

Player 构建目标为 1920×1080，但本轮桌面窗口采集文件为 2194×1234，未将其冒充为精确 1920×1080 backbuffer 证据。Escape 未形成可定位的暂停状态变更日志，因此暂停/恢复保持未验证。

## 自动化验证

```text
Unity 6000.0.80f1 EditMode Save.Tests / Whalebone.Tests.D19Hud.D19R02A01HudTests: 3/3 PASS
  game/tools/results/D19/D19-R02-A01/D19_R02_A01_EDITMODE_TESTS_FINAL2.xml

Unity 6000.0.80f1 PlayMode D16.PlayMode.Tests / Whalebone.Tests.D19Hud.D19R02A01HudPlayModeTests: 1/1 PASS
  game/tools/results/D19/D19-R02-A01/D19_R02_A01_PLAYMODE_TESTS_FINAL.xml

Unity Assembly-CSharp compile: PASS
Windows Player build: PASS
git diff --check: 待本轮收尾命令复核
```

## 未闭合事项

1. 真实 Player 5—10 分钟路线、一次港口操作、暂停/恢复、鼠标/键盘切换和失败反馈需要由 A02/A03 的公开入口完成，并在后续 S02 统一复验。
2. 精确 1920×1080 Low/Medium/High 性能数据不属于 A01 已完成证据；由 A04/S02 采样。
3. InitialHarbor 画面仍有灰盒/低细节债务；A01 未越权修改场景，责任交给 A04。

详细机器结果见 `game/tools/results/D19/D19-R02-A01/D19_R02_A01_MACHINE_RESULT.json`。

## RERUN1 玩家文案复验（2026-08-15）

保留原始 PARTIAL 结果和截图。本轮只在 A01 的 HUD 写者范围内把主 HUD 的内部调试前缀替换为玩家可读文案，并略微收紧底部面板：`D2 控制路由` → `WASD 移动，E 交互`，`D3 贸易/出航` → `港口航行/贸易`，不改变任何输入、系统状态或事实写者。

真实 x64 Player 重新从 Main Menu → New Game → 创建游戏 → InitialHarbor，截图 `game/tools/results/D19/D19-R02-A01/31_rerun1_harbor_hud.jpg` 显示旧 D2/D3/D9/D10 调试前缀不再出现在主 HUD；D19 专项 EditMode 3/3、PlayMode 1/1 和 Player build 均通过。截图读回为 1099×648 逻辑像素，不能冒充精确 1920×1080 backbuffer。

任务结论仍为 **PARTIAL**：A01 仍未独立完成完整 5—10 分钟真人路线、港口操作、暂停/恢复、输入切换和失败反馈；这些由 A02/A03/S02 继续取证。机器结果：`game/tools/results/D19/D19-R02-A01/D19_R02_A01_RERUN1_MACHINE_RESULT.json`。
