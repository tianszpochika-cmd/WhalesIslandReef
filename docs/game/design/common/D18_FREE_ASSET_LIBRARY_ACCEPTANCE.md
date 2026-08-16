# D18-S02 免费资源库同步验收报告

状态：**BLOCKED**  
日期：2026-08-14  
验收范围：D18-S01、D18-A01—A07、D16-A04/A05 权威审计、25 个 D18 manifest、候选目录与 Unity 导入证据。

## 1. 最终结论

本次验收不能解锁 D19—D22 资源接入，原因是统一门槛中的 P0 样板闭环未完成：

1. 当前所有候选最高只有 `Candidate` 或 `ImportVerified`，`LocalDevelopmentReady` 数量为 **0**；
2. 25 个 manifest 的 `runtime_consumers` 均为空，A07 还记录了 303 条运行时消费者缺口；
3. P0 所需 Prefab、碰撞、交互锚点、LOD、动画/Animator 和消费者证据未齐；
4. 海水/天气没有可证明的兼容候选；
5. 起始船 `asset_ph_ship_fishing` 是已 integrated 且不在 A07 的 2,264 条 placeholder 输入，不能用候选船模型替代；
6. OpenGameArt Bird 只有 Blender 源文件，`NotImported`，不能计入可运行候选；
7. D16 外部门禁仍为 BLOCKED：人工历史/授权、目标设备和发布证据没有被本任务改变。

因此：**D18-S02 = BLOCKED**。不标记任何 `LocalDevelopmentReady`，不标记 `ReleaseApproved`，不接入 InitialHarbor，不解锁 D19—D22。

## 2. 读取与输入范围

已读取 D18-S01 标准、A01—A07 主成果/交接/机器结果、D16-A04/A05 当前审计、D18 候选 manifest、Unity 导入报告、D18-A07 替换映射和 Git/LFS/缓存策略。A03/A05 历史 BLOCKED 文件保留为失败记录；最终 A03/A05 交接及当前 manifest 作为当前状态依据。

权威输入：

- 25 个实际 `*.manifest.json`；
- 1,857 个 `candidate_files`；
- D16-A05 Registry placeholder 2,264 条；
- D18-A07 映射 2,264/2,264，结构审计 PASS，mapped 1,107，unmapped 1,157。

全目录扫描若把示例 manifest、辅助 JSON 和 A07 映射 JSON 混入，会得到 BLOCKED；该结果作为 A01 的负向夹具保留。本报告的正式包审计严格限定为 25 个 `*.manifest.json`。

## 3. 逐包来源、许可证、文件、哈希、状态和磁盘审计

所有 25 个实际 manifest 均通过 A01 只读 manifest 审计：25 PASS、0 error、0 issue；所有 `license_id` 为 `CC0-1.0`，本地许可证证据存在，archive SHA-256 为 64 位十六进制，候选路径均在项目边界内。下表的磁盘是 manifest 候选文件实际字节，不含 `.meta`。

