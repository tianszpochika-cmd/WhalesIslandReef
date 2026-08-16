import json
import subprocess
import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "tools/d18_r03_s01_review_rerun1.py"
RESULT = ROOT / "game/tools/results/D18/D18-R03-S01/D18_R03_S01_MACHINE_RESULT_RERUN1.json"


class D18R03S01ReviewRerun1Tests(unittest.TestCase):
    def test_review_is_readable_and_keeps_runtime_states_zero(self):
        completed = subprocess.run([sys.executable, str(SCRIPT)], cwd=ROOT, capture_output=True, text=True)
        self.assertEqual(completed.returncode, 0, completed.stdout + completed.stderr)
        result = json.loads(RESULT.read_text(encoding="utf-8"))
        self.assertEqual(result["conclusion"], "PASS")
        self.assertEqual(result["local_development_ready"], 0)
        self.assertEqual(result["release_approved"], 0)
        self.assertTrue(result["source_evidence_ok"])
        self.assertTrue(result["assembly_evidence_ok"])


if __name__ == "__main__":
    unittest.main()
