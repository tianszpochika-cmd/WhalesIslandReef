import unittest

from tools import d15_a06_assembly_smoke, d15_a06_catalog_check


class D15A06CatalogTests(unittest.TestCase):
    def test_catalog_has_frozen_batch_counts(self):
        manifest = d15_a06_catalog_check.load(d15_a06_catalog_check.BATCH / "catalog.json")
        self.assertEqual(manifest["counts"], {"hulls": 20, "modules": 80, "functional_points": 60, "asset_resources": 166})

    def test_catalog_checker_passes(self):
        self.assertEqual(d15_a06_catalog_check.main(), 0)

    def test_representative_assembly_smoke_passes(self):
        self.assertEqual(d15_a06_assembly_smoke.main(), 0)


if __name__ == "__main__":
    unittest.main()
