[CmdletBinding()]
param(
    [string]$PlayerPath = "",
    [string]$RouteFlag = "-d19-r03-route-rerun48",
    [ValidatePattern("^[A-Za-z0-9_-]+$")]
    [string]$EvidenceTag = "",
    [switch]$UseKeyboardMenuNavigation,
    [int]$MenuWarmupSeconds = 5,
    [int]$HarborWarmupSeconds = 10,
    [int]$CloseTimeoutSeconds = 8,
    [switch]$CaptureBootstrapRejectWindow,
    [switch]$WaterProbe,
    [ValidateSet("ALL", "I", "M", "T")]
    [string]$BootstrapRejectKey = "ALL"
)

$ErrorActionPreference = "Stop"
$scriptDefaultPlayerPath = Join-Path $PSScriptRoot "..\game\tools\results\D19\D19-R03-A01\D19_R03_A01_Player_RERUN48.exe"
if ([string]::IsNullOrWhiteSpace($PlayerPath)) { $PlayerPath = $scriptDefaultPlayerPath }
$workspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$resolvedPlayer = (Resolve-Path -LiteralPath $PlayerPath).Path
$bootstrapRejectRun = [bool]$CaptureBootstrapRejectWindow
$bootstrapRejectTag = if ($bootstrapRejectRun -and $BootstrapRejectKey -ne "ALL") { $BootstrapRejectKey } else { "" }
$bootstrapKeyArgument = if ($bootstrapRejectRun -and $BootstrapRejectKey -ne "ALL") {
    " -d19-r03-bootstrap-key-" + $BootstrapRejectKey.ToLowerInvariant()
} else { "" }
$runArgument = if ($bootstrapRejectRun) {
    "-d19-r03-bootstrap-reject-rerun1" + $bootstrapKeyArgument
} else { $RouteFlag }
$isolatedRouteRun = -not $bootstrapRejectRun -and -not [string]::IsNullOrWhiteSpace($EvidenceTag)
$waterProbeRun = [bool]$WaterProbe
$isolatedRouteTag = if ($isolatedRouteRun) { $EvidenceTag.ToUpperInvariant() } else { "" }
$isolatedScreenshotTag = if ($isolatedRouteRun) { $EvidenceTag.ToLowerInvariant() } else { "" }
# EvidenceTag namespaces the machine summary, log archive and screenshots. The
# read-only Player probe still names its JSON by RouteFlag, so derive that
# filename independently instead of treating a presentation tag as a probe tag.
$routeProbeTag = if ($RouteFlag -match "-rerun(\d+)$") {
    "RERUN$($Matches[1])"
} else {
    "RERUN48"
}
$resultRoot = Join-Path $workspaceRoot "game\tools\results\VISUAL-CLEAN\VC-G01"
$summaryPath = if ($bootstrapRejectRun -and $bootstrapRejectTag) {
    Join-Path $resultRoot ("VC_G01_BOOTSTRAP_REJECT_{0}_20260906.json" -f $bootstrapRejectTag)
} elseif ($bootstrapRejectRun) {
    Join-Path $resultRoot "VC_G01_BOOTSTRAP_REJECT_20260906.json"
} elseif ($isolatedRouteRun) {
    Join-Path $resultRoot ("VC_G01_RUNTIME_INPUT_{0}_20260906.json" -f $isolatedRouteTag)
} else {
    Join-Path $resultRoot "VC_G01_RUNTIME_INPUT_20260906.json"
}
$probePath = if ($bootstrapRejectRun -and $bootstrapRejectTag) {
    Join-Path $workspaceRoot ("game\tools\results\D19\D19-R03-A01\D19_R03_A01_RUNTIME_ROUTE_PROBE_BOOTSTRAP_REJECT_{0}_20260906.json" -f $bootstrapRejectTag)
} elseif ($bootstrapRejectRun) {
    Join-Path $workspaceRoot "game\tools\results\D19\D19-R03-A01\D19_R03_A01_RUNTIME_ROUTE_PROBE_BOOTSTRAP_REJECT_20260906.json"
} elseif ($isolatedRouteRun) {
    Join-Path $workspaceRoot ("game\tools\results\D19\D19-R03-A01\D19_R03_A01_RUNTIME_ROUTE_PROBE_{0}.json" -f $routeProbeTag)
} else {
    Join-Path $workspaceRoot "game\tools\results\D19\D19-R03-A01\D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json"
}
$probeArchivePath = if ($isolatedRouteRun) {
    Join-Path $resultRoot ("VC_G01_RUNTIME_ROUTE_PROBE_{0}_20260906.json" -f $isolatedRouteTag)
} else {
    $null
}
$playerLogPath = Join-Path $env:USERPROFILE "AppData\LocalLow\Whalebone Studio\WhalesIslandReef\Player.log"
$playerLogArchivePath = if ($bootstrapRejectRun -and $bootstrapRejectTag) {
    Join-Path $resultRoot ("VC_G01_PLAYER_RUNTIME_BOOTSTRAP_REJECT_{0}_20260906.log" -f $bootstrapRejectTag)
} elseif ($bootstrapRejectRun) {
    Join-Path $resultRoot "VC_G01_PLAYER_RUNTIME_BOOTSTRAP_REJECT_20260906.log"
} elseif ($isolatedRouteRun) {
    Join-Path $resultRoot ("VC_G01_PLAYER_RUNTIME_INPUT_{0}_20260906.log" -f $isolatedRouteTag)
} else { $null }
$screenshotDirectory = if ($bootstrapRejectRun -and $bootstrapRejectTag) {
    Join-Path $resultRoot ("runtime_bootstrap_reject_{0}_20260906" -f $bootstrapRejectTag)
} elseif ($bootstrapRejectRun) {
    Join-Path $resultRoot "runtime_bootstrap_reject_20260906"
} elseif ($isolatedRouteRun) {
    Join-Path $resultRoot ("runtime_input_{0}_20260906" -f $isolatedScreenshotTag)
} else {
    Join-Path $resultRoot "runtime_input_20260906"
}
$runStamp = Get-Date -Format "yyyyMMdd_HHmmss"
$bootstrapLogMarker = "[D19-R03-A01] Bootstrap owner-not-ready rejection window active"

