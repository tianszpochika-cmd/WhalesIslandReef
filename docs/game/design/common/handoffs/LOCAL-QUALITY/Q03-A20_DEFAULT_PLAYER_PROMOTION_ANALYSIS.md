# Q03-A20 默认 Player 晋级路径调研（只读，未落地）

> 日期：2026-09-14
> 执行者：Luna（AI agent）
> 卡片：`Q03-A20`（默认 Player 呈现晋级 —— 调研子卡）
> 状态：**`ANALYSED_NOT_IMPLEMENTED`** —— 路径已勘明，**未改任何生产代码**
> 前置：`Q03-A19`（正式动作面已重建为 Humanoid，门禁打开）
>
> ## ⚠️ 后续更新（2026-09-14，Q03-A23）
>
> 本卡第 5 节标记的「未验证的 Animator 冲突」**已被 Q03-A23 读码 + 实测排除**，
> 第 6 节记录的「无验证回路」也已用真实 Player 端到端日志绕开。
> 默认路径晋升**已由 `Q03-A23` 落地**并交付可试玩候选。
> 请以 [`Q03-A23_DEFAULT_PLAYER_PROMOTION_HANDOFF.md`](Q03-A23_DEFAULT_PLAYER_PROMOTION_HANDOFF.md) 为准；
> 本卡保留为当时的调研记录，第 5/6/7 节的结论已过时。

## 1. 为什么需要这张卡

`Q03-A19` 完成后，**默认 Player 仍然看不到新动作**。原因是 D20 桥接是 opt-in：

```csharp
// D20PlayerCandidatePresentationBridge.cs
private const string CandidateFlag = "-d20Candidate";
private const string FormalFlag = "-d20Formal";
private const string FormalHumanoidFlag = "-d20FormalHumanoid";

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
private static void InstallIfRequested()
{
    ...
    if (!candidateRequested && !_formalRequested && !_formalHumanoidRequested) return;   // 无 flag 直接返回
    ...
}
```

## 2. 真正的锁：一行常量

`game/Assets/Scripts/Gameplay/D2/D2RuntimeBootstrap.cs:59`

```csharp
// The scene's ph_crew_male asset is a Generic/NoAvatar skinned mesh.
// It is valid as an NPC anchor, but it has no audited action-axis
// contract for the Player. Keep it out of the controllable Player
// visual path until D20 supplies a verified runtime consumer; the
// procedural fallback has an explicit bone contract and avoids the
// distorted imported-rig poses visible in the native Player.
private const bool UseProceduralFallbackForUnverifiedImportedPlayer = true;
```

该常量在 3 处生效（L315 / L333 / L364），效果是：一旦检测到场景船员是「未验证导入蒙皮」，就**隔离它并换成程序化低模人物**，日志明说「这不是最终美术资产」。

**这就是用户报告的「任务动作扭曲 / 海洋不能游泳」的直接机制** —— 默认 Player 是一个程序化低模替身，没有骨骼动画。

**关键点：这条注释的解除条件已经被 Q03-A19 满足了。** 注释写的是「保持隔离，直到 D20 提供经过验证的运行时消费者」；现在 D20 已经有了有效 Humanoid Avatar + 15 个真实骨骼动画 clip。

## 3. 实测对照（只读探针）

| | 默认路径（现状） | D20 正式路径（可晋级） |
|---|---|---|
| 视觉来源 | `D2FallbackHumanoidBuilder` 程序化低模 | `Resources/D20/Formal/D20_R02_PlayerVisual_Male` |
| 蒙皮网格 | 无（Primitive 拼装） | **3 个 SkinnedMeshRenderer** |
| Avatar | 无 | **`D18_R03_A01_PlayerHumanoidMale`（valid=True, human=True）** |
| 可驱动骨骼动画 | **0 个** | **15/15 Humanoid clip** |

## 4. 晋级需要改什么（最小面）

1. `D20PlayerCandidateRuntimeInstaller.InstallIfRequested()` —— 无 flag 时也安装，且默认走 `formal + humanoidPose`；
2. 建议同时提供一个**反向 flag**（如 `-d2ProceduralPlayer`）保留回退能力，保证可逆。

改动局限在 `D20PlayerCandidatePresentationBridge.cs` 一个文件内。

## 5. ⚠️ 未验证的跨 owner 冲突风险（这是本卡不敢直接落地的原因）

