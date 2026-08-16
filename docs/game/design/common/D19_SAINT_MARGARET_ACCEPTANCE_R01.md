# D19-S02-R01 圣玛格丽特港本地样板复验报告

**任务**：D19-S02-R01  
**日期**：2026-08-14  
**结论**：**PARTIAL**

## 1. 结论摘要

本轮保留原 D19-S02 的 PARTIAL 报告、原始结果和旧回滚快照，并在新的 R01 目录建立了独立回滚基线。正式场景的 D19 集成层、锚点、材质、碰撞和自动化回归证据可复核，但三个强制门槛仍未满足：

1. 没有完成真实玩家控制的 5—10 分钟路线；
2. 没有取得真实本地 Player 的 1920×1080、Low/Medium/High 三档 60 秒性能采样；
3. 现有 R01 编辑器截图的人工视觉检查仍发现出生/海滩、市场、码头和近海区域存在大面积低细节地表、明亮白色/薄荷色区域或块状建筑观感。

因此不能将 R01 标记为 PASS，也不能把自动化运行时探针当作真人路线或目标设备性能证据。

## 2. 历史和边界

- D18-S02-v2 仍为 PASS；D18-S03 尚未执行。
- `LocalDevelopmentReady=0`、`ReleaseApproved=0`，本轮没有修改任何资源状态、manifest 或准入目录。
- D19-S01 Attempt 2 仍为 PASS。
- 原 D19-S02 报告和机器结果保持不变，见 `docs/game/design/common/D19_SAINT_MARGARET_ACCEPTANCE.md` 与 `game/tools/results/D19/D19-S02/`。
- 本轮没有修改 Systems、AgentCore、Registry、存档 Schema、contracts、server 或 web。

## 3. R01 回滚基线

基线文件：`game/tools/results/D19/D19-S02-R01/R01_PreVerificationBaseline/D19_S02_R01_PREVERIFICATION_BASELINE.json`。

| 项目 | 基线值 |
|---|---|
| 正式场景 | `game/Assets/Scenes/InitialHarbor.unity` |
| 场景 SHA-256 | `3E081A48B4D7B53E9084308B86587170527E006C637EC0B280E074449C8B02AA` |
| 场景字节数 | 133,143 |
| 场景 YAML GameObject 计数 | 24 |
| Prefab 实例计数 | 26 |
| MonoBehaviour 计数 | 7 |
| 稳定交互锚点 | 20 |
| 兼容节点 | `Dock`, `DockDeck`, `Gangplank`, `Beach`, `Sea`, `Main Camera`, `Harbor UI`, `HarborControls` |

回滚时使用该 R01 基线恢复 `InitialHarbor.unity` 及其 `.meta`，再按 `D19_S02_R01_INTEGRATION_RESULT.json` 中的 `d19-s02-r01-preintegration-baseline` 移除 R01 集成引用；不得删除稳定 ID、旧兼容节点或 D19-A01—A06 源工作场景。

## 4. 本轮实际集成和静态验证

R01 集成层保留并验证了项目自有的地形/港口表现细节、屋顶、立面、门、货箱、布料、码头护栏、系船柱、礁石和水体层；外部候选没有被提升状态或绕过准入目录。

当前可复核统计：

- 集成 Prefab 实例：776；
- 集成 Renderer：606；
- 集成 Collider：145；
- LODGroup：66；
- 稳定锚点：20；
- 材质缺失：0；
- Error Shader：0；
- Standard Shader：0；
- 粉色像素审计：0；
- 旧兼容节点：保留；
- Systems/contracts/Registry/存档 Schema：未修改。

这些是结构和材质门槛，不等于视觉质量门槛已经通过。

## 5. 视觉复验

R01 目录内有 16 张 1920×1080 PNG：14 个固定镜头、3 个天气变体以及其他辅助镜头。它们是编辑器 `Camera.Render` 记录，不是本地 Player 截图，故不能作为玩家镜头通过证据。人工检查结果见 `D19_S02_R01_VISUAL_QA.json`。

