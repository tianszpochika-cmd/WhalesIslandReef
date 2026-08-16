# D18-A05 船舶与港口资源交接

- 任务：D18-A05（A* 异步资产任务）
- 日期：2026-08-14
- 最终状态：`PARTIAL`
- 责任范围：`game/Assets/Art/Candidates/ShipsPortProps/`、`docs/game/design/common/assets/packs/ships_port_props/`、本任务结果与交接
- 运行时接入：无
- `ReleaseApproved`：未设置

## 前置任务

| 前置 | 状态 | 证据 |
|---|---|---|
| D18-S01 | PASS | `docs/game/design/common/handoffs/D18/D18-S01_HANDOFF.md` |
| D18-A01 | PASS | `docs/game/design/common/handoffs/D18/D18-A01_HANDOFF.md` |
| D18-A02 | PASS | `docs/game/design/common/handoffs/D18/D18-A02_HANDOFF.md`、`game/tools/results/D18/D18-A02/D18_A02_RESULT.json` |

## 包、来源、许可和哈希

| 包 | 官方链接 | 许可 | 官方归档原名 | SHA-256 | 实际模型 | 实际预览 | 导入 |
|---|---|---|---|---|---:|---:|---|
| Kenney Pirate Kit | https://kenney.nl/assets/pirate-kit | CC0-1.0 | `kenney_pirate-kit.zip` | `667ED2CAF92954DDB98F7B7CEDE831FE99AB75063C26B25E23D32715BEE9C943` | 72 | 72 | ImportVerified |
| Kenney Building Kit | https://kenney.nl/assets/building-kit | CC0-1.0 | `kenney_building-kit.zip` | `2740EF5772FB5FB3D7AAB881DB22D129F6B68AFE711B1A79E6D5E9E19CF3EEC6` | 79 | 79 | ImportVerified |
| Quaternius Pirate Kit | https://quaternius.com/packs/piratekit.html | CC0（官方页核验） | 未下载 | — | 0 | 0 | ReviewedNotDownloaded |

Kenney 两包的官方页面和包内 `License.txt` 均保留；CC0 证据入口为 `https://creativecommons.org/publicdomain/zero/1.0/`。Quaternius 只做官方页面和许可证核验，没有把网页的模型数量计入交付，也没有下载混入候选目录。

逐文件哈希和来源证据：

- `docs/game/design/common/assets/packs/ships_port_props/D18_A05_FILE_HASHES.json`
- `docs/game/design/common/assets/packs/ships_port_props/D18_A05_SOURCE_EVIDENCE.md`
- `docs/game/design/common/assets/packs/ships_port_props/kenney_pirate_kit_ships_port_props.manifest.json`
- `docs/game/design/common/assets/packs/ships_port_props/kenney_building_kit_ships_port_props.manifest.json`

外部缓存：`C:/Users/Administrator/AppData/Local/WhalesIslandReef/D18/cache/`。两个归档共 4,753,570 bytes（约 4.53 MiB），仓库没有保留压缩包或重复解压归档。当前候选目录观察值为 616 文件、6,771,478 bytes（约 6.46 MiB）；其中实际有效载荷 151 FBX + 151 PNG + 3 份许可证/来源文本，共 5,944,606 bytes（约 5.67 MiB），其余为 Unity `.meta`。本次执行对既有候选逐文件字节哈希核验后接管，未重复下载、复制或用缩放伪造变体；因此本次恢复执行新增候选有效载荷为 0 bytes，但下载和证据归档均已完成。

## 交付与导入结果

- 真实可复用候选：151（Pirate 72 + Building 79），满足至少 150。
- 实际下载并通过原始 Unity 导入预检：151，满足至少 60。
- 预览/联系表：`game/tools/results/D18/D18-A05/D18_A05_CONTACT_SHEET.png`，10 列缩略图，151 项。
- manifest 审计：`game/tools/results/D18/D18-A05/D18_A05_MANIFEST_AUDIT.json`，2 manifests，错误 0。
- Unity 审计：`game/tools/results/D18/D18-A05/D18_A05_IMPORT_AUDIT_SUMMARY.json`，退出码 0。
- Unity 指标：151 模型、231 Mesh、27 Animation Clip、161 材质、151/151 有效 Bounds、0 缺失引用、0 缺失脚本、0 错误、151 警告。
- 警告解释：151 个原始 Kenney 文件名含连字符，未符合项目 snake_case 命名约定；不影响导入、Bounds、材质或引用。没有把警告降为错误，也没有自动改名覆盖来源。
- 缺材质/异常比例：审计没有发现缺材质或无效 Bounds；所有扫描候选每个记录 1 个材质。比例、Pivot、轴向和碰撞仍需消费者预检确认。

详细尺寸和覆盖：[D18_A05_DIMENSION_AND_COVERAGE.json](../../../../../../game/tools/results/D18/D18-A05/D18_A05_DIMENSION_AND_COVERAGE.json)。

## 船舶字段和尺寸

下表是原始 FBX Bounds，单位按消费者约定为米；不是运行时船体裁决，也不是可驾驶承诺。

