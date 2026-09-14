# 资源来源与许可证证据 —— Universal Animation Library (UAL1 Standard)

> 引入日期：2026-09-14
> 执行者：Luna（AI agent）
> 卡片：Q03-A18（正式动作资源根因诊断）/ 为 Q03-A19 准备输入
> 状态：`CANDIDATE_PENDING_REVIEW` —— **已下载并验证可用，尚未正式准入**

## 1. 资源身份

| 项目 | 内容 |
|---|---|
| 名称 | Universal Animation Library（Standard 免费版） |
| 原作者 | Quaternius |
| 官方页面 | https://quaternius.com/packs/universalanimationlibrary.html |
| 官方发布 | https://quaternius.itch.io/universal-animation-library |
| 本次获取渠道 | GitHub 镜像 `IAFahim/quaternius.universalAnimationLibrary.standard` |
| 镜像原始文件 | `UAL1_Standard.fbx` |
| 许可证 | **CC0 1.0 Universal**（公共领域奉献，商用无限制，无需署名） |
| 许可证文本 | 同目录 `LICENSE_CC0.txt` |
| 发布日期 | 2025-03（页面标注 March 2025）；镜像收录日期见其 commit |

## 2. 文件哈希（可追溯）

| 文件 | 大小 | SHA256 |
|---|---|---|
| `UAL1_Standard.fbx` | 23.75 MB | `05B7912F6926EB023B3BF5133012DCA1EE27E41F453909857E1D471E9EACEF12` |

下载命令（可复现）：

```powershell
Invoke-WebRequest `
  -Uri "https://raw.githubusercontent.com/IAFahim/quaternius.universalAnimationLibrary.standard/main/UAL1_Standard.fbx" `
  -OutFile "game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusUniversalBaseCharacters/IncomingAnimations/UAL1_Standard.fbx"
```

## 3. 为什么选这个资源（CC0-A18 根因直接对应）

Q03-A18 诊断确认：玩家角色本体 `Superhero_Female_FullBody.fbx` **内嵌 0 个动画**；现有 15 个 `D20_Player_*.anim` 是根节点变换曲线（`humanMotion=False`），不驱动骨骼。

本资源**骨架命名与项目玩家角色逐字一致**：

```
玩家 Superhero_Female_FullBody.fbx:
  Armature / root / pelvis / spine_01 / spine_02 / spine_03 /
  clavicle_l / upperarm_l / lowerarm_l / hand_l / index_01_l ...

UAL1_Standard.fbx:
  Armature / root / pelvis / spine_01 / spine_02 / spine_03 /
  clavicle_l / upperarm_l / lowerarm_l / hand_l / index_01_l ...
```

两者同属 Quaternius 万能人形骨架体系，因此重定向风险极低。

## 4. 导入配置（本次已应用）

| 设置项 | 原值 | 现值 |
|---|---|---|
| `animationType` | `Generic` | **`Human`** |
| `avatarSetup` | `NoAvatar` | **`CreateFromThisModel`** |

应用方式：Unity Pipeline `set_import_settings`，`applied=["animationType","avatarSetup"]`，`unknown=[]`。

## 5. 验证结果（Unity Pipeline 只读实测）

| 检查项 | 结果 |
|---|---|
| 导入类型 | `animationType=Human` |
| 生成 Avatar | `UAL1_StandardAvatar`，`isValid=True`，`isHuman=True`，**52 根人形骨骼** |
| 非预览 clip 总数 | **45** |
| **`Armature\|Swim_Fwd_Loop`** | **`humanMotion=True`**，时长 `1.67s` ✅ |
| **`Armature\|Swim_Idle_Loop`** | **`humanMotion=True`**，时长 `4.17s` ✅ |

`humanMotion=True` 是本卡的核心验收点：证明这些 clip 是**骨骼肌肉空间动画**，而非旧的根节点变换曲线，可以直接被 Humanoid Animator 消费。

