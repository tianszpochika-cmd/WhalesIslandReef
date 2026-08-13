import json
import unittest
from pathlib import Path

from tools.d15_a07_batch_check import validate_batch


ROOT = Path(__file__).resolve().parents[2]
BATCH = ROOT / "game" / "Assets" / "Resources" / "ContentRegistry" / "batches" / "d15_a07" / "d15_a07_character_pet_batch.json"
REGISTRY = ROOT / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries"


class D15A07BatchTests(unittest.TestCase):
    def test_batch_meets_frozen_targets_and_shape(self):
        self.assertEqual([], validate_batch(BATCH))

    def test_representative_registry_entries_are_loadable_json(self):
        batch = json.loads(BATCH.read_text(encoding="utf-8"))
        for content_id in batch["representative_sample"]:
            category = "crew" if content_id.startswith("crew_") else "pet" if content_id.startswith("pet_") else "animal"
            path = REGISTRY / category / f"{content_id}.json"
            self.assertTrue(path.is_file(), content_id)
            entry = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual(content_id, entry["id"])
            self.assertTrue(entry["source"]["reference"])
            self.assertTrue(entry["license"]["commercial_use"])
            self.assertTrue(entry["owner_system"])

    def test_npc_instances_have_stable_save_and_spawn_identity(self):
        batch = json.loads(BATCH.read_text(encoding="utf-8"))
        instances = batch["npc_instances"]
        self.assertEqual(500, len(instances))
        self.assertEqual(500, len({item["instance_id"] for item in instances}))
        self.assertEqual(500, len({item["save_key"] for item in instances}))
        self.assertEqual(500, len({item["spawn_anchor_id"] for item in instances}))
        self.assertTrue(all(isinstance(value, (int, float)) for item in instances for value in item["position"]))


if __name__ == "__main__":
    unittest.main()
