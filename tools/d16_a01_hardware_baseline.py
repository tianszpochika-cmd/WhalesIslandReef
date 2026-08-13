#!/usr/bin/env python3
"""Capture a truthful local hardware baseline for D16-A01.

This is a host snapshot, not a game performance benchmark.  It records the
machine on which a runtime window was sampled so that FPS/CPU/memory reports
are not detached from their hardware context.  It never treats the local host
as a target or low-end device and never substitutes GPU utilization for GPU
frame time.
"""

from __future__ import annotations

import argparse
import csv
import ctypes
import datetime as dt
import json
import os
import platform
import shutil
import subprocess
from pathlib import Path
from typing import Callable, Sequence


GPU_QUERY = (
    "name,driver_version,utilization.gpu,memory.used,memory.total,temperature.gpu"
)


def _parse_number(value: str):
    cleaned = value.strip()
    if not cleaned or cleaned.upper() in {"N/A", "NA", "NOT SUPPORTED"}:
        return None
    try:
        return int(cleaned)
    except ValueError:
        try:
            return float(cleaned)
        except ValueError:
            return cleaned


def parse_nvidia_smi_csv(output: str) -> list[dict]:
    """Parse ``nvidia-smi --format=csv,noheader,nounits`` output."""
    devices: list[dict] = []
    for row in csv.reader(line for line in output.splitlines() if line.strip()):
        if len(row) < 6:
            continue
        devices.append({
            "name": row[0].strip(),
            "driver_version": row[1].strip(),
            "utilization_gpu_percent": _parse_number(row[2]),
            "memory_used_mb": _parse_number(row[3]),
            "memory_total_mb": _parse_number(row[4]),
            "temperature_c": _parse_number(row[5]),
        })
    return devices


def query_nvidia_smi(
    runner: Callable[..., subprocess.CompletedProcess] = subprocess.run,
    executable: str = "nvidia-smi",
) -> tuple[list[dict], str | None]:
    """Return one instantaneous NVIDIA telemetry sample, if available."""
    try:
        completed = runner(
            [
                executable,
                f"--query-gpu={GPU_QUERY}",
                "--format=csv,noheader,nounits",
            ],
            capture_output=True,
            text=True,
            check=False,
            timeout=5,
        )
    except (FileNotFoundError, OSError, subprocess.TimeoutExpired) as error:
        return [], error.__class__.__name__
    if completed.returncode != 0:
        return [], f"exit_code_{completed.returncode}"
    return parse_nvidia_smi_csv(completed.stdout), None


def _total_memory_bytes() -> int | None:
    if os.name == "nt":
        class MemoryStatus(ctypes.Structure):
            _fields_ = [
                ("dwLength", ctypes.c_ulong),
                ("dwMemoryLoad", ctypes.c_ulong),
                ("ullTotalPhys", ctypes.c_ulonglong),
                ("ullAvailPhys", ctypes.c_ulonglong),
                ("ullTotalPageFile", ctypes.c_ulonglong),
                ("ullAvailPageFile", ctypes.c_ulonglong),
                ("ullTotalVirtual", ctypes.c_ulonglong),
                ("ullAvailVirtual", ctypes.c_ulonglong),
                ("ullAvailExtendedVirtual", ctypes.c_ulonglong),
            ]

        status = MemoryStatus()
        status.dwLength = ctypes.sizeof(MemoryStatus)
        if ctypes.windll.kernel32.GlobalMemoryStatusEx(ctypes.byref(status)):
            return int(status.ullTotalPhys)
        return None

    meminfo = Path("/proc/meminfo")
    if meminfo.exists():
        for line in meminfo.read_text(encoding="utf-8", errors="replace").splitlines():
            if line.startswith("MemTotal:"):
                parts = line.split()
                if len(parts) >= 2:
                    return int(parts[1]) * 1024
    return None


def collect_baseline(
    runner: Callable[..., subprocess.CompletedProcess] = subprocess.run,
) -> dict:
    devices, gpu_error = query_nvidia_smi(runner=runner)
    return {
        "tool": "D16-A01 hardware baseline",
        "captured_at_utc": dt.datetime.now(dt.timezone.utc).isoformat(),
        "measurement_kind": "host_hardware_snapshot",
        "scope": "local_host_only",
        "is_target_device_evidence": False,
        "is_low_end_device_evidence": False,
        "does_not_measure": [
            "game_frame_gpu_time",
            "target_or_low_end_performance",
            "large_scene_load_time",
            "long_run_stability",
        ],
        "os": {
            "system": platform.system(),
            "release": platform.release(),
            "version": platform.version(),
            "machine": platform.machine(),
        },
        "cpu": {
            "logical_processors": os.cpu_count(),
            "processor": platform.processor(),
        },
        "memory": {
            "total_bytes": _total_memory_bytes(),
        },
        "gpu": {
            "source": "nvidia-smi",
            "sample_available": bool(devices),
            "query_error": gpu_error,
            "devices": devices,
        },
        "disk": {
            "workspace_free_bytes": shutil.disk_usage(Path.cwd()).free,
        },
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--json-out", type=Path, required=True)
    args = parser.parse_args()
    result = collect_baseline()
    args.json_out.parent.mkdir(parents=True, exist_ok=True)
    args.json_out.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    gpu = result["gpu"]
    print(
        "D16-A01 host baseline: "
        f"cpu={result['cpu']['logical_processors']} logical, "
        f"gpu_devices={len(gpu['devices'])}, "
        f"gpu_sample_available={gpu['sample_available']}"
    )
    print("Scope is local host only; this is not target/low-end or frame-time evidence.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
