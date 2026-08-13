"""Create a deterministic D16-A05 placeholder replacement queue.

The queue is a planning and evidence tool.  It never changes ``asset_state``
or treats an existing placeholder file as a final asset.  Each record keeps
its provenance and lists the concrete gates that a real, authorized final
resource must satisfy before an asset reviewer can replace it.
"""

from __future__ import annotations

import argparse
import json
from collections import Counter
from pathlib import Path
from typing import Any

from d16_a05_asset_audit import load_entries, normalize_project_root


DEFAULT_BATCH_SIZE = 50
APPROVED_REVIEW_STATES = {"Approved", "ApprovedWithWarning"}
REMOTE_REFERENCE_PREFIXES = ("http://", "https://", "git://", "git@")

# These are the smallest useful real-asset slice for validating the issues
# reported during D5/D16 playtests: a human avatar, a controllable boat, and
# the dock/island context around the boarding interaction.  This is queue
# ordering metadata only; it does not approve, replace, or upgrade anything.
VERTICAL_SLICE_PRIORITY_IDS = {
    "asset_ph_crew_male": 10,
    "asset_ph_ship_fishing": 20,
    "building_saint_marys_dock": 30,
    "building_small_boat_pier": 40,
    "island_saint_marys": 50,
}


def _as_list(value: Any) -> list[Any]:
    return value if isinstance(value, list) else []


def _resolve_path(project_root: Path, game_root: Path, reference: Any) -> Path | None:
    if not isinstance(reference, str) or not reference.strip():
        return None
    clean = reference.split("#", 1)[0].strip()
    candidate = Path(clean)
    if candidate.is_absolute():
        return candidate
    if candidate.parts and candidate.parts[0].lower() == game_root.name.lower():
        return project_root / candidate
    for base in (game_root, project_root):
        resolved = base / candidate
        if resolved.exists():
            return resolved
    return game_root / candidate


def _is_placeholder_reference(entry: dict[str, Any]) -> bool:
    stage = str(entry.get("production_stage", "")).lower()
    path_values = [entry.get("resource_path"), entry.get("runtime_prefab_path")]
    text = " ".join(str(value).lower() for value in path_values if value)
    return stage == "prototype_placeholder" or "placeholder" in text or "/ph_" in text or "\\ph_" in text


def _category(path: Path) -> str:
    return path.parent.name or "unclassified"


def _is_remote_reference(value: Any) -> bool:
    return isinstance(value, str) and value.strip().lower().startswith(REMOTE_REFERENCE_PREFIXES)


def _looks_like_local_path(value: str) -> bool:
    clean = value.split("#", 1)[0].strip()
    return "/" in clean or "\\" in clean or "." in Path(clean).name


def _missing_local_references(
    project_root: Path, game_root: Path, entry: dict[str, Any]
) -> list[str]:
    references: list[str] = []
    references.extend(value for value in _as_list(entry.get("source_refs")) if isinstance(value, str))
    references.extend(value for value in _as_list(entry.get("review_evidence_refs")) if isinstance(value, str))
    source = entry.get("source")
    if isinstance(source, dict) and isinstance(source.get("reference"), str):
        references.append(source["reference"])
    license_record = entry.get("license")
    if isinstance(license_record, dict) and isinstance(license_record.get("reference"), str):
        references.append(license_record["reference"])
    authorization = entry.get("authorization")
    if isinstance(authorization, dict):
        for field in ("license_ref", "source_url_or_path", "evidence_ref"):
            if isinstance(authorization.get(field), str):
                references.append(authorization[field])

    missing: list[str] = []
    for reference in references:
        if _is_remote_reference(reference) or not _looks_like_local_path(reference):
            continue
        resolved = _resolve_path(project_root, game_root, reference)
        if resolved is None or not resolved.exists():
            missing.append(reference)
    return sorted(set(missing))


def _queue_priority(asset_id: str) -> tuple[str, int | None]:
    rank = VERTICAL_SLICE_PRIORITY_IDS.get(asset_id)
    if rank is None:
        return "general", None
    return "vertical_slice", rank


