# D18 豁免与拒绝规则（D18-L19）

> 冻结日期：2026-08-16
> 任务：`D18-L19`（波次 1，并行；与 L02、L03、L04—L20 并行执行）
> 唯一写入范围：本文件、`game/tools/results/D18/D18-L19/`、`docs/game/design/common/handoffs/D18/D18-L19_LOGIC_HANDOFF.md`、当日记忆日志。
> 本文件是 [D18-L01 基线](../D18_LOGIC_GOVERNANCE_BASELINE.md) 第 2.4（拒绝条件）/2.5（撤销条件）节与 [L03 状态机规范](D18_ASSET_STATE_MACHINE.md)（T5/T6/T7、审计记录、豁免语义）在**拒绝/挂起/豁免/撤销**流程上的权威展开。
> 读取顺序：D18—D24 路线 → LOGIC_TASK_PROTOCOL → D18 章节任务卡 → L01 基线 → L03 状态机 → 本文。

## 0. 边界声明（三条"不"，硬性）

1. **不新增资源状态**：`Pending`、`WaiverRequested` 是**管理/审查语义**（记录级字段与独立豁免记录），**不是** L02 Schema `state` 枚举的取值；资源状态枚举保持 L03 冻结的七个（`Candidate`/`IntakeEligible`/`RuntimeRequested`/`RuntimeApproved`/`ReleaseApproved`/`Rejected`/`Revoked`），本文件不修改。
2. **不绕过商业授权**：豁免（waiver）**永远不能**把未获商业授权、NC、个人使用限制或禁止再分发变成可商用/可再分发；`rights` 四项由 L07 独立验证，waiver 不参与权利裁决（第 7.4 节双 no-go 定理）。
3. **不修改真实条目状态**：本文件只是规则规范；不创建、不改变任何真实 catalog、Registry、manifest 或验证器输出中的条目状态。当前真实事实：`LocalDevelopmentReady=0`、`ReleaseApproved=0`、无任何 Rejected/Revoked/waiver 记录（2026-08-16 基线快照）。

## 1. 范围与定位

### 1.1 本文定义什么

1. 四类管理语义的完整规则：`Rejected`（拒绝终态）、`Pending`（证据缺口保持）、`WaiverRequested`（豁免申请）、`Revoked`（撤销终态）——各自**触发条件、证据、审批角色、期限、可读范围、恢复方式、记录字段**；
2. 五类真实触发条件的处理矩阵：缺许可、冲突来源、重复哈希、不可再分发、证据缺失/被推翻（加越级申请）；
3. 豁免（waiver）的**可豁免白名单 / 不可豁免黑名单**、有效期、失效处理与审计记录；
4. 与 L03 状态机（T5/T6/T7、REJECT/REVOKE/RE_REGISTER 事件、审计记录字段）、L35（过期豁免审计）、L36（守卫条件）、L37（晋级申请）、L42（目录编译）的接口边界；
5. 硬性红线：waiver 不能绕过商业授权、不能变成 Runtime/Release 批准。

### 1.2 本文不做什么（红线）

- 不修改任何 Schema、验证器、真实 catalog、Unity、Prefab、场景、Registry 或资产文件（基线第 5.3 节全章禁区）；
- 不创建任何真实豁免记录或拒绝/撤销记录（规则只定义字段与流程；实际记录由人类复核或 L35/L42/L43 在各自任务中产生）；
- 不授予任何资源状态；不产生任何 `RuntimeApproved`/`ReleaseApproved` 授予。

### 1.3 消费者与只读数据源

| 消费者（只读本文件） | 用途 |
|---|---|
| L35 状态迁移审计 | 过期/失效 waiver → 任何批准判定 FAIL，必须引导 REJECT/REVOKE（L03 7.2 第 5 条） |
| L36 LocalDevelopmentReady 守卫 | C1—C10 条件**不因 waiver 变化**；waiver 不满足任何条件（L03 7.3） |
| L37 晋级申请 Schema | waiver 不改变申请合法性：`current_state` 仍必须为 IntakeEligible 或 RuntimeRequested |
| L42 目录编译器 | `IntakeEligible` 授予条件不参考 waiver；缺任何 P0 证据即不授予（T1 唯一路径） |
| L43 来源审计 / L49 状态复核 | 缺口/拒绝/撤销统计口径来自本文件 |
| L04—L10 验证器 | 各自 P0 缺口 → 本文件的 Pending/Rejected/WaiverRequested 处理路径 |

