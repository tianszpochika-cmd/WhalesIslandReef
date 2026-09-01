#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D20 纯逻辑统一测试入口（D20-L37）。

该运行器只执行离线编译、纯 C# runner、D20 边界审计和证据完整性检查。
它不会启动 Unity、Player、PlayMode、场景或真实输入，也不会运行状态同步。
"""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
import tempfile
import time
from datetime import datetime, timezone, timedelta
from pathlib import Path
from typing import Any, Dict, List, Optional


ROOT = Path(__file__).resolve().parents[1]
RESULT_ROOT = ROOT / "game" / "tools" / "results" / "D20"


def now_iso() -> str:
    tz = timezone(timedelta(hours=8))
    return datetime.now(tz).isoformat(timespec="seconds")


def run_command(command: List[str], cwd: Path = ROOT, timeout: int = 180) -> Dict[str, Any]:
    started = time.monotonic()
    record: Dict[str, Any] = {
        "command": " ".join(command),
        "exit_code": -1,
        "status": "FAIL",
        "duration_ms": 0,
        "stdout": "",
        "stderr": "",
    }
    try:
        completed = subprocess.run(
            command,
            cwd=str(cwd),
            text=True,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            timeout=timeout,
            check=False,
        )
        record["exit_code"] = completed.returncode
        record["status"] = "PASS" if completed.returncode == 0 else "FAIL"
        record["stdout"] = completed.stdout.strip()
        record["stderr"] = completed.stderr.strip()
    except subprocess.TimeoutExpired as exc:
        record["status"] = "TIMEOUT"
        record["stdout"] = (exc.stdout or "").strip() if isinstance(exc.stdout, str) else ""
        record["stderr"] = (exc.stderr or "").strip() if isinstance(exc.stderr, str) else ""
    except Exception as exc:  # pragma: no cover - defensive runner failure
        record["status"] = "ERROR"
        record["stderr"] = str(exc)
    record["duration_ms"] = int((time.monotonic() - started) * 1000)
    # 保留足够定位错误的输出，避免报告被临时 runner 的大量日志淹没。
    record["stdout_tail"] = record["stdout"][-6000:]
    record["stderr_tail"] = record["stderr"][-6000:]
    del record["stdout"]
    del record["stderr"]
    return record


def evidence_check() -> Dict[str, Any]:
    missing: List[str] = []
    bad_status: List[Dict[str, str]] = []
    invalid_json: List[str] = []
    for number in range(1, 36):
        task = f"D20-L{number:02d}"
        result_path = RESULT_ROOT / task / f"D20_L{number:02d}_LOGIC_RESULT.json"
        handoff_path = ROOT / "docs" / "game" / "design" / "common" / "handoffs" / "D20" / f"{task}_LOGIC_HANDOFF.md"
        if not result_path.exists():
            missing.append(result_path.relative_to(ROOT).as_posix())
            continue
        if not handoff_path.exists():
            missing.append(handoff_path.relative_to(ROOT).as_posix())
        try:
            payload = json.loads(result_path.read_text(encoding="utf-8"))
            if payload.get("task_id") != task or payload.get("status") != "LOGIC_PASS":
                bad_status.append({"task_id": task, "status": str(payload.get("status"))})
            required = {"task_id", "status", "updated_at", "summary", "changed_files", "verification", "runtime_deferred", "blockers", "handoff"}
            if not required.issubset(payload):
                bad_status.append({"task_id": task, "status": "missing_protocol_field"})
        except Exception as exc:
            invalid_json.append(f"{result_path.relative_to(ROOT).as_posix()}: {exc}")
    return {
        "status": "PASS" if not missing and not bad_status and not invalid_json else "FAIL",
        "expected_results": 35,
        "missing": missing,
        "bad_status": bad_status,
        "invalid_json": invalid_json,
    }


def build_suite() -> List[Dict[str, Any]]:
    dotnet = "dotnet"
    suites: List[Dict[str, Any]] = [
        {
            "id": "D20_BOUNDARY_SELF_TEST",
            "command": [sys.executable, "tools/d20_logic_boundary_audit.py", "--self-test"],
            "timeout": 30,
        },
        {
            "id": "D20_BOUNDARY_AUDIT",
            "command": [sys.executable, "tools/d20_logic_boundary_audit.py"],
            "timeout": 30,
        },
        {
            "id": "D20_SYSTEMS_BUILD",
            "command": [dotnet, "build", "game/Whalebone.Systems.csproj", "--no-restore", "--nologo", "-warnaserror"],
            "timeout": 180,
        },
        {
            "id": "D20_LOGIC_SMOKE_BUILD",
            "command": [dotnet, "build", "game/Temp/D20LogicSmoke/D20LogicSmoke.csproj", "--no-restore", "--nologo", "-warnaserror"],
            "timeout": 180,
        },
        {
            "id": "D20_LOGIC_SMOKE_RUN",
            "command": [dotnet, "run", "--project", "game/Temp/D20LogicSmoke/D20LogicSmoke.csproj", "--no-build", "--no-restore", "--", "all"],
            "timeout": 180,
        },
    ]
    batch_project = Path(tempfile.gettempdir()) / "d20-l28-l35-batch" / "D20L28L35Batch.csproj"
    if batch_project.exists():
        suites.extend(
            [
                {
                    "id": "D20_L28_L35_BATCH_BUILD",
                    "command": [dotnet, "build", str(batch_project), "--no-restore", "--nologo", "-warnaserror"],
                    "timeout": 180,
                },
                {
                    "id": "D20_L28_L35_BATCH_RUN",
                    "command": [dotnet, "run", "--project", str(batch_project), "--no-build", "--no-restore"],
                    "timeout": 240,
                },
            ]
        )
    else:
        suites.append(
            {
                "id": "D20_L28_L35_BATCH_RUN",
                "command": ["missing", str(batch_project)],
                "timeout": 1,
            }
        )
    return suites


def run_all() -> Dict[str, Any]:
    started = time.monotonic()
    suites: List[Dict[str, Any]] = []
    for definition in build_suite():
        result = run_command(definition["command"], timeout=definition["timeout"])
        result["id"] = definition["id"]
        suites.append(result)
    evidence = evidence_check()
    report = {
        "runner_name": "D20 Unified Pure Logic Test Runner",
        "timestamp": now_iso(),
        "status": "PASS" if all(s["status"] == "PASS" for s in suites) and evidence["status"] == "PASS" else "FAIL",
        "suites": suites,
        "evidence": evidence,
        "runtime_deferred": [
            "Unity Player/PlayMode、场景、Prefab、真实输入、动画、导航、画面、性能和设备测试未执行。",
        ],
        "duration_ms": int((time.monotonic() - started) * 1000),
    }
    return report


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path)
    args = parser.parse_args(argv)
    report = run_all()
    serialized = json.dumps(report, ensure_ascii=False, indent=2) + "\n"
    print(serialized, end="")
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(serialized, encoding="utf-8")
    return 0 if report["status"] == "PASS" else 1


if __name__ == "__main__":
    sys.exit(main())
