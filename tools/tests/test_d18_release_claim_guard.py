import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_release_claim_guard import main, run_guard  # noqa: E402


def _write(path: Path, content: str) -> Path:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8")
    return path


class D18ReleaseClaimGuardTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)
        self.addCleanup(self._tmp.cleanup)

    def run_guard(self):
        return run_guard(self.root)

    def write_manifest(self, record: dict, rel: str = "docs/game/design/common/assets/manifests/test.manifest.json"):
        return _write(self.root / rel, json.dumps(record, ensure_ascii=False, indent=2))

    def write_doc(self, content: str, rel: str = "docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md"):
        return _write(self.root / rel, content)

    def test_clean_candidate_manifest_and_catalog_pass(self):
        self.write_manifest({"pack_id": "pack_ok", "review_status": "Candidate", "candidate_files": []})
        _write(
            self.root / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json",
            json.dumps({"manifest_total": 1, "eligible_entries": [{"intake_id": "i1", "intake_eligible": True}]}),
        )
        result = self.run_guard()
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["error_count"], 0)
        self.assertFalse(result["approval_fields_modified"])

    def test_release_approved_without_human_evidence_flagged(self):
        self.write_manifest({"pack_id": "pack_bad", "review_status": "ReleaseApproved"})
        result = self.run_guard()
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("json_release_approved_without_human", result["issue_counts"])

    def test_release_approved_with_human_evidence_chain_allowed(self):
        _write(self.root / "docs/evidence/release.txt", "human review evidence")
        self.write_manifest(
            {
                "pack_id": "pack_human",
                "review_status": "ReleaseApproved",
                "previous_review_status": "RuntimeApproved",
                "human_reviewer": "human:responsible_reviewer",
                "release_evidence_path": "docs/evidence/release.txt",
            }
        )
        result = self.run_guard()
        self.assertEqual(result["status"], "PASS")

    def test_runtime_approved_state_hold_flagged(self):
        self.write_manifest({"pack_id": "pack_rt", "state": "RuntimeApproved"})
        result = self.run_guard()
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("json_approval_state_hold", result["issue_counts"])

    def test_approval_claim_field_flagged(self):
        for value in (True, "approved", "ready", 1):
            self.write_manifest({"pack_id": "pack_claim", "review_status": "Candidate", "release_approved": value})
            result = self.run_guard()
            self.assertEqual(result["status"], "BLOCKED", value)
            self.assertIn("json_approval_claim_field", result["issue_counts"], value)

    def test_falsy_claims_never_flagged(self):
        self.write_manifest(
            {"pack_id": "pack_falsy", "review_status": "Candidate", "release_approved": False, "local_development_ready": 0}
        )
        result = self.run_guard()
        self.assertEqual(result["status"], "PASS")

    def test_local_development_ready_claim_flagged(self):
        self.write_manifest({"pack_id": "pack_ldr", "review_status": "Candidate", "local_development_ready": True})
        result = self.run_guard()
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("json_readiness_claim", result["issue_counts"])

    def test_superseded_historical_doc_skipped(self):
        _write(
            self.root / "docs/game/design/common/D18_FREE_ASSET_SOURCE_CATALOG.md",
            "旧结果：ReleaseApproved 已授予，Steam Ready，可发布。PASS",
        )
        result = self.run_guard()
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["skipped_superseded_files"], 1)

    def test_historical_reference_negated_no_false_positive(self):
        self.write_doc(
            "- 不代表 Steam Ready，当前没有任何资产处于 ReleaseApproved 或 RuntimeApproved，可发布结论未关闭。\n"
            "- 历史证据 D18-S02 PASS 不作为通过依据。\n"
        )
        result = self.run_guard()
        self.assertEqual(result["status"], "PASS")

    def test_active_doc_overstep_claim_flagged(self):
        self.write_doc("资产 A 已完成全部验收，已 ReleaseApproved，Steam Ready，可发布。\n")
        result = self.run_guard()
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("doc_overstep_claim", result["issue_counts"])
        self.assertEqual(result["issues"][0]["line"], 1)

    def test_definitional_state_machine_lines_not_flagged(self):
        self.write_doc(
            "# 状态机\n"
            "### 5.4 RuntimeApproved\n"
            "禁止迁移（违反即失败）：\n"
            "- Candidate → RuntimeApproved（无直达路径）\n"
            "- Candidate → ReleaseApproved\n"
            "\n"
            "- 从 Candidate 出发到达 ReleaseApproved 的唯一路径是 T1 → T2 → T3 → T4；不存在直达边。\n"
            "```json\n"
            '{"from": "Candidate", "to": "ReleaseApproved"}\n'
            "```\n"
            "- RuntimeApproved / ReleaseApproved 持有者仍为 0；工具从不批准。\n"
        )
        result = self.run_guard()
        self.assertEqual(result["status"], "PASS")

    def test_task_card_detection_description_not_flagged(self):
        self.write_doc("检测 Candidate/IntakeEligible/RuntimeRequested 被写为 RuntimeApproved、ReleaseApproved、可发布或 Steam Ready 的越级声明。\n")
        result = self.run_guard()
        self.assertEqual(result["status"], "PASS")

    def test_superseded_result_as_pass_flagged(self):
        self.write_doc("D18-A01 PASS 证明当前资源门禁通过，D18-S02 已验收 PASS。\n")
        result = self.run_guard()
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("superseded_result_as_pass", result["issue_counts"])

    def test_guard_never_modifies_source_files(self):
        manifest = self.write_manifest({"pack_id": "pack_claim", "review_status": "Candidate", "release_approved": True})
        doc = self.write_doc("资产 B 已 ReleaseApproved，可发布。\n")
        before = (manifest.read_bytes(), doc.read_bytes())
        result = self.run_guard()
        self.assertEqual(result["status"], "BLOCKED")
        self.assertFalse(result["approval_fields_modified"])
        self.assertEqual((manifest.read_bytes(), doc.read_bytes()), before)

    def test_broken_json_reported(self):
        _write(self.root / "docs/game/design/common/assets/manifests/broken.manifest.json", "{not json")
        result = self.run_guard()
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("json_parse_error", result["issue_counts"])

    def test_cli_exit_codes(self):
        self.write_manifest({"pack_id": "pack_ok", "review_status": "Candidate"})
        self.assertEqual(main(["--project-root", str(self.root)]), 0)
        self.write_manifest({"pack_id": "pack_bad", "review_status": "ReleaseApproved"})
        self.assertEqual(main(["--project-root", str(self.root)]), 2)


if __name__ == "__main__":
    unittest.main()
