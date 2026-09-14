import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d25_visual_asset_audit import audit_catalog  # noqa: E402


class D25VisualAssetAuditTests(unittest.TestCase):
    def test_real_catalog_passes_read_only(self):
        catalog = ROOT / "docs/game/design/common/assets/d25_visual_asset_catalog.json"
        result = audit_catalog(ROOT, catalog)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["entry_count"], 4)
        self.assertTrue(result["read_only"])
        self.assertFalse(result["registry_modified"])

    def test_missing_anchor_and_path_escape_are_blocked(self):
        with tempfile.TemporaryDirectory() as temp:
            temp_root = Path(temp)
            prefab = temp_root / "broken.prefab"
            prefab.write_text("%YAML 1.1\n--- !u!1 &1\nGameObject:\n  m_Name: broken\n", encoding="utf-8")
            evidence = temp_root / "LICENSE.txt"
            evidence.write_text("internal test evidence", encoding="utf-8")
            catalog = temp_root / "catalog.json"
            catalog.write_text(
                json.dumps(
                    {
                        "catalog_kind": "d25_visual_asset_catalog",
                        "schema_version": 1,
                        "entries": [
                            {
                                "asset_id": "d25_fixture",
                                "prefab_path": str(prefab),
                                "source_state": "Candidate",
                                "license_evidence_path": str(evidence),
                                "required_components": ["anchor"],
                            }
                        ],
                    }
                ),
                encoding="utf-8",
            )
            result = audit_catalog(temp_root, catalog)
            self.assertEqual(result["status"], "BLOCKED")
            self.assertIn("unsafe_prefab_path", result["issue_counts"])


if __name__ == "__main__":
    unittest.main()
