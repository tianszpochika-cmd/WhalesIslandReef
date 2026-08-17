# D18 资源状态机规范（D18-L03）

> 冻结日期：2026-08-16
> 任务：`D18-L03`（波次 1，并行；与 L02、L04—L20 并行执行）
> 唯一写入范围：本文件、`game/tools/results/D18/D18-L03/`、`docs/game/design/common/handoffs/D18/D18-L03_LOGIC_HANDOFF.md`、当日记忆日志。
> 本文件是 [D18-L01 基线](../D18_LOGIC_GOVERNANCE_BASELINE.md) 第 2 节冻结语义的**完整展开**：状态图、表驱动迁移矩阵、审计记录格式、逐状态撤销规则与下游读取范围，以及 L02/L35/L36/L37 的字段与测试接口（只写接口规格，不实现、不修改这些任务的文件）。
> 读取顺序：D18—D24 路线 → LOGIC_TASK_PROTOCOL → D18 章节任务卡 → L01 基线 → 本文。

## 1. 范围与定位

### 1.1 本文定义什么

1. 七个资源状态的精确定义、证据要求、唯一写者、允许/禁止迁移、撤销规则、下游可读取范围；
2. 表驱动迁移矩阵（Markdown 表 + 机器可读 JSON 镜像，二者必须一致；L35 按此实现）；
3. 批准/拒绝/撤销的可审计记录格式与事件→记录映射（append-only）；
4. 与 L02（字段枚举）、L35（迁移审计）、L36（LocalDevelopmentReady 防线）、L37（晋级请求 Schema）的字段与测试接口规格；
5. 当前纯逻辑阶段与未来 Runtime 阶段的边界。

### 1.2 本文不做什么（红线）

- 不修改任何 Schema、验证器、真实 catalog、Unity、Prefab、场景、Registry、资产文件或既有 A/S/R 历史材料；
- 不授予任何真实资产 `RuntimeApproved` / `ReleaseApproved`（本逻辑阶段禁止，二者当前均为 0 个持有者）；
- 不运行 Player、不导入资源、不访问网络、不伪造审批记录。

## 2. 术语

| 术语 | 定义 |
|---|---|
| 状态（state） | 资源逻辑记录 `state` 字段的取值，严格枚举如下七个：`Candidate`、`IntakeEligible`、`RuntimeRequested`、`RuntimeApproved`、`ReleaseApproved`、`Rejected`、`Revoked`。禁止任何其他取值（含 `LocalDevelopmentReady`——它不是状态，是守卫结论）。 |
| 迁移（transition） | 一个资源记录在一条**可审计事件**驱动下从 `from_state` 变为 `to_state`。任何状态变化必须由事件触发并留下审计记录。 |
| 授予（grant） | 向资产写入一个新状态的操作（如 `INTAKE_APPROVE` 授予 `IntakeEligible`）。只有该状态的唯一写者可以授予。 |
| 审计记录（audit record） | 每次迁移产生的 append-only 记录（字段见第 6 节）。禁止静默覆盖、删除或改写历史记录。 |
| 豁免（waiver） | L19 定义的临时豁免流程（`D18_WAIVER_AND_REJECTION_RULES.md` 交付物）。豁免有有效期；**过期豁免不得产生任何批准**，只能触发 `REJECT`/`REVOKE` 并记录原因。 |

## 3. 状态定义总表

