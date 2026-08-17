# D19：48 个可直接复制的港口业务纯逻辑任务提示词

> 本文件是 D19 唯一的“复制给 Agent”入口。任务的编号、唯一写入范围和章节完成定义仍以 [D19 任务卡](AGENT_DEVELOPMENT_CHAPTER_11_D19.md) 与 [总任务索引](AGENT_DEVELOPMENT_TASKS.md) 为准。
>
> 开始许可已经具备：D18 当前为 **53/53 `LOGIC_PASS`**，且 D18-L53-C01 已消除 L50 的历史证据分裂。现在只能先执行 **`D19-L01`**；不要把多项编号交给同一个 Agent，也不要同时让多个 Agent 写同一文件。

## 执行顺序

```text
19.0  串行：D19-L01
  ↓ L01 = LOGIC_PASS
19.1  可同步并行：D19-L02、L04、L16、L17、L18
  ↓ 这 5 项全部 LOGIC_PASS
19.2  可同步并行：D19-L03、L05、L06、L23、L24
  ↓ 这 5 项全部 LOGIC_PASS
19.3  可同步并行：D19-L07、L08、L11、L12、L13、L14、L15、L22
  ↓ 这 8 项全部 LOGIC_PASS
19.4  可同步并行：D19-L09、L10、L19、L20
  ↓ 这 4 项全部 LOGIC_PASS
19.5  串行：D19-L21
  ↓ L21 = LOGIC_PASS
19.6  可同步并行：D19-L25 ～ D19-L32
  ↓ 这 8 项全部 LOGIC_PASS
19.7  严格串行：D19-L33 → L34 → L35 → L36 → L37 → L38 → L39 → L40 → L41 → L42
  ↓ 每一项均 LOGIC_PASS
19.8  严格串行门禁：D19-L43 → L44 → L45 → L46 → L47 → L48
```

`可同步并行` 只表示不同 Agent 可以在同一时间工作；每项提示词的“唯一写入范围”绝不可重叠。前置任务有一个不是 `LOGIC_PASS` 时，后续 Agent 必须写真实的 `LOGIC_BLOCKED`，而不是复制类型、猜接口或顺手修复前置任务。

## 所有提示词共同的硬约束

每个任务提示词已经重复了核心要求；本节用于你检查交付是否完整：

1. 当前只做代码、数据、状态机、事件、存档和测试逻辑。禁止启动 Unity Player/PlayMode，也禁止改场景、Prefab、Canvas、输入、碰撞、NavMesh、Builder、Build Settings、外部二进制资源、画面与性能配置。
2. `game/Assets/Scripts/Systems/D19/` 必须保持纯领域逻辑：不能引用 `UnityEngine`、Transform、Input、Canvas、网络、LLM 或其他表现层。D19 只能调用 D2/D3/D4/D9/Save 的已公开入口，不得直接改它们拥有的货币、库存、船只、船员或存档字段。
3. 每项结束必须写：
   - `game/tools/results/D19/D19-Lnn/D19_Lnn_LOGIC_RESULT.json`；
   - `docs/game/design/common/handoffs/D19/D19-Lnn_LOGIC_HANDOFF.md`；
   - 当天 `.workbuddy/memory/YYYY-MM-DD.md`；
   - `python tools/sync_logic_task_status.py` 和 `git diff --check` 的真实结果。
4. 结果 JSON 必须符合 [逻辑任务协议](LOGIC_TASK_PROTOCOL.md)：包括 `task_id`、状态、实际改动、实际命令、`runtime_deferred`、`blockers`、handoff 路径。无 P0 阻断和可复现证据才可写 `LOGIC_PASS`。D18 的 4 个 HDRI 哈希 Pending 只能登记为运行时/资源延期，不是 D19 修复对象。

---

## 波次 19.0：边界冻结（严格串行）

### D19-L01 — 港口领域边界基线

```text
你是 WhalesIslandReef 的纯逻辑开发 Agent。直接完成 D19-L01；这是 D19 的唯一冻结任务。在它 LOGIC_PASS 前，不要创建或修改任何 D19 系统、测试、临时接口，也不要执行旧 D19-Rxx/RERUN 任务。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前阅读：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md、docs/game/AGENT_DEVELOPMENT_TASKS.md、docs/game/AGENT_DEVELOPMENT_CHAPTER_11_D19.md、D18-L50/L53 的当前结果与接口文档，以及 D2/D3/D4/D9/Save 的公开设计和源码入口。

唯一写入范围：docs/game/design/common/D19_LOGIC_BOUNDARY_BASELINE.md、game/tools/results/D19/D19-L01/、docs/game/design/common/handoffs/D19/D19-L01_LOGIC_HANDOFF.md、当天记忆日志和自动生成的 docs/game/LOGIC_TASK_STATUS.md。不得修改 C#、contracts、现有系统、资源、场景、Prefab、UI、旧 R03 文档或任何其它任务文件。

实施：只读盘点 D2 航海、D3 贸易、D4 船舶/船员、D9 港口准备和既有 Save 的公开命令、读模型、事件、ID、拥有者和禁止直写字段；为 D19 冻结 PortVisit、泊位、港口服务、登船、离港、返港、命令幂等、事件和保存投影的边界。逐项明确“D19 可以编排什么、谁仍是唯一裁决者、D19 禁止写什么”，列出可消费的 D18 稳定键与所有 Runtime Deferred 项，并将旧 D19-Rxx/RERUN 标为 SUPERSEDED 历史证据。

成功标准：基线能让后续 Agent 找到真实公开入口和唯一写者；不把旧运行时、截图、FPS 或灰盒场景当作前置；不宣称资源 RuntimeApproved/ReleaseApproved 或游戏已可玩。验证所有引用文件/链接存在，并用 git diff --check 验证写入范围。

完成时按协议写 D19_L01_LOGIC_RESULT.json、handoff 和记忆日志，运行 python tools/sync_logic_task_status.py 与 git diff --check。最终报告真实 LOGIC_PASS/LOGIC_PARTIAL/LOGIC_BLOCKED、冻结的拥有权和结果路径。
```

---

## 波次 19.1：基础契约与目录（可同步并行）

### D19-L02 — 港口生命周期契约

