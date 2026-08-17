# D16 / D17 发布门禁单任务协议

本协议由 `D16-Cxx` / `D17-Cxx` 任务卡引用。将章节提示词原样复制给 Agent，并填入任务编号。该提示词加上对应任务卡构成完整可执行指令。

这条轨与 [D18—D24 纯逻辑轨](LOGIC_TASK_PROTOCOL.md) 并行存在，但**编号、结果目录、状态文件和验收口径全部独立**。不要把 `LOGIC_PASS` 写进本轨，也不要把 `GATE_PASS` 写进 D18—D24。

## 与旧编号的关系

| 旧身份 | 新分派编号 | 说明 |
|---|---|---|
| `D16-A01`—`D16-A06`、`D16-R01`—`D16-R06`、`D16-S01`/`S02` | `D16-C01`—`D16-C12` | 2026-08-17 合并后的分派 ID；`C01`—`C05` 已下发保留 |
| `D17-S01`/`A01`—`A06`/`S02` | `D17-C01`—`D17-C04` | 合并后的正式切片任务；本机基线只是输入 |
| 旧 `D16-C13`—`C31`、旧 `D17-C05`—`C16` | SUPERSEDED | 从未作为完成证据；禁止再派或写结果 |
| 旧 A/S/R/RERUN 结果 | 只读历史 | 禁止把旧结果改写成新 `GATE_PASS` |

一条旧门禁可以对应一项合并后的 C 任务。关闭旧门禁时，必须引用当前有效 C 任务的结果，而不是改写旧 handoff 的历史段落。

## 允许的工作

- 只修改任务卡列出的唯一写入范围；
- 收集或核对外部门禁要求的真实证据、人工审查决定、授权资源和目标设备记录；
- 运行仓库已有的只读审计 / 预检 / 安装器烟测 / 外部门禁审计；
- 在任务明确授权时，用既有单条审查工具写回一条人工决定；
- 为任务创建其唯一结果目录、交接报告和当日记忆日志。

## 禁止的工作

- 不把本机 PlayMode、编辑器 FPS、合成输入或候选资源写成正式通过；
- 不把 `placeholder` 改成 `integrated` / `verified` 来清零 A05；
- 不脚本批量批准 A04 Pending；
- 不执行 Steam 上传、商店发布或任何未授权外部发送；
- 不改 `contracts/`、Systems 裁决权、AgentCore 纯 C# 边界，除非任务卡明确要求且仍遵守铁律；
- 不修改 D18—D24 的 `*_LOGIC_RESULT.json` 或 `LOGIC_TASK_STATUS.md`；
- 不启动与本任务无关的 D19 逻辑开发，也不用本轨结果去关闭 D19。
- 开发阶段不把 `D16-C09`/`C10`/`C11` 的 `GATE_BLOCKED` 改成 `GATE_PASS`；真机四门推迟见 [D16_EXTERNAL_HARDWARE_DEFERRED.md](design/common/D16_EXTERNAL_HARDWARE_DEFERRED.md)。

## 固定的结果格式

每个任务在其唯一目录创建 `Dxx_Cnn_GATE_RESULT.json`：

```json
{
  "task_id": "D16-C01",
  "status": "GATE_PASS",
  "updated_at": "2026-08-17T23:30:00+08:00",
  "maps_to": ["D16-A04"],
  "summary": "一句话说明真实完成内容",
  "changed_files": ["相对路径"],
  "verification": [
    {"command": "实际执行的命令", "result": "PASS 或失败摘要"}
  ],
  "external_evidence": ["tools/results/external/....json 或原始材料路径"],
  "runtime_deferred": ["仍不能宣称完成的外部/设备/发布项"],
  "blockers": [],
  "handoff": "docs/game/design/common/handoffs/D16/D16-C01_GATE_HANDOFF.md"
}
```

只可使用 `PLANNED`、`IN_PROGRESS`、`GATE_PASS`、`GATE_PARTIAL`、`GATE_BLOCKED`。`GATE_PASS` 必须没有 P0 blocker，且引用可复核的真实证据。没有证据时必须写 `GATE_PARTIAL` 或 `GATE_BLOCKED`。

完成后运行：

```powershell
python tools/sync_gate_task_status.py
git diff --check
```

同步工具会更新 [门禁任务状态总览](GATE_TASK_STATUS.md)。每个任务自己的结果 JSON 才是事实源。

## 状态口径

| 状态 | 含义 |
|---|---|
| `GATE_PASS` | 本任务范围内的真实证据已齐，且未越权改其他模块 |
| `GATE_PARTIAL` | 交了一部分，但不能作为下游硬门禁 |
| `GATE_BLOCKED` | 缺前置、缺真实证据、缺授权或发现不可自行修复的缺口 |
| `PLANNED` / `IN_PROGRESS` | 尚未提交或正在做 |

本机代码绿、工具测试绿、D18 `LOGIC_PASS`、D19 港口逻辑推进，都不是本轨 `GATE_PASS`。
