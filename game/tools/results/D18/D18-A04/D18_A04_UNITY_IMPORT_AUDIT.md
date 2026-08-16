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
| manifest | 1 |
| candidate files | 24 |
| models | 24 |
| textures | 0 |
| audio | 0 |
| prefabs | 0 |
| Mesh | 30 |
| animation clips | 228 |
| materials | 121 |
| errors | 0 |
| warnings | 25 |

## 分类

- `Animals`: 15
- `Characters`: 1
- `Props`: 8

## manifest

### pack_d18_a04_characters_animals_fixture_v1
- file: `docs/game/design/common/assets/manifests/d18_a04_characters_animals_fixture.manifest.json`
- review_status: `Candidate`
- import_status: `NotImported`
- declared/scanned: 24/24
- warning `import_status_not_verified`: A01 import_status is not ImportVerified

## 当前候选问题与导入建议

### `Assets/Art/Candidates/Kenney/CharactersAnimals/KenneyBlockyCharacters/Models/character-a.fbx`
- category: `Characters`; target: `asset_kenney_blocky_character_a_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 6/27/1
- model_dimension: `9` Unity units; target scale: `character height/foot placement from content scale; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Alpaca.fbx`
- category: `Animals`; target: `animal_quaternius_alpaca_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/13/7
- model_dimension: `0.054` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Bull.fbx`
- category: `Animals`; target: `animal_quaternius_bull_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/12/7
- model_dimension: `0.081` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Cow.fbx`
- category: `Animals`; target: `animal_quaternius_cow_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/12/7
- model_dimension: `0.081` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Deer.fbx`
- category: `Animals`; target: `animal_quaternius_deer_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/13/7
- model_dimension: `0.044` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Donkey.fbx`
- category: `Animals`; target: `animal_quaternius_donkey_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/13/8
- model_dimension: `0.047` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Fox.fbx`
- category: `Animals`; target: `animal_quaternius_fox_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/12/5
- model_dimension: `0.059` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Horse.fbx`
- category: `Animals`; target: `animal_quaternius_horse_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/13/8
- model_dimension: `0.057` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Horse_White.fbx`
- category: `Animals`; target: `animal_quaternius_horse_white_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/13/7
- model_dimension: `0.057` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Husky.fbx`
- category: `Animals`; target: `animal_quaternius_husky_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/12/5
- model_dimension: `0.039` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/ShibaInu.fbx`
- category: `Animals`; target: `animal_quaternius_shiba_inu_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/12/6
- model_dimension: `0.039` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Stag.fbx`
- category: `Animals`; target: `animal_quaternius_stag_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 2/13/5
- model_dimension: `0.048` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Wolf.fbx`
- category: `Animals`; target: `animal_quaternius_wolf_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/12/4
- model_dimension: `0.056` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedFish/Models/Dolphin.fbx`
- category: `Props`; target: `animal_quaternius_dolphin_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/1/2
- model_dimension: `0.093` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedFish/Models/Fish1.fbx`
- category: `Animals`; target: `fish_quaternius_fish_01_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/1/3
- model_dimension: `0.08` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedFish/Models/Fish2.fbx`
- category: `Animals`; target: `fish_quaternius_fish_02_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/1/3
- model_dimension: `0.099` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedFish/Models/Fish3.fbx`
- category: `Animals`; target: `fish_quaternius_fish_03_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/1/3
- model_dimension: `0.079` Unity units; target scale: `species proportion and habitat reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedFish/Models/Manta ray.fbx`
- category: `Props`; target: `animal_quaternius_manta_ray_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/1/2
- model_dimension: `0.117` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedFish/Models/Shark.fbx`
- category: `Props`; target: `animal_quaternius_shark_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/1/2
- model_dimension: `0.099` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedFish/Models/Whale.fbx`
- category: `Props`; target: `animal_quaternius_whale_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/1/2
- model_dimension: `0.102` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedMen/Models/Male_Casual.fbx`
- category: `Props`; target: `crew_quaternius_male_casual_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/11/6
- model_dimension: `0.052` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedMen/Models/Male_LongSleeve.fbx`
- category: `Props`; target: `crew_quaternius_male_long_sleeve_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/11/5
- model_dimension: `0.052` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedMen/Models/Male_Shirt.fbx`
- category: `Props`; target: `crew_quaternius_male_shirt_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/11/9
- model_dimension: `0.052` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional
### `Assets/Art/Candidates/Kenney/CharactersAnimals/QuaterniusAnimatedMen/Models/Male_Suit.fbx`
- category: `Props`; target: `crew_quaternius_male_suit_v01`; importer: `ModelImporter`
- source_exists: `True`; mesh/animation/material: 1/11/7
- model_dimension: `0.052` Unity units; target scale: `category-specific D18 reference; owner Prefab decides target scale`
- warning `model_name_not_snake_case`: Model file name does not match the D18 stable naming rule
- suggestion: Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup
- suggestion: Animation import is enabled for a non-animated category; confirm this is intentional

## 边界

本报告只记录 Unity 当前导入结果和建议；不修改 Registry、asset_state、Prefab、场景或候选包，不写 ReleaseApproved，不下载资源。模型自身尺寸以记录为准，不由管线统一 scale；后续 Prefab/责任系统决定具体目标尺度、锚点和碰撞修正。
