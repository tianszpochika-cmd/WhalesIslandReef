// ============================================================
// D12MapDiscoveryContracts —— 海图发现、证据和工具契约
//
// 地图事实由 D12ExplorationSystem 唯一裁决；这些 DTO 只描述输入、输出和
// 可保存快照。定义中的真实水深是世界内容配置，发现状态则必须通过事实日志
// 和 D12 快照获得，表现层不能直接读取定义来绕过发现流程。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public static class D12MapDiscoveryIds
    {
        public const string SystemOwner = "ExplorationSystem";
        public const string SoundingLineToolId = "tool_sounding_line";
        public const string TelescopeToolId = "tool_telescope";
        public const string MapDiscoveryEventPrefix = "event_d12_map_discovery_";
    }

    public enum D12MapLayer
    {
        Coastline,
        NavigationHazards,
        Bathymetry,
        Ecology,
        CommunityKnowledge,
        ExplorationSites,
    }

    public enum D12MapFeatureType
    {
        UnknownCoast,
        Reef,
        Shoal,
        FishingGround,
        SafeChannel,
        Wreck,
        CaveEntrance,
    }

    public enum D12DiscoveryStage
    {
        Unknown,
        Rumor,
        Approximate,
        Located,
        Reliable,
        Verified,
    }

    public enum D12DiscoverySource
    {
        Exploration,
        Trade,
        Quest,
        CommunityKnowledge,
        Measurement,
    }

    public enum D12EvidenceKind
    {
        DirectSight,
        TelescopeObservation,
        SoundingLine,
        TradeChart,
        QuestClue,
        CommunityTestimony,
        ShipLog,
    }

    public enum D12MapCredibilityBand
    {
        Uncharted,
        Rumored,
        Approximate,
        Reliable,
        Verified,
    }

    [Serializable]
    public sealed class D12MapFeatureDefinition
    {
        public string Id;
        public D12MapFeatureType Type;
        public D12MapLayer Layer;
        public string LocationRef;
        public string RegionRef;
        public string DisplayName;
        public string ContentRef;
        public float TrueDepthMeters;
        public bool IsHazard;

        public D12MapFeatureDefinition Clone()
        {
            return new D12MapFeatureDefinition
            {
                Id = Id,
                Type = Type,
                Layer = Layer,
                LocationRef = LocationRef,
                RegionRef = RegionRef,
                DisplayName = DisplayName,
                ContentRef = ContentRef,
                TrueDepthMeters = TrueDepthMeters,
                IsHazard = IsHazard,
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new InvalidOperationException("地图特征必须有稳定 ID。");
            if (string.IsNullOrWhiteSpace(LocationRef)) throw new InvalidOperationException("地图特征必须有 location_ref：" + Id);
            if (string.IsNullOrWhiteSpace(RegionRef)) throw new InvalidOperationException("地图特征必须有 region_ref：" + Id);
            if (TrueDepthMeters < 0f) throw new InvalidOperationException("地图特征水深不能为负数：" + Id);
            if (!Enum.IsDefined(typeof(D12MapFeatureType), Type)) throw new InvalidOperationException("未知地图特征类型：" + Id);
            if (!Enum.IsDefined(typeof(D12MapLayer), Layer)) throw new InvalidOperationException("未知海图分层：" + Id);
        }
    }

    [Serializable]
    public sealed class D12ChartEvidence
    {
        public string EvidenceId;
        public D12DiscoverySource Source;
        public D12EvidenceKind Kind;
        public string SourceRef;
        public string EvidenceRef;
        public string Summary;
        public string LocationRef;
        public string ToolId;
        public string EventId;
        public int WorldDay;
        public long SaveRevision;
        public float Strength;

        public D12ChartEvidence Clone()
        {
            return new D12ChartEvidence
            {
                EvidenceId = EvidenceId,
                Source = Source,
                Kind = Kind,
                SourceRef = SourceRef,
                EvidenceRef = EvidenceRef,
                Summary = Summary,
                LocationRef = LocationRef,
                ToolId = ToolId,
                EventId = EventId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                Strength = Strength,
            };
        }

        public void Validate(string featureId)
        {
            if (string.IsNullOrWhiteSpace(EvidenceId)) throw new InvalidOperationException("海图证据缺少 evidence_id：" + featureId);
            if (string.IsNullOrWhiteSpace(SourceRef)) throw new InvalidOperationException("海图证据缺少来源：" + EvidenceId);
            if (string.IsNullOrWhiteSpace(EvidenceRef)) throw new InvalidOperationException("海图证据缺少证据引用：" + EvidenceId);
            if (string.IsNullOrWhiteSpace(Summary)) throw new InvalidOperationException("海图证据缺少可解释摘要：" + EvidenceId);
            if (WorldDay < 0) throw new InvalidOperationException("海图证据 world_day 不能为负数：" + EvidenceId);
            if (SaveRevision < 0) throw new InvalidOperationException("海图证据 save_revision 不能为负数：" + EvidenceId);
            if (Strength < 0f || Strength > 1f) throw new InvalidOperationException("海图证据强度必须在 0..1：" + EvidenceId);
            if (!Enum.IsDefined(typeof(D12DiscoverySource), Source)) throw new InvalidOperationException("海图证据来源枚举非法：" + EvidenceId);
            if (!Enum.IsDefined(typeof(D12EvidenceKind), Kind)) throw new InvalidOperationException("海图证据类型枚举非法：" + EvidenceId);
        }
    }

    [Serializable]
    public sealed class D12MapFeatureState
    {
        public string FeatureId;
        public D12DiscoveryStage Stage = D12DiscoveryStage.Unknown;
        public float Credibility;
        public int LastDiscoveredDay;
        public string LastSource;
        public string LastEvidenceId;
        public bool HasMeasuredDepth;
        public float MeasuredDepthMeters;
        public List<string> EvidenceIds = new List<string>();
        public List<D12ChartEvidence> Evidence = new List<D12ChartEvidence>();

        public D12MapFeatureState Clone()
        {
            var result = new D12MapFeatureState
            {
                FeatureId = FeatureId,
                Stage = Stage,
                Credibility = Credibility,
                LastDiscoveredDay = LastDiscoveredDay,
                LastSource = LastSource,
                LastEvidenceId = LastEvidenceId,
                HasMeasuredDepth = HasMeasuredDepth,
                MeasuredDepthMeters = MeasuredDepthMeters,
            };
            if (EvidenceIds != null) result.EvidenceIds.AddRange(EvidenceIds);
            if (Evidence != null)
                foreach (var evidence in Evidence)
                    if (evidence != null) result.Evidence.Add(evidence.Clone());
            return result;
        }
    }

    [Serializable]
    public sealed class D12MapDiscoverySnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public int LastWorldDay;
        public long Revision;
        public string LastEventId;
        public List<D12MapFeatureState> Features = new List<D12MapFeatureState>();

        public D12MapDiscoverySnapshot Clone()
        {
            var result = new D12MapDiscoverySnapshot
            {
                SchemaVersion = SchemaVersion,
                LastWorldDay = LastWorldDay,
                Revision = Revision,
                LastEventId = LastEventId,
            };
            if (Features != null)
                foreach (var feature in Features)
                    if (feature != null) result.Features.Add(feature.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D12 海图快照 schema_version 不匹配：收到 " + SchemaVersion + "。");
            if (LastWorldDay < 0) throw new InvalidOperationException("D12 海图快照 world_day 不能为负数。");
            if (Revision < 0) throw new InvalidOperationException("D12 海图快照 revision 不能为负数。");
            var featureIds = new HashSet<string>(StringComparer.Ordinal);
            if (Features == null) return;
            foreach (var feature in Features)
            {
                if (feature == null) throw new InvalidOperationException("D12 海图快照不能包含 null 特征。");
                if (string.IsNullOrWhiteSpace(feature.FeatureId)) throw new InvalidOperationException("D12 海图状态缺少 feature_id。");
                if (!featureIds.Add(feature.FeatureId)) throw new InvalidOperationException("D12 海图状态重复：" + feature.FeatureId);
                if (!Enum.IsDefined(typeof(D12DiscoveryStage), feature.Stage)) throw new InvalidOperationException("D12 海图阶段非法：" + feature.FeatureId);
                if (feature.Credibility < 0f || feature.Credibility > 1f) throw new InvalidOperationException("D12 海图可信度非法：" + feature.FeatureId);
                if (feature.LastDiscoveredDay < 0) throw new InvalidOperationException("D12 海图发现日非法：" + feature.FeatureId);
                if (feature.MeasuredDepthMeters < 0f) throw new InvalidOperationException("D12 测深结果不能为负数：" + feature.FeatureId);
                if (feature.Stage == D12DiscoveryStage.Unknown && (feature.Evidence == null || feature.Evidence.Count > 0))
                    throw new InvalidOperationException("未知海图特征不能带证据：" + feature.FeatureId);
                var evidenceIds = new HashSet<string>(StringComparer.Ordinal);
                if (feature.Evidence == null) continue;
                foreach (var evidence in feature.Evidence)
                {
                    if (evidence == null) throw new InvalidOperationException("D12 海图证据不能为 null：" + feature.FeatureId);
                    evidence.Validate(feature.FeatureId);
                    if (!evidenceIds.Add(evidence.EvidenceId)) throw new InvalidOperationException("D12 海图证据重复：" + evidence.EvidenceId);
                }
            }
        }
    }

    [Serializable]
    public sealed class D12RuntimeSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public D12MapDiscoverySnapshot MapDiscovery = new D12MapDiscoverySnapshot();
        public D12TarotRuntimeSnapshot Tarot = new D12TarotRuntimeSnapshot();
        public D12CollectionAtlasState CollectionAtlas = new D12CollectionAtlasState();
        /// <summary>D12-A04 社群库存、边界访问结果和长期声望；旧 D12 存档缺失时使用空快照。</summary>
        public D12CommunityTradeSnapshot CommunityTrade = new D12CommunityTradeSnapshot();
        /// <summary>D12-A02 沉船/洞穴/打捞快照；探索系统写入，Save 域只序列化。</summary>
        public D12SalvageSnapshot Salvage = new D12SalvageSnapshot();
        /// <summary>D12-A08 动态任务状态和日志事实引用；QuestSystem 写入，Save 域只序列化。</summary>
        public D12DynamicQuestLogSnapshot DynamicQuestLog = new D12DynamicQuestLogSnapshot();

        public D12RuntimeSnapshot Clone()
        {
            return new D12RuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                MapDiscovery = MapDiscovery == null ? null : MapDiscovery.Clone(),
                Tarot = Tarot == null ? null : Tarot.Clone(),
                CollectionAtlas = CollectionAtlas == null ? null : CollectionAtlas.Clone(),
                CommunityTrade = CommunityTrade == null ? null : CommunityTrade.Clone(),
                Salvage = Salvage == null ? null : Salvage.Clone(),
                DynamicQuestLog = DynamicQuestLog == null ? null : DynamicQuestLog.Clone(),
            };
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D12 运行时快照 schema_version 不匹配：收到 " + SchemaVersion + "。");
            if (MapDiscovery == null) throw new InvalidOperationException("D12 运行时快照缺少海图发现快照。");
            MapDiscovery.Validate();
            if (Tarot == null) throw new InvalidOperationException("D12 运行时快照缺少塔罗快照。");
            Tarot.Validate();
            if (CollectionAtlas == null) throw new InvalidOperationException("D12 运行时快照缺少收藏图鉴快照。");
            CollectionAtlas.Validate();
            if (CommunityTrade != null) CommunityTrade.Validate();
            if (Salvage == null) throw new InvalidOperationException("D12 运行时快照缺少沉船/洞穴打捞快照。");
            Salvage.Validate();
            if (DynamicQuestLog == null) throw new InvalidOperationException("D12 运行时快照缺少动态任务/日志快照。");
            DynamicQuestLog.Validate();
        }
    }

    [Serializable]
    public sealed class D12DiscoveryRequest
    {
        public string FeatureId;
        public string ActorRef = "player";
        public string SourceRef;
        public string EvidenceRef;
        public string Summary;
        public string LocationRef;
        public int WorldDay;
        public long SaveRevision;
        public float Strength;
        public D12DiscoverySource Source = D12DiscoverySource.Exploration;
        public D12EvidenceKind EvidenceKind = D12EvidenceKind.DirectSight;
        public string ToolId;
    }

    [Serializable]
    public sealed class D12CommunityKnowledgeProposal
    {
        public string ProposalId;
        public string FeatureId;
        public string SpeakerRef;
        public string SourceRef;
        public string EvidenceRef;
        public string Summary;
        public int WorldDay;
        public float ReliabilityHint;

        public D12CommunityKnowledgeProposal Clone()
        {
            return new D12CommunityKnowledgeProposal
            {
                ProposalId = ProposalId,
                FeatureId = FeatureId,
                SpeakerRef = SpeakerRef,
                SourceRef = SourceRef,
                EvidenceRef = EvidenceRef,
                Summary = Summary,
                WorldDay = WorldDay,
                ReliabilityHint = ReliabilityHint,
            };
        }
    }

    [Serializable]
    public sealed class D12CommunityKnowledgeAcceptance
    {
        public D12CommunityKnowledgeProposal Proposal;
        public string CommunityReceiptId;
        public bool CommunityConfirmed;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D12SoundingLineRequest
    {
        public string FeatureId;
        public string ActorRef = "player";
        public string LocationRef;
        public string EvidenceRef;
        public string SoundingPositionRef;
        public float LineLengthMeters;
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D12TelescopeRequest
    {
        public string FeatureId;
        public string ActorRef = "player";
        public string LocationRef;
        public string EvidenceRef;
        public float DistanceNauticalMiles;
        public float Visibility;
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D12DiscoveryResult
    {
        public bool Accepted;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string FeatureId;
        public D12DiscoveryStage Stage;
        public float Credibility;
        public D12MapCredibilityBand CredibilityBand;
        public D12ChartEvidence Evidence;
        public FactEvent Fact;

        public D12DiscoveryResult Clone()
        {
            return new D12DiscoveryResult
            {
                Accepted = Accepted,
                Duplicate = Duplicate,
                Code = Code,
                Message = Message,
                FeatureId = FeatureId,
                Stage = Stage,
                Credibility = Credibility,
                CredibilityBand = CredibilityBand,
                Evidence = Evidence == null ? null : Evidence.Clone(),
                Fact = Fact == null ? null : Fact.CloneFact(),
            };
        }
    }

    [Serializable]
    public sealed class D12ToolResult
    {
        public bool Succeeded;
        public string Code;
        public string Message;
        public string FeatureId;
        public string ToolId;
        public bool BottomReached;
        public float MeasuredDepthMeters;
        public D12DiscoveryResult Discovery;
        public FactEvent Fact;

        public D12ToolResult Clone()
        {
            return new D12ToolResult
            {
                Succeeded = Succeeded,
                Code = Code,
                Message = Message,
                FeatureId = FeatureId,
                ToolId = ToolId,
                BottomReached = BottomReached,
                MeasuredDepthMeters = MeasuredDepthMeters,
                Discovery = Discovery == null ? null : Discovery.Clone(),
                Fact = Fact == null ? null : Fact.CloneFact(),
            };
        }
    }

    [Serializable]
    public sealed class D12MapFeatureView
    {
        public string FeatureId;
        public D12MapFeatureType Type;
        public D12MapLayer Layer;
        public string DisplayName;
        public string RegionRef;
        public string LocationRef;
        public D12DiscoveryStage Stage;
        public D12MapCredibilityBand CredibilityBand;
        public float Credibility;
        public int EvidenceCount;
        public string LastSource;
        public bool HasMeasuredDepth;
        public float MeasuredDepthMeters;

        public D12MapFeatureView Clone()
        {
            return new D12MapFeatureView
            {
                FeatureId = FeatureId,
                Type = Type,
                Layer = Layer,
                DisplayName = DisplayName,
                RegionRef = RegionRef,
                LocationRef = LocationRef,
                Stage = Stage,
                CredibilityBand = CredibilityBand,
                Credibility = Credibility,
                EvidenceCount = EvidenceCount,
                LastSource = LastSource,
                HasMeasuredDepth = HasMeasuredDepth,
                MeasuredDepthMeters = MeasuredDepthMeters,
            };
        }
    }

    [Serializable]
    public sealed class D12MapReadModel
    {
        public int SchemaVersion = D12MapDiscoverySnapshot.CurrentSchemaVersion;
        public List<D12MapFeatureView> VisibleFeatures = new List<D12MapFeatureView>();

        public D12MapReadModel Clone()
        {
            var result = new D12MapReadModel { SchemaVersion = SchemaVersion };
            if (VisibleFeatures != null)
                foreach (var feature in VisibleFeatures)
                    if (feature != null) result.VisibleFeatures.Add(feature.Clone());
            return result;
        }
    }
}
