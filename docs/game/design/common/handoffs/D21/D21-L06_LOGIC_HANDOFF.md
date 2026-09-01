# D21-L06 逻辑交接

## 任务状态

- 状态：`LOGIC_PASS`。
- D20 公共船员只读投影、D21 只读岗位映射和 `D21CrewRequirementRules` 真实评估链已在纯 C# 中闭环；D10/Crew owner 仍是唯一写者。

## 已完成

- `D20CrewProjectionContracts` / `D20CrewProjectionAdapter` 发布稳定 ship/crew/source metadata、岗位字符串、可用性、失能、morale/fatigue/health，并执行 schema、ID、范围、重复字段校验、Clone 和确定性排序。
- `D21CrewProjectionAdapter.Map` 只读取 `D20CrewRosterProjection`，保留 source projection/owner 证据；岗位映射是显式唯一表，未知岗位拒绝，不降级成合法岗位。
- `D21CrewRequirementRules` 继续校验最低/标准/最大人数、必需岗位和平均士气/疲劳/健康；`ValidateDecision` 只为历史 D21-L27 预计算结果提供严格形状兼容。

## Public API / owner 边界

- D20：`D20CrewOwnerRosterReadSnapshot`、`D20CrewRosterProjection`、`D20CrewProjectionAdapter.Project`、`D20CrewProjectionAdapter.Validate`。
- D21：`D21CrewProjectionAdapter.Map`、`TryMapRole`、`D21CrewRequirementRules.ValidateProfile`、`ValidateRoster`、`Evaluate`。
- `D21VoyageCommandRequest.D20CrewProjection`（`CrewProjection` 别名）优先于旧输入；D21 Start 将其映射后重新调用 `Evaluate`。直接 `D21CrewRosterSnapshot` 仍可兼容，但同样走 `Evaluate` 和 roster 校验。
- D20/D21 不持有或写入 CrewAgent/D10/Crew owner 的事实集合；投影缺失或非法均 fail-closed。

## 验证

- `dotnet build .codex-temp/crew-projection-tests/CrewProjectionTests.csproj --no-restore --nologo -warnaserror`：PASS，0 warning / 0 error。
- `dotnet run --project .codex-temp/crew-projection-tests/CrewProjectionTests.csproj --no-build --no-restore`：PASS，D20 14/14、D21 16/16，`CREW_PROJECTION_FAILURES=0`。
- `dotnet build .codex-temp/d21-tests/D21IntegrationTests.csproj --no-restore --nologo -warnaserror`：PASS，0 warning / 0 error；包含既有 D21-L27/L28 离线回归和新增闭环测试。
- `dotnet run --project .codex-temp/d21-tests/D21IntegrationTests.csproj --no-build --no-restore`：PASS，退出码 0，既有回归无异常，新增 D21 crew 16 项断言通过。
- `python tools/d20_logic_boundary_audit.py`、`python tools/d21_logic_boundary_audit.py`、`git diff --check`：PASS；D21 审计 29 个 C# 文件，0 个 P0 违规。
- 结果文件：`game/tools/results/D21/D21-L06/D21_L06_LOGIC_RESULT.json`。

## 未完成 / Runtime Deferred

- 船员 owner 的真实排班/招募/解雇写入、船员 UI/动画、甲板站位、驾驶/物理、场景、输入、VFX、音频、性能、真机和发行门禁均未执行。
- D21-L27 补给性能文件未在本次任务修改。

## Blocker / 下一步

- 无纯逻辑 blocker。下游可消费 D20 public projection；仍不得把本交接解释为 Unity Player/PlayMode、Runtime 或 Release 通过。
