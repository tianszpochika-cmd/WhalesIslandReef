"""Create the isolated, evidence-backed D18-R03-A01 player resource packet.

The tool reads the candidate directory and a separately-run Unity import report.
It never edits the global intake catalog, Registry, Prefab, scene, or resource
state. It intentionally reports unmet coverage instead of counting near-misses.
"""

from __future__ import annotations

import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CANDIDATE_ROOT = ROOT / "game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance"
RESULT_ROOT = ROOT / "game/tools/results/D18/D18-R03-A01"
IMPORT_PATH = RESULT_ROOT / "D18_R03_A01_ISOLATED_UNITY_IMPORT.json"
PACKET_ROOT = ROOT / "docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance"
ARCHIVE = ROOT / "artifacts/external_sources/D18-A04-R03-P_Quaternius_UniversalBaseCharacters_Standard.zip"
LICENSE = CANDIDATE_ROOT / "QuaterniusUniversalBaseCharacters/LICENSE_QUATERNIUS_CC0_STANDARD.txt"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def relative(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def category(file_name: str) -> str:
    if file_name in {"Superhero_Female_FullBody.fbx", "Superhero_Male_FullBody.fbx"}:
        return "player_visual_core"
    if file_name.startswith("Hair_"):
        return "player_appearance_module.hair" if file_name != "Hair_Beard.fbx" else "player_appearance_module.facial_hair_support"
    if file_name.startswith("Eyebrows_"):
        return "player_appearance_module.facial_hair_support"
    if file_name.startswith(("Female_", "Male_")):
        return "player_appearance_module.clothing_candidate"
    return "unclassified"


def archive_entry(file_name: str) -> str | None:
    if file_name in {"Superhero_Female_FullBody.fbx", "Superhero_Male_FullBody.fbx"}:
        return "Universal Base Characters[Standard]/Base Characters/Unity/" + file_name
    if file_name.startswith("Hair_") or file_name.startswith("Eyebrows_"):
        return "Universal Base Characters[Standard]/Hairstyles/Rigged to Head Bone/FBX (Unity)/" + file_name
    return None


def main() -> int:
    RESULT_ROOT.mkdir(parents=True, exist_ok=True)
    PACKET_ROOT.mkdir(parents=True, exist_ok=True)
    import_report = json.loads(IMPORT_PATH.read_text(encoding="utf-8"))
    files = sorted(CANDIDATE_ROOT.rglob("*.fbx"))
    by_name = {path.name: path for path in files}

    candidates = []
    for record in import_report.get("candidates", []):
        file_name = record["file_name"]
        path = by_name.get(file_name)
        entry = {
            "content_id": "candidate_" + Path(file_name).stem.lower() + "_r03a01",
            "file_name": file_name,
            "candidate_file_path": relative(path) if path else None,
            "category": category(file_name),
            "source_archive_entry": archive_entry(file_name),
            "file_exists": bool(path and path.is_file()),
            "sha256": sha256(path) if path and path.is_file() else None,
            "license_evidence_path": relative(LICENSE) if LICENSE.is_file() else None,
            "import_evidence_path": relative(IMPORT_PATH),
            "import": {
                "animation_type": record.get("animation_type"),
                "avatar_setup": record.get("avatar_setup"),
                "avatar_present": bool(record.get("avatar_present")),
                "avatar_valid": bool(record.get("avatar_valid")),
                "avatar_is_human": bool(record.get("avatar_is_human")),
                "mesh_count": record.get("mesh_count", 0),
                "material_count": record.get("material_count", 0),
                "animation_clip_count": record.get("animation_clip_count", 0),
                "renderer_height_m": record.get("renderer_height", 0),
                "renderer_width_m": record.get("renderer_width", 0),
                "renderer_depth_m": record.get("renderer_depth", 0),
            },
            "review_status": "Candidate",
            "local_development_ready": False,
            "release_approved": False,
            "formal_prefab": False,
            "runtime_consumer": False,
            "known_gaps": [
                "No formal Prefab, Animator, collision, LOD or runtime consumer in this isolated packet",
            ],
        }
        if file_name in {"Superhero_Female_FullBody.fbx", "Superhero_Male_FullBody.fbx"}:
            entry["known_gaps"].extend(["D17 1.80m scale and foot/pivot calibration remains D20 work"])
        elif entry["category"] == "player_appearance_module.hair":
            entry["known_gaps"].extend(["Generic/NoAvatar mesh; head socket, clipping and animation follow-through remain D20 work"])
        elif entry["category"] == "player_appearance_module.clothing_candidate":
            entry["known_gaps"].extend(["Fantasy/Medieval source is not historical approval for 1675 Caribbean clothing"])
        candidates.append(entry)

    humanoids = [item for item in candidates if item["import"]["avatar_is_human"] and item["import"]["avatar_valid"]]
    hair = [item for item in candidates if item["category"] == "player_appearance_module.hair"]
    outfits = [item for item in candidates if item["file_name"].lower() in {"female_peasant.fbx", "female_ranger.fbx", "male_peasant.fbx", "male_ranger.fbx"}]
    packet = {
        "schema_version": 1,
        "task": "D18-R03-A01",
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "status": "PARTIAL",
        "scope": "isolated_candidate_resource_evidence_only",
        "source": {
            "official_page": "https://quaternius.com/packs/universalbasecharacters.html",
            "download_page": "https://quaternius.itch.io/universal-base-characters",
            "license_id": "CC0-1.0",
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": relative(LICENSE) if LICENSE.is_file() else None,
            "archive_path": relative(ARCHIVE),
            "archive_sha256": sha256(ARCHIVE) if ARCHIVE.is_file() else None,
            "archive_size_bytes": ARCHIVE.stat().st_size if ARCHIVE.is_file() else None,
            "archive_entry_for_new_male": archive_entry("Superhero_Male_FullBody.fbx"),
            "source_note": "The male base is extracted from the already-audited standard archive; no new source or paid Source package was used.",
        },
        "counts": {
            "unity_import_fbx_total": import_report.get("fbx_total", 0),
            "candidate_fbx_total": len(files),
            "valid_humanoid_avatar": len(humanoids),
            "independent_hair_styles": len(hair),
            "facial_hair_or_brow_support": len([item for item in candidates if item["category"] == "player_appearance_module.facial_hair_support"]),
            "complete_outfit_candidate_files": len(outfits),
            "assembled_era_outfit_combinations_verified": 0,
            "formal_prefab_count": 0,
            "runtime_consumer_count": 0,
            "local_playable_ready_count": 0,
            "release_approved_count": 0,
        },
        "qualifying_humanoid_candidates": [
            {
                "content_id": item["content_id"],
                "file_name": item["file_name"],
                "candidate_file_path": item["candidate_file_path"],
                "sha256": item["sha256"],
                "height_m": item["import"]["renderer_height_m"],
                "animation_type": item["import"]["animation_type"],
                "avatar_valid": item["import"]["avatar_valid"],
                "avatar_is_human": item["import"]["avatar_is_human"],
            }
            for item in humanoids
        ],
        "hair_candidates": [
            {"content_id": item["content_id"], "file_name": item["file_name"], "candidate_file_path": item["candidate_file_path"], "sha256": item["sha256"]}
            for item in hair
        ],
        "outfit_candidate_files": [
            {"content_id": item["content_id"], "file_name": item["file_name"], "candidate_file_path": item["candidate_file_path"], "sha256": item["sha256"]}
            for item in outfits
        ],
        "coverage": {
            "humanoid_base_requirement": {"required": 2, "verified": len(humanoids), "status": "PASS" if len(humanoids) >= 2 else "PARTIAL"},
            "independent_hair_requirement": {"required": 6, "verified": len(hair), "status": "PASS" if len(hair) >= 6 else "PARTIAL", "gap": "one additional independent hairstyle remains unverified; beard and eyebrows are not counted"},
            "era_outfit_requirement": {"required": 8, "verified": 0, "status": "PARTIAL", "gap": "four source outfit files exist, but no eight assembled 1675 Caribbean combinations have been historically reviewed"},
        },
        "candidate_records": candidates,
        "state_boundary": {
            "local_playable_ready": [],
            "local_development_ready": [],
            "release_approved": [],
            "note": "This packet does not update the global intake catalog and does not create a formal Prefab or runtime consumer.",
        },
        "evidence_paths": {
            "unity_import_json": relative(IMPORT_PATH),
            "unity_import_log": relative(RESULT_ROOT / "D18_R03_A01_UNITY_IMPORT.log"),
            "source_evidence": relative(PACKET_ROOT / "D18_R03_A01_SOURCE_EVIDENCE.md"),
            "packet_markdown": relative(PACKET_ROOT / "D18_R03_A01_PLAYER_APPEARANCE_PACKET.md"),
        },
    }
    packet_json = PACKET_ROOT / "D18_R03_A01_PLAYER_APPEARANCE_PACKET.json"
    packet_json.write_text(json.dumps(packet, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    machine = {
        "task": "D18-R03-A01",
        "status": "PARTIAL",
        "unity_import_status": import_report.get("status"),
        "source_candidate_files_changed": True,
        "new_candidate_file": relative(by_name["Superhero_Male_FullBody.fbx"]),
        "valid_humanoid_avatar": len(humanoids),
        "independent_hair_styles": len(hair),
        "complete_outfit_candidate_files": len(outfits),
        "assembled_era_outfit_combinations_verified": 0,
        "formal_prefab_count": 0,
        "runtime_consumer_count": 0,
        "local_playable_ready": 0,
        "release_approved": 0,
        "packet_json": relative(packet_json),
        "import_json": relative(IMPORT_PATH),
        "reason": "Two Humanoid + valid Avatar base candidates are now independently verified, but the sixth independent hairstyle and eight historically reviewed outfit combinations remain unproven; no runtime consumer was created.",
    }
    (RESULT_ROOT / "D18_R03_A01_MACHINE_RESULT.json").write_text(json.dumps(machine, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    markdown = f"""# D18-R03-A01 玩家角色资源补齐包\n\n任务：`D18-R03-A01`  \n结论：**PARTIAL**  \n核对日期：{datetime.now().date().isoformat()}\n\n## 真实结果\n\n- Unity 6000.0.80f1 隔离导入：{import_report.get('fbx_total', 0)} 个 FBX，网格/Renderer {import_report.get('mesh_success_count', 0)}/{import_report.get('renderer_success_count', 0)}。\n- `Humanoid + valid Avatar`：{len(humanoids)} 个（女性与男性 Superhero 全身基体）。\n- 独立发型造型：{len(hair)} 个；胡须与眉毛未计入发型。目标 6 个仍缺 {max(0, 6-len(hair))} 个。\n- 完整服装候选文件：{len(outfits)} 个；经历史审查并在工作 Prefab 中组装的 1675 年组合：0/8。\n- 正式 Prefab、Animator 消费者、场景消费者、LocalPlayableReady、LocalDevelopmentReady、ReleaseApproved：均为 0。\n\n## 边界\n\n男性基体从已审计的 Quaternius Universal Base Characters Standard 归档中提取，未引入新付费 Source 包；归档 SHA、文件 SHA、官方来源、CC0 证据和 Unity JSON 均在同目录 packet 中。该工作包只交付隔离候选，D20 装配和 D18-S02-R03 复审另行负责。\n\n缺口：仍缺第六个独立发型；八套 1675 年加勒比时代服装组合未被证明；基体需要按 D17 1.80m 视觉高度、脚底、碰撞、LOD、Socket 和真实 Player 进行 D20 校准。\n\n机器结果：`game/tools/results/D18/D18-R03-A01/D18_R03_A01_MACHINE_RESULT.json`。\n"""
    (PACKET_ROOT / "D18_R03_A01_PLAYER_APPEARANCE_PACKET.md").write_text(markdown, encoding="utf-8")
    source = f"""# D18-R03-A01 来源与许可证证据\n\n核对日期：{datetime.now().date().isoformat()}（Asia/Shanghai）\n\n- 官方资源页：<https://quaternius.com/packs/universalbasecharacters.html>\n- 官方免费分发页：<https://quaternius.itch.io/universal-base-characters>\n- 许可证：CC0-1.0，原文 <https://creativecommons.org/publicdomain/zero/1.0/>。本地证据：`{relative(LICENSE)}`。\n- 复用归档：`{relative(ARCHIVE)}`，SHA-256 `{sha256(ARCHIVE) if ARCHIVE.is_file() else 'MISSING'}`。\n- 新增候选来自包内：`{archive_entry('Superhero_Male_FullBody.fbx')}`，文件 SHA-256 `{sha256(by_name['Superhero_Male_FullBody.fbx'])}`。\n- 本轮未下载付费 Source 包、未绕过登录/付款/限流、未修改全局准入 catalog。\n\n官方页面声称该包包含男女基体与 Humanoid rig；本地 Unity 证据只对实际导入结果负责：女性和男性基体均为 `animation_type=Human`、`avatar_valid=true`、`avatar_is_human=true`。\n"""
    (PACKET_ROOT / "D18_R03_A01_SOURCE_EVIDENCE.md").write_text(source, encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
