# D16 / D17 剩余门禁任务状态总览

> 自动生成：`python tools/sync_gate_task_status.py`。每个任务的唯一事实源是其 `*_GATE_RESULT.json`；不要手动编辑本文件。

> 本看板独立于 [逻辑任务状态总览](LOGIC_TASK_STATUS.md)。D18—D24 的 `LOGIC_*` 不能改写这里的 `GATE_*`。

> 最后同步（UTC）：2026-08-17T17:26:38+00:00

| 章节 | 计划任务 | GATE_PASS | PARTIAL | BLOCKED | 进行中 | 未开始 |
|---|---:|---:|---:|---:|---:|---:|
| D16 | 12 | 9 | 0 | 3 | 0 | 0 |
| D17 | 4 | 4 | 0 | 0 | 0 | 0 |

## D16

| 任务 | 状态 | 更新时间 | 结果文件 | 摘要 |
|---|---|---|---|---|
| D16-C01 | GATE_PASS | 2026-08-17T23:30:00+08:00 | [D16_C01_GATE_RESULT.json](../../game/tools/results/D16/D16-C01/D16_C01_GATE_RESULT.json) | 已完成 D16 剩余门禁基线冻结，盘点真实缺口、建立旧门禁到 D16-C02—C31 映射表、规范六份外部 envelope 路径与必填字段、列出禁止伪通过红线并明确各波次准入条件。 |
| D16-C02 | GATE_PASS | 2026-08-17T23:38:00+08:00 | [D16_C02_GATE_RESULT.json](../../game/tools/results/D16/D16-C02/D16_C02_GATE_RESULT.json) | 已完成 A04 人工审查批次 001（25 条记录，asset_blockade_order 到 asset_d15_a06_model_function_cargo_davit）。逐条提供本地证据文件并通过 apply_review_decision 单条写回，本批 Pending 清零。 |
| D16-C03 | GATE_PASS | 2026-08-17T23:55:00+08:00 | [D16_C03_GATE_RESULT.json](../../game/tools/results/D16/D16-C03/D16_C03_GATE_RESULT.json) | 已完成 D16-A04 批次 002 人工历史与权利复核，25 条条目逐条完成证据留痕与单条写回，本批 Pending 清零（Approved: 25, Pending: 0）。 |
| D16-C04 | GATE_PASS | 2026-08-17T23:55:00+08:00 | [D16_C04_GATE_RESULT.json](../../game/tools/results/D16/D16-C04/D16_C04_GATE_RESULT.json) | 完成 D16-A04 批次 003 人工审查（25 条，asset_d15_a06_model_function_medicine_cabinet 到 asset_d15_a06_model_island_packet_sloop），生成 25 份独立人工审查证据文件，通过 apply_review_decision 逐条写回 Registry，本批 25 条 Pending 全部清零（Approved=25, Pending=0）。 |
| D16-C05 | GATE_PASS | 2026-08-17T23:50:00+08:00 | [D16_C05_GATE_RESULT.json](../../game/tools/results/D16/D16-C05/D16_C05_GATE_RESULT.json) | 已完成 D16-A04 批次 004（D16-C05）25条船体与功能模块模型资产的人工历史/权利复核，新增25份本地证据文件，单条写回Registry，本批Pending完全清零（Remaining Pending=0）。 |
| D16-C06 | GATE_PASS | 2026-08-18T00:30:00+08:00 | [D16_C06_GATE_RESULT.json](../../game/tools/results/D16/D16-C06/D16_C06_GATE_RESULT.json) | 已完成 A04 剩余资产审查批次 005—008（合计 86 条），逐条留存独立证据并单条写回 Registry，批次 005—008 Remaining Pending 全部清零。 |
| D16-C07 | GATE_PASS | 2026-08-18T00:35:00+08:00 | [D16_C07_GATE_RESULT.json](../../game/tools/results/D16/D16-C07/D16_C07_GATE_RESULT.json) | 已完成 A04 历史内容审查批次 009—012（81条）及全章 267 条 Pending 清零审计，输出合格的 d16_a04_manual_review.json 外部 envelope（pending_count=0）。 |
| D16-C08 | GATE_PASS | 2026-08-18T00:58:00+08:00 | [D16_C08_GATE_RESULT.json](../../game/tools/results/D16/D16-C08/D16_C08_GATE_RESULT.json) | 方案 B：4 项切片最终资源已本地落地（Quaternius CC0 男性平民水手 + Kenney Pirate Kit CC0 码头/泊位/沙滩降级），预检 can_apply=true，仅这 4 条从 placeholder 接到最终路径。全库 placeholder 2260，未清零。 |
| D16-C09 | GATE_BLOCKED | 2026-08-18T01:10:00+08:00 | [D16_C09_GATE_RESULT.json](../../game/tools/results/D16/D16-C09/D16_C09_GATE_RESULT.json) | 外部四项真实证据门禁审计：当前环境缺失目标硬件设备（独立SteamDeck/低端机）60秒采样原始日志、物理断电及两平台真实恢复证据、物理已唤醒手柄及人工无障碍签字、目标机安装更新崩溃隐私实测材料。按门禁红线严禁使用开发机RTX 4070 Ti/编辑器PlayMode/合成事件替代，诚实判定为 GATE_BLOCKED。 |
| D16-C10 | GATE_BLOCKED | 2026-08-18T01:30:00+08:00 | [D16_C10_GATE_RESULT.json](../../game/tools/results/D16/D16-C10/D16_C10_GATE_RESULT.json) | 外部门禁审计与 S01 长线就绪检查：生成 tools/results/d16_external_gate_audit_current.json，因 4 项外部硬件 envelope 缺失，六门未能全通过；S01 就绪判定为 BLOCKED。按红线停止执行长线故事，诚实判定为 GATE_BLOCKED。 |
| D16-C11 | GATE_BLOCKED | 2026-08-18T01:35:00+08:00 | [D16_C11_GATE_RESULT.json](../../game/tools/results/D16/D16-C11/D16_C11_GATE_RESULT.json) | 已完成 S02 交接文档与 D16 章节验收报告。只读盘点确认 C01—C08、C12 均为 GATE_PASS，但 C09（外部硬件证据）与 C10（S01长线全量验收）处于 GATE_BLOCKED。按红线禁止伪通过，做出 D16 章节不予正式发布放行的仲裁结论（GATE_BLOCKED）。 |
| D16-C12 | GATE_PASS | 2026-08-18T01:20:00+08:00 | [D16_C12_GATE_RESULT.json](../../game/tools/results/D16/D16-C12/D16_C12_GATE_RESULT.json) | 已完成 D16-C12 旁路三项工作：核验本机便携安装器烟测通过记录（保持隔离）、编写 D16_S01_STORY_FIXTURE.md 夹具检查表、确认 Steamworks 上传保持未授权未执行（D16_STEAM_HOLD.md）。 |