if (-not (Test-Path -LiteralPath $resolvedPlayer)) {
    throw "VC-G01 runtime input probe 找不到 Player: $resolvedPlayer"
}
New-Item -ItemType Directory -Force -Path $screenshotDirectory | Out-Null

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class VcG01NativeInput20260906
{
    private const uint KeyUp = 0x0002;
    private const uint InputKeyboard = 1;
    private const uint SendInputKeyUp = 0x0002;
    private const uint SendInputScanCode = 0x0008;
    private const uint LeftDown = 0x0002;
    private const uint LeftUp = 0x0004;

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeInput
    {
        public uint Type;
        public KeyboardInput Keyboard;
        // Win32 INPUT contains a 32-byte union on x64 (the mouse member is
        // the largest union arm). Keep the managed shape at 40 bytes so
        // SendInput does not reject the record and force the legacy fallback.
        public ulong UnionPadding;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetClientRect(IntPtr hWnd, out Rect rect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ClientToScreen(IntPtr hWnd, ref Point point);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetActiveWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool attach);

    [DllImport("user32.dll")]
    private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

    [DllImport("user32.dll")]
    private static extern bool AllowSetForegroundWindow(uint processId);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter,
        int x, int y, int width, int height, uint flags);

    private const uint AllowSetForegroundWindowAny = 0xFFFFFFFF;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpShowWindow = 0x0040;
    private const uint SwpNoOwnerZOrder = 0x0200;
    private static readonly IntPtr HwndTopmost = new IntPtr(-1);
    private static readonly IntPtr HwndNoTopmost = new IntPtr(-2);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int command);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, NativeInput[] inputs, int inputSize);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint code, uint mapType);

    public static bool Activate(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero) return false;
        uint targetProcessId;
        uint targetThreadId = GetWindowThreadProcessId(hWnd, out targetProcessId);
        uint currentThreadId = GetCurrentThreadId();
        IntPtr foregroundWindow = GetForegroundWindow();
        uint foregroundProcessId;
        uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindow, out foregroundProcessId);
        bool attachedForeground = foregroundThreadId != 0 && foregroundThreadId != currentThreadId &&
            AttachThreadInput(currentThreadId, foregroundThreadId, true);
        bool attachedTarget = targetThreadId != 0 && targetThreadId != currentThreadId &&
            AttachThreadInput(currentThreadId, targetThreadId, true);
        try
        {
            ShowWindow(hWnd, 5);
            // Unity Player may publish its main HWND before the first UI frame,
            // and Windows foreground-lock rules may reject the first call from
            // a shell-hosted probe. Temporarily put the window at the front,
            // attach the input queues, and verify the foreground HWND before
            // any key or mouse event is sent.
            AllowSetForegroundWindow(AllowSetForegroundWindowAny);
            SetWindowPos(hWnd, HwndTopmost, 0, 0, 0, 0,
                SwpNoSize | SwpNoMove | SwpShowWindow);
            SetWindowPos(hWnd, HwndNoTopmost, 0, 0, 0, 0,
                SwpNoSize | SwpNoMove | SwpShowWindow | SwpNoOwnerZOrder);
            // The foreground-lock policy accepts a bounded user-like Alt key
            // transition after the current foreground queue is attached. The
            // paired events do not select a menu or alter gameplay state.
            keybd_event(0x12, 0, 0, UIntPtr.Zero);
            keybd_event(0x12, 0, KeyUp, UIntPtr.Zero);
            for (var attempt = 0; attempt < 8; attempt++)
            {
                SetForegroundWindow(hWnd);
                BringWindowToTop(hWnd);
                SwitchToThisWindow(hWnd, true);
                SetActiveWindow(hWnd);
                SetFocus(hWnd);
                if (GetForegroundWindow() == hWnd) return true;
                System.Threading.Thread.Sleep(50);
            }
            return false;
        }
        finally
        {
            if (attachedTarget) AttachThreadInput(currentThreadId, targetThreadId, false);
            if (attachedForeground) AttachThreadInput(currentThreadId, foregroundThreadId, false);
        }
    }

    public static bool SetDpiAware()
    {
        return SetProcessDPIAware();
    }

    public static bool GetClientSize(IntPtr hWnd, out int width, out int height)
    {
        Rect rect;
        if (!GetClientRect(hWnd, out rect))
        {
            width = 0;
            height = 0;
            return false;
        }
        width = rect.Right - rect.Left;
        height = rect.Bottom - rect.Top;
        return width > 0 && height > 0;
    }

    public static bool GetWindowBounds(IntPtr hWnd, out int left, out int top, out int width, out int height)
    {
        Rect rect;
        if (!GetWindowRect(hWnd, out rect))
        {
            left = 0;
            top = 0;
            width = 0;
            height = 0;
            return false;
        }
        left = rect.Left;
        top = rect.Top;
        width = rect.Right - rect.Left;
        height = rect.Bottom - rect.Top;
        return width > 0 && height > 0;
    }

    public static bool ClickClient(IntPtr hWnd, int clientX, int clientY)
    {
        Point point = new Point { X = clientX, Y = clientY };
        if (!ClientToScreen(hWnd, ref point)) return false;
        if (!SetCursorPos(point.X, point.Y)) return false;
        mouse_event(LeftDown, 0, 0, 0, UIntPtr.Zero);
        mouse_event(LeftUp, 0, 0, 0, UIntPtr.Zero);
        return true;
    }

    public static bool PressKey(IntPtr hWnd, byte virtualKey, int holdMilliseconds)
    {
        if (!Activate(hWnd))
        {
            LastPressUsedSendInput = false;
            return false;
        }
        bool downUsedSendInput;
        bool down = SendKeyEvent(virtualKey, false, out downUsedSendInput);
        System.Threading.Thread.Sleep(Math.Max(1, holdMilliseconds));
        bool upUsedSendInput;
        bool up = SendKeyEvent(virtualKey, true, out upUsedSendInput);
        LastPressUsedSendInput = downUsedSendInput && upUsedSendInput;
        return down && up;
    }

    public static bool HoldKey(IntPtr hWnd, byte virtualKey, int holdMilliseconds)
    {
        if (!Activate(hWnd))
        {
            LastPressUsedSendInput = false;
            return false;
        }
        bool downUsedSendInput;
        bool down = SendKeyEvent(virtualKey, false, out downUsedSendInput);
        System.Threading.Thread.Sleep(Math.Max(1, holdMilliseconds));
        bool upUsedSendInput;
        bool up = SendKeyEvent(virtualKey, true, out upUsedSendInput);
        LastPressUsedSendInput = downUsedSendInput && upUsedSendInput;
        return down && up;
    }

    public static bool LastPressUsedSendInput { get; private set; }

    private static bool SendKeyEvent(byte virtualKey, bool keyUp, out bool usedSendInput)
    {
        ushort scanCode = (ushort)MapVirtualKey(virtualKey, 0);
        var input = new NativeInput
        {
            Type = InputKeyboard,
            Keyboard = new KeyboardInput
            {
                VirtualKey = 0,
                ScanCode = scanCode,
                Flags = SendInputScanCode | (keyUp ? SendInputKeyUp : 0),
                Time = 0,
                ExtraInfo = UIntPtr.Zero,
            },
        };
        var sent = SendInput(1, new[] { input }, Marshal.SizeOf(typeof(NativeInput)));
        usedSendInput = sent == 1;
        if (usedSendInput) return true;

        // 保留 Windows legacy fallback，便于旧版 Unity Player 在 SendInput
        // 被安全策略拦截时仍能证明 D29/D10/D3 的兼容键盘入口。
        keybd_event(virtualKey, 0, keyUp ? KeyUp : 0, UIntPtr.Zero);
        return true;
    }
}
"@

