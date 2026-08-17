"""D18-L38 read-only lineage guard: original / converted / derived / generated.

Guards the provenance lineage of asset records so that generated or derived
assets can never masquerade as third-party originals and never bypass the
license or rights of their source.

Input: a lineage catalog JSON (schema documented below).  Optional second
input: L02-style asset records (``--asset-records``) for a cross-check that
blocks approval-state records whose lineage is invalid (missing lineage blocks
approval).

The tool is strictly read-only: it never writes, converts, generates or
deletes any texture, model, binary or record.

Lineage catalog format (frozen by D18-L38, documented here):

.. code-block:: json

    {
      "lineage_kind": "d18_generated_asset_lineage",
      "schema_version": 1,
      "records": [
        {
          "canonical_asset_id": "asset_...",   // L02 key pattern ^asset_[a-z0-9_]+$
          "origin_kind": "original|converted|derived|generated",
          "parent_asset_id": "asset_..." | null,
          "conversion_steps": ["convert to PNG", ...],  // required when non-original
          "project_authored": true | false,             // required for generated
          "third_party_claim": {                        // ONLY for original
            "source_id": "source_...",
            "source_url": "https://...",
            "license_id": "..."
          },
          "license_basis": {                            // required for non-original
            "license_id": "...",
            "attribution_required": bool,
            "commercial_use": bool,
            "modification_allowed": bool,
            "distribution_with_game": bool
          },
          "evidence_paths": ["repo/relative/path", ...],  // optional
          "external_parent": {"source_url": "...", "evidence_path": "..."} | null
        }
      ]
    }

Rules (every finding carries record_id, rule, severity and message):

- ``json_parse_error``            P0  catalog is not valid JSON
- ``missing_records``             P0  records key missing or empty
- ``duplicate_record``            P0  canonical_asset_id repeated
- ``bad_asset_id``                P0  id does not match ^asset_[a-z0-9_]+$
- ``unknown_origin_kind``         P0  origin_kind not in the four-value enum
- ``self_parent``                 P0  parent_asset_id == canonical_asset_id
- ``circular_lineage``            P0  parent chain contains a cycle
- ``origin_with_parent``          P0  original declares parent or steps
- ``origin_missing_claim``        P0  original without third_party_claim
- ``non_original_missing_lineage`` P0 derived/converted without parent+steps,
                                      or generated without steps (missing
                                      lineage blocks approval; project-authored
                                      generated assets may have no external
                                      parent but still need generation steps)
- ``non_original_with_claim``     P0  non-original declares third_party_claim
- ``generated_masquerading_as_original`` P0 project-authored generated asset
                                      declares third_party_claim
- ``missing_parent_reference``    P0  parent id unknown and no external_parent
- ``missing_license_basis``       P0  non-original without license_basis
- ``license_widening``            P0  non-original broadens parent license/rights
- ``evidence_path_escape``        P0  absolute path or '..' traversal
- ``missing_evidence_path``       P1  listed relative evidence path not on disk
- ``approval_with_invalid_lineage`` P0 cross-check: approval-state record with
                                      invalid lineage record
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any

TASK = "D18-L38"
TOOL = "tools/d18_generated_asset_guard.py"

ORIGIN_KINDS = ("original", "converted", "derived", "generated")
NON_ORIGIN_KINDS = ("converted", "derived", "generated")
APPROVAL_STATES = ("IntakeEligible", "RuntimeRequested", "RuntimeApproved", "ReleaseApproved")

ASSET_ID_RE = re.compile(r"^asset_[a-z0-9_]+$")
REPO_PATH_RE = re.compile(r"^[A-Za-z0-9_.][A-Za-z0-9_./-]*$")
TRAVERSAL_RE = re.compile(r"(^|/)\.\.(/|$)")
URL_RE = re.compile(r"^https://[^\s]+$")

RIGHT_FIELDS = (
    "commercial_use",
    "modification_allowed",
    "distribution_with_game",
)

# ---------------------------------------------------------------------------
# finding helpers
# ---------------------------------------------------------------------------


def _finding(record_id: str, rule: str, severity: str, message: str) -> dict[str, str]:
    return {
        "record_id": record_id,
        "rule": rule,
        "severity": severity,
        "message": message,
    }


# ---------------------------------------------------------------------------
# structural validation of a single record
# ---------------------------------------------------------------------------


def _check_record(record: dict[str, Any], catalog: dict[str, dict[str, Any]]) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    asset_id = str(record.get("canonical_asset_id", ""))
    origin_kind = record.get("origin_kind")
    parent_id = record.get("parent_asset_id") or None
    steps = record.get("conversion_steps")
    has_steps = bool(steps)

    if not ASSET_ID_RE.fullmatch(asset_id):
        return [_finding(asset_id or "<empty>", "bad_asset_id", "P0", "canonical_asset_id 必须匹配 ^asset_[a-z0-9_]+$")]

    if origin_kind not in ORIGIN_KINDS:
        return [_finding(asset_id, "unknown_origin_kind", "P0", f"origin_kind 必须是 {ORIGIN_KINDS} 之一，实际为 {origin_kind!r}")]

    if parent_id == asset_id:
        findings.append(_finding(asset_id, "self_parent", "P0", "parent_asset_id 不能等于自身"))

    is_non_original = origin_kind in NON_ORIGIN_KINDS

    if origin_kind == "original":
        if parent_id or has_steps:
            findings.append(_finding(asset_id, "origin_with_parent", "P0", "original 不得声明 parent_asset_id 或 conversion_steps"))
        if not record.get("third_party_claim"):
            findings.append(_finding(asset_id, "origin_missing_claim", "P0", "original 必须携带 third_party_claim（来源、URL、许可）"))
    else:
        # 缺谱系必须阻断批准：派生/转换件必须有 parent 与转换步骤；
        # 生成件必须有生成步骤；项目自有生成件（project_authored）可无外部 parent，
        # 但一旦声明 parent 就必须可解析。
        if not has_steps or (not parent_id and not (origin_kind == "generated" and record.get("project_authored") is True)):
            findings.append(_finding(asset_id, "non_original_missing_lineage", "P0", "派生/转换/生成件必须指向 parent_asset_id 与 conversion_steps；缺谱系阻断批准（项目自有生成件可无外部 parent，但必须有生成步骤）"))
        if record.get("third_party_claim"):
            if origin_kind == "generated" and record.get("project_authored") is True:
                findings.append(_finding(asset_id, "generated_masquerading_as_original", "P0", "项目自有生成物不得声明 third_party_claim（禁止伪装为第三方原件）"))
            else:
                findings.append(_finding(asset_id, "non_original_with_claim", "P0", "非原件不得自行声明 third_party_claim；第三方来源声明只能存在于原始 parent 记录"))
        if record.get("license_basis") is None:
            findings.append(_finding(asset_id, "missing_license_basis", "P0", "非原件必须提供 license_basis（继承自 parent 的许可/权利）"))

    if parent_id and parent_id in catalog:
        parent = catalog[parent_id]
        if parent["origin_kind"] in NON_ORIGIN_KINDS and parent.get("license_basis") is None:
            # parent 自身的缺失由 parent 记录触发；此处只为谱系链检查保留
            pass
        child = record.get("license_basis")
        parent_basis = parent.get("license_basis")
        if child is not None and parent_basis is not None:
            for field in RIGHT_FIELDS:
                if parent_basis.get(field) is False and child.get(field) is True:
                    findings.append(
                        _finding(asset_id, "license_widening", "P0",
                                 f"派生件不得放宽 parent 权利：{field} parent=False 而子记录 True")
                    )
            if parent_basis.get("attribution_required") is True and child.get("attribution_required") is False:
                findings.append(
                    _finding(asset_id, "license_widening", "P0",
                             "派生件不得放宽 parent 署名要求：parent attribution_required=True 而子记录 False")
                )

    if parent_id and parent_id not in catalog and not record.get("external_parent"):
        findings.append(_finding(asset_id, "missing_parent_reference", "P0", f"parent_asset_id {parent_id!r} 不在目录中且无 external_parent 证据"))

    return findings


def _evidence_findings(record: dict[str, Any], project_root: Path) -> list[dict[str, str]]:
    asset_id = str(record.get("canonical_asset_id", ""))
    findings: list[dict[str, str]] = []
    paths = record.get("evidence_paths") or []
    if not isinstance(paths, list):
        return [_finding(asset_id, "evidence_path_escape", "P0", "evidence_paths 必须是数组")]
    for raw in paths:
        if not isinstance(raw, str) or not raw:
            continue
        if raw.startswith(("/", "\\", "C:", "c:")) or TRAVERSAL_RE.search(raw) or "\\" in raw or not REPO_PATH_RE.fullmatch(raw):
            findings.append(_finding(asset_id, "evidence_path_escape", "P0", f"证据路径越界或格式非法：{raw!r}"))
            continue
        if not (project_root / raw).is_file():
            findings.append(_finding(asset_id, "missing_evidence_path", "P1", f"证据文件不存在：{raw}"))
    external = record.get("external_parent")
    if isinstance(external, dict):
        evidence = external.get("evidence_path")
        if evidence and isinstance(evidence, str):
            if not REPO_PATH_RE.fullmatch(evidence) or TRAVERSAL_RE.search(evidence):
                findings.append(_finding(asset_id, "evidence_path_escape", "P0", f"external_parent 证据路径越界：{evidence!r}"))
            elif not (project_root / evidence).is_file():
                findings.append(_finding(asset_id, "missing_evidence_path", "P1", f"external_parent 证据文件不存在：{evidence}"))
    return findings


# ---------------------------------------------------------------------------
# catalog validation
# ---------------------------------------------------------------------------


def validate_lineage_catalog(catalog: Any, project_root: Path) -> dict[str, Any]:
    findings: list[dict[str, str]] = []
    if not isinstance(catalog, dict):
        return {
            "task": TASK, "tool": TOOL, "read_only": True, "status": "BLOCKED",
            "records_count": 0, "blocked": True,
            "findings_count": 1, "p0_findings_count": 1, "p1_findings_count": 0,
            "rule_counts": {"json_parse_error": 1},
            "findings": [_finding("<catalog>", "json_parse_error", "P0", "目录必须是 JSON 对象")],
        }
    records = catalog.get("records")
    if not isinstance(records, list) or not records:
        return {
            "task": TASK, "tool": TOOL, "read_only": True, "status": "BLOCKED",
            "records_count": 0, "blocked": True,
            "findings_count": 1, "p0_findings_count": 1, "p1_findings_count": 0,
            "rule_counts": {"missing_records": 1},
            "findings": [_finding("<catalog>", "missing_records", "P0", "records 缺失或为空")],
        }

    by_id: dict[str, dict[str, Any]] = {}
    for record in records:
        if not isinstance(record, dict):
            continue
        asset_id = str(record.get("canonical_asset_id", ""))
        if asset_id in by_id:
            findings.append(_finding(asset_id, "duplicate_record", "P0", "canonical_asset_id 重复"))
        by_id[asset_id] = record

    for record in records:
        if not isinstance(record, dict):
            continue
        findings.extend(_check_record(record, by_id))
        findings.extend(_evidence_findings(record, project_root))

    # 谱系环检测：从每个非原件沿 parent 链走，出现重复节点即环。
    for record in records:
        if not isinstance(record, dict):
            continue
        seen: set[str] = set()
        current = record.get("parent_asset_id") or None
        asset_id = str(record.get("canonical_asset_id", ""))
        while current is not None:
            if current == asset_id or current in seen:
                findings.append(_finding(asset_id, "circular_lineage", "P0", f"parent 链存在环：{asset_id} → {current}"))
                break
            seen.add(current)
            current_record = by_id.get(current)
            current = (current_record or {}).get("parent_asset_id") or None

    return _render(findings, len(records))


def _render(findings: list[dict[str, str]], records_count: int) -> dict[str, Any]:
    p0 = [f for f in findings if f["severity"] == "P0"]
    p1 = [f for f in findings if f["severity"] == "P1"]
    rule_counts: dict[str, int] = {}
    for finding in findings:
        rule_counts[finding["rule"]] = rule_counts.get(finding["rule"], 0) + 1
    findings.sort(key=lambda f: (f["record_id"], f["rule"]))
    return {
        "task": TASK,
        "tool": TOOL,
        "read_only": True,
        "status": "BLOCKED" if p0 else "PASS",
        "records_count": records_count,
        "blocked": bool(p0),
        "findings_count": len(findings),
        "p0_findings_count": len(p0),
        "p1_findings_count": len(p1),
        "rule_counts": {rule: rule_counts[rule] for rule in sorted(rule_counts)},
        "findings": findings,
    }


# ---------------------------------------------------------------------------
# optional cross-check with L02 asset records
# ---------------------------------------------------------------------------


def cross_check_asset_records(
    catalog: dict[str, Any],
    asset_records: Any,
    lineage_result: dict[str, Any],
) -> dict[str, Any]:
    """Block approval-state asset records whose lineage record is invalid.

    Only ids present in both inputs are compared; an original asset without a
    lineage record is untouched (lineage coverage is an original's own proof).
    """
    if not isinstance(asset_records, list) or not isinstance(catalog, dict):
        return {"matched_ids": [], "approval_records_checked": 0, "violations": 0}
    lineage_by_id = {str(record.get("canonical_asset_id", "")): record for record in catalog.get("records") or [] if isinstance(record, dict)}
    invalid_ids = {f["record_id"] for f in lineage_result["findings"] if f["severity"] == "P0" and f["record_id"] not in ("<catalog>",)}
    matched: list[str] = []
    approval_checked = 0
    violations = 0
    for record in asset_records:
        if not isinstance(record, dict):
            continue
        asset_id = str(record.get("canonical_asset_id", ""))
        if asset_id not in lineage_by_id:
            continue
        matched.append(asset_id)
        if record.get("state") in APPROVAL_STATES:
            approval_checked += 1
            if asset_id in invalid_ids:
                violations += 1
    return {
        "matched_ids": sorted(matched),
        "approval_records_checked": approval_checked,
        "violations": violations,
    }


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--lineage-catalog", type=Path, required=True, help="谱系目录 JSON（本工具文档格式）")
    parser.add_argument("--asset-records", type=Path, default=None, help="可选：L02 资源记录 JSON（数组），用于批准×谱系交叉检查")
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    project_root = args.project_root.resolve()
    try:
        catalog = json.loads(args.lineage_catalog.read_text(encoding="utf-8"))
    except (json.JSONDecodeError, OSError) as exc:
        result = {
            "task": TASK, "tool": TOOL, "read_only": True, "status": "BLOCKED",
            "records_count": 0, "blocked": True,
            "findings_count": 1, "p0_findings_count": 1, "p1_findings_count": 0,
            "rule_counts": {"json_parse_error": 1},
            "findings": [_finding("<catalog>", "json_parse_error", "P0", f"谱系目录不可读或非 JSON：{exc}")],
        }
    else:
        result = validate_lineage_catalog(catalog, project_root)
        result["lineage_catalog"] = str(args.lineage_catalog).replace("\\", "/")
        if args.asset_records:
            try:
                asset_records = json.loads(args.asset_records.read_text(encoding="utf-8"))
            except (json.JSONDecodeError, OSError) as exc:
                result["status"] = "BLOCKED"
                result["blocked"] = True
                result["findings"].append(_finding("<asset-records>", "json_parse_error", "P0", f"资源记录不可读或非 JSON：{exc}"))
                result["findings_count"] = len(result["findings"])
                result["p0_findings_count"] = sum(1 for f in result["findings"] if f["severity"] == "P0")
                result["asset_records"] = str(args.asset_records).replace("\\", "/")
                result["cross_check"] = {"matched_ids": [], "approval_records_checked": 0, "violations": 0}
            else:
                cross = cross_check_asset_records(catalog, asset_records, result)
                result["asset_records"] = str(args.asset_records).replace("\\", "/")
                result["cross_check"] = cross
                if cross["violations"]:
                    result["status"] = "BLOCKED"
                    result["blocked"] = True
                    result["findings"].append(_finding("<cross-check>", "approval_with_invalid_lineage", "P0", "存在持有批准状态的资源记录，其谱系记录无效（缺谱系阻断批准）"))
                    result["findings_count"] = len(result["findings"])
                    result["p0_findings_count"] = sum(1 for f in result["findings"] if f["severity"] == "P0")
        else:
            result["asset_records"] = None
            result["cross_check"] = {"matched_ids": [], "approval_records_checked": 0, "violations": 0}

    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