| 状态 | 精确定义 | 证据要求 | 唯一写者 | 本逻辑阶段可授予？ | 下游可读取范围 |
|---|---|---|---|---|---|
| `Candidate` | 有来源入口或磁盘文件，但证据链不完整或未通过准入校验；游戏逻辑**不得直接引用** Candidate 二进制 | 来源入口记录（source_id、source_url、original_filename、download_date 或 null）；无需完整许可/哈希 | 来源目录任务（L21—L24）登记；L42 编译器维护；人类复核写 Rejected/Revoked | 是（登记） | 全部验证器与审计（只读分析）；消费任务与游戏逻辑禁止引用其二进制 |
| `IntakeEligible` | 证据完备、可被指定消费任务用于**受控工作场景/Prefab** 的“准入”状态；**不是资源状态**，不表示导入成功、Prefab 可用、运行时可用或发布可用 | 许可证规范化（L04）+ 来源验证（L05）+ 哈希/文件存在（L06）+ 四项权利（L07）+ 路径边界（L08）+ 归属（L09）+ 包边界（L10）+ 内容映射（L15）+ 资源键（L16）全部通过 | L42 目录编译器（依据验证器通过结果授予） | 是（由准入工具授予） | 消费任务（D19—D22 受控工作场景）、L17/L18 守卫；仍禁止正式场景/运行时直接消费 |
| `RuntimeRequested` | 消费任务已建立受控工作场景/Prefab 引用并提交运行时晋级申请；**是申请，不是批准**，不产生任何运行时权利 | L37 申请数据（asset_id、当前状态、目标使用场景、描述器、碰撞/LOD/动画需求、审批人、风险、回退）+ 受控消费引用 + 用途 + 回退声明 | 消费任务（D19—D22 对应任务） | 是（仅申请数据可写；**不产生批准**） | L35/L36/L37 审计工具只读；无运行时消费者 |
| `RuntimeApproved` | 运行时消费证据完备（真实 Prefab/场景消费者、保存/恢复、回退入口）；**不表示 ReleaseApproved** | 真实运行时证据（未来 Runtime 阶段门禁，D18-S03 等价物） | 未来 Runtime 阶段门禁 | **否（本阶段禁止）** | L18/L46 只读；Runtime 阶段消费者 |
| `ReleaseApproved` | 可随游戏分发的发布审批 | 责任人类审核者确认（含 Steam/外部分发结论） | 责任人类审核者（工具从不批准） | **否（仅人类；本阶段禁止）** | 全员只读 |
| `Rejected` | 拒绝终态：记录拒绝原因、证据路径、操作者，可审计 | 拒绝原因必填（reason 非空） | 人类复核；验证器可标记但必须留原因 | 是（记录） | 审计 |
| `Revoked` | 撤销状态：原证据被推翻或授权撤回；保留原证据与撤销原因 | 撤销原因必填；必须引用被撤销的原授予记录 | 人类复核；验证器可标记但必须留原因 | 是（记录） | 审计；仅人类复核后可重新登记为 Candidate |

**关键语义（禁止混淆）**：

- `IntakeEligible` 只表示“证据完备可受控使用”，**不表示** Unity 导入成功、Prefab 可用、运行时可用或发布可用；
- `RuntimeApproved` 只表示“运行时消费证据完备”，**不表示** ReleaseApproved、Steam Ready 或可分发；
- `LocalDevelopmentReady` 是 L36 守卫的**结论**，不是状态；其最小逻辑条件见第 7.3 节，未来映射到 RuntimeApproved 的授予条件（基线 2.7）。

## 4. 迁移矩阵（表驱动，唯一事实源）

### 4.1 允许迁移总表

| # | from | to | 事件 | 条件 | 写者 | 审计记录 | 逻辑阶段可执行 |
|---|---|---|---|---|---|---|---|
| T1 | Candidate | IntakeEligible | INTAKE_APPROVE | 证据完备：L04—L10/L15/L16 全部验证器通过（唯一路径，不可跳过） | L42 目录编译器 | 是 | 是 |
| T2 | IntakeEligible | RuntimeRequested | RUNTIME_REQUEST | L37 申请数据 + 受控消费引用 + 用途 + 回退声明 | 消费任务（D19—D22） | 是 | 是 |
| T3 | RuntimeRequested | RuntimeApproved | RUNTIME_APPROVE | 未来 Runtime 阶段门禁（D18-S03 等价物）真实运行时证据 | Runtime 阶段门禁 | 是 | **否（本逻辑阶段禁止）** |
| T4 | RuntimeApproved | ReleaseApproved | RELEASE_APPROVE | 责任人类审核者确认（含 Steam/外部分发结论） | 责任人类审核者 | 是 | **否（本逻辑阶段禁止）** |
| T5 | 任意可拒绝状态 | Rejected | REJECT | 满足拒绝条件（4.5 节），reason 必填 | 人类复核；验证器可标记但留原因 | 是 | 是（仅记录，无授予） |
| T6 | 任意可撤销状态 | Revoked | REVOKE | 满足撤销条件（4.6 节），reason 必填，必须引用原授予记录 | 人类复核；验证器可标记但留原因 | 是 | 是（仅记录，无授予） |

