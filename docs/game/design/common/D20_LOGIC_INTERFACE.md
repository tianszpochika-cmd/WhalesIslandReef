# D20 纯逻辑公开接口

> 版本：D20-L40
> 适用范围：角色、玩家 presence、动作、NPC 行为、动物/宠物行为的离线逻辑层。
> 本文不发布 Unity 场景、Prefab、Animator、Input、网络或 LLM 接口。

## 1. 所有权边界

| 领域 | D20 可以拥有 | D20 只能读取/提出请求 | D20 禁止直接写入 |
|---|---|---|---|
| 玩家/角色 | 身份绑定、presence 投影、动作投影、表现只读事件 | D10 身份/特质/外观公开结果 | D2/D3 全局事实 |
| NPC | 日程输入、行为状态投影、交互资格与行为回执 | D10 身份/岗位、关系、经济、任务、港口公开接口 | 关系、经济、库存、任务、港口事实 |
| 动物/宠物 | 动物行为投影、跟随资格投影、生态只读适配 | D11 宠物 owner、D18 生态/水域读模型 | D11 宠物生命/照料/位置/丢失事实、生态事实 |
| 港口 | D19 命令结果的只读消费与 D19 请求 | D19PortCommandRouter.Route 的公开结果 | D19 私有字段、泊位、港口生命周期事实 |

Agent 或表现层只能提交经过 DTO 校验的命令；D20 服务负责校验、幂等、投影和回执，不替任何 owner 裁决事实。

## 2. 公开生产入口

- `D20CharacterCommandService`：角色命令唯一聚合入口，接收 presence/action/NPC/animal 请求，返回稳定状态、失败码、事件与只读投影。
- `D20NpcBehaviorService`：把已验证的 NPC 日程和行为请求交给 D20 状态机，输出行为投影与 owner request；不执行寻路、交易或关系写入。
- `D20AnimalBehaviorService`：把已验证的动物行为交给 D20 状态机，读取宠物跟随资格和生态/水域适配结果；不成为 D11 第二写者。
- `D20ActionEventRouter`：统一动作命令、owner 结果、D20 投影、表现事件和失败事件的顺序；相同命令回放相同回执，冲突命令拒绝。
- L01—L27 的契约、规则、状态机、保存投影和迁移类继续作为基础公开 DTO/纯函数使用。

## 2.1 船员公共只读投影（D20-L40 增补）

- `D20CrewOwnerRosterReadSnapshot` 是 D10/Crew owner 发布给 D20 的最小 read model；它只携带稳定 `ship_id`、`crew_id`、`save_revision`、`source_projection_id`、`source_owner_system`、资格岗位字符串、可用性、失能、`morale`、`fatigue` 和 `health`。
- `D20CrewProjectionAdapter.Project` 先执行 `D20CrewProjectionContracts.ValidateOwnerSnapshot`，再生成 `D20CrewRosterProjection` 的防御性副本。缺失名单、非法 schema/ID、重复 crew/岗位或越界/非有限数值均 fail-closed；不填充默认船员、不改写 owner 对象。
- `D20CrewRosterProjection.Clone` 深拷贝成员和岗位列表，并按稳定 `crew_id`、岗位字符串确定性排序；`Fingerprint` 不依赖运行时哈希，可用于命令幂等审计。`D20CrewProjectionAdapter.Validate` 可独立复核已发布投影。
- D20 不解释岗位语义，也不成为 D10/CrewSystem 的第二写者。D21 的 `D21CrewProjectionAdapter` 对岗位字符串执行唯一显式映射：`helm/helmsman→Helmsman`、`sailmaster/sail_master/sail-master→Sailmaster`、`lookout→Lookout`、`navigator→Navigator`、`repair/carpenter→Carpenter`、`cook→Cook`、`cargo_master/cargomaster→CargoMaster`、`boatmaster/boat_master→Boatmaster`、`gunner→Gunner`、`medic→Medic`、`deckhand→Deckhand`；未知字符串拒绝，不降级为 `Deckhand`。
- D21 `Start` 在存在该公共投影时优先映射并真实调用 `D21CrewRequirementRules.Evaluate`；没有公共投影时，已验证的 `D21CrewRosterSnapshot` 仍可兼容使用且同样重新校验。历史预计算 `CrewDecision` 仅保留给 D21-L27 离线回放的严格形状兼容，不覆盖公共投影或 roster 校验。

## 3. 命令、结果与事件约定

1. 命令必须有稳定 `command_id`、actor/entity id、world day、save revision 和业务 payload。
2. 相同 `command_id` 与相同指纹只能返回 duplicate/replay，不得再次修改 D20 状态或重复产生事实事件。
3. 相同 `command_id` 与不同指纹返回稳定冲突失败码；失败不会产生 DomainFactEvent。
4. 成功顺序为：输入校验 → 调用 owner 公开接口/接收 owner 结果 → 更新 D20 自有投影 → 产生只读表现事件/审计回执。
5. owner 缺失、权限不足、非法状态或恢复失败必须 fail-closed，并保留稳定失败原因；禁止“按钮可见即成功”。

## 4. 下游消费方向

| 消费方 | 允许消费 | 禁止做法 |
|---|---|---|
| D21 航海 | 角色 presence、船员行为/资格只读快照、D20 命令结果 | 复制 D20 船员事实或写 NPC/角色私有字段 |
| D22 UI | 只读 action/NPC/animal/presence projection、事件、失败码 | 从 UI 直接改状态或把视觉状态当业务成功 |
| D23 社会模拟 | NPC 行为投影、交互资格、只读关系/经济请求 | 绕过 D10/D11/经济/关系 owner |
| Save | D20 save projection、schema/revision、owner snapshot reference | 保存 Unity 对象、资源路径或第二份 owner 事实 |

## 5. 非目标

D20 逻辑接口不保证真实模型、动画、Prefab、NavMesh、物理、键鼠/手柄输入、UI 布局、网络服务、LLM 决策、Unity Player、性能或发行包可用；这些项目统一记录在 D20_RUNTIME_DEFERRED_REGISTER.md。