def _requirements(project_root: Path, game_root: Path, entry: dict[str, Any]) -> tuple[list[str], dict[str, Any]]:
    requirements: list[str] = []
    review = entry.get("historical_review")
    if review not in APPROVED_REVIEW_STATES:
        requirements.append("approved_history_or_content_review")
    if not _as_list(entry.get("review_evidence_refs")):
        requirements.append("review_evidence_refs")
    if not isinstance(entry.get("source"), dict) or not entry.get("source", {}).get("kind"):
        requirements.append("source_provenance")
    if not isinstance(entry.get("license"), dict) or not entry.get("license", {}).get("status"):
        requirements.append("license_record")
    authorization = entry.get("authorization")
    source_kind = str(entry.get("source", {}).get("kind", "")) if isinstance(entry.get("source"), dict) else ""
    license_status = str(entry.get("license", {}).get("status", "")) if isinstance(entry.get("license"), dict) else ""
    if not isinstance(authorization, dict) and not (
        source_kind.startswith("project-authored") and license_status.startswith("internal-authored")
    ):
        requirements.append("authorization_record")

    resource_path = entry.get("resource_path")
    runtime_prefab_path = entry.get("runtime_prefab_path")
    resource_file = _resolve_path(project_root, game_root, resource_path)
    prefab_file = _resolve_path(project_root, game_root, runtime_prefab_path)
    if resource_file is None or not resource_file.is_file():
        requirements.append("final_resource_file")
    if prefab_file is None or not prefab_file.is_file():
        requirements.append("runtime_prefab_or_consumer")
    if _is_placeholder_reference(entry):
        requirements.append("replace_placeholder_source_and_prefab")
    if not str(entry.get("lod_profile", "")).strip():
        requirements.append("lod_profile_or_lod_na")
    if not _as_list(entry.get("test_cases")):
        requirements.append("visual_load_collision_anchor_and_save_tests")

    missing_references = _missing_local_references(project_root, game_root, entry)
    if missing_references:
        requirements.append("provenance_and_review_reference_files")

    return requirements, {
        "source_file_exists": bool(resource_file and resource_file.is_file()),
        "runtime_prefab_exists": bool(prefab_file and prefab_file.is_file()),
        "source_kind": source_kind,
        "license_status": license_status,
        "authorization_present": isinstance(authorization, dict),
        "missing_local_references": missing_references,
    }


def build_queue(root: Path, batch_size: int = DEFAULT_BATCH_SIZE) -> dict[str, Any]:
    if batch_size <= 0:
        raise ValueError("batch_size must be greater than zero")
    project_root = root.parent if root.name == "game" else root
    records: list[dict[str, Any]] = []
    for path, entry in load_entries(root):
        if entry.get("asset_state") != "placeholder":
            continue
        asset_id = str(entry.get("id"))
        queue_priority, priority_rank = _queue_priority(asset_id)
        requirements, evidence = _requirements(project_root, root, entry)
        records.append({
            "id": asset_id,
            "path": path.as_posix(),
            "category": _category(path),
            "queue_priority": queue_priority,
            "priority_rank": priority_rank,
            "asset_state": entry.get("asset_state"),
            "production_stage": entry.get("production_stage"),
            "historical_review": entry.get("historical_review"),
            "historical_tag": entry.get("historical_tag"),
            "source_refs": _as_list(entry.get("source_refs")),
            "review_evidence_refs": _as_list(entry.get("review_evidence_refs")),
            "source": entry.get("source"),
            "license": entry.get("license"),
            "authorization": entry.get("authorization"),
            "resource_path": entry.get("resource_path"),
            "runtime_prefab_path": entry.get("runtime_prefab_path"),
            "lod_profile": entry.get("lod_profile"),
            "test_cases": _as_list(entry.get("test_cases")),
            "requirements": requirements,
            "evidence": evidence,
            "replacement_policy": "Replace only with an authorized final resource; never upgrade this record automatically.",
        })

    priority_records = sorted(
        (record for record in records if record["queue_priority"] == "vertical_slice"),
        key=lambda record: (record["priority_rank"], record["id"], record["path"]),
    )
    general_records = sorted(
        (record for record in records if record["queue_priority"] == "general"),
        key=lambda record: (record["category"], record["id"], record["path"]),
    )
    ordered_records = priority_records + general_records
    batches: list[dict[str, Any]] = []
    batch_number = 0
    scopes: list[tuple[str, str, list[dict[str, Any]]]] = []
    if priority_records:
        scopes.append(("vertical_slice_first", "vertical_slice", priority_records))
    for category in sorted({record["category"] for record in general_records}):
        scopes.append(("category", category, [
            record for record in general_records if record["category"] == category
        ]))
    for queue_scope, category, scoped in scopes:
        for offset in range(0, len(scoped), batch_size):
            batch_number += 1
            batch_records = scoped[offset : offset + batch_size]
            batch_id = f"D16-A05-{batch_number:03d}"
            for record in batch_records:
                record["batch_id"] = batch_id
                record["batch_number"] = batch_number
            batches.append({
                "batch_id": batch_id,
                "queue_scope": queue_scope,
                "category": category,
                "record_categories": dict(sorted(Counter(
                    record["category"] for record in batch_records
                ).items())),
                "record_count": len(batch_records),
                "record_ids": [record["id"] for record in batch_records],
            })

    requirement_counts = Counter(
        requirement for record in ordered_records for requirement in record["requirements"]
    )
    category_counts = Counter(record["category"] for record in ordered_records)
    priority_counts = Counter(record["queue_priority"] for record in ordered_records)
    pending_priority_ids = {record["id"] for record in priority_records}
    priority_ids_not_pending = [
        asset_id for asset_id, _ in sorted(VERTICAL_SLICE_PRIORITY_IDS.items(), key=lambda item: item[1])
        if asset_id not in pending_priority_ids
    ]
    return {
        "task": "D16-A05",
        "status": "BLOCKED" if records else "PASS",
        "root": root.as_posix(),
        "placeholder_count": len(records),
        "batch_size": batch_size,
        "batch_count": len(batches),
        "category_counts": dict(sorted(category_counts.items())),
        "priority_counts": dict(sorted(priority_counts.items())),
        "vertical_slice_priority_ids": [
            asset_id for asset_id, _ in sorted(VERTICAL_SLICE_PRIORITY_IDS.items(), key=lambda item: item[1])
        ],
        "vertical_slice_pending_ids": [record["id"] for record in priority_records],
        "vertical_slice_priority_ids_not_pending": priority_ids_not_pending,
        "requirement_counts": dict(sorted(requirement_counts.items())),
        "batches": batches,
        "records": ordered_records,
        "policy": "Queue only; no placeholder is upgraded and no final asset is inferred.",
    }


