# D28-S05 船海天气正式集成交接

## 状态

`PARTIAL`

## 已完成

- 水面、泡沫、天气预览和降水表现入口已接入运行时。
- R8/R11 Player 已确认海岸/水面表现可见；R6 已验证登船、出航、航行和靠岸入口。
- 天气表现只读公开状态/预览，不裁决 D21 航行事实。

## 未完成

- Calm、Rain、Storm 的完整航行中切换、返回和恢复证据尚未收口。
- 船体、锚点、泡沫、雾、粒子和 UI 在三天气下的最终画面质量与性能门禁仍缺失。

## 证据

- [R6 路线报告](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R6.md)
- [R8 runtime 日志](../../../../game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R8.log)
- [D28 结果](../../../../game/tools/results/D28/D28-S05/D28_S05_RESULT.json)

## 2026-09-03 R21-R23 表现复核

R21-R22 修复了降水层固定在世界原点、粒子不易读取的问题：Rain/Storm 现在跟随主相机，使用透明拉伸粒子，并按天气区分雨量、风向、颜色和水面波动倍率。R23 在独立 Player 中确认 14 个固定镜头和 Calm/Rain/Storm 三天气证据仍完整落盘，Rain/Storm 雨丝可读；同时复核 D30 路线条已移入港口标题安全区。

- [R23 视觉证据 JSON](../../../../../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R23.json)
- [R23 截图目录](../../../../../../game/tools/results/D31/D31-P04/visual_evidence_R23/)
- [R23 Player 构建日志](../../../../../../game/tools/results/D31/D31-P04/D31_P04_STANDALONE_BUILD_R23.log)

R21-R23 只关闭“天气完全不可读”这一具体缺陷；低模船体/建筑、泡沫/雾/UI 的整体质量和性能 1% low 门禁仍未闭环，D28-S05/G06 保持 `PARTIAL`。
