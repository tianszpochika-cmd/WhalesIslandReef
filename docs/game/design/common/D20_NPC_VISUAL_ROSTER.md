# D20 NPC Visual Roster

任务：D20-A03
状态：`PARTIAL` / `Candidate`
日期：2026-08-15

## 结论

已在隔离目录交付 12 个可辨识的 NPC 视觉 Prefab 和一份只读接口文件，覆盖船员、商人、码头、渔业、酒馆、船坞、港口守望代理和普通居民岗位。D18-S02-R01 的准入目录为 `PASS`，本批只使用其中 3 个 `npc_visual_core`、`intake_eligible` 人物源模型；没有使用 `Male_Suit.fbx`。

结论为 `PARTIAL` 的原因是：3 个源模型均为 `Generic/NoAvatar`，尚未达到 D20 Humanoid 重定向标准；12 个实例是受控材质、头饰和工具组合，不是 12 个独立人物网格；没有专门的历史守望/官员模型，也没有女性或完成历史审查的 1675 人物基底。本批保持 Candidate，未接入场景、生成器、NPC 行为或存档。

## 可集成入口

| 项目 | 结果 |
| --- | --- |
| 只读接口（D20-A04 唯一入口） | `game/tools/results/D20/D20-A03/D20_A03_NPC_VISUAL_INTERFACE.json` |
| Prefab 目录 | `game/Assets/Prefabs/Characters/D20/NPC/` |
| 模型/材质目录 | `game/Assets/Art/Models/Characters/D20/NPC/` |
| 测试 | `game/Assets/Tests/EditMode/D20/D20A03NpcVisualTests.cs` |
| Prefab 数量 | 12 |
| 独立人物源模型 | 3 |
| 新增材质 | 16（7 布料、3 肤色、2 发色、4 道具材质） |
| Registry/场景/NPC 行为改动 | 0 |

Prefab 根节点保持 1m = 1 Unity unit、Y-up、+Z forward、root scale = 1；`Visual` 子树按源文件边界调整至约 1.8m。根节点有约 1.8m CapsuleCollider、`Animator`（Root Motion 关闭）、LODGroup 和 `Anchors/Head`、`Anchors/Hand_R`、`Anchors/Carry`。Far LOD 是无碰撞的低细节胶囊视觉代理，不能被当作交互实体。

## 12 个样板 NPC

| npc_id | resource key / Prefab | 岗位可读性 | 源模型 | 受控外观组合 | 复用/审查 |
| --- | --- | --- | --- | --- | --- |
| `npc_d15_a07_instance_0001` | `npc_d20_deckhand_01` | deckhand | `crew_quaternius_male_shirt_v01` | ochre 布、暖肤色、草帽、绳圈 | 船员工作服候选；与 fisher/rigger 复用 Shirt |
| `npc_d15_a07_instance_0002` | `npc_d20_rigger_01` | rigger | `crew_quaternius_male_shirt_v01` | sea-green 布、中等肤色、头巾、绳圈 | 船员工作服候选；与 deckhand/fisher 复用 Shirt |
| `npc_d15_a07_instance_0008` | `npc_d20_shore_trader_01` | shore trader | `crew_quaternius_male_long_sleeve_v01` | navy 布、暖肤色、宽檐帽、账本 | 商人轮廓需历史审查；与 trader-02/船坞工复用 LongSleeve |
| `npc_d15_a07_instance_0028` | `npc_d20_shore_trader_02` | shore trader | `crew_quaternius_male_long_sleeve_v01` | burgundy 布、深肤色、宽檐帽、账本 | 同岗位颜色变体；不表达阵营/关系数值 |
| `npc_d15_a07_instance_0006` | `npc_d20_dock_clerk_01` | dock clerk | `crew_quaternius_male_casual_v01` | russet 布、中等肤色、草帽、码头钩 | 码头工具轮廓；与民居/酒馆复用 Casual |
| `npc_d15_a07_instance_0020` | `npc_d20_net_mender_01` | net mender | `crew_quaternius_male_casual_v01` | cream 布、深肤色、头巾、网针 | 渔业工作轮廓；历史服装仍需审查 |
| `npc_d15_a07_instance_0004` | `npc_d20_fisher_01` | fisher | `crew_quaternius_male_shirt_v01` | sea-green 布、深肤色、草帽、鱼竿 | 渔民轮廓；与 deckhand/rigger 复用 Shirt |
| `npc_d15_a07_instance_0010` | `npc_d20_tavern_cook_01` | tavern cook | `crew_quaternius_male_casual_v01` | burgundy 布、暖肤色、头巾、托盘 | 酒馆岗位候选；工具只用于视觉，不写库存/经济 |
| `npc_d15_a07_instance_0005` | `npc_d20_boatbuilder_01` | boatbuilder | `crew_quaternius_male_long_sleeve_v01` | ochre 布、中等肤色、头巾、木槌 | 船坞工候选；工具只用于视觉 |
| `npc_d15_a07_instance_0014` | `npc_d20_sailmaker_01` | sailmaker | `crew_quaternius_male_long_sleeve_v01` | cream 布、深肤色、草帽、帆针 | 船坞工候选；与 boatbuilder 复用 LongSleeve |
| `npc_d15_a07_instance_0018` | `npc_d20_harbor_watch_proxy` | harbor-watch/official proxy | `crew_quaternius_male_long_sleeve_v01` | indigo 布、深肤色、宽檐帽、手杖 | 没有专门历史官员源；必须人工审查，不能据此裁决身份/法律 |
| `npc_d15_a07_instance_0009` | `npc_d20_common_resident_01` | common resident | `crew_quaternius_male_casual_v01` | russet 布、暖肤色、无帽、篮子 | 居民候选；与 dock clerk/net mender/cook 复用 Casual |

