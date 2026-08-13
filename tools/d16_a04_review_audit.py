"""D16-A04 structural audit for history, sensitive-content and authorization records.

This intentionally does not decide historical truth. It only verifies that each
record carries an explicit review state, evidence references and a safe UI path.
Pending review remains a blocking result even when the JSON structure is valid.
"""

import argparse
import json
from collections import Counter
from pathlib import Path


SENSITIVE_TAGS = {
    "slavery", "forced_labor", "colonial", "religion", "religious_conflict",
    "whaling", "rare_animal", "gambling", "land", "identity", "sensitive",
}
ALLOWED_HISTORICAL_TAGS = {"historical", "inferred", "fictional"}
ALLOWED_REVIEWS = {"Pending", "Approved", "ApprovedWithWarning", "Rejected"}


def load_entries(root: Path):
    for path in sorted(root.rglob("*.json")):
        if path.name == "registry.json":
            continue
        try:
            value = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            continue
        if isinstance(value, dict) and value.get("id"):
            yield path, value


def audit(root: Path):
    errors = []
    pending = []
    rejected = []
    review_counts = Counter()
    asset_counts = Counter()
    sensitive_count = 0
    authorization_count = 0
    visual_asset_count = 0

    for path, entry in load_entries(root):
        record_id = entry["id"]
        tags = {str(tag).lower() for tag in entry.get("tags", [])}
        warnings = entry.get("content_warning_ids")
        sensitive = bool(tags & SENSITIVE_TAGS) or bool(warnings)
        has_history = "historical_tag" in entry or sensitive
        asset_state = entry.get("asset_state")
        has_visual_asset = asset_state in {"placeholder", "integrated", "verified", "production"}

        if has_visual_asset:
            visual_asset_count += 1
            asset_counts[str(entry.get("asset_state"))] += 1
            authorization = entry.get("authorization")
            license_record = entry.get("license")
            source_record = entry.get("source")
            source_kind = source_record.get("kind", "") if isinstance(source_record, dict) else ""
            license_status = license_record.get("status", "") if isinstance(license_record, dict) else ""
            # 项目自有占位音频/服装可能携带 project-authored-placeholder 或
            # internal-authored-plus-cc0；这些字段已经明确记录权利来源，不能
            # 因审计器只匹配最短枚举值而重复制造“缺授权”假缺口。外部素材仍
            # 要求显式 authorization 记录。
            has_rights_record = isinstance(authorization, dict) or (
                source_kind.startswith("project-authored")
                and license_status.startswith("internal-authored")
            )
            if has_rights_record:
                authorization_count += 1
            else:
                errors.append(f"{record_id}:visual_asset_missing_authorization")

        if not has_history:
            continue

        if sensitive:
            sensitive_count += 1
        tag = entry.get("historical_tag")
        if tag not in ALLOWED_HISTORICAL_TAGS:
            errors.append(f"{record_id}:invalid_or_missing_historical_tag")

        review = entry.get("historical_review")
        review_counts[str(review)] += 1
        if review not in ALLOWED_REVIEWS:
            errors.append(f"{record_id}:invalid_or_missing_historical_review")
        elif review == "Pending":
            pending.append(record_id)
        elif review == "Rejected":
            rejected.append(record_id)

        evidence = entry.get("review_evidence_refs")
        if not isinstance(evidence, list) or not evidence or not all(evidence):
            errors.append(f"{record_id}:review_evidence_refs_missing")

        if not isinstance(warnings, list):
            errors.append(f"{record_id}:content_warning_ids_missing")
        if not isinstance(entry.get("skip_allowed"), bool):
            errors.append(f"{record_id}:skip_allowed_missing")

        if review == "ApprovedWithWarning" and not warnings:
            errors.append(f"{record_id}:warning_review_without_warning_id")

    blockers = []
    if pending:
        blockers.append("pending_historical_reviews")
    if rejected:
        blockers.append("rejected_historical_reviews")
    if visual_asset_count != authorization_count:
        blockers.append("visual_asset_authorization_not_closed")
    if errors:
        blockers.append("structural_review_errors")

    return {
        "task": "D16-A04",
        "status": "IN_PROGRESS",
        "root": str(root),
        "entry_count": sum(1 for _ in load_entries(root)),
        "sensitive_entry_count": sensitive_count,
        "visual_asset_count": visual_asset_count,
        "authorization_record_count": authorization_count,
        "review_counts": dict(sorted(review_counts.items())),
        "asset_state_counts": dict(sorted(asset_counts.items())),
        "pending_review_count": len(pending),
        "rejected_review_count": len(rejected),
        "structural_error_count": len(errors),
        "structural_errors": errors[:100],
        "blocking_reasons": blockers,
        "note": "结构审计通过不等于历史事实、外部授权或敏感表现已由人工逐条批准。",
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("root", type=Path)
    parser.add_argument("--json-out", type=Path)
    args = parser.parse_args()
    result = audit(args.root)
    output = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
