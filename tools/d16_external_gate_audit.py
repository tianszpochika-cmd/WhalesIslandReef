"""Conservative audit for D16 external acceptance evidence.

This tool never approves content, changes registry states, edits the release
manifest, or treats an editor/synthetic run as a target-device result. It only
checks whether a reviewer/operator supplied the required evidence envelopes.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
from typing import Any


GATE_FILES = {
    "D16-A01": "d16_a01_target_device.json",
    "D16-A02": "d16_a02_power_cross_platform.json",
    "D16-A03": "d16_a03_device_accessibility.json",
    "D16-A04": "d16_a04_manual_review.json",
    "D16-A05": "d16_a05_final_assets.json",
    "D16-A06": "d16_a06_target_release_privacy.json",
}

COMMON_REQUIRED = (
    "status",
    "evidence_kind",
    "operator",
    "device_id",
    "captured_at",
    "raw_artifacts",
)

GATE_REQUIRED = {
    "D16-A01": (
        "duration_seconds",
        "fps_min",
        "fps_avg",
        "memory_peak_mb",
        "gc_alloc_bytes",
        "same_screen_count",
        "streaming_load_restore_pass",
        "save_restore_pass",
    ),
    "D16-A02": (
        "power_loss_pass",
        "cross_platform_pass",
        "migration_failure_preserves_original",
        "backup_restore_pass",
        "platforms",
    ),
    "D16-A03": (
        "physical_input_pass",
        "focus_pass",
        "accessibility_manual_pass",
        "resolutions",
        "input_devices",
    ),
    "D16-A04": (
        "reviewer_id",
        "decision_log_ref",
        "reviewed_count",
        "pending_count",
    ),
    "D16-A05": (
        "reviewer_id",
        "asset_manifest_ref",
        "final_asset_count",
        "placeholder_count",
        "scene_regression_pass",
    ),
    "D16-A06": (
        "install_update_uninstall_pass",
        "native_crash_review_pass",
        "privacy_manual_review_pass",
        "target_build_ref",
    ),
}


def _read_json(path: Path) -> tuple[dict[str, Any] | None, str | None]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except OSError as error:
        return None, f"read_error:{error}"
    except json.JSONDecodeError as error:
        return None, f"json_error:{error}"
    if not isinstance(value, dict):
        return None, "evidence_root_must_be_object"
    return value, None


def _is_present(value: Any) -> bool:
    if value is None or value is False:
        return False
    if isinstance(value, str):
        return bool(value.strip())
    if isinstance(value, (list, dict)):
        return bool(value)
    return True


def _bool_pass(value: Any) -> bool:
    return value is True or str(value).upper() == "PASS"


def _artifact_path(value: Any) -> str | None:
    if isinstance(value, str):
        return value.strip() or None
    if isinstance(value, dict):
        path = value.get("path")
        return path.strip() if isinstance(path, str) and path.strip() else None
    return None


def _validate_raw_artifacts(evidence_root: Path, artifacts: Any) -> list[str]:
    errors: list[str] = []
    if not isinstance(artifacts, list) or not artifacts:
        return ["raw_artifacts_must_be_non_empty_list"]

    for index, artifact in enumerate(artifacts):
        reference = _artifact_path(artifact)
        if reference is None:
            errors.append(f"raw_artifact_path_missing:{index}")
            continue

        path = Path(reference)
        if not path.is_absolute():
            evidence_root_resolved = evidence_root.resolve()
            path = (evidence_root / path).resolve()
            try:
                path.relative_to(evidence_root_resolved)
            except ValueError:
                errors.append(f"raw_artifact_outside_evidence_root:{reference}")
                continue
        else:
            path = path.resolve()
        if not path.is_file():
            errors.append(f"raw_artifact_missing:{reference}")

    return errors


def _validate_envelope(
    gate: str, evidence: dict[str, Any], evidence_root: Path
) -> list[str]:
    errors: list[str] = []
    for field in COMMON_REQUIRED + GATE_REQUIRED[gate]:
        if not _is_present(evidence.get(field)):
            errors.append(f"missing:{field}")

    if evidence.get("status") != "PASS":
        errors.append("status_must_be_PASS")
    if evidence.get("synthetic") is True:
        errors.append("synthetic_evidence_rejected")
    if evidence.get("editor_only") is True:
        errors.append("editor_only_evidence_rejected")
    if evidence.get("simulated") is True:
        errors.append("simulated_evidence_rejected")

    errors.extend(_validate_raw_artifacts(evidence_root, evidence.get("raw_artifacts")))

    if gate == "D16-A01":
        try:
            if float(evidence.get("duration_seconds", 0)) < 60:
                errors.append("duration_seconds_below_60")
            if float(evidence.get("fps_min", 0)) <= 0 or float(evidence.get("fps_avg", 0)) <= 0:
                errors.append("fps_values_invalid")
        except (TypeError, ValueError):
            errors.append("performance_numbers_invalid")
        if not _bool_pass(evidence.get("streaming_load_restore_pass")):
            errors.append("streaming_load_restore_not_PASS")
        if not _bool_pass(evidence.get("save_restore_pass")):
            errors.append("save_restore_not_PASS")
    elif gate == "D16-A02":
        for field in (
            "power_loss_pass",
            "cross_platform_pass",
            "migration_failure_preserves_original",
            "backup_restore_pass",
        ):
            if not _bool_pass(evidence.get(field)):
                errors.append(f"{field}_not_PASS")
        if not isinstance(evidence.get("platforms"), list) or len(evidence["platforms"]) < 2:
            errors.append("platforms_must_cover_at_least_two_platforms")
    elif gate == "D16-A03":
        for field in ("physical_input_pass", "focus_pass", "accessibility_manual_pass"):
            if not _bool_pass(evidence.get(field)):
                errors.append(f"{field}_not_PASS")
        if not isinstance(evidence.get("resolutions"), list) or not evidence["resolutions"]:
            errors.append("resolutions_must_be_non_empty_list")
        if not isinstance(evidence.get("input_devices"), list) or not evidence["input_devices"]:
            errors.append("input_devices_must_be_non_empty_list")
    elif gate == "D16-A04":
        try:
            if int(evidence.get("pending_count", -1)) != 0:
                errors.append("pending_count_not_zero")
            if int(evidence.get("reviewed_count", 0)) <= 0:
                errors.append("reviewed_count_not_positive")
        except (TypeError, ValueError):
            errors.append("review_counts_invalid")
    elif gate == "D16-A05":
        try:
            if int(evidence.get("placeholder_count", -1)) != 0:
                errors.append("placeholder_count_not_zero")
            if int(evidence.get("final_asset_count", 0)) <= 0:
                errors.append("final_asset_count_not_positive")
        except (TypeError, ValueError):
            errors.append("asset_counts_invalid")
        if not _bool_pass(evidence.get("scene_regression_pass")):
            errors.append("scene_regression_not_PASS")
    elif gate == "D16-A06":
        for field in (
            "install_update_uninstall_pass",
            "native_crash_review_pass",
            "privacy_manual_review_pass",
        ):
            if not _bool_pass(evidence.get(field)):
                errors.append(f"{field}_not_PASS")

    return errors


def audit(project: Path, evidence_root: Path) -> dict[str, Any]:
    gates: dict[str, Any] = {}
    for gate, filename in GATE_FILES.items():
        path = evidence_root / filename
        if not path.is_file():
            gates[gate] = {
                "status": "BLOCKED",
                "evidence_file": str(path),
                "errors": ["missing_evidence_file"],
            }
            continue

        evidence, parse_error = _read_json(path)
        if parse_error or evidence is None:
            gates[gate] = {
                "status": "BLOCKED",
                "evidence_file": str(path),
                "errors": [parse_error or "invalid_evidence"],
            }
            continue

        errors = _validate_envelope(gate, evidence, evidence_root)
        gates[gate] = {
            "status": "PASS" if not errors else "BLOCKED",
            "evidence_file": str(path),
            "errors": errors,
            "operator": evidence.get("operator"),
            "device_id": evidence.get("device_id"),
            "raw_artifact_count": len(evidence.get("raw_artifacts", []))
            if isinstance(evidence.get("raw_artifacts"), list)
            else 0,
        }

    all_external_gates_pass = all(result["status"] == "PASS" for result in gates.values())
    manifest_path = project / "Assets" / "Resources" / "D16ReleaseManifest.json"
    manifest: dict[str, Any] = {}
    if manifest_path.is_file():
        manifest, _ = _read_json(manifest_path)
        manifest = manifest or {}
    steam = manifest.get("steam") if isinstance(manifest, dict) else {}
    steam_authorized = isinstance(steam, dict) and steam.get("upload_authorized") is True

    blockers = [
        gate for gate, result in gates.items() if result["status"] != "PASS"
    ]
    if not steam_authorized:
        blockers.append("steam_upload_not_authorized")

    return {
        "task": "D16-EXTERNAL-GATES",
        "status": "PASS" if not blockers else "BLOCKED",
        "project": str(project),
        "evidence_root": str(evidence_root),
        "gates": gates,
        "all_external_gates_pass": all_external_gates_pass,
        "steam_upload_authorized": steam_authorized,
        "blocking_reasons": blockers,
        "policy": {
            "this_tool_never_approves_content": True,
            "this_tool_never_changes_registry": True,
            "this_tool_never_uploads": True,
        },
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("project", type=Path)
    parser.add_argument("--evidence-root", type=Path, default=Path("tools/results/external"))
    parser.add_argument("--json-out", type=Path)
    args = parser.parse_args()

    result = audit(args.project, args.evidence_root)
    output = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