```text
直接完成 D19-L02。前置：D19-L01 的结果 JSON 必须为 LOGIC_PASS；否则只写 LOGIC_BLOCKED 交接，不创建替代类型。阅读项目约定、D19-L01 基线和任务卡。

唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortLifecycleContracts.cs、game/tools/results/D19/D19-L02/、docs/game/design/common/handoffs/D19/D19-L02_LOGIC_HANDOFF.md、当天记忆日志和状态总览。不得修改 D2/D3/D4/D9/Save、contracts、其它 D19 文件或 Unity 内容。

在一个纯 C# 契约文件中定义稳定的港口访问状态、迁移原因、命令/结果 DTO、失败载体和不变式：PortVisit、Docked、Berthed、InService、Boarded、Departed、Returned。每一迁移必须有允许的前态、目标态、原因 ID 与稳定失败码；不得用 bool 堆出互相矛盾的状态，也不得使用坐标、Transform 或 UI 字符串。公共类型写 XML 注释说明拥有者与含义。

成功标准：状态枚举、迁移原因和失败语义能被后续任务直接消费；没有 Unity/网络/LLM 依赖，没有 D2/D3/D4/D9 状态副本。用仓库可用的编译/静态检查验证文件，并在结果中记录实际命令。完成全部协议留痕、状态同步和 diff 检查；如实报告状态与 Runtime Deferred 风险。
```

### D19-L04 — 稳定港口锚点目录

```text
直接完成 D19-L04。前置：D19-L01=LOGIC_PASS。阅读 D19-L01、D18-L46 稳定键接口、D19 任务卡和现有港口/船坞设计文档；绝不读取或写入 Unity 坐标。

唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortAnchorCatalog.cs、game/tools/results/D19/D19-L04/、docs/game/design/common/handoffs/D19/D19-L04_LOGIC_HANDOFF.md、当天记忆日志和状态总览。不得改场景、Prefab、D19Lighting、资源 catalog、任何其它 D19 系统或 D2—D9 文件。

实现纯数据目录：稳定的 port_id、berth_id、service_id、别名与废弃映射，以及查询/拒绝未知键的确定性 API。锚点只是业务语义键，不得包含 Vector3、GameObject、路径、候选资源绝对路径或“存在于场景”的断言。别名解析必须单向、无循环；废弃键必须返回可审计迁移提示。

成功标准：相同输入得到相同规范键；未知、循环别名、跨港泊位和重复键有稳定错误；目录只表达逻辑服务地址。运行可用的 C# 编译/静态检查，完整写结果、handoff、记忆、状态同步和 diff 检查；如实报告 LOGIC_*。
```

### D19-L16 — 港口事件契约

```text
直接完成 D19-L16。前置：D19-L01=LOGIC_PASS。阅读 D19-L01、数据模型、既有事件约定和 D19 任务卡。

唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortEventContracts.cs、game/tools/results/D19/D19-L16/、docs/game/design/common/handoffs/D19/D19-L16_LOGIC_HANDOFF.md、当天记忆日志和状态总览。禁止修改任何事件生产者、Save、其它 D19 文件、contracts、Unity 或表现层。

定义可重放的港口领域事件契约：稳定 event_id、event_type、schema/version、port/ship/actor/command 关联 ID、单调序列、逻辑时间、不可变 payload 与序列化边界。覆盖到港、泊位预留/释放、服务结果、登船/下船、离港、返港、补偿和拒绝事件；明确事件不直接含 UI 文案、Unity 对象或可变系统引用。

成功标准：事件可唯一关联命令并支持重放/去重；顺序与版本不依赖真实时钟或场景；未知事件/版本可以被上层安全拒绝。执行可用编译或静态检查，按协议写结果、handoff、记忆、状态同步和 diff 检查。
```

### D19-L17 — 港口失败码目录

```text
直接完成 D19-L17。前置：D19-L01=LOGIC_PASS。阅读 D19-L01、D2—D4 既有失败码、D19 任务卡和本地化约定。

唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortFailureCatalog.cs、game/tools/results/D19/D19-L17/、docs/game/design/common/handoffs/D19/D19-L17_LOGIC_HANDOFF.md、当天记忆日志和状态总览。不得改 UI 文案、其他系统失败码、Localization 资源或其它 D19 源文件。

建立 D19 唯一失败码/重试建议目录，覆盖未知港口、权限不足、非法生命周期、泊位冲突/过期、服务关闭、资金/库存/容量失败、登船失败、离港失败、保存冲突和不支持旧存档。每个条目须有稳定 code、本地化 key、可否重试及不会泄露内部对象的参数白名单；禁止直接写中文/英文 UI 文案。

成功标准：同一业务失败不会被多个字符串表示；后续系统能无 UI 依赖消费；不吞掉既有系统的原始失败事实。执行可用静态/编译检查并完整留痕；不存在证据时报告 PARTIAL/BLOCKED，不得编造 D2—D4 失败码映射。
```

### D19-L18 — 本地化键清单

```text
直接完成 D19-L18。前置：D19-L01=LOGIC_PASS。阅读 D19-L01、D19-L17 的公开接口、现有中英本地化规范和任务卡。

唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortLocalizationKeys.cs、game/tools/results/D19/D19-L18/、docs/game/design/common/handoffs/D19/D19-L18_LOGIC_HANDOFF.md、当天记忆日志和状态总览。不得改实际 UI、字符串表、Canvas、其它 D19 源文件或旧本地化内容。

建立 D19 使用的稳定 key 常量及纯检查 API：生命周期、服务、失败、补偿、迁移提示和读模型反馈都只能引用 key。实现 key 命名规则、重复检测和缺 key 报告接口；本任务只定义逻辑键，不添加或翻译显示文案。

成功标准：L17 的每个失败码都能映射到唯一有效的 key 常量；不存在散落 UI 字符串或 Unity 本地化 API；缺 key 不能静默回退为“成功”。执行可用编译/静态检查，完成协议中的结果、handoff、记忆、同步和 diff 检查。
```

---

## 波次 19.2：领域实现（可同步并行）

### D19-L03 — 到港访问状态机

```text
直接完成 D19-L03。精确前置：D19-L02、D19-L16、D19-L17 均为 LOGIC_PASS；读取它们的公开类型，缺失时只写 LOGIC_BLOCKED。禁止复制 lifecycle/event/failure 类型。

唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortVisitSystem.cs、game/tools/results/D19/D19-L03/、docs/game/design/common/handoffs/D19/D19-L03_LOGIC_HANDOFF.md、当天记忆日志和状态总览。不得改 L02/L16/L17、D2/D3/D4/D9/Save 或 Unity 内容。

实现纯领域到港/离港访问状态机：以不可变输入和明确命令 ID 驱动 L02 的合法迁移，输出新状态、事件草案和 D19-L17 失败码。重复的同一命令必须返回既有稳定结果而不产生第二个副作用；非法迁移必须原状态不变。不得触碰 Transform、NavMesh、真实时间或直接修改航海系统内部数值。

成功标准：每个成功/拒绝路径都有确定性结果和事件；状态仅由本系统拥有；外部系统通过公开适配接口消费。运行可用 C# 编译/静态检查并完整留痕、同步状态和 diff 检查。
```

