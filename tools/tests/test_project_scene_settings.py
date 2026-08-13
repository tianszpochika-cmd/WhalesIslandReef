import re
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
UNITY_ROOT = ROOT / "game"
BUILD_SETTINGS = UNITY_ROOT / "ProjectSettings" / "EditorBuildSettings.asset"


class ProjectSceneSettingsTests(unittest.TestCase):
    def test_runtime_scenes_are_enabled_in_stable_boot_order(self):
        content = BUILD_SETTINGS.read_text(encoding="utf-8")
        entries = [match.groupdict() for match in re.finditer(
            r"- (?:serializedVersion: 2\s+)?"
            r"enabled: (?P<enabled>[01])\s+"
            r"path: (?P<path>[^\r\n]+)\s+"
            r"guid: (?P<guid>[0-9a-f]+)",
            content,
        )]
        self.assertEqual(
            [entry["path"] for entry in entries],
            [
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scenes/InitialHarbor.unity",
            ],
            "主流程场景必须保持 MainMenu → Bootstrap → InitialHarbor 顺序。",
        )
        self.assertTrue(all(entry["enabled"] == "1" for entry in entries))

        for entry in entries:
            meta = UNITY_ROOT / (entry["path"] + ".meta")
            self.assertTrue(meta.is_file(), f"场景缺少 meta：{entry['path']}")
            meta_guid = re.search(r"^guid: ([0-9a-f]+)$", meta.read_text(encoding="utf-8"), re.MULTILINE)
            self.assertIsNotNone(meta_guid, f"场景 meta 缺少 guid：{entry['path']}")
            self.assertEqual(entry["guid"], meta_guid.group(1), f"场景 GUID 不一致：{entry['path']}")


if __name__ == "__main__":
    unittest.main()
