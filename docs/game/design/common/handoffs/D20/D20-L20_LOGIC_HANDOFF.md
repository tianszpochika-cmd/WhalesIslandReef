# D20-L20 逻辑交接说明

- 任务：`D20-L20`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L20/D20_L20_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20AnimalIdentityContracts.cs`，建立动物身份的纯逻辑边界：

- `D20AnimalClassification` 覆盖 `Terrestrial`、`Bird`、`Aquatic`、`Pet`，并用 `RareSpecies` 作为 D18 规定的横切标记；没有创建 `rare_` content_id 前缀；
- `D20AnimalPopulationKind` 区分 `Wild`、`Domestic`、`Pet`，保留 `animal_domestic_*` 不属于宠物的 D18 规则；
- `D20AnimalIdentity` 保存稳定 `AnimalId`、D18 `ContentId`、`SpeciesId`、年龄阶段、归属引用、owner_system、D20 生命周期投影、显式世界日和存档修订；
- `D20AnimalIdentityContracts` 校验小写 snake_case 内容 ID、稳定引用、分类位组合、野生/宠物/owner 一致性、生命周期日期、来源/因果引用和版本；
- `Clone`、`TryValidate`、`Validate`/`ValidateOrThrow` 保持调用方无法绕过校验后继续写外部领域；
- 未复制 D4 `AnimalState` 的生态压力/数量/健康/位置，也未复制 D11 `D11PetState` 的照料、走失、检疫或死亡事实。

## 2. 后续可消费公开 API

- `D20AnimalIdentityContracts.IsStableAnimalId(...)`、`IsStableAnimalContentId(...)`、`IsSupportedClassification(...)`、`PrimaryClassification(...)`、`OwnerSystemFor(...)`；
- `D20AnimalIdentityContracts.CreateDefault(...)`、`Validate(...)`、`ValidateOrThrow(...)`；
- `D20AnimalIdentity` 的 `AnimalId`、`ContentId`、`SpeciesId`、`Classification`、`PopulationKind`、`OwnershipKind`、`OwnerId`、`OwnerSystem`、`LifeStage`、`AgeDays`、`LifecycleState` 及防御性 `Clone()`；
- `D20AnimalIdentityValidationResult` 与 `D20AnimalIdentityValidationCode` 用于在行为/宠物聚合前拒绝非法身份。

野生动物的事实 owner 仍为 `EcologySystem`，宠物的事实 owner 仍为 `AnimalCareSystem`；本契约的 owner 字段只是公开边界引用，不授予 D20 直接写入 D4/D11 的权限。

## 3. 验证与未完成项

- .NET Framework `csc.exe` 独立 library 编译：exit 0，`/warnaserror+`，XML 文档生成成功；
- Add-Type 正/负 smoke：合法稀有水生身份、非法 `rare_` 前缀拒绝、宠物 owner 边界均通过；
- 目标源文件静态检查：无 Unity/网络/LLM/系统时钟/随机实现引用，尾随空白为 0；结果 JSON 解析和 `git diff --check` 通过；
- 未启动 Player、PlayMode、场景、输入、物理、资源导入或设备验证；没有修改 D4/D11 owner 或其他 Agent 文件。

模型、Prefab、Animator、场景、Transform、物理、寻路、输入、设备和资源准入继续保持 Runtime Deferred，不构成纯逻辑阻断。

## 4. Coordinator memory entry

请协调器在本批次汇合后统一写入当天日志：`D20-L20 LOGIC_PASS`；新增 `D20AnimalIdentityContracts`，冻结 D18 animal_/fish_/pet_ content_id、RareSpecies 横切标记、Wild/Domestic/Pet、owner 与生命周期投影边界；独立 csc、正负身份 smoke、静态禁用引用检查、JSON 解析和 diff 校验通过；未启动 Runtime。
