"""Unit tests for the D18-L17 candidate runtime guard (read-only scanner).

Positive fixtures (violations that must be BLOCKED):
- runtime C# referencing Assets/Art/Candidates
- runtime config JSON referencing Assets/Art/Candidates
- Registry JSON combining Candidate state with a runtime resource path
- runtime reference to the temporary download cache
- runtime reference to an external archive path
- candidate path written with backslashes

Negative fixtures (must stay PASS, or be classified non-blocking):
- clean runtime code loading placeholder ResourceKeys / formal Prefab paths
- comment/documentation mentioning Candidates (informational only)
- Editor-only file referencing Candidates (editor_only, informational)
- #if UNITY_EDITOR block referencing Candidates (editor_only)
- variable names that merely contain "candidates" (no false positive)

The tool must never modify the scanned files (read-only contract).
"""

import json
import tempfile
import unittest
from pathlib import Path

from tools.d18_candidate_runtime_guard import (
    DEFAULT_SCAN_ROOTS,
    ScanConfig,
    scan_project,
)

CLEAN_RUNTIME_CS = """using UnityEngine;

public class CleanRuntime : MonoBehaviour
{
    private void Start()
    {
        // Formal resources are loaded by ResourceKey only.
        var prefab = Resources.Load<GameObject>("Prefabs/ph_ship_fishing");
        var audio = Resources.Load<AudioClip>("Audio/SFX/ph_dock_ambient");
    }
}
"""

CLEAN_REGISTRY_JSON = """{
  "id": "asset_d15_a06_model_blackreef_brig",
  "category": "asset",
  "source": { "kind": "project-authored" },
  "source_refs": ["game/Assets/Content/D15/A06"],
  "asset_refs": []
}
"""

COMMENT_CS = """public class CommentOnly
{
    // TODO: replace placeholder with a candidate later (Assets/Art/Candidates/Kenney).
    public void Load()
    {
        Resources.Load<GameObject>("Prefabs/ph_placeholder");
    }
}
"""

EDITOR_BUILDER_CS = """using UnityEditor;

public class CandidatePrefabBuilder
{
    private const string ModelPath = "Assets/Art/Candidates/CharactersAnimals/Cat.fbx";
    public static void Build()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
    }
}
"""

UNITY_EDITOR_BLOCK_CS = """using UnityEngine;

public class EditorConditional
{
    public void Run()
    {
#if UNITY_EDITOR
        var candidate = "Assets/Art/Candidates/Environment/Rock.fbx";
        UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(candidate);
#endif
        Resources.Load<GameObject>("Prefabs/ph_rock");
    }
}
"""

RUNTIME_CANDIDATE_CS = """using UnityEngine;

public class RuntimeViolation
{
    public void Load()
    {
        var path = "Assets/Art/Candidates/Kenney/PirateKit/Models/ship-medium.fbx";
        var model = Resources.Load<GameObject>(path);
    }
}
"""

RUNTIME_CANDIDATE_BACKSLASH_CS = """public class BackslashViolation
{
    private const string Path = "Assets\\Art\\Candidates\\UIMaterialsAudio\\UI\\icon.png";
}
"""

CONFIG_CANDIDATE_JSON = """{
  "name": "runtime_config",
  "path": "game/Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/cannon.fbx",
  "usage": "loading"
}
"""

REGISTRY_CANDIDATE_STATE_JSON = """{
  "content_id": "candidate_placeholder_rock_01",
  "asset_state": "Candidate",
  "resource_path": "game/Assets/Resources/Runtime/rock.prefab"
}
"""

REGISTRY_PLACEHOLDER_WHITELIST_JSON = """{
  "id": "plant_shallow_seaweed",
  "asset_state": "placeholder",
  "model_ref": "unity-primitive://vegetation/wetland/lod2",
  "resource_path": null
}
"""

REGISTRY_PLACEHOLDER_VIOLATION_JSON = """{
  "id": "plant_placeholder_01",
  "asset_state": "placeholder",
  "model_ref": "game/Assets/Prefabs/Formal/rock.prefab"
}
"""