| 文件 | 用途 | X × Y × Z（m） | Mesh | 记录结论 |
|---|---|---:|---:|---|
| `ship-small.fbx` | 小船视觉船体 | 4.800 × 9.964 × 8.800 | 4 | CandidateOnly |
| `ship-pirate-small.fbx` | 小船变体 | 4.800 × 9.964 × 8.800 | 4 | CandidateOnly |
| `ship-medium.fbx` | 中型船/远景 | 4.800 × 9.964 × 10.600 | 6 | Candidate |
| `ship-pirate-medium.fbx` | 中型海盗变体 | 4.800 × 9.964 × 10.600 | 6 | Candidate |
| `ship-large.fbx` | 大型船/远景 | 4.800 × 9.964 × 13.100 | 6 | Candidate |
| `ship-pirate-large.fbx` | 大型海盗变体 | 4.800 × 9.964 × 13.100 | 6 | Candidate |
| `ship-ghost.fbx` | 远景剪影 | 4.800 × 9.964 × 10.600 | 6 | Candidate |
| `ship-wreck.fbx` | 损坏视觉变体 | 4.800 × 10.749 × 10.600 | 16 | Candidate |
| `boat-row-small.fbx` | 小交通艇 | 2.749 × 0.792 × 2.372 | 2 | Candidate |
| `boat-row-large.fbx` | 大交通艇 | 2.749 × 0.792 × 2.852 | 2 | Candidate |
| `mast.fbx` | 桅杆 | 4.386 × 7.864 × 1.740 | 3 | Candidate |
| `mast-ropes.fbx` | 桅杆/绳索 | 4.386 × 7.888 × 2.349 | 3 | Candidate |

船舶检查字段：

- 船体尺寸：已记录上述 Bounds；`cannon`、`cannon-mobile`、`cannon-ball` 另有尺寸记录在尺寸 JSON。
- 甲板高度、吃水参考：原始 FBX Bounds 无法安全推导，留给 Hull Prefab/ShipSystem 消费者定义水线、龙骨和甲板基准。
- Pivot、前向轴：目标约定是 Y-up/Z-forward，但原始审计不暴露 Pivot；D21 必须逐 Prefab 复核。
- 可拆模块：`mast`、`mast-ropes`、划艇、船体被登记为可拆候选；船帆为船体整合视觉，没有独立帆文件。
- 登船点、舵位：原始资源没有交互锚点或独立舵，必须由 D21 消费者 Prefab 补齐。
- 碰撞和 LOD：原始 FBX 为 0 Collider、0 LOD；本任务不偷偷生成正式 Prefab，故保持未完成。
- 动画可能性：船舶查询为 0 个动画 Clip，属于静态视觉候选，不声明可驾驶或已绑定动画。

## 港口建筑和道具检查

- 建筑真实候选 100：墙、半墙、低墙、角件、柱、门洞、窗洞、地板、屋顶、排水、楼梯、塔和结构模块；可支持木屋/仓库/市场的拼装原型。
- 网格闭合、门窗比例、地基、室内可能性、碰撞、拼接尺寸：原始 FBX 审计没有足够证据，全部保留为 Candidate，待 D19/D21 消费者组装预检。Building Kit 的门/窗/墙/地板/屋顶类别已在 manifest 分角色登记。
- 码头/栈桥：`structure-platform-dock-small`、`structure-platform-dock`、`platform`、`platform-planks`、`structure-platform-small`、`structure-platform`。
- 道具真实候选 18：桶、箱、瓶、宝箱、炮、炮弹、旗帜、桨、铲等；每个原始候选审计为 1 个嵌入材质。
- 抓取/摆放 Pivot、碰撞、批处理/实例化：未由原始 FBX 审计证明；消费者侧必须补交互点和 Collider，再决定静态批处理/实例化。

## 未完成项及原因

1. 独立帆、舵、锚、系船柱：两套已核验包没有明确模型；不以船体缩放或命名重写伪造。
2. 独立灯具、渔具、招牌：没有明确文件；旗帜只作为标识/招牌风格候选，不替代实体道具。
3. 消费者 Prefab：碰撞、LOD、交互锚点、Pivot、轴向、甲板、吃水、水线、建筑网格闭合和拼接仍待 D19/D21；本任务约束禁止创建正式 Prefab 和运行时接入。
4. 当前没有把任何候选接入 InitialHarbor、ShipSystem、BuildingSystem、Registry 或场景。

## D19 / D21 推荐

### D19

优先选择 `ship-ghost`、`ship-medium`、`ship-small` 和 `structure-platform-dock` 做港口远景与第一轮样板；木质模块优先 `floor`、`wall`、`wall-doorway-*`、`wall-window-*`、`roof-flat-*`、`stairs-*`；近景道具优先 `barrel`、`crate`、`chest`、`flag-pennant`、`tool-paddle`。

### D21

先以 `ship-small` / `ship-pirate-small` 建 Hull Prefab 预检，明确 Pivot、Y-up/Z-forward、甲板、吃水、水线、舵位、登船点和可拆模块，再讨论可驾驶。`ship-ghost` 用于低成本远景，`ship-wreck` 仅用于损坏视觉。独立帆/舵/锚/系船柱/灯具/渔具/招牌另开有证据的资源任务。

## 验证命令和结果

| 验证 | 命令/方式 | 结果 |
|---|---|---|
| manifest 审计 | `python tools/d18_asset_manifest_audit.py --manifest-root docs/game/design/common/assets/packs/ships_port_props --project-root . --output game/tools/results/D18/D18-A05/D18_A05_MANIFEST_AUDIT.json` | PASS，2 manifests，0 errors |
| Unity 模型/材质/Bounds | Unity 6000.0.80f1 batch `RunKenneyAudit`，临时指向本任务候选根，随后恢复 D18-A02 配置 | PASS，151 模型，0 errors |
| 缺失引用/脚本 | Unity 同一审计 | PASS，0/0 |
| 联系表 | 生成 `D18_A05_CONTACT_SHEET.png` 并目视检查 | PASS，151 项 |
| diff 格式 | `git diff --check` | PASS，退出码 0 |

## 边界声明

本任务没有修改主场景、正式船 Prefab、角色/UI 资源、ShipSystem、BuildingSystem、Registry、网络或 LLM 代码，也没有提交或推送 Git。资产最终保持 `Candidate` / `ImportVerified`，整体交付为 `PARTIAL`，供 D19 选择和 D21 消费者预检。
