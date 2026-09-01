# D20-L02 逻辑交接说明

- 任务：`D20-L02`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L02/D20_L02_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20CharacterIdentityContracts.cs`，冻结 D20 角色层的最小身份语义：

- `D20CharacterKind`：`Player`/`Npc`；动物和宠物不在此处复制；
- `D20CharacterLifeStage`：`Infant`、`Child`、`Adolescent`、`Adult`、`Elder`；
- `D20CharacterAvailability`：`Available`、`Busy`、`Unavailable`、`Incapacitated`、`Dead`、`Departed`；
- `D20CharacterImmutableFields`：稳定 `CharacterId`、类别、`ContentId`、身份 opaque 引用和来源引用，提供校验、深拷贝、匹配和确定性指纹；
- `D20CharacterIdentity`：版本化身份投影，保存生命阶段、可用性、稳定原因码和存档修订，并提供防御性 `Clone` 与 `Validate`；
- `D20CharacterIdentityContracts`：稳定键规则、枚举支持性、终止状态、默认投影和不可变字段匹配入口。

姓名、代词、证件、法律身份、外观、关系、健康数值、经济、港口、动物和宠物事实没有被复制或写入；`IdentityKey` 只作为外部 owner 的不透明引用。

## 2. 后续可消费公开 API

后续 D20 任务可使用：

- `D20CharacterIdentityContracts.CreateDefault(...)`、`IsStableCharacterId(...)`、`ImmutableFieldsMatch(...)`；
- `D20CharacterImmutableFields.Validate()`、`Clone()`、`Matches(...)`、`Fingerprint()`；
- `D20CharacterIdentity.CharacterId`/`CharacterKind` 只读别名、`Clone()`、`Validate()`。

## 3. 验证与未完成项

- .NET Framework `csc.exe` 独立 library 编译：通过，exit 0，`/warnaserror+`，XML 文档生成成功；
- 源码静态检查：无 engine/network/LLM/time/random 引用，无尾随空白；
- 未启动 Unity、Player、PlayMode、场景或真实设备；未新增测试文件，因为本任务写入范围只允许身份契约文件及本交接/结果留痕。

运行时序列化器、角色生成、表现资源、输入、物理、场景和设备验证保持 Runtime Deferred，不构成逻辑阻断。

## 4. Coordinator memory entry

请协调器在本批次汇合后统一写入当天日志：`D20-L02 LOGIC_PASS`；新增角色身份契约位于 `game/Assets/Scripts/Systems/D20/D20CharacterIdentityContracts.cs`，以稳定 ID/玩家-NPC 类别/生命阶段/可用性/不可变字段匹配作为后续 L03-L05 的只读基础；独立 csc 编译与静态边界检查通过；未启动 Runtime。
