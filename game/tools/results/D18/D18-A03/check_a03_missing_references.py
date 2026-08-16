"""Read-only missing-reference and boundary check for D18-A03 evidence."""

from __future__ import annotations

import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[5]
RESULT_ROOT = ROOT / "game" / "tools" / "results" / "D18" / "D18-A03"
MANIFEST_ROOT = ROOT / "docs" / "game" / "design" / "common" / "assets" / "packs" / "environment"


def main() -> None:
    audit = json.loads((RESULT_ROOT / "D18_A03_UNITY_IMPORT_AUDIT.json").read_text(encoding="utf-8"))
    issues: list[dict[str, str]] = []
    for record in audit.get("assets", []):
        if not record.get("source_exists", False):
            issues.append({"code": "source_missing", "path": record.get("path", "")})
        if record.get("missing_script_count", 0):
            issues.append({"code": "missing_script", "path": record.get("path", "")})
        if record.get("missing_reference_count", 0):
            issues.append({"code": "missing_reference", "path": record.get("path", "")})
        if any(token in record.get("path", "") for token in ("InitialHarbor", "Registry", "Assets/Prefabs")):
            issues.append({"code": "forbidden_runtime_boundary_reference", "path": record.get("path", "")})

    manifest_count = 0
    runtime_consumers: list[str] = []
    for path in sorted(MANIFEST_ROOT.glob("*.manifest.json"), key=lambda item: item.name.casefold()):
        manifest_count += 1
        manifest = json.loads(path.read_text(encoding="utf-8"))
        runtime_consumers.extend(manifest.get("runtime_consumers", []))
        for candidate in manifest.get("candidate_files", []):
            candidate_path = candidate.get("path", "")
            if ".." in Path(candidate_path).parts:
                issues.append({"code": "candidate_path_escape", "path": candidate_path})
    for consumer in runtime_consumers:
        issues.append({"code": "unapproved_runtime_consumer", "path": consumer})

    result = {
        "task": "D18-A03",
        "status": "PASS" if not issues else "BLOCKED",
        "read_only": True,
        "registry_modified": False,
        "initial_harbor_integrated": False,
        "manifest_count": manifest_count,
        "audited_asset_count": len(audit.get("assets", [])),
        "missing_script_count": sum(record.get("missing_script_count", 0) for record in audit.get("assets", [])),
        "missing_reference_count": sum(record.get("missing_reference_count", 0) for record in audit.get("assets", [])),
        "runtime_consumer_count": len(runtime_consumers),
        "issues": issues,
    }
    (RESULT_ROOT / "D18_A03_MISSING_REFERENCE_CHECK.json").write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False))
    raise SystemExit(0 if not issues else 1)


if __name__ == "__main__":
    main()
