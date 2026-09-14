import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
RUNTIME = ROOT / "game/Assets/Scripts/Gameplay/D26/D26HarborEnvironmentRuntime.cs"


class D26VegetationRuntimeTests(unittest.TestCase):
    def setUp(self):
        self.source = RUNTIME.read_text(encoding="utf-8")

    def test_legacy_tree_root_is_found_below_scene_root(self):
        self.assertIn("GetComponentsInChildren<Transform>(true)", self.source)
        self.assertIn("legacyRoot.name != LegacyTreeRootName", self.source)
        self.assertIn("D19_SaintMargaret_Root is a child", self.source)

    def test_all_legacy_tree_renderers_are_quarantined(self):
        start = self.source.index("private static int DisableLegacyTreeRenderers")
        method = self.source[start:]
        self.assertIn("legacyRoot.GetComponentsInChildren<Renderer>(true)", method)
        self.assertIn("renderers[k].enabled = false", method)

    def test_legacy_tree_containment_does_not_add_gameplay_authority(self):
        start = self.source.index("private static int DisableLegacyTreeRenderers")
        method = self.source[start:]
        self.assertIn("presentation containment only", method)
        self.assertNotIn("AddComponent<Collider>", method)
        self.assertNotIn("NavMesh", method)

    def test_legacy_tree_names_are_explicitly_scoped(self):
        self.assertEqual(1, self.source.count("private const string LegacyTreeRootName"))
        self.assertEqual(1, self.source.count("private const string LegacyPlaceholderTreeRootName"))
        self.assertIn("legacyRoot.name != LegacyTreeRootName", self.source)
        self.assertIn("legacyRoot.name != LegacyPlaceholderTreeRootName", self.source)


if __name__ == "__main__":
    unittest.main()
