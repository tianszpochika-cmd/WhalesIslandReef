// ============================================================
// D21ShipSaveProjection —— D21-L19 船舶/航程 Save 投影
//
// Save projection 只保存 D21 自有语义：航程时钟、航向、锚泊状态、天气种子、
// 补给消耗账本和事件游标。D19/D3/D9/D11 等 owner 的事实只以
// owner + snapshot_id + schema_version + revision 引用保存，不复制货物、天气、
// 港口、船员或生态字段；SaveSystem 仍是根档序列化入口，不在此裁决数值。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.D11;
using Whalebone.Systems.D19;

namespace Whalebone.Systems.D21
{
    /// <summary>对上游公开版本化快照的不可变引用，不是快照内容副本。</summary>
    [Serializable]
    public sealed class D21OwnerSnapshotReference
    {
        public const int CurrentSchemaVersion = 1;

        public string OwnerSystem = string.Empty;
        public string SnapshotId = string.Empty;
        public int SnapshotSchemaVersion;
        public long Revision;

        public string ReferenceKey
        {
            get
            {
                return (OwnerSystem ?? string.Empty) + "|" + (SnapshotId ?? string.Empty) + "|" +
                    SnapshotSchemaVersion.ToString(CultureInfo.InvariantCulture) + "|" +
                    Revision.ToString(CultureInfo.InvariantCulture);
            }
        }

        public D21OwnerSnapshotReference Clone()
        {
            return new D21OwnerSnapshotReference
            {
                OwnerSystem = OwnerSystem ?? string.Empty,
                SnapshotId = SnapshotId ?? string.Empty,
                SnapshotSchemaVersion = SnapshotSchemaVersion,
                Revision = Revision,
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(OwnerSystem) || string.IsNullOrWhiteSpace(SnapshotId) ||
                SnapshotSchemaVersion <= 0 || Revision < 0)
                throw new InvalidOperationException("D21 owner snapshot reference is incomplete.");
        }

        public static D21OwnerSnapshotReference FromD19(D19PortSaveProjection projection)
        {
            if (projection == null) throw new InvalidOperationException("D19 port projection is required.");
            return new D21OwnerSnapshotReference
            {
                OwnerSystem = "D19PortSystem",
                SnapshotId = "port:" + (projection.PortId ?? string.Empty) + ":ship:" + (projection.ShipId ?? string.Empty),
                SnapshotSchemaVersion = projection.SchemaVersion,
                Revision = projection.SaveRevision,
            };
        }

        public static D21OwnerSnapshotReference FromD9(D9WeatherSeaSnapshot snapshot)
        {
            if (snapshot == null) throw new InvalidOperationException("D9 weather snapshot is required.");
            string id = string.IsNullOrEmpty(snapshot.LastEventId)
                ? "weather:" + snapshot.WorldDay.ToString(CultureInfo.InvariantCulture)
                : snapshot.LastEventId;
            return new D21OwnerSnapshotReference
            {
                OwnerSystem = "D9EnvironmentSystem",
                SnapshotId = id,
                SnapshotSchemaVersion = snapshot.SchemaVersion,
                Revision = Math.Max(0, snapshot.Revision),
            };
        }

        public static D21OwnerSnapshotReference FromD3(D3RuntimeSnapshot snapshot)
        {
            if (snapshot == null) throw new InvalidOperationException("D3 runtime snapshot is required.");
            return new D21OwnerSnapshotReference
            {
                OwnerSystem = "D3InventorySystem",
                SnapshotId = "d3:" + (snapshot.CurrentLocationId ?? string.Empty) + ":" +
                    (snapshot.DestinationId ?? string.Empty),
                // D3RuntimeSnapshot predates a dedicated schema constant; the public
                // runtime shape is currently the v1 compatibility shape.
                SnapshotSchemaVersion = 1,
                Revision = Math.Max(0L, snapshot.SaveRevision),
            };
        }

        public static D21OwnerSnapshotReference FromD11(D11WhaleDolphinRuntimeSnapshot snapshot)
        {
            if (snapshot == null) throw new InvalidOperationException("D11 ecology snapshot is required.");
            return new D21OwnerSnapshotReference
            {
                OwnerSystem = D11WhaleDolphinSchema.OwnerSystem,
                SnapshotId = string.IsNullOrEmpty(snapshot.LastEventId)
                    ? "ecology:" + snapshot.WorldDay.ToString(CultureInfo.InvariantCulture)
                    : snapshot.LastEventId,
                SnapshotSchemaVersion = snapshot.SchemaVersion,
                Revision = Math.Max(0L, snapshot.Revision),
            };
        }
    }

    /// <summary>D21 船侧锚泊/系泊投影；港口泊位事实仍由 D19 引用。</summary>
    [Serializable]
    public sealed class D21AnchorSaveProjection
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string AnchorState = "Released";
        public string MooringId = string.Empty;
        public string PortId = string.Empty;
        public string BerthId = string.Empty;
        public int LastTransitionTick;
        public bool IsAnchored;
        public bool IsMooringStable;
        public D21OwnerSnapshotReference PortSnapshot;

