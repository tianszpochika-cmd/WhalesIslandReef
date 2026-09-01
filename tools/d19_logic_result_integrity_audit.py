#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Validate the fixed result protocol for all D19 logic tasks.

This audit checks the evidence envelope only. It does not turn runtime,
Player, visual, or Release work into a logic pass and it does not access the
network.
"""

from __future__ import annotations

import json
from datetime import datetime
from pathlib import Path, PurePosixPath
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
EXPECTED_TASK_IDS = [f"D19-L{number:02d}" for number in range(1, 49)]
VALID_STATUSES = {
    "PLANNED",
    "IN_PROGRESS",
    "LOGIC_PASS",
    "LOGIC_PARTIAL",
    "LOGIC_BLOCKED",
    "RUNTIME_DEFERRED",
}
REQUIRED_FIELDS = (
    "task_id",
    "status",
    "updated_at",
    "summary",
    "changed_files",
    "verification",
    "runtime_deferred",
    "blockers",
    "handoff",
)
LEGACY_FIELDS = {"date", "verifications", "files_written", "handoff_document", "written_files"}


def _safe_relative(value: Any) -> bool:
    if not isinstance(value, str) or not value.strip():
        return False
    normalized = value.replace("\\", "/")
    path = PurePosixPath(normalized)
    if normalized.startswith("/") or any(part in {"", ".", ".."} for part in path.parts):
        return False
    if ":" in path.parts[0]:
        return False
    return True


def _append_error(errors: list[dict[str, str]], task_id: str, code: str, detail: str) -> None:
    errors.append({"task_id": task_id, "code": code, "detail": detail})


def _validate_record(root: Path, task_id: str) -> tuple[dict[str, Any], list[dict[str, str]]]:
    result_path = root / "game" / "tools" / "results" / "D19" / task_id / f"{task_id.replace('-', '_')}_LOGIC_RESULT.json"
    errors: list[dict[str, str]] = []
    record: dict[str, Any] = {"task_id": task_id, "path": str(result_path.relative_to(root)).replace("\\", "/")}
    if not result_path.exists():
        _append_error(errors, task_id, "missing_result", f"缺少结果文件：{record['path']}")
        return record, errors

    try:
        data = json.loads(result_path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        _append_error(errors, task_id, "invalid_json", str(exc))
        return record, errors
    if not isinstance(data, dict):
        _append_error(errors, task_id, "record_not_object", "结果 JSON 顶层必须是对象")
        return record, errors

    record["status"] = data.get("status")
    missing = [field for field in REQUIRED_FIELDS if field not in data]
    if missing:
        _append_error(errors, task_id, "missing_required_fields", ", ".join(missing))
    legacy = sorted(field for field in LEGACY_FIELDS if field in data)
    if legacy:
        _append_error(errors, task_id, "legacy_fields", ", ".join(legacy))
    if data.get("task_id") != task_id:
        _append_error(errors, task_id, "task_id_mismatch", repr(data.get("task_id")))
    if data.get("status") not in VALID_STATUSES:
        _append_error(errors, task_id, "invalid_status", repr(data.get("status")))
    elif data.get("status") != "LOGIC_PASS":
        _append_error(errors, task_id, "non_pass_status", repr(data.get("status")))

    updated_at = data.get("updated_at")
    if not isinstance(updated_at, str) or not updated_at.strip():
        _append_error(errors, task_id, "invalid_updated_at", "updated_at 必须是带时区的 ISO 时间字符串")
    else:
        try:
            parsed = datetime.fromisoformat(updated_at.replace("Z", "+00:00"))
            if parsed.tzinfo is None:
                _append_error(errors, task_id, "updated_at_without_timezone", updated_at)
        except ValueError:
            _append_error(errors, task_id, "invalid_updated_at", updated_at)

    if not isinstance(data.get("summary"), str) or not data["summary"].strip():
        _append_error(errors, task_id, "invalid_summary", "summary 不能为空")

    changed_files = data.get("changed_files")
    if not isinstance(changed_files, list) or not changed_files or not all(_safe_relative(item) for item in changed_files):
        _append_error(errors, task_id, "invalid_changed_files", "changed_files 必须是非空相对路径字符串数组")

    verification = data.get("verification")
    if not isinstance(verification, list) or not verification:
        _append_error(errors, task_id, "invalid_verification", "verification 必须是非空对象数组")
    else:
        for index, item in enumerate(verification):
            if not isinstance(item, dict) or not isinstance(item.get("command"), str) or not item["command"].strip() or not isinstance(item.get("result"), str) or not item["result"].strip():
                _append_error(errors, task_id, "invalid_verification_item", f"verification[{index}] 必须包含非空 command/result")

    for field in ("runtime_deferred", "blockers"):
        values = data.get(field)
        if not isinstance(values, list) or not all(isinstance(item, str) and item.strip() for item in values):
            _append_error(errors, task_id, f"invalid_{field}", f"{field} 必须是字符串数组")

    handoff = data.get("handoff")
    if not _safe_relative(handoff):
        _append_error(errors, task_id, "invalid_handoff", "handoff 必须是安全的相对路径")
    else:
        handoff_path = root / Path(handoff)
        try:
            inside_root = handoff_path.resolve().is_relative_to(root.resolve())
        except AttributeError:
            inside_root = str(handoff_path.resolve()).startswith(str(root.resolve()))
        if not inside_root or not handoff_path.exists():
            _append_error(errors, task_id, "missing_handoff", handoff)

    return record, errors


def audit_d19_logic_results(root: Path = ROOT) -> tuple[bool, dict[str, Any]]:
    records: list[dict[str, Any]] = []
    errors: list[dict[str, str]] = []
    for task_id in EXPECTED_TASK_IDS:
        record, record_errors = _validate_record(root, task_id)
        records.append(record)
        errors.extend(record_errors)

    report = {
        "audit_name": "D19 Logic Result Protocol Integrity Audit",
        "passed": not errors,
        "expected_task_count": len(EXPECTED_TASK_IDS),
        "result_files_found": sum(1 for record in records if "status" in record),
        "logic_pass_count": sum(1 for record in records if record.get("status") == "LOGIC_PASS"),
        "error_count": len(errors),
        "errors": errors,
        "records": records,
    }
    return report["passed"], report


def main() -> int:
    passed, report = audit_d19_logic_results()
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if passed else 1


if __name__ == "__main__":
    raise SystemExit(main())
