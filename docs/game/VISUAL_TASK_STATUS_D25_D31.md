# D25—D31 视觉与交互任务状态

> 自动生成：`python tools/sync_visual_task_status.py`。每个任务的唯一事实源是其 `*_RESULT.json`。
> 本看板不覆盖 D18—D24 逻辑状态，也不改变 D16/D17 发布门禁。

> 最后同步（UTC）：2026-09-03T16:17:23+00:00

| 章节 | 计划任务 | PASS/ART_PASS | SCENE_PASS | RUNTIME_PASS | PARTIAL | BLOCKED | IN_PROGRESS | PLANNED |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| D25 | 7 | 5 | 0 | 0 | 2 | 0 | 0 | 0 |
| D26 | 6 | 4 | 0 | 0 | 2 | 0 | 0 | 0 |
| D27 | 6 | 4 | 0 | 0 | 2 | 0 | 0 | 0 |
| D28 | 6 | 4 | 0 | 0 | 2 | 0 | 0 | 0 |
| D29 | 6 | 4 | 0 | 0 | 2 | 0 | 0 | 0 |
| D30 | 6 | 4 | 0 | 0 | 2 | 0 | 0 | 0 |
| D31 | 6 | 2 | 0 | 0 | 4 | 0 | 0 | 0 |

## 任务明细