## 6. 可用动作清单（45 个，按用途归类）

**移动 / 运动（关闭「动作扭曲」）**
`Idle_Loop`, `Idle_Talking_Loop`, `Idle_Torch_Loop`, `Walk_Loop`, `Walk_Formal_Loop`, `Jog_Fwd_Loop`, `Sprint_Loop`, `Crouch_Idle_Loop`, `Crouch_Fwd_Loop`, `Jump_Start`, `Jump_Loop`, `Jump_Land`, `Roll`, `Roll_RM`, `Push_Loop`

**游泳（关闭「海洋不能游泳」）**
`Swim_Idle_Loop`, `Swim_Fwd_Loop`

**航海 / 交互（上下船、操舵、贸易）**
`Interact`, `PickUp_Table`, `Driving_Loop`, `Fixing_Kneeling`, `Sitting_Enter`, `Sitting_Idle_Loop`, `Sitting_Exit`, `Sitting_Talking_Loop`

**受击 / 战斗（简化格斗）**
`Hit_Chest`, `Hit_Head`, `Punch_Jab`, `Punch_Cross`, `Sword_Idle`, `Sword_Attack`, `Sword_Attack_RM`, `Death01`

**其他（备用）**
`A_TPose`, `Dance_Loop`, `Spell_Simple_Enter/Exit/Idle_Loop/Shoot`, `Pistol_*`（本作 1675 年无枪械，**不接入**）

> 注：`Pistol_*` 与 `Spell_*` 共 11 个 clip 不符合 1675 年加勒比海写实设定，**明确不接入**，仅作为资源包完整性记录。

## 7. 已知边界与风险

1. **骨骼数量差异**：本资源 Avatar 有 **52** 根人形骨骼，项目玩家 Avatar（`D20_PlayerHumanoidAvatar_R01`）只有 **20** 根。缺失的手指/脚趾等末端骨骼会在重定向时被丢弃或近似。躯干与四肢主干映射完整（Hips/Spine×3/Neck/Head/双臂/双腿全部具备），因此不影响本次要解决的动作质量目标。
2. **尚未做端到端重定向实测**：本卡只验证到"资源可被识别为 Humanoid 且 clip 含肌肉数据"。真正把 clip 挂到玩家 Avatar 上播放并观察效果，属于 Q03-A19 的范围。
3. **未准入**：本资源当前位于 `Art/Candidates/...` 候选区，状态 `CANDIDATE_PENDING_REVIEW`。**没有**写入 D18 catalog、**没有**修改 `LocalDevelopmentReady`、**没有**替换默认 Player 的任何动作。
4. **镜像依赖**：本次通过第三方 GitHub 镜像获取（官方 itch.io 的免费下载需要游戏级 download key，匿名 HTTP 流程受阻）。许可证文本为官方 CC0 原文，但因来源是镜像，**建议正式准入前由人工确认一次官方页面**。

## 8. 下一步（Q03-A19）

1. 建立 `D20_Player_*.anim` 与 UAL clip 的映射表；
2. 以 `D20_PlayerHumanoidAvatar_R01` 为目标 Avatar 重建 15 个 D20 clip 为 Humanoid 肌肉空间；
3. 在 `D20_PlayerAnimation.controller` 补上**缺失的 Swimming 状态与过渡**（当前 15 状态、`transition_count=0`、`hasSwimmingState=false`）；
4. 跑 `Q03FormalActionAdmissionGateTests`，目标由 `PASS_AS_BLOCKED_GATE` 转为真实准入通过（`humanMotion > 0`）；
5. 回归 Q08 动作整合检查点，再回 Q31 人工视觉门禁。

## 9. 引用

- 根因诊断：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A18_FORMAL_ANIMATION_ROOT_CAUSE.md`
- 投放说明：同目录 `README.md`
- 现有门禁证据：`game/tools/results/LOCAL-QUALITY/Q03/`、`game/tools/results/LOCAL-QUALITY/Q08/`
