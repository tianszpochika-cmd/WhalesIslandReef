"""Read-only static audit for the D29/D30 interaction button matrix."""

from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
MATRIX_PATH = ROOT / "docs/game/design/common/D29_D30_INTERACTION_BUTTON_MATRIX.json"
FORBIDDEN = ("HttpClient", "UnityWebRequest", "System.Net.Http", "using System.Net")


def load_matrix() -> dict[str, Any]:
    return json.loads(MATRIX_PATH.read_text(encoding="utf-8"))


def _audit_entry(entry: dict[str, Any], errors: list[str], *, require_id: bool) -> None:
    source_value = entry.get("source")
    callback = entry.get("callback")
    markers = entry.get("feedback_markers")
    if require_id and not entry.get("id"):
        errors.append("button entry has no id")
    if not isinstance(source_value, str) or not isinstance(callback, str):
        errors.append(f"{entry.get('id', entry.get('surface', 'unknown'))}: source/callback missing")
        return
    if not isinstance(markers, list) or not markers:
        errors.append(f"{entry.get('id', callback)}: feedback_markers missing")
    source_path = ROOT / source_value
    if not source_path.is_file():
        errors.append(f"{entry.get('id', callback)}: source missing: {source_value}")
        return
    source = source_path.read_text(encoding="utf-8")
    method = callback.rsplit(".", 1)[-1]
    if not re.search(r"\b(?:private|public|protected|internal)?\s*(?:static\s+)?[A-Za-z0-9_<>,.?\[\]]+\s+" + re.escape(method) + r"\s*\(", source):
        errors.append(f"{entry.get('id', callback)}: callback method missing: {method}")
    if not any(str(marker) in source for marker in markers):
        errors.append(f"{entry.get('id', callback)}: no declared feedback marker found")
    for forbidden in FORBIDDEN:
        if forbidden in source:
            errors.append(f"{entry.get('id', callback)}: forbidden network token: {forbidden}")


def audit() -> dict[str, Any]:
    matrix = load_matrix()
    buttons = matrix.get("buttons", [])
    keyboard_only = matrix.get("keyboard_only_controls", [])
    errors: list[str] = []
    ids = [entry.get("id") for entry in buttons]
    if len(ids) != len(set(ids)):
        errors.append("button ids are not unique")
    if len(buttons) < 29:
        errors.append(f"button matrix unexpectedly small: {len(buttons)}")
    for entry in buttons:
        _audit_entry(entry, errors, require_id=True)
    for entry in keyboard_only:
        _audit_entry(entry, errors, require_id=False)
    return {
        "task_id": "D29-S05",
        "audit": "D29_D30_INTERACTION_BUTTON_MATRIX",
        "status": "PASS" if not errors else "PARTIAL",
        "button_count": len(buttons),
        "keyboard_only_count": len(keyboard_only),
        "errors": errors,
        "source": "docs/game/design/common/D29_D30_INTERACTION_BUTTON_MATRIX.json",
    }


if __name__ == "__main__":
    print(json.dumps(audit(), ensure_ascii=False, indent=2))