### D19-L05 — 港口服务可用性

```text
直接完成 D19-L05。精确前置：D19-L04、D19-L17、D19-L18 均 LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortAvailabilitySystem.cs、game/tools/results/D19/D19-L05/、docs/game/design/common/handoffs/D19/D19-L05_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现纯函数/纯状态的服务可用性裁决：根据明确传入的港口、逻辑时间段、势力许可、天气逻辑标签、设施状态和服务 ID，计算可用/不可用、原因码、重试建议和相关本地化 key。不得读取系统时钟、场景天气、NPC GameObject 或资源；NPC 仅以逻辑角色/服务可用性标记表示。未知/冲突输入必须保守拒绝。

成功标准：同一输入可重复得到同一结果；服务关闭不会修改资金、库存、关系或场景；原因全部来自 L17、key 来自 L18。运行可用静态/编译检查并按协议写结果、handoff、记忆、状态同步和 diff 检查。
```

### D19-L06 — 泊位原子逻辑

```text
直接完成 D19-L06。精确前置：D19-L02、D19-L04、D19-L16、D19-L17 均 LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19BerthSystem.cs、game/tools/results/D19/D19-L06/、docs/game/design/common/handoffs/D19/D19-L06_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现泊位预留、确认占用、释放、过期和冲突的原子领域状态。使用稳定 berth_id/ship_id/command_id/逻辑时间，不使用坐标或物理检测；同一船不可占两个泊位，同一泊位不可同时给两船，失败不得半写。对船型/能力不匹配、租期无效、跨港、重复命令和过期释放返回 L17 错误并生成 L16 事件草案。

成功标准：任何操作后状态满足不变式，重复调用无额外副作用，所有可变泊位事实只由本系统写。运行可用检查，按协议完整留痕；不修改 D19-L04 目录或 D4 船只数值。
```

### D19-L23 — 港口权限校验器

```text
直接完成 D19-L23。精确前置：D19-L02、D19-L04、D19-L17、D19-L18=LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortAuthorityValidator.cs、game/tools/results/D19/D19-L23/、docs/game/design/common/handoffs/D19/D19-L23_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现纯权限校验：基于明确传入的 actor_id、ship_id、port_id、service_id、货物/成员归属和授权声明，判断谁能请求何种港口命令。权限校验只返回允许/拒绝与 L17 失败码/key，绝不能“修正”归属、创建授权、查询网络或读取当前登录状态。拒绝跨船、跨港、跨玩家和伪造服务访问。

成功标准：所有权限结果可重放；没有跨实体状态写入；下游适配器可在调用所有者前使用它。执行可用检查并写完整结果、handoff、记忆、状态同步和 diff 检查。
```

### D19-L24 — 非法状态防护器

```text
直接完成 D19-L24。精确前置：D19-L02、D19-L17、D19-L18=LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19InvalidStateGuard.cs、game/tools/results/D19/D19-L24/、docs/game/design/common/handoffs/D19/D19-L24_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现集中式不变式/非法状态防护：验证生命周期迁移、港口/泊位归属、命令 ID、服务前置、船员位置语义和状态组合。输入无效时必须拒绝、原状态不变并返回 L17 错误与 L18 key；不可吞异常，也不可通过 bool 默认值“自动修复”。防护器只校验，不拥有市场、库存、船舶或保存写入。

成功标准：后续 L07—L10 和命令路由能复用同一校验语义；没有 Unity/网络/LLM 依赖或对其它系统私写。执行可用检查，按协议写结果、handoff、记忆、同步和 diff 检查。
```

---

## 波次 19.3：港口服务与编排基础（可同步并行）

### D19-L07 — 靠泊规则

```text
直接完成 D19-L07。精确前置：D19-L03、D19-L05、D19-L06、D19-L17、D19-L23、D19-L24 均为 LOGIC_PASS。任一缺失时不要复制接口，只写 LOGIC_BLOCKED。

唯一写入范围：game/Assets/Scripts/Systems/D19/D19DockingRules.cs、game/tools/results/D19/D19-L07/、docs/game/design/common/handoffs/D19/D19-L07_LOGIC_HANDOFF.md、当天记忆日志和状态总览。不得修改生命周期、泊位、权限或船舶所有者文件。

实现靠泊前置规则的纯校验：港口访问状态、船只逻辑状态、泊位能力/归属、服务可用性、权限与非法状态防护必须都通过，才可生成“允许靠泊”的确定性裁决。它只消费 L06 的预留/占用命令，绝不实现物理碰撞、距离、Transform、NavMesh 或自动移动船只。失败必须保持所有状态不变并给出 L17 原因。

成功标准：靠泊不是仅凭一个 bool；船型不兼容、无访问、无权、已占用、服务关闭、非法状态和重复请求均有稳定结果。运行可用检查，按协议写结果、handoff、记忆、同步和 diff 检查。
```

### D19-L08 — 登船/下船工作流

```text
直接完成 D19-L08。精确前置：D19-L02、D19-L03、D19-L16、D19-L17、D19-L23、D19-L24 均 LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19BoardingWorkflow.cs、game/tools/results/D19/D19-L08/、docs/game/design/common/handoffs/D19/D19-L08_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

定义纯业务登船/下船请求、成员位置语义、允许状态、失败回退和幂等结果。成员位置只能是逻辑枚举/稳定实体 ID，不得访问角色 Transform、动画、碰撞或输入。处理部分成员失败时必须给出原子拒绝或明确可审计补偿草案，不能留下“船上/岸上”双重状态；为每个成功/失败结果产生 L16 事件草案。

成功标准：重复 command_id 不会再移动成员；无权、无泊位/船只、非法生命周期、未知成员和已在目标位置都确定性处理；不直接改 D20、船员或 Save。执行可用检查与完整协议留痕。
```

### D19-L11 — 市场港口适配器

