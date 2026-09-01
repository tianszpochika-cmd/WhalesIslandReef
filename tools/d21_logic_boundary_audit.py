#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D21 纯逻辑边界静态审计。

扫描 D21 Systems 与 EditMode tests，拦截 Unity/运行时表现、网络/LLM、真实
时钟、不可控随机和跨 owner 私写。该工具只读源码并将机器可读结果写到 stdout，
不会生成场景、Prefab、二进制资源或修改项目状态。
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Dict, Iterable, List, Tuple


P0_RULES = (
    {
        "id": "D21-AUDIT-001",
        "name": "Forbidden Unity/runtime reference",
        "pattern": re.compile(
            r"\b(?:UnityEngine(?:\.[A-Za-z_][A-Za-z0-9_]*)*|MonoBehaviour|"
            r"ScriptableObject|GameObject|Transform|Rigidbody|Collider|Physics|"
            r"NavMesh|Canvas|RectTransform|Shader|SceneManager|Prefab|"
            r"Animator|PlayerConnection|Instantiate|FindObjectOfType|"
            r"Resources\.Load|Application\.persistentDataPath|"
            r"Input\.(?:Get[A-Za-z0-9_]*|mousePosition|touches|anyKey[A-Za-z]*|"
            r"acceleration|compass|gyro|location|deviceOrientation|simulateMouseWithTouches)|"
            r"Time\.(?:time|deltaTime))\b"
        ),
        "description": "D21 Systems/tests must not use Unity scene, physics, UI, input, animation or frame-time APIs.",
    },
    {
        "id": "D21-AUDIT-002",
        "name": "Forbidden network/LLM reference",
        "pattern": re.compile(
            r"\b(?:System\.Net(?:\.[A-Za-z_][A-Za-z0-9_]*)*|HttpClient|"
            r"HttpWebRequest|WebRequest|UnityWebRequest|WebSocket|Socket|TcpClient|"
            r"UdpClient|OpenAI|DeepSeek|Qwen|Ollama|LLM|ChatCompletion)\b"
        ),
        "description": "D21 pure logic must not perform network access or call an LLM/AI service.",
    },
    {
        "id": "D21-AUDIT-003",
        "name": "Forbidden wall-clock/non-deterministic random",
        "pattern": re.compile(
            r"\b(?:DateTime\.(?:Now|UtcNow)|DateTimeOffset\.(?:Now|UtcNow)|"
            r"Stopwatch|new\s+Random\s*\(|Random\.|Guid\.NewGuid|"
            r"Environment\.TickCount|Thread\.Sleep)\b"
        ),
        "description": "D21 must use explicit saved world time and stable seeds, never wall clock or uncontrolled randomness.",
    },
    {
        "id": "D21-AUDIT-004",
        "name": "Forbidden foreign owner mutation",
        "pattern": re.compile(
            r"\b(?:GameState\.Instance|PlayerState\.Instance|PlayerPrefs|"
            r"D[2349]\w*\._[A-Za-z_]\w*|D19\w*\._[A-Za-z_]\w*|"
            r"D20\w*\._[A-Za-z_]\w*)\b"
        ),
        "description": "D21 may consume public snapshots/results but must not write foreign private state or global presentation state.",
    },
)

BINARY_SUFFIXES = {".unity", ".prefab", ".asset", ".anim", ".controller", ".fbx"}


def _code_lines(content: str) -> Iterable[Tuple[int, str]]:
    """Yield source lines with comments and quoted literal contents removed."""

    in_block_comment = False
    string_quote = ""
    verbatim_string = False
    escaped = False
    for line_number, raw_line in enumerate(content.splitlines(), start=1):
        result: List[str] = []
        index = 0
        while index < len(raw_line):
            if string_quote:
                char = raw_line[index]
                if verbatim_string:
                    if char == '"' and index + 1 < len(raw_line) and raw_line[index + 1] == '"':
                        index += 2
                        continue
                    if char == string_quote:
                        string_quote = ""
                        verbatim_string = False
                    index += 1
                    continue
                if escaped:
                    escaped = False
                elif char == "\\":
                    escaped = True
                elif char == string_quote:
                    string_quote = ""
                index += 1
                continue
            if in_block_comment:
                end = raw_line.find("*/", index)
                if end < 0:
                    index = len(raw_line)
                    break
                in_block_comment = False
                index = end + 2
                continue
            if raw_line.startswith("/*", index):
                in_block_comment = True
                index += 2
                continue
            if raw_line.startswith("//", index):
                break
            char = raw_line[index]
            if char in ('"', "'"):
                string_quote = char
                verbatim_string = (
                    char == '"'
                    and ((index > 0 and raw_line[index - 1] == "@")
                         or (index > 1 and raw_line[index - 2:index] == "@$"))
                )
                escaped = False
                index += 1
                continue
            result.append(char)
            index += 1
        cleaned = "".join(result).strip()
        if cleaned:
            yield line_number, cleaned


