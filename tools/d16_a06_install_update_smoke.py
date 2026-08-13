"""Workspace-only install/update/uninstall smoke test for a Unity player.

This is deliberately not an OS installer test. It copies a built player into a
scoped staging directory, replaces the player with an update payload while
preserving a separate save sentinel, then removes only the installed payload.
"""

import argparse
import json
import shutil
from pathlib import Path


def safe_staging_root(path: Path) -> Path:
    resolved = path.resolve()
    if resolved.name != "d16_a06_install_update_smoke" or resolved.parent.name != "artifacts":
        raise ValueError("staging root must be artifacts/d16_a06_install_update_smoke")
    return resolved


def copy_player(source: Path, destination: Path) -> int:
    shutil.copytree(source, destination)
    return sum(1 for item in destination.rglob("*") if item.is_file())


def run(build_dir: Path, staging_dir: Path):
    build_dir = build_dir.resolve()
    staging_dir = safe_staging_root(staging_dir)
    player = build_dir / "WhalesIslandReef.exe"
    if not player.is_file():
        raise FileNotFoundError(player)
    if staging_dir.exists():
        shutil.rmtree(staging_dir)
    staging_dir.mkdir(parents=True)

    install_dir = staging_dir / "installed"
    update_dir = staging_dir / "update_payload"
    user_data = staging_dir / "user_data"
    user_data.mkdir()
    save_file = user_data / "save.db"
    save_sentinel = b"d16-a06-save-sentinel-v1"
    save_file.write_bytes(save_sentinel)

    install_files = copy_player(build_dir, install_dir)
    install_ok = (install_dir / "WhalesIslandReef.exe").is_file()

    update_files = copy_player(build_dir, update_dir)
    shutil.rmtree(install_dir)
    update_dir.rename(install_dir)
    update_ok = (install_dir / "WhalesIslandReef.exe").is_file() and save_file.read_bytes() == save_sentinel

    shutil.rmtree(install_dir)
    uninstall_ok = not install_dir.exists() and save_file.is_file() and save_file.read_bytes() == save_sentinel

    return {
        "status": "PASS" if install_ok and update_ok and uninstall_ok else "FAIL",
        "build_dir": str(build_dir),
        "staging_dir": str(staging_dir),
        "install_files": install_files,
        "update_files": update_files,
        "install_ok": install_ok,
        "update_preserved_user_save": update_ok,
        "uninstall_preserved_user_save": uninstall_ok,
        "destructive_install_test_performed": True,
        "note": "仅操作 workspace artifacts/d16_a06_install_update_smoke，不是 OS 安装器或 Steam 发布测试。",
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("build_dir", type=Path)
    parser.add_argument("staging_dir", type=Path)
    parser.add_argument("--json-out", type=Path, required=True)
    args = parser.parse_args()

    result = run(args.build_dir, args.staging_dir)
    output = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    args.json_out.parent.mkdir(parents=True, exist_ok=True)
    args.json_out.write_text(output, encoding="utf-8")
    print(output, end="")
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
