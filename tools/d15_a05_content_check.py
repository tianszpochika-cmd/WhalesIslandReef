"""D15-A05 batch content and asset handoff checker.

This checker validates immutable content definitions and the declared handoff
to ItemCatalog/AppearanceSystem. It never writes runtime money, damage,
reward, durability, or equipment decisions.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Dict, Iterable, List, Set, Tuple


BATCH = "D15-A05*"
REQUIRED = {
    "id", "schema_version", "name", "display_name_key", "category", "tags",
    "description", "gameplay_role", "valid_regions", "valid_habitats",
    "valid_weather", "dependencies", "references", "asset_refs", "source_refs",
    "source_note", "source", "license", "historical_tag", "phase", "status",
    "owner_system", "test_id", "reviewer", "production_batch",
    "representative_sample", "production_stage", "batch_role", "content_category",
    "behavior_profile", "asset_state", "quality_level", "model_ref", "material_ref",
    "collision_ref", "lod_profile", "interaction_entry", "function_entry",
    "test_cases", "historical_review", "review_evidence_refs", "content_warning_ids",
    "skip_allowed", "integration",
}
REQUIRED_CATEGORIES = {
    "tool", "weapon", "fishing_gear", "seed", "food", "fish_processing",
    "clothing", "hair", "accessory", "furniture", "ship_cabin", "collectible",
}
FORBIDDEN_DECISION_FIELDS = {
    "price", "damage", "reward", "durability", "max_durability", "strength",
    "strength_cap", "weight", "volume", "effect_value", "drop_rate",
}
ID_RE = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)+$")


def load_json(path: Path) -> Dict[str, Any]:
    with path.open("r", encoding="utf-8") as handle:
        value = json.load(handle)
    if not isinstance(value, dict):
        raise ValueError(f"{path}: JSON root must be an object")
    return value


def files_for_batch(root: Path) -> List[Tuple[Path, Dict[str, Any]]]:
    result: List[Tuple[Path, Dict[str, Any]]] = []
    for path in sorted((root / "entries").rglob("*.json")):
        data = load_json(path)
        if data.get("production_batch") == BATCH:
            result.append((path, data))
    return result


def require_text(data: Dict[str, Any], key: str, path: Path, errors: List[str]) -> None:
    if not isinstance(data.get(key), str) or not data[key].strip():
        errors.append(f"{path}: {key} must be non-empty text")


def check_path_reference(value: str, root: Path, path: Path, errors: List[str]) -> None:
    if value.startswith("none:"):
        return
    file_part = value.split(":", 1)[0]
    if not (root.parent.parent.parent.parent / file_part).exists():
        errors.append(f"{path}: asset reference does not exist: {file_part}")


def check_batch(root: Path) -> Tuple[List[str], Dict[str, int], int]:
    errors: List[str] = []
    rows = files_for_batch(root)
    ids: Set[str] = set()
    tests: Set[str] = set()
    categories: Dict[str, int] = {}
    functional = 0
    decorative = 0
    known_assets: Dict[str, Dict[str, Any]] = {}
    for asset_path in sorted((root / "entries" / "asset").glob("*.json")):
        asset = load_json(asset_path)
        if isinstance(asset.get("id"), str):
            known_assets[asset["id"]] = asset

    for path, data in rows:
        missing = REQUIRED - set(data)
        if missing:
            errors.append(f"{path}: missing fields {sorted(missing)}")
        entry_id = data.get("id", path.stem)
        if not isinstance(entry_id, str) or not ID_RE.match(entry_id):
            errors.append(f"{path}: invalid stable id {entry_id!r}")
        if entry_id in ids:
            errors.append(f"{path}: duplicate id {entry_id}")
        ids.add(entry_id)
        test_id = data.get("test_id")
        if not isinstance(test_id, str) or test_id in tests:
            errors.append(f"{path}: missing or duplicate test_id {test_id!r}")
        tests.add(test_id)
        if data.get("schema_version") != 1 or data.get("phase") != "D15":
            errors.append(f"{path}: schema_version/phase must be 1/D15")
        for key in ("id", "name", "display_name_key", "description", "gameplay_role",
                    "source_note", "owner_system", "test_id", "model_ref", "material_ref",
                    "collision_ref", "lod_profile", "interaction_entry", "behavior_profile"):
            require_text(data, key, path, errors)
        if not isinstance(data.get("tags"), list) or not data["tags"]:
            errors.append(f"{path}: tags must be a non-empty list")
        category = data.get("content_category")
        if category not in REQUIRED_CATEGORIES:
            errors.append(f"{path}: unsupported content_category {category!r}")
        else:
            categories[category] = categories.get(category, 0) + 1
        role = data.get("batch_role")
        if role == "functional":
            functional += 1
            require_text(data, "function_entry", path, errors)
        elif role == "decorative":
            decorative += 1
        else:
            errors.append(f"{path}: batch_role must be functional/decorative")
        if data.get("asset_state") != "placeholder" or data.get("quality_level") != "Q2":
            errors.append(f"{path}: D15-A05 batch must explicitly declare shared Q2 placeholder state")
        if data.get("production_stage") != "integrated":
            errors.append(f"{path}: production_stage must be integrated")
        license_data = data.get("license")
        if not isinstance(license_data, dict) or license_data.get("commercial_use") is not True:
            errors.append(f"{path}: commercial authorization is required")
        source = data.get("source")
        if not isinstance(source, dict) or not all(isinstance(source.get(k), str) and source[k].strip() for k in ("kind", "author", "reference")):
            errors.append(f"{path}: source.kind/author/reference are required")
        refs = data.get("asset_refs")
        if not isinstance(refs, list) or not refs:
            errors.append(f"{path}: asset_refs must contain an authorized shared asset")
        else:
            for asset_id in refs:
                asset = known_assets.get(asset_id)
                if not asset or not asset_id.startswith("asset_"):
                    errors.append(f"{path}: missing asset reference {asset_id}")
                elif asset.get("license", {}).get("commercial_use") is not True:
                    errors.append(f"{path}: asset {asset_id} lacks commercial authorization")
        check_path_reference(data.get("model_ref", ""), root, path, errors)
        check_path_reference(data.get("material_ref", ""), root, path, errors)
        check_path_reference(data.get("collision_ref", ""), root, path, errors)
        test_cases = data.get("test_cases")
        if not isinstance(test_cases, list) or len(test_cases) < 3 or any(not isinstance(v, str) or not v for v in test_cases):
            errors.append(f"{path}: load/interaction/owner test_cases are required")
        integration = data.get("integration")
        if not isinstance(integration, dict) or integration.get("canonical_write_owner") != data.get("owner_system"):
            errors.append(f"{path}: integration canonical_write_owner must match owner_system")
        if data.get("historical_review") not in {"Approved", "ApprovedWithWarning"}:
            errors.append(f"{path}: historical_review must be approved")
        if data.get("historical_review") == "ApprovedWithWarning" and not data.get("content_warning_ids"):
            errors.append(f"{path}: warning review requires content_warning_ids")
        for forbidden in FORBIDDEN_DECISION_FIELDS:
            if forbidden in data:
                errors.append(f"{path}: forbidden runtime decision field {forbidden}")

    if functional != 200:
        errors.append(f"functional count={functional}, expected 200")
    if decorative != 300:
        errors.append(f"decorative count={decorative}, expected 300")
    missing_categories = REQUIRED_CATEGORIES - set(categories)
    if missing_categories:
        errors.append(f"missing coverage categories: {sorted(missing_categories)}")
    sample_rows = rows[::10]
    for path, data in sample_rows:
        if not data.get("interaction_entry") or not data.get("asset_refs"):
            errors.append(f"{path}: sampled entry is not loadable/interactable")
    return errors, categories, len(sample_rows)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("registry_root", type=Path)
    args = parser.parse_args()
    errors, categories, sample_count = check_batch(args.registry_root)
    if errors:
        for error in errors:
            print("ERROR " + error)
        print(f"D15-A05 content check FAILED: {len(errors)} error(s)")
        return 1
    print("D15-A05 content check OK: functional=200 decorative=300 total=500")
    print("categories=" + json.dumps(categories, ensure_ascii=False, sort_keys=True))
    print(f"sample_load_interaction_checks={sample_count}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
