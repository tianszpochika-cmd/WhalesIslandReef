// ============================================================
// D21ProvisionConsumptionSystem —— 航程补给账本与固定步消耗（D21-L14）
//
// D21 只拥有航程中的补给账本和消耗裁决。港口购买/装载、D3 货物字典和
// ShipState.Supplies 仍由各自 owner 通过公开命令修改；本系统不持有它们
// 的引用，也不把兼容模型当成第二写者。每个 tick 先完整校验，再原子地
// 计算食物/淡水消耗与损耗；重复应用返回原结果，冲突或短缺不留下部分修改。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>补给 tick 的结算状态。</summary>
    public enum D21ProvisionStatus
    {
        Rejected = 0,
        Consumed = 1,
        HungerWarning = 2,
        ThirstWarning = 3,
        HungerAndThirstWarning = 4,
        VoyageFailed = 5,
        Duplicate = 6
    }

    /// <summary>补给系统的稳定失败码。</summary>
    public static class D21ProvisionFailureCodes
    {
        public const string RequestRequired = "d21.provision.request_required";
        public const string SchemaVersionInvalid = "d21.provision.schema_version_invalid";
        public const string VoyageIdRequired = "d21.provision.voyage_id_required";
        public const string VoyageIdInvalid = "d21.provision.voyage_id_invalid";
        public const string ShipIdRequired = "d21.provision.ship_id_required";
        public const string ApplicationIdRequired = "d21.provision.application_id_required";
        public const string ApplicationIdInvalid = "d21.provision.application_id_invalid";
        public const string WorldDayNegative = "d21.provision.world_day_negative";
        public const string SaveRevisionNegative = "d21.provision.save_revision_negative";
        public const string TickNegative = "d21.provision.tick_negative";
        public const string CrewCountNegative = "d21.provision.crew_count_negative";
        public const string FoodNegative = "d21.provision.food_negative";
        public const string WaterNegative = "d21.provision.water_negative";
        public const string FoodPerCrewInvalid = "d21.provision.food_per_crew_invalid";
        public const string WaterPerCrewInvalid = "d21.provision.water_per_crew_invalid";
        public const string MultiplierInvalid = "d21.provision.multiplier_invalid";
        public const string LossRateInvalid = "d21.provision.loss_rate_invalid";
        public const string LossUnitsNegative = "d21.provision.loss_units_negative";
        public const string ThresholdInvalid = "d21.provision.threshold_invalid";
        public const string WeatherImpactRejected = "d21.provision.weather_impact_rejected";
        public const string ApplicationConflict = "d21.provision.application_conflict";
        public const string DuplicateTick = "d21.provision.duplicate_tick";
        public const string VoyageAlreadyFailed = "d21.provision.voyage_already_failed";
        public const string StockMismatch = "d21.provision.stock_mismatch";
        public const string CalculationOverflow = "d21.provision.calculation_overflow";
        public const string InsufficientFood = "d21.provision.insufficient_food";
        public const string InsufficientWater = "d21.provision.insufficient_water";
        public const string InsufficientFoodAndWater = "d21.provision.insufficient_food_and_water";
        public const string FoodDepleted = "d21.provision.food_depleted";
        public const string WaterDepleted = "d21.provision.water_depleted";
        public const string FoodAndWaterDepleted = "d21.provision.food_and_water_depleted";
    }

    /// <summary>
    /// 补给消耗请求。FoodUnits/WaterUnits 只在该航程第一次应用时作为
    /// 账本初始快照使用；后续 tick 读取本系统账本，避免调用方反复覆盖事实。
    /// </summary>
    [Serializable]
    public sealed class D21ProvisionConsumptionRequest
    {
        public const int CurrentSchemaVersion = 1;
        public const int ReferenceMaxLength = 160;
        public const int ApplicationIdMaxLength = 128;

        public int SchemaVersion = CurrentSchemaVersion;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public string ApplicationId = string.Empty;
        public int WorldDay;
        public long SaveRevision;
        public int TickIndex;
        public int CrewCount;
        public int FoodUnits;
        public int WaterUnits;
        public float FoodUnitsPerCrew = 1f;
        public float WaterUnitsPerCrew = 1f;
        public float FoodConsumptionMultiplier = 1f;
        public float WaterConsumptionMultiplier = 1f;
        public int FoodLossUnits;
        public int WaterLossUnits;
        public float FoodLossRate;
        public float WaterLossRate;
        public int HungerThresholdUnits = 1;
        public int ThirstThresholdUnits = 1;
        public bool EnforceStockMatch;
        public D21WeatherImpactResult WeatherImpact;

        /// <summary>常见命名别名，单位仍为本系统的整数补给单位。</summary>
        public int Food
        {
            get { return FoodUnits; }
            set { FoodUnits = value; }
        }

        /// <summary>常见命名别名，单位仍为本系统的整数补给单位。</summary>
        public int Water
        {
            get { return WaterUnits; }
            set { WaterUnits = value; }
        }

        /// <summary>天气引起的食物损耗比例别名。</summary>
        public float FoodLossFraction
        {
            get { return FoodLossRate; }
            set { FoodLossRate = value; }
        }

        /// <summary>天气引起的淡水损耗比例别名。</summary>
        public float WaterLossFraction
        {
            get { return WaterLossRate; }
            set { WaterLossRate = value; }
        }

        /// <summary>返回请求副本，避免调用方为下一 tick 改写已提交的载荷。</summary>
        public D21ProvisionConsumptionRequest Clone()
        {
            return new D21ProvisionConsumptionRequest
            {
                SchemaVersion = SchemaVersion,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                ApplicationId = ApplicationId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                TickIndex = TickIndex,
                CrewCount = CrewCount,
                FoodUnits = FoodUnits,
                WaterUnits = WaterUnits,
                FoodUnitsPerCrew = FoodUnitsPerCrew,
                WaterUnitsPerCrew = WaterUnitsPerCrew,
                FoodConsumptionMultiplier = FoodConsumptionMultiplier,
                WaterConsumptionMultiplier = WaterConsumptionMultiplier,
                FoodLossUnits = FoodLossUnits,
                WaterLossUnits = WaterLossUnits,
                FoodLossRate = FoodLossRate,
                WaterLossRate = WaterLossRate,
                HungerThresholdUnits = HungerThresholdUnits,
                ThirstThresholdUnits = ThirstThresholdUnits,
                EnforceStockMatch = EnforceStockMatch,
                WeatherImpact = WeatherImpact == null ? null : WeatherImpact.Clone()
            };
        }
    }

    /// <summary>
    /// 已提交 ApplicationId 的不可变持久链。每个新账本状态只追加一个节点，
    /// 旧状态、重放记录和公开快照可以共享此前的历史而无需复制完整列表。
    /// </summary>
    [Serializable]
    internal sealed class D21ProvisionApplicationHistory
    {
        public D21ProvisionApplicationHistory Previous { get; }
        public string ApplicationId { get; }
        public int Count { get; }

        public D21ProvisionApplicationHistory(
            string applicationId,
            D21ProvisionApplicationHistory previous)
        {
            ApplicationId = applicationId ?? string.Empty;
            Previous = previous;
            Count = previous == null ? 1 : previous.Count + 1;
        }
    }

    /// <summary>
    /// D21 航程补给账本只读快照。它不等同于 D3/ShipState 库存，
    /// 外部只能通过下一条经过校验的 tick 请求使本账本前进。
    /// </summary>
    [Serializable]
    public sealed class D21ProvisionLedgerSnapshot
    {
        private readonly D21ProvisionApplicationHistory _appliedApplicationHistory;

        public int SchemaVersion { get; }
        public string VoyageId { get; }
        public string ShipId { get; }
        public int LastTickIndex { get; }
        public int FoodRemaining { get; }
        public int WaterRemaining { get; }
        public int TotalFoodConsumed { get; }
        public int TotalWaterConsumed { get; }
        public int TotalFoodLoss { get; }
        public int TotalWaterLoss { get; }
        public int HungerThresholdUnits { get; }
        public int ThirstThresholdUnits { get; }
        public bool IsFailed { get; }
        public string FailureCode { get; }
        public IReadOnlyList<string> AppliedApplicationIds
        {
            get
            {
                if (_appliedApplicationHistory == null)
                    return new List<string>();

                // The internal chain is newest-first; materialize the public
                // oldest-first order only when a caller explicitly reads it.
                string[] ids = new string[_appliedApplicationHistory.Count];
                D21ProvisionApplicationHistory cursor = _appliedApplicationHistory;
                for (int index = ids.Length - 1; index >= 0; index--)
                {
                    ids[index] = cursor.ApplicationId;
                    cursor = cursor.Previous;
                }
                return new List<string>(ids);
            }
        }

        internal D21ProvisionLedgerSnapshot(
            string voyageId,
            string shipId,
            int lastTickIndex,
            int foodRemaining,
            int waterRemaining,
            int totalFoodConsumed,
            int totalWaterConsumed,
            int totalFoodLoss,
            int totalWaterLoss,
            int hungerThresholdUnits,
            int thirstThresholdUnits,
            bool isFailed,
            string failureCode,
            D21ProvisionApplicationHistory appliedApplicationHistory)
        {
            SchemaVersion = D21ProvisionConsumptionRequest.CurrentSchemaVersion;
            VoyageId = voyageId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            LastTickIndex = lastTickIndex;
            FoodRemaining = Math.Max(0, foodRemaining);
            WaterRemaining = Math.Max(0, waterRemaining);
            TotalFoodConsumed = Math.Max(0, totalFoodConsumed);
            TotalWaterConsumed = Math.Max(0, totalWaterConsumed);
            TotalFoodLoss = Math.Max(0, totalFoodLoss);
            TotalWaterLoss = Math.Max(0, totalWaterLoss);
            HungerThresholdUnits = Math.Max(0, hungerThresholdUnits);
            ThirstThresholdUnits = Math.Max(0, thirstThresholdUnits);
            IsFailed = isFailed;
            FailureCode = failureCode ?? string.Empty;
            _appliedApplicationHistory = appliedApplicationHistory;
        }

        public D21ProvisionLedgerSnapshot Clone()
        {
            return new D21ProvisionLedgerSnapshot(
                VoyageId,
                ShipId,
                LastTickIndex,
                FoodRemaining,
                WaterRemaining,
                TotalFoodConsumed,
                TotalWaterConsumed,
                TotalFoodLoss,
                TotalWaterLoss,
                HungerThresholdUnits,
                ThirstThresholdUnits,
                IsFailed,
                FailureCode,
                _appliedApplicationHistory);
        }
    }

    /// <summary>一次补给 tick 的不可变结果。</summary>
    [Serializable]
    public sealed class D21ProvisionConsumptionResult
    {
        private readonly D21ProvisionLedgerSnapshot _ledger;

        public int SchemaVersion { get; }
        public bool Success { get; }
        public bool Applied { get; }
        public bool IsDuplicate { get; }
        public bool VoyageFailed { get; }
        public string VoyageId { get; }
        public string ShipId { get; }
        public string ApplicationId { get; }
        public int TickIndex { get; }
        public int FoodConsumed { get; }
        public int WaterConsumed { get; }
        public int FoodLoss { get; }
        public int WaterLoss { get; }
        public int FoodAfter { get; }
        public int WaterAfter { get; }
        public bool HungerThresholdReached { get; }
        public bool ThirstThresholdReached { get; }
        public string FailureCode { get; }
        public string FailureReason { get { return FailureCode; } }
        public D21ProvisionStatus Status { get; }
        public D21ProvisionLedgerSnapshot Ledger
        {
            get { return _ledger == null ? null : _ledger.Clone(); }
        }

        private D21ProvisionConsumptionResult(
            bool success,
            bool applied,
            bool duplicate,
            bool voyageFailed,
            string voyageId,
            string shipId,
            string applicationId,
            int tickIndex,
            int foodConsumed,
            int waterConsumed,
            int foodLoss,
            int waterLoss,
            int foodAfter,
            int waterAfter,
            bool hungerThresholdReached,
            bool thirstThresholdReached,
            string failureCode,
            D21ProvisionStatus status,
            D21ProvisionLedgerSnapshot ledger)
        {
            SchemaVersion = D21ProvisionConsumptionRequest.CurrentSchemaVersion;
            Success = success;
            Applied = applied;
            IsDuplicate = duplicate;
            VoyageFailed = voyageFailed;
            VoyageId = voyageId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            ApplicationId = applicationId ?? string.Empty;
            TickIndex = tickIndex;
            FoodConsumed = Math.Max(0, foodConsumed);
            WaterConsumed = Math.Max(0, waterConsumed);
            FoodLoss = Math.Max(0, foodLoss);
            WaterLoss = Math.Max(0, waterLoss);
            FoodAfter = Math.Max(0, foodAfter);
            WaterAfter = Math.Max(0, waterAfter);
            HungerThresholdReached = hungerThresholdReached;
            ThirstThresholdReached = thirstThresholdReached;
            FailureCode = failureCode ?? string.Empty;
            Status = status;
            // D21ProvisionLedgerSnapshot is immutable; its persistent history
            // pointer is safe to share while Ledger still returns a new wrapper.
            _ledger = ledger;
        }

        internal static D21ProvisionConsumptionResult Create(
            D21ProvisionConsumptionRequest request,
            bool success,
            bool applied,
            bool duplicate,
            bool voyageFailed,
            int foodConsumed,
            int waterConsumed,
            int foodLoss,
            int waterLoss,
            int foodAfter,
            int waterAfter,
            bool hungerThresholdReached,
            bool thirstThresholdReached,
            string failureCode,
            D21ProvisionStatus status,
            D21ProvisionLedgerSnapshot ledger)
        {
            return new D21ProvisionConsumptionResult(
                success,
                applied,
                duplicate,
                voyageFailed,
                request == null ? string.Empty : request.VoyageId,
                request == null ? string.Empty : request.ShipId,
                request == null ? string.Empty : request.ApplicationId,
                request == null ? 0 : request.TickIndex,
                foodConsumed,
                waterConsumed,
                foodLoss,
                waterLoss,
                foodAfter,
                waterAfter,
                hungerThresholdReached,
                thirstThresholdReached,
                failureCode,
                status,
                ledger);
        }

        internal static D21ProvisionConsumptionResult Rejected(
            D21ProvisionConsumptionRequest request,
            string failureCode,
            D21ProvisionLedgerSnapshot ledger = null)
        {
            return Create(
                request,
                false,
                false,
                false,
                false,
                0,
                0,
                0,
                0,
                ledger == null ? 0 : ledger.FoodRemaining,
                ledger == null ? 0 : ledger.WaterRemaining,
                false,
                false,
                failureCode,
                D21ProvisionStatus.Rejected,
                ledger);
        }
    }

    /// <summary>
    /// D21-L14 补给消耗 owner。系统实例持有单一航程账本，不使用静态可变状态。
    /// </summary>
    public sealed class D21ProvisionConsumptionSystem
    {
        private const int MaxReferenceLength = D21ProvisionConsumptionRequest.ReferenceMaxLength;
        private const int MaxApplicationIdLength = D21ProvisionConsumptionRequest.ApplicationIdMaxLength;
        private const int MaxCrewCount = 1000000;
        private const int MaxStockUnits = 2000000000;
        private const int MaxThresholdUnits = 2000000000;

        private sealed class LedgerState
        {
            public string VoyageId = string.Empty;
            public string ShipId = string.Empty;
            public int LastTickIndex = -1;
            public int FoodRemaining;
            public int WaterRemaining;
            public int TotalFoodConsumed;
            public int TotalWaterConsumed;
            public int TotalFoodLoss;
            public int TotalWaterLoss;
            public int HungerThresholdUnits;
            public int ThirstThresholdUnits;
            public bool IsFailed;
            public string FailureCode = string.Empty;
            public D21ProvisionApplicationHistory AppliedApplicationHistory;

            public LedgerState Clone()
            {
                var result = new LedgerState
                {
                    VoyageId = VoyageId,
                    ShipId = ShipId,
                    LastTickIndex = LastTickIndex,
                    FoodRemaining = FoodRemaining,
                    WaterRemaining = WaterRemaining,
                    TotalFoodConsumed = TotalFoodConsumed,
                    TotalWaterConsumed = TotalWaterConsumed,
                    TotalFoodLoss = TotalFoodLoss,
                    TotalWaterLoss = TotalWaterLoss,
                    HungerThresholdUnits = HungerThresholdUnits,
                    ThirstThresholdUnits = ThirstThresholdUnits,
                    IsFailed = IsFailed,
                    FailureCode = FailureCode,
                    AppliedApplicationHistory = AppliedApplicationHistory,
                };
                return result;
            }

            public D21ProvisionLedgerSnapshot Snapshot()
            {
                return new D21ProvisionLedgerSnapshot(
                    VoyageId,
                    ShipId,
                    LastTickIndex,
                    FoodRemaining,
                    WaterRemaining,
                    TotalFoodConsumed,
                    TotalWaterConsumed,
                    TotalFoodLoss,
                    TotalWaterLoss,
                    HungerThresholdUnits,
                    ThirstThresholdUnits,
                    IsFailed,
                    FailureCode,
                    AppliedApplicationHistory);
            }
        }

        /// <summary>
        /// 每个应用只保存重放所需的标量与不可变账本快照；不保存该 tick
        /// 之前的 ApplicationId 列表副本。
        /// </summary>
        private sealed class ApplicationReplayRecord
        {
            public bool Success;
            public bool VoyageFailed;
            public int FoodConsumed;
            public int WaterConsumed;
            public int FoodLoss;
            public int WaterLoss;
            public int FoodAfter;
            public int WaterAfter;
            public bool HungerThresholdReached;
            public bool ThirstThresholdReached;
            public string FailureCode = string.Empty;
            public D21ProvisionLedgerSnapshot Ledger;

            public static ApplicationReplayRecord From(
                D21ProvisionConsumptionResult result)
            {
                return new ApplicationReplayRecord
                {
                    Success = result.Success,
                    VoyageFailed = result.VoyageFailed,
                    FoodConsumed = result.FoodConsumed,
                    WaterConsumed = result.WaterConsumed,
                    FoodLoss = result.FoodLoss,
                    WaterLoss = result.WaterLoss,
                    FoodAfter = result.FoodAfter,
                    WaterAfter = result.WaterAfter,
                    HungerThresholdReached = result.HungerThresholdReached,
                    ThirstThresholdReached = result.ThirstThresholdReached,
                    FailureCode = result.FailureCode,
                    Ledger = result.Ledger,
                };
            }

            public D21ProvisionConsumptionResult Replay(
                D21ProvisionConsumptionRequest request)
            {
                return D21ProvisionConsumptionResult.Create(
                    request,
                    Success,
                    false,
                    true,
                    VoyageFailed,
                    FoodConsumed,
                    WaterConsumed,
                    FoodLoss,
                    WaterLoss,
                    FoodAfter,
                    WaterAfter,
                    HungerThresholdReached,
                    ThirstThresholdReached,
                    FailureCode,
                    D21ProvisionStatus.Duplicate,
                    Ledger);
            }
        }

        private readonly Dictionary<string, LedgerState> _ledgers =
            new Dictionary<string, LedgerState>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _applicationFingerprints =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, ApplicationReplayRecord> _applicationResults =
            new Dictionary<string, ApplicationReplayRecord>(StringComparer.Ordinal);

        /// <summary>
        /// 原子应用一个显式 tick。短缺/冲突/非法输入均不改变该航程账本。
        /// </summary>
        public D21ProvisionConsumptionResult Consume(D21ProvisionConsumptionRequest request)
        {
            string validationFailure = Validate(request);
            if (!string.IsNullOrEmpty(validationFailure))
            {
                return D21ProvisionConsumptionResult.Rejected(request, validationFailure);
            }

            string ledgerKey = request.VoyageId;
            string applicationKey = ledgerKey + "|" + request.ApplicationId;
            LedgerState current;
            _ledgers.TryGetValue(ledgerKey, out current);

            int startingFood = current == null ? request.FoodUnits : current.FoodRemaining;
            int startingWater = current == null ? request.WaterUnits : current.WaterRemaining;
            string fingerprint = BuildFingerprint(request);

            ApplicationReplayRecord previousResult;
            string previousFingerprint;
            if (_applicationFingerprints.TryGetValue(applicationKey, out previousFingerprint))
            {
                if (!string.Equals(previousFingerprint, fingerprint, StringComparison.Ordinal))
                {
                    return D21ProvisionConsumptionResult.Rejected(
                        request,
                        D21ProvisionFailureCodes.ApplicationConflict,
                        current == null ? null : current.Snapshot());
                }

                if (_applicationResults.TryGetValue(applicationKey, out previousResult))
                    return previousResult.Replay(request);
            }

            if (current != null && current.IsFailed)
            {
                return D21ProvisionConsumptionResult.Rejected(
                    request,
                    D21ProvisionFailureCodes.VoyageAlreadyFailed,
                    current.Snapshot());
            }

            if (current != null)
            {
                if (request.TickIndex <= current.LastTickIndex)
                {
                    return D21ProvisionConsumptionResult.Rejected(
                        request,
                        D21ProvisionFailureCodes.DuplicateTick,
                        current.Snapshot());
                }

                if (request.EnforceStockMatch &&
                    (request.FoodUnits != current.FoodRemaining || request.WaterUnits != current.WaterRemaining))
                {
                    return D21ProvisionConsumptionResult.Rejected(
                        request,
                        D21ProvisionFailureCodes.StockMismatch,
                        current.Snapshot());
                }
            }

            int foodConsumed;
            int waterConsumed;
            int foodLoss;
            int waterLoss;
            string calculationFailure;
            if (!TryCalculateUnits(
                    startingFood,
                    startingWater,
                    request,
                    out foodConsumed,
                    out waterConsumed,
                    out foodLoss,
                    out waterLoss,
                    out calculationFailure))
            {
                return D21ProvisionConsumptionResult.Rejected(
                    request,
                    calculationFailure,
                    current == null ? null : current.Snapshot());
            }

            bool foodInsufficient = (long)startingFood < (long)foodConsumed + foodLoss;
            bool waterInsufficient = (long)startingWater < (long)waterConsumed + waterLoss;
            if (foodInsufficient || waterInsufficient)
            {
                string shortageCode = foodInsufficient && waterInsufficient
                    ? D21ProvisionFailureCodes.InsufficientFoodAndWater
                    : (foodInsufficient
                        ? D21ProvisionFailureCodes.InsufficientFood
                        : D21ProvisionFailureCodes.InsufficientWater);
                return D21ProvisionConsumptionResult.Rejected(
                    request,
                    shortageCode,
                    current == null ? null : current.Snapshot());
            }

            int foodAfter = startingFood - foodConsumed - foodLoss;
            int waterAfter = startingWater - waterConsumed - waterLoss;
            bool hungerReached = foodAfter <= request.HungerThresholdUnits;
            bool thirstReached = waterAfter <= request.ThirstThresholdUnits;
            bool depletedFood = foodAfter == 0;
            bool depletedWater = waterAfter == 0;
            bool voyageFailed = depletedFood || depletedWater;
            string failureCode = voyageFailed ? DepletionCode(depletedFood, depletedWater) : string.Empty;
            D21ProvisionStatus status = voyageFailed
                ? D21ProvisionStatus.VoyageFailed
                : (hungerReached && thirstReached
                    ? D21ProvisionStatus.HungerAndThirstWarning
                    : (hungerReached
                        ? D21ProvisionStatus.HungerWarning
                        : (thirstReached ? D21ProvisionStatus.ThirstWarning : D21ProvisionStatus.Consumed)));

            LedgerState next = current == null ? new LedgerState() : current.Clone();
            next.VoyageId = request.VoyageId;
            next.ShipId = request.ShipId;
            next.LastTickIndex = request.TickIndex;
            next.FoodRemaining = foodAfter;
            next.WaterRemaining = waterAfter;
            next.TotalFoodConsumed = SafeAdd(next.TotalFoodConsumed, foodConsumed);
            next.TotalWaterConsumed = SafeAdd(next.TotalWaterConsumed, waterConsumed);
            next.TotalFoodLoss = SafeAdd(next.TotalFoodLoss, foodLoss);
            next.TotalWaterLoss = SafeAdd(next.TotalWaterLoss, waterLoss);
            next.HungerThresholdUnits = request.HungerThresholdUnits;
            next.ThirstThresholdUnits = request.ThirstThresholdUnits;
            next.IsFailed = voyageFailed;
            next.FailureCode = failureCode;
            next.AppliedApplicationHistory = new D21ProvisionApplicationHistory(
                request.ApplicationId,
                next.AppliedApplicationHistory);

            D21ProvisionConsumptionResult result = D21ProvisionConsumptionResult.Create(
                request,
                !voyageFailed,
                true,
                false,
                voyageFailed,
                foodConsumed,
                waterConsumed,
                foodLoss,
                waterLoss,
                foodAfter,
                waterAfter,
                hungerReached,
                thirstReached,
                failureCode,
                status,
                next.Snapshot());

            _ledgers[ledgerKey] = next;
            _applicationFingerprints[applicationKey] = fingerprint;
            _applicationResults[applicationKey] = ApplicationReplayRecord.From(result);
            return result;
        }

        /// <summary>语义别名，供航程解析器调用。</summary>
        public D21ProvisionConsumptionResult Apply(D21ProvisionConsumptionRequest request)
        {
            return Consume(request);
        }

        /// <summary>语义别名，强调固定步结算。</summary>
        public D21ProvisionConsumptionResult ConsumeTick(D21ProvisionConsumptionRequest request)
        {
            return Consume(request);
        }

        /// <summary>Try 风格入口；结果始终保留稳定失败/耗尽原因。</summary>
        public bool TryConsume(
            D21ProvisionConsumptionRequest request,
            out D21ProvisionConsumptionResult result)
        {
            result = Consume(request);
            return result != null && result.Success;
        }

        /// <summary>读取某一航程的防御性账本快照。</summary>
        public D21ProvisionLedgerSnapshot GetSnapshot(string voyageId)
        {
            if (string.IsNullOrEmpty(voyageId)) return null;
            LedgerState state;
            return _ledgers.TryGetValue(voyageId, out state) ? state.Snapshot() : null;
        }

        private static string Validate(D21ProvisionConsumptionRequest request)
        {
            if (request == null) return D21ProvisionFailureCodes.RequestRequired;
            if (request.SchemaVersion != D21ProvisionConsumptionRequest.CurrentSchemaVersion)
                return D21ProvisionFailureCodes.SchemaVersionInvalid;
            if (!IsStableReference(request.VoyageId, MaxReferenceLength))
                return string.IsNullOrWhiteSpace(request.VoyageId)
                    ? D21ProvisionFailureCodes.VoyageIdRequired
                    : D21ProvisionFailureCodes.VoyageIdInvalid;
            if (!IsStableReference(request.ShipId, MaxReferenceLength))
                return D21ProvisionFailureCodes.ShipIdRequired;
            if (!IsStableReference(request.ApplicationId, MaxApplicationIdLength))
                return string.IsNullOrWhiteSpace(request.ApplicationId)
                    ? D21ProvisionFailureCodes.ApplicationIdRequired
                    : D21ProvisionFailureCodes.ApplicationIdInvalid;
            if (request.WorldDay < 0) return D21ProvisionFailureCodes.WorldDayNegative;
            if (request.SaveRevision < 0) return D21ProvisionFailureCodes.SaveRevisionNegative;
            if (request.TickIndex < 0) return D21ProvisionFailureCodes.TickNegative;
            if (request.CrewCount < 0 || request.CrewCount > MaxCrewCount)
                return D21ProvisionFailureCodes.CrewCountNegative;
            if (request.FoodUnits < 0 || request.FoodUnits > MaxStockUnits)
                return D21ProvisionFailureCodes.FoodNegative;
            if (request.WaterUnits < 0 || request.WaterUnits > MaxStockUnits)
                return D21ProvisionFailureCodes.WaterNegative;
            if (!IsFiniteNonNegative(request.FoodUnitsPerCrew) || request.FoodUnitsPerCrew > 100f)
                return D21ProvisionFailureCodes.FoodPerCrewInvalid;
            if (!IsFiniteNonNegative(request.WaterUnitsPerCrew) || request.WaterUnitsPerCrew > 100f)
                return D21ProvisionFailureCodes.WaterPerCrewInvalid;
            if (!IsFiniteMultiplier(request.FoodConsumptionMultiplier) ||
                !IsFiniteMultiplier(request.WaterConsumptionMultiplier))
                return D21ProvisionFailureCodes.MultiplierInvalid;
            if (!IsFiniteRate(request.FoodLossRate) || !IsFiniteRate(request.WaterLossRate))
                return D21ProvisionFailureCodes.LossRateInvalid;
            if (request.FoodLossUnits < 0 || request.WaterLossUnits < 0)
                return D21ProvisionFailureCodes.LossUnitsNegative;
            if (request.HungerThresholdUnits < 0 || request.HungerThresholdUnits > MaxThresholdUnits ||
                request.ThirstThresholdUnits < 0 || request.ThirstThresholdUnits > MaxThresholdUnits)
                return D21ProvisionFailureCodes.ThresholdInvalid;
            if (request.WeatherImpact != null && !request.WeatherImpact.Success)
                return D21ProvisionFailureCodes.WeatherImpactRejected;
            return string.Empty;
        }

        private static bool TryCalculateUnits(
            int foodBefore,
            int waterBefore,
            D21ProvisionConsumptionRequest request,
            out int foodConsumed,
            out int waterConsumed,
            out int foodLoss,
            out int waterLoss,
            out string failureCode)
        {
            foodConsumed = 0;
            waterConsumed = 0;
            foodLoss = 0;
            waterLoss = 0;
            failureCode = string.Empty;

            float foodMultiplier = request.FoodConsumptionMultiplier;
            float waterMultiplier = request.WaterConsumptionMultiplier;
            if (request.WeatherImpact != null)
            {
                foodMultiplier *= request.WeatherImpact.FoodConsumptionMultiplier;
                waterMultiplier *= request.WeatherImpact.WaterConsumptionMultiplier;
            }

            if (!TryCeilingToInt((double)request.CrewCount * request.FoodUnitsPerCrew * foodMultiplier, out foodConsumed) ||
                !TryCeilingToInt((double)request.CrewCount * request.WaterUnitsPerCrew * waterMultiplier, out waterConsumed) ||
                !TryCeilingToInt(foodBefore * (double)request.FoodLossRate, out foodLoss) ||
                !TryCeilingToInt(waterBefore * (double)request.WaterLossRate, out waterLoss))
            {
                failureCode = D21ProvisionFailureCodes.CalculationOverflow;
                return false;
            }

            if (!TryAddToInt(foodLoss, request.FoodLossUnits, out foodLoss) ||
                !TryAddToInt(waterLoss, request.WaterLossUnits, out waterLoss))
            {
                failureCode = D21ProvisionFailureCodes.CalculationOverflow;
                return false;
            }

            return true;
        }

        private static string DepletionCode(bool food, bool water)
        {
            if (food && water) return D21ProvisionFailureCodes.FoodAndWaterDepleted;
            return food ? D21ProvisionFailureCodes.FoodDepleted : D21ProvisionFailureCodes.WaterDepleted;
        }

        private static bool IsStableReference(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maxLength || value.Trim() != value)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool letter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == ':' || c == '_' || c == '-' || c == '.';
                if (!letter && !digit && !separator) return false;
            }

            return true;
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
        }

        private static bool IsFiniteMultiplier(float value)
        {
            return IsFiniteNonNegative(value) && value <= 8f;
        }

        private static bool IsFiniteRate(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f && value <= 1f;
        }

        private static bool TryCeilingToInt(double value, out int result)
        {
            result = 0;
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d || value > int.MaxValue)
                return false;
            double ceiling = Math.Ceiling(value);
            if (ceiling > int.MaxValue) return false;
            result = (int)ceiling;
            return true;
        }

        private static bool TryAddToInt(int left, int right, out int result)
        {
            long sum = (long)left + right;
            if (sum < 0L || sum > int.MaxValue)
            {
                result = 0;
                return false;
            }

            result = (int)sum;
            return true;
        }

        private static int SafeAdd(int left, int right)
        {
            long sum = (long)Math.Max(0, left) + Math.Max(0, right);
            return sum > int.MaxValue ? int.MaxValue : (int)sum;
        }

        private static string BuildFingerprint(D21ProvisionConsumptionRequest request)
        {
            return string.Join("|", new string[]
            {
                request.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                request.VoyageId ?? string.Empty,
                request.ShipId ?? string.Empty,
                request.ApplicationId ?? string.Empty,
                request.WorldDay.ToString(CultureInfo.InvariantCulture),
                request.SaveRevision.ToString(CultureInfo.InvariantCulture),
                request.TickIndex.ToString(CultureInfo.InvariantCulture),
                request.CrewCount.ToString(CultureInfo.InvariantCulture),
                request.FoodUnits.ToString(CultureInfo.InvariantCulture),
                request.WaterUnits.ToString(CultureInfo.InvariantCulture),
                request.FoodUnitsPerCrew.ToString("R", CultureInfo.InvariantCulture),
                request.WaterUnitsPerCrew.ToString("R", CultureInfo.InvariantCulture),
                request.FoodConsumptionMultiplier.ToString("R", CultureInfo.InvariantCulture),
                request.WaterConsumptionMultiplier.ToString("R", CultureInfo.InvariantCulture),
                request.FoodLossUnits.ToString(CultureInfo.InvariantCulture),
                request.WaterLossUnits.ToString(CultureInfo.InvariantCulture),
                request.FoodLossRate.ToString("R", CultureInfo.InvariantCulture),
                request.WaterLossRate.ToString("R", CultureInfo.InvariantCulture),
                request.HungerThresholdUnits.ToString(CultureInfo.InvariantCulture),
                request.ThirstThresholdUnits.ToString(CultureInfo.InvariantCulture),
                request.EnforceStockMatch.ToString(CultureInfo.InvariantCulture),
                request.WeatherImpact == null ? string.Empty : request.WeatherImpact.InputFingerprint
            });
        }
    }
}
