// ============================================================
// D20CharacterCommandService —— D20-L28 角色命令统一入口（纯 C#）
//
// 本文件只编排已经发布的 D20 投影与 owner 公共 API。身份、presence 和动作
// 投影由 D20 自有系统写入；D10/D11/D19 的事实不在此复制或裁决。港口命令
// 必须经过注入的 D19PortCommandRouter.Route(D19RoutedPortCommand)。
// ============================================================

using System;
using System.Collections.Generic;
using System.Text;
using Whalebone.Systems.D19;

namespace Whalebone.Systems.D20
{
    /// <summary>D20-L28 角色命令类别。</summary>
    public enum D20CharacterCommandKind
    {
        None = 0,
        UpdatePresence = 1,
        StartAction = 2,
        InterruptAction = 3,
        CompleteAction = 4,
        SelectAppearance = 5,
        EvaluateNpcInteraction = 6,
        RoutePortCommand = 7,
    }

    /// <summary>D20-L28 统一命令结果状态。</summary>
    public enum D20CharacterCommandStatus
    {
        Rejected = 0,
        Accepted = 1,
        Succeeded = 2,
        Duplicate = 3,
        Unavailable = 4,
    }

    /// <summary>D20-L28 稳定命令失败码。</summary>
    public enum D20CharacterCommandFailureCode
    {
        None = 0,
        MissingRequest = 1,
        InvalidSchemaVersion = 2,
        MissingCommandId = 3,
        DuplicateCommandConflict = 4,
        InvalidCommandKind = 5,
        MissingActorId = 6,
        IdentityInvalid = 7,
        IdentityMismatch = 8,
        PresenceRequestMissing = 9,
        PresenceRejected = 10,
        ActionIntentMissing = 11,
        ActionIntentInvalid = 12,
        ActionActorMismatch = 13,
        ConditionRejected = 14,
        PriorityRejected = 15,
        ActionRejected = 16,
        InterruptRequestMissing = 17,
        InterruptRejected = 18,
        AppearanceRequestMissing = 19,
        AppearanceRejected = 20,
        NpcInteractionRequestMissing = 21,
        NpcInteractionUnavailable = 22,
        PortCommandMissing = 23,
        PortActorMismatch = 24,
        PortRouterMissing = 25,
        PortRejected = 26,
    }

    /// <summary>
    /// 角色命令统一输入。所有嵌套对象都属于已发布的纯逻辑 DTO；命令本身不携带
    /// Unity 对象、坐标、网络请求或外部系统私有句柄。
    /// </summary>
    [Serializable]
    public sealed class D20CharacterCommand
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public string ActorId = string.Empty;
        public D20CharacterCommandKind Kind = D20CharacterCommandKind.None;
        public D20CharacterIdentity Identity;
        public D20PlayerPresenceRequest PresenceRequest;
        public D20ActionIntent ActionIntent;
        public D20ActionPriorityContext PriorityContext;
        public D20CharacterConditionEvaluation Condition;
        public D20ActionInterruptRequest InterruptRequest;
        public D20AppearanceSelectionRequest AppearanceRequest;
        public D20NpcInteractionAvailabilityRequest NpcInteractionRequest;
        public D20NpcBehaviorSnapshot NpcProjection;
        public D20AnimalIdentity AnimalIdentityProjection;
        public D20AnimalBehaviorSnapshot AnimalProjection;
        public D19RoutedPortCommand PortCommand;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();

        public static D20CharacterCommand Create(string commandId, string actorId,
            D20CharacterCommandKind kind)
        {
            return new D20CharacterCommand
            {
                CommandId = commandId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                Kind = kind,
            };
        }

        public static D20CharacterCommand ForPresence(string commandId,
            D20PlayerPresenceRequest request, D20CharacterIdentity identity = null)
        {
            return new D20CharacterCommand
            {
                CommandId = commandId ?? string.Empty,
                ActorId = request == null ? string.Empty : request.PlayerId ?? string.Empty,
                Kind = D20CharacterCommandKind.UpdatePresence,
                Identity = identity == null ? null : identity.Clone(),
                PresenceRequest = request == null ? null : request.Clone(),
                WorldDay = request == null ? 0 : request.WorldDay,
                SaveRevision = request == null ? 0 : request.SaveRevision,
            };
        }

