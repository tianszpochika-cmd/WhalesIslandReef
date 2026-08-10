// D14-A01 海战裁决：所有命中、分区损伤、火灾、进水、撤退和投降均在此裁决。
// 该类只读取 D9 天气快照；UI/Agent 只能提交 request，不能写入 State。

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public sealed class D14NavalCombatSystem
    {
        private readonly EventLog _eventLog;
        private D14NavalCombatState _state = new D14NavalCombatState();
        private D9WeatherSeaSnapshot _weather;
        private readonly HashSet<string> _appliedEventKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<string> _combatEventIds = new List<string>();

        public D14NavalCombatSystem(EventLog eventLog = null, D14NavalCombatRuntimeSnapshot initial = null)
        {
            if (initial != null && initial.EventLog != null && eventLog == null)
                _eventLog = EventLog.FromSnapshot(initial.EventLog);
            else
                _eventLog = eventLog ?? new EventLog();

            if (initial != null)
            {
                initial.Validate();
                _state = initial.State == null ? new D14NavalCombatState() : initial.State.Clone();
                _weather = initial.Weather == null ? null : initial.Weather.Clone();
                if (initial.AppliedEventKeys != null) foreach (var key in initial.AppliedEventKeys) _appliedEventKeys.Add(key);
                if (initial.CombatEventIds != null) _combatEventIds.AddRange(initial.CombatEventIds);
            }
        }

        public D14NavalCombatState Snapshot { get { return _state.Clone(); } }
        public D9WeatherSeaSnapshot WeatherSnapshot { get { return _weather == null ? null : _weather.Clone(); } }
        public IReadOnlyList<FactEvent> CombatLog { get { return _eventLog.ReadFactsForUi(int.MaxValue); } }
        public EventLog SharedEventLog { get { return _eventLog; } }

        public D14NavalCombatRuntimeSnapshot CaptureSnapshot()
        {
            var result = new D14NavalCombatRuntimeSnapshot
            {
                State = _state.Clone(),
                Weather = _weather == null ? null : _weather.Clone(),
                EventLog = _eventLog.CaptureSnapshot(),
            };
            result.AppliedEventKeys.AddRange(_appliedEventKeys);
            result.CombatEventIds.AddRange(_combatEventIds);
            result.Validate();
            return result;
        }

        public D14CombatResult StartEncounter(D14CombatStartRequest request)
        {
            if (!CanRequest(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "接战请求缺少操作号。", D14NavalCombatStatus.Idle);
            var duplicate = DuplicateIfKnown(request.OperationId, request.SaveRevision);
            if (duplicate != null) return duplicate;
            if (_state.Status != D14NavalCombatStatus.Idle && _state.Status != D14NavalCombatStatus.Victory &&
                _state.Status != D14NavalCombatStatus.Defeated && _state.Status != D14NavalCombatStatus.Surrendered &&
                _state.Status != D14NavalCombatStatus.Escaping)
                return Reject("combat_active", "当前已有进行中的战斗。", _state.Status);
            if (request.PlayerShip == null || request.EnemyShip == null || request.Kinematics == null || request.Weather == null)
                return Reject("missing_combat_inputs", "接战需要双方船舶、航行参数和天气快照。", _state.Status);

            try
            {
                request.PlayerShip.Validate("player_ship");
                request.EnemyShip.Validate("enemy_ship");
                request.Kinematics.Validate();
                request.Weather.Validate();
            }
            catch (Exception ex) { return Reject("invalid_combat_inputs", ex.Message, _state.Status); }

            if (string.IsNullOrWhiteSpace(request.WeatherSourceEventId) || !HasFact(request.WeatherSourceEventId))
                return Reject("weather_source_missing", "天气必须来自已记录的 EnvironmentSystem 事实。", _state.Status);
            if (!HasAllFacts(request.CauseEventIds, true))
                return Reject("cause_source_missing", "接战因果来源中存在未记录事实。", _state.Status);

            _weather = request.Weather.Clone();
            _state = new D14NavalCombatState
            {
                EncounterId = request.EncounterId,
                PlayerShipId = request.PlayerShip.ShipId,
                EnemyShipId = request.EnemyShip.ShipId,
                Status = D14NavalCombatStatus.Engaged,
                Kinematics = request.Kinematics.Clone(),
                PlayerDamage = CreateDamage(request.PlayerShip),
                EnemyDamage = CreateDamage(request.EnemyShip),
                PlayerGunnerSkill = request.PlayerShip.GunnerSkill,
                PlayerManeuverSkill = request.PlayerShip.ManeuverSkill,
                EnemyGunnerSkill = request.EnemyShip.GunnerSkill,
                EnemyManeuverSkill = request.EnemyShip.ManeuverSkill,
                Turn = 0,
                WorldDay = Math.Max(0, request.WorldDay),
                SaveRevision = Math.Max(0L, request.SaveRevision),
                WeatherSourceEventId = request.WeatherSourceEventId,
            };
            foreach (var gun in request.PlayerShip.Guns) _state.PlayerGuns.Add(gun.Clone());
            foreach (var gun in request.EnemyShip.Guns) _state.EnemyGuns.Add(gun.Clone());
            return Commit(request.OperationId, request.WorldDay, request.SaveRevision, request.LocationRef,
                request.CauseEventIds, "combat_started", "海战接触已确认，进入炮战距离。", "combat_started",
                new Dictionary<string, string>
                {
                    ["encounter_id"] = request.EncounterId ?? "",
                    ["distance"] = F(_state.Kinematics.Distance),
                    ["relative_bearing"] = F(_state.Kinematics.RelativeBearingDegrees),
                    ["wind_direction"] = F(_weather.WindDirectionDegrees),
                    ["wind_speed"] = F(_weather.WindSpeed),
                });
        }

        public D14CombatResult LoadGun(D14CombatLoadGunRequest request)
        {
            var duplicate = DuplicateIfKnown(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision);
            if (duplicate != null) return duplicate;
            if (!CanActive(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "装填请求缺少操作号。", _state.Status);
            if (request.Ammo == D14NavalAmmoType.None || !Enum.IsDefined(typeof(D14NavalAmmoType), request.Ammo))
                return Reject("invalid_ammo", "必须选择有效弹种。", _state.Status);
            var gun = FindGun(request.GunId);
            if (gun == null) return Reject("gun_missing", "找不到指定炮位。", _state.Status);
            if (gun.Disabled || gun.ConditionPercent <= 0) return Reject("gun_disabled", "该炮位已损坏，不能装填。", _state.Status);
            if (gun.LoadedAmmo != D14NavalAmmoType.None || gun.ReloadRemainingTurns > 0)
                return Reject("gun_not_ready", "该炮位尚未完成上一轮装填。", _state.Status);
            if (gun.Available(request.Ammo) <= 0) return Reject("ammunition_empty", "该炮位没有该弹种。", _state.Status);
            gun.Consume(request.Ammo);
            gun.LoadedAmmo = request.Ammo;
            gun.HasAim = false;
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                "gun_loaded", "炮位已装填" + request.Ammo + "。", "gun_loaded",
                new Dictionary<string, string> { ["gun_id"] = gun.GunId, ["ammo"] = request.Ammo.ToString() });
            result.ReloadRemainingTurns = gun.ReloadRemainingTurns;
            return result;
        }

        public D14CombatResult Aim(D14CombatAimRequest request)
        {
            var duplicate = DuplicateIfKnown(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision);
            if (duplicate != null) return duplicate;
            if (!CanActive(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "瞄准请求缺少操作号。", _state.Status);
            if (request.TargetZone != D14TargetZone.Auto && !Enum.IsDefined(typeof(D14TargetZone), request.TargetZone))
                return Reject("invalid_target_zone", "目标分区无效。", _state.Status);
            if (!Finite(request.BearingDegrees)) return Reject("invalid_bearing", "瞄准角度必须是有限数值。", _state.Status);
            var gun = FindGun(request.GunId);
            if (gun == null) return Reject("gun_missing", "找不到指定炮位。", _state.Status);
            if (gun.LoadedAmmo == D14NavalAmmoType.None) return Reject("gun_unloaded", "未装填的炮位不能瞄准。", _state.Status);
            gun.HasAim = true;
            gun.AimBearingDegrees = Normalize(request.BearingDegrees);
            gun.AimTargetZone = request.TargetZone;
            return Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                "aim_set", "瞄准角度和船体目标分区已记录。", "aim_set",
                new Dictionary<string, string>
                {
                    ["gun_id"] = gun.GunId, ["bearing"] = F(gun.AimBearingDegrees), ["target_zone"] = request.TargetZone.ToString(),
                });
        }

        public D14CombatResult Fire(D14CombatFireRequest request)
        {
            var duplicate = DuplicateIfKnown(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision);
            if (duplicate != null) return duplicate;
            if (!CanActive(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "开火请求缺少操作号。", _state.Status);
            var gun = FindGun(request.GunId);
            if (gun == null) return Reject("gun_missing", "找不到指定炮位。", _state.Status);
            if (gun.LoadedAmmo == D14NavalAmmoType.None || !gun.HasAim)
                return Reject("fire_not_ready", "炮位必须完成装填和瞄准后才能开火。", _state.Status);

            var ammo = gun.LoadedAmmo;
            float chance = HitChance(ammo, gun);
            string seed = (_state.EncounterId ?? "") + "|" + _state.Turn + "|" + gun.GunId + "|" + ammo + "|" + gun.AimTargetZone;
            float roll = Stable01(seed);
            bool hit = roll <= chance;
            bool critical = hit && roll <= chance * 0.16f;
            D14HitKind hitKind = D14HitKind.Miss;
            int damage = 0;
            int crewLoss = 0;
            if (hit)
            {
                hitKind = ResolveHitKind(ammo, gun.AimTargetZone);
                damage = BaseDamage(ammo, hitKind, critical);
                int fireBefore = _state.EnemyDamage.FireLevel;
                int floodingBefore = _state.EnemyDamage.FloodingLevel;
                crewLoss = ApplyHit(hitKind, damage, critical, seed);
                if (hitKind == D14HitKind.Hull && _state.EnemyDamage.FireLevel > fireBefore) hitKind = D14HitKind.Fire;
                else if (hitKind == D14HitKind.Hull && _state.EnemyDamage.FloodingLevel > floodingBefore) hitKind = D14HitKind.Flooding;
            }

            gun.LoadedAmmo = D14NavalAmmoType.None;
            gun.HasAim = false;
            gun.ReloadRemainingTurns = ReloadTurns(ammo);
            UpdateTerminalStatus();
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                hit ? "fire_hit" : "fire_miss", hit ? "炮弹命中，分区损伤已由 CombatSystem 裁决。" : "炮弹未命中。", "fire_resolved",
                new Dictionary<string, string>
                {
                    ["gun_id"] = gun.GunId, ["ammo"] = ammo.ToString(), ["hit"] = hit.ToString(),
                    ["critical"] = critical.ToString(), ["hit_kind"] = hitKind.ToString(),
                    ["damage"] = damage.ToString(CultureInfo.InvariantCulture), ["crew_loss"] = crewLoss.ToString(CultureInfo.InvariantCulture),
                    ["chance"] = F(chance), ["roll"] = F(roll), ["distance"] = F(_state.Kinematics.Distance),
                });
            result.Hit = hit; result.Critical = critical; result.Damage = damage; result.CrewLoss = crewLoss;
            result.HitKind = hitKind; result.TargetZone = gun.AimTargetZone.ToString(); result.ReloadRemainingTurns = gun.ReloadRemainingTurns;
            result.State = _state.Clone();
            return result;
        }

        public D14CombatResult AdvanceRound(D14CombatAdvanceRequest request)
        {
            var duplicate = DuplicateIfKnown(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision);
            if (duplicate != null) return duplicate;
            if (!CanActive(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "推进回合请求缺少操作号。", _state.Status);
            int turns = request.Turns <= 0 ? 1 : Math.Min(10, request.Turns);
            for (int i = 0; i < turns; i++)
            {
                _state.Turn++;
                TickGuns(_state.PlayerGuns);
                TickGuns(_state.EnemyGuns);
                TickDamage(_state.PlayerDamage);
                TickDamage(_state.EnemyDamage);
                UpdateTerminalStatus();
                if (IsTerminal(_state.Status)) break;
            }
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                _state.Status == D14NavalCombatStatus.Sinking ? "combat_sinking" : "round_advanced",
                "炮战回合推进，装填、火灾和进水状态已更新。", "round_advanced",
                new Dictionary<string, string>
                {
                    ["turn"] = _state.Turn.ToString(CultureInfo.InvariantCulture), ["player_fire"] = _state.PlayerDamage.FireLevel.ToString(CultureInfo.InvariantCulture),
                    ["player_flooding"] = _state.PlayerDamage.FloodingLevel.ToString(CultureInfo.InvariantCulture), ["enemy_fire"] = _state.EnemyDamage.FireLevel.ToString(CultureInfo.InvariantCulture),
                    ["enemy_flooding"] = _state.EnemyDamage.FloodingLevel.ToString(CultureInfo.InvariantCulture),
                });
            result.State = _state.Clone();
            return result;
        }

        public D14CombatResult AttemptRetreat(D14CombatRetreatRequest request)
        {
            var duplicate = DuplicateIfKnown(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision);
            if (duplicate != null) return duplicate;
            if (!CanActive(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "撤退请求缺少操作号。", _state.Status);
            float speedAdvantage = Clamp01((_state.Kinematics.PlayerSpeed - _state.Kinematics.EnemySpeed + 4f) / 12f);
            float distanceFactor = Clamp01(_state.Kinematics.Distance / 260f);
            float rudderFactor = _state.PlayerDamage.RudderIntegrity / 100f;
            float stormPenalty = _weather == null ? 0f : _weather.HullRisk * 0.18f;
            float chance = Clamp01(0.22f + distanceFactor * 0.32f + speedAdvantage * 0.28f + rudderFactor * 0.18f - stormPenalty);
            float roll = Stable01((_state.EncounterId ?? "") + "|retreat|" + _state.Turn);
            bool succeeded = chance >= 0.90f || roll <= chance;
            if (succeeded) _state.Status = D14NavalCombatStatus.Escaping;
            else
            {
                _state.PlayerDamage.CurrentHull = Math.Max(0, _state.PlayerDamage.CurrentHull - 5);
                _state.PlayerDamage.FloodingLevel = Math.Min(100, _state.PlayerDamage.FloodingLevel + 4);
                UpdateTerminalStatus();
            }
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                succeeded ? "retreat_succeeded" : "retreat_failed", succeeded ? "已脱离炮战距离。" : "撤退受阻，船体承受追击损伤。", "retreat_resolved",
                new Dictionary<string, string>
                {
                    ["succeeded"] = succeeded.ToString(), ["chance"] = F(chance), ["roll"] = F(roll),
                    ["distance"] = F(_state.Kinematics.Distance), ["player_speed"] = F(_state.Kinematics.PlayerSpeed),
                }, succeeded ? WorldEventStatus.Normal : WorldEventStatus.Failed);
            result.State = _state.Clone();
            return result;
        }

        public D14CombatResult Surrender(D14CombatSurrenderRequest request)
        {
            var duplicate = DuplicateIfKnown(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision);
            if (duplicate != null) return duplicate;
            if (!CanActive(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "投降请求缺少操作号。", _state.Status);
            _state.Status = D14NavalCombatStatus.Surrendered;
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                "surrendered", "船长已向对手投降；俘虏、资产和法律后果交由后续系统处理。", "surrendered",
                new Dictionary<string, string> { ["enemy_ship_id"] = _state.EnemyShipId ?? "", ["player_hull"] = _state.PlayerDamage.CurrentHull.ToString(CultureInfo.InvariantCulture) });
            result.State = _state.Clone();
            return result;
        }

        private D14CombatResult Commit(string operationId, int worldDay, long saveRevision, string location,
            IEnumerable<string> causes, string code, string message, string factCode, Dictionary<string, string> values,
            WorldEventStatus status = WorldEventStatus.Normal)
        {
            string eventId = D14NavalCombatIds.EventPrefix + operationId;
            string key = eventId + "\u001f" + Math.Max(0L, saveRevision).ToString(CultureInfo.InvariantCulture);
            FactEvent fact = _eventLog.Find(eventId, Math.Max(0L, saveRevision));
            if (fact != null || _appliedEventKeys.Contains(key))
                return new D14CombatResult { Accepted = true, Duplicate = true, Code = "duplicate", Message = "该战斗操作已结算，不重复修改战斗状态。", EventId = eventId, Fact = fact, Status = _state.Status, State = _state.Clone() };

            var causeList = ResolveCauses(causes);
            if (status == WorldEventStatus.Failed)
                values["recovery_route"] = D14NavalCombatIds.RecoveryRouteId;
            var append = _eventLog.Append(new WorldEvent(
                eventId, Math.Max(0, worldDay), string.IsNullOrWhiteSpace(location) ? "open_sea" : location,
                D14NavalCombatIds.OwnerSystem,
                new EventResult { ResultCode = code, Summary = message, FactIds = new List<string> { "combat:" + (_state.EncounterId ?? "unknown") + ":" + factCode }, Values = values },
                new[] { _state.PlayerShipId ?? "player_ship", _state.EnemyShipId ?? "enemy_ship" },
                new[] { "d14:naval_combat" }, causeList, Math.Max(0L, saveRevision), status,
                status == WorldEventStatus.Failed ? new RecoveryRoute { RouteId = D14NavalCombatIds.RecoveryRouteId, Action = D14NavalCombatIds.RecoveryAction, ResumeFromEventId = eventId, Description = D14NavalCombatIds.RecoveryDescription } : null));
            fact = append.Fact;
            _appliedEventKeys.Add(key);
            if (!_combatEventIds.Contains(eventId)) _combatEventIds.Add(eventId);
            _state.LastEventId = eventId;
            _state.WorldDay = Math.Max(_state.WorldDay, Math.Max(0, worldDay));
            _state.SaveRevision = Math.Max(_state.SaveRevision, Math.Max(0L, saveRevision));
            return new D14CombatResult { Accepted = true, Code = code, Message = message, EventId = eventId, Fact = fact, Status = _state.Status, State = _state.Clone() };
        }

        private D14CombatResult Reject(string code, string message, D14NavalCombatStatus status)
        {
            return new D14CombatResult { Accepted = false, Code = code, Message = message, Status = status, State = _state.Clone(), HitKind = D14HitKind.Miss };
        }

        private D14CombatResult DuplicateIfKnown(string operationId, long revision)
        {
            if (!CanRequest(operationId, revision)) return null;
            string eventId = D14NavalCombatIds.EventPrefix + operationId;
            string key = eventId + "\u001f" + revision.ToString(CultureInfo.InvariantCulture);
            if (!_appliedEventKeys.Contains(key) && _eventLog.Find(eventId, revision) == null) return null;
            return new D14CombatResult
            {
                Accepted = true, Duplicate = true, Code = "duplicate", Message = "该战斗操作已结算，不重复修改战斗状态。",
                EventId = eventId, Fact = _eventLog.Find(eventId, revision), Status = _state.Status, State = _state.Clone(),
            };
        }

        private bool CanRequest(string operationId, long revision)
        {
            return !string.IsNullOrWhiteSpace(operationId) && revision >= 0;
        }

        private bool CanActive(string operationId, long revision)
        {
            return CanRequest(operationId, revision) &&
                (_state.Status == D14NavalCombatStatus.Engaged || _state.Status == D14NavalCombatStatus.Approaching || _state.Status == D14NavalCombatStatus.Detected);
        }

        private bool HasFact(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            foreach (var fact in _eventLog.Facts) if (string.Equals(fact.EventId, id, StringComparison.Ordinal)) return true;
            return false;
        }

        private bool HasAllFacts(IEnumerable<string> ids, bool allowEmpty)
        {
            var list = ids == null ? new List<string>() : new List<string>(ids);
            if (!allowEmpty && list.Count == 0) return false;
            foreach (var id in list) if (!HasFact(id)) return false;
            return true;
        }

        private List<string> ResolveCauses(IEnumerable<string> causes)
        {
            var result = new List<string>();
            if (causes != null) foreach (var id in causes) if (!string.IsNullOrWhiteSpace(id) && !result.Contains(id)) result.Add(id);
            if (result.Count == 0 && !string.IsNullOrWhiteSpace(_state.LastEventId)) result.Add(_state.LastEventId);
            if (result.Count == 0 && !string.IsNullOrWhiteSpace(_state.WeatherSourceEventId)) result.Add(_state.WeatherSourceEventId);
            return result;
        }

        private D14GunState FindGun(string gunId)
        {
            if (string.IsNullOrWhiteSpace(gunId)) return null;
            foreach (var gun in _state.PlayerGuns) if (string.Equals(gun.GunId, gunId, StringComparison.Ordinal)) return gun;
            return null;
        }

        private float HitChance(D14NavalAmmoType ammo, D14GunState gun)
        {
            float alignment = 1f - Clamp01(Math.Abs(SignedAngleDelta(gun.AimBearingDegrees, _state.Kinematics.RelativeBearingDegrees)) / 90f);
            float ideal = ammo == D14NavalAmmoType.GrapeShot ? 35f : (ammo == D14NavalAmmoType.ChainShot ? 100f : 150f);
            float range = 1f - Clamp01(Math.Abs(_state.Kinematics.Distance - ideal) / ideal);
            float visibility = _weather == null ? 0.7f : _weather.Visibility;
            float crosswind = _weather == null ? 0f : Math.Abs((float)Math.Sin((Normalize(_weather.WindDirectionDegrees - _state.Kinematics.PlayerHeadingDegrees)) * Math.PI / 180.0));
            float windPenalty = _weather == null ? 0f : Math.Min(0.24f, crosswind * _weather.WindSpeed / 24f * 0.28f);
            float speedPenalty = Clamp01(Math.Abs(_state.Kinematics.PlayerSpeed - _state.Kinematics.EnemySpeed) / 14f) * 0.10f;
            float skill = _state.PlayerGunnerSkill / 100f * 0.16f;
            float shortRangePenalty = ammo == D14NavalAmmoType.GrapeShot && _state.Kinematics.Distance > 80f ? 0.25f : 0f;
            return Clamp(0.12f + alignment * 0.34f + range * 0.25f + visibility * 0.12f + skill + (1f - speedPenalty) * 0.07f - windPenalty - shortRangePenalty, 0.03f, 0.94f);
        }

        private D14HitKind ResolveHitKind(D14NavalAmmoType ammo, D14TargetZone zone)
        {
            if (zone == D14TargetZone.Auto)
                zone = ammo == D14NavalAmmoType.ChainShot ? D14TargetZone.Mast : (ammo == D14NavalAmmoType.GrapeShot ? D14TargetZone.Crew : D14TargetZone.Hull);
            switch (zone)
            {
                case D14TargetZone.Gun: return D14HitKind.Gun;
                case D14TargetZone.Mast: return D14HitKind.Sail;
                case D14TargetZone.Rudder: return D14HitKind.Rudder;
                case D14TargetZone.Crew: return D14HitKind.Crew;
                case D14TargetZone.Powder: return D14HitKind.PowderRisk;
                default: return D14HitKind.Hull;
            }
        }

        private int BaseDamage(D14NavalAmmoType ammo, D14HitKind kind, bool critical)
        {
            int baseDamage = ammo == D14NavalAmmoType.RoundShot ? 18 : (ammo == D14NavalAmmoType.ChainShot ? 8 : 6);
            if (kind == D14HitKind.Gun || kind == D14HitKind.Rudder) baseDamage = Math.Max(3, baseDamage - 2);
            return critical ? (int)Math.Ceiling(baseDamage * 1.6f) : baseDamage;
        }

        private int ApplyHit(D14HitKind kind, int damage, bool critical, string seed)
        {
            var target = _state.EnemyDamage;
            int crewLoss = 0;
            switch (kind)
            {
                case D14HitKind.Hull: target.CurrentHull = Math.Max(0, target.CurrentHull - damage); target.TiltDegrees = Math.Min(30f, target.TiltDegrees + damage * 0.08f); break;
                case D14HitKind.Gun: target.GunOperationalCount = Math.Max(0, target.GunOperationalCount - 1); break;
                case D14HitKind.Sail: target.MastIntegrity = Math.Max(0, target.MastIntegrity - damage * 2); _state.Kinematics.EnemySpeed = Math.Max(0f, _state.Kinematics.EnemySpeed - damage * 0.05f); break;
                case D14HitKind.Rudder: target.RudderIntegrity = Math.Max(0, target.RudderIntegrity - damage * 2); break;
                case D14HitKind.Crew: crewLoss = Math.Min(target.Crew, Math.Max(1, damage / 2)); target.Crew -= crewLoss; break;
                case D14HitKind.PowderRisk: target.PowderRisk = Math.Min(100, target.PowderRisk + damage * 3); break;
            }

            float secondary = Stable01(seed + "|secondary");
            if (kind == D14HitKind.Hull && secondary < (critical ? 0.60f : 0.35f))
            {
                target.FireLevel = Math.Min(100, target.FireLevel + (critical ? 30 : 18));
                if (target.FireLevel >= 30) kind = D14HitKind.Fire;
            }
            if (kind == D14HitKind.Hull && secondary >= 0.52f && secondary < 0.84f)
                target.FloodingLevel = Math.Min(100, target.FloodingLevel + (critical ? 35 : 20));
            if (kind == D14HitKind.Gun && target.PowderRisk > 50 && secondary < 0.20f)
            {
                target.FireLevel = Math.Min(100, target.FireLevel + 45);
                target.PowderRisk = Math.Min(100, target.PowderRisk + 20);
            }
            return crewLoss;
        }

        private static void TickGuns(List<D14GunState> guns)
        {
            foreach (var gun in guns)
                if (gun.ReloadRemainingTurns > 0) gun.ReloadRemainingTurns--;
        }

        private static void TickDamage(D14HullDamageState damage)
        {
            if (damage.FireLevel > 0)
            {
                damage.CurrentHull = Math.Max(0, damage.CurrentHull - Math.Max(1, damage.FireLevel / 15));
                damage.FireLevel = Math.Max(0, damage.FireLevel - 4);
            }
            if (damage.FloodingLevel > 0)
            {
                damage.CurrentHull = Math.Max(0, damage.CurrentHull - Math.Max(1, damage.FloodingLevel / 20));
                damage.FloodingLevel = Math.Min(100, damage.FloodingLevel + 2);
            }
        }

        private void UpdateTerminalStatus()
        {
            if (_state.PlayerDamage.CurrentHull <= 0 || _state.PlayerDamage.FloodingLevel >= 100) _state.Status = D14NavalCombatStatus.Sinking;
            else if (_state.EnemyDamage.CurrentHull <= 0 || _state.EnemyDamage.FloodingLevel >= 100 || _state.EnemyDamage.Crew <= 0) _state.Status = D14NavalCombatStatus.Victory;
        }

        private static D14HullDamageState CreateDamage(D14CombatShipProfile profile)
        {
            return new D14HullDamageState
            {
                CurrentHull = profile.HullMax, MaxHull = profile.HullMax, Crew = profile.Crew, MaxCrew = profile.Crew,
                GunOperationalCount = profile.Guns.Count,
            };
        }

        private static int ReloadTurns(D14NavalAmmoType ammo) { return ammo == D14NavalAmmoType.GrapeShot ? 1 : (ammo == D14NavalAmmoType.ChainShot ? 2 : 3); }
        private static bool IsTerminal(D14NavalCombatStatus status)
        {
            return status == D14NavalCombatStatus.Victory || status == D14NavalCombatStatus.Defeated || status == D14NavalCombatStatus.Sinking || status == D14NavalCombatStatus.Surrendered || status == D14NavalCombatStatus.Escaping;
        }
        private static float Stable01(string text)
        {
            unchecked
            {
                uint hash = 2166136261u;
                for (int i = 0; i < (text ?? "").Length; i++) { hash ^= text[i]; hash *= 16777619u; }
                return (hash % 10000u) / 10000f;
            }
        }
        private static float Normalize(float value) { value %= 360f; return value < 0f ? value + 360f : value; }
        private static float SignedAngleDelta(float a, float b) { float delta = Normalize(a - b); return delta > 180f ? delta - 360f : delta; }
        private static float Clamp01(float value) { return Clamp(value, 0f, 1f); }
        private static float Clamp(float value, float min, float max) { return Math.Max(min, Math.Min(max, value)); }
        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        private static string F(float value) { return value.ToString("0.####", CultureInfo.InvariantCulture); }
    }
}