| pack_id | 来源 | 状态 / 导入 | 文件 | 字节 | Unity 证据 |
|---|---|---|---:|---:|---|
| `pack_kenney_blocky_characters_vertical_slice_v1` | Kenney | Candidate / ImportVerified | 1 | 534,960 | A02：7 文件合计 |
| `pack_kenney_pirate_kit_vertical_slice_v1` | Kenney | Candidate / ImportVerified | 6 | 274,000 | A02：7 文件合计 |
| `pack_kenney_blocky_characters_d18_a04_v1` | Kenney | Candidate / ImportVerified | 1 | 534,960 | A04：24 文件合计 |
| `pack_opengameart_bird_d18_a04_v1` | OpenGameArt | Candidate / NotImported | 1 | 665,532 | Blender 源，未导入 |
| `pack_quaternius_animated_fish_d18_a04_v1` | Quaternius | Candidate / ImportVerified | 7 | 809,428 | A04：24 文件合计 |
| `pack_quaternius_animated_men_d18_a04_v1` | Quaternius | Candidate / ImportVerified | 4 | 7,934,512 | A04：24 文件合计 |
| `pack_quaternius_ultimate_animated_animals_d18_a04_v1` | Quaternius | Candidate / ImportVerified | 12 | 40,553,200 | A04：24 文件合计 |
| `pack_kenney_nature_kit_d18_a03_v1` | Kenney | ImportVerified / ImportVerified | 329 | 9,124,095 | A03：329 模型 |
| `pack_polyhaven_coral_mud_01_ao_jpg_d18_a03_v1` | Poly Haven | ImportVerified / ImportVerified | 1 | 423,543 | A03：337 文件合计 |
| `pack_polyhaven_coral_mud_01_diff_jpg_d18_a03_v1` | Poly Haven | ImportVerified / ImportVerified | 1 | 827,516 | A03：337 文件合计 |
| `pack_polyhaven_coral_mud_01_nor_gl_jpg_d18_a03_v1` | Poly Haven | ImportVerified / ImportVerified | 1 | 1,177,136 | A03：337 文件合计 |
| `pack_polyhaven_coral_mud_01_rough_jpg_d18_a03_v1` | Poly Haven | ImportVerified / ImportVerified | 1 | 515,339 | A03：337 文件合计 |
| `pack_polyhaven_sand_01_ao_jpg_d18_a03_v1` | Poly Haven | ImportVerified / ImportVerified | 1 | 298,726 | A03：337 文件合计 |
| `pack_polyhaven_sand_01_diff_jpg_d18_a03_v1` | Poly Haven | ImportVerified / ImportVerified | 1 | 485,088 | A03：337 文件合计 |
| `pack_polyhaven_sand_01_nor_gl_jpg_d18_a03_v1` | Poly Haven | ImportVerified / ImportVerified | 1 | 877,821 | A03：337 文件合计 |
| `pack_polyhaven_sand_01_rough_jpg_d18_a03_v1` | Poly Haven | ImportVerified / ImportVerified | 1 | 189,667 | A03：337 文件合计 |
| `pack_kenney_building_kit_ships_port_props_v1` | Kenney | Candidate / ImportVerified | 79 | 2,549,104 | A05：151 模型合计 |
| `pack_kenney_pirate_kit_ships_port_props_v1` | Kenney | Candidate / ImportVerified | 72 | 3,147,136 | A05：151 模型合计 |
| `pack_kenney_impact_sounds_v1` | Kenney | ImportVerified / ImportVerified | 10 | 61,865 | A06：1,337 文件合计 |
| `pack_kenney_input_prompts_v15` | Kenney | ImportVerified / ImportVerified | 356 | 139,529 | A06：1,337 文件合计 |
| `pack_kenney_interface_sounds_v1` | Kenney | ImportVerified / ImportVerified | 10 | 55,423 | A06：1,337 文件合计 |
| `pack_kenney_ui_audio_v1` | Kenney | ImportVerified / ImportVerified | 10 | 48,481 | A06：1,337 文件合计 |
| `pack_kenney_ui_pack_v2` | Kenney | ImportVerified / ImportVerified | 868 | 643,497 | A06：1,337 文件合计 |
| `pack_polyhaven_hdri_sky_references_v1` | Poly Haven | ImportVerified / ImportVerified | 4 | 6,075,667 | A06：1,337 文件合计 |
| `pack_polyhaven_ui_materials_selected_v1` | Poly Haven | ImportVerified / ImportVerified | 79 | 52,882,146 | A06：1,337 文件合计 |

汇总：manifest 候选文件 1,857，实际候选文件 1,857，manifest 候选字节 **130,828,371**。候选根目录含 `.meta`、许可证和预览共 4,134 文件、**136,473,038 bytes**。

许可证和哈希结论：证据链完整到来源/许可证/manifest/archive SHA-256/文件存在；并不等于 Prefab、运行时、性能或人工发布批准。A04 的 23 个独立模型身份、A05 的 151 模型、A06 的逐项 Poly Haven 哈希和 A03 的 337 文件导入记录均保留原始证据。

## 4. Unity 导入、测试和状态转换

| 证据 | 实际结果 | 判定 |
|---|---|---|
| A01 逐包 manifest 审计 | 25/25 PASS，0 errors，0 issues | PASS |
| A02 Kenney vertical slice | 7 文件、15 Mesh、27 clips、7 materials、0 errors、7 warnings | PASS，保留 warning |
| A02 EditMode | 6/6 passed，0 failed | PASS |
| A03 环境 | 337 文件、329 模型、8 纹理、0 errors、235 warnings | PASS，保留 warning |
| A04 角色/动物 | 24 FBX、30 Mesh、228 clips、121 materials、0 errors、25 warnings | PASS，保留 warning |
| A05 船/建筑/道具 | 151 模型、231 Mesh、27 clips、161 materials、0 errors、151 warnings | PASS，保留 warning |
| A06 UI/材质/音频 | 1,337 文件、1,307 textures、30 audio、0 errors、37 warnings | PASS，保留 warning |

warning 没有被删除或静默升级：A03/A04/A05 主要是原始命名约定；A06 包含短音频 Vorbis 和状态提示。A04 Bird 仍为 `NotImported`，不能计入 Unity 可运行样本。

状态机静态核对：9 个 manifest 为 `Candidate`，16 个为 `ImportVerified`；24 个 `ImportVerified`，1 个 `NotImported`；`LocalDevelopmentReady=0`，`ReleaseApproved=0`。没有检测到非法状态值、Candidate 直接 ReleaseApproved 或未批准 `runtime_consumers`。历史转换签名不是所有 manifest 都可由当前文件独立证明，因此不把当前状态解释为人工授权完成。

## 5. 九类抽样结果

抽样文件：[`D18_S02_SAMPLING_RESULTS.json`](../../../../game/tools/results/D18/D18-S02/D18_S02_SAMPLING_RESULTS.json)。按固定日期种子抽取环境、角色、动物、鱼、船、建筑、道具、UI、材质、音频各 3 项，共 **30 项**；目录扫描和文件存在检查 30/30 PASS。

