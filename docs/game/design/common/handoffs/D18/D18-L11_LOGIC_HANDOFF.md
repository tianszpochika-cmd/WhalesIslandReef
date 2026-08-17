# D18-L11 交接报告

## 任务信息与结论

- 任务编号：`D18-L11`（波次 1，并行）
- 日期：2026-08-16
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D18/D18-L11/D18_L11_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `docs/game/design/common/assets/D18_TAXONOMY_SHIPS_PORT.md`（新建，本任务独占）
- `game/tools/results/D18/D18-L11/D18_L11_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D18/D18-L11_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-16.md`（追加当日记忆）

未修改 catalog、资源、Unity、场景、Prefab、Registry、Systems、contracts、server、web 或其它 D18 工具。

## 冻结内容摘要

1. **7 个逻辑类别**：`ship`、`hull_component`、`port_service`、`dock`、`berth`、`building`、`prop`，含子类明细（船型定义/可驾驶船体/变体/损坏/远景/桅杆索具/船上武器/交通艇/船体模块/功能点；装卸/修理/补给/上下船/排队/检疫/检查；货运码头/栈桥/平台；大船/小船泊位/锚地；九类功能建筑 + 六类构件；仓储/旗帜/工具/武器道具）。
2. **content_id 命名规则**：字符集 `[a-z0-9_]`（1,865 条真实 intake 条目验证）；子前缀 `ship_`/`ship_module_`/`ship_function_`/`ship_rigging_`/`ship_weapon_`/`boat_`/`port_service_`/`dock_`/`berth_`/`building_`（沿用 Registry 80 条真实 id）/`prop_`；进入 Inventory 的改装物品沿用 `item_`（D15 冻结不改）。
3. **子类 → L02 `category` 唯一映射**（L42 编译器约束）：ship/hull_component → `ship`；port_service/dock/berth/building → `building`；prop → `prop`。**注意**：A 系列准入目录把码头/平台模块归 `domain=ship`，本词典冻结为 `dock → building`（A 系列口径 SUPERSEDED）。
4. **弃用映射**：intake 角色/domain → L 子类（含 `ship_hull_playable_or_distant` 拆分为 playable/distant 两个键）；D4 运行时船型裸键 `sloop`/`small_fishing` 与设计文档 `fishing_boat` 的命名差异（以代码/注册表为准）；`d18a05_*`/`candidate_*` 候选 id 只作只读证据；`placeholder_*` 保持占位状态，`ship_*`/`prop_*`/`dock_*`/`berth_*`/`port_service_*` 无 Registry 记录，**不得补造**。
5. **8 条不可混用关系**：船型数据≠船体视觉候选；内容定义 id≠资源记录 id；构件≠功能建筑；dock/berth 状态≠建筑状态；port_service≠经济/工期数值；损坏/远景≠可驾驶船体；禁止缩放/改名/复制补数量；历史状态不改写。
6. **D19/D21 使用边界**：D19 只读 `building_*`/`port_service_*`/`dock_*`/`berth_*`/`ship_*` 键与 PortVisit 状态机契约；D21 只读 `ship_*`/`ship_module_*`/`ship_function_*`/`hull_component` 键；两者都禁止直接引用 `Assets/Art/Candidates` 候选文件。
7. **边界声明**：分类只服务数据/资源映射，不改变船舶/港口/经济/场景状态；不统计模型数量、不声称模型已存在；每类给出命名例子、允许资源类型、所有者与 D19/D21 使用边界。

## 明确未宣称

- 不统计候选文件数量，不宣称任何模型已存在、已导入、可驾驶或运行时可用。
- 不宣称任何资产为 `RuntimeApproved`/`ReleaseApproved`（本阶段禁止）。
- `ship_*`/`prop_*`/`dock_*`/`berth_*`/`port_service_*` 内容键为规则冻结，尚无 Registry 记录，未补造。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| python 读取 intake catalog（domain=ship 21 条 / domain=building 112 条的角色、文件名、manifest；1,865 条 content_id 字符集） | PASS：子类与命名规则以真实数据锚定 |
| python 读取 replacement map 与 Registry 前缀（building_ 80 条等） | PASS：弃用映射以真实 Registry 为据，无补造 |
| 只读读取 ShipTypeDef.cs / SeedData.cs（sloop/small_fishing）对照 02_SHIP_TYPES.md | PASS：命名差异如实登记 |
| python 链接检查（17 个相对链接） | PASS：缺失 0 |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |
| `python tools/sync_logic_task_status.py` | PASS：D18-L11=LOGIC_PASS，总览重新生成 |

## 待办 / 后续注意事项

- L12—L14 分类词典与本词典共享 L02 `category` 枚举；L45 一致性审计将核对三份词典与 Schema。
- L21—L24 来源目录按本词典与 L02 Schema 写记录；`dock → building` 映射需与 L33/L34 对 A 系列 domain=ship 的历史口径做说明性衔接（已写入第 7 节弃用映射）。
- D19/D21 使用边界在对应章节任务卡落地时引用本词典第 9 节。
