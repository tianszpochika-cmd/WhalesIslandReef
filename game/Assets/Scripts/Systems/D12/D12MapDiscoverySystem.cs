// ============================================================
// D12MapDiscoverySystem —— 海图唯一写者
//
// 发现状态、可信度和工具测量都只在本系统内修改。EventLog 记录已经裁决的
// 发现事实，SaveSystem 只保存本系统提供的快照；UI 只能消费 GetMapReadModel，
// Agent/船员只能通过 ProposeCommunityKnowledge 产生未生效的提案。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public sealed class D12MapDiscoverySystem
    {
        private const float TelescopeMaxRangeNauticalMiles = 20f;
        private const float TelescopeMinimumVisibility = 0.35f;

        private readonly List<D12MapFeatureDefinition> _orderedDefinitions =
            new List<D12MapFeatureDefinition>();
        private readonly Dictionary<string, D12MapFeatureDefinition> _definitions =
            new Dictionary<string, D12MapFeatureDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D12MapFeatureState> _states =
            new Dictionary<string, D12MapFeatureState>(StringComparer.Ordinal);
        private readonly EventLog _eventLog;
        private int _lastWorldDay;
        private long _revision;
        private string _lastEventId;

        public D12MapDiscoverySystem(IEnumerable<D12MapFeatureDefinition> definitions, EventLog eventLog)
            : this(definitions, eventLog, null)
        {
        }

        public D12MapDiscoverySystem(IEnumerable<D12MapFeatureDefinition> definitions,
            EventLog eventLog, D12MapDiscoverySnapshot savedSnapshot)
        {
            if (definitions == null) throw new ArgumentNullException("definitions");
            _eventLog = eventLog ?? new EventLog();
            foreach (var source in definitions)
            {
                if (source == null) throw new InvalidOperationException("海图目录不能包含 null 特征。");
                var definition = source.Clone();
                definition.Validate();
                if (_definitions.ContainsKey(definition.Id))
                    throw new InvalidOperationException("海图目录重复：" + definition.Id);
                _definitions.Add(definition.Id, definition);
                _orderedDefinitions.Add(definition);
                _states.Add(definition.Id, NewUnknownState(definition.Id));
            }

            if (savedSnapshot != null)
            {
                savedSnapshot.Validate();
                _lastWorldDay = savedSnapshot.LastWorldDay;
                _revision = savedSnapshot.Revision;
                _lastEventId = savedSnapshot.LastEventId;
                foreach (var savedState in savedSnapshot.Features)
                {
                    if (!_definitions.ContainsKey(savedState.FeatureId))
                        throw new InvalidOperationException("存档包含未注册的海图特征：" + savedState.FeatureId);
                    _states[savedState.FeatureId] = savedState.Clone();
                }
            }
        }

        public EventLog EventLog { get { return _eventLog; } }

        /// <summary>返回副本；调用方不能通过该属性写回发现状态。</summary>
        public D12MapDiscoverySnapshot Snapshot
        {
            get { return CaptureSnapshot(); }
        }

        public D12MapDiscoverySnapshot CaptureSnapshot()
        {
            var snapshot = new D12MapDiscoverySnapshot
            {
                LastWorldDay = _lastWorldDay,
                Revision = _revision,
                LastEventId = _lastEventId,
            };
            foreach (var definition in _orderedDefinitions)
                snapshot.Features.Add(_states[definition.Id].Clone());
            snapshot.Validate();
            return snapshot;
        }

        /// <summary>只读单特征副本；未知特征返回 null，不泄露目录真实位置。</summary>
        public D12MapFeatureState GetFeatureState(string featureId)
        {
            D12MapFeatureState state;
            return _states.TryGetValue(featureId ?? string.Empty, out state) ? state.Clone() : null;
        }

        /// <summary>
        /// UI 唯一海图读取面：未达到 Unknown 的特征不会出现，且传闻阶段不暴露精确位置，
        /// 真水深永远不从目录直接输出，只能输出已经完成的测深读数。
        /// </summary>
        public D12MapReadModel GetMapReadModel()
        {
            var result = new D12MapReadModel();
            foreach (var definition in _orderedDefinitions)
            {
                var state = _states[definition.Id];
                if (state.Stage == D12DiscoveryStage.Unknown) continue;
                result.VisibleFeatures.Add(new D12MapFeatureView
                {
                    FeatureId = definition.Id,
                    Type = definition.Type,
                    Layer = definition.Layer,
                    DisplayName = definition.DisplayName,
                    RegionRef = definition.RegionRef,
                    LocationRef = state.Stage >= D12DiscoveryStage.Approximate ? definition.LocationRef : string.Empty,
                    Stage = state.Stage,
                    CredibilityBand = ToCredibilityBand(state.Credibility, state.Stage),
                    Credibility = state.Credibility,
                    EvidenceCount = state.Evidence == null ? 0 : state.Evidence.Count,
                    LastSource = state.LastSource,
                    HasMeasuredDepth = state.HasMeasuredDepth,
                    MeasuredDepthMeters = state.HasMeasuredDepth ? state.MeasuredDepthMeters : 0f,
                });
            }
            return result;
        }

        /// <summary>玩家在海上直接观察到海岸、危险或鱼场；不接受 Agent 伪造的社区知识来源。</summary>
        public D12DiscoveryResult Discover(D12DiscoveryRequest request)
        {
            if (request == null) return Reject("invalid_request", "发现请求不能为空。", null);
            if (request.Source != D12DiscoverySource.Exploration)
                return Reject("source_not_exploration", "直接发现只能由探索来源提交。", request.FeatureId);
            float strength = request.Strength <= 0f ? 0.68f : request.Strength;
            return ApplyDiscovery(request, strength, false, 0f);
        }

        public D12DiscoveryResult RecordTradeKnowledge(string featureId, string tradeRef,
            string evidenceRef, string summary, string locationRef, int worldDay, long saveRevision)
        {
            return ApplyDiscovery(new D12DiscoveryRequest
            {
                FeatureId = featureId,
                Source = D12DiscoverySource.Trade,
                EvidenceKind = D12EvidenceKind.TradeChart,
                SourceRef = "trade:" + (tradeRef ?? string.Empty),
                EvidenceRef = evidenceRef,
                Summary = summary,
                LocationRef = locationRef,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
                Strength = 0.55f,
            }, 0.55f, false, 0f);
        }

        public D12DiscoveryResult RecordQuestKnowledge(string featureId, string questRef,
            string evidenceRef, string summary, string locationRef, int worldDay, long saveRevision)
        {
            return ApplyDiscovery(new D12DiscoveryRequest
            {
                FeatureId = featureId,
                Source = D12DiscoverySource.Quest,
                EvidenceKind = D12EvidenceKind.QuestClue,
                SourceRef = "quest:" + (questRef ?? string.Empty),
                EvidenceRef = evidenceRef,
                Summary = summary,
                LocationRef = locationRef,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
                Strength = 0.78f,
            }, 0.78f, false, 0f);
        }

        /// <summary>
        /// Agent/NPC 可以调用此方法生成传闻提案，但该方法不写快照、不追加事实。
        /// 只有带社区确认回执的 AcceptCommunityKnowledge 才能把传闻记为 Rumor。
        /// </summary>
        public D12CommunityKnowledgeProposal ProposeCommunityKnowledge(string featureId,
            string speakerRef, string sourceRef, string evidenceRef, string summary,
            int worldDay, float reliabilityHint)
        {
            if (!_definitions.ContainsKey(featureId ?? string.Empty)) return null;
            if (string.IsNullOrWhiteSpace(speakerRef) || string.IsNullOrWhiteSpace(sourceRef) ||
                string.IsNullOrWhiteSpace(evidenceRef) || string.IsNullOrWhiteSpace(summary)) return null;
            return new D12CommunityKnowledgeProposal
            {
                ProposalId = "proposal:map:" + featureId + ":" + evidenceRef,
                FeatureId = featureId,
                SpeakerRef = speakerRef,
                SourceRef = sourceRef,
                EvidenceRef = evidenceRef,
                Summary = summary,
                WorldDay = Math.Max(0, worldDay),
                ReliabilityHint = Clamp(reliabilityHint <= 0f ? 0.35f : reliabilityHint, 0f, 0.49f),
            };
        }

        public D12DiscoveryResult AcceptCommunityKnowledge(D12CommunityKnowledgeAcceptance acceptance)
        {
            if (acceptance == null || acceptance.Proposal == null)
                return Reject("invalid_proposal", "社区知识提案不能为空。", null);
            var proposal = acceptance.Proposal;
            if (!acceptance.CommunityConfirmed || string.IsNullOrWhiteSpace(acceptance.CommunityReceiptId))
                return Reject("community_confirmation_required", "Agent 提案必须经过社区确认回执，不能直接解锁海图。", proposal.FeatureId);
            if (proposal.WorldDay < 0 || acceptance.SaveRevision < 0)
                return Reject("invalid_revision", "社区知识的日期或存档修订号非法。", proposal.FeatureId);

            return ApplyDiscovery(new D12DiscoveryRequest
            {
                FeatureId = proposal.FeatureId,
                ActorRef = proposal.SpeakerRef,
                Source = D12DiscoverySource.CommunityKnowledge,
                EvidenceKind = D12EvidenceKind.CommunityTestimony,
                SourceRef = "community:" + proposal.SourceRef,
                EvidenceRef = acceptance.CommunityReceiptId,
                Summary = proposal.Summary,
                WorldDay = proposal.WorldDay,
                SaveRevision = acceptance.SaveRevision,
                Strength = Clamp(proposal.ReliabilityHint, 0.20f, 0.49f),
            }, 0.35f, false, 0f);
        }

        /// <summary>
        /// 测深绳只对可以触底的海图特征产生深度证据；绳长不足时返回可解释失败，
        /// 不会凭空标记暗礁或浅滩已发现。
        /// </summary>
        public D12ToolResult UseSoundingLine(D12SoundingLineRequest request)
        {
            if (request == null) return ToolReject("invalid_request", "测深请求不能为空。", null);
            D12MapFeatureDefinition definition;
            if (!_definitions.TryGetValue(request.FeatureId ?? string.Empty, out definition))
                return ToolReject("feature_unknown", "海图目录中没有这个目标。", request.FeatureId);
            if (definition.Type == D12MapFeatureType.UnknownCoast)
                return ToolReject("not_soundable", "未知海岸需要望远镜或登陆勘察，不能用测深绳确认海岸线。", request.FeatureId);
            if (request.LineLengthMeters <= 0f)
                return ToolReject("line_required", "测深绳长度必须大于零。", request.FeatureId);
            if (request.LineLengthMeters + 0.001f < definition.TrueDepthMeters)
                return new D12ToolResult
                {
                    Succeeded = false,
                    Code = "line_too_short",
                    Message = "测深绳没有触底：只能确认水深超过当前绳长，不能把它记成精确海图证据。",
                    FeatureId = request.FeatureId,
                    ToolId = D12MapDiscoveryIds.SoundingLineToolId,
                    BottomReached = false,
                    MeasuredDepthMeters = request.LineLengthMeters,
                };

            string evidenceRef = string.IsNullOrWhiteSpace(request.EvidenceRef)
                ? "sounding:" + request.FeatureId + ":" + request.SaveRevision.ToString(CultureInfo.InvariantCulture)
                : request.EvidenceRef;
            var discovery = ApplyDiscovery(new D12DiscoveryRequest
            {
                FeatureId = request.FeatureId,
                ActorRef = request.ActorRef,
                Source = D12DiscoverySource.Measurement,
                EvidenceKind = D12EvidenceKind.SoundingLine,
                SourceRef = "tool:" + D12MapDiscoveryIds.SoundingLineToolId,
                EvidenceRef = evidenceRef,
                Summary = "测深绳触底，读数 " + definition.TrueDepthMeters.ToString("0.0", CultureInfo.InvariantCulture) + " 米。",
                LocationRef = request.LocationRef,
                ToolId = D12MapDiscoveryIds.SoundingLineToolId,
                WorldDay = request.WorldDay,
                SaveRevision = request.SaveRevision,
                Strength = 0.96f,
            }, 0.96f, true, definition.TrueDepthMeters);
            return new D12ToolResult
            {
                Succeeded = discovery.Accepted,
                Code = discovery.Code,
                Message = discovery.Message,
                FeatureId = request.FeatureId,
                ToolId = D12MapDiscoveryIds.SoundingLineToolId,
                BottomReached = discovery.Accepted,
                MeasuredDepthMeters = discovery.Accepted ? definition.TrueDepthMeters : 0f,
                Discovery = discovery,
            };
        }

        /// <summary>望远镜受能见度和距离约束，可把海岸或鱼场从未知推进到已定位。</summary>
        public D12ToolResult UseTelescope(D12TelescopeRequest request)
        {
            if (request == null) return ToolReject("invalid_request", "望远镜请求不能为空。", null);
            D12MapFeatureDefinition definition;
            if (!_definitions.TryGetValue(request.FeatureId ?? string.Empty, out definition))
                return ToolReject("feature_unknown", "海图目录中没有这个目标。", request.FeatureId);
            if (request.DistanceNauticalMiles < 0f || request.DistanceNauticalMiles > TelescopeMaxRangeNauticalMiles)
                return ToolReject("out_of_range", "目标超出望远镜的可解释观察距离。", request.FeatureId);
            if (request.Visibility < TelescopeMinimumVisibility)
                return ToolReject("visibility_too_low", "能见度不足，望远镜不能形成可靠标记。", request.FeatureId);
            if (definition.Type == D12MapFeatureType.SafeChannel)
                return ToolReject("not_visible_channel", "安全航道不能靠远望直接确认，需要测深或任务资料。", request.FeatureId);

            string evidenceRef = string.IsNullOrWhiteSpace(request.EvidenceRef)
                ? "telescope:" + request.FeatureId + ":" + request.SaveRevision.ToString(CultureInfo.InvariantCulture)
                : request.EvidenceRef;
            var discovery = ApplyDiscovery(new D12DiscoveryRequest
            {
                FeatureId = request.FeatureId,
                ActorRef = request.ActorRef,
                Source = D12DiscoverySource.Measurement,
                EvidenceKind = D12EvidenceKind.TelescopeObservation,
                SourceRef = "tool:" + D12MapDiscoveryIds.TelescopeToolId,
                EvidenceRef = evidenceRef,
                Summary = "望远镜在能见度 " + request.Visibility.ToString("0.00", CultureInfo.InvariantCulture) + " 下确认了目标轮廓。",
                LocationRef = request.LocationRef,
                ToolId = D12MapDiscoveryIds.TelescopeToolId,
                WorldDay = request.WorldDay,
                SaveRevision = request.SaveRevision,
                Strength = 0.74f,
            }, 0.74f, false, 0f);
            return new D12ToolResult
            {
                Succeeded = discovery.Accepted,
                Code = discovery.Code,
                Message = discovery.Message,
                FeatureId = request.FeatureId,
                ToolId = D12MapDiscoveryIds.TelescopeToolId,
                BottomReached = false,
                MeasuredDepthMeters = 0f,
                Discovery = discovery,
            };
        }

        private D12DiscoveryResult ApplyDiscovery(D12DiscoveryRequest request,
            float defaultStrength, bool measured, float measuredDepth)
        {
            if (request == null) return Reject("invalid_request", "发现请求不能为空。", null);
            D12MapFeatureDefinition definition;
            if (!_definitions.TryGetValue(request.FeatureId ?? string.Empty, out definition))
                return Reject("feature_unknown", "海图目录中没有这个目标。", request.FeatureId);
            if (request.WorldDay < 0 || request.SaveRevision < 0)
                return Reject("invalid_revision", "发现日期或存档修订号非法。", request.FeatureId);
            if (string.IsNullOrWhiteSpace(request.SourceRef) || string.IsNullOrWhiteSpace(request.EvidenceRef) ||
                string.IsNullOrWhiteSpace(request.Summary))
                return Reject("evidence_required", "海图可信度必须绑定来源、证据引用和解释摘要。", request.FeatureId);
            if (!Enum.IsDefined(typeof(D12DiscoverySource), request.Source) ||
                !Enum.IsDefined(typeof(D12EvidenceKind), request.EvidenceKind))
                return Reject("invalid_evidence_kind", "海图证据来源或类型非法。", request.FeatureId);

            var state = _states[definition.Id];
            float strength = request.Strength <= 0f ? defaultStrength : request.Strength;
            if (strength < 0f || strength > 1f)
                return Reject("invalid_strength", "证据强度必须在 0..1。", request.FeatureId);
            string evidenceId = BuildEvidenceId(request.FeatureId, request.Source, request.EvidenceRef);
            if (state.EvidenceIds.Contains(evidenceId))
                return Duplicate(request.FeatureId, state, evidenceId, FindFact(request.FeatureId, request.Source, request.EvidenceRef, request.SaveRevision));

            string eventId = BuildEventId(request.FeatureId, request.Source, evidenceId);
            var factResult = _eventLog.Append(new WorldEvent(
                eventId,
                Math.Max(0, request.WorldDay),
                string.IsNullOrWhiteSpace(request.LocationRef) ? definition.LocationRef : request.LocationRef,
                D12MapDiscoveryIds.SystemOwner,
                new EventResult
                {
                    ResultCode = "map_discovery_recorded",
                    Summary = request.Summary,
                    FactIds = new List<string> { "map:" + request.FeatureId, "evidence:" + evidenceId },
                    Values = new Dictionary<string, string>
                    {
                        ["feature_id"] = request.FeatureId,
                        ["source"] = request.Source.ToString(),
                        ["evidence_kind"] = request.EvidenceKind.ToString(),
                        ["evidence_id"] = evidenceId,
                        ["strength"] = strength.ToString("0.###", CultureInfo.InvariantCulture),
                        ["tool_id"] = request.ToolId ?? string.Empty,
                    },
                },
                new[] { string.IsNullOrWhiteSpace(request.ActorRef) ? "player" : request.ActorRef },
                new[] { "map:evidence:" + evidenceId },
                new[] { request.EvidenceRef },
                request.SaveRevision));

            if (factResult.IsDuplicate)
                return Duplicate(request.FeatureId, state, evidenceId, factResult.Fact);

            var evidence = new D12ChartEvidence
            {
                EvidenceId = evidenceId,
                Source = request.Source,
                Kind = request.EvidenceKind,
                SourceRef = request.SourceRef,
                EvidenceRef = request.EvidenceRef,
                Summary = request.Summary,
                LocationRef = string.IsNullOrWhiteSpace(request.LocationRef) ? definition.LocationRef : request.LocationRef,
                ToolId = request.ToolId,
                EventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                SaveRevision = request.SaveRevision,
                Strength = strength,
            };
            evidence.Validate(definition.Id);
            state.EvidenceIds.Add(evidenceId);
            state.Evidence.Add(evidence);
            state.Credibility = CalculateCredibility(state.Evidence);
            state.Stage = CalculateStage(state.Evidence, state.Credibility);
            state.LastDiscoveredDay = Math.Max(state.LastDiscoveredDay, Math.Max(0, request.WorldDay));
            state.LastSource = request.Source.ToString();
            state.LastEvidenceId = evidenceId;
            if (measured)
            {
                state.HasMeasuredDepth = true;
                state.MeasuredDepthMeters = Math.Max(0f, measuredDepth);
            }
            _lastWorldDay = Math.Max(_lastWorldDay, Math.Max(0, request.WorldDay));
            _revision = Math.Max(_revision + 1, request.SaveRevision);
            _lastEventId = eventId;
            return new D12DiscoveryResult
            {
                Accepted = true,
                Duplicate = false,
                Code = "discovery_recorded",
                Message = "海图已记录：" + definition.DisplayName + "，可信度 " + state.Credibility.ToString("0.00", CultureInfo.InvariantCulture) + "。",
                FeatureId = definition.Id,
                Stage = state.Stage,
                Credibility = state.Credibility,
                CredibilityBand = ToCredibilityBand(state.Credibility, state.Stage),
                Evidence = evidence.Clone(),
                Fact = factResult.Fact,
            };
        }

        private D12DiscoveryResult Duplicate(string featureId, D12MapFeatureState state,
            string evidenceId, FactEvent fact)
        {
            D12ChartEvidence evidence = null;
            if (state.Evidence != null)
                foreach (var item in state.Evidence)
                    if (item != null && item.EvidenceId == evidenceId) evidence = item.Clone();
            return new D12DiscoveryResult
            {
                Accepted = true,
                Duplicate = true,
                Code = "duplicate_discovery",
                Message = "这条海图证据已经记录，未重复追加事实。",
                FeatureId = featureId,
                Stage = state.Stage,
                Credibility = state.Credibility,
                CredibilityBand = ToCredibilityBand(state.Credibility, state.Stage),
                Evidence = evidence,
                Fact = fact,
            };
        }

        private FactEvent FindFact(string featureId, D12DiscoverySource source,
            string evidenceRef, long saveRevision)
        {
            string evidenceId = BuildEvidenceId(featureId, source, evidenceRef);
            return _eventLog.Find(BuildEventId(featureId, source, evidenceId), saveRevision);
        }

        private static D12MapFeatureState NewUnknownState(string featureId)
        {
            return new D12MapFeatureState { FeatureId = featureId };
        }

        private static float CalculateCredibility(List<D12ChartEvidence> evidence)
        {
            if (evidence == null || evidence.Count == 0) return 0f;
            float best = 0f;
            var sources = new HashSet<D12DiscoverySource>();
            foreach (var item in evidence)
            {
                if (item == null) continue;
                best = Math.Max(best, item.Strength);
                sources.Add(item.Source);
            }
            return Clamp(best + Math.Min(0.15f, Math.Max(0, sources.Count - 1) * 0.05f), 0f, 1f);
        }

        private static D12DiscoveryStage CalculateStage(List<D12ChartEvidence> evidence, float credibility)
        {
            if (evidence == null || evidence.Count == 0) return D12DiscoveryStage.Unknown;
            bool hasSounding = false;
            var sources = new HashSet<D12DiscoverySource>();
            foreach (var item in evidence)
            {
                if (item == null) continue;
                hasSounding |= item.Kind == D12EvidenceKind.SoundingLine;
                sources.Add(item.Source);
            }
            if (hasSounding || (credibility >= 0.92f && sources.Count >= 2)) return D12DiscoveryStage.Verified;
            if (credibility >= 0.80f) return D12DiscoveryStage.Reliable;
            if (credibility >= 0.65f) return D12DiscoveryStage.Located;
            if (credibility >= 0.50f) return D12DiscoveryStage.Approximate;
            return D12DiscoveryStage.Rumor;
        }

        public static D12MapCredibilityBand ToCredibilityBand(float credibility, D12DiscoveryStage stage)
        {
            if (stage == D12DiscoveryStage.Unknown) return D12MapCredibilityBand.Uncharted;
            if (stage == D12DiscoveryStage.Rumor || credibility < 0.50f) return D12MapCredibilityBand.Rumored;
            if (stage == D12DiscoveryStage.Approximate || credibility < 0.65f) return D12MapCredibilityBand.Approximate;
            if (stage == D12DiscoveryStage.Verified || credibility >= 0.92f) return D12MapCredibilityBand.Verified;
            return D12MapCredibilityBand.Reliable;
        }

        private static string BuildEvidenceId(string featureId, D12DiscoverySource source, string evidenceRef)
        {
            return "map-evidence:" + SafeId(featureId) + ":" + source.ToString().ToLowerInvariant() + ":" + SafeId(evidenceRef);
        }

        private static string BuildEventId(string featureId, D12DiscoverySource source, string evidenceId)
        {
            return D12MapDiscoveryIds.MapDiscoveryEventPrefix + SafeId(featureId) + "_" +
                source.ToString().ToLowerInvariant() + "_" + SafeId(evidenceId);
        }

        private static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unknown";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-') chars[i] = '_';
            return new string(chars);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static D12DiscoveryResult Reject(string code, string message, string featureId)
        {
            return new D12DiscoveryResult
            {
                Accepted = false,
                Code = code,
                Message = message,
                FeatureId = featureId,
                Stage = D12DiscoveryStage.Unknown,
                CredibilityBand = D12MapCredibilityBand.Uncharted,
            };
        }

        private static D12ToolResult ToolReject(string code, string message, string featureId)
        {
            return new D12ToolResult
            {
                Succeeded = false,
                Code = code,
                Message = message,
                FeatureId = featureId,
                MeasuredDepthMeters = 0f,
            };
        }
    }
}