```text
直接完成 D19-L11。精确前置：D19-L17、D19-L18、D19-L23 均 LOGIC_PASS，并从 D19-L01 找到 D3 的真实公开市场命令；找不到时 LOGIC_BLOCKED，不得自建货币/库存模型。

唯一写入范围：game/Assets/Scripts/Systems/D19/D19MarketPortAdapter.cs、game/tools/results/D19/D19-L11/、docs/game/design/common/handoffs/D19/D19-L11_LOGIC_HANDOFF.md、当天记忆日志和状态总览。不得修改 D3、库存/货币所有者、Save、UI 或其它 D19 文件。

封装港口市场请求的港口上下文、权限/服务前置、命令 ID 透传和 D3 公开调用；将 D3 的成功/失败投影为 D19 失败码、本地化 key 与事件草案，但 D3 仍独占价格、货币和库存裁决。禁止缓存一份可写库存或吞掉 D3 原始失败。

成功标准：没有可绕过 D3 的买卖路径；输入/输出可被 L22 幂等与 L20 保存消费；任何 D3 接口不匹配都明确阻断。运行可用静态/编译检查，完成结果、handoff、记忆、状态同步和 diff 检查。
```

### D19-L12 — 仓库港口适配器

```text
直接完成 D19-L12。精确前置：D19-L17、D19-L18、D19-L23=LOGIC_PASS，并确认 D3/D9 的真实公开仓库/库存接口。唯一写入范围：game/Assets/Scripts/Systems/D19/D19WarehousePortAdapter.cs、game/tools/results/D19/D19-L12/、docs/game/design/common/handoffs/D19/D19-L12_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现港口仓库取放的编排适配：校验港口服务与权限，透传命令 ID，调用原库存/容量所有者，返回规范的 D19 成功/失败/事件草案。必须保留货物归属、容量和数量的唯一写者；禁止复制库存集合、修改船舱、创建 UI 或把容量失败写成成功。

成功标准：未知物品、跨实体货物、容量满、重复命令和底层拒绝均有确定性结果；没有直接写 D3/D9 字段。执行可用检查并按协议留痕、同步和 diff 检查。
```

### D19-L13 — 酒馆港口适配器

```text
直接完成 D19-L13。精确前置：D19-L05、D19-L17、D19-L18、D19-L23=LOGIC_PASS，并确认酒馆/招募/关系系统的真实公开命令。唯一写入范围：game/Assets/Scripts/Systems/D19/D19TavernPortAdapter.cs、game/tools/results/D19/D19-L13/、docs/game/design/common/handoffs/D19/D19-L13_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现酒馆服务、社交或招募请求的港口上下文适配。先用 L05 判定服务和 NPC 逻辑可用性，再由原系统裁决费用、关系、候选船员和状态修改；D19 只翻译为稳定命令结果、失败码、key 和事件草案。禁止直接改关系、金钱、NPC 行为、模型或对话 UI。

成功标准：关闭服务、关系前置不足、权限不足、费用失败、底层拒绝和重复命令均可审计且不产生双重副作用。运行可用检查并完成所有协议留痕。
```

### D19-L14 — 船坞港口适配器

```text
直接完成 D19-L14。精确前置：D19-L17、D19-L18、D19-L23=LOGIC_PASS，并确认 D4 船舶检修/改造的真实公开入口。唯一写入范围：game/Assets/Scripts/Systems/D19/D19ShipyardPortAdapter.cs、game/tools/results/D19/D19-L14/、docs/game/design/common/handoffs/D19/D19-L14_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现船坞服务的上下文、授权和命令适配：港口服务/权限通过后才请求 D4，透传 command_id，保留 D4 对耐久、船体、改装、费用和资格的唯一裁决。适配器只能转换结果、失败码、本地化 key 和事件草案；不得写船舶数值、Prefab、模型或候选资源。

成功标准：不支持改装、无权、服务关闭、底层拒绝和重复请求都有稳定行为；任何 D4 公共接口缺失均诚实 BLOCKED。完成可用检查和协议留痕。
```

### D19-L15 — 出航准备适配器

```text
直接完成 D19-L15。精确前置：D19-L17、D19-L18、D19-L23=LOGIC_PASS，并确认 D9/D2/D4 的出航准备公开读模型/命令。唯一写入范围：game/Assets/Scripts/Systems/D19/D19PreparationPortAdapter.cs、game/tools/results/D19/D19-L15/、docs/game/design/common/handoffs/D19/D19-L15_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现补给、船员、货舱、船只和航线准备的只读汇总与命令适配。每条检查保留原所有者、原始原因和 D19 失败码/key；D19 不得自动补货、扣钱、修改船员或替航海系统启动航程。输出必须可供 L09 的离港工作流确定性读取。

成功标准：准备清单可以完整说明可否离港而不拥有任一数值；缺接口/冲突/未知状态保守拒绝；没有 UI、场景或运行时依赖。执行可用检查和全部协议留痕。
```

### D19-L19 — 港口只读模型

```text
直接完成 D19-L19。精确前置：D19-L03、D19-L05、D19-L06、D19-L11—L15、D19-L17、D19-L18 均为 LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortReadModel.cs、game/tools/results/D19/D19-L19/、docs/game/design/common/handoffs/D19/D19-L19_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

定义只读港口状态投影，面向未来 UI/Presentation：访问状态、泊位摘要、可用服务、行动反馈、失败 code/key、准备清单和版本/更新时间语义。输入来自各领域系统的不可变快照，输出不可写；不得反向调用 UI、Canvas、场景或将读模型变成裁决来源。不能泄露实体私密字段或内部可变集合。

成功标准：读模型只消费不拥有，任何 Presentation 即使缺失也不影响业务；同输入得到稳定排序和稳定输出。运行可用静态/编译检查，完成完整协议留痕。
```

### D19-L22 — 命令幂等控制

```text
直接完成 D19-L22。精确前置：D19-L02、D19-L03、D19-L06、D19-L16、D19-L17、D19-L24 均 LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19CommandIdempotency.cs、game/tools/results/D19/D19-L22/、docs/game/design/common/handoffs/D19/D19-L22_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现港口命令幂等语义：命令 ID、命令指纹、已完成结果、处理中标记、冲突/过期策略和可保存的最小投影。覆盖市场、仓库、泊位、登船、离港和保存命令的共用语义，但不替任何适配器执行副作用。相同 ID+相同指纹返回原结果；同 ID+不同指纹明确冲突；不得用全局静态缓存或真实时钟。

成功标准：重复投递不会二次扣款、二次占泊或二次发事件；所有状态可由 L20 保存；异常/中断有可审计安全结果。执行可用检查并按协议写结果、handoff、记忆、同步和 diff 检查。
```

---

## 波次 19.4：离港、返港与保存投影（可同步并行）

### D19-L09 — 离港工作流

