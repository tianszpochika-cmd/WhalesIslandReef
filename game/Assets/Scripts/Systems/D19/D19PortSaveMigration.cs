// ============================================================
// D19PortSaveMigration —— D19 港口旧存档迁移与状态兼容处理（纯 C#，D19-L21）
//
// 归属：Whalebone.Systems.D19（D19 章节唯一存档迁移文件，D19-L21）
// 目的：实现旧存档缺少 D19 字段、旧 key、旧版本和不可能状态时的安全迁移：
//       1. 合法缺字段：映射到保守确定的安全默认值（CreateDefault 语义）；
//       2. 锚点键/别名迁移：旧 D3/D9 键（如 santa_margarita/building_saint_marys_dock）
//          经 D19-L04 锚点目录确定性迁移为规范键并保留 MigrationHint；
//       3. 不可能/冲突/数据损失状态：返回 D19-L17 的不可逆错误/人工恢复提示，
//          绝不伪造泊位、成员位置、费用或完成命令；
//       4. 幂等与版本单调：相同输入迁移结果必然相同，已是当前版本 (SchemaVersion=1)
//          且状态合法的投影不进行二次或重复变异；
//       5. 铁律约束：迁移绝不提升权限、绝不自动开服务、绝不产生业务副作用；
//          无 UnityEngine/Transform/UI 依赖，失败可被 L39 测试精确定位。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 港口存档迁移状态枚举（D19-L21，对应 SaveMigrationStatus 语义）。
    /// </summary>
    public enum D19PortMigrationStatus
    {
        /// <summary>占位：未定义。</summary>
        None = 0,

        /// <summary>迁移成功：已补充缺省字段/规范化旧键，生成合规的 SchemaVersion=1 投影。</summary>
        Migrated,

        /// <summary>已是最新：输入已经是完整且合法的最新版本，未做任何修改。</summary>
        AlreadyUpToDate,

        /// <summary>无法迁移：存在不可能状态或歧义，已安全拒绝并给出不可逆/人工恢复提示。</summary>
        Failed,

        /// <summary>不支持的版本：SchemaVersion 高于当前支持版本或非法。</summary>
        UnsupportedVersion
    }

    /// <summary>
    /// 港口存档迁移结果载体（D19-L21，纯数据对象）。
    /// </summary>
    public sealed class D19PortMigrationResult
    {
        /// <summary>迁移状态。</summary>
        public D19PortMigrationStatus Status;

        /// <summary>源 Schema 版本。</summary>
        public int SourceSchemaVersion;

        /// <summary>目标 Schema 版本（成功时为 CurrentSchemaVersion）。</summary>
        public int TargetSchemaVersion;

        /// <summary>迁移后的投影对象（失败时为 null）。</summary>
        public D19PortSaveProjection MigratedProjection;

        /// <summary>L17 稳定业务失败码（失败时非空）。</summary>
        public string ErrorCode = string.Empty;

        /// <summary>L17/L18 本地化键（失败或提示时非空）。</summary>
        public string LocKey = string.Empty;

        /// <summary>精确技术原因/不可逆人工恢复提示（小写蛇形或审计文本，不含 UI 显示文案）。</summary>
        public string TechnicalReason = string.Empty;

        /// <summary>迁移标记与警告列表（记录修复的缺省字段、别名转换提示等）。</summary>
        public List<string> MigrationFlags = new List<string>();

        /// <summary>原始数据是否完好保留（恒为 true，绝不破坏输入）。</summary>
        public bool OriginalPreserved = true;

        /// <summary>是否成功迁移或已是最新。</summary>
        public bool Succeeded
        {
            get { return Status == D19PortMigrationStatus.Migrated || Status == D19PortMigrationStatus.AlreadyUpToDate; }
        }

        public static D19PortMigrationResult Success(
            D19PortSaveProjection projection,
            int sourceVersion,
            List<string> flags,
            bool wasAlreadyUpToDate)
        {
            return new D19PortMigrationResult
            {
                Status = wasAlreadyUpToDate ? D19PortMigrationStatus.AlreadyUpToDate : D19PortMigrationStatus.Migrated,
                SourceSchemaVersion = sourceVersion,
                TargetSchemaVersion = D19PortSaveProjection.CurrentSchemaVersion,
                MigratedProjection = projection,
                MigrationFlags = flags ?? new List<string>(),
                OriginalPreserved = true
            };
        }

        public static D19PortMigrationResult Fail(
            D19PortMigrationStatus status,
            int sourceVersion,
            string errorCode,
            string locKey,
            string technicalReason,
            List<string> flags = null)
        {
            return new D19PortMigrationResult
            {
                Status = status,
                SourceSchemaVersion = sourceVersion,
                TargetSchemaVersion = D19PortSaveProjection.CurrentSchemaVersion,
                MigratedProjection = null,
                ErrorCode = errorCode ?? string.Empty,
                LocKey = locKey ?? string.Empty,
                TechnicalReason = technicalReason ?? string.Empty,
                MigrationFlags = flags ?? new List<string>(),
                OriginalPreserved = true
            };
        }
    }

    /// <summary>
    /// D19 港口旧存档迁移器（纯函数/纯逻辑，零全局静态状态）。
    /// </summary>
    public static class D19PortSaveMigration
    {
        public const int SupportedSchemaVersion = D19PortSaveProjection.CurrentSchemaVersion; // 1

        /// <summary>
        /// 执行港口保存投影的迁移与合法性修复。
        /// 若输入为 null，则返回带有保守默认值的 Migrated 结果。
        /// </summary>
        public static D19PortMigrationResult Migrate(D19PortSaveProjection source, string defaultShipId = "")
        {
            var flags = new List<string>();

            // 1. 完全缺失输入：安全创建保守默认投影
            if (source == null)
            {
                flags.Add("source_null_created_default");
                var defaultProj = D19PortSaveProjection.CreateDefault(defaultShipId);
                return D19PortMigrationResult.Success(defaultProj, 0, flags, wasAlreadyUpToDate: false);
            }

            int sourceVersion = source.SchemaVersion;

            // 2. 版本检查：版本大于当前支持版本 -> 不支持的旧版本/未来版本
            if (sourceVersion > SupportedSchemaVersion)
            {
                return D19PortMigrationResult.Fail(
                    D19PortMigrationStatus.UnsupportedVersion,
                    sourceVersion,
                    "d19.save.unsupported_version",
                    D19PortLocalizationKeys.MigrationSaveUnsupported,
                    "future_schema_version_not_supported");
            }

            if (sourceVersion < 0)
            {
                return D19PortMigrationResult.Fail(
                    D19PortMigrationStatus.Failed,
                    sourceVersion,
                    "d19.save.unsupported_version",
                    D19PortLocalizationKeys.MigrationSaveUnsupported,
                    "negative_schema_version_invalid");
            }

            bool modified = false;

            // 如果版本从 0 升级到 1
            if (sourceVersion == 0)
            {
                flags.Add("upgraded_schema_version_0_to_1");
                modified = true;
            }

            // 克隆一份进行安全迁移与校验，确保原始对象不被污染
            D19PortSaveProjection target = source.Clone();
            target.SchemaVersion = SupportedSchemaVersion;

            // 3. 基础标量与时钟校验
            if (target.WorldDay < 0)
            {
                return D19PortMigrationResult.Fail(
                    D19PortMigrationStatus.Failed,
                    sourceVersion,
                    "d19.save.conflict",
                    D19PortLocalizationKeys.MigrationSaveConflict,
                    "negative_world_day_impossible_state");
            }

            if (target.SaveRevision < 0)
            {
                return D19PortMigrationResult.Fail(
                    D19PortMigrationStatus.Failed,
                    sourceVersion,
                    "d19.save.conflict",
                    D19PortLocalizationKeys.MigrationSaveConflict,
                    "negative_save_revision_impossible_state");
            }

            if (string.IsNullOrWhiteSpace(target.ShipId))
            {
                if (!string.IsNullOrWhiteSpace(defaultShipId))
                {
                    target.ShipId = defaultShipId.Trim();
                    flags.Add("ship_id_filled_from_default");
                    modified = true;
                }
                else
                {
                    target.ShipId = string.Empty;
                }
            }

            // 4. 港口键别名与废弃键迁移 (通过 L04 锚点目录)
            if (!string.IsNullOrWhiteSpace(target.PortId))
            {
                var anchorCatalog = D19PortAnchorCatalog.Instance;
                var portResolve = anchorCatalog.ResolvePort(target.PortId);
                if (!portResolve.Success)
                {
                    return D19PortMigrationResult.Fail(
                        D19PortMigrationStatus.Failed,
                        sourceVersion,
                        "d19.port.unknown_port",
                        D19PortLocalizationKeys.FailureUnknownPort,
                        "unknown_port_key:" + target.PortId);
                }

                if (!string.Equals(target.PortId, portResolve.CanonicalKey, StringComparison.Ordinal))
                {
                    flags.Add("migrated_port_alias:" + target.PortId + "->" + portResolve.CanonicalKey);
                    if (portResolve.IsDeprecated && !string.IsNullOrEmpty(portResolve.MigrationHint))
                    {
                        flags.Add("port_migration_hint:" + portResolve.MigrationHint);
                    }
                    target.PortId = portResolve.CanonicalKey;
                    modified = true;
                }
            }

            // 5. 访问生命周期状态与泊位组合一致性校验
            D19PortVisitState visitState;
            if (string.IsNullOrWhiteSpace(target.VisitState))
            {
                visitState = D19PortVisitState.None;
                target.VisitState = D19PortVisitState.None.ToString();
                flags.Add("empty_visit_state_defaulted_to_none");
                modified = true;
            }
            else if (!Enum.TryParse(target.VisitState, true, out visitState))
            {
                return D19PortMigrationResult.Fail(
                    D19PortMigrationStatus.Failed,
                    sourceVersion,
                    "d19.lifecycle.invalid_transition",
                    D19PortLocalizationKeys.FailureInvalidTransition,
                    "unrecognized_visit_state:" + target.VisitState);
            }
            else
            {
                target.VisitState = visitState.ToString();
            }

            // 不可能状态检查：不在港口 (None / Departed) 却持有 PortId 或 BerthId
            if (visitState == D19PortVisitState.None || visitState == D19PortVisitState.Departed)
            {
                if (!string.IsNullOrWhiteSpace(target.BerthId))
                {
                    return D19PortMigrationResult.Fail(
                        D19PortMigrationStatus.Failed,
                        sourceVersion,
                        "d19.lifecycle.invalid_transition",
                        D19PortLocalizationKeys.FailureInvalidTransition,
                        "berth_assigned_in_non_port_state:" + visitState);
                }
            }

            // 必须持泊位状态 (Berthed / InService) 缺 BerthId -> 数据损失，不可伪造泊位
            if (visitState == D19PortVisitState.Berthed || visitState == D19PortVisitState.InService)
            {
                if (string.IsNullOrWhiteSpace(target.BerthId))
                {
                    return D19PortMigrationResult.Fail(
                        D19PortMigrationStatus.Failed,
                        sourceVersion,
                        "d19.berth.occupied",
                        D19PortLocalizationKeys.FailureNotBerthed,
                        "missing_berth_in_berthed_state_data_loss");
                }

                if (string.IsNullOrWhiteSpace(target.PortId))
                {
                    return D19PortMigrationResult.Fail(
                        D19PortMigrationStatus.Failed,
                        sourceVersion,
                        "d19.port.unknown_port",
                        D19PortLocalizationKeys.FailureNotInPort,
                        "missing_port_in_berthed_state");
                }
            }

            // 校验已存在的 BerthId 归属与别名
            if (!string.IsNullOrWhiteSpace(target.BerthId))
            {
                var anchorCatalog = D19PortAnchorCatalog.Instance;
                var berthResolve = anchorCatalog.ResolveBerth(target.BerthId);
                if (!berthResolve.Success)
                {
                    return D19PortMigrationResult.Fail(
                        D19PortMigrationStatus.Failed,
                        sourceVersion,
                        "d19.port.unknown_port",
                        D19PortLocalizationKeys.FailureUnknownBerth,
                        "unknown_berth_key:" + target.BerthId);
                }

                if (!string.Equals(target.BerthId, berthResolve.CanonicalKey, StringComparison.Ordinal))
                {
                    flags.Add("migrated_berth_alias:" + target.BerthId + "->" + berthResolve.CanonicalKey);
                    if (berthResolve.IsDeprecated && !string.IsNullOrEmpty(berthResolve.MigrationHint))
                    {
                        flags.Add("berth_migration_hint:" + berthResolve.MigrationHint);
                    }
                    target.BerthId = berthResolve.CanonicalKey;
                    modified = true;
                }

                // 跨港泊位检测
                if (!string.IsNullOrWhiteSpace(target.PortId) &&
                    !string.Equals(berthResolve.Port, target.PortId, StringComparison.Ordinal))
                {
                    return D19PortMigrationResult.Fail(
                        D19PortMigrationStatus.Failed,
                        sourceVersion,
                        "d19.port.permission_denied",
                        D19PortLocalizationKeys.FailureInvalidTransition,
                        "cross_port_berth_conflict:berth_port=" + berthResolve.Port + ",current_port=" + target.PortId);
                }
            }

            // 6. 泊位分配列表清理与别名规范化
            if (target.BerthAllocations == null)
            {
                target.BerthAllocations = new List<D19BerthAllocationSaveEntry>();
                flags.Add("initialized_empty_berth_allocations");
                modified = true;
            }
            else
            {
                var anchorCatalog = D19PortAnchorCatalog.Instance;
                var seenBerths = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < target.BerthAllocations.Count; i++)
                {
                    var alloc = target.BerthAllocations[i];
                    if (alloc == null || string.IsNullOrWhiteSpace(alloc.BerthId))
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.berth.occupied",
                            D19PortLocalizationKeys.FailureInvariantViolation,
                            "null_or_empty_berth_id_in_allocations");
                    }

                    var bRes = anchorCatalog.ResolveBerth(alloc.BerthId);
                    if (!bRes.Success)
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.port.unknown_port",
                            D19PortLocalizationKeys.FailureUnknownBerth,
                            "unknown_berth_in_allocation:" + alloc.BerthId);
                    }

                    if (!string.Equals(alloc.BerthId, bRes.CanonicalKey, StringComparison.Ordinal))
                    {
                        flags.Add("migrated_alloc_berth_alias:" + alloc.BerthId + "->" + bRes.CanonicalKey);
                        alloc.BerthId = bRes.CanonicalKey;
                        modified = true;
                    }

                    if (!seenBerths.Add(alloc.BerthId))
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.berth.occupied",
                            D19PortLocalizationKeys.FailureInvariantViolation,
                            "duplicate_berth_id_in_allocations:" + alloc.BerthId);
                    }

                    D19BerthState bState;
                    if (string.IsNullOrWhiteSpace(alloc.State) || !Enum.TryParse(alloc.State, true, out bState))
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.berth.occupied",
                            D19PortLocalizationKeys.FailureInvariantViolation,
                            "invalid_berth_state_in_allocation:" + alloc.State);
                    }
                    alloc.State = bState.ToString();

                    // 不可能状态：Occupied 或 Reserved 却缺少占用者船只标识
                    if ((bState == D19BerthState.Occupied || bState == D19BerthState.Reserved) &&
                        string.IsNullOrWhiteSpace(alloc.ShipId))
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.berth.occupied",
                            D19PortLocalizationKeys.FailureInvariantViolation,
                            "occupied_or_reserved_berth_missing_ship_id:" + alloc.BerthId);
                    }

                    // 预留必须有租期
                    if (bState == D19BerthState.Reserved && alloc.ExpiresWorldDay <= alloc.ReservedWorldDay)
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.berth.expired",
                            D19PortLocalizationKeys.FailureInvariantViolation,
                            "reserved_berth_invalid_expiry:" + alloc.BerthId);
                    }
                }
            }

            // 7. 成员逻辑位置列表清理与去重校验
            if (target.MemberLocations == null)
            {
                target.MemberLocations = new List<D19MemberLocationSaveEntry>();
                flags.Add("initialized_empty_member_locations");
                modified = true;
            }
            else
            {
                var seenMembers = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < target.MemberLocations.Count; i++)
                {
                    var m = target.MemberLocations[i];
                    if (m == null || string.IsNullOrWhiteSpace(m.MemberId))
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.boarding.failed",
                            D19PortLocalizationKeys.FailureInvariantViolation,
                            "null_or_empty_member_id_in_locations");
                    }

                    if (!seenMembers.Add(m.MemberId))
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.boarding.failed",
                            D19PortLocalizationKeys.FailureInvariantViolation,
                            "duplicate_member_id_in_locations:" + m.MemberId);
                    }

                    D19CrewLocation loc;
                    if (string.IsNullOrWhiteSpace(m.Location) || !Enum.TryParse(m.Location, true, out loc) || loc == D19CrewLocation.Unknown)
                    {
                        // 缺省位置保守默认安全值：OnShore（绝不自动置为 OnBoard 伪造在船）
                        m.Location = D19CrewLocation.OnShore.ToString();
                        flags.Add("member_location_defaulted_to_on_shore:" + m.MemberId);
                        modified = true;
                    }
                    else
                    {
                        m.Location = loc.ToString();
                    }
                }
            }

            // 8. 港口服务状态列表检查（绝不自动开启新服务）
            if (target.ServiceStates == null)
            {
                target.ServiceStates = new List<D19PortServiceSaveEntry>();
                flags.Add("initialized_empty_service_states");
                modified = true;
            }
            else
            {
                var anchorCatalog = D19PortAnchorCatalog.Instance;
                for (int i = 0; i < target.ServiceStates.Count; i++)
                {
                    var s = target.ServiceStates[i];
                    if (s == null || string.IsNullOrWhiteSpace(s.ServiceId))
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.service.unavailable",
                            D19PortLocalizationKeys.FailureInvariantViolation,
                            "null_or_empty_service_id_in_states");
                    }

                    var sRes = anchorCatalog.ResolveService(s.ServiceId);
                    if (sRes.Success && !string.Equals(s.ServiceId, sRes.CanonicalKey, StringComparison.Ordinal))
                    {
                        flags.Add("migrated_service_alias:" + s.ServiceId + "->" + sRes.CanonicalKey);
                        s.ServiceId = sRes.CanonicalKey;
                        modified = true;
                    }
                }
            }

            // 9. 未完成命令与补偿草案列表合法性
            if (target.PendingCommands == null)
            {
                target.PendingCommands = new List<D19PendingCommandSaveEntry>();
                flags.Add("initialized_empty_pending_commands");
                modified = true;
            }
            else
            {
                for (int i = 0; i < target.PendingCommands.Count; i++)
                {
                    var p = target.PendingCommands[i];
                    if (p == null || string.IsNullOrWhiteSpace(p.CommandId))
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.save.conflict",
                            D19PortLocalizationKeys.FailureMissingCommandId,
                            "invalid_pending_command_missing_id");
                    }
                    if (p.Payload == null)
                    {
                        p.Payload = new Dictionary<string, string>(StringComparer.Ordinal);
                        modified = true;
                    }
                }
            }

            if (target.CompensationDrafts == null)
            {
                target.CompensationDrafts = new List<D19CompensationDraftSaveEntry>();
                flags.Add("initialized_empty_compensation_drafts");
                modified = true;
            }
            else
            {
                for (int i = 0; i < target.CompensationDrafts.Count; i++)
                {
                    var c = target.CompensationDrafts[i];
                    if (c == null || string.IsNullOrWhiteSpace(c.DraftId) || string.IsNullOrWhiteSpace(c.OriginalCommandId))
                    {
                        return D19PortMigrationResult.Fail(
                            D19PortMigrationStatus.Failed,
                            sourceVersion,
                            "d19.save.conflict",
                            D19PortLocalizationKeys.FailureInvariantViolation,
                            "invalid_compensation_draft_missing_id");
                    }
                    if (c.Parameters == null)
                    {
                        c.Parameters = new Dictionary<string, string>(StringComparer.Ordinal);
                        modified = true;
                    }
                }
            }

            // 10. 命令幂等记录投影检查与初始化
            if (target.Idempotency == null)
            {
                target.Idempotency = new D19IdempotencySaveProjection
                {
                    ShipId = target.ShipId,
                    RetentionWorldDays = 30,
                    MaxRetainedEntries = 512,
                    Entries = new List<D19IdempotencySaveEntry>()
                };
                flags.Add("initialized_default_idempotency_projection");
                modified = true;
            }
            else
            {
                if (target.Idempotency.Entries == null)
                {
                    target.Idempotency.Entries = new List<D19IdempotencySaveEntry>();
                    modified = true;
                }
                else
                {
                    for (int i = 0; i < target.Idempotency.Entries.Count; i++)
                    {
                        var entry = target.Idempotency.Entries[i];
                        if (entry == null || string.IsNullOrWhiteSpace(entry.CommandId))
                        {
                            return D19PortMigrationResult.Fail(
                                D19PortMigrationStatus.Failed,
                                sourceVersion,
                                "d19.save.conflict",
                                D19PortLocalizationKeys.FailureMissingCommandId,
                                "invalid_idempotency_entry_missing_command_id");
                        }
                    }
                }
            }

            // 11. 最终确定性验证 (通过 Validate 校验)
            try
            {
                target.Validate();
            }
            catch (Exception ex)
            {
                return D19PortMigrationResult.Fail(
                    D19PortMigrationStatus.Failed,
                    sourceVersion,
                    "d19.save.conflict",
                    D19PortLocalizationKeys.FailureInvariantViolation,
                    "validation_failed:" + ex.Message);
            }

            bool wasAlreadyUpToDate = !modified && (sourceVersion == SupportedSchemaVersion);
            return D19PortMigrationResult.Success(target, sourceVersion, flags, wasAlreadyUpToDate);
        }
    }
}
