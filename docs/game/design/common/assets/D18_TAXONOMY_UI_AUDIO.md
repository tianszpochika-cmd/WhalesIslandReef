# D18 UI/音频/材质分类词典（D18-L14）

> 冻结日期：2026-08-16
> 任务：`D18-L14`（波次 1，并行；与 L02、L03、L04—L20 并行执行）
> 唯一写入范围：本文件、`game/tools/results/D18/D18-L14/`、`docs/game/design/common/handoffs/D18/D18-L14_LOGIC_HANDOFF.md`、当日记忆日志。
> 本文件是 [D18-L01 基线](../D18_LOGIC_GOVERNANCE_BASELINE.md) 共享字段词典第 6.1 节 `category` 字段在 **UI/音频/材质域**的权威分类来源（与 L11 船舶港口、L12 角色、L13 生态并列）。
> 读取顺序：D18—D24 路线 → LOGIC_TASK_PROTOCOL → D18 章节任务卡 → L01 基线 → 本文；相关只读设计见 [D21 航海/天气逻辑章节](../../../AGENT_DEVELOPMENT_CHAPTER_13_D21.md)、[D22 UI 逻辑章节](../../../AGENT_DEVELOPMENT_CHAPTER_14_D22.md)、[D15-A08 任务/音频/本地化批次](../D15_QUEST_AUDIO_LOCALIZATION_CATALOG.md)。

## 1. 范围与定位

### 1.1 本文定义什么

1. UI 图标/切片（`ui_icon`）、输入提示（`input_prompt`）、音效（`audio_cue`）、音乐（`music`）、材质（`material`）、贴图（`texture`）、HDRI（`hdri`）、天气表现（`weather_presentation`）、字体（`font`）共 9 个**逻辑分类键**；
2. 每类许可字段（L04 标准化口径）与权利边界（L07 四项权利口径）；
3. content_id 与 ResourceKey 约束（前缀、命名、引用目标、禁止项）；
4. 本地化 key 与可访问性标签规则（D22-L07/L14/L15 消费）；
5. 红线：UI/材质/音频分类**不接入 Canvas、Shader、AudioSource 或视觉效果**（第 7 节）；
6. 实际导入、音量、颜色空间、视觉效果的 **Runtime Deferred** 登记（第 8 节）。

### 1.2 本文不做什么（红线）

- 不修改任何真实 catalog、Schema、验证器、Unity、Prefab、场景、Canvas、Registry 或资源文件（基线第 5.3 节全章禁区）；
- 不授予任何资源状态：本词典只是分类规范，`RuntimeApproved`/`ReleaseApproved` 持有者仍为 0；
- 不创建、不接线任何 Canvas / Graphic / Sprite / Material / Shader / AudioSource / AudioMixer / ParticleSystem / 天空盒组件；
- 不统计或声称模型/材质/音频已可用于运行时：本文"现有证据"数字全部来自磁盘真实扫描（见第 10 节），不补猜。

### 1.3 消费者与只读数据源

| 消费者（只读本词典） | 用途 |
|---|---|
| D21-L11 `D21WeatherInputContracts` | 天气输入（季节/风向/风力/降水/能见度/气压）是确定性逻辑输入；`weather_presentation` 只登记表现映射，不裁决天气数值 |
| D21-L12 `D21SeaStateSystem` | 海况/风险由天气输入裁决；本词典不提供任何表现资源裁决 |
| D21-L31 延期登记 | 天气表现（天空/降水/风暴视觉）登记 Runtime 延期 |
| D22-L07 `D22FeedbackCatalog` | 反馈码与本地化 key 消费；UI 资源分类不裁决反馈内容 |
| D22-L14 `D22AccessibilityReadModel` | 缩放/对比/字幕/减少动态效果设置与资源变体标签的静态对应 |
| D22-L15 `D22LocalizationKeyAudit` | 校验反馈/标签不硬编码用户可见字符串 |
| D22-L31 边界审计 | 阻止 Canvas/EventSystem/Unity UI 组件/业务私写（与本文第 7 节一致） |
| D22-L34 延期登记 | 布局、实际按钮、键鼠/手柄、分辨率与可访问性实测 |
| D18-L15/L16/L45 | content_id 关联、ResourceKey、category 枚举一致性核对 |
| D18-L21—L24 来源目录 / D18-L42 编译器 | UI/音频/材质记录分类登记与编译 |

