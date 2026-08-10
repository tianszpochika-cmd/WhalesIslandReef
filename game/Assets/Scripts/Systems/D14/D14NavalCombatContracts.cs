// D14-A01 海战裁决契约：战斗系统是命中、损伤、状态和战斗事件的唯一写者。

using System;
using System.Collections.Generic;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public enum D14NavalCombatStatus
    {
        Idle,
        Detected,
        Approaching,
        Engaged,
        BoardingReady,
        Boarded,
        Escaping,
        Surrendered,
        Victory,
        Defeated,
        Sinking,
    }

    public enum D14NavalAmmoType
    {
        None,
        RoundShot,
        ChainShot,
        GrapeShot,
    }

    public enum D14GunPosition
    {
        PortBow,
        PortMidships,
        PortStern,
        StarboardBow,
        StarboardMidships,
        StarboardStern,
        Bow,
        Stern,
    }

    public enum D14TargetZone
    {
        Auto,
        Hull,
        Gun,
        Mast,
        Rudder,
        Crew,
        Powder,
    }

    public enum D14HitKind
    {
        Miss,
        Graze,
        Hull,
        Gun,
        Sail,
        Rudder,
        Crew,
        Fire,
        Flooding,
        PowderRisk,
    }

    [Serializable]
    public sealed class D14GunState
    {
        public string GunId;
        public D14GunPosition Position;
        public int RoundShot;
        public int ChainShot;
        public int GrapeShot;
        public D14NavalAmmoType LoadedAmmo = D14NavalAmmoType.None;
        public int ReloadRemainingTurns;
        public int ConditionPercent = 100;
        public bool Disabled;
        public bool HasAim;
        public float AimBearingDegrees;
        public D14TargetZone AimTargetZone = D14TargetZone.Auto;

        public D14GunState Clone()
        {
            return new D14GunState
            {
                GunId = GunId,
                Position = Position,
                RoundShot = RoundShot,
                ChainShot = ChainShot,
                GrapeShot = GrapeShot,
                LoadedAmmo = LoadedAmmo,
                ReloadRemainingTurns = ReloadRemainingTurns,
                ConditionPercent = ConditionPercent,
                Disabled = Disabled,
                HasAim = HasAim,
                AimBearingDegrees = AimBearingDegrees,
                AimTargetZone = AimTargetZone,
            };
        }

        public int Available(D14NavalAmmoType ammo)
        {
            switch (ammo)
            {
                case D14NavalAmmoType.RoundShot: return RoundShot;
                case D14NavalAmmoType.ChainShot: return ChainShot;
                case D14NavalAmmoType.GrapeShot: return GrapeShot;
                default: return 0;
            }
        }

        public void Consume(D14NavalAmmoType ammo)
        {
            switch (ammo)
            {
                case D14NavalAmmoType.RoundShot: RoundShot--; break;
                case D14NavalAmmoType.ChainShot: ChainShot--; break;
                case D14NavalAmmoType.GrapeShot: GrapeShot--; break;
            }
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(GunId)) throw new InvalidOperationException(field + ".gun_id is required.");
            if (!Enum.IsDefined(typeof(D14GunPosition), Position)) throw new InvalidOperationException(field + ".position is invalid.");
            if (!Enum.IsDefined(typeof(D14NavalAmmoType), LoadedAmmo)) throw new InvalidOperationException(field + ".loaded_ammo is invalid.");
            if (!Enum.IsDefined(typeof(D14TargetZone), AimTargetZone)) throw new InvalidOperationException(field + ".aim_target_zone is invalid.");
            if (RoundShot < 0 || ChainShot < 0 || GrapeShot < 0 || ReloadRemainingTurns < 0 ||
                ConditionPercent < 0 || ConditionPercent > 100) throw new InvalidOperationException(field + " contains an out-of-range gun value.");
            if (!IsFinite(AimBearingDegrees)) throw new InvalidOperationException(field + ".aim_bearing must be finite.");
        }

        private static bool IsFinite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    [Serializable]
    public sealed class D14CombatShipProfile
    {
        public string ShipId;
        public string ShipTypeId;
        public int HullMax = 100;
        public int Crew = 20;
        public float Speed;
        public float Heading;
        public int GunnerSkill = 50;
        public int ManeuverSkill = 50;
        public int ConditionPercent = 100;
        public List<D14GunState> Guns = new List<D14GunState>();

        public D14CombatShipProfile Clone()
        {
            var result = new D14CombatShipProfile
            {
                ShipId = ShipId, ShipTypeId = ShipTypeId, HullMax = HullMax, Crew = Crew,
                Speed = Speed, Heading = Heading, GunnerSkill = GunnerSkill,
                ManeuverSkill = ManeuverSkill, ConditionPercent = ConditionPercent,
            };
            if (Guns != null) foreach (var gun in Guns) if (gun != null) result.Guns.Add(gun.Clone());
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(ShipId)) throw new InvalidOperationException(field + ".ship_id is required.");
            if (HullMax <= 0 || Crew <= 0 || Speed < 0f || ConditionPercent < 0 || ConditionPercent > 100 ||
                GunnerSkill < 0 || GunnerSkill > 100 || ManeuverSkill < 0 || ManeuverSkill > 100)
                throw new InvalidOperationException(field + " contains an out-of-range ship value.");
            if (Guns == null) throw new InvalidOperationException(field + ".guns is required.");
            foreach (var gun in Guns) if (gun == null) throw new InvalidOperationException(field + ".guns cannot contain null.");
            for (int i = 0; i < Guns.Count; i++) Guns[i].Validate(field + ".guns[" + i + "]");
        }
    }

    [Serializable]
    public sealed class D14HullDamageState
    {
        public int CurrentHull;
        public int MaxHull;
        public int Crew;
        public int MaxCrew;
        public int MastIntegrity = 100;
        public int RudderIntegrity = 100;
        public int GunOperationalCount;
        public int FireLevel;
        public int FloodingLevel;
        public int PowderRisk;
        public float TiltDegrees;

        public D14HullDamageState Clone()
        {
            return new D14HullDamageState
            {
                CurrentHull = CurrentHull, MaxHull = MaxHull, Crew = Crew, MaxCrew = MaxCrew,
                MastIntegrity = MastIntegrity, RudderIntegrity = RudderIntegrity,
                GunOperationalCount = GunOperationalCount, FireLevel = FireLevel,
                FloodingLevel = FloodingLevel, PowderRisk = PowderRisk, TiltDegrees = TiltDegrees,
            };
        }

        public void Validate(string field)
        {
            if (MaxHull <= 0 || CurrentHull < 0 || CurrentHull > MaxHull || MaxCrew <= 0 || Crew < 0 || Crew > MaxCrew ||
                MastIntegrity < 0 || MastIntegrity > 100 || RudderIntegrity < 0 || RudderIntegrity > 100 ||
                GunOperationalCount < 0 || FireLevel < 0 || FireLevel > 100 || FloodingLevel < 0 || FloodingLevel > 100 ||
                PowderRisk < 0 || PowderRisk > 100 || float.IsNaN(TiltDegrees) || float.IsInfinity(TiltDegrees))
                throw new InvalidOperationException(field + " contains an out-of-range damage value.");
        }
    }

    [Serializable]
    public sealed class D14CombatKinematicsState
    {
        public float Distance;
        public float RelativeBearingDegrees;
        public float PlayerHeadingDegrees;
        public float EnemyHeadingDegrees;
        public float PlayerSpeed;
        public float EnemySpeed;

        public D14CombatKinematicsState Clone()
        {
            return new D14CombatKinematicsState
            {
                Distance = Distance, RelativeBearingDegrees = RelativeBearingDegrees,
                PlayerHeadingDegrees = PlayerHeadingDegrees, EnemyHeadingDegrees = EnemyHeadingDegrees,
                PlayerSpeed = PlayerSpeed, EnemySpeed = EnemySpeed,
            };
        }

        public void Validate()
        {
            if (!IsFinite(Distance) || Distance < 0f || !IsFinite(RelativeBearingDegrees) ||
                !IsFinite(PlayerHeadingDegrees) || !IsFinite(EnemyHeadingDegrees) ||
                !IsFinite(PlayerSpeed) || !IsFinite(EnemySpeed) || PlayerSpeed < 0f || EnemySpeed < 0f)
                throw new InvalidOperationException("D14 combat kinematics are outside their saved ranges.");
        }

        private static bool IsFinite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    [Serializable]
    public sealed class D14NavalCombatState
    {
        public string EncounterId;
        public string PlayerShipId;
        public string EnemyShipId;
        public D14NavalCombatStatus Status = D14NavalCombatStatus.Idle;
        public D14CombatKinematicsState Kinematics = new D14CombatKinematicsState();
        public D14HullDamageState PlayerDamage = new D14HullDamageState();
        public D14HullDamageState EnemyDamage = new D14HullDamageState();
        public List<D14GunState> PlayerGuns = new List<D14GunState>();
        public List<D14GunState> EnemyGuns = new List<D14GunState>();
        public int PlayerGunnerSkill;
        public int PlayerManeuverSkill;
        public int EnemyGunnerSkill;
        public int EnemyManeuverSkill;
        public int Turn;
        public int WorldDay;
        public long SaveRevision;
        public string WeatherSourceEventId;
        public string LastEventId;

        public D14NavalCombatState Clone()
        {
            var result = new D14NavalCombatState
            {
                EncounterId = EncounterId, PlayerShipId = PlayerShipId, EnemyShipId = EnemyShipId,
                Status = Status, Kinematics = Kinematics == null ? new D14CombatKinematicsState() : Kinematics.Clone(),
                PlayerDamage = PlayerDamage == null ? new D14HullDamageState() : PlayerDamage.Clone(),
                EnemyDamage = EnemyDamage == null ? new D14HullDamageState() : EnemyDamage.Clone(),
                PlayerGunnerSkill = PlayerGunnerSkill, PlayerManeuverSkill = PlayerManeuverSkill,
                EnemyGunnerSkill = EnemyGunnerSkill, EnemyManeuverSkill = EnemyManeuverSkill,
                Turn = Turn, WorldDay = WorldDay, SaveRevision = SaveRevision,
                WeatherSourceEventId = WeatherSourceEventId, LastEventId = LastEventId,
            };
            if (PlayerGuns != null) foreach (var gun in PlayerGuns) if (gun != null) result.PlayerGuns.Add(gun.Clone());
            if (EnemyGuns != null) foreach (var gun in EnemyGuns) if (gun != null) result.EnemyGuns.Add(gun.Clone());
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(EncounterId) || string.IsNullOrWhiteSpace(PlayerShipId) || string.IsNullOrWhiteSpace(EnemyShipId))
                throw new InvalidOperationException("D14 combat identifiers are required.");
            if (!Enum.IsDefined(typeof(D14NavalCombatStatus), Status)) throw new InvalidOperationException("D14 combat status is invalid.");
            if (Kinematics == null || PlayerDamage == null || EnemyDamage == null || PlayerGuns == null || EnemyGuns == null)
                throw new InvalidOperationException("D14 combat nested state is required.");
            Kinematics.Validate(); PlayerDamage.Validate("player_damage"); EnemyDamage.Validate("enemy_damage");
            foreach (var gun in PlayerGuns) if (gun == null) throw new InvalidOperationException("player_guns contains null.");
            foreach (var gun in EnemyGuns) if (gun == null) throw new InvalidOperationException("enemy_guns contains null.");
            for (int i = 0; i < PlayerGuns.Count; i++) PlayerGuns[i].Validate("player_guns[" + i + "]");
            for (int i = 0; i < EnemyGuns.Count; i++) EnemyGuns[i].Validate("enemy_guns[" + i + "]");
            if (Turn < 0 || WorldDay < 0 || SaveRevision < 0) throw new InvalidOperationException("D14 combat time values must be non-negative.");
        }
    }

    [Serializable]
    public sealed class D14NavalCombatRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = 1;
        public int SchemaVersion = CurrentSchemaVersion;
        public D14NavalCombatState State = new D14NavalCombatState();
        public D9WeatherSeaSnapshot Weather;
        public List<string> AppliedEventKeys = new List<string>();
        public List<string> CombatEventIds = new List<string>();
        public EventLogSnapshot EventLog;

        public D14NavalCombatRuntimeSnapshot Clone()
        {
            var result = new D14NavalCombatRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                State = State == null ? null : State.Clone(),
                Weather = Weather == null ? null : Weather.Clone(),
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            if (CombatEventIds != null) result.CombatEventIds.AddRange(CombatEventIds);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion) throw new InvalidOperationException("D14 combat schema_version mismatch: expected " + CurrentSchemaVersion + ", received " + SchemaVersion + ".");
            if (State != null) State.Validate();
            if (Weather != null) Weather.Validate();
            if (AppliedEventKeys == null || CombatEventIds == null) throw new InvalidOperationException("D14 combat event references are required.");
        }
    }

    [Serializable]
    public sealed class D14CombatStartRequest
    {
        public string OperationId;
        public string EncounterId;
        public D14CombatShipProfile PlayerShip;
        public D14CombatShipProfile EnemyShip;
        public D14CombatKinematicsState Kinematics;
        public D9WeatherSeaSnapshot Weather;
        public string WeatherSourceEventId;
        public string LocationRef = "open_sea";
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14CombatLoadGunRequest
    {
        public string OperationId;
        public string GunId;
        public D14NavalAmmoType Ammo;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14CombatAimRequest
    {
        public string OperationId;
        public string GunId;
        public float BearingDegrees;
        public D14TargetZone TargetZone = D14TargetZone.Auto;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14CombatFireRequest
    {
        public string OperationId;
        public string GunId;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14CombatAdvanceRequest
    {
        public string OperationId;
        public int Turns = 1;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14CombatRetreatRequest
    {
        public string OperationId;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14CombatSurrenderRequest
    {
        public string OperationId;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14CombatResult
    {
        public bool Accepted;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public FactEvent Fact;
        public D14NavalCombatStatus Status;
        public bool Hit;
        public bool Critical;
        public int Damage;
        public int CrewLoss;
        public D14HitKind HitKind;
        public string TargetZone;
        public int ReloadRemainingTurns;
        public D14NavalCombatState State;
    }

    public static class D14NavalCombatIds
    {
        public const string OwnerSystem = "CombatSystem";
        public const string RecoveryRouteId = "d14_combat_recovery_reengage";
        public const string RecoveryAction = "repair_or_break_contact_then_reengage";
        public const string RecoveryDescription = "修复受损船体或拉开距离后，以新操作号重新接战。";
        public const string EventPrefix = "event_d14_naval_";
    }
}
