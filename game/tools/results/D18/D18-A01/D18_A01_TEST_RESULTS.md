# D18-A01 工具与资源审计结果

- 任务：`D18-A01`
- 日期：`2026-08-14`
- 结论：**PASS**（来源治理、只读审计、负向夹具、官方入口核对和缓存边界完成；六个未下载包按事实保持 Candidate/NotImported）
- 工具性质：只读；`registry_modified=false`；不批准、不导入、不写 Registry。

## 命令与结果

| 命令 | 结果 |
|---|---|
| `python -m unittest tools.tests.test_d18_asset_manifest_audit -v` | **8/8 PASS** |
| `python tools/d18_asset_manifest_audit.py --manifest-root docs/game/design/common/assets/manifests --project-root . --output game/tools/results/D18/D18-A01/d18_asset_manifest_audit.json` | exit `0`；`PASS`；2 manifests；0 errors |
| `python tools/d18_asset_manifest_audit.py --manifest-root tools/tests/fixtures/d18_asset_manifest_audit --project-root . --output game/tools/results/D18/D18-A01/d18_asset_manifest_fixture_audit.json` | exit `2`；`BLOCKED`；7 fixtures；14 errors |
| `git lfs version` | `git-lfs/3.5.1`，本机安装 |
| `git remote -v` | 有 `origin` URL；不证明远程 LFS endpoint、配额或写权限 |
| `git diff --check` | 交付前复核要求无 whitespace error |

## 真实候选审计

真实目录 `docs/game/design/common/assets/manifests/` 包含：

1. Kenney Pirate Kit 垂直切片 manifest：真实归档 SHA-256、包内 `License.txt`、6 个真实候选 FBX；状态 `Candidate`，导入证据 `ImportVerified`。
2. Kenney Blocky Characters 垂直切片 manifest：真实归档 SHA-256、包内 `License.txt`、1 个真实候选 FBX；状态 `Candidate`，导入证据 `ImportVerified`。

两者均没有 `runtime_consumers`，因此工具不会把候选误判为已接入场景或正式 Prefab。

## 故意损坏夹具

| 夹具 | 触发规则 | 结果 |
|---|---|---|
| `missing_field/missing_sha256.json` | 缺失字段、哈希缺失 | BLOCKED |
| `path_escape/escape.json` | 候选路径逃逸项目根目录 | BLOCKED |
| `broken_evidence/missing_license.json` | 许可证证据断链 | BLOCKED |
| `state_transition/illegal.json` | Candidate 直接跳 ReleaseApproved、无责任人类证据 | BLOCKED |
| `runtime_reference/unapproved.json` | 未批准 manifest 声明 InitialHarbor 运行时消费者 | BLOCKED |
| `conflict/source_a.json + source_b.json` | 同一文件被两个来源/包声称 | BLOCKED |

## 机器结果

- [真实候选审计 JSON](d18_asset_manifest_audit.json)
- [损坏夹具审计 JSON](d18_asset_manifest_fixture_audit.json)
- [Schema](../../../../../tools/schemas/d18_asset_pack_manifest.schema.json)
- [审计工具](../../../../../tools/d18_asset_manifest_audit.py)

## LFS/缓存结论

本机 Git LFS 已安装，但 `.gitattributes` 不存在，远程配额/endpoint/恢复流程不可确认，因此本任务没有新增 LFS 规则，也没有迁移历史。`.gitignore` 只增加 `artifacts/external_sources` 下 zip/7z/rar/tar/gz 的本地缓存忽略规则；没有删除或移动现有归档。

## 非阻塞待办

- Kenney UI Pack、Input Prompts 和四个 Quaternius 包只完成官方页面/许可证入口核对，未下载，保持无哈希、无本地文件计数、无 Unity 导入和 `Candidate`；后续资源任务下载当天补证据。
- Poly Haven 与 ambientCG 只建立来源级 CC0 规则；实际单项资产仍需下载日来源页、文件哈希和本地许可证证据。
- D16-A04 的人工历史/授权 Pending、D16-A05 的 placeholder、风格/比例/LOD/碰撞/锚点/消费者审核未被本工具代替。
