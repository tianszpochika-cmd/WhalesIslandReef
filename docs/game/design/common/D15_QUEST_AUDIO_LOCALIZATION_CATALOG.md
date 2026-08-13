# D15-A08* 任务、音乐与本地化目录

## 1. 交付范围

D15-A08* 完成了任务/音频/本地化的内容目录生产，不把内容目录误报为运行时裁决。目录只提供不可变定义、来源、授权、触发条件、审查状态和所属系统入口；QuestSystem、RewardSystem、Faction/Legal、RelationshipSystem、EconomySystem 和音频运行时继续由各自系统写入状态。

| 内容 | 数量 | ID 规则 | 质量/状态 |
|---|---:|---|---|
| 主线章节 | 5 | `quest_d15_mainline_chapter_01`—`05` | Q3，production，100% 目录检查 |
| 支线模板 | 120 | `quest_d15_side_001`—`120` | Q2，production，12 个玩法域各 10 条 |
| 音乐主题 | 30 | `asset_d15_a08_music_theme_01`—`30` | Q2 目录；二进制暂为 placeholder |
| 音频层 | 8 | `asset_d15_a08_audio_layer_{port,voyage,combat,storm,night,faction,poker,relationship}` | Q1 目录占位，需替换至 Q2/Q3 |
| 中英本地化 key | 452 | `content.*` / `audio.*` | `zh-CN` 与 `en-US` 完整配对 |

`asset_d15_a08_catalog` 是 ContentRegistry 可达性根，只引用本批次内容，不拥有任务、奖励、关系、经济或音频裁决。

## 2. 任务目录与 D12 接口

5 个主线章节按航线因果串联：

1. `quest_d15_mainline_chapter_01`：潮线测绘与海图/打捞事实。
2. `quest_d15_mainline_chapter_02`：社群互助与港口交易事实。
3. `quest_d15_mainline_chapter_03`：货单、生产/运输和销售事实。
4. `quest_d15_mainline_chapter_04`：生境观察与资源恢复事实。
5. `quest_d15_mainline_chapter_05`：风暴预警、航行恢复与最后测深事实。

支线按 12 个玩法域分组，每域 10 个模板：潮线测绘、沉船打捞、港口货单、生境守望、社群互助、船坞工单、港法凭据、水手照护、风暴预警、船员誓约、节期礼仪、牌局见闻。模板编号是稳定目录编号，不代表运行时已接受或完成。

每条 `quest_` 记录都包含：

- 至少两个目标，每个目标有 `objective_id`、`description_key`、`target_owner_system`、统一事实事件码、计数和匹配条件；事实必须由所属系统事件提供。
- `event_prefixes` 的 accepted/progress/completed/failed/recovered 五类事件前缀；QuestSystem 只据事件更新自己的任务状态。
- 失败结果码、默认恢复路线和至少两条替代路线。替代路线只说明可接受的事实事件、所属系统和因果，不替代所属系统裁决。
- `reward_request`：稳定的 `request_id_policy`（含 `save_revision`）、来源事实要求、`RewardSystem` owner、审计要求和 `contains_final_values=false`。内容资产不写入最终奖励、金钱、库存、债务、声望或关系。
- `source_refs`、`source_note`、授权字段、`historical_review`、警告/跳过/表现等级和唯一 D8 测试 ID。

## 3. 音乐与音频目录

30 个主题覆盖主菜单、启航港、开阔航程、夜航、风暴、海战、接舷、雨林、洞穴、红树林、贸易港、酒馆、节期/社群、任务锚点、船员静默、结局、渔场、果园、工坊、船坞、救助、势力紧张、私掠、检查、债务/保险、牌局、生态恢复、发现和归航。

8 个层的强制覆盖如下：

| 层 | `audio_event_id` 状态触发 | 总线/回退 |
|---|---|---|
| port | `port_state=open` | Environment / UI 文本与事件日志 |
| voyage | `voyage_phase=underway` | Environment / UI 文本与事件日志 |
| combat | `combat_state=engaged` | Combat / UI 文本与事件日志 |
| storm | `weather=storm` | Weather / UI 文本与事件日志 |
| night | `time=night` | Environment / UI 文本与事件日志 |
| faction | `faction_relation=tense` | Faction / UI 文本与事件日志 |
| poker | `card_game=active` | Diegetic / UI 文本与事件日志 |
| relationship | `relationship_state=conversation` | Relationship / UI 文本与事件日志 |

每个音乐/音频资产带唯一 `audio_event_id`、触发条件、强度、距离、方向性、循环、优先级、`mixer_bus`、`mix_tags`、`subtitle_key`、fallback、来源、授权、审查和加载档案。触发条件是可检查的状态标签，不使用 Agent、网络或 LLM 选择。

仓库当前没有正式录音二进制，因此本批次明确使用 `asset_state=placeholder`、`.gitkeep` 占位引用和 `placeholder_policy`。`game/Assets/Audio/D15_A08_PLACEHOLDER_POLICY.md` 规定正式素材替换门槛；没有把占位资源伪称为 Q3 音频。

## 4. 本地化与审查

本批次本地化文件为：

- `game/Assets/Resources/Localization/D15_A08/zh-CN.json`
- `game/Assets/Resources/Localization/D15_A08/en-US.json`

452 个 key 的计算为：125 条任务 ×（名称 + 两个目标说明）= 375；38 个音乐/音频资产 ×（名称 + 字幕）= 76；目录根名称 1 个。两种 locale 必须拥有完全相同的 key 集；逻辑不读取硬编码 UI 文本。

社群、港法和节期模板带 `ApprovedWithWarning`、`d14_history_context`、`skip_allowed=true` 和 `Omitted` 表现等级；其余目录条目为 `Approved`。所有条目都保留 D14 审查证据路径。未审查或缺来源的内容不能进入本批次 production 目录。

## 5. 检查与后续门槛

专用检查器：

```text
python tools/d15_a08_content_check.py game/Assets/Resources/ContentRegistry
python tools/tests/test_d15_a08_content_check.py
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

本次结果：A08 检查器 PASS（5/120/30/8，452 keys）；缺 key 和孤儿 key 负向测试 PASS；D8 ContentRegistry PASS（2812 entries / 13 prefixes）。

批量内容仍需在 D15-S02/音频 QA 阶段完成：音乐正式素材替换、音频层混音实听、无声/跳过路径验证、每层触发抽样、主线 100% 内容审查、支线至少 12 条抽样、音乐至少 6 条抽样，以及本地化语言审校。目录生产完成不等于这些表现层门槛已完成。
