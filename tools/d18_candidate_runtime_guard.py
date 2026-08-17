"""D18-L17 read-only guard: static scan for candidate asset references.

Scans runtime code, configuration and Registry content for direct references
to ``Assets/Art/Candidates``, temporary download locations and Candidate-state
assets.  Comment/documentation lines and Editor-only code are classified and
reported as informational; they never block the result.  The tool is strictly
read-only: it never replaces, removes or rewrites any reference.

Rules (each finding carries file, line, rule, severity, classification,
snippet and a fix direction):

- ``candidate_path_reference``        runtime reference to ``Art/Candidates``
- ``temp_cache_reference``            reference to the temporary download cache
- ``external_archive_reference``      reference to an external archive path
- ``candidate_state_runtime_reference`` candidate/placeholder state combined
                                      with a runtime consumption marker
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any, Iterable

TASK = "D18-L17"
TOOL = "tools/d18_candidate_runtime_guard.py"

DEFAULT_SCAN_ROOTS = (
    "game/Assets/Scripts",
    "game/Assets/Content",
    "game/Assets/Settings",
    "game/Assets/Resources",
)
DEFAULT_EXCLUDE_DIRS = {"Candidates", "obj", ".git", "Library"}
DEFAULT_EXTENSIONS = {".cs", ".json"}

# ---------------------------------------------------------------------------
# line classification helpers
# ---------------------------------------------------------------------------


def _is_editor_file(path: Path) -> bool:
    return any(part == "Editor" or part.endswith(".Editor") for part in path.parts)


@dataclass
class ScanConfig:
    project_root: Path
    roots: list[Path]
    extensions: set[str]
    exclude_dirs: set[str]


@dataclass
class Finding:
    file: str
    line: int
    rule: str
    severity: str
    classification: str
    snippet: str
    fix_direction: str


# ---------------------------------------------------------------------------
# rules
# ---------------------------------------------------------------------------

CANDIDATE_PATH_RE = re.compile(r"(?<![a-z0-9_])art/candidates(?![a-z0-9_])")
CANDIDATE_PATH_ASSETS_RE = re.compile(r"(?<![a-z0-9_])assets/art/candidates(?![a-z0-9_])")
TEMP_CACHE_RE = re.compile(r"d18/cache|whalesislandreef[/\\]d18")
EXTERNAL_ARCHIVE_RE = re.compile(r"external_sources")
ARCHIVE_TOKEN_RE = re.compile(r"[\w./:\\\-]+\.(?:zip|rar|7z|tar\.gz|tar)\b", re.IGNORECASE)
PATH_CHAR_RE = re.compile(r"[/\\:]")
CANDIDATE_STATE_RE = re.compile(r"(?<![a-z0-9_])candidate(?![a-z0-9_])|candidate_state|candidate_asset|intake_eligible")
RUNTIME_CONSUME_RE = re.compile(
    r"Resources\.Load|LoadAsset|Addressables|Instantiate|runtime_prefab_path|resource_path|model_ref"
)

FIX_DIRECTIONS = {
    "candidate_path_reference": (
        "候选文件不得被运行时代码/配置/Registry 直接引用。改为通过 D18 逻辑资源接口"
        "（L16 ResourceKey / L46 只读接口）引用正式内容键；人工修复，工具不自动替换。"
    ),
    "temp_cache_reference": (
        "移除对临时下载缓存路径的引用。下载缓存只由来源登记任务（L21—L24）记录，"
        "运行时不得读取缓存目录。人工修复，工具不自动删除。"
    ),
    "external_archive_reference": (
        "移除对外部归档路径的引用。归档位于仓库外缓存或 artifacts/external_sources，"
        "运行时不得读取压缩包。人工修复，工具不自动删除。"
    ),
    "candidate_state_runtime_reference": (
        "Candidate/placeholder/intake 状态资产不可被运行时消费。等待 L35 迁移审计与"
        "未来 Runtime 晋级门禁后，经正式 ResourceKey/content_id 引用；人工修复。"
    ),
}


def _classify_line(stripped: str) -> str:
    """Return 'comment' for comment lines, else 'code'.

    ``.json`` comments are jsonc-style ``//`` only; URLs inside JSON strings
    are never treated as comments because only a leading ``//`` is recognised.
    """
    if stripped.startswith(("//", "/*", "*")):
        return "comment"
    return "code"


def _match_rules(normalized: str) -> list[str]:
    rules: list[str] = []
    if CANDIDATE_PATH_ASSETS_RE.search(normalized) or CANDIDATE_PATH_RE.search(normalized):
        rules.append("candidate_path_reference")
    if TEMP_CACHE_RE.search(normalized):
        rules.append("temp_cache_reference")
    if EXTERNAL_ARCHIVE_RE.search(normalized):
        rules.append("external_archive_reference")
    for token in ARCHIVE_TOKEN_RE.findall(normalized):
        if PATH_CHAR_RE.search(token):
            rules.append("external_archive_reference")
            break
    if CANDIDATE_STATE_RE.search(normalized) and RUNTIME_CONSUME_RE.search(normalized):
        rules.append("candidate_state_runtime_reference")
    return rules


def _scan_text(
    path: Path,
    text: str,
    relative: str,
    config: ScanConfig,
) -> list[Finding]:
    ext = path.suffix.lower()
    is_editor = _is_editor_file(path)
    findings: list[Finding] = []
    in_block_comment = False
    in_editor_block = False
    for index, raw_line in enumerate(text.splitlines(), start=1):
        stripped = raw_line.strip()
        lower = raw_line.lower().replace("\\", "/")
        if ext == ".cs":
            if "#if" in stripped and "UNITY_EDITOR" in stripped:
                in_editor_block = True
                continue
            if "#endif" in stripped:
                in_editor_block = False
                continue
            if "/*" in stripped and not in_block_comment:
                in_block_comment = "*/" not in stripped
            if "*/" in stripped and in_block_comment:
                in_block_comment = False
        if not stripped:
            continue
        if in_block_comment or _classify_line(stripped) == "comment":
            classification = "comment_or_documentation"
        elif is_editor or in_editor_block:
            classification = "editor_only"
        else:
            classification = "runtime"
        rules = _match_rules(lower)
        for rule in rules:
            severity = "P1" if rule == "candidate_state_runtime_reference" else "P0"
            if classification != "runtime":
                severity = "INFO"
            findings.append(
                Finding(
                    file=relative,
                    line=index,
                    rule=rule,
                    severity=severity,
                    classification=classification,
                    snippet=stripped[:160],
                    fix_direction=FIX_DIRECTIONS[rule],
                )
            )
    return findings


CANDIDATE_STATE_VALUES = {"candidate", "candidate_pending_review", "candidate_pending", "pending_review"}
PLACEHOLDER_STATE_VALUES = {"placeholder"}
RUNTIME_PATH_FIELDS = ("resource_path", "runtime_prefab_path", "model_ref", "registry_path")


def _is_placeholder_reference(value: str) -> bool:
    """True when a runtime-path value is itself a placeholder reference.

    Accepted forms: ``ph_``/``asset_ph_``/``placeholder_`` prefixed keys,
    ``unity-primitive://`` URIs and any path whose name contains
    ``placeholder`` (e.g. ``game/Assets/Prefabs/Placeholder/...`` or
    ``.../d15_item_prop_placeholder.prefab``).
    """
    lowered = value.lower()
    return (
        lowered.startswith(("placeholder_", "ph_", "asset_ph_", "unity-primitive://", "primitive://"))
        or "unity-primitive" in lowered
        or "placeholder" in lowered
    )


def _json_candidate_state_findings(
    path: Path,
    text: str,
    relative: str,
) -> list[Finding]:
    """Structural check for Registry/config JSON.

    Per-object: a Candidate-state object must not declare any runtime path;
    a placeholder-state object must not declare a non-placeholder runtime path.
    Line numbers point at the line containing the offending value.
    """
    try:
        data = json.loads(text)
    except (json.JSONDecodeError, ValueError):
        return []
    lines = text.splitlines()
    findings: list[Finding] = []

    def line_of(value: str) -> int:
        for index, raw in enumerate(lines, start=1):
            if value in raw:
                return index
        return 0

    def walk(node: Any) -> None:
        if isinstance(node, dict):
            state = node.get("asset_state") or node.get("state")
            state_key = "asset_state" if node.get("asset_state") is not None else "state"
            offender: str | None = None
            for field in RUNTIME_PATH_FIELDS:
                value = node.get(field)
                if isinstance(value, str) and value:
                    if state_key == "asset_state" and isinstance(state, str):
                        lowered = state.lower()
                        if lowered in CANDIDATE_STATE_VALUES:
                            offender = value
                            break
                        if lowered in PLACEHOLDER_STATE_VALUES and not _is_placeholder_reference(value):
                            offender = value
                            break
            if offender is not None:
                findings.append(
                    Finding(
                        file=relative,
                        line=line_of(offender),
                        rule="candidate_state_runtime_reference",
                        severity="P1",
                        classification="runtime",
                        snippet=offender[:160],
                        fix_direction=FIX_DIRECTIONS["candidate_state_runtime_reference"],
                    )
                )
            for value in node.values():
                walk(value)
        elif isinstance(node, list):
            for value in node:
                walk(value)

    walk(data)
    return findings


def scan_project(config: ScanConfig) -> dict[str, Any]:
    """Scan the configured roots and return a deterministic JSON result."""

    findings: list[Finding] = []
    scanned_files: list[str] = []
    root_paths: list[str] = []
    for root in config.roots:
        if not root.is_dir():
            continue
        root_paths.append(str(root.relative_to(config.project_root)).replace("\\", "/"))
        for path in sorted(root.rglob("*")):
            if path.is_file() and path.suffix.lower() in config.extensions:
                if any(part in config.exclude_dirs for part in path.parts):
                    continue
                relative = str(path.relative_to(config.project_root)).replace("\\", "/")
                scanned_files.append(relative)
                try:
                    text = path.read_text(encoding="utf-8", errors="replace")
                except OSError:
                    continue
                findings.extend(_scan_text(path, text, relative, config))
                if path.suffix.lower() == ".json":
                    findings.extend(_json_candidate_state_findings(path, text, relative))

    findings.sort(key=lambda f: (f.file, f.line, f.rule))
    rendered = [f.__dict__ for f in findings]
    runtime_findings = [f for f in findings if f.classification == "runtime"]
    blocked = bool(runtime_findings)
    return {
        "task": TASK,
        "tool": TOOL,
        "read_only": True,
        "status": "BLOCKED" if blocked else "PASS",
        "scan_roots": root_paths,
        "exclude_dirs": sorted(config.exclude_dirs),
        "extensions": sorted(config.extensions),
        "scanned_files_count": len(scanned_files),
        "scanned_files": scanned_files,
        "findings_count": len(rendered),
        "runtime_findings_count": len(runtime_findings),
        "comment_findings_count": sum(1 for f in findings if f.classification == "comment_or_documentation"),
        "editor_findings_count": sum(1 for f in findings if f.classification == "editor_only"),
        "rule_counts": {
            rule: sum(1 for f in findings if f.rule == rule)
            for rule in sorted(FIX_DIRECTIONS)
        },
        "findings": rendered,
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--scan-root", type=Path, action="append", default=[])
    parser.add_argument("--exclude-dir", type=str, action="append", default=[])
    parser.add_argument("--extensions", type=str, default=",".join(sorted(DEFAULT_EXTENSIONS)))
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    project_root = args.project_root.resolve()
    roots = [item.resolve() for item in args.scan_root] or [
        (project_root / item).resolve() for item in DEFAULT_SCAN_ROOTS
    ]
    exclude_dirs = set(DEFAULT_EXCLUDE_DIRS) | {item.strip() for item in args.exclude_dir if item.strip()}
    extensions = {item.strip().lower() for item in args.extensions.split(",") if item.strip()}
    config = ScanConfig(
        project_root=project_root,
        roots=roots,
        extensions=extensions,
        exclude_dirs=exclude_dirs,
    )
    result = scan_project(config)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
