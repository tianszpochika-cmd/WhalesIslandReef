# D24 纯逻辑内容覆盖

> 对应任务：`D24-L29`。覆盖统计来自当前 D24/D23 离线证据；不把画面、资产导入或 Player 行为计入逻辑完成。

## 1. 已覆盖范围

| 领域 | 逻辑覆盖 | 机器证据 |
|---|---|---|
| 契约与三端消费 | AgentReply、WorldEvent、P2A registry、game/server/web 版本标记与 validator 边界 | D24-L05、D24-L22、contract audit |
| 内容与稳定 ID | content/resource/event key 唯一性、owner、来源、弃用和测试映射 | D24-L02—L04 |
| 所有权 | D3/D19/D20/D21/D22 唯一写者；Agent 只提议；合法 Save projection 写入不误报为越权 | D24-L04、data ownership audit |
| Save/migration | D19/D20 migration、D21 ship Save projection、unknown-field round-trip、拒绝/原始保留语义 | D24-L06、D24-L19、D24-L28 |
| 确定性 | 固定 seed、显式世界日/修订/Tick、事件排序、幂等、replay/fingerprint | D24-L07、D24-L17、D24-L27 |
| 经济/库存/事件 | 货币守恒、容量/归属、不变式、event_id/cause/revision/dedupe | D24-L08—L10、D23 full-loop |
| 错误/本地化/可访问性 | 稳定错误码、localization key、disabled reason、纯逻辑 accessibility read model | D24-L11—L13、D22 acceptance |
| 资源状态 | Candidate/Deferred 状态、来源证据和历史债务不越级 | D24-L14、D24-L16 |
| AgentCore/Systems 边界 | AgentCore 独立 .NET；Systems 无网络/LLM/墙钟/不可控随机 | D24-L20、D24-L21、D24-L23 |
| 跨章节场景 | 新游戏→港口→交易/仓库→登船→出航→40 Tick→抵达→返港→Save restore；replay/interruption/long-run | D23 26 cases，13925 assertions，0 failures；D24-L27 7003 assertions，0 failures |

## 2. 规模与回归指标

- D18—D23：`247/247` 结果 JSON、`247/247` handoff、`247/247 LOGIC_PASS`。
- D24 Python 逻辑审计：`15/15` 通过。
- D24-L17—L22：`6/6` 纯 C# 测试通过；L18 当前枚举盘点 `389` 个 enum、`2680` 个 enum value；L20 扫描 `103` 个 Systems 文件且 `0` forbidden hit；L21 链接 `21` 个 AgentCore source、无 package/project refs；L22 web consumer `1` 个且 validator 有效。
- D24-L27：临时纯 .NET build/run `0 warning/0 error`、`D24_ASSERTIONS=7003`、`D24_FAILURES=0`。
- D24-L24 tooling reproducibility：包含 AgentCore 在内 `16/16` 工具语义 JSON 稳定。

## 3. 未实现或未关闭的内容

这些项目不是 D24 逻辑审计可凭空关闭的内容：

- 真实 Unity 场景中的船体、港口、角色、动物、植被、材质、动画、音频、VFX、Shader、碰撞体、LOD 和资源导入；D18 的部分资源哈希仍保持 Pending。
- Player/PlayMode 的真实点击、键鼠/手柄/触摸、相机、Transform、物理、NavMesh、Canvas/UI 排版、分辨率、字幕和焦点设备行为。
- 真实 SaveSystem 磁盘/云端/崩溃恢复/并发同步、网络服务、LLM/Agent runtime、目标设备、性能采样、Steam 和发行包。
- 全仓库历史 D9/D15/D16 内容字段与 review-packet 基线仍需独立清理；它们不被 D24 纯逻辑 runner 隐式改写或伪装成通过。

## 4. P0/P1 缺口结论

当前 D24 逻辑门禁没有发现新的 P0 blocker；P1 风险主要是 Runtime 验证、真实 Save/设备/性能以及历史内容基线。若任一未来改动降低上述数量、删除关键 migration/error marker、引入第二写者或让固定回放漂移，必须把候选版降为 `LOGIC_BLOCKED`/`LOGIC_FAIL`，不能以本文件的历史统计继续放行。

## 5. 禁止的完成表述

在 D24 结果、handoff、发布说明或交接中不得写“可玩”“画面完成”“实机通过”“性能达标”“RuntimeApproved”“ReleaseApproved”或“Steam ready”。允许的结论只有当前纯逻辑任务状态，以及明确列出的 Runtime Deferred。
