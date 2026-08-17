# 第 9B 章：D16 / D17 剩余发布门禁（合并后 16 项）

> 本章只处理 **D18 之前仍未正式收口** 的工作。可复制提示词在 [GATE_PROMPTS](AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATE_PROMPTS.md)。要从 C06 一次串行做到 D17-C04，复制 [单 Agent 顺序总提示词](AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_SERIAL_RUNNER.md)。协议见 [GATE_TASK_PROTOCOL.md](GATE_TASK_PROTOCOL.md)。状态由 `python tools/sync_gate_task_status.py` 汇总到 [GATE_TASK_STATUS.md](GATE_TASK_STATUS.md)。
>
> **2026-08-17 合并修订**：原 47 项过细。`D16-C01`—`C05` 已下发，编号保留。`D16-C06` 起与全部 `D17-Cxx` 按 agent 一次能做完的粒度合并。旧 `D16-C13`—`C31`、旧 `D17-C05`—`C16` **从未开工，禁止再派**。
>
> **2026-08-18 开发阶段推迟**：`D16-C09` / `C10` / `C11` 的真机、断电、手柄、目标机发布实测**暂不列入开发阶段必做**。结果保持 `GATE_BLOCKED`，详见 [D16_EXTERNAL_HARDWARE_DEFERRED.md](design/common/D16_EXTERNAL_HARDWARE_DEFERRED.md)。禁止把 BLOCKED 改成 PASS。开发继续走切片与逻辑轨。

## 轨道边界

- **本轨**：真实设备、人工审查、授权最终资源、断电/跨平台、隐私、安装器和正式 D17 故事。
- **逻辑轨**：D18—D24 / 当前 D19。两边可同时推进，结果文件和状态词不得混用。
- 旧 `D16-A/R/S`、`D17-A/S` 只是历史身份。

## 当前分派编号

| 集合 | 编号 | 数量 | 说明 |
|---|---|---:|---|
| 已下发保留 | `D16-C01`—`C05` | 5 | 基线 + A04 批次 001—004；C05 进行中，不要改提示词 |
| D16 合并后 | `D16-C06`—`C12` | 7 | 剩余审查、资源、外证、S01/S02、旁路 |
| D17 合并后 | `D17-C01`—`C04` | 4 | 冻结+资源、运行时切片、真机回归、正式验收 |
| 作废勿派 | 旧 `D16-C13`—`C31`、旧 `D17-C05`—`C16` | — | SUPERSEDED |

## 执行顺序

```text
已完成 / 进行中（不要重派、不要改提示词）
  D16-C01  GATE_PASS
  D16-C02 / C03 / C04  已下发的 A04 001—003（以结果 JSON 为准）
  D16-C05  进行中：A04 批次 004

16.1b 串行或单 agent：D16-C06
      做完剩余资产审查批次 005—008
  ↓ C02、C03、C04、C05、C06 全部 GATE_PASS
16.2  串行：D16-C07
      历史内容批次 009—012 + A04 关闭 envelope
  ↓ C07 = GATE_PASS
16.3  串行：D16-C08
      A05 预检 + 切片资源包 + 接入计划 + 切片关闭
  ↓ C08 = GATE_PASS
16.4  串行：D16-C09
      四份外部证据：目标机性能 / 断电跨平台 / 真机 a11y / 目标机发布隐私
  ↓ C09 = GATE_PASS
16.5  严格串行：D16-C10
      六门审计 → S01 就绪 → 长线故事 → 故事复核
  ↓ C10 = GATE_PASS
16.6  严格串行：D16-C11
      S02 交接 + D16 章节放行
  ↓ C11 = GATE_PASS 后才允许 D17-C01
16.X  旁路，C01 后可做、不是 S01 前置：D16-C12
      本机安装器回归 + S01 夹具 + Steam 保持未授权

17.0  串行：D17-C01   切片冻结 + 角色/环境最终资源登记
17.1  串行：D17-C02   物理 → 上下船 → 镜头 → 动作 → 失败恢复
17.2  串行：D17-C03   真机输入 + 目标性能 + 资源场景回归
17.3  串行：D17-C04   正式 10 次故事 + 重启复核 + 状态审计 + 章节验收
```

同一 agent 一次只领一项合并任务。前置不是 `GATE_PASS` 时写真实 `GATE_BLOCKED`。

## D16 任务卡

