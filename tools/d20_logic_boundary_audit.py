#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D20 纯逻辑边界静态审计（D20-L36）。

该脚本只扫描 D20 的纯逻辑生产代码，以及 D20-L28—L35 的纯 C# 测试。
历史 D20-A* Unity 视觉测试不属于本阶段逻辑门禁，不能被混入扫描结果。
规则故意匹配 Unity/网络/时钟 API，而不是普通字段名（例如 D20WeatherInput），
避免把合法的逻辑输入 DTO 误报成 Unity Input API。
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Dict, Iterable, List, Tuple


ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOT = ROOT / "game" / "Assets" / "Scripts" / "Systems" / "D20"
TEST_ROOT = ROOT / "game" / "Assets" / "Tests" / "EditMode" / "D20"

LOGIC_TEST_NAMES = {
    "D20ContentIdIntegrationTests.cs",
    "D20CharacterSaveLoadTests.cs",
    "D20DeterministicBehaviorTests.cs",
    "D20BehaviorInvariantTests.cs",
}


RULES: Tuple[Tuple[str, str, re.Pattern[str]], ...] = (
    (
        "D20-AUDIT-001",
        "Unity/editor/runtime API",
        re.compile(
            r"(?:\busing\s+(?:UnityEngine|UnityEditor)(?:\.[A-Za-z0-9_]+)*\s*;"
            r"|\bUnityEngine\.[A-Za-z0-9_.]+"
            r"|\bUnityEditor\.[A-Za-z0-9_.]+"
            r"|\b(?:GameObject|MonoBehaviour|ScriptableObject|Canvas|NavMesh|Animator|Physics|Shader|"
            r"PlayerPrefs|CharacterController|SceneManager|AssetDatabase|PrefabUtility)\b"
            r"|\bInput\.(?:Get|GetAxis|GetButton|GetKey|GetMouse|mousePosition)\w*"
            r"|\bInputSystem\b)"
        ),
    ),
    (
        "D20-AUDIT-002",
        "network/LLM API",
        re.compile(
            r"(?:\busing\s+System\.Net(?:\.[A-Za-z0-9_]+)*\s*;"
            r"|\b(?:HttpClient|HttpWebRequest|WebRequest|WebClient|UnityWebRequest|WebSocket|Socket)\b"
            r"|\b(?:OpenAI|DeepSeek|Qwen|Ollama|LLMClient|ChatCompletion)\b)"
        ),
    ),
    (
        "D20-AUDIT-003",
        "system clock/non-deterministic random",
        re.compile(
            r"(?:\bDateTime\.(?:Now|UtcNow|Today)\b"
            r"|\bStopwatch\b"
            r"|\bTime\.(?:time|deltaTime|unscaledTime|realtimeSinceStartup)\b"
            r"|\bGuid\.NewGuid\s*\("
            r"|\b(?:System\.)?Random\s*\(\s*\)"
            r"|\bnew\s+(?:System\.)?Random\s*\(\s*\))"
        ),
    ),
    (
        "D20-AUDIT-004",
        "foreign owner direct mutation",
        re.compile(
            r"(?:\b(?:GameState|PlayerState|D3TradeState|D4ShipyardSystem|D9PortShipPreparationSystem)"
            r"\s*\.\s*(?:Instance|_[A-Za-z0-9_]+)\b)"
        ),
    ),
)


def _mask_comments_and_strings(line: str) -> str:
    """Mask comments and quoted strings while retaining code tokens."""

    line = re.sub(r"//.*$", "", line)
    line = re.sub(r'@"(?:""|[^"])*"', '""', line)
    line = re.sub(r'"(?:\\.|[^"\\])*"', '""', line)
    line = re.sub(r"'(?:\\.|[^'\\])*'", "''", line)
    return line


