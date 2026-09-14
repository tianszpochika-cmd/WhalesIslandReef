# IncomingAnimations —— 正式人形动作投放目录

> 用途：把外部下载的人形（Humanoid）动画 FBX 放进本目录，交给 Q03-A19 做重定向与准入。
> 创建：2026-09-14（Q03-A18 诊断子卡）
> 状态：`ASSET_ACQUIRED / VERIFIED` —— 素材已由 agent 自行获取并验证，无需人工下载。

## ✅ 更新（2026-09-14）：素材已到位

原本需要人工下载的清单**已由 agent 自主完成**：

- 已获取 **Universal Animation Library（Quaternius，CC0 1.0）** → `UAL1_Standard.fbx`（23.75 MB）
- 已切换导入为 `Human`，生成 `UAL1_StandardAvatar`（`isValid=True`、`isHuman=True`、52 根人形骨骼）
- 共 **45 个可用骨骼动画**，**含 `Swim_Fwd_Loop` 与 `Swim_Idle_Loop`**（`humanMotion=True`）
- 骨架命名与项目玩家角色**逐字一致**，重定向风险极低

详见同目录 `RESOURCE_PROVENANCE.md`（含哈希、许可证、验证数据）。

**下方原始清单仅作历史记录**，现在不再需要人工下载。若日后需要补充 UAL 未覆盖的动作（如航海专属握舵姿态），才会用到上面的 Mixamo 流程。

## 为什么需要这个目录

Q03-A18 只读诊断确认：当前玩家角色本体 `Superhero_Female_FullBody.fbx` **内嵌 0 个动画**，现有 15 个 D20 clip 全是根节点变换曲线（`/localPosition.y`、`/localEulerAnglesRaw.*`），不含骨骼动作。

因此需要外部合法 Humanoid 动作源，重定向到项目已有的有效 Avatar 上。

## 已就绪、不需要重做的部分

| 资产 | 状态 |
|---|---|
| `Assets/Art/Models/Characters/D20/Player/D20_PlayerHumanoidAvatar_R01.asset` | `isValid=True`、人形骨骼 20、骨架 70 |
| `.../ProjectOwned/R03/Avatars/D18_R03_A01_PlayerHumanoidMale.asset` | `isValid=True`、20 / 70 |
| `.../ProjectOwned/R03/Avatars/D18_R03_A01_PlayerHumanoidFemale.asset` | `isValid=True`、20 / 70 |
| 角色骨架 `Armature/root/pelvis/spine_01/.../thigh_l/calf_l/foot_l` | 命名规范，可标准重定向 |

**结论：不用换角色模型，不用重做骨架映射，只缺动作。**

## 下载硬性要求（不满足则无法重定向）

1. **必须是人形（Humanoid）骨架**的 FBX；
2. **同一个来源角色的动作要成组下载** —— Mixamo 要求所有动作基于同一个角色，骨骼映射才一致；
3. 导出格式 **FBX Binary (.fbx)**；
4. **Skin = Without Skin**（只要动作，不要网格，避免重复 200MB 模型）；
5. **帧率 30 fps**；
6. **Keyframe Reduction = none**（保留精度）。

## 需要的动作清单

### P0 —— 决定能否关闭「动作扭曲 / 不能游泳」

| 文件名（建议） | Mixamo 搜索词 | 对应现有 clip |
|---|---|---|
| `Anim_Swim.fbx` | `Swimming` | （新增，当前完全缺失） |
| `Anim_Idle.fbx` | `Idle` | `D20_Player_Idle` |
| `Anim_Walk.fbx` | `Walking` | `D20_Player_Walk` |
| `Anim_Run.fbx` | `Running` | `D20_Player_Run` |
| `Anim_Jump.fbx` | `Jumping` | `D20_Player_Jump` |
| `Anim_Fall.fbx` | `Falling Idle` | `D20_Player_Fall` |
| `Anim_Land.fbx` | `Landing` | `D20_Player_Land` |

### P1 —— 上下船与操舵

| 文件名（建议） | Mixamo 搜索词 | 对应现有 clip |
|---|---|---|
| `Anim_Board.fbx` | `Climbing` 或 `Climbing To Top` | `D20_Player_Board` |
| `Anim_Disembark.fbx` | `Climbing Down` | `D20_Player_Disembark` |
| `Anim_Helm.fbx` | `Standing` 或 `Idle`（后续按 D7 舵位适配） | `D20_Player_Helm` |
| `Anim_Interact.fbx` | `Picking Up` 或 `Use Item` | `D20_Player_Interact` |
| `Anim_Talk.fbx` | `Talking` | `D20_Player_Talk` |
| `Anim_Carry.fbx` | `Carrying` 或 `Box Carry` | `D20_Player_Carry` |

### P2 —— 次要

| 文件名（建议） | Mixamo 搜索词 | 对应现有 clip |
|---|---|---|
| `Anim_Hit.fbx` | `Hit Reaction` | `D20_Player_Hit` |
| `Anim_Recover.fbx` | `Stumble Backwards` 或 `Getting Up` | `D20_Player_Recover` |

## 备选方案（不想用 Mixamo 时）

项目内已有 **CC0** 候选人形动作库：
`game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedMen/`
（授权见同目录 `License.txt`，CC0 1.0 Universal）

每个模型内嵌 11 个骨骼动画：
`Man_Idle, Man_Walk, Man_Run, Man_Jump, Man_RunningJump, Man_Standing, Man_Sitting, Man_Clapping, Man_Punch, Man_SwordSlash, Man_Death`

- 优点：已在工程内、CC0、零下载；
- 缺点：**没有 Swim**，且当前导入为 `Generic / NoAvatar`，需要先改成 Humanoid；
- 若走这条路，只需额外补一个游泳动作。

## 投放之后

把文件放进本目录后**告诉我**，我将执行 Q03-A19：

1. 校验每个 FBX 的 `animationType` / Avatar 有效性；
2. 重定向到 `D20_PlayerHumanoidAvatar_R01`；
3. 重建 15 个 `D20_Player_*.anim` 为 Humanoid 肌肉空间；
4. 在 `D20_PlayerAnimation.controller` 补上**缺失的 Swimming 状态与过渡**（当前 15 个状态、`transition_count=0`、`hasSwimmingState=false`）；
5. 跑 `Q03FormalActionAdmissionGateTests`，目标由 `PASS_AS_BLOCKED_GATE` 转为真实准入通过（`humanMotion > 0`）；
6. 回归 Q08 动作整合检查点，再回 Q31。

## 边界

本目录是**投放暂存区**，不是准入结果。放入文件不等于资源已准入；
正式准入仍需过 D18 catalog / `LocalDevelopmentReady` 门禁并留许可证证据。
