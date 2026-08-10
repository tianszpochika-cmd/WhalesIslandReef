import json
import shutil
import tempfile
import unittest
from pathlib import Path

from tools.content_registry_check import validate_registry


REPOSITORY_ROOT = Path(__file__).resolve().parents[2]
REGISTRY_ROOT = REPOSITORY_ROOT / "game" / "Assets" / "Resources" / "ContentRegistry"


class ContentRegistryCheckerTests(unittest.TestCase):
    def copy_registry(self):
        temporary = tempfile.TemporaryDirectory()
        destination = Path(temporary.name) / "ContentRegistry"
        shutil.copytree(REGISTRY_ROOT, destination)
        return temporary, destination

    @staticmethod
    def read_entry(root, relative_path):
        path = root / "entries" / relative_path
        with path.open("r", encoding="utf-8") as handle:
            return path, json.load(handle)

    @staticmethod
    def write_json(path, value):
        with path.open("w", encoding="utf-8") as handle:
            json.dump(value, handle, ensure_ascii=False, indent=2)

    @staticmethod
    def codes(errors):
        return {error.code for error in errors}

    def test_formal_registry_passes(self):
        self.assertEqual([], validate_registry(REGISTRY_ROOT))

    def test_duplicate_id_is_reported_with_entry_id(self):
        temporary, root = self.copy_registry()
        with temporary:
            path, value = self.read_entry(root, "ship/ship_moonwake.json")
            duplicate_path = root / "entries" / "ship" / "ship_duplicate.json"
            value["test_id"] = "D8-A02-SHIP-DUPLICATE"
            self.write_json(duplicate_path, value)
            errors = validate_registry(root)
            self.assertIn("duplicate-id", self.codes(errors))
            self.assertTrue(any(error.entry_id == "ship_moonwake" for error in errors if error.code == "duplicate-id"))

    def test_invalid_prefix_is_reported(self):
        temporary, root = self.copy_registry()
        with temporary:
            path, value = self.read_entry(root, "item/item_salted_cod.json")
            value["id"] = "cargo_salted_cod"
            value["test_id"] = "D8-A02-ITEM-INVALID-PREFIX"
            self.write_json(root / "entries" / "item" / "cargo_salted_cod.json", value)
            errors = validate_registry(root)
            self.assertIn("invalid-prefix", self.codes(errors))
            self.assertTrue(any(error.entry_id == "cargo_salted_cod" for error in errors if error.code == "invalid-prefix"))

    def test_missing_reference_is_reported_with_source_id(self):
        temporary, root = self.copy_registry()
        with temporary:
            path, value = self.read_entry(root, "quest/quest_first_chart.json")
            value["references"].append("item_does_not_exist")
            self.write_json(path, value)
            errors = validate_registry(root)
            self.assertIn("missing-reference", self.codes(errors))
            self.assertTrue(any(error.entry_id == "quest_first_chart" for error in errors if error.code == "missing-reference"))

    def test_missing_source_and_license_are_rejected(self):
        temporary, root = self.copy_registry()
        with temporary:
            source_path, source_value = self.read_entry(root, "item/item_salted_cod.json")
            source_value.pop("source")
            self.write_json(source_path, source_value)
            license_path, license_value = self.read_entry(root, "pet/pet_cabin_cat.json")
            license_value.pop("license")
            self.write_json(license_path, license_value)
            errors = validate_registry(root)
            self.assertIn("missing-source", self.codes(errors))
            self.assertIn("missing-license", self.codes(errors))

    def test_unlicensed_entry_is_rejected(self):
        temporary, root = self.copy_registry()
        with temporary:
            path, value = self.read_entry(root, "asset/asset_ph_ship_fishing.json")
            value["license"]["commercial_use"] = False
            self.write_json(path, value)
            errors = validate_registry(root)
            self.assertIn("invalid-license", self.codes(errors))
            self.assertTrue(any(error.entry_id == "asset_ph_ship_fishing" for error in errors if error.code == "invalid-license"))

    def test_orphan_entry_is_rejected(self):
        temporary, root = self.copy_registry()
        with temporary:
            path, value = self.read_entry(root, "item/item_salted_cod.json")
            value["id"] = "item_orphan_sample"
            value["test_id"] = "D8-A02-ITEM-ORPHAN"
            self.write_json(root / "entries" / "item" / "item_orphan_sample.json", value)
            errors = validate_registry(root)
            self.assertIn("orphan-reference", self.codes(errors))
            self.assertTrue(any(error.entry_id == "item_orphan_sample" for error in errors if error.code == "orphan-reference"))


if __name__ == "__main__":
    unittest.main()
