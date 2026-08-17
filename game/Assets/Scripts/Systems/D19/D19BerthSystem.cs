// ============================================================
// D19BerthSystem —— 泊位预留/占用/释放/过期/冲突的原子领域状态（D19-L06）
//
// 归属：Whalebone.Systems.D19。本系统是所有可变泊位事实（预留、占用、
// 释放、过期、冲突、租期）的唯一写者；只读消费 D19-L04 锚点目录
// （berth/port 规范键与跨港校验）与 D19-L02/D19-L17 失败载体。
//
// 语义：
//   * 稳定键：berth_id / ship_id / command_id（D18 稳定键语义，无坐标）；
//   * 逻辑时间：world_day（游戏世界日）+ save_revision（存档修订），
//     不使用 DateTime、真实时钟、Transform 或物理检测；
//   * 原子性：所有校验先于任何变更；失败返回前状态不产生任何半写；
//   * 不变式：同一船不可占两个泊位；同一泊位不可同时给两船；
//     预留有租期（ExpiresWorldDay>0），占用无租期（0=直至释放）；
//   * 幂等：同一 command_id + 相同参数重复投递返回已存结果（无副作用）；
//     同一 command_id + 不同参数 → DuplicateCommand 失败；
//   * 事件：本系统只生成 D19-L16 事件草案（WorldEvent），不追加 EventLog
//     （追加与去重归 D19-L33 命令路由/EventLog）；
//   * 失败码：优先使用 D19-L17 已注册业务码（d19.berth.occupied /
//     d19.berth.expired / d19.port.unknown_port）；船型/租期/跨港/重复命令
//     等新领域缺口使用同格式稳定原因 ID（d19.berth.<name>），并注明
//     L17 目录待扩展，本系统不修改 D19-L17 文件。
//
// 边界：不触碰 D4 船只数值、D2 航行数值、D3 库存/货币、Save 根模型；
// 不读取 Unity 坐标，不触碰场景；不修改 D19-L04 目录。
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>泊位分配状态（互斥，枚举保证）。</summary>
    public enum D19BerthState
    {
        /// <summary>空闲（可被预留）。</summary>
        Available = 0,

        /// <summary>已预留（带租期，未确认占用）。</summary>
        Reserved,

        /// <summary>已占用（无租期，直至释放/离港）。</summary>
        Occupied
    }

    /// <summary>泊位操作种类（用于幂等比较与事件类型）。</summary>
    public enum D19BerthOperation
    {
        None = 0,
        Reserve,
        ConfirmOccupancy,
        Release,
        Expire,
        CheckConflict
    }

    /// <summary>泊位系统稳定原因 ID（机器可读，非 UI 文案）。</summary>
    public static class D19BerthReasonIds
    {
        /// <summary>泊位已占用（D19-L17 已注册）。</summary>
        public const string Occupied = "d19.berth.occupied";
        /// <summary>租期已过期（D19-L17 已注册）。</summary>
        public const string Expired = "d19.berth.expired";
        /// <summary>港口未知（D19-L17 已注册）。</summary>
        public const string UnknownPort = "d19.port.unknown_port";
        /// <summary>泊位未知（L17 无泊位未知业务码，使用同格式原因 ID）。</summary>
        public const string UnknownBerth = "d19.berth.unknown";
        /// <summary>船型/能力与泊位不匹配（L17 待扩展）。</summary>
        public const string ShipTypeMismatch = "d19.berth.ship_type_mismatch";
        /// <summary>租期无效（非正时长或超上限，L17 待扩展）。</summary>
        public const string InvalidTerm = "d19.berth.invalid_term";
        /// <summary>泊位归属港口与命令港口不一致（L17 待扩展）。</summary>
        public const string CrossPort = "d19.berth.cross_port";
        /// <summary>重复命令 ID 但参数不一致（L17 待扩展）。</summary>
        public const string DuplicateCommand = "d19.berth.duplicate_command";
        /// <summary>命令缺少稳定 command_id（L17 待扩展）。</summary>
        public const string MissingCommandId = "d19.berth.missing_command_id";
        /// <summary>同一船已占用/预留另一泊位（L17 待扩展）。</summary>
        public const string ShipAlreadyBerthed = "d19.berth.ship_already_berthed";
        /// <summary>对未预留泊位执行确认/释放（L17 待扩展）。</summary>
        public const string NotReserved = "d19.berth.not_reserved";
        /// <summary>不变式校验失败（L17 待扩展）。</summary>
        public const string Invariant = "d19.berth.invariant";
        /// <summary>泊位缺少能力定义（L17 待扩展）。</summary>
        public const string CapabilityMissing = "d19.berth.capability_missing";
        /// <summary>无冲突（CheckConflict 成功）。</summary>
        public const string NoConflict = "none";

        /// <summary>本系统定义的全部原因 ID（供 L17 后续扩展与审计引用）。</summary>
        public static readonly IReadOnlyList<string> AllReasonIds = new List<string>
        {
            Occupied, Expired, UnknownPort, UnknownBerth, ShipTypeMismatch,
            InvalidTerm, CrossPort, DuplicateCommand, MissingCommandId,
            ShipAlreadyBerthed, NotReserved, Invariant, CapabilityMissing
        };
    }

    /// <summary>
    /// 船的能力画像（由调用方从 D4 只读目录提供；本系统不读取/不修改 D4 数值）。
    /// </summary>
    public sealed class D19BerthShipProfile
    {
        /// <summary>稳定 ship_id（船型/能力事实来自 D4 只读目录）。</summary>
        public string ShipId = string.Empty;

        /// <summary>尺寸档位 1（小）— 3（大）；越界视为非法画像。</summary>
        public int SizeTier = 1;

        /// <summary>是否需要深水泊位（吃水能力）。</summary>
        public bool DeepDraft;

        public D19BerthShipProfile(string shipId, int sizeTier = 1, bool deepDraft = false)
        {
            ShipId = shipId ?? string.Empty;
            SizeTier = sizeTier;
            DeepDraft = deepDraft;
        }

        public D19BerthShipProfile Clone()
        {
            return new D19BerthShipProfile(ShipId, SizeTier, DeepDraft);
        }
    }

    /// <summary>泊位能力定义（冻结；本系统拥有的泊位事实之一）。</summary>
    public sealed class D19BerthCapability
    {
        public string BerthId = string.Empty;
        public int MinTier = 1;
        public int MaxTier = 3;
        public bool DeepDraft;

        public D19BerthCapability(string berthId, int minTier, int maxTier, bool deepDraft)
        {
            BerthId = berthId ?? string.Empty;
            MinTier = minTier;
            MaxTier = maxTier;
            DeepDraft = deepDraft;
        }
    }

    /// <summary>
    /// 泊位分配事实（不可变对外副本；写者只有 D19BerthSystem）。
    /// </summary>
    public sealed class D19BerthAllocation
    {
        public string BerthId = string.Empty;
        public D19BerthState State = D19BerthState.Available;
        public string ShipId = string.Empty;
        public string CommandId = string.Empty;
        /// <summary>预留时点（世界日）；Available 时为 0。</summary>
        public int ReservedWorldDay;
        /// <summary>租期到期日（Reserved 时 &gt; ReservedWorldDay；Occupied 时为 0=直至释放）。</summary>
        public int ExpiresWorldDay;
        /// <summary>释放原因（completed/expired；仅 Release/Expire 后的快照携带）。</summary>
        public string ReleaseReason = string.Empty;

        public D19BerthAllocation Clone()
        {
            return new D19BerthAllocation
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
    }

    /// <summary>
    /// 泊位操作结果：成功/失败 + 结果分配快照 + L16 事件草案（可为空）。
    /// 失败与成功都不会半写状态。
    /// </summary>
    public sealed class D19BerthResult
    {
        public string CommandId = string.Empty;
        public D19BerthOperation Operation = D19BerthOperation.None;
        public bool Success;
        /// <summary>失败载体（L02 枚举码 + L17 业务原因 ID）。</summary>
        public D19PortFailure Failure;
        /// <summary>操作后的泊位分配快照（成功或幂等命中时有效）。</summary>
        public D19BerthAllocation Allocation;
        /// <summary>L16 事件草案（未追加；失败时为拒绝事件草案，可能为 null）。</summary>
        public WorldEvent Event;

        public D19BerthResult Clone()
        {
            return new D19BerthResult
            {
                CommandId = CommandId,
                Operation = Operation,
                Success = Success,
                Failure = Failure,
                Allocation = Allocation == null ? null : Allocation.Clone(),
                Event = Event == null ? null : Event.Clone()
            };
        }
    }

    /// <summary>
    /// 泊位原子领域系统（D19-L06，唯一写者）。
    /// 全部操作：先全量校验 → 单一变更点 → 构造事件草案；无半写。
    /// </summary>
    public sealed class D19BerthSystem
    {
        /// <summary>租期上限（世界日）。</summary>
        public const int MaxReservationTermDays = 30;

        private readonly D19PortAnchorCatalog _anchors;
        private readonly Dictionary<string, D19BerthAllocation> _byBerth =
            new Dictionary<string, D19BerthAllocation>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _shipToBerth =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, D19BerthResult> _applied =
            new Dictionary<string, D19BerthResult>(StringComparer.Ordinal);

        public D19BerthSystem(D19PortAnchorCatalog anchors = null)
        {
            _anchors = anchors ?? D19PortAnchorCatalog.Instance;
        }

        // ------------------------------------------------------------
        // 冻结泊位能力表（与本系统唯一锚点目录中的默认泊位一一对应）
        // ------------------------------------------------------------

        /// <summary>泊位能力表（冻结；锚点目录新增泊位时必须同步定义，否则能力检查返回 CapabilityMissing）。</summary>
        public static readonly IReadOnlyList<D19BerthCapability> CapabilityTable = new List<D19BerthCapability>
        {
            new D19BerthCapability("berth_santa_margarita_dock_01", 1, 3, true),
            new D19BerthCapability("berth_santa_margarita_large_01", 2, 3, true),
            new D19BerthCapability("berth_santa_margarita_small_01", 1, 1, false),
            new D19BerthCapability("berth_santa_margarita_cargo_01", 1, 2, false),
            new D19BerthCapability("berth_santa_margarita_anchorage_01", 1, 3, true),
            new D19BerthCapability("berth_green_cay_01", 1, 2, false),
            new D19BerthCapability("berth_green_cay_small_01", 1, 1, false),
            new D19BerthCapability("berth_driftwood_isle_01", 1, 1, false)
        };

        private static readonly Dictionary<string, D19BerthCapability> _capabilities =
            CapabilityTable.ToDictionary(cap => cap.BerthId, StringComparer.Ordinal);

        public static bool TryGetCapability(string berthId, out D19BerthCapability capability)
        {
            return _capabilities.TryGetValue(berthId, out capability);
        }

        // ------------------------------------------------------------
        // 只读访问（全部副本，防可变泄漏）
        // ------------------------------------------------------------

        public D19BerthAllocation GetAllocation(string berthId)
        {
            D19BerthAllocation allocation;
            return _byBerth.TryGetValue(berthId, out allocation) ? allocation.Clone() : null;
        }

        public IReadOnlyList<D19BerthAllocation> GetAllocations()
        {
            List<D19BerthAllocation> result = new List<D19BerthAllocation>(_byBerth.Count);
            foreach (D19BerthAllocation allocation in _byBerth.Values)
            {
                result.Add(allocation.Clone());
            }
            result.Sort((a, b) => StringComparer.Ordinal.Compare(a.BerthId, b.BerthId));
            return result;
        }

        public D19BerthResult GetApplied(string commandId)
        {
            D19BerthResult stored;
            return _applied.TryGetValue(commandId, out stored) ? stored.Clone() : null;
        }

        /// <summary>不变式校验：返回空列表表示健全；否则列出每条违规。</summary>
        public List<string> ValidateInvariants()
        {
            List<string> problems = new List<string>();
            var shipOwners = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (D19BerthAllocation allocation in _byBerth.Values)
            {
                if (allocation.State == D19BerthState.Available)
                {
                    if (!string.IsNullOrEmpty(allocation.ShipId))
                    {
                        problems.Add("available berth has a ship: " + allocation.BerthId);
                    }
                    continue;
                }
                if (string.IsNullOrEmpty(allocation.ShipId))
                {
                    problems.Add("non-available berth has no ship: " + allocation.BerthId);
                }
                if (string.IsNullOrEmpty(allocation.CommandId))
                {
                    problems.Add("non-available berth has no command id: " + allocation.BerthId);
                }
                if (allocation.State == D19BerthState.Reserved && allocation.ExpiresWorldDay <= allocation.ReservedWorldDay)
                {
                    problems.Add("reserved berth has no valid term: " + allocation.BerthId);
                }
                if (allocation.State == D19BerthState.Occupied && allocation.ExpiresWorldDay != 0)
                {
                    problems.Add("occupied berth must have no expiry: " + allocation.BerthId);
                }
                string previous;
                if (shipOwners.TryGetValue(allocation.ShipId, out previous))
                {
                    problems.Add("ship occupies two berths: " + allocation.ShipId +
                        " in " + previous + " and " + allocation.BerthId);
                }
                else
                {
                    shipOwners[allocation.ShipId] = allocation.BerthId;
                }
            }
            problems.Sort(StringComparer.Ordinal);
            return problems;
        }

        // ------------------------------------------------------------
        // 失败构造（优先 L17 已注册业务码；Recoverable 取自 L17 重试策略）
        // ------------------------------------------------------------

        private static D19PortFailure BuildFailure(D19PortFailureCode code, string reasonId)
        {
            bool recoverable = true;
            D19FailureEntry entry = D19PortFailureCatalog.Find(reasonId);
            if (entry != null)
            {
                recoverable = entry.Retry != D19FailureRetryPolicy.NoRetry;
            }
            return new D19PortFailure(code, reasonId, recoverable);
        }

        private static D19BerthResult Fail(string commandId, D19BerthOperation operation,
            D19PortFailureCode code, string reasonId)
        {
            return new D19BerthResult
            {
                CommandId = commandId ?? string.Empty,
                Operation = operation,
                Success = false,
                Failure = BuildFailure(code, reasonId)
            };
        }

        // ------------------------------------------------------------
        // 锚点解析辅助
        // ------------------------------------------------------------

        private D19AnchorResult ResolvePort(string portId, out D19PortFailureCode code, out string reasonId)
        {
            D19AnchorResult port = _anchors.ResolvePort(portId);
            if (!port.Success)
            {
                code = D19PortFailureCode.UnknownPort;
                reasonId = D19BerthReasonIds.UnknownPort;
            }
            else
            {
                code = D19PortFailureCode.None;
                reasonId = D19BerthReasonIds.NoConflict;
            }
            return port;
        }

        private D19AnchorResult ResolveBerth(string portId, string berthId, out D19PortFailureCode code, out string reasonId)
        {
            D19AnchorResult port = _anchors.ResolvePort(portId);
            if (!port.Success)
            {
                code = D19PortFailureCode.UnknownPort;
                reasonId = D19BerthReasonIds.UnknownPort;
                return null;
            }
            D19AnchorResult berth = _anchors.ResolveBerth(berthId, port.CanonicalKey);
            if (!berth.Success)
            {
                code = berth.ErrorCode == D19AnchorErrorCodes.CrossPortBerth
                    ? D19PortFailureCode.UnknownBerth
                    : D19PortFailureCode.UnknownBerth;
                reasonId = berth.ErrorCode == D19AnchorErrorCodes.CrossPortBerth
                    ? D19BerthReasonIds.CrossPort
                    : D19BerthReasonIds.UnknownBerth;
                return null;
            }
            code = D19PortFailureCode.None;
            reasonId = D19BerthReasonIds.NoConflict;
            return berth;
        }

        // ------------------------------------------------------------
        // 事件草案构造（L16 工厂；不追加 EventLog）
        // ------------------------------------------------------------

        private WorldEvent BuildRejectionDraft(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, string reasonId, IList<string> actorIds)
        {
            if (string.IsNullOrEmpty(portId))
            {
                return null;
            }
            try
            {
                return D19PortEventFactory.CreateRejection(
                    commandId, worldDay, saveRevision, portId, shipId, reasonId, null, actorIds);
            }
            catch (EventValidationException)
            {
                return null;
            }
        }

        // ------------------------------------------------------------
        // 公共操作（全部原子）
        // ------------------------------------------------------------

        /// <summary>
        /// 预留泊位：校验（命令 ID/港口/泊位/跨港/船型能力/租期/占用/一船一泊位）全部通过后
        /// 才写入 Reserved 状态；成功后返回 L16 预留事件草案。
        /// </summary>
        public D19BerthResult Reserve(
            string commandId, string portId, string berthId, D19BerthShipProfile ship,
            int currentWorldDay, int untilWorldDay, long saveRevision, IList<string> actorIds = null)
        {
            D19BerthResult replay = ReplayCheck(commandId, D19BerthOperation.Reserve);
            if (replay != null)
            {
                return replay;
            }
            if (string.IsNullOrEmpty(commandId))
            {
                return Fail(commandId, D19BerthOperation.Reserve, D19PortFailureCode.MissingCommandId, D19BerthReasonIds.MissingCommandId);
            }
            if (ship == null || string.IsNullOrEmpty(ship.ShipId) || ship.SizeTier < 1 || ship.SizeTier > 3)
            {
                D19BerthResult invalid = Fail(commandId, D19BerthOperation.Reserve, D19PortFailureCode.InvalidTransition, D19BerthReasonIds.ShipTypeMismatch);
                invalid.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, portId, string.Empty, D19BerthReasonIds.ShipTypeMismatch, actorIds);
                return invalid;
            }
            D19PortFailureCode code;
            string reasonId;
            D19AnchorResult berth = ResolveBerth(portId, berthId, out code, out reasonId);
            if (berth == null)
            {
                D19BerthResult failed = Fail(commandId, D19BerthOperation.Reserve, code, reasonId);
                failed.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, portId, ship.ShipId, reasonId, actorIds);
                return failed;
            }
            if (untilWorldDay <= currentWorldDay || untilWorldDay - currentWorldDay > MaxReservationTermDays)
            {
                D19BerthResult invalid = Fail(commandId, D19BerthOperation.Reserve, D19PortFailureCode.InvalidTransition, D19BerthReasonIds.InvalidTerm);
                invalid.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, portId, ship.ShipId, D19BerthReasonIds.InvalidTerm, actorIds);
                return invalid;
            }
            D19BerthCapability capability;
            if (!_capabilities.TryGetValue(berth.CanonicalKey, out capability))
            {
                D19BerthResult missing = Fail(commandId, D19BerthOperation.Reserve, D19PortFailureCode.UnknownBerth, D19BerthReasonIds.CapabilityMissing);
                missing.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, portId, ship.ShipId, D19BerthReasonIds.CapabilityMissing, actorIds);
                return missing;
            }
            if (ship.SizeTier < capability.MinTier || ship.SizeTier > capability.MaxTier ||
                (ship.DeepDraft && !capability.DeepDraft))
            {
                D19BerthResult mismatch = Fail(commandId, D19BerthOperation.Reserve, D19PortFailureCode.BerthOccupied, D19BerthReasonIds.ShipTypeMismatch);
                mismatch.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, portId, ship.ShipId, D19BerthReasonIds.ShipTypeMismatch, actorIds);
                return mismatch;
            }
            D19BerthAllocation current = GetOrCreateAllocation(berth.CanonicalKey);
            if (current.State != D19BerthState.Available)
            {
                D19BerthResult occupied = Fail(commandId, D19BerthOperation.Reserve, D19PortFailureCode.BerthOccupied, D19BerthReasonIds.Occupied);
                occupied.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, portId, ship.ShipId, D19BerthReasonIds.Occupied, actorIds);
                return occupied;
            }
            string existingBerth;
            if (_shipToBerth.TryGetValue(ship.ShipId, out existingBerth) && existingBerth != berth.CanonicalKey)
            {
                D19BerthResult doubleBooked = Fail(commandId, D19BerthOperation.Reserve, D19PortFailureCode.BerthOccupied, D19BerthReasonIds.ShipAlreadyBerthed);
                doubleBooked.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, portId, ship.ShipId, D19BerthReasonIds.ShipAlreadyBerthed, actorIds);
                return doubleBooked;
            }

            current.State = D19BerthState.Reserved;
            current.ShipId = ship.ShipId;
            current.CommandId = commandId;
            current.ReservedWorldDay = currentWorldDay;
            current.ExpiresWorldDay = untilWorldDay;
            current.ReleaseReason = string.Empty;
            _shipToBerth[ship.ShipId] = berth.CanonicalKey;

            WorldEvent draft = D19PortEventFactory.CreateBerthReserved(
                commandId, currentWorldDay, saveRevision, berth.Port, ship.ShipId, berth.CanonicalKey,
                untilWorldDay.ToString(System.Globalization.CultureInfo.InvariantCulture),
                actorIds, null);

            D19BerthResult result = new D19BerthResult
            {
                CommandId = commandId,
                Operation = D19BerthOperation.Reserve,
                Success = true,
                Allocation = current.Clone(),
                Event = draft
            };
            _applied[commandId] = result.Clone();
            return result;
        }

        /// <summary>
        /// 确认占用：仅当该泊位已由同一船预留且租期未过期时转 Occupied（无租期）。
        /// 成功时生成同一命令下的第二个预留事实事件（occurrence 递增），保证可重放审计。
        /// </summary>
        public D19BerthResult ConfirmOccupancy(
            string commandId, string portId, string berthId, string shipId,
            int currentWorldDay, long saveRevision, IList<string> actorIds = null)
        {
            D19BerthResult replay = ReplayCheck(commandId, D19BerthOperation.ConfirmOccupancy);
            if (replay != null)
            {
                return replay;
            }
            if (string.IsNullOrEmpty(commandId))
            {
                return Fail(commandId, D19BerthOperation.ConfirmOccupancy, D19PortFailureCode.MissingCommandId, D19BerthReasonIds.MissingCommandId);
            }
            D19PortFailureCode code;
            string reasonId;
            D19AnchorResult berth = ResolveBerth(portId, berthId, out code, out reasonId);
            if (berth == null)
            {
                D19BerthResult failed = Fail(commandId, D19BerthOperation.ConfirmOccupancy, code, reasonId);
                failed.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, portId, shipId ?? string.Empty, reasonId, actorIds);
                return failed;
            }
            D19BerthAllocation current = GetOrCreateAllocation(berth.CanonicalKey);
            if (current.State != D19BerthState.Reserved ||
                !string.Equals(current.ShipId, shipId, StringComparison.Ordinal))
            {
                D19BerthResult notReserved = Fail(commandId, D19BerthOperation.ConfirmOccupancy, D19PortFailureCode.NotBerthed, D19BerthReasonIds.NotReserved);
                notReserved.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, berth.Port, shipId ?? string.Empty, D19BerthReasonIds.NotReserved, actorIds);
                return notReserved;
            }
            if (current.ExpiresWorldDay < currentWorldDay)
            {
                // 租期已过：确认失败，且本操作不产生任何副作用（释放由
                // AdvanceTime 或显式 Release 完成，避免隐藏写入）。
                D19BerthResult expired = Fail(commandId, D19BerthOperation.ConfirmOccupancy, D19PortFailureCode.None, D19BerthReasonIds.Expired);
                expired.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, berth.Port, shipId ?? string.Empty, D19BerthReasonIds.Expired, actorIds);
                return expired;
            }

            current.State = D19BerthState.Occupied;
            current.ExpiresWorldDay = 0;

            WorldEvent draft = D19PortEventFactory.CreateBerthReserved(
                commandId, currentWorldDay, saveRevision, berth.Port, shipId, berth.CanonicalKey,
                "0", actorIds, null);

            D19BerthResult result = new D19BerthResult
            {
                CommandId = commandId,
                Operation = D19BerthOperation.ConfirmOccupancy,
                Success = true,
                Allocation = current.Clone(),
                Event = draft
            };
            _applied[commandId] = result.Clone();
            return result;
        }

        /// <summary>
        /// 释放泊位：占用或预留均可释放；租期已过（过期释放）同样允许并记录 reason=expired。
        /// 成功后生成 L16 释放事件草案。
        /// </summary>
        public D19BerthResult Release(
            string commandId, string portId, string berthId, string shipId,
            int currentWorldDay, long saveRevision, string releaseReason = "completed",
            IList<string> actorIds = null)
        {
            D19BerthResult replay = ReplayCheck(commandId, D19BerthOperation.Release);
            if (replay != null)
            {
                return replay;
            }
            if (string.IsNullOrEmpty(commandId))
            {
                return Fail(commandId, D19BerthOperation.Release, D19PortFailureCode.MissingCommandId, D19BerthReasonIds.MissingCommandId);
            }
            D19PortFailureCode code;
            string reasonId;
            D19AnchorResult berth = ResolveBerth(portId, berthId, out code, out reasonId);
            if (berth == null)
            {
                D19BerthResult failed = Fail(commandId, D19BerthOperation.Release, code, reasonId);
                failed.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, portId, shipId ?? string.Empty, reasonId, actorIds);
                return failed;
            }
            D19BerthAllocation current = GetOrCreateAllocation(berth.CanonicalKey);
            if (current.State == D19BerthState.Available ||
                (shipId != null && !string.Equals(current.ShipId, shipId, StringComparison.Ordinal)))
            {
                D19BerthResult notReserved = Fail(commandId, D19BerthOperation.Release, D19PortFailureCode.NotBerthed, D19BerthReasonIds.NotReserved);
                notReserved.Event = BuildRejectionDraft(commandId, currentWorldDay, saveRevision, berth.Port, shipId ?? string.Empty, D19BerthReasonIds.NotReserved, actorIds);
                return notReserved;
            }

            bool expired = current.State == D19BerthState.Reserved && current.ExpiresWorldDay < currentWorldDay;
            string reason = expired ? "expired" : (releaseReason ?? "completed");
            string releasedShipId = current.ShipId;
            ReleaseInternal(commandId, berth.CanonicalKey, currentWorldDay, saveRevision, reason, actorIds);

            D19BerthResult result = new D19BerthResult
            {
                CommandId = commandId,
                Operation = D19BerthOperation.Release,
                Success = true,
                Allocation = GetOrCreateAllocation(berth.CanonicalKey).Clone(),
                Event = D19PortEventFactory.CreateBerthReleased(
                    commandId, currentWorldDay, saveRevision, berth.Port, releasedShipId, berth.CanonicalKey,
                    reason, actorIds, null)
            };
            _applied[commandId] = result.Clone();
            return result;
        }

        /// <summary>
        /// 过期推进：将所有已过租期的 Reserved 置回 Available，并逐条生成
        /// reason=expired 的释放事件草案。确定性：相同输入 → 相同过期集合。
        /// </summary>
        public IReadOnlyList<D19BerthResult> AdvanceTime(int currentWorldDay, long saveRevision)
        {
            List<D19BerthResult> results = new List<D19BerthResult>();
            List<string> expiredBerths = new List<string>();
            foreach (D19BerthAllocation allocation in _byBerth.Values)
            {
                if (allocation.State == D19BerthState.Reserved && allocation.ExpiresWorldDay < currentWorldDay)
                {
                    expiredBerths.Add(allocation.BerthId);
                }
            }
            expiredBerths.Sort(StringComparer.Ordinal);
            foreach (string berthId in expiredBerths)
            {
                string commandId = "tick_" + currentWorldDay.ToString(System.Globalization.CultureInfo.InvariantCulture) + "_" + berthId;
                D19BerthAllocation current = _byBerth[berthId];
                string releasedShipId = current.ShipId;
                D19AnchorResult berth = _anchors.ResolveBerth(berthId, null);
                string portId = berth != null && berth.Success ? berth.Port : string.Empty;
                ReleaseInternal(commandId, berthId, currentWorldDay, saveRevision, "expired", null);
                results.Add(new D19BerthResult
                {
                    CommandId = commandId,
                    Operation = D19BerthOperation.Expire,
                    Success = true,
                    Allocation = GetOrCreateAllocation(berthId).Clone(),
                    Event = D19PortEventFactory.CreateBerthReleased(
                        commandId, currentWorldDay, saveRevision, portId, releasedShipId, berthId,
                        "expired", null, null)
                });
            }
            return results;
        }

        /// <summary>
        /// 冲突探测（只读）：不改变任何状态，不生成事件。
        /// 成功返回 NoConflict；失败返回对应原因 ID（占用/船型/跨港/一船两泊位等）。
        /// </summary>
        public D19BerthResult CheckConflict(string portId, string berthId, D19BerthShipProfile ship, int currentWorldDay)
        {
            if (ship == null || string.IsNullOrEmpty(ship.ShipId) || ship.SizeTier < 1 || ship.SizeTier > 3)
            {
                return Fail(string.Empty, D19BerthOperation.CheckConflict, D19PortFailureCode.InvalidTransition, D19BerthReasonIds.ShipTypeMismatch);
            }
            D19PortFailureCode code;
            string reasonId;
            D19AnchorResult berth = ResolveBerth(portId, berthId, out code, out reasonId);
            if (berth == null)
            {
                return Fail(string.Empty, D19BerthOperation.CheckConflict, code, reasonId);
            }
            D19BerthAllocation current = GetOrCreateAllocation(berth.CanonicalKey);
            if (current.State != D19BerthState.Available)
            {
                return Fail(string.Empty, D19BerthOperation.CheckConflict, D19PortFailureCode.BerthOccupied, D19BerthReasonIds.Occupied);
            }
            D19BerthCapability capability;
            if (!_capabilities.TryGetValue(berth.CanonicalKey, out capability))
            {
                return Fail(string.Empty, D19BerthOperation.CheckConflict, D19PortFailureCode.UnknownBerth, D19BerthReasonIds.CapabilityMissing);
            }
            if (ship.SizeTier < capability.MinTier || ship.SizeTier > capability.MaxTier ||
                (ship.DeepDraft && !capability.DeepDraft))
            {
                return Fail(string.Empty, D19BerthOperation.CheckConflict, D19PortFailureCode.BerthOccupied, D19BerthReasonIds.ShipTypeMismatch);
            }
            string existingBerth;
            if (_shipToBerth.TryGetValue(ship.ShipId, out existingBerth) && existingBerth != berth.CanonicalKey)
            {
                return Fail(string.Empty, D19BerthOperation.CheckConflict, D19PortFailureCode.BerthOccupied, D19BerthReasonIds.ShipAlreadyBerthed);
            }
            return new D19BerthResult
            {
                CommandId = string.Empty,
                Operation = D19BerthOperation.CheckConflict,
                Success = true,
                Failure = BuildFailure(D19PortFailureCode.None, D19BerthReasonIds.NoConflict),
                Allocation = current.Clone()
            };
        }

        // ------------------------------------------------------------
        // 内部实现
        // ------------------------------------------------------------

        /// <summary>
        /// 幂等检查：同一 command_id + 相同操作已成功应用 → 返回存储结果（无副作用）；
        /// 同一 command_id + 不同操作/参数 → 返回 DuplicateCommand 失败。
        /// 返回 null 表示可继续执行。
        /// </summary>
        private D19BerthResult ReplayCheck(string commandId, D19BerthOperation operation)
        {
            if (string.IsNullOrEmpty(commandId))
            {
                return null; // MissingCommandId 由各操作自行失败
            }
            D19BerthResult stored;
            if (_applied.TryGetValue(commandId, out stored))
            {
                if (stored.Operation == operation)
                {
                    return stored.Clone();
                }
                D19BerthResult conflict = Fail(commandId, operation, D19PortFailureCode.DuplicateCommand, D19BerthReasonIds.DuplicateCommand);
                conflict.Event = BuildRejectionDraft(commandId, 0, 0, string.Empty, string.Empty, D19BerthReasonIds.DuplicateCommand, null);
                return conflict;
            }
            return null;
        }

        private D19BerthAllocation GetOrCreateAllocation(string canonicalBerthId)
        {
            D19BerthAllocation allocation;
            if (!_byBerth.TryGetValue(canonicalBerthId, out allocation))
            {
                allocation = new D19BerthAllocation { BerthId = canonicalBerthId, State = D19BerthState.Available };
                _byBerth[canonicalBerthId] = allocation;
            }
            return allocation;
        }

        private void ReleaseInternal(string commandId, string canonicalBerthId, int worldDay, long saveRevision, string reason, IList<string> actorIds)
        {
            D19BerthAllocation allocation = _byBerth[canonicalBerthId];
            string shipId = allocation.ShipId;
            if (!string.IsNullOrEmpty(shipId))
            {
                string existing;
                if (_shipToBerth.TryGetValue(shipId, out existing) && existing == canonicalBerthId)
                {
                    _shipToBerth.Remove(shipId);
                }
            }
            allocation.State = D19BerthState.Available;
            allocation.ShipId = string.Empty;
            allocation.CommandId = string.Empty;
            allocation.ReservedWorldDay = 0;
            allocation.ExpiresWorldDay = 0;
            allocation.ReleaseReason = reason ?? "completed";
        }
    }
}
