"""Read-only missing-evidence audit for D18 logic asset records (D18-L34).

For every record the audit enumerates missing source / license / hash /
rights / content_id / descriptor evidence, classifies each gap as P0 / P1 /
P2 and gives a non-overstepping next step.  It never fills values, never
approves, never downloads and never writes state.  Output is machine
readable and deterministic: records are sorted by canonical_asset_id and
gaps follow a fixed check order, so repeated runs on the same input produce
identical results (CLI output only adds an ``as_of`` timestamp).

Severity semantics follow the D18-L01 baseline and L03 state machine:
- P0: blocks the record (schema violations, missing license, non-granted
  rights, conflicts, forbidden state claims, forged/absolute paths);
- P1: keeps the record Pending (nullable evidence like sha256/download_date,
  evidence paths to archive, content-id reference check by L15, missing
  L25-L29 descriptor);
- P2: completeness notes (runtime_deferred register, descriptor fallback).
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from datetime import datetime, timezone
from pathlib import Path, PurePath
from typing import Any

SEVERITY_P0 = "P0"
SEVERITY_P1 = "P1"
SEVERITY_P2 = "P2"

DIMENSIONS = ("record", "source", "license", "hash", "rights", "content_id", "descriptor")

ASSET_ID_RE = re.compile(r"^asset_[a-z0-9_]+$")
SOURCE_ID_RE = re.compile(r"^source_[a-z0-9_]+$")
CONTENT_ID_RE = re.compile(r"^[a-z0-9_]+$")
SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")
URL_RE = re.compile(r"^https?://[^\s]+$")
DRIVE_RE = re.compile(r"^[a-zA-Z]:")

VALID_STATES = {
    "Candidate",
    "IntakeEligible",
    "RuntimeRequested",
    "RuntimeApproved",
    "ReleaseApproved",
    "Rejected",
    "Revoked",
}
RIGHT_FIELDS = ("commercial_use", "modification_allowed", "repository_storage", "distribution_with_game")
PERMISSIVE_LICENSES = {"cc0", "cc010", "ccby", "ccby40", "ccby30", "ofl11", "mit"}
_NON_ALNUM_RE = re.compile(r"[^a-z0-9]")

NEXT_STEPS = {
    "GAP_ASSET_ID_MISSING": "登记方补齐 canonical_asset_id（asset_ 前缀业务键，L21—L24）；不得用磁盘路径或显示名",
    "GAP_CATEGORY_MISSING": "按 L11—L14 分类词典登记 category",
    "GAP_CATEGORY_INVALID": "category 必须命中 L02 Schema 枚举与 L11—L14 词典；由 L45 交叉核对",
    "GAP_STATE_MISSING": "登记 state（L03 七状态枚举）",
    "GAP_STATE_INVALID": "state 必须是 L03 七枚举；Pending/WaiverRequested/LocalDevelopmentReady 不是状态",
    "GAP_STATE_APPROVED_NO_EVIDENCE": "本逻辑阶段禁止 RuntimeApproved/ReleaseApproved 授予（基线 2.6、L03 第 8 节）；由 L18/L35 拒绝，须附 human_reviewer/runtime_consumers 等证据字段",
    "GAP_EVIDENCE_PATHS_MISSING": "补齐仓库内相对证据路径（基线 6.1 evidence_paths）；缺证据路径且属 L19 白名单由人类裁决 waiver",
    "GAP_EVIDENCE_PATH_INVALID": "证据路径必须是仓库内相对路径，禁止绝对路径、盘符、..（L08 校验）",
    "GAP_RUNTIME_DEFERRED_MISSING": "逐记录登记 runtime_deferred（基线 6.1），不得以延期替代通过",
    "GAP_RECORD_NOT_OBJECT": "记录必须是对象（dict）；由来源目录任务（L21—L24）修正",
    "GAP_SOURCE_ID_MISSING": "补齐 source_id（source_<来源名>，L21—L24 登记）",
    "GAP_SOURCE_ID_INVALID": "source_id 必须匹配 ^source_[a-z0-9_]+$",
    "GAP_SOURCE_URL_MISSING": "补齐官方来源 source_url（L05 静态记录，不访问网络验证）",
    "GAP_SOURCE_URL_INVALID": "source_url 必须是 http(s) URL",
    "GAP_DOWNLOAD_DATE_PENDING": "未下载时保持 null，不写猜测日期（基线 6.1 download_date）",
    "GAP_ORIGINAL_FILENAME_MISSING": "补齐原始包/文件名；不因改名造新包 ID",
    "GAP_LICENSE_MISSING": "补齐许可证对象（L04 标准化）；未知/冲突/NC 阻断，不得猜测 CC0",
    "GAP_LICENSE_ID_MISSING": "补齐规范化 license_id（L04）",
    "GAP_LICENSE_URL_MISSING": "补齐官方许可证链接 license_url",
    "GAP_LICENSE_URL_INVALID": "license_url 必须是 http(s) URL",
    "GAP_LICENSE_EVIDENCE_MISSING": "补齐包内许可证证据相对路径（基线 2.4.1 拒绝条件）；证据路径待归档属 L19 白名单，由人类裁决",
    "GAP_LICENSE_EVIDENCE_INVALID": "许可证证据路径必须是仓库内相对路径（L10 包内校验）",
    "GAP_ATTRIBUTION_UNKNOWN": "补齐 attribution_required（L04）；未知时保守阻断（L09 校验）",
    "GAP_ATTRIBUTION_INVALID": "attribution_required 必须是布尔值（L09）",
    "GAP_CC0_FALSE_ATTRIBUTION": "CC0 不要求署名，禁止伪造必须署名（L09 冲突）；由登记方修正",
    "GAP_SHA256_PENDING": "由 L06 对真实文件计算；原文件不可得时保持 null/Pending，绝不补猜",
    "GAP_SHA256_INVALID": "sha256 必须是 64 位十六进制（L06）",
    "GAP_RIGHTS_MISSING": "补齐 rights 四项显式记录（L07）",
    "GAP_RIGHT_FIELD_MISSING": "四项权利（commercial_use/modification_allowed/repository_storage/distribution_with_game）每项必须显式布尔（L07）",
    "GAP_RIGHT_NOT_GRANTED": "权利为 false/unknown 的条目不可分发（基线 2.4.4）；按 L19 走拒绝/撤销，waiver 不能绕过商业授权",
    "GAP_RIGHTS_LICENSE_CONFLICT": "许可为宽松许可但权利声明为 false/unknown，来源页与包内许可或权利矛盾（基线 2.4.2 冲突）；由人类复核或 L07 重新裁决",
    "GAP_CONTENT_IDS_MISSING": "补齐 content_ids（L15 校验存在性；不允许游戏数据反向依赖候选文件）",
    "GAP_CONTENT_ID_INVALID": "content_ids 元素必须是小写 snake_case",
    "GAP_DESCRIPTOR_MISSING": "由 L25—L29 生成导入描述器（本审计不创建）；缺描述器时记录保持 Pending",
    "GAP_DESCRIPTOR_TYPE_MISMATCH": "描述器 type 必须匹配记录 category（L25 Schema 与 L45 交叉核对；冲突）",
    "GAP_DESCRIPTOR_FALLBACK_MISSING": "描述器 fallback（reference+description）必须声明（L25 Schema required）",
}


def _gap(dimension: str, code: str, severity: str, field: str) -> dict[str, str]:
    return {
        "dimension": dimension,
        "code": code,
        "severity": severity,
        "field": field,
        "next_step": NEXT_STEPS.get(code, "由责任任务核实"),
    }


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _normalize_license_id(value: str) -> str:
    return _NON_ALNUM_RE.sub("", value.lower())


def _is_permissive_license(license_id: str) -> bool:
    return _normalize_license_id(license_id) in PERMISSIVE_LICENSES


def _is_cc0(license_id: str) -> bool:
    norm = _normalize_license_id(license_id)
    return norm.startswith("cc0") and "by" not in norm


def _valid_evidence_path(value: str) -> bool:
    if not value.strip() or " " in value.strip():
        return False
    if DRIVE_RE.match(value):
        return False
    pure = PurePath(value)
    if pure.is_absolute():
        return False
    return ".." not in pure.parts


def _valid_url(value: str) -> bool:
    return bool(URL_RE.match(value.strip()))


def _audit_one(record: Any, descriptor_index: dict[str, Any] | None) -> dict[str, Any]:
    gaps: list[dict[str, str]] = []
    record_id = "?"
    if not isinstance(record, dict):
        gaps.append(_gap("record", "GAP_RECORD_NOT_OBJECT", SEVERITY_P0, "record"))
        return {"canonical_asset_id": record_id, "has_gaps": True, "gaps": gaps}

    record_id = _text(record.get("canonical_asset_id")) or "?"

    asset_id = _text(record.get("canonical_asset_id"))
    if not asset_id:
        gaps.append(_gap("record", "GAP_ASSET_ID_MISSING", SEVERITY_P0, "canonical_asset_id"))
    elif not ASSET_ID_RE.match(asset_id):
        gaps.append(_gap("record", "GAP_ASSET_ID_INVALID", SEVERITY_P0, "canonical_asset_id"))

    category = _text(record.get("category"))
    if not category:
        gaps.append(_gap("record", "GAP_CATEGORY_MISSING", SEVERITY_P0, "category"))

    state = _text(record.get("state"))
    if not state:
        gaps.append(_gap("record", "GAP_STATE_MISSING", SEVERITY_P0, "state"))
    elif state not in VALID_STATES:
        gaps.append(_gap("record", "GAP_STATE_INVALID", SEVERITY_P0, "state"))
    else:
        if state == "RuntimeApproved" and not record.get("runtime_consumers"):
            gaps.append(_gap("record", "GAP_STATE_APPROVED_NO_EVIDENCE", SEVERITY_P0, "state"))
        if state == "ReleaseApproved" and not (
            record.get("human_reviewer") and record.get("human_review_date") and record.get("release_evidence_path")
        ):
            gaps.append(_gap("record", "GAP_STATE_APPROVED_NO_EVIDENCE", SEVERITY_P0, "state"))

    source_id = _text(record.get("source_id"))
    if not source_id:
        gaps.append(_gap("source", "GAP_SOURCE_ID_MISSING", SEVERITY_P0, "source_id"))
    elif not SOURCE_ID_RE.match(source_id):
        gaps.append(_gap("source", "GAP_SOURCE_ID_INVALID", SEVERITY_P0, "source_id"))

    source_url = _text(record.get("source_url"))
    if not source_url:
        gaps.append(_gap("source", "GAP_SOURCE_URL_MISSING", SEVERITY_P0, "source_url"))
    elif not _valid_url(source_url):
        gaps.append(_gap("source", "GAP_SOURCE_URL_INVALID", SEVERITY_P0, "source_url"))

    if record.get("download_date") is None:
        gaps.append(_gap("source", "GAP_DOWNLOAD_DATE_PENDING", SEVERITY_P1, "download_date"))

    if not _text(record.get("original_filename")):
        gaps.append(_gap("source", "GAP_ORIGINAL_FILENAME_MISSING", SEVERITY_P0, "original_filename"))

    license_obj = record.get("license")
    if not isinstance(license_obj, dict):
        gaps.append(_gap("license", "GAP_LICENSE_MISSING", SEVERITY_P0, "license"))
    else:
        license_id = _text(license_obj.get("license_id"))
        if not license_id:
            gaps.append(_gap("license", "GAP_LICENSE_ID_MISSING", SEVERITY_P0, "license.license_id"))
        license_url = _text(license_obj.get("license_url"))
        if not license_url:
            gaps.append(_gap("license", "GAP_LICENSE_URL_MISSING", SEVERITY_P0, "license.license_url"))
        elif not _valid_url(license_url):
            gaps.append(_gap("license", "GAP_LICENSE_URL_INVALID", SEVERITY_P0, "license.license_url"))
        evidence = _text(license_obj.get("license_evidence_path"))
        if not evidence:
            gaps.append(_gap("license", "GAP_LICENSE_EVIDENCE_MISSING", SEVERITY_P0, "license.license_evidence_path"))
        elif not _valid_evidence_path(evidence):
            gaps.append(_gap("license", "GAP_LICENSE_EVIDENCE_INVALID", SEVERITY_P0, "license.license_evidence_path"))
        attribution = license_obj.get("attribution_required")
        if attribution is None:
            gaps.append(_gap("license", "GAP_ATTRIBUTION_UNKNOWN", SEVERITY_P0, "license.attribution_required"))
        elif not isinstance(attribution, bool):
            gaps.append(_gap("license", "GAP_ATTRIBUTION_INVALID", SEVERITY_P0, "license.attribution_required"))
        elif attribution and license_id and _is_cc0(license_id):
            gaps.append(_gap("license", "GAP_CC0_FALSE_ATTRIBUTION", SEVERITY_P0, "license.attribution_required"))

    sha256 = record.get("sha256")
    if sha256 is None:
        gaps.append(_gap("hash", "GAP_SHA256_PENDING", SEVERITY_P1, "sha256"))
    elif not isinstance(sha256, str) or not SHA256_RE.match(sha256.strip()):
        gaps.append(_gap("hash", "GAP_SHA256_INVALID", SEVERITY_P0, "sha256"))

    rights = record.get("rights")
    if not isinstance(rights, dict):
        gaps.append(_gap("rights", "GAP_RIGHTS_MISSING", SEVERITY_P0, "rights"))
    else:
        any_false = False
        for field in RIGHT_FIELDS:
            value = rights.get(field)
            if value is None or not isinstance(value, bool):
                gaps.append(_gap("rights", "GAP_RIGHT_FIELD_MISSING", SEVERITY_P0, "rights." + field))
            elif value is False:
                gaps.append(_gap("rights", "GAP_RIGHT_NOT_GRANTED", SEVERITY_P0, "rights." + field))
                any_false = True
        if any_false and isinstance(license_obj, dict):
            license_id = _text(license_obj.get("license_id"))
            if license_id and _is_permissive_license(license_id):
                gaps.append(_gap("rights", "GAP_RIGHTS_LICENSE_CONFLICT", SEVERITY_P0, "rights"))

    content_ids = record.get("content_ids")
    if not isinstance(content_ids, list) or not content_ids:
        gaps.append(_gap("content_id", "GAP_CONTENT_IDS_MISSING", SEVERITY_P0, "content_ids"))
    else:
        for content_id in content_ids:
            if not isinstance(content_id, str) or not CONTENT_ID_RE.match(content_id):
                gaps.append(_gap("content_id", "GAP_CONTENT_ID_INVALID", SEVERITY_P0, "content_ids"))
                break

    evidence_paths = record.get("evidence_paths")
    if not isinstance(evidence_paths, list) or not evidence_paths:
        gaps.append(_gap("record", "GAP_EVIDENCE_PATHS_MISSING", SEVERITY_P1, "evidence_paths"))
    else:
        for path in evidence_paths:
            if not isinstance(path, str) or not _valid_evidence_path(path):
                gaps.append(_gap("record", "GAP_EVIDENCE_PATH_INVALID", SEVERITY_P0, "evidence_paths"))
                break

    if descriptor_index is not None:
        descriptor = descriptor_index.get(record_id)
        if descriptor is None:
            gaps.append(_gap("descriptor", "GAP_DESCRIPTOR_MISSING", SEVERITY_P1, "descriptor"))
        else:
            if isinstance(descriptor, dict) and descriptor.get("type") != category:
                gaps.append(_gap("descriptor", "GAP_DESCRIPTOR_TYPE_MISMATCH", SEVERITY_P0, "descriptor.type"))
            else:
                fallback = descriptor.get("fallback")
                if not isinstance(fallback, dict) or not fallback.get("reference") or not fallback.get("description"):
                    gaps.append(_gap("descriptor", "GAP_DESCRIPTOR_FALLBACK_MISSING", SEVERITY_P2, "descriptor.fallback"))
    else:
        gaps.append(_gap("descriptor", "GAP_DESCRIPTOR_MISSING", SEVERITY_P1, "descriptor"))

    if not isinstance(record.get("runtime_deferred"), list) or not record.get("runtime_deferred"):
        gaps.append(_gap("record", "GAP_RUNTIME_DEFERRED_MISSING", SEVERITY_P2, "runtime_deferred"))

    return {"canonical_asset_id": record_id, "has_gaps": len(gaps) > 0, "gaps": gaps}


def audit_record(record: Any) -> dict[str, Any]:
    """Audit one record; returns a deterministic gap report. Read-only."""
    return _audit_one(record, None)


def audit_records(records: list[Any], descriptors: list[dict[str, Any]] | None = None) -> dict[str, Any]:
    """Audit a list of records; summary + per-record reports. Never mutates input."""
    descriptor_index: dict[str, Any] | None = None
    if isinstance(descriptors, list):
        descriptor_index = {}
        for descriptor in descriptors:
            if isinstance(descriptor, dict) and isinstance(descriptor.get("source_asset_id"), str):
                descriptor_index[descriptor["source_asset_id"]] = descriptor
    results = [_audit_one(record, descriptor_index) for record in records]
    results.sort(key=lambda result: result["canonical_asset_id"])
    gap_list = [gap for result in results for gap in result["gaps"]]
    by_severity = {"P0": 0, "P1": 0, "P2": 0}
    by_dimension = {dimension: 0 for dimension in DIMENSIONS}
    by_code: dict[str, int] = {}
    for gap in gap_list:
        by_severity[gap["severity"]] += 1
        by_dimension[gap["dimension"]] += 1
        by_code[gap["code"]] = by_code.get(gap["code"], 0) + 1
    return {
        "schema_version": "1.0",
        "audit_kind": "d18_missing_evidence_audit",
        "total": len(results),
        "records_with_gaps": sum(1 for result in results if result["has_gaps"]),
        "gaps_total": len(gap_list),
        "by_severity": by_severity,
        "by_dimension": by_dimension,
        "by_code": by_code,
        "records": results,
    }


def load_source_catalogs(catalog_dir: str | Path | None = None) -> list[dict[str, Any]]:
    """Discover source catalog JSON files and collect their records (read-only)."""
    if catalog_dir is None:
        catalog_dir = Path(__file__).resolve().parents[1] / "docs/game/design/common/assets/sources"
    catalog_dir = Path(catalog_dir)
    records: list[dict[str, Any]] = []
    if not catalog_dir.exists():
        return records
    for path in sorted(catalog_dir.glob("*.json")):
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except (json.JSONDecodeError, OSError):
            continue
        for record in data.get("records", []) if isinstance(data, dict) else []:
            if isinstance(record, dict):
                records.append(dict(record, audit_source_file=str(path.relative_to(catalog_dir))))
    return records


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="D18 missing-evidence audit (read-only)")
    parser.add_argument("--catalog-dir", default=None, help="source catalog directory (default: docs/game/design/common/assets/sources)")
    parser.add_argument("--descriptors-dir", default=None, help="optional directory of L25 import descriptor JSON files")
    parser.add_argument("--output", default=None, help="optional output path (default: stdout); never writes state")
    args = parser.parse_args(argv)

    records = load_source_catalogs(args.catalog_dir)
    descriptors: list[dict[str, Any]] = []
    if args.descriptors_dir:
        for path in sorted(Path(args.descriptors_dir).glob("*.json")):
            try:
                data = json.loads(path.read_text(encoding="utf-8"))
            except (json.JSONDecodeError, OSError):
                continue
            items = data if isinstance(data, list) else [data]
            descriptors.extend(item for item in items if isinstance(item, dict))

    report = audit_records(records, descriptors)
    report["as_of"] = datetime.now(timezone.utc).isoformat(timespec="seconds")
    payload = json.dumps(report, ensure_ascii=False, indent=2, sort_keys=True)
    if args.output:
        Path(args.output).write_text(payload + "\n", encoding="utf-8")
    else:
        print(payload)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
