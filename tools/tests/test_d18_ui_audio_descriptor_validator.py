import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_ui_audio_descriptor_validator import (  # noqa: E402
    DESCRIPTOR_KIND,
    discover_descriptor_files,
    extract_taxonomy_json,
    validate_ui_audio_descriptors,
)

TAXONOMY = ROOT / "docs/game/design/common/assets/D18_TAXONOMY_UI_AUDIO.md"
LOCALIZATION = ROOT / "game/Assets/Resources/Localization/D15_A08"


def make_descriptor(category="ui_icon", purpose="feedback_icon", runtime_intent="ui_feedback_display", license_id="CC0-1.0", **overrides):
    data = {
        "descriptor_kind": DESCRIPTOR_KIND,
        "descriptor_schema_version": 1,
        "canonical_asset_id": "asset_ui_fixture_icon_v1",
        "category": category,
        "purpose": purpose,
        "runtime_intent": runtime_intent,
        "source_id": "source_fixture",
        "license": {
            "license_id": license_id,
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": "docs/game/design/common/assets/packs/fixture/LICENSE.txt",
            "attribution_required": False,
            "rights": {
                "commercial_use": True,
                "modification_allowed": True,
                "repository_storage": True,
                "distribution_with_game": True,
            },
        },
        "localization": {
            "accessibility_label_key": "ui.fixture_batch.asset_ui_fixture_icon_v1.alt",
            "name_key": "ui.fixture_batch.asset_ui_fixture_icon_v1.name",
        },
        "fallback": {"kind": "alt_text", "reference": "ui.fixture_batch.asset_ui_fixture_icon_v1.alt"},
        "runtime_deferred": ["import", "canvas_wiring"],
        "evidence_paths": ["docs/game/design/common/assets/packs/fixture/LICENSE.txt"],
        "notes": "fixture",
    }
    data.update(overrides)
    return data


def audio_descriptor(**overrides):
    data = make_descriptor(
        category="audio_cue",
        purpose="ui_feedback_cue",
        runtime_intent="audio_feedback_playback",
        canonical_asset_id="asset_audio_fixture_click_v1",
        localization={
            "subtitle_key": "audio.fixture_batch.asset_audio_fixture_click_v1.subtitle",
            "name_key": "audio.fixture_batch.asset_audio_fixture_click_v1.name",
        },
        fallback={"kind": "subtitle", "reference": "audio.fixture_batch.asset_audio_fixture_click_v1.subtitle"},
        runtime_deferred=["import", "audiosource_wiring", "mixer_wiring", "volume"],
    )
    data.update(overrides)
    return data


class Tree:
    def __init__(self, tmpdir):
        self.root = Path(tmpdir)
        self.descriptor_dir = self.root / "tools/fixtures/descriptors"
        self.descriptor_dir.mkdir(parents=True, exist_ok=True)

    def write_descriptor(self, name, data):
        path = self.descriptor_dir / name
        path.write_text(json.dumps(data, ensure_ascii=False, indent=1), encoding="utf-8")
        return path

    def write_evidence(self, rel="docs/game/design/common/assets/packs/fixture/LICENSE.txt"):
        path = self.root / rel
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text("fixture evidence", encoding="utf-8")
        return path

    def write_localization(self, keys):
        path = self.root / "localization/zh-CN.json"
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(json.dumps({"schema_version": 1, "locale": "zh-CN", "batch": "fixture", "keys": {k: "值" for k in keys}}, ensure_ascii=False, indent=1), encoding="utf-8")
        return path

    def snapshot(self):
        return sorted(
            (str(p.relative_to(self.root)).replace("\\", "/"), p.stat().st_size, int(p.stat().st_mtime))
            for p in self.root.rglob("*")
            if p.is_file()
        )