        public static D20CharacterCommand ForAction(string commandId,
            D20ActionIntent intent, D20ActionPriorityContext priorityContext = null,
            D20CharacterConditionEvaluation condition = null,
            D20CharacterIdentity identity = null,
            D19RoutedPortCommand portCommand = null)
        {
            return new D20CharacterCommand
            {
                CommandId = commandId ?? string.Empty,
                ActorId = intent == null ? string.Empty : intent.ActorId ?? string.Empty,
                Kind = D20CharacterCommandKind.StartAction,
                Identity = identity == null ? null : identity.Clone(),
                ActionIntent = intent == null ? null : intent.Clone(),
                PriorityContext = priorityContext == null ? null : priorityContext.Clone(),
                Condition = condition == null ? null : condition.Clone(),
                PortCommand = ClonePortCommand(portCommand),
                WorldDay = intent == null ? 0 : intent.WorldDay,
                SaveRevision = intent == null ? 0 : intent.SaveRevision,
            };
        }

        public static D20CharacterCommand ForPort(string commandId,
            D19RoutedPortCommand portCommand, D20CharacterIdentity identity = null)
        {
            return new D20CharacterCommand
            {
                CommandId = commandId ?? string.Empty,
                ActorId = portCommand == null ? string.Empty : portCommand.ActorId ?? string.Empty,
                Kind = D20CharacterCommandKind.RoutePortCommand,
                Identity = identity == null ? null : identity.Clone(),
                PortCommand = ClonePortCommand(portCommand),
                WorldDay = portCommand == null ? 0 : portCommand.WorldDay,
                SaveRevision = portCommand == null ? 0 : portCommand.SaveRevision,
            };
        }