这些 `npc_id` 是现有 D15 内容实例键；本批没有增加或修改 Registry。颜色、头饰和道具只承担远近距离的视觉识别，不代表 faction、religion、class、relation 或任何强逻辑状态。

## 动作、LOD 与碰撞边界

D18 人物源实际带有 `Man_Idle`、`Man_Walk`、`Man_Run`、`Man_Jump`、`Man_Death` 等基础片段；未宣称存在命名为 Interact 的正式动作。交互缺口使用稳定站姿/程序侧后续 fallback，视觉包不触发任何 NPC 事件。Prefab 的 Animator 不带运行时 Controller；D20-A04 需要在行为系统边界内决定是否绑定动作，不得回写本目录。

建议 LOD 距离：0–12m 使用源渲染器和岗位模块，12–32m 使用同一渲染器的中距级别，32–80m 使用低细节 FarProxy，80m 以上裁剪。Animator 使用 `CullUpdateTransforms`；LOD/Animator 只影响表现，不写生成、岗位、对话、关系或保存数据。

## 时代与审查

- `Male_Suit.fbx` 按 D18 审查结论排除，未复制、改名或伪装成 1675 服装。
- 工作服、头巾、草帽、宽檐帽、账本、网针、木槌和手杖均标为视觉候选，不宣称历史定稿。
- `npc_d20_harbor_watch_proxy` 是缺口代理；其服装和权力身份必须由人工历史审查后才可进入正式阵容。
- 没有女性基底和完成审查的 1675 人物基底；不得以颜色变体填充该缺口。
- 资源状态保持 `Candidate`；本文件和接口文件均不授予 `LocalDevelopmentReady` 或 `ReleaseApproved`。

## 验收记录

Unity 6000.0.80f1 构建器报告确认 12 个 Prefab、3 个独立源模型、16 个材质、每个 Prefab 1 个根碰撞、1 个 LODGroup、1 个 Animator、3 个交互锚点、非零 bounds 和完整材质引用，报告 errors 为空。主工程 EditMode 验收应以 `D20_A03_UNITY_TEST_RESULTS.xml` 为准；测试覆盖稳定资源 key、Visual/碰撞/LOD/Animator/锚点、材质 shader、非零 bounds 和接口边界。

机器报告：`game/tools/results/D20/D20-A03/D20_A03_PREFAB_BUILD_REPORT.json`。联系表：`game/tools/results/D20/D20-A03/D20_A03_NPC_CONTACT_SHEET.md`。D20-A04 只能读取 `D20_A03_NPC_VISUAL_INTERFACE.json`，不得改写本任务目录。
