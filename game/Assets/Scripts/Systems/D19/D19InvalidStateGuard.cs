// ============================================================
// D19InvalidStateGuard —— D19 集中式不变式/非法状态防护（纯 C#，D19-L24）
//
// 归属：Whalebone.Systems.D19（D19 唯一集中防护器，D19-L24）
// 目的：为 D19 港口编排提供统一校验语义——生命周期迁移、港口/泊位/服务归属、
//       命令 ID、服务前置、船员位置语义与状态组合。输入无效时必须拒绝、
//       原状态不变，并返回 D19-L17 业务失败码 + D19-L18 本地化 key；
//       绝不吞异常，也绝不通过 bool 默认值"自动修复"。
// 边界：
//   - 只读消费 D19PortLifecycleContracts（L02）、D19PortAnchorCatalog（L04）、
//     D19PortFailureCatalog（L17）、D19PortLocalizationKeys（L18）；
//     不修改这四个文件；不复制任何状态字段；
//   - 不引用 Unity（Transform/Canvas/Input/网络/LLM）；不写任何显示文案；
//   - 只校验：不拥有、不写入市场/库存/船舶/保存；不执行任何命令副作用；
//   - 异常策略：业务输入非法 → 显式 D19GuardVerdict；基础设施引用（如 catalog
//     为 null）属于编程错误 → 让异常向上传播，绝不 try/catch 吞掉。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 船员位置语义（D19-L24，纯逻辑输入，非持久状态）。
    /// 登船要求 OnShore → OnBoard；下船要求 OnBoard → OnShore；
    /// 位置未提供（Unknown）时跳过位置一致性检查。
    /// </summary>
    public enum D19CrewLocation
    {
        /// <summary>占位：未知/未提供。</summary>
        Unknown = 0,

        /// <summary>在岸上（未登船）。</summary>
        OnShore,

        /// <summary>在船上（已登船）。</summary>
        OnBoard
    }

    /// <summary>
    /// 防护结果（D19-L24）。Allowed=true 表示可继续；Allowed=false 时
    /// BusinessCode/LocKey 来自 L17 目录条目（与 L18 key 绑定），
    /// ReasonId 为精确稳定子原因（生命周期原因 ID / 锚点错误码 / 语义详情），
    /// Params 只包含该条目参数白名单内的值（不泄露内部对象）。
    /// </summary>
    public sealed class D19GuardVerdict
    {
        /// <summary>是否允许继续（false 时其余字段有效）。</summary>
        public bool Allowed;

        /// <summary>稳定业务失败码（L17，Allowed=false 时非空）。</summary>
        public string BusinessCode = string.Empty;

        /// <summary>稳定本地化 key（L17 条目绑定 L18 常量，Allowed=false 时非空）。</summary>
        public string LocKey = string.Empty;

        /// <summary>重试建议（L17 条目）。</summary>
        public D19FailureRetryPolicy Retry;

        /// <summary>生命周期子原因（L02；无对应时 None）。</summary>
        public D19PortFailureCode LifecycleCode;

        /// <summary>精确稳定子原因（小写蛇形，可入事件与存档）。</summary>
        public string ReasonId = string.Empty;

        /// <summary>白名单参数（name=value；只含条目 ParamWhitelist 允许的稳定标量）。</summary>
        public Dictionary<string, string> Params = new Dictionary<string, string>();

        /// <summary>放行结果（每次调用返回新实例，无共享可变状态）。</summary>
        public static D19GuardVerdict Allow()
        {
            D19GuardVerdict verdict = new D19GuardVerdict();
            verdict.Allowed = true;
            return verdict;
        }
    }

    /// <summary>
    /// 集中式不变式/非法状态防护（D19-L24，所有者：本防护器）。
    /// 后续 D19-L03/L05—L10 与命令路由复用同一校验语义；防护器不裁决任何
    /// 业务数值，不写入市场/库存/船舶/保存。
    /// </summary>
    public static class D19InvalidStateGuard
    {
        // ------------------------------------------------------------
        // 稳定子原因（小写蛇形，可入事件/存档/日志；非显示文案）
        // ------------------------------------------------------------

        public const string ReasonMissingCommandId = "missing_command_id";
        public const string ReasonInvalidCommandId = "invalid_command_id";
        public const string ReasonInvariantViolation = "invariant_violation";
        public const string ReasonInvalidPortKey = "invalid_port_key";
        public const string ReasonUnknownPort = "unknown_port";
        public const string ReasonInvalidBerthKey = "invalid_berth_key";
        public const string ReasonUnknownBerth = "unknown_berth";
        public const string ReasonCrossPortBerth = "cross_port_berth";
        public const string ReasonUnknownService = "unknown_service";
        public const string ReasonCrossPortService = "cross_port_service";
        public const string ReasonNotBerthed = "not_berthed";
        public const string ReasonNotMoored = "not_moored";
        public const string ReasonAlreadyOnBoard = "already_on_board";
        public const string ReasonNotOnBoard = "not_on_board";
        public const string ReasonCrewNotOnBoard = "crew_not_on_board";
        public const string ReasonCrewOnShoreAfterDeparture = "crew_on_shore_after_departure";
        public const string ReasonStateNoneWithIds = "state_none_with_ids";
        public const string ReasonStateRequiresBerth = "state_requires_berth";

        // ------------------------------------------------------------
        // 生命周期迁移（L02 迁移表封装；原状态不变，仅 out 输出目标态）
        // ------------------------------------------------------------

        /// <summary>按 L02 冻结迁移表判定 (from, reason) 迁移；拒绝时返回
        /// L17 业务码 + L18 key 的 D19GuardVerdict，原状态不变。</summary>
        public static bool CanTransition(
            D19PortVisitState from,
            D19PortTransitionReason reason,
            out D19PortVisitState to,
            out D19GuardVerdict verdict)
        {
            D19PortFailureCode rejection;
            if (D19PortLifecycleContracts.CanTransition(from, reason, out to, out rejection))
            {
                verdict = D19GuardVerdict.Allow();
                return true;
            }

            D19PortFailureCode detail = D19PortLifecycleContracts.ResolveRejectionCode(from, reason);
            verdict = RejectForTransition(reason, detail, EnumToSnake(detail), from);
            return false;
        }

        private static D19GuardVerdict RejectForTransition(
            D19PortTransitionReason reason,
            D19PortFailureCode detail,
            string reasonId,
            D19PortVisitState from)
        {
            D19FailureEntry entry = D19PortFailureCatalog.ForLifecycleCode(detail);
            if (entry == null)
            {
                switch (reason)
                {
                    case D19PortTransitionReason.Boarded:
                        entry = D19PortFailureCatalog.Find("d19.boarding.failed");
                        break;
                    case D19PortTransitionReason.Departed:
                        entry = D19PortFailureCatalog.Find("d19.departure.failed");
                        break;
                    case D19PortTransitionReason.BerthAssigned:
                        entry = D19PortFailureCatalog.Find("d19.berth.occupied");
                        break;
                    case D19PortTransitionReason.ServiceStarted:
                        entry = D19PortFailureCatalog.Find("d19.service.unavailable");
                        break;
                    default:
                        entry = D19PortFailureCatalog.Find("d19.lifecycle.invalid_transition");
                        break;
                }
            }
            if (entry == null)
            {
                entry = D19PortFailureCatalog.Find("d19.lifecycle.invalid_transition");
            }

            Dictionary<string, string> available = new Dictionary<string, string>();
            available["from_state"] = EnumToSnake(from);
            available["reason_id"] = reasonId;
            available["state"] = EnumToSnake(from);
            return Reject(entry.Code, reasonId, detail, available);
        }

        // ------------------------------------------------------------
        // 命令 ID（稳定命令标识；缺失/非法 → 拒绝，绝不静默成功）
        // ------------------------------------------------------------

        /// <summary>命令 ID 必须非空、无空白/控制字符、长度 ≤ 64。</summary>
        public static D19GuardVerdict CheckCommandId(string commandId)
        {
            if (string.IsNullOrEmpty(commandId))
            {
                return Reject("d19.lifecycle.invalid_transition", ReasonMissingCommandId, D19PortFailureCode.MissingCommandId);
            }
            if (commandId.Length > 64)
            {
                return Reject("d19.lifecycle.invalid_transition", ReasonInvalidCommandId, D19PortFailureCode.MissingCommandId);
            }
            foreach (char c in commandId)
            {
                if (char.IsWhiteSpace(c) || char.IsControl(c))
                {
                    return Reject("d19.lifecycle.invalid_transition", ReasonInvalidCommandId, D19PortFailureCode.MissingCommandId);
                }
            }
            return D19GuardVerdict.Allow();
        }

        // ------------------------------------------------------------
        // 港口 / 泊位 / 服务归属（L04 锚点目录封装）
        // ------------------------------------------------------------

        /// <summary>港口键必须存在于锚点目录（格式非法/未知 → 拒绝）。</summary>
        public static D19GuardVerdict CheckPort(D19PortAnchorCatalog catalog, string portId)
        {
            D19AnchorResult result = catalog.ResolvePort(portId);
            if (result.Success)
            {
                return D19GuardVerdict.Allow();
            }
            string reasonId = result.ErrorCode == D19AnchorErrorCodes.InvalidKeyFormat ? ReasonInvalidPortKey : ReasonUnknownPort;
            return Reject("d19.port.unknown_port", reasonId, D19PortFailureCode.UnknownPort, portId);
        }

        /// <summary>泊位必须存在且归属于指定港口（跨港/未知/格式非法 → 拒绝，
        /// 子原因精确保留；业务码使用 L17 泊位域条目）。</summary>
        public static D19GuardVerdict CheckBerth(D19PortAnchorCatalog catalog, string berthId, string portId)
        {
            D19AnchorResult result = catalog.ResolveBerth(berthId, portId);
            if (result.Success)
            {
                return D19GuardVerdict.Allow();
            }
            string reasonId;
            switch (result.ErrorCode)
            {
                case D19AnchorErrorCodes.InvalidKeyFormat:
                    reasonId = ReasonInvalidBerthKey;
                    break;
                case D19AnchorErrorCodes.CrossPortBerth:
                    reasonId = ReasonCrossPortBerth;
                    break;
                case D19AnchorErrorCodes.AliasCycle:
                case D19AnchorErrorCodes.DuplicateKey:
                    reasonId = result.ErrorCode;
                    break;
                default:
                    reasonId = ReasonUnknownBerth;
                    break;
            }
            Dictionary<string, string> available = new Dictionary<string, string>();
            available["berth_id"] = berthId ?? string.Empty;
            available["port_id"] = portId ?? string.Empty;
            return Reject("d19.berth.occupied", reasonId, D19PortFailureCode.BerthOccupied, available);
        }

        /// <summary>服务必须存在且归属于指定港口（跨港/未知 → 拒绝，
        /// 子原因精确保留；业务码使用 L17 服务域条目）。</summary>
        public static D19GuardVerdict CheckService(D19PortAnchorCatalog catalog, string serviceId, string portId)
        {
            D19AnchorResult result = catalog.ResolveService(serviceId, portId);
            if (result.Success)
            {
                return D19GuardVerdict.Allow();
            }
            string reasonId = result.ErrorCode == D19AnchorErrorCodes.CrossPortService ? ReasonCrossPortService : ReasonUnknownService;
            Dictionary<string, string> available = new Dictionary<string, string>();
            available["service"] = serviceId ?? string.Empty;
            available["port_id"] = portId ?? string.Empty;
            return Reject("d19.service.unavailable", reasonId, D19PortFailureCode.ServiceUnavailable, available);
        }

        // ------------------------------------------------------------
        // 服务前置（归属 + 状态前置：开始服务必须处于 Berthed）
        // ------------------------------------------------------------

        /// <summary>服务请求前置：服务归属当前港口（CheckService）+ 访问状态必须
        /// 处于 Berthed（L02 迁移表：StartService 仅由 Berthed 进入 InService）。</summary>
        public static D19GuardVerdict CheckServicePrerequisite(
            D19PortAnchorCatalog catalog,
            string serviceId,
            string portId,
            D19PortVisitState state)
        {
            D19GuardVerdict ownership = CheckService(catalog, serviceId, portId);
            if (!ownership.Allowed)
            {
                return ownership;
            }
            if (state != D19PortVisitState.Berthed)
            {
                Dictionary<string, string> available = new Dictionary<string, string>();
                available["service"] = serviceId ?? string.Empty;
                available["port_id"] = portId ?? string.Empty;
                return Reject("d19.service.unavailable", ReasonNotBerthed, D19PortFailureCode.ServiceUnavailable, available);
            }
            return D19GuardVerdict.Allow();
        }

        // ------------------------------------------------------------
        // 船员位置语义（登船/下船）
        // ------------------------------------------------------------

        /// <summary>船员位置迁移语义：OnShore→OnBoard（登船）与 OnBoard→OnShore
        /// （下船）允许；重复登船/重复下船拒绝；Unknown 输入拒绝（不自动猜测）。</summary>
        public static D19GuardVerdict CheckCrewLocation(D19CrewLocation current, D19CrewLocation requested)
        {
            if (current == D19CrewLocation.Unknown || requested == D19CrewLocation.Unknown)
            {
                return Reject("d19.boarding.failed", ReasonInvariantViolation, D19PortFailureCode.AlreadyBoarded);
            }
            if (current == requested)
            {
                string reasonId = current == D19CrewLocation.OnBoard ? ReasonAlreadyOnBoard : ReasonNotOnBoard;
                return Reject("d19.boarding.failed", reasonId, D19PortFailureCode.AlreadyBoarded);
            }
            return D19GuardVerdict.Allow();
        }

        // ------------------------------------------------------------
        // 状态组合（单快照一致性）
        // ------------------------------------------------------------

        /// <summary>
        /// 状态组合一致性（一次性校验，确定性顺序）：
        /// 1) None 时不得携带 port/berth 事实；
        /// 2) 非 None 状态必须携带有效港口键；
        /// 3) Berthed/InService 必须携带属于当前港口的泊位键（Boarded 可选但若
        ///    提供也必须归属当前港口）；
        /// 4) 船员位置语义：Boarded/Departed 要求船员 OnBoard（提供了位置事实时）。
        /// 返回首个失败（顺序固定），全部通过返回 Allowed。
        /// </summary>
        public static D19GuardVerdict CheckStateCombination(
            D19PortAnchorCatalog catalog,
            D19PortVisitState state,
            string portId,
            string berthId,
            D19CrewLocation shipboardCrew)
        {
            if (state == D19PortVisitState.None)
            {
                if (!string.IsNullOrEmpty(portId) || !string.IsNullOrEmpty(berthId))
                {
                    return Reject("d19.lifecycle.invalid_transition", ReasonStateNoneWithIds, D19PortFailureCode.InvalidTransition);
                }
                return D19GuardVerdict.Allow();
            }

            D19GuardVerdict port = CheckPort(catalog, portId);
            if (!port.Allowed)
            {
                return port;
            }

            bool berthRequired = state == D19PortVisitState.Berthed || state == D19PortVisitState.InService;
            bool berthOptional = state == D19PortVisitState.Boarded;
            if (berthRequired)
            {
                if (string.IsNullOrEmpty(berthId))
                {
                    return Reject("d19.berth.occupied", ReasonStateRequiresBerth, D19PortFailureCode.UnknownBerth, berthId);
                }
                D19GuardVerdict berth = CheckBerth(catalog, berthId, portId);
                if (!berth.Allowed)
                {
                    return berth;
                }
            }
            else if (berthOptional && !string.IsNullOrEmpty(berthId))
            {
                D19GuardVerdict berth = CheckBerth(catalog, berthId, portId);
                if (!berth.Allowed)
                {
                    return berth;
                }
            }

            if (shipboardCrew != D19CrewLocation.Unknown)
            {
                if (state == D19PortVisitState.Boarded && shipboardCrew == D19CrewLocation.OnShore)
                {
                    return Reject("d19.boarding.failed", ReasonCrewNotOnBoard, D19PortFailureCode.AlreadyBoarded);
                }
                if (state == D19PortVisitState.Departed && shipboardCrew == D19CrewLocation.OnShore)
                {
                    return Reject("d19.departure.failed", ReasonCrewOnShoreAfterDeparture, D19PortFailureCode.NotReadyToDepart);
                }
            }
            return D19GuardVerdict.Allow();
        }

        // ------------------------------------------------------------
        // 不变式（L02 ValidateInvariant 封装）
        // ------------------------------------------------------------

        /// <summary>不变式校验（当前 L02 冻结：Berthed 必须携带非空 BerthId）；
        /// 违反时返回 L17 码 + L18 key，不自动修复。</summary>
        public static D19GuardVerdict CheckInvariant(D19PortVisitState state, string berthId)
        {
            D19PortFailureCode code = D19PortLifecycleContracts.ValidateInvariant(state, berthId);
            if (code == D19PortFailureCode.None)
            {
                return D19GuardVerdict.Allow();
            }
            string reasonId = code == D19PortFailureCode.UnknownBerth ? ReasonUnknownBerth : ReasonInvariantViolation;
            return RejectForTransition(D19PortTransitionReason.BerthAssigned, code, reasonId, state);
        }

        // ------------------------------------------------------------
        // 命令级组合校验（供 L03/L05—L10 与命令路由复用同一语义）
        // ------------------------------------------------------------

        /// <summary>
        /// 单条命令的完整校验：命令 ID → 港口 → 泊位（按种类要求）→ 生命周期迁移
        /// （种类 → 原因映射）→ 船员位置/状态组合。Compensate 种类不做正向迁移校验
        /// （补偿不是正向迁移）。原状态与命令都不被修改。
        /// currentBerthId 为当前持泊事实：AssignBerth 使用命令内新泊位，
        /// 其余种类使用当前泊位（若存在）。
        /// </summary>
        public static D19GuardVerdict CheckCommand(
            D19PortAnchorCatalog catalog,
            D19PortCommand command,
            D19PortVisitState currentState,
            string currentBerthId,
            D19CrewLocation shipboardCrew)
        {
            D19GuardVerdict commandId = CheckCommandId(command.CommandId);
            if (!commandId.Allowed)
            {
                return commandId;
            }

            D19PortVisitState to;
            D19GuardVerdict verdict;
            switch (command.Kind)
            {
                case D19PortCommandKind.Compensate:
                    return D19GuardVerdict.Allow();
                case D19PortCommandKind.AssignBerth:
                    D19GuardVerdict berth = CheckBerth(catalog, command.BerthId, command.PortId);
                    if (!berth.Allowed)
                    {
                        return berth;
                    }
                    break;
                default:
                    break;
            }

            if (!CanTransition(currentState, KindToReason(command.Kind), out to, out verdict))
            {
                return verdict;
            }

            string targetBerthId = command.Kind == D19PortCommandKind.AssignBerth ? command.BerthId : currentBerthId;
            D19GuardVerdict stateCombination = CheckStateCombination(catalog, to, command.PortId, targetBerthId, shipboardCrew);
            return stateCombination;
        }

        /// <summary>命令种类 → 迁移原因（确定性映射；Compensate 无正向原因）。</summary>
        public static D19PortTransitionReason KindToReason(D19PortCommandKind kind)
        {
            switch (kind)
            {
                case D19PortCommandKind.EnterPort: return D19PortTransitionReason.EnterPortAccepted;
                case D19PortCommandKind.Dock: return D19PortTransitionReason.DockAssigned;
                case D19PortCommandKind.AssignBerth: return D19PortTransitionReason.BerthAssigned;
                case D19PortCommandKind.StartService: return D19PortTransitionReason.ServiceStarted;
                case D19PortCommandKind.CompleteService: return D19PortTransitionReason.ServiceCompleted;
                case D19PortCommandKind.Board: return D19PortTransitionReason.Boarded;
                case D19PortCommandKind.Depart: return D19PortTransitionReason.Departed;
                case D19PortCommandKind.Return: return D19PortTransitionReason.Returned;
                default: return D19PortTransitionReason.None;
            }
        }

        // ------------------------------------------------------------
        // 内部工具
        // ------------------------------------------------------------

        /// <summary>枚举名 → 小写蛇形稳定标识（如 NotMoored → "not_moored"）。</summary>
        public static string EnumToSnake(Enum value)
        {
            string name = value == null ? string.Empty : value.ToString();
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            System.Text.StringBuilder builder = new System.Text.StringBuilder(name.Length + 4);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c >= 'A' && c <= 'Z')
                {
                    if (i > 0)
                    {
                        builder.Append('_');
                    }
                    builder.Append((char)(c - 'A' + 'a'));
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        /// <summary>构造拒绝结果（业务码 + L18 key + 子原因 + 生命周期码）。</summary>
        public static D19GuardVerdict Reject(
            string businessCode,
            string reasonId,
            D19PortFailureCode lifecycleCode)
        {
            return Reject(businessCode, reasonId, lifecycleCode, new Dictionary<string, string>());
        }

        /// <summary>构造拒绝结果，并附带一个端口参数（port_id 白名单）。</summary>
        public static D19GuardVerdict Reject(
            string businessCode,
            string reasonId,
            D19PortFailureCode lifecycleCode,
            string portId)
        {
            Dictionary<string, string> available = new Dictionary<string, string>();
            available["port_id"] = portId ?? string.Empty;
            return Reject(businessCode, reasonId, lifecycleCode, available);
        }

        /// <summary>构造拒绝结果：参数只保留 L17 条目白名单内的稳定标量。</summary>
        public static D19GuardVerdict Reject(
            string businessCode,
            string reasonId,
            D19PortFailureCode lifecycleCode,
            Dictionary<string, string> availableParams)
        {
            D19FailureEntry entry = D19PortFailureCatalog.Find(businessCode);
            D19GuardVerdict verdict = new D19GuardVerdict();
            verdict.Allowed = false;
            if (entry == null)
            {
                // 目录缺失属于编程错误：显式抛出，绝不静默回退为"成功"。
                throw new InvalidOperationException("guard cannot resolve L17 business code: " + businessCode);
            }
            verdict.BusinessCode = entry.Code;
            verdict.LocKey = entry.LocKey;
            verdict.Retry = entry.Retry;
            verdict.LifecycleCode = lifecycleCode;
            verdict.ReasonId = reasonId ?? string.Empty;
            if (availableParams != null)
            {
                foreach (string name in entry.ParamWhitelist)
                {
                    string value;
                    if (availableParams.TryGetValue(name, out value))
                    {
                        verdict.Params[name] = value;
                    }
                }
            }
            return verdict;
        }
    }
}
