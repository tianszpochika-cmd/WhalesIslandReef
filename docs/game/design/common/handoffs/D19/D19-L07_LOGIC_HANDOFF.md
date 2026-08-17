# D19-L07 交接报告

## 任务信息与结论

- 任务编号：`D19-L07`（精确前置：D19-L03、L05、L06、L17、L23、L24 均 LOGIC_PASS，已全部满足）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L07/D19_L07_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19DockingRules.cs`（新建，纯 C#、零 Unity 依赖）
- `game/tools/results/D19/D19-L07/D19_L07_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L07_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）
- `docs/game/LOGIC_TASK_STATUS.md`（仅由 sync 脚本写入）

未修改生命周期（L02/L03）、泊位（L06）、权限（L23）、失败码（L17）、本地化键（L18）、锚点（L04）、事件（L16）或其它任何所有者文件；未启动 Player。

## 公开接口（`Whalebone.Systems.D19.D19DockingRules`）

**输入事实**（`D19DockingRequest`，调用方明确传入，本系统不读取运行时状态）：

| 字段 | 来源 |
|---|---|
| CommandId / ShipId / ActorId / PortId / BerthId | 命令与身份事实 |
| ShipProfile（sizeTier / deepDraft） | L06 `D19BerthShipProfile` |
| VisitState（State 快照） | L03 `D19PortVisitSystem.State` 只读 |
| RequestedBerthAllocation（目标泊位分配） | L06 `GetAllocation`（null=空闲） |
| ShipCurrentAllocation（本船当前泊位） | L06 `GetAllocations`（null=无事实） |
| FacilityAvailability（设施可用性裁决） | L05 `Adjudicate` 结果（null=跳过因子） |
| AuthorityClaims（授权声明） | L23 输入约定（null=跳过因子） |

**裁决管道**（`Adjudicate`，首个失败即返回，顺序固定）：

1. 命令 ID（L24）→ 缺失/非法 → `d19.lifecycle.invalid_transition`
2. 港口/泊位归属（L24 封装 L04）→ 未知港/跨港泊位/键格式非法
3. 港口访问状态（L03）：仅 **Docked / Returned** 允许；None/Departed→`not_in_port`、PortVisit→`not_moored`、Berthed/InService→`ship_already_berthed`
4. 船只逻辑状态：本船已持泊位 → `ship_already_berthed`；同一泊位 → `duplicate_request`（**NoRetry**）
5. 服务可用性（L05 结果）：非 Available → 透传其 L17 码/key/重试 + `Detail` 蛇形子原因（`facility_closed`/`blockaded`/…）
6. 权限（L23）：拒绝 → `d19.port.permission_denied` + `authority_denied.<cause>`（cross_player / cross_port / …）
7. 船型能力（L06 能力表）：`capability_missing` / `ship_tier_incompatible` / `draft_incompatible`（全部 `d19.berth.occupied`）
8. 泊位状态（L06 分配）：他人占用→`occupied`、他人预留→`reserved_by_other`、本人持有→`duplicate_request`
9. 状态组合一致性（L24：目标 Berthed 持本港泊位）

**执行入口**（`AdjudicateReservation`）：裁决通过 → 消费 L06 `Reserve` 命令（幂等/事件由 L06 负责）；裁决未通过 → `ReserveResult=null`，**L06 未被调用、泊位状态不变**。`ReadAllocations` 从 L06 只读接口补齐分配事实。

## 关键口径

- 每个拒绝 = `D19DockingVerdict { Allowed=false, BusinessCode(L17), LocKey(L18), Retry(L17 条目), ReasonId(精确蛇形子原因), CanonicalPortId/BerthId }`。
- **失败保持所有状态不变**（实测：拒绝路径 `ReserveResult=null`、泊位系统分配为空、L06 不变式全程成立）。
- **靠泊不是仅凭一个 bool**：船型不兼容、无访问、无权、已占用、服务关闭、非法状态、重复请求七类均有独立稳定结果。
- 不实现物理碰撞/距离/Transform/NavMesh/自动移动；唯一写路径是裁决通过后消费 L06 的 Reserve 命令（泊位状态唯一写者仍是 L06）。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `dotnet build`（临时工程，链接真实 D19 11 文件 + EventContracts.cs） | PASS：0 警告 / 0 错误 |
| 临时控制台断言 30 项 | PASS：30/30（七类拒绝稳定结果 + 允许路径 + 确定性 + L06 消费/未消费边界 + 只读 + 拒绝必带 L17 码/L18 key） |
| 静态扫描 | PASS：0 处 Unity/物理/移动实现 |
| `python tools/sync_logic_task_status.py` | PASS：D19-L07=LOGIC_PASS |
| `git diff --check` | PASS：无空白错误，仅既有 LF→CRLF 警告 |

## 明确未宣称

- 未占用/预留任何真实泊位（L06 状态只在本系统临时测试实例中变更）。
- 未实现物理靠泊、碰撞、距离或自动移动。
- 未裁决资金/库存/船员事实（这些由各自所有者系统提供，本系统只消费输入）。

## 待办 / 后续注意事项

- L08（登船工作流）与 L33（命令路由）可调用 `Adjudicate`/`AdjudicateReservation` 复用同一靠泊语义。
- L06 落地占用/租期事实后，若需在裁决中纳入租期过期（`d19.berth.expired`），先确认 L17 目录条目再扩展管道。
- 显示文案由本地化任务按 `verdict.LocKey` 补充。