```text
直接完成 D19-L09。精确前置：D19-L07、D19-L08、D19-L15、D19-L16、D19-L17、D19-L22、D19-L24 均为 LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19DepartureWorkflow.cs、game/tools/results/D19/D19-L09/、docs/game/design/common/handoffs/D19/D19-L09_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现离港的确定性编排：验证登船、出航准备、生命周期、权限/安全状态与幂等记录；随后请求离泊并发出航程启动意图。D19 只能通过 D2/D4/D9 的公开命令提出启动请求，禁止直接写 SailingSystem、船只、库存或船员数值。任一中途失败必须保留原状态或产生可审计补偿草案，不可留下已离泊但未开始航程的半状态。

成功标准：成功、准备不足、离泊失败、航程拒绝、重复命令和中断都确定性；事件顺序符合 L16。运行可用检查并完整留痕；任何公开航程接口缺失必须真实 BLOCKED。
```

### D19-L10 — 返港工作流

```text
直接完成 D19-L10。精确前置：D19-L03、D19-L06、D19-L07、D19-L16、D19-L17、D19-L22、D19-L24 均 LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19ArrivalWorkflow.cs、game/tools/results/D19/D19-L10/、docs/game/design/common/handoffs/D19/D19-L10_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现返港编排：验证在航/返港前态、目标港与权限，按泊位规则请求预留/占用，产生航程结束意图并恢复可用的港口服务状态。不得从真实位置、碰撞或场景判断到港；不得直接结束 D2 航程或写库存/船员。泊位无法分配、航程结束被拒绝、重复返港和中断必须有确定性失败/补偿结果。

成功标准：成功返港不会留下孤儿泊位或双重航程状态；所有事件有稳定命令/序列关联。运行可用检查和协议留痕。
```

### D19-L20 — 港口存档投影

```text
直接完成 D19-L20。精确前置：D19-L02、D19-L03、D19-L06、D19-L08、D19-L16、D19-L17、D19-L22 均 LOGIC_PASS，并从 L01 找到既有 Save 的公开扩展点。唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortSaveProjection.cs、game/tools/results/D19/D19-L20/、docs/game/design/common/handoffs/D19/D19-L20_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

定义 D19 可保存/可恢复的纯投影：访问状态、泊位租期、服务状态、成员逻辑位置、幂等记录、未完成命令/补偿草案、事件游标与版本。通过既有 Save 的公开扩展接口读写，D19 不得复制/替换整个 Save 根模型、写 Unity 对象或持久化 UI。序列化字段必须稳定、版本化、无绝对路径/场景引用。

成功标准：每个 D19 可变事实有唯一投影位置；无不可保存的全局静态状态；旧 Save 尚缺字段的处理留给 L21。运行可用编译/静态检查并完整留痕。
```

---

## 波次 19.5：存档迁移（严格串行）

### D19-L21 — 港口旧档迁移

```text
直接完成 D19-L21。精确前置：D19-L20=LOGIC_PASS；必须读取 L20 的公开投影与既有 Save 迁移规则，禁止重定义投影。唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortSaveMigration.cs、game/tools/results/D19/D19-L21/、docs/game/design/common/handoffs/D19/D19-L21_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现旧存档缺少 D19 字段、旧 key、旧版本和不可能状态时的安全迁移。合法缺字段应映射到保守确定默认值；存在数据损失或歧义时必须返回 L17 的不可逆错误/人工恢复提示，不能伪造泊位、成员位置、费用或完成命令。迁移必须幂等、版本单调，不能在同一输入上重复变化。

成功标准：迁移不会提升权限、自动开服务或产生业务副作用；失败可被 L39 测试定位。运行可用检查，写所有协议留痕并同步状态。
```

---

## 波次 19.6：领域回归（可同步并行）

### D19-L25 — 市场适配器测试

```text
直接完成 D19-L25。精确前置：D19-L11、D19-L16、D19-L17、D19-L18、D19-L22、D19-L23、D19-L24、D19-L21 均为 LOGIC_PASS。只测试现有公共接口；前置不通过时写 LOGIC_BLOCKED，绝不在测试任务中改生产代码。

唯一写入范围：game/Assets/Tests/EditMode/D19/D19MarketPortAdapterTests.cs、game/tools/results/D19/D19-L25/、docs/game/design/common/handoffs/D19/D19-L25_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

写无场景、无网络的确定性单元测试：合法买卖；资金不足；库存不足；权限/服务拒绝；相同 command_id 重放不重复扣款/发事件；同 ID 不同请求冲突；D3 原始失败被保留。测试只能使用 D19/D3 公开测试替身，不得新建可写货币/库存副本，也不得测试 UI。

成功标准：每个断言对应真实业务不变式，目标测试全部通过；若现有生产逻辑失败，在结果精确指出接口/错误码/命令而不是绕过。运行可用的 EditMode 静态测试或仓库既有测试入口，记录命令和结果；完成协议留痕、同步和 diff 检查。
```

### D19-L26 — 仓库适配器测试

```text
直接完成 D19-L26。精确前置：D19-L12、D19-L20、D19-L21、D19-L22、D19-L23、D19-L24=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19WarehousePortAdapterTests.cs、game/tools/results/D19/D19-L26/、docs/game/design/common/handoffs/D19/D19-L26_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

写纯确定性测试：入库、取库、容量满、跨实体归属拒绝、底层库存失败、重复命令稳定结果，以及仓库相关港口投影保存/恢复后等价。只调用公开适配器与原系统替身，不修改生产文件/Save 根模型，不使用 GameObject/Player。

成功标准：不会因失败产生物品丢失、重复物品或容量负数；投影不含 Unity 引用；每条失败映射到稳定原因。运行目标测试并真实记录结果；完成 JSON、handoff、记忆、同步和 diff 检查。
```

### D19-L27 — 酒馆适配器测试

```text
直接完成 D19-L27。精确前置：D19-L05、D19-L13、D19-L17、D19-L18、D19-L21、D19-L23=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19TavernPortAdapterTests.cs、game/tools/results/D19/D19-L27/、docs/game/design/common/handoffs/D19/D19-L27_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

写无 Unity 场景的测试：服务可用/关闭、逻辑天气/时间/势力限制、关系前置、费用拒绝、原系统拒绝、重复命令和事件/key 映射。NPC 只使用逻辑标识与可用性，禁止测试模型、寻路、对话面板或行为树。

成功标准：服务不可用不会修改关系/费用；成功与拒绝均可重放；所有反馈来自 L17/L18。运行相关测试、如实写结果；不在测试任务修生产逻辑。
```

### D19-L28 — 船坞适配器测试

