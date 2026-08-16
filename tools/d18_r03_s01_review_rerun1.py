"""Read-only RERUN1 gate for the D18 player-appearance workbench.

The original A01 packet and import report remain the source/license evidence;
the RERUN1 Unity audit is an additional proof of the isolated project-owned
Avatar, hair and outfit assembly. This tool never edits the global intake
catalog or promotes any runtime state.
"""

from __future__ import annotations

import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PACKET = ROOT / "docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/D18_R03_A01_PLAYER_APPEARANCE_PACKET.json"
IMPORT = ROOT / "game/tools/results/D18/D18-R03-A01/D18_R03_A01_ISOLATED_UNITY_IMPORT.json"
AUDIT = ROOT / "game/tools/results/D18/D18-R03-A01/D18_R03_A01_EDITOR_AUDIT_RERUN1.json"
REVIEW_DIR = ROOT / "docs/game/design/common/assets/review_packets/D18-R03-S01"
RESULT_DIR = ROOT / "game/tools/results/D18/D18-R03-S01"
REVIEW_JSON = REVIEW_DIR / "D18_R03_S01_CHARACTER_APPEARANCE_REVIEW_RERUN1.json"
REVIEW_MD = REVIEW_DIR / "D18_R03_S01_CHARACTER_APPEARANCE_REVIEW_RERUN1.md"
MACHINE = RESULT_DIR / "D18_R03_S01_MACHINE_RESULT_RERUN1.json"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def local_path(value: str | None) -> Path | None:
    if not value:
        return None
    normalized = value.replace("\\", "/")
    if normalized.startswith("Assets/"):
        return ROOT / "game" / normalized
    return ROOT / normalized


