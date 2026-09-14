[CmdletBinding()]
param(
    [string]$PlayerPath = "game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN48.exe",
    [string]$RouteFlag = "-d19-r03-route-rerun48",
    [int]$WarmupSeconds = 15,
    [int]$CloseTimeoutSeconds = 8,
    [string]$ProbePath = "game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json",
    [string]$PlayerLogPath = "",
    [string]$SummaryPath = "game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_STARTUP_SMOKE_20260906.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$workspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$resolvedPlayer = (Resolve-Path (Join-Path $workspaceRoot $PlayerPath)).Path
$resolvedProbe = Join-Path $workspaceRoot $ProbePath

if (-not (Test-Path -LiteralPath $resolvedPlayer -PathType Leaf)) {
    throw "VC-G01 startup smoke Player 不存在：$resolvedPlayer"
}

if ([string]::IsNullOrWhiteSpace($PlayerLogPath)) {
    $PlayerLogPath = Join-Path $env:USERPROFILE "AppData/LocalLow/Whalebone Studio/WhalesIslandReef/Player.log"
}

$probeBefore = $null
if (Test-Path -LiteralPath $resolvedProbe -PathType Leaf) {
    $probeBefore = (Get-Item -LiteralPath $resolvedProbe).LastWriteTimeUtc
}

# 只拒绝同一已知 Player 路径的并发实例，避免关闭用户或其他项目的进程。
$running = @(Get-Process -ErrorAction SilentlyContinue | Where-Object {
        try {
            if ([string]::IsNullOrWhiteSpace($_.Path)) { return $false }
            return ((Resolve-Path -LiteralPath $_.Path -ErrorAction Stop).Path -ieq $resolvedPlayer)
        }
        catch {
            return $false
        }
    })
if ($running.Count -gt 0) {
    throw "VC-G01 startup smoke 拒绝并发运行：PID=$($running.Id -join ',')"
}

$workingDirectory = Split-Path -Parent $resolvedPlayer
$process = Start-Process -FilePath $resolvedPlayer -ArgumentList @($RouteFlag) -WorkingDirectory $workingDirectory -PassThru
$startedAt = Get-Date
$closeRequested = $false
$gracefulExit = $false
$forceStopped = $false
$hasExitedAfterWarmup = $false
$exitCode = $null

try {
    Write-Output "STARTED_PID=$($process.Id)"
    Start-Sleep -Seconds $WarmupSeconds
    $process.Refresh()
    $hasExitedAfterWarmup = $process.HasExited
    Write-Output "AFTER_WARMUP_HAS_EXITED=$hasExitedAfterWarmup"

    if (-not $process.HasExited) {
        $closeRequested = $process.CloseMainWindow()
        Write-Output "CLOSE_MAIN_WINDOW=$closeRequested"
        $gracefulExit = $process.WaitForExit($CloseTimeoutSeconds * 1000)
        Write-Output "GRACEFUL_EXIT=$gracefulExit"
    }

    $process.Refresh()
    if ($process.HasExited) {
        $exitCode = $process.ExitCode
    }
}
finally {
    $process.Refresh()
    if (-not $process.HasExited) {
        # 只处理本脚本刚启动且仍存活的同一个 PID，避免遗留 Player 阻塞后续复测。
        Stop-Process -Id $process.Id -Force
        $forceStopped = $true
        Write-Output "FORCE_STOPPED=TRUE"
    }
}

$probeExists = Test-Path -LiteralPath $resolvedProbe -PathType Leaf
$probeFresh = $false
$probe = $null
if ($probeExists) {
    $probeItem = Get-Item -LiteralPath $resolvedProbe
    $probeFresh = $null -eq $probeBefore -or $probeItem.LastWriteTimeUtc -gt $probeBefore
    $probe = Get-Content -LiteralPath $resolvedProbe -Raw | ConvertFrom-Json
}

$logExists = Test-Path -LiteralPath $PlayerLogPath -PathType Leaf
$logFresh = $false
$logHasStartupMarkers = $false
if ($logExists) {
    $logItem = Get-Item -LiteralPath $PlayerLogPath
    $logFresh = $logItem.LastWriteTime -ge $startedAt
    $logText = Get-Content -LiteralPath $PlayerLogPath -Raw
    $logHasStartupMarkers = $logText.Contains("[MainMenu] UI 初始化完成") -and
        $logText.Contains("[D19-R03-A01] Route probe component started")
}

$summary = [ordered]@{
    task_id = "VC-G01"
    smoke = "bounded_player_startup"
    status = "STARTUP_OBSERVED_ONLY"
    route_flag = $RouteFlag
    player_path = $resolvedPlayer
    started_at_local = $startedAt.ToString("o")
    warmup_seconds = $WarmupSeconds
    close_timeout_seconds = $CloseTimeoutSeconds
    has_exited_after_warmup = $hasExitedAfterWarmup
    close_main_window_requested = $closeRequested
    graceful_exit = $gracefulExit
    force_stopped = $forceStopped
    exit_code = $exitCode
    probe_path = $resolvedProbe
    probe_exists = $probeExists
    probe_fresh = $probeFresh
    probe_scene = if ($null -eq $probe) { "" } else { [string]$probe.scene }
    probe_player_found = if ($null -eq $probe) { $false } else { [bool]$probe.player_found }
    probe_observations = if ($null -eq $probe) { 0 } else { @($probe.observations).Count }
    probe_events = if ($null -eq $probe) { 0 } else { @($probe.events).Count }
    player_log_path = $PlayerLogPath
    player_log_exists = $logExists
    player_log_fresh = $logFresh
    player_log_has_startup_markers = $logHasStartupMarkers
    runtime_route_proof = $false
    note = "启动/退出可观察；未发送真实键盘输入，不能证明 InitialHarbor 路线或交互通过。"
}

$summaryJson = $summary | ConvertTo-Json -Depth 8
$resolvedSummary = Join-Path $workspaceRoot $SummaryPath
$summaryDirectory = Split-Path -Parent $resolvedSummary
if (-not [string]::IsNullOrWhiteSpace($summaryDirectory)) {
    [System.IO.Directory]::CreateDirectory($summaryDirectory) | Out-Null
}
Set-Content -LiteralPath $resolvedSummary -Value $summaryJson -Encoding UTF8
Write-Output $summaryJson

if (-not $probeExists -or -not $probeFresh -or -not $logExists -or -not $logHasStartupMarkers) {
    exit 2
}
