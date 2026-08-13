import pathlib
import unittest


ROOT = pathlib.Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "tools" / "d16_a03_device_probe.ps1"


class D16A03DeviceProbeTests(unittest.TestCase):
    def test_probe_is_explicitly_non_pass_and_does_not_synthesize_manual_results(self):
        text = SCRIPT.read_text(encoding="utf-8")
        self.assertIn('status = "IN_PROGRESS"', text)
        self.assertIn('physical_input_pass = "PENDING_MANUAL"', text)
        self.assertIn('focus_pass = "PENDING_MANUAL"', text)
        self.assertIn('accessibility_manual_pass = "PENDING_MANUAL"', text)
        self.assertIn("没有发送按钮输入", text)
        self.assertNotIn('status = "PASS"', text)

    def test_probe_records_raw_artifacts_and_physical_device_inventory(self):
        text = SCRIPT.read_text(encoding="utf-8")
        self.assertIn("Get-PnpDevice -PresentOnly", text)
        self.assertIn("raw_artifacts", text)
        self.assertIn("controllers", text)
        self.assertIn("resolutions", text)
        self.assertIn("XInputGetState", text)
        self.assertIn("xinput_connected_users", text)
        self.assertIn("xinput_results", text)
        self.assertIn("NO_ACTIVE_XINPUT_USER", text)
        self.assertIn("xinput_probe_status", text)
        self.assertNotIn('FriendlyName -match "Xbox|Gamepad|Controller|', text)


if __name__ == "__main__":
    unittest.main()
