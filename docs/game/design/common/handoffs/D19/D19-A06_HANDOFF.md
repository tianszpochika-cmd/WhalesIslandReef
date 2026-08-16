# D19-A06 交接报告：港口道具、VFX 与音景

任务编号：D19-A06  
日期：2026-08-14  
结论：**PARTIAL**

## 1. 交付范围

已交付 D19 专属工作场景、4 个 PropSet Prefab、5 个 VFX 代理 Prefab、5 个音频区域、30 个逐项许可音频候选、静态路线/碰撞/哈希/音频元数据检查、5 张静态工作场景验证板、测试和文档。没有修改 `InitialHarbor`、Terrain/建筑/水体、Registry、存档、Systems、经济/库存/任务/动物/天气逻辑，也没有写全局 AudioMixer 或正式 UI。

主要文件：

- `game/Assets/Scenes/D19/Work/D19_Ambience_Work.unity`
- `game/Assets/Prefabs/Props/D19/`
- `game/Assets/Prefabs/VFX/D19/`
- `game/Assets/Audio/D19/`
- `game/Assets/Audio/D19/D19_AMBIENCE_ZONES.json`
- `game/Assets/Scenes/D19/Work/Data/D19_A06_PLACEMENT_MANIFEST.json`
- `game/tools/results/D19/D19-A06/D19_A06_STATIC_AUDIT.json`

## 2. 资源来源、许可与单项哈希

视觉代理使用 `source_id=project_owned_d19_lowpoly_proxy`：Unity 内置 Primitive Mesh + 已有项目自有 D19 港口材质，不使用 D18 Candidate-only 外部模型。VFX 为项目自有代理，未宣称正式粒子表现。

音频候选均来自 D18 intake-eligible 清单，包级来源为 `source_kenney`，许可证均为 `CC0-1.0`；本地许可证凭据保留在 `game/Assets/Audio/D19/License/`，源许可证路径和 D18 导入证据路径也保留在 `D19_A06_STATIC_AUDIT.json`。每个单项的 content ID、包、目标文件和 SHA-256 如下：

