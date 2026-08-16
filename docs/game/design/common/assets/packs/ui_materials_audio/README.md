# D18-A06 UI、材质与音频候选包

本目录只保存 D18-A06 的来源证据、机器 manifest 和候选包说明。候选资源不会自动进入正式 Canvas、场景、EventSystem、输入映射或运行时构建，也不代表 `ReleaseApproved`。

## 官方来源

- Kenney UI Pack：<https://kenney.nl/assets/ui-pack>
- Kenney Input Prompts：<https://kenney.nl/assets/input-prompts>
- Kenney Interface Sounds：<https://kenney.nl/assets/interface-sounds>
- Kenney UI Audio：<https://kenney.nl/assets/ui-audio>
- Kenney Impact Sounds：<https://kenney.nl/assets/impact-sounds>
- Poly Haven License：<https://polyhaven.com/license>
- Poly Haven selected asset pages are listed in `polyhaven_selected_assets.json`.
- Existing project font evidence: `game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md`.

## 机器入口

- `*_v*.manifest.json`：D18-A01 manifest schema v1，状态由证据驱动，禁止人工批准字段。
- `polyhaven_selected_assets.json`：每一个材质/HDRI 单项的官方页面、实际文件、哈希和尺寸。
- `font_license_audit.json`：现有 Noto Sans CJK SC 的 OFL 证据；本任务不复制或覆盖正式字体文件。

下载归档保存在仓库外的 `C:\Users\Administrator\Desktop\d18-a06-downloads`，仓库只保存候选切片、许可证证据和哈希。Poly Haven 只下载明确列出的 1K 单项贴图/HDRI，不抓取整站。
