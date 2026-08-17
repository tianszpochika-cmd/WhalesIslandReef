"""D18-L45 schema compatibility audit tests (wave 3, serial; waits for L44).

Positive fixtures (compatible):
- every frozen semantic slot agrees (state enum, category enum, schema
  version, required record fields, result status enum);
- an audit over an empty project (fixture slots, no real data) stays PASS;
- every delivered *_LOGIC_RESULT.json is compliant (fields, status, task_id).

Negative fixtures (incompatible):
- an injected inconsistent slot blocks with slot_inconsistent P0;
- the real audit blocks on the known category split (real-data regression:
  source catalogs use 16 category values vs the frozen 11-value enum;
  L42 rejects while L39 aggregates without a gap - cross-tool split).

Contracts: read-only (nothing modified), offline, JSON-serialisable output
that D24 can consume.
"""

import json
import tempfile
import unittest
from pathlib import Path

from tools.d18_schema_compatibility_audit import (
    RESULT_REQUIRED_FIELDS,
    VALID_RESULT_STATUS,
    audit_schema_compatibility,
    check_result_jsons,
    check_slots,
    collect_sources,
)

PROJECT_ROOT = Path(__file__).resolve().parents[2]

FROZEN_STATES = ("Candidate", "IntakeEligible", "Rejected", "ReleaseApproved", "Revoked", "RuntimeApproved", "RuntimeRequested")
FROZEN_CATEGORIES = (
    "animal", "audio", "building", "character", "environment", "fish",
    "lighting_reference", "material", "prop", "ship", "ui",
)
FROZEN_REQUIRED = (
    "canonical_asset_id", "category", "content_ids", "download_date",
    "evidence_paths", "license", "original_filename", "rights",
    "runtime_deferred", "sha256", "source_id", "source_url", "state",
)


def _compatible_slots() -> dict:
    return {
        "state_enum": {"expected": None, "sources": {name: sorted(FROZEN_STATES) for name in ("L02 schema", "L39", "L42", "L18")}},
        "category_enum": {"expected": None, "sources": {name: sorted(FROZEN_CATEGORIES) for name in ("L02 schema", "L42", "L16", "L25")}},
        "schema_version": {"expected": None, "sources": {name: 1 for name in ("L25 schema", "L37 schema", "L42 compiler")}},
        "record_required_fields": {"expected": None, "sources": {"L02 schema": sorted(FROZEN_REQUIRED), "L42 compiler": sorted(FROZEN_REQUIRED)}},
        "result_status_enum": {"expected": None, "sources": {"sync tool": sorted(VALID_RESULT_STATUS)}},
    }


