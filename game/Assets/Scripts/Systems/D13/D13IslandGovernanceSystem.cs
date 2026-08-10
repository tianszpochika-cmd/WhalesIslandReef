// D13-A05 IslandGovernanceSystem。
// 该系统唯一写居民、家庭、就业投影、健康、满意度、治安及治理记录；
// 产权、供给、经济、政策、社群和代理人结果必须来自外部 authority。

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public sealed class D13IslandGovernanceSystem
    {
        private readonly D13IslandGovernanceDefinition _definition;
        private readonly Dictionary<string, D13GovernanceJobDefinition> _jobs = new Dictionary<string, D13GovernanceJobDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13HouseholdState> _households = new Dictionary<string, D13HouseholdState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13ResidentState> _residents = new Dictionary<string, D13ResidentState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13GovernanceAgreementState> _agreements = new Dictionary<string, D13GovernanceAgreementState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13GovernanceProvisionRecord> _provisions = new Dictionary<string, D13GovernanceProvisionRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13GovernanceTaxRecord> _taxes = new Dictionary<string, D13GovernanceTaxRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13GovernanceWageRecord> _wages = new Dictionary<string, D13GovernanceWageRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13GovernanceIncidentState> _incidents = new Dictionary<string, D13GovernanceIncidentState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13GovernanceAgentAssignment> _agents = new Dictionary<string, D13GovernanceAgentAssignment>(StringComparer.Ordinal);
        private readonly HashSet<string> _appliedEventKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly ID13GovernanceResidenceAuthority _residenceAuthority;
        private readonly ID13GovernanceProvisionAuthority _provisionAuthority;
        private readonly ID13GovernanceEconomyAuthority _economyAuthority;
        private readonly ID13GovernancePolicyAuthority _policyAuthority;
        private readonly ID13GovernanceCommunityAuthority _communityAuthority;
        private readonly ID13GovernanceAgentAuthority _agentAuthority;
        private readonly EventLog _eventLog;
        private D13GovernancePolicyState _policy;
        private D13SecurityStatus _securityStatus = D13SecurityStatus.Stable;
        private int _securityScore;
        private int _lastWorldDay;
        private string _lastEventId;
        private long _revision;

        public D13IslandGovernanceSystem(
            ID13GovernanceResidenceAuthority residenceAuthority,
            ID13GovernanceProvisionAuthority provisionAuthority,
            ID13GovernanceEconomyAuthority economyAuthority,
            ID13GovernancePolicyAuthority policyAuthority,
            ID13GovernanceCommunityAuthority communityAuthority,
            ID13GovernanceAgentAuthority agentAuthority,
            EventLog eventLog = null,
            D13GovernanceRuntimeSnapshot initial = null,
            D13IslandGovernanceDefinition definition = null,
            IEnumerable<D13GovernanceJobDefinition> jobs = null)
        {
            D13IslandGovernanceConfiguration.ThrowIfInvalid();
            _definition = (definition ?? D13IslandGovernanceCatalog.FindIsland(D13IslandGovernanceCatalog.SaintMarysIslandId)).Clone();
            _definition.Validate();
            _residenceAuthority = residenceAuthority ?? new UnavailableResidenceAuthority();
            _provisionAuthority = provisionAuthority ?? new UnavailableProvisionAuthority();
            _economyAuthority = economyAuthority ?? new UnavailableEconomyAuthority();
            _policyAuthority = policyAuthority ?? new UnavailablePolicyAuthority();
            _communityAuthority = communityAuthority ?? new UnavailableCommunityAuthority();
            _agentAuthority = agentAuthority ?? new UnavailableAgentAuthority();
            _eventLog = eventLog ?? new EventLog();
            foreach (var source in jobs ?? D13IslandGovernanceCatalog.AllJobs)
            {
                if (source == null) continue;
                source.Validate();
                if (_jobs.ContainsKey(source.JobId)) throw new InvalidOperationException("Duplicate governance job: " + source.JobId);
                _jobs[source.JobId] = source.Clone();
            }
            _policy = new D13GovernancePolicyState
            {
                TaxRateBps = _definition.DefaultTaxRateBps,
                WaterRationingLevel = 0,
                FoodRationingLevel = 0,
                EducationOpen = true,
                ReligionArrangementId = _definition.DefaultReligionArrangementId,
                SecurityPostureId = "watch_regular",
            };
            _securityScore = _definition.SecurityBase;
            if (initial != null) Restore(initial);
        }

        public EventLog EventLog { get { return _eventLog; } }
        public long Revision { get { return _revision; } }
        public D13IslandGovernanceDefinition Definition { get { return _definition.Clone(); } }

        public D13ResidentState FindResident(string residentId)
        {
            D13ResidentState state;
            return _residents.TryGetValue(residentId ?? string.Empty, out state) ? state.Clone() : null;
        }

        public D13HouseholdState FindHousehold(string householdId)
        {
            D13HouseholdState state;
            return _households.TryGetValue(householdId ?? string.Empty, out state) ? state.Clone() : null;
        }

        public D13GovernanceJobDefinition FindJob(string jobId)
        {
            D13GovernanceJobDefinition job;
            return _jobs.TryGetValue(jobId ?? string.Empty, out job) ? job.Clone() : null;
        }

        public D13GovernanceReadModel GetReadModel()
        {
            var result = new D13GovernanceReadModel
            {
                Definition = _definition.Clone(), Policy = _policy.Clone(), SecurityScore = _securityScore,
                SecurityStatus = _securityStatus, Revision = _revision,
            };
            foreach (var item in _households.Values.OrderBy(item => item.HouseholdId, StringComparer.Ordinal)) result.Households.Add(item.Clone());
            foreach (var item in _residents.Values.OrderBy(item => item.ResidentId, StringComparer.Ordinal)) result.Residents.Add(item.Clone());
            foreach (var item in _agreements.Values.OrderBy(item => item.AgreementId, StringComparer.Ordinal)) result.Agreements.Add(item.Clone());
            foreach (var item in _provisions.Values.OrderBy(item => item.RecordId, StringComparer.Ordinal)) result.ProvisionRecords.Add(item.Clone());
            foreach (var item in _taxes.Values.OrderBy(item => item.AssessmentId, StringComparer.Ordinal)) result.Taxes.Add(item.Clone());
            foreach (var item in _wages.Values.OrderBy(item => item.PaymentId, StringComparer.Ordinal)) result.Wages.Add(item.Clone());
            foreach (var item in _incidents.Values.OrderBy(item => item.IncidentId, StringComparer.Ordinal)) result.Incidents.Add(item.Clone());
            foreach (var item in _agents.Values.OrderBy(item => item.AssignmentId, StringComparer.Ordinal)) result.AgentAssignments.Add(item.Clone());
            return result;
        }

        public D13GovernanceRuntimeSnapshot CaptureSnapshot()
        {
            var result = new D13GovernanceRuntimeSnapshot
            {
                Revision = _revision, Policy = _policy.Clone(), SecurityStatus = _securityStatus, SecurityScore = _securityScore,
                LastWorldDay = _lastWorldDay, LastEventId = _lastEventId,
            };
            foreach (var item in _households.Values) result.Households.Add(item.Clone());
            foreach (var item in _residents.Values) result.Residents.Add(item.Clone());
            foreach (var item in _agreements.Values) result.Agreements.Add(item.Clone());
            foreach (var item in _provisions.Values) result.ProvisionRecords.Add(item.Clone());
            foreach (var item in _taxes.Values) result.Taxes.Add(item.Clone());
            foreach (var item in _wages.Values) result.Wages.Add(item.Clone());
            foreach (var item in _incidents.Values) result.Incidents.Add(item.Clone());
            foreach (var item in _agents.Values) result.AgentAssignments.Add(item.Clone());
            foreach (var key in _appliedEventKeys) result.AppliedEventKeys.Add(key);
            foreach (var fact in _eventLog.Facts)
                if (fact != null && fact.EventId.StartsWith("event_d13_governance_", StringComparison.Ordinal) && !result.AppliedEventKeys.Contains(fact.DedupeKey)) result.AppliedEventKeys.Add(fact.DedupeKey);
            result.Validate();
            return result;
        }

        public void Restore(D13GovernanceRuntimeSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            foreach (var household in snapshot.Households) if (household.IslandId != _definition.IslandId) throw new InvalidOperationException("Governance snapshot references another island household.");
            foreach (var resident in snapshot.Residents) if (resident.IslandId != _definition.IslandId || !_households.ContainsKey(resident.HouseholdId) && !snapshot.Households.Any(item => item.HouseholdId == resident.HouseholdId)) throw new InvalidOperationException("Governance snapshot references an unknown household.");
            _households.Clear(); foreach (var item in snapshot.Households) _households[item.HouseholdId] = item.Clone();
            _residents.Clear(); foreach (var item in snapshot.Residents) _residents[item.ResidentId] = item.Clone();
            _agreements.Clear(); foreach (var item in snapshot.Agreements) _agreements[item.AgreementId] = item.Clone();
            _provisions.Clear(); foreach (var item in snapshot.ProvisionRecords) _provisions[item.RecordId] = item.Clone();
            _taxes.Clear(); foreach (var item in snapshot.Taxes) _taxes[item.AssessmentId] = item.Clone();
            _wages.Clear(); foreach (var item in snapshot.Wages) _wages[item.PaymentId] = item.Clone();
            _incidents.Clear(); foreach (var item in snapshot.Incidents) _incidents[item.IncidentId] = item.Clone();
            _agents.Clear(); foreach (var item in snapshot.AgentAssignments) _agents[item.AssignmentId] = item.Clone();
            _appliedEventKeys.Clear(); foreach (var key in snapshot.AppliedEventKeys) _appliedEventKeys.Add(key);
            _policy = snapshot.Policy.Clone(); _securityStatus = snapshot.SecurityStatus; _securityScore = snapshot.SecurityScore; _lastWorldDay = snapshot.LastWorldDay; _lastEventId = snapshot.LastEventId; _revision = snapshot.Revision;
        }

        public D13GovernanceResult MigrateIn(D13MigrationInRequest request)
        {
            if (request == null) return Invalid("invalid_request", "迁入请求不存在。", D13GovernanceIds.LandRecoveryRoute);
            string eventId = EventId(request.OperationId, "migrate_in_" + request.ResidentId);
            var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.ResidentId);
            if (!ValidIsland(request.IslandId) || string.IsNullOrWhiteSpace(request.ResidentId) || string.IsNullOrWhiteSpace(request.HouseholdId)) return Failure(request, eventId, "migration_request_invalid", "迁入必须提供岛屿、居民和家庭引用。", D13GovernanceIds.LandRecoveryRoute);
            if (_residents.ContainsKey(request.ResidentId)) return Failure(request, eventId, "resident_exists", "该居民已经登记，不能重复迁入。", D13GovernanceIds.LandRecoveryRoute);
            var residence = _residenceAuthority.ValidateResidence(_definition.Clone(), request, request.WorldDay);
            if (residence == null || !residence.Allowed) return Failure(request, eventId, residence == null ? "residence_authority_required" : residence.Code, residence == null ? "必须由 Property/法律 authority 提供居住资格。" : residence.Message, D13GovernanceIds.LandRecoveryRoute);
            if (residence.SourceOwnerSystem != D13GovernanceIds.PropertyOwner || !SourceExists(residence.PropertySourceEventId) || !SourceExists(residence.LegalSourceEventId)) return Failure(request, eventId, "residence_source_invalid", "迁入必须带有 PropertySystem 和法律来源事实。", D13GovernanceIds.LandRecoveryRoute);
            if (!string.IsNullOrWhiteSpace(request.SourceEventId) && !SourceExists(request.SourceEventId)) return Failure(request, eventId, "migration_source_missing", "迁入来源事实不存在。", D13GovernanceIds.LandRecoveryRoute);
            if (!Enum.IsDefined(typeof(D13ResidentRole), request.Role)) return Failure(request, eventId, "resident_role_invalid", "居民家庭角色无效。", D13GovernanceIds.LandRecoveryRoute);
            if (_definition.ReligionContent.ReviewStatus == D13GovernanceContentReviewStatus.Blocked || _definition.ReligionContent.ReviewStatus == D13GovernanceContentReviewStatus.PendingReview || (_definition.ReligionContent.ReviewStatus == D13GovernanceContentReviewStatus.ApprovedWithWarning && !request.ReviewAcknowledged)) return Failure(request, eventId, "religion_review_required", "宗教内容需要审查确认后才能作为居民登记语境。", D13GovernanceIds.ReligionRecoveryRoute);
            D13HouseholdState household;
            bool newHousehold = !_households.TryGetValue(request.HouseholdId, out household);
            if (newHousehold)
            {
                if (OccupiedHousing() + 1 > _definition.HousingCapacity) return Failure(request, eventId, "housing_full", "住房容量不足，迁入不会绕过住房状态。", D13GovernanceIds.LandRecoveryRoute);
                household = new D13HouseholdState { HouseholdId = request.HouseholdId, IslandId = _definition.IslandId, FamilyName = string.IsNullOrWhiteSpace(request.FamilyName) ? request.HouseholdId : request.FamilyName, HousingUnits = 1 };
            }
            var resident = new D13ResidentState
            {
                ResidentId = request.ResidentId, HouseholdId = request.HouseholdId, IslandId = _definition.IslandId, Role = request.Role,
                Lifecycle = D13ResidentLifecycle.Resident, Health = 100, Satisfaction = 70, EducationLevel = request.Role == D13ResidentRole.Child ? 0 : 1,
                ReligionId = string.IsNullOrWhiteSpace(request.ReligionId) ? _definition.DefaultReligionArrangementId : request.ReligionId,
                ReligionContentId = string.IsNullOrWhiteSpace(request.ReligionContentId) ? _definition.ReligionContentId : request.ReligionContentId,
                LastWorldDay = Math.Max(0, request.WorldDay),
            };
            household.MemberIds.Add(resident.ResidentId); TouchHousehold(household, eventId, request.WorldDay); TouchResident(resident, eventId, request.WorldDay);
            _households[household.HouseholdId] = household; _residents[resident.ResidentId] = resident;
            var fact = AppendEvent(eventId, request, "governance_migration_in", "居民在住房、产权、宗教审查和来源事实通过后迁入岛屿。", new Dictionary<string, string> { ["resident_id"] = resident.ResidentId, ["household_id"] = household.HouseholdId, ["new_household"] = newHousehold.ToString(), ["property_source_event_id"] = residence.PropertySourceEventId, ["legal_source_event_id"] = residence.LegalSourceEventId, ["religion_content_id"] = resident.ReligionContentId });
            _revision++; return Success(fact, resident, household, null, null, null, null);
        }

        public D13GovernanceResult MigrateOut(D13MigrationOutRequest request)
        {
            if (request == null) return Invalid("invalid_request", "迁出请求不存在。", D13GovernanceIds.LandRecoveryRoute);
            string eventId = EventId(request.OperationId, "migrate_out_" + request.ResidentId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.ResidentId);
            D13ResidentState resident; if (!_residents.TryGetValue(request.ResidentId ?? string.Empty, out resident) || resident.IslandId != _definition.IslandId) return Failure(request, eventId, "resident_missing", "居民不存在。", D13GovernanceIds.LandRecoveryRoute);
            resident.Lifecycle = D13ResidentLifecycle.Departed; resident.EmploymentStatus = D13EmploymentStatus.OnLeave; resident.OccupationId = null; resident.WagePerDay = 0; resident.WageDue = 0; TouchResident(resident, eventId, request.WorldDay);
            D13HouseholdState household; if (_households.TryGetValue(resident.HouseholdId, out household)) { household.MemberIds.Remove(resident.ResidentId); TouchHousehold(household, eventId, request.WorldDay); if (household.MemberIds.Count == 0) _households.Remove(household.HouseholdId); }
            var fact = AppendEvent(eventId, request, "governance_migration_out", "居民迁出，家庭和工作岗位投影同步释放。", new Dictionary<string, string> { ["resident_id"] = resident.ResidentId }); _revision++; return Success(fact, resident, household, null, null, null, null);
        }

        public D13GovernanceResult AssignJob(D13JobAssignmentRequest request)
        {
            if (request == null) return Invalid("invalid_request", "职业分配请求不存在。", D13GovernanceIds.EconomyRecoveryRoute);
            string eventId = EventId(request.OperationId, "assign_job_" + request.ResidentId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.ResidentId);
            D13ResidentState resident; D13GovernanceJobDefinition job; if (!TryActiveResident(request.ResidentId, out resident) || !_jobs.TryGetValue(request.JobId ?? string.Empty, out job)) return Failure(request, eventId, "job_or_resident_missing", "居民或岗位不存在。", D13GovernanceIds.EconomyRecoveryRoute);
            int occupied = _residents.Values.Count(item => item.Lifecycle == D13ResidentLifecycle.Resident && item.OccupationId == job.JobId && item.ResidentId != resident.ResidentId); if (occupied >= job.Slots) return Failure(request, eventId, "job_slots_full", "岗位已满，不能无因果超编。", D13GovernanceIds.EconomyRecoveryRoute, resident);
            if (resident.EducationLevel < job.RequiredEducation) return Failure(request, eventId, "education_required", "居民教育等级不足，先通过教育服务。", D13GovernanceIds.EconomyRecoveryRoute, resident);
            var wage = _economyAuthority.ApproveWage(resident.Clone(), job.Clone(), request.WorldDay, request.SaveRevision);
            if (wage == null || !wage.Allowed || wage.SourceOwnerSystem != D13GovernanceIds.EconomyOwner || wage.Amount != job.WagePerDay || !SourceExists(wage.SourceEventId)) return Failure(request, eventId, wage == null ? "economy_authority_required" : wage.Code, "工资必须由 EconomySystem 以来源事实裁决。", D13GovernanceIds.EconomyRecoveryRoute, resident);
            resident.OccupationId = job.JobId; resident.EmploymentStatus = D13EmploymentStatus.Employed; resident.WagePerDay = wage.Amount; resident.WageDue = wage.Amount; TouchResident(resident, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "governance_job_assigned", "岗位、教育门槛和工资来源已通过 authority，居民进入工作状态。", new Dictionary<string, string> { ["resident_id"] = resident.ResidentId, ["job_id"] = job.JobId, ["wage_per_day"] = wage.Amount.ToString(CultureInfo.InvariantCulture), ["wage_source_event_id"] = wage.SourceEventId }); _revision++; return Success(fact, resident, null, null, null, null, null);
        }

        public D13GovernanceResult AdvanceDay(D13GovernanceDayRequest request)
        {
            if (request == null) return Invalid("invalid_request", "治理日推进请求不存在。", D13GovernanceIds.ProvisionRecoveryRoute);
            string eventId = EventId(request.OperationId, "advance_day_" + request.WorldDay); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, null);
            var active = _residents.Values.Where(IsActive).ToList();
            int foodRequired = active.Count * _definition.FoodUnitsPerResident; int waterRequired = active.Count * _definition.WaterUnitsPerResident;
            var provision = _provisionAuthority.Provide(new D13GovernanceProvisionRequest { OperationId = eventId + "_provision", IslandId = _definition.IslandId, ResidentCount = active.Count, FoodRequired = foodRequired, WaterRequired = waterRequired, FoodRationingLevel = _policy.FoodRationingLevel, WaterRationingLevel = _policy.WaterRationingLevel, WorldDay = request.WorldDay, SaveRevision = request.SaveRevision });
            if (provision == null || !provision.Allowed || provision.FoodUnits < 0 || provision.WaterUnits < 0 || provision.FoodUnits > foodRequired || provision.WaterUnits > waterRequired || provision.FoodSourceOwnerSystem != D13GovernanceIds.EcologyOwner || provision.WaterSourceOwnerSystem != D13GovernanceIds.EcologyOwner || !SourceExists(provision.FoodSourceEventId) || !SourceExists(provision.WaterSourceEventId)) return Failure(request, eventId, provision == null ? "provision_authority_required" : provision.Code, "食物和供水必须来自带来源的 Ecology/资源 authority。", D13GovernanceIds.ProvisionRecoveryRoute);
            bool foodShort = provision.FoodUnits < foodRequired; bool waterShort = provision.WaterUnits < waterRequired;
            foreach (var resident in active)
            {
                if (foodShort) { resident.Health = Clamp(resident.Health - D13IslandGovernanceConfiguration.Int(D13IslandGovernanceConfiguration.FoodShortageHealthLoss)); resident.Satisfaction = Clamp(resident.Satisfaction - 6); }
                if (waterShort) { resident.Health = Clamp(resident.Health - D13IslandGovernanceConfiguration.Int(D13IslandGovernanceConfiguration.WaterShortageHealthLoss)); resident.Satisfaction = Clamp(resident.Satisfaction - 7); }
                if (resident.EmploymentStatus == D13EmploymentStatus.Unemployed) resident.Satisfaction = Clamp(resident.Satisfaction - D13IslandGovernanceConfiguration.Int(D13IslandGovernanceConfiguration.UnemploymentSatisfactionLoss));
                if (_policy.EducationOpen && resident.EducationLevel > 0) resident.Satisfaction = Clamp(resident.Satisfaction + D13IslandGovernanceConfiguration.Int(D13IslandGovernanceConfiguration.EducationSatisfactionGain));
                if (resident.Health <= D13IslandGovernanceConfiguration.Int(D13IslandGovernanceConfiguration.MinimumHealth)) { resident.Health = 0; resident.Lifecycle = D13ResidentLifecycle.Deceased; }
                TouchResident(resident, eventId, request.WorldDay);
            }
            int guardScore = _residents.Values.Where(IsActive).Sum(item => _jobs.ContainsKey(item.OccupationId ?? string.Empty) ? _jobs[item.OccupationId].SecurityContribution : 0);
            _securityScore = Clamp(_definition.SecurityBase + guardScore - (foodShort ? 8 : 0) - (waterShort ? 8 : 0) + (_policy.SecurityPostureId == "watch_strict" ? 6 : _policy.SecurityPostureId == "watch_reduced" ? -8 : 0) - _incidents.Values.Where(item => !item.Resolved).Sum(item => item.Severity * 10));
            int incidentRoll = Stable(_definition.IslandId + ":" + request.WorldDay + ":" + request.Seed) % 10000; if (incidentRoll < D13IslandGovernanceConfiguration.Int(D13IslandGovernanceConfiguration.SecurityIncidentThresholdBps) + (foodShort || waterShort ? 400 : 0))
            {
                string incidentId = "incident_" + _definition.IslandId + "_" + request.WorldDay; if (!_incidents.ContainsKey(incidentId)) { var incident = new D13GovernanceIncidentState { IncidentId = incidentId, IslandId = _definition.IslandId, Severity = foodShort && waterShort ? 3 : 1, WorldDay = Math.Max(0, request.WorldDay), Revision = 1 }; _incidents[incidentId] = incident; _securityScore = Clamp(_securityScore - incident.Severity * 10); }
            }
            _securityStatus = _securityScore >= 75 ? D13SecurityStatus.Stable : _securityScore >= 50 ? D13SecurityStatus.Tense : _securityScore >= 25 ? D13SecurityStatus.Unsafe : D13SecurityStatus.Crisis;
            var record = new D13GovernanceProvisionRecord { RecordId = "provision_" + SafeId(request.OperationId) + "_" + request.SaveRevision.ToString(CultureInfo.InvariantCulture), IslandId = _definition.IslandId, ResidentCount = active.Count, FoodRequired = foodRequired, FoodGranted = provision.FoodUnits, WaterRequired = waterRequired, WaterGranted = provision.WaterUnits, FoodSourceEventId = provision.FoodSourceEventId, WaterSourceEventId = provision.WaterSourceEventId, WorldDay = Math.Max(0, request.WorldDay), SaveRevision = Math.Max(0, request.SaveRevision) };
            _provisions[record.RecordId] = record; TouchGlobal(eventId, request.WorldDay); var fact = AppendEvent(eventId, request, "governance_day_advanced", "住房、食物、供水、工作、教育和治安共同影响居民状态；资源数量来自外部 authority。", new Dictionary<string, string> { ["resident_count"] = active.Count.ToString(CultureInfo.InvariantCulture), ["food_required"] = foodRequired.ToString(CultureInfo.InvariantCulture), ["food_granted"] = provision.FoodUnits.ToString(CultureInfo.InvariantCulture), ["water_required"] = waterRequired.ToString(CultureInfo.InvariantCulture), ["water_granted"] = provision.WaterUnits.ToString(CultureInfo.InvariantCulture), ["security_score"] = _securityScore.ToString(CultureInfo.InvariantCulture), ["security_status"] = _securityStatus.ToString(), ["food_source_event_id"] = provision.FoodSourceEventId, ["water_source_event_id"] = provision.WaterSourceEventId }); _revision++; return Success(fact, null, null, _policy, record, null, null);
        }

        public D13GovernanceResult AssessTax(D13GovernanceTaxRequest request)
        {
            if (request == null) return Invalid("invalid_request", "税费评估请求不存在。", D13GovernanceIds.EconomyRecoveryRoute);
            string eventId = EventId(request.OperationId, "assess_tax_" + request.HouseholdId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, null);
            D13HouseholdState household; if (!_households.TryGetValue(request.HouseholdId ?? string.Empty, out household) || household.IslandId != _definition.IslandId) return Failure(request, eventId, "household_missing", "家庭不存在。", D13GovernanceIds.EconomyRecoveryRoute);
            if (request.TaxBase < 0) return Failure(request, eventId, "tax_base_invalid", "税基不能为负数。", D13GovernanceIds.EconomyRecoveryRoute);
            var decision = _economyAuthority.AssessTax(household.HouseholdId, request.TaxBase, _policy.TaxRateBps, request.WorldDay, request.SaveRevision);
            if (decision == null || !decision.Allowed || decision.SourceOwnerSystem != D13GovernanceIds.EconomyOwner || decision.AmountDue < 0 || decision.AmountCollected < 0 || decision.AmountCollected > decision.AmountDue || !SourceExists(decision.SourceEventId)) return Failure(request, eventId, "invalid_tax_decision", "税费必须由 EconomySystem 按当前政策和来源事实裁决。", D13GovernanceIds.EconomyRecoveryRoute);
            var record = new D13GovernanceTaxRecord { AssessmentId = eventId, HouseholdId = household.HouseholdId, TaxBase = request.TaxBase, TaxRateBps = _policy.TaxRateBps, AmountDue = decision.AmountDue, AmountCollected = decision.AmountCollected, SourceEventId = decision.SourceEventId, WorldDay = Math.Max(0, request.WorldDay), SaveRevision = Math.Max(0, request.SaveRevision) }; _taxes[record.AssessmentId] = record;
            if (decision.AmountCollected < decision.AmountDue) foreach (var id in household.MemberIds) { D13ResidentState resident; if (_residents.TryGetValue(id, out resident) && IsActive(resident)) { resident.Satisfaction = Clamp(resident.Satisfaction - 2); TouchResident(resident, eventId, request.WorldDay); } }
            var fact = AppendEvent(eventId, request, "governance_tax_assessed", "税率来自治理政策，税基和收缴结果由 EconomySystem 裁决；治理系统只保存评估事实。", new Dictionary<string, string> { ["household_id"] = household.HouseholdId, ["tax_base"] = request.TaxBase.ToString(CultureInfo.InvariantCulture), ["tax_rate_bps"] = _policy.TaxRateBps.ToString(CultureInfo.InvariantCulture), ["amount_due"] = decision.AmountDue.ToString(CultureInfo.InvariantCulture), ["amount_collected"] = decision.AmountCollected.ToString(CultureInfo.InvariantCulture), ["economy_source_event_id"] = decision.SourceEventId }); _revision++; return Success(fact, null, household, null, null, record, null);
        }

        public D13GovernanceResult PayWage(D13GovernanceWageRequest request)
        {
            if (request == null) return Invalid("invalid_request", "工资支付请求不存在。", D13GovernanceIds.EconomyRecoveryRoute);
            string eventId = EventId(request.OperationId, "pay_wage_" + request.ResidentId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.ResidentId);
            D13ResidentState resident; D13GovernanceJobDefinition job; if (!TryActiveResident(request.ResidentId, out resident) || string.IsNullOrWhiteSpace(resident.OccupationId) || !_jobs.TryGetValue(resident.OccupationId, out job) || resident.WageDue <= 0) return Failure(request, eventId, "wage_not_due", "居民没有可支付的登记工资。", D13GovernanceIds.EconomyRecoveryRoute, resident);
            var decision = _economyAuthority.ApproveWage(resident.Clone(), job.Clone(), request.WorldDay, request.SaveRevision); if (decision == null || !decision.Allowed || decision.SourceOwnerSystem != D13GovernanceIds.EconomyOwner || decision.Amount != resident.WageDue || string.IsNullOrWhiteSpace(decision.LedgerRef) || !SourceExists(decision.SourceEventId)) return Failure(request, eventId, "invalid_wage_decision", "工资金额和账本必须由 EconomySystem 裁决。", D13GovernanceIds.EconomyRecoveryRoute, resident);
            var record = new D13GovernanceWageRecord { PaymentId = eventId, ResidentId = resident.ResidentId, Amount = decision.Amount, Currency = decision.Currency, SourceEventId = decision.SourceEventId, LedgerRef = decision.LedgerRef, WorldDay = Math.Max(0, request.WorldDay), SaveRevision = Math.Max(0, request.SaveRevision) }; _wages[record.PaymentId] = record; resident.WageDue = 0; resident.Satisfaction = Clamp(resident.Satisfaction + 3); TouchResident(resident, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "governance_wage_settled", "工资由 EconomySystem 通过账本裁决并记录，治理系统不直接改金钱。", new Dictionary<string, string> { ["resident_id"] = resident.ResidentId, ["amount"] = record.Amount.ToString(CultureInfo.InvariantCulture), ["ledger_ref"] = record.LedgerRef, ["economy_source_event_id"] = record.SourceEventId }); _revision++; return Success(fact, resident, null, null, null, null, record);
        }

        public D13GovernanceResult ApplyPolicy(D13GovernancePolicyRequest request)
        {
            if (request == null) return Invalid("invalid_request", "政策请求不存在。", D13GovernanceIds.PolicyRecoveryRoute);
            string eventId = EventId(request.OperationId, "policy_" + request.IslandId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, null);
            if (!ValidIsland(request.IslandId)) return Failure(request, eventId, "island_unknown", "未知岛屿不能应用政策。", D13GovernanceIds.PolicyRecoveryRoute);
            var decision = _policyAuthority.ApprovePolicy(_definition.Clone(), request, _policy.Clone()); if (decision == null || !decision.Allowed || (decision.SourceOwnerSystem != D13GovernanceIds.PropertyOwner && decision.SourceOwnerSystem != D13GovernanceIds.LegalOwner) || decision.TaxRateBps < 0 || decision.TaxRateBps > 5000 || decision.WaterRationingLevel < 0 || decision.WaterRationingLevel > 2 || decision.FoodRationingLevel < 0 || decision.FoodRationingLevel > 2 || string.IsNullOrWhiteSpace(decision.ReligionArrangementId) || string.IsNullOrWhiteSpace(decision.SecurityPostureId) || !SourceExists(decision.SourceEventId)) return Failure(request, eventId, "invalid_policy_decision", "税、供给、教育、宗教和治安政策必须由法律/产权 authority 批准。", D13GovernanceIds.PolicyRecoveryRoute);
            _policy = new D13GovernancePolicyState { TaxRateBps = decision.TaxRateBps, WaterRationingLevel = decision.WaterRationingLevel, FoodRationingLevel = decision.FoodRationingLevel, EducationOpen = decision.EducationOpen, ReligionArrangementId = decision.ReligionArrangementId, SecurityPostureId = decision.SecurityPostureId, SourceEventId = decision.SourceEventId, LastUpdatedDay = Math.Max(0, request.WorldDay), Revision = _policy.Revision + 1 }; TouchGlobal(eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "governance_policy_applied", "治理政策由外部法律/产权 authority 批准后写入政策状态。", new Dictionary<string, string> { ["tax_rate_bps"] = _policy.TaxRateBps.ToString(CultureInfo.InvariantCulture), ["water_rationing_level"] = _policy.WaterRationingLevel.ToString(CultureInfo.InvariantCulture), ["food_rationing_level"] = _policy.FoodRationingLevel.ToString(CultureInfo.InvariantCulture), ["education_open"] = _policy.EducationOpen.ToString(), ["religion_arrangement_id"] = _policy.ReligionArrangementId, ["security_posture_id"] = _policy.SecurityPostureId, ["policy_source_event_id"] = _policy.SourceEventId }); _revision++; return Success(fact, null, null, _policy, null, null, null);
        }

        public D13GovernancePolicyProposal ProposePolicy(D13GovernancePolicyProposal proposal)
        {
            if (proposal == null || string.IsNullOrWhiteSpace(proposal.ProposalId) || !ValidIsland(proposal.IslandId)) return null;
            var result = proposal.Clone(); result.ProposedByAgent = true; return result;
        }

        public D13GovernanceResult EnrollEducation(D13EducationRequest request)
        {
            if (request == null) return Invalid("invalid_request", "教育请求不存在。", D13GovernanceIds.PolicyRecoveryRoute);
            string eventId = EventId(request.OperationId, "education_" + request.ResidentId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.ResidentId);
            D13ResidentState resident; if (!TryActiveResident(request.ResidentId, out resident)) return Failure(request, eventId, "resident_missing", "居民不存在。", D13GovernanceIds.PolicyRecoveryRoute);
            if (!_policy.EducationOpen || _definition.EducationCapacity <= 0) return Failure(request, eventId, "education_closed", "当前教育政策或设施容量不允许入学。", D13GovernanceIds.PolicyRecoveryRoute, resident);
            if (resident.EducationLevel >= 3) return Failure(request, eventId, "education_complete", "居民教育等级已经达到本切片上限。", D13GovernanceIds.PolicyRecoveryRoute, resident);
            resident.EducationLevel++; resident.Satisfaction = Clamp(resident.Satisfaction + 4); TouchResident(resident, eventId, request.WorldDay); var fact = AppendEvent(eventId, request, "governance_education_enrolled", "教育服务提升居民教育等级和满意度；教育政策由治理状态控制。", new Dictionary<string, string> { ["resident_id"] = resident.ResidentId, ["education_level"] = resident.EducationLevel.ToString(CultureInfo.InvariantCulture) }); _revision++; return Success(fact, resident, null, null, null, null, null);
        }

        public D13GovernanceResult PracticeReligion(D13ReligionPracticeRequest request)
        {
            if (request == null) return Invalid("invalid_request", "宗教实践请求不存在。", D13GovernanceIds.ReligionRecoveryRoute);
            string eventId = EventId(request.OperationId, "religion_" + request.ResidentId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.ResidentId);
            D13ResidentState resident; if (!TryActiveResident(request.ResidentId, out resident)) return Failure(request, eventId, "resident_missing", "居民不存在。", D13GovernanceIds.ReligionRecoveryRoute);
            if (_definition.ReligionContent.ReviewStatus == D13GovernanceContentReviewStatus.Blocked || _definition.ReligionContent.ReviewStatus == D13GovernanceContentReviewStatus.PendingReview || (_definition.ReligionContent.ReviewStatus == D13GovernanceContentReviewStatus.ApprovedWithWarning && !request.ReviewAcknowledged)) return Failure(request, eventId, "religion_review_required", "宗教内容审查警告尚未确认。", D13GovernanceIds.ReligionRecoveryRoute, resident);
            if (!string.IsNullOrWhiteSpace(request.ReligionContentId) && request.ReligionContentId != _definition.ReligionContentId) return Failure(request, eventId, "religion_content_mismatch", "宗教内容 ID 与当前审查目录不一致。", D13GovernanceIds.ReligionRecoveryRoute, resident);
            resident.ReligionId = string.IsNullOrWhiteSpace(request.ReligionId) ? _definition.DefaultReligionArrangementId : request.ReligionId; resident.ReligionContentId = _definition.ReligionContentId; resident.FaithPracticeCount++; resident.Satisfaction = Clamp(resident.Satisfaction + 2); TouchResident(resident, eventId, request.WorldDay); var fact = AppendEvent(eventId, request, "governance_religion_practiced", "宗教实践只记录经过内容审查的语境事实，不把信仰简化为善恶或阵营。", new Dictionary<string, string> { ["resident_id"] = resident.ResidentId, ["religion_id"] = resident.ReligionId, ["religion_content_id"] = resident.ReligionContentId, ["review_status"] = _definition.ReligionContent.ReviewStatus.ToString() }); _revision++; return Success(fact, resident, null, null, null, null, null);
        }

        public D13GovernanceResult ApplyCommunityAgreement(D13GovernanceAgreementRequest request)
        {
            if (request == null) return Invalid("invalid_request", "共同体协议请求不存在。", D13GovernanceIds.CommunityRecoveryRoute);
            string eventId = EventId(request.OperationId, "agreement_" + request.AgreementId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, null);
            var decision = _communityAuthority.ValidateAgreement(_definition.Clone(), request, request.WorldDay); if (decision == null || !decision.Allowed || decision.SourceOwnerSystem != D13GovernanceIds.CommunityOwner || string.IsNullOrWhiteSpace(decision.AgreementId) || decision.Content == null || decision.Content.ReviewStatus == D13GovernanceContentReviewStatus.Blocked || decision.Content.ReviewStatus == D13GovernanceContentReviewStatus.PendingReview || (decision.Content.ReviewStatus == D13GovernanceContentReviewStatus.ApprovedWithWarning && !request.ReviewAcknowledged) || !SourceExists(decision.SourceEventId)) return Failure(request, eventId, decision == null ? "community_authority_required" : decision.Code, "共同体协议必须有社群 owner、审查状态和统一事实来源。", D13GovernanceIds.CommunityRecoveryRoute);
            var agreement = new D13GovernanceAgreementState { AgreementId = decision.AgreementId, CommunityId = decision.CommunityId, Status = decision.Status, Content = decision.Content.Clone(), SourceEventId = decision.SourceEventId, LastUpdatedDay = Math.Max(0, request.WorldDay), Revision = 1 }; _agreements[agreement.AgreementId] = agreement; var fact = AppendEvent(eventId, request, "governance_community_agreement_applied", "共同体协议写入治理投影；社群库存和声望仍由 CommunityTradeSystem 管理。", new Dictionary<string, string> { ["agreement_id"] = agreement.AgreementId, ["community_id"] = agreement.CommunityId, ["agreement_status"] = agreement.Status, ["community_source_event_id"] = agreement.SourceEventId, ["content_review_status"] = agreement.Content.ReviewStatus.ToString() }); _revision++; return Success(fact, null, null, null, null, null, null, agreement);
        }

        public D13GovernanceResult ManageAgent(D13GovernanceAgentRequest request)
        {
            if (request == null) return Invalid("invalid_request", "代理人管理请求不存在。", D13GovernanceIds.AgentRecoveryRoute);
            string eventId = EventId(request.OperationId, "agent_" + request.AgentRef); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.ResidentId);
            if (!string.IsNullOrWhiteSpace(request.ResidentId) && !TryActiveResident(request.ResidentId, out D13ResidentState resident)) return Failure(request, eventId, "resident_missing", "代理人管理引用的居民不存在。", D13GovernanceIds.AgentRecoveryRoute);
            var decision = _agentAuthority.ApproveManagement(_definition.Clone(), request, request.WorldDay); if (decision == null || !decision.Allowed || decision.SourceOwnerSystem != D13GovernanceIds.AgentOwner || string.IsNullOrWhiteSpace(decision.AgentRef) || string.IsNullOrWhiteSpace(decision.ManagementRole) || !SourceExists(decision.SourceEventId)) return Failure(request, eventId, "invalid_agent_decision", "代理人管理必须由 AgentManagementSystem 返回来源化决定；Agent 不能自我任命。", D13GovernanceIds.AgentRecoveryRoute);
            var assignment = new D13GovernanceAgentAssignment { AssignmentId = eventId, AgentRef = decision.AgentRef, IslandId = _definition.IslandId, ResidentId = request.ResidentId, ManagementRole = decision.ManagementRole, SourceEventId = decision.SourceEventId, WorldDay = Math.Max(0, request.WorldDay), Revision = 1 }; _agents[assignment.AssignmentId] = assignment; var fact = AppendEvent(eventId, request, "governance_agent_managed", "代理人管理结果由外部 authority 裁决，治理系统只保存岛屿管理分配投影。", new Dictionary<string, string> { ["agent_ref"] = assignment.AgentRef, ["management_role"] = assignment.ManagementRole, ["agent_source_event_id"] = assignment.SourceEventId }); _revision++; return Success(fact, null, null, null, null, null, null, null, null, assignment);
        }

        public D13GovernanceResult ResolveSecurityIncident(D13SecurityIncidentRequest request)
        {
            if (request == null) return Invalid("invalid_request", "治安事件请求不存在。", D13GovernanceIds.SecurityRecoveryRoute);
            string eventId = EventId(request.OperationId, "security_resolve_" + request.IncidentId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, null);
            D13GovernanceIncidentState incident; if (!_incidents.TryGetValue(request.IncidentId ?? string.Empty, out incident)) return Failure(request, eventId, "incident_missing", "治安事件不存在。", D13GovernanceIds.SecurityRecoveryRoute);
            if (incident.Resolved) return Failure(request, eventId, "incident_resolved", "治安事件已经解决。", D13GovernanceIds.SecurityRecoveryRoute);
            if (string.IsNullOrWhiteSpace(request.ResolutionId)) return Failure(request, eventId, "resolution_required", "治安恢复必须提供可追踪的处理路线。", D13GovernanceIds.SecurityRecoveryRoute);
            incident.Resolved = true; incident.ResolutionId = request.ResolutionId; incident.Revision++; incident.WorldDay = Math.Max(incident.WorldDay, request.WorldDay); _securityScore = Clamp(_securityScore + incident.Severity * 8); _securityStatus = _securityScore >= 75 ? D13SecurityStatus.Stable : _securityScore >= 50 ? D13SecurityStatus.Tense : _securityScore >= 25 ? D13SecurityStatus.Unsafe : D13SecurityStatus.Crisis; TouchGlobal(eventId, request.WorldDay); var fact = AppendEvent(eventId, request, "governance_security_incident_resolved", "治安事件按明确恢复路线解决，安全评分由 IslandGovernanceSystem 更新。", new Dictionary<string, string> { ["incident_id"] = incident.IncidentId, ["resolution_id"] = incident.ResolutionId, ["security_score"] = _securityScore.ToString(CultureInfo.InvariantCulture) }); _revision++; return Success(fact, null, null, null, null, null, null, null, incident, null);
        }

        private bool ValidIsland(string islandId) { return string.Equals(_definition.IslandId, islandId, StringComparison.Ordinal); }
        private bool IsActive(D13ResidentState resident) { return resident != null && resident.IslandId == _definition.IslandId && resident.Lifecycle == D13ResidentLifecycle.Resident; }
        private bool TryActiveResident(string residentId, out D13ResidentState resident) { if (_residents.TryGetValue(residentId ?? string.Empty, out resident) && IsActive(resident)) return true; resident = null; return false; }
        private int OccupiedHousing() { return _households.Values.Sum(item => item.HousingUnits); }
        private bool SourceExists(string eventId) { return !string.IsNullOrWhiteSpace(eventId) && FindAnyFact(eventId) != null; }
        private FactEvent FindAnyFact(string eventId) { if (string.IsNullOrWhiteSpace(eventId)) return null; return _eventLog.Facts.FirstOrDefault(item => item != null && item.EventId == eventId); }
        private FactEvent Existing(string eventId, long revision) { string key = (eventId ?? string.Empty) + "\u001f" + Math.Max(0L, revision).ToString(CultureInfo.InvariantCulture); var fact = _eventLog.Find(eventId, Math.Max(0L, revision)); if (fact != null) return fact; return _appliedEventKeys.Contains(key) ? new FactEvent { EventId = eventId, SaveRevision = Math.Max(0L, revision), Result = new EventResult { ResultCode = "duplicate", Summary = "治理操作已经在此前存档中结算。" } } : null; }
        private FactEvent AppendEvent(string eventId, D13GovernanceOperationRequest request, string resultCode, string summary, Dictionary<string, string> values, WorldEventStatus status = WorldEventStatus.Normal, string recoveryRoute = null)
        {
            var causeIds = (request == null || request.CauseEventIds == null ? Enumerable.Empty<string>() : request.CauseEventIds).Where(item => !string.IsNullOrWhiteSpace(item) && FindAnyFact(item) != null).Distinct(StringComparer.Ordinal).ToArray();
            var route = string.IsNullOrWhiteSpace(recoveryRoute) ? null : new RecoveryRoute { RouteId = recoveryRoute, Action = "review_and_retry", ResumeFromEventId = eventId, Description = "由对应治理 authority 补充事实后重试。" };
            var worldEvent = new WorldEvent(eventId, Math.Max(0, request == null ? 0 : request.WorldDay), _definition.IslandId, D13GovernanceIds.SystemOwner, new EventResult { ResultCode = resultCode, Summary = summary, FactIds = new List<string> { "governance_fact:" + eventId }, Values = values ?? new Dictionary<string, string>() }, new[] { request == null || string.IsNullOrWhiteSpace(request.ActorRef) ? D13GovernanceIds.PlayerRef : request.ActorRef }, new[] { "governance:island" }, causeIds, request == null ? 0 : Math.Max(0, request.SaveRevision), status, route);
            var fact = _eventLog.Append(worldEvent).Fact; _appliedEventKeys.Add(worldEvent.DedupeKey); _lastEventId = eventId; _lastWorldDay = Math.Max(_lastWorldDay, worldEvent.WorldDay); return fact;
        }
        private string EventId(string operationId, string fallback) { return "event_d13_governance_" + (string.IsNullOrWhiteSpace(operationId) ? fallback : operationId); }
        private void TouchResident(D13ResidentState state, string eventId, int day) { state.Revision++; state.LastEventId = eventId; state.LastWorldDay = Math.Max(state.LastWorldDay, Math.Max(0, day)); }
        private void TouchHousehold(D13HouseholdState state, string eventId, int day) { state.Revision++; state.LastEventId = eventId; state.LastUpdatedDay = Math.Max(state.LastUpdatedDay, Math.Max(0, day)); }
        private void TouchGlobal(string eventId, int day) { _lastEventId = eventId; _lastWorldDay = Math.Max(_lastWorldDay, Math.Max(0, day)); }
        private static int Clamp(int value) { return Math.Max(0, Math.Min(100, value)); }
        private static int Stable(string value) { unchecked { uint hash = 2166136261u; foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u; return (int)(hash & 0x7fffffff); } }
        private static string SafeId(string value) { if (string.IsNullOrWhiteSpace(value)) return "none"; var chars = value.ToCharArray(); for (int i = 0; i < chars.Length; i++) if (!(char.IsLetterOrDigit(chars[i]) || chars[i] == '_' || chars[i] == '-')) chars[i] = '_'; return new string(chars); }

        private D13GovernanceResult Success(FactEvent fact, D13ResidentState resident, D13HouseholdState household, D13GovernancePolicyState policy, D13GovernanceProvisionRecord provision, D13GovernanceTaxRecord tax, D13GovernanceWageRecord wage, D13GovernanceAgreementState agreement = null, D13GovernanceIncidentState incident = null, D13GovernanceAgentAssignment agent = null)
        {
            return new D13GovernanceResult { Succeeded = true, Code = fact == null || fact.Result == null ? "ok" : fact.Result.ResultCode, Message = fact == null || fact.Result == null ? string.Empty : fact.Result.Summary, EventId = fact == null ? string.Empty : fact.EventId, Resident = resident == null ? null : resident.Clone(), Household = household == null ? null : household.Clone(), Policy = policy == null ? null : policy.Clone(), Provision = provision == null ? null : provision.Clone(), Tax = tax == null ? null : tax.Clone(), Wage = wage == null ? null : wage.Clone(), Agreement = agreement == null ? null : agreement.Clone(), Incident = incident == null ? null : incident.Clone(), AgentAssignment = agent == null ? null : agent.Clone(), SecurityScore = _securityScore, SecurityStatus = _securityStatus, Fact = fact == null ? null : fact.CloneFact() };
        }
        private D13GovernanceResult Failure(D13GovernanceOperationRequest request, string eventId, string code, string message, string route, D13ResidentState resident = null) { var fact = AppendEvent(eventId, request, code ?? "governance_failed", message ?? "治理操作失败。", new Dictionary<string, string> { ["failure_code"] = code ?? "governance_failed", ["recovery_route"] = route ?? D13GovernanceIds.PolicyRecoveryRoute }, WorldEventStatus.Failed, route); _revision++; return new D13GovernanceResult { Succeeded = false, Code = code, Message = message, EventId = eventId, RecoveryRouteId = route, Resident = resident == null ? null : resident.Clone(), Fact = fact }; }
        private D13GovernanceResult Invalid(string code, string message, string route) { return new D13GovernanceResult { Succeeded = false, Code = code, Message = message, RecoveryRouteId = route }; }
        private D13GovernanceResult Duplicate(FactEvent fact, string residentId) { return new D13GovernanceResult { Succeeded = true, Duplicate = true, Code = "duplicate", Message = "治理操作已结算，未重复改变人口、税、工资或治安。", EventId = fact == null ? string.Empty : fact.EventId, Resident = FindResident(residentId), Fact = fact == null ? null : fact.CloneFact(), SecurityScore = _securityScore, SecurityStatus = _securityStatus }; }

        private sealed class UnavailableResidenceAuthority : ID13GovernanceResidenceAuthority { public D13GovernanceResidenceDecision ValidateResidence(D13IslandGovernanceDefinition definition, D13MigrationInRequest request, int worldDay) { return new D13GovernanceResidenceDecision { Allowed = false, Code = "residence_authority_required", Message = "必须由 Property/法律 authority 提供居住资格。" }; } }
        private sealed class UnavailableProvisionAuthority : ID13GovernanceProvisionAuthority { public D13GovernanceProvisionDecision Provide(D13GovernanceProvisionRequest request) { return new D13GovernanceProvisionDecision { Allowed = false, Code = "provision_authority_required", Message = "必须由 Ecology/资源 authority 提供食物和供水结果。" }; } }
        private sealed class UnavailableEconomyAuthority : ID13GovernanceEconomyAuthority { public D13GovernanceWageDecision ApproveWage(D13ResidentState resident, D13GovernanceJobDefinition job, int worldDay, long saveRevision) { return new D13GovernanceWageDecision { Allowed = false, Code = "economy_authority_required" }; } public D13GovernanceTaxDecision AssessTax(string householdId, int taxBase, int taxRateBps, int worldDay, long saveRevision) { return new D13GovernanceTaxDecision { Allowed = false, Code = "economy_authority_required" }; } }
        private sealed class UnavailablePolicyAuthority : ID13GovernancePolicyAuthority { public D13GovernancePolicyDecision ApprovePolicy(D13IslandGovernanceDefinition definition, D13GovernancePolicyRequest request, D13GovernancePolicyState currentState) { return new D13GovernancePolicyDecision { Allowed = false, Code = "policy_authority_required" }; } }
        private sealed class UnavailableCommunityAuthority : ID13GovernanceCommunityAuthority { public D13GovernanceAgreementDecision ValidateAgreement(D13IslandGovernanceDefinition definition, D13GovernanceAgreementRequest request, int worldDay) { return new D13GovernanceAgreementDecision { Allowed = false, Code = "community_authority_required" }; } }
        private sealed class UnavailableAgentAuthority : ID13GovernanceAgentAuthority { public D13GovernanceAgentDecision ApproveManagement(D13IslandGovernanceDefinition definition, D13GovernanceAgentRequest request, int worldDay) { return new D13GovernanceAgentDecision { Allowed = false, Code = "agent_authority_required" }; } }
    }
}
