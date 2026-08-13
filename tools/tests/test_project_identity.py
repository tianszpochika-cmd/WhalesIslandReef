import json
import re
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
UNITY_ROOT = ROOT / "game"


class ProjectIdentityTests(unittest.TestCase):
    def test_public_product_name_is_consistent_across_runtime_and_release_files(self):
        settings = (UNITY_ROOT / "ProjectSettings" / "ProjectSettings.asset").read_text(encoding="utf-8")
        setup = (UNITY_ROOT / "Assets" / "Editor" / "WhaleboneProjectSetup.cs").read_text(encoding="utf-8")
        manifest = json.loads((UNITY_ROOT / "Assets" / "Resources" / "D16ReleaseManifest.json").read_text(encoding="utf-8"))
        localization = (UNITY_ROOT / "Assets" / "Scripts" / "Gameplay" / "UI" / "Localization.cs").read_text(encoding="utf-8")

        product_match = re.search(r"^\s*productName:\s*(.+?)\s*$", settings, re.MULTILINE)
        company_match = re.search(r"^\s*companyName:\s*(.+?)\s*$", settings, re.MULTILINE)
        self.assertIsNotNone(product_match)
        self.assertIsNotNone(company_match)
        self.assertEqual(product_match.group(1), "WhalesIslandReef")
        self.assertEqual(company_match.group(1), "Whalebone Studio")
        self.assertEqual(manifest["product_name"], product_match.group(1))
        self.assertEqual(manifest["company_name"], company_match.group(1))
        self.assertIn('PlayerSettings.productName = "WhalesIslandReef";', setup)
        self.assertIn('{"title", "WhalesIslandReef"}', localization)

    def test_save_path_rename_has_a_legacy_migration_guard(self):
        save_system = (UNITY_ROOT / "Assets" / "Scripts" / "Systems" / "Save" / "SaveSystem.cs").read_text(encoding="utf-8")
        self.assertIn('private const string LegacyProductName = "Whalebone";', save_system)
        self.assertIn("TryMigrateLegacyProductSave(currentPath);", save_system)
        self.assertIn("SaveFileSafety.RestoreBackup(currentPath, backup.BackupPath);", save_system)
        self.assertIn("旧存档和备份保留", save_system)


if __name__ == "__main__":
    unittest.main()
