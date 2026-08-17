import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

import d18_catalog_claim_audit as audit  # noqa: E402


def claim_with(claim_id, expected=None, kind=None):
    claim = dict(next(c for c in audit.CLAIMS if c["claim_id"] == claim_id))
    if expected is not None:
        claim["expected"] = expected
    if kind is not None:
        claim["kind"] = kind
    return claim


def write_json(root: Path, rel: str, data) -> None:
    path = root / rel
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False), encoding="utf-8")


class PatchedRootMixin:
    """Point the tool's record sources at a throwaway fixture tree."""

    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.tmp = Path(self._tmp.name)
        self._patch = {
            "REPO_ROOT": self.tmp,
            "INTAKE_CATALOG": "intake.json",
            "LOCAL_DEV_CATALOG": "local_dev.json",
            "REPLACEMENT_MAP": "replacement.json",
            "REGISTRY_DIR": "registry",
            "CANDIDATES_DIR": "candidates",
            "LOCALIZATION_DIR": "localization",
            "POLYHAVEN_SELECTION": "polyhaven.json",
            "FONT_AUDIT": "font.json",
            "SOURCE_CATALOGS": {
                "kenney": "sources/kenney.json",
                "quaternius": "sources/quaternius.json",
                "polyhaven": "sources/polyhaven.json",
                "ambientcg": "sources/ambientcg.json",
            },
        }
        self._originals = {name: getattr(audit, name) for name in self._patch}
        for name, value in self._patch.items():
            setattr(audit, name, value)

    def tearDown(self):
        for name, value in self._originals.items():
            setattr(audit, name, value)
        self._tmp.cleanup()


def make_intake(entries, excluded=None):
    return {
        "eligible_entries": entries,
        "excluded_manifests": excluded or [],
        "counts": {"errors": 0},
    }


