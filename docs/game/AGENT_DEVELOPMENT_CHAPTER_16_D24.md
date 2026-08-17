# 第 16 章：D24 回归、迁移与逻辑候选版（36 项）

> 本章只验收“代码层面可交接的逻辑候选版”。它不做性能、画面、真实设备、跨平台或 Steam 发布验收。

## 复制给 Agent 的单任务执行提示词

```text
你是 WhalesIslandReef 的纯逻辑开发 Agent。直接执行任务 [TASK_ID]。阅读 AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md 和本文件任务卡。

只做逻辑回归、迁移、Schema、边界和确定性审计。禁止启动 Player/PlayMode、性能采样、截图、场景/Prefab/UI/资源二进制修改，也不得把逻辑候选版称为发布版。完成后写 D24 结果 JSON、handoff、记忆日志并刷新状态总览。
```

## 任务卡与执行顺序

| ID | 波次 | 写入范围（唯一） | 实施内容与逻辑验收 |
|---|---|---|---|
| D24-L01 | S | `docs/game/design/common/D24_LOGIC_CANDIDATE_BASELINE.md` | 冻结 D18—D23 接受的接口版本、测试命令、逻辑候选版定义和 Runtime/发布隔离。 |
| D24-L02 | P | `tools/d24_content_catalog_coverage_audit.py` | 审计内容目录是否有 ID、所有者、来源、弃用和测试覆盖，不核对模型展示。 |
| D24-L03 | P | `tools/d24_content_id_uniqueness_audit.py` | 审计跨章节 content_id/resource key/事件 ID 唯一性和迁移映射。 |
| D24-L04 | P | `tools/d24_data_ownership_audit.py` | 扫描单一写者违规、Presentation 私写、UI 私写和跨系统绕过。 |
| D24-L05 | P | `tools/d24_contract_compatibility_audit.py` | 校验 contracts、game/server/web 的版本/字段兼容；不新增私有协议。 |
| D24-L06 | P | `tools/d24_save_migration_audit.py` | 汇总 D18—D23 存档投影、迁移、弃用字段和不可逆失败策略。 |
| D24-L07 | P | `tools/d24_deterministic_seed_audit.py` | 审计随机种子、时间、事件顺序和测试重放是否可复现。 |
| D24-L08 | P | `tools/d24_currency_invariant_audit.py` | 审计贸易、船坞、酒馆、奖励、金融等货币守恒和边界。 |
| D24-L09 | P | `tools/d24_inventory_invariant_audit.py` | 审计库存、货舱、仓库、宠物/采集物、容量和归属不变式。 |
| D24-L10 | P | `tools/d24_event_integrity_audit.py` | 审计事件 ID、因果、排序、幂等、重放和存档关联。 |
| D24-L11 | P | `tools/d24_error_catalog_audit.py` | 审计错误码可定位、可本地化、无硬编码可见文本且无吞错。 |
| D24-L12 | P | `tools/d24_localization_key_audit.py` | 审计 D18—D23 用户可见逻辑 key 的缺失/重复/弃用。 |
| D24-L13 | P | `tools/d24_accessibility_logic_audit.py` | 审计可访问性读模型、禁用原因和反馈逻辑，不测实际视觉。 |
| D24-L14 | P | `tools/d24_asset_provenance_audit.py` | 调用 D18 逻辑审计，确认候选/运行时/发布状态未越级。 |
| D24-L15 | P | `tools/d24_test_matrix_audit.py` | 将每个 P0 领域需求映射到实际单元/契约/模拟测试。 |
| D24-L16 | P | `tools/d24_historical_debt_audit.py` | 将旧 R01—R03 运行时问题登记为 Runtime Deferred，阻止污染逻辑门禁。 |
| D24-L17 | P | `game/Assets/Tests/EditMode/D24/D24SeedReplayTests.cs` | 跨港口、角色、航海、UI 命令重放固定种子并比较摘要。 |
| D24-L18 | P | `game/Assets/Tests/EditMode/D24/D24StateCoverageTests.cs` | 检查所有公开状态/失败码有正向或负向测试覆盖。 |
| D24-L19 | P | `game/Assets/Tests/EditMode/D24/D24SchemaRoundTripTests.cs` | 验证所有公开 DTO/Save/事件的序列化往返与未知字段处理。 |
| D24-L20 | P | `game/Assets/Tests/EditMode/D24/D24NoNetworkInSystemsTests.cs` | 扫描 Systems 禁止网络、HttpClient、LLM 和环境依赖。 |
| D24-L21 | P | `game/Assets/Tests/EditMode/D24/D24NoUnityInAgentCoreTests.cs` | 验证 AgentCore 独立构建且无 Unity/第三方依赖。 |
| D24-L22 | P | `game/Assets/Tests/EditMode/D24/D24CrossEndContractTests.cs` | 校验 game/server/web 契约变更三端同步。 |
| D24-L23 | P | `tools/d24_agentcore_build_audit.py` | 运行并记录 AgentCore 独立 build 与相关测试。 |
| D24-L24 | P | `tools/d24_tooling_reproducibility_audit.py` | 校验 D18—D23 关键审计/测试工具可从干净工作目录运行。 |
| D24-L25 | S | `tools/d24_logic_regression_runner.py` | 汇总 D18—D23 审计、构建、单测、剧本和迁移到一次可重复执行。 |
| D24-L26 | S | `tools/d24_regression_baseline_compare.py` | 对比固定基线，阻止测试数量下降、关键错误码/迁移被删除。 |
| D24-L27 | S | `game/Assets/Tests/EditMode/D24/D24FullLogicRegressionTests.cs` | 执行跨章节确定性回归，不含 Unity 运行时测试。 |
| D24-L28 | S | `docs/game/design/common/D24_LOGIC_MIGRATION_COMPATIBILITY.md` | 发布支持版本、迁移策略、存档回退和数据丢失风险。 |
| D24-L29 | S | `docs/game/design/common/D24_LOGIC_CONTENT_COVERAGE.md` | 发布逻辑覆盖、未实现内容、P0/P1 缺口及不能声称完成的边界。 |
| D24-L30 | S | `docs/game/design/common/D24_RUNTIME_VALIDATION_BACKLOG.md` | 生成后续实机测试清单：画面、模型、输入、碰撞、UI、性能、设备和发布。 |
| D24-L31 | S | `docs/game/design/common/D24_LOGIC_RELEASE_NOTES.md` | 编写“逻辑候选版”说明，明确不是可玩候选/发布版。 |
| D24-L32 | G | `game/tools/results/D24/D24-L32/` | 审核总回归命令、失败分类、可重复性和证据路径。 |
| D24-L33 | G | `docs/game/design/common/D24_LOGIC_BOUNDARY_AUDIT.md` | 审核所有权、契约、AgentCore、Systems、资源状态和 Runtime 隔离。 |
| D24-L34 | G | `docs/game/design/common/D24_LOGIC_STATUS_REVIEW.md` | 审核 280 项结果 JSON、handoff、记忆日志和状态总览一致性。 |
| D24-L35 | G | `docs/game/design/common/D24_LOGIC_CANDIDATE_ACCEPTANCE.md` | 给出逻辑候选版唯一结论：PASS/PARTIAL/BLOCKED，禁止发布结论。 |
| D24-L36 | G | `docs/game/design/common/D24_NEXT_RUNTIME_PHASE_HANDOFF.md` | 仅在 L35 PASS 时发布后续 Runtime 阶段的有限测试入口和停止规则。 |

## 依赖

L02—L24 需要 D23-L38 `LOGIC_PASS` 后并行；L25—L31 串行；L32—L36 汇合。D24-L35 即使通过，也只能证明逻辑候选版，不替代视觉、性能、实机或 D16 发布门禁。
