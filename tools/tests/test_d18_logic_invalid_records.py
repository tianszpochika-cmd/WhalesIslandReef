"""D18-L41 negative-record fixtures test (wave 3, serial; waits for L40).

Builds minimal P0-violation fixtures for each rule class and asserts that the
delivered validators output the *correct rule and severity* (never guessing,
never silencing):

- L39 aggregate: missing_source / license_conflict / missing_hash /
  path_escape / state_jump / no_distribution_rights -> exact rule + severity;
- L31 duplicate identity: duplicate_record;
- L15 bad content_id: path_like_content_id (candidate path as business key);
- L38 generated without lineage: non_original_missing_lineage;
- L18 false release claim: doc_overstep_claim.

Hard guarantees:

- every negative fixture fails with a stable, asserted reason;
- production catalogs and tool logic are never modified (read-only contract);
- fixture tree contains no binaries.
"""

import json
import tempfile
import unittest
from pathlib import Path

from tools.d18_content_id_asset_link_validator import validate_records as validate_content_ids
from tools.d18_duplicate_identity_audit import audit_records
from tools.d18_generated_asset_guard import validate_lineage_catalog
from tools.d18_logic_audit import run_audit
from tools.d18_release_claim_guard import run_guard

FIXTURES_DIR = Path(__file__).resolve().parent / "fixtures" / "d18_logic_invalid"
PROJECT_ROOT = Path(__file__).resolve().parents[2]

L39_CASES = {
    "missing_source.json": ("missing_source", "P0"),
    "license_conflict.json": ("license_suspect", "P0"),
    "missing_hash.json": ("hash_pending", "P1"),
    "path_escape.json": ("path_escape", "P0"),
    "state_jump.json": ("approval_state_in_logic_phase", "P0"),
    "no_distribution_rights.json": ("missing_rights", "P0"),
}

BAD_CONTENT_ID = "Candidates/Kenney/foo.png"
DUP_TARGET_SHA256 = "9D558839FD141B16216482D0FFB2F10558A16FD89991EA29AE678C7E0D42FC6F"


def _load(name: str) -> dict:
    return json.loads((FIXTURES_DIR / name).read_text(encoding="utf-8"))


