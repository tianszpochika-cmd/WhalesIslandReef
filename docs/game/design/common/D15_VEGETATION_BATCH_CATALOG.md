# D15-A02 植被批量目录

任务：`D15-A02*`  
日期：2026-08-11  
状态：**已完成目录生产与 D15-A02 专项校验**  
前置：`D15-S01` 通过；代表植被样例 `plant_red_mangrove`、`plant_coast_agave_fiber`、`plant_farm_cassava_mature` 已登记并通过代表内容门禁。

## 1. 交付规模

本批次使用 `tools/d15_a02_vegetation_batch.py` 可复现生成 1,200 条新内容；连同 11 条既有 D11 代表样例，Content Registry 中共有 **1,211 条 `plant_` 条目**，位于 `game/Assets/Resources/ContentRegistry/entries/plant/`。所有条目均使用 `plant_` ID，文件名与 ID 一致。

| 类别 | D15-A02 新增 | Registry 总数（含代表样例） |
|---|---:|---:|
| 乔木 `tree` | 130 | 132 |
| 灌木 `shrub` | 100 | 100 |
| 草本 `herb` | 110 | 110 |
| 蕨类 `fern` | 80 | 80 |
| 苔藓 `moss` | 60 | 60 |
| 附生 `epiphyte` | 70 | 70 |
| 海滨 `coastal` | 100 | 102 |
| 湿地 `wetland` | 100 | 102 |
| 农作物 `crop` | 130 | 132 |
| 果树 `fruit_tree` | 100 | 101 |
| 药材 `medicinal` | 90 | 92 |
| 香料 `spice` | 70 | 70 |
| 变体 `variant` | 60 | 60 |
| **合计** | **1,200** | **1,211** |

成长阶段、风暴损伤、枯萎、恢复、旱季、雨季、盐风、阴生和开阔地等形态变体都写入独立目录字段、资产状态、行为入口和测试入口，因此按 D15 冻结口径计为独立条目，而不是只改名称。

## 2. 统一字段与资产状态

每条 `plant_` 记录保留 D8 必填字段，并增加以下 D15 字段：

- 分类/生态：`plant_class`、`scientific_or_historical_note`、`soil_tags`、耐盐/耐旱/需水、海拔、季节、`growth_stage`、采集部位、资源用途、毒性、燃料、动物关系、种植/引入状态；
- 表现：`model_ref`、`model_status`、`material_ref`、`material_status`、`collision_ref`、`collision_status`、`asset_state`、`lod_profile`、`wind_profile`；
- 生境/行为：`habitat_profile`、`use_profile`、`growth_profile`、`behavior_profile`；
- 生产/审查：`production_batch`、`representative_sample`、`production_stage`、`quality_level`、`historical_review`、`review_evidence_refs`、`content_warning_ids`、`context_note_key`、`skip_allowed`、`presentation_levels`；
- 追溯：`source_refs`、`source_note`、`source`、`license`、`authorization`、`reviewer`、`test_id`、`test_cases`。

正式目录条目不留空的模型/材质状态：

- `model_ref=unity-primitive://vegetation/<class>/lod2`，`model_status=placeholder_primitive_ready`；
- `material_ref` 指向仓库已存在的 `ph_mat_*.mat`，`material_status=source_ready`；
- `asset_state=placeholder`、`quality_level=Q2` 明确这是可扫描/可读取的占位表现，不冒充最终模型；
- 13 个 `asset_vegetation_profile_*` 支持条目提供共享模型/材质/LOD/风场配置，位于 `entries/asset/`，不改变植物内容 ID 的计数口径。

植物 LOD 使用冻结的 A02 档位：LOD0/LOD1/LOD2 上限为 6,000/2,000/600 三角面，远景为群落代理；风场按海滨强风、红树林低位风、雨林冠层风、山脊阵风、洞穴回流和农地开阔风等配置区分。近景共享交互代理，不为每个实例复制高成本碰撞。

资产路径和授权说明见 [植被资产配置 README](../../../../game/Assets/Art/Vegetation/README.md) 与 `game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md`。所有批量条目使用 `project-authored` / `internal-authored`，商业使用和修改权限均为 `true`；没有新增外部下载。

## 3. 生境、用途和审查

条目覆盖海滩与沙丘、红树林/盐沼、干旱灌丛、低地季雨林、湿润雨林、云雾森林、河岸湿地、石灰岩林、火山岩坡和农地/种植园/废弃地。`valid_habitats`、`habitat_profile` 和 `valid_regions` 只表达不可变内容适用范围，不写运行时生态数量或刷新结果。

用途覆盖木材、纤维、燃料、食物、果实、农产、染料、药材、香料、毒性警告、动物栖息和观察指标。药材批次中按稳定规则抽取毒性条目，登记 `ApprovedWithWarning`、`content_warning_plant_toxicity`、上下文 key、四级表现和可跳过字段；实际健康后果仍由医疗/船员 owner 消费事实并裁决。

`EcologySystem` 是植物定义对应的 canonical owner。目录只登记 `behavior_profile` 和 `EcologySystem:plant_resource_profile:*` 接管入口；不写生态公式、资源数量、成熟结算、采集奖励、库存、价格或经济结果。UI 只读这些定义，Agent 只能提出观察/表达，不可直接解锁或修改植物状态。

## 4. 测试与结果

已加入：

- `tools/tests/test_d15_a02_vegetation_batch.py`：配额总和、样例字段、资产配置非空壳和已提交批次深度校验；
- `game/Assets/Tests/EditMode/D8ContentRegistryTests.cs`：保留 D8 代表 ID 检查，数量断言改为兼容 D15 批量目录，并要求运行时读取到至少 1,000 个 `plant_` 条目；
- `tools/d15_a02_vegetation_batch.py`：目录写入、meta 生成、roots 登记和全量字段校验。

本任务已执行：

```text
python -m unittest tools.tests.test_d15_a02_vegetation_batch -v
  4/4 PASS

python tools/d15_a02_vegetation_batch.py game/Assets/Resources/ContentRegistry
  generated=1200; plant_registry=1211; asset_profiles=13

D15-A02 owned registry validation
  global_errors=0
  owned_d15_a02_errors=0

dotnet build game/Whalebone.Content.Runtime.csproj --no-restore -t:Rebuild -p:NoWarn=0162
  0 warning / 0 error
```

全局 `tools/content_registry_check.py` 在最终复核时通过，A02 自有 `plant_` 与 `asset_vegetation_profile_*` 记录专项错误为 0；并行批次的 roots 补齐也已完成。

完整 `Assembly-CSharp-Editor` 编译本轮未通过，阻塞点是工作区既有 `game/Assets/Scripts/Agent/AgentCore/CrewAgent.cs` 缺少 `D10CrewBehaviorState`（CS0246），与本任务目录/资产改动无关；本任务没有修改该文件，也没有将 Unity EditMode 运行结果标记为通过。

## 5. 后续责任

- D15-S02：做 5% 且不少于 50 条的分层功能抽样，覆盖全部 13 类、成长阶段、生境和毒性/药材标签；
- 美术替换任务：把 `unity-primitive://` 与 `ph_mat_*.mat` 占位配置替换为最终模型/材质，并保留稳定 `plant_` ID、LOD 和风场 profile；
- D11/D13：继续由既有 owner 消费 `behavior_profile`、资源用途和采集入口，不从目录复制运行时状态；
- D15-S02：继续按冻结比例做全批次功能抽样和表现替换验收。
