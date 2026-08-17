"""D18-L44 game reference audit tests (wave 3, serial; waits for L43).

Positive fixtures (must stay PASS):
- runtime C# using stable keys (asset_/ph_/d10: ResourceKeys)
- Registry JSON with snake_case business keys
- comment lines, test files and Editor files referencing Candidates are
  reported as informational only (documentation/test/editor_only) and never
  block

Negative fixtures (must be BLOCKED with the exact rule + P0 severity):
- runtime C# referencing Assets/Art/Candidates
- runtime C# referencing a .zip archive path
- JSON referencing a temp download cache
- JSON key position holding a display name (whitespace / upper case)
- JSON key position holding a path
- JSON reference field (e.g. sky_reference_asset) holding a candidate_ value
- runtime C# holding a candidate_ key inside a resource context (P1, human
  review, not blocking)

Contracts: read-only (no file modified), never starts Unity, result JSON
serialisable.
"""

import json
import tempfile
import unittest
from pathlib import Path

from tools.d18_game_reference_audit import (
    DEFAULT_SCAN_ROOTS,
    scan_game_references,
)

CLEAN_RUNTIME_CS = """using UnityEngine;

public class CleanRuntime : MonoBehaviour
{
    private void Start()
    {
        var prefab = Resources.Load<GameObject>("Prefabs/ph_ship_fishing");
        var key = "asset_kenney_ui_pack_v2";
        var rk = "d10:ship:ship_medium_v01";
    }
}
"""

CLEAN_REGISTRY_JSON = """{
  "id": "animal_agouti_forest",
  "schema_version": 1,
  "name": "Agouti Forest",
  "display_name_key": "content.animal.animal_agouti_forest.name",
  "category": "animal"
}
"""

COMMENT_CS = """public class CommentOnly
{
    // TODO: later the candidate pack lives under Assets/Art/Candidates/Kenney.
    public void Load()
    {
        Resources.Load<GameObject>("Prefabs/ph_rock");
    }
}
"""

TEST_FILE_CS = """using NUnit.Framework;

public class CandidateRefTests
{
    [Test]
    public void UsesCandidatePathInFixture()
    {
        var path = "Assets/Art/Candidates/Environment/Rock.fbx";
        Assert.NotNull(path);
    }
}
"""

EDITOR_BUILDER_CS = """using UnityEditor;

public class FixtureBuilder
{
    private const string ModelPath = "Assets/Art/Candidates/CharactersAnimals/Cat.fbx";
    public static void Build()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
    }
}
"""

RUNTIME_CANDIDATE_PATH_CS = """public class RuntimeViolation
{
    public void Load()
    {
        var path = "Assets/Art/Candidates/Kenney/PirateKit/Models/ship-medium.fbx";
        Resources.Load<GameObject>(path);
    }
}
"""

RUNTIME_ARCHIVE_CS = """public class ArchiveViolation
{
    private const string Zip = "C:/Users/Administrator/Downloads/kenney_ui-pack.zip";
}
"""

RUNTIME_CANDIDATE_KEY_CS = """public class CandidateKeySuspect
{
    private const string ResourceKey = "candidate_hdri_green_point_park";
}
"""

TEMP_CACHE_JSON = """{
  "name": "runtime_config",
  "cache": "C:/Users/Administrator/AppData/Local/WhalesIslandReef/D18/cache/kenney.zip",
  "usage": "loading"
}
"""

DISPLAY_NAME_KEY_JSON = """{
  "id": "Agouti Forest",
  "schema_version": 1
}
"""

PATH_KEY_JSON = """{
  "content_id": "Assets/Art/Candidates/Environment/Rock.fbx"
}
"""

REFERENCE_CANDIDATE_JSON = """{
  "profile": "d19_lighting_profile_1",
  "sky_reference_asset": "candidate_hdri_green_point_park"
}
"""


