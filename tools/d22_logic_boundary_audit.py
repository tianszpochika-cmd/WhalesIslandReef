#!/usr/bin/env python3
"""D22 pure-logic boundary audit.

The audit masks C# comments, string literals and character literals before
matching rules.  This keeps examples, diagnostics and documentation-like
comments from becoming false violations while still checking executable code.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Sequence


DEFAULT_RELATIVE_PATHS = (
    Path("game/Assets/Scripts/Systems/D22"),
    Path("game/Assets/Tests/EditMode/D22"),
)


FORBIDDEN_RULES = (
    ("CANVAS", re.compile(r"\bCanvas(?:Group|Renderer)?\b")),
    (
        "EVENT_SYSTEM",
        re.compile(r"\bEventSystem\b|\bSelectable\b|\bGraphicRaycaster\b"),
    ),
    (
        "INPUT",
        re.compile(
            r"\bInputSystem\b|\bKeyboard\s*\.\s*current\b|"
            r"\bGamepad\s*\.\s*current\b|\bTouchscreen\s*\.\s*current\b|"
            r"\bInput\s*\.\s*(?:Get|GetKey|GetButton|GetAxis|mousePosition)\b"
        ),
    ),
    (
        "UNITY_UI",
        re.compile(
            r"\bUnityEngine\b|\bMonoBehaviour\b|\bGameObject\b|"
            r"\bRectTransform\b|\bUnityEvent\b|\bTMPro\b|\bTextMeshPro\b|"
            r"\bButton\b|\bImage\b|\bRawImage\b"
        ),
    ),
    (
        "NETWORK",
        re.compile(
            r"\bHttpClient\b|\bHttpWebRequest\b|\bWebRequest\b|"
            r"\bUnityWebRequest\b|\bSocket\b|\bSystem\s*\.\s*Net\b|"
            r"\bNetworkStream\b|\bWebSocket\b"
        ),
    ),
    (
        "LLM",
        re.compile(
            r"\bLLM\b|\bOpenAI\b|\bChatGPT\b|\bOllama\b|"
            r"\bLanguageModel\b|\bPrompt(?:Template)?\b"
        ),
    ),
    (
        "WALL_CLOCK",
        re.compile(
            r"\bDateTime\s*\.\s*(?:Now|UtcNow)\b|"
            r"\bDateTimeOffset\s*\.\s*(?:Now|UtcNow)\b|"
            r"\bStopwatch\b|\bTime\s*\.\s*(?:deltaTime|timeScale|frameCount)\b"
        ),
    ),
    (
        "UNCONTROLLED_RANDOM",
        re.compile(
            r"\bSystem\s*\.\s*Random\b|\bUnityEngine\s*\.\s*Random\b|"
            r"\bRandom\s*\.\s*Shared\b|\bnew\s+Random\s*\("
        ),
    ),
)


# These patterns intentionally target assignments to canonical domain facts,
# not DTO fields such as CommandResult or ordinary local counters.
BUSINESS_WRITE_RULES = (
    (
        "BUSINESS_PRIVATE_WRITE",
        re.compile(
            r"(?:\b[A-Za-z_]\w*\s*\.\s*)+"
            r"(?:Cargo|Supplies|Hull|Money|Gold|Price|Inventory|MarketStock|"
            r"CrewIds|QuestState|ShipState|VoyageState|D3InventoryState|"
            r"D3MarketState|D19PortReadModel|D20Character|D21Ship)\b\s*"
            r"(?:=|\+=|-=|\*=|/=|\+\+|--)|"
            r"\b(?:Cargo|Supplies|Hull|Money|Gold|Price|Inventory|MarketStock)\b"
            r"\s*\[[^\n;]*\]\s*(?:=|\+=|-=|\*=|/=)"
        ),
    ),
    (
        "DOMAIN_OWNER_PRIVATE_WRITE",
        re.compile(
            r"\b(?:D3InventorySystem|TradeEconomySystem|SaveSystem|"
            r"D19PortCommandRouter|D20CharacterCommandService|"
            r"D21VoyageCommandService)\b\s*\.\s*[A-Za-z_]\w*"
            r"\s*(?:=|\+=|-=|\+\+|--)"
        ),
    ),
)


# These are caller-owned fixture/local-clone mutations, not D22 business/UI
# writes.  Keep both allowances narrow.  The test changes the local
# D3InventoryState after projection to prove defensive copying; the adapter
# writes only to a newly-created safeInventory clone before read-only D3
# calculations.  Direct owner/source assignments remain prohibited.
ALLOWED_TEST_FIXTURE_WRITE = re.compile(
    r"\binventory\s*\.\s*Cargo\s*\[[^\n;]*\]\s*(?:=|\+=|-=|\*=|/=)"
)
ALLOWED_LOCAL_CLONE_WRITE = re.compile(
    r"\bsafeInventory\s*\.\s*(?:Cargo|PersonalItems)\s*"
    r"\[[^\n;]*\]\s*(?:=|\+=|-=|\*=|/=)"
)


def mask_csharp(source: str) -> str:
    """Replace comments and literals with spaces while preserving newlines."""

    chars = list(source)
    length = len(source)
    index = 0
    state = "normal"
    verbatim = False

    def blank(position: int) -> None:
        if chars[position] not in "\r\n":
            chars[position] = " "

    while index < length:
        current = source[index]
        next_char = source[index + 1] if index + 1 < length else ""

        if state == "line_comment":
            blank(index)
            if current in "\r\n":
                state = "normal"
            index += 1
            continue

        if state == "block_comment":
            blank(index)
            if current == "*" and next_char == "/":
                blank(index + 1)
                index += 2
                state = "normal"
            else:
                index += 1
            continue

        if state == "string":
            blank(index)
            if verbatim:
                if current == '"' and next_char == '"':
                    blank(index + 1)
                    index += 2
                    continue
                if current == '"':
                    state = "normal"
                    verbatim = False
                index += 1
                continue
            if current == "\\":
                if index + 1 < length:
                    blank(index + 1)
                    index += 2
                else:
                    index += 1
                continue
            if current == '"':
                state = "normal"
            index += 1
            continue

        if state == "raw_string":
            blank(index)
            if source[index : index + 3] == '"""':
                blank(index + 1)
                blank(index + 2)
                index += 3
                state = "normal"
            else:
                index += 1
            continue

        if state == "char":
            blank(index)
            if current == "\\":
                if index + 1 < length:
                    blank(index + 1)
                    index += 2
                else:
                    index += 1
                continue
            if current == "'":
                state = "normal"
            index += 1
            continue

        if current == "/" and next_char == "/":
            blank(index)
            blank(index + 1)
            index += 2
            state = "line_comment"
            continue
        if current == "/" and next_char == "*":
            blank(index)
            blank(index + 1)
            index += 2
            state = "block_comment"
            continue
        if current == '"':
            if source[index : index + 3] == '"""':
                blank(index)
                blank(index + 1)
                blank(index + 2)
                state = "raw_string"
                index += 3
                continue
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


