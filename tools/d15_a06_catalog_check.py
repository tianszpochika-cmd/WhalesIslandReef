"""Validate the D15-A06 batch without loading Unity or executing ShipSystem."""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path
from typing import Any, Dict, Iterable, List


ROOT = Path(__file__).resolve().parents[1]
BATCH = ROOT / "game" / "Assets" / "Content" / "D15" / "A06"
ID_PATTERN = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)+$")
TEST_ID_PATTERN = re.compile(r"^D8-A02-[A-Z0-9-]+$")
FORBIDDEN_FIELDS = {"weight", "draft", "damage", "speed", "price", "stat_modifiers", "risk_modifiers", "effect_formula"}


def load(path: Path) -> Dict[str, Any]:
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise AssertionError("JSON root must be an object: %s" % path)
    return value


def walk_keys(value: Any) -> Iterable[str]:
    if isinstance(value, dict):
        for key, child in value.items():
            yield key
            yield from walk_keys(child)
    elif isinstance(value, list):
        for child in value:
            yield from walk_keys(child)


def check_entry(path: Path, expected_prefix: str, known_assets: set[str], category: str) -> Dict[str, Any]:
    entry = load(path)
    content_id = entry.get("id")
    assert isinstance(content_id, str) and content_id.startswith(expected_prefix), "%s: invalid ID" % path
    assert ID_PATTERN.fullmatch(content_id), "%s: ID is not lower snake case" % content_id
    assert entry.get("category") == category, "%s: category mismatch" % content_id
    for field in ("name", "display_name_key", "source_refs", "source_note", "source", "license", "historical_tag", "phase", "owner_system", "test_id"):
        assert field in entry, "%s: missing D8 field %s" % (content_id, field)
    assert isinstance(entry["source_refs"], list) and entry["source_refs"], "%s: missing source refs" % content_id
    assert isinstance(entry["source"], dict) and entry["source"].get("kind"), "%s: missing source provenance" % content_id
    assert isinstance(entry["license"], dict) and entry["license"].get("status"), "%s: missing license" % content_id
    assert entry["license"].get("commercial_use") is True, "%s: commercial authorization is not explicit" % content_id
    assert entry["historical_tag"] and entry["phase"] and entry["owner_system"], "%s: incomplete history/phase/owner" % content_id
    assert isinstance(entry["test_id"], str) and TEST_ID_PATTERN.fullmatch(entry["test_id"]), "%s: invalid test_id" % content_id
    for field in ("model_ref", "collision_ref", "anchor_refs", "animation_refs", "audio_refs", "ui_ref", "test_cases", "production_batch", "production_stage", "asset_state"):
        assert field in entry, "%s: missing %s" % (content_id, field)
    assert entry["model_ref"] in known_assets, "%s: model_ref is not an asset entry" % content_id
    assert entry.get("material_ref") in known_assets, "%s: material_ref is not an asset entry" % content_id
    assert entry["collision_ref"] in known_assets, "%s: collision_ref is not an asset entry" % content_id
    assert entry.get("anchor_data_ref") in known_assets, "%s: anchor_data_ref is not an asset entry" % content_id
    assert entry["ui_ref"] in known_assets, "%s: ui_ref is not an asset entry" % content_id
    assert isinstance(entry["anchor_refs"], list) and entry["anchor_refs"], "%s: missing anchors" % content_id
    assert isinstance(entry["animation_refs"], list) and entry["animation_refs"], "%s: missing animation refs" % content_id
    assert isinstance(entry["audio_refs"], list) and entry["audio_refs"], "%s: missing audio refs" % content_id
    for ref in entry["animation_refs"] + entry["audio_refs"]:
        assert ref in known_assets, "%s: missing asset ref %s" % (content_id, ref)
    assert entry["production_batch"] == "D15-A06*", "%s: wrong production batch" % content_id
    assert entry["production_stage"] in {"integrated", "verified"}, "%s: batch not integrated" % content_id
    assert entry["asset_state"] in {"integrated", "verified"}, "%s: asset not integrated" % content_id
    assert isinstance(entry.get("test_cases"), list) and len(entry["test_cases"]) >= 4, "%s: incomplete test cases" % content_id
    assert not (FORBIDDEN_FIELDS & set(walk_keys(entry))), "%s: contains strong-logic field %s" % (content_id, FORBIDDEN_FIELDS & set(walk_keys(entry)))
    assert path.stem == content_id, "%s: filename mismatch" % content_id
    return entry


