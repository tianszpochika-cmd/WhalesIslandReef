# D24 逻辑边界审计

> 对应任务：`D24-L33`。本文按 D24-L25/L26/L27 的真实机器证据审查所有权、契约、AgentCore、Systems、资源状态和 Runtime 隔离。

## 1. 审计结果

| 边界 | 真实证据 | 结论 |
|---|---|---|
| 单一写者 | `d24_data_ownership_audit.py`：合法 D20 Save projection 写入按 `LEGAL_OWNER_WRITE` 分类，未授权写入为 0 | 通过；D23/D24 只编排/验证 |
| game/server/web 契约 | `D24CrossEndContractTests`：contract id 存在，game/server marker 存在，web validator consumer=1 且有效 | 通过；无单端私有协议 |
| Systems 禁止依赖 | `D24NoNetworkInSystemsTests`：`systems_files=103`、`forbidden_hits=0`、LLM token boundary self-check 通过 | 通过；D18/D23 可选缺失不被伪装成 Runtime 证据 |
| AgentCore 独立性 | `D24NoUnityInAgentCoreTests`：target `net9.0`、linked sources=21、package refs=0、project refs=0；AgentCore build/`--d9-check` 通过 | 通过；未将 Unity/第三方/网络引入核心 |
| Save/migration | D19/D20 migration、D21 Save projection 结构/marker/round-trip 通过 | 通过；失败保留原始 projection、Runtime Save 介质仍延期 |
| 资源状态 | provenance/historical debt 审计通过；Candidate/Pending/Editor-only 仍保持原状态 | 通过逻辑隔离；不授予导入/运行时批准 |
| Runtime/发行隔离 | L25 scope flags 全为 false；无 Player/PlayMode、场景、Prefab、Canvas、输入、设备、性能、网络、LLM 或 Steam 命令 | 通过；所有上述项目仍 Runtime Deferred |

## 2. 发现与处理

- D18—D23 旧结果中存在 34 个兼容形状 finding（扩展 JSON 字段、对象形 handoff 或 handoff 未重复写结果链接/状态）；对应任务结果均可解析、状态为 `LOGIC_PASS`、结果与 handoff 文件均存在，因此 L25 将其列为兼容信息而非隐藏 blocker。
- D24-L27 首次尝试引用 D23 project 时触发了仓库既有 Agent Runtime 缺少 `NumericConfigurationTests/V3` 的工程重建错误；最终入口改为引用已经真实编译的 D23/D22/Systems/AgentCore DLL，并在独立临时工程 `0 warning/0 error`、运行 `7003 assertions/0 failures`。这不是对缺失依赖的绕过式业务通过，而是缩小编译边界以保持纯逻辑任务范围。
- 资源来源/许可证、真实 SaveSystem、UI/输入/物理、网络/LLM 和设备/性能问题没有被本审计删除或升级；它们继续由 Runtime Deferred backlog 管理。

## 3. 禁止越界

D24 不允许在 `game/Assets/Scripts/Systems/` 增加 LLM/网络代码，不允许 D23/D24 写入 D3/D19/D20/D21/D22 事实，不允许 agent reply 携带裁决数值，也不允许 Candidate 直接迁移到 `RuntimeApproved`/`ReleaseApproved`。任何后续边界改动必须重新运行相应静态审计和 D24-L25/L26/L27。
