#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D24-L23 AgentCore 独立构建/自测审计。

仅调用 AgentCore 的本地 .NET 项目和离线 ``--d9-check`` 分支；不会触发
Ollama、云端模型、Unity Player 或任何网络请求。
"""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path
from typing import Dict, Sequence


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="D24-L23 AgentCore build audit")
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--configuration", choices=("Debug", "Release"), default="Debug")
    parser.add_argument("--json-out", type=Path, default=None)
    return parser.parse_args(argv)


def run(command: Sequence[str], cwd: Path) -> Dict[str, object]:
    completed = subprocess.run(
        list(command), cwd=str(cwd), capture_output=True, text=True,
        encoding="utf-8", errors="replace", check=False,
    )
    return {
        "command": " ".join(command),
        "exit_code": completed.returncode,
        "stdout": completed.stdout[-12000:],
        "stderr": completed.stderr[-12000:],
    }


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    root = args.root.expanduser().resolve()
    project = root / "game" / "AgentCore" / "AgentCore.csproj"
    checks = []
    if not project.exists():
        checks.append({"id": "project_exists", "status": "FAIL", "path": str(project)})
        payload = {"schema_version": "d24.agentcore-build.v1", "status": "FAIL", "passed": False, "checks": checks}
        return _emit(payload, args.json_out)

    build = run(("dotnet", "build", str(project), "--configuration", args.configuration, "--no-restore", "--nologo", "-warnaserror"), root)
    checks.append({"id": "agentcore_build", "status": "PASS" if build["exit_code"] == 0 else "FAIL", "command": build["command"], "exit_code": build["exit_code"]})
    # Bootstrap's default interactive branch can probe external services. The
    # D9 check is the repository-provided offline executable branch instead.
    smoke = run(("dotnet", "run", "--project", str(project), "--configuration", args.configuration, "--no-build", "--no-restore", "--nologo", "--", "--d9-check"), root) if build["exit_code"] == 0 else {"command": "skipped", "exit_code": 1, "stdout": "", "stderr": "build failed"}
    checks.append({"id": "offline_d9_self_test", "status": "PASS" if smoke["exit_code"] == 0 else "FAIL", "command": smoke["command"], "exit_code": smoke["exit_code"]})
    payload = {
        "schema_version": "d24.agentcore-build.v1",
        "status": "PASS" if all(item["status"] == "PASS" for item in checks) else "FAIL",
        "passed": all(item["status"] == "PASS" for item in checks),
        "checks": checks,
        "build": build,
        "smoke": smoke,
        "runtime_deferred": [
            "仅执行 AgentCore .NET 编译和离线 --d9-check；交互式模型、Ollama/云端、Unity Player/PlayMode、网络和设备未执行。"
        ],
    }
    return _emit(payload, args.json_out)


def _emit(payload: Dict[str, object], output: Path | None) -> int:
    encoded = json.dumps(payload, ensure_ascii=False, indent=2, sort_keys=True)
    print(encoded)
    if output is not None:
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(encoded + "\n", encoding="utf-8")
    return 0 if payload.get("passed") else 1


if __name__ == "__main__":
    sys.exit(main())
