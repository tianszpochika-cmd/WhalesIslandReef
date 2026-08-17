# D16-A05 垂直切片资源包（D16-C08 方案 B）

- 状态: `SLICE_FINAL_LOCAL / 4_ITEMS`
- 日期: 2026-08-18
- 目标: 先让垂直切片能跑，不追求 2264 placeholder 全换完

| 条目 | 最终网格 | 运行时 Prefab | 碰撞 | 锚点 |
|---|---|---|---|---|
| `asset_ph_crew_male` | `game/Assets/Art/SliceFinal/D16C08/crew_male_final.fbx` | `game/Assets/Prefabs/SliceFinal/D16C08/crew_male_final.prefab` | 根 CapsuleCollider | `anchor_crew_foot` |
| `building_saint_marys_dock` | `game/Assets/Art/SliceFinal/D16C08/saint_marys_dock_final.fbx` | `game/Assets/Prefabs/SliceFinal/D16C08/saint_marys_dock_final.prefab` | 根 BoxCollider | `anchor_dock_entry` |
| `building_small_boat_pier` | `game/Assets/Art/SliceFinal/D16C08/small_boat_pier_final.fbx` | `game/Assets/Prefabs/SliceFinal/D16C08/small_boat_pier_final.prefab` | 根 BoxCollider | `anchor_pier_board` |
| `island_saint_marys` | `game/Assets/Art/SliceFinal/D16C08/saint_marys_sand_final.fbx` | `game/Assets/Prefabs/SliceFinal/D16C08/saint_marys_sand_final.prefab` | 根 BoxCollider | `anchor_beach_ground` |

原始包与 License 保存在 `artifacts/external_sources/d16_c08_slice/`。路径不含 `placeholder` 或 `ph_`。
