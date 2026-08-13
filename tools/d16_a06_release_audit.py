"""D16-A06 release-preparation static audit.

This checks repository-local evidence only. It never builds, installs,
uninstalls, uploads or sends anything to an external service.
"""

import argparse
import json
import re
from pathlib import Path


def read(path: Path):
    try:
        return path.read_text(encoding="utf-8")
    except OSError:
        return ""


def installer_status_matches_evidence(manifest_installer, formal_installer_smoke_evidence: bool) -> bool:
    """Require the manifest to reflect a passed formal smoke test when one exists."""
    if not formal_installer_smoke_evidence:
        return True
    return (
        isinstance(manifest_installer, dict)
        and all(manifest_installer.get(key) == "PASS"
                for key in ("install_test", "uninstall_test", "update_test"))
    )


def audit(project: Path):
    settings = project / "ProjectSettings" / "ProjectSettings.asset"
    build_settings = project / "ProjectSettings" / "EditorBuildSettings.asset"
    version_file = project / "ProjectSettings" / "ProjectVersion.txt"
    steam = project / "Assets" / "Scripts" / "Systems" / "D5" / "D5SteamSystem.cs"
    template_fallback = project / "Assets" / "Scripts" / "Agent" / "AgentCore" / "LLMClient.cs"
    save_safety = project / "Assets" / "Scripts" / "Systems" / "Save" / "SaveFileSafety.cs"
    local_diagnostics = project / "Assets" / "Scripts" / "Gameplay" / "D16" / "D16LocalDiagnostics.cs"
    diagnostic_record = project / "Assets" / "Scripts" / "Gameplay" / "D16" / "D16DiagnosticRecord.cs"
    release_manifest = project / "Assets" / "Resources" / "D16ReleaseManifest.json"

    settings_text = read(settings)
    build_text = read(build_settings)
    version_text = read(version_file)
    steam_text = read(steam)
    fallback_text = read(template_fallback)
    manifest_data = {}
    manifest_parse_error = ""
    build_log = project.parent / "tools" / "results" / "d16_a06_windows_build_unity6.log"
    build_player = project.parent / "artifacts" / "d16_a06_windows_build" / "WhalesIslandReef.exe"
    build_evidence = build_player.exists() and "Build Finished, Result: Success" in read(build_log)
    install_update_result = project.parent / "tools" / "results" / "d16_a06_install_update_smoke_result.json"
    install_update_data = {}
    if install_update_result.exists():
        try:
            install_update_data = json.loads(read(install_update_result))
        except json.JSONDecodeError:
            install_update_data = {}
    workspace_install_update_evidence = (
        isinstance(install_update_data, dict)
        and install_update_data.get("status") == "PASS"
        and install_update_data.get("install_ok") is True
        and install_update_data.get("update_preserved_user_save") is True
        and install_update_data.get("uninstall_preserved_user_save") is True
    )
    if release_manifest.exists():
        try:
            manifest_data = json.loads(read(release_manifest))
        except json.JSONDecodeError as error:
            manifest_parse_error = str(error)
    manifest_steam = manifest_data.get("steam") if isinstance(manifest_data, dict) else None
    manifest_crash_reporting = manifest_data.get("crash_reporting") if isinstance(manifest_data, dict) else None
    manifest_content_registry = manifest_data.get("content_registry") if isinstance(manifest_data, dict) else None
    manifest_save_compatibility = manifest_data.get("save_compatibility") if isinstance(manifest_data, dict) else None
    manifest_installer = manifest_data.get("installer") if isinstance(manifest_data, dict) else None
    manifest_blocking_reasons = []
    if isinstance(manifest_data, dict) and isinstance(manifest_data.get("blocking_reasons"), list):
        manifest_blocking_reasons = [
            str(reason) for reason in manifest_data.get("blocking_reasons", [])
            if str(reason).strip()
        ]
    workspace_deployment_patterns = ("*installer*.ps1", "*installer*.bat", "*install*.ps1", "*install*.bat", "*updater*.ps1", "*updater*.bat", "*update*.ps1", "*update*.bat")
    formal_installer_artifact_patterns = ("*.msi", "*.msix", "*.appx", "*setup*.exe", "*installer*.exe", "*updater*.exe")
    tool_root = project.parent / "tools"
    workspace_deployment_script_present = any(
        candidate.is_file()
        for folder in ("Assets", "ProjectSettings")
        for pattern in workspace_deployment_patterns
        for candidate in project.joinpath(folder).rglob(pattern)
    ) or any(candidate.is_file() for pattern in workspace_deployment_patterns for candidate in tool_root.glob(pattern))
    formal_installer_artifact_present = any(
        candidate.is_file()
        for pattern in formal_installer_artifact_patterns
        for candidate in (project.parent / "artifacts").rglob(pattern)
    )
    formal_installer_result = project.parent / "tools" / "results" / "d16_a06_formal_installer_smoke_result.json"
    formal_installer_data = {}
    if formal_installer_result.exists():
        try:
            formal_installer_data = json.loads(read(formal_installer_result))
        except json.JSONDecodeError:
            formal_installer_data = {}
    formal_installer_smoke_evidence = (
        isinstance(formal_installer_data, dict)
        and formal_installer_data.get("status") == "PASS"
        and formal_installer_data.get("install_ok") is True
        and formal_installer_data.get("failed_update_rejected") is True
        and formal_installer_data.get("failed_update_preserved_install") is True
        and formal_installer_data.get("failed_update_preserved_user_save") is True
        and formal_installer_data.get("update_ok") is True
        and formal_installer_data.get("uninstall_ok") is True
        and formal_installer_data.get("user_save_preserved") is True
    )
    manifest_installer_status_aligned = installer_status_matches_evidence(
        manifest_installer, formal_installer_smoke_evidence)
    project_product_match = re.search(r"^\s*productName:\s*(.+?)\s*$", settings_text, re.MULTILINE)
    project_company_match = re.search(r"^\s*companyName:\s*(.+?)\s*$", settings_text, re.MULTILINE)
    project_version_match = re.search(r"^\s*bundleVersion:\s*(.+?)\s*$", settings_text, re.MULTILINE)
    manifest_runtime_contract_valid = (
        isinstance(manifest_data, dict)
        and isinstance(manifest_data.get("product_name"), str)
        and isinstance(manifest_data.get("company_name"), str)
        and isinstance(manifest_data.get("unity_version"), str)
        and manifest_data.get("target_platform") == "StandaloneWindows64"
        and (project_product_match is None or manifest_data.get("product_name") == project_product_match.group(1))
        and (project_company_match is None or manifest_data.get("company_name") == project_company_match.group(1))
        and (project_version_match is None or isinstance(manifest_data.get("bundle_version", ""), str))
        and isinstance(manifest_content_registry, dict)
        and manifest_content_registry.get("path") == "Assets/Resources/ContentRegistry/registry.json"
        and manifest_content_registry.get("qa_status") == "D15-S02-PASS"
        and manifest_content_registry.get("asset_authorization_status") in {"PENDING", "PASS", "APPROVED"}
        and isinstance(manifest_data.get("localization"), list)
        and {"zh-CN", "en-US"}.issubset(set(manifest_data.get("localization", [])))
        and manifest_data.get("offline_mode") is True
        and isinstance(manifest_save_compatibility, dict)
        and isinstance(manifest_save_compatibility.get("current_version"), int)
        and manifest_save_compatibility.get("current_version") >= 1
        and manifest_save_compatibility.get("file_backup_required") is True
        and manifest_save_compatibility.get("migration_failure_preserves_original") is True
        and isinstance(manifest_installer, dict)
        and all(manifest_installer.get(key) in {"PENDING", "PASS", "BLOCKED"}
                for key in ("install_test", "uninstall_test", "update_test"))
    )
    manifest_valid = (
        isinstance(manifest_data, dict)
        and manifest_data.get("release_status") == "blocked"
        and manifest_data.get("manifest_version") == 1
        and isinstance(manifest_data.get("required_scenes"), list)
        and "Assets/Scenes/MainMenu.unity" in manifest_data.get("required_scenes", [])
        and "Assets/Scenes/InitialHarbor.unity" in manifest_data.get("required_scenes", [])
        and isinstance(manifest_data.get("blocking_reasons"), list)
        and len(manifest_data.get("blocking_reasons", [])) > 0
        and isinstance(manifest_steam, dict)
        and manifest_steam.get("upload_authorized") is False
        and manifest_steam.get("upload_executed") is False
        and isinstance(manifest_crash_reporting, dict)
        and manifest_crash_reporting.get("external_upload") is False
        and manifest_crash_reporting.get("local_opt_in") is True
        and manifest_crash_reporting.get("raw_message_collection") is False
        and manifest_crash_reporting.get("raw_stack_collection") is False
        and manifest_crash_reporting.get("delete_entrypoint") == "D16LocalDiagnostics.RevokeConsentAndClear"
        and manifest_runtime_contract_valid
    )
    checks = {
        "project_version_present": bool(re.search(r"m_EditorVersion:\s*\S+", version_text)),
        "windows_product_identity_present": all(
            token in settings_text for token in ("companyName:", "productName:", "bundleVersion:")
        ),
        "required_scenes_enabled": all(
            token in build_text for token in ("Assets/Scenes/MainMenu.unity", "Assets/Scenes/InitialHarbor.unity")
        ),
        "crash_report_api_not_enabled_by_default": "enableCrashReportAPI: 0" in settings_text,
        "offline_steam_bridge_present": "OfflineMode" in steam_text and "UNCONFIGURED" in steam_text,
        "template_fallback_present": "TemplateFallback" in fallback_text,
        "file_level_save_safety_present": save_safety.exists(),
        "local_redacted_diagnostics_present": (
            local_diagnostics.exists()
            and diagnostic_record.exists()
            and "PlayerPrefs.GetInt(ConsentKey, 0) == 1" in read(local_diagnostics)
            and "raw_message_omitted" in read(diagnostic_record)
            and "raw_stack_omitted" in read(diagnostic_record)
            and "Application.logMessageReceived" in read(local_diagnostics)
        ),
        "release_manifest_present": release_manifest.exists(),
        "release_manifest_runtime_contract_complete": manifest_runtime_contract_valid,
        "release_manifest_installer_status_aligned": manifest_installer_status_aligned,
        "release_manifest_blocked_and_complete": manifest_valid,
        "steam_upload_policy_safe": (
            isinstance(manifest_steam, dict)
            and manifest_steam.get("upload_authorized") is False
            and manifest_steam.get("upload_executed") is False
        ),
        "unity_windows_build_evidence": build_evidence,
        "workspace_install_update_smoke_evidence": workspace_install_update_evidence,
        # 工作区脚本只证明隔离 staging 部署入口存在，不冒充正式 OS 安装器。
        "installer_or_update_script_present": workspace_deployment_script_present,
        "workspace_deployment_script_present": workspace_deployment_script_present,
        "formal_installer_artifact_present": formal_installer_artifact_present,
        "formal_installer_smoke_evidence": formal_installer_smoke_evidence,
    }
    missing = [name for name, passed in checks.items() if not passed]
    blockers = []
    if "release_manifest_present" in missing:
        blockers.append("release_manifest_missing")
    elif "release_manifest_blocked_and_complete" in missing:
        blockers.append("release_manifest_invalid_or_not_blocked")
    if "release_manifest_runtime_contract_complete" in missing:
        blockers.append("release_manifest_contract_incomplete")
    if "release_manifest_installer_status_aligned" in missing:
        blockers.append("release_manifest_installer_status_stale")
    if not checks["local_redacted_diagnostics_present"]:
        blockers.append("local_redacted_diagnostics_missing")
    if "installer_or_update_script_present" in missing:
        blockers.append("installer_update_flow_missing")
    if not formal_installer_artifact_present:
        blockers.append("formal_os_installer_missing")
    elif not formal_installer_smoke_evidence:
        blockers.append("formal_os_installer_not_smoke_tested")
    if not checks["workspace_install_update_smoke_evidence"]:
        blockers.append("install_uninstall_update_not_run")
    if not checks["unity_windows_build_evidence"]:
        blockers.append("unity_build_not_run")
    # 发布清单中的项目级阻断必须原样进入审计结果，避免只显示 Steam 政策而
    # 隐藏目标设备、隐私、A01—A05 等尚未闭合的证据。Steam 未授权是策略性
    # 延后项，仍单独保留在 policy_blocking_reasons 中，不能伪装成已上传。
    for reason in manifest_blocking_reasons:
        if reason not in blockers:
            blockers.append(reason)
    policy_blocking_reasons = []
    if (
        isinstance(manifest_steam, dict)
        and manifest_steam.get("upload_authorized") is False
        and manifest_steam.get("upload_executed") is False
    ):
        policy_blocking_reasons.append("steam_upload_not_authorized")
        if not any(
            reason.strip().lower().replace("_", " ") == "steam upload not authorized"
            for reason in blockers
        ) and "steam_upload_not_authorized" not in blockers:
            blockers.append("steam_upload_not_authorized")
    local_blockers = [
        reason for reason in blockers
        if reason not in manifest_blocking_reasons and reason not in policy_blocking_reasons
    ]

    return {
        "task": "D16-A06",
        "status": "IN_PROGRESS",
        "project": str(project),
        "checks": checks,
        "missing_static_evidence": missing,
        "blocking_reasons": blockers,
        "local_blocking_reasons": local_blockers,
        "release_manifest_blocking_reasons": manifest_blocking_reasons,
        "policy_blocking_reasons": policy_blocking_reasons,
        "technical_preparation_status": "PASS" if not local_blockers and not missing else "IN_PROGRESS",
        "release_manifest_status": manifest_data.get("release_status") if isinstance(manifest_data, dict) else None,
        "release_manifest_blocking_reason_count": len(manifest_data.get("blocking_reasons", [])) if isinstance(manifest_data, dict) and isinstance(manifest_data.get("blocking_reasons"), list) else 0,
        "release_manifest_parse_error": manifest_parse_error,
        "policy": {
            "external_upload_performed": False,
            "steam_upload_performed": False,
            "destructive_install_test_performed": False,
            "workspace_staging_install_update_test_performed": workspace_install_update_evidence,
            "formal_os_installer_test_performed": formal_installer_smoke_evidence,
        },
        "note": "工作区 staging 部署入口与正式便携 Windows 安装器分开统计；正式安装器的本机安装/负向更新/合法更新/卸载烟测已实测，Steam 上传仍未授权、未执行。",
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("project", type=Path)
    parser.add_argument("--json-out", type=Path)
    args = parser.parse_args()
    result = audit(args.project)
    output = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
