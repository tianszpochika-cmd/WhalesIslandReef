# D20-L32 逻辑交接说明

- 任务：`D20-L32`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L32/D20_L32_LOGIC_RESULT.json`

## 已完成

- 新增 `D20ContentIdIntegrationTests`，验证角色 identity content_id 稳定性、NPC 兼容别名映射与未知 ID 保留。
- 验证动物 species catalog 的稳定 content_id、非路径 ID 和外观 catalog/resource key 的稳定性、非候选/非 Assets 路径约束。
- 测试保持纯 C#，不读取文件系统、不启动 Unity、不依赖反射或网络；未修改生产 owner、旧 runtime 或指定状态/记忆文件。

## 公开接口

- `D20ContentIdIntegrationTests.Run()`；
- 测试断言覆盖 `D20CharacterIdentityContracts`、动物 catalog 和 `D20AppearanceSelectionRules` 的公开契约。

## 验证与延期

- D20-L32 批量逻辑 runner 82 assertions PASS；生产 D20 编译 0 warnings、0 errors；禁用引用扫描 0 hits。
- 未启动 Player/PlayMode，未验证真实资源导入、Assets/Prefab、addressables、场景、模型、设备、性能或跨端 Runtime。

## Coordinator memory entry

协调器在批次汇合时记录：`D20-L32 LOGIC_PASS`；新增 content_id 集成测试，82 assertions 覆盖角色/NPC/动物/外观键稳定性与未知保留；纯逻辑编译、批量 smoke 和禁用依赖扫描通过；真实资源/Unity Runtime 延期。
