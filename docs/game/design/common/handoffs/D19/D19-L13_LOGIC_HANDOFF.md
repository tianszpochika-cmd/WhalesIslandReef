# D19-L13 交接报告

## 任务信息与结论

- 任务编号：`D19-L13`（波次 19.2，并行；前置 D19-L05/L17/L18/L23=LOGIC_PASS 全部满足）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L13/D19_L13_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19TavernPortAdapter.cs`（新建，唯一酒馆港口适配器）
- `game/tools/results/D19/D19-L13/D19_L13_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L13_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未修改任何 D19 既有文件（L02/L04/L05/L11/L16/L17/L18/L22/L23 等）、D9/D10/D3/D4/Save/Agent 源文件、UI 文案或本地化资源。

## 公开接口（供 L33 路由 / L19 读模型 / L20 保存 / L25—L32 测试消费）

- `D19TavernRequestKind`：Visit / Recruit / Social
- `D19TavernRequest`：CommandId/ActorId/ShipId/PortId/BerthId/ServiceId/WorldDay/SaveRevision + L05 输入（TimePeriod/FactionPermission/Weather/FacilityState/Staffed）+ AvailabilityGate（可选注入）+ CandidateId（Recruit）+ SocialRequest（D10TavernRequest，Social）
- `D19TavernResult`：Success/DuplicateSettlement/CommandId/ReasonCode（L17）/LocKey（L18）/RetryPolicy/OriginalOwnerCode（原系统码，永不吞掉）/Cost/CrewCount/MoneyAfter/SaveRevision/CandidateId/EventDraft（L16 工厂，未追加）/Note
- `D19TavernPortAdapter.Visit(state, ship, crew, preparation, request, claims)` / `Recruit(..., candidate, ...)` / `Social(tavern, request, claims)` / `Validate()`

## 执行管线（L11 市场适配器同族模式）

1. **输入校验**：接口不匹配（null/缺 command_id/缺 candidate_id/缺 social_request/服务种类不符）→ 明确阻断（Blocked），不调原系统；
2. **L23 权限**：Kind=StartService + 锚点服务键（service_<port>_tavern / service_<port>_recruitment），拒绝跨船/跨玩家/跨港/伪造服务 → L17 permission_denied；
3. **L05 可用性**：Visit/Social 用 "tavern"（L05 已知服务）；Recruit 的真实覆盖缺口（L05 不含 recruitment）不伪造结论，改由 D9 CheckService 原系统裁决并在 Note 标注；
4. **原系统裁决**：D9.VisitTavern / D9.Recruit（费用/入列/士气）或 D10.RequestAction（校验/敏感警告/事实追加）；活 state 透传，无缓存副本；
5. **投影**：D9/D10 真实失败码 → L17 条目（近似映射在 Note 标注，原码保留）；映射缺失 → owner_code_unmapped 明确阻断；事件草案由 L16 工厂构造（ServiceResult / 拒绝语义），追加与去重由 EventLog（event_id+save_revision）完成。

## 失败码投影表（真实码，无编造）

| 原系统码 | L17 码 | 说明 |
|---|---|---|
| not_enough_money | d19.trade.funds_insufficient | D9 货币裁决（费用失败） |
| crew_full | d19.inventory.capacity_exceeded | 近似：船员编制容量 |
| already_recruited / invalid_candidate / invalid_quantity | d19.inventory.operation_failed | 近似：操作失败 |
| invalid_state | d19.service.unavailable | 近似：原系统状态缺失 |
| invalid_port | d19.port.unknown_port | D9 港口裁决 |
| not_moored | d19.lifecycle.invalid_transition | 近似：生命周期前置未满足 |
| duplicate_settlement | d19.save.conflict | D9 幂等去重（DuplicateSettlement 透传） |
| warning_required | d19.port.permission_denied | 近似：敏感警告未确认=访问被拒 |
| D10 校验拒绝族（request_required/actor_required/port_required/negative_time_or_revision/unknown_action/unknown_sensitivity/invalid_cause_event/candidate_required/crew_required/invalid_rest_hours/invalid_alcohol_strength/rumor_not_found/poker_table_not_found/side_job_not_found） | d19.inventory.operation_failed | 近似：D10 请求校验拒绝 |

