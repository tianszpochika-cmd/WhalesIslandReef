# D19-S01 Attempt 2 交接报告

## 任务与结论

- 任务编号：D19-S01（重新执行 / Attempt 2）
- 日期：2026-08-14
- 结论：**PASS**
- Attempt 1：**BLOCKED**。原因是旧版 D18-S02 把运行时就绪证据提前放入资源准入门，形成循环依赖；旧报告、旧机器结果和旧布局清点均保留，未篡改为已冻结。
- Attempt 2：基于 **D18-S02 v2 PASS**，完成不改正式场景的空间布局、接口锚点、资源准入和工作边界冻结。

## 修改文件

- `docs/game/design/common/D19_SAINT_MARGARET_WORLD_BUILD_SPEC.md`
- `docs/game/design/common/assets/d19_saint_margaret_layout_v1.json`
- `game/tools/results/D19/D19-S01/D19_S01_ATTEMPT_2_MACHINE_RESULT.json`
- `docs/game/design/common/handoffs/D19/D19-S01_RERUN_HANDOFF.md`
- `.workbuddy/memory/2026-08-14.md`

没有修改 `game/Assets/Scenes/InitialHarbor.unity`、Prefab、运行时代码、Systems、AgentCore、Registry、存档、contracts、server、web、构建设置或资源状态；没有下载资源、提交或推送 Git。旧版 `D19-S01_HANDOFF.md` 与 `D19_S01_READONLY_STRUCTURE_AND_GATE.json` 保持原样。

## 冻结布局与接口

- 世界原点：`SM_PORT_ORIGIN=(0,0,0)`；1 Unity 单位=1 米；Y-up；+Z 向内陆；-Z 向海/出航；海平面 Y=0；布局网格 1m、锚点网格 0.25m；
- 样板范围：X `[-128,128]m`、Z `[-128,144]m`；玩家目标可见 128m、港口总览 256m、远景 384m；
- 关键冻结点：出生 `(-22,0.2,88)`、港口入口 `(0,0.2,112)`、市场 `(26,0.2,62)`、仓库 `(48,0.2,34)`、酒馆 `(-42,0.2,50)`、船坞 `(60,0.2,8)`、主码头 `(0,0.2,-12)`、泊位 `(0,0,-30)`、登船 `(0,0.5,-16.75)`、离港 `(0,0,-78)`、返港 `(0,0,-54)`；
- 环境边界：海滩、道路、植被区、岩坡、浅水/暗礁和深海边界的中心、尺寸、owner task、共享锚点均在布局 JSON；
- 接口保护：`santa_margarita`、`building_saint_marys_dock`、`berth_santa_margarita`、`tavern_santa_margarita`、NPC 及 `D2_PlayerAvatar`、`ph_ship_fishing`、`D7_HelmPoint`、D3/D4 Canvas、`D9PortServicePage`、`D10TavernSocialPage` 等稳定 ID 不变；`Dock`、`DockDeck`、`Gangplank`、`Beach`、`Sea`、`Harbor UI`、`HarborControls` 和 D6 碰撞节点保留兼容策略。

当前 InitialHarbor 只读基线：24 个 GameObject、25 个 PrefabInstance、7 个场景 MonoBehaviour、NavMeshData=`fileID: 0`，场景 SHA-256 为 `D3D76848BDA2E00F3313F111D7E0DBD2B59E8D306C1A681765B67FD5A591932B`。本任务未改变该哈希。

## 资源准入与任务边界

- D19-A01—A06 只能使用 `d18_prefab_candidate_intake_catalog.json` 中 `intake_eligible=true` 的外部资源；S01 不把任何候选晋级 `LocalDevelopmentReady`；
- 可用外部 manifest 只包含准入目录已有的环境、材质、灯光参考和音频包。D19-A02 当前没有 intake-eligible 外部建筑模型，船/舵/帆/锚和角色缺口保持记录，禁止导入 Candidate-only 包；
- D19-A04 的水体/Shader、D19-A05 的灯光/天气表现和 D19-A06 的项目自有 VFX/配置按责任任务创建并记录所有权，不冒充外部资源；
- A01—A06 写入范围互不重叠：A01 地形、A02 港口建筑、A03 植被/生境、A04 水体、A05 灯光/天气、A06 道具/VFX/音频；每个 Work 场景和目录在 JSON 中冻结；
- A01—A06 不得写 InitialHarbor；只有 D19-S02 的指定人类同步集成者可在预集成快照后写正式场景。

## 14 个镜头与验收路线

14 个固定镜头已写入布局 JSON：主菜单、出生点、市场、码头总览、酒馆入口、海岸、登船、船上、离港、近海、天气、返港、设置/无障碍、调试边界。镜头有固定位置、朝向和验收目标；截图和运行时通过不在 S01 虚构。

玩家路线目标 420 秒，允许 300—600 秒：出生 → 海滩 → 市场 → 仓库 → 酒馆入口 → 码头 → 泊位 → 登船 → 离港 → 近海返港 → 下船/保存恢复。每一步的锚点、时长和失败责任在 JSON 中列出。

## D19-S02 集成与回滚

顺序固定为：锁定证据/布局版本 → Terrain → 港口建筑 → 生境植被 → 水体 → 灯光天气 → 道具/VFX/音景 → 14 镜头、路线、保存恢复和性能回归。S02 在首次写场景前必须生成 `d19-s02-preintegration-baseline` 快照；失败时恢复快照、保留失败证据、不得删历史结果或改存档。

## 真实风险与未完成事项

1. `LocalDevelopmentReady=0`、`ReleaseApproved=0` 是当前真实状态；不阻断 S01 布局冻结，但阻止资源发布和最终运行时就绪声明。
2. 当前没有 Prefab、运行时消费者、碰撞、NavMesh、LOD、Animator、PlayMode、截图或保存恢复证据；由 A01—A06、D19-S02 和 D18-S03 产生。
3. D19-A02 缺少 intake-eligible 外部建筑模型；不得以 Candidate-only Kenney 船港包或通用模型冒充。角色、鸟/猫/狗、独立舵/帆/锚也保持 D18 缺口。
4. InitialHarbor 当前 Build Settings 场景条目为 0，NavMeshData 未绑定；本任务只登记风险，不擅自修复构建或场景。
5. 正式主场景唯一写者的人类姓名/账号尚未填写，D19-S02 开工前必须补入快照和交接。

## 验收命令与结果

```text
python -m json.tool docs/game/design/common/assets/d19_saint_margaret_layout_v1.json
  PASS

python <read-only layout validator>
  PASS：D18-S02-v2=PASS；14 shots；route=420s；stable IDs complete；A01-A06 write scopes disjoint；all external manifest IDs intake_eligible；InitialHarbor unchanged

python -c <read-only intake catalog/path/state validation>
  PASS：catalog PASS；LocalDevelopmentReady=0；ReleaseApproved=0；no Candidate-only resource selector

git diff --check
  PASS
```

未启动 Unity，未用截图替代布局或接口验证。D19-S01 Attempt 2 结论为 **PASS**，可按冻结表启动 D19-A01—A06 受控工作场景任务；正式场景集成必须等待 D19-S02，运行时资源晋级必须等待 D18-S03。
