// ============================================================
// D21VoyageRiskSystem —— D21-L17 航程风险资格与确定性种子
//
// 风险系统只读取航线/天气/船况/生态/船员 owner 提交的公开快照，输出
// 风险资格、阈值、滚动值和稳定种子；它不直接施加损伤、疾病、法律或
// 生态后果。种子由稳定字符串和整数经 FNV-1a 派生，避免 HashCode、
// 系统时间或不可控随机源造成重放差异。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.D11;

namespace Whalebone.Systems.D21
{
    /// <summary>可被 D21 航程 owner 识别的风险类别。</summary>
    public enum D21VoyageRiskType
    {
        None = 0,
        Reef = 1,
        Storm = 2,
        Hostile = 3,
        Disease = 4,
    }

    /// <summary>单项风险的可保存种子与阈值输入摘要。</summary>
    [Serializable]
    public sealed class D21VoyageRiskSeed
    {
        public D21VoyageRiskType RiskType;
        public long Seed;
        public int RollPermille;
        public string InputFingerprint = string.Empty;

        public D21VoyageRiskSeed Clone()
        {
            return new D21VoyageRiskSeed
            {
                RiskType = RiskType,
                Seed = Seed,
                RollPermille = RollPermille,
                InputFingerprint = InputFingerprint ?? string.Empty,
            };
        }
    }

    /// <summary>
    /// 风险计算输入。数值均为已由所属 owner 裁决的 permille 快照，
    /// D21 只消费，不把这些字段保存为天气/船员/生态事实。
    /// </summary>
    [Serializable]
    public sealed class D21VoyageRiskInput
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public string RouteId = string.Empty;
        public string LocationRef = string.Empty;
        public long VoyageSeed;
        public D21TickContext TickContext;

        public bool ReefZone;
        public bool StormActive;
        public bool HostileWaters;
        public bool DiseaseExposure;
        public int ReefRiskPermille;
        public int StormRiskPermille;
        public int HostileRiskPermille;
        public int DiseaseRiskPermille;
        public int SeaStateRiskPermille;
        public int VisibilityPermille = 1000;
        public int ShipConditionPermille = 1000;
        public int CrewHealthPermille = 1000;
        public int CrewExposurePermille;
        public bool HasLookout;
        public bool HasMedicine;

        // 这些是只读上游证据引用，不是 D21 天气/生态的第二份快照。
        public string WeatherSnapshotId = string.Empty;
        public int WeatherSnapshotSchemaVersion;
        public long WeatherSnapshotRevision;
        public string EcologySnapshotId = string.Empty;
        public int EcologySnapshotSchemaVersion;
        public long EcologySnapshotRevision;
        public List<string> InputFactIds = new List<string>();

