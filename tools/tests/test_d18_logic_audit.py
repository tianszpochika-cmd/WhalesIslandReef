"""Unit tests for the D18-L39 logic audit aggregator (read-only, offline).

Positive fixtures:
- every frozen L02..L38 deliverable is present and schemas/catalogs are valid JSON
- every validator/guard/audit entry passes the fixed-fixture smoke (no swallowed errors)
- a complete record has no P0/P1 gaps
- the full audit over the real source catalogs stays PASS (no approval states)

Negative fixtures:
- a missing deliverable is detected and counted (never forged into PASS)
- a missing module / entry fails the smoke and the task id is listed
- an approval state (RuntimeApproved) or invalid state inside the logic phase
  blocks the whole audit with state issues
- an unreadable input catalog blocks the audit
- a record without license/rights/content/evidence/source produces field gaps

The tool writes nothing to Unity, assets or catalogs (read-only contract) and
its result is always JSON-serialisable.
"""

import json
import tempfile
import unittest
from pathlib import Path

from tools.d18_logic_audit import (
    REGISTRY,
    Entry,
    aggregate_catalogs,
    aggregate_record,
    check_deliverables,
    default_catalog_paths,
    load_taxonomy_map,
    run_audit,
    run_smoke,
    _min_record,
)

REAL_ROOT = Path(__file__).resolve().parents[2]

REAL_TAXONOMY_MAP = {
    "terrestrial_animal": "animal",
    "aquatic_animal": "animal",
    "bird": "animal",
    "pet": "animal",
    "npc_visual": "character",
    "player_visual": "character",
    "hair": "character",
    "outfit": "character",
}


def _catalog(*records: dict) -> dict:
    return {"catalog_kind": "d18_source_logic_catalog", "schema_version": 1, "records": list(records)}


def _broken_record() -> dict:
    record = dict(_min_record())
    record["license"] = None
    record.pop("rights", None)
    record["content_ids"] = []
    record["evidence_paths"] = []
    record["source_url"] = "not-a-url"
    record["sha256"] = None
    return record