Add-Type -AssemblyName System.Drawing

$VirtualKey = @{
    W = [byte]0x57
    A = [byte]0x41
    S = [byte]0x53
    D = [byte]0x44
    E = [byte]0x45
    I = [byte]0x49
    M = [byte]0x4D
    T = [byte]0x54
    Escape = [byte]0x1B
    Tab = [byte]0x09
    Return = [byte]0x0D
}

$steps = [System.Collections.Generic.List[object]]::new()
$screenshots = [System.Collections.Generic.List[object]]::new()
$process = $null
$windowHandle = [IntPtr]::Zero
$clientWidth = 0
$clientHeight = 0
$referenceScale = 1.0
$startedAt = Get-Date
$sceneMarkerSeen = $false
$probeResult = $null
$playerLogText = ""
$fatalError = $null

function Add-Step {
    param(
        [string]$Name,
        [string]$Kind,
        [bool]$Succeeded,
        [hashtable]$Details = @{}
    )

    $record = [ordered]@{
        name = $Name
        kind = $Kind
        succeeded = $Succeeded
        timestamp_local = (Get-Date).ToString("o")
    }
    foreach ($key in $Details.Keys) { $record[$key] = $Details[$key] }
    $steps.Add([pscustomobject]$record)
}

function Activate-Player {
    if ($windowHandle -eq [IntPtr]::Zero) { throw "Player 主窗口句柄为空" }
    # WScript.Shell is a bounded Windows-shell fallback for sessions where
    # SetForegroundWindow is rejected by the foreground-lock policy. It only
    # activates the already-started Player; it does not synthesize gameplay.
    try {
        $shell = New-Object -ComObject WScript.Shell
        [void]$shell.AppActivate($process.Id)
    } catch { }
    for ($attempt = 0; $attempt -lt 6; $attempt++) {
        if ([VcG01NativeInput20260906]::Activate($windowHandle)) {
            Start-Sleep -Milliseconds 120
            return
        }
        Start-Sleep -Milliseconds 120
    }
    throw "无法激活 Player 主窗口"
}

function Send-KeyPress {
    param(
        [string]$Name,
        [byte]$Key,
        [int]$AfterMilliseconds = 500,
        [int]$HoldMilliseconds = 80
    )
    $ok = [VcG01NativeInput20260906]::PressKey($windowHandle, $Key, $HoldMilliseconds)
    $nativeAccepted = [VcG01NativeInput20260906]::LastPressUsedSendInput
    Add-Step -Name $Name -Kind "key_press" -Succeeded $ok -Details @{ virtual_key = $Key; hold_ms = $HoldMilliseconds; send_input_accepted = $nativeAccepted }
    Start-Sleep -Milliseconds $AfterMilliseconds
}

function Send-KeyHold {
    param(
        [string]$Name,
        [byte]$Key,
        [int]$HoldMilliseconds,
        [int]$AfterMilliseconds = 300
    )
    $ok = [VcG01NativeInput20260906]::HoldKey($windowHandle, $Key, $HoldMilliseconds)
    $nativeAccepted = [VcG01NativeInput20260906]::LastPressUsedSendInput
    Add-Step -Name $Name -Kind "key_hold" -Succeeded $ok -Details @{ virtual_key = $Key; hold_ms = $HoldMilliseconds; send_input_accepted = $nativeAccepted }
    Start-Sleep -Milliseconds $AfterMilliseconds
}

function Click-ReferencePoint {
    param(
        [string]$Name,
        [double]$OffsetX,
        [double]$OffsetY,
        [int]$AfterMilliseconds = 800
    )
    $clientX = [int]($clientWidth / 2 + $OffsetX * $referenceScale)
    $clientY = [int]($clientHeight / 2 - $OffsetY * $referenceScale)
    $ok = [VcG01NativeInput20260906]::ClickClient($windowHandle, $clientX, $clientY)
    Add-Step -Name $Name -Kind "mouse_click" -Succeeded $ok -Details @{
        client_x = $clientX
        client_y = $clientY
        reference_offset_x = $OffsetX
        reference_offset_y = $OffsetY
    }
    Start-Sleep -Milliseconds $AfterMilliseconds
}

