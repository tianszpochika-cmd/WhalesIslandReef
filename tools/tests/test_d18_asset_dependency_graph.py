"""Tests for tools/d18_asset_dependency_graph.py (D18-L30)."""

import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_asset_dependency_graph import (  # noqa: E402
    build_graph,
    load_items,
    load_packages,
    FORBIDDEN_STATE_PAIRS,
    STATES,
)

FIXTURE_BASE = Path(tempfile.gettempdir()) / "d18_l30_fixtures"


def _write_json(name: str, data) -> Path:
    FIXTURE_BASE.mkdir(parents=True, exist_ok=True)
    path = FIXTURE_BASE / name
    path.write_text(json.dumps(data), encoding="utf-8")
    return path


def record(asset_id, state="Candidate", license_id="CC0-1.0", **extra):
    data = {
        "canonical_asset_id": asset_id,
        "state": state,
        "license": {
            "license_id": license_id,
            "license_evidence_path": "game/Assets/Art/Candidates/License.txt",
        },
    }
    data.update(extra)
    return data


def descriptor(asset_id, **extra):
    data = {"asset_id": asset_id}
    data.update(extra)
    return data


def codes(result):
    return set(result["issue_counts"])


class D18AssetDependencyGraphTests(unittest.TestCase):
    def build(self, records, descriptors=(), incompatible_pairs=None):
        return build_graph(records, descriptors, incompatible_pairs=incompatible_pairs)

    def test_acyclic_graph_passes(self):
        records = [
            record("asset_a", "IntakeEligible", package="pack_a", dependencies=["asset_b"]),
            record("asset_b", "IntakeEligible", package="pack_a"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["node_count"], 2)
        self.assertEqual(result["edge_count"], 1)
        self.assertEqual(result["approved_count"], 0)
        self.assertTrue(result["read_only"])
        self.assertFalse(result["catalog_modified"])

    def test_result_is_deterministic(self):
        records = [
            record("asset_b", "IntakeEligible", dependencies=["asset_c", "asset_a"]),
            record("asset_a", "Candidate", package="pack_x"),
            record("asset_c", "RuntimeRequested", package="pack_y"),
        ]
        first = self.build(records)
        second = self.build(records)
        self.assertEqual(first["fingerprint"], second["fingerprint"])
        self.assertEqual(json.dumps(first, sort_keys=True), json.dumps(second, sort_keys=True))

    def test_duplicate_edges_are_deduped(self):
        records = [
            record("asset_a", dependencies=["asset_b", "asset_b"]),
            record("asset_b"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["edge_count"], 1)

    def test_record_and_descriptor_merge_same_attrs(self):
        records = [record("asset_a", "IntakeEligible", package="pack_a")]
        descs = [
            descriptor("asset_a", package="pack_a", dependencies=["asset_b"]),
            descriptor("asset_b", license={"license_id": "CC0-1.0", "license_evidence_path": "x.txt"}),
        ]
        result = self.build(records, descs)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["edge_count"], 1)

    def test_cycle_two_nodes_is_blocked(self):
        records = [
            record("asset_a", dependencies=["asset_b"]),
            record("asset_b", dependencies=["asset_a"]),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("cycle", codes(result))
        self.assertEqual(result["cycle_count"], 1)
        self.assertIn("asset_a", result["cycles"][0])
        self.assertIn("asset_b", result["cycles"][0])

    def test_cycle_three_nodes_is_blocked(self):
        records = [
            record("asset_a", dependencies=["asset_b"]),
            record("asset_b", dependencies=["asset_c"]),
            record("asset_c", dependencies=["asset_a"]),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertEqual(result["cycle_count"], 1)
        self.assertEqual(result["cycles"][0], ["asset_a", "asset_b", "asset_c"])

    def test_self_loop_is_blocked(self):
        records = [record("asset_a", dependencies=["asset_a"])]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("cycle", codes(result))
        self.assertEqual(result["cycles"], [["asset_a"]])

    def test_dangling_target_is_blocked(self):
        records = [record("asset_a", dependencies=["asset_ghost"])]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("dangling_reference", codes(result))
        self.assertEqual(result["issues"][0]["edge"], "asset_a -> asset_ghost")

    def test_descriptor_edge_to_undeclared_target_is_blocked(self):
        descs = [descriptor("asset_a", dependencies=["asset_ghost"])]
        result = self.build([], descs)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("dangling_reference", codes(result))

    def test_empty_edge_target_is_blocked(self):
        descs = [descriptor("asset_a", dependencies=[""])]
        result = self.build([], descs)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_edge", codes(result))

    def test_invalid_edge_entry_is_blocked(self):
        descs = [descriptor("asset_a", dependencies=[42])]
        result = self.build([], descs)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_edge", codes(result))

    def test_non_list_edges_is_blocked(self):
        records = [record("asset_a", dependencies="asset_b")]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_edges", codes(result))

    def test_cross_package_incompatible_pair_is_blocked(self):
        records = [
            record("asset_a", package="pack_a", dependencies=["asset_b"]),
            record("asset_b", package="pack_b"),
        ]
        result = self.build(records, incompatible_pairs={("pack_a", "pack_b")})
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("cross_package_incompatibility", codes(result))

    def test_incompatible_pair_is_order_insensitive(self):
        records = [
            record("asset_a", package="pack_a", dependencies=["asset_b"]),
            record("asset_b", package="pack_b"),
        ]
        result = self.build(records, incompatible_pairs={("pack_b", "pack_a")})
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("cross_package_incompatibility", codes(result))

    def test_node_declared_incompatibility_is_blocked(self):
        records = [
            record("asset_a", package="pack_a", dependencies=["asset_b"], incompatible_packages=["pack_b"]),
            record("asset_b", package="pack_b"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("cross_package_incompatibility", codes(result))

    def test_same_package_edge_is_ok(self):
        records = [
            record("asset_a", package="pack_a", dependencies=["asset_b"]),
            record("asset_b", package="pack_a"),
        ]
        result = self.build(records, incompatible_pairs={("pack_a", "pack_b")})
        self.assertEqual(result["status"], "PASS")

    def test_skip_candidate_to_runtime_approved_is_blocked(self):
        records = [
            record("asset_a", "Candidate", dependencies=["asset_b"]),
            record("asset_b", "RuntimeApproved", runtime_consumers=["x"]),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("state_skip_edge", codes(result))

    def test_skip_candidate_to_release_approved_is_blocked(self):
        records = [
            record("asset_a", "Candidate", dependencies=["asset_b"]),
            record("asset_b", "ReleaseApproved"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("state_skip_edge", codes(result))

    def test_skip_intake_eligible_to_runtime_approved_is_blocked(self):
        records = [
            record("asset_a", "IntakeEligible", dependencies=["asset_b"]),
            record("asset_b", "RuntimeApproved", runtime_consumers=["x"]),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("state_skip_edge", codes(result))

    def test_skip_intake_eligible_to_release_approved_is_blocked(self):
        records = [
            record("asset_a", "IntakeEligible", dependencies=["asset_b"]),
            record("asset_b", "ReleaseApproved"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("state_skip_edge", codes(result))

    def test_skip_runtime_requested_to_release_approved_is_blocked(self):
        records = [
            record("asset_a", "RuntimeRequested", dependencies=["asset_b"]),
            record("asset_b", "ReleaseApproved"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("state_skip_edge", codes(result))

    def test_skip_candidate_to_runtime_requested_is_blocked(self):
        records = [
            record("asset_a", "Candidate", dependencies=["asset_b"]),
            record("asset_b", "RuntimeRequested"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("state_skip_edge", codes(result))

    def test_rejected_node_has_no_out_edges(self):
        records = [
            record("asset_a", "Rejected", dependencies=["asset_b"]),
            record("asset_b", "IntakeEligible"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("state_skip_edge", codes(result))

    def test_release_approved_to_rejected_is_blocked(self):
        records = [
            record("asset_a", "ReleaseApproved", dependencies=["asset_b"]),
            record("asset_b", "Rejected"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("state_skip_edge", codes(result))

    def test_legal_state_chain_passes(self):
        records = [
            record("asset_a", "Candidate", dependencies=["asset_b"]),
            record("asset_b", "IntakeEligible", dependencies=["asset_c"]),
            record("asset_c", "RuntimeRequested", dependencies=["asset_d"]),
            record("asset_d", "RuntimeApproved", runtime_consumers=["x"], dependencies=["asset_e"]),
            record("asset_e", "ReleaseApproved"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "PASS")
        self.assertNotIn("state_skip_edge", codes(result))

    def test_t4_runtime_approved_to_release_approved_is_allowed(self):
        records = [
            record("asset_a", "RuntimeApproved", runtime_consumers=["x"], dependencies=["asset_b"]),
            record("asset_b", "ReleaseApproved"),
        ]
        result = self.build(records)
        self.assertEqual(result["status"], "PASS")
        self.assertNotIn("state_skip_edge", codes(result))

    def test_unknown_state_is_blocked(self):
        records = [record("asset_a", "Pending")]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("unknown_state", codes(result))

    def test_missing_license_is_blocked(self):
        records = [{"canonical_asset_id": "asset_a", "state": "Candidate"}]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("missing_license", codes(result))

    def test_unspecified_license_id_is_blocked(self):
        for bad in ("", "unknown", "pending"):
            records = [record("asset_a", license_id=bad)]
            result = self.build(records)
            self.assertEqual(result["status"], "BLOCKED", bad)
            self.assertIn("missing_license", codes(result), bad)

    def test_missing_license_evidence_is_blocked(self):
        records = [{
            "canonical_asset_id": "asset_a",
            "state": "Candidate",
            "license": {"license_id": "CC0-1.0"},
        }]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("missing_license_evidence", codes(result))

    def test_descriptor_without_license_is_blocked(self):
        descs = [descriptor("asset_a", dependencies=["asset_b"]), descriptor("asset_b")]
        result = self.build([], descs)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("missing_license", codes(result))

    def test_duplicate_node_is_blocked(self):
        records = [record("asset_a"), record("asset_a")]
        result = self.build(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("duplicate_node", codes(result))

    def test_record_descriptor_state_conflict_is_blocked(self):
        records = [record("asset_a", "Candidate")]
        descs = [descriptor("asset_a", state="IntakeEligible")]
        result = self.build(records, descs)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("node_conflict", codes(result))

    def test_empty_graph_is_blocked(self):
        result = self.build([])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("no_nodes", codes(result))

    def test_invalid_record_is_blocked(self):
        result = self.build(["not-an-object"])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_record", codes(result))

    def test_input_issues_are_blocked(self):
        result = build_graph([], [], input_issues=[{"code": "file_not_found", "severity": "error", "message": "missing"}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("file_not_found", codes(result))

    def test_load_items_shapes(self):
        list_file = _write_json("graph_records_list.json", [record("asset_a")])
        loaded, code, message = load_items(list_file, "records")
        self.assertEqual(code, "")
        self.assertEqual(loaded[0]["canonical_asset_id"], "asset_a")

        dict_file = _write_json("graph_descriptors_dict.json", {"descriptors": [descriptor("asset_b")]})
        loaded, code, message = load_items(dict_file, "descriptors")
        self.assertEqual(code, "")
        self.assertEqual(loaded[0]["asset_id"], "asset_b")

        bad = _write_json("graph_bad_shape.json", {"packages": []})
        loaded, code, message = load_items(bad, "records")
        self.assertEqual(code, "records_not_supported")
        self.assertIsNone(loaded)

    def test_load_items_rejects_missing_and_broken_files(self):
        loaded, code, message = load_items(FIXTURE_BASE / "graph_missing.json", "records")
        self.assertEqual(code, "file_not_found")
        broken = _write_json("graph_broken.json", None)
        broken.write_bytes(b"{not json")
        loaded, code, message = load_items(broken, "records")
        self.assertEqual(code, "file_json_error")
        self.assertIsNone(loaded)

    def test_load_packages(self):
        pairs, code, message = load_packages(_write_json("graph_packages.json", {
            "incompatible_pairs": [["pack_b", "pack_a"], ["pack_c", "pack_c"]],
        }))
        self.assertEqual(code, "incompatible_pair_invalid")
        self.assertIsNone(pairs)

        pairs, code, message = load_packages(_write_json("graph_packages_ok.json", {
            "incompatible_pairs": [["pack_b", "pack_a"]],
        }))
        self.assertEqual(code, "")
        self.assertEqual(pairs, {("pack_a", "pack_b")})

        bad = _write_json("graph_packages_bad.json", {"foo": []})
        pairs, code, message = load_packages(bad)
        self.assertEqual(code, "packages_not_supported")
        self.assertIsNone(pairs)

    def test_frozen_state_vocabulary(self):
        self.assertEqual(tuple(STATES), (
            "Candidate", "IntakeEligible", "RuntimeRequested", "RuntimeApproved",
            "ReleaseApproved", "Rejected", "Revoked",
        ))
        self.assertNotIn(("RuntimeApproved", "ReleaseApproved"), FORBIDDEN_STATE_PAIRS)

    def test_graph_is_read_only_against_inputs(self):
        records_path = _write_json("graph_ro_records.json", [
            record("asset_a", "Candidate", dependencies=["asset_b"]),
            record("asset_b", "IntakeEligible"),
        ])
        before = records_path.read_bytes()
        loaded, code, message = load_items(records_path, "records")
        self.assertEqual(code, "")
        result = build_graph(loaded)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(records_path.read_bytes(), before)
        self.assertFalse(result["catalog_modified"])

    def test_node_state_omitted_from_output_when_unknown(self):
        descs = [descriptor("asset_a", license={"license_id": "CC0-1.0", "license_evidence_path": "x.txt"})]
        result = self.build([], descs)
        self.assertEqual(result["status"], "PASS")
        node_row = result["nodes"][0]
        self.assertNotIn("state", node_row)
        self.assertIn("Unknown", result["node_count_by_state"])


if __name__ == "__main__":
    unittest.main()
