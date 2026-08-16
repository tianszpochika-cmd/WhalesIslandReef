import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[3]
AUDIT = ROOT / "game/tools/results/D19/D19-A06/D19_A06_STATIC_AUDIT.json"


class D19A06AmbienceStaticAuditTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.audit = json.loads(AUDIT.read_text(encoding="utf-8"))

    def test_scope_and_counts(self):
        counts = self.audit["counts"]
        self.assertEqual(counts["prop_prefabs"], 4)
        self.assertEqual(counts["vfx_prefabs"], 5)
        self.assertEqual(counts["ambience_zone_count"], 5)
        self.assertEqual(counts["audio_candidates"], 30)
        self.assertEqual(counts["screenshots_static_plates"], 5)

    def test_audio_is_licensed_and_measured(self):
        records = self.audit["audio"]["records"]
        self.assertEqual(len(records), 30)
        self.assertTrue(all(record["source_exists"] for record in records))
        self.assertTrue(all(record["destination_exists"] for record in records))
        self.assertTrue(all(record["license_id"] == "CC0-1.0" for record in records))
        self.assertTrue(all(record["channels"] >= 1 for record in records))
        self.assertTrue(all(record["sample_rate_hz"] > 0 for record in records))
        self.assertTrue(all(record["loop"] is False and record["loop_point"] is None for record in records))
        self.assertEqual(len(self.audit["audio"]["duplicate_hashes_in_d19_copy"]), 2)
        self.assertIn("source-to-work-copy duplication", self.audit["audio"]["duplicate_policy"])

    def test_scene_has_no_uncontrolled_audio_or_blocking_colliders(self):
        scene = self.audit["scene"]
        self.assertFalse(scene["has_direct_audio_source"])
        self.assertFalse(scene["non_trigger_colliders_found"])
        self.assertEqual(scene["forbidden_tokens_found"], [])
        self.assertTrue(scene["initial_harbor_untouched_in_git_status"])

    def test_route_clearance_and_static_plates(self):
        self.assertTrue(all(item["pass"] for item in self.audit["route_checks"]))
        for screenshot in self.audit["validation"]["screenshots"]:
            self.assertTrue((ROOT / screenshot).exists(), screenshot)

    def test_unavailable_runtime_is_explicit(self):
        self.assertEqual(self.audit["status"], "PARTIAL")
        self.assertIn("BLOCKED_UNAVAILABLE", self.audit["validation"]["unity_compile_and_test"])
        self.assertFalse(self.audit["audio"]["environment_layers_ready"])


if __name__ == "__main__":
    unittest.main()