        public D21AnchorSaveProjection Clone()
        {
            return new D21AnchorSaveProjection
            {
                SchemaVersion = SchemaVersion,
                AnchorState = AnchorState ?? string.Empty,
                MooringId = MooringId ?? string.Empty,
                PortId = PortId ?? string.Empty,
                BerthId = BerthId ?? string.Empty,
                LastTransitionTick = LastTransitionTick,
                IsAnchored = IsAnchored,
                IsMooringStable = IsMooringStable,
                PortSnapshot = PortSnapshot == null ? null : PortSnapshot.Clone(),
            };
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || string.IsNullOrWhiteSpace(AnchorState) ||
                LastTransitionTick < 0)
                throw new InvalidOperationException("D21 anchor projection is outside its saved range.");
            if (IsMooringStable && string.IsNullOrWhiteSpace(MooringId))
                throw new InvalidOperationException("D21 stable mooring requires mooring_id.");
            if (PortSnapshot != null) PortSnapshot.Validate();
        }
    }

    /// <summary>D21 航程/航线投影；路线内容只通过稳定 ID 和版本引用保存。</summary>
    [Serializable]
    public sealed class D21VoyageSaveProjection
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public string RouteId = string.Empty;
        public string OriginId = string.Empty;
        public string DestinationId = string.Empty;
        public string CurrentWaypointId = string.Empty;
        public string Status = "Idle";
        public int LastTickIndex;
        public D21VoyageTimeState Time;
        public D21CourseState Course;
        public List<D21VoyageRiskSeed> RiskSeeds = new List<D21VoyageRiskSeed>();

        public D21VoyageSaveProjection Clone()
        {
            var result = new D21VoyageSaveProjection
            {
                SchemaVersion = SchemaVersion,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                RouteId = RouteId ?? string.Empty,
                OriginId = OriginId ?? string.Empty,
                DestinationId = DestinationId ?? string.Empty,
                CurrentWaypointId = CurrentWaypointId ?? string.Empty,
                Status = Status ?? string.Empty,
                LastTickIndex = LastTickIndex,
                Time = Time == null ? null : Time.Clone(),
                Course = Course == null ? null : Course.Clone(),
            };
            if (RiskSeeds != null)
                foreach (D21VoyageRiskSeed seed in RiskSeeds)
                    if (seed != null) result.RiskSeeds.Add(seed.Clone());
            return result;
        }

        public void Validate(string rootShipId = "")
        {
            if (SchemaVersion != CurrentSchemaVersion || string.IsNullOrWhiteSpace(Status) || LastTickIndex < 0)
                throw new InvalidOperationException("D21 voyage projection is outside its saved range.");
            if (!string.IsNullOrEmpty(rootShipId) && !string.IsNullOrEmpty(ShipId) &&
                !string.Equals(rootShipId, ShipId, StringComparison.Ordinal))
                throw new InvalidOperationException("D21 voyage projection ship_id does not match root ship.");
            if (Time != null)
            {
                Time.Validate();
                if (!string.IsNullOrEmpty(VoyageId) && !string.IsNullOrEmpty(Time.VoyageId) &&
                    !string.Equals(VoyageId, Time.VoyageId, StringComparison.Ordinal))
                    throw new InvalidOperationException("D21 voyage time voyage_id does not match projection.");
                if (!string.IsNullOrEmpty(ShipId) && !string.IsNullOrEmpty(Time.ShipId) &&
                    !string.Equals(ShipId, Time.ShipId, StringComparison.Ordinal))
                    throw new InvalidOperationException("D21 voyage time ship_id does not match projection.");
            }
            if (Course != null) Course.Validate();
            if (RiskSeeds == null) throw new InvalidOperationException("D21 voyage risk seeds are required.");
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (D21VoyageRiskSeed seed in RiskSeeds)
            {
                if (seed == null || seed.RiskType == D21VoyageRiskType.None || seed.Seed < 0 ||
                    seed.RollPermille < 0 || seed.RollPermille > 999 ||
                    !keys.Add(seed.RiskType.ToString()))
                    throw new InvalidOperationException("D21 voyage risk seed projection is invalid.");
            }
        }
    }

    /// <summary>天气 owner 的版本化引用与 D21 航程种子；不复制风/能见度等天气事实。</summary>
    [Serializable]
    public sealed class D21WeatherSeedSaveProjection
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public long VoyageSeed;
        public long WeatherSeed;
        public string WeatherSourceId = string.Empty;
        public int WeatherSnapshotSchemaVersion;
        public long WeatherSnapshotRevision;
        public List<string> AppliedWeatherEventIds = new List<string>();

        public D21WeatherSeedSaveProjection Clone()
        {
            var result = new D21WeatherSeedSaveProjection
            {
                SchemaVersion = SchemaVersion,
                VoyageSeed = VoyageSeed,
                WeatherSeed = WeatherSeed,
                WeatherSourceId = WeatherSourceId ?? string.Empty,
                WeatherSnapshotSchemaVersion = WeatherSnapshotSchemaVersion,
                WeatherSnapshotRevision = WeatherSnapshotRevision,
            };
            if (AppliedWeatherEventIds != null) result.AppliedWeatherEventIds.AddRange(AppliedWeatherEventIds);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || VoyageSeed < 0 || WeatherSeed < 0 ||
                WeatherSnapshotSchemaVersion <= 0 || WeatherSnapshotRevision < 0 ||
                string.IsNullOrWhiteSpace(WeatherSourceId) || AppliedWeatherEventIds == null)
                throw new InvalidOperationException("D21 weather seed projection is incomplete.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in AppliedWeatherEventIds)
                if (string.IsNullOrWhiteSpace(id) || !ids.Add(id))
                    throw new InvalidOperationException("D21 weather event seed list contains a duplicate or empty id.");
        }
    }

    /// <summary>D21 航程补给消耗账本；库存余额由 D3/Cargo owner 保存。</summary>
    [Serializable]
    public sealed class D21ProvisionLedgerSaveProjection
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public long LedgerRevision;
        public long ConsumedWaterUnits;
        public long ConsumedFoodUnits;
        public int LastAppliedTick;
        public string LastEventId = string.Empty;
        public D21OwnerSnapshotReference InventorySnapshot;

        public D21ProvisionLedgerSaveProjection Clone()
        {
            return new D21ProvisionLedgerSaveProjection
            {
                SchemaVersion = SchemaVersion,
                LedgerRevision = LedgerRevision,
                ConsumedWaterUnits = ConsumedWaterUnits,
                ConsumedFoodUnits = ConsumedFoodUnits,
                LastAppliedTick = LastAppliedTick,
                LastEventId = LastEventId ?? string.Empty,
                InventorySnapshot = InventorySnapshot == null ? null : InventorySnapshot.Clone(),
            };
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || LedgerRevision < 0 ||
                ConsumedWaterUnits < 0 || ConsumedFoodUnits < 0 || LastAppliedTick < 0)
                throw new InvalidOperationException("D21 provision ledger is outside its saved range.");
            if (InventorySnapshot != null) InventorySnapshot.Validate();
            if (ConsumedWaterUnits == 0 && ConsumedFoodUnits == 0 && !string.IsNullOrEmpty(LastEventId))
                throw new InvalidOperationException("D21 empty provision ledger cannot carry a consumption event.");
        }
    }

    /// <summary>D21 事件游标、幂等键和完整事件 log 的 Save 投影。</summary>
    [Serializable]
    public sealed class D21EventCursorSaveProjection
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string LastEventId = string.Empty;
        public long LastEventSequence;
        public long LastEventSaveRevision;
        public List<string> AppliedEventKeys = new List<string>();
        public D21VoyageEventLogSnapshot EventLog = new D21VoyageEventLogSnapshot();

        public D21EventCursorSaveProjection Clone()
        {
            var result = new D21EventCursorSaveProjection
            {
                SchemaVersion = SchemaVersion,
                LastEventId = LastEventId ?? string.Empty,
                LastEventSequence = LastEventSequence,
                LastEventSaveRevision = LastEventSaveRevision,
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || LastEventSequence < 0 || LastEventSaveRevision < 0 ||
                AppliedEventKeys == null || EventLog == null)
                throw new InvalidOperationException("D21 event cursor projection is incomplete.");
            EventLog.Validate();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (string key in AppliedEventKeys)
                if (string.IsNullOrWhiteSpace(key) || !keys.Add(key))
                    throw new InvalidOperationException("D21 applied event keys contain a duplicate or empty key.");
            if (EventLog.Events.Count == 0)
            {
                if (LastEventSequence != 0 || !string.IsNullOrEmpty(LastEventId))
                    throw new InvalidOperationException("D21 empty event log has a non-empty cursor.");
            }
            else
            {
                D21VoyageEvent last = EventLog.Events[EventLog.Events.Count - 1];
                if (LastEventSequence != last.LogSequence || !string.Equals(LastEventId, last.EventId, StringComparison.Ordinal) ||
                    LastEventSaveRevision != last.SaveRevision)
                    throw new InvalidOperationException("D21 event cursor does not point to the last event.");
            }
        }
    }

    /// <summary>D21 船舶领域根 Save projection。</summary>
    [Serializable]
    public sealed class D21ShipSaveProjection
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string ShipId = string.Empty;
        public int WorldDay;
        public long SaveRevision;
        public D21OwnerSnapshotReference ShipSnapshot;
        public D21AnchorSaveProjection Anchor = new D21AnchorSaveProjection();
        public D21VoyageSaveProjection Voyage = new D21VoyageSaveProjection();
        public D21WeatherSeedSaveProjection WeatherSeed = new D21WeatherSeedSaveProjection();
        public D21ProvisionLedgerSaveProjection Provisions = new D21ProvisionLedgerSaveProjection();
        public D21EventCursorSaveProjection Events = new D21EventCursorSaveProjection();
        public List<D21OwnerSnapshotReference> UpstreamSnapshots = new List<D21OwnerSnapshotReference>();

        public static D21ShipSaveProjection CreateDefault(string shipId)
        {
            string id = shipId ?? string.Empty;
            var result = new D21ShipSaveProjection
            {
                ShipId = id,
                ShipSnapshot = new D21OwnerSnapshotReference
                {
                    OwnerSystem = "D21ShipSystem",
                    SnapshotId = "ship:" + id,
                    SnapshotSchemaVersion = 1,
                    Revision = 0,
                },
                Voyage = new D21VoyageSaveProjection
                {
                    ShipId = id,
                    Status = "Idle",
                    Time = new D21VoyageTimeState
                    {
                        ShipId = id,
                        IsArrived = true,
                        CurrentPhase = D21DayNightPhase.Night,
                    },
                },
                WeatherSeed = new D21WeatherSeedSaveProjection
                {
                    VoyageSeed = 0,
                    WeatherSeed = 0,
                    WeatherSourceId = "d9:default",
                    WeatherSnapshotSchemaVersion = 1,
                    WeatherSnapshotRevision = 0,
                },
            };
            result.Validate();
            return result;
        }

        public static D21ShipSaveProjection Build(
            string shipId,
            int worldDay,
            long saveRevision,
            D21OwnerSnapshotReference shipSnapshot,
            D21AnchorSaveProjection anchor,
            D21VoyageSaveProjection voyage,
            D21WeatherSeedSaveProjection weatherSeed,
            D21ProvisionLedgerSaveProjection provisions,
            D21EventCursorSaveProjection events,
            IEnumerable<D21OwnerSnapshotReference> upstreamSnapshots = null)
        {
            var result = new D21ShipSaveProjection
            {
                ShipId = shipId ?? string.Empty,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
                ShipSnapshot = shipSnapshot == null ? null : shipSnapshot.Clone(),
                Anchor = anchor == null ? new D21AnchorSaveProjection() : anchor.Clone(),
                Voyage = voyage == null ? new D21VoyageSaveProjection() : voyage.Clone(),
                WeatherSeed = weatherSeed == null ? new D21WeatherSeedSaveProjection() : weatherSeed.Clone(),
                Provisions = provisions == null ? new D21ProvisionLedgerSaveProjection() : provisions.Clone(),
                Events = events == null ? new D21EventCursorSaveProjection() : events.Clone(),
            };
            if (upstreamSnapshots != null)
                foreach (D21OwnerSnapshotReference reference in upstreamSnapshots)
                    if (reference != null) result.UpstreamSnapshots.Add(reference.Clone());
            result.Validate();
            return result;
        }

        public D21ShipSaveProjection Clone()
        {
            var result = new D21ShipSaveProjection
            {
                SchemaVersion = SchemaVersion,
                ShipId = ShipId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                ShipSnapshot = ShipSnapshot == null ? null : ShipSnapshot.Clone(),
                Anchor = Anchor == null ? null : Anchor.Clone(),
                Voyage = Voyage == null ? null : Voyage.Clone(),
                WeatherSeed = WeatherSeed == null ? null : WeatherSeed.Clone(),
                Provisions = Provisions == null ? null : Provisions.Clone(),
                Events = Events == null ? null : Events.Clone(),
            };
            if (UpstreamSnapshots != null)
                foreach (D21OwnerSnapshotReference reference in UpstreamSnapshots)
                    if (reference != null) result.UpstreamSnapshots.Add(reference.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || string.IsNullOrWhiteSpace(ShipId) ||
                WorldDay < 0 || SaveRevision < 0 || ShipSnapshot == null || Anchor == null || Voyage == null ||
                WeatherSeed == null || Provisions == null || Events == null || UpstreamSnapshots == null)
                throw new InvalidOperationException("D21 ship save projection is incomplete.");
            ShipSnapshot.Validate();
            Anchor.Validate();
            Voyage.Validate(ShipId);
            WeatherSeed.Validate();
            Provisions.Validate();
            Events.Validate();
            var references = new HashSet<string>(StringComparer.Ordinal) { ShipSnapshot.ReferenceKey };
            foreach (D21OwnerSnapshotReference reference in UpstreamSnapshots)
            {
                if (reference == null) throw new InvalidOperationException("D21 upstream snapshot reference cannot be null.");
                reference.Validate();
                if (!references.Add(reference.ReferenceKey))
                    throw new InvalidOperationException("D21 upstream snapshot references contain a duplicate key.");
            }
        }

        public string ComputeFingerprint()
        {
            Validate();
            return "d21save_v" + SchemaVersion.ToString(CultureInfo.InvariantCulture) + "_" +
                ShipId + "_d" + WorldDay.ToString(CultureInfo.InvariantCulture) + "_r" +
                SaveRevision.ToString(CultureInfo.InvariantCulture) + "_t" +
                Voyage.LastTickIndex.ToString(CultureInfo.InvariantCulture) + "_e" +
                Events.LastEventSequence.ToString(CultureInfo.InvariantCulture) + "_" +
                (Events.LastEventId ?? string.Empty);
        }

        public string ToJson()
        {
            return D21ShipSaveProjectionSerializer.Serialize(this);
        }

        public static D21ShipSaveProjection FromJson(string json)
        {
            return D21ShipSaveProjectionSerializer.Deserialize(json);
        }
    }

    /// <summary>Save projection 建造别名，保持 owner 与根 Save 解耦。</summary>
    public static class D21ShipSaveProjectionMapper
    {
        public static D21ShipSaveProjection BuildProjection(
            string shipId,
            int worldDay,
            long saveRevision,
            D21OwnerSnapshotReference shipSnapshot,
            D21AnchorSaveProjection anchor,
            D21VoyageSaveProjection voyage,
            D21WeatherSeedSaveProjection weatherSeed,
            D21ProvisionLedgerSaveProjection provisions,
            D21EventCursorSaveProjection events,
            IEnumerable<D21OwnerSnapshotReference> upstreamSnapshots = null)
        {
            return D21ShipSaveProjection.Build(shipId, worldDay, saveRevision, shipSnapshot, anchor,
                voyage, weatherSeed, provisions, events, upstreamSnapshots);
        }
    }

    /// <summary>D21 Save projection 的稳定 JSON 编解码器。</summary>
    public static class D21ShipSaveProjectionSerializer
    {
        public static string Serialize(D21ShipSaveProjection projection)
        {
            if (projection == null) throw new InvalidOperationException("D21 ship save projection is required.");
            projection.Validate();
            var sb = new StringBuilder(2048);
            sb.Append('{');
            Field(sb, "schema_version", projection.SchemaVersion); sb.Append(',');
            Field(sb, "ship_id", projection.ShipId); sb.Append(',');
            Field(sb, "world_day", projection.WorldDay); sb.Append(',');
            Field(sb, "save_revision", projection.SaveRevision); sb.Append(',');
            sb.Append("\"ship_snapshot\":"); WriteReference(sb, projection.ShipSnapshot); sb.Append(',');
            sb.Append("\"anchor\":"); WriteAnchor(sb, projection.Anchor); sb.Append(',');
            sb.Append("\"voyage\":"); WriteVoyage(sb, projection.Voyage); sb.Append(',');
            sb.Append("\"weather_seed\":"); WriteWeatherSeed(sb, projection.WeatherSeed); sb.Append(',');
            sb.Append("\"provisions\":"); WriteProvisions(sb, projection.Provisions); sb.Append(',');
            sb.Append("\"events\":"); WriteEvents(sb, projection.Events); sb.Append(',');
            sb.Append("\"upstream_snapshots\":"); WriteReferences(sb, projection.UpstreamSnapshots);
            sb.Append('}');
            return sb.ToString();
        }

        public static D21ShipSaveProjection Deserialize(string json)
        {
            try
            {
                Dictionary<string, object> root = D21CanonicalJson.ParseObject(json);
                var projection = new D21ShipSaveProjection
                {
                    SchemaVersion = D21CanonicalJson.GetInt(root, "schema_version"),
                    ShipId = D21CanonicalJson.GetString(root, "ship_id"),
                    WorldDay = D21CanonicalJson.GetInt(root, "world_day"),
                    SaveRevision = D21CanonicalJson.GetLong(root, "save_revision"),
                    ShipSnapshot = ReadReference(D21CanonicalJson.GetObject(root, "ship_snapshot")),
                    Anchor = ReadAnchor(D21CanonicalJson.GetObject(root, "anchor")),
                    Voyage = ReadVoyage(D21CanonicalJson.GetObject(root, "voyage")),
                    WeatherSeed = ReadWeatherSeed(D21CanonicalJson.GetObject(root, "weather_seed")),
                    Provisions = ReadProvisions(D21CanonicalJson.GetObject(root, "provisions")),
                    Events = ReadEvents(D21CanonicalJson.GetObject(root, "events")),
                    UpstreamSnapshots = ReadReferences(root, "upstream_snapshots"),
                };
                projection.Validate();
                return projection;
            }
            catch (Exception ex)
            {
                throw new D21EventSerializationException("D21 ship save projection JSON is invalid: " + ex.Message, ex);
            }
        }

        private static void WriteReference(StringBuilder sb, D21OwnerSnapshotReference reference)
        {
            if (reference == null) throw new InvalidOperationException("D21 snapshot reference is required.");
            reference.Validate();
            sb.Append('{');
            Field(sb, "owner_system", reference.OwnerSystem); sb.Append(',');
            Field(sb, "snapshot_id", reference.SnapshotId); sb.Append(',');
            Field(sb, "snapshot_schema_version", reference.SnapshotSchemaVersion); sb.Append(',');
            Field(sb, "revision", reference.Revision);
            sb.Append('}');
        }

        private static D21OwnerSnapshotReference ReadReference(Dictionary<string, object> root)
        {
            var result = new D21OwnerSnapshotReference
            {
                OwnerSystem = D21CanonicalJson.GetString(root, "owner_system"),
                SnapshotId = D21CanonicalJson.GetString(root, "snapshot_id"),
                SnapshotSchemaVersion = D21CanonicalJson.GetInt(root, "snapshot_schema_version"),
                Revision = D21CanonicalJson.GetLong(root, "revision"),
            };
            result.Validate();
            return result;
        }

        private static void WriteAnchor(StringBuilder sb, D21AnchorSaveProjection anchor)
        {
            anchor.Validate();
            sb.Append('{');
            Field(sb, "schema_version", anchor.SchemaVersion); sb.Append(',');
            Field(sb, "anchor_state", anchor.AnchorState); sb.Append(',');
            Field(sb, "mooring_id", anchor.MooringId); sb.Append(',');
            Field(sb, "port_id", anchor.PortId); sb.Append(',');
            Field(sb, "berth_id", anchor.BerthId); sb.Append(',');
            Field(sb, "last_transition_tick", anchor.LastTransitionTick); sb.Append(',');
            Field(sb, "is_anchored", anchor.IsAnchored); sb.Append(',');
            Field(sb, "is_mooring_stable", anchor.IsMooringStable); sb.Append(',');
            sb.Append("\"port_snapshot\":");
            if (anchor.PortSnapshot == null) sb.Append("null"); else WriteReference(sb, anchor.PortSnapshot);
            sb.Append('}');
        }

        private static D21AnchorSaveProjection ReadAnchor(Dictionary<string, object> root)
        {
            var result = new D21AnchorSaveProjection
            {
                SchemaVersion = D21CanonicalJson.GetInt(root, "schema_version"),
                AnchorState = D21CanonicalJson.GetString(root, "anchor_state"),
                MooringId = D21CanonicalJson.GetString(root, "mooring_id"),
                PortId = D21CanonicalJson.GetString(root, "port_id"),
                BerthId = D21CanonicalJson.GetString(root, "berth_id"),
                LastTransitionTick = D21CanonicalJson.GetInt(root, "last_transition_tick"),
                IsAnchored = D21CanonicalJson.GetBool(root, "is_anchored"),
                IsMooringStable = D21CanonicalJson.GetBool(root, "is_mooring_stable"),
            };
            object snapshot;
            if (root.TryGetValue("port_snapshot", out snapshot) && snapshot != null)
                result.PortSnapshot = ReadReference(snapshot as Dictionary<string, object>);
            result.Validate();
            return result;
        }

        private static void WriteVoyage(StringBuilder sb, D21VoyageSaveProjection voyage)
        {
            voyage.Validate();
            sb.Append('{');
            Field(sb, "schema_version", voyage.SchemaVersion); sb.Append(',');
            Field(sb, "voyage_id", voyage.VoyageId); sb.Append(',');
            Field(sb, "ship_id", voyage.ShipId); sb.Append(',');
            Field(sb, "route_id", voyage.RouteId); sb.Append(',');
            Field(sb, "origin_id", voyage.OriginId); sb.Append(',');
            Field(sb, "destination_id", voyage.DestinationId); sb.Append(',');
            Field(sb, "current_waypoint_id", voyage.CurrentWaypointId); sb.Append(',');
            Field(sb, "status", voyage.Status); sb.Append(',');
            Field(sb, "last_tick_index", voyage.LastTickIndex); sb.Append(',');
            sb.Append("\"time\":");
            if (voyage.Time == null) sb.Append("null"); else WriteTime(sb, voyage.Time);
            sb.Append(',');
            sb.Append("\"course\":");
            if (voyage.Course == null) sb.Append("null"); else WriteCourse(sb, voyage.Course);
            sb.Append(',');
            sb.Append("\"risk_seeds\":"); WriteRiskSeeds(sb, voyage.RiskSeeds);
            sb.Append('}');
        }

        private static D21VoyageSaveProjection ReadVoyage(Dictionary<string, object> root)
        {
            var result = new D21VoyageSaveProjection
            {
                SchemaVersion = D21CanonicalJson.GetInt(root, "schema_version"),
                VoyageId = D21CanonicalJson.GetString(root, "voyage_id"),
                ShipId = D21CanonicalJson.GetString(root, "ship_id"),
                RouteId = D21CanonicalJson.GetString(root, "route_id"),
                OriginId = D21CanonicalJson.GetString(root, "origin_id"),
                DestinationId = D21CanonicalJson.GetString(root, "destination_id"),
                CurrentWaypointId = D21CanonicalJson.GetString(root, "current_waypoint_id"),
                Status = D21CanonicalJson.GetString(root, "status"),
                LastTickIndex = D21CanonicalJson.GetInt(root, "last_tick_index"),
                RiskSeeds = ReadRiskSeeds(root, "risk_seeds"),
            };
            object value;
            if (root.TryGetValue("time", out value) && value != null)
                result.Time = ReadTime(value as Dictionary<string, object>);
            if (root.TryGetValue("course", out value) && value != null)
                result.Course = ReadCourse(value as Dictionary<string, object>);
            return result;
        }

        private static void WriteTime(StringBuilder sb, D21VoyageTimeState time)
        {
            time.Validate();
            sb.Append('{');
            Field(sb, "schema_version", time.SchemaVersion); sb.Append(',');
            Field(sb, "voyage_id", time.VoyageId); sb.Append(',');
            Field(sb, "ship_id", time.ShipId); sb.Append(',');
            Field(sb, "route_distance_milli_nm", time.RouteDistanceMilliNm); sb.Append(',');
            Field(sb, "distance_travelled_milli_nm", time.DistanceTravelledMilliNm); sb.Append(',');
            Field(sb, "distance_remainder_numerator", time.DistanceRemainderNumerator); sb.Append(',');
            Field(sb, "elapsed_ticks", time.ElapsedTicks); sb.Append(',');
            Field(sb, "start_world_day", time.StartWorldDay); sb.Append(',');
            Field(sb, "start_minute_of_day", time.StartMinuteOfDay); sb.Append(',');
            Field(sb, "last_save_revision", time.LastSaveRevision); sb.Append(',');
            Field(sb, "is_arrived", time.IsArrived); sb.Append(',');
            Field(sb, "current_phase", time.CurrentPhase.ToString());
            sb.Append('}');
        }

        private static D21VoyageTimeState ReadTime(Dictionary<string, object> root)
        {
            D21DayNightPhase phase = ParseEnum<D21DayNightPhase>(D21CanonicalJson.GetString(root, "current_phase"));
            var result = new D21VoyageTimeState
            {
                SchemaVersion = D21CanonicalJson.GetInt(root, "schema_version"),
                VoyageId = D21CanonicalJson.GetString(root, "voyage_id"),
                ShipId = D21CanonicalJson.GetString(root, "ship_id"),
                RouteDistanceMilliNm = D21CanonicalJson.GetLong(root, "route_distance_milli_nm"),
                DistanceTravelledMilliNm = D21CanonicalJson.GetLong(root, "distance_travelled_milli_nm"),
                DistanceRemainderNumerator = D21CanonicalJson.GetLong(root, "distance_remainder_numerator"),
                ElapsedTicks = D21CanonicalJson.GetInt(root, "elapsed_ticks"),
                StartWorldDay = D21CanonicalJson.GetInt(root, "start_world_day"),
                StartMinuteOfDay = D21CanonicalJson.GetInt(root, "start_minute_of_day"),
                LastSaveRevision = D21CanonicalJson.GetLong(root, "last_save_revision"),
                IsArrived = D21CanonicalJson.GetBool(root, "is_arrived"),
                CurrentPhase = phase,
            };
            result.Validate();
            return result;
        }

        private static void WriteCourse(StringBuilder sb, D21CourseState course)
        {
            course.Validate();
            sb.Append('{');
            Field(sb, "schema_version", course.SchemaVersion); sb.Append(',');
            Field(sb, "voyage_id", course.VoyageId); sb.Append(',');
            Field(sb, "ship_id", course.ShipId); sb.Append(',');
            Field(sb, "mode", course.Mode.ToString()); sb.Append(',');
            Field(sb, "has_course", course.HasCourse); sb.Append(',');
            Field(sb, "is_stopped", course.IsStopped); sb.Append(',');
            Field(sb, "heading_centidegrees", course.HeadingCentidegrees); sb.Append(',');
            Field(sb, "speed_permille", course.SpeedPermille); sb.Append(',');
            Field(sb, "last_accepted_tick", course.LastAcceptedTick); sb.Append(',');
            Field(sb, "cooldown_until_tick", course.CooldownUntilTick); sb.Append(',');
            Field(sb, "last_command_id", course.LastCommandId); sb.Append(',');
            sb.Append("\"applied_commands\":");
            sb.Append('[');
            for (int i = 0; i < course.AppliedCommands.Count; i++)
            {
                if (i > 0) sb.Append(',');
                D21CourseCommandReceipt receipt = course.AppliedCommands[i];
                sb.Append('{');
                Field(sb, "command_id", receipt.CommandId); sb.Append(',');
                Field(sb, "fingerprint", receipt.Fingerprint); sb.Append(',');
                Field(sb, "accepted_tick", receipt.AcceptedTick); sb.Append(',');
                Field(sb, "result_code", receipt.ResultCode);
                sb.Append('}');
            }
            sb.Append(']');
            sb.Append('}');
        }

        private static D21CourseState ReadCourse(Dictionary<string, object> root)
        {
            var result = new D21CourseState
            {
                SchemaVersion = D21CanonicalJson.GetInt(root, "schema_version"),
                VoyageId = D21CanonicalJson.GetString(root, "voyage_id"),
                ShipId = D21CanonicalJson.GetString(root, "ship_id"),
                Mode = ParseEnum<D21CourseMode>(D21CanonicalJson.GetString(root, "mode")),
                HasCourse = D21CanonicalJson.GetBool(root, "has_course"),
                IsStopped = D21CanonicalJson.GetBool(root, "is_stopped"),
                HeadingCentidegrees = D21CanonicalJson.GetInt(root, "heading_centidegrees"),
                SpeedPermille = D21CanonicalJson.GetInt(root, "speed_permille"),
                LastAcceptedTick = D21CanonicalJson.GetInt(root, "last_accepted_tick"),
                CooldownUntilTick = D21CanonicalJson.GetInt(root, "cooldown_until_tick"),
                LastCommandId = D21CanonicalJson.GetString(root, "last_command_id"),
            };
            foreach (object value in D21CanonicalJson.GetArray(root, "applied_commands"))
            {
                Dictionary<string, object> item = value as Dictionary<string, object>;
                if (item == null) throw new FormatException("D21 course receipt must be an object.");
                result.AppliedCommands.Add(new D21CourseCommandReceipt
                {
                    CommandId = D21CanonicalJson.GetString(item, "command_id"),
                    Fingerprint = D21CanonicalJson.GetString(item, "fingerprint"),
                    AcceptedTick = D21CanonicalJson.GetInt(item, "accepted_tick"),
                    ResultCode = D21CanonicalJson.GetString(item, "result_code"),
                });
            }
            result.Validate();
            return result;
        }

        private static void WriteRiskSeeds(StringBuilder sb, List<D21VoyageRiskSeed> seeds)
        {
            sb.Append('[');
            bool first = true;
            if (seeds != null)
                foreach (D21VoyageRiskSeed seed in seeds)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append('{');
                    Field(sb, "risk_type", seed.RiskType.ToString()); sb.Append(',');
                    Field(sb, "seed", seed.Seed); sb.Append(',');
                    Field(sb, "roll_permille", seed.RollPermille); sb.Append(',');
                    Field(sb, "input_fingerprint", seed.InputFingerprint);
                    sb.Append('}');
                }
            sb.Append(']');
        }

        private static List<D21VoyageRiskSeed> ReadRiskSeeds(Dictionary<string, object> root, string key)
        {
            var result = new List<D21VoyageRiskSeed>();
            foreach (object value in D21CanonicalJson.GetArray(root, key))
            {
                Dictionary<string, object> item = value as Dictionary<string, object>;
                if (item == null) throw new FormatException("D21 risk seed must be an object.");
                result.Add(new D21VoyageRiskSeed
                {
                    RiskType = ParseEnum<D21VoyageRiskType>(D21CanonicalJson.GetString(item, "risk_type")),
                    Seed = D21CanonicalJson.GetLong(item, "seed"),
                    RollPermille = D21CanonicalJson.GetInt(item, "roll_permille"),
                    InputFingerprint = D21CanonicalJson.GetString(item, "input_fingerprint"),
                });
            }
            return result;
        }

        private static void WriteWeatherSeed(StringBuilder sb, D21WeatherSeedSaveProjection weather)
        {
            weather.Validate();
            sb.Append('{');
            Field(sb, "schema_version", weather.SchemaVersion); sb.Append(',');
            Field(sb, "voyage_seed", weather.VoyageSeed); sb.Append(',');
            Field(sb, "weather_seed", weather.WeatherSeed); sb.Append(',');
            Field(sb, "weather_source_id", weather.WeatherSourceId); sb.Append(',');
            Field(sb, "weather_snapshot_schema_version", weather.WeatherSnapshotSchemaVersion); sb.Append(',');
            Field(sb, "weather_snapshot_revision", weather.WeatherSnapshotRevision); sb.Append(',');
            Field(sb, "applied_weather_event_ids", weather.AppliedWeatherEventIds);
            sb.Append('}');
        }

        private static D21WeatherSeedSaveProjection ReadWeatherSeed(Dictionary<string, object> root)
        {
            var result = new D21WeatherSeedSaveProjection
            {
                SchemaVersion = D21CanonicalJson.GetInt(root, "schema_version"),
                VoyageSeed = D21CanonicalJson.GetLong(root, "voyage_seed"),
                WeatherSeed = D21CanonicalJson.GetLong(root, "weather_seed"),
                WeatherSourceId = D21CanonicalJson.GetString(root, "weather_source_id"),
                WeatherSnapshotSchemaVersion = D21CanonicalJson.GetInt(root, "weather_snapshot_schema_version"),
                WeatherSnapshotRevision = D21CanonicalJson.GetLong(root, "weather_snapshot_revision"),
                AppliedWeatherEventIds = D21CanonicalJson.GetStringList(root, "applied_weather_event_ids"),
            };
            result.Validate();
            return result;
        }

        private static void WriteProvisions(StringBuilder sb, D21ProvisionLedgerSaveProjection provisions)
        {
            provisions.Validate();
            sb.Append('{');
            Field(sb, "schema_version", provisions.SchemaVersion); sb.Append(',');
            Field(sb, "ledger_revision", provisions.LedgerRevision); sb.Append(',');
            Field(sb, "consumed_water_units", provisions.ConsumedWaterUnits); sb.Append(',');
            Field(sb, "consumed_food_units", provisions.ConsumedFoodUnits); sb.Append(',');
            Field(sb, "last_applied_tick", provisions.LastAppliedTick); sb.Append(',');
            Field(sb, "last_event_id", provisions.LastEventId); sb.Append(',');
            sb.Append("\"inventory_snapshot\":");
            if (provisions.InventorySnapshot == null) sb.Append("null"); else WriteReference(sb, provisions.InventorySnapshot);
            sb.Append('}');
        }

        private static D21ProvisionLedgerSaveProjection ReadProvisions(Dictionary<string, object> root)
        {
            var result = new D21ProvisionLedgerSaveProjection
            {
                SchemaVersion = D21CanonicalJson.GetInt(root, "schema_version"),
                LedgerRevision = D21CanonicalJson.GetLong(root, "ledger_revision"),
                ConsumedWaterUnits = D21CanonicalJson.GetLong(root, "consumed_water_units"),
                ConsumedFoodUnits = D21CanonicalJson.GetLong(root, "consumed_food_units"),
                LastAppliedTick = D21CanonicalJson.GetInt(root, "last_applied_tick"),
                LastEventId = D21CanonicalJson.GetString(root, "last_event_id"),
            };
            object value;
            if (root.TryGetValue("inventory_snapshot", out value) && value != null)
                result.InventorySnapshot = ReadReference(value as Dictionary<string, object>);
            result.Validate();
            return result;
        }

        private static void WriteEvents(StringBuilder sb, D21EventCursorSaveProjection events)
        {
            events.Validate();
            sb.Append('{');
            Field(sb, "schema_version", events.SchemaVersion); sb.Append(',');
            Field(sb, "last_event_id", events.LastEventId); sb.Append(',');
            Field(sb, "last_event_sequence", events.LastEventSequence); sb.Append(',');
            Field(sb, "last_event_save_revision", events.LastEventSaveRevision); sb.Append(',');
            Field(sb, "applied_event_keys", events.AppliedEventKeys); sb.Append(',');
            sb.Append("\"event_log\":");
            WriteEventLog(sb, events.EventLog);
            sb.Append('}');
        }

        private static D21EventCursorSaveProjection ReadEvents(Dictionary<string, object> root)
        {
            var result = new D21EventCursorSaveProjection
            {
                SchemaVersion = D21CanonicalJson.GetInt(root, "schema_version"),
                LastEventId = D21CanonicalJson.GetString(root, "last_event_id"),
                LastEventSequence = D21CanonicalJson.GetLong(root, "last_event_sequence"),
                LastEventSaveRevision = D21CanonicalJson.GetLong(root, "last_event_save_revision"),
                AppliedEventKeys = D21CanonicalJson.GetStringList(root, "applied_event_keys"),
                EventLog = ReadEventLog(D21CanonicalJson.GetObject(root, "event_log")),
            };
            result.Validate();
            return result;
        }

        private static void WriteEventLog(StringBuilder sb, D21VoyageEventLogSnapshot log)
        {
            log.Validate();
            sb.Append('{');
            Field(sb, "schema_version", log.SchemaVersion); sb.Append(',');
            Field(sb, "next_sequence", log.NextSequence); sb.Append(',');
            sb.Append("\"events\":").Append(D21VoyageEventSerializer.SerializeMany(log.Events));
            sb.Append('}');
        }

        private static D21VoyageEventLogSnapshot ReadEventLog(Dictionary<string, object> root)
        {
            var result = new D21VoyageEventLogSnapshot
            {
                SchemaVersion = D21CanonicalJson.GetInt(root, "schema_version"),
                NextSequence = D21CanonicalJson.GetLong(root, "next_sequence"),
            };
            foreach (object value in D21CanonicalJson.GetArray(root, "events"))
            {
                Dictionary<string, object> item = value as Dictionary<string, object>;
                if (item == null) throw new FormatException("D21 event log item must be an object.");
                result.Events.Add(D21VoyageEventSerializer.Deserialize(D21CanonicalJson.Stringify(item)));
            }
            result.Validate();
            return result;
        }

        private static void WriteReferences(StringBuilder sb, List<D21OwnerSnapshotReference> references)
        {
            sb.Append('[');
            bool first = true;
            if (references != null)
                foreach (D21OwnerSnapshotReference reference in references)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    WriteReference(sb, reference);
                }
            sb.Append(']');
        }

        private static List<D21OwnerSnapshotReference> ReadReferences(Dictionary<string, object> root, string key)
        {
            var result = new List<D21OwnerSnapshotReference>();
            foreach (object value in D21CanonicalJson.GetArray(root, key))
            {
                Dictionary<string, object> item = value as Dictionary<string, object>;
                if (item == null) throw new FormatException("D21 snapshot reference must be an object.");
                result.Add(ReadReference(item));
            }
            return result;
        }

        private static void Field(StringBuilder sb, string name, string value)
        {
            D21CanonicalJson.WriteString(sb, name); sb.Append(':'); D21CanonicalJson.WriteString(sb, value ?? string.Empty);
        }

        private static void Field(StringBuilder sb, string name, int value)
        {
            D21CanonicalJson.WriteString(sb, name); sb.Append(':').Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void Field(StringBuilder sb, string name, long value)
        {
            D21CanonicalJson.WriteString(sb, name); sb.Append(':').Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void Field(StringBuilder sb, string name, bool value)
        {
            D21CanonicalJson.WriteString(sb, name); sb.Append(':').Append(value ? "true" : "false");
        }

        private static void Field(StringBuilder sb, string name, List<string> values)
        {
            D21CanonicalJson.WriteString(sb, name); sb.Append(':');
            sb.Append('[');
            bool first = true;
            if (values != null)
                foreach (string value in values)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    D21CanonicalJson.WriteString(sb, value ?? string.Empty);
                }
            sb.Append(']');
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            T parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : default(T);
        }
    }
}