| 单项文件 | 包 manifest | content_id | 许可 | SHA-256 |
|---|---|---|---|---|
| `d19_candidate_footstep_carpet_000.ogg` | `pack_kenney_impact_sounds_v1` | `candidate_audio_impact_sounds_footstep_carpet_000` | CC0-1.0 | `b9524658b52a99a20590f8feefc103088637fd13ea8ec35811aee785e1f2f330` |
| `d19_candidate_footstep_carpet_001.ogg` | `pack_kenney_impact_sounds_v1` | `candidate_audio_impact_sounds_footstep_carpet_001` | CC0-1.0 | `9af3c3f76705107339e72036e4053bdca3eaa50ad31c8c929f42ce8d40e5d781` |
| `d19_candidate_footstep_carpet_002.ogg` | `pack_kenney_impact_sounds_v1` | `candidate_audio_impact_sounds_footstep_carpet_002` | CC0-1.0 | `9af3c3f76705107339e72036e4053bdca3eaa50ad31c8c929f42ce8d40e5d781` |
| `d19_candidate_footstep_carpet_003.ogg` | `pack_kenney_impact_sounds_v1` | `candidate_audio_impact_sounds_footstep_carpet_003` | CC0-1.0 | `4ae7454338d57c7d102145919ae653c7d010eb542797c4f25a1c7470be3a7e4c` |
| `d19_candidate_footstep_carpet_004.ogg` | `pack_kenney_impact_sounds_v1` | `candidate_audio_impact_sounds_footstep_carpet_004` | CC0-1.0 | `4ae7454338d57c7d102145919ae653c7d010eb542797c4f25a1c7470be3a7e4c` |
| `d19_candidate_footstep_concrete_000.ogg` | `pack_kenney_impact_sounds_v1` | `candidate_audio_impact_sounds_footstep_concrete_000` | CC0-1.0 | `d7267e183067757c92c169de2a379abea592cfca6b39bb2e8feea15221ad79fe` |
| `d19_candidate_footstep_concrete_001.ogg` | `pack_kenney_impact_sounds_v1` | `candidate_audio_impact_sounds_footstep_concrete_001` | CC0-1.0 | `507a75eb19b897087855a3be3cf11a12d7fa37d143ad535eeebe0b8031c7325d` |
| `d19_candidate_footstep_concrete_002.ogg` | `pack_kenney_impact_sounds_v1` | `candidate_audio_impact_sounds_footstep_concrete_002` | CC0-1.0 | `b21fa546d74941196b25da8a20b7010abd8c05c4fcf93545d1f344e66a664e13` |
| `d19_candidate_footstep_concrete_003.ogg` | `pack_kenney_impact_sounds_v1` | `candidate_audio_impact_sounds_footstep_concrete_003` | CC0-1.0 | `8b513c2f0316324060b9174b8f28df6c2147418c506e6a53b77f18e149a5547b` |
| `d19_candidate_footstep_concrete_004.ogg` | `pack_kenney_impact_sounds_v1` | `candidate_audio_impact_sounds_footstep_concrete_004` | CC0-1.0 | `24dd8db2413e5b81ad181e1205c12a1c7801da5f85715d471b92a4b2f632e45e` |
| `d19_candidate_back_001.ogg` | `pack_kenney_interface_sounds_v1` | `candidate_audio_interface_sounds_back_001` | CC0-1.0 | `07db973f79f6ae0f2edc34561e7592e24d0577455919fb602cb8ecc0da991dcf` |
| `d19_candidate_back_002.ogg` | `pack_kenney_interface_sounds_v1` | `candidate_audio_interface_sounds_back_002` | CC0-1.0 | `61581c58194e3f19f531072edabbc344204c7e0a2887b8ededce4357bcf09195` |
| `d19_candidate_back_003.ogg` | `pack_kenney_interface_sounds_v1` | `candidate_audio_interface_sounds_back_003` | CC0-1.0 | `ff5e87de1230b1a5f1fa83e5a4ba9d7eb1c07154267ac34b510a17d4caa03cef` |
| `d19_candidate_back_004.ogg` | `pack_kenney_interface_sounds_v1` | `candidate_audio_interface_sounds_back_004` | CC0-1.0 | `c22e3c49f42c2a5ba6748e91283efe124f32822469c1258c3e5d7b3d7ab9361c` |
| `d19_candidate_bong_001.ogg` | `pack_kenney_interface_sounds_v1` | `candidate_audio_interface_sounds_bong_001` | CC0-1.0 | `d21d0f0b782445db579d11e2506b24cd1ac9d664ee33aeaf807761aa7b6fd710` |
| `d19_candidate_click_001.ogg` | `pack_kenney_interface_sounds_v1` | `candidate_audio_interface_sounds_click_001` | CC0-1.0 | `ccfb7fa0cccdd9faec0eb16033c732b1e308d139d80f799161495d58f7adcdb9` |
| `d19_candidate_click_002.ogg` | `pack_kenney_interface_sounds_v1` | `candidate_audio_interface_sounds_click_002` | CC0-1.0 | `adcd1f4adc35f1b41bc1b5bbefeff7aa44f2f3f0d96d3199b544140c7c1e761c` |
| `d19_candidate_click_003.ogg` | `pack_kenney_interface_sounds_v1` | `candidate_audio_interface_sounds_click_003` | CC0-1.0 | `2fa929138bc3a0f432696588f66eac94b8f6d463905ec8808c6461ce4b054292` |
| `d19_candidate_click_004.ogg` | `pack_kenney_interface_sounds_v1` | `candidate_audio_interface_sounds_click_004` | CC0-1.0 | `09e2c1b643b71b9aff9932b4e02d78db116446b9a14b00a5b2a9b28d2668fd8e` |
| `d19_candidate_click_005.ogg` | `pack_kenney_interface_sounds_v1` | `candidate_audio_interface_sounds_click_005` | CC0-1.0 | `a3ee7ebb036bc3cdc218ae52bb27548c4f49bcf07505274f8b61b1af8236acc2` |
| `d19_candidate_click1.ogg` | `pack_kenney_ui_audio_v1` | `candidate_audio_ui_audio_click1` | CC0-1.0 | `59175ac17cd49a68dd736285738441287636112a84a6f7ce0d89921bda5a5360` |
| `d19_candidate_click2.ogg` | `pack_kenney_ui_audio_v1` | `candidate_audio_ui_audio_click2` | CC0-1.0 | `c394b46bc6737f7f7edfbfa3031ce621b7a43d498d456d6198364eee9cd3e306` |
| `d19_candidate_click3.ogg` | `pack_kenney_ui_audio_v1` | `candidate_audio_ui_audio_click3` | CC0-1.0 | `e3e7fc7ccd9c5cfdf77bffa05aee10d4973812076c76dc889ba2b16ea434127f` |
| `d19_candidate_click4.ogg` | `pack_kenney_ui_audio_v1` | `candidate_audio_ui_audio_click4` | CC0-1.0 | `9c02bbfc872b50eb8750adf4b7c343120f19f5d2f04ab06a7a8948368a77b872` |
| `d19_candidate_click5.ogg` | `pack_kenney_ui_audio_v1` | `candidate_audio_ui_audio_click5` | CC0-1.0 | `52d41e0bb012731bf2a391d53ffb52265ef21589c7701e66b391d5e36a65b293` |
| `d19_candidate_mouseclick1.ogg` | `pack_kenney_ui_audio_v1` | `candidate_audio_ui_audio_mouseclick1` | CC0-1.0 | `ece1210c62ccae6c9ea4f4c59466d9d453d6b357c7d4b1a6c31196e5e6fea1bf` |
| `d19_candidate_mouserelease1.ogg` | `pack_kenney_ui_audio_v1` | `candidate_audio_ui_audio_mouserelease1` | CC0-1.0 | `23cb78cea76d8f1bb1266af228101a09b5d62063f4ac87ead15b1d395c6181f0` |
| `d19_candidate_rollover1.ogg` | `pack_kenney_ui_audio_v1` | `candidate_audio_ui_audio_rollover1` | CC0-1.0 | `4eb47e2e1540c44847b47a89825e21335c820a4e54cacf8c4c1eb5100e36af42` |
| `d19_candidate_rollover2.ogg` | `pack_kenney_ui_audio_v1` | `candidate_audio_ui_audio_rollover2` | CC0-1.0 | `d0bc0fcba496d73df1da8c145e5330df75f87ab5a4f33a9b71461b006b5c338e` |
| `d19_candidate_rollover3.ogg` | `pack_kenney_ui_audio_v1` | `candidate_audio_ui_audio_rollover3` | CC0-1.0 | `641df2c95468f1f035ff0ae964129da10543b4ea8ff6c13f66bcbd899dcfc113` |

