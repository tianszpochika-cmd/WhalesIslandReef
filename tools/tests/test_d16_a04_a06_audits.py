import json
import sys
import tempfile
import unittest
from datetime import date, timedelta
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d16_a04_review_audit import audit as audit_a04  # noqa: E402
from d16_a04_review_queue import build_queue as build_a04_queue  # noqa: E402
from d16_a04_review_queue import render_markdown as render_a04_queue  # noqa: E402
from d16_a04_approve_explicit_fictional import apply_gate as apply_fictional_gate  # noqa: E402
from d16_a04_apply_review_decision import apply_decision as apply_review_decision  # noqa: E402
from d16_a05_asset_audit import audit as audit_a05  # noqa: E402
from d16_a05_asset_audit import normalize_project_root  # noqa: E402
from d16_a06_release_audit import audit as audit_a06  # noqa: E402
from d16_a06_release_audit import installer_status_matches_evidence  # noqa: E402


class D16A04A06AuditTests(unittest.TestCase):
    def test_a04_fictional_gate_is_strict_and_dry_run_by_default(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory) / "game"
            root.mkdir()
            evidence = Path(directory) / "fictional.md"
            evidence.write_text("project-authored fictional content", encoding="utf-8")
            (root / "safe.json").write_text(json.dumps({
                "id": "item_safe_fiction",
                "historical_tag": "fictional",
                "historical_review": "Pending",
                "tags": ["sample"],
                "content_warning_ids": [],
                "skip_allowed": False,
                "source": {"kind": "project-authored"},
                "license": {"status": "internal-authored"},
                "review_evidence_refs": ["fictional.md"],
            }), encoding="utf-8")
            (root / "inferred.json").write_text(json.dumps({
                "id": "item_inferred",
                "historical_tag": "inferred",
                "historical_review": "Pending",
                "tags": [],
                "content_warning_ids": [],
                "skip_allowed": False,
                "source": {"kind": "project-authored"},
                "license": {"status": "internal-authored"},
                "review_evidence_refs": ["fictional.md"],
            }), encoding="utf-8")
            preview = apply_fictional_gate(root, apply=False, review_date="2026-08-12")
            self.assertEqual(preview["eligible_count"], 1)
            self.assertEqual(preview["changed_count"], 0)
            self.assertEqual(json.loads((root / "safe.json").read_text(encoding="utf-8"))["historical_review"], "Pending")
            applied = apply_fictional_gate(root, apply=True, review_date="2026-08-12")
            safe = json.loads((root / "safe.json").read_text(encoding="utf-8"))
        self.assertEqual(applied["changed_ids"], ["item_safe_fiction"])
        self.assertEqual(safe["historical_review"], "Approved")
        self.assertTrue(safe["review_decision_basis"]["not_a_historical_fact_claim"])
        self.assertEqual(safe["reviewer"], "D16-A04-SAFE-FICTIONAL-GATE")

    def test_a04_queue_is_human_only_and_preserves_pending(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "asset.json").write_text(json.dumps({
                "id": "asset_pending",
                "asset_state": "placeholder",
                "historical_tag": "inferred",
                "historical_review": "Pending",
                "review_evidence_refs": ["source-ref"],
                "content_warning_ids": [],
                "skip_allowed": False,
                "source_refs": ["source-doc"],
                "source": {"kind": "project-authored-placeholder"},
                "license": {"status": "internal-authored"},
            }), encoding="utf-8")
            (root / "quest.json").write_text(json.dumps({
                "id": "quest_sensitive",
                "historical_tag": "historical",
                "historical_review": "Pending",
                "tags": ["gambling"],
                "content_warning_ids": ["gambling"],
                "skip_allowed": True,
                "review_evidence_refs": ["quest-doc"],
            }), encoding="utf-8")
            queue = build_a04_queue(root)
            markdown = render_a04_queue(queue)
        self.assertEqual(queue["status"], "BLOCKED")
        self.assertEqual(queue["pending_count"], 2)
        self.assertEqual(queue["scope_counts"]["asset_history_and_authorization"], 1)
        self.assertEqual(queue["scope_counts"]["sensitive_content"], 1)
        self.assertTrue(all(record["approval_policy"] == "human_decision_required" for record in queue["records"]))
        asset_record = next(record for record in queue["records"] if record["id"] == "asset_pending")
        self.assertEqual(asset_record["source"]["kind"], "project-authored-placeholder")
        self.assertEqual(asset_record["license"]["status"], "internal-authored")
        self.assertIn("不批准历史事实", markdown)
        self.assertIn("asset_pending", markdown)

    def test_a04_queue_batches_are_stable_and_never_mix_scopes(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for entry_id, payload in (
                ("asset_pending", {
                    "asset_state": "placeholder",
                    "historical_tag": "inferred",
                }),
                ("quest_pending", {
                    "historical_tag": "historical",
                    "tags": [],
                }),
                ("quest_sensitive", {
                    "historical_tag": "historical",
                    "tags": ["gambling"],
                    "content_warning_ids": ["gambling"],
                }),
            ):
                record = {
                    "id": entry_id,
                    "historical_review": "Pending",
                    "content_warning_ids": [],
                    "skip_allowed": False,
                    "source": {"kind": "project-authored"},
                    "license": {"status": "internal-authored"},
                }
                record.update(payload)
                (root / f"{entry_id}.json").write_text(json.dumps(record), encoding="utf-8")
            queue = build_a04_queue(root, batch_size=1)

        self.assertEqual(queue["pending_count"], 3)
        self.assertEqual(queue["batch_count"], 3)
        self.assertEqual(
            [batch["batch_id"] for batch in queue["batches"]],
            ["D16-A04-001", "D16-A04-002", "D16-A04-003"],
        )
        self.assertTrue(all(batch["record_count"] == 1 for batch in queue["batches"]))
        self.assertTrue(all(len({record["scope"] for record in queue["records"] if record["batch_id"] == batch["batch_id"]}) == 1 for batch in queue["batches"]))
        self.assertTrue(all(record["approval_policy"] == "human_decision_required" for record in queue["records"]))

    def test_a04_keeps_pending_review_as_a_blocker(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "entry.json").write_text(json.dumps({
                "id": "asset_test",
                "asset_state": "integrated",
                "historical_tag": "inferred",
                "historical_review": "Pending",
                "review_evidence_refs": ["test-ref"],
                "content_warning_ids": [],
                "skip_allowed": False,
                "source": {"kind": "project-authored"},
                "license": {"status": "internal-authored"},
            }), encoding="utf-8")
            result = audit_a04(root)
        self.assertEqual(result["pending_review_count"], 1)
        self.assertIn("pending_historical_reviews", result["blocking_reasons"])

    def test_a04_explicit_decision_is_dry_run_then_single_record_write(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory) / "game"
            root.mkdir()
            evidence = Path(directory) / "review.md"
            evidence.write_text("Reviewer confirmed this is a project-authored fictional item.", encoding="utf-8")
            entry_path = root / "item.json"
            entry_path.write_text(json.dumps({
                "id": "item_reviewable_fiction",
                "historical_tag": "fictional",
                "historical_review": "Pending",
                "tags": [],
                "content_warning_ids": [],
                "skip_allowed": False,
                "review_evidence_refs": [],
                "source": {"kind": "project-authored"},
                "license": {"status": "internal-authored"},
            }), encoding="utf-8")
            kwargs = {
                "root": root,
                "record_id": "item_reviewable_fiction",
                "decision": "Approved",
                "reviewer": "Historical Content Reviewer",
                "evidence_refs": ["review.md"],
                "decision_basis": "The reviewer checked the attached project evidence and confirmed the fictional scope.",
                "review_date": "2026-08-12",
            }
            preview = apply_review_decision(apply=False, **kwargs)
            self.assertEqual(preview["status"], "DRY_RUN")
            self.assertEqual(json.loads(entry_path.read_text(encoding="utf-8"))["historical_review"], "Pending")
            applied = apply_review_decision(apply=True, **kwargs)
            payload = json.loads(entry_path.read_text(encoding="utf-8"))
        self.assertEqual(applied["status"], "APPLIED")
        self.assertEqual(payload["historical_review"], "Approved")
        self.assertEqual(payload["reviewer"], "Historical Content Reviewer")
        self.assertTrue(payload["review_decision_basis"]["explicit_human_decision"])
        self.assertFalse(payload["review_decision_basis"]["automated_approval"])

    def test_a04_sensitive_decision_requires_warning_skip_and_presentation(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory) / "game"
            root.mkdir()
            (Path(directory) / "sensitive-review.md").write_text(
                "Reviewer confirmed the contextual gambling warning and skip behavior.", encoding="utf-8")
            entry_path = root / "quest.json"
            entry_path.write_text(json.dumps({
                "id": "quest_sensitive_review",
                "historical_tag": "inferred",
                "historical_review": "Pending",
                "tags": ["gambling"],
                "content_warning_ids": [],
                "skip_allowed": False,
                "presentation_levels": [],
                "review_evidence_refs": [],
            }), encoding="utf-8")
            common = {
                "root": root,
                "record_id": "quest_sensitive_review",
                "reviewer": "Sensitive Content Reviewer",
                "evidence_refs": ["sensitive-review.md"],
                "decision_basis": "The reviewer selected contextual presentation with a visible warning and skip path.",
                "review_date": "2026-08-12",
            }
            unsafe = apply_review_decision(decision="Approved", apply=True, **common)
            self.assertEqual(unsafe["status"], "INVALID")
            self.assertEqual(json.loads(entry_path.read_text(encoding="utf-8"))["historical_review"], "Pending")
            applied = apply_review_decision(
                decision="ApprovedWithWarning",
                warning_ids=["warning_gambling_context"],
                skip_allowed=True,
                presentation_levels=["Contextual"],
                apply=True,
                **common,
            )
            payload = json.loads(entry_path.read_text(encoding="utf-8"))
        self.assertEqual(applied["status"], "APPLIED")
        self.assertEqual(payload["historical_review"], "ApprovedWithWarning")
        self.assertEqual(payload["content_warning_ids"], ["warning_gambling_context"])
        self.assertTrue(payload["skip_allowed"])
        self.assertEqual(payload["presentation_levels"], ["Contextual"])

    def test_a04_explicit_decision_refuses_missing_evidence_without_mutation(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory) / "game"
            root.mkdir()
            entry_path = root / "item.json"
            entry_path.write_text(json.dumps({
                "id": "item_missing_evidence",
                "historical_tag": "fictional",
                "historical_review": "Pending",
                "content_warning_ids": [],
                "skip_allowed": False,
            }), encoding="utf-8")
            result = apply_review_decision(
                root,
                record_id="item_missing_evidence",
                decision="Approved",
                reviewer="Reviewer",
                evidence_refs=["not-present.md"],
                decision_basis="This basis is long enough but the evidence is absent.",
                review_date="2026-08-12",
                apply=True,
            )
            payload = json.loads(entry_path.read_text(encoding="utf-8"))
        self.assertEqual(result["status"], "INVALID")
        self.assertIn("evidence_ref_not_found:not-present.md", result["errors"])
        self.assertEqual(payload["historical_review"], "Pending")

    def test_a04_rejects_invalid_or_future_review_date_without_mutation(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory) / "game"
            root.mkdir()
            (Path(directory) / "review.md").write_text(
                "Reviewer confirmed the evidence and decision scope.", encoding="utf-8")
            entry_path = root / "item.json"
            entry_path.write_text(json.dumps({
                "id": "item_invalid_review_date",
                "historical_tag": "fictional",
                "historical_review": "Pending",
                "content_warning_ids": [],
                "skip_allowed": False,
                "source": {"kind": "project-authored"},
                "license": {"status": "internal-authored"},
            }), encoding="utf-8")
            common = {
                "root": root,
                "record_id": "item_invalid_review_date",
                "decision": "Approved",
                "reviewer": "Historical Content Reviewer",
                "evidence_refs": ["review.md"],
                "decision_basis": "The reviewer checked the evidence and confirmed the fictional scope.",
                "apply": True,
            }
            invalid = apply_review_decision(review_date="2026-13-40", **common)
            future = apply_review_decision(
                review_date=(date.today() + timedelta(days=1)).isoformat(), **common)
            payload = json.loads(entry_path.read_text(encoding="utf-8"))
        self.assertEqual(invalid["status"], "INVALID")
        self.assertIn("review_date_must_be_iso_date", invalid["errors"])
        self.assertEqual(future["status"], "INVALID")
        self.assertIn("review_date_cannot_be_in_future", future["errors"])
        self.assertEqual(payload["historical_review"], "Pending")

    def test_a04_accepts_explicit_project_authored_placeholder_license(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "entry.json").write_text(json.dumps({
                "id": "asset_project_placeholder",
                "asset_state": "placeholder",
                "historical_tag": "fictional",
                "historical_review": "Approved",
                "review_evidence_refs": ["project-review"],
                "content_warning_ids": [],
                "skip_allowed": True,
                "source": {"kind": "project-authored-placeholder"},
                "license": {"status": "internal-authored-plus-cc0"},
            }), encoding="utf-8")
            result = audit_a04(root)
        self.assertEqual(result["authorization_record_count"], 1)
        self.assertNotIn("visual_asset_authorization_not_closed", result["blocking_reasons"])

    def test_a05_never_treats_placeholder_as_final(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            assets = root / "Assets"
            assets.mkdir()
            (root / "entry.json").write_text(json.dumps({
                "id": "asset_placeholder",
                "asset_state": "placeholder",
                "production_stage": "integrated",
            }), encoding="utf-8")
            result = audit_a05(root, assets)
        self.assertEqual(result["asset_state_counts"]["placeholder"], 1)
        self.assertIn("placeholder_assets_remain", result["blocking_reasons"])

    def test_a05_known_runtime_placeholders_have_traceable_metadata(self):
        registry = ROOT / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries" / "asset"
        expected = {
            "asset_ph_crew_male": (
                "game/Assets/Art/Placeholder/Models/Crew/ph_crew_male.fbx",
                "game/Assets/Prefabs/Placeholder/ph_crew_male.prefab",
            ),
            "asset_ph_palm_leaves": (
                "game/Assets/Art/Placeholder/Textures/ph_palm_leaves.png",
                "game/Assets/Prefabs/Placeholder/ph_palm_tree.prefab",
            ),
        }
        for entry_id, (resource_path, runtime_prefab_path) in expected.items():
            with self.subTest(entry_id=entry_id):
                entry = json.loads((registry / f"{entry_id}.json").read_text(encoding="utf-8"))
                self.assertEqual(entry["asset_state"], "placeholder")
                self.assertEqual(entry["production_stage"], "prototype_placeholder")
                self.assertEqual(entry["resource_path"], resource_path)
                self.assertEqual(entry["runtime_prefab_path"], runtime_prefab_path)
                self.assertTrue((ROOT / resource_path).is_file())
                self.assertTrue((ROOT / runtime_prefab_path).is_file())
                self.assertTrue(entry["test_cases"])
                self.assertTrue(entry["lod_profile"])

    def test_a05_reports_declared_resource_paths_and_blocks_broken_links(self):
        with tempfile.TemporaryDirectory() as directory:
            project = Path(directory) / "game"
            assets = project / "Assets"
            assets.mkdir(parents=True)
            (assets / "model.obj").write_text("o model\n", encoding="utf-8")
            (project / "integrated.json").write_text(json.dumps({
                "id": "asset_integrated",
                "asset_state": "integrated",
                "production_stage": "integrated",
                "lod_profile": {"lod0": "source"},
                "test_cases": ["load"],
                "resource_path": "Assets/model.obj",
                "source_path": "Assets/model.obj#model",
            }), encoding="utf-8")
            (project / "placeholder.json").write_text(json.dumps({
                "id": "asset_placeholder",
                "asset_state": "placeholder",
                "runtime_prefab_path": "Assets/missing.prefab",
            }), encoding="utf-8")
            result = audit_a05(project, assets)

        self.assertEqual(result["declared_path_counts"]["resource_path"]["exists"], 1)
        self.assertEqual(result["declared_path_counts"]["source_path"]["exists"], 1)
        self.assertEqual(result["declared_path_counts"]["runtime_prefab_path"]["missing"], 1)
        self.assertEqual(result["declared_path_missing_count"], 1)
        self.assertIn("declared_asset_paths_missing", result["blocking_reasons"])

    def test_a05_cli_root_normalizes_nested_content_registry_to_full_unity_project(self):
        with tempfile.TemporaryDirectory() as directory:
            project = Path(directory) / "game"
            registry = project / "Assets" / "Resources" / "ContentRegistry"
            registry.mkdir(parents=True)
            (project / "ProjectSettings").mkdir()
            self.assertEqual(normalize_project_root(registry, project / "Assets"), project.resolve())

    def test_a06_reports_static_baseline_without_claiming_release(self):
        with tempfile.TemporaryDirectory() as directory:
            project = Path(directory)
            (project / "ProjectSettings").mkdir()
            (project / "Assets/Scripts/Systems/D5").mkdir(parents=True)
            (project / "Assets/Scripts/Agent/AgentCore").mkdir(parents=True)
            (project / "Assets/Scripts/Systems/Save").mkdir(parents=True)
            (project / "ProjectSettings/ProjectSettings.asset").write_text(
                "companyName: Test\nproductName: Test\nbundleVersion: 1\n"
                "enableCrashReportAPI: 0\n", encoding="utf-8")
            (project / "ProjectSettings/EditorBuildSettings.asset").write_text(
                "Assets/Scenes/MainMenu.unity\nAssets/Scenes/InitialHarbor.unity\n", encoding="utf-8")
            (project / "ProjectSettings/ProjectVersion.txt").write_text(
                "m_EditorVersion: 6000.0.80f1\n", encoding="utf-8")
            (project / "Assets/Scripts/Systems/D5/D5SteamSystem.cs").write_text(
                "OfflineMode UNCONFIGURED\n", encoding="utf-8")
            (project / "Assets/Scripts/Agent/AgentCore/LLMClient.cs").write_text(
                "TemplateFallback\n", encoding="utf-8")
            (project / "Assets/Scripts/Systems/Save/SaveFileSafety.cs").write_text(
                "class SaveFileSafety {}\n", encoding="utf-8")
            result = audit_a06(project)
        self.assertTrue(result["checks"]["template_fallback_present"])
        self.assertIn("release_manifest_missing", result["blocking_reasons"])
        self.assertFalse(result["policy"]["steam_upload_performed"])

    def test_a06_requires_a_blocked_complete_manifest(self):
        with tempfile.TemporaryDirectory() as directory:
            project = Path(directory)
            (project / "ProjectSettings").mkdir()
            (project / "Assets/Scripts/Systems/D5").mkdir(parents=True)
            (project / "Assets/Scripts/Agent/AgentCore").mkdir(parents=True)
            (project / "Assets/Scripts/Systems/Save").mkdir(parents=True)
            (project / "Assets/Resources").mkdir(parents=True)
            (project / "ProjectSettings/ProjectSettings.asset").write_text(
                "companyName: Test\nproductName: Test\nbundleVersion: 1\n"
                "enableCrashReportAPI: 0\n", encoding="utf-8")
            (project / "ProjectSettings/EditorBuildSettings.asset").write_text(
                "Assets/Scenes/MainMenu.unity\nAssets/Scenes/InitialHarbor.unity\n", encoding="utf-8")
            (project / "ProjectSettings/ProjectVersion.txt").write_text(
                "m_EditorVersion: 6000.0.80f1\n", encoding="utf-8")
            (project / "Assets/Scripts/Systems/D5/D5SteamSystem.cs").write_text(
                "OfflineMode UNCONFIGURED\n", encoding="utf-8")
            (project / "Assets/Scripts/Agent/AgentCore/LLMClient.cs").write_text(
                "TemplateFallback\n", encoding="utf-8")
            (project / "Assets/Scripts/Systems/Save/SaveFileSafety.cs").write_text(
                "class SaveFileSafety {}\n", encoding="utf-8")
            (project / "Assets/Scripts/Gameplay/D16").mkdir(parents=True)
            (project / "Assets/Scripts/Gameplay/D16/D16LocalDiagnostics.cs").write_text(
                "PlayerPrefs.GetInt(ConsentKey, 0) == 1\nApplication.logMessageReceived\n", encoding="utf-8")
            (project / "Assets/Scripts/Gameplay/D16/D16DiagnosticRecord.cs").write_text(
                "raw_message_omitted raw_stack_omitted\n", encoding="utf-8")
            (project / "Assets/Resources/D16ReleaseManifest.json").write_text(json.dumps({
                "manifest_version": 1,
                "release_status": "blocked",
                "product_name": "Test",
                "company_name": "Test",
                "unity_version": "6000.0.80f1",
                "target_platform": "StandaloneWindows64",
                "required_scenes": ["Assets/Scenes/MainMenu.unity", "Assets/Scenes/InitialHarbor.unity"],
                "content_registry": {
                    "path": "Assets/Resources/ContentRegistry/registry.json",
                    "qa_status": "D15-S02-PASS",
                    "asset_authorization_status": "PENDING",
                },
                "localization": ["zh-CN", "en-US"],
                "save_compatibility": {
                    "current_version": 3,
                    "file_backup_required": True,
                    "migration_failure_preserves_original": True,
                },
                "offline_mode": True,
                "blocking_reasons": ["test_pending"],
                "crash_reporting": {
                    "external_upload": False,
                    "local_opt_in": True,
                    "raw_message_collection": False,
                    "raw_stack_collection": False,
                    "delete_entrypoint": "D16LocalDiagnostics.RevokeConsentAndClear",
                },
                "installer": {
                    "install_test": "PENDING",
                    "uninstall_test": "PENDING",
                    "update_test": "PENDING",
                },
                "steam": {"upload_authorized": False, "upload_executed": False},
            }), encoding="utf-8")
            result = audit_a06(project)
            self.assertTrue(result["checks"]["release_manifest_blocked_and_complete"])
            self.assertTrue(result["checks"]["release_manifest_runtime_contract_complete"])
            self.assertTrue(result["checks"]["steam_upload_policy_safe"])
            self.assertTrue(result["checks"]["local_redacted_diagnostics_present"])
        self.assertEqual(result["release_manifest_status"], "blocked")
        self.assertNotIn("release_manifest_invalid_or_not_blocked", result["blocking_reasons"])

    def test_a06_reports_manifest_blockers_separately_from_policy_blockers(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "game"
            project.mkdir()
            (project / "Assets/Resources").mkdir(parents=True)
            (project / "Assets/Resources/D16ReleaseManifest.json").write_text(json.dumps({
                "release_status": "blocked",
                "blocking_reasons": ["D16-A06 target-device evidence pending"],
                "steam": {"upload_authorized": False, "upload_executed": False},
            }), encoding="utf-8")
            result = audit_a06(project)
        self.assertEqual(
            result["release_manifest_blocking_reasons"],
            ["D16-A06 target-device evidence pending"],
        )
        self.assertIn("D16-A06 target-device evidence pending", result["blocking_reasons"])
        self.assertEqual(result["policy_blocking_reasons"], ["steam_upload_not_authorized"])
        self.assertIn("release_manifest_invalid_or_not_blocked", result["local_blocking_reasons"])
        self.assertEqual(result["technical_preparation_status"], "IN_PROGRESS")

    def test_a06_rejects_incomplete_runtime_manifest_contract(self):
        with tempfile.TemporaryDirectory() as directory:
            project = Path(directory)
            (project / "ProjectSettings").mkdir()
            (project / "Assets/Scripts/Systems/D5").mkdir(parents=True)
            (project / "Assets/Scripts/Agent/AgentCore").mkdir(parents=True)
            (project / "Assets/Scripts/Systems/Save").mkdir(parents=True)
            (project / "Assets/Scripts/Gameplay/D16").mkdir(parents=True)
            (project / "Assets/Resources").mkdir(parents=True)
            (project / "ProjectSettings/ProjectSettings.asset").write_text(
                "companyName: Test\nproductName: Test\nbundleVersion: 1\n"
                "enableCrashReportAPI: 0\n", encoding="utf-8")
            (project / "ProjectSettings/EditorBuildSettings.asset").write_text(
                "Assets/Scenes/MainMenu.unity\nAssets/Scenes/InitialHarbor.unity\n", encoding="utf-8")
            (project / "ProjectSettings/ProjectVersion.txt").write_text(
                "m_EditorVersion: 6000.0.80f1\n", encoding="utf-8")
            (project / "Assets/Scripts/Systems/D5/D5SteamSystem.cs").write_text(
                "OfflineMode UNCONFIGURED\n", encoding="utf-8")
            (project / "Assets/Scripts/Agent/AgentCore/LLMClient.cs").write_text(
                "TemplateFallback\n", encoding="utf-8")
            (project / "Assets/Scripts/Systems/Save/SaveFileSafety.cs").write_text(
                "class SaveFileSafety {}\n", encoding="utf-8")
            (project / "Assets/Scripts/Gameplay/D16/D16LocalDiagnostics.cs").write_text(
                "PlayerPrefs.GetInt(ConsentKey, 0) == 1\nApplication.logMessageReceived\n", encoding="utf-8")
            (project / "Assets/Scripts/Gameplay/D16/D16DiagnosticRecord.cs").write_text(
                "raw_message_omitted raw_stack_omitted\n", encoding="utf-8")
            (project / "Assets/Resources/D16ReleaseManifest.json").write_text(json.dumps({
                "manifest_version": 1,
                "release_status": "blocked",
                "product_name": "Test",
                "company_name": "Test",
                "unity_version": "6000.0.80f1",
                "target_platform": "StandaloneWindows64",
                "required_scenes": ["Assets/Scenes/MainMenu.unity", "Assets/Scenes/InitialHarbor.unity"],
                "content_registry": {"path": "Assets/Resources/ContentRegistry/registry.json", "qa_status": "D15-S02-PASS", "asset_authorization_status": "PENDING"},
                "localization": ["zh-CN", "en-US"],
                "save_compatibility": {"current_version": 3, "file_backup_required": True, "migration_failure_preserves_original": True},
                "offline_mode": False,
                "crash_reporting": {
                    "external_upload": False,
                    "local_opt_in": True,
                    "raw_message_collection": False,
                    "raw_stack_collection": False,
                    "delete_entrypoint": "D16LocalDiagnostics.RevokeConsentAndClear",
                },
                "installer": {"install_test": "PENDING", "uninstall_test": "PENDING", "update_test": "PENDING"},
                "steam": {"upload_authorized": False, "upload_executed": False},
                "blocking_reasons": ["test_pending"],
            }), encoding="utf-8")
            result = audit_a06(project)
        self.assertFalse(result["checks"]["release_manifest_runtime_contract_complete"])
        self.assertIn("release_manifest_contract_incomplete", result["blocking_reasons"])

    def test_a06_consumes_local_windows_build_evidence(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "game"
            project.mkdir()
            build_dir = root / "artifacts" / "d16_a06_windows_build"
            build_dir.mkdir(parents=True)
            results_dir = root / "tools" / "results"
            results_dir.mkdir(parents=True)
            (build_dir / "WhalesIslandReef.exe").write_bytes(b"player")
            (results_dir / "d16_a06_windows_build_unity6.log").write_text(
                "Build Finished, Result: Success.\n", encoding="utf-8")
            result = audit_a06(project)
        self.assertTrue(result["checks"]["unity_windows_build_evidence"])
        self.assertNotIn("unity_build_not_run", result["blocking_reasons"])

    def test_a06_does_not_treat_existing_folder_as_installer_script(self):
        with tempfile.TemporaryDirectory() as directory:
            project = Path(directory) / "game"
            (project / "Assets").mkdir(parents=True)
            (project / "ProjectSettings").mkdir(parents=True)
            result = audit_a06(project)
        self.assertFalse(result["checks"]["installer_or_update_script_present"])
        self.assertIn("installer_update_flow_missing", result["blocking_reasons"])

    def test_a06_consumes_workspace_install_update_smoke_evidence(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "game"
            project.mkdir()
            results_dir = root / "tools" / "results"
            results_dir.mkdir(parents=True)
            (results_dir / "d16_a06_install_update_smoke_result.json").write_text(json.dumps({
                "status": "PASS",
                "install_ok": True,
                "update_preserved_user_save": True,
                "uninstall_preserved_user_save": True,
            }), encoding="utf-8")
            result = audit_a06(project)
        self.assertTrue(result["checks"]["workspace_install_update_smoke_evidence"])
        self.assertTrue(result["policy"]["workspace_staging_install_update_test_performed"])
        self.assertNotIn("install_uninstall_update_not_run", result["blocking_reasons"])

    def test_a06_detects_workspace_scoped_install_update_script(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "game"
            (project / "Assets").mkdir(parents=True)
            (project / "ProjectSettings").mkdir(parents=True)
            (root / "tools").mkdir(parents=True)
            (root / "tools/d16_a06_install_update.ps1").write_text("# workspace scoped test deployer\n", encoding="utf-8")
            result = audit_a06(project)
        self.assertTrue(result["checks"]["installer_or_update_script_present"])
        self.assertTrue(result["checks"]["workspace_deployment_script_present"])
        self.assertFalse(result["checks"]["formal_installer_artifact_present"])
        self.assertFalse(result["checks"]["formal_installer_smoke_evidence"])
        self.assertIn("formal_os_installer_missing", result["blocking_reasons"])
        self.assertNotIn("installer_update_flow_missing", result["blocking_reasons"])

    def test_a06_formal_installer_requires_negative_update_evidence(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "game"
            (project / "Assets").mkdir(parents=True)
            (project / "ProjectSettings").mkdir(parents=True)
            installer_dir = root / "artifacts" / "d16_a06_formal_installer"
            installer_dir.mkdir(parents=True)
            (installer_dir / "WhalesIslandReefSetup.exe").write_bytes(b"setup")
            results_dir = root / "tools" / "results"
            results_dir.mkdir(parents=True)
            (results_dir / "d16_a06_formal_installer_smoke_result.json").write_text(json.dumps({
                "status": "PASS",
                "install_ok": True,
                "update_ok": True,
                "uninstall_ok": True,
                "user_save_preserved": True,
            }), encoding="utf-8")
            result = audit_a06(project)
        self.assertTrue(result["checks"]["formal_installer_artifact_present"])
        self.assertFalse(result["checks"]["formal_installer_smoke_evidence"])
        self.assertIn("formal_os_installer_not_smoke_tested", result["blocking_reasons"])

    def test_a06_manifest_installer_status_must_match_formal_smoke(self):
        pending = {"install_test": "PENDING", "uninstall_test": "PENDING", "update_test": "PENDING"}
        passed = {"install_test": "PASS", "uninstall_test": "PASS", "update_test": "PASS"}
        self.assertTrue(installer_status_matches_evidence(pending, False))
        self.assertFalse(installer_status_matches_evidence(pending, True))
        self.assertTrue(installer_status_matches_evidence(passed, True))


if __name__ == "__main__":
    unittest.main()
