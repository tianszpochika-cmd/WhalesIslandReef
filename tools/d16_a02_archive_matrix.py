"""Verify the D16-A02 archive matrix against exact Unity test case names.

This checker does not invent coverage: every manifest item must be an exact,
real passed test case from Unity's result XML. Substring matches are rejected
so a renamed or merely related test cannot silently satisfy a category.
"""

from __future__ import annotations

import json
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
DEFAULT_XML = ROOT / "game" / "D2_TestResults.xml"
REPORT = ROOT / "tools" / "results" / "d16_a02_archive_matrix_result.json"
MANIFEST = ROOT / "tools" / "d16_a02_archive_matrix_manifest.json"


def load_manifest(path: Path = MANIFEST) -> dict:
    data = json.loads(path.read_text(encoding="utf-8"))
    categories = data.get("categories")
    if not isinstance(categories, list) or not categories:
        raise ValueError("D16-A02 archive manifest must contain non-empty categories")
    labels = [item.get("label") for item in categories]
    if any(not label for label in labels) or len(labels) != len(set(labels)):
        raise ValueError("D16-A02 archive manifest category labels must be unique")
    for item in categories:
        tests = item.get("required_tests")
        if not isinstance(tests, list) or not tests or any(not name for name in tests):
            raise ValueError("Every D16-A02 category needs non-empty required_tests")
    return data


def build_report(xml_path: Path, manifest_path: Path = MANIFEST) -> dict:
    manifest = load_manifest(manifest_path)
    root = ET.parse(xml_path).getroot()
    cases = []
    failures = []
    for case in root.iter("test-case"):
        item = {
            "name": case.attrib.get("name", ""),
            "classname": case.attrib.get("classname", ""),
            "result": case.attrib.get("result", ""),
        }
        cases.append(item)
        if item["result"] not in ("Passed", "Skipped"):
            failures.append(item)

    passed_names = {case["name"] for case in cases if case["result"] == "Passed"}
    matched = {}
    missing = {}
    for category in manifest["categories"]:
        label = category["label"]
        required = category["required_tests"]
        matched[label] = [name for name in required if name in passed_names]
        absent = [name for name in required if name not in passed_names]
        if absent:
            missing[label] = absent

    return {
        "source": str(xml_path),
        "manifest": str(manifest_path),
        "test_case_count": len(cases),
        "failed_or_inconclusive_count": len(failures),
        "required_categories": [item["label"] for item in manifest["categories"]],
        "required_test_count": sum(len(item["required_tests"]) for item in manifest["categories"]),
        "matched_counts": {key: len(value) for key, value in matched.items()},
        "matched_tests": matched,
        "missing_tests": missing,
        "missing_categories": {key: value for key, value in missing.items()
                               if not matched.get(key)},
        "passed": not missing and not failures,
    }


def main() -> int:
    xml_path = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_XML
    report = build_report(xml_path)
    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if report["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
