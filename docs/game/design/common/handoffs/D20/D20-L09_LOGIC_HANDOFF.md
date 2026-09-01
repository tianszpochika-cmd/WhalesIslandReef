# D20-L09 逻辑交接说明

- 任务：`D20-L09`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L09/D20_L09_LOGIC_RESULT.json`

## 已完成

新增 `game/Assets/Scripts/Systems/D20/D20AppearanceSelectionRules.cs`：

- 通过 D10 `D10AppearanceCatalog`、`D10AppearanceItemDefinition`、`D10AppearanceSnapshot` 和 D10 上下文建立防御性输入；
- 校验角色 ID、outfit `content_id`、稳定 `resource_key`、槽位、模式、定义资产键、文化/职业/天气上下文；
- 装备模式检查 D10 owner 的拥有事实，伤病槽和伤病 overlay 拒绝普通选择，互斥组合返回稳定冲突码；
- 返回 `RequiresAppearanceOwner=true` 和只读 `ProjectedLoadout`，不直接装备、不改变耐久/所有权、不检查二进制资源存在性；
- `ValidateCatalog()` 对公开代表目录执行重复和稳定键检查，placeholder 仅作为声明字段透出。

## 公开 API

- `D20AppearanceSelectionRequest.Create(...)`、`Clone()`；
- `D20AppearanceSelectionRules.Validate(...)`、`Evaluate(...)`、`ValidateCatalog()`；
- `D20AppearanceSelectionRules.IsStableContentId(...)`、`IsStableResourceKey(...)`；
- `D20AppearanceSelectionResult`、`D20AppearanceSelectionCode`、`D20AppearanceSelectionMode`。

## 验证与未完成项

- 精确 C# harness 编译：通过，0 warnings / 0 errors；
- 确定性 smoke：通过，覆盖合法预览、placeholder、非法资源键和目录校验；
- 五个目标生产文件禁用引用扫描：通过；`git diff --check`：通过；机器结果 JSON：通过解析；
- 未加载资源或启动 Runtime，真实装备写入仍必须交给 D10 owner。

## Coordinator memory entry

请协调器在本批次汇合后统一写入当天日志：`D20-L09 LOGIC_PASS`；新增 D20 外观选择只读规则，消费 D10 公共目录/快照和 D18 稳定内容键，覆盖上下文、所有权、伤病保护、槽位互斥及 placeholder 声明，所有通过结果仍需 D10 owner 写入；C# 编译、smoke、禁用引用扫描、JSON 解析和 diff check 通过；Runtime/资源加载延后。
