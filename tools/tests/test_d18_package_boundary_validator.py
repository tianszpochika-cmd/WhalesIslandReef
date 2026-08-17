import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_package_boundary_validator import MANIFEST_KIND, discover_manifest_files, validate_pack_boundaries  # noqa: E402


def make_manifest(pack_id, source_id, candidate_files, evidence, review_status="Candidate", release_evidence=None, notes="fixture", **overrides):
    data = {
        "manifest_kind": MANIFEST_KIND,
        "schema_version": 1,
        "source_id": source_id,
        "pack_id": pack_id,
        "title": pack_id,
        "author": "fixture",
        "official_page": "https://example.invalid/pack",
        "download_url": "https://example.invalid/pack",
        "downloaded_at": "2026-08-16",
        "archive_name": "fixture.zip",
        "sha256": "A" * 64,
        "license_id": "CC0-1.0",
        "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
        "license_evidence_path": evidence,
        "attribution_required": False,
        "commercial_use": True,
        "modification_allowed": True,
        "source_redistribution": "allowed",
        "build_distribution": "allowed",
        "category": ["models"],
        "file_count": len(candidate_files),
        "unity_object_count": len(candidate_files),
        "review_status": review_status,
        "import_status": "ImportVerified",
        "candidate_files": [
            {"path": path_value, "role": "fixture", "target_id": f"fixture_{index}"}
            for index, path_value in enumerate(candidate_files)
        ],
        "runtime_consumers": [],
        "notes": notes,
    }
    if release_evidence is not None:
        data["release_evidence_path"] = release_evidence
    data.update(overrides)
    return data


GOOD_PACK = "game/Assets/Art/Candidates/FixtureSource/GoodPack"
GOOD_FILES = [GOOD_PACK + "/Models/a.fbx", GOOD_PACK + "/Models/b.fbx"]
GOOD_EVIDENCE = GOOD_PACK + "/LICENSE.txt"


class BoundaryTree:
    def __init__(self, tmpdir):
        self.root = Path(tmpdir)
        self.manifest_dir = self.root / "docs/game/design/common/assets/packs"

    def write(self, rel, content=b"fixture"):
        path = self.root / rel
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(content if isinstance(content, bytes) else content.encode("utf-8"))
        return path

    def pack_file(self, name, data):
        return self.write("docs/game/design/common/assets/packs/" + name, json.dumps(data, ensure_ascii=False, indent=1))

    def good_pack(self, pack_id="pack_fixture_good_v1", source_id="source_fixture", files=GOOD_FILES, evidence=GOOD_EVIDENCE, create=True, **overrides):
        if create:
            for file_path in files:
                self.write(file_path)
            self.write(evidence)
        return self.pack_file(pack_id + ".manifest.json", make_manifest(pack_id, source_id, files, evidence, **overrides))

    def records(self, entries):
        path = self.root / "records.json"
        path.write_text(json.dumps({"eligible_entries": entries}, ensure_ascii=False, indent=1), encoding="utf-8")
        return path

    def snapshot(self):
        return sorted(
            (str(p.relative_to(self.root)).replace("\\", "/"), p.stat().st_size, int(p.stat().st_mtime))
            for p in self.root.rglob("*")
            if p.is_file()
        )


