param(
    [string]$PlayerPath = 'C:\Users\Administrator\AppData\Local\Temp\whales-d19-r01-b-20260815\Player\WhalesIslandReef_D19_R01_S02.exe',
    [string]$ResultRoot = 'C:\Users\Administrator\Desktop\game\first01\game\tools\results\D19\D19-R01-S02'
)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class D19RouteWindow {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int command);
}
'@

$shotDir = Join-Path $ResultRoot 'screenshots'
New-Item -ItemType Directory -Force -Path $shotDir | Out-Null
$routeLog = Join-Path $ResultRoot 'D19_R01_S02_ROUTE_LOG.json'
$playerLog = Join-Path $ResultRoot 'route_player.log'
$arg = '-d19RouteShots -d19RouteShotDir "' + $shotDir + '" -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -popupwindow -logFile "' + $playerLog + '"'
$startedAt = Get-Date
$process = Start-Process -FilePath $PlayerPath -ArgumentList $arg -PassThru
$steps = New-Object System.Collections.Generic.List[object]

function Focus-Player {
    $current = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
    if ($null -eq $current) { return $false }
    [D19RouteWindow]::ShowWindow($current.MainWindowHandle, 9) | Out-Null
    [D19RouteWindow]::SetForegroundWindow($current.MainWindowHandle) | Out-Null
    Start-Sleep -Milliseconds 300
    return $true
}

function Send-PlayerKeys([string]$keys, [string]$label, [string]$purpose) {
    $focused = Focus-Player
    if ($focused) { [System.Windows.Forms.SendKeys]::SendWait($keys) }
    $steps.Add([pscustomobject]@{
        time_local = (Get-Date).ToString('o')
        input = $keys
        label = $label
        purpose = $purpose
        focus = $focused
        player_pid = $process.Id
    })
    Start-Sleep -Milliseconds 200
}

function Send-Repeated([string]$key, [int]$count, [string]$label, [string]$purpose) {
    for ($i = 0; $i -lt $count; $i++) {
        Send-PlayerKeys $key ($label + '_' + ($i + 1)) $purpose
        Start-Sleep -Milliseconds 100
    }
}

Start-Sleep -Seconds 8
Send-PlayerKeys '{ENTER}' 'main_menu_new_game' 'open New Game through player UI navigation'
Start-Sleep -Seconds 3
Send-PlayerKeys 'R01S02' 'captain_name' 'type captain name into the real InputField'
Send-PlayerKeys '{TAB}{ENTER}' 'new_game_confirm' 'confirm new game through player UI navigation'
Start-Sleep -Seconds 20
Send-Repeated 'w' 18 'move_to_port' 'walk from spawn toward the port route'
Send-Repeated 'a' 4 'align_market' 'align toward market/main street'
Send-PlayerKeys 'e' 'npc_interaction_attempt' 'attempt key NPC interaction; record visible response or failure'
Start-Sleep -Seconds 20
Send-PlayerKeys 'p' 'open_port_preparation' 'open D9 port preparation page through player input'
Start-Sleep -Seconds 2
Send-PlayerKeys 'm' 'market_purchase' 'attempt a market supply action through D9 UI input'
Start-Sleep -Seconds 10
Send-PlayerKeys '{ESC}' 'close_port_preparation' 'close D9 preparation page'
Send-PlayerKeys 'p' 'reopen_port_preparation' 'reopen D9 page before departure'
Start-Sleep -Seconds 2
Send-PlayerKeys 't' 'tavern_action' 'attempt tavern action through D9 input'
Start-Sleep -Seconds 10
Send-PlayerKeys 'd' 'departure_action' 'attempt departure through D9 input'
Start-Sleep -Seconds 10
Send-PlayerKeys 'e' 'boarding_attempt' 'attempt boarding/toggle through real interact input'
Send-Repeated 'w' 18 'ship_motion_attempt' 'attempt ship forward motion after boarding'
Send-PlayerKeys 'e' 'disembark_attempt' 'attempt disembark/toggle through real interact input'
Send-PlayerKeys '{F5}' 'save_checkpoint' 'save through the existing player save binding'
Start-Sleep -Seconds 25
Send-PlayerKeys 'n' 'voyage_advance_attempt' 'attempt voyage advance through D3 input'
Send-PlayerKeys 'l' 'return_settle_attempt' 'attempt return/land and settle through D3 input'
Start-Sleep -Seconds 20
Send-PlayerKeys 'p' 'berth_page' 'open port preparation for berth/return verification'
Start-Sleep -Seconds 2
Send-PlayerKeys 'b' 'berth_entry_attempt' 'attempt berth entry through real D9 input'
Send-PlayerKeys '{ESC}' 'close_final_page' 'close port page after return attempt'
Send-PlayerKeys '{F5}' 'final_save_checkpoint' 'repeat save after return/berth attempt'

# Keep the Player open until the 14th internal 1920x1080 route frame is emitted.
Start-Sleep -Seconds 80
$endedAt = Get-Date
[pscustomobject]@{
    task = 'D19-R01-S02'
    capture_kind = 'standalone_player_real_keyboard_route'
    player_path = $PlayerPath
    player_pid = $process.Id
    start_local = $startedAt.ToString('o')
    driver_end_local = $endedAt.ToString('o')
    driver_duration_seconds = ($endedAt - $startedAt).TotalSeconds
    required_continuous_route_seconds = 300
    keys_sent_by_driver = $steps.Count
    steps = @($steps)
    screenshot_directory = $shotDir
    screenshot_capture_owner = 'D19R01S02RuntimeSampler via ScreenCapture.CaptureScreenshot inside standalone Player'
    note = 'Input was sent to the real standalone Player window. This log records input, not success; each outcome requires screenshot/log review.'
} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $routeLog -Encoding UTF8
