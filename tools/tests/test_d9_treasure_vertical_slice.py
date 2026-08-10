import json
import unittest
from pathlib import Path

from tools.content_registry_check import validate_registry


ROOT = Path(__file__).resolve().parents[2]
QUEST_PATH = ROOT / "game/Assets/Resources/ContentRegistry/entries/quest/quest_first_chart.json"
SYSTEM_PATH = ROOT / "game/Assets/Scripts/Systems/D9/D9TreasureQuestSystem.cs"


class D9TreasureVerticalSliceTests(unittest.TestCase):
    def test_content_sample_has_non_puzzle_routes_and_equipment(self):
        quest = json.loads(QUEST_PATH.read_text(encoding="utf-8"))

        self.assertEqual("quest_first_chart", quest["id"])
        self.assertEqual("landmark:saint_marys_chart_cove", quest["site_ref"])
        self.assertEqual(["item_chart_compass"], quest["required_equipment"])
        self.assertFalse(quest["puzzle_gate"])
        self.assertGreaterEqual(len(quest["recovery_routes"]), 3)

    def test_content_registry_and_system_owner_are_present(self):
        errors = validate_registry(ROOT / "game/Assets/Resources/ContentRegistry")
        self.assertEqual([], errors)

        source = SYSTEM_PATH.read_text(encoding="utf-8")
        for event_id in (
            "event_d9_treasure_clue",
            "event_d9_treasure_resolved",
            "event_d9_treasure_recovered",
        ):
            self.assertIn(event_id, source)
        self.assertIn('OwnerSystem = "QuestSystem"', source)
        self.assertNotRegex(source, r"HttpClient|UnityWebRequest|LLMClient|Ollama|DeepSeek|System\.Net")


if __name__ == "__main__":
    unittest.main()
