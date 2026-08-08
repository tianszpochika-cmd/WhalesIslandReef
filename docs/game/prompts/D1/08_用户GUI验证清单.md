# D1 集成验证清单（用户 GUI 操作 · 一次性）

> 用途：三线（P1A/P1B/P1C）代码已完成，但都未经真实 Unity 编译/运行验证（batchmode License 故障）。
> 本清单是 D1 能否进入 P2 的**闸门**。全程约 15-25 分钟，按序执行。

## 前置

- [ ] Unity 6（6000.0.80f1）已安装；打开 Unity Hub → 确认已登录 + **个人许可已激活**（Preferences → Licenses → Personal，免费）
- [ ] 打开方式：Unity Hub → Open → 选 `C:\Users\Administrator\Desktop\game\first01\game`

## 步骤 1：首次导入（最关键）

- [ ] 打开项目 → 等待首次导入完成（首次会生成全部 .meta、ProjectSettings.asset；**Library 若报异常直接重启编辑器**，会自动重建）
- [ ] 观察 **Console**（Window → General → Console）：
  - 预期：`0 errors`（警告可接受）
  - 若报错：**截图全部错误** → 贴给我，我指派对应模块修复（不要自己改代码）
  - 常见预期错误点：P1A 的 UGUI 代码真实编译、P1B 的 sqlite3.dll P/Invoke 平台校验、P1C 手写 YAML 场景引用

## 步骤 2：EditMode 单测（P1B 闸门）

- [ ] Window → General → Test Runner → **EditMode** 页 → Run All
- [ ] 预期：8 个用例全绿（SaveSystemTests：NewGame/Save/Load 还原/种子/幂等/覆盖旧档）
- [ ] 失败：贴 Test Runner 报错给我

## 步骤 3：主菜单 Play 验证（P1A 闸门）

- [ ] 双击 `Assets/Scenes/MainMenu.unity` → 点 Play
- [ ] 检查四项：
  1. **新游戏**：点开 → 输入船长名 → 确认 → 不报错（桩实现已由 P1B 真实实现替换）
  2. **继续**：无存档时应置灰 + 提示"没有存档"；新游戏后重启再进应可点
  3. **设置**：音量滑块/画质/语言切换即时生效；重启后保持（PlayerPrefs）
  4. **关于**：显示 v0.1.0-D1
- [ ] 异常：截图 → 贴给我

## 步骤 4：初始环境验证（P1C 闸门）

- [ ] 停止 Play → 双击 `Assets/Scenes/InitialHarbor.unity` → 点 Play
- [ ] 检查：海面/沙滩/码头可见；**小渔船停靠码头、老水手站船头**；相机跟随船（右键旋转/滚轮缩放）；中文标签"圣玛格丽特港 / Sainte-Marguerite · 1675"
- [ ] 若场景纯灰（材质未变彩色）：可运行菜单 `Tools > Whalebone > Build InitialHarbor` 重建彩色版（可选）

## 步骤 5：全链路手测（P2 的前半，可在 GUI 顺手完成）

- [ ] 从 MainMenu 开始：新游戏（船长名"测试船长"）→ 自动切到 InitialHarbor（SaveSystem.NewGame 真实执行）
- [ ] 退出编辑器 → 重新打开 → 继续按钮可用 → 点击 → 回到港口（Load 生效）

## 完成标准

步骤 1-5 全过后：**回报我 ① Console 错误数 ② 单测结果 ③ 各步骤是否通过**。
我将据此：判定 D1 进入 P2 集成验收（打包冒烟），或指派修复。

> 若中途任何一步卡住：**不要跳过、不要自己改代码**——截图贴给我，由统筹指派到对应模块（P1A/P1B/P1C）。