        public D21VoyageRiskInput Clone()
        {
            var result = new D21VoyageRiskInput
            {
                SchemaVersion = SchemaVersion,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                RouteId = RouteId ?? string.Empty,
                LocationRef = LocationRef ?? string.Empty,
                VoyageSeed = VoyageSeed,
                TickContext = TickContext == null ? null : TickContext.Clone(),
                ReefZone = ReefZone,
                StormActive = StormActive,
                HostileWaters = HostileWaters,
                DiseaseExposure = DiseaseExposure,
                ReefRiskPermille = ReefRiskPermille,
                StormRiskPermille = StormRiskPermille,
                HostileRiskPermille = HostileRiskPermille,
                DiseaseRiskPermille = DiseaseRiskPermille,
                SeaStateRiskPermille = SeaStateRiskPermille,
                VisibilityPermille = VisibilityPermille,
                ShipConditionPermille = ShipConditionPermille,
                CrewHealthPermille = CrewHealthPermille,
                CrewExposurePermille = CrewExposurePermille,
                HasLookout = HasLookout,
                HasMedicine = HasMedicine,
                WeatherSnapshotId = WeatherSnapshotId ?? string.Empty,
                WeatherSnapshotSchemaVersion = WeatherSnapshotSchemaVersion,
                WeatherSnapshotRevision = WeatherSnapshotRevision,
                EcologySnapshotId = EcologySnapshotId ?? string.Empty,
                EcologySnapshotSchemaVersion = EcologySnapshotSchemaVersion,
                EcologySnapshotRevision = EcologySnapshotRevision,
            };
            if (InputFactIds != null) result.InputFactIds.AddRange(InputFactIds);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D21 voyage risk schema_version mismatch.");
            if (string.IsNullOrWhiteSpace(VoyageId) || string.IsNullOrWhiteSpace(ShipId) ||
                string.IsNullOrWhiteSpace(RouteId) || string.IsNullOrWhiteSpace(LocationRef))
                throw new InvalidOperationException("D21 voyage risk ids and location_ref are required.");
            if (VoyageSeed < 0 || TickContext == null)
                throw new InvalidOperationException("D21 voyage risk seed and tick context are required.");
            TickContext.Validate();
            if (ReefRiskPermille < 0 || ReefRiskPermille > 1000 ||
                StormRiskPermille < 0 || StormRiskPermille > 1000 ||
                HostileRiskPermille < 0 || HostileRiskPermille > 1000 ||
                DiseaseRiskPermille < 0 || DiseaseRiskPermille > 1000 ||
                SeaStateRiskPermille < 0 || SeaStateRiskPermille > 1000 ||
                VisibilityPermille < 0 || VisibilityPermille > 1000 ||
                ShipConditionPermille < 0 || ShipConditionPermille > 1000 ||
                CrewHealthPermille < 0 || CrewHealthPermille > 1000 ||
                CrewExposurePermille < 0 || CrewExposurePermille > 1000)
                throw new InvalidOperationException("D21 voyage risk permille input is outside [0, 1000].");
            if (WeatherSnapshotRevision < 0 || EcologySnapshotRevision < 0)
                throw new InvalidOperationException("D21 upstream snapshot revisions cannot be negative.");
            if (InputFactIds == null) throw new InvalidOperationException("D21 risk input facts are required.");
            foreach (string factId in InputFactIds)
                if (string.IsNullOrWhiteSpace(factId))
                    throw new InvalidOperationException("D21 risk input facts cannot contain empty ids.");
        }

        /// <summary>
        /// 从 D9 天气和 D11 生态公开读取快照建立输入；只读字段被压缩为
        /// 本次计算的输入摘要，不把上游对象挂入 D21 状态。
        /// </summary>
        public static D21VoyageRiskInput FromPublicSnapshots(
            string voyageId,
            string shipId,
            string routeId,
            string locationRef,
            long voyageSeed,
            D21TickContext tickContext,
            D9WeatherSeaSnapshot weather,
            D11MarineEcologyReadSnapshot ecology,
            int reefRiskPermille = 0,
            int hostileRiskPermille = 0,
            int diseaseRiskPermille = 0,
            bool reefZone = false,
            bool hostileWaters = false,
            D4RuntimeSnapshot survivalSnapshot = null)
        {
            var result = new D21VoyageRiskInput
            {
                VoyageId = voyageId ?? string.Empty,
                ShipId = shipId ?? string.Empty,
                RouteId = routeId ?? string.Empty,
                LocationRef = locationRef ?? string.Empty,
                VoyageSeed = voyageSeed,
                TickContext = tickContext == null ? null : tickContext.Clone(),
                ReefZone = reefZone,
                HostileWaters = hostileWaters,
                ReefRiskPermille = ClampPermille(reefRiskPermille),
                HostileRiskPermille = ClampPermille(hostileRiskPermille),
                DiseaseRiskPermille = ClampPermille(diseaseRiskPermille),
            };

            if (weather != null)
            {
                result.WeatherSnapshotId = string.IsNullOrEmpty(weather.LastEventId)
                    ? "d9-weather-day-" + weather.WorldDay.ToString(CultureInfo.InvariantCulture)
                    : weather.LastEventId;
                result.WeatherSnapshotSchemaVersion = weather.SchemaVersion;
                result.WeatherSnapshotRevision = Math.Max(0, weather.Revision);
                result.StormActive = weather.Weather == D9WeatherState.Storm;
                result.VisibilityPermille = ToPermille(weather.Visibility);
                result.SeaStateRiskPermille = ToPermille(weather.HullRisk);
                result.StormRiskPermille = ToPermille(weather.HullRisk);
                result.InputFactIds.Add("weather:" + result.WeatherSnapshotId);
            }

            if (ecology != null)
            {
                result.EcologySnapshotId = ecology.HabitatId ?? string.Empty;
                result.EcologySnapshotSchemaVersion = D11WhaleDolphinSchema.CurrentVersion;
                result.EcologySnapshotRevision = 0;
                result.DiseaseExposure = ecology.EcologyStress >= 0.65f;
                result.ReefZone = result.ReefZone ||
                    (!string.IsNullOrEmpty(ecology.HabitatId) &&
                     ecology.HabitatId.IndexOf("reef", StringComparison.OrdinalIgnoreCase) >= 0);
                int stress = ToPermille(ecology.EcologyStress);
                result.SeaStateRiskPermille = Math.Max(result.SeaStateRiskPermille, stress / 2);
                result.InputFactIds.Add("ecology:" + result.EcologySnapshotId);
            }

            ApplySurvivalSnapshot(result, survivalSnapshot);
            result.Validate();
            return result;
        }

