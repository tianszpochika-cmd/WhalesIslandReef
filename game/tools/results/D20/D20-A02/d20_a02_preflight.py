#!/usr/bin/env python3
"""Read-only D20-A02 gate check.

This deliberately does not create or mutate an Animator, Prefab, controller,
or runtime code. A blocked preflight exits zero so the result itself is a
stable, machine-readable handoff rather than a failed shell command.
"""

from __future__ import annotations

import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[5]
A01_INTERFACE = ROOT / "game/tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE.json"
A01_PREFAB = ROOT / "game/Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_Neutral.prefab"
D18_CATALOG = ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"
D18_HANDOFF = ROOT / "docs/game/design/common/handoffs/D18/D18-A04-R01_HANDOFF.md"
D20_STANDARD = ROOT / "docs/game/design/common/D20_CHARACTER_RIG_ANIMATION_STANDARD.md"
D6_CONTROLLER = ROOT / "game/Assets/Resources/D6/D6CrewAnimator.controller"


def main() -> int:
    interface_exists = A01_INTERFACE.is_file()
    interface = json.loads(A01_INTERFACE.read_text(encoding="utf-8")) if interface_exists else {}
    catalog = json.loads(D18_CATALOG.read_text(encoding="utf-8")) if D18_CATALOG.is_file() else {}

    eligible_action_candidates = [
        entry
        for entry in catalog.get("eligible_entries", [])
        if "D20-A02" in entry.get("allowed_task_scope", [])
        and entry.get("manifest_id") in {
            "pack_kenney_blocky_characters_d18_a04_v1",
            "pack_quaternius_animated_men_d18_a04_v1",
        }
    ]

    skeleton = interface.get("skeleton_avatar", {})
    fallback = interface.get("stable_fallback", {})
    checks = {
        "a01_interface_exists": interface_exists,
        "a01_interface_json_valid": interface_exists and interface.get("task") == "D20-A01",
        "a01_rig_interface_ready": interface.get("rig_interface_ready") is True,
        "a01_base_prefab_exists": A01_PREFAB.is_file(),
        "a01_avatar_present": skeleton.get("avatar_present") is True,
        "a01_not_generic_no_avatar": not (
            skeleton.get("animation_type") == "Generic"
            and skeleton.get("avatar_setup") == "NoAvatar"
        ),
        "a01_fallback_declared": (
            fallback.get("preserves_gameplay_state") is True
            and fallback.get("preserves_appearance_history") is True
        ),
        "d18_action_evidence_present": D18_HANDOFF.is_file(),
        "d20_standard_present": D20_STANDARD.is_file(),
        "legacy_controller_present": D6_CONTROLLER.is_file(),
    }

    blockers = []
    if not checks["a01_rig_interface_ready"]:
        blockers.append({
            "id": "D20-A01-RIG-INTERFACE-NOT-READY",
            "reason": "D20-A01 interface explicitly sets rig_interface_ready=false.",
            "observed": interface.get("rig_interface_ready"),
        })
    if not checks["a01_avatar_present"] or not checks["a01_not_generic_no_avatar"]:
        blockers.append({
            "id": "D20-A01-AVATAR-UNVERIFIED",
            "reason": "The admitted player source is Generic/NoAvatar and has no Avatar; A02 cannot safely retarget player actions.",
            "animation_type": skeleton.get("animation_type"),
            "avatar_setup": skeleton.get("avatar_setup"),
            "avatar_present": skeleton.get("avatar_present"),
        })

    status = "BLOCKED" if blockers else "READY_FOR_IMPLEMENTATION"
    result = {
        "schema_version": 1,
        "task": "D20-A02",
        "status": status,
        "read_only": True,
        "implementation_started": False,
        "implementation_allowed": not blockers,
        "a01_interface_path": "game/tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE.json",
        "a01_observed": {
            "status": interface.get("status"),
            "rig_interface_ready": interface.get("rig_interface_ready"),
            "base_prefab_path": interface.get("repository_prefab_path"),
            "skeleton_avatar": skeleton,
            "stable_fallback": fallback,
        },
        "d18_legal_action_candidate_summary": {
            "eligible_entry_count_for_d20_a02": len(eligible_action_candidates),
            "manifest_ids": sorted({entry.get("manifest_id") for entry in eligible_action_candidates}),
            "evidence_handoff": "docs/game/design/common/handoffs/D18/D18-A04-R01_HANDOFF.md",
            "known_evidence": "24 imported FBX candidates and 228 source clips; no Avatar/Animator consumer evidence.",
        },
        "checks": checks,
        "blockers": blockers,
        "planned_action_mapping_not_implemented": {
            "locomotion": ["Idle", "Walk", "Run"],
            "airborne": ["Jump", "Fall", "Land"],
            "interaction": ["Interact", "Talk", "Carry", "Fish", "Helm"],
            "ship": ["Board", "Disembark"],
            "optional": ["Hit", "Recover"],
            "snapshot_inputs": [
                "grounded",
                "speed01",
                "vertical_speed",
                "control_mode",
                "interaction_kind",
                "action_intent",
                "health_band_or_hit_event",
                "lod_band",
            ],
            "root_motion": "false; D17 CharacterController remains the only movement writer",
        },
        "artifacts_not_created_due_to_gate": [
            "game/Assets/Animations/D20/Player/ Animator Controller or Override",
            "game/Assets/Scripts/Presentation/D20/PlayerAnimation/ runtime adapter",
            "D20-A02 Unity/PlayMode screenshots and action regression evidence",
        ],
        "next_gate": "D20-A01 must publish rig_interface_ready=true with verified Avatar/skeleton/Prefab evidence before rerunning D20-A02.",
    }
    output = ROOT / "game/tools/results/D20/D20-A02/D20_A02_PREFLIGHT_RESULT.json"
    output.write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
