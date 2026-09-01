#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D23 纯逻辑跨系统边界审计。

只扫描 D23 的 C# 剧本源码和确定性执行器，不启动 Unity/Player，也不读取
运行时凭据、场景或资源。注释和字符串会先屏蔽，避免把说明文字误判为
可执行依赖；命中任何越界规则都会以非零退出码结束。
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Dict, Iterable, List, Sequence


ROOT = Path(__file__).resolve().parents[1]
D23_TEST_ROOT = ROOT / "game" / "Assets" / "Tests" / "EditMode" / "D23"


FORBIDDEN_RULES = (
    (
        "UNITY_RUNTIME",
        re.compile(
            r"\bUnityEngine\b|\bMonoBehaviour\b|\bGameObject\b|"
            r"\bTransform\b|\bNavMesh\b|\bSceneManager\b|\bPrefab\b|"
            r"\bPlayMode\b|\bCanvas(?:Group|Renderer)?\b"
        ),
    ),
    (
        "INPUT_OR_UI",
        re.compile(
            r"\bInputSystem\b|\bKeyboard\s*\.\s*current\b|"
            r"\bGamepad\s*\.\s*current\b|\bTouchscreen\s*\.\s*current\b|"
            r"\bInput\s*\.\s*(?:Get|GetKey|GetButton|GetAxis|mousePosition)\b|"
            r"\bEventSystem\b|\bSelectable\b|\bGraphicRaycaster\b|\bRectTransform\b|"
            r"\bTextMeshPro\b|\bTMPro\b|\bUnityEvent\b"
        ),
    ),
    (
        "NETWORK_OR_LLM",
        re.compile(
            r"\bHttpClient\b|\bHttpWebRequest\b|\bWebRequest\b|"
            r"\bUnityWebRequest\b|\bNetworkStream\b|\bWebSocket\b|\bSocket\b|"
            r"\bSystem\s*\.\s*Net\b|\brequests\b|\burllib\b|\bOpenAI\b|"
            r"\bChatGPT\b|\bLLM\b|\bLanguageModel\b"
        ),
    ),
    (
        "WALL_CLOCK_OR_NONDETERMINISM",
        re.compile(
            r"\bDateTime\s*\.\s*(?:Now|UtcNow)\b|"
            r"\bDateTimeOffset\s*\.\s*(?:Now|UtcNow)\b|\bStopwatch\b|"
            r"\bTime\s*\.\s*(?:deltaTime|timeScale|frameCount)\b|"
            r"\bSystem\s*\.\s*Random\b|\bUnityEngine\s*\.\s*Random\b|"
            r"\bRandom\s*\.\s*Shared\b|\bnew\s+Random\s*\("
        ),
    ),
)


BUSINESS_WRITE_RULES = (
    (
        "DIRECT_OWNER_FACT_WRITE",
        re.compile(
            r"(?:\b[A-Za-z_]\w*\s*\.\s*)+"
            r"(?:Money|Gold|Inventory|Cargo|MarketStock|Price|Supplies|"
            r"Hull|ConditionState|VoyageState|CrewIds|QuestState)\b\s*"
            r"(?:\+=|-=|\*=|/=|(?<![=!<>])=(?!=)|\+\+|--)|"
            r"\b(?:Money|Gold|Inventory|Cargo|MarketStock|Price|Supplies)\b"
            r"\s*\[[^\n;]*\]\s*(?:\+=|-=|\*=|/=|(?<![=!<>])=(?!=))"
        ),
    ),
)


def mask_csharp(source: str) -> str:
    """屏蔽 C# 注释、字符串和字符字面量，同时保留换行位置。"""

    chars = list(source)
    index = 0
    state = "normal"
    verbatim = False

    def blank(position: int) -> None:
        if chars[position] not in "\r\n":
            chars[position] = " "

    while index < len(source):
        current = source[index]
        following = source[index + 1] if index + 1 < len(source) else ""

        if state == "line_comment":
            blank(index)
            if current in "\r\n":
                state = "normal"
            index += 1
            continue
        if state == "block_comment":
            blank(index)
            if current == "*" and following == "/":
                blank(index + 1)
                index += 2
                state = "normal"
            else:
                index += 1
            continue
        if state == "string":
            blank(index)
            if verbatim:
                if current == '"' and following == '"':
                    blank(index + 1)
                    index += 2
                    continue
                if current == '"':
                    state = "normal"
                    verbatim = False
                index += 1
                continue
            if current == "\\":
                blank(index + 1) if index + 1 < len(source) else None
                index += 2
                continue
            if current == '"':
                state = "normal"
            index += 1
            continue
        if state == "char":
            blank(index)
            if current == "\\":
                blank(index + 1) if index + 1 < len(source) else None
                index += 2
                continue
            if current == "'":
                state = "normal"
            index += 1
            continue

        if current == "/" and following == "/":
            blank(index)
            blank(index + 1)
            index += 2
            state = "line_comment"
            continue
        if current == "/" and following == "*":
            blank(index)
            blank(index + 1)
            index += 2
            state = "block_comment"
            continue
        if current == '"':
            blank(index)
            state = "string"
            verbatim = index > 0 and source[index - 1] == "@"
            index += 1
            continue
        if current == "'":
            blank(index)
            state = "char"
            index += 1
            continue
        index += 1

    return "".join(chars)


