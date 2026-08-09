using System;

namespace Whalebone.Systems.D5
{
    /// <summary>本地成就、云存档待上传队列和离线安全状态的唯一裁决者。</summary>
    public sealed class D5SteamSystem
    {
        private readonly D5SteamState _state;

        public D5SteamSystem(D5SteamState initial = null)
        {
            _state = initial == null ? new D5SteamState() : initial.Clone();
            if (string.IsNullOrEmpty(_state.AppId)) _state.AppId = "UNCONFIGURED";
        }

        public D5SteamState Snapshot => _state.Clone();

        public bool UnlockAchievement(string achievementId)
        {
            if (D5AchievementCatalog.Find(achievementId) == null) return false;
            if (_state.UnlockedAchievements.Contains(achievementId)) return false;
            _state.UnlockedAchievements.Add(achievementId);
            return true;
        }

        public bool HasAchievement(string achievementId)
        {
            return !string.IsNullOrEmpty(achievementId) && _state.UnlockedAchievements.Contains(achievementId);
        }

        public int QueueCloudUpload()
        {
            if (!_state.CloudEnabled) return _state.PendingCloudRevision;
            _state.PendingCloudRevision = Math.Max(1, _state.PendingCloudRevision + 1);
            _state.LastSync = "pending";
            return _state.PendingCloudRevision;
        }

        public bool MarkCloudSynced(int revision)
        {
            if (revision <= 0 || revision < _state.PendingCloudRevision) return false;
            _state.PendingCloudRevision = 0;
            _state.LastSync = "synced:" + revision;
            return true;
        }

        public string StatusText()
        {
            if (_state.OfflineMode) return "Steam Bridge 离线安全模式";
            if (_state.PendingCloudRevision > 0) return "云存档待同步 #" + _state.PendingCloudRevision;
            return "Steam Bridge 已连接";
        }
    }
}
