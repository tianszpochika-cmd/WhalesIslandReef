// ============================================================
// D20ActionEventRouter —— D20-L31 命令/状态/事件顺序路由（纯 C#）
//
// 统一顺序：CommandReceived -> Validation -> OwnerResult（如有） ->
// DomainFact（仅 owner 成功且非重复） -> D20StateProjectionUpdated ->
// PresentationReadOnly；失败只追加 Failure，不伪造领域事实。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D20
{
    /// <summary>D20-L31 事件处理阶段。</summary>
    public enum D20ActionEventPhase
    {
        CommandReceived = 0,
        Validation = 1,
        OwnerResult = 2,
        DomainFact = 3,
        D20StateProjectionUpdated = 4,
        PresentationReadOnly = 5,
        Failure = 6,
    }

    /// <summary>路由器生成的稳定纯逻辑事件。</summary>
    [Serializable]
    public sealed class D20ActionEvent
    {
        public int Sequence;
        public string EventId = string.Empty;
        public string CommandId = string.Empty;
        public string ActorId = string.Empty;
        public D20ActionEventPhase Phase;
        public string OwnerSystem = string.Empty;
        public bool Success;
        public bool IsDuplicate;
        public bool IsDomainFact;
        public bool IsReadOnly = true;
        public bool StateChanged;
        public string ReasonCode = string.Empty;
        public D20PresentationEvent PresentationEvent;
        public List<string> CauseEventIds = new List<string>();

        public D20ActionEvent Clone()
        {
            var result = new D20ActionEvent
            {
                Sequence = Sequence,
                EventId = EventId ?? string.Empty,
                CommandId = CommandId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                Phase = Phase,
                OwnerSystem = OwnerSystem ?? string.Empty,
                Success = Success,
                IsDuplicate = IsDuplicate,
                IsDomainFact = IsDomainFact,
                IsReadOnly = IsReadOnly,
                StateChanged = StateChanged,
                ReasonCode = ReasonCode ?? string.Empty,
                PresentationEvent = PresentationEvent,
            };
            if (CauseEventIds != null)
                result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }
    }

    /// <summary>D20-L31 一次命令路由结果。</summary>
    [Serializable]
    public sealed class D20ActionEventRouteResult
    {
        public bool Accepted;
        public bool Succeeded;
        public bool IsDuplicate;
        public string CommandId = string.Empty;
        public string ActorId = string.Empty;
        public string RequestFingerprint = string.Empty;
        public string FailureCode = string.Empty;
        public string ReasonCode = string.Empty;
        public D20CharacterCommandResult CharacterResult;
        public D20NpcBehaviorServiceResult NpcResult;
        public D20AnimalBehaviorServiceResult AnimalResult;
        public List<D20ActionEvent> Events = new List<D20ActionEvent>();

        public D20ActionEventRouteResult Clone()
        {
            var result = new D20ActionEventRouteResult
            {
                Accepted = Accepted,
                Succeeded = Succeeded,
                IsDuplicate = IsDuplicate,
                CommandId = CommandId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                RequestFingerprint = RequestFingerprint ?? string.Empty,
                FailureCode = FailureCode ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                CharacterResult = CharacterResult == null ? null : CharacterResult.Clone(),
                NpcResult = NpcResult == null ? null : NpcResult.Clone(),
                AnimalResult = AnimalResult == null ? null : AnimalResult.Clone(),
                Events = new List<D20ActionEvent>(),
            };
            if (Events != null)
                foreach (D20ActionEvent item in Events)
                    if (item != null) result.Events.Add(item.Clone());
            return result;
        }

        /// <summary>验证事件顺序和领域事实不变量。</summary>
        public bool Validate()
        {
            if (Events == null || Events.Count < 2) return false;
            int previousSequence = -1;
            D20ActionEventPhase previousPhase = D20ActionEventPhase.CommandReceived;
            foreach (D20ActionEvent item in Events)
            {
                if (item == null || item.Sequence <= previousSequence ||
                    !string.Equals(item.CommandId, CommandId, StringComparison.Ordinal) ||
                    !string.Equals(item.EventId,
                        BuildEventId(CommandId, item.Sequence), StringComparison.Ordinal))
                    return false;
                if (item.Phase < previousPhase && item.Phase != D20ActionEventPhase.Failure)
                    return false;
                if (item.IsDomainFact && (!Succeeded ||
                    (IsDuplicate && !item.IsDuplicate) || !item.Success))
                    return false;
                if (item.IsDomainFact && item.IsReadOnly)
                    return false;
                previousSequence = item.Sequence;
                previousPhase = item.Phase;
            }
            return Events[0].Phase == D20ActionEventPhase.CommandReceived &&
                Events[1].Phase == D20ActionEventPhase.Validation;
        }

        internal static string BuildEventId(string commandId, int sequence)
        {
            return "d20-action-event-" + (commandId ?? string.Empty) + "-" + sequence;
        }
    }

    /// <summary>
    /// D20-L31 顺序路由器。它只调用 L28-L30 的 public API，不夺取任何 owner
    /// 的裁决权，也不在失败或重复路径伪造 DomainFactEvent。
    /// </summary>
    public sealed class D20ActionEventRouter
    {
        public const string OwnerSystem = "D20ActionEventRouter";

        private readonly D20CharacterCommandService _characterService;
        private readonly D20NpcBehaviorService _npcService;
        private readonly D20AnimalBehaviorService _animalService;
        private readonly Dictionary<string, D20ActionEventRouteResult> _receipts =
            new Dictionary<string, D20ActionEventRouteResult>(StringComparer.Ordinal);

        public D20ActionEventRouter(
            D20CharacterCommandService characterService = null,
            D20NpcBehaviorService npcService = null,
            D20AnimalBehaviorService animalService = null)
        {
            _characterService = characterService ?? new D20CharacterCommandService();
            _npcService = npcService ?? new D20NpcBehaviorService();
            _animalService = animalService ?? new D20AnimalBehaviorService();
        }

        public D20CharacterCommandService CharacterService { get { return _characterService; } }
        public D20NpcBehaviorService NpcService { get { return _npcService; } }
        public D20AnimalBehaviorService AnimalService { get { return _animalService; } }

        /// <summary>路由 D20-L28 角色命令。</summary>
        public D20ActionEventRouteResult Route(D20CharacterCommand command)
        {
            string commandId = command == null ? string.Empty : command.CommandId ?? string.Empty;
            string actorId = command == null ? string.Empty : command.ActorId ?? string.Empty;
            string fingerprint = command == null ? string.Empty : command.Fingerprint();
            D20ActionEventRouteResult duplicate;
            if (TryReplay(commandId, fingerprint, out duplicate)) return duplicate;

            var result = Base(commandId, actorId, fingerprint);
            AddEvent(result, D20ActionEventPhase.CommandReceived, true, false,
                "character_command_received", false, false, null, null);
            if (!IsValidCharacterRoute(command))
            {
                AddEvent(result, D20ActionEventPhase.Validation, false, false,
                    "character_command_validation_failed", false, false, null, null);
                result.ReasonCode = "character_command_validation_failed";
                AddEvent(result, D20ActionEventPhase.Failure, false, false,
                    result.ReasonCode, false, false, null, null);
                return Cache(commandId, result);
            }

            AddEvent(result, D20ActionEventPhase.Validation, true, false,
                "character_command_validation_passed", false, false, null, null);
            result.CharacterResult = _characterService.Execute(command);
            result.Accepted = result.CharacterResult != null && result.CharacterResult.Accepted;
            result.Succeeded = result.CharacterResult != null && result.CharacterResult.Succeeded;
            result.IsDuplicate = result.CharacterResult != null && result.CharacterResult.IsDuplicate;
            result.ReasonCode = result.CharacterResult == null
                ? "character_command_result_missing" : result.CharacterResult.ReasonCode;
            if (result.CharacterResult != null && result.CharacterResult.PortResult != null)
            {
                var port = result.CharacterResult.PortResult;
                AddEvent(result, D20ActionEventPhase.OwnerResult, port.Success,
                    port.IsDuplicate, port.ReasonCode, false, false, null,
                    result.CharacterResult.CauseEventIds);
                if (port.Success && !port.IsDuplicate && port.EventDraft != null)
                {
                    AddEvent(result, D20ActionEventPhase.DomainFact, true, false,
                        "d19_domain_fact_accepted", true, false, null,
                        result.CharacterResult.CauseEventIds);
                }
            }
            AddCharacterProjectionAndPresentation(result);
            if (!result.Accepted)
                AddEvent(result, D20ActionEventPhase.Failure, false,
                    result.IsDuplicate, result.ReasonCode, false, false, null,
                    result.CharacterResult == null ? null : result.CharacterResult.CauseEventIds);
            return Cache(commandId, result);
        }

        /// <summary>路由 D20-L29 NPC 行为命令。</summary>
        public D20ActionEventRouteResult Route(D20NpcBehaviorCommand command)
        {
            string commandId = command == null ? string.Empty : command.CommandId ?? string.Empty;
            string npcId = command == null ? string.Empty : command.NpcId ?? string.Empty;
            string fingerprint = command == null ? string.Empty : command.Fingerprint();
            D20ActionEventRouteResult duplicate;
            if (TryReplay(commandId, fingerprint, out duplicate)) return duplicate;
            var result = Base(commandId, npcId, fingerprint);
            AddEvent(result, D20ActionEventPhase.CommandReceived, true, false,
                "npc_command_received", false, false, null, null);
            if (command == null || !D20NpcIdentityContracts.IsStableReference(command.CommandId, false) ||
                !D20NpcIdentityContracts.IsStableNpcId(command.NpcId))
            {
                AddEvent(result, D20ActionEventPhase.Validation, false, false,
                    "npc_command_validation_failed", false, false, null, null);
                result.ReasonCode = "npc_command_validation_failed";
                AddEvent(result, D20ActionEventPhase.Failure, false, false,
                    result.ReasonCode, false, false, null, null);
                return Cache(commandId, result);
            }
            AddEvent(result, D20ActionEventPhase.Validation, true, false,
                "npc_command_validation_passed", false, false, null, null);
            result.NpcResult = _npcService.Execute(command);
            result.Accepted = result.NpcResult != null && result.NpcResult.Accepted;
            result.Succeeded = result.NpcResult != null && result.NpcResult.Succeeded;
            result.IsDuplicate = result.NpcResult != null && result.NpcResult.IsDuplicate;
            result.ReasonCode = result.NpcResult == null
                ? "npc_command_result_missing" : result.NpcResult.ReasonCode;
            AddEvent(result, D20ActionEventPhase.D20StateProjectionUpdated,
                result.Accepted, result.IsDuplicate, result.ReasonCode,
                false, result.Accepted && !result.IsDuplicate, null, null);
            if (!result.Accepted)
                AddEvent(result, D20ActionEventPhase.Failure, false,
                    result.IsDuplicate, result.ReasonCode, false, false, null, null);
            return Cache(commandId, result);
        }

        /// <summary>路由 D20-L30 动物行为命令。</summary>
        public D20ActionEventRouteResult Route(D20AnimalBehaviorCommand command)
        {
            string commandId = command == null ? string.Empty : command.CommandId ?? string.Empty;
            string animalId = command == null ? string.Empty : command.AnimalId ?? string.Empty;
            string fingerprint = command == null ? string.Empty : command.Fingerprint();
            D20ActionEventRouteResult duplicate;
            if (TryReplay(commandId, fingerprint, out duplicate)) return duplicate;
            var result = Base(commandId, animalId, fingerprint);
            AddEvent(result, D20ActionEventPhase.CommandReceived, true, false,
                "animal_command_received", false, false, null, null);
            if (command == null || !D20AnimalIdentityContracts.IsStableSpeciesId(command.CommandId) ||
                !D20AnimalIdentityContracts.IsStableAnimalId(command.AnimalId))
            {
                AddEvent(result, D20ActionEventPhase.Validation, false, false,
                    "animal_command_validation_failed", false, false, null, null);
                result.ReasonCode = "animal_command_validation_failed";
                AddEvent(result, D20ActionEventPhase.Failure, false, false,
                    result.ReasonCode, false, false, null, null);
                return Cache(commandId, result);
            }
            AddEvent(result, D20ActionEventPhase.Validation, true, false,
                "animal_command_validation_passed", false, false, null, null);
            result.AnimalResult = _animalService.Execute(command);
            result.Accepted = result.AnimalResult != null && result.AnimalResult.Accepted;
            result.Succeeded = result.AnimalResult != null && result.AnimalResult.Succeeded;
            result.IsDuplicate = result.AnimalResult != null && result.AnimalResult.IsDuplicate;
            result.ReasonCode = result.AnimalResult == null
                ? "animal_command_result_missing" : result.AnimalResult.ReasonCode;
            AddEvent(result, D20ActionEventPhase.D20StateProjectionUpdated,
                result.Accepted, result.IsDuplicate, result.ReasonCode,
                false, result.Accepted && !result.IsDuplicate, null, null);
            if (!result.Accepted)
                AddEvent(result, D20ActionEventPhase.Failure, false,
                    result.IsDuplicate, result.ReasonCode, false, false, null, null);
            return Cache(commandId, result);
        }

        /// <summary>行为请求的便捷路由入口。</summary>
        public D20ActionEventRouteResult Route(D20NpcBehaviorRequest request)
        {
            return Route(D20NpcBehaviorCommand.Create(
                request == null ? string.Empty : request.CommandId, request));
        }

        /// <summary>动物输入的便捷路由入口。</summary>
        public D20ActionEventRouteResult Route(D20AnimalBehaviorInput input)
        {
            return Route(D20AnimalBehaviorCommand.Create(input));
        }

        private void AddCharacterProjectionAndPresentation(D20ActionEventRouteResult result)
        {
            if (result.CharacterResult == null) return;
            var character = result.CharacterResult;
            if (character.ActionResult != null && character.Accepted)
            {
                AddEvent(result, D20ActionEventPhase.D20StateProjectionUpdated,
                    true, character.ActionResult.Duplicate,
                    character.ActionResult.ReasonCode,
                    false, !character.ActionResult.Duplicate,
                    null, character.CauseEventIds);
                D20PresentationEvent presentation;
                string presentationReason;
                if (!character.ActionResult.Duplicate &&
                    D20PresentationEventContracts.TryCreateFromActionResult(
                        character.ActionResult, out presentation, out presentationReason))
                {
                    AddEvent(result, D20ActionEventPhase.PresentationReadOnly, true, false,
                        presentationReason, false, false, presentation, character.CauseEventIds);
                }
            }
            else if ((character.AppearanceResult != null ||
                      character.NpcInteractionResult != null) && character.Accepted)
            {
                AddEvent(result, D20ActionEventPhase.D20StateProjectionUpdated,
                    true, false, character.ReasonCode, false, false, null,
                    character.CauseEventIds);
            }
        }

        private bool TryReplay(string commandId, string fingerprint,
            out D20ActionEventRouteResult result)
        {
            D20ActionEventRouteResult previous;
            if (!_receipts.TryGetValue(commandId ?? string.Empty, out previous))
            {
                result = null;
                return false;
            }
            if (!string.Equals(previous.RequestFingerprint, fingerprint,
                StringComparison.Ordinal))
            {
                result = Base(commandId, string.Empty, fingerprint);
                AddEvent(result, D20ActionEventPhase.CommandReceived, true, false,
                    "command_received", false, false, null, null);
                AddEvent(result, D20ActionEventPhase.Validation, false, false,
                    "command_id_conflict", false, false, null, null);
                result.ReasonCode = "command_id_conflict";
                AddEvent(result, D20ActionEventPhase.Failure, false, true,
                    result.ReasonCode, false, false, null, null);
                return true;
            }
            result = previous.Clone();
            result.IsDuplicate = true;
            foreach (D20ActionEvent item in result.Events)
                if (item != null) item.IsDuplicate = true;
            return true;
        }

        private D20ActionEventRouteResult Cache(string commandId,
            D20ActionEventRouteResult result)
        {
            if (D20CharacterIdentityContracts.IsStableCharacterId(commandId))
                _receipts[commandId] = result.Clone();
            return result.Clone();
        }

        private static D20ActionEventRouteResult Base(string commandId,
            string actorId, string fingerprint)
        {
            return new D20ActionEventRouteResult
            {
                CommandId = commandId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                RequestFingerprint = fingerprint ?? string.Empty,
            };
        }

        private static bool IsValidCharacterRoute(D20CharacterCommand command)
        {
            return command != null &&
                D20CharacterIdentityContracts.IsStableCharacterId(command.CommandId) &&
                D20CharacterIdentityContracts.IsStableCharacterId(command.ActorId);
        }

        private static void AddEvent(D20ActionEventRouteResult result,
            D20ActionEventPhase phase, bool success, bool duplicate,
            string reasonCode, bool isDomainFact, bool stateChanged,
            D20PresentationEvent presentation, IEnumerable<string> causes)
        {
            int sequence = result.Events == null ? 0 : result.Events.Count;
            var item = new D20ActionEvent
            {
                Sequence = sequence,
                EventId = D20ActionEventRouteResult.BuildEventId(result.CommandId, sequence),
                CommandId = result.CommandId ?? string.Empty,
                ActorId = result.ActorId ?? string.Empty,
                Phase = phase,
                OwnerSystem = isDomainFact ? "D19PortCommandRouter" : OwnerSystem,
                Success = success,
                IsDuplicate = duplicate,
                IsDomainFact = isDomainFact,
                IsReadOnly = !isDomainFact,
                StateChanged = stateChanged,
                ReasonCode = reasonCode ?? string.Empty,
                PresentationEvent = presentation,
            };
            if (causes != null) item.CauseEventIds.AddRange(causes);
            result.Events.Add(item);
        }
    }
}