class SchemaCompatibilityAuditTests(unittest.TestCase):
    def setUp(self) -> None:
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self) -> None:
        self._tmp.cleanup()

    # ------------------------------------------------------------------
    # positive fixtures
    # ------------------------------------------------------------------

    def test_all_real_slots_are_consistent(self):
        slots = collect_sources(PROJECT_ROOT)
        checked, findings = check_slots(slots)
        self.assertEqual(len(checked), 5)
        self.assertTrue(all(item["consistent"] for item in checked.values()), checked)
        self.assertEqual(findings, [])
        self.assertEqual(checked["state_enum"]["source_values"]["L02 schema"], sorted(FROZEN_STATES))
        self.assertEqual(checked["category_enum"]["source_values"]["L02 schema"], sorted(FROZEN_CATEGORIES))

    def test_compatible_fixture_over_empty_project_passes(self):
        result = audit_schema_compatibility(self.root, overrides=_compatible_slots())
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["summary"]["consistent_slots"], 5)
        self.assertEqual(result["summary"]["p0_findings"], 0)

    def test_result_jsons_are_compliant(self):
        summary, findings = check_result_jsons(PROJECT_ROOT)
        self.assertGreaterEqual(summary["file_count"], 1)
        self.assertEqual(findings, [])
        self.assertEqual(summary["bad_status_count"], 0)
        self.assertEqual(summary["missing_field_count"], 0)
        self.assertEqual(summary["bad_task_id_count"], 0)

    # ------------------------------------------------------------------
    # negative fixtures
    # ------------------------------------------------------------------

    def test_inconsistent_slot_blocks(self):
        slots = _compatible_slots()
        slots["state_enum"]["sources"]["L39"] = sorted(FROZEN_STATES + ("Approved",))
        checked, findings = check_slots(slots)
        self.assertFalse(checked["state_enum"]["consistent"])
        self.assertEqual(findings[0]["rule"], "slot_inconsistent")
        self.assertEqual(findings[0]["severity"], "P0")
        result = audit_schema_compatibility(self.root, overrides=slots)
        self.assertEqual(result["status"], "BLOCKED")

    def test_real_audit_passes_after_migration(self):
        # L51 双层分类契约后：真实 sources 目录 category 全在 11 值枚举内，
        # category_split_real_data=0、cross_tool_acceptance_split=0、5 槽一致 → PASS。
        result = audit_schema_compatibility(PROJECT_ROOT)
        self.assertEqual(result["status"], "PASS")
        rules = {f["rule"] for f in result["semantic_splits"]}
        self.assertNotIn("category_split_real_data", rules)
        self.assertNotIn("cross_tool_acceptance_split", rules)
        self.assertEqual(result["summary"]["consistent_slots"], 5)
        self.assertEqual(result["summary"]["p0_findings"], 0)
        real = result["real_data"]["source_categories"]
        self.assertEqual(real["extra_categories"], [])

    def test_injected_bad_category_still_blocks(self):
        # 注入错误分类（非法大类直接写入 category）→ 仍 BLOCKED。
        bad_root = self.root / "docs" / "game" / "design" / "common" / "assets" / "sources"
        bad_root.mkdir(parents=True)
        (bad_root / "d18_bad_logic_catalog.json").write_text(json.dumps({
            "catalog_kind": "d18_source_logic_catalog", "schema_version": 1,
            "records": [{
                "canonical_asset_id": "asset_bad_cat_v01",
                "category": "terrestrial_animal",
                "source_id": "source_kenney",
                "source_url": "https://kenney.nl/assets/ui-pack",
                "license": {"license_id": "CC0-1.0", "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
                            "license_evidence_path": "tools/tests/fixtures/d18_logic_valid/evidence/license_cc0.txt",
                            "attribution_required": False},
                "rights": {"commercial_use": True, "modification_allowed": True,
                           "repository_storage": True, "distribution_with_game": True},
                "download_date": "2026-08-16", "original_filename": "x.zip",
                "sha256": "A" * 64, "state": "Candidate",
                "content_ids": ["x_v01"], "evidence_paths": ["tools/tests/fixtures/d18_logic_valid/evidence/license_cc0.txt"],
                "runtime_deferred": [],
            }],
        }, ensure_ascii=False), encoding="utf-8")
        result = audit_schema_compatibility(self.root, overrides=_compatible_slots())
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("category_split_real_data", {f["rule"] for f in result["semantic_splits"]})

    def test_injected_bad_taxonomy_parent_still_blocks(self):
        # 注入错误父类（taxonomy_category 的映射父类 != category）→ L39 P0、L42 拒绝。
        from tools.d18_logic_audit import aggregate_record
        from tools.d18_logic_catalog_compiler import _record_schema_check
        from tools.d18_logic_catalog_compiler import load_taxonomy_map
        taxonomy_map = load_taxonomy_map(PROJECT_ROOT)
        record = {
            "canonical_asset_id": "asset_bad_parent_v01",
            "category": "ship",
            "taxonomy_category": "outfit",
            "source_id": "source_kenney",
            "source_url": "https://kenney.nl/assets/ui-pack",
            "license": {"license_id": "CC0-1.0", "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
                        "license_evidence_path": "tools/tests/fixtures/d18_logic_valid/evidence/license_cc0.txt",
                        "attribution_required": False},
            "rights": {"commercial_use": True, "modification_allowed": True,
                       "repository_storage": True, "distribution_with_game": True},
            "download_date": "2026-08-16", "original_filename": "x.zip",
            "sha256": "A" * 64, "state": "Candidate",
            "content_ids": ["x_v01"], "evidence_paths": ["tools/tests/fixtures/d18_logic_valid/evidence/license_cc0.txt"],
            "runtime_deferred": [],
        }
        _, gaps = aggregate_record(record, taxonomy_map)
        self.assertTrue(any(g["rule"] == "taxonomy_parent_mismatch" and g["severity"] == "P0" for g in gaps))
        ok, _ = _record_schema_check(record, taxonomy_map)
        self.assertFalse(ok)

    def test_injected_unknown_taxonomy_still_blocks(self):
        # 注入未知细分类 → L39 P0、L42 拒绝。
        from tools.d18_logic_audit import aggregate_record
        from tools.d18_logic_catalog_compiler import _record_schema_check
        from tools.d18_logic_catalog_compiler import load_taxonomy_map
        taxonomy_map = load_taxonomy_map(PROJECT_ROOT)
        record = {
            "canonical_asset_id": "asset_unknown_tax_v01",
            "category": "animal",
            "taxonomy_category": "mystery_beast",
            "source_id": "source_kenney",
            "source_url": "https://kenney.nl/assets/ui-pack",
            "license": {"license_id": "CC0-1.0", "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
                        "license_evidence_path": "tools/tests/fixtures/d18_logic_valid/evidence/license_cc0.txt",
                        "attribution_required": False},
            "rights": {"commercial_use": True, "modification_allowed": True,
                       "repository_storage": True, "distribution_with_game": True},
            "download_date": "2026-08-16", "original_filename": "x.zip",
            "sha256": "A" * 64, "state": "Candidate",
            "content_ids": ["x_v01"], "evidence_paths": ["tools/tests/fixtures/d18_logic_valid/evidence/license_cc0.txt"],
            "runtime_deferred": [],
        }
        _, gaps = aggregate_record(record, taxonomy_map)
        self.assertTrue(any(g["rule"] == "unknown_taxonomy_category" and g["severity"] == "P0" for g in gaps))
        ok, _ = _record_schema_check(record, taxonomy_map)
        self.assertFalse(ok)

    def test_findings_never_rewrite_tools(self):
        # 语义分裂只记录 blocker/责任任务，语义中明确"不得批量重写子工具"。
        result = audit_schema_compatibility(PROJECT_ROOT)
        self.assertEqual(result["status"], "PASS")
        for finding in result["semantic_splits"]:
            self.assertTrue(finding["owner_task"])
            self.assertTrue(finding["fix_direction"])

    # ------------------------------------------------------------------
    # contracts
    # ------------------------------------------------------------------

    def test_tool_is_read_only(self):
        before = {p: p.read_text(encoding="utf-8") for p in (PROJECT_ROOT / "tools/schemas").glob("*.json")}
        audit_schema_compatibility(PROJECT_ROOT)
        for path, text in before.items():
            self.assertEqual(path.read_text(encoding="utf-8"), text)

    def test_result_json_serialisable(self):
        result = audit_schema_compatibility(PROJECT_ROOT)
        json.dumps(result, ensure_ascii=False)
        self.assertTrue(all(field in result for field in ("task", "tool", "status", "slots", "semantic_splits", "summary")))


if __name__ == "__main__":
    unittest.main()
