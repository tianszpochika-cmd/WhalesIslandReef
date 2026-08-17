"""Read-only attribution metadata validator for D18-L09.

The validator only inspects metadata records: it never edits credits, UI or
assets, never accesses the network, and never treats the presence of an
attribution entry as proof of commercial rights.  When a license/rights
record marks attribution as required, the author, a license text or link,
a credits display location and an asset association must all be present.
CC0 must not be falsely marked as requiring attribution, and an unknown
attribution requirement is returned as a P0 issue instead of being guessed.

Consumes the D18-L04 normalized license interface (``canonical_license``,
``attribution_required``, ``license_url``, ``license_evidence_path``) either
nested under ``license`` or at the record top level, so it stays compatible
with the baseline field dictionary without importing the L04 module.
"""

from __future__ import annotations

import re
from pathlib import PurePath
from typing import Any
from urllib.parse import urlparse

SEVERITY_P0 = "P0"
SEVERITY_WARN = "WARN"

ATTR_INPUT_TYPE_INVALID = "ATTR_INPUT_TYPE_INVALID"
ATTR_REQUIREMENT_UNKNOWN = "ATTR_REQUIREMENT_UNKNOWN"
ATTR_REQUIREMENT_INVALID = "ATTR_REQUIREMENT_INVALID"
ATTR_CC0_FALSE_ATTRIBUTION_REQUIRED = "ATTR_CC0_FALSE_ATTRIBUTION_REQUIRED"
ATTR_AUTHOR_MISSING = "ATTR_AUTHOR_MISSING"
ATTR_AUTHOR_PLACEHOLDER = "ATTR_AUTHOR_PLACEHOLDER"
ATTR_LICENSE_REF_MISSING = "ATTR_LICENSE_REF_MISSING"
ATTR_LICENSE_URL_INVALID = "ATTR_LICENSE_URL_INVALID"
ATTR_LICENSE_EVIDENCE_PATH_INVALID = "ATTR_LICENSE_EVIDENCE_PATH_INVALID"
ATTR_CREDITS_LOCATION_MISSING = "ATTR_CREDITS_LOCATION_MISSING"
ATTR_CREDITS_LOCATION_PLACEHOLDER = "ATTR_CREDITS_LOCATION_PLACEHOLDER"
ATTR_ASSET_LINK_MISSING = "ATTR_ASSET_LINK_MISSING"

WARN_BY_LICENSE_NO_ATTRIBUTION = "WARN_BY_LICENSE_NO_ATTRIBUTION"
WARN_PUBLIC_DOMAIN_ATTRIBUTION_CLAIM = "WARN_PUBLIC_DOMAIN_ATTRIBUTION_CLAIM"
WARN_CANONICAL_LICENSE_MISSING = "WARN_CANONICAL_LICENSE_MISSING"

NOTE_NO_COMMERCIAL_RIGHTS_INFERENCE = "NOTE_NO_COMMERCIAL_RIGHTS_INFERENCE"

