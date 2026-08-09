using System;

namespace Whalebone.Systems.D4
{
    /// <summary>炮战和简化登船战的唯一数值裁决者。</summary>
    public sealed class D4CombatSystem
    {
        private readonly CombatState _state;

        public D4CombatSystem(CombatState initial = null)
        {
            _state = initial == null ? new CombatState() : initial.Clone();
        }

        public CombatState Snapshot => _state.Clone();

        public void BeginEncounter(string enemyId, int playerHull, int playerHullMax, int playerCannons, int playerCrew,
            int enemyHull, int enemyHullMax, int enemyCannons, int enemyCrew, float distance, float wind)
        {
            _state.Status = D4CombatStatus.Engaged;
            _state.EnemyId = string.IsNullOrEmpty(enemyId) ? "ph_privateer_sloop" : enemyId;
            _state.PlayerHullMax = Math.Max(1, playerHullMax);
            _state.PlayerHull = Clamp(playerHull, 0, _state.PlayerHullMax);
            _state.EnemyHullMax = Math.Max(1, enemyHullMax);
            _state.EnemyHull = Clamp(enemyHull, 0, _state.EnemyHullMax);
            _state.Distance = Clamp(distance, 10f, 100f);
            _state.Wind = Clamp(wind, -1f, 1f);
            _state.PlayerCannons = Math.Max(1, playerCannons);
            _state.PlayerCrew = Math.Max(1, playerCrew);
            _state.EnemyCannons = Math.Max(1, enemyCannons);
            _state.EnemyCrew = Math.Max(1, enemyCrew);
            _state.LoadedCount = 0;
            _state.EnemyLoadedCount = Math.Min(_state.EnemyCannons, 2);
            _state.Weapon = D4WeaponId.RoundShot;
            _state.Turn = 0;
            _state.BoardingScore = 0;
        }

        public bool SelectWeapon(D4WeaponId weapon)
        {
            if (_state.Status != D4CombatStatus.Engaged || _state.LoadedCount > 0) return false;
            _state.Weapon = weapon;
            return true;
        }

        public int Reload(int cannonCount)
        {
            if (_state.Status != D4CombatStatus.Engaged) return 0;
            _state.LoadedCount = Clamp(cannonCount, 0, _state.PlayerCannons);
            return _state.LoadedCount;
        }

        public CombatResult Fire(int gunnerySkill, bool windAdvantage = false)
        {
            if (_state.Status != D4CombatStatus.Engaged)
                return Reject("当前没有进行中的炮战。");
            if (_state.LoadedCount <= 0)
                return Reject("炮尚未装填。");

            int volley = Math.Min(_state.LoadedCount, _state.PlayerCannons);
            float weaponAccuracy = AccuracyFor(_state.Weapon);
            float distanceFactor = 1f - Math.Abs(_state.Distance - 35f) / 90f;
            float chance = Clamp(0.25f + weaponAccuracy + gunnerySkill * 0.035f + distanceFactor * 0.18f + (windAdvantage ? 0.08f : 0f), 0.10f, 0.92f);
            bool hit = Stable01(_state.EnemyId + ":" + _state.Turn + ":" + _state.Weapon) <= chance;
            bool critical = hit && Stable01("critical:" + _state.EnemyId + ":" + _state.Turn) > 0.87f;
            int damage = hit ? Math.Max(1, (int)Math.Round(volley * DamageFor(_state.Weapon) * (critical ? 1.5f : 1f) * (0.82f + distanceFactor * 0.25f))) : 0;
            int crewLoss = hit && _state.Weapon == D4WeaponId.GrapeShot ? Math.Max(1, damage / 7) : 0;
            _state.EnemyHull = Math.Max(0, _state.EnemyHull - damage);
            _state.EnemyCrew = Math.Max(0, _state.EnemyCrew - crewLoss);
            _state.LoadedCount = 0;

            int playerDamage = 0;
            if (_state.EnemyHull > 0) playerDamage = EnemyFire();
            _state.PlayerHull = Math.Max(0, _state.PlayerHull - playerDamage);
            if (_state.EnemyHull <= 0) _state.Status = D4CombatStatus.Victory;
            else if (_state.PlayerHull <= 0) _state.Status = D4CombatStatus.Defeat;
            else _state.Turn++;

            return new CombatResult
            {
                Accepted = true,
                Hit = hit,
                Critical = critical,
                Damage = damage,
                EnemyCrewLoss = crewLoss,
                PlayerDamage = playerDamage,
                Status = _state.Status,
                Message = hit ? (critical ? "炮弹命中要害！" : "炮弹命中敌船。") : "炮弹落空。",
            };
        }

