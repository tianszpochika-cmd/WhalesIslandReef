# 项目文档索引

本目录采用“需求 → 设计 → 开发计划 → 历史证据”的单向结构。新任务只从当前权威入口派生，归档区只读。

## 当前权威入口

- [统一需求总纲 v3.0](game/REQUIREMENTS.md)：所有系统、玩法、数量和完成定义；
- [统一设计总览](game/design/README.md)：领域结构、数据、边界和设计流程；
- [玩法设计总目录](game/design/gameplay/README.md)：经营、捕捞、动物、牌局、关系、装扮、探索和奖励；
- [D7 后统一开发路线](game/DEV_ROADMAP.md)：D8—D16 历史执行线、D17 保留基线和 D18—D24 本地开发轨；
- [D18—D24 画面与完整本地可玩路线](game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md)：免费资源、样板岛、角色、船海、UI、完整闭环和扩量原则；
- [D18 免费资源库准入验收（S02 v2）](game/design/common/D18_FREE_ASSET_LIBRARY_ADMISSION_ACCEPTANCE.md)：资源证据准入、受控开发解锁与 D18-S03 运行时晋级门；
- [Agent 开发任务总索引](game/AGENT_DEVELOPMENT_TASKS.md)：列出全部章节、编号和执行顺序；实际提示词位于 D8—D16、D18—D24 独立章节文件；
- [技术架构](ARCHITECTURE.md)：三端、契约和目录规则；
- [开发规范](DEVELOPMENT_STANDARDS.md)：工程边界和验收要求；
- [D8 数值与枚举统一配置](game/design/common/D8_NUMERIC_ENUM_CONFIG.md)：统一单位、范围、枚举、所有权和配置检查入口；
- [D9 船员 Agent 垂直切片](game/design/common/D9_CREW_AGENT_VERTICAL_SLICE.md)：核心船员 Persona、记忆、提议、关系因果裁决和离线模板；
- [D10 船员岗位与行为状态](game/design/common/D10_CREW_BEHAVIOR_STATE.md)：轮班、岗位效率、疲劳健康、拒绝/怠工、请假调岗、离队和叛变前兆；
- [D10 同步集成验收报告](game/design/common/D10_ACCEPTANCE_REPORT.md)：工资、救援、偏袒、关系因果、行为、港口身份外观与读档一致性验收；
- [D11 钓鱼与海上采集](game/design/common/D11_FISHING_HARVESTING.md)：鱼种、深度、时间、潮汐、渔具、浅水贝类/海草、处理品、市场反馈与幂等读档；
- [D12 海图、测深与发现系统](game/design/common/D12_MAP_DISCOVERY.md)：海图分层、未知海岸、暗礁/浅滩/鱼场、工具、可信度、证据和存档边界；
- [D12 线索与寻宝任务](game/design/common/D12_TREASURE_CLUES.md)：多来源线索、可靠度、交叉验证、时间窗、装备前置、替代路线和奖励追溯；
- [D12 沉船、洞穴与打捞系统](game/design/common/D12_WRECK_CAVE_SALVAGE.md)：沉船/洞穴位置、潮汐、装备、风险、竞争者、法律裁决、失败恢复和奖励请求幂等；
- [D13 产权与资产交易](game/design/common/D13_PROPERTY_ASSET_MARKET.md)：岛屿、地块、房屋、仓库、工坊、酒馆、船坞、农地与渔场的调查、估值、权利交易、共同治理、抵押、继承和争议；
- [D15 植被批量目录](game/design/common/D15_VEGETATION_BATCH_CATALOG.md)：1,000—2,000 植物条目的分类、模型/材质占位状态、LOD、风场、生境、用途、来源/授权和抽样加载门禁；
- [D19 港口建筑样板](game/design/common/D19_PORT_ARCHITECTURE.md)：圣玛格丽特港口模块化 Prefab、稳定交互锚点、碰撞、LOD、材质复用和工作场景验证；
- [D19 圣玛格丽特正式场景验收](game/design/common/D19_SAINT_MARGARET_ACCEPTANCE.md)：InitialHarbor 串行集成、稳定交互 ID、兼容节点、固定镜头、PlayMode 回归、性能边界和回滚证据；
- [D13 金融、信用与保险](game/design/common/D13_FINANCE_CREDIT_INSURANCE.md)：现金、赊账、贷款、担保、利息、展期、保险证据、拒赔、违约、催收、查封与破产保护；
- [D13 农业、果园与蔬菜](game/design/common/D13_FARMING_ORCHARDS.md)：地块、种子、土壤、水源、季节、播种、病虫害、风暴、收获、轮作、加工与销售；
- [环境搭建](game/SETUP.md)：本机运行与测试；
- [契约目录](../contracts/)。

## 已完成基线

D1—D7 的范围、提示词和验收文件已经移入 [历史归档](archive/README.md)。它们用于追溯已完成事实，不再作为未来需求或开发入口。

## 设计领域

- [船舶](game/design/ship/README.md)
- [玩法](game/design/gameplay/README.md)
- [角色](game/design/character/README.md)
- [植被](game/design/vegetation/README.md)
- [动物](game/design/animals/README.md)
- [岛屿](game/design/islands/README.md)
- [环境](game/design/environment/README.md)
- [建筑](game/design/buildings/README.md)
- [道具](game/design/items/README.md)
- [经济与金融](game/design/economy/README.md)
- [势力、宗教与法律](game/design/factions/README.md)
- [战斗](game/design/combat/README.md)
- [任务与事件](game/design/quests/README.md)
- [UI](game/design/ui/README.md)
- [音频](game/design/audio/README.md)

## 归档原则

归档文件不可作为新需求的事实源。若归档内容与当前文档冲突，以 `REQUIREMENTS.md`、`design/` 和 `DEV_ROADMAP.md` 为准。