只读数据源（真实磁盘事实）：准入目录 `d18_prefab_candidate_intake_catalog.json`（`known_gaps` 非空 1,865 条；"controlled work-scene consumer not yet authored" 1,254 条、"no runtime consumer is asserted by the source manifest" 337 条等）；`d18_local_development_ready_catalog.json`（`status=BLOCKED`，0/0）；A04 授权复核 `Pending` 267 条（D16_A04_REVIEW_PACKET_CURRENT.md，非 D18 章节责任）；`historical_review` 三值 `Pending/Approved/ApprovedWithWarning`（D8/D14 既有审查字段，与资源状态正交）。

## 2. 术语与状态关系（Pending/WaiverRequested 不是状态）

| 术语 | 定义 | 存放位置 |
|---|---|---|
| 资源状态 `state` | L02 Schema `state` 字段，严格七值枚举（L03 第 3 节） | 资源逻辑记录 `state` |
| `Rejected` | 七状态之一：拒绝终态（L03 5.6） | `state="Rejected"` |
| `Revoked` | 七状态之一：撤销终态（L03 5.7） | `state="Revoked"` |
| `Pending` | **审查语义，不是状态**：证据缺口未补齐时的保持形态；`state` 字段不写 "Pending" | 记录级 `review_status` / `known_gaps` / 缺口清单 |
| `WaiverRequested` | **审查语义，不是状态**：豁免申请流程的阶段（Requested/Active/Expired/Revoked）；`state` 字段不写 "WaiverRequested" | 独立豁免记录 + 记录级 `waiver_status` |

**关键口径（防混淆）**：

- `Pending`/`WaiverRequested` 若被写入 `state` 字段 → 校验失败（L02/L35 拒绝），因为它们不是状态；保持与 L03 闭合枚举一致。
- 既有 `historical_review=Pending`（D14，审查字段）、A04 `Pending` 267 条（授权复核）、`LocalDevelopmentReady` 目录 `BLOCKED`（守卫结论）都不是资源状态，与本文件语义一致地并存。
- **状态不可"挂起"**：一个条目要么是七状态之一；"证据缺口"只通过字段表达（known_gaps、waiver_status），不通过新状态表达。

## 3. 四类管理语义总表

| 维度 | `Rejected`（拒绝终态） | `Pending`（缺口保持） | `WaiverRequested`（豁免申请） | `Revoked`（撤销终态） |
|---|---|---|---|---|
| 性质 | 资源状态（七枚举之一） | 审查语义（记录级字段） | 审查语义（独立记录+字段） | 资源状态（七枚举之一） |
| 触发条件 | 第 4 节矩阵中的拒绝触发：缺许可、冲突来源、不可再分发、越级申请、证据被推翻（未授予过）、豁免过期不补 | 任一 P0 证据缺失/验证器返回 Pending（L04—L10/L15/L16 未全过）；缺口可补、无伪造迹象 | 存在**可豁免白名单内**的证据缺口（第 7.2 节），由人类决定是否提交豁免申请 | 曾获授予/登记的条目证据被推翻（L03 4.6）：哈希不匹配、许可撤回/URL 失效、伪造/补猜、不再满足所在状态证据要求 |
| 证据 | reason 必填 + evidence_paths + operator + occurred_at（L03 6.1） | 缺口清单（缺失字段/验证器失败码/known_gaps 条目） | 豁免记录全部字段（第 7.5 节），`expires_at` 必填 | reason 必填 + `prior_record_id` 必填（引用原授予/登记记录） |
| 审批角色 | 人类复核；验证器可标记（`tool:<名>`）但必须留 reason | 无（保守默认自动保持，不需批准） | **仅责任人类审核者**（`granted_by=human:<角色>`）；工具/验证器不得批准 | 人类复核；验证器可标记但必须留 reason + 引用记录 |
| 期限 | 无（终态） | 无自动过期；保持到补齐或人类 REJECT/REVOKE | **必填 `expires_at`**；到期自动失效（`waiver_status=Expired`） | 无（终态） |
| 可读范围 | 审计 | 验证器/审计只读；消费任务禁止引用二进制 | 审计 + 验证器（仅缺口状态读取）；不改变 L17/L18 守卫结论 | 审计；仅人类复核可重新登记 |
| 恢复方式 | **不自动恢复**；重新进入管线只能人类从来源重新登记（新 canonical_asset_id 或人类裁决） | 补齐证据 → 重新验证 → T1（L42 编译）→ IntakeEligible；**Pending 永不直接产生授予** | 到期/条件不成立/人类撤销 → 缺口恢复原状；可重新申请（append-only 新记录）；**waiver 不授予任何状态** | T7：`Revoked → Candidate` 仅人类复核（重新开始证据链）；禁止直接复活到任何高级状态 |
| 记录字段 | L03 6.1 审计字段 | `review_status`、`known_gaps`、`missing_evidence[]`、`updated_at` | 第 7.5 节豁免记录字段 | L03 6.1 审计字段（reason + prior_record_id） |

