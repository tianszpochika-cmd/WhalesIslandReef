# D24 纯逻辑候选版基线

> 对应任务：`D24-L01`。版本：2026-09-02（Asia/Shanghai）。
> 本文冻结 D18—D23 的逻辑接受入口、D24 回归命令、候选版定义与 Runtime/发行隔离。

## 1. 基线性质

D24 的“逻辑候选版”只表示：契约、纯 C# owner API、存档投影/迁移、状态与错误边界、离线静态审计、确定性单测和跨章节模拟均有可重跑证据。它不是可玩候选版，不表示 Unity Player/PlayMode、画面、输入、设备、性能、网络、LLM、真实存档介质或 Steam 发行通过。

D24 只接受以下三类结果状态：`LOGIC_PASS`、`LOGIC_BLOCKED`、`LOGIC_FAIL`。任何缺失证据、失败退出码、跨端契约不一致或数值 owner 越权都必须阻止候选版结论；不能用编译成功、目录存在或旧截图替代逻辑证据。

## 2. D18—D23 放行快照

当前 D24-L25 runner 读取并校验 D18—D23 全部计划任务的结果/交接链：

| 章节 | 终审入口 | 任务数 | 当前状态 | D24 消费方式 |
|---|---|---:|---|---|
| D18 | `D18-L50`、`D18-L53` | 53 | `LOGIC_PASS` | 读取资源逻辑状态、稳定键和 Runtime Deferred 证据；不消费二进制导入批准 |
| D19 | `D19-L48` | 48 | `LOGIC_PASS` | 使用港口/泊位公开命令、错误目录、Save projection/migration |
| D20 | `D20-L40` | 40 | `LOGIC_PASS` | 使用 identity/presence、角色/NPC/动物 projection 和迁移接口 |
| D21 | `D21-L32` | 32 | `LOGIC_PASS` | 使用船舶、天气、航程 Tick、事件和 Save projection |
| D22 | `D22-L36` | 36 | `LOGIC_PASS` | 使用 UI command/feedback/read-model 的纯 DTO 边界，不消费 Canvas 状态 |
| D23 | `D23-L38` | 38 | `LOGIC_PASS` | 使用固定种子全链路、回放、中断、长跑与保存恢复证据 |

L25 的当前真实协议快照为 `247/247` 结果、`247/247` handoff、`247/247` `LOGIC_PASS`。旧章节中少量扩展字段或简化 handoff 文本按兼容形状记录为 finding，不改变结果 JSON 的任务状态，也不遮蔽缺失文件/错误状态。

## 3. 跨端契约和版本事实

以下值直接来自当前契约或已接受的纯逻辑实现，作为 D24 回归的输入事实：

| 契约/投影 | 当前版本事实 | 唯一事实源 |
|---|---|---|
| AgentReply | `agent_reply.v1.1` | `contracts/agent-reply.schema.json`；game/server/web 均须遵守同一 Schema |
| WorldEvent | `schema_version=1` | `contracts/world-event.schema.json` |
| HTTP API | OpenAPI `0.1.0` | `contracts/openapi/v1.yaml` |
| P2A action registry | `version=d8-a04`、`registry_version=p2a.v1` | `contracts/p2a-action-registry.json` |
| D19 港口 Save | `D19PortSaveProjection.CurrentSchemaVersion=1` | `game/Assets/Scripts/Systems/D19/D19PortSaveProjection.cs` |
| D20 角色 Save | 迁移器从 `D20CharacterSaveProjection.CurrentSchemaVersion` 读取支持版本 | `game/Assets/Scripts/Systems/D20/D20CharacterSaveMigration.cs` |
| D21 船舶/航程 Save | `D21ShipSaveProjection.CurrentSchemaVersion=1` | `game/Assets/Scripts/Systems/D21/D21ShipSaveProjection.cs` |

Agent/LLM 只能提交被 Schema 接受的对白、关系姿态或 action intent；金钱、价格、伤害、奖励、天气和关系数值仍由所属强逻辑 owner 裁决。契约审计必须同时看到 game、server 与 web 的消费标记，不能新增单端私有协议。

## 4. D23 固定场景输入

D24 回归复用 D23 已接受的纯逻辑夹具，固定输入如下：

| 字段 | 固定值 |
|---|---|
| seed | `2301` |
| world_day | `23` |
| save_revision | `230100` |
| player_id | `player_d23_001` |
| ship_id | `ship_d23_sloop_001` |
| port_id | `port_santa_margarita` |
| berth_id | `berth_santa_margarita_dock_01` |
| commodity_id | `sugar` |
| voyage_id | `voyage_d23_001` |

D23 的当前回归硬指标为 `D23_CASES=26`、`D23_ASSERTIONS=13925`、`D23_FAILURES=0`；L26 固定基线阻止 case 数、断言数下降或 failure 数上升。D24-L27 再以 10 次同 tag full-loop、一次 interruption matrix 和一次 50 Tick long-run 对同一摘要做跨章节回归。

## 5. 可重复命令

候选版必须由当前 checkout 重新执行下列命令获得机器证据，不手改数字：

```text
python tools/d24_logic_regression_runner.py --root . --configuration Release --json-out game/tools/results/D24/D24-L25/D24_L25_RUNNER_REPORT.json
python tools/d24_regression_baseline_compare.py --root . --baseline game/tools/results/D24/D24-L26/D24_L26_BASELINE.json --current game/tools/results/D24/D24-L25/D24_L25_RUNNER_REPORT.json --json-out game/tools/results/D24/D24-L26/D24_L26_COMPARE.json
python tools/d24_logic_regression_runner.py --root . --configuration Release --include-d24-l27 --json-out game/tools/results/D24/D24-L27/D24_L27_RUNNER_REPORT.json
```

L25 固定执行 Python 语法、15 项 D24 逻辑审计、D24-L17—L22 纯 C#、AgentCore build/`--d9-check` 和 D23 26-case runner；L26 读取固定 baseline 并检查数量、关键标记、迁移器和错误码；L27 的临时 .NET 工程只引用已通过构建的纯逻辑 DLL。

## 6. 唯一写者与迁移边界

- D3 继续拥有金钱/库存；D19 通过公开港口/仓库 adapter；D20 只提供 identity/presence 和角色/NPC/动物 projection；D21 拥有船舶/航程自身状态；D22 只拥有 UI 逻辑投影；D23/D24 只编排和验证，不复制或裁决领域事实。
- Save migration 必须 clone 输入、校验版本/不可能状态、保留原始数据并在拒绝时 fail closed；D19、D20 的迁移器和 D21 Save projection 是基线中的关键 marker。
- 关键错误码必须保持可定位、可本地化且不把 `Rejected`、`Blocked`、`Pending`、`Duplicate` 或 `Cancelled` 伪装为业务成功。L26 对 D19 failure catalog、D22 feedback catalog 和迁移失败 marker 做源码存在性检查。

## 7. Runtime/发行隔离

本基线不执行也不批准：Unity Player/PlayMode、场景/Prefab/Canvas/UI、真实键鼠/手柄/触摸、Transform/Animator/物理/NavMesh、模型/材质/音频/VFX、分辨率与布局、设备、性能、网络、LLM/Agent runtime、真实 SaveSystem 介质、Steam 或发行包。所有这些写入 Runtime Deferred register；`LOGIC_PASS`、D24 logic candidate、编译成功和离线模拟通过均不等于 `RuntimeApproved` 或 `ReleaseApproved`。