def main() -> int:
    manifest = load(BATCH / "catalog.json")
    counts = manifest["counts"]
    assert counts["hulls"] == 20, counts
    assert counts["modules"] == 80, counts
    assert counts["functional_points"] == 60, counts
    asset_entries = {path.stem: load(path) for path in (BATCH / "entries" / "assets").glob("*.json")}
    assert len(asset_entries) == counts["asset_resources"], "asset count mismatch"
    assert all(key.startswith("asset_") for key in asset_entries), "asset prefix mismatch"
    for asset_id, asset in asset_entries.items():
        assert asset.get("asset_state") in {"integrated", "verified"}, "%s missing asset state" % asset_id
        assert asset.get("resource_path"), "%s missing resource path" % asset_id
        assert asset.get("source_refs") and asset.get("source", {}).get("kind"), "%s missing source provenance" % asset_id
        assert asset.get("license", {}).get("commercial_use") is True, "%s missing commercial authorization" % asset_id
        assert TEST_ID_PATTERN.fullmatch(asset.get("test_id", "")), "%s invalid test_id" % asset_id
    known_assets = set(asset_entries)

    hulls = [check_entry(path, "ship_", known_assets, "ship") for path in sorted((BATCH / "entries" / "hulls").glob("*.json"))]
    modules = [check_entry(path, "item_", known_assets, "item") for path in sorted((BATCH / "entries" / "modules").glob("*.json"))]
    functions = [check_entry(path, "ship_", known_assets, "ship") for path in sorted((BATCH / "entries" / "functional_points").glob("*.json"))]
    assert len(hulls) == 20 and len(modules) == 80 and len(functions) == 60
    assert all("functional_point" not in item["id"] for item in hulls), "functional points mixed into hull set"
    assert all("ship_module" in item["id"] for item in modules), "module ID rule mismatch"
    assert all("ship_function_" in item["id"] for item in functions), "functional point ID rule mismatch"

    for entry in hulls + modules + functions:
        model_path = ROOT / entry["model_path"]
        assert model_path.is_file(), "%s: model file missing %s" % (entry["id"], model_path)
        assert (BATCH / "assets" / "profiles" / "collision_profiles.json").is_file()
        assert (BATCH / "assets" / "profiles" / "anchor_profiles.json").is_file()
        assert (BATCH / "assets" / "profiles" / "animation_profiles.json").is_file()
        assert (BATCH / "assets" / "profiles" / "audio_profiles.json").is_file()
        assert (BATCH / "assets" / "profiles" / "ui_profiles.json").is_file()

    samples = manifest["representative_samples"]
    assert load(BATCH / "entries" / "hulls" / (samples["hull"] + ".json"))["representative_sample"]
    assert load(BATCH / "entries" / "modules" / (samples["module"] + ".json"))["representative_sample"]
    assert load(BATCH / "entries" / "functional_points" / (samples["functional_point"] + ".json"))["representative_sample"]
    print("D15-A06 catalog PASS: hulls=%d modules=%d functional_points=%d assets=%d; models/collision/anchors/animation/audio/UI/test fields complete" % (len(hulls), len(modules), len(functions), len(asset_entries)))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (AssertionError, KeyError, json.JSONDecodeError) as exc:
        print("D15-A06 catalog FAILED: %s" % exc)
        raise SystemExit(1)
