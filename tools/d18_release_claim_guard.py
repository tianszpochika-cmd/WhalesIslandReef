"""Release claim guard for D18 logic records and documents (D18-L18).

Scans manifests, logic catalogs and active D18 documents for overstep
claims: assets in Candidate / IntakeEligible / RuntimeRequested being
declared RuntimeApproved, ReleaseApproved, releasable or Steam Ready.

Rules:

- JSON records declaring ``state``/``review_status`` RuntimeApproved are
  always flagged (the logic phase cannot grant runtime approval).
- JSON records declaring ReleaseApproved are flagged unless the record
  carries a responsible human reviewer, an existing local release evidence
  path and a prior RuntimeApproved state (the only legal chain).
- Truthy approval claim fields (``release_approved``, ``runtime_approved``,
  ``steam_ready``, ``release_ready``, ``release_claim``,
  ``distribution_ready``, ``publish_ready``) and readiness claims
  (``local_development_ready``) on any scanned record are flagged.
  Falsy values (false / 0) are never claims.
- Markdown lines containing claim keywords are flagged unless the line is
  definitional or negated (``禁止``/``不表示``/``不得``/``没有任何``/``0 个``
  and similar), or describes the guard's own detection task (``检测``/
  ``越级``/``扫描``...).
- Historical (SUPERSEDED) files from the D18-L01 baseline section 4 are
  skipped entirely; historical results referenced by an active doc as a
  current PASS (``superseded_result_as_pass``) are flagged.

The guard is strictly read-only: it never writes approval fields, never
modifies source files or manifests, and never approves anything. The only
file it writes is the optional ``--output`` result JSON.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from pathlib import Path
from typing import Any, Iterable

SUPERSEDED_STEM_PREFIXES = (
    "d18_d20_r03",
    "d18_free_asset",
    "d18_unity_asset_import_pipeline",
    "d18_visual_art_direction_and_budget",
    "d18_placeholder_replacement_map",
    "d18_character_animal_asset_packet",
    "d18_environment_asset_packet",
    "d18_ship_building_prop_asset_packet",
    "d18_ui_audio_material_asset_packet",
)
SUPERSEDED_STEM_CONTAINS = ("d18_a0", "d18_r03", "d18_s01", "d18_s02")

CLAIM_FIELDS = (
    "release_approved",
    "runtime_approved",
    "steam_ready",
    "release_ready",
    "release_claim",
    "distribution_ready",
    "publish_ready",
)
READINESS_FIELDS = ("local_development_ready",)

CLAIM_KEYWORDS = (
    "ReleaseApproved",
    "RuntimeApproved",
    "Steam Ready",
    "SteamReady",
    "可发布",
    "发布就绪",
    "可上架",
    "Steam 就绪",
    "已批准发布",
    "发布通过",
)

SKIP_CONTEXTS = (
    "SUPERSEDED",
    "superseded",
    "历史证据",
    "历史结果",
    "历史诊断",
    "历史引用",
    "历史材料",
    "历史快照",
    "历史记录",
    "历史归档",
    "历史检查",
    "历史断言",
    "仅作历史",
    "只作历史",
    "不作为通过依据",
    "不作为",
    "不表示",
    "不证明",
    "不代表",
    "不等于",
    "不自动等于",
    "不自动",
    "不是",
    "不能",
    "不得",
    "不可",
    "不会",
    "无法",
    "尚未",
    "未完成",
    "未开始",
    "未通过",
    "未放行",
    "未闭合",
    "未执行",
    "未批准",
    "未实现",
    "没有",
    "没有任何",
    "无任何",
    "无持有",
    "均为 0",
    "当前 0",
    "持有者 0",
    "0 个",
    "=0",
    "延期",
    "待定",
    "BLOCKED",
    "PARTIAL",
    "PENDING",
    "PLANNED",
    "IN_PROGRESS",
    "RUNTIME_DEFERRED",
    "禁止",
    "从不",
    "绝不",
    "不产生",
    "不授予",
    "不修改",
    "不批准",
    "不升级",
    "不视为",
    "不允许",
    "不宣称",
    "不改变",
    "不增加",
    "不伪造",
    "不删除",
    "检测",
    "扫描",
    "识别",
    "拦截",
    "阻止",
    "越级",
    "伪造",
    "拒绝",
    "视为",
    "防线",
    "守卫",
    "校验",
    "验证",
    "审计",
    "检查",
    "未来 Runtime",
    "未来运行时",
    "本阶段禁止",
    "阶段禁止",
    "仅人类",
    "责任人类",
    "人类审核",
    "人类复核",
    "人类批准",
    "不可发布",
    "不能据此",
    "不能把",
    "不放行",
    "不满足",
    "不得宣称",
    "不得宣布",
    "不得标记",
    "没有把",
    "不能证明",
    "不能单独证明",
    "无法证明",
    "未授予",
    "仍为 0",
    "未统计",
    "只输出",
    "无直达路径",
    "唯一路径",
    "断言",
    "无出边",
    "互斥",
    "展开",
    "事件字典",
    "状态枚举",
    "条件证据",
    "必填",
    "仅当",
    "授予",
    "不存在",
    "视为伪造",
    "仅描述",
    "仅声明",
    "本文只",
    "本章只",
    "只表示",
    "只证明",
    "仅是",
)

HISTORICAL_TASK_RE = re.compile(r"(D18-[AS]\d|D18-R03|RERUN\d{1,2}|D19-R0\d)")
PASS_RE = re.compile(r"\bPASS\b|通过")

STATE_NAMES = (
    "Candidate",
    "IntakeEligible",
    "RuntimeRequested",
    "RuntimeApproved",
    "ReleaseApproved",
    "Rejected",
    "Revoked",
)
HEADING_RE = re.compile(r"^#{1,6}\s+\S")
PROHIBITION_RE = re.compile(r"禁止|不得|不输出|不能|不作为|不允许|不证明|不做|不宣称|不产生|不授予|无法|不可|不能当作|不表示|不代表|不自动")


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _stem(path: Path) -> str:
    return path.stem.casefold()


def is_superseded_path(path: Path) -> bool:
    """Mirror the D18-L01 baseline section 4 SUPERSEDED isolation list."""

    rel = str(path).replace("\\", "/").casefold()
    if "/archive/" in rel or rel.startswith("archive/"):
        return True
    parts = [part for part in rel.split("/") if part]
    if "handoffs" in parts:
        for part in parts:
            if part.casefold().startswith("d18-a") or part.casefold().startswith("d18-s") or part.casefold().startswith("d18-r03-"):
                return True
    stem = _stem(path)
    if stem.startswith(SUPERSEDED_STEM_PREFIXES):
        return True
    if any(token in stem for token in SUPERSEDED_STEM_CONTAINS):
        return True
    return False


def _state_norm(value: Any) -> str:
    if not isinstance(value, str):
        return ""
    names = {
        "candidate": "Candidate",
        "intakeeligible": "IntakeEligible",
        "runtimerequested": "RuntimeRequested",
        "runtimeapproved": "RuntimeApproved",
        "releaseapproved": "ReleaseApproved",
        "rejected": "Rejected",
        "revoked": "Revoked",
    }
    return names.get(value.strip().casefold().replace("_", ""), "")


def _truthy(value: Any) -> bool:
    if isinstance(value, bool):
        return value
    if isinstance(value, (int, float)):
        return value > 0
    if isinstance(value, str):
        return value.strip().casefold() in {"true", "yes", "approved", "ready", "passed", "granted", "ok", "y", "1"}
    return False


def _has_human_release_evidence(project_root: Path, record: dict[str, Any]) -> bool:
    if not _text(record.get("human_reviewer")):
        return False
    if _state_norm(record.get("previous_review_status") or record.get("previous_state")) != "RuntimeApproved":
        return False
    evidence = _text(record.get("release_evidence_path"))
    if not evidence:
        return False
    candidate = Path(evidence.replace("\\", "/"))
    if candidate.is_absolute():
        return False
    resolved = (project_root / candidate).resolve()
    try:
        resolved.relative_to(project_root.resolve())
    except ValueError:
        return False
    return resolved.is_file()


def _scan_json_document(project_root: Path, path: Path, issues: list[dict[str, Any]]) -> None:
    rel = str(path.relative_to(project_root)) if path.resolve().is_relative_to(project_root.resolve()) else str(path)
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError:
        issues.append({"code": "json_file_missing", "severity": "error", "file": rel, "message": "file disappeared while scanning"})
        return
    except json.JSONDecodeError as exc:
        issues.append({"code": "json_parse_error", "severity": "error", "file": rel, "message": f"line {exc.lineno}: {exc.msg}"})
        return
    except OSError as exc:
        issues.append({"code": "json_read_error", "severity": "error", "file": rel, "message": str(exc)})
        return

    def walk(node: Any, json_path: str) -> None:
        if isinstance(node, dict):
            state = _state_norm(node.get("state") or node.get("review_status"))
            if state == "RuntimeApproved":
                issues.append(
                    {"code": "json_approval_state_hold", "severity": "error", "file": rel, "path": json_path,
                     "field": "state/review_status",
                     "message": "RuntimeApproved 状态持有在本逻辑阶段属于伪造：唯一路径必须经 IntakeEligible → RuntimeRequested → Runtime 阶段门禁"}
                )
            elif state == "ReleaseApproved" and not _has_human_release_evidence(project_root, node):
                issues.append(
                    {"code": "json_release_approved_without_human", "severity": "error", "file": rel, "path": json_path,
                     "field": "state/review_status",
                     "message": "ReleaseApproved 声明缺少责任人类审核者/发布证据文件/前置 RuntimeApproved 链，属于越级声明"}
                )
            for field in CLAIM_FIELDS + READINESS_FIELDS:
                if field in node and _truthy(node[field]):
                    code = "json_readiness_claim" if field in READINESS_FIELDS else "json_approval_claim_field"
                    issues.append(
                        {"code": code, "severity": "error", "file": rel, "path": json_path, "field": field,
                         "message": f"字段 {field}={node[field]!r} 在纯逻辑阶段构成越级批准/就绪声明"}
                    )
            for key, value in node.items():
                walk(value, f"{json_path}/{key}")
        elif isinstance(node, list):
            for index, value in enumerate(node):
                walk(value, f"{json_path}[{index}]")

    walk(data, "/")


def _line_has_claim(line: str) -> bool:
    lowered = line.casefold()
    return any(keyword.casefold() in lowered for keyword in CLAIM_KEYWORDS)


def _line_is_skipped(line: str) -> bool:
    return any(context in line for context in SKIP_CONTEXTS)


def _line_is_state_enumeration(line: str) -> bool:
    return sum(1 for name in STATE_NAMES if name in line) >= 3


def _scan_doc(project_root: Path, path: Path, issues: list[dict[str, Any]]) -> None:
    rel = str(path.relative_to(project_root)) if path.resolve().is_relative_to(project_root.resolve()) else str(path)
    try:
        lines = path.read_text(encoding="utf-8").splitlines()
    except OSError as exc:
        issues.append({"code": "doc_read_error", "severity": "error", "file": rel, "message": str(exc)})
        return
    in_fence = False
    under_prohibition = False
    for number, line in enumerate(lines, start=1):
        stripped = line.strip()
        if stripped.startswith("```"):
            in_fence = not in_fence
            continue
        if in_fence:
            continue
        if not stripped:
            continue
        if PROHIBITION_RE.search(stripped):
            under_prohibition = True
            continue
        if under_prohibition:
            if stripped[0] in "-*>|" or stripped[0].isdigit():
                continue
            under_prohibition = False
        if _line_is_skipped(stripped):
            continue
        if HEADING_RE.match(stripped) or "→" in stripped or "->" in stripped or _line_is_state_enumeration(stripped):
            continue
        if _line_has_claim(stripped):
            issues.append(
                {"code": "doc_overstep_claim", "severity": "error", "file": rel, "line": number,
                 "message": stripped[:300]}
            )
            continue
        if HISTORICAL_TASK_RE.search(stripped) and PASS_RE.search(stripped):
            issues.append(
                {"code": "superseded_result_as_pass", "severity": "error", "file": rel, "line": number,
                 "message": stripped[:300]}
            )


def _collect_doc_files(project_root: Path) -> tuple[list[Path], int]:
    files: list[Path] = []
    skipped = 0
    direct = [
        "docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md",
        "docs/game/AGENT_DEVELOPMENT_CHAPTER_10_D18.md",
        "docs/game/AGENT_DEVELOPMENT_CHAPTER_10_D18_PROMPTS.md",
        "docs/game/AGENT_DEVELOPMENT_TASKS.md",
        "docs/game/LOGIC_TASK_STATUS.md",
    ]
    for rel in direct:
        path = project_root / rel
        if path.is_file():
            files.append(path)
    common = project_root / "docs/game/design/common"
    if common.is_dir():
        for path in sorted(common.glob("*.md")):
            if path.stem.startswith("D18") and not is_superseded_path(path):
                files.append(path)
            else:
                skipped += 1
    assets = common / "assets"
    if assets.is_dir():
        for path in sorted(assets.rglob("*.md")):
            if is_superseded_path(path):
                skipped += 1
            else:
                files.append(path)
    handoffs_d18 = common / "handoffs/D18"
    if handoffs_d18.is_dir():
        for path in sorted(handoffs_d18.rglob("*.md")):
            if is_superseded_path(path) or not path.stem.startswith("D18-L"):
                skipped += 1
            else:
                files.append(path)
    return files, skipped


def _collect_json_files(project_root: Path) -> tuple[list[Path], int]:
    assets = project_root / "docs/game/design/common/assets"
    files: list[Path] = []
    skipped = 0
    if assets.is_dir():
        for path in sorted(assets.rglob("*.json")):
            if is_superseded_path(path):
                skipped += 1
            else:
                files.append(path)
    return files, skipped


def run_guard(project_root: Path) -> dict[str, Any]:
    """Scan manifests, logic catalogs and active D18 documents (read-only)."""

    project_root = project_root.resolve()
    issues: list[dict[str, Any]] = []
    json_files, json_skipped = _collect_json_files(project_root)
    doc_files, doc_skipped = _collect_doc_files(project_root)
    for path in json_files:
        _scan_json_document(project_root, path, issues)
    for path in doc_files:
        _scan_doc(project_root, path, issues)
    error_count = sum(1 for issue in issues if issue.get("severity") == "error")
    return {
        "task": "D18-L18",
        "tool": "tools/d18_release_claim_guard.py",
        "read_only": True,
        "approval_fields_modified": False,
        "status": "PASS" if error_count == 0 else "BLOCKED",
        "scanned_json_files": len(json_files),
        "scanned_doc_files": len(doc_files),
        "skipped_superseded_files": json_skipped + doc_skipped,
        "error_count": error_count,
        "issue_counts": dict(sorted(Counter(issue["code"] for issue in issues).items())),
        "issues": issues,
        "json_files": [str(path.relative_to(project_root)) for path in json_files],
        "doc_files": [str(path.relative_to(project_root)) for path in doc_files],
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    result = run_guard(args.project_root)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
