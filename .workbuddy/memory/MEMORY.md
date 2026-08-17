# 项目长期约定（MEMORY）— 鲸骨航线

> 项目级 curated 记忆：跨会话必须遵守的约定与关键事实。每次任务结束前回读核对。

## 项目身份

1675 年加勒比海开放世界航海沙盒 RPG（纯 Agent 游戏）：AI 船员社会 × 航海贸易 × 真实主义。
Steam 目标（大陆个人开发者，无版号海外发行路径）。三端一体：Unity 客户端 + NestJS 服务端 + Next.js 官网，contracts/ 契约桥接。

## 铁律 5 条（自动加载，见根 AGENTS.md）

1. 契约先行：改 contracts/ 必须三端同步；禁止单端私有协议
2. 强逻辑红线：Systems/ 禁 LLM/网络代码；数值修改只在所属系统类内
3. 数据所有权唯一：LLM 只提议不裁决；输出过 AgentReply Schema 校验
4. AgentCore 纯 C#：零 Unity/第三方依赖，可独立 dotnet build
5. 产出必须留痕：代码 + 文档 + 当日记忆日志

## 文档地图（先读索引，别全读）

- 开工对齐：`docs/CONTEXT.md`（必读）
- 工程规范：`docs/DEVELOPMENT_STANDARDS.md`（所有权/拆分/耦合/DoD）
- 架构与层级：`docs/ARCHITECTURE.md`
- 设计宪法：`docs/game/DESIGN_PILLARS.md` ｜ 边界：`docs/game/design/DESIGN_BOUNDARIES.md`
- 需求：`docs/game/REQUIREMENTS.md`（v1.0 过审）｜ 创意池：`docs/game/IDEA_BANK.md`
- 数据模型：`docs/game/design/DATA_MODELS.md`
- D16/D17 剩余门禁：`docs/game/AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATES.md`（C 编号；有效 `D16-C01`—`C12`、`D17-C01`—`C04`，与 D19 的 L 编号分轨）
- 真机推迟：`docs/game/design/common/D16_EXTERNAL_HARDWARE_DEFERRED.md`（C09/C10/C11 开发阶段不跑真机，保持 BLOCKED，禁止改 PASS）

## 关键技术决策（勿轻易推翻）

- 年份 **1675**；完全写实无奇幻；沙盒（后期岛屿经营）；不设解密门槛
- 引擎 Unity 6 + URP + Crest Ocean；Agent 混合架构（本地 Ollama/qwen2.5 优先 + DeepSeek 云端兜底）
- 服务端 NestJS 11 + TS7 + Prisma/PostgreSQL + Redis；token 双轨（上游 token 成本侧 vs 玩家点数售卖侧）
- 8 大系统；MVP 分级 M0→M3（分级=执行顺序，非删减）
- 战斗=简化格斗；奴隶商品已移除（改契约劳工）；疾病用疟疾/黄热病（无梅毒）
- 多人=EA 后；服务器与目标用户放海外（规避境内网络游戏运营监管）

## 环境备注

- 本机：i5-13600KF / 32GB / RTX 4070 Ti 12GB / Win11，dotnet 9 + node 22 可用
- 环境无 sleep 命令（等待用 curl --retry-connrefused）；Bash 删除受限（safe-delete），用 PowerShell Move-Item
