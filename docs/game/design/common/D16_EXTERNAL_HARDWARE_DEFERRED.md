# D16 外部真实实测：开发阶段推迟说明

> **决策日期**: 2026-08-18  
> **决策人**: 项目统筹（用户）  
> **效力**: 开发阶段（当前至 D17 切片可玩、D18—D24 逻辑推进）  
> **不效力**: Steam / 商店 / 正式发布

## 一句话

目标机性能、物理断电/跨平台、真机手柄与无障碍、目标机安装卸载与隐私复核，**暂不列入开发阶段必做项**。已有 `GATE_BLOCKED` 日志保留为事实源，**不得改写成 `GATE_PASS`**。

## 推迟范围（仍保持 BLOCKED）

| 编号 | 内容 | 当前结果 | 开发阶段要求 |
|---|---|---|---|
| `D16-C09` | A01/A02/A03/A06 四份外部真机 envelope | `GATE_BLOCKED` | 只留缺口说明，不补假证据 |
| `D16-C10` | 六门审计 → S01 长线故事 | `GATE_BLOCKED` | 不跑正式全版本长线 |
| `D16-C11` | D16 章节正式放行 | `GATE_BLOCKED` | 不宣称 D16 可对外发布 |

对应缺件（补齐前禁止伪通过）：

1. `tools/results/external/d16_a01_target_device.json` — 目标机 + 低端机各 ≥60 秒
2. `tools/results/external/d16_a02_power_cross_platform.json` — 真断电 + 至少两平台
3. `tools/results/external/d16_a03_device_accessibility.json` — 已唤醒物理手柄 + 人工无障碍
4. `tools/results/external/d16_a06_target_release_privacy.json` — 目标机安装/更新/卸载 + 崩溃 + 隐私

## 开发阶段仍要做

- 本机逻辑、切片可玩、C01—C08 / C12、D17 切片文档与本地闭环
- D18—D24 纯逻辑轨（与本轨状态词分家）
- Steam 保持未授权、未上传

## 开发阶段不要做

- 不要用 RTX 4070 Ti / PlayMode / 合成手柄填上述四个 envelope 并标 `PASS`
- 不要把 C09—C11 的 `GATE_BLOCKED` 改成 `GATE_PASS`
- 不要因本机绿而关闭 Steam 或宣称正式发布

## 何时再做

进入**预发布 / 真机验收波次**（有目标机或 Steam Deck、物理断电条件、已唤醒手柄）后再开 C09 → C10 → C11。证据格式仍以 [D16_EXTERNAL_ACCEPTANCE_RUNBOOK.md](D16_EXTERNAL_ACCEPTANCE_RUNBOOK.md) 为准。
