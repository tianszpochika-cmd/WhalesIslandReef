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
| manifest | 7 |
| candidate files | 1337 |
| models | 0 |
| textures | 1307 |
| audio | 30 |
| prefabs | 0 |
| Mesh | 0 |
| animation clips | 0 |
| materials | 0 |
| errors | 0 |
| warnings | 37 |

## 分类

- `Audio`: 30
- `Environment`: 20
- `Props`: 59
- `UI`: 1224
- `Vegetation`: 4

## manifest

### pack_kenney_impact_sounds_v1
- file: `docs/game/design/common/assets/packs/ui_materials_audio/kenney_impact_sounds_v1.manifest.json`
- review_status: `ImportVerified`
- import_status: `ImportVerified`
- declared/scanned: 10/10
- warning `review_status_not_candidate`: The audit reports the manifest status but never changes it
### pack_kenney_input_prompts_v15
- file: `docs/game/design/common/assets/packs/ui_materials_audio/kenney_input_prompts_v15.manifest.json`
- review_status: `ImportVerified`
- import_status: `ImportVerified`
- declared/scanned: 356/356
- warning `review_status_not_candidate`: The audit reports the manifest status but never changes it
### pack_kenney_interface_sounds_v1
- file: `docs/game/design/common/assets/packs/ui_materials_audio/kenney_interface_sounds_v1.manifest.json`
- review_status: `ImportVerified`
- import_status: `ImportVerified`
- declared/scanned: 10/10
- warning `review_status_not_candidate`: The audit reports the manifest status but never changes it
### pack_kenney_ui_audio_v1
- file: `docs/game/design/common/assets/packs/ui_materials_audio/kenney_ui_audio_v1.manifest.json`
- review_status: `ImportVerified`
- import_status: `ImportVerified`
- declared/scanned: 10/10
- warning `review_status_not_candidate`: The audit reports the manifest status but never changes it
### pack_kenney_ui_pack_v2
- file: `docs/game/design/common/assets/packs/ui_materials_audio/kenney_ui_v2.manifest.json`
- review_status: `ImportVerified`
- import_status: `ImportVerified`
- declared/scanned: 868/868
- warning `review_status_not_candidate`: The audit reports the manifest status but never changes it
### pack_polyhaven_hdri_sky_references_v1
- file: `docs/game/design/common/assets/packs/ui_materials_audio/polyhaven_hdri_sky_references_v1.manifest.json`
- review_status: `ImportVerified`
- import_status: `ImportVerified`
- declared/scanned: 4/4
- warning `review_status_not_candidate`: The audit reports the manifest status but never changes it
### pack_polyhaven_ui_materials_selected_v1
- file: `docs/game/design/common/assets/packs/ui_materials_audio/polyhaven_ui_materials_selected_v1.manifest.json`
- review_status: `ImportVerified`
- import_status: `ImportVerified`
- declared/scanned: 79/79
- warning `review_status_not_candidate`: The audit reports the manifest status but never changes it

## 当前候选问题与导入建议

### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/footstep_carpet_000.ogg`
- category: `Audio`; target: `candidate_audio_impact_sounds_footstep_carpet_000`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/footstep_carpet_001.ogg`
- category: `Audio`; target: `candidate_audio_impact_sounds_footstep_carpet_001`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/footstep_carpet_002.ogg`
- category: `Audio`; target: `candidate_audio_impact_sounds_footstep_carpet_002`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/footstep_carpet_003.ogg`
- category: `Audio`; target: `candidate_audio_impact_sounds_footstep_carpet_003`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/footstep_carpet_004.ogg`
- category: `Audio`; target: `candidate_audio_impact_sounds_footstep_carpet_004`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/footstep_concrete_000.ogg`
- category: `Audio`; target: `candidate_audio_impact_sounds_footstep_concrete_000`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/footstep_concrete_001.ogg`
- category: `Audio`; target: `candidate_audio_impact_sounds_footstep_concrete_001`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/footstep_concrete_002.ogg`
- category: `Audio`; target: `candidate_audio_impact_sounds_footstep_concrete_002`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/footstep_concrete_003.ogg`
- category: `Audio`; target: `candidate_audio_impact_sounds_footstep_concrete_003`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyImpactSounds/footstep_concrete_004.ogg`
- category: `Audio`; target: `candidate_audio_impact_sounds_footstep_concrete_004`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/back_001.ogg`
- category: `Audio`; target: `candidate_audio_interface_sounds_back_001`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/back_002.ogg`
- category: `Audio`; target: `candidate_audio_interface_sounds_back_002`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/back_003.ogg`
- category: `Audio`; target: `candidate_audio_interface_sounds_back_003`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/back_004.ogg`
- category: `Audio`; target: `candidate_audio_interface_sounds_back_004`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/bong_001.ogg`
- category: `Audio`; target: `candidate_audio_interface_sounds_bong_001`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/click_001.ogg`
- category: `Audio`; target: `candidate_audio_interface_sounds_click_001`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/click_002.ogg`
- category: `Audio`; target: `candidate_audio_interface_sounds_click_002`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/click_003.ogg`
- category: `Audio`; target: `candidate_audio_interface_sounds_click_003`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/click_004.ogg`
- category: `Audio`; target: `candidate_audio_interface_sounds_click_004`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyInterfaceSounds/click_005.ogg`
- category: `Audio`; target: `candidate_audio_interface_sounds_click_005`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/click1.ogg`
- category: `Audio`; target: `candidate_audio_ui_audio_click1`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/click2.ogg`
- category: `Audio`; target: `candidate_audio_ui_audio_click2`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/click3.ogg`
- category: `Audio`; target: `candidate_audio_ui_audio_click3`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/click4.ogg`
- category: `Audio`; target: `candidate_audio_ui_audio_click4`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/click5.ogg`
- category: `Audio`; target: `candidate_audio_ui_audio_click5`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/mouseclick1.ogg`
- category: `Audio`; target: `candidate_audio_ui_audio_mouseclick1`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/mouserelease1.ogg`
- category: `Audio`; target: `candidate_audio_ui_audio_mouserelease1`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/rollover1.ogg`
- category: `Audio`; target: `candidate_audio_ui_audio_rollover1`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/rollover2.ogg`
- category: `Audio`; target: `candidate_audio_ui_audio_rollover2`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/Audio/KenneyUiAudio/rollover3.ogg`
- category: `Audio`; target: `candidate_audio_ui_audio_rollover3`; importer: `AudioImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
- warning `short_audio_vorbis`: Very short audio uses Vorbis; review transient quality
### `Assets/Art/Candidates/UIMaterialsAudio/HDRI/PolyHaven/green_point_park_1k.hdr`
- category: `Props`; target: `candidate_hdri_green_point_park`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/HDRI/PolyHaven/joburg_central_sunset_1k.hdr`
- category: `Props`; target: `candidate_hdri_joburg_central_sunset`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/HDRI/PolyHaven/kloppenheim_06_1k.hdr`
- category: `Props`; target: `candidate_hdri_kloppenheim_06`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/HDRI/PolyHaven/venice_sunset_1k.hdr`
- category: `Props`; target: `candidate_hdri_venice_sunset`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_grass_rock/aerial_grass_rock_albedo.jpg`
- category: `Vegetation`; target: `candidate_material_aerial_grass_rock_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_grass_rock/aerial_grass_rock_ao.jpg`
- category: `Vegetation`; target: `candidate_material_aerial_grass_rock_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_grass_rock/aerial_grass_rock_normal.jpg`
- category: `Vegetation`; target: `candidate_material_aerial_grass_rock_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_grass_rock/aerial_grass_rock_roughness.jpg`
- category: `Vegetation`; target: `candidate_material_aerial_grass_rock_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_rocks_01/aerial_rocks_01_albedo.jpg`
- category: `Props`; target: `candidate_material_aerial_rocks_01_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_rocks_01/aerial_rocks_01_ao.jpg`
- category: `Props`; target: `candidate_material_aerial_rocks_01_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_rocks_01/aerial_rocks_01_normal.jpg`
- category: `Props`; target: `candidate_material_aerial_rocks_01_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_rocks_01/aerial_rocks_01_roughness.jpg`
- category: `Props`; target: `candidate_material_aerial_rocks_01_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/bark_brown_01/bark_brown_01_albedo.jpg`
- category: `Props`; target: `candidate_material_bark_brown_01_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/bark_brown_01/bark_brown_01_ao.jpg`
- category: `Props`; target: `candidate_material_bark_brown_01_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/bark_brown_01/bark_brown_01_normal.jpg`
- category: `Props`; target: `candidate_material_bark_brown_01_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/bark_brown_01/bark_brown_01_roughness.jpg`
- category: `Props`; target: `candidate_material_bark_brown_01_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/blue_metal_plate/blue_metal_plate_albedo.jpg`
- category: `Props`; target: `candidate_material_blue_metal_plate_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/blue_metal_plate/blue_metal_plate_ao.jpg`
- category: `Props`; target: `candidate_material_blue_metal_plate_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/blue_metal_plate/blue_metal_plate_normal.jpg`
- category: `Props`; target: `candidate_material_blue_metal_plate_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/blue_metal_plate/blue_metal_plate_roughness.jpg`
- category: `Props`; target: `candidate_material_blue_metal_plate_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_leather/brown_leather_albedo.jpg`
- category: `Props`; target: `candidate_material_brown_leather_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_leather/brown_leather_ao.jpg`
- category: `Props`; target: `candidate_material_brown_leather_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_leather/brown_leather_normal.jpg`
- category: `Props`; target: `candidate_material_brown_leather_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_leather/brown_leather_roughness.jpg`
- category: `Props`; target: `candidate_material_brown_leather_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud/brown_mud_albedo.jpg`
- category: `Props`; target: `candidate_material_brown_mud_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud/brown_mud_ao.jpg`
- category: `Props`; target: `candidate_material_brown_mud_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud/brown_mud_normal.jpg`
- category: `Props`; target: `candidate_material_brown_mud_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud/brown_mud_roughness.jpg`
- category: `Props`; target: `candidate_material_brown_mud_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud_dry/brown_mud_dry_albedo.jpg`
- category: `Props`; target: `candidate_material_brown_mud_dry_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud_dry/brown_mud_dry_ao.jpg`
- category: `Props`; target: `candidate_material_brown_mud_dry_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud_dry/brown_mud_dry_normal.jpg`
- category: `Props`; target: `candidate_material_brown_mud_dry_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud_dry/brown_mud_dry_roughness.jpg`
- category: `Props`; target: `candidate_material_brown_mud_dry_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_planks_03/brown_planks_03_albedo.jpg`
- category: `Props`; target: `candidate_material_brown_planks_03_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_planks_03/brown_planks_03_ao.jpg`
- category: `Props`; target: `candidate_material_brown_planks_03_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_planks_03/brown_planks_03_normal.jpg`
- category: `Props`; target: `candidate_material_brown_planks_03_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_planks_03/brown_planks_03_roughness.jpg`
- category: `Props`; target: `candidate_material_brown_planks_03_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/clean_pebbles/clean_pebbles_albedo.jpg`
- category: `Props`; target: `candidate_material_clean_pebbles_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/clean_pebbles/clean_pebbles_ao.jpg`
- category: `Props`; target: `candidate_material_clean_pebbles_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/clean_pebbles/clean_pebbles_normal.jpg`
- category: `Props`; target: `candidate_material_clean_pebbles_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/clean_pebbles/clean_pebbles_roughness.jpg`
- category: `Props`; target: `candidate_material_clean_pebbles_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_01/coast_sand_01_albedo.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_01_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_01/coast_sand_01_ao.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_01_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_01/coast_sand_01_normal.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_01_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_01/coast_sand_01_roughness.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_01_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_02/coast_sand_02_albedo.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_02_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_02/coast_sand_02_ao.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_02_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_02/coast_sand_02_normal.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_02_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_02/coast_sand_02_roughness.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_02_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_03/coast_sand_03_albedo.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_03_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_03/coast_sand_03_ao.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_03_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_03/coast_sand_03_normal.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_03_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_03/coast_sand_03_roughness.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_03_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_04/coast_sand_04_albedo.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_04_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_04/coast_sand_04_ao.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_04_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_04/coast_sand_04_normal.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_04_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/coast_sand_04/coast_sand_04_roughness.jpg`
- category: `Environment`; target: `candidate_material_coast_sand_04_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/fabric_pattern_05/fabric_pattern_05_ao.jpg`
- category: `Props`; target: `candidate_material_fabric_pattern_05_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/fabric_pattern_05/fabric_pattern_05_normal.jpg`
- category: `Props`; target: `candidate_material_fabric_pattern_05_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/fabric_pattern_05/fabric_pattern_05_roughness.jpg`
- category: `Props`; target: `candidate_material_fabric_pattern_05_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/fine_grained_wood/fine_grained_wood_albedo.jpg`
- category: `Props`; target: `candidate_material_fine_grained_wood_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/fine_grained_wood/fine_grained_wood_ao.jpg`
- category: `Props`; target: `candidate_material_fine_grained_wood_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/fine_grained_wood/fine_grained_wood_normal.jpg`
- category: `Props`; target: `candidate_material_fine_grained_wood_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/fine_grained_wood/fine_grained_wood_roughness.jpg`
- category: `Props`; target: `candidate_material_fine_grained_wood_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/metal_plate/metal_plate_albedo.jpg`
- category: `Props`; target: `candidate_material_metal_plate_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/metal_plate/metal_plate_ao.jpg`
- category: `Props`; target: `candidate_material_metal_plate_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/metal_plate/metal_plate_normal.jpg`
- category: `Props`; target: `candidate_material_metal_plate_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/metal_plate/metal_plate_roughness.jpg`
- category: `Props`; target: `candidate_material_metal_plate_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rock_face/rock_face_albedo.jpg`
- category: `Environment`; target: `candidate_material_rock_face_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rock_face/rock_face_ao.jpg`
- category: `Environment`; target: `candidate_material_rock_face_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rock_face/rock_face_normal.jpg`
- category: `Environment`; target: `candidate_material_rock_face_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rock_face/rock_face_roughness.jpg`
- category: `Environment`; target: `candidate_material_rock_face_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rough_wood/rough_wood_albedo.jpg`
- category: `Props`; target: `candidate_material_rough_wood_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rough_wood/rough_wood_ao.jpg`
- category: `Props`; target: `candidate_material_rough_wood_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rough_wood/rough_wood_normal.jpg`
- category: `Props`; target: `candidate_material_rough_wood_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rough_wood/rough_wood_roughness.jpg`
- category: `Props`; target: `candidate_material_rough_wood_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/wooden_planks/wooden_planks_albedo.jpg`
- category: `Props`; target: `candidate_material_wooden_planks_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/wooden_planks/wooden_planks_ao.jpg`
- category: `Props`; target: `candidate_material_wooden_planks_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/wooden_planks/wooden_planks_normal.jpg`
- category: `Props`; target: `candidate_material_wooden_planks_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/wooden_planks/wooden_planks_roughness.jpg`
- category: `Props`; target: `candidate_material_wooden_planks_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/wood_floor_deck/wood_floor_deck_albedo.jpg`
- category: `Props`; target: `candidate_material_wood_floor_deck_albedo`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/wood_floor_deck/wood_floor_deck_ao.jpg`
- category: `Props`; target: `candidate_material_wood_floor_deck_ao`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/wood_floor_deck/wood_floor_deck_normal.jpg`
- category: `Props`; target: `candidate_material_wood_floor_deck_normal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/wood_floor_deck/wood_floor_deck_roughness.jpg`
- category: `Props`; target: `candidate_material_wood_floor_deck_roughness`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_0.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_0`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_0_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_0_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_1.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_1`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_1_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_1_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_2.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_2`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_2_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_2_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_3.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_3`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_3_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_3_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_4.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_4`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_4_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_4_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_5.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_5`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_5_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_5_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_6.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_6`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_6_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_6_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_7.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_7`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_7_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_7_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_8.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_8`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_8_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_8_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_9.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_9`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_9_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_9_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_a.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_a`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_alt.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_alt`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_alt_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_alt_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_any.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_any`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_any_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_any_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_apostrophe.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_apostrophe`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_apostrophe_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_apostrophe_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_all.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_all`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_down.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_down`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_down_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_down_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_horizontal.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_horizontal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_horizontal_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_horizontal_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_left.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_left`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_left_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_left_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_none.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_none`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_right.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_right`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_right_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_right_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_up.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_up`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_up_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_up_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_vertical.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_vertical`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrows_vertical_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrows_vertical_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrow_down.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrow_down`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrow_down_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrow_down_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrow_left.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrow_left`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrow_left_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrow_left_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrow_right.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrow_right`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrow_right_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrow_right_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrow_up.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrow_up`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_arrow_up_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_arrow_up_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_asterisk.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_asterisk`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_asterisk_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_asterisk_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_a_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_a_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_b.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_b`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_backspace.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_backspace`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_backspace_icon.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_backspace_icon`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_backspace_icon_alternative.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_backspace_icon_alternative`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_backspace_icon_alternative_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_backspace_icon_alternative_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_backspace_icon_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_backspace_icon_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_backspace_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_backspace_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_bracket_close.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_bracket_close`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_bracket_close_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_bracket_close_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_bracket_greater.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_bracket_greater`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_bracket_greater_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_bracket_greater_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_bracket_less.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_bracket_less`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_bracket_less_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_bracket_less_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_bracket_open.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_bracket_open`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_bracket_open_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_bracket_open_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_b_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_b_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_c.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_c`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_capslock.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_capslock`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_capslock_icon.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_capslock_icon`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_capslock_icon_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_capslock_icon_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_capslock_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_capslock_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_caret.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_caret`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_caret_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_caret_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_colon.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_colon`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_colon_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_colon_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_comma.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_comma`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_command.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_command`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_command_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_command_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_comma_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_comma_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_ctrl.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_ctrl`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_ctrl_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_ctrl_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_c_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_c_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_d.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_d`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_delete.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_delete`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_delete_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_delete_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_d_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_d_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_e.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_end.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_end`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_end_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_end_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_enter.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_enter`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_enter_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_enter_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_equals.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_equals`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_equals_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_equals_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_escape.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_escape`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_escape_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_escape_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_exclamation.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_exclamation`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_exclamation_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_exclamation_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_e_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_e_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f1.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f1`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f10.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f10`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f10_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f10_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f11.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f11`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f11_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f11_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f12.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f12`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f12_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f12_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f1_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f1_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f2.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f2`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f2_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f2_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f3.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f3`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f3_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f3_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f4.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f4`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f4_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f4_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f5.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f5`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f5_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f5_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f6.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f6`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f6_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f6_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f7.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f7`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f7_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f7_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f8.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f8`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f8_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f8_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f9.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f9`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f9_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f9_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_function.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_function`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_function_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_function_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_f_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_f_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_g.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_g`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_g_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_g_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_h.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_h`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_home.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_home`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_home_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_home_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_h_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_h_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_i.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_i`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_insert.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_insert`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_insert_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_insert_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_i_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_i_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_j.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_j`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_j_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_j_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_k.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_k`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_k_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_k_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_l.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_l`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_l_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_l_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_m.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_m`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_minus.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_minus`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_minus_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_minus_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_m_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_m_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_n.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_numlock.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_numlock`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_numlock_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_numlock_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_numpad_enter.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_numpad_enter`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_numpad_enter_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_numpad_enter_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_numpad_plus.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_numpad_plus`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_numpad_plus_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_numpad_plus_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_n_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_n_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_o.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_o`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_option.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_option`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_option_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_option_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_o_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_o_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_p.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_p`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_page_down.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_page_down`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_page_down_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_page_down_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_page_up.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_page_up`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_page_up_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_page_up_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_pause.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_pause`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_pause_break.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_pause_break`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_pause_break_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_pause_break_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_pause_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_pause_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_period.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_period`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_period_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_period_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_plus.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_plus`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_plus_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_plus_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_printscreen.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_printscreen`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_printscreen_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_printscreen_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_p_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_p_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_q.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_q`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_question.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_question`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_question_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_question_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_quote.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_quote`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_quote_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_quote_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_q_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_q_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_r.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_r`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_return.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_return`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_return_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_return_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_r_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_r_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_s.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_scroll_lock.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_scroll_lock`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_scroll_lock_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_scroll_lock_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_semicolon.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_semicolon`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_semicolon_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_semicolon_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_shift.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_shift`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_shift_icon.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_shift_icon`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_shift_icon_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_shift_icon_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_shift_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_shift_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_slash_back.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_slash_back`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_slash_back_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_slash_back_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_slash_forward.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_slash_forward`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_slash_forward_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_slash_forward_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_space.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_space`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_space_icon.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_space_icon`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_space_icon_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_space_icon_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_space_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_space_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_s_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_s_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_t.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_t`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_tab.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_tab`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_tab_icon.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_tab_icon`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_tab_icon_alternative.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_tab_icon_alternative`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_tab_icon_alternative_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_tab_icon_alternative_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_tab_icon_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_tab_icon_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_tab_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_tab_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_tilde.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_tilde`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_tilde_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_tilde_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_t_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_t_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_u.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_u`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_underscore.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_underscore`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_underscore_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_underscore_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_u_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_u_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_v.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_v`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_v_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_v_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_w.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_win.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_win`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_win_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_win_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_w_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_w_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_x.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_x`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_x_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_x_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_y.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_y`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_y_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_y_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_z.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_z`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/keyboard_z_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_keyboard_z_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_horizontal.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_horizontal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_left.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_left`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_left_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_left_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_move.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_move`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_right.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_right`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_right_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_right_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_scroll.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_scroll`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_scroll_down.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_scroll_down`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_scroll_down_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_scroll_down_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_scroll_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_scroll_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_scroll_up.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_scroll_up`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_scroll_up_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_scroll_up_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_scroll_vertical.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_scroll_vertical`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_scroll_vertical_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_scroll_vertical_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_side.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_side`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_side_back.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_side_back`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_side_back_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_side_back_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_side_forward.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_side_forward`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_side_forward_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_side_forward_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_side_outline.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_side_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_small.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/KeyboardMouse/mouse_vertical.png`
- category: `UI`; target: `candidate_input_keyboard_mouse_mouse_vertical`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/controller_xbox360.png`
- category: `UI`; target: `candidate_input_xbox_series_controller_xbox360`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/controller_xboxone.png`
- category: `UI`; target: `candidate_input_xbox_series_controller_xboxone`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/controller_xboxseries.png`
- category: `UI`; target: `candidate_input_xbox_series_controller_xboxseries`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/controller_xbox_adaptive.png`
- category: `UI`; target: `candidate_input_xbox_series_controller_xbox_adaptive`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_a.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_a`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_a_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_a_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_b.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_b`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_back.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_back`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_back_icon.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_back_icon`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_back_icon_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_back_icon_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_back_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_back_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_b_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_b_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_color_a.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_color_a`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_color_a_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_color_a_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_color_b.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_color_b`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_color_b_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_color_b_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_color_x.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_color_x`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_color_x_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_color_x_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_color_y.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_color_y`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_color_y_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_color_y_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_menu.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_menu`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_menu_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_menu_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_share.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_share`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_share_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_share_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_start.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_start`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_start_icon.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_start_icon`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_start_icon_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_start_icon_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_start_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_start_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_view.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_view`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_view_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_view_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_x.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_x`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_x_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_x_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_y.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_y`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_button_y_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_button_y_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_all.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_all`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_down.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_down`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_down_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_down_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_horizontal.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_horizontal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_horizontal_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_horizontal_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_left.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_left`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_left_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_left_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_none.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_none`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_right.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_right`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_right_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_right_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_round.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_round`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_round_all.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_round_all`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_round_down.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_round_down`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_round_horizontal.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_round_horizontal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_round_left.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_round_left`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_round_right.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_round_right`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_round_up.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_round_up`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_round_vertical.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_round_vertical`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_up.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_up`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_up_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_up_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_vertical.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_vertical`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_dpad_vertical_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_dpad_vertical_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_elite_paddle_bottom_left.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_elite_paddle_bottom_left`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_elite_paddle_bottom_left_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_elite_paddle_bottom_left_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_elite_paddle_bottom_right.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_elite_paddle_bottom_right`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_elite_paddle_bottom_right_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_elite_paddle_bottom_right_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_elite_paddle_top_left.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_elite_paddle_top_left`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_elite_paddle_top_left_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_elite_paddle_top_left_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_elite_paddle_top_right.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_elite_paddle_top_right`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_elite_paddle_top_right_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_elite_paddle_top_right_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_guide.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_guide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_guide_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_guide_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_lb.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_lb`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_lb_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_lb_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_ls.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_ls`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_ls_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_ls_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_lt.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_lt`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_lt_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_lt_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_rb.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_rb`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_rb_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_rb_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_rs.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_rs`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_rs_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_rs_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_rt.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_rt`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_rt_outline.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_rt_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_l.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_l`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_l_down.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_l_down`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_l_horizontal.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_l_horizontal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_l_left.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_l_left`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_l_press.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_l_press`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_l_right.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_l_right`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_l_up.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_l_up`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_l_vertical.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_l_vertical`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_r.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_r`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_r_down.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_r_down`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_r_horizontal.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_r_horizontal`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_r_left.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_r_left`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_r_press.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_r_press`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_r_right.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_r_right`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_r_up.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_r_up`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_r_vertical.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_r_vertical`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_side_l.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_side_l`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_side_r.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_side_r`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_top_l.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_top_l`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyInputPrompts/XboxSeries/xbox_stick_top_r.png`
- category: `UI`; target: `candidate_input_xbox_series_xbox_stick_top_r`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_basic_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_basic_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_basic_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_basic_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_basic_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_basic_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_basic_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_basic_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_decorative_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_decorative_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_decorative_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_decorative_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_decorative_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_decorative_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_decorative_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/arrow_decorative_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_rectangle_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_rectangle_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_rectangle_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_rectangle_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_rectangle_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_rectangle_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_rectangle_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_rectangle_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_round_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_round_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_round_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_round_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_round_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_round_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_round_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_round_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_square_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_square_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_square_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_square_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_square_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_square_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_square_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_square_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_round_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_round_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_round_grey_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_round_round_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_round_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_square_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_square_color_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_square_color_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_square_color_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_square_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_square_grey_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_square_grey_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/check_square_grey_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/icon_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/icon_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/icon_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/icon_outline_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/icon_outline_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/icon_outline_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/icon_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/icon_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_hangle.png`
- category: `UI`; target: `candidate_ui_kenney_slide_hangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_horizontal_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_horizontal_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_horizontal_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_horizontal_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_horizontal_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_horizontal_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_vertical_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_vertical_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_vertical_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_vertical_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_vertical_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/slide_vertical_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/star.png`
- category: `UI`; target: `candidate_ui_kenney_star`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/star_outline.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Default/star_outline_depth.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline_depth`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_basic_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_basic_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_basic_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_basic_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_basic_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_basic_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_basic_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_basic_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_decorative_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_decorative_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_decorative_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_decorative_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_decorative_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_decorative_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_decorative_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/arrow_decorative_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_rectangle_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_rectangle_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_rectangle_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_rectangle_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_rectangle_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_rectangle_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_rectangle_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_rectangle_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_round_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_round_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_round_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_round_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_round_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_round_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_round_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_round_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_square_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_square_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_square_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_square_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_square_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_square_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_square_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_square_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_round_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_round_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_round_grey_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_round_round_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_round_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_square_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_square_color_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_square_color_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_square_color_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_square_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_square_grey_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_square_grey_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/check_square_grey_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/icon_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/icon_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/icon_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/icon_outline_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/icon_outline_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/icon_outline_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/icon_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/icon_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_hangle.png`
- category: `UI`; target: `candidate_ui_kenney_slide_hangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_horizontal_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_horizontal_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_horizontal_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_horizontal_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_horizontal_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_horizontal_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_vertical_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_vertical_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_vertical_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_vertical_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_vertical_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/slide_vertical_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/star.png`
- category: `UI`; target: `candidate_ui_kenney_star`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/star_outline.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Blue/Double/star_outline_depth.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline_depth`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/divider.png`
- category: `UI`; target: `candidate_ui_kenney_divider`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/divider_edges.png`
- category: `UI`; target: `candidate_ui_kenney_divider_edges`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_arrow_down_dark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_down_dark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_arrow_down_light.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_down_light`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_arrow_down_outline.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_down_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_arrow_up_dark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_up_dark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_arrow_up_light.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_up_light`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_arrow_up_outline.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_up_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_play_dark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_play_dark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_play_light.png`
- category: `UI`; target: `candidate_ui_kenney_icon_play_light`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_play_outline.png`
- category: `UI`; target: `candidate_ui_kenney_icon_play_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_repeat_dark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_repeat_dark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_repeat_light.png`
- category: `UI`; target: `candidate_ui_kenney_icon_repeat_light`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/icon_repeat_outline.png`
- category: `UI`; target: `candidate_ui_kenney_icon_repeat_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/input_outline_rectangle.png`
- category: `UI`; target: `candidate_ui_kenney_input_outline_rectangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/input_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_input_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/input_rectangle.png`
- category: `UI`; target: `candidate_ui_kenney_input_rectangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Default/input_square.png`
- category: `UI`; target: `candidate_ui_kenney_input_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/divider.png`
- category: `UI`; target: `candidate_ui_kenney_divider`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/divider_edges.png`
- category: `UI`; target: `candidate_ui_kenney_divider_edges`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_arrow_down_dark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_down_dark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_arrow_down_light.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_down_light`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_arrow_down_outline.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_down_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_arrow_up_dark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_up_dark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_arrow_up_light.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_up_light`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_arrow_up_outline.png`
- category: `UI`; target: `candidate_ui_kenney_icon_arrow_up_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_play_dark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_play_dark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_play_light.png`
- category: `UI`; target: `candidate_ui_kenney_icon_play_light`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_play_outline.png`
- category: `UI`; target: `candidate_ui_kenney_icon_play_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_repeat_dark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_repeat_dark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_repeat_light.png`
- category: `UI`; target: `candidate_ui_kenney_icon_repeat_light`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/icon_repeat_outline.png`
- category: `UI`; target: `candidate_ui_kenney_icon_repeat_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/input_outline_rectangle.png`
- category: `UI`; target: `candidate_ui_kenney_input_outline_rectangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/input_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_input_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/input_rectangle.png`
- category: `UI`; target: `candidate_ui_kenney_input_rectangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Extra/Double/input_square.png`
- category: `UI`; target: `candidate_ui_kenney_input_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_basic_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_basic_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_basic_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_basic_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_basic_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_basic_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_basic_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_basic_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_decorative_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_decorative_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_decorative_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_decorative_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_decorative_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_decorative_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_decorative_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/arrow_decorative_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_rectangle_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_rectangle_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_rectangle_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_rectangle_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_rectangle_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_rectangle_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_rectangle_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_rectangle_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_round_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_round_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_round_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_round_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_round_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_round_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_round_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_round_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_square_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_square_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_square_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_square_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_square_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_square_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_square_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_square_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_round_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_round_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_round_grey_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_round_round_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_round_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_square_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_square_color_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_square_color_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_square_color_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_square_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_square_grey_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_square_grey_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/check_square_grey_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/icon_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/icon_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/icon_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/icon_outline_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/icon_outline_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/icon_outline_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/icon_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/icon_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_hangle.png`
- category: `UI`; target: `candidate_ui_kenney_slide_hangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_horizontal_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_horizontal_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_horizontal_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_horizontal_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_horizontal_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_horizontal_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_vertical_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_vertical_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_vertical_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_vertical_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_vertical_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/slide_vertical_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/star.png`
- category: `UI`; target: `candidate_ui_kenney_star`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/star_outline.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Default/star_outline_depth.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline_depth`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_basic_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_basic_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_basic_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_basic_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_basic_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_basic_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_basic_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_basic_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_decorative_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_decorative_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_decorative_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_decorative_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_decorative_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_decorative_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_decorative_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/arrow_decorative_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_rectangle_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_rectangle_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_rectangle_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_rectangle_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_rectangle_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_rectangle_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_rectangle_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_rectangle_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_round_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_round_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_round_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_round_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_round_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_round_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_round_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_round_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_square_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_square_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_square_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_square_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_square_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_square_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_square_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_square_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_round_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_round_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_round_grey_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_round_round_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_round_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_square_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_square_color_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_square_color_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_square_color_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_square_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_square_grey_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_square_grey_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/check_square_grey_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/icon_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/icon_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/icon_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/icon_outline_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/icon_outline_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/icon_outline_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/icon_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/icon_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_hangle.png`
- category: `UI`; target: `candidate_ui_kenney_slide_hangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_horizontal_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_horizontal_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_horizontal_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_horizontal_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_horizontal_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_horizontal_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_vertical_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_vertical_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_vertical_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_vertical_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_vertical_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/slide_vertical_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/star.png`
- category: `UI`; target: `candidate_ui_kenney_star`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/star_outline.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Green/Double/star_outline_depth.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline_depth`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_basic_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_basic_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_basic_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_basic_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_basic_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_basic_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_basic_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_basic_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_decorative_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_decorative_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_decorative_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_decorative_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_decorative_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_decorative_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_decorative_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/arrow_decorative_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_rectangle_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_rectangle_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_rectangle_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_rectangle_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_rectangle_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_rectangle_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_rectangle_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_rectangle_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_round_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_round_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_round_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_round_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_round_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_round_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_round_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_round_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_square_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_square_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_square_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_square_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_square_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_square_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_square_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_square_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_round_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_round_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_round_grey_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_round_round_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_round_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_square_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_square_color_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_square_color_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_square_color_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_square_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_square_grey_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_square_grey_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/check_square_grey_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/icon_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/icon_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/icon_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/icon_outline_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/icon_outline_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/icon_outline_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/icon_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/icon_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_hangle.png`
- category: `UI`; target: `candidate_ui_kenney_slide_hangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_horizontal_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_horizontal_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_horizontal_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_horizontal_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_horizontal_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_horizontal_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_vertical_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_vertical_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_vertical_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_vertical_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_vertical_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/slide_vertical_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/star.png`
- category: `UI`; target: `candidate_ui_kenney_star`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/star_outline.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Default/star_outline_depth.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline_depth`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_basic_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_basic_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_basic_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_basic_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_basic_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_basic_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_basic_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_basic_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_decorative_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_decorative_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_decorative_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_decorative_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_decorative_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_decorative_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_decorative_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/arrow_decorative_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_rectangle_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_rectangle_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_rectangle_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_rectangle_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_rectangle_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_rectangle_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_rectangle_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_rectangle_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_round_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_round_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_round_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_round_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_round_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_round_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_round_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_round_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_square_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_square_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_square_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_square_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_square_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_square_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_square_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_square_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_round_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_round_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_round_grey_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_round_round_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_round_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_square_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_square_color_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_square_color_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_square_color_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_square_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_square_grey_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_square_grey_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/check_square_grey_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/icon_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/icon_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/icon_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/icon_outline_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/icon_outline_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/icon_outline_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/icon_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/icon_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_hangle.png`
- category: `UI`; target: `candidate_ui_kenney_slide_hangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_horizontal_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_horizontal_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_horizontal_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_horizontal_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_horizontal_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_horizontal_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_vertical_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_vertical_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_vertical_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_vertical_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_vertical_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/slide_vertical_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/star.png`
- category: `UI`; target: `candidate_ui_kenney_star`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/star_outline.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Grey/Double/star_outline_depth.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline_depth`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_basic_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_basic_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_basic_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_basic_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_basic_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_basic_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_basic_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_basic_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_decorative_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_decorative_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_decorative_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_decorative_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_decorative_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_decorative_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_decorative_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/arrow_decorative_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_rectangle_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_rectangle_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_rectangle_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_rectangle_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_rectangle_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_rectangle_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_rectangle_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_rectangle_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_round_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_round_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_round_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_round_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_round_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_round_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_round_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_round_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_square_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_square_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_square_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_square_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_square_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_square_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_square_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_square_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_round_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_round_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_round_grey_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_round_round_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_round_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_square_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_square_color_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_square_color_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_square_color_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_square_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_square_grey_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_square_grey_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/check_square_grey_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/icon_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/icon_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/icon_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/icon_outline_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/icon_outline_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/icon_outline_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/icon_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/icon_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_hangle.png`
- category: `UI`; target: `candidate_ui_kenney_slide_hangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_horizontal_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_horizontal_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_horizontal_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_horizontal_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_horizontal_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_horizontal_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_vertical_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_vertical_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_vertical_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_vertical_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_vertical_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/slide_vertical_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/star.png`
- category: `UI`; target: `candidate_ui_kenney_star`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/star_outline.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Default/star_outline_depth.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline_depth`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_basic_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_basic_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_basic_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_basic_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_basic_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_basic_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_basic_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_basic_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_decorative_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_decorative_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_decorative_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_decorative_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_decorative_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_decorative_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_decorative_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/arrow_decorative_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_rectangle_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_rectangle_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_rectangle_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_rectangle_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_rectangle_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_rectangle_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_rectangle_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_rectangle_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_round_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_round_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_round_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_round_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_round_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_round_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_round_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_round_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_square_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_square_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_square_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_square_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_square_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_square_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_square_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_square_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_round_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_round_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_round_grey_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_round_round_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_round_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_square_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_square_color_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_square_color_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_square_color_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_square_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_square_grey_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_square_grey_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/check_square_grey_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/icon_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/icon_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/icon_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/icon_outline_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/icon_outline_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/icon_outline_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/icon_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/icon_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_hangle.png`
- category: `UI`; target: `candidate_ui_kenney_slide_hangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_horizontal_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_horizontal_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_horizontal_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_horizontal_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_horizontal_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_horizontal_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_vertical_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_vertical_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_vertical_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_vertical_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_vertical_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/slide_vertical_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/star.png`
- category: `UI`; target: `candidate_ui_kenney_star`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/star_outline.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Red/Double/star_outline_depth.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline_depth`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_basic_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_basic_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_basic_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_basic_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_basic_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_basic_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_basic_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_basic_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_decorative_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_decorative_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_decorative_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_decorative_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_decorative_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_decorative_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_decorative_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/arrow_decorative_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_rectangle_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_rectangle_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_rectangle_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_rectangle_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_rectangle_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_rectangle_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_rectangle_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_rectangle_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_round_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_round_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_round_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_round_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_round_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_round_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_round_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_round_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_square_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_square_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_square_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_square_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_square_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_square_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_square_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_square_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_round_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_round_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_round_grey_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_round_round_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_round_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_square_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_square_color_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_square_color_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_square_color_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_square_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_square_grey_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_square_grey_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/check_square_grey_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/icon_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/icon_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/icon_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/icon_outline_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/icon_outline_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/icon_outline_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/icon_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/icon_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_hangle.png`
- category: `UI`; target: `candidate_ui_kenney_slide_hangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_horizontal_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_horizontal_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_horizontal_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_horizontal_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_horizontal_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_horizontal_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_vertical_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_vertical_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_vertical_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_vertical_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_vertical_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/slide_vertical_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/star.png`
- category: `UI`; target: `candidate_ui_kenney_star`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/star_outline.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Default/star_outline_depth.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline_depth`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_basic_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_basic_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_basic_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_basic_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_basic_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_basic_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_basic_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_basic_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_basic_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_decorative_e.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_decorative_e_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_e_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_decorative_n.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_decorative_n_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_n_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_decorative_s.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_decorative_s_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_s_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_decorative_w.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/arrow_decorative_w_small.png`
- category: `UI`; target: `candidate_ui_kenney_arrow_decorative_w_small`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_rectangle_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_rectangle_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_rectangle_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_rectangle_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_rectangle_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_rectangle_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_rectangle_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_rectangle_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_rectangle_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_rectangle_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_rectangle_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_round_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_round_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_round_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_round_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_round_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_round_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_round_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_round_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_round_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_round_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_round_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_square_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_square_depth_border.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_border`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_square_depth_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_square_depth_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_square_depth_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_square_depth_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_depth_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_square_flat.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_flat`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_square_gloss.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gloss`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_square_gradient.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_gradient`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/button_square_line.png`
- category: `UI`; target: `candidate_ui_kenney_button_square_line`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_round_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_round_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_round_grey_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_grey_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_round_round_circle.png`
- category: `UI`; target: `candidate_ui_kenney_check_round_round_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_square_color.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_square_color_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_square_color_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_square_color_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_color_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_square_grey.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_square_grey_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_square_grey_cross.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/check_square_grey_square.png`
- category: `UI`; target: `candidate_ui_kenney_check_square_grey_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/icon_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/icon_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/icon_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/icon_outline_checkmark.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_checkmark`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/icon_outline_circle.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_circle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/icon_outline_cross.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_cross`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/icon_outline_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_outline_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/icon_square.png`
- category: `UI`; target: `candidate_ui_kenney_icon_square`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_hangle.png`
- category: `UI`; target: `candidate_ui_kenney_slide_hangle`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_horizontal_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_horizontal_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_horizontal_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_horizontal_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_horizontal_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_horizontal_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_horizontal_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_vertical_color.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_vertical_color_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_vertical_color_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_color_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_vertical_grey.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_vertical_grey_section.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/slide_vertical_grey_section_wide.png`
- category: `UI`; target: `candidate_ui_kenney_slide_vertical_grey_section_wide`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/star.png`
- category: `UI`; target: `candidate_ui_kenney_star`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/star_outline.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``
### `Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/Yellow/Double/star_outline_depth.png`
- category: `UI`; target: `candidate_ui_kenney_star_outline_depth`; importer: `TextureImporter`
- source_exists: `True`; mesh/animation/material: 0/0/0
- model_dimension: `0` Unity units; target scale: ``

## 边界

本报告只记录 Unity 当前导入结果和建议；不修改 Registry、asset_state、Prefab、场景或候选包，不写 ReleaseApproved，不下载资源。模型自身尺寸以记录为准，不由管线统一 scale；后续 Prefab/责任系统决定具体目标尺度、锚点和碰撞修正。
