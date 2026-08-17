"""D18-L30 asset dependency graph builder and read-only checker.

Builds a stable, deterministic directed dependency graph from static JSON
inputs only (asset records + import-descriptor-style documents) and detects:

* ``cycle`` — directed cycles (strongly connected components with more than
  one node, and self-loops);
* ``dangling_reference`` / ``dangling_source`` — an edge whose target (or
  source) is not a declared node;
* ``cross_package_incompatibility`` — an edge between two packages declared
  incompatible (either globally in the ``--packages`` file or per-node via
  ``incompatible_packages``);
* ``state_skip_edge`` — a dependency edge that violates the frozen D18 state
  machine (``D18_ASSET_STATE_MACHINE.md`` section 4.2/4.4 forbidden pairs),
  e.g. a Candidate asset directly linked to a RuntimeApproved/ReleaseApproved
  asset without passing IntakeEligible -> RuntimeRequested;
* ``missing_license`` / ``missing_license_evidence`` — a node without a
  license object, with an unspecified license id, or without license evidence.

Input contract (fixed, testable, read-only):

* ``--records``: asset records shaped like the frozen L02 schema
  (``tools/schemas/d18_logic_asset_record.schema.json``). Used fields:
  ``canonical_asset_id``, ``state``, ``license`` (``license_id``,
  ``license_evidence_path``), optional ``package``/``manifest_id``/``pack_id``,
  optional ``incompatible_packages`` and optional ``dependencies`` extension
  (the L02 schema itself has ``additionalProperties: false``; a graph input
  may carry edge declarations as a documented extension).
* ``--descriptors``: import-descriptor-style documents. This is the D18-L30
  dependency-graph input contract — NOT the L25 import descriptor schema
  (``tools/schemas/d18_import_descriptor.schema.json`` is owned by D18-L25 and
  was still pending at authoring time). A descriptor is an object with
  ``asset_id`` (required), optional ``package``, optional ``state``, optional
  ``license``, optional ``incompatible_packages`` and optional ``dependencies``
  (list of asset-id strings or ``{"asset_id": ...}`` objects). Harvesting
  field-level references from the future L25 descriptor fields (material
  slots, skeleton, animations, bounds, ...) is a wave-3/L45 alignment task.
* ``--packages``: optional file declaring global incompatibilities:
  ``{"incompatible_pairs": [["pack_a", "pack_b"], ...]}`` (order-insensitive).

The tool never reads or hashes binaries, never imports assets, never writes
the catalog/registry and never grants any state (``approved_count`` is always
0). Output is a single fixed JSON document; ordering is fully deterministic
(nodes, edges, cycles and issues are sorted) and ``fingerprint`` is the SHA-256
of the canonical graph core so stability is machine-testable.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import sys
from pathlib import Path
from typing import Any, Iterable

STATUS_OK = "OK"
STATUS_PENDING = "PENDING"
STATUS_ERROR = "error"

# Frozen seven-state vocabulary (D18_ASSET_STATE_MACHINE.md section 3).
STATES = (
    "Candidate",
    "IntakeEligible",
    "RuntimeRequested",
    "RuntimeApproved",
    "ReleaseApproved",
    "Rejected",
    "Revoked",
)

# Frozen forbidden pairs (D18_ASSET_STATE_MACHINE.md section 4.4 expanded):
# Candidate has no direct path to RuntimeRequested/RuntimeApproved/
# ReleaseApproved; IntakeEligible/RuntimeRequested cannot skip ahead;
# ReleaseApproved cannot be Rejected; Rejected is terminal; no state may enter
# ReleaseApproved except the explicit T4 edge RuntimeApproved -> ReleaseApproved.
FORBIDDEN_STATE_PAIRS: set[tuple[str, str]] = set()

for _from, _to in (
    ("Candidate", "RuntimeRequested"),
    ("Candidate", "RuntimeApproved"),
    ("Candidate", "ReleaseApproved"),
    ("IntakeEligible", "RuntimeApproved"),
    ("IntakeEligible", "ReleaseApproved"),
    ("RuntimeRequested", "ReleaseApproved"),
    ("ReleaseApproved", "Rejected"),
):
    FORBIDDEN_STATE_PAIRS.add((_from, _to))
for _state in STATES:
    FORBIDDEN_STATE_PAIRS.add(("Rejected", _state))
    if _state != "RuntimeApproved":  # T4 explicit exception
        FORBIDDEN_STATE_PAIRS.add((_state, "ReleaseApproved"))
del _from, _to, _state

UNSPECIFIED_LICENSE_IDS = {"", "unknown", "pending", "n/a", "na", "none"}

ROLE_KINDS = {"dependencies", "references"}


def load_json_file(path: Path) -> tuple[Any | None, str, str]:
    """Load a JSON file; return (data, error_code, message)."""

    try:
        return json.loads(path.read_text(encoding="utf-8")), "", ""
    except FileNotFoundError:
        return None, "file_not_found", f"file does not exist: {path}"
    except json.JSONDecodeError as exc:
        return None, "file_json_error", f"line {exc.lineno}: {exc.msg} in {path.name}"
    except OSError as exc:
        return None, "file_read_error", str(exc)


def load_items(path: Path, key: str) -> tuple[list[Any] | None, str, str]:
    """Load a list, ``{"records": [...]}`` or ``{"descriptors": [...]}`` file."""

    data, code, message = load_json_file(path)
    if data is None:
        return None, code, message
    if isinstance(data, list):
        return data, "", ""
    if isinstance(data, dict) and isinstance(data.get(key), list):
        return data[key], "", ""
    return None, f"{key}_not_supported", f"JSON must be a list or {{\"{key}\": [...]}}: {path}"


def load_packages(path: Path) -> tuple[set[tuple[str, str]] | None, str, str]:
    """Load global incompatible package pairs from a JSON file.

    Accepted shape: ``{"incompatible_pairs": [["pack_a", "pack_b"], ...]}``.
    Each pair must have exactly two distinct non-empty strings; pairs are
    normalized order-insensitively.
    """

    data, code, message = load_json_file(path)
    if data is None:
        return None, code, message
    raw_pairs = data.get("incompatible_pairs") if isinstance(data, dict) else None
    if raw_pairs is None:
        return None, "packages_not_supported", f"packages JSON must be an object with \"incompatible_pairs\": {path}"
    pairs: set[tuple[str, str]] = set()
    for raw in raw_pairs:
        if not isinstance(raw, list) or len(raw) != 2 or not all(isinstance(item, str) and item.strip() for item in raw):
            return None, "incompatible_pair_invalid", f"each incompatible pair must be [pack_a, pack_b] with two distinct non-empty strings: {raw!r}"
        left, right = raw[0].strip(), raw[1].strip()
        if left == right:
            return None, "incompatible_pair_invalid", f"incompatible pair must reference two distinct packages: {raw!r}"
        pairs.add(tuple(sorted((left, right))))
    return pairs, "", ""


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _package_of(node_data: dict[str, Any]) -> str:
    for field in ("package", "manifest_id", "pack_id"):
        value = _text(node_data.get(field))
        if value:
            return value
    return ""


def _license_info(node_data: dict[str, Any]) -> tuple[Any, str, str]:
    """Return (license_raw, license_id, license_evidence_path)."""

    license_raw = node_data.get("license")
    if isinstance(license_raw, dict):
        license_id = _text(license_raw.get("license_id"))
        evidence = _text(license_raw.get("license_evidence_path"))
        return license_raw, license_id, evidence
    return license_raw, "", ""


def _node_attrs(entry: Any, is_record: bool) -> dict[str, Any]:
    """Extract graph attributes from a record or descriptor entry."""

    if is_record:
        identifier = entry.get("canonical_asset_id")
        id_field = "canonical_asset_id"
    else:
        identifier = entry.get("asset_id")
        if identifier is None:
            identifier = entry.get("source_asset_id")
        id_field = "asset_id"
    return {
        "id": identifier if isinstance(identifier, str) else None,
        "id_field": id_field,
        "state": entry.get("state") if isinstance(entry, dict) else None,
        "package": _package_of(entry) if isinstance(entry, dict) else "",
        "license": entry.get("license") if isinstance(entry, dict) else None,
        "incompatible_packages": entry.get("incompatible_packages") if isinstance(entry, dict) else None,
    }


def _edge_entries(node_data: dict[str, Any], node_id: str, issues: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """Extract edge declarations from a node entry."""

    edges: list[dict[str, Any]] = []
    if not isinstance(node_data, dict):
        return edges
    for field in ROLE_KINDS:
        raw = node_data.get(field)
        if raw is None:
            continue
        if not isinstance(raw, list):
            issues.append({
                "code": "invalid_edges",
                "severity": "error",
                "message": f"{field} must be an array on node {node_id!r}",
                "node": node_id,
            })
            continue
        for entry in raw:
            if isinstance(entry, str):
                edges.append({"from": node_id, "to": entry.strip(), "kind": "depends_on"})
            elif isinstance(entry, dict):
                target = entry.get("asset_id")
                if not isinstance(target, str):
                    target = entry.get("target")
                if not isinstance(target, str):
                    target = entry.get("id")
                kind = _text(entry.get("kind")) or "depends_on"
                edges.append({"from": node_id, "to": target.strip() if isinstance(target, str) else "", "kind": kind})
            else:
                issues.append({
                    "code": "invalid_edge",
                    "severity": "error",
                    "message": f"edge entry on node {node_id!r} must be a string or an object with asset_id",
                    "node": node_id,
                })
    return edges


def _unknown_state(node_data: dict[str, Any]) -> bool:
    return node_data.get("state") not in STATES


def _node_license_issues(node_id: str, license_raw: Any, license_id: str, evidence: str) -> list[dict[str, Any]]:
    """Return license-related issues for one node."""

    issues: list[dict[str, Any]] = []
    if license_raw is None:
        issues.append({
            "code": "missing_license",
            "severity": "error",
            "message": "node declares no license object; license id and evidence are required for any D18 graph node",
            "node": node_id,
        })
        return issues
    if not isinstance(license_raw, dict):
        issues.append({
            "code": "missing_license",
            "severity": "error",
            "message": "license must be an object with license_id and license_evidence_path",
            "node": node_id,
        })
        return issues
    if not license_id or license_id.lower() in UNSPECIFIED_LICENSE_IDS:
        issues.append({
            "code": "missing_license",
            "severity": "error",
            "message": "license id is unspecified; unknown/pending/empty license must never be treated as granted",
            "node": node_id,
        })
    if not evidence:
        issues.append({
            "code": "missing_license_evidence",
            "severity": "error",
            "message": "license has no license_evidence_path; evidence must be a repo-relative path",
            "node": node_id,
        })
    return issues


def _tarjan_cycles(adjacency: dict[str, list[str]]) -> list[list[str]]:
    """Return strongly connected components with >1 node plus self-loops."""

    index: dict[str, int] = {}
    lowlink: dict[str, int] = {}
    on_stack: set[str] = set()
    stack: list[str] = []
    components: list[list[str]] = []
    counter = 0

    def strongconnect(node: str) -> None:
        nonlocal counter
        index[node] = counter
        lowlink[node] = counter
        counter += 1
        stack.append(node)
        on_stack.add(node)
        for successor in adjacency.get(node, ()):
            if successor not in index:
                strongconnect(successor)
                lowlink[node] = min(lowlink[node], lowlink[successor])
            elif successor in on_stack:
                lowlink[node] = min(lowlink[node], index[successor])
        if lowlink[node] == index[node]:
            component: list[str] = []
            while True:
                member = stack.pop()
                on_stack.discard(member)
                component.append(member)
                if member == node:
                    break
            components.append(sorted(component))

    for node in sorted(adjacency):
        if node not in index:
            strongconnect(node)

    cycles: list[list[str]] = []
    for component in components:
        if len(component) > 1:
            cycles.append(component)
        elif component and component[0] in adjacency.get(component[0], ()):
            cycles.append([component[0]])
    return sorted(cycles, key=lambda value: (len(value), value[0]))


def _edge_key(edge: dict[str, Any]) -> tuple[str, str, str]:
    return (edge["from"], edge["to"], edge.get("kind") or "depends_on")


def build_graph(
    records: Iterable[Any],
    descriptors: Iterable[Any] = (),
    *,
    incompatible_pairs: set[tuple[str, str]] | None = None,
    input_issues: Iterable[dict[str, Any]] = (),
) -> dict[str, Any]:
    """Build the dependency graph and return a deterministic result document."""

    issues: list[dict[str, Any]] = []
    nodes: dict[str, dict[str, Any]] = {}
    edge_map: dict[tuple[str, str, str], dict[str, Any]] = {}

    for entry in records:
        if not isinstance(entry, dict):
            issues.append({
                "code": "invalid_record",
                "severity": "error",
                "message": "record must be an object",
                "node": f"record[{len(nodes)}]",
            })
            continue
        attrs = _node_attrs(entry, is_record=True)
        node_id = attrs["id"]
        if not node_id:
            issues.append({
                "code": "invalid_node_id",
                "severity": "error",
                "message": f"record without non-empty {attrs['id_field']} is skipped",
                "node": attrs["id_field"],
            })
            continue
        if node_id in nodes:
            issues.append({
                "code": "duplicate_node",
                "severity": "error",
                "message": f"node {node_id!r} is declared more than once across inputs",
                "node": node_id,
            })
            continue
        nodes[node_id] = attrs
        for edge in _edge_entries(entry, node_id, issues):
            edge_map[_edge_key(edge)] = edge

    for entry in descriptors:
        if not isinstance(entry, dict):
            issues.append({
                "code": "invalid_descriptor",
                "severity": "error",
                "message": "descriptor must be an object",
                "node": f"descriptor[{len(nodes)}]",
            })
            continue
        attrs = _node_attrs(entry, is_record=False)
        node_id = attrs["id"]
        if not node_id:
            issues.append({
                "code": "invalid_node_id",
                "severity": "error",
                "message": "descriptor without non-empty asset_id is skipped",
                "node": "asset_id",
            })
            continue
        existing = nodes.get(node_id)
        if existing is not None:
            conflict_fields = []
            for field in ("state", "package", "license"):
                first, second = existing.get(field), attrs.get(field)
                if first is not None and second is not None and first != second:
                    conflict_fields.append(field)
            if conflict_fields:
                issues.append({
                    "code": "node_conflict",
                    "severity": "error",
                    "message": f"record and descriptor disagree on node {node_id!r}: {sorted(conflict_fields)}",
                    "node": node_id,
                })
        else:
            nodes[node_id] = attrs
        for edge in _edge_entries(entry, node_id, issues):
            edge_map[_edge_key(edge)] = edge

    edges = [dict(value) for value in edge_map.values()]

    if not nodes:
        issues.append({
            "code": "no_nodes",
            "severity": "error",
            "message": "no nodes were declared; nothing was verified",
            "node": "",
        })

    adjacency: dict[str, list[str]] = {}
    for edge in edges:
        adjacency.setdefault(edge["from"], [])
        adjacency.setdefault(edge["to"], [])
        adjacency[edge["from"]].append(edge["to"])
        if edge["to"] not in nodes:
            issues.append({
                "code": "dangling_reference",
                "severity": "error",
                "message": f"edge target {edge['to']!r} is not a declared node",
                "node": edge["from"],
                "edge": f"{edge['from']} -> {edge['to']}",
            })
        if not edge["to"]:
            issues.append({
                "code": "invalid_edge",
                "severity": "error",
                "message": f"edge from {edge['from']!r} has an empty target",
                "node": edge["from"],
                "edge": f"{edge['from']} -> {edge['to']}",
            })
    for node_id in nodes:
        adjacency.setdefault(node_id, [])

    cycles = _tarjan_cycles(adjacency)
    for cycle in cycles:
        issues.append({
            "code": "cycle",
            "severity": "error",
            "message": "dependency cycle detected",
            "node": cycle[0],
            "cycle": cycle,
        })

    packages = sorted({nodes[node_id]["package"] for node_id in nodes if nodes[node_id]["package"]})
    incompatible_pairs = {tuple(sorted(pair)) for pair in (incompatible_pairs or ())}

    for edge in edges:
        from_id, to_id = edge["from"], edge["to"]
        if from_id not in nodes or to_id not in nodes:
            continue
        from_package = nodes[from_id]["package"]
        to_package = nodes[to_id]["package"]
        if from_package and to_package:
            pair = tuple(sorted((from_package, to_package)))
            if pair in incompatible_pairs:
                issues.append({
                    "code": "cross_package_incompatibility",
                    "severity": "error",
                    "message": f"edge crosses packages declared globally incompatible: {from_package!r} <-> {to_package!r}",
                    "node": from_id,
                    "edge": f"{from_id} -> {to_id}",
                })
        declared = nodes[from_id].get("incompatible_packages")
        if isinstance(declared, list) and to_package and any(isinstance(item, str) and item == to_package for item in declared):
            issues.append({
                "code": "cross_package_incompatibility",
                "severity": "error",
                "message": f"source node declares package {to_package!r} incompatible but depends on it",
                "node": from_id,
                "edge": f"{from_id} -> {to_id}",
            })

    for edge in edges:
        from_id, to_id = edge["from"], edge["to"]
        if from_id not in nodes or to_id not in nodes:
            continue
        from_state = nodes[from_id].get("state")
        to_state = nodes[to_id].get("state")
        if from_state in STATES and to_state in STATES:
            pair = (from_state, to_state)
            if pair in FORBIDDEN_STATE_PAIRS:
                issues.append({
                    "code": "state_skip_edge",
                    "severity": "error",
                    "message": (
                        f"dependency edge violates the frozen D18 state machine: "
                        f"{from_state} -> {to_state} is a forbidden pair (unique path requires "
                        f"T1 IntakeEligible -> T2 RuntimeRequested -> T3 RuntimeApproved -> T4 ReleaseApproved)"
                    ),
                    "node": from_id,
                    "edge": f"{from_id} -> {to_id}",
                })

    for node_id in sorted(nodes):
        state = nodes[node_id].get("state")
        if state is not None and state not in STATES:
            issues.append({
                "code": "unknown_state",
                "severity": "error",
                "message": f"state {state!r} is not one of the frozen seven states",
                "node": node_id,
            })

    for node_id in sorted(nodes):
        attrs = nodes[node_id]
        license_raw, license_id, evidence = _license_info(attrs)
        issues.extend(_node_license_issues(node_id, license_raw, license_id, evidence))

    issues.extend(input_issues)

    error_count = sum(1 for issue in issues if issue.get("severity") == STATUS_ERROR)
    if error_count > 0:
        status = "BLOCKED"
    else:
        status = "PASS"

    code_counts: dict[str, int] = {}
    for issue in issues:
        code_counts[issue["code"]] = code_counts.get(issue["code"], 0) + 1

    sorted_issues = sorted(
        issues,
        key=lambda item: (item["code"], str(item.get("message")), str(item.get("node")), str(item.get("edge"))),
    )

    node_rows = sorted(
        [
            {
                "asset_id": node_id,
                "state": nodes[node_id].get("state"),
                "package": nodes[node_id]["package"],
                "license_id": _license_info(nodes[node_id])[1] or None,
                "license_evidence_path": _license_info(nodes[node_id])[2] or None,
            }
            for node_id in nodes
        ],
        key=lambda row: row["asset_id"],
    )
    for row in node_rows:
        if row["state"] not in STATES:
            row.pop("state", None)
    edge_rows = sorted(
        [
            {"from": edge["from"], "to": edge["to"], "kind": edge.get("kind") or "depends_on"}
            for edge in edges
        ],
        key=lambda row: (row["from"], row["to"], row["kind"]),
    )
    cycle_rows = [sorted(cycle) for cycle in cycles]

    canonical_core = {
        "nodes": node_rows,
        "edges": edge_rows,
        "incompatible_pairs": [list(pair) for pair in sorted(incompatible_pairs)],
    }
    fingerprint = hashlib.sha256(
        json.dumps(canonical_core, sort_keys=True, separators=(",", ":")).encode("utf-8")
    ).hexdigest()

    state_counts: dict[str, int] = {}
    for node_id in nodes:
        state = nodes[node_id].get("state")
        state_counts[state if state in STATES else "Unknown"] = state_counts.get(state if state in STATES else "Unknown", 0) + 1

    return {
        "task": "D18-L30",
        "tool": "tools/d18_asset_dependency_graph.py",
        "schema_version": "1.0",
        "read_only": True,
        "approved_count": 0,
        "catalog_modified": False,
        "status": status,
        "node_count": len(nodes),
        "edge_count": len(edge_rows),
        "cycle_count": len(cycle_rows),
        "package_count": len(packages),
        "incompatible_pair_count": len(incompatible_pairs),
        "node_count_by_state": dict(sorted(state_counts.items())),
        "node_count_by_package": {package: sum(1 for node_id in nodes if nodes[node_id]["package"] == package) for package in packages},
        "nodes": node_rows,
        "edges": edge_rows,
        "cycles": cycle_rows,
        "issue_counts": dict(sorted(code_counts.items())),
        "issues": sorted_issues,
        "fingerprint": fingerprint,
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--records", type=Path, action="append", default=[], help="asset records JSON (repeatable)")
    parser.add_argument("--descriptors", type=Path, action="append", default=[], help="import-descriptor-style JSON (repeatable)")
    parser.add_argument("--packages", type=Path, default=None, help="global incompatible package pairs JSON")
    parser.add_argument("--output", type=Path, default=None, help="write result JSON to this path")
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)

    records: list[Any] = []
    descriptors: list[Any] = []
    input_issues: list[dict[str, Any]] = []

    for records_path in args.records:
        loaded, code, message = load_items(records_path, "records")
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(records_path)})
            continue
        records.extend(loaded)
    for descriptors_path in args.descriptors:
        loaded, code, message = load_items(descriptors_path, "descriptors")
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(descriptors_path)})
            continue
        descriptors.extend(loaded)

    incompatible_pairs: set[tuple[str, str]] | None = None
    if args.packages is not None:
        loaded, code, message = load_packages(args.packages)
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(args.packages)})
        else:
            incompatible_pairs = loaded

    result = build_graph(
        records,
        descriptors,
        incompatible_pairs=incompatible_pairs,
        input_issues=input_issues,
    )
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    exit_codes = {"PASS": 0, "PENDING": 1, "BLOCKED": 2}
    return exit_codes.get(result["status"], 3)


if __name__ == "__main__":
    raise SystemExit(main())
