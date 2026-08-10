"""Validate the D9-S01 vertical-slice freeze without loading Unity or game code."""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path
from typing import Any, Dict, List


REPOSITORY_ROOT = Path(__file__).resolve().parents[1]
FREEZE_PATH = REPOSITORY_ROOT / "docs" / "game" / "design" / "common" / "D9_VERTICAL_SLICE_FREEZE.md"
D8_ACCEPTANCE_PATH = REPOSITORY_ROOT / "docs" / "game" / "design" / "common" / "D8_ACCEPTANCE_REPORT.md"
MANIFEST_START = "<!-- D9_FREEZE_MANIFEST_BEGIN -->"
MANIFEST_END = "<!-- D9_FREEZE_MANIFEST_END -->"


def _read(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def _manifest() -> Dict[str, Any]:
    document = _read(FREEZE_PATH)
    if MANIFEST_START not in document or MANIFEST_END not in document:
        raise ValueError("D9 freeze manifest markers are missing")
    payload = document.split(MANIFEST_START, 1)[1].split(MANIFEST_END, 1)[0]
    match = re.search(r"```json\s*(\{.*?\})\s*```", payload, flags=re.DOTALL)
    if match is None:
        raise ValueError("D9 freeze JSON block is missing")
    value = json.loads(match.group(1))
    if not isinstance(value, dict):
        raise ValueError("D9 freeze manifest must be an object")
    return value


def _require_path(errors: List[str], relative_path: str) -> None:
    if not (REPOSITORY_ROOT / relative_path).is_file():
        errors.append(f"missing entry path: {relative_path}")


def _registry_ids() -> set[str]:
    registry_root = REPOSITORY_ROOT / "game" / "Assets" / "Resources" / "ContentRegistry"
    ids = set()
    for path in (registry_root / "entries").rglob("*.json"):
        value = json.loads(path.read_text(encoding="utf-8"))
        ids.add(value.get("id", path.stem))
    return ids


def _systems_red_line_errors() -> List[str]:
    patterns = re.compile(r"HttpClient|UnityWebRequest|WebRequest|System\.Net|LLMClient|Ollama|DeepSeek|https?://")
    errors: List[str] = []
    systems_root = REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Systems"
    for path in systems_root.rglob("*.cs"):
        for line_number, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
            stripped = line.strip()
            if stripped.startswith("//") or stripped.startswith("*"):
                continue
            if patterns.search(line):
                errors.append(f"Systems red-line reference: {path.relative_to(REPOSITORY_ROOT)}:{line_number}")
    return errors


def validate_freeze() -> List[str]:
    errors: List[str] = []
    if not FREEZE_PATH.is_file():
        return [f"missing freeze document: {FREEZE_PATH}"]
    if not D8_ACCEPTANCE_PATH.is_file():
        errors.append("missing D8 acceptance report")
    else:
        d8 = _read(D8_ACCEPTANCE_PATH)
        if "D8 通过" not in d8 or "允许进入 D9" not in d8:
            errors.append("D8 acceptance report does not authorize D9")

    try:
        manifest = _manifest()
    except (OSError, ValueError, json.JSONDecodeError) as exc:
        return errors + [f"invalid D9 freeze manifest: {exc}"]

    if manifest.get("task_id") != "D9-S01":
        errors.append("manifest task_id must be D9-S01")
    if manifest.get("freeze_version") != "d9.vertical_slice.v1":
        errors.append("manifest freeze_version must be d9.vertical_slice.v1")

    scene = manifest.get("scene_boundary", {})
    if scene.get("entry_scene") != "game/Assets/Scenes/InitialHarbor.unity":
        errors.append("D9 entry scene must be InitialHarbor")
    for path in scene.get("preserved_scenes", []):
        _require_path(errors, path)
    logical_nodes = {node.get("kind"): node for node in scene.get("logical_nodes", [])}
    for kind in ("harbor", "village", "nearshore_fishing_ground"):
        if kind not in logical_nodes:
            errors.append(f"missing logical scene node: {kind}")
    if len(scene.get("preserved_scenes", [])) != 3:
        errors.append("D9 must preserve exactly the three D7 scene entries")
    if "driftwood_isle" not in scene.get("excluded_legacy_nodes", []):
        errors.append("D3 legacy driftwood_isle must be explicitly excluded from the D9 slice")

    for prefab in manifest.get("prefab_entries", []):
        _require_path(errors, prefab.get("path", ""))
    if len(manifest.get("prefab_entries", [])) != 5:
        errors.append("Prefab entry count must remain five frozen D7/D8 entries")

    samples = manifest.get("data_samples", {})
    required_samples = {
        "harbor",
        "village",
        "nearshore_fishing_ground",
        "ship",
        "commodities",
        "core_crew_agent",
        "weather_event",
        "treasure",
        "combat",
    }
    missing_samples = sorted(required_samples - set(samples))
    if missing_samples:
        errors.append(f"missing data samples: {', '.join(missing_samples)}")
    if len(samples.get("commodities", [])) != 10:
        errors.append("D9 must freeze exactly ten commodities")

    required_content_ids = {
        "island_saint_marys",
        "building_saint_marys_dock",
        "ship_moonwake",
        "crew_isaac_reed",
        "fish_mackerel_school",
        "quest_first_chart",
        "reward_first_chart",
        "asset_ph_ship_fishing",
        "asset_ph_crew_male",
    }
    try:
        content_ids = _registry_ids()
        missing_content = sorted(required_content_ids - content_ids)
        if missing_content:
            errors.append(f"missing D8 content IDs: {', '.join(missing_content)}")
    except (OSError, json.JSONDecodeError) as exc:
        errors.append(f"content registry could not be read: {exc}")

    commodity_ids = [item.get("id") for item in samples.get("commodities", [])]
    catalog_path = REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Systems" / "D3" / "CommodityCatalog.cs"
    catalog_text = _read(catalog_path)
    code_ids = re.findall(r'new CommodityDefinition\("([^"]+)"', catalog_text)
    if commodity_ids != code_ids:
        errors.append(f"freeze commodity list does not match CommodityCatalog.All: {commodity_ids} != {code_ids}")

    world_catalog_path = REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Systems" / "D3" / "WorldCatalog.cs"
    world_catalog = _read(world_catalog_path)
    if 'GreenCayId = "green_cay"' not in world_catalog or "D3LocationType.Village" not in world_catalog:
        errors.append("D3 WorldCatalog no longer exposes the frozen green_cay village")
    if "D3WeatherType.Storm" not in world_catalog:
        errors.append("D3 WeatherCatalog no longer exposes the frozen Storm state")

    combat_path = REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D4" / "D4RuntimeBootstrap.cs"
    combat_text = _read(combat_path)
    combat_system_path = REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Systems" / "D4" / "D4CombatSystem.cs"
    combat_system = _read(combat_system_path)
    if "ph_spanish_patrol" not in combat_text or "Escape(" not in combat_system:
        errors.append("D4 frozen patrol encounter or retreat entry is missing")

    events = manifest.get("event_chain", [])
    event_ids = [event.get("event_id") for event in events]
    if len(event_ids) != 14 or len(set(event_ids)) != len(event_ids):
        errors.append("event chain must contain 14 unique event IDs")
    for event in events:
        if not event.get("owner_system") or not event.get("test_id"):
            errors.append(f"event lacks owner/test boundary: {event}")
        if event.get("owner_system") in set(manifest.get("forbidden_owners", [])):
            errors.append(f"forbidden event owner: {event.get('owner_system')}")
    if manifest.get("dedupe_key") != "event_id + save_revision":
        errors.append("D9 event dedupe key must remain event_id + save_revision")

    ui_pages = manifest.get("ui_pages", [])
    if len(ui_pages) != 6 or len({page.get("id") for page in ui_pages}) != 6:
        errors.append("D9 must freeze six unique UI pages")
    for page in ui_pages:
        if page.get("writes") != [] or not page.get("entry") or not page.get("test_id"):
            errors.append(f"UI page has write authority or incomplete boundary: {page.get('id')}")

    budget = manifest.get("performance_budget", {})
    cpu = budget.get("cpu_ms", {})
    if budget.get("target_fps") != 60 or budget.get("frame_ms") != 16.6:
        errors.append("D9 performance target must be 60 FPS / 16.6 ms")
    if round(sum(cpu.get(key, 0) for key in ("domain_and_environment", "input_and_ui", "physics", "render_submission", "headroom")), 1) != 16.6:
        errors.append("D9 CPU budget components must sum to 16.6 ms")
    for key in ("active_dynamic_objects", "visible_placeholder_objects", "physics_contacts_per_frame", "template_agent_reply_ms", "save_restore_ms"):
        if not isinstance(budget.get(key), (int, float)) or budget[key] <= 0:
            errors.append(f"invalid performance budget: {key}")

    expected_agents = {f"D9-A0{index}" for index in range(1, 8)}
    agent_boundaries = manifest.get("agent_boundaries", {})
    if set(agent_boundaries) != expected_agents:
        errors.append("D9 agent boundary matrix must contain D9-A01 through D9-A07 exactly")
    for agent, boundary in agent_boundaries.items():
        for path in boundary.get("entry_points", []):
            _require_path(errors, path)
        for field in ("samples", "test_boundary", "budget"):
            if not boundary.get(field):
                errors.append(f"{agent} missing {field}")

    errors.extend(_systems_red_line_errors())
    return errors


def main() -> int:
    errors = validate_freeze()
    if errors:
        for error in errors:
            print(f"ERROR {error}")
        print(f"D9 vertical slice freeze FAILED: {len(errors)} error(s)")
        return 1
    print("D9 vertical slice freeze OK: scenes=3, prefabs=5, commodities=10, events=14, ui_pages=6, agents=7")
    return 0


if __name__ == "__main__":
    sys.exit(main())