## 4. 五类触发条件的处理矩阵（成功状态依据）

> 每条真实触发的处理路径必须落到下表唯一分支；路径判定顺序 = 表内行序（先伪造/权利，后证据/数量）。

| # | 触发条件 | 定义/证据 | 处理路径 | 说明 |
|---|---|---|---|---|
| 1 | **不可再分发** | L07 任何一项权利为 false/unknown/矛盾：NC、个人使用限制、明确禁止再分发/随游戏分发 | **Rejected**（不可豁免，第 7.3 节黑名单第 1 条） | 若条目已获授予 → **Revoked**（第 8 节）；waiver 永远不能改写权利结论 |
| 2 | **伪造/补猜证据** | 来源/作者声明冲突或伪造、补猜哈希/许可/数量、来源页与包内许可矛盾且无法解释 | 未授予 → **Rejected**；已授予 → **Revoked** | 伪造是黑名单（不可豁免）；必须留 reason + 证据路径 |
| 3 | **缺许可** | `license` 缺失，或许可证证据文件不存在/不可读；L04 返回未知/冲突/阻断 | 可补（如证据文件未归档）→ **Pending**；人类认定不可补/来源不可信 → **Rejected** | 未知/冲突许可证**不可豁免**（黑名单第 2 条），不得猜测 CC0 |
| 4 | **冲突来源** | 同一规范化路径多来源声称；来源页与包内许可矛盾 | 先 **Pending**（登记冲突清单，append-only）；人类复核后：来源可裁定 → 保留合法来源并补齐；不可裁定/伪造 → **Rejected** 或 **Revoked** | 冲突不自动拒绝：先挂起、后人类裁决（基线 2.4.2 语义） |
| 5 | **重复哈希** | 哈希相同但 canonical_id 不同 | 先 **Pending**（只登记不删除，L31 裁定）；L31 裁定不可调和的重复 → 后入条目 **Rejected** | 两步语义：登记挂起 → 裁定拒绝；重复条目保留原证据（基线 2.4.3） |
| 6 | **证据缺失/被推翻** | 下载日期 null、`sha256` pending、`evidence_paths` 缺失、文件不存在、哈希不匹配、来源页内容变更 | 缺口可补 → **Pending**（或白名单内证据缺口 → **WaiverRequested**）；缺口不可补 → **Rejected**；曾获授予 → **Revoked** | 文件**不存在**不可豁免（第 7.2 节白名单排除）；哈希不匹配属"被推翻"，非"待补" |
| 7 | **越级申请** | 无消费消费者、无 `RuntimeRequested` 却申请 `RuntimeApproved`；`current_state=Candidate` 直接申请 | **Rejected**（申请数据无效） | L37 校验；越级不可豁免、不可转 Pending |
| 8 | **豁免过期/条件不成立** | `expires_at` 已过，或豁免条件（conditions）不再满足 | **不产生任何批准**；人类复核后 → 缺口恢复 **Pending**，或满足拒绝条件 → **Rejected** / 已授予 → **Revoked**（L03 4.5.7） | L35 必须将"过期豁免+批准"判定为 FAIL（L03 7.2 第 5 条） |

**判定优先级**（一行只走一径）：伪造/权利（#1/#2）→ 缺许可（#3）→ 来源冲突（#4）→ 重复哈希（#5）→ 证据（#6）→ 越级（#7）→ 豁免（#8）。

