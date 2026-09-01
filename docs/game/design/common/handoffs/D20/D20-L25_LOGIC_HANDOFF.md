# D20-L25 Logic Handoff

## 状态

- task_id：`D20-L25`
- status：`LOGIC_PASS`
- 结果文件：[D20_L25_LOGIC_RESULT.json](../../../../../../game/tools/results/D20/D20-L25/D20_L25_LOGIC_RESULT.json)

## 已完成

- 新增 `D20AquaticBehaviorAdapter`，只读消费 D4 ecology snapshot 与 D11 whale/dolphin 的公开 runtime snapshot、encounter 和 ecology/environment read snapshot。
- 输出 fish school、whale、dolphin 的实体种类、可见性、观察/捕鱼/护航/救援/捕鲸交互状态、风险、路线/栖息地、生态压力和来源 revision。
- observation blocked/visibility 不足会阻断观察；海豚护航仅在公开 `DolphinEscortAvailable` 下可用；搁浅救援仅在 `IsStranded` 下可用；捕鲸保持 `OwnerRequired`，不由 D20 绕过政策 owner。
- 适配器不调用 D4/D11 的推进、迁徙、resolve、救援、释放或其他事实写入 API；读取前后 owner revision 不变。

## 公开接口与接线

- 构造：`D20AquaticBehaviorAdapter(D4EcologySystem ecologySystem = null, D11WhaleDolphinSystem whaleDolphinSystem = null)`。
- 读取：`ReadFishSchool`、`ReadEncounter`、`ReadMarineEncounter`、`Read`、`EvaluateInteraction`、`Snapshot`。
- 纯函数：`ReadFishSchool(EcologyState, ...)`、`ReadEncounter(D11MarineEncounter, ...)`、`BuildSnapshot(EcologyState, D11WhaleDolphinRuntimeSnapshot)`。
- 返回 read model 和 interaction evaluation 均为副本，不暴露 D4/D11 owner 内部可变对象。

## 依赖核对

- 执行时未找到 D20-L20、D20-L21、D20-L22 的生产源文件、结果文件和 handoff 文件。
- 本任务仅使用当前存在的 D4/D11 公开读取面，未复制或臆造缺失的 D20 类型；后续整合必须保持生态与海洋 owner 的唯一写者原则。

## 验证证据

- `dotnet build game/Whalebone.Systems.csproj --no-restore --nologo`：0 warnings，0 errors。
- `dotnet build .codex-temp/D20LogicCompile.csproj --no-restore --nologo`：D20-L23/L24/L25 纯 C# 目标编译通过，0 warnings，0 errors。
- `dotnet run --project .codex-temp/D20LogicSmoke.csproj --no-restore --nologo`：`D20-L23/L24/L25 LOGIC SMOKE PASS`，含 marine visible/observe、捕鲸 owner-required、blocked observation 与 revision 不变断言。
- 禁止依赖扫描 0 hits；L24/L25 owner mutation calls 0；目标文件尾随空白 0；`git diff --check` exit code 0。

## 待办与运行时延期

- Unity 海洋/动物表现、场景/Prefab、UI、输入、音频、物理、设备、PlayMode、真实交互呈现和性能验证均未执行。
- 真实海洋运行时编排、可见交互和发布验收需后续 Runtime/集成任务完成；本 handoff 不声明 `RuntimeApproved`、发布就绪或真实设备通过。

## Coordinator memory entry

请协调者将本任务结果同步到共享状态与 2026-09-01 记忆；本子任务未修改全局状态文件或 `.workbuddy/memory`。