        public D20CharacterCommand Clone()
        {
            var result = new D20CharacterCommand
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                Kind = Kind,
                Identity = Identity == null ? null : Identity.Clone(),
                PresenceRequest = PresenceRequest == null ? null : PresenceRequest.Clone(),
                ActionIntent = ActionIntent == null ? null : ActionIntent.Clone(),
                PriorityContext = PriorityContext == null ? null : PriorityContext.Clone(),
                Condition = Condition == null ? null : Condition.Clone(),
                InterruptRequest = InterruptRequest == null ? null : InterruptRequest.Clone(),
                AppearanceRequest = AppearanceRequest == null ? null : AppearanceRequest.Clone(),
                NpcInteractionRequest = NpcInteractionRequest == null
                    ? null : NpcInteractionRequest.Clone(),
                NpcProjection = NpcProjection == null ? null : NpcProjection.Clone(),
                AnimalIdentityProjection = AnimalIdentityProjection == null
                    ? null : AnimalIdentityProjection.Clone(),
                AnimalProjection = AnimalProjection == null ? null : AnimalProjection.Clone(),
                PortCommand = ClonePortCommand(PortCommand),
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (CauseEventIds != null)
                result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        /// <summary>不包含 command_id 的稳定指纹，供同 ID 冲突检查使用。</summary>
        public string Fingerprint()
        {
            var builder = new StringBuilder("d20-character-command-v1|");
            Append(builder, ActorId);
            Append(builder, ((int)Kind).ToString());
            Append(builder, WorldDay.ToString());
            Append(builder, SaveRevision.ToString());
            if (Identity != null)
                Append(builder, Identity.ImmutableFields == null
                    ? string.Empty : Identity.ImmutableFields.Fingerprint());
            if (PresenceRequest != null)
            {
                Append(builder, PresenceRequest.CommandId);
                Append(builder, PresenceRequest.PlayerId);
                Append(builder, ((int)PresenceRequest.DesiredState).ToString());
                Append(builder, ((int)PresenceRequest.Reason).ToString());
                Append(builder, PresenceRequest.PortId);
                Append(builder, PresenceRequest.ShipId);
                Append(builder, PresenceRequest.LocationRef);
            }
            if (ActionIntent != null)
            {
                Append(builder, ActionIntent.CommandId);
                Append(builder, ((int)ActionIntent.Kind).ToString());
                Append(builder, ActionIntent.ActionId);
                Append(builder, ActionIntent.ActorId);
                Append(builder, ActionIntent.TargetId);
                Append(builder, ActionIntent.LocationRef);
                Append(builder, ActionIntent.OwnerSystem);
            }
            if (PriorityContext != null)
            {
                Append(builder, ((int)PriorityContext.PresenceState).ToString());
                Append(builder, PriorityContext.CharacterAvailable ? "1" : "0");
                Append(builder, PriorityContext.OwnerPermission ? "1" : "0");
                Append(builder, PriorityContext.HasTarget ? "1" : "0");
                Append(builder, PriorityContext.ActiveIntent == null
                    ? string.Empty : PriorityContext.ActiveIntent.CommandId);
            }
            if (Condition != null)
            {
                Append(builder, ((int)Condition.ActionKind).ToString());
                Append(builder, Condition.Eligible ? "1" : "0");
                Append(builder, ((int)Condition.FailureCode).ToString());
                Append(builder, Condition.ReasonCode);
            }
            if (InterruptRequest != null)
            {
                Append(builder, InterruptRequest.CommandId);
                Append(builder, ((int)InterruptRequest.Reason).ToString());
                Append(builder, InterruptRequest.TargetCommandId);
                Append(builder, ((int)InterruptRequest.Availability).ToString());
            }
            if (AppearanceRequest != null)
            {
                Append(builder, AppearanceRequest.CharacterId);
                Append(builder, AppearanceRequest.ItemId);
                Append(builder, AppearanceRequest.ContentId);
                Append(builder, AppearanceRequest.ResourceKey);
                Append(builder, ((int)AppearanceRequest.Slot).ToString());
                Append(builder, ((int)AppearanceRequest.Mode).ToString());
            }
            if (NpcInteractionRequest != null)
            {
                Append(builder, NpcInteractionRequest.NpcId);
                Append(builder, NpcInteractionRequest.RoleId);
                Append(builder, ((int)NpcInteractionRequest.InteractionKind).ToString());
                Append(builder, ((int)NpcInteractionRequest.BehaviorState).ToString());
                Append(builder, ((int)NpcInteractionRequest.Availability).ToString());
                Append(builder, NpcInteractionRequest.IsBusy ? "1" : "0");
            }
            if (PortCommand != null)
            {
                Append(builder, PortCommand.CommandId);
                Append(builder, ((int)PortCommand.Kind).ToString());
                Append(builder, PortCommand.ActorId);
                Append(builder, PortCommand.ShipId);
                Append(builder, PortCommand.PortId);
                Append(builder, PortCommand.BerthId);
                Append(builder, PortCommand.ServiceId);
            }
            if (CauseEventIds != null)
                foreach (string causeEventId in CauseEventIds)
                    Append(builder, causeEventId);
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, string value)
        {
            string text = value ?? string.Empty;
            builder.Append(text.Length).Append(':').Append(text).Append('|');
        }

        internal static D19RoutedPortCommand ClonePortCommand(D19RoutedPortCommand command)
        {
            if (command == null) return null;
            var result = new D19RoutedPortCommand(
                command.CommandId, command.Kind, command.ActorId, command.ShipId,
                command.PortId, command.BerthId, command.ServiceId,
                command.WorldDay, command.SaveRevision,
                command.Claims == null ? null : new D19AuthorityClaims(
                    command.Claims.ShipId, command.Claims.ActorOwnsShip,
                    command.Claims.ActorIsCrewOfShip, command.Claims.ShipCurrentPortId));
            if (command.Parameters != null)
                foreach (var pair in command.Parameters)
                    result.Parameters[pair.Key] = pair.Value;
            if (command.ActorIds != null)
                result.ActorIds = new List<string>(command.ActorIds);
            return result;
        }
    }

