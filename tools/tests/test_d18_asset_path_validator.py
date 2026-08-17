import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_asset_path_validator import (  # noqa: E402
    CANDIDATE_ROOT,
    FORMAL_RESOURCE_ROOTS,
    LOGIC_METADATA_ROOTS,
    check_catalog,
    validate_path,
)


def codes(result):
    return tuple(issue.code for issue in result.issues)


class D18AssetPathValidatorTests(unittest.TestCase):
    def assert_valid(self, path, kind="evidence", field=""):
        result = validate_path(path, kind=kind, field=field)
        self.assertTrue(result.valid, f"应为合法路径，实际问题: {codes(result)}")
        return result

    def assert_rejected(self, path, expected_code, kind="evidence", field=""):
        result = validate_path(path, kind=kind, field=field)
        self.assertFalse(result.valid, f"应为被拒绝路径: {path!r}")
        self.assertIn(expected_code, codes(result))
        issue = next(i for i in result.issues if i.code == expected_code)
        self.assertTrue(issue.allowed_roots, "错误输出必须包含允许根")
        expected_value = "" if path is None else str(path)
        self.assertIn(expected_value, issue.value)
        return result

    # --- 分隔符：Windows / Unix / 混合 ---

    def test_windows_backslash_separators_accepted(self):
        self.assert_valid(r"game\Assets\Art\Candidates\Kenney\foo.fbx", kind="candidate")

    def test_unix_forward_separators_accepted(self):
        self.assert_valid("game/Assets/Art/Candidates/Kenney/foo.fbx", kind="candidate")

    def test_mixed_separators_normalized(self):
        result = self.assert_valid("game/Assets\\Art/Candidates\\Kenney/foo.fbx", kind="candidate")
        self.assertEqual(result.normalized, "game/Assets/Art/Candidates/Kenney/foo.fbx")

    def test_leading_dot_and_trailing_slash_normalized(self):
        result = self.assert_valid("./game/Assets/Art/Candidates/Kenney/foo.fbx/", kind="candidate")
        self.assertEqual(result.normalized, "game/Assets/Art/Candidates/Kenney/foo.fbx")

    # --- 绝对路径 / 驱动器 / UNC ---

    def test_absolute_posix_rejected(self):
        self.assert_rejected("/etc/art/foo.fbx", "absolute_path_rejected")

    def test_absolute_windows_rejected(self):
        self.assert_rejected(r"C:\Users\me\Desktop\game\first01\game\Assets\Art\Candidates\foo.fbx", "drive_path_rejected", kind="candidate")

    def test_absolute_windows_slash_drive_rejected(self):
        self.assert_rejected("C:/art/foo.fbx", "drive_path_rejected")

    def test_unc_rejected(self):
        self.assert_rejected(r"\\server\share\art\foo.fbx", "absolute_path_rejected")

    def test_drive_relative_rejected(self):
        self.assert_rejected("D:art/foo.fbx", "drive_path_rejected")

    def test_env_var_path_rejected(self):
        self.assert_rejected("%TEMP%/foo.fbx", "env_var_path_rejected")

    # --- 父级回退 .. ---

    def test_parent_traversal_rejected(self):
        self.assert_rejected("game/Assets/Art/Candidates/../../Scripts/Systems/Foo.cs", "parent_traversal_rejected", kind="candidate")

    def test_parent_traversal_windows_separators_rejected(self):
        self.assert_rejected(r"game\Assets\Art\Candidates\..\..\Scripts\Systems\Foo.cs", "parent_traversal_rejected")

    def test_parent_traversal_case_insensitive_segments(self):
        self.assert_rejected("GAME/ASSETS/ART/CANDIDATES/../..", "parent_traversal_rejected", kind="candidate")

    # --- 被忽略的本地缓存 ---

    def test_unity_library_cache_rejected(self):
        self.assert_rejected("game/Library/PackageCache/pkg/foo.fbx", "ignored_cache_path_rejected")

    def test_unity_temp_cache_rejected(self):
        self.assert_rejected("game/Temp/art/foo.fbx", "ignored_cache_path_rejected")

    def test_python_cache_rejected(self):
        self.assert_rejected("tools/__pycache__/d18_asset_path_validator.pyc", "ignored_cache_path_rejected")

    def test_node_modules_rejected(self):
        self.assert_rejected("server/node_modules/pkg/index.js", "ignored_cache_path_rejected")

    def test_artifacts_archive_rejected(self):
        self.assert_rejected("artifacts/external_sources/kenney.zip", "ignored_cache_path_rejected")

    def test_cache_segment_case_insensitive_rejected(self):
        self.assert_rejected("game/LIBRARY/foo.fbx", "ignored_cache_path_rejected")

    # --- 运行时引用字段 ---

    def test_candidate_binary_in_runtime_reference_rejected(self):
        self.assert_rejected(
            "game/Assets/Art/Candidates/Kenney/Models/tree.fbx",
            "candidate_binary_in_runtime_reference",
            kind="runtime_reference",
            field="runtime_reference_paths",
        )

    def test_runtime_reference_to_formal_resource_allowed(self):
        self.assert_valid("game/Assets/Art/Models/Ships/foo.fbx", kind="runtime_reference")

    def test_runtime_reference_to_resources_allowed(self):
        self.assert_valid("game/Assets/Resources/Fonts/noto.otf", kind="runtime_reference")

    def test_runtime_reference_to_metadata_rejected(self):
        self.assert_rejected(
            "docs/game/design/common/assets/packs/foo.manifest.json",
            "runtime_reference_outside_formal_roots",
            kind="runtime_reference",
        )

    # --- 大小写 ---

    def test_root_case_mismatch_rejected(self):
        self.assert_rejected("Game/Assets/Art/Candidates/Kenney/foo.fbx", "root_case_mismatch", kind="candidate")

    def test_canonical_case_accepted(self):
        self.assert_valid("game/Assets/Art/Candidates/Kenney/foo.fbx", kind="candidate")

    def test_case_insensitive_root_membership_but_mismatch_flagged(self):
        self.assert_rejected("GAME/Assets/Art/Candidates/x.png", "root_case_mismatch", kind="candidate")

    # --- 越界与合法相对路径 ---

    def test_candidate_outside_root_rejected(self):
        result = self.assert_rejected("game/Assets/Art/Models/Ships/foo.fbx", "outside_allowed_root", kind="candidate")
        self.assertIn(CANDIDATE_ROOT, result.issues[0].allowed_roots)

    def test_error_output_explains_allowed_roots(self):
        result = validate_path("game/Assets/Scripts/Systems/Foo.cs", kind="candidate")
        self.assertIn("outside_allowed_root", codes(result))
        self.assertIn(CANDIDATE_ROOT, result.issues[0].allowed_roots)
        self.assertIn("game/Assets/Scripts/Systems/Foo.cs", result.issues[0].value)

    def test_legal_candidate_path(self):
        self.assert_valid("game/Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/icon.png", kind="candidate")

    def test_legal_metadata_path(self):
        self.assert_valid("docs/game/design/common/assets/sources/d18_kenney_logic_catalog.json", kind="metadata")

    def test_legal_evidence_path_in_results(self):
        self.assert_valid("game/tools/results/D18/D18-A06/D18_A06_UNITY_IMPORT_AUDIT.json", kind="evidence")

    def test_candidate_license_evidence_allowed_as_evidence(self):
        self.assert_valid("game/Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/License.txt", kind="evidence")

    def test_legal_formal_path(self):
        for root in FORMAL_RESOURCE_ROOTS:
            self.assert_valid(f"{root}/foo.fbx", kind="formal")

    def test_metadata_outside_root_rejected(self):
        self.assert_rejected("game/Assets/Art/Candidates/Kenney/foo.fbx", "outside_allowed_root", kind="metadata")
        for root in LOGIC_METADATA_ROOTS:
            self.assertIn(root, validate_path("game/Assets/Art/Candidates/Kenney/foo.fbx", kind="metadata").issues[0].allowed_roots)

    def test_empty_and_none_rejected(self):
        self.assert_rejected("", "empty_path_rejected")
        self.assert_rejected(None, "empty_path_rejected")

    def test_root_itself_without_file_rejected(self):
        self.assert_rejected("game/Assets/Art/Candidates", "outside_allowed_root", kind="candidate")

    def test_sibling_root_name_not_confused(self):
        self.assert_rejected("game/Assets/Art/CandidatesFoo/x.png", "outside_allowed_root", kind="candidate")

    # --- 真实准入目录 ---

    def test_real_intake_catalog_has_no_path_violations(self):
        summary = check_catalog()
        self.assertEqual(summary["entries_checked"], 1865)
        self.assertEqual(summary["violation_count"], 0)
        self.assertEqual(summary["violation_by_code"], {})
        self.assertTrue(summary["read_only"])
        self.assertGreater(summary["path_fields_checked"], 0)


if __name__ == "__main__":
    unittest.main()
