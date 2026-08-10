import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
IDENTITY = ROOT / "game/Assets/Scripts/Systems/D10/D10PlayerIdentitySystem.cs"
SAVE = ROOT / "game/Assets/Scripts/Systems/Save/SaveSystem.cs"
UNITY_TEST = ROOT / "game/Assets/Tests/EditMode/D10PlayerIdentityTests.cs"
DOC = ROOT / "docs/game/design/common/D10_PLAYER_IDENTITY_EXPRESSION.md"


class D10PlayerIdentityStaticTests(unittest.TestCase):
    def test_identity_source_covers_creation_port_inspection_and_history(self):
        source = IDENTITY.read_text(encoding="utf-8")
        for token in (
            "GenderExpressionKey",
            "Name",
            "Title",
            "AgeBandKey",
            "CultureOriginKey",
            "IdentityHistory",
            "D10IdentityDocumentRecord",
            "InspectAtPort",
            "ApplyPortUpdate",
            "PreservedLegalHistoryRefs",
            "PreservedDebtHistoryRefs",
            "PreservedReputationHistoryRefs",
            "PreservedRelationHistoryRefs",
        ):
            self.assertIn(token, source)
        self.assertNotRegex(source, r"HttpClient|UnityWebRequest|LLMClient|Ollama|DeepSeek|System\.Net")

    def test_save_projection_and_tests_are_present(self):
        save = SAVE.read_text(encoding="utf-8")
        test = UNITY_TEST.read_text(encoding="utf-8")
        self.assertIn("SaveD10Identity", save)
        self.assertIn("LoadD10Identity", save)
        self.assertIn('["identity"]', save)
        self.assertIn("NewGame_PersistsExpressionNameTitleAgeCultureAndLongText", test)
        self.assertIn("PortInspectionAndUpdate_ChangesTitleAppearanceAndDocumentWithoutErasingHistory", test)
        self.assertIn("SaveD10Identity_RestoresHistoryAndKeepsDomainReferences", test)

    def test_document_freezes_game_only_boundary_and_non_destructive_changes(self):
        doc = DOC.read_text(encoding="utf-8")
        for token in (
            "Character/Identity",
            "GenderExpressionKey",
            "AgeBandKey",
            "CultureOriginKey",
            "PreservedHistory",
            "port_access_required",
            "unknown_appearance_loadout",
            "identityHistory",
            "location_json.identity",
        ):
            self.assertIn(token, doc)
        self.assertIn("不进入 server/web", doc)


if __name__ == "__main__":
    unittest.main()
