import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d16_a05_replacement_preflight import validate_manifest  # noqa: E402


def _write(path: Path, content: str = "evidence") -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8")


def _valid_manifest() -> dict:
    return {
        "schema_version": 1,
        "task": "D16-A05-REPLACEMENT",
        "operator": "asset-owner-01",
        "captured_at": "2026-08-13T12:00:00+08:00",
        "raw_artifacts": ["tools/results/external/a05_asset_review.log"],
        "replacements": [{
            "id": "asset_placeholder_test",
            "final_resource_path": "game/Assets/Art/Final/crew_male.fbx",
            "runtime_prefab_path": "game/Assets/Prefabs/Final/crew_male.prefab",
            "lod_profile": "crew_final_lod3",
            "collision_ref": "crew_final_capsule",
            "anchor_refs": ["anchor_crew_root"],
            "test_cases": ["d16-a05-final-crew-load", "d16-a05-final-crew-collision"],
            "source": {
                "kind": "external-asset",
                "author": "Example Studio",
                "reference": "game/Assets/Art/Final/SOURCE.md",
            },
            "license": {
                "status": "CC0-1.0",
                "holder": "Example Studio",
                "reference": "game/Assets/Art/Final/LICENSE.txt",
                "commercial_use": True,
                "modification_allowed": True,
            },
            "authorization": {
                "authorization_id": "auth-example-crew-001",
                "rights_holder": "Example Studio",
                "license_ref": "game/Assets/Art/Final/LICENSE.txt",
                "source_url_or_path": "game/Assets/Art/Final/SOURCE.md",
                "acquired_at": "2026-08-13",
                "commercial_use": True,
                "modification_allowed": True,
                "expiry_or_scope": "CC0-1.0; project final crew model",
                "evidence_ref": "game/Assets/Art/Final/RIGHTS.md",
            },
            "review": {
                "decision": "Approved",
                "reviewer": "asset-review-owner",
                "evidence_refs": ["game/Assets/Art/Final/REVIEW.md"],
            },
        }],
    }


class D16A05ReplacementPreflightTests(unittest.TestCase):
    def _fixture(self):
        temp = tempfile.TemporaryDirectory()
        root = Path(temp.name)
        entry = root / "game/Assets/Resources/ContentRegistry/entries/asset.json"
        _write(entry, json.dumps({
            "id": "asset_placeholder_test",
            "asset_state": "placeholder",
            "production_stage": "prototype_placeholder",
        }))
        for relative in (
            "game/Assets/Art/Final/crew_male.fbx",
            "game/Assets/Prefabs/Final/crew_male.prefab",
            "game/Assets/Art/Final/SOURCE.md",
            "game/Assets/Art/Final/LICENSE.txt",
            "game/Assets/Art/Final/RIGHTS.md",
            "game/Assets/Art/Final/REVIEW.md",
            "tools/results/external/a05_asset_review.log",
        ):
            _write(root / relative)
        return temp, root, entry

    def test_complete_packet_passes_without_writing_registry(self):
        temp, root, entry = self._fixture()
        try:
            before = entry.read_text(encoding="utf-8")
            report = validate_manifest(root, _valid_manifest())
            after = entry.read_text(encoding="utf-8")
        finally:
            temp.cleanup()
        self.assertEqual(report["status"], "PASS")
        self.assertTrue(report["can_apply"])
        self.assertEqual(report["validated_count"], 1)
        self.assertEqual(before, after)

    def test_missing_authorization_and_review_files_are_rejected(self):
        temp, root, _ = self._fixture()
        try:
            manifest = _valid_manifest()
            manifest["replacements"][0]["authorization"]["evidence_ref"] = "missing-rights.md"
            manifest["replacements"][0]["review"]["evidence_refs"] = []
            report = validate_manifest(root, manifest)
        finally:
            temp.cleanup()
        codes = {item["code"] for item in report["issues"]}
        self.assertEqual(report["status"], "REJECTED")
        self.assertIn("authorization_reference_missing", codes)
        self.assertIn("review_evidence_missing", codes)

    def test_placeholder_path_and_traversal_are_rejected(self):
        temp, root, _ = self._fixture()
        try:
            manifest = _valid_manifest()
            record = manifest["replacements"][0]
            record["final_resource_path"] = "game/Assets/Art/Placeholder/ph_crew_male.fbx"
            record["runtime_prefab_path"] = "../outside.prefab"
            report = validate_manifest(root, manifest)
        finally:
            temp.cleanup()
        codes = {item["code"] for item in report["issues"]}
        self.assertEqual(report["status"], "REJECTED")
        self.assertIn("final_path_is_placeholder", codes)
        self.assertIn("unsafe_final_path", codes)

    def test_existing_integrated_entry_cannot_be_replaced_again(self):
        temp, root, entry = self._fixture()
        try:
            data = json.loads(entry.read_text(encoding="utf-8"))
            data["asset_state"] = "integrated"
            entry.write_text(json.dumps(data), encoding="utf-8")
            report = validate_manifest(root, _valid_manifest())
        finally:
            temp.cleanup()
        self.assertEqual(report["status"], "REJECTED")
        self.assertIn("asset_not_placeholder", {item["code"] for item in report["issues"]})


if __name__ == "__main__":
    unittest.main()