function Capture-PlayerWindow {
    param(
        [string]$Name,
        [int]$AfterMilliseconds = 220
    )

    Start-Sleep -Milliseconds $AfterMilliseconds
    $capturePath = Join-Path $screenshotDirectory ("{0}_{1}.png" -f $runStamp, $Name)
    $left = 0
    $top = 0
    $width = 0
    $height = 0
    $succeeded = $false
    $errorMessage = $null
    $bitmap = $null
    $graphics = $null
    try {
        if (-not [VcG01NativeInput20260906]::GetWindowBounds($windowHandle, [ref]$left, [ref]$top, [ref]$width, [ref]$height)) {
            throw "无法读取 Player 窗口边界"
        }
        if ($width -lt 1 -or $height -lt 1) { throw "Player 窗口边界无效：${width}x${height}" }
        $bitmap = New-Object System.Drawing.Bitmap($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.CopyFromScreen($left, $top, 0, 0, $bitmap.Size, [System.Drawing.CopyPixelOperation]::SourceCopy)
        $bitmap.Save($capturePath, [System.Drawing.Imaging.ImageFormat]::Png)
        $succeeded = Test-Path -LiteralPath $capturePath
    }
    catch {
        $errorMessage = $_.Exception.Message
    }
    finally {
        if ($graphics) { $graphics.Dispose() }
        if ($bitmap) { $bitmap.Dispose() }
    }
    $record = [ordered]@{
        name = $Name
        path = $capturePath
        succeeded = $succeeded
        window_left = $left
        window_top = $top
        window_width = $width
        window_height = $height
        error = $errorMessage
    }
    $screenshots.Add([pscustomobject]$record)
    Add-Step -Name ("screenshot_{0}" -f $Name) -Kind "screenshot" -Succeeded $succeeded -Details @{
        path = $capturePath
        window_width = $width
        window_height = $height
        error = $errorMessage
    }
}

function Read-PlayerLog {
    if (Test-Path -LiteralPath $playerLogPath) {
        return Get-Content -LiteralPath $playerLogPath -Raw -ErrorAction SilentlyContinue
    }
    return ""
}

function Get-PageStateMatrix {
    param(
        [object]$Probe,
        [object[]]$InputSteps
    )

    $observations = if ($Probe) { @($Probe.observations) } else { @() }
    $events = if ($Probe) { @($Probe.events) } else { @() }
    $inputNames = @($InputSteps | ForEach-Object { $_.name })
    $requiredNames = @(
        "warehouse_open_i",
        "warehouse_close_escape",
        "warehouse_duplicate_i_first",
        "warehouse_duplicate_i_second",
        "pause_open_after_warehouse_escape",
        "pause_recover_after_warehouse",
        "market_open_m",
        "market_duplicate_m",
        "market_close_escape",
        "tavern_open_t",
        "tavern_duplicate_t",
        "tavern_close_escape"
    )
    $missingNames = @($requiredNames | Where-Object { $inputNames -notcontains $_ })

    $warehouseOpenTransitions = @($events | Where-Object { $_.type -eq "warehouse_page_open_observed" }).Count
    $warehouseCloseTransitions = @($events | Where-Object { $_.type -eq "warehouse_page_close_observed" }).Count
    $marketOpenTransitions = @($events | Where-Object { $_.type -eq "market_page_open_observed" }).Count
    $marketCloseTransitions = @($events | Where-Object { $_.type -eq "market_page_close_observed" }).Count
    $tavernOpenTransitions = @($events | Where-Object { $_.type -eq "tavern_page_open_observed" }).Count
    $tavernCloseTransitions = @($events | Where-Object { $_.type -eq "tavern_page_close_observed" }).Count
    $warehouseInputEdges = @($events | Where-Object { $_.type -eq "warehouse_input_edge_observed" }).Count
    $marketInputEdges = @($events | Where-Object { $_.type -eq "market_input_edge_observed" }).Count
    $tavernInputEdges = @($events | Where-Object { $_.type -eq "tavern_input_edge_observed" }).Count
    $warehouseVisibleSamples = @($observations | Where-Object { [bool]$_.warehouse_visible }).Count
    $marketVisibleSamples = @($observations | Where-Object { [bool]$_.market_visible }).Count
    $tavernVisibleSamples = @($observations | Where-Object { [bool]$_.tavern_visible }).Count
    $d3ReadySamples = @($observations | Where-Object { [bool]$_.d3_owner_ready }).Count
    $d10ReadySamples = @($observations | Where-Object { [bool]$_.d10_owner_ready }).Count
    $warehousePanelFoundSamples = @($observations | Where-Object { [bool]$_.d29_warehouse_panel_found }).Count
    $lastObservation = if ($observations.Count -gt 0) { $observations[$observations.Count - 1] } else { $null }
    $lastWarehouseVisible = if ($lastObservation) { [bool]$lastObservation.warehouse_visible } else { $false }
    $lastMarketVisible = if ($lastObservation) { [bool]$lastObservation.market_visible } else { $false }
    $lastTavernVisible = if ($lastObservation) { [bool]$lastObservation.tavern_visible } else { $false }
    $allMatrixStepsPresent = $missingNames.Count -eq 0

    $warehouse = [ordered]@{
        required_steps_present = $allMatrixStepsPresent
        owner_ready_observations = $d3ReadySamples
        panel_found_observations = $warehousePanelFoundSamples
        input_edge_count = $warehouseInputEdges
        visible_sample_count = $warehouseVisibleSamples
        open_transition_count = $warehouseOpenTransitions
        close_transition_count = $warehouseCloseTransitions
        duplicate_i_toggle_proof = [bool]($allMatrixStepsPresent -and
            $warehouseInputEdges -ge 3 -and
            $warehouseOpenTransitions -ge 2 -and
            $warehouseCloseTransitions -ge 2)
        escape_close_recovery_proof = [bool]($allMatrixStepsPresent -and
            $warehouseCloseTransitions -ge 2 -and -not $lastWarehouseVisible)
        owner_not_ready_reject_proof = $false
        owner_not_ready_reject_note = "本序列未在 D3 owner 未就绪窗口发起 I；该 reject 仍需独立运行时窗口证据。"
    }
    $market = [ordered]@{
        required_steps_present = $allMatrixStepsPresent
        owner_ready_observations = $d3ReadySamples
        input_edge_count = $marketInputEdges
        visible_sample_count = $marketVisibleSamples
        open_transition_count = $marketOpenTransitions
        close_transition_count = $marketCloseTransitions
        duplicate_m_stable_page_proof = [bool]($allMatrixStepsPresent -and
            $marketInputEdges -ge 2 -and
            $marketVisibleSamples -ge 2 -and
            $marketOpenTransitions -eq 1 -and
            $marketCloseTransitions -eq 1)
        escape_close_recovery_proof = [bool]($allMatrixStepsPresent -and
            $marketOpenTransitions -eq 1 -and
            $marketCloseTransitions -eq 1 -and -not $lastMarketVisible)
        owner_not_ready_reject_proof = $false
        owner_not_ready_reject_note = "本序列未在 D3 owner 未就绪窗口发起 M；该 reject 仍需独立运行时窗口证据。"
    }
    $tavern = [ordered]@{
        required_steps_present = $allMatrixStepsPresent
        owner_ready_observations = $d10ReadySamples
        input_edge_count = $tavernInputEdges
        visible_sample_count = $tavernVisibleSamples
        open_transition_count = $tavernOpenTransitions
        close_transition_count = $tavernCloseTransitions
        duplicate_t_stable_page_proof = [bool]($allMatrixStepsPresent -and
            $tavernInputEdges -ge 2 -and
            $tavernVisibleSamples -ge 2 -and
            $tavernOpenTransitions -eq 1 -and
            $tavernCloseTransitions -eq 1)
        escape_close_recovery_proof = [bool]($allMatrixStepsPresent -and
            $tavernOpenTransitions -eq 1 -and
            $tavernCloseTransitions -eq 1 -and -not $lastTavernVisible)
        owner_not_ready_reject_proof = $false
        owner_not_ready_reject_note = "本序列未在 D10 owner 未就绪窗口发起 T；该 reject 仍需独立运行时窗口证据。"
    }
    $matrixProof = [bool]($warehouse.duplicate_i_toggle_proof -and
        $warehouse.escape_close_recovery_proof -and
        $market.duplicate_m_stable_page_proof -and
        $market.escape_close_recovery_proof -and
        $tavern.duplicate_t_stable_page_proof -and
        $tavern.escape_close_recovery_proof)

    return [ordered]@{
        schema = "VC_G01_RUNTIME_INTERACTION_MATRIX_V1"
        required_steps_present = $allMatrixStepsPresent
        missing_steps = $missingNames
        matrix_proof = $matrixProof
        warehouse = $warehouse
        market = $market
        tavern = $tavern
        final_temporary_pages_closed = [bool](-not $lastWarehouseVisible -and
            -not $lastMarketVisible -and -not $lastTavernVisible)
        note = "页面状态来自 standalone Player 只读 probe 的 owner/page 状态，不由截图猜测；owner-not-ready reject 不在本次序列中伪造为 PASS。"
    }
}

function Wait-ForPlayerLogMarker {
    param([string]$Pattern, [int]$TimeoutSeconds)
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $text = Read-PlayerLog
        if ($text -match $Pattern) { return $true }
        Start-Sleep -Milliseconds 250
    } while ((Get-Date) -lt $deadline)
    return $false
}

