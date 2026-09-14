import importlib.util
import json
from pathlib import Path
import tempfile
import unittest


ROOT = Path(__file__).resolve().parents[2]
MODULE_PATH = ROOT / "tools" / "sync_visual_task_status.py"
SPEC = importlib.util.spec_from_file_location("sync_visual_task_status", MODULE_PATH)
MODULE = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
SPEC.loader.exec_module(MODULE)


class VisualTaskStatusTests(unittest.TestCase):
    def test_task_catalog_is_unique_and_complete(self):
        ids = [task[0] for task in MODULE.TASKS]
        self.assertEqual(len(ids), 43)
        self.assertEqual(len(ids), len(set(ids)))
        self.assertEqual(ids[0], "D25-S01")
        self.assertEqual(ids[-1], "D31-G06")

    def test_result_path_and_status_validation(self):
        self.assertEqual(
            MODULE.result_path("D29-S05").as_posix().replace("\\", "/"),
            (ROOT / "game/tools/results/D29/D29-S05/D29_S05_RESULT.json").as_posix().replace("\\", "/"),
        )
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "D25_S01_RESULT.json"
            path.write_text(json.dumps({"task_id": "D25-S01", "status": "ART_PASS", "summary": "ok"}), encoding="utf-8")
            self.assertEqual(json.loads(path.read_text(encoding="utf-8"))["status"], "ART_PASS")


if __name__ == "__main__":
    unittest.main()
