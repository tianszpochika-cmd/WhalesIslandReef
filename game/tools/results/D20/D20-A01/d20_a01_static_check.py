#!/usr/bin/env python3
"""Read-only D20-A01 asset/interface check; does not import Unity assets."""

from __future__ import annotations

import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[5]
INTERFACE = ROOT / "game/tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE.json"
PREFAB = ROOT / "game/Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_Neutral.prefab"
CATALOG = ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"


def main() -> int:
    interface = json.loads(INTERFACE.read_text(encoding="utf-8"))
    catalog = json.loads(CATALOG.read_text(encoding="utf-8"))
    prefab_text = PREFAB.read_text(encoding="utf-8")
    eligible = [
        item for item in catalog.get("eligible_entries", [])
        if item.get("intake_id") == "intake_3469c541b7a7968018ba"
    ]
    checks = {
        "interface_json_valid": True,
        "status_is_partial": interface.get("status") == "PARTIAL",
        "rig_not_overclaimed": interface.get("rig_interface_ready") is False,
        "base_prefab_exists": PREFAB.is_file(),
        "source_guid_is_intake_model": "2d800d0a6b7873b42a6c68d197a0d187" in prefab_text,
        "no_gameplay_script_in_prefab": "m_Script:" not in prefab_text,
        "no_animator_in_prefab": "Animator:" not in prefab_text,
        "no_character_controller_in_prefab": "CharacterController:" not in prefab_text,
        "prefab_root_parent_is_scene_root": "m_TransformParent: {fileID: 0}" in prefab_text,
        "candidate_is_intake_eligible": len(eligible) == 1 and eligible[0].get("intake_eligible") is True,
        "fallback_preserves_state": interface.get("stable_fallback", {}).get("preserves_gameplay_state") is True,
        "a02_contract_is_read_only": interface.get("consumer_must_not_write_back") is True,
    }
    result = {
        "task": "D20-A01",
        "audit_kind": "static_candidate_prefab_interface",
        "read_only": True,
        "unity_editor_run": False,
        "status": "PASS" if all(checks.values()) else "FAIL",
        "checks": checks,
        "known_unverified": [
            "Unity YAML import and PrefabUtility validation",
            "SkinnedMeshRenderer/material binding",
            "Avatar and exact D20 skeleton mapping",
            "bounds/foot/camera/anchor runtime probes",
            "pink material and Console regression check",
        ],
    }
    out = ROOT / "game/tools/results/D20/D20-A01/D20_A01_STATIC_CHECK.json"
    out.write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False))
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
