// ============================================================
// D12PokerSystem —— 固定种子加勒比五张牌牌局
//
// 牌面、下注、作弊判定、输赢、退出和牌局事实只在本系统写入。金钱、债务、
// 奖励、关系和情报只生成带稳定 request_id 的请求，实际写入仍归属各自 owner。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public sealed class D12PokerSystem
    {
        private readonly EventLog _eventLog;
        private readonly Dictionary<string, D12PokerTableDefinition> _tables =
            new Dictionary<string, D12PokerTableDefinition>(StringComparer.Ordinal);
        private readonly D12PokerRuntimeSnapshot _snapshot;

        public D12PokerSystem(IEnumerable<D12PokerTableDefinition> definitions, EventLog eventLog)
            : this(definitions, eventLog, null)
        {
        }

        public D12PokerSystem(IEnumerable<D12PokerTableDefinition> definitions,
            EventLog eventLog, D12PokerRuntimeSnapshot savedSnapshot)
        {
            if (definitions == null) throw new ArgumentNullException("definitions");
            _eventLog = eventLog ?? new EventLog();
            foreach (var source in definitions)
            {
                if (source == null) throw new InvalidOperationException("牌局目录不能包含 null 桌。");
                var definition = source.Clone();
                definition.Validate();
                if (_tables.ContainsKey(definition.TableId))
                    throw new InvalidOperationException("牌局目录重复：" + definition.TableId);
                _tables.Add(definition.TableId, definition);
            }

            _snapshot = savedSnapshot == null
                ? new D12PokerRuntimeSnapshot()
                : savedSnapshot.Clone();
            _snapshot.Validate();
        }

        public EventLog EventLog { get { return _eventLog; } }

        /// <summary>返回独立副本；表现层不能通过快照引用改牌局状态。</summary>
        public D12PokerRuntimeSnapshot Snapshot { get { return CaptureSnapshot(); } }

        public D12PokerRuntimeSnapshot CaptureSnapshot()
        {
            _snapshot.Validate();
            return _snapshot.Clone();
        }

        public D12PokerTableDefinition GetTable(string tableId)
        {
            D12PokerTableDefinition table;
            return _tables.TryGetValue(tableId ?? string.Empty, out table) ? table.Clone() : null;
        }

        public D12PokerSessionState GetSession(string tableId)
        {
            var session = FindSession(tableId);
            return session == null ? null : session.Clone();
        }

        public IReadOnlyList<D12PokerSessionState> GetSessionReadModel()
        {
            var result = new List<D12PokerSessionState>();
            foreach (var session in _snapshot.Sessions)
                if (session != null) result.Add(session.Clone());
            return result;
        }

        public D12PokerActionResult Apply(D12PokerActionRequest request)
        {
            if (request == null) return D12PokerActionResult.Rejected("invalid_request", "牌局动作不能为空。");
            if (!Enum.IsDefined(typeof(D12PokerAction), request.Action) || request.Action == D12PokerAction.Unknown)
                return D12PokerActionResult.Rejected("unknown_action", "未知牌局动作不能执行。");
            if (string.IsNullOrWhiteSpace(request.TableId) || string.IsNullOrWhiteSpace(request.PlayerId))
                return D12PokerActionResult.Rejected("missing_identity", "牌局动作必须提供 table_id 和 player_id。");
            if (request.WorldDay < 0 || request.SaveRevision < 0)
                return D12PokerActionResult.Rejected("invalid_revision", "牌局日期或存档修订号不能为负数。");

            D12PokerTableDefinition table;
            if (!_tables.TryGetValue(request.TableId, out table))
                return D12PokerActionResult.Rejected("table_not_found", "牌局桌不存在。");
            if (!string.IsNullOrWhiteSpace(request.LocationRef) &&
                !string.Equals(request.LocationRef, table.LocationRef, StringComparison.Ordinal))
                return D12PokerActionResult.Rejected("wrong_location", "不在该牌桌所属地点，不能执行牌局动作。");

            switch (request.Action)
            {
                case D12PokerAction.Enter: return Enter(table, request);
                case D12PokerAction.PlaceBet: return PlaceBet(table, request);
                case D12PokerAction.AttemptCheat: return AttemptCheat(table, request);
                case D12PokerAction.Resolve: return Resolve(table, request);
                case D12PokerAction.Exit: return Exit(table, request);
                default: return D12PokerActionResult.Rejected("unknown_action", "未知牌局动作不能执行。");
            }
        }

        /// <summary>
        /// 经济/奖励 owner 在处理完本局请求后可声明牌局结算。EventLog 的结算门
        /// 会在保存和读档后继续拒绝第二次声明；本方法不直接写金额或奖励。
        /// </summary>
        public D12PokerActionResult ClaimSettlement(string gameId, long saveRevision)
        {
            var record = FindRecord(gameId);
            if (record == null) return D12PokerActionResult.Rejected("record_not_found", "找不到可声明的牌局记录。");
            if (record.SettlementClaimed)
            {
                var duplicateRecord = ResultFromRecord("duplicate_settlement", "该牌局记录已经声明过，不能再次发放。", record,
                    _eventLog.Find(record.SourceEventId, saveRevision));
                duplicateRecord.Accepted = false;
                return duplicateRecord;
            }
            var claim = _eventLog.ClaimSettlement(record.SourceEventId, saveRevision);
            if (claim.IsDuplicate)
            {
                var duplicateClaim = ResultFromRecord("duplicate_settlement", "该牌局结算已经声明过，不能再次发放。", record, claim.Fact);
                duplicateClaim.Accepted = false;
                return duplicateClaim;
            }
            record.SettlementClaimed = true;
            var claimKey = Key(record.SourceEventId, saveRevision);
            if (!_snapshot.AppliedEventKeys.Contains(claimKey)) _snapshot.AppliedEventKeys.Add(claimKey);
            return ResultFromRecord("settlement_claimed", "牌局结算已声明，等待所属 owner 应用请求。", record, claim.Fact);
        }

        private D12PokerActionResult Enter(D12PokerTableDefinition table, D12PokerActionRequest request)
        {
            var existing = FindSession(table.TableId);
            if (existing != null)
                return D12PokerActionResult.Rejected(
                    existing.Status == D12PokerTableStatus.InHand ? "already_in_hand" : "table_session_closed",
                    existing.Status == D12PokerTableStatus.InHand ? "该牌桌已有未结束牌局。" : "该牌桌本次牌局已经结束。" );

            var economy = NormalizeEconomy(request.Economy);
            long proposedDebt;
            long incrementalDebt;
            string fundingError;
            if (!TryFund(table.EntryFee, table, economy, 0, out proposedDebt, out incrementalDebt, out fundingError))
                return D12PokerActionResult.Rejected(fundingError, "入场费无法由当前余额或允许的债务承担。");

            long sequence = ++_snapshot.NextGameSequence;
            string gameId = "poker_game_" + SafeId(table.TableId) + "_" + sequence.ToString(CultureInfo.InvariantCulture);
            var hands = Deal(table.FixedSeed, gameId);
            var session = new D12PokerSessionState
            {
                TableId = table.TableId,
                PlayerId = request.PlayerId,
                GameId = gameId,
                RulesetId = table.RulesetId,
                FixedSeed = table.FixedSeed,
                EntryFee = table.EntryFee,
                BetCap = table.BetCap,
                ProposedDebt = proposedDebt,
                Status = D12PokerTableStatus.InHand,
                Outcome = D12PokerOutcome.Unknown,
                CheatRisk = D12PokerCheatRisk.None,
                WorldDay = request.WorldDay,
                SaveRevision = request.SaveRevision,
            };
            session.PlayerHand.AddRange(hands.Item1);
            session.HouseHand.AddRange(hands.Item2);

            string eventId = EventId("enter", gameId);
            var append = AppendFact(eventId, table, request, request.PlayerId,
                "poker_entered", "牌局已进入；牌面由固定种子生成并写入快照。", new Dictionary<string, string>
                {
                    ["game_id"] = gameId,
                    ["fixed_seed"] = table.FixedSeed.ToString(CultureInfo.InvariantCulture),
                    ["entry_fee"] = table.EntryFee.ToString(CultureInfo.InvariantCulture),
                    ["bet_cap"] = table.BetCap.ToString(CultureInfo.InvariantCulture),
                });
            if (append.IsDuplicate)
                return D12PokerActionResult.Rejected("duplicate_action", "该入局事实已经存在，但当前快照没有可安全恢复的会话。");

            session.LastEventId = eventId;
            _snapshot.Sessions.Add(session);
            MarkApplied(eventId, request.SaveRevision, request.WorldDay);
            var economyRequest = AddEconomyRequest(session, eventId, "entry_fee", -table.EntryFee);
            var debtRequest = AddDebtRequestIfNeeded(session, eventId, request, table, incrementalDebt);
            return Result("entered", "牌局已进入；入场费已生成 Economy 请求。", append.Fact, session,
                economyRequest, debtRequest, null, null, null);
        }

        private D12PokerActionResult PlaceBet(D12PokerTableDefinition table, D12PokerActionRequest request)
        {
            var session = FindSession(table.TableId);
            var valid = RequireInHand(session, request.PlayerId);
            if (valid != null) return valid;
            if (request.Amount <= 0) return D12PokerActionResult.Rejected("invalid_bet", "下注金额必须大于零。");
            if (request.Amount > table.BetCap || session.CurrentBet > table.BetCap - request.Amount)
                return D12PokerActionResult.Rejected("bet_cap_exceeded", "下注不能超过本桌下注上限。");

            long nextBet = session.CurrentBet + request.Amount;
            var economy = NormalizeEconomy(request.Economy);
            long proposedDebt;
            long incrementalDebt;
            string fundingError;
            if (!TryFund(table.EntryFee + nextBet, table, economy, session.ProposedDebt,
                out proposedDebt, out incrementalDebt, out fundingError))
                return D12PokerActionResult.Rejected(fundingError, "下注后本金和入场费超出余额/债务上限。");

            string eventId = EventId("bet_" + nextBet.ToString(CultureInfo.InvariantCulture), session.GameId);
            var append = AppendFact(eventId, table, request, new[] { request.PlayerId },
                "poker_bet_placed", "下注已记录；金额仍等待 EconomySystem 应用。", new Dictionary<string, string>
                {
                    ["game_id"] = session.GameId,
                    ["bet"] = nextBet.ToString(CultureInfo.InvariantCulture),
                    ["bet_cap"] = table.BetCap.ToString(CultureInfo.InvariantCulture),
                });
            if (append.IsDuplicate)
                return ResultFromSession("duplicate_action", "该下注事实已存在。", session, append.Fact);

            session.CurrentBet = nextBet;
            session.ProposedDebt = proposedDebt;
            session.LastEventId = eventId;
            MarkApplied(eventId, request.SaveRevision, request.WorldDay);
            var economyRequest = AddEconomyRequest(session, eventId, "stake_commit", -request.Amount);
            var debtRequest = AddDebtRequestIfNeeded(session, eventId, request, table, incrementalDebt);
            return Result("bet_placed", "下注已记录，尚未由牌局系统直接扣钱。", append.Fact, session,
                economyRequest, debtRequest, null, null, null);
        }

        private D12PokerActionResult AttemptCheat(D12PokerTableDefinition table, D12PokerActionRequest request)
        {
            var session = FindSession(table.TableId);
            var valid = RequireInHand(session, request.PlayerId);
            if (valid != null) return valid;
            if (session.CheatAttempted)
                return D12PokerActionResult.Rejected("cheat_already_attempted", "同一局不能重复尝试作弊。");

            int threshold = table.CheatRiskPercent;
            if (table.BetCap > 0) threshold += (int)Math.Min(20L, session.CurrentBet * 20L / table.BetCap);
            threshold = Math.Max(0, Math.Min(100, threshold));
            int roll = (int)(StableHash(session.FixedSeed.ToString(CultureInfo.InvariantCulture) + ":" + session.GameId + ":cheat") % 100U);
            string eventId = EventId("cheat", session.GameId);
            bool exposed = roll < threshold;
            var append = AppendFact(eventId, table, request, request.PlayerId,
                exposed ? "poker_cheat_exposed" : "poker_cheat_unseen",
                exposed ? "作弊尝试被发现，牌局直接结束。" : "作弊尝试暂未被发现，但风险事实已记录。",
                new Dictionary<string, string>
                {
                    ["game_id"] = session.GameId,
                    ["risk_percent"] = threshold.ToString(CultureInfo.InvariantCulture),
                    ["risk_roll"] = roll.ToString(CultureInfo.InvariantCulture),
                    ["exposed"] = exposed ? "true" : "false",
                }, exposed ? WorldEventStatus.Normal : WorldEventStatus.Normal);
            if (append.IsDuplicate)
                return ResultFromSession("duplicate_action", "该作弊事实已存在。", session, append.Fact);

            session.CheatAttempted = true;
            session.CheatRisk = exposed ? D12PokerCheatRisk.Exposed : D12PokerCheatRisk.Suspected;
            session.LastEventId = eventId;
            MarkApplied(eventId, request.SaveRevision, request.WorldDay);
            var relation = AddRelationRequest(session, table, eventId,
                exposed ? -8 : -1, exposed ? "作弊被发现，信任和声誉受损。" : "未被发现的作弊尝试仍留下怀疑线索。");
            var information = AddInformationRequest(session, table, eventId,
                exposed ? "cheat_exposed" : "cheat_attempt_unseen", exposed ? 1f : 0.55f);
            if (exposed)
            {
                var record = FinalizeRecord(session, table, request, eventId, D12PokerOutcome.Cheated, 0);
                session.Status = D12PokerTableStatus.Cheated;
                session.Outcome = D12PokerOutcome.Cheated;
                return Result("cheat_exposed", "作弊被发现，已记录失败结算与社会后果。", append.Fact, session,
                    null, null, null, relation, information, record);
            }
            return Result("cheat_unseen", "作弊暂未暴露，但风险和怀疑已进入事实链。", append.Fact, session,
                null, null, null, relation, information);
        }

        private D12PokerActionResult Resolve(D12PokerTableDefinition table, D12PokerActionRequest request)
        {
            var session = FindSession(table.TableId);
            var valid = RequireInHand(session, request.PlayerId);
            if (valid != null) return valid;

            string eventId = EventId("resolve", session.GameId);
            var playerScore = Evaluate(session.PlayerHand);
            var houseScore = Evaluate(session.HouseHand);
            int comparison = Compare(playerScore, houseScore);
            var outcome = comparison > 0 ? D12PokerOutcome.Win : comparison < 0 ? D12PokerOutcome.Loss : D12PokerOutcome.Push;
            long payout = outcome == D12PokerOutcome.Win ? SafeMultiply(session.CurrentBet, 2) :
                outcome == D12PokerOutcome.Push ? session.CurrentBet : 0;
            var append = AppendFact(eventId, table, request, request.PlayerId,
                OutcomeCode(outcome), "牌局已按固定牌面结算；经济、奖励、关系和情报由各 owner 读取请求。",
                new Dictionary<string, string>
                {
                    ["game_id"] = session.GameId,
                    ["outcome"] = outcome.ToString(),
                    ["player_category"] = playerScore.Category.ToString(),
                    ["house_category"] = houseScore.Category.ToString(),
                    ["payout"] = payout.ToString(CultureInfo.InvariantCulture),
                    ["cheat_risk"] = session.CheatRisk.ToString(),
                });
            if (append.IsDuplicate)
                return ResultFromSession("duplicate_settlement", "该牌局已经有结算事实，不能重复领奖。", session, append.Fact);

            session.Status = D12PokerTableStatus.Settled;
            session.Outcome = outcome;
            session.LastEventId = eventId;
            MarkApplied(eventId, request.SaveRevision, request.WorldDay);
            var economyRequest = payout == 0 ? null : AddRewardRequest(session, eventId, payout, outcome);
            var relationDelta = outcome == D12PokerOutcome.Win ? 2 : outcome == D12PokerOutcome.Loss ? -2 : 0;
            var relation = AddRelationRequest(session, table, eventId, relationDelta,
                outcome == D12PokerOutcome.Win ? "公平赢牌提高尊重。" : outcome == D12PokerOutcome.Loss ? "输牌和风险承担降低信任。" : "平局未改变关系。", relationDelta == 0);
            var information = AddInformationRequest(session, table, eventId,
                outcome == D12PokerOutcome.Win ? "opponent_tell_confirmed" : outcome == D12PokerOutcome.Loss ? "opponent_tell_missed" : "table_pattern_uncertain",
                outcome == D12PokerOutcome.Win ? 0.82f : 0.45f);
            var record = FinalizeRecord(session, table, request, eventId, outcome, payout, playerScore.Category, houseScore.Category);
            return Result("settled", "牌局已结算；结果和请求已经保存。", append.Fact, session,
                null, null, economyRequest, relation, information, record);
        }

        private D12PokerActionResult Exit(D12PokerTableDefinition table, D12PokerActionRequest request)
        {
            var session = FindSession(table.TableId);
            var valid = RequireInHand(session, request.PlayerId);
            if (valid != null) return valid;

            string eventId = EventId("exit", session.GameId);
            var append = AppendFact(eventId, table, request, request.PlayerId,
                "poker_exited", "玩家退出牌局；已提交的入场费和下注不自动退款。", new Dictionary<string, string>
                {
                    ["game_id"] = session.GameId,
                    ["bet_lost"] = session.CurrentBet.ToString(CultureInfo.InvariantCulture),
                });
            if (append.IsDuplicate)
                return ResultFromSession("duplicate_settlement", "该退出事实已经存在。", session, append.Fact);

            session.Status = D12PokerTableStatus.Exited;
            session.Outcome = D12PokerOutcome.Fold;
            session.LastEventId = eventId;
            MarkApplied(eventId, request.SaveRevision, request.WorldDay);
            var relation = AddRelationRequest(session, table, eventId, -1, "中途退出牌局留下轻微失约印象。");
            var information = AddInformationRequest(session, table, eventId, "exit_before_showdown", 0.65f);
            var record = FinalizeRecord(session, table, request, eventId, D12PokerOutcome.Fold, 0);
            return Result("exited", "已退出牌局；不返还已提交的入场费或下注。", append.Fact, session,
                null, null, null, relation, information, record);
        }

        private D12PokerActionResult RequireInHand(D12PokerSessionState session, string playerId)
        {
            if (session == null) return D12PokerActionResult.Rejected("session_not_found", "该牌桌没有进行中的牌局。");
            if (!string.Equals(session.PlayerId, playerId, StringComparison.Ordinal))
                return D12PokerActionResult.Rejected("player_mismatch", "牌局玩家与请求玩家不一致。");
            if (session.Status != D12PokerTableStatus.InHand)
                return D12PokerActionResult.Rejected("session_closed", "该牌局已经结束，不能再次改变结果。");
            return null;
        }

        private bool TryFund(long totalCost, D12PokerTableDefinition table,
            D12PokerEconomyReadSnapshot economy, long priorProposedDebt,
            out long proposedDebt, out long incrementalDebt, out string error)
        {
            proposedDebt = 0;
            incrementalDebt = 0;
            error = "";
            if (totalCost < 0 || economy.AvailableMoney < 0 || economy.CurrentDebt < 0)
            {
                error = "invalid_money_snapshot";
                return false;
            }
            if (totalCost <= economy.AvailableMoney) return true;
            proposedDebt = totalCost - economy.AvailableMoney;
            long effectiveLimit = table.DebtLimit > 0 ? table.DebtLimit : economy.DebtLimit;
            if (!economy.CanBorrow || effectiveLimit <= 0 || economy.CurrentDebt > effectiveLimit ||
                proposedDebt > effectiveLimit - economy.CurrentDebt)
            {
                error = "debt_limit_exceeded";
                return false;
            }
            incrementalDebt = proposedDebt > priorProposedDebt ? proposedDebt - priorProposedDebt : 0;
            return true;
        }

        private D12PokerEconomyReadSnapshot NormalizeEconomy(D12PokerEconomyReadSnapshot economy)
        {
            return economy == null ? new D12PokerEconomyReadSnapshot() : economy;
        }

        private D12PokerSessionState FindSession(string tableId)
        {
            foreach (var session in _snapshot.Sessions)
                if (session != null && string.Equals(session.TableId, tableId, StringComparison.Ordinal)) return session;
            return null;
        }

        private D12PokerGameRecord FindRecord(string gameId)
        {
            foreach (var record in _snapshot.Records)
                if (record != null && string.Equals(record.GameId, gameId, StringComparison.Ordinal)) return record;
            return null;
        }

        private EventAppendResult AppendFact(string eventId, D12PokerTableDefinition table,
            D12PokerActionRequest request, string actorId, string resultCode, string summary,
            Dictionary<string, string> values, WorldEventStatus status = WorldEventStatus.Normal)
        {
            return AppendFact(eventId, table, request, new[] { actorId }, resultCode, summary, values, status);
        }

        private EventAppendResult AppendFact(string eventId, D12PokerTableDefinition table,
            D12PokerActionRequest request, IEnumerable<string> actorIds, string resultCode, string summary,
            Dictionary<string, string> values, WorldEventStatus status = WorldEventStatus.Normal)
        {
            return _eventLog.Append(new WorldEvent
            {
                EventId = eventId,
                WorldDay = request.WorldDay,
                LocationRef = table.LocationRef,
                ActorIds = actorIds == null ? new List<string>() : new List<string>(actorIds),
                InputFacts = new List<string> { "table:" + table.TableId, "ruleset:" + table.RulesetId },
                OwnerSystem = D12PokerIds.OwnerSystem,
                Result = new EventResult
                {
                    ResultCode = resultCode,
                    Summary = summary,
                    FactIds = new List<string> { "poker:" + eventId },
                    Values = values ?? new Dictionary<string, string>(),
                },
                SaveRevision = request.SaveRevision,
                OutcomeStatus = status,
            });
        }

        private void MarkApplied(string eventId, long saveRevision, int worldDay)
        {
            var key = Key(eventId, saveRevision);
            if (!_snapshot.AppliedEventKeys.Contains(key)) _snapshot.AppliedEventKeys.Add(key);
            _snapshot.LastEventId = eventId;
            _snapshot.LastWorldDay = Math.Max(_snapshot.LastWorldDay, worldDay);
            _snapshot.Revision = Math.Max(_snapshot.Revision, saveRevision);
        }

        private D12PokerEconomyRequest AddEconomyRequest(D12PokerSessionState session,
            string sourceEventId, string kind, long amount)
        {
            var request = new D12PokerEconomyRequest
            {
                RequestId = session.GameId + ":economy:" + kind + ":" + sourceEventId,
                GameId = session.GameId,
                SourceEventId = sourceEventId,
                PlayerId = session.PlayerId,
                Kind = kind,
                Amount = amount,
            };
            AddUnique(_snapshot.EconomyRequests, request, delegate(D12PokerEconomyRequest value) { return value.RequestId; });
            return request.Clone();
        }

        private D12PokerDebtRequest AddDebtRequestIfNeeded(D12PokerSessionState session,
            string sourceEventId, D12PokerActionRequest action, D12PokerTableDefinition table, long incrementalDebt)
        {
            if (incrementalDebt <= 0) return null;
            var request = new D12PokerDebtRequest
            {
                RequestId = session.GameId + ":debt:" + sourceEventId,
                GameId = session.GameId,
                SourceEventId = sourceEventId,
                PlayerId = session.PlayerId,
                Principal = incrementalDebt,
                CurrentDebt = NormalizeEconomy(action.Economy).CurrentDebt,
                DebtLimit = table.DebtLimit > 0 ? table.DebtLimit : NormalizeEconomy(action.Economy).DebtLimit,
            };
            AddUnique(_snapshot.DebtRequests, request, delegate(D12PokerDebtRequest value) { return value.RequestId; });
            return request.Clone();
        }

        private D12PokerRewardRequest AddRewardRequest(D12PokerSessionState session,
            string sourceEventId, long payout, D12PokerOutcome outcome)
        {
            var request = new D12PokerRewardRequest
            {
                RequestId = session.GameId + ":reward",
                RewardId = "reward_poker_win:" + session.GameId,
                GameId = session.GameId,
                SourceEventId = sourceEventId,
                PlayerId = session.PlayerId,
                RewardTag = outcome == D12PokerOutcome.Push ? "poker_push_refund" : "poker_win_payout",
                CurrencyAmount = payout,
            };
            AddUnique(_snapshot.RewardRequests, request, delegate(D12PokerRewardRequest value) { return value.RequestId; });
            return request.Clone();
        }

        private D12PokerRelationImpactRequest AddRelationRequest(D12PokerSessionState session,
            D12PokerTableDefinition table, string sourceEventId, int delta, string reason, bool skip = false)
        {
            if (skip || string.IsNullOrWhiteSpace(table.RelationTargetId)) return null;
            var request = new D12PokerRelationImpactRequest
            {
                RequestId = session.GameId + ":relation:" + sourceEventId,
                SourceEventId = sourceEventId,
                SourceId = session.PlayerId,
                TargetId = table.RelationTargetId,
                ProposedDelta = delta,
                Reason = reason,
            };
            AddUnique(_snapshot.RelationImpactRequests, request, delegate(D12PokerRelationImpactRequest value) { return value.RequestId; });
            return request.Clone();
        }

        private D12PokerInformationRequest AddInformationRequest(D12PokerSessionState session,
            D12PokerTableDefinition table, string sourceEventId, string code, float reliability)
        {
            var request = new D12PokerInformationRequest
            {
                RequestId = session.GameId + ":information:" + sourceEventId,
                SourceEventId = sourceEventId,
                PlayerId = session.PlayerId,
                InformationPoolId = table.InformationPoolId,
                InformationCode = code,
                Reliability = Math.Max(0f, Math.Min(1f, reliability)),
            };
            AddUnique(_snapshot.InformationRequests, request, delegate(D12PokerInformationRequest value) { return value.RequestId; });
            return request.Clone();
        }

        private D12PokerGameRecord FinalizeRecord(D12PokerSessionState session,
            D12PokerTableDefinition table, D12PokerActionRequest request, string eventId,
            D12PokerOutcome outcome, long payout,
            D12PokerHandCategory playerCategory = D12PokerHandCategory.Unknown,
            D12PokerHandCategory houseCategory = D12PokerHandCategory.Unknown)
        {
            var record = new D12PokerGameRecord
            {
                RecordId = "poker_record:" + session.GameId,
                GameId = session.GameId,
                TableId = table.TableId,
                PlayerId = session.PlayerId,
                SourceEventId = eventId,
                SaveRevision = request.SaveRevision,
                WorldDay = request.WorldDay,
                FixedSeed = session.FixedSeed,
                EntryFee = session.EntryFee,
                Bet = session.CurrentBet,
                Payout = payout,
                NetProposedDelta = payout - session.EntryFee - session.CurrentBet,
                Outcome = outcome,
                PlayerHandCategory = playerCategory,
                HouseHandCategory = houseCategory,
                CheatRisk = session.CheatRisk,
            };
            foreach (var card in session.PlayerHand) record.PlayerCardIds.Add(card.CardId);
            foreach (var card in session.HouseHand) record.HouseCardIds.Add(card.CardId);
            AddUnique(_snapshot.Records, record, delegate(D12PokerGameRecord value) { return value.RecordId; });
            return record.Clone();
        }

        private D12PokerActionResult Result(string code, string message, FactEvent fact,
            D12PokerSessionState session, D12PokerEconomyRequest economy, D12PokerDebtRequest debt,
            D12PokerRewardRequest reward, D12PokerRelationImpactRequest relation,
            D12PokerInformationRequest information, D12PokerGameRecord record = null)
        {
            return new D12PokerActionResult
            {
                Accepted = true,
                Code = code,
                Message = message,
                Fact = fact == null ? null : fact.CloneFact(),
                Session = session == null ? null : session.Clone(),
                Status = session == null ? D12PokerTableStatus.Unknown : session.Status,
                Outcome = session == null ? D12PokerOutcome.Unknown : session.Outcome,
                CheatRisk = session == null ? D12PokerCheatRisk.Unknown : session.CheatRisk,
                EconomyRequest = economy == null ? null : economy.Clone(),
                DebtRequest = debt == null ? null : debt.Clone(),
                RewardRequest = reward == null ? null : reward.Clone(),
                RelationRequest = relation == null ? null : relation.Clone(),
                InformationRequest = information == null ? null : information.Clone(),
                Record = record == null ? null : record.Clone(),
            };
        }

        private D12PokerActionResult ResultFromSession(string code, string message,
            D12PokerSessionState session, FactEvent fact)
        {
            var record = FindRecord(session == null ? null : session.GameId);
            return Result(code, message, fact, session, null, null, null, null, null, record);
        }

        private D12PokerActionResult ResultFromRecord(string code, string message,
            D12PokerGameRecord record, FactEvent fact)
        {
            var session = FindSession(record == null ? null : record.TableId);
            return Result(code, message, fact, session, null, null, null, null, null, record);
        }

        private static void AddUnique<T>(List<T> target, T value, Func<T, string> idSelector)
            where T : class
        {
            var id = idSelector(value);
            foreach (var existing in target)
                if (existing != null && string.Equals(idSelector(existing), id, StringComparison.Ordinal)) return;
            target.Add(value);
        }

        private static string EventId(string action, string gameId)
        {
            return D12PokerIds.EventPrefix + action + "_" + SafeId(gameId);
        }

        private static string Key(string eventId, long revision)
        {
            return (eventId ?? "") + "\u001f" + revision.ToString(CultureInfo.InvariantCulture);
        }

        private static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unknown";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-') chars[i] = '_';
            return new string(chars);
        }

        private static string OutcomeCode(D12PokerOutcome outcome)
        {
            return "poker_" + outcome.ToString().ToLowerInvariant();
        }

        private static long SafeMultiply(long left, long right)
        {
            if (left <= 0 || right <= 0) return 0;
            if (left > long.MaxValue / right) return long.MaxValue;
            return left * right;
        }

        private static Tuple<List<D12PokerCard>, List<D12PokerCard>> Deal(long seed, string gameId)
        {
            var deck = new List<D12PokerCard>();
            foreach (D12PokerCardSuit suit in new[] { D12PokerCardSuit.Clubs, D12PokerCardSuit.Diamonds, D12PokerCardSuit.Hearts, D12PokerCardSuit.Spades })
                foreach (D12PokerCardRank rank in new[] { D12PokerCardRank.Two, D12PokerCardRank.Three, D12PokerCardRank.Four, D12PokerCardRank.Five, D12PokerCardRank.Six, D12PokerCardRank.Seven, D12PokerCardRank.Eight, D12PokerCardRank.Nine, D12PokerCardRank.Ten, D12PokerCardRank.Jack, D12PokerCardRank.Queen, D12PokerCardRank.King, D12PokerCardRank.Ace })
                    deck.Add(new D12PokerCard
                    {
                        CardId = "card_" + rank + "_" + suit,
                        Suit = suit,
                        Rank = rank,
                    });

            uint state = StableHash(seed.ToString(CultureInfo.InvariantCulture) + ":" + gameId);
            for (int i = deck.Count - 1; i > 0; i--)
            {
                state = unchecked(state * 1664525U + 1013904223U);
                int index = (int)(state % (uint)(i + 1));
                var card = deck[i];
                deck[i] = deck[index];
                deck[index] = card;
            }
            var player = new List<D12PokerCard>();
            var house = new List<D12PokerCard>();
            for (int i = 0; i < 5; i++)
            {
                player.Add(deck[i].Clone());
                house.Add(deck[i + 5].Clone());
            }
            return Tuple.Create(player, house);
        }

        private sealed class HandScore
        {
            public D12PokerHandCategory Category;
            public List<int> Tiebreakers = new List<int>();
        }

        private sealed class RankGroup
        {
            public int Rank;
            public int Count;
        }

        private static HandScore Evaluate(List<D12PokerCard> cards)
        {
            var result = new HandScore { Category = D12PokerHandCategory.HighCard };
            if (cards == null || cards.Count != 5) return result;
            var ranks = cards.Select(delegate(D12PokerCard card) { return (int)card.Rank; }).OrderByDescending(delegate(int rank) { return rank; }).ToList();
            var groups = ranks.GroupBy(delegate(int rank) { return rank; }).Select(delegate(IGrouping<int, int> group)
            {
                return new RankGroup { Rank = group.Key, Count = group.Count() };
            }).OrderByDescending(delegate(RankGroup group) { return group.Count; }).ThenByDescending(delegate(RankGroup group) { return group.Rank; }).ToList();
            bool flush = cards.All(delegate(D12PokerCard card) { return card.Suit == cards[0].Suit; });
            bool straight = ranks.Distinct().Count() == 5 && (ranks[0] - ranks[4] == 4 ||
                (ranks[0] == 14 && ranks[1] == 5 && ranks[2] == 4 && ranks[3] == 3 && ranks[4] == 2));
            int straightHigh = ranks[0] == 14 && ranks[1] == 5 ? 5 : ranks[0];

            if (straight && flush) { result.Category = D12PokerHandCategory.StraightFlush; result.Tiebreakers.Add(straightHigh); return result; }
            if (groups[0].Count == 4)
            {
                result.Category = D12PokerHandCategory.FourOfAKind;
                result.Tiebreakers.Add(groups[0].Rank); result.Tiebreakers.Add(groups[1].Rank); return result;
            }
            if (groups[0].Count == 3 && groups[1].Count == 2)
            {
                result.Category = D12PokerHandCategory.FullHouse;
                result.Tiebreakers.Add(groups[0].Rank); result.Tiebreakers.Add(groups[1].Rank); return result;
            }
            if (flush) { result.Category = D12PokerHandCategory.Flush; result.Tiebreakers.AddRange(ranks); return result; }
            if (straight) { result.Category = D12PokerHandCategory.Straight; result.Tiebreakers.Add(straightHigh); return result; }
            if (groups[0].Count == 3)
            {
                result.Category = D12PokerHandCategory.ThreeOfAKind;
                result.Tiebreakers.Add(groups[0].Rank); result.Tiebreakers.AddRange(groups.Skip(1).Select(delegate(RankGroup group) { return group.Rank; })); return result;
            }
            if (groups[0].Count == 2 && groups[1].Count == 2)
            {
                result.Category = D12PokerHandCategory.TwoPair;
                result.Tiebreakers.Add(groups[0].Rank); result.Tiebreakers.Add(groups[1].Rank); result.Tiebreakers.Add(groups[2].Rank); return result;
            }
            if (groups[0].Count == 2)
            {
                result.Category = D12PokerHandCategory.Pair;
                result.Tiebreakers.Add(groups[0].Rank); result.Tiebreakers.AddRange(groups.Skip(1).Select(delegate(RankGroup group) { return group.Rank; })); return result;
            }
            result.Tiebreakers.AddRange(ranks);
            return result;
        }

        private static int Compare(HandScore left, HandScore right)
        {
            if (left.Category != right.Category) return left.Category > right.Category ? 1 : -1;
            int count = Math.Max(left.Tiebreakers.Count, right.Tiebreakers.Count);
            for (int i = 0; i < count; i++)
            {
                int l = i < left.Tiebreakers.Count ? left.Tiebreakers[i] : 0;
                int r = i < right.Tiebreakers.Count ? right.Tiebreakers[i] : 0;
                if (l != r) return l > r ? 1 : -1;
            }
            return 0;
        }

        private static uint StableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261U;
                if (value != null)
                    foreach (char c in value) { hash ^= c; hash *= 16777619U; }
                return hash;
            }
        }
    }
}
