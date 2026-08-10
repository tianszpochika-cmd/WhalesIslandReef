# 存档系统（SAVE_SYSTEM）— D1-P1B 实现说明

> 状态：**已实现**（2026-08-08）｜ 对应 DATA_MODELS §7 存档映射
> 范围：SQLite 存档 + 新游戏数据流（Player/Ship/CrewAgent 生成）
> 契约：`SaveSystem` 公共 API 由 P0 定稿，本阶段未改签名（仅新增 GameState 与内部测试扩展）

---

## 1. SQLite 方案

| 项 | 选型 | 理由 |
|---|---|---|
| 托管层 | **自写 P/Invoke 封装**（`Db/SqliteNative.cs` + `Db/SqliteDb.cs`） | 零第三方托管依赖；只需一个原生库；符合 Systems 层"少依赖"气质 |
| 原生库 | **sqlite.org 官方预编译 `sqlite3.dll`（x64, 3.53.4）**，置于 `Assets/Plugins/x86_64/` | 官方源、可离线复现；编辑器（EditMode 测试）与 Windows 构建均加载此插件 |
| JSON | 自写 `Json/SaveJson.cs`（递归下降编解码） | Unity `JsonUtility` 不支持 Dictionary；`MiniJson`（AgentCore）只读不可写 |

> 落选方案及原因：
> - `com.unity.sqlite`：Unity 6 未发布该包（registry 无）；
> - `Mono.Data.Sqlite`（Unity 自带托管库）：依赖 `System.Data`，与 Unity 6 默认 .NET Standard 2.1 profile 兼容性风险高；
> - `sqlite-net` 源码包：官方源在 GitHub，本机网络不可达；SQLitePCLRaw dll 链多文件 + 初始化繁琐。
>
> 多平台备注：M0 仅支持 Windows（编辑器/构建）。Android/iOS 需另行放置对应 ABI 的 sqlite3 原生库（D1 不阻塞，记录待办）。

## 2. 表结构（与 DATA_MODELS §7 一致，5 张表）

```sql
players(id TEXT PK, name TEXT, money INTEGER, skills_json TEXT, traits_json TEXT,
        reputation_json TEXT, inventory_json TEXT, ship_id TEXT,
        location_json TEXT, play_seconds INTEGER)
ships(id TEXT PK, ship_type_id TEXT, name TEXT, hull INTEGER, cargo_json TEXT,
      cannons INTEGER, loaded_count INTEGER, supplies_json TEXT, crew_json TEXT,
      upgrades_json TEXT, state_json TEXT, ship_log_json TEXT)
crew_agents(id TEXT PK, name TEXT, role TEXT, culture TEXT, traits_json TEXT,
            stats_json TEXT, persona_json TEXT, memory_json TEXT, relations_json TEXT)
trait_defs(id TEXT PK, name TEXT, category TEXT, desc TEXT, stat_mods_json TEXT,
           persona_note TEXT, event_mods_json TEXT)          -- 种子（20 条）
ship_type_defs(id TEXT PK, name TEXT, speed REAL, turn REAL, cargo_max INTEGER,
               crew_max INTEGER, cannon_max INTEGER, hull_max INTEGER,
               draft REAL, price INTEGER)                    -- 种子（2 条）
```

- 迁移：`CREATE TABLE IF NOT EXISTS` + `PRAGMA user_version = 1`（M0 简单幂等，无破坏性迁移）；
- 种子幂等：`INSERT OR IGNORE`（按 id），重复执行不产生脏数据；
- `ships.hullMax` 不落库：Load 时按 `ship_type_id` 回查 `ship_type_defs.hull_max`（数据单一事实源）。

## 3. 种子数据