> 通配符口径（基线 2.2 的精确化，不改变基线意图）：**任意可拒绝状态** = Candidate、IntakeEligible、RuntimeRequested、RuntimeApproved（活动状态中除 ReleaseApproved 外，不含 Rejected/Revoked）；**任意可撤销状态** = Candidate、IntakeEligible、RuntimeRequested、RuntimeApproved、ReleaseApproved（全部非终态）。Rejected、Revoked 是终态：除 T7 外无出边，不被再次 REJECT/REVOKE——基线 2.2 中"任意状态→Rejected/Revoked"按终态语义解释为"任意非终态状态"，否则会产生 Rejected→Rejected 自环等无意义迁移。
| T7 | Revoked | Candidate | RE_REGISTER | 人类复核后重新登记（重新开始证据链） | 责任人类复核者 | 是 | 是（仅人类） |

### 4.2 禁止迁移（显式列举，违反即失败）

| from | to | 禁止原因 |
|---|---|---|
| Candidate | RuntimeRequested | 越级申请：无准入证据 |
| Candidate | RuntimeApproved | **无直达路径**：必须依次经过 IntakeEligible → RuntimeRequested |
| Candidate | ReleaseApproved | **无直达路径**：必须经过 IntakeEligible → RuntimeRequested → RuntimeApproved → 人类审核 |
| IntakeEligible | RuntimeApproved | 越级：缺 RuntimeRequested 申请 |
| IntakeEligible | ReleaseApproved | 越级：缺申请与运行时证据 |
| RuntimeRequested | ReleaseApproved | 越级：缺 RuntimeApproved 与人类审核 |
| ReleaseApproved | Rejected | 发布已批准后不可直接标记拒绝；只能 REVOKE |
| Rejected | 任何状态 | Rejected 是终态，无出边 |
| 任何状态 | ReleaseApproved | 除 T4（仅 RuntimeApproved + 人类审核）外一律禁止 |
| 任何状态 | IntakeEligible / RuntimeRequested / RuntimeApproved | 除矩阵 T1—T3 外一律禁止（禁止自选迁移、跳过中间状态、“越级后再补”） |

**可达性定理（本规范的核心保证）**：

- 从 `Candidate` 出发到达 `RuntimeApproved` 的**唯一路径**是 `T1 → T2 → T3`；
- 从 `Candidate` 出发到达 `ReleaseApproved` 的**唯一路径**是 `T1 → T2 → T3 → T4`；
- 因此**不存在**任何从 `Candidate` 直接到 `RuntimeApproved` / `ReleaseApproved` 的路径；`IntakeEligible`、`RuntimeRequested` 同理不存在直达发布路径。
- 上述性质由 L35 表驱动审计自动校验（测试接口见 7.2），本任务的验证脚本亦按 4.4 的 JSON 矩阵复验。

### 4.3 事件字典

| 事件 | 含义 | 必须产生审计记录 | 逻辑阶段允许发生 |
|---|---|---|---|
| INTAKE_APPROVE | 授予 IntakeEligible | 是 | 是（L42） |
| RUNTIME_REQUEST | 提交运行时晋级申请 | 是 | 是（消费任务） |
| RUNTIME_APPROVE | 授予 RuntimeApproved | 是 | **否** |
| RELEASE_APPROVE | 授予 ReleaseApproved | 是 | **否** |
| REJECT | 拒绝（→ Rejected） | 是（reason 必填） | 是（记录） |
| REVOKE | 撤销（→ Revoked） | 是（reason 必填 + 引用原记录） | 是（记录） |
| RE_REGISTER | 人类复核后 Revoked → Candidate 重新登记 | 是 | 是（仅人类） |

### 4.4 机器可读矩阵（与 4.1 表一致，L35 应以此实现表驱动审计）