def _iter_logic_files() -> Iterable[Path]:
    if SOURCE_ROOT.exists():
        yield from sorted(SOURCE_ROOT.rglob("*.cs"))
    if TEST_ROOT.exists():
        for name in sorted(LOGIC_TEST_NAMES):
            path = TEST_ROOT / name
            if path.exists():
                yield path


def _relative(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def audit_file(path: Path) -> List[Dict[str, Any]]:
    violations: List[Dict[str, Any]] = []
    try:
        lines = path.read_text(encoding="utf-8", errors="strict").splitlines()
    except Exception as exc:  # pragma: no cover - defensive path
        return [
            {
                "rule_id": "D20-AUDIT-ERR",
                "rule_name": "file-read-error",
                "severity": "P0",
                "file": _relative(path),
                "line": 0,
                "matched": str(exc),
                "line_content": "",
            }
        ]

    for line_number, raw in enumerate(lines, start=1):
        code = _mask_comments_and_strings(raw)
        if not code.strip():
            continue
        for rule_id, rule_name, pattern in RULES:
            match = pattern.search(code)
            if match:
                violations.append(
                    {
                        "rule_id": rule_id,
                        "rule_name": rule_name,
                        "severity": "P0",
                        "file": _relative(path),
                        "line": line_number,
                        "matched": match.group(0),
                        "line_content": raw.strip(),
                    }
                )
    return violations


def run_audit() -> Dict[str, Any]:
    files = list(_iter_logic_files())
    violations: List[Dict[str, Any]] = []
    for path in files:
        violations.extend(audit_file(path))
    return {
        "audit_name": "D20 Logic Boundary Static Audit",
        "status": "PASS" if not violations and files else "LOGIC_FAIL",
        "passed": not violations and bool(files),
        "scanned_files_count": len(files),
        "scanned_files": [_relative(path) for path in files],
        "violations_count": len(violations),
        "violations": violations,
        "scope": {
            "production": "game/Assets/Scripts/Systems/D20/**/*.cs",
            "tests": sorted(
                f"game/Assets/Tests/EditMode/D20/{name}" for name in LOGIC_TEST_NAMES
            ),
            "excluded": "D20-A* Unity visual/presentation tests",
        },
    }


def self_test() -> Dict[str, Any]:
    valid = """
    using System;
    public sealed class ValidInput { public string Input; public int WorldDay; }
    """
    invalid = """
    using UnityEngine;
    public sealed class InvalidInput { public void Tick() { Input.GetKey(\"x\"); } }
    """
    valid_violations = _audit_text("valid_fixture.cs", valid)
    invalid_violations = _audit_text("invalid_fixture.cs", invalid)
    passed = not valid_violations and any(
        v["rule_id"] == "D20-AUDIT-001" for v in invalid_violations
    )
    return {
        "self_test": "PASS" if passed else "FAIL",
        "valid_fixture_violations": valid_violations,
        "invalid_fixture_violations": invalid_violations,
    }


def _audit_text(name: str, content: str) -> List[Dict[str, Any]]:
    violations: List[Dict[str, Any]] = []
    for line_number, raw in enumerate(content.splitlines(), start=1):
        code = _mask_comments_and_strings(raw)
        for rule_id, rule_name, pattern in RULES:
            match = pattern.search(code)
            if match:
                violations.append(
                    {
                        "rule_id": rule_id,
                        "rule_name": rule_name,
                        "severity": "P0",
                        "file": name,
                        "line": line_number,
                        "matched": match.group(0),
                        "line_content": raw.strip(),
                    }
                )
    return violations


def main(argv: List[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--self-test", action="store_true")
    parser.add_argument("--output", type=Path)
    args = parser.parse_args(argv)

    report: Dict[str, Any]
    if args.self_test:
        report = self_test()
    else:
        report = run_audit()

    serialized = json.dumps(report, ensure_ascii=False, indent=2) + "\n"
    print(serialized, end="")
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(serialized, encoding="utf-8")
    return 0 if report.get("status", report.get("self_test")) == "PASS" else 1


if __name__ == "__main__":
    sys.exit(main())
