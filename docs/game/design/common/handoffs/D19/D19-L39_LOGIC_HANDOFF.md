# D19-L39 逻辑交接文档：历史存档兼容测试

## 1. 任务元数据
- 任务编号：`D19-L39`
- 所属章节：`D19 港口领域逻辑（波次 19.7 整合写者与全流程验证）`
- 状态：`LOGIC_PASS`
- 编写范围：`game/Assets/Tests/EditMode/D19/D19LegacySaveTests.cs`
- 交付物：
  - `game/Assets/Tests/EditMode/D19/D19LegacySaveTests.cs`
  - `game/tools/results/D19/D19-L39/D19_L39_LOGIC_RESULT.json`
  - `docs/game/design/common/handoffs/D19/D19-L39_LOGIC_HANDOFF.md`

## 2. 夹具设计与迁移断言
1. **夹具 1（缺失 D19 块）**：传入 null 存档，迁移器安全调用 `CreateDefault`，补齐规范 `SchemaVersion=1`，生成安全默认值；
2. **夹具 2（旧别名迁移）**：`santa_margarita` 与 `building_saint_marys_dock` 平滑迁移至规范锚点键并保留 `MigrationHint`；
3. **夹具 3（版本 0 升级）**：版本由 0 升级至 1，未知/缺省船员位置安全回退为 `OnShore`；
4. **夹具 4（不支持版本拦截）**：未来版本（2）与负数版本明确拒绝，返回 `d19.save.unsupported_version`；
5. **夹具 5（不可能状态与数据损失）**：处于 `Berthed` 状态却缺少 `BerthId` 或 `PortId` 判定为不可逆数据损失并拒绝，**绝不伪造泊位**；
6. **夹具 6（跨港泊位冲突）**：当前停靠港与泊位归属港不一致时明确拦截，返回 `d19.port.permission_denied`；
7. **夹具 7（损坏状态校验）**：负数世界日、空占用船 ID、无效过期日、重复船员 ID 均被明确拦截；
8. **夹具 8（迁移幂等性）**：已迁移的合规结果再次迁移直接返回 `AlreadyUpToDate`，绝不产生二次变异。

## 3. 验证结果
- `dotnet run` 运行全部 8 组夹具测试通过（8 PASSED, 0 FAILED）；
- 状态总览脚本同步通过。
