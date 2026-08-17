// ============================================================
// D19PortLocalizationKeys —— D19 港口本地化逻辑键清单与纯检查 API（纯 C#）
//
// 归属：Whalebone.Systems.D19（D19 章节唯一本地化键文件，D19-L18）
// 目的：冻结 D19 行为结果使用的稳定本地化 key 常量（生命周期/服务/失败/补偿/
//       迁移提示/读模型反馈），并提供纯检查 API：key 命名规则、重复检测、
//       缺 key 报告。任何 D19 逻辑/读模型/失败反馈只能引用这里的 key 常量，
//       不得散落 UI 字符串，也不得直接调用 Unity Localization API。
// 这里不引用 Unity（Transform/Canvas/Text）、不引用 UnityEngine.Localization、
// 不翻译任何显示文案：本文件只定义逻辑键，不含中英文显示文本。
// key 与 D19PortLifecycleContracts（D19-L02）的 D19PortFailureCode /
// D19PortTransitionReason / D19PortCommandKind 一一对应；d19.port.* 事件摘要
// key 与 D19PortEventContracts（D19-L16）的 LocalizationKeyPattern 兼容；
// D19-L17 失败码目录落地后（含扩展枚举值）由 ValidateRegistry 通过反射自动
// 报告缺 key 的码。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// D19 港口本地化逻辑键清单与纯检查 API（D19-L18，所有者：D19 本地化键，
    /// 由后续 D19-L05/L19/L21 等只读消费）。
    /// 所有 key 遵循命名规则：点分小写 snake_case，首段固定 "d19"。
    /// 本类不持有任何显示文案；缺 key 必须显式失败，绝不静默回退为"成功"。
    /// </summary>
    public static class D19PortLocalizationKeys
    {
        // ---------- 命名规则 ----------

        /// <summary>key 首段（固定）。</summary>
        public const string KeyPrefix = "d19";

        /// <summary>命名规则：d19.<domain>.<name>，每段 [a-z0-9_]+。</summary>
        public static bool IsValidKey(string key)
        {
            if (string.IsNullOrEmpty(key) || !key.StartsWith(KeyPrefix + ".", StringComparison.Ordinal))
            {
                return false;
            }
            foreach (string part in key.Split('.'))
            {
                if (part.Length == 0)
                {
                    return false;
                }
                foreach (char c in part)
                {
                    if (!((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_'))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>缺 key 显式哨兵：调用方必须把此值当作"缺失"，不得当作成功文案。</summary>
        public const string MissingKey = "d19.key.missing";

        // ---------- 域段（用于命名规则与分组） ----------

        public const string DomainLifecycle = "lifecycle";
        public const string DomainService = "service";
        public const string DomainFailure = "failure";
        public const string DomainCompensation = "compensation";
        public const string DomainMigration = "migration";
        public const string DomainReadModel = "readmodel";
        public const string DomainPort = "port";

        // ---------- 生命周期（命令种类，D19PortCommandKind 一一对应） ----------

        public const string LifecycleEnterPort = "d19.lifecycle.enter_port";
        public const string LifecycleDock = "d19.lifecycle.dock";
        public const string LifecycleAssignBerth = "d19.lifecycle.assign_berth";
        public const string LifecycleStartService = "d19.lifecycle.start_service";
        public const string LifecycleCompleteService = "d19.lifecycle.complete_service";
        public const string LifecycleBoard = "d19.lifecycle.board";
        public const string LifecycleDepart = "d19.lifecycle.depart";
        public const string LifecycleReturn = "d19.lifecycle.return";
        public const string LifecycleCompensate = "d19.lifecycle.compensate";

        // ---------- 生命周期（迁移原因，D19PortTransitionReason 一一对应） ----------

        public const string LifecycleEnterAccepted = "d19.lifecycle.enter_accepted";
        public const string LifecycleDockAssigned = "d19.lifecycle.dock_assigned";
        public const string LifecycleBerthAssigned = "d19.lifecycle.berth_assigned";
        public const string LifecycleServiceStarted = "d19.lifecycle.service_started";
        public const string LifecycleServiceCompleted = "d19.lifecycle.service_completed";
        public const string LifecycleBoarded = "d19.lifecycle.boarded";
        public const string LifecycleDeparted = "d19.lifecycle.departed";
        public const string LifecycleReturned = "d19.lifecycle.returned";
        public const string LifecycleRevisitAccepted = "d19.lifecycle.revisit_accepted";

        // ---------- 港口服务（market/warehouse/tavern/shipyard/supply） ----------

        public const string ServiceMarket = "d19.service.market";
        public const string ServiceWarehouse = "d19.service.warehouse";
        public const string ServiceTavern = "d19.service.tavern";
        public const string ServiceShipyard = "d19.service.shipyard";
        public const string ServiceSupply = "d19.service.supply";

        // ---------- 失败（D19PortFailureCode 非 None 值一一对应；L17 目录消费） ----------

        public const string FailureInvalidTransition = "d19.failure.invalid_transition";
        public const string FailureUnknownPort = "d19.failure.unknown_port";
        public const string FailureUnknownBerth = "d19.failure.unknown_berth";
        public const string FailureBerthOccupied = "d19.failure.berth_occupied";
        public const string FailureNotInPort = "d19.failure.not_in_port";
        public const string FailureNotMoored = "d19.failure.not_moored";
        public const string FailureNotBerthed = "d19.failure.not_berthed";
        public const string FailureAlreadyBoarded = "d19.failure.already_boarded";
        public const string FailureNotReadyToDepart = "d19.failure.not_ready_to_depart";
        public const string FailureMissingCommandId = "d19.failure.missing_command_id";
        public const string FailureDuplicateCommand = "d19.failure.duplicate_command";
        public const string FailureServiceUnavailable = "d19.failure.service_unavailable";
        public const string FailureInvariantViolation = "d19.failure.invariant_violation";

        // ---------- 补偿（补偿/回退反馈） ----------

        public const string CompensationRolledBack = "d19.compensation.rolled_back";
        public const string CompensationPartial = "d19.compensation.partial";
        public const string CompensationFailed = "d19.compensation.failed";

        // ---------- 迁移提示（旧存档/保存兼容反馈） ----------

        public const string MigrationSaveUnsupported = "d19.migration.save_unsupported";
        public const string MigrationSaveRepaired = "d19.migration.save_repaired";
        public const string MigrationSaveConflict = "d19.migration.save_conflict";

        // ---------- 读模型反馈（只读消费，D19-L19 读模型输出） ----------

        public const string ReadModelPortOpen = "d19.readmodel.port_open";
        public const string ReadModelPortClosed = "d19.readmodel.port_closed";
        public const string ReadModelBerthAvailable = "d19.readmodel.berth_available";
        public const string ReadModelBerthTaken = "d19.readmodel.berth_taken";
        public const string ReadModelServiceOpen = "d19.readmodel.service_open";
        public const string ReadModelServiceClosed = "d19.readmodel.service_closed";
        public const string ReadModelCommandAccepted = "d19.readmodel.command_accepted";
        public const string ReadModelCommandRejected = "d19.readmodel.command_rejected";

        // ---------- 港口事件摘要（d19.port.*，与 D19-L16 冻结的
        // D19PortEventSchema.LocalizationKeyPattern ^d19\.port\.[a-z0-9_.]+$
        // 兼容；D19PortEventType 十类一一对应，供事件 summary 引用） ----------

        public const string PortEventArrival = "d19.port.event.arrival";
        public const string PortEventBerthReserved = "d19.port.event.berth_reserved";
        public const string PortEventBerthReleased = "d19.port.event.berth_released";
        public const string PortEventServiceResult = "d19.port.event.service_result";
        public const string PortEventBoarded = "d19.port.event.boarded";
        public const string PortEventDisembarked = "d19.port.event.disembarked";
        public const string PortEventDeparture = "d19.port.event.departure";
        public const string PortEventReturn = "d19.port.event.return";
        public const string PortEventCompensation = "d19.port.event.compensation";
        public const string PortEventRejection = "d19.port.event.rejection";

        // ---------- 冻结 key 全集（重复检测与缺 key 报告的输入） ----------

        /// <summary>本清单声明的全部 key（含 MissingKey）。</summary>
        public static readonly IReadOnlyList<string> AllKeys = new List<string>
        {
            MissingKey,
            LifecycleEnterPort,
            LifecycleDock,
            LifecycleAssignBerth,
            LifecycleStartService,
            LifecycleCompleteService,
            LifecycleBoard,
            LifecycleDepart,
            LifecycleReturn,
            LifecycleCompensate,
            LifecycleEnterAccepted,
            LifecycleDockAssigned,
            LifecycleBerthAssigned,
            LifecycleServiceStarted,
            LifecycleServiceCompleted,
            LifecycleBoarded,
            LifecycleDeparted,
            LifecycleReturned,
            LifecycleRevisitAccepted,
            ServiceMarket,
            ServiceWarehouse,
            ServiceTavern,
            ServiceShipyard,
            ServiceSupply,
            FailureInvalidTransition,
            FailureUnknownPort,
            FailureUnknownBerth,
            FailureBerthOccupied,
            FailureNotInPort,
            FailureNotMoored,
            FailureNotBerthed,
            FailureAlreadyBoarded,
            FailureNotReadyToDepart,
            FailureMissingCommandId,
            FailureDuplicateCommand,
            FailureServiceUnavailable,
            FailureInvariantViolation,
            CompensationRolledBack,
            CompensationPartial,
            CompensationFailed,
            MigrationSaveUnsupported,
            MigrationSaveRepaired,
            MigrationSaveConflict,
            ReadModelPortOpen,
            ReadModelPortClosed,
            ReadModelBerthAvailable,
            ReadModelBerthTaken,
            ReadModelServiceOpen,
            ReadModelServiceClosed,
            ReadModelCommandAccepted,
            ReadModelCommandRejected,
            PortEventArrival,
            PortEventBerthReserved,
            PortEventBerthReleased,
            PortEventServiceResult,
            PortEventBoarded,
            PortEventDisembarked,
            PortEventDeparture,
            PortEventReturn,
            PortEventCompensation,
            PortEventRejection
        };

        // ---------- 失败码 → key 映射（D19-L17 失败码目录的唯一 key 来源） ----------

        private static readonly Dictionary<D19PortFailureCode, string> FailureKeys = BuildFailureKeys();

        private static Dictionary<D19PortFailureCode, string> BuildFailureKeys()
        {
            Dictionary<D19PortFailureCode, string> map = new Dictionary<D19PortFailureCode, string>();
            // 每个非 None 失败码必须在此注册其唯一 key；缺注册即为"缺 key"，
            // 由 ValidateRegistry 通过反射报告，绝不静默回退。
            map[D19PortFailureCode.InvalidTransition] = FailureInvalidTransition;
            map[D19PortFailureCode.UnknownPort] = FailureUnknownPort;
            map[D19PortFailureCode.UnknownBerth] = FailureUnknownBerth;
            map[D19PortFailureCode.BerthOccupied] = FailureBerthOccupied;
            map[D19PortFailureCode.NotInPort] = FailureNotInPort;
            map[D19PortFailureCode.NotMoored] = FailureNotMoored;
            map[D19PortFailureCode.NotBerthed] = FailureNotBerthed;
            map[D19PortFailureCode.AlreadyBoarded] = FailureAlreadyBoarded;
            map[D19PortFailureCode.NotReadyToDepart] = FailureNotReadyToDepart;
            map[D19PortFailureCode.MissingCommandId] = FailureMissingCommandId;
            map[D19PortFailureCode.DuplicateCommand] = FailureDuplicateCommand;
            map[D19PortFailureCode.ServiceUnavailable] = FailureServiceUnavailable;
            map[D19PortFailureCode.InvariantViolation] = FailureInvariantViolation;
            return map;
        }

        /// <summary>按失败码取唯一 key。返回 false 表示该码未注册 key（缺 key），
        /// 调用方必须显式处理，不得把失败当成成功。</summary>
        public static bool TryGetFailureKey(D19PortFailureCode code, out string key)
        {
            return FailureKeys.TryGetValue(code, out key);
        }

        /// <summary>取失败码 key 的显式安全版：未注册时返回 MissingKey 哨兵
        /// （调用方必须与 MissingKey 比较并显式处理，绝不静默成功）。</summary>
        public static string FailureKeyOrMissing(D19PortFailureCode code)
        {
            string key;
            return TryGetFailureKey(code, out key) ? key : MissingKey;
        }

        // ---------- 迁移原因 → key 映射 ----------

        private static readonly Dictionary<D19PortTransitionReason, string> TransitionReasonKeys =
            new Dictionary<D19PortTransitionReason, string>
            {
                { D19PortTransitionReason.EnterPortAccepted, LifecycleEnterAccepted },
                { D19PortTransitionReason.DockAssigned, LifecycleDockAssigned },
                { D19PortTransitionReason.BerthAssigned, LifecycleBerthAssigned },
                { D19PortTransitionReason.ServiceStarted, LifecycleServiceStarted },
                { D19PortTransitionReason.ServiceCompleted, LifecycleServiceCompleted },
                { D19PortTransitionReason.Boarded, LifecycleBoarded },
                { D19PortTransitionReason.Departed, LifecycleDeparted },
                { D19PortTransitionReason.Returned, LifecycleReturned },
                { D19PortTransitionReason.RevisitAccepted, LifecycleRevisitAccepted }
            };

        /// <summary>按迁移原因取 key；未注册（含 None）返回 false。</summary>
        public static bool TryGetTransitionReasonKey(D19PortTransitionReason reason, out string key)
        {
            return TransitionReasonKeys.TryGetValue(reason, out key);
        }

        // ---------- 港口服务 ID → key 映射 ----------

        /// <summary>服务稳定 ID（D19-L01 冻结：市场/仓库/酒馆/船坞/补给）。</summary>
        public const string ServiceIdMarket = "market";
        public const string ServiceIdWarehouse = "warehouse";
        public const string ServiceIdTavern = "tavern";
        public const string ServiceIdShipyard = "shipyard";
        public const string ServiceIdSupply = "supply";

        /// <summary>按服务稳定 ID 取 key；未知 ID 返回 false（缺 key 显式处理）。</summary>
        public static bool TryGetServiceKey(string serviceId, out string key)
        {
            switch (serviceId)
            {
                case ServiceIdMarket: key = ServiceMarket; return true;
                case ServiceIdWarehouse: key = ServiceWarehouse; return true;
                case ServiceIdTavern: key = ServiceTavern; return true;
                case ServiceIdShipyard: key = ServiceShipyard; return true;
                case ServiceIdSupply: key = ServiceSupply; return true;
                default: key = null; return false;
            }
        }

        // ---------- 港口事件类型 → 摘要 key 映射（D19-L16 事件 summary 引用） ----------

        private static readonly Dictionary<D19PortEventType, string> EventSummaryKeys =
            new Dictionary<D19PortEventType, string>
            {
                { D19PortEventType.Arrival, PortEventArrival },
                { D19PortEventType.BerthReserved, PortEventBerthReserved },
                { D19PortEventType.BerthReleased, PortEventBerthReleased },
                { D19PortEventType.ServiceResult, PortEventServiceResult },
                { D19PortEventType.Boarded, PortEventBoarded },
                { D19PortEventType.Disembarked, PortEventDisembarked },
                { D19PortEventType.Departure, PortEventDeparture },
                { D19PortEventType.Return, PortEventReturn },
                { D19PortEventType.Compensation, PortEventCompensation },
                { D19PortEventType.Rejection, PortEventRejection }
            };

        /// <summary>按港口事件类型取摘要 key；未注册（含 Unknown）返回 false
        /// （未知事件不得得到任何"成功"文案键）。</summary>
        public static bool TryGetEventSummaryKey(D19PortEventType type, out string key)
        {
            return EventSummaryKeys.TryGetValue(type, out key);
        }

        // ---------- 注册表检查（重复检测 + 缺 key 报告接口） ----------

        /// <summary>注册表检查结果：重复/非法 key、无 key 的失败码、总 key 数。</summary>
        public struct D19KeyValidationResult
        {
            /// <summary>声明的 key 总数。</summary>
            public int TotalKeyCount;

            /// <summary>重复出现的 key（AllKeys 内重复或与常量重复）。</summary>
            public List<string> DuplicateKeys;

            /// <summary>不符合命名规则的 key。</summary>
            public List<string> InvalidKeys;

            /// <summary>没有注册 key 的失败码（含 L17 未来扩展值）。</summary>
            public List<D19PortFailureCode> FailureCodesWithoutKeys;

            /// <summary>是否存在任何问题（重复/非法/缺 key）。</summary>
            public bool HasErrors
            {
                get
                {
                    return (DuplicateKeys != null && DuplicateKeys.Count > 0)
                        || (InvalidKeys != null && InvalidKeys.Count > 0)
                        || (FailureCodesWithoutKeys != null && FailureCodesWithoutKeys.Count > 0);
                }
            }
        }

        /// <summary>
        /// 运行完整注册表检查：命名规则、重复检测、失败码缺 key 报告。
        /// 返回结果对象；HasErrors=true 时调用方必须显式处理，不得静默回退为成功。
        /// D19PortFailureCode 每新增一个值（D19-L17 扩展），此检查自动报告缺 key。
        /// </summary>
        public static D19KeyValidationResult ValidateRegistry()
        {
            D19KeyValidationResult result = new D19KeyValidationResult();
            result.DuplicateKeys = new List<string>();
            result.InvalidKeys = new List<string>();
            result.FailureCodesWithoutKeys = new List<D19PortFailureCode>();

            HashSet<string> seen = new HashSet<string>();
            foreach (string key in AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    result.InvalidKeys.Add(key ?? "<null>");
                    continue;
                }
                if (!IsValidKey(key))
                {
                    result.InvalidKeys.Add(key);
                }
                if (!seen.Add(key))
                {
                    result.DuplicateKeys.Add(key);
                }
            }

            foreach (D19PortFailureCode code in Enum.GetValues(typeof(D19PortFailureCode)))
            {
                if (code == D19PortFailureCode.None)
                {
                    continue; // None 是成功哨兵，不是失败，不需要失败 key
                }
                string key;
                if (!FailureKeys.TryGetValue(code, out key))
                {
                    result.FailureCodesWithoutKeys.Add(code);
                }
            }

            result.TotalKeyCount = seen.Count;
            return result;
        }
    }
}