def audit_file_content(file_path: str, content: str) -> List[Dict[str, object]]:
    violations: List[Dict[str, object]] = []
    if not file_path.lower().endswith(".cs"):
        return violations
    for line_number, line in _code_lines(content):
        for rule in P0_RULES:
            match = rule["pattern"].search(line)
            if match:
                violations.append(
                    {
                        "rule_id": rule["id"],
                        "rule_name": rule["name"],
                        "severity": "P0",
                        "file": file_path.replace("\\", "/"),
                        "line": line_number,
                        "matched": match.group(0),
                        "line_content": line,
                        "description": rule["description"],
                    }
                )
    return violations


def _relative(path: Path, base_dir: Path) -> str:
    return path.relative_to(base_dir).as_posix()


def audit_directory(target_dir: Path, base_dir: Path) -> Tuple[List[str], List[Dict[str, object]]]:
    scanned_files: List[str] = []
    violations: List[Dict[str, object]] = []
    if not target_dir.exists():
        violations.append(
            {
                "rule_id": "D21-AUDIT-ERR",
                "rule_name": "Missing audit target",
                "severity": "P0",
                "file": _relative(target_dir, base_dir),
                "line": 0,
                "matched": str(target_dir),
                "line_content": "",
                "description": "Required D21 source/test directory does not exist.",
            }
        )
        return scanned_files, violations

    for path in sorted(target_dir.rglob("*")):
        if not path.is_file():
            continue
        relative_path = _relative(path, base_dir)
        if path.suffix.lower() in BINARY_SUFFIXES:
            violations.append(
                {
                    "rule_id": "D21-AUDIT-005",
                    "rule_name": "Forbidden runtime binary asset",
                    "severity": "P0",
                    "file": relative_path,
                    "line": 0,
                    "matched": path.suffix.lower(),
                    "line_content": "",
                    "description": "D21 pure-logic directories must not own scenes, Prefabs, animation or model assets.",
                }
            )
        if path.suffix.lower() != ".cs":
            continue
        scanned_files.append(relative_path)
        try:
            content = path.read_text(encoding="utf-8", errors="ignore")
        except OSError as exc:
            violations.append(
                {
                    "rule_id": "D21-AUDIT-ERR",
                    "rule_name": "File read error",
                    "severity": "P0",
                    "file": relative_path,
                    "line": 0,
                    "matched": str(exc),
                    "line_content": "",
                    "description": "Unable to read a required D21 source file.",
                }
            )
            continue
        violations.extend(audit_file_content(relative_path, content))
    return scanned_files, violations


def run_self_test() -> Dict[str, object]:
    good = r'''// UnityEngine.Transform HttpClient DateTime.UtcNow new Random()
/* A block comment may contain forbidden executable-looking text:
   UnityEngine.GameObject, Socket, Stopwatch and D20Thing._privateField.
*/
public static class Pure
{
    private const string Normal = "UnityEngine.Transform // HttpClient DateTime.UtcNow new Random()";
    private const string Verbatim = @"UnityEngine.GameObject /* Socket */ Stopwatch DateTime.UtcNow";
    public static int Tick(int value) { return value + 1; } // Physics.Raycast
}'''
    bad = "using UnityEngine;\nvar now = DateTime.UtcNow;\nvar socket = new Socket();"
    good_violations = audit_file_content("selftest/Pure.cs", good)
    bad_violations = audit_file_content("selftest/Bad.cs", bad)
    detected_ids = {item["rule_id"] for item in bad_violations}
    expected_ids = {"D21-AUDIT-001", "D21-AUDIT-002", "D21-AUDIT-003"}
    passed = not good_violations and expected_ids.issubset(detected_ids)
    return {
        "passed": passed,
        "good_fixture_violations": good_violations,
        "bad_fixture_rule_ids": sorted(detected_ids),
    }


def run_audit(base_dir: Path) -> Dict[str, object]:
    source_dir = base_dir / "game" / "Assets" / "Scripts" / "Systems" / "D21"
    test_dir = base_dir / "game" / "Assets" / "Tests" / "EditMode" / "D21"
    scanned_files: List[str] = []
    violations: List[Dict[str, object]] = []
    for target_dir in (source_dir, test_dir):
        files, target_violations = audit_directory(target_dir, base_dir)
        scanned_files.extend(files)
        violations.extend(target_violations)

    return {
        "audit_name": "D21 Logic Boundary Static Audit",
        "status": "LOGIC_PASS" if not violations else "LOGIC_FAIL",
        "passed": not violations,
        "base_dir": base_dir.as_posix(),
        "scanned_files_count": len(scanned_files),
        "scanned_files": sorted(scanned_files),
        "violations_count": len(violations),
        "violations": violations,
        "checks": [rule["id"] for rule in P0_RULES]
        + ["D21-AUDIT-005"],
    }


def main(argv: List[str]) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base-dir", default=".", help="repository root to audit")
    parser.add_argument("--self-test", action="store_true", help="run deterministic rule fixtures")
    args = parser.parse_args(argv)
    base_dir = Path(args.base_dir).resolve()
    if args.self_test:
        result = run_self_test()
        print(json.dumps(result, indent=2, ensure_ascii=False, sort_keys=True))
        return 0 if result["passed"] else 1
    report = run_audit(base_dir)
    print(json.dumps(report, indent=2, ensure_ascii=False, sort_keys=True))
    return 0 if report["passed"] else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
