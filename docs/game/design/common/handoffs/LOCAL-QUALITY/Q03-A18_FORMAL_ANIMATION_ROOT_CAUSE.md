# Q03-A18 正式动作资源根因诊断（只读）

> 日期：2026-09-14
> 执行者：Luna（AI agent）
> 卡片：Q03-A18（Q03 正式动作/Avatar 资源门禁的诊断子卡）
> 状态：`DIAGNOSED` —— 根因已定位，**未做任何生产代码/资源改动**
> 关联门禁：`Q03=BLOCKED_ASSET`、`Q08=BLOCKED`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`

## 1. 诊断方法

使用已连接的 Unity Pipeline（`com.unity.pipeline@0.6.0-exp.1`、Unity `6000.0.80f1`、Editor PID `53332`、`127.0.0.1:7800`）做**只读** `eval` 查询。

调用格式（本次探明的正确协议，供后续复用）：

```
POST http://127.0.0.1:7800/api/exec
Authorization: Bearer <Library/Pipeline/.unity-pipeline-port 中的 evalToken>
Content-Type: application/json

{"command":"eval","parameters":{"code":"return <表达式>;","timeout":60000}}
```

注意三点，否则会失败：
1. 参数键名是 `parameters`（**不是** `args` / `params` / `code` 直挂）；
2. `code` 必须是**完整语句**，需显式写 `return ...;`，不能只写表达式；
3. 求值结果在响应的 `result.result`（不是 `result.output`，`output` 恒为 `null`）。

本次查询全程只读，未修改生产代码、资源、Prefab、场景或运行时配置；Editor 结束时 `ready/stopped`。

## 2. 根因结论（直接原因）

**当前玩家角色本体没有任何动画数据；现有的 15 个 D20 动作 clip 全是「根节点变换曲线」，不是骨骼动作。**

证据：

| 检查项 | 实测结果 | 含义 |
|---|---|---|
| `D20_Player_Walk.anim` 的 `humanMotion` | `False` | 不含 Humanoid 肌肉空间数据 |
| 同 clip 的曲线绑定路径 | `/localPosition.y`、`/localEulerAnglesRaw.x/y/z`（根节点） | 只是在整体位移/倾斜整个角色根，**没有驱动任何骨骼** |
| 玩家模型 `Superhero_Female_FullBody.fbx` 内嵌 clip 数 | `0` | 角色本体不带任何动作 |
| 该 FBX 的 `animationType` | `Generic`（值 `1`） | 未按 Humanoid 导入 |

这解释了 Q03 门禁日志一直报的 `clips=15; humanMotion=0; genericRoot=15` —— 门禁判得没错，资源确实不合格。

**推论（对应你亲眼看到的 4 个缺陷）**：
- **「任务动作扭曲」** —— 根本没有骨骼级 walk/run 动作，只有根节点位移，看起来就是滑步/扭曲；
- **「海洋不能游泳」** —— 不存在游泳动作，当前只能用程序化代码整体倾斜 visualRoot（pitch 64°）+ 摆四肢凑数；
- **「上船像控制瓢」** —— 没有握舵/手部动作，只能靠程序化前臂角度近似；
- **「树在空中飘」** —— **这一条与动作资源无关**，是 D19-R03 表现生成器的地形接地问题（见第 4 节）。

## 3. 已具备的有利条件（不需要重建的部分）

### 3.1 有效的人形 Avatar 已经存在且可用

| Avatar 资产 | `isValid` | 人形骨骼数 | 骨架骨骼数 |
|---|---|---|---|
| `Assets/Art/Models/Characters/D20/Player/D20_PlayerHumanoidAvatar_R01.asset` | `True` | 20 | 70 |
| `.../PlayerAppearance/ProjectOwned/R03/Avatars/D18_R03_A01_PlayerHumanoidMale.asset` | `True` | 20 | 70 |
| `.../PlayerAppearance/ProjectOwned/R03/Avatars/D18_R03_A01_PlayerHumanoidFemale.asset` | `True` | 20 | 70 |

这三个 Avatar 由 `game/Assets/Editor/D20A01R01/D20A01PlayerVisualBuilder.cs` 通过 `AvatarBuilder.BuildHumanAvatar` 从同一套已准入骨架生成，20 个人形骨骼映射完整（Hips→pelvis、Spine→spine_01 … RightFoot→foot_r）。

**关键含义**：只要有合法的 Humanoid 动画源，就能直接重定向（retarget）到这个 Avatar，**不需要换角色模型、不需要重做骨架映射**。

### 3.2 角色骨架命名规范，支持标准重定向

```
Armature/root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/
lowerarm_l/hand_l/index_01_l/.../thigh_l/calf_l/foot_l/...
```

命名清晰、层级正常，是标准可重定向骨架。

### 3.3 项目里已有候选人形动作库（CC0）

`game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedMen/`（授权：**CC0 1.0 Universal**，见同目录 `License.txt`，商用无限制）

4 个模型（`Male_Casual` / `Male_LongSleeve` / `Male_Shirt` / `Male_Suit`）**每个都内嵌同样 11 个骨骼动画**：

```
Man_Idle, Man_Walk, Man_Run, Man_Jump, Man_RunningJump,
Man_Standing, Man_Sitting, Man_Clapping, Man_Punch, Man_SwordSlash, Man_Death
```

当前导入状态：`animationType=Generic`, `avatarSetup=NoAvatar`, `sourceAvatar=null`。

**缺口：这 11 个里没有游泳动作（无 Swim）**，也没有 Board / Disembark / Helm 这类航海专用动作。

## 4. 「树在空中飘」的独立结论

该项**不是资源缺失**，而是表现生成器的接地逻辑问题：

- 路径：`D19R03HarborPresentationBuilder` 的候选植被放置
- 现状：已改用 `GroundHeight()` 优先对 `D19_TerrainRoot` 的 MeshCollider 做射线，解析式 `Height()` 仅作回退（Q20-A05）
- 已验证：`activeLegacyTreeRenderers=0`、`activeR03Vegetation=24`、`unsupportedR03Vegetation=0`
- 仍开放：正式棕榈资源未准入（`D18 LocalDevelopmentReady = BLOCKED / 0 / 0`，`RenderAuthoredPalms=false`）

**结论**：这一条**不需要你下载任何东西**，属于代码/准入可自行推进的范围。

## 5. 需要补充的资源（最小集）

| 优先级 | 动作 | 用途 | 备注 |
|---|---|---|---|
| P0 | **Swim / Swimming** | 关闭「海洋不能游泳」 | Mixamo 有 `Swimming`；Quaternius 动作库内没有 |
| P0 | Idle / Walk / Run | 关闭「任务动作扭曲」 | Quaternius 候选已具备，但需转 Humanoid 才能用 |
| P0 | Jump / Fall / Land | 替换现有根变换占位 | 同上 |
| P1 | Board / Disembark / Helm | 上下船与操舵 | Mixamo 无航海语义，可用 `Climbing` / `Standing` 近似后按 owner 适配 |
| P1 | Interact / Talk / Carry | 交互、对话、搬运 | 现有 clip 名已定义，缺真实内容 |
| P2 | Sitting / Hit / Recover | 酒馆、受击、恢复 | 候选库已具备 |

**硬性技术要求**（不满足则无法重定向）：
1. 必须是 **Humanoid / 含人形骨架**的 FBX；
2. 同一个来源角色的动作要**成组下载**（Mixamo 同一角色的动作才能共享骨骼映射）；
3. 导出格式 **FBX Binary (.fbx)**，**Skin = Without Skin**（只要动作，不要网格）；
4. 帧率 **30 fps**，Keyframe Reduction = none（保留精度）。

## 6. 本次没有做的事（边界声明）

- 未修改任何生产代码、测试、Prefab、场景、材质或运行时配置；
- 未把 `Generic` 改成 `Humanoid`、未 reimport 任何 FBX、未新建 Avatar 或 AnimatorController；
- 未准入任何正式资源，`D18 LocalDevelopmentReady` 保持 `BLOCKED / 0 / 0`；
- 因此 `Q03` 继续 `BLOCKED_ASSET`、`Q08` 继续 `BLOCKED`、`Q20` 继续 `PARTIAL/BLOCKED_ASSET`、`Q31` 继续 `PARTIAL`、`VC-G01` 继续 `IN_PROGRESS`、`VC-G02/VC-G03` 继续 `LOCKED`。

## 7. 建议的后续卡片

- **Q03-A19**（单一 owner：D20 动画 / ANIM）：导入正式 Humanoid 动作源，重建 15 个 D20 clip 为 Humanoid 肌肉空间，并在 `D20_PlayerAnimation.controller` 中**补上缺失的 Swimming 状态与过渡**（当前 controller 15 个状态、`transition_count=0`、`hasSwimmingState=false`）。验收：`Q03FormalActionAdmissionGateTests` 由 `PASS_AS_BLOCKED_GATE` 转为真实准入通过，`humanMotion > 0`。
- **Q20-A06**（单一 owner：D19-R03 表现生成器 / ART）：正式棕榈资源准入 + 坡面根部接地。与动作资源解耦，可并行。
- 两者完成后回归 Q08（动作整合检查点），再回 Q31。

## 8. 证据

- 诊断时间：2026-09-14
- 环境：Unity `6000.0.80f1`、`com.unity.pipeline@0.6.0-exp.1`、Editor PID `53332`、`127.0.0.1:7800`、`ready/stopped`
- 工作区：分支 `agent/publish-project-changes-20260813`、HEAD `dc1f8713`（`docs: pause local quality at Q31 R5`）
- 相关既有结果：`game/tools/results/LOCAL-QUALITY/Q03/`、`game/tools/results/LOCAL-QUALITY/Q08/`
- 相关源码：`game/Assets/Editor/D20A01R01/D20A01PlayerVisualBuilder.cs`、`game/Assets/Tests/EditMode/D20A02/D20A02AnimationBuilder.cs`
