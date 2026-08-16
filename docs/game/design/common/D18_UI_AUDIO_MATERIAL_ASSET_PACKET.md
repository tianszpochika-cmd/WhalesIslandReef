# D18-A06 UI、材质、HDRI 与音频资产包

- 任务：`D18-A06`
- 核对日期：`2026-08-14`
- 结论：**PASS（候选与 Unity 导入预检通过；未接入正式运行时）**
- 资源状态：manifest 为 `ImportVerified`；没有任何 `ReleaseApproved` 或正式消费者。

## 1. 范围与边界

本批只写入 `game/Assets/Art/Candidates/UIMaterialsAudio/`、本资产包文档、资源包证据目录和 D18-A06 结果/交接。没有修改正式 Canvas、EventSystem、InputAction/Input Map、Systems、Registry、场景、Prefab、构建设置或既有字体二进制。音频只作为候选文件和试听数据，不能修改游戏状态；材质和 HDRI 只作为导入预检候选。

## 2. 官方包与许可

| 包 | 官方入口 | 许可 | 下载归档 SHA-256 | 本地候选/导入数量 |
|---|---|---|---|---:|
| Kenney UI Pack 2.0 | [official page](https://kenney.nl/assets/ui-pack) | CC0-1.0；包内 `License.txt` | `A8A14A234911EB648C062622915C93E79E94E97CB7F9F375A70F6617F1174318` | 868 PNG |
| Kenney Input Prompts 1.5A | [official page](https://kenney.nl/assets/input-prompts) | CC0-1.0；包内 `License.txt` | `AC2FCF599080B0F3BA2D174C9474DB6DF1A0E96FF0662580E2DA79A122AB78A1` | 356 PNG |
| Kenney Interface Sounds 1.0 | [official page](https://kenney.nl/assets/interface-sounds) | CC0-1.0；包内 `License.txt` | `F2193D072726D6758A5F7871B2DCC54DCCE0D5C35C6F0A62F92549B327C81232` | 10 OGG |
| Kenney UI Audio | [official page](https://kenney.nl/assets/ui-audio) | CC0-1.0；包内 `License.txt` | `946FC23A63D535D693EB31B2EABB80C8C28D6351E2186B344CEB71B2CB1D5EB6` | 10 OGG |
| Kenney Impact Sounds 1.0 | [official page](https://kenney.nl/assets/impact-sounds) | CC0-1.0；包内 `License.txt` | `029D734AF1582474EDF3A694D1B0CEBC97C1C152F2F39FA34D4C2BAFC5DE77F8` | 10 OGG |
| Poly Haven selected materials | [license](https://polyhaven.com/license)；逐项页面见 selected catalog | CC0；单项页面/API 与本地哈希逐条保留 | canonical digest 见 `polyhaven_ui_materials_selected_v1.manifest.json` | 20 material IDs / 79 JPG |
| Poly Haven HDRI references | [license](https://polyhaven.com/license)；逐项页面见 selected catalog | CC0；单项页面/API 与本地哈希逐条保留 | canonical digest 见 `polyhaven_hdri_sky_references_v1.manifest.json` | 4 HDR |

Kenney 许可证原文和 Poly Haven 许可证据快照分别位于 `kenney_cc0_evidence_2026-08-14.txt` 与 `polyhaven_cc0_evidence_2026-08-14.txt`。归档保留在仓库外 `C:\Users\Administrator\Desktop\d18-a06-downloads`，没有把原始压缩包加入 Git。

## 3. UI 与输入候选

实际复制并预检：

- UI Pack：868 个 PNG，来自 UI Pack 的 Blue/Green/Grey/Red/Yellow/Extra 视觉切片；用于面板边框、按钮、滑条、箭头、勾选、分页和导航反馈。
- Input Prompts：356 个 PNG，其中 Keyboard & Mouse 与 Xbox Series Default 均有按键、方向、肩键、扳机、摇杆和菜单提示；Xbox 资产只作为通用手柄视觉提示，不替换当前输入绑定。
- 文件级总数 1,224；重复哈希审计保留了 23 个重复哈希组，去重后 UI/输入仍有 1,144 个独立文件内容，满足候选数量门槛。

建议映射：键鼠提示使用 `KeyboardMouse`，Xbox 风格提示使用 `XboxSeries`；D16 无障碍设置仍是输入和可访问性的唯一入口，资源包不写偏好或绑定。

## 4. 港口 PBR 材质

实际下载 20 个 Poly Haven 单项，每项只取 1K JPG。选集包含木材（`wood_floor_deck`、`wooden_planks`、`fine_grained_wood`、`rough_wood`、`brown_planks_03`、`bark_brown_01`）、沙地/泥地/水岸（`coast_sand_01`—`04`、`brown_mud`、`brown_mud_dry`）、岩石/碎石（`rock_face`、`aerial_rocks_01`、`aerial_grass_rock`、`clean_pebbles`）、布/皮革（`fabric_pattern_05`、`brown_leather`）和金属（`metal_plate`、`blue_metal_plate`）。完整逐文件 URL、哈希、大小、分辨率和 map 组成见 `polyhaven_selected_assets.json`。

Unity 预检记录每张 JPG 的尺寸、通道和 TextureImporter 设置；Unity 报告中实际导入纹理使用 `Default`、最大尺寸 2048、Compressed、Mipmaps 开启。没有自动建 Material、没有改正式材质，也没有把 AO 缺失的单项伪装为完整四图集。

## 5. HDRI / 天空参考

| 参考 | 用途建议 | 时代/场景注意 |
|---|---|---|
| `green_point_park` | 加勒比晴天/上午的光照色彩参考 | 远景含现代城市元素，只可取天空/光色，不直接作为港口背景 |
| `kloppenheim_06` | 晴间偏云、海面高光参考 | 山地背景与加勒比港口不完全匹配，使用天空色与光照方向即可 |
| `joburg_central_sunset` | 黄昏/低太阳角度参考 | 城市/工业远景不进入游戏背景 |
| `venice_sunset` | 黄昏暖色、夜间前过渡参考 | 建筑/水岸背景不直接使用，只取天空和色温策略 |

四个文件均为 1K equirectangular HDR，Unity 导入对象为 1024×512 Texture2D。后续 Unity 集成应在独立天空/光照测试场景中把它们作为参考或可替换天空纹理，分别测试晴/阴/黄昏/夜间曝光；D18-A06 不赋值 Skybox、Volume 或正式场景。

## 6. 音频候选

30 个音频逐文件复制、哈希和解码审计，来源只来自上述三个 Kenney CC0 包：Interface Sounds 10、UI Audio 10、Impact Sounds 10。每个文件都能回溯到包内 `License.txt`，没有按混杂站点整体推定许可。

`D18_A06_AUDIO_PREVIEW.md` 记录每个 OGG 的通道、采样率、时长、RMS/Peak dBFS、压缩与循环策略。实际样本为 2 通道、44.1 kHz，Unity 预检为 Vorbis / DecompressOnLoad / preload。所有候选目前作为一次性 UI/交互/脚步/撞击片段，loop=false，循环点为空；环境底噪、海浪、风暴、港口人声和音乐仍需专门的逐项 CC0 取证，未被本任务伪造为已完成。

30 个短片段保留 Unity `short_audio_vorbis` 非阻塞警告，最终使用前需要听感、瞬态、混音总线和无障碍字幕/图标复核。本批没有归一化、裁剪或修改音频逻辑。

## 7. 字体审计

现有 `game/Assets/Resources/Fonts/ph_font_noto_sans_cjk_sc.otf` 已由 `game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md` 记录为 Noto Sans CJK SC、SIL OFL 1.1，并有 Google Noto CJK 来源链接，符合商业嵌入候选政策。本任务没有复制、替换或覆盖正式字体和 GUID；审计记录见 `font_license_audit.json`。Kenney UI Pack 随包字体没有进入候选，因为本任务未建立该字体的独立 OFL/商业嵌入证明。

## 8. 验证结果

- manifest 审计：7 个 D18-A01 manifest，0 errors，PASS。
- Unity `6000.0.80f1` report-only 导入：1,337 个候选文件，1,307 TextureImporter、30 AudioImporter，0 errors，PASS；37 条非阻塞警告（30 条短音频瞬态提示、7 条 manifest 状态报告提示）；结果见 `D18_A06_UNITY_IMPORT_AUDIT.json`，逐项原始结果见 `D18_A02_KENNEY_IMPORT_AUDIT.json`。
- 静态纹理/音频审计：纹理尺寸/通道、音频解码/响度、重复 SHA-256、20 套材质组成均有记录；结果见 `D18_A06_STATIC_AUDIT.json`。
- 视觉联系表：`D18_A06_UI_MATERIAL_CONTACT_SHEET.png`，1920×1680 PNG。
- 音频试听清单：`D18_A06_AUDIO_PREVIEW.md`。
- `git diff --check`：PASS。

本包可供后续 D18 本地候选挑选，但不等于运行时集成、目标设备无障碍、最终时代审查或 Steam/ReleaseApproved。