## 5. 拒绝规则详述（→ `Rejected`）

### 5.1 触发与证据

- 触发：第 4 节 #1/#2/#3（人类判定不可补）/#4（裁决失败）/#5（L31 裁定）/#6（不可补）/#7/#8；
- 证据：`reason`（拒绝原因，**必填非空**）、`evidence_paths`（仓库内相对路径）、`operator`（`human:<角色>` 或 `tool:<工具名>`，验证器标记时必须留原因）、`occurred_at`（ISO-8601 带时区）、`prior_record_id`（如适用，引用被拒绝的申请/登记记录）。

### 5.2 审批角色与可读范围

- 人类复核写终态；验证器只能**标记**（不得直接写入真实 `state`，L02/L35 校验 operator 合法）。
- 可读范围：审计；游戏逻辑、消费任务、运行时一律不可读（L17/L18 守卫）。

### 5.3 恢复方式

- **不自动恢复**（L03 5.6）：被拒绝条目重新进入管线只能由人类从来源重新登记（新 `canonical_asset_id` 或人类明确裁决），不走自动重试；不因 waiver 复活。

## 6. Pending 规则详述（证据缺口保持）

### 6.1 触发与证据

- 触发：任一 P0 证据缺失或验证器返回 Pending（L04—L10/L15/L16 未全过）、下载日期 `null`、`sha256` pending、`known_gaps` 非空（准入目录 1,865 条全带 `known_gaps` 的真实事实）、A04 授权复核未决（267 条，非 D18 责任）。
- 证据：`missing_evidence[]`（缺口清单：字段名/验证器失败码/known_gaps 条目）、`updated_at`。

### 6.2 审批角色、期限、可读范围

- 无审批角色（保守默认自动保持）；无自动过期；保持到补齐或人类 REJECT/REVOKE。
- 可读：验证器/审计只读；消费任务与游戏逻辑禁止引用二进制（L17）；不满足 L36 C1—C10 中任何相关条件。

### 6.3 恢复方式

- 补齐证据 → 重新验证全部相关验证器 → T1（L42 编译授予 `IntakeEligible`）；
- **Pending 永不直接产生任何授予**；waiver 不会把 Pending 变成 IntakeEligible。

## 7. WaiverRequested 规则详述（豁免）

### 7.1 定义

豁免是对**非权利类证据缺口**的临时、有期限、人类批准的审查安排。豁免生效期间，该缺口**不立即触发 REJECT 判定**（REJECT 暂停），条目可继续受控审查；**豁免本身不授予任何状态、不满足任何守卫条件**。

### 7.2 可豁免白名单（仅证据类缺口）

| 白名单项 | 条件 |
|---|---|
| 逐文件哈希 pending（`sha256` 待计算） | 原文件存在于仓库外下载目录（L06 返回 Pending 而非文件缺失） |
| 证据文件路径待归档（`evidence_paths` 暂缺） | 来源官方页/包清单可静态核验；证据文件真实存在且后续可归档 |
| 描述器非权利字段缺失 | 如 LOD 意图、轴向声明等（L25—L29 非权利字段），不改变权利/许可结论 |
| 其他人类认定的非权利、非伪造缺口 | 必须写明 rationale 与 conditions |

### 7.3 不可豁免黑名单（永远禁止 waiver）

1. **商业授权缺失/NC/个人使用限制/禁止再分发**（L07 任何一项非真）——waiver 不能绕过商业授权（第 7.4 节定理 1）；
2. 许可证未知/冲突（L04 阻断）——不得猜测 CC0；
3. 伪造/补猜证据、来源伪造；
4. 越级申请（L37）；
5. 文件不存在（不是"待补"）；
6. 任何指向 `RuntimeApproved`/`ReleaseApproved` 的需求——waiver 不能变成 Runtime/Release 批准（第 7.4 节定理 2）。

### 7.4 双 no-go 定理（硬性）

> **定理 1（商业授权）**：任何豁免记录、豁免审批或豁免条件，均不得把"未获商业授权 / NC / 个人使用限制 / 禁止再分发"改写为"可商用 / 可再分发"。`rights` 四项必须为显式真值并由 L07 独立验证；waiver 不参与、不覆盖、不替代权利裁决。违反 → 视为伪造（L35/L49 拒绝）。
>
> **定理 2（批准路径）**：任何豁免（含有效期内）均不得产生 `RuntimeApproved` 或 `ReleaseApproved` 授予、不得替代 L42 的 `IntakeEligible` 授予条件、不得缩短或跳过 T1→T2→T3→T4 唯一路径（L03 第 4.2 节可达性定理）。豁免只暂停 REJECT 判定，从不产生批准。

