# D15-A01* 交接报告

任务：`D15-A01*`  
日期：2026-08-11  
状态：**通过**

## 1. 交付物

- 岛屿目录：[D15_ISLAND_BATCH_CATALOG.md](../../D15_ISLAND_BATCH_CATALOG.md)
- 50 条内容定义：`game/Assets/Resources/ContentRegistry/entries/island/`
- 注册表：`game/Assets/Resources/ContentRegistry/registry.json`
- 内容检查器：`tools/d15_a01_island_check.py`
- 测试：`tools/tests/test_d15_a01_island_check.py`
- 记忆日志：`.workbuddy/memory/2026-08-11.md`

## 2. 完成内容

- 50 个唯一 `island_` ID，8 类主地貌配额：火山 8、喀斯特 10、珊瑚沙洲 8、红树林 4、冲积河口 2、湿润山地雨林 8、背风干旱灌丛 5、环礁 5。
- 每座岛具有 D8 目录字段、来源/授权、历史标签、`IslandSystem` owner、唯一测试 ID、三层自然区域、淡水、洞穴、暗礁、生境、资源、建筑入口、任务入口、登陆路线、危险、流式分区、存档键和恢复规则。
- 所有资源只引用已注册的 `plant_`/`animal_`/`fish_` 内容；建筑和任务只保存入口/投影，不写生态量、建筑状态、库存、经济、任务或奖励结果。
- 模型和材质使用 `asset_ph_palm_leaves` 可替换占位，所有条目标明 `asset_state=placeholder`；没有伪造最终美术完成。
- 注册表根节点加入 50 条岛屿，并补回圣玛丽岛原有的 `plant_farm_indigo_mature`、`item_dried_mackerel`、`item_fish_oil` 引用；另外将已有 D15 道具/服装占位资源登记为根节点，保持全图可达。

## 3. 流式/保存/恢复边界

每座岛固定三个 cell：`coast`、`inland`、`upper`。目录声明近/中/远景加载策略、预加载主登陆点/任务入口、离开前保存和释放高成本对象。`save_contract` 使用 `location_json.island.<id>` 和稳定岛屿 ID，恢复字段包含发现/访问、资源节点、建筑投影、任务入口投影和 cell 状态。

本任务只提供内容契约和准入测试，没有在 `Systems/` 加入新的 Island/Save/Ecology/Building/Quest 裁决逻辑，也没有网络或 LLM。

## 4. 验证结果

```text
python tools/d15_a01_island_check.py game/Assets/Resources/ContentRegistry
  PASS: islands=50, templates=8, stream_save_restore_sample=10

python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
  PASS: 174 entries / 13 prefixes

python -m unittest discover -s tools/tests -p 'test_d15_a01_island_check.py' -v
  PASS: 1/1
```

抽样为确定性的 10/50（20%），验证内容目录层面的 stream → save → restore，不生成运行时事实。Unity 场景级加载峰值、碰撞和最终美术质量仍由 D15-S02/D16 按冻结 LOD 和性能预算复测。

## 5. 交给后续任务

1. D11/Ecology 接管资源刷新、耗尽、恢复、生境和采集裁决，不复制 `natural_archive.resources` 为第二份状态。
2. D13/D12/ShipSystem 分别接管建筑、任务、产权、航海、吃水、暗礁和登陆风险；目录入口只提供引用和可达性。
3. 真实模型、材质和音频进入 `asset_` 前补齐来源、许可证、商业使用、署名、修改权限和证据。
4. D15-S02 按冻结表抽样至少 10 座，检查缺模型、缺材质、缺碰撞、缺 LOD、错误生境、错误历史年代、授权和孤儿引用。

## 6. 结论

**D15-A01* 通过，允许 D15-A02*—A08* 使用这 50 座岛的稳定 ID、自然档案和只读入口。** 后续系统只能通过 owner 和统一事件/存档接管运行时状态。
