import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d15_a08_content_check import (  # noqa: E402
    D15A08CatalogError,
    _validate_localization_maps,
    validate_catalog,
)


class D15A08ContentCheckTests(unittest.TestCase):
    def test_d15_a08_catalog_passes(self) -> None:
        counts = validate_catalog(
            ROOT / "game" / "Assets" / "Resources" / "ContentRegistry",
            ROOT / "game" / "Assets" / "Resources" / "Localization" / "D15_A08",
        )
        self.assertEqual(
            counts,
            {
                "mainline": 5,
                "side_templates": 120,
                "music_themes": 30,
                "audio_layers": 8,
                "localization_keys": 452,
            },
        )

    def test_localization_checker_detects_missing_and_orphan_keys(self) -> None:
        expected = {"content.example.name"}
        with self.assertRaisesRegex(D15A08CatalogError, "missing"):
            _validate_localization_maps(expected, {}, {"content.example.name": "Example"})
        with self.assertRaisesRegex(D15A08CatalogError, "orphan"):
            _validate_localization_maps(
                expected,
                {"content.example.name": "示例", "content.orphan": "孤儿"},
                {"content.example.name": "Example"},
            )


if __name__ == "__main__":
    unittest.main()