- **trait_defs 20 条**：身体 8（强壮/瘦弱/肺病/好视力/医学徒/落难贵族/前海军/酒量好）+
  性格 8（贪婪/怯懦/忠诚/嗜酒/豪爽/记仇/乐天/谨慎）+ 经历 4（老水手/前私掠者/逃犯/传教士）；
  - `stat_mods`/`event_mods` 数值约定见 `TraitDef.cs` 文件头（属性 ±%、技能绝对值、事件概率修正）；
  - 疾病类（疟疾/黄热病等）按 DATA_MODELS 约定 **M1** 引入。
- **ship_type_defs 2 条**（史实锚定见 `ShipTypeDef.cs` 文件头）：
  - `small_fishing` 小渔船：speed 4.0 / turn 6.0 / cargo 40 / crew 4 / cannon 0 / hull 60 / draft 0.8m / 300 银币
  - `sloop` 单桅纵帆船：speed 7.0 / turn 5.0 / cargo 120 / crew 12 / cannon 8 / hull 150 / draft 1.6m / 3000 银币

## 4. NewGame 数据流（`SaveSystem.NewGame(NewGameParams)`）

1. 建表 + 种子落库（幂等）；
2. **覆盖旧档**：清空 `players/ships/crew_agents` 三表（新游戏语义；种子表保留）；
3. 生成 Player：船长名 / 金钱 500 / 技能全 1 / 初始位置=圣玛格丽特港（`santa_margarita`）/ 随机 3-4 条特质；
4. 生成 Ship：`small_fishing`（调用方未指定时回退）、满耐久、默认补给（水/食物 20）、空货舱；
5. 生成初始船员**老水手**：固定含 `trait_old_sailor` 经历特质 + 随机 2-3 条（共 3-4 条）、culture=eng、role=deckhand；
6. 写入内存 `GameState` + 落库三表 → `HasSave() == true`。

## 5. Save / Load

- `Save()`：GameState（Player/Ship/CrewAgent）→ `INSERT OR REPLACE` 回三表（幂等覆盖）；
- `Load()`：读三表 → 还原内存对象 → `true`；无存档/损坏 → `false`（主菜单"继续"按钮据此置灰）。

## 6. GameState（静态持有，供 P2 及后续阶段）

- `CurrentPlayer`（PlayerState）/ `CurrentShip`（ShipState）/ `CrewAgents`（List\<CrewAgent\>）；
- `CrewAgentTraitIds`：crewId → TraitId[]（**临时桥接**，见第 7 节）；
- 单一写者：NewGame/Load/Save 是唯一修改入口；各系统只读 + 经 `Save()` 写回。

## 7. 已知限制与后续（P2 解除）

1. **AgentMemory 不还原**：`AgentMemory` 内部状态私有，M0 的 `memory_json` 仅存骨架；Load 后以空记忆重建（事件持久化随 IMemoryStore 接入 SQLite 时解决）；
2. **特质 id 桥接**：AgentCore 的 `PersonaDefinition.Traits` 为显示名（List\<string\>），
   而 DATA_MODELS §3 要求 `crew_agents.traits_json` 存 TraitId —— 当前由 `GameState.CrewAgentTraitIds`
   维护映射，**待 AgentCore 扩展**（Persona.Traits → TraitId[] + 数值修正注入）后迁移；
3. **role/culture 固定**：M0 初始船员统一 `deckhand`/`eng`，P2 船员系统接管维护；
4. **多平台 sqlite3**：Android/iOS ABI 原生库未引入（D1 范围外）。

## 8. 测试

- EditMode 单测：`Assets/Tests/EditMode/SaveSystemTests.cs`（Unity Test Framework，8 用例）：
  NewGame → HasSave → Save → Load 字段还原一致；种子 20+2；分类 8/8/4；种子幂等；
- 每个用例使用独立临时 db（`SaveSystem.DbPath` 覆盖，经 `InternalsVisibleTo("Save.Tests")`），不触碰真实存档；
- 脱离 Unity 的同构自测：`/tmp/wb_selftest`（dotnet 控制台 + UnityEngine stub + AgentCore 引用），44 项断言全过。
