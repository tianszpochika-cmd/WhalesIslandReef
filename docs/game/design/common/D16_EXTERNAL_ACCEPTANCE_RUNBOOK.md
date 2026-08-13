# D16 外部真实性验收包

状态：**BLOCKED / 等待真实证据**（2026-08-12）

这份运行手册只定义证据格式和复核顺序，不替代目标设备、人工内容审核、授权资产或发布授权。当前工作区没有任何外部 PASS 证据；缺失状态由 `tools/d16_external_gate_audit.py` 保持为 BLOCKED。

## 统一证据规则

每个门禁在 `tools/results/external/` 下提供一个 JSON 文件，并在 `raw_artifacts` 中列出不可变的原始日志、截图、视频、报告或签字记录。所有文件必须同时包含：

- `status: "PASS"`、`evidence_kind`、`operator`、`device_id`、`captured_at`；
- 非空 `raw_artifacts`；
- 真实设备/人工来源，不能标记 `synthetic`、`editor_only` 或 `simulated`；
- 原始材料和证据文件的路径必须可复核，不能只写一句“已通过”。

审计器会把 `raw_artifacts` 中的相对路径按 `tools/results/external/` 解析，并逐个检查文件确实存在且是文件；绝对路径也必须存在。只填写一个不存在的文件名不能通过审计，审计器不会替操作者复制、生成或补齐原始材料。
相对路径不能通过 `..` 逃出该证据目录；需要引用外部绝对路径时，仍必须确保目标文件真实存在且可由复核者访问。

审计入口：

```text
python tools/d16_external_gate_audit.py game \
  --evidence-root tools/results/external \
  --json-out tools/results/d16_external_gate_audit_current.json
```

脚本只读检查，不修改 Registry、发布清单、审查结论，也不上传 Steam。

## 六项证据文件

| 门禁 | 文件 | 必须覆盖的真实内容 |
|---|---|---|
| D16-A01 | `d16_a01_target_device.json` | 至少 60 秒目标/低端设备窗口；最低/平均 FPS、峰值内存、GC、同屏数、流式加载/恢复、保存/恢复和原始日志 |
| D16-A02 | `d16_a02_power_cross_platform.json` | 物理断电或等价强制中止、至少两个平台、迁移失败保留原档、备份恢复和原始存档对比 |
| D16-A03 | `d16_a03_device_accessibility.json` | 真实键盘/手柄、目标分辨率、焦点、字体/字幕、色觉/低刺激和人工可访问性记录 |
| D16-A04 | `d16_a04_manual_review.json` | 3,139 条登记逐条结果；责任审核人、决定依据、warning/skip；最终 `pending_count=0` |
| D16-A05 | `d16_a05_final_assets.json` | 真实来源和授权的最终资源、LOD/碰撞/锚点、场景回归；最终 `placeholder_count=0` |
| D16-A06 | `d16_a06_target_release_privacy.json` | 目标机安装/更新/卸载、原生崩溃复核、隐私人工复核和目标构建引用 |

## 正确顺序

1. D16-R01 人工逐条处理 A04 Pending；不得用脚本批量批准历史、敏感或权利结论。
2. D16-R05 接入有真实来源/授权的 A05 最终资源；不得把 `placeholder` 改名成最终资源。
3. 在目标设备执行 A01、A02、A03、A06，并保留原始材料。
4. 执行本审计入口；六项都 PASS 且没有证据错误后，才允许更新对应交接报告。
5. 再执行 D16-S01 长线故事，随后 D16-S02 最终交接；D17 正式验收必须等待 S02。

即使六项外部门禁全部 PASS，只要 `upload_authorized` 仍为 false，就不能执行 Steam 上传；Steam 状态由用户明确授权后单独处理。

## 当前审计结果

`tools/results/d16_external_gate_audit_current_20260812.json` 当前为 BLOCKED：A01—A06 六个外部证据文件均不存在，Steam 上传仍未授权。这个结果是预期的诚实状态，不是代码失败，也不能用本机 Unity 60/60 回归替代。

2026-08-13 只读刷新将最新审计写入 `tools/results/d16_external_gate_audit_current_20260813.json`；结果仍为 `BLOCKED`，六个证据文件缺失，Steam 上传未授权。A03 的本机设备预检另见 `tools/results/d16_a03_device_probe_current.json`，它保持 `IN_PROGRESS/PENDING_MANUAL`，不能作为外部 envelope。

## A05 最终资源包接入前预检

真实资源到位后，先使用 `tools/d16_a05_replacement_preflight.py` 对替换包做只读预检。替换包必须包含 `schema_version=1`、`task=D16-A05-REPLACEMENT`、操作者/时间、实际存在的 `raw_artifacts`，以及每个条目的最终资源文件、运行时 Prefab/消费者、来源、商业使用与修改许可、授权记录、人工 `Approved`/`ApprovedWithWarning` 决定、LOD、碰撞、锚点和测试用例。

预检拒绝以下情况：路径逃出仓库、仍指向 `Placeholder`/`ph_`、资源文件或证据不存在、只提供远程链接而没有本地原始材料、目标条目已经不是 placeholder、授权/人工审核字段不完整。预检默认只读，不复制文件、不改 Registry、不批准内容、不修改 `asset_state`；当前没有可供预检的最终资源包，因此 A05 仍保持 BLOCKED。

## D16-S01 启动前只读预检

六项外部 evidence 到位后，先运行 `tools/d16_s01_readiness.py`，将当前外部门禁快照与 A04/A05/A06 机器审计合并检查。只有六项 gate 均为 `PASS`、没有 gate errors、A04 Pending/Rejected/结构错误均为 0、A05 placeholder/非法元数据/声明路径断链均为 0、A06 技术准备为 `PASS` 时，输出才会是 `READY_TO_START`。

该工具只输出启动资格，不执行 D16-S01 故事；当前结果 `tools/results/d16_s01_readiness_current_20260813.json` 为 `BLOCKED`。Steam 上传授权单独记录，未获用户明确授权时不会执行。

## 本机设备预检（不等于 PASS）

在外部人工验收开始前，可以运行 `powershell -ExecutionPolicy Bypass -File tools/d16_a03_device_probe.ps1` 记录当前 Windows 电脑实际枚举到的手柄、键盘、鼠标、显卡和屏幕配置。该工具只写入 `tools/results/d16_a03_device_probe_current.json` 与原始预检文件，明确保持 `IN_PROGRESS` 和 `PENDING_MANUAL`，不会发送按钮输入、不会生成 PASS，也不会替代目标分辨率、焦点、字幕、字体、色觉或低刺激人工检查。
