// D20NpcBehaviorAuthorityGuard —— D20-L19 NPC 行为权限边界。
//
// 守卫把 D20 行为可以写入的投影、只能提出的 owner 请求和禁止直写的
// 外部域分开。它既能在运行时检查一条权限请求，也能对纯数据声明集合
// 做静态审计；不访问文件、反射、引擎对象或任何外部服务。

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D20
{
    /// <summary>NPC 行为权限声明所涉及的逻辑域。</summary>
    public enum D20NpcBehaviorAuthorityDomain
    {
        Unknown = 0,
        BehaviorProjection = 1,
        ScheduleInput = 2,
        InteractionReadModel = 3,
        PresentationReadModel = 4,
        Economy = 5,
        Relationship = 6,
        Inventory = 7,
        Quest = 8,
        CoreAgent = 9,
        Identity = 10,
        Health = 11,
        Port = 12,
    }

    /// <summary>权限声明的操作类型。</summary>
    public enum D20NpcBehaviorAuthorityOperation
    {
        Unknown = 0,
        Read = 1,
        WriteProjection = 2,
        SubmitOwnerRequest = 3,
        WriteFact = 4,
        EmitReadOnlyEvent = 5,
    }

    /// <summary>权限检查的稳定失败码。</summary>
    public enum D20NpcBehaviorAuthorityFailureCode
    {
        None = 0,
        InvalidRequest = 1,
        MissingNpcId = 2,
        UnknownDomain = 3,
        UnknownOperation = 4,
        InvalidOwnerSystem = 5,
        ProposalRequired = 6,
        ForbiddenExternalWrite = 7,
        InvalidProjectionOwner = 8,
        ReadModelWrite = 9,
        CoreAgentForbidden = 10,
        InvalidClaim = 11,
    }

    /// <summary>运行时权限请求的校验码。</summary>
    public enum D20NpcBehaviorAuthorityRequestValidationCode
    {
        None = 0,
        MissingRequest = 1,
        InvalidSchemaVersion = 2,
        MissingNpcId = 3,
        UnknownDomain = 4,
        UnknownOperation = 5,
        InvalidOwnerSystem = 6,
        InvalidMemberName = 7,
    }

    /// <summary>权限声明校验结果。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorAuthorityRequestValidationResult
    {
        /// <summary>请求是否通过结构校验。</summary>
        public bool Valid;

        /// <summary>稳定校验码。</summary>
        public D20NpcBehaviorAuthorityRequestValidationCode Code;

        /// <summary>机器可读原因。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>创建成功结果。</summary>
        public static D20NpcBehaviorAuthorityRequestValidationResult Success()
        {
            return new D20NpcBehaviorAuthorityRequestValidationResult
            {
                Valid = true,
                Code = D20NpcBehaviorAuthorityRequestValidationCode.None,
                ReasonCode = string.Empty,
            };
        }

        /// <summary>创建失败结果。</summary>
        public static D20NpcBehaviorAuthorityRequestValidationResult Failure(
            D20NpcBehaviorAuthorityRequestValidationCode code,
            string reasonCode)
        {
            return new D20NpcBehaviorAuthorityRequestValidationResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? string.Empty,
            };
        }

        /// <summary>返回防御性副本。</summary>
        public D20NpcBehaviorAuthorityRequestValidationResult Clone()
        {
            return new D20NpcBehaviorAuthorityRequestValidationResult
            {
                Valid = Valid,
                Code = Code,
                ReasonCode = ReasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>可序列化的 NPC 行为权限请求。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorAuthorityRequest
    {
        /// <summary>权限请求版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>权限请求版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>NPC 稳定 ID。</summary>
        public string NpcId = string.Empty;

        /// <summary>被访问的逻辑域。</summary>
        public D20NpcBehaviorAuthorityDomain Domain =
            D20NpcBehaviorAuthorityDomain.Unknown;

        /// <summary>请求执行的操作。</summary>
        public D20NpcBehaviorAuthorityOperation Operation =
            D20NpcBehaviorAuthorityOperation.Unknown;

        /// <summary>实际 owner 的稳定引用；直接写行为投影时必须精确匹配。</summary>
        public string OwnerSystem = string.Empty;

        /// <summary>可选的行为成员引用，仅用于审计定位。</summary>
        public string MemberName = string.Empty;

        /// <summary>是否只是向外部 owner 提出请求。</summary>
        public bool IsProposal;

        /// <summary>声明是否不改变事实。</summary>
        public bool IsReadOnly = true;

        /// <summary>创建权限请求；外部域只能以 proposal 形式提交。</summary>
        public static D20NpcBehaviorAuthorityRequest Create(
            string npcId,
            D20NpcBehaviorAuthorityDomain domain,
            D20NpcBehaviorAuthorityOperation operation,
            string ownerSystem = "",
            bool isProposal = false,
            string memberName = "")
        {
            return new D20NpcBehaviorAuthorityRequest
            {
                NpcId = npcId ?? string.Empty,
                Domain = domain,
                Operation = operation,
                OwnerSystem = ownerSystem ?? string.Empty,
                MemberName = memberName ?? string.Empty,
                IsProposal = isProposal,
                IsReadOnly = operation != D20NpcBehaviorAuthorityOperation.WriteProjection &&
                    operation != D20NpcBehaviorAuthorityOperation.WriteFact,
            };
        }

        /// <summary>返回深拷贝。</summary>
        public D20NpcBehaviorAuthorityRequest Clone()
        {
            return new D20NpcBehaviorAuthorityRequest
            {
                SchemaVersion = SchemaVersion,
                NpcId = NpcId ?? string.Empty,
                Domain = Domain,
                Operation = Operation,
                OwnerSystem = OwnerSystem ?? string.Empty,
                MemberName = MemberName ?? string.Empty,
                IsProposal = IsProposal,
                IsReadOnly = IsReadOnly,
            };
        }

        /// <summary>验证权限请求的版本、ID、域、操作和 owner 引用。</summary>
        public D20NpcBehaviorAuthorityRequestValidationResult Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                return D20NpcBehaviorAuthorityRequestValidationResult.Failure(
                    D20NpcBehaviorAuthorityRequestValidationCode.InvalidSchemaVersion,
                    "npc_behavior_authority_schema_version_mismatch");
            if (!D20NpcIdentityContracts.IsStableNpcId(NpcId))
                return D20NpcBehaviorAuthorityRequestValidationResult.Failure(
                    D20NpcBehaviorAuthorityRequestValidationCode.MissingNpcId,
                    "npc_behavior_authority_npc_id_missing");
            if (!D20NpcBehaviorAuthorityGuard.IsSupportedDomain(Domain))
                return D20NpcBehaviorAuthorityRequestValidationResult.Failure(
                    D20NpcBehaviorAuthorityRequestValidationCode.UnknownDomain,
                    "npc_behavior_authority_domain_unknown");
            if (!D20NpcBehaviorAuthorityGuard.IsSupportedOperation(Operation))
                return D20NpcBehaviorAuthorityRequestValidationResult.Failure(
                    D20NpcBehaviorAuthorityRequestValidationCode.UnknownOperation,
                    "npc_behavior_authority_operation_unknown");
            if (!D20NpcIdentityContracts.IsStableReference(OwnerSystem, true))
                return D20NpcBehaviorAuthorityRequestValidationResult.Failure(
                    D20NpcBehaviorAuthorityRequestValidationCode.InvalidOwnerSystem,
                    "npc_behavior_authority_owner_invalid");
            if (!D20NpcIdentityContracts.IsStableReference(MemberName, true))
                return D20NpcBehaviorAuthorityRequestValidationResult.Failure(
                    D20NpcBehaviorAuthorityRequestValidationCode.InvalidMemberName,
                    "npc_behavior_authority_member_invalid");

            return D20NpcBehaviorAuthorityRequestValidationResult.Success();
        }
    }

    /// <summary>一次权限检查的只读结果。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorAuthorityResult
    {
        /// <summary>请求是否允许。</summary>
        public bool Allowed;

        /// <summary>结果是否只读。</summary>
        public bool IsReadOnly = true;

        /// <summary>NPC 稳定 ID。</summary>
        public string NpcId = string.Empty;

        /// <summary>访问的逻辑域。</summary>
        public D20NpcBehaviorAuthorityDomain Domain;

        /// <summary>请求操作。</summary>
        public D20NpcBehaviorAuthorityOperation Operation;

        /// <summary>owner 引用。</summary>
        public string OwnerSystem = string.Empty;

        /// <summary>是否为 proposal。</summary>
        public bool IsProposal;

        /// <summary>稳定失败码。</summary>
        public D20NpcBehaviorAuthorityFailureCode FailureCode;

        /// <summary>稳定原因码。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D20NpcBehaviorAuthorityResult Clone()
        {
            return new D20NpcBehaviorAuthorityResult
            {
                Allowed = Allowed,
                IsReadOnly = IsReadOnly,
                NpcId = NpcId ?? string.Empty,
                Domain = Domain,
                Operation = Operation,
                OwnerSystem = OwnerSystem ?? string.Empty,
                IsProposal = IsProposal,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>供静态审计使用的纯数据权限声明。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorAuthorityClaim
    {
        /// <summary>声明所属类型的稳定文本。</summary>
        public string DeclaringType = string.Empty;

        /// <summary>声明所属成员的稳定文本。</summary>
        public string MemberName = string.Empty;

        /// <summary>声明访问的逻辑域。</summary>
        public D20NpcBehaviorAuthorityDomain Domain;

        /// <summary>声明执行的操作。</summary>
        public D20NpcBehaviorAuthorityOperation Operation;

        /// <summary>声明的 owner 引用。</summary>
        public string OwnerSystem = string.Empty;

        /// <summary>声明是否只是 proposal。</summary>
        public bool IsProposal;

        /// <summary>声明是否只读。</summary>
        public bool IsReadOnly = true;

        /// <summary>返回防御性副本。</summary>
        public D20NpcBehaviorAuthorityClaim Clone()
        {
            return new D20NpcBehaviorAuthorityClaim
            {
                DeclaringType = DeclaringType ?? string.Empty,
                MemberName = MemberName ?? string.Empty,
                Domain = Domain,
                Operation = Operation,
                OwnerSystem = OwnerSystem ?? string.Empty,
                IsProposal = IsProposal,
                IsReadOnly = IsReadOnly,
            };
        }
    }

    /// <summary>批量权限静态审计结果。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorAuthorityAuditResult
    {
        /// <summary>是否没有越权声明。</summary>
        public bool Passed;

        /// <summary>按声明顺序记录稳定违规码。</summary>
        public List<string> Violations = new List<string>();

        /// <summary>违规数量。</summary>
        public int ViolationCount
        {
            get { return Violations == null ? 0 : Violations.Count; }
        }

        /// <summary>返回深拷贝。</summary>
        public D20NpcBehaviorAuthorityAuditResult Clone()
        {
            var result = new D20NpcBehaviorAuthorityAuditResult
            {
                Passed = Passed,
                Violations = new List<string>(),
            };
            if (Violations != null) result.Violations.AddRange(Violations);
            return result;
        }
    }

    /// <summary>
    /// D20-L19 NPC 行为权限守卫。唯一允许的直接写入是 D20 行为投影；
    /// 经济、关系、库存、任务和 Agent 核心库等外部域只能提交 proposal，
    /// 最终事实必须回到其 owner。
    /// </summary>
    public static class D20NpcBehaviorAuthorityGuard
    {
        /// <summary>权限守卫 owner 标识。</summary>
        public const string OwnerSystem = "D20NpcBehaviorAuthorityGuard";

        /// <summary>行为投影唯一直接写者。</summary>
        public const string BehaviorProjectionOwner = "D20NpcBehaviorStateMachine";

        /// <summary>判断逻辑域是否受当前版本支持。</summary>
        public static bool IsSupportedDomain(
            D20NpcBehaviorAuthorityDomain domain)
        {
            return domain >= D20NpcBehaviorAuthorityDomain.BehaviorProjection &&
                   domain <= D20NpcBehaviorAuthorityDomain.Port;
        }

        /// <summary>判断操作是否受当前版本支持。</summary>
        public static bool IsSupportedOperation(
            D20NpcBehaviorAuthorityOperation operation)
        {
            return operation >= D20NpcBehaviorAuthorityOperation.Read &&
                   operation <= D20NpcBehaviorAuthorityOperation.EmitReadOnlyEvent;
        }

        /// <summary>判断域是否是不得直接写入的既有 owner 域。</summary>
        public static bool IsForbiddenExternalWriteDomain(
            D20NpcBehaviorAuthorityDomain domain)
        {
            return domain == D20NpcBehaviorAuthorityDomain.Economy ||
                   domain == D20NpcBehaviorAuthorityDomain.Relationship ||
                   domain == D20NpcBehaviorAuthorityDomain.Inventory ||
                   domain == D20NpcBehaviorAuthorityDomain.Quest ||
                   domain == D20NpcBehaviorAuthorityDomain.CoreAgent;
        }

        /// <summary>检查单条运行时权限请求。</summary>
        public static D20NpcBehaviorAuthorityResult Evaluate(
            D20NpcBehaviorAuthorityRequest request)
        {
            D20NpcBehaviorAuthorityResult result = CreateBaseResult(request);
            if (request == null)
                return Reject(result, D20NpcBehaviorAuthorityFailureCode.InvalidRequest,
                    "npc_behavior_authority_request_missing");

            D20NpcBehaviorAuthorityRequestValidationResult validation = request.Validate();
            if (!validation.Valid)
                return Reject(result, D20NpcBehaviorAuthorityFailureCode.InvalidRequest,
                    validation.ReasonCode);

            result.NpcId = request.NpcId;
            result.Domain = request.Domain;
            result.Operation = request.Operation;
            result.OwnerSystem = request.OwnerSystem ?? string.Empty;
            result.IsProposal = request.IsProposal;

            if (request.Operation == D20NpcBehaviorAuthorityOperation.Read)
            {
                if (!request.IsReadOnly)
                    return Reject(result, D20NpcBehaviorAuthorityFailureCode.InvalidClaim,
                        "npc_behavior_authority_read_not_read_only");
                return Allow(result, "npc_behavior_authority_read_allowed", true);
            }

            if (request.Operation == D20NpcBehaviorAuthorityOperation.EmitReadOnlyEvent)
            {
                if (!request.IsReadOnly ||
                    (request.Domain != D20NpcBehaviorAuthorityDomain.BehaviorProjection &&
                     request.Domain != D20NpcBehaviorAuthorityDomain.InteractionReadModel &&
                     request.Domain != D20NpcBehaviorAuthorityDomain.PresentationReadModel))
                    return Reject(result, D20NpcBehaviorAuthorityFailureCode.ReadModelWrite,
                        "npc_behavior_authority_event_not_read_only");
                return Allow(result, "npc_behavior_authority_read_event_allowed", true);
            }

            if (request.Operation == D20NpcBehaviorAuthorityOperation.SubmitOwnerRequest)
            {
                if (!request.IsProposal)
                    return Reject(result, D20NpcBehaviorAuthorityFailureCode.ProposalRequired,
                        "npc_behavior_authority_owner_request_requires_proposal");
                if (request.Domain == D20NpcBehaviorAuthorityDomain.CoreAgent)
                    return Reject(result, D20NpcBehaviorAuthorityFailureCode.CoreAgentForbidden,
                        "npc_behavior_authority_core_agent_forbidden");
                if (request.Domain == D20NpcBehaviorAuthorityDomain.BehaviorProjection ||
                    request.Domain == D20NpcBehaviorAuthorityDomain.ScheduleInput ||
                    request.Domain == D20NpcBehaviorAuthorityDomain.InteractionReadModel ||
                    request.Domain == D20NpcBehaviorAuthorityDomain.PresentationReadModel)
                    return Reject(result, D20NpcBehaviorAuthorityFailureCode.InvalidClaim,
                        "npc_behavior_authority_internal_request_invalid");
                if (string.IsNullOrEmpty(request.OwnerSystem))
                    return Reject(result, D20NpcBehaviorAuthorityFailureCode.InvalidOwnerSystem,
                        "npc_behavior_authority_owner_missing");
                return Allow(result, "npc_behavior_authority_owner_request_allowed", true);
            }

            if (request.Operation == D20NpcBehaviorAuthorityOperation.WriteProjection)
            {
                if (request.IsReadOnly)
                    return Reject(result, D20NpcBehaviorAuthorityFailureCode.InvalidClaim,
                        "npc_behavior_authority_projection_write_marked_read_only");
                if (request.Domain != D20NpcBehaviorAuthorityDomain.BehaviorProjection)
                    return Reject(result, D20NpcBehaviorAuthorityFailureCode.ForbiddenExternalWrite,
                        "npc_behavior_authority_external_write_forbidden");
                if (!string.Equals(request.OwnerSystem, BehaviorProjectionOwner,
                        StringComparison.Ordinal))
                    return Reject(result, D20NpcBehaviorAuthorityFailureCode.InvalidProjectionOwner,
                        "npc_behavior_authority_projection_owner_invalid");
                return Allow(result, "npc_behavior_authority_projection_write_allowed", false);
            }

            if (request.Operation == D20NpcBehaviorAuthorityOperation.WriteFact)
            {
                if (request.Domain == D20NpcBehaviorAuthorityDomain.CoreAgent)
                    return Reject(result, D20NpcBehaviorAuthorityFailureCode.CoreAgentForbidden,
                        "npc_behavior_authority_core_agent_forbidden");
                return Reject(result, D20NpcBehaviorAuthorityFailureCode.ForbiddenExternalWrite,
                    "npc_behavior_authority_fact_write_forbidden");
            }

            return Reject(result, D20NpcBehaviorAuthorityFailureCode.InvalidRequest,
                "npc_behavior_authority_operation_unhandled");
        }

        /// <summary>判断某域是否能被行为层直接写入。</summary>
        public static bool CanWrite(
            D20NpcBehaviorAuthorityDomain domain,
            string ownerSystem)
        {
            return Evaluate(D20NpcBehaviorAuthorityRequest.Create(
                "npc_authority_probe",
                domain,
                D20NpcBehaviorAuthorityOperation.WriteProjection,
                ownerSystem)).Allowed;
        }

        /// <summary>判断某域是否可以被行为层以 proposal 形式请求 owner。</summary>
        public static bool CanSubmitOwnerRequest(
            D20NpcBehaviorAuthorityDomain domain,
            string ownerSystem)
        {
            return Evaluate(D20NpcBehaviorAuthorityRequest.Create(
                "npc_authority_probe",
                domain,
                D20NpcBehaviorAuthorityOperation.SubmitOwnerRequest,
                ownerSystem,
                true)).Allowed;
        }

        /// <summary>
        /// 对行为模块公开的权限声明进行静态式审计。该方法只检查声明数据，
        /// 不执行声明，也不因通过审计而替代真正 owner 的业务裁决。
        /// </summary>
        public static D20NpcBehaviorAuthorityAuditResult Audit(
            IEnumerable<D20NpcBehaviorAuthorityClaim> claims)
        {
            var result = new D20NpcBehaviorAuthorityAuditResult
            {
                Passed = true,
            };
            if (claims == null) return result;

            int index = 0;
            foreach (D20NpcBehaviorAuthorityClaim claim in claims)
            {
                if (claim == null)
                {
                    result.Passed = false;
                    result.Violations.Add(index + ":npc_behavior_authority_claim_missing");
                    index++;
                    continue;
                }

                if (!D20NpcIdentityContracts.IsStableReference(claim.DeclaringType, true) ||
                    !D20NpcIdentityContracts.IsStableReference(claim.MemberName, true))
                {
                    result.Passed = false;
                    result.Violations.Add(index + ":npc_behavior_authority_claim_reference_invalid");
                    index++;
                    continue;
                }

                D20NpcBehaviorAuthorityRequest request =
                    D20NpcBehaviorAuthorityRequest.Create(
                        "npc_authority_audit_npc",
                        claim.Domain,
                        claim.Operation,
                        claim.OwnerSystem,
                        claim.IsProposal,
                        claim.MemberName);
                request.IsReadOnly = claim.IsReadOnly;
                D20NpcBehaviorAuthorityResult check = Evaluate(request);
                if (!check.Allowed)
                {
                    result.Passed = false;
                    result.Violations.Add(index + ":" + check.ReasonCode);
                }
                index++;
            }

            return result;
        }

        private static D20NpcBehaviorAuthorityResult CreateBaseResult(
            D20NpcBehaviorAuthorityRequest request)
        {
            return new D20NpcBehaviorAuthorityResult
            {
                Allowed = false,
                IsReadOnly = true,
                NpcId = request == null ? string.Empty : request.NpcId ?? string.Empty,
                Domain = request == null
                    ? D20NpcBehaviorAuthorityDomain.Unknown : request.Domain,
                Operation = request == null
                    ? D20NpcBehaviorAuthorityOperation.Unknown : request.Operation,
                OwnerSystem = request == null ? string.Empty : request.OwnerSystem ?? string.Empty,
                IsProposal = request != null && request.IsProposal,
                FailureCode = D20NpcBehaviorAuthorityFailureCode.InvalidRequest,
                ReasonCode = string.Empty,
            };
        }

        private static D20NpcBehaviorAuthorityResult Allow(
            D20NpcBehaviorAuthorityResult result,
            string reasonCode,
            bool isReadOnly)
        {
            result.Allowed = true;
            result.IsReadOnly = isReadOnly;
            result.FailureCode = D20NpcBehaviorAuthorityFailureCode.None;
            result.ReasonCode = reasonCode ?? string.Empty;
            return result;
        }

        private static D20NpcBehaviorAuthorityResult Reject(
            D20NpcBehaviorAuthorityResult result,
            D20NpcBehaviorAuthorityFailureCode failureCode,
            string reasonCode)
        {
            result.Allowed = false;
            result.IsReadOnly = true;
            result.FailureCode = failureCode;
            result.ReasonCode = reasonCode ?? string.Empty;
            return result;
        }
    }
}
