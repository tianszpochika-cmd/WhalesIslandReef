"""Fill missing D16-A04 review structure without approving historical content.

Only entries that already carry a historical tag and lack one or more review
fields are changed. They remain Pending; existing source_refs are copied as
candidate evidence references, never as proof of approval.
"""

import argparse
import json
from pathlib import Path


def load_entries(root: Path):
    for path in sorted(root.rglob("*.json")):
        if path.name == "registry.json":
            continue
        try:
            entry = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            continue
        if isinstance(entry, dict) and entry.get("id"):
            yield path, entry


def pending_candidates(root: Path):
    for path, entry in load_entries(root):
        if "historical_tag" not in entry:
            continue
        missing = (
            entry.get("historical_review") not in {"Pending", "Approved", "ApprovedWithWarning", "Rejected"}
            or not isinstance(entry.get("review_evidence_refs"), list)
            or not entry.get("review_evidence_refs")
            or not isinstance(entry.get("content_warning_ids"), list)
            or not isinstance(entry.get("skip_allowed"), bool)
        )
        if missing:
            yield path, entry


def apply(root: Path) -> list[str]:
    changed = []
    for path, entry in pending_candidates(root):
        refs = entry.get("source_refs")
        if not isinstance(refs, list) or not refs or not all(refs):
            raise ValueError(f"{entry['id']} has no usable source_refs; refusing to invent evidence")

        entry["historical_review"] = "Pending"
        entry["review_evidence_refs"] = list(refs)
        entry["content_warning_ids"] = []
        entry["skip_allowed"] = False
        path.write_text(json.dumps(entry, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        changed.append(entry["id"])
    return changed


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("root", type=Path)
    parser.add_argument("--apply", action="store_true")
    args = parser.parse_args()

    candidates = list(pending_candidates(args.root))
    if not args.apply:
        print(f"pending_metadata_candidates={len(candidates)}")
        return 0

    changed = apply(args.root)
    print(f"updated_pending_metadata={len(changed)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