### 7.5 豁免记录字段（`waiver_id` 记录，append-only）

| 字段 | 语义 | 必填 |
|---|---|---|
| `waiver_id` | 唯一 ID，格式 `W-<canonical_asset_id>-<seq>` | 是 |
| `canonical_asset_id` | 被豁免条目（业务键） | 是 |
| `gap_type` | 缺口类型（白名单项） | 是 |
| `waived_evidence_fields` | 被豁免的证据字段清单 | 是 |
| `missing_evidence` | 原缺口清单（豁免前快照） | 是 |
| `rationale` | 人类理由 | 是 |
| `granted_by` | `human:<角色>`；**不得是工具** | 是 |
| `granted_at` | 批准时间 ISO-8601 | 是 |
| `expires_at` | 有效期截止（ISO 日期）；**缺失视为非法记录** | 是 |
| `conditions` | 豁免维持条件（人类可写约束） | 否 |
| `waiver_status` | `Requested` / `Active` / `Expired` / `Revoked` | 是 |
| `prior_record_id` | 引用原缺口登记记录（如适用） | 否 |
| `state_unchanged` | 豁免期间条目 `state` 不变（恒 true，审计断言） | 是 |

- 流程：人类提交申请（Requested）→ 责任人类审核者批准（Active）→ 到期（Expired，自动）或条件失效/人类撤销（Revoked）；
- 到期/失效处理：**不产生任何批准**；缺口恢复原状（Pending 语义）；人类复核后可重新申请（新记录，append-only）或 REJECT/REVOKE（第 4 节 #8）；
- L35 测试接口：过期 waiver 存在时任何批准判定 → FAIL（L03 7.2 第 5 条）。

### 7.6 审批角色与可读范围

- 审批：**仅责任人类审核者**；工具/验证器不得批准（granted_by 非 human → 伪造，L35/L49 拒绝）；
- 可读：审计与验证器（仅读缺口状态）；waiver 不改变 L17/L18 守卫结论——豁免中的 `Candidate` 仍禁止被游戏逻辑引用二进制。

## 8. 撤销规则详述（→ `Revoked`）

### 8.1 触发与证据

- 触发（L03 4.6）：哈希不再匹配真实文件；许可证撤回或来源 URL 失效；发现来源/作者声明冲突或伪造/补猜；审计发现不再满足所在状态证据要求；已授予状态（IntakeEligible/RuntimeRequested/RuntimeApproved/ReleaseApproved）均可撤销。
- 证据：`reason` 必填 + `prior_record_id` **必填**（引用被撤销的原授予/登记记录）+ `evidence_paths` + operator + occurred_at。

### 8.2 Rejected 与 Revoked 的区分

- `Rejected`：从未获得有效授予的条目被拒绝，或申请/登记被驳回（无授予可撤销）；
- `Revoked`：**曾获得授予/登记的条目**因证据被推翻而撤销，保留原证据与原因；
- `ReleaseApproved` → 只能 `Revoked`（不可 `Rejected`，L03 4.2 禁止边）。

### 8.3 恢复方式

- T7：`Revoked → Candidate` **仅人类复核**（重新开始证据链）；禁止直接复活到 IntakeEligible/RuntimeRequested/RuntimeApproved/ReleaseApproved（L03 5.7）。

## 9. 审计记录规范（统一，append-only）

- 全部拒绝/撤销/豁免/重新登记记录只追加、不覆盖、不删除（基线 2.5；L03 6.3）；
- 同一 `canonical_asset_id` 的完整链条可从审计记录重建；任何状态必须是 L03 矩阵链上可达状态；
- 伪造检测（L35 覆盖）：REJECT/REVOKE 缺 reason、REVOKE 缺 prior_record_id、RE_REGISTER operator 非 human、waiver granted_by 非 human、waiver 缺 expires_at、过期 waiver 伴随批准 → 一律失败；
- 记录字段以 L03 6.1 为准，本文第 7.5 节豁免记录字段为扩展（不改变 L03 字段语义）。

