"""Validate the D8 content registry without loading Unity or project code."""

from __future__ import annotations

import argparse
import json
import re
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Dict, Iterable, List, Optional, Set, Tuple


EXPECTED_PREFIXES: Tuple[str, ...] = (
    "ship_",
    "crew_",
    "island_",
    "building_",
    "item_",
    "animal_",
    "plant_",
    "fish_",
    "quest_",
    "reward_",
    "outfit_",
    "pet_",
    "asset_",
)

CONTENT_STATUSES = {
    "designed",
    "cataloged",
    "prototyped",
    "production",
    "verified",
    "deprecated",
}
HISTORICAL_TAGS = {"historical", "inferred", "fictional"}
PHASES = {"D7", "D8", "D9", "D10", "D11", "D12", "D13", "D14", "D15", "D16"}
ID_PATTERN = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)+$")
TEST_ID_PATTERN = re.compile(r"^D8-A02-[A-Z0-9-]+$")

REQUIRED_ENTRY_FIELDS = (
    "id",
    "schema_version",
    "name",
    "display_name_key",
    "category",
    "tags",
    "description",
    "gameplay_role",
    "valid_regions",
    "valid_habitats",
    "valid_weather",
    "dependencies",
    "references",
    "asset_refs",
    "source_refs",
    "source_note",
    "source",
    "license",
    "historical_tag",
    "phase",
    "status",
    "owner_system",
    "test_id",
    "reviewer",
)
REFERENCE_FIELDS = ("references", "dependencies", "asset_refs")


@dataclass(frozen=True)
class RegistryError:
    code: str
    entry_id: str
    path: Path
    message: str

    def format(self) -> str:
        return (
            f"ERROR {self.code} entry={self.entry_id} "
            f"file={self.path.as_posix()} message={self.message}"
        )


def _error(errors: List[RegistryError], code: str, entry_id: str, path: Path, message: str) -> None:
    errors.append(RegistryError(code, entry_id, path, message))


def _text(value: Any) -> bool:
    return isinstance(value, str) and bool(value.strip())


def _prefix_for(content_id: str) -> Optional[str]:
    for prefix in EXPECTED_PREFIXES:
        if content_id.startswith(prefix):
            return prefix
    return None


def _load_json(path: Path, errors: List[RegistryError], entry_id: str) -> Optional[Dict[str, Any]]:
    try:
        with path.open("r", encoding="utf-8") as handle:
            value = json.load(handle)
    except (OSError, json.JSONDecodeError) as exc:
        _error(errors, "invalid-json", entry_id, path, str(exc))
        return None
    if not isinstance(value, dict):
        _error(errors, "invalid-entry", entry_id, path, "JSON root must be an object")
        return None
    return value


def _validate_manifest(root: Path, manifest: Dict[str, Any], errors: List[RegistryError]) -> Dict[str, Dict[str, Any]]:
    path = root / "registry.json"
    if manifest.get("schema_version") != 1:
        _error(errors, "invalid-registry-version", "<registry>", path, "schema_version must be 1")
    if not _text(manifest.get("registry_id")):
        _error(errors, "missing-registry-id", "<registry>", path, "registry_id is required")

    prefixes = manifest.get("prefixes")
    if not isinstance(prefixes, list):
        _error(errors, "invalid-prefix-registry", "<registry>", path, "prefixes must be a list")
        prefixes = []
    actual_prefixes = {value for value in prefixes if isinstance(value, str)}
    for expected in EXPECTED_PREFIXES:
        if expected not in actual_prefixes:
            _error(errors, "missing-prefix", "<registry>", path, f"required prefix {expected} is not registered")
    for actual in sorted(actual_prefixes - set(EXPECTED_PREFIXES)):
        _error(errors, "invalid-prefix", "<registry>", path, f"unregistered prefix {actual}")

    definitions = manifest.get("prefix_definitions")
    if not isinstance(definitions, list):
        _error(errors, "invalid-prefix-registry", "<registry>", path, "prefix_definitions must be a list")
        definitions = []
    definitions_by_prefix: Dict[str, Dict[str, Any]] = {}
    for definition in definitions:
        if not isinstance(definition, dict) or not _text(definition.get("prefix")):
            _error(errors, "invalid-prefix-definition", "<registry>", path, "each prefix definition needs a prefix")
            continue
        prefix = definition["prefix"]
        if prefix in definitions_by_prefix:
            _error(errors, "duplicate-prefix", "<registry>", path, f"prefix {prefix} is defined more than once")
        definitions_by_prefix[prefix] = definition
    for prefix in EXPECTED_PREFIXES:
        if prefix not in definitions_by_prefix:
            _error(errors, "missing-prefix-definition", "<registry>", path, f"missing definition for {prefix}")
    return definitions_by_prefix


