# D13-A01 产权与资产交易

任务：`D13-A01`  
日期：2026-08-11  
状态：已实现并通过自动化测试

## 1. 范围

本模块建立岛屿与资产权利的确定性裁决边界，覆盖：

- 岛屿经营权、地块、房屋、仓库、工坊、酒馆、船坞、农地、渔场；
- 产权调查、经济估值、购买、租赁、共同治理、抵押、出售、继承、转让和争议；
- 失败恢复、法律来源、经济来源、统一 `EventLog` 事实和本地存档恢复。

样例定义位于 `D13PropertyCatalog`，使用 `property_` 运行时实体 ID。它们不是把 D5 的 12 座岛屿伪装成 50 座完整内容，而是挂在既有岛屿/港口引用下的 D13 资产实例定义。

## 2. 唯一写者与 authority 边界

| 数据 | 唯一写者 | 本模块行为 |
|---|---|---|
| 产权、租约、共同治理成员、抵押标记、转让/继承后的权利投影 | `PropertySystem` | 仅在法律与经济 authority 均批准后写入 `D13PropertyState` |
| 资产价格和估值 | `EconomySystem` / `ID13PropertyEconomyAuthority` | PropertySystem 只保存 Economy 返回的来源化报价，不计算面积、岛屿或类型系数 |
| 金钱、账本、抵押本金和债务 | Economy/Finance | PropertySystem 只接收 `LedgerRef` 和批准结果，不修改 `D3TradeState.Money` |
| 登记、法律归属、保护状态和争议 | `FactionLegalSystem` / `ID13PropertyLegalAuthority` | PropertySystem 只保存法律决定的只读投影，不自行判定产权 |
| 货物、仓库库存、生产和船坞服务 | Inventory/Building/Shipyard | 资产系统只写资产权利，不复制库存、建筑工期或船型服务状态 |
| UI 与 Agent | UI / Agent | 只提交 request 或读取 read model；不能直接改权利、价格、金钱或法律状态 |

没有注入 authority 时，系统会明确拒绝操作并给出 `*_authority_required`，不会使用隐藏默认价格或默认法律判决。

## 3. 样例资产目录

`D13PropertyCatalog.CreateVerticalSlice()` 提供九类样例：

| 资产 | ID | 特殊边界 |
|---|---|---|
| 岛屿经营权 | `property_estate_saint_marys` | 购买经营权不抹除居民、势力或历史法律关系 |
| 地块 | `property_parcel_green_cay_mangrove_edge` | 保护区/社区 agreement 由法律 authority 确认 |
| 房屋 | `property_house_santa_marys_quayside` | 登记居住权来自产权调查 |
| 仓库 | `property_warehouse_santa_marys_north_store` | 货物仍由 Inventory/Cargo 写入 |
| 工坊 | `property_workshop_driftwood_ropewalk` | 劳工和经营状态留给治理/建筑系统 |
| 酒馆 | `property_tavern_santa_marys_anchor` | 不复制 D10 酒馆社交状态 |
| 船坞 | `property_shipyard_santa_marys_dock` | 船型、维修和服务价格留给 Shipyard/Economy |
| 农地 | `property_farmland_green_cay_cassava` | 作物、生长和生态后果留给农业/Ecology |
| 渔场 | `property_fishery_green_cay_shallow_reef` | 鱼群数量、压力和恢复留给 Ecology |

每个定义包含岛屿、位置、控制势力、默认登记人、法律登记引用、社区/保护区标志、历史标签、来源说明、阶段和测试 ID。

## 4. 确定性规则

1. 调查必须先于估值和所有权交易。调查失败仍留下 `property_survey_denied` 事实和 `route_property_title_review` 恢复路线。
2. 估值只能来自 Economy authority 的 `D13ValuationQuote`；UI/Agent 提交的 `OfferedAmount` 不会覆盖 quote。
3. 购买、租赁、共同治理、抵押、出售、转让和继承必须同时通过 Legal authority 与 Economy authority；争议开立/解决只由 Legal authority 决定，不凭空生成金额。
4. 抵押申请人必须是当前登记所有者；租约结束日必须晚于当前世界日；出售/转让/继承必须带受让方；共同治理必须带参与方列表。
5. 失败、拒绝、来源缺失或请求形状错误不改变产权状态，只追加带恢复路线的 `failed` 事实。
6. 同一 `event_id + save_revision` 再次提交返回 `Duplicate`，不重复转移、抵押、添加租约或追加事实。
7. 产权状态的 `LegalStatus`、`LegalSourceEventId` 和 `LatestValuation*` 是已裁决来源的存档投影，不是 PropertySystem 自行推导的法律或价格。

