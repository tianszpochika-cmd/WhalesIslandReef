# 统一设计文档（design/）

> 本目录回答“系统如何工作、数据谁负责、玩家如何感知和验收”。唯一产品需求入口是上级目录的 `REQUIREMENTS.md`；唯一开发计划入口是上级目录的 `DEV_ROADMAP.md`。

## 文档地图与领域目录层级

设计文档按领域组织；D1—D7 阶段记录已经移入 `docs/archive/milestones/D1-D7/`，当前根目录只保留统一设计事实源：

    design/
    ├── common/       共享设计、边界、数据和存档规则
    ├── gameplay/     玩家玩法闭环、资产、经营和小游戏
    ├── ship/         船体、船型、改装、航行、船坞和锚泊
    ├── character/    玩家、船员 Agent、NPC、特质和动作
    ├── vegetation/   植被目录、生境、动态和采集
    ├── animals/      动物目录、行为和水生生态
    ├── islands/      岛屿、地质、洞穴、海岸和自然结构
    ├── environment/  天气、水体、光照、声音和性能
    ├── buildings/    建筑、港口、船坞和交互
    ├── items/        道具、材料、工具和货物
    ├── economy/      贸易、金融、工资和维护
    ├── factions/     势力、宗教、法律身份和社群
    ├── combat/       海战、接舷、武器和战果
    ├── quests/       主线、支线、事件和日志
    ├── ui/           UI、海图、输入和本地化
    └── audio/        音效、音乐、语音和混音

每个领域目录先有 README，再按 01、02、03……的顺序拆专题。当前领域和玩法目录都已经完成第一版 0→1 设计，具体范围、数量目标、系统依赖和下一步制作入口见 [全领域设计矩阵](common/06_DOMAIN_DESIGN_MATRIX.md)。这表示设计可进入开发拆分，不表示美术资源、代码和批量内容已经全部制作完成。

| 文档 | 定位 | 状态 |
|---|---|---|
| `DESIGN_BOUNDARIES.md` | ★ 权限、系统所有权、Agent 边界和冲突仲裁 | ✅ v3.0 |
| `DATA_MODELS.md` | 核心实体、事件、存档和版本迁移 | ✅ v3.0 |
| D1—D7 历史设计/验收 | `../../archive/milestones/D1-D7/` | 📦 只读，不作为当前入口 |
| `D4_SCOPE.md` | D4 内容扩充范围、数据所有权与验收门 | ✅ D4 已完成 |
| `D4_ACCEPTANCE_REPORT.md` | D4 自动化与运行时最终验收 | ✅ D4 已通过 |
| `D5_SCOPE.md` | D5 打磨上架范围、外部发布边界与验收门 | ✅ D5 已完成 |
| `D5_RELEASE_CHECKLIST.md` | Steam 技术准备与外部发布人工清单 | ✅ 技术项完成 |
| `D5_ACCEPTANCE_REPORT.md` | D5 自动化、运行时与发布桥接验收 | ✅ D5 已通过 |

## 领域专题入口

| 领域 | 入口 | 第一版状态 |
|---|---|---|
| 共享规则 | [common/README.md](common/README.md) | ✅ |
| 船舶 | [ship/README.md](ship/README.md) | ✅ |
| 角色 | [character/README.md](character/README.md) | ✅ |
| 植被 | [vegetation/README.md](vegetation/README.md) | ✅ |
| 动物 | [animals/README.md](animals/README.md) | ✅ |
| 岛屿 | [islands/README.md](islands/README.md) | ✅ |
| 环境 | [environment/README.md](environment/README.md) | ✅ |
| 建筑 | [buildings/README.md](buildings/README.md) | ✅ |
| 道具 | [items/README.md](items/README.md) | ✅ |
| 经济 | [economy/README.md](economy/README.md) | ✅ |
| 势力 | [factions/README.md](factions/README.md) | ✅ |
| 战斗 | [combat/README.md](combat/README.md) | ✅ |
| 任务 | [quests/README.md](quests/README.md) | ✅ |
| UI | [ui/README.md](ui/README.md) | ✅ |
| 音频 | [audio/README.md](audio/README.md) | ✅ |

## 统一设计工作流

1. 先查 `REQUIREMENTS.md` 是否已有需求 ID；没有就登记到创意入口，不能直接进代码；
2. 玩法先写 `gameplay/`，系统规则再写对应领域目录；
3. 写实现前先落 `DATA_MODELS.md`、唯一写者和事件流；
4. 跨端数据先更新 `contracts/`，再同步 game/server/web；
5. 边界冲突按 `DESIGN_BOUNDARIES.md` 仲裁；
6. 完成后必须更新需求状态、路线图、测试入口和记忆日志。

## 当前权威入口

- [玩法总目录](gameplay/README.md)
- [全领域设计矩阵](common/06_DOMAIN_DESIGN_MATRIX.md)
- [跨领域核心流程](common/07_CROSS_DOMAIN_FLOWS.md)
- [D8 数据边界冻结表](common/D8_DATA_BOUNDARY_FREEZE.md)
- [D8 内容 ID 注册表](common/D8_CONTENT_ID_REGISTRY.md)
- [D8 数值与枚举统一配置](common/D8_NUMERIC_ENUM_CONFIG.md)
- [D9 船员 Agent 垂直切片](common/D9_CREW_AGENT_VERTICAL_SLICE.md)
- [D10 船员岗位与行为状态](common/D10_CREW_BEHAVIOR_STATE.md)
- [D18 视觉美术方向与本地开发预算冻结](common/D18_VISUAL_ART_DIRECTION_AND_BUDGET.md)
- [D18 Unity 资产导入与 Prefab 预检管线](common/D18_UNITY_ASSET_IMPORT_PIPELINE.md)
- [D18 免费资源库准入验收（S02 v2）](common/D18_FREE_ASSET_LIBRARY_ADMISSION_ACCEPTANCE.md)
- [D18 PrefabCandidateIntake 准入目录](common/assets/d18_prefab_candidate_intake_catalog.json)
- [数据模型](DATA_MODELS.md)
- [设计边界](DESIGN_BOUNDARIES.md)

开发任务总索引见上级目录的 [AGENT_DEVELOPMENT_TASKS.md](../AGENT_DEVELOPMENT_TASKS.md)，每个 D8—D16 开发章节的可复制提示词位于同目录对应的 `AGENT_DEVELOPMENT_CHAPTER_*.md` 文件。
