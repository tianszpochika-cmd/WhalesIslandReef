#!/usr/bin/env python3
"""Read-only material/texture descriptor validator for D18 logic assets (D18-L27).

Validates ``d18_material_descriptor`` JSON records (static data for the
material/texture/hdri/font categories frozen by D18-L14).  The field
vocabulary aligns with the merged D18-L25 import-descriptor schema
(``source_asset_id``, ``license_dependency``, ``fallback``,
``represents_imported_asset``) and the D18-L02 record license/rights blocks.

Checks (each rejection carries a stable code, field and message):

* R1 asset_id        ``source_asset_id`` must be ``asset_<lower_snake_case>``;
                     when a records catalog is supplied the id must exist;
                     disk paths / display names are never accepted.
* R2 category        must be one of the L14 material-domain keys
                     (material, texture, hdri, font); unknown keys fail.
* R3 category<->map  PBR roles (albedo/normal/roughness/ao/metalness/height/
                     opacity/emissive) only for ``material``; ``texture``
                     must not carry PBR-role maps; ``hdri`` exactly one
                     ``hdri`` map; ``font`` no maps.
* R4 map roles       role must be in the frozen role vocabulary; a role may
                     appear at most once per descriptor.
* R5 channels        per-role channel semantics (e.g. albedo->rgb, normal->
                     tangent xyz/xy, roughness/ao/metalness/height->r,
                     opacity->a); a role/channel mismatch fails.
* R6 color space     every map must declare ``color_space_intent`` in
                     {srgb, linear}; this is intent only, never an import
                     setting claim.
* R7 PBR set         when ``pbr_set_complete`` is true the descriptor must
                     include albedo + normal + roughness + ao; an AO-less
                     claim of completeness fails (AO missing cannot fake a
                     full four-map set).
* R8 license         ``license_dependency`` must normalize to a known license
                     via the L04 vocabulary, never NC/ND/SA; the evidence
                     file must exist inside the project root; ``rights``
                     (when present) must have all four booleans explicit.
                     "free"/"downloadable" never grant distribution.
* R9 fallback        a non-empty fallback (reference + description) is
                     required; reference must be ``asset_*``/``placeholder_*``
                     and must not point at a candidate binary.
* R10 runtime         ``runtime_deferred`` must be a non-empty string array
    deferred          (honest per-record risks; deferral never replaces a pass).
* R11 not imported   ``represents_imported_asset`` must be false; the tool
                     never claims an import.

The tool is strictly read-only and offline: it never creates a Material,
Shader, TextureImporter, color-space/compression setting or any Unity asset,
never inspects image pixels (actual image checking is deferred to Runtime),
never approves assets and never writes the registry.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Iterable

try:
    from d18_license_normalizer import canonicalize_license_id
except ImportError:  # package-qualified invocation (python -m unittest tools.tests.*)
    from tools.d18_license_normalizer import canonicalize_license_id

TASK = "D18-L27"
TOOL = "tools/d18_material_descriptor_validator.py"

ASSET_ID_RE = re.compile(r"^asset_[a-z0-9_]+$")
FALLBACK_REF_RE = re.compile(r"^(asset_[a-z0-9_]+|placeholder_[a-z0-9_]+)$")
CANDIDATE_BINARY_MARKERS = ("assets/art/candidates", "art/candidates", "candidates/")
BINARY_EXTENSIONS = {".fbx", ".obj", ".png", ".jpg", ".jpeg", ".tga", ".psd", ".exr", ".hdr", ".mat", ".shader", ".prefab"}

# L14 material-domain classification keys (D18_TAXONOMY_UI_AUDIO.md section 2.1)
MATERIAL_CATEGORIES = ("material", "texture", "hdri", "font")

# L25 slot-role / L14 material-map role vocabulary
MAP_ROLES = ("albedo", "normal", "roughness", "ao", "metalness", "emissive", "height", "opacity", "hdri", "ui", "misc")
PBR_ROLES = ("albedo", "normal", "roughness", "ao", "metalness", "emissive", "height", "opacity")
CHANNEL_SEMANTICS = {
    "albedo": {"rgb", "rgba"},
    "normal": {"xyz", "xy"},
    "roughness": {"r"},
    "ao": {"r"},
    "metalness": {"r"},
    "emissive": {"rgb", "rgba"},
    "height": {"r"},
    "opacity": {"a"},
    "hdri": {"rgb", "rgba"},
    "ui": {"rgb", "rgba"},
    "misc": {"rgb", "rgba", "r", "a"},
}
COLOR_SPACE_INTENTS = ("srgb", "linear")
RIGHT_FIELDS = ("commercial_use", "modification_allowed", "repository_storage", "distribution_with_game")
NC_ND_SA_MARKERS = ("-NC", "-ND", "-SA")


class Issue(dict):
    def __init__(self, code: str, severity: str, field: str, message: str, *, record: str = ""):
        item: dict[str, Any] = {"code": code, "severity": severity, "field": field, "message": message}
        if record:
            item["record"] = record
        super().__init__(sorted(item.items()))


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _inside(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def _usable_license(license_id: Any) -> tuple[bool, str]:
    canonical, lookup = canonicalize_license_id(license_id)
    if lookup != "ok" or canonical is None:
        return False, f"license not usable: lookup={lookup!r}"
    if any(marker in canonical.upper() for marker in NC_ND_SA_MARKERS):
        return False, f"license {canonical!r} carries NC/ND/SA restriction; cannot be confirmed for game distribution"
    return True, f"license {canonical!r} usable"


def _is_candidate_binary_reference(value: str) -> bool:
    lowered = value.casefold().replace("\\", "/")
    if any(marker in lowered for marker in CANDIDATE_BINARY_MARKERS):
        return True
    if Path(lowered).suffix.lower() in BINARY_EXTENSIONS:
        return True
    return False


def validate_descriptor(descriptor: Any, project_root: Path, records: list[Any] | None = None) -> dict[str, Any]:
    """Validate one material/texture descriptor; deterministic JSON verdict."""
    record_id = ""
    if not isinstance(descriptor, dict):
        return {
            "record": "",
            "status": "FAIL",
            "issues": [dict(Issue("descriptor_not_object", "error", "descriptor", "descriptor root must be an object"))],
        }
    record_id = _text(descriptor.get("source_asset_id"))
    issues: list[Issue] = []
    known_ids = {_text(r.get("canonical_asset_id")) for r in (records or []) if isinstance(r, dict)} if records is not None else None

    # R1 asset_id
    asset_id = _text(descriptor.get("source_asset_id"))
    if not asset_id:
        issues.append(Issue("missing_asset_id", "error", "source_asset_id", "source_asset_id is required (asset_<lower_snake_case>)", record=record_id))
    elif not ASSET_ID_RE.fullmatch(asset_id):
        issues.append(Issue("invalid_asset_id", "error", "source_asset_id", f"source_asset_id {asset_id!r} must be asset_<lower_snake_case>; never a disk path or display name", record=record_id))
    elif known_ids is not None and asset_id not in known_ids:
        issues.append(Issue("asset_id_not_found", "error", "source_asset_id", f"source_asset_id {asset_id!r} is not present in the supplied records catalog", record=record_id))

    # R2 category
    category = _text(descriptor.get("category"))
    if not category:
        issues.append(Issue("missing_category", "error", "category", "category is required (material/texture/hdri/font)", record=record_id))
    elif category not in MATERIAL_CATEGORIES:
        issues.append(Issue("unknown_category", "error", "category", f"category {category!r} is not in the L14 material-domain dictionary {MATERIAL_CATEGORIES}", record=record_id))

    # R3 + R4 + R5 + R6 maps
    maps = descriptor.get("maps")
    if not isinstance(maps, list):
        if maps is not None:
            issues.append(Issue("maps_not_array", "error", "maps", "maps must be an array", record=record_id))
        maps = []
    seen_roles: set[str] = set()
    for index, entry in enumerate(maps):
        field = f"maps[{index}]"
        if not isinstance(entry, dict):
            issues.append(Issue("map_not_object", "error", field, "each map must be an object", record=record_id))
            continue
        role = _text(entry.get("map_role"))
        channels = _text(entry.get("channels")).casefold()
        color_space = _text(entry.get("color_space_intent")).casefold()
        if not role:
            issues.append(Issue("missing_map_role", "error", f"{field}.map_role", "map_role is required", record=record_id))
        elif role not in MAP_ROLES:
            issues.append(Issue("unknown_map_role", "error", f"{field}.map_role", f"map_role {role!r} is not in the frozen vocabulary {MAP_ROLES}", record=record_id))
        elif role in seen_roles:
            issues.append(Issue("duplicate_map_role", "error", f"{field}.map_role", f"map_role {role!r} appears more than once", record=record_id))
        seen_roles.add(role)
        if role in CHANNEL_SEMANTICS and channels and channels not in CHANNEL_SEMANTICS[role]:
            issues.append(Issue("invalid_channels", "error", f"{field}.channels", f"channels {channels!r} not valid for role {role!r}; expected {sorted(CHANNEL_SEMANTICS[role])}", record=record_id))
        elif not channels and role:
            issues.append(Issue("missing_channels", "error", f"{field}.channels", "channels is required (per-role channel semantics)", record=record_id))
        if not color_space:
            issues.append(Issue("missing_color_space", "error", f"{field}.color_space_intent", "color_space_intent is required (srgb/linear; intent only)", record=record_id))
        elif color_space not in COLOR_SPACE_INTENTS:
            issues.append(Issue("invalid_color_space", "error", f"{field}.color_space_intent", f"color_space_intent {color_space!r} must be srgb or linear (intent, never a settings claim)", record=record_id))
        source_path = _text(entry.get("source_path"))
        if source_path:
            lowered = source_path.casefold().replace("\\", "/")
            if lowered.startswith(("/", "c:")) or "://" in lowered or ".." in lowered.split("/"):
                issues.append(Issue("source_path_not_repo_relative", "error", f"{field}.source_path", f"source_path {source_path!r} must be a repository-relative path", record=record_id))

    # R3 category<->map conflict
    roles = {m.get("map_role") for m in maps if isinstance(m, dict) and m.get("map_role")}
    pbr_roles_present = roles & set(PBR_ROLES)
    if category == "material" and pbr_roles_present and not {"albedo", "normal", "roughness", "ao"} & pbr_roles_present:
        issues.append(Issue("category_map_conflict", "error", "maps", "a material descriptor must declare PBR role maps (albedo/normal/roughness/ao/...)", record=record_id))
    if category == "texture" and pbr_roles_present:
        issues.append(Issue("category_map_conflict", "error", "maps", f"a texture descriptor must not carry PBR role maps: {sorted(pbr_roles_present)}", record=record_id))
    if category == "hdri" and (len(maps) != 1 or roles != {"hdri"}):
        issues.append(Issue("category_map_conflict", "error", "maps", "an hdri descriptor must declare exactly one hdri map and nothing else", record=record_id))
    if category == "font" and maps:
        issues.append(Issue("category_map_conflict", "error", "maps", "a font descriptor must not declare texture maps", record=record_id))

    # R7 PBR set completeness (AO missing cannot fake a full set)
    if descriptor.get("pbr_set_complete") is True:
        missing_pbr = [r for r in ("albedo", "normal", "roughness", "ao") if r not in roles]
        if missing_pbr:
            issues.append(Issue("pbr_incomplete", "error", "pbr_set_complete", f"pbr_set_complete claims a full set but is missing {missing_pbr}; AO missing cannot fake completeness", record=record_id))

    # R8 license dependency
    license_dep = descriptor.get("license_dependency")
    if not isinstance(license_dep, dict):
        issues.append(Issue("missing_license_dependency", "error", "license_dependency", "license_dependency is required", record=record_id))
    else:
        license_id = _text(license_dep.get("license_id"))
        usable, reason = _usable_license(license_id)
        if not usable:
            issues.append(Issue("invalid_license", "error", "license_dependency.license_id", reason, record=record_id))
        evidence_path = _text(license_dep.get("license_evidence_path"))
        if not evidence_path:
            issues.append(Issue("missing_license_evidence", "error", "license_dependency.license_evidence_path", "license_evidence_path is required", record=record_id))
        else:
            resolved = (project_root / evidence_path.replace("\\", "/")).resolve()
            if not _inside(resolved, project_root):
                issues.append(Issue("license_evidence_escape", "error", "license_dependency.license_evidence_path", "license evidence path escapes the project root", record=record_id))
            elif not resolved.is_file():
                issues.append(Issue("license_evidence_missing_file", "error", "license_dependency.license_evidence_path", f"license evidence file does not exist: {evidence_path}", record=record_id))
        attribution = license_dep.get("attribution_required")
        if not isinstance(attribution, bool):
            issues.append(Issue("invalid_attribution", "error", "license_dependency.attribution_required", "attribution_required must be an explicit boolean", record=record_id))

    # R8 rights (when present, all four explicit)
    rights = descriptor.get("rights")
    if rights is not None:
        if not isinstance(rights, dict):
            issues.append(Issue("rights_not_object", "error", "rights", "rights must be an object", record=record_id))
        else:
            missing_rights = [f for f in RIGHT_FIELDS if rights.get(f) is not True]
            if missing_rights:
                issues.append(Issue("rights_not_explicit", "error", "rights", f"four distribution rights must be explicit; missing/false: {missing_rights}", record=record_id))

    # R9 fallback
    fallback = descriptor.get("fallback")
    if not isinstance(fallback, dict):
        issues.append(Issue("missing_fallback", "error", "fallback", "a non-empty fallback is required for every material descriptor", record=record_id))
    else:
        reference = _text(fallback.get("reference"))
        description = _text(fallback.get("description"))
        if not reference:
            issues.append(Issue("empty_fallback", "error", "fallback.reference", "fallback.reference must be a placeholder/asset reference", record=record_id))
        elif not FALLBACK_REF_RE.fullmatch(reference):
            issues.append(Issue("invalid_fallback_reference", "error", "fallback.reference", f"fallback.reference {reference!r} must be asset_*/placeholder_*", record=record_id))
        if not description:
            issues.append(Issue("empty_fallback", "error", "fallback.description", "fallback.description must explain the degradation", record=record_id))
        if reference and _is_candidate_binary_reference(reference):
            issues.append(Issue("fallback_candidate_binary", "error", "fallback.reference", f"fallback must not reference a candidate binary: {reference!r}", record=record_id))

    # R10 runtime_deferred
    deferred = descriptor.get("runtime_deferred")
    if not isinstance(deferred, list) or not deferred:
        issues.append(Issue("missing_runtime_deferred", "error", "runtime_deferred", "runtime_deferred must be a non-empty array of honest runtime risks", record=record_id))
    elif any(not isinstance(item, str) or not _text(item) for item in deferred):
        issues.append(Issue("invalid_runtime_deferred", "error", "runtime_deferred", "each runtime_deferred entry must be a non-empty string", record=record_id))

    # R11 not imported
    if descriptor.get("represents_imported_asset") is not False:
        issues.append(Issue("claims_imported", "error", "represents_imported_asset", "represents_imported_asset must be false; a descriptor never claims an import", record=record_id))

    errors = [issue for issue in issues if issue["severity"] == "error"]
    return {
        "record": record_id,
        "status": "PASS" if not errors else "FAIL",
        "issue_count": len(issues),
        "error_count": len(errors),
        "issues": [dict(issue) for issue in issues],
    }


def validate_descriptors(descriptors: Iterable[Any], project_root: Path, records: list[Any] | None = None) -> dict[str, Any]:
    from collections import Counter
    items = [validate_descriptor(d, project_root, records) for d in descriptors]
    code_counts = Counter(issue["code"] for item in items for issue in item["issues"])
    errors = sum(item["error_count"] for item in items)
    return {
        "task": TASK,
        "tool": TOOL,
        "read_only": True,
        "approved_count": 0,
        "status": "PASS" if errors == 0 else "FAIL",
        "descriptor_count": len(items),
        "error_count": errors,
        "issue_count": sum(item["issue_count"] for item in items),
        "issue_counts": dict(sorted(code_counts.items())),
        "descriptors": items,
    }


def _load_input(path: Path) -> tuple[Any, list[Any] | None]:
    data = json.loads(path.read_text(encoding="utf-8"))
    if isinstance(data, dict) and "descriptors" in data:
        return data["descriptors"], data.get("records")
    if isinstance(data, dict) and "descriptor" in data:
        return [data["descriptor"]], data.get("records")
    if isinstance(data, list):
        return data, None
    return [data], None


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, required=True, help="JSON with a descriptor, a list of descriptors, or {'descriptors': [...], 'records': [...]}")
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    try:
        descriptors, records = _load_input(args.input)
    except (OSError, json.JSONDecodeError) as exc:
        print(f"{TOOL}: cannot read input {args.input}: {exc}", file=sys.stderr)
        return 2
    result = validate_descriptors(descriptors, args.project_root.resolve(), records)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
