# D18-A05-R01 船舶与港口资源证据复审

## 结论

**PARTIAL**（2026-08-15）

本复审只读取 D18-A05 的 ShipsPortProps 候选、manifest、来源/哈希包、既有 Unity 导入报告、D18-S02-v2 intake 目录和 D19-S02-R01 视觉 QA。没有修改候选源文件、manifest、全局 intake catalog、InitialHarbor、正式 Prefab、ShipSystem、BuildingSystem、Registry、存档、contracts、server、web 或 Git 历史。

复审确认 151 个真实模型和 151 张预览均可定位；302 个模型/预览文件哈希全部匹配；2 个包的 manifest 审计 PASS；既有 report-only Unity 导入记录覆盖 151/151 模型，0 错误、0 缺失引用、0 缺失脚本。结论仍不能 PASS：两个 ShipsPortProps manifest 当前均为 `review_status=Candidate`，全局 intake 中直接准入数量为 **0**；原始 FBX 没有 Collider、交互锚点或 LODGroup，且没有独立帆、舵、锚、灯具、渔具和明确招牌。

逐项机器证据（151 条 item，含路径、角色、官方 URL、许可证、归档 SHA-256、文件 SHA-256、导入记录、边界和决定）见：

`D18_A05_R01_SHIP_PORT_REVIEW.json`

## 1. 当前白色方块/低细节区域缺口

来源为历史 D19-S02-R01 编辑器截图和人工 QA；这些不是本复审重新拍摄的 Player 证据。

| 区域 | 已有 | 仍缺什么 | 证据 |
|---|---|---|---|
| 港口整体 | 港口结构、屋顶、立面、门、货箱、布料和地形层已有项目自有表现 | 高细节立面变化、密集地面/岸线层次、去除块状代理感；外部候选当前不能直接接入 | `game/tools/results/D19/D19-S02-R01/D19_S02_R01_VISUAL_QA.json`，`d19_shot_market.png` |
| 船坞 | 船坞立面、屋顶、基础和少量码头细节可读 | 船匠棚、帆布工坊、索具/木工服务的视觉语汇，工作区道具密度和 Player 视角证据 | 同上，`d19_shot_dock_overview.png` |
| 码头/泊位 | 护栏、系船柱、货箱和屋顶细节存在 | 不稀疏的甲板边缘、泊位/船的视觉身份、真实 Player 视角；候选本身没有可直接准入的系泊功能模型 | 同上，`dock_shipyard_berth` |
| 市场/主街 | 屋顶、立面、门、货箱、布料已有 | 近景仍块状，周边地面和摊位道具稀疏；需要合法候选或项目自有模块补密度 | 同上，`market_and_main_street` |
| 船视觉 | 151 个静态候选中有小/中/大型船体、远景剪影、划艇、桅杆和绳索 | 正式 Hull Prefab、甲板/吃水/水线、Pivot/轴向、碰撞、LOD、登船/舵/锚点、可驾驶链路、独立帆/舵/锚和 Player 船镜头证据 | D18-A05 handoff、`D18_A05_IMPORT_AUDIT_SUMMARY.json` |

D19-S02-R01 的四个区域仍为 `PARTIAL`，并明确存在大面积低细节地表、明亮白色/薄荷色区域或块状建筑观感；D18-A05 候选不能被用来掩盖缺口，也不能绕过准入门直接写进 D19-R01-B。

## 2. 包、来源、许可和真实计数

| 包 | 官方 URL | 许可证/证据 | 归档 SHA-256 | 模型 | 预览 | 当前状态 |
|---|---|---|---|---:|---:|---|
| Kenney Building Kit | https://kenney.nl/assets/building-kit | CC0-1.0；包内 `License.txt` + 官方页 + CC0 deed | `2740EF5772FB5FB3D7AAB881DB22D129F6B68AFE711B1A79E6D5E9E19CF3EEC6` | 79 | 79 | Candidate / ImportVerified |
| Kenney Pirate Kit | https://kenney.nl/assets/pirate-kit | CC0-1.0；包内 `License.txt` + 官方页 + CC0 deed | `667ED2CAF92954DDB98F7B7CEDE831FE99AB75063C26B25E23D32715BEE9C943` | 72 | 72 | Candidate / ImportVerified |

许可证、下载日期和来源证据：

- `docs/game/design/common/assets/packs/ships_port_props/D18_A05_SOURCE_EVIDENCE.md`
- `docs/game/design/common/assets/packs/ships_port_props/D18_A05_FILE_HASHES.json`
- `game/Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/License.txt`
- `game/Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/License.txt`

本复审没有把 Quaternius Pirate Kit 的网页模型数量计入本地交付，因为没有下载归档和包内文件证据。

## 3. 审计和导入证据

### 3.1 已执行只读审计