class D18CatalogClaimAuditFixtureTests(PatchedRootMixin, unittest.TestCase):
    def test_claim_match_and_diff(self):
        write_json(
            self.tmp,
            "intake.json",
            make_intake([{"manifest_id": "m1", "candidate_file_path": "game/Assets/Art/Candidates/A/a.fbx", "domain": "ui", "intake_id": "i1"}]),
        )
        match = audit.compute_claim(claim_with("intake_eligible_file_total", expected=1))
        self.assertEqual(match["status"], "MATCH")
        diff = audit.compute_claim(claim_with("intake_eligible_file_total", expected=2))
        self.assertEqual(diff["status"], "DIFF")
        self.assertIn("实际 1", diff["reason"])

    def test_duplicate_intake_entries_are_inflated(self):
        dup_entry = {"manifest_id": "m1", "candidate_file_path": "game/Assets/Art/Candidates/A/a.fbx", "domain": "ui", "intake_id": "i1"}
        write_json(self.tmp, "intake.json", make_intake([dup_entry, dict(dup_entry, intake_id="i2")]))
        result = audit.compute_claim(claim_with("intake_duplicate_manifest_file_references", expected=0))
        self.assertEqual(result["status"], "DIFF")
        self.assertIn("重复 candidate_file_path", result["reason"])
        findings = audit.duplicate_inflation()
        intake_finding = next(f for f in findings if f["source"] == "intake_catalog" and f["key_field"] == "candidate_file_path")
        self.assertEqual(intake_finding["raw"], 2)
        self.assertEqual(intake_finding["unique"], 1)
        self.assertEqual(intake_finding["inflated"], 1)

    def test_pending_categories_total_branch(self):
        write_json(
            self.tmp,
            "local_dev.json",
            {"status": "BLOCKED", "local_development_ready_count": 0, "release_approved_count": 0, "pending_categories": [
                {"category": "ui", "candidate_files": 10, "status": "Pending"},
                {"category": "material", "candidate_files": 3, "status": "Pending"},
                {"category": "audio", "candidate_files": 2, "status": "Pending"},
            ]},
        )
        self.assertEqual(audit.compute_claim(claim_with("pending_categories_total", expected=3))["status"], "MATCH")
        self.assertEqual(audit.compute_claim(claim_with("pending_category_material", expected=3))["status"], "MATCH")
        self.assertEqual(audit.compute_claim(claim_with("pending_category_material", expected=4))["status"], "DIFF")

    def test_target_claim_is_unsupported_never_a_completion_count(self):
        result = audit.compute_claim(claim_with("req_vegetation_entries_target"))
        self.assertEqual(result["status"], "UNSUPPORTED")
        self.assertIn("目标", result["reason"])
        self.assertIsNone(result["actual"])

    def test_missing_source_returns_unknown_without_filling(self):
        write_json(self.tmp, "sources/kenney.json", {"records": []})
        write_json(self.tmp, "sources/quaternius.json", {"records": []})
        write_json(self.tmp, "sources/ambientcg.json", {"records": []})
        # polyhaven source intentionally absent
        result = audit.compute_claim(claim_with("source_polyhaven_records"))
        self.assertEqual(result["status"], "UNKNOWN")
        self.assertIn("记录源不存在", result["reason"])
        self.assertIsNone(result["actual"])

    def test_source_catalog_reported_with_valid_state(self):
        write_json(
            self.tmp,
            "sources/kenney.json",
            {"records": [{"canonical_asset_id": "asset_x_v1", "state": "Candidate"}, {"canonical_asset_id": "asset_y_v1", "state": "Candidate"}]},
        )
        result = audit.compute_claim(claim_with("source_kenney_records"))
        self.assertEqual(result["status"], "REPORTED")
        self.assertEqual(result["actual"], 2)

    def test_source_catalog_invalid_state_is_diff(self):
        write_json(
            self.tmp,
            "sources/kenney.json",
            {"records": [{"canonical_asset_id": "asset_x_v1", "state": "Approved"}]},
        )
        result = audit.compute_claim(claim_with("source_kenney_records"))
        self.assertEqual(result["status"], "DIFF")
        self.assertIn("非冻结状态", result["reason"])

    def test_source_catalog_duplicate_ids_are_diff(self):
        write_json(
            self.tmp,
            "sources/kenney.json",
            {"records": [{"canonical_asset_id": "asset_x_v1", "state": "Candidate"}, {"canonical_asset_id": "asset_x_v1", "state": "Candidate"}]},
        )
        result = audit.compute_claim(claim_with("source_kenney_records"))
        self.assertEqual(result["status"], "DIFF")
        self.assertIn("重复 canonical_asset_id", result["reason"])

    def test_registry_counts_and_duplicate_inflation(self):
        write_json(self.tmp, "registry/animal/a.json", {"id": "animal_x", "category": "animal"})
        write_json(self.tmp, "registry/animal/b.json", {"id": "animal_x", "category": "animal"})
        write_json(self.tmp, "registry/animal/c.json", {"id": "animal_y", "category": "animal"})
        write_json(self.tmp, "registry/index.json", {"registry_id": "index", "roots": []})
        self.assertEqual(audit.compute_claim(claim_with("registry_entries", expected=3))["status"], "MATCH")
        result = audit.compute_claim(claim_with("registry_dup_content_id", expected=0))
        self.assertEqual(result["status"], "DIFF")
        finding = next(f for f in audit.duplicate_inflation() if f["source"] == "registry")
        self.assertEqual((finding["raw"], finding["unique"], finding["inflated"]), (3, 2, 1))

    def test_replacement_counts_and_dup_detection(self):
        write_json(
            self.tmp,
            "replacement.json",
            {
                "records": [
                    {"content_id": "animal_x", "placeholder_asset": {"asset_state": "placeholder"}},
                    {"content_id": "animal_x", "placeholder_asset": {"asset_state": "placeholder"}},
                    {"content_id": "animal_y", "placeholder_asset": {"asset_state": "placeholder"}},
                ],
                "audit": {"input_placeholder_count": 3, "output_record_count": 3, "mapped_count": 2, "unmapped_count": 1, "blocked_count": 0},
            },
        )
        self.assertEqual(audit.compute_claim(claim_with("replacement_input", expected=3))["status"], "MATCH")
        result = audit.compute_claim(claim_with("replacement_dup_content_id", expected=0))
        self.assertEqual(result["status"], "DIFF")
        self.assertIn("重复 content_id", result["reason"])

    def test_disk_count_ignores_meta_files(self):
        cand = self.tmp / "candidates" / "Kenney"
        cand.mkdir(parents=True)
        (cand / "a.fbx").write_bytes(b"x")
        (cand / "a.fbx.meta").write_bytes(b"m")
        (cand / "b.png").write_bytes(b"x")
        self.assertEqual(audit.compute_claim(claim_with("disk_candidate_files", expected=2))["status"], "MATCH")
        self.assertEqual(audit.compute_claim(claim_with("disk_candidate_top_dirs", expected=1))["status"], "MATCH")

    def test_per_manifest_observation_reports_difference(self):
        write_json(
            self.tmp,
            "intake.json",
            make_intake(
                [
                    {"manifest_id": "m1", "manifest_path": "manifests/m1.json", "candidate_file_path": "game/Assets/Art/Candidates/A/a.fbx", "domain": "ui", "intake_id": "i1"},
                    {"manifest_id": "m1", "manifest_path": "manifests/m1.json", "candidate_file_path": "game/Assets/Art/Candidates/A/b.fbx", "domain": "ui", "intake_id": "i2"},
                ]
            ),
        )
        write_json(
            self.tmp,
            "manifests/m1.json",
            {"candidate_files": [{"path": "a"}, {"path": "b"}, {"path": "c"}]},
        )
        observations = audit.per_manifest_entry_observation()
        self.assertEqual(len(observations), 1)
        self.assertEqual(observations[0]["manifest_candidate_files"], 3)
        self.assertEqual(observations[0]["eligible_entries"], 2)
        self.assertEqual(observations[0]["difference"], 1)
        self.assertEqual(observations[0]["status"], "INFO")

    def test_localization_key_count(self):
        write_json(
            self.tmp,
            "localization/zh-CN.json",
            {"keys": {"a.b.c": "x", "a.b.d": "y", "e": "z"}},
        )
        write_json(
            self.tmp,
            "localization/en-US.json",
            {"keys": {"a.b.c": "x", "a.b.d": "y", "e": "z"}},
        )
        self.assertEqual(audit.compute_claim(claim_with("l14_localization_keys", expected=3))["status"], "MATCH")

    def test_mismatched_locales_are_reported(self):
        write_json(self.tmp, "localization/zh-CN.json", {"keys": {"a": "x", "b": "y"}})
        write_json(self.tmp, "localization/en-US.json", {"keys": {"a": "x"}})
        result = audit.compute_claim(claim_with("l14_localization_keys", expected=3))
        self.assertEqual(result["status"], "DIFF")
        self.assertIn("各 locale key 数不一致", result["reason"])


