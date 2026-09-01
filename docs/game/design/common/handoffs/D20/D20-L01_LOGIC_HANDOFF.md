# D20-L01 逻辑交接说明

- 任务：`D20-L01`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L01/D20_L01_LOGIC_RESULT.json`
- 基线：`docs/game/design/common/D20_LOGIC_BOUNDARY_BASELINE.md`

## 1. 已交付

本任务冻结了 D20 后续纯逻辑开发所需的单一写者边界和公开接口：

1. 玩家语义 presence/action 与 Core/Progression、D10 身份/外观的分界；
2. NPC 身份、D10CrewBehaviorSystem 的岗位/轮班/疲劳/健康/行为所有权、RelationSystem canonical graph 的关系所有权，以及 D10 酒馆编排边界；
3. D11 Ecology、钓鱼/植被、养殖/活鱼舱、宠物、珍稀动物、鲸豚路线投影和跨域事件的写者边界；
4. Presentation 只读消费、D18 内容元数据消费和候选资源/运行时审批隔离；
5. D19 `D19PortCommandRouter.Route(D19RoutedPortCommand)` 唯一命令入口、`D19PortReadModel` 只读入口、`D19PortSaveProjection/Migration` 保存边界；
6. D20 命令、读模型、事实事件、表现事件、幂等、迁移和失败回退语义；
7. D20-L02—L27 并行、L28—L35 串行、L36—L40 汇合门禁，以及 D21—D24 的只读下游依赖；
8. D20-A01—A06、D20-S01/S02 历史视觉/运行时结果隔离清单。

## 2. 前置与接续

- `D19-L48` 当前结果为 `LOGIC_PASS`：48 个结果协议检查通过，D19 逻辑边界审计 38/38 零违规，Unity EditMode `WhalesIslandReef.D19A02.Tests` 为 117/117。D20-L28 的港口命令部分可以调用 D19 Router，但仍不得读写 D19 私有字段。
- D20-L02—L27 可在本基线之后按两个并行波次执行；每项只能消费公开类型。
- D20-L28—L35 必须严格串行：L28 统一 D20 角色命令入口，L29/L30 聚合并保持 D10/D11/D19 所有权，L31 固定事件顺序/重放，L32—L35 做内容、保存、确定性和不变式验证。
- D20-L36—L40 是门禁与汇合任务，不检验模型、Animator、Prefab、场景、真实行为画面或设备性能。

## 3. 实施红线

- 不要在本基线之外创建 D20 系统实现来“预先占位”；相邻任务按任务卡各自落盘。
- 不要直接修改 D10/D11/D19 的状态、私有字段、存档副本、事件日志或外部经济/关系/生态事实。
- D20PetOwnershipSystem 只能拥有 D20 自己的跟随/资格投影；`D11PetState` 仍由 D11PetSystem 唯一写入。
- 港口动作必须通过 D19 Router；不存在公开接口时将对应任务标为 `LOGIC_BLOCKED`，不得用旧场景或视觉脚本绕过。
- Presentation 事件不得成为事实写入口；资源缺失、候选路径和历史截图都只能进入 Runtime Deferred/隔离说明。
- 纯逻辑实现不得引入 Unity、网络、LLM、Animator、Transform、NavMesh、Prefab、Scene、Canvas、输入、设备性能依赖。

## 4. 验证与未关闭风险

本任务只执行文档/结果/状态同步范围的验证：

- `python tools/sync_logic_task_status.py`：PASS；
- `git diff --check`：PASS。

运行时风险保持延期：正式模型/Avatar/Animator/Prefab/场景、物理/寻路/输入/UI/音频、资源运行时准入、PlayMode、截图、设备性能和发布审批均未在本任务执行。`LOGIC_PASS` 只表示 D20-L01 基线完成，不表示 D20 章节、可玩版本或 Release 通过。
