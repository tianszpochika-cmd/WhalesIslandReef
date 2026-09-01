# D20-L27 逻辑交接说明

- 任务：`D20-L27`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L27/D20_L27_LOGIC_RESULT.json`

## 已完成

- 新增 `D20CharacterSaveMigration`，只处理 `D20CharacterSaveProjection`，不接管 SaveSystem 根迁移或任何 D10/D11/D19 owner 事实。
- 支持根投影及已实现 D20 子投影的 v0→v1 确定性升级：玩家身份/presence/action、L10 身份表现、NPC 身份/行为、动物身份/行为、宠物所有权、事件游标和 owner 引用均检查未知版本；可安全补齐的列表/回执缺失统一为空。
- 缺失动作表示“没有可恢复动作”，缺失 NPC/动物列表表示空集合；迁移不创建角色、动物、动作、宠物生命、生态、关系、经济、港口或其它已发生事实。
- 当前版本重复迁移返回 `AlreadyUpToDate`，输出为深拷贝且结果确定；未来/负数版本、非法 ID、跨投影冲突、不可能状态和不完整回执返回 `Rejected`，并在 `OriginalProjection` 保留原始投影副本。
- v0 NPC 未声明迁移状态时仅标记为 `UnknownPreserved`（前提是稳定 ID 已存在），不猜测身份；L17 没有回执表的限制保持显式，不伪造命令/事件。
- 未修改 `SaveSystem`、D10、D11、D19、旧 A/R/S 运行时或指定状态/记忆文件。

## 公开接口

- `D20CharacterSaveMigration.Migrate(source, defaultPlayerId)`；
- `D20CharacterSaveMigrationStatus`：`Rejected`、`Migrated`、`AlreadyUpToDate`；
- `D20CharacterSaveMigrationResult`：`Succeeded`、`MigratedProjection`、`OriginalProjection`、`MigrationFlags`、`ReasonCode`。

## 验证与延期

- `dotnet build game/Whalebone.Systems.csproj --no-restore --nologo -warnaserror`：0 warnings、0 errors。
- `dotnet build .codex-temp/D20LogicCompile.csproj --no-restore --nologo -warnaserror`：D20 目录独立纯 C# 编译通过，0 warnings、0 errors。
- `dotnet run --project .codex-temp/D20LogicSmoke.csproj --nologo`：v0→v1 缺失字段安全默认、重复迁移幂等、未来版本拒绝保留原始投影和非法投影负向 smoke 通过；该 smoke 同时覆盖 L26 投影路径。
- 禁用引用扫描 0 hits、目标文件尾随空白 0、结果 JSON 解析通过、`git diff --check` exit 0。
- 未启动 Player/PlayMode，未验证 SaveSystem 磁盘读写、owner 迁移组合顺序、场景/Prefab/Canvas/Input/Transform/Animator、模型、设备、性能或真实运行时恢复；这些保持 Runtime Deferred。

## Coordinator memory entry

协调器在批次汇合时记录：`D20-L27 LOGIC_PASS`；新增 D20 角色保存迁移器，支持 v0→v1、动作/NPC/动物缺失的安全 absent/empty 默认、嵌套版本检查、重复迁移幂等及未来/非法版本原始副本拒绝；纯 C# 编译、迁移 smoke、禁用引用扫描、JSON 解析和 diff 校验通过；Runtime/SaveSystem 组合延期。
