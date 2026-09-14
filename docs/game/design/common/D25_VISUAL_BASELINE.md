# D25 视觉基线与债务登记

> 任务：`D25-S01`  
> 状态：`PASS`（基线冻结完成；尚不代表画面已完成）  
> 日期：2026-09-02

## 当前基线

| 项目 | 当前事实 |
|---|---|
| Unity | `6000.0.80f1` |
| 渲染管线 | Universal Render Pipeline `17.0.4` |
| 正式入口 | `MainMenu → Bootstrap → InitialHarbor` |
| InitialHarbor SHA-256 | `5BDBC715033C62AA50332FBA5FB4D1133CC25DE9FF19BF521B02E3AC7F75EFC0` |
| Bootstrap SHA-256 | `5E7C881049C8942C45B5E3A1D65F6A4118AC82B399FABFA0653765E88A3416B1` |
| 当前 Git 基线 | `01024cae2823c26b2595a660e6dde5879870dfc8` |
| 当前 Unity 编译观察 | 最新 Editor.log 出现 `Tundra build success`；历史错误行保留在日志中，后续必须以新的干净构建段复核 |
| 当前 Player 观察 | 可进入 `InitialHarbor`；这只是启动冒烟，不是完整 Runtime 验收 |

## 已有可复用表现层

- `D19_R03_HarborPresentation`：已有港口地形、建筑、码头、植被和船体表现，但历史验收为 `PARTIAL`，不能直接当作最终画面；
- `D19/Water`：已有水体、泡沫、浅水、礁石和多档材质；
- `D19/Vegetation`：已有生境 Prefab、风响应材质和布局数据；
- `D20/PlayerVisuals`、`D20/NPC`、`D20/Wildlife`：已有工作 Prefab，但部分仍是 Candidate/工作层，必须通过 D25 资源验证和 D27 运行时集成；
- `D22` 读模型和 UI 命令：已有纯逻辑入口，正式 Canvas 仍需 D29 唯一集成者接线。

## 第一批视觉债务

1. 旧 D5/D7/D9/D10/D16 Canvas 可能在不同启动顺序创建，造成 HUD 叠加或射线拦截；
2. 玩家模式仍可能显示调试路线、FPS、内部状态和过长文本；
3. R03 仍有平坦地表、远景块状建筑、重复旧 Renderer 和局部遮挡风险；
4. D2 接地恢复链与 R03 地表曾出现不同 surface 选择，场景集成必须保留稳定 owner 并记录实际接地；
5. 角色和动物已有工作 Prefab，但正式运行时消费者、Animator、服装/发型数量和水生行为表现尚未统一；
6. 天气、海水、船体、UI 按钮和保存重载目前没有新的 D25—D31 运行时闭环证据。

## 不可变边界

- D18—D24 逻辑系统、D20/D21/D22 owner、AgentCore、contracts 和 SaveSystem 不由 D25 表现任务改写；
- 表现层只读取公开快照/事件，不裁决交易、关系、库存、天气、航行、动物或存档事实；
- `Candidate`/`Pending` 资源保持原状态，不能因为进入工作 Prefab 自动升级为发布资源；
- D25 之后的所有正式场景改动必须由 `S` 任务串行完成，并保留回滚快照。

