# 【提示词 P1C-Pre】占位素材准备（纯文件操作，可在 Unity 下载期间执行）

## 角色与任务
你是《鲸骨航线》的素材整理工程师。任务：在 Unity 尚未安装时，下载并归档初始环境所需的 **CC0 占位素材**（灰盒开发第一阶段）。

## 必读
1. `docs/AGENTS.md`（铁律）
2. `docs/game/SETUP.md` 第 4 节（占位素材纪律：只用 CC0/CC-BY；`ph_` 前缀；授权登记）
3. `docs/ARCHITECTURE.md` 8.1（资源位置规范）

## 工作目录
- 目标：`game/Assets/Art/Placeholder/`（按子目录归档）
- 备案：`assets/placeholder/`（仓库级共享副本，git-lfs 后续处理）

## 任务清单
1. **下载 CC0 素材**（来源仅限 kenney.nl / polyhaven.com，其他站点先回报统筹者）：
   - **Kenney "Ship Builder Kit"**（CC0：船只零件/帆/船身，最贴合航海占位）
   - **Kenney "Prototype Kit"**（CC0：低模人形/基础几何体——老水手/玩家占位）
   - **Kenney "Nature Kit"**（CC0：树/岩石/草地——岛屿环境占位）
   - 如个别包下载失败，用同站其他 CC0 包替代并记录
2. **归档整理**：
   - `game/Assets/Art/Placeholder/Models/`（船/人形/环境模型）
   - `game/Assets/Art/Placeholder/Textures/`（若包内含贴图）
   - `game/Assets/Art/Placeholder/Materials/`（若包内含材质）
   - 命名：`ph_` 前缀（如 `ph_ship_fishing.fbx`、`ph_crew_male.fbx`）——**不要改名包内原始资源名**，只需在 README 中列映射
3. **授权登记**：更新 `assets/placeholder/README.md`（或新建）：每个包的来源 URL、授权类型（CC0）、下载日期、用途标注（哪些用于港口/船/人物占位）。
4. **清单交付**：输出一份 `placeholder_manifest.md`（放 `game/Assets/Art/Placeholder/`）：文件名 ↔ 用途 ↔ 来源对照，供 P1C（场景搭建）直接引用。

## 验收（DoD）
- [ ] 素材全部来自 CC0 授权源；授权登记表完整（URL/日期/用途）
- [ ] 目录结构符合 ARCHITECTURE 8.1；README/清单齐全
- [ ] 未放置任何非 CC0/版权不明素材

## 输出与留痕
`git commit -m "chore: D1-P1C-Pre 占位素材归档 (CC0)"`；回报：已下载包清单 + 素材清单路径 + 总大小。