## 10. 下游接口（只写接口，不实现、不修改）

| 下游 | 接口边界 |
|---|---|
| L35 状态迁移审计 | 读本文件第 4 节矩阵与第 7.4 节定理；"过期 waiver + 批准" → FAIL；REJECT/REVOKE 审计完整性校验（L03 7.2） |
| L36 LocalDevelopmentReady 守卫 | C1—C10 不看 waiver；waiver 不得满足任何条件（L03 7.3） |
| L37 晋级请求 Schema | `current_state` 必须为 IntakeEligible 或 RuntimeRequested；waiver 不改变该要求；越级 → 本文件 #7 → Rejected |
| L42 目录编译器 | `IntakeEligible` 授予条件（T1）不参考 waiver；缺任何 P0 证据即不授予 |
| L43 来源审计 / L49 状态复核 | 用本文件第 4 节矩阵统计 Rejected/Pending/WaiverRequested/Revoked 缺口与记录 |
| L47 复核 | 检查是否有记录把 waiver 写成批准 |

## 11. Runtime 延期与未决人工风险（登记，不是通过条件）

| 风险 | 说明 | 责任人 |
|---|---|---|
| 豁免审批 | 只有责任人类审核者可批准；当前无任何豁免记录，本逻辑阶段不产生 | 人类 + L19 流程 |
| A04 人工审核 267 条 Pending | 授权记录未完成；相关条目保持 Pending，不得宣称证据完备 | A04 复核流程（非 D18 章节责任） |
| Revoked → Candidate 重新登记 | 仅人类复核后执行 | 人类 |
| 拒绝/撤销的真实记录写入 | 本文件只定义规则；实际记录由人类复核或 L35/L43 在各自任务中产生 | 人类 / L35 / L43 |
| 重复哈希的 L31 裁定 | 重复先登记挂起（Pending），裁定由 L31 | L31 |

## 12. 机器可校验摘要（L35/L36/L42/L49 直接解析）

```json
{
  "schema_version": "1.0",
  "document": "docs/game/design/common/assets/D18_WAIVER_AND_REJECTION_RULES.md",
  "task_id": "D18-L19",
  "frozen": "2026-08-16",
  "states_unchanged": ["Candidate", "IntakeEligible", "RuntimeRequested", "RuntimeApproved", "ReleaseApproved", "Rejected", "Revoked"],
  "administrative_semantics": {"Pending": "review_status field, not a state", "WaiverRequested": "waiver record status, not a state"},
  "trigger_matrix": [
    {"trigger": "non_distributable", "path": "Rejected", "waivable": false, "note": "rights false/unknown/conflict; L07"},
    {"trigger": "forged_evidence", "path": "Rejected_or_Revoked", "waivable": false, "note": "forged or guessed evidence"},
    {"trigger": "missing_license", "path": "Pending_or_Rejected", "waivable": "unknown/conflict license not waivable", "note": "L04 block; never guess CC0"},
    {"trigger": "source_conflict", "path": "Pending_then_human_decision", "waivable": false, "note": "register conflict append-only first"},
    {"trigger": "duplicate_hash", "path": "Pending_then_L31_then_Rejected", "waivable": false, "note": "register only, never delete"},
    {"trigger": "missing_or_overturned_evidence", "path": "Pending_or_WaiverRequested_or_Rejected_or_Revoked", "waivable": "whitelist evidence gaps only", "note": "missing file is not waivable"},
    {"trigger": "out_of_order_request", "path": "Rejected", "waivable": false, "note": "L37 current_state gate"},
    {"trigger": "waiver_expired", "path": "no_approval_then_Pending_or_Rejected_or_Revoked", "waivable": false, "note": "L35 FAIL on approval with expired waiver"}
  ],
  "waiver_whitelist": ["hash_pending_original_in_external_download_dir", "evidence_path_to_be_archived", "non_rights_descriptor_field_missing", "other_non_rights_non_forged_gap_with_rationale"],
  "waiver_blacklist": ["missing_commercial_authorization", "nc_or_personal_use_or_no_redistribution", "unknown_or_conflicting_license", "forged_or_guessed_evidence", "out_of_order_request", "file_does_not_exist", "any_runtime_or_release_approval_need"],
  "no_go_theorems": [
    "waiver_cannot_override_commercial_authorization_or_rights_fields",
    "waiver_cannot_produce_runtime_or_release_approval_and_cannot_shorten_T1_T2_T3_T4_path"
  ],
  "deadlines": {"waiver_expires_at_required": true, "waiver_expiry_auto_invalidates": true, "rejected_revoked_terminal_no_expiry": true, "pending_no_auto_expiry": true},
  "approval_roles": {"reject": "human_or_validator_with_reason", "revoke": "human_or_validator_with_reason_and_prior_record_id", "waiver_grant": "human_only", "re_register": "human_only"},
  "record_fields": {
    "audit": ["record_id", "canonical_asset_id", "from_state", "to_state", "event", "operator", "occurred_at", "evidence_paths", "reason", "prior_record_id", "result_state"],
    "waiver": ["waiver_id", "canonical_asset_id", "gap_type", "waived_evidence_fields", "missing_evidence", "rationale", "granted_by", "granted_at", "expires_at", "conditions", "waiver_status", "prior_record_id", "state_unchanged"]
  },
  "append_only": true,
  "no_real_entry_state_modified": true
}
```

