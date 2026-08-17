"""Read-only path-boundary validator for D18 logic asset records (D18-L08).

Defines and validates the four path boundaries frozen by D18-L01
(``docs/game/design/common/D18_LOGIC_GOVERNANCE_BASELINE.md`` section 6.1):

* Candidate source files     -> ``game/Assets/Art/Candidates/``
* Logic metadata/evidence    -> ``docs/game/design/common/assets``,
                                ``docs/game/design/common/handoffs``,
                                ``game/tools/results`` (plus the candidate root,
                                because license evidence text is co-located with
                                candidate files, e.g. ``.../Candidates/*/License.txt``)
* Ignored local caches       -> frozen segment set derived from the committed
                                ``.gitignore`` at the L01 baseline date; the tool
                                never reads or rewrites ``.gitignore``
* Future formal resources    -> ``game/Assets/Art/{Models,Textures,Materials,
                                Placeholder,Vegetation}``, ``game/Assets/Audio``,
                                ``game/Assets/Prefabs``, ``game/Assets/UI``,
                                ``game/Assets/Resources``, ``game/Assets/Settings``,
                                ``game/Assets/ThirdParty``

Rejected in every kind: absolute paths, drive/UNC paths, parent traversal
(``..``), environment-variable paths, temp/cache locations, and candidate
binaries appearing directly inside runtime reference fields.

The tool is strictly read-only: it never moves, deletes, imports or rewrites
files, and it never edits ``.gitignore`` or real resource directories. Error
output always explains the allowed roots and the violating path.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from dataclasses import dataclass, field as dc_field
from pathlib import Path
from typing import Any, Iterable


REPO_ROOT = Path(__file__).resolve().parents[1]

CANDIDATE_ROOT = "game/Assets/Art/Candidates"
LOGIC_METADATA_ROOTS = (
    "docs/game/design/common/assets",
    "docs/game/design/common/handoffs",
    "game/tools/results",
)
FORMAL_RESOURCE_ROOTS = (
    "game/Assets/Art/Models",
    "game/Assets/Art/Textures",
    "game/Assets/Art/Materials",
    "game/Assets/Art/Placeholder",
    "game/Assets/Art/Vegetation",
    "game/Assets/Audio",
    "game/Assets/Prefabs",
    "game/Assets/UI",
    "game/Assets/Resources",
    "game/Assets/Settings",
    "game/Assets/ThirdParty",
)
EVIDENCE_ROOTS = LOGIC_METADATA_ROOTS + (CANDIDATE_ROOT,)

# Frozen from the committed .gitignore (L01 baseline, 2026-08-16). Segment
# matching is case-insensitive, mirroring the .gitignore [Tt]emp style rules.
IGNORED_CACHE_SEGMENTS = frozenset(
    {
        "library",
        "temp",
        "obj",
        "build",
        "builds",
        "logs",
        "usersettings",
        "memorycaptures",
        "recordings",
        "terraindatacache",
        "screenshots",
        "serverdata",
        "caches",
        "packagecache",
        "monobleedingedge",
        "node_modules",
        "dist",
        "coverage",
        "__pycache__",
        "bin",
        ".vs",
        ".idea",
        ".vscode",
    }
)
ARTIFACTS_SEGMENT = "artifacts"

VALID_KINDS = ("candidate", "evidence", "metadata", "formal", "runtime_reference")

_DRIVE_RE = re.compile(r"^[A-Za-z]:")
_DOT_SEGMENT = "."
_PARENT_SEGMENT = ".."


@dataclass(frozen=True)
class PathIssue:
    code: str
    field: str
    value: str
    reason: str
    allowed_roots: tuple[str, ...]


@dataclass(frozen=True)
class ValidationResult:
    kind: str
    normalized: str
    issues: tuple[PathIssue, ...] = dc_field(default_factory=tuple)

    @property
    def valid(self) -> bool:
        return not self.issues


def allowed_roots_for(kind: str) -> tuple[str, ...]:
    if kind == "candidate":
        return (CANDIDATE_ROOT,)
    if kind == "evidence":
        return EVIDENCE_ROOTS
    if kind == "metadata":
        return LOGIC_METADATA_ROOTS
    if kind in ("formal", "runtime_reference"):
        return FORMAL_RESOURCE_ROOTS
    raise ValueError(f"未知路径类型: {kind!r}")


def normalize_path(value: str) -> str:
    """Normalize Windows/Unix/mixed separators to forward slashes and drop
    ``.`` and empty segments. Keeps ``..`` for later rejection."""
    raw = value.strip().replace("\\", "/")
    segments = [s for s in raw.split("/") if s not in ("", _DOT_SEGMENT)]
    return "/".join(segments)


def _segment_under_root(normalized: str, root: str) -> bool:
    """True when ``normalized`` is strictly deeper than ``root`` and shares the
    root prefix segment-by-segment, case-insensitively (Windows reality)."""
    path_parts = normalized.split("/")
    root_parts = root.split("/")
    return len(path_parts) > len(root_parts) and all(
        p.lower() == r.lower() for p, r in zip(path_parts, root_parts)
    )


def _root_case_matches(normalized: str, root: str) -> bool:
    root_parts = root.split("/")
    return normalized.split("/")[: len(root_parts)] == list(root_parts)


def _cache_segment(normalized: str) -> str | None:
    parts = normalized.split("/")
    if parts[0].lower() == ARTIFACTS_SEGMENT:
        return parts[0]
    for part in parts:
        if part.lower() in IGNORED_CACHE_SEGMENTS:
            return part
    return None


def _reject(result: ValidationResult, code: str, field: str, value: str, reason: str, kind: str) -> ValidationResult:
    return ValidationResult(
        kind=kind,
        normalized=result.normalized,
        issues=result.issues + (PathIssue(code=code, field=field, value=value, reason=reason, allowed_roots=allowed_roots_for(kind)),),
    )


def validate_path(value: Any, kind: str = "evidence", field: str = "") -> ValidationResult:
    """Validate a single path string against the frozen D18 path boundaries."""
    normalized = ""
    result = ValidationResult(kind=kind, normalized=normalized)

    if not isinstance(value, str) or not value.strip():
        return _reject(result, "empty_path_rejected", field, "" if value is None else str(value), "路径为空，必须填写仓库内相对路径。", kind)
    raw = value.strip()
    slashed = raw.replace("\\", "/")
    if slashed.startswith("/"):
        return _reject(result, "absolute_path_rejected", field, raw, "绝对路径（POSIX 或 UNC 风格）不允许；路径必须是仓库内相对路径。", kind)
    if _DRIVE_RE.match(slashed):
        return _reject(result, "drive_path_rejected", field, raw, "驱动器/盘符路径不允许（如 C:\\... 或 C:foo）；路径必须是仓库内相对路径。", kind)
    normalized = normalize_path(raw)
    result = ValidationResult(kind=kind, normalized=normalized)

    if not normalized:
        return _reject(result, "empty_path_rejected", field, raw, "规范化后路径为空。", kind)
    if "\x00" in raw:
        return _reject(result, "control_character_rejected", field, raw, "路径包含 NUL/控制字符。", kind)
    if "%" in raw or raw.strip().startswith("$"):
        return _reject(result, "env_var_path_rejected", field, raw, "路径包含环境变量引用（%...% 或 $...），不属于仓库相对路径。", kind)
    if any(part == _PARENT_SEGMENT for part in normalized.split("/")):
        return _reject(result, "parent_traversal_rejected", field, raw, "路径包含 .. 父级回退，禁止越出允许根。", kind)

    cache = _cache_segment(normalized)
    if cache is not None:
        return _reject(result, "ignored_cache_path_rejected", field, raw, f"路径进入被忽略的本地缓存/生成目录段 {cache!r}（依据 .gitignore 冻结清单）。", kind)

    if kind == "runtime_reference":
        if _segment_under_root(normalized, CANDIDATE_ROOT):
            return _reject(result, "candidate_binary_in_runtime_reference", field, raw, "运行时引用字段直接指向 Candidate 二进制，游戏逻辑不得直接引用 Candidate。", kind)
        if not any(_segment_under_root(normalized, root) for root in FORMAL_RESOURCE_ROOTS):
            return _reject(result, "runtime_reference_outside_formal_roots", field, raw, "运行时引用必须指向未来正式资源根；不允许直接指向候选或逻辑元数据。", kind)
        return result

    allowed = allowed_roots_for(kind)
    for root in allowed:
        if _segment_under_root(normalized, root):
            if _root_case_matches(normalized, root):
                return result
            return _reject(result, "root_case_mismatch", field, raw, f"根前缀大小写与规范不一致（规范根：{root}）；路径必须大小写确定。", kind)
    return _reject(
        result,
        "outside_allowed_root",
        field,
        raw,
        "路径不在允许根内；允许根：" + "、".join(allowed) + "。",
        kind,
    )


def classify_path(value: Any) -> str:
    """Best-effort category label for a path, used in error output."""
    result = validate_path(value, kind="evidence")
    if not result.valid:
        return result.issues[0].code
    if _segment_under_root(result.normalized, CANDIDATE_ROOT):
        return "candidate"
    if any(_segment_under_root(result.normalized, root) for root in LOGIC_METADATA_ROOTS):
        return "metadata"
    if any(_segment_under_root(result.normalized, root) for root in FORMAL_RESOURCE_ROOTS):
        return "formal"
    return "outside"


def check_catalog(catalog_path: Path | None = None) -> dict[str, Any]:
    """Read-only audit of the real intake catalog path fields (D18-S02-R02
    historical snapshot). Skips absent fields; presence is other validators'
    domain (e.g. D18-L09/L34)."""
    catalog_path = catalog_path or REPO_ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"
    catalog = json.loads(catalog_path.read_text(encoding="utf-8"))
    entries = catalog.get("eligible_entries", [])
    violations: list[dict[str, str]] = []
    by_code: Counter[str] = Counter()
    fields_checked = 0
    for entry in entries:
        checks = (
            ("candidate_file_path", "candidate"),
            ("manifest_path", "evidence"),
            ("import_verified_evidence_path", "evidence"),
        )
        for field, kind in checks:
            value = entry.get(field)
            if value is None or (isinstance(value, str) and not value.strip()):
                continue
            fields_checked += 1
            result = validate_path(value, kind=kind, field=field)
            if not result.valid:
                issue = result.issues[0]
                by_code[issue.code] += 1
                violations.append(
                    {
                        "intake_id": str(entry.get("intake_id", "")),
                        "field": field,
                        "value": str(value),
                        "code": issue.code,
                        "reason": issue.reason,
                        "allowed_roots": list(issue.allowed_roots),
                    }
                )
        license_evidence = (entry.get("license") or {}).get("license_evidence_path")
        if license_evidence:
            fields_checked += 1
            result = validate_path(license_evidence, kind="evidence", field="license.license_evidence_path")
            if not result.valid:
                issue = result.issues[0]
                by_code[issue.code] += 1
                violations.append(
                    {
                        "intake_id": str(entry.get("intake_id", "")),
                        "field": "license.license_evidence_path",
                        "value": str(license_evidence),
                        "code": issue.code,
                        "reason": issue.reason,
                        "allowed_roots": list(issue.allowed_roots),
                    }
                )
        hash_path = (entry.get("hash_evidence") or {}).get("path")
        if hash_path:
            fields_checked += 1
            result = validate_path(hash_path, kind="evidence", field="hash_evidence.path")
            if not result.valid:
                issue = result.issues[0]
                by_code[issue.code] += 1
                violations.append(
                    {
                        "intake_id": str(entry.get("intake_id", "")),
                        "field": "hash_evidence.path",
                        "value": str(hash_path),
                        "code": issue.code,
                        "reason": issue.reason,
                        "allowed_roots": list(issue.allowed_roots),
                    }
                )
    return {
        "catalog": str(catalog_path.relative_to(REPO_ROOT)).replace("\\", "/"),
        "entries_checked": len(entries),
        "path_fields_checked": fields_checked,
        "violation_count": len(violations),
        "violation_by_code": dict(by_code),
        "violations": violations,
        "read_only": True,
    }


def _print_issue(issue: PathIssue) -> None:
    print(f"[REJECT] {issue.code}  field={issue.field!r}")
    print(f"  value: {issue.value}")
    print(f"  allowed_roots: {', '.join(issue.allowed_roots) or '(无)'}")
    print(f"  reason: {issue.reason}")


def main(argv: Iterable[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="D18 资源路径边界验证器（只读，D18-L08）")
    parser.add_argument("paths", nargs="*", help="要验证的相对路径")
    parser.add_argument("--kind", choices=sorted(VALID_KINDS), default="evidence", help="路径所属边界类型")
    parser.add_argument("--field", default="", help="字段名（用于错误输出）")
    parser.add_argument("--check-catalog", action="store_true", help="审计真实准入目录的全部路径字段")
    parser.add_argument("--self-check", action="store_true", help="输出允许根清单与只读保证")
    args = parser.parse_args(list(argv) if argv is not None else None)

    if args.self_check:
        print("D18 资源路径边界（冻结于 D18-L01 基线）")
        for kind in VALID_KINDS:
            print(f"  {kind}: {', '.join(allowed_roots_for(kind))}")
        print("  忽略缓存段: " + ", ".join(sorted(IGNORED_CACHE_SEGMENTS)))
        print("  只读保证: 工具不移动/删除/导入文件，不修改 .gitignore 与真实资源目录。")
        return 0

    if args.check_catalog:
        summary = check_catalog()
        print(json.dumps({k: v for k, v in summary.items() if k != "violations"}, ensure_ascii=False, indent=2))
        for violation in summary["violations"]:
            print(f"[REJECT] {violation['code']}  intake_id={violation['intake_id']}  field={violation['field']!r}")
            print(f"  value: {violation['value']}")
            print(f"  allowed_roots: {', '.join(violation['allowed_roots'])}")
            print(f"  reason: {violation['reason']}")
        return 1 if summary["violation_count"] else 0

    failed = 0
    for path in args.paths:
        result = validate_path(path, kind=args.kind, field=args.field)
        if result.valid:
            print(f"[PASS] {path}  ->  {result.normalized}")
        else:
            failed = 1
            for issue in result.issues:
                _print_issue(issue)
    if not args.paths and not args.check_catalog and not args.self_check:
        parser.print_usage()
        return 2
    return failed


if __name__ == "__main__":
    raise SystemExit(main())
