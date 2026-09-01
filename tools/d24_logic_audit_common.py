#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""可复核的 D24 纯逻辑审计原语。

审计脚本只读取工作树。每个审计都返回稳定排序的结构化证据，既报告
“工具能否复现”，也报告被审计对象是否真的满足条件；不会把 Unity
Player/PlayMode、视觉资源、性能、设备或网络状态提升为逻辑通过。
"""

from __future__ import annotations

import hashlib
import json
import re
from collections import Counter, defaultdict
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Any, Dict, Iterable, List, Sequence, Tuple


ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOTS = (
    ROOT / "game" / "Assets" / "Scripts" / "Systems",
    ROOT / "game" / "AgentCore",
)
CONTRACT_ROOT = ROOT / "contracts"
CONTENT_ROOT = ROOT / "game" / "Assets" / "Resources" / "ContentRegistry"
RESULT_ROOT = ROOT / "game" / "tools" / "results"

LOGIC_CHAPTERS = ("D18", "D19", "D20", "D21", "D22", "D23")
RUNTIME_TERMS = (
    "UnityEngine",
    "MonoBehaviour",
    "GameObject",
    "Transform",
    "Animator",
    "NavMesh",
    "Canvas",
    "EventSystem",
    "Prefab",
    "PlayMode",
    "Player",
)


def resolve_root(root: Path | str | None = None) -> Path:
    """返回审计根目录；CLI 的 ``--root`` 只影响本次调用。"""

    candidate = ROOT if root is None else Path(root)
    return candidate.expanduser().resolve()


def rel(path: Path, root: Path | str | None = None) -> str:
    base = resolve_root(root)
    try:
        return path.resolve().relative_to(base).as_posix()
    except ValueError:
        return path.as_posix().replace("\\", "/")


def iter_files(*roots: Path, suffixes: Sequence[str] = ()) -> Iterable[Path]:
    """按路径稳定遍历文件，并消除重叠根目录导致的重复。"""

    normalized = {item.lower() for item in suffixes}
    seen: set[Path] = set()
    for root in roots:
        root = Path(root)
        if not root.exists():
            continue
        candidates = [root] if root.is_file() else root.rglob("*")
        for path in sorted(candidates):
            if not path.is_file() or path in seen:
                continue
            if normalized and path.suffix.lower() not in normalized:
                continue
            seen.add(path)
            yield path


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8", errors="replace")


def sha256_file(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def manifest_digest(paths: Iterable[Path], root: Path | str | None = None) -> str:
    """对相对路径和文件内容做确定性摘要，不泄漏绝对工作树路径。"""

    entries = []
    for path in sorted({Path(item) for item in paths}):
        if path.is_file():
            entries.append(f"{rel(path, root)}\0{sha256_file(path)}\n")
    return hashlib.sha256("".join(entries).encode("utf-8")).hexdigest()


def load_json_files(
    root: Path,
    *,
    base_root: Path | str | None = None,
) -> Tuple[List[Tuple[Path, Any]], List[Dict[str, str]]]:
    records: List[Tuple[Path, Any]] = []
    errors: List[Dict[str, str]] = []
    paths = list(iter_files(Path(root), suffixes=(".json",)))
    for path in paths:
        try:
            records.append((path, json.loads(read_text(path))))
        except Exception as exc:  # pragma: no cover - defensive audit path
            errors.append({"file": rel(path, base_root), "error": str(exc)})
    return records, errors


def _mask_range(chars: List[str], start: int, end: int) -> None:
    for index in range(start, min(end, len(chars))):
        if chars[index] != "\n":
            chars[index] = " "


def strip_comments_and_strings(source: str) -> str:
    """屏蔽 C# 注释、普通/逐字/原始字符串和字符字面量。

    换行被保留，所以扫描结果的行号仍对应源文件。插值字符串整体屏蔽
    是有意的保守策略：D24 禁止规则只需要识别可执行代码中的危险调用，
    不把插值内部文本误作独立源码证据。
    """

    chars = list(source)
    length = len(source)
    index = 0

    while index < length:
        char = source[index]
        nxt = source[index + 1] if index + 1 < length else ""

        if char == "/" and nxt == "/":
            end = source.find("\n", index)
            _mask_range(chars, index, length if end < 0 else end)
            index = length if end < 0 else end
            continue

        if char == "/" and nxt == "*":
            end = source.find("*/", index + 2)
            end = length if end < 0 else end + 2
            _mask_range(chars, index, end)
            index = end
            continue

        if char not in ('"', "'"):
            index += 1
            continue

        quote = char
        raw_width = 0
        if quote == '"':
            width = 0
            while index + width < length and source[index + width] == '"':
                width += 1
            if width >= 3:
                raw_width = width

        verbatim = quote == '"' and (
            (index > 0 and source[index - 1] == "@")
            or (index > 1 and source[index - 2:index] in ("$@", "@$"))
        )

        cursor = index + (raw_width or 1)
        if raw_width:
            closing = '"' * raw_width
            while cursor < length:
                found = source.find(closing, cursor)
                if found < 0:
                    cursor = length
                    break
                cursor = found + raw_width
                break
        else:
            while cursor < length:
                current = source[cursor]
                if current == "\n" and not verbatim:
                    cursor += 1
                    break
                if current == quote:
                    if verbatim and cursor + 1 < length and source[cursor + 1] == quote:
                        cursor += 2
                        continue
                    cursor += 1
                    break
                if not verbatim and current == "\\":
                    cursor += 2
                else:
                    cursor += 1

        _mask_range(chars, index, cursor)
        index = cursor

    return "".join(chars)


def _root_source_paths(root: Path) -> Tuple[Path, ...]:
    return (
        root / "game" / "Assets" / "Scripts" / "Systems",
        root / "AgentCore",
        root / "game" / "AgentCore",
    )


def scan_csharp(
    patterns: Sequence[Tuple[str, str]],
    roots: Sequence[Path] | None = None,
    *,
    root: Path | str | None = None,
) -> Dict[str, Any]:
    base = resolve_root(root)
    scan_roots = tuple(roots) if roots is not None else _root_source_paths(base)
    files = list(iter_files(*scan_roots, suffixes=(".cs",)))
    compiled = [(rule_id, re.compile(pattern, re.IGNORECASE)) for rule_id, pattern in patterns]
    hits: List[Dict[str, Any]] = []
    rule_counts: Counter[str] = Counter()
    for path in files:
        cleaned = strip_comments_and_strings(read_text(path))
        for line_no, line in enumerate(cleaned.splitlines(), start=1):
            for rule_id, pattern in compiled:
                match = pattern.search(line)
                if match:
                    rule_counts[rule_id] += 1
                    hits.append(
                        {
                            "rule_id": rule_id,
                            "file": rel(path, base),
                            "line": line_no,
                            "matched": match.group(0).strip(),
                        }
                    )
    return {
        "scanned_files": [rel(path, base) for path in files],
        "scanned_file_count": len(files),
        "input_digest": manifest_digest(files, base),
        "rule_counts": dict(sorted(rule_counts.items())),
        "hits": hits,
    }


def result(
    name: str,
    checks: List[Dict[str, Any]],
    *,
    warnings: List[Dict[str, Any]] | None = None,
    evidence: Dict[str, Any] | None = None,
    blockers: List[Dict[str, Any]] | None = None,
) -> Dict[str, Any]:
    failures = [item for item in checks if item.get("status") not in ("PASS", "WARN")]
    return {
        "schema_version": "d24.logic-audit.v2",
        "audit_name": name,
        "status": "PASS" if not failures else "FAIL",
        "passed": not failures,
        "checks": checks,
        "warnings": warnings or [],
        "blockers": blockers or [],
        "evidence": evidence or {},
        "runtime_deferred": [
            "Unity Player/PlayMode、画面、模型、Prefab、输入、物理、性能、真机、网络和发行未由 D24 逻辑审计放行。"
        ],
    }


def check_json_parse(root: Path | str | None = None) -> Dict[str, Any]:
    base = resolve_root(root)
    records: List[Tuple[Path, Any]] = []
    errors: List[Dict[str, str]] = []
    input_paths: List[Path] = []
    for target in (base / "contracts", base / "game" / "Assets" / "Resources" / "ContentRegistry"):
        root_records, root_errors = load_json_files(target, base_root=base)
        records.extend(root_records)
        errors.extend(root_errors)
        input_paths.extend(path for path, _ in root_records)
        input_paths.extend(base / item["file"] for item in root_errors)
    return {
        "id": "json_parse",
        "status": "PASS" if not errors else "FAIL",
        "files": len(records),
        "input_digest": manifest_digest(input_paths, base),
        "errors": errors,
    }


def collect_result_statuses(root: Path | str | None = None) -> Dict[str, Any]:
    base = resolve_root(root)
    statuses: Dict[str, str] = {}
    occurrences: Dict[str, List[str]] = defaultdict(list)
    malformed: List[Dict[str, str]] = []
    files: List[Path] = []
    for chapter in LOGIC_CHAPTERS:
        chapter_root = base / "game" / "tools" / "results" / chapter
        for path in iter_files(chapter_root, suffixes=(".json",)):
            if "_LOGIC_RESULT" not in path.name:
                continue
            files.append(path)
            try:
                payload = json.loads(read_text(path))
                if not isinstance(payload, dict):
                    raise ValueError("result payload must be an object")
                task_id = str(payload.get("task_id", "")).strip()
                status = str(payload.get("status", "")).strip()
                if not task_id or not status:
                    raise ValueError("task_id and status are required")
                occurrences[task_id].append(rel(path, base))
                statuses.setdefault(task_id, status)
            except Exception as exc:
                malformed.append({"file": rel(path, base), "error": str(exc)})
    duplicates = {
        task_id: paths for task_id, paths in sorted(occurrences.items()) if len(paths) > 1
    }
    return {
        "statuses": dict(sorted(statuses.items())),
        "malformed": malformed,
        "duplicate_task_ids": duplicates,
        "result_file_count": len(files),
        "input_digest": manifest_digest(files, base),
    }


def _top_level_objects(payload: Any) -> Iterable[Dict[str, Any]]:
    if isinstance(payload, dict):
        yield payload
    elif isinstance(payload, list):
        yield from (item for item in payload if isinstance(item, dict))


def duplicate_values(
    records: Iterable[Tuple[Path, Any]],
    keys: Sequence[str],
    *,
    base_root: Path | str | None = None,
) -> Dict[str, List[Dict[str, str]]]:
    locations: Dict[str, List[Dict[str, str]]] = defaultdict(list)
    for path, payload in records:
        for obj in _top_level_objects(payload):
            for key in keys:
                value = obj.get(key)
                if isinstance(value, str) and value.strip():
                    locations[f"{key}:{value}"].append({"file": rel(path, base_root), "key": key})
    return {key: value for key, value in sorted(locations.items()) if len(value) > 1}


def stable_key(value: str) -> bool:
    return bool(re.fullmatch(r"[a-z][a-z0-9]*(?:_[a-z0-9]+)*", value or ""))


def count_tokens(root: Path, tokens: Sequence[str]) -> Counter[str]:
    counts: Counter[str] = Counter()
    for path in iter_files(root, suffixes=(".cs", ".json", ".md")):
        text = read_text(path)
        for token in tokens:
            counts[token] += text.count(token)
    return counts


def _nonempty(value: Any) -> bool:
    if value is None:
        return False
    if isinstance(value, str):
        return bool(value.strip())
    if isinstance(value, (list, tuple, dict, set)):
        return bool(value)
    return True


def _has_any(obj: Dict[str, Any], keys: Sequence[str]) -> bool:
    return any(_nonempty(obj.get(key)) for key in keys)


def _entry_records(base: Path) -> Tuple[List[Tuple[Path, Any]], List[Dict[str, str]]]:
    entries = base / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries"
    return load_json_files(entries, base_root=base)


def _audit_content_coverage(base: Path) -> Dict[str, Any]:
    records, errors = _entry_records(base)
    missing: Dict[str, List[str]] = defaultdict(list)
    invalid_ids: List[str] = []
    ids: List[str] = []
    for path, payload in records:
        objects = list(_top_level_objects(payload))
        for index, item in enumerate(objects):
            identity = next((item.get(key) for key in ("id", "content_id", "resource_key") if _nonempty(item.get(key))), None)
            label = rel(path, base) if len(objects) == 1 else f"{rel(path, base)}#{index + 1}"
            if not isinstance(identity, str) or not identity.strip():
                missing["identity"].append(label)
            else:
                ids.append(identity.strip())
                if not stable_key(identity.strip()):
                    invalid_ids.append(label)
            if not _has_any(item, ("owner_system", "owner", "data_owner", "system_owner")):
                missing["owner"].append(label)
            if not _has_any(item, ("source", "source_refs", "source_note", "references", "review_evidence_refs")):
                missing["provenance"].append(label)
            if not _has_any(item, ("test_id", "test_cases", "tests", "validation", "evidence_paths")):
                missing["validation"].append(label)
            if not _has_any(item, ("status", "phase", "deprecated", "lifecycle")):
                missing["lifecycle"].append(label)

    registry_path = base / "game" / "Assets" / "Resources" / "ContentRegistry" / "registry.json"
    registry_roots: set[str] = set()
    registry_error = ""
    if registry_path.exists():
        try:
            registry = json.loads(read_text(registry_path))
            registry_roots = {str(item) for item in registry.get("roots", []) if isinstance(item, str)}
        except Exception as exc:  # pragma: no cover - covered by parse audit
            registry_error = str(exc)
    else:
        registry_error = "registry.json is missing"
    id_set = set(ids)
    unregistered = sorted(id_set - registry_roots)
    stale_registry = sorted(registry_roots - id_set)
    failed = bool(errors or not records or missing or invalid_ids or registry_error or unregistered or stale_registry)
    return {
        "id": "content_catalog_shape_and_coverage",
        "status": "FAIL" if failed else "PASS",
        "files": len(records),
        "entries": len(ids),
        "json_errors": errors,
        "missing_by_requirement": {key: values[:20] for key, values in sorted(missing.items())},
        "missing_counts": {key: len(values) for key, values in sorted(missing.items())},
        "invalid_id_examples": invalid_ids[:20],
        "registry_unregistered_examples": unregistered[:20],
        "registry_stale_examples": stale_registry[:20],
        "registry_error": registry_error,
        "input_digest": manifest_digest([path for path, _ in records] + [registry_path], base),
    }


def _audit_unique_ids(base: Path) -> Dict[str, Any]:
    records, errors = _entry_records(base)
    duplicates = duplicate_values(records, ("id", "content_id", "resource_key", "event_id"), base_root=base)
    return {
        "id": "content_key_uniqueness",
        "status": "PASS" if not errors and not duplicates and records else "FAIL",
        "json_errors": errors,
        "entry_files": len(records),
        "duplicate_count": len(duplicates),
        "duplicates": duplicates,
        "input_digest": manifest_digest([path for path, _ in records], base),
    }


def _active_logic_roots(base: Path) -> Tuple[Path, ...]:
    systems = base / "game" / "Assets" / "Scripts" / "Systems"
    return tuple(systems / chapter for chapter in ("D18", "D19", "D20", "D21", "D22", "D23") if (systems / chapter).exists())


def _audit_ownership(base: Path) -> Dict[str, Any]:
    scan = scan_csharp(
        (
            ("foreign_singleton_write", r"\b(?:GameState|PlayerState)\s*\.\s*Instance\s*\.\s*[A-Za-z_]\w*\s*(?:=|\+=|-=|\+\+|--)(?!=)"),
            ("private_foreign_write", r"\bD(?:19|20|21|22)\w*\s*\.\s*_[A-Za-z_]\w*\s*(?:=|\+=|-=|\+\+|--)(?!=)"),
            ("presentation_write", r"\b(?:Presentation|ReadModel|ViewModel)\w*\s*\.\s*[A-Za-z_]\w*\s*(?:=|\+=|-=|\+\+|--)(?!=)"),
        ),
        roots=_active_logic_roots(base),
        root=base,
    )
    legal_projection_hits: List[Dict[str, Any]] = []
    unapproved_hits: List[Dict[str, Any]] = []
    for hit in scan["hits"]:
        # A save migration is the owner of its own nested save projection. In
        # particular, D20CharacterSaveMigration intentionally normalizes the
        # D20 identity-presentation block; this is not a presentation system
        # mutating another system's live state. Keep the finding as evidence
        # instead of hiding it with a path exclusion.
        if (
            hit["rule_id"] == "presentation_write"
            and hit["file"] == "game/Assets/Scripts/Systems/D20/D20CharacterSaveMigration.cs"
        ):
            legal_projection_hits.append({
                **hit,
                "classification": "LEGAL_OWNER_WRITE",
                "reason": "D20CharacterSaveMigration owns and normalizes D20 identity presentation inside its save projection.",
            })
        else:
            unapproved_hits.append({**hit, "classification": "UNAPPROVED_OWNER_WRITE"})
    return {
        "id": "single_writer_scan",
        "status": "PASS" if not unapproved_hits else "FAIL",
        "scanned_files": scan["scanned_files"],
        "scanned_file_count": scan["scanned_file_count"],
        "input_digest": scan["input_digest"],
        "rule_counts": scan["rule_counts"],
        "hits": unapproved_hits,
        "legal_projection_hits": legal_projection_hits,
        "raw_hit_count": len(scan["hits"]),
    }


def _canonical_contract_paths(base: Path) -> List[Path]:
    root = base / "contracts"
    paths = []
    for path in iter_files(root, suffixes=(".json", ".yaml", ".yml")):
        parts = {part.lower() for part in path.relative_to(root).parts}
        if "test-fixtures" in parts or "tests" in parts or "__pycache__" in parts:
            continue
        paths.append(path)
    return paths


def _audit_contracts(base: Path) -> Dict[str, Any]:
    paths = _canonical_contract_paths(base)
    records: List[Tuple[Path, Any]] = []
    errors: List[Dict[str, str]] = []
    for path in paths:
        if path.suffix.lower() != ".json":
            continue
        try:
            payload = json.loads(read_text(path))
            records.append((path, payload))
        except Exception as exc:
            errors.append({"file": rel(path, base), "error": str(exc)})

    schema_locations: Dict[str, List[str]] = defaultdict(list)
    missing_shape: List[str] = []
    for path, payload in records:
        if not isinstance(payload, dict):
            missing_shape.append(rel(path, base))
            continue
        schema_id = payload.get("$id")
        if not isinstance(schema_id, str) or not schema_id.strip():
            missing_shape.append(rel(path, base))
        else:
            schema_locations[schema_id].append(rel(path, base))
    duplicate_ids = {key: value for key, value in sorted(schema_locations.items()) if len(value) > 1}
    yaml_paths = [path for path in paths if path.suffix.lower() in (".yaml", ".yml")]
    yaml_shape: List[str] = []
    for path in yaml_paths:
        text = read_text(path)
        if not re.search(r"(?m)^\s*openapi\s*:", text) or not re.search(r"(?m)^\s*info\s*:", text):
            yaml_shape.append(rel(path, base))
    failed = bool(errors or missing_shape or duplicate_ids or yaml_shape or not paths)
    return {
        "id": "contract_json_yaml_compatibility",
        "status": "FAIL" if failed else "PASS",
        "files": len(paths),
        "json_files": len(records),
        "yaml_files": len(yaml_paths),
        "json_errors": errors,
        "missing_schema_shape": missing_shape,
        "duplicate_schema_ids": duplicate_ids,
        "invalid_openapi_files": yaml_shape,
        "input_digest": manifest_digest(paths, base),
    }


def _source_pattern_check(base: Path, audit_id: str, requirements: Sequence[Tuple[str, Sequence[str]]]) -> Dict[str, Any]:
    missing_files: List[str] = []
    missing_patterns: Dict[str, List[str]] = {}
    input_paths: List[Path] = []
    for relative, patterns in requirements:
        path = base / relative
        if not path.exists():
            missing_files.append(relative)
            continue
        input_paths.append(path)
        cleaned = strip_comments_and_strings(read_text(path))
        absent = [pattern for pattern in patterns if not re.search(pattern, cleaned, re.IGNORECASE | re.MULTILINE)]
        if absent:
            missing_patterns[relative] = absent
    failed = bool(missing_files or missing_patterns)
    return {
        "id": audit_id,
        "status": "FAIL" if failed else "PASS",
        "missing_files": missing_files,
        "missing_patterns": missing_patterns,
        "input_files": [rel(path, base) for path in input_paths],
        "input_digest": manifest_digest(input_paths, base),
    }


def _audit_determinism(base: Path) -> Dict[str, Any]:
    scan = scan_csharp(
        (
            ("wall_clock", r"\b(?:DateTime|DateTimeOffset)\s*\.\s*(?:Now|UtcNow)\b"),
            ("uncontrolled_random", r"\b(?:new\s+Random\s*\(|Guid\s*\.\s*NewGuid\s*\(|Environment\s*\.\s*TickCount)"),
            ("blocking_sleep", r"\b(?:Thread\s*\.\s*Sleep|Task\s*\.\s*Delay)\b"),
        ),
        roots=_active_logic_roots(base),
        root=base,
    )
    return {"id": "deterministic_time_random_scan", "status": "PASS" if not scan["hits"] else "FAIL", **scan}


def _audit_asset_provenance(base: Path) -> Dict[str, Any]:
    statuses = collect_result_statuses(base)
    required = ("D18-L50", "D18-L53")
    missing = [task for task in required if statuses["statuses"].get(task) != "LOGIC_PASS"]
    return {
        "id": "d18_logic_prerequisite",
        "status": "PASS" if not missing and not statuses["malformed"] and not statuses["duplicate_task_ids"] else "FAIL",
        "required_tasks": list(required),
        "missing_or_not_pass": missing,
        "malformed": statuses["malformed"],
        "duplicate_task_ids": statuses["duplicate_task_ids"],
        "result_file_count": statuses["result_file_count"],
        "input_digest": statuses["input_digest"],
    }


def _audit_test_matrix(base: Path) -> Dict[str, Any]:
    statuses = collect_result_statuses(base)
    chapter_counts = {
        chapter: sum(1 for task in statuses["statuses"] if task.startswith(chapter + "-"))
        for chapter in LOGIC_CHAPTERS
    }
    missing_chapters = [chapter for chapter, count in chapter_counts.items() if count == 0]
    failed = bool(statuses["malformed"] or statuses["duplicate_task_ids"] or missing_chapters)
    return {
        "id": "logic_result_matrix",
        "status": "FAIL" if failed else "PASS",
        "result_count": len(statuses["statuses"]),
        "chapter_counts": chapter_counts,
        "missing_chapters": missing_chapters,
        "malformed": statuses["malformed"],
        "duplicate_task_ids": statuses["duplicate_task_ids"],
        "input_digest": statuses["input_digest"],
    }


def run_named_audit(kind: str, root: Path | str | None = None) -> Dict[str, Any]:
    """执行 D24-L02—L16 的一个审计。"""

    base = resolve_root(root)
    if kind == "content_catalog_coverage":
        return result("D24 content catalog coverage", [_audit_content_coverage(base)])
    if kind == "content_id_uniqueness":
        return result("D24 content id uniqueness", [_audit_unique_ids(base)])
    if kind == "data_ownership":
        return result("D24 data ownership", [_audit_ownership(base)])
    if kind == "contract_compatibility":
        return result("D24 contract compatibility", [_audit_contracts(base), check_json_parse(base)])
    if kind == "save_migration":
        return result(
            "D24 save migration coverage",
            [_source_pattern_check(base, "save_migration_sources", (
                ("game/Assets/Scripts/Systems/D19/D19PortSaveMigration.cs", (r"SupportedSchemaVersion", r"Migrate\s*\(")),
                ("game/Assets/Scripts/Systems/D20/D20CharacterSaveMigration.cs", (r"SupportedSchemaVersion", r"Migrate\s*\(", r"Validate\s*\(")),
                ("game/Assets/Scripts/Systems/D21/D21ShipSaveProjection.cs", (r"CurrentSchemaVersion", r"CreateDefault\s*\(", r"Validate\s*\(")),
            ))],
        )
    if kind == "deterministic_seed":
        return result("D24 deterministic seed", [_audit_determinism(base)])
    if kind == "currency_invariant":
        return result(
            "D24 currency invariant coverage",
            [_source_pattern_check(base, "currency_owner_sources", (
                ("game/Assets/Scripts/Systems/D3/TradeEconomySystem.cs", (r"D3InventorySystem", r"SaveRevision", r"Try(Add|Remove)Cargo")),
                ("game/Assets/Scripts/Systems/D13/D13FinanceSystem.cs", (r"CashBalance", r"ApplyCash", r"SaveRevision")),
                ("game/Assets/Scripts/Systems/D14/D14RewardContracts.cs", (r"Reward|Currency|Amount",)),
            ))],
        )
    if kind == "inventory_invariant":
        return result(
            "D24 inventory invariant coverage",
            [_source_pattern_check(base, "inventory_owner_sources", (
                ("game/Assets/Scripts/Systems/D3/D3InventorySystem.cs", (r"CanAddCargo|TryAddCargo", r"TryRemoveCargo", r"UsedWeight|UsedVolume")),
                ("game/Assets/Scripts/Systems/D21/D21CargoCapacityRules.cs", (r"Capacity|Weight|Volume", r"Validate|Can")),
                ("game/Assets/Scripts/Systems/D21/D21ProvisionConsumptionSystem.cs", (r"Consume|Consumed", r"Validate|Apply")),
            ))],
        )
    if kind == "event_integrity":
        return result(
            "D24 event integrity",
            [_source_pattern_check(base, "event_owner_sources", (
                ("game/Assets/Scripts/Systems/D21/D21VoyageEventContracts.cs", (r"EventId|Sequence|SaveRevision", r"Validate|Clone")),
                ("game/Assets/Scripts/Systems/D21/D21VoyageEventRouter.cs", (r"Route|Apply|Append", r"Idempot|Duplicate|EventId")),
                ("game/Assets/Scripts/Systems/D20/D20ActionEventRouter.cs", (r"Route|Dispatch|Apply", r"EventId|Sequence|SaveRevision")),
            ))],
        )
    if kind == "error_catalog":
        return result(
            "D24 error catalog",
            [_source_pattern_check(base, "error_catalog_sources", (
                ("game/Assets/Scripts/Systems/D19/D19PortFailureCatalog.cs", (r"Entries", r"TryResolve|Find", r"Loc|Localization")),
                ("game/Assets/Scripts/Systems/D22/D22FeedbackCatalog.cs", (r"Entries|GetEntries", r"TryGet|Create", r"Loc|Localization")),
            ))],
        )
    if kind == "localization":
        return result(
            "D24 localization keys",
            [_source_pattern_check(base, "localization_sources", (
                ("game/Assets/Scripts/Systems/D19/D19PortLocalizationKeys.cs", (r"public|internal", r"Migration|Port|Failure")),
                ("game/Assets/Scripts/Systems/D22/D22LocalizationKeyAudit.cs", (r"Key|Loc", r"Validate|Audit|Try")),
            ))],
        )
    if kind == "accessibility":
        return result(
            "D24 accessibility logic",
            [_source_pattern_check(base, "accessibility_sources", (
                ("game/Assets/Scripts/Systems/D22/D22AccessibilityReadModel.cs", (r"class|struct", r"Validate|Build|Create")),
                ("game/Assets/Scripts/Systems/D22/D22DisabledReasonResolver.cs", (r"class|static", r"Resolve|Reason|Disabled")),
            ))],
        )
    if kind == "asset_provenance":
        return result("D24 asset provenance", [_audit_asset_provenance(base)])
    if kind == "test_matrix":
        return result("D24 test matrix", [_audit_test_matrix(base)])
    if kind == "historical_debt":
        requirements = (
            ("docs/game/design/common/D20_RUNTIME_DEFERRED_REGISTER.md", (r"runtime|deferred|blocked",)),
            ("docs/game/design/common/D18_RUNTIME_DEFERRED_REGISTER.md", (r"runtime|deferred|blocked",)),
        )
        return result("D24 historical debt", [_source_pattern_check(base, "runtime_deferred_registers", requirements)])
    return result("D24 unknown audit", [{"id": "unknown_kind", "status": "FAIL", "kind": kind}])


def self_test() -> Dict[str, Any]:
    """不依赖仓库内容的 scanner/root 覆盖；供 CI 和复现审计调用。"""

    source = (
        "// DateTime.Now should be ignored\n"
        "var text = @\"Guid.NewGuid() // still text\";\n"
        "/* Task.Delay(1) */\n"
        "var value = DateTime.Now;\n"
        "var character = '\\'';\n"
    )
    masked = strip_comments_and_strings(source)
    checks = [
        {"id": "comments_and_strings_masked", "status": "PASS" if "DateTime.Now" not in masked.splitlines()[0] and "Guid.NewGuid" not in masked and "Task.Delay" not in masked else "FAIL"},
        {"id": "executable_code_retained", "status": "PASS" if "DateTime.Now" in masked.splitlines()[3] else "FAIL"},
    ]
    with TemporaryDirectory(prefix="d24-audit-selftest-") as directory:
        base = Path(directory)
        source_path = base / "Sample.cs"
        source_path.write_text(source, encoding="utf-8")
        scan = scan_csharp((("clock", r"DateTime\s*\.\s*Now"),), roots=(base,), root=base)
        checks.append({"id": "scanner_finds_executable_line", "status": "PASS" if len(scan["hits"]) == 1 and scan["hits"][0]["line"] == 4 else "FAIL", "scan": scan})
    return {"schema_version": "d24.logic-audit-self-test.v1", "passed": all(item["status"] == "PASS" for item in checks), "checks": checks}
