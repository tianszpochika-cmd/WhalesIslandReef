"""Read-only readiness gate for D16-S01.

D16-S01 is the full-version long-run story and must not start while any of
the six D16-A01..A06 external gates is unresolved.  This tool combines the
already-generated gate/audit snapshots into one explicit decision.  It never
starts Unity, changes a registry, approves content, uploads anything, or
turns a blocked result into a pass.

Steam upload authorization is reported separately: it is not required to
start the local D16-S01 story, and remains forbidden unless explicitly
authorized by the user.
"""

from __future__ import annotations

import argparse
import json
from datetime import date
from pathlib import Path
from typing import Any


REQUIRED_GATES = [f"D16-A0{number}" for number in range(1, 7)]


def _load_json(path: Path) -> dict[str, Any]:
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise ValueError(f"expected JSON object: {path}")
    return value


def _gate_blockers(external: dict[str, Any]) -> list[str]:
    blockers: list[str] = []
    gates = external.get("gates")
    if not isinstance(gates, dict):
        return ["external_gate_snapshot_missing_gates"]
    for gate_id in REQUIRED_GATES:
        gate = gates.get(gate_id)
        if not isinstance(gate, dict):
            blockers.append(f"{gate_id}:missing_gate_record")
            continue
        if gate.get("status") != "PASS":
            blockers.append(f"{gate_id}:status_{gate.get('status', 'missing')}")
        errors = gate.get("errors", [])
        if errors:
            blockers.append(f"{gate_id}:errors_present")
    return blockers


def evaluate(
    external: dict[str, Any],
    a04: dict[str, Any],
    a05: dict[str, Any],
    a06: dict[str, Any],
    *,
    captured_at: str | None = None,
) -> dict[str, Any]:
    """Evaluate whether D16-S01 may start, without performing the story."""

    blockers = _gate_blockers(external)

    if a04.get("pending_review_count") != 0:
        blockers.append(f"D16-A04:pending_review_count_{a04.get('pending_review_count', 'missing')}")
    if a04.get("rejected_review_count") != 0:
        blockers.append(f"D16-A04:rejected_review_count_{a04.get('rejected_review_count', 'missing')}")
    if a04.get("structural_error_count") != 0:
        blockers.append(f"D16-A04:structural_error_count_{a04.get('structural_error_count', 'missing')}")

    states = a05.get("asset_state_counts")
    if not isinstance(states, dict):
        blockers.append("D16-A05:asset_state_counts_missing")
    elif states.get("placeholder", 0) != 0:
        blockers.append(f"D16-A05:placeholder_count_{states.get('placeholder')}")
    if a05.get("invalid_metadata_count") != 0:
        blockers.append(f"D16-A05:invalid_metadata_count_{a05.get('invalid_metadata_count', 'missing')}")
    if a05.get("declared_path_missing_count") != 0:
        blockers.append(f"D16-A05:declared_path_missing_count_{a05.get('declared_path_missing_count', 'missing')}")

    if a06.get("technical_preparation_status") != "PASS":
        blockers.append(f"D16-A06:technical_preparation_{a06.get('technical_preparation_status', 'missing')}")

    # Steam is deliberately not a start gate for the local S01 story.
    steam_authorized = bool(external.get("steam_upload_authorized", False))
    status = "READY_TO_START" if not blockers else "BLOCKED"
    return {
        "schema_version": 1,
        "task": "D16-S01",
        "status": status,
        "captured_at": captured_at or date.today().isoformat(),
        "required_external_gates": REQUIRED_GATES,
        "external_gate_status": {
            gate_id: external.get("gates", {}).get(gate_id, {}).get("status", "MISSING")
            for gate_id in REQUIRED_GATES
        },
        "blocking_reasons": sorted(set(blockers)),
        "steam_upload_authorized": steam_authorized,
        "steam_policy_note": (
            "Steam upload is not required for local D16-S01 readiness and remains "
            "unexecuted unless explicitly authorized."
        ),
        "story_executed": False,
        "policy": {
            "read_only": True,
            "does_not_start_story": True,
            "does_not_change_registry": True,
            "does_not_approve_content": True,
            "does_not_upload": True,
        },
    }


def evaluate_files(
    external_path: Path,
    a04_path: Path,
    a05_path: Path,
    a06_path: Path,
    *,
    captured_at: str | None = None,
) -> dict[str, Any]:
    return evaluate(
        _load_json(external_path),
        _load_json(a04_path),
        _load_json(a05_path),
        _load_json(a06_path),
        captured_at=captured_at,
    )


def main() -> int:
    parser = argparse.ArgumentParser(description="Evaluate read-only D16-S01 readiness")
    parser.add_argument("--external-audit", type=Path, required=True)
    parser.add_argument("--a04-audit", type=Path, required=True)
    parser.add_argument("--a05-audit", type=Path, required=True)
    parser.add_argument("--a06-audit", type=Path, required=True)
    parser.add_argument("--captured-at", default=date.today().isoformat())
    parser.add_argument("--json-out", type=Path)
    args = parser.parse_args()

    result = evaluate_files(
        args.external_audit,
        args.a04_audit,
        args.a05_audit,
        args.a06_audit,
        captured_at=args.captured_at,
    )
    output = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
