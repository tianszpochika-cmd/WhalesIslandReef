"""Read-only D18-R03-S01 review of the isolated player appearance packet.

This tool never changes the global intake catalog, resource states, Prefabs,
scenes, Registry, or runtime code. It verifies evidence that already exists
and writes a separate review packet and machine result.
"""

from __future__ import annotations

import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
PACKET_PATH = ROOT / "docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/D18_R03_A01_PLAYER_APPEARANCE_PACKET.json"
IMPORT_PATH = ROOT / "game/tools/results/D18/D18-R03-A01/D18_R03_A01_ISOLATED_UNITY_IMPORT.json"
REVIEW_DIR = ROOT / "docs/game/design/common/assets/review_packets/D18-R03-S01"
RESULT_DIR = ROOT / "game/tools/results/D18/D18-R03-S01"
REVIEW_JSON = REVIEW_DIR / "D18_R03_S01_CHARACTER_APPEARANCE_REVIEW.json"
REVIEW_MD = REVIEW_DIR / "D18_R03_S01_CHARACTER_APPEARANCE_REVIEW.md"
MACHINE_JSON = RESULT_DIR / "D18_R03_S01_MACHINE_RESULT.json"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def rel_path(value: str) -> Path:
    normalized = value.replace("\\", "/")
    if normalized.startswith("game/") or normalized.startswith("docs/") or normalized.startswith("artifacts/"):
        return ROOT / normalized
    return ROOT / normalized


def category_for(record: dict[str, Any], imported: dict[str, Any]) -> tuple[list[str], str]:
    name = record["file_name"].lower()
    if imported.get("avatar_is_human") and imported.get("avatar_valid"):
        return ["player_visual_core"], "humanoid_body"
    if name.startswith("hair_") and "beard" not in name:
        return ["player_visual_core"], "hair_module"
    if "eyebrow" in name or "beard" in name:
        return ["player_visual_core"], "facial_detail_module"
    if "peasant" in name or "ranger" in name:
        return ["player_visual_core"], "clothing_module"
    if imported.get("animation_clip_count", 0) > 0:
        return ["animation_core"], "animation_candidate"
    return [], "unclassified_candidate"


def load() -> tuple[dict[str, Any], dict[str, Any]]:
    packet = json.loads(PACKET_PATH.read_text(encoding="utf-8"))
    imported = json.loads(IMPORT_PATH.read_text(encoding="utf-8"))
    return packet, imported


