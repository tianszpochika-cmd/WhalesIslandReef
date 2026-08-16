# D18-A03 环境候选资源包

> 任务：D18-A03（样板岛环境资源）  
> 核对日期：2026-08-14  
> 当前结论：**PASS（候选包交付 / ImportVerified）**

本任务交付的是可追溯、可导入预检的环境候选包，不是正式场景替换，也不是发布授权。资源没有接入 InitialHarbor，没有写入 Registry、Prefab、场景或运行时系统；`ReleaseApproved` 仍为 false，必须由责任人类审核者另行确认。

## 1. 前置和交付边界

| 前置 | 机器结果 | 证据 |
|---|---|---|
| D18-S01 | PASS | `docs/game/design/common/D18_VISUAL_ART_DIRECTION_AND_BUDGET.md` 与 D18-S01 交接 |
| D18-A01 | PASS | `game/tools/results/D18/D18-A01/D18_A01_RESULT.json` |
| D18-A02 | PASS | `game/tools/results/D18/D18-A02/D18_A02_RESULT.json`、6/6 EditMode 测试 |
| A03 manifest 审计 | PASS，9 个 manifest，0 错误 | `game/tools/results/D18/D18-A03/D18_A03_MANIFEST_AUDIT.json` |
| A03 缺失引用/边界检查 | PASS，337 个文件，0 缺引用，0 运行时消费者 | `game/tools/results/D18/D18-A03/D18_A03_MISSING_REFERENCE_CHECK.json` |

允许写入的目录只有：

- `game/Assets/Art/Candidates/Environment/`
- `docs/game/design/common/assets/packs/environment/`
- `game/tools/results/D18/D18-A03/`
- 本报告、D18-A03 交接和当天记忆日志。

未修改 Systems、contracts、Registry、InitialHarbor 场景、正式 Prefab、构建设置或玩法逻辑。

## 2. 官方来源与许可证据

所有来源在 2026-08-14 以官方页面核对。网页链接只是来源定位；本地许可证证据和原始包内许可证同时保留。

