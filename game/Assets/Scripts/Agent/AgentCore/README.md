# AgentCore 硬链接镜像目录

> 本目录是 `game/AgentCore/` 的 **NTFS 硬链接镜像**（同一物理文件，编辑任一侧即同步两侧）。

## 铁律（必须遵守）

1. **唯一写者 = `game/AgentCore/`**：所有源码修改都在 `game/AgentCore/` 中进行
   （它是纯 C# 独立库，铁律 4：可独立 `dotnet build`，AgentCore.csproj 不动）。
2. **本目录禁止直接编辑**：这里的 .cs 与 AgentCore 是同一物理文件，改这里就是改 AgentCore，
   但为了保持"唯一写者"约定清晰，请一律从 `game/AgentCore/` 改起。
3. **新增/删除文件时**：在 `game/AgentCore/` 添加文件后，必须在本目录同步建立硬链接：
   ```powershell
   New-Item -ItemType HardLink -Path "Assets/Scripts/Agent/AgentCore/<新文件>.cs" `
            -Target "..\..\..\..\AgentCore\<新文件>.cs"
   ```
4. **git checkout 后若硬链接失效**（Git 可能以独立文件重建）：运行根目录工具
   `tools/rebuild_agentcore_links.ps1` 重建全部链接。

## 当前链接文件

| Unity 侧（本目录） | 源（game/AgentCore/） |
|---|---|
| AgentCoreTests.cs | AgentCoreTests.cs |
| AgentMemory.cs | AgentMemory.cs |
| AgentModels.cs | AgentModels.cs |
| Bootstrap.cs | Bootstrap.cs |
| CrewAgent.cs | CrewAgent.cs |
| DialogueSession.cs | DialogueSession.cs |
| LLMClient.cs | LLMClient.cs |
| MutinyMonitor.cs | MutinyMonitor.cs |

## 归属程序集

本目录归属 asmdef `Whalebone.Agent.Runtime`（见 `../Whalebone.Agent.Runtime.asmdef`）。
注意：AgentCore 含 LLM 网络代码（LLMClient.cs），因此其程序集属于 **Agent 层**，
绝不能放入 `Assets/Scripts/Systems/`（Systems 红线：禁 LLM/网络）。
