"""Smoke-test the formal portable Windows installer in an isolated artifacts tree."""

from __future__ import annotations

import argparse
import json
import subprocess
import tempfile
from pathlib import Path


def _invoke(installer: Path, operation: str, payload: Path, install: Path, user_data: Path) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        [
            str(installer),
            "--operation",
            operation,
            "--payload",
            str(payload),
            "--install-dir",
            str(install),
            "--user-data-dir",
            str(user_data),
        ],
        check=False,
        capture_output=True,
        text=True,
    )


def _run(installer: Path, operation: str, payload: Path, install: Path, user_data: Path) -> dict:
    result = _invoke(installer, operation, payload, install, user_data)
    if result.returncode != 0:
        raise RuntimeError(f"{operation} failed: {result.stderr.strip()}")
    try:
        return json.loads(result.stdout)
    except json.JSONDecodeError as error:
        raise RuntimeError(f"{operation} returned invalid JSON: {result.stdout!r}") from error


def smoke(installer: Path, payload: Path, artifacts_root: Path) -> dict:
    if not installer.is_file():
        raise FileNotFoundError(installer)
    if not (payload / "WhalesIslandReef.exe").is_file():
        raise FileNotFoundError(payload / "WhalesIslandReef.exe")

    smoke_root = Path(tempfile.mkdtemp(prefix="d16_a06_formal_installer_smoke_", dir=artifacts_root))
    install = smoke_root / "installed"
    user_data = smoke_root / "user_data"
    user_data.mkdir()
    sentinel = user_data / "save.db"
    sentinel.write_text("formal-installer-smoke-save", encoding="utf-8")

    install_result = _run(installer, "install", payload, install, user_data)
    install_ok = install_result.get("status") == "PASS" and (install / "WhalesIslandReef.exe").is_file()

    sentinel.write_text("formal-installer-smoke-save-after-install", encoding="utf-8")
    invalid_payload = smoke_root / "invalid_payload"
    invalid_payload.mkdir()
    (invalid_payload / "not-a-unity-build.txt").write_text("intentional negative fixture", encoding="utf-8")
    failed_update_result = _invoke(installer, "update", invalid_payload, install, user_data)
    failed_update_rejected = failed_update_result.returncode != 0
    failed_update_preserved_install = (install / "WhalesIslandReef.exe").is_file()
    failed_update_preserved_user_save = sentinel.read_text(encoding="utf-8") == "formal-installer-smoke-save-after-install"

    update_result = _run(installer, "update", payload, install, user_data)
    update_ok = update_result.get("status") == "PASS" and (install / "WhalesIslandReef.exe").is_file()
    update_preserved = sentinel.read_text(encoding="utf-8") == "formal-installer-smoke-save-after-install"

    uninstall_result = _run(installer, "uninstall", payload, install, user_data)
    uninstall_ok = uninstall_result.get("status") == "PASS" and not install.exists()
    user_save_preserved = sentinel.is_file() and sentinel.read_text(encoding="utf-8") == "formal-installer-smoke-save-after-install"

    passed = (
        install_ok
        and failed_update_rejected
        and failed_update_preserved_install
        and failed_update_preserved_user_save
        and update_ok
        and uninstall_ok
        and update_preserved
        and user_save_preserved
    )
    return {
        "task": "D16-A06",
        "status": "PASS" if passed else "FAIL",
        "installer": str(installer),
        "payload": str(payload),
        "smoke_root": str(smoke_root),
        "install_ok": install_ok,
        "failed_update_rejected": failed_update_rejected,
        "failed_update_preserved_install": failed_update_preserved_install,
        "failed_update_preserved_user_save": failed_update_preserved_user_save,
        "update_ok": update_ok,
        "uninstall_ok": uninstall_ok,
        "update_preserved_user_save": update_preserved,
        "user_save_preserved": user_save_preserved,
        "external_publish_performed": False,
        "steam_upload_performed": False,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--installer", type=Path, required=True)
    parser.add_argument("--payload", type=Path, required=True)
    parser.add_argument("--artifacts-root", type=Path, required=True)
    parser.add_argument("--json-out", type=Path, required=True)
    args = parser.parse_args()
    result = smoke(args.installer.resolve(), args.payload.resolve(), args.artifacts_root.resolve())
    args.json_out.parent.mkdir(parents=True, exist_ok=True)
    args.json_out.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
