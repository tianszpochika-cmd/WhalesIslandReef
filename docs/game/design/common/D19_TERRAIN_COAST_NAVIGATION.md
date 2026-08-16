# D19-A01 地形、海岸与导航工作成果

## 结论

**PARTIAL**。

本任务的 D19 Terrain Work 场景已经可构建，并已用 Unity PlayMode 探针证明：地形存在、出生/市场/码头落地射线命中、洞口阻挡射线命中、暗礁碰撞射线命中、NavMesh 注册有效，6 个关键路线采样全部可采样。EditMode 7/7 通过。

仍不能把本任务写成完整 PASS：当前证据是受控 Work 场景探针，不是 D2/D17 真实玩家控制器连续移动的 5—10 分钟 PlayMode 回归；目标设备的 CPU/GPU/GC/峰值内存也尚未采集。因此把正式场景接入、真实玩家行走回归和设备性能采集留给 D19-S02/D19-A06，并保留 D18-S03 的晋级门禁。

## 工作边界与不变项

- 工作场景：`game/Assets/Scenes/D19/Work/D19_Terrain_Work.unity`。
- TerrainRoot Prefab：`game/Assets/Prefabs/Environment/D19/Terrain/D19_TerrainRoot.prefab`。
- 本任务没有修改 `game/Assets/Scenes/InitialHarbor.unity`。复核 SHA-256：`D3D76848BDA2E00F3313F111D7E0DBD2B59E8D306C1A681765B67FD5A591932B`，与 D19-S01 只读基线一致。
- 没有修改 Systems、AgentCore、Registry、存档、Bootstrap、经济公式或正式场景。
- 资源仍不是 `LocalDevelopmentReady`，也不是 `ReleaseApproved`；D19-A01 只消费 D18 准入目录中的 intake-eligible 资源。

## 冻结空间参数

| 项目 | 值 |
|---|---|
| 原点 | `SM_PORT_ORIGIN=(0,0,0)` |
| 单位/轴向 | 1 Unity unit = 1 m；Y-up；+Z inland；-Z sea/departure |
| 海平面 | `Y=0` |
| TerrainRoot transform | `(-128,-8,-128)` |
| Terrain world bounds | X `[-128,128]`；Z `[-128,144]` |
| Terrain size | `[256,32,272] m` |
| Heightmap | `257 x 257` |
| Alphamap/BaseMap | `256 x 256` |
| 可见验收分辨率 | `1280 x 720`；D18 目标基线为 1080p |
| Terrain visual mesh | 4,485 vertices；8,704 triangles；无 Collider，仅作表现层 |
| NavMesh source mesh | 65 x 69 网格；4,485 vertices；8,704 triangles |
| NavMesh agent | 默认 Humanoid；radius `0.35`；height `1.8`；slope `38°`；climb `0.35` |

高度函数包含海岸波动、港口缓坡、道路压平、内陆高点和岩坡；海床在海岸外侧降至约 `-7 m`，以便浅滩/暗礁/深海边界可以与海上模块继续对齐。可视网格与 TerrainCollider 使用同一高度函数，表现层不拥有玩法碰撞。

## 地表与候选资源

TerrainLayer 和表现网格分为 Sand、Rock、Grass、Mud 四层；Sand/Mud 使用已准入的 Poly Haven 纹理，Grass 使用 D19-A01 项目自有颜色材质，岩石表现使用已准入的 Kenney Nature Kit 模型和 Poly Haven Coral/Mud 纹理。

本场景实际引用的 D18 manifest ID：

- `pack_kenney_nature_kit_d18_a03_v1`：`cliff_corner_rock.fbx`、`rock_largeF.fbx`、`rock_tallF.fbx`、`ground_pathBend.fbx`、`bridge_wood.fbx`、`cliff_stepsCorner_stone.fbx`。
- `pack_polyhaven_sand_01_diff_jpg_d18_a03_v1`：`sand_01_diff_1k.jpg`。
- `pack_polyhaven_coral_mud_01_diff_jpg_d18_a03_v1`：`coral_mud_01_diff_1k.jpg`。

外部 FBX 保持源引用，不复制成独立逻辑资源；每个实例在 Work Prefab 内补 MeshCollider 仅用于该样板的代表性环境碰撞。没有使用候选目录外的模型，也没有把缺少的海水、天气、建筑、角色、船舵/锚/帆等候选冒充为已就绪资源。

## Prefab 与所有权

| Prefab | 责任 | 说明 |
|---|---|---|
| `D19_TerrainRoot` | D19-A01 | Terrain、TerrainCollider、无碰撞可视网格、四层表现材质 |
| `D19_TerrainCollision` | D19-A01 | 暗礁、岩坡排除、洞内阻挡、北/西/东边界、深海边界 |
| `D19_TerrainAnchors` | D19-A01 | 恢复点、海滩、浅水线、植被边界、道路净空、岩坡、暗礁、航道 |
| `D19_CaveFissure` | D19-A01 | 岩缝/洞口样板；入口置于冻结高度函数的岩坡表面 |
| `D19_TerrainSurfaceSet` | D19-A01 | 6 个 intake-eligible Kenney 环境模型实例及代表性 MeshCollider |
| `D19_TerrainPresentation` | D19-A01 | Work 相机与方向光；不拥有 HUD 或玩法 UI |
| `D19_TerrainNavMesh` | D19-A01 | 注册 `D19_SaintMargaret_NavMesh.asset` 的运行时组件 |
| `D19_TerrainRuntimeProbe` | D19-A01 | 只读落地、碰撞、NavMesh 和路线探针，不写玩法状态 |