def verify() -> tuple[dict, dict, str]:
    packet = json.loads(PACKET.read_text(encoding="utf-8"))
    imported = json.loads(IMPORT.read_text(encoding="utf-8"))
    audit = json.loads(AUDIT.read_text(encoding="utf-8"))
    errors: list[str] = []
    license_path = local_path(packet["source"].get("license_evidence_path"))
    archive_path = local_path(packet["source"].get("archive_path"))
    if not license_path or not license_path.is_file():
        errors.append("license_evidence_missing")
    if not archive_path or not archive_path.is_file():
        errors.append("source_archive_missing")
    archive_actual = sha256(archive_path) if archive_path and archive_path.is_file() else None
    if archive_actual != packet["source"].get("archive_sha256"):
        errors.append("source_archive_sha256_mismatch")
    if imported.get("status") != "PASS" or imported.get("errors"):
        errors.append("isolated_unity_import_not_clean")

    imported_by_name = {item.get("file_name"): item for item in imported.get("candidates", [])}
    records = []
    for record in packet.get("candidate_records", []):
        path = local_path(record.get("candidate_file_path"))
        exists = bool(path and path.is_file())
        actual = sha256(path) if exists else None
        import_record = imported_by_name.get(record.get("file_name"), {})
        hash_ok = exists and actual == record.get("sha256")
        import_ok = bool(import_record) and import_record.get("file_exists") is True
        if not exists:
            errors.append("candidate_missing:" + str(record.get("file_name")))
        elif not hash_ok:
            errors.append("candidate_sha256_mismatch:" + str(record.get("file_name")))
        if not import_ok:
            errors.append("candidate_import_evidence_missing:" + str(record.get("file_name")))
        records.append({
            "content_id": record.get("content_id"),
            "file_name": record.get("file_name"),
            "candidate_file_path": record.get("candidate_file_path"),
            "sha256_recorded": record.get("sha256"),
            "sha256_actual": actual,
            "file_exists": exists,
            "hash_verified": hash_ok,
            "unity_import_record": import_record,
            "license_evidence_path": packet["source"].get("license_evidence_path"),
            "review_status": "Candidate",
            "recommended_for_controlled_work_prefab": hash_ok and import_ok and not errors,
            "local_development_ready": False,
            "release_approved": False,
        })

    generated = []
    generated_paths = [audit.get("hair_prefab"), audit.get("workbench_prefab"), audit.get("female_avatar"), audit.get("male_avatar")]
    generated_paths += [item.get("prefab") for item in audit.get("outfit_variants", [])]
    for value in generated_paths:
        path = local_path(value)
        exists = bool(path and path.is_file())
        if not exists:
            errors.append("generated_asset_missing:" + str(value))
        generated.append({"path": value, "file_exists": exists, "sha256": sha256(path) if exists else None})

    source_evidence_ok = not errors
    assembly_ok = (
        audit.get("status") == "PASS"
        and audit.get("body_directions_verified", 0) >= 2
        and audit.get("independent_hair_styles_verified", 0) >= 6
        and audit.get("outfit_combinations_verified", 0) >= 8
        and all(item.get("valid") for item in audit.get("outfit_variants", []))
        and not audit.get("errors")
    )
    conclusion = "PASS" if source_evidence_ok and assembly_ok else "PARTIAL"
    review = {
        "schema": "D18_R03_S01_CHARACTER_APPEARANCE_REVIEW_RERUN1",
        "task": "D18-R03-S01",
        "run": "RERUN1",
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "conclusion": conclusion,
        "scope": "read_only_source_evidence_and_isolated_workbench_consumption_gate",
        "inputs": {
            "historical_packet": str(PACKET.relative_to(ROOT)).replace("\\", "/"),
            "unity_import": str(IMPORT.relative_to(ROOT)).replace("\\", "/"),
            "unity_workbench_audit": str(AUDIT.relative_to(ROOT)).replace("\\", "/"),
            "license_evidence": packet["source"].get("license_evidence_path"),
            "archive": packet["source"].get("archive_path"),
            "archive_sha256_recorded": packet["source"].get("archive_sha256"),
            "archive_sha256_actual": archive_actual,
        },
        "counts": {
            "source_candidate_records": len(records),
            "source_files_present": sum(item["file_exists"] for item in records),
            "source_hashes_verified": sum(item["hash_verified"] for item in records),
            "unity_import_records": len(imported_by_name),
            "valid_humanoid_bases": audit.get("body_directions_verified", 0),
            "independent_hairstyles": audit.get("independent_hair_styles_verified", 0),
            "assembled_outfit_combinations": audit.get("outfit_combinations_verified", 0),
            "generated_assets_verified": sum(item["file_exists"] for item in generated),
            "runtime_consumers": 0,
            "local_playable_ready": 0,
            "local_development_ready": 0,
            "release_approved": 0,
        },
        "consumption_categories": {
            "player_visual_core": {"status": "WORKBENCH_EVIDENCE_READY", "humanoid_bases": 2, "hair_modules": 6, "outfit_combinations": 8},
            "npc_visual_core": {"status": "OUT_OF_SCOPE", "count": 0},
            "wildlife_core": {"status": "OUT_OF_SCOPE", "count": 0},
            "pet_core": {"status": "OUT_OF_SCOPE", "count": 0},
            "animation_core": {"status": "OPEN", "count": 0, "note": "D20-A02 owns the action layer; no independent animation clips are claimed here."},
            "female_or_neutral_human_gap": {"status": "FEMALE_VERIFIED_NEUTRAL_UNPROVEN"},
        },
        "source_records": records,
        "generated_workbench_assets": generated,
        "state_boundary": {
            "global_intake_catalog_modified": False,
            "candidate_resource_states_modified": False,
            "formal_scene_consumer_created": False,
            "runtime_consumer_created": False,
            "local_development_ready_written": False,
            "release_approved_written": False,
        },
        "errors": sorted(set(errors)),
        "gaps": [
            "1675 Caribbean clothing review remains PROVISIONAL_LOCAL_VISUAL.",
            "No formal Player consumer, animation binding, collision, save/load or D18-S03 runtime evidence is claimed.",
            "Candidate resources remain outside LocalDevelopmentReady and ReleaseApproved.",
        ],
    }
    machine = {
        "schema": "D18_R03_S01_MACHINE_RESULT_RERUN1",
        "task": "D18-R03-S01",
        "run": "RERUN1",
        "conclusion": conclusion,
        "review_json": str(REVIEW_JSON.relative_to(ROOT)).replace("\\", "/"),
        "review_markdown": str(REVIEW_MD.relative_to(ROOT)).replace("\\", "/"),
        "counts": review["counts"],
        "source_evidence_ok": source_evidence_ok,
        "assembly_evidence_ok": assembly_ok,
        "global_catalog_changed": False,
        "resource_state_changed": False,
        "local_development_ready": 0,
        "release_approved": 0,
        "consumption_gate": {
            "D20-A01-R02": "ALLOWED_FOR_ISOLATED_WORK_PREFAB_ONLY",
            "D20-A02-R02": "WAIT_FOR_D20-A01-R02_REAL_PLAYER_ACTION_CHECK",
            "D18-S03": "REQUIRED_FOR_RUNTIME_PROMOTION",
        },
        "error_count": len(review["errors"]),
    }
    markdown = "\n".join([
        "# D18-R03-S01 RERUN1 角色外观资源消费门禁",
        "",
        f"结论：**{conclusion}**。本轮只读核对历史 A01 源证据和新建隔离工作台，不修改全局准入目录或资源状态。",
        "",
        "## 真实输入",
        "",
        f"- 历史 A01 packet：`{review['inputs']['historical_packet']}`",
        f"- 隔离 Unity 导入：`{review['inputs']['unity_import']}`",
        f"- RERUN1 工作台审计：`{review['inputs']['unity_workbench_audit']}`",
        f"- 许可证证据：`{review['inputs']['license_evidence']}`；归档 SHA 记录/实际：`{review['inputs']['archive_sha256_recorded']}` / `{review['inputs']['archive_sha256_actual']}`",
        "",
        "## 计数",
        "",
        f"- 源候选记录：{review['counts']['source_candidate_records']}；文件存在：{review['counts']['source_files_present']}；哈希通过：{review['counts']['source_hashes_verified']}。",
        f"- Unity 导入记录：{review['counts']['unity_import_records']}；有效 Humanoid：{review['counts']['valid_humanoid_bases']}；独立发型：{review['counts']['independent_hairstyles']}；装配组合：{review['counts']['assembled_outfit_combinations']}。",
        f"- 新生成工作台资产存在：{review['counts']['generated_assets_verified']}；运行时消费者：0；LocalPlayableReady：0；LocalDevelopmentReady：0；ReleaseApproved：0。",
        "",
        "## 门禁结论",
        "",
        "- `player_visual_core`：2 个全身方向、6 个发型、8 个装配变体可用于指定 D20 隔离工作 Prefab；这不是正式运行时准入。",
        "- `npc_visual_core`、`wildlife_core`、`pet_core`：本包不覆盖，保持空，不用玩家资源冒充。",
        "- `animation_core`：保持 OPEN，由 D20 动作任务负责。",
        "- `female_or_neutral_human_gap`：女性已验证；中性方向无证据，不宣称覆盖。",
        "- 服装的 1675 年历史审查保持 `PROVISIONAL_LOCAL_VISUAL`。",
        "",
        "## 后续边界",
        "",
        "D20-A01-R02 可开始隔离工作 Prefab；D20-A02-R02 需等待外观 Prefab 的真实动作检查；D18-S03 仍是运行时消费者、碰撞、LOD、保存恢复和 LocalDevelopmentReady 的唯一晋级门禁。",
        "",
        f"错误数：{len(review['errors'])}。机器结果：`{MACHINE.relative_to(ROOT).as_posix()}`。",
    ]) + "\n"
    return review, machine, markdown


def main() -> int:
    REVIEW_DIR.mkdir(parents=True, exist_ok=True)
    RESULT_DIR.mkdir(parents=True, exist_ok=True)
    review, machine, markdown = verify()
    REVIEW_JSON.write_text(json.dumps(review, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    REVIEW_MD.write_text(markdown, encoding="utf-8")
    MACHINE.write_text(json.dumps(machine, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps(machine, indent=2, ensure_ascii=False))
    return 0 if machine["conclusion"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
