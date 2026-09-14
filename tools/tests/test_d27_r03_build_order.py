import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
D19_BUILDER = ROOT / "game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs"
D27_BUILDER = ROOT / "game/Assets/Editor/D27/D27R03CharacterWildlifePresentationBuilder.cs"


class D27R03BuildOrderTests(unittest.TestCase):
    def setUp(self):
        self.d19 = D19_BUILDER.read_text(encoding="utf-8")
        self.d27 = D27_BUILDER.read_text(encoding="utf-8")

    def test_d19_rebuild_replays_d27_attachment_after_replacing_base_prefab(self):
        save = self.d19.index("PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath")
        attach = self.d19.index("D27R03CharacterWildlifePresentationBuilder.BuildAndAttach();")
        reload_asset = self.d19.index("AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath)", attach)
        self.assertLess(save, attach)
        self.assertLess(attach, reload_asset)
        self.assertIn("D27 is a nested presentation layer", self.d19)

    def test_d27_attachment_removes_previous_named_node_and_instantiates_once(self):
        start = self.d27.index("private static void AttachToR03Prefab")
        end = self.d27.index("private static void AddNpc", start)
        method = self.d27[start:end]
        self.assertIn("r03.transform.Find(OutputRootName)", method)
        self.assertIn("DestroyImmediate(previous.gameObject)", method)
        self.assertEqual(1, method.count("PrefabUtility.InstantiatePrefab(prefab)"))
        self.assertEqual(1, method.count("instance.transform.SetParent(r03.transform, false)"))


if __name__ == "__main__":
    unittest.main()
