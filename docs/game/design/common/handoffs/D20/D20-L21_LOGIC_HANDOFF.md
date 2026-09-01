# D20-L21 逻辑交接说明

- 任务：`D20-L21`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L21/D20_L21_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20AnimalSpeciesCatalog.cs`，以 D18 生态分类和 D11 宠物公开定义建立只读目录：

- 目录包含 9 个已冻结逻辑 ID：`animal_agouti_forest`、`animal_domestic_chicken`、`animal_anhinga`、`animal_caribbean_parrot`、`fish_mackerel_school`、`animal_green_turtle`、`fish_bottlenose_dolphin`、`pet_cabin_cat`、`pet_harbor_dog`；
- `D20AnimalClassification` 使用 `Terrestrial`/`Bird`/`Aquatic`/`Pet` 基础位与 `RareSpecies` 横切位，绿海龟和宽吻海豚保持原有 `animal_`/`fish_` 前缀；
- `D20AnimalBehaviorAbility` 提供 `Rest`、`Forage`、`Alert`、`Flee`、`Follow`、`Recover`、`Social`、`Fly`、`Aquatic`、`DomesticCare`、`Observe` 等稳定能力标签；
- `TaxonomyCategory`、`PopulationKind` 和 `OwnerSystem` 按 D18 唯一写者校验：生态/鱼类由 `EcologySystem`，宠物由 `AnimalCareSystem`；
- `All`、`Find`、`FindByCategory`、`TryGetDefinition` 返回防御性副本；`ValidateCatalog` 检查重复 ID、分类/前缀、owner、能力位和稳定逻辑引用；
- 目录不读磁盘 Registry、不引用候选路径、不检查模型/Prefab，不生成动物实例、数量、健康、压力、收集或运行时视觉事实。

## 2. 后续可消费公开 API

- `D20AnimalSpeciesCatalog.All`、`Find(...)`、`TryGetDefinition(...)`、`FindByCategory(...)`、`IsKnownContentId(...)`、`ValidateCatalog()`；
- `D20AnimalSpeciesCatalog.CategoryFor(...)`、`TaxonomyCategoryFor(...)`、`AbilityTagIdsFor(...)`；
- `D20AnimalSpeciesDefinition` 的 `ContentId`、`SpeciesId`、`Classification`、`TaxonomyCategory`、`PopulationKind`、`OwnerSystem`、`Capabilities`、`AbilityTags`、`PrimaryCategory`、`Category`、`IsRareSpecies`；
- `D20AnimalBehaviorAbility` 作为 L22 状态机的能力前置输入，`D20AnimalSpeciesValidationResult` 用于拒绝不符合 D18 边界的定义。

`RareSpecies` 不是独立内容前缀；D18 的生态数量、种群、捕获/收集、宠物照料和法律后果仍由对应 owner 处理。

## 3. 验证与未完成项

- .NET Framework `csc.exe` 编译 L20/L21：exit 0，`/warnaserror+`，XML 文档生成成功；
- Add-Type 正/负 smoke：目录 9 条全量校验、五类过滤、稀有横切、宠物 owner、副本保护和非法 `rare_` 前缀拒绝均通过；
- 目标源文件静态检查：无 Unity/网络/LLM/系统时钟/随机实现引用，尾随空白为 0；结果 JSON 解析和 `git diff --check` 通过；
- 未启动 Player、PlayMode、场景、输入、物理、资源导入或设备验证；没有修改 D18 Registry、D4 Ecology 或 D11 Pet owner。

模型、Prefab、Animator、场景、Transform、物理、寻路、输入、设备、资源准入和真实生态运行时接线继续保持 Runtime Deferred。

## 4. Coordinator memory entry

请协调器在本批次汇合后统一写入当天日志：`D20-L21 LOGIC_PASS`；新增 `D20AnimalSpeciesCatalog`，登记 9 个 D18/D11 已冻结 animal_/fish_/pet_ content_id，RareSpecies 保持横切标记，并以稳定位能力标签提供 L22 前置；独立 csc、目录正负 smoke、静态禁用引用检查、JSON 解析和 diff 校验通过；未启动 Runtime。