    /// <summary>D20-L28 统一角色命令结果；嵌套结果均为防御性副本。</summary>
    [Serializable]
    public sealed class D20CharacterCommandResult
    {
        public int SchemaVersion = D20CharacterCommand.CurrentSchemaVersion;
        public bool Accepted;
        public bool Succeeded;
        public bool IsDuplicate;
        public string CommandId = string.Empty;
        public string RequestFingerprint = string.Empty;
        public string ActorId = string.Empty;
        public D20CharacterCommandKind Kind;
        public D20CharacterCommandStatus Status;
        public D20CharacterCommandFailureCode FailureCode;
        public string ReasonCode = string.Empty;
        public string LocalizationKey = string.Empty;
        public D20CharacterIdentity IdentityProjection;
        public D20PlayerPresenceResult PresenceResult;
        public D20ActionPriorityDecision PriorityDecision;
        public D20ActionInterruptResult ActionResult;
        public D20AppearanceSelectionResult AppearanceResult;
        public D20NpcInteractionAvailabilityResult NpcInteractionResult;
        public D20NpcBehaviorSnapshot NpcProjection;
        public D20AnimalIdentity AnimalIdentityProjection;
        public D20AnimalBehaviorSnapshot AnimalProjection;
        public D19RoutedCommandResult PortResult;
        public List<string> CauseEventIds = new List<string>();

        public D20CharacterCommandResult Clone()
        {
            var result = new D20CharacterCommandResult
            {
                SchemaVersion = SchemaVersion,
                Accepted = Accepted,
                Succeeded = Succeeded,
                IsDuplicate = IsDuplicate,
                CommandId = CommandId ?? string.Empty,
                RequestFingerprint = RequestFingerprint ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                Kind = Kind,
                Status = Status,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                LocalizationKey = LocalizationKey ?? string.Empty,
                IdentityProjection = IdentityProjection == null ? null : IdentityProjection.Clone(),
                PresenceResult = PresenceResult == null ? null : PresenceResult.Clone(),
                PriorityDecision = PriorityDecision == null ? null : PriorityDecision.Clone(),
                ActionResult = ActionResult == null ? null : ActionResult.Clone(),
                AppearanceResult = AppearanceResult == null ? null : AppearanceResult.Clone(),
                NpcInteractionResult = NpcInteractionResult == null
                    ? null : NpcInteractionResult.Clone(),
                NpcProjection = NpcProjection == null ? null : NpcProjection.Clone(),
                AnimalIdentityProjection = AnimalIdentityProjection == null
                    ? null : AnimalIdentityProjection.Clone(),
                AnimalProjection = AnimalProjection == null ? null : AnimalProjection.Clone(),
                PortResult = ClonePortResult(PortResult),
            };
            if (CauseEventIds != null)
                result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        internal static D19RoutedCommandResult ClonePortResult(D19RoutedCommandResult source)
        {
            if (source == null) return null;
            var result = new D19RoutedCommandResult
            {
                Success = source.Success,
                IsDuplicate = source.IsDuplicate,
                CommandId = source.CommandId ?? string.Empty,
                Kind = source.Kind,
                ReasonCode = source.ReasonCode ?? string.Empty,
                LocKey = source.LocKey ?? string.Empty,
                RetryPolicy = source.RetryPolicy,
                StateAfter = source.StateAfter,
                EventDraft = source.EventDraft,
                CompensationDraft = source.CompensationDraft,
            };
            if (source.ResultData != null)
                foreach (var pair in source.ResultData)
                    result.ResultData[pair.Key] = pair.Value;
            return result;
        }
    }

    /// <summary>
    /// D20-L28 角色命令唯一入口。该类只写 D20 自有投影与本地回执；所有外部
    /// owner 变化都通过其已发布的 public request/result boundary 完成。
    /// </summary>
    public sealed class D20CharacterCommandService
    {
        public const string OwnerSystem = "D20CharacterCommandService";

        private readonly D20PlayerPresenceSystem _presenceSystem;
        private readonly D20ActionInterruptSystem _actionSystem;
        private readonly D19PortCommandRouter _portRouter;
        private readonly Dictionary<string, D20CharacterCommandResult> _receipts =
            new Dictionary<string, D20CharacterCommandResult>(StringComparer.Ordinal);
        private D20CharacterIdentity _identity;

        public D20CharacterCommandService(
            D20PlayerPresenceSystem presenceSystem = null,
            D20ActionInterruptSystem actionSystem = null,
            D19PortCommandRouter portRouter = null,
            D20CharacterIdentity initialIdentity = null)
        {
            _presenceSystem = presenceSystem ?? new D20PlayerPresenceSystem();
            _actionSystem = actionSystem ?? new D20ActionInterruptSystem();
            _portRouter = portRouter;
            if (initialIdentity != null)
            {
                initialIdentity.Validate();
                _identity = initialIdentity.Clone();
            }
        }

