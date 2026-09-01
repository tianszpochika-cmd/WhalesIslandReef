// ============================================================
// D22NotificationQueue —— D22-L13 通知队列与存档投影（纯 C#）
//
// 通知是 presentation projection，不是领域事件 owner。队列以显式世界日、
// 稳定事件 ID、去重键和单调序列维持确定性，不读取系统时钟或设备状态。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Whalebone.Systems.D22
{
    /// <summary>通知排序优先级。</summary>
    public enum D22NotificationPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3,
    }

    /// <summary>通知的逻辑生命周期。</summary>
    public enum D22NotificationState
    {
        Pending = 0,
        Acknowledged = 1,
        Expired = 2,
        Dismissed = 3,
    }

    /// <summary>通知在 UI 存档投影中的持久化策略。</summary>
    public enum D22NotificationPersistence
    {
        Transient = 0,
        UntilAcknowledged = 1,
        UntilExpired = 2,
        Always = 3,
    }

    /// <summary>通知操作的稳定结果码。</summary>
    public enum D22NotificationFailureCode
    {
        None = 0,
        MissingEvent = 1,
        InvalidSchemaVersion = 2,
        InvalidEventId = 3,
        InvalidDeduplicationKey = 4,
        InvalidLocalizationKey = 5,
        InvalidPriority = 6,
        InvalidWorldDay = 7,
        DuplicateEvent = 8,
        DuplicateEventConflict = 9,
        DuplicateDeduplicationKey = 10,
        NotificationNotFound = 11,
        AlreadyAcknowledged = 12,
        AlreadyExpired = 13,
        InvalidSnapshot = 14,
        DuplicateSnapshotEntry = 15,
    }

    /// <summary>通知队列操作类型。</summary>
    public enum D22NotificationOperation
    {
        None = 0,
        Enqueue = 1,
        Acknowledge = 2,
        Expire = 3,
        Restore = 4,
    }

    /// <summary>领域 owner 发来的结构化通知事件；不含用户可见硬编码文案。</summary>
    [Serializable]
    public sealed class D22NotificationEvent
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string EventId = string.Empty;
        public string DeduplicationKey = string.Empty;
        public string CorrelationId = string.Empty;
        public string SourceOwner = string.Empty;
        public string LocalizationKey = string.Empty;
        public string TitleLocalizationKey = string.Empty;
        public string BodyLocalizationKey = string.Empty;
        public D22NotificationPriority Priority = D22NotificationPriority.Normal;
        public int CreatedWorldDay;
        public int ExpiresWorldDay = -1;
        public bool RequiresAcknowledgement;
        public D22NotificationPersistence Persistence = D22NotificationPersistence.UntilAcknowledged;

        /// <summary>兼容“过期世界日”命名的稳定访问。</summary>
        public int ExpiryWorldDay
        {
            get { return ExpiresWorldDay; }
            set { ExpiresWorldDay = value; }
        }

        /// <summary>兼容通知 ID 命名；事件 ID 仍是幂等主键。</summary>
        public string NotificationId
        {
            get { return EventId; }
            set { EventId = value ?? string.Empty; }
        }

        /// <summary>兼容存档策略命名。</summary>
        public D22NotificationPersistence SavePolicy
        {
            get { return Persistence; }
            set { Persistence = value; }
        }

        /// <summary>返回防御性副本。</summary>
        public D22NotificationEvent Clone()
        {
            return new D22NotificationEvent
            {
                SchemaVersion = SchemaVersion,
                EventId = EventId ?? string.Empty,
                DeduplicationKey = DeduplicationKey ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                SourceOwner = SourceOwner ?? string.Empty,
                LocalizationKey = LocalizationKey ?? string.Empty,
                TitleLocalizationKey = TitleLocalizationKey ?? string.Empty,
                BodyLocalizationKey = BodyLocalizationKey ?? string.Empty,
                Priority = Priority,
                CreatedWorldDay = CreatedWorldDay,
                ExpiresWorldDay = ExpiresWorldDay,
                RequiresAcknowledgement = RequiresAcknowledgement,
                Persistence = Persistence,
            };
        }

        /// <summary>计算不依赖时钟的事件指纹。</summary>
        public string Fingerprint()
        {
            return D22NotificationQueue.ComputeFingerprint(this);
        }
    }

    /// <summary>队列内的通知记录；Sequence 只表达确定性入队顺序。</summary>
    [Serializable]
    public sealed class D22NotificationRecord
    {
        public int SchemaVersion = D22NotificationEvent.CurrentSchemaVersion;
        public string EventId = string.Empty;
        public string DeduplicationKey = string.Empty;
        public string CorrelationId = string.Empty;
        public string SourceOwner = string.Empty;
        public string LocalizationKey = string.Empty;
        public string TitleLocalizationKey = string.Empty;
        public string BodyLocalizationKey = string.Empty;
        public D22NotificationPriority Priority = D22NotificationPriority.Normal;
        public int CreatedWorldDay;
        public int ExpiresWorldDay = -1;
        public bool RequiresAcknowledgement;
        public D22NotificationPersistence Persistence = D22NotificationPersistence.UntilAcknowledged;
        public D22NotificationState State = D22NotificationState.Pending;
        public long Sequence;
        public int AcknowledgedWorldDay = -1;

        /// <summary>当前是否仍可展示/确认。</summary>
        public bool IsActive
        {
            get { return State == D22NotificationState.Pending; }
        }

        /// <summary>返回防御性副本。</summary>
        public D22NotificationRecord Clone()
        {
            return new D22NotificationRecord
            {
                SchemaVersion = SchemaVersion,
                EventId = EventId ?? string.Empty,
                DeduplicationKey = DeduplicationKey ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                SourceOwner = SourceOwner ?? string.Empty,
                LocalizationKey = LocalizationKey ?? string.Empty,
                TitleLocalizationKey = TitleLocalizationKey ?? string.Empty,
                BodyLocalizationKey = BodyLocalizationKey ?? string.Empty,
                Priority = Priority,
                CreatedWorldDay = CreatedWorldDay,
                ExpiresWorldDay = ExpiresWorldDay,
                RequiresAcknowledgement = RequiresAcknowledgement,
                Persistence = Persistence,
                State = State,
                Sequence = Sequence,
                AcknowledgedWorldDay = AcknowledgedWorldDay,
            };
        }

        /// <summary>按显式世界日判断是否到期。</summary>
        public bool IsExpiredAt(int worldDay)
        {
            return ExpiresWorldDay >= 0 && worldDay >= ExpiresWorldDay;
        }
    }

    /// <summary>保存后仍需保留的事件幂等索引。</summary>
    [Serializable]
    public sealed class D22ProcessedNotificationEvent
    {
        public string EventId = string.Empty;
        public string Fingerprint = string.Empty;
        public string DeduplicationKey = string.Empty;
        public string ExistingNotificationId = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22ProcessedNotificationEvent Clone()
        {
            return new D22ProcessedNotificationEvent
            {
                EventId = EventId ?? string.Empty,
                Fingerprint = Fingerprint ?? string.Empty,
                DeduplicationKey = DeduplicationKey ?? string.Empty,
                ExistingNotificationId = ExistingNotificationId ?? string.Empty,
            };
        }
    }

    /// <summary>通知队列完整状态与存档过滤后的 UI 投影。</summary>
    [Serializable]
    public sealed class D22NotificationQueueSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public long NextSequence = 1;
        public List<D22NotificationRecord> Notifications = new List<D22NotificationRecord>();
        public List<D22ProcessedNotificationEvent> ProcessedEvents = new List<D22ProcessedNotificationEvent>();

        /// <summary>兼容 Items 命名。</summary>
        public List<D22NotificationRecord> Items
        {
            get { return Notifications; }
            set { Notifications = value ?? new List<D22NotificationRecord>(); }
        }

        /// <summary>返回深拷贝。</summary>
        public D22NotificationQueueSnapshot Clone()
        {
            var result = new D22NotificationQueueSnapshot
            {
                SchemaVersion = SchemaVersion,
                NextSequence = NextSequence,
            };
            if (Notifications != null)
                foreach (var item in Notifications) result.Notifications.Add(item == null ? null : item.Clone());
            if (ProcessedEvents != null)
                foreach (var item in ProcessedEvents) result.ProcessedEvents.Add(item == null ? null : item.Clone());
            return result;
        }
    }

    /// <summary>通知队列操作结果。</summary>
    [Serializable]
    public sealed class D22NotificationQueueResult
    {
        public int SchemaVersion = D22NotificationQueueSnapshot.CurrentSchemaVersion;
        public D22NotificationOperation Operation;
        public bool Accepted;
        public bool Changed;
        public bool NoChange;
        public bool Duplicate;
        public bool Conflict;
        public D22NotificationFailureCode FailureCode;
        public string ReasonCode = string.Empty;
        public string EventId = string.Empty;
        public string NotificationId = string.Empty;
        public string ExistingNotificationId = string.Empty;
        public int AffectedCount;
        public D22NotificationRecord Record;
        public D22NotificationQueueSnapshot Snapshot;

        /// <summary>返回防御性副本。</summary>
        public D22NotificationQueueResult Clone()
        {
            return new D22NotificationQueueResult
            {
                SchemaVersion = SchemaVersion,
                Operation = Operation,
                Accepted = Accepted,
                Changed = Changed,
                NoChange = NoChange,
                Duplicate = Duplicate,
                Conflict = Conflict,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                EventId = EventId ?? string.Empty,
                NotificationId = NotificationId ?? string.Empty,
                ExistingNotificationId = ExistingNotificationId ?? string.Empty,
                AffectedCount = AffectedCount,
                Record = Record == null ? null : Record.Clone(),
                Snapshot = Snapshot == null ? null : Snapshot.Clone(),
            };
        }
    }

    /// <summary>批量到期操作结果。</summary>
    [Serializable]
    public sealed class D22NotificationExpiryResult
    {
        public bool Accepted;
        public bool Changed;
        public int WorldDay;
        public List<string> ExpiredNotificationIds = new List<string>();

        /// <summary>返回防御性副本。</summary>
        public D22NotificationExpiryResult Clone()
        {
            var result = new D22NotificationExpiryResult
            {
                Accepted = Accepted,
                Changed = Changed,
                WorldDay = WorldDay,
            };
            if (ExpiredNotificationIds != null) result.ExpiredNotificationIds.AddRange(ExpiredNotificationIds);
            return result;
        }
    }

    /// <summary>
    /// D22-L13 通知队列。它拥有 presentation notification 的生命周期，
    /// 但不拥有领域事件、奖励或任何业务事实。
    /// </summary>
    public sealed class D22NotificationQueue
    {
        public const int CurrentSchemaVersion = D22NotificationQueueSnapshot.CurrentSchemaVersion;
        public const string OwnerSystem = "D22NotificationQueue";

        private readonly List<D22NotificationRecord> notifications = new List<D22NotificationRecord>();
        private readonly List<D22ProcessedNotificationEvent> processedEvents = new List<D22ProcessedNotificationEvent>();
        private long nextSequence = 1;

        /// <summary>返回完整队列快照的防御性副本。</summary>
        public D22NotificationQueueSnapshot Snapshot()
        {
            return CreateSnapshot();
        }

        /// <summary>按优先级降序、入队序升序返回当前活动通知。</summary>
        public IReadOnlyList<D22NotificationRecord> GetActive(int worldDay = -1)
        {
            if (worldDay >= 0) Expire(worldDay);
            var result = new List<D22NotificationRecord>();
            foreach (var notification in notifications)
                if (notification != null && notification.IsActive) result.Add(notification.Clone());
            result.Sort(CompareForDisplay);
            return new ReadOnlyCollection<D22NotificationRecord>(result);
        }

        /// <summary>取得下一条确定性通知；没有活动项时返回空。</summary>
        public D22NotificationRecord PeekNext(int worldDay = -1)
        {
            if (worldDay >= 0) Expire(worldDay);
            D22NotificationRecord best = null;
            foreach (var notification in notifications)
            {
                if (notification == null || !notification.IsActive) continue;
                if (best == null || CompareForDisplay(notification, best) < 0) best = notification;
            }
            return best == null ? null : best.Clone();
        }

        /// <summary>按事件 ID 幂等入队，并按活动去重键防止重复提示。</summary>
        public D22NotificationQueueResult Enqueue(D22NotificationEvent notification, int worldDay)
        {
            D22NotificationFailureCode validation = ValidateEvent(notification, worldDay);
            if (validation != D22NotificationFailureCode.None)
                return Reject(D22NotificationOperation.Enqueue, validation, ReasonFor(validation), notification == null ? "" : notification.EventId);

            string fingerprint = notification.Fingerprint();
            D22NotificationRecord existing = FindByEventId(notification.EventId);
            if (existing != null)
            {
                if (string.Equals(Fingerprint(existing), fingerprint, StringComparison.Ordinal))
                    return Duplicate(notification.EventId, existing.EventId, D22NotificationFailureCode.DuplicateEvent,
                        "d22.notification.duplicate_event", existing);
                return new D22NotificationQueueResult
                {
                    Operation = D22NotificationOperation.Enqueue,
                    Accepted = false,
                    Conflict = true,
                    FailureCode = D22NotificationFailureCode.DuplicateEventConflict,
                    ReasonCode = "d22.notification.duplicate_event_conflict",
                    EventId = notification.EventId ?? string.Empty,
                    ExistingNotificationId = existing.EventId ?? string.Empty,
                };
            }

            if (!string.IsNullOrEmpty(notification.DeduplicationKey))
            {
                D22NotificationRecord activeDuplicate = FindActiveByDeduplicationKey(notification.DeduplicationKey);
                if (activeDuplicate != null)
                {
                    AddProcessedEvent(notification, activeDuplicate.EventId);
                    return Duplicate(notification.EventId, activeDuplicate.EventId,
                        D22NotificationFailureCode.DuplicateDeduplicationKey,
                        "d22.notification.duplicate_deduplication_key", activeDuplicate);
                }
            }

            var record = new D22NotificationRecord
            {
                EventId = notification.EventId,
                DeduplicationKey = string.IsNullOrEmpty(notification.DeduplicationKey)
                    ? notification.EventId
                    : notification.DeduplicationKey,
                CorrelationId = notification.CorrelationId ?? string.Empty,
                SourceOwner = notification.SourceOwner ?? string.Empty,
                LocalizationKey = notification.LocalizationKey ?? string.Empty,
                TitleLocalizationKey = notification.TitleLocalizationKey ?? string.Empty,
                BodyLocalizationKey = notification.BodyLocalizationKey ?? string.Empty,
                Priority = notification.Priority,
                CreatedWorldDay = notification.CreatedWorldDay,
                ExpiresWorldDay = notification.ExpiresWorldDay,
                RequiresAcknowledgement = notification.RequiresAcknowledgement,
                Persistence = notification.Persistence,
                State = D22NotificationState.Pending,
                Sequence = nextSequence++,
                AcknowledgedWorldDay = -1,
            };
            notifications.Add(record);
            AddProcessedEvent(notification, record.EventId);
            return new D22NotificationQueueResult
            {
                Operation = D22NotificationOperation.Enqueue,
                Accepted = true,
                Changed = true,
                FailureCode = D22NotificationFailureCode.None,
                ReasonCode = "d22.notification.enqueued",
                EventId = record.EventId,
                NotificationId = record.EventId,
                Record = record.Clone(),
            };
        }

        /// <summary>以事件自身 CreatedWorldDay 作为显式校验日的便捷入口。</summary>
        public D22NotificationQueueResult Enqueue(D22NotificationEvent notification)
        {
            return Enqueue(notification, notification == null ? 0 : notification.CreatedWorldDay);
        }

        /// <summary>确认一条通知；确认是 presentation 状态，不改变领域事件。</summary>
        public D22NotificationQueueResult Acknowledge(string notificationId, int worldDay)
        {
            if (string.IsNullOrEmpty(notificationId))
                return Reject(D22NotificationOperation.Acknowledge, D22NotificationFailureCode.NotificationNotFound,
                    "d22.notification.missing_notification", notificationId);
            if (worldDay < 0)
                return Reject(D22NotificationOperation.Acknowledge, D22NotificationFailureCode.InvalidWorldDay,
                    "d22.notification.invalid_world_day", notificationId);

            D22NotificationRecord record = FindByEventId(notificationId);
            if (record == null)
                return Reject(D22NotificationOperation.Acknowledge, D22NotificationFailureCode.NotificationNotFound,
                    "d22.notification.not_found", notificationId);
            if (record.IsActive && record.IsExpiredAt(worldDay))
                record.State = D22NotificationState.Expired;
            if (record.State == D22NotificationState.Acknowledged)
                return NoChange(D22NotificationOperation.Acknowledge, D22NotificationFailureCode.AlreadyAcknowledged,
                    "d22.notification.already_acknowledged", record);
            if (record.State == D22NotificationState.Expired)
                return Reject(D22NotificationOperation.Acknowledge, D22NotificationFailureCode.AlreadyExpired,
                    "d22.notification.already_expired", notificationId);

            record.State = D22NotificationState.Acknowledged;
            record.AcknowledgedWorldDay = worldDay;
            return new D22NotificationQueueResult
            {
                Operation = D22NotificationOperation.Acknowledge,
                Accepted = true,
                Changed = true,
                FailureCode = D22NotificationFailureCode.None,
                ReasonCode = "d22.notification.acknowledged",
                EventId = record.EventId,
                NotificationId = record.EventId,
                Record = record.Clone(),
            };
        }

        /// <summary>Confirm 是 Acknowledge 的语义别名。</summary>
        public D22NotificationQueueResult Confirm(string notificationId, int worldDay)
        {
            return Acknowledge(notificationId, worldDay);
        }

        /// <summary>按显式世界日标记已到期通知；不使用系统时钟。</summary>
        public D22NotificationExpiryResult Expire(int worldDay)
        {
            var result = new D22NotificationExpiryResult
            {
                Accepted = worldDay >= 0,
                Changed = false,
                WorldDay = worldDay,
            };
            if (worldDay < 0) return result;
            foreach (var notification in notifications)
            {
                if (notification == null || !notification.IsActive || !notification.IsExpiredAt(worldDay)) continue;
                notification.State = D22NotificationState.Expired;
                result.Changed = true;
                result.ExpiredNotificationIds.Add(notification.EventId ?? string.Empty);
            }
            return result;
        }

        /// <summary>
        /// 生成符合 Persistence 策略的 UI 存档投影。传入 worldDay 时先执行同一到期规则。
        /// </summary>
        public D22NotificationQueueSnapshot CreateSaveSnapshot(int worldDay = -1)
        {
            if (worldDay >= 0) Expire(worldDay);
            var result = new D22NotificationQueueSnapshot
            {
                NextSequence = nextSequence,
            };
            var persistedIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var notification in notifications)
            {
                if (notification == null || !ShouldPersist(notification)) continue;
                result.Notifications.Add(notification.Clone());
                persistedIds.Add(notification.EventId ?? string.Empty);
            }
            foreach (var processed in processedEvents)
            {
                if (processed == null || !persistedIds.Contains(processed.ExistingNotificationId)) continue;
                result.ProcessedEvents.Add(processed.Clone());
            }
            return result;
        }

        /// <summary>从 UI 存档投影恢复队列；恢复过程再次校验稳定 ID、版本和重复项。</summary>
        public D22NotificationQueueResult Restore(D22NotificationQueueSnapshot snapshot, int worldDay = -1)
        {
            if (snapshot == null)
                return Reject(D22NotificationOperation.Restore, D22NotificationFailureCode.InvalidSnapshot,
                    "d22.notification.missing_snapshot", string.Empty);
            if (snapshot.SchemaVersion != CurrentSchemaVersion)
                return Reject(D22NotificationOperation.Restore, D22NotificationFailureCode.InvalidSnapshot,
                    "d22.notification.invalid_snapshot_schema", string.Empty);
            if (snapshot.Notifications == null || snapshot.ProcessedEvents == null || snapshot.NextSequence < 1)
                return Reject(D22NotificationOperation.Restore, D22NotificationFailureCode.InvalidSnapshot,
                    "d22.notification.invalid_snapshot", string.Empty);

            var restored = new List<D22NotificationRecord>();
            var seenEventIds = new HashSet<string>(StringComparer.Ordinal);
            var seenActiveKeys = new HashSet<string>(StringComparer.Ordinal);
            long maximumSequence = 0;
            foreach (var candidate in snapshot.Notifications)
            {
                if (!ValidateRecord(candidate))
                    return Reject(D22NotificationOperation.Restore, D22NotificationFailureCode.InvalidSnapshot,
                        "d22.notification.invalid_snapshot_record", candidate == null ? "" : candidate.EventId);
                if (!seenEventIds.Add(candidate.EventId))
                    return Reject(D22NotificationOperation.Restore, D22NotificationFailureCode.DuplicateSnapshotEntry,
                        "d22.notification.duplicate_snapshot_entry", candidate.EventId);
                if (candidate.IsActive && !seenActiveKeys.Add(candidate.DeduplicationKey))
                    return Reject(D22NotificationOperation.Restore, D22NotificationFailureCode.DuplicateSnapshotEntry,
                        "d22.notification.duplicate_snapshot_key", candidate.EventId);
                var clone = candidate.Clone();
                if (worldDay >= 0 && clone.IsActive && clone.IsExpiredAt(worldDay))
                    clone.State = D22NotificationState.Expired;
                restored.Add(clone);
                if (clone.Sequence > maximumSequence) maximumSequence = clone.Sequence;
            }

            var seenProcessedEventIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var processed in snapshot.ProcessedEvents)
            {
                if (processed == null || string.IsNullOrEmpty(processed.EventId) ||
                    string.IsNullOrEmpty(processed.Fingerprint) || !seenProcessedEventIds.Add(processed.EventId) ||
                    FindSnapshotNotification(snapshot.Notifications, processed.ExistingNotificationId) == null)
                    return Reject(D22NotificationOperation.Restore, D22NotificationFailureCode.InvalidSnapshot,
                        "d22.notification.invalid_processed_event", string.Empty);
            }
            notifications.Clear();
            processedEvents.Clear();
            notifications.AddRange(restored);
            foreach (var processed in snapshot.ProcessedEvents)
                processedEvents.Add(processed.Clone());
            nextSequence = Math.Max(snapshot.NextSequence, maximumSequence + 1);
            return new D22NotificationQueueResult
            {
                Operation = D22NotificationOperation.Restore,
                Accepted = true,
                Changed = true,
                FailureCode = D22NotificationFailureCode.None,
                ReasonCode = "d22.notification.restored",
                AffectedCount = notifications.Count,
                Snapshot = Snapshot(),
            };
        }

        /// <summary>以快照创建新队列实例。</summary>
        public static D22NotificationQueue FromSnapshot(D22NotificationQueueSnapshot snapshot, int worldDay = -1)
        {
            var queue = new D22NotificationQueue();
            D22NotificationQueueResult result = queue.Restore(snapshot, worldDay);
            if (!result.Accepted) return null;
            return queue;
        }

        /// <summary>事件指纹为稳定字段串，不含序列或运行时状态。</summary>
        public static string ComputeFingerprint(D22NotificationEvent notification)
        {
            if (notification == null) return string.Empty;
            var builder = new StringBuilder();
            Append(builder, notification.SchemaVersion);
            Append(builder, notification.EventId);
            Append(builder, notification.DeduplicationKey);
            Append(builder, notification.CorrelationId);
            Append(builder, notification.SourceOwner);
            Append(builder, notification.LocalizationKey);
            Append(builder, notification.TitleLocalizationKey);
            Append(builder, notification.BodyLocalizationKey);
            Append(builder, (int)notification.Priority);
            Append(builder, notification.CreatedWorldDay);
            Append(builder, notification.ExpiresWorldDay);
            Append(builder, notification.RequiresAcknowledgement ? 1 : 0);
            Append(builder, (int)notification.Persistence);
            return builder.ToString();
        }

        private static string Fingerprint(D22NotificationRecord record)
        {
            return ComputeFingerprint(new D22NotificationEvent
            {
                SchemaVersion = record.SchemaVersion,
                EventId = record.EventId,
                DeduplicationKey = record.DeduplicationKey,
                CorrelationId = record.CorrelationId,
                SourceOwner = record.SourceOwner,
                LocalizationKey = record.LocalizationKey,
                TitleLocalizationKey = record.TitleLocalizationKey,
                BodyLocalizationKey = record.BodyLocalizationKey,
                Priority = record.Priority,
                CreatedWorldDay = record.CreatedWorldDay,
                ExpiresWorldDay = record.ExpiresWorldDay,
                RequiresAcknowledgement = record.RequiresAcknowledgement,
                Persistence = record.Persistence,
            });
        }

        private D22NotificationQueueSnapshot CreateSnapshot()
        {
            var result = new D22NotificationQueueSnapshot
            {
                NextSequence = nextSequence,
            };
            foreach (var notification in notifications)
                if (notification != null) result.Notifications.Add(notification.Clone());
            foreach (var processed in processedEvents)
                if (processed != null) result.ProcessedEvents.Add(processed.Clone());
            return result;
        }

        private D22NotificationRecord FindByEventId(string eventId)
        {
            foreach (var notification in notifications)
                if (notification != null && string.Equals(notification.EventId, eventId, StringComparison.Ordinal)) return notification;
            return null;
        }

        private static D22NotificationRecord FindSnapshotNotification(
            IList<D22NotificationRecord> snapshotNotifications,
            string eventId)
        {
            if (snapshotNotifications == null || string.IsNullOrEmpty(eventId)) return null;
            foreach (var notification in snapshotNotifications)
                if (notification != null && string.Equals(notification.EventId, eventId, StringComparison.Ordinal)) return notification;
            return null;
        }

        private D22NotificationRecord FindActiveByDeduplicationKey(string key)
        {
            foreach (var notification in notifications)
                if (notification != null && notification.IsActive &&
                    string.Equals(notification.DeduplicationKey, key, StringComparison.Ordinal)) return notification;
            return null;
        }

        private void AddProcessedEvent(D22NotificationEvent notification, string existingNotificationId)
        {
            foreach (var processed in processedEvents)
            {
                if (processed != null && string.Equals(processed.EventId, notification.EventId, StringComparison.Ordinal)) return;
            }
            processedEvents.Add(new D22ProcessedNotificationEvent
            {
                EventId = notification.EventId ?? string.Empty,
                Fingerprint = notification.Fingerprint(),
                DeduplicationKey = notification.DeduplicationKey ?? string.Empty,
                ExistingNotificationId = existingNotificationId ?? string.Empty,
            });
        }

        private static bool ShouldPersist(D22NotificationRecord notification)
        {
            switch (notification.Persistence)
            {
                case D22NotificationPersistence.Transient:
                    return false;
                case D22NotificationPersistence.UntilAcknowledged:
                    return notification.State != D22NotificationState.Acknowledged &&
                           notification.State != D22NotificationState.Expired;
                case D22NotificationPersistence.UntilExpired:
                    return notification.State != D22NotificationState.Expired;
                case D22NotificationPersistence.Always:
                    return true;
                default:
                    return false;
            }
        }

        private static int CompareForDisplay(D22NotificationRecord first, D22NotificationRecord second)
        {
            int priority = ((int)second.Priority).CompareTo((int)first.Priority);
            if (priority != 0) return priority;
            int sequence = first.Sequence.CompareTo(second.Sequence);
            if (sequence != 0) return sequence;
            return string.Compare(first.EventId, second.EventId, StringComparison.Ordinal);
        }

        private static D22NotificationFailureCode ValidateEvent(D22NotificationEvent notification, int worldDay)
        {
            if (notification == null) return D22NotificationFailureCode.MissingEvent;
            if (notification.SchemaVersion != D22NotificationEvent.CurrentSchemaVersion)
                return D22NotificationFailureCode.InvalidSchemaVersion;
            if (!D22ScreenContracts.IsStableId(notification.EventId))
                return D22NotificationFailureCode.InvalidEventId;
            if (!string.IsNullOrEmpty(notification.DeduplicationKey) &&
                !D22ScreenContracts.IsStableId(notification.DeduplicationKey))
                return D22NotificationFailureCode.InvalidDeduplicationKey;
            if (!IsLocalizationKey(notification.LocalizationKey) ||
                (!string.IsNullOrEmpty(notification.TitleLocalizationKey) && !IsLocalizationKey(notification.TitleLocalizationKey)) ||
                (!string.IsNullOrEmpty(notification.BodyLocalizationKey) && !IsLocalizationKey(notification.BodyLocalizationKey)))
                return D22NotificationFailureCode.InvalidLocalizationKey;
            if (notification.Priority < D22NotificationPriority.Low || notification.Priority > D22NotificationPriority.Critical)
                return D22NotificationFailureCode.InvalidPriority;
            if (notification.CreatedWorldDay < 0 || worldDay < 0 || notification.CreatedWorldDay > worldDay ||
                (notification.ExpiresWorldDay < -1) ||
                (notification.ExpiresWorldDay >= 0 && notification.ExpiresWorldDay < notification.CreatedWorldDay))
                return D22NotificationFailureCode.InvalidWorldDay;
            return D22NotificationFailureCode.None;
        }

        private static bool ValidateRecord(D22NotificationRecord record)
        {
            if (record == null || record.SchemaVersion != D22NotificationRecordSchemaVersion() ||
                !D22ScreenContracts.IsStableId(record.EventId) ||
                !D22ScreenContracts.IsStableId(record.DeduplicationKey) ||
                !IsLocalizationKey(record.LocalizationKey) ||
                (!string.IsNullOrEmpty(record.TitleLocalizationKey) && !IsLocalizationKey(record.TitleLocalizationKey)) ||
                (!string.IsNullOrEmpty(record.BodyLocalizationKey) && !IsLocalizationKey(record.BodyLocalizationKey)) ||
                record.Priority < D22NotificationPriority.Low || record.Priority > D22NotificationPriority.Critical ||
                record.CreatedWorldDay < 0 || record.ExpiresWorldDay < -1 ||
                (record.ExpiresWorldDay >= 0 && record.ExpiresWorldDay < record.CreatedWorldDay) ||
                record.Sequence < 1 || record.AcknowledgedWorldDay < -1)
                return false;
            return record.State >= D22NotificationState.Pending && record.State <= D22NotificationState.Dismissed;
        }

        private static int D22NotificationRecordSchemaVersion()
        {
            return D22NotificationEvent.CurrentSchemaVersion;
        }

        private static bool IsLocalizationKey(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.StartsWith("ui.", StringComparison.Ordinal)) return false;
            return D22ScreenContracts.IsStableId(value);
        }

        private static void Append(StringBuilder builder, string value)
        {
            builder.Append(value ?? string.Empty).Append('|');
        }

        private static void Append(StringBuilder builder, int value)
        {
            builder.Append(value).Append('|');
        }

        private static D22NotificationQueueResult Duplicate(
            string eventId,
            string notificationId,
            D22NotificationFailureCode code,
            string reasonCode,
            D22NotificationRecord record)
        {
            return new D22NotificationQueueResult
            {
                Operation = D22NotificationOperation.Enqueue,
                Accepted = true,
                Changed = false,
                NoChange = true,
                Duplicate = true,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                EventId = eventId ?? string.Empty,
                NotificationId = notificationId ?? string.Empty,
                ExistingNotificationId = notificationId ?? string.Empty,
                Record = record == null ? null : record.Clone(),
            };
        }

        private static D22NotificationQueueResult NoChange(
            D22NotificationOperation operation,
            D22NotificationFailureCode code,
            string reasonCode,
            D22NotificationRecord record)
        {
            return new D22NotificationQueueResult
            {
                Operation = operation,
                Accepted = true,
                Changed = false,
                NoChange = true,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                NotificationId = record == null ? string.Empty : record.EventId ?? string.Empty,
                Record = record == null ? null : record.Clone(),
            };
        }

        private static D22NotificationQueueResult Reject(
            D22NotificationOperation operation,
            D22NotificationFailureCode code,
            string reasonCode,
            string eventId)
        {
            return new D22NotificationQueueResult
            {
                Operation = operation,
                Accepted = false,
                Changed = false,
                NoChange = false,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                EventId = eventId ?? string.Empty,
            };
        }

        private static string ReasonFor(D22NotificationFailureCode code)
        {
            switch (code)
            {
                case D22NotificationFailureCode.MissingEvent: return "d22.notification.missing_event";
                case D22NotificationFailureCode.InvalidSchemaVersion: return "d22.notification.invalid_schema_version";
                case D22NotificationFailureCode.InvalidEventId: return "d22.notification.invalid_event_id";
                case D22NotificationFailureCode.InvalidDeduplicationKey: return "d22.notification.invalid_deduplication_key";
                case D22NotificationFailureCode.InvalidLocalizationKey: return "d22.notification.invalid_localization_key";
                case D22NotificationFailureCode.InvalidPriority: return "d22.notification.invalid_priority";
                case D22NotificationFailureCode.InvalidWorldDay: return "d22.notification.invalid_world_day";
                default: return "d22.notification.invalid_event";
            }
        }
    }
}
