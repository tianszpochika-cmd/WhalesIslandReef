# D18-A01 交接报告

- 任务编号：`D18-A01`
- 日期：`2026-08-14`
- 范围：免费资源来源治理、manifest Schema、只读审计、Kenney 真实候选审计和 LFS/缓存策略
- 最终结论：**PASS**

`PASS` 仅表示 D18-A01 规定的来源治理、manifest Schema、只读审计、负向夹具、官方许可入口核对和 Git/LFS 缓存边界已经完成。首批八个包中，两个 Kenney 包有本地真实证据；其余六个仍保持 `Candidate`/`NotImported`，没有把未下载事实写成哈希、数量或审核通过。人工授权/历史审查和远程 LFS 能力仍未证明，并已保留给后续责任任务。

## 1. 实际修改文件

### 主成果与来源证据

- `docs/game/design/common/D18_FREE_ASSET_SOURCE_CATALOG.md`
- `docs/game/design/common/assets/d18_asset_pack_manifest.example.json`
- `docs/game/design/common/assets/manifests/kenney_pirate_kit_vertical_slice.manifest.json`
- `docs/game/design/common/assets/manifests/kenney_blocky_characters_vertical_slice.manifest.json`
- `tools/schemas/d18_asset_pack_manifest.schema.json`

### 工具、测试和夹具

- `tools/d18_asset_manifest_audit.py`
- `tools/tests/test_d18_asset_manifest_audit.py`
- `tools/tests/fixtures/d18_asset_manifest_audit/` 下 7 个故意损坏/冲突夹具

### 机器结果与留痕

- `game/tools/results/D18/D18-A01/d18_asset_manifest_audit.json`
- `game/tools/results/D18/D18-A01/d18_asset_manifest_fixture_audit.json`
- `game/tools/results/D18/D18-A01/D18_A01_TEST_RESULTS.md`
- `game/tools/results/D18/D18-A01/D18_A01_RESULT.json`
- `.gitignore`：仅增加 `artifacts/external_sources` 的压缩包缓存忽略规则
- `.workbuddy/memory/2026-08-14.md`

未修改 Unity 主场景、正式模型、Prefab、Registry、Systems、contracts、server、web、构建设置或 Git 历史；未提交、未推送。

## 2. 来源与真实核对日期

核对日期均为 `2026-08-14`：

- Kenney：Pirate Kit、Blocky Characters、UI Pack、Input Prompts 官方包页均标注 CC0；Pirate Kit 和 Blocky Characters 另外有本地包内 `License.txt` 与 2026-08-13 归档哈希。
- Quaternius：Pirate Kit、Ultimate Stylized Nature、Ultimate Animated Animal Pack、Animated Man Pack 官方包页均显示 CC0，并写明个人/商业项目可用；本任务未下载这些包。
- Poly Haven：官方 [License](https://polyhaven.com/license) 页说明资产为 CC0、可商业使用/修改/再分发且不要求署名；站点 Logo、缩略图、文本和元数据不随资产授权。
- ambientCG：官方站和资产页说明下载资产为 CC0、可商业使用且无需署名；[license information](https://docs.ambientcg.com/books/website-licensing/page/license-information) 作为官方入口留档，单项下载仍需重新保存证据。

官方页面 URL、类别、官方页声明数量和“本地是否下载/是否有哈希/是否导入”已写入 [来源目录](../../D18_FREE_ASSET_SOURCE_CATALOG.md)。页面展示数量不被冒充为本地文件计数。

## 3. 工具命令与测试结果

```text
python -m unittest tools.tests.test_d18_asset_manifest_audit -v
8/8 PASS

python tools/d18_asset_manifest_audit.py --manifest-root docs/game/design/common/assets/manifests --project-root . --output game/tools/results/D18/D18-A01/d18_asset_manifest_audit.json
exit 0 / PASS / 2 manifests / 0 errors

python tools/d18_asset_manifest_audit.py --manifest-root tools/tests/fixtures/d18_asset_manifest_audit --project-root . --output game/tools/results/D18/D18-A01/d18_asset_manifest_fixture_audit.json
exit 2 / BLOCKED / 7 fixtures / 14 errors

git lfs version
git-lfs/3.5.1

git diff --check
交付前复核通过
```

审计检查缺字段、重复包 ID、状态跃迁、哈希、许可证证据、候选文件、路径逃逸、未批准运行时引用和同文件多来源冲突。工具返回 `BLOCKED` 时只输出结果，不修改 Registry 或状态。

## 4. 当前可证明与不可证明的许可范围

### 可证明

- 两个 Kenney 垂直切片归档名和 SHA-256 来自既有 D16-A05 候选证据；包内 `License.txt` 明确 CC0、个人/教育/商业使用和可修改。
- 两个 Kenney manifest 的候选 FBX 路径存在，许可证证据存在，`runtime_consumers` 为空；现有 Unity 导入证据仍只代表导入，不代表最终表现或发布。
- 四个官方来源的来源级许可证入口和适用类别已登记。

### 不可证明/保持 Pending

- Kenney UI Pack/Input Prompts、四个 Quaternius 包没有本地归档、哈希、文件计数、Unity 对象计数或包内许可证快照。
- Poly Haven/ambientCG 只有来源级 CC0 说明；具体单项资源的作者、资产页、哈希、下载包和本地证据尚未建立。
- 没有证明任何资源已经 `ReleaseApproved`；Agent 和工具不代替责任人类审核者。
- `git lfs` 本机安装不等于 GitHub LFS 远程 endpoint、配额、权限和恢复流程可用。

## 5. 未解决风险与后续责任

| 严重度 | 风险 | 责任/处理顺序 |
|---|---|---|
| P0 | 混合许可、NC、仅个人、登录/会员下载、禁止再分发或来源不明 | 保持 Candidate/Blocked；A03/A04 逐包补证据，人类确认后才能继续 |
| P0 | D16-A04 267 条人工 Pending 和 D16-A05 2264 个 placeholder | 不由 A01 自动批准或替换；按 D16/A03—A06 责任边界处理 |
| P1 | Quaternius/在线来源未下载，不能填写哈希/数量 | 这是后续资源任务的输入条件，不是 A01 治理失败；下载当天新建 manifest 版本，保存包内许可证/官方页面/哈希后再审计 |
| P1 | LFS 配额不可确认 | 继续使用仓库外/被忽略缓存；确认 endpoint/配额/恢复测试前不提交大包 |
| P1 | Kenney 候选尚缺最终风格、比例、材质、LOD、碰撞、锚点和消费者证据 | 交给 D18-A03—A06；不得从 `ImportVerified` 推导 `LocalDevelopmentReady` |

## 6. 后续 Agent 注意事项

- 从 [manifest 示例](../../assets/d18_asset_pack_manifest.example.json) 复制；未下载字段写 `null`，不要猜哈希或文件数量。
- 使用唯一 `source_<name>` / `pack_<source>_<pack>_v<number>` ID；不要把颜色变体伪装成新包。
- 先运行 [只读审计工具](../../../../../../tools/d18_asset_manifest_audit.py)，再进行自己的职责范围工作；不要把工具输出改写为批准。
- `runtime_consumers` 在人工批准和正式消费者证据前必须为空；候选不允许引用 InitialHarbor 或正式 Prefab。
- 继续保持 AgentCore/System/Unity 主场景/Prefab/Registry 和 Git 历史边界。
