// ============================================================
// D20NpcBehaviorService —— D20-L29 NPC 行为聚合服务（纯 C#）
//
// 该服务聚合角色身份、岗位解释、日程输入、D20 行为投影、交互资格和只读
// 事件所需结果。经济、关系、库存、任务、健康、港口和 Agent Core 仍由各自
// owner 裁决；外部域只能以经 D20-L19 审核的 proposal 形式进入。
// ============================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace Whalebone.Systems.D20
{
    /// <summary>由 D20-NPC 服务消费的外部 owner 只读声明。</summary>
    [Serializable]
    public sealed class D20NpcOwnerProjectionInput
    {
        public string NpcId = string.Empty;
        public D20CharacterAvailability Availability = D20CharacterAvailability.Unknown;
        public bool IsBusy;
        public bool IsRestRequired;
        public bool IsInjured;
        public string SourceOwnerSystem = string.Empty;

        public D20NpcOwnerProjectionInput Clone()
        {
            return new D20NpcOwnerProjectionInput
            {
                NpcId = NpcId ?? string.Empty,
                Availability = Availability,
                IsBusy = IsBusy,
                IsRestRequired = IsRestRequired,
                IsInjured = IsInjured,
                SourceOwnerSystem = SourceOwnerSystem ?? string.Empty,
            };
        }
    }

    /// <summary>NPC 行为命令，行为状态和 owner proposal 分开表达。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorCommand
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public string NpcId = string.Empty;
        public D20NpcBehaviorRequest BehaviorRequest;
        public D20NpcOwnerProjectionInput OwnerProjection;
        public D20NpcBehaviorAuthorityRequest OwnerRequest;
        public D20NpcIdentity Identity;
        public D20NpcInteractionAvailabilityRequest InteractionRequest;

        public static D20NpcBehaviorCommand Create(string commandId,
            D20NpcBehaviorRequest behaviorRequest)
        {
            return new D20NpcBehaviorCommand
            {
                CommandId = commandId ?? string.Empty,
                NpcId = behaviorRequest == null ? string.Empty : behaviorRequest.NpcId ?? string.Empty,
                BehaviorRequest = behaviorRequest == null ? null : behaviorRequest.Clone(),
            };
        }

        public D20NpcBehaviorCommand Clone()
        {
            return new D20NpcBehaviorCommand
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                NpcId = NpcId ?? string.Empty,
                BehaviorRequest = BehaviorRequest == null ? null : BehaviorRequest.Clone(),
                OwnerProjection = OwnerProjection == null ? null : OwnerProjection.Clone(),
                OwnerRequest = OwnerRequest == null ? null : OwnerRequest.Clone(),
                Identity = Identity == null ? null : Identity.Clone(),
                InteractionRequest = InteractionRequest == null
                    ? null : InteractionRequest.Clone(),
            };
        }

        public string Fingerprint()
        {
            var builder = new StringBuilder("d20-npc-command-v1|");
            Append(builder, NpcId);
            if (BehaviorRequest != null)
            {
                Append(builder, BehaviorRequest.CommandId);
                Append(builder, ((int)BehaviorRequest.Kind).ToString());
                Append(builder, BehaviorRequest.ReasonCode);
                if (BehaviorRequest.Schedule != null)
                {
                    Append(builder, BehaviorRequest.Schedule.RoleId);
                    Append(builder, ((int)BehaviorRequest.Schedule.TimeSlot).ToString());
                    Append(builder, ((int)BehaviorRequest.Schedule.ScheduledDuty).ToString());
                    Append(builder, ((int)BehaviorRequest.Schedule.Availability).ToString());
                    Append(builder, BehaviorRequest.Schedule.IsBusy ? "1" : "0");
                    Append(builder, BehaviorRequest.Schedule.IsRestRequired ? "1" : "0");
                    Append(builder, BehaviorRequest.Schedule.IsInjured ? "1" : "0");
                    Append(builder, BehaviorRequest.Schedule.WorldDay.ToString());
                    Append(builder, BehaviorRequest.Schedule.SaveRevision.ToString());
                }
            }
            if (OwnerProjection != null)
            {
                Append(builder, OwnerProjection.SourceOwnerSystem);
                Append(builder, ((int)OwnerProjection.Availability).ToString());
                Append(builder, OwnerProjection.IsBusy ? "1" : "0");
                Append(builder, OwnerProjection.IsRestRequired ? "1" : "0");
                Append(builder, OwnerProjection.IsInjured ? "1" : "0");
            }
            if (OwnerRequest != null)
            {
                Append(builder, ((int)OwnerRequest.Domain).ToString());
                Append(builder, ((int)OwnerRequest.Operation).ToString());
                Append(builder, OwnerRequest.OwnerSystem);
                Append(builder, OwnerRequest.IsProposal ? "1" : "0");
            }
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, string value)
        {
            string text = value ?? string.Empty;
            builder.Append(text.Length).Append(':').Append(text).Append('|');
        }
    }

    /// <summary>NPC 行为服务的只读聚合投影。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorReadModel
    {
        public int SchemaVersion = D20NpcBehaviorCommand.CurrentSchemaVersion;
        public string NpcId = string.Empty;
        public D20NpcIdentity Identity;
        public D20NpcRoleDefinition Role;
        public D20NpcScheduleInput Schedule;
        public D20NpcBehaviorSnapshot Behavior;
        public D20NpcOwnerProjectionInput OwnerProjection;

        public D20NpcBehaviorReadModel Clone()
        {
            return new D20NpcBehaviorReadModel
            {
                SchemaVersion = SchemaVersion,
                NpcId = NpcId ?? string.Empty,
                Identity = Identity == null ? null : Identity.Clone(),
                Role = Role == null ? null : Role.Clone(),
                Schedule = Schedule == null ? null : Schedule.Clone(),
                Behavior = Behavior == null ? null : Behavior.Clone(),
                OwnerProjection = OwnerProjection == null ? null : OwnerProjection.Clone(),
            };
        }
    }

    /// <summary>NPC 行为服务的统一结果。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorServiceResult
    {
        public bool Accepted;
        public bool Succeeded;
        public bool IsDuplicate;
        public string CommandId = string.Empty;
        public string NpcId = string.Empty;
        public string RequestFingerprint = string.Empty;
        public D20NpcBehaviorFailureCode FailureCode;
        public string ReasonCode = string.Empty;
        public D20NpcBehaviorTransitionResult BehaviorResult;
        public D20NpcInteractionAvailabilityResult InteractionResult;
        public D20NpcBehaviorAuthorityResult AuthorityResult;
        public D20NpcBehaviorReadModel Projection;

        public D20NpcBehaviorServiceResult Clone()
        {
            return new D20NpcBehaviorServiceResult
            {
                Accepted = Accepted,
                Succeeded = Succeeded,
                IsDuplicate = IsDuplicate,
                CommandId = CommandId ?? string.Empty,
                NpcId = NpcId ?? string.Empty,
                RequestFingerprint = RequestFingerprint ?? string.Empty,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                BehaviorResult = BehaviorResult == null ? null : BehaviorResult.Clone(),
                InteractionResult = InteractionResult == null ? null : InteractionResult.Clone(),
                AuthorityResult = AuthorityResult == null ? null : AuthorityResult.Clone(),
                Projection = Projection == null ? null : Projection.Clone(),
            };
        }
    }

    /// <summary>
    /// D20-L29 NPC 行为聚合服务。它是 D20 行为投影的编排入口，但不是任何
    /// 外部事实 owner 的替代品。
    /// </summary>
    public sealed class D20NpcBehaviorService
    {
        public const string OwnerSystem = "D20NpcBehaviorService";

        private readonly Dictionary<string, D20NpcBehaviorStateMachine> _behaviors =
            new Dictionary<string, D20NpcBehaviorStateMachine>(StringComparer.Ordinal);
        private readonly Dictionary<string, D20NpcIdentity> _identities =
            new Dictionary<string, D20NpcIdentity>(StringComparer.Ordinal);
        private readonly Dictionary<string, D20NpcScheduleInput> _schedules =
            new Dictionary<string, D20NpcScheduleInput>(StringComparer.Ordinal);
        private readonly Dictionary<string, D20NpcOwnerProjectionInput> _owners =
            new Dictionary<string, D20NpcOwnerProjectionInput>(StringComparer.Ordinal);
        private readonly Dictionary<string, D20NpcBehaviorServiceResult> _receipts =
            new Dictionary<string, D20NpcBehaviorServiceResult>(StringComparer.Ordinal);

        /// <summary>注册身份和可选的 D20 行为保存投影。</summary>
        public void Register(D20NpcIdentity identity,
            D20NpcBehaviorSnapshot initialSnapshot = null)
        {
            if (identity == null) throw new ArgumentNullException("identity");
            identity.Validate();
            string npcId = identity.NpcId;
            if (initialSnapshot != null)
            {
                var snapshot = initialSnapshot.Clone();
                snapshot.Validate();
                if (!string.Equals(snapshot.NpcId, npcId, StringComparison.Ordinal))
                    throw new InvalidOperationException("NPC identity and behavior IDs must match.");
                _behaviors[npcId] = new D20NpcBehaviorStateMachine(snapshot);
            }
            else if (!_behaviors.ContainsKey(npcId))
            {
                _behaviors[npcId] = new D20NpcBehaviorStateMachine();
            }
            _identities[npcId] = identity.Clone();
        }

        /// <summary>应用 D20 行为请求；外部 owner 只作为 proposal/只读输入。</summary>
        public D20NpcBehaviorServiceResult Execute(D20NpcBehaviorCommand command)
        {
            if (command == null)
                return Failure(string.Empty, string.Empty, "npc_behavior_command_missing");
            if (command.SchemaVersion != D20NpcBehaviorCommand.CurrentSchemaVersion)
                return Failure(command.CommandId, command.NpcId,
                    "npc_behavior_command_schema_version_mismatch");
            if (!D20NpcIdentityContracts.IsStableReference(command.CommandId, false))
                return Failure(command.CommandId, command.NpcId,
                    "npc_behavior_command_id_missing");

            string fingerprint = command.Fingerprint();
            D20NpcBehaviorServiceResult previous;
            if (_receipts.TryGetValue(command.CommandId, out previous))
            {
                if (!string.Equals(previous.RequestFingerprint, fingerprint,
                    StringComparison.Ordinal))
                    return Failure(command.CommandId, command.NpcId,
                        "npc_behavior_command_id_conflict");
                var duplicate = previous.Clone();
                duplicate.IsDuplicate = true;
                duplicate.ReasonCode = "npc_behavior_command_replayed";
                return duplicate;
            }

            if (command.OwnerRequest != null)
            {
                var authority = D20NpcBehaviorAuthorityGuard.Evaluate(command.OwnerRequest);
                if (authority == null || !authority.Allowed)
                {
                    var denied = Failure(command.CommandId, command.NpcId,
                        authority == null ? "npc_behavior_authority_result_missing" : authority.ReasonCode);
                    denied.AuthorityResult = authority == null ? null : authority.Clone();
                    denied.RequestFingerprint = fingerprint;
                    Store(command.CommandId, denied);
                    return denied.Clone();
                }
            }

            var result = ApplyBehavior(command.BehaviorRequest, command.Identity,
                command.OwnerProjection, command.InteractionRequest);
            result.CommandId = command.CommandId;
            result.RequestFingerprint = fingerprint;
            if (command.OwnerRequest != null)
                result.AuthorityResult = D20NpcBehaviorAuthorityGuard.Evaluate(command.OwnerRequest);
            Store(command.CommandId, result);
            return result.Clone();
        }

        /// <summary>直接应用一个已发布 D20 行为请求的便捷入口。</summary>
        public D20NpcBehaviorServiceResult Apply(D20NpcBehaviorRequest request)
        {
            return Execute(D20NpcBehaviorCommand.Create(
                request == null ? string.Empty : request.CommandId, request));
        }

        /// <summary>按日程推进一个 NPC，命令 ID 由既有状态机确定性生成。</summary>
        public D20NpcBehaviorServiceResult Tick(D20NpcScheduleInput schedule)
        {
            if (schedule == null)
                return Failure(string.Empty, string.Empty, "npc_behavior_schedule_missing");
            string commandId = "npc-tick-" + schedule.NpcId + "-" +
                schedule.WorldDay + "-" + schedule.SaveRevision + "-" +
                (int)schedule.TimeSlot;
            return Execute(D20NpcBehaviorCommand.Create(commandId,
                D20NpcBehaviorRequest.Create(
                    D20NpcBehaviorRequestKind.SynchronizeSchedule,
                    commandId, schedule)));
        }

        /// <summary>只评估交互资格，不写 NPC 行为或外部 owner 事实。</summary>
        public D20NpcInteractionAvailabilityResult EvaluateInteraction(
            D20NpcInteractionAvailabilityRequest request)
        {
            return D20NpcInteractionAvailability.Evaluate(request);
        }

        /// <summary>读取指定 NPC 的防御性聚合投影。</summary>
        public D20NpcBehaviorReadModel Read(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return null;
            D20NpcBehaviorStateMachine behavior;
            if (!_behaviors.TryGetValue(npcId, out behavior)) return null;
            D20NpcBehaviorReadModel result = new D20NpcBehaviorReadModel
            {
                NpcId = npcId,
                Behavior = behavior.CaptureSnapshot(),
            };
            D20NpcIdentity identity;
            if (_identities.TryGetValue(npcId, out identity)) result.Identity = identity.Clone();
            D20NpcScheduleInput schedule;
            if (_schedules.TryGetValue(npcId, out schedule)) result.Schedule = schedule.Clone();
            D20NpcOwnerProjectionInput owner;
            if (_owners.TryGetValue(npcId, out owner)) result.OwnerProjection = owner.Clone();
            if (result.Identity != null)
            {
                D20NpcRoleDefinition role;
                if (D20NpcRoleCatalog.TryGetRole(result.Identity.RoleId, out role))
                    result.Role = role.Clone();
            }
            return result;
        }

        /// <summary>捕获按稳定 ID 排序的 D20 行为快照。</summary>
        public List<D20NpcBehaviorSnapshot> CaptureBehaviorSnapshots()
        {
            var ids = new List<string>(_behaviors.Keys);
            ids.Sort(StringComparer.Ordinal);
            var result = new List<D20NpcBehaviorSnapshot>();
            foreach (string id in ids)
                result.Add(_behaviors[id].CaptureSnapshot());
            return result;
        }

        private D20NpcBehaviorServiceResult ApplyBehavior(
            D20NpcBehaviorRequest request,
            D20NpcIdentity identity,
            D20NpcOwnerProjectionInput owner,
            D20NpcInteractionAvailabilityRequest interactionRequest)
        {
            if (request == null)
                return Failure(string.Empty, string.Empty, "npc_behavior_request_missing");
            if (identity != null)
            {
                identity.Validate();
                if (!string.Equals(identity.NpcId, request.NpcId, StringComparison.Ordinal))
                    return Failure(request.CommandId, request.NpcId, "npc_behavior_identity_mismatch");
                Register(identity);
            }

            D20NpcBehaviorStateMachine machine;
            if (!_behaviors.TryGetValue(request.NpcId, out machine))
                machine = new D20NpcBehaviorStateMachine();
            var behaviorResult = machine.Apply(request);
            var result = new D20NpcBehaviorServiceResult
            {
                Accepted = behaviorResult != null && behaviorResult.Accepted,
                Succeeded = behaviorResult != null && behaviorResult.Succeeded,
                CommandId = request.CommandId ?? string.Empty,
                NpcId = request.NpcId ?? string.Empty,
                FailureCode = behaviorResult == null
                    ? D20NpcBehaviorFailureCode.InvalidRequest : behaviorResult.FailureCode,
                ReasonCode = behaviorResult == null
                    ? "npc_behavior_result_missing" : behaviorResult.ReasonCode,
                BehaviorResult = behaviorResult == null ? null : behaviorResult.Clone(),
            };
            if (interactionRequest != null)
                result.InteractionResult = D20NpcInteractionAvailability.Evaluate(interactionRequest);

            if (result.Accepted)
            {
                _behaviors[request.NpcId] = machine;
                if (request.Schedule != null)
                    _schedules[request.NpcId] = request.Schedule.Clone();
                if (owner != null)
                    _owners[request.NpcId] = owner.Clone();
                result.Projection = Read(request.NpcId);
            }
            return result;
        }

        private void Store(string commandId, D20NpcBehaviorServiceResult result)
        {
            if (D20NpcIdentityContracts.IsStableReference(commandId, false))
                _receipts[commandId] = result.Clone();
        }

        private static D20NpcBehaviorServiceResult Failure(string commandId,
            string npcId, string reasonCode)
        {
            return new D20NpcBehaviorServiceResult
            {
                Accepted = false,
                Succeeded = false,
                CommandId = commandId ?? string.Empty,
                NpcId = npcId ?? string.Empty,
                FailureCode = D20NpcBehaviorFailureCode.InvalidRequest,
                ReasonCode = reasonCode ?? string.Empty,
            };
        }
    }
}
