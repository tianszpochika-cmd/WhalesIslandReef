# D18-A04-R02-B Bird Gap-Fill Packet

结论：**PARTIAL**

本任务只交付可审计的隔离候选，不执行 D18-S02-R01 准入，不建立正式动物 Prefab，不接入 D20 刷新或运行时逻辑。

## 现有鸟类缺口

现有 `OpenGameArtBird/Source/bird.blend` 只有 Blender 源文件，既有 manifest 标为 `NotImported`，Unity 模型/对象数为 0，也没有 Unity 网格、Renderer、bounds 或动作导入证据。因此不能被计为 D20-A05 的近景或远景鸟模型。

## 交付包与真实数量

| 项目 | 真实结果 |
|---|---|
| 来源包 | Quaternius Animal Pack Vol.2 |
| 许可证 | CC0-1.0；官方页、FAQ、下载页和法律文本见 manifest/许可证证据 |
| 可计入候选 | 1 个：`eagle.fbx` |
| 实际 Unity 导入 | 1 个 FBX |
| 网格 | 1 |
| 材质槽 | 4 |
| 外部纹理文件 | 0（材质随模型导入） |
| Renderer | 1 个 `SkinnedMeshRenderer` |
| 动作片段 | 3：`EagleArmature|Flying`、`EagleArmature|Idle`、`Eagle|Idle` |
| Avatar | 0；Importer 为 `Generic/NoAvatar` |
| Animator 组件 | 0；源 FBX 未带正式 Animator |
| 骨架证据 | D18 审计报告 33 个导入 Transform；detail dump 报告 24 个蒙皮骨骼 |
| bounds | 有效；最大维度约 `0.1246103793` Unity 单位，未修改源文件 |
| Collider / LODGroup / Prefab | 均为 0；本任务不创建 |

Chicken 和 Rooster 的 CC0 页面与下载归档已逐项核验，但实际归档只有 `.blend`、license 和纹理，没有 FBX/GLB/OBJ 可供 Unity 预检，均以 `REJECTED_SOURCE_ONLY` 记录，不复制到候选目录，不计入模型数。

## D20-A05 适配边界

| 使用距离 | 判断 | 需要的后续工作 |
|---|---|---|
| 港口/近海中近景 | `PARTIAL / NeedsModification` | D20 正式 Prefab 提供 Animator/Controller 策略、单位比例、碰撞、LOD 和必要的落地/栖停逻辑；当前不得直接接入 |
| 远景/中远景 | `Candidate` | 可作为飞行剪影/远景鸟基体；仍需 D20 负责 LOD、远景 fallback、批次预算和场景放置 |
| 贴脸近景 | `BLOCKED` | 缺 Avatar/Animator、Walk/Perch/Takeoff/Land 片段，且源 bounds 很小；必须先完成 D20 适配审查 |

已有动作只覆盖 `Flying` 与 `Idle`。没有 Walk、Takeoff、Land、Perch、Feeding 或 Landing 片段。允许的 fallback 需求仅作登记，不在本任务实现：

- 飞行：使用已有 `Flying`，远景可降级为低频循环或 billboard；
- 栖停/站立：使用 `Idle`，不得假定它等价于 Perch；
- 起飞/降落/进食：由 D20 另行提供状态入口或明确无动作降级；
- LOD：至少需要一个简化网格/远景 billboard 策略，当前没有 LODGroup。

## 导入与命名问题

Unity 6000.0.80f1、URP、只读审计：`PASS`，0 错误、1 警告。警告为官方原始 `eagle.fbx` 不符合 D18 稳定蛇形命名规则；为保持可追溯性未改名。`Generic/NoAvatar`、无 Animator、无 LOD、无 Collider 是候选适配缺口，不被隐藏。

机器证据：

- Manifest：`docs/game/design/common/assets/packs/characters_animals/gap_fill/bird/quaternius_animal_pack_vol2_bird_d18_a04_r02_b.manifest.json`
- Unity 导入审计：`game/tools/results/D18/D18-A04-R02-B/D18_A04_R02_B_UNITY_IMPORT_AUDIT.json`
- 鸟类 detail dump：`game/tools/results/D18/D18-A04-R02-B/D18_A04_R02_B_BIRD_DETAIL.json`
- 来源许可证证据：`game/Assets/Art/Candidates/CharactersAnimals/GapFill/Bird/LICENSE_EVIDENCE_QUATERNIUS_CC0.md`

## 隔离和准入边界

候选只位于 `game/Assets/Art/Candidates/CharactersAnimals/GapFill/Bird/`。本任务未修改全局 catalog、准入目录、D20/D11 逻辑、Prefab、场景、Systems、存档、contracts、server、web 或 Git 历史。`runtime_consumers` 保持为空。只有 D18-S02-R01 可以根据本包证据决定是否转为准入状态。
