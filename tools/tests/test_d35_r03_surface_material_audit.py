import unittest

from tools.d35_r03_surface_material_audit import audit


class D35R03SurfaceMaterialAuditTests(unittest.TestCase):
    def test_current_r03_static_material_audit_passes(self):
        result = audit()
        self.assertEqual(result["status"], "PASS", result["errors"])
        self.assertEqual(result["revision"], "R76")
        self.assertEqual(result["material_count"], 12)
        self.assertEqual(result["textured_material_count"], 11)
        self.assertEqual(result["color_only_material_count"], 1)
        self.assertEqual(result["normal_map_meta_pass"], 6)
        self.assertEqual(result["editor_audit"]["renderer_count"], 294)
        self.assertEqual(result["editor_audit"]["collider_count"], 1)
        self.assertFalse(result["runtime_proof"])


if __name__ == "__main__":
    unittest.main()