| 任务 | 模式 | 工作内容 | 状态 | 更新时间 | 结果 JSON | 下一个任务 |
|---|---|---|---|---|---|---|
| D25-S01 | S | 基线冻结与视觉债务清点 | PASS | 2026-09-02T20:30:00+08:00 | [D25_S01_RESULT.json](../../game/tools/results/D25/D25-S01/D25_S01_RESULT.json) | D25-P02—P05 |
| D25-P02 | P | 统一视觉主题、材质和质量档 | ART_PASS | 2026-09-02T20:45:00+08:00 | [D25_P02_RESULT.json](../../game/tools/results/D25/D25-P02/D25_P02_RESULT.json) | D25-S06 |
| D25-P03 | P | 相机、天空、灯光和雾 | ART_PASS | 2026-09-02T20:45:00+08:00 | [D25_P03_RESULT.json](../../game/tools/results/D25/D25-P03/D25_P03_RESULT.json) | D25-S06 |
| D25-P04 | P | 玩家模式 HUD 清理和信息层级 | ART_PASS | 2026-09-02T20:45:00+08:00 | [D25_P04_RESULT.json](../../game/tools/results/D25/D25-P04/D25_P04_RESULT.json) | D25-S06 |
| D25-P05 | P | 资源和 Prefab 视觉准入验证器 | ART_PASS | 2026-09-02T21:40:00+08:00 | [D25_P05_RESULT.json](../../game/tools/results/D25/D25-P05/D25_P05_RESULT.json) | D25-S06 |
| D25-S06 | S | 视觉基础正式场景集成 | PARTIAL | 2026-09-03T16:12:54+08:00 | [D25_S06_RESULT.json](../../game/tools/results/D25/D25-S06/D25_S06_RESULT.json) | D25-G07 |
| D25-G07 | G | 视觉基础汇合门禁 | PARTIAL | 2026-09-03T02:50:41+08:00 | [D25_G07_RESULT.json](../../game/tools/results/D25/D25-G07/D25_G07_RESULT.json) | D26/D27/D28/D29 |
| D26-P01 | P | 自然地形和海岸层次 | ART_PASS | 2026-09-02T21:05:00+08:00 | [D26_P01_RESULT.json](../../game/tools/results/D26/D26-P01/D26_P01_RESULT.json) | D26-S05 |
| D26-P02 | P | 港口建筑和功能轮廓 | ART_PASS | 2026-09-02T21:05:00+08:00 | [D26_P02_RESULT.json](../../game/tools/results/D26/D26-P02/D26_P02_RESULT.json) | D26-S05 |
| D26-P03 | P | 植被、岩石和风响应 | ART_PASS | 2026-09-02T21:05:00+08:00 | [D26_P03_RESULT.json](../../game/tools/results/D26/D26-P03/D26_P03_RESULT.json) | D26-S05 |
| D26-P04 | P | 港口道具和生活痕迹 | ART_PASS | 2026-09-02T21:05:00+08:00 | [D26_P04_RESULT.json](../../game/tools/results/D26/D26-P04/D26_P04_RESULT.json) | D26-S05 |
| D26-S05 | S | 环境正式场景集成 | PARTIAL | 2026-09-03T09:28:37+08:00 | [D26_S05_RESULT.json](../../game/tools/results/D26/D26-S05/D26_S05_RESULT.json) | D26-G06 |
| D26-G06 | G | 环境视觉汇合门禁 | PARTIAL | 2026-09-03T09:28:37+08:00 | [D26_G06_RESULT.json](../../game/tools/results/D26/D26-G06/D26_G06_RESULT.json) | D27/D28/D29 |
| D27-P01 | P | 玩家身体、发型、服装和手持物 | ART_PASS | 2026-09-02T21:25:00+08:00 | [D27_P01_RESULT.json](../../game/tools/results/D27/D27-P01/D27_P01_RESULT.json) | D27-S05 |
| D27-P02 | P | 岗位可读 NPC | ART_PASS | 2026-09-02T21:25:00+08:00 | [D27_P02_RESULT.json](../../game/tools/results/D27/D27-P02/D27_P02_RESULT.json) | D27-S05 |
| D27-P03 | P | 动物、宠物和水生生物 | ART_PASS | 2026-09-02T21:25:00+08:00 | [D27_P03_RESULT.json](../../game/tools/results/D27/D27-P03/D27_P03_RESULT.json) | D27-S05 |
| D27-P04 | P | 动画、锚点、LOD 和表现回退 | ART_PASS | 2026-09-02T21:25:00+08:00 | [D27_P04_RESULT.json](../../game/tools/results/D27/D27-P04/D27_P04_RESULT.json) | D27-S05 |
| D27-S05 | S | 角色和动物运行时集成 | PARTIAL | 2026-09-03T23:51:47+08:00 | [D27_S05_RESULT.json](../../game/tools/results/D27/D27-S05/D27_S05_RESULT.json) | D27-G06 |
| D27-G06 | G | 角色与动物视觉门禁 | PARTIAL | 2026-09-03T02:50:41+08:00 | [D27_G06_RESULT.json](../../game/tools/results/D27/D27-G06/D27_G06_RESULT.json) | D28/D29 |
| D28-P01 | P | 船体、甲板、桅杆、帆和锚 | ART_PASS | 2026-09-02T21:30:00+08:00 | [D28_P01_RESULT.json](../../game/tools/results/D28/D28-P01/D28_P01_RESULT.json) | D28-S05 |
| D28-P02 | P | 海水、泡沫、浅水和礁石 | ART_PASS | 2026-09-02T21:30:00+08:00 | [D28_P02_RESULT.json](../../game/tools/results/D28/D28-P02/D28_P02_RESULT.json) | D28-S05 |
| D28-P03 | P | Calm、Rain、Storm 天气表现 | ART_PASS | 2026-09-02T21:30:00+08:00 | [D28_P03_RESULT.json](../../game/tools/results/D28/D28-P03/D28_P03_RESULT.json) | D28-S05 |
| D28-P04 | P | 天空、远景、雾和后处理 | ART_PASS | 2026-09-02T21:30:00+08:00 | [D28_P04_RESULT.json](../../game/tools/results/D28/D28-P04/D28_P04_RESULT.json) | D28-S05 |
| D28-S05 | S | 船海天气正式集成 | PARTIAL | 2026-09-03T17:44:35+08:00 | [D28_S05_RESULT.json](../../game/tools/results/D28/D28-S05/D28_S05_RESULT.json) | D28-G06 |
| D28-G06 | G | 船海天气汇合门禁 | PARTIAL | 2026-09-03T17:44:35+08:00 | [D28_G06_RESULT.json](../../game/tools/results/D28/D28-G06/D28_G06_RESULT.json) | D29/D30 |
| D29-P01 | P | 主 HUD 和状态布局 | ART_PASS | 2026-09-02T21:30:00+08:00 | [D29_P01_RESULT.json](../../game/tools/results/D29/D29-P01/D29_P01_RESULT.json) | D29-S05 |
| D29-P02 | P | 市场、仓库、酒馆、船坞等面板 | ART_PASS | 2026-09-02T21:30:00+08:00 | [D29_P02_RESULT.json](../../game/tools/results/D29/D29-P02/D29_P02_RESULT.json) | D29-S05 |
| D29-P03 | P | 按钮、焦点和命令桥接 | ART_PASS | 2026-09-02T21:30:00+08:00 | [D29_P03_RESULT.json](../../game/tools/results/D29/D29-P03/D29_P03_RESULT.json) | D29-S05 |
| D29-P04 | P | 本地化、缩放和可访问性 | ART_PASS | 2026-09-02T21:30:00+08:00 | [D29_P04_RESULT.json](../../game/tools/results/D29/D29-P04/D29_P04_RESULT.json) | D29-S05 |
| D29-S05 | S | 正式 UI Canvas 集成 | PARTIAL | 2026-09-04T00:06:07+08:00 | [D29_S05_RESULT.json](../../game/tools/results/D29/D29-S05/D29_S05_RESULT.json) | D29-G06 |
| D29-G06 | G | UI 交互汇合门禁 | PARTIAL | 2026-09-03T22:02:43+08:00 | [D29_G06_RESULT.json](../../game/tools/results/D29/D29-G06/D29_G06_RESULT.json) | D30 |
| D30-P01 | P | 逻辑状态到视觉反馈桥 | ART_PASS | 2026-09-02T21:35:00+08:00 | [D30_P01_RESULT.json](../../game/tools/results/D30/D30-P01/D30_P01_RESULT.json) | D30-S05 |
| D30-P02 | P | 相机过渡和流程表现 | ART_PASS | 2026-09-02T21:35:00+08:00 | [D30_P02_RESULT.json](../../game/tools/results/D30/D30-P02/D30_P02_RESULT.json) | D30-S05 |
| D30-P03 | P | 保存加载后的视觉恢复 | ART_PASS | 2026-09-02T21:35:00+08:00 | [D30_P03_RESULT.json](../../game/tools/results/D30/D30-P03/D30_P03_RESULT.json) | D30-S05 |
| D30-P04 | P | 音效、环境声、粒子和动作反馈 | ART_PASS | 2026-09-02T21:35:00+08:00 | [D30_P04_RESULT.json](../../game/tools/results/D30/D30-P04/D30_P04_RESULT.json) | D30-S05 |
| D30-S05 | S | 视觉可玩完整路线 | PARTIAL | 2026-09-03T17:44:35+08:00 | [D30_S05_RESULT.json](../../game/tools/results/D30/D30-S05/D30_S05_RESULT.json) | D30-G06 |
| D30-G06 | G | 视觉可玩汇合门禁 | PARTIAL | 2026-09-03T17:44:35+08:00 | [D30_G06_RESULT.json](../../game/tools/results/D30/D30-G06/D30_G06_RESULT.json) | D31 |
| D31-P01 | P | LOD、遮挡、批处理和对象池 | ART_PASS | 2026-09-02T21:35:00+08:00 | [D31_P01_RESULT.json](../../game/tools/results/D31/D31-P01/D31_P01_RESULT.json) | D31-S05 |
| D31-P02 | P | 贴图、Shader、阴影和粒子预算 | ART_PASS | 2026-09-02T21:35:00+08:00 | [D31_P02_RESULT.json](../../game/tools/results/D31/D31-P02/D31_P02_RESULT.json) | D31-S05 |
| D31-P03 | P | 1080p 三档画质与性能采样 | PARTIAL | 2026-09-03T05:24:30+08:00 | [D31_P03_RESULT.json](../../game/tools/results/D31/D31-P03/D31_P03_RESULT.json) | D31-S05 |
| D31-P04 | P | 固定镜头、天气和 UI 回归 | PARTIAL | 2026-09-06T02:15:57+08:00 | [D31_P04_RESULT.json](../../game/tools/results/D31/D31-P04/D31_P04_RESULT.json) | D31-S05 |
| D31-S05 | S | Visual Candidate 正式整合 | PARTIAL | 2026-09-06T02:15:57+08:00 | [D31_S05_RESULT.json](../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json) | D31-G06 |
| D31-G06 | G | 视觉与交互候选版总验收 | PARTIAL | 2026-09-06T02:15:57+08:00 | [D31_G06_RESULT.json](../../game/tools/results/D31/D31-G06/D31_G06_RESULT.json) | 后续 Runtime/Release 门禁 |

