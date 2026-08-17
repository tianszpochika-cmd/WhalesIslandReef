// ============================================================
// D19PortAnchorCatalog —— 港口锚点目录（纯数据，D19-L04）
//
// 锚点只是业务语义键：port_id / berth_id / service_id。
// 本目录不含 Vector3、GameObject、Transform、文件路径、候选资源
// 绝对路径或任何"存在于场景"的断言；它只表达逻辑服务地址。
// 别名解析单向且无循环；废弃键返回可审计迁移提示。
// 相同输入永远得到相同规范键；未知/循环别名/跨港泊位/重复键
// 返回稳定错误码。不读取 Unity 坐标，不触碰场景。
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 锚点种类：港口 / 泊位 / 服务。
    /// </summary>
    public enum D19AnchorKind
    {
        Port,
        Berth,
        Service,
    }

    /// <summary>
    /// 稳定错误码（机器可读，禁止自由文本在 code 位置出现）。
    /// </summary>
    public static class D19AnchorErrorCodes
    {
        public const string None = "none";
        /// <summary>输入为空/空白/含路径字符/大写/非小写蛇形。</summary>
        public const string InvalidKeyFormat = "invalid_key_format";
        /// <summary>格式合法但目录中不存在该港口键。</summary>
        public const string UnknownPortKey = "unknown_port_key";
        /// <summary>格式合法但目录中不存在该泊位键。</summary>
        public const string UnknownBerthKey = "unknown_berth_key";
        /// <summary>格式合法但目录中不存在该服务键。</summary>
        public const string UnknownServiceKey = "unknown_service_key";
        /// <summary>请求的归属港口与该泊位/服务实际归属不一致。</summary>
        public const string CrossPortBerth = "cross_port_berth";
        public const string CrossPortService = "cross_port_service";
        /// <summary>别名解析出现循环（防御性；正式目录构造时已拒绝循环）。</summary>
        public const string AliasCycle = "alias_cycle";
        /// <summary>目录构造错误：重复键/重复别名（正式目录永不出现）。</summary>
        public const string DuplicateKey = "duplicate_key";
    }

    /// <summary>
    /// 解析结果：成功时给出规范键与（可选）迁移提示。
    /// </summary>
    public sealed class D19AnchorResult
    {
        public bool Success;
        public string ErrorCode = D19AnchorErrorCodes.None;
        public string Input = string.Empty;
        public D19AnchorKind Kind;
        public string CanonicalKey = string.Empty;
        /// <summary>归属港口规范键（泊位/服务必填；港口为自身键）。</summary>
        public string Port = string.Empty;
        public bool IsDeprecated;
        public string MigrationHint = string.Empty;

        public override string ToString()
        {
            return Success
                ? $"ok:{Kind}:{CanonicalKey}" + (IsDeprecated ? ":deprecated" : "")
                : $"err:{ErrorCode}:{Input}";
        }
    }

    /// <summary>
    /// 目录条目：key 可指向自身（规范键）或另一 key（别名链）。
    /// </summary>
    public sealed class D19AnchorDefinition
    {
        public string Key = string.Empty;
        /// <summary>解析目标：自身 = 规范键；否则 = 别名（单向、无环）。</summary>
        public string Canonical = string.Empty;
        public D19AnchorKind Kind;
        /// <summary>归属港口规范键。</summary>
        public string Port = string.Empty;
        /// <summary>是否为废弃键（解析成功但返回迁移提示）。</summary>
        public bool Deprecated;
        /// <summary>废弃/别名迁移提示（可审计）。</summary>
        public string MigrationHint = string.Empty;

        public D19AnchorDefinition() { }

        public D19AnchorDefinition(string key, D19AnchorKind kind, string port, bool deprecated = false, string migrationHint = "")
        {
            Key = key;
            Canonical = key;
            Kind = kind;
            Port = port;
            Deprecated = deprecated;
            MigrationHint = migrationHint;
        }
    }

    /// <summary>
    /// 港口锚点目录：稳定 port/berth/service 键 + 别名 + 废弃映射。
    /// 纯数据；确定性解析；无 Unity 依赖。
    /// </summary>
    public sealed class D19PortAnchorCatalog
    {
        private const int MaxAliasHops = 64;

        /// <summary>正式目录（构造时校验：重复键/循环别名/跨港归属/键格式）。</summary>
        public static D19PortAnchorCatalog Instance { get; } = CreateDefault();

        private readonly Dictionary<string, D19AnchorDefinition> _byKey;
        private readonly IReadOnlyList<string> _portKeys;
        private readonly IReadOnlyList<string> _berthKeys;
        private readonly IReadOnlyList<string> _serviceKeys;

        private D19PortAnchorCatalog(IReadOnlyDictionary<string, D19AnchorDefinition> byKey)
        {
            _byKey = new Dictionary<string, D19AnchorDefinition>(byKey);
            _portKeys = byKey.Values.Where(d => d.Kind == D19AnchorKind.Port).Select(d => d.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray();
            _berthKeys = byKey.Values.Where(d => d.Kind == D19AnchorKind.Berth).Select(d => d.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray();
            _serviceKeys = byKey.Values.Where(d => d.Kind == D19AnchorKind.Service).Select(d => d.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray();
        }

        public IReadOnlyList<string> PortKeys => _portKeys;
        public IReadOnlyList<string> BerthKeys => _berthKeys;
        public IReadOnlyList<string> ServiceKeys => _serviceKeys;

        // ------------------------------------------------------------
        // 公开解析 API（确定性：相同输入 → 相同输出）
        // ------------------------------------------------------------

        public D19AnchorResult ResolvePort(string input)
        {
            return Resolve(input, D19AnchorKind.Port, null);
        }

        public D19AnchorResult ResolveBerth(string input, string owningPort = null)
        {
            return Resolve(input, D19AnchorKind.Berth, owningPort);
        }

        public D19AnchorResult ResolveService(string input, string owningPort = null)
        {
            return Resolve(input, D19AnchorKind.Service, owningPort);
        }

        public bool TryResolve(string input, D19AnchorKind kind, out D19AnchorResult result)
        {
            result = Resolve(input, kind, null);
            return result.Success;
        }

        public bool TryResolve(string input, D19AnchorKind kind, string owningPort, out D19AnchorResult result)
        {
            result = Resolve(input, kind, owningPort);
            return result.Success;
        }

        private D19AnchorResult Resolve(string input, D19AnchorKind kind, string owningPort)
        {
            var result = new D19AnchorResult { Input = input ?? string.Empty, Kind = kind };
            string raw = input == null ? string.Empty : input.Trim();
            if (!IsValidKeyFormat(raw))
            {
                result.ErrorCode = D19AnchorErrorCodes.InvalidKeyFormat;
                return result;
            }
            if (!_byKey.TryGetValue(raw, out D19AnchorDefinition entry))
            {
                result.ErrorCode = UnknownCode(kind);
                return result;
            }
            if (entry.Kind != kind)
            {
                // 键存在但种类不符：按未知处理并注明种类，保证稳定错误。
                result.ErrorCode = UnknownCode(kind);
                return result;
            }

            // 别名解析：单向行走，visited 集合防环。
            var visited = new HashSet<string>(StringComparer.Ordinal);
            string current = raw;
            int hops = 0;
            while (true)
            {
                if (!visited.Add(current) || hops > MaxAliasHops)
                {
                    result.ErrorCode = D19AnchorErrorCodes.AliasCycle;
                    return result;
                }
                D19AnchorDefinition node = _byKey[current];
                if (string.Equals(node.Canonical, node.Key, StringComparison.Ordinal))
                {
                    break; // 规范键
                }
                if (!_byKey.TryGetValue(node.Canonical, out D19AnchorDefinition next))
                {
                    result.ErrorCode = UnknownCode(kind);
                    return result;
                }
                current = node.Canonical;
                hops++;
            }

            D19AnchorDefinition canonical = _byKey[current];
            if (canonical.Kind != kind)
            {
                result.ErrorCode = UnknownCode(kind);
                return result;
            }
            if (!string.IsNullOrEmpty(owningPort))
            {
                if (canonical.Kind == D19AnchorKind.Berth && !string.Equals(owningPort, canonical.Port, StringComparison.Ordinal))
                {
                    result.ErrorCode = D19AnchorErrorCodes.CrossPortBerth;
                    return result;
                }
                if (canonical.Kind == D19AnchorKind.Service && !string.Equals(owningPort, canonical.Port, StringComparison.Ordinal))
                {
                    result.ErrorCode = D19AnchorErrorCodes.CrossPortService;
                    return result;
                }
            }
            result.Success = true;
            result.CanonicalKey = current;
            result.Port = canonical.Port;
            result.IsDeprecated = entry.Deprecated;
            result.MigrationHint = entry.Deprecated ? (string.IsNullOrEmpty(entry.MigrationHint) ? MigrationHintFor(canonical) : entry.MigrationHint) : string.Empty;
            return result;
        }

        // ------------------------------------------------------------
        // 构造与校验
        // ------------------------------------------------------------

        /// <summary>
        /// 从条目集合构造目录并完整校验；失败时返回稳定错误清单。
        /// 校验：键格式、规范键唯一、跨种类重复、别名存在性、单向性、
        /// 无环（DFS）、废弃键迁移提示非空、泊位/服务归属港口存在。
        /// </summary>
        public static bool TryCreate(
            IEnumerable<D19AnchorDefinition> definitions,
            out D19PortAnchorCatalog catalog,
            out IReadOnlyList<string> errors)
        {
            var errorList = new List<string>();
            var byKey = new Dictionary<string, D19AnchorDefinition>(StringComparer.Ordinal);
            var ports = new HashSet<string>(StringComparer.Ordinal);

            foreach (var definition in definitions ?? Enumerable.Empty<D19AnchorDefinition>())
            {
                if (definition == null || string.IsNullOrEmpty(definition.Key) || string.IsNullOrEmpty(definition.Canonical))
                {
                    errorList.Add("duplicate_key: 空键/空目标");
                    continue;
                }
                if (!IsValidKeyFormat(definition.Key) || !IsValidKeyFormat(definition.Canonical))
                {
                    errorList.Add(D19AnchorErrorCodes.DuplicateKey + ": 键格式非法: " + definition.Key);
                    continue;
                }
                if (byKey.ContainsKey(definition.Key))
                {
                    errorList.Add(D19AnchorErrorCodes.DuplicateKey + ": " + definition.Key);
                    continue;
                }
                byKey[definition.Key] = definition;
            }

            foreach (var definition in byKey.Values)
            {
                if (definition.Kind == D19AnchorKind.Port)
                {
                    ports.Add(definition.Key);
                }
            }
            foreach (var definition in byKey.Values)
            {
                if (definition.Kind != D19AnchorKind.Port && !ports.Contains(definition.Port))
                {
                    errorList.Add("cross_port: 归属港口不存在: " + definition.Key);
                }
                if (!byKey.ContainsKey(definition.Canonical))
                {
                    errorList.Add("alias_missing_target: " + definition.Key + " -> " + definition.Canonical);
                }
                if (definition.Deprecated && string.IsNullOrWhiteSpace(definition.MigrationHint))
                {
                    errorList.Add("deprecated_without_hint: " + definition.Key);
                }
            }
            // 单向性 + 无环（DFS）
            foreach (var definition in byKey.Values)
            {
                var path = new HashSet<string>(StringComparer.Ordinal);
                string current = definition.Key;
                while (!string.Equals(current, byKey[current].Canonical, StringComparison.Ordinal))
                {
                    if (!path.Add(current))
                    {
                        errorList.Add(D19AnchorErrorCodes.AliasCycle + ": " + definition.Key);
                        break;
                    }
                    string next = byKey[current].Canonical;
                    if (string.Equals(byKey[next].Canonical, current, StringComparison.Ordinal))
                    {
                        errorList.Add(D19AnchorErrorCodes.AliasCycle + ": " + definition.Key);
                        break;
                    }
                    current = next;
                }
            }

            if (errorList.Count > 0)
            {
                catalog = null;
                errors = errorList;
                return false;
            }
            catalog = new D19PortAnchorCatalog(byKey);
            errors = Array.Empty<string>();
            return true;
        }

        private static D19PortAnchorCatalog CreateDefault()
        {
            if (!TryCreate(BuildDefaultDefinitions(), out D19PortAnchorCatalog catalog, out IReadOnlyList<string> errors))
            {
                throw new InvalidOperationException("D19PortAnchorCatalog 默认数据构造失败: " + string.Join("; ", errors));
            }
            return catalog;
        }

        // ------------------------------------------------------------
        // 冻结默认数据（业务语义键；以 D3 PortCatalog / D9 泊位 /
        // D9PortService / 港口设计文档为事实来源）
        // ------------------------------------------------------------

        private const string PortSantaMargarita = "port_santa_margarita";
        private const string PortGreenCay = "port_green_cay";
        private const string PortDriftwoodIsle = "port_driftwood_isle";

        internal static IReadOnlyList<D19AnchorDefinition> BuildDefaultDefinitions()
        {
            var list = new List<D19AnchorDefinition>();

            // --- 港口（规范键 port_*；D3 裸键为废弃别名，迁移提示指向规范键） ---
            list.Add(new D19AnchorDefinition(PortSantaMargarita, D19AnchorKind.Port, PortSantaMargarita));
            list.Add(new D19AnchorDefinition(PortGreenCay, D19AnchorKind.Port, PortGreenCay));
            list.Add(new D19AnchorDefinition(PortDriftwoodIsle, D19AnchorKind.Port, PortDriftwoodIsle));
            list.Add(DeprecatedAlias("santa_margarita", D19AnchorKind.Port, PortSantaMargarita, PortSantaMargarita,
                "D3 历史港口键；迁移至 " + PortSantaMargarita));
            list.Add(DeprecatedAlias("green_cay", D19AnchorKind.Port, PortGreenCay, PortGreenCay,
                "D3 历史港口键；迁移至 " + PortGreenCay));
            list.Add(DeprecatedAlias("driftwood_isle", D19AnchorKind.Port, PortDriftwoodIsle, PortDriftwoodIsle,
                "D3 历史港口键；迁移至 " + PortDriftwoodIsle));

            // --- 泊位（规范键 berth_<port>_<zone>_<n>） ---
            list.Add(new D19AnchorDefinition("berth_santa_margarita_dock_01", D19AnchorKind.Berth, PortSantaMargarita));
            list.Add(new D19AnchorDefinition("berth_santa_margarita_large_01", D19AnchorKind.Berth, PortSantaMargarita));
            list.Add(new D19AnchorDefinition("berth_santa_margarita_small_01", D19AnchorKind.Berth, PortSantaMargarita));
            list.Add(new D19AnchorDefinition("berth_santa_margarita_cargo_01", D19AnchorKind.Berth, PortSantaMargarita));
            list.Add(new D19AnchorDefinition("berth_santa_margarita_anchorage_01", D19AnchorKind.Berth, PortSantaMargarita));
            list.Add(new D19AnchorDefinition("berth_green_cay_01", D19AnchorKind.Berth, PortGreenCay));
            list.Add(new D19AnchorDefinition("berth_green_cay_small_01", D19AnchorKind.Berth, PortGreenCay));
            list.Add(new D19AnchorDefinition("berth_driftwood_isle_01", D19AnchorKind.Berth, PortDriftwoodIsle));
            list.Add(DeprecatedAlias("building_saint_marys_dock", D19AnchorKind.Berth, PortSantaMargarita, "berth_santa_margarita_dock_01",
                "D9 历史泊位键（saint_marys 命名与港口 santa_margarita 不一致）；迁移至 berth_santa_margarita_dock_01"));

            // --- 服务（规范键 service_<port>_<name>；词汇来自 D9PortService 与港口设计文档） ---
            string[] santaServices =
            {
                "market", "tavern", "shipyard", "supplies", "repair",
                "recruitment", "wages", "warehouse", "customs",
            };
            foreach (string service in santaServices)
            {
                list.Add(new D19AnchorDefinition("service_" + PortSantaMargarita.Remove(0, "port_".Length) + "_" + service,
                    D19AnchorKind.Service, PortSantaMargarita));
            }
            string[] greenCayServices = { "market", "tavern", "supplies", "repair", "wages", "warehouse" };
            foreach (string service in greenCayServices)
            {
                list.Add(new D19AnchorDefinition("service_" + PortGreenCay.Remove(0, "port_".Length) + "_" + service,
                    D19AnchorKind.Service, PortGreenCay));
            }
            // 荒岛无服务（设计文档：荒岛以资源、危险和历史地点为主）

            return list;
        }

        // ------------------------------------------------------------
        // 工具
        // ------------------------------------------------------------

        private static D19AnchorDefinition DeprecatedAlias(string key, D19AnchorKind kind, string port, string canonicalTarget, string migrationHint)
        {
            return new D19AnchorDefinition(key, kind, port, true, migrationHint)
            {
                Canonical = canonicalTarget,
            };
        }

        private static bool IsValidKeyFormat(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            foreach (char c in key)
            {
                bool lower = c >= 'a' && c <= 'z';
                bool digit = c >= '0' && c <= '9';
                bool underscore = c == '_';
                if (!lower && !digit && !underscore)
                {
                    return false;
                }
            }
            return true;
        }

        private static string UnknownCode(D19AnchorKind kind)
        {
            switch (kind)
            {
                case D19AnchorKind.Port:
                    return D19AnchorErrorCodes.UnknownPortKey;
                case D19AnchorKind.Berth:
                    return D19AnchorErrorCodes.UnknownBerthKey;
                default:
                    return D19AnchorErrorCodes.UnknownServiceKey;
            }
        }

        private static string MigrationHintFor(D19AnchorDefinition canonical)
        {
            return "已迁移至 " + canonical.Key;
        }
    }
}
