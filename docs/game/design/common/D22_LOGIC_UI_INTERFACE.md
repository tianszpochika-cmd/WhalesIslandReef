# D22 纯逻辑 UI 接口（D22-L26—L32）

> 版本：D22-L32，2026-09-01（Asia/Shanghai）
> 范围：D22 逻辑命令、反馈、只读 ViewModel 组合和 Save/Load 投影。
> 本文不发布 Canvas、UGUI、EventSystem、Input、Prefab、场景、Player 或设备接口。

## 1. 总体方向

```text
Runtime 输入适配器
    -> D22UiCommandRequest
    -> D22UiCommandValidator
    -> D22UiCommandRouter
    -> 注入的公开 owner handler
    -> owner 回执 + D22FeedbackRecord

owner/D22 snapshot
    -> typed read-model adapter 或 D22ViewModelSourceRegistration
    -> D22ViewModelComposer
    -> D22ViewModel
    -> Runtime view adapter
```

D22 只拥有命令生命周期、面板/焦点/暂停/通知的逻辑投影和反馈关联。市场价格、金币、库存、港口访问、船况/航程、NPC、任务、Save 根快照等事实仍由所属 owner 写入。D22 不接受从按钮可见、动画、Canvas 状态、固定 token、缓存数值或本地乐观值推导出的业务成功。

## 2. 命令信封

`D22UiCommandRequest` 的稳定字段为：

| 字段 | 规则 |
|---|---|
| `CommandId` | 跨重试不变；稳定 ASCII ID；用于幂等主键 |
| `CorrelationId` | 本次交互/owner 回执关联 ID |
| `ActionId` / `Kind` | 必须与 `D22UiCommandContracts` 定义一致 |
| `ActorId` | 行为者稳定 ID |
| `ScreenId` / `PanelId` | 已登记 D22 screen/panel 逻辑 ID |
| `TargetId` | 对港口、市场、库存、船舶、NPC、任务等命令必填 |
| `WorldDay` / `SaveRevision` | 必须与 D22 上下文一致；D22 不推进世界时间 |
| `OwnerSystem` | 可为空；非空时必须与命令定义一致，不能借此替换 owner |
| `FocusedControlId` | 可选逻辑焦点 token；不是 UGUI 选择对象 |
| `Parameters` | 唯一键、稳定标量值；具体业务语义由 owner 解释 |
| `CauseEventIds` | 可选、唯一稳定事实事件 ID |

命令种类及默认公开 owner：

| `Kind` | `ActionId` | owner 方向 |
|---|---|---|
| `OpenPanel` / `ClosePanel` | `ui.open_panel` / `ui.close_panel` | D22 面板逻辑处理入口；Runtime 仍需显式接线 |
| `NewGame` | `ui.new_game` | `Core/Progression` |
| `ContinueGame` / `SaveGame` / `LoadGame` | `ui.continue_game` / `ui.save_game` / `ui.load_game` | `SaveSystem` |
| `Pause` / `Resume` | `ui.pause` / `ui.resume` | `PauseOwner`；D22 只输出意图/读模型 |
| `PortAction` | `ui.port_action` | `D19PortCommandRouter` |
| `MarketAction` | `ui.market_action` | `Economy/Market` |
| `InventoryAction` | `ui.inventory_action` | `Inventory/Cargo` |
| `ShipAction` | `ui.ship_action` | `Ship/Voyage` |
| `CrewAction` / `DialogueAction` | `ui.crew_action` / `ui.dialogue_action` | `Character/Crew` |
| `QuestAction` | `ui.quest_action` | `Quest/Event` |
| `AccessibilityUpdate` | `ui.accessibility_update` | `D16AccessibilitySettingsStore` |
| `AcknowledgeFeedback` | `ui.acknowledge_feedback` | D22 notification/feedback owner |
| `RetryOwnerCommand` / `CancelOwnerCommand` | `ui.retry_owner_command` / `ui.cancel_owner_command` | D22 只转发，不替 owner 执行 |