function Get-BootstrapRejectMatrix {
    param(
        [object]$Probe,
        [object[]]$InputSteps,
        [string]$RunLog,
        [string]$TargetKey = "ALL"
    )

    $events = if ($Probe) { @($Probe.events) } else { @() }
    $warehouseEdges = @($events | Where-Object {
        $_.type -eq "warehouse_input_edge_observed" -and
        [bool]$_.runtime_state_captured -and
        -not [bool]$_.d3_owner_ready -and
        -not [bool]$_.warehouse_visible
    })
    $marketEdges = @($events | Where-Object {
        $_.type -eq "market_input_edge_observed" -and
        [bool]$_.runtime_state_captured -and
        -not [bool]$_.d3_owner_ready -and
        -not [bool]$_.market_visible
    })
    $tavernEdges = @($events | Where-Object {
        $_.type -eq "tavern_input_edge_observed" -and
        [bool]$_.runtime_state_captured -and
        -not [bool]$_.d10_owner_ready -and
        -not [bool]$_.tavern_visible
    })
    $warehouseOpenCount = @($events | Where-Object { $_.type -eq "warehouse_page_open_observed" }).Count
    $marketOpenCount = @($events | Where-Object { $_.type -eq "market_page_open_observed" }).Count
    $tavernOpenCount = @($events | Where-Object { $_.type -eq "tavern_page_open_observed" }).Count
    $warehouseStep = @($InputSteps | Where-Object { $_.name -eq "bootstrap_warehouse_i" }) | Select-Object -First 1
    $marketStep = @($InputSteps | Where-Object { $_.name -eq "bootstrap_market_m" }) | Select-Object -First 1
    $tavernStep = @($InputSteps | Where-Object { $_.name -eq "bootstrap_tavern_t" }) | Select-Object -First 1
    $logMarkerFound = $RunLog.Contains($bootstrapLogMarker)
    $warehouseLog = $RunLog -match [regex]::Escape("仓库 owner 尚未就绪，暂不能读取库存。")
    $marketLog = $RunLog -match [regex]::Escape("市场 owner 尚未就绪；按钮已响应，请稍后再试。")
    $tavernLog = $RunLog -match [regex]::Escape("酒馆 owner 尚未就绪；按钮已响应，请稍后再试。")
    $warehouseStepProof = $null -ne $warehouseStep -and [bool]$warehouseStep.succeeded -and [bool]$warehouseStep.send_input_accepted
    $marketStepProof = $null -ne $marketStep -and [bool]$marketStep.succeeded -and [bool]$marketStep.send_input_accepted
    $tavernStepProof = $null -ne $tavernStep -and [bool]$tavernStep.succeeded -and [bool]$tavernStep.send_input_accepted
    $warehouseProof = [bool]($logMarkerFound -and $warehouseStepProof -and
        $warehouseEdges.Count -gt 0 -and $warehouseLog -and $warehouseOpenCount -eq 0)
    $marketProof = [bool]($logMarkerFound -and $marketStepProof -and
        $marketEdges.Count -gt 0 -and $marketLog -and $marketOpenCount -eq 0)
    $tavernProof = [bool]($logMarkerFound -and $tavernStepProof -and
        $tavernEdges.Count -gt 0 -and $tavernLog -and $tavernOpenCount -eq 0)
    $targetProof = switch ($TargetKey) {
        "I" { $warehouseProof }
        "M" { $marketProof }
        "T" { $tavernProof }
        default { [bool]($warehouseProof -and $marketProof -and $tavernProof) }
    }

    return [ordered]@{
        schema = "VC_G01_RUNTIME_BOOTSTRAP_REJECT_MATRIX_V1"
        target_key = $TargetKey
        run_marker_found = $logMarkerFound
        warehouse = [ordered]@{
            input_step_succeeded = $warehouseStepProof
            unready_owner_edge_count = $warehouseEdges.Count
            reject_log_found_after_marker = [bool]$warehouseLog
            page_open_transition_count = $warehouseOpenCount
            proof = $warehouseProof
            note = "I edge 的同帧 probe 状态记录 d3_owner_ready=false 且仓库页未显示，并在本次 run marker 后看到 D29 owner-not-ready 反馈。"
        }
        market = [ordered]@{
            input_step_succeeded = $marketStepProof
            unready_owner_edge_count = $marketEdges.Count
            reject_log_found_after_marker = [bool]$marketLog
            page_open_transition_count = $marketOpenCount
            proof = $marketProof
            note = "M edge 的同帧 probe 状态记录 d3_owner_ready=false 且市场页未显示，并在本次 run marker 后看到 D29 owner-not-ready 反馈。"
        }
        tavern = [ordered]@{
            input_step_succeeded = $tavernStepProof
            unready_owner_edge_count = $tavernEdges.Count
            reject_log_found_after_marker = [bool]$tavernLog
            page_open_transition_count = $tavernOpenCount
            proof = $tavernProof
            note = "T edge 的同帧 probe 状态记录 d10_owner_ready=false 且酒馆页未显示，并在本次 run marker 后看到 D29 owner-not-ready 反馈。"
        }
        matrix_proof = [bool]($warehouseProof -and $marketProof -and $tavernProof)
        target_proof = [bool]$targetProof
        note = "只接受本次 bootstrap run marker 之后的 Player.log，并要求真实 SendInput step、同帧 owner 未就绪状态、D29 reject 反馈和无页面打开四项同时成立；不会用 owner-ready 或截图推导 reject。"
    }
}

