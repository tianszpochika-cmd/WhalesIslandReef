// ============================================================
// D19LegacySaveTests —— 历史存档兼容与迁移测试（纯 C# EditMode）
//
// 归属：Whalebone.Systems.D19.Tests（D19-L39 唯一测试产物）
// 目标：针对历史旧存档夹具构建全覆盖测试：
//       1. 夹具 1：完全缺失 D19 块（null 存档输入）-> 确定默认值（CreateDefault 语义，OnShore 成员，SchemaVersion=1）；
//       2. 夹具 2：旧 D3/D9 键与别名（santa_margarita/building_saint_marys_dock）-> 规范键迁移并保留 MigrationHint；
//       3. 夹具 3：SchemaVersion=0 升级到 1，合法缺字段平滑填充；
//       4. 夹具 4：未来或不支持版本（SchemaVersion > 1 或 < 0）-> 明确拦截并返回 d19.save.unsupported_version；
//       5. 夹具 5：不可能状态/数据损失（处于 Berthed 却缺 BerthId/PortId）-> 明确拒绝并返回 d19.berth.occupied，绝不伪造泊位；
//       6. 夹具 6：跨港泊位冲突（泊位所属港与当前停靠港不一致）-> 明确拒绝并返回 d19.port.permission_denied；
//       7. 夹具 7：非法/不可能状态（负数世界日、重复成员 ID、重复泊位分配、占用泊位缺失船 ID、预留过期日无效）-> 全部明确拒绝；
//       8. 夹具 8：迁移幂等性（重复迁移结果完全一致，已是最新返回 AlreadyUpToDate，绝不产生二次状态突变）。
// ============================================================

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D19;

namespace Whalebone.Systems.D19.Tests
{
    [TestFixture]
    public sealed class D19LegacySaveTests
    {
        [Test]
        public void LegacyFixture1_MissingD19Block_ReturnsSafeDefaultProjection()
        {
            var res = D19PortSaveMigration.Migrate(null, "ship_legacy_01");
            Assert.That(res.Succeeded, Is.True);
            Assert.That(res.Status, Is.EqualTo(D19PortMigrationStatus.Migrated));
            Assert.That(res.MigratedProjection, Is.Not.Null);
            Assert.That(res.MigratedProjection.SchemaVersion, Is.EqualTo(D19PortSaveProjection.CurrentSchemaVersion));
            Assert.That(res.MigratedProjection.ShipId, Is.EqualTo("ship_legacy_01"));
            Assert.That(res.MigratedProjection.VisitState, Is.EqualTo("None"));
            Assert.That(res.MigrationFlags.Contains("source_null_created_default"), Is.True);
        }

        [Test]
        public void LegacyFixture2_DeprecatedAliases_MigratedToCanonicalWithHints()
        {
            var legacy = D19PortSaveProjection.CreateDefault("ship_legacy_02");
            legacy.PortId = "santa_margarita"; // D3 旧港口别名
            legacy.VisitState = "Docked";

            var res = D19PortSaveMigration.Migrate(legacy);
            Assert.That(res.Succeeded, Is.True);
            Assert.That(res.MigratedProjection.PortId, Is.EqualTo("port_santa_margarita"));
            Assert.That(res.MigrationFlags.Exists(f => f.StartsWith("migrated_port_alias:")), Is.True);
            Assert.That(res.MigrationFlags.Exists(f => f.StartsWith("port_migration_hint:")), Is.True);

            // 泊位旧别名测试
            var legacyBerth = D19PortSaveProjection.CreateDefault("ship_legacy_02");
            legacyBerth.PortId = "port_santa_margarita";
            legacyBerth.VisitState = "Berthed";
            legacyBerth.BerthId = "building_saint_marys_dock"; // D9 旧泊位别名
            legacyBerth.BerthAllocations.Add(new D19BerthAllocationSaveEntry
            {
                BerthId = "building_saint_marys_dock",
                State = "Occupied",
                ShipId = "ship_legacy_02"
            });

            var resBerth = D19PortSaveMigration.Migrate(legacyBerth);
            Assert.That(resBerth.Succeeded, Is.True);
            Assert.That(resBerth.MigratedProjection.BerthId, Is.EqualTo("berth_santa_margarita_dock_01"));
            Assert.That(resBerth.MigratedProjection.BerthAllocations[0].BerthId, Is.EqualTo("berth_santa_margarita_dock_01"));
        }

