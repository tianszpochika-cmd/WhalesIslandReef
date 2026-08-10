// D14-A05 势力/港口法律目录。势力定义复用 D4FactionCatalog，避免产生第二份势力名录；
// A05 只增加法律审查元数据和可保存的港口法律入口。

using System;
using System.Collections.Generic;
using Whalebone.Systems.D4;

namespace Whalebone.Systems.D14
{
    public sealed class D14FactionLegalCatalog
    {
        private readonly Dictionary<string, D14FactionLegalDefinition> _factions =
            new Dictionary<string, D14FactionLegalDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D14PortLegalDefinition> _ports =
            new Dictionary<string, D14PortLegalDefinition>(StringComparer.Ordinal);

        public D14FactionLegalCatalog()
        {
            foreach (var definition in D4FactionCatalog.All)
            {
                var copy = new D14FactionLegalDefinition
                {
                    FactionId = definition.Id, DisplayName = definition.Name, Culture = definition.Culture,
                    HistoricalTag = definition.Id == "pirates" || definition.Id == "smugglers" ? "fictional" : "inferred",
                    ReviewStatus = "approved", SourceRef = "docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md",
                };
                if (definition.HomePorts != null) copy.HomePortIds.AddRange(definition.HomePorts);
                AddFaction(copy);
            }

            AddPort(new D14PortLegalDefinition { PortId = "santa_margarita", ControllingFactionId = "spain", SourceRef = "docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md" });
            AddPort(new D14PortLegalDefinition { PortId = "green_cay", ControllingFactionId = "england", SourceRef = "docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md" });
            AddPort(new D14PortLegalDefinition { PortId = "driftwood_isle", ControllingFactionId = "france", SourceRef = "docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md" });
        }

        public IReadOnlyList<D14FactionLegalDefinition> Factions()
        {
            var result = new List<D14FactionLegalDefinition>();
            foreach (var value in _factions.Values) result.Add(value.Clone());
            return result.AsReadOnly();
        }

        public IReadOnlyList<D14PortLegalDefinition> Ports()
        {
            var result = new List<D14PortLegalDefinition>();
            foreach (var value in _ports.Values) result.Add(value.Clone());
            return result.AsReadOnly();
        }

        public D14FactionLegalDefinition FindFaction(string factionId)
        {
            D14FactionLegalDefinition value;
            return factionId != null && _factions.TryGetValue(factionId, out value) ? value.Clone() : null;
        }

        public D14PortLegalDefinition FindPort(string portId)
        {
            D14PortLegalDefinition value;
            return portId != null && _ports.TryGetValue(portId, out value) ? value.Clone() : null;
        }

        private void AddFaction(D14FactionLegalDefinition definition)
        {
            definition.Validate("d14.faction.catalog");
            if (_factions.ContainsKey(definition.FactionId)) throw new InvalidOperationException("Duplicate faction: " + definition.FactionId);
            _factions.Add(definition.FactionId, definition.Clone());
        }

        private void AddPort(D14PortLegalDefinition definition)
        {
            definition.Validate("d14.port.catalog");
            if (FindFaction(definition.ControllingFactionId) == null) throw new InvalidOperationException("Port controls unknown faction: " + definition.PortId);
            if (_ports.ContainsKey(definition.PortId)) throw new InvalidOperationException("Duplicate port: " + definition.PortId);
            _ports.Add(definition.PortId, definition.Clone());
        }
    }
}