```json
{
  "schema_version": "1.0",
  "document": "docs/game/design/common/assets/D18_ASSET_STATE_MACHINE.md",
  "states": [
    "Candidate", "IntakeEligible", "RuntimeRequested",
    "RuntimeApproved", "ReleaseApproved", "Rejected", "Revoked"
  ],
  "events": [
    "INTAKE_APPROVE", "RUNTIME_REQUEST", "RUNTIME_APPROVE",
    "RELEASE_APPROVE", "REJECT", "REVOKE", "RE_REGISTER"
  ],
  "allowed_transitions": [
    {"from": "Candidate", "event": "INTAKE_APPROVE", "to": "IntakeEligible", "writer": "L42_catalog_compiler", "audit_required": true, "logic_phase": true},
    {"from": "IntakeEligible", "event": "RUNTIME_REQUEST", "to": "RuntimeRequested", "writer": "consumer_task", "audit_required": true, "logic_phase": true},
    {"from": "RuntimeRequested", "event": "RUNTIME_APPROVE", "to": "RuntimeApproved", "writer": "runtime_phase_gate", "audit_required": true, "logic_phase": false},
    {"from": "RuntimeApproved", "event": "RELEASE_APPROVE", "to": "ReleaseApproved", "writer": "human_responsible_reviewer", "audit_required": true, "logic_phase": false},
    {"from": "<any_rejectable>", "event": "REJECT", "to": "Rejected", "writer": "human_or_validator", "audit_required": true, "logic_phase": true},
    {"from": "<any_revocable>", "event": "REVOKE", "to": "Revoked", "writer": "human_or_validator", "audit_required": true, "logic_phase": true},
    {"from": "Revoked", "event": "RE_REGISTER", "to": "Candidate", "writer": "human_reviewer", "audit_required": true, "logic_phase": true}
  ],
  "forbidden_pairs": [
    {"from": "Candidate", "to": "RuntimeRequested"},
    {"from": "Candidate", "to": "RuntimeApproved"},
    {"from": "Candidate", "to": "ReleaseApproved"},
    {"from": "IntakeEligible", "to": "RuntimeApproved"},
    {"from": "IntakeEligible", "to": "ReleaseApproved"},
    {"from": "RuntimeRequested", "to": "ReleaseApproved"},
    {"from": "ReleaseApproved", "to": "Rejected"},
    {"from": "Rejected", "to": "<any>"},
    {"from": "<any>", "to": "ReleaseApproved"}
  ]
}
```

- `<any>` 展开为全部 7 个状态（用于 forbidden_pairs）；`<any_rejectable>` 展开为 Candidate、IntakeEligible、RuntimeRequested、RuntimeApproved；`<any_revocable>` 展开为 Candidate、IntakeEligible、RuntimeRequested、RuntimeApproved、ReleaseApproved。Rejected/Revoked 为终态，除 T7 外无出边。
- 展开后：`forbidden_pairs` 与 `allowed_transitions` 必须互斥，唯一显式例外是 `RuntimeApproved → ReleaseApproved`（T4 显式允许边，见下条）；本任务验证脚本自动复验（见第 10 节）。
- 注意：`forbidden_pairs` 最后一条 `{"from":"<any>","to":"ReleaseApproved"}` 的本意是"除 T4 外任何条目不得进入 ReleaseApproved"；`RuntimeApproved → ReleaseApproved` 由 `allowed_transitions` **显式列出**，审计工具必须优先承认显式允许边，再拒绝未列出的边（对非显式边一律拒绝）。

### 4.5 拒绝条件（→ Rejected，reason 必填）

1. 缺许可证，或许可证据文件不存在/不可读；
2. 来源冲突：同一规范化路径多来源声称、来源页与包内许可矛盾；
3. 重复文件：哈希相同但 canonical_id 不同（先登记，不删除，L31 裁定）；
4. 不可分发：NC、个人使用限制、明确禁止再分发/随游戏分发；
5. 越级申请：无消费消费者或无 `RuntimeRequested` 却申请 `RuntimeApproved`；
6. 证据被推翻：哈希不匹配、文件缺失、来源页内容变更；
7. 豁免（waiver）过期或豁免条件不再成立（L19 流程，人类复核）。

### 4.6 撤销条件（→ Revoked，reason 必填，必须引用原授予记录）

1. 哈希不再匹配真实文件；
2. 许可证撤回或来源 URL 失效；
3. 发现来源/作者声明冲突或伪造、补猜证据；
4. 审计发现该条目不再满足所在状态的证据要求；
5. 已授予状态（IntakeEligible / RuntimeRequested / RuntimeApproved / ReleaseApproved）均可被撤销；撤销后原证据与原因全部保留，重新登记只能走 `Revoked → Candidate`（人类复核）。

## 5. 逐状态细则

### 5.1 Candidate

