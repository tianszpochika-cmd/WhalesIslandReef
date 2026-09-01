# D20 角色、NPC、动物逻辑最终验收

> D20-L40；这是 D20 纯逻辑章节的唯一门禁文档。

## 证据核对

- D20-L01—L35：35/35 `LOGIC_PASS`，结果 JSON 和 handoff 全部存在并可解析。
- D20-L36：`tools/d20_logic_boundary_audit.py` 扫描 34 个纯逻辑生产/测试文件，0 个 P0 违规；自测可识别 Unity Input API，同时不误报普通 `Input` DTO 字段。
- D20-L37：`tools/d20_logic_test_runner.py` 统一 runner `PASS`；边界自测、边界扫描、Systems 编译、D20 smoke、L28—L35 batch 编译/执行和 35 项证据完整性均通过。
- D20-L38：公开接口、owner boundary、命令/结果/事件/保存方向已发布；D20-L40 增补了 D10/Crew → D20 船员公共只读投影、来源 owner、严格校验、Clone/确定性排序和下游岗位映射边界。
- D20-L39：Runtime/Player/画面/性能/实机延期登记已发布。
- D20-L40 crew projection 回归：D20 投影断言 14/14、D21 映射/Start/拒绝/直接 roster/owner 不写断言 16/16；联合纯 C# 编译与执行退出码均为 0。

## 逻辑放行范围

D20 已完成角色身份与 presence、动作优先级/中断/事件、NPC 日程/行为/交互资格、动物/宠物行为投影、content_id 约束、保存投影/迁移、幂等、确定性回放、跨 D19 公开路由边界，以及 D10/Crew → D20 的船员公共只读投影。D20 不拥有 D10/D11/D19/经济/关系/库存/任务的事实。

## 明确未放行范围

没有执行 Unity Player/PlayMode、场景、Prefab、模型、Animator、NavMesh、真实输入、Canvas、网络、LLM、真机、画面、性能或发行包测试；这些全部保持 `runtime_deferred`。

特别说明：D21-L06 的 crew projection 现在有 D20 公共只读投影和 D21 映射证据；D20 仍不修改 D21 结果来代替 D21 自己的岗位/人数/状态规则。D21-L27 的补给性能文件未在本次任务修改。

## 最终结论

在纯逻辑开发范围内：**D20 = LOGIC_PASS**。

下游放行：允许 D21 重新接入 D20 公共角色/NPC 投影，允许 D22/D23 消费本文列出的只读接口；不代表 Runtime/Player/Release 已通过。
