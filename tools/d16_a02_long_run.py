"""D16-A02 repeated save/migration pressure check.

This is a bounded local stress witness, not the D16-S01 full-game endurance
run. It repeatedly exercises the real SaveSystem entry points on one temporary
SQLite file and verifies the final player fact, migration idempotency and file
integrity.
"""

from __future__ import annotations

import json
import sqlite3
import subprocess
import sys
import tempfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
RUNNER = ROOT / "tools" / "d16_a02_runner" / "bin" / "Debug" / "net9.0" / "D16A02Runner.dll"
REPORT = ROOT / "tools" / "results" / "d16_a02_long_run_result.json"


def inspect_final_save(path: Path) -> dict:
    result = {
        "exists": path.exists(),
        "sqlite_integrity": None,
        "user_version": None,
        "v3_tables_present": False,
        "legacy_player": None,
        "backup_count": len(list(Path(str(path) + ".backups").glob("save_*.db"))),
    }
    if not path.exists():
        return result
    try:
        db = sqlite3.connect(path)
        try:
            result["sqlite_integrity"] = db.execute("PRAGMA integrity_check").fetchone()[0]
            result["user_version"] = db.execute("PRAGMA user_version").fetchone()[0]
            tables = {
                row[0]
                for row in db.execute(
                    "SELECT name FROM sqlite_master WHERE type='table'"
                ).fetchall()
            }
            result["v3_tables_present"] = {"save_metadata", "v3_entities"}.issubset(tables)
            player = db.execute("SELECT name, money FROM players LIMIT 1").fetchone()
            if player:
                result["legacy_player"] = {"name": player[0], "money": player[1]}
        finally:
            db.close()
    except sqlite3.Error as exc:
        result["sqlite_integrity"] = "error: " + str(exc)
    return result


def main() -> int:
    iterations = int(sys.argv[1]) if len(sys.argv) > 1 else 100
    if iterations <= 0:
        raise SystemExit("iteration count must be positive")
    if not RUNNER.exists():
        raise SystemExit("D16A02Runner.dll 不存在，请先构建 tools/d16_a02_runner。")

    with tempfile.TemporaryDirectory(prefix="wb_d16_a02_long_run_") as temp:
        save_path = Path(temp) / "long_run.db"
        process = subprocess.run(
            ["dotnet", str(RUNNER), "--long-run", str(save_path), str(iterations)],
            cwd=ROOT,
            capture_output=True,
            text=True,
            timeout=300,
            check=False,
        )
        final = inspect_final_save(save_path)
        expected_player = {"name": "长线存档船长-" + str(iterations - 1), "money": 500 + iterations - 1}
        passed = (
            process.returncode == 0
            and f"iterations={iterations}" in process.stdout
            and final["sqlite_integrity"] == "ok"
            and final["user_version"] == 1
            and final["v3_tables_present"]
            and final["legacy_player"] == expected_player
            and final["backup_count"] >= iterations
        )
        report = {
            "runner": str(RUNNER),
            "iterations": iterations,
            "exit_code": process.returncode,
            "final": final,
            "expected_final_player": expected_player,
            "passed": passed,
            "stdout": process.stdout[-2000:],
            "stderr": process.stderr[-2000:],
        }

    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if report["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