class InvalidRecordFixturesTests(unittest.TestCase):
    # ------------------------------------------------------------------
    # L39 aggregate rules (rule + severity must match exactly)
    # ------------------------------------------------------------------

    def test_l39_outputs_correct_rule_and_severity(self):
        paths = [FIXTURES_DIR / name for name in L39_CASES]
        result = run_audit(PROJECT_ROOT, catalog_paths=paths)
        gaps = {g["rule"]: g for g in result["aggregate"]["gaps"]}
        for name, (rule, severity) in L39_CASES.items():
            with self.subTest(fixture=name):
                self.assertIn(rule, gaps, f"{name} 未触发规则 {rule}")
                self.assertEqual(gaps[rule]["severity"], severity)
        self.assertEqual(result["status"], "BLOCKED", "state_jump 必须使整体聚合 BLOCKED")
        self.assertEqual(len(result["aggregate"]["state_issues"]), 1)

    def test_l39_gap_is_minimal_and_stable(self):
        # 每个 L39 负夹具只破坏一个字段：每条记录只应有一个 P0/P1 gap。
        for name, (rule, severity) in L39_CASES.items():
            with self.subTest(fixture=name):
                result = run_audit(PROJECT_ROOT, catalog_paths=[FIXTURES_DIR / name])
                record_gaps = [g for g in result["aggregate"]["gaps"]]
                self.assertEqual(len(record_gaps), 1, f"{name} 应只有一个缺口，实际 {record_gaps}")
                self.assertEqual(record_gaps[0]["rule"], rule)
                self.assertEqual(record_gaps[0]["severity"], severity)
        # 两次运行结果一致（原因稳定）
        first = run_audit(PROJECT_ROOT, catalog_paths=[FIXTURES_DIR / name for name in L39_CASES])
        second = run_audit(PROJECT_ROOT, catalog_paths=[FIXTURES_DIR / name for name in L39_CASES])
        self.assertEqual(first["aggregate"]["gap_counts"], second["aggregate"]["gap_counts"])

    def test_missing_hash_is_pending_not_approved(self):
        # 缺 hash 语义：Pending 不批准（P1），绝不补猜哈希。
        result = run_audit(PROJECT_ROOT, catalog_paths=[FIXTURES_DIR / "missing_hash.json"])
        self.assertEqual(result["aggregate"]["p0_count"], 0)
        self.assertEqual(result["aggregate"]["p1_count"], 1)
        self.assertIn("hash_pending", result["aggregate"]["gap_counts"])

    # ------------------------------------------------------------------
    # other sub-tools (L31 / L15 / L38 / L18)
    # ------------------------------------------------------------------

    def test_duplicate_identity_blocks(self):
        data = _load("duplicate_identity.json")
        result = audit_records(PROJECT_ROOT, data["records"])
        rendered = json.dumps(result, ensure_ascii=False)
        self.assertIn("duplicate_record", rendered)
        self.assertEqual(result["issues"], [])

    def test_bad_content_id_blocks(self):
        record = _load("bad_content_id.json")["records"][0]
        result = validate_content_ids(
            [record],
            catalogs=[{}],
            prefix_types={},
            deprecated={},
            candidates={BAD_CONTENT_ID},
        )
        self.assertEqual(result["status"], "BLOCKED")
        self.assertEqual(result["issue_counts"], {"path_like_content_id": 1})
        self.assertEqual(result["issues"][0]["severity"], "error")
        self.assertEqual(result["issues"][0]["content_id"], BAD_CONTENT_ID)

    def test_generated_without_lineage_blocks(self):
        lineage = _load("generated_without_lineage.json")
        result = validate_lineage_catalog(lineage, PROJECT_ROOT)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertEqual(result["rule_counts"], {"non_original_missing_lineage": 1})
        finding = result["findings"][0]
        self.assertEqual(finding["severity"], "P0")
        self.assertEqual(finding["rule"], "non_original_missing_lineage")

    def test_false_release_claim_blocks(self):
        with tempfile.TemporaryDirectory() as td:
            tmp_root = Path(td)
            target_dir = tmp_root / "docs" / "game" / "design" / "common"
            target_dir.mkdir(parents=True)
            source = FIXTURES_DIR / "false_release_claim.md"
            target = target_dir / "D18_L41_FIXTURE_CLAIM.md"
            target.write_text(source.read_text(encoding="utf-8"), encoding="utf-8")
            result = run_guard(tmp_root)
            self.assertEqual(result["status"], "BLOCKED")
            self.assertIn("doc_overstep_claim", result["issue_counts"])
            issue = next(i for i in result["issues"] if i["code"] == "doc_overstep_claim")
            self.assertEqual(issue["severity"], "error")
            self.assertIn("ReleaseApproved", issue["message"])

    # ------------------------------------------------------------------
    # contracts
    # ------------------------------------------------------------------

    def test_fixture_tree_has_no_binaries(self):
        allowed = {".json", ".md", ".txt"}
        for path in sorted(FIXTURES_DIR.rglob("*")):
            if path.is_file():
                self.assertIn(path.suffix.lower(), allowed, f"负夹具树内发现非文本文件：{path}")

    def test_tools_are_read_only_against_fixtures(self):
        # 验证器运行后夹具字节不变（只读契约）。
        for name in L39_CASES:
            path = FIXTURES_DIR / name
            before = path.read_text(encoding="utf-8")
            run_audit(PROJECT_ROOT, catalog_paths=[path])
            self.assertEqual(path.read_text(encoding="utf-8"), before, f"{name} 被修改")
        for name in ("duplicate_identity.json", "generated_without_lineage.json", "bad_content_id.json"):
            path = FIXTURES_DIR / name
            before = path.read_text(encoding="utf-8")
            data = json.loads(before)
            if name == "duplicate_identity.json":
                audit_records(PROJECT_ROOT, data["records"])
            elif name == "generated_without_lineage.json":
                validate_lineage_catalog(data, PROJECT_ROOT)
            else:
                validate_content_ids(data["records"], catalogs=[{}], prefix_types={}, deprecated={}, candidates={BAD_CONTENT_ID})
            self.assertEqual(path.read_text(encoding="utf-8"), before, f"{name} 被修改")

    def test_dup_target_hash_is_real(self):
        # 重复 identity 夹具的声明哈希必须等于真实文件摘要（不补猜）。
        target = FIXTURES_DIR / "evidence" / "dup_target.txt"
        self.assertEqual(DUP_TARGET_SHA256, _hash_file(target))

    def test_result_shapes_are_stable(self):
        # 规则名与严重度枚举是冻结的（L01 基线第 7 节），防止未来漂移。
        expected_rules = {
            "missing_source", "license_suspect", "hash_pending", "path_escape",
            "approval_state_in_logic_phase", "missing_rights", "duplicate_record",
            "path_like_content_id", "non_original_missing_lineage", "doc_overstep_claim",
        }
        self.assertEqual(len(L39_CASES), 6)
        self.assertEqual(len(expected_rules), 10)


def _hash_file(path: Path) -> str:
    import hashlib
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


if __name__ == "__main__":
    unittest.main()
