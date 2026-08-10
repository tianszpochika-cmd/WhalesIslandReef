# D13-A02 金融、信用与保险

任务：`D13-A02`  
日期：2026-08-11  
状态：已实现并通过自动化测试

## 1. 范围

本模块实现可保存、可解释、可追溯的金融垂直切片：

- 现金账户、收支、赊账、贷款、本金、信用评分与信用额度；
- 担保、抵押物、利息、到期、宽限、展期费用与展期上限；
- 保险报价、保费、免赔额、保额上限、证据、排除项、拒赔和部分赔付；
- 违约、催收通知、可用现金催收、法律查封请求和破产保护；
- 统一 `EventLog` 事实、失败恢复路线、风险说明和存档恢复幂等。

本模块的系统类为 `D13FinanceSystem`。它只拥有 Finance 域的账户、债务、担保、保险、证据、理赔和金融查封请求投影，不拥有产权、库存、经济报价或法律归属。

## 2. 唯一写者与 authority 边界

| 数据 | 唯一写者 | D13-A02 行为 |
|---|---|---|
| 账户现金、信用额度/评分、债务、利息、担保、保险单、理赔、金融查封请求 | `FinanceSystem` | 接收经过来源验证的 request，写自己的快照和事实 |
| 价格、外部资金来源、抵押估值 | `EconomySystem` | 通过 `EconomySystem` 来源事实或 `ID13FinanceCollateralAuthority` 提供裁决 |
| 产权、资产法律归属、实际查封 | `PropertySystem` / `FactionLegalSystem` | authority 返回来源化决定；FinanceSystem 只写 `approved_pending_property_owner` 请求投影 |
| 库存、货物、奖励、关系和最终赔付外部账 | 所属系统 | FinanceSystem 不复制、不直接改写 |
| UI / Agent | UI / Agent | 只能提交 request 或读取 `GetReadModel()`；Agent 不能批准贷款、理赔、债务或奖励 |

没有 Economy/Legal/Property authority 或来源事实时，系统安全拒绝，不猜造价格、金钱、法律结果或资产归属。

## 3. 统一配置与确定性计算

`D13FinanceConfiguration` 使用 D8 `NumericParameterDefinition`，启动时执行 `Validate()`。代表性配置为：

| 参数 | 默认值 | 用途 |
|---|---:|---|
| BaseLoanInterestPercent | 12% | 贷款基础年化简化月率 |
| CreditRiskSpreadPercent | 8% | 信用评分较低时增加风险利差 |
| LatePenaltyPercent | 5% | 超过宽限期时的一次性滞纳金 |
| RenewalFeePercent | 3% | 展期资本化费用 |
| InsurancePremiumPercent | 8% | 保险月保费基准 |
| InsuranceDeductiblePercent | 15% | 免赔额 |
| ClaimCoveragePercent | 85% | 单保单最高赔付占保额比例 |
| GraceDays / DefaultLoanTermDays | 3 / 30 日 | 宽限和默认期限 |
| MaxRenewals / RenewalTermDays | 3 / 15 日 | 展期上限和默认展期长度 |
| CollectionFee | 5 | 正式催收通知计入的费用 |
| BankruptcyProtectionDays | 7 日 | 破产保护期限；保护不免除本金 |

所有金额使用 `long` 并经过 D8 金额边界。贷款利率为基础利率加确定性的信用风险利差；利息按经过的世界日计入。还款固定按“滞纳金→利息→本金”顺序。赊账创建债务但不增加现金，贷款才按批准本金增加账户现金。

## 4. 金融生命周期

1. `OpenAccount` 必须引用 `EconomySystem` 的已记录来源事实。
2. `RequestLoan` 可提交抵押和担保；抵押物的估值/法律来源由 authority 返回，FinanceSystem 只保存引用。`RequestCredit` 创建 `MerchantCredit`，不发放现金。
3. `AccrueInterest` 按统一配置计息；破产保护期间写入冻结事实，不删除本金。
4. `EvaluateDue` 在到期前不改变风险状态；宽限期内为 `PastDue`，超过宽限进入 `Defaulted` 并降低信用评分、记录滞纳金。
5. `Renew` 将透明展期费用资本化，增加到期日和展期次数，并降低少量信用；超过上限拒绝并提供恢复路线。
6. `Repay` 只从 Finance 账户现金中结算，结清后债务进入 `Paid` 并改善信用。
7. `Collect(IssueNotice)` 首次正式通知计入配置催收费；`CollectAvailableCash` 仍按相同清偿顺序结算。债务没有可用现金时，事实明确给出展期、任务抵债或法律查封路线。
8. `RequestAssetSeizure` 必须是违约/催收债务且通过 Legal/Property authority；FinanceSystem 写金融查封请求，实际产权变化留给 `PropertySystem`。
9. `RequestBankruptcyProtection` 冻结利息和催收状态至确定日期；`ReleaseBankruptcyProtection` 后恢复原状态，任何债务不会凭保护自动免除。