只读数据源（真实磁盘事实）：准入目录 `d18_prefab_candidate_intake_catalog.json`（domain=`ui` 1,224 / `audio` 30 / `material` 87 / `lighting_reference` 4，合计 1,345 条，许可全为 CC0-1.0）；`docs/game/design/common/assets/packs/ui_materials_audio/` 下 7 个 manifest；`polyhaven_selected_assets.json`（20 materials / 4 HDRIs）；`font_license_audit.json`（OFL 政策，1 条既有占位字体）；`game/Assets/Resources/Localization/D15_A08/`（452 个本地化 key）。

## 2. 逻辑分类词典（category 字段权威取值——UI/音频/材质域）

> **双层分类契约（D18-L51）**：L02 记录中 `category` 永远是 11 值资源大类（`ui`/`audio`/`material`/`lighting_reference`/`environment`）；下表是**领域细分类（`taxonomy_category`）**的权威词典。细分类不得直接写入 `category`；本域来源目录当前实际使用大类（`ui`/`audio`/`material`/`lighting_reference`），未使用下表细分类作为 `taxonomy_category`——若未来使用，记录必须写 `category=<大类>` + `taxonomy_category=<细分类>`，且细分类必须登记在机器映射 [d18_taxonomy_category_map.json](../../../../../tools/schemas/d18_taxonomy_category_map.json)。下表细分类与 L02 大类的映射关系见 2.1 `schema_category_map`（与 `d18_taxonomy_category_map.json` 的 `parent_category` 语义一致）；L45 自动核对。

