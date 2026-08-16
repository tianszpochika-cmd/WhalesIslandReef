# D18-A01 免费资源来源目录与审计规则

- 任务：`D18-A01`
- 核对日期：`2026-08-14`（官方网页/许可证原文入口）
- 当前状态：**PASS**
- 说明：本任务的交付范围是来源治理、manifest Schema、只读审计、首批包登记和 LFS/缓存边界。现有两个 Kenney 候选已具备本地证据并通过审计；其余六个包仍明确保持 `Candidate`/`NotImported`，没有伪造归档名、哈希、文件数、Unity 对象数或审核结果。未下载不是本治理任务的失败状态，后续资源任务必须在下载当天补齐证据。

## 1. 使用边界

本目录服务 D18-A03—A06 的资源制作和审计，不是内容 Registry，不是 Prefab 清单，也不是发布批准表。来源记录、manifest、许可证证据、导入结果和人工审核结果分别留痕：

- `Candidate` 只代表有来源入口的候选；
- `LicenseEvidenceComplete` 只代表许可证证据字段齐全，不能推导导入或发布；
- `ImportVerified` 只代表导入工具/Unity 证据通过，不能推导人类审核；
- `LocalDevelopmentReady` 只允许本地开发轨使用；
- `ReleaseApproved` 只能由责任人类审核者确认；工具不批准、不写 Registry、不升级状态；
- 不确定的授权、混合许可证、来源不明或需要登录的下载始终保持 `Candidate`/人工 `Pending`，不得自动接入。

机器入口：[manifest Schema](../../../../tools/schemas/d18_asset_pack_manifest.schema.json)、[manifest 示例](assets/d18_asset_pack_manifest.example.json)、[只读审计工具](../../../../tools/d18_asset_manifest_audit.py)、[现有 Kenney manifests](assets/manifests/)。

## 2. 官方来源核对

### 2.1 来源级许可入口

