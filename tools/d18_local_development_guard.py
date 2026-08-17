"""D18-L36 LocalDevelopmentReady guard.

Defines and verifies the minimal logic conditions for a D18 asset record to be
``LocalDevelopmentReady``.  LocalDevelopmentReady is a **logic conclusion only**:
it is NOT Unity import, NOT a Prefab, NOT a runtime consumer, NOT performance
validation and NOT ReleaseApproved.  The tool never writes approvals and its
``approved_count`` is always 0.

Conditions (C1—C10, each must pass for READY; the tool reports every missing
condition):

- C1 source complete          source_id/source_url/original_filename/download_date
- C2 license usable           known license (L04 vocabulary), not NC/ND/SA,
                              evidence file present on disk
- C3 provenance passed        external verdict from D18-L05
- C4 hash verified            sha256 64-hex + external file-match verdict from D18-L06
- C5 rights explicit          all four rights explicitly true (L07 semantics)
- C6 category in taxonomy     category hits the frozen L11—L14 key vocabulary
- C7 content ids valid        non-empty, snake_case, no candidate_ prefix,
                              + external existence verdict from D18-L15
- C8 resource keys valid      unique, non-path-like keys + D18-L16 verdict
- C9 descriptors complete     mesh/material/animation/ui_audio all complete (L25—L29)
- C10 state chain valid       state in the frozen 7-enum, not Rejected/Revoked,
                              never RuntimeApproved/ReleaseApproved in the logic
                              phase, + external D18-L35 chain verdict

Anti-fabrication rule C11 (informational, never satisfies any condition):
historical visual/player artifacts (screenshots, player logs, RERUN results)
can never make a record ready; if they are the only evidence the record is
NOT_READY with all real conditions missing.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Iterable

from d18_license_normalizer import canonicalize_license_id

TASK = "D18-L36"
TOOL = "tools/d18_local_development_guard.py"

STATES = {
    "Candidate", "IntakeEligible", "RuntimeRequested",
    "RuntimeApproved", "ReleaseApproved", "Rejected", "Revoked",
}
LOGIC_PHASE_FORBIDDEN_STATES = {"RuntimeApproved", "ReleaseApproved"}
TERMINAL_STATES = {"Rejected", "Revoked"}

# Frozen L11—L14 taxonomy key vocabulary (2026-08-16).
TAXONOMY_KEYS = {
    # L12 characters
    "player_visual", "npc_visual", "hair", "outfit", "accessory", "role_variant",
    "body", "injury_overlay", "shared_body",
    # L11 ships/port
    "ship", "hull_component", "port_service", "dock", "berth", "building", "prop",
    # L13 ecology
    "terrestrial_animal", "bird", "aquatic_animal", "rare_species", "pet",
    "vegetation", "terrain", "water_body",
    # L14 ui/audio/material
    "ui_icon", "input_prompt", "audio_cue", "music", "material", "texture",
    "hdri", "weather_presentation", "font",
}

SOURCE_ID_RE = re.compile(r"^source_[a-z0-9_]+$")
SOURCE_URL_RE = re.compile(r"^https://[^\s]+$")
DATE_RE = re.compile(r"^\d{4}-\d{2}-\d{2}$")
SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")
CONTENT_ID_RE = re.compile(r"^[a-z0-9_]+$")
RESOURCE_KEY_RE = re.compile(r"^[a-z0-9_:][a-z0-9_:.-]*$")
IMAGE_EVIDENCE_RE = re.compile(r"\.(?:png|jpe?g|bmp|gif)$", re.IGNORECASE)
PLAYER_LOG_EVIDENCE_RE = re.compile(r"player.*\.log$|runtime_route|rerun", re.IGNORECASE)
VISUAL_ARTIFACT_MARKERS = (
    "screenshot", "fixed_lens", "performance_sample", "player.log", "route_probe",
)
CANDIDATE_PREFIX = "candidate_"

CONDITION_NAMES = ["C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10"]
NOT_READY_DEFINITION = [
    "unity_import", "prefab", "runtime_consumers", "performance", "release_approved",
]


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _usable_license(license_id: Any) -> tuple[bool, str]:
    """Known license via L04 vocabulary that is not NC/ND/SA restricted."""
    canonical, lookup = canonicalize_license_id(license_id)
    if lookup != "ok" or canonical is None:
        return False, f"license not usable: lookup={lookup!r}"
    canonical_upper = canonical.upper()
    for marker in ("-NC", "-ND", "-SA"):
        if marker in canonical_upper:
            return False, f"license {canonical!r} carries {marker} restriction; cannot be confirmed for game distribution"
    return True, f"license {canonical!r} usable"


def _condition(passed: bool, detail: str) -> dict[str, Any]:
    return {"pass": bool(passed), "detail": detail}


def _is_visual_artifact_path(path: str) -> bool:
    return bool(IMAGE_EVIDENCE_RE.search(path)) or bool(PLAYER_LOG_EVIDENCE_RE.search(path)) or any(
        marker in path.casefold() for marker in VISUAL_ARTIFACT_MARKERS
    )


def _evaluate_record(record: Any, index: int, evidence: dict[str, Any], project_root: Path) -> dict[str, Any]:
    if not isinstance(record, dict):
        return {
            "canonical_asset_id": f"record[{index}]", "verdict": "NOT_READY",
            "conditions": {}, "missing_conditions": ["C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10"],
            "notes": ["record must be an object"],
        }
    record_id = _text(record.get("canonical_asset_id")) or f"record[{index}]"
    ev = evidence.get(record_id, {}) if isinstance(evidence, dict) else {}
    notes: list[str] = []
    conditions: dict[str, dict[str, Any]] = {}

    # C1 source complete
    source_id = _text(record.get("source_id"))
    source_url = _text(record.get("source_url"))
    original_filename = _text(record.get("original_filename"))
    download_date = _text(record.get("download_date") or "")
    if not SOURCE_ID_RE.fullmatch(source_id):
        conditions["C1"] = _condition(False, f"source_id {source_id!r} must match source_[a-z0-9_]+")
    elif not SOURCE_URL_RE.fullmatch(source_url):
        conditions["C1"] = _condition(False, f"source_url {source_url!r} must be an https URL")
    elif not original_filename:
        conditions["C1"] = _condition(False, "original_filename is empty")
    elif not DATE_RE.fullmatch(download_date):
        conditions["C1"] = _condition(False, f"download_date {download_date!r} must be a real ISO date or the record is not downloaded")
    else:
        conditions["C1"] = _condition(True, "source record complete")

    # C2 license usable
    license_obj = record.get("license")
    if not isinstance(license_obj, dict):
        conditions["C2"] = _condition(False, "license object missing")
    else:
        license_id = _text(license_obj.get("license_id"))
        license_url = _text(license_obj.get("license_url"))
        evidence_path = _text(license_obj.get("license_evidence_path"))
        attribution = license_obj.get("attribution_required")
        usable, reason = _usable_license(license_id)
        if not license_url.startswith("https://"):
            conditions["C2"] = _condition(False, "license_url must be https")
        elif not isinstance(attribution, bool):
            conditions["C2"] = _condition(False, "attribution_required must be an explicit boolean")
        elif not usable:
            conditions["C2"] = _condition(False, reason)
        elif not evidence_path or not (project_root / evidence_path).is_file():
            conditions["C2"] = _condition(False, f"license evidence file missing or not a repo-relative file: {evidence_path!r}")
        else:
            conditions["C2"] = _condition(True, f"{reason}; attribution_required={attribution}")

    # C3 provenance (external L05 verdict)
    c3 = ev.get("c3_provenance")
    if isinstance(c3, dict) and c3.get("status") == "PASS":
        conditions["C3"] = _condition(True, "provenance validated by D18-L05")
    elif isinstance(c3, dict) and c3.get("status") == "PENDING":
        conditions["C3"] = _condition(False, "provenance pending (D18-L05); evidence incomplete")
    else:
        conditions["C3"] = _condition(False, f"provenance verdict missing or failed: {c3!r}")

    # C4 hash verified (self format + external L06 verdict)
    sha256 = record.get("sha256")
    file_ok = ev.get("c4_file_matches")
    if not isinstance(sha256, str) or not SHA256_RE.fullmatch(sha256):
        conditions["C4"] = _condition(False, "sha256 missing or not 64 hex characters")
    elif file_ok is not True:
        conditions["C4"] = _condition(False, f"file-match verdict (D18-L06) missing or false: {file_ok!r}")
    else:
        conditions["C4"] = _condition(True, "sha256 format valid and file content verified by D18-L06")

    # C5 rights explicit
    rights = record.get("rights")
    if not isinstance(rights, dict):
        conditions["C5"] = _condition(False, "rights object missing")
    else:
        missing = [k for k in ("commercial_use", "modification_allowed", "repository_storage", "distribution_with_game") if rights.get(k) is not True]
        if missing:
            conditions["C5"] = _condition(False, f"rights not all explicitly true: missing/false {missing}")
        else:
            conditions["C5"] = _condition(True, "all four rights explicitly true")

    # C6 category in taxonomy
    category = _text(record.get("category"))
    if category not in TAXONOMY_KEYS:
        conditions["C6"] = _condition(False, f"category {category!r} not in the frozen L11—L14 taxonomy keys")
    else:
        conditions["C6"] = _condition(True, f"category {category!r} hits the frozen taxonomy")

    # C7 content ids valid
    content_ids = record.get("content_ids")
    if not isinstance(content_ids, list) or not content_ids:
        conditions["C7"] = _condition(False, "content_ids missing or empty")
    else:
        bad_format = [c for c in content_ids if not isinstance(c, str) or not CONTENT_ID_RE.fullmatch(c)]
        leaked = [c for c in content_ids if isinstance(c, str) and c.startswith(CANDIDATE_PREFIX)]
        exists = ev.get("c7_content_ids_exist")
        if bad_format:
            conditions["C7"] = _condition(False, f"content ids must be lower snake_case: {bad_format}")
        elif leaked:
            conditions["C7"] = _condition(False, f"content ids must not be candidate-derived keys: {leaked}")
        elif exists is not True:
            conditions["C7"] = _condition(False, f"content-id existence verdict (D18-L15) missing or false: {exists!r}")
        else:
            conditions["C7"] = _condition(True, "content ids valid and exist (D18-L15)")

    # C8 resource keys valid
    resource_keys = ev.get("resource_keys")
    if not isinstance(resource_keys, list) or not resource_keys:
        conditions["C8"] = _condition(False, "resource_keys evidence missing or empty")
    else:
        bad = [k for k in resource_keys if not isinstance(k, str) or not RESOURCE_KEY_RE.fullmatch(k)]
        duplicated = sorted({k for k in resource_keys if resource_keys.count(k) > 1}) if all(isinstance(k, str) for k in resource_keys) else []
        keys_ok = ev.get("c8_resource_keys_ok")
        if bad:
            conditions["C8"] = _condition(False, f"resource keys must not be paths/extensions/display names: {bad}")
        elif duplicated:
            conditions["C8"] = _condition(False, f"resource keys must be unique: {duplicated}")
        elif keys_ok is not True:
            conditions["C8"] = _condition(False, f"resource-key verdict (D18-L16) missing or false: {keys_ok!r}")
        else:
            conditions["C8"] = _condition(True, "resource keys unique and stable (D18-L16)")

    # C9 descriptors complete
    descriptors = ev.get("descriptors")
    if not isinstance(descriptors, dict) or not descriptors:
        conditions["C9"] = _condition(False, "descriptors evidence missing (mesh/material/animation/ui_audio)")
    else:
        missing_descriptors = [k for k in ("mesh", "material", "animation", "ui_audio") if descriptors.get(k) is not True]
        if missing_descriptors:
            conditions["C9"] = _condition(False, f"descriptor completeness missing: {missing_descriptors}")
        else:
            conditions["C9"] = _condition(True, "descriptors complete (L25—L29)")

    # C10 state chain valid
    state = _text(record.get("state"))
    chain_ok = ev.get("c10_state_chain_valid")
    if state not in STATES:
        conditions["C10"] = _condition(False, f"state {state!r} not in the frozen seven-state enum")
    elif state in TERMINAL_STATES:
        conditions["C10"] = _condition(False, f"state {state} is terminal; cannot be ready")
    elif state in LOGIC_PHASE_FORBIDDEN_STATES:
        conditions["C10"] = _condition(False, f"state {state} cannot be granted in the logic phase; zero holders")
    elif chain_ok is not True:
        conditions["C10"] = _condition(False, f"state-chain verdict (D18-L35) missing or invalid: {chain_ok!r}")
    else:
        conditions["C10"] = _condition(True, f"state {state} valid and chain verified (D18-L35)")

    # C11 anti-fabrication (informational only)
    visual_paths = [p for p in record.get("evidence_paths", []) if isinstance(p, str) and _is_visual_artifact_path(p)]
    if visual_paths:
        notes.append(f"historical visual/player artifacts present ({len(visual_paths)}): they never satisfy any condition and cannot cause Ready: {visual_paths[:3]}")

    missing = [name for name in CONDITION_NAMES if not conditions[name]["pass"]]
    verdict = "READY" if not missing else "NOT_READY"
    if verdict == "NOT_READY":
        notes.append("missing conditions must be closed with logic evidence before readiness can be claimed")
    return {
        "canonical_asset_id": record_id,
        "verdict": verdict,
        "conditions": conditions,
        "missing_conditions": missing,
        "notes": notes,
    }


def evaluate_records(records: Iterable[Any], evidence: dict[str, Any], project_root: Path) -> dict[str, Any]:
    """Evaluate all records; deterministic JSON-compatible result."""
    items = [_evaluate_record(record, index, evidence, project_root) for index, record in enumerate(records)]
    if not items:
        items = [{
            "canonical_asset_id": "<none>", "verdict": "NOT_READY",
            "conditions": {}, "missing_conditions": CONDITION_NAMES,
            "notes": ["no records were supplied"],
        }]
    ready = sum(1 for item in items if item["verdict"] == "READY")
    not_ready = len(items) - ready
    return {
        "task": TASK,
        "tool": TOOL,
        "read_only": True,
        "approved_count": 0,
        "local_development_ready_is_not": NOT_READY_DEFINITION,
        "status": "READY" if not_ready == 0 else "NOT_READY",
        "record_count": len(items),
        "ready_count": ready,
        "not_ready_count": not_ready,
        "records": items,
    }


def _load_input(path: Path) -> tuple[dict[str, Any] | None, list[dict[str, Any]], str]:
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError:
        return None, [{"code": "input_not_found", "severity": "error", "message": f"input file does not exist: {path}"}], "input_not_found"
    except json.JSONDecodeError as exc:
        return None, [{"code": "input_json_error", "severity": "error", "message": f"line {exc.lineno}: {exc.msg} in {path.name}"}], "input_json_error"
    if not isinstance(data, dict):
        return None, [{"code": "input_not_supported", "severity": "error", "message": "input JSON must be an object with records/condition_evidence"}], "input_not_supported"
    return data, [], ""


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, required=True, help="guard input JSON: {records: [...], condition_evidence: {...}}")
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    data, input_issues, _ = _load_input(args.input)
    if data is None:
        result: dict[str, Any] = {
            "task": TASK, "tool": TOOL, "read_only": True, "approved_count": 0,
            "local_development_ready_is_not": NOT_READY_DEFINITION,
            "status": "BLOCKED", "record_count": 0, "ready_count": 0, "not_ready_count": 0,
            "records": [], "input_issues": input_issues,
        }
    else:
        result = evaluate_records(data.get("records") or [], data.get("condition_evidence") or {}, args.project_root.resolve())
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    exit_codes = {"READY": 0, "NOT_READY": 1, "BLOCKED": 2}
    return exit_codes.get(result["status"], 3)


if __name__ == "__main__":
    raise SystemExit(main())
