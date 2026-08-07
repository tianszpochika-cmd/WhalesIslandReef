# 设计阶段文档（design/）

> 需求已过审（REQUIREMENTS v1.0），进入设计阶段：把"做什么"转成"怎么做、边界、数据"。

## 文档地图

| 文档 | 定位 | 状态 |
|---|---|---|
| `DESIGN_BOUNDARIES.md` | ★ 边界总纲（玩家/船员/AI/NPC/动物/系统间） | ✅ 已建 v1.0 |
| `DATA_MODELS.md` | 实体与数据模型 | 🟡 骨架 |
| `BALANCE.md` | 数值平衡基线 | 🟡 骨架 |
| `COMBAT.md` | 战斗设计 | 🟡 骨架 |
| `PROGRESSION.md` | 成长与进度 | 🟡 骨架 |

## 设计阶段工作流

1. 任何新系统/元素：先查 `IDEA_BANK` → 支柱排序 → 过 `DESIGN_BOUNDARIES` 边界
2. 写实现前：先落数据模型（DATA_MODELS）→ 数值基线（BALANCE）→ 边界确认
3. 边界冲突：按 DESIGN_BOUNDARIES 第 7 节仲裁
4. 契约先行：改动数据模型须同步 `contracts/` 与 `game/AgentCore`