def is_allowed_fixture_write(path: Path, masked: str, match: re.Match[str]) -> bool:
    normalized = str(path).replace("\\", "/")
    line_start = masked.rfind("\n", 0, match.start()) + 1
    line_end = masked.find("\n", match.end())
    if line_end < 0:
        line_end = len(masked)
    line = masked[line_start:line_end].strip()
    return (
        match.re is BUSINESS_WRITE_RULES[0][1]
        and (
            (
                "game/Assets/Tests/EditMode/D22/" in normalized
                and bool(ALLOWED_TEST_FIXTURE_WRITE.match(line))
            )
            or (
                "game/Assets/Scripts/Systems/D22/" in normalized
                and bool(ALLOWED_LOCAL_CLONE_WRITE.match(line))
            )
        )
    )


def scan_text(path: Path, source: str) -> List[Dict[str, object]]:
    masked = mask_csharp(source)
    rules = FORBIDDEN_RULES + BUSINESS_WRITE_RULES
    issues: List[Dict[str, object]] = []
    for rule_id, pattern in rules:
        for match in pattern.finditer(masked):
            if rule_id == "BUSINESS_PRIVATE_WRITE" and is_allowed_fixture_write(path, masked, match):
                continue
            issues.append(
                {
                    "rule": rule_id,
                    "path": str(path),
                    "line": line_number(source, match.start()),
                    "match": match.group(0).strip(),
                }
            )
    issues.sort(key=lambda item: (str(item["path"]), int(item["line"]), str(item["rule"])))
    return issues


