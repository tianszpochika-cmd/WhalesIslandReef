// ============================================================
// D22PortReadModelAdapter —— D22-L18 港口只读 UI 适配器（纯 C#）
//
// 只消费 D19PortReadModel 及其公开行动反馈。泊位、服务、登船/出航
// 清单在边界处重新排序并复制；缺失投影只产生禁用/部分状态，不补造
// 港口事实，也不调用 D19 命令路由或任何表现层 API。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Whalebone.Systems.D19;

namespace Whalebone.Systems.D22
{
    /// <summary>D19 港口 owner 的输入边界。</summary>
    [Serializable]
    public sealed class D22PortOwnerSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string SourceSchemaVersionText = D19PortReadModel.SchemaVersion;
        public D19PortReadModel Port;
        public D19ActionFeedbackReadModel LastActionFeedback;
        public string DisabledReasonCode = string.Empty;
        public string DisabledLocalizationKey = string.Empty;
        public List<string> MissingModuleIds = new List<string>();
    }

    /// <summary>港口访问状态的 D22 防御性投影。</summary>
    [Serializable]
    public sealed class D22PortVisitReadModel
    {
        public string State { get; }
        public string PortId { get; }
        public string BerthId { get; }
        public string ShipId { get; }
        public bool IsInPort { get; }
        public bool IsBerthed { get; }
        public bool IsBoarded { get; }
        public bool CanStartService { get; }
        public bool CanDepart { get; }
        public string LocalizationKey { get; }
        public string SourceLocalizationKey { get; }

        internal D22PortVisitReadModel(D19PortVisitReadModel source)
        {
            State = source == null ? string.Empty : source.State.ToString();
            PortId = source == null ? string.Empty : source.PortId ?? string.Empty;
            BerthId = source == null ? string.Empty : source.BerthId ?? string.Empty;
            ShipId = source == null ? string.Empty : source.ShipId ?? string.Empty;
            IsInPort = source != null && source.IsInPort;
            IsBerthed = source != null && source.IsBerthed;
            IsBoarded = source != null && source.IsBoarded;
            CanStartService = source != null && source.CanStartService;
            CanDepart = source != null && source.CanDepart;
            LocalizationKey = D22ReadModelAdapterSupport.Key("port", "visit");
            SourceLocalizationKey = source == null ? string.Empty : source.StateLocKey ?? string.Empty;
        }

        private D22PortVisitReadModel(
            string state,
            string portId,
            string berthId,
            string shipId,
            bool isInPort,
            bool isBerthed,
            bool isBoarded,
            bool canStartService,
            bool canDepart,
            string localizationKey,
            string sourceLocalizationKey)
        {
            State = state ?? string.Empty;
            PortId = portId ?? string.Empty;
            BerthId = berthId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            IsInPort = isInPort;
            IsBerthed = isBerthed;
            IsBoarded = isBoarded;
            CanStartService = canStartService;
            CanDepart = canDepart;
            LocalizationKey = localizationKey ?? string.Empty;
            SourceLocalizationKey = sourceLocalizationKey ?? string.Empty;
        }

        public D22PortVisitReadModel Clone()
        {
            return new D22PortVisitReadModel(
                State, PortId, BerthId, ShipId, IsInPort, IsBerthed, IsBoarded,
                CanStartService, CanDepart, LocalizationKey, SourceLocalizationKey);
        }
    }

    /// <summary>泊位单项只读投影。</summary>
    [Serializable]
    public sealed class D22PortBerthReadModel
    {
        public string BerthId { get; }
        public string State { get; }
        public string OccupyingShipId { get; }
        public string CommandId { get; }
        public int ReservedWorldDay { get; }
        public int ExpiresWorldDay { get; }
        public string ReleaseReason { get; }
        public int MinTier { get; }
        public int MaxTier { get; }
        public bool DeepDraft { get; }
        public bool IsAvailable { get; }
        public bool IsOccupiedByCurrentShip { get; }
        public string DisabledReasonCode { get; }
        public string LocalizationKey { get; }
        public string SourceLocalizationKey { get; }

        internal D22PortBerthReadModel(D19BerthReadModelItem source)
        {
            BerthId = source == null ? string.Empty : source.BerthId ?? string.Empty;
            State = source == null ? string.Empty : source.State.ToString();
            OccupyingShipId = source == null ? string.Empty : source.OccupyingShipId ?? string.Empty;
            CommandId = source == null ? string.Empty : source.CommandId ?? string.Empty;
            ReservedWorldDay = source == null ? 0 : source.ReservedWorldDay;
            ExpiresWorldDay = source == null ? 0 : source.ExpiresWorldDay;
            ReleaseReason = source == null ? string.Empty : source.ReleaseReason ?? string.Empty;
            MinTier = source == null ? 0 : source.MinTier;
            MaxTier = source == null ? 0 : source.MaxTier;
            DeepDraft = source != null && source.DeepDraft;
            IsAvailable = source != null && source.IsAvailable;
            IsOccupiedByCurrentShip = source != null && source.IsOccupiedByCurrentShip;
            DisabledReasonCode = IsAvailable ? string.Empty : "d22.port.berth_unavailable";
            LocalizationKey = D22ReadModelAdapterSupport.Key("port", "berth");
            SourceLocalizationKey = source == null ? string.Empty : source.StatusLocKey ?? string.Empty;
        }

        private D22PortBerthReadModel(
            string berthId,
            string state,
            string occupyingShipId,
            string commandId,
            int reservedWorldDay,
            int expiresWorldDay,
            string releaseReason,
            int minTier,
            int maxTier,
            bool deepDraft,
            bool isAvailable,
            bool isOccupiedByCurrentShip,
            string disabledReasonCode,
            string localizationKey,
            string sourceLocalizationKey)
        {
            BerthId = berthId ?? string.Empty;
            State = state ?? string.Empty;
            OccupyingShipId = occupyingShipId ?? string.Empty;
            CommandId = commandId ?? string.Empty;
            ReservedWorldDay = reservedWorldDay;
            ExpiresWorldDay = expiresWorldDay;
            ReleaseReason = releaseReason ?? string.Empty;
            MinTier = minTier;
            MaxTier = maxTier;
            DeepDraft = deepDraft;
            IsAvailable = isAvailable;
            IsOccupiedByCurrentShip = isOccupiedByCurrentShip;
            DisabledReasonCode = disabledReasonCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            SourceLocalizationKey = sourceLocalizationKey ?? string.Empty;
        }

        public D22PortBerthReadModel Clone()
        {
            return new D22PortBerthReadModel(
                BerthId, State, OccupyingShipId, CommandId, ReservedWorldDay, ExpiresWorldDay,
                ReleaseReason, MinTier, MaxTier, DeepDraft, IsAvailable, IsOccupiedByCurrentShip,
                DisabledReasonCode, LocalizationKey, SourceLocalizationKey);
        }
    }

    /// <summary>港口服务单项只读投影。</summary>
    [Serializable]
    public sealed class D22PortServiceReadModel
    {
        public string ServiceId { get; }
        public string Verdict { get; }
        public bool IsEnabled { get; }
        public string ReasonCode { get; }
        public string DisabledReasonCode { get; }
        public string LocalizationKey { get; }
        public string SourceLocalizationKey { get; }
        public string RetryPolicy { get; }
        public string Detail { get; }
        public string RetryHint { get; }

        internal D22PortServiceReadModel(D19ServiceReadModelItem source)
        {
            ServiceId = source == null ? string.Empty : source.ServiceId ?? string.Empty;
            Verdict = source == null ? string.Empty : source.Verdict.ToString();
            IsEnabled = source != null && source.IsAvailable;
            ReasonCode = source == null ? "d22.port.service_missing" : source.ReasonCode ?? string.Empty;
            DisabledReasonCode = IsEnabled ? string.Empty :
                (string.IsNullOrEmpty(ReasonCode) ? "d22.port.service_unavailable" : ReasonCode);
            LocalizationKey = D22ReadModelAdapterSupport.Key("port", "service");
            SourceLocalizationKey = source == null ? string.Empty : source.FeedbackLocKey ?? string.Empty;
            RetryPolicy = source == null ? string.Empty : source.RetryPolicy.ToString();
            Detail = source == null ? string.Empty : source.Detail.ToString();
            RetryHint = source == null ? string.Empty : source.RetryHint.ToString();
        }

        private D22PortServiceReadModel(
            string serviceId,
            string verdict,
            bool isEnabled,
            string reasonCode,
            string disabledReasonCode,
            string localizationKey,
            string sourceLocalizationKey,
            string retryPolicy,
            string detail,
            string retryHint)
        {
            ServiceId = serviceId ?? string.Empty;
            Verdict = verdict ?? string.Empty;
            IsEnabled = isEnabled;
            ReasonCode = reasonCode ?? string.Empty;
            DisabledReasonCode = disabledReasonCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            SourceLocalizationKey = sourceLocalizationKey ?? string.Empty;
            RetryPolicy = retryPolicy ?? string.Empty;
            Detail = detail ?? string.Empty;
            RetryHint = retryHint ?? string.Empty;
        }

        public D22PortServiceReadModel Clone()
        {
            return new D22PortServiceReadModel(
                ServiceId, Verdict, IsEnabled, ReasonCode, DisabledReasonCode,
                LocalizationKey, SourceLocalizationKey, RetryPolicy, Detail, RetryHint);
        }
    }

    /// <summary>登船/出航检查项只读投影。</summary>
    [Serializable]
    public sealed class D22BoardingCheckReadModel
    {
        public string CheckId { get; }
        public string Owner { get; }
        public string Status { get; }
        public bool Passed { get; }
        public string OriginalCode { get; }
        public string FailureCode { get; }
        public string FailureCodeId { get; }
        public string LocalizationKey { get; }
        public string SourceLocalizationKey { get; }
        public string Detail { get; }

        internal D22BoardingCheckReadModel(D19DepartureCheckReadModelItem source)
        {
            CheckId = source == null ? string.Empty : source.CheckId ?? string.Empty;
            Owner = source == null ? string.Empty : source.Owner ?? string.Empty;
            Status = source == null ? string.Empty : source.Status.ToString();
            Passed = source != null && source.Passed;
            OriginalCode = source == null ? string.Empty : source.OriginalCode ?? string.Empty;
            FailureCode = source == null ? string.Empty : source.FailureCode.ToString();
            FailureCodeId = source == null ? string.Empty : source.FailureCodeId ?? string.Empty;
            LocalizationKey = D22ReadModelAdapterSupport.Key("port", "boarding_check");
            SourceLocalizationKey = source == null ? string.Empty : source.LocKey ?? string.Empty;
            Detail = source == null ? string.Empty : source.Detail ?? string.Empty;
        }

        private D22BoardingCheckReadModel(
            string checkId,
            string owner,
            string status,
            bool passed,
            string originalCode,
            string failureCode,
            string failureCodeId,
            string localizationKey,
            string sourceLocalizationKey,
            string detail)
        {
            CheckId = checkId ?? string.Empty;
            Owner = owner ?? string.Empty;
            Status = status ?? string.Empty;
            Passed = passed;
            OriginalCode = originalCode ?? string.Empty;
            FailureCode = failureCode ?? string.Empty;
            FailureCodeId = failureCodeId ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            SourceLocalizationKey = sourceLocalizationKey ?? string.Empty;
            Detail = detail ?? string.Empty;
        }

        public D22BoardingCheckReadModel Clone()
        {
            return new D22BoardingCheckReadModel(
                CheckId, Owner, Status, Passed, OriginalCode, FailureCode, FailureCodeId,
                LocalizationKey, SourceLocalizationKey, Detail);
        }
    }

    /// <summary>D22-L18 港口 UI 读模型。</summary>
    [Serializable]
    public sealed class D22PortReadModel : D22ReadModelAdapterModel
    {
        public string PortId { get; }
        public string ShipId { get; }
        public D22PortVisitReadModel Visit { get; }
        public int TotalBerths { get; }
        public int AvailableBerths { get; }
        public int OccupiedBerths { get; }
        public int ReservedBerths { get; }
        public IReadOnlyList<D22PortBerthReadModel> Berths { get; }
        public int TotalServices { get; }
        public int AvailableServices { get; }
        public IReadOnlyList<D22PortServiceReadModel> Services { get; }
        public string BoardingDestinationId { get; }
        public bool CanDepart { get; }
        public bool HasUnknownBoardingChecks { get; }
        public int TotalBoardingChecks { get; }
        public int PassedBoardingChecks { get; }
        public IReadOnlyList<D22BoardingCheckReadModel> BoardingChecks { get; }
        public IReadOnlyList<string> BlockingCheckIds { get; }
        public D22AdapterOutcomeReadModel LastAction { get; }

        private D22PortReadModel(
            D22ReadModelAdapterMetadata metadata,
            string portId,
            string shipId,
            D22PortVisitReadModel visit,
            IEnumerable<D22PortBerthReadModel> berths,
            int totalBerths,
            int availableBerths,
            int occupiedBerths,
            int reservedBerths,
            IEnumerable<D22PortServiceReadModel> services,
            int totalServices,
            int availableServices,
            string boardingDestinationId,
            bool canDepart,
            bool hasUnknownBoardingChecks,
            IEnumerable<D22BoardingCheckReadModel> boardingChecks,
            int totalBoardingChecks,
            int passedBoardingChecks,
            IEnumerable<string> blockingCheckIds,
            D22AdapterOutcomeReadModel lastAction)
            : base(metadata)
        {
            PortId = portId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            Visit = visit == null ? null : visit.Clone();
            TotalBerths = totalBerths;
            AvailableBerths = availableBerths;
            OccupiedBerths = occupiedBerths;
            ReservedBerths = reservedBerths;
            Berths = CopySortedBerths(berths);
            TotalServices = totalServices;
            AvailableServices = availableServices;
            Services = CopySortedServices(services);
            BoardingDestinationId = boardingDestinationId ?? string.Empty;
            CanDepart = canDepart;
            HasUnknownBoardingChecks = hasUnknownBoardingChecks;
            TotalBoardingChecks = totalBoardingChecks;
            PassedBoardingChecks = passedBoardingChecks;
            BoardingChecks = CopySortedChecks(boardingChecks);
            BlockingCheckIds = new ReadOnlyCollection<string>(D22ReadModelAdapterSupport.SortedDistinct(blockingCheckIds));
            LastAction = lastAction == null ? null : lastAction.Clone();
        }

        public D22PortReadModel Clone()
        {
            return new D22PortReadModel(
                CopyMetadata(), PortId, ShipId, Visit, Berths, TotalBerths, AvailableBerths,
                OccupiedBerths, ReservedBerths, Services, TotalServices, AvailableServices,
                BoardingDestinationId, CanDepart, HasUnknownBoardingChecks, BoardingChecks,
                TotalBoardingChecks, PassedBoardingChecks, BlockingCheckIds, LastAction);
        }

        public static D22PortReadModel Project(D22PortOwnerSnapshot snapshot)
        {
            var issues = new List<D22ReadModelAdapterIssue>();
            var missing = new List<string>();
            if (snapshot == null)
            {
                missing.Add("d19.port");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_snapshot", "d22.port.snapshot_missing", D22ReadModelAdapterSupport.Key("port", "blocked"),
                    "d22.port.snapshot_missing", "D19", "d19.port", true));
                return Empty(0, issues, missing);
            }
            if (snapshot.SchemaVersion != D22PortOwnerSnapshot.CurrentSchemaVersion)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_schema_version", "d22.port.schema_version_mismatch", D22ReadModelAdapterSupport.Key("port", "invalid"),
                    "d22.port.schema_version_mismatch", "D22PortOwnerSnapshot", "d22.port", true));
            }
            if (!string.Equals(snapshot.SourceSchemaVersionText, D19PortReadModel.SchemaVersion, StringComparison.Ordinal))
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_source_schema_version", "d22.port.source_schema_version_mismatch", D22ReadModelAdapterSupport.Key("port", "invalid"),
                    "d22.port.source_schema_version_mismatch", "D19", "d19.port", true));
            }
            if (snapshot.Port == null)
            {
                missing.Add("d19.port");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_port_module", "d22.port.source_missing", D22ReadModelAdapterSupport.Key("port", "blocked"),
                    "d22.port.source_missing", "D19", "d19.port", true));
                return Empty(0, issues, MergeMissing(missing, snapshot.MissingModuleIds));
            }

            var source = snapshot.Port;
            D22ReadModelAdapterSupport.ValidateRevisionAndDay(source.SaveRevision, source.WorldDay, "D19", "d19.port", issues);
            var visit = source.Visit;
            if (visit == null)
            {
                missing.Add("d19.port.visit");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_visit_projection", "d22.port.visit_missing", D22ReadModelAdapterSupport.Key("port", "partial"),
                    "d22.port.visit_missing", "D19", "d19.port.visit", false));
            }
            string portId = visit == null ? string.Empty : visit.PortId ?? string.Empty;
            string shipId = visit == null ? string.Empty : visit.ShipId ?? string.Empty;
            ValidateId(portId, "port_id_invalid", "d19.port.visit", issues);
            ValidateId(shipId, "ship_id_invalid", "d19.port.visit", issues, true);

            var berthItems = new List<D22PortBerthReadModel>();
            int totalBerths = 0;
            int availableBerths = 0;
            int occupiedBerths = 0;
            int reservedBerths = 0;
            if (source.Berths == null)
            {
                missing.Add("d19.port.berths");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_berth_projection", "d22.port.berths_missing", D22ReadModelAdapterSupport.Key("port", "partial"),
                    "d22.port.berths_missing", "D19", "d19.port.berths", false));
            }
            else
            {
                totalBerths = source.Berths.TotalBerths;
                availableBerths = source.Berths.AvailableBerths;
                occupiedBerths = source.Berths.OccupiedBerths;
                reservedBerths = source.Berths.ReservedBerths;
                var seen = new HashSet<string>(StringComparer.Ordinal);
                if (source.Berths.Berths != null)
                {
                    foreach (var item in source.Berths.Berths)
                    {
                        if (item == null || !D22ReadModelAdapterSupport.IsStableId(item.BerthId) || !seen.Add(item.BerthId))
                        {
                            issues.Add(D22ReadModelAdapterSupport.Issue(
                                "invalid_berth_projection", "d22.port.berth_invalid", D22ReadModelAdapterSupport.Key("port", "partial"),
                                "d22.port.berth_invalid", "D19", "d19.port.berths", false));
                            continue;
                        }
                        var copy = new D22PortBerthReadModel(item);
                        berthItems.Add(copy);
                        D22ReadModelAdapterSupport.ValidateSourceKey(item.StatusLocKey, "D19", "d19.port.berths", issues);
                    }
                }
            }

            var serviceItems = new List<D22PortServiceReadModel>();
            int totalServices = 0;
            int availableServices = 0;
            if (source.Services == null)
            {
                missing.Add("d19.port.services");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_service_projection", "d22.port.services_missing", D22ReadModelAdapterSupport.Key("port", "partial"),
                    "d22.port.services_missing", "D19", "d19.port.services", false));
            }
            else
            {
                totalServices = source.Services.TotalServices;
                availableServices = source.Services.AvailableServices;
                var seen = new HashSet<string>(StringComparer.Ordinal);
                if (source.Services.Services != null)
                {
                    foreach (var item in source.Services.Services)
                    {
                        if (item == null || !D22ReadModelAdapterSupport.IsStableId(item.ServiceId) || !seen.Add(item.ServiceId))
                        {
                            issues.Add(D22ReadModelAdapterSupport.Issue(
                                "invalid_service_projection", "d22.port.service_invalid", D22ReadModelAdapterSupport.Key("port", "partial"),
                                "d22.port.service_invalid", "D19", "d19.port.services", false));
                            continue;
                        }
                        serviceItems.Add(new D22PortServiceReadModel(item));
                        D22ReadModelAdapterSupport.ValidateSourceCode(item.ReasonCode, "D19", "d19.port.services", issues);
                        D22ReadModelAdapterSupport.ValidateSourceKey(item.ServiceLocKey, "D19", "d19.port.services", issues);
                        D22ReadModelAdapterSupport.ValidateSourceKey(item.FeedbackLocKey, "D19", "d19.port.services", issues);
                    }
                }
            }

            var boardingItems = new List<D22BoardingCheckReadModel>();
            string destinationId = string.Empty;
            bool canDepart = false;
            bool hasUnknown = false;
            int totalChecks = 0;
            int passedChecks = 0;
            var blockingCheckIds = new List<string>();
            if (source.DepartureChecklist == null)
            {
                missing.Add("d19.port.departure_checklist");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_departure_projection", "d22.port.departure_checklist_missing", D22ReadModelAdapterSupport.Key("port", "partial"),
                    "d22.port.departure_checklist_missing", "D19", "d19.port.departure", false));
            }
            else
            {
                var checklist = source.DepartureChecklist;
                destinationId = checklist.DestinationId ?? string.Empty;
                canDepart = checklist.CanDepart;
                hasUnknown = checklist.HasUnknown;
                totalChecks = checklist.TotalChecks;
                passedChecks = checklist.PassedChecks;
                if (checklist.BlockingCheckIds != null) blockingCheckIds.AddRange(checklist.BlockingCheckIds);
                var seen = new HashSet<string>(StringComparer.Ordinal);
                if (checklist.Checks != null)
                {
                    foreach (var item in checklist.Checks)
                    {
                        if (item == null || !D22ReadModelAdapterSupport.IsStableId(item.CheckId) || !seen.Add(item.CheckId))
                        {
                            issues.Add(D22ReadModelAdapterSupport.Issue(
                                "invalid_departure_check", "d22.port.departure_check_invalid", D22ReadModelAdapterSupport.Key("port", "partial"),
                                "d22.port.departure_check_invalid", "D19", "d19.port.departure", false));
                            continue;
                        }
                        boardingItems.Add(new D22BoardingCheckReadModel(item));
                        D22ReadModelAdapterSupport.ValidateSourceCode(item.OriginalCode, "D19", "d19.port.departure", issues);
                        D22ReadModelAdapterSupport.ValidateSourceCode(item.FailureCodeId, "D19", "d19.port.departure", issues);
                        D22ReadModelAdapterSupport.ValidateSourceKey(item.LocKey, "D19", "d19.port.departure", issues);
                    }
                }
            }
            ValidateId(destinationId, "destination_id_invalid", "d19.port.departure", issues, true);

            var feedbackSource = snapshot.LastActionFeedback ?? source.LastActionFeedback;
            D22AdapterOutcomeReadModel action = null;
            string sourceError = string.Empty;
            string sourceReason = snapshot.DisabledReasonCode ?? string.Empty;
            string sourceLoc = snapshot.DisabledLocalizationKey ?? string.Empty;
            if (feedbackSource != null)
            {
                sourceError = feedbackSource.FailureCode ?? string.Empty;
                sourceReason = string.IsNullOrEmpty(sourceReason) ? feedbackSource.ReasonId : sourceReason;
                sourceLoc = string.IsNullOrEmpty(sourceLoc) ? feedbackSource.FeedbackLocKey : sourceLoc;
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceError, "D19", "d19.port.action", issues);
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D19", "d19.port.action", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D19", "d19.port.action", issues);
                action = D22ReadModelAdapterSupport.Outcome(
                    "port", "D19", true, feedbackSource.Success, false, sourceError, sourceReason, sourceLoc,
                    feedbackSource.Success ? string.Empty :
                        (string.IsNullOrEmpty(sourceReason) ? "d22.port.action_rejected" : sourceReason), issues);
            }
            else if (!string.IsNullOrEmpty(sourceReason))
            {
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D22PortOwnerSnapshot", "d22.port", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D22PortOwnerSnapshot", "d22.port", issues);
                action = D22ReadModelAdapterSupport.Outcome(
                    "port", "D22PortOwnerSnapshot", true, false, false, sourceReason, sourceReason, sourceLoc, sourceReason, issues);
            }

            bool invalid = snapshot.SchemaVersion != D22PortOwnerSnapshot.CurrentSchemaVersion ||
                !string.Equals(snapshot.SourceSchemaVersionText, D19PortReadModel.SchemaVersion, StringComparison.Ordinal);
            bool blocked = false;
            bool partial = issues.Count > 0 || source.Berths == null || source.Services == null || source.DepartureChecklist == null;
            var status = D22ReadModelAdapterSupport.ResolveStatus(invalid, blocked, partial, issues);
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "port", "D19", 1, source.SaveRevision, source.WorldDay, status,
                sourceError, sourceError, sourceReason,
                string.IsNullOrEmpty(sourceReason) ? string.Empty : sourceReason,
                sourceLoc, MergeMissing(missing, snapshot.MissingModuleIds), issues);
            return new D22PortReadModel(
                metadata,
                portId,
                shipId,
                visit == null ? null : new D22PortVisitReadModel(visit),
                berthItems,
                totalBerths,
                availableBerths,
                occupiedBerths,
                reservedBerths,
                serviceItems,
                totalServices,
                availableServices,
                destinationId,
                canDepart,
                hasUnknown,
                boardingItems,
                totalChecks,
                passedChecks,
                blockingCheckIds,
                action);
        }

        public static D22PortReadModel Adapt(D22PortOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        public static D22PortReadModel FromSnapshot(D22PortOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        private static D22PortReadModel Empty(
            int worldDay,
            List<D22ReadModelAdapterIssue> issues,
            IEnumerable<string> missing)
        {
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "port", "D19", 0, 0, worldDay, D22ReadModelAdapterStatus.Blocked,
                "d22.port.source_missing", string.Empty, "d22.port.source_missing",
                "d22.port.source_missing", string.Empty, missing, issues);
            return new D22PortReadModel(
                metadata, string.Empty, string.Empty, null,
                new List<D22PortBerthReadModel>(), 0, 0, 0, 0,
                new List<D22PortServiceReadModel>(), 0, 0, string.Empty, false, true,
                new List<D22BoardingCheckReadModel>(), 0, 0, new List<string>(), null);
        }

        private static void ValidateId(
            string value,
            string code,
            string moduleId,
            List<D22ReadModelAdapterIssue> issues,
            bool allowEmpty = false)
        {
            if (!D22ReadModelAdapterSupport.IsStableId(value, allowEmpty))
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    code, "d22.port." + code, D22ReadModelAdapterSupport.Key("port", "partial"),
                    "d22.port." + code, "D19", moduleId, false));
            }
        }

        private static List<string> MergeMissing(List<string> current, IEnumerable<string> extra)
        {
            var result = new List<string>();
            if (current != null) result.AddRange(current);
            if (extra != null) result.AddRange(extra);
            return D22ReadModelAdapterSupport.SortedDistinct(result);
        }

        private static IReadOnlyList<D22PortBerthReadModel> CopySortedBerths(IEnumerable<D22PortBerthReadModel> source)
        {
            var result = new List<D22PortBerthReadModel>();
            if (source != null)
                foreach (var item in source)
                    if (item != null) result.Add(item.Clone());
            result.Sort(CompareBerths);
            return new ReadOnlyCollection<D22PortBerthReadModel>(result);
        }

        private static IReadOnlyList<D22PortServiceReadModel> CopySortedServices(IEnumerable<D22PortServiceReadModel> source)
        {
            var result = new List<D22PortServiceReadModel>();
            if (source != null)
                foreach (var item in source)
                    if (item != null) result.Add(item.Clone());
            result.Sort(CompareServices);
            return new ReadOnlyCollection<D22PortServiceReadModel>(result);
        }

        private static IReadOnlyList<D22BoardingCheckReadModel> CopySortedChecks(IEnumerable<D22BoardingCheckReadModel> source)
        {
            var result = new List<D22BoardingCheckReadModel>();
            if (source != null)
                foreach (var item in source)
                    if (item != null) result.Add(item.Clone());
            result.Sort(CompareChecks);
            return new ReadOnlyCollection<D22BoardingCheckReadModel>(result);
        }

        private static int CompareBerths(D22PortBerthReadModel left, D22PortBerthReadModel right)
        {
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.BerthId,
                right == null ? null : right.BerthId);
        }

        private static int CompareServices(D22PortServiceReadModel left, D22PortServiceReadModel right)
        {
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.ServiceId,
                right == null ? null : right.ServiceId);
        }

        private static int CompareChecks(D22BoardingCheckReadModel left, D22BoardingCheckReadModel right)
        {
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.CheckId,
                right == null ? null : right.CheckId);
        }
    }
}
