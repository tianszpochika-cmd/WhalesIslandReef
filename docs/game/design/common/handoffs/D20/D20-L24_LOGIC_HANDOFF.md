# D20-L24 Logic Handoff

## 状态

- task_id：`D20-L24`
- status：`LOGIC_PASS`
- 结果文件：[D20_L24_LOGIC_RESULT.json](../../../../../../game/tools/results/D20/D20-L24/D20_L24_LOGIC_RESULT.json)

## 已完成

- 新增 `D20WildlifePopulationAdapter`，只读消费 `D4EcologySystem.Snapshot`。
- 输出栖息地、动物和鱼群的稳定读模型，包含实体/物种/栖息地 ID、生命/行为、health、biomass、capacity、fishing pressure、ecology stress、pollution、turbidity、revision 和来源 owner。
- 对 present、depleted、dead、collected、invalid 与缺失数据给出明确状态、观察资格、交互资格、风险等级和反馈文本。
- 适配器不调用 `Seed`、`Tick`、`Move`、`Commit` 或其他 Ecology owner 写入方法，不拥有任何人口、压力、栖息地、死亡或资源事实。

## 公开接口与接线

- 构造：`D20WildlifePopulationAdapter(D4EcologySystem ecologySystem = null)`。
- 读取：`ReadAnimal`、`ReadFishSchool`、`Read`、`Snapshot`。
- 纯函数：`ReadAnimal(EcologyState, ...)`、`ReadFishSchool(EcologyState, ...)`、`BuildSnapshot(EcologyState)`。
- 返回的 habitat、animal、fish school 和 records 均为独立副本；不存在的实体保持 `InputAvailable=false`，不会伪造人口事实。

## 依赖核对

- 执行时未找到 D20-L20、D20-L21、D20-L22 的生产源文件、结果文件和 handoff 文件。
- 本任务使用当前存在的 D4 ecology 公共快照，未复制或臆造缺失的 D20 类型；后续内容模型接线应继续通过公开契约完成。

## 验证证据

- `dotnet build game/Whalebone.Systems.csproj --no-restore --nologo`：0 warnings，0 errors。
- `dotnet build .codex-temp/D20LogicCompile.csproj --no-restore --nologo`：D20-L23/L24/L25 纯 C# 目标编译通过，0 warnings，0 errors。
- `dotnet run --project .codex-temp/D20LogicSmoke.csproj --no-restore --nologo`：`D20-L23/L24/L25 LOGIC SMOKE PASS`，含鱼群成功、缺失动物失败与输入快照不变断言。
- 禁止依赖扫描 0 hits；L24/L25 owner mutation calls 0；目标文件尾随空白 0；`git diff --check` exit code 0。

## 待办与运行时延期

- Unity EcologySystem 注入、世界/场景/Prefab、UI、输入、动画、物理、设备、PlayMode、真实群落展示和性能验证均未执行。
- D20-L20/L21/L22 内容模型与运行时整合需后续任务完成；本 handoff 不声明 `RuntimeApproved`、发布就绪或真实设备通过。

## Coordinator memory entry

请协调者将本任务结果同步到共享状态与 2026-09-01 记忆；本子任务未修改全局状态文件或 `.workbuddy/memory`。