class GameReferenceAuditTests(unittest.TestCase):
    def setUp(self) -> None:
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)
        self.scripts = self.root / "game" / "Assets" / "Scripts"
        self.content = self.root / "game" / "Assets" / "Content"
        self.settings = self.root / "game" / "Assets" / "Settings"
        self.resources = self.root / "game" / "Assets" / "Resources"
        for folder in (self.scripts, self.content, self.settings, self.resources):
            folder.mkdir(parents=True, exist_ok=True)

    def tearDown(self) -> None:
        self._tmp.cleanup()

    def _write(self, relative: str, text: str) -> Path:
        path = self.root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")
        return path

    def _run(self, relative: str, text: str) -> dict:
        self._write(relative, text)
        return scan_game_references(
            self.root.resolve(),
            roots=[(self.root / item).resolve() for item in DEFAULT_SCAN_ROOTS],
            extensions={".cs", ".json"},
        )

    def _runtime_rules(self, result: dict) -> list:
        return [f for f in result["findings"] if f["classification"] == "runtime"]

    # ------------------------------------------------------------------
    # positive fixtures
    # ------------------------------------------------------------------

    def test_clean_runtime_passes(self):
        self._write("game/Assets/Scripts/Gameplay/CleanRuntime.cs", CLEAN_RUNTIME_CS)
        self._write("game/Assets/Content/Registry/clean.json", CLEAN_REGISTRY_JSON)
        result = scan_game_references(
            self.root.resolve(),
            roots=[(self.root / item).resolve() for item in DEFAULT_SCAN_ROOTS],
            extensions={".cs", ".json"},
        )
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["runtime_findings_count"], 0)

    def test_comment_mention_is_documentation(self):
        result = self._run("game/Assets/Scripts/Gameplay/CommentOnly.cs", COMMENT_CS)
        self.assertEqual(result["status"], "PASS")
        matches = [f for f in result["findings"] if f["rule"] == "candidate_path_reference"]
        self.assertEqual(len(matches), 1)
        self.assertEqual(matches[0]["classification"], "documentation")
        self.assertEqual(matches[0]["severity"], "INFO")

    def test_test_file_is_test_classification(self):
        result = self._run("game/Assets/Scripts/Gameplay/Tests/CandidateRefTests.cs", TEST_FILE_CS)
        self.assertEqual(result["status"], "PASS")
        matches = [f for f in result["findings"] if f["rule"] == "candidate_path_reference"]
        self.assertEqual(len(matches), 1)
        self.assertEqual(matches[0]["classification"], "test")

    def test_editor_file_is_editor_only(self):
        result = self._run("game/Assets/Scripts/Gameplay/Editor/FixtureBuilder.cs", EDITOR_BUILDER_CS)
        self.assertEqual(result["status"], "PASS")
        matches = [f for f in result["findings"] if f["rule"] == "candidate_path_reference"]
        self.assertEqual(len(matches), 1)
        self.assertEqual(matches[0]["classification"], "editor_only")
        self.assertEqual(matches[0]["owner"], "gameplay")

    def test_state_code_candidate_prefix_is_not_reported(self):
        # candidate_required 是状态码，不是资源业务键；行级规则不得误报。
        source = (
            "public class Guard\n{\n"
            "    private const string Code = \"candidate_required\";\n}\n"
        )
        result = self._run("game/Assets/Scripts/Systems/Guard.cs", source)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["findings_count"], 0)

    # ------------------------------------------------------------------
    # negative fixtures
    # ------------------------------------------------------------------

    def test_runtime_candidate_path_blocks(self):
        result = self._run("game/Assets/Scripts/Gameplay/RuntimeViolation.cs", RUNTIME_CANDIDATE_PATH_CS)
        self.assertEqual(result["status"], "BLOCKED")
        finding = self._runtime_rules(result)[0]
        self.assertEqual(finding["rule"], "candidate_path_reference")
        self.assertEqual(finding["severity"], "P0")
        self.assertEqual(finding["owner"], "gameplay")
        self.assertGreaterEqual(finding["line"], 1)

    def test_runtime_archive_reference_blocks(self):
        result = self._run("game/Assets/Scripts/Gameplay/ArchiveViolation.cs", RUNTIME_ARCHIVE_CS)
        self.assertEqual(result["status"], "BLOCKED")
        rules = {f["rule"] for f in self._runtime_rules(result)}
        self.assertIn("archive_reference", rules)

    def test_temp_cache_json_blocks(self):
        result = self._run("game/Assets/Settings/runtime_config.json", TEMP_CACHE_JSON)
        self.assertEqual(result["status"], "BLOCKED")
        rules = {f["rule"] for f in self._runtime_rules(result)}
        self.assertIn("temp_cache_reference", rules)

    def test_display_name_as_key_blocks(self):
        result = self._run("game/Assets/Content/Registry/display_key.json", DISPLAY_NAME_KEY_JSON)
        self.assertEqual(result["status"], "BLOCKED")
        finding = self._runtime_rules(result)[0]
        self.assertEqual(finding["rule"], "display_name_as_key")
        self.assertEqual(finding["severity"], "P0")
        self.assertEqual(finding["owner"], "registry")

    def test_path_as_key_blocks(self):
        result = self._run("game/Assets/Content/Registry/path_key.json", PATH_KEY_JSON)
        self.assertEqual(result["status"], "BLOCKED")
        rules = {f["rule"] for f in self._runtime_rules(result)}
        # 值同时是候选路径（行级）与键位置路径（结构级）；结构级规则必须命中。
        self.assertIn("key_position_path_reference", rules)
        self.assertIn("candidate_path_reference", rules)

    def test_reference_field_with_candidate_value_blocks(self):
        result = self._run("game/Assets/Settings/Lighting/profile.json", REFERENCE_CANDIDATE_JSON)
        self.assertEqual(result["status"], "BLOCKED")
        finding = self._runtime_rules(result)[0]
        self.assertEqual(finding["rule"], "candidate_key_as_business_key")
        self.assertEqual(finding["severity"], "P0")
        self.assertEqual(finding["owner"], "settings")

    def test_candidate_key_in_resource_context_is_p1(self):
        result = self._run("game/Assets/Scripts/Gameplay/CandidateKeySuspect.cs", RUNTIME_CANDIDATE_KEY_CS)
        self.assertEqual(result["status"], "PASS")
        finding = self._runtime_rules(result)[0]
        self.assertEqual(finding["rule"], "candidate_key_as_business_key")
        self.assertEqual(finding["severity"], "P1")

    # ------------------------------------------------------------------
    # contracts
    # ------------------------------------------------------------------

    def test_tool_is_read_only(self):
        relative = "game/Assets/Scripts/Gameplay/RuntimeViolation.cs"
        path = self._write(relative, RUNTIME_CANDIDATE_PATH_CS)
        before = path.read_text(encoding="utf-8")
        self._run(relative, RUNTIME_CANDIDATE_PATH_CS)
        self.assertEqual(path.read_text(encoding="utf-8"), before)

    def test_result_json_serialisable(self):
        result = self._run("game/Assets/Scripts/Gameplay/RuntimeViolation.cs", RUNTIME_CANDIDATE_PATH_CS)
        json.dumps(result, ensure_ascii=False)


if __name__ == "__main__":
    unittest.main()
