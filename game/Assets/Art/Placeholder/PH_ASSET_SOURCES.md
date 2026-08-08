# 占位素材来源与授权清单（P1C 初始环境场景）

> 纪律依据：`docs/game/SETUP.md` §5（占位素材只用 CC0/CC-BY）+ `docs/ARCHITECTURE.md` §8.1（`ph_` 前缀、整目录替换）。
> 更新：2026-08-08（P1C）

## 总原则

P1C 全部占位素材 = **Unity 内置几何体 + 引擎内置默认材质 + 一张开源字体**。
未引入任何第三方 3D 模型/贴图/音频，无版权不明素材，替换时整目录更换、不污染正式目录。

## 清单

| 资产 | 路径 | 类型 | 来源 | 授权 |
|---|---|---|---|---|
| 海面/沙滩/码头/船/水手/杂物 | 场景内几何体（Cube/Capsule/Sphere/Plane/Cylinder） | 占位几何体 | Unity 引擎内置 Primitive（`unity_builtin_extra`） | Unity 引擎自带，无第三方版权 |
| 渲染材质 | 场景/Prefab 引用引擎内置 Default-Material（fileID 10303） | 灰盒占位材质 | Unity 引擎内置 | 同上（后续可用 `Tools/Whalebone/Build InitialHarbor` 生成 URP 彩色占位材质） |
| 中文字体 | `Assets/Art/Placeholder/Fonts/ph_font_noto_sans_cjk_sc.otf` | OTF 字体 | [googlefonts/noto-cjk](https://github.com/googlefonts/noto-cjk) `Sans/OTF/SimplifiedChinese/NotoSansCJKsc-Regular.otf` | **SIL Open Font License 1.1**（可商用，见下方链接） |
| 船 Prefab | `Assets/Prefabs/Placeholder/ph_ship_fishing.prefab` | Prefab（几何体组合） | 本任务生成 | 自产占位 |
| 水手 Prefab | `Assets/Prefabs/Placeholder/ph_crew_male.prefab` | Prefab（几何体组合） | 本任务生成 | 自产占位 |
| 场景 | `Assets/Scenes/InitialHarbor.unity` | 场景 | 本任务生成 | 自产占位 |

## 授权文件

- Noto Sans CJK SC：OFL 1.1 全文 https://openfontlicense.org/ ｜ 项目仓库 https://github.com/googlefonts/noto-cjk
- 说明：Noto CJK 与思源黑体同源（Adobe 与 Google 合作），OFL 允许商业使用与再分发。

## 可选升级：URP 彩色占位版

仓库附带编辑器工具 `Assets/Editor/InitialHarborBuilder.cs`（菜单 `Tools > Whalebone > Build InitialHarbor`，幂等）：
运行后会把场景重建为 URP Lit 彩色材质版（海/沙/木/帆/人物分色），并生成 `Art/Placeholder/Materials/ph_mat_*.mat`。
当前提交保持引擎默认材质的灰盒版（可直接加载、零额外资产依赖）。

## 替换计划（灰盒 → 正式素材）

- D2+ 引入 Crest Ocean 后，海面占位平面替换为真实海洋渲染；
- Synty 海盗/港口包（SETUP.md §5.2 预算 $150-300）到位后，整目录替换 `Art/Placeholder/` 与 `Prefabs/Placeholder/`，场景仅改 Prefab 引用即可（先决条件：场景只引用 Prefab 实例，已满足）。
