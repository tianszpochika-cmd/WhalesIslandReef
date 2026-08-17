# D16-A05 垂直切片接入计划（D16-C08 方案 B）

## 接入清单

只改这 4 条 Content Registry：

1. `asset_ph_crew_male`
2. `building_saint_marys_dock`
3. `building_small_boat_pier`
4. `island_saint_marys`

预检通过后写入最终 `resource_path`/`model_ref`、`runtime_prefab_path`、授权/审查字段，并将这 4 条 `asset_state` 从 `placeholder` 接到最终路径。其余条目一律不动。

## 回滚点

- 回滚基线：除上述 4 条外，全库 placeholder 计数保持 2264-4=2260。
- 回滚动作：恢复这 4 个 JSON 的 placeholder 路径与 `asset_state=placeholder`，不删原始 zip。

## 红线

- 禁止清零 2264 placeholder。
- 禁止把 `Candidates/Kenney` 原路径标 final。
- 禁止用 Kenney Blocky Characters 当最终角色。
- 不改 Systems 数值或 D2/D3/D4/D9 裁决权。
