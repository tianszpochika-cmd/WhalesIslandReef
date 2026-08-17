#!/usr/bin/env python3
"""D18-L07 distribution rights validator (read-only).

Verifies that each asset record carries an explicit conclusion AND evidence
for the four distribution rights defined in the D18 governance baseline:
commercial use, modification, repository storage, and distribution with the
game build.

Policies (frozen by D18-L01 and this task card):

- Every one of the four rights must resolve to an explicit "allowed" or
  "denied" value backed by an evidence file present in the repository.
- "false", "unknown" and "contradictory" values are kept distinct.
- "free" or "downloadable" claims are NEVER interpreted as permission for
  commercial use or redistribution; they can only restrict, never grant.
- Any unknown / contradiction / missing evidence / explicit denial blocks
  Runtime/Release migration for that record (migration_allowed=False).
- The tool never writes approval status and never accesses the network.

Input record keys (superset of manifest / intake catalog license fields):

    record_id, source, license_id, license_url, evidence_path,
    commercial_use (bool), modification_allowed (bool),
    source_redistribution ("allowed"|"not_allowed"|...),
    build_distribution ("allowed"|"not_allowed"|...),
    free_claims (list[str]), downloadable (bool), notes (str)
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any

RIGHT_KEYS = (
    "commercial_use",
    "modification_allowed",
    "source_redistribution",
    "build_distribution",
)

NON_COMMERCIAL_MARKERS = [
    "non-commercial",
    "noncommercial",
    "not for commercial",
    "commercial use prohibited",
    "no commercial",
    "personal use only",
    "personal-use",
    "private use",
    "仅个人",
    "个人使用",
    "非商业",
    "禁止商用",
    "不可商用",
]

ISOLATED_NC_PATTERN = re.compile(r"(^|[-\s_.])(nc|ncl)([-\s_.]|$)", re.IGNORECASE)

FREE_CLAIM_MARKERS = ["free", "gratis", "免费", "无料"]
DOWNLOAD_CLAIM_MARKERS = ["download", "downloadable", "下载", "可下载"]


def normalize_bool_right(value: Any) -> str:
    """Return allowed/denied/unknown for boolean-typed rights.

    Only real JSON booleans are accepted as explicit conclusions; strings such
    as "true"/"yes" are treated as unknown (conservative, never widening).
    """
    if value is True:
        return "allowed"
    if value is False:
        return "denied"
    return "unknown"


def normalize_distribution_right(value: Any) -> str:
    """Return allowed/denied/unknown for distribution-typed rights.

    "not_applicable" and "pending" are treated as unknown: they are not
    explicit conclusions and must block migration until clarified.
    """
    if isinstance(value, str):
        lowered = value.strip().lower()
        if lowered == "allowed":
            return "allowed"
        if lowered == "not_allowed":
            return "denied"
    return "unknown"


def normalize_right(value: Any, right: str) -> str:
    """Dispatch to the right normalizer for the given right key."""
    if right in ("commercial_use", "modification_allowed"):
        return normalize_bool_right(value)
    return normalize_distribution_right(value)


def contains_nc_marker(license_id: Any, notes: Any, claims: list[str]) -> list[str]:
    """Return non-commercial markers found in license id / notes / claims.

    NC license ids (e.g. CC-BY-NC-4.0) are matched via an isolated-token
    pattern so that ids such as "CC0-1.0" or "OFL-1.1" are never misread.
    """
    found: list[str] = []
    pieces = [str(license_id or ""), str(notes or "")]
    pieces.extend(str(c) for c in claims if c)
    joined = " | ".join(pieces).lower()
    for marker in NON_COMMERCIAL_MARKERS:
        if marker.lower() in joined:
            found.append(marker)
    if found:
        return found
    if isinstance(license_id, str) and ISOLATED_NC_PATTERN.search(license_id):
        found.append(f"license_id {license_id!r} contains an NC (non-commercial) token")
    return found


def contains_free_or_download_claim(notes: Any, claims: list[str], downloadable: Any) -> bool:
    """True if the record contains free/downloadable claims (non-evidence)."""
    if downloadable is True:
        return True
    pieces = [str(notes or "")]
    pieces.extend(str(c) for c in claims if c)
    joined = " ".join(pieces).lower()
    for marker in FREE_CLAIM_MARKERS + DOWNLOAD_CLAIM_MARKERS:
        if marker.lower() in joined:
            return True
    return False


def _evidence_problem(record: dict[str, Any], root: Path) -> tuple[bool, list[str]]:
    """Check evidence_path validity and existence under root.

    Returns (ok, problems). Absolute paths, parent escapes and missing files
    are all treated as missing evidence.
    """
    raw = record.get("evidence_path")
    if raw is None or str(raw).strip() == "":
        return False, ["evidence_path is empty or missing"]
    candidate = Path(str(raw).strip())
    if candidate.is_absolute():
        return False, [f"evidence_path is absolute: {raw}"]
    if ".." in candidate.parts:
        return False, [f"evidence_path escapes repository root: {raw}"]
    if (root / candidate).is_file():
        return True, []
    return False, [f"evidence file does not exist under root: {raw}"]


def validate_record(record: dict[str, Any], root: Path | None = None) -> dict[str, Any]:
    """Validate one rights record and return a full result dict.

    The result is a plain data object: this function never writes approval
    state, never edits any file, and never touches the network.
    """
    if root is None:
        root = Path.cwd()
    record_id = str(record.get("record_id") or record.get("pack_id") or "unknown")
    source = str(record.get("source") or "manual")

    values: dict[str, str] = {right: normalize_right(record.get(right), right) for right in RIGHT_KEYS}

    nc_markers = contains_nc_marker(
        record.get("license_id"), record.get("notes"), record.get("free_claims") or []
    )
    has_free_claim = contains_free_or_download_claim(
        record.get("notes"), record.get("free_claims") or [], record.get("downloadable")
    )

    if nc_markers and values["commercial_use"] == "unknown":
        values["commercial_use"] = "denied"

    evidence_ok, evidence_problems = _evidence_problem(record, root)

    blockers: list[dict[str, str]] = []
    if not evidence_ok:
        for problem in evidence_problems:
            blockers.append({"right": "evidence", "kind": "missing_evidence", "detail": problem})

    for right in RIGHT_KEYS:
        value = values[right]
        if value == "allowed":
            if right == "commercial_use" and nc_markers:
                blockers.append(
                    {
                        "right": right,
                        "kind": "conflict",
                        "detail": f"declared allowed but non-commercial marker(s) present: {', '.join(nc_markers)}",
                    }
                )
        elif value == "denied":
            if right == "commercial_use" and nc_markers and record.get(right) is None:
                detail = f"non-commercial marker(s) present: {', '.join(nc_markers)}"
            else:
                detail = "explicitly denied by the record"
            blockers.append({"right": right, "kind": "denied", "detail": detail})
        else:
            if has_free_claim:
                detail = "no explicit conclusion; free/downloadable claims are not permission evidence"
            else:
                detail = "no explicit conclusion in the record"
            blockers.append({"right": right, "kind": "unknown", "detail": detail})

    rights_out: dict[str, dict[str, Any]] = {}
    for right in RIGHT_KEYS:
        raw = record.get(right)
        if right in ("commercial_use", "modification_allowed"):
            explicit = raw is True or raw is False
        else:
            explicit = isinstance(raw, str) and raw.strip().lower() in ("allowed", "not_allowed")
        rights_out[right] = {
            "value": values[right],
            "explicit": explicit,
        }

    verdict = "CLEARED" if not blockers else "BLOCKED"
    return {
        "record_id": record_id,
        "source": source,
        "verdict": verdict,
        "migration_allowed": verdict == "CLEARED",
        "rights": rights_out,
        "evidence": {"path": record.get("evidence_path"), "present": evidence_ok},
        "blockers": blockers,
        "claims_guarded": has_free_claim,
        "non_commercial_markers": nc_markers,
    }


def compare_records(primary: dict[str, Any], secondary: dict[str, Any]) -> list[dict[str, str]]:
    """Detect per-right contradictions between two sources of the same record.

    Only verdict mismatches (allowed vs denied vs unknown) are reported.
    """
    conflicts: list[dict[str, str]] = []
    for right in RIGHT_KEYS:
        a = normalize_right(primary.get(right), right)
        b = normalize_right(secondary.get(right), right)
        if a != b:
            conflicts.append(
                {
                    "right": right,
                    "kind": "conflict",
                    "detail": f"{primary.get('source', 'primary')}={a} vs {secondary.get('source', 'secondary')}={b}",
                }
            )
    return conflicts


def record_from_manifest(manifest: dict[str, Any]) -> dict[str, Any]:
    """Build a rights record from a d18_asset_pack_manifest object."""
    return {
        "record_id": manifest.get("pack_id") or manifest.get("manifest_id") or "unknown",
        "source": "manifest",
        "license_id": manifest.get("license_id"),
        "license_url": manifest.get("license_url"),
        "evidence_path": manifest.get("license_evidence_path"),
        "commercial_use": manifest.get("commercial_use"),
        "modification_allowed": manifest.get("modification_allowed"),
        "source_redistribution": manifest.get("source_redistribution"),
        "build_distribution": manifest.get("build_distribution"),
        "attribution_required": manifest.get("attribution_required"),
        "notes": manifest.get("notes"),
        "free_claims": [],
        "downloadable": None,
    }


def record_from_intake_entry(entry: dict[str, Any]) -> dict[str, Any]:
    """Build a rights record from a d18_prefab_candidate_intake_catalog entry."""
    license_obj = entry.get("license") or {}
    return {
        "record_id": entry.get("intake_id") or "unknown",
        "source": "intake_catalog",
        "license_id": license_obj.get("license_id"),
        "license_url": license_obj.get("license_url"),
        "evidence_path": license_obj.get("license_evidence_path"),
        "commercial_use": license_obj.get("commercial_use"),
        "modification_allowed": license_obj.get("modification_allowed"),
        "source_redistribution": license_obj.get("source_redistribution"),
        "build_distribution": license_obj.get("build_distribution"),
        "notes": entry.get("known_gaps"),
        "free_claims": [],
        "downloadable": None,
    }


def discover_manifest_paths(root: Path, extra_dirs: list[str] | None = None) -> list[Path]:
    """Collect every manifest file under the canonical D18 asset dirs.

    Both ".manifest.json" and "_manifest.json" suffixes are matched (one
    historical pack uses "D18_A04_R02_C_manifest.json"), deduplicated and
    sorted for a deterministic scan order.
    """
    patterns = [
        "docs/game/design/common/assets/manifests/*.manifest.json",
        "docs/game/design/common/assets/packs/**/*.manifest.json",
        "docs/game/design/common/assets/packs/**/*_manifest.json",
    ]
    patterns.extend(extra_dirs or [])
    seen: set[Path] = set()
    for pattern in patterns:
        for path in root.glob(pattern):
            resolved = path.resolve()
            if resolved not in seen:
                seen.add(resolved)
    return sorted(seen, key=str)


def scan_manifests(root: Path, extra_dirs: list[str] | None = None) -> list[dict[str, Any]]:
    """Validate every discovered manifest and return per-record results."""
    results: list[dict[str, Any]] = []
    for path in discover_manifest_paths(root, extra_dirs):
        try:
            manifest = json.loads(path.read_text(encoding="utf-8"))
        except (json.JSONDecodeError, OSError) as exc:
            results.append(
                {
                    "record_id": str(path),
                    "source": "manifest",
                    "verdict": "BLOCKED",
                    "migration_allowed": False,
                    "blockers": [
                        {"right": "evidence", "kind": "missing_evidence", "detail": f"unreadable manifest: {exc}"}
                    ],
                }
            )
            continue
        result = validate_record(record_from_manifest(manifest), root)
        result["manifest_path"] = str(path.relative_to(root)).replace("\\", "/")
        results.append(result)
    return results


def scan_intake_catalog(
    root: Path, catalog_path: str | None = None
) -> tuple[list[dict[str, Any]], int, int]:
    """Validate every eligible intake entry.

    Returns (results, entry_count, manifest_count). Each entry is also
    cross-checked against its own manifest when the manifest was scanned, so
    a manifest/catalog contradiction is reported as a conflict blocker on the
    entry.
    """
    if catalog_path is None:
        catalog_path = "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"
    catalog = json.loads((root / catalog_path).read_text(encoding="utf-8"))
    entries = catalog.get("eligible_entries") or []
    manifest_results = scan_manifests(root)

    manifest_data: dict[str, dict[str, Any]] = {}
    for path in discover_manifest_paths(root):
        try:
            manifest_data[str(path.relative_to(root)).replace("\\", "/")] = json.loads(
                path.read_text(encoding="utf-8")
            )
        except (json.JSONDecodeError, OSError):
            pass

    results: list[dict[str, Any]] = []
    for entry in entries:
        record = record_from_intake_entry(entry)
        result = validate_record(record, root)
        manifest_rel = str(entry.get("manifest_path") or "").replace("\\", "/")
        if manifest_rel in manifest_data:
            conflicts = compare_records(record, record_from_manifest(manifest_data[manifest_rel]))
            for conflict in conflicts:
                result["blockers"].append(conflict)
            if conflicts and result["verdict"] == "CLEARED":
                result["verdict"] = "BLOCKED"
                result["migration_allowed"] = False
        result["manifest_path"] = manifest_rel
        results.append(result)
    return results, len(entries), len(manifest_results)


def build_report(root: Path, results: list[dict[str, Any]], sources: dict[str, int]) -> dict[str, Any]:
    """Aggregate per-record results into a deterministic report dict."""
    cleared = sum(1 for r in results if r.get("verdict") == "CLEARED")
    blocked = sum(1 for r in results if r.get("verdict") == "BLOCKED")
    migrated = sum(1 for r in results if r.get("migration_allowed"))
    kinds: dict[str, int] = {}
    for r in results:
        for b in r.get("blockers") or []:
            kind = b.get("kind")
            kinds[kind] = kinds.get(kind, 0) + 1
    return {
        "tool": "d18_distribution_rights_validator",
        "task_id": "D18-L07",
        "read_only": True,
        "network_access": False,
        "approval_status_unchanged": True,
        "sources": sources,
        "totals": {
            "records": len(results),
            "cleared": cleared,
            "blocked": blocked,
            "migration_allowed": migrated,
        },
        "blocker_kind_counts": kinds,
    }


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="D18-L07 distribution rights validator (read-only)")
    parser.add_argument("--root", default=None, help="repository root (default: two levels above this file)")
    parser.add_argument("--output", default=None, help="write the JSON scan report to this path")
    parser.add_argument("--record", default=None, help="validate a single rights record JSON file")
    parser.add_argument("--no-intake", action="store_true", help="skip the intake catalog scan")
    parser.add_argument("--max-records", type=int, default=None, help="cap the number of intake entries scanned")
    args = parser.parse_args(argv)

    root = Path(args.root) if args.root else Path(__file__).resolve().parents[1]

    if args.record:
        record = json.loads(Path(args.record).read_text(encoding="utf-8"))
        result = validate_record(record, root)
        print(json.dumps(result, ensure_ascii=False, indent=2))
        if args.output:
            Path(args.output).write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        return 0 if result["verdict"] == "CLEARED" else 1

    all_results: list[dict[str, Any]] = []
    sources: dict[str, int] = {}

    manifest_results = scan_manifests(root)
    all_results.extend(manifest_results)
    sources["manifests"] = len(manifest_results)

    intake_results, entry_count, manifest_count = (
        scan_intake_catalog(root) if not args.no_intake else ([], 0, 0)
    )
    if args.max_records is not None:
        intake_results = intake_results[: args.max_records]
    all_results.extend(intake_results)
    sources["intake_catalog_entries_scanned"] = len(intake_results)
    sources["intake_catalog_total"] = entry_count

    report = build_report(root, all_results, sources)
    payload = {"report": report, "results": all_results}
    if args.output:
        Path(args.output).write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
