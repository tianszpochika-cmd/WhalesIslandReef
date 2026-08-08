# ============================================================
# rebuild_agentcore_links.ps1 —— 重建 AgentCore → Unity Assets 硬链接
#
# 用途：
#   git checkout / clone 后，Assets/Scripts/Agent/AgentCore/ 下的文件
#   可能被 Git 重建为独立副本（硬链接失效），导致与 game/AgentCore/
#   产生双份漂移。运行本脚本删除 Unity 侧副本并重建 NTFS 硬链接。
#
# 用法（仓库根目录）：
#   powershell -ExecutionPolicy Bypass -File tools/rebuild_agentcore_links.ps1
#
# 注意：
#   - 只在硬链接失效时执行（正常开发不用跑）
#   - 会删除 Unity 侧副本（内容与 AgentCore 相同，安全）
# ============================================================

$ErrorActionPreference = "Stop"

$RepoRoot   = Split-Path -Parent $PSScriptRoot
$SrcDir     = Join-Path $RepoRoot "game\AgentCore"
$DstDir     = Join-Path $RepoRoot "game\Assets\Scripts\Agent\AgentCore"

$files = @(
  "AgentCoreTests.cs", "AgentMemory.cs", "AgentModels.cs",
  "Bootstrap.cs", "CrewAgent.cs", "DialogueSession.cs",
  "LLMClient.cs", "MutinyMonitor.cs"
)

if (!(Test-Path $SrcDir)) { Write-Error "源目录不存在: $SrcDir"; exit 1 }
New-Item -ItemType Directory -Force -Path $DstDir | Out-Null

$rebuilt = 0
foreach ($f in $files) {
  $src = Join-Path $SrcDir $f
  $dst = Join-Path $DstDir $f
  if (!(Test-Path $src)) { Write-Warning "跳过（源缺失）: $f"; continue }

  # 已是硬链接（LinkCount>=2 且指向同一文件）→ 跳过
  $item = Get-Item $dst -ErrorAction SilentlyContinue
  if ($item -and $item.LinkType -eq "HardLink") {
    Write-Output "OK（已是硬链接）: $f"
    continue
  }

  # 删除旧副本（普通文件或失效链接）
  if ($item) { Remove-Item $dst -Force }
  New-Item -ItemType HardLink -Path $dst -Target $src | Out-Null
  Write-Output "REBUILT: $f"
  $rebuilt++
}

Write-Output ""
Write-Output "完成。重建 $rebuilt 个硬链接。"
if ($rebuilt -gt 0) {
  Write-Output "提醒：unity 侧 .meta 文件由 Unity 首次导入时自动生成。"
}