REGISTRY_PLACEHOLDER_NAMED_REF_JSON = """{
  "id": "crew_placeholder_01",
  "asset_state": "placeholder",
  "model_ref": "game/Assets/Prefabs/Placeholder/ph_crew_male.prefab",
  "resource_path": "asset_ph_crew_male"
}
"""

TEMP_CACHE_CS = """public class TempCacheViolation
{
    private const string Cache = "C:/Users/Administrator/AppData/Local/WhalesIslandReef/D18/cache/kenney.zip";
}
"""

EXTERNAL_ARCHIVE_JSON = """{
  "archive": "artifacts/external_sources/pirate_kit.zip"
}
"""


class CandidateRuntimeGuardTests(unittest.TestCase):
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

    def _config(self) -> ScanConfig:
        return ScanConfig(
            project_root=self.root.resolve(),
            roots=[(self.root / item).resolve() for item in DEFAULT_SCAN_ROOTS],
            extensions={".cs", ".json"},
            exclude_dirs={"Candidates", "obj", ".git", "Library"},
        )

    def _write(self, relative: str, text: str) -> Path:
        path = self.root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")
        return path

    def _run(self, relative: str, text: str) -> dict:
        self._write(relative, text)
        return scan_project(self._config())

    def test_clean_runtime_passes(self):
        result = self._run("game/Assets/Scripts/Gameplay/CleanRuntime.cs", CLEAN_RUNTIME_CS)
        self._write("game/Assets/Content/D15/registry/clean.json", CLEAN_REGISTRY_JSON)
        result = scan_project(self._config())
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["runtime_findings_count"], 0)

    def test_comment_mention_is_informational(self):
        result = self._run("game/Assets/Scripts/Gameplay/CommentOnly.cs", COMMENT_CS)
        self.assertEqual(result["status"], "PASS")
        matches = [f for f in result["findings"] if f["rule"] == "candidate_path_reference"]
        self.assertEqual(len(matches), 1)
        self.assertEqual(matches[0]["classification"], "comment_or_documentation")
        self.assertEqual(matches[0]["severity"], "INFO")

    def test_editor_file_is_editor_only(self):
        result = self._run(
            "game/Assets/Scripts/Gameplay/Editor/CandidatePrefabBuilder.cs",
            EDITOR_BUILDER_CS,
        )
        self.assertEqual(result["status"], "PASS")
        matches = [f for f in result["findings"] if f["rule"] == "candidate_path_reference"]
        self.assertEqual(len(matches), 1)
        self.assertEqual(matches[0]["classification"], "editor_only")
        self.assertEqual(matches[0]["severity"], "INFO")

    def test_unity_editor_block_is_editor_only(self):
        result = self._run(
            "game/Assets/Scripts/Gameplay/EditorConditional.cs",
            UNITY_EDITOR_BLOCK_CS,
        )
        self.assertEqual(result["status"], "PASS")
        matches = [f for f in result["findings"] if f["rule"] == "candidate_path_reference"]
        self.assertEqual(len(matches), 1)
        self.assertEqual(matches[0]["classification"], "editor_only")

    def test_runtime_candidate_path_blocks(self):
        result = self._run("game/Assets/Scripts/Gameplay/RuntimeViolation.cs", RUNTIME_CANDIDATE_CS)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertEqual(result["runtime_findings_count"], 1)
        finding = result["findings"][0]
        self.assertEqual(finding["rule"], "candidate_path_reference")
        self.assertEqual(finding["classification"], "runtime")
        self.assertEqual(finding["severity"], "P0")
        self.assertIn("fix_direction", finding)

    def test_runtime_candidate_path_with_backslashes_blocks(self):
        result = self._run(
            "game/Assets/Scripts/Gameplay/BackslashViolation.cs",
            RUNTIME_CANDIDATE_BACKSLASH_CS,
        )
        self.assertEqual(result["status"], "BLOCKED")
        self.assertEqual(result["findings"][0]["rule"], "candidate_path_reference")

    def test_config_candidate_path_blocks(self):
        result = self._run("game/Assets/Settings/runtime_config.json", CONFIG_CANDIDATE_JSON)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertEqual(result["findings"][0]["rule"], "candidate_path_reference")

    def test_registry_candidate_state_blocks(self):
        result = self._run(
            "game/Assets/Content/Registry/candidate_state.json",
            REGISTRY_CANDIDATE_STATE_JSON,
        )
        self.assertEqual(result["status"], "BLOCKED")
        self.assertEqual(result["findings"][0]["rule"], "candidate_state_runtime_reference")
        self.assertEqual(result["findings"][0]["severity"], "P1")

    def test_registry_placeholder_refs_are_allowed(self):
        for name, text in (
            ("whitelist_primitive.json", REGISTRY_PLACEHOLDER_WHITELIST_JSON),
            ("whitelist_named.json", REGISTRY_PLACEHOLDER_NAMED_REF_JSON),
        ):
            with self.subTest(name=name):
                result = self._run(f"game/Assets/Content/Registry/{name}", text)
                self.assertEqual(result["status"], "PASS")
                self.assertEqual(result["findings_count"], 0)

    def test_registry_placeholder_with_formal_path_blocks(self):
        result = self._run(
            "game/Assets/Content/Registry/placeholder_violation.json",
            REGISTRY_PLACEHOLDER_VIOLATION_JSON,
        )
        self.assertEqual(result["status"], "BLOCKED")
        self.assertEqual(result["findings"][0]["rule"], "candidate_state_runtime_reference")
        self.assertEqual(result["findings"][0]["snippet"], "game/Assets/Prefabs/Formal/rock.prefab")

    def test_temp_cache_path_blocks(self):
        result = self._run("game/Assets/Scripts/Gameplay/TempCacheViolation.cs", TEMP_CACHE_CS)
        self.assertEqual(result["status"], "BLOCKED")
        rules = {f["rule"] for f in result["findings"]}
        self.assertIn("temp_cache_reference", rules)
        self.assertIn("external_archive_reference", rules)

    def test_external_archive_path_blocks(self):
        result = self._run("game/Assets/Settings/archive_config.json", EXTERNAL_ARCHIVE_JSON)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertEqual(result["findings"][0]["rule"], "external_archive_reference")

    def test_variable_named_candidates_is_not_a_false_positive(self):
        source = (
            "public class Probe\n{\n"
            "    private static readonly string[] GpuCounterCandidates =\n"
            "        { \"GPU\" };\n}\n"
        )
        result = self._run("game/Assets/Scripts/Systems/Probe.cs", source)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["findings_count"], 0)

    def test_tool_is_read_only(self):
        relative = "game/Assets/Scripts/Gameplay/RuntimeViolation.cs"
        path = self._write(relative, RUNTIME_CANDIDATE_CS)
        before = path.read_text(encoding="utf-8")
        self._run(relative, RUNTIME_CANDIDATE_CS)
        self.assertEqual(path.read_text(encoding="utf-8"), before)

    def test_finding_carries_line_number_and_snippet(self):
        result = self._run("game/Assets/Scripts/Gameplay/RuntimeViolation.cs", RUNTIME_CANDIDATE_CS)
        finding = result["findings"][0]
        self.assertGreaterEqual(finding["line"], 1)
        self.assertTrue(finding["snippet"])
        self.assertEqual(finding["file"], "game/Assets/Scripts/Gameplay/RuntimeViolation.cs")

    def test_multiline_block_comment_not_flagged_as_runtime(self):
        source = (
            "public class BlockComment\n{\n"
            "    /*\n"
            "     * Assets/Art/Candidates/Environment/Rock.fbx is only a candidate\n"
            "     * and must not be loaded at runtime.\n"
            "     */\n"
            "    private void Run() { }\n}\n"
        )
        result = self._run("game/Assets/Scripts/Gameplay/BlockComment.cs", source)
        self.assertEqual(result["status"], "PASS")
        matches = [f for f in result["findings"] if f["rule"] == "candidate_path_reference"]
        self.assertEqual(len(matches), 1)
        self.assertEqual(matches[0]["classification"], "comment_or_documentation")

    def test_result_json_serialisable(self):
        result = self._run("game/Assets/Scripts/Gameplay/RuntimeViolation.cs", RUNTIME_CANDIDATE_CS)
        json.dumps(result, ensure_ascii=False)


if __name__ == "__main__":
    unittest.main()
