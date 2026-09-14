# Q03-A23 默认 Player 晋升为 D20 正式 Humanoid 路径

> 日期：2026-09-14
> 执行者：Luna（AI agent）
> 卡片：`Q03-A23`（默认 Player 呈现晋升）
> 单一 owner：**D2 呈现 / PRESENTATION**（可见的默认路由切换）
> 状态：**`DELIVERED_VERIFIED`**
> 前置：`Q03-A19`（Humanoid 动作面）、`Q03-A21`（可见 Player）、`Q03-A22`（贴图+服装）

## 1. 一句话结论

**玩家角色不再需要任何命令行开关就是 D20 正式角色。** 之前那套「程序化低模替身」退居为可选的诊断路径，由 `-d2ProceduralPlayer` 显式启用。

## 2. 为什么之前是 opt-in，现在可以翻

`D2RuntimeBootstrap.cs:59` 的注释写明了隔离条件：

```csharp
// ph_crew_male 是 Generic/NoAvatar 蒙皮。它作为 NPC 锚点有效，但对 Player
// 没有经过审计的动作轴契约。保持它远离可控 Player 视觉路径，
// 直到 D20 提供经过验证的运行时消费者；程序化回退有明确的骨骼契约，
// 可避免原生 Player 里可见的导入骨架扭曲姿态。
private const bool UseProceduralFallbackForUnverifiedImportedPlayer = true;
```

**这个「直到」条件已由 Q03-A19 满足**：

| 隔离条件要求 | 现状 |
|---|---|
| 经过验证的运行时消费者 | ✅ `D20PlayerCandidatePresentationBridge` 正式路径 |
| 有效的人形骨架 | ✅ Avatar `valid=True / isHuman=True` |
| 经过审计的动作轴契约 | ✅ 15/15 clip `humanMotion=true`、`genericRoot=0`、19 条过渡 |

而且那个程序化替身**正是**用户报告的「任务动作扭曲」与「海洋不能游泳」的直接成因 —— 它没有骨骼动画。

## 3. 改动内容

**唯一生产代码改动**：`Assets/Scripts/Gameplay/D2/D20PlayerCandidatePresentationBridge.cs`

| 位置 | 改动 |
|---|---|
| `D20PlayerCandidateRuntimeInstaller.InstallIfRequested()` | 无路由 flag 时**默认晋升** formal + humanoidPose |
| 同上 | 新增 `-d2ProceduralPlayer` **回退开关**（保留旧路线，无需重新构建） |
| 同上 | 显式 `-d20Candidate/-d20Formal/-d20FormalHumanoid` 仍可指定特定路线（QA 用） |
| `D20PlayerCandidateRuntimeInstallerBehaviour` | 新增 `_explicitRoute`，让日志如实区分「默认」与「显式 flag」 |
| 类级注释 | 更新为反映晋升事实（原注释是 opt-in 时代写的，已过时） |

**未改动**：`D2RuntimeBootstrap.cs`（那行常量仍是 `true`，NP​​C 路径与呈现所有者边界不变）、`Systems/`、`AgentCore/`、`contracts/`、场景、动画资产。

## 4. ⚠️ 曾担心的 Animator 冲突：已排除

Q03-A20 调研时标记了一个未验证风险：`D2CharacterAnimationController.DisableOtherAnimators()` 会禁用物理根下**除自己以外**的所有 Animator，而 D20 把可视实例挂为物理根子节点，落在扫描范围内。

**本次读源码 + 实测排除了这个风险：**

- `SetupFormalAnimator()`（内含 `DisableOtherAnimators`）**只在 `Awake` 调用一次**（`D2CharacterAnimationController.cs:157`）；
- D2 物理根（`D2_PlayerAvatar`）在 bootstrap 阶段创建，其 `Awake` 早于 D20 桥接安装；
- D20 桥接是在之后的 `Update` 轮询里挂载的 —— **那时 `DisableOtherAnimators` 早已执行完毕**；
- 实测佐证：桥接报告 `candidateRenderers=7`、`Humanoid pose adapter=True`，且 Q03-A19 的骨骼级探针实测到 19 根骨骼产生位移。

结论：**该冲突不会发生**。原调研中的「推测」现已由证据取代。

## 5. 修复了一个我自己的设计缺陷

Q03-A21 交付的 QA runner 自带安装逻辑，**会覆盖生产安装器的决定**：

第一次测试回退开关时出现：

```
[D20/Default] -d2ProceduralPlayer supplied; retaining the procedural stand-in route.   ← 生产安装器正确让位
[D20/Formal]  R02 Player visual installed ...                                          ← QA runner 又把 D20 装回去了 ❌
```

已修：runner 读取 `-d2ProceduralPlayer`，若存在则**主动让位**（`runner_stood_down`），不再越过生产决策。

## 6. 验证证据

### 两条路径实测（同一份 R03 构建）

**默认路径（无路由 flag）**：

```
[D20/Default] No route flag supplied; promoting the formal Humanoid player route.
[D20/Formal]  R02 Player visual installed under the D2 physical root;
              root motion disabled; generic path adapter=False; Humanoid pose adapter=True.
[D20/Formal]  installer completed (default route).
```

**回退路径（`-d2ProceduralPlayer`）**：

