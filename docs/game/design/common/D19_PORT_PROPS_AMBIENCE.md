# D19-A06 港口道具、环境 VFX 与音景

任务：D19-A06（异步表现模块）  
日期：2026-08-14  
当前结论：**PARTIAL（工作场景资产包完成；Unity 运行时导入和真实音频 Mixer 验证待 D19-S02 / D18-S03）**

## 范围与写者边界

本成果只写入 D19-A06 的工作场景、道具/VFX Prefab、D19 音频候选、测试、结果和文档目录。没有修改 `InitialHarbor`、Terrain/建筑/水体、Registry、存档、Systems、经济/库存/任务/动物生成或天气强逻辑，也没有把任何资源标为 `LocalDevelopmentReady` 或 `ReleaseApproved`。

工作场景：

- `game/Assets/Scenes/D19/Work/D19_Ambience_Work.unity`
- 根层级：`AmbienceRoot`、`PropSetRoot`、`VFXProxyRoot`、`AmbienceZones`
- 音频区采用触发体和事件图，不在场景放置不可控 `AudioSource`
- 正式 `InitialHarbor` 接入仍由 D19-S02 唯一同步集成者负责

## 内容清单

| 类别 | 真实数量 | 结果 | 说明 |
|---|---:|---|---|
| PropSet Prefab | 4 | Candidate | 木桶/货箱/渔网/绳索/工具/货物堆、市场陈列、修船物件、夜间灯具；使用项目自有低模代理和现有 D19 材质 |
| VFX Prefab | 5 | Candidate/Proxy | 烟、尘、火光、远景昆虫/海鸟、轻浪溅水；烟尘/海鸟/溅水为轻量代理，正式 ParticleSystem 接入待后续导入验证 |
| 环境音区 | 5 | Candidate | 港口、市场、海岸、船坞、夜间；每区保留 base/near/far/directional/interaction 层位 |
| 逐项音频候选 | 30 | LicenseEvidenceComplete / ImportPreflight | 3 个 D18 intake-eligible Kenney CC0 包各 10 个；均为短 one-shot 交互候选，不冒充自然环境录音 |
| 静态验证板 | 5 | Static pass | 仅是工作场景布局验证板，明确标注不是 Unity 运行时截图 |
| 自动化测试 | 5/5 | PASS | 路线、碰撞、资源许可、音频元数据、缺少 Unity 的降级状态均可复核 |

## 道具与 VFX

道具来源为 `project_owned_d19_lowpoly_proxy`：Unity 内置 Primitive Mesh 与已有项目自有 D19 港口材质，未导入 D18 Candidate-only 外部模型。所有道具不带实体阻挡碰撞；场景区域的 BoxCollider 均为 Trigger，仅作为未来区域消费者的边界提示。

| asset_id | Prefab | 使用区域 | 距离/对象池策略 |
|---|---|---|---|
| `prop.d19.port.prop_set` | `game/Assets/Prefabs/Props/D19/D19_Port_PropSet.prefab` | port/market/warehouse/dock | 0–12m LOD0；12–32m 低复杂度代理；32–80m 远景代理；离区归还 `D19_PROP_POOL` |
| `prop.d19.market.display` | `game/Assets/Prefabs/Props/D19/D19_Market_Display.prefab` | market | 只表现货台和布棚代理，不创建交易状态；不抢 D19-A02 服务锚点 |
| `prop.d19.shipyard.repair_set` | `game/Assets/Prefabs/Props/D19/D19_Shipyard_RepairSet.prefab` | shipyard/dock | 只表现木料和工具；修理结果由 ShipyardSystem 裁决 |
| `prop.d19.night.lamp_set` | `game/Assets/Prefabs/Props/D19/D19_Night_LampSet.prefab` | port/market/shipyard/night | 仅受控 Point Light；无持续闪烁、无实体碰撞 |