        [Test]
        public void LegacyFixture3_SchemaVersion0_UpgradedTo1WithDefaults()
        {
            var v0 = D19PortSaveProjection.CreateDefault("ship_legacy_03");
            v0.SchemaVersion = 0;
            v0.MemberLocations.Add(new D19MemberLocationSaveEntry("crew_01", "")); // 未知/缺省位置

            var res = D19PortSaveMigration.Migrate(v0);
            Assert.That(res.Succeeded, Is.True);
            Assert.That(res.SourceSchemaVersion, Is.EqualTo(0));
            Assert.That(res.TargetSchemaVersion, Is.EqualTo(1));
            Assert.That(res.MigratedProjection.SchemaVersion, Is.EqualTo(1));
            Assert.That(res.MigratedProjection.MemberLocations[0].Location, Is.EqualTo("OnShore"));
            Assert.That(res.MigrationFlags.Contains("upgraded_schema_version_0_to_1"), Is.True);
        }

        [Test]
        public void LegacyFixture4_FutureOrNegativeVersion_ExplicitlyRejected()
        {
            var future = D19PortSaveProjection.CreateDefault("ship_legacy_04");
            future.SchemaVersion = 2; // 未来版本

            var resFuture = D19PortSaveMigration.Migrate(future);
            Assert.That(resFuture.Succeeded, Is.False);
            Assert.That(resFuture.Status, Is.EqualTo(D19PortMigrationStatus.UnsupportedVersion));
            Assert.That(resFuture.ErrorCode, Is.EqualTo("d19.save.unsupported_version"));
            Assert.That(resFuture.LocKey, Is.EqualTo(D19PortLocalizationKeys.MigrationSaveUnsupported));

            var negative = D19PortSaveProjection.CreateDefault("ship_legacy_04");
            negative.SchemaVersion = -1; // 非法负数版本

            var resNeg = D19PortSaveMigration.Migrate(negative);
            Assert.That(resNeg.Succeeded, Is.False);
            Assert.That(resNeg.ErrorCode, Is.EqualTo("d19.save.unsupported_version"));
        }

        [Test]
        public void LegacyFixture5_DataLossInBerthedState_RejectedWithoutForgingBerth()
        {
            // 处于 Berthed 却缺失 BerthId
            var broken1 = D19PortSaveProjection.CreateDefault("ship_legacy_05");
            broken1.PortId = "port_santa_margarita";
            broken1.VisitState = "Berthed";
            broken1.BerthId = "";

            var res1 = D19PortSaveMigration.Migrate(broken1);
            Assert.That(res1.Succeeded, Is.False);
            Assert.That(res1.ErrorCode, Is.EqualTo("d19.berth.occupied"));
            Assert.That(res1.LocKey, Is.EqualTo(D19PortLocalizationKeys.FailureNotBerthed));
            Assert.That(res1.TechnicalReason, Is.EqualTo("missing_berth_in_berthed_state_data_loss"));

            // 处于 Berthed 却缺失 PortId
            var broken2 = D19PortSaveProjection.CreateDefault("ship_legacy_05");
            broken2.PortId = "";
            broken2.VisitState = "Berthed";
            broken2.BerthId = "berth_santa_margarita_dock_01";

            var res2 = D19PortSaveMigration.Migrate(broken2);
            Assert.That(res2.Succeeded, Is.False);
            Assert.That(res2.ErrorCode, Is.EqualTo("d19.port.unknown_port"));
        }

        [Test]
        public void LegacyFixture6_CrossPortBerthConflict_Rejected()
        {
            var cross = D19PortSaveProjection.CreateDefault("ship_legacy_06");
            cross.PortId = "port_green_cay";
            cross.VisitState = "Berthed";
            cross.BerthId = "berth_santa_margarita_dock_01"; // 属于 santa_margarita

            var res = D19PortSaveMigration.Migrate(cross);
            Assert.That(res.Succeeded, Is.False);
            Assert.That(res.ErrorCode, Is.EqualTo("d19.port.permission_denied"));
            Assert.That(res.TechnicalReason.StartsWith("cross_port_berth_conflict:"), Is.True);
        }