def _markdown_value(value: Any) -> str:
    if value is None:
        return ""
    if isinstance(value, list):
        return "; ".join(_markdown_value(item) for item in value)
    if isinstance(value, dict):
        return json.dumps(value, ensure_ascii=False, sort_keys=True)
    return str(value).replace("|", "\\|").replace("\n", " ")


def render_markdown(queue: dict[str, Any]) -> str:
    lines = [
        "# D16-A05 Placeholder 替换队列",
        "",
        "> 该文件只登记替换前置条件，不批准授权、不替换资源、不升级 `asset_state`。",
        "",
        f"- 状态：**{queue['status']}**",
        f"- Placeholder：**{queue['placeholder_count']}**",
        f"- 批次：**{queue['batch_count']}**（每批最多 {queue['batch_size']} 条）",
        f"- 类别统计：`{json.dumps(queue['category_counts'], ensure_ascii=False, sort_keys=True)}`",
        f"- 队列优先级统计：`{json.dumps(queue['priority_counts'], ensure_ascii=False, sort_keys=True)}`",
        f"- 垂直切片优先 ID（当前队列内）：`{', '.join(queue['vertical_slice_pending_ids'])}`",
        f"- 优先表中不在 placeholder 队列的 ID：`{', '.join(queue['vertical_slice_priority_ids_not_pending']) or '无'}`",
        f"- 缺口统计：`{json.dumps(queue['requirement_counts'], ensure_ascii=False, sort_keys=True)}`",
        "- 地面碰撞、角色姿态和上下船动作仍由运行时/动画专项负责，不会由 A05 资源队列虚构为已完成。",
        "",
        "## 批次",
        "",
        "| 批次 | 队列范围 | 类别/范围 | 条目数 | 首条 ID | 末条 ID |",
        "|---|---|---|---:|---|---|",
    ]
    for batch in queue["batches"]:
        ids = batch["record_ids"]
        lines.append("| " + " | ".join(_markdown_value(value) for value in (
            batch["batch_id"], batch["queue_scope"], batch["category"], batch["record_count"],
            ids[0] if ids else "", ids[-1] if ids else "",
        )) + " |")
    lines.extend([
        "",
        "## 条目",
        "",
        "| 批次 | 队列优先级 | ID | 类别 | 当前资源 | 当前 Prefab | 审查 | 授权 | 必须补齐 |",
        "|---|---|---|---|---|---|---|---|---|",
    ])
    for record in queue["records"]:
        lines.append("| " + " | ".join(_markdown_value(value) for value in (
            record["batch_id"], record["queue_priority"], record["id"], record["category"],
            record["resource_path"], record["runtime_prefab_path"],
            record["historical_review"], record["evidence"]["authorization_present"],
            record["requirements"],
        )) + " |")
    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description="Create the D16-A05 placeholder replacement queue")
    parser.add_argument("root", type=Path)
    parser.add_argument("--batch-size", type=int, default=DEFAULT_BATCH_SIZE)
    parser.add_argument("--json-out", type=Path)
    parser.add_argument("--markdown-out", type=Path)
    args = parser.parse_args()
    queue = build_queue(normalize_project_root(args.root), batch_size=args.batch_size)
    output = json.dumps(queue, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    if args.markdown_out:
        args.markdown_out.parent.mkdir(parents=True, exist_ok=True)
        args.markdown_out.write_text(render_markdown(queue), encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
