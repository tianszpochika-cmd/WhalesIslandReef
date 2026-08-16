# D18-A04-R02-F 女性/中性人形候选补缺包

结论：**PARTIAL**

本包只登记隔离的 Candidate 资源，不执行 D20 玩家 Prefab、正式角色替换、Animator、场景接入或准入裁决。D18-S02-R01 仍是唯一准入裁决入口。

## 补缺目标与实际交付

当前 D18-A04 已有中性 Kenney 基体和 Quaternius 男性包，但没有可核验的女性人形基体。本包补入一套官方 Quaternius Animated Women Pack 的四个独立 FBX 模型：

| target_id | 原始文件 | 源文件 SHA-256 | 大小 |
|---|---|---|---:|
| `asset_quaternius_animated_women_female_alternative_v01` | `Female_Alternative.fbx` | `6096FADC3B31007B1026AA9211560E89917B4B9A16A118629D44CCD269FA7F00` | 1,983,292 |
| `asset_quaternius_animated_women_female_casual_v01` | `Female_Casual.fbx` | `CAF4787B97AEFCF53F94C44D15C4BAF99808DE6D6E806372235DE587DDF6A6B0` | 1,992,940 |
| `asset_quaternius_animated_women_female_dress_v01` | `Female_Dress.fbx` | `9D806915935520BBE8F70287A9AEF465E10153E855E7D20F7242BB653B211480` | 1,971,500 |
| `asset_quaternius_animated_women_female_tank_top_v01` | `Female_TankTop.fbx` | `8825C86A1459A817E400B0AD1396EA3BF99C2D3AA234F5BEC3A9E6F3AF10746D` | 1,981,036 |

四个文件均为真实不同的 FBX 网格，不以换色重复计数。隔离目录为：

`game/Assets/Art/Candidates/CharactersAnimals/GapFill/FemaleNeutral/Models/`

机器 manifest：`d18_a04_r02_f.manifest.json`。它保持 `review_status=Candidate`、`import_status=NotImported`、`runtime_consumers=[]`，没有 `intake_eligible`、`LocalDevelopmentReady` 或 `ReleaseApproved`。

## 来源与许可证

- 官方资源页：[Quaternius Animated Women Pack](https://quaternius.com/packs/animatedwomen.html)。页面列出 4 个 animated human models、FBX/OBJ/Blend、CC0，并说明可用于个人和商业项目。
- 官方下载入口：[Quaternius Google Drive folder](https://drive.google.com/drive/folders/1c13R--fMqdR6r2MRlcKKsbPky0__T-yJ?usp=sharing)。
- 实际下载字节：[Poly Pizza direct archive](https://static.poly.pizza/list/HHSKxnk1mY-zip--1745939261.zip)。Poly Pizza 页面将包作者标为 Quaternius、列出同一四个模型并标记 Public Domain (CC0)；该 URL 只作为实际字节来源记录，不替代官方资源页。
- 下载日期：2026-08-15。
- 原始下载文件名：`Animated-Women-Pack-HHSKxnk1mY-zip--1745939261.zip`。
- 原包大小：1,783,003 bytes。
- 原包 SHA-256：`BCC6170D0175388FDF9AC7A3FC1E59B9250178E913B94B0D374B4795A1311C9F`。
- 许可证快照：`game/Assets/Art/Candidates/CharactersAnimals/GapFill/FemaleNeutral/LICENSE_EVIDENCE.md`。
- 许可证：[CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/)，官方 FAQ：[Quaternius FAQ](https://quaternius.com/faq.html)。

原包目前位于 `.gitignore` 覆盖的 `artifacts/external_sources/` 根缓存；普通 Git 历史不包含压缩原包。

## 源文件结构审计

`D18_A04_R02_F_FBX_STATIC_AUDIT.json` 是只读二进制 FBX 结构审计，不冒充 Unity 导入结果。四个 FBX 的共同证据：

- FBX binary version 7400；每个文件 1 个 `Geometry`、1 个 Mesh model、42 个 `LimbNode`、32 个 Deformer、1 个 Connections node。
- 每个文件 11 个 `AnimationStack`、11 个 `AnimationLayer`、3,168 个 `AnimationCurve`；四个文件合计 44 个源动画栈，不宣称为 Unity `AnimationClip` 数量。
- 动作标签：`Female_Clapping`、`Female_Death`、`Female_Idle`、`Female_Jump`、`Female_Punch`、`Female_Run`、`Female_RunningJump`、`Female_Sitting`、`Female_Standing`、`Female_SwordSlash`、`Female_Walk`。
- 材质节点分别为 5、8、8、11 个；可以观察到 `Skin`、`Eyes`、`Hair`/`HairBase` 与服装材质，但每个文件仍只有一个 Mesh model，没有独立可换发型/服装网格的证据。
- 骨架包含 `Hips`、`Torso`、`Neck`、`Head`、`Shoulder.L/R`、`UpperArm.L/R`、`LowerArm.L/R`、`UpperLeg.L/R`、`LowerLeg.L/R`、`Foot.L/R` 等；D20 要求的精确 `Spine`/`Chest` 命名、Humanoid Avatar、重定向安全性尚未验证。
- 未发现 `Texture` 节点；材质/贴图是否在 Unity 中正确提取和绑定仍需导入检查。

## D20 适配边界

这套资源可以作为 D20-A01 的“女性外观方向”候选证据，补足“没有女性基体”的真实缺口；它不能单独证明第二外观方向已经可发布，也不能证明时代适配。

仍未补齐或未获准入的项目：

- 1675 年加勒比服装：`Casual`、`TankTop`、`Dress`、`Alternative` 的时代/文化适配未完成，不能直接列为可用港口角色；需要人工时代审查和必要的服装替换。
- 发型：只有 `Hair`/`HairBase` 材质名，没有六个独立可换发型槽或独立发型网格证据。
- 服装：只有材质分组，没有 D10 outfit/asset 组合数据，也没有帽子、外套、鞋、手套、饰品和职业装备模块。
- 技术：没有 Unity Editor，因此没有 Scale/Y-up/轴向、脚底、bounds、Read/Write、Avatar、材质提取和动画片段导入结果。
- 运行时：没有 Prefab、Animator、场景、CharacterSystem、存档或 Registry 改动。

## 验证结果

| 检查 | 结果 |
|---|---|
| manifest schema/审计 | PASS；0 issues，manifest 保持 Candidate |
| 原包哈希 | PASS；原包和本地缓存 SHA-256 一致 |
| FBX 静态结构 | PASS；4/4 文件可读取头部、几何、骨架、材质节点和动画栈 |
| Unity 导入/Console | NOT RUN；机器没有 Unity Editor executable |
| D20 Prefab/Animator/场景接入 | NOT RUN；按任务边界禁止 |
| git diff --check | 待最终交接前执行 |

没有 Unity Editor 是本任务标记 PARTIAL 的直接原因；不得把静态 FBX 证据升级为 Unity import verified。可复现的 Unity 检查命令、当前环境探测和实际结果见 `game/tools/results/D18/D18-A04-R02-F/D18_A04_R02_F_VALIDATION.md`。
