# D18-A02 测试结果

日期：2026-08-14  
结论：**PASS**

## 已有真实审计结果

- Kenney manifest：2；候选文件：7；模型：7；Mesh：15；动画 clip：27；材质：7；错误：0；警告：7（非阻塞，原始文件名命名建议）。
- JSON/Markdown 报告：`D18_A02_KENNEY_IMPORT_AUDIT.json`、`D18_A02_KENNEY_IMPORT_AUDIT.md`。
- 报告模式：`report_only`；Registry、`asset_state`、下载和 `ReleaseApproved` 均为 false。

## EditMode/批处理

已加入测试覆盖：

1. 九类分类规则；
2. `Library`/`Temp`/`Logs`、绝对外部路径和 `..` 路径逃逸拒绝；
3. 自动修复必须显式选择且只能在测试夹具；
4. Prefab renderer/material/bounds/collider/anchor/LOD/missing script/missing reference/name 预检；
5. 报告 JSON 序列化确定性；
6. 候选 manifest 审计重复运行结果确定性。

本次已在项目锁释放后真实执行 Unity 批处理，审计退出码 `0`，报告为 `PASS`；EditMode XML 为 `6/6 passed, 0 failed`，退出码 `0`。Unity Hub 保留不影响批处理；当前结果和日志见 `game/tools/results/D18/D18-A02/`。