| 分类 | 精确定义 | 现有真实证据（2026-08-16） | 允许资源类型 | 唯一写者/owner | D21/D22 使用边界 | 缺失/弃用规则 |
|---|---|---|---|---|---|---|
| `ui_icon` | UI 图标与九宫格切片（面板边框、按钮、滑条、箭头、勾选、分页、导航反馈、星标等） | intake `ui_slice_or_navigation_icon` 868 条（Kenney UI Pack 2.0，CC0-1.0，包内 License.txt） | PNG 图片描述器（L26/L29）；不直接引用二进制 | D22 UI 逻辑层（只读 key）；来源目录任务登记 | D22-L07 反馈码/图标 key 只读引用；**不接线 Canvas/Sprite**（第 7 节） | 缺候选/许可/哈希 → Pending（L04/L06/L07 未过不得 IntakeEligible）；不因缺图标伪造占位为正式资源 |
| `input_prompt` | 输入提示图（键鼠 / Xbox Series 风格键帽视觉提示） | intake `input_prompt_keyboard_mouse` 257 + `input_prompt_xbox_series` 99 = 356 条（Kenney Input Prompts 1.5A，CC0-1.0） | PNG 图片描述器 | D22 UI 逻辑层（只读）；来源目录登记 | 提示图引用**输入动作 ID**（D22-L11 Escape 意图、D22-L12 指针模式只读），不内嵌键帽文本、不替换输入绑定（D18-A06 第 3 节口径） | Xbox 提示只作通用视觉提示，不得声称替代当前输入绑定；缺提示图 → 显示文本回退，不伪造键帽 |
| `audio_cue` | 短音效（按钮点击、界面音、脚步/碰撞 impact） | intake `audio_candidate` 30 条（Kenney UI Audio 10 + Interface Sounds 10 + Impact Sounds 10，均 CC0-1.0，10 OGG 每包） | OGG 音频描述器（L29）；不直接引用二进制 | D22 音频反馈逻辑（只读 key）；来源目录登记 | D22-L07 反馈码可引用 cue key；**不接线 AudioSource/AudioMixer**，不声明音量/衰减（第 7、8 节） | 缺 cue/许可 → Pending；不得把“可下载/可试听”解释为可商用/可再分发（L07） |
| `music` | 音乐/主题/氛围曲（非短音效） | **0 条外部候选**；D15-A08 目录 38 个音乐/音频**逻辑条目**（`asset_d15_a08_music_theme_*`）是 Registry 内容条目，不是外部候选文件；不补猜 | 音频描述器；逻辑引用 `asset_` 内容条目 | D22 音频反馈逻辑（只读）；来源目录登记 | 音乐触发/循环/优先级由 D15-A08 逻辑目录字段（`audio_event_id`/`mix_tags`/`subtitle_key`）描述；**音量/混音 Runtime 延期** | 无外部音乐候选时保持空登记；不得把内容条目写成候选文件或已接入 |
| `material` | PBR 材质贴图组（albedo/normal/roughness/ao map 组合） | intake `material` 87 条（Poly Haven 20 材质 79 个 map 文件 + 沙滩/海床材质 8 条；全部 CC0-1.0） | JPG/PNG 贴图描述器（L27） | L42 编译器（登记）；表现层只读 | **不创建 Material 实例、不接 Shader/管线**（第 7 节）；颜色空间/压缩/导入设置全部 Runtime（第 8 节） | AO 缺失的单项不得伪装为完整四图集（D18-A06 第 4 节口径）；缺 map → Pending |
| `texture` | 独立贴图（非 PBR 组件、非 UI 切片的贴图，如装饰贴花） | **0 条独立候选**（当前全部贴图已按 PBR map 或 UI 切片归类）；不补猜 | 贴图描述器（L27） | L42 编译器（登记）；表现层只读 | 同 `material`：不接 Shader/材质实例 | 无独立候选 → 保持空类；不把 UI 切片或 PBR map 重分类为 texture |
| `hdri` | HDRI 天空/光照参考（天空盒与光照烘焙参考） | intake `lighting_reference/hdri_sky_reference` 4 条（Poly Haven HDRI 4，CC0-1.0，单项页面/API 与本地哈希逐条保留） | HDR 文件描述器（L27） | L42 编译器（登记）；表现层只读 | 只作光照参考登记；**不接线天空盒/光照组件**（第 7 节） | 单项许可证据必须逐条保留（站点许可不能替代单项证据）；缺单项证据 → Pending |
| `weather_presentation` | 天气表现（Calm/Rain/Storm 的天空/降水/风暴视觉与配套音效的**组合映射**） | **0 条独立候选**；天气状态（`Calm`/`Rain`/`Storm`）由 D21-L11 输入契约定义，表现映射全部 Runtime 延期 | 无独立二进制；组合引用 `hdri`/`texture`/`audio_cue` | D21 天气逻辑（状态裁决）；表现层无裁决权 | D21-L11/L12 只读输入与海况裁决；`weather_presentation` 不裁决能见度/风力/事件概率（第 6 节） | 任一组合资源不许可 → 该天气表现不接入；Calm-only 现状如实保留（D19-R03 历史事实），不伪造 Rain/Storm |
| `font` | 字体（UI 文本渲染用字体文件） | **0 条新候选**；既有占位 `ph_font_noto_sans_cjk_sc.otf`（`game/Assets/Resources/Fonts/`，OFL 审查 PASS_WITH_SCOPE：仅 OFL 或显式商业嵌入许可合格，不覆盖既有正式资源） | OTF/TTF 字体描述器（L27） | 来源目录登记；D22 本地化读模型只读 | 字体嵌入/随游戏分发须满足 OFL 许可证文本随附（第 3 节）；**不接 Text/TextMeshPro 组件** | 非 OFL/无商业嵌入许可的字体不得接入；占位字体不是正式字体的事实保留 |

### 2.1 机器可读词典（与上表一致，L15/L16/L45 可据此校验）