        public D20PlayerPresenceSystem PresenceSystem { get { return _presenceSystem; } }
        public D20ActionInterruptSystem ActionSystem { get { return _actionSystem; } }
        public D19PortCommandRouter PortRouter { get { return _portRouter; } }
        public D20CharacterIdentity IdentityProjection
        {
            get { return _identity == null ? null : _identity.Clone(); }
        }

        /// <summary>唯一命令执行入口；同 ID 同指纹只回放，不重复产生副作用。</summary>
        public D20CharacterCommandResult Execute(D20CharacterCommand command)
        {
            D20CharacterCommandResult invalid;
            if (!ValidateCommand(command, out invalid))
                return invalid;

            string fingerprint = command.Fingerprint();
            D20CharacterCommandResult previous;
            if (_receipts.TryGetValue(command.CommandId, out previous))
            {
                if (!string.Equals(previous.RequestFingerprint, fingerprint, StringComparison.Ordinal))
                    return Failure(command, D20CharacterCommandFailureCode.DuplicateCommandConflict,
                        "character_command_id_conflict", false);
                var duplicate = previous.Clone();
                duplicate.IsDuplicate = true;
                duplicate.Status = D20CharacterCommandStatus.Duplicate;
                duplicate.ReasonCode = "character_command_replayed";
                return duplicate;
            }

            D20CharacterCommandResult result = Dispatch(command);
            result.RequestFingerprint = fingerprint;
            result.IdentityProjection = _identity == null ? null : _identity.Clone();
            if (D20CharacterIdentityContracts.IsStableCharacterId(command.CommandId))
            {
                var receipt = result.Clone();
                _receipts[command.CommandId] = receipt;
            }
            return result.Clone();
        }

        /// <summary>Execute 的语义别名，供上层聚合器和测试使用。</summary>
        public D20CharacterCommandResult Handle(D20CharacterCommand command)
        {
            return Execute(command);
        }

        private bool ValidateCommand(D20CharacterCommand command,
            out D20CharacterCommandResult failure)
        {
            if (command == null)
            {
                failure = Failure(null, D20CharacterCommandFailureCode.MissingRequest,
                    "character_command_missing", false);
                return false;
            }
            if (command.SchemaVersion != D20CharacterCommand.CurrentSchemaVersion)
            {
                failure = Failure(command, D20CharacterCommandFailureCode.InvalidSchemaVersion,
                    "character_command_schema_version_mismatch", false);
                return false;
            }
            if (!D20CharacterIdentityContracts.IsStableCharacterId(command.CommandId))
            {
                failure = Failure(command, D20CharacterCommandFailureCode.MissingCommandId,
                    "character_command_id_missing", false);
                return false;
            }
            if (!D20CharacterIdentityContracts.IsStableCharacterId(command.ActorId))
            {
                failure = Failure(command, D20CharacterCommandFailureCode.MissingActorId,
                    "character_actor_id_invalid", false);
                return false;
            }
            if (!Enum.IsDefined(typeof(D20CharacterCommandKind), command.Kind) ||
                command.Kind == D20CharacterCommandKind.None)
            {
                failure = Failure(command, D20CharacterCommandFailureCode.InvalidCommandKind,
                    "character_command_kind_invalid", false);
                return false;
            }
            if (command.Identity != null)
            {
                try
                {
                    command.Identity.Validate();
                }
                catch (InvalidOperationException)
                {
                    failure = Failure(command, D20CharacterCommandFailureCode.IdentityInvalid,
                        "character_identity_invalid", false);
                    return false;
                }
                if (command.Identity.CharacterKind != D20CharacterKind.Player ||
                    !string.Equals(command.Identity.CharacterId, command.ActorId,
                        StringComparison.Ordinal))
                {
                    failure = Failure(command, D20CharacterCommandFailureCode.IdentityMismatch,
                        "character_identity_actor_mismatch", false);
                    return false;
                }
                if (_identity != null &&
                    !D20CharacterIdentityContracts.ImmutableFieldsMatch(_identity, command.Identity))
                {
                    failure = Failure(command, D20CharacterCommandFailureCode.IdentityMismatch,
                        "character_identity_immutable_fields_changed", false);
                    return false;
                }
            }
            if (command.Kind == D20CharacterCommandKind.UpdatePresence &&
                command.PresenceRequest != null &&
                !string.Equals(command.PresenceRequest.PlayerId, command.ActorId,
                    StringComparison.Ordinal))
            {
                failure = Failure(command, D20CharacterCommandFailureCode.IdentityMismatch,
                    "character_presence_actor_mismatch", false);
                return false;
            }
            failure = null;
            return true;
        }

