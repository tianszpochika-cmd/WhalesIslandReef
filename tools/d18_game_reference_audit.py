"""D18-L44 read-only game reference audit.

Scans game code, data and the Content Registry to confirm that business logic
references stable ``content_id`` / ResourceKey values instead of Candidate
paths, temporary download directories, archive zip paths or display names.

Rules (every finding carries file, line, rule, severity, classification,
owner, snippet and fix direction):

- ``candidate_path_reference``      reference to ``Assets/Art/Candidates``
- ``temp_cache_reference``          reference to a temporary download cache
- ``archive_reference``             reference to a .zip/.rar/.7z archive path
- ``external_archive_path``         reference to artifacts/external_sources
- ``candidate_key_as_business_key`` runtime use of the historical
                                    ``candidate_`` key prefix
- ``key_position_path_reference``   JSON id/content_id/resource_key value that
                                    is a path (slash, backslash, extension,
                                    Candidates) instead of a business key
- ``display_name_as_key``           JSON id field whose value is a display name
                                    (whitespace or upper case) instead of a
                                    snake_case business key

Classification separates real runtime references from non-blocking contexts:

- ``runtime``        blocking (P0): production code/config/Registry
- ``test``           informational: files under test directories
- ``editor_only``    informational: Editor directories / UNITY_EDITOR blocks
- ``documentation``  informational: comment lines in code

Owners are derived from the repository layout (systems/gameplay/agent/core/
presentation/networking/registry/resources/settings/agentcore/editor/tools).

The tool is strictly read-only: it never modifies game code or the Registry,
never starts Unity and never accesses the network.  It reports offenders with
file/line and an owner; fixes are human-driven.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Any

TASK = "D18-L44"
TOOL = "tools/d18_game_reference_audit.py"

DEFAULT_SCAN_ROOTS = (
    "game/Assets/Scripts",
    "game/Assets/Content",
    "game/Assets/Resources",
    "game/Assets/Settings",
    "game/AgentCore",
)
DEFAULT_EXCLUDE_DIRS = {"Candidates", "obj", "Library", "Packages", ".git", "Temp"}
DEFAULT_EXTENSIONS = {".cs", ".json"}

# ---------------------------------------------------------------------------
# owner / classification helpers
# ---------------------------------------------------------------------------

OWNER_RULES = (
    ("game/AgentCore", "agentcore"),
    ("game/Assets/Scripts/Systems", "systems"),
    ("game/Assets/Scripts/Gameplay", "gameplay"),
    ("game/Assets/Scripts/Agent", "agent"),
    ("game/Assets/Scripts/Core", "core"),
    ("game/Assets/Scripts/Presentation", "presentation"),
    ("game/Assets/Scripts/Networking", "networking"),
    ("game/Assets/Content", "registry"),
    ("game/Assets/Resources", "resources"),
    ("game/Assets/Settings", "settings"),
    ("game/Assets/Editor", "editor"),
    ("tools", "tools"),
)


def _owner_of(relative: str) -> str:
    lowered = relative.lower()
    for prefix, owner in OWNER_RULES:
        if lowered.startswith(prefix.lower()):
            return owner
    return "other"


def _is_editor_path(path: Path) -> bool:
    return any(part == "Editor" or part.endswith(".Editor") for part in path.parts)


def _is_test_path(path: Path) -> bool:
    return any(part.lower() in ("tests", "test") or part.endswith(".Tests") for part in path.parts)


# ---------------------------------------------------------------------------
# line-scan rules
# ---------------------------------------------------------------------------

CANDIDATE_PATH_RE = re.compile(r"(?<![a-z0-9_])assets/art/candidates(?![a-z0-9_])")
CANDIDATE_PATH_BACKSLASH_RE = re.compile(r"(?<![a-z0-9_])assets\\art\\candidates(?![a-z0-9_])")
TEMP_CACHE_RE = re.compile(r"d18/cache|whalesislandreef[/\\]d18|appdata[/\\]local[/\\]whalesislandreef")
EXTERNAL_ARCHIVE_RE = re.compile(r"artifacts[/\\]external_sources")
ARCHIVE_TOKEN_RE = re.compile(r"[\w./:\\\-]+\.(?:zip|rar|7z|tar\.gz|tgz)\b", re.IGNORECASE)
PATH_CHAR_RE = re.compile(r"[/\\:]")
CANDIDATE_KEY_PREFIX_RE = re.compile(r"(?<![a-z0-9_])candidate_[a-z0-9_]+")

FIX_DIRECTIONS = {
    "candidate_path_reference": "运行时不得引用 Assets/Art/Candidates 路径；改为稳定 content_id/ResourceKey（L16/L46 接口）。人工修复。",
    "temp_cache_reference": "运行时不得引用临时下载缓存；缓存只由来源登记任务（L21—L24）记录。人工修复。",
    "archive_reference": "运行时不得引用下载归档（zip/rar/7z）；归档不参与业务引用。人工修复。",
    "external_archive_path": "运行时不得引用外部归档目录（artifacts/external_sources）。人工修复。",
    "candidate_key_as_business_key": "业务数据/代码把候选键（candidate_ 前缀）当正式引用；必须改用稳定 content_id/ResourceKey（L16 只允许 candidate_ 作 deprecated alias）。人工修复。",
    "key_position_path_reference": "JSON 键位置（id/content_id/resource_key/asset_id）必须是 snake_case 业务键，禁止候选路径/临时目录/显示名。人工修复。",
    "display_name_as_key": "JSON 键位置使用了显示名（含空格/大写）；必须用 snake_case 业务键 + display_name_key 引用文案。人工修复。",
}

RUNTIME_BLOCKING_RULES = frozenset(FIX_DIRECTIONS)

# candidate_ 前缀在 C# 行内是否疑似资源/业务键引用（需资源上下文词，避免状态码如
# candidate_required/candidate_present 误报；JSON 由引用字段结构规则精确处理）。
RESOURCE_CONTEXT_RE = re.compile(r"asset|resource|content_id|target_id|model_ref|prefab|\bkey\b|ref\b|\bpath\b|\bLoad\b", re.IGNORECASE)


def _classify_line(stripped: str) -> str:
    if stripped.startswith(("//", "/*", "*")):
        return "documentation"
    return "code"


def _match_line_rules(normalized: str, allow_candidate_key: bool = True) -> list[str]:
    rules: list[str] = []
    if CANDIDATE_PATH_RE.search(normalized) or CANDIDATE_PATH_BACKSLASH_RE.search(normalized):
        rules.append("candidate_path_reference")
    if TEMP_CACHE_RE.search(normalized):
        rules.append("temp_cache_reference")
    if EXTERNAL_ARCHIVE_RE.search(normalized):
        rules.append("external_archive_path")
    for token in ARCHIVE_TOKEN_RE.findall(normalized):
        if PATH_CHAR_RE.search(token):
            rules.append("archive_reference")
            break
    # JSON 的 candidate_ 值由结构规则（引用字段）精确处理；行级疑似仅用于 C#。
    if allow_candidate_key and CANDIDATE_KEY_PREFIX_RE.search(normalized) and RESOURCE_CONTEXT_RE.search(normalized):
        rules.append("candidate_key_as_business_key")
    return rules


@dataclass
class Finding:
    file: str
    line: int
    rule: str
    severity: str
    classification: str
    owner: str
    snippet: str
    fix_direction: str


def _scan_text(path: Path, text: str, relative: str) -> list[Finding]:
    ext = path.suffix.lower()
    is_editor = _is_editor_path(path)
    is_test = _is_test_path(path)
    owner = _owner_of(relative)
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
        if in_block_comment or _classify_line(stripped) == "documentation":
            classification = "documentation"
        elif is_editor or in_editor_block:
            classification = "editor_only"
        elif is_test:
            classification = "test"
        else:
            classification = "runtime"
        for rule in _match_line_rules(lower, allow_candidate_key=(ext == ".cs")):
            # 行级 candidate_key 命中只是疑似（可能为状态码）；P1 提示人工复核。
            severity = "P0" if classification == "runtime" and rule != "candidate_key_as_business_key" else ("P1" if classification == "runtime" else "INFO")
            findings.append(
                Finding(
                    file=relative,
                    line=index,
                    rule=rule,
                    severity=severity,
                    classification=classification,
                    owner=owner,
                    snippet=stripped[:160],
                    fix_direction=FIX_DIRECTIONS[rule],
                )
            )
    return findings


# ---------------------------------------------------------------------------
# JSON structural rules (key positions)
# ---------------------------------------------------------------------------

KEY_FIELDS = ("id", "content_id", "resource_key", "asset_id", "canonical_asset_id", "batch_id")
# 业务引用字段：值必须是稳定键；candidate_ 前缀值 = 业务数据直接引用候选资源（真实违规，P0）。
REFERENCE_FIELDS = (
    "sky_reference_asset", "model_ref", "resource_path", "runtime_prefab_path", "target_id",
    "asset_refs", "reference", "asset_path", "prefab_path", "audio_ref", "material_ref", "visual_ref",
)
SNAKE_CASE_RE = re.compile(r"^[a-z0-9_]+$")
PATHY_VALUE_RE = re.compile(r"[/\\]|\.(?:fbx|png|jpg|jpeg|zip|rar|7z|blend|gltf|prefab|unity|obj|tga|ogg|wav)\b", re.IGNORECASE)
DISPLAY_NAME_RE = re.compile(r"\s|[A-Z]")
CANDIDATE_KEY_VALUE_RE = re.compile(r"^candidate_[a-z0-9_]+$")


def _json_key_findings(path: Path, text: str, relative: str) -> list[Finding]:
    try:
        data = json.loads(text)
    except (json.JSONDecodeError, ValueError):
        return []
    lines = text.splitlines()
    owner = _owner_of(relative)
    findings: list[Finding] = []

    def line_of(value: str) -> int:
        for index, raw in enumerate(lines, start=1):
            if value in raw:
                return index
        return 0

    def walk(node: Any) -> None:
        if isinstance(node, dict):
            for key, value in node.items():
                if key in KEY_FIELDS and isinstance(value, str) and value:
                    if "Candidates" in value or PATHY_VALUE_RE.search(value):
                        findings.append(
                            Finding(
                                file=relative, line=line_of(value), rule="key_position_path_reference",
                                severity="P0", classification="runtime", owner=owner,
                                snippet=value[:160], fix_direction=FIX_DIRECTIONS["key_position_path_reference"],
                            )
                        )
                    elif DISPLAY_NAME_RE.search(value):
                        findings.append(
                            Finding(
                                file=relative, line=line_of(value), rule="display_name_as_key",
                                severity="P0", classification="runtime", owner=owner,
                                snippet=value[:160], fix_direction=FIX_DIRECTIONS["display_name_as_key"],
                            )
                        )
                if key in REFERENCE_FIELDS and isinstance(value, str) and value:
                    if CANDIDATE_KEY_VALUE_RE.match(value) or "Candidates" in value:
                        findings.append(
                            Finding(
                                file=relative, line=line_of(value), rule="candidate_key_as_business_key",
                                severity="P0", classification="runtime", owner=owner,
                                snippet=value[:160], fix_direction=FIX_DIRECTIONS["candidate_key_as_business_key"],
                            )
                        )
                walk(value)
        elif isinstance(node, list):
            for item in node:
                walk(item)

    walk(data)
    return findings


# ---------------------------------------------------------------------------
# orchestrator
# ---------------------------------------------------------------------------


def scan_game_references(project_root: Path, roots: list[Path] | None = None, extensions: set[str] | None = None) -> dict[str, Any]:
    project_root = project_root.resolve()
    roots = roots or [(project_root / item).resolve() for item in DEFAULT_SCAN_ROOTS]
    extensions = extensions or set(DEFAULT_EXTENSIONS)
    findings: list[Finding] = []
    scanned_files: list[str] = []
    root_paths: list[str] = []
    for root in roots:
        if not root.is_dir():
            continue
        root_paths.append(str(root.relative_to(project_root)).replace("\\", "/"))
        for path in sorted(root.rglob("*")):
            if not path.is_file() or path.suffix.lower() not in extensions:
                continue
            relative = str(path.relative_to(project_root)).replace("\\", "/")
            # 排除判断只针对仓库内相对路径段（绝对路径中的 Temp 等系统段不得触发）。
            if any(part in DEFAULT_EXCLUDE_DIRS for part in Path(relative).parts):
                continue
            scanned_files.append(relative)
            try:
                text = path.read_text(encoding="utf-8", errors="replace")
            except OSError:
                continue
            findings.extend(_scan_text(path, text, relative))
            if path.suffix.lower() == ".json":
                findings.extend(_json_key_findings(path, text, relative))

    findings.sort(key=lambda f: (f.file, f.line, f.rule))
    rendered = [f.__dict__ for f in findings]
    runtime_findings = [f for f in findings if f.classification == "runtime"]
    # 只有 P0 实际运行时违规阻断；P1 疑似（人工复核）与 INFO（test/editor/注释）不阻断。
    blocking_findings = [f for f in runtime_findings if f.severity == "P0"]
    blocked = bool(blocking_findings)
    from collections import Counter
    return {
        "task": TASK,
        "tool": TOOL,
        "read_only": True,
        "status": "BLOCKED" if blocked else "PASS",
        "scan_roots": root_paths,
        "extensions": sorted(extensions),
        "scanned_files_count": len(scanned_files),
        "findings_count": len(rendered),
        "runtime_findings_count": len(runtime_findings),
        "blocking_findings_count": len(blocking_findings),
        "classification_counts": dict(sorted(Counter(f.classification for f in findings).items())),
        "owner_counts": dict(sorted(Counter(f.owner for f in findings).items())),
        "rule_counts": dict(sorted(Counter(f.rule for f in findings).items())),
        "findings": rendered,
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--scan-root", type=Path, action="append", default=[])
    parser.add_argument("--extensions", type=str, default=",".join(sorted(DEFAULT_EXTENSIONS)))
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    project_root = args.project_root.resolve()
    roots = [item.resolve() for item in args.scan_root] or None
    extensions = {item.strip().lower() for item in args.extensions.split(",") if item.strip()}
    result = scan_game_references(project_root, roots, extensions)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