        public CombatResult Board()
        {
            if (_state.Status != D4CombatStatus.Engaged) return Reject("当前不能接舷。");
            if (_state.Distance > 28f) return Reject("距离太远，无法接舷。", false);
            int score = _state.PlayerCrew * 4 + (int)(Stable01("board:" + _state.EnemyId + ":" + _state.Turn) * 20f);
            int resistance = _state.EnemyCrew * 3 + 12;
            _state.BoardingScore = score - resistance;
            if (score >= resistance)
            {
                _state.Status = D4CombatStatus.Boarded;
                _state.EnemyHull = Math.Max(0, _state.EnemyHull - 20);
                _state.Status = D4CombatStatus.Victory;
                return new CombatResult { Accepted = true, Damage = 20, Status = _state.Status, Message = "接舷战胜利，敌船已被控制。" };
            }
            int damage = Math.Max(1, resistance / 5);
            _state.PlayerHull = Math.Max(0, _state.PlayerHull - damage);
            if (_state.PlayerHull <= 0) _state.Status = D4CombatStatus.Defeat;
            return new CombatResult { Accepted = true, PlayerDamage = damage, Status = _state.Status, Message = "接舷战失败，船员被迫撤回。" };
        }

        public CombatResult Escape()
        {
            if (_state.Status != D4CombatStatus.Engaged) return Reject("当前没有可脱离的炮战。");
            bool escaped = Stable01("escape:" + _state.EnemyId + ":" + _state.Turn) < 0.58f + Math.Max(0f, _state.Wind) * 0.12f;
            if (escaped)
            {
                _state.Status = D4CombatStatus.Escaped;
                return new CombatResult { Accepted = true, Status = _state.Status, Message = "成功脱离敌舰。" };
            }
            int damage = Math.Max(1, _state.EnemyCannons / 2);
            _state.PlayerHull = Math.Max(0, _state.PlayerHull - damage);
            if (_state.PlayerHull <= 0) _state.Status = D4CombatStatus.Defeat;
            return new CombatResult { Accepted = true, PlayerDamage = damage, Status = _state.Status, Message = "脱离失败，敌舰追击造成损伤。" };
        }

        private int EnemyFire()
        {
            if (_state.EnemyLoadedCount <= 0) _state.EnemyLoadedCount = Math.Min(_state.EnemyCannons, 2);
            bool hit = Stable01("enemy:" + _state.EnemyId + ":" + _state.Turn) < 0.34f;
            int damage = hit ? Math.Max(1, (int)Math.Round(_state.EnemyLoadedCount * 5.5f)) : 0;
            _state.EnemyLoadedCount = 0;
            return damage;
        }

        private static CombatResult Reject(string message, bool accepted = false)
        {
            return new CombatResult { Accepted = accepted, Status = D4CombatStatus.Engaged, Message = message };
        }

        private static float AccuracyFor(D4WeaponId weapon)
        {
            switch (weapon)
            {
                case D4WeaponId.ChainShot: return 0.28f;
                case D4WeaponId.GrapeShot: return 0.34f;
                default: return 0.38f;
            }
        }

        private static float DamageFor(D4WeaponId weapon)
        {
            switch (weapon)
            {
                case D4WeaponId.ChainShot: return 5.5f;
                case D4WeaponId.GrapeShot: return 4.5f;
                default: return 8f;
            }
        }

        private static float Stable01(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (hash % 10000u) / 10000f;
            }
        }

        private static int Clamp(int value, int min, int max) { return Math.Max(min, Math.Min(max, value)); }
        private static float Clamp(float value, float min, float max) { return Math.Max(min, Math.Min(max, value)); }
    }
}