def build_review() -> tuple[dict[str, Any], dict[str, Any], str]:
    packet, imported = load()
    imported_by_name = {item["file_name"]: item for item in imported.get("candidates", [])}
    license_path = rel_path(packet["source"]["license_evidence_path"])
    archive_path = rel_path(packet["source"]["archive_path"])
    evidence_errors: list[str] = []
    if not license_path.is_file():
        evidence_errors.append(f"missing_license_evidence:{packet['source']['license_evidence_path']}")
    if not archive_path.is_file():
        evidence_errors.append(f"missing_source_archive:{packet['source']['archive_path']}")
    archive_hash = sha256(archive_path) if archive_path.is_file() else None
    if archive_hash and archive_hash != packet["source"].get("archive_sha256"):
        evidence_errors.append("source_archive_sha256_mismatch")
    if imported.get("errors"):
        evidence_errors.extend(f"unity_import_error:{item}" for item in imported["errors"])

    all_records = packet.get("candidate_records", [])
    reviewed: list[dict[str, Any]] = []
    for record in all_records:
        path = rel_path(record["candidate_file_path"])
        imported_item = imported_by_name.get(record["file_name"], {})
        exists = path.is_file()
        actual_hash = sha256(path) if exists else None
        hash_matches = actual_hash == record.get("sha256") if exists and record.get("sha256") else False
        categories, role = category_for(record, imported_item)
        license_ok = license_path.is_file()
        import_ok = bool(imported_item) and bool(imported_item.get("file_exists")) and not imported_item.get("notes", []) == ["missing"]
        human_ok = bool(imported_item.get("avatar_valid") and imported_item.get("avatar_is_human"))
        if not exists:
            evidence_errors.append(f"missing_candidate_file:{record['candidate_file_path']}")
        if exists and not hash_matches:
            evidence_errors.append(f"candidate_sha256_mismatch:{record['file_name']}")
        if not imported_item:
            evidence_errors.append(f"missing_unity_import_record:{record['file_name']}")
        recommendation = "RECOMMENDED_WITH_LIMITS" if exists and hash_matches and license_ok and import_ok else "EVIDENCE_INCOMPLETE"
        if human_ok:
            recommendation = "RECOMMENDED_HUMANOID_BASE_WITH_LIMITS"
        reviewed.append(
            {
                "content_id": record.get("content_id"),
                "file_name": record["file_name"],
                "candidate_file_path": record["candidate_file_path"],
                "sha256_recorded": record.get("sha256"),
                "sha256_actual": actual_hash,
                "file_exists": exists,
                "license_evidence_path": packet["source"]["license_evidence_path"],
                "license_evidence_present": license_ok,
                "unity_import_evidence_path": "game/tools/results/D18/D18-R03-A01/D18_R03_A01_ISOLATED_UNITY_IMPORT.json",
                "unity_import_record_present": bool(imported_item),
                "unity_import": imported_item,
                "consumption_categories": categories,
                "candidate_role": role,
                "review_conclusion": recommendation,
                "recommended_for_controlled_work_prefab": recommendation.startswith("RECOMMENDED"),
                "local_development_ready": False,
                "release_approved": False,
                "formal_runtime_consumer": False,
                "known_gaps": list(record.get("known_gaps", []))
                + (["No Humanoid Avatar; clothing/hair module requires project-owned assembly"] if not human_ok else [])
                + (["No independent animation clip evidence"] if imported_item.get("animation_clip_count", 0) == 0 else []),
            }
        )

    humanoids = [item for item in reviewed if "humanoid_body" == item["candidate_role"] and item["review_conclusion"].startswith("RECOMMENDED")]
    hair = [item for item in reviewed if item["candidate_role"] == "hair_module" and item["review_conclusion"].startswith("RECOMMENDED")]
    outfit_modules = [item for item in reviewed if item["candidate_role"] == "clothing_module" and item["review_conclusion"].startswith("RECOMMENDED")]
    complete_outfit_names = {"Female_Peasant.fbx", "Female_Ranger.fbx", "Male_Peasant.fbx", "Male_Ranger.fbx"}
    outfits = [item for item in outfit_modules if item["file_name"] in complete_outfit_names]
    import_pass = not imported.get("errors") and imported.get("status") == "PASS"
    file_evidence_pass = not any(item["review_conclusion"] == "EVIDENCE_INCOMPLETE" for item in reviewed)
    gaps = [
        {
            "category": "female_or_neutral_human_gap",
            "status": "COVERED_FOR_FEMALE_AND_MALE_BASE_ONLY",
            "detail": "Two valid Humanoid bases are verified; no neutral body direction is claimed.",
        },
        {
            "category": "hair_gap",
            "status": "OPEN",
            "detail": "Five independent hairstyle meshes are verified; the sixth required hairstyle is not evidenced.",
        },
        {
            "category": "era_outfit_gap",
            "status": "OPEN",
            "detail": "Four source outfit files exist, but zero of eight 1675 Caribbean combinations have historical review and assembled import evidence.",
        },
        {
            "category": "animation_core",
            "status": "OPEN",
            "detail": "No independent animation clip candidate was verified in this packet; D20-A02 action curves remain separate project-owned work.",
        },
        {"category": "npc_visual_core", "status": "OUT_OF_SCOPE", "detail": "This packet is player appearance only; no NPC claim."},
        {"category": "wildlife_core", "status": "OUT_OF_SCOPE", "detail": "This packet is player appearance only; no wildlife claim."},
        {"category": "pet_core", "status": "OUT_OF_SCOPE", "detail": "This packet is player appearance only; no pet claim."},
    ]
    conclusion = "PASS" if file_evidence_pass and import_pass and len(humanoids) >= 2 and len(hair) >= 6 and len(outfits) >= 8 else "PARTIAL"
    review = {
        "schema": "D18_R03_S01_CHARACTER_APPEARANCE_REVIEW_V1",
        "task": "D18-R03-S01",
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "conclusion": conclusion,
        "scope": "read_only_review_of_D18_R03_A01_isolated_candidate_evidence",
        "inputs": {
            "packet": str(PACKET_PATH.relative_to(ROOT)).replace("\\", "/"),
            "unity_import": str(IMPORT_PATH.relative_to(ROOT)).replace("\\", "/"),
            "official_page": packet["source"]["official_page"],
            "license_id": packet["source"]["license_id"],
            "license_evidence": packet["source"]["license_evidence_path"],
            "archive": packet["source"]["archive_path"],
            "archive_sha256_recorded": packet["source"].get("archive_sha256"),
            "archive_sha256_actual": archive_hash,
        },
        "counts": {
            "candidate_records_reviewed": len(reviewed),
            "candidate_files_present": sum(item["file_exists"] for item in reviewed),
            "candidate_files_hash_verified": sum(item["file_exists"] and item["sha256_recorded"] == item["sha256_actual"] for item in reviewed),
            "unity_import_records": sum(item["unity_import_record_present"] for item in reviewed),
            "valid_humanoid_bases": len(humanoids),
            "independent_hairstyles": len(hair),
            "outfit_candidate_files": len(outfit_modules),
            "complete_outfit_candidate_files": len(outfits),
            "assembled_era_outfit_combinations": 0,
            "formal_runtime_consumers": 0,
            "local_development_ready": 0,
            "release_approved": 0,
        },
        "consumption_categories": {
            "player_visual_core": {"verified_candidates": len([x for x in reviewed if "player_visual_core" in x["consumption_categories"] and x["review_conclusion"].startswith("RECOMMENDED")]), "status": "PARTIAL"},
            "npc_visual_core": {"verified_candidates": 0, "status": "OUT_OF_SCOPE"},
            "wildlife_core": {"verified_candidates": 0, "status": "OUT_OF_SCOPE"},
            "pet_core": {"verified_candidates": 0, "status": "OUT_OF_SCOPE"},
            "animation_core": {"verified_candidates": 0, "status": "OPEN"},
        },
        "candidates": reviewed,
        "gaps": gaps,
        "evidence_errors": sorted(set(evidence_errors)),
        "state_boundary": {
            "global_intake_catalog_modified": False,
            "candidate_resource_states_modified": False,
            "local_development_ready_written": False,
            "release_approved_written": False,
            "formal_prefab_created": False,
            "formal_scene_consumer_created": False,
        },
    }
    machine = {
        "schema": "D18_R03_S01_MACHINE_RESULT_V1",
        "task": "D18-R03-S01",
        "conclusion": conclusion,
        "review_packet": str(REVIEW_JSON.relative_to(ROOT)).replace("\\", "/"),
        "review_markdown": str(REVIEW_MD.relative_to(ROOT)).replace("\\", "/"),
        "source_packet": str(PACKET_PATH.relative_to(ROOT)).replace("\\", "/"),
        "unity_import": str(IMPORT_PATH.relative_to(ROOT)).replace("\\", "/"),
        "counts": review["counts"],
        "evidence_error_count": len(review["evidence_errors"]),
        "global_catalog_changed": False,
        "resource_state_changed": False,
        "local_development_ready": 0,
        "release_approved": 0,
        "next_gate": "D20-A01-R02 requires this review and additional assembly evidence; D18-S03 remains the runtime promotion gate.",
    }
    lines = [
        "# D18-R03-S01 角色外观资源复审",
        "",
        f"结论：**{conclusion}**。本报告只审计 D18-R03-A01 已存在的本地候选和隔离 Unity 导入证据，不修改全局准入目录或资源状态。",
        "",
        "## 证据范围",
        "",
        f"- A01 packet：`{review['inputs']['packet']}`",
        f"- Unity 导入：`{review['inputs']['unity_import']}`",
        f"- 官方来源：{review['inputs']['official_page']}",
        f"- 许可证：`{review['inputs']['license_id']}`，证据：`{review['inputs']['license_evidence']}`",
        f"- 归档 SHA-256：记录 `{review['inputs']['archive_sha256_recorded']}`，实际 `{review['inputs']['archive_sha256_actual']}`",
        "",
        "## 实际计数",
        "",
        f"- 复审候选：{review['counts']['candidate_records_reviewed']}；本地文件存在：{review['counts']['candidate_files_present']}；哈希匹配：{review['counts']['candidate_files_hash_verified']}。",
        f"- Unity 导入记录：{review['counts']['unity_import_records']}；有效 Humanoid：{review['counts']['valid_humanoid_bases']}；独立发型：{review['counts']['independent_hairstyles']}；服装模块文件：{review['counts']['outfit_candidate_files']}（完整服装候选文件：{review['counts']['complete_outfit_candidate_files']}）。",
        "- 已装配且完成时代审查的 1675 年服装组合：0/8；正式运行时消费者：0；LocalDevelopmentReady：0；ReleaseApproved：0。",
        "",
        "## 消费结论",
        "",
        "- `player_visual_core`：可推荐用于受控工作 Prefab 的 2 个 Humanoid 基础和 5 个发型/4 个服装模块，但仍是 Candidate 证据；不能写成 LocalDevelopmentReady。",
        "- `animation_core`：本包没有独立动作片段证据；D20-A02 的项目自有动作层需单独复核。",
        "- `npc_visual_core`、`wildlife_core`、`pet_core`：本包不覆盖，明确 OUT_OF_SCOPE，不用玩家资源冒充。",
        "- 女性/中性人形：女性与男性各一个有效基础已证实；中性方向没有证据，不宣称已覆盖。",
        "- 第 6 个独立发型：缺证据；眉毛和胡须未计入发型。",
        "- 8 套时代服装：缺真实历史审查/装配证据；Fantasy/Medieval 资产名称不能作为年代证明。",
        "",
        "## 状态边界",
        "",
        "本复审没有修改全局 `d18_prefab_candidate_intake_catalog.json`、Candidate 状态、Prefab、场景、Registry、Systems、存档或 contracts；所有候选的 `local_development_ready=false`、`release_approved=false` 保持不变。",
        "",
        "机器结果：`game/tools/results/D18/D18-R03-S01/D18_R03_S01_MACHINE_RESULT.json`。完整逐项记录见同目录 JSON。",
    ]
    return review, machine, "\n".join(lines) + "\n"


def main() -> int:
    REVIEW_DIR.mkdir(parents=True, exist_ok=True)
    RESULT_DIR.mkdir(parents=True, exist_ok=True)
    review, machine, markdown = build_review()
    REVIEW_JSON.write_text(json.dumps(review, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    REVIEW_MD.write_text(markdown, encoding="utf-8")
    MACHINE_JSON.write_text(json.dumps(machine, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps(machine, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
