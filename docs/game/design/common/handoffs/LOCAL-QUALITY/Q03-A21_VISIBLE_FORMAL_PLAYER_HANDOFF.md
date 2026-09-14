# Q03-A21 可见 D20 正式动作 Player 交付（可试玩）

> 日期：2026-09-14
> 执行者：Luna（AI agent）
> 卡片：`Q03-A21`（让 Q03-A19 的正式动作面**可见**的 QA 交付）
> 状态：**`DELIVERED_VERIFIED`**
> 用途：让用户亲眼验证「动作扭曲」「海洋不能游泳」两项缺陷是否已由真实骨骼动画解决

## 1. 交付物

**唯一试玩候选**：

```
game/tools/results/LOCAL-QUALITY/Q03-A21/Q03_A21_D20_FORMAL_ACTION_PLAYER_R03.exe
```

| 项目 | 值 |
|---|---|
| 构建 | `build_5ccb7c921d77`，`StandaloneWindows64` |
| 结果 | **Succeeded，0 errors / 8 warnings** |
| 大小 | 221,344,653 bytes |
| EXE SHA256 | `98751D0DFF0DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81` |
| Assembly-CSharp SHA256 | `112730027359BFA6D50DFE11E3D0382DC0E71543C2F00A3AD02E814321F38B04` |
| 选项 | `Development + AllowDebugging` |

## 2. 怎么运行

**双击即可**（不需要加任何参数）：

```
Q03_A21_D20_FORMAL_ACTION_PLAYER_R03.exe
```

启动后它会：

1. 停在主菜单（**正常，你可以自己点**）；
2. 无参数直接双击时：**主菜单保持原样**，你按正常流程点「新游戏 → 确认」进港。

**想直接跳过菜单**（无人值守/快速看人）时加这两个参数：

```powershell
.\Q03_A21_D20_FORMAL_ACTION_PLAYER_R03.exe -q03a21FormalPlayer -q03a21AutoQuit
```

- `-q03a21FormalPlayer` —— 自动走「新游戏」路径进港，并挂载 D20 正式 Humanoid 角色
- `-q03a21AutoQuit` —— 安装完成后约 20 秒自动退出（只看不玩时用）

> ⚠️ **不加 `-q03a21FormalPlayer` 的话，你看到的还是旧的程序化低模人** —— 因为本卡**没有**改默认路径（见第 5 节）。

## 3. 你会看到什么

进港后玩家由 **D20 正式角色**呈现（不再是程序化拼装低模）：

- **走路 / 跑步** —— 真实骨骼步态，不再是「根节点滑行」
- **游泳**（进水后）—— 真实划水动作（`Swim_Fwd_Loop`），不再是整体倾斜 64° 的假姿态
- **上下船 / 操舵** —— 有真实的手部与身体动作

## 4. 自动化自检证据（已验证）

`R03` 端到端无人值守自检，`Player.log` 实测轨迹：

```
[Q03A21] runner_awake autoQuit=True
[Q03A21] scene_loaded=MainMenu
[Q03A21] newgame_created
[Q03A21] load_harbor_requested phase=initial
[Q03A21] scene_loaded=InitialHarbor
[Q03A21] d2_root_found name=D2_PlayerAvatar
[Q03A21] bridge_installed mode=Formal humanoidPoseAdapter=True
         genericPathAdapter=False swimmingSupported=True
         hiddenRenderers=17 candidateRenderers=3
[Q03A21] auto_quit
```

解读：

| 字段 | 含义 |
|---|---|
| `mode=Formal` | 走正式资源路径（`Resources/D20/Formal/*`） |
| `humanoidPoseAdapter=True` | 使用 Humanoid 重定向路径（Q03-A19 的成果生效） |
| `genericPathAdapter=False` | **旧的 generic 变换路径变通已不再被使用** |
| `swimmingSupported=True` | 游泳状态可用（`Fish` 状态存在） |
| `hiddenRenderers=17` | 17 个旧程序化渲染器已隐藏 |
| `candidateRenderers=3` | D20 角色的 3 个渲染器生效 |

进程干净退出 `code=0`，**零运行时错误**。

## 5. 本次**没有**改的东西（重要）

- **未修改默认 Player 路径**：`D2RuntimeBootstrap.UseProceduralFallbackForUnverifiedImportedPlayer` 仍为 `true`；
- **未修改** `D20PlayerCandidatePresentationBridge`（仍是 opt-in）；
- **未修改** `Resources/`、场景、Prefab、任何 Systems 类；
- **未触碰** D2/D7/D20 的任何生产代码。

新增的两个 QA 专属文件：

