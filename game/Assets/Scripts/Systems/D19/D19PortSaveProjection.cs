// ============================================================
// D19PortSaveProjection —— D19 港口可保存/可恢复纯投影模型（纯 C#，D19-L20）
//
// 归属：Whalebone.Systems.D19（所有者：本系统，D19-L20）
// 目的：定义 D19 可保存/可恢复的纯投影：
//       1. 访问状态（VisitState, PortId, BerthId, ShipId）
//       2. 泊位分配与租期（Allocations: BerthId, State, ShipId, CommandId, ReservedWorldDay, ExpiresWorldDay, ReleaseReason）
//       3. 服务状态/快照（ServiceRecords / ServiceStates）
//       4. 成员逻辑位置（MemberLocations: MemberId -> D19CrewLocation）
//       5. 命令幂等记录（Idempotency: D19IdempotencySaveProjection）
//       6. 未完成命令/补偿草案（PendingCommands: CommandId, Kind, Fingerprint, WorldDay, Payload / CompensationDrafts）
//       7. 事件游标与版本（LastEventSequence, LastEventId, SchemaVersion, SaveRevision）
//
// 铁律与约束：
//   * 零 Unity 依赖、零 UnityEngine 引用、零 Transform/GameObject/Canvas/UI 持久化；
//   * 序列化字段全部为可序列化标量、列表或字典，字段命名稳定、版本化；
//   * 严禁包含绝对路径、场景引用或环境指针；
//   * 每个 D19 可变事实有唯一投影位置；
//   * 无不可保存的全局静态状态（无 static 业务状态，纯 DTO + 纯投影转换）；
//   * 绝不复制/替换整个 Save 根模型（GameState/PlayerState 由 SaveSystem 裁决）；
//   * D19 通过 SaveSystem 公开扩展接口（Load/Save 投影、LoadD9Runtime 等）对接；
//   * 旧 Save 尚缺字段的处理留给 L21（本文件定义当前稳定投影 SchemaVersion=1 与安全默认值）。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 成员逻辑位置投影条目（纯 C#，D19-L20）。
    /// </summary>
    [Serializable]
    public sealed class D19MemberLocationSaveEntry
    {
        /// <summary>成员稳定实体 ID。</summary>
        public string MemberId = string.Empty;

        /// <summary>成员逻辑位置（OnShore / OnBoard）。</summary>
        public string Location = "OnShore";

        public D19MemberLocationSaveEntry() { }

        public D19MemberLocationSaveEntry(string memberId, string location)
        {
            MemberId = memberId ?? string.Empty;
            Location = location ?? "OnShore";
        }

        public D19MemberLocationSaveEntry Clone()
        {
            return new D19MemberLocationSaveEntry(MemberId, Location);
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(MemberId))
                throw new InvalidOperationException("D19 成员位置投影缺少 MemberId。");
        }
    }

    /// <summary>
    /// 泊位分配状态投影条目（纯 C#，D19-L20）。
    /// </summary>
    [Serializable]
    public sealed class D19BerthAllocationSaveEntry
    {
        public string BerthId = string.Empty;
        public string State = "Available"; // D19BerthState 枚举名
        public string ShipId = string.Empty;
        public string CommandId = string.Empty;
        public int ReservedWorldDay;
        public int ExpiresWorldDay;
        public string ReleaseReason = string.Empty;

        public D19BerthAllocationSaveEntry() { }

        public D19BerthAllocationSaveEntry Clone()
        {
            return new D19BerthAllocationSaveEntry
            {
                BerthId = BerthId,
                State = State,
                ShipId = ShipId,
                CommandId = CommandId,
                ReservedWorldDay = ReservedWorldDay,
                ExpiresWorldDay = ExpiresWorldDay,
                ReleaseReason = ReleaseReason
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(BerthId))
                throw new InvalidOperationException("D19 泊位分配投影缺少 BerthId。");
        }
    }

    /// <summary>
    /// 未完成命令 / 挂起命令投影条目（纯 C#，D19-L20）。
    /// </summary>
    [Serializable]
    public sealed class D19PendingCommandSaveEntry
    {
        public string CommandId = string.Empty;
        public string Kind = string.Empty; // D19PortCommandKind 枚举名
        public string Fingerprint = string.Empty;
        public string PortId = string.Empty;
        public string BerthId = string.Empty;
        public string ActorId = string.Empty;
        public int WorldDay;
        public long SaveRevision;
        public Dictionary<string, string> Payload = new Dictionary<string, string>(StringComparer.Ordinal);

        public D19PendingCommandSaveEntry() { }

        public D19PendingCommandSaveEntry Clone()
        {
            var result = new D19PendingCommandSaveEntry
            {
                CommandId = CommandId,
                Kind = Kind,
                Fingerprint = Fingerprint,
                PortId = PortId,
                BerthId = BerthId,
                ActorId = ActorId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision
            };
            if (Payload != null)
            {
                foreach (var kv in Payload)
                {
                    result.Payload[kv.Key] = kv.Value;
                }
            }
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CommandId))
                throw new InvalidOperationException("D19 未完成命令投影缺少 CommandId。");
        }
    }

    /// <summary>
    /// 补偿草案投影条目（纯 C#，D19-L20，供 L34 补偿系统持久化）。
    /// </summary>
    [Serializable]
    public sealed class D19CompensationDraftSaveEntry
    {
        public string DraftId = string.Empty;
        public string OriginalCommandId = string.Empty;
        public string Domain = string.Empty;
        public string Reason = string.Empty;
        public string Status = "Pending"; // Pending / Executed / Cancelled
        public int WorldDay;
        public long SaveRevision;
        public Dictionary<string, string> Parameters = new Dictionary<string, string>(StringComparer.Ordinal);

        public D19CompensationDraftSaveEntry() { }

        public D19CompensationDraftSaveEntry Clone()
        {
            var result = new D19CompensationDraftSaveEntry
            {
                DraftId = DraftId,
                OriginalCommandId = OriginalCommandId,
                Domain = Domain,
                Reason = Reason,
                Status = Status,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision
            };
            if (Parameters != null)
            {
                foreach (var kv in Parameters)
                {
                    result.Parameters[kv.Key] = kv.Value;
                }
            }
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(DraftId))
                throw new InvalidOperationException("D19 补偿草案投影缺少 DraftId。");
            if (string.IsNullOrWhiteSpace(OriginalCommandId))
                throw new InvalidOperationException("D19 补偿草案投影缺少 OriginalCommandId。");
        }
    }

    /// <summary>
    /// 港口服务状态投影条目（纯 C#，D19-L20）。
    /// </summary>
    [Serializable]
    public sealed class D19PortServiceSaveEntry
    {
        public string ServiceId = string.Empty;
        public string PortId = string.Empty;
        public string FacilityState = "Open";
        public bool Staffed = true;
        public int LastUpdatedWorldDay;

        public D19PortServiceSaveEntry() { }

        public D19PortServiceSaveEntry Clone()
        {
            return new D19PortServiceSaveEntry
            {
                ServiceId = ServiceId,
                PortId = PortId,
                FacilityState = FacilityState,
                Staffed = Staffed,
                LastUpdatedWorldDay = LastUpdatedWorldDay
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ServiceId))
                throw new InvalidOperationException("D19 港口服务投影缺少 ServiceId。");
        }
    }

    /// <summary>
    /// D19 港口领域可保存/可恢复的完整纯投影模型（纯 C#，D19-L20）。
    /// 涵盖：访问状态、泊位租期、服务状态、成员位置、幂等记录、未完成命令/补偿草案、事件游标与版本。
    /// </summary>
    [Serializable]
    public sealed class D19PortSaveProjection
    {
        public const int CurrentSchemaVersion = 1;

        /// <summary>投影 Schema 版本（单调递增，版本化）。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>船只稳定标识。</summary>
        public string ShipId = string.Empty;

        /// <summary>当前访问港口稳定键（可为空）。</summary>
        public string PortId = string.Empty;

        /// <summary>当前访问状态（D19PortVisitState 枚举名）。</summary>
        public string VisitState = "None";

        /// <summary>当前占用/分配的泊位 ID（仅 Berthed/InService 时有效）。</summary>
        public string BerthId = string.Empty;

        /// <summary>最后保存的世界日。</summary>
        public int WorldDay;

        /// <summary>单调递增的存档修订号。</summary>
        public long SaveRevision;

        /// <summary>最后处理的事件 ID 游标。</summary>
        public string LastEventId = string.Empty;

        /// <summary>最后处理的事件序列号游标。</summary>
        public long LastEventSequence;

        /// <summary>泊位分配事实列表。</summary>
        public List<D19BerthAllocationSaveEntry> BerthAllocations = new List<D19BerthAllocationSaveEntry>();

        /// <summary>成员逻辑位置列表。</summary>
        public List<D19MemberLocationSaveEntry> MemberLocations = new List<D19MemberLocationSaveEntry>();

        /// <summary>港口服务状态列表。</summary>
        public List<D19PortServiceSaveEntry> ServiceStates = new List<D19PortServiceSaveEntry>();

        /// <summary>未完成命令列表（处理中/待执行）。</summary>
        public List<D19PendingCommandSaveEntry> PendingCommands = new List<D19PendingCommandSaveEntry>();

        /// <summary>补偿草案列表（供中断恢复时对账）。</summary>
        public List<D19CompensationDraftSaveEntry> CompensationDrafts = new List<D19CompensationDraftSaveEntry>();

        /// <summary>命令幂等记录投影（D19-L22 提供）。</summary>
        public D19IdempotencySaveProjection Idempotency = new D19IdempotencySaveProjection();

        /// <summary>创建安全默认快照。</summary>
        public static D19PortSaveProjection CreateDefault(string shipId = "")
        {
            return new D19PortSaveProjection
            {
                SchemaVersion = CurrentSchemaVersion,
                ShipId = shipId ?? string.Empty,
                PortId = string.Empty,
                VisitState = D19PortVisitState.None.ToString(),
                BerthId = string.Empty,
                WorldDay = 0,
                SaveRevision = 0,
                LastEventId = string.Empty,
                LastEventSequence = 0,
                Idempotency = new D19IdempotencySaveProjection { ShipId = shipId ?? string.Empty }
            };
        }

        /// <summary>深拷贝投影副本。</summary>
        public D19PortSaveProjection Clone()
        {
            var clone = new D19PortSaveProjection
            {
                SchemaVersion = SchemaVersion,
                ShipId = ShipId,
                PortId = PortId,
                VisitState = VisitState,
                BerthId = BerthId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                LastEventId = LastEventId,
                LastEventSequence = LastEventSequence,
                Idempotency = Idempotency == null ? null : new D19IdempotencySaveProjection
                {
                    SchemaVersion = Idempotency.SchemaVersion,
                    ShipId = Idempotency.ShipId,
                    RetentionWorldDays = Idempotency.RetentionWorldDays,
                    MaxRetainedEntries = Idempotency.MaxRetainedEntries,
                    Entries = new List<D19IdempotencySaveEntry>()
                }
            };

            if (Idempotency != null && Idempotency.Entries != null)
            {
                foreach (var entry in Idempotency.Entries)
                {
                    if (entry != null)
                    {
                        clone.Idempotency.Entries.Add(new D19IdempotencySaveEntry
                        {
                            CommandId = entry.CommandId,
                            Fingerprint = entry.Fingerprint,
                            Domain = entry.Domain,
                            State = entry.State,
                            ResultCode = entry.ResultCode,
                            SummaryKey = entry.SummaryKey,
                            WorldDay = entry.WorldDay,
                            Occurrence = entry.Occurrence
                        });
                    }
                }
            }

            if (BerthAllocations != null)
            {
                foreach (var item in BerthAllocations)
                {
                    if (item != null) clone.BerthAllocations.Add(item.Clone());
                }
            }

            if (MemberLocations != null)
            {
                foreach (var item in MemberLocations)
                {
                    if (item != null) clone.MemberLocations.Add(item.Clone());
                }
            }

            if (ServiceStates != null)
            {
                foreach (var item in ServiceStates)
                {
                    if (item != null) clone.ServiceStates.Add(item.Clone());
                }
            }

            if (PendingCommands != null)
            {
                foreach (var item in PendingCommands)
                {
                    if (item != null) clone.PendingCommands.Add(item.Clone());
                }
            }

            if (CompensationDrafts != null)
            {
                foreach (var item in CompensationDrafts)
                {
                    if (item != null) clone.CompensationDrafts.Add(item.Clone());
                }
            }

            return clone;
        }

        /// <summary>
        /// 确定性结构与版本验证。非法或版本不匹配时抛出明确异常。
        /// </summary>
        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
            {
                throw new InvalidOperationException("D19 港口投影 Schema 版本不匹配: 期望 " +
                    CurrentSchemaVersion + "，收到 " + SchemaVersion + "。");
            }

            if (WorldDay < 0)
                throw new InvalidOperationException("D19 港口投影 WorldDay 不能为负数。");

            if (SaveRevision < 0)
                throw new InvalidOperationException("D19 港口投影 SaveRevision 不能为负数。");

            if (BerthAllocations != null)
            {
                foreach (var b in BerthAllocations)
                {
                    if (b != null) b.Validate();
                }
            }

            if (MemberLocations != null)
            {
                foreach (var m in MemberLocations)
                {
                    if (m != null) m.Validate();
                }
            }

            if (ServiceStates != null)
            {
                foreach (var s in ServiceStates)
                {
                    if (s != null) s.Validate();
                }
            }

            if (PendingCommands != null)
            {
                foreach (var p in PendingCommands)
                {
                    if (p != null) p.Validate();
                }
            }

            if (CompensationDrafts != null)
            {
                foreach (var c in CompensationDrafts)
                {
                    if (c != null) c.Validate();
                }
            }
        }
    }

    /// <summary>
    /// 港口投影建造与恢复助手（纯函数/纯映射，无全局静态状态）。
    /// </summary>
    public static class D19PortSaveProjectionMapper
    {
        /// <summary>
        /// 从 D19 各领域内存快照/数据构建完整纯保存投影。
        /// </summary>
        public static D19PortSaveProjection BuildProjection(
            string shipId,
            string portId,
            D19PortVisitState visitState,
            string berthId,
            int worldDay,
            long saveRevision,
            string lastEventId,
            long lastEventSequence,
            IEnumerable<D19BerthAllocation> berthAllocations,
            IDictionary<string, D19CrewLocation> memberLocations,
            IEnumerable<D19PortServiceSaveEntry> serviceStates,
            IEnumerable<D19PendingCommandSaveEntry> pendingCommands,
            IEnumerable<D19CompensationDraftSaveEntry> compensationDrafts,
            D19IdempotencySaveProjection idempotency)
        {
            var projection = new D19PortSaveProjection
            {
                SchemaVersion = D19PortSaveProjection.CurrentSchemaVersion,
                ShipId = shipId ?? string.Empty,
                PortId = portId ?? string.Empty,
                VisitState = visitState.ToString(),
                BerthId = berthId ?? string.Empty,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
                LastEventId = lastEventId ?? string.Empty,
                LastEventSequence = lastEventSequence,
                Idempotency = idempotency == null ? new D19IdempotencySaveProjection { ShipId = shipId ?? string.Empty } : idempotency
            };

            if (berthAllocations != null)
            {
                foreach (var a in berthAllocations)
                {
                    if (a != null)
                    {
                        projection.BerthAllocations.Add(new D19BerthAllocationSaveEntry
                        {
                            BerthId = a.BerthId ?? string.Empty,
                            State = a.State.ToString(),
                            ShipId = a.ShipId ?? string.Empty,
                            CommandId = a.CommandId ?? string.Empty,
                            ReservedWorldDay = a.ReservedWorldDay,
                            ExpiresWorldDay = a.ExpiresWorldDay,
                            ReleaseReason = a.ReleaseReason ?? string.Empty
                        });
                    }
                }
            }

            if (memberLocations != null)
            {
                foreach (var kv in memberLocations)
                {
                    if (!string.IsNullOrEmpty(kv.Key))
                    {
                        projection.MemberLocations.Add(new D19MemberLocationSaveEntry(kv.Key, kv.Value.ToString()));
                    }
                }
            }

            if (serviceStates != null)
            {
                foreach (var s in serviceStates)
                {
                    if (s != null) projection.ServiceStates.Add(s.Clone());
                }
            }

            if (pendingCommands != null)
            {
                foreach (var p in pendingCommands)
                {
                    if (p != null) projection.PendingCommands.Add(p.Clone());
                }
            }

            if (compensationDrafts != null)
            {
                foreach (var c in compensationDrafts)
                {
                    if (c != null) projection.CompensationDrafts.Add(c.Clone());
                }
            }

            return projection;
        }

        /// <summary>
        /// 尝试从投影中解析访问生命周期状态（失败返回 None）。
        /// </summary>
        public static D19PortVisitState ParseVisitState(D19PortSaveProjection projection)
        {
            if (projection == null || string.IsNullOrWhiteSpace(projection.VisitState))
                return D19PortVisitState.None;

            D19PortVisitState state;
            if (Enum.TryParse(projection.VisitState, true, out state))
                return state;

            return D19PortVisitState.None;
        }

        /// <summary>
        /// 尝试从投影恢复成员逻辑位置映射字典。
        /// </summary>
        public static Dictionary<string, D19CrewLocation> RestoreMemberLocations(D19PortSaveProjection projection)
        {
            var result = new Dictionary<string, D19CrewLocation>(StringComparer.Ordinal);
            if (projection == null || projection.MemberLocations == null)
                return result;

            foreach (var entry in projection.MemberLocations)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.MemberId))
                    continue;

                D19CrewLocation loc;
                if (Enum.TryParse(entry.Location, true, out loc))
                {
                    result[entry.MemberId] = loc;
                }
                else
                {
                    result[entry.MemberId] = D19CrewLocation.OnShore;
                }
            }

            return result;
        }

        /// <summary>
        /// 尝试从投影恢复泊位分配列表。
        /// </summary>
        public static List<D19BerthAllocation> RestoreBerthAllocations(D19PortSaveProjection projection)
        {
            var result = new List<D19BerthAllocation>();
            if (projection == null || projection.BerthAllocations == null)
                return result;

            foreach (var entry in projection.BerthAllocations)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.BerthId))
                    continue;

                D19BerthState state;
                if (!Enum.TryParse(entry.State, true, out state))
                    state = D19BerthState.Available;

                result.Add(new D19BerthAllocation
                {
                    BerthId = entry.BerthId,
                    State = state,
                    ShipId = entry.ShipId ?? string.Empty,
                    CommandId = entry.CommandId ?? string.Empty,
                    ReservedWorldDay = entry.ReservedWorldDay,
                    ExpiresWorldDay = entry.ExpiresWorldDay,
                    ReleaseReason = entry.ReleaseReason ?? string.Empty
                });
            }

            return result;
        }
    }
}
