$ErrorActionPreference = 'Stop'
$repo = (Get-Item $PSScriptRoot).Parent.Parent.Parent.Parent.Parent.FullName
$catalog = Get-Content (Join-Path $PSScriptRoot 'D18_A03_CANDIDATE_CATALOG.json') -Raw | ConvertFrom-Json
$source = Join-Path $repo 'artifacts/d18_a03_work/kenney_nature-kit/Side'
$output = Join-Path $PSScriptRoot 'kenney_nature_kit_contact_sheet.png'
$sample = @($catalog.import_verified_sample_paths | ForEach-Object { [IO.Path]::GetFileNameWithoutExtension($_) })
$images = @($sample | ForEach-Object { Join-Path $source ($_ + '.png') } | Where-Object { Test-Path -LiteralPath $_ })
if ($images.Count -lt 50) { throw "Contact sheet requires 50 source previews; found $($images.Count)" }

Add-Type -AssemblyName System.Drawing
$columns = 5
$cellWidth = 220
$cellHeight = 150
$rows = [int][Math]::Ceiling([double]$images.Count / [double]$columns)
$bitmapWidth = [int]$columns * [int]$cellWidth
$bitmapHeight = [int]$rows * [int]$cellHeight
$bitmap = New-Object Drawing.Bitmap ([int]$bitmapWidth, [int]$bitmapHeight)
$graphics = [Drawing.Graphics]::FromImage($bitmap)
$graphics.Clear([Drawing.Color]::White)
$font = New-Object Drawing.Font('Arial', 8)
$brush = New-Object Drawing.SolidBrush([Drawing.Color]::Black)
try {
    for ($index = 0; $index -lt $images.Count; $index++) {
        $image = [Drawing.Image]::FromFile($images[$index])
        try {
            $x = ($index % $columns) * $cellWidth
            $y = [Math]::Floor($index / $columns) * $cellHeight
            $scale = [Math]::Min(($cellWidth - 12) / $image.Width, ($cellHeight - 28) / $image.Height)
            $width = [Math]::Max(1, [int]($image.Width * $scale))
            $height = [Math]::Max(1, [int]($image.Height * $scale))
            $left = $x + [int](($cellWidth - $width) / 2)
            $top = $y + 4
            $graphics.DrawImage($image, $left, $top, $width, $height)
            $label = [IO.Path]::GetFileNameWithoutExtension($images[$index])
            $graphics.DrawString($label, $font, $brush, $x + 4, $y + $cellHeight - 20)
        } finally {
            $image.Dispose()
        }
    }
    $bitmap.Save($output, [Drawing.Imaging.ImageFormat]::Png)
} finally {
    $brush.Dispose()
    $font.Dispose()
    $graphics.Dispose()
    $bitmap.Dispose()
}

$result = [ordered]@{
    task = 'D18-A03'
    status = 'PASS'
    source = 'artifacts/d18_a03_work/kenney-nature-kit/Side'
    output = 'game/tools/results/D18/D18-A03/kenney_nature_kit_contact_sheet.png'
    sample_count = $images.Count
    columns = $columns
    rows = $rows
}
$json = $result | ConvertTo-Json -Depth 4
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'D18_A03_CONTACT_SHEET.json'), $json + [Environment]::NewLine, (New-Object Text.UTF8Encoding($false)))
Write-Output ($result | ConvertTo-Json -Compress)