| 文件 | 性质 |
|---|---|
| `game/Assets/Scripts/Gameplay/D20/QA/Q03A21VisibleFormalPlayerRunner.cs` | QA runner，**无 flag 时完全惰性** |
| `game/Assets/Editor/Q03A21/Q03A21VisibleFormalPlayerBuilder.cs` | QA 构建入口（手动 menu 用） |

因此这个 Player **可随时作废，不影响正式流程**。

## 6. ⚠️ 已知美术缺口：角色是「纯色块」，没有贴图

按用户「希望人物看起来像样」的要求，我做了只读审计，结论必须如实说明：

**当前 D20 角色的全部材质都是纯色，一张贴图都没接：**

| 渲染器 | 材质 | 贴图 |
|---|---|---|
| `SuperHero_Male`（身体） | `D20_PlayerSkin_Warm` | **NONE**，纯色 `(0.72,0.42,0.25)` |
| `Eyebrows` | `MI_Hair_1` | **NONE**，纯色 `(0.17,0.01,0.01)` |
| `Eyes` | `MI_Eyes` | **NONE**，纯色 `(0.80,0.80,0.80)` |

**而工程里躺着 14 张现成、导入配置正确的 BaseColor 贴图没用上**，例如：

- `T_Superhero_Male_Ligh.png` / `T_Superhero_Male_Normal.png` / `T_Superhero_Male_Roughness.png`
- `T_Peasant_BaseColor.png` / `T_Peasant_Normal.png` / `T_Peasant_ORM.png`
- `T_Ranger_BaseColor.png`、`T_Hair_1_BaseColor.png`、`T_Regular_Male_Dark_BaseColor.png` 等

### 第二个美术缺口：正式路径**没有服装**

| 预制体 | 蒙皮网格 | 服装 |
|---|---|---|
| `Resources/D20/Formal/...`（本次晋级用） | 3（眉/眼/裸身体） | **无** ❌ |
| `Resources/D20/Candidate/...` | 7 | 4 件农民装 ✅ |

正式路径用的是 R02 源预制体（只有裸身体）；候选路径额外合并了 `D18_R03_A01_outfit_male_peasant_1675_local_r03` 的 4 件农民装。

**所以你现在看到的会是「一个有真实动作的裸低模人」。** 动作是对的，外观还需要一张专门的美术卡。

### 建议的后续美术卡（`Q03-A22`，owner：D20 美术 / ART）

1. 把 14 张已准入 BaseColor / Normal / ORM 贴图接到对应材质（皮肤、头发、眼睛）；
2. 把 4 件农民装合并进正式路径（复用候选路径已验证的骨骼重绑定逻辑）；
3. 统一皮肤/头发材质到单一来源，消除 `D20_PlayerSkin_*` × `MI_*` 的重复分叉；
4. 以 RenderTexture 或原生截图复核。

## 7. 复现命令

```powershell
# 1) 构建（走异步 build 命令，避免 Pipeline 60s 单请求上限）
#    Pipeline: {"command":"build","parameters":{
#      "target":"StandaloneWindows64",
#      "outputPath":"tools/results/LOCAL-QUALITY/Q03-A21/Q03_A21_D20_FORMAL_ACTION_PLAYER_R03.exe",
#      "options":["Development","AllowDebugging"],
#      "scenes":["Assets/Scenes/MainMenu.unity","Assets/Scenes/InitialHarbor.unity"],
#      "confirm":true}}
#    然后轮询 build_status 直到 completed

# 2) 自检
.\Q03_A21_D20_FORMAL_ACTION_PLAYER_R03.exe -q03a21FormalPlayer -q03a21AutoQuit

# 3) 看轨迹
Get-Content "$env:USERPROFILE\AppData\LocalLow\Whalebone Studio\WhalesIslandReef\Player.log" |
  Select-String "\[Q03A21\]"
```

## 8. 边界声明

- 本卡只证明**动作面可见且装得上**，不证明用户主观认可；
- **不证明**外观达标（见第 6 节缺口）；
- 不修改默认路径，**不解除** `Q03` 的正式目录准入；
- 舵位 IK 自然度、植被接地、目标设备性能、`VC-G01/VC-G02/VC-G03` 均**不变**。

## 9. 证据

- 构建产物：`game/tools/results/LOCAL-QUALITY/Q03-A21/`
- 运行轨迹：`%USERPROFILE%\AppData\LocalLow\Whalebone Studio\WhalesIslandReef\Player.log`（过滤 `[Q03A21]`）
- 机器可读报告：`%USERPROFILE%\AppData\LocalLow\Whalebone Studio\WhalesIslandReef\Q03A21\Q03A21_VISIBLE_FORMAL_PLAYER_RUNTIME.log`
- 前置：`game/tools/results/LOCAL-QUALITY/Q03-A19/Q03_A19_RESULT.json`
- 晋级调研：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A20_DEFAULT_PLAYER_PROMOTION_ANALYSIS.md`
