import json
import subprocess
import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from d16_a01_hardware_baseline import (  # noqa: E402
    collect_baseline,
    parse_nvidia_smi_csv,
)


class D16A01HardwareBaselineTests(unittest.TestCase):
    def test_parser_keeps_gpu_telemetry_numeric_and_handles_na(self):
        devices = parse_nvidia_smi_csv(
            "NVIDIA Test, 600.1, 14, 4807, 12282, 41\n"
            "NVIDIA Headless, 600.1, N/A, N/A, 8192, N/A\n"
        )
        self.assertEqual(devices[0]["utilization_gpu_percent"], 14)
        self.assertEqual(devices[0]["memory_total_mb"], 12282)
        self.assertIsNone(devices[1]["temperature_c"])

    def test_baseline_marks_local_scope_and_never_claims_target(self):
        def fake_runner(*_args, **_kwargs):
            return subprocess.CompletedProcess(
                args=[],
                returncode=0,
                stdout="NVIDIA Test, 600.1, 14, 4807, 12282, 41\n",
                stderr="",
            )

        result = collect_baseline(runner=fake_runner)
        self.assertEqual(result["scope"], "local_host_only")
        self.assertFalse(result["is_target_device_evidence"])
        self.assertFalse(result["is_low_end_device_evidence"])
        self.assertTrue(result["gpu"]["sample_available"])
        self.assertIn("game_frame_gpu_time", result["does_not_measure"])
        json.dumps(result)


if __name__ == "__main__":
    unittest.main()
