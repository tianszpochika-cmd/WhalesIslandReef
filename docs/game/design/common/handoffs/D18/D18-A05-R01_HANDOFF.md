# D18-A05-R01 船舶/港口资源证据复审交接

## 结论

**PARTIAL**（2026-08-15）

本任务为只读复审，不修改 InitialHarbor、ShipSystem、BuildingSystem、正式 Prefab、候选源、全局 intake catalog、Registry、存档、contracts/server/web 或 Git 历史。

## 真实复审数

- 2 个 manifest：Kenney Building Kit 79 模型、Kenney Pirate Kit 72 模型。
- 151 个真实模型逐项复审，151 张官方预览存在。
- 302 个模型/预览文件哈希核对：302/302 匹配，0 缺失，0 mismatch。
- 151 个既有 Unity report-only 导入资产记录：151/151；0 errors、0 缺引用、0 缺脚本、0 invalid Bounds；151 个命名 warning。
- 151 个候选当前 `intake_eligible`：**0**。两个 manifest 均以 `review_status=Candidate` 被 D18-S02-v2 排除；`LocalDevelopmentReady=0`、`ReleaseApproved=0`。

## 可供消费的类别

| 类别 | 数量 | D19-R01-B | D21 |
|---|---:|---|---|
| `port_structure_core` | 96 | 可作为 D18-S02-R01 的候选证据；准入后再做工作 Prefab 预检 | 次要港口壳体参考 |
| `dock_and_shipyard_core` | 6 | 可作为码头/船坞缺口证据；需缝合、甲板、碰撞、泊位检查 | 船坞上下文参考 |
| `market_prop_core` | 16 | 可作为市场密度候选证据；需 Pivot/碰撞/批处理检查 | 非主要消费 |
| `ship_visual_candidate` | 15 | 仅远景船上下文，不替换正式船 | D21-A01/A02 review-only 输入 |
| `sail_rig_gap` | 0 | 不适用 | 独立帆缺口 |
| `rudder_anchor_gap` | 0 个独立模型；raw anchors=0 | 不适用 | 独立舵/锚和交互锚点缺口 |

D19-R01-B 与 D21 当前直接可消费数均为 0；“推荐”仅表示证据/工作 Prefab 评审价值，不表示已获准入。

## 关键缺口

1. D19-S02-R01 仍发现港口、船坞、码头、市场低细节/块状/白色或薄荷色区域；没有 Player-camera 通过证据。
2. 建筑模块的闭合网格、门窗比例、地基、室内可能性、碰撞和拼接尺寸没有 raw FBX 证据。
3. 船体的 Pivot、Y-up/Z-forward、甲板高度、吃水/水线、碰撞、LOD、Boarding/Helm/Mooring/Rudder anchors 和动画没有证据。
4. 包内没有独立帆、舵、锚、系船柱、灯具、渔具和明确招牌；桅杆/绳索不等于帆装完成。
5. 不能用 Quaternius 网页数字代替下载、许可证和哈希证据。

## 证据路径

- 主复审：[D18_A05_R01_SHIP_PORT_REVIEW.md](../../assets/review_packets/D18-A05-R01/D18_A05_R01_SHIP_PORT_REVIEW.md)
- 逐项机器复审：[D18_A05_R01_SHIP_PORT_REVIEW.json](../../assets/review_packets/D18-A05-R01/D18_A05_R01_SHIP_PORT_REVIEW.json)
- 机器结果：`game/tools/results/D18/D18-A05-R01/D18_A05_R01_MACHINE_RESULT.json`
- manifest 审计：`game/tools/results/D18/D18-A05-R01/D18_A05_R01_MANIFEST_AUDIT.json`
- intake 复核：`game/tools/results/D18/D18-A05-R01/D18_A05_R01_INTAKE_AUDIT.json`
- 既有 Unity 导入：`game/tools/results/D18/D18-A05/D18_A02_KENNEY_IMPORT_AUDIT.json`
- 来源/哈希：`docs/game/design/common/assets/packs/ships_port_props/D18_A05_SOURCE_EVIDENCE.md`、`D18_A05_FILE_HASHES.json`

## 验证与后续

- `d18_asset_manifest_audit.py`：PASS，2 manifests，0 errors。
- `d18_prefab_candidate_intake_audit.py`：PASS，25 manifests、1,857 files、1,674 eligible files、0 errors；未写全局 catalog。
- R01 逐项审计：151 模型、302 哈希、0 mismatch、151 导入记录；总任务 PARTIAL 由准入和消费者证据缺口导致。
- `git diff --check`：交付前复核；本任务不提交/推送。

D18-S02-R01 负责决定是否更新准入；D19-R01-B 只能使用更新后的 `intake_eligible` 或项目自有 fallback；D21-A01/A02 负责 Hull/帆装消费者预检；D18-S03 负责最终运行时晋级裁决。