| ID | 模式 | 映射 | 写入范围（唯一） | 一次做完的内容 |
|---|---|---|---|---|
| D16-C01 | 已过 | 全章 | `D16_GATE_CLOSURE_BASELINE.md` | 已冻结。只读，勿重做。 |
| D16-C02 | 已下发 | A04-001 | `reviews/D16_A04_BATCH_001.md` | 批次 001，保持原提示词。 |
| D16-C03 | 已下发 | A04-002 | `reviews/D16_A04_BATCH_002.md` | 批次 002。 |
| D16-C04 | 已下发 | A04-003 | `reviews/D16_A04_BATCH_003.md` | 批次 003。 |
| D16-C05 | 进行中 | A04-004 | `reviews/D16_A04_BATCH_004.md` | 批次 004。**不要改它的提示词。** |
| D16-C06 | S | A04-005—008 | `reviews/D16_A04_BATCH_005.md`—`008.md` | 剩余 86 条资产授权审查。 |
| D16-C07 | S | A04-009—012 + 关闭 | `reviews/D16_A04_BATCH_009.md`—`012.md`、`tools/results/external/d16_a04_manual_review.json` | 81 条历史内容 + `pending_count=0` envelope。 |
| D16-C08 | S | A05 | 切片最终资源目录、替换 manifest、预检 JSON、接入计划、`d16_a05_final_assets.json` | 方案 B：自行检索可商用免费资源，只替换切片 4 项并先保证能运行。 |
| D16-C09 | S | A01/A02/A03/A06 | 四份 `tools/results/external/d16_a0{1,2,3,6}_*.json` | 四门外证一次收齐。缺真机就 BLOCKED。 |
| D16-C10 | S | 审计 + S01 | 外部审计 JSON、S01 就绪 JSON、长线报告增量、C10 handoff | 审计通过后立刻跑长线并复核。 |
| D16-C11 | G | S02 + 放行 | `D16_FINAL_HANDOFF_INDEX.md` 增量、`D16_GATE_ACCEPTANCE.md` | 交接 + 是否放行 D17。 |
| D16-C12 | 旁路 | R02 / Steam | C12 结果目录、`D16_S01_STORY_FIXTURE.md`、`D16_STEAM_HOLD.md` | 本机安装器 + 夹具 + 确认 Steam 未上传。 |

## D17 任务卡

| ID | 模式 | 映射 | 写入范围（唯一） | 一次做完的内容 |
|---|---|---|---|---|
| D17-C01 | S | 旧 S01 + A05 准备 | `D17_SLICE_FREEZE.md`、`D17_SLICE_CHARACTER_ASSETS.md`、`D17_SLICE_ENV_ASSETS.md` | 冻结范围，并登记最终角色/船/码头/沙滩资源。不改运行时。 |
| D17-C02 | S | 旧 A01/A03/A04/A02/A06 | 开工时在 handoff 冻结的 D2/D6 运行时文件 | 物理、上下船、镜头焦点、正式动作、失败恢复，一条切片做完。 |
| D17-C03 | S | 真机/性能/资源 | `d17_device_input.json`、`d17_target_perf.json`、`D17_SLICE_ASSET_REGRESSION.md` | 真机输入点 + 目标窗口采样 + 场景资源回归。 |
| D17-C04 | G | 旧 S02 | 故事脚本、`d17_s02_ten_cycles.json`、重启审计、状态审查、`D17_GATE_ACCEPTANCE.md` | 正式 10 次 + 验收。本机旧 1/1 不得复用。 |

## 完成后每项都要留痕

1. `game/tools/results/[DXX]/[TASK_ID]/[TASK_ID下划线]_GATE_RESULT.json`
2. `docs/game/design/common/handoffs/[DXX]/[TASK_ID]_GATE_HANDOFF.md`
3. 当天 `.workbuddy/memory/YYYY-MM-DD.md`
4. `python tools/sync_gate_task_status.py`
5. `git diff --check`

## 依赖细则

- `D16-C05` 未提交前，不要派 `D16-C07`。`C07` 必须等 `C02`—`C06` 全过。
- `D16-C06` 可以在 `C05` 仍进行时开始，只写批次 005—008，不准碰 004。
- A04 写回仍只能 `tools/d16_a04_apply_review_decision.py` **单条**；禁止脚本批批。
- `D16-C08` 不得把未替换 placeholder 改状态清零。
- `D16-C09` 证据禁止 `synthetic` / `editor_only` / `simulated`。开发阶段不采集真机，C09 保持 `GATE_BLOCKED`（见 D16_EXTERNAL_HARDWARE_DEFERRED.md）。
- `D16-C12` 不是 `D16-C10` 的前置。
- `D17-C01` 硬前置原为 `D16-C11 = GATE_PASS`。开发阶段已豁免该硬前置以便推进切片，**不等于 C11 已通过**。
- Steam 未授权不是失败；`D16-C12` 的通过标准是确认未上传。
