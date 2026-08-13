import tempfile
import unittest
from pathlib import Path

import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from d16_a01_static_baseline import scan


class D16A01StaticBaselineTests(unittest.TestCase):
    def test_component_inventory_is_explicitly_static(self):
        with tempfile.TemporaryDirectory() as directory:
            scene = Path(directory) / "Sample.unity"
            scene.write_text(
                "--- !u!1 &1\n  m_Name: Harbor\n"
                "--- !u!23 &2\n"
                "--- !u!64 &3\n",
                encoding="utf-8",
            )
            result = scan(scene)
        self.assertFalse(result["runtime_performance_measured"])
        self.assertEqual(result["component_counts"]["GameObject"], 1)
        self.assertEqual(result["component_counts"]["MeshRenderer"], 1)
        self.assertEqual(result["component_counts"]["MeshCollider"], 1)
        self.assertEqual(result["runtime_metrics"]["cpu_ms"], None)


if __name__ == "__main__":
    unittest.main()