        private D20CharacterCommandResult Dispatch(D20CharacterCommand command)
        {
            if (command.Identity != null)
                _identity = command.Identity.Clone();

            switch (command.Kind)
            {
                case D20CharacterCommandKind.UpdatePresence:
                    return DispatchPresence(command);
                case D20CharacterCommandKind.StartAction:
                    return DispatchStartAction(command);
                case D20CharacterCommandKind.InterruptAction:
                    return DispatchInterrupt(command);
                case D20CharacterCommandKind.CompleteAction:
                    return DispatchComplete(command);
                case D20CharacterCommandKind.SelectAppearance:
                    return DispatchAppearance(command);
                case D20CharacterCommandKind.EvaluateNpcInteraction:
                    return DispatchNpcInteraction(command);
                case D20CharacterCommandKind.RoutePortCommand:
                    return DispatchPort(command);
                default:
                    return Failure(command, D20CharacterCommandFailureCode.InvalidCommandKind,
                        "character_command_kind_invalid", false);
            }
        }

        private D20CharacterCommandResult DispatchPresence(D20CharacterCommand command)
        {
            if (command.PresenceRequest == null)
                return Failure(command, D20CharacterCommandFailureCode.PresenceRequestMissing,
                    "character_presence_request_missing", false);
            var result = Base(command);
            var presence = _presenceSystem.Apply(command.PresenceRequest);
            result.PresenceResult = presence == null ? null : presence.Clone();
            if (presence == null || !presence.Accepted)
                return FailResult(result, D20CharacterCommandFailureCode.PresenceRejected,
                    presence == null ? "character_presence_result_missing" : presence.ReasonCode);

            result.Accepted = true;
            result.Succeeded = true;
            result.Status = presence.Duplicate
                ? D20CharacterCommandStatus.Duplicate : D20CharacterCommandStatus.Succeeded;
            result.IsDuplicate = presence.Duplicate;
            result.ReasonCode = presence.Duplicate
                ? "character_presence_replayed" : "character_presence_applied";
            if (presence.Event != null)
            {
                var actionRequest = D20ActionInterruptRequest.Presence(presence);
                result.ActionResult = _actionSystem.Apply(actionRequest);
            }
            return result;
        }

        private D20CharacterCommandResult DispatchStartAction(D20CharacterCommand command)
        {
            var result = Base(command);
            if (command.ActionIntent == null)
                return FailResult(result, D20CharacterCommandFailureCode.ActionIntentMissing,
                    "character_action_intent_missing");
            var validation = command.ActionIntent.Validate();
            if (!validation.Valid)
                return FailResult(result, D20CharacterCommandFailureCode.ActionIntentInvalid,
                    validation.ReasonCode);
            if (!string.Equals(command.ActionIntent.ActorId, command.ActorId,
                StringComparison.Ordinal))
                return FailResult(result, D20CharacterCommandFailureCode.ActionActorMismatch,
                    "character_action_actor_mismatch");
            if (command.Condition != null &&
                (!command.Condition.Eligible || command.Condition.ActionKind != command.ActionIntent.Kind))
                return FailResult(result, D20CharacterCommandFailureCode.ConditionRejected,
                    string.IsNullOrEmpty(command.Condition.ReasonCode)
                        ? "character_condition_rejected" : command.Condition.ReasonCode);

            var context = command.PriorityContext == null
                ? new D20ActionPriorityContext() : command.PriorityContext.Clone();
            if (context.PresenceState == D20PlayerPresenceState.Unknown)
                context.PresenceState = _presenceSystem.Snapshot.State;
            if (command.Identity != null &&
                command.Identity.Availability != D20CharacterAvailability.Unknown &&
                command.Identity.Availability != D20CharacterAvailability.Available)
                context.CharacterAvailable = false;
            context.HasTarget = context.HasTarget || !string.IsNullOrEmpty(command.ActionIntent.TargetId);
            var decision = D20ActionPriorityRules.Evaluate(command.ActionIntent, context);
            result.PriorityDecision = decision == null ? null : decision.Clone();
            if (decision == null || !decision.Allowed)
                return FailResult(result, D20CharacterCommandFailureCode.PriorityRejected,
                    decision == null ? "character_priority_result_missing" : decision.ReasonCode);

            var action = _actionSystem.RegisterActiveAction(command.ActionIntent, context);
            result.ActionResult = action == null ? null : action.Clone();
            if (action == null || !action.Accepted)
                return FailResult(result, D20CharacterCommandFailureCode.ActionRejected,
                    action == null ? "character_action_result_missing" : action.ReasonCode);
            result.Accepted = true;
            result.Succeeded = true;
            result.Status = D20CharacterCommandStatus.Succeeded;
            result.ReasonCode = "character_action_started";
            return result;
        }

