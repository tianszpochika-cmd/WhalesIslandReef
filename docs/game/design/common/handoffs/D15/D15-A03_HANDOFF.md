# D15-A03* 交接报告

任务：`D15-A03*`  
日期：2026-08-11  
状态：**A03 内容批次通过；D15-S02 继续负责跨场景/美术最终抽样**

## 1. 交付范围

- 在 `game/Assets/Resources/ContentRegistry/entries/animal/` 与 `entries/fish/` 完成 D15-A03 批次 300 条目。
- 300 条目由 297 个新内容定义与 3 个冻结代表样例组成：
  - `animal_green_turtle`
  - `animal_domestic_chicken`
  - `fish_mackerel_school`
- 覆盖陆生、鸟类、爬行、昆虫、淡水/河口、近岸鱼、鲨鳐、海龟、鲸豚、甲壳类/软体和海岸鸟 11 类。
- 批次分布：166 个 `animal_`、134 个 `fish_`；每条 ID 唯一，文件名与 ID 一致。

## 2. 新增配置/字段

D15 扩展字段已加入 `ContentRegistryEntry` 读取 DTO，供 Unity 内容读取器和抽样测试读取：

- 生产：`production_batch`、`representative_sample`、`production_stage`、`quality_level`、`asset_state`；
- 行为与生态：`d15_category`、`behavior_level`、`behavior_profile`、`habitat_profile`、`refresh_profile`、`food_chain_profile`；
- 风险与法律：`danger_profile`、`legal_profile`；
- 表现预算：`performance_budget`、`model_ref`、`material_ref`、`collision_ref`、`lod_profile`；
- 追溯与审查：`authorization`、`test_cases`、`historical_review`、`review_evidence_refs`、`content_warning_ids`、`context_note_key`、`skip_allowed`、`presentation_levels`。

所有条目统一为 `owner_system=EcologySystem`，行为字段只提供确定性逻辑接管引用，不包含公式、网络地址、LLM prompt 或运行时状态。受保护物种使用 `ApprovedWithWarning`、`content_warning_wildlife_protection` 和可跳过表现等级。

## 3. 替换/清理的硬编码

本任务没有修改或复制 EcologySystem 的行为、刷新、危险、法律、鱼群数量、动物数量、经济、库存或奖励公式；没有把运行时结果写进目录。  
仅将 3 个代表样例补齐 D15 数据字段，并将 300 个批次 ID 接入已有 D8 registry roots，避免新增第二套内容主键或无主内容。

## 4. 所有权边界

- 内容目录/读取器：只读不可变定义、来源、授权、LOD 和 owner 接管引用。
- EcologySystem：唯一裁决动物/鱼群刷新、数量、行为、位置、生态压力、保护区和法律结果。
- Inventory/Cargo、Economy、Legal、Reward、UI、Agent：本任务未取得或新增写权限。
- Systems 未加入网络、HTTP 或 LLM；动物内容不使用 LLM。

## 5. 测试与命令结果

已执行：

```powershell
python tools/d15_a03_animal_batch_check.py game/Assets/Resources/ContentRegistry
# PASS: 300 entries; 166 animal / 134 fish; 300 unique test IDs
# category counts: terrestrial 40, birds 25, reptiles 25, insects 20,
# freshwater_estuary 25, nearshore_fish 50, sharks_rays 20,
# sea_turtles 15, whales_dolphins 15, crustaceans_softbody 35,
# coastal_birds 30

python -m unittest tools/tests/test_d15_a03_animal_batch_check.py
# PASS: 1 test

python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
# PASS: Content registry OK: 2812 entries, 13 prefixes

# JSON parse check
# PASS: 305 animal/fish JSON files
```

Unity EditMode 测试入口已登记：`D15.Tests.D15AnimalBatchTests`，覆盖 300 条目加载、唯一 ID、字段完整性、代表样例、Ecology owner 和确定性逻辑入口。内容读取器 DTO 的编译应由 `Whalebone.Content.Runtime.csproj` 验证；当前整仓 Unity Editor 是否能运行还需避让占用中的 Unity 进程，并由 D15-S02 做最终批次抽样。

## 6. 遗留与后续

- 300 条目的模型/材质/碰撞目前是明确标记的 Q2 placeholder，不虚报为最终美术完成；替换责任进入后续资产 QA/D15-S02。
- D15-S02 仍需按冻结规则完成每类至少一次抽样、加载/行为接管/保存恢复、错误生境、缺碰撞、重复模型和性能预算复核。
- 本交付不改变 D11 生态公式、采集平衡或任何经济/战斗数值。
