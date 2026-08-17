"""D18-L42 logic catalog compiler tests (wave 3, serial; waits for L41).

Positive fixtures:
- the four real source catalogs compile deterministically (two runs produce
  byte-identical output), with stable sort and unique ids;
- pending-hash records (sha256=null) are preserved and counted;
- Rejected/Revoked-state records are preserved (never dropped);
- identical records merged from several sources are deduplicated;
- the output contains no timestamp (deterministic contract).

Negative fixtures:
- Schema-illegal records are rejected with a stable reason and never enter
  the compiled records;
- conflicting declarations of the same canonical_asset_id are excluded and
  listed in conflicts;
- unreadable / non-JSON sources are rejected (exit code 2);
- the real fine-grained categories (outfit / terrestrial_animal / ...) are
  rejected against the D18-L02 11-value enum (observed real-data status,
  tracked for D18-L45 alignment).

Contracts:
- the compiler never writes Unity Assets, Prefabs, Registry, approval states
  or download caches; the only writable destination is its own --output;
- source files are never modified (read-only contract).
"""

import json
import tempfile
import unittest
from pathlib import Path

from tools.d18_logic_catalog_compiler import (
    compile_catalogs,
    default_source_paths,
    load_taxonomy_map,
    main,
    _record_schema_check,
)

PROJECT_ROOT = Path(__file__).resolve().parents[2]
SOURCES_DIR = PROJECT_ROOT / "docs/game/design/common/assets/sources"

# D18-L51 双层分类契约迁移后：161 条来源记录全部合法编译。
REAL_COMPILED = 161
REAL_REJECTED = 0
REAL_PENDING_HASH = 4

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


def _valid_record(asset_id: str = "asset_ship_medium_v01", **overrides: object) -> dict:
    record = {
        "canonical_asset_id": asset_id,
        "category": "ship",
        "source_id": "source_kenney",
        "source_url": "https://kenney.nl/assets/pirate-kit",
        "license": {
            "license_id": "CC0-1.0",
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": "tools/tests/fixtures/d18_logic_valid/evidence/license_cc0.txt",
            "attribution_required": False,
        },
        "rights": {
            "commercial_use": True,
            "modification_allowed": True,
            "repository_storage": True,
            "distribution_with_game": True,
        },
        "download_date": "2026-08-16",
        "original_filename": "fixture_ship_pack.zip",
        "sha256": "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
        "state": "Candidate",
        "content_ids": ["ship_medium_v01"],
        "evidence_paths": ["tools/tests/fixtures/d18_logic_valid/evidence/license_cc0.txt"],
        "runtime_deferred": [],
    }
    record.update(overrides)
    return record


def _write_source(root: Path, name: str, records: list[dict]) -> Path:
    path = root / name
    path.write_text(json.dumps({"catalog_kind": "d18_source_logic_catalog", "schema_version": 1, "records": records}, ensure_ascii=False), encoding="utf-8")
    return path


