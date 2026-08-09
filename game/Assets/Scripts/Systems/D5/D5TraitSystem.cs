using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D5
{
    /// <summary>50 条特质目录、确定性生成和修正值的唯一裁决者。</summary>
    public static class D5TraitSystem
    {
        public static List<string> GenerateTraitIds(string seed, int requestedCount)
        {
            int count = Math.Max(3, Math.Min(4, requestedCount));
            var result = new List<string>();
            int cursor = StableHash(seed);
            while (result.Count < count)
            {
                cursor = Next(cursor);
                var trait = D5TraitCatalog.All[Math.Abs(cursor) % D5TraitCatalog.All.Count];
                if (!result.Contains(trait.Id)) result.Add(trait.Id);
            }
            return result;
        }

        public static bool IsValidTraitSet(IEnumerable<string> traitIds, int min = 0, int max = 4)
        {
            var seen = new HashSet<string>();
            int count = 0;
            if (traitIds != null)
                foreach (var id in traitIds)
                {
                    if (D5TraitCatalog.Find(id) == null || !seen.Add(id)) return false;
                    count++;
                }
            return count >= min && count <= max;
        }

        public static int Modifier(string traitId, string modifier)
        {
            var trait = D5TraitCatalog.Find(traitId);
            return trait != null && trait.Modifier == modifier ? trait.Value : 0;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                if (value != null) foreach (char c in value) hash = hash * 31 + c;
                return hash == int.MinValue ? int.MaxValue : Math.Abs(hash);
            }
        }

        private static int Next(int value)
        {
            unchecked
            {
                int next = value * 1103515245 + 12345;
                return next == int.MinValue ? 1 : next;
            }
        }
    }
}