## 5. 交易状态变化

| 操作 | 状态变化 |
|---|---|
| Purchase | 登记所有者、控制者变为申请人；清理租约/共同治理/旧抵押标记 |
| Lease | 保留登记所有者，写入租户和结束日，状态为 `Leasehold` |
| CoGovernance | 保留原登记所有者，写入去重后的共同治理成员，状态为 `CoGovernance` |
| Mortgage | 保留所有者，写入 Economy 返回的 `LedgerRef`，状态为 `Mortgaged` |
| Sale | 由当前所有者转给受让方，清理租约/共同治理/抵押标记 |
| Transfer | 由当前所有者转给受让方，状态为 `Transferred` |
| Inheritance | 由法律确认的继承对象接收，状态为 `Inherited` |
| OpenDispute | 写入争议登记号，状态为 `Disputed`，不自行决定胜负 |
| ResolveDispute | 接受 Legal authority 返回的归属对象，恢复为 `PrivateOwnership` 或 `Transferred` 投影 |

## 6. 事实事件与来源

所有结果都先追加共享 `EventLog`，再由 PropertySystem 写入状态；事件 `result.values` 保留：

- `property_id`、操作类型和结果码；
- `legal_source` / `economy_source` / `ledger_ref`；
- 调查证据数量、登记所有者、争议号和最终归属；
- 估值金额、货币和估值依据。

调查和估值事实的 `owner_system` 分别是 `FactionLegalSystem` 和 `EconomySystem`；产权状态改变事实的 `owner_system` 是 `PropertySystem`。因果引用只接受已经进入日志的事实，不伪造不存在的 cause。

失败事实必须带 `RecoveryRoute`：产权文书复核、经济估值复核、交易复核或法律申诉。恢复是后续新事实，不覆盖原拒绝。

## 7. 存档

`D13PropertyRuntimeSnapshot` 写入 `location_json.d13`，包含：

- 所有 `D13PropertyState`：所有权、租约、共同治理、抵押、法律投影、争议、估值来源、修订号和最后事实；
- 调查记录和证据引用；
- Economy 返回的最新估值报价；
- 已应用事实键投影。

完整事实仍使用 D8 统一共享 `EventLogSnapshot` 的存档入口（通常随 D3 runtime 保存），D13 不复制一份事件日志或账本。读档后先恢复 D13 投影和共享事实，再由同一 `event_id + save_revision` 保护重复交易。

旧 D7/D8 存档没有 `d13` 时返回空引用，不猜造地块、房屋或产权；新档通过 `SaveSystem.SaveD13Runtime` 写入，`SaveSystem.LoadD13Runtime` 只返回副本。

## 8. 历史与敏感内容审查

资产定义使用 `inferred` 历史标签和项目来源说明；产权不是“付钱即抹除居民”的按钮。拥有居民、社区或保护区标志的资产必须由 Legal authority 返回协议、租赁、共同治理或争议结果。系统不把殖民制度、社群土地或强迫劳动做成无代价的利润优化，也不引入现代银行或网络裁决。

## 9. 测试入口

- `D13PropertyAssetSystemTests.Catalog_ContainsIslandParcelHouseWarehouseWorkshopTavernShipyardFarmlandAndFishery`
- `D13PropertyAssetSystemTests.FullLifecycle_UsesLegalAndEconomyAuthorities_AndRestoresWithoutDuplicateSettlement`
- `D13PropertyAssetSystemTests.AuthorityDenial_DoesNotChangeOwnershipOrInventPrice`
- `D13PropertyAssetSystemTests.SaveSystem_PersistsPropertyOwnershipSurveyValuationAndDisputeProjection`
- 运行器：`tools/d13_a01_runner/D13A01Runner.csproj`

测试覆盖九类资产目录、调查、估值、购买、租赁、共同治理、抵押、出售、转让、继承、争议、法律/经济拒绝、来源事件、保存恢复和重复保护。

## 10. 后续模块接入

- D13-A02 应实现真正的 Finance/Economy authority，消费 `LedgerRef`，不得在 PropertySystem 再写账户或债务。
- D13-A05 应消费共同治理与居民协议事实，不能把 `CoGovernanceRefs` 当作人口状态副本。
- D13-A06 应读取资产权利并写建筑、仓库和船坞状态，不得改 `OwnerRef`。
- D13-A03/A04/A07 只能读取农地/渔场/工坊/仓库权利和事件；生产、库存、价格和市场仍由各自 owner 裁决。