```text
直接完成 D19-L28。精确前置：D19-L14、D19-L17、D19-L18、D19-L21、D19-L23=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19ShipyardPortAdapterTests.cs、game/tools/results/D19/D19-L28/、docs/game/design/common/handoffs/D19/D19-L28_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

写纯逻辑测试：合法维修/改造请求透传、服务关闭、权限拒绝、船型/资格不支持、D4 拒绝和重复命令。断言适配器不直接改船只属性，成功改变只能来自 D4 测试替身的公开裁决，失败不留下半改装状态。

成功标准：覆盖原系统裁决边界与稳定失败码，不创建模型/Prefab/材质。执行测试并完整留痕、同步和 diff 检查。
```

### D19-L29 — 泊位与靠泊测试

```text
直接完成 D19-L29。精确前置：D19-L06、D19-L07、D19-L16、D19-L17、D19-L21、D19-L22、D19-L24=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19BerthDockingTests.cs、game/tools/results/D19/D19-L29/、docs/game/design/common/handoffs/D19/D19-L29_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

写确定性测试：两船竞争同泊位、船型不兼容、预留→占用、租期过期、释放、跨港、重复预留及靠泊前置拒绝。每次操作后断言无船双占、无泊位双占、失败无状态改变、事件顺序/命令 ID 可追踪。不得用物理距离、碰撞器或场景。

成功标准：原子性与不变式被正反例覆盖；所有逻辑时间来自固定输入。运行测试、写真实结果及完整协议留痕。
```

### D19-L30 — 登船与离港测试

```text
直接完成 D19-L30。精确前置：D19-L08、D19-L09、D19-L15、D19-L16、D19-L17、D19-L21、D19-L22、D19-L23、D19-L24=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19BoardingDepartureTests.cs、game/tools/results/D19/D19-L30/、docs/game/design/common/handoffs/D19/D19-L30_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

写固定输入的工作流测试：上下船成功、成员已在目标位置、成员/权限失败、准备不足、离泊失败、航程启动拒绝、补偿、重复离港命令。断言失败后成员位置、泊位、访问状态和航程意图一致；断言成功事件严格按发生次序关联同一 command_id。禁止调用真实输入、船只移动、场景或 D20 表现。

成功标准：无成员双重位置、无已离泊未起航的孤儿状态、无重复副作用。只测试不改生产文件；运行测试与完整留痕。
```

### D19-L31 — 返港测试

```text
直接完成 D19-L31。精确前置：D19-L06、D19-L07、D19-L10、D19-L16、D19-L17、D19-L21、D19-L22、D19-L24=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19ArrivalReturnTests.cs、game/tools/results/D19/D19-L31/、docs/game/design/common/handoffs/D19/D19-L31_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

写返港/再靠泊测试：合法返港、无空泊位、泊位失效、航程结束拒绝、重复返港、服务恢复和中断补偿。断言无孤儿泊位、无双重航程状态、失败不会把在航状态伪写为返港；检查事件与失败 key/code 可重放。

成功标准：没有任何依赖坐标、碰撞、NavMesh 或 Player 的断言；所有状态转换可确定复现。运行目标测试并按协议写结果、handoff、记忆、同步和 diff 检查。
```

### D19-L32 — 服务可用性与 NPC 逻辑测试

```text
直接完成 D19-L32。精确前置：D19-L05、D19-L13、D19-L17、D19-L18、D19-L19、D19-L21=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19AvailabilityAndNpcTests.cs、game/tools/results/D19/D19-L32/、docs/game/design/common/handoffs/D19/D19-L32_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

使用固定逻辑输入测试天气标签、时间段、势力许可、设施状态和 NPC 服务标识对港口服务/读模型的确定性影响。覆盖未知标签、冲突输入、关闭服务、无可用 NPC、稳定排序和失败 key。NPC 不是 Unity 角色，禁止引用行为树、动画、模型、NavMesh 或场景。

成功标准：同输入多次结果完全相同；任何不可用服务不产生关系/费用/招募副作用；覆盖 L05 与 L19 的边界。运行测试并完成全部协议留痕。
```

---

## 波次 19.7：港口闭环整合（严格串行，唯一写者）

### D19-L33 — 港口命令路由

```text
直接完成 D19-L33。精确前置：D19-L02—L32 中所有计划任务均为 LOGIC_PASS。逐个读取结果 JSON；任何一个不是 PASS 时，本任务必须 LOGIC_BLOCKED，不得以局部实现替代。

唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortCommandRouter.cs、game/tools/results/D19/D19-L33/、docs/game/design/common/handoffs/D19/D19-L33_LOGIC_HANDOFF.md、当天记忆日志和状态总览。禁止反向修改任何已有 D19、D2—D9、Save 或测试文件。

将所有 D19 公开命令汇合为唯一入口：先做 command_id/指纹、授权、生命周期与服务校验，后调用适配器/工作流，按确定顺序写事件和保存投影，返回只读结果。路由器只编排，不夺取市场、库存、船只、船员、航程或 Save 的裁决权；不能出现巨型 switch 中直接写字段。失败路径必须不产生不一致副作用，并预留给 L34 的补偿语义。

成功标准：每种港口命令只有一个入口和一致的校验/事件/保存顺序；无 Unity、网络或 UI 依赖。运行可用编译/静态检查，完整留痕、同步和 diff 检查。
```

### D19-L34 — 港口补偿策略

```text
直接完成 D19-L34。精确前置：D19-L33=LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortCompensation.cs、game/tools/results/D19/D19-L34/、docs/game/design/common/handoffs/D19/D19-L34_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

为市场、泊位、登船、离港、返港和保存中途失败定义可审计的补偿计划与执行顺序：已完成副作用、可逆动作、不可逆动作、幂等补偿 command_id、事件和需要人工恢复的终止状态。补偿只能调用拥有者公开撤销/查询接口；若不可安全撤销，必须保守停止并登记。不得直接回写 D3/D4/D9/Save 私有状态。

成功标准：补偿不会生成新的副作用循环、不会删除事件历史、相同补偿可重放；没有“catch 后悄悄成功”。运行可用检查并按协议留痕。
```

### D19-L35 — 事件顺序审计

```text
直接完成 D19-L35。精确前置：D19-L34=LOGIC_PASS。唯一写入范围：game/Assets/Scripts/Systems/D19/D19PortEventOrderAudit.cs、game/tools/results/D19/D19-L35/、docs/game/design/common/handoffs/D19/D19-L35_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现事件序列审计与重放验证：序列单调、event_id 唯一、command_id 关联、预期状态转换、补偿后顺序和禁止重复副作用。它只能读 L16 事件和路由输出，返回审计报告/失败码；不得修改事件、重排历史或把失败事件删除。提供可由后续测试调用的纯 API。

成功标准：能定位第一个坏序列/重复事件/状态跳跃，能证明合法序列可重建只读状态；不使用真实时钟。运行可用检查，完整写结果和交接。
```

