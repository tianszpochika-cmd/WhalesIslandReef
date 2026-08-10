using System;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D10;
using Whalebone.Systems.Save;

namespace D10.Tests
{
    public class D10PlayerIdentityTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d10_identity_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string path = _dbPath + suffix;
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        [Test]
        public void NewGame_PersistsExpressionNameTitleAgeCultureAndLongText()
        {
            var creation = new D10PlayerIdentityCreation
            {
                Name = "Marisol de la Cruz",
                GenderExpressionKey = "gender_expression_androgynous",
                AgeBandKey = "age_band_mature_adult",
                CultureOriginKey = "culture_origin_afro_caribbean",
                Title = new D10LocalizedText("title.chart_keeper", "海图保管人", "Chart Keeper"),
                IdentityNote = new D10LocalizedText("identity.note.long", new string('海', 4000), new string('s', 4000)),
            };
            creation.PronounKeys.Add("pronoun_key_they");

            SaveSystem.NewGame(new NewGameParams
            {
                CaptainName = "legacy-name-is-overridden-by-identity",
                Identity = creation,
            });

            var player = GameState.CurrentPlayer;
            Assert.AreEqual("Marisol de la Cruz", player.Name);
            Assert.AreEqual("Marisol de la Cruz", player.Identity.Name);
            Assert.AreEqual("gender_expression_androgynous", player.Identity.GenderExpressionKey);
            Assert.AreEqual("title.chart_keeper", player.Identity.Title.Key);
            Assert.AreEqual("age_band_mature_adult", player.Identity.AgeBandKey);
            Assert.AreEqual("culture_origin_afro_caribbean", player.Identity.CultureOriginKey);
            Assert.AreEqual(4000, player.Identity.IdentityNote.Zh.Length);
            Assert.AreEqual(1, player.Identity.IdentityHistory.Count);
            Assert.AreEqual("", player.Identity.IdentityHistory[0].PreviousName);
            Assert.AreEqual("Marisol de la Cruz", player.Identity.IdentityHistory[0].NewName);

            SaveSystem.Save();
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var restored = GameState.CurrentPlayer.Identity;
            Assert.AreEqual("Marisol de la Cruz", restored.Name);
            Assert.AreEqual("Chart Keeper", restored.Title.Resolve("en"));
            Assert.AreEqual(4000, restored.IdentityNote.En.Length);
            Assert.AreEqual("pronoun_key_they", restored.PronounKeys[0]);
        }

        [Test]
        public void PortInspectionAndUpdate_ChangesTitleAppearanceAndDocumentWithoutErasingHistory()
        {
            var preserved = new D10IdentityHistoryPreservationContext();
            preserved.LegalHistoryRefs.Add("legal:case:old_identity");
            preserved.DebtHistoryRefs.Add("debt:ledger:captain");
            preserved.ReputationHistoryRefs.Add("reputation:england:42");
            preserved.RelationHistoryRefs.Add("relation:crew:old_name");
            var state = D10PlayerIdentityState.CreateNew(
                "player:d10-test",
                D10PlayerIdentityCreation.Default("旧称谓船长"),
                2,
                preserved);
            var appearance = new D10AppearanceSystem(D10AppearanceCatalog.CreateStarterSnapshot());
            var identity = new D10PlayerIdentitySystem(state, new D10AppearanceOwnerWriter(appearance));

            var seaInspection = identity.InspectAtPort(false, "nearshore_saint_marys");
            Assert.IsFalse(seaInspection.CanEditTitle);
            Assert.IsFalse(seaInspection.CanEditAppearance);
            Assert.IsFalse(seaInspection.CanRecordDocument);

            var portInspection = identity.InspectAtPort(true, "santa_margarita");
            Assert.IsTrue(portInspection.CanEditTitle);
            Assert.IsTrue(portInspection.CanEditAppearance);
            Assert.AreEqual(1, portInspection.PreservedDebtHistoryCount);

            var result = identity.ApplyPortUpdate(new D10PortIdentityUpdateRequest
            {
                NewTitle = new D10LocalizedText("title/harbor_witness", "港口见证人", "Harbor Witness"),
                AppearanceLoadoutId = "outfit_hat_straw_deck",
                Document = new D10IdentityDocumentRecord
                {
                    DocumentTypeKey = "document.port_register",
                    IssuerRef = "port:santa_margarita",
                    RecordText = new D10LocalizedText("document.port_register.note", "港务登记副本", "Port registry copy"),
                },
                ChangeReasonKey = "identity.port_service",
                WorldDay = 7,
            }, true, "santa_margarita", new D10IdentityHistoryPreservationContext
            {
                LegalHistoryRefs = { "legal:case:new_alias" },
                DebtHistoryRefs = { "debt:ledger:captain" },
            });

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual("title/harbor_witness", result.Identity.Title.Key);
            Assert.AreEqual(1, result.Identity.Documents.Count);
            Assert.AreEqual("document.port_register", result.Identity.Documents[0].DocumentTypeKey);
            Assert.AreEqual("outfit_hat_straw_deck", appearance.Snapshot.ActiveLoadout.Get(D10AppearanceSlot.Hat));
            Assert.AreEqual(2, result.Identity.IdentityHistory.Count);
            Assert.AreEqual("title.captain", result.Identity.IdentityHistory[1].PreviousTitleKey);
            Assert.AreEqual("title/harbor_witness", result.Identity.IdentityHistory[1].NewTitleKey);
            Assert.Contains("legal:case:old_identity", result.Identity.PreservedHistory.LegalHistoryRefs);
            Assert.Contains("legal:case:new_alias", result.Identity.PreservedHistory.LegalHistoryRefs);
            Assert.Contains("debt:ledger:captain", result.Identity.PreservedHistory.DebtHistoryRefs);
            Assert.Contains("reputation:england:42", result.Identity.PreservedHistory.ReputationHistoryRefs);
            Assert.Contains("relation:crew:old_name", result.Identity.PreservedHistory.RelationHistoryRefs);
        }

