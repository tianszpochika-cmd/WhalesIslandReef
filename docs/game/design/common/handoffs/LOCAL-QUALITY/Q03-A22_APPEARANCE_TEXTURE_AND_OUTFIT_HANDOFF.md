# Q03-A22 角色外观贴图接线与服装合并

> 日期：2026-09-14
> 执行者：Luna（AI agent）
> 卡片：`Q03-A22`（D20 玩家外观美术接线）
> 单一 owner：**D20 美术 / ART**
> 状态：**`DELIVERED_VERIFIED`**
> 前置：`Q03-A19`（Humanoid 动作面）、`Q03-A21`（可见 Player 交付）

## 1. 一句话结论

**玩家角色从「纯色块裸低模」变成「有皮肤/头发/眼睛贴图 + 农民装」的角色。** 全部素材原本就在工程里，本卡是**纯整合**，未引入任何新素材。

## 2. 交付物

```
game/tools/results/LOCAL-QUALITY/Q03-A22/Q03_A22_TEXTURED_PLAYER_R01.exe
```

| 项目 | 值 |
|---|---|
| 构建 | `build_d0b8d7eb5afb`，`StandaloneWindows64` |
| 结果 | **Succeeded，0 errors / 1 warning** |
| 大小 | 243,860,861 bytes（较 R03 的 221 MB 增加，新增贴图与服装网格） |
| EXE SHA256 | `98751D0DFF0DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81` |

**运行方式**（同 Q03-A21）：

```powershell
cd "C:\Users\Administrator\Desktop\game\first01\game\tools\results\LOCAL-QUALITY\Q03-A22"
.\Q03_A22_TEXTURED_PLAYER_R01.exe -q03a21FormalPlayer -q03a21AutoQuit
```

## 3. 解决了什么

### 修复前（Q03-A21 审计实测）

| 渲染器 | 材质 | 贴图 |
|---|---|---|
| `SuperHero_Male`（身体） | `D20_PlayerSkin_Warm` | **NONE**，纯色 `(0.72,0.42,0.25)` |
| `Eyebrows` | `MI_Hair_1` | **NONE**，纯色 `(0.17,0.01,0.01)` |
| `Eyes` | `MI_Eyes` | **NONE**，纯色 `(0.80,0.80,0.80)` |

且正式路径**只有裸身体 3 个蒙皮网格，没有服装**。

### 修复后（验证实测）

| 路径 | 渲染器 | 状态 |
|---|---|---|
| 源外观 Prefab | 3 | 皮肤/头发/眼睛**全部 +Base +Nrm** |
| **Formal（晋级用）** | **7** | 3 身体件**全部带贴图** + **4 件农民装带贴图** |
| Candidate | 7 | 身体件带贴图（服装件仍为 FBX 生成材质，未接线） |
| 运行时皮肤材质 | — | `base=T_Superhero_Male_Ligh`、`normal=T_Superhero_Male_Normal`、`smooth=0.196` |

## 4. 贴图映射表

| 目标 | BaseColor | Normal | 其他 |
|---|---|---|---|
| 身体（`SuperHero_Male`） | `T_Superhero_Male_Ligh.png` | `T_Superhero_Male_Normal.png` | `T_Superhero_Male_Roughness.png` |
| 头发（`Eyebrows`） | `T_Hair_1_BaseColor.png` | `T_Hair_1_Normal.png` | — |
| 眼睛（`Eyes`） | `T_Eye_Brown.png` | `T_Eye_Normal.png` | — |
| 农民装（4 件） | `T_Peasant_BaseColor.png` | `T_Peasant_Normal.png` | `T_Peasant_ORM.png` |

全部来自已准入的 Quaternius **CC0** 包，且**原本就在工程内**。

**法线贴图使用 Unity 专用版本**：该资源包同时提供原始法线与 `Normals Unity - Godot/` 版本，Unity 版已按 `TextureImporterType.NormalMap` + `sRGB=false` 重新导入（否则 URP 会把法线当颜色贴图打光，表面发亮失真）。

## 5. 新增/修改的资产

| 文件 | 性质 |
|---|---|
| `Assets/Art/Models/Characters/D20/Player/Materials/D20_PlayerSkin_Warm.mat` | 修改：接身体 BaseColor + Normal + 由粗糙度均值推导的 Smoothness(0.196) |
| `.../D20_PlayerHair_Brown.mat` | 修改：接头发 BaseColor + Normal |
| `.../D20_PlayerEye_Brown.mat` | **新增**：接眼睛 BaseColor + Normal |
| `.../D20_PlayerOutfit_Peasant.mat` | **新增**：接农民装 BaseColor + Normal |
| `Assets/Editor/Q03A22/Q03A22AppearanceTextureBuilder.cs` | 新增：材质接线 builder |
| `Assets/Editor/Q03A22/Q03A22FormalOutfitBuilder.cs` | 新增：正式路径服装合并 builder |
| `Assets/Resources/D20/Formal/D20_R02_PlayerVisual_Male.prefab` | 重建：含 4 件农民装 |
| `Assets/Resources/D20/Materials/D20_PlayerSkin_Warm.mat` | 同步贴图（防运行时被覆盖，见第 6 节） |

