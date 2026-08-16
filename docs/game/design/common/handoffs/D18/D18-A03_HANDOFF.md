# D18-A03 交接报告：环境候选资源包

- 任务编号：D18-A03
- 日期：2026-08-14
- 结论：**PASS**（候选包和导入预检通过；不是 `LocalDevelopmentReady`，不是 `ReleaseApproved`）
- 单一写者边界：仅环境候选目录、环境 manifest、A03 结果、环境包报告、D18-A03 交接和记忆日志

## 1. 实际修改文件和产物

### 环境候选

- `game/Assets/Art/Candidates/Environment/KenneyNatureKit/Models/`：329 个官方包内 FBX 与 Unity `.meta`
- `game/Assets/Art/Candidates/Environment/KenneyNatureKit/License.txt`
- `game/Assets/Art/Candidates/Environment/PolyHaven/Textures/`：Sand 01 4 张 1K JPG、Coral Mud 01 4 张 1K JPG 与 `.meta`

### Manifest 和许可证

- `docs/game/design/common/assets/packs/environment/kenney_nature_kit_d18_a03.manifest.json`
- 8 个 `polyhaven_*_d18_a03.manifest.json`，每个对应一个真实 JPG 文件和真实 SHA-256；没有使用“合并归档”伪造哈希
- `docs/game/design/common/assets/packs/environment/licenses/polyhaven_license_evidence_2026-08-14.md`

### 机器结果和工具

- `game/tools/results/D18/D18-A03/build_a03_packet.py`
- `game/tools/results/D18/D18-A03/finalize_a03_packet.py`
- `game/tools/results/D18/D18-A03/check_a03_missing_references.py`
- `game/tools/results/D18/D18-A03/make_contact_sheet.ps1`
- `game/tools/results/D18/D18-A03/D18_A03_RESULT.json`
- `game/tools/results/D18/D18-A03/D18_A03_CANDIDATE_CATALOG.json`
- `game/tools/results/D18/D18-A03/D18_A03_UNITY_IMPORT_AUDIT.json/.md`
- `game/tools/results/D18/D18-A03/D18_A03_IMPORT_MATRIX.json`
- `game/tools/results/D18/D18-A03/D18_A03_MANIFEST_AUDIT.json`
- `game/tools/results/D18/D18-A03/D18_A03_MISSING_REFERENCE_CHECK.json`
- `game/tools/results/D18/D18-A03/D18_A03_CONTACT_SHEET.json`
- `game/tools/results/D18/D18-A03/kenney_nature_kit_contact_sheet.png`

没有修改 Unity 主场景、Prefab、Registry、InitialHarbor、Systems、contracts、构建设置或正式内容 ID。

## 2. 包清单、来源、许可和哈希

| 包/实际内容 | 官方页面 | 许可证证据 | 实际数量 | 哈希 |
|---|---|---|---:|---|
| Kenney Nature Kit / FBX 格式 | https://kenney.nl/assets/nature-kit | 包内 `game/Assets/Art/Candidates/Environment/KenneyNatureKit/License.txt`；页面标注 CC0 | 329 FBX / Unity object 329 | `FA7974A0D342BFE63C38664BA9F8EC1A4AAB8EA25F099BDC56870E33588C4D9D`（官方归档，10,537,521 bytes） |
| Poly Haven Sand 01 / 1K maps | https://polyhaven.com/a/sand_01 | https://polyhaven.com/license；本地 `polyhaven_license_evidence_2026-08-14.md` | 4 JPG / Unity texture 4 | 每个 JPG 的 SHA-256 记录在对应 manifest 和 A03 主报告 |
| Poly Haven Coral Mud 01 / 1K maps | https://polyhaven.com/a/coral_mud_01 | https://polyhaven.com/license；本地 `polyhaven_license_evidence_2026-08-14.md` | 4 JPG / Unity texture 4 | 每个 JPG 的 SHA-256 记录在对应 manifest 和 A03 主报告 |

Poly Haven 文件的实际 SHA-256：

- `sand_01_diff_1k.jpg`: `A73B90EAA6D675EF57E7D48A8CF23E1F412E353AB2E29CF73930B8ECD4E76875`
- `sand_01_nor_gl_1k.jpg`: `73984AECDC35B1FFBC322D64B7F8D8165B06970D11F37DD47010609FCB532704`
- `sand_01_rough_1k.jpg`: `CCB179B12C56A9032D60E4F4E2DA690B3612A58E1784EB14576372690CDCD1CB`
- `sand_01_ao_1k.jpg`: `BE5FED35A2B259088682241CE4CF1EB0EE5D36B820F520E3F5E7F353BD1F627A`
- `coral_mud_01_diff_1k.jpg`: `42FDAEA6FD0263678F70332ADD3C109FDD54E4C32B61FC98CC054D4F1A9DEAD1`
- `coral_mud_01_nor_gl_1k.jpg`: `D19D8AC5CAB1B4C524E51EA91DBEB12DC3D986772B5D280D434096228E5DDA38`
- `coral_mud_01_rough_1k.jpg`: `CD841F419432F31253AED9FD12E775041A961CFB01AF59BF3BAED5E4459D035A`
- `coral_mud_01_ao_1k.jpg`: `FB0C6B07FEE149527D553F83D683855A9E1DDEBEC94FD80AB8FC9B878FD62B07`