MESSAGES: dict[str, dict[str, str]] = {
    "zh-CN": {
        ATTR_INPUT_TYPE_INVALID: "输入必须是对象（dict）形式的资源记录",
        ATTR_REQUIREMENT_UNKNOWN: "无法确定是否必须署名（attribution_required 缺失），保守阻断，不得猜测",
        ATTR_REQUIREMENT_INVALID: "attribution_required 必须是布尔值",
        ATTR_CC0_FALSE_ATTRIBUTION_REQUIRED: "CC0 不要求署名，禁止把 CC0 记录标记为必须署名",
        ATTR_AUTHOR_MISSING: "必须署名但缺少作者（author）",
        ATTR_AUTHOR_PLACEHOLDER: "必须署名但作者是占位值，不能视为真实作者",
        ATTR_LICENSE_REF_MISSING: "必须署名但缺少许可证文本或链接（license_url / license_evidence_path / license_text_path）",
        ATTR_LICENSE_URL_INVALID: "许可证链接必须是有效的 http(s) URL",
        ATTR_LICENSE_EVIDENCE_PATH_INVALID: "许可证证据路径必须是仓库内相对路径，禁止绝对路径或 ..",
        ATTR_CREDITS_LOCATION_MISSING: "必须署名但缺少展示/credits 位置（credits_location / display_location / credits_placement）",
        ATTR_CREDITS_LOCATION_PLACEHOLDER: "展示/credits 位置是占位值，不能视为真实位置",
        ATTR_ASSET_LINK_MISSING: "必须署名但缺少资源关联（canonical_asset_id / asset_id / content_ids）",
        WARN_BY_LICENSE_NO_ATTRIBUTION: "CC-BY 系列许可证通常要求署名，但 attribution_required=false；由 L04 许可证标准化复核",
        WARN_PUBLIC_DOMAIN_ATTRIBUTION_CLAIM: "公有领域内容不要求署名，但 attribution_required=true；需 L04 复核",
        WARN_CANONICAL_LICENSE_MISSING: "缺少规范化许可证标识（canonical_license），无法确认 CC0/署名类判断",
        NOTE_NO_COMMERCIAL_RIGHTS_INFERENCE: "署名存在不构成商业权利证明；商业权利由 L07 独立验证",
    },
    "en": {
        ATTR_INPUT_TYPE_INVALID: "input must be an object (dict) asset record",
        ATTR_REQUIREMENT_UNKNOWN: "cannot determine whether attribution is required (attribution_required missing); conservative block, no guessing",
        ATTR_REQUIREMENT_INVALID: "attribution_required must be a boolean",
        ATTR_CC0_FALSE_ATTRIBUTION_REQUIRED: "CC0 does not require attribution; CC0 records must not be marked as attribution-required",
        ATTR_AUTHOR_MISSING: "attribution required but author is missing",
        ATTR_AUTHOR_PLACEHOLDER: "attribution required but author is a placeholder, not a real author",
        ATTR_LICENSE_REF_MISSING: "attribution required but no license text or link (license_url / license_evidence_path / license_text_path)",
        ATTR_LICENSE_URL_INVALID: "license link must be a valid http(s) URL",
        ATTR_LICENSE_EVIDENCE_PATH_INVALID: "license evidence path must be a relative repo path; absolute paths and .. are forbidden",
        ATTR_CREDITS_LOCATION_MISSING: "attribution required but credits display location (credits_location / display_location / credits_placement) is missing",
        ATTR_CREDITS_LOCATION_PLACEHOLDER: "credits display location is a placeholder, not a real location",
        ATTR_ASSET_LINK_MISSING: "attribution required but asset association (canonical_asset_id / asset_id / content_ids) is missing",
        WARN_BY_LICENSE_NO_ATTRIBUTION: "CC-BY family licenses usually require attribution but attribution_required=false; needs L04 normalization review",
        WARN_PUBLIC_DOMAIN_ATTRIBUTION_CLAIM: "public domain content does not require attribution but attribution_required=true; needs L04 review",
        WARN_CANONICAL_LICENSE_MISSING: "canonical license id missing; cannot confirm CC0/attribution-class decisions",
        NOTE_NO_COMMERCIAL_RIGHTS_INFERENCE: "presence of attribution is not proof of commercial rights; commercial rights are validated independently by L07",
    },
}

PLACEHOLDER_VALUES = {
    "",
    "unknown",
    "anonymous",
    "n/a",
    "na",
    "tbd",
    "todo",
    "none",
    "?",
    "-",
    "--",
    "unknown author",
    "未知",
    "待定",
    "无",
    "佚名",
    "placeholder",
}
_NON_ALNUM_RE = re.compile(r"[^a-z0-9]")
_DRIVE_RE = re.compile(r"^[a-zA-Z]:")


def localize(code: str, lang: str = "zh-CN") -> str:
    """Return the localized message for a stable error code, falling back to en then the code."""
    table = MESSAGES.get(lang) or MESSAGES["en"]
    if code in table:
        return table[code]
    return MESSAGES["en"].get(code, code)


