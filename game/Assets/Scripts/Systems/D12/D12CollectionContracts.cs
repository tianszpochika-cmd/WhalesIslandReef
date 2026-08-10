// ============================================================
// D12CollectionContracts —— 统一收藏/图鉴快照与输入契约
//
// D12CollectionAtlasSystem 只写自己的图鉴状态和成就事实。动物、植物、
// 鱼获、船、外观、地图、任务和武器的领域系统仍是各自状态的唯一写者。
// 收藏证据必须引用已经进入 EventLog 的事实；字符串描述本身不能解锁。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public enum D12CollectionCategory
    {
        Animal,
        Plant,
        Fish,
        Ship,
        Clothing,
        Map,
        HistoricalFile,
        Treasure,
        SpecialWeapon,
    }

    public enum D12CollectionAcquisitionMethod
    {
        Observation,
        Harvest,
        Catch,
        Purchase,
        Gift,
        Salvage,
        QuestReward,
        MapDiscovery,
        Transfer,
        Crafted,
        AuthorizedArchive,
        AuthorizedArmory,
    }

    [Serializable]
    public sealed class D12CollectionEntryDefinition
    {
        public string ContentId;
        public D12CollectionCategory Category;
        public string DisplayNameKey;
        public string SourceRef;
        public string AchievementId;
        public bool RequiresAuthorization;
        public string RequiredAuthorizationRef;
        public List<D12CollectionAcquisitionMethod> AllowedAcquisitionMethods =
            new List<D12CollectionAcquisitionMethod>();

        public D12CollectionEntryDefinition Clone()
        {
            var result = new D12CollectionEntryDefinition
            {
                ContentId = ContentId,
                Category = Category,
                DisplayNameKey = DisplayNameKey,
                SourceRef = SourceRef,
                AchievementId = AchievementId,
                RequiresAuthorization = RequiresAuthorization,
                RequiredAuthorizationRef = RequiredAuthorizationRef,
            };
            if (AllowedAcquisitionMethods != null)
                result.AllowedAcquisitionMethods.AddRange(AllowedAcquisitionMethods);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ContentId)) throw new InvalidOperationException("收藏定义缺少 content_id。");
            if (!Enum.IsDefined(typeof(D12CollectionCategory), Category)) throw new InvalidOperationException("收藏类别非法：" + ContentId);
            if (string.IsNullOrWhiteSpace(DisplayNameKey)) throw new InvalidOperationException("收藏定义缺少 display_name_key：" + ContentId);
            if (string.IsNullOrWhiteSpace(SourceRef)) throw new InvalidOperationException("收藏定义缺少 source_ref：" + ContentId);
            if (string.IsNullOrWhiteSpace(AchievementId)) throw new InvalidOperationException("收藏定义缺少 achievement_id：" + ContentId);
            if (RequiresAuthorization && string.IsNullOrWhiteSpace(RequiredAuthorizationRef))
                throw new InvalidOperationException("需要授权的收藏定义缺少 required_authorization_ref：" + ContentId);
            if (AllowedAcquisitionMethods == null || AllowedAcquisitionMethods.Count == 0)
                throw new InvalidOperationException("收藏定义缺少允许的获得方式：" + ContentId);
            foreach (var method in AllowedAcquisitionMethods)
                if (!Enum.IsDefined(typeof(D12CollectionAcquisitionMethod), method))
                    throw new InvalidOperationException("收藏获得方式非法：" + ContentId);
        }
    }

    [Serializable]
    public sealed class D12CollectionEvidenceRecord
    {
        public string EvidenceId;
        public string ContentId;
        public D12CollectionCategory Category;
        public string FactEventId;
        public long FactSaveRevision;
        public string SourceRef;
        public string EvidenceRef;
        public string LocationRef;
        public int WorldDay;
        public D12CollectionAcquisitionMethod AcquisitionMethod;
        public string AuthorizationRef;
        public string AuthorizationFactEventId;
        public long AuthorizationSaveRevision;
        public string RecordedEventId;
        public long RecordedSaveRevision;

        public D12CollectionEvidenceRecord Clone()
        {
            return new D12CollectionEvidenceRecord
            {
                EvidenceId = EvidenceId,
                ContentId = ContentId,
                Category = Category,
                FactEventId = FactEventId,
                FactSaveRevision = FactSaveRevision,
                SourceRef = SourceRef,
                EvidenceRef = EvidenceRef,
                LocationRef = LocationRef,
                WorldDay = WorldDay,
                AcquisitionMethod = AcquisitionMethod,
                AuthorizationRef = AuthorizationRef,
                AuthorizationFactEventId = AuthorizationFactEventId,
                AuthorizationSaveRevision = AuthorizationSaveRevision,
                RecordedEventId = RecordedEventId,
                RecordedSaveRevision = RecordedSaveRevision,
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(EvidenceId)) throw new InvalidOperationException("图鉴证据缺少 evidence_id。");
            if (string.IsNullOrWhiteSpace(ContentId)) throw new InvalidOperationException("图鉴证据缺少 content_id：" + EvidenceId);
            if (string.IsNullOrWhiteSpace(FactEventId)) throw new InvalidOperationException("图鉴证据缺少事实事件：" + EvidenceId);
            if (FactSaveRevision < 0) throw new InvalidOperationException("图鉴事实 save_revision 非法：" + EvidenceId);
            if (string.IsNullOrWhiteSpace(SourceRef)) throw new InvalidOperationException("图鉴证据缺少来源：" + EvidenceId);
            if (string.IsNullOrWhiteSpace(EvidenceRef)) throw new InvalidOperationException("图鉴证据缺少 evidence_ref：" + EvidenceId);
            if (string.IsNullOrWhiteSpace(LocationRef)) throw new InvalidOperationException("图鉴证据缺少 location_ref：" + EvidenceId);
            if (WorldDay < 0) throw new InvalidOperationException("图鉴证据 world_day 非法：" + EvidenceId);
            if (!Enum.IsDefined(typeof(D12CollectionAcquisitionMethod), AcquisitionMethod)) throw new InvalidOperationException("图鉴获得方式非法：" + EvidenceId);
            if (string.IsNullOrWhiteSpace(RecordedEventId)) throw new InvalidOperationException("图鉴证据缺少 recorded_event_id：" + EvidenceId);
            if (RecordedSaveRevision < 0) throw new InvalidOperationException("图鉴记录 save_revision 非法：" + EvidenceId);
        }
    }

    [Serializable]
    public sealed class D12CollectionRecord
    {
        public string CollectionId;
        public string ContentId;
        public D12CollectionCategory Category;
        public bool Unlocked;
        public string FirstEvidenceId;
        public string FirstLocationRef;
        public int FirstWorldDay;
        public D12CollectionAcquisitionMethod FirstAcquisitionMethod;
        public List<string> EvidenceIds = new List<string>();
        public string AchievementId;
        public bool AchievementUnlocked;
        public string AchievementEventId;
        public long Revision;

        public D12CollectionRecord Clone()
        {
            var result = new D12CollectionRecord
            {
                CollectionId = CollectionId,
                ContentId = ContentId,
                Category = Category,
                Unlocked = Unlocked,
                FirstEvidenceId = FirstEvidenceId,
                FirstLocationRef = FirstLocationRef,
                FirstWorldDay = FirstWorldDay,
                FirstAcquisitionMethod = FirstAcquisitionMethod,
                AchievementId = AchievementId,
                AchievementUnlocked = AchievementUnlocked,
                AchievementEventId = AchievementEventId,
                Revision = Revision,
            };
            if (EvidenceIds != null) result.EvidenceIds.AddRange(EvidenceIds);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CollectionId)) throw new InvalidOperationException("图鉴记录缺少 collection_id。");
            if (string.IsNullOrWhiteSpace(ContentId)) throw new InvalidOperationException("图鉴记录缺少 content_id：" + CollectionId);
            if (!Enum.IsDefined(typeof(D12CollectionCategory), Category)) throw new InvalidOperationException("图鉴记录类别非法：" + CollectionId);
            if (!Unlocked) throw new InvalidOperationException("已写入图鉴的记录必须是 unlocked：" + CollectionId);
            if (string.IsNullOrWhiteSpace(FirstEvidenceId)) throw new InvalidOperationException("图鉴记录缺少首次证据：" + CollectionId);
            if (string.IsNullOrWhiteSpace(FirstLocationRef)) throw new InvalidOperationException("图鉴记录缺少首次地点：" + CollectionId);
            if (FirstWorldDay < 0) throw new InvalidOperationException("图鉴记录首次时间非法：" + CollectionId);
            if (!Enum.IsDefined(typeof(D12CollectionAcquisitionMethod), FirstAcquisitionMethod)) throw new InvalidOperationException("图鉴记录首次获得方式非法：" + CollectionId);
            if (EvidenceIds == null || EvidenceIds.Count == 0) throw new InvalidOperationException("图鉴记录必须引用证据：" + CollectionId);
            if (string.IsNullOrWhiteSpace(AchievementId)) throw new InvalidOperationException("图鉴记录缺少 achievement_id：" + CollectionId);
            if (!AchievementUnlocked || string.IsNullOrWhiteSpace(AchievementEventId)) throw new InvalidOperationException("图鉴记录缺少成就事实：" + CollectionId);
            if (Revision < 0) throw new InvalidOperationException("图鉴记录 revision 非法：" + CollectionId);
        }
    }

    [Serializable]
    public sealed class D12CollectionAchievement
    {
        public string AchievementId;
        public string ContentId;
        public bool Unlocked;
        public string EvidenceId;
        public string EventId;
        public int WorldDay;
        public long SaveRevision;

        public D12CollectionAchievement Clone()
        {
            return new D12CollectionAchievement
            {
                AchievementId = AchievementId,
                ContentId = ContentId,
                Unlocked = Unlocked,
                EvidenceId = EvidenceId,
                EventId = EventId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(AchievementId) || string.IsNullOrWhiteSpace(ContentId) || !Unlocked)
                throw new InvalidOperationException("收藏成就记录不完整：" + AchievementId);
            if (string.IsNullOrWhiteSpace(EvidenceId) || string.IsNullOrWhiteSpace(EventId)) throw new InvalidOperationException("收藏成就缺少事实引用：" + AchievementId);
            if (WorldDay < 0 || SaveRevision < 0) throw new InvalidOperationException("收藏成就时间或 revision 非法：" + AchievementId);
        }
    }

    [Serializable]
    public sealed class D12CollectionAtlasState
    {
        public const int CurrentSchemaVersion = 1;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public List<D12CollectionRecord> Records = new List<D12CollectionRecord>();
        public List<D12CollectionEvidenceRecord> Evidence = new List<D12CollectionEvidenceRecord>();
        public List<D12CollectionAchievement> Achievements = new List<D12CollectionAchievement>();
        public List<string> AppliedEventKeys = new List<string>();

        public D12CollectionAtlasState Clone()
        {
            var result = new D12CollectionAtlasState { SchemaVersion = SchemaVersion, Revision = Revision };
            if (Records != null) foreach (var item in Records) if (item != null) result.Records.Add(item.Clone());
            if (Evidence != null) foreach (var item in Evidence) if (item != null) result.Evidence.Add(item.Clone());
            if (Achievements != null) foreach (var item in Achievements) if (item != null) result.Achievements.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion) throw new InvalidOperationException("D12 图鉴 schema_version 不匹配：收到 " + SchemaVersion + "。");
            if (Revision < 0) throw new InvalidOperationException("D12 图鉴 revision 不能为负数。");
            var recordIds = new HashSet<string>(StringComparer.Ordinal);
            var recordEvidenceReferences = new Dictionary<string, string>(StringComparer.Ordinal);
            if (Records == null) throw new InvalidOperationException("D12 图鉴 records 不能为 null。");
            foreach (var record in Records)
            {
                if (record == null) throw new InvalidOperationException("D12 图鉴 records 不能包含 null。");
                record.Validate();
                if (!recordIds.Add(record.CollectionId)) throw new InvalidOperationException("D12 图鉴记录重复：" + record.CollectionId);
                var recordEvidenceIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var evidenceId in record.EvidenceIds)
                {
                    if (string.IsNullOrWhiteSpace(evidenceId) || !recordEvidenceIds.Add(evidenceId))
                        throw new InvalidOperationException("D12 图鉴记录证据引用重复或为空：" + record.CollectionId);
                    if (recordEvidenceReferences.ContainsKey(evidenceId))
                        throw new InvalidOperationException("D12 图鉴证据被多个记录引用：" + evidenceId);
                    recordEvidenceReferences.Add(evidenceId, record.ContentId);
                }
            }
            var evidenceIds = new HashSet<string>(StringComparer.Ordinal);
            if (Evidence == null) throw new InvalidOperationException("D12 图鉴 evidence 不能为 null。");
            foreach (var item in Evidence)
            {
                if (item == null) throw new InvalidOperationException("D12 图鉴 evidence 不能包含 null。");
                item.Validate();
                if (!evidenceIds.Add(item.EvidenceId)) throw new InvalidOperationException("D12 图鉴证据重复：" + item.EvidenceId);
                string recordContentId;
                if (!recordEvidenceReferences.TryGetValue(item.EvidenceId, out recordContentId) || recordContentId != item.ContentId)
                    throw new InvalidOperationException("D12 图鉴证据没有匹配的记录：" + item.EvidenceId);
            }
            var achievementIds = new HashSet<string>(StringComparer.Ordinal);
            if (Achievements == null) throw new InvalidOperationException("D12 图鉴 achievements 不能为 null。");
            foreach (var item in Achievements)
            {
                if (item == null) throw new InvalidOperationException("D12 图鉴 achievements 不能包含 null。");
                item.Validate();
                if (!achievementIds.Add(item.AchievementId)) throw new InvalidOperationException("D12 图鉴成就重复：" + item.AchievementId);
                var matchingRecord = false;
                foreach (var record in Records)
                    if (record.ContentId == item.ContentId && record.AchievementId == item.AchievementId && record.AchievementEventId == item.EventId)
                    {
                        matchingRecord = true;
                        break;
                    }
                if (!matchingRecord) throw new InvalidOperationException("D12 图鉴成就没有匹配的记录：" + item.AchievementId);
            }
            if (AppliedEventKeys == null) throw new InvalidOperationException("D12 图鉴 applied_event_keys 不能为 null。");
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var key in AppliedEventKeys)
                if (string.IsNullOrWhiteSpace(key) || !keys.Add(key)) throw new InvalidOperationException("D12 图鉴 applied_event_keys 非法。");
        }
    }

    [Serializable]
    public sealed class D12CollectionRecordRequest
    {
        public string ContentId;
        public D12CollectionCategory Category;
        public string FactEventId;
        public long FactSaveRevision;
        public string EvidenceRef;
        public string LocationRef;
        public int WorldDay;
        public D12CollectionAcquisitionMethod AcquisitionMethod;
        public string ActionId;
        public string ActorRef = "player";
        public string AuthorizationRef;
        public string AuthorizationFactEventId;
        public long AuthorizationSaveRevision;
    }

    [Serializable]
    public sealed class D12CollectionResult
    {
        public bool Accepted;
        public bool Duplicate;
        public bool AchievementUnlocked;
        public string Code;
        public string Message;
        public D12CollectionRecord Record;
        public D12CollectionEvidenceRecord Evidence;
        public D12CollectionAchievement Achievement;
        public FactEvent Fact;
        public FactEvent AchievementFact;

        public D12CollectionResult Clone()
        {
            return new D12CollectionResult
            {
                Accepted = Accepted,
                Duplicate = Duplicate,
                AchievementUnlocked = AchievementUnlocked,
                Code = Code,
                Message = Message,
                Record = Record == null ? null : Record.Clone(),
                Evidence = Evidence == null ? null : Evidence.Clone(),
                Achievement = Achievement == null ? null : Achievement.Clone(),
                Fact = Fact == null ? null : Fact.CloneFact(),
                AchievementFact = AchievementFact == null ? null : AchievementFact.CloneFact(),
            };
        }
    }
}
