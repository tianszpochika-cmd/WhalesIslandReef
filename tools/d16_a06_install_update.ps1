[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("Install", "Update", "Uninstall")]
    [string]$Operation,

    [Parameter(Mandatory = $true)]
    [string]$BuildDir,

    [Parameter(Mandatory = $true)]
    [string]$InstallDir,

    [Parameter(Mandatory = $true)]
    [string]$UserDataDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$workspaceRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $workspaceRoot "artifacts"))

function Resolve-InputPath {
    param([Parameter(Mandatory = $true)][string]$PathValue)
    if ([IO.Path]::IsPathRooted($PathValue)) {
        return [IO.Path]::GetFullPath($PathValue)
    }
    return [IO.Path]::GetFullPath((Join-Path (Get-Location).Path $PathValue))
}

function Assert-ArtifactPath {
    param([Parameter(Mandatory = $true)][string]$PathValue)
    $fullPath = Resolve-InputPath $PathValue
    $rootWithSeparator = $artifactRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($rootWithSeparator, [StringComparison]::OrdinalIgnoreCase)) {
        throw "只允许操作 workspace artifacts 目录内的路径: $fullPath"
    }
    return $fullPath
}

function Copy-BuildPayload {
    param(
        [Parameter(Mandatory = $true)][string]$SourceDir,
        [Parameter(Mandatory = $true)][string]$TargetDir
    )

    if (-not (Test-Path -LiteralPath (Join-Path $SourceDir "WhalesIslandReef.exe") -PathType Leaf)) {
        throw "构建目录缺少 WhalesIslandReef.exe: $SourceDir"
    }

    if (Test-Path -LiteralPath $TargetDir) {
        Remove-Item -LiteralPath $TargetDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
    Get-ChildItem -LiteralPath $SourceDir -Force | Copy-Item -Destination $TargetDir -Recurse -Force
}

function Remove-ScopedDirectory {
    param([Parameter(Mandatory = $true)][string]$PathValue)
    if (Test-Path -LiteralPath $PathValue) {
        Remove-Item -LiteralPath $PathValue -Recurse -Force
    }
}

function Restore-PreviousInstall {
    param(
        [Parameter(Mandatory = $true)][string]$InstallTarget,
        [Parameter(Mandatory = $true)][string]$PreviousTarget
    )

    if (Test-Path -LiteralPath $InstallTarget) {
        Remove-ScopedDirectory $InstallTarget
    }
    if (Test-Path -LiteralPath $PreviousTarget) {
        Move-Item -LiteralPath $PreviousTarget -Destination $InstallTarget
    }
}

$buildPath = Assert-ArtifactPath $BuildDir
$installPath = Assert-ArtifactPath $InstallDir
$userDataPath = Assert-ArtifactPath $UserDataDir
$stagePath = Assert-ArtifactPath (($installPath + ".staging"))
$previousPath = Assert-ArtifactPath (($installPath + ".previous"))
$separator = [IO.Path]::DirectorySeparatorChar
$installPrefix = $installPath.TrimEnd($separator) + $separator
$buildPrefix = $buildPath.TrimEnd($separator) + $separator
if ($buildPath.Equals($installPath, [StringComparison]::OrdinalIgnoreCase) -or
    $installPath.StartsWith($buildPrefix, [StringComparison]::OrdinalIgnoreCase) -or
    $buildPath.StartsWith($installPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "构建目录和安装目录必须互斥，拒绝覆盖构建产物"
}
if ($userDataPath.Equals($installPath, [StringComparison]::OrdinalIgnoreCase) -or
    $userDataPath.StartsWith($installPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "存档目录必须独立于安装目录，卸载时才能保留用户数据"
}

if ($Operation -eq "Install" -or $Operation -eq "Update") {
    if (-not (Test-Path -LiteralPath $buildPath -PathType Container)) {
        throw "构建目录不存在: $buildPath"
    }
    if ($Operation -eq "Update" -and -not (Test-Path -LiteralPath $installPath -PathType Container)) {
        throw "更新目标不存在，拒绝把更新当作首次安装: $installPath"
    }
    if ($Operation -eq "Install" -and (Test-Path -LiteralPath $installPath)) {
        throw "安装目标已存在，请使用 Update，拒绝覆盖现有安装: $installPath"
    }

    Remove-ScopedDirectory $stagePath
    Remove-ScopedDirectory $previousPath
    Copy-BuildPayload -SourceDir $buildPath -TargetDir $stagePath

    $oldInstallMoved = $false
    try {
        if (Test-Path -LiteralPath $installPath) {
            Move-Item -LiteralPath $installPath -Destination $previousPath
            $oldInstallMoved = $true
        }
        Move-Item -LiteralPath $stagePath -Destination $installPath
        if ($oldInstallMoved) {
            Remove-ScopedDirectory $previousPath
        }
    }
    catch {
        # 更新失败时恢复旧版本；安装失败时只清理半成品。
        if ($oldInstallMoved -and (Test-Path -LiteralPath $previousPath)) {
            Restore-PreviousInstall -InstallTarget $installPath -PreviousTarget $previousPath
        }
        Remove-ScopedDirectory $stagePath
        throw
    }
    New-Item -ItemType Directory -Path $userDataPath -Force | Out-Null
}
elseif ($Operation -eq "Uninstall") {
    if (Test-Path -LiteralPath $stagePath) {
        Remove-Item -LiteralPath $stagePath -Recurse -Force
    }
    if (Test-Path -LiteralPath $previousPath) {
        Remove-Item -LiteralPath $previousPath -Recurse -Force
    }
    if (Test-Path -LiteralPath $installPath) {
        Remove-Item -LiteralPath $installPath -Recurse -Force
    }
}

[pscustomobject]@{
    operation = $Operation
    status = "PASS"
    build_dir = $buildPath
    install_dir = $installPath
    user_data_dir = $userDataPath
    install_exists = Test-Path -LiteralPath $installPath -PathType Container
    user_data_preserved = Test-Path -LiteralPath $userDataPath -PathType Container
    scope = "workspace-artifacts-only"
    external_publish_performed = $false
} | ConvertTo-Json -Compress
