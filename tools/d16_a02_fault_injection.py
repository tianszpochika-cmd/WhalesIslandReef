"""D16-A02 controlled process-abort checks for local save safety.

The child process exits at an explicit write boundary. The parent process then
opens the files again and verifies that the old bytes or the legacy SQLite
transaction remain recoverable. This is not a power-loss claim; it is a
repeatable process-interruption witness for the same failure boundaries.
"""

from __future__ import annotations

import json
import sqlite3
import subprocess
import tempfile
import time
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
RUNNER = ROOT / "tools" / "d16_a02_runner" / "bin" / "Debug" / "net9.0" / "D16A02Runner.dll"
REPORT = ROOT / "tools" / "results" / "d16_a02_fault_injection_result.json"


def run_child(mode: str, path: Path) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        ["dotnet", str(RUNNER), mode, str(path)],
        cwd=ROOT,
        capture_output=True,
        text=True,
        timeout=30,
        check=False,
    )


def run_force_kill(mode: str, path: Path, ready_marker: Path) -> dict:
    """Run until the child reaches a boundary, then terminate the child process.

    The marker is deliberately left in place while the child is alive. The
    child only proceeds after the marker is removed, so seeing the marker and
    then killing the process proves that the kill happened while the hook was
    holding the process at the requested boundary.
    """
    ready_marker.unlink(missing_ok=True)
    process = subprocess.Popen(
        ["dotnet", str(RUNNER), mode, str(path), str(ready_marker)],
        cwd=ROOT,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
    )
    ready_seen = False
    force_kill_sent = False
    deadline = time.monotonic() + 30
    while time.monotonic() < deadline:
        if ready_marker.exists():
            ready_seen = True
            break
        if process.poll() is not None:
            break
        time.sleep(0.01)

    if process.poll() is None:
        try:
            process.kill()
            force_kill_sent = True
        except ProcessLookupError:
            force_kill_sent = False

    try:
        stdout, stderr = process.communicate(timeout=10)
    except subprocess.TimeoutExpired:
        process.kill()
        force_kill_sent = True
        stdout, stderr = process.communicate(timeout=10)

    ready_marker.unlink(missing_ok=True)
    return {
        "exit_code": process.returncode,
        "ready_seen": ready_seen,
        "force_kill_sent": force_kill_sent,
        "terminated_after_ready": ready_seen and force_kill_sent and process.returncode != 0,
        "passed": ready_seen and force_kill_sent and process.returncode != 0,
        "stdout": stdout[-1000:],
        "stderr": stderr[-1000:],
    }


def read_migration_state(path: Path) -> tuple[str, int | None, set[str], tuple | None]:
    if not path.exists():
        return "missing", None, set(), None
    try:
        db = sqlite3.connect(path)
        try:
            integrity = db.execute("PRAGMA integrity_check").fetchone()[0]
            user_version = db.execute("PRAGMA user_version").fetchone()[0]
            table_names = {
                row[0]
                for row in db.execute(
                    "SELECT name FROM sqlite_master WHERE type='table'"
                ).fetchall()
            }
            player = db.execute("SELECT name, money FROM players LIMIT 1").fetchone()
            return integrity, user_version, table_names, player
        finally:
            db.close()
    except sqlite3.Error as exc:
        return "error: " + str(exc), None, set(), None


def read_save_entity_state(path: Path) -> tuple[str, tuple | None, int | None, int | None]:
    if not path.exists():
        return "missing", None, None, None
    try:
        db = sqlite3.connect(path)
        try:
            integrity = db.execute("PRAGMA integrity_check").fetchone()[0]
            player = db.execute("SELECT name, money FROM players LIMIT 1").fetchone()
            ship_count = db.execute("SELECT COUNT(*) FROM ships").fetchone()[0]
            crew_count = db.execute("SELECT COUNT(*) FROM crew_agents").fetchone()[0]
            return integrity, player, ship_count, crew_count
        finally:
            db.close()
    except sqlite3.Error as exc:
        return "error: " + str(exc), None, None, None


