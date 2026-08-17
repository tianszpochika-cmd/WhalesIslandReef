// ============================================================
// D19MarketPortAdapter —— 港口市场适配器（纯 C#，D19-L11）
//
// 封装港口市场请求：港口上下文、权限/服务前置、命令 ID 透传与
// D3 公开调用（TradeEconomySystem.Buy/Sell 幂等入口）。D3 仍独占
// 价格、货币与库存裁决：本适配器不缓存任何可写库存/货币引用，
// 只把调用方提供的活 D3TradeState 透传给 D3，并把 D3 的成功/失败
// 投影为 D19 失败码（L17 目录）、本地化 key（L18）与事件草案
// （L16 工厂，不追加）。D3 原始失败码永远保留在 OriginalD3Code，
// 绝不吞掉。任何 D3 接口不匹配 → 明确阻断（Rejected/BLOCKED），
// 不自建货币/库存模型。
// 无 Unity 依赖；不读取系统时钟/场景；无写路径（事件草案与结果
// 只是数据，追加由调用方/EventLog 完成）。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 港口市场交易请求（纯数据）。CommandId 透传 D3 幂等与事件草案；
    /// AvailabilityGate 为调用方（L33 路由）注入的 D19-L05 可用性裁决。
    /// </summary>
    public sealed class D19MarketTradeRequest
    {
        public string CommandId = string.Empty;
        public string ActorId = string.Empty;
        public string ShipId = string.Empty;
        /// <summary>港口键（L04 锚点目录；D3 裸键别名可解析）。</summary>
        public string PortId = string.Empty;
        /// <summary>可选泊位键；提供时必须属于请求港（L23 校验）。</summary>
        public string BerthId = string.Empty;
        /// <summary>服务 ID（必须为 L18 ServiceIdMarket="market"）。</summary>
        public string ServiceId = D19PortLocalizationKeys.ServiceIdMarket;
        public string CommodityId = string.Empty;
        public int Quantity;
        /// <summary>逻辑世界日（事件草案用；非系统时钟）。</summary>
        public int WorldDay;
        /// <summary>透传 D3 幂等修订号（0 = 由 D3 自增）。</summary>
        public long SaveRevision;
        /// <summary>L05 可用性裁决（可选；提供且非 Available 时直接不可用）。</summary>
        public D19ServiceAvailabilityResult AvailabilityGate;
        /// <summary>D3 价格上下文（可选；缺省时按 D3 自身默认口径从市场状态只读构建）。</summary>
        public TradePriceContext PriceContext;
    }

    /// <summary>
    /// 港口市场交易结果（纯数据）。Success=false 时 ReasonCode 来自 L17、
    /// LocKey 来自 L18；SaveRevision 与 EventDraft 供 L22 幂等与 L20 保存消费。
    /// </summary>
    public sealed class D19MarketTradeResult
    {
        public bool Success;
        public bool DuplicateSettlement;
        public string CommandId = string.Empty;
        /// <summary>D19 失败码（L17 目录；Success=false 时有效）。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>本地化 key（L18；Success=false 时有效）。</summary>
        public string LocKey = string.Empty;
        public D19FailureRetryPolicy RetryPolicy;
        public string CommodityId = string.Empty;
        public int Quantity;
        public int TotalPrice;
        public int MoneyAfter;
        /// <summary>D3 保存修订号（L20 保存投影消费）。</summary>
        public long SaveRevision;
        /// <summary>D3 原始失败码（永远保留，绝不吞掉）。</summary>
        public string OriginalD3Code = string.Empty;
        /// <summary>事件草案（L16 工厂构造，未追加；由调用方决定落日志）。</summary>
        public WorldEvent EventDraft;
        /// <summary>追加说明（接口缺口/近似映射，机器可读稳定文本）。</summary>
        public string Note = string.Empty;
    }

    /// <summary>
    /// 港口市场适配器（D19-L11，所有者：本文件）。
    /// 静态纯函数；只调用 D3 公开契约；无缓存、无写路径。
    /// </summary>
    public static class D19MarketPortAdapter
    {
        // ---------- 冻结 D3 失败码 → L17 目录投影表 ----------
        // 每个 D3 稳定码必须在此登记；无精确 L17 语义的码使用保守近似
        // 并保留 OriginalD3Code，绝不吞掉原始失败。缺失映射 → 明确阻断。

        private sealed class Projection
        {
            public string D3Code;
            public string L17Code;
            public string Note;
            public Projection(string d3Code, string l17Code, string note)
            {
                D3Code = d3Code;
                L17Code = l17Code;
                Note = note;
            }
        }

        private static readonly Projection[] D3ToL17 = new Projection[]
        {
            new Projection("not_enough_money", "d19.trade.funds_insufficient", "D3 货币裁决：资金不足"),
            new Projection("market_empty", "d19.service.unavailable", "近似：市场缺货 = 服务无法履约；原始码保留"),
            new Projection("weight_full", "d19.inventory.capacity_exceeded", "D3 库存裁决：重量超限"),
            new Projection("cargo_full", "d19.inventory.capacity_exceeded", "D3 库存裁决：货舱体积不足"),
            new Projection("invalid_reference", "d19.service.unavailable", "近似：引用无法被市场服务满足（港口已前置校验，多为货物引用）"),
            new Projection("invalid_quantity", "d19.inventory.operation_failed", "近似：数量非法导致操作失败"),
            new Projection("invalid_state", "d19.service.unavailable", "D3 状态缺失：市场服务不可用"),
            new Projection("invalid_revision", "d19.save.conflict", "D3 修订号无效 = 保存/幂等冲突"),
            new Projection("duplicate_settlement", "d19.save.conflict", "D3 幂等去重：事实已结算（Succeeded=true 由 DuplicateSettlement 透传）"),
        };

        // ---------- 公开入口 ----------

        public static D19MarketTradeResult Buy(
            D3TradeState state,
            TradeEconomySystem economy,
            D19MarketTradeRequest request,
            D19AuthorityClaims claims)
        {
            return Execute(state, economy, request, claims, D3MarketAction.Buy);
        }

        public static D19MarketTradeResult Sell(
            D3TradeState state,
            TradeEconomySystem economy,
            D19MarketTradeRequest request,
            D19AuthorityClaims claims)
        {
            return Execute(state, economy, request, claims, D3MarketAction.Sell);
        }

        // ---------- 执行管线 ----------

        private static D19MarketTradeResult Execute(
            D3TradeState state,
            TradeEconomySystem economy,
            D19MarketTradeRequest request,
            D19AuthorityClaims claims,
            D3MarketAction action)
        {
            var result = new D19MarketTradeResult
            {
                CommandId = request == null ? string.Empty : request.CommandId,
                CommodityId = request == null ? string.Empty : request.CommodityId,
                Quantity = request == null ? 0 : request.Quantity,
            };

            // 1) 输入校验（接口不匹配 → 明确阻断，不调 D3）
            if (request == null)
            {
                return Blocked(result, "request_null", "请求对象为空；接口不匹配，未调用 D3");
            }
            if (state == null)
            {
                return Blocked(result, "d3_state_null", "D3TradeState 为空；接口不匹配，未调用 D3");
            }
            if (economy == null)
            {
                return Blocked(result, "d3_economy_null", "TradeEconomySystem 为空；接口不匹配，未调用 D3");
            }
            if (string.IsNullOrEmpty(request.CommandId))
            {
                return Blocked(result, "missing_command_id", "command_id 必填（幂等与事件关联）；接口不匹配");
            }
            if (string.IsNullOrEmpty(request.CommodityId))
            {
                return Blocked(result, "missing_commodity_id", "commodity_id 必填；接口不匹配");
            }
            if (request.Quantity <= 0)
            {
                return Blocked(result, "invalid_quantity", "quantity 必须为正整数；接口不匹配");
            }
            if (request.ServiceId != D19PortLocalizationKeys.ServiceIdMarket)
            {
                return Blocked(result, "service_not_market", "本适配器只处理 market 服务；接口不匹配");
            }

            // 2) 权限/服务前置（L23 权威校验：港口/泊位/服务键、跨船/跨玩家/跨港）
            // 注意：L23 的 ServiceId 是 L04 锚点目录键（service_<port>_<name>），
            // 而请求方使用 L18 裸服务 ID（market）；这里按已解析端口构造目录键。
            var portResolution = D19PortAnchorCatalog.Instance.ResolvePort(request.PortId);
            string portName = portResolution.Success
                ? portResolution.CanonicalKey.Substring("port_".Length)
                : request.PortId;
            string catalogServiceId = "service_" + portName + "_" + request.ServiceId;
            var authority = D19PortAuthorityValidator.Validate(
                new D19AuthorityContext(
                    D19PortCommandKind.StartService,
                    request.ActorId,
                    request.ShipId,
                    request.PortId,
                    string.IsNullOrEmpty(request.BerthId) ? null : request.BerthId,
                    catalogServiceId,
                    null),
                claims);
            if (!authority.Allowed)
            {
                return FromAuthority(result, authority);
            }

            // 3) 可用性前置（L05 裁决：注入的 gate 或市场登记最小前置）
            var gate = request.AvailabilityGate;
            if (gate == null)
            {
                gate = D19PortAvailabilitySystem.Adjudicate(
                    authority.CanonicalPortId,
                    request.ServiceId,
                    D19ServiceTimePeriod.Morning,
                    D19FactionPermission.Allowed,
                    D19WeatherLabel.Clear,
                    D19FacilityState.Open,
                    true);
            }
            if (gate.Verdict != D19AvailabilityVerdict.Available)
            {
                result.Success = false;
                result.ReasonCode = gate.ReasonCode;
                result.LocKey = gate.LocKey;
                result.RetryPolicy = gate.RetryPolicy;
                result.Note = "service gate: " + gate.Detail;
                return result;
            }

            // 4) 港口键 → D3 键（D3 PortCatalog 使用裸键：port_X → X）
            string d3PortId = authority.CanonicalPortId.StartsWith("port_", StringComparison.Ordinal)
                ? authority.CanonicalPortId.Substring("port_".Length)
                : authority.CanonicalPortId;

            // 5) D3 公开调用（幂等入口；活 state 透传，无缓存副本）
            TradePriceContext context = request.PriceContext;
            if (context == null)
            {
                context = new TradePriceContext
                {
                    WorldDay = request.WorldDay,
                    Weather = state.Market == null ? D3WeatherType.Calm : state.Market.Weather,
                    FactionPolicyId = state.Market == null ? "neutral" : state.Market.FactionPolicyId,
                    FactionPolicyCoefficient = state.Market == null ? 1f : state.Market.FactionPolicyCoefficient,
                };
            }
            TradeResult trade = action == D3MarketAction.Buy
                ? economy.Buy(state, d3PortId, request.CommodityId, request.Quantity, context, request.SaveRevision)
                : economy.Sell(state, d3PortId, request.CommodityId, request.Quantity, context, request.SaveRevision);

            // 6) 投影 D3 结果 → D19 结果（原始码保留）
            return Project(state, request, trade, action);
        }

        private static D19MarketTradeResult Project(
            D3TradeState state,
            D19MarketTradeRequest request,
            TradeResult trade,
            D3MarketAction action)
        {
            var result = new D19MarketTradeResult
            {
                CommandId = request.CommandId,
                CommodityId = request.CommodityId,
                Quantity = trade == null ? request.Quantity : trade.Quantity,
                SaveRevision = trade == null ? request.SaveRevision : trade.SaveRevision,
                MoneyAfter = trade == null ? (state == null ? 0 : state.Money) : trade.MoneyAfter,
                TotalPrice = trade == null ? 0 : trade.TotalPrice,
                OriginalD3Code = trade == null ? "d3_result_null" : trade.Code,
            };

            if (trade == null)
            {
                return Blocked(result, "d3_result_null", "D3 返回空结果；接口不匹配，明确阻断");
            }

            if (trade.Succeeded)
            {
                result.Success = true;
                result.DuplicateSettlement = trade.DuplicateSettlement;
                result.ReasonCode = string.Empty;
                result.LocKey = D19PortLocalizationKeys.ReadModelCommandAccepted;
                result.RetryPolicy = D19FailureRetryPolicy.NoRetry;
                result.Note = trade.DuplicateSettlement ? "d3 duplicate_settlement passed through (idempotent)" : string.Empty;
                result.EventDraft = BuildEventDraft(request, action, true, trade, result);
                return result;
            }

            // D3 失败 → L17 投影（映射缺失 → 明确阻断）
            var projection = FindProjection(trade.Code);
            if (projection == null)
            {
                return Blocked(result, "d3_code_unmapped:" + trade.Code, "D3 失败码未登记投影；接口不匹配，明确阻断");
            }
            D19FailureEntry entry;
            if (!D19PortFailureCatalog.TryResolve(projection.L17Code, out entry))
            {
                return Blocked(result, "l17_entry_missing:" + projection.L17Code, "L17 目录缺少映射条目；明确阻断");
            }
            result.Success = false;
            result.ReasonCode = entry.Code;
            result.LocKey = entry.LocKey;
            result.RetryPolicy = entry.Retry;
            result.Note = projection.Note;
            result.EventDraft = BuildEventDraft(request, action, false, trade, result);
            return result;
        }

        // ---------- 事件草案（L16 工厂；只构造不追加） ----------

        private static WorldEvent BuildEventDraft(
            D19MarketTradeRequest request,
            D3MarketAction action,
            bool succeeded,
            TradeResult trade,
            D19MarketTradeResult result)
        {
            string resultCode = succeeded
                ? (action == D3MarketAction.Buy ? "port.market.buy_success" : "port.market.sell_success")
                : (action == D3MarketAction.Buy ? "port.market.buy_rejected" : "port.market.sell_rejected");
            var values = new Dictionary<string, string>
            {
                { "commodity_id", request.CommodityId },
                { "quantity", request.Quantity.ToString() },
                { "save_revision", result.SaveRevision.ToString() },
            };
            if (succeeded)
            {
                values["total_price"] = result.TotalPrice.ToString();
                values["money_after"] = result.MoneyAfter.ToString();
            }
            else
            {
                values["d3_code"] = trade.Code;
                values["reason_code"] = result.ReasonCode;
            }
            return D19PortEventFactory.Create(
                D19PortEventType.ServiceResult,
                request.CommandId,
                request.WorldDay,
                result.SaveRevision,
                request.PortId,
                request.ShipId,
                new List<string> { request.ActorId },
                new List<string>(),
                resultCode,
                D19PortLocalizationKeys.PortEventServiceResult,
                new List<string>(),
                values);
        }

        // ---------- 工具 ----------

        private static Projection FindProjection(string d3Code)
        {
            foreach (var projection in D3ToL17)
            {
                if (projection.D3Code == d3Code)
                {
                    return projection;
                }
            }
            return null;
        }

        private static D19MarketTradeResult Blocked(D19MarketTradeResult result, string reasonCode, string note)
        {
            result.Success = false;
            result.ReasonCode = reasonCode;
            result.LocKey = D19PortLocalizationKeys.MissingKey;
            result.RetryPolicy = D19FailureRetryPolicy.NoRetry;
            result.Note = note;
            return result;
        }

        private static D19MarketTradeResult FromAuthority(D19MarketTradeResult result, D19AuthorityResult authority)
        {
            result.Success = false;
            result.ReasonCode = authority.FailureCode;
            result.LocKey = authority.LocKey;
            result.RetryPolicy = authority.Failure == null
                ? D19FailureRetryPolicy.NoRetry
                : authority.Failure.Retry;
            result.Note = "authority: " + authority.Cause;
            return result;
        }

        // ---------- 自检（确定性） ----------

        /// <summary>
        /// 自检：投影表每条 D3 码 → L17 码可解析、L17 条目 key 合法且在 L18 注册表、
        /// 服务 ID 恒为 market。返回空列表表示健全。
        /// </summary>
        public static List<string> Validate()
        {
            List<string> problems = new List<string>();
            var seenD3 = new HashSet<string>();
            foreach (var projection in D3ToL17)
            {
                if (string.IsNullOrEmpty(projection.D3Code))
                {
                    problems.Add("projection with empty d3 code");
                    continue;
                }
                if (!seenD3.Add(projection.D3Code))
                {
                    problems.Add("duplicate d3 code projection: " + projection.D3Code);
                }
                D19FailureEntry entry;
                if (!D19PortFailureCatalog.TryResolve(projection.L17Code, out entry))
                {
                    problems.Add("l17 entry missing for " + projection.D3Code + " -> " + projection.L17Code);
                    continue;
                }
                if (!D19PortLocalizationKeys.IsValidKey(entry.LocKey))
                {
                    problems.Add("invalid loc key for " + projection.L17Code + ": " + entry.LocKey);
                }
            }
            if (!D19PortLocalizationKeys.IsValidKey(D19PortLocalizationKeys.ReadModelCommandAccepted))
            {
                problems.Add("readmodel.command_accepted key invalid");
            }
            problems.Sort(StringComparer.Ordinal);
            return problems;
        }
    }
}
