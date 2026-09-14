import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "tools" / "vc_g01_runtime_startup_smoke.ps1"


class VCG01RuntimeStartupSmokeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = SCRIPT.read_text(encoding="utf-8")

    def test_uses_the_explicit_rerun48_route_and_known_workspace_player(self):
        self.assertIn("D19_R03_A01_Player_RERUN48.exe", self.source)
        self.assertIn('"-d19-r03-route-rerun48"', self.source)
        self.assertIn("Resolve-Path (Join-Path $workspaceRoot $PlayerPath)", self.source)

    def test_rejects_only_the_same_player_path_when_already_running(self):
        self.assertIn("同一已知 Player 路径的并发实例", self.source)
        self.assertIn("return ((Resolve-Path -LiteralPath $_.Path", self.source)
        self.assertIn("throw \"VC-G01 startup smoke 拒绝并发运行", self.source)

    def test_closes_the_process_started_by_the_smoke_and_records_force_stop(self):
        self.assertIn("$closeRequested = $process.CloseMainWindow()", self.source)
        self.assertIn("$gracefulExit = $process.WaitForExit", self.source)
        self.assertIn("Stop-Process -Id $process.Id -Force", self.source)
        self.assertIn("force_stopped = $forceStopped", self.source)

    def test_requires_fresh_probe_and_visible_startup_markers_without_claiming_route_proof(self):
        self.assertIn("probe_fresh = $probeFresh", self.source)
        self.assertIn('logHasStartupMarkers = $logText.Contains("[MainMenu] UI 初始化完成")', self.source)
        self.assertIn("runtime_route_proof = $false", self.source)
        self.assertIn('status = "STARTUP_OBSERVED_ONLY"', self.source)

    def test_persists_the_machine_summary_under_the_visual_clean_results(self):
        self.assertIn("VC_G01_STARTUP_SMOKE_20260906.json", self.source)
        self.assertIn("Set-Content -LiteralPath $resolvedSummary", self.source)


if __name__ == "__main__":
    unittest.main()
