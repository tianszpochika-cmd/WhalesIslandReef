# D18-A05 船舶与港口道具资源包

> 任务：D18-A05（2026-08-14）
> 
> 结论：`PARTIAL`。本包提供 151 个真实、可复核的 CC0 模型候选和 151 张官方预览，原始 FBX 导入预检通过；不接入运行时，不创建正式船 Prefab，不声明 ReleaseApproved。

## 交付范围

候选资源只写入 `game/Assets/Art/Candidates/ShipsPortProps/`，包 manifest 和来源证据只写入 `docs/game/design/common/assets/packs/ships_port_props/`。两套归档保留在外部缓存，不进入仓库：`C:/Users/Administrator/AppData/Local/WhalesIslandReef/D18/cache/`。

| 包 | 官方页 | 许可证 | 归档 SHA-256 | 真实模型 | 官方预览 | 导入状态 |
|---|---|---|---|---:|---:|---|
| Kenney Pirate Kit | [官方页面](https://kenney.nl/assets/pirate-kit) | CC0-1.0 | `667ED2CAF92954DDB98F7B7CEDE831FE99AB75063C26B25E23D32715BEE9C943` | 72 | 72 | ImportVerified |
| Kenney Building Kit | [官方页面](https://kenney.nl/assets/building-kit) | CC0-1.0 | `2740EF5772FB5FB3D7AAB881DB22D129F6B68AFE711B1A79E6D5E9E19CF3EEC6` | 79 | 79 | ImportVerified |

许可证证据由每包 `License.txt`、官方页面和 [Creative Commons CC0 1.0 deed](https://creativecommons.org/publicdomain/zero/1.0/) 组成。Quaternius Pirate Kit 已核验官方 CC0 页面，但没有下载或把网页的 71 Models 数字计入本地数量，因为本次两套 Kenney 包已经提供 151 个真实模型；不以页面数量代替本地哈希和导入证据。

详细文件级哈希：[D18_A05_FILE_HASHES.json](assets/packs/ships_port_props/D18_A05_FILE_HASHES.json)；来源和下载记录：[D18_A05_SOURCE_EVIDENCE.md](assets/packs/ships_port_props/D18_A05_SOURCE_EVIDENCE.md)。

## 数量、磁盘和审计

- 候选模型：151（Pirate 72 + Building 79），达到“至少 150 个模型或模块”。
- 已下载并完成 Unity 原始模型预检：151，达到“至少 60 个样板子集”。
- 候选预览：151；联系表：[D18_A05_CONTACT_SHEET.png](../../../../game/tools/results/D18/D18-A05/D18_A05_CONTACT_SHEET.png)。
- 外部归档下载：2 个，共 4,753,570 bytes（约 4.53 MiB）；仓库不保留压缩包和解压副本。
- 当前候选目录观察值：616 个文件、6,771,478 bytes（约 6.46 MiB），其中 151 FBX、151 PNG、3 份许可证/来源文本和 Unity `.meta`。模型/预览有效载荷为 305 个文件、5,944,606 bytes（约 5.67 MiB）。本次恢复执行采用字节哈希核验并接管既有候选，未重复下载或复制变体。
- manifest 审计：[D18_A05_MANIFEST_AUDIT.json](../../../../game/tools/results/D18/D18-A05/D18_A05_MANIFEST_AUDIT.json)：2 manifests，错误 0。
- Unity 审计：[D18_A05_IMPORT_AUDIT_SUMMARY.md](../../../../game/tools/results/D18/D18-A05/D18_A05_IMPORT_AUDIT_SUMMARY.md)：151 模型、231 Mesh、27 Animation Clip、161 材质、Bounds 151/151、缺失引用 0、错误 0、警告 151。
- 151 条警告都是原始 Kenney 连字符文件名未符合项目 snake_case 命名约定；未发现导入、Bounds、材质、脚本或引用错误。

尺寸与覆盖明细：[D18_A05_DIMENSION_AND_COVERAGE.json](../../../../game/tools/results/D18/D18-A05/D18_A05_DIMENSION_AND_COVERAGE.json)。

## 船舶尺寸参考

单位按消费者约定为米；以下是原始 FBX Bounds，不是可驾驶性能承诺。原始资产没有 Collider、交互锚点、LOD 或独立运行时动画，Pivot、Y-up/Z-forward、甲板基准和吃水必须在 D21 Hull Prefab 预检中落定。

| 候选 | 角色 | X × Y × Z (m) | Mesh |
|---|---|---:|---:|
| `boat-row-large` | 划艇/交通艇 | 2.749 × 0.792 × 2.852 | 2 |
| `boat-row-small` | 小划艇/交通艇 | 2.749 × 0.792 × 2.372 | 2 |
| `ship-small` | 小船视觉船体 | 4.800 × 9.964 × 8.800 | 4 |
| `ship-pirate-small` | 小船海盗变体 | 4.800 × 9.964 × 8.800 | 4 |
| `ship-medium` | 中型船/远景 | 4.800 × 9.964 × 10.600 | 6 |
| `ship-pirate-medium` | 中型海盗变体 | 4.800 × 9.964 × 10.600 | 6 |
| `ship-large` | 大型船/远景 | 4.800 × 9.964 × 13.100 | 6 |
| `ship-pirate-large` | 大型海盗变体 | 4.800 × 9.964 × 13.100 | 6 |
| `ship-ghost` | 远景剪影 | 4.800 × 9.964 × 10.600 | 6 |
| `ship-wreck` | 损坏船体 | 4.800 × 10.749 × 10.600 | 16 |
| `mast` | 桅杆候选 | 4.386 × 7.864 × 1.740 | 3 |
| `mast-ropes` | 桅杆/绳索候选 | 4.386 × 7.888 × 2.349 | 3 |

另有 `cannon`（1.402 × 1.055 × 2.394m）、`cannon-mobile`（1.679 × 1.380 × 2.394m）和 `cannon-ball`（0.637 × 0.670 × 0.552m）作为船上道具。船体、桅杆、远景船和划艇已记录可拆候选关系；帆、舵、锚没有独立文件，不能假装已满足模块化要求。

## 港口建筑与道具检查

- 建筑 100 个：墙、半墙、低墙、角件、柱、门洞、窗洞、地板、屋顶、排水、楼梯和塔/结构模块。具备木屋、仓库、市场原型的拼装材料，但网格闭合、门窗比例、地基、室内可用性、碰撞和拼接尺寸未在原始 FBX 层面宣称通过；下游必须做消费者侧组装预检。
- 码头/栈桥候选：`structure-platform-dock-small`、`structure-platform-dock`、`platform`、`platform-planks`、`structure-platform-small`、`structure-platform`。
- 道具 18 个：桶、箱、瓶、宝箱、炮、炮弹、旗帜、桨、铲等。材质审计每个原始候选为 1 个嵌入材质；抓取/摆放 Pivot、碰撞和批处理/实例化仍需消费者 Prefab 预检。
- 不存在明确命名的系船柱、灯具、独立渔具、独立招牌、舵、锚和帆；旗帜只能作为标识/招牌风格候选，不替代这些缺口。

## D19 / D21 推荐

### D19 选择优先级

1. 港口远景与第一轮样板：`ship-ghost`、`ship-medium`、`ship-small`、`structure-platform-dock`。
2. 木质港口拼装：Building Kit 的 `floor`、`wall`、`wall-doorway-*`、`wall-window-*`、`roof-flat-*`、`stairs-*`。
3. 近景道具：`barrel`、`crate`、`chest`、`flag-pennant`、`tool-paddle`。

### D21 船舶与运行时前置

1. 先对 `ship-small` 和 `ship-pirate-small` 做 Hull Prefab 预检：Pivot、Y-up/Z-forward、甲板高度、吃水/水线、舵位、登船点和可拆模块。
2. 为小船补碰撞、LOD 和交互锚点后，才评估“可驾驶”；当前只称为视觉船体候选。
3. 为远景船单独做低成本 Prefab；不把 `ship-wreck` 当作可驾驶船体。
4. 独立帆/舵/锚/系船柱/灯具/渔具/招牌需要证据完整的后续资源包；不以缩放同一模型补数量。

## 状态和边界

- 包与文件：`Candidate`。
- 原始导入预检：`ImportVerified`。
- 运行时消费者：空。
- `ReleaseApproved`：未设置。
- 未修改 ShipSystem、BuildingSystem、Registry、场景、正式 Prefab、角色/UI 资源或网络/LLM 代码。
- 总结：`PARTIAL`，原因是独立船舶/港口部件与消费者级碰撞、Pivot、锚点、LOD 仍待下游；数量、来源、许可证、哈希、联系表和原始导入预检已交付。
