"""Tests for tools/d18_local_development_guard.py (D18-L36)."""

import contextlib
import io
import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_local_development_guard import evaluate_records, main  # noqa: E402


def ready_record(project_root: Path) -> dict:
    return {
        "canonical_asset_id": "asset_quaternius_male_casual_v01",
        "category": "npc_visual",
        "source_id": "source_quaternius",
        "source_url": "https://quaternius.com/packs/animatedmen.html",
        "original_filename": "Male_Casual.fbx",
        "download_date": "2026-08-14",
        "sha256": "A" * 64,
        "state": "Candidate",
        "license": {
            "license_id": "CC0-1.0",
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": "game/Assets/Art/Candidates/license.txt",
            "attribution_required": False,
        },
        "rights": {
            "commercial_use": True,
            "modification_allowed": True,
            "repository_storage": True,
            "distribution_with_game": True,
        },
        "content_ids": ["crew_quaternius_male_casual_v01"],
        "evidence_paths": ["game/Assets/Art/Candidates/Models/Male_Casual.fbx"],
        "runtime_deferred": [],
    }


def ready_evidence(record_id: str) -> dict:
    return {
        record_id: {
            "c3_provenance": {"status": "PASS", "detail": "ok"},
            "c4_file_matches": True,
            "c7_content_ids_exist": True,
            "resource_keys": ["d10:appearance:outfit_x"],
            "c8_resource_keys_ok": True,
            "descriptors": {"mesh": True, "material": True, "animation": True, "ui_audio": True},
            "c10_state_chain_valid": True,
        }
    }


class D18LocalDevelopmentGuardTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)
        self.addCleanup(self._tmp.cleanup)
        lic = self.root / "game/Assets/Art/Candidates/license.txt"
        lic.parent.mkdir(parents=True, exist_ok=True)
        lic.write_text("CC0-1.0", encoding="utf-8")
        model = self.root / "game/Assets/Art/Candidates/Models/Male_Casual.fbx"
        model.parent.mkdir(parents=True, exist_ok=True)
        model.write_bytes(b"mesh")
        self.record = ready_record(self.root)
        self.record_id = self.record["canonical_asset_id"]
        self.evidence = ready_evidence(self.record_id)

    def evaluate(self, record=None, evidence=None):
        return evaluate_records([record if record is not None else self.record], evidence if evidence is not None else self.evidence, self.root)

    def set_evidence(self, **changes):
        for key, value in changes.items():
            self.evidence[self.record_id][key] = value

    def missing(self, result):
        return result["records"][0]["missing_conditions"]

    def test_full_ready_record_is_ready(self):
        result = self.evaluate()
        self.assertEqual(result["status"], "READY")
        self.assertEqual(result["ready_count"], 1)
        self.assertEqual(result["approved_count"], 0)
        self.assertEqual(result["records"][0]["verdict"], "READY")
        self.assertEqual(self.missing(result), [])
        self.assertIn("unity_import", result["local_development_ready_is_not"])
        self.assertIn("prefab", result["local_development_ready_is_not"])
        self.assertIn("runtime_consumers", result["local_development_ready_is_not"])
        self.assertIn("performance", result["local_development_ready_is_not"])
        self.assertIn("release_approved", result["local_development_ready_is_not"])

    def test_missing_source_is_not_ready(self):
        self.record["download_date"] = None
        result = self.evaluate()
        self.assertEqual(result["records"][0]["verdict"], "NOT_READY")
        self.assertIn("C1", self.missing(result))

    def test_bad_source_url_is_not_ready(self):
        self.record["source_url"] = "http://quaternius.com/packs/animatedmen.html"
        result = self.evaluate()
        self.assertIn("C1", self.missing(result))

    def test_nc_license_is_not_ready(self):
        self.record["license"]["license_id"] = "CC-BY-NC-4.0"
        result = self.evaluate()
        self.assertIn("C2", self.missing(result))

    def test_nd_and_sa_licenses_are_not_ready(self):
        for license_id in ("CC-BY-ND-4.0", "CC-BY-SA-4.0"):
            self.record["license"]["license_id"] = license_id
            result = self.evaluate()
            self.assertIn("C2", self.missing(result), license_id)

    def test_unknown_license_is_not_ready(self):
        self.record["license"]["license_id"] = "Totally-Unknown-License"
        result = self.evaluate()
        self.assertIn("C2", self.missing(result))

    def test_missing_license_evidence_file_is_not_ready(self):
        self.record["license"]["license_evidence_path"] = "game/Assets/missing_license.txt"
        result = self.evaluate()
        self.assertIn("C2", self.missing(result))

    def test_pending_provenance_is_not_ready(self):
        self.set_evidence(c3_provenance={"status": "PENDING"})
        result = self.evaluate()
        self.assertIn("C3", self.missing(result))

    def test_missing_provenance_verdict_is_not_ready(self):
        del self.evidence[self.record_id]["c3_provenance"]
        result = self.evaluate()
        self.assertIn("C3", self.missing(result))

    def test_bad_sha256_is_not_ready(self):
        self.record["sha256"] = "not-a-hash"
        result = self.evaluate()
        self.assertIn("C4", self.missing(result))

    def test_null_sha256_is_not_ready(self):
        self.record["sha256"] = None
        result = self.evaluate()
        self.assertIn("C4", self.missing(result))

    def test_file_mismatch_verdict_is_not_ready(self):
        self.set_evidence(c4_file_matches=False)
        result = self.evaluate()
        self.assertIn("C4", self.missing(result))

    def test_rights_not_explicit_is_not_ready(self):
        self.record["rights"]["distribution_with_game"] = False
        result = self.evaluate()
        self.assertIn("C5", self.missing(result))

    def test_rights_object_missing_is_not_ready(self):
        del self.record["rights"]
        result = self.evaluate()
        self.assertIn("C5", self.missing(result))

    def test_category_outside_taxonomy_is_not_ready(self):
        self.record["category"] = "gadget"
        result = self.evaluate()
        self.assertIn("C6", self.missing(result))

    def test_category_hits_taxonomy_for_all_domains(self):
        for category in ("npc_visual", "ship", "terrestrial_animal", "ui_icon", "material"):
            self.record["category"] = category
            result = self.evaluate()
            self.assertNotIn("C6", self.missing(result), category)

    def test_candidate_content_key_is_not_ready(self):
        self.record["content_ids"] = ["candidate_hair_buzzed_r03p"]
        result = self.evaluate()
        self.assertIn("C7", self.missing(result))

    def test_content_ids_missing_existence_verdict_is_not_ready(self):
        self.set_evidence(c7_content_ids_exist=False)
        result = self.evaluate()
        self.assertIn("C7", self.missing(result))

    def test_empty_content_ids_is_not_ready(self):
        self.record["content_ids"] = []
        result = self.evaluate()
        self.assertIn("C7", self.missing(result))

    def test_path_like_resource_key_is_not_ready(self):
        self.set_evidence(resource_keys=["game/Assets/Models/x.fbx"])
        result = self.evaluate()
        self.assertIn("C8", self.missing(result))

    def test_duplicate_resource_key_is_not_ready(self):
        self.set_evidence(resource_keys=["d10:appearance:outfit_x", "d10:appearance:outfit_x"])
        result = self.evaluate()
        self.assertIn("C8", self.missing(result))

    def test_missing_resource_keys_verdict_is_not_ready(self):
        self.set_evidence(resource_keys=["d10:appearance:outfit_x"], c8_resource_keys_ok=False)
        result = self.evaluate()
        self.assertIn("C8", self.missing(result))

    def test_incomplete_descriptors_is_not_ready(self):
        self.set_evidence(descriptors={"mesh": True, "material": True, "animation": False, "ui_audio": True})
        result = self.evaluate()
        self.assertIn("C9", self.missing(result))

    def test_missing_descriptors_evidence_is_not_ready(self):
        self.set_evidence(descriptors={})
        result = self.evaluate()
        self.assertIn("C9", self.missing(result))

    def test_rejected_state_is_not_ready(self):
        self.record["state"] = "Rejected"
        result = self.evaluate()
        self.assertIn("C10", self.missing(result))

    def test_revoked_state_is_not_ready(self):
        self.record["state"] = "Revoked"
        result = self.evaluate()
        self.assertIn("C10", self.missing(result))

    def test_approved_states_are_not_ready_in_logic_phase(self):
        for state in ("RuntimeApproved", "ReleaseApproved"):
            self.record["state"] = state
            result = self.evaluate()
            self.assertEqual(result["records"][0]["verdict"], "NOT_READY", state)
            self.assertIn("C10", self.missing(result), state)

    def test_unknown_state_is_not_ready(self):
        self.record["state"] = "LocalDevelopmentReady"
        result = self.evaluate()
        self.assertIn("C10", self.missing(result))

    def test_state_chain_verdict_false_is_not_ready(self):
        self.set_evidence(c10_state_chain_valid=False)
        result = self.evaluate()
        self.assertIn("C10", self.missing(result))

    def test_screenshot_only_evidence_never_causes_ready(self):
        self.record["evidence_paths"] = ["game/tools/results/D18/screenshot_rerun1.png", "game/tools/results/D18/player.log"]
        self.record["download_date"] = None
        self.record["sha256"] = None
        self.record["license"]["license_evidence_path"] = "game/tools/results/D18/player.log"
        result = self.evaluate()
        record_out = result["records"][0]
        self.assertEqual(record_out["verdict"], "NOT_READY")
        for name in ("C1", "C2", "C4"):
            self.assertIn(name, self.missing(result))
        self.assertTrue(any("never satisfy any condition" in note for note in record_out["notes"]))

    def test_visual_artifacts_do_not_block_real_ready(self):
        self.record["evidence_paths"] = ["game/Assets/Art/Candidates/Models/Male_Casual.fbx", "game/tools/results/D18/screenshot_rerun1.png"]
        result = self.evaluate()
        record_out = result["records"][0]
        self.assertEqual(record_out["verdict"], "READY")
        self.assertTrue(any("never satisfy any condition" in note for note in record_out["notes"]))

    def test_old_candidate_without_evidence_is_not_ready(self):
        self.record["download_date"] = None
        self.record["sha256"] = None
        result = self.evaluate()
        self.assertEqual(result["records"][0]["verdict"], "NOT_READY")
        for name in ("C1", "C4"):
            self.assertIn(name, self.missing(result))

    def test_historical_r03_pass_records_do_not_cause_ready(self):
        self.record["evidence_paths"] = ["docs/game/design/common/D19_R03_S01_RERUN20_REAL_PLAYER_REVIEW.md"]
        self.record["download_date"] = None
        self.record["sha256"] = None
        result = self.evaluate()
        record_out = result["records"][0]
        self.assertEqual(record_out["verdict"], "NOT_READY")
        self.assertTrue(any("never satisfy any condition" in note for note in record_out["notes"]))
        self.assertTrue(any("rerun" in note.casefold() for note in record_out["notes"]))

    def test_multiple_records_aggregate(self):
        second = dict(self.record)
        second["canonical_asset_id"] = "asset_second"
        second["download_date"] = None
        evidence = dict(self.evidence)
        evidence[second["canonical_asset_id"]] = dict(self.evidence[self.record_id])
        result = evaluate_records([self.record, second], evidence, self.root)
        self.assertEqual(result["status"], "NOT_READY")
        self.assertEqual(result["ready_count"], 1)
        self.assertEqual(result["not_ready_count"], 1)
        self.assertEqual(result["records"][1]["missing_conditions"], ["C1"])

    def test_empty_records_is_not_ready(self):
        result = evaluate_records([], {}, self.root)
        self.assertEqual(result["status"], "NOT_READY")
        self.assertEqual(result["record_count"], 1)
        self.assertEqual(result["records"][0]["verdict"], "NOT_READY")

    def test_invalid_record_is_not_ready(self):
        result = evaluate_records(["not-an-object"], {}, self.root)
        self.assertEqual(result["records"][0]["verdict"], "NOT_READY")

    def test_deterministic_same_input_same_output(self):
        first = evaluate_records([self.record], self.evidence, self.root)
        second = evaluate_records([self.record], self.evidence, self.root)
        self.assertEqual(json.dumps(first, sort_keys=True), json.dumps(second, sort_keys=True))

    def test_read_only_never_touches_input(self):
        before = json.dumps(self.record, sort_keys=True)
        result = self.evaluate()
        self.assertEqual(result["approved_count"], 0)
        self.assertEqual(json.dumps(self.record, sort_keys=True), before)

    def test_cli_exit_codes(self):
        input_path = self.root / "input.json"
        input_path.write_text(json.dumps({"records": [self.record], "condition_evidence": self.evidence}), encoding="utf-8")
        with tempfile.TemporaryDirectory() as temp:
            out = Path(temp) / "result.json"
            with contextlib.redirect_stdout(io.StringIO()):
                code = main(["--input", str(input_path), "--project-root", str(self.root), "--output", str(out)])
            self.assertEqual(code, 0)
            result = json.loads(out.read_text(encoding="utf-8"))
            self.assertEqual(result["status"], "READY")
            self.assertEqual(result["approved_count"], 0)

        self.record["download_date"] = None
        input_path.write_text(json.dumps({"records": [self.record], "condition_evidence": self.evidence}), encoding="utf-8")
        with contextlib.redirect_stdout(io.StringIO()):
            code = main(["--input", str(input_path), "--project-root", str(self.root)])
        self.assertEqual(code, 1)

        with contextlib.redirect_stdout(io.StringIO()):
            code = main(["--input", str(self.root / "missing.json"), "--project-root", str(self.root)])
        self.assertEqual(code, 2)


if __name__ == "__main__":
    unittest.main()