def line_number(source: str, offset: int) -> int:
    return source.count("\n", 0, offset) + 1


def scan_text(path: Path, source: str) -> List[Dict[str, object]]:
    masked = mask_csharp(source)
    issues: List[Dict[str, object]] = []
    for rule_id, pattern in FORBIDDEN_RULES + BUSINESS_WRITE_RULES:
        for match in pattern.finditer(masked):
            # D23ScenarioFixtureFactory is the declared fixture construction
            # boundary. It initializes owner-owned snapshots only; scenario
            # files outside it must use public owner APIs.
            if rule_id == "DIRECT_OWNER_FACT_WRITE" and path.name == "D23ScenarioFixtureFactory.cs":
                continue
            issues.append(
                {
                    "rule": rule_id,
                    "path": path.resolve().relative_to(ROOT).as_posix(),
                    "line": line_number(source, match.start()),
                    "match": match.group(0).strip(),
                }
            )
    issues.sort(key=lambda item: (str(item["path"]), int(item["line"]), str(item["rule"])))
    return issues


def discover_files() -> List[Path]:
    if not D23_TEST_ROOT.exists():
        return []
    return sorted(D23_TEST_ROOT.glob("*.cs"))


def audit() -> Dict[str, object]:
    files = discover_files()
    issues: List[Dict[str, object]] = []
    unreadable: List[str] = []
    for path in files:
        try:
            source = path.read_text(encoding="utf-8-sig")
        except (OSError, UnicodeError) as error:
            unreadable.append("{}: {}".format(path, error))
            continue
        issues.extend(scan_text(path, source))

    if unreadable:
        issues.extend(
            {"rule": "UNREADABLE_SOURCE", "path": item, "line": 0, "match": item}
            for item in unreadable
        )
    return {
        "status": "PASS" if files and not issues else "FAIL",
        "files_scanned": len(files),
        "issues": issues,
        "rules": [rule_id for rule_id, _ in FORBIDDEN_RULES + BUSINESS_WRITE_RULES],
        "scope": "D23 EditMode pure C# scenarios only; Unity/runtime is deferred",
    }


def self_test() -> Dict[str, object]:
    cases = [
        ("comment_and_literal_masking", '// UnityEngine\nvar x = "HttpClient";', False),
        ("unity_rejected", "using UnityEngine;", True),
        ("random_rejected", "var random = new Random();", True),
        ("owner_write_rejected", "fixture.TradeState.Money = 12;", True),
        ("owner_api_allowed", "D3InventorySystem.TryAddCargo(inventory, item, 1, out error);", False),
    ]
    failures: List[str] = []
    for name, source, expect_issue in cases:
        found = bool(scan_text(ROOT / "self-test.cs", source))
        if found != expect_issue:
            failures.append(name)
    return {
        "status": "PASS" if not failures else "FAIL",
        "cases": len(cases),
        "failures": failures,
    }


def main(argv: Sequence[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--json", action="store_true", help="输出机器可读审计 JSON")
    parser.add_argument("--self-test", action="store_true", help="只运行审计器自检")
    args = parser.parse_args(argv)

    report = self_test() if args.self_test else audit()
    if args.json or args.self_test:
        print(json.dumps(report, ensure_ascii=False, sort_keys=True))
    else:
        print("D23_BOUNDARY_STATUS={}".format(report["status"]))
        print("D23_BOUNDARY_FILES={}".format(report.get("files_scanned", 0)))
        print("D23_BOUNDARY_ISSUES={}".format(len(report.get("issues", []))))
    return 0 if report["status"] == "PASS" else 1


if __name__ == "__main__":
    sys.exit(main())