```json
{
  "schema_version": "1.0",
  "document": "docs/game/design/common/assets/D18_TAXONOMY_UI_AUDIO.md",
  "task_id": "D18-L14",
  "frozen": "2026-08-16",
  "scope": "ui_audio_material",
  "schema_category_map": {
    "ui_icon": "ui",
    "input_prompt": "ui",
    "font": "ui",
    "audio_cue": "audio",
    "music": "audio",
    "material": "material",
    "texture": "material",
    "hdri": "lighting_reference",
    "weather_presentation": "environment"
  },
  "categories": [
    {"category": "ui_icon", "definition": "UI 图标与九宫格切片", "schema_category": "ui", "owner": "D22 UI 逻辑层（只读）", "runtime_deferred": ["import", "canvas_wiring", "sprite_assembly", "nine_slice_scale"]},
    {"category": "input_prompt", "definition": "输入提示图（键鼠/Xbox 风格）", "schema_category": "ui", "owner": "D22 UI 逻辑层（只读）", "runtime_deferred": ["import", "input_binding_display", "canvas_wiring"]},
    {"category": "audio_cue", "definition": "短音效（按钮/界面/impact）", "schema_category": "audio", "owner": "D22 音频反馈逻辑（只读）", "runtime_deferred": ["import", "volume", "mixer_wiring", "audiosource_wiring"]},
    {"category": "music", "definition": "音乐/主题/氛围曲", "schema_category": "audio", "owner": "D22 音频反馈逻辑（只读）", "runtime_deferred": ["import", "volume", "loop", "priority", "mixer_wiring"]},
    {"category": "material", "definition": "PBR 材质贴图组（albedo/normal/roughness/ao）", "schema_category": "material", "owner": "L42 编译器（登记）", "runtime_deferred": ["import", "colorspace", "compression", "shader_wiring", "material_instance"]},
    {"category": "texture", "definition": "独立贴图（非 PBR 组件/UI 切片）", "schema_category": "material", "owner": "L42 编译器（登记）", "runtime_deferred": ["import", "colorspace", "shader_wiring"]},
    {"category": "hdri", "definition": "HDRI 天空/光照参考", "schema_category": "lighting_reference", "owner": "L42 编译器（登记）", "runtime_deferred": ["import", "skybox_wiring", "lighting_bake"]},
    {"category": "weather_presentation", "definition": "天气表现组合映射（Calm/Rain/Storm）", "schema_category": "environment", "owner": "D21 天气逻辑（状态裁决）", "runtime_deferred": ["visual_effects", "skybox", "precipitation", "lighting"]},
    {"category": "font", "definition": "字体（UI 文本渲染）", "schema_category": "ui", "owner": "来源目录登记", "runtime_deferred": ["import", "embedding_verification", "text_component_wiring"]}
  ],
  "intake_role_category_map": {
    "ui_slice_or_navigation_icon": "ui_icon",
    "input_prompt_keyboard_mouse": "input_prompt",
    "input_prompt_xbox_series": "input_prompt",
    "audio_candidate": "audio_cue",
    "pbr_map_albedo": "material",
    "pbr_map_normal": "material",
    "pbr_map_roughness": "material",
    "pbr_map_ao": "material",
    "beach_material": "material",
    "beach_material_normal": "material",
    "beach_material_roughness": "material",
    "beach_material_ao": "material",
    "seabed_material": "material",
    "seabed_material_normal": "material",
    "seabed_material_roughness": "material",
    "seabed_material_ao": "material",
    "hdri_sky_reference": "hdri"
  },
  "music": {"external_candidates": 0, "note": "D15-A08 的 38 个音乐/音频逻辑条目是 Registry 内容条目，不是外部候选文件"},
  "weather_presentation": {"external_candidates": 0, "states": ["Calm", "Rain", "Storm"], "note": "天气状态由 D21-L11 输入契约定义；表现映射全部 Runtime 延期"},
  "font": {"external_candidates": 0, "placeholder": "ph_font_noto_sans_cjk_sc.otf", "policy": "仅 OFL 或显式商业嵌入许可合格；不覆盖既有正式资源"},
  "license_fields": ["license_id", "license_url", "license_evidence_path", "attribution_required"],
  "rights_fields": ["commercial_use", "modification_allowed", "repository_storage", "distribution_with_game"],
  "content_id_rules": {
    "formal_prefix": "asset_",
    "intake_prefixes": ["candidate_ui_", "candidate_input_", "candidate_audio_", "candidate_material_"],
    "forbidden_prefixes": ["ui_", "audio_", "material_", "hdri_", "music_", "font_", "texture_"],
    "id_pattern": "小写 ASCII snake_case，文件名与 ID 完全一致",
    "reference_targets_must_exist": true
  },
  "resource_key_rules": {
    "logical_format": "d18:<分类键>:<content_id>",
    "examples": ["d18:ui_icon:asset_ui_kenney_button_rectangle_border", "d18:audio_cue:asset_audio_ui_audio_click4", "d18:material:asset_material_wooden_planks_albedo"],
    "forbidden": ["磁盘路径", "显示名", "Unity 对象引用", "绝对路径", "候选文件路径", "Resources 运行时路径"]
  },
  "localization_rules": {
    "no_hardcoded_user_visible_strings": true,
    "key_namespace": "ui.*（规则预留）与 audio.*（既有事实）",
    "audio_key_format": "audio.<batch>.<asset_id>.<field>",
    "subtitle_key_required_for_audio_cue": true,
    "accessibility_label_key_required_for_ui_icon_and_input_prompt": true,
    "input_prompt_references_input_action_id_not_keycap_text": true
  },
  "accessibility_variants": ["contrast_variant", "motion_reduced_variant", "subtitle_key", "accessibility_label_key"],
  "red_lines": ["no_canvas_wiring", "no_shader_wiring", "no_audiosource_wiring", "no_visual_effects"],
  "missing_rules": ["id_absent_or_mismatch", "category_not_in_enum", "license_or_hash_or_evidence_gap", "license_not_cc0_without_review", "material_map_set_incomplete", "hdri_per_item_evidence_absent", "font_not_ofl_or_explicit_embedding_license"],
  "deprecation_rules": ["append_only_record", "no_new_references_after_deprecation", "rename_preserves_old_id"]
}
```