| 包/资产 | 官方页 | 下载形式 | 许可 | 实际文件/哈希 |
|---|---|---|---|---|
| Kenney Nature Kit | [官方资产页](https://kenney.nl/assets/nature-kit) | 官方直链 `kenney_nature-kit.zip`，只提取 FBX | CC0；包内 `License.txt` 允许个人、教育、商业使用与修改 | 329 个 FBX；归档 SHA-256 `FA7974A0D342BFE63C38664BA9F8EC1A4AAB8EA25F099BDC56870E33588C4D9D`，10,537,521 bytes |
| Poly Haven Sand 01 | [官方资产页](https://polyhaven.com/a/sand_01) / [官方许可页](https://polyhaven.com/license) | 4 个官方 1K JPG 直链 | CC0 | 见下表 |
| Poly Haven Coral Mud 01 | [官方资产页](https://polyhaven.com/a/coral_mud_01) / [官方许可页](https://polyhaven.com/license) | 4 个官方 1K JPG 直链 | CC0 | 见下表 |

Poly Haven 的本地证据文件为 [`polyhaven_license_evidence_2026-08-14.md`](assets/packs/environment/licenses/polyhaven_license_evidence_2026-08-14.md)。它记录了 CC0 边界：资产可商用、修改和再分发；站点 Logo、缩略图、文字和元数据不随资产许可，本任务没有复制这些站点内容。

### Poly Haven 实际下载文件

| 文件 | 官方直链 | SHA-256 | 用途 |
|---|---|---|---|
| `sand_01_diff_1k.jpg` | `https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/sand_01/sand_01_diff_1k.jpg` | `A73B90EAA6D675EF57E7D48A8CF23E1F412E353AB2E29CF73930B8ECD4E76875` | 沙滩颜色 |
| `sand_01_nor_gl_1k.jpg` | `https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/sand_01/sand_01_nor_gl_1k.jpg` | `73984AECDC35B1FFBC322D64B7F8D8165B06970D11F37DD47010609FCB532704` | 沙滩法线 |
| `sand_01_rough_1k.jpg` | `https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/sand_01/sand_01_rough_1k.jpg` | `CCB179B12C56A9032D60E4F4E2DA690B3612A58E1784EB14576372690CDCD1CB` | 沙滩粗糙度 |
| `sand_01_ao_1k.jpg` | `https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/sand_01/sand_01_ao_1k.jpg` | `BE5FED35A2B259088682241CE4CF1EB0EE5D36B820F520E3F5E7F353BD1F627A` | 沙滩 AO |
| `coral_mud_01_diff_1k.jpg` | `https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/coral_mud_01/coral_mud_01_diff_1k.jpg` | `42FDAEA6FD0263678F70332ADD3C109FDD54E4C32B61FC98CC054D4F1A9DEAD1` | 暴露海床颜色 |
| `coral_mud_01_nor_gl_1k.jpg` | `https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/coral_mud_01/coral_mud_01_nor_gl_1k.jpg` | `D19D8AC5CAB1B4C524E51EA91DBEB12DC3D986772B5D280D434096228E5DDA38` | 海床法线 |
| `coral_mud_01_rough_1k.jpg` | `https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/coral_mud_01/coral_mud_01_rough_1k.jpg` | `CD841F419432F31253AED9FD12E775041A961CFB01AF59BF3BAED5E4459D035A` | 海床粗糙度 |
| `coral_mud_01_ao_1k.jpg` | `https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/coral_mud_01/coral_mud_01_ao_1k.jpg` | `FB0C6B07FEE149527D553F83D683855A9E1DDEBEC94FD80AB8FC9B878FD62B07` | 海床 AO |

机器清单位于 [`docs/game/design/common/assets/packs/environment/`](assets/packs/environment/)。Kenney 包只保留 FBX 格式和 `License.txt`；没有把同一模型的 OBJ、DAE、GLB、STL、预览图和示例工程重复放入仓库。Poly Haven 只下载上述 8 个明确需要的 1K JPG，没有抓取整站或保留 8K 归档。

## 3. 数量和样板覆盖

| 指标 | 实际值 | 证据 |
|---|---:|---|
| Kenney 可复用模型候选 | 329 FBX | `D18_A03_CANDIDATE_CATALOG.json` |
| Poly Haven 材质图候选 | 8 JPG | 同上 |
| 候选总数 | 337 | 同上 |
| Unity 导入审计文件数 | 337 | `D18_A03_UNITY_IMPORT_AUDIT.json` |
| Unity 导入样板抽样 | 50 模型 | `D18_A03_IMPORT_MATRIX.json` |
| 联系表 | 5 列 × 10 行 | `game/tools/results/D18/D18-A03/kenney_nature_kit_contact_sheet.png` |

50 个样板不是复制变体，而是从真实 FBX 中确定性选择。抽样覆盖：棕榈（`tree_palm`）、树/阔叶候选（`tree`/`oak`）、灌木、草、岩石、悬崖、沙滩平台、土路、木材、石材和水边/河道模块。Kenney Nature Kit 的命名与原始包保持一致，便于来源追溯；稳定目标 ID 在 manifest 中登记，不把原始文件名直接当作跨系统内容 ID。

浅水/海床边界如下：Poly Haven Coral Mud 01 是暴露海床材质候选，满足海床材质样板；本任务没有伪造水面 Shader 或浅水运行时材质，水面表现仍由水体系统和后续 Prefab/材质责任人决定。

## 4. Unity 导入预检

导入使用隔离的临时 Unity 6000.0.80f1 项目，复用 D18-A02 的只读审计器；没有打开或改写主项目场景。审计报告已复制到 A03 结果目录，作为真实导入证据。

| 指标 | 结果 |
|---|---:|
| 状态 | PASS |
| FBX / JPG | 329 / 8 |
| Unity mesh | 338 |
| Unity material | 688 |
| animation clip | 0 |
| 导入错误 | 0 |
| 导入警告 | 235 |
| missing script / missing reference | 0 / 0 |
| collider / LODGroup / interaction anchor | 0 / 0 / 0 |
| 纹理导入尺寸 | 8 张均为 1024 × 1024，未调整源文件 |

235 条警告可解释为：226 个 Kenney 原始文件名不符合项目蛇形命名规则，另有导入快照中 9 条 manifest 状态提示。原始文件名不能在来源包阶段改写；目标 ID 已稳定化，后续 Prefab/正式目录阶段由责任 Agent 决定显示名。最终 A03 manifest 审计已经是 0 issue。

碰撞、LOD 和交互锚点为 0 是有意的边界，不是把缺项标成完成：本任务只交付原始候选和导入测量，不创建 Prefab，不添加碰撞体、LODGroup 或交互点。它们必须在 D19/后续 Prefab 责任任务中逐项验证。纹理没有 3D bounds 属于类型预期，不是丢失引用。

导入矩阵记录每个文件的 importer、主对象类型、mesh/material/texture 数量、bounds、Unity 单位/轴向设置、命名状态、缺引用、碰撞、LOD 和锚点状态，见 [`D18_A03_IMPORT_MATRIX.json`](../../../../game/tools/results/D18/D18-A03/D18_A03_IMPORT_MATRIX.json)。

## 5. 资源状态和运行时边界

| 状态 | 本任务结果 |
|---|---|
| Candidate | 下载、哈希、许可证和目录登记的起点 |
| LicenseEvidenceComplete | Kenney 包内许可证和 Poly Haven 本地许可证证据完整 |
| ImportVerified | 当前 9 个 manifest 已进入该状态；Unity 预检和 manifest 审计通过 |
| LocalDevelopmentReady | 未进入；尚无 Prefab、碰撞、LOD、锚点或消费者接入 |
| ReleaseApproved | 未进入；禁止 Agent 代签，只能由有责任的人类审核者确认 |
| Rejected | 未进入 |

所有 manifest 的 `runtime_consumers` 为空，A03 结果的 `initial_harbor_integrated` 和 `release_approved` 均为 false。候选资源不能被 UI、Agent 或审计脚本直接接入运行时。

## 6. 磁盘和 Git 策略

- 已忽略的外部缓存归档：`artifacts/external_sources/kenney_nature-kit.zip`，10,537,521 bytes；仅用于复核和可重复构建，不应复制进普通 Git 历史。
- 候选目录实际磁盘占用：约 14,867,410 bytes（329 FBX、8 JPG、许可证和 Unity `.meta`）。
- 候选目录中的 FBX 约 9,124,095 bytes，Poly Haven JPG 约 4,794,836 bytes。
- 当前没有确认远程 Git LFS 支持、配额或大文件策略，因此没有新增 `.gitattributes`，也没有迁移历史。后续若资源包明显增大，应先确认远程 LFS/配额；在确认前继续使用仓库外或被忽略的本地缓存，禁止静默加入数 GB 压缩包。

## 7. 后续责任和未完成项

本包已经满足“真实下载、真实哈希、真实导入统计、50 个抽样、337 个候选、可审计且不接入运行时”的 A03 交付标准。以下事项明确留给后续任务：

1. 为选中的模型建立正式 Prefab，并验证比例、Pivot、碰撞、LOD、锚点和交互入口；此时才能评估 `LocalDevelopmentReady`。
2. 为 Coral Mud/Sand 图组建立材质实例，水面/浅水 Shader 不在本包中；不得把 JPG 直接当作已完成的水体表现。
3. 处理 Kenney 原始命名警告时保留来源文件名和稳定 ID 映射，不批量重命名源资产导致哈希失效。
4. 任何正式构建或 InitialHarbor 替换前，必须有人类审核者确认许可、性能、碰撞、LOD、光照和表现；本报告不授予 `ReleaseApproved`。

## 8. 机器结果索引

- [`D18_A03_RESULT.json`](../../../../game/tools/results/D18/D18-A03/D18_A03_RESULT.json)
- [`D18_A03_CANDIDATE_CATALOG.json`](../../../../game/tools/results/D18/D18-A03/D18_A03_CANDIDATE_CATALOG.json)
- [`D18_A03_UNITY_IMPORT_AUDIT.json`](../../../../game/tools/results/D18/D18-A03/D18_A03_UNITY_IMPORT_AUDIT.json)
- [`D18_A03_MANIFEST_AUDIT.json`](../../../../game/tools/results/D18/D18-A03/D18_A03_MANIFEST_AUDIT.json)
- [`D18_A03_MISSING_REFERENCE_CHECK.json`](../../../../game/tools/results/D18/D18-A03/D18_A03_MISSING_REFERENCE_CHECK.json)
- [`D18_A03_IMPORT_MATRIX.json`](../../../../game/tools/results/D18/D18-A03/D18_A03_IMPORT_MATRIX.json)
- [`D18_A03_CONTACT_SHEET.json`](../../../../game/tools/results/D18/D18-A03/D18_A03_CONTACT_SHEET.json)
- [`kenney_nature_kit_contact_sheet.png`](../../../../game/tools/results/D18/D18-A03/kenney_nature_kit_contact_sheet.png)
