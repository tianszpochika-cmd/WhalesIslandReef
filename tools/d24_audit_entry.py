#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D24-L02—L16 审计脚本的统一 CLI 入口。"""

from __future__ import annotations

import json
import argparse
from pathlib import Path
import sys
from typing import List

from d24_logic_audit_common import run_named_audit, self_test


def _parse_args(argv: List[str] | None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="D24 pure logic audit")
    parser.add_argument("--root", type=Path, default=None, help="repository root to audit")
    parser.add_argument("--json-out", type=Path, default=None, help="also write the JSON payload here")
    parser.add_argument("--self-test", action="store_true", help="run the audit scanner self-test")
    return parser.parse_args(argv)


def main(kind: str, argv: List[str] | None = None) -> int:
    args = _parse_args(argv)
    payload = self_test() if args.self_test else run_named_audit(kind, args.root)
    encoded = json.dumps(payload, ensure_ascii=False, indent=2, sort_keys=True)
    print(encoded)
    if args.json_out is not None:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(encoded + "\n", encoding="utf-8")
    return 0 if payload.get("passed") else 1


if __name__ == "__main__":
    sys.exit(main("unknown", sys.argv[1:]))
