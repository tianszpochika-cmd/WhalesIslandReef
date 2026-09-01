// ============================================================
// D22MarketReadModelAdapter —— D22-L17 市场只读 UI 适配器（纯 C#）
//
// 本文件同时承载 D22 读模型适配器共用的不可变元数据/问题/结果类型。
// 适配器只消费 D3/D19 的公开快照或结果，永远不调用 owner 写入口；
// 输入集合、结果与价格原因在投影边界被复制，UI 只能获得只读集合。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Whalebone.Systems.D3;
using Whalebone.Systems.D19;

namespace Whalebone.Systems.D22
{
    /// <summary>D22 读模型适配结果的保守状态。</summary>
    public enum D22ReadModelAdapterStatus
    {
        Ready = 0,
        Partial = 1,
        Blocked = 2,
        Invalid = 3,
    }

    /// <summary>适配器发现的结构化问题；不包含用户可见文案。</summary>
    [Serializable]
    public sealed class D22ReadModelAdapterIssue
    {
        public string Code { get; }
        public string ReasonCode { get; }
        public string LocalizationKey { get; }
        public string DisabledReasonCode { get; }
        public string SourceOwner { get; }
        public string ModuleId { get; }
        public bool Blocking { get; }

        public D22ReadModelAdapterIssue(
            string code,
            string reasonCode,
            string localizationKey,
            string disabledReasonCode,
            string sourceOwner,
            string moduleId,
            bool blocking)
        {
            Code = code ?? string.Empty;
            ReasonCode = reasonCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            DisabledReasonCode = disabledReasonCode ?? string.Empty;
            SourceOwner = sourceOwner ?? string.Empty;
            ModuleId = moduleId ?? string.Empty;
            Blocking = blocking;
        }

        public D22ReadModelAdapterIssue Clone()
        {
            return new D22ReadModelAdapterIssue(
                Code,
                ReasonCode,
                LocalizationKey,
                DisabledReasonCode,
                SourceOwner,
                ModuleId,
                Blocking);
        }
    }