## 13. 明确未宣称

- 本文件没有创建任何豁免/拒绝/撤销记录，没有修改任何真实条目状态（`state`、Registry、catalog、manifest 均未动）；
- 没有任何豁免成为 `RuntimeApproved`/`ReleaseApproved` 或替代 L42 准入授予（当前无任何豁免记录）；
- 未修改 ContentRegistry、`asset_state`、存档 Schema、`contracts/`、`server/`、`web/`；不引用旧 D18-A/S/R 结论作为通过依据（基线第 4 节）。

## 14. 本任务实际执行的验证（与结果 JSON 一致）

1. **规则一致性机器检查**（Python 一次性脚本，临时目录不落仓库）：解析第 12 节 JSON；断言 states_unchanged 与 L03 状态枚举一致、trigger_matrix 八条触发与第 4 节表一一对应、白名单/黑名单无交集、双 no-go 定理字段存在、deadlines/approval_roles/record_fields 与第 3/7/9 节一致、waiver 记录 `state_unchanged=true`；
2. **与 L03 状态机交叉核对**（Python 只读解析）：T5/T6/T7 与 L03 4.4 矩阵一致（Rejected/Revoked 终态、Revoked→Candidate 仅 RE_REGISTER+human、ReleaseApproved 只可 REVOKE）；L03 7.2 第 5 条"过期豁免+批准 → FAIL"已纳入本文件 #8 与 no-go 定理 2；
3. **真实事实只读核对**：准入目录 `known_gaps` 非空 1,865 条、`local_development_ready` BLOCKED 0/0、A04 Pending 267 与第 1.3/11 节引用一致；本任务未修改任何条目；
4. **相对链接存在性检查**（Python，同 L01/L03 方法）；
5. `git diff --check`；
6. `python tools/sync_logic_task_status.py`。

## 15. 链接索引

- 路线与协议：[D18—D24 纯逻辑完成路线](../../../D18_D24_VISUAL_PLAYABLE_ROADMAP.md)、[单任务逻辑开发协议](../../../LOGIC_TASK_PROTOCOL.md)
- 基线与状态机：[D18-L01 逻辑治理基线](../D18_LOGIC_GOVERNANCE_BASELINE.md)、[L03 状态机规范](D18_ASSET_STATE_MACHINE.md)、[D18 章节任务卡](../../../AGENT_DEVELOPMENT_CHAPTER_10_D18.md)、[D18 详细提示词](../../../AGENT_DEVELOPMENT_CHAPTER_10_D18_PROMPTS.md)
- 平行词典：[L11 船舶港口](D18_TAXONOMY_SHIPS_PORT.md)、[L12 角色](D18_TAXONOMY_CHARACTERS.md)、[L13 生态](D18_TAXONOMY_ECOLOGY.md)、[L14 UI/音频/材质](D18_TAXONOMY_UI_AUDIO.md)
- 只读数据（本任务不修改）：[准入目录](d18_prefab_candidate_intake_catalog.json)、[本地就绪目录](d18_local_development_ready_catalog.json)
- 状态总览：[逻辑任务状态总览](../../../LOGIC_TASK_STATUS.md)