def discover_files(root: Path, relative_paths: Iterable[Path]) -> List[Path]:
    files: List[Path] = []
    for relative in relative_paths:
        directory = root / relative
        if not directory.exists():
            continue
        files.extend(sorted(directory.rglob("*.cs")))
    return files


def scan_files(files: Sequence[Path]) -> List[Dict[str, object]]:
    issues: List[Dict[str, object]] = []
    for path in files:
        try:
            source = path.read_text(encoding="utf-8-sig")
        except UnicodeDecodeError as error:
            issues.append(
                {
                    "rule": "SOURCE_ENCODING",
                    "path": str(path),
                    "line": 1,
                    "match": str(error),
                }
            )
            continue
        issues.extend(scan_text(path, source))
    issues.sort(key=lambda item: (str(item["path"]), int(item["line"]), str(item["rule"])))
    return issues


def run_self_test() -> None:
    fixture = (
        '// Canvas EventSystem Input HttpClient DateTime.Now new Random()\n'
        '/* GameObject and LLM are documentation only. */\n'
        'string text = "Canvas EventSystem InputSystem HttpClient DateTime.Now";\n'
        'string rawText = """Canvas EventSystem InputSystem HttpClient""";\n'
        "char quote = '\"';\n"
        'int safeValue = 1;\n'
        'Canvas actualCanvas;\n'
    )
    issues = scan_text(Path("self-test.cs"), fixture)
    if len(issues) != 1 or issues[0]["rule"] != "CANVAS":
        raise AssertionError("comment/string masking self-test failed: " + repr(issues))

    business_fixture = (
        '// Cargo = 1;\n'
        'string ignored = "ShipState.Hull = 2";\n'
        'cargoValue = 1;\n'
    )
    business_issues = scan_text(Path("business-self-test.cs"), business_fixture)
    if business_issues:
        raise AssertionError("lowercase/local value masking self-test failed: " + repr(business_issues))

    fixture_write = (
        'inventory.Cargo["timber"] = 2;\n'
        'state.Cargo["timber"] = 2;\n'
    )
    fixture_issues = scan_text(
        Path("game/Assets/Tests/EditMode/D22/D22ReadModelAdapterTests.cs"),
        fixture_write,
    )
    if len(fixture_issues) != 1 or fixture_issues[0]["line"] != 2:
        raise AssertionError("owner fixture exception/self-write detection failed: " + repr(fixture_issues))

    clone_write = (
        "safeInventory.Cargo[entry.Key] = entry.Value;\n"
        "state.Cargo[entry.Key] = entry.Value;\n"
    )
    clone_issues = scan_text(
        Path("game/Assets/Scripts/Systems/D22/D22InventoryReadModelAdapter.cs"),
        clone_write,
    )
    if len(clone_issues) != 1 or clone_issues[0]["line"] != 2:
        raise AssertionError("local clone exception/self-write detection failed: " + repr(clone_issues))


def main(argv: Optional[Sequence[str]] = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--self-test", action="store_true")
    parser.add_argument("--json", action="store_true", dest="as_json")
    args = parser.parse_args(argv)

    if args.self_test:
        try:
            run_self_test()
        except AssertionError as error:
            print("D22 boundary audit self-test FAIL: " + str(error), file=sys.stderr)
            return 1
        print("D22 boundary audit self-test PASS")

    root = args.root.resolve()
    files = discover_files(root, DEFAULT_RELATIVE_PATHS)
    issues = scan_files(files)
    report = {
        "status": "PASS" if not issues else "FAIL",
        "files_scanned": len(files),
        "issues": issues,
        "rules": [rule_id for rule_id, _ in FORBIDDEN_RULES + BUSINESS_WRITE_RULES],
    }
    if args.as_json:
        print(json.dumps(report, ensure_ascii=False, indent=2))
    elif issues:
        print("D22 boundary audit FAIL: {} issue(s) in {} file(s)".format(len(issues), len(files)))
        for issue in issues:
            print("{path}:{line}: [{rule}] {match}".format(**issue))
    else:
        print("D22 boundary audit PASS: {} file(s), 0 issue(s)".format(len(files)))
    return 0 if not issues else 1


if __name__ == "__main__":
    raise SystemExit(main())
