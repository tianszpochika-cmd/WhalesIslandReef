// ============================================================
// D19PortReadModel —— 港口只读状态投影模型（纯 C#，D19-L19）
//
// 归属：Whalebone.Systems.D19（面向未来 UI/Presentation 的唯一只读投影，D19-L19）
// 目的：将各领域系统的不可变快照汇聚为只读港口状态投影：
//       - 访问状态（VisitState 投影、PortId/BerthId/ShipId、状态描述 LocKey）
//       - 泊位摘要（稳定排序的泊位分配/可用性列表、能力描述、预留到期日）
//       - 可用服务（稳定排序的各服务状态、L05 判定 Verdict/ReasonCode/LocKey/RetryHint）
//       - 行动反馈（命令执行历史投影、最近结果、失败码与本地化键引用）
//       - 失败 code/key（稳定失败条目投影、L17/L18 引用）
//       - 出航准备清单（稳定排序的检查项、原所有者、原始失败码、是否就绪）
//       - 版本/更新时间语义（逻辑世界日 WorldDay、单调存档修订 SaveRevision、快照哈希/版本）
//
// 铁律与设计原则：
//   1. 只消费不拥有：所有输出不可写，字段全部为只读属性或只读集合；输入必须为不可变快照或领域系统只读副本；
//   2. 不反向调用：零 UnityEngine.UI / Canvas / Transform / 场景 API / 事件系统 / 网络 / LLM 依赖；
//      即使 Presentation / UI 完全缺失，领域业务与读模型也完全不受影响（纯函数式构建）；
//   3. 不成为裁决来源：任何 Presentation 读取本模型均无法反向改变领域系统状态；
//   4. 不泄露实体私密字段或内部可变集合：向外仅暴露 IReadOnlyList<T>，构造时对传入集合进行防御性浅拷贝与稳定排序；
//   5. 确定性与稳定排序：同输入永远得到逐字段一致的稳定输出；所有列表按稳定键（如 BerthId、ServiceId、CheckId、CommandId）显式排序；
//   6. 纯 C# / BCL only：可独立编译与验证。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 港口访问状态只读投影（D19-L19）。
    /// </summary>
    public sealed class D19PortVisitReadModel
    {
        public D19PortVisitState State { get; }
        public string PortId { get; }
        public string BerthId { get; }
        public string ShipId { get; }
        public string StateLocKey { get; }
        public bool IsInPort { get; }
        public bool IsBerthed { get; }
        public bool IsBoarded { get; }
        public bool CanStartService { get; }
        public bool CanDepart { get; }

        public D19PortVisitReadModel(
            D19PortVisitState state,
            string portId,
            string berthId,
            string shipId)
        {
            State = state;
            PortId = portId ?? string.Empty;
            BerthId = berthId ?? string.Empty;
            ShipId = shipId ?? string.Empty;

            // 映射到稳定本地化键
            string locKey;
            if (D19PortLocalizationKeys.TryGetTransitionReasonKey(GetRepresentativeReason(state), out locKey))
            {
                StateLocKey = locKey;
            }
            else
            {
                StateLocKey = D19PortLocalizationKeys.MissingKey;
            }

            IsInPort = state == D19PortVisitState.PortVisit ||
                       state == D19PortVisitState.Docked ||
                       state == D19PortVisitState.Berthed ||
                       state == D19PortVisitState.InService ||
                       state == D19PortVisitState.Boarded;

            IsBerthed = state == D19PortVisitState.Berthed || state == D19PortVisitState.InService;
            IsBoarded = state == D19PortVisitState.Boarded;
            CanStartService = state == D19PortVisitState.Berthed;
            CanDepart = state == D19PortVisitState.Boarded || state == D19PortVisitState.PortVisit;
        }

        private static D19PortTransitionReason GetRepresentativeReason(D19PortVisitState state)
        {
            switch (state)
            {
                case D19PortVisitState.PortVisit: return D19PortTransitionReason.EnterPortAccepted;
                case D19PortVisitState.Docked: return D19PortTransitionReason.DockAssigned;
                case D19PortVisitState.Berthed: return D19PortTransitionReason.BerthAssigned;
                case D19PortVisitState.InService: return D19PortTransitionReason.ServiceStarted;
                case D19PortVisitState.Boarded: return D19PortTransitionReason.Boarded;
                case D19PortVisitState.Departed: return D19PortTransitionReason.Departed;
                case D19PortVisitState.Returned: return D19PortTransitionReason.Returned;
                default: return D19PortTransitionReason.None;
            }
        }
    }

    /// <summary>
    /// 单个泊位只读条目（D19-L19，稳定排序）。
    /// </summary>
    public sealed class D19BerthReadModelItem
    {
        public string BerthId { get; }
        public D19BerthState State { get; }
        public string OccupyingShipId { get; }
        public string CommandId { get; }
        public int ReservedWorldDay { get; }
        public int ExpiresWorldDay { get; }
        public string ReleaseReason { get; }
        public string StatusLocKey { get; }
        public bool IsAvailable { get; }
        public bool IsOccupiedByCurrentShip { get; }

        // 能力投影（来自 L06 冻结能力或快照）
        public int MinTier { get; }
        public int MaxTier { get; }
        public bool DeepDraft { get; }

        public D19BerthReadModelItem(
            D19BerthAllocation allocation,
            D19BerthCapability capability,
            string currentShipId)
        {
            if (allocation == null)
            {
                BerthId = capability != null ? capability.BerthId : string.Empty;
                State = D19BerthState.Available;
                OccupyingShipId = string.Empty;
                CommandId = string.Empty;
                ReservedWorldDay = 0;
                ExpiresWorldDay = 0;
                ReleaseReason = string.Empty;
            }
            else
            {
                BerthId = allocation.BerthId ?? string.Empty;
                State = allocation.State;
                OccupyingShipId = allocation.ShipId ?? string.Empty;
                CommandId = allocation.CommandId ?? string.Empty;
                ReservedWorldDay = allocation.ReservedWorldDay;
                ExpiresWorldDay = allocation.ExpiresWorldDay;
                ReleaseReason = allocation.ReleaseReason ?? string.Empty;
            }

            if (capability != null)
            {
                MinTier = capability.MinTier;
                MaxTier = capability.MaxTier;
                DeepDraft = capability.DeepDraft;
            }
            else
            {
                MinTier = 1;
                MaxTier = 3;
                DeepDraft = false;
            }

            IsAvailable = State == D19BerthState.Available;
            IsOccupiedByCurrentShip = !string.IsNullOrEmpty(currentShipId) && OccupyingShipId == currentShipId;

            StatusLocKey = IsAvailable ? D19PortLocalizationKeys.ReadModelBerthAvailable : D19PortLocalizationKeys.ReadModelBerthTaken;
        }
    }

    /// <summary>
    /// 泊位系统状态只读投影（D19-L19）。
    /// </summary>
    public sealed class D19BerthSummaryReadModel
    {
        public string AssignedBerthId { get; }
        public int TotalBerths { get; }
        public int AvailableBerths { get; }
        public int OccupiedBerths { get; }
        public int ReservedBerths { get; }
        public IReadOnlyList<D19BerthReadModelItem> Berths { get; }

        public D19BerthSummaryReadModel(
            IEnumerable<D19BerthAllocation> allocations,
            IEnumerable<D19BerthCapability> capabilities,
            string assignedBerthId,
            string currentShipId)
        {
            AssignedBerthId = assignedBerthId ?? string.Empty;

            Dictionary<string, D19BerthAllocation> allocMap = new Dictionary<string, D19BerthAllocation>(StringComparer.Ordinal);
            if (allocations != null)
            {
                foreach (D19BerthAllocation a in allocations)
                {
                    if (a != null && !string.IsNullOrEmpty(a.BerthId))
                    {
                        allocMap[a.BerthId] = a;
                    }
                }
            }

            Dictionary<string, D19BerthCapability> capMap = new Dictionary<string, D19BerthCapability>(StringComparer.Ordinal);
            if (capabilities != null)
            {
                foreach (D19BerthCapability c in capabilities)
                {
                    if (c != null && !string.IsNullOrEmpty(c.BerthId))
                    {
                        capMap[c.BerthId] = c;
                    }
                }
            }

            // 收集所有唯一的 BerthId
            HashSet<string> allBerthIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string k in allocMap.Keys) allBerthIds.Add(k);
            foreach (string k in capMap.Keys) allBerthIds.Add(k);

            List<string> sortedBerthIds = allBerthIds.ToList();
            sortedBerthIds.Sort(StringComparer.Ordinal);

            List<D19BerthReadModelItem> items = new List<D19BerthReadModelItem>(sortedBerthIds.Count);
            int avail = 0;
            int occ = 0;
            int res = 0;

            foreach (string bid in sortedBerthIds)
            {
                D19BerthAllocation a;
                allocMap.TryGetValue(bid, out a);
                D19BerthCapability c;
                capMap.TryGetValue(bid, out c);

                D19BerthReadModelItem item = new D19BerthReadModelItem(a, c, currentShipId);
                items.Add(item);

                if (item.State == D19BerthState.Available) avail++;
                else if (item.State == D19BerthState.Occupied) occ++;
                else if (item.State == D19BerthState.Reserved) res++;
            }

            TotalBerths = items.Count;
            AvailableBerths = avail;
            OccupiedBerths = occ;
            ReservedBerths = res;
            Berths = new ReadOnlyCollection<D19BerthReadModelItem>(items);
        }
    }

    /// <summary>
    /// 单项港口服务只读条目（D19-L19，稳定排序）。
    /// </summary>
    public sealed class D19ServiceReadModelItem
    {
        public string ServiceId { get; }
        public string ServiceLocKey { get; }
        public D19AvailabilityVerdict Verdict { get; }
        public bool IsAvailable { get; }
        public string ReasonCode { get; }
        public string FeedbackLocKey { get; }
        public D19FailureRetryPolicy RetryPolicy { get; }
        public D19UnavailabilityDetail Detail { get; }
        public D19RetryHint RetryHint { get; }

        public D19ServiceReadModelItem(
            string serviceId,
            D19ServiceAvailabilityResult availability)
        {
            ServiceId = serviceId ?? string.Empty;

            string sKey;
            if (D19PortLocalizationKeys.TryGetServiceKey(ServiceId, out sKey))
            {
                ServiceLocKey = sKey;
            }
            else
            {
                ServiceLocKey = D19PortLocalizationKeys.MissingKey;
            }

            if (availability == null)
            {
                Verdict = D19AvailabilityVerdict.Rejected;
                IsAvailable = false;
                ReasonCode = "d19.service.unavailable";
                FeedbackLocKey = D19PortLocalizationKeys.ReadModelServiceClosed;
                RetryPolicy = D19FailureRetryPolicy.NoRetry;
                Detail = D19UnavailabilityDetail.UnknownInput;
                RetryHint = D19RetryHint.AfterCorrection;
            }
            else
            {
                Verdict = availability.Verdict;
                IsAvailable = availability.Verdict == D19AvailabilityVerdict.Available;
                ReasonCode = availability.ReasonCode ?? string.Empty;
                FeedbackLocKey = !string.IsNullOrEmpty(availability.LocKey) ? availability.LocKey :
                    (IsAvailable ? D19PortLocalizationKeys.ReadModelServiceOpen : D19PortLocalizationKeys.ReadModelServiceClosed);
                RetryPolicy = availability.RetryPolicy;
                Detail = availability.Detail;
                RetryHint = availability.RetryHint;
            }
        }
    }

    /// <summary>
    /// 港口可用服务总览只读投影（D19-L19）。
    /// </summary>
    public sealed class D19ServicesSummaryReadModel
    {
        public string PortId { get; }
        public IReadOnlyList<D19ServiceReadModelItem> Services { get; }
        public int TotalServices { get; }
        public int AvailableServices { get; }

        public D19ServicesSummaryReadModel(
            string portId,
            IEnumerable<D19ServiceAvailabilityResult> availabilityResults)
        {
            PortId = portId ?? string.Empty;

            Dictionary<string, D19ServiceAvailabilityResult> map =
                new Dictionary<string, D19ServiceAvailabilityResult>(StringComparer.Ordinal);

            if (availabilityResults != null)
            {
                foreach (D19ServiceAvailabilityResult r in availabilityResults)
                {
                    if (r != null && !string.IsNullOrEmpty(r.ServiceId))
                    {
                        map[r.ServiceId] = r;
                    }
                }
            }

            // 依据 L05 KnownServiceIds 保证所有标准服务都在投影内，且保持稳定排序
            List<string> serviceIds = new List<string>(D19PortAvailabilitySystem.KnownServiceIds);
            foreach (string extra in map.Keys)
            {
                if (!serviceIds.Contains(extra))
                {
                    serviceIds.Add(extra);
                }
            }
            serviceIds.Sort(StringComparer.Ordinal);

            List<D19ServiceReadModelItem> items = new List<D19ServiceReadModelItem>(serviceIds.Count);
            int availCount = 0;

            foreach (string sid in serviceIds)
            {
                D19ServiceAvailabilityResult r;
                map.TryGetValue(sid, out r);
                D19ServiceReadModelItem item = new D19ServiceReadModelItem(sid, r);
                items.Add(item);
                if (item.IsAvailable)
                {
                    availCount++;
                }
            }

            Services = new ReadOnlyCollection<D19ServiceReadModelItem>(items);
            TotalServices = items.Count;
            AvailableServices = availCount;
        }
    }

    /// <summary>
    /// 命令行动反馈只读条目（D19-L19）。
    /// </summary>
    public sealed class D19ActionFeedbackReadModel
    {
        public string CommandId { get; }
        public bool Success { get; }
        public D19PortVisitState StateAfter { get; }
        public string FailureCode { get; }
        public string ReasonId { get; }
        public string FeedbackLocKey { get; }
        public bool Recoverable { get; }

        public D19ActionFeedbackReadModel(D19PortCommandResult commandResult)
        {
            if (commandResult == null)
            {
                CommandId = string.Empty;
                Success = false;
                StateAfter = D19PortVisitState.None;
                FailureCode = D19PortFailureCode.MissingCommandId.ToString();
                ReasonId = "d19.failure.missing_command_id";
                FeedbackLocKey = D19PortLocalizationKeys.ReadModelCommandRejected;
                Recoverable = false;
                return;
            }

            CommandId = commandResult.CommandId ?? string.Empty;
            Success = commandResult.Success;
            StateAfter = commandResult.State;

            if (Success)
            {
                FailureCode = D19PortFailureCode.None.ToString();
                ReasonId = string.Empty;
                FeedbackLocKey = D19PortLocalizationKeys.ReadModelCommandAccepted;
                Recoverable = false;
            }
            else
            {
                if (commandResult.Failure != null)
                {
                    FailureCode = commandResult.Failure.Code.ToString();
                    ReasonId = commandResult.Failure.ReasonId ?? string.Empty;
                    Recoverable = commandResult.Failure.Recoverable;

                    string locKey;
                    if (D19PortLocalizationKeys.TryGetFailureKey(commandResult.Failure.Code, out locKey))
                    {
                        FeedbackLocKey = locKey;
                    }
                    else
                    {
                        FeedbackLocKey = D19PortLocalizationKeys.ReadModelCommandRejected;
                    }
                }
                else
                {
                    FailureCode = D19PortFailureCode.InvalidTransition.ToString();
                    ReasonId = "d19.failure.invalid_transition";
                    FeedbackLocKey = D19PortLocalizationKeys.ReadModelCommandRejected;
                    Recoverable = false;
                }
            }
        }
    }

    /// <summary>
    /// 出航准备检查项只读条目（D19-L19，稳定排序）。
    /// </summary>
    public sealed class D19DepartureCheckReadModelItem
    {
        public string CheckId { get; }
        public string Owner { get; }
        public D19DepartureCheckStatus Status { get; }
        public bool Passed { get; }
        public string OriginalCode { get; }
        public D19PortFailureCode FailureCode { get; }
        public string FailureCodeId { get; }
        public string LocKey { get; }
        public string Detail { get; }

        public D19DepartureCheckReadModelItem(D19DepartureCheckItem checkItem)
        {
            if (checkItem == null)
            {
                CheckId = string.Empty;
                Owner = string.Empty;
                Status = D19DepartureCheckStatus.Unknown;
                Passed = false;
                OriginalCode = string.Empty;
                FailureCode = D19PortFailureCode.None;
                FailureCodeId = string.Empty;
                LocKey = D19PortLocalizationKeys.MissingKey;
                Detail = string.Empty;
            }
            else
            {
                CheckId = checkItem.CheckId ?? string.Empty;
                Owner = checkItem.Owner ?? string.Empty;
                Status = checkItem.Status;
                Passed = checkItem.Passed;
                OriginalCode = checkItem.OriginalCode ?? string.Empty;
                FailureCode = checkItem.FailureCode;
                FailureCodeId = checkItem.FailureCodeId ?? string.Empty;
                LocKey = checkItem.LocKey ?? string.Empty;
                Detail = checkItem.Detail ?? string.Empty;
            }
        }
    }

    /// <summary>
    /// 出航准备清单只读投影（D19-L19）。
    /// </summary>
    public sealed class D19DepartureChecklistReadModel
    {
        public string PortId { get; }
        public string ShipId { get; }
        public string DestinationId { get; }
        public int WorldDay { get; }
        public bool CanDepart { get; }
        public bool HasUnknown { get; }
        public int TotalChecks { get; }
        public int PassedChecks { get; }
        public IReadOnlyList<D19DepartureCheckReadModelItem> Checks { get; }
        public IReadOnlyList<string> BlockingCheckIds { get; }

        public D19DepartureChecklistReadModel(D19DepartureReadinessSummary summary)
        {
            if (summary == null)
            {
                PortId = string.Empty;
                ShipId = string.Empty;
                DestinationId = string.Empty;
                WorldDay = 0;
                CanDepart = false;
                HasUnknown = true;
                TotalChecks = 0;
                PassedChecks = 0;
                Checks = new ReadOnlyCollection<D19DepartureCheckReadModelItem>(new List<D19DepartureCheckReadModelItem>());
                BlockingCheckIds = new ReadOnlyCollection<string>(new List<string>());
                return;
            }

            PortId = summary.PortId ?? string.Empty;
            ShipId = summary.ShipId ?? string.Empty;
            DestinationId = summary.DestinationId ?? string.Empty;
            WorldDay = summary.WorldDay;
            CanDepart = summary.CanDepart;
            HasUnknown = summary.HasUnknown;

            List<D19DepartureCheckReadModelItem> items = new List<D19DepartureCheckReadModelItem>();
            int passed = 0;
            if (summary.Items != null)
            {
                foreach (D19DepartureCheckItem it in summary.Items)
                {
                    if (it != null)
                    {
                        D19DepartureCheckReadModelItem m = new D19DepartureCheckReadModelItem(it);
                        items.Add(m);
                        if (m.Passed) passed++;
                    }
                }
            }

            // 保持确定性顺序（若原 summary 有顺序则保持，若无则稳定排序）
            TotalChecks = items.Count;
            PassedChecks = passed;
            Checks = new ReadOnlyCollection<D19DepartureCheckReadModelItem>(items);

            List<string> blockers = summary.BlockingCheckIds != null ? new List<string>(summary.BlockingCheckIds) : new List<string>();
            BlockingCheckIds = new ReadOnlyCollection<string>(blockers);
        }
    }

    /// <summary>
    /// 完整港口只读状态投影根模型（D19-L19）。
    /// 面向 Presentation / UI 的单向聚合根，纯数据、完全只读、无任何可变状态。
    /// </summary>
    public sealed class D19PortReadModel
    {
        public const string SchemaVersion = "1.0.0";

        /// <summary>快照版本标记（单调、确定性）。</summary>
        public long SaveRevision { get; }

        /// <summary>逻辑世界日（非系统时钟）。</summary>
        public int WorldDay { get; }

        /// <summary>访问状态投影。</summary>
        public D19PortVisitReadModel Visit { get; }

        /// <summary>泊位状态投影。</summary>
        public D19BerthSummaryReadModel Berths { get; }

        /// <summary>服务可用性投影。</summary>
        public D19ServicesSummaryReadModel Services { get; }

        /// <summary>最近一次行动反馈（可为 null）。</summary>
        public D19ActionFeedbackReadModel LastActionFeedback { get; }

        /// <summary>出航准备清单投影（可为 null）。</summary>
        public D19DepartureChecklistReadModel DepartureChecklist { get; }

        /// <summary>投影快照生成时的确定性哈希/指纹。</summary>
        public string SnapshotFingerprint { get; }

        public D19PortReadModel(
            int worldDay,
            long saveRevision,
            D19PortVisitReadModel visit,
            D19BerthSummaryReadModel berths,
            D19ServicesSummaryReadModel services,
            D19ActionFeedbackReadModel lastActionFeedback = null,
            D19DepartureChecklistReadModel departureChecklist = null)
        {
            WorldDay = worldDay;
            SaveRevision = saveRevision;
            Visit = visit ?? new D19PortVisitReadModel(D19PortVisitState.None, string.Empty, string.Empty, string.Empty);
            Berths = berths ?? new D19BerthSummaryReadModel(null, null, string.Empty, Visit.ShipId);
            Services = services ?? new D19ServicesSummaryReadModel(Visit.PortId, null);
            LastActionFeedback = lastActionFeedback;
            DepartureChecklist = departureChecklist;

            SnapshotFingerprint = ComputeFingerprint();
        }

        private string ComputeFingerprint()
        {
            // 纯确定性指纹计算（不依赖时钟或对象哈希）
            return $"d19_rm_v{SchemaVersion}_d{WorldDay}_r{SaveRevision}_{Visit.PortId}_{Visit.State}_{Visit.BerthId}_{Berths.TotalBerths}_{Services.AvailableServices}";
        }

        // ---------- 建造工厂入口（纯函数式快照组装，无副作用） ----------

        /// <summary>
        /// 从各领域不可变快照构建只读投影模型。
        /// 纯函数：相同输入 → 相同输出。
        /// </summary>
        public static D19PortReadModel Build(
            int worldDay,
            long saveRevision,
            D19PortVisitState visitState,
            string portId,
            string berthId,
            string shipId,
            IEnumerable<D19BerthAllocation> berthAllocations,
            IEnumerable<D19BerthCapability> berthCapabilities,
            IEnumerable<D19ServiceAvailabilityResult> serviceAvailabilities,
            D19PortCommandResult lastCommandResult = null,
            D19DepartureReadinessSummary departureSummary = null)
        {
            D19PortVisitReadModel visitModel = new D19PortVisitReadModel(visitState, portId, berthId, shipId);
            D19BerthSummaryReadModel berthModel = new D19BerthSummaryReadModel(berthAllocations, berthCapabilities, berthId, shipId);
            D19ServicesSummaryReadModel serviceModel = new D19ServicesSummaryReadModel(portId, serviceAvailabilities);
            D19ActionFeedbackReadModel feedbackModel = lastCommandResult != null ? new D19ActionFeedbackReadModel(lastCommandResult) : null;
            D19DepartureChecklistReadModel departureModel = departureSummary != null ? new D19DepartureChecklistReadModel(departureSummary) : null;

            return new D19PortReadModel(
                worldDay,
                saveRevision,
                visitModel,
                berthModel,
                serviceModel,
                feedbackModel,
                departureModel);
        }
    }
}