- **证据**：来源入口记录即可（source_id、source_url、original_filename、download_date 或 null）；无需完整许可/哈希。
- **唯一写者**：来源目录任务（L21—L24）登记；L42 编译器维护；人类复核写 Rejected/Revoked。
- **允许迁移**：T1（→ IntakeEligible）、T5（→ Rejected）、T6（→ Revoked，仅当来源记录被撤回/伪造时）。
- **禁止迁移**：T3 之前的一切出边（→ RuntimeRequested / RuntimeApproved / ReleaseApproved）。
- **撤销规则**：无授予可撤销；若来源登记记录被推翻（伪造来源/下载日期），走 REVOKE 并引用登记记录。
- **下游可读取范围**：全部验证器与审计只读；**游戏逻辑与任何消费任务禁止引用 Candidate 二进制**（L17 守卫）。

### 5.2 IntakeEligible

- **证据**：L04（许可规范化）、L05（来源）、L06（哈希/文件存在）、L07（四项权利）、L08（路径边界）、L09（归属）、L10（包边界）、L15（内容映射）、L16（资源键）全部通过；由 L42 编译器依据验证器结果授予。
- **唯一写者**：L42 目录编译器。
- **允许迁移**：T2（→ RuntimeRequested，消费任务申请）、T5、T6。
- **禁止迁移**：→ RuntimeApproved / ReleaseApproved（直达均禁止）。
- **撤销规则**：任一证据失效（哈希不匹配、许可撤回、伪造）→ REVOKE（4.6）。
- **下游可读取范围**：消费任务（D19—D22）可读元数据并在**受控工作场景/Prefab**中使用文件；L17/L18 守卫只读。**不表示** Unity 导入成功、正式场景接入、运行时消费者、LocalDevelopmentReady、ReleaseApproved 或 Steam Ready。

### 5.3 RuntimeRequested

- **证据**：L37 申请数据（完整字段见 7.4）+ 受控消费引用 + 用途 + 回退声明。
- **唯一写者**：消费任务（D19—D22 对应任务）。
- **允许迁移**：T3（→ RuntimeApproved，未来 Runtime 阶段）、T5、T6。
- **禁止迁移**：→ ReleaseApproved。
- **撤销规则**：申请撤回/证据失效 → REVOKE；申请被驳回 → REJECT。
- **下游可读取范围**：L35/L36/L37 审计工具只读；**无任何运行时权利**，禁止真实 Player/Prefab 消费链声称可用。

### 5.4 RuntimeApproved

- **证据**：真实运行时证据（真实 Prefab/场景消费者、保存/恢复、回退入口）——未来 Runtime 阶段门禁（D18-S03 等价物）验证。
- **唯一写者**：未来 Runtime 阶段门禁；**本逻辑阶段任何工具不得授予，当前持有者 0**。
- **允许迁移**：T4（→ ReleaseApproved，人类审核）、T5、T6。
- **禁止迁移**：其他一切；**不表示 ReleaseApproved**。
- **撤销规则**：运行时证据失效/回退缺失 → REVOKE。
- **下游可读取范围**：L18/L46 只读；Runtime 阶段消费者。

### 5.5 ReleaseApproved

- **证据**：责任人类审核者确认（含 Steam/外部分发结论），append-only 记录。
- **唯一写者**：责任人类审核者；**工具从不批准**。
- **允许迁移**：T6（→ Revoked）。**禁止迁移**：→ Rejected、→ 任何其他状态。
- **撤销规则**：发现伪造/权利撤回/法律风险 → REVOKE（人类）。
- **下游可读取范围**：全员只读。当前持有者 0。

### 5.6 Rejected

- **证据**：reason 必填（拒绝原因、证据路径、操作者、日期）。
- **唯一写者**：人类复核；验证器可标记但必须留原因。
- **允许迁移**：无（终态，亦不被再次 REJECT/REVOKE）。被拒绝条目如确需重新进入管线，只能由人类从来源重新登记为新 Candidate（新 canonical_asset_id 或人类裁决），**不自动恢复**。
- **下游可读取范围**：审计。

### 5.7 Revoked

- **证据**：reason 必填 + 必须引用被撤销的原授予/登记记录（prior_record_id）。
- **唯一写者**：人类复核；验证器可标记但必须留原因。
- **允许迁移**：T7（→ Candidate，人类复核后重新登记，重新开始证据链）。
- **禁止迁移**：→ IntakeEligible / RuntimeRequested / RuntimeApproved / ReleaseApproved（撤销后不能直接复活到任何高级状态）。
- **下游可读取范围**：审计；只有人类复核可重新登记。

## 6. 审计记录规范（每次批准、拒绝、撤销都有可审计记录）

