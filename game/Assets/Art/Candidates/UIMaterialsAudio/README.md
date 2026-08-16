# D18-A06 UI / 输入 / 材质 / HDRI / 音频候选

本目录是候选资源区，不是正式资源库。所有文件均保持 D18-A06 的 `ImportVerified` 证据状态，未写入 Canvas、EventSystem、InputAction、场景、Registry、Systems 或构建设置，也未做 `ReleaseApproved`。

## 当前真实数量

- UI Pack PNG：868；Input Prompts PNG：356（Keyboard & Mouse 与 Xbox Series Default）。文件级总数 1,224，去重哈希后 1,144 个独立文件内容。
- Poly Haven PBR：20 个独立材质 ID、79 张 1K JPG map；每套至少有 albedo/normal/roughness，AO 按官方单项实际发布情况记录。
- Poly Haven HDRI：4 个 1K HDR 文件；其中 3 个独立文件哈希，重复关系保留在静态审计中。
- Kenney 音频：30 个 OGG（Interface Sounds、UI Audio、Impact Sounds 各 10 个）；28 个独立文件哈希。

## 证据与状态

- 包内 `License.txt` 随 Kenney 候选保存；Poly Haven 使用官方 CC0 页面快照和逐项页面/API URL。
- 完整 manifest、单项来源、文件 SHA-256、分辨率和来源映射见 `docs/game/design/common/assets/packs/ui_materials_audio/`。
- 候选不是运行时接入。后续任务必须重新核对时代/画面/混音/无障碍和目标设备证据。
