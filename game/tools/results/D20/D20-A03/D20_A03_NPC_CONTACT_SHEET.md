# D20-A03 NPC contact sheet

状态：`Candidate`；不代表场景接入或运行时身份裁决。所有条目都映射到现有 D15 `npc_id`，可用于 D20-A04 只读选择。

| # | npc_id | resource key | 岗位 | 身体/服装识别 | 头饰 | 工具/道具 | 源模型 | 历史审查 |
|---:|---|---|---|---|---|---|---|---|
| 1 | `npc_d15_a07_instance_0001` | `npc_d20_deckhand_01` | deckhand | ochre / warm | straw_hat | rope_coil | Male_Shirt | candidate |
| 2 | `npc_d15_a07_instance_0002` | `npc_d20_rigger_01` | rigger | sea_green / medium | headwrap | rope_coil | Male_Shirt | candidate |
| 3 | `npc_d15_a07_instance_0008` | `npc_d20_shore_trader_01` | shore trader | navy / warm | broad_hat | ledger | Male_LongSleeve | required |
| 4 | `npc_d15_a07_instance_0028` | `npc_d20_shore_trader_02` | shore trader | burgundy / deep | broad_hat | ledger | Male_LongSleeve | required |
| 5 | `npc_d15_a07_instance_0006` | `npc_d20_dock_clerk_01` | dock clerk | russet / medium | straw_hat | dock_hook | Male_Casual | candidate |
| 6 | `npc_d15_a07_instance_0020` | `npc_d20_net_mender_01` | net mender | cream / deep | headwrap | net_needle | Male_Casual | candidate |
| 7 | `npc_d15_a07_instance_0004` | `npc_d20_fisher_01` | fisher | sea_green / deep | straw_hat | fishing_pole | Male_Shirt | candidate |
| 8 | `npc_d15_a07_instance_0010` | `npc_d20_tavern_cook_01` | tavern cook | burgundy / warm | headwrap | serving_tray | Male_Casual | candidate |
| 9 | `npc_d15_a07_instance_0005` | `npc_d20_boatbuilder_01` | boatbuilder | ochre / medium | headwrap | mallet | Male_LongSleeve | candidate |
| 10 | `npc_d15_a07_instance_0014` | `npc_d20_sailmaker_01` | sailmaker | cream / deep | straw_hat | sail_needle | Male_LongSleeve | candidate |
| 11 | `npc_d15_a07_instance_0018` | `npc_d20_harbor_watch_proxy` | harbor watch proxy | indigo / deep | broad_hat | staff | Male_LongSleeve | human review |
| 12 | `npc_d15_a07_instance_0009` | `npc_d20_common_resident_01` | common resident | russet / warm | none | basket | Male_Casual | candidate |

复用统计：`Male_Casual` 4 个、`Male_LongSleeve` 5 个、`Male_Shirt` 3 个；独立人物源模型 3 个。岗位识别来自服装色块、头饰和道具轮廓的组合，而不是头顶文字或逻辑标签。所有工具均为表现层模块，不负责交易、库存、关系、对话或岗位裁决。

远距方案：近距使用完整人物与岗位模块，中距由 LODGroup 保持可读轮廓，远距使用无碰撞 capsule `FarProxy`，80m 以上裁剪；Animator 采用 `CullUpdateTransforms`。基础源动作只确认 Idle/Walk/Run/Jump/Death；Interact 尚未提供，后续只能使用稳定姿态 fallback。

对应机器证据：`D20_A03_PREFAB_BUILD_REPORT.json`；只读接口：`D20_A03_NPC_VISUAL_INTERFACE.json`。本联系表没有宣称 Humanoid Avatar、正式历史服装、场景接入或 `LocalDevelopmentReady`。
