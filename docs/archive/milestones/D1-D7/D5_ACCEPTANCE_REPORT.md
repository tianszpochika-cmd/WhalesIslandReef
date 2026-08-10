# D5 最终验收报告：打磨上架技术阶段

> 验收日期：2026-08-09
> 范围：D5-01 至 D5-10；技术结论：**通过**
> 外部发布动作：Steam Partner 配置、付款、审核和运营未自动执行，见 `D5_RELEASE_CHECKLIST.md`。

## 1. 验收结论

D5 已把 D4 灰盒可玩环扩充为可展示、可验证、具备发布技术桥接的版本：

`12 岛海图 → 发现/访问 → 50 条特质 → 基础角色预设 → 中英切换 → 1080p/60 目标监控 → 成就/云存档队列 → 发布清单`

没有 Steamworks SDK 或真实 App ID 时，游戏仍能离线启动、操作和保存，不把外部服务故障变成游戏卡死。

## 2. 功能清单

| 领域 | 完成内容 | 证据 |
|---|---|---|
| 世界 | 12 个岛屿、5 种类型、5 种加勒比生境、发现/访问状态 | `D5IslandCatalog`、`D5WorldSystem` |
| 特质 | 50 条：身体 15、性格 20、经历 10、疾病 5；确定性生成 3-4 条 | `D5TraitCatalog`、`D5TraitSystem` |
| 捏脸 | 8 个基础角色预设；皮肤、发型、发色、服装、体型参数 | `D5CharacterCatalog`、`D5CharacterSystem` |
| 本地化 | D5 UI、岛屿、特质、角色预设全部中英双语 | `D5LocalizationSystem` |
| 性能 | 1920×1080、60 FPS 目标；低/中/高档位；运行时帧采样和达标报告 | `D5PerformanceSystem`、`D5RuntimeBootstrap` |
| Steam | 8 个成就；离线安全 Bridge；云存档待上传队列 | `D5SteamSystem`、D5 HUD |
| 存档 | D5 快照写入 `location_json.d5`；旧 D1-D4 存档兼容 | `SaveSystem`、`D5SaveSystemTests` |
| Unity | InitialHarbor 海图、12 个岛标记、角色预设切换、语言/画质/成就/保存 | `D5RuntimeBootstrap` |
| Server | 世界、特质、捏脸、性能、Steam 云状态契约镜像 | `server/src/modules/d5` |
| 发布资料 | 商店元数据和人工发布清单 | `D5ReleaseCatalog`、`D5_RELEASE_CHECKLIST.md` |

## 3. 游戏内操作

进入 `InitialHarbor` 后：

- `I`：发现下一个岛；`V`：访问当前岛；海图上的已发现标记也可点击。
- `P`：循环角色预设；`L`：中英切换；`F1`：循环低/中/高画质。
- `F2`：解锁当前可用的演示成就；`F5`：保存 D5 状态并加入云存档待同步队列。
- D2/D3/D4 的移动、镜头、贸易、炮战、动物、医疗、主线和保存快捷键保持兼容。

## 4. 自动化证据

| 检查 | 结果 |
|---|---:|
| Unity 6 D2+D3+D4+D5+Save EditMode | **63/63 通过，0 失败，0 跳过** |
| D5 存档回读 | 通过：世界、语言、特质、成就和云队列均恢复 |
| D5 Unity 运行时冒烟 | 通过：主 HUD、按钮可点击射线、旧 HUD 隐藏、D5WorldMap、12 个岛标记和玩家对象均创建 |
| AgentCore `dotnet build` | 通过：0 警告、0 错误 |
| AgentCore Mock 自检 | 通过：全部自测通过 |
| Server `npm run build` | 通过 |
| D5 JSON Schema/OpenAPI 检查 | 通过 |
| Systems 网络/LLM 边界扫描 | 通过 |
| `git diff --check` | 通过 |

Unity 测试和冒烟均使用临时副本，不修改工作区的 `Library/`、`Temp/` 和玩家存档。

## 5. D5 后 UI 稳定性修复（2026-08-09）

用户验收后发现 InitialHarbor 同时显示 D2/D3/D4/D5 四套运行时 HUD，造成文字和按钮重叠；D5 按钮的文字还会拦截鼠标射线，导致点击文字区域没有回调。

- 新增 `RuntimeHudCoordinator`：D5 作为主 HUD，D2/D3/D4 旧说明层默认隐藏且不拦截鼠标；进入 D3 贸易面板时临时显示独立贸易层，关闭后恢复 D5 HUD。
- D5、D3、D4、暂停菜单和对话按钮统一处理文字射线与中英刷新；暂停 Canvas 提升到 `sortingOrder=500`，不会被游戏 HUD 盖住。
- D4/D5 脚本热重载后会自动重建缺失的运行时系统引用，避免空引用异常让输入看起来失效。

验证：Unity 6 工程编译 0 警告/0 错误；临时副本 D5 运行时冒烟通过；D2/D3/D4/D5/Save EditMode 63/63 通过。

## 6. 玩家外观与上船交互修正（2026-08-09）

- 修复 D2 玩家默认显示 Capsule 的问题：有场景船员网格时复制 `ph_crew_male` 为玩家外观，只有素材缺失才降级为 Capsule。
- 修复“按 E 只切模式、人物不在船上”的问题：登船后玩家站到甲板并跟随船体，下船解除绑定；D5 右侧 HUD 增加“上船/下船 [E]”按钮。
- Unity 6 临时副本运行时冒烟通过：角色模型识别、上船/下船父子关系、D5 HUD 可点击性和既有 12 岛 UI 均通过；`dotnet build game/game.sln` 0 警告/0 错误。

## 7. 已知边界

- 当前低多边形和运行时占位资源用于验证内容密度与交互，最终商店 Capsule、宣传图、预告片仍需制作。
- 1920×1080/60 是目标配置和采样门；实际不同硬件的最终 FPS 需要在目标机器上做长时间 Profile。
- Steam 成就/云存档桥接在无 SDK 时使用离线安全模式；真实 App ID、SDK、Partner 账号、付款和商店审核是外部发布动作。
- 愿望单运营、Next Fest 报名、税务/银行资料和 EA 发布不能由本地代码代办。

## 8. DoD 对照

- [x] D5 范围、契约和数据所有权已记录。
- [x] 12 岛、50 特质、8 预设、本地化和性能系统已完成并测试。
- [x] Unity 海图、12 标记、预设切换和发布状态面板已接入。
- [x] InitialHarbor HUD 单一显示策略、按钮点击射线和中英按钮刷新已验证。
- [x] D5 快照保存/读取及旧存档兼容已完成。
- [x] Server 契约镜像构建通过。
- [x] 63/63 EditMode 与 D5 运行时冒烟通过。
- [x] 文档、路线图和当日 memory 已更新。
- [x] Systems 无网络/LLM 依赖；外部发布边界已明确。
- [x] 本地 `master` 提交前工作区检查完成。