## 2026-09-06 R87 状态更新

- `D31-P04` 新增 R87 正式复核记录：真实 Player 固定镜头 `14/14`、天气变体 `3/3`，全部为 `1920x1080` backbuffer；状态仍为 `PARTIAL`，因为这批证据只证明画面文件可采集，未关闭最终低模质量、真实交互路线和性能门禁。
- `D31-G06` 已登记 R87 与当前全量回归，但继续为 `PARTIAL`；`D31-S05` 不因镜头采集自动升级。
- `VC-G01` 继续为 `IN_PROGRESS`：R87 没有真实 WASD、转向、登船、下船、仓库/市场/酒馆成功/失败/重复/关闭恢复证据。
- `VC-G02`、`VC-G03` 继续锁定；不得把 R87 的视觉采集结果写成 D31 或 VC-G01 的最终通过。

## 规则

- P 任务只写独立工作目录/Prefab/材质/测试；S 任务是正式场景或正式 Canvas 的唯一写者。
- G 任务只审计和汇合，不得顺手修改其他章节。
- `RUNTIME_PASS` 必须来自当前 Player 的真实运行过程；编辑器预览、静态计数和编译成功不能替代它。
- `VISUAL_CANDIDATE` 不等于 `RuntimeApproved` 或 `ReleaseApproved`。
