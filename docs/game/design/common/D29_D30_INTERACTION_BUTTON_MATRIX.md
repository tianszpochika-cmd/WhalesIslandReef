# D29—D30 交互按钮矩阵

这份矩阵把“有一个按钮”与“按钮接到了哪个 owner、反馈在哪里、关闭后如何恢复”分开记录。机器可读主表是 [D29_D30_INTERACTION_BUTTON_MATRIX.json](D29_D30_INTERACTION_BUTTON_MATRIX.json)，审计工具会逐行读取源文件并拒绝缺少回调或反馈的条目。

## 范围

- D29 主 HUD：市场、仓库、酒馆、船坞、登船、天气、保存、整备和仓库关闭。
- D4 内容页：势力、炮战、装填、开炮、船体改装、动物观察、医疗、主线和内容页关闭。
- D9 整备页：泊位、盐、木材、酒馆、补给、修船、招募、工资、船坞、离港和整备页关闭。
- D4 另有 1/2/3 选弹、接舷、脱离和保存等键盘专用入口；D29 的 `Q` 画质预览也没有独立按钮，统一列在 JSON 的 `keyboard_only_controls`。

## 判定规则

1. 每个条目必须有现存源文件和可定位的回调方法。
2. 每个按钮必须有可见反馈或明确的只读刷新路径；owner 未就绪时也必须显示失败原因，不能静默。
3. 业务事实只能由已有 D2/D3/D4/D9/D10/D28 owner 裁决，D29/D30 表现层不能复制公式或写入事实。
4. 关闭按钮和 Escape/快捷键必须恢复主 HUD，不能留下两个模态页面同时可交互。
5. D29 主 HUD、仓库、D4、D9、D10 使用 `D16ModalFocusScope`；页面隐藏或不可交互时必须清理自己持有的 EventSystem 选择，不能让隐藏页面继续抢输入。
6. D29 主 HUD 会根据 owner 缺失/未就绪状态更新按钮的视觉状态，但入口保持 `Button.interactable=true`，以便点击事件进入统一的 owner 等待/拒绝反馈；状态栏仍列出未就绪入口，键盘兼容入口也必须返回可见失败原因。`interactable` 不是业务权限，最终裁决仍由 owner 完成。
7. 市场、酒馆、船坞、登船、整备、保存等一次性请求使用短窗口防重；天气和画质预览切换是明确可重复操作，不得被一次性防重误伤。
8. D30 收到失败事件或短窗口重复事件时必须保持当前阶段，显示“状态未改变”，不得把失败/重复误报为成功阶段。
9. 静态矩阵通过不等于 Player 运行通过；逐按钮运行状态仍须在 D29-G06/D30-G06 结果中提供当前构建证据。

## 执行命令

```text
python tools/d29_d30_interaction_matrix_audit.py
python -m unittest -v tools.tests.test_d29_d30_interaction_matrix
```

机器结果：`game/tools/results/D29/D29-S05/D29_D30_INTERACTION_MATRIX_AUDIT.json`。

R19 代码级焦点审计：`game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R19.json`。
R30/R31/R32 代码级补充审计：`game/tools/results/D31/D31-S05/` 下对应的 `D31_S05_INTERACTION_LOGIC_AUDIT_Rxx.json`。
这些结果证明 owner 结果传播、不可用状态、重复保护和失败阶段投影的源码边界，不替代成功/失败/禁用/重复点击的 Player 运行证据。