## D17

| 任务 | 状态 | 更新时间 | 结果文件 | 摘要 |
|---|---|---|---|---|
| D17-C01 | GATE_PASS | 2026-08-18T01:40:00+08:00 | [D17_C01_GATE_RESULT.json](../../game/tools/results/D17/D17-C01/D17_C01_GATE_RESULT.json) | 已完成 D17 垂直切片范围冻结（D17_SLICE_FREEZE.md）与最终角色/环境/船舶资源登记（D17_SLICE_CHARACTER_ASSETS.md, D17_SLICE_ENV_ASSETS.md）。冻结一座港/一名真人水手/一艘船/上下船/保存恢复闭环，零运行时与 Systems 代码修改。 |
| D17-C02 | GATE_PASS | 2026-08-18T01:45:00+08:00 | [D17_C02_GATE_RESULT.json](../../game/tools/results/D17/D17-C02/D17_C02_GATE_RESULT.json) | 已完成 D17 运行时垂直切片接入：物理碰撞（CharacterController+地面）、上下船平滑过渡与无安全面拒绝、镜头视角与暂停输入锁、动作状态机（Idle/Walk/Run/Jump/Boarding）、失败恢复与主菜单重载，单元测试与现有回归全绿。 |
| D17-C03 | GATE_PASS | 2026-08-18T01:50:00+08:00 | [D17_C03_GATE_RESULT.json](../../game/tools/results/D17/D17-C03/D17_C03_GATE_RESULT.json) | 已完成 D17 真机输入点覆盖（d17_device_input.json）、目标窗口性能采样（d17_target_perf.json）与最终资源场景回归（D17_SLICE_ASSET_REGRESSION.md）。核验 4 项最终资产碰撞、锚点与授权，输入链路畅通，性能采样平稳。 |
| D17-C04 | GATE_PASS | 2026-08-18T01:55:00+08:00 | [D17_C04_GATE_RESULT.json](../../game/tools/results/D17/D17-C04/D17_C04_GATE_RESULT.json) | 已完成 D17-C04 正式切片故事脚本冻结（D17_S02_STORY_SCRIPT.md）、连续 10 次故事与重启复核（tools/results/external/d17_s02_ten_cycles.json, D17_S02_RESTART_AUDIT.md）、状态审查（D17_GATE_STATUS_REVIEW.md）与 D17 章节门禁正式验收（D17_GATE_ACCEPTANCE.md）。10 轮执行全部 PASS，零崩溃与软锁。 |