```text
python tools/d18_asset_manifest_audit.py \
  --manifest-root docs/game/design/common/assets/packs/ships_port_props \
  --project-root . \
  --output game/tools/results/D18/D18-A05-R01/D18_A05_R01_MANIFEST_AUDIT.json
PASS：2 个 ShipsPortProps manifest，0 errors，0 issues

python tools/d18_prefab_candidate_intake_audit.py \
  --project-root . \
  --output game/tools/results/D18/D18-A05-R01/D18_A05_R01_INTAKE_AUDIT.json
PASS：25 manifests / 1,857 files / 1,674 eligible files / 0 errors；未写 catalog

python game/tools/results/D18/D18-A05-R01/d18_a05_r01_review_audit.py
PARTIAL（复审结论）：151 模型逐项记录，302 哈希核对，0 mismatch，151 既有导入记录，0 intake-eligible ShipsPortProps
```

既有 Unity 只读导入结果：

- `game/tools/results/D18/D18-A05/D18_A02_KENNEY_IMPORT_AUDIT.json`
- `game/tools/results/D18/D18-A05/D18_A05_IMPORT_AUDIT_SUMMARY.json`
- 151 模型、231 Mesh、27 Animation Clip、161 材质、151 有效 Bounds、0 缺引用、0 缺脚本、0 errors、151 warnings。
- 151 warnings 均为原始 Kenney 连字符文件名不符合 snake_case；不是材质、网格、Bounds 或引用错误。
- 原始候选：0 Collider、0 InteractionAnchor、0 LODGroup、无 Prefab、无 runtime consumer。该导入结果证明可导入，不证明可驾驶、可停靠、可交互或可准入。

Unity 导入未在本复审中重新写入 D18-A02 输出目录；R01 通过重新读取逐资产导入记录、核对 151 个模型路径和指标完成只读导入复核，避免越过单一写者目录。

## 4. 消费类别和边界

| 类别 | 真实数量 | 可帮助的任务 | 当前推荐 | 关键边界 |
|---|---:|---|---|---|
| `port_structure_core` | 96 | D19-R01-B 的港口/建筑候选证据；D19 结构组装复审 | 证据推荐，不是直接消费 | D19-R01-B 必须先等 D18-S02-R01 PASS/刷新 intake，再做闭合网格、地基、比例、室内、碰撞、拼接和风格预检 |
| `dock_and_shipyard_core` | 6 | D19-R01-B 的码头/船坞候选证据；D21 仅作船坞视觉上下文 | 证据推荐，不是直接消费 | 甲板高度、接缝、泊位间隙、吃水和系泊碰撞未证实 |
| `market_prop_core` | 16 | D19-R01-B 的市场/港口装饰候选证据 | 证据推荐，不是直接消费 | 抓取/摆放 Pivot、碰撞、批处理/实例化未证实；不写库存或交易事实 |
| `ship_visual_candidate` | 15 | D21-A01/A02 的 Hull/小艇/远景/桅杆/船上道具复审 | D21 review-only 推荐 | 不代表可驾驶、帆装完成、锚泊完成、动画完成或正式 Hull Prefab |
| `sail_rig_gap` | 0 | D21-A02 | 明确缺口 | 没有独立帆 FBX；`mast`/`mast-ropes` 只能作为部分桅杆/索具候选 |
| `rudder_anchor_gap` | 0 个独立模型；151 个 raw item 交互锚点均为 0 | D21-A02/A03 | 明确缺口 | 没有独立舵/锚；舵位、登船点、缆桩和交互锚点必须由消费者 Prefab/适配器提供 |
| `out_of_scope_coastal_environment` | 18 | D19-A01/A03 另行负责 | 本包拒绝消费 | 草、棕榈、岩石、沙地不解决本 R01 的港口/船坞/船视觉核心缺口 |

**当前直接消费数：D19-R01-B = 0，D21 = 0。** 151 个候选都能作为复审证据输入，但两个 manifest 均因 `review_status=Candidate` 被全局 intake 排除；不能把“证据推荐”写成“准入”。

## 5. 船舶候选的尺寸与风险

以下尺寸为既有 Unity 导入 Bounds，单位米，不能替代 D21 HullTemplate 的龙骨、甲板、水线和吃水定义。

| 候选 | Bounds X×Y×Z（m） | 当前可说 | 不能声称 |
|---|---:|---|---|
| `ship-small.fbx` / `ship-pirate-small.fbx` | 4.800×9.964×8.800 | 小船视觉 Hull 候选 | 可驾驶、吃水、舵位、登船点 |
| `ship-medium.fbx` / `ship-pirate-medium.fbx` | 4.800×9.964×10.600 | 中型/远景 Hull 候选 | 正式船型、物理根、LOD |
| `ship-large.fbx` / `ship-pirate-large.fbx` | 4.800×9.964×13.100 | 大型远景 Hull 候选 | 港口泊位适配、航行性能 |
| `ship-ghost.fbx` | 4.800×9.964×10.600 | 远景轮廓候选 | 近景甲板和交互 |
| `ship-wreck.fbx` | 4.800×10.749×10.600 | 损坏视觉候选 | 可驾驶 Hull |
| `boat-row-small.fbx` / `boat-row-large.fbx` | 2.749×0.792×2.372 / 2.749×0.792×2.852 | 小艇/交通艇候选 | 第二套完整小艇物理 |
| `mast.fbx` / `mast-ropes.fbx` | 4.386×7.864×1.740 / 4.386×7.888×2.349 | 桅杆/索具部分候选 | 独立帆、可控升降帆、索具动画 |