## 3. 许可字段与权利边界

### 3.1 通用字段（L04/L07 口径，与基线 6.1、L02 Schema 一致）

每类每条记录必须显式登记（缺一不可，未知/冲突/NC 保守阻断，不猜测 CC0）：

| 字段 | 语义 | 说明 |
|---|---|---|
| `license_id` | 规范化许可证标识（L04） | 当前 intake 全部 1,345 条为 `CC0-1.0`（真实事实，2026-08-16） |
| `license_url` | 官方许可证链接（https） | 静态记录，不访问网络验证（L05 口径） |
| `license_evidence_path` | 包内许可证证据相对路径 | 必须在包内（L10 校验）；Kenney 包内 `License.txt`、Poly Haven 逐项证据（`polyhaven_cc0_evidence_2026-08-14.txt`） |
| `attribution_required` | 是否必须署名（L09 校验） | CC0-1.0 → false；任何"必须署名"声明必须带作者/展示位置（L09） |
| `rights` 四项 | `commercial_use`/`modification_allowed`/`repository_storage`/`distribution_with_game` | 每项显式记录（L07）；"免费/可下载"不等于可商用/可再分发 |

### 3.2 分类特殊规则

| 分类 | 特殊许可规则 |
|---|---|
| `audio_cue` / `music` | 随游戏分发必须 `distribution_with_game=true`；NC 或"个人使用"限制 → 直接阻断（L07）；署名要求按 L09；音量/混音不是许可字段（Runtime） |
| `font` | 仅 OFL 或显式商业嵌入许可合格（font_license_audit 政策事实）；OFL 要求许可证文本随字体分发、不得单独转售字体文件；既有占位字体不是正式字体，不覆盖 |
| `material` / `texture` | 颜色空间/压缩/导入设置不是许可字段；AO 缺失的单项不得伪装为完整四图集 |
| `hdri` | 站点许可不能替代单项证据；单项页面/API 与本地哈希逐条保留（polyhaven_selected_assets.json 事实） |
| `weather_presentation` | 无独立许可；继承组合资源许可，任一组合资源不许可 → 该天气表现不接入 |

## 4. content_id 与 ResourceKey 约束

### 4.1 content_id 约束（与 D8 注册表一致，不新增前缀）

1. intake 层 content_id 保持既有 `candidate_ui_*` / `candidate_input_*` / `candidate_audio_*` / `candidate_material_*`（准入目录真实事实，L21—L24 登记时保留，不重写）；
2. 正式逻辑记录 `canonical_asset_id` 只允许 `^asset_[a-z0-9_]+$`（L02 Schema 强制，业务键；如 `asset_ui_kenney_button_rectangle_border`、`asset_audio_ui_audio_click4`、`asset_material_wooden_planks_albedo`）；
3. **禁止新增** `ui_`、`audio_`、`material_`、`hdri_`、`music_`、`font_`、`texture_` 等第二套前缀（D8 13 前缀 + D15_CONTENT_PRODUCTION_FREEZE 口径）；
4. ID 必须：小写 ASCII snake_case、全局唯一、文件名与 ID 完全一致、改名不改变 ID；
5. `content_ids` 引用目标必须已登记（D8 第 5 节）；`asset_refs` 目标必须为 `asset_` 前缀；
6. 记录不得引用候选路径（`Assets/Art/Candidates/...`）作为 content_id 或 evidence（L15/L17 守卫）。