        [Test]
        public void LegacyFixture7_CorruptedImpossibleStates_Rejected()
        {
            // 负数世界日
            var negDay = D19PortSaveProjection.CreateDefault("ship_legacy_07");
            negDay.WorldDay = -5;
            var resDay = D19PortSaveMigration.Migrate(negDay);
            Assert.That(resDay.Succeeded, Is.False);
            Assert.That(resDay.TechnicalReason, Is.EqualTo("negative_world_day_impossible_state"));

            // 占用泊位缺少占用船 ID
            var missingOccupant = D19PortSaveProjection.CreateDefault("ship_legacy_07");
            missingOccupant.BerthAllocations.Add(new D19BerthAllocationSaveEntry
            {
                BerthId = "berth_santa_margarita_dock_01",
                State = "Occupied",
                ShipId = ""
            });
            var resOcc = D19PortSaveMigration.Migrate(missingOccupant);
            Assert.That(resOcc.Succeeded, Is.False);
            Assert.That(resOcc.TechnicalReason.StartsWith("occupied_or_reserved_berth_missing_ship_id:"), Is.True);

            // 预留泊位过期日早于等于预留日
            var badExpiry = D19PortSaveProjection.CreateDefault("ship_legacy_07");
            var badAlloc = new D19BerthAllocationSaveEntry
            {
                BerthId = "berth_santa_margarita_dock_01",
                State = "Reserved",
                ShipId = "ship_legacy_07",
                CommandId = "cmd_01",
                ReservedWorldDay = 10,
                ExpiresWorldDay = 10
            };
            badExpiry.BerthAllocations.Add(badAlloc);
            var resExp = D19PortSaveMigration.Migrate(badExpiry);
            Assert.That(resExp.Succeeded, Is.False);
            Assert.That(resExp.TechnicalReason.StartsWith("reserved_berth_invalid_expiry:"), Is.True);

            // 重复成员位置 ID
            var dupMember = D19PortSaveProjection.CreateDefault("ship_legacy_07");
            dupMember.MemberLocations.Add(new D19MemberLocationSaveEntry("crew_01", "OnShore"));
            dupMember.MemberLocations.Add(new D19MemberLocationSaveEntry("crew_01", "OnBoard"));
            var resDup = D19PortSaveMigration.Migrate(dupMember);
            Assert.That(resDup.Succeeded, Is.False);
            Assert.That(resDup.TechnicalReason.StartsWith("duplicate_member_id_in_locations:"), Is.True);
        }

        [Test]
        public void LegacyFixture8_MigrationIdempotencyAndNoSideEffects()
        {
            var v0 = D19PortSaveProjection.CreateDefault("ship_legacy_08");
            v0.SchemaVersion = 0;
            v0.PortId = "santa_margarita";

            // 第一次迁移
            var firstRun = D19PortSaveMigration.Migrate(v0);
            Assert.That(firstRun.Succeeded, Is.True);
            Assert.That(firstRun.Status, Is.EqualTo(D19PortMigrationStatus.Migrated));
            Assert.That(firstRun.MigratedProjection.SchemaVersion, Is.EqualTo(1));
            Assert.That(firstRun.MigratedProjection.PortId, Is.EqualTo("port_santa_margarita"));

            // 第二次迁移相同输出
            var secondRun = D19PortSaveMigration.Migrate(firstRun.MigratedProjection);
            Assert.That(secondRun.Succeeded, Is.True);
            Assert.That(secondRun.Status, Is.EqualTo(D19PortMigrationStatus.AlreadyUpToDate));
            Assert.That(secondRun.MigratedProjection.SchemaVersion, Is.EqualTo(1));
            Assert.That(secondRun.MigratedProjection.PortId, Is.EqualTo("port_santa_margarita"));
        }
    }
}
