# D18-A02 Kenney 导入审计

- task: `D18-A02`
- status: **PASS**
- mode: `report_only`
- read-only: `True`
- Registry changed: `False`
- asset_state changed: `False`
- ReleaseApproved asserted: `False`

## 统计

| 指标 | 数量 |
|---|---:|
| manifest | 2 |
| candidate files | 151 |
| models | 151 |
| textures | 0 |
| audio | 0 |
| prefabs | 0 |
| Mesh | 231 |
| animation clips | 27 |
| materials | 161 |
| errors | 0 |
| warnings | 151 |

## 分类

- `Buildings`: 100
- `Environment`: 9
- `Props`: 18
- `Ships`: 15
- `Vegetation`: 9

## manifest

### pack_kenney_building_kit_ships_port_props_v1
- file: `docs/game/design/common/assets/packs/ships_port_props/kenney_building_kit_ships_port_props.manifest.json`
- review_status: `Candidate`
- import_status: `ImportVerified`
- declared/scanned: 79/79
### pack_kenney_pirate_kit_ships_port_props_v1
- file: `docs/game/design/common/assets/packs/ships_port_props/kenney_pirate_kit_ships_port_props.manifest.json`
- review_status: `Candidate`
- import_status: `ImportVerified`
- declared/scanned: 72/72

## 当前候选问题与导入建议

### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/barricade-doorway-a.fbx`
- category: `Buildings`; target: `d18a05_building_barricade_doorway_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.46` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/barricade-doorway-b.fbx`
- category: `Buildings`; target: `d18a05_building_barricade_doorway_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.583` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/barricade-doorway-c.fbx`
- category: `Buildings`; target: `d18a05_building_barricade_doorway_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.747` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/barricade-window-a.fbx`
- category: `Buildings`; target: `d18a05_building_barricade_window_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.256` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/barricade-window-b.fbx`
- category: `Buildings`; target: `d18a05_building_barricade_window_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.256` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/barricade-window-c.fbx`
- category: `Buildings`; target: `d18a05_building_barricade_window_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.356` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/border-corner-diagonal.fbx`
- category: `Buildings`; target: `d18a05_building_border_corner_diagonal`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/border-corner-round.fbx`
- category: `Buildings`; target: `d18a05_building_border_corner_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/border-corner-small.fbx`
- category: `Buildings`; target: `d18a05_building_border_corner_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/border-corner.fbx`
- category: `Buildings`; target: `d18a05_building_border_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/border-high-corner-diagonal.fbx`
- category: `Buildings`; target: `d18a05_building_border_high_corner_diagonal`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/border-high-corner-round.fbx`
- category: `Buildings`; target: `d18a05_building_border_high_corner_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/border-high-corner-small.fbx`
- category: `Buildings`; target: `d18a05_building_border_high_corner_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/border-high-corner.fbx`
- category: `Buildings`; target: `d18a05_building_border_high_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/border-high.fbx`
- category: `Buildings`; target: `d18a05_building_border_high`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/border.fbx`
- category: `Buildings`; target: `d18a05_building_border`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/column-thin.fbx`
- category: `Buildings`; target: `d18a05_building_column_thin`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/column-wide.fbx`
- category: `Buildings`; target: `d18a05_building_column_wide`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/column.fbx`
- category: `Buildings`; target: `d18a05_building_column`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/detail-pipe.fbx`
- category: `Buildings`; target: `d18a05_building_detail_pipe`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.434` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/door-rotate-round-a.fbx`
- category: `Buildings`; target: `d18a05_building_door_rotate_round_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/3/1
- model_dimension: `2.125` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/door-rotate-round-b.fbx`
- category: `Buildings`; target: `d18a05_building_door_rotate_round_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/3/1
- model_dimension: `2.125` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/door-rotate-round-c.fbx`
- category: `Buildings`; target: `d18a05_building_door_rotate_round_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/3/1
- model_dimension: `2.125` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/door-rotate-round-d.fbx`
- category: `Buildings`; target: `d18a05_building_door_rotate_round_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 3/3/2
- model_dimension: `2.125` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/door-rotate-square-a.fbx`
- category: `Buildings`; target: `d18a05_building_door_rotate_square_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/3/1
- model_dimension: `2.125` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/door-rotate-square-b.fbx`
- category: `Buildings`; target: `d18a05_building_door_rotate_square_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/3/1
- model_dimension: `2.125` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/door-rotate-square-c.fbx`
- category: `Buildings`; target: `d18a05_building_door_rotate_square_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/3/1
- model_dimension: `2.125` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/door-rotate-square-d.fbx`
- category: `Buildings`; target: `d18a05_building_door_rotate_square_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 3/3/2
- model_dimension: `2.125` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/floor-corner-diagonal.fbx`
- category: `Buildings`; target: `d18a05_building_floor_corner_diagonal`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/floor-corner-round.fbx`
- category: `Buildings`; target: `d18a05_building_floor_corner_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/floor-half.fbx`
- category: `Buildings`; target: `d18a05_building_floor_half`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/floor-quarter.fbx`
- category: `Buildings`; target: `d18a05_building_floor_quarter`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/floor.fbx`
- category: `Buildings`; target: `d18a05_building_floor`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/gutter-vertical-bottom.fbx`
- category: `Buildings`; target: `d18a05_building_gutter_vertical_bottom`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.3` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/gutter-vertical-short.fbx`
- category: `Buildings`; target: `d18a05_building_gutter_vertical_short`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/gutter-vertical-top.fbx`
- category: `Buildings`; target: `d18a05_building_gutter_vertical_top`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.3` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/gutter-vertical-wall.fbx`
- category: `Buildings`; target: `d18a05_building_gutter_vertical_wall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/gutter-vertical.fbx`
- category: `Buildings`; target: `d18a05_building_gutter_vertical`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/plating-detailed-wide.fbx`
- category: `Buildings`; target: `d18a05_building_plating_detailed_wide`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/plating-detailed.fbx`
- category: `Buildings`; target: `d18a05_building_plating_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/plating-wide.fbx`
- category: `Buildings`; target: `d18a05_building_plating_wide`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/plating.fbx`
- category: `Buildings`; target: `d18a05_building_plating`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/roof-flat-center.fbx`
- category: `Buildings`; target: `d18a05_building_roof_flat_center`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/roof-flat-corner-inner.fbx`
- category: `Buildings`; target: `d18a05_building_roof_flat_corner_inner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/roof-flat-corner.fbx`
- category: `Buildings`; target: `d18a05_building_roof_flat_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/roof-flat-patch-large.fbx`
- category: `Buildings`; target: `d18a05_building_roof_flat_patch_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.6` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/roof-flat-patch.fbx`
- category: `Buildings`; target: `d18a05_building_roof_flat_patch`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.1` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/roof-flat-side.fbx`
- category: `Buildings`; target: `d18a05_building_roof_flat_side`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/roof-flat-square.fbx`
- category: `Buildings`; target: `d18a05_building_roof_flat_square`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.395` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/stairs-center-short.fbx`
- category: `Buildings`; target: `d18a05_building_stairs_center_short`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/stairs-center.fbx`
- category: `Buildings`; target: `d18a05_building_stairs_center`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/stairs-closed-short.fbx`
- category: `Buildings`; target: `d18a05_building_stairs_closed_short`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/stairs-closed.fbx`
- category: `Buildings`; target: `d18a05_building_stairs_closed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/stairs-open-short.fbx`
- category: `Buildings`; target: `d18a05_building_stairs_open_short`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/stairs-open.fbx`
- category: `Buildings`; target: `d18a05_building_stairs_open`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/stairs-sides-short.fbx`
- category: `Buildings`; target: `d18a05_building_stairs_sides_short`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 3/0/1
- model_dimension: `4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/stairs-sides.fbx`
- category: `Buildings`; target: `d18a05_building_stairs_sides`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 3/0/1
- model_dimension: `6` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-corner-column-bottom.fbx`
- category: `Buildings`; target: `d18a05_building_wall_corner_column_bottom`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-corner-column-small-bottom.fbx`
- category: `Buildings`; target: `d18a05_building_wall_corner_column_small_bottom`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-corner-column-small.fbx`
- category: `Buildings`; target: `d18a05_building_wall_corner_column_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-corner-column.fbx`
- category: `Buildings`; target: `d18a05_building_wall_corner_column`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-corner-diagonal.fbx`
- category: `Buildings`; target: `d18a05_building_wall_corner_diagonal`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-corner-round.fbx`
- category: `Buildings`; target: `d18a05_building_wall_corner_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-corner.fbx`
- category: `Buildings`; target: `d18a05_building_wall_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-doorway-round.fbx`
- category: `Buildings`; target: `d18a05_building_wall_doorway_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-doorway-square.fbx`
- category: `Buildings`; target: `d18a05_building_wall_doorway_square`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-doorway-wide-round.fbx`
- category: `Buildings`; target: `d18a05_building_wall_doorway_wide_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-doorway-wide-square.fbx`
- category: `Buildings`; target: `d18a05_building_wall_doorway_wide_square`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-half.fbx`
- category: `Buildings`; target: `d18a05_building_wall_half`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-low.fbx`
- category: `Buildings`; target: `d18a05_building_wall_low`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-window-round-detailed.fbx`
- category: `Buildings`; target: `d18a05_building_wall_window_round_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/2
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-window-round.fbx`
- category: `Buildings`; target: `d18a05_building_wall_window_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/2
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-window-square-detailed.fbx`
- category: `Buildings`; target: `d18a05_building_wall_window_square_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/2
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-window-square.fbx`
- category: `Buildings`; target: `d18a05_building_wall_window_square`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/2
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-window-wide-round-detailed.fbx`
- category: `Buildings`; target: `d18a05_building_wall_window_wide_round_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/2
- model_dimension: `4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-window-wide-round.fbx`
- category: `Buildings`; target: `d18a05_building_wall_window_wide_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/2
- model_dimension: `4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-window-wide-square-detailed.fbx`
- category: `Buildings`; target: `d18a05_building_wall_window_wide_square_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/2
- model_dimension: `4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall-window-wide-square.fbx`
- category: `Buildings`; target: `d18a05_building_wall_window_wide_square`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/2
- model_dimension: `4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyBuildingKit/Models/wall.fbx`
- category: `Buildings`; target: `d18a05_building_wall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/barrel.fbx`
- category: `Props`; target: `d18a05_pirate_barrel`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.336` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/boat-row-large.fbx`
- category: `Ships`; target: `d18a05_pirate_boat_row_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/1
- model_dimension: `2.852` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/boat-row-small.fbx`
- category: `Ships`; target: `d18a05_pirate_boat_row_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/1
- model_dimension: `2.749` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/bottle-large.fbx`
- category: `Props`; target: `d18a05_pirate_bottle_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.89` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/bottle.fbx`
- category: `Props`; target: `d18a05_pirate_bottle`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.89` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/cannon-ball.fbx`
- category: `Ships`; target: `d18a05_pirate_cannon_ball`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.67` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/cannon-mobile.fbx`
- category: `Ships`; target: `d18a05_pirate_cannon_mobile`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 3/0/1
- model_dimension: `2.394` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/cannon.fbx`
- category: `Ships`; target: `d18a05_pirate_cannon`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 3/0/1
- model_dimension: `2.394` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/castle-door.fbx`
- category: `Buildings`; target: `d18a05_pirate_castle_door`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/castle-gate.fbx`
- category: `Buildings`; target: `d18a05_pirate_castle_gate`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/castle-wall.fbx`
- category: `Buildings`; target: `d18a05_pirate_castle_wall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/castle-window.fbx`
- category: `Buildings`; target: `d18a05_pirate_castle_window`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/chest.fbx`
- category: `Props`; target: `d18a05_pirate_chest`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/3/1
- model_dimension: `1.785` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/crate-bottles.fbx`
- category: `Props`; target: `d18a05_pirate_crate_bottles`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.309` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/crate.fbx`
- category: `Props`; target: `d18a05_pirate_crate`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.309` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/flag-high-pennant.fbx`
- category: `Props`; target: `d18a05_pirate_flag_high_pennant`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.597` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/flag-high.fbx`
- category: `Props`; target: `d18a05_pirate_flag_high`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.597` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/flag-pennant.fbx`
- category: `Props`; target: `d18a05_pirate_flag_pennant`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.097` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/flag-pirate-high-pennant.fbx`
- category: `Props`; target: `d18a05_pirate_flag_pirate_high_pennant`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.597` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/flag-pirate-high.fbx`
- category: `Props`; target: `d18a05_pirate_flag_pirate_high`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.597` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/flag-pirate-pennant.fbx`
- category: `Props`; target: `d18a05_pirate_flag_pirate_pennant`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.097` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/flag-pirate.fbx`
- category: `Props`; target: `d18a05_pirate_flag_pirate`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.097` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/flag.fbx`
- category: `Props`; target: `d18a05_pirate_flag`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.097` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/grass-patch.fbx`
- category: `Vegetation`; target: `d18a05_pirate_grass_patch`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.313` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/grass-plant.fbx`
- category: `Vegetation`; target: `d18a05_pirate_grass_plant`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.237` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/grass.fbx`
- category: `Vegetation`; target: `d18a05_pirate_grass`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.544` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/hole.fbx`
- category: `Environment`; target: `d18a05_pirate_hole`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.861` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/mast-ropes.fbx`
- category: `Ships`; target: `d18a05_pirate_mast_ropes`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 3/0/1
- model_dimension: `7.888` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/mast.fbx`
- category: `Ships`; target: `d18a05_pirate_mast`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 3/0/1
- model_dimension: `7.864` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/palm-bend.fbx`
- category: `Vegetation`; target: `d18a05_pirate_palm_bend`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.251` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/palm-detailed-bend.fbx`
- category: `Vegetation`; target: `d18a05_pirate_palm_detailed_bend`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.251` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/palm-detailed-straight.fbx`
- category: `Vegetation`; target: `d18a05_pirate_palm_detailed_straight`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.213` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/palm-straight.fbx`
- category: `Vegetation`; target: `d18a05_pirate_palm_straight`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.213` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/patch-grass-foliage.fbx`
- category: `Vegetation`; target: `d18a05_pirate_patch_grass_foliage`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `5.273` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/patch-grass.fbx`
- category: `Vegetation`; target: `d18a05_pirate_patch_grass`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `5.273` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/patch-sand-foliage.fbx`
- category: `Environment`; target: `d18a05_pirate_patch_sand_foliage`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `7.736` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/patch-sand.fbx`
- category: `Environment`; target: `d18a05_pirate_patch_sand`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `7.736` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/platform-planks.fbx`
- category: `Props`; target: `d18a05_pirate_platform_planks`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.702` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/platform.fbx`
- category: `Props`; target: `d18a05_pirate_platform`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.513` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/rocks-a.fbx`
- category: `Environment`; target: `d18a05_pirate_rocks_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `5.113` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/rocks-b.fbx`
- category: `Environment`; target: `d18a05_pirate_rocks_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.705` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/rocks-c.fbx`
- category: `Environment`; target: `d18a05_pirate_rocks_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.669` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/rocks-sand-a.fbx`
- category: `Environment`; target: `d18a05_pirate_rocks_sand_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `5.113` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/rocks-sand-b.fbx`
- category: `Environment`; target: `d18a05_pirate_rocks_sand_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.71` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/rocks-sand-c.fbx`
- category: `Environment`; target: `d18a05_pirate_rocks_sand_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.722` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/ship-ghost.fbx`
- category: `Ships`; target: `d18a05_pirate_ship_ghost`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 6/0/1
- model_dimension: `10.6` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/ship-large.fbx`
- category: `Ships`; target: `d18a05_pirate_ship_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 6/0/1
- model_dimension: `13.1` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/ship-medium.fbx`
- category: `Ships`; target: `d18a05_pirate_ship_medium`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 6/0/1
- model_dimension: `10.6` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/ship-pirate-large.fbx`
- category: `Ships`; target: `d18a05_pirate_ship_pirate_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 6/0/1
- model_dimension: `13.1` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/ship-pirate-medium.fbx`
- category: `Ships`; target: `d18a05_pirate_ship_pirate_medium`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 6/0/1
- model_dimension: `10.6` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/ship-pirate-small.fbx`
- category: `Ships`; target: `d18a05_pirate_ship_pirate_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 4/0/1
- model_dimension: `9.964` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/ship-small.fbx`
- category: `Ships`; target: `d18a05_pirate_ship_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 4/0/1
- model_dimension: `9.964` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/ship-wreck.fbx`
- category: `Ships`; target: `d18a05_pirate_ship_wreck`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 16/0/1
- model_dimension: `10.749` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/structure-fence-sides.fbx`
- category: `Buildings`; target: `d18a05_pirate_structure_fence_sides`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/1
- model_dimension: `3.45` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/structure-fence.fbx`
- category: `Buildings`; target: `d18a05_pirate_structure_fence`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.5` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/structure-platform-dock-small.fbx`
- category: `Buildings`; target: `d18a05_pirate_structure_platform_dock_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.512` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/structure-platform-dock.fbx`
- category: `Buildings`; target: `d18a05_pirate_structure_platform_dock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.513` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/structure-platform-small.fbx`
- category: `Buildings`; target: `d18a05_pirate_structure_platform_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.512` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/structure-platform.fbx`
- category: `Buildings`; target: `d18a05_pirate_structure_platform`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.513` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/structure-roof.fbx`
- category: `Buildings`; target: `d18a05_pirate_structure_roof`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.4` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/structure.fbx`
- category: `Buildings`; target: `d18a05_pirate_structure`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.5` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tool-paddle.fbx`
- category: `Props`; target: `d18a05_pirate_tool_paddle`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.349` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tool-shovel.fbx`
- category: `Props`; target: `d18a05_pirate_tool_shovel`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.349` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tower-base-door.fbx`
- category: `Buildings`; target: `d18a05_pirate_tower_base_door`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.56` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tower-base.fbx`
- category: `Buildings`; target: `d18a05_pirate_tower_base`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.16` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tower-complete-large.fbx`
- category: `Buildings`; target: `d18a05_pirate_tower_complete_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `10.225` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tower-complete-small.fbx`
- category: `Buildings`; target: `d18a05_pirate_tower_complete_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `6.785` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tower-middle-windows.fbx`
- category: `Buildings`; target: `d18a05_pirate_tower_middle_windows`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.072` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tower-middle.fbx`
- category: `Buildings`; target: `d18a05_pirate_tower_middle`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.053` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tower-roof.fbx`
- category: `Buildings`; target: `d18a05_pirate_tower_roof`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.628` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tower-top.fbx`
- category: `Buildings`; target: `d18a05_pirate_tower_top`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.354` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tower-watch.fbx`
- category: `Buildings`; target: `d18a05_pirate_tower_watch`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `3.049` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional

## 边界

本报告只记录 Unity 当前导入结果和建议；不修改 Registry、asset_state、Prefab、场景或候选包，不写 ReleaseApproved，不下载资源。模型自身尺寸以记录为准，不由管线统一 scale；后续 Prefab/责任系统决定具体目标尺度、锚点和碰撞修正。
