# D17 垂直切片环境与船舶资源登记（D17_SLICE_ENV_ASSETS）

## 1. 码头主平台 (building_saint_marys_dock)
- **网格**: `game/Assets/Art/SliceFinal/D16C08/saint_marys_dock_final.fbx`
- **Prefab**: `game/Assets/Prefabs/SliceFinal/D16C08/saint_marys_dock_final.prefab`
- **碰撞体**: BoxCollider (8m x 1m x 16m)
- **锚点**: `anchor_dock_entry`

## 2. 小艇泊位 (building_small_boat_pier)
- **网格**: `game/Assets/Art/SliceFinal/D16C08/small_boat_pier_final.fbx`
- **Prefab**: `game/Assets/Prefabs/SliceFinal/D16C08/small_boat_pier_final.prefab`
- **碰撞体**: BoxCollider (3m x 0.6m x 6m)
- **锚点**: `anchor_pier_board`

## 3. 沙滩与岛屿地面 (island_saint_marys)
- **网格**: `game/Assets/Art/SliceFinal/D16C08/saint_marys_sand_final.fbx`
- **Prefab**: `game/Assets/Prefabs/SliceFinal/D16C08/saint_marys_sand_final.prefab`
- **碰撞体**: BoxCollider (20m x 0.2m x 20m)
- **锚点**: `anchor_beach_ground`

## 4. 船舶展示与操船实体
- **切片船只**: 小型风帆工作船 (挂载船舵、桅杆与上下船触发器)，物理与操船状态由 D4 ShipSystem / D2 控制器驱动。
