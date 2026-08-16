#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

namespace WhalesIslandReef.Editor.D18ArtPipeline.Tests
{
    public sealed class D18ArtPipelineEditModeTests
    {
        [Test]
        public void ClassificationCoversAllNineFrozenCategories()
        {
            Assert.AreEqual("Ships", D18ArtPipelineAudit.Classify("Assets/Art/Candidates/Kenney/Ships/ship_pirate_v01.fbx", "candidate_ship_visual"));
            Assert.AreEqual("Characters", D18ArtPipelineAudit.Classify("Assets/Art/Candidates/Kenney/Characters/character_a.fbx", "candidate_character_visual"));
            Assert.AreEqual("Animals", D18ArtPipelineAudit.Classify("Assets/Art/Candidates/Kenney/Animals/parrot.fbx", "candidate_animal_visual"));
            Assert.AreEqual("Environment", D18ArtPipelineAudit.Classify("Assets/Art/Candidates/Kenney/Environment/rock_a.fbx", "candidate_rock_visual"));
            Assert.AreEqual("Buildings", D18ArtPipelineAudit.Classify("Assets/Art/Candidates/Kenney/Buildings/dock_a.fbx", "candidate_dock_visual"));
            Assert.AreEqual("Props", D18ArtPipelineAudit.Classify("Assets/Art/Candidates/Kenney/Props/barrel_a.fbx", "candidate_prop_visual"));
            Assert.AreEqual("Vegetation", D18ArtPipelineAudit.Classify("Assets/Art/Candidates/Kenney/Vegetation/palm_a.fbx", "candidate_palm_visual"));
            Assert.AreEqual("UI", D18ArtPipelineAudit.Classify("Assets/Art/Candidates/Kenney/UI/icon_interact.png", "ui_icon"));
            Assert.AreEqual("Audio", D18ArtPipelineAudit.Classify("Assets/Art/Candidates/Kenney/Audio/rope.wav", "audio_sfx"));
        }

        [Test]
        public void ScanPolicyExcludesLibraryTempLogsAndExternalPaths()
        {
            Assert.IsFalse(D18ArtPipelineAudit.CanScanProjectPath("Library/anything.fbx"));
            Assert.IsFalse(D18ArtPipelineAudit.CanScanProjectPath("Temp/anything.fbx"));
            Assert.IsFalse(D18ArtPipelineAudit.CanScanProjectPath("Logs/anything.log"));
            Assert.IsFalse(D18ArtPipelineAudit.CanScanProjectPath("C:/outside/d18_asset.fbx"));
            Assert.IsFalse(D18ArtPipelineAudit.CanScanProjectPath("Assets/Editor/D18ArtPipeline/Tests/Fixtures/../outside.fbx"));
            Assert.IsFalse(D18ArtPipelineAudit.IsD18TestFixturePath("Assets/Editor/D18ArtPipeline/Tests/Fixtures/../outside.fbx"));
            Assert.IsTrue(D18ArtPipelineAudit.CanScanProjectPath("Assets/Editor/D18ArtPipeline/Tests/Fixtures/D18ArtPipelineFixture.json"));
        }

        [Test]
        public void AutomaticFixRequiresExplicitFixtureSelection()
        {
            var candidate = D18ArtPipelineAudit.ApplySuggestionsForFixture("Assets/Art/Candidates/Kenney/PirateKit/Models/ship-pirate-small.fbx", true);
            Assert.IsFalse(candidate.allowed);
            Assert.IsFalse(candidate.changed);

            var unselectedFixture = D18ArtPipelineAudit.ApplySuggestionsForFixture("Assets/Editor/D18ArtPipeline/Tests/Fixtures/D18ArtPipelineFixture.json", false);
            Assert.IsFalse(unselectedFixture.allowed);
            Assert.IsFalse(unselectedFixture.changed);

            var selectedFixture = D18ArtPipelineAudit.ApplySuggestionsForFixture("Assets/Editor/D18ArtPipeline/Tests/Fixtures/D18ArtPipelineFixture.json", true);
            Assert.IsTrue(selectedFixture.allowed);
            Assert.IsFalse(selectedFixture.changed);
        }

        [Test]
        public void PrefabPreflightReportsRequiredPresentationAndInteractionFields()
        {
            var root = new GameObject("test_fixture_ship_v01");
            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.name = "mesh_body";
            mesh.transform.SetParent(root.transform);
            var anchor = new GameObject("InteractionAnchor");
            anchor.transform.SetParent(root.transform);
            root.AddComponent<LODGroup>();
            try
            {
                var result = D18ArtPipelineAudit.AuditPrefabObject(root, "Assets/Editor/D18ArtPipeline/Tests/Fixtures/test_fixture_ship_v01.prefab");
                Assert.GreaterOrEqual(result.mesh_renderer_count, 1);
                Assert.GreaterOrEqual(result.collider_count, 1);
                Assert.GreaterOrEqual(result.interaction_anchor_count, 1);
                Assert.GreaterOrEqual(result.lod_group_count, 1);
                Assert.IsTrue(result.bounds_valid);
                Assert.IsTrue(result.naming_valid);
                Assert.IsFalse(System.Array.Exists(result.issues, issue => issue.code == "prefab_renderer_missing"));
                Assert.IsFalse(System.Array.Exists(result.issues, issue => issue.code == "prefab_material_missing"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ReportSerializationIsDeterministic()
        {
            var report = new D18AuditReport
            {
                schema_version = 1,
                task = "D18-A02",
                status = "PASS",
                audit_mode = "report_only",
                read_only = true,
                registry_modified = false,
                asset_state_modified = false,
                release_approved_asserted = false,
                downloaded_assets = false,
                scanned_roots = new[] { "Assets/Editor/D18ArtPipeline/Tests/Fixtures/" },
                manifests = new D18ManifestSummary[0],
                assets = new D18AssetAuditRecord[0],
                prefabs = new D18PrefabAuditRecord[0],
                category_counts = new D18CategoryCount[0],
            };
            var first = JsonUtility.ToJson(report, true);
            var second = JsonUtility.ToJson(report, true);
            Assert.AreEqual(first, second);
        }

        [Test]
        public void CandidateAuditIsDeterministicAcrossRepeatedRuns()
        {
            var manifestRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", "..", "docs/game/design/common/assets/manifests"));
            var manifests = System.IO.Directory.GetFiles(manifestRoot, "*.manifest.json", System.IO.SearchOption.TopDirectoryOnly);
            var first = D18ArtPipelineAudit.AuditManifestFiles(manifests);
            var second = D18ArtPipelineAudit.AuditManifestFiles(manifests);
            Assert.AreEqual(UnityEngine.JsonUtility.ToJson(first, true), UnityEngine.JsonUtility.ToJson(second, true));
        }
    }
}
#endif
