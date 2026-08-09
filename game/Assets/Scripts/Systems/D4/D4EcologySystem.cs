using System;

namespace Whalebone.Systems.D4
{
    public sealed class WildlifeResult
    {
        public bool Accepted;
        public bool Dangerous;
        public int Damage;
        public int CollectionValue;
        public string DiseaseId;
        public string Message;
    }

    /// <summary>动物栖息地与可预判行为状态机的唯一裁决者。</summary>
    public sealed class D4EcologySystem
    {
        private readonly EcologyState _state;

        public D4EcologySystem(EcologyState initial = null)
        {
            _state = initial == null ? new EcologyState() : initial.Clone();
        }

        public EcologyState Snapshot => _state.Clone();

        public void SeedHabitat(string habitatId, int day, int seed)
        {
            _state.HabitatId = string.IsNullOrEmpty(habitatId) ? "jungle" : habitatId;
            _state.Animals.Clear();
            foreach (var definition in D4AnimalCatalog.All)
            {
                if (definition.Habitat != _state.HabitatId && !(definition.Marine && _state.HabitatId == "open_sea")) continue;
                _state.Animals.Add(new AnimalState
                {
                    AnimalId = definition.Id,
                    Behavior = BehaviorFor(definition, day, seed),
                    Health = 100,
                    CollectionValue = definition.CollectionValue,
                });
            }
        }

        public void Tick(int day, int seed, bool bloodInWater)
        {
            foreach (var animal in _state.Animals)
            {
                var definition = D4AnimalCatalog.Find(animal.AnimalId);
                if (definition == null) continue;
                animal.Behavior = BehaviorFor(definition, day, seed + _state.CollectionCount);
                if (bloodInWater && definition.Id == "shark") animal.Behavior = D4AnimalBehavior.Attacking;
                if (animal.Behavior == D4AnimalBehavior.Attacking) animal.Health = Math.Max(0, animal.Health - 1);
            }
        }

        public WildlifeResult Observe(string animalId)
        {
            var animal = Find(animalId);
            if (animal == null) return new WildlifeResult { Message = "没有发现这种动物。" };
            animal.Observed = true;
            var definition = D4AnimalCatalog.Find(animalId);
            return new WildlifeResult { Accepted = true, CollectionValue = definition == null ? 0 : definition.CollectionValue, Message = "已记录动物观察。" };
        }

        public WildlifeResult Collect(string animalId)
        {
            var animal = Find(animalId);
            var definition = D4AnimalCatalog.Find(animalId);
            if (animal == null || definition == null) return new WildlifeResult { Message = "没有可收集的动物记录。" };
            if (!animal.Observed) return new WildlifeResult { Message = "请先观察并确认物种。" };
            if (definition.CollectionValue <= 0) return new WildlifeResult { Message = "这种动物只能观察，不能收集。" };
            if (_state.CollectionCount >= 3) return new WildlifeResult { Message = "本次地点的收藏额度已满。" };
            _state.CollectionCount++;
            return new WildlifeResult { Accepted = true, CollectionValue = definition.CollectionValue, Message = "已获得动物标本/收藏记录。" };
        }

        public WildlifeResult ResolveDanger(string animalId, int protection, int seed)
        {
            var definition = D4AnimalCatalog.Find(animalId);
            if (definition == null) return new WildlifeResult { Message = "未知动物。" };
            if (definition.Danger <= 0) return new WildlifeResult { Accepted = true, Message = "动物没有主动攻击。" };
            int dangerRoll = Stable(definition.Id + ":" + seed) % 100;
            if (dangerRoll + protection >= definition.Danger + 20)
                return new WildlifeResult { Accepted = true, Dangerous = true, Message = "装备和队形让你安全避开了危险。" };
            int damage = Math.Max(1, definition.Danger / 12);
            string disease = definition.Id == "mosquito_swarm" ? "malaria" : definition.Id == "venomous_snake" ? "dysentery" : null;
            return new WildlifeResult { Accepted = true, Dangerous = true, Damage = damage, DiseaseId = disease, Message = "遭遇危险动物，队伍受到伤害。" };
        }

        private AnimalState Find(string animalId)
        {
            if (animalId == null) return null;
            foreach (var animal in _state.Animals) if (animal.AnimalId == animalId) return animal;
            return null;
        }

        private static D4AnimalBehavior BehaviorFor(AnimalDefinition definition, int day, int seed)
        {
            int roll = Stable(definition.Id + ":" + day + ":" + seed) % 100;
            if (definition.Danger >= 60 && roll > 72) return D4AnimalBehavior.Attacking;
            if (definition.Rare && roll > 82) return D4AnimalBehavior.BreedingHint;
            if (roll > 55) return D4AnimalBehavior.Alert;
            return D4AnimalBehavior.Foraging;
        }

        private static int Stable(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash & 0x7fffffff);
            }
        }
    }
}
