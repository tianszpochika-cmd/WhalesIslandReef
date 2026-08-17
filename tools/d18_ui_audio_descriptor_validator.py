"""Read-only UI/audio descriptor validator for D18 logic assets (D18-L29).

Validates ``d18_ui_audio_descriptor`` JSON records (static data produced by the
L42 catalog compiler for the UI/audio categories frozen by D18-L14):

* purpose: a frozen per-category purpose vocabulary; unknown purposes fail;
* category: one of the five UI/audio classes of D18-L14 (ui_icon, input_prompt,
  audio_cue, music, font); material/texture/hdri/weather_presentation belong to
  the D18-L27 material descriptor;
* license: L02 license/rights blocks normalized with the D18-L04 normalizer;
  missing license, missing fields, NC/personal/unknown, missing evidence file
  and ``distribution_with_game`` gaps for audio classes fail; fonts require
  OFL-1.1 or an explicit embedding license (D18-L14 section 3.2);
* localization/accessibility labels: ``ui_icon``/``input_prompt`` require
  ``accessibility_label_key``, ``audio_cue``/``music`` require ``subtitle_key``
  (D18-L14 section 5.2); label values must be dotted lowercase keys, never
  hardcoded user-visible text; ``input_prompt`` must reference an input action
  id (D22-L11/L12) instead of embedding keycap text;
* alt-text/mute fallback: every descriptor must declare a ``fallback`` object
  (alt text, subtitle, visual indicator, silent or explicitly reasoned none);
  missing fallback, missing reference and reference mismatch fail;
* runtime intent: a frozen per-category intent vocabulary, consistent with the
  purpose; fields that declare runtime wiring (volume, mixer, AudioSource,
  Canvas, Sprite, Shader, color space, compression, import settings, ...) are
  rejected as out-of-bounds (D18-L14 section 7 red lines).

The tool is strictly read-only and offline: it parses JSON/stat data only, never
creates Canvas, AudioSource, buttons, mixers or imports audio, never approves
assets and never writes the registry. It validates D18-L14 taxonomy machine JSON
(extracted from the frozen taxonomy document) against its baked constants.

The D18-L25 import-descriptor schema was not merged when this task ran; this
tool therefore uses only wave-1 frozen fields (D18-L01 baseline section 6,
L02 record fields, L04 license semantics, L07 rights, L14 taxonomy) and defines
no L25-owned fields (units, axes, bounds, material slots, skeleton, root-motion,
collision/LOD intent).
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from pathlib import Path
from typing import Any, Iterable

from d18_asset_path_validator import validate_path  # D18-L08 frozen path boundaries
from d18_license_normalizer import normalize_license  # D18-L04 license normalization


DESCRIPTOR_KIND = "d18_ui_audio_descriptor"
DESCRIPTOR_SCHEMA_VERSION = 1

TAXONOMY_DOCUMENT = "docs/game/design/common/assets/D18_TAXONOMY_UI_AUDIO.md"

UI_AUDIO_CATEGORIES = frozenset({"ui_icon", "input_prompt", "audio_cue", "music", "font"})
ALL_L14_CATEGORIES = frozenset(
    {"ui_icon", "input_prompt", "audio_cue", "music", "font", "material", "texture", "hdri", "weather_presentation"}
)
CATEGORY_SCHEMA_MAP = {
    "ui_icon": "ui",
    "input_prompt": "ui",
    "font": "ui",
    "audio_cue": "audio",
    "music": "audio",
}

PURPOSES: dict[str, frozenset[str]] = {
    "ui_icon": frozenset({"feedback_icon", "navigation_icon", "state_icon", "panel_slice"}),
    "input_prompt": frozenset({"input_action_hint"}),
    "audio_cue": frozenset({"ui_feedback_cue", "impact_sfx", "footstep_sfx", "interaction_sfx"}),
    "music": frozenset({"theme_music", "ambient_music", "menu_music"}),
    "font": frozenset({"ui_text_font"}),
}

RUNTIME_INTENTS: dict[str, frozenset[str]] = {
    "ui_icon": frozenset({"ui_feedback_display", "ui_navigation_display", "ui_state_display", "panel_asset"}),
    "input_prompt": frozenset({"input_hint_display"}),
    "audio_cue": frozenset({"audio_feedback_playback", "impact_playback", "footstep_playback", "interaction_playback"}),
    "music": frozenset({"music_playback"}),
    "font": frozenset({"text_rendering"}),
}

PURPOSE_RUNTIME_INTENT: dict[str, str] = {
    "feedback_icon": "ui_feedback_display",
    "navigation_icon": "ui_navigation_display",
    "state_icon": "ui_state_display",
    "panel_slice": "panel_asset",
    "input_action_hint": "input_hint_display",
    "ui_feedback_cue": "audio_feedback_playback",
    "impact_sfx": "impact_playback",
    "footstep_sfx": "footstep_playback",
    "interaction_sfx": "interaction_playback",
    "theme_music": "music_playback",
    "ambient_music": "music_playback",
    "menu_music": "music_playback",
    "ui_text_font": "text_rendering",
}

FALLBACK_KINDS: dict[str, frozenset[str]] = {
    "ui_icon": frozenset({"alt_text", "text_label", "none"}),
    "input_prompt": frozenset({"alt_text", "text_label", "none"}),
    "audio_cue": frozenset({"subtitle", "visual_indicator", "silent", "none"}),
    "music": frozenset({"subtitle", "visual_indicator", "silent", "none"}),
    "font": frozenset({"system_font", "none"}),
}

REQUIRED_LABEL_KEYS: dict[str, tuple[str, ...]] = {
    "ui_icon": ("accessibility_label_key",),
    "input_prompt": ("accessibility_label_key",),
    "audio_cue": ("subtitle_key",),
    "music": ("subtitle_key",),
    "font": (),
}

FORBIDDEN_RUNTIME_TOKENS = frozenset(
    {
        "volume",
        "attenuation",
        "spatialization",
        "spatialize",
        "mixer",
        "audiosource",
        "audio_source",
        "canvas",
        "sprite",
        "image_component",
        "imagecomponent",
        "graphic",
        "recttransform",
        "textmeshpro",
        "text_component",
        "textcomponent",
        "shader",
        "material_instance",
        "materialinstance",
        "colorspace",
        "color_space",
        "compression",
        "mipmap",
        "import_settings",
        "importsettings",
        "particles",
        "particle",
        "skybox",
        "lighting_bake",
        "visual_effects",
        "visualeffects",
        "prefab_assembly",
        "playback_enabled",
        "loop_enabled",
    }
)

ASSET_ID_RE = re.compile(r"^asset_[a-z0-9_]+$")
SOURCE_ID_RE = re.compile(r"^source_[a-z0-9_]+$")
ACTION_ID_RE = re.compile(r"^[a-z][a-z0-9_]*$")
UI_KEY_RE = re.compile(r"^ui\.[a-z0-9_]+\.[a-z0-9_]+\.[a-z0-9_]+$")
AUDIO_KEY_RE = re.compile(r"^audio\.[a-z0-9_]+\.[a-z0-9_]+\.[a-z0-9_]+$")
DOTTED_KEY_RE = re.compile(r"^[a-z][a-z0-9_]*(\.[a-z0-9_]+)+$")
HTTP_URL_RE = re.compile(r"^https?://[^\s]+$")
JSON_FENCE_RE = re.compile(r"```json\s*\n(.*?)```", re.DOTALL)


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _inside(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def _issue(
    issues: list[dict[str, Any]],
    code: str,
    message: str,
    *,
    descriptor: str = "",
    descriptor_id: str = "",
    severity: str = "error",
) -> None:
    item: dict[str, Any] = {"code": code, "message": message, "severity": severity}
    if descriptor:
        item["descriptor"] = descriptor
    if descriptor_id:
        item["descriptor_id"] = descriptor_id
    issues.append(item)


def _label_key_issue(value: Any, field: str, key_re: re.Pattern[str], issues: list[dict[str, Any]], name: str, descriptor_id: str) -> bool:
    if not isinstance(value, str) or not value.strip():
        _issue(issues, "invalid_label_key", f"{field} must be a non-empty localization key", descriptor=name, descriptor_id=descriptor_id)
        return False
    if re.search(r"\s", value) or any(ord(char) > 127 for char in value):
        _issue(issues, "hardcoded_label_text", f"{field} must be a localization key, not user-visible text: {value!r}", descriptor=name, descriptor_id=descriptor_id)
        return False
    if not key_re.fullmatch(value):
        _issue(issues, "invalid_label_key", f"{field} does not match the frozen key format: {value!r}", descriptor=name, descriptor_id=descriptor_id)
        return False
    return True


def _check_path_safe(project_root: Path, raw: Any, field: str, issues: list[dict[str, Any]], name: str, descriptor_id: str) -> str | None:
    if not isinstance(raw, str) or not raw.strip():
        return None
    stripped = raw.strip()
    if re.match(r"^[a-zA-Z][a-zA-Z0-9+.\-]*://", stripped) or stripped.startswith("file:"):
        _issue(issues, "invalid_evidence_path", f"{field} is a URL, not a repository-relative path: {raw}", descriptor=name, descriptor_id=descriptor_id)
        return None
    result = validate_path(stripped, kind="evidence", field=field)
    for path_issue in result.issues:
        _issue(issues, "invalid_evidence_path", f"{field}: {path_issue.value} — {path_issue.reason}", descriptor=name, descriptor_id=descriptor_id)
    if not result.valid:
        return None
    resolved = (project_root / result.normalized).resolve()
    if not _inside(resolved, project_root):
        _issue(issues, "invalid_evidence_path", f"{field} resolves outside the project: {raw}", descriptor=name, descriptor_id=descriptor_id)
        return None
    return result.normalized


def validate_descriptor(
    project_root: Path,
    descriptor: Any,
    issues: list[dict[str, Any]],
    *,
    name: str = "",
    localization_keys: set[str] | None = None,
) -> None:
    if not isinstance(descriptor, dict):
        _issue(issues, "descriptor_not_object", "descriptor root must be a JSON object", descriptor=name)
        return
    descriptor_id = _text(descriptor.get("canonical_asset_id"))
    if descriptor.get("descriptor_kind") != DESCRIPTOR_KIND:
        _issue(issues, "invalid_descriptor_kind", f"descriptor_kind must be {DESCRIPTOR_KIND}", descriptor=name, descriptor_id=descriptor_id)
    if descriptor.get("descriptor_schema_version") != DESCRIPTOR_SCHEMA_VERSION:
        _issue(issues, "unsupported_descriptor_version", f"descriptor_schema_version must be {DESCRIPTOR_SCHEMA_VERSION}", descriptor=name, descriptor_id=descriptor_id)

    if not _text(descriptor.get("canonical_asset_id")):
        _issue(issues, "missing_asset_id", "canonical_asset_id is required (asset_<lower_snake_case> business key)", descriptor=name, descriptor_id=descriptor_id)
    elif not ASSET_ID_RE.fullmatch(_text(descriptor.get("canonical_asset_id"))):
        _issue(issues, "invalid_asset_id", "canonical_asset_id must match asset_<lower_snake_case>; never a disk path or display name", descriptor=name, descriptor_id=descriptor_id)

    category = _text(descriptor.get("category"))
    if not category:
        _issue(issues, "missing_category", "category is required (D18-L14 UI/audio classes)", descriptor=name, descriptor_id=descriptor_id)
    elif category not in UI_AUDIO_CATEGORIES:
        if category in ALL_L14_CATEGORIES:
            _issue(issues, "unsupported_category", f"category {category!r} belongs to the D18-L27 material descriptor, not the UI/audio descriptor", descriptor=name, descriptor_id=descriptor_id)
        else:
            _issue(issues, "unknown_category", f"category {category!r} is not one of the D18-L14 UI/audio classes: {sorted(UI_AUDIO_CATEGORIES)}", descriptor=name, descriptor_id=descriptor_id)
        category = ""

    purpose = _text(descriptor.get("purpose"))
    if not purpose:
        _issue(issues, "missing_purpose", "purpose is required", descriptor=name, descriptor_id=descriptor_id)
    elif category and purpose not in PURPOSES[category]:
        allowed = PURPOSES.get(category, frozenset())
        if any(purpose in values for values in PURPOSES.values()):
            _issue(issues, "purpose_category_mismatch", f"purpose {purpose!r} is not allowed for category {category!r}; allowed: {sorted(allowed)}", descriptor=name, descriptor_id=descriptor_id)
        else:
            _issue(issues, "unknown_purpose", f"purpose {purpose!r} is not in the frozen purpose vocabulary; allowed for {category or '<category>'}: {sorted(allowed)}", descriptor=name, descriptor_id=descriptor_id)

    runtime_intent = _text(descriptor.get("runtime_intent"))
    if not runtime_intent:
        _issue(issues, "missing_runtime_intent", "runtime_intent is required", descriptor=name, descriptor_id=descriptor_id)
    elif category and runtime_intent not in RUNTIME_INTENTS[category]:
        _issue(issues, "unknown_runtime_intent", f"runtime_intent {runtime_intent!r} is not allowed for category {category!r}; allowed: {sorted(RUNTIME_INTENTS.get(category, frozenset()))}", descriptor=name, descriptor_id=descriptor_id)
    if purpose and runtime_intent and PURPOSE_RUNTIME_INTENT.get(purpose) not in (None, runtime_intent):
        _issue(issues, "purpose_runtime_intent_mismatch", f"purpose {purpose!r} requires runtime_intent {PURPOSE_RUNTIME_INTENT[purpose]!r}, got {runtime_intent!r}", descriptor=name, descriptor_id=descriptor_id)

    source_id = descriptor.get("source_id")
    if source_id is not None and not (isinstance(source_id, str) and SOURCE_ID_RE.fullmatch(_text(source_id))):
        _issue(issues, "invalid_source_id", "source_id must match source_<lower_snake_case>", descriptor=name, descriptor_id=descriptor_id)

    license_obj = descriptor.get("license")
    if not isinstance(license_obj, dict):
        _issue(issues, "missing_license", "license object is required (license_id, license_url, license_evidence_path, attribution_required)", descriptor=name, descriptor_id=descriptor_id)
    else:
        license_id = _text(license_obj.get("license_id"))
        license_url = _text(license_obj.get("license_url"))
        evidence_raw = license_obj.get("license_evidence_path")
        attribution = license_obj.get("attribution_required")
        for field in ("license_id", "license_url", "license_evidence_path", "attribution_required"):
            if field not in license_obj:
                _issue(issues, "missing_license_field", f"license.{field} is required", descriptor=name, descriptor_id=descriptor_id)
        if license_url and not HTTP_URL_RE.fullmatch(license_url):
            _issue(issues, "invalid_license_url", "license_url must be an http(s) URL", descriptor=name, descriptor_id=descriptor_id)
        if attribution is not None and not isinstance(attribution, bool):
            _issue(issues, "invalid_license_field", "license.attribution_required must be boolean", descriptor=name, descriptor_id=descriptor_id)
        normalized_evidence = _check_path_safe(project_root, evidence_raw, "license.license_evidence_path", issues, name, descriptor_id)
        if evidence_raw is not None:
            resolved_evidence = (project_root / normalized_evidence).resolve() if normalized_evidence else None
            if resolved_evidence is not None and not resolved_evidence.is_file():
                _issue(issues, "license_evidence_missing", f"license evidence file does not exist: {evidence_raw}", descriptor=name, descriptor_id=descriptor_id)
            elif resolved_evidence is not None and resolved_evidence.stat().st_size == 0:
                _issue(issues, "license_evidence_missing", f"license evidence file is empty: {evidence_raw}", descriptor=name, descriptor_id=descriptor_id)

        rights = license_obj.get("rights")
        rights_ok = True
        if not isinstance(rights, dict):
            _issue(issues, "missing_rights", "license.rights with commercial_use/modification_allowed/repository_storage/distribution_with_game is required", descriptor=name, descriptor_id=descriptor_id)
            rights_ok = False
        else:
            for field in ("commercial_use", "modification_allowed", "repository_storage", "distribution_with_game"):
                if not isinstance(rights.get(field), bool):
                    _issue(issues, "missing_rights_field", f"license.rights.{field} must be boolean", descriptor=name, descriptor_id=descriptor_id)
                    rights_ok = False

        if license_id and rights_ok and isinstance(attribution, bool):
            l04_record: dict[str, Any] = {
                "record_id": descriptor_id,
                "license_id": license_id,
                "attribution_required": attribution,
                "commercial_use": rights.get("commercial_use"),
                "modification_allowed": rights.get("modification_allowed"),
                "source_redistribution": "allowed" if rights.get("repository_storage") else "not_allowed",
                "build_distribution": "allowed" if rights.get("distribution_with_game") else "not_allowed",
            }
            normalized = normalize_license(l04_record)
            if normalized["status"] != "NORMALIZED":
                first_error = normalized["errors"][0] if normalized["errors"] else {}
                _issue(
                    issues,
                    "license_not_normalized",
                    f"license {license_id!r} is not normalized: {normalized['status_reason']} ({first_error.get('code', '')})",
                    descriptor=name,
                    descriptor_id=descriptor_id,
                )
        if category in ("audio_cue", "music") and rights_ok and isinstance(rights, dict) and rights.get("distribution_with_game") is not True:
            _issue(issues, "distribution_with_game_required", "audio_cue/music requires license.rights.distribution_with_game = true (D18-L14 section 3.2)", descriptor=name, descriptor_id=descriptor_id)
        if category == "font":
            embedding_explicit = license_obj.get("embedding_license_explicit")
            canonical = ""
            if license_id and rights_ok and isinstance(attribution, bool):
                canonical = normalize_license(l04_record).get("canonical_license") or ""
            if canonical != "OFL-1.1" and embedding_explicit is not True:
                _issue(issues, "font_license_not_ofl", "font requires OFL-1.1 or license.embedding_license_explicit = true (D18-L14 font policy)", descriptor=name, descriptor_id=descriptor_id)
            if embedding_explicit is not None and not isinstance(embedding_explicit, bool):
                _issue(issues, "invalid_license_field", "license.embedding_license_explicit must be boolean", descriptor=name, descriptor_id=descriptor_id)

    localization = descriptor.get("localization")
    if not isinstance(localization, dict):
        _issue(issues, "missing_localization", "localization object is required (accessibility/subtitle/variant label keys)", descriptor=name, descriptor_id=descriptor_id)
        localization = {}
    else:
        for field in REQUIRED_LABEL_KEYS.get(category, ()):
            if field not in localization or not _text(localization.get(field)):
                _issue(issues, "missing_accessibility_label" if field == "accessibility_label_key" else "missing_subtitle_key", f"localization.{field} is required for category {category}", descriptor=name, descriptor_id=descriptor_id)
        alt_value = localization.get("accessibility_label_key")
        if alt_value is not None and _label_key_issue(alt_value, "localization.accessibility_label_key", UI_KEY_RE, issues, name, descriptor_id):
            if localization_keys is not None and _text(alt_value) not in localization_keys:
                _issue(issues, "label_key_not_in_localization", f"localization.accessibility_label_key {alt_value!r} is not present in the localization set", descriptor=name, descriptor_id=descriptor_id)
        subtitle_value = localization.get("subtitle_key")
        if subtitle_value is not None and _label_key_issue(subtitle_value, "localization.subtitle_key", AUDIO_KEY_RE, issues, name, descriptor_id):
            if localization_keys is not None and _text(subtitle_value) not in localization_keys:
                _issue(issues, "label_key_not_in_localization", f"localization.subtitle_key {subtitle_value!r} is not present in the localization set", descriptor=name, descriptor_id=descriptor_id)
        name_value = localization.get("name_key")
        name_key_re = AUDIO_KEY_RE if category in ("audio_cue", "music") else UI_KEY_RE
        if name_value is not None and _label_key_issue(name_value, "localization.name_key", name_key_re, issues, name, descriptor_id):
            if localization_keys is not None and _text(name_value) not in localization_keys:
                _issue(issues, "label_key_not_in_localization", f"localization.name_key {name_value!r} is not present in the localization set", descriptor=name, descriptor_id=descriptor_id)
        for field in ("contrast_variant", "motion_reduced_variant"):
            value = localization.get(field)
            if value is not None and not (isinstance(value, str) and ASSET_ID_RE.fullmatch(_text(value))):
                _issue(issues, "invalid_variant_reference", f"localization.{field} must reference an asset_<id> variant, got {value!r}", descriptor=name, descriptor_id=descriptor_id)
        if category == "input_prompt":
            action_id = _text(localization.get("input_action_id"))
            if not action_id:
                _issue(issues, "missing_input_action_id", "input_prompt requires localization.input_action_id (D22-L11/L12 input action, no keycap text)", descriptor=name, descriptor_id=descriptor_id)
            elif not ACTION_ID_RE.fullmatch(action_id):
                _issue(issues, "invalid_input_action_id", "localization.input_action_id must be a lower snake_case action id, not keycap text", descriptor=name, descriptor_id=descriptor_id)

    fallback = descriptor.get("fallback")
    if not isinstance(fallback, dict):
        _issue(issues, "missing_fallback", "fallback object is required (alt text/subtitle/silent fallback declaration)", descriptor=name, descriptor_id=descriptor_id)
    else:
        kind = _text(fallback.get("kind"))
        if not kind:
            _issue(issues, "missing_fallback_kind", "fallback.kind is required", descriptor=name, descriptor_id=descriptor_id)
        elif category and kind not in FALLBACK_KINDS.get(category, frozenset()):
            _issue(issues, "unknown_fallback_kind", f"fallback.kind {kind!r} is not allowed for category {category!r}; allowed: {sorted(FALLBACK_KINDS.get(category, frozenset()))}", descriptor=name, descriptor_id=descriptor_id)
        else:
            if kind == "none":
                if not _text(fallback.get("reason")):
                    _issue(issues, "fallback_none_without_reason", "fallback.kind = none requires a non-empty reason", descriptor=name, descriptor_id=descriptor_id)
            elif kind != "silent":
                reference = _text(fallback.get("reference"))
                if not reference:
                    _issue(issues, "missing_fallback_reference", f"fallback.kind {kind!r} requires a reference (localization key or asset id)", descriptor=name, descriptor_id=descriptor_id)
                else:
                    if kind in ("subtitle", "alt_text", "text_label"):
                        expected = None
                        if kind == "subtitle":
                            expected = _text(localization.get("subtitle_key")) if isinstance(localization, dict) else ""
                        elif kind == "alt_text":
                            expected = _text(localization.get("accessibility_label_key")) if isinstance(localization, dict) else ""
                        if expected and reference != expected:
                            _issue(issues, "fallback_reference_mismatch", f"fallback.reference {reference!r} does not match localization key {expected!r}", descriptor=name, descriptor_id=descriptor_id)
                        elif not expected:
                            _issue(issues, "fallback_reference_mismatch", f"fallback.kind {kind!r} has no matching localization key in this descriptor", descriptor=name, descriptor_id=descriptor_id)
                        elif localization_keys is not None and reference not in localization_keys:
                            _issue(issues, "label_key_not_in_localization", f"fallback.reference {reference!r} is not present in the localization set", descriptor=name, descriptor_id=descriptor_id)
                    elif kind == "visual_indicator":
                        if not ASSET_ID_RE.fullmatch(reference):
                            _issue(issues, "invalid_fallback_reference", f"fallback.kind visual_indicator requires an asset_<id> reference, got {reference!r}", descriptor=name, descriptor_id=descriptor_id)
                    elif kind == "system_font":
                        if reference != "system_default" and not ASSET_ID_RE.fullmatch(reference):
                            _issue(issues, "invalid_fallback_reference", f"fallback.kind system_font requires system_default or an asset_<id> reference, got {reference!r}", descriptor=name, descriptor_id=descriptor_id)

    deferred = descriptor.get("runtime_deferred")
    if not isinstance(deferred, list):
        _issue(issues, "missing_runtime_deferred", "runtime_deferred must be a non-empty list of honestly registered runtime risks", descriptor=name, descriptor_id=descriptor_id)
    elif not deferred:
        _issue(issues, "empty_runtime_deferred", "runtime_deferred must not be empty", descriptor=name, descriptor_id=descriptor_id)
    else:
        for entry in deferred:
            if not isinstance(entry, str) or not entry.strip():
                _issue(issues, "invalid_runtime_deferred_entry", "runtime_deferred entries must be non-empty strings", descriptor=name, descriptor_id=descriptor_id)

    for token, path in _forbidden_runtime_fields(descriptor):
        _issue(issues, "forbidden_runtime_field", f"descriptor declares a runtime field out of bounds (D18-L14 section 7): {path} = {token!r}", descriptor=name, descriptor_id=descriptor_id)

    evidence_paths = descriptor.get("evidence_paths")
    if evidence_paths is not None:
        if not isinstance(evidence_paths, list) or not all(isinstance(item, str) for item in evidence_paths):
            _issue(issues, "invalid_evidence_path", "evidence_paths must be a list of repository-relative paths", descriptor=name, descriptor_id=descriptor_id)
        else:
            for item in evidence_paths:
                _check_path_safe(project_root, item, "evidence_paths[]", issues, name, descriptor_id)


def _forbidden_runtime_fields(descriptor: dict[str, Any]) -> list[tuple[str, str]]:
    found: list[tuple[str, str]] = []

    def walk(value: Any, path: str) -> None:
        if isinstance(value, dict):
            for key, item in value.items():
                field_path = f"{path}.{key}" if path else str(key)
                lowered = str(key).casefold().replace("-", "_")
                for token in FORBIDDEN_RUNTIME_TOKENS:
                    if token in lowered:
                        found.append((str(key), field_path))
                        break
                walk(item, field_path)

    walk(descriptor, "")
    return found


def extract_taxonomy_json(taxonomy_path: Path) -> dict[str, Any] | None:
    try:
        text = taxonomy_path.read_text(encoding="utf-8")
    except OSError:
        return None
    match = JSON_FENCE_RE.search(text)
    if match:
        try:
            return json.loads(match.group(1))
        except json.JSONDecodeError:
            return None
    try:
        data = json.loads(text)
    except json.JSONDecodeError:
        return None
    return data if isinstance(data, dict) else None


def check_taxonomy(taxonomy: dict[str, Any] | None, issues: list[dict[str, Any]], *, document: str = "") -> dict[str, Any]:
    if taxonomy is None:
        _issue(issues, "taxonomy_missing", f"taxonomy document not found or not parseable: {document or '<none>'}", severity="warning")
        return {"document": document, "matched": False, "errors": ["taxonomy not available"]}
    errors: list[str] = []
    categories = taxonomy.get("categories", [])
    ui_audio = {item.get("category") for item in categories if item.get("schema_category") in ("ui", "audio")}
    if ui_audio != set(UI_AUDIO_CATEGORIES):
        errors.append(f"UI/audio categories mismatch: expected {sorted(UI_AUDIO_CATEGORIES)}, got {sorted(ui_audio)}")
    schema_map = taxonomy.get("schema_category_map", {})
    for category, schema_category in CATEGORY_SCHEMA_MAP.items():
        if schema_map.get(category) != schema_category:
            errors.append(f"schema_category_map[{category}] = {schema_map.get(category)!r}, expected {schema_category!r}")
    localization_rules = taxonomy.get("localization_rules", {})
    if localization_rules.get("subtitle_key_required_for_audio_cue") is not True:
        errors.append("localization_rules.subtitle_key_required_for_audio_cue is not true")
    if localization_rules.get("accessibility_label_key_required_for_ui_icon_and_input_prompt") is not True:
        errors.append("localization_rules.accessibility_label_key_required_for_ui_icon_and_input_prompt is not true")
    if errors:
        for error in sorted(errors):
            _issue(issues, "taxonomy_mismatch", f"taxonomy cross-check failed: {error}")
        return {"document": document, "matched": False, "errors": sorted(errors)}
    return {"document": document, "matched": True, "errors": []}


def load_localization_keys(path: Path) -> tuple[set[str], int]:
    keys: set[str] = set()

    def collect(value: Any, prefix: str) -> None:
        if isinstance(value, dict):
            for key, item in value.items():
                dotted = f"{prefix}.{key}" if prefix else str(key)
                keys.add(dotted)
                collect(item, dotted)

    files = [path] if path.is_file() else sorted(item for item in path.rglob("*.json") if item.is_file())
    for file_path in files:
        try:
            data = json.loads(file_path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            continue
        if isinstance(data, dict) and isinstance(data.get("keys"), dict):
            collect(data["keys"], "")
        else:
            collect(data, "")
    return keys, len(files)


def discover_descriptor_files(descriptor_root: Path | None, descriptors: Iterable[Path]) -> list[Path]:
    paths: set[Path] = set()
    for path in descriptors:
        if path.is_file():
            paths.add(path.resolve())
        elif path.is_dir():
            paths.update(item.resolve() for item in path.rglob("*.json"))
    if descriptor_root is not None:
        if descriptor_root.is_file():
            paths.add(descriptor_root.resolve())
        elif descriptor_root.is_dir():
            paths.update(item.resolve() for item in descriptor_root.rglob("*.json"))
    return sorted(paths)


def validate_ui_audio_descriptors(
    project_root: Path,
    descriptor_paths: Iterable[Path],
    *,
    taxonomy_path: Path | None = None,
    localization: Path | None = None,
) -> dict[str, Any]:
    project_root = project_root.resolve()
    taxonomy_path = taxonomy_path.resolve() if taxonomy_path is not None else None
    paths = sorted(Path(item).resolve() for item in descriptor_paths)
    issues: list[dict[str, Any]] = []
    localization_keys: set[str] | None = None
    localization_files = 0
    if localization is not None:
        localization_keys, localization_files = load_localization_keys(localization)
    if localization is not None and not localization_keys:
        _issue(issues, "localization_empty", "localization input provided but no keys were loaded", severity="error")

    taxonomy = extract_taxonomy_json(taxonomy_path) if taxonomy_path is not None else None
    taxonomy_display = ""
    if taxonomy_path is not None:
        taxonomy_display = str(taxonomy_path.relative_to(project_root)).replace("\\", "/") if _inside(taxonomy_path, project_root) else str(taxonomy_path)
    taxonomy_check = check_taxonomy(taxonomy, issues, document=taxonomy_display)

    descriptor_files: list[str] = []
    loaded = 0
    for path in paths:
        name = str(path.relative_to(project_root)).replace("\\", "/") if _inside(path, project_root) else str(path)
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except OSError as exc:
            _issue(issues, "descriptor_read_error", str(exc), descriptor=name)
            continue
        except json.JSONDecodeError as exc:
            _issue(issues, "descriptor_json_error", f"line {exc.lineno}: {exc.msg}", descriptor=name)
            continue
        if not isinstance(data, dict):
            loaded += 1
            descriptor_files.append(name)
            validate_descriptor(project_root, data, issues, name=name, localization_keys=localization_keys)
            continue
        if "descriptor_kind" not in data:
            continue
        loaded += 1
        descriptor_files.append(name)
        validate_descriptor(project_root, data, issues, name=name, localization_keys=localization_keys)

    if loaded == 0:
        _issue(issues, "no_descriptors", "no d18_ui_audio_descriptor files were found (L42 catalog compiler produces them)")

    code_counts = Counter(issue["code"] for issue in issues)
    errors = sum(1 for issue in issues if issue.get("severity") == "error")
    failed_names: set[str] = set()
    failed_ids: dict[str, str] = {}
    for issue in issues:
        name = issue.get("descriptor", "")
        if not name:
            continue
        failed_names.add(name)
        if issue.get("descriptor_id"):
            failed_ids.setdefault(name, issue["descriptor_id"])
    result: dict[str, Any] = {
        "task": "D18-L29",
        "tool": "tools/d18_ui_audio_descriptor_validator.py",
        "read_only": True,
        "registry_modified": False,
        "status": "PASS" if errors == 0 else "BLOCKED",
        "descriptor_count": loaded,
        "descriptor_files": descriptor_files,
        "descriptor_summary": {
            "total": loaded,
            "passed": loaded - len(failed_names),
            "failed": len(failed_names),
            "failed_descriptor_ids": sorted(set(failed_ids.values())),
        },
        "taxonomy_check": taxonomy_check,
        "error_count": errors,
        "issue_count": len(issues),
        "issue_counts": dict(sorted(code_counts.items())),
        "issues": issues,
    }
    if localization is not None:
        result["localization_files"] = localization_files
        result["localization_key_count"] = len(localization_keys or set())
    return result


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--descriptor-root", type=Path, default=None)
    parser.add_argument("--descriptor", type=Path, action="append", default=[])
    parser.add_argument("--taxonomy", type=Path, default=None, help="D18-L14 taxonomy document or JSON (default: frozen repo document)")
    parser.add_argument("--localization", type=Path, default=None, help="directory or file with localization key sets (e.g. game/Assets/Resources/Localization/D15_A08)")
    parser.add_argument("--output", type=Path, default=None, help="write the machine result JSON to this file")
    return parser


def main(argv: Iterable[str] | None = None) -> int:
    args = _build_parser().parse_args(list(argv) if argv is not None else None)
    project_root = args.project_root.resolve()
    taxonomy_path = args.taxonomy if args.taxonomy is not None else project_root / TAXONOMY_DOCUMENT
    paths = discover_descriptor_files(args.descriptor_root, args.descriptor)
    result = validate_ui_audio_descriptors(
        project_root,
        paths,
        taxonomy_path=taxonomy_path,
        localization=args.localization.resolve() if args.localization is not None else None,
    )
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