## 3. Router 接线规则

入口为：

```csharp
var router = new D22UiCommandRouter();
router.Register("D19PortCommandRouter", request =>
    D22UiOwnerCommandResult.Succeeded("D19PortCommandRouter", "port.accepted"));

D22UiCommandResult result = router.Route(request, context);
D22UiCommandRouteResult detailed = router.RouteDetailed(request, context);
```

`Register` 保存的是 owner 标识和公开 delegate，不保存 owner 实例、私有字段、可变业务字典或 Unity 对象。handler 收到 `request.Clone()`，不得把 D22 命令当作事实写入授权。

Router 按以下顺序工作：

1. 克隆调用方上下文，并把自己的已处理命令投影加入只读校验上下文；不修改调用方 `context`。
2. 调用 `D22UiCommandValidator.Validate`，拒绝 shape、权限、焦点、上下文、修订、冷却或禁用错误。
3. 本地校验失败不调用 owner；同内容 `CommandId + fingerprint` 返回 `Duplicate`，同 ID 不同 fingerprint 返回 `Rejected`。
4. 校验通过后按命令定义查找精确 owner。缺 handler、null 回执、owner schema/command/correlation/owner ID 不匹配或 handler 异常均返回 `Blocked`，不会返回业务成功。
5. 只把结构化 owner 回执映射为 `D22UiCommandResult` 与 `D22FeedbackRecord`。owner 的 `OwnerResultCode`、`ReasonCode`、`DisabledReasonCode` 和 `BusinessStateChanged` 原样保留；反馈码为空时按生命周期使用稳定默认码。
6. 只有通过本地校验并完成 owner 调用的命令才进入 UI 幂等缓存。缓存是 D22 投影，不是 SaveSystem 根存档，也不包含市场/库存/船舶/NPC/任务事实。

缺 owner 的典型结果：

```text
Outcome            = Blocked
Accepted           = false
BusinessStateChanged = false
FeedbackCode       = d22.failure.owner_unavailable
DisabledReasonCode = d22.disabled.owner_unavailable
ReasonCode         = d22.router.owner_unavailable
```

## 4. Router UI 存档投影

`CreateSaveSnapshot()` / `Snapshot()` 只返回 `D22UiCommandRouterSnapshot`，其中每一项包含 `CommandId`、请求 fingerprint、`CorrelationId` 和已复制的 D22 结果。`Restore(snapshot)` 先校验版本、稳定 ID、结果 ID 一致性、结果 schema 和重复主键，再原子替换 router 的 UI 幂等索引；坏快照不会清空当前索引。

调用方向必须是：

```text
SaveSystem 读取/写入根存档
    -> 合并/恢复 D22UiCommandRouterSnapshot
    -> D22 router 仅恢复自身 UI 幂等投影
```

Router 不直接调用 SaveSystem，也不把 `Restore` 当作领域事实恢复成功。

## 5. ViewModel 组合接口

`D22ViewModelCompositionInput` 只接受以下已存在 D22 公开快照的防御性输入：

- `D22PanelTransitionSnapshot`
- `D22FocusSnapshot`
- `D22PauseReadModel`
- `D22NotificationQueueSnapshot`
- `D22LoadStateReadModel`
- `D22AccessibilityReadModel`
- `ID22ViewModelSourceAdapter` 列表

上游 adapter 通过以下最小只读边界接入：

```csharp
public interface ID22ViewModelSourceAdapter
{
    string ModuleId { get; }
    int ExpectedSchemaVersion { get; }
    D22ViewModelSourceSnapshot ReadSnapshot();
}
```

`D22ViewModelSourceSnapshot` 只包含 module ID、schema、owner、可用性、reason/localization/fingerprint 和稳定标量字段。`D22ViewModelSourceRegistration.FromSnapshot` 适用于已经由上游公开接口产生的快照；它不是业务状态缓存。

必需模块的固定输出顺序为：

```text
module.market
module.port
module.ship
module.inventory
module.crew
module.quest
module.save
```