重复哈希检查发现两组：`9af3...e5d781` 对应 carpet_001/002，`4ae7...a7e4c` 对应 carpet_003/004；这是源文件实际同哈希，保留两项来源记录，不把重复当成许可缺失。

## 3. 音频/VFX/性能问题

- 真实数量：4 PropSet、5 VFX、5 音区、30 音频候选、165,769 bytes 音频二进制副本、5 静态验证板。
- 本次目标文件/结果/文档/记忆的限定统计为 577,428 bytes；其中 `game/Assets/Audio/D19` 为 227,862 bytes（含 Unity `.meta` 和许可证），`game/tools/results/D19/D19-A06` 为 225,433 bytes。该统计不包含工作区其他 D19 任务文件，也不等同于 Git 最终增量。
- 音频：通道、44.1kHz、时长、循环点和 RMS dBFS 代理已测；LUFS、Unity AudioImporter、压缩质量、MixerGroup 实际绑定未测。
- 环境层缺口：无逐项确认的海浪、风、木结构、人群、鸟、自然溅水录音，zone graph 对这些层保留 `missing_legal_candidate`；不得使用 UI 音效冒充自然环境。
- VFX：烟、尘、远景昆虫/海鸟、轻浪溅水为代理网格/标记，正式粒子导入和透明度审计待 Unity；火光为受控 Point Light，不闪烁、不遮挡 UI。
- 性能：静态预算为 4 活动 PropSet、5 VFX 代理、5 Trigger、0 直接 AudioSource、0 透明 VFX 材质；真实 CPU/GPU/GC、对象池运行时回收、距离衰减和 Mixer ducking 未测。

## 4. 路线与安全检查

基于 `d19_route_spawn_to_return_07m`，9 个摆放点全部 `non_blocking=true`，静态最近路线距离全部通过；Prop/VFX Prefab 没有实体 Collider，场景区 Collider 全部为 Trigger。结果脚本和逐点数值在 `D19_A06_STATIC_AUDIT.json` 的 `route_checks` 中。

5 张验证板：

- `D19_A06_01_port.png`
- `D19_A06_02_market.png`
- `D19_A06_03_coast.png`
- `D19_A06_04_shipyard.png`
- `D19_A06_05_night.png`

它们是静态工作场景布局板，不是 Unity PlayMode 截图，已在文件内标明。

## 5. 验证与未完成项

- Python 静态审计：PASS。
- Python 单元测试：5/5 PASS。
- `git diff --check`：最终交接前执行。
- Unity 编译/导入/Test Runner/PlayMode：当前机器没有 Unity executable，记录为 `BLOCKED_UNAVAILABLE`，没有伪造 PASS。
- 正式场景接入、真实 Mixer/字幕/视觉替代、运行时性能和粒子导入：交给 D19-S02 / D18-S03。

## 6. 最终状态

**PARTIAL**。工作场景、道具/VFX 代理、受控事件图、逐项 CC0 交互音和静态安全证据已交付；自然环境音合法候选、Unity 真实导入/运行时验证和正式场景集成仍未完成。不得在本交接点把资源晋级为 `LocalDevelopmentReady` 或 `ReleaseApproved`。