### D19-L36 — 确定性港口剧本测试

```text
直接完成 D19-L36。精确前置：D19-L35=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19DeterministicPortScenarioTests.cs、game/tools/results/D19/D19-L36/、docs/game/design/common/handoffs/D19/D19-L36_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

写一个无 Unity 场景的固定港口剧本：到港 → 预留/靠泊 → 市场或仓库服务 → 酒馆或船坞可用性 → 登船 → 准备检查 → 离港 → 返港。使用固定 ID、逻辑时间和测试替身，逐步断言路由结果、状态、事件顺序、幂等性和只读模型；至少包含一个失败后补偿分支。

成功标准：同一剧本重复运行字节/结构等价；不依赖 Player、输入、场景资源或真实日期。测试失败不得改生产实现；运行测试、真实记录结果并完整留痕。
```

### D19-L37 — 固定种子不变式模糊测试

```text
直接完成 D19-L37。精确前置：D19-L36=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19PortFuzzInvariantTests.cs、game/tools/results/D19/D19-L37/、docs/game/design/common/handoffs/D19/D19-L37_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

使用固定随机种子、明确记录种子值，生成至少 1,000 次混合港口命令。每一步校验：无负库存/费用假成功、无船双泊位、无泊位双占、无成员双位置、无非法生命周期、无同一 command_id 双副作用、事件序列可审计、保存投影可序列化。失败输出最小可重放命令序列与种子；禁止使用非确定随机、系统时钟、Unity 或网络。

成功标准：测试稳定、可重跑且不少于 1,000 命令；不能用跳过断言/吞异常达成通过。运行测试，按协议记录真实覆盖、命令数、种子和结果。
```

### D19-L38 — 港口保存/加载往返测试

```text
直接完成 D19-L38。精确前置：D19-L37=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19PortSaveLoadTests.cs、game/tools/results/D19/D19-L38/、docs/game/design/common/handoffs/D19/D19-L38_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

为每个生命周期节点写保存 → 加载 → 继续同一命令的等价性测试：到港、预留、靠泊、服务中、登船、离港、返港、未完成命令和补偿中。比较状态、泊位、成员位置、命令幂等记录、事件游标、读模型和下一命令结果；不得比较对象引用或 UI。包括一次保存中断/重放的保守处理。

成功标准：往返不会丢失或复制副作用，不会恢复出不可能状态；每个差异有明确失败信息。运行测试并完成协议留痕。
```

### D19-L39 — 历史存档兼容测试

```text
直接完成 D19-L39。精确前置：D19-L38=LOGIC_PASS。唯一写入范围：game/Assets/Tests/EditMode/D19/D19LegacySaveTests.cs、game/tools/results/D19/D19-L39/、docs/game/design/common/handoffs/D19/D19-L39_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

为旧存档夹具构造/使用测试：完全没有 D19 块、旧 port/berth 别名、旧版本字段、缺命令记录、冲突租期和无法安全恢复的不可能状态。验证 L21 迁移在安全情形提供确定默认值、可重跑；不安全情形明确拒绝并保留恢复提示，绝不伪造费用/泊位/成员位置。

成功标准：迁移不越权改变其它系统数据，错误可定位且本地化 key/code 稳定。运行测试、如实记录夹具/结果，不修改生产文件。
```

### D19-L40 — 逻辑边界静态审计

```text
直接完成 D19-L40。精确前置：D19-L39=LOGIC_PASS。唯一写入范围：tools/d19_logic_boundary_audit.py、tools/tests/test_d19_logic_boundary_audit.py、game/tools/results/D19/D19-L40/、docs/game/design/common/handoffs/D19/D19-L40_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现只读静态审计工具并写单元测试。扫描 D19 新增代码，P0 阻止 UnityEngine、Transform、GameObject、Input、Canvas、PlayerConnection、NavMesh、网络/HTTP/LLM、系统时钟/非确定随机、D2/D3/D4/D9/Save 的私有字段直写、场景/Prefab/资源写入和越过 D19 目录的生产改动。审计必须使用明确规则、相对路径、稳定错误码与可读报告；工具不可修改被审计文件或访问网络。

成功标准：正例通过，故意注入的每类违规夹具被 P0 拦截；历史 SUPERSEDED 文档不被误报为 D19 新代码。运行工具测试和对真实 D19 源码的审计，结果为 PASS 才能写 LOGIC_PASS；完整留痕、同步与 diff 检查。
```

### D19-L41 — D19 统一逻辑测试运行器

```text
直接完成 D19-L41。精确前置：D19-L40=LOGIC_PASS。唯一写入范围：tools/d19_logic_test_runner.py、tools/tests/test_d19_logic_test_runner.py、game/tools/results/D19/D19-L41/、docs/game/design/common/handoffs/D19/D19-L41_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

实现一个离线、可重复的统一运行器：发现并按固定顺序运行 D19 编译/静态检查、L25—L39 测试和 L40 边界审计，收集真实 exit code、测试数、失败项、耗时、输入版本，输出机器可读 JSON 汇总。禁止启动 Player/PlayMode、使用 shell 猜测成功、吞掉失败或把历史 Rxx 测试混进来。对运行器写测试，覆盖发现顺序、失败传播、缺结果、超时/不可执行的诚实报告。

成功标准：任何子项失败都会使总汇总失败，成功汇总可让 L43 复核；运行真实 D19 逻辑套件一次并记录命令/输出。完成所有协议留痕、状态同步和 diff 检查。
```

### D19-L42 — 对后续章节的港口逻辑接口

```text
直接完成 D19-L42。精确前置：D19-L41=LOGIC_PASS，且读取其机器汇总确认所有 D19 P0 测试/审计通过。唯一写入范围：docs/game/design/common/D19_LOGIC_PORT_INTERFACE.md、game/tools/results/D19/D19-L42/、docs/game/design/common/handoffs/D19/D19-L42_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

发布只读的 D20—D23 接口文档：唯一命令入口、请求/结果、生命周期、事件、读模型、失败码/key、权限/幂等/补偿语义、存档投影/迁移版本、公开拥有者与禁止直写清单。逐项链接真实 C# 类型和测试证据；声明后续章节只能消费 API，不能修改 D19 内部。单列 Runtime Deferred：锚点坐标、碰撞、输入、HUD、NPC/角色表现、场景、模型、画面和性能。

成功标准：文档不包含不存在的 API、不把逻辑通过说成实机可玩、不向 D20/D23 承诺 Unity 资源。验证所有链接与类型可解析，完成完整留痕、同步和 diff 检查。
```

