// ============================================================
// D21CargoCapacityRules —— D21-L05 货舱容量、危险货物与超载后果（纯 C#）
//
// 本文件只读取货物行、容量和船侧储运条件并给出确定性判定。货物数量仍由
// D3InventorySystem 唯一写入；这里输出可交给 D3 的只读 mutation request。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>货物的储运危险类别。</summary>
    public enum D21CargoDangerClass
    {
        /// <summary>普通货物或未分类货物。</summary>
        None = 0,
        /// <summary>食品。</summary>
        Food = 1,
        /// <summary>可燃货物。</summary>
        Flammable = 2,
        /// <summary>爆炸物或火药。</summary>
        Explosive = 3,
        /// <summary>有毒货物。</summary>
        Toxic = 4,
        /// <summary>活体货物。</summary>
        Live = 5,
        /// <summary>腐蚀性货物。</summary>
        Corrosive = 6,
    }

    /// <summary>货物数量变化意图；不在 D21 中直接写入数量。</summary>
    public enum D21CargoMutationKind
    {
        /// <summary>未初始化。</summary>
        None = 0,
        /// <summary>请求增加货物。</summary>
        Add = 1,
        /// <summary>请求移除货物。</summary>
        Remove = 2,
    }

    /// <summary>超载比例对应的航行后果。</summary>
    public enum D21OverloadConsequence
    {
        /// <summary>未超载。</summary>
        None = 0,
        /// <summary>轻度超载，降低速度。</summary>
        ReducedSpeed = 1,
        /// <summary>中度超载，限制操纵。</summary>
        RestrictedManeuver = 2,
        /// <summary>严重超载，吃水风险过高。</summary>
        UnsafeDraft = 3,
        /// <summary>超载超过出航阈值。</summary>
        DepartureRefused = 4,
    }

    /// <summary>货舱规则校验或评估失败的稳定原因码。</summary>
    public enum D21CargoCapacityFailureCode
    {
        /// <summary>没有错误。</summary>
        None = 0,
        /// <summary>请求缺失。</summary>
        InvalidRequest = 1,
        /// <summary>容量快照无效。</summary>
        InvalidSnapshot = 2,
        /// <summary>货物行无效。</summary>
        InvalidLine = 3,
        /// <summary>货物类别不受支持。</summary>
        UnsupportedDangerClass = 4,
        /// <summary>货物数量不足以执行移除。</summary>
        InsufficientQuantity = 5,
        /// <summary>容量或总量计算溢出。</summary>
        CapacityOverflow = 6,
        /// <summary>货物超过容量。</summary>
        CapacityExceeded = 7,
        /// <summary>危险货物隔离条件不满足。</summary>
        DangerousGoodsConflict = 8,
        /// <summary>船只 ID 不匹配。</summary>
        ShipMismatch = 9,
        /// <summary>保存版本无效。</summary>
        InvalidSaveRevision = 10,
        /// <summary>owner mutation request 无效。</summary>
        InvalidOwnerRequest = 11,
    }

    /// <summary>一条货物行的只读逻辑快照。</summary>
    public sealed class D21CargoLineSnapshot
    {
        /// <summary>创建空货物行。</summary>
        public D21CargoLineSnapshot()
        {
            DangerClass = D21CargoDangerClass.None;
        }

        /// <summary>货物或商品的稳定 ID。</summary>
        public string ItemId { get; set; }

        /// <summary>货物数量。</summary>
        public int Quantity { get; set; }

        /// <summary>单件重量。</summary>
        public double UnitWeight { get; set; }

        /// <summary>单件体积或占用容量。</summary>
        public double UnitVolume { get; set; }

        /// <summary>危险货物类别。</summary>
        public D21CargoDangerClass DangerClass { get; set; }

        /// <summary>货物所在隔离舱的稳定 ID；普通货物可为空。</summary>
        public string StorageCompartmentId { get; set; }

        /// <summary>计算该行总重量。</summary>
        public double TotalWeight()
        {
            return Quantity * UnitWeight;
        }

        /// <summary>计算该行总体积。</summary>
        public double TotalVolume()
        {
            return Quantity * UnitVolume;
        }

        /// <summary>复制货物行。</summary>
        public D21CargoLineSnapshot Clone()
        {
            return new D21CargoLineSnapshot
            {
                ItemId = ItemId,
                Quantity = Quantity,
                UnitWeight = UnitWeight,
                UnitVolume = UnitVolume,
                DangerClass = DangerClass,
                StorageCompartmentId = StorageCompartmentId,
            };
        }
    }

    /// <summary>船侧危险货物储运条件的只读快照。</summary>
    public sealed class D21CargoStorageContext
    {
        /// <summary>是否有防火隔离。</summary>
        public bool HasFireSeparation { get; set; }

        /// <summary>是否有独立火药舱。</summary>
        public bool HasPowderMagazine { get; set; }

        /// <summary>是否有足够通风。</summary>
        public bool HasVentilation { get; set; }

        /// <summary>是否有活体货物舱。</summary>
        public bool HasLiveHold { get; set; }

        /// <summary>复制储运条件。</summary>
        public D21CargoStorageContext Clone()
        {
            return new D21CargoStorageContext
            {
                HasFireSeparation = HasFireSeparation,
                HasPowderMagazine = HasPowderMagazine,
                HasVentilation = HasVentilation,
                HasLiveHold = HasLiveHold,
            };
        }
    }

    /// <summary>容量 owner 的只读输入快照。</summary>
    public sealed class D21CargoCapacitySnapshot
    {
        /// <summary>创建空容量快照。</summary>
        public D21CargoCapacitySnapshot()
        {
            SchemaVersion = D21CargoCapacityRules.CurrentSchemaVersion;
            ExistingLines = new List<D21CargoLineSnapshot>();
            StorageContext = new D21CargoStorageContext();
        }

        /// <summary>容量契约版本。</summary>
        public int SchemaVersion { get; set; }

        /// <summary>船只稳定 ID。</summary>
        public string ShipId { get; set; }

        /// <summary>最大体积容量。</summary>
        public double MaxVolume { get; set; }

        /// <summary>最大重量容量。</summary>
        public double MaxWeight { get; set; }

        /// <summary>兼容外部命名的体积容量别名。</summary>
        public double CapacityVolume
        {
            get { return MaxVolume; }
            set { MaxVolume = value; }
        }

        /// <summary>兼容外部命名的重量容量别名。</summary>
        public double CapacityWeight
        {
            get { return MaxWeight; }
            set { MaxWeight = value; }
        }

        /// <summary>当前货物行。</summary>
        public List<D21CargoLineSnapshot> ExistingLines { get; set; }

        /// <summary>船侧储运条件。</summary>
        public D21CargoStorageContext StorageContext { get; set; }

        /// <summary>容量快照保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>复制容量快照。</summary>
        public D21CargoCapacitySnapshot Clone()
        {
            var clone = new D21CargoCapacitySnapshot
            {
                SchemaVersion = SchemaVersion,
                ShipId = ShipId,
                MaxVolume = MaxVolume,
                MaxWeight = MaxWeight,
                StorageContext = StorageContext == null ? null : StorageContext.Clone(),
                SaveRevision = SaveRevision,
            };
            if (ExistingLines != null)
            {
                for (var index = 0; index < ExistingLines.Count; index++)
                {
                    if (ExistingLines[index] != null)
                    {
                        clone.ExistingLines.Add(ExistingLines[index].Clone());
                    }
                }
            }
            return clone;
        }
    }

    /// <summary>货物容量规则的评估请求。</summary>
    public sealed class D21CargoCapacityEvaluationRequest
    {
        /// <summary>创建增加货物的评估请求。</summary>
        public D21CargoCapacityEvaluationRequest()
        {
            MutationKind = D21CargoMutationKind.Add;
        }

        /// <summary>容量与现有货物快照。</summary>
        public D21CargoCapacitySnapshot Snapshot { get; set; }

        /// <summary>拟增加或移除的货物行。</summary>
        public D21CargoLineSnapshot Candidate { get; set; }

        /// <summary>数量变化意图。</summary>
        public D21CargoMutationKind MutationKind { get; set; }

        /// <summary>可选的调用方请求 ID。</summary>
        public string RequestId { get; set; }

        /// <summary>可选的触发事实 ID。</summary>
        public string CauseEventId { get; set; }

        /// <summary>复制评估请求。</summary>
        public D21CargoCapacityEvaluationRequest Clone()
        {
            return new D21CargoCapacityEvaluationRequest
            {
                Snapshot = Snapshot == null ? null : Snapshot.Clone(),
                Candidate = Candidate == null ? null : Candidate.Clone(),
                MutationKind = MutationKind,
                RequestId = RequestId,
                CauseEventId = CauseEventId,
            };
        }
    }

    /// <summary>容量校验结果。</summary>
    public sealed class D21CargoCapacityValidationResult
    {
        /// <summary>创建容量校验结果。</summary>
        public D21CargoCapacityValidationResult(bool valid, D21CargoCapacityFailureCode code, IEnumerable<string> errors)
        {
            Valid = valid;
            Code = code;
            Errors = new ReadOnlyCollection<string>(new List<string>(errors ?? new string[0]));
        }

        /// <summary>是否通过校验。</summary>
        public bool Valid { get; private set; }

        /// <summary>首个稳定原因码。</summary>
        public D21CargoCapacityFailureCode Code { get; private set; }

        /// <summary>稳定错误码集合。</summary>
        public ReadOnlyCollection<string> Errors { get; private set; }

        /// <summary>复制校验结果。</summary>
        public D21CargoCapacityValidationResult Clone()
        {
            return new D21CargoCapacityValidationResult(Valid, Code, Errors);
        }
    }

    /// <summary>提供给 D3 数量 owner 的只读货物 mutation request。</summary>
    public sealed class D21CargoMutationRequest
    {
        /// <summary>请求 ID。</summary>
        public string RequestId { get; set; }

        /// <summary>目标船只 ID。</summary>
        public string ShipId { get; set; }

        /// <summary>数量变化意图。</summary>
        public D21CargoMutationKind MutationKind { get; set; }

        /// <summary>变化货物行。</summary>
        public D21CargoLineSnapshot Line { get; set; }

        /// <summary>调用方读取到的保存版本。</summary>
        public int ExpectedSaveRevision { get; set; }

        /// <summary>触发请求的事件 ID。</summary>
        public string CauseEventId { get; set; }

        /// <summary>复制 mutation request。</summary>
        public D21CargoMutationRequest Clone()
        {
            return new D21CargoMutationRequest
            {
                RequestId = RequestId,
                ShipId = ShipId,
                MutationKind = MutationKind,
                Line = Line == null ? null : Line.Clone(),
                ExpectedSaveRevision = ExpectedSaveRevision,
                CauseEventId = CauseEventId,
            };
        }
    }

    /// <summary>货舱容量、危险货物和超载后果的确定性规则。</summary>
    public sealed class D21CargoCapacityDecision
    {
        /// <summary>是否完成了有效评估。</summary>
        public bool Valid { get; set; }

        /// <summary>数量 owner 是否可以接受该变化。</summary>
        public bool Allowed { get; set; }

        /// <summary>评估后是否存在超载。</summary>
        public bool IsOverloaded { get; set; }

        /// <summary>在该状态下是否允许出航。</summary>
        public bool DepartureAllowed { get; set; }

        /// <summary>现有货物总重量。</summary>
        public double UsedWeight { get; set; }

        /// <summary>现有货物总体积。</summary>
        public double UsedVolume { get; set; }

        /// <summary>评估后的总重量。</summary>
        public double ProjectedWeight { get; set; }

        /// <summary>评估后的总体积。</summary>
        public double ProjectedVolume { get; set; }

        /// <summary>重量容量剩余量，可为负数以表达超载。</summary>
        public double RemainingWeight { get; set; }

        /// <summary>体积容量剩余量，可为负数以表达超载。</summary>
        public double RemainingVolume { get; set; }

        /// <summary>超出容量的最大比例。</summary>
        public double OverloadRatio { get; set; }

        /// <summary>确定性超载后果。</summary>
        public D21OverloadConsequence OverloadConsequence { get; set; }

        /// <summary>航行速度系数，范围为 0 到 1。</summary>
        public double NavigationSpeedMultiplier { get; set; }

        /// <summary>首个稳定原因码。</summary>
        public D21CargoCapacityFailureCode ReasonCode { get; set; }

        /// <summary>稳定错误码集合。</summary>
        public ReadOnlyCollection<string> Errors { get; set; }

        /// <summary>危险货物冲突集合。</summary>
        public ReadOnlyCollection<string> DangerousGoodsConflicts { get; set; }

        /// <summary>创建空决策。</summary>
        public D21CargoCapacityDecision()
        {
            ReasonCode = D21CargoCapacityFailureCode.None;
            Errors = new ReadOnlyCollection<string>(new List<string>());
            DangerousGoodsConflicts = new ReadOnlyCollection<string>(new List<string>());
        }

        /// <summary>复制容量决策。</summary>
        public D21CargoCapacityDecision Clone()
        {
            return new D21CargoCapacityDecision
            {
                Valid = Valid,
                Allowed = Allowed,
                IsOverloaded = IsOverloaded,
                DepartureAllowed = DepartureAllowed,
                UsedWeight = UsedWeight,
                UsedVolume = UsedVolume,
                ProjectedWeight = ProjectedWeight,
                ProjectedVolume = ProjectedVolume,
                RemainingWeight = RemainingWeight,
                RemainingVolume = RemainingVolume,
                OverloadRatio = OverloadRatio,
                OverloadConsequence = OverloadConsequence,
                NavigationSpeedMultiplier = NavigationSpeedMultiplier,
                ReasonCode = ReasonCode,
                Errors = new ReadOnlyCollection<string>(Errors == null ? new List<string>() : new List<string>(Errors)),
                DangerousGoodsConflicts = new ReadOnlyCollection<string>(DangerousGoodsConflicts == null ? new List<string>() : new List<string>(DangerousGoodsConflicts)),
            };
        }
    }

    /// <summary>货舱容量规则 owner；不写入 D3 库存数量。</summary>
    public static class D21CargoCapacityRules
    {
        /// <summary>当前 D21-L05 货舱规则版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>容量规则的 owner 名称。</summary>
        public const string OwnerSystem = "D21CargoCapacityRules";

        /// <summary>校验一条货物行。</summary>
        public static D21CargoCapacityValidationResult ValidateLine(D21CargoLineSnapshot line)
        {
            var errors = new List<string>();
            if (line == null)
            {
                errors.Add("cargo_line_missing");
                return new D21CargoCapacityValidationResult(false, D21CargoCapacityFailureCode.InvalidLine, errors);
            }
            if (!D21ShipIdentityContracts.IsStableId(line.ItemId, 128) || line.Quantity <= 0)
            {
                errors.Add("cargo_line_identity_or_quantity_invalid");
            }
            if (!IsFiniteNonNegative(line.UnitWeight) || !IsFiniteNonNegative(line.UnitVolume))
            {
                errors.Add("cargo_line_measure_invalid");
            }
            if (line.DangerClass < D21CargoDangerClass.None || line.DangerClass > D21CargoDangerClass.Corrosive)
            {
                errors.Add("cargo_line_danger_class_invalid");
            }
            if (line.DangerClass != D21CargoDangerClass.None && !D21ShipIdentityContracts.IsStableId(line.StorageCompartmentId, 96))
            {
                errors.Add("cargo_line_compartment_required");
            }
            if (errors.Count > 0)
            {
                return new D21CargoCapacityValidationResult(false, D21CargoCapacityFailureCode.InvalidLine, errors);
            }
            return new D21CargoCapacityValidationResult(true, D21CargoCapacityFailureCode.None, new string[0]);
        }

        /// <summary>评估增加或移除货物后的容量和危险货物状态。</summary>
        public static D21CargoCapacityDecision Evaluate(D21CargoCapacityEvaluationRequest request)
        {
            var decision = new D21CargoCapacityDecision();
            if (request == null || request.Snapshot == null || request.Candidate == null)
            {
                return FailureDecision(decision, D21CargoCapacityFailureCode.InvalidRequest, "cargo_request_missing");
            }
            var snapshotValidation = ValidateSnapshot(request.Snapshot);
            if (!snapshotValidation.Valid)
            {
                return FailureDecision(decision, snapshotValidation.Code, "cargo_snapshot_invalid");
            }
            var candidateValidation = ValidateLine(request.Candidate);
            if (!candidateValidation.Valid)
            {
                return FailureDecision(decision, candidateValidation.Code, "cargo_candidate_invalid");
            }
            if (request.MutationKind != D21CargoMutationKind.Add && request.MutationKind != D21CargoMutationKind.Remove)
            {
                return FailureDecision(decision, D21CargoCapacityFailureCode.InvalidRequest, "cargo_mutation_kind_invalid");
            }
            if (!string.IsNullOrEmpty(request.RequestId) && !D21ShipIdentityContracts.IsStableId(request.RequestId, 128))
            {
                return FailureDecision(decision, D21CargoCapacityFailureCode.InvalidRequest, "cargo_request_id_invalid");
            }
            if (!string.IsNullOrEmpty(request.CauseEventId) && !D21ShipIdentityContracts.IsStableId(request.CauseEventId, 128))
            {
                return FailureDecision(decision, D21CargoCapacityFailureCode.InvalidRequest, "cargo_cause_event_invalid");
            }

            var resultingLines = CloneLines(request.Snapshot.ExistingLines);
            if (request.MutationKind == D21CargoMutationKind.Add)
            {
                resultingLines.Add(request.Candidate.Clone());
            }
            else
            {
                string removeError;
                if (!TryRemoveLine(resultingLines, request.Candidate, out removeError))
                {
                    return FailureDecision(decision, D21CargoCapacityFailureCode.InsufficientQuantity, removeError);
                }
            }

            double usedWeight;
            double usedVolume;
            if (!TryTotals(request.Snapshot.ExistingLines, out usedWeight, out usedVolume))
            {
                return FailureDecision(decision, D21CargoCapacityFailureCode.CapacityOverflow, "cargo_existing_total_overflow");
            }
            double projectedWeight;
            double projectedVolume;
            if (!TryTotals(resultingLines, out projectedWeight, out projectedVolume))
            {
                return FailureDecision(decision, D21CargoCapacityFailureCode.CapacityOverflow, "cargo_projected_total_overflow");
            }

            decision.Valid = true;
            decision.UsedWeight = Round(usedWeight);
            decision.UsedVolume = Round(usedVolume);
            decision.ProjectedWeight = Round(projectedWeight);
            decision.ProjectedVolume = Round(projectedVolume);
            decision.RemainingWeight = Round(request.Snapshot.MaxWeight - projectedWeight);
            decision.RemainingVolume = Round(request.Snapshot.MaxVolume - projectedVolume);
            decision.OverloadRatio = Round(ComputeOverloadRatio(projectedWeight, projectedVolume, request.Snapshot.MaxWeight, request.Snapshot.MaxVolume));
            decision.IsOverloaded = decision.OverloadRatio > 0d;
            decision.OverloadConsequence = ConsequenceFor(decision.OverloadRatio);
            decision.NavigationSpeedMultiplier = SpeedMultiplierFor(decision.OverloadConsequence);
            decision.DepartureAllowed = decision.OverloadConsequence != D21OverloadConsequence.DepartureRefused;

            var conflicts = FindDangerousGoodsConflicts(resultingLines, request.Snapshot.StorageContext);
            decision.DangerousGoodsConflicts = new ReadOnlyCollection<string>(conflicts);
            if (conflicts.Count > 0)
            {
                decision.Allowed = false;
                decision.ReasonCode = D21CargoCapacityFailureCode.DangerousGoodsConflict;
                decision.Errors = new ReadOnlyCollection<string>(conflicts);
                return decision;
            }
            if (decision.IsOverloaded)
            {
                decision.Allowed = false;
                decision.ReasonCode = D21CargoCapacityFailureCode.CapacityExceeded;
                decision.Errors = new ReadOnlyCollection<string>(new[] { "cargo_capacity_exceeded" });
                return decision;
            }

            decision.Allowed = true;
            decision.ReasonCode = D21CargoCapacityFailureCode.None;
            decision.Errors = new ReadOnlyCollection<string>(new string[0]);
            return decision;
        }

        /// <summary>校验容量快照但不执行货物变化。</summary>
        public static D21CargoCapacityValidationResult ValidateSnapshot(D21CargoCapacitySnapshot snapshot)
        {
            var errors = new List<string>();
            if (snapshot == null)
            {
                return new D21CargoCapacityValidationResult(false, D21CargoCapacityFailureCode.InvalidSnapshot, new[] { "cargo_snapshot_missing" });
            }
            if (snapshot.SchemaVersion != CurrentSchemaVersion || !D21ShipIdentityContracts.IsStableId(snapshot.ShipId, 96))
            {
                errors.Add("cargo_snapshot_identity_invalid");
            }
            if (!IsFiniteNonNegative(snapshot.MaxWeight) || !IsFiniteNonNegative(snapshot.MaxVolume))
            {
                errors.Add("cargo_snapshot_capacity_invalid");
            }
            if (snapshot.SaveRevision < 0 || snapshot.ExistingLines == null || snapshot.StorageContext == null)
            {
                errors.Add("cargo_snapshot_metadata_invalid");
            }
            if (snapshot.ExistingLines != null)
            {
                for (var index = 0; index < snapshot.ExistingLines.Count; index++)
                {
                    if (!ValidateLine(snapshot.ExistingLines[index]).Valid)
                    {
                        errors.Add("cargo_existing_line_invalid");
                    }
                }
            }
            if (errors.Count > 0)
            {
                return new D21CargoCapacityValidationResult(false, D21CargoCapacityFailureCode.InvalidSnapshot, errors);
            }
            return new D21CargoCapacityValidationResult(true, D21CargoCapacityFailureCode.None, new string[0]);
        }

        /// <summary>从评估请求构造交给 D3 的只读数量变化请求。</summary>
        public static bool TryBuildOwnerMutationRequest(D21CargoCapacityEvaluationRequest request, out D21CargoMutationRequest ownerRequest, out string error)
        {
            ownerRequest = null;
            error = null;
            if (request == null || request.Snapshot == null || request.Candidate == null || !D21ShipIdentityContracts.IsStableId(request.RequestId, 128))
            {
                error = "cargo_owner_request_invalid";
                return false;
            }
            if (request.MutationKind != D21CargoMutationKind.Add && request.MutationKind != D21CargoMutationKind.Remove)
            {
                error = "cargo_owner_mutation_kind_invalid";
                return false;
            }
            if (!string.IsNullOrEmpty(request.CauseEventId) && !D21ShipIdentityContracts.IsStableId(request.CauseEventId, 128))
            {
                error = "cargo_owner_cause_event_invalid";
                return false;
            }
            var lineValidation = ValidateLine(request.Candidate);
            if (!lineValidation.Valid)
            {
                error = "cargo_owner_line_invalid";
                return false;
            }
            ownerRequest = new D21CargoMutationRequest
            {
                RequestId = request.RequestId,
                ShipId = request.Snapshot.ShipId,
                MutationKind = request.MutationKind,
                Line = request.Candidate.Clone(),
                ExpectedSaveRevision = request.Snapshot.SaveRevision,
                CauseEventId = request.CauseEventId,
            };
            return true;
        }

        /// <summary>判断一组货物在给定储运条件下是否没有隔离冲突。</summary>
        public static bool IsDangerousGoodsCompatible(IEnumerable<D21CargoLineSnapshot> lines, D21CargoStorageContext context, out ReadOnlyCollection<string> conflicts)
        {
            conflicts = new ReadOnlyCollection<string>(FindDangerousGoodsConflicts(CloneLines(lines), context));
            return conflicts.Count == 0;
        }

        private static D21CargoCapacityValidationResult ValidateSnapshotForTotals(D21CargoCapacitySnapshot snapshot)
        {
            return ValidateSnapshot(snapshot);
        }

        private static D21CargoCapacityDecision FailureDecision(D21CargoCapacityDecision decision, D21CargoCapacityFailureCode code, string error)
        {
            decision.Valid = false;
            decision.Allowed = false;
            decision.DepartureAllowed = false;
            decision.ReasonCode = code;
            decision.Errors = new ReadOnlyCollection<string>(new[] { error ?? code.ToString() });
            return decision;
        }

        private static List<D21CargoLineSnapshot> CloneLines(IEnumerable<D21CargoLineSnapshot> source)
        {
            var result = new List<D21CargoLineSnapshot>();
            if (source == null)
            {
                return result;
            }
            foreach (var line in source)
            {
                if (line != null)
                {
                    result.Add(line.Clone());
                }
            }
            return result;
        }

        private static bool TryRemoveLine(List<D21CargoLineSnapshot> lines, D21CargoLineSnapshot candidate, out string error)
        {
            error = null;
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var compartmentMatches = string.IsNullOrEmpty(candidate.StorageCompartmentId) || string.Equals(line.StorageCompartmentId, candidate.StorageCompartmentId, StringComparison.Ordinal);
                if (string.Equals(line.ItemId, candidate.ItemId, StringComparison.Ordinal) && compartmentMatches && line.DangerClass == candidate.DangerClass)
                {
                    if (line.Quantity < candidate.Quantity)
                    {
                        error = "cargo_quantity_insufficient";
                        return false;
                    }
                    line.Quantity -= candidate.Quantity;
                    if (line.Quantity == 0)
                    {
                        lines.RemoveAt(index);
                    }
                    return true;
                }
            }
            error = "cargo_item_not_found";
            return false;
        }

        private static bool TryTotals(IEnumerable<D21CargoLineSnapshot> lines, out double weight, out double volume)
        {
            weight = 0d;
            volume = 0d;
            if (lines == null)
            {
                return true;
            }
            foreach (var line in lines)
            {
                if (line == null)
                {
                    return false;
                }
                var lineWeight = line.TotalWeight();
                var lineVolume = line.TotalVolume();
                if (double.IsNaN(lineWeight) || double.IsInfinity(lineWeight) || double.IsNaN(lineVolume) || double.IsInfinity(lineVolume))
                {
                    return false;
                }
                weight += lineWeight;
                volume += lineVolume;
                if (double.IsNaN(weight) || double.IsInfinity(weight) || double.IsNaN(volume) || double.IsInfinity(volume))
                {
                    return false;
                }
            }
            return true;
        }

        private static double ComputeOverloadRatio(double weight, double volume, double maxWeight, double maxVolume)
        {
            var weightRatio = maxWeight <= 0d ? (weight > 0d ? double.MaxValue : 0d) : weight / maxWeight;
            var volumeRatio = maxVolume <= 0d ? (volume > 0d ? double.MaxValue : 0d) : volume / maxVolume;
            var ratio = Math.Max(weightRatio, volumeRatio) - 1d;
            return Math.Max(0d, ratio);
        }

        private static D21OverloadConsequence ConsequenceFor(double overloadRatio)
        {
            if (overloadRatio <= 0d) return D21OverloadConsequence.None;
            if (overloadRatio <= 0.05d) return D21OverloadConsequence.ReducedSpeed;
            if (overloadRatio <= 0.15d) return D21OverloadConsequence.RestrictedManeuver;
            if (overloadRatio <= 0.25d) return D21OverloadConsequence.UnsafeDraft;
            return D21OverloadConsequence.DepartureRefused;
        }

        private static double SpeedMultiplierFor(D21OverloadConsequence consequence)
        {
            switch (consequence)
            {
                case D21OverloadConsequence.ReducedSpeed: return 0.9d;
                case D21OverloadConsequence.RestrictedManeuver: return 0.75d;
                case D21OverloadConsequence.UnsafeDraft: return 0.55d;
                case D21OverloadConsequence.DepartureRefused: return 0d;
                default: return 1d;
            }
        }

        private static List<string> FindDangerousGoodsConflicts(List<D21CargoLineSnapshot> lines, D21CargoStorageContext context)
        {
            var conflicts = new List<string>();
            if (context == null)
            {
                conflicts.Add("cargo_storage_context_missing");
                return conflicts;
            }
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                if (line == null || line.DangerClass == D21CargoDangerClass.None)
                {
                    continue;
                }
                if ((line.DangerClass == D21CargoDangerClass.Flammable || line.DangerClass == D21CargoDangerClass.Explosive) && !context.HasFireSeparation)
                {
                    conflicts.Add("cargo_fire_separation_required");
                }
                if (line.DangerClass == D21CargoDangerClass.Explosive && !context.HasPowderMagazine)
                {
                    conflicts.Add("cargo_powder_magazine_required");
                }
                if ((line.DangerClass == D21CargoDangerClass.Toxic || line.DangerClass == D21CargoDangerClass.Corrosive) && !context.HasVentilation)
                {
                    conflicts.Add("cargo_ventilation_required");
                }
                if (line.DangerClass == D21CargoDangerClass.Live && !context.HasLiveHold)
                {
                    conflicts.Add("cargo_live_hold_required");
                }
            }
            for (var left = 0; left < lines.Count; left++)
            {
                for (var right = left + 1; right < lines.Count; right++)
                {
                    if (!string.IsNullOrEmpty(lines[left].StorageCompartmentId) && string.Equals(lines[left].StorageCompartmentId, lines[right].StorageCompartmentId, StringComparison.Ordinal) && PairConflicts(lines[left].DangerClass, lines[right].DangerClass))
                    {
                        conflicts.Add("cargo_compartment_pair_conflict_" + lines[left].StorageCompartmentId);
                    }
                }
            }
            return Deduplicate(conflicts);
        }

        private static bool PairConflicts(D21CargoDangerClass left, D21CargoDangerClass right)
        {
            if ((left == D21CargoDangerClass.Food && (right == D21CargoDangerClass.Toxic || right == D21CargoDangerClass.Corrosive)) ||
                (right == D21CargoDangerClass.Food && (left == D21CargoDangerClass.Toxic || left == D21CargoDangerClass.Corrosive)))
            {
                return true;
            }
            if ((left == D21CargoDangerClass.Explosive && right == D21CargoDangerClass.Flammable) ||
                (right == D21CargoDangerClass.Explosive && left == D21CargoDangerClass.Flammable))
            {
                return true;
            }
            if ((left == D21CargoDangerClass.Live && (right == D21CargoDangerClass.Toxic || right == D21CargoDangerClass.Corrosive)) ||
                (right == D21CargoDangerClass.Live && (left == D21CargoDangerClass.Toxic || left == D21CargoDangerClass.Corrosive)))
            {
                return true;
            }
            return false;
        }

        private static List<string> Deduplicate(List<string> values)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<string>();
            for (var index = 0; index < values.Count; index++)
            {
                if (seen.Add(values[index]))
                {
                    result.Add(values[index]);
                }
            }
            return result;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d;
        }

        private static double Round(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                return value;
            }
            return Math.Round(value, 4, MidpointRounding.AwayFromZero);
        }
    }
}
