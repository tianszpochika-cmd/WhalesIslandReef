# D21-L24 逻辑交接

## 任务状态

- 状态：`LOGIC_PASS`。
- `D21VoyageCommandService.Start` 已优先消费 D20 公共船员只读投影，经过 D21 只读映射后真实调用 `D21CrewRequirementRules.Evaluate`；不修改 D10/Crew owner。

## 已完成

- `D21VoyageCommandRequest.D20CrewProjection`（兼容别名 `CrewProjection`）进入命令 clone 和确定性 fingerprint。
- Start 的输入优先级为 D20 public projection → 已验证 `D21CrewRosterSnapshot` → D21-L27 历史预计算 decision 兼容；前两条路径均执行 D21 roster/profile 校验和 `Evaluate`。
- 成功的 `D21VoyageStartPlan` 保留 `CrewSourceProjectionId` / `CrewSourceOwnerSystem` 审计引用以及重新计算的 `CrewDecision`。
- D20 schema/ID/范围/重复字段、D21 岗位未知值、船只不匹配、修订不匹配和缺失投影均有稳定 fail-closed 路径；未知岗位不降级为 Deckhand。

## Public API / owner 边界

- `D21CrewProjectionAdapter.Map`：D20 `D20CrewRosterProjection` → D21 `D21CrewRosterSnapshot`。
- `D21VoyageCommandService.Route` / `StartVoyage`：唯一航程命令入口。
- 船况、船体、天气、补给、港口和船员事实继续由各自 owner 裁决；本服务只编排公开快照、规则和结果。

## 验证

- `dotnet build .codex-temp/crew-projection-tests/CrewProjectionTests.csproj --no-restore --nologo -warnaserror`：PASS，0 warning / 0 error。
- `dotnet run --project .codex-temp/crew-projection-tests/CrewProjectionTests.csproj --no-build --no-restore`：PASS，D20 14/14、D21 16/16，覆盖合法映射、排序/Clone、未知岗位、缺失数据、Start 通过/拒绝、直接 roster 和 owner 不写。
- `dotnet build .codex-temp/d21-tests/D21IntegrationTests.csproj --no-restore --nologo -warnaserror`：PASS，0 warning / 0 error。
- `dotnet run --project .codex-temp/d21-tests/D21IntegrationTests.csproj --no-build --no-restore`：PASS，退出码 0，新增 crew 投影断言 16 项且 `D21_INTEGRATION_FAILURES=0`。
- `python tools/d21_logic_boundary_audit.py`：PASS，29 个 D21 C# 文件、0 个 P0 违规；`git diff --check`：PASS。

## Runtime Deferred / 下一步

- Unity Player/PlayMode、场景/Prefab、真实驾驶/物理/输入、船员 UI/动画、性能、真机和发行门禁未执行。
- D21-L27 补给性能文件未在本次任务修改；其既有离线回归只作为兼容回归证据，不替代 Runtime 证据。
