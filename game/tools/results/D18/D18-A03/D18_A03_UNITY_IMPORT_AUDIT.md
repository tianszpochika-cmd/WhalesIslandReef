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
| manifest | 9 |
| candidate files | 337 |
| models | 329 |
| textures | 8 |
| audio | 0 |
| prefabs | 0 |
| Mesh | 338 |
| animation clips | 0 |
| materials | 688 |
| errors | 0 |
| warnings | 235 |

## 分类

- `Environment`: 245
- `Vegetation`: 92

## manifest

### pack_kenney_nature_kit_d18_a03_v1
- file: `docs/game/design/common/assets/packs/environment/kenney_nature_kit_d18_a03.manifest.json`
- review_status: `Candidate`
- import_status: `NotImported`
- declared/scanned: 329/329
- warning `import_status_not_verified`: A01 import_status is not ImportVerified
### pack_polyhaven_coral_mud_01_ao_jpg_d18_a03_v1
- file: `docs/game/design/common/assets/packs/environment/polyhaven_coral_mud_01_ao_1k_jpg_d18_a03.manifest.json`
- review_status: `Candidate`
- import_status: `NotImported`
- declared/scanned: 1/1
- warning `import_status_not_verified`: A01 import_status is not ImportVerified
### pack_polyhaven_coral_mud_01_diff_jpg_d18_a03_v1
- file: `docs/game/design/common/assets/packs/environment/polyhaven_coral_mud_01_diff_1k_jpg_d18_a03.manifest.json`
- review_status: `Candidate`
- import_status: `NotImported`
- declared/scanned: 1/1
- warning `import_status_not_verified`: A01 import_status is not ImportVerified
### pack_polyhaven_coral_mud_01_nor_gl_jpg_d18_a03_v1
- file: `docs/game/design/common/assets/packs/environment/polyhaven_coral_mud_01_nor_gl_1k_jpg_d18_a03.manifest.json`
- review_status: `Candidate`
- import_status: `NotImported`
- declared/scanned: 1/1
- warning `import_status_not_verified`: A01 import_status is not ImportVerified
### pack_polyhaven_coral_mud_01_rough_jpg_d18_a03_v1
- file: `docs/game/design/common/assets/packs/environment/polyhaven_coral_mud_01_rough_1k_jpg_d18_a03.manifest.json`
- review_status: `Candidate`
- import_status: `NotImported`
- declared/scanned: 1/1
- warning `import_status_not_verified`: A01 import_status is not ImportVerified
### pack_polyhaven_sand_01_ao_jpg_d18_a03_v1
- file: `docs/game/design/common/assets/packs/environment/polyhaven_sand_01_ao_1k_jpg_d18_a03.manifest.json`
- review_status: `Candidate`
- import_status: `NotImported`
- declared/scanned: 1/1
- warning `import_status_not_verified`: A01 import_status is not ImportVerified
### pack_polyhaven_sand_01_diff_jpg_d18_a03_v1
- file: `docs/game/design/common/assets/packs/environment/polyhaven_sand_01_diff_1k_jpg_d18_a03.manifest.json`
- review_status: `Candidate`
- import_status: `NotImported`
- declared/scanned: 1/1
- warning `import_status_not_verified`: A01 import_status is not ImportVerified
### pack_polyhaven_sand_01_nor_gl_jpg_d18_a03_v1
- file: `docs/game/design/common/assets/packs/environment/polyhaven_sand_01_nor_gl_1k_jpg_d18_a03.manifest.json`
- review_status: `Candidate`
- import_status: `NotImported`
- declared/scanned: 1/1
- warning `import_status_not_verified`: A01 import_status is not ImportVerified
### pack_polyhaven_sand_01_rough_jpg_d18_a03_v1
- file: `docs/game/design/common/assets/packs/environment/polyhaven_sand_01_rough_1k_jpg_d18_a03.manifest.json`
- review_status: `Candidate`
- import_status: `NotImported`
- declared/scanned: 1/1
- warning `import_status_not_verified`: A01 import_status is not ImportVerified

## 当前候选问题与导入建议

### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bed.fbx`
- category: `Environment`; target: `env_kenney_nature_bed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.747` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bed_floor.fbx`
- category: `Environment`; target: `env_kenney_nature_bed_floor`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.747` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_center_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_center_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_center_stoneRound.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_center_stone_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_center_wood.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_center_wood`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_center_woodRound.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_center_wood_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_side_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_side_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_side_stoneRound.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_side_stone_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_side_wood.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_side_wood`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_side_woodRound.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_side_wood_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_stoneNarrow.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_stone_narrow`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_stoneRound.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_stone_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_stoneRoundNarrow.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_stone_round_narrow`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_wood.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_wood`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_woodNarrow.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_wood_narrow`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_woodRound.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_wood_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/bridge_woodRoundNarrow.fbx`
- category: `Environment`; target: `env_kenney_nature_bridge_wood_round_narrow`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1.04` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cactus_short.fbx`
- category: `Vegetation`; target: `env_kenney_nature_cactus_short`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.533` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cactus_tall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_cactus_tall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.75` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/campfire_bricks.fbx`
- category: `Environment`; target: `env_kenney_nature_campfire_bricks`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.469` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/campfire_logs.fbx`
- category: `Environment`; target: `env_kenney_nature_campfire_logs`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.292` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/campfire_planks.fbx`
- category: `Environment`; target: `env_kenney_nature_campfire_planks`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.35` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/campfire_stones.fbx`
- category: `Environment`; target: `env_kenney_nature_campfire_stones`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.541` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/canoe.fbx`
- category: `Environment`; target: `env_kenney_nature_canoe`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.15` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/canoe_paddle.fbx`
- category: `Environment`; target: `env_kenney_nature_canoe_paddle`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.59` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockCave_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_cave_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockCave_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_cave_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockDiagonal_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_diagonal_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockDiagonal_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_diagonal_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockHalf_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_half_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockHalf_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_half_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockQuarter_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_quarter_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockQuarter_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_quarter_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockSlopeHalfWalls_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_slope_half_walls_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockSlopeHalfWalls_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_slope_half_walls_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockSlopeWalls_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_slope_walls_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockSlopeWalls_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_slope_walls_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockSlope_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_slope_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.2` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_blockSlope_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_slope_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.2` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_block_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_block_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_block_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cave_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_cave_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cave_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_cave_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cornerInnerLarge_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_inner_large_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cornerInnerLarge_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_inner_large_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cornerInnerTop_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_inner_top_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cornerInnerTop_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_inner_top_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cornerInner_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_inner_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cornerInner_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_inner_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cornerLarge_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_large_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cornerLarge_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_large_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cornerTop_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_top_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_cornerTop_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_top_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_corner_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_corner_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_corner_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_diagonal_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_diagonal_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.169` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_diagonal_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_diagonal_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.169` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_halfCornerInner_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_half_corner_inner_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_halfCornerInner_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_half_corner_inner_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_halfCorner_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_half_corner_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.5` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_halfCorner_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_half_corner_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.5` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_half_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_half_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_half_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_half_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_large_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_large_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_large_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_large_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_stepsCornerInner_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_steps_corner_inner_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_stepsCornerInner_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_steps_corner_inner_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_stepsCorner_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_steps_corner_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.05` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_stepsCorner_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_steps_corner_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.05` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_steps_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_steps_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.05` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_steps_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_steps_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.05` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_topDiagonal_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_top_diagonal_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.269` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_topDiagonal_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_top_diagonal_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.269` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_top_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_top_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_top_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_top_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_waterfallTop_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_waterfall_top_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/5
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_waterfallTop_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_waterfall_top_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/6
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_waterfall_rock.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_waterfall_rock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_waterfall_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_cliff_waterfall_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_bambooStageA.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_bamboo_stage_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.549` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_bambooStageB.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_bamboo_stage_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.89` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_cornStageA.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_corn_stage_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.5` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_cornStageB.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_corn_stage_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_cornStageC.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_corn_stage_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.25` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_cornStageD.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_corn_stage_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 5/0/2
- model_dimension: `1.25` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_dirtDoubleRow.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_dirt_double_row`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_dirtDoubleRowCorner.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_dirt_double_row_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.938` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_dirtDoubleRowEnd.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_dirt_double_row_end`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.938` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_dirtRow.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_dirt_row`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_dirtRowCorner.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_dirt_row_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.779` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_dirtRowEnd.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_dirt_row_end`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.62` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_dirtSingle.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_dirt_single`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.4` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_leafsStageA.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_leafs_stage_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.427` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_leafsStageB.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_leafs_stage_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.677` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_wheatStageA.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_wheat_stage_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.54` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crops_wheatStageB.fbx`
- category: `Environment`; target: `env_kenney_nature_crops_wheat_stage_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.54` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crop_carrot.fbx`
- category: `Environment`; target: `env_kenney_nature_crop_carrot`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.619` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crop_melon.fbx`
- category: `Environment`; target: `env_kenney_nature_crop_melon`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.418` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crop_pumpkin.fbx`
- category: `Environment`; target: `env_kenney_nature_crop_pumpkin`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.452` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/crop_turnip.fbx`
- category: `Environment`; target: `env_kenney_nature_crop_turnip`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.661` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_bend.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_bend`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_bendCenter.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_bend_center`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_corner.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_gate.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_gate`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_planks.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_planks`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_planksDouble.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_planks_double`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_simple.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_simple`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_simpleCenter.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_simple_center`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_simpleDiagonal.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_simple_diagonal`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_simpleDiagonalCenter.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_simple_diagonal_center`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_simpleHigh.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_simple_high`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.041` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/fence_simpleLow.fbx`
- category: `Environment`; target: `env_kenney_nature_fence_simple_low`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.041` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/flower_purpleA.fbx`
- category: `Vegetation`; target: `env_kenney_nature_flower_purple_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.243` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/flower_purpleB.fbx`
- category: `Vegetation`; target: `env_kenney_nature_flower_purple_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.239` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/flower_purpleC.fbx`
- category: `Vegetation`; target: `env_kenney_nature_flower_purple_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.183` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/flower_redA.fbx`
- category: `Vegetation`; target: `env_kenney_nature_flower_red_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.293` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/flower_redB.fbx`
- category: `Vegetation`; target: `env_kenney_nature_flower_red_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.259` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/flower_redC.fbx`
- category: `Vegetation`; target: `env_kenney_nature_flower_red_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.233` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/flower_yellowA.fbx`
- category: `Vegetation`; target: `env_kenney_nature_flower_yellow_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.193` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/flower_yellowB.fbx`
- category: `Vegetation`; target: `env_kenney_nature_flower_yellow_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.239` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/flower_yellowC.fbx`
- category: `Vegetation`; target: `env_kenney_nature_flower_yellow_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.181` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/grass.fbx`
- category: `Vegetation`; target: `env_kenney_nature_grass`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.392` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/grass_large.fbx`
- category: `Vegetation`; target: `env_kenney_nature_grass_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.409` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/grass_leafs.fbx`
- category: `Vegetation`; target: `env_kenney_nature_grass_leafs`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.257` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/grass_leafsLarge.fbx`
- category: `Vegetation`; target: `env_kenney_nature_grass_leafs_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.487` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_grass.fbx`
- category: `Vegetation`; target: `env_kenney_nature_ground_grass`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathBend.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_bend`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathBendBank.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_bend_bank`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathCorner.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathCornerSmall.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_corner_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathCross.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_cross`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathEnd.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_end`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathEndClosed.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_end_closed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathOpen.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_open`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathRocks.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_rocks`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathSide.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_side`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathSideOpen.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_side_open`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathSplit.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_split`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathStraight.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_straight`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathTile.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_path_tile`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverBend.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_bend`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverBendBank.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_bend_bank`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverCorner.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverCornerSmall.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_corner_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverCross.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_cross`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverEnd.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_end`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverEndClosed.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_end_closed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverOpen.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_open`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverRocks.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_rocks`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/5
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverSide.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_side`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverSideOpen.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_side_open`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverSplit.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_split`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverStraight.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_straight`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_riverTile.fbx`
- category: `Environment`; target: `env_kenney_nature_ground_river_tile`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/hanging_moss.fbx`
- category: `Environment`; target: `env_kenney_nature_hanging_moss`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.478` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/lily_large.fbx`
- category: `Environment`; target: `env_kenney_nature_lily_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.314` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/lily_small.fbx`
- category: `Environment`; target: `env_kenney_nature_lily_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.218` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/log.fbx`
- category: `Environment`; target: `env_kenney_nature_log`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.71` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/log_large.fbx`
- category: `Environment`; target: `env_kenney_nature_log_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/log_stack.fbx`
- category: `Environment`; target: `env_kenney_nature_log_stack`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.71` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/log_stackLarge.fbx`
- category: `Environment`; target: `env_kenney_nature_log_stack_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.71` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/mushroom_red.fbx`
- category: `Vegetation`; target: `env_kenney_nature_mushroom_red`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.203` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/mushroom_redGroup.fbx`
- category: `Vegetation`; target: `env_kenney_nature_mushroom_red_group`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.27` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/mushroom_redTall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_mushroom_red_tall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.25` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/mushroom_tan.fbx`
- category: `Vegetation`; target: `env_kenney_nature_mushroom_tan`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.221` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/mushroom_tanGroup.fbx`
- category: `Vegetation`; target: `env_kenney_nature_mushroom_tan_group`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.27` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/mushroom_tanTall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_mushroom_tan_tall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.25` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/path_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_path_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/path_stoneCircle.fbx`
- category: `Environment`; target: `env_kenney_nature_path_stone_circle`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.932` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/path_stoneCorner.fbx`
- category: `Environment`; target: `env_kenney_nature_path_stone_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.778` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/path_stoneEnd.fbx`
- category: `Environment`; target: `env_kenney_nature_path_stone_end`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.618` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/path_wood.fbx`
- category: `Environment`; target: `env_kenney_nature_path_wood`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/path_woodCorner.fbx`
- category: `Environment`; target: `env_kenney_nature_path_wood_corner`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.75` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/path_woodEnd.fbx`
- category: `Environment`; target: `env_kenney_nature_path_wood_end`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.615` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/plant_bush.fbx`
- category: `Vegetation`; target: `env_kenney_nature_plant_bush`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.396` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/plant_bushDetailed.fbx`
- category: `Vegetation`; target: `env_kenney_nature_plant_bush_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.603` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/plant_bushLarge.fbx`
- category: `Vegetation`; target: `env_kenney_nature_plant_bush_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.374` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/plant_bushLargeTriangle.fbx`
- category: `Vegetation`; target: `env_kenney_nature_plant_bush_large_triangle`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.471` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/plant_bushSmall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_plant_bush_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.383` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/plant_bushTriangle.fbx`
- category: `Vegetation`; target: `env_kenney_nature_plant_bush_triangle`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.414` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/plant_flatShort.fbx`
- category: `Vegetation`; target: `env_kenney_nature_plant_flat_short`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.278` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/plant_flatTall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_plant_flat_tall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.284` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/platform_beach.fbx`
- category: `Environment`; target: `env_kenney_nature_platform_beach`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.893` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/platform_grass.fbx`
- category: `Vegetation`; target: `env_kenney_nature_platform_grass`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.893` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/platform_stone.fbx`
- category: `Environment`; target: `env_kenney_nature_platform_stone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.893` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/pot_large.fbx`
- category: `Environment`; target: `env_kenney_nature_pot_large`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.564` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/pot_small.fbx`
- category: `Environment`; target: `env_kenney_nature_pot_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.323` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeA.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_large_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.015` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeB.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_large_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1.015` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeC.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_large_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.064` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeD.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_large_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1.069` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeE.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_large_e`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.095` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeF.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_large_f`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1.095` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallA.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.361` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallB.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.361` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallC.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.361` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallD.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.426` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallE.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_e`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.361` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallF.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_f`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.354` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallFlatA.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_flat_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.496` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallFlatB.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_flat_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.43` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallFlatC.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_flat_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.47` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallG.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_g`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.686` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallH.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_h`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.462` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallI.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_i`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.414` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallTopA.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_top_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.489` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_smallTopB.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_small_top_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.646` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallA.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_tall_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.996` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallB.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_tall_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.884` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallC.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_tall_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.783` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallD.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_tall_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.772` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallE.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_tall_e`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.527` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallF.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_tall_f`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.574` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallG.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_tall_g`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.783` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallH.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_tall_h`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.711` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallI.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_tall_i`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.812` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallJ.fbx`
- category: `Environment`; target: `env_kenney_nature_rock_tall_j`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.62` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/sign.fbx`
- category: `Environment`; target: `env_kenney_nature_sign`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.409` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/statue_block.fbx`
- category: `Environment`; target: `env_kenney_nature_statue_block`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.4` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/statue_column.fbx`
- category: `Environment`; target: `env_kenney_nature_statue_column`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/statue_columnDamaged.fbx`
- category: `Environment`; target: `env_kenney_nature_statue_column_damaged`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.7` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/statue_head.fbx`
- category: `Environment`; target: `env_kenney_nature_statue_head`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/statue_obelisk.fbx`
- category: `Environment`; target: `env_kenney_nature_statue_obelisk`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.875` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/statue_ring.fbx`
- category: `Environment`; target: `env_kenney_nature_statue_ring`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.796` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_largeA.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_large_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.015` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_largeB.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_large_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.015` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_largeC.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_large_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.064` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_largeD.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_large_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.069` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_largeE.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_large_e`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.095` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_largeF.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_large_f`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.095` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallA.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.361` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallB.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.361` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallC.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.361` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallD.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.426` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallE.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_e`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.361` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallF.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_f`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.354` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallFlatA.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_flat_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.496` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallFlatB.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_flat_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.43` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallFlatC.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_flat_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.47` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallG.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_g`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.686` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallH.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_h`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.462` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallI.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_i`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.414` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallTopA.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_top_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.489` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_smallTopB.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_small_top_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.646` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_tallA.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_tall_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.996` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_tallB.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_tall_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.884` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_tallC.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_tall_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.783` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_tallD.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_tall_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.772` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_tallE.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_tall_e`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.527` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_tallF.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_tall_f`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.574` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_tallG.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_tall_g`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.783` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_tallH.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_tall_h`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.711` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_tallI.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_tall_i`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.812` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stone_tallJ.fbx`
- category: `Environment`; target: `env_kenney_nature_stone_tall_j`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.62` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stump_old.fbx`
- category: `Environment`; target: `env_kenney_nature_stump_old`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.371` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stump_oldTall.fbx`
- category: `Environment`; target: `env_kenney_nature_stump_old_tall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.667` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stump_round.fbx`
- category: `Environment`; target: `env_kenney_nature_stump_round`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.371` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stump_roundDetailed.fbx`
- category: `Environment`; target: `env_kenney_nature_stump_round_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.371` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stump_square.fbx`
- category: `Environment`; target: `env_kenney_nature_stump_square`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.36` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stump_squareDetailed.fbx`
- category: `Environment`; target: `env_kenney_nature_stump_square_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.44` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/stump_squareDetailedWide.fbx`
- category: `Environment`; target: `env_kenney_nature_stump_square_detailed_wide`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.64` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tent_detailedClosed.fbx`
- category: `Environment`; target: `env_kenney_nature_tent_detailed_closed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.875` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tent_detailedOpen.fbx`
- category: `Environment`; target: `env_kenney_nature_tent_detailed_open`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.875` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tent_smallClosed.fbx`
- category: `Environment`; target: `env_kenney_nature_tent_small_closed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/4
- model_dimension: `0.636` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tent_smallOpen.fbx`
- category: `Environment`; target: `env_kenney_nature_tent_small_open`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `0.666` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_blocks.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_blocks`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.185` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_blocks_dark.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_blocks_dark`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.185` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_blocks_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_blocks_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.185` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_cone.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_cone`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.43` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_cone_dark.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_cone_dark`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.43` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_cone_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_cone_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.43` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_default.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_default`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.708` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_default_dark.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_default_dark`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.708` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_default_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_default_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.708` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_detailed.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1.332` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_detailed_dark.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_detailed_dark`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1.332` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_detailed_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_detailed_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1.332` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_fat.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_fat`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.15` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_fat_darkh.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_fat_darkh`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.15` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_fat_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_fat_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.15` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_oak.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_oak`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.226` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_oak_dark.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_oak_dark`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.226` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_oak_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_oak_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.226` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_palm.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_palm`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.515` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_palmBend.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_palm_bend`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.38` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_palmDetailedShort.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_palm_detailed_short`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 3/0/2
- model_dimension: `1.034` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_palmDetailedTall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_palm_detailed_tall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 3/0/2
- model_dimension: `1.248` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_palmShort.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_palm_short`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.056` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_palmTall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_palm_tall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.356` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineDefaultA.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_default_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.546` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineDefaultB.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_default_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.546` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineGroundA.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_ground_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `0.914` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineGroundB.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_ground_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `1.042` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineRoundA.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_round_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.366` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineRoundB.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_round_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/3
- model_dimension: `1.196` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineRoundC.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_round_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.253` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineRoundD.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_round_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.082` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineRoundE.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_round_e`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.255` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineRoundF.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_round_f`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.255` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineSmallA.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_small_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.967` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineSmallB.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_small_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.009` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineSmallC.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_small_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.123` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineSmallD.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_small_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `0.956` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineTallA.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_tall_a`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.53` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineTallA_detailed.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_tall_a_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.53` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineTallB.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_tall_b`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.935` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineTallB_detailed.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_tall_b_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.935` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineTallC.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_tall_c`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.67` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineTallC_detailed.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_tall_c_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.67` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineTallD.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_tall_d`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `2.075` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_pineTallD_detailed.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_pine_tall_d_detailed`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `2.075` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_plateau.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_plateau`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.249` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_plateau_dark.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_plateau_dark`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.249` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_plateau_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_plateau_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.249` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_simple.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_simple`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.519` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_simple_dark.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_simple_dark`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.519` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_simple_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_simple_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.519` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_small.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.11` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_small_dark.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_small_dark`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.11` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_small_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_small_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.11` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_tall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_tall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.688` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_tall_dark.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_tall_dark`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.688` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_tall_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_tall_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.688` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_thin.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_thin`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.49` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_thin_dark.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_thin_dark`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.49` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_thin_fall.fbx`
- category: `Vegetation`; target: `env_kenney_nature_tree_thin_fall`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/2
- model_dimension: `1.49` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Environment/PolyHaven/Textures/coral_mud_01_ao_1k.jpg`
- category: `Environment`; target: `material_polyhaven_coral_mud_01_ao_jpg`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/Environment/PolyHaven/Textures/coral_mud_01_diff_1k.jpg`
- category: `Environment`; target: `material_polyhaven_coral_mud_01_diff_jpg`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/Environment/PolyHaven/Textures/coral_mud_01_nor_gl_1k.jpg`
- category: `Environment`; target: `material_polyhaven_coral_mud_01_nor_gl_jpg`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/Environment/PolyHaven/Textures/coral_mud_01_rough_1k.jpg`
- category: `Environment`; target: `material_polyhaven_coral_mud_01_rough_jpg`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/Environment/PolyHaven/Textures/sand_01_ao_1k.jpg`
- category: `Environment`; target: `material_polyhaven_sand_01_ao_jpg`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/Environment/PolyHaven/Textures/sand_01_diff_1k.jpg`
- category: `Environment`; target: `material_polyhaven_sand_01_diff_jpg`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/Environment/PolyHaven/Textures/sand_01_nor_gl_1k.jpg`
- category: `Environment`; target: `material_polyhaven_sand_01_nor_gl_jpg`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/Environment/PolyHaven/Textures/sand_01_rough_1k.jpg`
- category: `Environment`; target: `material_polyhaven_sand_01_rough_jpg`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``

## 边界

本报告只记录 Unity 当前导入结果和建议；不修改 Registry、asset_state、Prefab、场景或候选包，不写 ReleaseApproved，不下载资源。模型自身尺寸以记录为准，不由管线统一 scale；后续 Prefab/责任系统决定具体目标尺度、锚点和碰撞修正。
