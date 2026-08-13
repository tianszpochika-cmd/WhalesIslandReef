import json
import os
import shutil
import subprocess
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "tools" / "d16_a06_install_update.ps1"
ARTIFACTS = ROOT / "artifacts"


def powershell_executable():
    for candidate in ("pwsh", "powershell"):
        resolved = shutil.which(candidate)
        if resolved:
            return resolved
    return None


class D16A06InstallUpdateScriptTests(unittest.TestCase):
    @unittest.skipUnless(powershell_executable(), "Windows PowerShell is required for the deployment script contract test")
    def test_install_update_uninstall_and_existing_install_guard(self):
        staging = Path(tempfile.mkdtemp(prefix="d16_a06_script_contract_", dir=ARTIFACTS))
        try:
            build = staging / "build"
            install = staging / "installed"
            user_data = staging / "user_data"
            build.mkdir(parents=True)
            exe = build / "WhalesIslandReef.exe"
            exe.write_bytes(b"build-v1")

            install_result = self.run_script("Install", build, install, user_data)
            self.assertEqual(install_result["status"], "PASS")
            self.assertEqual((install / "WhalesIslandReef.exe").read_bytes(), b"build-v1")

            save = user_data / "save.db"
            save.write_bytes(b"save-sentinel")
            exe.write_bytes(b"build-v2")

            update_result = self.run_script("Update", build, install, user_data)
            self.assertEqual(update_result["status"], "PASS")
            self.assertEqual((install / "WhalesIslandReef.exe").read_bytes(), b"build-v2")
            self.assertEqual(save.read_bytes(), b"save-sentinel")

            existing_install = self.run_script_raw("Install", build, install, user_data)
            self.assertNotEqual(existing_install.returncode, 0)
            self.assertEqual((install / "WhalesIslandReef.exe").read_bytes(), b"build-v2")
            self.assertTrue(save.is_file())

            uninstall_result = self.run_script("Uninstall", build, install, user_data)
            self.assertEqual(uninstall_result["status"], "PASS")
            self.assertFalse(install.exists())
            self.assertEqual(save.read_bytes(), b"save-sentinel")
        finally:
            shutil.rmtree(staging, ignore_errors=True)

    def run_script(self, operation, build, install, user_data):
        completed = self.run_script_raw(operation, build, install, user_data)
        self.assertEqual(completed.returncode, 0, completed.stdout + completed.stderr)
        return json.loads(completed.stdout.strip())

    def run_script_raw(self, operation, build, install, user_data):
        command = [
            powershell_executable(),
            "-NoProfile",
            "-ExecutionPolicy",
            "Bypass",
            "-File",
            os.fspath(SCRIPT),
            "-Operation",
            operation,
            "-BuildDir",
            os.fspath(build),
            "-InstallDir",
            os.fspath(install),
            "-UserDataDir",
            os.fspath(user_data),
        ]
        return subprocess.run(command, cwd=ROOT, capture_output=True, text=True, check=False)


if __name__ == "__main__":
    unittest.main()
