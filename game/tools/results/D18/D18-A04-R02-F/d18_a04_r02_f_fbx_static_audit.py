#!/usr/bin/env python3
"""Read-only FBX binary signature audit for D18-A04-R02-F.

This is deliberately not a Unity importer and never writes project assets. It
only reports the FBX header and node signatures that can be checked without a
Unity Editor installation.
"""

from __future__ import annotations

import argparse
import json
import struct
from collections import Counter
from pathlib import Path
from typing import Any


HEADER = b"Kaydara FBX Binary"
SENTINEL_SIZE = 13


class FbxParseError(RuntimeError):
    pass


class Reader:
    def __init__(self, data: bytes, wide: bool) -> None:
        self.data = data
        self.wide = wide
        self.pos = 0

    def take(self, count: int) -> bytes:
        end = self.pos + count
        if end > len(self.data):
            raise FbxParseError(f"read past end at {self.pos} (+{count})")
        chunk = self.data[self.pos:end]
        self.pos = end
        return chunk

    def u32(self) -> int:
        return struct.unpack("<I", self.take(4))[0]

    def u64(self) -> int:
        return struct.unpack("<Q", self.take(8))[0]

    def i16(self) -> int:
        return struct.unpack("<h", self.take(2))[0]

    def i32(self) -> int:
        return struct.unpack("<i", self.take(4))[0]

    def i64(self) -> int:
        return struct.unpack("<q", self.take(8))[0]

    def f32(self) -> float:
        return struct.unpack("<f", self.take(4))[0]

    def f64(self) -> float:
        return struct.unpack("<d", self.take(8))[0]


def read_property(reader: Reader) -> Any:
    kind = reader.take(1).decode("ascii", errors="replace")
    if kind == "Y":
        return reader.i16()
    if kind == "C":
        return bool(reader.take(1)[0])
    if kind == "I":
        return reader.i32()
    if kind == "F":
        return reader.f32()
    if kind == "D":
        return reader.f64()
    if kind == "L":
        return reader.i64()
    if kind in {"f", "d", "l", "i", "b"}:
        count = reader.u32()
        encoding = reader.u32()
        length = reader.u32()
        payload = reader.take(length)
        # The audit needs array presence, not every vertex. Keep this bounded.
        return {"array_type": kind, "count": count, "encoding": encoding, "bytes": len(payload)}
    if kind == "S":
        length = reader.u32()
        return reader.take(length).decode("utf-8", errors="replace")
    if kind == "R":
        length = reader.u32()
        return {"raw_bytes": len(reader.take(length))}
    raise FbxParseError(f"unsupported property type {kind!r} at {reader.pos - 1}")


def parse_node(reader: Reader, nodes: list[dict[str, Any]], version: int) -> None:
    offset_size = 8 if version >= 7500 else 4
    start = reader.pos
    end_offset = reader.u64() if offset_size == 8 else reader.u32()
    property_count = reader.u64() if offset_size == 8 else reader.u32()
    property_length = reader.u64() if offset_size == 8 else reader.u32()
    name_length = reader.take(1)[0]
    if end_offset == 0 and property_count == 0 and property_length == 0 and name_length == 0:
        return
    name = reader.take(name_length).decode("utf-8", errors="replace")
    properties = [read_property(reader) for _ in range(property_count)]
    property_end = start + offset_size * 3 + 1 + name_length + property_length
    if reader.pos != property_end:
        raise FbxParseError(f"property length mismatch for {name}: {reader.pos} != {property_end}")
    nodes.append({"name": name, "properties": properties})
    if reader.pos > end_offset or end_offset > len(reader.data):
        raise FbxParseError(f"invalid node boundary for {name}: {reader.pos}..{end_offset}")
    # A node with no children ends exactly at its property payload. A node
    # containing children ends with a 13-byte null sentinel; sibling nodes do
    # not have a sentinel between them.
    if reader.pos < end_offset:
        child_end = end_offset - SENTINEL_SIZE
        if child_end < reader.pos:
            raise FbxParseError(f"invalid child boundary for {name}: {reader.pos}..{end_offset}")
        while reader.pos < child_end:
            parse_node(reader, nodes, version)
        if reader.pos < end_offset:
            reader.take(end_offset - reader.pos)


