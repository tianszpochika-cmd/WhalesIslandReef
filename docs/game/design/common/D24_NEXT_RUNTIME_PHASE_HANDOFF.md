# D24 后续 Runtime 阶段交接（有限首轮）

> 当前状态：`RUNTIME_DEFERRED`
>
> 本文是从纯逻辑候选版进入后续验证的范围交接，不是 Runtime 执行报告，也不授予 `RuntimeApproved` 或 `ReleaseApproved`。

## 1. 进入条件

只有满足以下条件，后续执行者才可建立独立 Runtime 验证记录：

1. `game/tools/results/D24/D24-L35/D24_L35_LOGIC_RESULT.json` 保持 `LOGIC_PASS`，且候选验收文档与 L35 回归报告可复现。
2. `docs/game/LOGIC_TASK_STATUS.md` 由同步脚本重新生成，D24-L01—L36 结果协议完整；不得手动把延期项改成通过。
3. 使用当前工作树和明确的构建标识；不得把 D18—D23 历史 Player/RERUN/视觉产物冒充当前 Runtime 证据。
4. Runtime 执行前另行确认 Unity、目标设备、输入权限、场景入口、资源状态和真实 SaveSystem 介质均可用；缺任何一项就登记为阻断或延期。

## 2. P0 首轮范围

首轮只验证最小可观察闭环，按以下顺序建立独立证据：

| 顺序 | 范围 | 最小观察点 |
|---|---|---|
| P0-1 | Player 启动与 `InitialHarbor` | 构建启动、进入入口场景、无启动崩溃/缺资源；记录当前 build id |
| P0-2 | 港口 → 船 → 出航 | 通过真实输入完成选船/登船/离港，画面状态和逻辑状态一致；记录拒绝路径 |
| P0-3 | 航行 → 抵达 → 返港 | 完成最小路线，观察航行、到达、返港和状态恢复；不以单一截图代替过程证据 |
| P0-4 | Save / Load / Continue | 在航行前后和中断点执行真实介质保存、加载、继续；核对货物、资金、船况、事件和 projection 不丢失、不重复 |

P0 不包含大规模美术扩量、性能优化、多人/网络接入或 LLM 行为扩展；这些要单独建 Runtime 任务和证据目录。

## 3. 停止规则

遇到任一条件立即停止当前波次并记录复现步骤、build id、日志和影响范围，不得以截图、重试或手工改档掩盖：

- Player 崩溃、卡死、启动失败、场景/Prefab/资源缺失或入口不可达；
- 真实输入无法完成关键动作、焦点/按钮状态不可观察，或视觉状态与逻辑状态不一致；
- 命令绕过 owner/authority guard、出现非法状态跃迁、重复副作用、负数资源或事件顺序错误；
- Save/Load/Continue 丢失或重复写入资金、货物、船况、事件、projection；
- 设备分辨率、音频、物理、相机、帧率/内存或异步加载出现达到验收阈值的回归；
- 需要网络、LLM、外部账号或未经授权的设备/权限才能继续，而当前任务没有明确授权。

## 4. 证据和结论规则

- Runtime 证据必须来自当前构建的真实运行过程，包含可重放输入、日志、状态快照和失败记录；历史逻辑结果只能作为前置条件。
- `LOGIC_PASS`、`RUNTIME_DEFERRED` 和 Runtime 实测结果分栏记录；任何单项 Runtime 通过都不能自动推出完整 playable 或发行通过。
- 只有完成对应 Runtime 门禁、资源准入、性能/设备和人工发布审查后，才可以由授权流程分别判断 `RuntimeApproved` 或 `ReleaseApproved`。

## 5. 本次交接边界

本任务没有启动 Unity、Player、PlayMode、设备、网络、LLM、性能采样或真实 SaveSystem。后续执行者从 P0-1 开始，并在每个停止规则触发时保留 `RUNTIME_DEFERRED` 或记录独立 blocker。