        private static void ApplySurvivalSnapshot(D21VoyageRiskInput target, D4RuntimeSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Health == null || snapshot.Health.Crew == null ||
                snapshot.Health.Crew.Count == 0) return;

            int minHealth = 100;
            int maxFatigue = 0;
            foreach (CrewHealthState member in snapshot.Health.Crew)
            {
                if (member == null) continue;
                minHealth = Math.Min(minHealth, Math.Max(0, Math.Min(100, member.Health)));
                maxFatigue = Math.Max(maxFatigue, Math.Max(0, Math.Min(100, member.Fatigue)));
                if (member.Diseases != null && member.Diseases.Count > 0)
                    target.DiseaseExposure = true;
            }
            target.CrewHealthPermille = minHealth * 10;
            target.CrewExposurePermille = maxFatigue * 10;
            target.InputFactIds.Add("crew_health:d4");
        }

        private static int ClampPermille(int value)
        {
            return Math.Max(0, Math.Min(1000, value));
        }

        private static int ToPermille(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0;
            return ClampPermille((int)Math.Round(value * 1000f, MidpointRounding.AwayFromZero));
        }
    }

    /// <summary>单项风险的资格与确定性裁决输出。</summary>
    [Serializable]
    public sealed class D21VoyageRiskQualification
    {
        public D21VoyageRiskType RiskType;
        public bool Eligible;
        public bool Triggered;
        public int ThresholdPermille;
        public int RollPermille;
        public string Code = string.Empty;
        public D21VoyageRiskSeed Seed;
        public List<string> InputFactIds = new List<string>();

        public D21VoyageRiskQualification Clone()
        {
            var result = new D21VoyageRiskQualification
            {
                RiskType = RiskType,
                Eligible = Eligible,
                Triggered = Triggered,
                ThresholdPermille = ThresholdPermille,
                RollPermille = RollPermille,
                Code = Code ?? string.Empty,
                Seed = Seed == null ? null : Seed.Clone(),
            };
            if (InputFactIds != null) result.InputFactIds.AddRange(InputFactIds);
            return result;
        }
    }

    /// <summary>风险系统一次固定 Tick 的完整只读结果。</summary>
    [Serializable]
    public sealed class D21VoyageRiskResult
    {
        public bool Accepted;
        public bool AnyTriggered;
        public D21VoyageRiskType PrimaryTriggeredRisk;
        public string Code = string.Empty;
        public List<D21VoyageRiskQualification> Qualifications = new List<D21VoyageRiskQualification>();

        public D21VoyageRiskResult Clone()
        {
            var result = new D21VoyageRiskResult
            {
                Accepted = Accepted,
                AnyTriggered = AnyTriggered,
                PrimaryTriggeredRisk = PrimaryTriggeredRisk,
                Code = Code ?? string.Empty,
            };
            if (Qualifications != null)
                foreach (D21VoyageRiskQualification item in Qualifications)
                    if (item != null) result.Qualifications.Add(item.Clone());
            return result;
        }
    }

    /// <summary>纯函数风险资格与种子 owner。</summary>
    public static class D21VoyageRiskSystem
    {
        public const int CurrentSchemaVersion = D21VoyageRiskInput.CurrentSchemaVersion;

        public static D21VoyageRiskResult Evaluate(D21VoyageRiskInput input)
        {
            if (input == null)
                return new D21VoyageRiskResult { Accepted = false, Code = "missing_input" };
            try
            {
                input.Validate();
                var result = new D21VoyageRiskResult { Accepted = true, Code = "risk_evaluated" };
                D21VoyageRiskType[] order =
                {
                    D21VoyageRiskType.Reef,
                    D21VoyageRiskType.Storm,
                    D21VoyageRiskType.Hostile,
                    D21VoyageRiskType.Disease,
                };

                foreach (D21VoyageRiskType riskType in order)
                {
                    bool eligible;
                    int threshold = CalculateThreshold(input, riskType, out eligible);
                    long seed = DeriveSeed(input, riskType);
                    int roll = RollPermille(seed);
                    bool triggered = eligible && roll < threshold;
                    var qualification = new D21VoyageRiskQualification
                    {
                        RiskType = riskType,
                        Eligible = eligible,
                        Triggered = triggered,
                        ThresholdPermille = threshold,
                        RollPermille = roll,
                        Code = !eligible ? "not_eligible" : (triggered ? "triggered" : "not_triggered"),
                        Seed = new D21VoyageRiskSeed
                        {
                            RiskType = riskType,
                            Seed = seed,
                            RollPermille = roll,
                            InputFingerprint = BuildInputFingerprint(input, riskType),
                        },
                    };
                    if (input.InputFactIds != null)
                        qualification.InputFactIds.AddRange(input.InputFactIds);
                    result.Qualifications.Add(qualification);
                    if (triggered && !result.AnyTriggered)
                    {
                        result.AnyTriggered = true;
                        result.PrimaryTriggeredRisk = riskType;
                    }
                }
                return result;
            }
            catch (InvalidOperationException)
            {
                return new D21VoyageRiskResult { Accepted = false, Code = "invalid_risk_input" };
            }
            catch (OverflowException)
            {
                return new D21VoyageRiskResult { Accepted = false, Code = "risk_counter_overflow" };
            }
        }

        public static D21VoyageRiskQualification Qualify(
            D21VoyageRiskInput input,
            D21VoyageRiskType riskType)
        {
            D21VoyageRiskResult result = Evaluate(input);
            foreach (D21VoyageRiskQualification qualification in result.Qualifications)
                if (qualification.RiskType == riskType) return qualification;
            return new D21VoyageRiskQualification
            {
                RiskType = riskType,
                Code = result.Code,
            };
        }

        /// <summary>稳定风险种子；相同航程/步/修订/类别输入必得相同结果。</summary>
        public static long DeriveSeed(
            long voyageSeed,
            string voyageId,
            string routeId,
            string locationRef,
            int tickIndex,
            long saveRevision,
            D21VoyageRiskType riskType)
        {
            if (voyageSeed < 0 || tickIndex < 0 || saveRevision < 0)
                throw new ArgumentOutOfRangeException("D21 deterministic risk seed input is invalid.");
            ulong hash = 14695981039346656037UL;
            hash = MixString(hash, voyageSeed.ToString(CultureInfo.InvariantCulture));
            hash = MixString(hash, voyageId ?? string.Empty);
            hash = MixString(hash, routeId ?? string.Empty);
            hash = MixString(hash, locationRef ?? string.Empty);
            hash = MixString(hash, tickIndex.ToString(CultureInfo.InvariantCulture));
            hash = MixString(hash, saveRevision.ToString(CultureInfo.InvariantCulture));
            hash = MixString(hash, riskType.ToString());
            long result = (long)(hash & 0x7fffffffffffffffUL);
            return result == 0 ? 1 : result;
        }

        private static long DeriveSeed(D21VoyageRiskInput input, D21VoyageRiskType riskType)
        {
            long seed = input.VoyageSeed == 0 ? input.TickContext.VoyageSeed : input.VoyageSeed;
            return DeriveSeed(seed, input.VoyageId, input.RouteId, input.LocationRef,
                input.TickContext.TickIndex, input.TickContext.SaveRevision, riskType);
        }

        private static int RollPermille(long seed)
        {
            long value = seed % 1000L;
            return (int)(value < 0 ? value + 1000L : value);
        }

        private static int CalculateThreshold(
            D21VoyageRiskInput input,
            D21VoyageRiskType riskType,
            out bool eligible)
        {
            int threshold;
            switch (riskType)
            {
                case D21VoyageRiskType.Reef:
                    eligible = input.ReefZone || input.ReefRiskPermille > 0;
                    threshold = input.ReefRiskPermille + input.SeaStateRiskPermille / 2 +
                        (1000 - input.VisibilityPermille) / 4 +
                        (1000 - input.ShipConditionPermille) / 10 -
                        (input.HasLookout ? 100 : 0);
                    break;
                case D21VoyageRiskType.Storm:
                    eligible = input.StormActive || input.StormRiskPermille > 0 ||
                        input.SeaStateRiskPermille >= 600;
                    threshold = input.StormRiskPermille + input.SeaStateRiskPermille / 2 +
                        (1000 - input.ShipConditionPermille) / 8 + (input.StormActive ? 100 : 0);
                    break;
                case D21VoyageRiskType.Hostile:
                    eligible = input.HostileWaters || input.HostileRiskPermille > 0;
                    threshold = input.HostileRiskPermille + input.SeaStateRiskPermille / 5 +
                        (input.HasLookout ? 0 : 75);
                    break;
                case D21VoyageRiskType.Disease:
                    eligible = input.DiseaseExposure || input.DiseaseRiskPermille > 0 ||
                        input.CrewExposurePermille > 0;
                    threshold = input.DiseaseRiskPermille + input.CrewExposurePermille / 2 +
                        (1000 - input.CrewHealthPermille) / 4 - (input.HasMedicine ? 100 : 0);
                    break;
                default:
                    eligible = false;
                    threshold = 0;
                    break;
            }
            return Math.Max(0, Math.Min(1000, threshold));
        }

        private static string BuildInputFingerprint(D21VoyageRiskInput input, D21VoyageRiskType riskType)
        {
            return "d21risk_v" + input.SchemaVersion.ToString(CultureInfo.InvariantCulture) + "|" +
                (input.VoyageId ?? string.Empty) + "|" + (input.RouteId ?? string.Empty) + "|" +
                input.TickContext.TickIndex.ToString(CultureInfo.InvariantCulture) + "|" +
                input.TickContext.SaveRevision.ToString(CultureInfo.InvariantCulture) + "|" + riskType;
        }

        private static ulong MixString(ulong hash, string value)
        {
            unchecked
            {
                string text = value ?? string.Empty;
                for (int i = 0; i < text.Length; i++)
                {
                    hash ^= text[i];
                    hash *= 1099511628211UL;
                }
                hash ^= 0xffUL;
                hash *= 1099511628211UL;
                return hash;
            }
        }
    }
}