### 4.2 ResourceKey 约束

1. 逻辑 ResourceKey 格式：`d18:<分类键>:<content_id>`（如 `d18:ui_icon:asset_ui_kenney_button_rectangle_border`、`d18:audio_cue:asset_audio_ui_audio_click4`、`d18:material:asset_material_wooden_planks_albedo`、`d18:hdri:asset_hdri_green_point_park`），稳定、唯一、可迁移（L16 验证器执行）；
2. **禁止**用磁盘路径、显示名、Unity 对象引用、绝对路径、候选文件路径或 `Resources` 运行时路径（如 `Fonts/ph_font_noto_sans_cjk_sc.otf`）作 ResourceKey；
3. 运行时 `Resources` 路径与导入路径是 Runtime 阶段实现细节：本词典只冻结逻辑键规则，不冻结或声称路径下已有资源。

## 5. 本地化与可访问性标签

### 5.1 本地化（D22-L15 强制，不硬编码用户可见字符串）

- 用户可见字符串一律走本地化 key（D15-A08 既有事实：452 keys，`content.*` 与 `audio.d15_a08.asset_*.subtitle/name`，zh-CN/en-US 双 locale 完整配对）；
- 音频 key 格式：`audio.<batch>.<asset_id>.<field>`；`audio_cue`/`music` 必须有 `subtitle_key`（既有 `audio.*.subtitle` 事实）；
- `ui.*` 命名空间为 UI 资源 key 的规则预留（`ui.<batch>.<asset_id>.<field>`，如替代文本/标签），**当前真实 key 集不包含 UI 资源键，不补猜数量**；
- `input_prompt` 引用输入动作 ID（D22-L11/L12 只读输入逻辑），键帽标签由输入逻辑+本地化解析，**不内嵌键帽文本**；
- D22-L15 审计校验反馈/标签不硬编码；本词典不定义具体 key 内容（D22-L07/L32 独占实现）。

### 5.2 可访问性标签（D22-L14 读模型静态对应）

| 标签字段 | 含义 | 适用分类 |
|---|---|---|
| `accessibility_label_key` | 可本地化的替代文本（图标/提示图的文字替代） | `ui_icon`、`input_prompt`（必填） |
| `contrast_variant` | 标准/高对比变体引用（静态字段，不裁决显示设置） | `ui_icon`、`input_prompt`、`material` |
| `motion_reduced_variant` | 减少动态效果变体引用（无快速扫掠/闪烁版本） | `ui_icon`、`audio_cue` |
| `subtitle_key` | 字幕/文字替代 key | `audio_cue`、`music`（必填） |

- 本词典只定义标签**字段与取值规则**，不产生可访问性裁决；缩放/对比/字幕/减少动态效果的实际设置与实测属 D22-L14/L34 Runtime 范畴。

## 6. 与 D21/D22 的接口（只声明接口，不实现）

| 下游任务 | 本词典提供的输入 | 接口边界 |
|---|---|---|
| D21-L11 天气输入契约 | `weather_presentation` 状态枚举（Calm/Rain/Storm）只作逻辑状态引用 | 天气输入（季节/风向/风力/降水/能见度/气压）由 D21 裁决；表现分类不产生任何天气数值 |
| D21-L12 海况系统 | 不消费表现分类 | 海况/风险/事件概率由天气输入映射裁决 |
| D22-L07 反馈目录 | `ui_icon`/`audio_cue` 分类键与 key 格式 | 反馈码引用资源 key；**反馈内容由 D22 裁决，资源分类不裁决** |
| D22-L14 可访问性读模型 | 第 5.2 节变体标签字段 | 标签是静态对应，设置实测延期 |
| D22-L15 本地化审计 | 第 5.1 节 key 规则 | 校验不硬编码用户可见字符串 |
| D22-L31 边界审计 | 第 7 节红线清单 | 阻止 Canvas/EventSystem/Unity UI 组件/业务私写 |
| L15/L16/L45 | 2.1 JSON 词典 | content_id 存在性、ResourceKey 规则、category 枚举一致性 |