成功路径：tavern_request_accepted（D10）/ D9 Accepted → Success；duplicate_tavern_request / duplicate_settlement → Success + DuplicateSettlement=true；sensitive_skipped → Success + Note。

## 成功标准核对（全部可审计、无双重副作用）

| 场景 | 行为 | 证据 |
|---|---|---|
| 关闭服务 | L05 前置拦截，D9 不被调用 | 断言 trade.Money 不变 |
| 关系前置不足 | 请求层原系统当前不产生关系码；原码经投影/阻断保留 | 登记说明（D10.Resolve 阶段由 owners 裁决） |
| 权限不足 | L23 拒绝（跨玩家/跨港泊位） | 断言 permission_denied + Note |
| 费用失败 | D9 not_enough_money → L17 funds_insufficient | 断言无入列副作用、原码保留 |
| 底层拒绝 | crew_full / invalid_rest_hours 等 → L17 近似 | 断言原码保留、事件草案存在 |
| 重复命令 | D10 幂等去重透传 DuplicateSettlement | 断言二次调用不重复副作用 |

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| 前置核对 + 原系统真实码盘点 | PASS（真实码清单见结果 JSON） |
| 仓库外临时 dotnet 工程（真实源文件，LLM 层临时桩不落仓库） | PASS：编译 0 错误 0 警告；53 项确定性断言 0 失败 |
| `python tools/sync_logic_task_status.py` | PASS：D19-L13=LOGIC_PASS |
| `git diff --check` | PASS：exit 0（仅既有 LF→CRLF 警告） |

## 明确未宣称

- 未直接改关系、金钱、NPC 行为、模型或对话 UI（原系统裁决，本适配器零写路径）；
- 未创建授权、未修正归属、未查询网络/登录状态；
- 未修改任何 D19/D9/D10/D3/D4/Save/Agent 既有文件；
- 未授予任何 RuntimeApproved/ReleaseApproved。

## 待办 / 后续注意事项

- **跨任务发现（相邻模块，未越权修复）**：L11 市场适配器将 L18 服务 ID（"market"）直接传给 L23 权限校验，而 L23 以 L04 锚点目录解析服务键（service_<port>_market）→ L11 的权限前置会拒绝合法请求；本适配器先解析港口再构造锚点服务键传入。建议 L45/接口对齐时统一适配器家族的服务键构造（L11 需修复）。
- L18 服务注册表未收录 recruitment（真实缺口，本适配器本地定义 RecruitmentServiceId）。
- D10 港口键（saint_marys）与 D19 规范键（port_santa_margarita）跨章对齐交 L45。
- D19 事件草案追加由调用方/L22 幂等执行；L20 保存投影消费 SaveRevision。
- 无 P0 blocker；Runtime 延期项见结果 JSON。

## 附录：只读消费的既有文件（本任务未修改）

- `game/Assets/Scripts/Systems/D9/D9PortShipPreparationSystem.cs`（VisitTavern/Recruit，D9PreparationResult）
- `game/Assets/Scripts/Systems/D10/D10TavernSocialSystem.cs`、`D10TavernContracts.cs`（RequestAction、D10TavernRequest/Result）
- `game/Assets/Scripts/Systems/D19/D19PortAvailabilitySystem.cs`（L05）、`D19PortAuthorityValidator.cs`（L23）、`D19PortFailureCatalog.cs`（L17）、`D19PortLocalizationKeys.cs`（L18）、`D19PortEventContracts.cs`（L16）、`D19PortLifecycleContracts.cs`（L02）