所有 manifest 在最终机器审计中为 `review_status=ImportVerified`、`import_status=ImportVerified`，且 `runtime_consumers=[]`。没有一个 manifest 是 `ReleaseApproved`。

## 3. 导入和抽样结果

使用隔离 Unity 6000.0.80f1 项目运行只读 D18-A02 导入器：

```text
status=PASS
files=337 (models=329, textures=8)
meshes=338, materials=688, animation_clips=0
errors=0, warnings=235
missing_scripts=0, missing_references=0
colliders=0, lod_groups=0, interaction_anchors=0
```

50 个样板模型已实际导入预检并生成联系表，抽样覆盖：

`palm=true, broadleaf_or_tree=true, bush=true, grass=true, rock=true, cliff=true, beach=true, dirt_path=true, wood=true, stone=true, water_edge=true`。

联系表路径：`game/tools/results/D18/D18-A03/kenney_nature_kit_contact_sheet.png`。

235 条警告不构成导入失败：226 条是官方原始文件名不符合本项目蛇形命名规则，9 条来自导入快照中的 manifest 状态提示；没有通过改名或删除资源掩盖问题。最终 A03 manifest 审计为 9 个 manifest、0 errors、0 issues。

## 4. 磁盘增量与缓存策略

- 环境候选目录：约 14,867,410 bytes（包含 `.meta` 和许可证）
- Kenney 外部缓存归档：10,537,521 bytes，位于被忽略的 `artifacts/external_sources/kenney_nature-kit.zip`
- 候选 FBX：约 9,124,095 bytes
- Poly Haven JPG：约 4,794,836 bytes
- 没有确认远程 Git LFS 支持或配额，所以没有新增 `.gitattributes`，没有迁移历史，也没有提交数 GB 二进制；后续大包应继续使用外部/被忽略缓存，先确认 LFS 后再制定正式策略。

## 5. 未满足项和责任归属

这些是明确的后续工作，不是本任务隐瞒的完成项：

1. **Prefab/交互责任（后续 D19 环境资产任务）**：原始 FBX 没有碰撞体、LODGroup、交互锚点；A03 只测量并记录为 0，不创建 Prefab。
2. **水体表现责任（水体/材质责任任务）**：Coral Mud 01 是海床材质，尚未创建浅水水面 Shader 或正式材质实例。
3. **正式接入责任（InitialHarbor/Registry 所属任务）**：本任务没有消费者，不能直接把候选资源视为可玩场景资源。
4. **人类许可/发布审核（责任审核者）**：`ReleaseApproved` 必须由有责任的人类审核者确认，Agent 不代签。
5. **原始命名警告（Prefab/目录责任任务）**：保留原始文件名和哈希；若要显示名映射，新增映射而不是重命名源文件。

## 6. 验证命令和结果

```text
python game/tools/results/D18/D18-A03/build_a03_packet.py
  -> 329 Kenney models, 8 Poly Haven textures, 337 candidates

Unity 6000.0.80f1 isolated report-only audit
  -> PASS, 337 files, 0 errors, 235 warnings

python game/tools/results/D18/D18-A03/finalize_a03_packet.py
  -> PASS, ImportVerified sample=50

python tools/d18_asset_manifest_audit.py --manifest-root docs/game/design/common/assets/packs/environment --project-root . --output game/tools/results/D18/D18-A03/D18_A03_MANIFEST_AUDIT.json
  -> PASS, 9 manifests, 0 errors, 0 issues

python game/tools/results/D18/D18-A03/check_a03_missing_references.py
  -> PASS, 337 assets, 0 missing references, 0 runtime consumers

powershell -ExecutionPolicy Bypass -File game/tools/results/D18/D18-A03/make_contact_sheet.ps1
  -> PASS, 50 previews, 5x10 contact sheet
```

## 7. 最终状态

**PASS**：D18-A03 的候选资源、许可证证据、实际哈希、目录、导入预检、50 个样板抽样、机器审计和联系表均已交付；状态冻结在 `ImportVerified`，不允许据此推断 `LocalDevelopmentReady` 或 `ReleaseApproved`。D19 可以从联系表和 manifest 选择资源，但必须继续完成 Prefab、碰撞、LOD、材质实例、场景接入和人类发布审查。