def _validate_entry_shape(
    root: Path,
    path: Path,
    data: Dict[str, Any],
    entry_id: str,
    definitions: Dict[str, Dict[str, Any]],
    errors: List[RegistryError],
) -> None:
    for field in REQUIRED_ENTRY_FIELDS:
        if field not in data:
            _error(errors, "missing-field", entry_id, path, f"required field {field} is missing")

    if data.get("schema_version") != 1:
        _error(errors, "invalid-entry-version", entry_id, path, "schema_version must be 1")
    if not _text(data.get("name")):
        _error(errors, "missing-name", entry_id, path, "name is required")
    if not _text(data.get("display_name_key")):
        _error(errors, "missing-localization-key", entry_id, path, "display_name_key is required")
    elif not data["display_name_key"].startswith("content."):
        _error(errors, "invalid-localization-key", entry_id, path, "display_name_key must start with content.")

    prefix = _prefix_for(entry_id)
    if prefix is None:
        _error(errors, "invalid-prefix", entry_id, path, "ID does not start with a registered prefix")
    elif not ID_PATTERN.fullmatch(entry_id) or not re.fullmatch(re.escape(prefix) + r"[a-z0-9]+(?:_[a-z0-9]+)*", entry_id):
        _error(errors, "invalid-id", entry_id, path, f"ID must match {prefix}<lower_snake_case>")
    else:
        definition = definitions.get(prefix)
        if definition:
            expected_directory = root / str(definition.get("directory", ""))
            if path.parent.resolve() != expected_directory.resolve():
                _error(
                    errors,
                    "wrong-directory",
                    entry_id,
                    path,
                    f"ID prefix {prefix} belongs under {expected_directory.as_posix()}",
                )
            expected_category = definition.get("entity_type")
            if data.get("category") != expected_category:
                _error(errors, "category-prefix-mismatch", entry_id, path, f"category must be {expected_category}")
            expected_owner = definition.get("owner_system")
            if data.get("owner_system") != expected_owner:
                _error(errors, "owner-prefix-mismatch", entry_id, path, f"owner_system must be {expected_owner}")

    if path.stem != entry_id:
        _error(errors, "filename-mismatch", entry_id, path, "file name must equal the content ID")
    if data.get("historical_tag") not in HISTORICAL_TAGS:
        _error(errors, "invalid-historical-tag", entry_id, path, "historical_tag must be historical, inferred, or fictional")
    if data.get("phase") not in PHASES:
        _error(errors, "invalid-phase", entry_id, path, "phase must be D7 through D16")
    if data.get("status") not in CONTENT_STATUSES:
        _error(errors, "invalid-status", entry_id, path, "status is not a registered content status")
    if not _text(data.get("owner_system")):
        _error(errors, "missing-owner", entry_id, path, "owner_system is required")
    if not _text(data.get("test_id")):
        _error(errors, "missing-test-id", entry_id, path, "test_id is required")
    elif not TEST_ID_PATTERN.fullmatch(data["test_id"]):
        _error(errors, "invalid-test-id", entry_id, path, "test_id must identify a D8-A02 test")

    for field in ("tags", "valid_regions", "valid_habitats", "valid_weather", *REFERENCE_FIELDS, "source_refs"):
        value = data.get(field)
        if not isinstance(value, list) or any(not _text(item) for item in value):
            _error(errors, "invalid-list-field", entry_id, path, f"{field} must be a list of non-empty strings")

    source = data.get("source")
    if not isinstance(source, dict) or not _text(source.get("kind")) or not _text(source.get("reference")):
        _error(errors, "missing-source", entry_id, path, "source.kind and source.reference are required")
    if not _text(data.get("source_note")) or not isinstance(data.get("source_refs"), list) or not data.get("source_refs"):
        _error(errors, "missing-source", entry_id, path, "source_note and at least one source_refs entry are required")

    license_data = data.get("license")
    if not isinstance(license_data, dict):
        _error(errors, "missing-license", entry_id, path, "license metadata is required")
    else:
        for field in ("status", "holder", "reference"):
            if not _text(license_data.get(field)):
                _error(errors, "missing-license", entry_id, path, f"license.{field} is required")
        for field in ("commercial_use", "attribution_required", "modification_allowed"):
            if not isinstance(license_data.get(field), bool):
                _error(errors, "invalid-license", entry_id, path, f"license.{field} must be boolean")
        if license_data.get("commercial_use") is not True:
            _error(errors, "invalid-license", entry_id, path, "license must permit commercial use for formal content")


