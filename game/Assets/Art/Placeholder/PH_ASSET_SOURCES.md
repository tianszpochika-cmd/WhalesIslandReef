# 占位素材来源与授权清单（P1C 初始环境场景 · P1C-U2 美化）

> 纪律依据：`docs/game/SETUP.md` §5（占位素材只用 CC0/CC-BY）+ `docs/ARCHITECTURE.md` §8.1（`ph_` 前缀、整目录替换）。
> 更新：2026-08-08（P1C-U2，新增 Quaternius CC0 模型与贴图）

## 总原则

占位素材 = **Quaternius CC0 低模模型（船/人形）+ Unity 内置几何体（植被/岩石/码头）+ 自写 URP 纯色材质 + 开源字体**。
全部来自 CC0/CC-BY 白名单平台；未引入任何版权不明素材；替换时整目录更换、不污染正式目录。

## 清单

| 资产 | 路径 | 类型 | 来源 | 授权 |
|---|---|---|---|---|
| 小渔船模型 | `Assets/Art/Placeholder/Models/Ships/ph_ship_fishing.fbx` | FBX（BoatWSail 带帆小船） | **Quaternius** *Ship pack*（2026-08-08 下载） | **CC0 1.0** |
| 老水手模型 | `Assets/Art/Placeholder/Models/Crew/ph_crew_male.fbx` | FBX（Animated Human 男性站姿） | **Quaternius** *Man Animated*（2026-08-08 下载） | **CC0 1.0** |
| 角色贴图 | `Assets/Art/Placeholder/Textures/ph_crew_clothed_light.png` | 32×32 调色板贴图 | **Quaternius** *Man Animated*（2026-08-08 下载） | CC0 1.0 |
| 棕榈叶贴图 | `Assets/Art/Placeholder/Textures/ph_palm_leaves.png` | 32×32 调色板贴图 | **Quaternius** *Desert Pack*（2026-08-08 下载） | CC0 1.0 |
| 岩石贴图 | `Assets/Art/Placeholder/Textures/ph_rock.png` | 32×32 调色板贴图 | **Quaternius** *RocksFlowersGrassPack*（2026-08-08 下载） | CC0 1.0 |
| 草/花贴图 | `Assets/Art/Placeholder/Textures/ph_grass.png` / `ph_flower.png` / `ph_flower2.png` | 32×32 调色板贴图 | **Quaternius** *RocksFlowersGrassPack*（2026-08-08 下载） | CC0 1.0 |
| 授权原文 | `Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt` | 授权文件 | Quaternius 各包内嵌 | CC0 1.0 全文 |
| 中文字体 | `Assets/Art/Placeholder/Fonts/ph_font_noto_sans_cjk_sc.otf` | OTF 字体 | [googlefonts/noto-cjk](https://github.com/googlefonts/noto-cjk) `Sans/OTF/SimplifiedChinese/NotoSansCJKsc-Regular.otf` | **SIL Open Font License 1.1** |
| 椰树/岩石/草丛/花 | 场景内几何体（Cylinder/Cube/Sphere 组合） | 占位几何体 | Unity 引擎内置 Primitive + 自写 URP 纯色材质 | 引擎自带/自产 |
| 码头/沙滩/海面 | 场景内几何体 + URP 材质 | 占位 | 同上 | 同上 |
| 渲染材质 | `Assets/Art/Placeholder/Materials/ph_mat_*.mat`（由安装器生成） | URP Lit 材质 | `HarborArtInstaller.cs` 自产 | 自产（引擎 Shader） |
| 场景 | `Assets/Scenes/InitialHarbor.unity`（由安装器生成） | 场景 | `HarborArtInstaller.cs` 自产 | 自产占位 |

## 下载记录（2026-08-08）

| 素材 | 下载源 | 说明 |
|---|---|---|
| Ship pack / Man Animated / Desert Pack / RocksFlowersGrassPack | Quaternius 官网 https://quaternius.com（作者 @Quaternius，Patreon: https://www.patreon.com/quaternius）；**镜像仓库** [beep2bleep/FreeAssetsByKenneyNLandQuaternius](https://github.com/beep2bleep/FreeAssetsByKenneyNLandQuaternius)（GitHub，含各包 License.txt，本机通过 GitHub API 下载） | kenney.nl 官网本机不可达（网络限制），故船/人形改用 Quaternius（同为 CC0） |
| 椰树/岩石/草丛 | 未下载模型（Quaternius 包内 .blend 无法被 Unity 导入，本机无 Blender 转换）→ **降级**：内置几何体 + URP 纯色材质（任务允许的降级方案） | 贴图已下载存档，正式美术阶段可用 |

## 授权文件

- **Quaternius**：CC0 1.0 Universal（Public Domain Dedication）→ 见 `PH_QUATERNIUS_LICENSE.txt` 与 https://creativecommons.org/publicdomain/zero/1.0/
- **Noto Sans CJK SC**：OFL 1.1 全文 https://openfontlicense.org/ ｜ 项目仓库 https://github.com/googlefonts/noto-cjk

## 场景安装方式（P1C-U2）

所有场景改动经 Editor 脚本完成（**绝不手写场景 YAML**）：
1. Unity 打开工程（自动导入 FBX/贴图并生成 .meta）
2. 菜单 `Tools > Whalebone > Install Harbor Art (占位美化, 幂等)` → 重建 InitialHarbor：
   - 海面：半透明蓝 URP Lit（轻微起伏不做，占位）
   - 沙滩：沙黄色 + 岩石×6；码头：木色平台/桩/系缆桩/木桶/货箱/跳板
   - 船：Quaternius FBX（按包围盒缩放到 4.6m，船头朝海，覆盖船体棕材质）
   - 老水手：Quaternius FBX（缩放到 1.8m，站船头面向大海，覆盖亚麻色材质）
   - 椰树×5 + 草丛×8 + 花×4：内置几何体沿沙滩边缘
   - 灯光：暖色 Directional + Trilight 环境光调亮；天空：Procedural Skybox（纯代码）
   - 保留：CameraFollow 绑定、UGUI 地名标签、Build Settings 顺序
3. 验证：`Tools > Whalebone > Smoke Test Harbor Art`（断言船/水手/椰树/岩石/海面材质/相机/标签）

## 替换计划（灰盒 → 正式素材）

- D2+ 引入 Crest Ocean 后，海面占位平面替换为真实海洋渲染；
- Synty 海盗/港口包（SETUP.md §5.2 预算 $150-300）到位后，整目录替换 `Art/Placeholder/` 与 `Prefabs/Placeholder/`，场景仅改脚本内模型路径即可。
