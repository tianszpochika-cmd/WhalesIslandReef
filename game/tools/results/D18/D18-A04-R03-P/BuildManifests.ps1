$ErrorActionPreference = 'Stop'
$root = 'C:\Users\Administrator\Desktop\game\first01'
$packetPath = Join-Path $root 'docs\game\design\common\assets\packs\characters_animals\gap_fill\player_appearance\D18_A04_R03_P_PLAYER_APPEARANCE_PACKET.json'
$packet = Get-Content -LiteralPath $packetPath -Raw | ConvertFrom-Json
$manifestRoot = Join-Path $root 'docs\game\design\common\assets\packs\characters_animals\gap_fill\player_appearance'
$utf8 = New-Object System.Text.UTF8Encoding($false)

foreach ($package in $packet.source_packages) {
    $items = @($packet.modules | Where-Object { $_.package_id -eq $package.package_id })
    if ($items.Count -eq 0) { continue }
    $candidateFiles = foreach ($item in $items) {
        [ordered]@{
            path = $item.candidate_file_path
            role = $item.category
            target_id = $item.content_id
        }
    }
    $manifest = [ordered]@{
        manifest_kind = 'd18_asset_pack_manifest'
        schema_version = 1
        source_id = 'source_quaternius'
        pack_id = $package.package_id
        title = $package.title
        author = 'Quaternius'
        official_page = $package.official_page
        download_url = $package.download_page
        downloaded_at = $package.downloaded_at.Substring(0, 10)
        archive_name = $package.archive_name
        sha256 = $package.archive_sha256
        license_id = $package.license_id
        license_url = $package.license_url
        license_evidence_path = $package.license_evidence_path
        attribution_required = $false
        commercial_use = $package.commercial_use
        modification_allowed = $package.modification_allowed
        source_redistribution = 'allowed'
        build_distribution = 'allowed'
        category = @('character', 'player_appearance', 'animation_support')
        file_count = $items.Count
        unity_object_count = (@($items | Measure-Object -Property mesh_count -Sum).Sum)
        review_status = 'Candidate'
        import_status = 'ImportVerified'
        candidate_files = @($candidateFiles)
        runtime_consumers = @()
        notes = 'R03-P source manifest. Candidate files have local SHA-256 and isolated Unity import evidence. No formal Prefab, Animator consumer, scene, Registry, LocalDevelopmentReady or ReleaseApproved state is asserted. Package target gaps remain recorded in the review packet.'
    }
    $name = $package.package_id + '.manifest.json'
    $path = Join-Path $manifestRoot $name
    [IO.File]::WriteAllText($path, (($manifest | ConvertTo-Json -Depth 12) + "`n"), $utf8)
}