def _validate_references(
    entries: Iterable[Tuple[Path, Dict[str, Any], str]],
    ids: Set[str],
    errors: List[RegistryError],
) -> Dict[str, Set[str]]:
    graph: Dict[str, Set[str]] = {}
    for path, data, entry_id in entries:
        targets: Set[str] = set()
        for field in REFERENCE_FIELDS:
            values = data.get(field, [])
            if not isinstance(values, list):
                continue
            for target in values:
                if not isinstance(target, str):
                    continue
                prefix = _prefix_for(target)
                if prefix is None:
                    _error(errors, "invalid-prefix", entry_id, path, f"reference {target} has an illegal prefix")
                elif target not in ids:
                    _error(errors, "missing-reference", entry_id, path, f"reference target {target} does not exist")
                else:
                    targets.add(target)
                if field == "asset_refs" and prefix != "asset_":
                    _error(errors, "invalid-asset-reference", entry_id, path, f"asset_refs target {target} must use asset_ prefix")
        graph[entry_id] = targets
    return graph


def validate_registry(registry_root: Path) -> List[RegistryError]:
    root = Path(registry_root)
    errors: List[RegistryError] = []
    manifest_path = root / "registry.json"
    if not manifest_path.is_file():
        _error(errors, "missing-manifest", "<registry>", manifest_path, "registry.json is required")
        return errors
    manifest = _load_json(manifest_path, errors, "<registry>")
    if manifest is None:
        return errors

    definitions = _validate_manifest(root, manifest, errors)
    entries_dir = root / "entries"
    if not entries_dir.is_dir():
        _error(errors, "missing-entries-directory", "<registry>", entries_dir, "entries directory is required")
        return errors

    entries: List[Tuple[Path, Dict[str, Any], str]] = []
    ids: Set[str] = set()
    paths_by_id: Dict[str, Path] = {}
    test_ids: Dict[str, Path] = {}
    for path in sorted(entries_dir.rglob("*.json")):
        entry_id = path.stem
        data = _load_json(path, errors, entry_id)
        if data is None:
            continue
        actual_id = data.get("id")
        if isinstance(actual_id, str) and actual_id.strip():
            entry_id = actual_id
        else:
            _error(errors, "missing-id", entry_id, path, "id is required")
        entries.append((path, data, entry_id))
        if entry_id in ids:
            _error(errors, "duplicate-id", entry_id, path, f"ID already registered in {paths_by_id[entry_id].as_posix()}")
        else:
            ids.add(entry_id)
            paths_by_id[entry_id] = path
        test_id = data.get("test_id")
        if isinstance(test_id, str) and test_id:
            if test_id in test_ids:
                _error(errors, "duplicate-test-id", entry_id, path, f"test_id already used in {test_ids[test_id].as_posix()}")
            else:
                test_ids[test_id] = path
        _validate_entry_shape(root, path, data, entry_id, definitions, errors)

    graph = _validate_references(entries, ids, errors)
    roots = manifest.get("roots")
    if not isinstance(roots, list) or any(not _text(root_id) for root_id in roots):
        _error(errors, "invalid-roots", "<registry>", manifest_path, "roots must be a non-empty list of IDs")
        roots = []
    reachable: Set[str] = set()
    pending = [root_id for root_id in roots if root_id in ids]
    for root_id in roots:
        if root_id not in ids:
            _error(errors, "missing-reference", "<registry>", manifest_path, f"root target {root_id} does not exist")
    while pending:
        current = pending.pop()
        if current in reachable:
            continue
        reachable.add(current)
        pending.extend(target for target in graph.get(current, set()) if target not in reachable)
    for entry_id in sorted(ids - reachable):
        _error(
            errors,
            "orphan-reference",
            entry_id,
            paths_by_id[entry_id],
            "content entry is not reachable from any registry root",
        )
    return errors


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="Validate the Whalebone D8 content ID registry")
    parser.add_argument("registry_root", type=Path, help="path to the ContentRegistry directory")
    args = parser.parse_args(argv)
    errors = validate_registry(args.registry_root)
    if errors:
        for item in errors:
            print(item.format())
        print(f"Content registry FAILED: {len(errors)} error(s)")
        return 1
    entry_count = len(list((args.registry_root / "entries").rglob("*.json")))
    print(f"Content registry OK: {entry_count} entries, {len(EXPECTED_PREFIXES)} prefixes")
    return 0


if __name__ == "__main__":
    sys.exit(main())
