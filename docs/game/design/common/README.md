# 共享设计

本目录存放跨领域都要遵守的设计规则，不存放某一个玩法的专属数值。

规划文档：

- 设计支柱与冲突仲裁；
- 数据所有权和系统边界；
- 统一 ID、标签、版本和存档规则；
- 共享数值、时间、天气、事件和状态枚举；
- 资产命名、来源、授权和验收规则；
- 全领域 0→1 交付矩阵；
- 航行、港口、战斗、生态和存档的跨领域流程。

专题入口：

- [01_SHARED_RULES.md](01_SHARED_RULES.md)
- [02_DATA_GOVERNANCE.md](02_DATA_GOVERNANCE.md)
- [03_CONTENT_ID_CATALOG.md](03_CONTENT_ID_CATALOG.md)
- [04_TIME_WEATHER_STATE.md](04_TIME_WEATHER_STATE.md)
- [05_DESIGN_QA.md](05_DESIGN_QA.md)
- [06_DOMAIN_DESIGN_MATRIX.md](06_DOMAIN_DESIGN_MATRIX.md)
- [07_CROSS_DOMAIN_FLOWS.md](07_CROSS_DOMAIN_FLOWS.md)
- [D8_DATA_BOUNDARY_FREEZE.md](D8_DATA_BOUNDARY_FREEZE.md)：D8-S01 核心实体、D7 字段、v3 字段、写者、读取者、存档、版本和跨端边界冻结表
- [D8_ENTITY_DTO_CATALOG.md](D8_ENTITY_DTO_CATALOG.md)：D8-A01 v3 纯 C# 实体、稳定 ID、单位类型、D7 兼容和序列化策略
- [D8_SAVE_MIGRATION.md](D8_SAVE_MIGRATION.md)：D8-A05 D7 SQLite 到 save_version=3 的字段迁移、默认值、回滚和幂等规则
- [D8_CONTENT_ID_REGISTRY.md](D8_CONTENT_ID_REGISTRY.md)：D8-A02 内容 ID 前缀、来源/授权字段、引用图、代表样例和扫描器
- [D8_NUMERIC_ENUM_CONFIG.md](D8_NUMERIC_ENUM_CONFIG.md)：D8-A06 数值单位、默认值、边界、拥有者、来源、测试入口和 canonical 枚举
- [D9_WEATHER_SEA_STATE.md](D9_WEATHER_SEA_STATE.md)：D9-A05 三天气、海况影响、唯一写者、存档和事件幂等边界
- [D9_CREW_AGENT_VERTICAL_SLICE.md](D9_CREW_AGENT_VERTICAL_SLICE.md)：D9-A04 核心船员 Persona、记忆、提议、关系因果裁决与离线模板
- [D10_CREW_BEHAVIOR_STATE.md](D10_CREW_BEHAVIOR_STATE.md)：D10-A02 船员岗位、轮班、疲劳健康、行为状态、离队前兆与存档
- [D9_FISHING_VERTICAL_SLICE.md](D9_FISHING_VERTICAL_SLICE.md)：D9-A02 近海鱼群、手线、鱼获库存、处理、出售/烹饪、存档和唯一写者边界
- [D10_ROMANCE_CONFLICT_EVENTS.md](D10_ROMANCE_CONFLICT_EVENTS.md)：D10-A03 关系阶段、恋爱同意、冲突事件、因果日志和存档边界
- [D10_PLAYER_IDENTITY_EXPRESSION.md](D10_PLAYER_IDENTITY_EXPRESSION.md)：D10-A04 玩家身份表达、港口修改、证件、历史保留和存档边界
- [D11_ECOLOGY_STATE_MACHINE.md](D11_ECOLOGY_STATE_MACHINE.md)：D11-A01 生境、植物、动物、鱼群、天气海况、压力、污染、恢复和生态存档边界
- [D11_RARE_ANIMAL_COLLECTION.md](D11_RARE_ANIMAL_COLLECTION.md)：D11-A05 珍稀动物发现、图鉴、救助、合法转移、保护区、收藏和非法交易边界
- [D12_MAP_DISCOVERY.md](D12_MAP_DISCOVERY.md)：D12-A01 海图分层、发现来源、可信度、测深/望远镜和存档边界
- [D12_WRECK_CAVE_SALVAGE.md](D12_WRECK_CAVE_SALVAGE.md)：D12-A02 沉船/洞穴位置、潮汐、装备、风险、竞争者、法律裁决、失败恢复和奖励请求
- [D12_TAROT_DIVINATION.md](D12_TAROT_DIVINATION.md)：D12-A06 非超自然牌面、占卜信任、解释偏差、软线索、求证和错误后果

设计宪法 `../DESIGN_PILLARS.md`、权限边界 `../DESIGN_BOUNDARIES.md` 和数据模型 `../DATA_MODELS.md` 是根级权威文件；旧的数值、战斗、成长和存档入口已经归档，避免多个文件同时声称自己是事实源。
