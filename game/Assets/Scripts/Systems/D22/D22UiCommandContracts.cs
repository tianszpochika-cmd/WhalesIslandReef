// ============================================================
// D22UiCommandContracts —— D22-L05 UI 请求、关联 ID 与结果模型（纯 C#）
//
// UI 只提交确定性命令信封。命令结果可以表示 owner 尚未处理、成功、拒绝
// 或重复，但本文件不执行任何业务写入，也不把视觉状态当作事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Whalebone.Systems.D22
{
    /// <summary>D22 UI 命令种类；业务领域结果仍归各自 owner。</summary>
    public enum D22UiCommandKind
    {
        /// <summary>非法占位。</summary>
        None = 0,
        /// <summary>请求打开逻辑面板。</summary>
        OpenPanel = 1,
        /// <summary>请求关闭逻辑面板。</summary>
        ClosePanel = 2,
        /// <summary>请求新建游戏。</summary>
        NewGame = 3,
        /// <summary>请求继续游戏。</summary>
        ContinueGame = 4,
        /// <summary>请求保存。</summary>
        SaveGame = 5,
        /// <summary>请求读取。</summary>
        LoadGame = 6,
        /// <summary>暂停意图；实际时间状态由相应 owner 处理。</summary>
        Pause = 7,
        /// <summary>恢复意图；实际时间状态由相应 owner 处理。</summary>
        Resume = 8,
        /// <summary>转发港口公开命令。</summary>
        PortAction = 9,
        /// <summary>转发市场公开命令。</summary>
        MarketAction = 10,
        /// <summary>转发库存公开命令。</summary>
        InventoryAction = 11,
        /// <summary>转发船舶公开命令。</summary>
        ShipAction = 12,
        /// <summary>转发角色/船员公开命令。</summary>
        CrewAction = 13,
        /// <summary>转发任务公开命令。</summary>
        QuestAction = 14,
        /// <summary>转发对话后续公开意图。</summary>
        DialogueAction = 15,
        /// <summary>请求修改 D16 所有的表现偏好。</summary>
        AccessibilityUpdate = 16,
        /// <summary>确认或关闭一条反馈。</summary>
        AcknowledgeFeedback = 17,
        /// <summary>对同一 owner 命令提出安全重试。</summary>
        RetryOwnerCommand = 18,
        /// <summary>取消尚未完成的 owner 请求。</summary>
        CancelOwnerCommand = 19,
    }

    /// <summary>UI 命令的生命周期结果。</summary>
    public enum D22UiCommandOutcome
    {
        /// <summary>等待所属 owner 裁决。</summary>
        Pending = 0,
        /// <summary>请求已被 D22 接受并交给 owner。</summary>
        Accepted = 1,
        /// <summary>owner 已裁决成功。</summary>
        Succeeded = 2,
        /// <summary>请求或 owner 拒绝。</summary>
        Rejected = 3,
        /// <summary>重复命令，调用方应复用已知回执。</summary>
        Duplicate = 4,
        /// <summary>owner 明确不可用或依赖缺失。</summary>
        Blocked = 5,
        /// <summary>请求被取消。</summary>
        Cancelled = 6,
    }

    /// <summary>命令形状、上下文或幂等校验的稳定码。</summary>
    public enum D22UiCommandValidationCode
    {
        /// <summary>没有错误。</summary>
        None = 0,
        /// <summary>请求为空。</summary>
        MissingRequest = 1,
        /// <summary>上下文为空。</summary>
        MissingContext = 2,
        /// <summary>契约版本不支持。</summary>
        InvalidSchemaVersion = 3,
        /// <summary>缺少命令 ID。</summary>
        MissingCommandId = 4,
        /// <summary>缺少关联 ID。</summary>
        MissingCorrelationId = 5,
        /// <summary>缺少行为者 ID。</summary>
        MissingActorId = 6,
        /// <summary>命令种类未知。</summary>
        UnknownCommandKind = 7,
        /// <summary>动作 ID 与种类不一致。</summary>
        ActionIdMismatch = 8,
        /// <summary>稳定标识格式非法。</summary>
        InvalidIdentifier = 9,
        /// <summary>缺少屏幕上下文。</summary>
        MissingScreenId = 10,
        /// <summary>屏幕未登记。</summary>
        UnknownScreenId = 11,
        /// <summary>缺少面板上下文。</summary>
        MissingPanelId = 12,
        /// <summary>面板未登记。</summary>
        UnknownPanelId = 13,
        /// <summary>目标稳定 ID 是该命令的必需字段。</summary>
        MissingTargetId = 14,
        /// <summary>参数集合含空键、重复键或非法值。</summary>
        InvalidParameter = 15,
        /// <summary>同一命令 ID 已处理且内容一致。</summary>
        DuplicateCommand = 16,
        /// <summary>同一命令 ID 携带了不同内容。</summary>
        DuplicateCommandConflict = 17,
        /// <summary>调用方没有该命令权限。</summary>
        PermissionDenied = 18,
        /// <summary>请求的屏幕/面板不是当前活动上下文。</summary>
        ContextMismatch = 19,
        /// <summary>请求没有当前逻辑焦点。</summary>
        FocusMismatch = 20,
        /// <summary>显式世界日冷却尚未结束。</summary>
        CooldownActive = 21,
        /// <summary>命令被前置禁用。</summary>
        Disabled = 22,
        /// <summary>世界日或存档修订不一致。</summary>
        RevisionMismatch = 23,
        /// <summary>会话当前不接受命令。</summary>
        SessionInactive = 24,
        /// <summary>声明的 owner 与命令种类不一致。</summary>
        OwnerSystemMismatch = 25,
        /// <summary>当前面板不能接收命令。</summary>
        CommandTargetUnavailable = 26,
    }

    /// <summary>一个稳定键值参数；参数不能携带 Unity 对象或不可审计引用。</summary>
    [Serializable]
    public sealed class D22UiParameter
    {
        /// <summary>稳定参数名。</summary>
        public string Key = string.Empty;
        /// <summary>稳定标量值；具体业务语义由 owner 解释。</summary>
        public string Value = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22UiParameter Clone()
        {
            return new D22UiParameter
            {
                Key = Key ?? string.Empty,
                Value = Value ?? string.Empty,
            };
        }
    }

    /// <summary>命令种类的稳定定义。</summary>
    [Serializable]
    public sealed class D22UiCommandDefinition
    {
        /// <summary>命令种类。</summary>
        public D22UiCommandKind Kind;
        /// <summary>稳定动作 ID。</summary>
        public string ActionId = string.Empty;
        /// <summary>建议接收该请求的公开 owner 标识。</summary>
        public string OwnerSystem = string.Empty;
        /// <summary>是否必须带 target_id。</summary>
        public bool RequiresTarget;
        /// <summary>是否必须拥有可命令的面板上下文。</summary>
        public bool RequiresCommandPanel = true;
        /// <summary>返回防御性副本。</summary>
        public D22UiCommandDefinition Clone()
        {
            return new D22UiCommandDefinition
            {
                Kind = Kind,
                ActionId = ActionId ?? string.Empty,
                OwnerSystem = OwnerSystem ?? string.Empty,
                RequiresTarget = RequiresTarget,
                RequiresCommandPanel = RequiresCommandPanel,
            };
        }
    }

    /// <summary>已处理命令的只读幂等索引条目。</summary>
    [Serializable]
    public sealed class D22ProcessedCommand
    {
        /// <summary>稳定命令 ID。</summary>
        public string CommandId = string.Empty;
        /// <summary>请求内容指纹。</summary>
        public string Fingerprint = string.Empty;
        /// <summary>关联 ID。</summary>
        public string CorrelationId = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22ProcessedCommand Clone()
        {
            return new D22ProcessedCommand
            {
                CommandId = CommandId ?? string.Empty,
                Fingerprint = Fingerprint ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
            };
        }
    }

    /// <summary>UI 命令校验所需的逻辑上下文快照。</summary>
    [Serializable]
    public sealed class D22UiCommandContext
    {
        /// <summary>上下文契约版本。</summary>
        public int SchemaVersion = D22UiCommandContracts.CurrentSchemaVersion;
        /// <summary>当前活动屏幕。</summary>
        public string ActiveScreenId = string.Empty;
        /// <summary>当前活动普通面板。</summary>
        public string ActivePanelId = string.Empty;
        /// <summary>当前焦点控制 ID。</summary>
        public string FocusedControlId = D22FocusControlIds.None;
        /// <summary>当前阻塞模态 ID；为空表示没有阻塞模态。</summary>
        public string ActiveModalId = string.Empty;
        /// <summary>当前阻塞模态 owner。</summary>
        public string ActiveModalOwnerId = string.Empty;
        /// <summary>当前上下文允许的行为者。</summary>
        public string ActorId = string.Empty;
        /// <summary>当前世界日。</summary>
        public int WorldDay;
        /// <summary>当前逻辑存档修订。</summary>
        public long SaveRevision;
        /// <summary>会话是否活动。</summary>
        public bool SessionActive = true;
        /// <summary>屏幕/面板上下文是否完整有效。</summary>
        public bool ContextValid = true;
        /// <summary>外部权限 owner 是否准许。</summary>
        public bool PermissionGranted = true;
        /// <summary>本地前置是否启用命令。</summary>
        public bool CommandEnabled = true;
        /// <summary>禁用原因稳定码；空值表示没有额外禁用原因。</summary>
        public string DisabledReasonCode = string.Empty;
        /// <summary>在该世界日前不得再次提交同类命令；0 表示没有冷却。</summary>
        public int CooldownUntilWorldDay;
        /// <summary>非空时只允许列表中的命令种类。</summary>
        public List<D22UiCommandKind> AllowedCommandKinds = new List<D22UiCommandKind>();
        /// <summary>已处理命令索引；校验器只读，不会写入。</summary>
        public List<D22ProcessedCommand> ProcessedCommands = new List<D22ProcessedCommand>();

        /// <summary>返回深拷贝，避免校验器持有调用方可变集合。</summary>
        public D22UiCommandContext Clone()
        {
            var result = new D22UiCommandContext
            {
                SchemaVersion = SchemaVersion,
                ActiveScreenId = ActiveScreenId ?? string.Empty,
                ActivePanelId = ActivePanelId ?? string.Empty,
                FocusedControlId = FocusedControlId ?? D22FocusControlIds.None,
                ActiveModalId = ActiveModalId ?? string.Empty,
                ActiveModalOwnerId = ActiveModalOwnerId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                SessionActive = SessionActive,
                ContextValid = ContextValid,
                PermissionGranted = PermissionGranted,
                CommandEnabled = CommandEnabled,
                DisabledReasonCode = DisabledReasonCode ?? string.Empty,
                CooldownUntilWorldDay = CooldownUntilWorldDay,
            };
            if (AllowedCommandKinds != null)
                result.AllowedCommandKinds.AddRange(AllowedCommandKinds);
            if (ProcessedCommands != null)
            {
                foreach (var command in ProcessedCommands)
                    result.ProcessedCommands.Add(command == null ? null : command.Clone());
            }
            return result;
        }
    }

    /// <summary>UI 命令请求信封；所有字段都是稳定逻辑数据。</summary>
    [Serializable]
    public sealed class D22UiCommandRequest
    {
        /// <summary>命令契约版本。</summary>
        public int SchemaVersion = D22UiCommandContracts.CurrentSchemaVersion;
        /// <summary>跨重试不变的命令 ID。</summary>
        public string CommandId = string.Empty;
        /// <summary>把一次 UI 交互与 owner 回执关联起来的 ID。</summary>
        public string CorrelationId = string.Empty;
        /// <summary>稳定动作 ID。</summary>
        public string ActionId = string.Empty;
        /// <summary>命令种类。</summary>
        public D22UiCommandKind Kind = D22UiCommandKind.None;
        /// <summary>提交请求的行为者 ID。</summary>
        public string ActorId = string.Empty;
        /// <summary>请求发出的屏幕 ID。</summary>
        public string ScreenId = string.Empty;
        /// <summary>请求发出的面板 ID。</summary>
        public string PanelId = string.Empty;
        /// <summary>适用时的业务目标稳定 ID。</summary>
        public string TargetId = string.Empty;
        /// <summary>请求发生的世界日。</summary>
        public int WorldDay;
        /// <summary>请求发生时的逻辑存档修订。</summary>
        public long SaveRevision;
        /// <summary>调用方声明的公开 owner；不等于 owner 已接受。</summary>
        public string OwnerSystem = string.Empty;
        /// <summary>发出请求时的逻辑焦点控制 ID。</summary>
        public string FocusedControlId = D22FocusControlIds.None;
        /// <summary>稳定参数。</summary>
        public List<D22UiParameter> Parameters = new List<D22UiParameter>();
        /// <summary>可追溯的已存在事实事件 ID。</summary>
        public List<string> CauseEventIds = new List<string>();

        /// <summary>无参构造器供序列化器使用。</summary>
        public D22UiCommandRequest() { }

        /// <summary>按稳定命令定义构造请求，自动填充动作和 owner 提示。</summary>
        public static D22UiCommandRequest Create(
            D22UiCommandKind kind,
            string commandId,
            string correlationId,
            string actorId,
            string screenId,
            string panelId,
            string targetId = "",
            int worldDay = 0,
            long saveRevision = 0)
        {
            D22UiCommandDefinition definition;
            if (!D22UiCommandContracts.TryGetDefinition(kind, out definition))
                throw new ArgumentException("Unknown D22 UI command kind.", "kind");
            return new D22UiCommandRequest
            {
                Kind = kind,
                ActionId = definition.ActionId,
                OwnerSystem = definition.OwnerSystem,
                CommandId = commandId ?? string.Empty,
                CorrelationId = correlationId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                ScreenId = screenId ?? string.Empty,
                PanelId = panelId ?? string.Empty,
                TargetId = targetId ?? string.Empty,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
            };
        }

        /// <summary>返回深拷贝。</summary>
        public D22UiCommandRequest Clone()
        {
            var result = new D22UiCommandRequest
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                ActionId = ActionId ?? string.Empty,
                Kind = Kind,
                ActorId = ActorId ?? string.Empty,
                ScreenId = ScreenId ?? string.Empty,
                PanelId = PanelId ?? string.Empty,
                TargetId = TargetId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                OwnerSystem = OwnerSystem ?? string.Empty,
                FocusedControlId = FocusedControlId ?? D22FocusControlIds.None,
            };
            if (Parameters != null)
            {
                foreach (var parameter in Parameters)
                    result.Parameters.Add(parameter == null ? null : parameter.Clone());
            }
            if (CauseEventIds != null)
                result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        /// <summary>按稳定键查找参数。</summary>
        public bool TryGetParameter(string key, out string value)
        {
            if (Parameters != null)
            {
                foreach (var parameter in Parameters)
                {
                    if (parameter != null && string.Equals(parameter.Key, key, StringComparison.Ordinal))
                    {
                        value = parameter.Value ?? string.Empty;
                        return true;
                    }
                }
            }
            value = string.Empty;
            return false;
        }

        /// <summary>执行只读的契约形状校验。</summary>
        public D22UiCommandValidationCode ValidateShape()
        {
            return D22UiCommandContracts.ValidateShape(this);
        }

        /// <summary>返回用于幂等判断的确定性请求指纹。</summary>
        public string Fingerprint()
        {
            return D22UiCommandContracts.ComputeFingerprint(this);
        }
    }

    /// <summary>UI 命令结果；业务 owner 的事实结果只以 OwnerResultCode 表达。</summary>
    [Serializable]
    public sealed class D22UiCommandResult
    {
        /// <summary>结果契约版本。</summary>
        public int SchemaVersion = D22UiCommandContracts.CurrentSchemaVersion;
        /// <summary>对应请求命令 ID。</summary>
        public string CommandId = string.Empty;
        /// <summary>对应关联 ID。</summary>
        public string CorrelationId = string.Empty;
        /// <summary>生命周期结果。</summary>
        public D22UiCommandOutcome Outcome = D22UiCommandOutcome.Pending;
        /// <summary>校验码；owner 结果不覆盖 D22 校验事实。</summary>
        public D22UiCommandValidationCode ValidationCode;
        /// <summary>是否通过 D22 本地验证。</summary>
        public bool Accepted;
        /// <summary>是否是幂等重复回执。</summary>
        public bool Duplicate;
        /// <summary>UI 是否收到明确的业务状态变化事实；默认由 owner 填写。</summary>
        public bool BusinessStateChanged;
        /// <summary>实际业务裁决 owner。</summary>
        public string OwnerSystem = string.Empty;
        /// <summary>owner 返回的稳定结果码。</summary>
        public string OwnerResultCode = string.Empty;
        /// <summary>D22 反馈目录稳定码。</summary>
        public string FeedbackCode = string.Empty;
        /// <summary>D22 禁用原因稳定码。</summary>
        public string DisabledReasonCode = string.Empty;
        /// <summary>机器可读的本地校验/拒绝原因 key。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>请求关联的存档修订。</summary>
        public long SaveRevision;

        /// <summary>返回深拷贝。</summary>
        public D22UiCommandResult Clone()
        {
            return new D22UiCommandResult
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                Outcome = Outcome,
                ValidationCode = ValidationCode,
                Accepted = Accepted,
                Duplicate = Duplicate,
                BusinessStateChanged = BusinessStateChanged,
                OwnerSystem = OwnerSystem ?? string.Empty,
                OwnerResultCode = OwnerResultCode ?? string.Empty,
                FeedbackCode = FeedbackCode ?? string.Empty,
                DisabledReasonCode = DisabledReasonCode ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                SaveRevision = SaveRevision,
            };
        }

        /// <summary>创建本地拒绝结果。</summary>
        public static D22UiCommandResult Rejected(
            D22UiCommandRequest request,
            D22UiCommandValidationCode code,
            string disabledReasonCode = "",
            string reasonCode = "")
        {
            return new D22UiCommandResult
            {
                CommandId = request == null ? string.Empty : request.CommandId ?? string.Empty,
                CorrelationId = request == null ? string.Empty : request.CorrelationId ?? string.Empty,
                Outcome = D22UiCommandOutcome.Rejected,
                ValidationCode = code,
                Accepted = false,
                DisabledReasonCode = disabledReasonCode ?? string.Empty,
                ReasonCode = reasonCode ?? string.Empty,
                SaveRevision = request == null ? 0 : request.SaveRevision,
            };
        }
    }

    /// <summary>D22-L05 UI 命令稳定定义与只读指纹工具。</summary>
    public static class D22UiCommandContracts
    {
        /// <summary>当前命令契约版本。</summary>
        public const int CurrentSchemaVersion = 1;
        /// <summary>命令契约 owner。</summary>
        public const string OwnerSystem = "D22UiCommandContracts";
        /// <summary>命令/关联/行为者 ID 最大长度。</summary>
        public const int IdentifierMaxLength = 160;
        /// <summary>参数名最大长度。</summary>
        public const int ParameterKeyMaxLength = 80;
        /// <summary>参数值最大长度。</summary>
        public const int ParameterValueMaxLength = 512;

        private static readonly ReadOnlyCollection<D22UiCommandDefinition> Definitions =
            new ReadOnlyCollection<D22UiCommandDefinition>(new List<D22UiCommandDefinition>
            {
                Definition(D22UiCommandKind.OpenPanel, "ui.open_panel", OwnerSystem, true, false),
                Definition(D22UiCommandKind.ClosePanel, "ui.close_panel", OwnerSystem, true, true),
                Definition(D22UiCommandKind.NewGame, "ui.new_game", "Core/Progression", false, true),
                Definition(D22UiCommandKind.ContinueGame, "ui.continue_game", "SaveSystem", false, true),
                Definition(D22UiCommandKind.SaveGame, "ui.save_game", "SaveSystem", false, true),
                Definition(D22UiCommandKind.LoadGame, "ui.load_game", "SaveSystem", true, true),
                Definition(D22UiCommandKind.Pause, "ui.pause", "PauseOwner", false, false),
                Definition(D22UiCommandKind.Resume, "ui.resume", "PauseOwner", false, false),
                Definition(D22UiCommandKind.PortAction, "ui.port_action", "D19PortCommandRouter", true, true),
                Definition(D22UiCommandKind.MarketAction, "ui.market_action", "Economy/Market", true, true),
                Definition(D22UiCommandKind.InventoryAction, "ui.inventory_action", "Inventory/Cargo", true, true),
                Definition(D22UiCommandKind.ShipAction, "ui.ship_action", "Ship/Voyage", true, true),
                Definition(D22UiCommandKind.CrewAction, "ui.crew_action", "Character/Crew", true, true),
                Definition(D22UiCommandKind.QuestAction, "ui.quest_action", "Quest/Event", true, true),
                Definition(D22UiCommandKind.DialogueAction, "ui.dialogue_action", "Character/Crew", true, true),
                Definition(D22UiCommandKind.AccessibilityUpdate, "ui.accessibility_update", "D16AccessibilitySettingsStore", false, true),
                Definition(D22UiCommandKind.AcknowledgeFeedback, "ui.acknowledge_feedback", OwnerSystem, true, false),
                Definition(D22UiCommandKind.RetryOwnerCommand, "ui.retry_owner_command", OwnerSystem, true, true),
                Definition(D22UiCommandKind.CancelOwnerCommand, "ui.cancel_owner_command", OwnerSystem, true, true),
            });

        /// <summary>返回稳定定义副本。</summary>
        public static IReadOnlyList<D22UiCommandDefinition> GetDefinitions()
        {
            var result = new List<D22UiCommandDefinition>();
            foreach (var definition in Definitions)
                result.Add(definition.Clone());
            return new ReadOnlyCollection<D22UiCommandDefinition>(result);
        }

        /// <summary>按命令种类获取稳定定义副本。</summary>
        public static bool TryGetDefinition(D22UiCommandKind kind, out D22UiCommandDefinition definition)
        {
            foreach (var candidate in Definitions)
            {
                if (candidate.Kind == kind)
                {
                    definition = candidate.Clone();
                    return true;
                }
            }
            definition = null;
            return false;
        }

        /// <summary>只检查命令信封，不读取权限、焦点或业务状态。</summary>
        public static D22UiCommandValidationCode ValidateShape(D22UiCommandRequest request)
        {
            if (request == null) return D22UiCommandValidationCode.MissingRequest;
            if (request.SchemaVersion != CurrentSchemaVersion) return D22UiCommandValidationCode.InvalidSchemaVersion;
            if (!IsStableToken(request.CommandId)) return D22UiCommandValidationCode.MissingCommandId;
            if (!IsStableToken(request.CorrelationId)) return D22UiCommandValidationCode.MissingCorrelationId;
            if (!IsStableToken(request.ActorId)) return D22UiCommandValidationCode.MissingActorId;

            D22UiCommandDefinition definition;
            if (!TryGetDefinition(request.Kind, out definition)) return D22UiCommandValidationCode.UnknownCommandKind;
            if (!string.Equals(request.ActionId, definition.ActionId, StringComparison.Ordinal))
                return D22UiCommandValidationCode.ActionIdMismatch;
            if (!string.IsNullOrEmpty(request.OwnerSystem) &&
                !string.Equals(request.OwnerSystem, definition.OwnerSystem, StringComparison.Ordinal))
                return D22UiCommandValidationCode.OwnerSystemMismatch;
            if (!D22ScreenContracts.IsKnownScreenId(request.ScreenId))
                return string.IsNullOrEmpty(request.ScreenId)
                    ? D22UiCommandValidationCode.MissingScreenId
                    : D22UiCommandValidationCode.UnknownScreenId;
            if (definition.RequiresCommandPanel && !D22ScreenContracts.IsKnownPanelId(request.PanelId))
                return string.IsNullOrEmpty(request.PanelId)
                    ? D22UiCommandValidationCode.MissingPanelId
                    : D22UiCommandValidationCode.UnknownPanelId;
            if (!definition.RequiresCommandPanel && !string.IsNullOrEmpty(request.PanelId) &&
                !D22ScreenContracts.IsKnownPanelId(request.PanelId))
                return D22UiCommandValidationCode.UnknownPanelId;
            if (definition.RequiresTarget && !IsStableToken(request.TargetId))
                return D22UiCommandValidationCode.MissingTargetId;
            if (request.WorldDay < 0 || request.SaveRevision < 0)
                return D22UiCommandValidationCode.RevisionMismatch;
            if (!IsOptionalStableToken(request.FocusedControlId))
                return D22UiCommandValidationCode.InvalidIdentifier;
            if (request.CauseEventIds == null)
                return D22UiCommandValidationCode.InvalidParameter;
            var causeEventIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var causeEventId in request.CauseEventIds)
                if (!IsStableToken(causeEventId) || !causeEventIds.Add(causeEventId))
                    return D22UiCommandValidationCode.InvalidParameter;
            return ValidateParameters(request.Parameters);
        }

        /// <summary>验证参数键唯一、值为稳定标量且长度受限。</summary>
        public static D22UiCommandValidationCode ValidateParameters(IList<D22UiParameter> parameters)
        {
            if (parameters == null) return D22UiCommandValidationCode.InvalidParameter;
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var parameter in parameters)
            {
                if (parameter == null || !IsStableToken(parameter.Key) || parameter.Key.Length > ParameterKeyMaxLength ||
                    parameter.Value == null || parameter.Value.Length > ParameterValueMaxLength ||
                    !keys.Add(parameter.Key))
                    return D22UiCommandValidationCode.InvalidParameter;
            }
            return D22UiCommandValidationCode.None;
        }

        /// <summary>计算顺序稳定、参数顺序无关的请求指纹。</summary>
        public static string ComputeFingerprint(D22UiCommandRequest request)
        {
            if (request == null) return string.Empty;
            var builder = new StringBuilder();
            AppendPart(builder, request.SchemaVersion.ToString(CultureInfo.InvariantCulture));
            AppendPart(builder, request.CommandId);
            AppendPart(builder, request.CorrelationId);
            AppendPart(builder, request.ActionId);
            AppendPart(builder, request.Kind.ToString());
            AppendPart(builder, request.ActorId);
            AppendPart(builder, request.ScreenId);
            AppendPart(builder, request.PanelId);
            AppendPart(builder, request.TargetId);
            AppendPart(builder, request.WorldDay.ToString(CultureInfo.InvariantCulture));
            AppendPart(builder, request.SaveRevision.ToString(CultureInfo.InvariantCulture));
            AppendPart(builder, request.OwnerSystem);
            AppendPart(builder, request.FocusedControlId);

            var parameters = new List<D22UiParameter>();
            if (request.Parameters != null)
            {
                foreach (var parameter in request.Parameters)
                    if (parameter != null) parameters.Add(parameter);
            }
            parameters.Sort(delegate (D22UiParameter left, D22UiParameter right)
            {
                int key = string.Compare(left.Key ?? string.Empty, right.Key ?? string.Empty, StringComparison.Ordinal);
                return key != 0
                    ? key
                    : string.Compare(left.Value ?? string.Empty, right.Value ?? string.Empty, StringComparison.Ordinal);
            });
            foreach (var parameter in parameters)
            {
                AppendPart(builder, parameter.Key);
                AppendPart(builder, parameter.Value);
            }
            if (request.CauseEventIds != null)
            {
                var causes = new List<string>(request.CauseEventIds);
                causes.Sort(StringComparer.Ordinal);
                foreach (var cause in causes) AppendPart(builder, cause);
            }
            return builder.ToString();
        }

        /// <summary>验证 ASCII 稳定 token，不允许空白或用户可见文案。</summary>
        public static bool IsStableToken(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > IdentifierMaxLength)
                return false;
            foreach (var character in value)
            {
                if (!((character >= 'a' && character <= 'z') ||
                      (character >= 'A' && character <= 'Z') ||
                      (character >= '0' && character <= '9') ||
                      character == '.' || character == '_' || character == '-' || character == ':'))
                    return false;
            }
            return true;
        }

        /// <summary>允许空值的稳定 token，用于非命令目标的焦点占位。</summary>
        public static bool IsOptionalStableToken(string value)
        {
            return string.IsNullOrEmpty(value) || IsStableToken(value);
        }

        private static D22UiCommandDefinition Definition(
            D22UiCommandKind kind,
            string actionId,
            string ownerSystem,
            bool requiresTarget,
            bool requiresCommandPanel)
        {
            return new D22UiCommandDefinition
            {
                Kind = kind,
                ActionId = actionId,
                OwnerSystem = ownerSystem,
                RequiresTarget = requiresTarget,
                RequiresCommandPanel = requiresCommandPanel,
            };
        }

        private static void AppendPart(StringBuilder builder, string value)
        {
            value = value ?? string.Empty;
            builder.Append(value.Length).Append(':').Append(value).Append('|');
        }
    }
}
