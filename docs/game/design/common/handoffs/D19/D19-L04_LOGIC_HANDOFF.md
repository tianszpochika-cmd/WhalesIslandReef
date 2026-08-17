# D19-L04 交接报告

## 任务信息与结论

- 任务编号：`D19-L04`（波次 1，并行；前置 D19-L01=LOGIC_PASS 已满足）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L04/D19_L04_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19PortAnchorCatalog.cs`（新建，纯数据目录）
- `game/tools/results/D19/D19-L04/D19_L04_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L04_LOGIC_HANDOFF.md`（本文件）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未修改场景、Prefab、D19Lighting、资源 catalog、任何其它 D19 系统文件或 D2—D9 文件；未启动 Player；未读取 Unity 坐标。

## 冻结内容

### 键空间（全部为业务语义键，`^[a-z0-9_]+$`）

| 种类 | 规范键 | 事实来源 |
|---|---|---|
| port_id | `port_santa_margarita` / `port_green_cay` / `port_driftwood_isle` | D3 `PortCatalog`（StartingPortId 等） |
| berth_id | `berth_<port>_<zone>_<n>`（santa_margarita：dock/large/small/cargo/anchorage；green_cay：`berth_green_cay_01`（与 D9 同键）+ small；driftwood_isle：anchorage） | D9 泊位键、港口设计文档分区 |
| service_id | `service_<port>_<name>`（santa_margarita 9 项：market/tavern/shipyard/supplies/repair/recruitment/wages/warehouse/customs；green_cay 6 项；**荒岛无服务**） | `D9PortService` 枚举 + 01_BUILDING_OVERVIEW/02_PORT_SHIPYARD |

### 别名/废弃映射（单向、无环）

- D3 港口裸键 `santa_margarita`/`green_cay`/`driftwood_isle` → 废弃别名，解析成功并返回迁移提示（指向 `port_*`）；
- D9 泊位 `building_saint_marys_dock`（命名与港口 santa_margarita 不一致，历史事实）→ 废弃别名，迁移至 `berth_santa_margarita_dock_01`；
- 别名解析单向：只从别名指向规范键；DFS 构造校验 + 运行时 visited 集合双重防环。

### 确定性 API 与稳定错误码

- `ResolvePort/ResolveBerth(input, owningPort?)/ResolveService(input, owningPort?)/TryResolve`——相同输入永远相同输出；
- 错误码：`invalid_key_format`（空/路径/盘符/大写/显示名）、`unknown_port_key`/`unknown_berth_key`/`unknown_service_key`、`cross_port_berth`/`cross_port_service`（归属港口不匹配）、`alias_cycle`、`duplicate_key`；
- `TryCreate` 构造校验：键格式、重复键、跨种类重复、别名目标存在、单向无环、废弃键必有迁移提示、泊位/服务归属港口存在——校验失败返回稳定错误清单（`Instance` 构造时校验，失败抛明确异常）。

## 验证（全部真实执行）

| 验证 | 命令 | 结果 |
|---|---|---|
| 编译 | 临时 net9.0 控制台工程直接链接真实 `.cs` | 0 warning 0 error，exit 0（零 Unity 依赖可独立编译） |
| 确定性断言 | 临时 harness 39 项 | 39/39 PASS（2000 次同输入同输出；废弃解析+迁移提示；未知/格式/跨港稳定错误；循环别名构造拒绝+运行时防环；重复键构造拒绝；荒岛无服务；键无路径字符；每泊位归属一致） |
| 纯 BCL | grep UnityEngine/Vector3/GameObject/Transform/NavMesh/Scene/Resources/Assets | 无引用（仅注释声明排除） |
| `git diff --check` | — | PASS：exit 0，仅既有 LF→CRLF 警告 |
| 状态同步 | `python tools/sync_logic_task_status.py` | PASS：D19-L04=LOGIC_PASS |

开发期真实修复：`DeprecatedAlias` 助手最初把规范目标误传给 `port` 构造参数，`TryCreate` 的「归属港口不存在」校验当场拦截（cross_port 错误），修复后复验 39/39 全过——证明构造期校验真实生效。

## 明确未宣称

- 目录只表达逻辑服务地址；不含任何场景存在性断言、坐标或 Transform。
- 泊位容量/占用、服务可用性、吃水/潮位/碰撞等行为分别属 D19-L05/L06/L07 与 Runtime，本目录不裁决。

## 待办 / 后续注意事项

- D19-L05（服务可用性）、L06（泊位逻辑）、L07（靠泊规则）、L33（命令路由）消费本目录键；L19（读模型）输出规范键。
- D9 历史键的运行时迁移/退役属后续整合；本目录只提供可审计迁移提示。
- 若 D18-L11 词典后续冻结 dock/berth 新分类，本目录 berth/dock 键按 L45 口径对齐。