## 5. 保险、证据与拒赔

保险报价由 `QuoteInsurance` 确定性生成，返回保费、免赔额、最高赔付、有效期、承保范围、排除项、证据要求、风险和最坏结果。购买时保费由 FinanceSystem 账户扣除，不能由 UI 直接改钱。

`RegisterEvidence` 只登记证据引用，不创造事故事实。`SubmitClaim` 必须引用共享 `EventLog` 中已存在的事故，并满足：

- 至少 `MinimumInsuranceEvidence` 项证据；
- 保险单要求的证据类型（货物保险代表为事故事实 + 货物清单，船体保险为事故事实 + 损伤报告）；
- 事故标签在承保范围内且不命中 `illegal`、`fraud`、`intentional`、`abandoned` 排除项；
- 损失在有效期内，赔付扣除免赔额且不超过剩余保额。

证据不足、排除项、未承保事件和事故事实缺失都会生成 `Denied` 理赔事实，赔付为零并带 `route_insurance_evidence_review` 恢复路线；已付保费不会自动退回。超过免赔额但受保额限制时生成 `PartiallyPaid`，同时保存 `ApprovedPayout` 与 `DeniedAmount`。

## 6. 事实、风险和 UI 读模型

所有成功、拒绝和失败结果先进入共享 `EventLog`，事件 owner 为 `FinanceSystem`，事实 values 至少包含操作对象、来源事实、金额拆分、风险或最坏结果。外部来源必须来自 `EconomySystem` 或经 Legal/Property authority 的来源事件；cause 只引用日志中已有事实。

UI 只读取 `D13FinanceReadModel` 或 `D13FinanceResult`：

- 贷款：显示本金、当前利息、滞纳金、当前应付、利率、担保责任上限、抵押物可能查封和破产保护的暂缓性质；
- 保险：显示保费、免赔额、最高赔付、排除项、证据门槛、拒赔后果和最坏赔付为零；
- 催收/查封：显示催收费、催收状态、法律来源和 PropertySystem 尚未被本系统直接改写；
- 失败：显示 `Code`、来源缺失原因和 `RecoveryRouteId`。

Agent 只能提出贷款谈判、展期、保险申请或证据补充建议；裁决始终由 FinanceSystem 和所属 authority 完成。

## 7. 存档与幂等

`D13FinanceRuntimeSnapshot` 写入 `location_json.d13Finance`，保存账户、债务、担保、证据、保单、理赔、查封请求、修订号和已应用金融事实键。共享 `EventLogSnapshot` 继续保存完整事实，不在 FinanceSystem 复制事件日志。

读档时 `SaveSystem.LoadD13FinanceRuntime()` 返回副本，`D13FinanceSystem.Restore()` 先校验引用关系，再恢复 Finance 投影。相同 `event_id + save_revision` 的请求返回 `Duplicate`，不会重复增加现金、利息、债务、保险赔付、催收费或查封请求。

旧存档没有 `d13Finance` 时使用空快照；不会凭空生成账户、债务或保单。

## 8. 测试入口

- `D13FinanceSystemTests.LoanInterestRenewalDefaultCollectionSeizureAndBankruptcyAreDeterministic`
- `D13FinanceSystemTests.InsuranceEvidenceDenialPartialPayoutAndSaveRestoreAreTraceable`
- `D13FinanceSystemTests.InvalidSourceAndInsufficientCashLeaveCanonicalStateUnchanged`
- 运行器：`tools/d13_a02_runner/D13A02Runner.csproj`

测试覆盖现金/赊账/贷款、担保抵押、利息、展期、到期宽限、违约、信用变化、催收、法律查封请求、破产保护、保险报价与购买、证据不足拒赔、部分赔付、保存恢复和重复保护。
