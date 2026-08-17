// ============================================================
// D19TavernPortAdapter —— 港口酒馆适配器（纯 C#，D19-L13）
//
// 封装酒馆服务（Visit）、招募（Recruit）与社交（Social）请求的港口上下文：
// 先由 D19-L05 判定服务/NPC 逻辑可用性、D19-L23 判定权限，再由原系统
// （D9PortShipPreparationSystem / D10TavernSocialSystem）裁决费用、关系、
// 候选船员与状态修改；D19 只把结果投影为稳定命令结果、L17 失败码、L18 key
// 与 L16 事件草案（只构造不追加）。原系统原始失败码永远保留在
// OriginalOwnerCode，绝不吞掉；映射缺失 → 明确阻断（Blocked）。
// 禁止直接改关系、金钱、NPC 行为、模型或对话 UI——本类无任何写路径。
// 无 Unity 依赖；不读取系统时钟/登录状态；重复命令通过原系统去重
// （D9 duplicate_settlement / D10 duplicate_tavern_request）与稳定事件草案
// （event_id + save_revision 去重由 EventLog 完成）保证不产生双重副作用。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 酒馆请求种类（D19-L13）：酒馆服务 / 招募 / 社交。
    /// </summary>
    public enum D19TavernRequestKind
    {
        Visit,
        Recruit,
        Social,
    }

    /// <summary>
    /// 港口酒馆请求（纯数据）。CommandId 透传幂等与事件草案；
    /// AvailabilityGate 为调用方注入的 D19-L05 裁决（可选）。
    /// </summary>
    public sealed class D19TavernRequest
    {
        public string CommandId = string.Empty;
        public string ActorId = string.Empty;
        public string ShipId = string.Empty;
        /// <summary>港口键（L04 锚点目录；D3/D9 裸键别名可解析）。</summary>
        public string PortId = string.Empty;
        /// <summary>可选泊位键；提供时必须属于请求港（L23 校验）。</summary>
        public string BerthId = string.Empty;
        /// <summary>服务 ID（L18 口径：Visit/Social="tavern"，Recruit="recruitment"）。</summary>
        public string ServiceId = string.Empty;
        /// <summary>逻辑世界日（事件草案用；非系统时钟）。</summary>
        public int WorldDay;
        /// <summary>透传原系统幂等修订号（0 = 由原系统自增）。</summary>
        public long SaveRevision;
        /// <summary>L05 可用性输入（未注入 AvailabilityGate 时用于裁决）。</summary>
        public D19ServiceTimePeriod TimePeriod;
        public D19FactionPermission FactionPermission;
        public D19WeatherLabel Weather;
        public D19FacilityState FacilityState;
        public bool Staffed;
        /// <summary>L05 可用性裁决（可选；提供且非 Available 时直接不可用）。</summary>
        public D19ServiceAvailabilityResult AvailabilityGate;
        /// <summary>招募目标候选人 ID（仅 Recruit）。</summary>
        public string CandidateId = string.Empty;
        /// <summary>社交请求（仅 Social；D10 原系统请求，PortId 采用 D10 自身键约定）。</summary>
        public D10TavernRequest SocialRequest;
    }

    /// <summary>
    /// 港口酒馆结果（纯数据）。Success=false 时 ReasonCode 来自 L17、LocKey 来自 L18；
    /// OriginalOwnerCode 永远保留原系统失败码；EventDraft 供调用方落日志。
    /// </summary>
    public sealed class D19TavernResult
    {
        public bool Success;
        public bool DuplicateSettlement;
        public string CommandId = string.Empty;
        /// <summary>D19 失败码（L17 目录；Success=false 时有效）。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>本地化 key（L18；Success=false 时有效）。</summary>
        public string LocKey = string.Empty;
        public D19FailureRetryPolicy RetryPolicy;
        /// <summary>原系统（D9/D10）原始失败码（永远保留，绝不吞掉）。</summary>
        public string OriginalOwnerCode = string.Empty;
        public int Cost;
        public int CrewCount;
        public int MoneyAfter;
        /// <summary>保存修订号（事件草案/投影用；透传请求值或原系统值）。</summary>
        public long SaveRevision;
        public string CandidateId = string.Empty;
        /// <summary>事件草案（L16 工厂构造，未追加；由调用方决定落日志）。</summary>
        public WorldEvent EventDraft;
        /// <summary>追加说明（接口缺口/近似映射/覆盖缺口，机器可读稳定文本）。</summary>
        public string Note = string.Empty;
    }

    /// <summary>
    /// 港口酒馆适配器（D19-L13，所有者：本文件）。
    /// 静态纯函数；只调用原系统公开契约；无缓存、无写路径。
    /// </summary>
    public static class D19TavernPortAdapter
    {
        /// <summary>招募服务稳定 ID（L18 服务注册表未收录 recruitment；本适配器本地定义，
        /// L05 已知服务集也不含它 → 招募可用性由 D9 CheckService 原系统裁决）。</summary>
        public const string RecruitmentServiceId = "recruitment";

        // ---------- 服务键对齐（L18 服务 ID → L04 锚点目录服务键后缀） ----------

        /// <summary>服务 ID → 锚点目录服务键后缀（与 L05 CatalogServiceSuffix 口径一致）。</summary>
        private static readonly Dictionary<string, string> CatalogServiceSuffix =
            new Dictionary<string, string>
            {
                { D19PortLocalizationKeys.ServiceIdTavern, "tavern" },
                { RecruitmentServiceId, "recruitment" },
            };

        // ---------- 冻结原系统失败码 → L17 目录投影表 ----------
        // 每个真实原系统码必须在此登记；无精确 L17 语义的码使用保守近似并保留
        // OriginalOwnerCode；缺失映射 → 明确阻断。绝不吞掉原始失败。

        private sealed class Projection
        {
            public string OwnerCode;
            public string L17Code;
            public string Note;
            public Projection(string ownerCode, string l17Code, string note)
            {
                OwnerCode = ownerCode;
                L17Code = l17Code;
                Note = note;
            }
        }

        /// <summary>D9 酒馆/招募相关失败码 → L17（来自 D9PortShipPreparationSystem 真实码）。</summary>
        private static readonly Projection[] D9ToL17 = new Projection[]
        {
            new Projection("not_enough_money", "d19.trade.funds_insufficient", "D9 货币裁决：费用失败（TrySpend）"),
            new Projection("crew_full", "d19.inventory.capacity_exceeded", "近似：船员编制容量不足 = 容量失败；原始码保留"),
            new Projection("already_recruited", "d19.inventory.operation_failed", "近似：成员已在船上 = 操作失败；原始码保留"),
            new Projection("invalid_candidate", "d19.inventory.operation_failed", "近似：招募对象无效 = 操作失败；原始码保留"),
            new Projection("invalid_state", "d19.service.unavailable", "近似：原系统状态缺失 = 服务不可用；原始码保留"),
            new Projection("invalid_port", "d19.port.unknown_port", "D9 港口裁决：港口不存在"),
            new Projection("not_moored", "d19.lifecycle.invalid_transition", "近似：未靠泊使用服务 = 生命周期前置不满足；原始码保留"),
            new Projection("invalid_quantity", "d19.inventory.operation_failed", "近似：数量非法 = 操作失败；原始码保留"),
            new Projection("duplicate_settlement", "d19.save.conflict", "D9 幂等去重：事实已结算（Accepted=true 由 DuplicateSettlement 透传）"),
        };

        /// <summary>D10 酒馆请求拒绝码 → L17（来自 D10TavernSocialSystem.ValidateRequest/RequestAction 真实码）。</summary>
        private static readonly Projection[] D10ToL17 = new Projection[]
        {
            new Projection("request_required", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("actor_required", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("port_required", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("negative_time_or_revision", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("unknown_action", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("unknown_sensitivity", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("invalid_cause_event", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("candidate_required", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("crew_required", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("invalid_rest_hours", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("invalid_alcohol_strength", "d19.inventory.operation_failed", "近似：D10 请求校验拒绝 = 操作失败；原始码保留"),
            new Projection("rumor_not_found", "d19.inventory.operation_failed", "近似：引用不存在 = 操作失败；原始码保留"),
            new Projection("poker_table_not_found", "d19.inventory.operation_failed", "近似：引用不存在 = 操作失败；原始码保留"),
            new Projection("side_job_not_found", "d19.inventory.operation_failed", "近似：引用不存在 = 操作失败；原始码保留"),
            new Projection("warning_required", "d19.port.permission_denied", "近似：敏感内容警告未确认 = 访问被拒；原始码保留"),
        };

        // ---------- 公开入口 ----------

        /// <summary>酒馆服务（D9.VisitTavern：费用/士气/疲劳由 D9 裁决）。</summary>
        public static D19TavernResult Visit(
            D3TradeState state,
            ShipState ship,
            IList<CrewAgent> crew,
            D9PortShipPreparationSystem preparation,
            D19TavernRequest request,
            D19AuthorityClaims claims)
        {
            return ExecuteVisitOrRecruit(state, ship, crew, preparation, null, request, claims, D19TavernRequestKind.Visit);
        }

        /// <summary>招募（D9.Recruit：费用/候选船员/入列由 D9 裁决）。</summary>
        public static D19TavernResult Recruit(
            D3TradeState state,
            ShipState ship,
            IList<CrewAgent> crew,
            D9PortShipPreparationSystem preparation,
            CrewAgent candidate,
            D19TavernRequest request,
            D19AuthorityClaims claims)
        {
            return ExecuteVisitOrRecruit(state, ship, crew, preparation, candidate, request, claims, D19TavernRequestKind.Recruit);
        }

        /// <summary>社交（D10TavernSocialSystem.RequestAction：请求校验/事实追加/敏感警告由 D10 裁决）。</summary>
        public static D19TavernResult Social(
            D10TavernSocialSystem tavern,
            D19TavernRequest request,
            D19AuthorityClaims claims)
        {
            var result = new D19TavernResult
            {
                CommandId = request == null ? string.Empty : request.CommandId,
                CandidateId = request == null ? string.Empty : request.CandidateId,
                SaveRevision = request == null ? 0L : request.SaveRevision,
            };

            // 1) 输入校验（接口不匹配 → 明确阻断，不调原系统）
            if (request == null)
            {
                return Blocked(result, "request_null", "请求对象为空；接口不匹配，未调用 D10");
            }
            if (tavern == null)
            {
                return Blocked(result, "d10_tavern_null", "D10TavernSocialSystem 为空；接口不匹配，未调用 D10");
            }
            if (string.IsNullOrEmpty(request.CommandId))
            {
                return Blocked(result, "missing_command_id", "command_id 必填（幂等与事件关联）；接口不匹配");
            }
            if (request.SocialRequest == null)
            {
                return Blocked(result, "missing_social_request", "social_request 必填；接口不匹配");
            }
            if (request.ServiceId != D19PortLocalizationKeys.ServiceIdTavern)
            {
                return Blocked(result, "service_not_tavern", "本适配器社交路径只处理 tavern 服务；接口不匹配");
            }

            // 2) 权限/服务前置（L23：港口/泊位/服务键、跨船/跨玩家/跨港）
            D19AuthorityResult authority = ValidateAuthority(request, claims, "tavern");
            if (!authority.Allowed)
            {
                return FromAuthority(result, authority);
            }

            // 3) 可用性前置（L05：酒馆服务与 NPC 逻辑可用性）
            bool gateSkipped;
            D19ServiceAvailabilityResult gate = ResolveGate(authority, request, "tavern", out gateSkipped);
            if (!gateSkipped && gate.Verdict != D19AvailabilityVerdict.Available)
            {
                return FromGate(result, gate);
            }

            // 4) D10 原系统裁决（请求校验/敏感警告/事实追加去重）
            D10TavernRequestResult owner = tavern.RequestAction(request.SocialRequest);

            // 5) 投影 D10 结果 → D19 结果（原始码保留）
            return ProjectSocial(result, request, owner);
        }

        // ---------- Visit/Recruit 共用管线 ----------

        private static D19TavernResult ExecuteVisitOrRecruit(
            D3TradeState state,
            ShipState ship,
            IList<CrewAgent> crew,
            D9PortShipPreparationSystem preparation,
            CrewAgent candidate,
            D19TavernRequest request,
            D19AuthorityClaims claims,
            D19TavernRequestKind kind)
        {
            var result = new D19TavernResult
            {
                CommandId = request == null ? string.Empty : request.CommandId,
                CandidateId = request == null ? string.Empty : request.CandidateId,
                SaveRevision = request == null ? 0L : request.SaveRevision,
            };

            // 1) 输入校验（接口不匹配 → 明确阻断，不调 D9）
            if (request == null)
            {
                return Blocked(result, "request_null", "请求对象为空；接口不匹配，未调用 D9");
            }
            if (state == null)
            {
                return Blocked(result, "d3_state_null", "D3TradeState 为空；接口不匹配，未调用 D9");
            }
            if (ship == null)
            {
                return Blocked(result, "ship_null", "ShipState 为空；接口不匹配，未调用 D9");
            }
            if (preparation == null)
            {
                return Blocked(result, "d9_preparation_null", "D9PortShipPreparationSystem 为空；接口不匹配，未调用 D9");
            }
            if (crew == null)
            {
                return Blocked(result, "crew_null", "crew 列表为空；接口不匹配，未调用 D9");
            }
            if (string.IsNullOrEmpty(request.CommandId))
            {
                return Blocked(result, "missing_command_id", "command_id 必填（幂等与事件关联）；接口不匹配");
            }
            string serviceId = kind == D19TavernRequestKind.Recruit
                ? RecruitmentServiceId
                : D19PortLocalizationKeys.ServiceIdTavern;
            if (!string.IsNullOrEmpty(request.ServiceId) && request.ServiceId != serviceId)
            {
                return Blocked(result, "service_mismatch:" + request.ServiceId, "服务 ID 与请求种类不符；接口不匹配");
            }
            if (kind == D19TavernRequestKind.Recruit)
            {
                if (string.IsNullOrEmpty(request.CandidateId))
                {
                    return Blocked(result, "missing_candidate_id", "recruit 请求必须携带 candidate_id；接口不匹配");
                }
                if (candidate == null)
                {
                    return Blocked(result, "candidate_null", "candidate 对象为空；接口不匹配，未调用 D9");
                }
            }

            // 2) 权限/服务前置（L23）
            string suffix = kind == D19TavernRequestKind.Recruit ? "recruitment" : "tavern";
            D19AuthorityResult authority = ValidateAuthority(request, claims, suffix);
            if (!authority.Allowed)
            {
                return FromAuthority(result, authority);
            }

            // 3) 可用性前置（L05；recruitment 不在 L05 KnownServiceIds → 由 D9 CheckService 裁决）
            bool gateSkipped;
            D19ServiceAvailabilityResult gate = ResolveGate(authority, request, suffix, out gateSkipped);
            if (!gateSkipped && gate.Verdict != D19AvailabilityVerdict.Available)
            {
                return FromGate(result, gate);
            }
            if (gateSkipped)
            {
                result.Note = "availability: recruitment outside L05 KnownServiceIds; adjudicated by D9 CheckService";
            }

            // 4) 港口键 → D9/D3 键（D9 PortCatalog 使用裸键：port_X → X）
            string d9PortId = authority.CanonicalPortId.StartsWith("port_", StringComparison.Ordinal)
                ? authority.CanonicalPortId.Substring("port_".Length)
                : authority.CanonicalPortId;

            // 5) D9 原系统裁决（费用/关系/候选船员/状态修改；活 state 透传，无缓存副本）
            D9PreparationResult owner = kind == D19TavernRequestKind.Recruit
                ? preparation.Recruit(state, ship, d9PortId, crew, candidate)
                : preparation.VisitTavern(state, ship, d9PortId, crew);

            // 6) 投影 D9 结果 → D19 结果（原始码保留）
            return ProjectOwner(result, request, owner, kind);
        }

        // ---------- 前置裁决（L23 权限 / L05 可用性） ----------

        private static D19AuthorityResult ValidateAuthority(
            D19TavernRequest request,
            D19AuthorityClaims claims,
            string serviceSuffix)
        {
            // L23 以 L04 锚点目录解析服务归属，服务键必须是锚点键
            // （service_<port>_<suffix>）；先解析港口再构造锚点服务键。
            string serviceAnchorKey = string.Empty;
            D19AnchorResult portResolve = D19PortAnchorCatalog.Instance.ResolvePort(request.PortId);
            if (portResolve.Success)
            {
                string portToken = portResolve.CanonicalKey.StartsWith("port_", StringComparison.Ordinal)
                    ? portResolve.CanonicalKey.Substring("port_".Length)
                    : portResolve.CanonicalKey;
                serviceAnchorKey = "service_" + portToken + "_" + serviceSuffix;
            }
            return D19PortAuthorityValidator.Validate(
                new D19AuthorityContext(
                    D19PortCommandKind.StartService,
                    request.ActorId,
                    request.ShipId,
                    request.PortId,
                    string.IsNullOrEmpty(request.BerthId) ? null : request.BerthId,
                    serviceAnchorKey),
                claims);
        }

        private static D19ServiceAvailabilityResult ResolveGate(
            D19AuthorityResult authority,
            D19TavernRequest request,
            string serviceSuffix,
            out bool skipped)
        {
            skipped = false;
            if (request.AvailabilityGate != null)
            {
                return request.AvailabilityGate;
            }
            string serviceId = request.ServiceId;
            if (string.IsNullOrEmpty(serviceId))
            {
                serviceId = serviceSuffix == "recruitment"
                    ? RecruitmentServiceId
                    : D19PortLocalizationKeys.ServiceIdTavern;
            }
            if (serviceId == RecruitmentServiceId)
            {
                // 真实覆盖缺口：L05 KnownServiceIds 不含 recruitment（保守 Reject 会阻断全部招募）；
                // 可用性交由 D9 CheckService("recruitment") 原系统裁决，不伪造 L05 结论。
                skipped = true;
                return null;
            }
            return D19PortAvailabilitySystem.Adjudicate(
                authority.CanonicalPortId,
                serviceId,
                request.TimePeriod,
                request.FactionPermission,
                request.Weather,
                request.FacilityState,
                request.Staffed);
        }

        // ---------- 投影（原系统结果 → D19 结果） ----------

        private static D19TavernResult ProjectOwner(
            D19TavernResult result,
            D19TavernRequest request,
            D9PreparationResult owner,
            D19TavernRequestKind kind)
        {
            result.OriginalOwnerCode = owner == null ? "d9_result_null" : owner.Code;
            if (owner == null)
            {
                return Blocked(result, "d9_result_null", "D9 返回空结果；接口不匹配，明确阻断");
            }

            if (owner.Accepted)
            {
                result.Success = true;
                result.DuplicateSettlement = owner.Duplicate;
                result.ReasonCode = string.Empty;
                result.LocKey = D19PortLocalizationKeys.ReadModelCommandAccepted;
                result.RetryPolicy = D19FailureRetryPolicy.NoRetry;
                result.Cost = owner.Cost;
                result.CrewCount = owner.Quantity;
                result.MoneyAfter = owner.MoneyAfter;
                result.Note = MergeNote(result.Note,
                    owner.Duplicate ? "d9 duplicate_settlement passed through (idempotent)" : string.Empty);
                result.EventDraft = BuildOwnerEventDraft(request, kind, true, owner, result);
                return result;
            }

            // 原系统失败 → L17 投影（映射缺失 → 明确阻断）
            var projection = FindProjection(D9ToL17, owner.Code);
            if (projection == null)
            {
                return Blocked(result, "owner_code_unmapped:" + owner.Code,
                    "D9 失败码未登记投影；接口不匹配，明确阻断");
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
            result.MoneyAfter = owner.MoneyAfter;
            result.Note = MergeNote(result.Note, projection.Note);
            result.EventDraft = BuildOwnerEventDraft(request, kind, false, owner, result);
            return result;
        }

        private static D19TavernResult ProjectSocial(
            D19TavernResult result,
            D19TavernRequest request,
            D10TavernRequestResult owner)
        {
            result.OriginalOwnerCode = owner == null ? "d10_result_null" : owner.Code;
            if (owner == null)
            {
                return Blocked(result, "d10_result_null", "D10 返回空结果；接口不匹配，明确阻断");
            }

            if (owner.Accepted)
            {
                result.Success = true;
                result.DuplicateSettlement = owner.Duplicate;
                result.ReasonCode = string.Empty;
                result.LocKey = D19PortLocalizationKeys.ReadModelCommandAccepted;
                result.RetryPolicy = D19FailureRetryPolicy.NoRetry;
                result.Note = owner.Status == D10TavernRequestStatus.Skipped
                    ? "d10 skipped passed through"
                    : (owner.Duplicate ? "d10 duplicate_tavern_request passed through (idempotent)" : string.Empty);
                result.EventDraft = BuildSocialEventDraft(request, true, owner);
                return result;
            }

            // 原系统拒绝 → L17 投影（映射缺失 → 明确阻断）
            var projection = FindProjection(D10ToL17, owner.Code);
            if (projection == null)
            {
                return Blocked(result, "owner_code_unmapped:" + owner.Code,
                    "D10 失败码未登记投影；接口不匹配，明确阻断");
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
            result.Note = MergeNote(result.Note, projection.Note);
            result.EventDraft = BuildSocialEventDraft(request, false, owner);
            return result;
        }

        // ---------- 事件草案（L16 工厂；只构造不追加；去重由 EventLog 完成） ----------

        private static WorldEvent BuildOwnerEventDraft(
            D19TavernRequest request,
            D19TavernRequestKind kind,
            bool succeeded,
            D9PreparationResult owner,
            D19TavernResult result)
        {
            string resultCode = succeeded
                ? (kind == D19TavernRequestKind.Recruit ? "port.tavern.recruit_applied" : "port.tavern.visit_applied")
                : "port.tavern.rejected";
            var values = new Dictionary<string, string>
            {
                { "save_revision", result.SaveRevision.ToString() },
            };
            if (succeeded)
            {
                values["cost"] = result.Cost.ToString();
                values["crew_count"] = result.CrewCount.ToString();
                values["money_after"] = result.MoneyAfter.ToString();
            }
            else
            {
                values["owner_code"] = owner.Code;
                values["reason_code"] = result.ReasonCode;
            }
            if (kind == D19TavernRequestKind.Recruit)
            {
                values["candidate_id"] = request.CandidateId;
            }
            return D19PortEventFactory.CreateServiceResult(
                request.CommandId,
                request.WorldDay,
                result.SaveRevision,
                request.PortId,
                request.ShipId,
                request.ServiceId,
                resultCode,
                new List<string> { request.ActorId },
                new List<string>(),
                D19PortLocalizationKeys.PortEventServiceResult,
                null,
                values);
        }

        private static WorldEvent BuildSocialEventDraft(
            D19TavernRequest request,
            bool succeeded,
            D10TavernRequestResult owner)
        {
            string resultCode = succeeded ? "port.tavern.social_applied" : "port.tavern.rejected";
            var values = new Dictionary<string, string>
            {
                { "save_revision", request.SaveRevision.ToString() },
                { "d10_action", request.SocialRequest.Action.ToString() },
                { "d10_code", owner.Code },
            };
            if (!succeeded)
            {
                values["reason_code"] = owner.Code;
            }
            return D19PortEventFactory.CreateServiceResult(
                request.CommandId,
                request.WorldDay,
                request.SaveRevision,
                request.PortId,
                request.ShipId,
                request.ServiceId,
                resultCode,
                new List<string> { request.ActorId },
                new List<string>(),
                D19PortLocalizationKeys.PortEventServiceResult,
                null,
                values);
        }

        // ---------- 工具 ----------

        private static string MergeNote(string existing, string next)
        {
            if (string.IsNullOrEmpty(existing))
            {
                return next ?? string.Empty;
            }
            if (string.IsNullOrEmpty(next))
            {
                return existing;
            }
            return existing + " | " + next;
        }

        private static Projection FindProjection(Projection[] table, string ownerCode)
        {
            foreach (var projection in table)
            {
                if (projection.OwnerCode == ownerCode)
                {
                    return projection;
                }
            }
            return null;
        }

        private static D19TavernResult Blocked(D19TavernResult result, string reasonCode, string note)
        {
            result.Success = false;
            result.ReasonCode = reasonCode;
            result.LocKey = D19PortLocalizationKeys.MissingKey;
            result.RetryPolicy = D19FailureRetryPolicy.NoRetry;
            result.Note = note;
            return result;
        }

        private static D19TavernResult FromAuthority(D19TavernResult result, D19AuthorityResult authority)
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

        private static D19TavernResult FromGate(D19TavernResult result, D19ServiceAvailabilityResult gate)
        {
            result.Success = false;
            result.ReasonCode = gate.ReasonCode;
            result.LocKey = gate.LocKey;
            result.RetryPolicy = gate.RetryPolicy;
            result.Note = "service gate: " + gate.Detail;
            return result;
        }

        // ---------- 自检（确定性） ----------

        /// <summary>
        /// 自检：投影表每条原系统码 → L17 码可解析、L17 条目 key 合法且在 L18 注册表、
        /// 服务后缀映射与 L18 服务 ID 一致、事件摘要 key 合法。返回空列表表示健全。
        /// </summary>
        public static List<string> Validate()
        {
            List<string> problems = new List<string>();
            ValidateTable(problems, D9ToL17, "D9");
            ValidateTable(problems, D10ToL17, "D10");
            if (!D19PortLocalizationKeys.IsValidKey(D19PortLocalizationKeys.PortEventServiceResult))
            {
                problems.Add("L18 event summary key invalid: " + D19PortLocalizationKeys.PortEventServiceResult);
            }
            foreach (var pair in CatalogServiceSuffix)
            {
                if (string.IsNullOrEmpty(pair.Value))
                {
                    problems.Add("service without catalog suffix: " + pair.Key);
                }
                if (pair.Key == RecruitmentServiceId)
                {
                    // 真实覆盖缺口（L18 服务注册表未收录 recruitment）：不做 L18 key 断言，
                    // 由 L18 扩展或 L45 一致性核对补齐；L05 已知服务集同样不含它。
                    continue;
                }
                string key;
                if (!D19PortLocalizationKeys.TryGetServiceKey(pair.Key, out key))
                {
                    problems.Add("service without L18 key: " + pair.Key);
                }
            }
            problems.Sort(StringComparer.Ordinal);
            return problems;
        }

        private static void ValidateTable(List<string> problems, Projection[] table, string ownerName)
        {
            var seen = new HashSet<string>();
            foreach (var projection in table)
            {
                if (string.IsNullOrEmpty(projection.OwnerCode))
                {
                    problems.Add(ownerName + " projection with empty owner code");
                    continue;
                }
                if (!seen.Add(projection.OwnerCode))
                {
                    problems.Add("duplicate " + ownerName + " code projection: " + projection.OwnerCode);
                }
                D19FailureEntry entry;
                if (!D19PortFailureCatalog.TryResolve(projection.L17Code, out entry))
                {
                    problems.Add("l17 entry missing for " + ownerName + " " + projection.OwnerCode + " -> " + projection.L17Code);
                    continue;
                }
                if (!D19PortLocalizationKeys.IsValidKey(entry.LocKey))
                {
                    problems.Add("invalid loc key for " + projection.L17Code + ": " + entry.LocKey);
                }
            }
        }
    }
}