class D18PackageBoundaryValidatorTests(unittest.TestCase):
    def validate(self, tree, manifests=(), records=None):
        paths = [tree.manifest_dir / name for name in manifests]
        return validate_pack_boundaries(tree.root, paths, records=records)

    def assert_blocked(self, result, *codes):
        self.assertEqual(result["status"], "BLOCKED")
        self.assertTrue(result["read_only"])
        self.assertFalse(result["registry_modified"])
        for code in codes:
            self.assertIn(code, result["issue_counts"], result["issues"])

    def test_legal_pack_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assertEqual(result["status"], "PASS")
            self.assertTrue(result["read_only"])
            self.assertFalse(result["registry_modified"])
            self.assertEqual(result["error_count"], 0)
            self.assertEqual(result["pack_summary"]["passed"], 1)
            self.assertEqual(result["pack_summary"]["failed"], 0)

    def test_two_legal_packs_pass(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            tree.write("game/Assets/Art/Candidates/FixtureSource/SecondPack/Models/c.fbx")
            tree.write("game/Assets/Art/Candidates/FixtureSource/SecondPack/LICENSE.txt")
            tree.pack_file(
                "pack_fixture_second_v1.manifest.json",
                make_manifest(
                    "pack_fixture_second_v1",
                    "source_fixture",
                    ["game/Assets/Art/Candidates/FixtureSource/SecondPack/Models/c.fbx"],
                    "game/Assets/Art/Candidates/FixtureSource/SecondPack/LICENSE.txt",
                ),
            )
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json", "pack_fixture_second_v1.manifest.json"])
            self.assertEqual(result["status"], "PASS")
            self.assertEqual(result["pack_summary"]["passed"], 2)

    def test_evidence_outside_pack_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write("docs/game/design/common/assets/packs/shared_license.txt")
            tree.good_pack(evidence="docs/game/design/common/assets/packs/shared_license.txt")
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "license_evidence_outside_pack")

    def test_evidence_inside_project_but_outside_candidates_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write("tools/notes/license.txt")
            tree.good_pack(evidence="tools/notes/license.txt")
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "license_evidence_outside_pack")

    def test_evidence_file_missing_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack(evidence=GOOD_PACK + "/NOT_PRESENT.txt", create=False)
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "license_evidence_missing")

    def test_evidence_file_empty_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            tree.write(GOOD_EVIDENCE, b"")
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "license_evidence_missing")

    def test_manifest_without_file_list_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write(GOOD_EVIDENCE)
            tree.pack_file("pack_fixture_empty_v1.manifest.json", make_manifest("pack_fixture_empty_v1", "source_fixture", [], GOOD_EVIDENCE))
            result = self.validate(tree, ["pack_fixture_empty_v1.manifest.json"])
            self.assert_blocked(result, "manifest_without_file_list")

    def test_missing_candidate_files_field_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write(GOOD_EVIDENCE)
            data = make_manifest("pack_fixture_nofield_v1", "source_fixture", [GOOD_FILES[0]], GOOD_EVIDENCE)
            data["candidate_files"] = None
            tree.pack_file("pack_fixture_nofield_v1.manifest.json", data)
            result = self.validate(tree, ["pack_fixture_nofield_v1.manifest.json"])
            self.assert_blocked(result, "manifest_without_file_list")

    def test_candidate_entry_invalid_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write(GOOD_EVIDENCE)
            tree.write(GOOD_FILES[0])
            data = make_manifest("pack_fixture_entry_v1", "source_fixture", [GOOD_FILES[0]], GOOD_EVIDENCE)
            data["candidate_files"] = [{"path": GOOD_FILES[0], "role": ""}]
            tree.pack_file("pack_fixture_entry_v1.manifest.json", data)
            result = self.validate(tree, ["pack_fixture_entry_v1.manifest.json"])
            self.assert_blocked(result, "candidate_file_entry_invalid")

    def test_candidate_file_missing_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write(GOOD_EVIDENCE)
            tree.good_pack(files=[GOOD_PACK + "/Models/not_on_disk.fbx"], create=False)
            tree.write(GOOD_EVIDENCE)
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "candidate_file_missing")

    def test_candidate_outside_pack_root_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write("docs/game/design/common/assets/packs/declared.fbx")
            tree.write(GOOD_EVIDENCE)
            tree.good_pack(files=["docs/game/design/common/assets/packs/declared.fbx"])
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "candidate_file_outside_pack")

    def test_unsafe_declared_paths_rejected(self):
        cases = [
            ("C:\\temp\\evil.fbx", "drive_path_rejected"),
            ("/abs/path/evil.fbx", "absolute_path_rejected"),
            (GOOD_PACK + "/../../escape.fbx", "parent_traversal_rejected"),
            ("https://example.com/evil.fbx", "declared_path_unsafe"),
            ("%USERPROFILE%/evil.fbx", "env_var_path_rejected"),
        ]
        for unsafe, code in cases:
            with self.subTest(path=unsafe):
                with tempfile.TemporaryDirectory() as tmp:
                    tree = BoundaryTree(tmp)
                    tree.write(GOOD_EVIDENCE)
                    tree.good_pack(files=[unsafe], create=False)
                    result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
                    self.assert_blocked(result, code)

    def test_incompatible_source_duplicate_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            tree.write(GOOD_PACK + "/LICENSE_B.txt")
            tree.pack_file(
                "pack_fixture_good_b_v1.manifest.json",
                make_manifest("pack_fixture_good_b_v1", "source_other", [GOOD_FILES[0]], GOOD_PACK + "/LICENSE_B.txt"),
            )
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json", "pack_fixture_good_b_v1.manifest.json"])
            self.assert_blocked(result, "file_declared_by_incompatible_sources")
            self.assertEqual(result["pack_summary"]["failed"], 2)

    def test_same_source_multiple_packs_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            tree.write(GOOD_PACK + "/LICENSE_B.txt")
            tree.pack_file(
                "pack_fixture_good_b_v1.manifest.json",
                make_manifest("pack_fixture_good_b_v1", "source_fixture", [GOOD_FILES[0]], GOOD_PACK + "/LICENSE_B.txt"),
            )
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json", "pack_fixture_good_b_v1.manifest.json"])
            self.assert_blocked(result, "file_declared_by_multiple_packs")

    def test_duplicate_within_manifest_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack(files=[GOOD_FILES[0], GOOD_FILES[0]])
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "duplicate_file_in_manifest")

    def test_separator_variant_is_same_file_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            tree.write(GOOD_PACK + "/LICENSE_B.txt")
            backslashed = GOOD_PACK.replace("/", "\\") + "\\Models\\a.fbx"
            tree.pack_file(
                "pack_fixture_good_b_v1.manifest.json",
                make_manifest("pack_fixture_good_b_v1", "source_fixture", [backslashed], GOOD_PACK + "/LICENSE_B.txt"),
            )
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json", "pack_fixture_good_b_v1.manifest.json"])
            self.assert_blocked(result, "file_declared_by_multiple_packs")

    def test_evidence_shared_across_packs_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write("game/Assets/Art/Candidates/FixtureSource/Shared/LICENSE.txt")
            shared = "game/Assets/Art/Candidates/FixtureSource/Shared/LICENSE.txt"
            tree.good_pack(pack_id="pack_fixture_good_v1", evidence=shared)
            tree.write("game/Assets/Art/Candidates/FixtureSource/SecondPack/Models/c.fbx")
            tree.pack_file(
                "pack_fixture_second_v1.manifest.json",
                make_manifest("pack_fixture_second_v1", "source_fixture", ["game/Assets/Art/Candidates/FixtureSource/SecondPack/Models/c.fbx"], shared),
            )
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json", "pack_fixture_second_v1.manifest.json"])
            self.assert_blocked(result, "evidence_shared_across_packs")

    def test_evidence_collides_with_candidate_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write(GOOD_FILES[0])
            tree.good_pack(files=[GOOD_FILES[0]], evidence=GOOD_FILES[0])
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "evidence_collides_with_candidate")

    def test_evidence_points_to_other_pack_file_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write(GOOD_FILES[0])
            tree.write(GOOD_PACK + "/LICENSE_B.txt")
            tree.pack_file(
                "pack_fixture_other_v1.manifest.json",
                make_manifest("pack_fixture_other_v1", "source_fixture", [GOOD_FILES[0]], GOOD_PACK + "/LICENSE_B.txt"),
            )
            tree.pack_file(
                "pack_fixture_evidence_v1.manifest.json",
                make_manifest("pack_fixture_evidence_v1", "source_fixture", [GOOD_PACK + "/Models/own.fbx"], GOOD_FILES[0]),
            )
            tree.write(GOOD_PACK + "/Models/own.fbx")
            result = self.validate(tree, ["pack_fixture_other_v1.manifest.json", "pack_fixture_evidence_v1.manifest.json"])
            self.assert_blocked(result, "evidence_collides_with_other_pack_file")

    def test_evidence_category_mismatch_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.write("game/Assets/Art/Candidates/OtherCategory/Shared/LICENSE.txt")
            tree.good_pack(evidence="game/Assets/Art/Candidates/OtherCategory/Shared/LICENSE.txt")
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "evidence_category_mismatch")

    def test_records_traceable_pass(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            entries = [
                {
                    "intake_id": "intake_1",
                    "manifest_id": "pack_fixture_good_v1",
                    "manifest_path": "docs/game/design/common/assets/packs/pack_fixture_good_v1.manifest.json",
                    "candidate_file_path": GOOD_FILES[0],
                    "license": {"license_evidence_path": GOOD_EVIDENCE},
                },
                {
                    "intake_id": "intake_2",
                    "manifest_id": "pack_fixture_good_v1",
                    "manifest_path": "docs/game/design/common/assets/packs/pack_fixture_good_v1.manifest.json",
                    "candidate_file_path": GOOD_FILES[1],
                    "license": {"license_evidence_path": GOOD_EVIDENCE},
                },
            ]
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"], records=entries)
            self.assertEqual(result["status"], "PASS")
            self.assertEqual(result["record_count"], 2)
            self.assertEqual(result["record_summary"]["passing"], 2)
            self.assertEqual(result["record_summary"]["failed"], 0)

    def test_record_unknown_manifest_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            entries = [
                {
                    "intake_id": "intake_1",
                    "manifest_id": "pack_does_not_exist_v1",
                    "candidate_file_path": GOOD_FILES[0],
                    "license": {"license_evidence_path": GOOD_EVIDENCE},
                }
            ]
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"], records=entries)
            self.assert_blocked(result, "record_manifest_missing")

    def test_record_manifest_path_mismatch_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            entries = [
                {
                    "intake_id": "intake_1",
                    "manifest_id": "pack_fixture_good_v1",
                    "manifest_path": "docs/game/design/common/assets/packs/wrong_name.manifest.json",
                    "candidate_file_path": GOOD_FILES[0],
                    "license": {"license_evidence_path": GOOD_EVIDENCE},
                }
            ]
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"], records=entries)
            self.assert_blocked(result, "record_manifest_path_mismatch")

    def test_record_file_not_declared_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            entries = [
                {
                    "intake_id": "intake_1",
                    "manifest_id": "pack_fixture_good_v1",
                    "manifest_path": "docs/game/design/common/assets/packs/pack_fixture_good_v1.manifest.json",
                    "candidate_file_path": "game/Assets/Art/Candidates/FixtureSource/GoodPack/Models/undeclared.fbx",
                    "license": {"license_evidence_path": GOOD_EVIDENCE},
                }
            ]
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"], records=entries)
            self.assert_blocked(result, "record_file_not_declared_by_pack")

    def test_record_evidence_mismatch_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            entries = [
                {
                    "intake_id": "intake_1",
                    "manifest_id": "pack_fixture_good_v1",
                    "manifest_path": "docs/game/design/common/assets/packs/pack_fixture_good_v1.manifest.json",
                    "candidate_file_path": GOOD_FILES[0],
                    "license": {"license_evidence_path": "game/Assets/Art/Candidates/FixtureSource/GoodPack/OTHER.txt"},
                }
            ]
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"], records=entries)
            self.assert_blocked(result, "record_evidence_mismatch")

    def test_record_evidence_missing_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            entries = [
                {
                    "intake_id": "intake_1",
                    "manifest_id": "pack_fixture_good_v1",
                    "manifest_path": "docs/game/design/common/assets/packs/pack_fixture_good_v1.manifest.json",
                    "candidate_file_path": GOOD_FILES[0],
                    "license": {},
                }
            ]
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"], records=entries)
            self.assert_blocked(result, "record_evidence_missing")

    def test_records_empty_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"], records=[])
            self.assert_blocked(result, "records_empty")

    def test_no_manifests_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            result = validate_pack_boundaries(tree.root, [tree.manifest_dir])
            self.assert_blocked(result, "no_manifests")

    def test_release_evidence_escape_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack(review_status="ReleaseApproved", release_evidence="../outside.txt")
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "parent_traversal_rejected")

    def test_release_evidence_drive_path_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack(review_status="ReleaseApproved", release_evidence="C:\\outside\\review.txt")
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json"])
            self.assert_blocked(result, "drive_path_rejected")

    def test_manifest_read_error_reported(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            broken = tree.manifest_dir / "pack_broken_v1.manifest.json"
            broken.parent.mkdir(parents=True, exist_ok=True)
            broken.write_text("{not json", encoding="utf-8")
            result = validate_pack_boundaries(tree.root, [broken])
            self.assert_blocked(result, "manifest_json_error", "no_manifests")

    def test_tool_read_only_never_writes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = BoundaryTree(tmp)
            tree.good_pack()
            tree.write("game/Assets/Art/Candidates/FixtureSource/SecondPack/Models/c.fbx")
            tree.write("game/Assets/Art/Candidates/FixtureSource/SecondPack/LICENSE.txt")
            tree.pack_file(
                "pack_fixture_second_v1.manifest.json",
                make_manifest(
                    "pack_fixture_second_v1",
                    "source_fixture",
                    ["game/Assets/Art/Candidates/FixtureSource/SecondPack/Models/c.fbx"],
                    "game/Assets/Art/Candidates/FixtureSource/SecondPack/LICENSE.txt",
                ),
            )
            before = tree.snapshot()
            result = self.validate(tree, ["pack_fixture_good_v1.manifest.json", "pack_fixture_second_v1.manifest.json"])
            after = tree.snapshot()
            self.assertEqual(result["status"], "PASS")
            self.assertTrue(result["read_only"])
            self.assertFalse(result["registry_modified"])
            self.assertEqual(before, after)

    def test_real_repo_scan_detects_out_of_pack_evidence(self):
        packs_root = ROOT / "docs/game/design/common/assets/packs"
        manifests_root = ROOT / "docs/game/design/common/assets/manifests"
        catalog = ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"
        paths = discover_manifest_files(packs_root, [manifests_root])
        records = (json.loads(catalog.read_text(encoding="utf-8")) or {}).get("eligible_entries", [])
        result = validate_pack_boundaries(ROOT, paths, records=records)
        self.assertTrue(result["read_only"])
        self.assertFalse(result["registry_modified"])
        self.assertEqual(result["manifest_count"], 30)
        self.assertEqual(result["record_count"], len(records))
        self.assertIn("license_evidence_outside_pack", result["issue_counts"])
        expected_failed = []
        for path in paths:
            data = json.loads(path.read_text(encoding="utf-8"))
            if not isinstance(data, dict) or data.get("manifest_kind") != MANIFEST_KIND:
                continue
            evidence = (data.get("license_evidence_path") or "")
            if not evidence.replace("\\", "/").casefold().startswith("game/Assets/Art/Candidates/".casefold()):
                expected_failed.append(data.get("pack_id"))
        self.assertEqual(result["pack_summary"]["failed"], len(expected_failed))
        self.assertEqual(set(result["pack_summary"]["failed_pack_ids"]), set(expected_failed))
        self.assertEqual(result["record_summary"]["failed"], 0)
        self.assertEqual(result["record_summary"]["passing"], len(records))
        self.assertEqual(result["status"], "BLOCKED" if result["error_count"] else "PASS")


if __name__ == "__main__":
    unittest.main()
