# D18-A07 Handoff

## 任务

- 任务编号：D18-A07
- 日期：2026-08-14
- 结论：**PARTIAL**
- 结构审计：**PASS**
- 说明：映射已生成且可重复，但 D18 候选尚未有 LocalDevelopmentReady，运行时消费者和 P2/P3 语义候选仍缺，因此不能宣告资源接入完成或 ReleaseApproved。

## 实际修改文件

- `tools/d18_placeholder_replacement_map.py`
- `tools/tests/test_d18_placeholder_replacement_map.py`
- `docs/game/design/common/assets/replacement/d18_placeholder_replacement_map.json`
- `docs/game/design/common/D18_PLACEHOLDER_REPLACEMENT_MAP.md`
- `game/tools/results/D18/D18-A07/D18_A07_MACHINE_RESULT.json`
- `game/tools/results/D18/D18-A07/D18_A07_CANDIDATE_CATALOG.json`
- `game/tools/results/D18/D18-A07/D18_A07_DETERMINISM_RECHECK.md`
- `docs/game/design/common/handoffs/D18/D18-A07_HANDOFF.md`
- `.workbuddy/memory/2026-08-14.md`

没有修改 Registry、`asset_state`、Prefab、场景、正式资源、Systems 或候选 manifest。

## 数据边界

权威输入是 Registry 中 `asset_state == placeholder` 的 2,264 条记录；候选输入是 25 个 D18 manifest 的 1,857 个 `candidate_files`。每条输出记录均保存 content ID、placeholder 路径、领域、P0—P3、可见频率、候选 pack/file、候选状态、许可证证据、消费者、碰撞/锚点/LOD/动画需求、负责阶段和阻断原因。

工具只写输出映射和结果，不写实体数据。`planned_candidate` 不改变 Registry 状态；同一候选被多条记录使用时记录为明确的 `many_to_one_reuse`，不得视为独立模型；岛屿模块组合记录为 `one_to_many_composition`。

## 真实统计

| 指标 | 结果 |
|---|---:|
| 输入 placeholder | 2,264 |
| 输出记录 | 2,264 |
| planned_candidate | 1,107 |
| unmapped | 1,157 |
| blocked_candidate_evidence | 0 |
| 合法复用组 | 35 |
| 非法一对多 | 0 |
| 重复 content ID | 0 |
| 候选文件断链 | 0 |
| 许可证证据断链 | 0 |
| 非法映射状态 | 0 |
| 重复写者 | 0 |
| 运行时消费者缺口 | 303 |
| LocalDevelopmentReady | 0 |
| ReleaseApproved | 0 |

分阶段：P0 `5/5/0`（总数/有候选/未映射）；P1 `548/535/13`；P2 `500/32/468`；P3 `1211/535/676`。

## P0 摘要

- `asset_ph_crew_male` → Kenney Blocky Characters vertical-slice `character-a.fbx`；
- `building_saint_marys_dock` → Pirate Kit vertical-slice `structure-platform-dock-small.fbx`；
- `building_small_boat_pier` → Pirate Kit vertical-slice `structure-platform-small.fbx`；
- `island_saint_marys` → `patch-sand.fbx` + `rocks-sand-a.fbx` + `palm-detailed-straight.fbx`；
- `crew_d15_a07_preset_001` → Quaternius `crew_quaternius_male_shirt_v01`；
- 玩家、起始船、海水/天气、核心 UI 另列控制点：起始船 `asset_ph_ship_fishing` 已 integrated 且不属于输入；海水/天气在当前候选目录无兼容候选；核心 UI 无 placeholder content ID，只保留 Kenney Input Prompts 候选控制点。

P0 的主要阻断是 Prefab/运行时消费者、碰撞、锚点、LOD、动画绑定和 LocalDevelopmentReady 人工确认，不是候选文件或许可证证据缺失。

## 未映射原因

1. 165 个 Outfit 没有服装专用候选，不能用角色基础模型冒充服装；
2. 食物、鱼获处理、药材、工具等 Item 中有 303 条没有同语义模型，不能用桶、炮或通用道具代替；
3. 植物长尾的具体物种/生长状态超出当前 Kenney Nature Kit 的语义覆盖；
4. D15-A08 catalog 与 vegetation profile 是数据/配置入口，不应伪装成视觉或音频文件；
5. 当前 A03—A06 没有可证明的海面和天气运行时资源。

## 工具与测试结果

```text
python -m unittest tools.tests.test_d18_placeholder_replacement_map -v
Ran 4 tests ... OK

python tools/d18_placeholder_replacement_map.py --project-root . \
  --output-map docs/game/design/common/assets/replacement/d18_placeholder_replacement_map.json \
  --result-dir game/tools/results/D18/D18-A07
input_placeholder_count=2264
output_record_count=2264
mapped_count=1107
unmapped_count=1157
```

全量工具第二次运行的 digest 与第一次相同：
`e908eef063e1019fa19e273cb5234c955913dd781855277408821ac2b5d68c75`，结果为 `stable=True`。单元测试包含确定性、合法多对一、缺候选/缺证据阻断、非法一对多四类夹具。

## 后续责任与门禁

- D19：仅从映射接收岛屿、植被、建筑和 P0 布局候选，并补 Prefab、锚点、碰撞、LOD；
- D20：接收 Crew/Animal/Pet，补角色/动物 Prefab、动画、碰撞和历史/身份审查；
- D21：接收 Fish 与船海控制点，单独补海面/天气候选，不替换已 integrated 的起始船 ID；
- D22：接收 UI、音频、材质候选及核心 UI 控制点，补运行时消费者；
- D23：补齐 Item/Outfit 的真实语义候选；
- D24：完成全量 QA、许可证复核、性能与 ReleaseApproved 人工门禁。

后续 Agent 不得绕过本映射直接下载或接入新文件。只有有责任的人类审核者可以确认 `ReleaseApproved`；工具、Agent 和映射文件不得代签。

最终修订重跑（包含玩家 P0 控制点）：digest `e908eef063e1019fa19e273cb5234c955913dd781855277408821ac2b5d68c75`，`stable=True`；以机器结果和确定性复核文件为最终摘要。
