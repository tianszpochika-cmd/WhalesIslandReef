import json
import tempfile
import unittest
from pathlib import Path

from tools.d18_placeholder_replacement_map import audit_mapping, build_mapping


class D18PlaceholderReplacementMapTests(unittest.TestCase):
    def make_project(self, entries, candidates, evidence=True):
        temp = tempfile.TemporaryDirectory()
        root = Path(temp.name)
        registry = root / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries" / "fixture"
        registry.mkdir(parents=True)
        for entry in entries:
            (registry / (entry["id"] + ".json")).write_text(json.dumps(entry), encoding="utf-8")
        manifest_dir = root / "docs" / "game" / "design" / "common" / "assets" / "manifests"
        manifest_dir.mkdir(parents=True)
        candidate_dir = root / "game" / "Assets" / "Art" / "Candidates"
        candidate_dir.mkdir(parents=True)
        if evidence:
            (candidate_dir / "License.txt").write_text("CC0-1.0", encoding="utf-8")
        for candidate in candidates:
            path = candidate_dir / Path(candidate["path"]).name
            path.write_bytes(b"fixture")
        manifest = {
            "manifest_kind": "d18_asset_pack_manifest",
            "schema_version": 1,
            "source_id": "source_fixture",
            "pack_id": "pack_fixture",
            "title": "Fixture",
            "official_page": "https://example.invalid/fixture",
            "license_id": "CC0-1.0",
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": "game/Assets/Art/Candidates/License.txt",
            "review_status": "Candidate",
            "import_status": "ImportVerified",
            "candidate_files": [dict(candidate, path="game/Assets/Art/Candidates/" + Path(candidate["path"]).name) for candidate in candidates],
            "runtime_consumers": [],
        }
        (manifest_dir / "fixture.manifest.json").write_text(json.dumps(manifest), encoding="utf-8")
        return temp, root

    @staticmethod
    def entry(content_id, category="crew"):
        return {
            "id": content_id,
            "asset_state": "placeholder",
            "category": category,
            "owner_system": "FixtureSystem",
            "resource_path": "game/Assets/Art/placeholder.fbx",
            "model_ref": "placeholder_model",
        }

    def test_mapping_is_deterministic_and_covers_authoritative_ids(self):
        temp, root = self.make_project(
            [self.entry("crew_fixture_001"), self.entry("crew_fixture_002")],
            [{"target_id": "shared_character", "role": "candidate_character_visual", "path": "character.fbx"}],
        )
        self.addCleanup(temp.cleanup)
        first = build_mapping(root)
        second = build_mapping(root)
        self.assertEqual(first["deterministic_digest"], second["deterministic_digest"])
        self.assertEqual([r["content_id"] for r in first["records"]], ["crew_fixture_001", "crew_fixture_002"])
        self.assertEqual(first["audit"] if "audit" in first else None, None)
        audit = audit_mapping(root, first)
        self.assertEqual(audit["input_placeholder_count"], 2)
        self.assertEqual(audit["output_record_count"], 2)
        self.assertEqual(audit["duplicate_content_id_count"], 0)

    def test_many_to_one_reuse_is_legal_and_explicit(self):
        temp, root = self.make_project(
            [self.entry("crew_fixture_001"), self.entry("crew_fixture_002")],
            [{"target_id": "shared_character", "role": "candidate_character_visual", "path": "character.fbx"}],
        )
        self.addCleanup(temp.cleanup)
        mapping = build_mapping(root)
        audit = audit_mapping(root, mapping)
        self.assertEqual(audit["duplicate_candidate_path_count"], 0)
        self.assertGreaterEqual(audit["legal_reuse_path_count"], 1)
        self.assertTrue(all(record["mapping_type"] == "many_to_one_reuse" for record in mapping["records"]))

    def test_missing_candidate_evidence_is_blocked(self):
        temp, root = self.make_project(
            [self.entry("crew_fixture_001")],
            [{"target_id": "shared_character", "role": "candidate_character_visual", "path": "character.fbx"}],
            evidence=False,
        )
        self.addCleanup(temp.cleanup)
        mapping = build_mapping(root)
        record = mapping["records"][0]
        record["status"] = "blocked_candidate_evidence"
        record["target_candidates"] = [{"path": "missing.fbx", "file_exists": False, "license_evidence_exists": False}]
        audit = audit_mapping(root, mapping)
        self.assertEqual(audit["missing_file_count"], 1)
        self.assertEqual(audit["missing_license_evidence_count"], 1)
        self.assertEqual(audit["status"], "BLOCKED")

    def test_illegal_one_to_many_is_reported(self):
        temp, root = self.make_project(
            [self.entry("crew_fixture_001")],
            [{"target_id": "shared_character", "role": "candidate_character_visual", "path": "character.fbx"}],
        )
        self.addCleanup(temp.cleanup)
        mapping = build_mapping(root)
        target = dict(mapping["records"][0]["target_candidates"][0])
        mapping["records"][0]["target_candidates"] = [target, dict(target, candidate_id="second")]
        mapping["records"][0]["mapping_type"] = "one_to_many_composition"
        audit = audit_mapping(root, mapping)
        self.assertEqual(audit["one_to_many_issue_count"], 1)


if __name__ == "__main__":
    unittest.main()