class D18CatalogClaimAuditRealDataTests(unittest.TestCase):
    def test_real_repo_full_report_has_no_diffs(self):
        report = audit.full_report()
        self.assertTrue(report["read_only"])
        self.assertGreaterEqual(report["claims_total"], 50)
        self.assertEqual(report["by_status"].get("DIFF", 0), 0)
        self.assertEqual(report["by_status"].get("UNKNOWN", 0), 0)
        self.assertEqual(report["by_status"].get("UNSUPPORTED", 0), 2)

    def test_real_key_claims_match(self):
        for claim_id in (
            "intake_eligible_file_total",
            "intake_candidate_file_total",
            "disk_candidate_files",
            "registry_entries",
            "replacement_input",
            "replacement_mapped",
            "replacement_unmapped",
            "l14_localization_keys",
            "l14_domain_ui",
            "l14_manifest_kenney_ui_v2",
        ):
            result = audit.compute_claim(claim_with(claim_id))
            self.assertEqual(result["status"], "MATCH", f"{claim_id} 应 MATCH：{result}")
            self.assertIsNotNone(result["actual"])

    def test_real_duplicate_inflation_is_zero(self):
        findings = audit.duplicate_inflation()
        for finding in findings:
            if finding["inflated"] is None:
                self.assertIn("记录源不存在", finding["reason"])
            else:
                self.assertEqual(finding["inflated"], 0, finding)
                self.assertEqual(finding["raw"], finding["unique"], finding)


if __name__ == "__main__":
    unittest.main()