class D18UiAudioDescriptorValidatorTests(unittest.TestCase):
    def validate(self, tree, data_or_paths, taxonomy=TAXONOMY, localization=None):
        if isinstance(data_or_paths, (list, tuple)):
            paths = data_or_paths
        else:
            paths = [tree.write_descriptor("fixture.json", data_or_paths)]
        return validate_ui_audio_descriptors(tree.root, paths, taxonomy_path=taxonomy, localization=localization)

    def assert_blocked(self, result, *codes):
        self.assertEqual(result["status"], "BLOCKED")
        self.assertTrue(result["read_only"])
        self.assertFalse(result["registry_modified"])
        for code in codes:
            self.assertIn(code, result["issue_counts"], result["issues"])

    def assert_taxonomy_matched(self, result):
        self.assertTrue(result["taxonomy_check"]["matched"], result["taxonomy_check"])

    def test_valid_ui_icon_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            result = self.validate(tree, make_descriptor())
            self.assertEqual(result["status"], "PASS")
            self.assert_taxonomy_matched(result)
            self.assertEqual(result["descriptor_summary"]["passed"], 1)

    def test_valid_input_prompt_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(
                category="input_prompt",
                purpose="input_action_hint",
                runtime_intent="input_hint_display",
                canonical_asset_id="asset_input_fixture_confirm_v1",
                localization={
                    "accessibility_label_key": "ui.fixture_batch.asset_input_fixture_confirm_v1.alt",
                    "input_action_id": "confirm",
                },
                fallback={"kind": "alt_text", "reference": "ui.fixture_batch.asset_input_fixture_confirm_v1.alt"},
            )
            result = self.validate(tree, data)
            self.assertEqual(result["status"], "PASS")

    def test_valid_audio_cue_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            result = self.validate(tree, audio_descriptor())
            self.assertEqual(result["status"], "PASS")

    def test_valid_music_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = audio_descriptor(
                category="music",
                purpose="theme_music",
                runtime_intent="music_playback",
                canonical_asset_id="asset_audio_fixture_theme_v1",
                localization={"subtitle_key": "audio.fixture_batch.asset_audio_fixture_theme_v1.subtitle"},
                fallback={"kind": "subtitle", "reference": "audio.fixture_batch.asset_audio_fixture_theme_v1.subtitle"},
            )
            result = self.validate(tree, data)
            self.assertEqual(result["status"], "PASS")

    def test_valid_font_ofl_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(
                category="font",
                purpose="ui_text_font",
                runtime_intent="text_rendering",
                canonical_asset_id="asset_font_fixture_v1",
                license_id="OFL-1.1",
                localization={},
                fallback={"kind": "system_font", "reference": "system_default"},
            )
            data["license"]["attribution_required"] = True
            result = self.validate(tree, data)
            self.assertEqual(result["status"], "PASS")

    def test_font_cc0_fails_without_explicit_embedding(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(
                category="font",
                purpose="ui_text_font",
                runtime_intent="text_rendering",
                canonical_asset_id="asset_font_fixture_v1",
                license_id="CC0-1.0",
                localization={},
                fallback={"kind": "system_font", "reference": "system_default"},
            )
            result = self.validate(tree, data)
            self.assert_blocked(result, "font_license_not_ofl")

    def test_font_explicit_embedding_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(
                category="font",
                purpose="ui_text_font",
                runtime_intent="text_rendering",
                canonical_asset_id="asset_font_fixture_v1",
                license_id="CC0-1.0",
                license={"license_id": "CC0-1.0", "license_url": "https://creativecommons.org/publicdomain/zero/1.0/", "license_evidence_path": "docs/game/design/common/assets/packs/fixture/LICENSE.txt", "attribution_required": False, "embedding_license_explicit": True, "rights": {"commercial_use": True, "modification_allowed": True, "repository_storage": True, "distribution_with_game": True}},
                localization={},
                fallback={"kind": "system_font", "reference": "system_default"},
            )
            result = self.validate(tree, data)
            self.assertEqual(result["status"], "PASS")

    def test_unknown_purpose_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            result = self.validate(tree, make_descriptor(purpose="mystery_purpose"))
            self.assert_blocked(result, "unknown_purpose")

    def test_purpose_category_mismatch_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            result = self.validate(tree, make_descriptor(category="ui_icon", purpose="theme_music", runtime_intent="ui_feedback_display"))
            self.assert_blocked(result, "purpose_category_mismatch")

    def test_missing_purpose_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            del data["purpose"]
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_purpose")

    def test_missing_category_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            del data["category"]
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_category")

    def test_unsupported_material_category_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            result = self.validate(tree, make_descriptor(category="material", purpose="feedback_icon"))
            self.assert_blocked(result, "unsupported_category")

    def test_unknown_category_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            result = self.validate(tree, make_descriptor(category="widget"))
            self.assert_blocked(result, "unknown_category")

    def test_missing_license_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            del data["license"]
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_license")

    def test_missing_license_field_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            del data["license"]["license_url"]
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_license_field")

    def test_nc_license_blocked(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            result = self.validate(tree, make_descriptor(license_id="CC-BY-NC-4.0"))
            self.assert_blocked(result, "license_not_normalized")

    def test_audio_requires_distribution_with_game(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = audio_descriptor()
            data["license"]["rights"]["distribution_with_game"] = False
            result = self.validate(tree, data)
            self.assert_blocked(result, "distribution_with_game_required", "license_not_normalized")

    def test_license_evidence_missing_file_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            data = make_descriptor()
            data["license"]["license_evidence_path"] = "docs/game/design/common/assets/packs/fixture/NOT_THERE.txt"
            result = self.validate(tree, data)
            self.assert_blocked(result, "license_evidence_missing")

    def test_evidence_path_escape_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            data["evidence_paths"] = ["../outside.txt"]
            result = self.validate(tree, data)
            self.assert_blocked(result, "invalid_evidence_path")

    def test_missing_accessibility_label_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            del data["localization"]["accessibility_label_key"]
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_accessibility_label")

    def test_missing_subtitle_key_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = audio_descriptor()
            del data["localization"]["subtitle_key"]
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_subtitle_key")

    def test_hardcoded_label_text_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            data["localization"]["accessibility_label_key"] = "返回按钮"
            result = self.validate(tree, data)
            self.assert_blocked(result, "hardcoded_label_text")

    def test_invalid_label_key_format_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            data["localization"]["accessibility_label_key"] = "Ui.Batch.Key.Alt"
            result = self.validate(tree, data)
            self.assert_blocked(result, "invalid_label_key")

    def test_missing_input_action_id_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(
                category="input_prompt",
                purpose="input_action_hint",
                runtime_intent="input_hint_display",
                canonical_asset_id="asset_input_fixture_confirm_v1",
                localization={"accessibility_label_key": "ui.fixture_batch.asset_input_fixture_confirm_v1.alt"},
                fallback={"kind": "alt_text", "reference": "ui.fixture_batch.asset_input_fixture_confirm_v1.alt"},
            )
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_input_action_id")

    def test_keycap_text_as_action_id_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(
                category="input_prompt",
                purpose="input_action_hint",
                runtime_intent="input_hint_display",
                canonical_asset_id="asset_input_fixture_confirm_v1",
                localization={"accessibility_label_key": "ui.fixture_batch.asset_input_fixture_confirm_v1.alt", "input_action_id": "按 Y 确认"},
                fallback={"kind": "alt_text", "reference": "ui.fixture_batch.asset_input_fixture_confirm_v1.alt"},
            )
            result = self.validate(tree, data)
            self.assert_blocked(result, "invalid_input_action_id")

    def test_missing_fallback_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            del data["fallback"]
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_fallback")

    def test_fallback_none_without_reason_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(fallback={"kind": "none"})
            result = self.validate(tree, data)
            self.assert_blocked(result, "fallback_none_without_reason")

    def test_fallback_none_with_reason_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(fallback={"kind": "none", "reason": "decorative panel slice without meaning"})
            result = self.validate(tree, data)
            self.assertEqual(result["status"], "PASS")

    def test_fallback_missing_reference_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(fallback={"kind": "alt_text"})
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_fallback_reference")

    def test_fallback_reference_mismatch_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(fallback={"kind": "alt_text", "reference": "ui.fixture_batch.other.alt"})
            result = self.validate(tree, data)
            self.assert_blocked(result, "fallback_reference_mismatch")

    def test_silent_fallback_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = audio_descriptor(fallback={"kind": "silent"})
            result = self.validate(tree, data)
            self.assertEqual(result["status"], "PASS")

    def test_unknown_fallback_kind_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = audio_descriptor(fallback={"kind": "popup", "reference": "x"})
            result = self.validate(tree, data)
            self.assert_blocked(result, "unknown_fallback_kind")

    def test_missing_runtime_intent_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            del data["runtime_intent"]
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_runtime_intent")

    def test_unknown_runtime_intent_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(runtime_intent="play_sound_now")
            result = self.validate(tree, data)
            self.assert_blocked(result, "unknown_runtime_intent")

    def test_purpose_runtime_intent_mismatch_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(purpose="feedback_icon", runtime_intent="panel_asset")
            result = self.validate(tree, data)
            self.assert_blocked(result, "purpose_runtime_intent_mismatch")

    def test_forbidden_runtime_field_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(volume=0.8)
            result = self.validate(tree, data)
            self.assert_blocked(result, "forbidden_runtime_field")

    def test_forbidden_nested_runtime_field_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(import_settings={"max_texture_size": 1024})
            result = self.validate(tree, data)
            self.assert_blocked(result, "forbidden_runtime_field")

    def test_missing_runtime_deferred_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            del data["runtime_deferred"]
            result = self.validate(tree, data)
            self.assert_blocked(result, "missing_runtime_deferred")

    def test_empty_runtime_deferred_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(runtime_deferred=[])
            result = self.validate(tree, data)
            self.assert_blocked(result, "empty_runtime_deferred")

    def test_invalid_asset_id_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(canonical_asset_id="Assets/Art/Candidates/ui/icon.png")
            result = self.validate(tree, data)
            self.assert_blocked(result, "invalid_asset_id")

    def test_invalid_source_id_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(source_id="KenneySource")
            result = self.validate(tree, data)
            self.assert_blocked(result, "invalid_source_id")

    def test_invalid_variant_reference_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor(localization={"accessibility_label_key": "ui.fixture_batch.asset_ui_fixture_icon_v1.alt", "contrast_variant": "ui/high_contrast.png"})
            result = self.validate(tree, data)
            self.assert_blocked(result, "invalid_variant_reference")

    def test_localization_key_existence_check(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            loc = tree.write_localization(
                [
                    "ui.fixture_batch.asset_ui_fixture_icon_v1.alt",
                    "ui.fixture_batch.asset_ui_fixture_icon_v1.name",
                ]
            )
            result = self.validate(tree, make_descriptor(), localization=loc)
            self.assertEqual(result["status"], "PASS")
            self.assertEqual(result["localization_key_count"], 2)

    def test_localization_key_missing_from_set_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            loc = tree.write_localization(["content.quest.quest_x.name"])
            result = self.validate(tree, make_descriptor(), localization=loc)
            self.assert_blocked(result, "label_key_not_in_localization")

    def test_localization_empty_set_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            loc = tree.write_localization([])
            result = self.validate(tree, make_descriptor(), localization=loc)
            self.assert_blocked(result, "localization_empty")

    def test_invalid_descriptor_kind_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            data = make_descriptor()
            data["descriptor_kind"] = "d18_mesh_descriptor"
            result = self.validate(tree, data)
            self.assert_blocked(result, "invalid_descriptor_kind")

    def test_descriptor_not_object_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            path = tree.descriptor_dir / "list.json"
            path.write_text('[1, 2, 3]', encoding="utf-8")
            result = self.validate(tree, [path])
            self.assert_blocked(result, "descriptor_not_object")

    def test_no_descriptors_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            result = self.validate(tree, [tree.descriptor_dir / "empty.json"] if (tree.descriptor_dir / "empty.json").exists() else [tree.descriptor_dir / "missing.json"])
            self.assert_blocked(result, "no_descriptors")

    def test_manifest_files_are_skipped(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            manifest = tree.descriptor_dir / "not_a_descriptor.json"
            manifest.write_text(json.dumps({"manifest_kind": "d18_asset_pack_manifest"}), encoding="utf-8")
            result = self.validate(tree, [manifest])
            self.assert_blocked(result, "no_descriptors")

    def test_taxonomy_cross_check_passes(self):
        taxonomy = extract_taxonomy_json(TAXONOMY)
        self.assertIsNotNone(taxonomy)
        self.assertEqual(taxonomy.get("task_id"), "D18-L14")

    def test_tool_read_only_never_writes(self):
        with tempfile.TemporaryDirectory() as tmp:
            tree = Tree(tmp)
            tree.write_evidence()
            self.validate(tree, make_descriptor())
            before = tree.snapshot()
            result = self.validate(tree, make_descriptor())
            after = tree.snapshot()
            self.assertEqual(result["status"], "PASS")
            self.assertTrue(result["read_only"])
            self.assertFalse(result["registry_modified"])
            self.assertEqual(before, after)

    def test_real_taxonomy_and_localization_scan(self):
        taxonomy = extract_taxonomy_json(TAXONOMY)
        self.assertIsNotNone(taxonomy)
        result = validate_ui_audio_descriptors(ROOT, [ROOT / "docs/game/design/common/assets/packs"], taxonomy_path=TAXONOMY, localization=LOCALIZATION)
        self.assertTrue(result["taxonomy_check"]["matched"])
        self.assertIn("no_descriptors", result["issue_counts"])
        self.assertEqual(result["localization_key_count"], 452)
        self.assertEqual(result["localization_files"], 2)


if __name__ == "__main__":
    unittest.main()
