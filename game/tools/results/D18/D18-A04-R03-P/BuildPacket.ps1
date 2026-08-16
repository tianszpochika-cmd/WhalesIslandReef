$ErrorActionPreference = 'Stop'
$root = 'C:\Users\Administrator\Desktop\game\first01'
$candidateRoot = Join-Path $root 'game\Assets\Art\Candidates\CharactersAnimals\GapFill\PlayerAppearance'
$resultRoot = Join-Path $root 'game\tools\results\D18\D18-A04-R03-P'
$importPath = Join-Path $resultRoot 'D18_A04_R03_P_ISOLATED_UNITY_IMPORT.json'
$import = Get-Content -LiteralPath $importPath -Raw | ConvertFrom-Json
$archiveDir = Join-Path $root 'artifacts\external_sources'
$uArchive = Join-Path $archiveDir 'D18-A04-R03-P_Quaternius_UniversalBaseCharacters_Standard.zip'
$mArchive = Join-Path $archiveDir 'D18-A04-R03-P_Quaternius_ModularCharacterOutfitsFantasy_Standard.zip'
$uArchiveHash = (Get-FileHash -LiteralPath $uArchive -Algorithm SHA256).Hash
$mArchiveHash = (Get-FileHash -LiteralPath $mArchive -Algorithm SHA256).Hash
$uFiles = Get-ChildItem -LiteralPath (Join-Path $candidateRoot 'QuaterniusUniversalBaseCharacters') -Recurse -File
$mFiles = Get-ChildItem -LiteralPath (Join-Path $candidateRoot 'QuaterniusModularCharacterOutfitsFantasy') -Recurse -File
$fbxFiles = Get-ChildItem -LiteralPath $candidateRoot -Recurse -File -Filter '*.fbx'
$modules = foreach ($item in $import.candidates) {
    $source = $fbxFiles | Where-Object { $_.Name -eq $item.file_name } | Select-Object -First 1
    if ($null -eq $source) { continue }
    $relative = $source.FullName.Substring($root.Length + 1).Replace('\', '/')
    $isBase = $item.file_name -eq 'Superhero_Female_FullBody.fbx'
    $isHair = $item.file_name -match '^Hair_'
    $isEyebrow = $item.file_name -eq 'Eyebrows_Female.fbx'
    $isFemale = $item.file_name -match '^Female_'
    $package = if ($source.FullName -match 'QuaterniusUniversalBaseCharacters') { 'pack_quaternius_universal_base_characters_standard_v01' } else { 'pack_quaternius_modular_character_outfits_fantasy_standard_v01' }
    $archiveEntry = if ($isBase) {
        'Universal Base Characters[Standard]/Base Characters/Unity/' + $item.file_name
    } elseif ($isHair -or $isEyebrow) {
        'Universal Base Characters[Standard]/Hairstyles/Rigged to Head Bone/FBX (Unity)/' + $item.file_name
    } elseif ($isFemale -or $item.file_name -match '^Male_') {
        if ($item.file_name -match '^(Female|Male)_(Peasant|Ranger)_(Arms|Body|Feet|Legs|Acc_|Head_)') {
            'Modular Character Outfits - Fantasy[Standard]/Exports/FBX (Unity)/Modular Parts/' + $item.file_name
        } else {
            'Modular Character Outfits - Fantasy[Standard]/Exports/FBX (Unity)/Outfits/' + $item.file_name
        }
    } else { 'unclassified' }
    $category = if ($isBase) { 'player_visual_core' } elseif ($isHair) { 'player_appearance_module.hair' } elseif ($isEyebrow) { 'player_appearance_module.facial_hair_support' } elseif ($isFemale) { 'player_appearance_module.female_clothing' } else { 'player_appearance_module.male_clothing_reference' }
    [ordered]@{
        content_id = ('candidate_' + [IO.Path]::GetFileNameWithoutExtension($item.file_name).ToLowerInvariant() + '_r03p')
        package_id = $package
        category = $category
        candidate_file_path = $relative
        source_archive_entry = $archiveEntry
        sha256 = (Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash
        file_size_bytes = $source.Length
        review_status = 'Candidate'
        import_status = 'ImportVerified_IsolatedFixture'
        unity_import_evidence_path = 'game/tools/results/D18/D18-A04-R03-P/D18_A04_R03_P_ISOLATED_UNITY_IMPORT.json'
        license_evidence_path = if ($package -like 'pack_quaternius_universal*') { 'game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusUniversalBaseCharacters/LICENSE_QUATERNIUS_CC0_STANDARD.txt' } else { 'game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusModularCharacterOutfitsFantasy/LICENSE_QUATERNIUS_CC0_STANDARD.txt' }
        file_exists = $item.file_exists
        mesh_count = $item.mesh_count
        material_count = $item.material_count
        texture_count_in_fbx = $item.texture_count
        renderer_height_m = $item.renderer_height
        renderer_width_m = $item.renderer_width
        renderer_depth_m = $item.renderer_depth
        global_scale = $item.global_scale
        animation_type = $item.animation_type
        avatar_setup = $item.avatar_setup
        avatar_present = $item.avatar_present
        avatar_valid = $item.avatar_valid
        avatar_is_human = $item.avatar_is_human
        formal_prefab = $false
        formal_scene_consumer = $false
        local_development_ready = $false
        release_approved = $false
        notes = @($item.notes + 'Candidate directory is isolated; no D20 Prefab, Animator, Registry or scene consumer was created.')
    }
}
$hairModules = @($modules | Where-Object { $_.category -eq 'player_appearance_module.hair' })
$hairStyleModules = @($hairModules | Where-Object { $_.candidate_file_path -notmatch 'Hair_Beard\.fbx$' })
$facialHairModules = @($hairModules | Where-Object { $_.candidate_file_path -match 'Hair_Beard\.fbx$' })
$femaleClothing = @($modules | Where-Object { $_.category -eq 'player_appearance_module.female_clothing' })
$packet = [ordered]@{
    schema_version = 1
    task = 'D18-A04-R03-P'
    generated_at = (Get-Date).ToUniversalTime().ToString('o')
    status = 'PARTIAL'
    conclusion_scope = 'isolated_candidate_source_and_import_evidence_only'
    source_packages = @(
        [ordered]@{ package_id = 'pack_quaternius_universal_base_characters_standard_v01'; title = 'Universal Base Characters'; official_page = 'https://quaternius.com/packs/universalbasecharacters.html'; download_page = 'https://quaternius.itch.io/universal-base-characters'; license_id = 'CC0-1.0'; license_url = 'https://creativecommons.org/publicdomain/zero/1.0/'; archive_name = 'D18-A04-R03-P_Quaternius_UniversalBaseCharacters_Standard.zip'; archive_path = 'artifacts/external_sources/D18-A04-R03-P_Quaternius_UniversalBaseCharacters_Standard.zip'; archive_sha256 = $uArchiveHash; archive_size_bytes = (Get-Item -LiteralPath $uArchive).Length; downloaded_at = '2026-08-15T05:04:45.6554860Z'; license_evidence_path = 'game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusUniversalBaseCharacters/LICENSE_QUATERNIUS_CC0_STANDARD.txt'; commercial_use = $true; modification_allowed = $true; source_redistribution = 'standard_archive_not_added_to_git; external cache only' }
        [ordered]@{ package_id = 'pack_quaternius_modular_character_outfits_fantasy_standard_v01'; title = 'Modular Character Outfits - Fantasy'; official_page = 'https://quaternius.com/packs/modularcharacteroutfitsfantasy.html'; download_page = 'https://quaternius.itch.io/modular-character-outfits-fantasy'; license_id = 'CC0-1.0'; license_url = 'https://creativecommons.org/publicdomain/zero/1.0/'; archive_name = 'D18-A04-R03-P_Quaternius_ModularCharacterOutfitsFantasy_Standard.zip'; archive_path = 'artifacts/external_sources/D18-A04-R03-P_Quaternius_ModularCharacterOutfitsFantasy_Standard.zip'; archive_sha256 = $mArchiveHash; archive_size_bytes = (Get-Item -LiteralPath $mArchive).Length; downloaded_at = '2026-08-15T05:06:05.1786878Z'; license_evidence_path = 'game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusModularCharacterOutfitsFantasy/LICENSE_QUATERNIUS_CC0_STANDARD.txt'; commercial_use = $true; modification_allowed = $true; source_redistribution = 'standard_archive_not_added_to_git; external cache only' }
    )
    actual_counts = [ordered]@{ candidate_fbx = $fbxFiles.Count; candidate_png = @($uFiles + $mFiles | Where-Object { $_.Extension -eq '.png' }).Count; candidate_license_files = @($uFiles + $mFiles | Where-Object { $_.Name -match 'LICENSE|README' }).Count; unity_import_fbx_total = $import.fbx_total; unity_mesh_success = $import.mesh_success_count; unity_renderer_success = $import.renderer_success_count; valid_humanoid_avatar = $import.valid_humanoid_avatar_count; independent_hair_related_geometry = $hairModules.Count; independent_hair_style_geometry = $hairStyleModules.Count; facial_hair_geometry = $facialHairModules.Count; female_clothing_modules = $femaleClothing.Count; female_full_outfits = @($femaleClothing | Where-Object { $_.content_id -match 'female_(peasant|ranger)_r03p' }).Count }
    modules = @($modules)
    compatibility = [ordered]@{ player_humanoid_candidate = 'Superhero_Female_FullBody.fbx is Human + valid Avatar in isolated Unity 6000.0.80f1 fixture; height 1.8537m requires D20 calibration against 1.80m D17 baseline'; hair_attachment = 'Hair FBX meshes imported as Generic/NoAvatar; head-bone attachment and clipping remain D20 work'; clothing_attachment = 'Female peasant/ranger meshes imported as Generic/NoAvatar; modular assembly and bone compatibility remain D20 work'; texture_binding = '47 PNG files are present in Candidate directory; isolated runner measured FBX embedded texture count only, so final material binding remains unverified'; lod_collision_anchor = 'Not created in R03-P; D20 Prefab task owns these checks' }
    gaps = @('hair_style_slot_06_not_verified_as_independent_geometry', 'eight era-appropriate 1675 Caribbean clothing combinations not verified; current pack is medieval/fantasy', 'female base is Superhero proportion and is 1.8537m in isolated bounds, requiring D17 scale calibration', 'no formal Prefab, Animator, runtime consumer, save consumer or scene integration', 'D18-S02-R02 must decide intake eligibility; this packet does not update global catalog')
    evidence_paths = [ordered]@{ unity_import_json = 'game/tools/results/D18/D18-A04-R03-P/D18_A04_R03_P_ISOLATED_UNITY_IMPORT.json'; unity_import_log = 'game/tools/results/D18/D18-A04-R03-P/D18_A04_R03_P_ISOLATED_UNITY_IMPORT.log'; candidate_root = 'game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance'; source_cache = 'artifacts/external_sources'; source_evidence = 'docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/D18_A04_R03_P_SOURCE_EVIDENCE.md' }
}
$packetJson = $packet | ConvertTo-Json -Depth 14
$utf8 = New-Object System.Text.UTF8Encoding($false)
$packetPath = Join-Path $root 'docs\game\design\common\assets\packs\characters_animals\gap_fill\player_appearance\D18_A04_R03_P_PLAYER_APPEARANCE_PACKET.json'
[IO.Directory]::CreateDirectory((Split-Path -Parent $packetPath)) | Out-Null
[IO.File]::WriteAllText($packetPath, $packetJson + "`n", $utf8)
$machine = [ordered]@{ task = 'D18-A04-R03-P'; status = 'PARTIAL'; generated_at = (Get-Date).ToUniversalTime().ToString('o'); archive_count = 2; candidate_fbx = $fbxFiles.Count; candidate_png = $packet.actual_counts.candidate_png; unity_import_total = $import.fbx_total; unity_mesh_success = $import.mesh_success_count; valid_humanoid_avatar = $import.valid_humanoid_avatar_count; independent_hair_related_geometry = $hairModules.Count; independent_hair_style_geometry = $hairStyleModules.Count; facial_hair_geometry = $facialHairModules.Count; female_clothing_modules = $femaleClothing.Count; female_full_outfits = $packet.actual_counts.female_full_outfits; local_development_ready = 0; release_approved = 0; source_candidate_files_changed = $false; packet_json = 'docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/D18_A04_R03_P_PLAYER_APPEARANCE_PACKET.json'; import_json = 'game/tools/results/D18/D18-A04-R03-P/D18_A04_R03_P_ISOLATED_UNITY_IMPORT.json'; notes = @('PARTIAL: source/license/hash/import evidence is complete for isolated candidates, but target coverage is incomplete', 'Only 5 true independent hairstyles plus 1 beard/eyebrow-related file were found; 1675 Caribbean clothing coverage is not proven', 'No formal D20 Prefab, scene, Registry, resource state or runtime consumer was written') }
[IO.File]::WriteAllText((Join-Path $resultRoot 'D18_A04_R03_P_MACHINE_RESULT.json'), ($machine | ConvertTo-Json -Depth 8) + "`n", $utf8)