def _first_str(source: dict[str, Any], keys: tuple[str, ...]) -> str | None:
    for key in keys:
        value = source.get(key)
        if isinstance(value, str) and value.strip():
            return value.strip()
    return None


def _normalize_license_id(value: str) -> str:
    return _NON_ALNUM_RE.sub("", value.lower())


def _is_placeholder(value: str) -> bool:
    return value.strip().lower() in PLACEHOLDER_VALUES


def _valid_url(value: str) -> bool:
    if " " in value:
        return False
    parsed = urlparse(value)
    return parsed.scheme in ("http", "https") and bool(parsed.netloc)


def _valid_evidence_path(value: str) -> bool:
    if not value.strip() or " " in value.strip():
        return False
    if _DRIVE_RE.match(value):
        return False
    pure = PurePath(value)
    if pure.is_absolute():
        return False
    return ".." not in pure.parts


def _issue(code: str, field: str, severity: str) -> dict[str, str]:
    return {"code": code, "field": field, "severity": severity, "message_key": code}


def validate_attribution(record: Any) -> dict[str, Any]:
    """Validate attribution metadata of one asset record.

    Returns a dict with ``valid``, stable issue codes, warnings, notes and an
    explicit ``commercial_rights_inferred: false`` marker.  Never mutates the
    input and never touches files, credits, UI or assets.
    """
    issues: list[dict[str, str]] = []
    warnings: list[dict[str, str]] = []
    notes: list[dict[str, str]] = []
    record_id = "?"
    required: Any = None
    canonical: str | None = None

    if not isinstance(record, dict):
        issues.append(_issue(ATTR_INPUT_TYPE_INVALID, "record", SEVERITY_P0))
        return {
            "record_id": record_id,
            "attribution_required": None,
            "canonical_license": None,
            "valid": False,
            "issues": issues,
            "warnings": warnings,
            "notes": notes,
            "commercial_rights_inferred": False,
        }

    record_id = str(
        record.get("record_id")
        or record.get("canonical_asset_id")
        or record.get("asset_id")
        or record.get("pack_id")
        or "?"
    )
    license_obj = record.get("license") if isinstance(record.get("license"), dict) else {}
    rights_obj = record.get("rights") if isinstance(record.get("rights"), dict) else {}

    required = record.get(
        "attribution_required",
        license_obj.get("attribution_required", rights_obj.get("attribution_required")),
    )
    if required is None:
        issues.append(_issue(ATTR_REQUIREMENT_UNKNOWN, "attribution_required", SEVERITY_P0))
        return {
            "record_id": record_id,
            "attribution_required": None,
            "canonical_license": canonical,
            "valid": False,
            "issues": issues,
            "warnings": warnings,
            "notes": notes,
            "commercial_rights_inferred": False,
        }
    if not isinstance(required, bool):
        issues.append(_issue(ATTR_REQUIREMENT_INVALID, "attribution_required", SEVERITY_P0))
        return {
            "record_id": record_id,
            "attribution_required": required,
            "canonical_license": canonical,
            "valid": False,
            "issues": issues,
            "warnings": warnings,
            "notes": notes,
            "commercial_rights_inferred": False,
        }

    canonical = _first_str(license_obj, ("canonical_license", "license_id")) or _first_str(
        record, ("canonical_license", "license_id")
    )
    norm = _normalize_license_id(canonical) if canonical else ""
    is_cc0 = bool(canonical) and norm.startswith("cc0") and "by" not in norm
    is_cc_by = bool(canonical) and norm.startswith("ccby")
    is_public_domain = bool(canonical) and norm in ("publicdomain", "publicdomain1.0")
    if canonical is None:
        warnings.append(_issue(WARN_CANONICAL_LICENSE_MISSING, "canonical_license", SEVERITY_WARN))

    if required and is_cc0:
        issues.append(_issue(ATTR_CC0_FALSE_ATTRIBUTION_REQUIRED, "attribution_required", SEVERITY_P0))
    if required and is_public_domain:
        warnings.append(_issue(WARN_PUBLIC_DOMAIN_ATTRIBUTION_CLAIM, "attribution_required", SEVERITY_WARN))
    if not required and is_cc_by:
        warnings.append(_issue(WARN_BY_LICENSE_NO_ATTRIBUTION, "attribution_required", SEVERITY_WARN))

    if required:
        author = _first_str(record, ("author",)) or _first_str(license_obj, ("author",))
        if author is None:
            issues.append(_issue(ATTR_AUTHOR_MISSING, "author", SEVERITY_P0))
        elif _is_placeholder(author):
            issues.append(_issue(ATTR_AUTHOR_PLACEHOLDER, "author", SEVERITY_P0))

        license_url = _first_str(license_obj, ("license_url",)) or _first_str(record, ("license_url",))
        evidence_path = _first_str(license_obj, ("license_evidence_path",)) or _first_str(
            record, ("license_evidence_path",)
        )
        license_text_path = _first_str(license_obj, ("license_text_path",)) or _first_str(
            record, ("license_text_path",)
        )
        if license_url is None and evidence_path is None and license_text_path is None:
            issues.append(_issue(ATTR_LICENSE_REF_MISSING, "license", SEVERITY_P0))
        else:
            if license_url is not None and not _valid_url(license_url):
                issues.append(_issue(ATTR_LICENSE_URL_INVALID, "license_url", SEVERITY_P0))
            if evidence_path is not None and not _valid_evidence_path(evidence_path):
                issues.append(_issue(ATTR_LICENSE_EVIDENCE_PATH_INVALID, "license_evidence_path", SEVERITY_P0))
            if license_text_path is not None and not _valid_evidence_path(license_text_path):
                issues.append(_issue(ATTR_LICENSE_EVIDENCE_PATH_INVALID, "license_text_path", SEVERITY_P0))

        credits_location = _first_str(
            record, ("credits_location", "display_location", "credits_placement")
        ) or _first_str(license_obj, ("credits_location",))
        if credits_location is None:
            issues.append(_issue(ATTR_CREDITS_LOCATION_MISSING, "credits_location", SEVERITY_P0))
        elif _is_placeholder(credits_location):
            issues.append(_issue(ATTR_CREDITS_LOCATION_PLACEHOLDER, "credits_location", SEVERITY_P0))

        asset_id = _first_str(record, ("canonical_asset_id", "asset_id"))
        content_ids = record.get("content_ids")
        has_content_ids = isinstance(content_ids, list) and len(content_ids) > 0
        if asset_id is None and not has_content_ids:
            issues.append(_issue(ATTR_ASSET_LINK_MISSING, "asset_link", SEVERITY_P0))

    notes.append(_issue(NOTE_NO_COMMERCIAL_RIGHTS_INFERENCE, "scope", SEVERITY_WARN))
    valid = not any(issue["severity"] == SEVERITY_P0 for issue in issues)
    return {
        "record_id": record_id,
        "attribution_required": required,
        "canonical_license": canonical,
        "valid": valid,
        "issues": issues,
        "warnings": warnings,
        "notes": notes,
        "commercial_rights_inferred": False,
    }


def validate_attribution_records(records: Any) -> dict[str, Any]:
    """Aggregate ``validate_attribution`` over a list of records (read-only)."""
    if not isinstance(records, list):
        return {"total": 0, "valid": 0, "invalid": 0, "issue_counts": {ATTR_INPUT_TYPE_INVALID: 1}}
    results = [validate_attribution(record) for record in records]
    issue_counts: dict[str, int] = {}
    for result in results:
        for issue in result["issues"]:
            issue_counts[issue["code"]] = issue_counts.get(issue["code"], 0) + 1
    return {
        "total": len(results),
        "valid": sum(1 for result in results if result["valid"]),
        "invalid": sum(1 for result in results if not result["valid"]),
        "issue_counts": issue_counts,
    }