        private D20CharacterCommandResult DispatchInterrupt(D20CharacterCommand command)
        {
            var result = Base(command);
            if (command.InterruptRequest == null)
                return FailResult(result, D20CharacterCommandFailureCode.InterruptRequestMissing,
                    "character_interrupt_request_missing");
            if (!string.IsNullOrEmpty(command.InterruptRequest.ActorId) &&
                !string.Equals(command.InterruptRequest.ActorId, command.ActorId,
                    StringComparison.Ordinal))
                return FailResult(result, D20CharacterCommandFailureCode.ActionActorMismatch,
                    "character_interrupt_actor_mismatch");
            var action = _actionSystem.Apply(command.InterruptRequest);
            result.ActionResult = action == null ? null : action.Clone();
            if (action == null || !action.Accepted)
                return FailResult(result, D20CharacterCommandFailureCode.InterruptRejected,
                    action == null ? "character_interrupt_result_missing" : action.ReasonCode);
            result.Accepted = true;
            result.Succeeded = true;
            result.Status = action.Duplicate
                ? D20CharacterCommandStatus.Duplicate : D20CharacterCommandStatus.Succeeded;
            result.IsDuplicate = action.Duplicate;
            result.ReasonCode = action.Duplicate
                ? "character_interrupt_replayed" : "character_interrupt_applied";
            return result;
        }

        private D20CharacterCommandResult DispatchComplete(D20CharacterCommand command)
        {
            var result = Base(command);
            var action = _actionSystem.Complete(command.CommandId, command.ActorId,
                command.WorldDay, command.SaveRevision);
            result.ActionResult = action == null ? null : action.Clone();
            if (action == null || !action.Accepted)
                return FailResult(result, D20CharacterCommandFailureCode.ActionRejected,
                    action == null ? "character_complete_result_missing" : action.ReasonCode);
            result.Accepted = true;
            result.Succeeded = true;
            result.Status = action.Duplicate
                ? D20CharacterCommandStatus.Duplicate : D20CharacterCommandStatus.Succeeded;
            result.IsDuplicate = action.Duplicate;
            result.ReasonCode = action.Duplicate
                ? "character_complete_replayed" : "character_action_completed";
            return result;
        }

        private D20CharacterCommandResult DispatchAppearance(D20CharacterCommand command)
        {
            var result = Base(command);
            if (command.AppearanceRequest == null)
                return FailResult(result, D20CharacterCommandFailureCode.AppearanceRequestMissing,
                    "character_appearance_request_missing");
            if (!string.Equals(command.AppearanceRequest.CharacterId, command.ActorId,
                StringComparison.Ordinal))
                return FailResult(result, D20CharacterCommandFailureCode.IdentityMismatch,
                    "character_appearance_actor_mismatch");
            var appearance = D20AppearanceSelectionRules.Evaluate(command.AppearanceRequest);
            result.AppearanceResult = appearance == null ? null : appearance.Clone();
            if (appearance == null || !appearance.Valid)
                return FailResult(result, D20CharacterCommandFailureCode.AppearanceRejected,
                    appearance == null ? "character_appearance_result_missing" : appearance.ReasonCode);
            result.Accepted = true;
            result.Succeeded = true;
            result.Status = D20CharacterCommandStatus.Accepted;
            result.ReasonCode = "character_appearance_selection_valid";
            return result;
        }