```
[D20/Default] -d2ProceduralPlayer supplied; retaining the procedural stand-in route.
[Q03A21]      runner_stood_down reason=-d2ProceduralPlayer
（无任何 D20 安装日志）
```

两次进程均 `code=0` 干净退出，**错误扫描无异常**。

### 回归

| 测试套件 | 模式 | 结果 |
|---|---|---|
| `D20A02` | EditMode | **10/10 PASS** |
| `VcG01WorldLogic` | EditMode | **14/14 PASS** |
| `D16Accessibility` | EditMode | **8/8 PASS** |
| 编译 | — | `failed=false / errors=0` |

## 7. 交付物

```
game/tools/results/LOCAL-QUALITY/Q03-A23/Q03_A23_DEFAULT_PLAYER_R03.exe
```

| 项目 | 值 |
|---|---|
| 构建 | `build_859bab32a0ef`，`StandaloneWindows64` |
| 结果 | **Succeeded，0 errors** |
| EXE SHA256 | `98751D0DFF0DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81` |

**运行方式**：直接双击即可看到 D20 正式角色（贴图 + 农民装 + 真实骨骼动作）。
自动进港（免点菜单）：额外加 `-q03a21FormalPlayer`（QA runner 激活，只负责推进场景，不负责安装）。

## 8. 已知边界（如实登记）

1. **外观合理性仍未获人工确认** —— 本卡只证明「默认路径 = D20 正式角色」且可逆，**不主张用户认可外观**。用户看过 Q03-A22 版本后尚未给出反馈。
2. **Q03 的资源目录准入仍未完成** —— 晋升的是**运行时呈现路由**，不是 D18 catalog 的 `LocalDevelopmentReady` 准入。
3. **程序化替身仍在创建** —— bootstrap 仍会生成它作为 D2 物理根承载体，其 17 个渲染器被桥接隐藏。这是设计使然（物理根与可视层分离），不是残留缺陷，但它仍占用构建体积与启动开销。
4. **PlayMode 集成测试仍未复验** —— 本会话 Pipeline 的 PlayMode 只会话首次可靠产出终态，`Q03D20FormalPresentationPlayModeTests` / `Q03D20FormalHumanoidPosePlayModeTests` / `Q03D20CandidateIntegrationPlayModeTests` 未能取得新终态。已用 Editor 静态读码 + 真实 Player 端到端日志替代，但**不声称这三个套件通过**。
5. **`Q31` / `VC-G01` / `VC-G02` / `VC-G03` 状态不变**。

## 9. 回滚方法

无需重新构建：

```powershell
.\Q03_A23_DEFAULT_PLAYER_R03.exe -d2ProceduralPlayer
```

或恢复源码：把 `InstallIfRequested()` 改回「无 flag 时 return」即可（改动集中在单个方法内）。

## 10. 证据索引

- 交付 Player：`game/tools/results/LOCAL-QUALITY/Q03-A23/Q03_A23_DEFAULT_PLAYER_R03.exe`
- 前置交接：`Q03-A21_VISIBLE_FORMAL_PLAYER_HANDOFF.md`、`Q03-A22_APPEARANCE_TEXTURE_AND_OUTFIT_HANDOFF.md`
- 晋升调研（含已被本次推翻的推测）：`Q03-A20_DEFAULT_PLAYER_PROMOTION_ANALYSIS.md`
- 改动源码：`game/Assets/Scripts/Gameplay/D2/D20PlayerCandidatePresentationBridge.cs`

## 2026-09-14 current-source revalidation addendum

后续接管回归已补齐本卡当时明确保留的 PlayMode 缺口：当前 `Q03Formal` EditMode=`5/5 PASS`，`Q03D20Formal` PlayMode=`2/2 PASS`，D2 Animation=`8/8 PASS`，D17 Action Quality=`16/16 PASS`，recompile=`failed=false/errors=[]`。旧的“尚未取得三个 Q03D20 PlayMode 终态”描述属于本卡当时的时间点，不删除历史记录；当前权威节点为 [Q03 current default route result](../../../../../../game/tools/results/LOCAL-QUALITY/Q03/Q03_CURRENT_DEFAULT_HUMANOID_ROUTE_20260914_LUNA_RESULT.json)。

同一 connected Pipeline 的 `InitialHarbor` route probe 直接观察到默认 `Formal`、`HumanoidPoseAdapter=true`、`GenericPathAdapter=false`、7 个 candidate skinned renderer、17 个旧 fallback renderer 隐藏、`SwimmingVisualSupported=true`、installation failure 为空。随后用当前生产源码构建 exact Player `build_099a228c638b`（`0 errors / 8 warnings`，EXE SHA=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`）。

状态边界不变但需要更精确表述：Q03-A23 已经证明默认呈现路由和正式 Humanoid 代码/资源门在范围内成立；它仍没有证明 compositor-visible 画面、前台 Win32 物理输入、自然动作/泳姿/握舵、全镜头穿模、植被接地、目标设备性能、D18 `LocalDevelopmentReady` 或 VC-G01 最终通过。`-d2ProceduralPlayer` 回退开关仍可用。下一依赖卡为 Q04/Q05，最终画面门仍由 Q31/VC-G01 负责。