| 区域 | 结果 | 实际发现 |
|---|---|---|
| 出生点/海滩入口 | PARTIAL | 有地形和植被层，但宽阔的多边形地表及明亮白色海滩带仍明显。 |
| 市场/主街 | PARTIAL | 已有屋顶、立面、门、货箱和布料；远景仍呈块状建筑和稀疏地表。 |
| 码头/船坞/泊位 | PARTIAL | 护栏、系船柱、货箱和屋顶可读；整体细节密度仍不足，且没有本地 Player 证据。 |
| 海岸/浅水/近海/远景 | PARTIAL | 有礁石/水体对象，但仍存在大片薄荷色/白色区域和稀疏远景。 |

因此“无明显灰盒、裸方块、白平面”的强制视觉门槛未通过。缺口责任应回到 D19 集成表现/环境资源任务和后续 D18-S03 真实晋级审查，不得以浓雾、裁切或关闭对象掩盖。

## 6. 真人路线和交互

自动化 PlayMode 探针已通过 `spawn → walk → board → sail → disembark → save → reload` 的技术链路，实际运行约 2.5 秒；它验证了登船、下船、保存调用和场景重载后的 20 个锚点，但其中的登船/保存使用测试代码反射调用，不是用户输入驱动的完整真人路线。

本轮已成功构建本地 Windows Player，但在准备获取窗口状态和输入控制时，Computer Use 被用户物理 Escape 中止。依照安全规则没有继续发送窗口输入，因此以下项目均为 NOT_RUN：

- 连续 5—10 分钟实际玩家路线；
- 出生 → 市场 → 仓库 → 酒馆 → 码头 → 登船 → 出航 → 返港 → 保存 → 重载的逐节点时间戳、输入和屏幕反馈；
- 至少 9 张真人路线截图。

这不是通过条件，整体只能 PARTIAL。

## 7. 性能复验

本地 Player 构建成功，但未完成真实窗口采样。现有两类数据必须分开解释：

- 编辑器 1920×1080 `Camera.Render`：平均 21.0956 ms，P95 306.1391 ms；不代表目标设备或 PlayMode。
- Batch PlayMode 探针：请求 1920×1080，实际 640×480，120 帧，平均 16.6483 ms，P95 17.957 ms，GPU timing 不可用；不满足 D18 三档目标设备门槛。

Low/Medium/High 每档 warm-up 30 秒、采样 60 秒、CPU/GPU/GC/内存和设备信息均未完成。完整记录见 `D19_S02_R01_PERFORMANCE_1080P_RERUN_V2.json`。

## 8. 回归

| 回归 | 结果 |
|---|---:|
| D19-S02 EditMode | 4 passed / 0 failed / 0 skipped |
| 全部 D19 EditMode 过滤集 | 15 passed / 0 failed / 0 skipped |
| D19-S02 PlayMode | 1 passed / 0 failed / 0 skipped |
| R01 自动化路线/性能探针 | 2 passed / 0 failed / 0 skipped |
| D16/D17 核心 PlayMode | 63 passed / 0 failed / 1 skipped（Ignored） |

结果 XML、日志和机器汇总在 `game/tools/results/D19/D19-S02-R01/`。自动回归通过不替代真人路线、视觉和目标设备性能门禁。

## 9. 下一步

1. 在不改 Systems、稳定 ID、存档和资源状态的前提下，继续补齐四个区域的合法表现资产密度与地表/岸线层次；缺少合法候选时登记 D18 修复任务。
2. 在用户可操作的真实 Player 中重新执行 5—10 分钟路线并保存至少 9 张路线截图。
3. 在真实 1920×1080 Player 中按 Low/Medium/High 三档执行 30 秒 warm-up + 60 秒采样，补齐 GPU、CPU、GC、内存和设备数据。
4. 完成后再提交 D19-S02-R01 复验，并由 D18-S03 统一裁决资源是否能晋级 `LocalDevelopmentReady`。