---

## 波次 19.8：章节汇合门禁（严格串行）

### D19-L43 — P0 证据复核

```text
直接完成 D19-L43。精确前置：D19-L42=LOGIC_PASS。唯一写入范围：game/tools/results/D19/D19-L43/、docs/game/design/common/handoffs/D19/D19-L43_LOGIC_HANDOFF.md、当天记忆日志和状态总览。禁止改代码、测试、契约、场景、资源或前置结果。

以只读方式逐一核对 L01—L42 的结果 JSON、handoff、L41 机器汇总、L40 真实审计、测试与编译证据。建立 P0 矩阵：生命周期、授权、泊位、服务、市场、仓库、酒馆、船坞、登离港、事件、幂等、保存、迁移、确定性剧本、1,000 次 fuzz、边界审计各自对应哪项真实证据。缺文件、过期结果、失败命令、P0 blocker、越界改动或状态矛盾都必须阻断。

成功标准：只在所有 P0 有当前可复核 PASS 证据时写 LOGIC_PASS；不能“补跑/修复”前置，发现问题只能精确列为 BLOCKED。按协议写 JSON/handoff/记忆并运行状态同步与 diff 检查。
```

### D19-L44 — 持久化所有权审计

```text
直接完成 D19-L44。精确前置：D19-L43=LOGIC_PASS。唯一写入范围：docs/game/design/common/D19_LOGIC_PERSISTENCE_AUDIT.md、game/tools/results/D19/D19-L44/、docs/game/design/common/handoffs/D19/D19-L44_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

只读审计 D19 的所有可变事实：访问状态、泊位、服务、成员逻辑位置、幂等命令、未完成工作、补偿、事件游标和迁移版本。对每项写明唯一写者、保存位置、加载/迁移路径、往返测试证据、非法/旧档策略和是否误写到 D2/D3/D4/D9/Save 私有字段。核对 L20/L21/L38/L39/L40/L41 的真实输出；不得修改任何实现或把 Runtime 问题伪装为存档通过。

成功标准：没有无主可变状态、不可保存的全局状态或双写者；每项都有可定位证据。完成协议留痕、同步和 diff 检查；证据不足即 LOGIC_BLOCKED。
```

### D19-L45 — 工作流与不变式审计

```text
直接完成 D19-L45。精确前置：D19-L44=LOGIC_PASS。唯一写入范围：docs/game/design/common/D19_LOGIC_WORKFLOW_AUDIT.md、game/tools/results/D19/D19-L45/、docs/game/design/common/handoffs/D19/D19-L45_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

只读复核 D19 的端到端确定性剧本、失败补偿、事件重放、幂等行为和 L37 不变式。逐个检查“到港→靠泊→服务→登船→离港→返港”“无泊位”“准备失败”“底层服务失败”“保存中断/恢复”“旧档迁移”是否有当前测试/审计证据。明确列出不变式：无负数/假成功、无双泊位、无双位置、无非法迁移、无重复副作用、无孤儿事件。

成功标准：引用 L36—L41 的真实命令和结果，不写运行时路线/截图/FPS；发现 P0 测试缺口时不修代码，只 BLOCKED。按协议写结果、handoff、记忆、状态同步和 diff 检查。
```

### D19-L46 — 运行时延期登记

```text
直接完成 D19-L46。精确前置：D19-L45=LOGIC_PASS。唯一写入范围：docs/game/design/common/D19_RUNTIME_DEFERRED_REGISTER.md、game/tools/results/D19/D19-L46/、docs/game/design/common/handoffs/D19/D19-L46_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

建立 D19 未来 Runtime 阶段唯一待测清单：场景锚点实际坐标、靠泊碰撞、登船触发、NPC 视觉/寻路、输入、HUD/面板互斥、按钮点击、模型/Prefab、动画、光照画面、加载和性能。每条包含逻辑前置接口、风险、未来验收方式、禁止在当前阶段宣称通过的原因；同时列出哪些项已由纯逻辑完成而非实机确认。

成功标准：完整而不重复，不启动 Unity，不生成截图/性能数据，不把 Pending HDRI 或资源候选升级审批。完成完整协议留痕、同步和 diff 检查。
```

### D19-L47 — 章节状态审查

```text
直接完成 D19-L47。精确前置：D19-L46=LOGIC_PASS。唯一写入范围：docs/game/design/common/D19_LOGIC_STATUS_REVIEW.md、game/tools/results/D19/D19-L47/、docs/game/design/common/handoffs/D19/D19-L47_LOGIC_HANDOFF.md、当天记忆日志和状态总览。

只读审查 D19-L01—L46 的每一个结果 JSON、handoff、当日记忆引用、唯一写入范围、状态同步面板、L41 汇总和 L43—L46 审计。检查任务数恰为 48、无重复/缺编号、无 PLANNED/PARTIAL/BLOCKED、结果路径存在、状态与证据一致、无越权/运行时伪通过。识别任何旧 A/S/R/RERUN 误混入新 L 任务的情况。

成功标准：报告给出逐项矩阵及真实统计；有任何 P0 缺口就 LOGIC_BLOCKED，不能自己修复或修改前置。按协议写结果、handoff、记忆、同步和 diff 检查。
```

### D19-L48 — D19 唯一逻辑验收

```text
直接完成 D19-L48。精确前置：D19-L47=LOGIC_PASS。唯一写入范围：docs/game/design/common/D19_LOGIC_ACCEPTANCE.md、game/tools/results/D19/D19-L48/、docs/game/design/common/handoffs/D19/D19-L48_LOGIC_HANDOFF.md、当天记忆日志和状态总览。此任务是只读验收；禁止顺手改代码、测试、旧结果或任何运行时内容。

依据 L43 P0 矩阵、L44 持久化审计、L45 工作流审计、L46 延期清单、L47 状态审查和 L41 的机器汇总，给出 D19 唯一结论。验收必须明确：D20/D23 可消费哪些纯逻辑接口；哪些数据/命令/保存/事件已通过；哪些仍是 Runtime Deferred；是否存在 P0 blocker。只有全部 48 个新 L 任务有当前 `LOGIC_PASS` 且无 P0 blocker 才可写 `LOGIC_PASS`。

成功标准：不使用截图、Player、FPS、旧 R03 或“看起来能跑”作为证据；不把逻辑通过称为港口画面/交互完成。写结果、handoff、记忆，运行状态同步与 diff 检查，并最终只报告真实 LOGIC_PASS/LOGIC_PARTIAL/LOGIC_BLOCKED、D20/D23 放行结论和 Runtime Deferred 路径。
```
