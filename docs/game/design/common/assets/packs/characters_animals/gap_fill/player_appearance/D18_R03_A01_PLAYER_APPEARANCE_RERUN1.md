# D18-R03-A01 RERUN1 玩家外观资源完成证据

任务：`D18-R03-A01`  
本轮结论：**PASS（隔离资源完成 / 工作台证据）**  
核对日期：2026-08-16  
Unity：6000.0.80f1

## 真实完成项

- 2 个独立全身方向：Quaternius 女性与男性全身基体；源 FBX 的文件 SHA、CC0 证据和隔离导入结果沿用原 D18-R03-A01 packet。每个基体另生成一个隔离项目自有 `Avatar` 资产，Unity 审计确认 `isValid=true`、`isHuman=true`。
- 6 个独立发型几何：原始审计的 5 个发型网格，加上项目自有 `Hair_TwistPonytail` 网格模块。项目自有模块不是外部来源声明，也未写入全局资源状态。
- 8 个真实装配变体：女性/男性 Peasant 与 Ranger，各自包含无配件、兜帽或肩甲的实际 FBX 几何组合；每个变体通过 Unity Prefab 导入审计，具有 Renderer、Mesh、Material，无运行时 MonoBehaviour。

机器证据：

- `game/tools/results/D18/D18-R03-A01/D18_R03_A01_MACHINE_RESULT_RERUN1.json`
- `game/tools/results/D18/D18-R03-A01/D18_R03_A01_EDITOR_AUDIT_RERUN1.json`
- `game/tools/results/D18/D18-R03-A01/D18_R03_A01_BUILD_RERUN1D.log`
- `docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/D18_R03_A01_PLAYER_APPEARANCE_PACKET_RERUN1.json`

## 状态边界

本轮没有修改候选 FBX 的真实导入状态、全局 `d18_prefab_candidate_intake_catalog.json`、ResourceKey、正式生成器、InitialHarbor 或运行时消费者。以下数量必须保持为零：

- `LocalPlayableReady=0`
- `LocalDevelopmentReady=0`
- `ReleaseApproved=0`
- 正式场景消费者 `0`
- 运行时消费者 `0`

因此本轮 PASS 只表示“D18 隔离资源和工作台覆盖完成”，不表示角色已经可以在正式 Player 中使用，也不改变 D18-S03 的运行时晋级职责。

## 真实限制

- 服装来源的历史适配仍为 `PROVISIONAL_LOCAL_VISUAL`，不能把 Fantasy/Medieval 包名当作 1675 年加勒比历史结论。
- 头发仍需由 D20-A01-R02 绑定到真实头部骨骼/Socket 并检查动画跟随、裁切、LOD 和保存恢复。
- 生成的 Avatar 和装配 Prefab 位于 D18 R03 隔离目录，D20 只能在其授权工作目录中选择性复用，不能绕过 D18-S03 写入正式场景。
