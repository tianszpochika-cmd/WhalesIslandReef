param(
    [string]$OutputPath = "tools/results/d16_a03_device_probe_current.json",
    [string]$RawPath = "tools/results/d16_a03_device_probe_raw_current.json"
)

$ErrorActionPreference = "Stop"

function Resolve-ProjectPath([string]$Path) {
    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }
    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Path)
    )
}

function Get-SafeDeviceId([string]$ComputerName, [string]$ControllerName) {
    $base = "$ComputerName|$ControllerName"
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($base)
        return (($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString("x2") }) -join "").Substring(0, 16)
    }
    finally {
        $sha.Dispose()
    }
}

$outputFile = Resolve-ProjectPath $OutputPath
$rawFile = Resolve-ProjectPath $RawPath
$outputDirectory = Split-Path -Parent $outputFile
$rawDirectory = Split-Path -Parent $rawFile
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $rawDirectory -Force | Out-Null

$computer = Get-CimInstance Win32_ComputerSystem
$os = Get-CimInstance Win32_OperatingSystem
$gpu = @(Get-CimInstance Win32_VideoController | ForEach-Object {
        [ordered]@{
            name = $_.Name
            driver_version = $_.DriverVersion
            current_width = $_.CurrentHorizontalResolution
            current_height = $_.CurrentVerticalResolution
            adapter_ram_bytes = $_.AdapterRAM
        }
    })

$presentDevices = @(Get-PnpDevice -PresentOnly | Where-Object {
        $_.Status -eq "OK" -and (
            $_.FriendlyName -match "Xbox|Gamepad|DualSense|DualShock|Joystick|XInput" -or
            ($_.Class -match "GameController" -and $_.FriendlyName)
        )
    } | ForEach-Object {
        [pscustomobject]@{
            name = $_.FriendlyName
            class = $_.Class
            status = $_.Status
        }
    } | Sort-Object name -Unique)

$keyboardDevices = @(Get-PnpDevice -PresentOnly | Where-Object {
        $_.Status -eq "OK" -and $_.FriendlyName -match "Keyboard"
    } | Select-Object -ExpandProperty FriendlyName -Unique)
$mouseDevices = @(Get-PnpDevice -PresentOnly | Where-Object {
        $_.Status -eq "OK" -and $_.FriendlyName -match "Mouse|Pointing"
    } | Select-Object -ExpandProperty FriendlyName -Unique)

$xinputConnectedUsers = @()
$xinputResults = @()
$xinputProbeStatus = "NOT_RUN"
$xinputApiAvailable = $false
$xinputProbeError = ""
try {
    $xinputProbeStatus = "RUNNING"
    if (-not ("D16XInputProbe" -as [type])) {
        Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class D16XInputProbe
{
    [StructLayout(LayoutKind.Sequential)]
    public struct XINPUT_GAMEPAD
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XINPUT_STATE
    {
        public uint dwPacketNumber;
        public XINPUT_GAMEPAD Gamepad;
    }

    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
    public static extern uint XInputGetState(uint dwUserIndex, out XINPUT_STATE state);
}
"@
    }

    $xinputApiAvailable = $true

    for ($userIndex = 0; $userIndex -lt 4; $userIndex++) {
        $state = New-Object D16XInputProbe+XINPUT_STATE
        $result = [D16XInputProbe]::XInputGetState([uint32]$userIndex, [ref]$state)
        $xinputResults += [pscustomobject]@{
            user_index = $userIndex
            result_code = $result
        }
        if ($result -eq 0) {
            $xinputConnectedUsers += [pscustomobject]@{
                user_index = $userIndex
                packet_number = $state.dwPacketNumber
            }
        }
    }

    $xinputProbeStatus = if ($xinputConnectedUsers.Count -gt 0) {
        "CONNECTED"
    }
    else {
        "NO_ACTIVE_XINPUT_USER"
    }
}
catch {
    $xinputConnectedUsers = @()
    $xinputResults = @()
    $xinputProbeStatus = "PROBE_ERROR"
    $xinputProbeError = $_.Exception.Message
}

$controllerNames = @($presentDevices | ForEach-Object { $_.name })
$deviceLabel = if ($controllerNames.Count -gt 0) {
    $controllerNames[0]
}
else {
    "no-controller-detected"
}

$raw = [ordered]@{
    captured_at = [DateTime]::UtcNow.ToString("o")
    computer_name = $env:COMPUTERNAME
    operating_system = $os.Caption
    os_version = $os.Version
    cpu = $computer.NumberOfLogicalProcessors
    total_memory_mb = [Math]::Round($computer.TotalPhysicalMemory / 1MB, 0)
    gpu = $gpu
    controllers = $presentDevices
    keyboards = $keyboardDevices
    mice = $mouseDevices
    xinput_probe_status = $xinputProbeStatus
    xinput_api_available = $xinputApiAvailable
    xinput_results = $xinputResults
    xinput_connected_users = $xinputConnectedUsers
    xinput_probe_error = $xinputProbeError
    note = "设备发现预检；没有发送按钮输入，也没有执行人工可访问性结论。"
}
$rawJson = $raw | ConvertTo-Json -Depth 8
Set-Content -LiteralPath $rawFile -Value ($rawJson + [Environment]::NewLine) -Encoding UTF8

$probe = [ordered]@{
    task = "D16-A03"
    status = "IN_PROGRESS"
    evidence_kind = "physical_device_probe"
    operator = "Codex preflight"
    device_id = "physical-windows-$($env:COMPUTERNAME)-$(Get-SafeDeviceId $env:COMPUTERNAME $deviceLabel)"
    captured_at = $raw.captured_at
    physical_input_pass = "PENDING_MANUAL"
    focus_pass = "PENDING_MANUAL"
    accessibility_manual_pass = "PENDING_MANUAL"
    resolutions = @($gpu | Where-Object { $_.current_width -and $_.current_height } | ForEach-Object {
            "$($_.current_width)x$($_.current_height)"
        } | Select-Object -Unique)
    input_devices = @($controllerNames + ($xinputConnectedUsers | ForEach-Object { "XInputUser$($_.user_index)" }) + $keyboardDevices + $mouseDevices | Where-Object { $_ } | Select-Object -Unique)
    xinput_probe_status = $xinputProbeStatus
    raw_artifacts = @($RawPath)
    notes = @(
        "当前电脑发现物理输入设备；尚未执行人工按键、焦点、字体、色觉或低刺激验收。",
        "该文件是预检记录，不是 D16-A03 PASS 证据。"
    )
}
$probeJson = $probe | ConvertTo-Json -Depth 8
Set-Content -LiteralPath $outputFile -Value ($probeJson + [Environment]::NewLine) -Encoding UTF8
Write-Output $probeJson