## 7. 红线：不接入 Canvas、Shader、AudioSource 或视觉效果（冻结）

> 与 D22-L01（Runtime Canvas 隔离）、D22-L31（边界审计）一致。本逻辑阶段及后续登记只产生**分类与描述器**，任何运行时接线都禁止：

| 分类组 | 禁止（不得写入/接线） | 允许（静态） |
|---|---|---|
| `ui_icon` / `input_prompt` / `font` | Canvas、Graphic/RectTransform 组件装配、Sprite/Image 组件引用、Text/TextMeshPro 组件 | 图片/字体描述器（L26/L27/L29）、分类键、许可/权利字段、本地化/可访问性标签 |
| `material` / `texture` / `hdri` | Material 实例、MaterialPropertyBlock、Shader/URP 管线声明、天空盒/光照组件、压缩/颜色空间设置 | 贴图/HDRI 描述器（L27）、map 组合完整性、许可/权利字段 |
| `audio_cue` / `music` | AudioSource/AudioMixer 装配、音量/衰减/空间化参数、循环/优先级声明为"已生效" | 音频描述器（L29）、触发用途、`subtitle_key`/`mix_tags`（既有逻辑目录字段口径）、许可/权利字段 |
| `weather_presentation` | 天空盒、雾、粒子、降水、风暴视觉效果、光照组件 | 组合引用（hdri/texture/audio_cue）、状态枚举引用、D21 只读输入 |

- 上述"允许"列全部是**静态元数据/描述器**；任何接入行为必须等到 Runtime 阶段并经对应章节门禁。
- 音量、颜色空间、压缩、导入设置、视觉表现不是分类字段；在分类记录中出现即视为越界（L17/L18/L36/L38 守卫）。

## 8. Runtime Deferred 登记（实际导入/音量/颜色空间/视觉效果，全部延期）

| 延期项 | 说明 | 相关分类 |
|---|---|---|
| 实际导入（PNG/OGG/JPG/HDR/OTF → Unity 资源） | L26—L29 只做描述器静态验证；真实导入属 Runtime | 全部 |
| 音量/衰减/空间化/混音 | 不接线 AudioSource/AudioMixer；参数运行时设置 | `audio_cue`、`music` |
| 颜色空间/压缩/导入设置 | sRGB/线性、压缩格式、Mipmap、最大尺寸由 Runtime 导入管线设置 | `material`、`texture`、`hdri` |
| Canvas/Sprite/Text 组件装配与九宫格缩放 | D22 布局/分辨率实测延期（D22-L34） | `ui_icon`、`input_prompt`、`font` |
| Shader/Material 实例与 URP 管线 | Runtime 阶段；描述器只静态验证 | `material`、`texture` |
| 视觉效果（天空盒/雾/粒子/降水/风暴） | Calm-only 现状保留，Rain/Storm 表现未制作 | `weather_presentation` |
| 字体嵌入验证（OFL 文本随附） | 占位字体不覆盖；正式字体选择延期 | `font` |
| 可访问性实测（缩放/对比/字幕/减少动态效果） | D22-L14/L34 | 全部 |
| 天气表现与 D21 状态的实际绑定 | 天气状态由 D21 裁决，表现映射未接线 | `weather_presentation` |

## 9. 明确未宣称

- 本词典没有统计任何模型/材质/音频"已运行时可用"；`LocalDevelopmentReady=0`、`ReleaseApproved=0`、L 系列 `RuntimeApproved` 无持有者（基线 1.5/2.6 节）。
- 第 2 节所有 intake 引用都是 Candidate 登记；没有任何候选资源被写为已接入 Canvas、Shader、AudioSource 或视觉效果。
- 本词典不修改 ContentRegistry、`asset_state`、存档 Schema、`contracts/`、`server/`、`web/`、`game/Assets/Resources/Fonts/`；不引用旧 D18-A/S/R 结论作为通过依据（基线第 4 节）。D18-A06 历史包（SUPERSEDED）只作候选文件与许可证据的只读数据源。

## 10. 数据事实附录（2026-08-16 磁盘盘点，不补猜）

