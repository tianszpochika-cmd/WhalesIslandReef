// ============================================================
// D19PortLifecycleContracts —— D19 港口访问生命周期契约（纯 C#）
//
// 归属：Whalebone.Systems.D19（D19 章节唯一契约文件之一）
// 目的：冻结港口访问状态机、迁移原因、命令/结果 DTO、失败载体与不变式，
//       供 D19-L03 及后续任务直接消费。
// 这里不引用 Unity（Transform/坐标）、Input、Canvas、网络或 LLM；
// 不复制 D2/D3/D4/D9/Save 的任何状态字段（只读使用其公开契约，不建副本）。
// 状态互斥由枚举保证，不依赖 bool 组合；不使用坐标或 UI 字符串。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 港口访问生命周期状态（D19-L02，所有者：D19PortVisitSystem）。
    /// 每个值表示互斥的领域事实；迁移只能按 D19PortTransitionRule 表进行。
    /// 不含坐标/Transform/UI 含义，仅表达业务状态。
    /// </summary>
    public enum D19PortVisitState
    {
        /// <summary>未进入港口访问（初始态，非可持久化业务态）。</summary>
        None = 0,

        /// <summary>PortVisit：已到港（进入港口访问，航程到达或访问开始）。</summary>
        PortVisit,

        /// <summary>Docked：已靠码头（船体靠岸完成，泊位未分配）。</summary>
        Docked,

        /// <summary>Berthed：已分配泊位（泊位占用成立，携带 BerthId）。</summary>
        Berthed,

        /// <summary>InService：港口服务中（市场/仓库/酒馆/船坞/补给任一服务进行中）。</summary>
        InService,

        /// <summary>Boarded：已登船（成员在船上就绪，允许离港流程）。</summary>
        Boarded,

        /// <summary>Departed：已离港（航程已启动）。</summary>
        Departed,

        /// <summary>Returned：已返港（航程结束回到港口，可重新进入访问循环）。</summary>
        Returned
    }

    /// <summary>
    /// 迁移原因（D19-L02，所有者：D19PortVisitSystem）。
    /// 每个迁移必须同时具备允许的前态、目标态与原因；原因不可跨状态复用。
    /// </summary>
    public enum D19PortTransitionReason
    {
        /// <summary>占位：无原因（非法输入）。</summary>
        None = 0,

        /// <summary>EnterPortAccepted：进港请求被接受，进入 PortVisit。</summary>
        EnterPortAccepted,

        /// <summary>DockAssigned：码头靠泊完成，进入 Docked。</summary>
        DockAssigned,

        /// <summary>BerthAssigned：泊位分配成功，进入 Berthed（携带 BerthId）。</summary>
        BerthAssigned,

        /// <summary>ServiceStarted：港口服务开始，进入 InService。</summary>
        ServiceStarted,

        /// <summary>ServiceCompleted：服务完成，回到 Berthed。</summary>
        ServiceCompleted,

        /// <summary>Boarded：成员登船完成，进入 Boarded。</summary>
        Boarded,

        /// <summary>Departed：离港检查通过并启动航程，进入 Departed。</summary>
        Departed,

        /// <summary>Returned：航程结束返港，进入 Returned。</summary>
        Returned,

        /// <summary>RevisitAccepted：返港后重新进入访问循环，Returned → PortVisit。</summary>
        RevisitAccepted
    }

    /// <summary>
    /// 稳定失败码（D19-L02，所有者：D19PortFailureCatalog 的 L17 前身）。
    /// 失败码是机器可读稳定标识，不承载 UI 文案；文案键由 L17/L18 另行定义。
    /// </summary>
    public enum D19PortFailureCode
    {
        /// <summary>无失败（成功）。</summary>
        None = 0,

        /// <summary>状态机不允许该迁移（前态/原因不匹配）。</summary>
        InvalidTransition,

        /// <summary>港口标识未知或不可达。</summary>
        UnknownPort,

        /// <summary>泊位标识未知。</summary>
        UnknownBerth,

        /// <summary>泊位已被占用或冲突。</summary>
        BerthOccupied,

        /// <summary>当前不在港口访问状态。</summary>
        NotInPort,

        /// <summary>未完成靠泊（需要先 Docked）。</summary>
        NotMoored,

        /// <summary>未分配泊位（需要先 Berthed）。</summary>
        NotBerthed,

        /// <summary>成员已登船，重复登船被拒绝。</summary>
        AlreadyBoarded,

        /// <summary>未满足离港前置检查。</summary>
        NotReadyToDepart,

        /// <summary>命令缺少稳定 CommandId。</summary>
        MissingCommandId,

        /// <summary>命令为重复投递（幂等拒绝，结果应返回既有结果）。</summary>
        DuplicateCommand,

        /// <summary>港口服务当前不可用（时间/势力/天气/设施前置不满足）。</summary>
        ServiceUnavailable,

        /// <summary>内部状态不一致（不变式校验失败）。</summary>
        InvariantViolation
    }

    /// <summary>
    /// 迁移规则（D19-L02，所有者：D19PortVisitSystem）。
    /// 每条规则固定：允许的前态、目标态、原因、稳定原因 ID 与"该迁移被拒绝时"的稳定失败码。
    /// 后续任务只能消费此表，不得自行发明未登记迁移。
    /// </summary>
    public struct D19PortTransitionRule
    {
        /// <summary>允许的前态。</summary>
        public D19PortVisitState From;

        /// <summary>目标态。</summary>
        public D19PortVisitState To;

        /// <summary>迁移原因。</summary>
        public D19PortTransitionReason Reason;

        /// <summary>稳定原因 ID（点分小写，可入事件与存档）。</summary>
        public string ReasonId;

        /// <summary>当该迁移被拒绝时返回的稳定失败码。</summary>
        public D19PortFailureCode FailureCode;

        public D19PortTransitionRule(
            D19PortVisitState from,
            D19PortVisitState to,
            D19PortTransitionReason reason,
            string reasonId,
            D19PortFailureCode failureCode)
        {
            From = from;
            To = to;
            Reason = reason;
            ReasonId = reasonId;
            FailureCode = failureCode;
        }
    }

    /// <summary>
    /// 港口命令种类（D19-L02，所有者：D19PortCommandRouter 的 L33 前身）。
    /// 命令种类决定校验入口，不包含参数事实（参数在 D19PortCommand 中）。
    /// </summary>
    public enum D19PortCommandKind
    {
        /// <summary>占位：非法种类。</summary>
        None = 0,

        /// <summary>进港（EnterPort）。</summary>
        EnterPort,

        /// <summary>靠码头（Dock）。</summary>
        Dock,

        /// <summary>分配泊位（AssignBerth）。</summary>
        AssignBerth,

        /// <summary>开始港口服务（StartService）。</summary>
        StartService,

        /// <summary>完成港口服务（CompleteService）。</summary>
        CompleteService,

        /// <summary>登船（Board）。</summary>
        Board,

        /// <summary>离港（Depart）。</summary>
        Depart,

        /// <summary>返港（Return）。</summary>
        Return,

        /// <summary>取消/回退当前命令（Compensate）。</summary>
        Compensate
    }

    /// <summary>
    /// 港口命令 DTO（D19-L02，所有者：D19PortCommandRouter）。
    /// 稳定 CommandId 用于幂等与事件关联；PortId/BerthId 使用 D18 稳定键语义（不承载坐标）。
    /// </summary>
    public sealed class D19PortCommand
    {
        /// <summary>稳定命令 ID（调用方生成，跨重试不变）。</summary>
        public string CommandId;

        /// <summary>命令种类。</summary>
        public D19PortCommandKind Kind;

        /// <summary>目标港口稳定键。</summary>
        public string PortId;

        /// <summary>目标泊位稳定键（仅 AssignBerth 必需）。</summary>
        public string BerthId;

        /// <summary>命令发出的世界日（用于服务可用性上下文，仅读）。</summary>
        public int IssuedWorldDay;

        public D19PortCommand(string commandId, D19PortCommandKind kind, string portId, string berthId = null, int issuedWorldDay = 0)
        {
            CommandId = commandId ?? string.Empty;
            Kind = kind;
            PortId = portId ?? string.Empty;
            BerthId = berthId;
            IssuedWorldDay = issuedWorldDay;
        }
    }

    /// <summary>
    /// 港口命令结果 DTO（D19-L02，所有者：D19PortReadModel 的 L19 前身）。
    /// Success=false 时 Failure 提供稳定失败码与原因 ID；不承载 UI 文案。
    /// </summary>
    public sealed class D19PortCommandResult
    {
        /// <summary>与请求一致的稳定命令 ID。</summary>
        public string CommandId;

        /// <summary>命令是否成功。</summary>
        public bool Success;

        /// <summary>命令执行后的港口访问状态（成功或幂等命中时有效）。</summary>
        public D19PortVisitState State;

        /// <summary>失败载体（Success=false 时有效）。</summary>
        public D19PortFailure Failure;

        public D19PortCommandResult(string commandId, bool success, D19PortVisitState state, D19PortFailure failure = null)
        {
            CommandId = commandId ?? string.Empty;
            Success = success;
            State = state;
            Failure = failure;
        }
    }

    /// <summary>
    /// 失败载体（D19-L02，所有者：D19PortFailureCatalog 的 L17 前身）。
    /// 只含稳定码与原因 ID，不包含显示文案；可重试建议由 L17 细化。
    /// </summary>
    public sealed class D19PortFailure
    {
        /// <summary>稳定失败码。</summary>
        public D19PortFailureCode Code;

        /// <summary>稳定原因 ID（点分小写，如 "d19.berth.occupied"）。</summary>
        public string ReasonId;

        /// <summary>是否可重试（幂等/补偿语义由命令层决定）。</summary>
        public bool Recoverable;

        public D19PortFailure(D19PortFailureCode code, string reasonId, bool recoverable = false)
        {
            Code = code;
            ReasonId = reasonId ?? string.Empty;
            Recoverable = recoverable;
        }
    }

    /// <summary>
    /// 港口访问生命周期契约入口（D19-L02，所有者：D19PortVisitSystem）。
    /// 提供冻结迁移表与不变式校验；不触碰任何场景/坐标/输入。
    /// </summary>
    public static class D19PortLifecycleContracts
    {
        /// <summary>
        /// 冻结迁移表。每个迁移固定前态、目标态、原因、原因 ID 与拒绝失败码；
        /// 后续任务与测试必须只使用此表（表驱动，禁止另建迁移路径）。
        /// </summary>
        public static readonly IReadOnlyList<D19PortTransitionRule> TransitionRules =
            new List<D19PortTransitionRule>
            {
                // 到港：进入访问
                new D19PortTransitionRule(D19PortVisitState.None, D19PortVisitState.PortVisit, D19PortTransitionReason.EnterPortAccepted, "d19.transition.enter_port_accepted", D19PortFailureCode.NotInPort),
                new D19PortTransitionRule(D19PortVisitState.Returned, D19PortVisitState.PortVisit, D19PortTransitionReason.RevisitAccepted, "d19.transition.revisit_accepted", D19PortFailureCode.NotInPort),

                // 靠码头：PortVisit → Docked
                new D19PortTransitionRule(D19PortVisitState.PortVisit, D19PortVisitState.Docked, D19PortTransitionReason.DockAssigned, "d19.transition.dock_assigned", D19PortFailureCode.NotMoored),

                // 泊位分配：Docked → Berthed（携带 BerthId）
                new D19PortTransitionRule(D19PortVisitState.Docked, D19PortVisitState.Berthed, D19PortTransitionReason.BerthAssigned, "d19.transition.berth_assigned", D19PortFailureCode.NotBerthed),
                new D19PortTransitionRule(D19PortVisitState.Returned, D19PortVisitState.Berthed, D19PortTransitionReason.BerthAssigned, "d19.transition.berth_reassigned", D19PortFailureCode.NotBerthed),

                // 服务：Berthed → InService → Berthed
                new D19PortTransitionRule(D19PortVisitState.Berthed, D19PortVisitState.InService, D19PortTransitionReason.ServiceStarted, "d19.transition.service_started", D19PortFailureCode.ServiceUnavailable),
                new D19PortTransitionRule(D19PortVisitState.InService, D19PortVisitState.Berthed, D19PortTransitionReason.ServiceCompleted, "d19.transition.service_completed", D19PortFailureCode.InvalidTransition),

                // 登船：Docked/Berthed → Boarded
                new D19PortTransitionRule(D19PortVisitState.Docked, D19PortVisitState.Boarded, D19PortTransitionReason.Boarded, "d19.transition.boarded", D19PortFailureCode.AlreadyBoarded),
                new D19PortTransitionRule(D19PortVisitState.Berthed, D19PortVisitState.Boarded, D19PortTransitionReason.Boarded, "d19.transition.boarded", D19PortFailureCode.AlreadyBoarded),

                // 离港：Boarded（或 PortVisit 直达）→ Departed
                new D19PortTransitionRule(D19PortVisitState.Boarded, D19PortVisitState.Departed, D19PortTransitionReason.Departed, "d19.transition.departed", D19PortFailureCode.NotReadyToDepart),
                new D19PortTransitionRule(D19PortVisitState.PortVisit, D19PortVisitState.Departed, D19PortTransitionReason.Departed, "d19.transition.departed_direct", D19PortFailureCode.NotReadyToDepart),

                // 返港：Departed → Returned
                new D19PortTransitionRule(D19PortVisitState.Departed, D19PortVisitState.Returned, D19PortTransitionReason.Returned, "d19.transition.returned", D19PortFailureCode.InvalidTransition)
            };

        /// <summary>
        /// 判定迁移是否允许（前态 + 原因 → 目标态）。
        /// 返回 true 表示存在合法规则；失败时给出该路径的标准失败码（FailureCode 参数输出）。
        /// </summary>
        public static bool CanTransition(
            D19PortVisitState from,
            D19PortTransitionReason reason,
            out D19PortVisitState to,
            out D19PortFailureCode failureCode)
        {
            foreach (D19PortTransitionRule rule in TransitionRules)
            {
                if (rule.From == from && rule.Reason == reason)
                {
                    to = rule.To;
                    failureCode = D19PortFailureCode.None;
                    return true;
                }
            }

            to = from;
            failureCode = ResolveRejectionCode(from, reason);
            return false;
        }

        /// <summary>
        /// 迁移被拒时的稳定失败码：优先 InvalidTransition，若原因暗示具体前置缺口则给对应码。
        /// </summary>
        public static D19PortFailureCode ResolveRejectionCode(
            D19PortVisitState from,
            D19PortTransitionReason reason)
        {
            if (reason == D19PortTransitionReason.None)
            {
                return D19PortFailureCode.InvalidTransition;
            }

            if (reason == D19PortTransitionReason.BerthAssigned && from != D19PortVisitState.Docked && from != D19PortVisitState.Returned)
            {
                return D19PortFailureCode.NotMoored;
            }

            if (reason == D19PortTransitionReason.ServiceStarted && from != D19PortVisitState.Berthed)
            {
                return D19PortFailureCode.NotBerthed;
            }

            if (reason == D19PortTransitionReason.Boarded && from != D19PortVisitState.Docked && from != D19PortVisitState.Berthed)
            {
                return D19PortFailureCode.NotMoored;
            }

            if (reason == D19PortTransitionReason.Departed && from != D19PortVisitState.Boarded && from != D19PortVisitState.PortVisit)
            {
                return D19PortFailureCode.NotReadyToDepart;
            }

            if (reason == D19PortTransitionReason.EnterPortAccepted && from != D19PortVisitState.None && from != D19PortVisitState.Returned)
            {
                return D19PortFailureCode.AlreadyBoarded;
            }

            return D19PortFailureCode.InvalidTransition;
        }

        /// <summary>
        /// 不变式校验（D19-L02，所有者：D19PortVisitSystem）。
        /// 保证：Berthed 必须携带非空 BerthId；InService 必须由 Berthed 进入（状态机已保证）；
        /// Boarded 后不得回到 InService；Departed 前必须 Boarded（或 PortVisit 直达，见迁移表）。
        /// 返回 null 表示不变式成立；否则返回稳定失败码。
        /// </summary>
        public static D19PortFailureCode ValidateInvariant(
            D19PortVisitState state,
            string berthId,
            bool berthedFromDocked = false)
        {
            if (state == D19PortVisitState.Berthed && string.IsNullOrEmpty(berthId))
            {
                return D19PortFailureCode.UnknownBerth;
            }

            return D19PortFailureCode.None;
        }
    }
}
