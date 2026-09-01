#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D24-L24：在干净临时工作目录中重复运行工具并比较语义输出。"""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
import tempfile
from pathlib import Path
from typing import Any, Dict, Sequence


AUDIT_SCRIPTS = (
    "d24_accessibility_logic_audit.py",
    "d24_asset_provenance_audit.py",
    "d24_content_catalog_coverage_audit.py",
    "d24_content_id_uniqueness_audit.py",
    "d24_contract_compatibility_audit.py",
    "d24_currency_invariant_audit.py",
    "d24_data_ownership_audit.py",
    "d24_deterministic_seed_audit.py",
    "d24_error_catalog_audit.py",
    "d24_event_integrity_audit.py",
    "d24_historical_debt_audit.py",
    "d24_inventory_invariant_audit.py",
    "d24_localization_key_audit.py",
    "d24_save_migration_audit.py",
    "d24_test_matrix_audit.py",
)


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="D24 tooling reproducibility audit")
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--include-agentcore", action="store_true")
    parser.add_argument("--json-out", type=Path, default=None)
    return parser.parse_args(argv)


def _semantic(value: Any) -> Any:
    """删除编译器耗时/路径输出，保留审计决定和输入证据。"""

    if isinstance(value, dict):
        return {key: _semantic(item) for key, item in sorted(value.items()) if key not in {"stdout", "stderr"}}
    if isinstance(value, list):
        return [_semantic(item) for item in value]
    return value


def _run(script: Path, root: Path, cwd: Path, output: Path) -> Dict[str, Any]:
    command = [sys.executable, str(script), "--root", str(root), "--json-out", str(output)]
    completed = subprocess.run(
        command, cwd=str(cwd), capture_output=True, text=True,
        encoding="utf-8", errors="replace", check=False,
    )
    parsed = None
    parse_error = ""
    try:
        parsed = json.loads(output.read_text(encoding="utf-8"))
    except Exception as error:
        parse_error = str(error)
        try:
            parsed = json.loads(completed.stdout)
        except Exception:
            parsed = None
    return {
        "exit_code": completed.returncode,
        "valid_json": isinstance(parsed, dict),
        "observed_passed": parsed.get("passed") if isinstance(parsed, dict) else None,
        "semantic": _semantic(parsed),
        "parse_error": parse_error,
    }


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    root = args.root.expanduser().resolve()
    scripts = list(AUDIT_SCRIPTS)
    if args.include_agentcore:
        scripts.append("d24_agentcore_build_audit.py")
    checks = []
    with tempfile.TemporaryDirectory(prefix="d24-repro-cwd-") as directory:
        clean_cwd = Path(directory)
        for name in scripts:
            script = root / "tools" / name
            first_output = clean_cwd / (name + ".first.json")
            second_output = clean_cwd / (name + ".second.json")
            first = _run(script, root, clean_cwd, first_output) if script.exists() else {
                "exit_code": 1, "valid_json": False, "observed_passed": None, "semantic": None, "parse_error": "script missing"
            }
            second = _run(script, root, clean_cwd, second_output) if script.exists() else {
                "exit_code": 1, "valid_json": False, "observed_passed": None, "semantic": None, "parse_error": "script missing"
            }
            same = first["valid_json"] and second["valid_json"] and first["semantic"] == second["semantic"]
            checks.append({
                "id": name,
                "status": "PASS" if same else "FAIL",
                "reproducible": same,
                "first_exit_code": first["exit_code"],
                "second_exit_code": second["exit_code"],
                "first_observed_passed": first["observed_passed"],
                "second_observed_passed": second["observed_passed"],
                "valid_json": first["valid_json"] and second["valid_json"],
                "parse_errors": [error for error in (first["parse_error"], second["parse_error"]) if error],
            })
    payload = {
        "schema_version": "d24.tooling-reproducibility.v1",
        "status": "PASS" if all(item["status"] == "PASS" for item in checks) else "FAIL",
        "passed": all(item["status"] == "PASS" for item in checks),
        "checks": checks,
        "clean_workdir": "temporary_directory",
        "runtime_deferred": [
            "仅在临时工作目录重复执行本地 D24 工具；不启动 Unity/Player/PlayMode，不访问网络，不改写仓库状态。"
        ],
    }
    encoded = json.dumps(payload, ensure_ascii=False, indent=2, sort_keys=True)
    print(encoded)
    if args.json_out is not None:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(encoded + "\n", encoding="utf-8")
    return 0 if payload["passed"] else 1


if __name__ == "__main__":
    sys.exit(main())