### 6.1 审计记录字段（语义契约，L35 校验）

| 字段 | 语义 | 必填 | 禁止/口径 |
|---|---|---|---|
| `record_id` | 记录唯一 ID | 是 | 仓库内唯一，格式 `TR-<canonical_asset_id>-<seq>` |
| `canonical_asset_id` | 规范资源 ID（业务键） | 是 | 不得用磁盘路径或显示名 |
| `from_state` | 迁移前状态 | 是 | 必须是第 3 节枚举 |
| `to_state` | 迁移后状态 | 是 | 必须是第 3 节枚举 |
| `event` | 事件名 | 是 | 必须是 4.3 事件枚举 |
| `operator` | 操作者 | 是 | `tool:<工具名>` 或 `human:<角色>`；RELEASE_APPROVE 的 operator 必须是 human |
| `occurred_at` | 事件时间 | 是 | ISO-8601 带时区 |
| `evidence_paths` | 证据文件相对路径 | 是 | 仓库内相对路径；禁止绝对路径泄露 |
| `reason` | 原因 | REJECT/REVOKE 必填；其余可空 | 非空字符串 |
| `prior_record_id` | 被引用的原记录 | REVOKE 必填（原授予/登记记录）；RE_REGISTER 建议填 | 必须指向已存在记录 |
| `result_state` | 迁移后的实际状态 | 是 | 必须与 to_state 一致 |

### 6.2 事件 → 记录映射（全部必须落记录）

| 事件 | 记录类型 | reason 必填 | prior_record_id 必填 |
|---|---|---|---|
| INTAKE_APPROVE | 授予 | 否 | 否 |
| RUNTIME_REQUEST | 申请 | 否 | 否 |
| RUNTIME_APPROVE | 授予 | 否 | 否（可引用申请记录） |
| RELEASE_APPROVE | 授予 | 否 | 是（引用 RuntimeApproved 授予记录） |
| REJECT | 拒绝 | **是** | 否 |
| REVOKE | 撤销 | **是** | **是** |
| RE_REGISTER | 重新登记 | 是（登记说明） | 建议填 |

### 6.3 append-only 规则

- 审计记录只追加、不覆盖、不删除；工具不得静默清除或改写历史（基线 2.5）；
- 同一 canonical_asset_id 的完整迁移链可从审计记录重建；**任何状态必须是链上可达状态**；
- 伪造检测：审计工具发现 RELEASE_APPROVE 的 operator 非 human、REVOKE 缺 prior_record_id、REJECT/REVOKE 缺 reason、或迁移不在矩阵中 → 一律判定伪造并失败（L35 测试覆盖，见 7.2）。

## 7. 与其他 L 任务的字段与测试接口（本任务只写接口，不抢改）

> 本节是 L02/L35/L36/L37 的实现依据。这些任务的写入范围归属不变；本任务不修改任何相关文件。字段语义冲突时以 L01 基线第 6 节与本文为准。

### 7.1 L02（`tools/schemas/d18_logic_asset_record.schema.json`）接口

- `state` 字段必须是本规范第 3 节的七状态枚举；任何其他取值（如 `Approved`、`LocalDevelopmentReady`、`Pending`）必须校验失败；
- `evidence_paths` 必须是仓库内相对路径（禁止绝对路径、`..` 越界）；
- 非法夹具至少覆盖：缺许可、坏 hash、**越级状态**（state 直接写 RuntimeApproved/ReleaseApproved）、空来源、路径越界；
- 注意：L02 只校验**记录形态**（字段、枚举、路径）；**迁移链合法性**由 L35 校验，L02 不负责。

### 7.2 L35（`tools/d18_status_transition_audit.py`）测试接口（必须覆盖清单）

表驱动审计按 4.4 JSON 矩阵实现，读入 allowed_transitions 与 forbidden_pairs，展开 `<any>`/`<any_non_release>`，逐条产出允许/禁止判定；工具只读，不改变任何真实记录状态。测试必须覆盖：