    /// <summary>适配器根元数据；所有集合和反馈均为防御性副本。</summary>
    [Serializable]
    public sealed class D22ReadModelAdapterMetadata
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; }
        public string AdapterId { get; }
        public string SourceOwner { get; }
        public int SourceSchemaVersion { get; }
        public long SourceRevision { get; }
        public int WorldDay { get; }
        public D22ReadModelAdapterStatus Status { get; }
        public string ErrorCode { get; }
        public string SourceErrorCode { get; }
        public string ReasonCode { get; }
        public string LocalizationKey { get; }
        public string DisabledReasonCode { get; }
        public string FeedbackCode { get; }
        public bool HasBlockingIssue { get; }
        public IReadOnlyList<string> MissingModuleIds { get; }
        public IReadOnlyList<D22ReadModelAdapterIssue> Issues { get; }
        public D22FeedbackRecord Feedback { get; }

        internal D22ReadModelAdapterMetadata(
            string adapterId,
            string sourceOwner,
            int sourceSchemaVersion,
            long sourceRevision,
            int worldDay,
            D22ReadModelAdapterStatus status,
            string errorCode,
            string sourceErrorCode,
            string reasonCode,
            string localizationKey,
            string disabledReasonCode,
            string feedbackCode,
            IEnumerable<string> missingModuleIds,
            IEnumerable<D22ReadModelAdapterIssue> issues,
            D22FeedbackRecord feedback)
        {
            SchemaVersion = CurrentSchemaVersion;
            AdapterId = adapterId ?? string.Empty;
            SourceOwner = sourceOwner ?? string.Empty;
            SourceSchemaVersion = sourceSchemaVersion;
            SourceRevision = sourceRevision;
            WorldDay = worldDay;
            Status = status;
            ErrorCode = errorCode ?? string.Empty;
            SourceErrorCode = sourceErrorCode ?? string.Empty;
            ReasonCode = reasonCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            DisabledReasonCode = disabledReasonCode ?? string.Empty;
            FeedbackCode = feedbackCode ?? string.Empty;

            var missing = new List<string>();
            if (missingModuleIds != null)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var moduleId in missingModuleIds)
                {
                    if (!string.IsNullOrEmpty(moduleId) && seen.Add(moduleId))
                        missing.Add(moduleId);
                }
            }
            missing.Sort(StringComparer.Ordinal);
            MissingModuleIds = new ReadOnlyCollection<string>(missing);

            var copiedIssues = new List<D22ReadModelAdapterIssue>();
            if (issues != null)
            {
                foreach (var issue in issues)
                    if (issue != null) copiedIssues.Add(issue.Clone());
            }
            copiedIssues.Sort(CompareIssues);
            HasBlockingIssue = false;
            foreach (var issue in copiedIssues)
                if (issue.Blocking) HasBlockingIssue = true;
            Issues = new ReadOnlyCollection<D22ReadModelAdapterIssue>(copiedIssues);
            Feedback = feedback == null ? null : feedback.Clone();
        }

        public D22ReadModelAdapterMetadata Clone()
        {
            return new D22ReadModelAdapterMetadata(
                AdapterId,
                SourceOwner,
                SourceSchemaVersion,
                SourceRevision,
                WorldDay,
                Status,
                ErrorCode,
                SourceErrorCode,
                ReasonCode,
                LocalizationKey,
                DisabledReasonCode,
                FeedbackCode,
                MissingModuleIds,
                Issues,
                Feedback);
        }

        private static int CompareIssues(D22ReadModelAdapterIssue left, D22ReadModelAdapterIssue right)
        {
            int result = string.Compare(left == null ? string.Empty : left.Code,
                right == null ? string.Empty : right.Code, StringComparison.Ordinal);
            if (result != 0) return result;
            result = string.Compare(left == null ? string.Empty : left.ModuleId,
                right == null ? string.Empty : right.ModuleId, StringComparison.Ordinal);
            if (result != 0) return result;
            return string.Compare(left == null ? string.Empty : left.ReasonCode,
                right == null ? string.Empty : right.ReasonCode, StringComparison.Ordinal);
        }
    }

    /// <summary>适配器统一输出的 owner 结果摘要。</summary>
    [Serializable]
    public sealed class D22AdapterOutcomeReadModel
    {
        public bool HasResult { get; }
        public bool Succeeded { get; }
        public bool IsDuplicate { get; }
        public string ErrorCode { get; }
        public string SourceErrorCode { get; }
        public string ReasonCode { get; }
        public string LocalizationKey { get; }
        public string SourceLocalizationKey { get; }
        public string DisabledReasonCode { get; }
        public D22FeedbackRecord Feedback { get; }

        internal D22AdapterOutcomeReadModel(
            bool hasResult,
            bool succeeded,
            bool isDuplicate,
            string errorCode,
            string sourceErrorCode,
            string reasonCode,
            string localizationKey,
            string sourceLocalizationKey,
            string disabledReasonCode,
            D22FeedbackRecord feedback)
        {
            HasResult = hasResult;
            Succeeded = succeeded;
            IsDuplicate = isDuplicate;
            ErrorCode = errorCode ?? string.Empty;
            SourceErrorCode = sourceErrorCode ?? string.Empty;
            ReasonCode = reasonCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            SourceLocalizationKey = sourceLocalizationKey ?? string.Empty;
            DisabledReasonCode = disabledReasonCode ?? string.Empty;
            Feedback = feedback == null ? null : feedback.Clone();
        }

        public D22AdapterOutcomeReadModel Clone()
        {
            return new D22AdapterOutcomeReadModel(
                HasResult,
                Succeeded,
                IsDuplicate,
                ErrorCode,
                SourceErrorCode,
                ReasonCode,
                LocalizationKey,
                SourceLocalizationKey,
                DisabledReasonCode,
                Feedback);
        }
    }

    /// <summary>公共只读投影基类；派生模型没有任何 owner 写引用。</summary>
    [Serializable]
    public class D22ReadModelAdapterModel
    {
        private readonly D22ReadModelAdapterMetadata _metadata;

        protected D22ReadModelAdapterModel(D22ReadModelAdapterMetadata metadata)
        {
            _metadata = metadata ?? D22ReadModelAdapterSupport.MissingMetadata();
        }

        public D22ReadModelAdapterMetadata Metadata
        {
            get { return _metadata.Clone(); }
        }

        public D22ReadModelAdapterStatus Status
        {
            get { return _metadata.Status; }
        }

        /// <summary>Ready/Partial 都代表有可安全读取的投影；Blocked/Invalid 不代表完整有效数据。</summary>
        public bool IsValid
        {
            get { return Status == D22ReadModelAdapterStatus.Ready || Status == D22ReadModelAdapterStatus.Partial; }
        }

        public bool IsPartial
        {
            get { return Status == D22ReadModelAdapterStatus.Partial; }
        }

        public bool IsBlocked
        {
            get { return Status == D22ReadModelAdapterStatus.Blocked || _metadata.HasBlockingIssue; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public string ErrorCode { get { return _metadata.ErrorCode; } }
        public string SourceErrorCode { get { return _metadata.SourceErrorCode; } }
        public string ReasonCode { get { return _metadata.ReasonCode; } }
        public string LocalizationKey { get { return _metadata.LocalizationKey; } }
        public string DisabledReasonCode { get { return _metadata.DisabledReasonCode; } }
        public string FeedbackCode { get { return _metadata.FeedbackCode; } }
        public IReadOnlyList<string> MissingModuleIds { get { return _metadata.MissingModuleIds; } }
        public IReadOnlyList<D22ReadModelAdapterIssue> Issues { get { return _metadata.Issues; } }
        public D22FeedbackRecord Feedback { get { return _metadata.Feedback == null ? null : _metadata.Feedback.Clone(); } }

        protected D22ReadModelAdapterMetadata CopyMetadata()
        {
            return _metadata.Clone();
        }
    }

    /// <summary>共享校验、稳定排序和 feedback 映射工具。</summary>
    internal static class D22ReadModelAdapterSupport
    {
        public static readonly StringComparer Ordinal = StringComparer.Ordinal;

        public static D22ReadModelAdapterMetadata MissingMetadata()
        {
            var issues = new List<D22ReadModelAdapterIssue>
            {
                Issue("missing_snapshot", "d22.adapter.snapshot_missing", "ui.d22.adapter.invalid", "d22.adapter.snapshot_missing", "D22", "d22.snapshot", true),
            };
            return BuildMetadata(
                "unknown",
                "D22",
                0,
                0,
                0,
                D22ReadModelAdapterStatus.Blocked,
                "d22.adapter.snapshot_missing",
                string.Empty,
                "d22.adapter.snapshot_missing",
                "",
                "d22.adapter.snapshot_missing",
                new List<string> { "d22.snapshot" },
                issues);
        }

        public static D22ReadModelAdapterIssue Issue(
            string code,
            string reasonCode,
            string localizationKey,
            string disabledReasonCode,
            string sourceOwner,
            string moduleId,
            bool blocking)
        {
            return new D22ReadModelAdapterIssue(
                code,
                reasonCode,
                localizationKey,
                disabledReasonCode,
                sourceOwner,
                moduleId,
                blocking);
        }

        public static string Key(string adapterId, string state)
        {
            return "ui.d22.adapter." + (adapterId ?? "unknown") + "." + (state ?? "unknown");
        }

        public static bool IsStableId(string value, bool allowEmpty = false)
        {
            return allowEmpty && string.IsNullOrEmpty(value) || D22ScreenContracts.IsStableId(value);
        }

        public static bool IsStableSourceKey(string value)
        {
            return !string.IsNullOrEmpty(value) && D22ScreenContracts.IsStableId(value);
        }

        public static string StableSourceCode(string value, string fallback)
        {
            if (IsStableId(value)) return value;
            return fallback ?? string.Empty;
        }

        public static void ValidateSourceCode(
            string value,
            string sourceOwner,
            string moduleId,
            List<D22ReadModelAdapterIssue> issues)
        {
            if (!string.IsNullOrEmpty(value) && !D22ScreenContracts.IsStableId(value))
            {
                issues.Add(Issue(
                    "invalid_error_code",
                    "d22.adapter.invalid_source_error_code",
                    Key("shared", "invalid_source_error_code"),
                    "d22.adapter.invalid_source_error_code",
                    sourceOwner,
                    moduleId,
                    false));
            }
        }

        public static void ValidateSourceKey(
            string value,
            string sourceOwner,
            string moduleId,
            List<D22ReadModelAdapterIssue> issues)
        {
            if (!string.IsNullOrEmpty(value) && !IsStableSourceKey(value))
            {
                issues.Add(Issue(
                    "invalid_localization_key",
                    "d22.adapter.invalid_source_localization_key",
                    Key("shared", "invalid_source_localization_key"),
                    "d22.adapter.invalid_source_localization_key",
                    sourceOwner,
                    moduleId,
                    false));
            }
        }

        public static void ValidateRevisionAndDay(
            long revision,
            int worldDay,
            string sourceOwner,
            string moduleId,
            List<D22ReadModelAdapterIssue> issues)
        {
            if (revision < 0)
            {
                issues.Add(Issue(
                    "invalid_revision",
                    "d22.adapter.revision_invalid",
                    Key("shared", "invalid_revision"),
                    "d22.adapter.revision_invalid",
                    sourceOwner,
                    moduleId,
                    true));
            }
            if (worldDay < 0)
            {
                issues.Add(Issue(
                    "invalid_world_day",
                    "d22.adapter.world_day_invalid",
                    Key("shared", "invalid_world_day"),
                    "d22.adapter.world_day_invalid",
                    sourceOwner,
                    moduleId,
                    true));
            }
        }

        public static D22ReadModelAdapterStatus ResolveStatus(
            bool invalid,
            bool blocked,
            bool partial,
            List<D22ReadModelAdapterIssue> issues)
        {
            if (issues != null)
            {
                foreach (var issue in issues)
                {
                    if (issue == null) continue;
                    if (issue.Code.StartsWith("invalid_", StringComparison.Ordinal) ||
                        issue.Code.EndsWith("_invalid", StringComparison.Ordinal) ||
                        issue.Code == "invalid_schema_version" ||
                        issue.Code == "invalid_revision" ||
                        issue.Code == "invalid_world_day" ||
                        issue.Code == "invalid_source_shape")
                        invalid = true;
                    if (issue.Blocking) blocked = true;
                    if (issue.Code != "") partial = true;
                }
            }
            if (invalid) return D22ReadModelAdapterStatus.Invalid;
            if (blocked) return D22ReadModelAdapterStatus.Blocked;
            if (partial) return D22ReadModelAdapterStatus.Partial;
            return D22ReadModelAdapterStatus.Ready;
        }

        public static D22ReadModelAdapterMetadata BuildMetadata(
            string adapterId,
            string sourceOwner,
            int sourceSchemaVersion,
            long sourceRevision,
            int worldDay,
            D22ReadModelAdapterStatus status,
            string errorCode,
            string sourceErrorCode,
            string reasonCode,
            string disabledReasonCode,
            string sourceLocalizationKey,
            IEnumerable<string> missingModuleIds,
            IEnumerable<D22ReadModelAdapterIssue> issues)
        {
            string state = status == D22ReadModelAdapterStatus.Ready ? "ready" :
                status == D22ReadModelAdapterStatus.Partial ? "partial" :
                status == D22ReadModelAdapterStatus.Blocked ? "blocked" : "invalid";
            string localizationKey = Key(adapterId, state);
            string safeError = string.IsNullOrEmpty(errorCode)
                ? (status == D22ReadModelAdapterStatus.Ready ? string.Empty : "d22.adapter.owner_unavailable")
                : StableSourceCode(errorCode, "d22.adapter.owner_error");
            string safeReason = reasonCode ?? string.Empty;
            if (string.IsNullOrEmpty(safeReason) && issues != null)
            {
                foreach (var issue in issues)
                {
                    if (issue != null && !string.IsNullOrEmpty(issue.ReasonCode))
                    {
                        safeReason = issue.ReasonCode;
                        break;
                    }
                }
            }
            string feedbackCode = status == D22ReadModelAdapterStatus.Ready
                ? D22FeedbackCodes.CommandCompleted
                : status == D22ReadModelAdapterStatus.Partial
                    ? D22FeedbackCodes.OwnerPending
                    : status == D22ReadModelAdapterStatus.Blocked
                        ? D22FeedbackCodes.OwnerUnavailable
                        : D22FeedbackCodes.OwnerRejected;
            var feedback = D22FeedbackCatalog.Create(feedbackCode, adapterId, sourceOwner, safeReason);
            return new D22ReadModelAdapterMetadata(
                adapterId,
                sourceOwner,
                sourceSchemaVersion,
                sourceRevision,
                worldDay,
                status,
                safeError,
                sourceErrorCode,
                safeReason,
                localizationKey,
                disabledReasonCode,
                feedbackCode,
                missingModuleIds,
                issues,
                feedback);
        }

        public static D22AdapterOutcomeReadModel Outcome(
            string adapterId,
            string sourceOwner,
            bool hasResult,
            bool succeeded,
            bool duplicate,
            string sourceErrorCode,
            string sourceReasonCode,
            string sourceLocalizationKey,
            string disabledReasonCode,
            List<D22ReadModelAdapterIssue> issues)
        {
            string errorCode = succeeded ? string.Empty :
                StableSourceCode(sourceErrorCode, "d22.adapter.owner_rejected");
            string reasonCode = sourceReasonCode ?? string.Empty;
            if (string.IsNullOrEmpty(reasonCode)) reasonCode = sourceErrorCode ?? string.Empty;
            string ownKey = Key(adapterId, succeeded ? "result_succeeded" : "result_failed");
            string feedbackCode = succeeded
                ? (duplicate ? D22FeedbackCodes.DuplicateCommand : D22FeedbackCodes.CommandCompleted)
                : (string.IsNullOrEmpty(disabledReasonCode) ? D22FeedbackCodes.OwnerRejected : D22FeedbackCodes.Disabled);
            var feedback = D22FeedbackCatalog.Create(feedbackCode, adapterId, sourceOwner, reasonCode);
            return new D22AdapterOutcomeReadModel(
                hasResult,
                succeeded,
                duplicate,
                errorCode,
                sourceErrorCode,
                reasonCode,
                ownKey,
                sourceLocalizationKey,
                disabledReasonCode,
                feedback);
        }

        public static List<string> SortedDistinct(IEnumerable<string> values)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (values != null)
            {
                foreach (var value in values)
                    if (!string.IsNullOrEmpty(value) && seen.Add(value)) result.Add(value);
            }
            result.Sort(StringComparer.Ordinal);
            return result;
        }

        public static int CompareNullable(string left, string right)
        {
            return string.Compare(left ?? string.Empty, right ?? string.Empty, StringComparison.Ordinal);
        }
    }

    /// <summary>D3/D19 市场 owner 的输入边界；调用方负责提供公开快照副本。</summary>
    [Serializable]
    public sealed class D22MarketOwnerSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public int SourceSchemaVersion = 1;
        public int WorldDay;
        public long SaveRevision;
        public string PortId = string.Empty;
        public D3MarketState Market;
        public D3TradeState TradeState;
        public List<MarketQuote> Quotes = new List<MarketQuote>();
        public D19MarketTradeResult LastPortTradeResult;
        public TradeResult LastTradeResult;
        public string DisabledReasonCode = string.Empty;
        public string DisabledLocalizationKey = string.Empty;
        public List<string> MissingModuleIds = new List<string>();
    }

    /// <summary>价格解释只读条目。</summary>
    [Serializable]
    public sealed class D22PriceReasonReadModel
    {
        public string Kind { get; }
        public string Code { get; }
        public string Explanation { get; }
        public float Coefficient { get; }

        internal D22PriceReasonReadModel(PriceReason source)
        {
            Kind = source == null ? string.Empty : source.Kind.ToString();
            Code = source == null ? string.Empty : source.Code ?? string.Empty;
            Explanation = source == null ? string.Empty : source.Explanation ?? string.Empty;
            Coefficient = source == null ? 0f : source.Coefficient;
        }

        public D22PriceReasonReadModel Clone()
        {
            return new D22PriceReasonReadModel(Kind, Code, Explanation, Coefficient);
        }

        private D22PriceReasonReadModel(string kind, string code, string explanation, float coefficient)
        {
            Kind = kind ?? string.Empty;
            Code = code ?? string.Empty;
            Explanation = explanation ?? string.Empty;
            Coefficient = coefficient;
        }
    }

    /// <summary>市场商品/报价只读条目。</summary>
    [Serializable]
    public sealed class D22MarketItemReadModel
    {
        public string CommodityId { get; }
        public int BuyUnitPrice { get; }
        public int SellUnitPrice { get; }
        public int Stock { get; }
        public int Demand { get; }
        public int PlayerTradeVolume { get; }
        public bool IsOutOfStock { get; }
        public bool IsEnabled { get; }
        public string DisabledReasonCode { get; }
        public string LocalizationKey { get; }
        public string SourceLocalizationKey { get; }
        public string SourceErrorCode { get; }
        public IReadOnlyList<D22PriceReasonReadModel> PriceReasons { get; }

        internal D22MarketItemReadModel(
            string commodityId,
            MarketQuote quote,
            bool enabled,
            string disabledReasonCode,
            string sourceLocalizationKey,
            string sourceErrorCode,
            IEnumerable<D22PriceReasonReadModel> reasons)
        {
            CommodityId = commodityId ?? string.Empty;
            BuyUnitPrice = quote == null ? 0 : quote.BuyUnitPrice;
            SellUnitPrice = quote == null ? 0 : quote.SellUnitPrice;
            Stock = quote == null ? 0 : quote.Stock;
            Demand = quote == null ? 0 : quote.Demand;
            PlayerTradeVolume = quote == null ? 0 : quote.PlayerTradeVolume;
            IsOutOfStock = quote == null || quote.IsOutOfStock;
            IsEnabled = enabled && quote != null && !IsOutOfStock;
            DisabledReasonCode = disabledReasonCode ?? string.Empty;
            LocalizationKey = D22ReadModelAdapterSupport.Key("market", "item");
            SourceLocalizationKey = sourceLocalizationKey ?? string.Empty;
            SourceErrorCode = sourceErrorCode ?? string.Empty;
            var copiedReasons = new List<D22PriceReasonReadModel>();
            if (reasons != null)
                foreach (var reason in reasons)
                    if (reason != null) copiedReasons.Add(reason.Clone());
            copiedReasons.Sort(CompareReasons);
            PriceReasons = new ReadOnlyCollection<D22PriceReasonReadModel>(copiedReasons);
        }

        private D22MarketItemReadModel(
            string commodityId,
            int buyUnitPrice,
            int sellUnitPrice,
            int stock,
            int demand,
            int playerTradeVolume,
            bool isOutOfStock,
            bool isEnabled,
            string disabledReasonCode,
            string localizationKey,
            string sourceLocalizationKey,
            string sourceErrorCode,
            IEnumerable<D22PriceReasonReadModel> reasons)
        {
            CommodityId = commodityId ?? string.Empty;
            BuyUnitPrice = buyUnitPrice;
            SellUnitPrice = sellUnitPrice;
            Stock = stock;
            Demand = demand;
            PlayerTradeVolume = playerTradeVolume;
            IsOutOfStock = isOutOfStock;
            IsEnabled = isEnabled;
            DisabledReasonCode = disabledReasonCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            SourceLocalizationKey = sourceLocalizationKey ?? string.Empty;
            SourceErrorCode = sourceErrorCode ?? string.Empty;
            var copiedReasons = new List<D22PriceReasonReadModel>();
            if (reasons != null)
                foreach (var reason in reasons)
                    if (reason != null) copiedReasons.Add(reason.Clone());
            copiedReasons.Sort(CompareReasons);
            PriceReasons = new ReadOnlyCollection<D22PriceReasonReadModel>(copiedReasons);
        }

        public D22MarketItemReadModel Clone()
        {
            return new D22MarketItemReadModel(
                CommodityId,
                BuyUnitPrice,
                SellUnitPrice,
                Stock,
                Demand,
                PlayerTradeVolume,
                IsOutOfStock,
                IsEnabled,
                DisabledReasonCode,
                LocalizationKey,
                SourceLocalizationKey,
                SourceErrorCode,
                PriceReasons);
        }

        private static int CompareReasons(D22PriceReasonReadModel left, D22PriceReasonReadModel right)
        {
            int result = D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.Code,
                right == null ? null : right.Code);
            if (result != 0) return result;
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.Kind,
                right == null ? null : right.Kind);
        }
    }

    /// <summary>D22-L17 市场 UI 读模型。</summary>
    [Serializable]
    public sealed class D22MarketReadModel : D22ReadModelAdapterModel
    {
        public string PortId { get; }
        public int WorldDay { get; }
        public long SaveRevision { get; }
        public bool HasMoney { get; }
        public int Money { get; }
        public string Weather { get; }
        public string FactionPolicyId { get; }
        public float FactionPolicyCoefficient { get; }
        public IReadOnlyList<D22MarketItemReadModel> Items { get; }
        public D22AdapterOutcomeReadModel LastTrade { get; }

        private D22MarketReadModel(
            D22ReadModelAdapterMetadata metadata,
            string portId,
            int worldDay,
            long saveRevision,
            bool hasMoney,
            int money,
            string weather,
            string factionPolicyId,
            float factionPolicyCoefficient,
            IEnumerable<D22MarketItemReadModel> items,
            D22AdapterOutcomeReadModel lastTrade)
            : base(metadata)
        {
            PortId = portId ?? string.Empty;
            WorldDay = worldDay;
            SaveRevision = saveRevision;
            HasMoney = hasMoney;
            Money = money;
            Weather = weather ?? string.Empty;
            FactionPolicyId = factionPolicyId ?? string.Empty;
            FactionPolicyCoefficient = factionPolicyCoefficient;
            var copiedItems = new List<D22MarketItemReadModel>();
            if (items != null)
                foreach (var item in items)
                    if (item != null) copiedItems.Add(item.Clone());
            copiedItems.Sort(CompareItems);
            Items = new ReadOnlyCollection<D22MarketItemReadModel>(copiedItems);
            LastTrade = lastTrade == null ? null : lastTrade.Clone();
        }

        public D22MarketReadModel Clone()
        {
            return new D22MarketReadModel(
                CopyMetadata(),
                PortId,
                WorldDay,
                SaveRevision,
                HasMoney,
                Money,
                Weather,
                FactionPolicyId,
                FactionPolicyCoefficient,
                Items,
                LastTrade);
        }

        public static D22MarketReadModel Project(D22MarketOwnerSnapshot snapshot)
        {
            var issues = new List<D22ReadModelAdapterIssue>();
            var missing = new List<string>();
            if (snapshot == null)
            {
                missing.Add("d3.market");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_snapshot", "d22.market.snapshot_missing", D22ReadModelAdapterSupport.Key("market", "blocked"),
                    "d22.market.snapshot_missing", "D3", "d3.market", true));
                return Empty(0, 0, D22ReadModelAdapterStatus.Blocked, issues, missing);
            }
            if (snapshot.SchemaVersion != D22MarketOwnerSnapshot.CurrentSchemaVersion)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_schema_version", "d22.market.schema_version_mismatch", D22ReadModelAdapterSupport.Key("market", "invalid"),
                    "d22.market.schema_version_mismatch", "D22MarketOwnerSnapshot", "d22.market", true));
            }
            var market = snapshot.Market ?? (snapshot.TradeState == null ? null : snapshot.TradeState.Market);
            if (market == null)
            {
                missing.Add("d3.market");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_market_module", "d22.market.source_missing", D22ReadModelAdapterSupport.Key("market", "blocked"),
                    "d22.market.source_missing", "D3", "d3.market", true));
            }
            D22ReadModelAdapterSupport.ValidateRevisionAndDay(snapshot.SaveRevision, snapshot.WorldDay, "D3", "d3.market", issues);
            if (!D22ReadModelAdapterSupport.IsStableId(snapshot.PortId, true))
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_port_id", "d22.market.port_id_invalid", D22ReadModelAdapterSupport.Key("market", "partial"),
                    "d22.market.port_id_invalid", "D3", "d3.market", false));
            }

            var quoteMap = new Dictionary<string, MarketQuote>(StringComparer.Ordinal);
            if (snapshot.Quotes != null)
            {
                foreach (var quote in snapshot.Quotes)
                {
                    if (quote == null || !D22ReadModelAdapterSupport.IsStableId(quote.CommodityId))
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_market_quote", "d22.market.quote_invalid", D22ReadModelAdapterSupport.Key("market", "partial"),
                            "d22.market.quote_invalid", "D3", "d3.market", false));
                        continue;
                    }
                    if (quoteMap.ContainsKey(quote.CommodityId))
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "duplicate_market_quote", "d22.market.quote_duplicate", D22ReadModelAdapterSupport.Key("market", "partial"),
                            "d22.market.quote_duplicate", "D3", "d3.market", false));
                        continue;
                    }
                    quoteMap.Add(quote.CommodityId, quote);
                }
            }

            var commodityIds = new List<string>();
            if (market != null)
            {
                AddDictionaryKeys(commodityIds, market.Stock, issues);
                AddDictionaryKeys(commodityIds, market.Demand, issues);
                AddDictionaryKeys(commodityIds, market.PlayerTradeVolume, issues);
            }
            foreach (var quoteId in quoteMap.Keys)
                if (!commodityIds.Contains(quoteId)) commodityIds.Add(quoteId);
            commodityIds.Sort(StringComparer.Ordinal);

            var items = new List<D22MarketItemReadModel>();
            foreach (var commodityId in commodityIds)
            {
                MarketQuote quote;
                quoteMap.TryGetValue(commodityId, out quote);
                var reasons = new List<D22PriceReasonReadModel>();
                if (quote != null && quote.PriceReasons != null)
                    foreach (var reason in quote.PriceReasons)
                        if (reason != null) reasons.Add(new D22PriceReasonReadModel(reason));
                bool enabled = quote != null && !quote.IsOutOfStock;
                string disabledReason = enabled ? string.Empty :
                    (quote == null ? "d22.market.quote_missing" : "d22.market.out_of_stock");
                items.Add(new D22MarketItemReadModel(
                    commodityId,
                    quote,
                    enabled,
                    disabledReason,
                    string.Empty,
                    string.Empty,
                    reasons));
                if (quote == null)
                {
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "missing_market_quote", "d22.market.quote_missing", D22ReadModelAdapterSupport.Key("market", "partial"),
                        "d22.market.quote_missing", "D3", "d3.market", false));
                }
            }

            bool hasMoney = snapshot.TradeState != null;
            int money = hasMoney ? snapshot.TradeState.Money : 0;
            if (!hasMoney)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_money_projection", "d22.market.money_missing", D22ReadModelAdapterSupport.Key("market", "partial"),
                    "d22.market.money_missing", "D3", "d3.trade", false));
            }

            string sourceError = string.Empty;
            string sourceReason = snapshot.DisabledReasonCode ?? string.Empty;
            string sourceLoc = snapshot.DisabledLocalizationKey ?? string.Empty;
            bool disabled = !string.IsNullOrEmpty(sourceReason);
            D22AdapterOutcomeReadModel outcome = null;
            if (snapshot.LastPortTradeResult != null)
            {
                var trade = snapshot.LastPortTradeResult;
                sourceError = !string.IsNullOrEmpty(trade.ReasonCode) ? trade.ReasonCode : trade.OriginalD3Code;
                sourceReason = string.IsNullOrEmpty(sourceReason) ? trade.ReasonCode : sourceReason;
                sourceLoc = string.IsNullOrEmpty(sourceLoc) ? trade.LocKey : sourceLoc;
                disabled = disabled || !trade.Success;
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceError, "D19", "d19.market", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D19", "d19.market", issues);
                outcome = D22ReadModelAdapterSupport.Outcome(
                    "market", "D19", true, trade.Success, trade.DuplicateSettlement,
                    sourceError, sourceReason, sourceLoc,
                    disabled ? (string.IsNullOrEmpty(sourceReason) ? "d22.market.owner_rejected" : sourceReason) : string.Empty,
                    issues);
            }
            else if (snapshot.LastTradeResult != null)
            {
                var trade = snapshot.LastTradeResult;
                sourceError = trade.Code ?? string.Empty;
                sourceReason = string.IsNullOrEmpty(sourceReason) ? sourceError : sourceReason;
                disabled = disabled || !trade.Succeeded;
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceError, "D3", "d3.trade", issues);
                outcome = D22ReadModelAdapterSupport.Outcome(
                    "market", "D3", true, trade.Succeeded, trade.DuplicateSettlement,
                    sourceError, sourceReason, string.Empty,
                    disabled ? (string.IsNullOrEmpty(sourceReason) ? "d22.market.owner_rejected" : sourceReason) : string.Empty,
                    issues);
            }
            else if (disabled)
            {
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D22MarketOwnerSnapshot", "d22.market", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D22MarketOwnerSnapshot", "d22.market", issues);
                outcome = D22ReadModelAdapterSupport.Outcome(
                    "market", "D22MarketOwnerSnapshot", true, false, false,
                    sourceReason, sourceReason, sourceLoc, sourceReason, issues);
            }

            if (disabled && string.IsNullOrEmpty(sourceReason))
                sourceReason = "d22.market.owner_rejected";
            bool invalid = snapshot.SchemaVersion != D22MarketOwnerSnapshot.CurrentSchemaVersion;
            bool blocked = market == null;
            bool partial = issues.Count > 0 || snapshot.SourceSchemaVersion <= 0;
            if (snapshot.SourceSchemaVersion <= 0)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_source_schema_version", "d22.market.source_schema_version_invalid", D22ReadModelAdapterSupport.Key("market", "invalid"),
                    "d22.market.source_schema_version_invalid", "D3", "d3.market", true));
                invalid = true;
            }
            var status = D22ReadModelAdapterSupport.ResolveStatus(invalid, blocked, partial, issues);
            if (disabled && status == D22ReadModelAdapterStatus.Ready) status = D22ReadModelAdapterStatus.Partial;
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "market",
                "D3",
                snapshot.SourceSchemaVersion,
                snapshot.SaveRevision,
                snapshot.WorldDay,
                status,
                sourceError,
                sourceError,
                sourceReason,
                disabled ? sourceReason : string.Empty,
                sourceLoc,
                MergeMissing(missing, snapshot.MissingModuleIds),
                issues);
            return new D22MarketReadModel(
                metadata,
                snapshot.PortId,
                snapshot.WorldDay,
                snapshot.SaveRevision,
                hasMoney,
                money,
                market == null ? string.Empty : market.Weather.ToString(),
                market == null ? string.Empty : market.FactionPolicyId,
                market == null ? 0f : market.FactionPolicyCoefficient,
                items,
                outcome);
        }

        public static D22MarketReadModel Adapt(D22MarketOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        public static D22MarketReadModel FromSnapshot(D22MarketOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        private static D22MarketReadModel Empty(
            int worldDay,
            long revision,
            D22ReadModelAdapterStatus status,
            List<D22ReadModelAdapterIssue> issues,
            List<string> missing)
        {
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "market", "D3", 0, revision, worldDay, status,
                "d22.market.source_missing", string.Empty, "d22.market.source_missing",
                "d22.market.source_missing", string.Empty, missing, issues);
            return new D22MarketReadModel(
                metadata, string.Empty, worldDay, revision, false, 0, string.Empty, string.Empty, 0f,
                new List<D22MarketItemReadModel>(), null);
        }

        private static void AddDictionaryKeys(
            List<string> target,
            IDictionary<string, int> values,
            List<D22ReadModelAdapterIssue> issues)
        {
            if (values == null) return;
            foreach (var key in values.Keys)
            {
                if (!D22ReadModelAdapterSupport.IsStableId(key))
                {
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_market_key", "d22.market.commodity_id_invalid", D22ReadModelAdapterSupport.Key("market", "partial"),
                        "d22.market.commodity_id_invalid", "D3", "d3.market", false));
                    continue;
                }
                if (!target.Contains(key)) target.Add(key);
            }
        }

        private static List<string> MergeMissing(List<string> current, IEnumerable<string> extra)
        {
            var all = new List<string>();
            if (current != null) all.AddRange(current);
            if (extra != null) all.AddRange(extra);
            return D22ReadModelAdapterSupport.SortedDistinct(all);
        }

        private static int CompareItems(D22MarketItemReadModel left, D22MarketItemReadModel right)
        {
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.CommodityId,
                right == null ? null : right.CommodityId);
        }
    }
}
