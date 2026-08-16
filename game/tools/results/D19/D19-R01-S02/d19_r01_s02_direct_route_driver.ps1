param(
    [string]$PlayerPath = 'C:\Users\Administrator\AppData\Local\Temp\whales-d19-r01-b-20260815\Player\WhalesIslandReef_D19_R01_S02.exe',
    [string]$ResultRoot = 'C:\Users\Administrator\Desktop\game\first01\game\tools\results\D19\D19-R01-S02'
)

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class D19DirectRouteKeys {
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int command);
 [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
 public static bool Press(IntPtr hWnd, byte vk){ if(hWnd==IntPtr.Zero)return false; ShowWindow(hWnd,9); SetForegroundWindow(hWnd); keybd_event(vk,0,0,UIntPtr.Zero); keybd_event(vk,0,2,UIntPtr.Zero); return true; }
}
'@

$shotDir = Join-Path $ResultRoot 'screenshots'
New-Item -ItemType Directory -Force -Path $shotDir | Out-Null
$routeLog = Join-Path $ResultRoot 'D19_R01_S02_DIRECT_ROUTE_LOG.json'
$playerLog = Join-Path $ResultRoot 'direct_route_player.log'
$arg = '-d19RouteShots -d19DirectInitialHarbor -d19RouteShotDir "' + $shotDir + '" -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -popupwindow -logFile "' + $playerLog + '"'
$startedAt = Get-Date
$process = Start-Process -FilePath $PlayerPath -ArgumentList $arg -PassThru
$steps = New-Object System.Collections.Generic.List[object]

function Press-Key([byte]$vk, [string]$label, [string]$purpose) {
    $current = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
    $focused = $false
    if ($null -ne $current) { $focused = [D19DirectRouteKeys]::Press($current.MainWindowHandle, $vk) }
    $steps.Add([pscustomobject]@{time_local=(Get-Date).ToString('o'); key_code=$vk; label=$label; purpose=$purpose; focus=$focused; player_pid=$process.Id})
    Start-Sleep -Milliseconds 180
}

function Repeat-Key([byte]$vk, [int]$count, [string]$label, [string]$purpose) {
    for ($i=0; $i -lt $count; $i++) { Press-Key $vk ($label + '_' + ($i + 1)) $purpose; Start-Sleep -Milliseconds 120 }
}

Start-Sleep -Seconds 12
Press-Key 0x57 'spawn_move_forward' 'real W input from direct InitialHarbor spawn'
Repeat-Key 0x57 24 'move_forward' 'walk toward market/main street'
Repeat-Key 0x41 6 'move_left' 'align toward NPC/market route'
Press-Key 0x45 'npc_interaction_attempt' 'real E NPC interaction attempt'
Start-Sleep -Seconds 18
Press-Key 0x50 'open_port_preparation' 'real P open D9 port preparation'
Start-Sleep -Seconds 3
Press-Key 0x4D 'market_purchase_attempt' 'real M market supply attempt'
Press-Key 0x1B 'close_port_page' 'real Escape close port page'
Press-Key 0x50 'reopen_port_preparation' 'real P reopen port preparation'
Start-Sleep -Seconds 3
Press-Key 0x54 'tavern_action_attempt' 'real T tavern action attempt'
Press-Key 0x44 'departure_attempt' 'real D departure attempt'
Start-Sleep -Seconds 12
Press-Key 0x45 'boarding_attempt' 'real E boarding attempt'
Repeat-Key 0x57 24 'ship_forward' 'real W ship movement attempt'
Press-Key 0x45 'disembark_attempt' 'real E disembark attempt'
Press-Key 0x74 'save_checkpoint' 'real F5 save checkpoint'
Start-Sleep -Seconds 20
Press-Key 0x4E 'voyage_advance_attempt' 'real N voyage advance attempt'
Press-Key 0x4C 'return_settle_attempt' 'real L return/land attempt'
Start-Sleep -Seconds 15
Press-Key 0x50 'berth_port_page' 'real P berth page attempt'
Start-Sleep -Seconds 3
Press-Key 0x42 'berth_entry_attempt' 'real B berth entry attempt'
Press-Key 0x1B 'close_berth_page' 'real Escape close berth page'
Press-Key 0x74 'final_save_checkpoint' 'real F5 final save checkpoint'

# Keep the standalone Player active for the 14-frame contract (approximately 6.5 minutes).
Start-Sleep -Seconds 250
$endedAt = Get-Date
[pscustomobject]@{
    task='D19-R01-S02'
    capture_kind='standalone_player_real_keyboard_route_direct_scene'
    player_path=$PlayerPath
    player_pid=$process.Id
    start_local=$startedAt.ToString('o')
    driver_end_local=$endedAt.ToString('o')
    driver_duration_seconds=($endedAt-$startedAt).TotalSeconds
    required_continuous_route_seconds=300
    keys_sent_by_driver=$steps.Count
    steps=@($steps)
    screenshot_directory=$shotDir
    screenshot_capture_owner='D19R01S02RuntimeSampler via ScreenCapture.CaptureScreenshot inside standalone Player'
    limitation='Direct InitialHarbor entry is a supplement; it does not prove MainMenu character creation.'
} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $routeLog -Encoding UTF8