1. T1—T7 每条允许迁移 → PASS；
2. 4.2 表内每条禁止迁移 → FAIL（含 Candidate→RuntimeApproved、Candidate→ReleaseApproved 越级）；
3. `IntakeEligible → RuntimeApproved`、`RuntimeRequested → ReleaseApproved` → FAIL；
4. Revoked 恢复：`Revoked → Candidate` 仅当事件=RE_REGISTER 且 operator 为 human → PASS；`Revoked → IntakeEligible` / `Revoked → RuntimeApproved` → FAIL；
5. 过期 waiver：豁免已过期/失效 → 任何批准判定 FAIL，必须引导 REJECT/REVOKE（reason 必填）；
6. ReleaseApproved 伪造：RELEASE_APPROVE 的 operator 非 human、或 from 非 RuntimeApproved → FAIL；
7. 审计完整性：REJECT/REVOKE 缺 reason → FAIL；REVOKE 缺 prior_record_id → FAIL；
8. 未知事件 / 未知状态 / 矩阵外迁移 → FAIL；
9. 对真实记录只读：运行工具前后真实记录状态不变。

### 7.3 L36（`tools/d18_local_development_guard.py`）接口

`LocalDevelopmentReady` 最小逻辑条件（守卫结论，非状态）：

| 条件 | 内容 | 对应证据 |
|---|---|---|
| C1 | 来源完整 | L21—L24 来源目录记录 |
| C2 | 许可规范化且非未知/冲突/NC | L04 |
| C3 | 来源验证通过 | L05 |
| C4 | 哈希存在且文件存在 | L06 |
| C5 | 四项权利全部显式为真 | L07 |
| C6 | 分类命中冻结词典 | L11—L14 |
| C7 | content_ids 存在且不反向依赖候选 | L15 |
| C8 | ResourceKey 唯一稳定 | L16 |
| C9 | 描述器完整 | L25—L29 |
| C10 | 状态链合法（第 4 节矩阵）且当前非 Rejected/Revoked；本逻辑阶段不得处于 RuntimeApproved/ReleaseApproved（持有者 0） | L35 审计 |

- 工具必须输出缺失条件清单（C1—C10 中哪些不满足）；缺任一条件 → 结论不是 Ready；
- **旧候选文件、旧 Player 截图、历史 R01—R03 的 PASS/PARTIAL 记录均不得满足任何条件**；
- Ready 结论**不等于** Unity 导入、Prefab 可用、运行时消费者、性能通过或 ReleaseApproved。

### 7.4 L37（`tools/schemas/d18_runtime_promotion_request.schema.json`）字段语义

| 字段 | 语义 |
|---|---|
| `asset_id` | 被申请资产（业务键） |
| `current_state` | 申请时状态；**必须为 IntakeEligible 或 RuntimeRequested**（Candidate 直接申请 → 非法夹具） |
| `target_usage_scene` | 目标使用场景（受控工作场景/Prefab 标识） |
| `descriptors` | 网格/材质/动画/UI 音频描述器引用（L25—L29） |
| `requirements` | 碰撞/LOD/动画需求声明 |
| `approver` | 审批人（human 角色）；**不得是工具** |
| `risks` | 风险清单 |
| `rollback` | 回退声明（必填） |
| `submitted_at` | 申请时间 ISO-8601 |

- Schema 只能表达**申请**；**禁止任何字段/机制表达 RuntimeApproved/ReleaseApproved 授予**；
- 非法夹具至少覆盖：current_state=Candidate（越级请求）、缺 rollback、target=ReleaseApproved（非法目标状态）、缺 evidence 引用；
- 合法申请在逻辑阶段只产生 `RUNTIME_REQUEST` 审计记录，绝不产生批准。

## 8. 逻辑阶段与 Runtime 阶段边界

| 维度 | 当前逻辑阶段（D18-L01—L50） | 未来 Runtime 阶段（R25+，另行建立） |
|---|---|---|
| 可达最高状态 | `RuntimeRequested`（仅申请数据 + 审计记录） | `RuntimeApproved`（真实运行时证据）→ 人类审核后 `ReleaseApproved` |
| 授予动作 | 允许：INTAKE_APPROVE（L42）、RUNTIME_REQUEST（消费任务）、REJECT/REVOKE/RE_REGISTER（记录） | 允许：RUNTIME_APPROVE、RELEASE_APPROVE |
| 禁止 | 一切 Player/PlayMode/导入/Prefab/场景/Canvas/Build Settings/截图/性能/网络；一切 RUNTIME_APPROVE/RELEASE_APPROVE 写入 | — |
| 验证手段 | 编译、单元测试、Schema 校验、静态审计、确定性模拟 | 真实 Player、导入验证、保存/恢复、碰撞/LOD/性能、视觉 QA、人工审核 |
| 入口条件 | — | D18—D24 全部 P0 任务 LOGIC_PASS + 所有 RUNTIME_DEFERRED 有测试清单 + 用户明确要求（路线第 7 节） |

