"""D18-L40 valid-record fixtures test (wave 3, serial; waits for L39).

Builds fully legal static records for at least five classes (ships/port,
character, ecology, UI/audio, material) covering source, license, rights,
hash, category, content_id, ResourceKey, state and descriptor dimensions.

The test asserts legality by *calling the delivered validators* - it never
re-implements their rules:

- D18-L39 ``aggregate_record``: record must be fully legal (zero gaps);
- D18-L26 ``validate_descriptor``: descriptor must be legal and its type must
  match the record category;
- D18-L16 ``validate_records``: the ResourceKey must be legal and unique.

Hard guarantees:

- all fixture records stay in {Candidate, IntakeEligible}; the fixtures never
  produce RuntimeApproved or ReleaseApproved (asserted by state value and by
  the absence of those literals in the fixtures);
- fixtures contain no real binaries and no fabricated download evidence
  (only .json fixtures and .txt evidence markers inside the fixture tree);
- the aggregated audit over the fixtures stays PASS with zero state issues.
"""

import json
import re
import unittest
from pathlib import Path

from tools.d18_logic_audit import aggregate_record, run_audit

FIXTURES_DIR = Path(__file__).resolve().parent / "fixtures" / "d18_logic_valid"
PROJECT_ROOT = Path(__file__).resolve().parents[2]

REQUIRED_RECORD_FIELDS = {
    "canonical_asset_id", "category", "source_id", "source_url", "license",
    "rights", "download_date", "original_filename", "sha256", "state",
    "content_ids", "evidence_paths", "runtime_deferred",
}
LEGAL_STATES = {"Candidate", "IntakeEligible"}
FORBIDDEN_LITERALS = ("RuntimeApproved", "ReleaseApproved")
SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")
SNAKE_CASE_RE = re.compile(r"^[a-z0-9_]+$")

EXPECTED_CLASSES = {"ships_port", "character", "ecology", "ui_audio", "material"}


def _load_fixtures() -> list[dict]:
    fixtures = []
    for path in sorted(FIXTURES_DIR.glob("*.json")):
        fixtures.append(json.loads(path.read_text(encoding="utf-8")))
    return fixtures


class ValidRecordFixturesTests(unittest.TestCase):
    def setUp(self) -> None:
        self.fixtures = _load_fixtures()
        self.assertGreaterEqual(len(self.fixtures), 5)

    # ------------------------------------------------------------------
    # fixture tree integrity
    # ------------------------------------------------------------------

    def test_five_classes_covered(self):
        classes = {fixture["fixture_class"] for fixture in self.fixtures}
        self.assertGreaterEqual(classes, EXPECTED_CLASSES)

    def test_no_binary_files_in_fixture_tree(self):
        allowed = {".json", ".txt", ".md"}
        for path in sorted(FIXTURES_DIR.rglob("*")):
            if path.is_file():
                self.assertIn(path.suffix.lower(), allowed, f"夹具树内发现非文本文件：{path}")

    def test_no_approval_literals_in_fixtures(self):
        for path in sorted(FIXTURES_DIR.rglob("*")):
            if path.is_file() and path.suffix.lower() == ".json":
                text = path.read_text(encoding="utf-8")
                for literal in FORBIDDEN_LITERALS:
                    self.assertNotIn(literal, text, f"{path} 包含 {literal} 字面量")

    def test_resource_keys_are_unique_and_snake_case(self):
        keys = [fixture["resource_key"] for fixture in self.fixtures]
        self.assertEqual(len(keys), len(set(keys)), "ResourceKey 必须全局唯一")
        for key in keys:
            self.assertRegex(key, r"^d(10|20):[a-z0-9_]+:[a-z0-9_]+$")

    def test_evidence_markers_are_text_files(self):
        for fixture in self.fixtures:
            for evidence in fixture["records"][0]["evidence_paths"]:
                path = PROJECT_ROOT / evidence
                self.assertTrue(path.is_file(), f"证据文件不存在：{evidence}")
                self.assertEqual(path.suffix.lower(), ".txt")
                path.read_text(encoding="utf-8")  # 必须是可读文本

    # ------------------------------------------------------------------
    # per-fixture legality via delivered validators (no logic re-implementation)
    # ------------------------------------------------------------------

    def test_record_required_fields_present(self):
        for fixture in self.fixtures:
            record = fixture["records"][0]
            self.assertTrue(REQUIRED_RECORD_FIELDS <= set(record), f"记录缺字段：{record.get('canonical_asset_id')}")
            self.assertRegex(record["sha256"], SHA256_RE)
            self.assertTrue(all(SNAKE_CASE_RE.fullmatch(cid) for cid in record["content_ids"]))
            self.assertIn(record["state"], LEGAL_STATES)
            self.assertIn(record["category"], {
                "animal", "audio", "building", "character", "environment", "fish",
                "lighting_reference", "material", "prop", "ship", "ui",
            })

    def test_l39_aggregate_accepts_every_record(self):
        # 调用 L39 聚合规则：完全合法记录必须零缺口。
        for fixture in self.fixtures:
            record = fixture["records"][0]
            _, gaps = aggregate_record(record)
            self.assertEqual(
                gaps, [],
                f"{record['canonical_asset_id']} 未通过 L39 聚合：{[g['rule'] for g in gaps]}",
            )

    def test_l26_descriptor_accepts_every_fixture(self):
        # 调用 L26 描述器验证器（含 type↔category 类别匹配）。
        from tools.d18_mesh_descriptor_validator import validate_descriptor
        for fixture in self.fixtures:
            record = fixture["records"][0]
            findings = validate_descriptor(fixture["descriptor"], {record["canonical_asset_id"]: record})
            p0 = [f for f in findings if f["severity"] == "P0"]
            self.assertEqual(
                p0, [],
                f"{record['canonical_asset_id']} 描述器未通过 L26：{[f['rule'] for f in p0]}",
            )
            self.assertIs(fixture["descriptor"]["represents_imported_asset"], False)

    def test_l16_resource_key_accepts_every_fixture(self):
        # 调用 L16 ResourceKey 验证器：键必须合法且无任何问题。
        from tools.d18_resource_key_validator import validate_records
        views = [
            {
                "key": fixture["resource_key"],
                "content_id": fixture["content_ids"][0],
                "category": fixture["records"][0]["category"],
            }
            for fixture in self.fixtures
        ]
        result = validate_records(views)
        self.assertEqual(result["verdict"], "CLEARED")
        self.assertEqual(result["issue_kind_counts"], {})

    # ------------------------------------------------------------------
    # aggregated audit over fixtures
    # ------------------------------------------------------------------

    def test_aggregated_audit_over_fixtures_passes(self):
        catalog_paths = sorted(FIXTURES_DIR.glob("*.json"))
        result = run_audit(PROJECT_ROOT, catalog_paths=catalog_paths)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["blocked_reasons"], [])
        self.assertEqual(result["aggregate"]["record_count"], len(self.fixtures))
        self.assertEqual(result["aggregate"]["p0_count"], 0)
        self.assertEqual(result["aggregate"]["state_issues"], [])

    def test_no_fixture_produces_approval_states(self):
        for fixture in self.fixtures:
            self.assertNotIn(fixture["records"][0]["state"], FORBIDDEN_LITERALS)
            self.assertNotIn(fixture["records"][0]["state"], {"RuntimeApproved", "ReleaseApproved"})


if __name__ == "__main__":
    unittest.main()
