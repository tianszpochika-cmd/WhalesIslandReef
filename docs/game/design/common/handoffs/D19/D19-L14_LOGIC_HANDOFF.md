# D19-L14 交接报告

## 任务信息与结论

- 任务编号：`D19-L14`（波次 19.x；精确前置 D19-L17 / D19-L18 / D19-L23 均 LOGIC_PASS）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L14/D19_L14_LOGIC_RESULT.json`

## 前置核验（精确）

| 前置 | 状态 | 证据 |
|---|---|---|
| D19-L17 失败码目录 | LOGIC_PASS | `D19PortFailureCatalog.cs`（13 条冻结条目，含 D4 Accepted=false 事实不编造） |
| D19-L18 本地化键 | LOGIC_PASS | `D19PortLocalizationKeys.cs`（61 key + 缺 key 静态检查） |
| D19-L23 权限校验器 | LOGIC_PASS | `D19PortAuthorityValidator.cs`（15 值拒绝原因 + L17 映射） |
| D4 检修/改造公开入口 | 确认 | `D4ShipyardSystem.TryUpgrade / TryUnlockShip / Level / GetModifiers`、`D4ShipCatalog.Find` 真实存在；`ShipyardResult` 无稳定失败码；**D4 无公开检修 API** |

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19ShipyardPortAdapter.cs`（新建）
- `game/tools/results/D19/D19-L14/D19_L14_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L14_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未修改任何既有 D19/D4/Event/Config 源文件、Unity 场景/Prefab、船舶数值、契约或并行任务产物。**无 Unity API 引用（Transform/GameObject/Prefab/资源仅出现在注释中说明边界）。**

## 实现内容

### 执行链（冻结顺序，全部确定性）

1. 空请求/缺 `command_id` → MissingCommandId（L18 key `d19.failure.missing_command_id`）
2. **幂等命中**（L03 同款 `_completedResults`）：重复投递只重放既有结果（`DuplicateSubmission=true`），无新事件、无第二次 D4 调用
3. 空港口 → UnknownPort（L17 `d19.port.unknown_port`）
4. 服务匹配：非 `shipyard` 服务 ID → InvalidRequest（`port.shipyard.service_mismatch`）
5. 港口解析 + **权限**（L23 `Validate`，Kind=StartService）：拒绝 → NotAuthorized/UnknownPort（L17 码+key）；服务键按 L05 冻结口径构造 `service_<port>_<shipyard>`
6. **服务可用性**（L05 `Adjudicate`）：设施状态/势力/天气（船坞非天气敏感）/时段（夜间关闭）/人手/登记 → ServiceUnavailable（L17 `d19.service.unavailable`）
7. **生命周期前置**（L02 `CanTransition`：StartService 仅由 Berthed 进入 InService）→ 否则 ServiceUnavailable
8. **操作分派 → D4 唯一裁决**（费用/资格/等级/解锁），适配器只转换

### 五种稳定行为

| 场景 | Outcome | FailureCode / ReasonId |
|---|---|---|
| 不支持改装（未知槽位 / 已满级读 D4 公开 `Level` / 未知船型 / **检修未实现**） | UnsupportedModification | `port.shipyard.unknown_slot` / `.max_level` / `.unknown_ship_type` / `.repair_unimplemented`（诚实 BLOCKED，不调用 D4） |
| 无权（跨玩家/跨船/跨港/伪造服务） | NotAuthorized | L17 `d19.port.permission_denied` |
| 服务关闭（设施/夜间/人手/未登记/未分配泊位） | ServiceUnavailable | L17 `d19.service.unavailable` |
| D4 底层拒绝（Accepted=false，无稳定码） | UnderlyingRejected | `port.shipyard.rejected`（**不编造资金/声望原因**，money 未扣） |
| 重复请求 | 重放既有结果 + DuplicateSubmission | 与首投结果一致，无新事件 |

成功路径：`Accepted` + D4 稳定事实（Cost/NewLevel/ShipTypeId/MoneyAfter）+ `ServiceResult` 事件草案（`port.shipyard.applied`，summary 用 L18 `d19.port.event.service_result`）；拒绝路径生成 `Rejection` 事件草案（`port.rejected` + `port.reject_reason` 稳定 reason id）。

### 边界保证

- 适配器**只转换**：结果形态、失败码（L17 或稳定 reason id）、本地化 key（L18）、事件草案（L16 工厂）；**不写船舶数值、不碰 Prefab/模型/候选资源**；
- Money 以局部副本透传 `ref`，结果只报告 `MoneyAfter`，绝不写回任何系统；
- D4 规则（费用公式/声望门槛/等级上限）不复制、不预判（满级与未知船型仅读取 D4 公开只读 API `Level`/`Find`）；
- `Validate()` 自检：L17 三条目+重试策略、L18 key 合规与注册表、L16 `d19.port.*` 事件 key 兼容、L23 映射、D4 入口留痕。

## 验证结果

| 命令 | 结果 |
|---|---|
| 前置状态核验 + D4 源码盘点 | PASS（见上表） |
| 仓库外临时 dotnet 工程编译（真实 D19 九文件 + D4 三文件 + Event + Config + 适配器） | PASS：0 错误 0 警告 |
| 确定性断言 harness | PASS：57/57（成功/满级/未知槽位/检修/解锁/未知船型/资金不足/声望不足/跨玩家/跨港/设施关闭/夜间/人手/未泊位/未知港/缺 id/服务不匹配/重复重放×2/确定性/状态只由 D4 改变/协议留痕） |
| `python tools/sync_logic_task_status.py` | PASS：D19-L14=LOGIC_PASS |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |

## 明确未宣称

- 未接线真实 Player 船坞流程（编排属 D19-L33 路由器；EditMode 测试属 D19-L28）；
- 未声称 D4 支持检修：Repair 恒为诚实 BLOCKED；
- 未把 D4 底层拒绝写成资金/声望失败（无稳定码，不编造）。

## 待办 / 后续注意事项

- D19-L28（ShipyardPortAdapter EditMode 测试）可直接消费本适配器公共 API；
- D4 若未来提供稳定失败码或检修 API，本适配器按同样模式扩展（不破坏现有 reason id）；
- 跨存档幂等由 D19-L22 幂等层与 D19-L20 保存投影负责（本实例内缓存不持久化）。