- 本阶段任何工具/文档/结果 JSON 中出现 RUNTIME_APPROVE / RELEASE_APPROVE 授予或 `RuntimeApproved`/`ReleaseApproved` 状态持有 → 视为伪造，L18/L35 拒绝；
- 基线第 9 节 Runtime 延期清单（逐文件哈希、Unity 导入网格/材质/动画、Prefab/场景集成、保存恢复、A04 人工审核、数量差异对账）全部保持延期。

## 9. 未决人工许可风险（登记，不是通过条件）

| 风险 | 说明 | 责任人 |
|---|---|---|
| A04 人工审核 267 条 Pending | 授权记录未完成；涉及条目不得宣称证据完备 | A04 复核流程（非 D18 章节责任） |
| ReleaseApproved 授予 | 需要责任人类审核者（含 Steam/外部分发结论）；当前 0 个，工具从不批准 | 人类 |
| 豁免（waiver）审批与过期 | 缺证据条目的豁免只能由人类批准（L19）；过期豁免不得产生批准 | 人类 + L19 流程 |
| Revoked → Candidate 重新登记 | 只能人类复核后执行 | 人类 |
| 来源目录（L21—L24）中下载/许可记录 | 依赖人工确认的下载与许可事实；未确认项保持 Pending，不补猜 | 来源目录任务 + 人类复核 |

## 10. 本任务实际执行的验证（与结果 JSON 一致）

1. **矩阵一致性 + 可达性 + 互斥检查**（Python 一次性脚本，脚本本体在临时目录不落仓库）：
   - 解析 4.4 JSON 矩阵；展开 `<any>`/`<any_rejectable>`/`<any_revocable>`；断言：全部 from/to 在状态枚举内、全部 event 在事件枚举内、allowed 与 forbidden 互斥（唯一显式例外 RuntimeApproved→ReleaseApproved）、Rejected/Revoked 无出边（T7 除外）、4.1 表与 JSON 展开后的 (from,event,to) 集合完全一致；
   - 断言**不存在** Candidate→RuntimeRequested/RuntimeApproved/ReleaseApproved、IntakeEligible→RuntimeApproved/ReleaseApproved、RuntimeRequested→ReleaseApproved 等直达边；BFS 验证从 Candidate 到 ReleaseApproved 的唯一路径必经 IntakeEligible → RuntimeRequested → RuntimeApproved → T4。
2. **相对链接存在性检查**（Python，同 L01 方法）。
3. `git diff --check`（报告全局真实结果，含既有 LF→CRLF 警告）。
4. `python tools/sync_logic_task_status.py`（刷新状态总览）。

## 11. 链接索引

- 路线与协议：[D18—D24 纯逻辑完成路线](../../../D18_D24_VISUAL_PLAYABLE_ROADMAP.md)、[单任务逻辑开发协议](../../../LOGIC_TASK_PROTOCOL.md)
- 任务卡与基线：[D18 章节任务卡](../../../AGENT_DEVELOPMENT_CHAPTER_10_D18.md)、[D18 详细提示词](../../../AGENT_DEVELOPMENT_CHAPTER_10_D18_PROMPTS.md)、[D18-L01 逻辑治理基线](../D18_LOGIC_GOVERNANCE_BASELINE.md)、[D18-L01 交接报告](../handoffs/D18/D18-L01_LOGIC_HANDOFF.md)
- 数据（只读引用）：[准入目录](d18_prefab_candidate_intake_catalog.json)、[本地就绪目录](d18_local_development_ready_catalog.json)
- 状态总览：[逻辑任务状态总览](../../../LOGIC_TASK_STATUS.md)
- 相关任务卡（波次 2，未开始）：L35 状态迁移审计（D18 章节任务卡）、L36 LocalDevelopmentReady 防线（D18 章节任务卡）、L37 Runtime 晋级请求 Schema（D18 章节任务卡）
- L19/L37 交付物（本任务写完后尚不存在，以纯文本引用，不建链接）：`docs/game/design/common/assets/D18_WAIVER_AND_REJECTION_RULES.md`、`tools/schemas/d18_runtime_promotion_request.schema.json`
