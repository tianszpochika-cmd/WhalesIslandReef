using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D4
{
    public sealed class InspectionResult
    {
        public bool Allowed;
        public bool CargoSeized;
        public int Fine;
        public string Message;
    }

    public sealed class FactionActionResult
    {
        public bool Accepted;
        public int ReputationDelta;
        public int WantedDelta;
        public string Message;
    }

    /// <summary>势力、战争、通行、禁运与私掠状态的唯一裁决者。</summary>
    public sealed class D4FactionSystem
    {
        private readonly FactionState _state;

        public D4FactionSystem(FactionState initial = null)
        {
            _state = initial == null ? D4FactionCatalog.CreateDefault() : initial.Clone();
            foreach (var definition in D4FactionCatalog.All)
                if (!_state.Reputation.ContainsKey(definition.Id)) _state.Reputation[definition.Id] = 0;
        }

        public FactionState Snapshot => _state.Clone();

        public int GetReputation(string factionId)
        {
            int value;
            return factionId != null && _state.Reputation.TryGetValue(factionId, out value) ? value : 0;
        }

        public FactionActionResult ApplyReputation(string factionId, int delta, string reason)
        {
            if (D4FactionCatalog.Find(factionId) == null)
                return new FactionActionResult { Message = "未知势力。" };
            int before = GetReputation(factionId);
            _state.Reputation[factionId] = Clamp(before + delta, -100, 100);
            return new FactionActionResult
            {
                Accepted = true,
                ReputationDelta = _state.Reputation[factionId] - before,
                Message = string.IsNullOrEmpty(reason) ? "声望已更新。" : reason,
            };
        }

        public FactionActionResult SetWar(string factionA, string factionB, bool atWar)
        {
            var key = WarKey(factionA, factionB);
            if (key == null) return new FactionActionResult { Message = "战争双方无效。" };
            if (atWar && !_state.AtWar.Contains(key)) _state.AtWar.Add(key);
            if (!atWar) _state.AtWar.Remove(key);
            return new FactionActionResult { Accepted = true, Message = atWar ? "双方进入战争状态。" : "双方恢复停战。" };
        }

        public bool IsAtWar(string factionA, string factionB)
        {
            var key = WarKey(factionA, factionB);
            return key != null && _state.AtWar.Contains(key);
        }

        public FactionActionResult SetEmbargo(string portId, bool embargoed)
        {
            if (string.IsNullOrEmpty(portId)) return new FactionActionResult { Message = "港口无效。" };
            if (embargoed && !_state.EmbargoedPorts.Contains(portId)) _state.EmbargoedPorts.Add(portId);
            if (!embargoed) _state.EmbargoedPorts.Remove(portId);
            return new FactionActionResult { Accepted = true, Message = embargoed ? "港口已进入禁运。" : "港口解除禁运。" };
        }

        public bool IsEmbargoed(string portId) { return !string.IsNullOrEmpty(portId) && _state.EmbargoedPorts.Contains(portId); }

        public FactionActionResult IssuePrivateerPermit(string factionId)
        {
            if (D4FactionCatalog.Find(factionId) == null)
                return new FactionActionResult { Message = "未知发证势力。" };
            if (GetReputation(factionId) < 30)
                return new FactionActionResult { Message = "该势力声望不足，拒绝签发私掠证。" };
            if (!_state.PrivateerPermits.Contains(factionId)) _state.PrivateerPermits.Add(factionId);
            return new FactionActionResult { Accepted = true, Message = "私掠证已签发。" };
        }

        public bool HasPrivateerPermit(string factionId) { return factionId != null && _state.PrivateerPermits.Contains(factionId); }

        public float GetTariffMultiplier(string factionId)
        {
            int reputation = GetReputation(factionId);
            if (reputation >= 60) return 0.65f;
            if (reputation >= 30) return 0.80f;
            if (reputation < -40) return 1.25f;
            return 1f;
        }

        public bool CanEnterPort(string portId, string controllingFactionId)
        {
            if (IsEmbargoed(portId)) return GetReputation(controllingFactionId) >= 50 || HasPrivateerPermit("smugglers");
            return GetReputation(controllingFactionId) > -60;
        }

        public InspectionResult Inspect(string portId, string controllingFactionId, IEnumerable<string> cargoIds)
        {
            bool illegal = false;
            if (cargoIds != null)
                foreach (var cargo in cargoIds)
                    if (cargo == "rum" || cargo == "tobacco" || cargo == "spices") illegal = true;
            bool blocked = IsEmbargoed(portId) && GetReputation(controllingFactionId) < 50;
            if (!illegal && !blocked)
                return new InspectionResult { Allowed = true, Message = "登船检查通过。" };
            int fine = illegal ? 50 + _state.WantedLevel : 25;
            _state.WantedLevel = Clamp(_state.WantedLevel + (illegal ? 8 : 3), 0, 100);
            return new InspectionResult
            {
                Allowed = false,
                CargoSeized = illegal,
                Fine = fine,
                Message = illegal ? "违禁货物被扣押并处以罚款。" : "港口处于禁运，船只被拒绝入港。",
            };
        }

        public FactionActionResult CompleteEscort(string factionId, bool protectedCargo)
        {
            return ApplyReputation(factionId, protectedCargo ? 8 : -3, protectedCargo ? "护航成功，势力信任提高。" : "护航失误，势力声望下降。");
        }

        public FactionActionResult RecordPrivateerPrize(string issuerFactionId, string targetFactionId, bool hadPermit)
        {
            int issuerDelta = hadPermit ? 5 : -8;
            int wanted = hadPermit ? 0 : 12;
            var issuer = ApplyReputation(issuerFactionId, issuerDelta, hadPermit ? "合法私掠战果得到认可。" : "未经许可攻击商船，声望受损。");
            if (D4FactionCatalog.Find(targetFactionId) != null)
                ApplyReputation(targetFactionId, -4, "敌对商船损失造成关系恶化。");
            _state.WantedLevel = Clamp(_state.WantedLevel + wanted, 0, 100);
            issuer.WantedDelta = wanted;
            return issuer;
        }

        private static string WarKey(string a, string b)
        {
            if (D4FactionCatalog.Find(a) == null || D4FactionCatalog.Find(b) == null || a == b) return null;
            return string.CompareOrdinal(a, b) < 0 ? a + ":" + b : b + ":" + a;
        }

        private static int Clamp(int value, int min, int max) { return Math.Max(min, Math.Min(max, value)); }
    }
}