        [Test]
        public void PortUpdate_RejectsSeaAccessUnknownAppearanceAndInvalidDocumentWithoutPartialWrite()
        {
            var appearance = new D10AppearanceSystem(D10AppearanceCatalog.CreateStarterSnapshot());
            var identity = new D10PlayerIdentitySystem(
                D10PlayerIdentityState.CreateDefault("player:reject", "拒绝测试"),
                new D10AppearanceOwnerWriter(appearance));
            var before = identity.Snapshot;

            var atSea = identity.ApplyPortUpdate(new D10PortIdentityUpdateRequest
            {
                NewTitle = new D10LocalizedText("title.no", "不应生效", "Must not apply"),
            }, false, "open_sea");
            Assert.IsFalse(atSea.Succeeded);
            Assert.AreEqual("title.captain", identity.Snapshot.Title.Key);

            var unknownAppearance = identity.ApplyPortUpdate(new D10PortIdentityUpdateRequest
            {
                NewTitle = new D10LocalizedText("title.no", "不应生效", "Must not apply"),
                AppearanceLoadoutId = "outfit:not_registered",
            }, true, "santa_margarita");
            Assert.IsFalse(unknownAppearance.Succeeded);
            Assert.AreEqual("title.captain", identity.Snapshot.Title.Key);

            var invalidDocument = identity.ApplyPortUpdate(new D10PortIdentityUpdateRequest
            {
                Document = new D10IdentityDocumentRecord { DocumentTypeKey = "" },
            }, true, "santa_margarita");
            Assert.IsFalse(invalidDocument.Succeeded);
            Assert.AreEqual(before.IdentityHistory.Count, identity.Snapshot.IdentityHistory.Count);
        }

        [Test]
        public void SaveD10Identity_RestoresHistoryAndKeepsDomainReferences()
        {
            var creation = D10PlayerIdentityCreation.Default("存档身份船长");
            creation.GenderExpressionKey = "gender_expression_feminine";
            creation.CultureOriginKey = "culture_origin_spanish_caribbean";
            SaveSystem.NewGame(new NewGameParams { CaptainName = creation.Name, Identity = creation });

            var preserved = new D10IdentityHistoryPreservationContext();
            preserved.LegalHistoryRefs.Add("legal:permit:01");
            preserved.DebtHistoryRefs.Add("debt:account:01");
            preserved.ReputationHistoryRefs.Add("reputation:faction:01");
            preserved.RelationHistoryRefs.Add("relation:crew:01");
            var identity = new D10PlayerIdentitySystem(GameState.CurrentPlayer.Identity);
            var result = identity.ApplyPortUpdate(new D10PortIdentityUpdateRequest
            {
                NewTitle = new D10LocalizedText("title:captain_navigator", "船长领航员", "Captain Navigator"),
                Document = new D10IdentityDocumentRecord { DocumentTypeKey = "document.harbor_license" },
                WorldDay = 11,
            }, true, "santa_margarita", preserved);
            Assert.IsTrue(result.Succeeded);

            SaveSystem.SaveD10Identity(result.Identity);
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var restored = SaveSystem.LoadD10Identity();
            Assert.AreEqual("title:captain_navigator", restored.Title.Key);
            Assert.AreEqual("gender_expression_feminine", restored.GenderExpressionKey);
            Assert.AreEqual(2, restored.IdentityHistory.Count);
            Assert.AreEqual(1, restored.Documents.Count);
            Assert.Contains("legal:permit:01", restored.PreservedHistory.LegalHistoryRefs);
            Assert.Contains("debt:account:01", restored.PreservedHistory.DebtHistoryRefs);
            Assert.Contains("reputation:faction:01", restored.PreservedHistory.ReputationHistoryRefs);
            Assert.Contains("relation:crew:01", restored.PreservedHistory.RelationHistoryRefs);
            Assert.AreEqual("存档身份船长", GameState.CurrentPlayer.Name);
        }
    }
}