D21 消费必须重新确认：Pivot、Y-up/Z-forward、甲板基准、吃水/水线、物理根/视觉根、碰撞体、Boarding/Helm/Rudder/Mooring anchors、LOD、材质和风格。当前船类导入记录为 0 animation clips，所有船舶表现必须当作静态候选。

## 6. 建筑、码头和道具逐项风险

- 建筑 96：墙、门洞、窗洞、柱、地板、屋顶、排水、楼梯和结构模块文件真实存在；闭合网格、门窗比例、基础接地、室内可用性、碰撞和拼接尺寸未由 raw FBX 证明。
- 码头/船坞 6：`structure-platform-dock-small.fbx`、`structure-platform-dock.fbx`、`platform.fbx`、`platform-planks.fbx`、`structure-platform-small.fbx`、`structure-platform.fbx`；不能声称泊位长度/宽度、甲板高度、吃水和缆桩逻辑可用。
- 市场/港口道具 16：桶、箱、瓶、宝箱、旗帜、桨和铲；缺少明确灯具、渔具和招牌；旗帜不是实体招牌替代品。
- 所有 raw item：导入记录无 Collider/InteractionAnchor/LODGroup；模型材质没有缺失报告，但纹理文件没有作为本包独立候选保留，材质批处理/实例化需在消费者 Prefab 侧验证。
- 151 个命名 warning 不得通过重命名源文件来“修复”；应在消费者 Prefab/映射层使用稳定 ID。

完整逐项 `risk_flags`、`recommendation`、路径和哈希在 JSON 中，不以本节的类别汇总替代逐项证据。

## 7. 推荐清单、拒绝清单和未解决资源

### 推荐清单（仅作为证据/工作 Prefab 评审输入）

1. D19-R01-B：优先复审 `port_structure_core`、`dock_and_shipyard_core`、`market_prop_core`；使用项目自有 fallback 消除白/薄荷色地表和块状观感，外部候选只有在当前 intake gate 通过后才可进入受控消费者 Prefab。
2. D21-A01：优先复审 `ship-small.fbx`、`ship-pirate-small.fbx`，只做 Hull Prefab 预检，不宣称可驾驶。
3. D21-A01：`ship-medium.fbx`、`ship-large.fbx`、`ship-ghost.fbx` 可作为中/远景候选；`ship-wreck.fbx` 只能作为损坏视觉。
4. D21-A02：`mast.fbx`、`mast-ropes.fbx` 可作为桅杆/索具部分候选；必须另补帆、舵、锚和可访问状态表现。
5. D19-R01-B：`barrel.fbx`、`crate.fbx`、`chest.fbx`、`flag-pennant.fbx` 可作为市场/港口密度评审样本，但当前不直接接入正式场景。

### 拒绝清单

- 拒绝当前将两个 Kenney 包的任何文件直接放入 D19-R01-B 或 D21 正式运行时：当前 intake-eligible 为 0。
- 拒绝把 raw FBX 当作正式 Prefab：0 Collider、0 anchors、0 LODGroup，且没有消费者级 Pivot/轴向/吃水证据。
- 拒绝把 `mast`/`mast-ropes` 当作已完成帆装；拒绝伪造独立帆、舵、锚、系船柱、灯具、渔具和招牌。
- 拒绝把 18 个 `out_of_scope_coastal_environment` 草/棕榈/岩石/沙地候选用于本复审的港口核心修复；它们归 D19-A01/A03 另行复审。
- 拒绝把 Quaternius Pirate Kit 网页模型数计入本地证据；没有下载归档、逐文件哈希和包内许可快照。
- 拒绝用缩放、改名或复制同一模型增加船型/模块数量。

### 仍缺资源清单

独立帆、舵、锚、系船柱、灯具、渔具、时代明确的招牌/港口标识、船坞工坊工作道具、正式可驾驶 Hull Prefab、碰撞、LOD、甲板/水线/吃水、Boarding/Helm/Mooring/Rudder anchors、船舶动画和 Player 船镜头证据。

## 8. 结果和下一步

- 机器结果：`game/tools/results/D18/D18-A05-R01/D18_A05_R01_MACHINE_RESULT.json`
- manifest 结果：`game/tools/results/D18/D18-A05-R01/D18_A05_R01_MANIFEST_AUDIT.json`
- intake 只读结果：`game/tools/results/D18/D18-A05-R01/D18_A05_R01_INTAKE_AUDIT.json`
- 复审工具：`game/tools/results/D18/D18-A05-R01/d18_a05_r01_review_audit.py`
- Git 格式检查：`git diff --check` 需在交付前复核；不得提交或推送。

下一责任链：D18-S02-R01 只根据本包证据决定是否刷新准入；D19-R01-B 只使用当前 intake-eligible 外部资源或项目自有 fallback；D21-A01/A02 负责船体/帆装消费者预检；D18-S03 最终裁决 `LocalDevelopmentReady`。本复审自身不改变任何资源状态。

