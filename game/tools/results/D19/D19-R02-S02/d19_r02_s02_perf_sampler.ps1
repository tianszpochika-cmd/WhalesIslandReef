param(
    [Parameter(Mandatory = $true)][int]$TargetPid,
    [Parameter(Mandatory = $true)][string]$Quality,
    [Parameter(Mandatory = $true)][string]$OutputPath,
    [int]$WarmupSec = 30,
    [int]$SampleSec = 60
)

$startedAt = Get-Date
$warmupEndsAt = $startedAt.AddSeconds($WarmupSec)
Start-Sleep -Seconds $WarmupSec

$samples = New-Object System.Collections.Generic.List[object]
$sampleStartedAt = Get-Date
for ($i = 0; $i -lt $SampleSec; $i++) {
    $sampleAt = Get-Date
    $process = Get-Process -Id $TargetPid -ErrorAction SilentlyContinue
    $gpuRaw = $null
    $gpuError = $null
    try {
        $gpuRaw = (& nvidia-smi.exe --query-gpu=timestamp,utilization.gpu,utilization.memory,memory.used,memory.total,power.draw --format=csv,noheader,nounits 2>&1 | Out-String).Trim()
    } catch {
        $gpuError = $_.Exception.Message
    }

    $samples.Add([ordered]@{
        index = $i
        timestamp = $sampleAt.ToString('o')
        process_alive = ($null -ne $process)
        cpu_seconds = if ($null -ne $process) { [double]$process.CPU } else { $null }
        working_set_bytes = if ($null -ne $process) { [long]$process.WorkingSet64 } else { $null }
        private_memory_bytes = if ($null -ne $process) { [long]$process.PrivateMemorySize64 } else { $null }
        handle_count = if ($null -ne $process) { [int]$process.HandleCount } else { $null }
        thread_count = if ($null -ne $process) { [int]$process.Threads.Count } else { $null }
        gpu_raw = $gpuRaw
        gpu_error = $gpuError
    })

    if ($i -lt ($SampleSec - 1)) {
        Start-Sleep -Seconds 1
    }
}

$finishedAt = Get-Date
$payload = [ordered]@{
    schema = 'D19_R02_S02_PERF_PROCESS_SAMPLE_V1'
    quality_requested = $Quality
    target_pid = $TargetPid
    warmup_seconds = $WarmupSec
    sample_seconds = $SampleSec
    started_at = $startedAt.ToString('o')
    warmup_ended_at = $warmupEndsAt.ToString('o')
    sample_started_at = $sampleStartedAt.ToString('o')
    finished_at = $finishedAt.ToString('o')
    actual_resolution_from_player = 'not emitted by current runtime'
    fps = $null
    one_percent_low = $null
    p95_frame_time_ms = $null
    cpu_frame_time_ms = $null
    gpu_frame_time_ms = $null
    gc_alloc_bytes = $null
    notes = @(
        'Real standalone Player process was sampled after the requested warm-up.',
        'This process sampler does not infer FPS or frame timing from CPU/GPU utilization.',
        'Missing frame metrics remain unavailable; no PASS is inferred from process counters.'
    )
    samples = $samples.ToArray()
}

$parent = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force $parent | Out-Null
$payload | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 $OutputPath