正式集成时根节点顺序固定为：`D19_TerrainRoot` → `D19_TerrainCollision` → `D19_TerrainAnchors` → `D19_CaveFissure` → `D19_TerrainSurfaceSet` → `D19_TerrainPresentation` → `D19_TerrainNavMesh` → `D19_TerrainRuntimeProbe`。D19-S02 只能把这些 Prefab 作为受控集成单元接入，并保留回滚到上一版 InitialHarbor 的入口。

## 坐标与碰撞接口

### 共享锚点

| 稳定锚点 | Work 坐标 m | 用途 |
|---|---:|---|
| `anchor_player_recovery` / `D17_PlayerRecovery` | `(-22,0.2,82)` | 越界/跌落恢复参考 |
| `anchor_beach_landing` / `BeachGround` | `(-58,0.2,-8)` | 海滩登陆 |
| `anchor_shallow_waterline` | `(-16,0,-48)` | 浅水线 |
| `anchor_vegetation_boundary` | `(-72,0.5,54)` | 植被边界 |
| `anchor_road_clearance` | `(2,0.55,52)` | 道路净空 |
| `anchor_cliff_exclusion` | `(72,6,72)` | 岩坡禁行参考 |
| `anchor_reef_boundary` | `(-16,-1.5,-66)` | 暗礁边界 |
| `anchor_channel_safe` | `(0,-2,-96)` | 安全航道 |

### 代表性碰撞

- `D19_ReefCollision`：暗礁横向阻挡，保留在 A01 TerrainCollision 中；PlayMode 已用射线验证。
- `D19_CliffExclusion`：岩坡排除体，随冻结高度函数落在岩坡区域；不替代 Navigation/Ship 强逻辑。
- `D19_CaveInteriorBlocker`：洞内代表阻挡体；PlayMode 已用入口射线验证。
- `D19_BoundaryNorth/West/East` 与 `D19_DeepSeaBoundary`：安全边界/深海边界；越界恢复仍由 D17/玩法系统裁决。
- TerrainCollider：唯一地形落地几何来源；可视网格无 Collider，避免表现层制造第二份地面事实。

## 导航与路线证据

NavMesh 不是从未经验证的正式场景自动接管，而是由冻结 `TerrainHeight` 生成 Work 专属低分辨率 source mesh，再由 `UnityEngine.AI.NavMeshBuilder` 烘焙到 `D19_SaintMargaret_NavMesh.asset`。运行时 Prefab 注册后报告 `valid=True`。

探针采样路线顺序如下，作为 D19-S02 真实玩家控制器回归的固定入口：

1. `D17_PlayerRecovery`：`(-22,82)`；
2. `BeachGround`：`(-58,-8)`；
3. 市场服务点参考：`(26,62)`；
4. 仓库服务点参考：`(48,34)`；
5. 酒馆入口参考：`(-42,50)`；
6. `building_saint_marys_dock`：`(0,-12)`。

该顺序对应约 5—10 分钟步行/登船验收路线的几何采样版本；真实玩家控制器的连续移动、台阶跨越、跌落恢复和上/下船仍须在 D19-S02/A06 运行。当前机器结果已记录 6/6 NavMesh samples、出生/市场/码头落地射线 PASS，但不能替代完整路线回归。

## 验证结果

- Unity Work 场景构建：PASS；结果见 `game/tools/results/D19/D19-A01/D19_A01_BUILD_RESULT.json`。
- EditMode：7/7 PASS；结果见 `game/tools/results/D19/D19-A01/D19_A01_EDITMODE_TESTS_FINAL2.xml`。
- PlayMode Work 探针：PASS；落地射线 3/3、洞口阻挡射线、暗礁射线、NavMesh 注册和路线采样 6/6 均通过；结果见 `game/tools/results/D19/D19-A01/D19_A01_PLAYMODE_SMOKE.json`。
- 截图：5 张、1280x720，覆盖全景、海岸、道路、洞口、暗礁；索引见 `game/tools/results/D19/D19-A01/D19_A01_SCREENSHOTS.json`。
- 性能粗测：已记录静态资产/几何预算；结果见 `game/tools/results/D19/D19-A01/D19_A01_PERFORMANCE.json`。真实 CPU/GPU/GC/峰值内存留待目标设备采集。
- `git diff --check`：PASS（仅有 Git 的 CRLF 转换提示，无 whitespace error）；工作区不提交、不推送。

## 未决项与回滚

1. D19-S02 必须使用真实 D2/D17 控制器完成连续路线、坡度、台阶、洞口、跌落恢复和上/下船回归；失败时回滚到 D19 Work 场景上一版 Prefab，不修改保存字段或强逻辑公式。
2. D19-S02/A06 必须在目标设备采集 CPU/GPU/GC/内存和同屏实体；超限时只降低表现网格/LOD/流式范围，不删除 TerrainCollider、NavMesh、锚点或事件事实。
3. D18-S03 之前不能把本成果中的资源状态提升为 `LocalDevelopmentReady`，也不能接入正式 InitialHarbor 或标记 `ReleaseApproved`。
4. D2 当前控制器对历史 `D6_HarborCollision`/`BeachGround`/`DockGround`/`GangplankGround` 命名的兼容关系仍由 D19-S02 进行真实集成确认；A01 只提供锚点和代表碰撞，没有改 D2 公式。