class LogicCatalogCompilerTests(unittest.TestCase):
    def setUp(self) -> None:
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self) -> None:
        self._tmp.cleanup()

    # ------------------------------------------------------------------
    # positive fixtures
    # ------------------------------------------------------------------

    def test_real_sources_compile_deterministically(self):
        sources = default_source_paths(PROJECT_ROOT)
        self.assertEqual(len(sources), 4)
        first = compile_catalogs(sources)
        second = compile_catalogs(sources)
        self.assertEqual(
            json.dumps(first, sort_keys=True, ensure_ascii=False),
            json.dumps(second, sort_keys=True, ensure_ascii=False),
        )
        summary = first["audit_summary"]
        self.assertEqual(summary["compiled_records"], REAL_COMPILED)
        self.assertEqual(summary["rejected_records"], REAL_REJECTED)
        self.assertEqual(summary["pending_hash_records"], REAL_PENDING_HASH)
        self.assertEqual(summary["conflicts"], 0)
        self.assertEqual(summary["source_errors"], [])

    def test_output_is_sorted_and_unique(self):
        sources = default_source_paths(PROJECT_ROOT)
        result = compile_catalogs(sources)
        ids = [record["canonical_asset_id"] for record in result["records"]]
        self.assertEqual(ids, sorted(ids))
        self.assertEqual(len(ids), len(set(ids)))

    def test_pending_hash_records_are_preserved(self):
        record = _valid_record(asset_id="asset_pending_hash_v01", sha256=None)
        source = _write_source(self.root, "s1.json", [record])
        result = compile_catalogs([source])
        self.assertEqual(result["audit_summary"]["compiled_records"], 1)
        self.assertEqual(result["audit_summary"]["pending_hash_records"], 1)
        self.assertEqual(result["records"][0]["sha256"], None)

    def test_rejected_state_record_is_preserved(self):
        record = _valid_record(asset_id="asset_rejected_v01", state="Rejected")
        source = _write_source(self.root, "s1.json", [record])
        result = compile_catalogs([source])
        self.assertEqual(result["audit_summary"]["compiled_records"], 1)
        self.assertEqual(result["records"][0]["state"], "Rejected")

    def test_revoked_state_record_is_preserved(self):
        record = _valid_record(asset_id="asset_revoked_v01", state="Revoked")
        source = _write_source(self.root, "s1.json", [record])
        result = compile_catalogs([source])
        self.assertEqual(result["records"][0]["state"], "Revoked")

    def test_identical_duplicate_is_merged(self):
        record = _valid_record()
        first = _write_source(self.root, "s1.json", [record])
        second = _write_source(self.root, "s2.json", [record])
        result = compile_catalogs([first, second])
        summary = result["audit_summary"]
        self.assertEqual(summary["compiled_records"], 1)
        self.assertEqual(summary["merged_duplicates"], 1)
        self.assertEqual(summary["conflicts"], 0)

    def test_output_has_no_timestamp(self):
        sources = default_source_paths(PROJECT_ROOT)
        rendered = json.dumps(compile_catalogs(sources), ensure_ascii=False)
        self.assertNotIn("compiled_at", rendered)
        self.assertNotIn("timestamp", rendered)

    def test_no_approval_grants_in_output(self):
        sources = default_source_paths(PROJECT_ROOT)
        result = compile_catalogs(sources)
        for record in result["records"]:
            self.assertNotIn(record["state"], {"RuntimeApproved", "ReleaseApproved"})

    # ------------------------------------------------------------------
    # negative fixtures
    # ------------------------------------------------------------------

    def test_schema_invalid_record_is_rejected(self):
        record = _valid_record()
        del record["license"]
        source = _write_source(self.root, "s1.json", [record])
        result = compile_catalogs([source])
        summary = result["audit_summary"]
        self.assertEqual(summary["compiled_records"], 0)
        self.assertEqual(summary["rejected_records"], 1)
        self.assertIn("缺少必需字段 license", result["rejected"][0]["reason"])

    def test_unmigrated_fine_grained_category_is_rejected(self):
        # 细分类直接写入 category（未迁移）= 非法大类，仍拒绝（与 L39 category_invalid 同语义）。
        record = _valid_record(asset_id="asset_quaternius_fox_v01", category="terrestrial_animal")
        ok, reason = _record_schema_check(record, REAL_TAXONOMY_MAP)
        self.assertFalse(ok)
        self.assertIn("category 非法", reason)
        source = _write_source(self.root, "s1.json", [record])
        result = compile_catalogs([source], REAL_TAXONOMY_MAP)
        self.assertEqual(result["audit_summary"]["compiled_records"], 0)
        self.assertEqual(result["audit_summary"]["rejected_records"], 1)

    def test_unknown_taxonomy_category_is_rejected(self):
        record = _valid_record(asset_id="asset_mystery_v01", category="animal", taxonomy_category="mystery_beast")
        ok, reason = _record_schema_check(record, REAL_TAXONOMY_MAP)
        self.assertFalse(ok)
        self.assertIn("未知细分类", reason)
        source = _write_source(self.root, "s1.json", [record])
        result = compile_catalogs([source], REAL_TAXONOMY_MAP)
        self.assertEqual(result["audit_summary"]["compiled_records"], 0)
        self.assertEqual(result["audit_summary"]["rejected_records"], 1)

    def test_taxonomy_parent_mismatch_is_rejected(self):
        record = _valid_record(asset_id="asset_mismatch_v01", category="ship", taxonomy_category="outfit")
        ok, reason = _record_schema_check(record, REAL_TAXONOMY_MAP)
        self.assertFalse(ok)
        self.assertIn("父类", reason)
        self.assertIn("不匹配", reason)

    def test_taxonomy_bad_format_is_rejected(self):
        record = _valid_record(asset_id="asset_badfmt_v01", category="animal", taxonomy_category="Terrestrial Animal")
        ok, reason = _record_schema_check(record, REAL_TAXONOMY_MAP)
        self.assertFalse(ok)
        self.assertIn("taxonomy_category", reason)

    def test_all_eight_real_taxonomy_mappings_compile(self):
        # 8 个真实细分类 + 正确父类 = Schema 合法。
        for taxonomy, parent in REAL_TAXONOMY_MAP.items():
            with self.subTest(taxonomy=taxonomy):
                record = _valid_record(asset_id="asset_tax_" + taxonomy, category=parent, taxonomy_category=taxonomy)
                ok, reason = _record_schema_check(record, REAL_TAXONOMY_MAP)
                self.assertTrue(ok, reason)

    def test_old_record_without_taxonomy_still_legal(self):
        ok, reason = _record_schema_check(_valid_record(), REAL_TAXONOMY_MAP)
        self.assertTrue(ok, reason)

    def test_l39_and_l42_agree_on_records(self):
        # L39 与 L42 同语义：对同一正负记录结论一致。
        from tools.d18_logic_audit import aggregate_record
        positive = _valid_record(asset_id="asset_fox_v01", category="animal", taxonomy_category="terrestrial_animal")
        _, gaps = aggregate_record(positive, REAL_TAXONOMY_MAP)
        self.assertEqual(gaps, [])
        ok, _ = _record_schema_check(positive, REAL_TAXONOMY_MAP)
        self.assertTrue(ok)
        negative = _valid_record(asset_id="asset_bad_v01", category="animal", taxonomy_category="mystery_beast")
        _, gaps = aggregate_record(negative, REAL_TAXONOMY_MAP)
        self.assertTrue(any(g["rule"] == "unknown_taxonomy_category" for g in gaps))
        ok, _ = _record_schema_check(negative, REAL_TAXONOMY_MAP)
        self.assertFalse(ok)

    def test_l02_schema_fixtures_with_taxonomy(self):
        # 相关 L02 Schema 测试：合法/非法 taxonomy 夹具按 jsonschema 预期判定。
        import jsonschema
        schema = json.loads((PROJECT_ROOT / "tools/schemas/d18_logic_asset_record.schema.json").read_text(encoding="utf-8"))
        fixtures = PROJECT_ROOT / "tools/tests/d18_logic_asset_record_fixtures"
        validator = jsonschema.Draft202012Validator(schema)

        valid = json.loads((fixtures / "valid.json").read_text(encoding="utf-8"))
        validator.validate(valid)
        valid_tax = json.loads((fixtures / "valid_taxonomy_category.json").read_text(encoding="utf-8"))
        validator.validate(valid_tax)

        bad = json.loads((fixtures / "invalid_taxonomy_category_format.json").read_text(encoding="utf-8"))
        with self.assertRaises(jsonschema.ValidationError):
            validator.validate(bad)

    def test_quaternius_migration_counts_are_exact(self):
        # 61 条 Quaternius 迁移：21 animal + 40 character，细分类逐条保留。
        from collections import Counter
        source = SOURCES_DIR / "d18_quaternius_logic_catalog.json"
        data = json.loads(source.read_text(encoding="utf-8"))
        records = data["records"]
        self.assertEqual(len(records), 61)
        self.assertEqual(Counter(r["category"] for r in records), {"animal": 21, "character": 40})
        taxonomy = Counter(r.get("taxonomy_category") for r in records)
        self.assertEqual(taxonomy["terrestrial_animal"], 10)
        self.assertEqual(taxonomy["aquatic_animal"], 7)
        self.assertEqual(taxonomy["pet"], 3)
        self.assertEqual(taxonomy["bird"], 1)
        self.assertEqual(taxonomy["npc_visual"], 8)
        self.assertEqual(taxonomy["player_visual"], 1)
        self.assertEqual(taxonomy["hair"], 7)
        self.assertEqual(taxonomy["outfit"], 24)
        self.assertEqual(sum(v for k, v in taxonomy.items() if k), 61)

    def test_conflicting_declaration_is_excluded(self):
        first = _valid_record(asset_id="asset_conflict_v01", category="ship")
        second = _valid_record(asset_id="asset_conflict_v01", category="prop")
        source_a = _write_source(self.root, "s1.json", [first])
        source_b = _write_source(self.root, "s2.json", [second])
        result = compile_catalogs([source_a, source_b])
        summary = result["audit_summary"]
        self.assertEqual(summary["conflicts"], 1)
        self.assertEqual(summary["compiled_records"], 0)
        self.assertEqual(len(result["conflicts"]), 1)
        self.assertIn("category", result["conflicts"][0]["fields"])
        self.assertNotIn("asset_conflict_v01", [r["canonical_asset_id"] for r in result["records"]])

    def test_bad_source_json_is_rejected(self):
        source = self.root / "bad.json"
        source.write_text("{not json", encoding="utf-8")
        result = compile_catalogs([source])
        self.assertEqual(len(result["audit_summary"]["source_errors"]), 1)
        exit_code = main(["--project-root", str(PROJECT_ROOT), "--source", str(source)])
        self.assertEqual(exit_code, 2)

    def test_record_check_rejects_bad_hash_and_path(self):
        ok, reason = _record_schema_check(_valid_record(sha256="short"))
        self.assertFalse(ok)
        self.assertIn("sha256", reason)
        ok, reason = _record_schema_check(_valid_record(evidence_paths=["C:/Users/Administrator/x.txt"]))
        self.assertFalse(ok)
        self.assertIn("evidence", reason)

    # ------------------------------------------------------------------
    # contracts
    # ------------------------------------------------------------------

    def test_sources_are_read_only(self):
        sources = default_source_paths(PROJECT_ROOT)
        before = {path: path.read_text(encoding="utf-8") for path in sources}
        compile_catalogs(sources)
        for path, text in before.items():
            self.assertEqual(path.read_text(encoding="utf-8"), text)

    def test_result_json_serialisable(self):
        sources = default_source_paths(PROJECT_ROOT)
        json.dumps(compile_catalogs(sources), ensure_ascii=False)


if __name__ == "__main__":
    unittest.main()