def audit(path: Path) -> dict[str, Any]:
    data = path.read_bytes()
    if not data.startswith(HEADER):
        raise FbxParseError("missing FBX binary header")
    version = struct.unpack("<I", data[23:27])[0]
    reader = Reader(data, version >= 7500)
    reader.pos = 27
    nodes: list[dict[str, Any]] = []
    while reader.pos < len(data):
        before = reader.pos
        try:
            parse_node(reader, nodes, version)
        except FbxParseError:
            # Some exporters append non-node padding. Preserve the real header
            # and node signatures already read instead of inventing import data.
            reader.pos = before
            break
        if reader.pos <= before:
            break
    counts = Counter(node["name"] for node in nodes)
    model_types = Counter()
    animation_names: list[str] = []
    material_names: list[str] = []
    limb_names: list[str] = []
    mesh_names: list[str] = []

    def clean_label(value: str) -> str:
        return value.split("\x00", 1)[0].split("|", 1)[-1]

    for node in nodes:
        strings = [value for value in node["properties"] if isinstance(value, str)]
        if node["name"] == "AnimationStack":
            if strings:
                animation_names.append(clean_label(strings[0]))
        elif node["name"] == "Material":
            if strings:
                material_names.append(clean_label(strings[0]))
        elif node["name"] == "Model":
            model_type = next((value for value in strings if value in {"Mesh", "LimbNode", "Null", "Root"}), "Unknown")
            model_types[model_type] += 1
            if strings:
                label = clean_label(strings[0])
                if model_type == "LimbNode":
                    limb_names.append(label)
                elif model_type == "Mesh":
                    mesh_names.append(label)
    return {
        "file": path.as_posix(),
        "size_bytes": len(data),
        "fbx_header": data[:21].decode("ascii", errors="replace"),
        "fbx_version": version,
        "parsed_node_count": len(nodes),
        "node_counts": dict(sorted(counts.items())),
        "model_types": dict(sorted(model_types.items())),
        "animation_stack_names": animation_names,
        "material_names": material_names,
        "mesh_model_names": mesh_names,
        "limb_node_names": limb_names,
        "static_signatures": {
            "has_geometry": counts["Geometry"] > 0,
            "has_material": counts["Material"] > 0,
            "has_texture": counts["Texture"] > 0,
            "has_skeleton_deformer": counts["Deformer"] > 0,
            "has_animation_stack": counts["AnimationStack"] > 0,
            "has_animation_layer": counts["AnimationLayer"] > 0,
            "has_animation_curve": counts["AnimationCurve"] > 0,
            "has_connections": counts["Connections"] > 0,
        },
        "unity_import_verified": False,
        "limitations": [
            "Static FBX signature only; no Unity Editor was available in this workspace.",
            "Scale, axis, foot placement, Avatar, material assignment and bounds remain Unity-import checks.",
        ],
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    records = []
    errors = []
    for path in sorted(args.root.glob("*.fbx")):
        try:
            records.append(audit(path))
        except Exception as exc:  # noqa: BLE001 - report a per-file audit failure
            errors.append({"file": path.as_posix(), "error": str(exc)})
    result = {
        "task": "D18-A04-R02-F",
        "audit_kind": "fbx_binary_static_signature",
        "read_only": True,
        "source_directory": args.root.as_posix(),
        "file_count": len(records),
        "records": records,
        "errors": errors,
        "unity_editor_available": False,
        "unity_import_status": "NotImported",
    }
    args.output.write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    return 0 if not errors else 1


if __name__ == "__main__":
    raise SystemExit(main())
