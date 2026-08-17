# D19-L06 交接报告

## 任务信息与结论

- 任务编号：`D19-L06`（波次 1，并行）
- 前置：D19-L02 / D19-L04 / D19-L16 / D19-L17 均 LOGIC_PASS（已核实）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L06/D19_L06_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19BerthSystem.cs`（新建，泊位原子领域系统）
- `game/tools/results/D19/D19-L06/D19_L06_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L06_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未修改 D19-L04 目录、D4 船只数值、任何事件生产者、Save、其它 D19 文件、contracts、Unity 或表现层；未启动 Player。`D19BerthSystem.cs.meta` 由 Unity 下次导入时生成（非本任务范围）。

## 系统设计（`D19BerthSystem`）

### 操作（全部原子：先全量校验 → 单一变更点 → 事件草案；失败无半写）

| 操作 | 行为 | 事件草案（L16） |
|---|---|---|
| `Reserve` | 校验命令 ID/港口/泊位/跨港/能力/租期/占用/一船一泊位 → Reserved（带租期） | `CreateBerthReserved`（until_day 入 payload） |
| `ConfirmOccupancy` | 仅当同船已预留且未过期 → Occupied（无租期） | `CreateBerthReserved`（同一确认命令） |
| `Release` | 占用或预留均可释放；过期预留释放 reason=expired | `CreateBerthReleased`（release_reason） |
| `AdvanceTime` | 过期推进：过租期 Reserved → Available，逐条 reason=expired；确定性、幂等 | `CreateBerthReleased`（reason=expired） |
| `CheckConflict` | 只读冲突探测（占用/能力/跨港/一船两泊位），无变更无事件 | 无 |

### 失败语义（L17 + 同格式原因 ID）

- L17 已注册码：`d19.berth.occupied`（占用）、`d19.berth.expired`（过期）、`d19.port.unknown_port`（未知港口）；`Recoverable` 取自 L17 重试策略；
- 新领域原因 ID（同 `d19.berth.<name>` 格式，**L17 目录待扩展，本系统未修改 L17**）：`d19.berth.ship_type_mismatch`（档位/深水能力）、`d19.berth.invalid_term`（零时长或超 30 天上限）、`d19.berth.cross_port`（锚点目录跨港检测）、`d19.berth.duplicate_command`（同命令 ID 异参数）、`d19.berth.missing_command_id`、`d19.berth.unknown`、`d19.berth.ship_already_berthed`、`d19.berth.not_reserved`、`d19.berth.capability_missing`、`d19.berth.invariant`；
- 失败（可定位原因时）同时生成 L16 拒绝事件草案（`port.reject_reason` 入 payload）。

### 不变式与幂等

- 同一船不可占两个泊位（`_shipToBerth` 单射）；同一泊位不可同时给两船（每泊位单分配）；Reserved 必有租期（ExpiresWorldDay>ReservedWorldDay）、Occupied 无租期（0=直至释放）；`ValidateInvariants()` 全程为空；
- 同命令 ID + 相同操作 + 相同参数重复投递 → 返回存储结果（无副作用）；同命令 ID + 不同操作 → `DuplicateCommand` 失败且不产生任何变更；
- 所有可变泊位事实只由本系统写；只读访问全部返回副本（`GetAllocation`/`GetAllocations`/`GetApplied`）。

### 边界

- 稳定键：berth_id/ship_id/command_id（D18 语义）；逻辑时间：world_day + save_revision；无 DateTime/真实时钟/Transform/物理检测；
- 锚点解析只读消费 `D19PortAnchorCatalog`（L04，含跨港检测）；能力表（`CapabilityTable`，冻结）为本系统拥有的泊位事实，与 L04 默认 8 泊位一一对应；
- 不读取/不修改 D4 船只数值（船能力画像由调用方从 D4 只读目录传入）；事件只生成草案，追加与去重归 D19-L33 路由/EventLog。

## 验证（全部真实执行）

| 检查 | 结果 |
|---|---|
| 临时 dotnet 工程（netstandard2.1，引用真实 Event + L02 + L04 + L16 + L17 + L18 源码 + 本文件） | 0 警告 / 0 错误 |
| 临时控制台 45 项确定性断言（五操作全覆盖、L17 码/原因 ID、幂等重放无副作用、过期推进幂等、冲突拒绝保持原占用、逐阶段不变式、L16 草案过 Guard） | ALL CHECKS PASS（45/45） |
| `git diff --check` | exit 0（仅既有 LF→CRLF 警告） |
| `python tools/sync_logic_task_status.py` | 总览重新生成，D19-L06=LOGIC_PASS |

## 明确未宣称

- 未宣称泊位已接入真实港口流程（编排/EventLog 追加归 L33 及后续任务）；
- 未修改 D19-L04 目录、D4 船只数值、L17 失败目录；
- 未读取坐标/物理/场景；未启动 Player。

## 待办 / 后续注意事项

- L17 后续扩展应注册本系统定义的新领域原因 ID（见结果 JSON runtime_deferred）；
- D19-L20 保存投影可用 `GetAllocations()` 只读副本；D19-L22 幂等路由可复用 `GetApplied`；
- L04 新增泊位锚点时必须同步 `CapabilityTable`，否则该泊位返回 `d19.berth.capability_missing`（诚实拒绝，不猜测能力）。