| source_id | 官方入口 | 核对到的许可证事实 | 适用类别 | 当前收录结论 |
|---|---|---|---|---|
| `source_kenney` | [Kenney Assets](https://kenney.nl/assets)；包页见 §3 | 包页标注 `Creative Commons CC0`；已下载候选另有包内 `License.txt` | 低多边形船、岛、角色、UI、输入图标、道具 | CC0 包可进入本地候选；必须保留包内 License.txt 和下载哈希 |
| `source_quaternius` | [Quaternius Assets](https://quaternius.com/)；包页见 §3 | 各包官方页标注 `CC0`，并写明个人/商业项目可用；下载后仍需保存包内原文（如有）和哈希 | 低多边形海盗、自然、动物、角色/动画 | 页面证据已核对；未下载包不具备本地 LicenseEvidenceComplete |
| `source_polyhaven` | [Poly Haven License](https://polyhaven.com/license) | 官方许可页写明 HDRI、纹理和模型为 CC0，可商业使用、修改、再分发且不要求署名；网站 Logo、缩略图、文本和元数据不随 CC0 资产授权 | PBR 材质、HDRI、岩石、木材、地表 | 只收录实际资产文件；不复制网页预览图/Logo/站点文本 |
| `source_ambientcg` | [ambientCG 官方站](https://ambientcg.com/)；[官方许可信息入口](https://docs.ambientcg.com/books/website-licensing/page/license-information) | 官方站及资产页写明可下载资产为 CC0，可商业使用且无需署名；下载时仍需逐项保存资产页、下载文件和许可证入口 | PBR 地表、木材、石、织物、HDRI、少量模型 | 只收录单项资产页和文件哈希；官方 license information 页面若当日不可访问则保持人工复核 |

网页内容可能随版本、作者或站点策略变化。重新下载时必须重新核对官方页、包内许可证和哈希；本表的 2026-08-14 核对不能替代未来下载日证据。

### 2.2 来源和包 ID 规则

- `source_id`：`source_<小写来源名>`，同一官方来源只登记一次。
- `pack_id`：`pack_<小写来源>_<小写包名>_v<整数>`；同一包升级或重新下载许可证/内容变更时增加版本，不覆盖旧 manifest。
- 文件不得因为换颜色、换材质或改名而制造新的包 ID；变体写在 `candidate_files[].role` 或后续内容映射。
- `candidate_files[].target_id` 引用现有内容/资产目标，但不改变 `entity_id`、`content_id` 或 Registry 所有权。
- 同一规范化文件路径只能属于一个 `source_id + pack_id` 组合；不同来源声称同一文件时必须 `BLOCKED`，由人类核对来源和哈希。
- `sha256` 必须是实际下载归档的 64 位十六进制字符串；未下载写 `null`，不写猜测值。
- `file_count` 说明 manifest 的计数口径。当前 Kenney manifest 明确是“本垂直切片登记的候选文件数”，不是压缩包总文件数；未下载写 `null`。
- `unity_object_count` 只记录实际导入证据中的对象数；没有 Unity 导入报告写 `null`。

## 3. 第一批包清单

“官方页显示数量”仅是网页当日的展示信息；“本地事实”只有已经下载并有哈希/文件路径的 Kenney 垂直切片。所有 Quaternius、Kenney UI/Input 包在本任务中均不伪造本地下载事实。

| pack_id | 官方包页与类别 | 官方页当日声明 | 本地归档/哈希/计数 | review / import | 当前动作 |
|---|---|---|---|---|---|
| `pack_kenney_pirate_kit_vertical_slice_v1` | [Kenney Pirate Kit](https://kenney.nl/assets/pirate-kit)；船、岛、角色、建筑、道具 | 页面显示 3D、动画、70 files、CC0 | `kenney_pirate-kit.zip`; SHA-256 `667ED2CAF92954DDB98F7B7CEDE831FE99AB75063C26B25E23D32715BEE9C943`; 本 manifest 6 个候选 FBX | `Candidate`; `ImportVerified`（仅导入证据） | 保持候选；等待 A04/A05 人工、材质、LOD、碰撞、锚点和消费者确认 |
| `pack_kenney_blocky_characters_vertical_slice_v1` | [Kenney Blocky Characters](https://kenney.nl/assets/blocky-characters)；角色、动画 | 页面显示 3D、动画、20 files、CC0 | `kenney_blocky-characters_20.zip`; SHA-256 `5E123859AA0C1598342B600C6DB197024A1D63EB9EC531398B310725F589887E`; 本 manifest 1 个候选 FBX | `Candidate`; `ImportVerified`（仅导入证据） | 保持候选；等待风格、历史表现、比例、骨骼和消费者确认 |
| `pack_kenney_ui_pack_v2` | [Kenney UI Pack](https://kenney.nl/assets/ui-pack)；2D UI 面板/按钮/滑条 | 页面显示 2D、430 files、CC0 | 未下载：archive/hash/local count `null` | `Candidate`; `NotImported` | 只登记官方入口；下载后补本地 License/哈希/清单 |
| `pack_kenney_input_prompts_v1` | [Kenney Input Prompts](https://kenney.nl/assets/input-prompts)；键鼠/手柄/输入图标 | 页面显示 64×64、1500 files、CC0；官方页包含多平台 glyph | 未下载：archive/hash/local count `null` | `Candidate`; `NotImported` | 只登记官方入口；下载后核对字体/图标授权与 UI 适配 |
| `pack_quaternius_pirate_kit_v1` | [Quaternius Pirate Kit](https://quaternius.com/packs/piratekit.html)；海盗、环境、动画角色 | 页面显示 71 models、FBX/OBJ/Blend/glTF、CC0；2023-11 | 未下载：archive/hash/local count `null` | `Candidate`; `NotImported` | 不把页面数量当本地数量；下载/包内证据后再审 |
| `pack_quaternius_ultimate_stylized_nature_v1` | [Ultimate Stylized Nature Pack](https://quaternius.com/packs/ultimatestylizednature.html)；自然、无缝纹理/法线 | 页面显示 63 models、FBX/OBJ/Blend/glTF、CC0；2022-05 | 未下载：archive/hash/local count `null` | `Candidate`; `NotImported` | 需核对生境、材质、贴图和历史表现；不直接替换岛屿 |
| `pack_quaternius_ultimate_animated_animal_v1` | [Ultimate Animated Animal Pack](https://quaternius.com/packs/ultimateanimatedanimals.html)；动物/动画 | 页面显示 12 animals、每个 12+ animations、FBX/OBJ/Blend/glTF、CC0；2021-07 | 未下载：archive/hash/local count `null` | `Candidate`; `NotImported` | 需生态行为、碰撞、LOD 和敏感内容检查 |
| `pack_quaternius_animated_man_v1` | [Animated Man Pack](https://quaternius.com/packs/animatedman.html)；人形/动画 | 页面显示 2 models、FBX/OBJ/Blend、CC0；2017-10 | 未下载：archive/hash/local count `null` | `Candidate`; `NotImported` | 需历史服装、骨骼、动作、比例和角色边界检查 |

实际 manifest 文件：

- [Kenney Pirate Kit 垂直切片 manifest](assets/manifests/kenney_pirate_kit_vertical_slice.manifest.json)
- [Kenney Blocky Characters 垂直切片 manifest](assets/manifests/kenney_blocky_characters_vertical_slice.manifest.json)
- [通用 manifest 示例](assets/d18_asset_pack_manifest.example.json)

## 4. 许可和准入规则

### 4.1 CC0 基线

自动工具只接受“证据字段完整”的 CC0 作为**可进入下一人工/导入阶段的候选输入**，不自动把它变成 Approved 或 ReleaseApproved。必须同时有：

1. 官方来源页和下载入口；
2. 下载日期、归档名、实际 SHA-256；
3. 官方许可证页或包内许可证原文，且 `license_evidence_path` 可在仓库内解析；
4. 商业使用、修改、源文件再分发、构建分发和署名要求的明确值；
5. 候选文件路径、唯一目标 ID、导入状态和不进入运行时的证明；
6. 人工责任任务、历史/敏感内容和本项目低多边形适配的后续入口。

### 4.2 拒绝、阻塞和人工复核

下列情况不因“免费”“下载按钮可见”或搜索结果摘要而通过：

| 情况 | 工具动作 | 人工动作 |
|---|---|---|
| 许可证混杂、同包不同条款、第三方依赖未列出 | `BLOCKED: mixed_license` | 拆包、逐文件补证据，或拒绝整包 |
| 需要登录、Patreon/会员、Source key 或专属下载 | `BLOCKED: access_required`；不绕过登录/限流 | 由负责人确认授权和可复现下载；否则拒绝 |
| 仅限个人/教育、非商业 | `BLOCKED: non_commercial` | 不进入正式构建；若要用，先取得明确商业授权并由人类审核 |
| `NC`/NonCommercial | `BLOCKED: non_commercial` | 不自动接入；不得为了发布改写许可证 |
| 来源、作者、原始页或文件哈希不明 | `BLOCKED: provenance_unknown` | 补官方原文和哈希，不能猜作者或来源 |
| 禁止再分发源文件或禁止随产品分发 | `BLOCKED: redistribution_restricted` | 只在得到明确授权并设计不分发源文件的方案后人工复核 |
| 只有站点预览图、Logo、文本或元数据 | `BLOCKED: site_content_not_asset_license` | 不把网站内容当作资产 CC0；只保存真实资产和授权证据 |
| 许可证证据 URL 能访问但没有本地快照/包内原文 | `Candidate/Pending` | 下载当天保存本地证据；在此之前不进入 ImportVerified |

`ReleaseApproved` 只有责任人类审核者签署后才存在；审计工具只能检查签名字段/证据路径是否存在，不能验证人类决定的真实性，更不能推断批准。

## 5. A03—A06 可复制工作流

1. 复制 [manifest 示例](assets/d18_asset_pack_manifest.example.json)，改写 `source_id`/`pack_id`/标题和官方入口。
2. 只填真实下载日期、归档名、SHA-256、候选文件数和 Unity 对象数；未下载一律保留 `null` 并保持 `Candidate`/`NotImported`。
3. 把包内许可证原文或官方许可证证据放在可追踪路径；在 `license_evidence_path` 填仓库相对路径。
4. 运行：

   ```powershell
   python tools/d18_asset_manifest_audit.py --manifest-root docs/game/design/common/assets/manifests --project-root . --output game/tools/results/D18/D18-A01/d18_asset_manifest_audit.json
   ```

5. 只有审计无 blocker、人工审查任务已建立、导入/LOD/碰撞/锚点/消费者有证据时，才可继续 A03—A06 的职责范围；审计工具不会改 Registry、Prefab 或状态。
6. 任何资源进入运行时前，额外检查 `runtime_consumers` 是否为空或已有责任审核者和正式消费者证据；候选 manifest 不得引用 `InitialHarbor.unity` 或正式 Prefab。

## 6. Git LFS 与本地下载缓存审计

本次只读检查结果：

- `git lfs version`：`git-lfs/3.5.1 (GitHub; windows amd64)`，本机已安装；
- `git remote -v`：存在 GitHub `origin` URL，但这不能证明 LFS endpoint、配额、写权限或恢复流程；
- `.gitattributes`：当前不存在，没有隐式 LFS 追踪规则；
- 结论：**LFS 远程支持和配额不可确认**，本任务不创建 `.gitattributes`，不迁移历史，不把源压缩包加入普通 Git。

在 `.gitignore` 中只增加未来本地源缓存的最小规则：`/artifacts/external_sources/*.zip`、`*.7z`、`*.rar`、`*.tar`、`*.gz`。这不会删除现有未提交文件。下载包必须放在仓库外优先；仓库只保留 manifest、许可证证据、哈希和实际运行时需要的优化资源。

冻结上限：普通 Git 单个压缩包 ≤ `50 MiB`；单个运行时二进制建议 ≤ `100 MiB`；每个任务/章节仓库增量目标 ≤ `250 MiB`。超过上限或需要源包版本化时，先由人类负责人确认 LFS endpoint、配额和恢复测试，禁止 Agent 静默加入数 GB 二进制。

## 7. 当前结论

本任务的来源治理和工具链为 **PASS**：四个官方来源和八个首批包已核对入口；两个 Kenney 真实候选 manifest 可审计；未下载的六个包保持无哈希、无本地计数、无导入和 `Candidate`，这是刻意保留的事实边界，不是将其写成已批准。D16-A04 的人工 Pending、D16-A05 placeholder、资源风格/历史/碰撞/LOD/消费者和 LFS 远程能力仍属于后续任务的未决事项，不能被本结论掩盖。
