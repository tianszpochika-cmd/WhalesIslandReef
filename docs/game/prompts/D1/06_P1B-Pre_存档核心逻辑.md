# 【提示词 P1B-Pre】存档核心逻辑（纯 C#，可在 Unity 下载期间执行）

## 角色与任务
你是《鲸骨航线》的强逻辑层工程师。任务：在 **Unity 尚未安装**的情况下，先完成存档系统的纯 C# 核心逻辑——数据模型、种子数据、新游戏生成、序列化。全部不依赖 Unity API，可独立 `dotnet build` 验证（仿 `game/AgentCore/` 模式）。

## 必读
1. `docs/AGENTS.md`（铁律——尤其"强逻辑红线"：禁止 LLM/网络；"AgentCore 纯 C#"精神同样适用于本任务）
2. `docs/game/design/DATA_MODELS.md` 第 1-7 节（字段与表结构）
3. `docs/DEVELOPMENT_STANDARDS.md` 第 5 节（命名）与第 8 节（DoD）

## 工作目录
新建 `game/GameCore/`（独立 dotnet 项目，命名空间 `Whalebone.Game`，TargetFramework net9.0，LangVersion 9，**零第三方依赖**——JSON 序列化用轻量自研或 `System.Text.Json` 的 net9 原生支持均可，但**不得引用 Unity 或 NuGet 第三方包**）。

## 任务清单
1. **数据模型**（对应 DATA_MODELS 字段，纯数据类 + 序列化友好）：
   - `PlayerData`（name/money/skills/traits/reputation/inventory/shipId/locationJson/playSeconds）
   - `ShipData`（shipTypeId/name/hull/cargo/cannons/supplies/crewIds/upgrades/state）
   - `CrewAgentData`（name/role/culture/traits/stats/persona/memory/relations）
   - `TraitDef` / `ShipTypeDef`（定义表结构）
2. **种子数据**（静态表，`SeedData.cs`）：
   - `trait_defs` 首批 **20 条**：身体 8（强壮/瘦弱/肺病/肠胃好/老花眼/好视力/抗热/酒量好）+ 性格 8（贪婪/怯懦/忠诚/嗜酒/多疑/冲动/谨慎/豪爽）+ 经历 4（老水手/前私掠者/逃犯/医生学徒）。每条含 statMods（如 肺病→fatigue_max:-0.2）、personaNote（中文一句话，如"你时常咳嗽，爬桅杆吃力"）
   - `ship_type_defs` 首批 **2 条**：`small_fishing` 小渔船、`sloop` 单桅纵帆船（speed/turn/cargoMax/crewMax/cannonMax/hullMax/draft/price，值自定且史实合理）
3. **新游戏生成逻辑**（`GameFactory.cs`）：`NewGame(captainName)` → 生成 Player（初始金钱 500、技能默认 1）+ Ship（small_fishing 满耐久）+ 老水手 CrewAgent（**随机 3-4 条特质**：1-2 身体 + 1-2 性格，从种子表抽取；culture=eng、role=deckhand）
4. **序列化接口**（`ISaveStore` 抽象，为 Unity 阶段接 SQLite 预留）：
```csharp
public interface ISaveStore
{
    bool HasSave();
    void Write(SaveBundle bundle);   // SaveBundle = { PlayerData, ShipData, CrewAgentData[] }
    SaveBundle Read();
    void Clear();
}
```
   提供 `JsonFileStore`（写 JSON 文件到指定目录，供本阶段自测用；**Unity 阶段换成 SQLiteStore** 由 P1B 实现）。
5. **自测**（`GameCoreTests.cs` + 入口，仿 AgentCore 的 Bootstrap/测试模式）：`dotnet build` 后运行验证：NewGame → 数据完整（Player/Ship/老水手/特质 3-4 条）→ Write → Read → 字段一致；种子条数 20+2。

## 验收（DoD）
- [ ] `dotnet build` 通过；自测全绿（输出到终端）
- [ ] 零 Unity API、零第三方包；命名规范符合 STANDARDS 第 5 节
- [ ] 种子数据中文文案、史实合理；随机特质逻辑正确

## 输出与留痕
`git commit -m "feat: D1-P1B-Pre 存档核心逻辑 (GameCore)"`；回报：项目路径、build/自测输出、接口签名（ISaveStore 最终版——**这是 P1B 接入 Unity 时的契约**）。
