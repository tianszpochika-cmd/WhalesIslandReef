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
| candidate files | 7 |
| models | 7 |
| textures | 0 |
| audio | 0 |
| prefabs | 0 |
| Mesh | 15 |
| animation clips | 27 |
| materials | 7 |
| errors | 0 |
| warnings | 7 |

## 分类

- `Buildings`: 2
- `Characters`: 1
- `Environment`: 2
- `Ships`: 1
- `Vegetation`: 1

## manifest

### pack_kenney_blocky_characters_vertical_slice_v1
- file: `docs/game/design/common/assets/manifests/kenney_blocky_characters_vertical_slice.manifest.json`
- review_status: `Candidate`
- import_status: `ImportVerified`
- declared/scanned: 1/1
### pack_kenney_pirate_kit_vertical_slice_v1
- file: `docs/game/design/common/assets/manifests/kenney_pirate_kit_vertical_slice.manifest.json`
- review_status: `Candidate`
- import_status: `ImportVerified`
- declared/scanned: 6/6

## 当前候选问题与导入建议

### `Assets/Art/Candidates/Kenney/BlockyCharacters/Models/character-a.fbx`
- category: `Characters`; target: `asset_ph_crew_male`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 6/27/1
- model_dimension: `9` Unity units; target scale: `character height/foot placement from content scale; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/PirateKit/Models/palm-detailed-straight.fbx`
- category: `Vegetation`; target: `island_saint_marys`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `4.213` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/PirateKit/Models/patch-sand.fbx`
- category: `Environment`; target: `island_saint_marys`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `7.736` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/PirateKit/Models/rocks-sand-a.fbx`
- category: `Environment`; target: `island_saint_marys`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `5.113` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/PirateKit/Models/ship-pirate-small.fbx`
- category: `Ships`; target: `candidate_ship_pirate_small`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 4/0/1
- model_dimension: `9.964` Unity units; target scale: `ship waterline/keel and content hull dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/PirateKit/Models/structure-platform-dock-small.fbx`
- category: `Buildings`; target: `building_saint_marys_dock`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.512` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/PirateKit/Models/structure-platform-small.fbx`
- category: `Buildings`; target: `building_small_boat_pier`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/0/1
- model_dimension: `2.512` Unity units; target scale: `door/foundation/module dimensions; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional

## 边界

本报告只记录 Unity 当前导入结果和建议；不修改 Registry、asset_state、Prefab、场景或候选包，不写 ReleaseApproved，不下载资源。模型自身尺寸以记录为准，不由管线统一 scale；后续 Prefab/责任系统决定具体目标尺度、锚点和碰撞修正。
