using System;
using Whalebone.Game.Config;

namespace Whalebone.Systems.D4
{
    public sealed class HealthResult
    {
        public bool Accepted;
        public bool DiseaseAdded;
        public int Damage;
        public int SuppliesUsed;
        public string DiseaseId;
        public string Message;
    }

    /// <summary>船员健康、疾病发作、伤势和医疗资源的唯一裁决者。</summary>
    public sealed class D4SurvivalSystem
    {
        private readonly HealthState _state;

        public D4SurvivalSystem(HealthState initial = null)
        {
            _state = initial == null ? new HealthState() : initial.Clone();
        }

        public HealthState Snapshot => _state.Clone();

        public void EnsureCrew(string crewId)
        {
            if (string.IsNullOrEmpty(crewId) || Find(crewId) != null) return;
            _state.Crew.Add(new CrewHealthState
            {
                CrewId = crewId,
                Health = (int)NumericConfiguration.Default.Health.DefaultValue,
                Fatigue = (int)NumericConfiguration.Default.Fatigue.DefaultValue
            });
        }

        public HealthState CreateInitial(string[] crewIds, int medicalSupplies)
        {
            _state.Crew.Clear();
            _state.MedicalSupplies = Math.Max(0, medicalSupplies);
            if (crewIds != null) foreach (var id in crewIds) EnsureCrew(id);
            return Snapshot;
        }

        public void ApplyInjury(string crewId, int damage)
        {
            var crew = Find(crewId);
            if (crew == null) return;
            crew.Health = (int)NumericConfiguration.ClampHealth(crew.Health - Math.Max(0, damage));
        }

        public HealthResult TickDay(bool freshFood, bool mosquitoZone, bool pollutedWater, int seed)
        {
            HealthResult last = new HealthResult { Accepted = true, Message = "健康状态稳定。" };
            foreach (var crew in _state.Crew)
            {
                crew.Fatigue = (int)NumericConfiguration.ClampFatigue(crew.Fatigue + (freshFood ? 2 : 6));
                foreach (var disease in D4DiseaseCatalog.All)
                {
                    bool sourceActive = disease.Id == "scurvy" ? !freshFood :
                        disease.Id == "malaria" || disease.Id == "yellow_fever" ? mosquitoZone :
                        disease.Id == "dysentery" ? pollutedWater : crew.Health < 45;
                    if (sourceActive && !HasDisease(crew, disease.Id) && Roll(disease.Id + ":" + crew.CrewId + ":" + seed) < disease.DailyRisk)
                    {
                        crew.Diseases.Add(disease.Id);
                        last = new HealthResult { Accepted = true, DiseaseAdded = true, DiseaseId = disease.Id, Message = disease.Name + "开始发作。" };
                    }
                    if (HasDisease(crew, disease.Id))
                    {
                        crew.Health = (int)NumericConfiguration.ClampHealth(crew.Health - disease.DailyDamage);
                        last.Damage += disease.DailyDamage;
                    }
                }
            }
            return last;
        }

        public HealthResult Infect(string crewId, string diseaseId)
        {
            var crew = Find(crewId);
            var disease = D4DiseaseCatalog.Find(diseaseId);
            if (crew == null || disease == null) return new HealthResult { Message = "船员或疾病无效。" };
            if (!HasDisease(crew, disease.Id)) crew.Diseases.Add(disease.Id);
            return new HealthResult { Accepted = true, DiseaseAdded = true, DiseaseId = disease.Id, Message = disease.Name + "已记录。" };
        }

        public HealthResult Treat(string crewId, string diseaseId, int medicineSkill)
        {
            var crew = Find(crewId);
            var disease = D4DiseaseCatalog.Find(diseaseId);
            if (crew == null || disease == null) return new HealthResult { Message = "船员或疾病无效。" };
            if (!HasDisease(crew, disease.Id)) return new HealthResult { Message = "该船员没有这种疾病。" };
            int cost = disease.TreatmentCost;
            if (_state.MedicalSupplies < cost) return new HealthResult { Message = "医疗物资不足。" };
            _state.MedicalSupplies -= cost;
            int recovery = disease.TreatmentPower + Math.Max(0, medicineSkill) * 3;
            crew.Health = (int)NumericConfiguration.ClampHealth(crew.Health + recovery);
            if (recovery >= disease.TreatmentPower) crew.Diseases.Remove(disease.Id);
            return new HealthResult { Accepted = true, SuppliesUsed = cost, DiseaseId = disease.Id, Message = disease.Name + "治疗完成。" };
        }

        private CrewHealthState Find(string crewId)
        {
            foreach (var crew in _state.Crew) if (crew.CrewId == crewId) return crew;
            return null;
        }

        private static bool HasDisease(CrewHealthState crew, string diseaseId) { return crew.Diseases.Contains(diseaseId); }

        private static int Roll(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash % 100u);
            }
        }
    }
}
