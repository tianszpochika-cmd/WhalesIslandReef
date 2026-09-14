# VC-S00 视觉清理基线交接

结论：`PASS_WITHIN_SCOPE`。

本任务只登记当前 R57 standalone Player 的事实和问题，不宣称视觉完成，也不升级 D25—D31 正式门禁。

## 已确认

- R03 正式港口 Prefab 能出现在 Windows x64 Player，证据分辨率为 1920×1080、D3D11。
- 建筑、码头、船、植被和水面已经能被渲染，但空间层级仍不可靠。
- 直接可见的最高优先级问题是：R03 根节点高度与 D6 路线不一致、旧宽地面碰撞造成不可见阻挡、泡沫过厚过亮、水生生物未沉入水面、初始镜头过近。

## 证据

- `game/tools/results/D31/D31-P04/visual_evidence_R57/`
- `game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R57.json`
- `game/tools/results/D31/D31-P04/D31_P04_REVALIDATION_RESULT.json`

## 下一步

执行 `VC-A01`，将 R03 根节点和场景 Prefab override 归一到世界 Y=0，并保留运行时 guard；随后执行 `VC-A02` 让地形、码头和跳板碰撞跟随真实 R03 几何。
