# D1 合流代码审核报告（统筹审核）

> 时间：2026-08-08 ｜ 审核人：统筹（全局代码审核，用户 GUI 打开前）
> 范围：P1A(24bddad) / P1B(8f370df) / P1C(94801e1) 三线全部代码 + 场景 + 契约

## 审核结论：✅ 通过（1 处必修显示 bug 已修，可进入用户 GUI 验证）

## 绿灯清单（静态审核通过项）

| 检查项 | 结果 |
|---|---|
| asmdef 引用方向 | ✅ Systems→Agent.Runtime 单向；Tests→Systems+Agent；autoReferenced=true（P1A 默认程序集可引用） |
| 命名空间冲突 | ✅ 唯一重名 GameState（Core 枚举 vs Save 静态类）已用命名空间隔离，P1A 无引用歧义 |
| 契约三方对齐 | ✅ SaveSystem 4 方法 + NewGameParams 签名 = P0 契约；P1A 只调用、P1B 只实现 |
| 强逻辑红线 | ✅ SaveSystem 全文件零网络/LLM；SqliteNative 仅 DllImport 本地库 |
| 场景 GUID 引用 | ✅ InitialHarbor 引用的 CameraFollow/字体/2 个 Prefab guid 与 .meta 全部一致；MainMenu 零脚本引用（纯代码构建，天然免疫 guid 失配） |
| SQLite 方案 | ✅ 官方 sqlite3.dll(x64 3.53.4) + 自写 P/Invoke + 自写 JSON；44 断言离线自测过 + net48 真编译 0 错 0 警 |
| 本地化 | ✅ 主菜单/新游戏/设置/关于全部 key 中英双表登记；IsEnglish 存在 |
| 设置持久化 | ✅ SettingsStore wb_* 键名契约；ApplyAll 应用音量/画质 |
| GameManager 实例化 | ✅ MainMenuController.AutoBootstrap（RuntimeInitializeOnLoadMethod）自动创建单例，无 NRE 风险 |
| 存档表结构 | ✅ 与 DATA_MODELS §7 完全一致（5 表 + user_version=1 + 种子幂等） |

## 发现并已修复（提交 fix(D1)）

1. **【显示级 bug · 已修】主菜单中文豆腐**：`Fonts.Default` 原用内置 LegacyRuntime.ttf（无中文字形），中文 UI 会显示方块。
   - 修复：`ph_font_noto_sans_cjk_sc.otf` 移入 `Assets/Resources/Fonts/`（.meta 随行，GUID fc6186fe... 不变 → InitialHarbor 场景引用不受影响）；`Fonts.Default` 优先 `Resources.Load` Noto，失败兜底 LegacyRuntime。
2. **【流程 · 已修】P1A 的 5 个 .cs meta + UI 目录 meta 未提交**（P1B/P1C 的 meta 已入库）。已 git add 固定 GUID（未提交 meta 虽暂未被引用，但必须入库：未来场景引用脚本时依赖固定 guid）。

## 残余风险（不阻塞，记录观察）

| 项 | 说明 |
|---|---|
| UIPanel.cs 未被引用 | P1A 交付的基类当前无使用者，死代码（不产生编译错误），D2 用到时激活 |
| AgentMemory 不还原 | P1B 已知限制（memory_json 骨架），D2 对话系统实现时补 |
| 特质 id 桥接 | GameState.CrewAgentTraitIds 临时桥接，AgentCore Persona.Traits 扩展后迁移 |
| batchmode License | 本机 Unity 批处理模式 License 校验不稳定（Code 10），只影响自动化，不影响用户 GUI |
| Noto 字体进包 | 16MB 占位字体随包（正式版换更优方案即可） |

## 用户 GUI 验证（打开 Unity 后最小操作）

按 `docs/game/prompts/D1/08_用户GUI验证清单.md` 执行，重点顺序：
1. 首次导入 → Console 0 errors（如有报错截图回传，本审核已排除大部分静态风险）
2. Test Runner → EditMode → Run All（预期 8 绿）
3. MainMenu Play → 四项交互（中文应正常显示——字体已修）
4. InitialHarbor → 港口/船/老水手/中文标签
5. 全链路：新游戏→港口→重启→继续