def main() -> int:
    if not RUNNER.exists():
        raise SystemExit("D16A02Runner.dll 不存在，请先构建 tools/d16_a02_runner。")

    with tempfile.TemporaryDirectory(prefix="wb_d16_a02_fault_") as temp:
        root = Path(temp)
        atomic_path = root / "atomic.db"
        atomic_path.write_bytes(b"stable-v1")

        before_replace = run_child("--abort-after-flush", atomic_path)
        temporary_files_after_abort = list(root.glob("atomic.db.tmp_*"))
        old_file_after_abort = atomic_path.read_bytes() == b"stable-v1"
        before_replace_ok = (
            before_replace.returncode == 73
            and old_file_after_abort
            and len(temporary_files_after_abort) == 1
        )
        for temporary_file in temporary_files_after_abort:
            temporary_file.unlink(missing_ok=True)

        after_replace = run_child("--abort-after-replace", atomic_path)
        temporary_files_after_replace = list(root.glob("atomic.db.tmp_*"))
        after_replace_ok = (
            after_replace.returncode == 74
            and atomic_path.read_bytes() == b"new-replace"
            and not temporary_files_after_replace
        )

        migration_path = root / "migration.db"
        migration = run_child("--abort-during-migration", migration_path)
        integrity, user_version, table_names, player = read_migration_state(migration_path)
        migration_backups = list(Path(str(migration_path) + ".backups").glob("save_*.db"))
        migration_ok = (
            migration.returncode == 75
            and integrity == "ok"
            and user_version == 1
            and "save_metadata" not in table_names
            and "v3_entities" not in table_names
            and player == ("进程中止验收船长", 500)
            and bool(migration_backups)
        )

        force_flush_path = root / "atomic_force.db"
        force_flush_path.write_bytes(b"stable-force-v1")
        force_flush = run_force_kill(
            "--force-kill-after-flush", force_flush_path, root / "flush.ready"
        )
        force_flush_temporary_files = list(root.glob("atomic_force.db.tmp_*"))
        force_flush_old_file = force_flush_path.read_bytes() == b"stable-force-v1"
        force_flush_ok = (
            force_flush["passed"]
            and force_flush_old_file
            and len(force_flush_temporary_files) == 1
        )
        for temporary_file in force_flush_temporary_files:
            temporary_file.unlink(missing_ok=True)

        force_replace = run_force_kill(
            "--force-kill-after-replace", force_flush_path, root / "replace.ready"
        )
        force_replace_temporary_files = list(root.glob("atomic_force.db.tmp_*"))
        force_replace_new_file = force_flush_path.read_bytes() == b"new-force-replace"
        force_replace_ok = (
            force_replace["passed"]
            and force_replace_new_file
            and not force_replace_temporary_files
        )

        force_migration_path = root / "migration_force.db"
        force_migration = run_force_kill(
            "--force-kill-during-migration",
            force_migration_path,
            root / "migration.ready",
        )
        force_integrity, force_user_version, force_table_names, force_player = read_migration_state(
            force_migration_path
        )
        force_migration_backups = list(
            Path(str(force_migration_path) + ".backups").glob("save_*.db")
        )
        force_migration_ok = (
            force_migration["passed"]
            and force_integrity == "ok"
            and force_user_version == 1
            and "save_metadata" not in force_table_names
            and "v3_entities" not in force_table_names
            and force_player == ("强制终止验收船长", 500)
            and bool(force_migration_backups)
        )

        force_save_path = root / "save_force.db"
        force_save = run_force_kill(
            "--force-kill-during-save-after-player",
            force_save_path,
            root / "save.ready",
        )
        save_integrity, save_player, save_ship_count, save_crew_count = read_save_entity_state(
            force_save_path
        )
        force_save_ok = (
            force_save["passed"]
            and save_integrity == "ok"
            and save_player == ("保存中断验收船长", 500)
            and save_ship_count == 1
            and save_crew_count == 1
        )

        report = {
            "runner": str(RUNNER),
            "atomic_abort_before_replace": {
                "exit_code": before_replace.returncode,
                "old_file_preserved": old_file_after_abort,
                "temporary_file_seen_before_cleanup": len(temporary_files_after_abort) == 1,
                "passed": before_replace_ok,
                "stdout": before_replace.stdout[-1000:],
                "stderr": before_replace.stderr[-1000:],
            },
            "atomic_abort_after_replace": {
                "exit_code": after_replace.returncode,
                "new_file_complete": atomic_path.read_bytes() == b"new-replace",
                "temporary_file_left": bool(temporary_files_after_replace),
                "passed": after_replace_ok,
                "stdout": after_replace.stdout[-1000:],
                "stderr": after_replace.stderr[-1000:],
            },
            "migration_abort_before_commit": {
                "exit_code": migration.returncode,
                "sqlite_integrity": integrity,
                "user_version": user_version,
                "v3_tables_present": bool({"save_metadata", "v3_entities"} & table_names),
                "legacy_player": {"name": player[0], "money": player[1]} if player else None,
                "backup_count": len(migration_backups),
                "passed": migration_ok,
                "stdout": migration.stdout[-1000:],
                "stderr": migration.stderr[-1000:],
            },
            "force_kill_after_flush": {
                **force_flush,
                "old_file_preserved": force_flush_old_file,
                "temporary_file_seen_before_cleanup": len(force_flush_temporary_files) == 1,
                "passed": force_flush_ok,
            },
            "force_kill_after_replace": {
                **force_replace,
                "new_file_complete": force_replace_new_file,
                "temporary_file_left": bool(force_replace_temporary_files),
                "passed": force_replace_ok,
            },
            "force_kill_during_migration": {
                **force_migration,
                "sqlite_integrity": force_integrity,
                "user_version": force_user_version,
                "v3_tables_present": bool({"save_metadata", "v3_entities"} & force_table_names),
                "legacy_player": {
                    "name": force_player[0],
                    "money": force_player[1],
                } if force_player else None,
                "backup_count": len(force_migration_backups),
                "passed": force_migration_ok,
            },
            "force_kill_during_save_after_player": {
                **force_save,
                "sqlite_integrity": save_integrity,
                "player": {
                    "name": save_player[0],
                    "money": save_player[1],
                } if save_player else None,
                "ship_count": save_ship_count,
                "crew_count": save_crew_count,
                "passed": force_save_ok,
            },
            "controlled_process_abort_passed": before_replace_ok and after_replace_ok and migration_ok,
            "real_force_kill_passed": force_flush_ok and force_replace_ok and force_migration_ok and force_save_ok,
            "passed": (
                before_replace_ok
                and after_replace_ok
                and migration_ok
                and force_flush_ok
                and force_replace_ok
                and force_migration_ok
                and force_save_ok
            ),
        }

    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if report["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