| 类别 | 抽样 | Unity 导入证据 | 保存/交互/行为接管 |
|---|---:|---|---|
| 环境 | 3 | A03 PASS | 未运行：无消费者 |
| 角色 | 3 | A04 PASS | 未运行：无消费者 |
| 动物 | 3 | A04 PASS | 未运行：无消费者 |
| 鱼 | 3 | A04 PASS | 未运行：无消费者 |
| 船 | 3 | A05 PASS | 未运行：无船 Prefab/消费者 |
| 建筑 | 3 | A05 PASS | 未运行：无 Prefab/锚点 |
| 道具 | 3 | A05 PASS | 未运行：无交互入口 |
| UI | 3 | A06 PASS | 未运行：未接入 Canvas |
| 材质 | 3 | A03/A06 PASS | 未运行：未接入正式材质 |
| 音频 | 3 | A06 PASS | 未运行：未接入 Mixer |

保存、交互入口和行为接管不能在不接入场景/Prefab/运行时消费者的条件下伪造 PASS；本次明确记录为 `NOT_RUN_NO_RUNTIME_CONSUMER`。这正是 P0 闭环阻断项，而不是删除资源或偷换验收口径。

## 6. 重复、磁盘、仓库增量和 LFS

- 候选 payload 扫描：2,009 个 `.fbx/.blend/.obj/.png/.jpg/.ogg/.hdr`，131,100,653 bytes；30 个重复哈希组、120 个重复文件记录；
- 模型重复：7 个 hash 组、14 个 FBX，主要是 D16/A03/A05 垂直切片与扩展候选的重复保留：角色、棕榈、沙地、岩石、船、小码头；不删除，后续以 manifest/映射选择唯一入口；
- 候选目录无 zip/7z/rar/tar/gz/exe；外部缓存有 2 个归档、4,753,570 bytes；
- Git LFS 本机 `3.5.1` 已安装，endpoint 配置存在，但远程配额、上传权限和恢复流程未确认；当前无 `.gitattributes`；已有 7 个 D16 vertical FBX 由 Git LFS 跟踪，新增 A03—A06 候选仍是工作区未跟踪资源；
- `.gitignore` 仅保留外部压缩包缓存规则，没有把数百 MB 候选静默加入普通 Git 历史；本任务不迁移历史、不创建 LFS 规则、不提交或推送。

仓库策略判定：本地开发可保留候选目录，进入普通 Git 历史前必须确认远程 LFS 支持/配额，并按 D18 单包/章节预算执行；当前不满足发布资源库门槛。

## 7. P0 与 D19—D22 入口

| P0 要求 | 当前证据 | 结果 |
|---|---|---|
| 玩家 | A07 控制点复用 Blocky Character；无玩家 placeholder ID、无消费者 | BLOCKED |
| 关键 NPC | Quaternius Male Shirt，ImportVerified/Candidate | BLOCKED：无 LocalDevelopmentReady/Prefab |
| 圣玛格丽特地形 | Kenney Nature Kit、Sand/Coral Mud、vertical island 组合 | BLOCKED：无地形 Prefab/碰撞/LOD |
| 码头/建筑 | vertical dock/pier + Building Kit | BLOCKED：无锚点/碰撞/消费者 |
| 起始船 | `asset_ph_ship_fishing` 已 integrated，候选船不自动替换 | BLOCKED：无候选接入授权 |
| 海水/天气 | 当前 A03—A06 无兼容运行时候选 | BLOCKED |
| 核心 UI | Input Prompts/UI Pack ImportVerified 控制点 | BLOCKED：无 Canvas 消费者 |

[`d18_local_development_ready_catalog.json`](assets/d18_local_development_ready_catalog.json) 为空 ready catalog，保留 10 类 Pending 入口和阻断原因。D19—D22 在 S02 BLOCKED 时不得绕过该清单直接使用新下载文件。

## 8. D16 外部状态和责任任务

D16-A04 当前 267 条人工历史/授权 Pending，D16-A05 当前 2,264 placeholder；D16-S01/S02 和目标设备/外部发布门禁仍为 BLOCKED/未关闭。本次只读核对没有修改这些状态，也没有创建人工 ReleaseApproved。

解锁顺序：

1. D19：建立 P0 岛屿/码头/建筑 Prefab、碰撞、锚点、LOD 和消费者；
2. D20：建立关键 NPC/动物 Prefab、Animator、碰撞、LOD 与时代审查；
3. D21：处理船海消费者、海面/天气候选缺口，保留起始船实体 ID；
4. D22：接入 UI/音频/材质消费者、Canvas/Mixer/性能与可访问性入口；
5. D24：重新跑全量资源、运行时、性能、授权和人类 ReleaseApproved 门禁。

## 9. 验证命令

```powershell
python tools/d18_asset_manifest_audit.py --project-root . --manifest <each of 25 *.manifest.json> --output game/tools/results/D18/D18-S02/manifest_audits/<name>.json
python -c "...read-only candidate path/hash/duplicate/LFS checks..."
python -c "...read-only D18-A07 mapping audit and digest check..."
git lfs version
git lfs env
git diff --check
```

最终 `git diff --check` 通过；Git 仅提示既有工作区的行尾转换警告，没有 whitespace error。Unity、manifest、路径、许可证和抽样结果均保留在结果目录；本报告不替代任何前置交接。