**未触碰**：`Systems/`、`AgentCore/`、`contracts/`、场景、动画资产、默认 Player 路由、`D2RuntimeBootstrap`。

## 6. 一个必须修否则前功尽弃的坑

`D20PlayerCandidatePresentationBridge.ApplyReadableSkinMaterial()` 在**运行时强制**把 `Resources/D20/Materials/D20_PlayerSkin_Warm` 套到 `SuperHero_Male` 渲染器上。

该运行时副本在我第一次接线时**仍是旧的纯色版本** —— 意味着**Player 里皮肤贴图会被静默覆盖**，Editor 里看着对、跑起来又变回色块。

已在 builder 中加入 `SyncRuntimeSkinMaterial()` 步骤把两者保持同步，并在报告中记录 `runtime_skin_base_texture` / `runtime_skin_normal_texture` 作为回归断言点。

## 7. 已知边界与限制（如实登记）

1. **URP 版本无 `_MaskMap`** —— 实测该工程的 URP Lit 只暴露 `_OcclusionStrength`，**没有 `_MaskMap`**。因此已准入的 `T_Peasant_ORM.png` / `T_Superhero_Male_Roughness.png` **无法按标准通道打包**。当前做法：读取粗糙度贴图 64×64 下采样的**平均亮度**，取反后写入 `_Smoothness`（身体 0.196）。这是近似，不是完整的金属度/遮蔽/粗糙度管线。
2. **农民装 4 件共用一张 atlas 且 UV 区域重叠** —— 已实测登记（4 件两两重叠）。这是该模块化套装的**设计**（包内自带说明：各部件共享纹理）；已确认 4 件原本就共用同一材质，故单材质分配正确。
3. **Candidate 路径的服装件仍未接贴图** —— 其服装走的是 FBX 生成材质 `MI_Peasant`。**Formal 路径（晋级与交付用）已完整接线**，Candidate 仅作诊断路径，故未处理。
4. **外观合理性未做人工确认** —— 本卡只证明贴图/服装已生效，**不主张用户主观认可**。
5. 未做 GLB/纹理压缩优化；新增贴图使包体从 221 MB 增至 243 MB。

## 8. 验证证据

| 验证项 | 结果 |
|---|---|
| `Q03A22AppearanceTextureBuilder` | `status=PASS`，3 个渲染器接线，0 错误 |
| `Q03A22FormalOutfitBuilder` | `status=PASS`，7 渲染器/7 蒙皮，0 错误 |
| `D20A02` EditMode | **10/10 PASS** |
| `VcG01WorldLogic` EditMode | **14/14 PASS** |
| 编译 | `failed=false / errors=0` |
| 端到端 Player 自检 | 见下 |

端到端自检 `Player.log` 实测轨迹：

```
[Q03A21] scene_loaded=MainMenu
[Q03A21] newgame_created
[Q03A21] scene_loaded=InitialHarbor
[Q03A21] d2_root_found name=D2_PlayerAvatar
[Q03A21] bridge_installed mode=Formal humanoidPoseAdapter=True
         genericPathAdapter=False swimmingSupported=True
         hiddenRenderers=17 candidateRenderers=7      ← 由 3 增至 7，服装生效
[Q03A21] auto_quit                                       → code=0
```

错误扫描：**无异常**。

## 9. 复现命令

```powershell
# 顺序很重要：先接线材质，再重建正式资源，最后合并服装
# 1) Tools/WhalesIslandReef/Q03-A22/Wire Player Appearance Textures
# 2) Tools/WhalesIslandReef/D20-A02-R02/Build Formal Runtime Resources
# 3) Tools/WhalesIslandReef/Q03-A22/Merge Formal Player Outfit
```

> 注意顺序：第 2 步的 `EnsureReadableSkinRuntimeMaterial()` 会从源材质覆盖运行时副本，
> 而第 1 步已把源材质接好贴图，故覆盖结果正确。若顺序颠倒，运行时副本会退回纯色。

## 10. 状态影响

| 门禁 | 变化 |
|---|---|
| D20 玩家外观 | 纯色 → **贴图 + 服装** |
| `Q03` / `Q08` / `Q20` / `Q31` / `VC-G01` / `VC-G02` / `VC-G03` | **全部不变** |

## 11. 证据索引

- 接线报告：`game/tools/results/LOCAL-QUALITY/Q03-A22/Q03_A22_APPEARANCE_TEXTURE_WIRING.json`
- 服装报告：`game/tools/results/LOCAL-QUALITY/Q03-A22/Q03_A22_FORMAL_OUTFIT_MERGE.json`
- 交付 Player：`game/tools/results/LOCAL-QUALITY/Q03-A22/Q03_A22_TEXTURED_PLAYER_R01.exe`
- 前置：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A21_VISIBLE_FORMAL_PLAYER_HANDOFF.md`
