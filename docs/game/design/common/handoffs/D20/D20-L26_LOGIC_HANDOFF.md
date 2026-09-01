# D20-L26 逻辑交接说明

- 任务：`D20-L26`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L26/D20_L26_LOGIC_RESULT.json`

## 已完成

- 新增 `D20CharacterSaveProjection` 与 `D20CharacterSaveProjectionMapper`，只组合 D20 自有角色保存语义：玩家身份、presence、动作投影、L10 身份表现只读投影、NPC 身份/行为、动物身份/行为和 L23 宠物所有权映射。
- `D20IdentityPresentationSaveProjection`、身份历史和本地化文本是 L10 已生成模型的防御性保存副本；它们不成为 D10 身份、法律、关系或外观事实的第二写者。
- `D20SaveOwnerSnapshotReference` 只保存 owner、snapshot_id、schema、revision、last_event_id 和 cause 引用；D10/D11/D19 的字段与事实仍由各自 SaveProjection/owner 保存。
- `D20CharacterSaveEventCursor` 保存 D20 事件游标、已应用事件键和因果引用；L03/L06/L22 已有 presence/action/animal 回执继续随公开快照保留幂等信息。L17 当前没有独立回执表，NPC 包装只接受上层提供的可选命令/事件引用，缺失时保持为空，不伪造事件。
- `Clone()` 深拷贝所有嵌套列表和投影；`Validate()` 检查版本、稳定 ID、唯一性、跨投影 ID 一致性、owner 引用和不可能空值。L24/L25 的生态/海洋 live read model 不被当作可变事实写入 D20 存档。
- 未修改 `SaveSystem`、D10、D11、D19 或旧 A/R/S 运行时。

## 公开接口

- `D20CharacterSaveProjection.CreateDefault(...)`、`Clone()`、`Validate()`；
- `D20CharacterSaveProjectionMapper.BuildProjection(...)` / `Build(...)`；
- `D20IdentityPresentationSaveProjection.FromModel(...)`、`D20IdentityHistorySaveProjection.FromModel(...)`；
- `D20NpcBehaviorSaveProjection.FromSnapshot(...)`；
- `D20SaveOwnerSnapshotReference` 与 `D20CharacterSaveEventCursor`。

## 验证与延期

- `dotnet build game/Whalebone.Systems.csproj --no-restore --nologo -warnaserror`：0 warnings、0 errors。
- `dotnet build .codex-temp/D20LogicCompile.csproj --no-restore --nologo -warnaserror`：D20 目录独立纯 C# 编译通过，0 warnings、0 errors。
- `dotnet run --project .codex-temp/D20LogicSmoke.csproj --nologo`：save→load clone 等价、嵌套副本隔离和 owner/event 引用检查通过；该 smoke 同时覆盖 L27 迁移路径。
- 禁用引用扫描 0 hits、目标文件尾随空白 0、结果 JSON 解析通过、`git diff --check` exit 0。
- 未启动 Player/PlayMode，未验证 SaveSystem 根组合、磁盘存档、场景/Prefab/Canvas/Input/Transform/Animator、模型、设备、性能或真实运行时恢复；这些保持 Runtime Deferred。

## Coordinator memory entry

协调器在批次汇合时记录：`D20-L26 LOGIC_PASS`；新增 D20 角色保存投影与映射器，保存 D20 自有身份/action/行为/宠物投影、事件/因果/幂等载体和 D10/D11/D19 owner 快照引用，不复制外部事实、不修改 SaveSystem；纯 C# 编译、save→load smoke、禁用引用扫描、JSON 解析和 diff 校验通过；Runtime/真实存档接线延期。