class LogicAuditTests(unittest.TestCase):
    def setUp(self) -> None:
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self) -> None:
        self._tmp.cleanup()

    def _write_catalog(self, catalog: dict, name: str = "catalog.json") -> Path:
        path = self.root / name
        path.write_text(json.dumps(catalog, ensure_ascii=False), encoding="utf-8")
        return path

    # ------------------------------------------------------------------
    # positive fixtures
    # ------------------------------------------------------------------

    def test_all_deliverables_present(self):
        result = check_deliverables(REAL_ROOT)
        self.assertEqual(result["total"], len(REGISTRY))
        self.assertEqual(result["missing_count"], 0)
        self.assertTrue(all(e["present"] for e in result["entries"]))

    def test_all_smoke_entries_pass(self):
        result = run_smoke(REAL_ROOT)
        self.assertEqual(result["failed_count"], 0)
        self.assertEqual(result["total"], len(REGISTRY))

    def test_complete_record_has_no_gaps(self):
        summary, gaps = aggregate_record(_min_record())
        self.assertEqual(gaps, [])
        self.assertEqual(summary["gap_count"], 0)

    def test_full_audit_over_real_catalogs_passes(self):
        result = run_audit(REAL_ROOT)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["blocked_reasons"], [])
        self.assertGreaterEqual(result["aggregate"]["record_count"], 1)
        self.assertEqual(result["aggregate"]["state_issues"], [])

    def test_default_catalog_paths_finds_sources(self):
        paths = default_catalog_paths(REAL_ROOT)
        self.assertEqual(len(paths), 4)

    # ------------------------------------------------------------------
    # negative fixtures
    # ------------------------------------------------------------------

    def test_missing_deliverable_is_counted(self):
        fake = [
            Entry("D18-L02", "schema", "tools/schemas/d18_logic_asset_record.schema.json"),
            Entry("D18-L99", "validator", "tools/does_not_exist.py"),
        ]
        result = check_deliverables(REAL_ROOT, fake)
        self.assertEqual(result["missing_count"], 1)
        self.assertFalse(result["entries"][1]["present"])

    def test_missing_module_fails_smoke_and_lists_task(self):
        fake = [
            Entry("D18-L99", "validator", "tools/missing_module.py", "missing_module", "entry",
                  None, lambda m, r: m.entry())
        ]
        result = run_smoke(REAL_ROOT, fake)
        self.assertEqual(result["failed_count"], 1)
        self.assertEqual(result["results"][0]["smoke"], "FAIL")
        self.assertIn("error", result["results"][0])

    def test_missing_entry_fails_smoke(self):
        fake = [
            Entry("D18-L99", "validator", "tools/d18_license_normalizer.py", "d18_license_normalizer",
                  "no_such_entry", None, lambda m, r: m.no_such_entry())
        ]
        result = run_smoke(REAL_ROOT, fake)
        self.assertEqual(result["failed_count"], 1)

    def test_missing_interface_blocks_audit_with_task_id(self):
        fake = [
            Entry("D18-L99", "validator", "tools/missing_module.py", "missing_module", "entry",
                  None, lambda m, r: m.entry())
        ]
        result = run_audit(self.root, registry=fake)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertTrue(any("D18-L99" in reason for reason in result["blocked_reasons"]))

    def test_approval_state_in_logic_phase_blocks(self):
        record = dict(_min_record())
        record["state"] = "RuntimeApproved"
        catalog = self._write_catalog(_catalog(record))
        result = run_audit(REAL_ROOT, catalog_paths=[catalog])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertTrue(any("RuntimeApproved" in g["message"] for g in result["aggregate"]["gaps"]))
        self.assertEqual(len(result["aggregate"]["state_issues"]), 1)

    def test_invalid_state_blocks(self):
        record = dict(_min_record())
        record["state"] = "LocalDevelopmentReady"
        catalog = self._write_catalog(_catalog(record))
        result = run_audit(REAL_ROOT, catalog_paths=[catalog])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertTrue(any(g["rule"] == "state_invalid" for g in result["aggregate"]["gaps"]))

    def test_broken_record_produces_field_gaps(self):
        catalog = self._write_catalog(_catalog(_broken_record()))
        result = aggregate_catalogs(REAL_ROOT, [catalog])
        self.assertEqual(result["record_count"], 1)
        rules = {g["rule"] for g in result["gaps"]}
        self.assertIn("missing_license", rules)
        self.assertIn("missing_rights", rules)
        self.assertIn("missing_content_mapping", rules)
        self.assertIn("missing_evidence", rules)
        self.assertIn("missing_source", rules)
        self.assertIn("hash_pending", rules)

    def test_unreadable_catalog_blocks(self):
        catalog = self.root / "bad.json"
        catalog.write_text("{not json", encoding="utf-8")
        result = run_audit(self.root, catalog_paths=[catalog])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertEqual(result["aggregate"]["catalog_json_invalid_count"], 1)

    # ------------------------------------------------------------------
    # two-layer category contract (D18-L51)
    # ------------------------------------------------------------------

    def test_old_record_without_taxonomy_is_legal(self):
        # 没有 taxonomy_category 的既有大类记录仍然合法。
        _, gaps = aggregate_record(_min_record(), REAL_TAXONOMY_MAP)
        self.assertEqual(gaps, [])

    def test_all_eight_real_taxonomy_categories_are_legal(self):
        # 8 个真实细分类的父类映射合法（21 animal + 40 character）。
        for taxonomy, parent in REAL_TAXONOMY_MAP.items():
            with self.subTest(taxonomy=taxonomy):
                record = dict(_min_record())
                record["category"] = parent
                record["taxonomy_category"] = taxonomy
                _, gaps = aggregate_record(record, REAL_TAXONOMY_MAP)
                self.assertEqual(gaps, [])

    def test_unknown_taxonomy_category_blocks(self):
        record = dict(_min_record())
        record["category"] = "animal"
        record["taxonomy_category"] = "mystery_beast"
        _, gaps = aggregate_record(record, REAL_TAXONOMY_MAP)
        rules = {g["rule"] for g in gaps}
        self.assertIn("unknown_taxonomy_category", rules)
        self.assertEqual(next(g for g in gaps if g["rule"] == "unknown_taxonomy_category")["severity"], "P0")

    def test_taxonomy_parent_mismatch_blocks(self):
        record = dict(_min_record())
        record["category"] = "ship"
        record["taxonomy_category"] = "outfit"
        _, gaps = aggregate_record(record, REAL_TAXONOMY_MAP)
        rules = {g["rule"] for g in gaps}
        self.assertIn("taxonomy_parent_mismatch", rules)
        self.assertEqual(next(g for g in gaps if g["rule"] == "taxonomy_parent_mismatch")["severity"], "P0")

    def test_illegal_big_category_blocks_like_compiler(self):
        # L39 与 L42 同语义：非法大类（细分类直接写入 category）→ P0。
        record = dict(_min_record())
        record["category"] = "outfit"
        _, gaps = aggregate_record(record, REAL_TAXONOMY_MAP)
        rules = {g["rule"] for g in gaps}
        self.assertIn("category_invalid", rules)
        self.assertEqual(next(g for g in gaps if g["rule"] == "category_invalid")["severity"], "P0")

    def test_taxonomy_map_file_loads_from_real_root(self):
        taxonomy_map = load_taxonomy_map(REAL_ROOT)
        self.assertEqual(taxonomy_map, REAL_TAXONOMY_MAP)
        self.assertEqual(len(taxonomy_map), 8)

    def test_real_sources_audit_has_zero_p0_after_migration(self):
        # 迁移后真实来源：161 记录、P0=0、4 条 hash_pending 保留为 P1。
        result = run_audit(REAL_ROOT)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["aggregate"]["record_count"], 161)
        self.assertEqual(result["aggregate"]["p0_count"], 0)
        self.assertEqual(result["aggregate"]["p1_count"], 4)
        self.assertEqual(result["aggregate"]["gap_counts"], {"hash_pending": 4})

    # ------------------------------------------------------------------
    # contracts
    # ------------------------------------------------------------------

    def test_tool_is_read_only(self):
        catalog = self._write_catalog(_catalog(_min_record()))
        before = catalog.read_text(encoding="utf-8")
        aggregate_catalogs(REAL_ROOT, [catalog])
        self.assertEqual(catalog.read_text(encoding="utf-8"), before)

    def test_result_json_serialisable(self):
        result = run_audit(REAL_ROOT, catalog_paths=[self._write_catalog(_catalog(_min_record()))])
        json.dumps(result, ensure_ascii=False)


if __name__ == "__main__":
    unittest.main()