- 准入目录（D18-S02-R02 快照）UI/音频/材质域：domain=`ui` 1,224（`ui_slice_or_navigation_icon` 868 / `input_prompt_keyboard_mouse` 257 / `input_prompt_xbox_series` 99）、`audio` 30（`audio_candidate`）、`material` 87（PBR map 79 + 沙滩/海床 8）、`lighting_reference` 4（`hdri_sky_reference`）；合计 1,345 条，许可全为 `CC0-1.0`。
- 包 manifest（`packs/ui_materials_audio/`，全部 CC0-1.0）：kenney_ui_v2 868 / kenney_input_prompts_v15 356 / kenney_ui_audio_v1 10 / kenney_interface_sounds_v1 10 / kenney_impact_sounds_v1 10 / polyhaven_ui_materials_selected_v1 79 / polyhaven_hdri_sky_references_v1 4。
- Poly Haven 选集：20 material IDs（79 JPG，map 含 albedo/normal/roughness/ao）、4 HDRIs（`hdri_green_point_park` 等）；逐项页面/API 与本地哈希保留在 `polyhaven_selected_assets.json`。
- 字体：`font_license_audit.json` status=`PASS_WITH_SCOPE`，政策"仅 OFL 或显式商业嵌入许可合格；不覆盖既有正式资源"；1 条记录 `font_noto_sans_cjk_sc_existing_v1`（既有占位 `ph_font_noto_sans_cjk_sc.otf`）。
- 本地化：`game/Assets/Resources/Localization/D15_A08/` 452 keys（`content.*` + `audio.d15_a08.asset_*.subtitle/name`），zh-CN/en-US 双 locale key 集完全一致。

## 11. 本任务实际执行的验证（与结果 JSON 一致）

1. **分类与约束机器检查**（Python 一次性脚本，临时目录不落仓库）：解析 2.1 JSON 词典，断言 9 分类与上表一一对应、schema_category_map 命中 L02 Schema 枚举、intake_role_category_map 与准入目录真实 role 逐条核对（1,345 条无一遗漏）、前缀规则与 D8 13 前缀一致、ResourceKey 格式与禁止项存在、本地化/可访问性标签字段齐全、红线清单存在；
2. **真实数据交叉核对**（Python 只读扫描）：准入目录 domain/role 计数、7 个 manifest file_count、polyhaven 20/4、font audit、452 本地化 key 与本文第 10 节一致；
3. **相对链接存在性检查**（Python，同 L03/L12 方法）；
4. `git diff --check`；
5. `python tools/sync_logic_task_status.py`。

## 12. 链接索引

- 路线与协议：[D18—D24 纯逻辑完成路线](../../../D18_D24_VISUAL_PLAYABLE_ROADMAP.md)、[单任务逻辑开发协议](../../../LOGIC_TASK_PROTOCOL.md)
- 基线与本章：[D18-L01 逻辑治理基线](../D18_LOGIC_GOVERNANCE_BASELINE.md)、[D18 章节任务卡](../../../AGENT_DEVELOPMENT_CHAPTER_10_D18.md)、[L02 资源逻辑记录 Schema](../../../../../tools/schemas/d18_logic_asset_record.schema.json)
- 平行词典：[L11 船舶港口](D18_TAXONOMY_SHIPS_PORT.md)、[L12 角色](D18_TAXONOMY_CHARACTERS.md)、[L13 生态](D18_TAXONOMY_ECOLOGY.md)
- D21/D22 章节：[D21 航海/天气逻辑](../../../AGENT_DEVELOPMENT_CHAPTER_13_D21.md)、[D22 UI 逻辑](../../../AGENT_DEVELOPMENT_CHAPTER_14_D22.md)
- 只读数据（本任务不修改）：[准入目录](d18_prefab_candidate_intake_catalog.json)、`packs/ui_materials_audio/*.manifest.json`、`packs/ui_materials_audio/polyhaven_selected_assets.json`、`packs/ui_materials_audio/font_license_audit.json`、`game/Assets/Resources/Localization/D15_A08/`
- 历史（SUPERSEDED，只读引用）：[D18-A06 UI/材质/HDRI/音频资产包](../D18_UI_AUDIO_MATERIAL_ASSET_PACKET.md)
- 状态总览：[逻辑任务状态总览](../../../LOGIC_TASK_STATUS.md)
