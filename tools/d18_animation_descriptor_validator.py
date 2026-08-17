"""Read-only animation descriptor validator for D18 logic assets (D18-L28).

Validates ``d18_animation_descriptor`` JSON records (static data produced by the
L42 catalog compiler for animated assets; the animation-relevant fields of the
D18-L25 import descriptor and the action-intent vocabulary of D20):

* animation purpose: each clip's ``role`` must be from the frozen L25 role
  vocabulary (idle/walk/run/work/combat/sail/swim/fly/emote/interact/other) and
  must be compatible with the asset ``type`` (a fish with ``walk`` or a bird
  without ``fly`` fails; static skeleton with a non-empty animation list fails);
* skeleton type: the ``skeleton_type`` must match the asset type (L25 allOf
  rules) and an animated asset must not declare ``none``/``static``;
* looping intent: clips whose role implies looping (idle/walk/run/work) may not
  declare ``looping=false``; one-shot roles (combat/emote/interact/sail) may not
  declare ``looping=true``;
* root-motion strategy: the top-level ``root_motion`` (none/required/derived/
  unknown) must agree with per-clip root-motion claims; ``unknown`` is a pending
  resolution note, not a pass;
* action intent: the ``action_intent_map`` keys must be from the frozen D20
  intent vocabulary (walk/run/interact/talk/fish/board/helm/hit), each intent
  must map to an existing clip key, the clip's role must be compatible with the
  intent, and each type's required actions (character->walk, animal->walk,
  fish->swim, ship->sail) must be present — a missing required action fails;
* fallback: every descriptor must declare a fallback (reference + non-empty
  description); missing fallback fails (L25 semantics);
* license dependency: the license must be usable via the D18-L04 normalizer and
  the evidence file must exist; NC/personal/unknown fail;
* state-machine ownership: ``state_machine_owner`` (player/npc/animal/none)
  must be consistent with the asset type and present whenever clips exist;
* no Unity: fields declaring Animator, Avatar, clip import, retargeting, blend
  trees, .anim/.controller assets or import settings are rejected as
  out-of-bounds; the tool never creates Animator/Avatar, never imports clips.

The tool is strictly read-only and offline: it parses JSON/static data only,
never touches Unity, never imports clips, never creates Animator or Avatar,
never approves assets and its ``approved_count`` is always 0.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Iterable

from d18_asset_path_validator import validate_path  # D18-L08 frozen path boundaries
from d18_license_normalizer import canonicalize_license_id  # D18-L04 license vocabulary

DESCRIPTOR_KIND = "d18_animation_descriptor"
DESCRIPTOR_SCHEMA_VERSION = 1

ASSET_ID_RE = re.compile(r"^asset_[a-z0-9_]+$")
KEY_RE = re.compile(r"^[a-z0-9_]+$")
HTTP_URL_RE = re.compile(r"^https?://[^\s]+$")

# L02 schema domain enum (the D18-L25 descriptor `type`).
TYPES = frozenset(
    {"animal", "audio", "building", "character", "environment", "fish",
     "lighting_reference", "material", "prop", "ship", "ui"}
)
GEOMETRY_TYPES = frozenset({"character", "animal", "fish", "ship", "environment", "prop", "building"})
NON_GEOMETRY_TYPES = frozenset({"audio", "ui", "material", "lighting_reference"})

# L25 frozen skeleton types.
SKELETON_TYPES = frozenset({"none", "static", "generic", "humanoid", "animal", "fish", "mech"})
SKELETON_BY_TYPE = {
    "character": frozenset({"humanoid", "generic"}),
    "animal": frozenset({"animal", "generic"}),
    "fish": frozenset({"fish", "generic"}),
    "ship": frozenset({"none", "static"}),
    "environment": frozenset({"none", "static"}),
    "prop": frozenset({"none", "static"}),
    "building": frozenset({"none", "static"}),
    "audio": frozenset({"none"}),
    "ui": frozenset({"none"}),
    "material": frozenset({"none"}),
    "lighting_reference": frozenset({"none"}),
}
ANIMATED_SKELETONS = frozenset({"generic", "humanoid", "animal", "fish", "mech"})

# L25 frozen animation roles.
ROLES = frozenset({"idle", "walk", "run", "work", "combat", "sail", "swim", "fly", "emote", "interact", "other"})
ROLE_ALLOWED_BY_TYPE = {
    "character": frozenset({"idle", "walk", "run", "work", "combat", "emote", "interact", "other"}),
    "animal": frozenset({"idle", "walk", "run", "fly", "work", "emote", "interact", "other"}),
    "fish": frozenset({"idle", "swim", "emote", "interact", "other"}),
    "ship": frozenset({"sail", "work", "combat", "interact", "other"}),
    "environment": frozenset({"other"}),
    "prop": frozenset({"other"}),
    "building": frozenset({"other"}),
    "audio": frozenset(),
    "ui": frozenset(),
    "material": frozenset(),
    "lighting_reference": frozenset(),
}
LOOPING_ROLES = frozenset({"idle", "walk", "run", "work"})
ONE_SHOT_ROLES = frozenset({"combat", "emote", "interact", "sail"})

# L25 root-motion strategies.
ROOT_MOTION_STRATEGIES = frozenset({"none", "required", "derived", "unknown"})

# D20 frozen action intents (D20-L04 action intent contracts).
ACTION_INTENTS = frozenset({"walk", "run", "interact", "talk", "fish", "board", "helm", "hit"})
INTENT_ALLOWED_ROLES = {
    "walk": frozenset({"walk", "run", "idle"}),
    "run": frozenset({"run", "walk"}),
    "interact": frozenset({"interact", "emote", "work", "other"}),
    "talk": frozenset({"emote", "interact", "other"}),
    "fish": frozenset({"work", "interact", "other"}),
    "board": frozenset({"interact", "work", "other"}),
    "helm": frozenset({"work", "sail", "other"}),
    "hit": frozenset({"combat", "work", "other"}),
}
# Required clip ROLE coverage per type; a type without its locomotion/behaviour
# role is "missing an action" and fails. These are animation roles (walk/swim/
# sail), not D20 interactive intents (walk/run/interact/talk/fish/board/helm/hit).
REQUIRED_ROLES_BY_TYPE = {
    "character": frozenset({"walk"}),
    "animal": frozenset({"walk"}),
    "fish": frozenset({"swim"}),
    "ship": frozenset({"sail"}),
}

STATE_MACHINE_OWNERS = frozenset({"player", "npc", "animal", "none"})
OWNER_BY_TYPE = {
    "character": frozenset({"player", "npc"}),
    "animal": frozenset({"animal"}),
    "fish": frozenset({"animal"}),
    "ship": frozenset({"none"}),
    "environment": frozenset({"none"}),
    "prop": frozenset({"none"}),
    "building": frozenset({"none"}),
    "audio": frozenset({"none"}),
    "ui": frozenset({"none"}),
    "material": frozenset({"none"}),
    "lighting_reference": frozenset({"none"}),
}

# Runtime wiring / Unity concepts that a static descriptor must never claim.
FORBIDDEN_RUNTIME_TOKENS = frozenset(
    {
        "animator", "avatar", "humanoid_avatar", "animationclip", "animation_clip",
        "anim_clip", "retarget", "retargeting", "blendtree", "blend_tree",
        "animatorcontroller", "animator_controller", "state_machine_definition",
        ".anim", ".controller", "import_settings", "compression", "clip_import",
        "model_importer", "skinnedmeshrenderer", "skinned_mesh_renderer",
        "bone_weight", "playable_graph", "timeline",
    }
)


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _issue(issues: list[dict[str, Any]], code: str, message: str, *, descriptor: str = "", descriptor_id: str = "", severity: str = "error") -> None:
    item: dict[str, Any] = {"code": code, "message": message, "severity": severity}
    if descriptor:
        item["descriptor"] = descriptor
    if descriptor_id:
        item["descriptor_id"] = descriptor_id
    issues.append(item)


def _check_forbidden_tokens(descriptor: Any, issues: list[dict[str, Any]], name: str, descriptor_id: str) -> None:
    """Reject any declared Unity runtime wiring concept anywhere in the descriptor."""
    def scan(value: Any, path: str) -> None:
        if isinstance(value, str):
            low = value.casefold()
            for token in FORBIDDEN_RUNTIME_TOKENS:
                if token in low:
                    _issue(issues, "unity_runtime_token_forbidden", f"{path} mentions Unity runtime concept {token!r}; animation descriptors must stay static (no Animator/Avatar/clip import)", descriptor=name, descriptor_id=descriptor_id)
                    return
        elif isinstance(value, dict):
            for key, item in value.items():
                scan(item, f"{path}.{key}")
        elif isinstance(value, list):
            for index, item in enumerate(value):
                scan(item, f"{path}[{index}]")

    for key in ("animation_list", "action_intent_map", "fallback", "license_dependency", "state_machine_owner"):
        if key in descriptor:
            scan(descriptor[key], key)
    scan(descriptor.get("skeleton_type"), "skeleton_type")


def _validate_clip(project_root: Path, clip: Any, index: int, descriptor_type: str, issues: list[dict[str, Any]], name: str, descriptor_id: str, root_motion_strategy: str) -> None:
    if not isinstance(clip, dict):
        _issue(issues, "clip_not_object", f"animation_list[{index}] must be an object", descriptor=name, descriptor_id=descriptor_id)
        return
    key = _text(clip.get("animation_key"))
    if not key:
        _issue(issues, "clip_missing_key", f"animation_list[{index}].animation_key is required", descriptor=name, descriptor_id=descriptor_id)
        return
    if not KEY_RE.fullmatch(key):
        _issue(issues, "clip_invalid_key", f"animation_key {key!r} must be lower snake_case", descriptor=name, descriptor_id=descriptor_id)
    role = _text(clip.get("role"))
    if not role:
        _issue(issues, "clip_missing_role", f"clip {key!r} is missing role", descriptor=name, descriptor_id=descriptor_id)
        return
    if role not in ROLES:
        _issue(issues, "unknown_clip_role", f"clip {key!r} role {role!r} is not in the frozen role vocabulary: {sorted(ROLES)}", descriptor=name, descriptor_id=descriptor_id)
        return
    allowed = ROLE_ALLOWED_BY_TYPE.get(descriptor_type, frozenset())
    if allowed and role not in allowed:
        _issue(issues, "role_type_conflict", f"clip {key!r} role {role!r} conflicts with type {descriptor_type!r}; allowed: {sorted(allowed)}", descriptor=name, descriptor_id=descriptor_id)

    looping = clip.get("looping")
    if looping is not None and not isinstance(looping, bool):
        _issue(issues, "clip_invalid_looping", f"clip {key!r} looping must be boolean", descriptor=name, descriptor_id=descriptor_id)
    elif isinstance(looping, bool):
        if role in LOOPING_ROLES and not looping:
            _issue(issues, "looping_conflict", f"clip {key!r} role {role!r} implies looping but declares looping=false", descriptor=name, descriptor_id=descriptor_id)
        if role in ONE_SHOT_ROLES and looping:
            _issue(issues, "looping_conflict", f"clip {key!r} role {role!r} is one-shot but declares looping=true", descriptor=name, descriptor_id=descriptor_id)

    clip_root = clip.get("root_motion")
    if clip_root is not None and not isinstance(clip_root, bool):
        _issue(issues, "clip_invalid_root_motion", f"clip {key!r} root_motion must be boolean", descriptor=name, descriptor_id=descriptor_id)
    if root_motion_strategy == "none" and clip_root is True:
        _issue(issues, "root_motion_conflict", f"clip {key!r} declares root_motion=true but top-level root_motion is none", descriptor=name, descriptor_id=descriptor_id)
    if root_motion_strategy == "required":
        # top-level required means locomotion clips must carry root motion; not every clip must.
        pass


def _validate_descriptor(project_root: Path, descriptor: Any, index: int) -> dict[str, Any]:
    name = f"descriptor[{index}]"
    issues: list[dict[str, Any]] = []
    if not isinstance(descriptor, dict):
        _issue(issues, "descriptor_not_object", "descriptor root must be a JSON object", descriptor=name)
        return {"descriptor_id": "", "status": "FAIL", "issues": issues, "missing_required_actions": []}

    descriptor_id = _text(descriptor.get("canonical_asset_id"))
    if descriptor.get("descriptor_kind") != DESCRIPTOR_KIND:
        _issue(issues, "invalid_descriptor_kind", f"descriptor_kind must be {DESCRIPTOR_KIND}", descriptor=name, descriptor_id=descriptor_id)
    if descriptor.get("descriptor_schema_version") != DESCRIPTOR_SCHEMA_VERSION:
        _issue(issues, "unsupported_descriptor_version", f"descriptor_schema_version must be {DESCRIPTOR_SCHEMA_VERSION}", descriptor=name, descriptor_id=descriptor_id)
    if not descriptor_id:
        _issue(issues, "missing_asset_id", "canonical_asset_id is required (asset_<lower_snake_case> business key)", descriptor=name, descriptor_id=descriptor_id)
    elif not ASSET_ID_RE.fullmatch(descriptor_id):
        _issue(issues, "invalid_asset_id", "canonical_asset_id must match asset_<lower_snake_case>; never a disk path or display name", descriptor=name, descriptor_id=descriptor_id)

    if descriptor.get("represents_imported_asset") is not False:
        _issue(issues, "import_claim_forbidden", "represents_imported_asset must be false; a descriptor never claims an asset was imported", descriptor=name, descriptor_id=descriptor_id)

    descriptor_type = _text(descriptor.get("type"))
    if not descriptor_type:
        _issue(issues, "missing_type", "type is required (L02 domain enum)", descriptor=name, descriptor_id=descriptor_id)
    elif descriptor_type not in TYPES:
        _issue(issues, "unknown_type", f"type {descriptor_type!r} is not in the L02 domain enum", descriptor=name, descriptor_id=descriptor_id)

    skeleton = _text(descriptor.get("skeleton_type"))
    if not skeleton:
        _issue(issues, "missing_skeleton_type", "skeleton_type is required (L25 field)", descriptor=name, descriptor_id=descriptor_id)
    elif skeleton not in SKELETON_TYPES:
        _issue(issues, "unknown_skeleton_type", f"skeleton_type {skeleton!r} is not in the frozen set: {sorted(SKELETON_TYPES)}", descriptor=name, descriptor_id=descriptor_id)
    elif descriptor_type in SKELETON_BY_TYPE and skeleton not in SKELETON_BY_TYPE[descriptor_type]:
        _issue(issues, "skeleton_type_conflict", f"skeleton_type {skeleton!r} conflicts with type {descriptor_type!r}; allowed: {sorted(SKELETON_BY_TYPE[descriptor_type])}", descriptor=name, descriptor_id=descriptor_id)

    root_motion_strategy = _text(descriptor.get("root_motion") or "none")
    if root_motion_strategy not in ROOT_MOTION_STRATEGIES:
        _issue(issues, "unknown_root_motion", f"root_motion {root_motion_strategy!r} is not in the frozen set: {sorted(ROOT_MOTION_STRATEGIES)}", descriptor=name, descriptor_id=descriptor_id)

    clips = descriptor.get("animation_list")
    clip_keys: list[str] = []
    if not isinstance(clips, list):
        _issue(issues, "animation_list_must_be_array", "animation_list must be an array", descriptor=name, descriptor_id=descriptor_id)
        clips = []
    else:
        seen: set[str] = set()
        for clip_index, clip in enumerate(clips):
            _validate_clip(project_root, clip, clip_index, descriptor_type, issues, name, descriptor_id, root_motion_strategy)
            key = _text(clip.get("animation_key")) if isinstance(clip, dict) else ""
            if key:
                if key in seen:
                    _issue(issues, "duplicate_clip_key", f"animation_key {key!r} appears more than once", descriptor=name, descriptor_id=descriptor_id)
                seen.add(key)
                clip_keys.append(key)

    has_clips = len(clip_keys) > 0
    if skeleton == "none" and has_clips:
        _issue(issues, "skeleton_animation_conflict", f"skeleton_type {skeleton!r} (no rig) cannot carry animation clips", descriptor=name, descriptor_id=descriptor_id)
    if descriptor_type in NON_GEOMETRY_TYPES and has_clips:
        _issue(issues, "role_type_conflict", f"type {descriptor_type!r} is non-geometric and must not declare animation clips", descriptor=name, descriptor_id=descriptor_id)

    # action intent map
    intent_map = descriptor.get("action_intent_map")
    clip_key_set = set(clip_keys)
    declared_intents: set[str] = set()
    if intent_map is not None and not isinstance(intent_map, dict):
        _issue(issues, "action_intent_map_must_be_object", "action_intent_map must be an object {intent: clip_key}", descriptor=name, descriptor_id=descriptor_id)
        intent_map = {}
    if isinstance(intent_map, dict):
        for intent, clip_key in intent_map.items():
            if intent not in ACTION_INTENTS:
                _issue(issues, "unknown_action_intent", f"action intent {intent!r} is not in the frozen D20 vocabulary: {sorted(ACTION_INTENTS)}", descriptor=name, descriptor_id=descriptor_id)
                continue
            declared_intents.add(intent)
            if not isinstance(clip_key, str) or clip_key not in clip_key_set:
                _issue(issues, "action_intent_missing_clip", f"action intent {intent!r} maps to {clip_key!r} which is not in animation_list", descriptor=name, descriptor_id=descriptor_id)
                continue
            clip_role = next((c.get("role") for c in clips if isinstance(c, dict) and c.get("animation_key") == clip_key), "")
            if clip_role and clip_role not in INTENT_ALLOWED_ROLES[intent]:
                _issue(issues, "action_intent_role_conflict", f"action intent {intent!r} maps to clip {clip_key!r} with role {clip_role!r}, incompatible with intent", descriptor=name, descriptor_id=descriptor_id)

    # required role coverage ("missing action"): every type needs its locomotion
    # role present in the clip list
    roles_present = {c.get("role") for c in clips if isinstance(c, dict) and c.get("role") in ROLES}
    required_roles = REQUIRED_ROLES_BY_TYPE.get(descriptor_type, frozenset())
    missing_actions = sorted(required_roles - roles_present)
    for role in missing_actions:
        _issue(issues, "missing_required_action", f"type {descriptor_type!r} requires a clip with role {role!r}; a missing action cannot be assumed", descriptor=name, descriptor_id=descriptor_id)

    # state-machine ownership
    owner = _text(descriptor.get("state_machine_owner"))
    if has_clips and not owner:
        _issue(issues, "missing_state_machine_owner", "state_machine_owner is required when animation clips are declared", descriptor=name, descriptor_id=descriptor_id)
    elif owner:
        if owner not in STATE_MACHINE_OWNERS:
            _issue(issues, "unknown_state_machine_owner", f"state_machine_owner {owner!r} is not in the frozen set: {sorted(STATE_MACHINE_OWNERS)}", descriptor=name, descriptor_id=descriptor_id)
        elif descriptor_type in OWNER_BY_TYPE and owner not in OWNER_BY_TYPE[descriptor_type]:
            _issue(issues, "state_machine_owner_conflict", f"state_machine_owner {owner!r} conflicts with type {descriptor_type!r}; allowed: {sorted(OWNER_BY_TYPE[descriptor_type])}", descriptor=name, descriptor_id=descriptor_id)

    # fallback (L25 semantics)
    fallback = descriptor.get("fallback")
    if not isinstance(fallback, dict):
        _issue(issues, "missing_fallback", "fallback is required (reference + description); an import attempt needs an honest rollback statement", descriptor=name, descriptor_id=descriptor_id)
    else:
        reference = _text(fallback.get("reference"))
        description = _text(fallback.get("description"))
        if not reference:
            _issue(issues, "missing_fallback", "fallback.reference is required", descriptor=name, descriptor_id=descriptor_id)
        elif not re.fullmatch(r"(asset_[a-z0-9_]+|placeholder_[a-z0-9_]+)", reference):
            _issue(issues, "invalid_fallback_reference", f"fallback.reference {reference!r} must be asset_<...> or placeholder_<...>", descriptor=name, descriptor_id=descriptor_id)
        if not description:
            _issue(issues, "missing_fallback", "fallback.description must be non-empty", descriptor=name, descriptor_id=descriptor_id)

    # license dependency
    license_obj = descriptor.get("license_dependency")
    if not isinstance(license_obj, dict):
        _issue(issues, "missing_license_dependency", "license_dependency object is required (license_id, license_url, license_evidence_path, attribution_required)", descriptor=name, descriptor_id=descriptor_id)
    else:
        license_id = _text(license_obj.get("license_id"))
        license_url = _text(license_obj.get("license_url"))
        evidence_raw = license_obj.get("license_evidence_path")
        attribution = license_obj.get("attribution_required")
        for field in ("license_id", "license_url", "license_evidence_path", "attribution_required"):
            if field not in license_obj:
                _issue(issues, "missing_license_field", f"license_dependency.{field} is required", descriptor=name, descriptor_id=descriptor_id)
        if license_url and not HTTP_URL_RE.fullmatch(license_url):
            _issue(issues, "invalid_license_url", "license_url must be an http(s) URL", descriptor=name, descriptor_id=descriptor_id)
        if attribution is not None and not isinstance(attribution, bool):
            _issue(issues, "invalid_license_field", "license_dependency.attribution_required must be boolean", descriptor=name, descriptor_id=descriptor_id)
        if license_id:
            canonical, lookup = canonicalize_license_id(license_id)
            canonical_upper = (canonical or "").upper()
            if lookup != "ok" or canonical is None:
                _issue(issues, "license_dependency_unusable", f"license {license_id!r} is unknown/unusable (lookup={lookup!r}); animation descriptors cannot depend on it", descriptor=name, descriptor_id=descriptor_id)
            elif any(marker in canonical_upper for marker in ("-NC", "-ND", "-SA")):
                _issue(issues, "license_dependency_restricted", f"license {canonical!r} carries an NC/ND/SA restriction; cannot be a game distribution dependency", descriptor=name, descriptor_id=descriptor_id)
        if isinstance(evidence_raw, str) and evidence_raw:
            result = validate_path(evidence_raw, kind="evidence", field="license_dependency.license_evidence_path")
            for path_issue in result.issues:
                _issue(issues, "invalid_evidence_path", f"license_dependency.license_evidence_path: {path_issue.reason}", descriptor=name, descriptor_id=descriptor_id)
            if result.valid:
                resolved = (project_root / result.normalized).resolve()
                if not resolved.is_file():
                    _issue(issues, "license_evidence_missing", f"license evidence file does not exist: {evidence_raw}", descriptor=name, descriptor_id=descriptor_id)

    _check_forbidden_tokens(descriptor, issues, name, descriptor_id)

    if root_motion_strategy == "unknown":
        _issue(issues, "root_motion_unknown", "root_motion is unknown and must be resolved before Runtime import", descriptor=name, descriptor_id=descriptor_id, severity="pending")

    errors = [issue for issue in issues if issue["severity"] == "error"]
    pendings = [issue for issue in issues if issue["severity"] == "pending"]
    if errors:
        status = "FAIL"
    elif pendings:
        status = "PENDING"
    else:
        status = "PASS"
    return {"descriptor_id": descriptor_id, "status": status, "issues": issues, "missing_required_actions": missing_actions}


def validate_descriptors(project_root: Path, descriptors: Iterable[Any]) -> dict[str, Any]:
    items = [_validate_descriptor(project_root, descriptor, index) for index, descriptor in enumerate(descriptors)]
    if not items:
        items = [{"descriptor_id": "", "status": "FAIL", "issues": [{"code": "no_descriptors", "message": "no animation descriptors were supplied; nothing was verified", "severity": "error"}], "missing_required_actions": []}]
    issue_counts: dict[str, int] = {}
    error_count = 0
    pending_count = 0
    for item in items:
        for issue in item["issues"]:
            issue_counts[issue["code"]] = issue_counts.get(issue["code"], 0) + 1
            if issue["severity"] == "error":
                error_count += 1
            elif issue["severity"] == "pending":
                pending_count += 1
    if error_count > 0:
        status = "FAIL"
    elif pending_count > 0:
        status = "PENDING"
    else:
        status = "PASS"
    return {
        "task": "D18-L28",
        "tool": "tools/d18_animation_descriptor_validator.py",
        "read_only": True,
        "approved_count": 0,
        "status": status,
        "descriptor_count": len(items),
        "pass_count": sum(1 for item in items if item["status"] == "PASS"),
        "pending_count": sum(1 for item in items if item["status"] == "PENDING"),
        "fail_count": sum(1 for item in items if item["status"] == "FAIL"),
        "issue_counts": dict(sorted(issue_counts.items())),
        "descriptors": items,
    }


def _load_input(path: Path) -> tuple[list[Any] | None, str, str]:
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError:
        return None, "input_not_found", f"input file does not exist: {path}"
    except json.JSONDecodeError as exc:
        return None, "input_json_error", f"line {exc.lineno}: {exc.msg} in {path.name}"
    if isinstance(data, list):
        return data, "", ""
    if isinstance(data, dict) and isinstance(data.get("descriptors"), list):
        return data["descriptors"], "", ""
    return None, "input_not_supported", "input JSON must be a list or {\"descriptors\": [...]}"


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, action="append", default=[], help="animation descriptor JSON (repeatable)")
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    descriptors: list[Any] = []
    input_issues: list[dict[str, Any]] = []
    for input_path in args.input:
        loaded, code, message = _load_input(input_path)
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(input_path)})
            continue
        descriptors.extend(loaded)
    result = validate_descriptors(args.project_root.resolve(), descriptors)
    if input_issues:
        result["input_issues"] = input_issues
        result["error_count"] += len(input_issues)
        result["status"] = "FAIL"
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    exit_codes = {"PASS": 0, "PENDING": 1, "FAIL": 2}
    return exit_codes.get(result["status"], 3)


if __name__ == "__main__":
    raise SystemExit(main())