模块重复、未知、缺失、adapter 抛错、null 快照、`Available=false` 或 schema mismatch 都保留为 `D22ViewModelModuleState`，不会退回固定/本地伪快照：

| 状态 | 稳定 reason | localization key |
|---|---|---|
| 缺模块 | `d22.viewmodel.module_missing` | `ui.d22.viewmodel.module_missing` |
| adapter 不可用 | `d22.viewmodel.adapter_unavailable` | `ui.d22.viewmodel.adapter_unavailable` |
| adapter 失败 | `d22.viewmodel.adapter_failed` | `ui.d22.viewmodel.adapter_failed` |
| 版本不匹配 | `d22.viewmodel.version_mismatch` | `ui.d22.viewmodel.version_mismatch` |
| module ID 不匹配 | `d22.viewmodel.module_id_mismatch` | `ui.d22.viewmodel.module_id_mismatch` |
| 重复/未知 | `d22.viewmodel.duplicate_module` / `d22.viewmodel.unknown_module` | 对应 `ui.d22.viewmodel.*` |

`D22ViewModel.IsDegraded=true` 表示组合结果可读但依赖不完整；`ReasonCodes` 与 `LocalizationKeys` 按稳定顺序保留。`CompositionFingerprint` 覆盖 D22 快照、模块状态和字段顺序，便于 Save/Load 等价性检查。`IsDegraded` 不能被 Runtime 当作 owner 失败或业务成功。

当前 D21-L01—L32 已有 `LOGIC_PASS` 结果与章节验收文档；船舶模块仍必须只消费 D21 的公开 ship/voyage read model，不能由 D22 发明 D21 私有接口或用本地组合成功冒充航程/船况成功。若公开 adapter 缺失、版本不匹配或编译不可用，继续输出 `Blocked`/缺失原因。

## 6. Feedback 与本地化

反馈只传稳定码和 key，最终文案由 presentation edge 解析。主要路由码：

- 成功：`d22.success.command_accepted`、`d22.success.command_completed`、`d22.success.panel_opened`、`d22.success.panel_closed`
- 校验失败：`d22.failure.invalid_request`、`d22.failure.context_mismatch`、`d22.failure.focus_mismatch`、`d22.failure.revision_mismatch`、`d22.failure.disabled`
- owner：`d22.failure.owner_rejected`、`d22.failure.owner_unavailable`、`d22.warning.owner_pending`
- 幂等：`d22.warning.duplicate_command`、`d22.failure.duplicate_conflict`

`ReasonCode`、owner failure code 和 `DisabledReasonCode` 是机器字段，不得直接当作用户可见文案。缺 key 需要由本地化审计/Runtime QA 报告，不允许默默显示 reason code。

## 7. Runtime Deferred 与 D23 接线边界

D23 可以消费：

- `D22UiCommandRequest`、`D22UiCommandResult`、`D22UiCommandRouteResult`；
- `D22FeedbackRecord` 的 code/category/retry/localization；
- `D22ViewModel` 的 D22 状态快照、固定模块序列、缺失/版本原因和 fingerprint；
- `D22ViewModelModuleState.Fields` 中由已接受 adapter 提供的稳定标量投影。

D23/Runtime 不得：

- 从 `D22ViewModel` 反写 owner 事实或以 `BusinessStateChanged` 之外的 UI 状态推断成功；
- 直接构造 D19/D20/D21/经济/库存/任务/Save 私有状态；
- 在 Systems/D22 中加入 Canvas、EventSystem、Input、Unity UI、Time、网络、LLM、墙钟或不可控随机；
- 把 `D22ViewModelComposer` 的缺模块降级成空数据并隐藏 reason；
- 把命令 `Blocked`、`Rejected`、`Pending` 显示为业务成功。

以下工作保持 Runtime Deferred：真实按钮和点击、UGUI/EventSystem 选择、键鼠手柄/触摸、Canvas 布局、字幕/通知视觉、分辨率、动画、音频、Player/PlayMode、性能、目标设备和发布包。