`game/Assets/Scripts/Gameplay/D2/D2CharacterAnimationController.cs:842`

```csharp
private static void DisableOtherAnimators(Animator keeper)
{
    foreach (var animator in keeper.GetComponentsInChildren<Animator>(true))
        ...   // 禁用除 keeper 以外的所有 Animator
}
```

该方法由 `SetupFormalAnimator()`（Awake）调用。D20 桥接把可视实例挂为 **D2 物理根的子节点**，因此物理根下的 D20 Animator 落在 `GetComponentsInChildren<Animator>` 的扫描范围内。

- **理论时序**：`D2CharacterAnimationController.Awake` 先跑（此时 D20 实例尚未挂上）→ D20 在安装器的 Update 里挂载 → 可能不冲突。
- **但这是推测**。而且 `D2CharacterAnimationController` 全文**没有任何 D20 引用**，说明两个系统之间没有协同防护。
- 另注意：D20 安装器本身是 `[DefaultExecutionOrder(900)]`，桥接是 `1000`，`D2RuntimeBootstrap` 走 `RuntimeInitializeOnLoadMethod`，实际顺序需运行验证。

**结论：这是一个真实的、未验证的跨 owner 集成风险，不能靠读代码断言。** 它需要 PlayMode 集成测试来判定。

## 6. 工具边界：本轮无法完成验证

本会话中 Unity Pipeline 的 PlayMode 测试**只可靠产出过一次终态**（`D17ActionQualityPlayModeTests` 16/16，87.42s）。此后：

- `run_tests --mode PlayMode` 一律返回空 summary；
- `test_status` 持续返回上一次 D17 的缓存结果（total=16）；
- 尝试先 `editor_play` 再触发，仍无新终态。

而恰好用于验证本卡的两个套件都是 PlayMode：

- `Q03D20FormalPresentationPlayModeTests`
- `Q03D20FormalHumanoidPosePlayModeTests`
- `Q03D20CandidateIntegrationPlayModeTests`

因此**当前环境不具备落地本卡的验证回路**。

## 7. 为什么没有直接改

三条独立理由，任何一条都足以停手：

1. **不可验证** —— 见第 6 节；改默认呈现却无法跑集成回归，等于盲改。
2. **跨 owner** —— 需改 `Assets/Scripts/Gameplay/D2/`（D2 移动 owner），且第 5 节冲突涉及 `D2CharacterAnimationController`；本卡原定 owner 是 D20 动画。
3. **绕过人工门禁** —— 这会改变**默认玩家呈现**，而 `Q31` 的定义就是「人工试玩与 VC-G01」，且用户已明确**否决**过当前画面。在用户看到并认可之前就把它设为默认，是拿用户没批准的画面当成品。

## 8. 建议的落地顺序

1. **先构建一个带 `-d20FormalHumanoid` 的可见 Player**，让用户实际看新动作面（走、跑、游、操舵）。这是把 Q03-A19 成果变成可用反馈的最短路径。
2. 用户认可后，再翻默认（含反向 flag 保留回退），并补 PlayMode 集成回归。
3. 若用户不认可，缺陷退回对应 owner（动作质量 → D20 动画；握舵 → D7；植被 → Q20），默认保持不动。

## 9. 本卡未做的事（边界）

- **未修改任何生产代码**（`D2RuntimeBootstrap.cs` 常量仍为 `true`，桥接仍为 opt-in）；
- 未改默认 Player 呈现、未删除任何 flag、未动 `Assets/Scripts/`；
- 未声称新的默认行为可用；
- 探针脚本 `AgentScripts/LocalQuality/Q03A20PromotionProbe.cs` 为一次性只读诊断，**验收后已删除**。

## 10. 证据

- 环境：Unity `6000.0.80f1`、`com.unity.pipeline@0.6.0-exp.1`、Editor PID `53332`、`127.0.0.1:7800`
- 结束状态：`ready / compiling=false / playMode=stopped`（已干净退出 Play Mode）
- 相关代码：`D2RuntimeBootstrap.cs:59`、`D20PlayerCandidatePresentationBridge.cs:458-491`、`D2CharacterAnimationController.cs:842`
- 前置证据：`game/tools/results/LOCAL-QUALITY/Q03-A19/Q03_A19_RESULT.json`
