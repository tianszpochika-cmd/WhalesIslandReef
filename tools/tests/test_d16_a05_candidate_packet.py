import json
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


class D16A05CandidatePacketTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.packet_path = ROOT / "tools" / "results" / "d16_a05_candidate_packet_01_20260813.json"
        cls.packet = json.loads(cls.packet_path.read_text(encoding="utf-8"))

    def test_candidate_packet_is_pending_and_has_no_registry_mutation(self):
        self.assertEqual(self.packet["status"], "CANDIDATE_PENDING_REVIEW")
        self.assertEqual(self.packet["manual_review"]["decision"], "Pending")
        self.assertEqual(self.packet["registry_changes"], [])
        self.assertEqual(self.packet["asset_state_changes"], [])
        self.assertTrue(self.packet["manual_review"]["required"])

    def test_candidate_sources_and_files_exist(self):
        for reference in self.packet["raw_artifacts"]:
            with self.subTest(reference=reference):
                self.assertTrue((ROOT / reference).is_file())
        for candidate in self.packet["candidate_files"]:
            with self.subTest(path=candidate["path"]):
                self.assertTrue((ROOT / candidate["path"]).is_file())


if __name__ == "__main__":
    unittest.main()
