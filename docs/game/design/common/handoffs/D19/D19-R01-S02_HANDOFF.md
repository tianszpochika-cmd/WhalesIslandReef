# D19-R01-S02 交接报告

- 任务编号：D19-R01-S02
- 日期：2026-08-15
- 结论：**PARTIAL**
- D20-S02：**不放行**

## 本轮实际修改

本轮只新增验收结果、报告、交接和记忆；没有修改 `InitialHarbor` 或 D19 Integration Prefab。正式场景起止 SHA 均为 `4C45779C7B5A020583E9F11F9F8F61642C12F1EC3920E8001F4752EF626D7E9D`。D18 资源状态、Registry、Systems、Save Schema、contracts/server/web 均未改。

## 真实结果

### 资源

- R01-B 的 53 条使用记录去重为 32 个外部 `content_id`；32/32 在最新 intake catalog 的 `intake_eligible=true` 条目中可定位；缺失 0。
- 当前 catalog 快照：manifest 28、准入 manifest 24、候选文件 1,863、准入文件 1,833。
- `LocalDevelopmentReady=0`、`ReleaseApproved=0`，本轮没有晋级。
- 结论为 `PASS_STATIC_ONLY`，不等于运行时资源就绪。

### 路线

- Main Menu 真实 Player 操作未观察到 New Game→InitialHarbor 转换，不能计为完成。
- 直接 InitialHarbor 补充尝试从 11:19:42 到最后有效 route_09 帧 11:23:56，约 254 秒，低于要求的 300 秒。
- 有效 1920×1080 Player 帧 10 张；无完整 NPC、市场、登船、出航、返港、保存重载闭环。
- Player 日志出现持续 BeachGround 恢复输出，未给出完整交互反馈。

### 性能

Low/Medium/High 都有真实 Player 的 30 秒 warm-up + 60 秒采样，但均未达到 D18 预算：

| 档位 | 平均 FPS | 1% low | 结果 |
|---|---:|---:|---|
| Low | 13.54 | 0.18 | FAIL |
| Medium | 57.85 | 11.62 | FAIL |
| High | 59.18 | 21.49 | FAIL |

GPU timing unavailable，工作集/私有内存返回 0/0 无法作为有效零值；报告同时保留 sampler 的 3840×2160 current-resolution 不一致信号。

### 视觉与固定镜头

- 当前 Player 未完成 14 个固定验收镜头和天气变体；R01-B 的 14 张 Editor Camera.Render 仍是历史证据，未转作本轮 Player 通过证据。
- 当前出生/主街/市场邻接帧仍看到分面低细节地形、方块化建筑/道具和大面积调试/教程叠字；没有在已捕获帧中看到粉色材质，但不是全场景证明。
- 视觉门槛未通过。

### 回归

- 隔离副本 D19 EditMode：23/23 passed、0 failed、0 skipped。
- PlayMode 全 D19 和单独场景用例都在 Unity `PerformanceTestRunBuilder` 预构建阶段超时，无 XML；不计为 PASS。
- 主项目已有 D20 测试编译错误 `WhalesIslandReef.Presentation` 缺失，本轮未越权修复。
- D17 本轮未重新分类为通过；历史 XML 保留。

## 结果文件

- 总机器结果：[D19_R01_S02_MACHINE_RESULT.json](../../../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_MACHINE_RESULT.json)
- 资源引用审计：[D19_R01_S02_RESOURCE_REFERENCE_AUDIT.json](../../../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_RESOURCE_REFERENCE_AUDIT.json)
- 路线：[D19_R01_S02_ROUTE_LOG.json](../../../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_ROUTE_LOG.json)
- 性能汇总：[D19_R01_S02_PERFORMANCE_1080P.json](../../../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_PERFORMANCE_1080P.json)
- 视觉检查：[D19_R01_S02_VISUAL_QA.json](../../../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_VISUAL_QA.json)
- PlayMode 超时：[D19_R01_S02_PLAYMODE_TIMEOUT.json](../../../../../../game/tools/results/D19/D19-R01-S02/D19_R01_S02_PLAYMODE_TIMEOUT.json)
- 截图目录：[screenshots](../../../../../../game/tools/results/D19/D19-R01-S02/screenshots/)
- 主报告：[D19_SAINT_MARGARET_ACCEPTANCE_R01_S02.md](../../D19_SAINT_MARGARET_ACCEPTANCE_R01_S02.md)

## 交接注意事项

1. 先由新的 D19 修复任务处理真实 Player 交互闭环、调试叠字、灰盒/方块视觉和性能超限；不要修改强逻辑或把显示层当作交互裁决者。
2. 重新验收必须从 Main Menu 角色创建开始，完成至少 5 分钟连续路线，并对保存后的重载做实际验证。
3. 重新采集 Low/Medium/High 的目标分辨率一致性、GPU timing、工作集、CPU p95 和 1% low；不能复用本轮失败数据为 PASS，也不能复用历史 R01-A/R01-B 的不完整数据。
4. PlayMode 预构建超时需要单独修复/诊断；D20 的缺失命名空间仍由 D20 责任任务处理，本任务不绕过它。
5. D18-S03 尚未执行；`intake_eligible` 不等于 `LocalDevelopmentReady`，不能在 D19 或 D20 任务中擅自改变资源状态。

## 验证命令

```text
python tools/d18_prefab_candidate_intake_audit.py --project-root . --catalog docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json --output game/tools/results/D19/D19-R01-S02/D19_R01_S02_INTAKE_AUDIT.json  # PASS
python tools/d18_asset_manifest_audit.py --project-root . --manifest-root docs/game/design/common/assets/packs --output game/tools/results/D19/D19-R01-S02/D19_R01_S02_MANIFEST_AUDIT.json  # PASS
python -m unittest tools.tests.test_d18_prefab_candidate_intake tools.tests.test_d18_asset_manifest_audit  # 11/11 OK
git diff --check  # exit 0; existing line-ending warnings only
```

`pytest` 命令因当前 Python 环境缺少 pytest 模块而退出 1；没有把该命令伪报为通过。Unity EditMode 23/23 的 XML 和 PlayMode 超时日志已随结果目录保存。