try {
    [VcG01NativeInput20260906]::SetDpiAware() | Out-Null
    $existing = Get-Process -Name ([IO.Path]::GetFileNameWithoutExtension($resolvedPlayer)) -ErrorAction SilentlyContinue |
        Where-Object {
            try { (Resolve-Path -LiteralPath $_.Path -ErrorAction Stop).Path -eq $resolvedPlayer } catch { $false }
        }
    if ($existing) {
        throw "VC-G01 runtime input probe 拒绝已有同一路径 Player 并发实例"
    }

    $process = Start-Process -FilePath $resolvedPlayer -ArgumentList $runArgument -PassThru
    Add-Step -Name "player_started" -Kind "process" -Succeeded $true -Details @{ pid = $process.Id; path = $resolvedPlayer }

    $windowDeadline = (Get-Date).AddSeconds(15)
    do {
        $process.Refresh()
        $windowHandle = $process.MainWindowHandle
        if ($windowHandle -ne [IntPtr]::Zero) { break }
        Start-Sleep -Milliseconds 250
    } while ((Get-Date) -lt $windowDeadline)
    if ($windowHandle -eq [IntPtr]::Zero) { throw "Player 在 15 秒内没有可操作主窗口" }

    if (-not [VcG01NativeInput20260906]::GetClientSize($windowHandle, [ref]$clientWidth, [ref]$clientHeight)) {
        throw "无法读取 Player 客户区尺寸"
    }
    $referenceScale = [Math]::Sqrt(($clientWidth / 1920.0) * ($clientHeight / 1080.0))
    Add-Step -Name "player_window_bound" -Kind "window" -Succeeded $true -Details @{
        window_handle = $windowHandle.ToInt64()
        client_width = $clientWidth
        client_height = $clientHeight
        reference_scale = $referenceScale
        title = $process.MainWindowTitle
    }

    Activate-Player
    Start-Sleep -Seconds $MenuWarmupSeconds
    if ($UseKeyboardMenuNavigation) {
        # D16ModalFocusScope selects BtnNewGame on the visible main menu;
        # keyboard submit avoids DPI/window-coordinate ambiguity in a native
        # Player while still exercising the real Unity UI input path.
        Send-KeyPress -Name "main_menu_new_game_keyboard_submit" -Key $VirtualKey.Return -AfterMilliseconds 700
    } else {
        Click-ReferencePoint -Name "main_menu_new_game" -OffsetX 0 -OffsetY 80
    }
    if ($bootstrapRejectRun) {
        # The confirmation click loads InitialHarbor synchronously. Keep the
        # first real key edges inside the short D3/D10 bootstrap window; the
        # probe and D29 log decide whether the owners were actually unready.
        Click-ReferencePoint -Name "new_game_confirm_bootstrap_window" -OffsetX -100 -OffsetY -110 -AfterMilliseconds 25
        Start-Sleep -Milliseconds 120
        if ($BootstrapRejectKey -eq "ALL" -or $BootstrapRejectKey -eq "I") {
            Send-KeyPress -Name "bootstrap_warehouse_i" -Key $VirtualKey.I -AfterMilliseconds 8 -HoldMilliseconds 80
        }
        if ($BootstrapRejectKey -eq "ALL" -or $BootstrapRejectKey -eq "M") {
            Send-KeyPress -Name "bootstrap_market_m" -Key $VirtualKey.M -AfterMilliseconds 8 -HoldMilliseconds 80
        }
        if ($BootstrapRejectKey -eq "ALL" -or $BootstrapRejectKey -eq "T") {
            Send-KeyPress -Name "bootstrap_tavern_t" -Key $VirtualKey.T -AfterMilliseconds 8 -HoldMilliseconds 80
        }
        Capture-PlayerWindow -Name "bootstrap_reject_after_i_m_t"
        Start-Sleep -Milliseconds 1200
        Capture-PlayerWindow -Name "bootstrap_owner_ready_recovery"
    }
    else {
        if ($UseKeyboardMenuNavigation) {
            # NewGamePanel initially focuses CaptainInput. Keep the keyboard
            # edge in the evidence, then click the visible confirm button;
            # legacy UGUI InputField navigation does not consistently move
            # from text input to the sibling Button in a standalone Player.
            Send-KeyPress -Name "new_game_confirm_keyboard_tab" -Key $VirtualKey.Tab -AfterMilliseconds 120
            Click-ReferencePoint -Name "new_game_confirm_mouse_after_keyboard" -OffsetX -100 -OffsetY -110 -AfterMilliseconds 800
        } else {
            Click-ReferencePoint -Name "new_game_confirm" -OffsetX -100 -OffsetY -110
        }
        Capture-PlayerWindow -Name "route_initial_harbor"
        $sceneMarkerSeen = Wait-ForPlayerLogMarker -Pattern "InitialHarbor" -TimeoutSeconds $HarborWarmupSeconds
        Add-Step -Name "initial_harbor_log_marker" -Kind "log_observation" -Succeeded $sceneMarkerSeen -Details @{ pattern = "InitialHarbor" }

        # Native OS key events against the foreground standalone Player. The route
        # probe remains read-only; this harness never calls gameplay methods.
        Send-KeyPress -Name "board_or_reject_e" -Key $VirtualKey.E -AfterMilliseconds 900
        Capture-PlayerWindow -Name "board_or_reject_e"
        if ($waterProbeRun) {
            # Keep the player aboard while exercising the real ship owner.  The
            # probe records control_mode, ship position and heading so this is
            # evidence of ship movement/turning, not merely a boarding toast.
            Send-KeyHold -Name "ship_control_w" -Key $VirtualKey.W -HoldMilliseconds 1800
            Send-KeyHold -Name "ship_turn_a" -Key $VirtualKey.A -HoldMilliseconds 900
            Capture-PlayerWindow -Name "ship_control_w_a"
        }
        Send-KeyPress -Name "disembark_or_repeat_e" -Key $VirtualKey.E -AfterMilliseconds 900
        Capture-PlayerWindow -Name "disembark_or_repeat_e"
        if ($waterProbeRun) {
            # The R03 water route deliberately crosses the authored island edge
            # so the read-only probe can observe the real D2 swimming state and
            # the native Player can provide bounded visual evidence.  This is
            # still an OS input only; the harness never calls gameplay methods.
            Send-KeyHold -Name "move_s_into_water" -Key $VirtualKey.S -HoldMilliseconds 9000
            Capture-PlayerWindow -Name "swimming_water_route"
            Send-KeyHold -Name "move_w_back_to_shore" -Key $VirtualKey.W -HoldMilliseconds 5000
            Capture-PlayerWindow -Name "swimming_return_to_shore"
        } else {
            # Keep a short real W sample, then move away from the ship. The
            # player starts near world z=-0.7 while the ship is near z=-7, so
            # a long W hold moves toward the helm and makes the supposed
            # rejection press board again. S is the actual away-from-ship
            # direction for this authored InitialHarbor route.
            Send-KeyHold -Name "move_w" -Key $VirtualKey.W -HoldMilliseconds 800
            Send-KeyHold -Name "move_s_away_from_ship" -Key $VirtualKey.S -HoldMilliseconds 5000
            Send-KeyPress -Name "reject_e_far_from_ship" -Key $VirtualKey.E -AfterMilliseconds 900
            Capture-PlayerWindow -Name "reject_e_far_from_ship"
        }
        Send-KeyHold -Name "turn_or_strafe_a" -Key $VirtualKey.A -HoldMilliseconds 900
        Send-KeyHold -Name "turn_or_strafe_d" -Key $VirtualKey.D -HoldMilliseconds 900
        Send-KeyHold -Name "move_s" -Key $VirtualKey.S -HoldMilliseconds 1200

        Send-KeyPress -Name "warehouse_open_i" -Key $VirtualKey.I -AfterMilliseconds 700
        Capture-PlayerWindow -Name "warehouse_open_i"
        Send-KeyPress -Name "warehouse_close_escape" -Key $VirtualKey.Escape -AfterMilliseconds 500
        Capture-PlayerWindow -Name "warehouse_close_escape"
        Send-KeyPress -Name "warehouse_duplicate_i_first" -Key $VirtualKey.I -AfterMilliseconds 60
        Send-KeyPress -Name "warehouse_duplicate_i_second" -Key $VirtualKey.I -AfterMilliseconds 500
        Capture-PlayerWindow -Name "warehouse_duplicate_i"
        Send-KeyPress -Name "pause_open_after_warehouse_escape" -Key $VirtualKey.Escape -AfterMilliseconds 800
        Capture-PlayerWindow -Name "pause_open_after_warehouse"
        Send-KeyPress -Name "pause_recover_after_warehouse" -Key $VirtualKey.Escape -AfterMilliseconds 500
        Capture-PlayerWindow -Name "pause_recover_after_warehouse"

        Send-KeyPress -Name "market_open_m" -Key $VirtualKey.M -AfterMilliseconds 80
        Capture-PlayerWindow -Name "market_open_m"
        Send-KeyPress -Name "market_duplicate_m" -Key $VirtualKey.M -AfterMilliseconds 800
        Capture-PlayerWindow -Name "market_duplicate_m"
        Send-KeyPress -Name "market_close_escape" -Key $VirtualKey.Escape -AfterMilliseconds 500
        Capture-PlayerWindow -Name "market_close_escape"

        Send-KeyPress -Name "tavern_open_t" -Key $VirtualKey.T -AfterMilliseconds 80
        Capture-PlayerWindow -Name "tavern_open_t"
        Send-KeyPress -Name "tavern_duplicate_t" -Key $VirtualKey.T -AfterMilliseconds 800
        Capture-PlayerWindow -Name "tavern_duplicate_t"
        Send-KeyPress -Name "tavern_close_escape" -Key $VirtualKey.Escape -AfterMilliseconds 500
        Capture-PlayerWindow -Name "tavern_close_escape"

        Send-KeyPress -Name "pause_open_escape" -Key $VirtualKey.Escape -AfterMilliseconds 800
        Capture-PlayerWindow -Name "pause_open_escape"
        Send-KeyPress -Name "pause_recover_escape" -Key $VirtualKey.Escape -AfterMilliseconds 500
        Capture-PlayerWindow -Name "pause_recover_escape"
    }

    if ($bootstrapRejectRun) {
        $sceneMarkerSeen = Wait-ForPlayerLogMarker -Pattern "InitialHarbor" -TimeoutSeconds $HarborWarmupSeconds
        Add-Step -Name "initial_harbor_log_marker" -Kind "log_observation" -Succeeded $sceneMarkerSeen -Details @{ pattern = "InitialHarbor" }
    }

    if ($process -and -not $process.HasExited) {
        $closeRequested = $process.CloseMainWindow()
        $gracefulExit = $process.WaitForExit($CloseTimeoutSeconds * 1000)
        Add-Step -Name "player_graceful_close" -Kind "process" -Succeeded $gracefulExit -Details @{
            close_main_window_requested = $closeRequested
            graceful_exit = $gracefulExit
        }
        if (-not $gracefulExit) {
            Stop-Process -Id $process.Id -Force
            Add-Step -Name "player_force_close" -Kind "process" -Succeeded $true -Details @{ pid = $process.Id }
        }
    }
}
catch {
    $fatalError = $_.Exception.Message
    Add-Step -Name "probe_error" -Kind "error" -Succeeded $false -Details @{ message = $fatalError }
}
finally {
    if ($process -and -not $process.HasExited) {
        try { $process.CloseMainWindow() | Out-Null } catch { }
        try {
            if (-not $process.WaitForExit(3000)) { Stop-Process -Id $process.Id -Force }
        } catch { }
    }

    if (Test-Path -LiteralPath $probePath) {
        try { $probeResult = Get-Content -LiteralPath $probePath -Raw | ConvertFrom-Json } catch { }
    }
    $probeArchiveExists = $false
    if ($isolatedRouteRun -and (Test-Path -LiteralPath $probePath)) {
        Copy-Item -LiteralPath $probePath -Destination $probeArchivePath -Force
        $probeArchiveExists = Test-Path -LiteralPath $probeArchivePath
    }
    $playerLogText = Read-PlayerLog
    $runLogText = $playerLogText
    if ($bootstrapRejectRun -and -not [string]::IsNullOrEmpty($playerLogText)) {
        $markerIndex = $playerLogText.LastIndexOf($bootstrapLogMarker, [System.StringComparison]::Ordinal)
        if ($markerIndex -ge 0) { $runLogText = $playerLogText.Substring($markerIndex) }
    }
    $playerLogArchiveExists = $false
    if (($bootstrapRejectRun -or $isolatedRouteRun) -and (Test-Path -LiteralPath $playerLogPath)) {
        Copy-Item -LiteralPath $playerLogPath -Destination $playerLogArchivePath -Force
        $playerLogArchiveExists = Test-Path -LiteralPath $playerLogArchivePath
    }
    $eventTypes = @()
    if ($probeResult -and $probeResult.events) {
        $eventTypes = @($probeResult.events | ForEach-Object { $_.type } | Sort-Object -Unique)
    }
    $logMarkers = [ordered]@{
        scene_initial_harbor = [bool]($runLogText -match "InitialHarbor")
        boarded = [bool]($runLogText -match "已进入船只控制")
        disembarked = [bool]($runLogText -match "已退出船只控制")
        market_opened = [bool]($runLogText -match "市场面板已打开|market_opened")
        tavern_opened = [bool]($runLogText -match "酒馆社交面板已打开|tavern_opened")
        warehouse_opened = [bool]($runLogText -match "仓库面板已打开")
        pause_opened = [bool]($runLogText -match "\[HarborPause\] 游戏已暂停")
        pause_resumed = [bool]($runLogText -match "\[HarborPause\] 游戏已恢复")
    }
    $probeObservations = if ($probeResult) { @($probeResult.observations) } else { @() }
    $interactionMatrix = Get-PageStateMatrix -Probe $probeResult -InputSteps $steps.ToArray()
    $bootstrapRejectMatrix = if ($bootstrapRejectRun) {
        Get-BootstrapRejectMatrix -Probe $probeResult -InputSteps $steps.ToArray() -RunLog $runLogText -TargetKey $BootstrapRejectKey
    } else {
        [ordered]@{
            schema = "VC_G01_RUNTIME_BOOTSTRAP_REJECT_MATRIX_V1"
            target_key = "NONE"
            run_marker_found = $false
            matrix_proof = $false
            target_proof = $false
            note = "本次不是 bootstrap reject 专项运行，未对 owner-not-ready reject 作任何推断。"
        }
    }
    $movingObservationCount = @($probeObservations | Where-Object {
        $_.moving -or $_.speed -gt 0.0001
    }).Count
    $probeMaxSpeed = if ($probeObservations.Count -gt 0) {
        [double](($probeObservations | Measure-Object speed -Maximum).Maximum)
    } else { 0.0 }
    $farShipRejectProof = @($probeObservations | Where-Object {
        $_.feedback_code -eq "ship_too_far"
    }).Count -gt 0
    $swimmingObservationCount = @($probeObservations | Where-Object {
        $_.swimming -eq $true
    }).Count
    $swimmingObservationProof = [bool]($probeResult -and
        $probeResult.swimming_observed -and
        $swimmingObservationCount -gt 0)
    $shipObservations = @($probeObservations | Where-Object {
        $_.ship_found -eq $true -and $_.control_mode -eq "Ship"
    })
    $shipMaxDisplacement = if ($probeResult -and $probeResult.PSObject.Properties.Name -contains "ship_max_displacement") {
        [double]$probeResult.ship_max_displacement
    } else { 0.0 }
    $shipMaxHeadingDelta = if ($probeResult -and $probeResult.PSObject.Properties.Name -contains "ship_max_heading_delta") {
        [double]$probeResult.ship_max_heading_delta
    } else { 0.0 }
    $shipControlProof = [bool]($probeResult -and
        $probeResult.ship_control_observed -and
        $shipObservations.Count -ge 2 -and
        ($shipMaxDisplacement -ge 0.05 -or $shipMaxHeadingDelta -ge 1.0))
    $routeProbeProof = [bool]($probeResult -and
        $probeResult.scene -eq "InitialHarbor" -and
        $probeResult.player_found -and
        @($probeResult.observations).Count -gt 0 -and
        @($probeResult.events | Where-Object { $_.type -eq "landmark_reached" }).Count -gt 0 -and
        @($probeResult.events | Where-Object { $_.type -eq "displacement_observed" }).Count -gt 0)
    $businessInteractionProof = [bool]($logMarkers.boarded -and
        $logMarkers.disembarked -and
        $logMarkers.market_opened -and
        $logMarkers.tavern_opened -and
        $logMarkers.warehouse_opened)
    $summary = [ordered]@{
        task_id = "VC-G01"
        status = if ($fatalError) { "INPUT_ATTEMPT_ERROR" } elseif ($bootstrapRejectRun) { "BOOTSTRAP_REJECT_ATTEMPTED" } else { "INPUT_ATTEMPTED" }
        evidence_mode = "real native Win32 keyboard/mouse events sent to the standalone Player; probe remains read-only"
        started_at_local = $startedAt.ToString("o")
        finished_at_local = (Get-Date).ToString("o")
        player_path = $resolvedPlayer
        player_pid = if ($process) { $process.Id } else { $null }
        route_flag = $runArgument
        bootstrap_reject_run = $bootstrapRejectRun
        bootstrap_reject_target = if ($bootstrapRejectRun) { $BootstrapRejectKey } else { $null }
        window_handle = if ($windowHandle -ne [IntPtr]::Zero) { $windowHandle.ToInt64() } else { $null }
        client_width = $clientWidth
        client_height = $clientHeight
        reference_scale = $referenceScale
        scene_marker_seen = $sceneMarkerSeen
        input_steps = $steps.ToArray()
        screenshots = $screenshots.ToArray()
        screenshot_directory = $screenshotDirectory
        player_log_path = $playerLogPath
        player_log_exists = Test-Path -LiteralPath $playerLogPath
        player_log_run_marker = if ($bootstrapRejectRun) { $bootstrapLogMarker } else { $null }
        player_log_archive_path = $playerLogArchivePath
        player_log_archive_exists = $playerLogArchiveExists
        player_log_markers = $logMarkers
        probe_path = $probePath
        probe_exists = Test-Path -LiteralPath $probePath
        probe_archive_path = $probeArchivePath
        probe_archive_exists = $probeArchiveExists
        probe_scene = if ($probeResult) { $probeResult.scene } else { $null }
        probe_player_found = if ($probeResult) { [bool]$probeResult.player_found } else { $false }
        probe_observations = $probeObservations.Count
        probe_moving_observations = $movingObservationCount
        probe_max_speed = $probeMaxSpeed
        water_probe = $waterProbeRun
        probe_swimming_observations = $swimmingObservationCount
        ship_control_observations = $shipObservations.Count
        ship_max_displacement = $shipMaxDisplacement
        ship_max_heading_delta = $shipMaxHeadingDelta
        probe_event_types = $eventTypes
        input_observation_proof = [bool]($probeResult -and $probeResult.scene -eq "InitialHarbor" -and $probeResult.player_found -and @($probeResult.observations).Count -gt 0)
        runtime_route_proof = $routeProbeProof
        business_interaction_proof = $businessInteractionProof
        pause_recovery_proof = [bool]($logMarkers.pause_opened -and $logMarkers.pause_resumed)
        movement_observation_proof = [bool]($movingObservationCount -gt 0)
        swimming_observation_proof = $swimmingObservationProof
        ship_control_proof = $shipControlProof
        far_ship_reject_proof = $farShipRejectProof
        interaction_matrix = $interactionMatrix
        interaction_matrix_proof = [bool]$interactionMatrix.matrix_proof
        bootstrap_reject_matrix = $bootstrapRejectMatrix
        bootstrap_reject_matrix_proof = [bool]$bootstrapRejectMatrix.matrix_proof
        fatal_error = $fatalError
        note = if ($bootstrapRejectRun) {
            "本次是独立 bootstrap reject 专项运行：只发送确认点击后的 I/M/T 真实 OS 事件，要求 probe 同帧 owner 未就绪、D29 reject 日志和无页面打开同时成立；不把本次专项结果当作完整 VC-G01 页面矩阵。"
        } else {
            "本工具只发送真实 OS 键鼠事件并读取只读探针/日志/窗口截图；runtime_route_proof 仅表示真实 Player 路由探针闭环，movement_observation_proof 必须由 moving/速度样本支持，swimming_observation_proof 只在水域路径的真实 swimming 样本存在时成立，interaction_matrix_proof 只表示本次 I/M/T 重复与 Esc 关闭状态矩阵，不把 owner-not-ready 缺口伪造为 PASS。"
        }
    }
    New-Item -ItemType Directory -Force -Path $resultRoot | Out-Null
    $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding UTF8
    $summary | ConvertTo-Json -Depth 8
}