        private D20CharacterCommandResult DispatchNpcInteraction(D20CharacterCommand command)
        {
            var result = Base(command);
            if (command.NpcInteractionRequest == null)
                return FailResult(result, D20CharacterCommandFailureCode.NpcInteractionRequestMissing,
                    "character_npc_interaction_request_missing");
            var interaction = D20NpcInteractionAvailability.Evaluate(
                command.NpcInteractionRequest);
            result.NpcInteractionResult = interaction == null ? null : interaction.Clone();
            if (interaction == null || !interaction.Available)
                return FailResult(result, D20CharacterCommandFailureCode.NpcInteractionUnavailable,
                    interaction == null ? "character_npc_interaction_result_missing" : interaction.ReasonCode);
            result.Accepted = true;
            result.Succeeded = true;
            result.Status = D20CharacterCommandStatus.Accepted;
            result.ReasonCode = "character_npc_interaction_available";
            return result;
        }

        private D20CharacterCommandResult DispatchPort(D20CharacterCommand command)
        {
            var result = Base(command);
            if (command.PortCommand == null)
                return FailResult(result, D20CharacterCommandFailureCode.PortCommandMissing,
                    "character_port_command_missing");
            if (!string.Equals(command.PortCommand.ActorId, command.ActorId,
                StringComparison.Ordinal))
                return FailResult(result, D20CharacterCommandFailureCode.PortActorMismatch,
                    "character_port_actor_mismatch");
            if (_portRouter == null)
                return FailResult(result, D20CharacterCommandFailureCode.PortRouterMissing,
                    "character_port_router_missing");

            // D19 的唯一命令入口：不得在 D20 中复制或绕过港口 owner 裁决。
            var portResult = _portRouter.Route(D20CharacterCommand.ClonePortCommand(
                command.PortCommand));
            result.PortResult = D20CharacterCommandResult.ClonePortResult(portResult);
            if (portResult == null || !portResult.Success)
                return FailResult(result, D20CharacterCommandFailureCode.PortRejected,
                    portResult == null ? "character_port_result_missing" : portResult.ReasonCode);
            result.Accepted = true;
            result.Succeeded = true;
            result.IsDuplicate = portResult.IsDuplicate;
            result.Status = portResult.IsDuplicate
                ? D20CharacterCommandStatus.Duplicate : D20CharacterCommandStatus.Succeeded;
            result.ReasonCode = portResult.IsDuplicate
                ? "character_port_replayed" : "character_port_command_applied";
            return result;
        }

        private static D20CharacterCommandResult Base(D20CharacterCommand command)
        {
            var result = new D20CharacterCommandResult
            {
                CommandId = command == null ? string.Empty : command.CommandId ?? string.Empty,
                ActorId = command == null ? string.Empty : command.ActorId ?? string.Empty,
                Kind = command == null ? D20CharacterCommandKind.None : command.Kind,
                Status = D20CharacterCommandStatus.Rejected,
                FailureCode = D20CharacterCommandFailureCode.None,
            };
            if (command != null && command.CauseEventIds != null)
                result.CauseEventIds.AddRange(command.CauseEventIds);
            return result;
        }

        private static D20CharacterCommandResult Failure(D20CharacterCommand command,
            D20CharacterCommandFailureCode code, string reasonCode, bool duplicate)
        {
            var result = Base(command);
            result.FailureCode = code;
            result.ReasonCode = reasonCode ?? string.Empty;
            result.IsDuplicate = duplicate;
            result.Status = duplicate ? D20CharacterCommandStatus.Duplicate
                : D20CharacterCommandStatus.Rejected;
            return result;
        }

        private static D20CharacterCommandResult FailResult(
            D20CharacterCommandResult result,
            D20CharacterCommandFailureCode code,
            string reasonCode)
        {
            result.Accepted = false;
            result.Succeeded = false;
            result.FailureCode = code;
            result.ReasonCode = reasonCode ?? string.Empty;
            result.Status = D20CharacterCommandStatus.Rejected;
            return result;
        }
    }
}
