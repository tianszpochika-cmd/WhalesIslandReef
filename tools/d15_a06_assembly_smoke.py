"""Smoke-test the D15-A06 representative content handoff without Unity."""

from __future__ import annotations

import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
BATCH = ROOT / "game" / "Assets" / "Content" / "D15" / "A06"


def load(relative: str) -> dict:
    path = BATCH / relative
    return json.loads(path.read_text(encoding="utf-8"))


def find_entry(folder: str, content_id: str) -> dict:
    return load(f"entries/{folder}/{content_id}.json")


def main() -> int:
    manifest = load("catalog.json")
    samples = manifest["representative_samples"]
    hull = find_entry("hulls", samples["hull"])
    module = find_entry("modules", samples["module"])
    function = find_entry("functional_points", samples["functional_point"])

    assert module["references"] and hull["id"] in module["references"], "sample module cannot target sample hull"
    required_point = module["required_functional_point"]
    assert required_point in hull["functional_point_slots"], "sample module slot is not present on sample hull"
    assert function["id"] in hull["functional_point_slots"], "sample function point is not mountable on sample hull"
    assert function["interaction_entry"].startswith("ShipSystem.FunctionalPoint."), "function owner entry is not ShipSystem"
    assert function["owner_handoff"] == "ShipSystem", "function owner handoff is not ShipSystem"

    profile_dir = BATCH / "assets" / "profiles"
    profile_names = (
        "collision_profiles.json",
        "anchor_profiles.json",
        "animation_profiles.json",
        "audio_profiles.json",
        "ui_profiles.json",
    )
    for name in profile_names:
        profile = json.loads((profile_dir / name).read_text(encoding="utf-8"))
        assert profile.get("task") == "D15-A06*", f"profile task mismatch: {name}"

    for entry in (hull, module, function):
        model_path = ROOT / entry["model_path"]
        assert model_path.is_file(), f"representative model missing: {entry['id']}"
        assert entry["collision_ref"].startswith("asset_"), f"representative collision not asset-backed: {entry['id']}"
        assert entry["anchor_refs"], f"representative anchors missing: {entry['id']}"
        assert entry["animation_refs"] and entry["audio_refs"], f"representative presentation refs missing: {entry['id']}"
        assert entry["ui_ref"].startswith("asset_"), f"representative UI not asset-backed: {entry['id']}"
        assert len(entry["test_cases"]) >= 4, f"representative test cases incomplete: {entry['id']}"

    print(
        "D15-A06 assembly smoke PASS: %s + %s + %s; slot, owner handoff, model, collision, anchors, animation, audio, UI and save/restore test metadata verified"
        % (hull["id"], module["id"], function["id"])
    )
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (AssertionError, KeyError, json.JSONDecodeError) as exc:
        print(f"D15-A06 assembly smoke FAILED: {exc}")
        raise SystemExit(1)
