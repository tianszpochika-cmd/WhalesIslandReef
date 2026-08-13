# D16-A05 候选资源包 01：Kenney CC0 垂直切片资源

日期：2026-08-13  
状态：`CANDIDATE_PENDING_REVIEW`

> 本包是第一批真实资源候选，不是最终资源接入批准。它没有修改 Content Registry、`asset_state`、Prefab、场景引用或运行时逻辑。正式接入前仍需完成 A04 内容/表现审核、A05 资源人工审核、LOD/碰撞/锚点配置和 Unity 回归。

## 来源与许可证

来源页面：

- [Kenney Pirate Kit](https://kenney.nl/assets/pirate-kit)：页面标注 3D、船/岛/热带/角色资源，许可证为 Creative Commons CC0。
- [Kenney Blocky Characters](https://kenney.nl/assets/blocky-characters)：页面标注 3D 角色和动画资源，许可证为 Creative Commons CC0。

原始下载包已保存到 `artifacts/external_sources/`：

| 原始包 | SHA-256 | 许可证结论 |
|---|---|---|
| `kenney_pirate-kit.zip` | `667ED2CAF92954DDB98F7B7CEDE831FE99AB75063C26B25E23D32715BEE9C943` | License.txt 明确 CC0；可个人、教育和商业使用，可修改 |
| `kenney_blocky-characters_20.zip` | `5E123859AA0C1598342B600C6DB197024A1D63EB9EC531398B310725F589887E` | License.txt 明确 CC0；可个人、教育和商业使用，可修改 |

## 已保存的候选文件

候选文件位于 `game/Assets/Art/Candidates/Kenney/`，目前不被 Registry 读取：

| 垂直切片目标 | 候选文件 | 当前处理 |
|---|---|---|
| `asset_ph_crew_male` | `BlockyCharacters/Models/character-a.fbx` | 候选真人角色；需人工确认风格、骨骼/动画和历史表现适配 |
| `building_saint_marys_dock` | `PirateKit/Models/structure-platform-dock-small.fbx` | 候选码头；需配置碰撞、入口/服务锚点和港口材质 |
| `building_small_boat_pier` | `PirateKit/Models/structure-platform-small.fbx` | 候选小艇泊位；需配置靠泊碰撞和上下船锚点 |
| `island_saint_marys` | `PirateKit/Models/patch-sand.fbx`、`rocks-sand-a.fbx`、`palm-detailed-straight.fbx` | 候选沙地/岩石/棕榈组合；不是完整岛屿最终场景 |
| 垂直切片船舶候选 | `PirateKit/Models/ship-pirate-small.fbx` | 仅候选展示资源；不得覆盖已集成船或改变船控所有权 |

## 仍需完成的接入条件

1. 责任审核者确认资源来源、商业使用和修改许可，并在 A05 替换包中提供真实审核决定；
2. 为每个目标条目补齐最终资源路径、运行时 Prefab/消费者、LOD、碰撞、锚点、材质和测试用例；
3. Unity 导入后确认 FBX 尺寸、朝向、材质、阴影、碰撞层和玩家/船舶交互；
4. 通过 `tools/d16_a05_replacement_preflight.py` 后，才允许串行写入 Registry 并执行 Unity PlayMode 回归；
5. 在 A04 267 条 Pending 未由责任审核者逐条决定前，不能把本候选包写成历史/表现审核已通过。

## 当前验证

- 两个官方下载包及其 `License.txt` 已存在；候选 FBX 文件均已保存并有本地哈希。
- 预览图已人工目视抽查：角色、码头、小艇平台和小型船模型可读；它们仍是候选低多边形资源，不能直接宣称最终品质。
- 当前 Content Registry 未被本包修改；A05 仍保持 2,264 个 placeholder。

## Unity 导入级复核

已使用当前 Unity 6 工程的只读审计入口 `D16CandidateAssetAudit.Run`，结果记录在 `tools/results/d16_a05_candidate_import_current_20260813.json`：

- 7/7 候选 FBX 具备 `ModelImporter`、主模型和正的 Mesh bounds；专项测试 **4/4 PASS**（候选包 2/2、Unity 导入证据 2/2）。
- 角色 FBX 导入出 6 个 Mesh、27 个 Generic 动画片段；码头/泊位/沙地/岩石/棕榈/小船均有可读 Mesh。当前导入结果全部为 `globalScale=1`、`isReadable=false`，这只是导入事实，不是最终尺寸、碰撞或动画质量结论。
- 审计期间未修改导入器设置、Registry、Prefab、场景引用或 `asset_state`；Unity 批处理日志无脚本编译错误。

该结果只证明候选文件能被当前 Unity 工程读取。正式接入前仍需责任审核、角色尺寸/朝向适配、材质、LOD、碰撞体、交互锚点、运行时 Prefab/消费者和 PlayMode 表现验收。

## 候选导入后的 Unity 回归

- 使用候选导入审计后的当前工程重新执行 PlayMode：`game/tools/results/d17_playmode_full_after_candidate_import_20260813.xml`。
- 结果为 **60/60 PASS**、失败 0、跳过 0；日志中的 D17-S02 本机十轮循环为 **10/10 passed**。
- 该回归证明本机脚本编译、候选只读导入和现有运行时闭环没有发生回退；它不等于候选资源已经接入，也不替代人工审核、最终资源表现、目标设备或外部发布验收。