| asset_id | Prefab | 实现状态 | 限制 |
|---|---|---|---|
| `vfx.d19.smoke` | `D19_Smoke_VFX_Proxy.prefab` | Mesh proxy | 等待 Unity 粒子导入预检；最大实例 4 |
| `vfx.d19.dust` | `D19_Dust_VFX_Proxy.prefab` | Mesh proxy | 最大实例 3，避免遮挡 UI |
| `vfx.d19.firelight` | `D19_Firelight_VFX_Proxy.prefab` | Point Light proxy | 最大实例 8；不写天气/火灾逻辑 |
| `vfx.d19.insect_bird_distant` | `D19_InsectBird_Distant_VFX_Proxy.prefab` | Distant marker proxy | 最大实例 6；不生成动物实体 |
| `vfx.d19.sea_spray` | `D19_SeaSpray_VFX_Proxy.prefab` | Shore marker proxy | 最大实例 5；不写水体/天气参数 |

详细字段在 `game/Assets/Prefabs/Props/D19/D19_PROP_VFX_CATALOG.json`，摆放和路线约束在 `game/Assets/Scenes/D19/Work/Data/D19_A06_PLACEMENT_MANIFEST.json`。

## 音频区与逐项许可

D18 准入清单中允许 D19-A06 的资源为：

| source_id | manifest_id | 单项数量 | 许可证据 |
|---|---|---:|---|
| `source_kenney` | `pack_kenney_interface_sounds_v1` | 10 | `game/Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/License.txt`；CC0-1.0 |
| `source_kenney` | `pack_kenney_impact_sounds_v1` | 10 | `game/Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/License.txt`；CC0-1.0 |
| `source_kenney` | `pack_kenney_ui_audio_v1` | 10 | `game/Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/License.txt`；CC0-1.0 |

每个文件的 `intake_id`、`content_id`、源路径、目标路径、SHA-256、采样率、通道、时长、RMS 代理响度、循环点、许可证和导入证据路径均写入：

- `game/Assets/Audio/D19/D19_AUDIO_CANDIDATE_INDEX.json`
- `game/tools/results/D19/D19-A06/D19_A06_STATIC_AUDIT.json`

真实测量：30 个候选、165,769 bytes、44.1kHz 双声道短 one-shot；未设循环点；响度字段是 RMS dBFS 代理，不冒充 LUFS。发现 2 组重复哈希（每组 2 个不同源文件名），审计中保留并标记为源文件到 D19 工作副本的有意重复，未静默去重。

音频区配置：`game/Assets/Audio/D19/D19_AMBIENCE_ZONES.json`。每个区域包含 base/near/far/directional/interaction 层位、Mixer bus、字幕 key、静音回退和视觉事件标识。当前 30 个合法候选仅用于受控交互/脚步代理；海浪、风、木结构、人群、鸟和自然溅水没有逐项确认的合法录音，因此对应 clip 保持 `missing_legal_candidate`，不能用 Kenney UI 音效伪造环境声。

消费策略只记录事件 ID，交由现有 `D7AudioController` / `D9AudioPresentation` 请求并经过 `SettingsStore` 音量/静音设置；D19-A06 没有新增全局 AudioMixer，也没有直接播放的 AudioSource。无声时仍提供 `subtitle_key`、事件日志和视觉标识；重要提示必须压低环境/Diegetic 层。

## 路线、性能与验证

基于 D19-S01 冻结路线 `d19_route_spawn_to_return_07m` 做静态距离检查：9 个摆放点全部 `non_blocking=true`，声明的路线安全余量均通过；最近路线距离由结果脚本重新计算并写入 JSON。区域 Collider 为 Trigger，Prop/VFX Prefab 没有实体阻挡 Collider。

静态预算样本：4 个活动 PropSet 实例、5 个 VFX 代理、5 个区域 Trigger、0 个直接 AudioSource、0 个透明 VFX 材质；对象池和距离衰减字段已冻结，真实 CPU/GPU/GC、AudioImporter、PlayMode、Mixer 总线和持续路线需 Unity 环境在 D19-S02 / D18-S03 验证。

验证输出：

- `game/tools/results/D19/D19-A06/d19_a06_static_audit.py`
- `game/tools/results/D19/D19-A06/D19_A06_STATIC_AUDIT.json`
- `game/tools/results/D19/D19-A06/D19_A06_TEST_RESULTS.md`
- `game/tests/D19/test_d19_a06_ambience.py`
- `game/tools/results/D19/D19-A06/screenshots/` 下 5 张静态工作场景验证板

Unity executable 在当前环境不可用，故 Unity 编译、Unity Test Runner、真实 AudioImporter、PlayMode、实际 Mixer/字幕/视觉替代联动和运行时性能采样均没有伪造为 PASS。
